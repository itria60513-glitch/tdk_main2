using System;
using System.Text;
using Communication.Interface;
using TDKLogUtility.Module;

namespace TDKController
{
    /// <summary>
    /// Implements the Omron HEX RFID workflow.
    /// </summary>
    public class IDReaderOmronHex : IDReaderOmronASCII
    {
        private const int MaxPage = 30;

        public IDReaderOmronHex(CarrierIDReaderConfig config, IConnector connector, ILogUtility logger)
            : base(config, connector, logger)
        {
        }

        /// <inheritdoc />
        public override CarrierIDReaderType CarrierIDReaderType
        {
            get { return CarrierIDReaderType.OmronHex; }
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
                        _logger.WriteLog("CarrierIDReader", LogHeadType.Error, string.Format("GetCarrierID: invalid Omron HEX page {0}", Config.Page));
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
                        _logger.WriteLog("CarrierIDReader", LogHeadType.Error, string.Format("SetCarrierID: invalid Omron HEX payload for page {0}", Config.Page));
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

        protected new ErrorCode TryReadCarrierId(out string carrierID)
        {
            carrierID = string.Empty;

            string response;
            ErrorCode result = SendCommand(BuildReadCommand(Config.Page), Config.TimeoutMs, out response);
            if (result != ErrorCode.Success)
            {
                return result;
            }

            string payload = ExtractPayload(response);
            if (!IsValidHexPayload(payload))
            {
                _logger.WriteLog("CarrierIDReader", LogHeadType.Error, string.Format("GetCarrierID: malformed Omron HEX payload {0}", TrimResponse(response)));
                return ErrorCode.CarrierIdCommandFailed;
            }

            carrierID = HexToAscii(payload);
            return ErrorCode.Success;
        }

        protected new ErrorCode WriteCarrierId(string carrierID)
        {
            string response;
            return SendCommand(BuildWriteCommand(Config.Page, carrierID), Config.TimeoutMs, out response);
        }

        protected new string BuildReadCommand(int page)
        {
            return string.Format("0100{0}\r", BuildPageMask(page));
        }

        protected new string BuildWriteCommand(int page, string payload)
        {
            return string.Format("0200{0}{1}\r", BuildPageMask(page), payload.ToUpperInvariant());
        }

        protected new ErrorCode ValidateReadRequest()
        {
            return ValidatePage(MaxPage);
        }

        protected new ErrorCode ValidateWriteRequest(string carrierID)
        {
            ErrorCode pageResult = ValidatePage(MaxPage);
            if (pageResult != ErrorCode.Success)
            {
                return pageResult;
            }

            if (!IsValidHexWritePayload(carrierID))
            {
                return ErrorCode.CarrierIdInvalidParameter;
            }

            return ErrorCode.Success;
        }

        private static bool IsValidHexPayload(string payload)
        {
            return IsHexString(payload) && payload.Length % 2 == 0;
        }

        private static bool IsValidHexWritePayload(string carrierID)
        {
            return !string.IsNullOrEmpty(carrierID) && carrierID.Length == 16 && IsHexString(carrierID);
        }

        private static string HexToAscii(string payload)
        {
            byte[] bytes = new byte[payload.Length / 2];
            for (int index = 0; index < bytes.Length; index++)
            {
                bytes[index] = Convert.ToByte(payload.Substring(index * 2, 2), 16);
            }

            return Encoding.ASCII.GetString(bytes);
        }
    }
}