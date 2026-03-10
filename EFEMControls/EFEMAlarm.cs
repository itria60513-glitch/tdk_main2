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
using EFEM.DataCenter;
using EFEM.ExceptionManagements;

namespace EFEM.GUIControls
{
    public partial class EFEMAlarm : UserControl
    {
        #region Fields and Properties

        private DataTable dtHistory, dtActive;
        private string[] clnActiveAlarmHead = { "ALID", "Message", "SetTime", "AckTime", "Category", "Mode", "Acknowledge" };
        private string[] clnAlarmHistoryHead = { "ALID", "Message", "SetTime", "ClearTime", "Category", "Mode" };
        private bool isDtInited = false;
        private object lockActive = new object();
        private object lockHistory = new object();

        #endregion

        #region Constructor

        public EFEMAlarm()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods
        public void InitAll()
        {
            InitializeHistoryAlarm();
            InitializeActiveAlarm();
            alarmHistoryFilterCtrl1.OnTimePeriodChanged += new AlarmHistoryFilterCtrl.delTimePeriodChanged(alarmHistoryFilterCtrl1_OnTimePeriodChanged);
            alarmHistoryFilterCtrl1.InitAll();
            GUIBasic.Instance().OnLoginTypeChanged += new UserLoginForm.delLoginTypeChanged(EFEMAlarm_OnLoginTypeChanged);
            isDtInited = true;
            RefreshAll();
        }

