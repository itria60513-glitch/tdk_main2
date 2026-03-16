using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using EFEM.DataCenter;
using EFEM.ExceptionManagements;
using EFEM.FileUtilities;
using EFEM.LogUtilities;
using EFEM.VariableCenter;

namespace EFEM.GUIControls
{
    public class GUIBasic
    {
        private static GUIBasic _instance = null;
        private static string _objectName = ConstVC.ObjectName.GUI;        

        private AbstractFileUtilities _fu = null;
        private AbstractExceptionManagement _ex = null;
        private AbstractLogUtility _log = null;
        private ExceptionDictionary _ExDictionary = null;
        private EFEMVariableCenter _varCenter = null;
        private EFEMGUIBase _base = null;
        private UserLoginForm _login = null;

        private ToolTip m_toolTip = new ToolTip();

        public delegate void ExceptionNotifierSink(ExceptionInfo info);
        public delegate void InvalidateNotifierSink();
        public delegate void PreAlignAngleUpdatedDel(object value);
        public delegate void AdvanceModeDel(bool isAdvanceMode);

        public event VariableCenterCallbackEventHandler VariableCenterCallback = null;
        public event PreAlignAngleUpdatedDel OnPreAlignAngleUpdated = null;

        public event ExceptionNotifierSink OnExceptionAddRequired = null;
        public event ExceptionNotifierSink OnExceptionRemoveRequired = null;

        public event InvalidateNotifierSink OnInvalidateNotifierRequired = null;
        public event AlarmReportSendGUIEventHandler OnAlarmReportChanged = null;
        public event UserLoginForm.delLoginTypeChanged OnLoginTypeChanged = null;

        public delegate void delRestartGUISinck();
        public event delRestartGUISinck RestartGUISink = null;

        private GUIBasic()
        {
            //string ErrMsg = CheckAllEssentialFiles();
            //if (!string.IsNullOrWhiteSpace(ErrMsg))
            //{
            //    MessageBox.Show("Cannot execute EFEM GUI case of missing following essential file(s): \n\n" + ErrMsg, "EFEM GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    System.Environment.Exit(-1);
            //}

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length >= 2)
            {
                for (int i = 1; i < args.Length; i++)
                {
                    if (args[i].ToLower() == "-offline")
                        ConstVC.Debug.KeepOfflineAfterStartup = true;
                    else if (args[i].ToLower() == "-debug")
                    {
                        ConstVC.Debug.ShowRDGUI = true;
                        ConstVC.Debug.IsDebugCommandLine = true;
                        ConstVC.Debug.InCommDebugMode = true;
                        ConstVC.Debug.ShowCtrlCommStatus = true;
                        ConstVC.Debug.ShowHostCommStatus = true;
                        ConstVC.Debug.ShowFFUCommStatus = true;
                    }
                }
            }

            _log = LogUtilities.LogUtility.GetUniqueInstance();
            WriteLog(LogLevel.INFO, LogHeadType.System_NewStart, "");
            _fu = EFEM.FileUtilities.FileUtility.GetUniqueInstance();
            _ex = EFEM.ExceptionManagements.ExceptionManagement.GetUniqueInstance();
            _ExDictionary = _ex.GetExceptionDictionary(_objectName);
            _varCenter = EFEMVariableCenter.GetUniqueInstance();
            _varCenter.VariableCenterCallback += VarCenter_VariableCenterCallback;

            _fu.OnWriteUserLogRequired += new WriteUserLogDel(_fu_OnWriteUserLogRequired);

            if (_ex != null)
            {
                _ex.AlarmReportSendGUI += new AlarmReportSendGUIEventHandler(_ex_AlarmReportSendGUI);
            }
        }

