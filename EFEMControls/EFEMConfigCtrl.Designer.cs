namespace EFEM.GUIControls
{
    partial class EFEMConfigCtrl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabLogSetting = new System.Windows.Forms.TabPage();
            this.gLogUtility = new System.Windows.Forms.GroupBox();
            this.logConfigCtrl1 = new EFEM.GUIControls.LogConfigCtrl();
            this.logClientSetting1 = new EFEM.GUIControls.LogClientSetting();
            this.tabCommSettings = new System.Windows.Forms.TabPage();
            this.tabCtrlConfiguration = new System.Windows.Forms.TabControl();
            this.gCommunication = new System.Windows.Forms.GroupBox();
            this.tabLogSetting.SuspendLayout();
            this.gLogUtility.SuspendLayout();
            this.tabCommSettings.SuspendLayout();
            this.tabCtrlConfiguration.SuspendLayout();
            this.gCommunication.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabLogSetting
            // 
            this.tabLogSetting.AutoScroll = true;
            this.tabLogSetting.Controls.Add(this.gLogUtility);
            this.tabLogSetting.Location = new System.Drawing.Point(4, 22);
            this.tabLogSetting.Name = "tabLogSetting";
            this.tabLogSetting.Padding = new System.Windows.Forms.Padding(3);
            this.tabLogSetting.Size = new System.Drawing.Size(1241, 766);
            this.tabLogSetting.TabIndex = 3;
            this.tabLogSetting.Text = "Log Utility";
            this.tabLogSetting.UseVisualStyleBackColor = true;
            // 
            // gLogUtility
            // 
            this.gLogUtility.Controls.Add(this.logClientSetting1);
            this.gLogUtility.Controls.Add(this.logConfigCtrl1);
            this.gLogUtility.Location = new System.Drawing.Point(6, 6);
            this.gLogUtility.Name = "gLogUtility";
            this.gLogUtility.Size = new System.Drawing.Size(501, 580);
            this.gLogUtility.TabIndex = 2;
            this.gLogUtility.TabStop = false;
            this.gLogUtility.Text = "Log Utility";
            // 
            // logConfigCtrl1
            // 
            this.logConfigCtrl1.Location = new System.Drawing.Point(10, 15);
            this.logConfigCtrl1.Name = "logConfigCtrl1";
            this.logConfigCtrl1.Size = new System.Drawing.Size(486, 125);
            this.logConfigCtrl1.TabIndex = 3;
            // 
            // logClientSetting1
            // 
            this.logClientSetting1.Location = new System.Drawing.Point(10, 146);
            this.logClientSetting1.Name = "logClientSetting1";
            this.logClientSetting1.Size = new System.Drawing.Size(348, 424);
            this.logClientSetting1.TabIndex = 3;
            // 
            // tabCommSettings
            // 
            this.tabCommSettings.AutoScroll = true;
            this.tabCommSettings.Controls.Add(this.gCommunication);
            this.tabCommSettings.Location = new System.Drawing.Point(4, 22);
            this.tabCommSettings.Name = "tabCommSettings";
            this.tabCommSettings.Padding = new System.Windows.Forms.Padding(3);
            this.tabCommSettings.Size = new System.Drawing.Size(1241, 766);
            this.tabCommSettings.TabIndex = 2;
            this.tabCommSettings.Text = "Communication";
            this.tabCommSettings.UseVisualStyleBackColor = true;
            // 
            // tabCtrlConfiguration
            // 
            this.tabCtrlConfiguration.Controls.Add(this.tabCommSettings);
            this.tabCtrlConfiguration.Controls.Add(this.tabLogSetting);
            this.tabCtrlConfiguration.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabCtrlConfiguration.Location = new System.Drawing.Point(0, 0);
            this.tabCtrlConfiguration.Name = "tabCtrlConfiguration";
            this.tabCtrlConfiguration.SelectedIndex = 0;
            this.tabCtrlConfiguration.Size = new System.Drawing.Size(1249, 792);
            this.tabCtrlConfiguration.TabIndex = 0;
            // 
            // gCommunication
            // 
            this.gCommunication.Location = new System.Drawing.Point(6, 6);
            this.gCommunication.Name = "gCommunication";
            this.gCommunication.Size = new System.Drawing.Size(588, 300);
            this.gCommunication.TabIndex = 0;
            this.gCommunication.TabStop = false;
            this.gCommunication.Text = "Basic Communication Settings";
            // 
            // EFEMConfigCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.tabCtrlConfiguration);
            this.DoubleBuffered = true;
            this.Name = "EFEMConfigCtrl";
            this.Size = new System.Drawing.Size(1249, 792);
            this.tabLogSetting.ResumeLayout(false);
            this.gLogUtility.ResumeLayout(false);
            this.tabCommSettings.ResumeLayout(false);
            this.tabCtrlConfiguration.ResumeLayout(false);
            this.gCommunication.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage tabLogSetting;
        private System.Windows.Forms.GroupBox gLogUtility;
        private LogClientSetting logClientSetting1;
        private LogConfigCtrl logConfigCtrl1;
        private System.Windows.Forms.TabPage tabCommSettings;
        private System.Windows.Forms.GroupBox gCommunication;
        private System.Windows.Forms.TabControl tabCtrlConfiguration;
    }
}
