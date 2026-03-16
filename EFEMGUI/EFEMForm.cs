using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using Microsoft.WindowsAPICodePack.ApplicationServices;
using System.Runtime.CompilerServices;
using MS.WindowsAPICodePack.Internal;
using EFEM.DataCenter;
using EFEM.GUIControls;
using EFEM.ExceptionManagements;
using EFEM.LogUtilities;
using EFEM.VariableCenter;

namespace EFEMGUI
{
    public partial class EFEMForm : Form, EFEMGUIBase
    {
        private GUIExceptionManagementSink exSink = null;
        private bool _isAllCompomentInited = false;
        private static object _lockSingleException = new object();
        private static EFEMForm staticInstance = null;
        private static bool isRecoveryRegistered = false;
        private static System.Timers.Timer autoRestartTimer = new System.Timers.Timer(1000);
        private static System.Timers.Timer ARRTimer = new System.Timers.Timer(60000);

        public EFEMForm()
        {
            InitializeComponent();
            
            autoRestartTimer.Elapsed += new System.Timers.ElapsedEventHandler(autoRestartTimer_Elapsed);

            //GUIBasic.SetWindowPos(Notifier.Handle, GUIBasic.HWND_TOPMOST, 0, 0, 0, 0, GUIBasic.TOPMOST_FLAGS);

            exSink = new GUIExceptionManagementSink(efemMainControl, efemMainControl.efemStatusCtrl.efemAlarm);
            GUIBasic.Instance().ExceptionManagement.AlarmReportSendGUI += new AlarmReportSendGUIEventHandler(exSink.AlarmReportSend);
            GUIBasic.Instance().ExceptionManagement.OnAckAlarm += new AlarmAckEvent(exSink.AlarmAck);
            GUIBasic.Instance().RestartGUISink += new GUIBasic.delRestartGUISinck(RestartGUI);
            GUIBasic.Instance().GUIBase = this;
            ArrayList rst0 = GUIBasic.Instance().VariableCenter.InstantiateObjects();

            if (rst0 != null && rst0.Count > 0)
            {
                GUIBasic.Instance().ShowMessageOnTop("Error occurred during init VariableCenter. Application will be closed.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            GUIBasic.Instance().VariableCenterCallback +=new VariableCenterCallbackEventHandler(EFEMForm_VariableCenterCallback);

            string[] cmdLine = System.Environment.GetCommandLineArgs();
            if (cmdLine != null)
            {
                //foreach (string cmd in cmdLine)
                for (int i=0;i<cmdLine.Length;i++)
                {
                    GUIBasic.Instance().WriteLog(LogLevel.DEBUG, LogHeadType.Info, string.Format("cmd[{0}] = {1}", i, cmdLine[i])); 
                }
            }

            efemMainControl.InitAllGeneralComponts();
            GUIBasic.Instance().FileUtility.FlushEFEMConfig();
            staticInstance = this;
        }

        public static void RegisterApplicationRecoveryAndRestart()
        {
            GUIBasic.Instance().VariableCenter.SetValueAndFireCallback("ARR_Ready", false);
            if (!CoreHelpers.RunningOnWin7)
            {
                GUIBasic.Instance().WriteLog(LogHeadType.Notify, "Fail to register ARR due to operation system is not supported.");
                isRecoveryRegistered = false;
                return;
            }
            try
            {
                string folder = ConstVC.FilePath.CrashReportFolder;
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                RestartSettings restartSettings = new RestartSettings(string.Format(@"-restart"), RestartRestrictions.NotOnReboot | RestartRestrictions.NotOnPatch);
                ApplicationRestartRecoveryManager.RegisterForApplicationRestart(restartSettings);

                RecoverySettings recoverySettings =
                    new RecoverySettings(new RecoveryData(PerformRecovery, null), 0);
                ApplicationRestartRecoveryManager.RegisterForApplicationRecovery(recoverySettings);

                isRecoveryRegistered = true;
                ARRTimer.Elapsed += new System.Timers.ElapsedEventHandler(ARRTimer_Elapsed);
                ARRTimer.Start();
            }
            catch (Exception e)
            {
                GUIBasic.Instance().WriteLog(LogHeadType.Exception, "Fail to register ARR due exception occurred. Reason: " + e.Message);
                isRecoveryRegistered = false;
            }
        }

        static void ARRTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            string message = "ARR is ready.";
            ARRTimer.Stop();
            GUIBasic.Instance().WriteLog(LogHeadType.Info, message);

            GUIBasic.Instance().VariableCenter.SetValueAndFireCallback(ConstVC.VariableCenter.CurrentStatus, message);
            GUIBasic.Instance().VariableCenter.SetValueAndFireCallback("ARR_Ready", true);
        }

        private static void UnregisterApplicationRecoveryAndRestart()
        {
            GUIBasic.Instance().VariableCenter.SetValueAndFireCallback("ARR_Ready", false);
            if (!CoreHelpers.RunningOnWin7)
            {
                return;
            }

            if (isRecoveryRegistered)
            {
                ApplicationRestartRecoveryManager.UnregisterApplicationRestart();
                ApplicationRestartRecoveryManager.UnregisterApplicationRecovery();
            }
        }

        private static int PerformRecovery(object parameter)
        {
            try
            {
                ApplicationRestartRecoveryManager.ApplicationRecoveryInProgress();
                ApplicationRestartRecoveryManager.ApplicationRecoveryFinished(true);
            }
            catch
            {
                ApplicationRestartRecoveryManager.ApplicationRecoveryFinished(false);
            }

            return 0;
        }

        #region Handle Any unhandled Exception from UI or Thread
        public static void UI_UnhandledException(object sender, ThreadExceptionEventArgs t)
        {
            GUIBasic.Instance().WriteLog(LogLevel.ALL, LogHeadType.Exception_Software, "Unhandled Exception occurred in UI thread.");
            GUIBasic.Instance().WriteLog(t.Exception, LogLevel.ALL);
            GUIBasic.Instance().FlushLog();
        }

        public static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            lock (_lockSingleException)
            {
                try
                {
                    DisableForm();
                    GUIBasic.Instance().WriteLog(LogLevel.ALL, LogHeadType.Exception_Software, "Unhandled Exception occurred in current domain thread.");
                    Exception ex = (Exception)e.ExceptionObject;
                    GUIBasic.Instance().WriteLog(ex, LogLevel.ALL);
                    GUIBasic.Instance().FlushLog();
                    if (e.IsTerminating)
                    {
                        autoRestartTimer.Interval = 30 * 1000;
                        autoRestartTimer.Start();

                        if (GUIBasic.Instance().ShowMessageOnTop("Unexpected software exception can not be handled. Application will be terminated!\nSee EFEMGUI log for more detials!\n\nEFEM GUI will be restarted automatically after 30 seconds.\nClick [Cancel] to cancel restart procedure.\nClick [Ok] to restart immediately. ",
                            "EFEM GUI Exception", MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.Cancel)
                        {
                            autoRestartTimer.Stop();
                            autoRestartTimer.Dispose();
                            DisposeAllEFEMObject(false, false);
                        }
                        else
                        {
                            autoRestartTimer.Stop();
                            autoRestartTimer.Dispose();
                            DisposeAllEFEMObject(false, true);
                        }
                    }
                }
                catch
                {
                    if (e.IsTerminating)
                    {
                        GUIBasic.Instance().ShowMessageOnTop("EFEM - Unexpected software exception can not be handled. Application will be terminated!",
                            "EFEM GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                finally
                {
                    if (e.IsTerminating)
                        System.Environment.Exit(1);
                    else
                        EnableForm();
                }
            }
        }

        static void autoRestartTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            GUIBasic.Instance().WriteLog(LogLevel.ALL, LogHeadType.KeepEyesOn, "Auto restarting EFEM GUI...");
            GUIBasic.Instance().DisposeAllBasicObjects();
            Application.Restart();
            System.Environment.Exit(0);

            //DisposeAllEFEMObject(false, true, true);
        }
        #endregion

        void EFEMForm_VariableCenterCallback(string VariableName, object Value)
        {
            if (VariableName == "CurrentStatusMessage")
            {
                if (!_isAllCompomentInited && Value.ToString() == "All Initializations Finish")
                {
                    _isAllCompomentInited = true;
                    efemMainControl.SwitchToOperationPage(true);
                }
                if (!_isAllCompomentInited && Value.ToString() == "All Initializations Finish (Fail)")
                {
                    _isAllCompomentInited = false;
                    efemMainControl.SwitchToOperationPage(true, true);
                }
                else if (Value.ToString() == "ForceOperatedByUser") 
                {
                    efemMainControl.SwitchToOperationPage(false);
                    return;
                }
                efemMainControl.AddStatusMessage(Value.ToString());
            }
            else
                efemMainControl.VariableCenterUpdated(VariableName, Value);
        }

        public static EFEMForm GetInstance()
        {
            return staticInstance;
        }

        private static void DisableForm(bool isWholeForm = false)
        {
            if (staticInstance != null)
            {
                if (staticInstance.InvokeRequired)
                {
                    MethodInvoker del = delegate { DisableForm(isWholeForm); };
                    staticInstance.Invoke(del);
                }
                else
                {
                    if (isWholeForm)
                        staticInstance.Enabled = false;
                    else
                        GUIBasic.Instance().EnableMainForm(false);
                }
            }
        }

        private static void EnableForm(bool isWholeForm = false)
        {
            if (staticInstance != null)
            {
                if (staticInstance.InvokeRequired)
                {
                    MethodInvoker del = delegate { EnableForm(isWholeForm); };
                    staticInstance.Invoke(del);
                }
                else
                {
                    if (isWholeForm)
                        staticInstance.Enabled = true;
                    else
                        GUIBasic.Instance().EnableMainForm(true);
                }
            }
        }

        protected static void RestartGUI()
        {
            DisposeAllEFEMObject(false, true);
        }

        protected static void DisposeAllEFEMObject(bool Async = true, bool restartGUI = false, bool disableNotifier = false)
        {
            GUIBasic.Instance().SetDisposingState();

            if (restartGUI)
                Async = false;

            int iWaitTimeOut = 30000;
            DisableForm();
            ManualResetEvent syncObj = new ManualResetEvent(false);
            GUIBasic.Instance().WriteLog(LogHeadType.MethodEnter, "", string.Format("DisposeAllEFEMObject(), Restart = {0}", restartGUI));
            if (!disableNotifier)
            {
                Application.DoEvents();
            }

            if (Async)
                ThreadPool.RegisterWaitForSingleObject(syncObj, new WaitOrTimerCallback(ForceExit), syncObj, iWaitTimeOut, true);
            else
            {
                int counter = iWaitTimeOut / 100;
                while (!syncObj.WaitOne(100) && counter > 0)
                {
                    GUIBasic.Instance().InvalidateNotifier();
                    Application.DoEvents();
                    Thread.Yield();
                    counter--;
                }

                if (restartGUI)
                    Application.Restart();

                System.Environment.Exit(0);
            }
        }

        private static void ForceExit(object obj, bool timeout)
        {
            if (timeout)
            {
                ManualResetEvent _syncObj = (ManualResetEvent)obj;
                _syncObj.Set();
            }

            System.Environment.Exit(0);
        }

        protected static void TPOOL_DisposeAllEFEMObject(object syncobj)
        {
            Thread.Sleep(200);
            ManualResetEvent _syncObj = (ManualResetEvent)syncobj;            
            GUIBasic.Instance().DisposeAllBasicObjects();
            _syncObj.Set();
        }
        
        protected override void OnClosing(CancelEventArgs e)
        {
            GUIBasic.Instance().WriteLog(LogLevel.INFO, LogHeadType.Info, "User try to close EFEM GUI.");

            if (GUIBasic.Instance().ShowMessageOnTop("Sure to shutdown the EFEM GUI?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
                //GUIBasic.Instance().WriteLog(LogLevel.INFO, LogHeadType.Info, "Close EFEM GUI ==> User click YES");

                GUIBasic.Instance().ExceptionManagement.AlarmReportSendGUI -= new AlarmReportSendGUIEventHandler(exSink.AlarmReportSend);
                DisposeAllEFEMObject();
                e.Cancel = true; //will be terminated automatically
            }
            else
            {
                GUIBasic.Instance().WriteLog(LogLevel.INFO, LogHeadType.Info, "Close EFEM GUI ==> User click NO");
                e.Cancel = true;
                return;
            }
        }

        public void EnableMainForm(bool enable)
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { EnableMainForm(enable); };
                this.Invoke(del);
            }
            else
            {
                efemMainControl.SuspendLayout();
                efemMainControl.ResumeLayout(false);
                efemMainControl.PerformLayout();
            }
        }

        public Form GetBaseInstance()
        {
            return this;
        }

        public void SetLoginInfo(string userType, string userID)
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { SetLoginInfo(userType, userID); };
                this.Invoke(del);
            }
            else
            {
                efemMainControl.SetLoginInfo(userType, userID);

                if (string.IsNullOrWhiteSpace(userType) || string.IsNullOrWhiteSpace(userID))
                    this.Text = "EFEM GUI";
                else
                    this.Text = string.Format("EFEM GUI [Login: {0} (Type:{1})]", userID, userType);
            }
        }
    }

