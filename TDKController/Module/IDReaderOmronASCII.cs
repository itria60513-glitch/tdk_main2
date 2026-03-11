using System;
using Communication.Interface;
using TDKLogUtility.Module;

namespace TDKController
{
    /// <summary>
    /// Implements the Omron ASCII RFID workflow.
    /// </summary>
    public class IDReaderOmronASCII : CarrierIDReader
    {
        private const int MaxPage = 30;

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
                    ErrorCode validationResult = ValidateReadRequest();
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

                    return TryReadCarrierId(out carrierID);
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
                    ErrorCode validationResult = ValidateWriteRequest(carrierID);
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

                    return WriteCarrierId(carrierID);
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

        protected ErrorCode TryReadCarrierId(out string carrierID)
        {
            carrierID = string.Empty;

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

        protected ErrorCode WriteCarrierId(string carrierID)
        {
            string response;
            return SendCommand(BuildWriteCommand(Config.Page, carrierID), Config.TimeoutMs, out response);
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
            char[] frame = new[] { '0', '0', '0', '0', '0', '0', '0', '0' };
            bool isAscii = CarrierIDReaderType == CarrierIDReaderType.OmronASCII;
            int offset;

            switch (page)
            {
                case 1:
                    frame[7] = isAscii ? 'C' : '4';
                    break;
                case 2:
                    frame[6] = isAscii ? '1' : '0';
                    frame[7] = '8';
                    break;
                case 3:
                case 7:
                case 11:
                case 15:
                case 19:
                case 23:
                case 27:
                    offset = page / 4;
                    frame[6 - offset] = isAscii ? '3' : '1';
                    break;
                case 4:
                case 8:
                case 12:
                case 16:
                case 20:
                case 24:
                case 28:
                    offset = (page - 1) / 4;
                    frame[6 - offset] = isAscii ? '6' : '2';
                    break;
                case 5:
                case 9:
                case 13:
                case 17:
                case 21:
                case 25:
                case 29:
                    offset = (page - 2) / 4;
                    frame[6 - offset] = isAscii ? 'C' : '4';
                    break;
                case 6:
                case 10:
                case 14:
                case 18:
                case 22:
                case 26:
                case 30:
                    offset = (page - 3) / 4;
                    if (isAscii)
                    {
                        if (page < 30)
                        {
                            frame[5 - offset] = '1';
                        }

                        frame[6 - offset] = '8';
                    }
                    else
                    {
                        frame[6 - offset] = '8';
                    }

                    break;
                default:
                    frame[7] = isAscii ? 'C' : '4';
                    break;
            }

            return new string(frame);
        }

        protected ErrorCode ValidatePage(int maxPage)
        {
            return Config.Page >= 1 && Config.Page <= maxPage
                ? ErrorCode.Success
                : ErrorCode.CarrierIdInvalidPage;
        }

        protected ErrorCode ValidateReadRequest()
        {
            return ValidatePage(MaxPage);
        }

        protected ErrorCode ValidateWriteRequest(string carrierID)
        {
            return ValidateAsciiWrite(carrierID);
        }

        protected ErrorCode ValidateAsciiWrite(string carrierID)
        {
            ErrorCode pageResult = ValidatePage(MaxPage);
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