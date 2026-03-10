using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Diagnostics;
using EFEMInterface;
using EFEM.LogUtilities;
using EFEM.DataCenter;
using EFEM.ExceptionManagements;
using EFEM.FileUtilities;
using EFEMInterface.CommunicationChannel;
using EFEM.VariableCenter;

namespace EFEMMachineControl
{
    [HMIAuthor("Lawrence Ouyang", version = 1.0, Remark = "2015/07/30: Baseline.")]
    public class MachineControlImplement : AbstractMachineControl
    {
        #region DebugMode
        public bool IsForceIgnoreKeyLockSwitch { get { return _ForceIgnoreKeyLockSwitch; } }
        public bool IsProtectionDisabled4Debug { get { return MachineControlFactory.DisableProtect4Debug; } }
        public bool IsProtectionDisabled4Run { get { return MachineControlFactory.DisableProtect4Run; } }
        public bool IsProtectionDisabled4Maintain { get { return MachineControlFactory.DisableProtect4Maintain; } }
        #endregion

        protected char MainVersion = '1';
        protected char SubVersion = '0'; //used for Host to get the LP count (ex: SubVersion = 3 means 3 LPs is supported)
        protected bool _isChgRunModeSensorAllow = true;
        protected bool _isHostChgWaferSensingMode = false; //Did Host changed the mode?
        protected bool _isOfflineMode = true; //User selection or during Initialize
        protected bool _isRobotNeedHome = true;
        protected bool _isAlignerNeedHome = true;
        /// <summary>
        /// Password: "HMIEFEM_KEYSWITCH_DISABLE"
        /// If set this value to true, EFEM will still perform the command send from Host even switch keylock to Stop / Maintain position
        /// </summary>
        protected bool _ForceIgnoreKeyLockSwitch = false; //!!!Warning Safety Issue!!! 
        protected EFEMStatus _efemKeyPosition = EFEMStatus.Unknown; //Trigger by key switch (Run/Maintain/Stop). DONOT assign this value to EFEMStatus.Teach
        protected EFEMStatus preEFEMStatus = EFEMStatus.Unknown;
        protected int _LPCount = ConstVC.EFEM.DefaultLPCount;

        protected bool disposed = false;
        protected bool _IsBusy = false;
        protected static AbstractMachineControl uniqueInstance = null;
        protected string objectName = ConstVC.ObjectName.MachineControl;
        protected object _lockInit = new object();
        protected object _lockOnline = new object();
        protected object _lockDIInterlock = new object();
        protected object _lockException = new object();

        protected bool _isDIO0OnLine = false;
        protected bool[] _curDIO0Input = null;
        protected bool[] _dioInInterlock = null;
        protected bool _interLockDisabledInRunMode = false;
        protected bool _interLockDIOfflineDisable = false;

        protected AbstractExceptionManagement _ExManager = null;
        protected ExceptionDictionary _ExDictionary = null;
        protected AbstractLogUtility _log = null;
        protected AbstractFileUtilities _fu = null;
        protected EFEMVariableCenter _varCenter = null;

        #region Parameters for Run / Maintan mode (will download to robot automatically while mode switched)
        protected WaferSensingMode _wcModeinRun = WaferSensingMode.Unknwon;
        protected WaferSensingMode _wcModeinMaintain = WaferSensingMode.Unknwon;
        protected WaferSensingMode _CurWaferCheckingMode = WaferSensingMode.Unknwon;
        
        //For speed level control
        protected int _curRunLevelSel = ConstVC.EFEM.DefaultRunLevel;
        protected float[] _robotSpeedLevel = new float[] {
            ConstVC.EFEM.DefaultRobotSpeedForRun[0], 
            ConstVC.EFEM.DefaultRobotSpeedForRun[1],
            ConstVC.EFEM.DefaultRobotSpeedForRun[2],
            ConstVC.EFEM.DefaultRobotSpeedForRun[3]};
        protected float[] _alignerSpeedLevel = new float[] {
            ConstVC.EFEM.DefaultAlignerSpeedForRun[0], 
            ConstVC.EFEM.DefaultAlignerSpeedForRun[1],
            ConstVC.EFEM.DefaultAlignerSpeedForRun[2],
            ConstVC.EFEM.DefaultAlignerSpeedForRun[3]};

        protected float _robotSpeedRun = ConstVC.EFEM.DefaultRobotSpeedForRun[ConstVC.EFEM.DefaultRunLevel]; //speed (%)
        protected float _robotSpeedMaintain = ConstVC.EFEM.DefaultRobotSpeedForMaintain; //speed (%)
        protected float _CurRobotSpeed = ConstVC.EFEM.DefaultRobotSpeedForMaintain; //speed (%)

        protected float _alignerSpeedRun = ConstVC.EFEM.DefaultAlignerSpeedForRun[ConstVC.EFEM.DefaultRunLevel]; //speed (%)
        protected float _alignerSpeedMaintain = ConstVC.EFEM.DefaultAlignerSpeedForMaintain; //speed (%)
        protected float _CurAlignerSpeed = ConstVC.EFEM.DefaultAlignerSpeedForMaintain; //speed (%)

        protected double _alignerAngleRun = ConstVC.EFEM.DefaultPreAlignAngle;
        protected double _alignerAngleMaintain = ConstVC.EFEM.DefaultPreAlignAngle;
        protected double _CurAlignerAngle = ConstVC.EFEM.DefaultPreAlignAngle;
        #endregion

        public event EFEMStatusChanged OnEFEMStatusChanged;
        public event WaferSensingModeChanged OnWaferSensingModeChanged;

        //Will reject motion command from Host if any exception existed 
        protected Hashtable _ExQueue = new Hashtable();
        protected int CurrentAlarmCount = 0;
        protected int CurrentErrorCount = 0;
        protected int CurrentWarningCount = 0;

        internal static AbstractMachineControl GetUniqueInstance()
        {
            if (uniqueInstance == null)
            {
                uniqueInstance = new MachineControlImplement();
            }
            return uniqueInstance;
        }

        protected MachineControlImplement() : base()
        {
            OnEFEMStatusChanged += new EFEMStatusChanged(MachineControlImplement_OnEFEMStatusChanged);
        }

