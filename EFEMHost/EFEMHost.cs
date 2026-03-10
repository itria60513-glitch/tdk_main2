using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using EFEMInterface;
using EFEM.LogUtilities;
using EFEM.DataCenter;
using EFEM.ExceptionManagements;
using EFEM.FileUtilities;
using EFEMInterface.CommunicationChannel;
using EFEMMachineControl;
using EFEM.VariableCenter;
using System.Timers;

namespace EFEMHost
{
    [HMIAuthor("Lawrence Ouyang", version = 1.0, Remark = "2015/07/30: Baseline.")]
    [HMIAuthor("Leo", version = 1.1, Remark = "2015/09/17: Robot command call match completed.")]
    [HMIAuthor("Leo", version = 1.2, Remark = "2016/02/15: Support FFU New/Old Alarm Code and corresponding Version to Host.")]
    [HMIAuthor("Leo", version = 1.3, Remark = "2016/03/21: Support New/Old CHKWFR command format in different machine type.")]
    [HMIAuthor("Leo", version = 1.4, Remark = "2017/05/16: Under SimuRorzeRobot.NoSimulate, Support Arm_A(UpperArm) and ArmB(LowerArm) command.")]
    public class HostProxy
    {
        private bool disposed = false;
        private bool _IsHostInitialized = false;
        private bool _IsHostReady = false;
        private bool _IsHostOnline = false;
        private bool _IsHostConnected = false;
        private bool _IsAnyErrorExist = false;
        private bool _IsBusy = false;
        private bool _IsEFEMStateEMS = false;        

        private HostSyncEvnet _syncObj = new HostSyncEvnet();

        private AbstractExceptionManagement _ExManager = null;
        private ExceptionDictionary _ExDictionary = null;
        private AbstractLogUtility _log = null;
        private AbstractFileUtilities _fu = null;
        private EFEMVariableCenter _varCenter = null;

        private AbstractMachineControl _mc = null;
        //private AnalogIOBoard _AIO = null; //Analog I/O board
        private object _lockInit = new object();
        private object _lockSendCommand = new object();
        private object _lockDecodeCommand = new object();
        protected object _lockDIInterlock = new object();
        private object _lockBatteryCheck = new object();
        private object _lockFFUCheck = new object();
        private string objectName = ConstVC.ObjectName.HostProxy;
        private CommChannel _hostComm = null;
        private bool[] _curDIO0Input = null;
        private int ReperrToHostInterval = 3; //Second,The time interval for EFEM to Report Error to Host

        private System.Timers.Timer Timer_SendReady = null;                
        private System.Timers.Timer Timer_SendDoorError = null;
        private System.Timers.Timer Timer_Reperr = null;

        private int iSendReadyTimeInSecond = 5; //Second,The time interval when EFEM send "INF:READY/COMM" to Host to establish communication.
        private bool SupportNewFFUAlarmCode = false; //[False(Default):Old ErrorCode Rule(16000+1,2,3,4,5...n)] , [True:New ErrorCode Rule(16000+1,2,4,8,16,32,64,128...Power(2,n))]
        private string VersionToSimulateRorze = null;
        private string VersionToSimulateRorze_OldFFUAlarmCode = "6.0.1"; //Meet MachineControl's "bNewWAFCHECKVersion = true" and " bNewFFUAlarmCode = false"

        //TODO:If want to simulate "NewFFUAlarmCode" version , should use "EFEMExceptionInfo_NewFFUErrorCode_Ver611.xml" to match error code setting.
        private string VersionToSimulateRorze_NewFFUAlarmCode = "6.1.1"; //Meet MachineControl's "bNewWAFCHECKVersion = true" and " bNewFFUAlarmCode = true"        
        private bool _ForceToReGetStatus = false;

        //TODO:remember to set below flag to [false] when real machine status is OK to handle.        
        private const bool DEBUG_SimulateLPSlotCount = false; //[True:Simulate slot number=25],[*False:Depend on MachineControl's GetSlotCount()]
        private const bool DEBUG_Bypass_CANCheck = false;     //[True:Bypass CAN condition check],[*False:Check real machine's condition]
        private const bool DEBUG_Enable_TimeCalc = false;     //[True:Enable to calculate time spent],[*False:Disable]

        //For BackwardCompatible , OPTION item to meet different machine type & s/w version.
        //("*":Default setting)
        private const bool OPTION_EnableRobotPostAction = false; //[*False:Disable],[True:Report "INF:REPORT/WAF_Put/P501"]
        private const bool OPTION_NewFormat_CHKWFR = true;       //[*True:CHKWFR format with STATUS bits],[False:CHKWFR without STATUS bits]


        public event HostStatusChanged OnHostOnLineStatusChanged;        

        private class HostSyncEvnet : SyncEvent
        {

        }

        private HostProxy()
            : base()
        {

        }

        public bool IsHostInitialized
        {
            get { return this._IsHostInitialized; }
        }
        public bool IsHostReady
        {
            get { return this._IsHostReady && this._IsHostInitialized; }
        }
        public bool IsHostOnline
        {
            get { return this._IsHostOnline; }
            private set
            {
                this._IsHostOnline = value;
                if (OnHostOnLineStatusChanged != null)
                {
                    foreach (HostStatusChanged action in OnHostOnLineStatusChanged.GetInvocationList())
                    {
                        try
                        {
                            action.BeginInvoke(this._IsHostOnline, null, null);
                        }
                        catch (Exception e)
                        {
                            WriteLog(LogHeadType.Exception, string.Format("Exception occurs in set IsHostOnline. Delegate Source: {0}. Reason: {1}", action.ToString(), e.Message));
                        }
                    }
                }
            }
        }
        public bool IsHostConnected
        {
            get { return this._IsHostConnected; }
            private set
            {
                this._IsHostConnected = value;
            }
        }

        public bool IsAnyErrorExist
        {
            get 
            {
                return (_IsAnyErrorExist || STATUS_IsRobotError || STATUS_IsAlignerError);             
            }
        }

        public bool IsBusy
        {
            get
            {
                if (_mc != null)
                    return (_mc.IsEFEMBusy || this._IsBusy);
                else
                    return this._IsBusy;
            }
        }
        public bool IsHostCommandAllow
        {
            get 
            {
                if (_mc != null)
                    return _mc.IsHostCommandAllow;
                else
                    return false;
            }
        }
        public bool IsEFEMStateEMS
        {
            get { return _IsEFEMStateEMS; }
            private set
            {
                this._IsEFEMStateEMS = value;                        
            }                    
        }

        public int GetLoadportCount
        {
            get
            {
                if (_mc != null && SimulateRorzeRobot == (int)SimuRorzeRobot.NoSimulate)
                {
                    return _mc.LoadPortCount;
                }
                else
                {
                    return 2; //2 Loadport
                }
            }
        }

        public bool IsBatteryAbnormal
        {
            get
            {
                if (_mc != null)
                    return (BatteryErr_Controller || BatteryErr_Robot);
                else
                    return true;
            }
        }

        public bool ForceToReGetStatus
        {
            get
            {
                return _ForceToReGetStatus;
            }
        }

        public bool EFEMHostOnlineMode
        {
            get
            {
                return (_mc.IsInRunningMode && _mc.IsOnlineMode);
            }
        
        }

        public ArrayList InstantiateObjects()      //Step 1
        {
            Monitor.Enter(_lockInit);
            ArrayList al = new ArrayList();
            HRESULT hr = null;
            try
            {                                
                _log = LogUtility.GetUniqueInstance();
                WriteLog(LogHeadType.System_NewStart, "");

                _fu = FileUtility.GetUniqueInstance();
                _ExManager = ExceptionManagement.GetUniqueInstance();
                _ExDictionary = _ExManager.GetExceptionDictionary(objectName);

                _varCenter = EFEMVariableCenter.GetUniqueInstance();

                _mc = MachineControlFactory.GetUniqueInstance(MCOwner.HostProxy);

                bool updateConfig = false;                
                string setting = null;

                int iHostType = -2; //-2: unknown
                #region Get Host Type
                updateConfig = false;
                setting = _fu.GetPrivateProfileString("EFEMControl", "HostType");
                if (string.IsNullOrWhiteSpace(setting))
                    updateConfig = true;
                else
                {
                    if (!int.TryParse(setting, out iHostType))
                        updateConfig = true;
                    else
                    {
                        if (iHostType < (int)HostType.EMULATOR || iHostType > (int)HostType.HMI)
                            updateConfig = true;
                        else
                            updateConfig = false;
                    }
                }

                if (updateConfig)
                {
                    iHostType = (int)HostType;
                    _fu.WritePrivateProfileString("EFEMControl", "HostType", iHostType.ToString());
                }
                else
                {
                    HostType = (HostType)iHostType;
                }
                #endregion

                int iReperrToHostInterval = -1;//Not Defined
                #region Get Reperr(Report Error) to Host Timer Interval
                updateConfig = false;
                setting = _fu.GetPrivateProfileString("EFEMControl", "ReperrToHostInterval");
                if (string.IsNullOrWhiteSpace(setting))
                    updateConfig = true;
                else
                {
                    if (!int.TryParse(setting, out iReperrToHostInterval))
                        updateConfig = true;
                    else
                    {
                        if (iReperrToHostInterval <=0)
                            updateConfig = true;
                        else
                            updateConfig = false;
                    }
                }

                if (updateConfig)
                {
                    iReperrToHostInterval = ReperrToHostInterval;
                    _fu.WritePrivateProfileString("EFEMControl", "ReperrToHostInterval", iReperrToHostInterval.ToString());
                }
                else
                {
                    ReperrToHostInterval = iReperrToHostInterval;
                }
                #endregion
                                
                //////////////////////////////////////////                
                string value = _fu.GetPrivateProfileString("Communication", "Host");
                int commType = 0;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    try
                    {
                        commType = Convert.ToInt32(value);
                        if (commType != 0 && commType != 1)
                            commType = 0;
                    }
                    catch
                    {
                        commType = 0;
                    }
                }

                switch (commType)
                {
                    case (int)ConstVC.CommunicationType.TCPTP:
                        {
                            WriteLog(LogHeadType.Info, "Host Communication Type: [TCPIP]");
                            _hostComm = new TcpServer(new CommFormatRorze());
                        }
                        break;

                    case (int)ConstVC.CommunicationType.RS232:
                    default:
                        {
                            WriteLog(LogHeadType.Info, "Host Communication Type: [RS232]");
                            _hostComm = new Rs232b(new CommFormatRorze());
                        }
                        break;                    
                }                
                
                hr = _hostComm.Initialize("HostComm");

                if (hr == null)
                {
                    _hostComm.DataReceived += new DataReceivedEventHandler(HostComm_DataReceived);
                    _IsHostInitialized = true;
                    _varCenter.SetValueAndFireCallback("[ST]HostInitialized", true);
                }
                else
                {
                    string errMsg = "Fail to create comm port!";
                    WriteLog(LogHeadType.Exception, errMsg + " Reason: " + hr._message);
                    al.Add("[HostComm]" + hr._message);
                    _IsHostInitialized = false;
                    _varCenter.SetValueAndFireCallback("[ST]HostInitialized", false);
                }
                //////////////////////////////////////////

                if (al.Count > 0)
                    return al;
                else
                    return null;
            }
            catch (Exception e)
            {
                al.Add(e.Message);
                return al;
            }
            finally
            {
                Monitor.Exit(_lockInit);
            }
        }

