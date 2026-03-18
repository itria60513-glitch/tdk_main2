using System;
using System.Diagnostics;
using System.Text;
using Communication.Connector.Enum;
using Communication.Interface;
using Moq;
using NUnit.Framework;
using TDKLogUtility.Module;

namespace TDKController.Tests.Unit
{
    [TestFixture]
    public class IDReaderBarcodeReaderTests
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
                BarcodeReaderMaxRetryCount = 8,
            };

            _connectorMock.SetupProperty(connector => connector.IsConnected, false);

            _connectorMock.Setup(connector => connector.Connect())
                .Callback(() => _connectorMock.Object.IsConnected = true)
                .Returns((HRESULT)null);
            _connectorMock.Setup(connector => connector.Disconnect())
                .Callback(() => _connectorMock.Object.IsConnected = false);
        }

        [Test]
        public void GetCarrierID_WhenBarcodeReturnsSuccess_ReturnsCarrierId()
        {
            var reader = new IDReaderBarcodeReader(_config, _connectorMock.Object, _loggerMock.Object);
            Mock<IConnector> connectorMock = _connectorMock;

            _connectorMock.Setup(connector => connector.Send(It.IsAny<byte[]>(), It.IsAny<int>()))
                .Callback<byte[], int>((buffer, length) =>
                {
                    string command = Encoding.ASCII.GetString(buffer, 0, length);
                    if (command == "MOTORON\r" || command == "LOFF\r" || command == "MOTOROFF\r")
                    {
                        RaiseResponse(connectorMock, "OK\r");
                    }
                    else if (command == "LON\r")
                    {
                        RaiseResponse(connectorMock, "ABC12345\r");
                    }
                })
                .Returns((HRESULT)null);

            string carrierId;
            ErrorCode result = reader.GetCarrierID(1, out carrierId);

            Assert.AreEqual(ErrorCode.Success, result);
            Assert.AreEqual("ABC12345", carrierId);
        }

        [Test]
        public void GetCarrierID_WhenBarcodeUnreadableForEightAttempts_ReturnsCarrierIdReadFailed()
        {
            var reader = new IDReaderBarcodeReader(_config, _connectorMock.Object, _loggerMock.Object);
            Mock<IConnector> connectorMock = _connectorMock;
            int readAttempts = 0;

            _connectorMock.Setup(connector => connector.Send(It.IsAny<byte[]>(), It.IsAny<int>()))
                .Callback<byte[], int>((buffer, length) =>
                {
                    string command = Encoding.ASCII.GetString(buffer, 0, length);
                    if (command == "MOTORON\r" || command == "LOFF\r" || command == "MOTOROFF\r")
                    {
                        RaiseResponse(connectorMock, "OK\r");
                    }
                    else if (command == "LON\r")
                    {
                        readAttempts++;
                        RaiseResponse(connectorMock, "NG\r");
                    }
                })
                .Returns((HRESULT)null);

            string carrierId;
            ErrorCode result = reader.GetCarrierID(1, out carrierId);

            Assert.AreEqual(ErrorCode.CarrierIdReadFailed, result);
            Assert.AreEqual(8, readAttempts);
            Assert.IsEmpty(carrierId);
        }

        [Test]
        public void GetCarrierID_WhenBarcodeSucceedsAfterRetries_StopsOnFirstSuccess()
        {
            var reader = new IDReaderBarcodeReader(_config, _connectorMock.Object, _loggerMock.Object);
            Mock<IConnector> connectorMock = _connectorMock;
            int readAttempts = 0;

            _connectorMock.Setup(connector => connector.Send(It.IsAny<byte[]>(), It.IsAny<int>()))
                .Callback<byte[], int>((buffer, length) =>
                {
                    string command = Encoding.ASCII.GetString(buffer, 0, length);
                    if (command == "MOTORON\r" || command == "LOFF\r" || command == "MOTOROFF\r")
                    {
                        RaiseResponse(connectorMock, "OK\r");
                    }
                    else if (command == "LON\r")
                    {
                        readAttempts++;
                        RaiseResponse(connectorMock, readAttempts < 3 ? "NG\r" : "GOOD0001\r");
                    }
                })
                .Returns((HRESULT)null);

            string carrierId;
            ErrorCode result = reader.GetCarrierID(1, out carrierId);

            Assert.AreEqual(ErrorCode.Success, result);
            Assert.AreEqual(3, readAttempts);
            Assert.AreEqual("GOOD0001", carrierId);
        }

        [Test]
        public void GetCarrierID_WhenReadTimeoutOccurs_PerformsCleanupAndReturnsTimeout()
        {
            var reader = new IDReaderBarcodeReader(_config, _connectorMock.Object, _loggerMock.Object);
            Mock<IConnector> connectorMock = _connectorMock;
            var stopwatch = Stopwatch.StartNew();
            bool motorOffSent = false;
            bool disconnectCalled = false;

            _connectorMock.Setup(connector => connector.Disconnect())
                .Callback(() =>
                {
                    _connectorMock.Object.IsConnected = false;
                    disconnectCalled = true;
                });

            _connectorMock.Setup(connector => connector.Send(It.IsAny<byte[]>(), It.IsAny<int>()))
                .Callback<byte[], int>((buffer, length) =>
                {
                    string command = Encoding.ASCII.GetString(buffer, 0, length);
                    if (command == "MOTORON\r" || command == "LOFF\r")
                    {
                        RaiseResponse(connectorMock, "OK\r");
                    }
                    else if (command == "MOTOROFF\r")
                    {
                        motorOffSent = true;
                        RaiseResponse(connectorMock, "OK\r");
                    }
                })
                .Returns((HRESULT)null);

            string carrierId;
            ErrorCode result = reader.GetCarrierID(1, out carrierId);
            stopwatch.Stop();

            Assert.AreEqual(ErrorCode.CarrierIdTimeout, result);
            Assert.IsTrue(stopwatch.ElapsedMilliseconds >= _config.TimeoutMs);
            Assert.IsTrue(motorOffSent);
            _connectorMock.Verify(connector => connector.Connect(), Times.Once);
            Assert.IsTrue(disconnectCalled);
        }

        [Test]
        public void GetCarrierID_WhenBarcodeReturnsError_StopsRetryAndReturnsCommandFailed()
        {
            var reader = new IDReaderBarcodeReader(_config, _connectorMock.Object, _loggerMock.Object);
            Mock<IConnector> connectorMock = _connectorMock;
            int readAttempts = 0;

            _connectorMock.Setup(connector => connector.Send(It.IsAny<byte[]>(), It.IsAny<int>()))
                .Callback<byte[], int>((buffer, length) =>
                {
                    string command = Encoding.ASCII.GetString(buffer, 0, length);
                    if (command == "MOTORON\r" || command == "MOTOROFF\r")
                    {
                        RaiseResponse(connectorMock, "OK\r");
                    }
                    else if (command == "LON\r")
                    {
                        readAttempts++;
                        RaiseResponse(connectorMock, "ERROR\r");
                    }
                })
                .Returns((HRESULT)null);

            string carrierId;
            ErrorCode result = reader.GetCarrierID(1, out carrierId);

            Assert.AreEqual(ErrorCode.CarrierIdCommandFailed, result);
            Assert.AreEqual(1, readAttempts);
            Assert.IsEmpty(carrierId);
        }

        private static void RaiseResponse(Mock<IConnector> connectorMock, string response)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(response);
            connectorMock.Raise(connector => connector.DataReceived += null, bytes, bytes.Length);
        }
    }
}