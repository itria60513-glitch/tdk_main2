using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EFEM.DataCenter;
using EFEM.FileUtilities;
using Communication.GUI.ViewModels;
using System.Threading;

namespace Communication.GUI
{
    public partial class RS232SettingGUI : UserControl
    {
        private readonly RS232ConfigGuiViewModel _viewModel;
        private readonly SynchronizationContext _ctx;
        AbstractFileUtilities _fu = FileUtility.GetUniqueInstance();
        private string _select = string.Empty;
        public RS232SettingGUI(string select)
        {
            InitializeComponent();
            _viewModel = new RS232ConfigGuiViewModel(_ctx);
            DataBinding();
            _select = select;

            SerialParamSetting(_select);


        }

        private void SerialParamSetting(string select)
        {
            RS232Config serialSetting = _fu.GetSerialSetting(select);
            _viewModel.BaudRate = serialSetting.Baud.ToString();
            _viewModel.DataBits = serialSetting.DataBits.ToString();
            _viewModel.Parity = serialSetting.Parity;
            _viewModel.PortNumber = serialSetting.Port;
            _viewModel.StopBits = serialSetting.StopBits.ToString();

            SerialButtonValidationCheck();
        }

        private void button_save_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Do you want to save current settings?", "Serial Setting", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                string port = _viewModel.PortNumber;
                int baud = int.Parse(_viewModel.BaudRate);
                int parity = _viewModel.Parity;
                int databits = int.Parse(_viewModel.DataBits);
                int stopbits = int.Parse(_viewModel.StopBits);
                _fu.SerialPortConfigSave(_select, port, baud, parity, databits, stopbits);
                MessageBox.Show("Save Success.", "Serial Setting", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
        }

        private void btn_Apply_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Do you want to apply current settings?", "Serial Setting", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                string port = _viewModel.PortNumber;
                int baud = int.Parse(_viewModel.BaudRate);
                int parity = _viewModel.Parity;
                int databits = int.Parse(_viewModel.DataBits);
                int stopbits = int.Parse(_viewModel.StopBits);
                _fu.SerialConfigApply(_select, port, baud, parity, databits, stopbits);
                MessageBox.Show("Apply Success.", "Serial Setting", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button_default_setting_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Do you want to set to default value?", "Serial Setting", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                _fu.ResetToDefaultValue("RS232", _select);
                SerialParamSetting(_select);
                ComPortCombo.Refresh();
                BaudRateCombo.Refresh();
                ParityCombo.Refresh();
                DataBitCombo.Refresh();
                StopBitCombo.Refresh();
                MessageBox.Show("Reset to Default Value Success.", "Serial Setting", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }


        private void DataBinding()
        {
            _viewModel.PortNumber = string.Empty;
            ComPortCombo.DataBindings.Add(nameof(ComboBox.SelectedItem), _viewModel, nameof(_viewModel.PortNumber));

            _viewModel.BaudRate = string.Empty;
            BaudRateCombo.DataBindings.Add(nameof(ComboBox.SelectedItem), _viewModel, nameof(_viewModel.BaudRate));

            _viewModel.StopBits = string.Empty;
            StopBitCombo.DataBindings.Add(nameof(ComboBox.SelectedItem), _viewModel, nameof(_viewModel.StopBits));

            _viewModel.DataBits = string.Empty;
            DataBitCombo.DataBindings.Add(nameof(ComboBox.SelectedItem), _viewModel, nameof(_viewModel.DataBits));

            _viewModel.Parity = ParityCombo.SelectedIndex;
            ParityCombo.DataBindings.Add(nameof(ComboBox.SelectedIndex), _viewModel, nameof(_viewModel.Parity));


        }

        #region ConnectionCheck
        private void SerialButtonValidationCheck()
        {
            if (   ComPortCombo.Text == string.Empty
                || BaudRateCombo.Text == string.Empty
                || StopBitCombo.Text == string.Empty
                || DataBitCombo.Text == string.Empty
                || ParityCombo.Text == string.Empty)
            {
                btn_Apply.Enabled = false;
                button_save.Enabled = false;
            }
            else
            {
                btn_Apply.Enabled = true;
                button_save.Enabled = true;
            }
        }

        private void ConnectionCheck(Label lbl, ComboBox cb)
        {
            if (cb.SelectedIndex >= 0)
            {
                lbl.ForeColor = Color.Black;
                lbl.Text = lbl.Name;
                btn_Apply.Enabled = true;
                button_save.Enabled = true;
            }
            else
            {
                lbl.ForeColor = Color.Red;
                lbl.Text = lbl.Name + " (Invalid)";
                btn_Apply.Enabled = false;
                button_save.Enabled = false;
            }

        }


        #endregion

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SerialButtonValidationCheck();
        }
    }
}
