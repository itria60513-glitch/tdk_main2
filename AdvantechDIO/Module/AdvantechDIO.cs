using System;
using Automation.BDaq;
using AdvantechDIO.Config;
using DIO;
using TDKLogUtility.Module;

namespace AdvantechDIO.Module
{
    /// <summary>
    /// Advantech DAQNavi DIO board driver implementing <see cref="IIOBoard"/>.
    /// Uses <see cref="InstantDiCtrl"/> for digital input and <see cref="InstantDoCtrl"/> for digital output.
    /// All SDK calls are serialized under a single lock (<see cref="_syncRoot"/>).
    /// <para>
    /// Why: Provides a unified DIO interface for Advantech hardware, driven by XML configuration.
    /// How: Constructor injection of <see cref="ILogUtility"/> and <see cref="AdvantechDIOConfig"/>;
    ///      <see cref="Connect"/> is the only startup entry that initializes SDK controllers.
    /// Constraints: Single .cs file per module (constitution); error codes are int, not enum; unit-test #region labels describe behavior (no task IDs).
    /// </para>
    /// </summary>
    public class AdvantechDIO : IIOBoard, IDisposable
    {
        #region Constants

        private const string LogKey = "AdvantechDIO";

        // Guard error codes (constitution range: -1000 ~ -1099)
        /// <summary>Error code returned when an I/O method is called while the device is not connected.</summary>
        public const int NotConnectedError = -1001;
        /// <summary>Error code returned when portIndex exceeds the configured port count.</summary>
        public const int PortIndexOutOfRangeError = -1002;
        /// <summary>Error code returned when bitIndex exceeds the configured bits per port.</summary>
        public const int BitIndexOutOfRangeError = -1003;

        #endregion

        #region Fields

        private readonly ILogUtility _logUtility;
        private readonly object _syncRoot = new object();
        private AdvantechDIOConfig _config;

        /// <summary>
        /// XML-mapped configuration for device ID and port topology.
        /// Updating this property will also refresh derived topology properties.
        /// </summary>
        public AdvantechDIOConfig Config
        {
            get => _config;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                lock (_syncRoot)
                {
                    _config = value;
                }
            }
        }

        private InstantDiCtrl _instantDiCtrl;
        private InstantDoCtrl _instantDoCtrl;
        private bool _isConnected;
        private string _deviceName = string.Empty;
        private bool _disposed;

        #endregion

        #region Events

        /// <inheritdoc />
        public event EventHandler ExceptionOccurred;

        /// <inheritdoc />
        public event EventHandler DI_ValueChanged;

        /// <inheritdoc />
        public event EventHandler DO_ValueChanged;

        #endregion

        #region Properties

        /// <inheritdoc />
        public bool IsConnected => _isConnected;

        /// <inheritdoc />
        /// <remarks>Always returns false; this implementation targets physical Advantech hardware only.</remarks>
        public bool IsVirtual => false;

