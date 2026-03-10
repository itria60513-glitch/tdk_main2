using LogUtility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace EFEMGUI
{
    public partial class TestLogForm : Form
    {
        LogUtilityClient log = null;
        private TestDiLog test = null;
        public TestLogForm()
        {
            InitializeComponent();
        }

        private void TestLogForm_Load(object sender, EventArgs e)
        {
            log = new LogUtilityClient();
            tb_MainDirectory.Text = log.MainDirectory;
            tb_BufferSize.Text = log.BufferSize.ToString();
            tb_FlushPeriod.Text = log.AutoFlushTimerMinutes.ToString();
            tb_KeepDays.Text = log.DaysForPerservingLog.ToString();
            tb_MaxLogSize.Text = log.MaxLogSize.ToString();


            test = new TestDiLog(log);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            log.WriteLog("TDK", tb_LogMessage.Text);
            test.WriteLog(tb_LogMessage.Text);
            tb_LogMessage.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var mainDirectory = tb_MainDirectory.Text;
            int buffer = int.TryParse(tb_BufferSize.Text, out var b) ? b : log.BufferSize;
            int flushPeriod = int.TryParse(tb_FlushPeriod.Text, out var f) ? f : log.AutoFlushTimerMinutes;
            int keepDays = int.TryParse(tb_KeepDays.Text, out var k) ? k : log.DaysForPerservingLog;
            int maxLogSize = int.TryParse(tb_MaxLogSize.Text, out var m) ? m : log.MaxLogSize;
            log.UpdateLogSetting(mainDirectory, keepDays, flushPeriod);
            log.UpdateLogInfo(maxLogSize, buffer);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            log.ResetLogParams();
            tb_MainDirectory.Text = log.MainDirectory;
            tb_BufferSize.Text = log.BufferSize.ToString();
            tb_FlushPeriod.Text = log.AutoFlushTimerMinutes.ToString();
            tb_KeepDays.Text = log.DaysForPerservingLog.ToString();
            tb_MaxLogSize.Text = log.MaxLogSize.ToString();
        }
        
    }
}
