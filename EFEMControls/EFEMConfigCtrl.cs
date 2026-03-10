using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EFEM.GUIControls
{
    public partial class EFEMConfigCtrl : UserControl
    {
        public EFEMConfigCtrl()
        {
            InitializeComponent();
        }

        public void InitAll()
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { InitAll(); };
                this.Invoke(del);
            }
            else
            {
                logConfigCtrl1.InitAll();
            }
        }
    }
}