        public ArrayList EstablishCommunications() //Step 2
        {
            Monitor.Enter(_lockInit);
            WriteLog(LogHeadType.CallStart, "", "EstablishCommunications()");
            ArrayList al = new ArrayList();

            try
            {
                if (_hostComm != null)
                {
                    HRESULT hr = _hostComm.Connect();
                    if (hr == null)
                    {
                        IsHostConnected = true;
                        _varCenter.SetValueAndFireCallback("[ST]HostConnected", true);
                    }
                    else
                    {
                        IsHostConnected = false;
                        string errMsg = "Fail to connect to Host Computer!";
                        WriteLog(LogHeadType.Exception, errMsg + " Reason: " + hr._message);
                        al.Add("[HostComm]" + hr._message);
                        _varCenter.SetValueAndFireCallback("[ST]HostConnected", false);
                    }
                }

                if (al.Count > 0)
                {
                    WriteLog(LogHeadType.CallEnd, "Fail. Reason: " + ExtendMethods.ToStringHelper(al), "EstablishCommunications()");
                    return al;
                }
                else
                {
                    WriteLog(LogHeadType.CallEnd, "Success" + ExtendMethods.ToStringHelper(al), "EstablishCommunications()");
                    return null;
                }
            }
            catch (Exception e)
            {
                WriteLog(LogHeadType.CallEnd, "Fail. Reason: " + e.Message + "\n" + e.StackTrace, "EstablishCommunications()");
                al.Add(e.Message);
                return al;
            }
            finally
            {
                Monitor.Exit(_lockInit);
            }
        }
        public ArrayList DownloadParameters()      //Step 3
        {
            Monitor.Enter(_lockInit);
            WriteLog(LogHeadType.CallStart, "", "DownloadParameters()");
            ArrayList al = new ArrayList();

            try
            {
                if (al.Count > 0)
                {
                    WriteLog(LogHeadType.CallEnd, "Fail. Reason: " + ExtendMethods.ToStringHelper(al), "DownloadParameters()");
                    return al;
                }
                else
                {
                    WriteLog(LogHeadType.CallEnd, "Success", "DownloadParameters()");
                    return null;
                }
            }
            catch (Exception e)
            {
                WriteLog(LogHeadType.CallEnd, "Fail. Reason: " + e.Message, "DownloadParameters()");
                al.Add(e.Message);
                return al;
            }
            finally
            {
                Monitor.Exit(_lockInit);
            }
        }
        public ArrayList Initialize()              //Step 4
        {
            Monitor.Enter(_lockInit);
            WriteLog(LogHeadType.CallStart, "", "Initialize()");
            ArrayList al = new ArrayList();

            try
            {                

                #region Setup Timer for [Ready] command send
                if (Timer_SendReady == null)
                {
                    //For Change to Offline
                    IsHostOnline = false;
                    _varCenter.SetValueAndFireCallback("[ST]HostOnline", false);//OffLine

                    Timer_SendReady = new System.Timers.Timer();
                    Timer_SendReady.Elapsed += new ElapsedEventHandler(Timer_SendReady_Elapsed);
                    Timer_SendReady.Interval = iSendReadyTimeInSecond * 1000;
                                        
                    EnableReadyTimer(true);
                }
                #endregion

                #region Setup Timer for [DoorError] command send
                if (Timer_SendDoorError == null)
                {
                    Timer_SendDoorError = new System.Timers.Timer();
                    Timer_SendDoorError.Elapsed += new ElapsedEventHandler(Timer_SendDoorError_Elapsed);
                    Timer_SendDoorError.Interval = 1 * 1000; //Default is [1] second.                
                }
                #endregion                

                #region Setup Timer for [INF:REPERR] command send
                if (Timer_Reperr == null)
                {
                    Timer_Reperr = new System.Timers.Timer();
                    Timer_Reperr.Interval = ReperrToHostInterval * 1000; //Default is [3] second.                
                }
                #endregion

                
                if (al!= null && al.Count > 0)
                {
                    WriteLog(LogHeadType.CallEnd, "Fail. Reason: " + ExtendMethods.ToStringHelper(al), "Initialize()");
                    _IsHostReady = false;
                    return al;
                }
                else
                {
                    WriteLog(LogHeadType.CallEnd, "Success" + ExtendMethods.ToStringHelper(al), "Initialize()");
                    _IsHostReady = true;
                    return null;
                }
            }
            catch (Exception e)
            {
                WriteLog(LogHeadType.CallEnd, "Fail. Reason: " + e.Message + "\n" + e.StackTrace, "Initialize()");
                al.Add(e.Message);
                return al;
            }
            finally
            {
                Monitor.Exit(_lockInit);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    WriteLog(LogHeadType.Info, "Disposing...");

                    if (_hostComm != null)
                    {
                        WriteLog(LogHeadType.Info, "Closing communication port...");
                        
                        EnableReadyTimer(false);

                        _hostComm.DataReceived -= new DataReceivedEventHandler(HostComm_DataReceived);
                        _hostComm.Disconnect();
                        IsHostConnected = false;

                    }
                }
            }
            disposed = true;
        }

        #region WriteLog Methods
        private bool WriteLog(LogHeadType enLogType, string szLogMessage, string szRemark = null)
        {
            if (_log != null)
            {
                return WriteLog(LogHelper.GetDefaultLogLevel(enLogType), enLogType, szLogMessage, szRemark);
            }
            else
                return false;
        }
        private bool WriteLog(LogLevel level, LogHeadType enLogType, string szLogMessage, string szRemark = null)
        {
            if (_log != null)
                return _log.WriteLog(objectName, level, enLogType, szLogMessage, szRemark);
            else
                return false;
        }
        protected bool WriteLog(HRESULT hr, string track = null, LogLevel level = LogLevel.DEBUG)
        {
            if (_log != null)
            {
                if (string.IsNullOrWhiteSpace(track))
                    return _log.WriteLog(objectName, level, LogHeadType.Exception, hr._message, hr.ALID.ToString());
                else
                    return _log.WriteLog(objectName, level, LogHeadType.Exception, hr._message + track, hr.ALID.ToString());
            }
            else
                return false;
        }
        protected bool WriteLog(Exception hr, LogLevel level = LogLevel.DEBUG)
        {
            if (_log != null)
            {
                return _log.WriteLog(objectName, level, LogHeadType.Exception, hr.Message + hr.StackTrace);
            }
            else
                return false;
        }
        #endregion

        #region Host Communication

        /// <summary>
        /// Receive from HosrCommandCtrl command for simulate test only.
        /// </summary>
        /// <param name="strCmd"></param>
        public void HostCommandSend(string strCmd)
        {
            if (HostType == HostType.EMULATOR)
            {

                byte[] byarrCmdStr = ExtendMethods.StringToBytes(strCmd);

                HRESULT hr = null;

                hr = InnerSendCommand(byarrCmdStr);
                if (hr != null)
                {
                    WriteLog(LogHeadType.Exception, hr._message);
                    _varCenter.SetValueAndFireCallback(ConstVC.VariableCenter.CurrentStatus, hr._message);
                }
            }
            else
            {
                string strMsg = string.Format("[EFEMHost]HostType is [{0}], simulator command can't be send.", ExtendMethods.ToStringHelper(HostType));
                _varCenter.SetValueAndFireCallback(ConstVC.VariableCenter.CurrentStatus, strMsg);

                WriteLog(LogHeadType.Info, strMsg);

            }
        }

        private HRESULT SendCommandToHost(string strCmd, bool isErr = false)
        {
            byte[] byarrCmdStr = ExtendMethods.StringToBytes(strCmd);

            return InnerSendCommand(byarrCmdStr, isErr);
        }

        private void FireCommCallBack(MsgType type, string context, bool isErr)
        {
            if (ConstVC.Debug.MonitorHostComm)   
                _varCenter.FireCommunicationCallBack(Sender.Host, type, context, isErr);
        }

        private HRESULT InnerSendCommand(byte[] byarrCmdStr, bool isErr = false)
        {
            if (!IsHostConnected)
            {
                HRESULT hr = MakeException("HOST_NOT_CONNECTED");
                return hr;
            }

            //if (!EFEMHostOnlineMode)
            //{
            //    HRESULT hr = MakeException("NOT_ONLINE_MODE");
            //    return hr;
            //}
            

            if (!Monitor.TryEnter(_lockSendCommand, 4000))
            {
                HRESULT hr = MakeException("SERVICE_BUSY");
                return hr;
            }


            try
            {
               
                _hostComm.Send(byarrCmdStr, byarrCmdStr.Length);

                //Fire to VC for update to CommDiagnosticForm        
                if (ConstVC.Debug.MonitorHostComm)
                {
                    string COMM_Send = ExtendMethods.BytesToString(byarrCmdStr, 0, byarrCmdStr.Length);
                    FireCommCallBack(MsgType.SEND, COMM_Send, isErr);
                }                               

                return null;
            }
            finally
            {
                Monitor.Exit(_lockSendCommand);
            }
        }        

        /// <summary>
        /// EFEMHost call Robot command by different parameter.
        /// </summary>
        /// <param name="ActionId">EFEMAction command index</param>
        /// <param name="StationId">EFEMStation index</param>
        /// <param name="DeviceCode">EFEMDeviceCode index</param>
        /// <param name="RobotArm">EFEMRoborArm index</param>
        /// <param name="iPara">Integet parameter for different command parameter use</param>
        /// <param name="dPara">Double parameter for different command parameter use</param>
        /// <returns>Null mean "OK" ; else mean "Error" happen.</returns>
        public HRESULT Action(EFEMAction ActionId, EFEMStation StationId, EFEMDeviceCode DeviceCode,EFEMRobotArm RobotArm, int iPara, double dPara)
        {
            //Record Testing Action Command.
            string strAction = string.Format("[EFEMHost]Action(ActionId={0},StationId={1},DeviceCode={2},RobotArm={3},iPara={4},dPara={5}) FireCallback", ActionId, StationId,DeviceCode,RobotArm, iPara, dPara);
            _varCenter.SetValueAndFireCallback(ConstVC.VariableCenter.CurrentStatus, strAction);
            WriteLog(LogHeadType.MethodEnter, strAction);

            HRESULT hr = null;

            if (_mc != null)
            {
                switch (ActionId)
                {
                    case EFEMAction.ACTION_NONE:
                        break;
                    case EFEMAction.ACTION_SETANGLE:
                        {
                            //Command not support in current status.

                            int PresetNum = iPara;
                            double AngularAngle = dPara;                                                       
                        }
                        break;
                    case EFEMAction.ACTION_VERSION:
                        {

                            EFEM_Version = GetEFEMVersion;

                            UpdatePara_VERSION(EFEM_Version);                            
                        }
                        break;
                    case EFEMAction.ACTION_SAVELOG:
                        {
                            switch (DeviceCode)
                            {
                                case EFEMDeviceCode.R1:                                    
                                case EFEMDeviceCode.A1:                                    
                                case EFEMDeviceCode.All:                                    
                                default:
                                    _fu.FlushEFEMConfig();
                                    _log.ForceWrite();

                                    break;
                            }                                                        
                         
                        }
                        break;

                     //================ Additional Command ========================//
                    default:
                        break;
                }                               

            }

            strAction = string.Format("[EFEMHost]Action(ActionId={0} is {1})", ActionId, (hr == null)?"Success":"Failed");
            WriteLog(LogHeadType.MethodExit, strAction);

            return hr;
        }
        
        private void HostComm_DataReceived(byte[] byData, int Length)
        {
                     
            if (EFEMHostOnlineMode)
            {
                DecodeRxMsg(byData, Length);
            }
            else
            {
                string strErrorMsg = string.Format("[EFEMHost]EFEMHostOnlineMode is not [Online] , Ignore Host command");
                _varCenter.SetValueAndFireCallback(ConstVC.VariableCenter.CurrentStatus, strErrorMsg);
                WriteLog(LogHeadType.Notify, strErrorMsg);
            }

            IsHostConnected = true;
            
            //IsHostOnline = true;
        }

        public int DecodeRxMsg(byte[] byarrRxedStr, int iLength)
        {
            Monitor.Enter(_lockDecodeCommand);

            try
            {                                
                HRESULT hr = null;

                bool bErrorAck = false, bErrorInf = false;
                string strAckMsg = null, strInfMsg = null;
                string strRespondCmd = null;

                //Byte RAW Data                    
                byte[] byData = byarrRxedStr;                
                string strRawData = ExtendMethods.BytesToString(byarrRxedStr, 0, iLength);

                if (ConstVC.Debug.MonitorHostComm)
                {
                	//Fire to VC for update to CommDiagnosticForm                
                	FireCommCallBack(MsgType.RECV, strRawData, false);
				}

                //string strLengthField = strRawData.Substring(1, 3);
                string strCommandField = strRawData.Substring(4, strRawData.Length - 4 - 3 - 1); //TotalLength-[SOH(1)+length(3)]-[CheckSum(2)+ETX(1)]-[semicolon;(1)]         
                //string strCheckSum = strRawData.Substring(strRawData.Length - 3, 2);

                int iCmdKind = -1;

                hr = ChkMsgFrame(byData, iLength);
                if (hr == null)
                {
                    //=======================================================//
                    //    SOH   Length  Command;   CheckSum    ETX           //
                    //     1      3         n +1      2         1            //
                    //=======================================================//

                    int iIdx = GB.MSG_FIRST_BYTE_IDX;

                    if ((iCmdKind = GB.GetCmdCode(byData, ref iIdx)) != -1)//index of cmd in strarrCmd, ex IDX_STATUS for CMD_STATUS = "STATUS"
                    {
                        CmdVariableReset();

                        switch (iCmdKind)
                        {

                            case GB.IDX_READYCOMM://00(EFEM is Ready) issued by the EFEM
                                {

                                    /* EFEM --> Host      INF:READY:COMM; , no Host command message is required.
                                     * The READY message is sent by the EFEM continuously until the Host sends ACK:READY/COMM back to the EFEM.
                                     * The READY event message is always sent by the EFEM during power up to signal the host that the EFEM is ready to connect.  
                                     * 
                                     * Example:
                                     * INF:READY/COMM;	EFEM repeatedly sends INF:READY/COM
                                     * INF:READY/COMM;
                                     * INF:READY/COMM;
                                     * ACK:READY/COMM:	Host responds with ACK:READY/COMM
                                     */

                                    if (HostType == HostType.EMULATOR)
                                    {
                                        //Receive "INF_READY" command from Loopback RS232 then simulate Host Ack command.
                                        WriteLog(LogHeadType.Info, "Simulate [ACK:READY] Command to Host");
                                        ThreadPool.QueueUserWorkItem(new WaitCallback(TPOOL_SimulateACK_READY));
                                    }

                                }
                                break;
                            case GB.IDX_ACKREADY://25(Host Ack READY:COMM)
                                {
                                    /* EFEM <-- Host  
                                     * ACK:READY/COMM:	Host responds with "ACK:READY/COMM"                                     
                                     */

                                    IsHostOnline = true;
                                    _varCenter.SetValueAndFireCallback("[ST]HostOnline", true); //Change to "OnLine" mode.
                                    EnableReadyTimer(false);

                                }
                                break;
                            case GB.IDX_REPORT://02(INF:REPORT/WAF_Put/P501) issued by the EFEM
                                {
                                    /*
                                     * The REPORT event message is only sent to indicate that a wafer has been successfully transferred to the aligner.
                                     * Already implement in "TPOOL_REPORT_WaferPutOnAligner"
                                     */

                                }
                                break;
                            case GB.IDX_REPERR://12(INF:REPERR/0/16003) issued by the EFEM.
                                {
                                    /*
                                     * Report Error,from ex:DoorOpern(11352),FFU controller error(16001)... from EFEM to host
                                     */
                                }
                                break;
                            //==================================================================================================//
                            //Command not support 
                            //==================================================================================================//
                            case GB.IDX_ORGSH://01(Origin Search)
                                {
                                    /*
                                     * The ORGSH command performs an origin search on the axis specified by Parameter
                                     * HOST ← EFEM	INF:ORGSH/Parameter;
                                     *              ABS:ORGSH/Parameter;
                                     *              
                                     * Command parameters:
                                     * Parameter:	Required:
                                     * ALL	        Performs origin search on all axes
                                     * ROBOT	    Performs origin search on all robot axes only
                                     * ALIGN	    Performs origin search on all aligner axes only
                                     */

                                    //hr = ParseCmd_ORGSH(strCommandField);
                                    hr = MakeException("NAK_CMD_NOT_SUPPORT");
                                    if (hr == null)
                                    {
                                        strRespondCmd = MakeACK_ORGSH(strCommandField);                                        
                                    }
                                }
                                break;
                            case GB.IDX_ORGABS://16(Origin Search Absolute)
                                {

                                    //hr = ParseCmd_ORGSH(strCommandField); //ORGABS same as ORGSH paramete check
                                    hr = MakeException("NAK_CMD_NOT_SUPPORT");
                                    if (hr == null)
                                    {
                                        strRespondCmd = MakeACK_ORGABS(strCommandField);
                                    }
                                }
                                break;
                            case GB.IDX_PAUSE://07(Set Pause State)
                                {
                                    /*
                                     * The PAUSE command is used to move the robot from the RUNNING state to the PAUSE state.
                                     */

                                    //hr = ParseCmd_NoPara(strCommandField);
                                    hr = MakeException("NAK_CMD_NOT_SUPPORT");
                                    if (hr == null)
                                    {
                                        strRespondCmd = MakeACK_NoPara(strCommandField);
                                    }
                                }
                                break;
                            case GB.IDX_RESUME://09(Set Running State)
                                {
                                    /*
                                     * The RESUME command is used to move the EFEM control state from PAUSE to RUNNING.
                                     */

                                    //hr = ParseCmd_NoPara(strCommandField);
                                    hr = MakeException("NAK_CMD_NOT_SUPPORT");
                                    if (hr == null)
                                    {
                                        strRespondCmd = MakeACK_NoPara(strCommandField);
                                    }
                                }
                                break;                            
                            case GB.IDX_ABORT://11(Abort Command)
                                {
                                    /*
                                     * The ABORT command cancels the PAUSE state and places the EFEM in the EMS state. 
                                     */

                                    //hr = ParseCmd_NoPara(strCommandField);
                                    hr = MakeException("NAK_CMD_NOT_SUPPORT");
                                    if (hr == null)
                                    {
                                        strRespondCmd = MakeACK_NoPara(strCommandField);
                                    }
                                }
                                break;                            
                            case GB.IDX_SETANGLE://19(Set Aligner Preset Stops)
                                {

                                    //This command not support in current status, return NAK message.
                                    //hr = ParseCmd_SETANGLE(strCommandField);
                                    hr = MakeException("NAK_CMD_NOT_SUPPORT");

                                    if (hr == null)
                                    {
                                        strRespondCmd = MakeACK_SETANGLE(strCommandField);
                                    }
                                }
                                break;
                            case GB.IDX_MAPING://05,unknown command
                                {
                                    hr = MakeException("NAK_CMD_NOT_SUPPORT");
                                }
                                break;
                            //==================================================================================================//
                            case GB.IDX_TRANSB://03(Wafer Transfer)
                                {

                                    hr = ParseCmd_TRANSB(strCommandField);

                                    if (hr == null)
                                    {
                                        strRespondCmd = MakeACK_TRANSB(strCommandField);                                       
                                    }
                                }
                                break;
                            case GB.IDX_GOTO://04(Go To Waiting Position)
                                {

                                    hr = ParseCmd_GOTO(strCommandField);
                                    if (hr == null)
                                    {
                                        strRespondCmd = MakeACK_GOTO(strCommandField);
                                    }
                                }
                                break;                            
                            case GB.IDX_HOME://06(Robot Home)
                                {
                                    /* 
                                     * The HOME command moves the robot to its software origin position at high speed.   
                                     * An origin search is not performed by the HOME command. 
                                     */
                                    hr = ParseCmd_NoPara(strCommandField);
                                    if (hr == null)
                                    {
                                        strRespondCmd = MakeACK_NoPara(strCommandField);
                                    }

                                    //TODO:For DoorError Testing
                                    //EnableDoorErrorTimer(!Timer_SendDoorError.Enabled);

                                }
                                break;
                            
                            case GB.IDX_ERROR://08(Get EFEM Error)
                                {
                                    /*
                                     * The ERROR command requests information from the EFEM about the last EFEM error. 
                                     */
                                    hr = ParseCmd_NoPara(strCommandField);
                                    if (hr == null)
                                    {
                                        strRespondCmd = MakeACK_ERROR(strCommandField);
                                    }
                                }
                                break;                                                                                                                
                            case GB.IDX_INIT2://14(Initialize EFEM Data and Perform Origin Search)
                                {
                                    /*
                                     * The INIT2 command performs the same data initialization as the INIT command, 
                                     * but INIT2 also performs an origin search on all axes of the transfer system.                                     
                                     */
                                    hr = ParseCmd_NoPara(strCommandField);
                                    if (hr == null)
                                    {
                                        strRespondCmd = MakeACK_NoPara(strCommandField);
                                    }
                                }
                                break;
                            case GB.IDX_INIT://15(Initialize EFEM Data)
                                {
                                    /*
                                     * The INIT command initializes all EFEM internal data but does not perform an origin search
                                     */

                                    hr = ParseCmd_NoPara(strCommandField);
                                    if (hr == null)
                                    {
                                        strRespondCmd = MakeACK_NoPara(strCommandField);
                                    }
                                }
                                break;                            
                            case GB.IDX_VACUUM://17(Vacuum Control )
                                {

                                    hr = ParseCmd_VACUUM(strCommandField);
                                    if (hr == null)
                                    {
                                        strRespondCmd = MakeACK_VACUUM(strCommandField);
                                    }
                                }
                                break;
                            case GB.IDX_SETSPEED://18(Set Robot Speed)
                                {

                                    hr = ParseCmd_SETSPEED(strCommandField);
                                    if (hr == null)
                                    {
                                        strRespondCmd = MakeACK_SETSPEED(strCommandField);
                                    }
                                }
                                break;                            
                            case GB.IDX_CHKWFR://20(Check Wafer Presence)
                                {
                                    /*
                                     * The WAFCHECK command checks for the presence of a wafer on the robot arm and aligner.
                                     * 
                                     * Command parameters:
                                     * Equipment:	Required:
                                     * ALL	        Wafer presence to be checked on robot and aligner 
                                     * ROBOT	    Wafer presence to be checked on robot only
                                     * ALIGN	    Wafer presence to be checked on aligner only
                                     */
                                    hr = ParseCmd_CHKWFR(strCommandField);
                                    if (hr == null)
                                    {
                                        strRespondCmd = MakeACK_CHKWFR(strCommandField);
                                    }

                                }
                                break;
                            case GB.IDX_CHK_ALNGTYP://21(Aigner Get Type of Wafer)
                                {
                                    /*
                                     * ALNGTYP command returns information about the last wafer aligned.  
                                     * Two pieces of information will be returned:
                                     * The first item reports whether the wafer had a “notch” or a “flat”.  
                                     * The second item reports whether the wafer size was 200mm or 300mm.  
                                     */

                                    hr = ParseCmd_NoPara(strCommandField);
                                    if (hr == null)
                                    {
                                        strRespondCmd = MakeACK_NoPara(strCommandField);
                                    }
                                }
                                break;
                            case GB.IDX_STGPOS://23(New Command:Get LL1/LL2 Position)
                                {
                                    /*
                                     * Sending==> (1)014GETSTGPOS/1;F0(3)  Length:19
                                     * 
                                     * Sample:
                                     * INF:GETSTGPOS/X-Pos/Y-Pos/Z-Pos;
                                     */
                                    hr = ParseCmd_STGPOS(strCommandField);
                                    if (hr == null)
                                    {
                                        strRespondCmd = MakeACK_STGPOS(strCommandField);
                                    }
                                }
                                break;

                            case GB.IDX_EMS://10(Emergency stop)
                                {
                                    /*
                                     * The EMS command causes the robot to perform an emergency stop.  
                                     * The emergency stop is not controlled so position information may be lost.  
                                     * It is recommended that an origin search be performed after an emergency stop.
                                     */

                                    hr = ParseCmd_NoPara(strCommandField);
                                    //hr = MakeException("NAK_CMD_NOT_SUPPORT");
                                    if (hr == null)
                                    {
                                        strRespondCmd = MakeACK_NoPara(strCommandField);
                                    }
                                }
                                break;
                            
                            //=== New Added Command(Not Implemented yet) ===//
                            case GB.IDX_CLRALM://26(Clear Alarm)
                                {
                                    //hr = ParseCmd_CLRALM(strCommandField);
                                    hr = MakeException("NAK_CMD_NOT_SUPPORT");
                                    if (hr == null)
                                    {
                                        strRespondCmd = MakeACK_NoPara(strCommandField);
                                    }
                                }
                                break;

                            case GB.IDX_SETWFRSNR://27(Set Wafer sensing mode)
                                {

                                    hr = ParseCmd_SETWFRSNR(strCommandField);
                                    if (hr == null)
                                    {
                                        strRespondCmd = MakeACK_SETWFRSNR(strCommandField);
                                    }
                                }
                                break;   
                        }

                    }
                    else
                    {                        
                        hr = MakeException("NAK_CMD_NOT_EXIST");

                        if (HostType == HostType.EMULATOR)
                            return 0;
                    }
                }
                
                if (iCmdKind == GB.IDX_ACKREADY) 
                   return 0;

                if (HostType == HostType.EMULATOR)
                {                                       
                    //EFEM ---> Host
                    if (iCmdKind == GB.IDX_READYCOMM || iCmdKind == GB.IDX_REPERR || iCmdKind == GB.IDX_REPORT)
                        return 0;                                
                }

                if (hr != null) //Feedback NAK message
                {
                    bErrorAck = true;
                    WriteLog(LogHeadType.Exception, string.Format("[NAK]Host command had Syntax Error, Reason:[{0}][{1}]", hr.ALID, hr._message));
                    //============================================================================================================//
                    //NAK:The NAK response type indicates that the command message does not comply with the command format rules. //
                    //============================================================================================================//                                     
                    int ErrorId = MakeMapErrorID(hr);

                    strRespondCmd = MakeNAK(strCommandField, ErrorId);                    
                }
                else
                {
                    string CAN_ErrorPara = null;

                    if (DEBUG_Bypass_CANCheck)
                    {                        
                        CMD_CANType CAN_Type = CMD_CANType.ERROR_EFEM;

                        CAN_ErrorPara = ExtendMethods.ToStringHelper(CAN_Type);
                        
                        //Un-comment below code can simulate CAN Error Message.
                        //hr = MakeException(ExtendMethods.ToStringHelper(CMD_CANType.CONDITION_NOT_ALLOW)); 
                    }
                    else
                    {
                        hr = CAN_CheckCondition(iCmdKind, ref CAN_ErrorPara);
                    }

                    if (hr != null) //Feedback CAN message
                    {
                        bErrorAck = true;
                        WriteLog(LogHeadType.Exception, string.Format("[CAN]Host command can't be executed due to some conflict condition, Reason:[{0}][{1}]", hr.ALID, hr._message));
                        //=============================================================================================================//
                        //CAN:A CAN response to a Host command indicates that the command was received properly but cannot be executed //
                        //=============================================================================================================//                                                                   
                        strRespondCmd = MakeCAN(strCommandField, CAN_ErrorPara);                        
                    }

                }

                //=======================================================//
                //Send [ACK]/[NAK]/[CAN] to Host depend on strRespondCmd //
                //=======================================================//
                strAckMsg = strRespondCmd;
                SendCommandToHost(strAckMsg, bErrorAck);

                //Need motion completetion(Send INF or ABS) to Host.
                if (GB.iCmdTypeMotionData[iCmdKind] > 0 && hr == null)
                {

                    //Call robot command after feedback "ACK" to Host.                    

                    switch (iCmdKind)
                    {
                        //==================================================================================================//
                        //Command WILL NOT Be Enter
                        //==================================================================================================//                        
                        case GB.IDX_READYCOMM:
                            {                                
                                //iCmdTypeMotionData = 0
                            }
                            break;
                        case GB.IDX_ACKREADY:
                            {
                                //In ACK already done it.

                                //iCmdTypeMotionData = -1
                            }
                            break;
                        case GB.IDX_REPORT:
                            {
                                //iCmdTypeMotionData = 0
                            }
                            break;
                        case GB.IDX_REPERR:
                            {
                                //iCmdTypeMotionData = 0
                            }
                            break;
                        case GB.IDX_ORGSH:
                            {
                                //[Cmd_DeviceID]

                                ClearAlarm();
                            }
                            break;
                        case GB.IDX_ORGABS:
                            {
                                //[Cmd_DeviceID]                                

                                ClearAlarm();
                            }
                            break;
                        case GB.IDX_PAUSE:
                            {
                                //Command not support in current status.
                            }
                            break;
                        
                        case GB.IDX_RESUME:
                            {
                                //Command not support in current status.
                            }
                            break;                        
                        case GB.IDX_ABORT:
                            {
                                //Command not support in current status.
                                IsEFEMStateEMS = true;
                            }
                            break;
                        case GB.IDX_SETANGLE:
                            {
                                //Command not support in current status.

                                //[Cmd_iPara]Preset number
                                //[Cmd_AlignAngle]Angular position

                                //hr = Action(EFEMAction.ACTION_SETANGLE, EFEMStation.NONE, EFEMDeviceCode.R1, EFEMRoborArm.Lower, Cmd_iPara, Cmd_AlignAngle);
                            }
                            break;
                        case GB.IDX_MAPING:
                            {
                                //Not Support
                            }
                            break;
                        case GB.IDX_ERROR:
                            {
                                //iCmdTypeMotionData = 0
                            }
                            break;
                        case GB.IDX_STATUS:
                            {
                                //iCmdTypeMotionData = 0                                    
                            }
                            break;
                        case GB.IDX_VERSION:
                            {
                                //In ACK already done it.
                                //iCmdTypeMotionData = 0                                    
                            }
                            break;
                        case GB.IDX_SAVELOG:
                            {
                                //In ACK already done it.                                                                   
                                //iCmdTypeMotionData = 0   

                                //[Cmd_DeviceID] 

                                //Reserve for INF:SAVELOG
                                //hr = Action(EFEMAction.ACTION_SAVELOG, EFEMStation.NONE, Cmd_DeviceID, EFEMRobotArm.Lower, 0, 0);                                
                                //if (hr == null)
                                //{
                                //    strRespondCmd = MakeINF_NoPara(strCommandField);
                                //}
                            }
                            break;
                        //==================================================================================================//                                                                                      
                    }

                    if (hr != null)//Feedback ABS Message
                    {
                        bErrorInf = true;
                        string strError = string.Format("[ABS]Robot Action failed , Reason:[{0}][{1}]", hr.ALID, hr._message);
                        _varCenter.SetValueAndFireCallback(ConstVC.VariableCenter.CurrentStatus, strError);
                        WriteLog(LogHeadType.Exception,strError );
                        //============================================================================================================//
                        //ABS:The ABS type completion message indicates that the operational command was not completed.               //
                        //============================================================================================================//
                        //TODO:Leo,Need mapping Robot alarm id with original manual's.
                        //ABS_ErrorLevel = Convert.ToInt32(hr._category);//If we can take ErrorLevel as "category" in ExceptionInfo
                        ABS_ErrorLevel = 1;//[0:Minor] , [1:Major]
                        ABS_ErrorCode = MakeMapErrorID(hr);
                        //ABS_ErrorCode = Convert.ToInt32(hr._maperrid);

                        strRespondCmd = MakeABS(strCommandField, ABS_ErrorLevel, ABS_ErrorCode);

                    }
                    
                    Thread.Sleep(200);

                    //Send [INF]/[ABS] to Host depend on strRespondCmd.
                    strInfMsg = strRespondCmd;
                    SendCommandToHost(strInfMsg, bErrorInf);

                    //Additional Post Action                    
                    if (OPTION_EnableRobotPostAction)
                    {
                        if (hr == null)
                        {
                            //01.When wafer put on Aligner, need report "INF:REPORT/WAF_Put/P501" message
                            if (Cmd_ActionID == EFEMAction.ACTION_PUT && Cmd_StationId == EFEMStation.ALIGNER)
                            {
                                WriteLog(LogHeadType.Event, string.Format("[REPORT]Wafer transfer to the aligner is completed."));
                                ThreadPool.QueueUserWorkItem(new WaitCallback(TPOOL_REPORT_WaferPutOnAligner));
                            }
                        }
                    }
                }
                
            }
            catch (Exception e)
            {
                WriteLog(LogHeadType.Exception, string.Format("{0}{1}", "DecodeRxMsg() error - ", e.Message + "->" + e.StackTrace));                
            }
            finally
            {
                Monitor.Exit(_lockDecodeCommand);
            }

            return 0;
        }

        //chk TotalLength, SOH, LEN, & ETX,     If not right, discard the msg
        //ChkSum was done in the RS232 DLL
        public HRESULT ChkMsgFrame(byte[] byarrA, int iLength)
        {
            int iLEN = GetLEN(byarrA); //get the integer from bytes 1,2,3                

            if (iLEN != iLength - 5)
                return MakeException("NAK_LEN_WRONG");
            else if (byarrA[iLength - 4] != ';')
                return MakeException("NAK_SEMICOL_MISS");
            else if (byarrA[0] != 0x1 || byarrA[iLength - 1] != 0x3)
                return MakeException("NAK_HEADERCODE_INVALID");
            else if (iLength < GB.MIN_MSG_LEN || iLength > GB.MAX_MSG_LEN)
                return MakeException("NAK_LENGTH_OUTOFRANGE");

            return null;
        }

        //get the integer from bytes 1,2,3 (ASCII) and convert into integer
        public int GetLEN(byte[] byarrA)
        {
            int i, LEN = 0;

            for (i = 1; i < 4; i++)
            {
                if (i != 1)
                    LEN *= 10;

                if (byarrA[i] > '9' || byarrA[i] < '0')
                    return -1;
                else
                    LEN += (byarrA[i] - '0');
            }
            return LEN;
        }

        private string AddMsgFrame(string Command)
        {
            string strData = null;
            string strTempData = null;
            int iLEN = 0;

            //EX:Original Command:[ACK:STATUS/RB/090000011/000]
            //Add Len and Semicolon(;) field
            iLEN = Command.Length +1 +2; //Command(Length) + SemiColon[;](1) + CheckSum(2)
            strTempData = string.Format("{0:000}{1};", iLEN, Command);//[030ACK:STATUS/RB/090000011/000;]

            //Add CheckSum field
            strTempData = string.Format("{0}{1}", strTempData, CalculateChecksum(strTempData)); //[030ACK:STATUS/RB/090000011/000;27]

            strData = AddStartEndChar(strTempData);

            return strData;

        }

        private string CalculateChecksum(string dataToCalculate)
        {
            byte[] byteToCalculate = Encoding.Default.GetBytes(dataToCalculate);
            int checksum = 0;
            foreach (byte chData in byteToCalculate)
            {
                checksum += chData;
            }
            checksum &= 0xff;
            return checksum.ToString("X2");
        }
        
        private string AddStartEndChar(string Command)
        {
            string strData = null;

            strData = string.Format("{0}{1}{2}", Convert.ToChar(GB.MSG_SOH), Command, Convert.ToChar(GB.MSG_EOT));

            return strData;
        }


        #region Make ACK Message
        /// <summary>
        /// No need to add parameter to ACK command string,only need add "ACK:" in front of command.
        /// </summary>
        /// <param name="CommandField"></param>
        /// <returns>"ACK:"+ command string</returns>
        private string MakeACK_NoPara(string CommandField)
        {
            string strData = null;
            string strTempData = null;

            strTempData = string.Format("{0}{1}", GB.MSG_RESP_ACK, CommandField);
            strData = AddMsgFrame(strTempData);   

            return strData;
        }

        private string MakeACK_ORGSH(string CommandField)
        {
            string strData = null;
            string strTempData = null;

            strTempData = string.Format("{0}{1}", GB.MSG_RESP_ACK, CommandField);
            strData = AddMsgFrame(strTempData);  

            return strData;
        }

        private string MakeACK_TRANSB(string CommandField)
        {
            string strData = null;
            string strTempData = null;

            strTempData = string.Format("{0}{1}", GB.MSG_RESP_ACK, CommandField);
            strData = AddMsgFrame(strTempData);    

            return strData;
        }

        private string MakeACK_GOTO(string CommandField)
        {
            string strData = null;
            string strTempData = null;

            strTempData = string.Format("{0}{1}", GB.MSG_RESP_ACK, CommandField);
            strData = AddMsgFrame(strTempData); 

            return strData;
        }

        private string MakeACK_ERROR(string CommandField)
        {
            /* 
             * The ERROR command sent by the Host does not contain parameters.  
             * However the response message sent to the Host by the EFEM does contain parameters that identify the EFEM error:
             * [ACK:ERROR/x/yyyyy;]
             * x--Error severity: 		
             * 0		        Minor error
             * 1		        Major error
             * 
             * yyyyy--Error code:  
             * 10000~16999		See Figure ____ for a complete listing of errors
             * 
             *  If no major or minor error exists when ERROR is sent the response will be,
             *  ACK:ERROR/OK
             */

            string strData = null;
            string strTempData = null;

            string strErrorCode = null;            

            if (!IsAnyErrorExist)
            {
                strErrorCode = string.Format("/{0}", "OK");
            }
            else
            {
                int ErrorLevel, ErrorCode;

                ErrorLevel = ABS_ErrorLevel;
                ErrorCode = ABS_ErrorCode;

                strErrorCode = string.Format("/{0}/{1}", ErrorLevel, ErrorCode);
            }

            strTempData = string.Format("{0}{1}", GB.MSG_RESP_ACK, strErrorCode);
            strData = AddMsgFrame(strTempData); 

            return strData;

        }

        /// <summary>
        /// Get Robot parameter return then update to Status_Ack command parameter.
        /// </summary>
        private void UpdatePara_STATUS()
        {                        

            //TODO:MachineControl always judge UpperArm to check wafer status,here put Kawasaki Lower status in "C5_UpperArm" for MachineControl identify.
            //"C6_iWaferOnLowerArm" in MachineControl is for identify "GripperArm=1","ArmNotRetracted=0"...flag,TW EFEM using "5" for simulate as original RorzeRobot
            
            if (SimulateRorzeRobot == (int)SimuRorzeRobot.AsRorze_UpperArm) //2
            {
                C5_iWaferOnUpperArm = (STATUS_IsWaferOnArm2 == EFEMWaferStatus.Presence) ? 1 : 0;                
                C6_iWaferOnLowerArm = 5;
            }
            else if (SimulateRorzeRobot == (int)SimuRorzeRobot.AsRorze_LowerArm)//1
            {
                C5_iWaferOnUpperArm = (STATUS_IsWaferOnArm1 == EFEMWaferStatus.Presence) ? 1 : 0;                
                C6_iWaferOnLowerArm = 5;
            }
            else //Kawasaki use lower arm as Arm1
            {
                C5_iWaferOnUpperArm = (STATUS_IsWaferOnArm2 == EFEMWaferStatus.Presence) ? 1 : 0;
                C6_iWaferOnLowerArm = (STATUS_IsWaferOnArm1 == EFEMWaferStatus.Presence) ? 1 : 0; 
            }
            

            C7_iWaferOnAligner  = (STATUS_IsWaferOnAligner == EFEMWaferStatus.Presence ) ? 1 : 0;

            //EFEMDeviceStatus
            M1_iRobotStatus =   (int)STATUS_Robot;
            M2_iAlignerStatus = (int)STATUS_Aligner;
            M_bIsNeedHome = STATUS_IsNeedHome;

            //Boolean
            E1_iRobotError = (STATUS_IsRobotError) ? (int)EFEMErrorLevel.Major : (int)EFEMErrorLevel.NoError;
            E2_iAlignerError = (STATUS_IsAlignerError) ? (int)EFEMErrorLevel.Major : (int)EFEMErrorLevel.NoError; 

            S1_VacuumStatus = (STATUS_IsSysVacuumOn) ? 1 : 0;
            S2_CDAStatus = (STATUS_IsSysAirOn) ? 1 : 0;
            
        }

        private void UpdatePara_WaferSensingStatus()
        {
            //EFEMWaferSensingStatus
            B1_AlignerWaferSnrChk = (STATUS_WaferSnrChk_Aligner == EFEMWaferStatus.Presence) ? 1 : 0;
            B2_LP1WaferSnrChk = (STATUS_WaferSnrChk_LP1 == EFEMWaferStatus.Presence) ? 1 : 0;
            B3_LP2WaferSnrChk = (STATUS_WaferSnrChk_LP2 == EFEMWaferStatus.Presence) ? 1 : 0;
            B4_LP3WaferSnrChk = (STATUS_WaferSnrChk_LP3 == EFEMWaferStatus.Presence) ? 1 : 0;
        }

        /// <summary>
        /// output 9 bits of STATUS after query Robot
        /// </summary>
        /// <returns>Output string(ex:/090000011)</returns>
        private string STATUS_ToString()
        {
            string strData = null;
            string strTemp_M1 = null, strTemp_M2 = null;

            //Forming StatusPara [/090000011]
            strData = "/";
            strData += string.Format("{0}{1}{2}", C5_iWaferOnUpperArm, C6_iWaferOnLowerArm, C7_iWaferOnAligner);

            //M1:RobotStatus
            if (M1_iRobotStatus >= 0)
            {
                strTemp_M1 = Convert.ToString(M1_iRobotStatus);
            }
            else
            {
                if (M_bIsNeedHome)
                    strTemp_M1 = "?";
                else
                    strTemp_M1 = "0";
            
            }
            
            //M2:AlignerStatus
            if (M2_iAlignerStatus >= 0)
            {
                strTemp_M2 = Convert.ToString(M2_iAlignerStatus);
            }
            else
            {
                if (M_bIsNeedHome)
                    strTemp_M2 = "?";
                else
                    strTemp_M2 = "0";

            }

            strData += string.Format("{0}{1}", strTemp_M1, strTemp_M2);
            
            strData += string.Format("{0}{1}", E1_iRobotError, E2_iAlignerError);
            strData += string.Format("{0}{1}", S1_VacuumStatus, S2_CDAStatus);

            return strData;
        }


        /// <summary>
        /// output 3 or 4 WaferSensoring bits depend on (Aligner + LP number)
        /// </summary>        
        /// <returns>Output string(ex:/0000)</returns>
        private string WaferSensingStatus_ToString()
        {
            string strData = null;

            if (GetLoadportCount == 3) //ex:0001 for LP3
            {
                strData = string.Format("/{0}{1}{2}{3}", B1_AlignerWaferSnrChk, B2_LP1WaferSnrChk, B3_LP2WaferSnrChk, B4_LP3WaferSnrChk);
            }
            else//ex:010 for LP1
            {
                strData = string.Format("/{0}{1}{2}", B1_AlignerWaferSnrChk, B2_LP1WaferSnrChk, B3_LP2WaferSnrChk);                                
            }
            
            return strData;
        
        }

        private string MakeACK_STATUS(string CommandField)
        {
            //=========================================================================//
            //StatusData characters: /  C5	C6	C7	M1	M2	E1	E1	S1	S3 / B1 B2  B3 //
            //=========================================================================//
            string strData = null;
            string strTempData = null;
            string strStatusPara = null;            
           
            //Forming StatusPara [/090000011]
            strStatusPara = STATUS_ToString();

            //Additional WaferSensorData [/0000]            
            strStatusPara += WaferSensingStatus_ToString();

            //Compose "ACK:" and StatusPara
            strTempData = string.Format("{0}{1}{2}", GB.MSG_RESP_ACK, CommandField, strStatusPara);
            strData = AddMsgFrame(strTempData);

            return strData;

        }

        private string MakeACK_ORGABS(string CommandField)
        {
            string strData = null;
            string strTempData = null;

            strTempData = string.Format("{0}{1}", GB.MSG_RESP_ACK, CommandField);
            strData = AddMsgFrame(strTempData);

            return strData;
        }

        private string MakeACK_VACUUM(string CommandField)
        {
            string strData = null;
            string strTempData = null;

            strTempData = string.Format("{0}{1}", GB.MSG_RESP_ACK, CommandField);
            strData = AddMsgFrame(strTempData);   

            return strData;
        }
        
        private string MakeACK_SETSPEED(string CommandField)
        {
            string strData = null;
            string strTempData = null;

            strTempData = string.Format("{0}{1}", GB.MSG_RESP_ACK, CommandField);
            strData = AddMsgFrame(strTempData); 

            return strData;
        }

        private string MakeACK_SETANGLE(string CommandField)
        {
            string strData = null;
            string strTempData = null;

            strTempData = string.Format("{0}{1}", GB.MSG_RESP_ACK, CommandField);
            strData = AddMsgFrame(strTempData); 

            return strData;
        }

        private string MakeACK_CHKWFR(string CommandField)
        {
            string strData = null;
            string strTempData = null;

            strTempData = string.Format("{0}{1}", GB.MSG_RESP_ACK, CommandField);
            strData = AddMsgFrame(strTempData);  

            return strData;
        }

        private void UpdatePara_VERSION(string Versoin)
        {
            ACK_VERSION = Versoin;
        }

        private string MakeACK_VERSION(string CommandField)
        {
            string strData = null;
            string strTempData = null;

            strTempData = string.Format("{0}{1}/{2}", GB.MSG_RESP_ACK, CommandField, ACK_VERSION);
            strData = AddMsgFrame(strTempData);  

            return strData;
        }

        private string MakeACK_STGPOS(string CommandField)
        {
            string strData = null;
            string strTempData = null;

            strTempData = string.Format("{0}{1}", GB.MSG_RESP_ACK, CommandField);
            strData = AddMsgFrame(strTempData);  

            return strData;
        }

        private string MakeACK_SAVELOG(string CommandField)
        {
            string strData = null;
            string strTempData = null;

            strTempData = string.Format("{0}{1}", GB.MSG_RESP_ACK, CommandField);
            strData = AddMsgFrame(strTempData);  

            return strData;
        }

        private string MakeACK_SETWFRSNR(string CommandField)
        {
            string strData = null;
            string strTempData = null;

            strTempData = string.Format("{0}{1}", GB.MSG_RESP_ACK, CommandField);
            strData = AddMsgFrame(strTempData);

            return strData;
        }
        #endregion

        #region Make NAK Error Message
        private int MakeMapErrorID(HRESULT hr)
        {
            int ErrorId = 0;

            switch (SimulateRorzeRobot)
            {
                case (int)SimuRorzeRobot.AsRorze_LowerArm:
                case (int)SimuRorzeRobot.AsRorze_UpperArm:
                case (int)SimuRorzeRobot.NoSimulate: //TODO:NoSimulate mode , also refer to maperrid as RorzeRobot's.
                    ErrorId = Convert.ToInt32(hr._maperrid);
                    break;
                                
                default:
                    ErrorId = Convert.ToInt32(hr._errid);

                    //If ErrorId is 0 , use ALID((_agent) * 1000 + _id) instead of it.
                    if(ErrorId ==0)
                        ErrorId = Convert.ToInt32(hr.ALID);
                    break;

            }

            return ErrorId;
        }

        private string MakeNAK(string strCommandField, int ErrorId)
        {
            string strData = null;
            string strTempData = null;
            string strErrorMsg = null;

            strErrorMsg = string.Format("|MESSAGE{0}", ErrorId);
            strTempData = string.Format("{0}{1}{2}", GB.MSG_RESP_NAK, strCommandField, strErrorMsg);

            strData = AddMsgFrame(strTempData);                                        

            return strData;                
        }
        
        #endregion

        #region Make CAN Error Message
        private string MakeCAN(string strCommandField, string strErrorPara)
        {
            string strData = null;
            string strTempData = null;
            string strErrorMsg = null;

            strErrorMsg = string.Format("|{0}", strErrorPara);
            strTempData = string.Format("{0}{1}{2}", GB.MSG_RESP_CAN, strCommandField, strErrorMsg);

            strData = AddMsgFrame(strTempData);

            return strData;
        }
        #endregion

        #region CAN Condition Check
        public HRESULT CAN_CheckCondition(int CmdKind,ref string ErrorPara)
        {
            HRESULT hr = null;
            CMD_CANType CAN_Type = CMD_CANType.ACK;            
            bool bDeviceIsBusy = false;

            //General Condition Check
            if (!IsHostOnline)
                CAN_Type = CMD_CANType.NOTREADY_COMM;
            else if (!IsHostReady)
                CAN_Type = CMD_CANType.NOTREADY_EFEM;
            else if (!IsHostCommandAllow)
                CAN_Type = CMD_CANType.NOTRUNMODE;
            //=====================================================================================================//
            if (CAN_Type == CMD_CANType.ACK)
            {                

                switch (CmdKind)
                {
                    case GB.IDX_READYCOMM:
                        {
                            //Will not enter this.
                        }
                        break;                                    
                    case GB.IDX_MAPING:
                        {
                            //Not support
                        }
                        break;
                    case GB.IDX_REPORT:
                        {
                            //Will not enter this.
                        }
                        break;
                    case GB.IDX_REPERR:
                        {
                            //Will not enter this.
                        }
                        break;
                    case GB.IDX_STATUS:
                        {
                            //No additional check
                        }
                        break;
                    case GB.IDX_ERROR:
                        {
                            //No additional check
                        }
                        break;
                    case GB.IDX_CHK_ALNGTYP:
                        {
                            //No additional check
                        }
                        break;
                    case GB.IDX_VERSION:
                        {
                            //No additional check
                        }
                        break;
                    case GB.IDX_STGPOS:
                        {
                            //No additional check
                        }
                        break;
                    case GB.IDX_SAVELOG:
                        {
                            //No additional check
                        }
                        break;
                    case GB.IDX_ACKREADY:
                        {
                            //No additional check
                        }
                        break;
                    case GB.IDX_ORGSH:
                    case GB.IDX_ORGABS:
                        {
                            if (STATUS_Robot == EFEMDeviceStatus.Pause)
                                CAN_Type = CMD_CANType.HOLD;
                            else if (!STATUS_IsSysAirOn)
                                CAN_Type = CMD_CANType.SRCAIR;
                            else if (!STATUS_IsSysVacuumOn)
                                CAN_Type = CMD_CANType.SRCVAC;
                        }
                        break;    
                    case GB.IDX_TRANSB:
                        {
                            if (STATUS_Robot == EFEMDeviceStatus.Unknown || STATUS_Aligner == EFEMDeviceStatus.Unknown)
                                CAN_Type = CMD_CANType.NOTORG;
                            else if (IsEFEMStateEMS)
                                CAN_Type = CMD_CANType.EMS;
                            else if (STATUS_Robot == EFEMDeviceStatus.Pause)
                                CAN_Type = CMD_CANType.HOLD;
                            else if (IsAnyErrorExist)
                                CAN_Type = CMD_CANType.ERROR_EFEM;
                            else if (!STATUS_IsSysAirOn)
                                CAN_Type = CMD_CANType.SRCAIR;
                            else if (!STATUS_IsSysVacuumOn)
                                CAN_Type = CMD_CANType.SRCVAC;
                        }
                        break;
                    case GB.IDX_GOTO:
                        {
                            if (STATUS_Robot == EFEMDeviceStatus.Unknown || STATUS_Aligner == EFEMDeviceStatus.Unknown)
                                CAN_Type = CMD_CANType.NOTORG;
                            else if (IsEFEMStateEMS)
                                CAN_Type = CMD_CANType.EMS;
                            else if (STATUS_Robot == EFEMDeviceStatus.Pause)
                                CAN_Type = CMD_CANType.HOLD;
                            else if (IsAnyErrorExist)
                                CAN_Type = CMD_CANType.ERROR_EFEM;
                            else if (!STATUS_IsSysAirOn)
                                CAN_Type = CMD_CANType.SRCAIR;
                            else if (!STATUS_IsSysVacuumOn)
                                CAN_Type = CMD_CANType.SRCVAC;
                        }
                        break;
                    
                    case GB.IDX_HOME:
                        {
                            //if (STATUS_Robot == EFEMDeviceStatus.Unknown || STATUS_Aligner == EFEMDeviceStatus.Unknown)
                            //    CAN_Type = CMD_CANType.NOTORG;
                            
                            if (IsEFEMStateEMS)
                                CAN_Type = CMD_CANType.EMS;
                            else if (STATUS_Robot == EFEMDeviceStatus.Pause)
                                CAN_Type = CMD_CANType.HOLD;
                            else if (IsAnyErrorExist)
                                CAN_Type = CMD_CANType.ERROR_EFEM;
                            else if (!STATUS_IsSysAirOn)
                                CAN_Type = CMD_CANType.SRCAIR;
                            else if (!STATUS_IsSysVacuumOn)
                                CAN_Type = CMD_CANType.SRCVAC;
                        }
                        break;
                    case GB.IDX_PAUSE:
                        {
                            if (IsEFEMStateEMS)
                                CAN_Type = CMD_CANType.EMS;
                            else if (STATUS_Robot == EFEMDeviceStatus.Pause)
                                CAN_Type = CMD_CANType.HOLD;
                            else if (!IsBusy)                            
                                CAN_Type = CMD_CANType.NOTINMOTION;                           
                                
                        }
                        break;                    
                    case GB.IDX_RESUME:
                        {
                            if (STATUS_Robot != EFEMDeviceStatus.Pause)
                                CAN_Type = CMD_CANType.NOPAUSE;
                            else if (!STATUS_IsSysAirOn)
                                CAN_Type = CMD_CANType.SRCAIR;
                            else if (!STATUS_IsSysVacuumOn)
                                CAN_Type = CMD_CANType.SRCVAC;
                        }
                        break;                   
                    case GB.IDX_EMS:
                        {
                            if (STATUS_Robot == EFEMDeviceStatus.Pause)
                                CAN_Type = CMD_CANType.HOLD;
                            else if (!IsBusy)
                                CAN_Type = CMD_CANType.NOTINMOTION;  
                        }
                        break;
                    case GB.IDX_ABORT:
                        {
                            if (!STATUS_IsSysAirOn)
                                CAN_Type = CMD_CANType.SRCAIR;
                            else if (!STATUS_IsSysVacuumOn)
                                CAN_Type = CMD_CANType.SRCVAC;
                        }
                        break;                    
                    case GB.IDX_INIT:
                    case GB.IDX_INIT2:
                        {
                            if (STATUS_Robot == EFEMDeviceStatus.Pause)
                                CAN_Type = CMD_CANType.HOLD;
                            else if (!STATUS_IsSysAirOn)
                                CAN_Type = CMD_CANType.SRCAIR;
                            else if (!STATUS_IsSysVacuumOn)
                                CAN_Type = CMD_CANType.SRCVAC;
                        }
                        break;
                    case GB.IDX_VACUUM:
                        {
                            if (STATUS_Robot == EFEMDeviceStatus.Unknown || STATUS_Aligner == EFEMDeviceStatus.Unknown)
                                CAN_Type = CMD_CANType.NOTORG;
                            else if (STATUS_Robot == EFEMDeviceStatus.Pause)
                                CAN_Type = CMD_CANType.HOLD;
                            else if (IsAnyErrorExist)
                                CAN_Type = CMD_CANType.ERROR_EFEM;
                            else if (!STATUS_IsSysAirOn)
                                CAN_Type = CMD_CANType.SRCAIR;
                            else if (!STATUS_IsSysVacuumOn)
                                CAN_Type = CMD_CANType.SRCVAC;
                        }
                        break;
                    case GB.IDX_SETSPEED:
                        {
                            if (STATUS_Robot == EFEMDeviceStatus.Pause)
                                CAN_Type = CMD_CANType.HOLD;
                        }
                        break;
                    case GB.IDX_SETANGLE:
                        {
                            if (STATUS_Robot == EFEMDeviceStatus.Pause)
                                CAN_Type = CMD_CANType.HOLD;
                        }
                        break;
                    case GB.IDX_CHKWFR:
                        {
                            if (STATUS_Robot == EFEMDeviceStatus.Pause)
                                CAN_Type = CMD_CANType.HOLD;
                            else if (IsAnyErrorExist)
                                CAN_Type = CMD_CANType.ERROR_EFEM;
                            else if (!STATUS_IsSysAirOn)
                                CAN_Type = CMD_CANType.SRCAIR;
                            else if (!STATUS_IsSysVacuumOn)
                                CAN_Type = CMD_CANType.SRCVAC;
                        }
                        break;
                    
                    
                    default:
                        break;
                }
            }

            if (CAN_Type != CMD_CANType.ACK)
            {
                ErrorPara = ExtendMethods.ToStringHelper(CAN_Type);

                hr = MakeException(ExtendMethods.ToStringHelper(CMD_CANType.CONDITION_NOT_ALLOW)); //General CAN Exception
            }

            return hr;
        }
        #endregion

        #region Make INF Message

        private string MakeINF_NoPara(string CommandField)
        {
            string strData = null;
            string strTempData = null;

            strTempData = string.Format("{0}{1}", GB.MSG_RESP_INF, CommandField);
            strData = AddMsgFrame(strTempData);

            return strData;
        }                

        private string MakeINF_ALNGTYP(string CommandField)//INF will feedback ALNGTYP information
        {
            /*
             * INF:ALNGTYP/n1,n2;
             * Where: 
             * n1=1 or 2 	(notch / flat)
             * n2=8 or 12 	(200mm / 300mm)
             * 
             * ex: 019INF:ALNGTYP/1,12;
            */

            string strData = null;
            string strTempData = null;

            strTempData  = string.Format("{0}{1}", GB.MSG_RESP_INF, CommandField);
            strTempData += string.Format("/{0},{1}", N1_iAlignerType, N2_iWaferSize);
            
            strData = AddMsgFrame(strTempData);

            return strData;
        }

        private void UpdatePara_STGPOS()
        {
            //Not Use:STGPOS_OAngle
            //Not Use:STGPOS_IsTaught
            
            INF_LLPos[0] = STGPOS_LLPos[0];
            INF_LLPos[1] = STGPOS_LLPos[1];
            INF_LLPos[2] = STGPOS_LLPos[2];
        }

        private string MakeINF_STGPOS(string CommandField) //INF will feedback STGPOS information
        {
            /*
             * GETSTGPOS/1
             * ACK:GETSTGPOS/1
             * INF:GETSTGPOS/2008/-639996/170995

             * GETSTGPOS/2
             * ACK:GETSTGPOS/2
             * INF:GETSTGPOS/-10260/-618141/172295
             */

            string strData = null;
            string strTempData = null;

            strTempData  = string.Format("{0}{1}", GB.MSG_RESP_INF, CommandField);
            strTempData += string.Format("/{0}/{1}/{2}", INF_LLPos[0], INF_LLPos[1], INF_LLPos[2]);

            strData = AddMsgFrame(strTempData);

            return strData;
        }

        private string MakeINF_TRANSB(string CommandField)
        {
            string strData = null;
            string strTempData = null;
            string strPara = null;

            //Additional WaferSensorData [/0000]            
            strPara = WaferSensingStatus_ToString();
            strTempData = string.Format("{0}{1}{2}", GB.MSG_RESP_INF, CommandField, strPara);
            
            strData = AddMsgFrame(strTempData);

            return strData;
        }

        private string MakeINF_CHKWFR(string CommandField)
        {
            string strData = null;
            string strTempData = null;
            string strPara = null;

            //Additional WaferSensorData [090000011/0000]            
            strPara  = STATUS_ToString();
            strPara += WaferSensingStatus_ToString();

            strTempData = string.Format("{0}{1}{2}", GB.MSG_RESP_INF, CommandField, strPara);
            
            strData = AddMsgFrame(strTempData);

            return strData;
        }

        #endregion

        #region Make ABS Error Message
        private string MakeABS(string strCommandField, int ErrorLevel,int ErrorCode)
        {
            string strData = null;
            string strTempData = null;
            string strErrorMsg = null;

            strErrorMsg = string.Format("|ERROR/{0}/{1}", ABS_ErrorLevel, ABS_ErrorCode);
            strTempData = string.Format("{0}{1}{2}", GB.MSG_RESP_ABS, strCommandField, strErrorMsg);

            strData = AddMsgFrame(strTempData);

            return strData;
        }
        #endregion

        #region ErrorLevel and ErrorParameter Flag
        private void ClearAlarm()
        {
            IsEFEMStateEMS = false;
            
            ABS_ErrorLevel = 0;
            ABS_ErrorCode = 0;

            _IsAnyErrorExist = false;

        }

        #endregion

        #region ThreadPool collection
        //Send ACK:READY to Host(For RS232 loopback test)
        private void TPOOL_SimulateACK_READY(object para)
        {
            int iSleepTime = 1;
            
            //Sleep few seocnds
            Thread.Sleep(iSleepTime * 1000);
            
            SendCommandToHost(MakeACK_READY());

        }

        //Host --> EFEM , Host responds with "ACK:READY/COMM" to EFEM when receive EFEM "INF:READY/COMM"
        private string MakeACK_READY() 
        {
            /*
             * Example:
             * INF:READY/COMM;	EFEM repeatedly sends INF:READY/COM
             * INF:READY/COMM;
             * INF:READY/COMM;
             * ACK:READY/COMM:	Host responds with ACK:READY/COMM 
             */
            string strData = null;            
            string strTempData = null;

            strTempData = string.Format("{0}{1}", GB.MSG_RESP_ACK, GB.CMD_READYCOMM);

            strData = AddMsgFrame(strTempData);            

            return strData;
        }

        //=============================================================//
        /// <summary>
        /// Enable/Disable "INF:READY" Timer to send to Host.
        /// </summary>
        /// <param name="bEnable">[True:Enable timer],[False:Disable timer]</param>
        private void EnableReadyTimer(bool bEnable)
        {
            if (Timer_SendReady == null)
                return;            

            if (bEnable)
            {
                if (!Timer_SendReady.Enabled)
                {
                    Timer_SendReady.Start();
                    WriteLog(LogHeadType.Info, string.Format("[{0}] Timer which send [READY] command to Host", bEnable ? "Enable" : "Disable"));
                }
            }
            else
            {
                if (Timer_SendReady.Enabled)
                {
                    Timer_SendReady.Stop();
                    WriteLog(LogHeadType.Info, string.Format("[{0}] Timer which send [READY] command to Host", bEnable ? "Enable" : "Disable"));
                }
            }

        }

        //Send INF:READY to Host and waiting for "ACK:READY" feedback.
        void Timer_SendReady_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(TPOOL_SendReady));
        }

        private void TPOOL_SendReady(object para)
        {
            if (IsHostConnected && !IsHostOnline && EFEMHostOnlineMode)
            {
                SendCommandToHost(MakeINF_READY());
            }
        }


        //EFEM --> Host , The INF:READY event message is always sent by the EFEM during power up to signal the host that the EFEM is ready to connect.  
        private string MakeINF_READY()
        {
            /*
             * Example:
             * INF:READY/COMM;	EFEM repeatedly sends INF:READY/COM
             * INF:READY/COMM;
             * INF:READY/COMM;
             * ACK:READY/COMM:	Host responds with ACK:READY/COMM 
             */
            string strData = null;
            string strTempData = null;

            strTempData = string.Format("{0}{1}", GB.MSG_RESP_INF, GB.CMD_READYCOMM);

            strData = AddMsgFrame(strTempData);

            return strData;
        }
        //=============================================================//
        /// <summary>
        /// Enable/Disable DoorError Timer to report error to Host.
        /// </summary>
        /// <param name="bEnable">[True:Enable timer],[False:Disable timer]</param>
        private void EnableDoorErrorTimer(bool bEnable)
        {
            if (Timer_SendDoorError == null)
                return;
            

            if (bEnable)            
            {
                if (!Timer_SendDoorError.Enabled)
                {
                    Timer_SendDoorError.Start();
                    WriteLog(LogHeadType.Info, string.Format("[{0}] Timer which send [DoorError] command to Host", bEnable ? "Enable" : "Disable"));
                }
            }
            else
            {
                if (Timer_SendDoorError.Enabled)
                {
                    Timer_SendDoorError.Stop();
                    WriteLog(LogHeadType.Info, string.Format("[{0}] Timer which send [DoorError] command to Host", bEnable ? "Enable" : "Disable"));
                }
            }
        
        }
        
        void Timer_SendDoorError_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(TPOOL_SendDooeError));
        }

        private void TPOOL_SendDooeError(object para)
        {
            if (IsHostConnected && IsHostOnline && EFEMHostOnlineMode)
            {
                SendCommandToHost(REPERR_DoorOpen());
            }
        }

        private string REPERR_DoorOpen()
        {            
            string strData = null;            

            int iErrLevel = 1; //1:Major
            int iErrCode = 11352; //Special ErrorCode:Door interlock is made, any motion command causes a robot stall
            //iErrCode = Convert.ToInt32(MakeException(ExtendMethods.ToStringHelper(REPERR_Type.DOOR_INTERLOCK))._maperrid);

            HRESULT hr = MakeException(ExtendMethods.ToStringHelper(REPERR_Type.DOOR_INTERLOCK));

            int mapError = MakeMapErrorID(hr);

            if (mapError != 0)
                iErrCode = mapError;
            else
                iErrCode = 11352;            
            
            //ex:"INF:REPERR/1/11352"
            strData = MakeINF_REPERR(iErrLevel, iErrCode);                                  

            return strData;
        }
        //=============================================================//        
        
        /// <summary>
        /// Battery Abnormal Check logic and send REPERR to host,only send once when status changed.
        /// </summary>
        private void BatteryCheckLogic()
        {
            lock (_lockBatteryCheck)
            {

                if (IsBatteryAbnormal)
                {
                    if (BatteryErr_Controller)
                    {
                        WriteLog(LogHeadType.Exception_Warning, string.Format("Robot controller's battery is abnormal ,please check it!"));

                        ReportError RepErr = new ReportError(REPERR_Type.CONTROL_BATTERY_LOW, REPERR_Level.MINOR);

                        //ThreadPool.QueueUserWorkItem(new WaitCallback(TPOOL_SendReperr), RepErr);
                        TPOOL_SendReperr((object)RepErr);

                        Thread.Sleep(100);
                    }

                    if (BatteryErr_Robot)
                    {

                        WriteLog(LogHeadType.Exception_Warning, string.Format("Robot's battery is abnormal,please check it!"));

                        ReportError RepErr = new ReportError(REPERR_Type.RBT_BATTERY_LOW, REPERR_Level.MINOR);

                        //ThreadPool.QueueUserWorkItem(new WaitCallback(TPOOL_SendReperr), RepErr);
                        TPOOL_SendReperr((object)RepErr);

                        Thread.Sleep(100);
                    }
                }
            }
        
        }

        //=============================================================//

        /// <summary>
        /// Enable/Disable "INF:REPERR" Timer to send to Host repeatedly.
        /// </summary>
        /// <param name="bEnable">[True:Enable timer],[False:Disable timer]</param>        
        private void EnableReperrTimer(bool bEnable)
        {
            if (Timer_Reperr == null)
                return;            

            if (bEnable)
            {
                if (!Timer_Reperr.Enabled)
                {                    
                    Timer_Reperr.Start();
                    WriteLog(LogHeadType.Info, string.Format("[{0}] Timer which send [REPERR] command to Host", bEnable ? "Enable" : "Disable"));
                }
            }
            else
            {
                if (Timer_Reperr.Enabled)
                {
                    Timer_Reperr.Stop();
                    WriteLog(LogHeadType.Info, string.Format("[{0}] Timer which send [REPERR] command to Host", bEnable ? "Enable" : "Disable"));
                }
            }

        }

        private void TPOOL_SendReperr(object para)
        {
            ReportError RepErr = (ReportError)para;
            if (RepErr.Type == null) 
                return;

            if (IsHostConnected && IsHostOnline && EFEMHostOnlineMode)
            {
                SendCommandToHost(REPERR_ErrorEvent(RepErr));
            }
        }

        private string REPERR_ErrorEvent(ReportError RepErr)
        {
            string strData = null;

            int iErrLevel = Convert.ToInt32(RepErr.Level);
            int iErrCode;

            HRESULT hr = MakeException(ExtendMethods.ToStringHelper(RepErr.Type));
            int mapError = MakeMapErrorID(hr);

            iErrCode = mapError;

            //iErrCode = Convert.ToInt32(MakeException(ExtendMethods.ToStringHelper(RepErr.Type))._maperrid);
            
            //ex:"INF:REPERR/0/16001"
            strData = MakeINF_REPERR(iErrLevel, iErrCode);

            return strData;
        }        
        
        //Make "INF:REPERR" base
        private string MakeINF_REPERR(int iErrLevel, int iErrCode)
        {
            string strData = null;
            string strTempData = null;

            strTempData = string.Format("{0}{1}/{2}/{3}", GB.MSG_RESP_INF, GB.CMD_REPERR, iErrLevel, iErrCode);

            strData = AddMsgFrame(strTempData);            

            return strData;
        }

        //=============================================================//
        //REPORT event for WaferPutAligner OK.
        private void TPOOL_REPORT_WaferPutOnAligner(object para)
        {
            if (IsHostConnected && IsHostOnline && EFEMHostOnlineMode)
            {
                SendCommandToHost(REPORT_WaferPutOnAligner());
            }
            
        }
        private string REPORT_WaferPutOnAligner()
        {
            string strData = null;

            /*================================================================================================
            * During wafer transfer, a wafer placed on the aligner may be aligned automatically.  
            * During the alignment cycle the robot is free to complete other transfer tasks.  
            * The EFEM will send an event message to signal that the wafer transfer to the aligner is complete
            * ================================================================================================*/
            string strAction = "WAF_PUT";
            string strPara = "P501"; //Aligner
            

            //ex:"INF:REPORT/WAF_Put/P501"
            strData = MakeINF_REPORT(strAction, strPara);

            return strData;
        }

        //Make "INF:REPORT" base
        private string MakeINF_REPORT(string strPara1, string strPara2)
        {
            string strData = null;
            string strTempData = null;

            strTempData = string.Format("{0}{1}/{2}/{3}", GB.MSG_RESP_INF, GB.CMD_REPORT, strPara1, strPara2);

            strData = AddMsgFrame(strTempData);

            return strData;
        }        

        #endregion

        #region Parse Host Command Parameter

        private EFEMAction Cmd_ActionID;
        private int Cmd_SlotId;
        private double Cmd_AlignAngle;
        private EFEMRobotWaitingPos Cmd_RobotWaitPos;        
        private int Cmd_iPara; 

        private void CmdVariableReset()
        {
            //Reset Parameter
            Cmd_ActionID = EFEMAction.ACTION_NONE;
            Cmd_SlotId = 0;
            Cmd_AlignAngle = 0;
            Cmd_RobotWaitPos = EFEMRobotWaitingPos.Unknown;            
            Cmd_iPara = 0;
        }

        private int ParseSlotID(string Para)
        {
            int SlotId = -1;

            if (Para.Length >= 2)
            {
                int.TryParse(Para, out SlotId);                    
            }

            return SlotId;
        }

        private double ParseAlignAngle(string Para)
        {
            double dAngle = -1;

            if (Para.Length > 0)
            {
                double.TryParse(Para, out dAngle);
            }

            return dAngle;
        }
        
        /// <summary>
        /// Convert Robot speed from [SLOW/1/2/3] to float value[25%~100%].
        /// </summary>
        /// <param name="iSpeed">1:25% , 2:50% , 3:75% , 4:100%</param>
        /// <returns>Speed in float format</returns>
        private float RobotSpeedToFloat(int iSpeed)
        {
            float fSpeed = 0;

            switch (iSpeed)
            {
                case 1: // SLOW/1
                    fSpeed = 25.0f;
                    break;
                case 2:// SLOW/2
                    fSpeed = 50.0f;
                    break;
                case 3:// SLOW/3
                    fSpeed = 75.0f;
                    break;
                case 4:// FAST
                default:
                    fSpeed = 100.0f;
                    break;
            }

            return fSpeed;

        }


        /// <summary>
        /// Command without parameter , so no need to parse parameter and validate value.
        /// </summary>
        /// <param name="CommandField">Command from Host</param>
        /// <returns>0:No Error ; Negative:Error Code </returns>
        /// 
        private HRESULT ParseCmd_NoPara(string CommandField)
        {
            return null;
        }

        private HRESULT ParseCmd_ORGSH(string CommandField)
        {
            /*
             * The ORGABS command performs an absolute origin search on all specified axes.
             * 
             * HOST ← EFEM	INF:ORGABS/Parameter;
             *          	ABS:ORGABS/Parameter;
             * Command parameters:
             * Parameter:	Required:
             * ALL	        Performs origin search on all axes
             * ROBOT	    Performs origin search on all robot axes only
             * ALIGN	    Performs origin search on all aligner axes only
             */            

            string[] ParaField = CommandField.Split('/');

            if (ParaField.Length < 2)
                return MakeException("NAK_SLASH_MISS");

            return null;
        }

        private HRESULT ParseCmd_TRANSB(string CommandField)
        {
            /*
             * The TRANS command has several forms depending on the number of robot arms and the type of wafer transfers being performed
             * HOST ← EFEM 
             * INF:TRANS*B/[Parameter1]/[Parameter2]/[Parameter3];
             * ABS:TRANS*B/[Parameter1]/[Parameter2]/[Parameter3];
             * 
             * Command parameters:
             * Parameter1:	Optional for arm1:  Pnss>Pndd_A or Pnss>ARM_A
             * n	        Port number: 1~4 for loadports, 5 for aligner, A~E for loadlocks.
             * ss	        Source slot for wafer:  01~26; use only 01 for the aligner and loadlocks.
             * dd	        Destination slot for wafer:  01~26; use ony 01 for the aligner and loadlocks.
             * ARM_A	    Identifies the upper arm at its waiting position as the destination of the wafer transfer.  
             * 
             * Parameter2:	Not used for single arm robot type RR716.
             * 
             * Parameter3:	Optional:  ALIGNxxN or ALIGNDyyy.yyN 
             * xx	        Preset stopping angle:  01~10
             * yyy.yy	    Selectable stopping angle: 000.00~360.00 degrees
             */            

            string[] ParaField = CommandField.Split('/');
            
            if (ParaField.Length < 4)
                return MakeException("NAK_SLASH_MISS");

            string strPara1 = ParaField[1];
            string strPara2 = ParaField[2];
            string strPara3 = ParaField[3];

            string[] strParaField = null;
            string strParaField_Src = null;
            string strParaField_Des = null;
            string strParaField_Des_ArmSel = null;
            
            //Para1
            int SlotId = 0;

            //Para3
            double dAngle = -1;            
            /*
             * Parameter1:	Optional for arm1:  Pnss>Pndd_A or Pnss>ARM_A
             * n	        Port number: 1~4 for loadports, 5 for aligner, A~E for loadlocks.
             * ss	        Source slot for wafer:  01~26; use only 01 for the aligner and loadlocks.
             * dd	        Destination slot for wafer:  01~26; use ony 01 for the aligner and loadlocks.
             * ARM_A	    Identifies the upper arm at its waiting position as the destination of the wafer transfer.  
             */
            if (strPara1 != null && strPara1.Contains(">"))
            {
                strParaField = strPara1.Split('>');

                if (strParaField.Length == 2)
                {
                    strParaField_Src = strParaField[0];
                    strParaField_Des = strParaField[1];

                    //"Put to" command
                    if (strParaField_Src.Contains("ARM"))                    
                    {
                                                
                        strParaField = strParaField_Des.Split('_');
                        if (strParaField.Length == 2)
                        {
                            strParaField_Des = strParaField[0];
                            strParaField_Des_ArmSel = strParaField[1];

                            if (string.Compare(strParaField_Src, "ARM", true) == 0)
                            {

                                if (SimulateRorzeRobot != (int)SimuRorzeRobot.NoSimulate)
                                {
                                    if (SimulateRorzeRobot == (int)SimuRorzeRobot.AsRorze_LowerArm)
                                        RobotArm = EFEMRobotArm.Lower;
                                    else
                                        RobotArm = EFEMRobotArm.Upper;
                                }
                                else
                                {
                                    //TODO:Reserve for future to define correct command parameter.
                                    if (string.Compare(strParaField_Des_ArmSel, "A", true) == 0)
                                        RobotArm = EFEMRobotArm.Upper;
                                    else if (string.Compare(strParaField_Des_ArmSel, "B", true) == 0)
                                        RobotArm = EFEMRobotArm.Lower;

                                    //Always use LowerArm to get/put wafer.
                                    //RobotArm = EFEMRobotArm.Lower;
                                }
                            }
                            else
                            {
                                return MakeException("NAK_PARA_MISS");
                            }

                        }
                        else
                        {
                            return MakeException("NAK_UNDERSCORE_MISS");
                        }   

                        if (strParaField_Des[0] != 'P')
                        {
                            return MakeException("NAK_P_MISS");
                        }
                        else
                        {
                            StageId = ParseStageID(strParaField_Des.Substring(1, 1));
                            if (StageId == EFEMStation.NONE)
                                return MakeException("NAK_STAGE_OUTOFRANGE");
                        }

                        SlotId = ParseSlotID(strParaField_Des.Substring(2, 2));
                        if (SlotId < 1 || SlotId > GetLoadportSlotCount(StageId))
                            return MakeException("NAK_SLOT_PARAWRONG");

                        //Assign value
                        Cmd_ActionID = EFEMAction.ACTION_PUT;
                        Cmd_StationId = StageId;
                        Cmd_SlotId = SlotId;
                        Cmd_RobotArm = RobotArm;
                    }
                    else if (strParaField_Des.Contains("ARM")) //"Get from" command
                    {
                        strParaField = strParaField_Des.Split('_');
                        if (strParaField.Length == 2)
                        {
                            strParaField_Des = strParaField[0];
                            strParaField_Des_ArmSel = strParaField[1];

                            if (string.Compare(strParaField_Des, "ARM", true) == 0)
                            {

                                if (SimulateRorzeRobot != (int)SimuRorzeRobot.NoSimulate)
                                {
                                    if (SimulateRorzeRobot == (int)SimuRorzeRobot.AsRorze_LowerArm)
                                        RobotArm = EFEMRobotArm.Lower;
                                    else
                                        RobotArm = EFEMRobotArm.Upper;
                                }
                                else
                                {
                                    //TODO:Reserve for future to define correct command parameter.
                                    if (string.Compare(strParaField_Des_ArmSel, "A", true) == 0)
                                        RobotArm = EFEMRobotArm.Upper;
                                    else if (string.Compare(strParaField_Des_ArmSel, "B", true) == 0)
                                        RobotArm = EFEMRobotArm.Lower;                                    

                                    //Always use LowerArm to get/put wafer.
                                    //RobotArm = EFEMRobotArm.Lower;
                                }
                            }
                            else
                            {
                                return MakeException("NAK_PARA_MISS");                                                                
                            }

                        }
                        else
                        {
                            return MakeException("NAK_UNDERSCORE_MISS");
                        }

                        if (strParaField_Src[0] != 'P')
                        {
                            return MakeException("NAK_P_MISS");
                        }
                        else
                        {
                            SlotId = ParseSlotID(strParaField_Src.Substring(2, 2));
                            if (SlotId < 1 || SlotId > GetLoadportSlotCount(StageId))
                                return MakeException("NAK_SLOT_PARAWRONG");

                            //Assign value
                            Cmd_ActionID = EFEMAction.ACTION_GET;
                            Cmd_SlotId = SlotId;
                        }
                    }
                    else
                    {
                        return MakeException("NAK_PARA_MISS");
                        //return MakeException(CMD_NAKType.NAK_PARA_MISS.ToString());
                    }
                }
            }

            else if (strPara2 != null && strPara2.Contains(">"))
            {
                /*
                 * Parameter 2:	Not used for single arm robot type RR716
                 */

                return MakeException("NAK_CMD_NOT_SUPPORT");
            }
            else if (strPara3 != null && strPara3.Contains("ALIGN"))
            {
                /*
                 * Parameter3:	Optional:  ALIGNxxN or ALIGNDyyy.yyN 
                 * xx	        Preset stopping angle:  01~10
                 * yyy.yy	    Selectable stopping angle: 000.00~360.00 degrees
                 */

                if (string.Compare(strPara3.Substring(0, 6), "ALIGND", true) == 0)
                {                    
                    
                    //ALIGNDyyy.yyN 
                    dAngle = ParseAlignAngle(strPara3.Substring(6, 6));
                    if (dAngle == -1)
                        return MakeException("NAK_PARA_MISS");
                    else if (dAngle < 0 || dAngle > 360)
                        return MakeException("NAK_DEGREE_OUTOFRANGE");

                    //Assign value
                    Cmd_iPara = 0; //0:AlignD
                    Cmd_ActionID = EFEMAction.ACTION_ALIGN;
                    Cmd_AlignAngle = dAngle;

                }
                else if (string.Compare(strPara3.Substring(0, 5), "ALIGN", true) == 0)
                {
                    //TODO:[Note]:Not support ALIGNN,return "NAK_CMD_NOT_SUPPORT"
                    return MakeException("NAK_CMD_NOT_SUPPORT");

                    //ALIGNxxN 
                    dAngle = ParseAlignAngle(strPara3.Substring(5, 2));
                    if (dAngle == -1)
                        return MakeException("NAK_PARA_MISS");
                    else if (dAngle < 1 || dAngle > 10)
                        return MakeException("NAK_DEGREE_OUTOFRANGE");

                    //Assign value
                    Cmd_iPara = 1; //1:AlignN
                    Cmd_ActionID = EFEMAction.ACTION_ALIGN;
                    Cmd_AlignAngle = dAngle;
                }
                else
                {
                    return MakeException("NAK_PARA_MISS");
                }

            }

            return null;
        }

        private HRESULT ParseCmd_GOTO(string CommandField)
        {
            /* HOST --> EFEM	GOTO/Pnxx_au;
             * Command parameters:
             * Port:	Required:
             * n	    Port number: 1~4 for loadports, 5 for aligner, A~E for loadlocks.
             * ss	    Source slot for wafer:  01~26; use only 01 for the aligner and loadlocks.
             * a	    Robot arm: A or B; use only A for a single-arm robot.
             * u	    Z transfer position: U or L; see description for information
             */
            
            string[] ParaField = CommandField.Split('/');

            if (ParaField.Length < 2)
                return MakeException("NAK_SLASH_MISS");

            string strPara1 = ParaField[1];

            if (strPara1 == null || strPara1[0] != 'P')
            {
                return MakeException("NAK_P_MISS");
            }
            else
            {
                EFEMStation Port = EFEMStation.NONE;
                int Slot = -1;
                EFEMRobotArm RobotArm = EFEMRobotArm.Lower; //Default[A:Single Arm(LowerArm)]
                EFEMRobotWaitingPos RobotWaitingPos = EFEMRobotWaitingPos.Unknown;

                string strParaField_Port = strPara1.Substring(1, 1);
                string strParaField_Slot = strPara1.Substring(2, 2);
                string strParaField_RobotArm = strPara1.Substring(5, 1);
                string strParaField_ZPosition = strPara1.Substring(6, 2);

                //Port
                Port = ParseStageID(strParaField_Port);
                if (Port == EFEMStation.NONE)
                    return MakeException("NAK_STAGE_OUTOFRANGE");

                //Slot
                Slot = ParseSlotID(strParaField_Slot);
                if (Slot < 1 || Slot > ConstVC.EFEM.MaxRegularLPSlotCount)
                    return MakeException("NAK_SLOT_PARAWRONG");

                //Robot Arm
                if (SimulateRorzeRobot != (int)SimuRorzeRobot.NoSimulate)
                {
                    if (SimulateRorzeRobot == (int)SimuRorzeRobot.AsRorze_LowerArm)
                        RobotArm = EFEMRobotArm.Lower;
                    else
                        RobotArm = EFEMRobotArm.Upper;
                }
                else
                {
                    //Robot Arm ([Default]A:Single Arm(LowerArm) , B:Another Arm(UpperArm))
                    if (string.Compare(strParaField_RobotArm, "A", true) == 0)
                        RobotArm = EFEMRobotArm.Lower;
                    else if (string.Compare(strParaField_RobotArm, "B", true) == 0)
                        RobotArm = EFEMRobotArm.Upper;
                    else
                        return MakeException("NAK_DESINATOR_MISS");
                }

                //Z Transfer Position
                if (string.Compare(strParaField_ZPosition, "U", true) == 0)
                    RobotWaitingPos = EFEMRobotWaitingPos.Upper;
                if (string.Compare(strParaField_ZPosition, "L", true) == 0)
                    RobotWaitingPos = EFEMRobotWaitingPos.Lower;
                else
                    return MakeException("NAK_UL_MISS");

                //Assign value
                Cmd_SlotId = Slot;
                Cmd_RobotWaitPos = RobotWaitingPos;
            }

            return null;
        }

        private HRESULT ParseCmd_STATUS(string CommandField)
        {

            string[] ParaField = CommandField.Split('/');

            if (ParaField.Length < 2)
                return MakeException("NAK_SLASH_MISS");

            string strPara1 = ParaField[1];   

            if (strPara1[0] != 'R')
                return MakeException("NAK_R_MISS");
            else if (strPara1[1] != 'B')
                return MakeException("NAK_B_MISS");

            return null;
        }

        private HRESULT ParseCmd_VACUUM(string CommandField)
        {
            /*
             * The VACUUM command allows the Host to control the vacuum on the robot end-effector and the aligner spindle.   
             * 
             * HOST ← EFEM	INF:VACUUM/VacControl/Equipment;
             *          	ABS:VACUUM/VacControl/Equipment;
             * Command parameters:
             * VacControl:	Required:
             * ON	        Turn vacuum on 
             * OFF	        Turn vacuum off
             * 
             * Equipment:	Required:
             * ARM1	        Robot ARM1, the upper arm
             * ALIGN	    Aligner spindle vacuum
             */

            string[] ParaField = CommandField.Split('/');

            if (ParaField.Length < 2)
                return MakeException("NAK_SLASH_MISS");

            bool VacControl = false; //True:Turn vacuum on , False:Turn vacuum off.

            string strPara1 = ParaField[1];
            string strPara2 = ParaField[2];      

            
            //Vacuun Control
            switch (strPara1)
            {
                case GB.VACUUM_PARAM_ON:
                    VacControl = true;
                    break;

                case GB.VACUUM_PARAM_OFF:
                     VacControl = false;
                    break;

                default:
                    return MakeException("NAK_PARA_MISS");
                    break;
            }

            //Assign value
            Cmd_ActionID = VacControl ? EFEMAction.ACTION_ROB_VAC_ON : EFEMAction.ACTION_ROB_VAC_OFF;

            return null;
        }

        private HRESULT ParseCmd_SETSPEED(string CommandField)
        {
            /*
             * The SETSPD command is used to set the robot speed to normal speed or slow speed.
             * HOST ← EFEM	INF:SETSPD/Speed;
             *              ABS:SETSPD/Speed;
             *              
             * Command parameters:
             * Speed:	Required:
             * FAST	    Relative robot speed set to 100% of normal
             * SLOW	    Relative robot speed set to 25% of normal
             */

            string[] ParaField = CommandField.Split('/');

            if (ParaField.Length < 2)
                return MakeException("NAK_SLASH_MISS");

            int iSpeedLevel = 2;
            
            int startIdx = CommandField.IndexOf('/');
            string strPara1 = CommandField.Substring(startIdx+1);

            //1:25%, 2:50%, 3:75%, 4:Fast
            switch (strPara1)
	        {
		        case GB.SPEED_PARAM_SLOW25:
                    iSpeedLevel = 1;
                    break;
                
                case GB.SPEED_PARAM_SLOW50:
                    iSpeedLevel = 2;
                    break;

                case GB.SPEED_PARAM_SLOW75:
                    iSpeedLevel = 3;
                    break;

                case GB.SPEED_PARAM_FAST:
                    iSpeedLevel = 4;
                    break;

                default:
                    return MakeException("NAK_PARA_MISS");
                    break;
        	}

            //Assign value
            Cmd_iPara = iSpeedLevel;

            return null;
        }

        private HRESULT ParseCmd_SETANGLE(string CommandField)
        {
            /*
             * The SETALNDEG command allows up to 10 preset alignment positions to be set.  
             *  Parameter xx:	    Required:
             *  01..10	            Preset number for orientation position(for TRANS*B use)
             *
             *  Parameter yyy.yy:	Required:
             *  000.00..360.00:	    Angular position in degrees of preset position(Current use)
             */

            string[] ParaField = CommandField.Split('/');

            if (ParaField.Length < 2)
                return MakeException("NAK_SLASH_MISS");

            string strPara1 = ParaField[1];
            string strPara2 = ParaField[2];
            
            int xx = 0;
            float yy = 0.0f;

            //xx(Preset number)
            if (!int.TryParse(strPara1, out xx))
                return MakeException("NAK_PARA_MISS");
            if (xx < 1 || xx > 10)
                return MakeException("NAK_DEGREE_OUTOFRANGE");

            //yy(Angular position)
            if (!float.TryParse(strPara2, out yy))
                return MakeException("NAK_PARA_MISS");
            if (yy < 0 || yy > 360.0)
                return MakeException("NAK_DEGREE_OUTOFRANGE");

            //Assign value
            Cmd_iPara = xx;
            Cmd_AlignAngle = yy;

            return null;
        }

        private HRESULT ParseCmd_CHKWFR(string CommandField)
        {

            string[] ParaField = CommandField.Split('/');

            if (ParaField.Length < 2)
                return MakeException("NAK_SLASH_MISS");

            string strPara1 = ParaField[1];

            return null;
        }

        private HRESULT ParseCmd_STGPOS(string CommandField)
        {
            string[] ParaField = CommandField.Split('/');

            if (ParaField.Length < 2)
                return MakeException("NAK_SLASH_MISS");

            string strPara1 = ParaField[1];   

            int LL_Index = 0;

            if (!Int32.TryParse(strPara1, out LL_Index))
            {
                return MakeException("NAK_PARA_MISS");
            }
            if (LL_Index < 1 || LL_Index > 2) //Only two Loadlock(LLA/LLB)
            {
                return MakeException("NAK_PARA_MISS");
            }

            return null;
        }

        private HRESULT ParseCmd_SAVELOG(string CommandField)
        {
            string[] ParaField = CommandField.Split('/');

            if (ParaField.Length < 2)
                return MakeException("NAK_SLASH_MISS");
            
            string strPara1 = ParaField[1];   

            if (!GB.strarrSAVELOGParam.Contains(strPara1))
                return MakeException("NAK_PARA_MISS");

            return null;
        }

        private HRESULT ParseCmd_CLRALM(string CommandField)
        {

            string[] ParaField = CommandField.Split('/');

            if (ParaField.Length < 2)
                return MakeException("NAK_SLASH_MISS");

            string strPara1 = ParaField[1];

            return null;
        }

        private HRESULT ParseCmd_SETWFRSNR(string CommandField)
        {
            /*
             * The SETWFRSNR command is used to set the wafer sensing mode             
             * HOST ← EFEM	ACK:SETWFRSNR/mode;
             *              INF:SETWFRSNR/mode;
             *              
             * Command parameters:
             * mode:	Required:             
             * 1:RobotSensor
             * 2:ExtendSensor
             * 3:BothSensor
             */

            string[] ParaField = CommandField.Split('/');

            if (ParaField.Length < 2)
                return MakeException("NAK_SLASH_MISS");            

            string strPara1 = ParaField[1];
            
            int iMode = 1;

            if (!Int32.TryParse(strPara1, out iMode))
                return MakeException("NAK_PARA_MISS");
            if(!Enum.IsDefined(typeof(WaferSensingMode),iMode))
                return MakeException("NAK_PARA_MISS");

            //Assign value
            Cmd_iPara = iMode;
            
            return null;
        }

        #endregion

        #region Private Data
       
        //=========================================================================================================//
        // === For Robot GetEFEMVersion variable  === //
        private string EFEM_Version = null;

        // === For VERSION ACK parameter  === //
        public string ACK_VERSION = null;

        //=========================================================================================================//
        // === For Robot GetStatus variable  === //
        private EFEMWaferStatus STATUS_IsWaferOnArm1,STATUS_IsWaferOnArm2, STATUS_IsWaferOnAligner;
        private EFEMDeviceStatus STATUS_Robot , STATUS_Aligner;
        private bool STATUS_IsRobotError,STATUS_IsAlignerError;
        private bool STATUS_IsSysVacuumOn, STATUS_IsSysAirOn;
        private bool STATUS_IsNeedHome;
        private EFEMWaferStatus STATUS_WaferSnrChk_Aligner, STATUS_WaferSnrChk_LP1, STATUS_WaferSnrChk_LP2, STATUS_WaferSnrChk_LP3;

        // === For STATUS ACK parameter  === //
        private int C5_iWaferOnUpperArm, C6_iWaferOnLowerArm, C7_iWaferOnAligner;
        private int M1_iRobotStatus, M2_iAlignerStatus;
        private bool M_bIsNeedHome;
        private int E1_iRobotError, E2_iAlignerError;
        private int S1_VacuumStatus, S2_CDAStatus;
        private int B1_AlignerWaferSnrChk, B2_LP1WaferSnrChk, B3_LP2WaferSnrChk, B4_LP3WaferSnrChk;
        //=========================================================================================================//  

        // === For ALNGTY INF parameter  === //
        private int N1_iAlignerType, N2_iWaferSize;

        //=========================================================================================================//        
        // === For Robot GetStatus variable  === //
        public double[] STGPOS_LLPos = new double[3] { 0, 0, 0 };
        public double STGPOS_OAngle;
        public bool STGPOS_IsTaught;

        // === For STGPOS INF parameter  === //
        public double[] INF_LLPos = new double[3] { 0, 0, 0 };

        //=========================================================================================================//
        // === For ERROR Report and ABS ErrorLevel/Code record  === //
        public int ABS_ErrorLevel, ABS_ErrorCode;

        //=========================================================================================================//        
        // === For Battery Abnormal status Get  === //
        private bool BatteryErr_Controller, BatteryErr_Robot;


        //=========================================================================================================//
        #endregion

        #endregion

        #region RorzeRobot Parsing/Decoding Function

        public class LOC
        {
            //============ Original =============//
            //public const int LOC_HOME = 0;
            //public const int LOC_ROBOT = 0;//ROBOT is HOME
            //public const int LOC_P1 = 1;
            //public const int LOC_P2 = 2;
            //public const int LOC_LL1 = 4;//Used for LL position for caller(no matter 200 or 300 mm wafer); 200 mm wafer LL position for this module
            //public const int LOC_LL2 = 5;//300 mm wafer LL position for this module
            //public const int LOC_ALIGNER = 3;
            //public const int LOC_UNKOWN = 6;//unknown or not inited
            //public const int LOC_P3 = 7;//Adatored Foup 1
            //public const int LOC_P4 = 8;//Adatored Foup 2

            //============ For New EFEM Use =============//
            //public const int LOC_UNKOWN = -1;//unknown or not inited
            //public const int LOC_HOME = 0;
            //public const int LOC_ROBOT = 0;//ROBOT is HOME
            //public const int LOC_P1 = 1;
            //public const int LOC_P2 = 2;
            //public const int LOC_P3 = 3;
            //public const int LOC_P4 = 4;
            //public const int LOC_ALIGNER = 5;

            //public const int LOC_P1_Adapt = 6;//Adatored Foup 1
            //public const int LOC_P2_Adapt = 7;//Adatored Foup 2
            //public const int LOC_P3_Adapt = 8;//Adatored Foup 3

            //public const int LOC_LL1 = 10;//Used for LL position for caller(no matter 200 or 300 mm wafer); 200 mm wafer LL position for this module
            //public const int LOC_LL2 = 11;//300 mm wafer LL position for this module
        }

        public class GB : LOC
        {
            #region Constant Declarations
            public const int LOOPBACK_SIMU = 0;
            public const int UNIT_TEST = 0;
            public static int iNotchFlat = 0;//use it until Alignment Failed and switch
            public const int MIN_MSG_LEN = 12;//LeoModify: 15 --> 12
            public const int MAX_MSG_LEN = 66;
            public const int MAX_SLOT_NO = 25;
            public const int MSG_FIRST_BYTE_IDX = 4;//Idx of the 1st byte => SOH(1byte), LEN(3byte), 1st byte

            public const int TRANS_SOURCE = 0;
            public const int TRANS_DEST = 1;

            public const byte MSG_SOH = 0x1;//SOH
            public const byte MSG_EOT = 0x3;//EOT   End Of Text
            public const byte MSG_SemiColon = 0x3B;//;
            public const byte MSG_VerticalBar = 0x7C;//|

            public const string MSG_RESP_INF = "INF:";   //inform event
            public const string MSG_RESP_ACK = "ACK:";   //Normal receipt, Ack
            public const string MSG_RESP_CAN = "CAN:";   //Reject command, Cancel command
            public const string MSG_RESP_NAK = "NAK:";   //Abnormal receipt, Nak
            public const string MSG_RESP_ABS = "ABS:";   //Abnormal end
            public static string[] strarrRespType = { MSG_RESP_INF, MSG_RESP_ACK, MSG_RESP_CAN, MSG_RESP_NAK, MSG_RESP_ABS };
            public const int IDX_MSG_RESP_INF = 0;
            public const int IDX_MSG_RESP_ACK = 1;
            public const int IDX_MSG_RESP_CAN = 2;
            public const int IDX_MSG_RESP_NAK = 3;
            public const int IDX_MSG_RESP_ABS = 4;

            public const string PARAM_ORGSH_ALL = "ALL";   //Origin search, all axis
            public const string PARAM_ORGSH_ROB = "ROBOT"; //Origin search, Robot
            public const string PARAM_ORGSH_ALI = "ALIGN"; //Origin search, ALIGNer
            public const string PARAM_ORGSH_P1 = "P1";    //Origin search, Stage 1
            public const string PARAM_ORGSH_P2 = "P2";    //Origin search, Stage 2
            public const int I_PARAM_ORGSH_ALL = 0;      //Idx, Origin search, all axis
            public const int I_PARAM_ORGSH_ROB = 1;      //Idx, Origin search, Robot
            public const int I_PARAM_ORGSH_ALI = 2;      //Idx, Origin search, ALIGNer
            public const int I_PARAM_ORGSH_P1 = 3;      //Idx, Origin search, Stage 1
            public const int I_PARAM_ORGSH_P2 = 4;      //Idx, Origin search, Stage 2
            public static string[] strarrORGSHParam = { PARAM_ORGSH_ALL, PARAM_ORGSH_ROB, PARAM_ORGSH_ALI, PARAM_ORGSH_P1, PARAM_ORGSH_P2 };

            public const string PARAM_ORGABS = "ROBOT";//Origin search ABS

            public const string CMD_READYCOMM = "READY/COMM"; //For compose command string

            //==========================================================================================
            public const string CMD_ORGSH = "ORGSH";   //Origin search
            public const string CMD_ORGABS = "ORGABS";  //Origin search, only when encoder data lost
            public const string CMD_TRANSB = "TRANS*B";
            public const string CMD_GOTO = "GOTO";
            public const string CMD_MAPING = "MAPING";
            public const string CMD_HOME = "HOME";
            public const string CMD_PAUSE = "PAUSE";
            public const string CMD_RESUME = "RESUME";
            public const string CMD_EMS = "EMS";
            public const string CMD_ABORT = "ABORT";
            public const string CMD_ERROR = "ERROR";   //request error content
            //11001-11999 Robot error                   14:loadport   15:Mapping  16:System error
            //12001-12999 Aligner error 
            //13001-13999 Controller error 
            public const string CMD_STATUS = "STATUS";
            public const string CMD_INIT2 = "INIT2";
            public const string CMD_INIT = "INIT";
            public const string CMD_REPORT = "REPORT";
            //public const string CMD_READY = "READY/COMM";            
            public const string CMD_READY = "INF:READY/COMM";
            public const string CMD_VACUUM = "VACUUM";
            public const string CMD_REPERR = "REPERR";
            public const string CMD_SETSPEED = "SETSPD";
            public const string CMD_SETANGLE = "SETALNDEG";
            public const string CMD_CHKWFR = "WAFCHECK/ALL";
            public const string CMD_CHK_ALNGTYP = "ALNGTYP";
            //public const string CMD_SETSPDSLOW   = "SETSPD/SLOW";
            public const string CMD_VERSION = "VERSION";
            public const string CMD_STGPOS = "GETSTGPOS";
            public const string CMD_SAVELOG = "SAVELOG";
            public const string CMD_ACKREADY = "ACK:READY/COMM";

            public const string CMD_SETWFRSNR = "SETWFRSNR"; //New Added command.
            public static string[] strarrCmd =
		{
			CMD_READY,CMD_ORGSH,CMD_REPORT,CMD_TRANSB,CMD_GOTO,CMD_MAPING,CMD_HOME,CMD_PAUSE,CMD_ERROR,
			CMD_RESUME,CMD_EMS,CMD_ABORT,CMD_REPERR,CMD_STATUS,CMD_INIT2,CMD_INIT,CMD_ORGABS,CMD_VACUUM,
			CMD_SETSPEED,CMD_SETANGLE, CMD_CHKWFR, CMD_CHK_ALNGTYP, /*CMD_SETSPDSLOW,*/ CMD_VERSION, CMD_STGPOS, CMD_SAVELOG,CMD_ACKREADY,
            CMD_SETWFRSNR
		};

            public const int IDX_READYCOMM = 0;
            public const int IDX_ORGSH = 1;
            public const int IDX_REPORT = 2;
            public const int IDX_TRANSB = 3;
            public const int IDX_GOTO = 4;
            public const int IDX_MAPING = 5;
            public const int IDX_HOME = 6;
            public const int IDX_PAUSE = 7;
            public const int IDX_ERROR = 8;
            public const int IDX_RESUME = 9;
            public const int IDX_EMS = 10;
            public const int IDX_ABORT = 11;
            public const int IDX_REPERR = 12;
            public const int IDX_STATUS = 13;
            public const int IDX_INIT2 = 14;
            public const int IDX_INIT = 15;
            public const int IDX_ORGABS = 16;
            public const int IDX_VACUUM = 17;
            public const int IDX_SETSPEED = 18;
            public const int IDX_SETANGLE = 19;
            public const int IDX_CHKWFR = 20;
            public const int IDX_CHK_ALNGTYP = 21;
            //public const int IDX_SETSPDSLOW = 22;
            public const int IDX_VERSION = 22;
            public const int IDX_STGPOS = 23;
            public const int IDX_SAVELOG = 24;
            public const int IDX_ACKREADY = 25;//for COMM setup only
            public static int[] iCmdTypeMotionData = new int[27]
		{
			/*IDX_READYCOMM 0*/  0,
			/*IDX_ORGSH     1*/  1,
			/*IDX_REPORT    2*/  0,
			/*IDX_TRANSB    3*/  1,
			/*IDX_GOTO      4*/  1,
			/*IDX_MAPING    5*/  1,
			/*IDX_HOME      6*/  1,
			/*IDX_PAUSE     7*/  1,
			/*IDX_ERROR     8*/  0,
			/*IDX_RESUME    9*/  1,
			/*IDX_EMS      10*/  1,
			/*IDX_ABORT    11*/  0,//Original = 1,change to 0
			/*IDX_REPERR   12*/  0,
			/*IDX_STATUS   13*/  0,
			/*IDX_INIT2    14*/  1,
			/*IDX_INIT     15*/  1,
			/*IDX_ORGABS   16*/  1,
			/*IDX_VACUUM   17*/  1,
			/*IDX_SETSPEED 18*/  1,
			/*IDX_SETANGLE 19*/  0,//Original = 1,change to 0
			/*IDX_CHKWFR   20*/  1,
			/*IDX_CHK_ALNGTYP 21*/  1,//need to wait for INF
			//*IDX_SETSPDSLOW 22*/  1, 
			/*IDX_VERSION  22*/  0,
			/*IDX_STGPOS   23*/  1,  //need to wait for INF:GETSTGPOS/X-Pos/Y-Pos/Z-Pos;
			/*IDX_SAVELOG  24*/  0,//Original = 1,change to 0
			/*IDX_ACKREADY 25*/  -1,  //-1
			/*IDX_SETWFRSNR 26*/ 1 ,  
		};
			public const int IDX_SETWFRSNR = 26;//New command
            
			public const int IDX_CLRALM = 27;   //New command , not imlpemented yet.
            
            //

            public const string NAK_FACTOR_CKSUM = "|CKSUM";
            public const string NAK_FACTOR_MESSAGE = "|MESSAGE";
            public const string NAK_FACTOR_TIMING = "|TIMING";
            public const string NAK_FACTOR_LENGTH = "|LENGTH";
            public static string[] strarrNakFactor = {NAK_FACTOR_CKSUM,NAK_FACTOR_MESSAGE,
														 NAK_FACTOR_TIMING,NAK_FACTOR_LENGTH};

            public const int PARAM_STATUS_WFR_UARM = 5;
            public const int PARAM_STATUS_WFR_LARM = 6;
            public const int PARAM_STATUS_WFR_ALIGN = 7;
            //------------
            public const byte PARAM_STATUS_VAL_NOWAFER = 0;
            public const byte PARAM_STATUS_VAL_WAFER = 1;
            //------------
            public const int PARAM_STATUS_ROBOT = 8;  //M1 
            public const int PARAM_STATUS_ALIGN = 9;  //M2
            public const int PARAM_STATUS_P1 = 10; //M3
            public const int PARAM_STATUS_P2 = 11; //M4
            public const int PARAM_STATUS_P3 = 12; //M5
            public const int PARAM_STATUS_P4 = 13; //M6
            //----------
            //public const byte PARAM_STATUS_VAL_WAITING = 0; 
            public const byte PARAM_STATUS_VAL_INMOTION = 1;
            public const byte PARAM_STATUS_VAL_INPAUSE = 2;
            public const byte PARAM_STATUS_VAL_WAITING = (byte)'A';
            public const byte PARAM_STATUS_VAL_REQUEST_L = (byte)'B';
            public const byte PARAM_STATUS_VAL_LOADING = (byte)'C';
            public const byte PARAM_STATUS_VAL_COMPLETE_L = (byte)'D';
            //public const byte PARAM_STATUS_VAL_WAITING = (byte)'M'; 
            //----------


            public const string PARAM1_TRANS = "/Pnxx";
            public const string PARAM1ARM_TRANS = "ARM";
            public const string PARAM_UARM = "_A";//ARM_A(Upper Arm),or P123_A(Port1Slot23 use UpperArm)
            public const string PARAM_LARM = "_B";
            public const string PARAM_ARM_USED = PARAM_UARM;//"_A"
            public const string PARAM3_TRANS = "ALIGN";   //align with the Index
            public const string PARAM3D_TRANS = "ALIGND";  //Align with the angle
            public const int I_PARAM_NO_ARM = -1;
            public const int I_PARAM_UARM = 0;
            public const int I_PARAM_LARM = 1;

            public const string PARAM_GOTO = "/Pnxx_au";

            //the StationIDs need to be the same as what JobExecutive uses
            public const int STAGE_ID_ROBOT = 0;
            public const int STAGE_ID_LP1 = 1;
            public const int STAGE_ID_LP2 = 2;
            public const int STAGE_ID_ALIGNER = 3;
            public const int STAGE_ID_LL = 4;//Used for LL position for caller(no matter 200 or 300 mm wafer); 200 mm wafer LL position for this module
            public const int STAGE_ID_LL2 = 5;//300 mm wafer LL position for this module
            public const int STAGE_ID_LP3 = 7;//added for loadport 1 adaptor, only used internally
            public const int STAGE_ID_LP4 = 8;//added for loadport 2 adaptor, only used internally 
            public const byte PARAM_PORT_LP1 = (byte)'1';//Actual LP1
            public const byte PARAM_PORT_LP2 = (byte)'2';//Actual LP2
            public const byte PARAM_PORT_LP3 = (byte)'3';//use for LP1 Adaptor
            public const byte PARAM_PORT_LP4 = (byte)'4';//use for LP2 Adaptor
            public const byte PARAM_PORT_LL = (byte)'A';//200 mm Wafer Position
            public const byte PARAM_PORT_LL2 = (byte)'B';//300 mm Wafer Position
            public const byte PARAM_PORT_ALIGN = (byte)'5';
            
            //For NEW EFEM
            public const int STAGE_ID_LP5 = 9;
            public const int STAGE_ID_LP6 = 10;//added for loadport 3 adaptor, only used internally 
            public const byte PARAM_PORT_LP5 = (byte)'6'; //Actual LP3
            public const byte PARAM_PORT_LP6 = (byte)'7'; //use for LP3 Adaptor

            public const byte PARAM_PLACE_LOC = (byte)'U';
            public const byte PARAM_PICK_LOC = (byte)'L';

            public const int I_NOTCH = 0;
            public const int I_FLAT = 1;
            public const byte PARAM_ALIGN_NOTCH = (byte)'N';
            public const byte PARAM_ALIGN_FLAT = (byte)'O';

            public const string PARAM_STATUS_RB = "RB";
            public const string PARAM_STATUS_P1MAPDATA = "P1";
            public const string PARAM_STATUS_P2MAPDATA = "P2";
            public static string[] strarrStatusParam = {
														   PARAM_STATUS_RB,PARAM_STATUS_P1MAPDATA,PARAM_STATUS_P2MAPDATA};
            public const int IDX_PARAM_STATUS_RB = 0;
            public const int IDX_PARAM_STATUS_P1MAPDATA = 1;
            public const int IDX_PARAM_STATUS_P2MAPDATA = 2;

            public const string PARAM_REPORT_WAFGET = "WAF_Get";
            public const string PARAM_REPORT_WAFPUT = "WAF_Put";
            public const string PARAM_REPORT_CASIN = "CAS_IN";
            public const string PARAM_REPORT_CASOUT = "CAS_OUT";
            public const string PARAM_REPORT_LOADSW = "LOADSW";
            public const string PARAM_REPORT_UNLDSW = "UNLDSW";
            public const string PARAM_REPORT_STATUS = "STATUS";
            public const string PARAM_REPORT_SLOTMAP = "SLOT_MAP";
            public static string[] strarrReportParam = {
														   PARAM_REPORT_WAFGET,
														   PARAM_REPORT_WAFPUT,
														   PARAM_REPORT_CASIN,
														   PARAM_REPORT_CASOUT,
														   PARAM_REPORT_LOADSW,
														   PARAM_REPORT_UNLDSW,
														   PARAM_REPORT_STATUS,
														   PARAM_REPORT_SLOTMAP};
            public const int IDX_REPORT_PARAM_WAFGET = 0;
            public const int IDX_REPORT_PARAM_WAFPUT = 1;
            public const int IDX_REPORT_PARAM_CASIN = 2;
            public const int IDX_REPORT_PARAM_CASOUT = 3;
            public const int IDX_REPORT_PARAM_LOADSW = 4;
            public const int IDX_REPORT_PARAM_UNLDSW = 5;
            public const int IDX_REPORT_PARAM_STATUS = 6;
            public const int IDX_REPORT_PARAM_SLOTMAP = 7;

            public const string VACUUM_PARAM_OFF = "OFF";
            public const string VACUUM_PARAM_ON = "ON";
            public static string[] strarrVacuumParam1 = { VACUUM_PARAM_OFF, VACUUM_PARAM_ON };
            public const int IDX_VACUUM_PARAM_OFF = 0;
            public const int IDX_VACUUM_PARAM_ON = 1;

            public const string VACUUM_PARAM_ALIGN_ID = "ALIGN";
            public const string VACUUM_PARAM_ARM1_ID = "ARM1";
            public const string VACUUM_PARAM_ARM2_ID = "ARM2";
            public static string[] strarrVacuumParam2 = { VACUUM_PARAM_ALIGN_ID, VACUUM_PARAM_ARM1_ID, VACUUM_PARAM_ARM2_ID };
            public const int IDX_VACUUM_PARAM_ALIGN_ID = 0;
            public const int IDX_VACUUM_PARAM_ARM1_ID = 1;
            public const int IDX_VACUUM_PARAM_ARM2_ID = 2;

            public const string SPEED_PARAM_SLOW = "SLOW";
            public const string SPEED_PARAM_FAST = "FAST"; //100%
            public const string SPEED_PARAM_SLOW25 = "SLOW/1"; //25%
            public const string SPEED_PARAM_SLOW50 = "SLOW/2"; //50%
            public const string SPEED_PARAM_SLOW75 = "SLOW/3"; //75%
            public static string[] strarrSpeedParam = { /*SPEED_PARAM_SLOW,*/ SPEED_PARAM_SLOW25, SPEED_PARAM_SLOW50, SPEED_PARAM_SLOW75, SPEED_PARAM_FAST };

            //not used /////////////////////////////////////////////////////
            public const int IDX_SPEED_PARAM_SLOW = 0;
            public const int IDX_SPEED_PARAM_FAST = 1;
            public const string SPEEDSLOW_PARAM_25 = "1";
            public const string SPEEDSLOW_PARAM_50 = "2";
            public const string SPEEDSLOW_PARAM_75 = "3";
            //0,                  1,                   2,                3 
            public static string[] strarrSpeedSlowParam = { SPEEDSLOW_PARAM_25, SPEEDSLOW_PARAM_25, SPEEDSLOW_PARAM_50, SPEEDSLOW_PARAM_75, SPEEDSLOW_PARAM_75 };

            public const string LL_1_POS_ID = "1";
            public const string LL_2_POS_ID = "2";
            //0,         1,           2,         3 
            public static string[] strarrLLPosIDParam = { LL_1_POS_ID, LL_1_POS_ID, LL_2_POS_ID, LL_2_POS_ID };

            public const string PARAM_CHKWFR_ALL = "ALL";   //Wafer presence to be checked on robot and aligner 
            public const string PARAM_CHKWFR_ROB = "ROBOT"; //Wafer presence to be checked on robot only
            public const string PARAM_CHKWFR_ALI = "ALIGN"; //Wafer presence to be checked on aligner only        
            public static string[] strarrCHKWFRParam = { PARAM_CHKWFR_ALL, PARAM_CHKWFR_ROB, PARAM_CHKWFR_ALI };

            public const string PARAM_SAVELOG_ALL = "ALL";   //Wafer presence to be checked on robot and aligner 
            public const string PARAM_SAVELOG_ROB = "ROBOT"; //Wafer presence to be checked on robot only
            public const string PARAM_SAVELOG_ALI = "ALIGN"; //Wafer presence to be checked on aligner only        
            public static string[] strarrSAVELOGParam = { PARAM_SAVELOG_ALL, PARAM_SAVELOG_ROB, PARAM_SAVELOG_ALI };

            public const string PARAM_CLRALM_ALL = "ALL";   //Clear Alarm on robot and aligner 
            public const string PARAM_CLRALM_ROB = "ROBOT"; //Clear Alarm on robot only
            public const string PARAM_CLRALM_ALI = "ALIGN"; //Clear Alarm on aligner only
            public static string[] strarrCLRALMParam = { PARAM_CLRALM_ALL, PARAM_CLRALM_ROB, PARAM_CLRALM_ALI };

            public const int STATUS_BYTE_NO = 9;

            public const int RORZE_ALARM_CODE_NONE = 0;
            public const int RORZE_ALARM_CODE_ROBOT_ERR = 1;
            public const int RORZE_ALARM_CODE_ALIGNER_ERR = 2;
            public const int RORZE_ALARM_CODE_CONTROLLER_ERR = 3;
            public const int RORZE_ALARM_CODE_LOADPORT_ERR = 4;//won't happen in this RorzeRobot implemtation
            public const int RORZE_ALARM_CODE_MAPPING_ERR = 5;//won't happen in this RorzeRobot implemtation
            public const int RORZE_ALARM_CODE_FFU_ERR = 6;
            public const int RORZE_ALARM_CODE_FFU_VAC_PRESSURE = 7;
            public const int RORZE_ALARM_CODE_FFU_AIR_PRESSURE = 8;
            public const int RORZE_ALARM_CODE_UNKNOWN_ERR = 9;

            //public static double[] dAlignAngle = new double[10]{0.01, 1.12, 2.23, 3.34, 4.45, 5.56, 1.12, 2.23, 3.34, 4.45};
            public static byte[] byarrSTATUSBuffer = new byte[50] //at least more than STATUS_BYTE_NO
                            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            public static char[] byarrMAPResult = new char[25];
            //public static byte[] byarrMAPResult = new byte[25]
            //{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
            public static int[] iLoadPortType = new int[2];
            public static int[] iLoadPortAdaptorType = new int[2];
            public static int[] iLoadPortWfrSize = new int[2];
            public static int[] iLoadPortMapper = new int[2];
            public const int LOADPORT_TYPE_NOT_EXIST = 0;
            public const int LOADPORT_TYPE_ASYST = 1;
            public const int LOADPORT_TYPE_BROOKS = 2;
            public const int LOADPORT_TYPE_RORZE_OC = 3;
            public const int LOADPORT_TYPE_BROOKS_ADAPTOR = 4;
            public const int LOADPORT_TYPE_OPEN_CASSETTE = 5;
            public const int LOADPORT_TYPE_UNKNOWN = 6;

            //public static RORZE_SERVICE_CLASS RxService = new RORZE_SERVICE_CLASS();
            //public static RORZE_SERVICE_CLASS SendService = new RORZE_SERVICE_CLASS();

            public static bool boolACKEventON = false;
            public static bool boolNAKEventON = false;
            public static bool boolINFEventON = false;
            public static bool boolCANEventON = false;
            public static bool boolABSEventON = false;

            public static int iDoorOpenCounter = 0;
            public static bool boolPreDoorOpenSwitchON = false;
            public static bool boolDoorOpenSwitchON = false;
            public static bool boolRecvdDoorOpenEvent = false;
            public static bool boolDoorOpenSwitchAlarmSet = false;

            public static int iFFUErrorCounter = 0;
            public static bool boolPreFFUError = false;
            public static bool boolFFUError = false;
            public static bool boolRecvdFFUErrorEvent = false;
            public static bool boolFFUErrorAlarmSet = false;

            public static bool boolRecvdFFUControllerEvent = false;//16001
            public static bool boolRecvdFFU16002Event = false;
            public static bool boolRecvdFFU16003Event = false;
            //public static bool boolRecvdFFU16004Event = false;  //reserved
            public static bool boolRecvdFFU16005Event = false;
            public static bool boolRecvdFFU16006Event = false;

            public static bool boolRecvdINFComReady = false;

            public static int iRorzeAlarmCategory = RORZE_ALARM_CODE_NONE;
            public static int iErrorCode = 0;

            public const int RORZE_RETRACT_BIT_FIRMWARE_INSTALLED = 1;
            public static int iArmNotRetracted = 0;     //0/1   0: Arm Retracted / 1: Arm Not Retracted
            public static int iWaferOnUpperArm = 0;     //0/1   only Upper Arm is used
            public static int iWaferOnLowerArm = 0;     //0/1   not used (Lower Arm is removed)
            public static int iWaferOnAligner = 0;      //0/1
            public static int iWaferOnArmAtAligner = -1;  //0/1
            public static int iWaferOnArmAtLP1 = -1;      //0/1
            public static int iWaferOnArmAtLP2 = -1;      //0/1
            public static int iRobotStatus = 0;         //0/1/2/3(?)  Idle/Moving/Paused/NotORGSH
            public static int iAlignerStatus = 0;       //0/1/2/3(?)  Idle/Moving/Paused/NotORGSH
            public static int iRobotErrorStatus = 0;    //0/1/2 NoError/AbleConnectError/UnableConnectError
            public static int iAlignerErrorStatus = 0;  //0/1/2 NoError/AbleConnectError/UnableConnectError
            public static int iVacuumSourcePressure = 0;//0/1 NoPressure/PressureExist
            public static int iAirSourcePressure = 0;   //0/1 NoPressure/PressureExist
            public static int GRIPPER_ARM = 0;
            public static int iArmWaferSensorOn = 0;
            public static int iArmWaferSensorInstalled = 0;
            public static bool bTAIWAN_RORZE = false;

            #endregion

            static public int CopyString(byte[] byarrA, ref int iStartPos, string strB)
            {
                int i = 0;
                for (; i < strB.Length; i++)
                    byarrA[iStartPos++] = (byte)strB[i];
                return i;
            }

            static public int CopyString(byte[] byarrA, ref int iStartPos, byte[] byarrB, int iLength)
            {
                int i = 0;
                for (; i < iLength; i++)
                    byarrA[iStartPos++] = byarrB[i];
                return i;
            }
            static public int CompByteArray(byte[] byarrA, byte[] byarrB, int iAStartPos, int iBStartPos, int iLength)
            {
                for (int i = 0; i < iLength; i++)
                {
                    if (byarrA[i + iAStartPos] != byarrB[i + iBStartPos])
                        return -1;
                }
                return 0;
            }

            static public int CompByteString(byte[] byarrA, string strB, int iAStartPos, int iLength)
            {
                for (int i = 0; i < iLength; i++)
                {
                    if (byarrA[i + iAStartPos] != strB[i])
                        return -1;
                }
                return 0;
            }

            static public int CompByteString(byte[] byarrA, string strB, int iAStartPos)
            {
                for (int i = 0; i < strB.Length; i++)
                {
                    if (byarrA[i + iAStartPos] != strB[i])
                        return -1;
                }
                return 0;
            }

            //Calculate and Add two bytes of ChkSum at the end
            static public void AddCheckSum(byte[] byarrA, int iStartPos, int iEndPos)
            {
                int i, CheckSum = 0;

                for (i = iStartPos; i <= iEndPos; i++)
                    CheckSum += byarrA[i];
                byarrA[iEndPos + 1] = (byte)((CheckSum & 0xf0) >> 4);
                byarrA[iEndPos + 2] = (byte)(CheckSum & 0xf);
                if (byarrA[iEndPos + 1] > 9)
                    byarrA[iEndPos + 1] += ('A' - 10);
                else
                    byarrA[iEndPos + 1] += (byte)'0';
                if (byarrA[iEndPos + 2] > 9)
                    byarrA[iEndPos + 2] += ('A' - 10);
                else
                    byarrA[iEndPos + 2] += (byte)'0';
            }

            //convert integer (iLength)into 3 bytes of ASCII and save in byarrA[1] to byarrA[3]
            static public void AddLEN(byte[] byarrA, int iLength)
            {
                int i, iTempLength = iLength;

                for (i = 3; i > 0; i--)
                {
                    byarrA[i] = (byte)(iTempLength % 10);
                    iTempLength = iTempLength / 10;
                    if (byarrA[i] > 9)
                        byarrA[i] += ('A' - 10);
                    else
                        byarrA[i] += (byte)'0';
                }
            }
            //Add SOH, LEN, ';', ChkSum, & EOT     Try to avoid two ';'s
            static public int AddMsgFrame(byte[] byarrA, ref int iEndIdx) //iEndIdx: Idx of ChkSum[1]
            {
                if (byarrA[iEndIdx - 1] == ';')
                    iEndIdx--;
                int iTotalLength = iEndIdx + 1 + 2 + 1; //SOH+LEN+MSG+;+CHKSUM+EOT=1+3+(iEndIdx-4)+1+2+1
                int iLEN = iTotalLength - 5; //MSG+CHKSUM=iTotalLength-SOH-LEN-EOT
                if (byarrA.Length < iTotalLength) return -1;
                byarrA[0] = MSG_SOH;
                byarrA[iEndIdx] = (byte)';';
                byarrA[iTotalLength - 1] = MSG_EOT;
                AddLEN(byarrA, iLEN);
                AddCheckSum(byarrA, 1, iEndIdx);//LEN+MSG
                iEndIdx += 4;
                return 0;
            }
                        
            //skip the first space (only the first byte)
            //skip the 0x or 0X
            //stop if 0, 0x0d, '/', or space found at the end
            static public int ConvertHex(byte[] byarrA, int iStartPos, int iMaxLength, ref int iActualLength)
            {
                int iRes = 0, i = iStartPos;
                if (byarrA[i] == ' ')
                    i++;
                if (byarrA[i] == '0' && (byarrA[i + 1] == 'x' || byarrA[i + 1] == 'X'))
                    i += 2;
                for (int j = 0; j < iMaxLength && (byarrA[i] != 0 && byarrA[i] != ' ' && byarrA[i] != '/' && byarrA[i] != 0xd); j++, i++)
                {
                    if (byarrA[i] >= '0' && byarrA[i] <= '9')
                        iRes = (iRes << 4) + byarrA[i] - '0';
                    else
                        if (byarrA[i] >= 'a' && byarrA[i] <= 'f')
                            iRes = (iRes << 4) + 10 + byarrA[i] - 'a';
                        else
                            if (byarrA[i] >= 'A' && byarrA[i] <= 'F')
                                iRes = (iRes << 4) + 10 + byarrA[i] - 'A';
                            else
                            {
                                iActualLength = i - iStartPos;
                                return -1;//wrong data quit
                            }
                }
                iActualLength = i - iStartPos;
                return iRes;
            }
            
            static public int GetRespType(byte[] byarrA, ref int iArrIdx)
            {
                for (int i = 0; i < strarrRespType.Length; i++)
                {
                    if (CompByteString(byarrA, strarrRespType[i], iArrIdx) == 0)
                    {
                        iArrIdx += strarrRespType[i].Length;
                        return i;
                    }
                }
                return -1;
            }

            static public int GetNAKFactor(byte[] byarrA, ref int iArrIdx)
            {
                for (int i = 0; i < strarrNakFactor.Length; i++)
                {
                    if (CompByteString(byarrA, strarrNakFactor[i], iArrIdx) == 0)
                    {
                        iArrIdx += strarrNakFactor[i].Length;
                        return i;
                    }
                }
                return -1;
            }
            static public int GetCmdCode(byte[] byarrA, ref int iArrIdx)
            {
                return GetStringCode(byarrA, ref iArrIdx, 1);
            }
            static public int GetStringCode(byte[] byarrA, ref int iArrIdx, string[] strarrB)
            {
                for (int i = 0; i < strarrB.Length; i++)
                {
                    if (CompByteString(byarrA, strarrB[i], iArrIdx) == 0)
                    {
                        iArrIdx += strarrB[i].Length;
                        return i;
                    }
                }
                return -1;
            }
            static public int GetStringCode(byte[] byarrA, ref int iArrIdx, int iKind)
            {
                string[] strarrB = null;
                switch (iKind)
                {
                    case 1:
                        strarrB = strarrCmd;
                        break;
                    case 2:
                        strarrB = strarrCmd;
                        break;
                    case 3:
                        strarrB = strarrNakFactor;
                        break;
                    case 4:
                        strarrB = strarrReportParam;
                        break;
                }
                for (int i = 0; i < strarrB.Length; i++)
                {
                    if (CompByteString(byarrA, strarrB[i], iArrIdx) == 0)
                    {
                        iArrIdx += strarrB[i].Length;
                        return i;
                    }
                }
                return -1;
            }
            static public int GetReportParamCode1(byte[] byarrA, ref int iArrIdx)
            {
                int iTemp = iArrIdx, iRet;
                if (byarrA[iArrIdx] == '/')
                {
                    iTemp++;
                    iRet = GetStringCode(byarrA, ref iTemp, 4);
                    if (iRet != -1)
                    {
                        iArrIdx = iTemp;
                        return iRet;
                    }
                }
                return -1;
            }
            static public int GetReportParamCode2(byte[] byarrA, ref int iArrIdx, int iDataCode)
            {
                //for now iDataCode=2 => Event occurrance port, kind 2
                int iTemp = iArrIdx, iStageId = -1, iSlotId = -1;
                if (byarrA[iTemp++] == '/')
                {
                    if (byarrA[iTemp++] == 'P')
                    {
                        switch (byarrA[iTemp++])
                        {
                            case (byte)'1':
                                iStageId = 0;
                                if (byarrA[iTemp++] == 'S')
                                    iSlotId = GetInteger(byarrA, ref iTemp);
                                break;
                            case (byte)'2':
                                iStageId = 1;
                                if (byarrA[iTemp++] == 'S')
                                    iSlotId = GetInteger(byarrA, ref iTemp);
                                break;
                            case (byte)'A':
                                iStageId = 2;
                                iSlotId = 1;
                                break;
                            case (byte)'B':
                                iStageId = 3;
                                iSlotId = 1;
                                break;
                            case (byte)'5':
                                iStageId = 4;
                                iSlotId = 1;
                                break;
                            default:
                                iStageId = -1;
                                iSlotId = -1;
                                break;
                        }
                    }
                }
                if (iSlotId != -1)
                {
                    iArrIdx = iTemp;
                    return (iStageId << 8) + iSlotId;
                }
                return -1;
            }

            static public int GetInteger(byte[] byarrA, ref int iIdx)
            {
                int i = iIdx, iRes = 0;
                if (byarrA[i] == ' ')
                    i++;
                for (; i < byarrA.Length && byarrA[i] != ';' && byarrA[i] != '/' && byarrA[i] != ' ' && byarrA[i] != ',' && byarrA[i] != 0xd; i++)
                {
                    if (byarrA[i] >= '0' && byarrA[i] <= '9')
                        iRes = (iRes * 10) + byarrA[i] - '0';
                    else
                        return -1;//wrong data quit
                }
                iIdx = i;
                return iRes;
            }

            static public string GetStrData(byte[] byarrA, ref int iIdx)
            {
                int i = iIdx;
                int iTotalByte = 0;
                string strRes = null;
                byte[] byarrTmp = new byte[20];
                for (; i < byarrA.Length && byarrA[i] != ';' && byarrA[i] != '/' && byarrA[i] != 0xd; i++, iTotalByte++)
                    byarrTmp[i - iIdx] = byarrA[i];
                if (i > iIdx)
                {
                    char[] charrTmp = new char[iTotalByte];
                    for (int k = 0; k < iTotalByte; k++)
                        charrTmp[k] = (char)byarrTmp[k];
                    strRes = new string(charrTmp);
                    iIdx = i;
                }
                return strRes;
            }           

        }

        public enum EFEMAction
        {
            ACTION_NONE = 0,
            ACTION_GET = 1,
            ACTION_PUT = 2,
            ACTION_ALIGN = 3,
            ACTION_SETANGLE = 4,
            ACTION_HOME = 5,
            ACTION_INIT2 = 6,
            ACTION_STATUS = 7,
            ACTION_ROB_VAC_ON = 8,
            ACTION_ROB_VAC_OFF = 9,
            ACTION_ACK_READY_COMM = 10,
            ACTION_SPEED_FAST = 11,
            ACTION_SPEED_SLOW = 12,
            ACTION_CHKWFR = 13,
            ACTION_CHK_ALNGTYP = 14,
            ACTION_SETSPDSLOW = 15,
            ACTION_VERSION = 16,
            ACTION_STGPOS = 17,
            ACTION_SAVELOG = 18,

            //Additional ACTION(Define in manual but rarely use)
            ACTION_GOTO = 19,
            ACTION_EMS = 20,
            ACTION_CLRALM = 21,
            ACTION_SETWFRSNR = 22,
        }

        #endregion

        public int GetLoadportSlotCount(EFEMStation StationID)
        {
            return GB.MAX_SLOT_NO;//25
            
        }
        
        public object GetVariable(string Key)
        {
            try
            {
                if (_varCenter.IsVariableExist(Key))
                {
                    return _varCenter.GetValue(Key); // remote procedure call            
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        protected HRESULT MakeException(string errKey, bool writrLog = true)
        {
            if (string.IsNullOrWhiteSpace(errKey))
                return null;

            if (_ExDictionary == null)
            {
                return null;
            }
            else
            {
                if (_ExDictionary.Contains(errKey))
                {
                    HRESULT hr = _ExDictionary[errKey].hRESULT;
                    if (writrLog)
                        WriteLog(hr);
                    return hr;
                }
                else
                {
                    return null;
                }
            }
        }
        protected HRESULT MakeException(Exception ex, bool writrLog = true)
        {
            if (ex == null)
                return null;

            HRESULT hr = MakeException("SOFTWARE_EXCEPTION", false);
            hr._extramessage = ex.Message;

            if (writrLog)
                WriteLog(hr, ex.StackTrace);

            return hr;
        }

        public void OnVariableCenterCallback(string VariableName, object Value)
        {
            if (ConstVC.LogConfig.LogVCInHost)
                WriteLog(LogLevel.DEBUG, LogHeadType.Data, string.Format("Get VC Callback. VarName = {0}, Value = {1}", VariableName, ExtendMethods.ToStringHelper(Value)));
            
        }

    }    


    //Do not modify the enum name here!!
    public enum CMD_NAKType
    {
        [HMIDescription("Unknown Type")]
        UNKNOWN = -1,
        [HMIDescription("ACK OK")]
        ACK = 0,
        
        #region ===  NAK Section  ===
        [HMIDescription("NAK: LEN wrong")]
        NAK_LEN_WRONG = 1,
        [HMIDescription("NAK: Semicolon missing")]
        NAK_SEMICOL_MISS = 2,
        [HMIDescription("NAK: Forward slash missing")]
        NAK_SLASH_MISS = 3,
        [HMIDescription("NAK: Parameter missing or wrong")]
        NAK_PARA_MISS = 4,
        [HMIDescription("NAK: Parameter [P] is missing")]
        NAK_P_MISS = 5,
        [HMIDescription("NAK: Stage number out of range")]
        NAK_STAGE_OUTOFRANGE = 6,
        [HMIDescription("NAK: Loadport not in use")]
        NAK_LP_NOTINUSE = 7,
        [HMIDescription("NAK: Aligner not in use")]
        NAK_ALIGNER_NOTINUSE = 8,
        [HMIDescription("NAK: ARM2 not in use")]
        NAK_ARM2_NOTINUSE = 9,
        [HMIDescription("NAK: Arm not in use")]
        NAK_ARM_NOTINUSE = 10,

        [HMIDescription("NAK: Slot parameter is wrong")]
        NAK_SLOT_PARAWRONG = 11,               
        [HMIDescription("NAK: Underscore missing")]
        NAK_UNDERSCORE_MISS = 12,
        [HMIDescription("NAK: A or B designator missing")]
        NAK_DESINATOR_MISS = 13,        
        [HMIDescription("NAK: U or L missing")]
        NAK_UL_MISS = 14,
        [HMIDescription("NAK: Parameter [R] missing")]
        NAK_R_MISS = 15,
        [HMIDescription("NAK: Parameter [B] missing")]
        NAK_B_MISS = 16,        

        [HMIDescription("NAK: No parameter specified")]
        NAK_NOPARASPECIFIED = 17,                       
        [HMIDescription("NAK: Degrees parameter out of range")]
        NAK_DEGREE_OUTOFRANGE = 18,
        
        [HMIDescription("NAK: Invalid Start or End code")]
        NAK_HEADERCODE_INVALID = 31,
        [HMIDescription("NAK: Length out of range")]
        NAK_LENGTH_OUTOFRANGE = 32,
        [HMIDescription("NAK: Command not support.")]
        NAK_CMD_NOT_SUPPORT = 33,
        [HMIDescription("NAK: Command  not exist in list.")]
        NAK_CMD_NOT_EXIST = 34,

        #endregion

    }
    
    /// <summary>
    /// HMIDescription compose "Error/Parameter" message.
    /// </summary>
    public enum CMD_CANType
    {
        [HMIDescription("ACK OK")]
        ACK = 100,        
        
        [HMIDescription("EMS/none")]
        EMS ,
        [HMIDescription("ERROR/none")]
        ERROR_EFEM,
        [HMIDescription("NOTRUNMODE/none")]
        NOTRUNMODE,
        [HMIDescription("MAG_ERROR/none")]
        MAG_ERROR,
        [HMIDescription("NOTREADY/none")]
        NOTREADY_COMM,
        [HMIDescription("NOTREADY/none")]
        NOTREADY_EFEM,
        [HMIDescription("NOTORG/none")]
        NOTORG,
        [HMIDescription("SRCAIR/System")]
        SRCAIR,
        [HMIDescription("SRCVAC/System")]
        SRCVAC,
        [HMIDescription("RUNNING/none")]
        RUNNING,//110
        [HMIDescription("NOTINMOTION/none")]
        NOTINMOTION,
        [HMIDescription("HOLD/none")]
        HOLD,
        [HMIDescription("NOPAUSE/none")]
        NOPAUSE,
        [HMIDescription("RUNNING/ROBOT")]
        RUNNING_ROBOT,
        [HMIDescription("RUNNING/ALIGN")]
        RUNNING_ALIGNER,
        [HMIDescription("NOWAF/ALIGN")]
        NOWAF_ALIGN,
        [HMIDescription("WAFER/ALIGN")]
        WAFER_ALIGN,  
        [HMIDescription("NOWAF/ARM1")]
        NOWAF_ARM1,
        [HMIDescription("WAFER/ARM1")]
        WAFER_ARM1,
        [HMIDescription("NOWAF/ARM2")]
        NOWAF_ARM2,//120
        [HMIDescription("WAFER/ARM2")]
        WAFER_ARM2,

        [HMIDescription("CAN_CONDITION_NOT_ALLOW")]
        CONDITION_NOT_ALLOW,
        
    }

    public class ReportError
    {
        private Nullable<REPERR_Type> _type;
        private Nullable<REPERR_Level> _level;

        public ReportError()
        {
            _type = null; 
            _level = null;
        }

        public ReportError(REPERR_Type type, REPERR_Level level)
        {
            _type = type;
            _level = level;
        }

        public REPERR_Type? Type
        {
            set { _type = value; }
            get { return _type; }
        }

        public REPERR_Level? Level
        {
            set { _level = value; }
            get { return _level; }
        }

    }

    public enum REPERR_Level
    {
        [HMIDescription("Minor Error")]
        MINOR = 0,
        [HMIDescription("Major Error")]
        MAJOR = 1,

    }
    public enum REPERR_Type
    {
        //OLD EFEM(Reserve)
        [HMIDescription("DOOR_INTERLOCK")]
        DOOR_INTERLOCK = 1,
        
        //[HMIDescription("FFU_CONTROLLER_ERROR")]
        //FFU_CONTROL_ERR = 2,
        //[HMIDescription("FFU_UPPERLIMIT_ERROR")]
        //FFU_UP_LIMIT = 3,
        //[HMIDescription("FFU_LOWERLIMIT_ERROR")]
        //FFU_LOW_LIMIT = 4,
        //[HMIDescription("ALIGNER_FAN_ERROR")]
        //ALIGNER_FAN_ERR = 5,
        //[HMIDescription("VAC_ERROR")]
        //VAC_ERR = 6,
        //[HMIDescription("AIR_ERROR")]
        //AIR_ERR = 7,
        
        //NEW EFEM
        [HMIDescription("ROBOT_BATTERY_LOW")]
        RBT_BATTERY_LOW,
        [HMIDescription("CONTROLLER_BATTERY_LOW")]
        CONTROL_BATTERY_LOW,
        [HMIDescription("FFU_PWR_ALARM")]
        FFU_PWR_ALARM ,
        [HMIDescription("VAC_UPPER_ERROR")]
        VAC_UP_ERR ,
        [HMIDescription("VAC_LOWER_ERROR")]
        VAC_LOW_ERR ,
        [HMIDescription("AIR_UPPER_ERROR")]
        AIR_UP_ERR,
        [HMIDescription("AIR_LOWER_ERROR")]
        AIR_LOW_ERR,
        [HMIDescription("N2_UPPER_ERROR")]
        N2_UP_ERR ,
        [HMIDescription("N2_LOWER_ERROR")]
        N2_LOW_ERR ,
        [HMIDescription("PRESSURE_DIFF_1_ERROR")]
        PRESSURE_DIFF_1_ERR ,
        [HMIDescription("PRESSURE_DIFF_2_ERROR")]
        PRESSURE_DIFF_2_ERR ,
        
        [HMIDescription("FFU_SPEED_HIGH")]
        FFU_SPEED_HIGH ,
        [HMIDescription("FFU_SPEED_LOW")]
        FFU_SPEED_LOW ,
        [HMIDescription("FFU_OVER_STDDEV")]
        FFU_OVER_STDDEV

    }

    [HMIAuthor("Lawrence Ouyang", version = 1.0, Remark = "2015/07/30: Baseline.")]
    public class VariableCenterSink
    {
        private static VariableCenterSink _instance = null;

        public static VariableCenterSink Instance()
        {
            if (_instance == null)
                _instance = new VariableCenterSink();

            return _instance;
        }
    }

}

