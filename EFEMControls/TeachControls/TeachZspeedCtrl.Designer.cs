namespace EFEM.GUIControls.TeachControls
{
    partial class TeachZspeedCtrl
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
            this.gZ = new System.Windows.Forms.GroupBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnInfo_ZdropSpd = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnInfo_ZascSpd = new System.Windows.Forms.Button();
            this.lnbSPDZU = new EFEM.GUIControls.LabeledNumBox();
            this.lnbSPDZD = new EFEM.GUIControls.LabeledNumBox();
            this.gZ.SuspendLayout();
            this.SuspendLayout();
            // 
            // gZ
            // 
            this.gZ.Controls.Add(this.btnRefresh);
            this.gZ.Controls.Add(this.btnInfo_ZdropSpd);
            this.gZ.Controls.Add(this.btnApply);
            this.gZ.Controls.Add(this.btnInfo_ZascSpd);
            this.gZ.Controls.Add(this.lnbSPDZU);
            this.gZ.Controls.Add(this.lnbSPDZD);
            this.gZ.Location = new System.Drawing.Point(0, -2);
            this.gZ.Name = "gZ";
            this.gZ.Size = new System.Drawing.Size(222, 98);
            this.gZ.TabIndex = 20;
            this.gZ.TabStop = false;
            this.gZ.Text = "Z axis speed";
            // 
            // btnRefresh
            // 
            this.btnRefresh.Image = global::EFEM.GUIControls.Properties.Resources.refresh;
            this.btnRefresh.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnRefresh.Location = new System.Drawing.Point(158, 12);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(58, 40);
            this.btnRefresh.TabIndex = 22;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnInfo_ZdropSpd
            // 
            this.btnInfo_ZdropSpd.AutoSize = true;
            this.btnInfo_ZdropSpd.BackColor = System.Drawing.Color.Transparent;
            this.btnInfo_ZdropSpd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnInfo_ZdropSpd.FlatAppearance.BorderSize = 0;
            this.btnInfo_ZdropSpd.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnInfo_ZdropSpd.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnInfo_ZdropSpd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInfo_ZdropSpd.Image = global::EFEM.GUIControls.Properties.Resources.information;
            this.btnInfo_ZdropSpd.Location = new System.Drawing.Point(129, 62);
            this.btnInfo_ZdropSpd.Name = "btnInfo_ZdropSpd";
            this.btnInfo_ZdropSpd.Size = new System.Drawing.Size(26, 26);
            this.btnInfo_ZdropSpd.TabIndex = 32;
            this.btnInfo_ZdropSpd.UseVisualStyleBackColor = false;
            // 
            // btnApply
            // 
            this.btnApply.Image = global::EFEM.GUIControls.Properties.Resources.apply;
            this.btnApply.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnApply.Location = new System.Drawing.Point(158, 54);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(58, 40);
            this.btnApply.TabIndex = 21;
            this.btnApply.Text = "Apply ";
            this.btnApply.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // btnInfo_ZascSpd
            // 
            this.btnInfo_ZascSpd.AutoSize = true;
            this.btnInfo_ZascSpd.BackColor = System.Drawing.Color.Transparent;
            this.btnInfo_ZascSpd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnInfo_ZascSpd.FlatAppearance.BorderSize = 0;
            this.btnInfo_ZascSpd.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnInfo_ZascSpd.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnInfo_ZascSpd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInfo_ZascSpd.Image = global::EFEM.GUIControls.Properties.Resources.information;
            this.btnInfo_ZascSpd.Location = new System.Drawing.Point(129, 25);
            this.btnInfo_ZascSpd.Name = "btnInfo_ZascSpd";
            this.btnInfo_ZascSpd.Size = new System.Drawing.Size(26, 26);
            this.btnInfo_ZascSpd.TabIndex = 31;
            this.btnInfo_ZascSpd.UseVisualStyleBackColor = false;
            // 
            // lnbSPDZU
            // 
            this.lnbSPDZU.Caption = "Z Ascent Rate (%)";
            this.lnbSPDZU.DecimalPlaces = 0;
            this.lnbSPDZU.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.lnbSPDZU.Location = new System.Drawing.Point(11, 16);
            this.lnbSPDZU.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.lnbSPDZU.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.lnbSPDZU.Name = "lnbSPDZU";
            this.lnbSPDZU.Size = new System.Drawing.Size(108, 34);
            this.lnbSPDZU.TabIndex = 8;
            this.lnbSPDZU.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // lnbSPDZD
            // 
            this.lnbSPDZD.Caption = "Z Drop Rate (%)";
            this.lnbSPDZD.DecimalPlaces = 0;
            this.lnbSPDZD.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.lnbSPDZD.Location = new System.Drawing.Point(11, 54);
            this.lnbSPDZD.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.lnbSPDZD.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.lnbSPDZD.Name = "lnbSPDZD";
            this.lnbSPDZD.Size = new System.Drawing.Size(108, 34);
            this.lnbSPDZD.TabIndex = 9;
            this.lnbSPDZD.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // TeachZspeedCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gZ);
            this.Name = "TeachZspeedCtrl";
            this.Size = new System.Drawing.Size(222, 98);
            this.gZ.ResumeLayout(false);
            this.gZ.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gZ;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnInfo_ZdropSpd;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnInfo_ZascSpd;
        private LabeledNumBox lnbSPDZU;
        private LabeledNumBox lnbSPDZD;
    }
}
