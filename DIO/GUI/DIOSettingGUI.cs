using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using DIO.GUI.ViewModels;
using EFEM.FileUtilities;

namespace DIO.GUI
{
    public partial class DIOSettingGUI : UserControl
    {
        private readonly DIOSettingGuiViewModel _viewModel;
        private readonly SynchronizationContext _ctx;
        AbstractFileUtilities _fu = FileUtility.GetUniqueInstance();
        private string _select = string.Empty;

        public DIOSettingGUI(string select)
        {
            InitializeComponent();
            _viewModel = new DIOSettingGuiViewModel(_ctx);
            _select = select;
            DataBinding();

            DIOParamSetting(_select);


        }

        private void DIOParamSetting(string select)
        {
            bool isValid = true;
            DIOConfig dioSetting = _fu.GetDIOConfigSetting(select);
            _viewModel.Type = dioSetting.Type;
            _viewModel.Index = dioSetting.Index.ToString();
            _viewModel.DIMaxPort = dioSetting.MaxDIPort;
            _viewModel.DOMaxPort = dioSetting.MaxDOPort;
            _viewModel.PINCountPerPort = dioSetting.PinCountPerPort;

        }

        private void button_save_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Do you want to save current settings?", "DIO Setting", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                DIOConfig dio = new DIOConfig(_viewModel.Type, int.Parse(_viewModel.Index), _viewModel.DOMaxPort, _viewModel.DIMaxPort, _viewModel.PINCountPerPort);
                _fu.DIOSave(_select, dio);
                MessageBox.Show("Save Success.", "DIO Setting", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
        }

        private void button_default_setting_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Do you want to set to default value?", "DIO Setting", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                _fu.ResetToDefaultValue("DIO", _select);
                DIOParamSetting(_select);
                cmb_Type.Refresh();
                cmb_Index.Refresh();
                tb_DIPortMax.Refresh();
                tb_DOPortMax.Refresh();
                tb_PinCountPerPort.Refresh();
                MessageBox.Show("Reset to Default Value Success.", "DIO Setting", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }


        private void DataBinding()
        {
            _viewModel.Type = 0;
            cmb_Type.DataBindings.Add(nameof(ComboBox.SelectedIndex), _viewModel, nameof(_viewModel.Type));

            _viewModel.Index = string.Empty;
            cmb_Index.DataBindings.Add(nameof(ComboBox.SelectedItem), _viewModel, nameof(_viewModel.Index));

            _viewModel.DIMaxPort = 0;
            tb_DIPortMax.DataBindings.Add(nameof(TextBox.Text), _viewModel, nameof(_viewModel.DIMaxPort));

            _viewModel.DOMaxPort = 0;
            tb_DOPortMax.DataBindings.Add(nameof(TextBox.Text), _viewModel, nameof(_viewModel.DOMaxPort));

            _viewModel.PINCountPerPort = 0;
            tb_PinCountPerPort.DataBindings.Add(nameof(TextBox.Text), _viewModel, nameof(_viewModel.PINCountPerPort));

        }

        private void btn_Apply_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Do you want to apply current settings?", "DIO Setting", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                DIOConfig dio = new DIOConfig(_viewModel.Type, int.Parse(_viewModel.Index), _viewModel.DOMaxPort, _viewModel.DIMaxPort, _viewModel.PINCountPerPort);
                _fu.DIOConfigApply(_select, dio);
                MessageBox.Show("Apply Success.", "DIO Setting", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #region ValidationCheck
        private void ButtonValidationCheck()
        {
            int diMaxval = 0;
            int doMaxval = 0;
            if (   cmb_Index.SelectedItem != null 
                && cmb_Type.SelectedItem != null
                && int.TryParse(tb_DIPortMax.Text, out diMaxval) 
                && int.TryParse(tb_DOPortMax.Text, out doMaxval))
            {
                if (diMaxval >= 0 && doMaxval >= 0)
                {
                    btn_Apply.Enabled = true;
                    button_save.Enabled = true;
                }
                else
                {
                    btn_Apply.Enabled = false;
                    button_save.Enabled = false;
                }
            }
            else
            {
                btn_Apply.Enabled = false;
                button_save.Enabled = false;
            }
        }

        private void TextBox_ValueChanged(object sender, EventArgs e)
        {
            ButtonValidationCheck();
        }

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ButtonValidationCheck();
        }


        #endregion

    }
}
