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
        private const int MaxPage = 17;
        private HermesFrame? _lastParsedFrame;

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
                _lastParsedFrame = null;

                HermesFrame frame;
                ErrorCode parseResult = TryParseFrame(command, out frame);
                if (parseResult != ErrorCode.Success)
                {
                    return parseResult;
                }

                if (!IsSupportedResponse(frame.Message))
                {
                    return ErrorCode.CarrierIdCommandFailed;
                }

                _lastParsedFrame = frame;
                return ErrorCode.Success;
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
            try
            {
                return ExecuteRead(ValidateReadRequest, string.Format("GetCarrierID: invalid Hermes page {0}", Config.Page), TryReadCarrierId, out carrierID);
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
                return ExecuteWrite(carrierID, ValidateWriteRequest, string.Format("SetCarrierID: invalid Hermes payload for page {0}", Config.Page), WriteCarrierId);
            }
            catch (Exception ex)
            {
                _logger.WriteLog("CarrierIDReader", LogHeadType.Exception, string.Format("SetCarrierID: exception - {0}", ex.Message));
                throw;
            }
        }

        private ErrorCode ValidatePage()
        {
            return Config.Page >= 1 && Config.Page <= MaxPage
                ? ErrorCode.Success
                : ErrorCode.CarrierIdInvalidPage;
        }

        private ErrorCode ValidateReadRequest()
        {
            return ValidatePage();
        }

        private ErrorCode ValidateWriteRequest(string carrierID)
        {
            return ValidateWritePayload(carrierID);
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

        private ErrorCode TryReadCarrierId(out string carrierID)
        {
            carrierID = string.Empty;

            string response;
            ErrorCode result = SendCommand(BuildReadCommand(), Config.TimeoutMs, out response);
            if (result != ErrorCode.Success)
            {
                return result;
            }

            if (!_lastParsedFrame.HasValue || !IsReadResponse(_lastParsedFrame.Value.Message))
            {
                return ErrorCode.CarrierIdCommandFailed;
            }

            string info = _lastParsedFrame.Value.Message.Substring(2);
            carrierID = info.Length > 2 ? info.Substring(2) : string.Empty;
            return string.IsNullOrEmpty(carrierID) ? ErrorCode.CarrierIdCommandFailed : ErrorCode.Success;
        }

        private ErrorCode WriteCarrierId(string carrierID)
        {
            string response;
            return SendCommand(BuildWriteCommand(carrierID), Config.TimeoutMs, out response);
        }

        private string BuildReadCommand()
        {
            return PrepareCommand(string.Format("X0{0:D2}", Config.Page));
        }

        private string BuildWriteCommand(string carrierID)
        {
            return PrepareCommand(string.Format("W0{0:D2}{1}", Config.Page, carrierID.ToUpperInvariant()));
        }

        private string PrepareCommand(string message)
        {
            string length = message.Length.ToString("X2");
            string frameWithoutChecksums = string.Concat("S", length, message, "\r");
            return frameWithoutChecksums + ComputeChecksums(frameWithoutChecksums);
        }

        private ErrorCode TryParseFrame(string response, out HermesFrame frame)
        {
            frame = default(HermesFrame);
            string normalized = TrimResponse(response);
            if (string.IsNullOrEmpty(normalized) || normalized.Length < 10 || normalized[0] != 'S')
            {
                return ErrorCode.CarrierIdCommandFailed;
            }

            int messageLength = Convert.ToInt32(normalized.Substring(1, 2), 16);
            int frameLength = messageLength + 8;
            if (normalized.Length != frameLength)
            {
                return ErrorCode.CarrierIdCommandFailed;
            }

            int endIndex = frameLength - 5;
            if (normalized[endIndex] != '\r')
            {
                return ErrorCode.CarrierIdCommandFailed;
            }

            string message = normalized.Substring(3, messageLength);
            if (message.Length < 2 || message[1] != '0')
            {
                return ErrorCode.CarrierIdCommandFailed;
            }

            string checksum = normalized.Substring(frameLength - 4, 4);
            string expected = ComputeChecksums(normalized.Substring(0, frameLength - 4));
            if (!string.Equals(checksum, expected, StringComparison.OrdinalIgnoreCase))
            {
                return ErrorCode.CarrierIdChecksumError;
            }

            frame = new HermesFrame(message);
            return ErrorCode.Success;
        }

        private static bool IsSupportedResponse(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return false;
            }

            char responseType = message[0];
            return responseType == 'x' || responseType == 'w' || responseType == 'v';
        }

        private static bool IsReadResponse(string message)
        {
            return !string.IsNullOrEmpty(message) && message[0] == 'x';
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

        private struct HermesFrame
        {
            public HermesFrame(string message)
            {
                Message = message;
            }

            public string Message { get; }
        }
    }
}