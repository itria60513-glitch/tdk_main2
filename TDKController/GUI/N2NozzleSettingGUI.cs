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
    public partial class N2NozzleSettingGUI : UserControl
    {
        private readonly N2NozzleConfigGuiViewModel _viewModel;
        private readonly SynchronizationContext _ctx;
        AbstractFileUtilities _fu = FileUtility.GetUniqueInstance();
        private List<(string,int)> _commList = new List<(string,int)>();
        private string _select = string.Empty;
        public N2NozzleSettingGUI(string select)
        {
            InitializeComponent();
            _ctx = SynchronizationContext.Current;
            _viewModel = new N2NozzleConfigGuiViewModel(_ctx);
            _commList = _fu.GetCommList();
            BindComboBox(cmb_N2NComm, _commList);
            DataBinding();
            _select = select;
            N2NozzleSetting(_select);

        }

        private void BindComboBox(ComboBox cmb, List<(string, int)> commList)
        {
            cmb.BeginUpdate();
            try
            {
                cmb.Items.Clear();
                foreach(var item in commList)
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
            _viewModel.Comm = string.Empty;
            cmb_N2NComm.DataBindings.Add(nameof(ComboBox.SelectedItem), _viewModel, nameof(_viewModel.Comm));

        }

        private void btn_Apply_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Do you want to apply current settings?", "N2Nozzle Setting", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                N2NozzleConfig n2Nozzle = new N2NozzleConfig(_viewModel.Comm);
                _fu.N2NozzleConfigApply(_select, n2Nozzle);
                MessageBox.Show("Apply Success.", "N2Nozzle Setting", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
        }

        private void button_save_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Do you want to save current settings?", "N2Nozzle Setting", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                N2NozzleConfig n2Nozzle = new N2NozzleConfig(_viewModel.Comm);
                _fu.N2NozzleSave(_select, n2Nozzle);
                MessageBox.Show("Save Success.", "N2Nozzle Setting", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
        }

        private void button_default_setting_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Do you want to set to default value?", "N2Nozzle Setting", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                _fu.ResetToDefaultValue("N2Nozzle", _select);
                N2NozzleSetting(_select);
                cmb_N2NComm.Refresh();

                MessageBox.Show("Reset to Default Value Success.", "N2Nozzle Setting", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void N2NozzleSetting(string select)
        {
            N2NozzleConfig n2Nozzle = _fu.GetN2NozzleConfigSetting(select);
            _viewModel.Comm = n2Nozzle.Comm;
            ButtonValidationCheck();

        }

        private void ButtonValidationCheck()
        {
            if (cmb_N2NComm.SelectedItem != null)
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

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ButtonValidationCheck();
        }
    }
}
