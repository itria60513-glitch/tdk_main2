using System;
using System.Text;
using Communication.Interface;
using TDKLogUtility.Module;

namespace TDKController
{
    /// <summary>
    /// Implements the BL600 barcode reader workflow.
    /// </summary>
    public class IDReaderBarcodeReader : CarrierIDReader
    {
        private const int MotorOnTimeoutMs = 10000;
        private const int MotorOffTimeoutMs = 3000;
        private const int ReadTimeoutMs = 5000;
        private const string CommandMotorOn = "MOTORON\r";
        private const string CommandMotorOff = "MOTOROFF\r";
        private const string CommandRead = "LON\r";
        private const string CommandStop = "LOFF\r";

        private bool _waitingReadResult;

        public IDReaderBarcodeReader(CarrierIDReaderConfig config, IConnector connector, ILogUtility logger)
            : base(config, connector, logger)
        {
        }

        /// <inheritdoc />
        public override CarrierIDReaderType CarrierIDReaderType
        {
            get { return CarrierIDReaderType.BarcodeReader; }
        }

        /// <inheritdoc />
        public override ErrorCode ParseCarrierIDReaderData(string command)
        {
            try
            {
                string normalized = TrimResponse(command);
                if (string.IsNullOrEmpty(normalized))
                {
                    return ErrorCode.CarrierIdCommandFailed;
                }

                if (string.Equals(normalized, "OK", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(normalized, "NG", StringComparison.OrdinalIgnoreCase))
                {
                    return ErrorCode.Success;
                }

                if (string.Equals(normalized, "ERROR", StringComparison.OrdinalIgnoreCase))
                {
                    return ErrorCode.CarrierIdCommandFailed;
                }

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
        /// </summary>
        public ErrorCode MotorON()
        {
            try
            {
                string response;
                ErrorCode result = SendCommand(CommandMotorOn, MotorOnTimeoutMs, out response);
                return result == ErrorCode.Success && string.Equals(TrimResponse(response), "OK", StringComparison.OrdinalIgnoreCase)
                    ? ErrorCode.Success
                    : ErrorCode.CarrierIdMotorOnFailed;
            }
            catch (Exception ex)
            {
                _logger.WriteLog("CarrierIDReader", LogHeadType.Exception, string.Format("MotorON: exception - {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Sends the BL600 read trigger and returns the raw barcode response.
        /// </summary>
        public ErrorCode ReadBarCode(out string barcode)
        {
            barcode = string.Empty;

            try
            {
                _waitingReadResult = true;
                string response;
                ErrorCode result = SendCommand(CommandRead, ReadTimeoutMs, out response);
                if (result == ErrorCode.CarrierIdTimeout)
                {
                    string stopResponse;
                    SendCommand(CommandStop, 500, out stopResponse);
                    return result;
                }

                if (result != ErrorCode.Success)
                {
                    return result;
                }

                barcode = TrimResponse(response);
                return string.Equals(barcode, "ERROR", StringComparison.OrdinalIgnoreCase)
                    ? ErrorCode.CarrierIdCommandFailed
                    : ErrorCode.Success;
            }
            catch (Exception ex)
            {
                _logger.WriteLog("CarrierIDReader", LogHeadType.Exception, string.Format("ReadBarCode: exception - {0}", ex.Message));
                throw;
            }
            finally
            {
                _waitingReadResult = false;
            }
        }

        /// <summary>
        /// Sends the BL600 MOTOROFF command and waits for the OK acknowledgement.
        /// </summary>
        public ErrorCode MotorOFF()
        {
            try
            {
                string response;
                ErrorCode result = SendCommand(CommandMotorOff, MotorOffTimeoutMs, out response);
                return result == ErrorCode.Success && string.Equals(TrimResponse(response), "OK", StringComparison.OrdinalIgnoreCase)
                    ? ErrorCode.Success
                    : ErrorCode.CarrierIdMotorOffFailed;
            }
            catch (Exception ex)
            {
                _logger.WriteLog("CarrierIDReader", LogHeadType.Exception, string.Format("MotorOFF: exception - {0}", ex.Message));
                throw;
            }
        }

        /// <inheritdoc />
        public override ErrorCode GetCarrierID(out string carrierID)
        {
            carrierID = string.Empty;

            try
            {
                ErrorCode busyResult = AcquireBusy();
                if (busyResult != ErrorCode.Success)
                {
                    return busyResult;
                }

                try
                {
                    ErrorCode connectResult = ConnectReader();
                    if (connectResult != ErrorCode.Success)
                    {
                        return connectResult;
                    }

                    ErrorCode motorOnResult = MotorON();
                    if (motorOnResult != ErrorCode.Success)
                    {
                        _logger.WriteLog("CarrierIDReader", LogHeadType.Error, "GetCarrierID: MOTORON failed");
                        return motorOnResult;
                    }

                    ErrorCode result = ErrorCode.CarrierIdReadFailed;

                    for (int attempt = 0; attempt < Config.MaxRetryCount; attempt++)
                    {
                        string response;
                        ErrorCode readResult = ReadBarCode(out response);
                        if (readResult == ErrorCode.CarrierIdTimeout)
                        {
                            _logger.WriteLog("CarrierIDReader", LogHeadType.Error, string.Format("GetCarrierID: read timeout on attempt {0}", attempt + 1));
                            result = ErrorCode.CarrierIdTimeout;
                            break;
                        }

                        if (readResult != ErrorCode.Success)
                        {
                            result = readResult;
                            break;
                        }

                        string normalized = TrimResponse(response);
                        if (string.Equals(normalized, "NG", StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.WriteLog("CarrierIDReader", LogHeadType.Error, string.Format("GetCarrierID: unreadable barcode on attempt {0}", attempt + 1));
                            result = ErrorCode.CarrierIdReadFailed;
                            continue;
                        }

                        if (string.Equals(normalized, "ERROR", StringComparison.OrdinalIgnoreCase))
                        {
                            result = ErrorCode.CarrierIdCommandFailed;
                            break;
                        }

                        if (!string.Equals(normalized, "OK", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(normalized))
                        {
                            carrierID = normalized;
                            result = ErrorCode.Success;
                            break;
                        }
                    }

                    ErrorCode motorOffResult = MotorOFF();
                    if (motorOffResult != ErrorCode.Success)
                    {
                        _logger.WriteLog("CarrierIDReader", LogHeadType.Error, "GetCarrierID: MOTOROFF failed");
                        return result == ErrorCode.Success ? ErrorCode.CarrierIdMotorOffFailed : result;
                    }

                    return result;
                }
                finally
                {
                    DisconnectReader();
                    ReleaseBusy();
                }
            }
            catch (Exception ex)
            {
                _logger.WriteLog("CarrierIDReader", LogHeadType.Exception, string.Format("GetCarrierID: exception - {0}", ex.Message));
                throw;
            }
        }

        protected override void OnDataReceived(byte[] byData, int length)
        {
            try
            {
                if (byData == null || length <= 0)
                {
                    return;
                }

                string response = Encoding.ASCII.GetString(byData, 0, length);
                string normalized = TrimResponse(response);

                if (!_waitingReadResult && string.Equals(normalized, "OK", StringComparison.OrdinalIgnoreCase))
                {
                    LastResponse = response;
                    _responseSignal.Set();
                    return;
                }

                if (_waitingReadResult)
                {
                    LastResponse = response;
                    _responseSignal.Set();
                }
            }
            catch (Exception ex)
            {
                _logger.WriteLog("CarrierIDReader", LogHeadType.Exception, string.Format("OnDataReceived: exception - {0}", ex.Message));
            }
        }
    }
}