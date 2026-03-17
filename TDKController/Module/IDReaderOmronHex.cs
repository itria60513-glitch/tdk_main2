using System;
using System.Text;
using Communication.Interface;
using TDKLogUtility.Module;

namespace TDKController
{
    /// <summary>
    /// Implements the Omron HEX RFID workflow.
    /// Inherits from <see cref="IDReaderOmronASCII"/> and overrides the read/write logic
    /// to work with raw hexadecimal data instead of printable ASCII characters.
    ///
    /// Overall read flow:
    ///   1. GetCarrierID is called by the host application.
    ///   2. ExecuteRead (base class) acquires the busy lock, validates the page via
    ///      ValidateReadRequest, connects to the reader, then invokes ReadCarrierId.
    ///   3. ReadCarrierId builds a HEX-mode read command ("0100" + page mask),
    ///      sends it to the reader, and waits for a response.
    ///   4. The response payload (after stripping the "00" status prefix) is validated
    ///      as an even-length hex string, then converted from hex bytes to an ASCII string.
    ///   5. ExecuteRead disconnects from the reader and releases the busy lock.
    ///
    /// Overall write flow:
    ///   1. SetCarrierID is called with the carrier ID to write.
    ///   2. ExecuteWrite (base class) acquires the busy lock, validates the page and
    ///      payload via ValidateWriteRequest, connects to the reader, then invokes WriteCarrierId.
    ///   3. WriteCarrierId builds a HEX-mode write command ("0200" + page mask + hex payload),
    ///      sends it to the reader, and waits for a response.
    ///   4. ExecuteWrite disconnects from the reader and releases the busy lock.
    /// </summary>
    public class IDReaderOmronHex : IDReaderOmronASCII
    {
        // Maximum supported RFID memory page number for the Omron HEX reader.
        private const int MaxPage = 30;

        /// <summary>
        /// Initializes a new instance of <see cref="IDReaderOmronHex"/> with the given
        /// configuration, communication connector, and logger.
        /// The base constructor wires up the DataReceived event on the connector.
        /// </summary>
        public IDReaderOmronHex(CarrierIDReaderConfig config, IConnector connector, ILogUtility logger)
            : base(config, connector, logger)
        {
        }

        /// <inheritdoc />
        /// <remarks>Returns <see cref="CarrierIDReaderType.OmronHex"/> to identify this reader type.</remarks>
        public override CarrierIDReaderType CarrierIDReaderType
        {
            get { return CarrierIDReaderType.OmronHex; }
        }

        /// <summary>
        /// Core read operation for the Omron HEX reader.
        /// Called by ExecuteRead after the busy lock is acquired and the connection is established.
        ///
        /// Flow:
        ///   1. Build the read command string: "0100" + 8-char page bitmask + CR.
        ///   2. Send the command to the reader and wait up to Config.TimeoutMs for a response.
        ///   3. If SendCommand fails (timeout, parse error, etc.), return the error immediately.
        ///   4. Extract the data payload by stripping the leading "00" status prefix.
        ///   5. Validate that the payload is a well-formed hex string (even length, all hex chars).
        ///   6. Convert the hex payload to an ASCII string (each 2 hex chars = 1 ASCII byte).
        ///   7. Return the decoded carrier ID on success.
        /// </summary>
        protected override ErrorCode ReadCarrierId(int page, out string carrierID)
        {
            carrierID = string.Empty;

            // Step 1-2: Build and send the HEX-mode read command to the reader.
            string response;
            ErrorCode result = SendCommand(BuildReadCommand(page), Config.TimeoutMs, out response);
            if (result != ErrorCode.Success)
            {
                // Step 3: Return error if the command failed (timeout, connection issue, etc.).
                return result;
            }

            // Step 4: Strip the "00" success-status prefix from the raw response.
            string payload = ExtractPayload(response);

            // Step 5: Verify the payload contains only hex characters and has even length.
            if (!IsValidHexPayload(payload))
            {
                _logger.WriteLog("CarrierIDReader", LogHeadType.Error, string.Format("GetCarrierID: malformed Omron HEX payload {0}", TrimResponse(response)));
                return ErrorCode.CarrierIdCommandFailed;
            }

            // Step 6-7: Convert hex-encoded bytes to a readable ASCII carrier ID string.
            carrierID = HexToAscii(payload);
            return ErrorCode.Success;
        }

