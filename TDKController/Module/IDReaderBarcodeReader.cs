using System;
using Communication.Interface;
using TDKLogUtility.Module;

namespace TDKController
{
    /// <summary>
    /// Implements the BL600 barcode reader workflow.
    /// This reader communicates with a Keyence BL-600 series barcode scanner using
    /// simple text commands over a serial/TCP connection.
    ///
    /// Overall read flow (GetCarrierID):
    ///   1. GetCarrierID delegates to ExecuteRead (base class), which acquires the busy lock,
    ///      connects to the reader, then invokes ReadCarrierId.
    ///   2. ReadCarrierId sends MOTORON to activate the scanner motor and waits for "OK".
    ///   3. TryReadCarrierId sends the LON (read trigger) command up to BarcodeReaderMaxRetryCount times:
    ///      a. Each attempt sends LON and waits for a barcode string or status response.
    ///      b. The response is classified to existing error codes: printable ASCII = Success,
    ///         "NG" = CarrierIdReadFailed, "ERROR" = CarrierIdCommandFailed.
    ///      c. On timeout, LOFF is sent to cancel the trigger, and the read aborts.
    ///   4. After reading (success or failure), MOTOROFF is sent to deactivate the motor.
    ///   5. ExecuteRead disconnects from the reader and releases the busy lock.
    ///
    /// Note: This reader only supports reading (GetCarrierID). Writing (SetCarrierID)
    /// falls back to the base class default, which returns CarrierIdError.
    /// </summary>
    public class IDReaderBarcodeReader : CarrierIDReader
    {
        // Command timeout values in milliseconds.
        private const int MotorOnTimeoutMs = 10000;   // MOTORON may take longer due to motor spin-up.
        private const int MotorOffTimeoutMs = 3000;    // MOTOROFF is faster since the motor is already running.
        private const int ReadTimeoutMs = 5000;        // LON read trigger timeout.

        // BL600 command payloads. BarcodeProtocol appends the frame terminator.
        private const string CommandMotorOn = "MOTORON";    // Activates the scanner motor.
        private const string CommandMotorOff = "MOTOROFF";  // Deactivates the scanner motor.
        private const string CommandRead = "LON";           // Triggers a barcode read.
        private const string CommandStop = "LOFF";          // Cancels an in-progress read trigger.

        /// <summary>
        /// Initializes a new instance of <see cref="IDReaderBarcodeReader"/> with the given
        /// configuration, communication connector, and logger.
        /// The base constructor wires up the DataReceived event on the connector.
        /// </summary>
        public IDReaderBarcodeReader(CarrierIDReaderConfig config, IConnector connector, ILogUtility logger)
            : base(config, connector, logger)
        {
        }

        /// <inheritdoc />
        /// <remarks>Returns <see cref="CarrierIDReaderType.BarcodeReader"/> to identify this reader type.</remarks>
        public override CarrierIDReaderType CarrierIDReaderType
        {
            get { return CarrierIDReaderType.BarcodeReader; }
        }

