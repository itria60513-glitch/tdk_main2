using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EFEM.DataCenter;

namespace EFEM.GUIControls
{
    public partial class AlarmHistoryFilterCtrl : UserControl
    {
        private BindingSource curBindingSource = null;

        private DateTime alarmRangeMin = DateTimePicker.MinimumDateTime;
        private DateTime alarmRangeMax = DateTimePicker.MaximumDateTime;
        private ToolTip toolTip = new ToolTip();
        private bool isFilterEnabled = false;
        private bool isRDMode = false;
        public delegate void delTimePeriodChanged(string periodString);
        public event delTimePeriodChanged OnTimePeriodChanged = null;

        #region filter List
        /// <summary>
        /// FilterString[0] : DateTime Filter Command
        /// FilterString[1] : ALID Type Filter Command
        /// FilterString[2] : CatType Filter Command
        /// FilterString[3] : Message Filter Command
        /// </summary>
        private string[] filterString = new string[4] { "", "", "", ""};
        private List<string>[] slF = new List<string>[4] { new List<string>(), new List<string>(), new List<string>(), new List<string>() };
        private bool[] filterEnabled = new bool[4] { false, true, true, true };
        private FilterOper[] filterOper = new FilterOper[4] { FilterOper.AND, FilterOper.AND, FilterOper.AND, FilterOper.AND};
        private const string strWholePeriod = "Period: <Whole>";
        private string lDateTimePeriod = strWholePeriod;
        private bool _isFilterInited = false;
        private string _curAlarmListFilter = null;
        #endregion

        public AlarmHistoryFilterCtrl()
        {
            InitializeComponent();
        }

        public void InitAll()
        {
            InitializeAlarmFilterControls();
            RDMode = ConstVC.Debug.ShowRDGUI;
            cbCatType.SelectedIndex = 0;
            FireTimePeriodChanged(strWholePeriod);
        }

        public bool RDMode
        {
            get { return isRDMode; }
            set
            {
                isRDMode = value;
                lCurCommand.Visible = value;
                tbCurrentCommand.Visible = value;
                btnSendCommand.Visible = value;
            }
        }

        public BindingSource Source
        {
            get { return curBindingSource; }
            set
            {
                curBindingSource = value;
                UpdateFilter(value);
            }
        }

        public bool EnableFilter
        {
            get 
            { 
                return isFilterEnabled; 
            }
            set
            {
                isFilterEnabled = value;
                if (value)
                {
                    if (!cbPeriod.Checked)
                        FireTimePeriodChanged(strWholePeriod);
                    else
                        FireTimePeriodChanged(lDateTimePeriod);

                    UpdateAlarmFilter(false);
                }
                else
                {
                    FireTimePeriodChanged(strWholePeriod);

                    if (curBindingSource != null)
                        curBindingSource.Filter = null;
                }
            }
        }

        private void FireTimePeriodChanged(string message)
        {
            if (OnTimePeriodChanged != null)
            {
                OnTimePeriodChanged(message);
            }
        }

        private void UpdateFilterList(ComboBox ctrl, List<string> list, bool goCurrentSelected = true)
        {
            if (ctrl == null)
                return;

            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { UpdateFilterList(ctrl, list, goCurrentSelected); };
                this.Invoke(del);
            }
            else
            {
                string curSelected = ctrl.Text;
                int newSelectedIndex = -1;
                ctrl.Items.Clear();

                if (list != null && list.Count > 0)
                {
                    ctrl.Items.AddRange(list.ToArray());
                    newSelectedIndex = 0;
                }

                if (goCurrentSelected && ctrl.Items.Contains(curSelected))
                {
                    newSelectedIndex = ctrl.Items.IndexOf(curSelected);
                }

                if (newSelectedIndex >= 0)
                    ctrl.SelectedIndex = newSelectedIndex;               
            }
        }

