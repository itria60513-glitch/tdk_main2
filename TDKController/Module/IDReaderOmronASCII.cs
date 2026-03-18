using System;
using Communication.Interface;
using TDKLogUtility.Module;

namespace TDKController
{
    /// <summary>
    /// Implements the Omron ASCII RFID workflow.
    /// Communicates with an Omron V640/V680 series RFID reader using ASCII-mode commands.
    /// This class also serves as the base class for <see cref="IDReaderOmronHex"/>, which
    /// overrides the command format and payload handling for hexadecimal mode.
    ///
    /// Command format:
    ///   Read:  "0110" + 8-char page bitmask + CR   (ASCII mode read opcode "0110")
    ///   Write: "0210" + 8-char page bitmask + ASCII payload + CR   (ASCII mode write opcode "0210")
    ///
    /// Response format:
    ///   - Success: starts with "00" prefix followed by the data payload.
    ///   - Error: starts with a non-"00" error code.
    ///
    /// Page bitmask:
    ///   The Omron reader addresses memory using an 8-character bitmask rather than a simple
    ///   page number. BuildPageMask maps a logical page number (1-30) to the corresponding
    ///   bitmask, with different bit patterns for ASCII vs HEX modes (ASCII reads 3 blocks
    ///   per page vs HEX reads 1 block per page due to encoding differences).
    ///
    /// Overall read flow (GetCarrierID):
    ///   1. GetCarrierID delegates to ExecuteRead (base class).
    ///   2. ExecuteRead acquires busy lock, validates page via ValidateReadRequest [1..30],
    ///      connects to the reader, then invokes ReadCarrierId.
    ///   3. ReadCarrierId builds the ASCII read command, sends it, and waits for a response.
    ///   4. The response is validated by ParseCarrierIDReaderData (checks "00" prefix).
    ///   5. The data payload is extracted (strip "00" prefix) and verified as printable ASCII.
    ///   6. ExecuteRead disconnects and releases the busy lock.
    ///
    /// Overall write flow (SetCarrierID):
    ///   1. SetCarrierID delegates to ExecuteWrite (base class).
    ///   2. ExecuteWrite acquires busy lock, validates page and payload via ValidateWriteRequest
    ///      (page [1..30], payload must be 16 printable ASCII chars), connects, invokes WriteCarrierId.
    ///   3. WriteCarrierId builds the ASCII write command and sends it.
    ///   4. ExecuteWrite disconnects and releases the busy lock.
    /// </summary>
    public class IDReaderOmronASCII : CarrierIDReader
    {
        // Maximum supported RFID memory page number for the Omron ASCII reader.
        private const int MaxPage = 30;

        /// <summary>
        /// Initializes a new instance of <see cref="IDReaderOmronASCII"/> with the given
        /// configuration, communication connector, and logger.
        /// The base constructor wires up the DataReceived event on the connector.
        /// </summary>
        public IDReaderOmronASCII(CarrierIDReaderConfig config, IConnector connector, ILogUtility logger)
            : base(config, connector, logger)
        {
        }

        /// <inheritdoc />
        /// <remarks>Returns <see cref="CarrierIDReaderType.OmronASCII"/> to identify this reader type.</remarks>
        public override CarrierIDReaderType CarrierIDReaderType
        {
            get { return CarrierIDReaderType.OmronASCII; }
        }