        public static string CheckAllEssentialFiles()
        {
            string errMsg = "";

            //step1: EFEMConfig
            if (!File.Exists(ConstVC.FilePath.ConfigFolder + ConstVC.FilePath.EFEMConfig))
                errMsg += (ConstVC.FilePath.EFEMConfig + "\n");

            //step2: EFEMExceptionInfo
            if (!File.Exists(ConstVC.FilePath.ConfigFolder + ConstVC.FilePath.EFEMExceptionInfo))
                errMsg += (ConstVC.FilePath.EFEMExceptionInfo + "\n");

            //step3:EFEMObjectInfo
            if (!File.Exists(ConstVC.FilePath.ConfigFolder + ConstVC.FilePath.EFEMObjectInfo))
                errMsg += (ConstVC.FilePath.EFEMObjectInfo + "\n");

            //step4: EFEMVarCenter
            if (!File.Exists(ConstVC.FilePath.ConfigFolder + ConstVC.FilePath.EFEMVariableCenter))
                errMsg += (ConstVC.FilePath.EFEMVariableCenter + "\n");

            //step5: LogConfig
            if (!File.Exists(ConstVC.LogConfig.LogXMLFileFullName))
                errMsg += (ConstVC.LogConfig.LogXMLFileName + "\n");

            //step6: LogObjectInfo
            if (!File.Exists(ConstVC.LogConfig.LogObjectXMLFileFullName))
                errMsg += (ConstVC.LogConfig.LogObjectXMLFileName + "\n");

            //step 7: EFEMAlarmHistory (will be created automatically)
            //if (!File.Exists(ConstVC.FilePath.ConfigFolder + ConstVC.FilePath.EFEMAlarmHistory))
            //    errMsg += (ConstVC.FilePath.EFEMAlarmHistory + "\n");

            //step8: RS232
            if (!File.Exists(ConstVC.FilePath.ConfigFolder + ConstVC.FilePath.RS232))
                errMsg += (ConstVC.FilePath.RS232 + "\n");

            //step9: TCPIP
            if (!File.Exists(ConstVC.FilePath.ConfigFolder + ConstVC.FilePath.TCPIP))
                errMsg += (ConstVC.FilePath.TCPIP + "\n");

            return errMsg;
        }

        void _ex_AlarmReportSendGUI(byte ALCD, ExceptionInfo ExInfo, string TIMESTAMP, bool ifPopupMessageBox)
        {
            if (OnAlarmReportChanged != null)
                OnAlarmReportChanged(ALCD, ExInfo, TIMESTAMP, ifPopupMessageBox);

            if (ALCD == (byte)0x80)
            {
                if (OnExceptionAddRequired != null)
                    OnExceptionAddRequired(ExInfo);
            }
            else if (ALCD == (byte)0x00)
            {
                if (OnExceptionRemoveRequired != null)
                    OnExceptionRemoveRequired(ExInfo);
            }
        }

        public static GUIBasic Instance()
        {
            if (_instance == null)
                _instance = new GUIBasic();

            return _instance;
        }

        public void RestartGUI()
        {
            WriteLog(LogHeadType.Info, "Restarting GUI");

            if (RestartGUISink != null)
                RestartGUISink();
        }

        private bool isDisposing = false;

        public void SetDisposingState()
        {
            isDisposing = true;
        }

        public bool IsDisposing
        {
            get { return isDisposing; }
        }

        public void DisposeAllBasicObjects()
        {
            isDisposing = true;

            WriteLog(LogHeadType.Info, "Disposing...");

            Thread.Sleep(10);

            if (_fu != null)
            {
                FileUtility.FlushEFEMConfig();
                FileUtility.CloseAllFile();
                _fu = null;
            }

            Thread.Sleep(10);

            if (_varCenter != null)
            {
                _varCenter.VariableCenterCallback -= VarCenter_VariableCenterCallback;
                _varCenter.Dispose();
                _varCenter = null;
            }

            Thread.Sleep(10);

            if (_ex != null)
            {
                _ex.Dispose();
                _ex = null;
            }

            if (_log != null)
            {
                Log.ForceWrite();
                Log.Dispose();
                _log = null;
            }
        }

        public EFEMGUIBase GUIBase
        {
            get { return _base; }
            set
            {
                if (value != null)
                {
                    _base = value;
                }
            }
        }

        public EFEMVariableCenter VariableCenter
        {
            get { return _varCenter; }
        }

        public AbstractFileUtilities FileUtility
        {
            get { return _fu; }
        }

        public AbstractExceptionManagement ExceptionManagement
        {
            get { return _ex; }
        }

        public AbstractLogUtility Log
        {
            get { return _log; }
        }

        public ExceptionDictionary ExDictionary
        {
            get { return _ExDictionary; }
        }

