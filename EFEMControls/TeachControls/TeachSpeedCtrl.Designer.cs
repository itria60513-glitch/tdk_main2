namespace EFEM.GUIControls.TeachControls
{
    partial class TeachSpeedCtrl
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
            this.gSpeed = new System.Windows.Forms.GroupBox();
            this.cbWaferSensingMode = new System.Windows.Forms.ComboBox();
            this.lWaferSensingMode = new System.Windows.Forms.Label();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.gRobot = new System.Windows.Forms.GroupBox();
            this.gEvacuation = new System.Windows.Forms.GroupBox();
            this.ckbEnableP = new System.Windows.Forms.CheckBox();
            this.ckbEnableG = new System.Windows.Forms.CheckBox();
            this.btnInfo_WfrSensingMde = new System.Windows.Forms.Button();
            this.btnInfo_RbtSpd = new System.Windows.Forms.Button();
            this.btnInfo_Zspd = new System.Windows.Forms.Button();
            this.btnInfo_PUTSspd = new System.Windows.Forms.Button();
            this.btnInfo_GETSspd = new System.Windows.Forms.Button();
            this.lnbSpeedR = new EFEM.GUIControls.LabeledNumBox();
            this.lnbEvacuationSpdZ = new EFEM.GUIControls.LabeledNumBox();
            this.lnbEvacuationSpdP = new EFEM.GUIControls.LabeledNumBox();
            this.lnbEvacuationSpdG = new EFEM.GUIControls.LabeledNumBox();
            this.gSpeed.SuspendLayout();
            this.gRobot.SuspendLayout();
            this.gEvacuation.SuspendLayout();
            this.SuspendLayout();
            // 
            // gSpeed
            // 
            this.gSpeed.Controls.Add(this.btnInfo_WfrSensingMde);
            this.gSpeed.Controls.Add(this.cbWaferSensingMode);
            this.gSpeed.Controls.Add(this.lWaferSensingMode);
            this.gSpeed.Controls.Add(this.btnRefresh);
            this.gSpeed.Controls.Add(this.btnApply);
            this.gSpeed.Controls.Add(this.gRobot);
            this.gSpeed.Controls.Add(this.gEvacuation);
            this.gSpeed.Location = new System.Drawing.Point(3, 3);
            this.gSpeed.Name = "gSpeed";
            this.gSpeed.Size = new System.Drawing.Size(178, 353);
            this.gSpeed.TabIndex = 18;
            this.gSpeed.TabStop = false;
            this.gSpeed.Text = "Configuration for Maintenance";
            // 
            // cbWaferSensingMode
            // 
            this.cbWaferSensingMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbWaferSensingMode.FormattingEnabled = true;
            this.cbWaferSensingMode.Location = new System.Drawing.Point(13, 42);
            this.cbWaferSensingMode.Name = "cbWaferSensingMode";
            this.cbWaferSensingMode.Size = new System.Drawing.Size(112, 21);
            this.cbWaferSensingMode.TabIndex = 22;
            // 
            // lWaferSensingMode
            // 
            this.lWaferSensingMode.AutoSize = true;
            this.lWaferSensingMode.Location = new System.Drawing.Point(17, 23);
            this.lWaferSensingMode.Name = "lWaferSensingMode";
            this.lWaferSensingMode.Size = new System.Drawing.Size(110, 13);
            this.lWaferSensingMode.TabIndex = 21;
            this.lWaferSensingMode.Text = "Wafer Sensing Mode ";
            // 
            // btnRefresh
            // 
            this.btnRefresh.Image = global::EFEM.GUIControls.Properties.Resources.refresh;
            this.btnRefresh.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnRefresh.Location = new System.Drawing.Point(10, 309);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(58, 40);
            this.btnRefresh.TabIndex = 20;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnApply
            // 
            this.btnApply.Image = global::EFEM.GUIControls.Properties.Resources.apply;
            this.btnApply.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnApply.Location = new System.Drawing.Point(99, 309);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(58, 40);
            this.btnApply.TabIndex = 20;
            this.btnApply.Text = "Apply ";
            this.btnApply.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // gRobot
            // 
            this.gRobot.Controls.Add(this.btnInfo_RbtSpd);
            this.gRobot.Controls.Add(this.lnbSpeedR);
            this.gRobot.Location = new System.Drawing.Point(6, 67);
            this.gRobot.Name = "gRobot";
            this.gRobot.Size = new System.Drawing.Size(166, 57);
            this.gRobot.TabIndex = 19;
            this.gRobot.TabStop = false;
            this.gRobot.Text = "Robot";
            // 
            // gEvacuation
            // 
            this.gEvacuation.Controls.Add(this.btnInfo_Zspd);
            this.gEvacuation.Controls.Add(this.ckbEnableP);
            this.gEvacuation.Controls.Add(this.lnbEvacuationSpdZ);
            this.gEvacuation.Controls.Add(this.ckbEnableG);
            this.gEvacuation.Controls.Add(this.btnInfo_PUTSspd);
            this.gEvacuation.Controls.Add(this.btnInfo_GETSspd);
            this.gEvacuation.Controls.Add(this.lnbEvacuationSpdP);
            this.gEvacuation.Controls.Add(this.lnbEvacuationSpdG);
            this.gEvacuation.Location = new System.Drawing.Point(6, 126);
            this.gEvacuation.Name = "gEvacuation";
            this.gEvacuation.Size = new System.Drawing.Size(166, 177);
            this.gEvacuation.TabIndex = 18;
            this.gEvacuation.TabStop = false;
            this.gEvacuation.Text = "Evacuation when failed";
            // 
            // ckbEnableP
            // 
            this.ckbEnableP.AutoSize = true;
            this.ckbEnableP.Location = new System.Drawing.Point(4, 76);
            this.ckbEnableP.Name = "ckbEnableP";
            this.ckbEnableP.Size = new System.Drawing.Size(102, 17);
            this.ckbEnableP.TabIndex = 32;
            this.ckbEnableP.Text = "Enable in PUTS";
            this.ckbEnableP.UseVisualStyleBackColor = true;
            // 
            // ckbEnableG
            // 
            this.ckbEnableG.AutoSize = true;
            this.ckbEnableG.Location = new System.Drawing.Point(5, 17);
            this.ckbEnableG.Name = "ckbEnableG";
            this.ckbEnableG.Size = new System.Drawing.Size(102, 17);
            this.ckbEnableG.TabIndex = 31;
            this.ckbEnableG.Text = "Enable in GETS";
            this.ckbEnableG.UseVisualStyleBackColor = true;
            // 
            // btnInfo_WfrSensingMde
            // 
            this.btnInfo_WfrSensingMde.AutoSize = true;
            this.btnInfo_WfrSensingMde.BackColor = System.Drawing.Color.Transparent;
            this.btnInfo_WfrSensingMde.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnInfo_WfrSensingMde.FlatAppearance.BorderSize = 0;
            this.btnInfo_WfrSensingMde.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnInfo_WfrSensingMde.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnInfo_WfrSensingMde.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInfo_WfrSensingMde.Image = global::EFEM.GUIControls.Properties.Resources.information;
            this.btnInfo_WfrSensingMde.Location = new System.Drawing.Point(131, 38);
            this.btnInfo_WfrSensingMde.Name = "btnInfo_WfrSensingMde";
            this.btnInfo_WfrSensingMde.Size = new System.Drawing.Size(26, 26);
            this.btnInfo_WfrSensingMde.TabIndex = 29;
            this.btnInfo_WfrSensingMde.UseVisualStyleBackColor = false;
            // 
            // btnInfo_RbtSpd
            // 
            this.btnInfo_RbtSpd.AutoSize = true;
            this.btnInfo_RbtSpd.BackColor = System.Drawing.Color.Transparent;
            this.btnInfo_RbtSpd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnInfo_RbtSpd.FlatAppearance.BorderSize = 0;
            this.btnInfo_RbtSpd.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnInfo_RbtSpd.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnInfo_RbtSpd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInfo_RbtSpd.Image = global::EFEM.GUIControls.Properties.Resources.information;
            this.btnInfo_RbtSpd.Location = new System.Drawing.Point(125, 24);
            this.btnInfo_RbtSpd.Name = "btnInfo_RbtSpd";
            this.btnInfo_RbtSpd.Size = new System.Drawing.Size(26, 26);
            this.btnInfo_RbtSpd.TabIndex = 28;
            this.btnInfo_RbtSpd.UseVisualStyleBackColor = false;
            // 
            // btnInfo_Zspd
            // 
            this.btnInfo_Zspd.AutoSize = true;
            this.btnInfo_Zspd.BackColor = System.Drawing.Color.Transparent;
            this.btnInfo_Zspd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnInfo_Zspd.FlatAppearance.BorderSize = 0;
            this.btnInfo_Zspd.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnInfo_Zspd.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnInfo_Zspd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInfo_Zspd.Image = global::EFEM.GUIControls.Properties.Resources.information;
            this.btnInfo_Zspd.Location = new System.Drawing.Point(125, 146);
            this.btnInfo_Zspd.Name = "btnInfo_Zspd";
            this.btnInfo_Zspd.Size = new System.Drawing.Size(26, 26);
            this.btnInfo_Zspd.TabIndex = 34;
            this.btnInfo_Zspd.UseVisualStyleBackColor = false;
            // 
            // btnInfo_PUTSspd
            // 
            this.btnInfo_PUTSspd.AutoSize = true;
            this.btnInfo_PUTSspd.BackColor = System.Drawing.Color.Transparent;
            this.btnInfo_PUTSspd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnInfo_PUTSspd.FlatAppearance.BorderSize = 0;
            this.btnInfo_PUTSspd.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnInfo_PUTSspd.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnInfo_PUTSspd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInfo_PUTSspd.Image = global::EFEM.GUIControls.Properties.Resources.information;
            this.btnInfo_PUTSspd.Location = new System.Drawing.Point(125, 72);
            this.btnInfo_PUTSspd.Name = "btnInfo_PUTSspd";
            this.btnInfo_PUTSspd.Size = new System.Drawing.Size(26, 26);
            this.btnInfo_PUTSspd.TabIndex = 30;
            this.btnInfo_PUTSspd.UseVisualStyleBackColor = false;
            // 
            // btnInfo_GETSspd
            // 
            this.btnInfo_GETSspd.AutoSize = true;
            this.btnInfo_GETSspd.BackColor = System.Drawing.Color.Transparent;
            this.btnInfo_GETSspd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnInfo_GETSspd.FlatAppearance.BorderSize = 0;
            this.btnInfo_GETSspd.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnInfo_GETSspd.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnInfo_GETSspd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInfo_GETSspd.Image = global::EFEM.GUIControls.Properties.Resources.information;
            this.btnInfo_GETSspd.Location = new System.Drawing.Point(125, 10);
            this.btnInfo_GETSspd.Name = "btnInfo_GETSspd";
            this.btnInfo_GETSspd.Size = new System.Drawing.Size(26, 26);
            this.btnInfo_GETSspd.TabIndex = 29;
            this.btnInfo_GETSspd.UseVisualStyleBackColor = false;
            // 
            // lnbSpeedR
            // 
            this.lnbSpeedR.Caption = "Motion Speed (%)";
            this.lnbSpeedR.DecimalPlaces = 2;
            this.lnbSpeedR.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.lnbSpeedR.Location = new System.Drawing.Point(7, 17);
            this.lnbSpeedR.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.lnbSpeedR.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.lnbSpeedR.Name = "lnbSpeedR";
            this.lnbSpeedR.Size = new System.Drawing.Size(112, 34);
            this.lnbSpeedR.TabIndex = 14;
            this.lnbSpeedR.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lnbEvacuationSpdZ
            // 
            this.lnbEvacuationSpdZ.Caption = "Z Eleveting Speed (%)";
            this.lnbEvacuationSpdZ.DecimalPlaces = 2;
            this.lnbEvacuationSpdZ.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.lnbEvacuationSpdZ.Location = new System.Drawing.Point(6, 138);
            this.lnbEvacuationSpdZ.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.lnbEvacuationSpdZ.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.lnbEvacuationSpdZ.Name = "lnbEvacuationSpdZ";
            this.lnbEvacuationSpdZ.Size = new System.Drawing.Size(113, 34);
            this.lnbEvacuationSpdZ.TabIndex = 33;
            this.lnbEvacuationSpdZ.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lnbEvacuationSpdP
            // 
            this.lnbEvacuationSpdP.Caption = "Speed (%)";
            this.lnbEvacuationSpdP.DecimalPlaces = 2;
            this.lnbEvacuationSpdP.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.lnbEvacuationSpdP.Location = new System.Drawing.Point(6, 98);
            this.lnbEvacuationSpdP.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.lnbEvacuationSpdP.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.lnbEvacuationSpdP.Name = "lnbEvacuationSpdP";
            this.lnbEvacuationSpdP.Size = new System.Drawing.Size(140, 34);
            this.lnbEvacuationSpdP.TabIndex = 10;
            this.lnbEvacuationSpdP.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lnbEvacuationSpdG
            // 
            this.lnbEvacuationSpdG.Caption = "Speed (%)";
            this.lnbEvacuationSpdG.DecimalPlaces = 2;
            this.lnbEvacuationSpdG.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.lnbEvacuationSpdG.Location = new System.Drawing.Point(6, 36);
            this.lnbEvacuationSpdG.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.lnbEvacuationSpdG.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.lnbEvacuationSpdG.Name = "lnbEvacuationSpdG";
            this.lnbEvacuationSpdG.Size = new System.Drawing.Size(140, 34);
            this.lnbEvacuationSpdG.TabIndex = 8;
            this.lnbEvacuationSpdG.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // TeachSpeedCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gSpeed);
            this.Name = "TeachSpeedCtrl";
            this.Size = new System.Drawing.Size(184, 359);
            this.gSpeed.ResumeLayout(false);
            this.gSpeed.PerformLayout();
            this.gRobot.ResumeLayout(false);
            this.gRobot.PerformLayout();
            this.gEvacuation.ResumeLayout(false);
            this.gEvacuation.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gSpeed;
        private System.Windows.Forms.GroupBox gRobot;
        private System.Windows.Forms.Button btnInfo_RbtSpd;
        private LabeledNumBox lnbSpeedR;
        private System.Windows.Forms.GroupBox gEvacuation;
        private System.Windows.Forms.CheckBox ckbEnableP;
        private System.Windows.Forms.CheckBox ckbEnableG;
        private System.Windows.Forms.Button btnInfo_PUTSspd;
        private System.Windows.Forms.Button btnInfo_GETSspd;
        private LabeledNumBox lnbEvacuationSpdP;
        private LabeledNumBox lnbEvacuationSpdG;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnInfo_Zspd;
        private LabeledNumBox lnbEvacuationSpdZ;
        private System.Windows.Forms.Button btnInfo_WfrSensingMde;
        private System.Windows.Forms.ComboBox cbWaferSensingMode;
        private System.Windows.Forms.Label lWaferSensingMode;

    }
}
