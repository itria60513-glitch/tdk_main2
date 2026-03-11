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
    public class IDReaderOmronHexTests
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
                ReaderType = CarrierIDReaderType.OmronHex,
                TimeoutMs = 80,
                Page = 1,
            };

            _connectorMock.Setup(connector => connector.Connect()).Returns((HRESULT)null);
            _connectorMock.Setup(connector => connector.Disconnect());
        }

        [Test]
        public void GetCarrierID_WhenHexPayloadIsValid_ReturnsAsciiString()
        {
            var reader = new IDReaderOmronHex(_config, _connectorMock.Object, _loggerMock.Object);
            Mock<IConnector> connectorMock = _connectorMock;
            string sentCommand = null;
            _connectorMock.Setup(connector => connector.Send(It.IsAny<byte[]>(), It.IsAny<int>()))
                .Callback<byte[], int>((buffer, length) =>
                {
                    sentCommand = Encoding.ASCII.GetString(buffer, 0, length);
                    RaiseResponse(connectorMock, "004142434431323334\r");
                })
                .Returns((HRESULT)null);

            string carrierId;
            ErrorCode result = reader.GetCarrierID(out carrierId);

            Assert.AreEqual(ErrorCode.Success, result);
            Assert.AreEqual("ABCD1234", carrierId);
            Assert.AreEqual("010000000004\r", sentCommand);
        }

        [Test]
        public void GetCarrierID_WhenHexPayloadIsMalformed_ReturnsCarrierIdCommandFailed()
        {
            var reader = new IDReaderOmronHex(_config, _connectorMock.Object, _loggerMock.Object);
            Mock<IConnector> connectorMock = _connectorMock;
            _connectorMock.Setup(connector => connector.Send(It.IsAny<byte[]>(), It.IsAny<int>()))
                .Callback<byte[], int>((buffer, length) => RaiseResponse(connectorMock, "00ZZZ1\r"))
                .Returns((HRESULT)null);

            string carrierId;
            ErrorCode result = reader.GetCarrierID(out carrierId);

            Assert.AreEqual(ErrorCode.CarrierIdCommandFailed, result);
        }

        [Test]
        public void GetCarrierID_WhenTimeoutOccurs_ReturnsCarrierIdTimeout()
        {
            var reader = new IDReaderOmronHex(_config, _connectorMock.Object, _loggerMock.Object);
            _connectorMock.Setup(connector => connector.Send(It.IsAny<byte[]>(), It.IsAny<int>())).Returns((HRESULT)null);

            string carrierId;
            ErrorCode result = reader.GetCarrierID(out carrierId);

            Assert.AreEqual(ErrorCode.CarrierIdTimeout, result);
        }

        [Test]
        public void SetCarrierID_WhenHexPayloadIsValid_ReturnsSuccess()
        {
            var reader = new IDReaderOmronHex(_config, _connectorMock.Object, _loggerMock.Object);
            Mock<IConnector> connectorMock = _connectorMock;
            _connectorMock.Setup(connector => connector.Send(It.IsAny<byte[]>(), It.IsAny<int>()))
                .Callback<byte[], int>((buffer, length) => RaiseResponse(connectorMock, "00\r"))
                .Returns((HRESULT)null);

            ErrorCode result = reader.SetCarrierID("4142434445463031");

            Assert.AreEqual(ErrorCode.Success, result);
        }

        [Test]
        public void SetCarrierID_WhenHexPayloadHasInvalidCharacter_ReturnsCarrierIdInvalidParameter()
        {
            var reader = new IDReaderOmronHex(_config, _connectorMock.Object, _loggerMock.Object);

            ErrorCode result = reader.SetCarrierID("41424344454630ZZ");

            Assert.AreEqual(ErrorCode.CarrierIdInvalidParameter, result);
        }

        [Test]
        public void GetCarrierID_WhenHexDecodesToNonPrintableBytes_ReturnsSuccess()
        {
            var reader = new IDReaderOmronHex(_config, _connectorMock.Object, _loggerMock.Object);
            Mock<IConnector> connectorMock = _connectorMock;
            _connectorMock.Setup(connector => connector.Send(It.IsAny<byte[]>(), It.IsAny<int>()))
                .Callback<byte[], int>((buffer, length) => RaiseResponse(connectorMock, "000102030405060708\r"))
                .Returns((HRESULT)null);

            string carrierId;
            ErrorCode result = reader.GetCarrierID(out carrierId);

            Assert.AreEqual(ErrorCode.Success, result);
            Assert.AreEqual(8, carrierId.Length);
        }

        private static void RaiseResponse(Mock<IConnector> connectorMock, string response)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(response);
            connectorMock.Raise(connector => connector.DataReceived += null, bytes, bytes.Length);
        }
    }
}