using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using EFEM.ExceptionManagements;
using EFEM.DataCenter;
using EFEM.LogUtilities;
using EFEM.VariableCenter;
using Communication.Interface;
using Communication;
using EFEM.FileUtilities;

namespace EFEM.GUIControls
{
    public partial class EFEMMainControl : UserControl
    {
        private string sHostStatus = "Not Initialized Yet";
        private string sRobotStatus = "Not Initialized Yet";
        private string sAlignerStatus = "Not Initialized Yet";
        private string sDIO0Status = "Not Initialized Yet";
        private string sFFUStatus = "Not Initialized Yet";

        private bool _isOnlineMode = false; //Switch via GUI by user
        private FileUtilities.LoginType _curLoginType = FileUtilities.LoginType.None;
        private object _lockEFEMStatus = new object();

        private object _lockAlarmCount = new object();
        private int currentActiveAlarmCount = 0;
        //private ICommunication com;

        public EFEMMainControl()
        {
            InitializeComponent();

            //com = Communication.Module.Communication.GetUniqueInstance();

            tabMain.Appearance = TabAppearance.FlatButtons;
            tabMain.ItemSize = new Size(0, 1);
            tabMain.SizeMode = TabSizeMode.Fixed;

            StatusIndInit.SetToopTipMsg("Not Initialized Yet", ToolTipIcon.Error);
            StatusIndInit.AddOnUserClickEvent(new EventHandler(StatusIndInit_DoubleClick));

            StatusIndHost.SetToopTipMsg(sHostStatus, ToolTipIcon.Error);

            ChangeDisplayMode(true);

            this.startupController.InvokeHostActionRequested += (_, __) => SwitchToOperationPage();


            //GUIBasic.Instance().OnLoginTypeChanged += new UserLoginForm.delLoginTypeChanged(EFEMMainControl_OnLoginTypeChanged);
        }

        void EFEMMainControl_OnLoginTypeChanged(FileUtilities.LoginType type)
        {
            if (_curLoginType != type)
            {
                _curLoginType = type;
                ChangeDisplayMode();
            }
        }

