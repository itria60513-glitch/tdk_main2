namespace EFEM.GUIControls.TeachControls
{
    partial class TeachMotionConditionCtrl
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
            this.gMotion = new System.Windows.Forms.GroupBox();
            this.pMotionIllustration = new System.Windows.Forms.Panel();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.tMotion = new System.Windows.Forms.TabControl();
            this.tGetwafer = new System.Windows.Forms.TabPage();
            this.lnbGOFF = new EFEM.GUIControls.LabeledNumBox();
            this.lnbGCNF = new EFEM.GUIControls.LabeledNumBox();
            this.tPutwafer = new System.Windows.Forms.TabPage();
            this.btnInfo_PCNF = new System.Windows.Forms.Button();
            this.btnInfo_POFF = new System.Windows.Forms.Button();
            this.btnInfo_PADJ = new System.Windows.Forms.Button();
            this.lnbPCNF = new EFEM.GUIControls.LabeledNumBox();
            this.lnbPADJ = new EFEM.GUIControls.LabeledNumBox();
            this.lnbPOFF = new EFEM.GUIControls.LabeledNumBox();
            this.btnInfo_LOFF = new System.Windows.Forms.Button();
            this.lnbLOFF = new EFEM.GUIControls.LabeledNumBox();
            this.lnbUOFF = new EFEM.GUIControls.LabeledNumBox();
            this.btnInfo_UOFF = new System.Windows.Forms.Button();
            this.btnInfo_GCNF = new System.Windows.Forms.Button();
            this.btnInfo_GOFF = new System.Windows.Forms.Button();
            this.gMotion.SuspendLayout();
            this.pMotionIllustration.SuspendLayout();
            this.tMotion.SuspendLayout();
            this.tGetwafer.SuspendLayout();
            this.tPutwafer.SuspendLayout();
            this.SuspendLayout();
            // 
            // gMotion
            // 
            this.gMotion.Controls.Add(this.pMotionIllustration);
            this.gMotion.Controls.Add(this.tMotion);
            this.gMotion.Controls.Add(this.btnInfo_LOFF);
            this.gMotion.Controls.Add(this.lnbLOFF);
            this.gMotion.Controls.Add(this.lnbUOFF);
            this.gMotion.Controls.Add(this.btnInfo_UOFF);
            this.gMotion.Location = new System.Drawing.Point(3, 3);
            this.gMotion.Name = "gMotion";
            this.gMotion.Size = new System.Drawing.Size(493, 275);
            this.gMotion.TabIndex = 14;
            this.gMotion.TabStop = false;
            this.gMotion.Text = "Motion Control Condition";
            // 
            // pMotionIllustration
            // 
            this.pMotionIllustration.BackgroundImage = global::EFEM.GUIControls.Properties.Resources.Gets_motion;
            this.pMotionIllustration.Controls.Add(this.btnRefresh);
            this.pMotionIllustration.Controls.Add(this.btnApply);
            this.pMotionIllustration.Location = new System.Drawing.Point(188, 12);
            this.pMotionIllustration.Name = "pMotionIllustration";
            this.pMotionIllustration.Size = new System.Drawing.Size(299, 257);
            this.pMotionIllustration.TabIndex = 1;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Image = global::EFEM.GUIControls.Properties.Resources.refresh;
            this.btnRefresh.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnRefresh.Location = new System.Drawing.Point(14, 215);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(58, 40);
            this.btnRefresh.TabIndex = 22;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnApply
            // 
            this.btnApply.Image = global::EFEM.GUIControls.Properties.Resources.apply;
            this.btnApply.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnApply.Location = new System.Drawing.Point(228, 215);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(58, 40);
            this.btnApply.TabIndex = 21;
            this.btnApply.Text = "Apply ";
            this.btnApply.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // tMotion
            // 
            this.tMotion.Controls.Add(this.tGetwafer);
            this.tMotion.Controls.Add(this.tPutwafer);
            this.tMotion.Location = new System.Drawing.Point(6, 108);
            this.tMotion.Name = "tMotion";
            this.tMotion.SelectedIndex = 0;
            this.tMotion.Size = new System.Drawing.Size(163, 154);
            this.tMotion.TabIndex = 0;
            this.tMotion.SelectedIndexChanged += new System.EventHandler(this.tMotion_SelectedIndexChanged);
            // 
            // tGetwafer
            // 
            this.tGetwafer.Controls.Add(this.lnbGOFF);
            this.tGetwafer.Controls.Add(this.lnbGCNF);
            this.tGetwafer.Location = new System.Drawing.Point(4, 22);
            this.tGetwafer.Name = "tGetwafer";
            this.tGetwafer.Padding = new System.Windows.Forms.Padding(3);
            this.tGetwafer.Size = new System.Drawing.Size(155, 128);
            this.tGetwafer.TabIndex = 0;
            this.tGetwafer.Text = "Get Wafer";
            this.tGetwafer.UseVisualStyleBackColor = true;
            // 
            // lnbGOFF
            // 
            this.lnbGOFF.Caption = "GOFF [mm] (0~15)";
            this.lnbGOFF.DecimalPlaces = 0;
            this.lnbGOFF.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.lnbGOFF.Location = new System.Drawing.Point(6, 6);
            this.lnbGOFF.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.lnbGOFF.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.lnbGOFF.Name = "lnbGOFF";
            this.lnbGOFF.Size = new System.Drawing.Size(108, 34);
            this.lnbGOFF.TabIndex = 18;
            this.lnbGOFF.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // lnbGCNF
            // 
            this.lnbGCNF.Caption = "GCNF [mm] (0~15)";
            this.lnbGCNF.DecimalPlaces = 0;
            this.lnbGCNF.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.lnbGCNF.Location = new System.Drawing.Point(6, 55);
            this.lnbGCNF.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.lnbGCNF.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.lnbGCNF.Name = "lnbGCNF";
            this.lnbGCNF.Size = new System.Drawing.Size(108, 34);
            this.lnbGCNF.TabIndex = 17;
            this.lnbGCNF.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // tPutwafer
            // 
            this.tPutwafer.Controls.Add(this.btnInfo_PCNF);
            this.tPutwafer.Controls.Add(this.btnInfo_POFF);
            this.tPutwafer.Controls.Add(this.btnInfo_PADJ);
            this.tPutwafer.Controls.Add(this.lnbPCNF);
            this.tPutwafer.Controls.Add(this.lnbPADJ);
            this.tPutwafer.Controls.Add(this.lnbPOFF);
            this.tPutwafer.Location = new System.Drawing.Point(4, 22);
            this.tPutwafer.Name = "tPutwafer";
            this.tPutwafer.Padding = new System.Windows.Forms.Padding(3);
            this.tPutwafer.Size = new System.Drawing.Size(155, 128);
            this.tPutwafer.TabIndex = 1;
            this.tPutwafer.Text = "Put Wafer";
            this.tPutwafer.UseVisualStyleBackColor = true;
            // 
            // btnInfo_PCNF
            // 
            this.btnInfo_PCNF.AutoSize = true;
            this.btnInfo_PCNF.BackColor = System.Drawing.Color.Transparent;
            this.btnInfo_PCNF.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnInfo_PCNF.FlatAppearance.BorderSize = 0;
            this.btnInfo_PCNF.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnInfo_PCNF.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnInfo_PCNF.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInfo_PCNF.Image = global::EFEM.GUIControls.Properties.Resources.information;
            this.btnInfo_PCNF.Location = new System.Drawing.Point(121, 97);
            this.btnInfo_PCNF.Name = "btnInfo_PCNF";
            this.btnInfo_PCNF.Size = new System.Drawing.Size(26, 26);
            this.btnInfo_PCNF.TabIndex = 27;
            this.btnInfo_PCNF.UseVisualStyleBackColor = false;
            // 
            // btnInfo_POFF
            // 
            this.btnInfo_POFF.AutoSize = true;
            this.btnInfo_POFF.BackColor = System.Drawing.Color.Transparent;
            this.btnInfo_POFF.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnInfo_POFF.FlatAppearance.BorderSize = 0;
            this.btnInfo_POFF.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnInfo_POFF.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnInfo_POFF.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInfo_POFF.Image = global::EFEM.GUIControls.Properties.Resources.information;
            this.btnInfo_POFF.Location = new System.Drawing.Point(121, 59);
            this.btnInfo_POFF.Name = "btnInfo_POFF";
            this.btnInfo_POFF.Size = new System.Drawing.Size(26, 26);
            this.btnInfo_POFF.TabIndex = 26;
            this.btnInfo_POFF.UseVisualStyleBackColor = false;
            // 
            // btnInfo_PADJ
            // 
            this.btnInfo_PADJ.AutoSize = true;
            this.btnInfo_PADJ.BackColor = System.Drawing.Color.Transparent;
            this.btnInfo_PADJ.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnInfo_PADJ.FlatAppearance.BorderSize = 0;
            this.btnInfo_PADJ.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnInfo_PADJ.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnInfo_PADJ.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInfo_PADJ.Image = global::EFEM.GUIControls.Properties.Resources.information;
            this.btnInfo_PADJ.Location = new System.Drawing.Point(121, 14);
            this.btnInfo_PADJ.Name = "btnInfo_PADJ";
            this.btnInfo_PADJ.Size = new System.Drawing.Size(26, 26);
            this.btnInfo_PADJ.TabIndex = 25;
            this.btnInfo_PADJ.UseVisualStyleBackColor = false;
            // 
            // lnbPCNF
            // 
            this.lnbPCNF.Caption = "PCNF [mm]  (0~15)";
            this.lnbPCNF.DecimalPlaces = 0;
            this.lnbPCNF.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.lnbPCNF.Location = new System.Drawing.Point(6, 89);
            this.lnbPCNF.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.lnbPCNF.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.lnbPCNF.Name = "lnbPCNF";
            this.lnbPCNF.Size = new System.Drawing.Size(108, 34);
            this.lnbPCNF.TabIndex = 23;
            this.lnbPCNF.Value = new decimal(new int[] {
            7,
            0,
            0,
            0});
            // 
            // lnbPADJ
            // 
            this.lnbPADJ.Caption = "PADJ [mm]  (0~15)";
            this.lnbPADJ.DecimalPlaces = 0;
            this.lnbPADJ.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.lnbPADJ.Location = new System.Drawing.Point(6, 6);
            this.lnbPADJ.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.lnbPADJ.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.lnbPADJ.Name = "lnbPADJ";
            this.lnbPADJ.Size = new System.Drawing.Size(108, 34);
            this.lnbPADJ.TabIndex = 22;
            this.lnbPADJ.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // lnbPOFF
            // 
            this.lnbPOFF.Caption = "POFF [mm]  (0~15)";
            this.lnbPOFF.DecimalPlaces = 0;
            this.lnbPOFF.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.lnbPOFF.Location = new System.Drawing.Point(6, 47);
            this.lnbPOFF.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.lnbPOFF.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.lnbPOFF.Name = "lnbPOFF";
            this.lnbPOFF.Size = new System.Drawing.Size(108, 34);
            this.lnbPOFF.TabIndex = 21;
            this.lnbPOFF.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // btnInfo_LOFF
            // 
            this.btnInfo_LOFF.AutoSize = true;
            this.btnInfo_LOFF.BackColor = System.Drawing.Color.Transparent;
            this.btnInfo_LOFF.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnInfo_LOFF.FlatAppearance.BorderSize = 0;
            this.btnInfo_LOFF.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnInfo_LOFF.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnInfo_LOFF.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInfo_LOFF.Image = global::EFEM.GUIControls.Properties.Resources.information;
            this.btnInfo_LOFF.Location = new System.Drawing.Point(130, 76);
            this.btnInfo_LOFF.Name = "btnInfo_LOFF";
            this.btnInfo_LOFF.Size = new System.Drawing.Size(26, 26);
            this.btnInfo_LOFF.TabIndex = 19;
            this.btnInfo_LOFF.UseVisualStyleBackColor = false;
            // 
            // lnbLOFF
            // 
            this.lnbLOFF.Caption = "LOFF [mm] (0~15)";
            this.lnbLOFF.DecimalPlaces = 0;
            this.lnbLOFF.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.lnbLOFF.Location = new System.Drawing.Point(16, 68);
            this.lnbLOFF.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.lnbLOFF.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.lnbLOFF.Name = "lnbLOFF";
            this.lnbLOFF.Size = new System.Drawing.Size(108, 34);
            this.lnbLOFF.TabIndex = 15;
            this.lnbLOFF.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // lnbUOFF
            // 
            this.lnbUOFF.Caption = "UOFF [mm] (0~15)";
            this.lnbUOFF.DecimalPlaces = 0;
            this.lnbUOFF.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.lnbUOFF.Location = new System.Drawing.Point(16, 23);
            this.lnbUOFF.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.lnbUOFF.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.lnbUOFF.Name = "lnbUOFF";
            this.lnbUOFF.Size = new System.Drawing.Size(108, 34);
            this.lnbUOFF.TabIndex = 16;
            this.lnbUOFF.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // btnInfo_UOFF
            // 
            this.btnInfo_UOFF.AutoSize = true;
            this.btnInfo_UOFF.BackColor = System.Drawing.Color.Transparent;
            this.btnInfo_UOFF.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnInfo_UOFF.FlatAppearance.BorderSize = 0;
            this.btnInfo_UOFF.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnInfo_UOFF.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnInfo_UOFF.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInfo_UOFF.Image = global::EFEM.GUIControls.Properties.Resources.information;
            this.btnInfo_UOFF.Location = new System.Drawing.Point(130, 31);
            this.btnInfo_UOFF.Name = "btnInfo_UOFF";
            this.btnInfo_UOFF.Size = new System.Drawing.Size(26, 26);
            this.btnInfo_UOFF.TabIndex = 15;
            this.btnInfo_UOFF.UseVisualStyleBackColor = false;
            // 
            // btnInfo_GCNF
            // 
            this.btnInfo_GCNF.AutoSize = true;
            this.btnInfo_GCNF.BackColor = System.Drawing.Color.Transparent;
            this.btnInfo_GCNF.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnInfo_GCNF.FlatAppearance.BorderSize = 0;
            this.btnInfo_GCNF.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnInfo_GCNF.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnInfo_GCNF.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInfo_GCNF.Image = global::EFEM.GUIControls.Properties.Resources.information;
            this.btnInfo_GCNF.Location = new System.Drawing.Point(120, 63);
            this.btnInfo_GCNF.Name = "btnInfo_GCNF";
            this.btnInfo_GCNF.Size = new System.Drawing.Size(26, 26);
            this.btnInfo_GCNF.TabIndex = 21;
            this.btnInfo_GCNF.UseVisualStyleBackColor = false;
            // 
            // btnInfo_GOFF
            // 
            this.btnInfo_GOFF.AutoSize = true;
            this.btnInfo_GOFF.BackColor = System.Drawing.Color.Transparent;
            this.btnInfo_GOFF.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnInfo_GOFF.FlatAppearance.BorderSize = 0;
            this.btnInfo_GOFF.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnInfo_GOFF.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnInfo_GOFF.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInfo_GOFF.Image = global::EFEM.GUIControls.Properties.Resources.information;
            this.btnInfo_GOFF.Location = new System.Drawing.Point(120, 14);
            this.btnInfo_GOFF.Name = "btnInfo_GOFF";
            this.btnInfo_GOFF.Size = new System.Drawing.Size(26, 26);
            this.btnInfo_GOFF.TabIndex = 20;
            this.btnInfo_GOFF.UseVisualStyleBackColor = false;
            // 
            // TeachMotionConditionCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gMotion);
            this.Name = "TeachMotionConditionCtrl";
            this.Size = new System.Drawing.Size(502, 283);
            this.gMotion.ResumeLayout(false);
            this.gMotion.PerformLayout();
            this.pMotionIllustration.ResumeLayout(false);
            this.tMotion.ResumeLayout(false);
            this.tGetwafer.ResumeLayout(false);
            this.tPutwafer.ResumeLayout(false);
            this.tPutwafer.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gMotion;
        private System.Windows.Forms.Panel pMotionIllustration;
        private System.Windows.Forms.TabControl tMotion;
        private System.Windows.Forms.TabPage tGetwafer;
        private System.Windows.Forms.Button btnInfo_GCNF;
        private System.Windows.Forms.Button btnInfo_GOFF;
        private System.Windows.Forms.Button btnInfo_LOFF;
        private System.Windows.Forms.Button btnInfo_UOFF;
        private LabeledNumBox lnbGOFF;
        private LabeledNumBox lnbGCNF;
        private LabeledNumBox lnbUOFF;
        private LabeledNumBox lnbLOFF;
        private System.Windows.Forms.TabPage tPutwafer;
        private System.Windows.Forms.Button btnInfo_PCNF;
        private System.Windows.Forms.Button btnInfo_POFF;
        private System.Windows.Forms.Button btnInfo_PADJ;
        private LabeledNumBox lnbPCNF;
        private LabeledNumBox lnbPADJ;
        private LabeledNumBox lnbPOFF;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnApply;
    }
}
