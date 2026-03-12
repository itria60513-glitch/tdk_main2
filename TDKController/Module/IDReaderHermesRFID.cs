using System;
using Communication.Interface;
using TDKLogUtility.Module;

namespace TDKController
{
    /// <summary>
    /// Implements the Hermes RFID workflow.
    /// Communicates with a Hermes-protocol RFID reader using a custom framed protocol
    /// with start marker ('S'), length prefix, message body, CR delimiter, and dual checksums (XOR + ADD).
    ///
    /// Frame format (outgoing command):
    ///   S [LenHex2] [Message] CR [XOR2] [ADD2]
    ///   - 'S':        Start-of-frame marker.
    ///   - LenHex2:    2-char hex representation of the message body length.
    ///   - Message:    Protocol command (e.g., "X005" for read page 5, "W005..." for write page 5).
    ///   - CR:         Carriage return delimiter (0x0D).
    ///   - XOR2+ADD2:  4-char checksum — 2-char XOR checksum followed by 2-char additive checksum,
    ///                 computed over everything from 'S' through CR (inclusive).
    ///
    /// Frame format (incoming response):
    ///   S [LenHex2] [Message] CR [XOR2] [ADD2]
    ///   - Message[0]: Response type character — 'x' (read), 'w' (write), 'v' (version).
    ///   - Message[1]: Always '0'.
    ///   - For read responses: Message[2..3] = page number, Message[4..] = data payload.
    ///
    /// Overall read flow (GetCarrierID):
    ///   1. GetCarrierID delegates to ExecuteRead (base class).
    ///   2. ExecuteRead acquires busy lock, validates page via ValidatePage [1..17],
    ///      connects, then invokes TryReadCarrierId.
    ///   3. TryReadCarrierId builds a read command ("X0" + 2-digit page), wraps it in a Hermes frame,
    ///      and sends it. The response is parsed by ParseCarrierIDReaderData which validates
    ///      the frame structure, checksums, and response type.
    ///   4. The carrier ID is extracted from the parsed frame's message field (after skipping
    ///      the response type, '0', and 2-digit page prefix).
    ///   5. ExecuteRead disconnects and releases the busy lock.
    ///
    /// Overall write flow (SetCarrierID):
    ///   1. SetCarrierID delegates to ExecuteWrite (base class).
    ///   2. ExecuteWrite acquires busy lock, validates page and payload via ValidateWritePayload
    ///      (page [1..17], payload must be exactly 16 hex characters), connects, then invokes WriteCarrierId.
    ///   3. WriteCarrierId builds a write command ("W0" + 2-digit page + uppercase hex data),
    ///      wraps it in a Hermes frame, and sends it.
    ///   4. ExecuteWrite disconnects and releases the busy lock.
    /// </summary>
    public class IDReaderHermesRFID : CarrierIDReader
    {
        // Maximum supported RFID memory page number for the Hermes reader.
        private const int MaxPage = 17;

        /// <summary>
        /// Caches the last successfully parsed Hermes frame from ParseCarrierIDReaderData.
        /// Used by TryReadCarrierId to extract the data payload without re-parsing the response.
        /// Reset to null at the start of each ParseCarrierIDReaderData call.
        /// </summary>
        private HermesFrame? _lastParsedFrame;

        /// <summary>
        /// Initializes a new instance of <see cref="IDReaderHermesRFID"/> with the given
        /// configuration, communication connector, and logger.
        /// The base constructor wires up the DataReceived event on the connector.
        /// </summary>
        public IDReaderHermesRFID(CarrierIDReaderConfig config, IConnector connector, ILogUtility logger)
            : base(config, connector, logger)
        {
        }

        /// <inheritdoc />
        /// <remarks>Returns <see cref="CarrierIDReaderType.HermesRFID"/> to identify this reader type.</remarks>
        public override CarrierIDReaderType CarrierIDReaderType
        {
            get { return CarrierIDReaderType.HermesRFID; }
        }