        public void InvalidateNotifier()
        {
            if (OnInvalidateNotifierRequired != null)
            {
                OnInvalidateNotifierRequired();
            }
        }

        private void CheckLogInForm()
        {
            if (_login == null)
            {
                _login = UserLoginForm.GetInstance();
                _login.OnLoginTypeChanged += new UserLoginForm.delLoginTypeChanged(_login_OnLoginTypeChanged);

                if (_base != null)
                {
                    _base.SetLoginInfo(_login.UserLoginType, _login.UserLoginId);
                }
            }
        }

        public void SetFormAsTopMost(Form form)
        {
            if (form != null)
            {
                if (form.InvokeRequired)
                {
                    MethodInvoker del = delegate { SetFormAsTopMost(form); };
                    form.Invoke(del);
                }
                else
                {
                    GUIBasic.SetWindowPos(form.Handle, GUIBasic.HWND_TOPMOST, 0, 0, 0, 0, GUIBasic.TOPMOST_FLAGS);
                }
            }
        }

        void _login_OnLoginTypeChanged(LoginType type)
        {
            CheckLockForMaintain();

            ConstVC.EFEM.CurrentLoginUserType = type.ToString();

            if (_base != null)
            {
                _base.SetLoginInfo(_login.UserLoginType, _login.UserLoginId);
            }

            if (OnLoginTypeChanged != null)
            {
                foreach (UserLoginForm.delLoginTypeChanged action in OnLoginTypeChanged.GetInvocationList())
                {
                    try
                    {
                        action.BeginInvoke(type, null, null);
                    }
                    catch (Exception e)
                    {
                        WriteLog(LogHeadType.Exception,
                            string.Format("Exception occurs in OnLoginTypeChanged(). Delegate Source: {0}. Reason: {1}", action.ToString(), e.Message));
                    }
                }
            }
        }

        private void CheckLockForMaintain()
        {
            if (isDisposing)
                return;

            CheckLogInForm();
        }

        public bool IsMaintainFunctionAllow
        {
            get
            {
                return false;
            }
        }

        public bool IsServiceFunctionAllow
        {
            get
            {
                LoginType type = CurrentLoginType;

                if (type == LoginType.SWRD || type == LoginType.Service)
                    return true;
                else
                    return false;
            }
        }

        public LoginType CurrentLoginType
        {
            get
            {
                CheckLogInForm();
                return _login.CurrentLoginType;
            }
        }

        public void LogIn(bool async = true)
        {
            if (async)
                ThreadPool.QueueUserWorkItem(new WaitCallback(InnerLogin), null);
            else
                InnerLogin(null);
        }

        private void InnerLogin(object syncobj)
        {
            CheckLogInForm();
            ShowDialogOnTop(_login);
            //_login.ShowDialog();

            if (_base != null)
            {
                _base.SetLoginInfo(_login.UserLoginType, _login.UserLoginId);
            }
        }

        public void LogOut(bool async = true)
        {
            if (async)
                ThreadPool.QueueUserWorkItem(new WaitCallback(InnerLogOut), null);
            else
                InnerLogOut(null);
        }

        private void InnerLogOut(object syncobj)
        {
            CheckLogInForm();
            _login.Logout(_login.UserLoginId);

            if (_base != null)
            {
                _base.SetLoginInfo(_login.UserLoginType, _login.UserLoginId);
            }

            CheckLockForMaintain();
        }

        //By user: false
        //By System: true
        public void LockGUI(bool isForMaintain = false)
        {
            CheckLogInForm();

            if (isForMaintain)
                _login.lock4Maintain = true;
            else
                _login.SetUserLockMode(true);

            LogIn();
        }

        void _fu_OnWriteUserLogRequired(string message)
        {
            WirteUserActionLog(LogHeadType.Info, message, "UserDataCenter");
        }

        private void VarCenter_VariableCenterCallback(string variableName, object value)
        {
            VariableCenterCallbackEventHandler handler = VariableCenterCallback;
            if (handler != null)
            {
                handler(variableName, value);
            }
        }

        public bool WriteUserMemo(string szLogMessage)
        {
            return WirteUserActionLog(LogHeadType.None, "[USER MEMO] " + szLogMessage, null); 
        }

