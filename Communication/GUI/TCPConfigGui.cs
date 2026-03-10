using Communication.GUI.ViewModels;
using EFEM.FileUtilities;
using System;
using System.Threading;
using System.Windows.Forms;

namespace Communication.GUI
{
    public partial class TCPIPSettingGUI : UserControl
    {
        private readonly TCPConfigGuiViewModel _viewModel;
        private readonly SynchronizationContext _ctx;
        AbstractFileUtilities _fu = FileUtility.GetUniqueInstance();
        string _select = string.Empty;
        private bool _isIPValid = true;
        private bool _isPortValid = true;
        public TCPIPSettingGUI(string select)
        {
            InitializeComponent();
            _viewModel = new TCPConfigGuiViewModel(_ctx);
            DataBinding();
            _select = select;
            tcpParamSetting(_select);

        }

        private void tcpParamSetting(string select)
        {
            bool isValid = true;
            TCPConfig tcpSetting = _fu.GetTCPSetting(select);
            _viewModel.IpAddress = tcpSetting.Ip;
            _viewModel.Port = tcpSetting.Port;
            isValid = CommunicationTCPValidationCheck(TCPAddressEdit.Text, "IPAddress");
            SaveButtonEnableCheck("IPAddress", isValid);
            isValid = CommunicationTCPValidationCheck(TCPPortEdit.Text, "Port");
            SaveButtonEnableCheck("Port", isValid);

        }

        private void button_save_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Do you want to save current settings?", "TCP Setting", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                _fu.TCPConfigSave(_select, _viewModel.IpAddress, _viewModel.Port);
                MessageBox.Show("Save Success.", "TCP Setting", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
        }

        private void btn_Apply_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Do you want to apply current settings?", "TCP Setting", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                _fu.TCPConfigApply(_select, _viewModel.IpAddress, _viewModel.Port);
                MessageBox.Show("Apply Success.", "TCP Setting", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button_default_setting_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Do you want to set to default value?", "TCP Setting", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                _fu.ResetToDefaultValue("TCPIP", _select);
                tcpParamSetting(_select);
                TCPAddressEdit.Refresh();
                TCPPortEdit.Refresh();

                MessageBox.Show("Reset to Default Value Success.", "TCP Setting", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void DataBinding()
        {
            _viewModel.IpAddress = TCPAddressEdit.Text;
            TCPAddressEdit.DataBindings.Add(nameof(TextBox.Text), _viewModel, nameof(_viewModel.IpAddress));

            _viewModel.Port = TCPPortEdit.Text;
            TCPPortEdit.DataBindings.Add(nameof(TextBox.Text), _viewModel, nameof(_viewModel.Port));
        }

        #region ValidationCheck
        public bool CommunicationTCPValidationCheck(string value, string type)
        {
            bool isValid = true;
            switch (type)
            {
                case "Port":
                    if (string.IsNullOrWhiteSpace(value))
                        isValid = false;
                    else
                    {
                        try
                        {
                            int port = Convert.ToInt32(value);
                            if (port <= 0 || port > 65535)
                                isValid = false;
                        }
                        catch
                        {
                            isValid = false;
                        }
                    }

                    _isPortValid = isValid;
                    break;

                case "IPAddress":
                    try
                    {
                        var parts = value.Split('.');
                        if (parts.Length != 4)
                        {
                            isValid = false;
                        }
                        else
                        {
                            foreach (var part in parts)
                            {
                                if (part.Length == 0)
                                {
                                    isValid = false;
                                    break;
                                }

                                int ip = Convert.ToInt32(part);
                                if (ip < 0 || ip > 255)
                                {
                                    isValid = false;
                                    break;
                                }

                            }
                        }
                    }
                    catch
                    {
                        isValid = false;
                    }

                    _isIPValid = isValid;
                    break;

            }

            return isValid;
        }

        public void SaveButtonEnableCheck(string type, bool isValid)
        {
            //switch (type)
            //{
            //    case "Port":
            //        if (isValid)
            //        {
            //            lPort.ForeColor = Color.Black;
            //            lPort.Text = "Port";
            //        }
            //        else
            //        {
            //            lPort.ForeColor = Color.Red;
            //            lPort.Text = "Port (Invalid)";
            //        }

            //        break;

            //    case "IPAddress":
            //        if (isValid)
            //        {
            //            lAddress.ForeColor = Color.Black;
            //            lAddress.Text = "IP Address";
            //        }
            //        else
            //        {
            //            lAddress.ForeColor = Color.Red;
            //            lAddress.Text = "IP Address (Invalid)";
            //        }

            //        break;
            //}

            if (_isIPValid && _isPortValid)
            {
                button_save.Enabled = true;
                btn_Apply.Enabled = true;
            }
            else
            {
                button_save.Enabled = false;
                btn_Apply.Enabled = false;
            }
        }

        #endregion

        private void TCPAddressEdit_ValueChanged(object sender, EventArgs e)
        {
            bool isValid = true;
            isValid = CommunicationTCPValidationCheck(TCPAddressEdit.Text, "IPAddress");
            SaveButtonEnableCheck("IPAddress", isValid);
        }

        private void TCPPortEdit_ValueChanged(object sender, EventArgs e)
        {
            bool isValid = true;
            isValid = CommunicationTCPValidationCheck(TCPPortEdit.Text, "Port");
            SaveButtonEnableCheck("Port", isValid);
        }
    }
}
