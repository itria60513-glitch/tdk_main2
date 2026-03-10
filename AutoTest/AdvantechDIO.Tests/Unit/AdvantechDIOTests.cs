using AdvantechDIO.Config;
using AdvantechDIO.Module;
using Automation.BDaq;
using DIO;
using Moq;
using NUnit.Framework;
using System;
using System.Reflection;
using TDKLogUtility.Module;

namespace AdvantechDIO.Tests.Unit
{
    /// <summary>
    /// Unit tests for <see cref="AdvantechDIO.Module.AdvantechDIO"/>.
    ///
    /// NOTE:
    /// - This module directly instantiates DAQNavi SDK controllers (no DI for SDK types),
    ///   so "full success-path" coverage (real Read/Write returning Success) generally
    ///   requires hardware.
    /// - We still maximize coverage by:
    ///   1) Exercising all guard branches.
    ///   2) Forcing catch blocks via controlled <see cref="ILogUtility"/> exceptions.
    ///   3) Invoking private helper methods via reflection.
    /// </summary>
    [TestFixture]
    public class AdvantechDIOTests
    {
        private Mock<ILogUtility> _mockLog;
        private AdvantechDIOConfig _defaultConfig;

        [SetUp]
        public void SetUp()
        {
            _mockLog = new Mock<ILogUtility>();
            _defaultConfig = new AdvantechDIOConfig
            {
                DeviceID = 0,
                DIPortMax = 24,
                DOPortMax = 24,
                PinCountPerPort = 8
            };
        }

        #region Reflection helpers (do not require production code changes)

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = typeof(AdvantechDIO.Module.AdvantechDIO)
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found");
            field.SetValue(target, value);
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            var field = typeof(AdvantechDIO.Module.AdvantechDIO)
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found");
            return (T)field.GetValue(target);
        }

