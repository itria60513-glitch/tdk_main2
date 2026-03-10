namespace EFEM.GUIControls
{
    partial class EFEMStatusCtrl
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
            this.tcAlarm = new System.Windows.Forms.TabControl();
            this.tabAlarm = new System.Windows.Forms.TabPage();
            this.efemAlarm = new EFEM.GUIControls.EFEMAlarm();
            this.tabControllerAlarm = new System.Windows.Forms.TabPage();
            this.gControllerAlarm = new System.Windows.Forms.GroupBox();
            this.robotAlarmFormCtrl1 = new EFEM.GUIControls.RobotAlarmFormCtrl();
            this.tabVersionCheck = new System.Windows.Forms.TabPage();
            this.panelStatus = new System.Windows.Forms.Panel();
            this.tcAlarm.SuspendLayout();
            this.tabAlarm.SuspendLayout();
            this.tabControllerAlarm.SuspendLayout();
            this.gControllerAlarm.SuspendLayout();
            this.tabVersionCheck.SuspendLayout();
            this.SuspendLayout();
            // 
            // tcAlarm
            // 
            this.tcAlarm.Controls.Add(this.tabAlarm);
            this.tcAlarm.Controls.Add(this.tabControllerAlarm);
            this.tcAlarm.Controls.Add(this.tabVersionCheck);
            this.tcAlarm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcAlarm.Location = new System.Drawing.Point(496, 0);
            this.tcAlarm.Name = "tcAlarm";
            this.tcAlarm.SelectedIndex = 0;
            this.tcAlarm.Size = new System.Drawing.Size(748, 837);
            this.tcAlarm.TabIndex = 1;
            // 
            // tabAlarm
            // 
            this.tabAlarm.Controls.Add(this.efemAlarm);
            this.tabAlarm.Location = new System.Drawing.Point(4, 22);
            this.tabAlarm.Name = "tabAlarm";
            this.tabAlarm.Size = new System.Drawing.Size(740, 811);
            this.tabAlarm.TabIndex = 2;
            this.tabAlarm.Text = "System Alarm";
            this.tabAlarm.UseVisualStyleBackColor = true;
            this.tabAlarm.Resize += new System.EventHandler(this.tabAlarm_Resize);
            // 
            // efemAlarm
            // 
            this.efemAlarm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.efemAlarm.Location = new System.Drawing.Point(0, 0);
            this.efemAlarm.Name = "efemAlarm";
            this.efemAlarm.Size = new System.Drawing.Size(740, 811);
            this.efemAlarm.TabIndex = 0;
            // 
            // tabControllerAlarm
            // 
            this.tabControllerAlarm.Controls.Add(this.gControllerAlarm);
            this.tabControllerAlarm.Location = new System.Drawing.Point(4, 22);
            this.tabControllerAlarm.Name = "tabControllerAlarm";
            this.tabControllerAlarm.Padding = new System.Windows.Forms.Padding(3);
            this.tabControllerAlarm.Size = new System.Drawing.Size(740, 811);
            this.tabControllerAlarm.TabIndex = 1;
            this.tabControllerAlarm.Text = "Controller Alarm";
            this.tabControllerAlarm.UseVisualStyleBackColor = true;
            // 
            // gControllerAlarm
            // 
            this.gControllerAlarm.Controls.Add(this.robotAlarmFormCtrl1);
            this.gControllerAlarm.Dock = System.Windows.Forms.DockStyle.Top;
            this.gControllerAlarm.Location = new System.Drawing.Point(3, 3);
            this.gControllerAlarm.Name = "gControllerAlarm";
            this.gControllerAlarm.Size = new System.Drawing.Size(734, 304);
            this.gControllerAlarm.TabIndex = 0;
            this.gControllerAlarm.TabStop = false;
            this.gControllerAlarm.Text = "Controller Alarm History";
            // 
            // robotAlarmFormCtrl1
            // 
            this.robotAlarmFormCtrl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.robotAlarmFormCtrl1.Location = new System.Drawing.Point(3, 16);
            this.robotAlarmFormCtrl1.Name = "robotAlarmFormCtrl1";
            this.robotAlarmFormCtrl1.Size = new System.Drawing.Size(728, 285);
            this.robotAlarmFormCtrl1.TabIndex = 0;
            // 
            // panelStatus
            // 
            this.panelStatus.AutoScroll = true;
            this.panelStatus.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelStatus.Location = new System.Drawing.Point(0, 0);
            this.panelStatus.Name = "panelStatus";
            this.panelStatus.Size = new System.Drawing.Size(496, 837);
            this.panelStatus.TabIndex = 16;
            // 
            // EFEMStatusCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.tcAlarm);
            this.Controls.Add(this.panelStatus);
            this.Name = "EFEMStatusCtrl";
            this.Size = new System.Drawing.Size(1244, 837);
            this.tcAlarm.ResumeLayout(false);
            this.tabAlarm.ResumeLayout(false);
            this.tabControllerAlarm.ResumeLayout(false);
            this.gControllerAlarm.ResumeLayout(false);
            this.tabVersionCheck.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public EFEMAlarm efemAlarm;
        private System.Windows.Forms.TabControl tcAlarm;
        private System.Windows.Forms.TabPage tabControllerAlarm;
        private System.Windows.Forms.TabPage tabAlarm;
        private RobotAlarmFormCtrl robotAlarmFormCtrl1;
        private System.Windows.Forms.Panel panelStatus;
        private System.Windows.Forms.GroupBox gControllerAlarm;
        private System.Windows.Forms.TabPage tabVersionCheck;

    }
}
