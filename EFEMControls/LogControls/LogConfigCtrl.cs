using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Threading;
using EFEM.DataCenter;
using EFEM.LogUtilities;

namespace EFEM.GUIControls
{
    public partial class LogConfigCtrl : UserControl
    {
        public LogConfigCtrl()
        {
            InitializeComponent();
        }

        public void InitAll()
        {
            LogLevel maxItem = ExtendMethods.GetMaxValue<LogLevel>();
            int iLevelCount = (int)maxItem;

            cbLogLevel.Items.Clear();

            for (int i = 0; i <= iLevelCount; i++)
            {
                cbLogLevel.Items.Add(ExtendMethods.ToStringHelper(((LogLevel)i)));
            }

            gLogBasic.Enabled = true;
            panelBtns.Enabled = true;

            RefreshSettings();
        }

        public void RefreshSettings()
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { RefreshSettings(); };
                this.Invoke(del);
            }
            else
            {
                cbLogLevel.SelectedIndex = (int)GUIBasic.Instance().Log.LogLevelMode;
                numLogKeepingDays.Value = (int)GUIBasic.Instance().Log.DaysForPerservingLog;
                numAutoFlushPeriod.Value = (int)GUIBasic.Instance().Log.AutoFlushTimerMinutes;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshSettings();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            EnableThis(false);

            if (GUIBasic.Instance().ShowMessageOnTop("Sure to update Log configurations?", "Action Confirm", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                int[] config = new int[3];
                config[0] = Convert.ToInt32(this.numAutoFlushPeriod.Value);
                config[1] = Convert.ToInt32(numLogKeepingDays.Value);
                config[2] = cbLogLevel.SelectedIndex;

                ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_UpdateLogConfig), config);
            }
            else
                EnableThis(true);
        }

        private void TPool_UpdateLogConfig(object obj)
        {
            int timeSec = 600;
            int dayKeep = 10;
            int logLevel = (int)LogLevel.ALL;

            try
            {
                int[] config = (int[])obj;

                timeSec = config[0];
                dayKeep = config[1];
                logLevel = config[2];

                XDocument xdoc = GUIBasic.Instance().Log.GetLogParamXDocument();

                IEnumerable<XNode> paraNodes =
                    from para in xdoc.Elements("LogParam").Nodes()
                    where para.NodeType == XmlNodeType.Element && ((XElement)para).Name.ToString().Equals("AutoFlushPeriod")
                    select para;
                if (paraNodes != null)
                {
                    XElement xe = (XElement)paraNodes.First();
                    xe.Value = timeSec.ToString();
                }
                else
                {
                    xdoc.Root.Add(new XElement("AutoFlushPeriod", timeSec));
                }


                paraNodes =
                        from para in xdoc.Elements("LogParam").Nodes()
                        where para.NodeType == XmlNodeType.Element && ((XElement)para).Name.ToString().Equals("LogDeleteBufferDays")
                        select para;
                if (paraNodes != null)
                {
                    XElement xe = (XElement)paraNodes.First();
                    xe.Value = dayKeep.ToString();
                }
                else
                {
                    xdoc.Root.Add(new XElement("LogDeleteBufferDays", dayKeep));
                }

                paraNodes =
                        from para in xdoc.Elements("LogParam").Nodes()
                        where para.NodeType == XmlNodeType.Element && ((XElement)para).Name.ToString().Equals("LevelControl")
                        select para;
                if (paraNodes != null)
                {
                    XElement xe = (XElement)paraNodes.First();
                    xe.Value = logLevel.ToString();
                }
                else
                {
                    xdoc.Root.Add(new XElement("LevelControl", logLevel));
                }

                GUIBasic.Instance().Log.SetLogParamXDocument(xdoc);
            }
            catch (Exception e)
            {

                GUIBasic.Instance().ShowMessageOnTop(e.Message, "Update Configuration Fail", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                EnableThis(true);
            }
        }


        private void EnableThis(bool enabled)
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { EnableThis(enabled); };
                this.Invoke(del);
            }
            else
            {
                this.Enabled = enabled;
            }
        }

        private void btnFlushLogs_Click(object sender, EventArgs e)
        {
            btnFlushLogs.Enabled = false;
            ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_FlushBuffers), null);
        }

        private void TPool_FlushBuffers(object obj)
        {
            try
            {
                GUIBasic.Instance().Log.ForceWrite();
            }
            finally
            {
                EnableFlushBtn();
            }

        }

        private void EnableFlushBtn()
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { EnableFlushBtn(); };
                this.Invoke(del);
            }
            else
            {
                btnFlushLogs.Enabled = true;
            }
        }

        private void btnCleanLogs_Click(object sender, EventArgs e)
        {
            string message = string.Format("Sure to delete the log files that are before {0} days?", GUIBasic.Instance().Log.DaysForPerservingLog);

            if (GUIBasic.Instance().ShowMessageOnTop(message, "Action Confirm",
                 MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                btnCleanLogs.Enabled = false;
                ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_ClearLogs), null);
            }
        }

        private void TPool_ClearLogs(object obj)
        {
            try
            {
                GUIBasic.Instance().Log.ClearLogs();
            }
            finally
            {
                EnableClearBtn();
            }
        }

        private void EnableClearBtn()
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { EnableClearBtn(); };
                this.Invoke(del);
            }
            else
            {
                btnCleanLogs.Enabled = true;
            }
        }
    }
}
