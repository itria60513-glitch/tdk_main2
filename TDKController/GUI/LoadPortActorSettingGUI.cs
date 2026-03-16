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
using EFEM.FileUtilities;
using TDKController.GUI.ViewModels;

namespace TDKController.GUI
{
    public partial class LoadPortSettingGUI : UserControl
    {
        private readonly LoadPortActorConfigGuiViewModel _viewModel;
        private readonly SynchronizationContext _ctx;
        AbstractFileUtilities _fu = FileUtility.GetUniqueInstance();
        private List<(string, int)> _commList = new List<(string, int)>();
        private string _select = string.Empty;
        public LoadPortSettingGUI(string select)
        {
            InitializeComponent();
            _ctx = SynchronizationContext.Current;
            _viewModel = new LoadPortActorConfigGuiViewModel(_ctx);
            _commList = _fu.GetCommList();
            BindComboBox(cmb_LPActor, _commList);
            DataBinding();
            _select = select;
            LoadportActorSetting(_select);
        }

        private void BindComboBox(ComboBox cmb, List<(string, int)> commList)
        {
            cmb.BeginUpdate();
            try
            {
                cmb.Items.Clear();
                foreach (var item in commList)
                {
                    cmb.Items.Add(item.Item1);
                }
            }
            finally
            {
                cmb.EndUpdate();
            }
        }

        private void DataBinding()
        {
            _viewModel.Comm = cmb_LPActor.Text;
            cmb_LPActor.DataBindings.Add(nameof(ComboBox.SelectedItem), _viewModel, nameof(_viewModel.Comm));

            _viewModel.ACKTimeouts = 0;
            txt_ACKTimeout.DataBindings.Add(nameof(TextBox.Text), _viewModel, nameof(_viewModel.ACKTimeouts));

            _viewModel.INFTimeouts = 0;
            txt_INFTimeout.DataBindings.Add(nameof(TextBox.Text), _viewModel, nameof(_viewModel.INFTimeouts));

        }

        private void button_save_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Do you want to save current settings?", "LoadPort Setting", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                LoadPortConfig lpa = new LoadPortConfig(_viewModel.Comm, _viewModel.ACKTimeouts, _viewModel.INFTimeouts);
                _fu.LoadPortActorSave(_select, lpa);
                MessageBox.Show("Save Success.", "LoadPort Setting", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
        }

        private void btn_Apply_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Do you want to apply current settings?", "LoadPort Setting", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                LoadPortConfig lpa = new LoadPortConfig(_viewModel.Comm, _viewModel.ACKTimeouts, _viewModel.INFTimeouts);
                _fu.LoadPortConfigApply(_select, lpa);
                MessageBox.Show("Apply Success.", "LoadPort Setting", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
        }

        private void button_default_setting_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Do you want to set to default value?", "LoadPort Setting", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                _fu.ResetToDefaultValue("LoadPort", _select);
                LoadportActorSetting(_select);
                cmb_LPActor.Refresh();
                txt_INFTimeout.Refresh();
                txt_ACKTimeout.Refresh();

                MessageBox.Show("Reset to Default Value Success.", "LoadPort Setting", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void LoadportActorSetting(string select)
        {
            LoadPortConfig loadPortSetting = _fu.GetLoadPortConfigSetting(select);
            _viewModel.Comm = loadPortSetting.Comm;
            _viewModel.ACKTimeouts = loadPortSetting.ACKTimeout;
            _viewModel.INFTimeouts = loadPortSetting.INFTimeout;
            ButtonValidationCheck();

        }

        private void ButtonValidationCheck()
        {
            int ackval = 0;
            int absval = 0;
            if (   cmb_LPActor.SelectedItem != null
                && int.TryParse(txt_INFTimeout.Text, out absval)
                && int.TryParse(txt_ACKTimeout.Text, out ackval))
            {
                if (ackval >= 0 && absval >= 0)
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
    }
}
