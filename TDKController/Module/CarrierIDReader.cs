using System;
using System.Text;
using System.Threading;
using Communication.Interface;
using TDKLogUtility.Module;

namespace TDKController
{
    /// <summary>
    /// Provides shared carrier reader infrastructure for protocol-specific implementations.
    /// This abstract base class manages the full lifecycle of carrier ID reader operations
    /// including connection management, thread-safe busy locking, command/response exchange,
    /// and common validation utilities.
    ///
    /// Architecture overview:
    ///   - Subclasses (BarcodeReader, HermesRFID, OmronASCII, OmronHex) override
    ///     GetCarrierID / SetCarrierID / ParseCarrierIDReaderData for protocol-specific behavior.
    ///   - The base class provides two template methods: ExecuteRead and ExecuteWrite,
    ///     which enforce a consistent operation flow:
    ///       1. Acquire busy lock (thread-safe, one operation at a time).
    ///       2. Run optional validation (page range, payload format, etc.).
    ///       3. Connect to the reader hardware via IConnector.
    ///       4. Execute the protocol-specific read/write operation.
    ///       5. Disconnect from the reader and release the busy lock (in finally block).
    ///
    /// Asynchronous response handling:
    ///   - The IConnector.DataReceived event fires OnDataReceived, which stores the raw
    ///     response in LastResponse and signals _responseSignal.
    ///   - SendCommand waits on _responseSignal with a configurable timeout, then delegates
    ///     response validation to the subclass via ParseCarrierIDReaderData.
    /// </summary>
    public abstract class CarrierIDReader : ICarrierIDReader, IDisposable
    {
        private const string LogKey = "CarrierIDReader";

        /// <summary>
        /// Delegate signature for protocol-specific read operations.
        /// Used by ExecuteRead to invoke the subclass's core read logic.
        /// </summary>
        protected delegate ErrorCode ReadOperation(out string carrierID);

        private IConnector _connector;
        // Thread-safe busy flag: 0 = idle, 1 = busy. Prevents concurrent reader access.
        private int _busyFlag;
        // Thread-safe disposed flag: 0 = active, 1 = disposed. Ensures single disposal.
        private int _disposed;

        /// <summary>
        /// Initializes the base carrier ID reader.
        ///
        /// Flow:
        ///   1. Validate that config, connector, and logger are not null.
        ///   2. Store the configuration and logger references.
        ///   3. Set the Connector property, which wires up the DataReceived event handler.
        /// </summary>
        protected CarrierIDReader(CarrierIDReaderConfig config, IConnector connector, ILogUtility logger)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (connector == null) throw new ArgumentNullException(nameof(connector));
            // Setting Connector subscribes OnDataReceived to the connector's DataReceived event.
            Connector = connector;
        }

        /// <summary>Reader configuration (timeout, page, retry count, etc.).</summary>
        protected CarrierIDReaderConfig Config { get; }

        /// <summary>Logger instance for writing diagnostic and error messages.</summary>
        protected readonly ILogUtility _logger;

        /// <summary>
        /// Signaling mechanism for the send-receive cycle.
        /// Reset before sending a command; set by OnDataReceived when a response arrives.
        /// SendCommand blocks on this signal until the response arrives or the timeout elapses.
        /// </summary>
        protected readonly ManualResetEventSlim _responseSignal = new ManualResetEventSlim(false);

        /// <summary>
        /// Stores the most recent raw response received from the reader hardware.
        /// Written by OnDataReceived, read by SendCommand after _responseSignal is set.
        /// </summary>
        protected string LastResponse { get; set; }

        /// <summary>Identifies the specific reader protocol (BarcodeReader, HermesRFID, OmronASCII, OmronHex).</summary>
        public abstract CarrierIDReaderType CarrierIDReaderType { get; }