        protected void MachineControlImplement_OnEFEMStatusChanged(EFEMStatus status, bool isOnline)
        {
            if (preEFEMStatus != status)
            {
                WriteLog(LogHeadType.Info, string.Format("EFEM Status Changed. [{0}] -> [{1}]", preEFEMStatus, status));

                _isRobotNeedHome = true;
                _isAlignerNeedHome = true;

                bool isNeedToCheckInterLock = false;

                if (preEFEMStatus != EFEMStatus.Run && status == EFEMStatus.Run)
                    isNeedToCheckInterLock = true;

                if (preEFEMStatus == EFEMStatus.Run && status != EFEMStatus.Run)
                    isNeedToCheckInterLock = true;

                preEFEMStatus = status;

                if (!IsDIO0Online && !IsProtectionDisabled4Debug)
                    return;

                if (status == EFEMStatus.Run)
                {
                    if (_wcModeinRun != WaferSensingMode.Unknwon)
                    {
                        if (_CurWaferCheckingMode != _wcModeinRun)
                        {
                            _CurWaferCheckingMode = _wcModeinRun;
                            FireCurrentWaferSensingModeChanged(_CurWaferCheckingMode, true);
                        }
                    }
                }
                else if (status == EFEMStatus.Maintain)
                {
                    if (_wcModeinMaintain != WaferSensingMode.Unknwon)
                    {
                        if (_CurWaferCheckingMode != _wcModeinMaintain)
                        {
                            _CurWaferCheckingMode = _wcModeinMaintain;
                            FireCurrentWaferSensingModeChanged(_CurWaferCheckingMode, true);
                        }
                    }

                }
            }
        }

        void OnModuleExceptionEvent(string UniqueName, ExceptionInfo exception, bool set)
        {
            if (string.IsNullOrWhiteSpace(UniqueName) || exception == null)
                return;

            if (set)
                ExQAdd(UniqueName, exception);
            else
                ExQRemove(UniqueName);
        }

        public HRESULT LogActiveAlarm()
        {
            if (_log.LogLevelMode >= LogLevel.DEBUG)
            {
                if (_ExQueue != null)
                {
                    if (_ExQueue.Count > 0)
                    {
                        lock (_lockException)
                        {
                            try
                            {
                                WriteLog(LogHeadType.KeepEyesOn, "Active Alarm Count in MachineControl = " + _ExQueue.Count.ToString(), "LogActiveAlarm()");

                                int counter = 1;
                                foreach (DictionaryEntry de in _ExQueue)
                                {
                                    ExceptionInfo info = (ExceptionInfo)de.Value;
                                    WriteLog(LogHeadType.KeepEyesOn, string.Format("Active Alarm#{0} : {1}", counter, info.ToString()), "LogActiveAlarm()");
                                    counter++;
                                }
                            }
                            catch (Exception ex)
                            {
                                WriteLog(LogHeadType.Exception, "Failed to log active alarm list. Reason: " + ex.Message + ex.StackTrace, "LogActiveAlarm()");
                            }
                        }
                    }
                    else
                    {
                        WriteLog(LogHeadType.Info, "No any active alarm in MachineControl", "LogActiveAlarm()");
                    }
                }
                else
                {
                    WriteLog(LogHeadType.Info, "No any active alarm in MachineControl", "LogActiveAlarm()");
                }
            }

            return null;
        }