        void EFEMAlarm_OnLoginTypeChanged(FileUtilities.LoginType type)
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { EFEMAlarm_OnLoginTypeChanged(type); };
                this.Invoke(del);
            }
            else
            {
                if (type == FileUtilities.LoginType.SWRD)
                    alarmHistoryFilterCtrl1.RDMode = true;
                else
                    alarmHistoryFilterCtrl1.RDMode = false;
            }
        }

        void alarmHistoryFilterCtrl1_OnTimePeriodChanged(string periodString)
        {
            tabHistory.Text = "History - " + periodString;
        }

        //Initialization
        private void InitializeHistoryAlarm()
        {
            if (dtHistory == null)
                dtHistory = new DataTable("AlarmHistory");

            foreach (string title in clnAlarmHistoryHead)
            {
                DataColumn cln = new DataColumn(title, typeof(System.String));
                dtHistory.Columns.Add(cln);
            }
        }

        private void InitializeActiveAlarm()
        {
            if (dtActive == null)
                dtActive = new DataTable("AlarmActive");

            foreach (string title in clnActiveAlarmHead)
            {
                DataColumn cln = null;
                if (title == "Acknowledge")
                    cln = new DataColumn(title, typeof(System.Boolean));
                else
                    cln = new DataColumn(title, typeof(System.String));

                dtActive.Columns.Add(cln);
            }
        }

        //Data refresh and manupulation
        public delegate void AlarmRefreshInvokeHandler();

        public void RefreshAll()
        {
            if (!isDtInited)
                return;

            RefreshHisrotyAlarmPage();
            RefreshActiveAlarmPage();
        }

        public void ShowActiveAlarmPage()
        {
            tcAlarm.SelectedTab = tabActive;
        }

        public void AckAlarm(uint ALID, DateTime ackTime)
        {
            if (dtActive == null)
                return;

            DataRow drow = dtActive.AsEnumerable().Where(p => p.Field<string>(clnActiveAlarmHead[0]) == ALID.ToString()).FirstOrDefault();
            if (drow != null)
            {
                drow[clnActiveAlarmHead[6]] = true;
                drow[clnActiveAlarmHead[3]] = ackTime.ToString(ConstVC.Display.AlarmListDataTimeFormat);
                dtActive.AcceptChanges();
            }
        }

        private void PerformLayOut(Control ctrl)
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { PerformLayOut(ctrl); };
                this.Invoke(del);
            }
            else
            {
                ctrl.PerformLayout();
            }
        }

        private void EnableControl(ToolStripItem ctrl, bool enable)
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { EnableControl(ctrl, enable); };
                this.Invoke(del);
            }
            else
            {
                ctrl.Enabled = enable;
            }
        }

        public void RefreshHisrotyAlarmPage()
        {
            EnableControl(btnRefreshHistory, false);
            TPool_RefreshHisrotyAlarmPage(null);
        }

        public void RefreshActiveAlarmPage()
        {
            EnableControl(btnRefreshActive, false);
            TPool_RefreshActiveAlarmPage(null);
        }

        //DataTable & DataRow is not thread safe
        bool isAnyHistoryData = false;
        private void TPool_RefreshHisrotyAlarmPage(object para)
        {
            Monitor.Enter(lockHistory);
            try
            {
                if (!isDtInited)
                    return;

                dtHistory.Clear();
                if (GUIBasic.Instance().ExceptionManagement != null)
                {
                    ArrayList alHistory = GUIBasic.Instance().ExceptionManagement.ListHistoryAlarmRequest();

                    //foreach (ExceptionInfo item in alHistory)
                    //Lawrence 2015/12/23 [Bug Fix]: Add ToArray() to avoid racing condition
                    //Cause of alHistory is reference type and may be changed by ExceptionManagement
                    foreach (ExceptionInfo item in alHistory.ToArray()) 
                    {
                        DataRow row = dtHistory.NewRow();

                        row[clnAlarmHistoryHead[0]] = item.ALID.ToString();
                        row[clnAlarmHistoryHead[1]] = item.mesage;
                        row[clnAlarmHistoryHead[2]] = item.DateTime_Set.ToString(ConstVC.Display.AlarmListDataTimeFormat);
                        row[clnAlarmHistoryHead[3]] = item.DateTime_Clear.ToString(ConstVC.Display.AlarmListDataTimeFormat);
                        row[clnAlarmHistoryHead[4]] = item.category;
                        row[clnAlarmHistoryHead[5]] = item.mode;

                        dtHistory.Rows.Add(row);
                    }

                    isAnyHistoryData = alHistory.Count > 0;
                }
                else
                    isAnyHistoryData = false;
            }
            catch (Exception e)
            {
                GUIBasic.Instance().WriteLog(LogUtilities.LogHeadType.Exception, e.Message + e.StackTrace, "RefreshHistoryAlarmPage");
                GUIBasic.Instance().ShowMessageOnTop("Fail to get Alarm History. Reason: " + e.Message, "RefreshHistoryAlarmPage");
            }
            finally
            {
                BindingHistoryDB();

                PerformLayOut(dgHistory);
                EnableControl(btnRefreshHistory, true);
                EnableControl(btnEnableFilter, isAnyHistoryData);
                Monitor.Exit(lockHistory);
            }
        }

        //DataTable & DataRow is not thread safe
        private void TPool_RefreshActiveAlarmPage(object para)
        {
            Monitor.Enter(lockActive);
            try
            {
                if (!isDtInited)
                    return;

                dtActive.Clear();

                if (GUIBasic.Instance().ExceptionManagement != null)
                {
                    Hashtable hsActive = GUIBasic.Instance().ExceptionManagement.ListActiveAlarmRequest();

                    foreach (DictionaryEntry entry in hsActive)
                    {
                        DataRow row = dtActive.NewRow();
                        ExceptionInfo item = (ExceptionInfo)entry.Value;
                        bool isAck = (item.state >= ExStateReporting.ACKNOWLEDGED);
                        row[clnActiveAlarmHead[0]] = item.ALID.ToString();
                        row[clnActiveAlarmHead[1]] = item.mesage;
                        row[clnActiveAlarmHead[2]] = item.DateTime_Set.ToString(ConstVC.Display.AlarmListDataTimeFormat);
                        row[clnActiveAlarmHead[3]] = isAck ? item.DateTime_ACK.ToString(ConstVC.Display.AlarmListDataTimeFormat) : "---";
                        row[clnActiveAlarmHead[4]] = item.category;
                        row[clnActiveAlarmHead[5]] = item.mode;
                        row[clnActiveAlarmHead[6]] = isAck;

                        dtActive.Rows.Add(row);
                    }
                }
            }
            catch (Exception e)
            {
                GUIBasic.Instance().WriteLog(LogUtilities.LogHeadType.Exception, e.Message + e.StackTrace, "RefreshActiveAlarmPage");
                GUIBasic.Instance().ShowMessageOnTop("Fail to get Active Alarm List. Reason: " + e.Message, "RefreshActiveAlarmPage");
            }
            finally
            {
                BindingActiveDB();

                PerformLayOut(dgAlarm);
                EnableControl(btnRefreshActive, true);
                Monitor.Exit(lockActive);
            }
        }
        #endregion

        private void BindingActiveDB()
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { BindingActiveDB(); };
                this.Invoke(del);
            }
            else
            {
                if (bindingSourceActive.DataSource == null)
                {
                    bindingSourceActive.DataSource = dtActive;
                    dgAlarm.DataSource = bindingSourceActive;
                    bindingNavigatorActive.BindingSource = bindingSourceActive;
                    dgAlarm.StretchColumn(clnActiveAlarmHead[1]);
                    ChangeDeleteEnableState();
                }
            }
        }

        private void BindingHistoryDB()
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { BindingHistoryDB(); };
                this.Invoke(del);
            }
            else
            {
                if (bindingSourceHistory.DataSource == null)
                {
                    bindingSourceHistory.DataSource = dtHistory;
                    dgHistory.DataSource = bindingSourceHistory;
                    bindingNavigatorHistory.BindingSource = bindingSourceHistory;
                    dgHistory.StretchColumn(clnAlarmHistoryHead[1]);
                    alarmHistoryFilterCtrl1.Source = bindingSourceHistory;
                }
            }
        }

        private void bindingNavigatorDeleteItem_Click(object sender, EventArgs e)
        {
            ClearManualAlarm();
            ChangeDeleteEnableState();
        }

        private void ClearManualAlarm()
        {
            if (dgAlarm.SelectedRows.Count <= 0)
            {
                GUIBasic.Instance().ShowMessageOnTop("There is no data or selected item!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int index = dgAlarm.CurrentCell.RowIndex;

            if (index < 0)
            {
                GUIBasic.Instance().ShowMessageOnTop("No selected item for operation", "Alarm Clear");
                return;
            }

            UInt32 ALID = Convert.ToUInt32(dgAlarm.Rows[index].Cells[0].Value);
            ExMode Mode = (ExMode)Enum.Parse(typeof(ExMode), dgAlarm.Rows[index].Cells[5].Value.ToString());

            if (Mode == ExMode.Manual)
            {
                bool IsSuccess = GUIBasic.Instance().ExceptionManagement.AlarmClear(ALID);

                if (!IsSuccess)
                    GUIBasic.Instance().ShowMessageOnTop(string.Format("Clear Alarm:(ALID:[{0}]) failed", ALID));
                else
                    RefreshAll();
            }
            else
                GUIBasic.Instance().ShowMessageOnTop("Cannot clear a system managed alarm.\n Alarm will be cleared automatically when alarm state does not exist.", "Alarm Clear", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void bindingNavigatorPositionItem_TextChanged(object sender, EventArgs e)
        {
            ChangeDeleteEnableState();
        }

        private void bindingNavigatorPositionItem_EnabledChanged(object sender, EventArgs e)
        {
            ChangeDeleteEnableState();
        }

        private void ChangeDeleteEnableState()
        {
            //if (bindingNavigatorPositionItem.Enabled)
            //{
            //    int currentRow = Convert.ToInt32(bindingNavigatorPositionItem.Text);
            //    if (currentRow > 0 && currentRow <= dgAlarm.Rows.Count)
            //    {
            //        ExMode Mode = (ExMode)Enum.Parse(typeof(ExMode), dgAlarm.Rows[currentRow - 1].Cells[clnActiveAlarmHead[4]].Value.ToString());
            //        if (Mode == ExMode.ex_Manual)
            //            bindingNavigatorDeleteItem.Enabled = true;
            //        else
            //            bindingNavigatorDeleteItem.Enabled = false;
            //    }
            //    else
            //        bindingNavigatorDeleteItem.Enabled = false;
            //}
            //else
            //{
            //    bindingNavigatorDeleteItem.Enabled = false;
            //}
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshActiveAlarmPage();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            RefreshHisrotyAlarmPage();
        }

        private void btnEnableFilter_Click(object sender, EventArgs e)
        {
            if (btnEnableFilter.Checked)
            {
                gFilter.Visible = true;
                btnEnableFilter.Text = "Filter - Enabled";
                btnEnableFilter.Image = global::EFEM.GUIControls.Properties.Resources.filter;
                alarmHistoryFilterCtrl1.EnableFilter = true;
            }
            else
            {
                gFilter.Visible = false;
                btnEnableFilter.Text = "Filter - Disabled";
                btnEnableFilter.Image = global::EFEM.GUIControls.Properties.Resources.filter_disable;
                alarmHistoryFilterCtrl1.EnableFilter = false;
            }
        }

        public void ReLocateAll()
        {
            Control alarmparent = dgAlarm.Parent;
            Control historyParent = dgHistory.Parent;

            if (alarmparent != null && historyParent != null)
            {
                this.SuspendLayout();
                alarmparent.SuspendLayout();
                historyParent.SuspendLayout();
                dgAlarm.Parent = null;
                dgHistory.Parent = null;
                gFilter.Parent = null;
                bindingNavigatorActive.Parent = null;
                bindingNavigatorHistory.Parent = null;
                dgAlarm.Parent = alarmparent;
                bindingNavigatorActive.Parent = alarmparent;
                dgHistory.Parent = historyParent;
                gFilter.Parent = historyParent;
                bindingNavigatorHistory.Parent = historyParent;
                alarmparent.ResumeLayout(false);
                historyParent.ResumeLayout(false);
                this.ResumeLayout(false);
            }
        }

        private void tcAlarm_Resize(object sender, EventArgs e)
        {
            ReLocateAll();
        }
    }
}
