using System;
using System.Text;
using Communication.Interface;
using TDKLogUtility.Module;

namespace TDKController
{
    /// <summary>
    /// Implements the Omron ASCII RFID workflow.
    /// </summary>
    public class IDReaderOmronASCII : CarrierIDReader
    {
        public IDReaderOmronASCII(CarrierIDReaderConfig config, IConnector connector, ILogUtility logger)
            : base(config, connector, logger)
        {
        }

        /// <inheritdoc />
        public override CarrierIDReaderType CarrierIDReaderType
        {
            get { return CarrierIDReaderType.OmronASCII; }
        }

        /// <inheritdoc />
        public override ErrorCode ParseCarrierIDReaderData(string command)
        {
            try
            {
                string normalized = TrimResponse(command);
                return normalized.StartsWith("00", StringComparison.Ordinal)
                    ? ErrorCode.Success
                    : ErrorCode.CarrierIdCommandFailed;
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
                    ErrorCode validationResult = ValidatePage(30);
                    if (validationResult != ErrorCode.Success)
                    {
                        _logger.WriteLog("CarrierIDReader", LogHeadType.Error, string.Format("GetCarrierID: invalid Omron ASCII page {0}", Config.Page));
                        return validationResult;
                    }

                    ErrorCode connectResult = ConnectReader();
                    if (connectResult != ErrorCode.Success)
                    {
                        return connectResult;
                    }

                    string response;
                    ErrorCode result = SendCommand(BuildReadCommand(Config.Page), Config.TimeoutMs, out response);
                    if (result != ErrorCode.Success)
                    {
                        return result;
                    }

                    string payload = ExtractPayload(response);
                    if (!IsPrintableAscii(payload))
                    {
                        _logger.WriteLog("CarrierIDReader", LogHeadType.Error, string.Format("GetCarrierID: malformed Omron ASCII payload {0}", TrimResponse(response)));
                        return ErrorCode.CarrierIdCommandFailed;
                    }

                    carrierID = payload;
                    return ErrorCode.Success;
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

        /// <inheritdoc />
        public override ErrorCode SetCarrierID(string carrierID)
        {
            try
            {
                ErrorCode busyResult = AcquireBusy();
                if (busyResult != ErrorCode.Success)
                {
                    return busyResult;
                }

                try
                {
                    ErrorCode validationResult = ValidateAsciiWrite(carrierID);
                    if (validationResult != ErrorCode.Success)
                    {
                        _logger.WriteLog("CarrierIDReader", LogHeadType.Error, string.Format("SetCarrierID: invalid Omron ASCII payload for page {0}", Config.Page));
                        return validationResult;
                    }

                    ErrorCode connectResult = ConnectReader();
                    if (connectResult != ErrorCode.Success)
                    {
                        return connectResult;
                    }

                    string response;
                    return SendCommand(BuildWriteCommand(Config.Page, carrierID), Config.TimeoutMs, out response);
                }
                finally
                {
                    DisconnectReader();
                    ReleaseBusy();
                }
            }
            catch (Exception ex)
            {
                _logger.WriteLog("CarrierIDReader", LogHeadType.Exception, string.Format("SetCarrierID: exception - {0}", ex.Message));
                throw;
            }
        }

        protected string ExtractPayload(string response)
        {
            string normalized = TrimResponse(response);
            return normalized.Length <= 2 ? string.Empty : normalized.Substring(2);
        }

        protected string BuildReadCommand(int page)
        {
            return string.Format("0110{0}\r", BuildPageMask(page));
        }

        protected string BuildWriteCommand(int page, string payload)
        {
            return string.Format("0210{0}{1}\r", BuildPageMask(page), payload);
        }

        protected string BuildPageMask(int page)
        {
            long mask = 1L << (page - 1);
            return mask.ToString("X8");
        }

        protected ErrorCode ValidatePage(int maxPage)
        {
            return Config.Page >= 1 && Config.Page <= maxPage
                ? ErrorCode.Success
                : ErrorCode.CarrierIdInvalidPage;
        }

        protected ErrorCode ValidateAsciiWrite(string carrierID)
        {
            ErrorCode pageResult = ValidatePage(30);
            if (pageResult != ErrorCode.Success)
            {
                return pageResult;
            }

            if (string.IsNullOrEmpty(carrierID) || carrierID.Length != 16 || !IsPrintableAscii(carrierID))
            {
                return ErrorCode.CarrierIdInvalidParameter;
            }

            return ErrorCode.Success;
        }
    }
}