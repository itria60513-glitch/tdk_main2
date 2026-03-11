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
        private enum BarcodeReadState
        {
            Invalid = 0,
            Success = 1,
            Retry = 2,
            Fail = 3,
        }

        private const int MotorOnTimeoutMs = 10000;
        private const int MotorOffTimeoutMs = 3000;
        private const int ReadTimeoutMs = 5000;
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
                return SendAckCommand(CommandMotorOn, MotorOnTimeoutMs, ErrorCode.CarrierIdMotorOnFailed);
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
                return ReadRawBarcode(out barcode);
            }
            catch (Exception ex)
            {
                _logger.WriteLog("CarrierIDReader", LogHeadType.Exception, string.Format("ReadBarCode: exception - {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Sends the BL600 MOTOROFF command and waits for the OK acknowledgement.
        /// </summary>
        public ErrorCode MotorOFF()
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

                    ErrorCode result = TryReadCarrierId(out carrierID);

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

        private ErrorCode SendAckCommand(string command, int timeoutMs, ErrorCode failureCode)
        {
            string response;
            ErrorCode result = SendCommand(command, timeoutMs, out response);
            if (result != ErrorCode.Success)
            {
                return failureCode;
            }

            return IsOkResponse(response) ? ErrorCode.Success : failureCode;
        }

        private ErrorCode ReadRawBarcode(out string barcode)
        {
            barcode = string.Empty;

            string response;
            ErrorCode result = SendCommand(CommandRead, ReadTimeoutMs, out response);
            if (result == ErrorCode.CarrierIdTimeout)
            {
                StopReadTrigger();
                return result;
            }

            if (result != ErrorCode.Success)
            {
                return result;
            }

            barcode = TrimResponse(response);
            return ErrorCode.Success;
        }

        private ErrorCode TryReadCarrierId(out string carrierID)
        {
            carrierID = string.Empty;
            ErrorCode lastResult = ErrorCode.CarrierIdReadFailed;

            for (int attempt = 0; attempt < Config.MaxRetryCount; attempt++)
            {
                string barcode;
                ErrorCode readResult = ReadBarCode(out barcode);
                if (readResult == ErrorCode.CarrierIdTimeout)
                {
                    _logger.WriteLog("CarrierIDReader", LogHeadType.Error, string.Format("GetCarrierID: read timeout on attempt {0}", attempt + 1));
                    return ErrorCode.CarrierIdTimeout;
                }

                if (readResult != ErrorCode.Success)
                {
                    return readResult;
                }

                BarcodeReadState state = ClassifyReadState(barcode);
                if (state == BarcodeReadState.Success)
                {
                    carrierID = TrimResponse(barcode);
                    return ErrorCode.Success;
                }

                if (state == BarcodeReadState.Retry)
                {
                    _logger.WriteLog("CarrierIDReader", LogHeadType.Error, string.Format("GetCarrierID: unreadable barcode on attempt {0}", attempt + 1));
                    lastResult = ErrorCode.CarrierIdReadFailed;
                    continue;
                }

                if (state == BarcodeReadState.Fail)
                {
                    return ErrorCode.CarrierIdCommandFailed;
                }

                return ErrorCode.CarrierIdCommandFailed;
            }

            return lastResult;
        }

        private void StopReadTrigger()
        {
            string stopResponse;
            SendCommand(CommandStop, 500, out stopResponse);
        }

        private static BarcodeReadState ClassifyReadState(string response)
        {
            string normalized = TrimResponse(response);
            if (string.IsNullOrEmpty(normalized) || string.Equals(normalized, "OK", StringComparison.OrdinalIgnoreCase))
            {
                return BarcodeReadState.Invalid;
            }

            if (string.Equals(normalized, "NG", StringComparison.OrdinalIgnoreCase))
            {
                return BarcodeReadState.Retry;
            }

            if (string.Equals(normalized, "ERROR", StringComparison.OrdinalIgnoreCase))
            {
                return BarcodeReadState.Fail;
            }

            return IsPrintableAscii(normalized) ? BarcodeReadState.Success : BarcodeReadState.Invalid;
        }

        private static bool IsOkResponse(string response)
        {
            return string.Equals(TrimResponse(response), "OK", StringComparison.OrdinalIgnoreCase);
        }

    }
}