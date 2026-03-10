using Communication;
using Communication.Connector;
using Communication.Interface;
using Communication.Protocol;
using EFEM.DataCenter;
using EFEM.ExceptionManagements;
using EFEM.FileUtilities;
using LogUtility;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using TDKController;
using TDKController.Interface;
using TDKLogUtility.Module;
using static EFEM.DataCenter.ConstVC;
using LogHeadType = EFEM.LogUtilities.LogHeadType;

namespace EFEM.GUIControls
{
    public partial class StartupComponentCtrl : UserControl
    {
        public delegate ArrayList delInitializeGUI();
        private delInitializeGUI InitializeGUIHelper = null;
        private ArrayList errorHistory = new ArrayList();
        private int DelayTimeForDebug = 100;
        private bool Inversed = false;
        public event EventHandler InvokeHostActionRequested;
        AbstractFileUtilities _fu = FileUtility.GetUniqueInstance();
        private ILogUtility _log;
        private Dictionary<string, IConnector> _communication = new Dictionary<string, IConnector>();
        private Dictionary<string, ILoadPortActor> _loadPort = new Dictionary<string, ILoadPortActor>();

        public List<(string, int)> _commList = new List<(string, int)>();
        public Dictionary<string, TCPConfig> _tcpSetting = new Dictionary<string, TCPConfig>();
        public Dictionary<string, RS232Config> _serialSetting = new Dictionary<string, RS232Config>();
        public List<string> _dioList = new List<string>();
        public Dictionary<string, DIOConfig> _dioSetting = new Dictionary<string, DIOConfig>();
        public List<string> _loadPortList = new List<string>();
        public Dictionary<string, LoadPortConfig> _loadPortSetting = new Dictionary<string, LoadPortConfig>();
        public List<string> _n2NozzleList = new List<string>();
        public Dictionary<string, N2NozzleConfig> _n2NozzleSetting = new Dictionary<string, N2NozzleConfig>();

        private enum ProcedureStatus
        {
            Pending = 0,
            Working = 1,
            Finish = 2
        }

        enum CommType
        {
            RS232,
            TCPIP
        }

        public StartupComponentCtrl()
        {
            
            _log = LogUtilityClient.GetUniqueInstance("",0);
            InitializeComponent();
            InitDataGrid();
        }

        public ArrayList ErrorHistory
        {
            get 
            {
                if (errorHistory == null || errorHistory.Count == 0)
                    return null;
                else
                    return errorHistory; 
            }
        }

        public bool IsAnyErrorOccurs
        {
            get { return (ErrorHistory != null); }
        }

        public void ShowContinueButton(bool Hide = false)
        {
            btnContinue.Visible = !Hide;
            if (IsAnyErrorOccurs)
                timerBlinking.Enabled = true;
        }

        private void InitDataGrid()
        {
            for (int i = 0; i < 10; i++)
            {
                dataGridViewResult.Rows.Add();
                dataGridViewResult.Rows[i].Cells["Image"].Value = imageListStatus.Images[0];
                dataGridViewResult.Rows[i].Cells["Status"].Value = "Waiting";
                dataGridViewResult.Rows[i].Cells["ObjectName"].Value = GetObjectName(i);
                dataGridViewResult.Rows[i].Cells["ErrorMsg"].Value = "";
            }

            ExtendMethods.StretchLastColumn(dataGridViewResult);
        }

        public Control GetHistoryListControl()
        {
            return dataGridViewResult;
        }

        public void ReleaseHistoryListControl(Control ctrl)
        {
            ctrl.Parent = panelDataGrid;
        }

        private string GetObjectName(int ID)
        {
            switch (ID)
            {
                case 0:
                    return "Communication";
                case 1:
                    return "DIO";
                case 2:
                    return "LoadPortActor";
                case 3:
                    return "CarrierIDReader";
                case 4:
                    return "LightCurtain";
                case 5:
                    return "N2Nozzle";
                case 6:
                    return "LoadPortController";
                case 7:
                    return "LoadPortService";
                case 8:
                    return "E84Station";
                case 9:
                    return "Initialize GUI Components";
                default:
                    return "";
            }
        }

