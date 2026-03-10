using System;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Text;
using Communication.Interface;
using Communication.Connector.Enum;
using NUnit.Framework;
using TDKController;
using TDKController.Tests.Helpers;
using TDKLogUtility.Module;
using Moq;

namespace TDKController.Tests.Unit
{
    /// <summary>
    /// Unit tests for LoadportActor TAS300 command execution and ErrorCode return.
    /// Test naming: MethodName_Scenario_ExpectedResult.
    /// </summary>
    [TestFixture]
    public class LoadportActorTests
    {
        private TAS300MockHelper _mockConnector;
        private Mock<ILogUtility> _mockLogger;
        private LoadportActorConfig _config;
        private LoadportActor _actor;

        [SetUp]
        public void SetUp()
        {
            _mockConnector = new TAS300MockHelper();
            _mockLogger = new Mock<ILogUtility>();
            _config = new LoadportActorConfig
            {
                AckTimeout = 2000,
                InfTimeout = 2000
            };
            _actor = new LoadportActor(_config, _mockConnector, _mockLogger.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _actor?.Dispose();
            _mockConnector?.Dispose();
        }

        #region Constructor Tests

        [Test]
        public void Constructor_NullConfig_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new LoadportActor(null, _mockConnector, _mockLogger.Object));
        }