        /// <inheritdoc />
        /// <remarks>
        /// Validates BL600 responses. Called by SendCommand (base class) after each response arrives.
        ///
        /// Classification logic:
        ///   1. Trim the response and check for empty — return CarrierIdCommandFailed.
        ///   2. "OK" or "NG" — valid acknowledgements, return Success (further classification
        ///      is handled by ClassifyReadState during the read loop using existing error codes).
        ///   3. "ERROR" — hardware error response, return CarrierIdCommandFailed.
        ///   4. Any other printable ASCII string — treated as valid barcode data, return Success.
        ///   5. Non-printable data — return CarrierIdCommandFailed.
        /// </remarks>
        protected override ErrorCode ParseCarrierIDReaderData(string command)
        {
            try
            {
                // Step 1: Normalize the raw response by removing control characters and whitespace.
                string normalized = TrimResponse(command);
                if (string.IsNullOrEmpty(normalized))
                {
                    return ErrorCode.CarrierIdCommandFailed;
                }

                // Step 2: "OK" = motor command acknowledgement, "NG" = unreadable barcode.
                // Both are valid protocol responses.
                if (string.Equals(normalized, "OK", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(normalized, "NG", StringComparison.OrdinalIgnoreCase))
                {
                    return ErrorCode.Success;
                }

                // Step 3: "ERROR" indicates a hardware-level failure.
                if (string.Equals(normalized, "ERROR", StringComparison.OrdinalIgnoreCase))
                {
                    return ErrorCode.CarrierIdCommandFailed;
                }

                // Step 4-5: Any printable ASCII text is treated as barcode data.
                return IsPrintableAscii(normalized) ? ErrorCode.Success : ErrorCode.CarrierIdCommandFailed;
            }
            catch (Exception ex)
            {
                _logger.WriteLog("CarrierIDReader", LogHeadType.Exception, string.Format("ParseCarrierIDReaderData: exception - {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Sends the BL600 MOTORON command and waits for the OK acknowledgement.
        ///
        /// Flow:
        ///   1. Send the MOTORON payload via SendAckCommand.
        ///   2. Wait up to 10 seconds for the "OK" acknowledgement.
        ///   3. Return Success if "OK" received, or CarrierIdMotorOnFailed otherwise.
        /// </summary>
        private ErrorCode MotorON()
        {
            try
            {
                return SendAckCommand(CommandMotorOn, MotorOnTimeoutMs, ErrorCode.CarrierIdMotorOnFailed);
            }
            catch (Exception ex)
            {
                _logger.WriteLog("CarrierIDReader", LogHeadType.Exception, string.Format("MotorON: exception - {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Sends the BL600 MOTOROFF command and waits for the OK acknowledgement.
        ///
        /// Flow:
        ///   1. Send the MOTOROFF payload via SendAckCommand.
        ///   2. Wait up to 3 seconds for the "OK" acknowledgement.
        ///   3. Return Success if "OK" received, or CarrierIdMotorOffFailed otherwise.
        /// </summary>
        private ErrorCode MotorOFF()
        {
            try
            {
                return SendAckCommand(CommandMotorOff, MotorOffTimeoutMs, ErrorCode.CarrierIdMotorOffFailed);
            }
            catch (Exception ex)
            {
                _logger.WriteLog("CarrierIDReader", LogHeadType.Exception, string.Format("MotorOFF: exception - {0}", ex.Message));
                throw;
            }
        }

        /// <inheritdoc />
        public override ErrorCode GetCarrierID(int page, out string carrierID)
        {
            try
            {
                return ExecuteRead(out carrierID);
            }
            catch (Exception ex)
            {
                _logger.WriteLog("CarrierIDReader", LogHeadType.Exception, string.Format("GetCarrierID(page): exception - {0}", ex.Message));
                throw;
            }
        }

        /// <inheritdoc />
        public override ErrorCode SetCarrierID(int page, string carrierID)
        {
            try
            {
                return base.SetCarrierID(page, carrierID);
            }
            catch (Exception ex)
            {
                _logger.WriteLog("CarrierIDReader", LogHeadType.Exception, string.Format("SetCarrierID(page): exception - {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Core barcode read operation invoked by ExecuteRead after connection is established.
        ///
        /// Flow:
        ///   1. Send MOTORON to activate the barcode scanner motor; abort if it fails.
        ///   2. Call TryReadCarrierId to perform the read-with-retry loop.
        ///   3. Send MOTOROFF to deactivate the motor regardless of read result.
        ///   4. If MOTOROFF fails but the read succeeded, return CarrierIdMotorOffFailed.
        ///      If both read and MOTOROFF failed, return the original read error (more relevant).
        /// </summary>
        protected override ErrorCode ReadCarrierId(out string carrierID)
        {
            carrierID = string.Empty;

            // Step 1: Activate the scanner motor. Must succeed before attempting any reads.
            ErrorCode motorOnResult = MotorON();
            if (motorOnResult != ErrorCode.Success)
            {
                _logger.WriteLog("CarrierIDReader", LogHeadType.Error, "GetCarrierID: MOTORON failed");
                return motorOnResult;
            }

            // Step 2: Perform the read-with-retry loop (up to Config.BarcodeReaderMaxRetryCount attempts).
            ErrorCode result = TryReadCarrierId(out carrierID);

            // Step 3: Always attempt to turn off the motor, even if the read failed.
            ErrorCode motorOffResult = MotorOFF();
            if (motorOffResult != ErrorCode.Success)
            {
                _logger.WriteLog("CarrierIDReader", LogHeadType.Error, "GetCarrierID: MOTOROFF failed");
                // Step 4: Prioritize the original read error over the motor-off failure.
                return result == ErrorCode.Success ? ErrorCode.CarrierIdMotorOffFailed : result;
            }

            return result;
        }

        /// <summary>
        /// Sends a command and checks that the response is "OK".
        /// Used for MOTORON and MOTOROFF commands which expect a simple acknowledgement.
        ///
        /// Flow:
        ///   1. Send the command via SendCommand (base class) and wait for a response.
        ///   2. If SendCommand fails (timeout, parse error), return the specified failureCode.
        ///   3. Check if the trimmed response equals "OK".
        ///   4. Return Success if "OK", otherwise return the failureCode.
        /// </summary>
        private ErrorCode SendAckCommand(string command, int timeoutMs, ErrorCode failureCode)
        {
            string response;
            // Step 1-2: Send the command and get the raw response.
            ErrorCode result = SendCommand(command, timeoutMs, out response);
            if (result != ErrorCode.Success)
            {
                return failureCode;
            }

            // Step 3-4: Verify the response is the expected "OK" acknowledgement.
            return IsOkResponse(response) ? ErrorCode.Success : failureCode;
        }

        /// <summary>
        /// Sends the LON read trigger and retrieves the raw barcode response.
        ///
        /// Flow:
        ///   1. Send the LON payload to trigger a barcode read with a 5-second timeout.
        ///   2. If timeout occurs: send the LOFF payload to cancel the trigger, then return timeout error.
        ///   3. If any other error: return the error immediately.
        ///   4. On success: trim the response and return the raw barcode string.
        /// Exceptions are logged here and rethrown to the public operation boundary.
        /// </summary>
        private ErrorCode ReadRawBarcode(out string barcode)
        {
            barcode = string.Empty;

            try
            {
                string response;
                // Step 1: Send the LON trigger command to start barcode scanning.
                ErrorCode result = SendCommand(CommandRead, ReadTimeoutMs, out response);

                // Step 2: On timeout, send LOFF to cancel the pending read trigger on the hardware.
                if (result == ErrorCode.CarrierIdTimeout)
                {
                    StopReadTrigger();
                    return result;
                }

                // Step 3: Return any non-timeout errors immediately.
                if (result != ErrorCode.Success)
                {
                    return result;
                }

                // Step 4: Response received successfully; trim and return the barcode data.
                barcode = TrimResponse(response);
                return ErrorCode.Success;
            }
            catch (Exception ex)
            {
                _logger.WriteLog("CarrierIDReader", LogHeadType.Exception, string.Format("ReadRawBarcode: exception - {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Retry loop that attempts to read a barcode up to Config.BarcodeReaderMaxRetryCount times.
        /// Called by ReadCarrierId after MOTORON succeeds.
        /// Relies on ReadRawBarcode to encapsulate the LON send/wait flow and timeout cleanup.
        ///
        /// Flow (for each attempt):
        ///   1. Call ReadRawBarcode to send LON and get the raw response.
        ///   2. If timeout: abort immediately (no retry — timeout indicates hardware issue).
        ///   3. If other error: abort immediately.
        ///   4. Classify the response using ClassifyReadState:
        ///      - Success: valid barcode found — trim and return it.
        ///      - CarrierIdReadFailed: "NG" response — barcode unreadable, try again on next iteration.
        ///      - CarrierIdCommandFailed: invalid or "ERROR" response — abort immediately.
        ///   5. If all retries exhausted: return CarrierIdReadFailed.
        /// </summary>
        private ErrorCode TryReadCarrierId(out string carrierID)
        {
            carrierID = string.Empty;
            ErrorCode lastResult = ErrorCode.CarrierIdReadFailed;

            for (int attempt = 0; attempt < Config.BarcodeReaderMaxRetryCount; attempt++)
            {
                // Step 1: Send the LON read trigger and get the raw response.
                string barcode;
                ErrorCode readResult = ReadRawBarcode(out barcode);

                // Step 2: Timeout is non-recoverable — abort the entire read operation.
                if (readResult == ErrorCode.CarrierIdTimeout)
                {
                    _logger.WriteLog("CarrierIDReader", LogHeadType.Error, string.Format("GetCarrierID: read timeout on attempt {0}", attempt + 1));
                    return ErrorCode.CarrierIdTimeout;
                }

                // Step 3: Any other communication error — abort immediately.
                if (readResult != ErrorCode.Success)
                {
                    return readResult;
                }

                // Step 4: Classify the barcode response to determine the next action.
                ErrorCode state = ClassifyReadState(barcode);
                if (state == ErrorCode.Success)
                {
                    // Valid barcode data received — return the trimmed carrier ID.
                    carrierID = TrimResponse(barcode);
                    return ErrorCode.Success;
                }

                if (state == ErrorCode.CarrierIdReadFailed)
                {
                    // "NG" — barcode was unreadable on this attempt, continue to next retry.
                    _logger.WriteLog("CarrierIDReader", LogHeadType.Error, string.Format("GetCarrierID: unreadable barcode on attempt {0}", attempt + 1));
                    lastResult = ErrorCode.CarrierIdReadFailed;
                    continue;
                }

                return state;
            }

            // Step 5: All retries exhausted without a successful read.
            return lastResult;
        }

        /// <summary>
        /// Sends the LOFF command to cancel an in-progress barcode read trigger.
        /// Called after a read timeout to ensure the hardware is in a clean state.
        /// The response is intentionally ignored (best-effort cleanup).
        /// </summary>
        private void StopReadTrigger()
        {
            string stopResponse;
            SendCommand(CommandStop, 500, out stopResponse);
        }

        /// <summary>
        /// Classifies a raw barcode response into an existing operation result code.
        /// Used by TryReadCarrierId to decide whether to accept, retry, or abort.
        ///
        /// Classification rules:
        ///   - Empty / null / "OK": CarrierIdCommandFailed (no barcode data present).
        ///   - "NG": CarrierIdReadFailed (barcode unreadable but retryable).
        ///   - "ERROR": CarrierIdCommandFailed (hardware-level error).
        ///   - Printable ASCII text: Success (valid barcode data).
        ///   - Non-printable data: CarrierIdCommandFailed.
        /// </summary>
        private static ErrorCode ClassifyReadState(string response)
        {
            string normalized = TrimResponse(response);

            // Empty response or bare "OK" (motor ack without barcode data) — invalid.
            if (string.IsNullOrEmpty(normalized) || string.Equals(normalized, "OK", StringComparison.OrdinalIgnoreCase))
            {
                return ErrorCode.CarrierIdCommandFailed;
            }

            // "NG" — barcode not readable on this attempt, eligible for retry.
            if (string.Equals(normalized, "NG", StringComparison.OrdinalIgnoreCase))
            {
                return ErrorCode.CarrierIdReadFailed;
            }

            // "ERROR" — hardware failure, no retry possible.
            if (string.Equals(normalized, "ERROR", StringComparison.OrdinalIgnoreCase))
            {
                return ErrorCode.CarrierIdCommandFailed;
            }

            // Any printable ASCII string is treated as successful barcode data.
            return IsPrintableAscii(normalized) ? ErrorCode.Success : ErrorCode.CarrierIdCommandFailed;
        }

        /// <summary>
        /// Checks if the response is the "OK" acknowledgement (case-insensitive, trimmed).
        /// </summary>
        private static bool IsOkResponse(string response)
        {
            return string.Equals(TrimResponse(response), "OK", StringComparison.OrdinalIgnoreCase);
        }

    }
}