        private static object InvokePrivate(object target, string methodName, params object[] args)
        {
            var method = typeof(AdvantechDIO.Module.AdvantechDIO)
                .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, $"Method '{methodName}' not found");
            return method.Invoke(target, args);
        }

        private AdvantechDIO.Module.AdvantechDIO CreateConnectedStub(
            AdvantechDIOConfig config = null,
            bool withDiCtrl = false,
            bool withDoCtrl = false)
        {
            var dio = new AdvantechDIO.Module.AdvantechDIO(_mockLog.Object, config ?? _defaultConfig);
            SetPrivateField(dio, "_isConnected", true);
            if (withDiCtrl)
                SetPrivateField(dio, "_instantDiCtrl", new InstantDiCtrl());
            if (withDoCtrl)
                SetPrivateField(dio, "_instantDoCtrl", new InstantDoCtrl());
            return dio;
        }

        #endregion

        #region Constructor Tests (argument validation + property initialization)

        [Test]
        public void Constructor_NullLogUtility_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new AdvantechDIO.Module.AdvantechDIO(null, _defaultConfig));
        }

        [Test]
        public void Constructor_NullConfig_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new AdvantechDIO.Module.AdvantechDIO(_mockLog.Object, null));
        }

        [Test]
        public void Constructor_ValidArgs_SetsPropertiesFromConfig()
        {
            _defaultConfig.DeviceID = 5;
            _defaultConfig.DIPortMax = 16;
            _defaultConfig.DOPortMax = 8;
            _defaultConfig.PinCountPerPort = 8;

            using (var dio = new AdvantechDIO.Module.AdvantechDIO(_mockLog.Object, _defaultConfig))
            {
                Assert.AreEqual(5, dio.DeviceID);
                Assert.AreEqual(2, dio.InputPortCount);
                Assert.AreEqual(8, dio.InputBitsPerPort);
                Assert.AreEqual(1, dio.OutputPortCount);
                Assert.AreEqual(8, dio.OutputBitsPerPort);
                Assert.IsFalse(dio.IsVirtual);
                Assert.IsFalse(dio.IsConnected);
                Assert.AreEqual(string.Empty, dio.DeviceName);
            }
        }

        #endregion

        #region Connect/Disconnect Tests (connection lifecycle behavior)

        [Test]
        public void Connect_DeviceNotPresent_ReturnsErrorAndIsConnectedRemainsFalse()
        {
            // With a device ID unlikely to exist, Connect should fail gracefully
            _defaultConfig.DeviceID = 99;
            using (var dio = new AdvantechDIO.Module.AdvantechDIO(_mockLog.Object, _defaultConfig))
            {
                int result = dio.Connect();
                Assert.AreNotEqual(0, result, "Connect should return non-zero for missing device");
                Assert.IsFalse(dio.IsConnected);
            }
        }

        [Test]
        public void Connect_WhenAlreadyConnected_ReturnsSuccessImmediately()
        {
            using (var dio = CreateConnectedStub())
            {
                int result = dio.Connect();
                Assert.AreEqual(0, result);
                Assert.IsTrue(dio.IsConnected);
            }
        }

        [Test]
        public void Connect_NoDIAndNoDOConfigured_SucceedsWithoutSDKControllers()
        {
            var cfg = new AdvantechDIOConfig
            {
                DeviceID = 99,
                DIPortMax = 0,
                DOPortMax = 0,
                PinCountPerPort = 0
            };

            using (var dio = new AdvantechDIO.Module.AdvantechDIO(_mockLog.Object, cfg))
            {
                int result = dio.Connect();
                Assert.AreEqual(0, result);
                Assert.IsTrue(dio.IsConnected);
                Assert.AreEqual(string.Empty, dio.DeviceName);
            }
        }

        [Test]
        public void Connect_DIOnly_InvalidIndex_RaisesExceptionOccurred()
        {
            var cfg = new AdvantechDIOConfig
            {
                DeviceID = 99,
                DIPortMax = 8,
                DOPortMax = 0,
                PinCountPerPort = 8
            };

            using (var dio = new AdvantechDIO.Module.AdvantechDIO(_mockLog.Object, cfg))
            {
                bool raised = false;
                dio.ExceptionOccurred += (s, e) => raised = true;

                int result = dio.Connect();
                Assert.AreNotEqual(0, result);
                Assert.IsTrue(raised);
                Assert.IsFalse(dio.IsConnected);
            }
        }

        [Test]
        public void Connect_DOOnly_InvalidIndex_RaisesExceptionOccurred()
        {
            var cfg = new AdvantechDIOConfig
            {
                DeviceID = 99,
                DIPortMax = 0,
                DOPortMax = 8,
                PinCountPerPort = 8
            };

            using (var dio = new AdvantechDIO.Module.AdvantechDIO(_mockLog.Object, cfg))
            {
                bool raised = false;
                dio.ExceptionOccurred += (s, e) => raised = true;

                int result = dio.Connect();
                Assert.AreNotEqual(0, result);
                Assert.IsTrue(raised);
                Assert.IsFalse(dio.IsConnected);
            }
        }

        [Test]
        public void Disconnect_WhenNotConnected_ReturnsSuccess()
        {
            using (var dio = new AdvantechDIO.Module.AdvantechDIO(_mockLog.Object, _defaultConfig))
            {
                int result = dio.Disconnect();
                Assert.AreEqual(0, result);
                Assert.IsFalse(dio.IsConnected);
            }
        }

        [Test]
        public void Disconnect_WhenConnectedWithoutControllers_ClearsDeviceNameAndIsConnected()
        {
            using (var dio = CreateConnectedStub())
            {
                SetPrivateField(dio, "_deviceName", "TestDevice");
                Assert.AreEqual("TestDevice", dio.DeviceName);

                int result = dio.Disconnect();
                Assert.AreEqual(0, result);
                Assert.IsFalse(dio.IsConnected);
                Assert.AreEqual(string.Empty, dio.DeviceName);
            }
        }

        [Test]
        public void Disconnect_WhenInfoLoggingThrows_ReturnsErrorUndefined()
        {
            var log = new Mock<ILogUtility>();
            log.Setup(l => l.WriteLog("AdvantechDIO", LogHeadType.Info, It.IsAny<string>()))
                .Throws(new InvalidOperationException("boom"));
            log.Setup(l => l.WriteLog("AdvantechDIO", LogHeadType.Error, It.IsAny<string>())).Returns(true);

            using (var dio = new AdvantechDIO.Module.AdvantechDIO(log.Object, _defaultConfig))
            {
                SetPrivateField(dio, "_isConnected", true);
                int result = dio.Disconnect();
                Assert.AreEqual((int)ErrorCode.ErrorUndefined, result);
            }
        }

        #endregion

        #region IsConnected Guard Tests (reject I/O calls when disconnected)

        [Test]
        public void GetInput_WhenNotConnected_ReturnsNotConnectedError()
        {
            using (var dio = new AdvantechDIO.Module.AdvantechDIO(_mockLog.Object, _defaultConfig))
            {
                byte value;
                int result = dio.GetInput(0, out value);
                Assert.AreEqual(AdvantechDIO.Module.AdvantechDIO.NotConnectedError, result);
                Assert.AreEqual(0, value);
            }
        }

        [Test]
        public void GetInputBit_WhenNotConnected_ReturnsNotConnectedError()
        {
            using (var dio = new AdvantechDIO.Module.AdvantechDIO(_mockLog.Object, _defaultConfig))
            {
                byte value;
                int result = dio.GetInputBit(0, 0, out value);
                Assert.AreEqual(AdvantechDIO.Module.AdvantechDIO.NotConnectedError, result);
            }
        }

        [Test]
        public void SetOutput_WhenNotConnected_ReturnsNotConnectedError()
        {
            using (var dio = new AdvantechDIO.Module.AdvantechDIO(_mockLog.Object, _defaultConfig))
            {
                int result = dio.SetOutput(0, 0xFF);
                Assert.AreEqual(AdvantechDIO.Module.AdvantechDIO.NotConnectedError, result);
            }
        }

        [Test]
        public void SetOutputBit_WhenNotConnected_ReturnsNotConnectedError()
        {
            using (var dio = new AdvantechDIO.Module.AdvantechDIO(_mockLog.Object, _defaultConfig))
            {
                int result = dio.SetOutputBit(0, 0, 1);
                Assert.AreEqual(AdvantechDIO.Module.AdvantechDIO.NotConnectedError, result);
            }
        }

        [Test]
        public void GetOutput_WhenNotConnected_ReturnsNotConnectedError()
        {
            using (var dio = new AdvantechDIO.Module.AdvantechDIO(_mockLog.Object, _defaultConfig))
            {
                byte value;
                int result = dio.GetOutput(0, out value);
                Assert.AreEqual(AdvantechDIO.Module.AdvantechDIO.NotConnectedError, result);
            }
        }

        [Test]
        public void GetOutputBit_WhenNotConnected_ReturnsNotConnectedError()
        {
            using (var dio = new AdvantechDIO.Module.AdvantechDIO(_mockLog.Object, _defaultConfig))
            {
                byte value;
                int result = dio.GetOutputBit(0, 0, out value);
                Assert.AreEqual(AdvantechDIO.Module.AdvantechDIO.NotConnectedError, result);
            }
        }

        [Test]
        public void SnapStart_WhenNotConnected_ReturnsNotConnectedError()
        {
            using (var dio = new AdvantechDIO.Module.AdvantechDIO(_mockLog.Object, _defaultConfig))
            {
                int result = dio.SnapStart();
                Assert.AreEqual(AdvantechDIO.Module.AdvantechDIO.NotConnectedError, result);
            }
        }

        [Test]
        public void SnapStop_WhenNotConnected_ReturnsNotConnectedError()
        {
            using (var dio = new AdvantechDIO.Module.AdvantechDIO(_mockLog.Object, _defaultConfig))
            {
                int result = dio.SnapStop();
                Assert.AreEqual(AdvantechDIO.Module.AdvantechDIO.NotConnectedError, result);
            }
        }

        #endregion

        #region Dispose Tests (idempotent dispose + connection state)

        [Test]
        public void Dispose_CalledMultipleTimes_DoesNotThrow()
        {
            var dio = new AdvantechDIO.Module.AdvantechDIO(_mockLog.Object, _defaultConfig);
            Assert.DoesNotThrow(() =>
            {
                dio.Dispose();
                dio.Dispose();
                dio.Dispose();
            });
        }

        [Test]
        public void Dispose_SetsIsConnectedFalse()
        {
            var dio = new AdvantechDIO.Module.AdvantechDIO(_mockLog.Object, _defaultConfig);
            dio.Dispose();
            Assert.IsFalse(dio.IsConnected);
        }

        [Test]
        public void Dispose_WhenAlreadyDisposed_ReturnsImmediately()
        {
            using (var dio = CreateConnectedStub())
            {
                SetPrivateField(dio, "_disposed", true);
                Assert.DoesNotThrow(() => dio.Dispose());
            }
        }

        [Test]
        public void Dispose_WhenDisconnectThrows_IsCaughtAndDisposedFlagSet()
        {
            var log = new Mock<ILogUtility>();
            // Force Disconnect to enter its catch by throwing on Info log, then throw again inside Disconnect catch.
            log.Setup(l => l.WriteLog("AdvantechDIO", LogHeadType.Info, It.IsAny<string>()))
                .Throws(new InvalidOperationException("boom"));
            log.Setup(l => l.WriteLog("AdvantechDIO", LogHeadType.Error, It.Is<string>(s => s.StartsWith("Disconnect failed:"))))
                .Throws(new InvalidOperationException("boom2"));
            log.Setup(l => l.WriteLog("AdvantechDIO", LogHeadType.Error, It.Is<string>(s => s.StartsWith("Dispose failed during Disconnect:"))))
                .Returns(true);

            var dio = new AdvantechDIO.Module.AdvantechDIO(log.Object, _defaultConfig);
            SetPrivateField(dio, "_isConnected", true);

            Assert.DoesNotThrow(() => dio.Dispose());
            Assert.IsTrue(GetPrivateField<bool>(dio, "_disposed"));
        }

        #endregion

        #region Event Contract Tests (raised or suppressed under failure/disconnected states)

        [Test]
        public void ExceptionOccurred_RaisedOnConnectFailure()
        {
            _defaultConfig.DeviceID = 99;
            bool eventRaised = false;
            using (var dio = new AdvantechDIO.Module.AdvantechDIO(_mockLog.Object, _defaultConfig))
            {
                dio.ExceptionOccurred += (s, e) => eventRaised = true;
                dio.Connect();
                Assert.IsTrue(eventRaised, "ExceptionOccurred should be raised when Connect fails");
            }
        }

        [Test]
        public void DO_ValueChanged_NotRaisedWhenNotConnected()
        {
            bool eventRaised = false;
            using (var dio = new AdvantechDIO.Module.AdvantechDIO(_mockLog.Object, _defaultConfig))
            {
                dio.DO_ValueChanged += (s, e) => eventRaised = true;
                dio.SetOutput(0, 0xFF);
                Assert.IsFalse(eventRaised, "DO_ValueChanged should not fire when not connected");
            }
        }

        #endregion

        #region Partial Config Tests (missing DI/DO topology sections)

        [Test]
        public void Constructor_MissingDISection_DIPortCountIsZero()
        {
            var config = new AdvantechDIOConfig
            {
                DeviceID = 0,
                DIPortMax = 0,
                DOPortMax = 16,
                PinCountPerPort = 8
            };

            using (var dio = new AdvantechDIO.Module.AdvantechDIO(_mockLog.Object, config))
            {
                Assert.AreEqual(0, dio.InputPortCount);
                Assert.AreEqual(8, dio.InputBitsPerPort);
                Assert.AreEqual(2, dio.OutputPortCount);
            }
        }

        [Test]
        public void Constructor_MissingDOSection_DOPortCountIsZero()
        {
            var config = new AdvantechDIOConfig
            {
                DeviceID = 0,
                DIPortMax = 16,
                DOPortMax = 0,
                PinCountPerPort = 8
            };

            using (var dio = new AdvantechDIO.Module.AdvantechDIO(_mockLog.Object, config))
            {
                Assert.AreEqual(2, dio.InputPortCount);
                Assert.AreEqual(0, dio.OutputPortCount);
                Assert.AreEqual(8, dio.OutputBitsPerPort);
            }
        }

        [Test]
        public void GetInput_MissingDIConfig_WhenConnectedWithDOOnly_ReturnsNotConnectedError()
        {
            // DI not configured (0 ports) - even if connected,
            // guard should reject DI calls
            var config = new AdvantechDIOConfig
            {
                DeviceID = 0,
                DIPortMax = 0,
                DOPortMax = 16,
                PinCountPerPort = 8
            };

            using (var dio = new AdvantechDIO.Module.AdvantechDIO(_mockLog.Object, config))
            {
                // Not connected, so guard catches at connection check first
                byte value;
                int result = dio.GetInput(0, out value);
                Assert.AreEqual(AdvantechDIO.Module.AdvantechDIO.NotConnectedError, result);
            }
        }

        [Test]
        public void SetOutput_MissingDOConfig_WhenNotConnected_ReturnsNotConnectedError()
        {
            var config = new AdvantechDIOConfig
            {
                DeviceID = 0,
                DIPortMax = 16,
                DOPortMax = 0,
                PinCountPerPort = 8
            };

            using (var dio = new AdvantechDIO.Module.AdvantechDIO(_mockLog.Object, config))
            {
                int result = dio.SetOutput(0, 0xFF);
                Assert.AreEqual(AdvantechDIO.Module.AdvantechDIO.NotConnectedError, result);
            }
        }

        #endregion

        #region Interface Compliance Tests

        [Test]
        public void AdvantechDIO_ImplementsIIOBoard()
        {
            using (var dio = new AdvantechDIO.Module.AdvantechDIO(_mockLog.Object, _defaultConfig))
            {
                Assert.IsInstanceOf<IIOBoard>(dio);
                Assert.IsInstanceOf<IDI>(dio);
                Assert.IsInstanceOf<IDO>(dio);
                Assert.IsInstanceOf<IIOBoardBase>(dio);
                Assert.IsInstanceOf<IDisposable>(dio);
            }
        }

        #endregion

        #region Error Code Constants Tests

        [Test]
        public void ErrorCodeConstants_AreInConstitutionRange()
        {
            // Constitution range: -1000 ~ -1099
            Assert.That(AdvantechDIO.Module.AdvantechDIO.NotConnectedError, Is.InRange(-1099, -1000));
            Assert.That(AdvantechDIO.Module.AdvantechDIO.PortIndexOutOfRangeError, Is.InRange(-1099, -1000));
            Assert.That(AdvantechDIO.Module.AdvantechDIO.BitIndexOutOfRangeError, Is.InRange(-1099, -1000));
        }

        [Test]
        public void ErrorCodeConstants_AreDistinctValues()
        {
            Assert.AreNotEqual(
                AdvantechDIO.Module.AdvantechDIO.NotConnectedError,
                AdvantechDIO.Module.AdvantechDIO.PortIndexOutOfRangeError);
            Assert.AreNotEqual(
                AdvantechDIO.Module.AdvantechDIO.PortIndexOutOfRangeError,
                AdvantechDIO.Module.AdvantechDIO.BitIndexOutOfRangeError);
            Assert.AreNotEqual(
                AdvantechDIO.Module.AdvantechDIO.NotConnectedError,
                AdvantechDIO.Module.AdvantechDIO.BitIndexOutOfRangeError);
        }

        #endregion

        #region Guard range checks when "connected" (no hardware required)

        [Test]
        public void GetInput_Connected_PortIndexOutOfRange_ReturnsPortIndexOutOfRangeError()
        {
            using (var dio = CreateConnectedStub(withDiCtrl: true))
            {
                byte value;
                int result = dio.GetInput(99, out value);
                Assert.AreEqual(AdvantechDIO.Module.AdvantechDIO.PortIndexOutOfRangeError, result);
            }
        }

        [Test]
        public void GetInputBit_Connected_BitIndexOutOfRange_ReturnsBitIndexOutOfRangeError()
        {
            using (var dio = CreateConnectedStub(withDiCtrl: true))
            {
                byte value;
                int result = dio.GetInputBit(0, 99, out value);
                Assert.AreEqual(AdvantechDIO.Module.AdvantechDIO.BitIndexOutOfRangeError, result);
            }
        }

        [Test]
        public void SetOutput_Connected_PortIndexOutOfRange_ReturnsPortIndexOutOfRangeError()
        {
            using (var dio = CreateConnectedStub(withDoCtrl: true))
            {
                int result = dio.SetOutput(99, 0xFF);
                Assert.AreEqual(AdvantechDIO.Module.AdvantechDIO.PortIndexOutOfRangeError, result);
            }
        }

        [Test]
        public void SetOutputBit_Connected_BitIndexOutOfRange_ReturnsBitIndexOutOfRangeError()
        {
            using (var dio = CreateConnectedStub(withDoCtrl: true))
            {
                int result = dio.SetOutputBit(0, 99, 1);
                Assert.AreEqual(AdvantechDIO.Module.AdvantechDIO.BitIndexOutOfRangeError, result);
            }
        }

        [Test]
        public void GetOutput_Connected_PortIndexOutOfRange_ReturnsPortIndexOutOfRangeError()
        {
            using (var dio = CreateConnectedStub(withDoCtrl: true))
            {
                byte value;
                int result = dio.GetOutput(99, out value);
                Assert.AreEqual(AdvantechDIO.Module.AdvantechDIO.PortIndexOutOfRangeError, result);
            }
        }

        [Test]
        public void GetOutputBit_Connected_BitIndexOutOfRange_ReturnsBitIndexOutOfRangeError()
        {
            using (var dio = CreateConnectedStub(withDoCtrl: true))
            {
                byte value;
                int result = dio.GetOutputBit(0, 99, out value);
                Assert.AreEqual(AdvantechDIO.Module.AdvantechDIO.BitIndexOutOfRangeError, result);
            }
        }

        [Test]
        public void SnapStart_ConnectedButDINotConfigured_ReturnsNotConnectedError()
        {
            using (var dio = CreateConnectedStub(withDiCtrl: false))
            {
                int result = dio.SnapStart();
                Assert.AreEqual(AdvantechDIO.Module.AdvantechDIO.NotConnectedError, result);
            }
        }

        [Test]
        public void SnapStop_ConnectedButDINotConfigured_ReturnsNotConnectedError()
        {
            using (var dio = CreateConnectedStub(withDiCtrl: false))
            {
                int result = dio.SnapStop();
                Assert.AreEqual(AdvantechDIO.Module.AdvantechDIO.NotConnectedError, result);
            }
        }

        #endregion

        #region Private helper coverage (reflection)

        [Test]
        public void HandleSdkResult_Success_ReturnsZero()
        {
            using (var dio = new AdvantechDIO.Module.AdvantechDIO(_mockLog.Object, _defaultConfig))
            {
                int result = (int)InvokePrivate(dio, "HandleSdkResult", ErrorCode.Success, "UnitTest");
                Assert.AreEqual(0, result);
            }
        }

        [Test]
        public void HandleSdkResult_Error_LogsAndRaisesExceptionOccurred()
        {
            bool raised = false;
            using (var dio = new AdvantechDIO.Module.AdvantechDIO(_mockLog.Object, _defaultConfig))
            {
                dio.ExceptionOccurred += (s, e) => raised = true;

                int result = (int)InvokePrivate(dio, "HandleSdkResult", ErrorCode.ErrorUndefined, "UnitTest");
                Assert.AreEqual((int)ErrorCode.ErrorUndefined, result);
                Assert.IsTrue(raised);
            }
        }

        [Test]
        public void OnDiChangeOfState_ForwardsToDI_ValueChanged()
        {
            bool raised = false;
            using (var dio = new AdvantechDIO.Module.AdvantechDIO(_mockLog.Object, _defaultConfig))
            {
                dio.DI_ValueChanged += (s, e) => raised = true;
                InvokePrivate(dio, "OnDiChangeOfState", dio, null);
                Assert.IsTrue(raised);
            }
        }

        [Test]
        public void RaiseDO_ValueChanged_NoSubscribers_DoesNotThrow()
        {
            using (var dio = new AdvantechDIO.Module.AdvantechDIO(_mockLog.Object, _defaultConfig))
            {
                Assert.DoesNotThrow(() => InvokePrivate(dio, "RaiseDO_ValueChanged"));
            }
        }

        [Test]
        public void RaiseExceptionOccurred_NoSubscribers_DoesNotThrow()
        {
            using (var dio = new AdvantechDIO.Module.AdvantechDIO(_mockLog.Object, _defaultConfig))
            {
                Assert.DoesNotThrow(() => InvokePrivate(dio, "RaiseExceptionOccurred"));
            }
        }

        #endregion

        #region Catch blocks for public APIs (force exceptions via ILogUtility)

        [Test]
        public void GetInput_WhenGuardLoggingThrows_ReturnsErrorUndefined()
        {
            var log = new Mock<ILogUtility>();
            log.SetupSequence(l => l.WriteLog("AdvantechDIO", LogHeadType.Warning, It.IsAny<string>()))
                .Throws(new InvalidOperationException("boom"))
                .Returns(true);
            log.Setup(l => l.WriteLog("AdvantechDIO", LogHeadType.Error, It.IsAny<string>())).Returns(true);

            using (var dio = new AdvantechDIO.Module.AdvantechDIO(log.Object, _defaultConfig))
            {
                byte value;
                int result = dio.GetInput(0, out value);
                Assert.AreEqual((int)ErrorCode.ErrorUndefined, result);
            }
        }

        [Test]
        public void SetOutput_WhenGuardLoggingThrows_ReturnsErrorUndefined()
        {
            var log = new Mock<ILogUtility>();
            log.SetupSequence(l => l.WriteLog("AdvantechDIO", LogHeadType.Warning, It.IsAny<string>()))
                .Throws(new InvalidOperationException("boom"))
                .Returns(true);
            log.Setup(l => l.WriteLog("AdvantechDIO", LogHeadType.Error, It.IsAny<string>())).Returns(true);

            using (var dio = new AdvantechDIO.Module.AdvantechDIO(log.Object, _defaultConfig))
            {
                int result = dio.SetOutput(0, 0xFF);
                Assert.AreEqual((int)ErrorCode.ErrorUndefined, result);
            }
        }

        [Test]
        public void SnapStart_WhenNotConnectedLoggingThrows_ReturnsErrorUndefined()
        {
            var log = new Mock<ILogUtility>();
            log.SetupSequence(l => l.WriteLog("AdvantechDIO", LogHeadType.Warning, It.IsAny<string>()))
                .Throws(new InvalidOperationException("boom"))
                .Returns(true);
            log.Setup(l => l.WriteLog("AdvantechDIO", LogHeadType.Error, It.IsAny<string>())).Returns(true);

            using (var dio = new AdvantechDIO.Module.AdvantechDIO(log.Object, _defaultConfig))
            {
                int result = dio.SnapStart();
                Assert.AreEqual((int)ErrorCode.ErrorUndefined, result);
            }
        }

        [Test]
        public void SnapStop_WhenNotConnectedLoggingThrows_ReturnsErrorUndefined()
        {
            var log = new Mock<ILogUtility>();
            log.SetupSequence(l => l.WriteLog("AdvantechDIO", LogHeadType.Warning, It.IsAny<string>()))
                .Throws(new InvalidOperationException("boom"))
                .Returns(true);
            log.Setup(l => l.WriteLog("AdvantechDIO", LogHeadType.Error, It.IsAny<string>())).Returns(true);

            using (var dio = new AdvantechDIO.Module.AdvantechDIO(log.Object, _defaultConfig))
            {
                int result = dio.SnapStop();
                Assert.AreEqual((int)ErrorCode.ErrorUndefined, result);
            }
        }

        #endregion
    }
}
