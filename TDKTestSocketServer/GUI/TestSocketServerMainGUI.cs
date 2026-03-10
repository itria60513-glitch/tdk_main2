using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Communication.Protocol;
using TDKTestSocketServer.Config;
using TDKTestSocketServer.Module;

namespace TDKTestSocketServer
{
    public partial class TestSocketServerMainGUI : Form
    {
        private Dictionary<string, TDKTestServerConfig> configDict = new Dictionary<string, TDKTestServerConfig>();
        private List<SocketTestServer> serverList = new List<SocketTestServer>();
        private readonly string xmlPath = @"D:\TDKConfig\TDKTestServerConfig.xml";
        public TestSocketServerMainGUI()
        {
            InitializeComponent();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void AppendStatusMessage(string message)
        {
            if (configDict.ContainsKey(ServerNameCB.Text))
            {
                configDict[ServerNameCB.Text].StatusMessage = $"{configDict[ServerNameCB.Text].StatusMessage.Insert(0, $"{message}")}\n";
            }
            StatusText.Text = StatusText.Text.Insert(0, $"{message}\n");
        }

        private void AppendStatusMessage(string serverName,string message)
        {
            var config = configDict.Where(x => x.Key == serverName)
                                   .Select(x=>x.Value)
                                   .FirstOrDefault();

            if (config != null)
            {
                config.StatusMessage = message;
            }
        }


        private void TestSocketServerMainGUI_Load(object sender, EventArgs e)
        {
            #region Read from xml file and storage to dictionary
            var loadportConfig = TDKTestServerConfigService.ReadFromFileAsync(xmlPath,"Loadport").Result;
            configDict.Add("Loadport",loadportConfig);
            #region Assign Dictionary to gui
            ServerNameCB.Items.Clear();

            foreach (var configName in configDict.Keys)
            {
                ServerNameCB.Items.Add(configName);
            }
            if (ServerNameCB.Items.Count > 0)
            {
                ServerNameCB.SelectedIndex = 0;
            }

            #endregion Assign Dictionary to gui
            #endregion Read from xml file and storage to dictionary
            #region Init socket server
            var server = new SocketTestServer("Loadport", configDict["Loadport"], new LoadportProtocol());
            server.ShowMessageEventHandler += ServerOnShowMessageEventHandler;
            server.Start();
            serverList.Add(server);
            #endregion Init socket server
        }

        private void ServerOnShowMessageEventHandler(string serverName, string message)
        {

            if (InvokeRequired)
            {

                BeginInvoke(new Action(() =>
                    ServerOnShowMessageEventHandler(serverName, message)
                ));


                return;
            }

            if (ServerNameCB.Text == serverName)
            {
                AppendStatusMessage(message);
            }
            else
            {
                AppendStatusMessage(serverName, message);
            }
        }

        private void ServerNameCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            SettingDG.Rows.Clear();
            if (configDict.ContainsKey(ServerNameCB.Text))
            {
                foreach (var cmd in configDict[ServerNameCB.Text].Command)
                {
                    SettingDG.Rows.Add(cmd.Item1, cmd.Item2);
                }
            }

            StatusText.Text = configDict[ServerNameCB.Text].StatusMessage;
        }

        private void TestSocketServerMainGUI_FormClosed(object sender, FormClosedEventArgs e)
        {
            serverList.ForEach(x=>x.Stop());
        }

        private void ReOpenBtn_Click(object sender, EventArgs e)
        {
            var server = serverList[ServerNameCB.SelectedIndex];

            if (server != null)
            {
                server.Stop();
                SpinWait.SpinUntil(() => false, 3000);
                server.Start();
            }
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            var config = configDict[ServerNameCB.Text];

            if (config != null)
            {
                config.Command.Clear();
                foreach (DataGridViewRow row in SettingDG.Rows)
                {

                    string request = row.Cells["Request"].Value?.ToString();
                    string response = row.Cells["Response"].Value?.ToString();

                    if (string.IsNullOrWhiteSpace(request))
                        continue;

                    config.Command.Add(Tuple.Create(request, response ?? string.Empty));
                }

                if (ServerNameCB.Text == "Loadport")
                {
                    TDKTestServerConfigService.SaveLoadport(xmlPath, config);
                }
            }

        }
    }
}