        /// <inheritdoc />
        /// <remarks>Value comes from current <see cref="AdvantechDIOConfig.DeviceID"/>.</remarks>
        public int DeviceID
        {
            get
            {
                lock (_syncRoot)
                {
                    return _config.DeviceID;
                }
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// Empty string until <see cref="Connect"/> succeeds, then populated from SDK SelectedDevice.Description.
        /// </remarks>
        public string DeviceName => _deviceName;

        /// <inheritdoc />
        public int InputPortCount
        {
            get
            {
                lock (_syncRoot)
                {
                    return _config.DIPortCount;
                }
            }
        }

        /// <inheritdoc />
        public int InputBitsPerPort
        {
            get
            {
                lock (_syncRoot)
                {
                    return _config.PinCountPerPort;
                }
            }
        }

        /// <inheritdoc />
        public int OutputPortCount
        {
            get
            {
                lock (_syncRoot)
                {
                    return _config.DOPortCount;
                }
            }
        }

        /// <inheritdoc />
        public int OutputBitsPerPort
        {
            get
            {
                lock (_syncRoot)
                {
                    return _config.PinCountPerPort;
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of <see cref="AdvantechDIO"/> with dependency injection.
        /// </summary>
        /// <param name="logUtility">Logging facade for diagnostics. Must not be null.</param>
        /// <param name="config">XML-mapped configuration for device ID and port topology. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="logUtility"/> or <paramref name="config"/> is null.</exception>
        public AdvantechDIO(ILogUtility logUtility, AdvantechDIOConfig config)
        {
            _logUtility = logUtility ?? throw new ArgumentNullException(nameof(logUtility));
            Config = config ?? throw new ArgumentNullException(nameof(config));
        }

        #endregion

        #region Connection Lifecycle

        /// <summary>
        /// Establishes connection to the Advantech DIO device.
        /// This is the only startup entry point — initializes SDK controllers and selects the device.
        /// </summary>
        /// <returns>0 on success; SDK error code (cast to int) on failure.</returns>
        public int Connect()
        {
            try
            {
                lock (_syncRoot)
                {
                    if (_isConnected)
                    {
                        return (int)ErrorCode.Success;
                    }

                    var deviceInfo = new DeviceInformation(Config.DeviceID);

                    // Initialize DI controller if DI ports are configured
                    if (InputPortCount > 0)
                    {
                        _instantDiCtrl = new InstantDiCtrl();
                        _instantDiCtrl.SelectedDevice = deviceInfo;
                        if (!_instantDiCtrl.Initialized)
                        {
                            var errCode = ErrorCode.ErrorDeviceNotExist;
                            _logUtility.WriteLog(LogKey, LogHeadType.Error, $"DI controller failed to initialize for device ID {Config.DeviceID}");
                            RaiseExceptionOccurred();
                            CleanupControllers();
                            return (int)errCode;
                        }
                        _instantDiCtrl.ChangeOfState += new EventHandler<DiSnapEventArgs>(OnDiChangeOfState);
                    }

                    // Initialize DO controller if DO ports are configured
                    if (OutputPortCount > 0)
                    {
                        _instantDoCtrl = new InstantDoCtrl();
                        _instantDoCtrl.SelectedDevice = deviceInfo;
                        if (!_instantDoCtrl.Initialized)
                        {
                            var errCode = ErrorCode.ErrorDeviceNotExist;
                            _logUtility.WriteLog(LogKey, LogHeadType.Error, $"DO controller failed to initialize for device ID {Config.DeviceID}");
                            RaiseExceptionOccurred();
                            CleanupControllers();
                            return (int)errCode;
                        }
                    }

                    // Retrieve device name from whichever controller was initialized
                    if (_instantDiCtrl != null)
                    {
                        _deviceName = _instantDiCtrl.SelectedDevice.Description;
                    }
                    else if (_instantDoCtrl != null)
                    {
                        _deviceName = _instantDoCtrl.SelectedDevice.Description;
                    }

                    _isConnected = true;
                    _logUtility.WriteLog(LogKey, LogHeadType.Info, $"Connected to device [{DeviceID}] {_deviceName}");
                    return (int)ErrorCode.Success;
                }
            }
            catch (Exception ex)
            {
                _logUtility.WriteLog(LogKey, LogHeadType.Error, $"Connect failed: {ex.Message}");
                RaiseExceptionOccurred();
                CleanupControllers();
                return (int)ErrorCode.ErrorDeviceNotExist;
            }
        }

        /// <summary>
        /// Closes the connection and releases SDK controllers.
        /// </summary>
        /// <returns>0 on success.</returns>
        public int Disconnect()
        {
            try
            {
                lock (_syncRoot)
                {
                    if (!_isConnected)
                    {
                        return (int)ErrorCode.Success;
                    }

                    CleanupControllers();
                    _isConnected = false;
                    _deviceName = string.Empty;
                    _logUtility.WriteLog(LogKey, LogHeadType.Info, $"Disconnected device [{DeviceID}]");
                    return (int)ErrorCode.Success;
                }
            }
            catch (Exception ex)
            {
                _logUtility.WriteLog(LogKey, LogHeadType.Error, $"Disconnect failed: {ex.Message}");
                RaiseExceptionOccurred();
                return (int)ErrorCode.ErrorUndefined;
            }
        }

        #endregion

        #region DI Methods

        /// <summary>
        /// Reads the full byte value of the specified digital input port via SDK.
        /// </summary>
        /// <param name="portIndex">Zero-based input port index.</param>
        /// <param name="value">The byte value read from the port.</param>
        /// <returns>0 on success; guard or SDK error code on failure.</returns>
        public int GetInput(int portIndex, out byte value)
        {
            value = 0;
            try
            {
                lock (_syncRoot)
                {
                    int guard = GuardDI(portIndex);
                    if (guard != (int)ErrorCode.Success)
                        return guard;

                    ErrorCode err = _instantDiCtrl.Read(portIndex, out value);
                    return HandleSdkResult(err, nameof(GetInput));
                }
            }
            catch (Exception ex)
            {
                _logUtility.WriteLog(LogKey, LogHeadType.Error, $"GetInput failed: {ex.Message}");
                RaiseExceptionOccurred();
                return (int)ErrorCode.ErrorUndefined;
            }
        }

        /// <summary>
        /// Reads a single bit from the specified digital input port via SDK ReadBit.
        /// </summary>
        /// <param name="portIndex">Zero-based input port index.</param>
        /// <param name="bitIndex">Zero-based bit index within the port.</param>
        /// <param name="value">The bit value (0 or 1) as a byte.</param>
        /// <returns>0 on success; guard or SDK error code on failure.</returns>
        public int GetInputBit(int portIndex, int bitIndex, out byte value)
        {
            value = 0;
            try
            {
                lock (_syncRoot)
                {
                    int guard = GuardDI(portIndex, bitIndex);
                    if (guard != (int)ErrorCode.Success)
                        return guard;

                    ErrorCode err = _instantDiCtrl.ReadBit(portIndex, bitIndex, out value);
                    return HandleSdkResult(err, nameof(GetInputBit));
                }
            }
            catch (Exception ex)
            {
                _logUtility.WriteLog(LogKey, LogHeadType.Error, $"GetInputBit failed: {ex.Message}");
                RaiseExceptionOccurred();
                return (int)ErrorCode.ErrorUndefined;
            }
        }

        /// <summary>
        /// Starts snapshot (interrupt-driven) monitoring of DI state changes.
        /// </summary>
        /// <returns>0 on success; guard or SDK error code on failure.</returns>
        public int SnapStart()
        {
            try
            {
                lock (_syncRoot)
                {
                    if (!_isConnected)
                    {
                        _logUtility.WriteLog(LogKey, LogHeadType.Warning, "SnapStart called while not connected");
                        return NotConnectedError;
                    }

                    if (_instantDiCtrl == null)
                    {
                        _logUtility.WriteLog(LogKey, LogHeadType.Warning, "SnapStart called but DI is not configured");
                        return NotConnectedError;
                    }

                    ErrorCode err = _instantDiCtrl.SnapStart();
                    return HandleSdkResult(err, nameof(SnapStart));
                }
            }
            catch (Exception ex)
            {
                _logUtility.WriteLog(LogKey, LogHeadType.Error, $"SnapStart failed: {ex.Message}");
                RaiseExceptionOccurred();
                return (int)ErrorCode.ErrorUndefined;
            }
        }

        /// <summary>
        /// Stops snapshot (interrupt-driven) monitoring of DI state changes.
        /// </summary>
        /// <returns>0 on success; guard or SDK error code on failure.</returns>
        public int SnapStop()
        {
            try
            {
                lock (_syncRoot)
                {
                    if (!_isConnected)
                    {
                        _logUtility.WriteLog(LogKey, LogHeadType.Warning, "SnapStop called while not connected");
                        return NotConnectedError;
                    }

                    if (_instantDiCtrl == null)
                    {
                        _logUtility.WriteLog(LogKey, LogHeadType.Warning, "SnapStop called but DI is not configured");
                        return NotConnectedError;
                    }

                    ErrorCode err = _instantDiCtrl.SnapStop();
                    return HandleSdkResult(err, nameof(SnapStop));
                }
            }
            catch (Exception ex)
            {
                _logUtility.WriteLog(LogKey, LogHeadType.Error, $"SnapStop failed: {ex.Message}");
                RaiseExceptionOccurred();
                return (int)ErrorCode.ErrorUndefined;
            }
        }

        #endregion

        #region DO Methods

        /// <summary>
        /// Writes the full byte value to the specified digital output port via SDK.
        /// Triggers <see cref="DO_ValueChanged"/> on success.
        /// </summary>
        /// <param name="portIndex">Zero-based output port index.</param>
        /// <param name="value">The byte value to write.</param>
        /// <returns>0 on success; guard or SDK error code on failure.</returns>
        public int SetOutput(int portIndex, byte value)
        {
            try
            {
                lock (_syncRoot)
                {
                    int guard = GuardDO(portIndex);
                    if (guard != (int)ErrorCode.Success)
                        return guard;

                    ErrorCode err = _instantDoCtrl.Write(portIndex, value);
                    int result = HandleSdkResult(err, nameof(SetOutput));
                    if (result == (int)ErrorCode.Success)
                    {
                        RaiseDO_ValueChanged();
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logUtility.WriteLog(LogKey, LogHeadType.Error, $"SetOutput failed: {ex.Message}");
                RaiseExceptionOccurred();
                return (int)ErrorCode.ErrorUndefined;
            }
        }

        /// <summary>
        /// Sets a single bit within the specified digital output port using read-modify-write.
        /// Reads the current port value, modifies the target bit, then writes back.
        /// Triggers <see cref="DO_ValueChanged"/> on success.
        /// </summary>
        /// <param name="portIndex">Zero-based output port index.</param>
        /// <param name="bitIndex">Zero-based bit index within the port.</param>
        /// <param name="value">Bit value (0 or 1) as a byte.</param>
        /// <returns>0 on success; guard or SDK error code on failure.</returns>
        public int SetOutputBit(int portIndex, int bitIndex, byte value)
        {
            try
            {
                lock (_syncRoot)
                {
                    int guard = GuardDO(portIndex, bitIndex);
                    if (guard != (int)ErrorCode.Success)
                        return guard;

                    // Read current port value
                    byte currentValue;
                    ErrorCode readErr = _instantDoCtrl.Read(portIndex, out currentValue);
                    int readResult = HandleSdkResult(readErr, "SetOutputBit.Read");
                    if (readResult != (int)ErrorCode.Success)
                        return readResult;

                    // Modify the target bit
                    byte newValue;
                    if (value != 0)
                        newValue = (byte)(currentValue | (1 << bitIndex));
                    else
                        newValue = (byte)(currentValue & ~(1 << bitIndex));

                    // Write back
                    ErrorCode writeErr = _instantDoCtrl.Write(portIndex, newValue);
                    int writeResult = HandleSdkResult(writeErr, "SetOutputBit.Write");
                    if (writeResult == (int)ErrorCode.Success)
                    {
                        RaiseDO_ValueChanged();
                    }
                    return writeResult;
                }
            }
            catch (Exception ex)
            {
                _logUtility.WriteLog(LogKey, LogHeadType.Error, $"SetOutputBit failed: {ex.Message}");
                RaiseExceptionOccurred();
                return (int)ErrorCode.ErrorUndefined;
            }
        }

        /// <summary>
        /// Reads the full byte value of the specified digital output port via SDK.
        /// </summary>
        /// <param name="portIndex">Zero-based output port index.</param>
        /// <param name="value">The byte value read from the port.</param>
        /// <returns>0 on success; guard or SDK error code on failure.</returns>
        public int GetOutput(int portIndex, out byte value)
        {
            value = 0;
            try
            {
                lock (_syncRoot)
                {
                    int guard = GuardDO(portIndex);
                    if (guard != (int)ErrorCode.Success)
                        return guard;

                    ErrorCode err = _instantDoCtrl.Read(portIndex, out value);
                    return HandleSdkResult(err, nameof(GetOutput));
                }
            }
            catch (Exception ex)
            {
                _logUtility.WriteLog(LogKey, LogHeadType.Error, $"GetOutput failed: {ex.Message}");
                RaiseExceptionOccurred();
                return (int)ErrorCode.ErrorUndefined;
            }
        }

        /// <summary>
        /// Reads a single bit from the specified digital output port.
        /// Derives the bit from <see cref="GetOutput"/> byte value.
        /// </summary>
        /// <param name="portIndex">Zero-based output port index.</param>
        /// <param name="bitIndex">Zero-based bit index within the port.</param>
        /// <param name="value">The bit value (0 or 1) as a byte.</param>
        /// <returns>0 on success; guard or SDK error code on failure.</returns>
        public int GetOutputBit(int portIndex, int bitIndex, out byte value)
        {
            value = 0;
            try
            {
                lock (_syncRoot)
                {
                    int guard = GuardDO(portIndex, bitIndex);
                    if (guard != (int)ErrorCode.Success)
                        return guard;

                    byte portValue;
                    ErrorCode err = _instantDoCtrl.Read(portIndex, out portValue);
                    int result = HandleSdkResult(err, nameof(GetOutputBit));
                    if (result == (int)ErrorCode.Success)
                    {
                        value = (byte)((portValue >> bitIndex) & 0x01);
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logUtility.WriteLog(LogKey, LogHeadType.Error, $"GetOutputBit failed: {ex.Message}");
                RaiseExceptionOccurred();
                return (int)ErrorCode.ErrorUndefined;
            }
        }

        #endregion

        #region Dispose

        /// <summary>
        /// Disposes the AdvantechDIO instance, disconnecting and releasing SDK resources.
        /// Safe for repeated calls (idempotent via <see cref="_disposed"/> flag).
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                Disconnect();
            }
            catch (Exception ex)
            {
                _logUtility.WriteLog(LogKey, LogHeadType.Error, $"Dispose failed during Disconnect: {ex.Message}");
            }

            _disposed = true;
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Validates connection state and DI port index range.
        /// </summary>
        private int GuardDI(int portIndex, int bitIndex = -1)
        {
            if (!_isConnected)
            {
                _logUtility.WriteLog(LogKey, LogHeadType.Warning, "I/O method called while not connected");
                return NotConnectedError;
            }

            if (_instantDiCtrl == null)
            {
                _logUtility.WriteLog(LogKey, LogHeadType.Warning, "DI method called but DI is not configured");
                return NotConnectedError;
            }

            if (portIndex < 0 || portIndex >= InputPortCount)
            {
                _logUtility.WriteLog(LogKey, LogHeadType.Warning, $"DI portIndex {portIndex} out of range [0..{InputPortCount - 1}]");
                return PortIndexOutOfRangeError;
            }

            if (bitIndex >= 0 && (bitIndex < 0 || bitIndex >= InputBitsPerPort))
            {
                _logUtility.WriteLog(LogKey, LogHeadType.Warning, $"DI bitIndex {bitIndex} out of range [0..{InputBitsPerPort - 1}]");
                return BitIndexOutOfRangeError;
            }

            return (int)ErrorCode.Success;
        }

        /// <summary>
        /// Validates connection state and DO port index range.
        /// </summary>
        private int GuardDO(int portIndex, int bitIndex = -1)
        {
            if (!_isConnected)
            {
                _logUtility.WriteLog(LogKey, LogHeadType.Warning, "I/O method called while not connected");
                return NotConnectedError;
            }

            if (_instantDoCtrl == null)
            {
                _logUtility.WriteLog(LogKey, LogHeadType.Warning, "DO method called but DO is not configured");
                return NotConnectedError;
            }

            if (portIndex < 0 || portIndex >= OutputPortCount)
            {
                _logUtility.WriteLog(LogKey, LogHeadType.Warning, $"DO portIndex {portIndex} out of range [0..{OutputPortCount - 1}]");
                return PortIndexOutOfRangeError;
            }

            if (bitIndex >= 0 && (bitIndex < 0 || bitIndex >= OutputBitsPerPort))
            {
                _logUtility.WriteLog(LogKey, LogHeadType.Warning, $"DO bitIndex {bitIndex} out of range [0..{OutputBitsPerPort - 1}]");
                return BitIndexOutOfRangeError;
            }

            return (int)ErrorCode.Success;
        }

        /// <summary>
        /// Normalizes SDK ErrorCode to int. Logs and raises ExceptionOccurred for non-success.
        /// </summary>
        private int HandleSdkResult(ErrorCode err, string methodName)
        {
            if (err == ErrorCode.Success)
                return (int)ErrorCode.Success;

            _logUtility.WriteLog(LogKey, LogHeadType.Error, $"{methodName} SDK error: {err}");
            RaiseExceptionOccurred();
            return (int)err;
        }

        /// <summary>
        /// Releases SDK controller instances and unsubscribes events.
        /// </summary>
        private void CleanupControllers()
        {
            if (_instantDiCtrl != null)
            {
                _instantDiCtrl.ChangeOfState -= new EventHandler<DiSnapEventArgs>(OnDiChangeOfState);
                _instantDiCtrl.Dispose();
                _instantDiCtrl = null;
            }

            if (_instantDoCtrl != null)
            {
                _instantDoCtrl.Dispose();
                _instantDoCtrl = null;
            }
        }

        /// <summary>
        /// SDK 
        /// OfState callback — forwards to <see cref="DI_ValueChanged"/>.
        /// </summary>
        private void OnDiChangeOfState(object sender, DiSnapEventArgs e)
        {
            try
            {
                _logUtility.WriteLog(LogKey, LogHeadType.Info, "DI ChangeOfState triggered");
                DI_ValueChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _logUtility.WriteLog(LogKey, LogHeadType.Error, $"OnDiChangeOfState callback failed: {ex.Message}");
                RaiseExceptionOccurred();
            }
        }

        private void RaiseExceptionOccurred()
        {
            ExceptionOccurred?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseDO_ValueChanged()
        {
            _logUtility.WriteLog(LogKey, LogHeadType.Info, "DO_ValueChanged callback raised");
            DO_ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