        public void SwitchToOperationPage(bool checkError = true, bool HideButton = false)
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { SwitchToOperationPage(checkError, HideButton); };
                this.Invoke(del);
            }
            else
            {
                if (!checkError)
                {
                    if (!startupController.IsAnyErrorOccurs)
                    {
                        GUIBasic.Instance().VariableCenter.SetValueAndFireCallback(ConstVC.VariableCenter.CurrentStatus, "Switch to operation page. (Auto)");;
                    }
                    else
                        startupController.ShowContinueButton(HideButton);
                }
                else
                {
                    tabMain.SelectedTab = this.tabPageOperation;
                }

                UpdateInitializedStatus(false);
                StatusAlarmCount.DoubleClickEnabled = true;
                EFEMMainControl_OnLoginTypeChanged(LoginType.Admin);
            }
        }

        private void UpdateInitializedStatus(bool isDuringInitial)
        {
            if (isDuringInitial)
            {
                StatusIndInit.SetToopTipMsg("initializating EFEM Components.", ToolTipIcon.Info);
                StatusIndInit.Status = StatusType.Yellow;
            }
            else
            {
                if (startupController.IsAnyErrorOccurs)
                {
                    StatusIndInit.SetToopTipMsg("Error occurred during initialization on " + DateTime.Now, ToolTipIcon.Error);
                    StatusIndInit.Status = StatusType.Red;
                }
                else
                {
                    StatusIndInit.SetToopTipMsg("Initialized successfully on " + DateTime.Now, ToolTipIcon.Info);
                    StatusIndInit.Status = StatusType.Green;
                }
            }
        }

        public void InitAllGeneralComponts()
        {
            UpdateInitializedStatus(true);
            startupController.StartupAll(InitAllGUIComponts);
        }

        private ArrayList InitAllGUIComponts()
        {
            ArrayList al = new ArrayList();

            try
            {
                InitEfemStatusCtrl();
                InitEfemConfigCtrl();
            }
            catch (Exception e)
            {
                GUIBasic.Instance().WriteLog(LogLevel.ERROR, LogHeadType.Exception, "Exception occurred in InitAllGUIComponts(). " + 
                    e.Message + "\n" + e.StackTrace);
                al.Add(e.Message);
            }

            if (al == null || al.Count == 0)
                return null;
            else
                return al;
        }

        private void InitEfemStatusCtrl()
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { InitEfemStatusCtrl(); };
                this.Invoke(del);
            }
            else
            {
                GUIBasic.Instance().VariableCenter.SetValueAndFireCallback(ConstVC.VariableCenter.CurrentStatus,
                    "initializating EFEM Status Controls");

                efemStatusCtrl.InitAll();
            }
        }

        private void InitEfemConfigCtrl()
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { InitEfemConfigCtrl(); };
                this.Invoke(del);
            }
            else
            {
                GUIBasic.Instance().VariableCenter.SetValueAndFireCallback(ConstVC.VariableCenter.CurrentStatus,
                    "initializating EFEM Config Controls");
                //efemConfigCtrl.InitAll();
            }
        }

        public void SwitchToAlarmPage()
        {
            tabOperation.SelectedTab = tabStatus;
            efemStatusCtrl.ShowAlarmPage();
        }

        private void UpdateSwitchOnlineResult(bool isFail)
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { UpdateSwitchOnlineResult(isFail); };
                this.Invoke(del);
            }
            else
            {
                StatusOnline.Enabled = true;
            }
        }

        public void VariableCenterUpdated(string VariableName, object Value)
        {
            try
            {
                if (VariableName.StartsWith("[ST]"))
                {
                    UpdateStatusIndicator(VariableName, Value);
                }
                else if (VariableName.StartsWith("[TimeOut]"))
                {
                    if (VariableName == "[TimeOut]OnlineModeSwithProcedure")
                    {
                        UpdateSwitchOnlineResult(Convert.ToBoolean(Value));
                    }
                }
            }
            catch (Exception e)
            {
                GUIBasic.Instance().WriteLog(e, LogLevel.ERROR);
            }
        }

        #region Current Message Box
        public void ClearAllStatusMessage()
        {
            statusMessageCtrl.ClearAllMessage();
        }

        public void AddStatusMessage(string message, MsgType type = MsgType.INFO, bool isErr = false)
        {
            statusMessageCtrl.AddStatusMessage(message, type, isErr);
        }

        #endregion


        #region StatusIndicator
        private void UpdateStatusIndicator(string VariableName, object Value)
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { UpdateStatusIndicator(VariableName, Value); };
                this.Invoke(del);
            }
        }

        private void StatusIndInit_DoubleClick(object sender, EventArgs e)
        {
            if (startupController != null)
            {
                if (tabMain.SelectedTab != this.tabPageInit)
                {
                    Form tmpForm = new Form();
                    tmpForm.Text = "EFEM Initialization Status Report";
                    tmpForm.FormBorderStyle = FormBorderStyle.Fixed3D;
                    tmpForm.Size = new System.Drawing.Size(800, 250);
                    tmpForm.MaximizeBox = false;
                    tmpForm.MinimizeBox = false;

                    Control history = startupController.GetHistoryListControl();
                    history.Parent = tmpForm;

                    tmpForm.AutoScroll = true;
                    tmpForm.AutoSize = true;

                    tmpForm.StartPosition = FormStartPosition.CenterParent;
                    tmpForm.ShowDialog();

                    startupController.ReleaseHistoryListControl(history);
                    tmpForm.Dispose();
                }
            }
        }
        #endregion

        private void btnUIException_Click(object sender, EventArgs e)
        {
            throw new Exception("RD Debug - UI Exception");
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            if (GUIBasic.Instance().ShowMessageOnTop("Sure to restart GUI?", "EFEM GUI", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                GUIBasic.Instance().RestartGUI();
        }

        private void btnMakeAppException_Click(object sender, EventArgs e)
        {
            Application.OnThreadException(new Exception("RD Debug - Application Exception", new Exception("Inner")));
        }

        private void btnMakeCrash_Click(object sender, EventArgs e)
        {
            if (GUIBasic.Instance().VariableCenter.IsVariableExist("ARR_Ready"))
            {
                object value = GUIBasic.Instance().VariableCenter.GetValue("ARR_Ready");
                if  (!GUIBasic.Instance().VariableCenter.IsNotExistVariableReturn(value))
                {
                    bool state = (bool)value;
                    if (state)
                    {
                        Environment.FailFast("Crash Testing");
                        return;
                    }
                }
            }

            GUIBasic.Instance().ShowMessageOnTop("ARR is not ready. Please wait for at least 60 secconds", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            return;
        }

        private void tabOperation_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabPage current = (sender as TabControl).SelectedTab;
        }

        private void btnLockGUI_Click(object sender, EventArgs e)
        {
            GUIBasic.Instance().LockGUI();
        }

        private void btnUserLogin_Click(object sender, EventArgs e)
        {
            GUIBasic.Instance().LogIn();
        }

        private void btnUserLogOut_Click(object sender, EventArgs e)
        {
            GUIBasic.Instance().LogOut();
        }

        private void btnLockSystem_Click(object sender, EventArgs e)
        {
            GUIBasic.Instance().LockGUI();
        }

        public void AlarmEventFired(bool isClear)
        {
            lock (_lockAlarmCount)
            {
                if (isClear)
                    currentActiveAlarmCount--;
                else
                    currentActiveAlarmCount++;

                if (currentActiveAlarmCount <= 0)
                    StatusAlarmCount.BackColor = Color.Green;
                else
                    StatusAlarmCount.BackColor = Color.Red;

                StatusAlarmCount.Text = currentActiveAlarmCount.ToString();
            }
        }

        public void SetLoginInfo(string userType, string userID)
        {
            if (string.IsNullOrWhiteSpace(userType) || string.IsNullOrWhiteSpace(userID))
            {
                lLoginStatus.Text = "Status: <Not logged in>";
                lLoginStatus.ForeColor = Color.Red;
                btnUserLogOut.Enabled = false;
            }
            else
            {
                lLoginStatus.Text = string.Format("Status: [Login: {0} (Type:{1})]", userID, userType);
                lLoginStatus.ForeColor = Color.Black;
                btnUserLogOut.Enabled = true;
            }
        }

        private object _lockLayout = new object();
        private FileUtilities.LoginType lastLoginType = FileUtilities.LoginType.None;

        public void ChangeDisplayMode(bool forInit = false)
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { ChangeDisplayMode(); };
                this.Invoke(del);
            }
            else
            {
                lock (_lockLayout)
                {
                    if (forInit)
                    {
                        tabConfiguration.Parent = null;
                        return;
                    }

                    TabPage currentShow = tabOperation.SelectedTab;
                    this.tabOperation.SuspendLayout();

                    bool NeedToShowMaintainTab = GUIBasic.Instance().IsMaintainFunctionAllow;
                    bool NeedToShowServiceTab = GUIBasic.Instance().CurrentLoginType == FileUtilities.LoginType.Service;
                    bool IsCurrentMaintainTabShowed = false;

                    NeedToShowMaintainTab = true;

                    if (NeedToShowMaintainTab != IsCurrentMaintainTabShowed)
                    {
                        if (NeedToShowMaintainTab)
                        {
                            tabConfiguration.Parent = tabOperation;
                        }
                    }

                    try
                    {
                        if (currentShow != null && currentShow.Parent != null)
                            tabOperation.SelectedTab = currentShow;
                    }
                    catch (Exception ex)
                    {
                        //GUIBasic.Instance().WriteLog(ex, LogLevel.ERROR, "Failed to ChangeDisplayMode");
                    }

                    this.tabOperation.ResumeLayout(false);
                }
            }
        }

        private void StatusAlarmCount_DoubleClick(object sender, EventArgs e)
        {
            SwitchToAlarmPage();
        }
    }
}
