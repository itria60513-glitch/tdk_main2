using System;
using System.Text;
using System.Threading;
using Communication.Interface;
using TDKController.Interface;
using TDKLogUtility.Module;

namespace TDKController
{
    /// <summary>
    /// Provides shared carrier reader infrastructure for protocol-specific implementations.
    /// </summary>
    public abstract class CarrierIDReader : ICarrierIDReader, IDisposable
    {
        private const string LogKey = "CarrierIDReader";

        private IConnector _connector;
        private int _busyFlag;
        private int _disposed;

        protected CarrierIDReader(CarrierIDReaderConfig config, IConnector connector, ILogUtility logger)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (connector == null) throw new ArgumentNullException(nameof(connector));
            Connector = connector;
        }

        protected CarrierIDReaderConfig Config { get; }

        protected readonly ILogUtility _logger;

        protected readonly ManualResetEventSlim _responseSignal = new ManualResetEventSlim(false);

        protected string LastResponse { get; set; }

        public string CarrierIDReaderConfigPath { get; set; }

        public abstract CarrierIDReaderType CarrierIDReaderType { get; }

        protected internal IConnector Connector
        {
            get { return _connector; }
            set
            {
                if (_connector != null)
                {
                    _connector.DataReceived -= OnDataReceived;
                }

                _connector = value;

                if (_connector != null)
                {
                    _connector.DataReceived += OnDataReceived;
                }
            }
        }

        public abstract ErrorCode ParseCarrierIDReaderData(string command);

        public abstract ErrorCode GetCarrierID(out string carrierID);

        public virtual ErrorCode SetCarrierID(string carrierID)
        {
            try
            {
                return ErrorCode.CarrierIdError;
            }
            catch (Exception ex)
            {
                _logger.WriteLog(LogKey, LogHeadType.Exception, string.Format("SetCarrierID: exception - {0}", ex.Message));
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                if (Interlocked.Exchange(ref _disposed, 1) != 0)
                {
                    return;
                }

                if (_connector != null)
                {
                    _connector.DataReceived -= OnDataReceived;
                    _connector = null;
                }

                _responseSignal.Dispose();
            }
            catch (Exception ex)
            {
                _logger.WriteLog(LogKey, LogHeadType.Exception, string.Format("Dispose: exception - {0}", ex.Message));
                throw;
            }
        }

        protected ErrorCode AcquireBusy()
        {
            return Interlocked.CompareExchange(ref _busyFlag, 1, 0) == 0
                ? ErrorCode.Success
                : ErrorCode.CarrierIdBusy;
        }

        protected void ReleaseBusy()
        {
            Interlocked.Exchange(ref _busyFlag, 0);
        }

        protected ErrorCode ConnectReader()
        {
            try
            {
                if (_connector == null)
                {
                    _logger.WriteLog(LogKey, LogHeadType.Error, "ConnectReader: connector is null");
                    return ErrorCode.CarrierIdConnectFailed;
                }

                _connector.Connect();
                return ErrorCode.Success;
            }
            catch (Exception ex)
            {
                _logger.WriteLog(LogKey, LogHeadType.Exception, string.Format("ConnectReader: exception - {0}", ex.Message));
                return ErrorCode.CarrierIdConnectFailed;
            }
        }

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

        protected ErrorCode SendCommand(string command, int timeoutMs, out string response)
        {
            response = string.Empty;

            try
            {
                if (string.IsNullOrWhiteSpace(command))
                {
                    _logger.WriteLog(LogKey, LogHeadType.Error, "SendCommand: command is empty");
                    return ErrorCode.CarrierIdCommandFailed;
                }

                if (_connector == null)
                {
                    _logger.WriteLog(LogKey, LogHeadType.Error, "SendCommand: connector is null");
                    return ErrorCode.CarrierIdConnectFailed;
                }

                LastResponse = string.Empty;
                _responseSignal.Reset();
                byte[] commandBytes = Encoding.ASCII.GetBytes(command);
                _connector.Send(commandBytes, commandBytes.Length);

                if (!_responseSignal.Wait(timeoutMs > 0 ? timeoutMs : Config.TimeoutMs))
                {
                    _logger.WriteLog(LogKey, LogHeadType.Error, string.Format("SendCommand: timeout waiting for response to {0}", command.Trim()));
                    return ErrorCode.CarrierIdTimeout;
                }

                response = LastResponse ?? string.Empty;
                ErrorCode parseResult = ParseCarrierIDReaderData(response);
                if (parseResult != ErrorCode.Success)
                {
                    _logger.WriteLog(LogKey, LogHeadType.Error, string.Format("SendCommand: invalid response for {0}, payload={1}", command.Trim(), TrimResponse(response)));
                }

                return parseResult;
            }
            catch (Exception ex)
            {
                _logger.WriteLog(LogKey, LogHeadType.Exception, string.Format("SendCommand: exception - {0}", ex.Message));
                throw;
            }
        }

        protected static string TrimResponse(string response)
        {
            return string.IsNullOrEmpty(response) ? string.Empty : response.Trim('\0', '\r', '\n', ' ');
        }

        protected static bool IsPrintableAscii(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            for (int index = 0; index < value.Length; index++)
            {
                char current = value[index];
                if (current < 32 || current > 126)
                {
                    return false;
                }
            }

            return true;
        }

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

        protected virtual void OnDataReceived(byte[] byData, int length)
        {
            try
            {
                if (byData == null || length <= 0)
                {
                    return;
                }

                LastResponse = Encoding.ASCII.GetString(byData, 0, length);
                _responseSignal.Set();
            }
            catch (Exception ex)
            {
                _logger.WriteLog(LogKey, LogHeadType.Exception, string.Format("OnDataReceived: exception - {0}", ex.Message));
            }
        }
    }
}
