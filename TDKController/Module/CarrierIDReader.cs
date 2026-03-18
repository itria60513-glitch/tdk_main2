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
    ///       3. Ensure a connection is available for the operation.
    ///       4. Execute the protocol-specific read/write operation.
    ///       5. Release only the connection opened by this class, then release the busy lock.
    ///
    /// Asynchronous response handling:
    ///   - The IConnector.DataReceived event fires OnDataReceived, which stores the raw
    ///     response in LastResponse and signals _responseSignal.
    ///   - SendCommand waits on _responseSignal with a configurable timeout, then delegates
    ///     response validation to the subclass via ParseCarrierIDReaderData.
    /// </summary>
    public abstract class CarrierIDReader : ICarrierIDReader, IDisposable
    {
        #region Constants And Fields

        private const string LogKey = "CarrierIDReader";

        private IConnector _connector;
        private string _carrierID = string.Empty;
        // Thread-safe busy flag: 0 = idle, 1 = busy. Prevents concurrent reader access.
        private int _busyFlag;
        // Thread-safe disposed flag: 0 = active, 1 = disposed. Ensures single disposal.
        private int _disposed;

        #endregion

        #region Construction And Core Properties

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

        /// <summary>Reader configuration (timeout, retry count, and other shared settings).</summary>
        public CarrierIDReaderConfig Config { get; }

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

        /// <summary>
        /// Gets the last known carrier identifier value cached by this reader instance.
        /// </summary>
        public string CarrierID
        {
            get { return _carrierID; }
        }

        /// <summary>
        /// Raised when <see cref="CarrierID"/> changes after a successful read or write.
        /// </summary>
        public event CarrierIDChangedEventHandler CarrierIDChanged;

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
                ThrowIfDisposed();

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

        #endregion

        #region Protocol Contract

        /// <summary>
        /// Protocol-specific response parser. Called by SendCommand after a response is received.
        /// Each subclass implements its own validation logic (e.g., checking for "00" prefix,
        /// "OK"/"NG" responses, or Hermes frame checksums).
        /// </summary>
        protected abstract ErrorCode ParseCarrierIDReaderData(string command);

        /// <summary>
        /// Protocol-specific carrier ID read operation. Entry point called by the host application.
        /// Subclasses typically delegate to ExecuteRead with their own validation and read logic.
        /// </summary>
        public abstract ErrorCode GetCarrierID(int page, out string carrierID);

        /// <summary>
        /// Default implementation of page-based carrier ID write. Returns CarrierIdError to indicate
        /// that page-based writing is not supported. RFID readers override this to provide actual behavior.
        /// </summary>
        public virtual ErrorCode SetCarrierID(int page, string carrierID)
        {
            ThrowIfDisposed();
            return ErrorCode.CarrierIdError;
        }

        #endregion

        #region Disposal

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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases managed resources owned by this instance.
        /// The disposed flag is updated atomically to prevent duplicate cleanup.
        /// </summary>
        /// <param name="disposing">
        /// True when called from <see cref="Dispose()"/>; false when called from a finalizer.
        /// This type currently owns only managed resources, but the overload keeps the pattern extensible.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            // Step 1: Atomically check and set the disposed flag to prevent double disposal.
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            if (!disposing)
            {
                return;
            }

            try
            {
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
            }
        }

        #endregion

        #region Busy And Connection Control

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
                ThrowIfDisposed();

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
                ThrowIfDisposed();

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

        #endregion

        #region Template Methods

        /// <summary>
        /// Page-specific pre-read validation hook for readers that need to validate the target page.
        /// Readers without page-specific validation can use the default Success implementation.
        /// </summary>
        protected virtual ErrorCode ValidateReadRequest(int page)
        {
            return ErrorCode.Success;
        }

        /// <summary>
        /// Non-page pre-read validation hook for readers that do not care about page input.
        /// </summary>
        protected virtual ErrorCode ValidateReadRequest()
        {
            return ErrorCode.Success;
        }

        /// <summary>
        /// Non-page read hook invoked by the simple ExecuteRead template method.
        /// Readers such as BarcodeReader override this to provide their core read logic.
        /// </summary>
        protected virtual ErrorCode ReadCarrierId(out string carrierID)
        {
            carrierID = string.Empty;
            return ErrorCode.CarrierIdError;
        }

        /// <summary>
        /// Page-specific read hook invoked by the page-based ExecuteRead template method.
        /// Page-aware readers override this to perform their actual protocol-specific read.
        /// </summary>
        protected virtual ErrorCode ReadCarrierId(int page, out string carrierID)
        {
            carrierID = string.Empty;
            return ErrorCode.CarrierIdError;
        }

        /// <summary>
        /// Page-specific pre-write validation hook for readers that support writing.
        /// </summary>
        protected virtual ErrorCode ValidateWriteRequest(int page, string carrierID)
        {
            return ErrorCode.Success;
        }

        /// <summary>
        /// Page-specific write hook invoked by the page-based ExecuteWrite template method.
        /// </summary>
        protected virtual ErrorCode WriteCarrierId(int page, string carrierID)
        {
            return ErrorCode.CarrierIdError;
        }

        #endregion

        #region Shared Operation Flow

        /// <summary>
        /// Simplified ExecuteRead template method for readers without page-specific behavior.
        /// </summary>
        protected ErrorCode ExecuteRead(out string carrierID)
        {
            ThrowIfDisposed();

            carrierID = string.Empty;
            bool openedConnection;

            ErrorCode beginResult = TryBeginOperation("ExecuteRead", out openedConnection);
            if (beginResult != ErrorCode.Success)
            {
                return beginResult;
            }

            try
            {
                ErrorCode validationResult = ValidateReadRequest();
                if (validationResult != ErrorCode.Success)
                {
                    LogValidationFailure(validationResult, "ExecuteRead");
                    return validationResult;
                }

                ErrorCode readResult = ReadCarrierId(out carrierID);
                if (readResult == ErrorCode.Success)
                {
                    UpdateCarrierID(carrierID);
                }

                return readResult;
            }
            finally
            {
                EndOperation(openedConnection);
            }
        }

        /// <summary>
        /// Page-based ExecuteRead template method.
        /// The page-aware subclass only needs to override ValidateReadRequest and ReadCarrierId,
        /// and the shared busy-lock / connection lifecycle remains centralized here.
        /// </summary>
        protected ErrorCode ExecuteRead(int page, out string carrierID)
        {
            ThrowIfDisposed();

            carrierID = string.Empty;
            bool openedConnection;

            ErrorCode beginResult = TryBeginOperation("ExecuteRead", out openedConnection);
            if (beginResult != ErrorCode.Success)
            {
                return beginResult;
            }

            try
            {
                ErrorCode validationResult = ValidateReadRequest(page);
                if (validationResult != ErrorCode.Success)
                {
                    LogValidationFailure(validationResult, "ExecuteRead");
                    return validationResult;
                }

                ErrorCode readResult = ReadCarrierId(page, out carrierID);
                if (readResult == ErrorCode.Success)
                {
                    UpdateCarrierID(carrierID);
                }

                return readResult;
            }
            finally
            {
                EndOperation(openedConnection);
            }
        }

        /// <summary>
        /// Page-based ExecuteWrite template method.
        /// The page-aware subclass only needs to override ValidateWriteRequest and WriteCarrierId.
        /// </summary>
        protected ErrorCode ExecuteWrite(int page, string carrierID)
        {
            ThrowIfDisposed();

            bool openedConnection;

            ErrorCode beginResult = TryBeginOperation("ExecuteWrite", out openedConnection);
            if (beginResult != ErrorCode.Success)
            {
                return beginResult;
            }

            try
            {
                ErrorCode validationResult = ValidateWriteRequest(page, carrierID);
                if (validationResult != ErrorCode.Success)
                {
                    LogValidationFailure(validationResult, "ExecuteWrite");
                    return validationResult;
                }

                ErrorCode writeResult = WriteCarrierId(page, carrierID);
                if (writeResult == ErrorCode.Success)
                {
                    UpdateCarrierID(carrierID);
                }

                return writeResult;
            }
            finally
            {
                EndOperation(openedConnection);
            }
        }

        /// <summary>
        /// Acquires the busy lock and ensures a connection is available for the operation.
        /// </summary>
        private ErrorCode TryBeginOperation(string operationName, out bool openedConnection)
        {
            openedConnection = false;

            ErrorCode busyResult = AcquireBusy();
            if (busyResult != ErrorCode.Success)
            {
                return busyResult;
            }

            try
            {
                if (_connector == null)
                {
                    _logger.WriteLog(LogKey, LogHeadType.Error, string.Format("{0}: connector is null", operationName));
                    return ErrorCode.CarrierIdConnectFailed;
                }

                if (!_connector.IsConnected)
                {
                    ErrorCode connectResult = ConnectReader();
                    if (connectResult != ErrorCode.Success)
                    {
                        return connectResult;
                    }

                    openedConnection = true;
                }

                return ErrorCode.Success;
            }
            catch
            {
                ReleaseBusy();
                throw;
            }
        }

        /// <summary>
        /// Releases the connection opened by this class and then releases the busy lock.
        /// </summary>
        private void EndOperation(bool openedConnection)
        {
            if (openedConnection)
            {
                DisconnectReader();
            }

            ReleaseBusy();
        }

        #endregion

        #region Shared Utilities

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
        /// Writes a consistent validation failure log entry for shared template methods.
        /// This keeps ExecuteRead / ExecuteWrite signatures focused on behavior rather than log strings.
        /// </summary>
        private void LogValidationFailure(ErrorCode validationResult, string operationName)
        {
            _logger.WriteLog(
                LogKey,
                LogHeadType.Error,
                string.Format("{0}: validation failed with {1}", operationName, validationResult));
        }

        /// <summary>
        /// Updates the cached carrier ID and raises the change event only when the value changed.
        /// </summary>
        protected void UpdateCarrierID(string carrierID)
        {
            string normalized = carrierID ?? string.Empty;
            if (string.Equals(_carrierID, normalized, StringComparison.Ordinal))
            {
                return;
            }

            _carrierID = normalized;
            CarrierIDChanged?.Invoke();
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

        #endregion

        #region Lifetime And Event Handling

        /// <summary>
        /// Throws <see cref="ObjectDisposedException"/> when the reader has already been disposed.
        /// Prevents use-after-dispose on public and protected operation entry points.
        /// </summary>
        protected void ThrowIfDisposed()
        {
            if (Interlocked.CompareExchange(ref _disposed, 0, 0) != 0)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
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

        #endregion
    }
}
