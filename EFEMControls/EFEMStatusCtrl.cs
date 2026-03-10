using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EFEM.ExceptionManagements;
using System.Threading;

namespace EFEM.GUIControls
{
    public partial class EFEMStatusCtrl : UserControl
    {
        private string _ProgressKey = string.Empty;

        public void InitAll()
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { InitAll(); };
                this.Invoke(del);
            }
            else
            {
                InitializeAlarm();
            }
        } 

        public EFEMStatusCtrl()
        {
            InitializeComponent();
        }

        public void ShowAlarmPage()
        {
            tcAlarm.SelectedTab = tabAlarm;
            efemAlarm.ShowActiveAlarmPage();
        }

        private void InitializeAlarm()
        {
            efemAlarm.InitAll();
        }

        private void tabAlarm_Resize(object sender, EventArgs e)
        {
            efemAlarm.ReLocateAll();
        }
    }
}