        /// <summary>
        /// Core write operation for the Omron HEX reader.
        /// Called by ExecuteWrite after the busy lock is acquired and the connection is established.
        ///
        /// Flow:
        ///   1. Build the write command string: "0200" + 8-char page bitmask + uppercase hex payload + CR.
        ///   2. Send the command to the reader and wait up to Config.TimeoutMs for a response.
        ///   3. The response is parsed by ParseCarrierIDReaderData (inherited from OmronASCII)
        ///      which checks for a "00" success prefix.
        /// </summary>
        protected override ErrorCode WriteCarrierId(int page, string carrierID)
        {
            string response;
            // Build and send the HEX-mode write command with the carrier ID payload.
            return SendCommand(BuildWriteCommand(page, carrierID), Config.TimeoutMs, out response);
        }

        /// <summary>
        /// Builds the Omron HEX read payload.
        /// Format: "0100" (HEX read opcode) + 8-char page bitmask.
        /// OmronProtocol appends the carriage return terminator.
        /// </summary>
        protected override string BuildReadCommand(int page)
        {
            return string.Format("0100{0}", BuildPageMask(page));
        }

        /// <summary>
        /// Builds the Omron HEX write payload.
        /// Format: "0200" (HEX write opcode) + 8-char page bitmask + uppercase hex data.
        /// OmronProtocol appends the carriage return terminator.
        /// </summary>
        protected override string BuildWriteCommand(int page, string payload)
        {
            return string.Format("0200{0}{1}", BuildPageMask(page), payload.ToUpperInvariant());
        }

        /// <summary>
        /// Validates that the supplied page number is within the allowed range [1, 30].
        /// Called by ExecuteRead before the connection is established.
        /// </summary>
        protected override ErrorCode ValidateReadRequest(int page)
        {
            return ValidatePage(page, MaxPage);
        }

        /// <summary>
        /// Validates the page number and the carrier ID payload before a write operation.
        ///
        /// Validation steps:
        ///   1. Ensure the supplied page is within [1, 30].
        ///   2. Ensure the carrier ID is exactly 16 hex characters (representing 8 bytes of data).
        /// </summary>
        protected override ErrorCode ValidateWriteRequest(int page, string carrierID)
        {
            // Step 1: Validate the page number range.
            ErrorCode pageResult = ValidatePage(page, MaxPage);
            if (pageResult != ErrorCode.Success)
            {
                return pageResult;
            }

            // Step 2: Validate the carrier ID is a 16-char hex string.
            if (!IsValidHexWritePayload(carrierID))
            {
                return ErrorCode.CarrierIdInvalidParameter;
            }

            return ErrorCode.Success;
        }

        /// <summary>
        /// Checks whether the payload is a valid hex string with even length.
        /// An even length is required because each pair of hex characters represents one byte.
        /// </summary>
        private static bool IsValidHexPayload(string payload)
        {
            return IsHexString(payload) && payload.Length % 2 == 0;
        }

        /// <summary>
        /// Checks whether the carrier ID is a valid hex write payload:
        /// non-empty, exactly 16 hex characters (8 bytes), and all valid hex digits.
        /// </summary>
        private static bool IsValidHexWritePayload(string carrierID)
        {
            return !string.IsNullOrEmpty(carrierID) && carrierID.Length == 16 && IsHexString(carrierID);
        }

        /// <summary>
        /// Converts a hex-encoded string to its ASCII representation.
        ///
        /// Flow:
        ///   1. Allocate a byte array with half the length of the hex string.
        ///   2. Parse each pair of hex characters into a single byte (e.g., "41" -> 0x41 -> 'A').
        ///   3. Decode the byte array into an ASCII string.
        /// </summary>
        private static string HexToAscii(string payload)
        {
            // Step 1: Each 2 hex chars encode 1 byte, so output length = input length / 2.
            byte[] bytes = new byte[payload.Length / 2];

            // Step 2: Convert each hex pair to a byte value.
            for (int index = 0; index < bytes.Length; index++)
            {
                bytes[index] = Convert.ToByte(payload.Substring(index * 2, 2), 16);
            }

            // Step 3: Decode the raw bytes into a readable ASCII string.
            return Encoding.ASCII.GetString(bytes);
        }
    }
}