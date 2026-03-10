using LogUtility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TDKController.GUI.ViewModels;
using TDKLogUtility.Module_Config;

namespace TDKLogUtility.GUI
{
    public partial class LogSettingGUI : UserControl
    {
        private readonly LogSettingConfigViewModel _viewModel;
        private readonly SynchronizationContext _ctx;
        private LogUtilityClient log = null;
        public LogSettingGUI()
        {
            log = new LogUtilityClient();
            InitializeComponent();
            _viewModel = new LogSettingConfigViewModel(_ctx);
            DataBinding();
            LogSetting();
        }

        private void DataBinding()
        {
            _viewModel.MainDirectory = txt_LogDirectory.Text;
            txt_LogDirectory.DataBindings.Add(nameof(TextBox.Text), _viewModel, nameof(_viewModel.MainDirectory));

            _viewModel.BufferSize = 0;
            txt_bufferSize.DataBindings.Add(nameof(TextBox.Text), _viewModel, nameof(_viewModel.BufferSize));

            _viewModel.AutoFlushPeriod = 0;
            txt_FlushPeriod.DataBindings.Add(nameof(TextBox.Text), _viewModel, nameof(_viewModel.AutoFlushPeriod));

            _viewModel.LogKeepingDays = 0;
            txt_KeepingDays.DataBindings.Add(nameof(TextBox.Text), _viewModel, nameof(_viewModel.LogKeepingDays));

            _viewModel.MaxStorage = 0;
            txt_MaxStorage.DataBindings.Add(nameof(TextBox.Text), _viewModel, nameof(_viewModel.MaxStorage));

        }

        private void btn_Apply_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Do you want to apply current settings?", "Log Setting", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                log.ApplyLogSetting(_viewModel.MainDirectory, _viewModel.LogKeepingDays, _viewModel.AutoFlushPeriod);
                log.ApplyLogInfo(_viewModel.MaxStorage, _viewModel.BufferSize);
                MessageBox.Show("Apply Success.", "Log Setting", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
        }

        private void button_save_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Do you want to Save current settings?", "Log Setting", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                log.UpdateLogSetting(_viewModel.MainDirectory, _viewModel.LogKeepingDays, _viewModel.AutoFlushPeriod);
                log.UpdateLogInfo(_viewModel.MaxStorage, _viewModel.BufferSize);
                MessageBox.Show("Save Success.", "Log Setting", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
        }

        private void button_default_setting_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Do you want to set to default value?", "Log Setting", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                log.ResetLogParams();
                LogSetting();
                txt_LogDirectory.Refresh();
                txt_FlushPeriod.Refresh();
                txt_KeepingDays.Refresh();
                txt_MaxStorage.Refresh();
                txt_bufferSize.Refresh();

                MessageBox.Show("Reset to Default Value Success.", "Log Setting", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void LogSetting()
        {
            _viewModel.AutoFlushPeriod = log.AutoFlushTimerMinutes;
            _viewModel.BufferSize = log.BufferSize;
            _viewModel.LogKeepingDays = log.DaysForPerservingLog;
            _viewModel.MainDirectory = log.MainDirectory;
            _viewModel.MaxStorage = log.MaxLogSize;
        }

        private void ButtonValidationCheck()
        {
            int flush = 0;
            int keepDays = 0;
            int maxStorage = 0;
            int buffer = 0;
            if (    txt_LogDirectory.Text != null 
                 && int.TryParse(txt_FlushPeriod.Text, out flush)
                 && int.TryParse(txt_KeepingDays.Text, out keepDays)
                 && int.TryParse(txt_MaxStorage.Text, out maxStorage)
                 && int.TryParse(txt_bufferSize.Text, out buffer))
            {
                if (flush >= 0 && keepDays >= 0 && maxStorage >= 0 && buffer >= 0)
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
    }
}