        /// <summary>
        /// Gets or sets the communication connector.
        /// When the connector is changed:
        ///   1. Unsubscribe OnDataReceived from the old connector (if any).
        ///   2. Store the new connector reference.
        ///   3. Subscribe OnDataReceived to the new connector (if any).
        /// This ensures the event handler is always wired to exactly one connector.
        /// </summary>
        public IConnector Connector
        {
            get { return _connector; }
            set
            {
                // Step 1: Detach from the previous connector to avoid stale event subscriptions.
                if (_connector != null)
                {
                    _connector.DataReceived -= OnDataReceived;
                }

                // Step 2: Store the new connector reference.
                _connector = value;

                // Step 3: Attach to the new connector to start receiving data events.
                if (_connector != null)
                {
                    _connector.DataReceived += OnDataReceived;
                }
            }
        }

        /// <summary>
        /// Protocol-specific response parser. Called by SendCommand after a response is received.
        /// Each subclass implements its own validation logic (e.g., checking for "00" prefix,
        /// "OK"/"NG" responses, or Hermes frame checksums).
        /// </summary>
        public abstract ErrorCode ParseCarrierIDReaderData(string command);

        /// <summary>
        /// Protocol-specific carrier ID read operation. Entry point called by the host application.
        /// Subclasses typically delegate to ExecuteRead with their own validation and read logic.
        /// </summary>
        public abstract ErrorCode GetCarrierID(out string carrierID);