        /// <inheritdoc />
        /// <remarks>
        /// Validates and parses the raw response from the Hermes RFID reader.
        /// Called by SendCommand (base class) after each response arrives.
        ///
        /// Flow:
        ///   1. Reset _lastParsedFrame to null (clear any cached data from previous calls).
        ///   2. Attempt to parse the raw response into a HermesFrame via TryParseFrame:
        ///      a. Validate minimum length, start marker ('S'), frame length consistency.
        ///      b. Extract message length from hex prefix, verify overall frame length.
        ///      c. Check for CR delimiter at the expected position.
        ///      d. Extract and validate the message body (must have '0' at position [1]).
        ///      e. Compute and compare XOR + ADD checksums against the frame's trailing 4 chars.
        ///   3. Check that the response type is supported ('x' = read, 'w' = write, 'v' = version).
        ///   4. Cache the parsed frame in _lastParsedFrame for downstream use by TryReadCarrierId.
        ///   5. Return Success if all checks pass, or CarrierIdCommandFailed / CarrierIdChecksumError.
        /// </remarks>
        public override ErrorCode ParseCarrierIDReaderData(string command)
        {
            try
            {
                // Step 1: Clear any previously cached frame data.
                _lastParsedFrame = null;

                // Step 2: Parse the raw response into a structured HermesFrame.
                HermesFrame frame;
                ErrorCode parseResult = TryParseFrame(command, out frame);
                if (parseResult != ErrorCode.Success)
                {
                    return parseResult;
                }

                // Step 3: Verify the response type is one we recognize (read/write/version).
                if (!IsSupportedResponse(frame.Message))
                {
                    return ErrorCode.CarrierIdCommandFailed;
                }

                // Step 4: Cache the parsed frame for TryReadCarrierId to use.
                _lastParsedFrame = frame;
                return ErrorCode.Success;
            }
            catch (FormatException)
            {
                // Hex parsing failure in TryParseFrame (e.g., invalid length prefix).
                return ErrorCode.CarrierIdCommandFailed;
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
        ///   1. Delegates to ExecuteRead with ValidatePage (page [1..17]) and TryReadCarrierId.
        ///   2. ExecuteRead handles busy lock, validation, connection, and cleanup.
        /// </remarks>
        public override ErrorCode GetCarrierID(int page, out string carrierID)
        {
            try
            {
                return ExecuteRead(page, ValidatePage, TryReadCarrierId, out carrierID);
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
        ///   1. Delegates to ExecuteWrite with ValidateWritePayload and WriteCarrierId.
        ///   2. ValidateWritePayload checks page [1..17] and that the carrier ID is a 16-char hex string.
        ///   3. ExecuteWrite handles busy lock, validation, connection, and cleanup.
        /// </remarks>
        public override ErrorCode SetCarrierID(int page, string carrierID)
        {
            try
            {
                return ExecuteWrite(page, carrierID, ValidateWritePayload, WriteCarrierId);
            }
            catch (Exception ex)
            {
                _logger.WriteLog("CarrierIDReader", LogHeadType.Exception, string.Format("SetCarrierID(page): exception - {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Validates that the current page number is within the allowed range [1, 17].
        /// </summary>
        private ErrorCode ValidatePage(int page)
        {
            return page >= 1 && page <= MaxPage
                ? ErrorCode.Success
                : ErrorCode.CarrierIdInvalidPage;
        }

        /// <summary>
        /// Validates both the page number and the carrier ID payload for a write operation.
        ///
        /// Validation steps:
        ///   1. Ensure the supplied page is within [1, 17].
        ///   2. Ensure the carrier ID is exactly 16 hex characters (representing 8 bytes of RFID data).
        /// </summary>
        private ErrorCode ValidateWritePayload(int page, string carrierID)
        {
            // Step 1: Validate the page number range.
            ErrorCode pageResult = ValidatePage(page);
            if (pageResult != ErrorCode.Success)
            {
                return pageResult;
            }

            // Step 2: Validate the carrier ID is a 16-char hex string.
            if (string.IsNullOrEmpty(carrierID) || carrierID.Length != 16 || !IsHexString(carrierID))
            {
                return ErrorCode.CarrierIdInvalidParameter;
            }

            return ErrorCode.Success;
        }

        /// <summary>
        /// Core read operation for the Hermes RFID reader.
        /// Called by ExecuteRead after the busy lock is acquired and the connection is established.
        ///
        /// Flow:
        ///   1. Build the read command frame: "X0" + 2-digit page, wrapped in Hermes frame format.
        ///   2. Send the command and wait for a response (parsed by ParseCarrierIDReaderData).
        ///   3. Verify that _lastParsedFrame contains a valid read response (message starts with 'x').
        ///   4. Extract the data payload from the message:
        ///      - message[0] = 'x' (response type), message[1] = '0',
        ///      - message[2..3] = page number, message[4..] = carrier ID data.
        ///   5. Return the extracted carrier ID, or CarrierIdCommandFailed if empty.
        /// </summary>
        private ErrorCode TryReadCarrierId(int page, out string carrierID)
        {
            carrierID = string.Empty;

            // Step 1-2: Build and send the Hermes read command.
            string response;
            ErrorCode result = SendCommand(BuildReadCommand(page), Config.TimeoutMs, out response);
            if (result != ErrorCode.Success)
            {
                return result;
            }

            // Step 3: Verify the cached parsed frame is a read response ('x' type).
            if (!_lastParsedFrame.HasValue || !IsReadResponse(_lastParsedFrame.Value.Message))
            {
                return ErrorCode.CarrierIdCommandFailed;
            }

            // Step 4: Extract the carrier ID from the message.
            // Message format: "x0" + [2-digit page] + [carrier ID data]
            // Skip the first 2 chars ("x0"), then skip the 2-digit page to get the actual data.
            string info = _lastParsedFrame.Value.Message.Substring(2);
            carrierID = info.Length > 2 ? info.Substring(2) : string.Empty;

            // Step 5: Return error if the data payload is empty.
            return string.IsNullOrEmpty(carrierID) ? ErrorCode.CarrierIdCommandFailed : ErrorCode.Success;
        }

        /// <summary>
        /// Core write operation for the Hermes RFID reader.
        /// Called by ExecuteWrite after the busy lock is acquired and the connection is established.
        ///
        /// Flow:
        ///   1. Build the write command frame: "W0" + 2-digit page + uppercase hex carrier ID,
        ///      wrapped in Hermes frame format.
        ///   2. Send the command and wait for a response (parsed by ParseCarrierIDReaderData
        ///      which validates the 'w' response frame and checksums).
        /// </summary>
        private ErrorCode WriteCarrierId(int page, string carrierID)
        {
            string response;
            // Build and send the Hermes write command frame.
            return SendCommand(BuildWriteCommand(page, carrierID), Config.TimeoutMs, out response);
        }

        /// <summary>
        /// Builds the read command message: "X0" + 2-digit zero-padded page number.
        /// The 'X' prefix identifies this as a read request in the Hermes protocol.
        /// The message is then wrapped in a Hermes frame by PrepareCommand.
        /// </summary>
        private string BuildReadCommand(int page)
        {
            return PrepareCommand(string.Format("X0{0:D2}", page));
        }

        /// <summary>
        /// Builds the write command message: "W0" + 2-digit zero-padded page number + uppercase hex data.
        /// The 'W' prefix identifies this as a write request in the Hermes protocol.
        /// The message is then wrapped in a Hermes frame by PrepareCommand.
        /// </summary>
        private string BuildWriteCommand(int page, string carrierID)
        {
            return PrepareCommand(string.Format("W0{0:D2}{1}", page, carrierID.ToUpperInvariant()));
        }

        /// <summary>
        /// Wraps a message body into a complete Hermes protocol frame.
        ///
        /// Frame construction flow:
        ///   1. Compute the message length as a 2-char hex string (e.g., 4 -> "04").
        ///   2. Build the frame content: "S" + length + message + CR.
        ///   3. Compute dual checksums (XOR and ADD) over the entire frame content.
        ///   4. Append the 4-char checksum to complete the frame.
        ///
        /// Example: message "X005"
        ///   -> length = "04"
        ///   -> frame content = "S04X005\r"
        ///   -> checksums = ComputeChecksums("S04X005\r") = "XXYY"
        ///   -> final frame = "S04X005\rXXYY"
        /// </summary>
        private string PrepareCommand(string message)
        {
            // Step 1: Encode the message length as 2-char uppercase hex.
            string length = message.Length.ToString("X2");

            // Step 2: Build the frame content (everything before the checksum).
            string frameWithoutChecksums = string.Concat("S", length, message, "\r");

            // Step 3-4: Compute and append the dual checksums.
            return frameWithoutChecksums + ComputeChecksums(frameWithoutChecksums);
        }

        /// <summary>
        /// Parses a raw response string into a structured HermesFrame.
        /// Performs comprehensive validation of the frame format and integrity.
        ///
        /// Parsing flow:
        ///   1. Trim and validate minimum length (>= 10 chars) and start marker ('S').
        ///   2. Extract the 2-char hex message length from positions [1..2].
        ///   3. Compute expected total frame length: messageLength + 8
        ///      (1 for 'S' + 2 for length + messageLength + 1 for CR + 4 for checksums).
        ///   4. Verify the actual string length matches the expected frame length.
        ///   5. Verify the CR delimiter is at the expected position (frameLength - 5).
        ///   6. Extract the message body and verify format (message[1] must be '0').
        ///   7. Extract the trailing 4-char checksum and compare against computed expected value.
        ///   8. If all checks pass, return the parsed HermesFrame with the message body.
        /// </summary>
        private ErrorCode TryParseFrame(string response, out HermesFrame frame)
        {
            frame = default(HermesFrame);

            // Step 1: Basic validation — must be at least 10 chars and start with 'S'.
            string normalized = TrimResponse(response);
            if (string.IsNullOrEmpty(normalized) || normalized.Length < 10 || normalized[0] != 'S')
            {
                return ErrorCode.CarrierIdCommandFailed;
            }

            // Step 2: Parse the 2-char hex message length (bytes 1-2 after 'S').
            int messageLength = Convert.ToInt32(normalized.Substring(1, 2), 16);

            // Step 3: Compute expected total frame length.
            // Layout: S(1) + LenHex(2) + Message(messageLength) + CR(1) + Checksum(4) = messageLength + 8
            int frameLength = messageLength + 8;

            // Step 4: Verify actual length matches expected.
            if (normalized.Length != frameLength)
            {
                return ErrorCode.CarrierIdCommandFailed;
            }

            // Step 5: Verify CR delimiter at the expected position (just before the 4-char checksum).
            int endIndex = frameLength - 5;
            if (normalized[endIndex] != '\r')
            {
                return ErrorCode.CarrierIdCommandFailed;
            }

            // Step 6: Extract message body and validate format.
            // Message starts at index 3 (after "S" + 2-char length) with length = messageLength.
            // message[1] must be '0' per Hermes protocol convention.
            string message = normalized.Substring(3, messageLength);
            if (message.Length < 2 || message[1] != '0')
            {
                return ErrorCode.CarrierIdCommandFailed;
            }

            // Step 7: Verify frame integrity via dual checksums (XOR + ADD).
            // The checksum is computed over everything from 'S' through CR (exclusive of the checksum itself).
            string checksum = normalized.Substring(frameLength - 4, 4);
            string expected = ComputeChecksums(normalized.Substring(0, frameLength - 4));
            if (!string.Equals(checksum, expected, StringComparison.OrdinalIgnoreCase))
            {
                return ErrorCode.CarrierIdChecksumError;
            }

            // Step 8: All checks passed; wrap the message in a HermesFrame struct.
            frame = new HermesFrame(message);
            return ErrorCode.Success;
        }

        /// <summary>
        /// Checks whether the response message type is one of the supported Hermes responses.
        /// Supported types: 'x' (read response), 'w' (write response), 'v' (version response).
        /// The response type is the first character of the message body.
        /// </summary>
        private static bool IsSupportedResponse(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return false;
            }

            char responseType = message[0];
            return responseType == 'x' || responseType == 'w' || responseType == 'v';
        }

        /// <summary>
        /// Checks whether the message is a read response (first character is 'x').
        /// Used by TryReadCarrierId to confirm the response type before extracting data.
        /// </summary>
        private static bool IsReadResponse(string message)
        {
            return !string.IsNullOrEmpty(message) && message[0] == 'x';
        }

        /// <summary>
        /// Computes the dual checksum used in the Hermes protocol frame.
        ///
        /// Algorithm:
        ///   1. XOR checksum: XOR all character values together, masked to 8 bits.
        ///   2. ADD checksum: Sum all character values, masked to 8 bits (modulo 256).
        ///   3. Format both as 2-char uppercase hex, concatenated: XOR2 + ADD2 (4 chars total).
        ///
        /// This provides both a parity-style check (XOR) and an overflow-style check (ADD).
        /// </summary>
        private static string ComputeChecksums(string value)
        {
            int xorValue = 0;
            int addValue = 0;
            for (int index = 0; index < value.Length; index++)
            {
                // Accumulate XOR of all character code points.
                xorValue ^= value[index];
                // Accumulate sum of all character code points, masked to 8 bits.
                addValue = (addValue + value[index]) & 0xFF;
            }

            // Format as 4-char hex: 2-char XOR checksum + 2-char ADD checksum.
            return string.Format("{0:X2}{1:X2}", xorValue & 0xFF, addValue & 0xFF);
        }

        /// <summary>
        /// Lightweight struct that holds the parsed message body from a Hermes protocol frame.
        /// Used to pass parsed data from ParseCarrierIDReaderData to TryReadCarrierId
        /// via the _lastParsedFrame field.
        /// </summary>
        private struct HermesFrame
        {
            public HermesFrame(string message)
            {
                Message = message;
            }

            /// <summary>The raw message body extracted from the Hermes frame (between length prefix and CR).</summary>
            public string Message { get; }
        }
    }
}