        private string GetProcedureName(int ID)
        {
            switch (ID)
            {
                case 0:
                case 4:
                    return "Instantiate Objects";
                case 1:
                case 5:
                    return "Establish Communications";
                case 2:
                case 6:
                    return "Download Parameters";
                case 3:
                case 7:
                    return "Initialize";
                case 9:
                    return "Initialize GUI Components";
                default:
                    return "";
            }
        }

        public HRESULT StartupAll(delInitializeGUI InitGUIMethod)
        {
            InitializeGUIHelper = InitGUIMethod;


            //ThreadPool.QueueUserWorkItem(new WaitCallback(TPOOL_StartupAll), com);
            TPOOL_StartupAll();
            return null;
        }

        private void UpdateStatus(string currentStatus)
        {
            if (lCurrentStatus.InvokeRequired)
            {
                MethodInvoker del = delegate { UpdateStatus(currentStatus); };
                lCurrentStatus.Invoke(del);
            }
            else
            {
                if (currentStatus == "Finish")
                {
                    if (IsAnyErrorOccurs)
                    {
                        lCurrentStatus.BackColor = Color.Red;
                        lCurrentStatus.ForeColor = Color.White;
                        lCurrentStatus.Text = "Error Occurred During Initialization of EFEM components.";
                    }
                    else
                    {
                        lCurrentStatus.Text = "Success";
                    }
                }
                else
                    lCurrentStatus.Text = currentStatus;
            }
        }