        /// <summary>
        /// Default implementation of carrier ID write. Returns CarrierIdError to indicate
        /// that writing is not supported. Subclasses that support writing (HermesRFID, OmronASCII,
        /// OmronHex) override this to provide actual write functionality.
        /// </summary>
        public virtual ErrorCode SetCarrierID(string carrierID)
        {
            try
            {
                // Base implementation does not support writing; return error.
                return ErrorCode.CarrierIdError;
            }
            catch (Exception ex)
            {
                _logger.WriteLog(LogKey, LogHeadType.Exception, string.Format("SetCarrierID: exception - {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Releases all resources held by this reader instance.
        ///
        /// Flow:
        ///   1. Atomically set _disposed to 1; if already disposed, return immediately.
        ///   2. Unsubscribe OnDataReceived from the connector and null the reference.
        ///   3. Dispose the ManualResetEventSlim signal.
        /// Thread-safe: uses Interlocked.Exchange to guarantee single execution.
        /// </summary>
        public void Dispose()
        {
            try
            {
                // Step 1: Atomically check and set the disposed flag to prevent double disposal.
                if (Interlocked.Exchange(ref _disposed, 1) != 0)
                {
                    return;
                }

                // Step 2: Detach the event handler and release the connector reference.
                if (_connector != null)
                {
                    _connector.DataReceived -= OnDataReceived;
                    _connector = null;
                }

                // Step 3: Release the OS-level wait handle used for response signaling.
                _responseSignal.Dispose();
            }
            catch (Exception ex)
            {
                _logger.WriteLog(LogKey, LogHeadType.Exception, string.Format("Dispose: exception - {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Attempts to acquire the busy lock using an atomic compare-and-swap.
        /// Returns Success if the lock was acquired, or CarrierIdBusy if another operation
        /// is already in progress. This prevents concurrent access to the reader hardware.
        /// </summary>
        protected ErrorCode AcquireBusy()
        {
            // Atomically: if _busyFlag == 0, set it to 1 and return Success.
            // Otherwise, another operation holds the lock; return CarrierIdBusy.
            return Interlocked.CompareExchange(ref _busyFlag, 1, 0) == 0
                ? ErrorCode.Success
                : ErrorCode.CarrierIdBusy;
        }

        /// <summary>
        /// Releases the busy lock, allowing other operations to proceed.
        /// Called in the finally block of ExecuteRead / ExecuteWrite.
        /// </summary>
        protected void ReleaseBusy()
        {
            Interlocked.Exchange(ref _busyFlag, 0);
        }

        /// <summary>
        /// Establishes a connection to the reader hardware via the IConnector.
        ///
        /// Flow:
        ///   1. Check if the connector reference is valid (not null).
        ///   2. Call _connector.Connect() to open the communication channel.
        ///   3. Return Success on success, or CarrierIdConnectFailed on failure.
        /// </summary>
        protected ErrorCode ConnectReader()
        {
            try
            {
                // Step 1: Guard against null connector.
                if (_connector == null)
                {
                    _logger.WriteLog(LogKey, LogHeadType.Error, "ConnectReader: connector is null");
                    return ErrorCode.CarrierIdConnectFailed;
                }

                // Step 2: Open the communication channel (serial port, TCP socket, etc.).
                _connector.Connect();
                return ErrorCode.Success;
            }
            catch (Exception ex)
            {
                // Step 3: Connection failed due to an exception (port in use, network error, etc.).
                _logger.WriteLog(LogKey, LogHeadType.Exception, string.Format("ConnectReader: exception - {0}", ex.Message));
                return ErrorCode.CarrierIdConnectFailed;
            }
        }

        /// <summary>
        /// Closes the connection to the reader hardware.
        /// Exceptions are logged but not re-thrown, since this is called in cleanup/finally blocks.
        /// </summary>
        protected void DisconnectReader()
        {
            try
            {
                _connector?.Disconnect();
            }
            catch (Exception ex)
            {
                _logger.WriteLog(LogKey, LogHeadType.Exception, string.Format("DisconnectReader: exception - {0}", ex.Message));
            }
        }

        /// <summary>
        /// Sends a command string to the reader and waits for a response.
        /// This is the core communication method used by all protocol-specific implementations.
        ///
        /// Flow:
        ///   1. Validate that the command is not empty and the connector is available.
        ///   2. Clear the previous response and reset the response signal.
        ///   3. Encode the command as ASCII bytes and send via the connector.
        ///   4. Block on _responseSignal.Wait() until a response arrives or the timeout elapses.
        ///      - If timeout: log error and return CarrierIdTimeout.
        ///   5. Retrieve the raw response from LastResponse (set by OnDataReceived).
        ///   6. Delegate to ParseCarrierIDReaderData (subclass) to validate the response format.
        ///      - If parse fails: log the invalid response payload.
        ///   7. Return the parse result (Success or protocol-specific error code).
        /// </summary>
        protected ErrorCode SendCommand(string command, int timeoutMs, out string response)
        {
            response = string.Empty;

            try
            {
                // Step 1a: Validate command is not empty.
                if (string.IsNullOrWhiteSpace(command))
                {
                    _logger.WriteLog(LogKey, LogHeadType.Error, "SendCommand: command is empty");
                    return ErrorCode.CarrierIdCommandFailed;
                }

                // Step 1b: Validate connector is available.
                if (_connector == null)
                {
                    _logger.WriteLog(LogKey, LogHeadType.Error, "SendCommand: connector is null");
                    return ErrorCode.CarrierIdConnectFailed;
                }

                // Step 2: Clear previous state to prepare for the new send-receive cycle.
                LastResponse = string.Empty;
                _responseSignal.Reset();

                // Step 3: Encode and transmit the command bytes.
                byte[] commandBytes = Encoding.ASCII.GetBytes(command);
                _connector.Send(commandBytes, commandBytes.Length);

                // Step 4: Wait for the response. Fall back to Config.TimeoutMs when caller passes 0.
                if (!_responseSignal.Wait(timeoutMs > 0 ? timeoutMs : Config.TimeoutMs))
                {
                    // Timeout elapsed without receiving a response from the reader.
                    _logger.WriteLog(LogKey, LogHeadType.Error, string.Format("SendCommand: timeout waiting for response to {0}", command.Trim()));
                    return ErrorCode.CarrierIdTimeout;
                }

                // Step 5: Retrieve the raw response captured by OnDataReceived.
                response = LastResponse ?? string.Empty;

                // Step 6: Delegate response validation to the protocol-specific subclass.
                ErrorCode parseResult = ParseCarrierIDReaderData(response);
                if (parseResult != ErrorCode.Success)
                {
                    _logger.WriteLog(LogKey, LogHeadType.Error, string.Format("SendCommand: invalid response for {0}, payload={1}", command.Trim(), TrimResponse(response)));
                }

                // Step 7: Return the protocol-specific parse result.
                return parseResult;
            }
            catch (Exception ex)
            {
                _logger.WriteLog(LogKey, LogHeadType.Exception, string.Format("SendCommand: exception - {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Simplified ExecuteRead overload that skips validation.
        /// Delegates directly to the full overload with null validation parameters.
        /// Used by readers that don't require pre-read validation (e.g., BarcodeReader).
        /// </summary>
        protected ErrorCode ExecuteRead(ReadOperation readOperation, out string carrierID)
        {
            return ExecuteRead(null, null, readOperation, out carrierID);
        }

        /// <summary>
        /// Template method that orchestrates the full carrier ID read lifecycle.
        /// All subclasses use this method to ensure consistent locking, connection, and cleanup.
        ///
        /// Flow:
        ///   1. Acquire the busy lock to prevent concurrent reader access.
        ///      - If busy: return CarrierIdBusy immediately.
        ///   2. Run the optional validation operation (e.g., page range check).
        ///      - If validation fails: log error and return the validation error code.
        ///   3. Connect to the reader hardware via ConnectReader.
        ///      - If connection fails: return CarrierIdConnectFailed.
        ///   4. Invoke the protocol-specific readOperation to perform the actual read.
        ///   5. (finally) Disconnect from the reader if connected, then release the busy lock.
        ///
        /// The finally block guarantees cleanup even if the read operation throws an exception.
        /// </summary>
        protected ErrorCode ExecuteRead(Func<ErrorCode> validateOperation, string validationErrorMessage, ReadOperation readOperation, out string carrierID)
        {
            carrierID = string.Empty;
            bool connected = false;

            // Step 1: Acquire the busy lock (atomic compare-and-swap).
            ErrorCode busyResult = AcquireBusy();
            if (busyResult != ErrorCode.Success)
            {
                return busyResult;
            }

            try
            {
                // Step 2: Run optional pre-read validation (null means no validation needed).
                ErrorCode validationResult = validateOperation == null ? ErrorCode.Success : validateOperation();
                if (validationResult != ErrorCode.Success)
                {
                    if (!string.IsNullOrEmpty(validationErrorMessage))
                    {
                        _logger.WriteLog(LogKey, LogHeadType.Error, validationErrorMessage);
                    }

                    return validationResult;
                }

                // Step 3: Establish the connection to the reader hardware.
                ErrorCode connectResult = ConnectReader();
                if (connectResult != ErrorCode.Success)
                {
                    return connectResult;
                }

                // Step 4: Connection established; invoke the protocol-specific read operation.
                connected = true;
                return readOperation(out carrierID);
            }
            finally
            {
                // Step 5: Cleanup — disconnect if connected, then always release the busy lock.
                if (connected) DisconnectReader();
                ReleaseBusy();
            }
        }

        /// <summary>
        /// Template method that orchestrates the full carrier ID write lifecycle.
        /// Mirrors ExecuteRead but passes the carrier ID string to the validation and write operations.
        ///
        /// Flow:
        ///   1. Acquire the busy lock to prevent concurrent reader access.
        ///      - If busy: return CarrierIdBusy immediately.
        ///   2. Run the optional validation operation (e.g., page range + payload format check).
        ///      - If validation fails: log error and return the validation error code.
        ///   3. Connect to the reader hardware via ConnectReader.
        ///      - If connection fails: return CarrierIdConnectFailed.
        ///   4. Invoke the protocol-specific writeOperation to send the write command.
        ///   5. (finally) Disconnect from the reader if connected, then release the busy lock.
        /// </summary>
        protected ErrorCode ExecuteWrite(string carrierID, Func<string, ErrorCode> validateOperation, string validationErrorMessage, Func<string, ErrorCode> writeOperation)
        {
            bool connected = false;

            // Step 1: Acquire the busy lock (atomic compare-and-swap).
            ErrorCode busyResult = AcquireBusy();
            if (busyResult != ErrorCode.Success)
            {
                return busyResult;
            }

            try
            {
                // Step 2: Run optional pre-write validation (null means no validation needed).
                ErrorCode validationResult = validateOperation == null ? ErrorCode.Success : validateOperation(carrierID);
                if (validationResult != ErrorCode.Success)
                {
                    if (!string.IsNullOrEmpty(validationErrorMessage))
                    {
                        _logger.WriteLog(LogKey, LogHeadType.Error, validationErrorMessage);
                    }

                    return validationResult;
                }

                // Step 3: Establish the connection to the reader hardware.
                ErrorCode connectResult = ConnectReader();
                if (connectResult != ErrorCode.Success)
                {
                    return connectResult;
                }

                // Step 4: Connection established; invoke the protocol-specific write operation.
                connected = true;
                return writeOperation(carrierID);
            }
            finally
            {
                // Step 5: Cleanup — disconnect if connected, then always release the busy lock.
                if (connected) DisconnectReader();
                ReleaseBusy();
            }
        }

        /// <summary>
        /// Removes null characters, carriage returns, newlines, and spaces from both ends
        /// of the response string. Returns empty string if input is null or empty.
        /// Used by all subclasses to normalize raw reader responses before parsing.
        /// </summary>
        protected static string TrimResponse(string response)
        {
            return string.IsNullOrEmpty(response) ? string.Empty : response.Trim('\0', '\r', '\n', ' ');
        }

        /// <summary>
        /// Validates that every character in the string is a printable ASCII character
        /// (decimal 32 through 126, inclusive). Used by barcode and Omron ASCII readers
        /// to verify response payloads contain only visible characters.
        /// Returns false for null, empty, or strings containing control characters / extended ASCII.
        /// </summary>
        protected static bool IsPrintableAscii(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            for (int index = 0; index < value.Length; index++)
            {
                char current = value[index];
                // Printable ASCII range: space (0x20) through tilde (0x7E).
                if (current < 32 || current > 126)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Validates that every character in the string is a valid hexadecimal digit
        /// (0-9, A-F, a-f). Used by Hermes RFID and Omron HEX readers to verify
        /// that payloads contain only hex-encoded data.
        /// Returns false for null or empty strings.
        /// </summary>
        protected static bool IsHexString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            for (int index = 0; index < value.Length; index++)
            {
                char current = value[index];
                bool isDigit = current >= '0' && current <= '9';
                bool isUpper = current >= 'A' && current <= 'F';
                bool isLower = current >= 'a' && current <= 'f';
                if (!isDigit && !isUpper && !isLower)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Event handler invoked by the IConnector when data is received from the reader hardware.
        /// This is the asynchronous callback that completes the send-receive cycle started by SendCommand.
        ///
        /// Flow:
        ///   1. Validate the received data is not null and has positive length.
        ///   2. Decode the raw bytes into an ASCII string and store in LastResponse.
        ///   3. Signal _responseSignal to unblock the waiting SendCommand call.
        /// </summary>
        protected virtual void OnDataReceived(byte[] byData, int length)
        {
            try
            {
                // Step 1: Ignore empty or null data packets.
                if (byData == null || length <= 0)
                {
                    return;
                }

                // Step 2: Decode raw bytes to ASCII and store as the latest response.
                LastResponse = Encoding.ASCII.GetString(byData, 0, length);

                // Step 3: Signal that a response has arrived, unblocking SendCommand.
                _responseSignal.Set();
            }
            catch (Exception ex)
            {
                _logger.WriteLog(LogKey, LogHeadType.Exception, string.Format("OnDataReceived: exception - {0}", ex.Message));
            }
        }
    }
}