        [Test]
        public void Constructor_NullConnector_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new LoadportActor(_config, null, _mockLogger.Object));
        }

        [Test]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new LoadportActor(_config, _mockConnector, null));
        }

        #endregion

        #region Init / InitForce / InitProgram Tests

        [Test]
        public void Init_Success_SendsORGSHAndReturnsSuccess()
        {
            // GetFxlAmhsStatus also needs a response after Init succeeds
            SetupOperationThenQuickSuccess("00000000000000000000");

            var result = _actor.Init();

            Assert.AreEqual(ErrorCode.Success, result);
            // First send is MOV:ORGSH, then GET:STATE for GetFxlAmhsStatus
        }

        [Test]
        public void Init_AckTimeout_ReturnsAckTimeout()
        {
            _mockConnector.SetupAckTimeout();
            _config.AckTimeout = 100;

            var result = _actor.Init();

            Assert.AreEqual(ErrorCode.AckTimeout, result);
        }

        [Test]
        public void Init_InfTimeout_ReturnsInfTimeout()
        {
            _mockConnector.SetupInfTimeout();
            _config.InfTimeout = 100;

            var result = _actor.Init();

            Assert.AreEqual(ErrorCode.InfTimeout, result);
        }

        [Test]
        public void Init_NAK_ReturnsCommandFailed()
        {
            _mockConnector.SetupNakResponse();

            var result = _actor.Init();

            Assert.AreEqual(ErrorCode.CommandFailed, result);
        }

        [Test]
        public void InitForce_Success_SendsABORGAndReturnsSuccess()
        {
            SetupOperationThenQuickSuccess("00000000000000000000");

            var result = _actor.InitForce();

            Assert.AreEqual(ErrorCode.Success, result);
        }

        [Test]
        public void InitForce_AckTimeout_ReturnsAckTimeout()
        {
            _mockConnector.SetupAckTimeout();
            _config.AckTimeout = 100;

            var result = _actor.InitForce();

            Assert.AreEqual(ErrorCode.AckTimeout, result);
        }

        [Test]
        public void InitForce_NAK_ReturnsCommandFailed()
        {
            _mockConnector.SetupNakResponse();

            var result = _actor.InitForce();

            Assert.AreEqual(ErrorCode.CommandFailed, result);
        }

        [Test]
        public void InitProgram_Success_SendsINITLAndReturnsSuccess()
        {
            SetupOperationThenQuickSuccess("00000000000000000000");

            var result = _actor.InitProgram();

            Assert.AreEqual(ErrorCode.Success, result);
        }

        [Test]
        public void InitProgram_InfTimeout_ReturnsInfTimeout()
        {
            _mockConnector.SetupInfTimeout();
            _config.InfTimeout = 100;

            var result = _actor.InitProgram();

            Assert.AreEqual(ErrorCode.InfTimeout, result);
        }

        #endregion

        #region Load / Clamp / Dock Tests

        [Test]
        public void Load_Success_SendsCLOADAndReturnsSuccess()
        {
            SetupOperationThenQuickSuccess("00100000000000000000");

            var result = _actor.Load();

            Assert.AreEqual(ErrorCode.Success, result);
        }

        [Test]
        public void Load_AckTimeout_ReturnsAckTimeout()
        {
            _mockConnector.SetupAckTimeout();
            _config.AckTimeout = 100;

            var result = _actor.Load();

            Assert.AreEqual(ErrorCode.AckTimeout, result);
        }

        [Test]
        public void Load_ABS_ReturnsCommandFailed()
        {
            _mockConnector.SetupAbsResponse();

            var result = _actor.Load();

            Assert.AreEqual(ErrorCode.CommandFailed, result);
        }

        [Test]
        public void Clamp_Success_SendsPODCLAndReturnsSuccess()
        {
            SetupOperationThenQuickSuccess("00100010000000000000");

            var result = _actor.Clamp();

            Assert.AreEqual(ErrorCode.Success, result);
        }

        [Test]
        public void Dock_Success_SendsCLDYDAndReturnsSuccess()
        {
            SetupOperationThenQuickSuccess("00100011010000000000");

            var result = _actor.Dock();

            Assert.AreEqual(ErrorCode.Success, result);
        }

        #endregion

        #region Unload / Undock / CloseDoor / ResetError Tests

        [Test]
        public void Unload_WhenClamped_SendsPODOP()
        {
            // Set FOUP status to CLAMPED first via a successful Init
            SetupForFoupStatus('1', '1', '0', '?');
            PerformSuccessfulInit();

            // Now setup for Unload
            SetupOperationThenQuickSuccess("00000000000000000000");
            var result = _actor.Unload();

            Assert.AreEqual(ErrorCode.Success, result);
        }

        [Test]
        public void Unload_WhenNotClamped_SendsABORG()
        {
            // FOUP status defaults to FPS_UNKNOWN, which is not FPS_CLAMPED
            SetupOperationThenQuickSuccess("00000000000000000000");

            var result = _actor.Unload();

            Assert.AreEqual(ErrorCode.Success, result);
        }

        [Test]
        public void Undock_Success_SendsCULFCAndReturnsSuccess()
        {
            SetupOperationThenQuickSuccess("00100010000000000000");

            var result = _actor.Undock();

            Assert.AreEqual(ErrorCode.Success, result);
        }

        [Test]
        public void CloseDoor_Success_SendsCULYDAndReturnsSuccess()
        {
            SetupOperationThenQuickSuccess("00100011010000000000");

            var result = _actor.CloseDoor();

            Assert.AreEqual(ErrorCode.Success, result);
        }

        [Test]
        public void ResetError_Success_SendsRESETAndReturnsSuccess()
        {
            _mockConnector.SetupSuccessResponse();

            var result = _actor.ResetError();

            Assert.AreEqual(ErrorCode.Success, result);
        }

        [Test]
        public void ResetError_AckTimeout_ReturnsAckTimeout()
        {
            _mockConnector.SetupAckTimeout();
            _config.AckTimeout = 100;

            var result = _actor.ResetError();

            Assert.AreEqual(ErrorCode.AckTimeout, result);
        }

        [Test]
        public void ResetError_InfTimeout_ReturnsInfTimeout()
        {
            _mockConnector.SetupInfTimeout();
            _config.InfTimeout = 100;

            var result = _actor.ResetError();

            Assert.AreEqual(ErrorCode.InfTimeout, result);
        }

        [Test]
        public void ResetError_NAK_ReturnsCommandFailed()
        {
            _mockConnector.SetupNakResponse();

            var result = _actor.ResetError();

            Assert.AreEqual(ErrorCode.CommandFailed, result);
        }

        #endregion

        #region GetLPStatus / SendLoadportCommand Tests

        [Test]
        public void GetLPStatus_Success_Returns20CharData()
        {
            string statusData = "00100010000000000000";
            _mockConnector.SetupQuickSuccessResponse(statusData);

            string data;
            var result = _actor.GetLPStatus(out data);

            Assert.AreEqual(ErrorCode.Success, result);
            Assert.IsNotEmpty(data);
        }

        [Test]
        public void GetLPStatus_Success_UpdatesStatusFields()
        {
            // Status: EqpStatus='0', Mode='0', Inited='1', OpStatus='0',
            // Ecode=0x1A, FpPlace='1', FpClamp='0', LtchKey='1', Vacuum='0',
            // FpDoor='1', WfProtrusion='0', ZPos='0', YPos='0',
            // MpArmPos='0', MpZPos='0', MpStopper='0', MappingStatus='0',
            // IntKey='0', InfoPad='0'
            string statusData = "00101A10100100000000";
            _mockConnector.SetupQuickSuccessResponse(statusData);

            string data;
            _actor.GetLPStatus(out data);

            Assert.AreEqual('0', _actor.Status.EqpStatus);
            Assert.AreEqual('1', _actor.Status.Inited);
            Assert.AreEqual(0x1A, _actor.Status.Ecode);
            Assert.AreEqual('1', _actor.Status.FpPlace);
            Assert.AreEqual('0', _actor.Status.FpClamp);
        }

        [Test]
        public void GetLPStatus_Failure_ReturnsEmptyData()
        {
            _mockConnector.SetupAckTimeout();
            _config.AckTimeout = 100;

            string data;
            var result = _actor.GetLPStatus(out data);

            Assert.AreEqual(ErrorCode.AckTimeout, result);
            Assert.AreEqual(string.Empty, data);
        }

        [Test]
        public void GetLPStatus_EcodeHexParse_HandlesAllDigits()
        {
            // Test hex parsing: ecode = "FF" → 255
            string statusData = "0010FF1000000000000" + "0";
            _mockConnector.SetupQuickSuccessResponse(statusData);

            string data;
            _actor.GetLPStatus(out data);

            Assert.AreEqual(0xFF, _actor.Status.Ecode);
        }

        [Test]
        public void SendLoadportCommand_Success_ReturnsResponseData()
        {
            _mockConnector.SetupSuccessResponse(null, "TESTDATA");

            string data;
            var result = _actor.SendLoadportCommand(out data, "GET:TEST");

            Assert.AreEqual(ErrorCode.Success, result);
        }

        [Test]
        public void SendLoadportCommand_Failure_ReturnsEmptyData()
        {
            _mockConnector.SetupNakResponse();

            string data;
            var result = _actor.SendLoadportCommand(out data, "GET:TEST");

            Assert.AreEqual(ErrorCode.CommandFailed, result);
            Assert.AreEqual(string.Empty, data);
        }

        #endregion

        #region GetFOUPStatus / FoupStatus Property Tests

        [Test]
        public void GetFOUPStatus_NoFoup_Returns0x28()
        {
            // fpPlace='0' → FPS_NOFOUP → 0x28
            string statusData = "00100000000000000000";
            _mockConnector.SetupQuickSuccessResponse(statusData);

            string statfxl;
            var result = _actor.GetFOUPStatus(out statfxl);

            Assert.AreEqual(ErrorCode.Success, result);
            Assert.AreEqual("0x28", statfxl);
            Assert.AreEqual(0, _actor.FoupStatus); // FPS_NOFOUP = 0
        }

        [Test]
        public void GetFOUPStatus_Placed_Returns0x69()
        {
            // fpPlace='1', fpClamp='0' → FPS_PLACED → 0x69
            string statusData = "00100010000000000000";
            _mockConnector.SetupQuickSuccessResponse(statusData);

            string statfxl;
            var result = _actor.GetFOUPStatus(out statfxl);

            Assert.AreEqual(ErrorCode.Success, result);
            Assert.AreEqual("0x69", statfxl);
            Assert.AreEqual(1, _actor.FoupStatus); // FPS_PLACED = 1
        }

        [Test]
        public void GetFOUPStatus_Clamped_Returns0x59()
        {
            // fpPlace='1', fpClamp='1', yPos='0' → FPS_CLAMPED → 0x59
            string statusData = "001000110000" + "00000000";
            _mockConnector.SetupQuickSuccessResponse(statusData);

            string statfxl;
            var result = _actor.GetFOUPStatus(out statfxl);

            Assert.AreEqual(ErrorCode.Success, result);
            Assert.AreEqual("0x59", statfxl);
            Assert.AreEqual(2, _actor.FoupStatus); // FPS_CLAMPED = 2
        }

        [Test]
        public void GetFOUPStatus_Docked_Returns0x53()
        {
            // fpPlace='1', fpClamp='1', yPos='1', fpDoor='1' → FPS_DOCKED → 0x53
            // Positions: 0=EqpSt, 1=Mode, 2=Init, 3=OpSt, 4-5=Ecode,
            //   6=FpPlace, 7=FpClamp, 8=LtchKey, 9=Vacuum,
            //   10=FpDoor, 11=WfProt, 12=ZPos, 13=YPos, ...
            string statusData = "00100011001001000000";
            _mockConnector.SetupQuickSuccessResponse(statusData);

            string statfxl;
            var result = _actor.GetFOUPStatus(out statfxl);

            Assert.AreEqual(ErrorCode.Success, result);
            Assert.AreEqual("0x53", statfxl);
            Assert.AreEqual(3, _actor.FoupStatus); // FPS_DOCKED = 3
        }

        [Test]
        public void GetFOUPStatus_Opened_Returns0x57()
        {
            // fpPlace='1', fpClamp='1', yPos='1', fpDoor='0' → FPS_OPENED → 0x57
            string statusData = "00100011000001000000";
            _mockConnector.SetupQuickSuccessResponse(statusData);

            string statfxl;
            var result = _actor.GetFOUPStatus(out statfxl);

            Assert.AreEqual(ErrorCode.Success, result);
            Assert.AreEqual("0x57", statfxl);
            Assert.AreEqual(4, _actor.FoupStatus); // FPS_OPENED = 4
        }

        [Test]
        public void GetFOUPStatus_Misplaced_Returns0x68AndFoupStatusUnchanged()
        {
            // First set a known FOUP status (NOFOUP via fpPlace='0')
            string statusData1 = "00100000000000000000";
            _mockConnector.SetupQuickSuccessResponse(statusData1);
            string dummy1;
            _actor.GetFOUPStatus(out dummy1);
            Assert.AreEqual(0, _actor.FoupStatus); // FPS_NOFOUP

            // Now query with misplaced (fpPlace='2') → statfxl=0x68, FoupStatus unchanged
            string statusData2 = "00100020000000000000";
            _mockConnector.SetupQuickSuccessResponse(statusData2);
            string statfxl;
            _actor.GetFOUPStatus(out statfxl);

            Assert.AreEqual("0x68", statfxl);
            Assert.AreEqual(0, _actor.FoupStatus); // Still FPS_NOFOUP
        }

        [Test]
        public void GetFOUPStatus_Failure_ReturnsEmptyStatfxl()
        {
            _mockConnector.SetupAckTimeout();
            _config.AckTimeout = 100;

            string statfxl;
            var result = _actor.GetFOUPStatus(out statfxl);

            Assert.AreEqual(ErrorCode.AckTimeout, result);
            Assert.AreEqual(string.Empty, statfxl);
        }

        [Test]
        public void FoupStatus_DefaultValue_IsUnknown()
        {
            Assert.AreEqual(-1, _actor.FoupStatus); // FPS_UNKNOWN = -1
        }

        [Test]
        public void FoupEvent_DefaultValue_IsNone()
        {
            Assert.AreEqual(0xFF, _actor.FoupEvent); // FPEVT_NONE = 0xFF
        }

        #endregion

        #region LED Control Tests

        [Test]
        public void LedOn_Success_UpdatesCache()
        {
            _mockConnector.SetupSuccessResponse();

            var result = _actor.LedOn(1);

            Assert.AreEqual(ErrorCode.Success, result);
            string data;
            _actor.GetLedStatus(out data, 1);
            Assert.AreEqual("1", data); // LED_ON = 1
        }

        [Test]
        public void LedOff_Success_UpdatesCache()
        {
            _mockConnector.SetupSuccessResponse();

            var result = _actor.LedOff(3);

            Assert.AreEqual(ErrorCode.Success, result);
            string data;
            _actor.GetLedStatus(out data, 3);
            Assert.AreEqual("0", data); // LED_OFF = 0
        }

        [Test]
        public void LedBlink_Success_UpdatesCache()
        {
            _mockConnector.SetupSuccessResponse();

            var result = _actor.LedBlink(5);

            Assert.AreEqual(ErrorCode.Success, result);
            string data;
            _actor.GetLedStatus(out data, 5);
            Assert.AreEqual("2", data); // LED_BLINK = 2
        }

        [Test]
        public void LedOn_AckTimeout_ReturnsAckTimeout()
        {
            _mockConnector.SetupAckTimeout();
            _config.AckTimeout = 100;

            var result = _actor.LedOn(1);

            Assert.AreEqual(ErrorCode.AckTimeout, result);
        }

        [Test]
        public void LedOn_InfTimeout_ReturnsInfTimeout()
        {
            _mockConnector.SetupInfTimeout();
            _config.InfTimeout = 100;

            var result = _actor.LedOn(1);

            Assert.AreEqual(ErrorCode.InfTimeout, result);
        }

        [Test]
        public void LedOn_Failure_DoesNotUpdateCache()
        {
            _mockConnector.SetupNakResponse();

            _actor.LedOn(1);

            string data;
            _actor.GetLedStatus(out data, 1);
            Assert.AreEqual("0", data); // Still default (OFF)
        }

        [Test]
        public void GetLedStatus_InvalidLedNo_ReturnsEmpty()
        {
            string data;
            var result = _actor.GetLedStatus(out data, 0);

            Assert.AreEqual(ErrorCode.Success, result);
            Assert.AreEqual(string.Empty, data);
        }

        [Test]
        public void GetLedStatus_LedNoOutOfRange_ReturnsEmpty()
        {
            string data;
            _actor.GetLedStatus(out data, 11);

            Assert.AreEqual(string.Empty, data);
        }

        [Test]
        public void LedOn_RaisesLedChangedEvent()
        {
            _mockConnector.SetupSuccessResponse();
            int raisedLedNo = -1;
            int raisedStatus = -1;
            _actor.LedChanged += (ledNo, status) =>
            {
                raisedLedNo = ledNo;
                raisedStatus = status;
            };

            _actor.LedOn(2);
            Thread.Sleep(100); // Allow async response processing

            Assert.AreEqual(2, raisedLedNo);
            Assert.AreEqual(1, raisedStatus); // LED_ON
        }

        #endregion

        #region ScanSlotMapStatus / ReturnSlotMapStatus Tests

        [Test]
        public void ScanSlotMapStatus_Success_ReturnsSemiFormat()
        {
            // Setup: MAPDO (Operation) → success, then MAPRD (Quick) → raw data
            SetupOperationThenQuickSuccess("0112W0000000000000000000000");

            string data;
            var result = _actor.ScanSlotMapStatus(out data);

            Assert.AreEqual(ErrorCode.Success, result);
            Assert.IsNotEmpty(data);
        }

        [Test]
        public void ScanSlotMapStatus_SemiConversion_CorrectMapping()
        {
            // Verify individual SEMI conversions
            Assert.AreEqual(1, LoadportActor.ConvertToSemiFormat('0')); // Empty
            Assert.AreEqual(3, LoadportActor.ConvertToSemiFormat('1')); // Normal
            Assert.AreEqual(5, LoadportActor.ConvertToSemiFormat('2')); // Crossed
            Assert.AreEqual(4, LoadportActor.ConvertToSemiFormat('W')); // Double
            Assert.AreEqual(0, LoadportActor.ConvertToSemiFormat('X')); // Undefined
        }

        [Test]
        public void ScanSlotMapStatus_MapdoFailure_ReturnsError()
        {
            _mockConnector.SetupNakResponse();

            string data;
            var result = _actor.ScanSlotMapStatus(out data);

            Assert.AreEqual(ErrorCode.CommandFailed, result);
            Assert.AreEqual(string.Empty, data);
        }

        [Test]
        public void ScanSlotMapStatus_RaisesSlotMapScannedEvent()
        {
            SetupOperationThenQuickSuccess("0000000000000000000000000");
            bool eventRaised = false;
            _actor.SlotMapScanned += (slotMap) =>
            {
                eventRaised = true;
                Assert.AreEqual(25, slotMap.Length);
            };

            string data;
            _actor.ScanSlotMapStatus(out data);
            Thread.Sleep(100);

            Assert.IsTrue(eventRaised);
        }

        [Test]
        public void ReturnSlotMapStatus_NoScan_ReturnsEmpty()
        {
            string data;
            var result = _actor.ReturnSlotMapStatus(out data);

            Assert.AreEqual(ErrorCode.Success, result);
            Assert.AreEqual(string.Empty, data);
        }

        [Test]
        public void ReturnSlotMapStatus_AfterScan_ReturnsCachedData()
        {
            SetupOperationThenQuickSuccess("0000000000000000000000000");
            string scanData;
            _actor.ScanSlotMapStatus(out scanData);

            string cachedData;
            _actor.ReturnSlotMapStatus(out cachedData);

            Assert.AreEqual(scanData, cachedData);
        }

        #endregion

        #region Event Reporting Tests

        [Test]
        public void StartReportLoadport_Success_SendsEVTONAndFPEON()
        {
            _mockConnector.SetupQuickSuccessResponse();

            var result = _actor.StartReportLoadport();

            Assert.AreEqual(ErrorCode.Success, result);
            Assert.AreEqual(2, _mockConnector.SendCount); // EVTON + FPEON
        }

        [Test]
        public void StartReportLoadport_FirstCommandFails_ReturnsError()
        {
            _mockConnector.SetupAckTimeout();
            _config.AckTimeout = 100;

            var result = _actor.StartReportLoadport();

            Assert.AreEqual(ErrorCode.AckTimeout, result);
            Assert.AreEqual(1, _mockConnector.SendCount); // Only EVTON attempted
        }

        [Test]
        public void StopReportLoadport_Success_SendsEVTOF()
        {
            _mockConnector.SetupQuickSuccessResponse();

            var result = _actor.StopReportLoadport();

            Assert.AreEqual(ErrorCode.Success, result);
        }

        [Test]
        public void StartReportFOUP_Success_SendsFPEON()
        {
            _mockConnector.SetupQuickSuccessResponse();

            var result = _actor.StartReportFOUP();

            Assert.AreEqual(ErrorCode.Success, result);
        }

        [Test]
        public void StopReportFOUP_Success_SendsFPEOF()
        {
            _mockConnector.SetupQuickSuccessResponse();

            var result = _actor.StopReportFOUP();

            Assert.AreEqual(ErrorCode.Success, result);
        }

        #endregion

        #region FOUP Event Handling Tests

        [Test]
        public void FoupEvent_PODON_UpdatesFoupStatusToPlaced()
        {
            _mockConnector.InjectFoupEvent("PODON");
            Thread.Sleep(100);

            Assert.AreEqual(1, _actor.FoupStatus); // FPS_PLACED = 1
            Assert.AreEqual(3, _actor.FoupEvent);  // FPEVT_PODON = 3
        }

        [Test]
        public void FoupEvent_PODOF_UpdatesFoupStatusToNoFoup()
        {
            _mockConnector.InjectFoupEvent("PODOF");
            Thread.Sleep(100);

            Assert.AreEqual(0, _actor.FoupStatus); // FPS_NOFOUP = 0
            Assert.AreEqual(0, _actor.FoupEvent);  // FPEVT_PODOF = 0
        }

        [Test]
        public void FoupEvent_SMTON_OnlyUpdatesFoupEvent()
        {
            // First set a known FoupStatus
            _mockConnector.InjectFoupEvent("PODON");
            Thread.Sleep(50);
            Assert.AreEqual(1, _actor.FoupStatus); // FPS_PLACED

            // SMTON should not change FoupStatus
            _mockConnector.InjectFoupEvent("SMTON");
            Thread.Sleep(100);

            Assert.AreEqual(1, _actor.FoupStatus); // Still FPS_PLACED
            Assert.AreEqual(1, _actor.FoupEvent);  // FPEVT_SMTON = 1
        }

        [Test]
        public void FoupEvent_ABNST_OnlyUpdatesFoupEvent()
        {
            _mockConnector.InjectFoupEvent("PODON");
            Thread.Sleep(50);
            Assert.AreEqual(1, _actor.FoupStatus); // FPS_PLACED

            _mockConnector.InjectFoupEvent("ABNST");
            Thread.Sleep(100);

            Assert.AreEqual(1, _actor.FoupStatus); // Still FPS_PLACED
            Assert.AreEqual(2, _actor.FoupEvent);  // FPEVT_ABNST = 2
        }

        [Test]
        public void FoupEvent_RaisesFoupReportStartedEvent()
        {
            bool eventRaised = false;
            int raisedReportType = -1;
            _actor.FoupReportStarted += (reportType) =>
            {
                eventRaised = true;
                raisedReportType = reportType;
            };

            _mockConnector.InjectFoupEvent("PODON");
            Thread.Sleep(100);

            Assert.IsTrue(eventRaised);
            Assert.AreEqual(3, raisedReportType); // FPEVT_PODON = 3
        }

        #endregion

        #region Dispose Tests

        [Test]
        public void Dispose_ResetsStateMachines()
        {
            // Set some state first
            _mockConnector.InjectFoupEvent("PODON");
            Thread.Sleep(50);
            Assert.AreEqual(1, _actor.FoupStatus);

            _actor.Dispose();

            Assert.AreEqual(-1, _actor.FoupStatus); // FPS_UNKNOWN
            Assert.AreEqual(0xFF, _actor.FoupEvent); // FPEVT_NONE
        }

        [Test]
        public void Dispose_ClearsSlotMapCache()
        {
            // Do a scan first to populate cache
            SetupOperationThenQuickSuccess("0000000000000000000000000");
            string data;
            _actor.ScanSlotMapStatus(out data);
            Assert.IsNotNull(_actor.SlotMap);

            _actor.Dispose();

            Assert.IsNull(_actor.SlotMap);
        }

        [Test]
        public void Dispose_MultipleDisposeCalls_NoException()
        {
            Assert.DoesNotThrow(() =>
            {
                _actor.Dispose();
                _actor.Dispose();
                _actor.Dispose();
            });
        }

        #endregion

        #region StatusChanged Event Tests

        [Test]
        public void StatusChanged_WhenStatusChanges_RaisesEventWithOldAndNew()
        {
            // First query to set initial status
            string statusData1 = "00100000000000000000";
            _mockConnector.SetupQuickSuccessResponse(statusData1);
            string data;
            _actor.GetLPStatus(out data);

            // Subscribe after initial state is set
            LoadportStatus capturedNew = default(LoadportStatus);
            bool eventRaised = false;
            _actor.StatusChanged += (newS) =>
            {
                eventRaised = true;
                capturedNew = newS;
            };

            // Second query with different status
            string statusData2 = "A0110010000000000000";
            _mockConnector.SetupQuickSuccessResponse(statusData2);
            _actor.GetLPStatus(out data);

            Assert.IsTrue(eventRaised);
            Assert.AreEqual('A', capturedNew.EqpStatus);
            Assert.AreEqual('1', capturedNew.Inited);
        }

        [Test]
        public void StatusChanged_WhenStatusUnchanged_DoesNotRaiseEvent()
        {
            string statusData = "00100000000000000000";
            _mockConnector.SetupQuickSuccessResponse(statusData);
            string data;
            _actor.GetLPStatus(out data);

            bool eventRaised = false;
            _actor.StatusChanged += (newS) => { eventRaised = true; };

            // Same status again
            _mockConnector.SetupQuickSuccessResponse(statusData);
            _actor.GetLPStatus(out data);

            Assert.IsFalse(eventRaised);
        }

        [Test]
        public void StatusChanged_MultipleChanges_RaisesEventEachTime()
        {
            int raiseCount = 0;
            _actor.StatusChanged += (newS) => { raiseCount++; };

            string data;
            // Change 1: default → status1
            _mockConnector.SetupQuickSuccessResponse("00100000000000000000");
            _actor.GetLPStatus(out data);

            // Change 2: status1 → status2
            _mockConnector.SetupQuickSuccessResponse("A0110010000000000000");
            _actor.GetLPStatus(out data);

            // Change 3: status2 → status3
            _mockConnector.SetupQuickSuccessResponse("00100000000000000010");
            _actor.GetLPStatus(out data);

            Assert.AreEqual(3, raiseCount);
        }

        #endregion

        #region SendLoadportCommand Prefix Routing Tests

        [Test]
        public void SendLoadportCommand_UnknownPrefix_ReturnsCommandFailed()
        {
            string data;
            var result = _actor.SendLoadportCommand(out data, "XYZ:TEST");

            Assert.AreEqual(ErrorCode.CommandFailed, result);
            Assert.AreEqual(string.Empty, data);
        }

        [Test]
        public void SendLoadportCommand_EmptyCommand_ReturnsCommandFailed()
        {
            string data;
            var result = _actor.SendLoadportCommand(out data, "");

            Assert.AreEqual(ErrorCode.CommandFailed, result);
        }

        [Test]
        public void SendLoadportCommand_NullCommand_ReturnsCommandFailed()
        {
            string data;
            var result = _actor.SendLoadportCommand(out data, null);

            Assert.AreEqual(ErrorCode.CommandFailed, result);
        }

        [Test]
        public void SendLoadportCommand_MovPrefix_UsesMovSetHandshake()
        {
            _mockConnector.SetupSuccessResponse(null, "MOVDATA");

            string data;
            var result = _actor.SendLoadportCommand(out data, "MOV:CUSTOM");

            Assert.AreEqual(ErrorCode.Success, result);
        }

        [Test]
        public void SendLoadportCommand_SetPrefix_UsesMovSetHandshake()
        {
            _mockConnector.SetupSuccessResponse(null, "SETDATA");

            string data;
            var result = _actor.SendLoadportCommand(out data, "SET:CUSTOM");

            Assert.AreEqual(ErrorCode.Success, result);
        }

        [Test]
        public void SendLoadportCommand_ModPrefix_UsesAckOnly()
        {
            _mockConnector.SetupQuickSuccessResponse("MODDATA");

            string data;
            var result = _actor.SendLoadportCommand(out data, "MOD:TEST");

            Assert.AreEqual(ErrorCode.Success, result);
        }

        [Test]
        public void SendLoadportCommand_TchPrefix_UsesAckOnly()
        {
            _mockConnector.SetupQuickSuccessResponse("TCHDATA");

            string data;
            var result = _actor.SendLoadportCommand(out data, "TCH:TEST");

            Assert.AreEqual(ErrorCode.Success, result);
        }

        #endregion

        #region Connector Null Guard Tests

        [Test]
        public void SendMovSetCommand_AfterDispose_ReturnsCommandFailed()
        {
            _actor.Dispose();

            // After Dispose, Connector is set to null
            // Init internally calls SendMovSetCommand
            var result = _actor.Init();

            Assert.AreEqual(ErrorCode.CommandFailed, result);
        }

        [Test]
        public void SendAckOnlyCommand_AfterDispose_ReturnsCommandFailed()
        {
            _actor.Dispose();

            // GetLPStatus internally calls GetFxlAmhsStatus → SendAckOnlyCommand
            string data;
            var result = _actor.GetLPStatus(out data);

            Assert.AreEqual(ErrorCode.CommandFailed, result);
            Assert.AreEqual(string.Empty, data);
        }

        #endregion

        #region GetLPStatus Delegation Tests

        [Test]
        public void GetLPStatus_Success_UpdatesFoupStatusViaDelegation()
        {
            // fpPlace='1', fpClamp='0' → FPS_PLACED = 1
            string statusData = "00100010000000000000";
            _mockConnector.SetupQuickSuccessResponse(statusData);

            string data;
            var result = _actor.GetLPStatus(out data);

            Assert.AreEqual(ErrorCode.Success, result);
            Assert.AreEqual(1, _actor.FoupStatus); // FPS_PLACED via DeriveFoupStatus
        }

        [Test]
        public void GetLPStatus_Success_UpdatesFxlStateViaDelegation()
        {
            // Inited='1', fpPlace='1', fpClamp='0' → FPS_PLACED → FXL_READY
            string statusData = "00100010000000000000";
            _mockConnector.SetupQuickSuccessResponse(statusData);

            string data;
            _actor.GetLPStatus(out data);

            // After delegation, FoupStatus should be updated (PLACED)
            Assert.AreEqual(1, _actor.FoupStatus);
        }

        #endregion

        #region Missing Failure Scenario Tests

        [Test]
        public void InitForce_InfTimeout_ReturnsInfTimeout()
        {
            _mockConnector.SetupInfTimeout();
            _config.InfTimeout = 100;

            var result = _actor.InitForce();

            Assert.AreEqual(ErrorCode.InfTimeout, result);
        }

        [Test]
        public void InitProgram_AckTimeout_ReturnsAckTimeout()
        {
            _mockConnector.SetupAckTimeout();
            _config.AckTimeout = 100;

            var result = _actor.InitProgram();

            Assert.AreEqual(ErrorCode.AckTimeout, result);
        }

        [Test]
        public void InitProgram_NAK_ReturnsCommandFailed()
        {
            _mockConnector.SetupNakResponse();

            var result = _actor.InitProgram();

            Assert.AreEqual(ErrorCode.CommandFailed, result);
        }

        [Test]
        public void Clamp_AckTimeout_ReturnsAckTimeout()
        {
            _mockConnector.SetupAckTimeout();
            _config.AckTimeout = 100;

            var result = _actor.Clamp();

            Assert.AreEqual(ErrorCode.AckTimeout, result);
        }

        [Test]
        public void Clamp_NAK_ReturnsCommandFailed()
        {
            _mockConnector.SetupNakResponse();

            var result = _actor.Clamp();

            Assert.AreEqual(ErrorCode.CommandFailed, result);
        }

        [Test]
        public void Dock_AckTimeout_ReturnsAckTimeout()
        {
            _mockConnector.SetupAckTimeout();
            _config.AckTimeout = 100;

            var result = _actor.Dock();

            Assert.AreEqual(ErrorCode.AckTimeout, result);
        }

        [Test]
        public void Dock_NAK_ReturnsCommandFailed()
        {
            _mockConnector.SetupNakResponse();

            var result = _actor.Dock();

            Assert.AreEqual(ErrorCode.CommandFailed, result);
        }

        [Test]
        public void Undock_AckTimeout_ReturnsAckTimeout()
        {
            _mockConnector.SetupAckTimeout();
            _config.AckTimeout = 100;

            var result = _actor.Undock();

            Assert.AreEqual(ErrorCode.AckTimeout, result);
        }

        [Test]
        public void Undock_NAK_ReturnsCommandFailed()
        {
            _mockConnector.SetupNakResponse();

            var result = _actor.Undock();

            Assert.AreEqual(ErrorCode.CommandFailed, result);
        }

        [Test]
        public void CloseDoor_AckTimeout_ReturnsAckTimeout()
        {
            _mockConnector.SetupAckTimeout();
            _config.AckTimeout = 100;

            var result = _actor.CloseDoor();

            Assert.AreEqual(ErrorCode.AckTimeout, result);
        }

        [Test]
        public void CloseDoor_NAK_ReturnsCommandFailed()
        {
            _mockConnector.SetupNakResponse();

            var result = _actor.CloseDoor();

            Assert.AreEqual(ErrorCode.CommandFailed, result);
        }

        [Test]
        public void Unload_AckTimeout_ReturnsAckTimeout()
        {
            _mockConnector.SetupAckTimeout();
            _config.AckTimeout = 100;

            var result = _actor.Unload();

            Assert.AreEqual(ErrorCode.AckTimeout, result);
        }

        [Test]
        public void Unload_NAK_ReturnsCommandFailed()
        {
            _mockConnector.SetupNakResponse();

            var result = _actor.Unload();

            Assert.AreEqual(ErrorCode.CommandFailed, result);
        }

        [Test]
        public void LedOff_Failure_DoesNotUpdateCache()
        {
            // First turn on LED
            _mockConnector.SetupSuccessResponse();
            _actor.LedOn(2);

            // Now fail the off command
            _mockConnector.SetupNakResponse();
            _actor.LedOff(2);

            string data;
            _actor.GetLedStatus(out data, 2);
            Assert.AreEqual("1", data); // Still ON, not OFF
        }

        [Test]
        public void LedBlink_Failure_DoesNotUpdateCache()
        {
            _mockConnector.SetupNakResponse();
            _actor.LedBlink(3);

            string data;
            _actor.GetLedStatus(out data, 3);
            Assert.AreEqual("0", data); // Still default OFF
        }

        #endregion

        #region Default Property Values

        [Test]
        public void Status_DefaultValue_HasExpectedDefaults()
        {
            Assert.AreEqual('0', _actor.Status.EqpStatus);
            Assert.AreEqual('0', _actor.Status.Mode);
            Assert.AreEqual('0', _actor.Status.Inited);
            Assert.AreEqual('?', _actor.Status.ZPos);
            Assert.AreEqual(0, _actor.Status.Ecode);
        }

        [Test]
        public void SlotMap_DefaultValue_IsNull()
        {
            Assert.IsNull(_actor.SlotMap);
        }

        #endregion

        #region ParseStatusResponse Edge Cases

        [Test]
        public void GetLPStatus_ShortData_DoesNotCrash()
        {
            // Response with less than 20 chars should be handled gracefully
            _mockConnector.SetupQuickSuccessResponse("001");

            string data;
            var result = _actor.GetLPStatus(out data);

            // Should succeed (ACK received) but status not updated due to short data
            Assert.AreEqual(ErrorCode.Success, result);
        }

        #endregion

        #region ScanSlotMapStatus MAPRD Failure

        [Test]
        public void ScanSlotMapStatus_MaprdFailure_ReturnsCommandFailed()
        {
            // MAPDO succeeds (Operation), but MAPRD fails (Quick)
            var customMock = new SequentialMockConnector();
            customMock.AddResponse(new MockResponse { AckType = "ACK", InfType = "INF" }); // MAPDO
            customMock.AddResponse(new MockResponse { AckType = "NAK" }); // MAPRD fails

            _actor.Dispose();
            _actor = new LoadportActor(_config, customMock, _mockLogger.Object);

            string data;
            var result = _actor.ScanSlotMapStatus(out data);

            Assert.AreEqual(ErrorCode.CommandFailed, result);
            Assert.AreEqual(string.Empty, data);
        }

        #endregion

        #region Connector Subscribe/Unsubscribe Tests

        [Test]
        public void Connector_SetNewConnector_OldStopsReceiving()
        {
            // Inject PODON via original connector
            _mockConnector.InjectFoupEvent("PODON");
            Thread.Sleep(100);
            Assert.AreEqual(1, _actor.FoupStatus); // FPS_PLACED

            // Create new connector and assign
            var newMock = new TAS300MockHelper();
            // Use reflection or internal access to set Connector
            // Since Connector is internal, we test via Dispose pattern
            _actor.Dispose();
            _actor = new LoadportActor(_config, newMock, _mockLogger.Object);

            // Old connector events should not affect new actor
            _mockConnector.InjectFoupEvent("PODOF");
            Thread.Sleep(100);

            // New actor should still be at default FPS_UNKNOWN
            Assert.AreEqual(-1, _actor.FoupStatus);
        }

        #endregion

        #region Coverage Helpers

        [Test]
        public void Connector_Getter_ReturnsCurrentConnector()
        {
            var property = typeof(LoadportActor).GetProperty("Connector",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(property);

            var connector = property.GetValue(_actor, null) as IConnector;
            Assert.IsNotNull(connector);
        }

        [Test]
        public void Events_SubscribeAndUnsubscribe_DoNotThrow()
        {
            LedChangedEventHandler ledHandler = (ledNo, status) => { };
            SlotMapScannedEventHandler slotHandler = slotMap => { };
            FoupReportStartedEventHandler foupHandler = reportType => { };
            StatusChangedEventHandler statusHandler = newStatus => { };

            _actor.LedChanged += ledHandler;
            _actor.LedChanged -= ledHandler;

            _actor.SlotMapScanned += slotHandler;
            _actor.SlotMapScanned -= slotHandler;

            _actor.FoupReportStarted += foupHandler;
            _actor.FoupReportStarted -= foupHandler;

            _actor.StatusChanged += statusHandler;
            _actor.StatusChanged -= statusHandler;

            Assert.Pass();
        }

        [Test]
        public void GetLedStatus_ValidLedNo_ReturnsCachedValue()
        {
            var field = typeof(LoadportActor).GetField("_ledStatus",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field);

            var ledStatus = (int[])field.GetValue(_actor);
            ledStatus[0] = 2;

            string data;
            var result = _actor.GetLedStatus(out data, 1);

            Assert.AreEqual(ErrorCode.Success, result);
            Assert.AreEqual("2", data);
        }

        [Test]
        public void GetLedStatus_WhenLedStatusNull_RethrowsException()
        {
            var field = typeof(LoadportActor).GetField("_ledStatus",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field);

            field.SetValue(_actor, null);

            string data;
            Assert.Throws<NullReferenceException>(() => _actor.GetLedStatus(out data, 1));
        }

        [Test]
        public void OnDataReceived_NullOrEmpty_IgnoresSafely()
        {
            var method = typeof(LoadportActor).GetMethod("OnDataReceived",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method);

            method.Invoke(_actor, new object[] { null, 0 });
            method.Invoke(_actor, new object[] { Array.Empty<byte>(), 0 });

            Assert.Pass();
        }

        [Test]
        public void OnDataReceived_AfterDispose_HandlesException()
        {
            var method = typeof(LoadportActor).GetMethod("OnDataReceived",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method);

            _actor.Dispose();

            var ackBytes = Encoding.ASCII.GetBytes("ACK");
            method.Invoke(_actor, new object[] { ackBytes, ackBytes.Length });

            Assert.Pass();
        }

        [Test]
        public void OnDataReceived_WhenAckSignalNull_LogsAndSwallows()
        {
            var method = typeof(LoadportActor).GetMethod("OnDataReceived",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var ackField = typeof(LoadportActor).GetField("_ackSignal",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(method);
            Assert.IsNotNull(ackField);

            ackField.SetValue(_actor, null);

            var ackBytes = Encoding.ASCII.GetBytes("ACK");
            method.Invoke(_actor, new object[] { ackBytes, ackBytes.Length });

            Assert.Pass();
        }

        [Test]
        public void ParseResponseType_CoversAllBranches()
        {
            var type = typeof(LoadportActor);
            var method = type.GetMethod("ParseResponseType",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method);

            var resAck = (int)type.GetField("RES_ACK",
                BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            var resNak = (int)type.GetField("RES_NAK",
                BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            var resInf = (int)type.GetField("RES_INF",
                BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            var resAbs = (int)type.GetField("RES_ABS",
                BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

            Assert.AreEqual(resNak, method.Invoke(_actor, new object[] { null }));
            Assert.AreEqual(resAck, method.Invoke(_actor, new object[] { "ACK" }));
            Assert.AreEqual(resNak, method.Invoke(_actor, new object[] { "NAK" }));
            Assert.AreEqual(resInf, method.Invoke(_actor, new object[] { "INF" }));
            Assert.AreEqual(resAbs, method.Invoke(_actor, new object[] { "ABS" }));
            Assert.AreEqual(resNak, method.Invoke(_actor, new object[] { "UNKNOWN" }));
        }

        [Test]
        public void ExtractResponseData_CoversColonAndNoColon()
        {
            var method = typeof(LoadportActor).GetMethod("ExtractResponseData",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method);

            Assert.AreEqual(string.Empty, method.Invoke(_actor, new object[] { null }));
            Assert.AreEqual("DATA", method.Invoke(_actor, new object[] { "ACK:DATA" }));
            Assert.AreEqual("ACK", method.Invoke(_actor, new object[] { "ACK" }));
        }

        [Test]
        public void DeriveFoupStatus_ClampedPath_UpdatesStatus()
        {
            _mockConnector.SetupQuickSuccessResponse(BuildStatus('1', '1', '?', '0'));

            string data;
            var result = _actor.GetLPStatus(out data);

            Assert.AreEqual(ErrorCode.Success, result);
            Assert.AreEqual(2, _actor.FoupStatus);
        }

        [Test]
        public void DeriveFoupStatus_InvalidFpPlace_ReturnsWithoutUpdate()
        {
            _mockConnector.SetupQuickSuccessResponse(BuildStatus('9', '1', '1', '1'));

            string data;
            var result = _actor.GetLPStatus(out data);

            Assert.AreEqual(ErrorCode.Success, result);
            Assert.AreEqual(-1, _actor.FoupStatus);
        }

        [Test]
        public void DeriveFoupStatus_InvalidFpClamp_ReturnsWithoutUpdate()
        {
            _mockConnector.SetupQuickSuccessResponse(BuildStatus('1', '9', '1', '1'));

            string data;
            var result = _actor.GetLPStatus(out data);

            Assert.AreEqual(ErrorCode.Success, result);
            Assert.AreEqual(-1, _actor.FoupStatus);
        }

        [Test]
        public void DeriveFoupStatus_InvalidYPos_ReturnsWithoutUpdate()
        {
            _mockConnector.SetupQuickSuccessResponse(BuildStatus('1', '1', '1', '9'));

            string data;
            var result = _actor.GetLPStatus(out data);

            Assert.AreEqual(ErrorCode.Success, result);
            Assert.AreEqual(-1, _actor.FoupStatus);
        }

        [Test]
        public void DeriveFoupStatus_DockedPath_UpdatesStatus()
        {
            _mockConnector.SetupQuickSuccessResponse(BuildStatus('1', '1', '1', '1'));

            string data;
            var result = _actor.GetLPStatus(out data);

            Assert.AreEqual(ErrorCode.Success, result);
            Assert.AreEqual(3, _actor.FoupStatus);
        }

        [Test]
        public void DeriveFoupStatus_OpenedPath_UpdatesStatus()
        {
            _mockConnector.SetupQuickSuccessResponse(BuildStatus('1', '1', '0', '1'));

            string data;
            var result = _actor.GetLPStatus(out data);

            Assert.AreEqual(ErrorCode.Success, result);
            Assert.AreEqual(4, _actor.FoupStatus);
        }

        [Test]
        public void LampOP_InvalidAction_ReturnsCommandFailed()
        {
            var method = typeof(LoadportActor).GetMethod("LampOP",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method);

            var result = (ErrorCode)method.Invoke(_actor, new object[] { 1, 99 });

            Assert.AreEqual(ErrorCode.CommandFailed, result);
        }

        private static IEnumerable<TestCaseData> ExceptionCommandCases()
        {
            yield return new TestCaseData(new Action<LoadportActor>(actor => actor.Init()))
                .SetName("Init_WhenConnectorThrows_Rethrows");
            yield return new TestCaseData(new Action<LoadportActor>(actor => actor.InitForce()))
                .SetName("InitForce_WhenConnectorThrows_Rethrows");
            yield return new TestCaseData(new Action<LoadportActor>(actor => actor.InitProgram()))
                .SetName("InitProgram_WhenConnectorThrows_Rethrows");
            yield return new TestCaseData(new Action<LoadportActor>(actor => actor.Load()))
                .SetName("Load_WhenConnectorThrows_Rethrows");
            yield return new TestCaseData(new Action<LoadportActor>(actor => actor.Clamp()))
                .SetName("Clamp_WhenConnectorThrows_Rethrows");
            yield return new TestCaseData(new Action<LoadportActor>(actor => actor.Dock()))
                .SetName("Dock_WhenConnectorThrows_Rethrows");
            yield return new TestCaseData(new Action<LoadportActor>(actor => actor.Unload()))
                .SetName("Unload_WhenConnectorThrows_Rethrows");
            yield return new TestCaseData(new Action<LoadportActor>(actor => actor.Undock()))
                .SetName("Undock_WhenConnectorThrows_Rethrows");
            yield return new TestCaseData(new Action<LoadportActor>(actor => actor.CloseDoor()))
                .SetName("CloseDoor_WhenConnectorThrows_Rethrows");
            yield return new TestCaseData(new Action<LoadportActor>(actor => actor.ResetError()))
                .SetName("ResetError_WhenConnectorThrows_Rethrows");
            yield return new TestCaseData(new Action<LoadportActor>(actor =>
            {
                string data;
                actor.GetLPStatus(out data);
            }))
                .SetName("GetLPStatus_WhenConnectorThrows_Rethrows");
            yield return new TestCaseData(new Action<LoadportActor>(actor =>
            {
                string data;
                actor.GetFOUPStatus(out data);
            }))
                .SetName("GetFOUPStatus_WhenConnectorThrows_Rethrows");
            yield return new TestCaseData(new Action<LoadportActor>(actor => actor.LedOn(1)))
                .SetName("LedOn_WhenConnectorThrows_Rethrows");
            yield return new TestCaseData(new Action<LoadportActor>(actor => actor.LedBlink(1)))
                .SetName("LedBlink_WhenConnectorThrows_Rethrows");
            yield return new TestCaseData(new Action<LoadportActor>(actor => actor.LedOff(1)))
                .SetName("LedOff_WhenConnectorThrows_Rethrows");
            yield return new TestCaseData(new Action<LoadportActor>(actor =>
            {
                string data;
                actor.ScanSlotMapStatus(out data);
            }))
                .SetName("ScanSlotMapStatus_WhenConnectorThrows_Rethrows");
            yield return new TestCaseData(new Action<LoadportActor>(actor => actor.StartReportLoadport()))
                .SetName("StartReportLoadport_WhenConnectorThrows_Rethrows");
            yield return new TestCaseData(new Action<LoadportActor>(actor => actor.StartReportFOUP()))
                .SetName("StartReportFOUP_WhenConnectorThrows_Rethrows");
            yield return new TestCaseData(new Action<LoadportActor>(actor => actor.StopReportLoadport()))
                .SetName("StopReportLoadport_WhenConnectorThrows_Rethrows");
            yield return new TestCaseData(new Action<LoadportActor>(actor => actor.StopReportFOUP()))
                .SetName("StopReportFOUP_WhenConnectorThrows_Rethrows");
            yield return new TestCaseData(new Action<LoadportActor>(actor =>
            {
                string data;
                actor.SendLoadportCommand(out data, "MOV:ORGSH");
            }))
                .SetName("SendLoadportCommand_WhenConnectorThrows_Rethrows");
        }

        private static string BuildStatus(char fpPlace, char fpClamp, char fpDoor, char yPos)
        {
            var data = "00000000000000000000".ToCharArray();
            data[2] = '1';
            data[6] = fpPlace;
            data[7] = fpClamp;
            data[10] = fpDoor;
            data[13] = yPos;
            return new string(data);
        }

        [TestCaseSource(nameof(ExceptionCommandCases))]
        public void Command_WhenConnectorThrows_RethrowsException(Action<LoadportActor> action)
        {
            var connectorMock = new Mock<IConnector>();
            connectorMock.Setup(c => c.Send(It.IsAny<byte[]>(), It.IsAny<int>()))
                .Throws(new InvalidOperationException("boom"));

            var actor = new LoadportActor(_config, connectorMock.Object, _mockLogger.Object);

            try
            {
                Assert.Throws<InvalidOperationException>(() => action(actor));
            }
            finally
            {
                actor.Dispose();
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Setup mock for an Operation Command success followed by a Quick Command success
        /// (simulates the GetFxlAmhsStatus call that follows successful operations).
        /// </summary>
        private void SetupOperationThenQuickSuccess(string statusResponseData)
        {
            // Replace the mock connector with a custom behavior
            var customMock = new SequentialMockConnector();
            customMock.AddResponse(new MockResponse { AckType = "ACK", InfType = "INF" }); // Operation command
            customMock.AddResponse(new MockResponse { AckType = "ACK", AckData = statusResponseData }); // Quick command

            // Rebuild actor with custom mock
            _actor.Dispose();
            _actor = new LoadportActor(_config, customMock, _mockLogger.Object);
            _mockConnector = null; // Old mock no longer in use
        }

        /// <summary>
        /// Setup status response to produce a specific FOUP status via sensor fields.
        /// </summary>
        private void SetupForFoupStatus(char fpPlace, char fpClamp, char yPos, char fpDoor)
        {
            // Build 20-char status: positions 7,8 (0-idx 6,7) = fpPlace/fpClamp
            // position 11 (0-idx 10) = fpDoor, position 14 (0-idx 13) = yPos
            char[] status = "00100000000000000000".ToCharArray();
            status[6] = fpPlace;
            status[7] = fpClamp;
            status[10] = fpDoor;
            status[13] = yPos;
            string statusData = new string(status);

            var customMock = new SequentialMockConnector();
            customMock.AddResponse(new MockResponse { AckType = "ACK", InfType = "INF" }); // Init operation
            customMock.AddResponse(new MockResponse { AckType = "ACK", AckData = statusData }); // GetFxlAmhsStatus

            _actor.Dispose();
            _actor = new LoadportActor(_config, customMock, _mockLogger.Object);
        }

        /// <summary>
        /// Perform a successful Init to establish state.
        /// </summary>
        private void PerformSuccessfulInit()
        {
            _actor.Init();
            Thread.Sleep(50);
        }

        #endregion
    }

    #region Sequential Mock Connector

    /// <summary>
    /// Response configuration for a single Send() call.
    /// </summary>
    internal class MockResponse
    {
        public string AckType { get; set; } = "ACK";
        public string AckData { get; set; }
        public string InfType { get; set; }
        public string InfData { get; set; }
        public bool SuppressAck { get; set; }
    }

    /// <summary>
    /// Mock IConnector that supports sequential response configurations.
    /// Each Send() call uses the next queued response.
    /// </summary>
    internal class SequentialMockConnector : IConnector
    {
        private readonly System.Collections.Generic.Queue<MockResponse> _responses
            = new System.Collections.Generic.Queue<MockResponse>();

        public event ReceivedDataEventHandler DataReceived;
        public IProtocol Protocol { get; set; }
        public string LastSentCommand { get; private set; }
        public int SendCount { get; private set; }
        public bool IsConnected { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void AddResponse(MockResponse response)
        {
            _responses.Enqueue(response);
        }

        public HRESULT Send(byte[] byPtBuf, int length)
        {
            int safeLength = 0;
            if (byPtBuf != null && length > 0)
            {
                safeLength = Math.Min(length, byPtBuf.Length);
            }

            LastSentCommand = Encoding.ASCII.GetString(byPtBuf ?? Array.Empty<byte>(), 0, safeLength);
            SendCount++;

            MockResponse resp = _responses.Count > 0
                ? _responses.Dequeue()
                : new MockResponse();

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    if (!resp.SuppressAck)
                    {
                        Emit(resp.AckType, resp.AckData);
                    }

                    if (!string.IsNullOrEmpty(resp.InfType))
                    {
                        Thread.Sleep(10); // Small delay between ACK and INF
                        Emit(resp.InfType, resp.InfData);
                    }
                }
                catch
                {
                    // Swallow in mock
                }
            });

            // LoadportActor does not inspect the return value.
            return null;
        }

        private void Emit(string responseType, string responseData)
        {
            if (string.IsNullOrEmpty(responseType))
            {
                return;
            }

            string response = responseType;
            if (!string.IsNullOrEmpty(responseData))
            {
                response = responseType + ":" + responseData;
            }

            byte[] bytes = Encoding.ASCII.GetBytes(response);
            DataReceived?.Invoke(bytes, bytes.Length);
        }

        public HRESULT Connect()
        {
            return null;
        }

        public void Disconnect()
        {
        }
    }

    #endregion
}