        public bool WirteUserActionLog(LogHeadType enLogType, string szLogMessage, string szRemark = null)
        {
            if (_log != null)
                return _log.WriteLog(ConstVC.ObjectName.UserAction, LogLevel.ALL, enLogType, szLogMessage, szRemark);
            else
                return false;
        }

        #region WriteLog Methods for ScriptRun
        public bool WriteScriptLog(LogHeadType enLogType, string szLogMessage, string szRemark = null)
        {
            if (_log != null)
            {
                return WriteScriptLog(LogHelper.GetDefaultLogLevel(enLogType), enLogType, szLogMessage, szRemark);
            }
            else
                return false;
        }
        public bool WriteScriptLog(LogLevel level, LogHeadType enLogType, string szLogMessage, string szRemark = null)
        {
            if (_log != null)
                return _log.WriteLog(ConstVC.ObjectName.ScriptRun, level, enLogType, szLogMessage, szRemark);
            else
                return false;
        }
        public bool WriteScriptLog(Exception hr, LogLevel level = LogLevel.DEBUG)
        {
            if (_log != null)
            {
                return _log.WriteLog(ConstVC.ObjectName.ScriptRun, level, LogHeadType.Exception, hr.Message + hr.StackTrace);
            }
            else
                return false;
        }
        #endregion

        public bool WriteLog(LogHeadType enLogType, string szLogMessage, string szRemark = null)
        {
            if (_log != null)
            {
                return WriteLog(LogHelper.GetDefaultLogLevel(enLogType), enLogType, szLogMessage, szRemark);
            }
            else
                return false;
        }

        public bool WriteLog(LogLevel level, LogHeadType enLogType, string szLogMessage, string szRemark = null)
        {
            if (_log != null)
                return _log.WriteLog(_objectName, level, enLogType, szLogMessage, szRemark);
            else
                return false;
        }

        public bool WriteLog(HRESULT hr, string track = null, LogLevel level = LogLevel.ERROR)
        {
            if (_log != null)
            {
                if (string.IsNullOrWhiteSpace(track))
                    return _log.WriteLog(_objectName, level, LogHeadType.Exception, hr._message, hr.ALID.ToString());
                else
                    return _log.WriteLog(_objectName, level, LogHeadType.Exception, hr._message + track, hr.ALID.ToString());
            }
            else
                return false;
        }

        public bool WriteLog(Exception hr, LogLevel level = LogLevel.ERROR, string remark = null)
        {
            if (_log != null)
            {
                return _log.WriteLog(_objectName, level, LogHeadType.Exception, hr.Message + hr.StackTrace, remark);
            }
            else
                return false;
        }

        public bool FlushLog()
        {
            if (_log != null)
            {
                _log.ForceWrite();
                return true;
            }
            else
                return false;
        }

        public void SetToolTip(Control control, string Caption)
        {
            if (control != null)
                m_toolTip.SetToolTip(control, Caption);
        }

        public void FirePreAlignAngleUpdated(object value)
        {
            if (OnPreAlignAngleUpdated != null)
            {
                foreach (PreAlignAngleUpdatedDel action in OnPreAlignAngleUpdated.GetInvocationList())
                {
                    try
                    {
                        action.BeginInvoke(value, null, null);
                    }
                    catch (Exception e)
                    {
                        WriteLog(LogHeadType.Exception,
                            string.Format("Exception occurs in FirePreAlignAngleUpdated(). Delegate Source: {0}. Reason: {1}", action.ToString(), e.Message));
                    }
                }
            }
        }

        public delegate void ProgressAbort();
        public event ProgressAbort ProgressAbortEvent;

        private static bool progressShowing = false;
        protected string progress_key = "";
        protected static ManualResetEvent UserClick = new ManualResetEvent(false);
        private bool _isProgressAborted = false;

        private void NotifyProgressAbort()
        {
            ProgressAbort handler = ProgressAbortEvent;
            if (handler != null)
            {
                handler();
            }
        }

        protected string KeyGenerate()
        {
            Random i = new Random();
            return i.Next(0, 10000).ToString();
        }

        public bool isProgressWindowShowing
        {
            get { return progressShowing; }
        }

        //Is Aborted by User (abort button clicked)
        public bool isProgressAborted
        {
            get { return _isProgressAborted; }
        }

