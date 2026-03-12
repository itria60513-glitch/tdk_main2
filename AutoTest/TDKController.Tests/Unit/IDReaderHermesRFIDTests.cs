using System;
using System.Text;
using Communication.Connector.Enum;
using Communication.Interface;
using Moq;
using NUnit.Framework;
using TDKLogUtility.Module;

namespace TDKController.Tests.Unit
{
    [TestFixture]
    public class IDReaderHermesRFIDTests
    {
        private Mock<IConnector> _connectorMock;
        private Mock<ILogUtility> _loggerMock;
        private CarrierIDReaderConfig _config;

        [SetUp]
        public void SetUp()
        {
            _connectorMock = new Mock<IConnector>();
            _loggerMock = new Mock<ILogUtility>();
            _config = new CarrierIDReaderConfig
            {
                TimeoutMs = 80,
                Page = 1,
            };

            _connectorMock.Setup(connector => connector.Connect()).Returns((HRESULT)null);
            _connectorMock.Setup(connector => connector.Disconnect());
        }

        [Test]
        public void GetCarrierID_WhenHermesResponseIsValid_ReturnsCarrierId()
        {
            var reader = new IDReaderHermesRFID(_config, _connectorMock.Object, _loggerMock.Object);
            Mock<IConnector> connectorMock = _connectorMock;
            _connectorMock.Setup(connector => connector.Send(It.IsAny<byte[]>(), It.IsAny<int>()))
                .Callback<byte[], int>((buffer, length) => RaiseResponse(connectorMock, BuildFrame("x0014142434445463031")))
                .Returns((HRESULT)null);

            string carrierId;
            ErrorCode result = reader.GetCarrierID(out carrierId);

            Assert.AreEqual(ErrorCode.Success, result);
            Assert.AreEqual("4142434445463031", carrierId);
        }

        [Test]
        public void GetCarrierID_WhenChecksumFails_ReturnsCarrierIdChecksumError()
        {
            var reader = new IDReaderHermesRFID(_config, _connectorMock.Object, _loggerMock.Object);
            Mock<IConnector> connectorMock = _connectorMock;
            _connectorMock.Setup(connector => connector.Send(It.IsAny<byte[]>(), It.IsAny<int>()))
                .Callback<byte[], int>((buffer, length) => RaiseResponse(connectorMock, CorruptChecksum(BuildFrame("x0014142434445463031"))))
                .Returns((HRESULT)null);

            string carrierId;
            ErrorCode result = reader.GetCarrierID(out carrierId);

            Assert.AreEqual(ErrorCode.CarrierIdChecksumError, result);
        }

        [Test]
        public void GetCarrierID_WhenPageIsInvalid_ReturnsCarrierIdInvalidPage()
        {
            _config.Page = 18;
            var reader = new IDReaderHermesRFID(_config, _connectorMock.Object, _loggerMock.Object);

            string carrierId;
            ErrorCode result = reader.GetCarrierID(out carrierId);

            Assert.AreEqual(ErrorCode.CarrierIdInvalidPage, result);
        }

        [Test]
        public void GetCarrierID_WhenTimeoutOccurs_ReturnsCarrierIdTimeout()
        {
            var reader = new IDReaderHermesRFID(_config, _connectorMock.Object, _loggerMock.Object);
            _connectorMock.Setup(connector => connector.Send(It.IsAny<byte[]>(), It.IsAny<int>())).Returns((HRESULT)null);

            string carrierId;
            ErrorCode result = reader.GetCarrierID(out carrierId);

            Assert.AreEqual(ErrorCode.CarrierIdTimeout, result);
        }

        [Test]
        public void SetCarrierID_WhenPayloadIsValid_ReturnsSuccess()
        {
            var reader = new IDReaderHermesRFID(_config, _connectorMock.Object, _loggerMock.Object);
            Mock<IConnector> connectorMock = _connectorMock;
            _connectorMock.Setup(connector => connector.Send(It.IsAny<byte[]>(), It.IsAny<int>()))
                .Callback<byte[], int>((buffer, length) => RaiseResponse(connectorMock, BuildFrame("w0")))
                .Returns((HRESULT)null);

            ErrorCode result = reader.SetCarrierID("4142434445463031");

            Assert.AreEqual(ErrorCode.Success, result);
        }

        [Test]
        public void SetCarrierID_WhenPayloadIsInvalid_ReturnsCarrierIdInvalidParameter()
        {
            var reader = new IDReaderHermesRFID(_config, _connectorMock.Object, _loggerMock.Object);

            ErrorCode result = reader.SetCarrierID("INVALID-PAYLOAD");

            Assert.AreEqual(ErrorCode.CarrierIdInvalidParameter, result);
        }

        [Test]
        public void SetCarrierID_WhenDeviceReturnsUnsupportedResponse_ReturnsCarrierIdCommandFailed()
        {
            var reader = new IDReaderHermesRFID(_config, _connectorMock.Object, _loggerMock.Object);
            Mock<IConnector> connectorMock = _connectorMock;
            _connectorMock.Setup(connector => connector.Send(It.IsAny<byte[]>(), It.IsAny<int>()))
                .Callback<byte[], int>((buffer, length) => RaiseResponse(connectorMock, BuildFrame("z0")))
                .Returns((HRESULT)null);

            ErrorCode result = reader.SetCarrierID("4142434445463031");

            Assert.AreEqual(ErrorCode.CarrierIdCommandFailed, result);
        }

        private static void RaiseResponse(Mock<IConnector> connectorMock, string response)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(response);
            connectorMock.Raise(connector => connector.DataReceived += null, bytes, bytes.Length);
        }

        private static string BuildFrame(string message)
        {
            string body = string.Concat("S", message.Length.ToString("X2"), message, "\r");
            int xorValue = 0;
            int addValue = 0;

            for (int index = 0; index < body.Length; index++)
            {
                xorValue ^= body[index];
                addValue = (addValue + body[index]) & 0xFF;
            }

            return body + string.Format("{0:X2}{1:X2}", xorValue & 0xFF, addValue & 0xFF);
        }

        private static string CorruptChecksum(string frame)
        {
            return frame.Substring(0, frame.Length - 4) + "0000";
        }
    }
}