        public ArrayList InstantiateObjects()      //Step 1
        {
            Monitor.Enter(_lockInit);
            ArrayList al = new ArrayList();
            try
            {
                _log = LogUtility.GetUniqueInstance();
                WriteLog(LogHeadType.System_NewStart, "");

                _fu = FileUtility.GetUniqueInstance();
                _ExManager = ExceptionManagement.GetUniqueInstance();
                _ExDictionary = _ExManager.GetExceptionDictionary(objectName);
                _ExManager.OnAlarmCleared += new AlarmClearedEvent(_ExManager_OnAlarmCleared);

                _varCenter = EFEMVariableCenter.GetUniqueInstance();
                _varCenter.VariableCenterCallback += new VariableCenterCallbackEventHandler(VariableCenterSink.Instance().VariableCenterCallback);

                bool updateConfig = false;
                int iRobot = -2, iAligner = -2, iDIO0 = -2; //-2: Unknown
                string setting = null;

                #region IgnoreKeylockSwitch Configuration
                setting = _fu.GetPrivateProfileString("EFEMControl", "IgnoreKeylockSwitch");
                if (!string.IsNullOrWhiteSpace(setting) && setting == "HMIEFEM_KEYSWITCH_DISABLE")
                    _ForceIgnoreKeyLockSwitch = true;
                #endregion

                #region DisableProtection Configuration
                MachineControlFactory.DisableProtect4Debug = false;
                setting = _fu.GetPrivateProfileString("EFEMControl", "DisableProtectionForDebug");
                if (!string.IsNullOrWhiteSpace(setting) && setting == "HMIEFEM_PROTECTION_DISABLE")
                    MachineControlFactory.DisableProtect4Debug = true;

                MachineControlFactory.DisableProtect4Run = false;
                setting = _fu.GetPrivateProfileString("EFEMControl", "DisableProtectionForRun");
                if (!string.IsNullOrWhiteSpace(setting) && setting == "HMIEFEM_PROTECTION_DISABLE")
                    MachineControlFactory.DisableProtect4Run = true;
                #endregion

                #region Get Robot Type
                updateConfig = false;
                setting = _fu.GetPrivateProfileString("EFEMControl", "RobotType");
                if (string.IsNullOrWhiteSpace(setting))
                    updateConfig = true;
                else
                {
                    if (!int.TryParse(setting, out iRobot))
                        updateConfig = true;
                    else
                    {
                        updateConfig = false;
                    }
                }
                #endregion

                #region Get Wafer Sensing Mode
                updateConfig = false;
                setting = _fu.GetPrivateProfileString("EFEMControl", "WaferSensingMode_RUN");
                int iSetting = -1;
                if (string.IsNullOrWhiteSpace(setting))
                    updateConfig = true;
                else
                {
                    if (!int.TryParse(setting, out iSetting))
                        updateConfig = true;
                    else
                    {
                        if (iSetting < 0 || iSetting > (int)ExtendMethods.GetMaxValue<WaferSensingMode>())
                        {
                            updateConfig = true;
                        }
                        else
                        {
                            WaferSensingMode tmp = (WaferSensingMode)iSetting;
                            switch (tmp)
                            {
                                case WaferSensingMode.RobotSensor:
                                case WaferSensingMode.ExtendSensor:
                                case WaferSensingMode.BothSensor:
                                    {
                                        updateConfig = false;
                                        break;
                                    }
                                default: //Not allow to disable wafer sensing for Run mode
                                    {
                                        updateConfig = true;
                                        break;
                                    }
                            }
                        }
                    }
                }

                if (updateConfig)
                {
                    iSetting = (int)DefaultWaferSensingMode;
                    _fu.WritePrivateProfileString("EFEMControl", "WaferSensingMode_RUN", iSetting.ToString());
                }

               _wcModeinRun = (WaferSensingMode)iSetting;

                updateConfig = false;
                setting = _fu.GetPrivateProfileString("EFEMControl", "WaferSensingMode_MAINTAIN");
                iSetting = -1;
                if (string.IsNullOrWhiteSpace(setting))
                    updateConfig = true;
                else
                {
                    if (!int.TryParse(setting, out iSetting))
                        updateConfig = true;
                    else
                    {
                        if (iSetting < 0 || iSetting > (int)ExtendMethods.GetMaxValue<WaferSensingMode>())
                            updateConfig = true;
                        else
                            updateConfig = false;
                    }
                }

                if (updateConfig)
                {
                    iSetting = (int)DefaultWaferSensingMode;
                    _fu.WritePrivateProfileString("EFEMControl", "WaferSensingMode_MAINTAIN", iSetting.ToString());
                }

                _wcModeinMaintain = (WaferSensingMode)iSetting;
                #endregion

                WriteLog(LogHeadType.Info, string.Format("WaferSensingMode: Run Mode = {0}, Maintain Mode = {1}", _wcModeinRun.ToString(), _wcModeinMaintain.ToString()));

                #region Get Robot Speed Configuration
                for (int i = 0; i < 4; i++)
                {
                    updateConfig = false;
                    string KeyName = string.Format("RobotSpeed_Level{0}", i + 1);
                    setting = _fu.GetPrivateProfileString("EFEMControl", KeyName);
                    float fRobotSpeed = ConstVC.EFEM.DefaultRobotSpeedForRun[i];
                    if (string.IsNullOrWhiteSpace(setting))
                        updateConfig = true;
                    else
                    {
                        if (!float.TryParse(setting, out fRobotSpeed))
                            updateConfig = true;
                        else
                        {
                            if (fRobotSpeed < 1.0f || fRobotSpeed > 100.0f)
                                updateConfig = true;
                            else
                                _robotSpeedLevel[i] = fRobotSpeed;
                        }
                    }

                    if (updateConfig)
                    {
                        al.Add("Invalid Robot Speed Settings in EFEMConfig.");
                        fRobotSpeed = ConstVC.EFEM.DefaultRobotSpeedForRun[i];
                        _robotSpeedLevel[i] = fRobotSpeed;
                        _fu.WritePrivateProfileString("EFEMControl", KeyName, fRobotSpeed.ToString());
                    }

                    WriteLog(LogHeadType.Info, string.Format("Robot Speed Level{0} = {1}", i + 1, _robotSpeedLevel[i]));
                }
                #endregion

                #region Get Aligner Speed Configuration
                for (int i = 0; i < 4; i++)
                {
                    updateConfig = false;
                    string KeyName = string.Format("AlignerSpeed_Level{0}", i + 1);
                    setting = _fu.GetPrivateProfileString("EFEMControl", KeyName);
                    float fAlignerSpeed = ConstVC.EFEM.DefaultRobotSpeedForRun[i];
                    if (string.IsNullOrWhiteSpace(setting))
                        updateConfig = true;
                    else
                    {
                        if (!float.TryParse(setting, out fAlignerSpeed))
                            updateConfig = true;
                        else
                        {
                            if (fAlignerSpeed < 1.0f || fAlignerSpeed > 100.0f)
                                updateConfig = true;
                            else
                                _alignerSpeedLevel[i] = fAlignerSpeed;
                        }
                    }

                    if (updateConfig)
                    {
                        al.Add("Invalid Aligner Speed Settings in EFEMConfig.");
                        fAlignerSpeed = ConstVC.EFEM.DefaultRobotSpeedForRun[i];
                        _alignerSpeedLevel[i] = fAlignerSpeed;
                        _fu.WritePrivateProfileString("EFEMControl", KeyName, fAlignerSpeed.ToString());
                    }

                    WriteLog(LogHeadType.Info, string.Format("Aligner Speed Level{0} = {1}", i + 1, _robotSpeedLevel[i]));
                }
                #endregion


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
            ArrayList al = new ArrayList();
            try
            {
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
        public ArrayList DownloadParameters()      //Step 3
        {
            Monitor.Enter(_lockInit);
            ArrayList al = new ArrayList();
            try
            {
                string LPsetting = _fu.GetPrivateProfileString("EFEMControl", "LoadPortCount");
                if (string.IsNullOrWhiteSpace(LPsetting))
                {
                    _LPCount = ConstVC.EFEM.DefaultLPCount;
                    ConstVC.EFEM.CurrentLPCount = _LPCount;

                    string errMsg = "Fail to get LoadPortCount configuration!";
                    WriteLog(LogHeadType.Exception, errMsg);
                    al.Add(errMsg);
                }
                else
                {
                    _LPCount = Convert.ToInt32(LPsetting);
                    if (_LPCount < 2 || _LPCount > 3)
                        _LPCount = ConstVC.EFEM.DefaultLPCount;
                }

                #region N2PurgeEquipped

                bool isNeedToWrite = false;
                string N2EquippedSetting = _fu.GetPrivateProfileString("EFEMControl", "N2PurgeEquipped");

                if (string.IsNullOrWhiteSpace(N2EquippedSetting))
                {
                    isNeedToWrite = true;
                }
                else
                {
                    bool bResult = false;
                    if (!Boolean.TryParse(N2EquippedSetting, out bResult))
                    {
                        isNeedToWrite = true;
                    }
                }

                if (isNeedToWrite)
                {
                    //  System equipped with N2 purge. Default: false
                    _fu.WritePrivateProfileString("EFEMControl", "N2PurgeEquipped", Boolean.FalseString);
                }

                #endregion

                ConstVC.EFEM.CurrentLPCount = _LPCount;
                WriteLog(LogHeadType.Info, string.Format("{0} = {1}", "LoadPortCount", _LPCount));

                #region RunMode Interlock
                bool updateVal = false;
                string settingVal = _fu.GetPrivateProfileString("DIOControl", "InterLockInRunMode");
                if (string.IsNullOrWhiteSpace(settingVal))
                    updateVal = true;
                else
                {
                    if (!Boolean.TryParse(settingVal, out _interLockDisabledInRunMode))
                    {
                        _interLockDisabledInRunMode = false;
                        updateVal = true;
                    }
                }

                if (updateVal)
                    _fu.WritePrivateProfileString("DIOControl", "InterLockInRunMode", _interLockDisabledInRunMode.ToString());
                #endregion

                #region Offline interlock
                bool bEnableDIOOfflineInterlock = true;
                settingVal = _fu.GetPrivateProfileString("DIOControl", "InterLockOnline");
                if (string.IsNullOrWhiteSpace(settingVal))
                    updateVal = true;
                else
                {
                    if (!Boolean.TryParse(settingVal, out bEnableDIOOfflineInterlock))
                    {
                        bEnableDIOOfflineInterlock = true;
                        updateVal = true;
                    }
                }

                if (updateVal)
                    _fu.WritePrivateProfileString("DIOControl", "InterLockOnline", bEnableDIOOfflineInterlock.ToString());

                #endregion

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
        public ArrayList Initialize()              //Step 4
        {
            Monitor.Enter(_lockInit);
            ArrayList al = new ArrayList();
            try
            {
                #region Taught Position
                {
                    HRESULT hr = null;
                    bool updateConfig = false;
                    string setting = null;

                    #region Get BackupTaughtPosition setting
                    bool bBackupTaughtPosition = true;
                    updateConfig = false;
                    setting = _fu.GetPrivateProfileString("EFEMControl", "BackupTaughtPosition");
                    if (string.IsNullOrWhiteSpace(setting))
                        updateConfig = true;
                    else
                    {
                        if (!Boolean.TryParse(setting, out bBackupTaughtPosition))
                            updateConfig = true;
                    }

                    if (updateConfig)
                        _fu.WritePrivateProfileString("EFEMControl", "BackupTaughtPosition", bBackupTaughtPosition.ToString());

                    #endregion

                    if (bBackupTaughtPosition)
                    {

                        WriteLog(LogHeadType.CallStart, "", "_TaughtPos.ReadTaughtPosFromController()");

                        ArrayList ret = new ArrayList();
                        if (hr != null)
                        {
                            string errMsg = "Fail to read TaughtPosition from Controller.";
                            WriteLog(LogLevel.ERROR, LogHeadType.Exception, errMsg + " Reason: " + hr._message);

                            ret.Add(errMsg);
                        }

                        WriteLog(LogHeadType.CallStart, "", "_TaughtPos.WriteTaughtPositionToDefaultXml()");

                        if (ret != null && ret.Count > 0)
                        {
                            foreach (object obj in ret)
                                al.Add("[TaughtPos]" + obj.ToString());
                            WriteLog(LogHeadType.CallEnd, "--->Failed.", "_TaughtPos Read/Write operation is failed");
                        }
                        else
                            WriteLog(LogHeadType.CallEnd, "--->Succeeded.", "_TaughtPos Read/Write operation OK");
                    }
                }
                #endregion


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

        public string[] GetLocalIPv4(NetworkInterfaceType _type)
        {
            List<string> output = new List<string>();
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            output.Add(ip.Address.ToString());
                        }
                    }
                }
            }

            if (output.Count > 0)
                return output.ToArray();
            else
                return null;
        }

        public bool IsInRunningMode
        {
            get { return CurrentEFEMStatus == EFEMStatus.Run; }
        }

        public bool IsInMaintainMode
        {
            get { return (CurrentEFEMStatus == EFEMStatus.Maintain || CurrentEFEMStatus == EFEMStatus.Stop || CurrentEFEMStatus == EFEMStatus.Teach); }
        }

        protected bool IsTeachPendantConnected
        {
            get
            {
                    return false;                 
            }
        }

        public bool IsAllowToSwitchOnlineModeViaGUI
        {
            get
            {
                if (IsTeachPendantConnected)
                    return false;
                else
                    return (CurrentEFEMStatus == EFEMStatus.Run) || _ForceIgnoreKeyLockSwitch;
            }
        }

        public bool IsOnlineMode
        {
            get
            {
                if (IsTeachPendantConnected)
                    return false;
                else
                    return (!_isOfflineMode && (CurrentEFEMStatus == EFEMStatus.Run || _ForceIgnoreKeyLockSwitch));
            }
        }

        public WaferSensingMode CurrentWaferSensingMode
        {
            get
            {
                return _CurWaferCheckingMode;
            }
        }

        public EFEMStatus CurrentEFEMStatus
        {
            get 
            {
                if (_efemKeyPosition == EFEMStatus.Stop)
                    return EFEMStatus.Stop;
                
                if (IsTeachPendantConnected)
                    return EFEMStatus.Teach;
                else
                    return _efemKeyPosition;
            }
        }

        protected void FireCurrentWaferSensingModeChanged(WaferSensingMode mode, bool IsAsync)
        {
            if (OnWaferSensingModeChanged != null)
            {
                if (!IsAsync)
                {
                    OnWaferSensingModeChanged(mode);
                }
                else
                {
                    foreach (WaferSensingModeChanged action in OnWaferSensingModeChanged.GetInvocationList())
                    {
                        try
                        {
                            action.BeginInvoke(mode, null, null);
                        }
                        catch (Exception e)
                        {
                            WriteLog(LogHeadType.Exception,
                                string.Format("Exception occurs in FireCurrentWaferSensingModeChanged(). Delegate Source: {0}. Reason: {1}", action.ToString(), e.Message));
                        }
                    }
                }
            }
        }

        private object lockStatus = new object();
        protected void FireCurrentEFEMStatus(bool IsAsync)
        {

            EFEMStatus status = CurrentEFEMStatus;
            bool online = IsOnlineMode;

            if (OnEFEMStatusChanged != null)
            {
                if (!IsAsync)
                {
                    OnEFEMStatusChanged(status, online);
                }
                else
                {
                    foreach (EFEMStatusChanged action in OnEFEMStatusChanged.GetInvocationList())
                    {
                        try
                        {
                            action.BeginInvoke(status, online, null, null);
                        }
                        catch (Exception e)
                        {
                            WriteLog(LogHeadType.Exception,
                                string.Format("Exception occurs in FireCurrentEFEMStatus(). Delegate Source: {0}. Reason: {1}", action.ToString(), e.Message));
                        }
                    }
                }
            }
        }

        protected void FireEFEMStatusUnknown(bool IsAsync)
        {
            if (OnEFEMStatusChanged != null)
            {
                if (!IsAsync)
                {
                    OnEFEMStatusChanged(IsTeachPendantConnected ? EFEMStatus.Teach : EFEMStatus.Unknown, false);
                }
                else
                {
                    foreach (EFEMStatusChanged action in OnEFEMStatusChanged.GetInvocationList())
                    {
                        try
                        {
                            action.BeginInvoke(IsTeachPendantConnected ? EFEMStatus.Teach : EFEMStatus.Unknown, false, null, null);
                        }
                        catch (Exception e)
                        {
                            WriteLog(LogHeadType.Exception,
                                string.Format("Exception occurs in FireEFEMStatusUnknown(). Delegate Source: {0}. Reason: {1}", action.ToString(), e.Message));
                        }
                    }
                }
            }
        }

        protected bool ExQIsExist(string key)
        {
            lock (_lockException)
            {
                object obj = null;
                obj = _ExQueue[key];
                if (obj != null)
                    return true;
                return false;
            }
        }

        protected ExceptionInfo GetExQExistingEx(string key)
        {
            lock (_lockException)
            {
                if (!ExQIsExist(key))
                    return null;
                return _ExDictionary[key];
            }
        }

        void _ExManager_OnAlarmCleared(uint ALID)
        {
            lock (_lockException)
            {
                if (_ExQueue != null)
                {
                    foreach (DictionaryEntry de in _ExQueue)
                    {
                        ExceptionInfo info = (ExceptionInfo)de.Value;
                        if (info.ALID == ALID)
                        {
                            ExQRemove((string)de.Key, true);
                            break;
                        }
                    }
                }
            }
        }

        protected void ExQRemove(string key, bool fromExManager = false)
        {
            lock (_lockException)
            {
                if (ExQIsExist(key))
                {
                    ExceptionInfo info = (ExceptionInfo)_ExQueue[key];

                    if (!fromExManager)
                        _ExManager.AlarmClear(info.ALID);

                    _ExQueue.Remove(key);

                    switch (info.category)
                    {
                        case ExCategory.Warning:
                            {
                                CurrentWarningCount--;
                                break;
                            }
                        case ExCategory.Error:
                            {
                                CurrentErrorCount--;
                                break;
                            }
                        case ExCategory.Alarm:
                            {
                                CurrentAlarmCount--;
                                break;
                            }
                        default:
                            break;
                    }

                    WriteLog(LogHeadType.Exception_Clear, "[ExQRemove] " + info.ALTX, info.ALID.ToString());
                }
                else
                {
                    WriteLog(LogHeadType.Exception_Clear, string.Format("[ExQRemove] No Exception with name = {0} in queue.", key));
                }
            }

            LogActiveAlarm();
        }

        protected void ExQAdd(string key, ExceptionInfo info)
        {
            lock (_lockException)
            {
                if (!ExQIsExist(key))
                {
                    _ExQueue.Add(key, (object)info);
                    _ExManager.AlarmSet(_ExManager.GetTimeStamp(), info.ALID, info.extraMessage);

                    switch (info.category)
                    {
                        case ExCategory.Warning:
                            {
                                CurrentWarningCount++;
                                break;
                            }
                        case ExCategory.Error:
                            {
                                CurrentErrorCount++;
                                break;
                            }
                        case ExCategory.Alarm:
                            {
                                CurrentAlarmCount++;
                                break;
                            }
                        default:
                            break;
                    }

                    if (info.mode == ExMode.Auto)
                        WriteLog(LogHeadType.Exception_Set, "[ExQAdd][Auto] " + info.ALTX, info.ALID.ToString());
                    else
                        WriteLog(LogHeadType.Exception_Set, "[ExQAdd][Manual] " + info.ALTX, info.ALID.ToString());
                }
                else
                    WriteLog(LogHeadType.Exception_Set, "[ExQAdd][Duplicate-Dont add] " + info.ALTX, info.ALID.ToString());
            }

            LogActiveAlarm();
        }
        protected void ExQAdd(string errKey)
        {
            lock (_lockException)
            {
                if (_ExDictionary.Contains(errKey))
                {
                    ExQAdd(errKey, _ExDictionary[errKey]);
                }
                else
                {
                    throw new Exception(string.Format("Error Key - [{0}] can not be generated!", errKey));
                }
            }
        }

        public void OnVariableCenterCallback(string VariableName, object Value)
        {
            WriteLog(LogLevel.DEBUG, LogHeadType.Data, string.Format("Get VC Callback. VarName = {0}, Value = {1}", VariableName, ExtendMethods.ToStringHelper(Value)));

            if (VariableName == "[ST]DIO0Online")
            {
                _isDIO0OnLine = (bool)Value;

                if (!_ForceIgnoreKeyLockSwitch)
                {
                    //DIO online
                    if ((bool)Value)
                        FireCurrentEFEMStatus(true);
                    else //DIO offline (disconnect)
                    {
                        if (IsTeachPendantConnected)
                            FireCurrentEFEMStatus(true);
                        else
                            FireEFEMStatusUnknown(true);
                    }
                }
            }
        }

        #region Status
        public HRESULT SwitchOnlineMode(bool isOnline)
        {
            lock (_lockOnline)
            {
                if (isOnline)
                {
                    object status = _varCenter.GetValue("[ST]HostConnected");
                    if (_varCenter.IsNotExistVariableReturn(status) || (bool)status == false)
                    {
                        if (!IsProtectionDisabled4Debug)
                            return MakeException("SWITCH_ONLINE_HOSTFAIL");
                    }

                    //check connection
                    if (!IsProtectionDisabled4Debug)
                    {
                        status = _varCenter.GetValue("[ST]RobotConnected");
                        if (_varCenter.IsNotExistVariableReturn(status) || (bool)status == false)
                            return MakeException("SWITCH_ONLINE_ROBOTFAIL");

                        status = _varCenter.GetValue("[ST]DIO0Connected");
                        if (_varCenter.IsNotExistVariableReturn(status) || (bool)status == false)
                            return MakeException("SWITCH_ONLINE_IOFAIL");

                        status = _varCenter.GetValue("[ST]FFUConnected");
                        if (_varCenter.IsNotExistVariableReturn(status) || (bool)status == false)
                            return MakeException("SWITCH_ONLINE_FFUFAIL");
                    }
                }

                ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_SetToOnlineMode), (object)isOnline);

                return null;
            }
        }

        protected void TPool_SetToOnlineMode(object isOnline)
        {
            if (_efemKeyPosition == EFEMStatus.Unknown && _ForceIgnoreKeyLockSwitch)
                _efemKeyPosition = EFEMStatus.Run;

            int waitCounter = 0;
            while (IsEFEMBusy && waitCounter < 100)
            {
                waitCounter++;
                Thread.Sleep(100);
            }

            if (waitCounter >= 100) //timeout
            {
                MakeException("SWITCH_ONLINE_TIMEOUT");
                _varCenter.SetValueAndFireCallback("[TimeOut]OnlineModeSwithProcedure", true);
            }
            else
            {
                _varCenter.SetValueAndFireCallback("[TimeOut]OnlineModeSwithProcedure", false);
                _isOfflineMode = !(bool)isOnline;
                FireCurrentEFEMStatus(true);
            }
        }

        public bool IsHostCommandAllow
        {
            get { return IsOnlineMode; }
        }

        public bool IsGUICommandAllow
        {
            get
            {
                if (IsTeachPendantConnected)
                    return false;
                else
                    return !IsOnlineMode;
            }
        }

        public bool IsAnyAlarmExist 
        {
            get
            {
                return (CurrentAlarmCount > 0);
            }
        }

        public bool IsAnyErrorExist
        {
            get
            {
                return (CurrentErrorCount > 0);
            }
        }

        public bool IsAnyWarningExist
        {
            get
            {
                return (CurrentWarningCount > 0);
            }
        }

        public bool IsMotionCommandAllow
        {
            get
            {
                if (IsProtectionDisabled4Debug)
                    return true;
                else
                    return IsAnyAlarmExist;
            }
        }

        public bool IsDoorOpen
        {
            get
            {
                return false;
            }
        }

        public int LoadPortCount
        {
            get
            {
                return _LPCount;
            }
        }

        public bool IsEFEMBusy
        {
            get 
            {
                bool robotBsy = false;
                bool alinBsy = false;

                return (_IsBusy || robotBsy || alinBsy); 
            }
        }

        public bool IsInEMSStatus
        {
            get { return false; }
        }

        public bool IsDIO0Online
        {
            get { return _isDIO0OnLine; }
        }

        //Tree section. Each section can only contain one letter
        //Host must get Version after AbstractRobot.UpdateStatus() has been called
        public string GetEFEMVersion
        {
            get 
            {
                //Host got the version already, not allow to change sensing mode
                _isChgRunModeSensorAllow = false;

                int baseValue = 0x41; //"A"

                string version = string.Format("{0}.{1}.{2}", MainVersion, SubVersion + _LPCount, (char)baseValue);

                return version; 
            } 
        }

        //This code is prepared for Andrew to use in Host
        public bool DecodeVersion(string Version, ref int lpCount, ref bool isDualHand, ref bool isGrip, ref bool isUseRobotSensor, ref bool isUseExtendSensor)
        {
            if (Version.Length < 5)
                return false;

            string[] subVersion = Version.Split('.');
            if (subVersion.Length != 3)
                return false;

            char cInfo = subVersion[2][0];
            if (cInfo < 0x41 || cInfo > 0x41 + 26)
                return false;

            int EFEMInfo = cInfo - 0x41;
            string bin = Convert.ToString(EFEMInfo, 2).PadLeft(4, '0');
            isDualHand = bin[3] == '1';
            isGrip = bin[2] == '1';
            isUseRobotSensor = bin[1] == '1';
            isUseExtendSensor = bin[0] == '1';

            char clpInfo = subVersion[1][0];
            if (clpInfo < '0' || clpInfo > '9')
                return false;
            else
                lpCount = clpInfo - '0';

            return true;
        }

        public HRESULT GetStatus(ref EFEMWaferStatus WaferOnArm1, ref EFEMWaferStatus WaferOnArm2, ref EFEMWaferStatus WaferOnAligner,
                            ref EFEMDeviceStatus RobotStatus, ref EFEMDeviceStatus AlignerStatus,
                            ref bool IsRobotError, ref bool IsAlignerError,
                            ref bool IsSysVacuumOn, ref bool IsSysAirOn, ref bool IsNeedHome, bool reGet)
        {
            IsSysVacuumOn = false;
            IsSysAirOn = false;

            IsNeedHome = _isRobotNeedHome;
            return null;
        }
        #endregion

        #region Action

        public HRESULT CleanManualAlarm()
        {
            WriteLog(LogHeadType.MethodEnter, "CleanManualAlarm()");

            if (_ExQueue != null)
            {
                if (_ExQueue.Count > 0)
                {
                    List<string> ManualAlarmList = new List<string>();
                    lock (_lockException)
                    {
                        foreach (DictionaryEntry item in _ExQueue)
                        {
                            try
                            {
                                ExceptionInfo info = (ExceptionInfo)item.Value;
                                if (info.mode == ExMode.Manual)
                                {
                                    ManualAlarmList.Add(item.Key.ToString());

                                }
                            }
                            catch (Exception e)
                            {
                                WriteLog(e, LogLevel.ERROR);
                            }
                        }

                        foreach (string AlarmKey in ManualAlarmList)
                        {
                            try
                            {
                                WriteLog(LogHeadType.Info, "Clear Manual Alarm Key - " + AlarmKey);
                                ExQRemove(AlarmKey);
                            }
                            catch (Exception e)
                            {
                                WriteLog(e, LogLevel.ERROR);
                            }
                        }

                    }
                }
            }

            WriteLog(LogHeadType.MethodExit, "CleanManualAlarm()");
            return null;
        }

        public HRESULT Init() //Init & init2
        {
            HRESULT hr = null;

            hr = CleanManualAlarm();
            if (hr != null)
                return hr;

            FireCurrentWaferSensingModeChanged(_CurWaferCheckingMode, true);

            return hr;
        }

        public bool IsWaferSensingModeChangedByHost 
        {
            set
            {
                _isHostChgWaferSensingMode = value;
            }
            get
            {
                return _isHostChgWaferSensingMode;
            }
        }

        public HRESULT GetWaferSensingMode(bool isRunMode, ref WaferSensingMode mode)
        {
            if (isRunMode)
            {
                mode = _wcModeinRun;
            }
            else
            {
                mode = _wcModeinMaintain;
            }

            return null;
        }

        public WaferSensingMode DefaultWaferSensingMode
        {
            get { return WaferSensingMode.BothSensor; }
        }

        public HRESULT GetExtWfrSensingStatus(bool ForceReget, ref EFEMWaferStatus aligner, ref EFEMWaferStatus lp1, ref EFEMWaferStatus lp2, ref EFEMWaferStatus lp3)
        {
            bool bAlign = false;
            bool bLP1 = false;
            bool bLP2 = false;
            bool bLP3 = false;

            aligner = bAlign ? EFEMWaferStatus.Presence : EFEMWaferStatus.Absence;
            lp1 = _LPCount >= 1 ? (bLP1 ? EFEMWaferStatus.Presence : EFEMWaferStatus.Absence) : EFEMWaferStatus.Error;
            lp2 = _LPCount >= 2 ? (bLP2 ? EFEMWaferStatus.Presence : EFEMWaferStatus.Absence) : EFEMWaferStatus.Error;
            lp3 = _LPCount >= 3 ? (bLP3 ? EFEMWaferStatus.Presence : EFEMWaferStatus.Absence) : EFEMWaferStatus.Error;

            return null;
        }

        #endregion

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
                    WriteLog(LogHeadType.Info, "Disposing all objects...");

                    if (_varCenter != null)
                    _varCenter.VariableCenterCallback -= new VariableCenterCallbackEventHandler(VariableCenterSink.Instance().VariableCenterCallback);

                    if (_ExManager != null)
                        _ExManager.OnAlarmCleared -= new AlarmClearedEvent(_ExManager_OnAlarmCleared);
                }
            }
            disposed = true;
        }

        #region WriteLog Methods
        internal bool WriteLog(LogHeadType enLogType, string szLogMessage, string szRemark = null)
        {
            if (_log != null)
            {
                return WriteLog(LogHelper.GetDefaultLogLevel(enLogType), enLogType, szLogMessage, szRemark);
            }
            else
                return false;
        }
        internal bool WriteLog(LogLevel level, LogHeadType enLogType, string szLogMessage, string szRemark = null)
        {
            if (_log != null)
                return _log.WriteLog(objectName, level, enLogType, szLogMessage, szRemark);
            else
                return false;
        }
        internal bool WriteLog(HRESULT hr, string track = null, LogLevel level = LogLevel.DEBUG)
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
        internal bool WriteLog(Exception hr, LogLevel level = LogLevel.DEBUG)
        {
            if (_log != null)
            {
                return _log.WriteLog(objectName, level, LogHeadType.Exception, hr.Message + hr.StackTrace);
            }
            else
                return false;
        }
        #endregion

        #region Exception Helper
        internal HRESULT MakeException(string errKey, string extraMessage = "", bool writrLog = true)
        {
            if (string.IsNullOrWhiteSpace(errKey))
                return null;

            StackTrace stackTrace = new StackTrace();
            string callerMethodName = stackTrace.GetFrame(1).GetMethod().Name;

            if (_ExDictionary == null)
            {
                return null;
            }
            else
            {
                if (_ExDictionary.Contains(errKey))
                {
                    HRESULT hr = _ExDictionary[errKey].hRESULT;
                    hr._extramessage = callerMethodName;
                    if (!string.IsNullOrWhiteSpace(extraMessage))
                        hr._extramessage += " : " + extraMessage;

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
        internal HRESULT MakeException(Exception ex, string extraMessage = "", bool writrLog = true)
        {
            if (ex == null)
                return null;

            HRESULT hr = MakeException("SOFTWARE_EXCEPTION", extraMessage, false);
            hr._extramessage = ex.Message;

            if (writrLog)
                WriteLog(hr, ex.StackTrace);

            return hr;
        }
        #endregion
    }

    [HMIAuthor("Lawrence Ouyang", version = 1.0, Remark = "2015/09/22: To prevent HostProxy and GUIBasic to perform motion command in improper mode.")]
    public class MachineControlFactory
    {
        private static bool _disableProtect4Debug = false;   //Default: false
        private static bool _disableProtect4Run = false;     //Default: false
        private static bool _disableProtect4Maintain = true; //Default: true
        private static AbstractMachineControl uniqueInstance4Host = null;
        private static AbstractMachineControl uniqueInstance4GUI = null;

        public static AbstractMachineControl GetUniqueInstance(MCOwner owner)
        {
            switch (owner)
            {
                case MCOwner.HostProxy:
                    {
                        if (uniqueInstance4Host == null)
                            uniqueInstance4Host = new MachineControlImplementWrap(owner);

                        return uniqueInstance4Host;
                    }
                case MCOwner.GUIBasic:
                    {
                        if (uniqueInstance4GUI == null)
                            uniqueInstance4GUI = new MachineControlImplementWrap(owner);

                        return uniqueInstance4GUI;
                    }
                default:
                    return null;
            }
        }

        public static bool DisableProtect4Debug
        {
            get { return _disableProtect4Debug; }
            set { _disableProtect4Debug = value; } 
        }

        public static bool DisableProtect4Run
        {
            get { return _disableProtect4Run; }
            set { _disableProtect4Run = value; }
        }

        public static bool DisableProtect4Maintain
        {
            get { return _disableProtect4Maintain; }
            set { _disableProtect4Maintain = value; }
        }
    }

    [HMIAuthor("Lawrence Ouyang", version = 1.0, Remark = "2015/09/22: To prevent HostProxy and GUIBasic to perform motion command in improper mode.")]
    public class MachineControlImplementWrap : AbstractMachineControl
    {
        public event EFEMStatusChanged OnEFEMStatusChanged;
        public event WaferSensingModeChanged OnWaferSensingModeChanged;

        private MCOwner _owner = MCOwner.HostProxy;
        private string _sOwner = MCOwner.HostProxy.ToString();
        protected static AbstractMachineControl main = null;

        internal MachineControlImplementWrap(MCOwner owner)
        {
            _owner = owner;
            _sOwner = owner.ToString();

            if (main == null)
                main = MachineControlImplement.GetUniqueInstance();

            main.OnEFEMStatusChanged += new EFEMStatusChanged(MCWrap_OnEFEMStatusChanged);
            main.OnWaferSensingModeChanged += new WaferSensingModeChanged(MCWrap_OnWaferSensingModeChanged);
        }

        #region Debug
        //For Debug
        public bool IsForceIgnoreKeyLockSwitch { get { return main.IsForceIgnoreKeyLockSwitch; } }
        public bool IsProtectionDisabled4Debug { get { return main.IsProtectionDisabled4Debug; } }
        public bool IsProtectionDisabled4Run   { get { return main.IsProtectionDisabled4Run; } }
        public bool IsProtectionDisabled4Maintain { get { return main.IsProtectionDisabled4Maintain; } }
        #endregion

        [Flags]
        private enum CommandAllowOwnder
        {
            Host = 1,  //Action is only allow for Host
            GUI = 2,   //Action is only allow for GUI
            Both = Host | GUI,      //Action is allow for both of Host and GUI
        }

        private HRESULT TestAllow()
        {
            HRESULT hr = TestOwnerAllow();
            if (hr != null)
                return hr;

            hr = TestActionAllow();
            return hr;
        }

        private HRESULT TestOwnerAllow(CommandAllowOwnder comOwnder = CommandAllowOwnder.Both)
        {
            if (MachineControlFactory.DisableProtect4Debug)
                return null;
            else
            {
                if (this._owner == MCOwner.HostProxy)
                {
                    if (comOwnder.HasFlag(CommandAllowOwnder.Host) && IsHostCommandAllow)
                        return null;
                    else
                        return MakeException("NOTALLOW_HOST");
                }
                else
                {
                    if (comOwnder.HasFlag(CommandAllowOwnder.GUI) && IsGUICommandAllow)
                        return null;
                    else
                        return MakeException("NOTALLOW_GUI");
                }
            }
        }

        private HRESULT TestActionAllow()
        {
            //Protect only Alarm level exception. Still allow command if Warning or Error level exception exist.
            if (main.IsAnyAlarmExist)
            {
                switch (CurrentEFEMStatus)
                {
                    case EFEMStatus.Run:
                        {
                            if (main.IsProtectionDisabled4Run)
                                return null;
                            else
                            {
                                //LogActiveAlarm();
                                return MakeException("MOTION_REJECT_ALARMEXIST");
                            }
                        }
                    case EFEMStatus.KeyError:
                    case EFEMStatus.Unknown:
                        {
                            return MakeException("MOTION_REJECT_ALARMEXIST");
                        }
                    case EFEMStatus.Maintain:
                        {
                            if (main.IsProtectionDisabled4Maintain)
                                return null;
                            else
                            {
                                //LogActiveAlarm();
                                return MakeException("MOTION_REJECT_ALARMEXIST");
                            }
                        }
                    case EFEMStatus.Stop:
                    case EFEMStatus.Teach:
                    default:
                        {
                            return MakeException("MOTION_REJECT_STOP");
                        }
                }
            }
            else
                return null;
        }

        void MCWrap_OnWaferSensingModeChanged(WaferSensingMode mode)
        {
            if (OnWaferSensingModeChanged != null)
                OnWaferSensingModeChanged(mode);
        }

        void MCWrap_OnEFEMStatusChanged(EFEMStatus status, bool isOnline)
        {
            if (OnEFEMStatusChanged != null)
                OnEFEMStatusChanged(status, isOnline);
        }

        public bool IsHostCommandAllow
        {
            get { return main.IsHostCommandAllow; }
        }

        public bool IsGUICommandAllow
        {
            get { return main.IsGUICommandAllow; }
        }

        public bool IsAnyAlarmExist
        {
            get { return main.IsAnyAlarmExist; }
        }

        public bool IsAnyErrorExist
        {
            get { return main.IsAnyErrorExist; }
        }

        public bool IsAnyWarningExist
        {
            get { return main.IsAnyWarningExist; }
        }

        public bool IsMotionCommandAllow
        {
            get { return main.IsMotionCommandAllow; }
        }

        public bool IsInRunningMode
        {
            get { return main.IsInRunningMode; }
        }

        public bool IsInMaintainMode
        {
            get { return main.IsInMaintainMode; }
        }

        public bool IsAllowToSwitchOnlineModeViaGUI
        {
            get { return main.IsAllowToSwitchOnlineModeViaGUI; }
        }

        public bool IsOnlineMode
        {
            get { return main.IsOnlineMode; }
        }

        public bool IsDoorOpen
        {
            get { return main.IsDoorOpen; }
        }

        public int LoadPortCount
        {
            get { return main.LoadPortCount; }
        }

        public EFEMStatus CurrentEFEMStatus
        {
            get { return main.CurrentEFEMStatus; }
        }

        public WaferSensingMode CurrentWaferSensingMode
        {
            get { return main.CurrentWaferSensingMode; }
        }

        public bool IsEFEMBusy
        {
            get { return main.IsEFEMBusy; }
        }

        public bool IsInEMSStatus
        {
            get { return main.IsInEMSStatus; }
        }

        public string GetEFEMVersion
        {
            get { return main.GetEFEMVersion; }
        }

        public HRESULT SwitchOnlineMode(bool IsOnline)
        {
            WriteMethodStart("SwitchOnlineMode()", string.Format("IsOnline = {0}", IsOnline));
            HRESULT hr = main.SwitchOnlineMode(IsOnline);
            WriteMethodEnd("SwitchOnlineMode()", hr);
            return hr;
        }

        public HRESULT CleanManualAlarm()
        {
            WriteMethodStart("CleanManualAlarm()");

            HRESULT hr = main.CleanManualAlarm();

            WriteMethodEnd("CleanManualAlarm()", hr);
            return hr;
        }

        public bool IsWaferSensingModeChangedByHost
        {
            internal set
            {
                ((MachineControlImplement)main).IsWaferSensingModeChangedByHost = value;
            }
            get
            {
                return main.IsWaferSensingModeChangedByHost;
            }
        }

        public void OnVariableCenterCallback(string VariableName, object Value)
        {
            main.OnVariableCenterCallback(VariableName, Value);
        }

        public HRESULT LogActiveAlarm()
        {
            return main.LogActiveAlarm();
        }

        #region Log Function
        private bool WriteLog(LogHeadType enLogType, string szLogMessage, string szRemark = null)
        {
            if (main != null)
            {
                return WriteLog(LogHelper.GetDefaultLogLevel(enLogType), enLogType, szLogMessage, szRemark);
            }
            else
                return false;
        }
        private bool WriteLog(LogLevel level, LogHeadType enLogType, string szLogMessage, string szRemark = null)
        {
            if (main != null)
                return ((MachineControlImplement)main).WriteLog(level, enLogType, szLogMessage, szRemark);
            else
                return false;
        }
        private bool WriteLog(HRESULT hr, string track = null, LogLevel level = LogLevel.DEBUG)
        {
            if (main != null)
                return ((MachineControlImplement)main).WriteLog(hr, track, level);
            else
                return false;
        }
        private bool WriteLog(Exception hr, LogLevel level = LogLevel.DEBUG)
        {
            if (main != null)
                return ((MachineControlImplement)main).WriteLog(hr, level);
            else
                return false;
        }

        private bool WriteMethodStart(string MethodName, string message = null)
        {
            return WriteLog(LogLevel.INFO, LogHeadType.MethodEnter,
                string.Format("[Caller = {0}] {1}", _sOwner, message), MethodName);
        }

        private bool WriteMethodEnd(string MethodName, bool isSuccess, string errMessage)
        {
            if (isSuccess)
                return WriteLog(LogLevel.INFO, LogHeadType.MethodExit, "Success", MethodName);
            else
                return WriteLog(LogLevel.INFO, LogHeadType.MethodExit, "Fail! Reason: " + errMessage, MethodName);
        }

        private bool WriteMethodEnd(string MethodName, HRESULT result)
        {
            if (result == null)
                return WriteLog(LogLevel.INFO, LogHeadType.MethodExit, "Success", MethodName);
            else
                return WriteLog(LogLevel.INFO, LogHeadType.MethodExit, "Fail! Reason: " + result._message, MethodName);            
        }

        private HRESULT MakeException(string errKey, string extraMessage = "", bool writrLog = false)
        {
            return ((MachineControlImplement)main).MakeException(errKey, extraMessage, writrLog);
        }  
        #endregion
    }

    public class VariableCenterSink : AbstractVariableCenter_Sink
    {
        private static VariableCenterSink _instance = null;
        private AbstractMachineControl _mc = null;

        private VariableCenterSink()
        {
            _mc = MachineControlImplement.GetUniqueInstance();
        }

        public static VariableCenterSink Instance()
        {
            if (_instance == null)
                _instance = new VariableCenterSink();

            return _instance;
        }

        override protected void VariableCenterCallback_Sink(string VariableName, object Value)
        {
            try
            {
                _mc.OnVariableCenterCallback(VariableName, Value);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
