namespace EFEM.GUIControls
{
    partial class LogConfigCtrl
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
            this.btnCleanLogs = new System.Windows.Forms.Button();
            this.btnFlushLogs = new System.Windows.Forms.Button();
            this.gLogBasic = new System.Windows.Forms.GroupBox();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.lDays = new System.Windows.Forms.Label();
            this.lMins = new System.Windows.Forms.Label();
            this.numAutoFlushPeriod = new System.Windows.Forms.NumericUpDown();
            this.numLogKeepingDays = new System.Windows.Forms.NumericUpDown();
            this.lAutoFlush = new System.Windows.Forms.Label();
            this.lLogLevel = new System.Windows.Forms.Label();
            this.lLogBufferDays = new System.Windows.Forms.Label();
            this.cbLogLevel = new System.Windows.Forms.ComboBox();
            this.panelBtns = new System.Windows.Forms.Panel();
            this.gLogBasic.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAutoFlushPeriod)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLogKeepingDays)).BeginInit();
            this.panelBtns.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCleanLogs
            // 
            this.btnCleanLogs.Location = new System.Drawing.Point(3, 46);
            this.btnCleanLogs.Name = "btnCleanLogs";
            this.btnCleanLogs.Size = new System.Drawing.Size(120, 34);
            this.btnCleanLogs.TabIndex = 1;
            this.btnCleanLogs.Text = "Clean Logs";
            this.btnCleanLogs.UseVisualStyleBackColor = true;
            this.btnCleanLogs.Click += new System.EventHandler(this.btnCleanLogs_Click);
            // 
            // btnFlushLogs
            // 
            this.btnFlushLogs.Location = new System.Drawing.Point(3, 7);
            this.btnFlushLogs.Name = "btnFlushLogs";
            this.btnFlushLogs.Size = new System.Drawing.Size(120, 34);
            this.btnFlushLogs.TabIndex = 0;
            this.btnFlushLogs.Text = "Flush Buffers to Files";
            this.btnFlushLogs.UseVisualStyleBackColor = true;
            this.btnFlushLogs.Click += new System.EventHandler(this.btnFlushLogs_Click);
            // 
            // gLogBasic
            // 
            this.gLogBasic.Controls.Add(this.btnApply);
            this.gLogBasic.Controls.Add(this.btnRefresh);
            this.gLogBasic.Controls.Add(this.lDays);
            this.gLogBasic.Controls.Add(this.lMins);
            this.gLogBasic.Controls.Add(this.numAutoFlushPeriod);
            this.gLogBasic.Controls.Add(this.numLogKeepingDays);
            this.gLogBasic.Controls.Add(this.lAutoFlush);
            this.gLogBasic.Controls.Add(this.lLogLevel);
            this.gLogBasic.Controls.Add(this.lLogBufferDays);
            this.gLogBasic.Controls.Add(this.cbLogLevel);
            this.gLogBasic.Enabled = false;
            this.gLogBasic.Location = new System.Drawing.Point(3, 3);
            this.gLogBasic.Name = "gLogBasic";
            this.gLogBasic.Size = new System.Drawing.Size(343, 117);
            this.gLogBasic.TabIndex = 0;
            this.gLogBasic.TabStop = false;
            this.gLogBasic.Text = "Basic Settings";
            // 
            // btnApply
            // 
            this.btnApply.Image = global::EFEM.GUIControls.Properties.Resources.apply;
            this.btnApply.Location = new System.Drawing.Point(250, 51);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(87, 31);
            this.btnApply.TabIndex = 9;
            this.btnApply.Text = "Apply";
            this.btnApply.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Image = global::EFEM.GUIControls.Properties.Resources.arrow_refresh;
            this.btnRefresh.Location = new System.Drawing.Point(250, 14);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(87, 31);
            this.btnRefresh.TabIndex = 8;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // lDays
            // 
            this.lDays.AutoSize = true;
            this.lDays.Location = new System.Drawing.Point(197, 59);
            this.lDays.Name = "lDays";
            this.lDays.Size = new System.Drawing.Size(31, 13);
            this.lDays.TabIndex = 4;
            this.lDays.Text = "Days";
            // 
            // lMins
            // 
            this.lMins.AutoSize = true;
            this.lMins.Location = new System.Drawing.Point(197, 85);
            this.lMins.Name = "lMins";
            this.lMins.Size = new System.Drawing.Size(44, 13);
            this.lMins.TabIndex = 7;
            this.lMins.Text = "Minutes";
            // 
            // numAutoFlushPeriod
            // 
            this.numAutoFlushPeriod.Location = new System.Drawing.Point(122, 83);
            this.numAutoFlushPeriod.Maximum = new decimal(new int[] {
            720,
            0,
            0,
            0});
            this.numAutoFlushPeriod.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numAutoFlushPeriod.Name = "numAutoFlushPeriod";
            this.numAutoFlushPeriod.Size = new System.Drawing.Size(69, 20);
            this.numAutoFlushPeriod.TabIndex = 6;
            this.numAutoFlushPeriod.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // numLogKeepingDays
            // 
            this.numLogKeepingDays.Location = new System.Drawing.Point(122, 57);
            this.numLogKeepingDays.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numLogKeepingDays.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numLogKeepingDays.Name = "numLogKeepingDays";
            this.numLogKeepingDays.Size = new System.Drawing.Size(69, 20);
            this.numLogKeepingDays.TabIndex = 3;
            this.numLogKeepingDays.Value = new decimal(new int[] {
            7,
            0,
            0,
            0});
            // 
            // lAutoFlush
            // 
            this.lAutoFlush.AutoSize = true;
            this.lAutoFlush.Location = new System.Drawing.Point(16, 85);
            this.lAutoFlush.Name = "lAutoFlush";
            this.lAutoFlush.Size = new System.Drawing.Size(93, 13);
            this.lAutoFlush.TabIndex = 5;
            this.lAutoFlush.Text = "Auto Flush Period:";
            // 
            // lLogLevel
            // 
            this.lLogLevel.AutoSize = true;
            this.lLogLevel.Location = new System.Drawing.Point(16, 29);
            this.lLogLevel.Name = "lLogLevel";
            this.lLogLevel.Size = new System.Drawing.Size(57, 13);
            this.lLogLevel.TabIndex = 0;
            this.lLogLevel.Text = "Log Level:";
            // 
            // lLogBufferDays
            // 
            this.lLogBufferDays.AutoSize = true;
            this.lLogBufferDays.Location = new System.Drawing.Point(16, 59);
            this.lLogBufferDays.Name = "lLogBufferDays";
            this.lLogBufferDays.Size = new System.Drawing.Size(97, 13);
            this.lLogBufferDays.TabIndex = 2;
            this.lLogBufferDays.Text = "Log Keeping Days:";
            // 
            // cbLogLevel
            // 
            this.cbLogLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbLogLevel.FormattingEnabled = true;
            this.cbLogLevel.Location = new System.Drawing.Point(79, 26);
            this.cbLogLevel.Name = "cbLogLevel";
            this.cbLogLevel.Size = new System.Drawing.Size(112, 21);
            this.cbLogLevel.TabIndex = 1;
            // 
            // panelBtns
            // 
            this.panelBtns.Controls.Add(this.btnFlushLogs);
            this.panelBtns.Controls.Add(this.btnCleanLogs);
            this.panelBtns.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelBtns.Enabled = false;
            this.panelBtns.Location = new System.Drawing.Point(397, 0);
            this.panelBtns.Name = "panelBtns";
            this.panelBtns.Size = new System.Drawing.Size(126, 125);
            this.panelBtns.TabIndex = 1;
            // 
            // LogConfigCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelBtns);
            this.Controls.Add(this.gLogBasic);
            this.DoubleBuffered = true;
            this.Name = "LogConfigCtrl";
            this.Size = new System.Drawing.Size(523, 125);
            this.gLogBasic.ResumeLayout(false);
            this.gLogBasic.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAutoFlushPeriod)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLogKeepingDays)).EndInit();
            this.panelBtns.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCleanLogs;
        private System.Windows.Forms.Button btnFlushLogs;
        private System.Windows.Forms.GroupBox gLogBasic;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Label lDays;
        private System.Windows.Forms.Label lMins;
        private System.Windows.Forms.NumericUpDown numAutoFlushPeriod;
        private System.Windows.Forms.NumericUpDown numLogKeepingDays;
        private System.Windows.Forms.Label lAutoFlush;
        private System.Windows.Forms.Label lLogLevel;
        private System.Windows.Forms.Label lLogBufferDays;
        private System.Windows.Forms.ComboBox cbLogLevel;
        private System.Windows.Forms.Panel panelBtns;
        private System.Windows.Forms.Button btnApply;
    }
}
