using System;
using Communication.Interface;
using TDKLogUtility.Module;

namespace TDKController
{
    /// <summary>
    /// Implements the BL600 barcode reader workflow.
    /// </summary>
    public class IDReaderBarcodeReader : CarrierIDReader
    {
        private const string CommandMotorOn = "MOTORON\r";
        private const string CommandMotorOff = "MOTOROFF\r";
        private const string CommandRead = "LON\r";
        private const string CommandStop = "LOFF\r";

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

                return IsPrintableAscii(normalized) ? ErrorCode.Success : ErrorCode.CarrierIdCommandFailed;
            }
            catch (Exception ex)
            {
                _logger.WriteLog("CarrierIDReader", LogHeadType.Exception, string.Format("ParseCarrierIDReaderData: exception - {0}", ex.Message));
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

                    string response;
                    ErrorCode motorOnResult = SendCommand(CommandMotorOn, Config.TimeoutMs, out response);
                    if (motorOnResult != ErrorCode.Success || !string.Equals(TrimResponse(response), "OK", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.WriteLog("CarrierIDReader", LogHeadType.Error, "GetCarrierID: MOTORON failed");
                        return ErrorCode.CarrierIdMotorOnFailed;
                    }

                    ErrorCode result = ErrorCode.CarrierIdReadFailed;

                    for (int attempt = 0; attempt < Config.MaxRetryCount; attempt++)
                    {
                        ErrorCode readResult = SendCommand(CommandRead, Config.TimeoutMs, out response);
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

                        if (!string.Equals(normalized, "OK", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(normalized))
                        {
                            carrierID = normalized;
                            result = ErrorCode.Success;
                            break;
                        }
                    }

                    string cleanupResponse;
                    SendCommand(CommandStop, 500, out cleanupResponse);
                    ErrorCode motorOffResult = SendCommand(CommandMotorOff, Config.TimeoutMs, out cleanupResponse);
                    if (motorOffResult != ErrorCode.Success || !string.Equals(TrimResponse(cleanupResponse), "OK", StringComparison.OrdinalIgnoreCase))
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
    }
}