        private void UpdateProcedureStatus(int ProcedureID, ProcedureStatus status, ArrayList rst = null)
        {
            if (lCurrentStatus.InvokeRequired)
            {
                MethodInvoker del = delegate { UpdateProcedureStatus(ProcedureID, status, rst); };
                lCurrentStatus.Invoke(del);
            }
            else
            {
                switch (status)
                {
                    case ProcedureStatus.Pending:
                        {
                            dataGridViewResult.Rows[ProcedureID].Cells["Image"].Value = imageListStatus.Images[0];
                            dataGridViewResult.Rows[ProcedureID].Cells["Status"].Value = "Pending";
                            dataGridViewResult.Rows[ProcedureID].Cells["ErrorMsg"].Value = "";
                            break;
                        }
                    case ProcedureStatus.Working:
                        {
                            GUIBasic.Instance().WriteLog(LogHeadType.CallStart, GetProcedureName(ProcedureID));
                            dataGridViewResult.Rows[ProcedureID].Cells["Image"].Value = imageListStatus.Images[1];
                            dataGridViewResult.Rows[ProcedureID].Cells["Status"].Value = "Working";
                            dataGridViewResult.Rows[ProcedureID].Cells["ErrorMsg"].Value = "";
                            break;
                        }
                    case ProcedureStatus.Finish:
                        {
                            if (rst != null && rst.Count != 0)
                            {
                                string errMsg = ExtendMethods.ToStringHelper(rst, "; ");
                                GUIBasic.Instance().WriteLog(LogHeadType.CallEnd, GetProcedureName(ProcedureID) + ", Fail. Reason: " + errMsg);
                                errorHistory.Add("[" + GetProcedureName(ProcedureID) + "] " + errMsg);
                                dataGridViewResult.Rows[ProcedureID].Cells["Image"].Value = imageListStatus.Images[3];
                                dataGridViewResult.Rows[ProcedureID].Cells["Status"].Value = "Error";
                                dataGridViewResult.Rows[ProcedureID].Cells["ErrorMsg"].Value = errMsg;
                            }
                            else
                            {
                                GUIBasic.Instance().WriteLog(LogHeadType.CallEnd, GetProcedureName(ProcedureID) + ", Success.");
                                dataGridViewResult.Rows[ProcedureID].Cells["Image"].Value = imageListStatus.Images[2];
                                dataGridViewResult.Rows[ProcedureID].Cells["Status"].Value = "Success";
                                dataGridViewResult.Rows[ProcedureID].Cells["ErrorMsg"].Value = "";
                            }
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        private void TPOOL_StartupAll()
        {
            try
            {
                errorHistory.Clear();
                int curProcrdure = 0;
                string objectName;
                string procedureName;
                string str;
                ArrayList rst;
                //var obj = (ICommunication)para;

                objectName = GetObjectName(curProcrdure);
                procedureName = GetProcedureName(7);
                str = objectName + " Initialize Start.";
                _log.WriteLog("TDK_GUI", str);
                UpdateStatus(objectName + " : " + procedureName);
                UpdateProcedureStatus(curProcrdure, ProcedureStatus.Working);
                GUIBasic.Instance().VariableCenter.SetValueAndFireCallback(ConstVC.VariableCenter.CurrentStatus,
                    objectName + " -> " + procedureName);
                rst = Communication_Initialize();
                UpdateProcedureStatus(curProcrdure, ProcedureStatus.Finish, rst);
                Thread.Sleep(DelayTimeForDebug);
                curProcrdure = curProcrdure + 1;
                str = objectName + " Initialize End.";
                _log.WriteLog("TDK_GUI", str);

                objectName = GetObjectName(curProcrdure);
                procedureName = GetProcedureName(7);
                str = objectName + " Initialize Start.";
                _log.WriteLog("TDK_GUI", str);
                UpdateStatus(objectName + " : " + procedureName);
                UpdateProcedureStatus(curProcrdure, ProcedureStatus.Working);
                GUIBasic.Instance().VariableCenter.SetValueAndFireCallback(ConstVC.VariableCenter.CurrentStatus,
                    objectName + " -> " + procedureName);
                rst = DIO_Initialize();
                UpdateProcedureStatus(curProcrdure, ProcedureStatus.Finish, rst);
                Thread.Sleep(DelayTimeForDebug);
                curProcrdure = curProcrdure + 1;
                str = objectName + " Initialize End.";
                _log.WriteLog("TDK_GUI", str);

                objectName = GetObjectName(curProcrdure);
                procedureName = GetProcedureName(7);
                str = objectName + " Initialize Start.";
                _log.WriteLog("TDK_GUI", str);
                UpdateStatus(objectName + " : " + procedureName);
                UpdateProcedureStatus(curProcrdure, ProcedureStatus.Working);
                GUIBasic.Instance().VariableCenter.SetValueAndFireCallback(ConstVC.VariableCenter.CurrentStatus,
                    objectName + " -> " + procedureName);
                rst = LoadPort_Initialize();
                UpdateProcedureStatus(curProcrdure, ProcedureStatus.Finish, rst);
                Thread.Sleep(DelayTimeForDebug);
                curProcrdure = curProcrdure + 3;
                str = objectName + " Initialize End.";
                _log.WriteLog("TDK_GUI", str);

                objectName = GetObjectName(curProcrdure);
                procedureName = GetProcedureName(7);
                str = objectName + " Initialize Start.";
                _log.WriteLog("TDK_GUI", str);
                UpdateStatus(objectName + " : " + procedureName);
                UpdateProcedureStatus(curProcrdure, ProcedureStatus.Working);
                GUIBasic.Instance().VariableCenter.SetValueAndFireCallback(ConstVC.VariableCenter.CurrentStatus,
                    objectName + " -> " + procedureName);
                rst = N2Nozzle_Initialize();
                UpdateProcedureStatus(curProcrdure, ProcedureStatus.Finish, rst);
                Thread.Sleep(DelayTimeForDebug);
                curProcrdure = curProcrdure + 4;
                str = objectName + " Initialize End.";
                _log.WriteLog("TDK_GUI", str);


                #region Init GUI

                objectName = GetObjectName(curProcrdure);
                procedureName = GetProcedureName(curProcrdure);
                GUIBasic.Instance().WriteLog(LogHeadType.CallStart, "", objectName + "." + procedureName);
                UpdateStatus(objectName + " : " + procedureName);
                UpdateProcedureStatus(curProcrdure, ProcedureStatus.Working);
                GUIBasic.Instance().VariableCenter.SetValueAndFireCallback(ConstVC.VariableCenter.CurrentStatus,
                    objectName + " -> " + procedureName);
                if (InitializeGUIHelper != null)
                    rst = InitializeGUIHelper();
                else
                    rst = null;
                UpdateProcedureStatus(curProcrdure, ProcedureStatus.Finish, rst);
                Thread.Sleep(DelayTimeForDebug);
                curProcrdure++;

                //Clean log and start the timer to claen logs every 24 hours
                GUIBasic.Instance().Log.ClearLogs();
                GUIBasic.Instance().WriteLog(LogHeadType.CallEnd, "", objectName + "." + procedureName);

                #endregion

                UpdateStatus("Finish");
                GUIBasic.Instance().VariableCenter.SetValueAndFireCallback(ConstVC.VariableCenter.CurrentStatus,
                    "All Initializations Finish");
            }
            catch (Exception e)
            {
                UpdateStatus("Exception: " + e.Message);
                errorHistory.Add("[TPOOL_StartupAll] " + e.Message + e.StackTrace);
                GUIBasic.Instance().VariableCenter.SetValueAndFireCallback(ConstVC.VariableCenter.CurrentStatus,
                    "All Initializations Finish (Fail)");
            }
            finally
            { ShowContinueButton(false);
            }
        }

        public ArrayList Communication_Initialize()
        {
            string str = string.Empty;
            ArrayList al = new ArrayList();
            try
            {
                AbstractFileUtilities fu = FileUtility.GetUniqueInstance();
                IConnectorConfig com;

                _commList = fu.GetCommList();

                if (_commList.Count == 0)
                {
                    str = "Communication Config Load Error.";
                    al.Add(str);
                    _log.WriteLog("TDK_GUI",TDKLogUtility.Module.LogHeadType.Error, str);
                }
                else
                {
                    foreach (var comm in _commList)
                    {
                        switch ((CommType)comm.Item2)
                        {
                            case CommType.TCPIP:
                                _tcpSetting[comm.Item1] = fu.GetTCPSetting(comm.Item1);
                                if (   _tcpSetting[comm.Item1].Ip.Equals(string.Empty)
                                    || _tcpSetting[comm.Item1].Port.Equals(string.Empty))
                                {
                                    str = comm.Item1 + " : Wrong Value of Communication Setting.(TCPIP)";
                                    al.Add(str);
                                    _log.WriteLog("TDK_GUI", TDKLogUtility.Module.LogHeadType.Error, str);
                                }
                                else
                                {
                                    com = new TCPConnectorConfig(_tcpSetting[comm.Item1]);
                                    _communication[comm.Item1] = new TcpipConnector(new DefaultProtocol(), com, _log);
                                }

                                break;
                            case CommType.RS232:
                                _serialSetting[comm.Item1] = fu.GetSerialSetting(comm.Item1);
                                if (  !( Regex.IsMatch(_serialSetting[comm.Item1].Port, @"^COM\d+$"))
                                    || _serialSetting[comm.Item1].Baud == -1
                                    || _serialSetting[comm.Item1].Parity == -1
                                    || _serialSetting[comm.Item1].DataBits == -1
                                    || _serialSetting[comm.Item1].StopBits == -1)
                                {
                                    str = comm.Item1 + " : Wrong Value of Communication Setting.(RS232)";
                                    al.Add(str);
                                    _log.WriteLog("TDK_GUI", TDKLogUtility.Module.LogHeadType.Error, str);
                                }
                                else
                                {
                                    com = new RS232ConnectorConfig(_serialSetting[comm.Item1]);
                                    _communication[comm.Item1] = new Rs232Connector(_log, com);
                                }

                                break;
                            default:
                                str = comm.Item1 + " : Wrong Type of Communication Setting.(Not TCP or RS232)";
                                al.Add(str);
                                _log.WriteLog("TDK_GUI", TDKLogUtility.Module.LogHeadType.Error, str);
                                break;
                        }

                    }
                }
                return al;

            }
            finally
            {
            }

        }

        private ArrayList LoadPort_Initialize()
        {
            string str = string.Empty;
            ArrayList al = new ArrayList();
            LoadportActorConfig loadConfig = new LoadportActorConfig();
            List<string> commList = _commList.Select(x => x.Item1).ToList();
            try
            {
                _loadPortList = _fu.GetLoadPortList();
                if (_loadPortList.Count == 0)
                {
                    str = "LoadPort Config Load Error.";
                    al.Add(str);
                    _log.WriteLog("TDK_GUI", TDKLogUtility.Module.LogHeadType.Error, str);
                }
                else
                {
                    foreach (var loadPort in _loadPortList)
                    {
                        _loadPortSetting[loadPort] = _fu.GetLoadPortConfigSetting(loadPort);
                        string comm = _loadPortSetting[loadPort].Comm != null
                            ? _loadPortSetting[loadPort].Comm
                            : string.Empty;
                        if (   comm.Equals(string.Empty)
                            || !(commList.Contains(comm))
                            || _loadPortSetting[loadPort].INFTimeout < 0
                            || _loadPortSetting[loadPort].ACKTimeout < 0)
                        {
                            str = loadPort + " : Wrong Value of LoadPort Setting.";
                            al.Add(str);
                            _log.WriteLog("TDK_GUI", TDKLogUtility.Module.LogHeadType.Error, str);
                        }
                        else if (!(_communication.Keys.Contains(comm)))
                        {
                            str = loadPort + " : " + comm + " Initial Error.";
                            al.Add(str);
                            _log.WriteLog("TDK_GUI", TDKLogUtility.Module.LogHeadType.Error, str);
                        }
                        else if (ConnectCheck(_communication[comm]))
                        {
                            str = loadPort + " : " + comm + " Communication Connect Error.";
                            al.Add(str);
                            _log.WriteLog("TDK_GUI", TDKLogUtility.Module.LogHeadType.Error, str);
                        }
                        else
                        {
                            loadConfig.AckTimeout = _loadPortSetting[loadPort].ACKTimeout;
                            loadConfig.InfTimeout = _loadPortSetting[loadPort].INFTimeout;
                            _loadPort[loadPort] = new LoadportActor(loadConfig, _communication[comm], _log);
                        }


                    }
                }

                return al;
            }
            finally
            {
            }
        }

        private ArrayList DIO_Initialize()
        {
            string str = string.Empty;
            ArrayList al = new ArrayList();

            try
            {
                _dioList = _fu.GetDIOList();

                if (_dioList.Count == 0)
                {
                    str = "DIO Config Load Error.";
                    al.Add(str);
                    _log.WriteLog("TDK_GUI", TDKLogUtility.Module.LogHeadType.Error, str);
                }
                else
                {
                    foreach (var dioName in _dioList)
                    {
                        _dioSetting[dioName] = _fu.GetDIOConfigSetting(dioName);

                        if (   _dioSetting[dioName].Type == -1
                            || _dioSetting[dioName].Index == -1
                            || _dioSetting[dioName].MaxDIPort == -1
                            || _dioSetting[dioName].MaxDOPort == -1
                            || _dioSetting[dioName].PinCountPerPort == -1)
                        {
                            str = dioName + " : Wrong Value of DIO Setting.";
                            al.Add(str);
                            _log.WriteLog("TDK_GUI", TDKLogUtility.Module.LogHeadType.Error, str);
                        }
                        else
                        {
                            
                        }

                    }
                }

                //al.
                return al;

            }
            finally
            {
            }
        }

        private ArrayList N2Nozzle_Initialize()
        {
            //log = new LogUtilityClient();
            string str = string.Empty;
            ArrayList al = new ArrayList();
            List<string> commList = _commList.Select(x => x.Item1).ToList();
            try
            {
                _n2NozzleList = _fu.GetN2NozzleList();
                if (_n2NozzleList.Count == 0)
                {
                    str = "N2Nozzle Config Load Error.";
                    al.Add(str);
                    _log.WriteLog("TDK_GUI", TDKLogUtility.Module.LogHeadType.Error, str);
                }
                else
                {
                    foreach (var n2Nozzle in _n2NozzleList)
                    {
                        _n2NozzleSetting[n2Nozzle] = _fu.GetN2NozzleConfigSetting(n2Nozzle);
                        string comm = _n2NozzleSetting[n2Nozzle].Comm != null
                            ? _n2NozzleSetting[n2Nozzle].Comm
                            : string.Empty;
                        if (comm.Equals(string.Empty) || !(commList.Contains(comm)))
                        {
                            str = n2Nozzle + " : Wrong Value of N2Nozzle Setting.";
                            al.Add(str);
                            _log.WriteLog("TDK_GUI", TDKLogUtility.Module.LogHeadType.Error, str);
                        }
                        else if (!(_communication.Keys.Contains(comm)))
                        {
                            str = n2Nozzle + " : " + comm + " Initial Error.";
                            al.Add(str);
                            _log.WriteLog("TDK_GUI", TDKLogUtility.Module.LogHeadType.Error, str);
                        }
                        else if (ConnectCheck(_communication[comm]))
                        {
                            str = n2Nozzle + " : " + comm + " Communication Connect Error.";
                            al.Add(str);
                            _log.WriteLog("TDK_GUI", TDKLogUtility.Module.LogHeadType.Error, str);
                        }
                        else
                        {

                        }

                    }
                }

                return al;

            }
            finally
            {
            }
        }

        private bool ConnectCheck(IConnector connector)
        {
            if (connector.IsConnected)
            {
                return false;
            }
            else if (connector.Connect() != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void dataGridViewResult_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells["Image"].Value = imageListStatus.Images[0];
            e.Row.Cells["Status"].Value = "Waiting";
            e.Row.Cells["ErrorMsg"].Value = "";
        }

        private void btnContinue_Click(object sender, EventArgs e)
        {
            timerBlinking.Enabled = false;
            if (IsAnyErrorOccurs)
            {
                lCurrentStatus.BackColor = Color.Red;
                lCurrentStatus.ForeColor = Color.White;
            }

            InvokeHostActionRequested?.Invoke(this, EventArgs.Empty);

            GUIBasic.Instance().VariableCenter.SetValueAndFireCallback(ConstVC.VariableCenter.CurrentStatus, "ForceOperatedByUser");
        }

        private void timerBlinking_Tick(object sender, EventArgs e)
        {
            if (Inversed)
            {
                lCurrentStatus.BackColor = Color.Red;
                lCurrentStatus.ForeColor = Color.White;
            }
            else
            {
                lCurrentStatus.BackColor = SystemColors.Control;
                lCurrentStatus.ForeColor = Color.Black;
            }

            Inversed = !Inversed;
        }

        private void dataGridViewResult_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                object err = dataGridViewResult.Rows[e.RowIndex].Cells["ErrorMsg"].Value;
                if (err == null)
                    return;
                else
                {
                    string errorMsg = dataGridViewResult.Rows[e.RowIndex].Cells["ErrorMsg"].Value.ToString();
                    if (string.IsNullOrWhiteSpace(errorMsg))
                        return;
                    else
                    {
                        using (InitStatusErrorListForm listForm = new InitStatusErrorListForm())
                        {
                            string caption = string.Format("{0}",
                                dataGridViewResult.Rows[e.RowIndex].Cells["ObjectName"].Value.ToString());

                            if (listForm.AssignData(caption, errorMsg))
                            {
                                listForm.TopMost = true;
                                listForm.StartPosition = FormStartPosition.CenterParent;
                                listForm.ShowDialog();
                            }
                        }
                    }
                }
            }
        }

        private void copyAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                StringBuilder buffer = new StringBuilder();
                for (int j = 0; j < dataGridViewResult.RowCount; j++)
                {
                    for (int i = 1; i < dataGridViewResult.ColumnCount; i++)
                    {
                        buffer.Append(dataGridViewResult.Rows[j].Cells[i].Value.ToString());
                        buffer.Append("\t");
                    }

                    buffer.Append("\r\n");
                }

                Clipboard.SetText(buffer.ToString());
            }
            catch (Exception ex)
            {
                GUIBasic.Instance().WriteLog(LogHeadType.Exception, "Copy to clipboard failed! Reason: " + ex.Message);
                GUIBasic.Instance().ShowMessageOnTop("Copy to clipboard failed!");
            }
        }
    }
}
