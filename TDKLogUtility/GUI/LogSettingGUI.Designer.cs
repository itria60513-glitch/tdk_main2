namespace TDKLogUtility.GUI
{
    partial class LogSettingGUI
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.gComponent = new System.Windows.Forms.GroupBox();
            this.txt_bufferSize = new System.Windows.Forms.TextBox();
            this.lBufferSize = new System.Windows.Forms.Label();
            this.lMB = new System.Windows.Forms.Label();
            this.txt_FlushPeriod = new System.Windows.Forms.TextBox();
            this.lMaxStorage = new System.Windows.Forms.Label();
            this.lMinutes = new System.Windows.Forms.Label();
            this.lDays = new System.Windows.Forms.Label();
            this.txt_LogDirectory = new System.Windows.Forms.TextBox();
            this.txt_MaxStorage = new System.Windows.Forms.TextBox();
            this.txt_KeepingDays = new System.Windows.Forms.TextBox();
            this.lFlushPeriod = new System.Windows.Forms.Label();
            this.lKeepingDays = new System.Windows.Forms.Label();
            this.lLogDirectory = new System.Windows.Forms.Label();
            this.btn_Apply = new System.Windows.Forms.Button();
            this.button_default_setting = new System.Windows.Forms.Button();
            this.button_save = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.gComponent.SuspendLayout();
            this.SuspendLayout();
            // 
            // gComponent
            // 
            this.gComponent.Controls.Add(this.label1);
            this.gComponent.Controls.Add(this.txt_bufferSize);
            this.gComponent.Controls.Add(this.lBufferSize);
            this.gComponent.Controls.Add(this.lMB);
            this.gComponent.Controls.Add(this.txt_FlushPeriod);
            this.gComponent.Controls.Add(this.lMaxStorage);
            this.gComponent.Controls.Add(this.lMinutes);
            this.gComponent.Controls.Add(this.lDays);
            this.gComponent.Controls.Add(this.txt_LogDirectory);
            this.gComponent.Controls.Add(this.txt_MaxStorage);
            this.gComponent.Controls.Add(this.txt_KeepingDays);
            this.gComponent.Controls.Add(this.lFlushPeriod);
            this.gComponent.Controls.Add(this.lKeepingDays);
            this.gComponent.Controls.Add(this.lLogDirectory);
            this.gComponent.Dock = System.Windows.Forms.DockStyle.Top;
            this.gComponent.Location = new System.Drawing.Point(0, 0);
            this.gComponent.Name = "gComponent";
            this.gComponent.Size = new System.Drawing.Size(320, 275);
            this.gComponent.TabIndex = 0;
            this.gComponent.TabStop = false;
            this.gComponent.Text = "Log Setting";
            // 
            // txt_bufferSize
            // 
            this.txt_bufferSize.Location = new System.Drawing.Point(167, 230);
            this.txt_bufferSize.Name = "txt_bufferSize";
            this.txt_bufferSize.Size = new System.Drawing.Size(67, 20);
            this.txt_bufferSize.TabIndex = 12;
            this.txt_bufferSize.TextChanged += new System.EventHandler(this.TextBox_ValueChanged);
            // 
            // lBufferSize
            // 
            this.lBufferSize.Location = new System.Drawing.Point(20, 233);
            this.lBufferSize.Name = "lBufferSize";
            this.lBufferSize.Size = new System.Drawing.Size(114, 34);
            this.lBufferSize.TabIndex = 11;
            this.lBufferSize.Text = "BufferSize";
            this.lBufferSize.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lMB
            // 
            this.lMB.Location = new System.Drawing.Point(254, 180);
            this.lMB.Name = "lMB";
            this.lMB.Size = new System.Drawing.Size(51, 20);
            this.lMB.TabIndex = 10;
            this.lMB.Text = "MB";
            // 
            // txt_FlushPeriod
            // 
            this.txt_FlushPeriod.Location = new System.Drawing.Point(167, 130);
            this.txt_FlushPeriod.Name = "txt_FlushPeriod";
            this.txt_FlushPeriod.Size = new System.Drawing.Size(67, 20);
            this.txt_FlushPeriod.TabIndex = 6;
            this.txt_FlushPeriod.TextChanged += new System.EventHandler(this.TextBox_ValueChanged);
            // 
            // lMaxStorage
            // 
            this.lMaxStorage.Location = new System.Drawing.Point(20, 183);
            this.lMaxStorage.Name = "lMaxStorage";
            this.lMaxStorage.Size = new System.Drawing.Size(114, 34);
            this.lMaxStorage.TabIndex = 8;
            this.lMaxStorage.Text = "Directory Max Storage";
            this.lMaxStorage.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lMinutes
            // 
            this.lMinutes.Location = new System.Drawing.Point(254, 130);
            this.lMinutes.Name = "lMinutes";
            this.lMinutes.Size = new System.Drawing.Size(51, 20);
            this.lMinutes.TabIndex = 7;
            this.lMinutes.Text = "Minutes";
            // 
            // lDays
            // 
            this.lDays.Location = new System.Drawing.Point(254, 80);
            this.lDays.Name = "lDays";
            this.lDays.Size = new System.Drawing.Size(32, 20);
            this.lDays.TabIndex = 4;
            this.lDays.Text = "Days";
            // 
            // txt_LogDirectory
            // 
            this.txt_LogDirectory.Location = new System.Drawing.Point(167, 30);
            this.txt_LogDirectory.Name = "txt_LogDirectory";
            this.txt_LogDirectory.Size = new System.Drawing.Size(89, 20);
            this.txt_LogDirectory.TabIndex = 1;
            this.txt_LogDirectory.TextChanged += new System.EventHandler(this.TextBox_ValueChanged);
            // 
            // txt_MaxStorage
            // 
            this.txt_MaxStorage.Location = new System.Drawing.Point(167, 180);
            this.txt_MaxStorage.Name = "txt_MaxStorage";
            this.txt_MaxStorage.Size = new System.Drawing.Size(67, 20);
            this.txt_MaxStorage.TabIndex = 9;
            this.txt_MaxStorage.TextChanged += new System.EventHandler(this.TextBox_ValueChanged);
            // 
            // txt_KeepingDays
            // 
            this.txt_KeepingDays.Location = new System.Drawing.Point(167, 80);
            this.txt_KeepingDays.Name = "txt_KeepingDays";
            this.txt_KeepingDays.Size = new System.Drawing.Size(67, 20);
            this.txt_KeepingDays.TabIndex = 3;
            this.txt_KeepingDays.TextChanged += new System.EventHandler(this.TextBox_ValueChanged);
            // 
            // lFlushPeriod
            // 
            this.lFlushPeriod.Location = new System.Drawing.Point(36, 133);
            this.lFlushPeriod.Name = "lFlushPeriod";
            this.lFlushPeriod.Size = new System.Drawing.Size(98, 34);
            this.lFlushPeriod.TabIndex = 5;
            this.lFlushPeriod.Text = "Auto Flush Period";
            this.lFlushPeriod.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lKeepingDays
            // 
            this.lKeepingDays.Location = new System.Drawing.Point(36, 83);
            this.lKeepingDays.Name = "lKeepingDays";
            this.lKeepingDays.Size = new System.Drawing.Size(98, 34);
            this.lKeepingDays.TabIndex = 2;
            this.lKeepingDays.Text = "Log Keeping Days";
            this.lKeepingDays.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lLogDirectory
            // 
            this.lLogDirectory.Location = new System.Drawing.Point(36, 33);
            this.lLogDirectory.Name = "lLogDirectory";
            this.lLogDirectory.Size = new System.Drawing.Size(98, 34);
            this.lLogDirectory.TabIndex = 0;
            this.lLogDirectory.Text = "Log Main Directory";
            this.lLogDirectory.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // btn_Apply
            // 
            this.btn_Apply.Location = new System.Drawing.Point(8, 293);
            this.btn_Apply.Name = "btn_Apply";
            this.btn_Apply.Size = new System.Drawing.Size(75, 23);
            this.btn_Apply.TabIndex = 1;
            this.btn_Apply.Text = "Apply";
            this.btn_Apply.UseVisualStyleBackColor = true;
            this.btn_Apply.Click += new System.EventHandler(this.btn_Apply_Click);
            // 
            // button_default_setting
            // 
            this.button_default_setting.Location = new System.Drawing.Point(191, 293);
            this.button_default_setting.Name = "button_default_setting";
            this.button_default_setting.Size = new System.Drawing.Size(126, 23);
            this.button_default_setting.TabIndex = 3;
            this.button_default_setting.Text = "Reset to Default Value";
            this.button_default_setting.UseVisualStyleBackColor = true;
            this.button_default_setting.Click += new System.EventHandler(this.button_default_setting_Click);
            // 
            // button_save
            // 
            this.button_save.Location = new System.Drawing.Point(100, 293);
            this.button_save.Name = "button_save";
            this.button_save.Size = new System.Drawing.Size(75, 23);
            this.button_save.TabIndex = 2;
            this.button_save.Text = "Save";
            this.button_save.UseVisualStyleBackColor = true;
            this.button_save.Click += new System.EventHandler(this.button_save_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(254, 230);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 20);
            this.label1.TabIndex = 13;
            this.label1.Text = "MB";
            // 
            // LogSettingGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btn_Apply);
            this.Controls.Add(this.gComponent);
            this.Controls.Add(this.button_default_setting);
            this.Controls.Add(this.button_save);
            this.Name = "LogSettingGUI";
            this.Size = new System.Drawing.Size(320, 330);
            this.gComponent.ResumeLayout(false);
            this.gComponent.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gComponent;
        private System.Windows.Forms.Label lFlushPeriod;
        private System.Windows.Forms.Label lKeepingDays;
        private System.Windows.Forms.Label lLogDirectory;
        private System.Windows.Forms.Label lMinutes;
        private System.Windows.Forms.Label lDays;
        private System.Windows.Forms.TextBox txt_LogDirectory;
        private System.Windows.Forms.TextBox txt_MaxStorage;
        private System.Windows.Forms.TextBox txt_KeepingDays;
        private System.Windows.Forms.Label lMB;
        private System.Windows.Forms.TextBox txt_FlushPeriod;
        private System.Windows.Forms.Label lMaxStorage;
        private System.Windows.Forms.Button btn_Apply;
        private System.Windows.Forms.Button button_default_setting;
        private System.Windows.Forms.Button button_save;
        private System.Windows.Forms.TextBox txt_bufferSize;
        private System.Windows.Forms.Label lBufferSize;
        private System.Windows.Forms.Label label1;
    }
}