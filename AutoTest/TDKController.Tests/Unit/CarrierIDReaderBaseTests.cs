using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Communication.Connector.Enum;
using Communication.Interface;
using Moq;
using NUnit.Framework;
using TDKController.Interface;
using TDKLogUtility.Module;

namespace TDKController.Tests.Unit
{
    [TestFixture]
    public class CarrierIDReaderBaseTests
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
                TimeoutMs = 100,
                BarcodeReaderMaxRetryCount = 2,
            };

            _connectorMock.SetupProperty(connector => connector.IsConnected, false);

            _connectorMock.Setup(connector => connector.Connect())
                .Callback(() => _connectorMock.Object.IsConnected = true)
                .Returns((HRESULT)null);
            _connectorMock.Setup(connector => connector.Disconnect())
                .Callback(() => _connectorMock.Object.IsConnected = false);
        }

        [Test]
        public void Constructor_NullConfig_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new IDReaderBarcodeReader(null, _connectorMock.Object, _loggerMock.Object));
        }

        [Test]
        public void Constructor_NullConnector_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new IDReaderBarcodeReader(_config, null, _loggerMock.Object));
        }

        [Test]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new IDReaderBarcodeReader(_config, _connectorMock.Object, null));
        }

        [Test]
        public void GetCarrierID_WhenOperationAlreadyRunning_ReturnsCarrierIdBusy()
        {
            var reader = new IDReaderBarcodeReader(_config, _connectorMock.Object, _loggerMock.Object);
            Mock<IConnector> connectorMock = _connectorMock;
            var firstReadStarted = new ManualResetEventSlim(false);
            int sendCount = 0;

            _connectorMock.Setup(connector => connector.Send(It.IsAny<byte[]>(), It.IsAny<int>()))
                .Callback<byte[], int>((buffer, length) =>
                {
                    string command = Encoding.ASCII.GetString(buffer, 0, length);
                    sendCount++;
                    if (command == "MOTORON\r")
                    {
                        RaiseResponse(connectorMock, "OK\r");
                    }
                    else if (command == "LON\r")
                    {
                        firstReadStarted.Set();
                        ThreadPool.QueueUserWorkItem(_ =>
                        {
                            Thread.Sleep(50);
                            RaiseResponse(connectorMock, "CARRIER01\r");
                        });
                    }
                    else if (command == "LOFF\r" || command == "MOTOROFF\r")
                    {
                        RaiseResponse(connectorMock, "OK\r");
                    }
                })
                .Returns((HRESULT)null);

            Task<ErrorCode> firstTask = Task.Run(() =>
            {
                string carrierId;
                return reader.GetCarrierID(1, out carrierId);
            });

            Assert.IsTrue(firstReadStarted.Wait(500));

            string secondCarrierId;
            ErrorCode secondResult = reader.GetCarrierID(1, out secondCarrierId);

            Assert.AreEqual(ErrorCode.CarrierIdBusy, secondResult);
            Assert.IsEmpty(secondCarrierId);
            Assert.AreEqual(2, sendCount);
            Assert.AreEqual(ErrorCode.Success, firstTask.Result);
        }

        [Test]
        public void GetCarrierID_WhenTimeoutOccurs_ReleasesBusyForNextCall()
        {
            var reader = new IDReaderBarcodeReader(_config, _connectorMock.Object, _loggerMock.Object);
            Mock<IConnector> connectorMock = _connectorMock;
            int readCount = 0;

            _connectorMock.Setup(connector => connector.Send(It.IsAny<byte[]>(), It.IsAny<int>()))
                .Callback<byte[], int>((buffer, length) =>
                {
                    string command = Encoding.ASCII.GetString(buffer, 0, length);
                    if (command == "MOTORON\r")
                    {
                        RaiseResponse(connectorMock, "OK\r");
                    }
                    else if (command == "LON\r")
                    {
                        readCount++;
                        if (readCount > 1)
                        {
                            RaiseResponse(connectorMock, "GOODID01\r");
                        }
                    }
                    else if (command == "LOFF\r" || command == "MOTOROFF\r")
                    {
                        RaiseResponse(connectorMock, "OK\r");
                    }
                })
                .Returns((HRESULT)null);

            string firstCarrierId;
            ErrorCode firstResult = reader.GetCarrierID(1, out firstCarrierId);
            string secondCarrierId;
            ErrorCode secondResult = reader.GetCarrierID(1, out secondCarrierId);

            Assert.AreEqual(ErrorCode.CarrierIdTimeout, firstResult);
            Assert.AreEqual(ErrorCode.Success, secondResult);
            Assert.AreEqual("GOODID01", secondCarrierId);
        }

        [Test]
        public void SetCarrierID_OnBarcodeReader_ReturnsCarrierIdError()
        {
            var reader = new IDReaderBarcodeReader(_config, _connectorMock.Object, _loggerMock.Object);

            ErrorCode result = reader.SetCarrierID(1, "1234567890ABCDEF");

            Assert.AreEqual(ErrorCode.CarrierIdError, result);
        }

        [Test]
        public void GetCarrierID_AfterDispose_ThrowsObjectDisposedException()
        {
            var reader = new IDReaderBarcodeReader(_config, _connectorMock.Object, _loggerMock.Object);
            reader.Dispose();

            string carrierId;
            Assert.Throws<ObjectDisposedException>(() => reader.GetCarrierID(1, out carrierId));
        }

        [Test]
        public void SetCarrierID_AfterDispose_ThrowsObjectDisposedException()
        {
            var reader = new IDReaderBarcodeReader(_config, _connectorMock.Object, _loggerMock.Object);
            reader.Dispose();

            Assert.Throws<ObjectDisposedException>(() => reader.SetCarrierID(1, "1234567890ABCDEF"));
        }

        [Test]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            var reader = new IDReaderBarcodeReader(_config, _connectorMock.Object, _loggerMock.Object);

            reader.Dispose();

            Assert.DoesNotThrow(() => reader.Dispose());
        }

        [Test]
        public void GetCarrierID_WhenAlreadyConnected_DoesNotCallConnectOrDisconnect()
        {
            var reader = new IDReaderBarcodeReader(_config, _connectorMock.Object, _loggerMock.Object);
            Mock<IConnector> connectorMock = _connectorMock;
            _connectorMock.Object.IsConnected = true;

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
                        RaiseResponse(connectorMock, "READY001\r");
                    }
                })
                .Returns((HRESULT)null);

            string carrierId;
            ErrorCode result = reader.GetCarrierID(1, out carrierId);

            Assert.AreEqual(ErrorCode.Success, result);
            Assert.AreEqual("READY001", carrierId);
            _connectorMock.Verify(connector => connector.Connect(), Times.Never);
            _connectorMock.Verify(connector => connector.Disconnect(), Times.Never);
        }

        [Test]
        public void GetCarrierID_WhenNotConnected_CallsConnectAndDisconnect()
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
                        RaiseResponse(connectorMock, "READY002\r");
                    }
                })
                .Returns((HRESULT)null);

            string carrierId;
            ErrorCode result = reader.GetCarrierID(1, out carrierId);

            Assert.AreEqual(ErrorCode.Success, result);
            Assert.AreEqual("READY002", carrierId);
            _connectorMock.Verify(connector => connector.Connect(), Times.Once);
            _connectorMock.Verify(connector => connector.Disconnect(), Times.Once);
        }

        private static void RaiseResponse(Mock<IConnector> connectorMock, string response)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(response);
            connectorMock.Raise(connector => connector.DataReceived += null, bytes, bytes.Length);
        }
    }
}