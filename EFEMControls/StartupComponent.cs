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
using EFEMInterface;
using EFEM.ExceptionManagements;
using EFEM.DataCenter;

namespace EFEM.GUIControls
{
    public partial class StartupComponent : UserControl
    {
        private AbstractProduct _hostProduct = null;
        public StartupComponent()
        {
            InitializeComponent();
        }

        public HRESULT StartupAll(AbstractProduct hostProduct)
        {
            if (hostProduct == null)
                throw new ApplicationException("Invalid Host Product Assigned");
            else
            {
                _hostProduct = hostProduct;
                ThreadPool.QueueUserWorkItem(new WaitCallback(TPOOL_StartupAll), _hostProduct);
                return null;
            }
        }

        private void UpdateStatus(string currentStatus)
        {
            if (lCurrentStatus.InvokeRequired)
            {
                MethodInvoker del = delegate { UpdateStatus(currentStatus); };
                lCurrentStatus.Invoke(del);
            }
            else
                lCurrentStatus.Text = currentStatus;
        }

        private void TPOOL_StartupAll(object para)
        {
            try
            {
                AbstractHost _host = para as AbstractHost;

                UpdateStatus("InstantiateObjects");
                GUIBasic.Instance().VariableCenter.SetValueAndFireCallback("CurrentStatusMessage", "InstantiateObjects");
                ArrayList rst = _host.InstantiateObjects();
                if (rst != null && rst.Count > 0)
                    MessageBox.Show("Error occurs during InstantiateObjects.");
                Thread.Sleep(2000);

                UpdateStatus("EstablishCommunications");
                GUIBasic.Instance().VariableCenter.SetValueAndFireCallback("CurrentStatusMessage", "EstablishCommunications");
                rst = _host.EstablishCommunications();
                if (rst != null && rst.Count > 0)
                {
                    string test = ExtendMethods.ToStringHelper(rst);
                    MessageBox.Show("Error occurs during EstablishCommunications.");
                }
                Thread.Sleep(2000);

                UpdateStatus("DownloadParameters");
                GUIBasic.Instance().VariableCenter.SetValueAndFireCallback("CurrentStatusMessage", "DownloadParameters");
                rst = _host.DownloadParameters();
                if (rst != null && rst.Count > 0)
                    MessageBox.Show("Error occurs during DownloadParameters.");
                Thread.Sleep(2000);

                UpdateStatus("Initialize");
                GUIBasic.Instance().VariableCenter.SetValueAndFireCallback("CurrentStatusMessage", "Initialize");
                rst = _host.Initialize();
                if (rst != null && rst.Count > 0)
                    MessageBox.Show("Error occurs during Initialize.");

                UpdateStatus("Finish");
                GUIBasic.Instance().VariableCenter.SetValueAndFireCallback("CurrentStatusMessage", "AllStartupFinish");
            }
            catch (Exception e)
            {

            }
        }
    }
}
