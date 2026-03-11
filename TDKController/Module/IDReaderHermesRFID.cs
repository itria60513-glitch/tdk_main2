using System;
using Communication.Interface;
using TDKLogUtility.Module;

namespace TDKController
{
    /// <summary>
    /// Implements the Hermes RFID workflow.
    /// </summary>
    public class IDReaderHermesRFID : CarrierIDReader
    {
        public IDReaderHermesRFID(CarrierIDReaderConfig config, IConnector connector, ILogUtility logger)
            : base(config, connector, logger)
        {
        }

        /// <inheritdoc />
        public override CarrierIDReaderType CarrierIDReaderType
        {
            get { return CarrierIDReaderType.HermesRFID; }
        }

        /// <inheritdoc />
        public override ErrorCode ParseCarrierIDReaderData(string command)
        {
            try
            {
                string normalized = TrimResponse(command);
                if (string.IsNullOrEmpty(normalized) || normalized.Length < 8 || normalized[0] != 'S')
                {
                    return ErrorCode.CarrierIdCommandFailed;
                }

                int messageLength = Convert.ToInt32(normalized.Substring(1, 2), 16);
                if (normalized.Length < messageLength + 8)
                {
                    return ErrorCode.CarrierIdCommandFailed;
                }

                int checksumStart = 3 + messageLength + 1;
                string message = normalized.Substring(3, messageLength);
                string checksum = normalized.Substring(checksumStart, 4);
                string expected = ComputeChecksums(normalized.Substring(0, checksumStart));

                if (!string.Equals(checksum, expected, StringComparison.OrdinalIgnoreCase))
                {
                    return ErrorCode.CarrierIdChecksumError;
                }

                char responseType = message[0];
                return responseType == 'x' || responseType == 'w' || responseType == 'v'
                    ? ErrorCode.Success
                    : ErrorCode.CarrierIdCommandFailed;
            }
            catch (FormatException)
            {
                return ErrorCode.CarrierIdCommandFailed;
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
                    ErrorCode validationResult = ValidatePage();
                    if (validationResult != ErrorCode.Success)
                    {
                        _logger.WriteLog("CarrierIDReader", LogHeadType.Error, string.Format("GetCarrierID: invalid Hermes page {0}", Config.Page));
                        return validationResult;
                    }

                    ErrorCode connectResult = ConnectReader();
                    if (connectResult != ErrorCode.Success)
                    {
                        return connectResult;
                    }

                    string response;
                    ErrorCode result = SendCommand(PrepareCommand(string.Format("X0{0:D2}", Config.Page)), Config.TimeoutMs, out response);
                    if (result != ErrorCode.Success)
                    {
                        return result;
                    }

                    string message = ExtractMessage(response);
                    carrierID = message.Length > 1 ? message.Substring(1) : string.Empty;
                    return string.IsNullOrEmpty(carrierID) ? ErrorCode.CarrierIdCommandFailed : ErrorCode.Success;
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
                    ErrorCode validationResult = ValidateWritePayload(carrierID);
                    if (validationResult != ErrorCode.Success)
                    {
                        _logger.WriteLog("CarrierIDReader", LogHeadType.Error, string.Format("SetCarrierID: invalid Hermes payload for page {0}", Config.Page));
                        return validationResult;
                    }

                    ErrorCode connectResult = ConnectReader();
                    if (connectResult != ErrorCode.Success)
                    {
                        return connectResult;
                    }

                    string response;
                    return SendCommand(PrepareCommand(string.Format("W0{0:D2}{1}", Config.Page, carrierID.ToUpperInvariant())), Config.TimeoutMs, out response);
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

        private ErrorCode ValidatePage()
        {
            return Config.Page >= 1 && Config.Page <= 17
                ? ErrorCode.Success
                : ErrorCode.CarrierIdInvalidPage;
        }

        private ErrorCode ValidateWritePayload(string carrierID)
        {
            ErrorCode pageResult = ValidatePage();
            if (pageResult != ErrorCode.Success)
            {
                return pageResult;
            }

            if (string.IsNullOrEmpty(carrierID) || carrierID.Length != 16 || !IsHexString(carrierID))
            {
                return ErrorCode.CarrierIdInvalidParameter;
            }

            return ErrorCode.Success;
        }

        private string PrepareCommand(string message)
        {
            string length = message.Length.ToString("X2");
            string frameWithoutChecksums = string.Concat("S", length, message, "\r");
            return frameWithoutChecksums + ComputeChecksums(frameWithoutChecksums);
        }

        private string ExtractMessage(string response)
        {
            string normalized = TrimResponse(response);
            int length = Convert.ToInt32(normalized.Substring(1, 2), 16);
            return normalized.Substring(3, length);
        }

        private static string ComputeChecksums(string value)
        {
            int xorValue = 0;
            int addValue = 0;
            for (int index = 0; index < value.Length; index++)
            {
                xorValue ^= value[index];
                addValue = (addValue + value[index]) & 0xFF;
            }

            return string.Format("{0:X2}{1:X2}", xorValue & 0xFF, addValue & 0xFF);
        }
    }
}