        public void EnableMainForm(bool enable)
        {
            if (GUIBase != null)
            {
                GUIBase.EnableMainForm(enable);
            }
        }


        public void ShowMessageThread(string text, string caption = "EFEM GUI", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            object[] para = new object[4];
            para[0] = text;
            para[1] = caption;
            para[2] = buttons;
            para[3] = icon;

            ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_ShowMessage), para);
        }

        private void TPool_ShowMessage(object para)
        {
            object[] var = (object[])para;
            ShowMessage(var[0].ToString(), var[1].ToString(), (MessageBoxButtons)var[2], (MessageBoxIcon)var[3]);
        }

        public void ShowMessage(string text, string caption = "EFEM GUI", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            Form owner = Instance().GUIBase.GetBaseInstance();
            if (owner == null)
                return;
            else
            {
                if (_base.GetBaseInstance().InvokeRequired)
                {
                    MethodInvoker del = delegate { MessageBox.Show(_base.GetBaseInstance(), text, caption, buttons, icon); };
                    Instance().GUIBase.GetBaseInstance().Invoke(del);
                }
                else
                {
                    WriteLog(LogHeadType.Info, text, "ShowMessage()");
                    MessageBox.Show(_base.GetBaseInstance(), text, caption, buttons, icon);
                }
            }
        }

        public DialogResult ShowDialogOnTop(Form controlToShow)
        {
            //Form tf = Application.OpenForms.OfType<Form>().Where((t) => t.TopMost).FirstOrDefault();

            Form tf = null;
            var tt = Application.OpenForms.OfType<Form>().Where((t) => t.TopMost).ToList();

            foreach (Form form in tt)
            {
                tf = form;
                break;
            }

            if (_base.GetBaseInstance().InvokeRequired)
            {
                object rst = Instance().GUIBase.GetBaseInstance().Invoke(
                    new Func<DialogResult>(
                        delegate
                        {
                            if (!controlToShow.Visible)
                                return controlToShow.ShowDialog(tf);
                            else
                                return DialogResult.Retry;
                        }
                        ));

                return (DialogResult)rst;
            }
            else
            {
                WirteUserActionLog(LogHeadType.Info, controlToShow.Text, "ShowDialogOnTop()");
                DialogResult rst = controlToShow.ShowDialog(tf);
                WirteUserActionLog(LogHeadType.Info, "  ---> User click " + rst.ToString(), "ShowDialogOnTop()");
                return rst;
            }
        }

        public DialogResult ShowMessageOnTop(string text, string caption = "EFEM GUI", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None, MessageBoxDefaultButton defaultBtn = MessageBoxDefaultButton.Button1)
        {
            Form tf = null;
            var tt = Application.OpenForms.OfType<Form>().Where((t) => t.TopMost).ToList();

            foreach (Form form in tt)
            {
                tf = form;
                break;
            }

            if (_base.GetBaseInstance().InvokeRequired)
            {
                object rst = Instance().GUIBase.GetBaseInstance().Invoke(
                    new Func<DialogResult>(
                        delegate
                        {
                            return MessageBox.Show(tf, text, caption, buttons, icon, defaultBtn);
                        }
                        ));

                return (DialogResult)rst;
            }
            else
            {
                WirteUserActionLog(LogHeadType.Info, text, "ShowMessageBox()");
                DialogResult rst = MessageBox.Show(tf, text, caption, buttons, icon, defaultBtn);
                WirteUserActionLog(LogHeadType.Info, "  ---> User click " + rst.ToString(), "ShowMessageBox()"); 
                return rst;
            }
        }

        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public const UInt32 SWP_NOSIZE = 0x0001;
        public const UInt32 SWP_NOMOVE = 0x0002;
        public const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    }

    public interface EFEMGUIBase
    {
        void EnableMainForm(bool enable);
        void SetLoginInfo(string userType, string userID);
        Form GetBaseInstance();
    }

    public interface IProgressCallback
    {
        void Begin(int minimum, int maximum);
        void Begin();
        void SetRange(int minimum, int maximum);
        void SetText(String text);
        void StepTo(int val);
        void Increment(int val);
        bool IsAborting { get;}
        void End();
    }
}