        /// <inheritdoc />
        /// <remarks>
        /// Validates Omron responses. Called by SendCommand (base class) after each response arrives.
        ///
        /// Validation logic:
        ///   1. Trim the response to remove control characters and whitespace.
        ///   2. Check if the response starts with "00" (Omron success status prefix).
        ///      - "00" prefix: return Success (payload follows after the prefix).
        ///      - Other prefix: return CarrierIdCommandFailed (error code from reader).
        /// </remarks>
        protected override ErrorCode ParseCarrierIDReaderData(string command)
        {
            try
            {
                // Trim and check for the "00" success status prefix.
                string normalized = TrimResponse(command);
                return normalized.StartsWith("00", StringComparison.Ordinal)
                    ? ErrorCode.Success
                    : ErrorCode.CarrierIdCommandFailed;
            }
            catch (Exception ex)
            {
                _logger.WriteLog("CarrierIDReader", LogHeadType.Exception, string.Format("ParseCarrierIDReaderData: exception - {0}", ex.Message));
                throw;
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// Read flow entry point:
        ///   1. Delegates to the page-based ExecuteRead template method.
        ///   2. The base class calls ValidateReadRequest (page [1..30]) and ReadCarrierId.
        ///   3. ExecuteRead handles busy lock, validation, connection, and cleanup.
        /// </remarks>
        public override ErrorCode GetCarrierID(int page, out string carrierID)
        {
            try
            {
                return ExecuteRead(page, out carrierID);
            }
            catch (Exception ex)
            {
                _logger.WriteLog("CarrierIDReader", LogHeadType.Exception, string.Format("GetCarrierID(page): exception - {0}", ex.Message));
                throw;
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// Write flow entry point:
        ///   1. SetCarrierID delegates to ExecuteWrite (base class).
        ///   2. ExecuteWrite acquires busy lock, validates page and payload via ValidateWriteRequest
        ///      (page [1..30], payload must be 16 printable ASCII chars), connects, invokes WriteCarrierId.
        ///   3. WriteCarrierId builds the ASCII write command and sends it.
        ///   4. ExecuteWrite disconnects and releases the busy lock.
        /// </remarks>
        public override ErrorCode SetCarrierID(int page, string carrierID)
        {
            try
            {
                return ExecuteWrite(page, carrierID);
            }
            catch (Exception ex)
            {
                _logger.WriteLog("CarrierIDReader", LogHeadType.Exception, string.Format("SetCarrierID(page): exception - {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Extracts the data payload from an Omron response by stripping the leading 2-char status prefix.
        /// For a success response "00ABCDEFGH...", this returns "ABCDEFGH...".
        /// Used by ReadCarrierId (and the OmronHex subclass) to isolate the actual data.
        /// Returns empty string if the response has no payload (length <= 2).
        /// </summary>
        protected string ExtractPayload(string response)
        {
            string normalized = TrimResponse(response);
            // Strip the 2-char status prefix ("00" for success) to get the raw data payload.
            return normalized.Length <= 2 ? string.Empty : normalized.Substring(2);
        }

        /// <summary>
        /// Core read operation for the Omron ASCII reader.
        /// Called by ExecuteRead after the busy lock is acquired and the connection is established.
        ///
        /// Flow:
        ///   1. Build the ASCII read command: "0110" + 8-char page bitmask + CR.
        ///   2. Send the command and wait for a response (validated by ParseCarrierIDReaderData).
        ///   3. Extract the data payload by stripping the "00" status prefix.
        ///   4. Verify the payload contains only printable ASCII characters (0x20-0x7E).
        ///   5. Return the payload as the carrier ID string.
        /// </summary>
        protected override ErrorCode ReadCarrierId(int page, out string carrierID)
        {
            carrierID = string.Empty;

            // Step 1-2: Build and send the ASCII-mode read command.
            string response;
            ErrorCode result = SendCommand(BuildReadCommand(page), Config.TimeoutMs, out response);
            if (result != ErrorCode.Success)
            {
                return result;
            }

            // Step 3: Strip the "00" success prefix to get the raw data.
            string payload = ExtractPayload(response);

            // Step 4: Verify the payload contains only printable ASCII characters.
            if (!IsPrintableAscii(payload))
            {
                _logger.WriteLog("CarrierIDReader", LogHeadType.Error, string.Format("GetCarrierID: malformed Omron ASCII payload {0}", TrimResponse(response)));
                return ErrorCode.CarrierIdCommandFailed;
            }

            // Step 5: Return the validated ASCII payload as the carrier ID.
            carrierID = payload;
            return ErrorCode.Success;
        }

        /// <summary>
        /// Core write operation for the Omron ASCII reader.
        /// Called by ExecuteWrite after the busy lock is acquired and the connection is established.
        ///
        /// Flow:
        ///   1. Build the ASCII write command: "0210" + 8-char page bitmask + ASCII payload + CR.
        ///   2. Send the command and wait for a response (validated by ParseCarrierIDReaderData
        ///      which checks for the "00" success prefix).
        /// </summary>
        protected override ErrorCode WriteCarrierId(int page, string carrierID)
        {
            string response;
            // Build and send the ASCII-mode write command.
            return SendCommand(BuildWriteCommand(page, carrierID), Config.TimeoutMs, out response);
        }

        /// <summary>
        /// Builds the Omron ASCII read payload.
        /// Format: "0110" (ASCII read opcode) + 8-char page bitmask.
        /// OmronProtocol appends the carriage return terminator.
        /// </summary>
        protected virtual string BuildReadCommand(int page)
        {
            return string.Format("0110{0}", BuildPageMask(page));
        }

        /// <summary>
        /// Builds the Omron ASCII write payload.
        /// Format: "0210" (ASCII write opcode) + 8-char page bitmask + ASCII payload.
        /// OmronProtocol appends the carriage return terminator.
        /// </summary>
        protected virtual string BuildWriteCommand(int page, string payload)
        {
            return string.Format("0210{0}{1}", BuildPageMask(page), payload);
        }

        /// <summary>
        /// Maps a logical page number (1-30) to the 8-character bitmask used by Omron readers.
        ///
        /// The Omron V640/V680 RFID reader addresses memory blocks using a bitmask rather than
        /// a simple page number. Each logical page maps to specific bits in the 8-char mask.
        /// The mapping differs between ASCII and HEX modes because:
        ///   - ASCII mode reads 3 blocks per page (wider bitmask, e.g., 'C' = 0x0C = bits 2+3).
        ///   - HEX mode reads 1 block per page (narrower bitmask, e.g., '4' = 0x04 = bit 2 only).
        ///
        /// The bitmask is organized as 8 hex digits, with each representing 4 bits.
        /// Higher pages shift the active bit(s) into higher positions within the mask.
        ///
        /// Page grouping pattern (repeating every 4 pages):
        ///   - Page % 4 == 3 (pages 3,7,11,...): offset = page/4, set frame[6-offset].
        ///   - Page % 4 == 0 (pages 4,8,12,...): offset = (page-1)/4, set frame[6-offset].
        ///   - Page % 4 == 1 (pages 5,9,13,...): offset = (page-2)/4, set frame[6-offset].
        ///   - Page % 4 == 2 (pages 6,10,14,...): offset = (page-3)/4, may set two positions.
        ///   - Pages 1 and 2 have special mappings.
        /// </summary>
        protected string BuildPageMask(int page)
        {
            // Start with an 8-char zero mask.
            char[] frame = new[] { '0', '0', '0', '0', '0', '0', '0', '0' };

            // Determine if we're building for ASCII or HEX mode (affects bitmask width).
            bool isAscii = CarrierIDReaderType == CarrierIDReaderType.OmronASCII;
            int offset;

            switch (page)
            {
                // Page 1: Set the lowest-order digit.
                // ASCII: 'C' (0x0C = 3 blocks), HEX: '4' (0x04 = 1 block).
                case 1:
                    frame[7] = isAscii ? 'C' : '4';
                    break;

                // Page 2: Spans two digits in the mask.
                // ASCII: "18" (carries into next digit), HEX: "08".
                case 2:
                    frame[6] = isAscii ? '1' : '0';
                    frame[7] = '8';
                    break;

                // Pages 3,7,11,15,19,23,27: Group pattern — offset shifts left by page/4.
                case 3:
                case 7:
                case 11:
                case 15:
                case 19:
                case 23:
                case 27:
                    offset = page / 4;
                    frame[6 - offset] = isAscii ? '3' : '1';
                    break;

                // Pages 4,8,12,16,20,24,28: Group pattern — offset shifts left by (page-1)/4.
                case 4:
                case 8:
                case 12:
                case 16:
                case 20:
                case 24:
                case 28:
                    offset = (page - 1) / 4;
                    frame[6 - offset] = isAscii ? '6' : '2';
                    break;

                // Pages 5,9,13,17,21,25,29: Group pattern — offset shifts left by (page-2)/4.
                case 5:
                case 9:
                case 13:
                case 17:
                case 21:
                case 25:
                case 29:
                    offset = (page - 2) / 4;
                    frame[6 - offset] = isAscii ? 'C' : '4';
                    break;

                // Pages 6,10,14,18,22,26,30: Group pattern — offset shifts left by (page-3)/4.
                // In ASCII mode these pages span two bitmask positions (except page 30).
                case 6:
                case 10:
                case 14:
                case 18:
                case 22:
                case 26:
                case 30:
                    offset = (page - 3) / 4;
                    if (isAscii)
                    {
                        // ASCII mode: set carry bit in the higher digit (except for page 30 boundary).
                        if (page < 30)
                        {
                            frame[5 - offset] = '1';
                        }

                        frame[6 - offset] = '8';
                    }
                    else
                    {
                        // HEX mode: single digit, no carry needed.
                        frame[6 - offset] = '8';
                    }

                    break;

                // Default fallback: treat as page 1.
                default:
                    frame[7] = isAscii ? 'C' : '4';
                    break;
            }

            return new string(frame);
        }

        /// <summary>
        /// Validates that the supplied page number is within the allowed range [1, maxPage].
        /// Shared by both ASCII and HEX subclasses (each passes its own MaxPage constant).
        /// </summary>
        protected ErrorCode ValidatePage(int page, int maxPage)
        {
            return page >= 1 && page <= maxPage
                ? ErrorCode.Success
                : ErrorCode.CarrierIdInvalidPage;
        }

        /// <summary>
        /// Pre-read validation for ASCII mode: checks page range [1, 30].
        /// Called by ExecuteRead before the connection is established.
        /// Virtual to allow OmronHex to override with its own MaxPage.
        /// </summary>
        protected override ErrorCode ValidateReadRequest(int page)
        {
            return ValidatePage(page, MaxPage);
        }

        /// <summary>
        /// Pre-write validation for ASCII mode: delegates to ValidateAsciiWrite.
        /// Virtual to allow OmronHex to override with hex-specific validation.
        /// Called by ExecuteWrite before the connection is established.
        /// </summary>
        protected override ErrorCode ValidateWriteRequest(int page, string carrierID)
        {
            return ValidateAsciiWrite(page, carrierID);
        }

        /// <summary>
        /// Validates both the page number and the carrier ID payload for an ASCII write operation.
        ///
        /// Validation steps:
        ///   1. Ensure the supplied page is within [1, 30].
        ///   2. Ensure the carrier ID is exactly 16 printable ASCII characters.
        ///      (Each Omron memory page holds 16 bytes of data.)
        /// </summary>
        protected ErrorCode ValidateAsciiWrite(int page, string carrierID)
        {
            // Step 1: Validate the page number range.
            ErrorCode pageResult = ValidatePage(page, MaxPage);
            if (pageResult != ErrorCode.Success)
            {
                return pageResult;
            }

            // Step 2: Validate the carrier ID is a 16-char printable ASCII string.
            if (string.IsNullOrEmpty(carrierID) || carrierID.Length != 16 || !IsPrintableAscii(carrierID))
            {
                return ErrorCode.CarrierIdInvalidParameter;
            }

            return ErrorCode.Success;
        }
    }
}