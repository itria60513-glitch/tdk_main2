using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Communication.Connector.Enum;
using Communication.Interface;
using Moq;
using NUnit.Framework;
using TDKLogUtility.Module;

namespace TDKController.Tests.Unit
{
    [TestFixture]
    public class IDReaderOmronASCIITests
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
                ReaderType = CarrierIDReaderType.OmronASCII,
                TimeoutMs = 80,
                Page = 1,
            };

            _connectorMock.Setup(connector => connector.Connect()).Returns((HRESULT)null);
            _connectorMock.Setup(connector => connector.Disconnect());
        }

        [Test]
        public void GetCarrierID_WhenReadSucceeds_ReturnsAsciiPayload()
        {
            var reader = new IDReaderOmronASCII(_config, _connectorMock.Object, _loggerMock.Object);
            Mock<IConnector> connectorMock = _connectorMock;
            string sentCommand = null;

            _connectorMock.Setup(connector => connector.Send(It.IsAny<byte[]>(), It.IsAny<int>()))
                .Callback<byte[], int>((buffer, length) =>
                {
                    sentCommand = Encoding.ASCII.GetString(buffer, 0, length);
                    RaiseResponse(connectorMock, "00LOT123456789012\r");
                })
                .Returns((HRESULT)null);

            string carrierId;
            ErrorCode result = reader.GetCarrierID(out carrierId);

            Assert.AreEqual(ErrorCode.Success, result);
            Assert.AreEqual("LOT123456789012", carrierId);
            Assert.AreEqual("01100000000C\r", sentCommand);
        }

        [Test]
        public void GetCarrierID_WhenAsciiPageIsSix_UsesLegacyDualPageMask()
        {
            _config.Page = 6;
            var reader = new IDReaderOmronASCII(_config, _connectorMock.Object, _loggerMock.Object);
            Mock<IConnector> connectorMock = _connectorMock;
            string sentCommand = null;

            _connectorMock.Setup(connector => connector.Send(It.IsAny<byte[]>(), It.IsAny<int>()))
                .Callback<byte[], int>((buffer, length) =>
                {
                    sentCommand = Encoding.ASCII.GetString(buffer, 0, length);
                    RaiseResponse(connectorMock, "00LOT123456789012\r");
                })
                .Returns((HRESULT)null);

            string carrierId;
            ErrorCode result = reader.GetCarrierID(out carrierId);

            Assert.AreEqual(ErrorCode.Success, result);
            Assert.AreEqual("011000000180\r", sentCommand);
        }

        [Test]
        public void GetCarrierID_WhenPageIsInvalid_ReturnsCarrierIdInvalidPage()
        {
            _config.Page = 31;
            var reader = new IDReaderOmronASCII(_config, _connectorMock.Object, _loggerMock.Object);

            string carrierId;
            ErrorCode result = reader.GetCarrierID(out carrierId);

            Assert.AreEqual(ErrorCode.CarrierIdInvalidPage, result);
        }

        [Test]
        public void GetCarrierID_WhenPayloadContainsControlCharacter_ReturnsCarrierIdCommandFailed()
        {
            var reader = new IDReaderOmronASCII(_config, _connectorMock.Object, _loggerMock.Object);
            Mock<IConnector> connectorMock = _connectorMock;

            _connectorMock.Setup(connector => connector.Send(It.IsAny<byte[]>(), It.IsAny<int>()))
                .Callback<byte[], int>((buffer, length) => RaiseResponse(connectorMock, "00ABC\u0001DEF\r"))
                .Returns((HRESULT)null);

            string carrierId;
            ErrorCode result = reader.GetCarrierID(out carrierId);

            Assert.AreEqual(ErrorCode.CarrierIdCommandFailed, result);
        }

        [Test]
        public void GetCarrierID_WhenTimeoutOccurs_ReturnsCarrierIdTimeout()
        {
            var reader = new IDReaderOmronASCII(_config, _connectorMock.Object, _loggerMock.Object);
            _connectorMock.Setup(connector => connector.Send(It.IsAny<byte[]>(), It.IsAny<int>())).Returns((HRESULT)null);

            string carrierId;
            ErrorCode result = reader.GetCarrierID(out carrierId);

            Assert.AreEqual(ErrorCode.CarrierIdTimeout, result);
        }

        [Test]
        public void SetCarrierID_WhenWritePayloadIsValid_ReturnsSuccess()
        {
            var reader = new IDReaderOmronASCII(_config, _connectorMock.Object, _loggerMock.Object);
            Mock<IConnector> connectorMock = _connectorMock;
            _connectorMock.Setup(connector => connector.Send(It.IsAny<byte[]>(), It.IsAny<int>()))
                .Callback<byte[], int>((buffer, length) => RaiseResponse(connectorMock, "00\r"))
                .Returns((HRESULT)null);

            ErrorCode result = reader.SetCarrierID("CARRIERDATA0001X");

            Assert.AreEqual(ErrorCode.Success, result);
        }

        [Test]
        public void SetCarrierID_WhenPayloadHasWrongLength_ReturnsCarrierIdInvalidParameter()
        {
            var reader = new IDReaderOmronASCII(_config, _connectorMock.Object, _loggerMock.Object);

            ErrorCode result = reader.SetCarrierID("SHORT");

            Assert.AreEqual(ErrorCode.CarrierIdInvalidParameter, result);
        }

        [Test]
        public void GetCarrierID_WhenSecondCallOverlaps_ReturnsCarrierIdBusyWithoutExtraSend()
        {
            var reader = new IDReaderOmronASCII(_config, _connectorMock.Object, _loggerMock.Object);
            Mock<IConnector> connectorMock = _connectorMock;
            int sendCount = 0;
            var firstSendStarted = new ManualResetEventSlim(false);

            _connectorMock.Setup(connector => connector.Send(It.IsAny<byte[]>(), It.IsAny<int>()))
                .Callback<byte[], int>((buffer, length) =>
                {
                    sendCount++;
                    firstSendStarted.Set();
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        Thread.Sleep(40);
                        RaiseResponse(connectorMock, "00ASCII1234567890\r");
                    });
                })
                .Returns((HRESULT)null);

            Task<ErrorCode> firstTask = Task.Run(() =>
            {
                string carrierId;
                return reader.GetCarrierID(out carrierId);
            });

            Assert.IsTrue(firstSendStarted.Wait(500));

            string secondCarrierId;
            ErrorCode secondResult = reader.GetCarrierID(out secondCarrierId);

            Assert.AreEqual(ErrorCode.CarrierIdBusy, secondResult);
            Assert.AreEqual(1, sendCount);
            Assert.AreEqual(ErrorCode.Success, firstTask.Result);
        }

        private static void RaiseResponse(Mock<IConnector> connectorMock, string response)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(response);
            connectorMock.Raise(connector => connector.DataReceived += null, bytes, bytes.Length);
        }
    }
}