        private void ResetDateTimeFilterPeriod(bool goCurrentSelected = true)
        {

            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { ResetDateTimeFilterPeriod(goCurrentSelected); };
                this.Invoke(del);
            }
            else
            {
                try
                {
                    dtPerStart.MinDate = alarmRangeMin;
                    dtPerStart.MaxDate = alarmRangeMax;
                    dtPerEnd.MinDate = alarmRangeMin;
                    dtPerEnd.MaxDate = alarmRangeMax;
                }
                catch
                {
                    dtPerStart.MinDate = DateTimePicker.MinimumDateTime;
                    dtPerStart.MaxDate = DateTimePicker.MaximumDateTime;
                    dtPerEnd.MinDate = DateTimePicker.MinimumDateTime;
                    dtPerEnd.MaxDate = DateTimePicker.MaximumDateTime;
                }
                finally
                {
                    dtPerStart.Value = dtPerStart.MinDate;
                    dtPerEnd.Value = dtPerEnd.MaxDate;
                }
            }
        }

        private void UpdateFilter(BindingSource source)
        {
            if (source != null)
            {
                var historyList = source.List;

                if (historyList != null && historyList.Count > 0)
                {
                    try
                    {
                        var list = historyList
                             .OfType<DataRowView>()
                             .Select(r => r["ALID"].ToString())
                             .Distinct();

                        List<string> sorted = list.OrderBy(r => r, StringComparer.CurrentCultureIgnoreCase).ToList();

                        UpdateFilterList(cbALID, sorted, _isFilterInited);
                    }
                    catch (Exception ex)
                    {
                        GUIBasic.Instance().WriteLog(ex, LogUtilities.LogLevel.ERROR, "Failed to update ALID filter list.");
                    }

                    alarmRangeMin = DateTimePicker.MinimumDateTime;
                    alarmRangeMax = DateTimePicker.MaximumDateTime;

                    try
                    {
                        var list = historyList
                            .OfType<DataRowView>()
                            .Select(r => r["SetTime"].ToString())
                            .Distinct();

                        var sorted = list.OrderBy(r => r).ToList();

                        if (sorted != null && sorted.Count > 0)
                        {
                            DateTime tmpMin = Convert.ToDateTime(sorted[0]);
                            DateTime tmpMax = Convert.ToDateTime(sorted[sorted.Count - 1]);

                            if (alarmRangeMin < tmpMin)
                                alarmRangeMin = tmpMin;
                            if (alarmRangeMax > tmpMax)
                                alarmRangeMax = tmpMax;
                        }
                    }
                    catch (Exception ex)
                    {
                        GUIBasic.Instance().WriteLog(ex, LogUtilities.LogLevel.ERROR, "Failed to update alarm period.");
                    }
                    finally
                    {
                        ResetDateTimeFilterPeriod();
                    }

                    source.Filter = _curAlarmListFilter;
                    _isFilterInited = true;
                }
            }
        }

        private void InitializeAlarmFilterControls()
        {
            if (cbOPALID.SelectedIndex < 0)
                cbOPALID.SelectedIndex = (int)filterOper[(int)FilterType.ALID];
            if (cbOPCategory.SelectedIndex < 0)
                cbOPCategory.SelectedIndex = (int)filterOper[(int)FilterType.Category];
            if (cbOPMessage.SelectedIndex < 0)
                cbOPMessage.SelectedIndex = (int)filterOper[(int)FilterType.Message];

            toolTip.SetToolTip(cbOPALID, "ALID Filter Operation");
            toolTip.SetToolTip(cbOPCategory, "Category Filter Operation");
            toolTip.SetToolTip(cbOPMessage, "Message Filter Operation");
        }

        private string GetFilterString(FilterType type)
        {
            if (filterEnabled[(int)type])
                return filterString[(int)type];
            else
                return "";
        }

        private string GetTotalFilterCommandString()
        {
            bool isDateTimeEnabled = !string.IsNullOrWhiteSpace(GetFilterString(FilterType.DateTime));

            string DateTimeFilter = GetFilterString(FilterType.DateTime);
            string TotalFilterString = "";
            string preOp = "";

            for (int i = 1; i < filterString.Length; i++) //Skip DateTime
            {
                string sString = GetFilterString((FilterType)i);
                if (!string.IsNullOrWhiteSpace(sString))
                {
                    string Operation = " AND "; //For AND and NOT operation
                    if (filterOper[i] == FilterOper.OR)
                        Operation = " OR ";

                    if (TotalFilterString != "")
                        TotalFilterString += Operation;
                    else
                        preOp = Operation;

                    TotalFilterString += sString;
                }
            }

            if (isDateTimeEnabled)
            {
                string preBool = (preOp == " OR " ? "FALSE" : "TRUE");
                TotalFilterString = string.Format("{0} AND ({1}{2}{3})", DateTimeFilter, preBool, preOp, TotalFilterString);
            }

            return TotalFilterString;
        }

        private string getColumnName(FilterType type)
        {
            switch (type)
            {
                case FilterType.ALID:
                    return "ALID";
                case FilterType.Category:
                    return "Category";
                case FilterType.DateTime:
                    return "SetTime";
                case FilterType.Message:
                    return "Message";
            }

            return "";
        }

        private bool UpdateFilterString(FilterType type)
        {
            int index = (int)type;

            switch (type)
            {
                case FilterType.DateTime:
                    {
                        filterString[index] = "";
                        string columnName = getColumnName(type);

                        filterString[index] = string.Format("({0} >= #{1}# AND {0} <= #{2}#)",
                            columnName,
                            dtPerStart.Value.ToString(dtPerStart.CustomFormat),
                            dtPerEnd.Value.ToString(dtPerStart.CustomFormat));
                        return true;
                    }
                case FilterType.ALID:
                case FilterType.Category:
                    {
                        filterString[index] = "";
                        List<string> filterList = slF[index];
                        string columnName = getColumnName(type);
                        foreach (string fString in filterList)
                        {
                            if (filterOper[index] != FilterOper.NOT)
                            {
                                if (filterString[index] != "") filterString[index] += " OR ";
                                filterString[index] += (columnName + " = '" + fString + "'");
                            }
                            else
                            {
                                if (filterString[index] != "") filterString[index] += " AND ";
                                filterString[index] += (columnName + " <> '" + fString + "'");
                            }
                        }
                        if (filterString[index] != "")
                            filterString[index] = "(" + filterString[index] + ")";
                        return true;
                    }

                case FilterType.Message:
                    {
                        filterString[index] = "";
                        List<string> filterList = slF[index];
                        string columnName = getColumnName(type);
                        foreach (string fString in filterList)
                        {
                            string modified = fString.Replace("'", "''");
                            modified = modified.Replace("*", "[*]");
                            modified = modified.Replace("[", "[[--TMP");
                            modified = modified.Replace("]", "[]]");
                            modified = modified.Replace("[[--TMP", "[[]");

                            if (filterOper[index] != FilterOper.NOT)
                            {
                                if (filterString[index] != "") filterString[index] += " OR ";
                                filterString[index] += (columnName + " LIKE '%" + modified + "%'");
                            }
                            else
                            {
                                if (filterString[index] != "") filterString[index] += " AND ";
                                filterString[index] += (columnName + " NOT LIKE '%" + modified + "%'");
                            }
                        }
                        if (filterString[index] != "")
                            filterString[index] = "(" + filterString[index] + ")";

                        return true;
                    }
                default:
                    return false;
            }
        }

        private void UpdateAlarmFilter(bool reGet = true)
        {
            try
            {
                if (reGet)
                    _curAlarmListFilter = GetTotalFilterCommandString();

                tbCurrentCommand.Text = _curAlarmListFilter;

                if (curBindingSource != null)
                    curBindingSource.Filter = _curAlarmListFilter;
            }
            catch
            {

            }
        }

        public void ResetAllFilter()
        {
            _isFilterInited = false;
            _curAlarmListFilter = null;

            for (int i = 0; i < slF.Length; i++)
            {
                if (slF[i] != null)
                    slF[i].Clear();
            }

            cbALID.Items.Clear();
            cbMessage.Text = "";

            for (int i = 0; i < filterString.Length; i++)
                filterString[i] = "";

            if (curBindingSource != null)
            {
                curBindingSource.Filter = null;
            }
        }

        private void UpdateFilterListOfGUI(TextBox gTextbox, List<string> lFilterList)
        {
            if (lFilterList.Count == 0)
            {
                gTextbox.BackColor = SystemColors.Control;
                gTextbox.Text = "[No Filter]";
            }
            else
            {
                string ShowString = "";
                foreach (string fString in lFilterList)
                {
                    if (ShowString != "")
                        ShowString += " or ";

                    ShowString += "[" + fString + "]";
                }

                gTextbox.Text = ShowString;
                gTextbox.BackColor = Color.White;
            }
        }

        private void cbPeriod_CheckedChanged(object sender, EventArgs e)
        {
            filterEnabled[(int)FilterType.DateTime] = cbPeriod.Checked;
            dtPerStart.Enabled = cbPeriod.Checked;
            dtPerEnd.Enabled = cbPeriod.Checked;
            btnFilterPeriod.Enabled = cbPeriod.Checked;
            btnResetPeriod.Enabled = cbPeriod.Checked;

            if (!cbPeriod.Checked)
                FireTimePeriodChanged(strWholePeriod);
            else
                FireTimePeriodChanged(lDateTimePeriod);

            UpdateAlarmFilter();
        }

        private void cbALIDFilter_CheckedChanged(object sender, EventArgs e)
        {
            bool enable = cbALIDFilter.Checked;
            filterEnabled[(int)FilterType.ALID] = enable;
            cbALID.Enabled = enable;
            btnAddALID.Enabled = enable && cbALID.SelectedIndex >= 0;
            tbALID.Enabled = enable;
            btnClearALID.Enabled = enable;
            UpdateAlarmFilter();
        }

        private void cbMessageFilter_CheckedChanged(object sender, EventArgs e)
        {
            bool enable = cbMessageFilter.Checked;
            filterEnabled[(int)FilterType.Message] = enable;
            cbMessage.Enabled = enable;
            btnAddMessage.Enabled = enable && !string.IsNullOrWhiteSpace(cbMessage.Text);
            tbMessage.Enabled = enable;
            btnClearMessage.Enabled = enable;
            UpdateAlarmFilter();
        }

        private void cbCategoryFilter_CheckedChanged(object sender, EventArgs e)
        {
            bool enable = cbCategoryFilter.Checked;
            filterEnabled[(int)FilterType.Category] = enable;
            cbCatType.Enabled = enable;
            btnAddCatType.Enabled = enable && cbCatType.SelectedIndex >= 0;
            tbCatType.Enabled = enable;
            btnClearCatType.Enabled = enable;
            UpdateAlarmFilter();
        }

        private void dtPerStart_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                dtPerEnd.MinDate = dtPerStart.Value;
            }
            catch { }
        }

        private void dtPerEnd_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                dtPerStart.MaxDate = dtPerEnd.Value;
            }
            catch { }
        }

        private void btnFilterPeriod_Click(object sender, EventArgs e)
        {
            lDateTimePeriod = "Period: " + dtPerStart.Value.ToString() + " till " + dtPerEnd.Value.ToString();
            FireTimePeriodChanged(lDateTimePeriod);
            UpdateFilterString(FilterType.DateTime);
            UpdateAlarmFilter();
        }

        private void btnResetPeriod_Click(object sender, EventArgs e)
        {
            ResetDateTimeFilterPeriod();
        }

        private void cbALID_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnAddALID.Enabled = (cbALID.SelectedIndex >= 0);
        }

        private void cbMessage_TextChanged(object sender, EventArgs e)
        {
            btnAddMessage.Enabled = !string.IsNullOrWhiteSpace(cbMessage.Text);
        }

        private void cbCatType_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnAddCatType.Enabled = (cbCatType.SelectedIndex >= 0);
        }

        private void btnAddALID_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cbALID.Text)) return;

            int index = (int)FilterType.ALID;
            List<string> filterList = slF[index];

            if (!filterList.Contains(cbALID.Text))
            {
                filterList.Add(cbALID.Text);
                UpdateFilterListOfGUI(this.tbALID, filterList);
                UpdateFilterString(FilterType.ALID);

                UpdateAlarmFilter();
            }
        }

        private void btnAddMessage_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cbMessage.Text)) return;

            int index = (int)FilterType.Message;
            List<string> filterList = slF[index];

            if (!filterList.Any(s => s.Equals(cbMessage.Text, StringComparison.OrdinalIgnoreCase)))
            {
                filterList.Add(cbMessage.Text);
                UpdateFilterListOfGUI(this.tbMessage, filterList);
                UpdateFilterString(FilterType.Message);

                UpdateAlarmFilter();
            }
            else
            {
                MessageBox.Show(string.Format("Keyword - \"{0}\" is existed already!", cbMessage.Text));
            }
        }

        private void btnAddCatType_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cbCatType.Text)) return;

            int index = (int)FilterType.Category;
            List<string> filterList = slF[index];

            if (!filterList.Contains(cbCatType.Text))
            {
                filterList.Add(cbCatType.Text);
                UpdateFilterListOfGUI(this.tbCatType, filterList);
                UpdateFilterString(FilterType.Category);

                UpdateAlarmFilter();
            }
        }

        private void btnClearALID_Click(object sender, EventArgs e)
        {
            int index = (int)FilterType.ALID;
            List<string> filterList = slF[index];
            filterList.Clear();
            filterString[index] = "";
            UpdateFilterListOfGUI(this.tbALID, filterList);
            UpdateAlarmFilter();
        }

        private void btnClearMessage_Click(object sender, EventArgs e)
        {
            int index = (int)FilterType.Message;
            List<string> filterList = slF[index];
            filterList.Clear();
            filterString[index] = "";
            UpdateFilterListOfGUI(this.tbMessage, filterList);
            UpdateAlarmFilter();
        }

        private void btnClearCatType_Click(object sender, EventArgs e)
        {
            int index = (int)FilterType.Category;
            List<string> filterList = slF[index];
            filterList.Clear();
            filterString[index] = "";
            UpdateFilterListOfGUI(this.tbCatType, filterList);
            UpdateAlarmFilter();
        }

        private void cbOPALID_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = (int)FilterType.ALID;
            filterOper[index] = (FilterOper)cbOPALID.SelectedIndex;

            if (_isFilterInited)
            {
                UpdateFilterString(FilterType.ALID);
                UpdateAlarmFilter();
            }
        }

        private void cbOPMessage_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = (int)FilterType.Message;
            filterOper[index] = (FilterOper)cbOPMessage.SelectedIndex;

            if (_isFilterInited)
            {
                UpdateFilterString(FilterType.Message);
                UpdateAlarmFilter();
            }
        }

        private void cbOPCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = (int)FilterType.Category;
            filterOper[index] = (FilterOper)cbOPCategory.SelectedIndex;

            if (_isFilterInited)
            {
                UpdateFilterString(FilterType.Category);
                UpdateAlarmFilter();
            }
        }

        private void btnSendCommand_Click(object sender, EventArgs e)
        {
            string userCommand = tbCurrentCommand.Text;

            try
            {
                if (curBindingSource != null)
                    curBindingSource.Filter = userCommand;
            }
            catch (Exception ee)
            {
                MessageBox.Show("Fail to execute command. Reason: " + ee.Message, "Command Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public enum FilterType
    {
        DateTime = 0,
        ALID = 1,
        Category = 2,
        Message = 3
    }

    public enum FilterOper
    {
        OR = 0,
        AND = 1,
        NOT = 2
    }
}
