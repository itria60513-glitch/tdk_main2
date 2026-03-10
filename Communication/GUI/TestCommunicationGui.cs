using Communication.Interface;
using Communication.Protocol;
using EFEM.DataCenter;
using EFEM.FileUtilities;
using LogUtility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Communication.Connector;
using TDKLogUtility.Module;

namespace Communication.GUI
{
    public partial class TestCommunicationGui : UserControl
    {
        private IConnector _loadportConnector;
        private IProtocol _loadportProtocol;
        private IConnectorConfig _loadportConfig;
        private ILogUtility _log;
        private IConnector _comConnector;
        private IConnectorConfig _comConfig;
        public TestCommunicationGui()
        {
            InitializeComponent();
            this.Disposed += TestCommunicationGui_Disposed;
        }

        private void TestCommunicationGui_Disposed(object sender, EventArgs e)
        {
            _loadportConnector?.Disconnect();
            _comConnector?.Disconnect();
        }


        private void TestCommunicationGui_Load(object sender, EventArgs e)
        {
            #region Socket Init
            _loadportConfig = new TCPConnectorConfig(new TCPConfig("127.0.0.1", "8080"));
            _loadportProtocol = new LoadportProtocol();
            _log = LogUtilityClient.GetUniqueInstance("", 0);
            _loadportConnector = new TcpipConnector(_loadportProtocol, _loadportConfig, _log) { Name = "Loadport2" };
            _loadportConnector.DataReceived += _loadportConnector_DataReceived;
            var hrResult = _loadportConnector.Connect();

            if (hrResult != null)
            {
                SendCommandBtn.Text = @"Disconnected";
                SendCommandBtn.ForeColor = Color.Red;
                SendCommandBtn.Enabled = false;
                MessageTB.Text = MessageTB.Text.Insert(0, $"Tcp{_loadportConfig.Ip}:{_loadportConfig.Port} : Connecting fail.\n");
            }
            else
            {
                SendCommandBtn.Text = @"Send";
                SendCommandBtn.ForeColor = Color.Black;
                SendCommandBtn.Enabled = true;
                MessageTB.Text = MessageTB.Text.Insert(0, $"Tcp{_loadportConfig.Ip}:{_loadportConfig.Port} : Connected.\n");
            }

            #endregion Socket Init

            Rs232CB.Items.Add("COM1");
            Rs232CB.Items.Add("COM2");
        }

        private void _loadportConnector_DataReceived(byte[] byData, int length)
        {
            string requestStr = Encoding.UTF8.GetString(byData, 0, length);

            if (InvokeRequired)
            {
                this.BeginInvoke(new System.Action(() => _loadportConnector_DataReceived(byData, length)));

                return;
            }
            this.Invoke(() =>
            {
                MessageTB.Text = MessageTB.Text.Insert(0, $"Tcp Response: {requestStr}\n");
            });

        }

        private void Rs232CB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_comConnector != null)
            {
                _comConnector.Disconnect();
                MessageTB.Text = MessageTB.Text.Insert(0, $"Rs232 {_comConfig.Comport}: Disconnected.\n");
                SpinWait.SpinUntil(() => false, 3000);
            }

            if (string.IsNullOrEmpty(Rs232CB.Text))
                return;
            #region Comport Init

            if (_comConnector == null)
            {
                _comConfig = new RS232ConnectorConfig(new RS232Config(Rs232CB.Text, 9600, 0, 8, 1));
                _comConnector = new Rs232Connector(_log, _comConfig) { Name = "Loadport1" };
            }
            else
            {
                _comConfig.Comport = Rs232CB.Text;
            }

            var hrResult = _comConnector.Connect();
            if (hrResult != null)
            {
                MessageTB.Text = MessageTB.Text.Insert(0, $"Rs232 {_comConfig.Comport}: Connecting fail.\n");
            }
            else
            {
                MessageTB.Text = MessageTB.Text.Insert(0, $"Rs232 {_comConfig.Comport}: Connected.\n");
            }

            #endregion Comport Init
        }

        private void SendCommandBtn_Click(object sender, EventArgs e)
        {
            var sendCommand = Encoding.UTF8.GetBytes(CommandText.Text);
            _loadportConnector.Send(sendCommand, sendCommand.Length);
        }
    }
}