    public class GUIExceptionManagementSink : AbstractExceptionManagement4GUI_Sink
    {
        EFEMAlarm _alarm = null;
        EFEMMainControl _main = null;

        public GUIExceptionManagementSink(EFEMMainControl main, EFEMAlarm alarm)
        {
            _main = main;
            _alarm = alarm;
        }

        override protected void AlarmAck_Sink(uint ALID, DateTime ackTime)
        {
            if (_alarm != null)
                _alarm.Invoke(new MethodInvoker(delegate { _alarm.AckAlarm(ALID, ackTime); }));
        }

        override protected void AlarmReportSend_Sink(byte ALCD, ExceptionInfo ExInfo, string TIMESTAMP, bool ifPopupMessageBox)
        {
            try
            {
                //ArrayList al = new ArrayList();
                //al.Add(ALCD);
                //al.Add(ExInfo);
                //al.Add(TIMESTAMP);
                //al.Add(ifPopupMessageBox);

                //ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_AlarmReportSend), (object)al);

                bool isClear = (ALCD == 0X0);
                ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_AlarmReportSend), isClear);
            }
            catch (Exception ex)
            {
                GUIBasic.Instance().WriteLog(LogHeadType.Exception_Software, ex.Message + ex.StackTrace, "GUIExceptionManagementSink");
            }
        }

        private void TPool_AlarmReportSend(object ThreadPoolObj)
        {
            //ArrayList al = (ArrayList)ThreadPoolObj;
            //byte ALCD = (byte)al[0];
            //ExceptionInfo ExInfo = (ExceptionInfo)al[1];
            //string TIMESTAMP = (string)al[2];
            //bool ifPopupMessageBox = (bool)al[3];

            //string mode = (ALCD == 0X0) ? "[AlarmClear]" : "[AlarmSet]";

            bool isClear = Convert.ToBoolean(ThreadPoolObj);
            if(_alarm != null)
                _alarm.Invoke(new MethodInvoker(delegate { _alarm.RefreshAll(); }));

            if (_main != null)
                _main.Invoke(new MethodInvoker(delegate { _main.AlarmEventFired(isClear); }));

        }
    }
 }
