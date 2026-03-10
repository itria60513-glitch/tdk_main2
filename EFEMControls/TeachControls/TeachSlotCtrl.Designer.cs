namespace EFEM.GUIControls.TeachControls
{
    partial class TeachSlotCtrl
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
            this.gSlot = new System.Windows.Forms.GroupBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.lnbSlot = new EFEM.GUIControls.LabeledNumBox();
            this.lnbSDIS = new EFEM.GUIControls.LabeledNumBox();
            this.gSlot.SuspendLayout();
            this.SuspendLayout();
            // 
            // gSlot
            // 
            this.gSlot.Controls.Add(this.btnRefresh);
            this.gSlot.Controls.Add(this.btnApply);
            this.gSlot.Controls.Add(this.lnbSlot);
            this.gSlot.Controls.Add(this.lnbSDIS);
            this.gSlot.Location = new System.Drawing.Point(0, 3);
            this.gSlot.Name = "gSlot";
            this.gSlot.Size = new System.Drawing.Size(266, 96);
            this.gSlot.TabIndex = 25;
            this.gSlot.TabStop = false;
            this.gSlot.Text = "Slot";
            // 
            // btnRefresh
            // 
            this.btnRefresh.Image = global::EFEM.GUIControls.Properties.Resources.refresh;
            this.btnRefresh.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnRefresh.Location = new System.Drawing.Point(202, 12);
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
            this.btnApply.Location = new System.Drawing.Point(202, 54);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(58, 40);
            this.btnApply.TabIndex = 21;
            this.btnApply.Text = "Apply ";
            this.btnApply.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // lnbSlot
            // 
            this.lnbSlot.Caption = "Number of slots in station (1~50)";
            this.lnbSlot.DecimalPlaces = 0;
            this.lnbSlot.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.lnbSlot.Location = new System.Drawing.Point(16, 12);
            this.lnbSlot.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.lnbSlot.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.lnbSlot.Name = "lnbSlot";
            this.lnbSlot.Size = new System.Drawing.Size(173, 34);
            this.lnbSlot.TabIndex = 17;
            this.lnbSlot.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lnbSDIS
            // 
            this.lnbSDIS.Caption = "Slot Distance [mm] (1~100)";
            this.lnbSDIS.DecimalPlaces = 0;
            this.lnbSDIS.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.lnbSDIS.Location = new System.Drawing.Point(16, 48);
            this.lnbSDIS.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.lnbSDIS.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.lnbSDIS.Name = "lnbSDIS";
            this.lnbSDIS.Size = new System.Drawing.Size(173, 34);
            this.lnbSDIS.TabIndex = 17;
            this.lnbSDIS.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // TeachSlotCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gSlot);
            this.Name = "TeachSlotCtrl";
            this.Size = new System.Drawing.Size(269, 102);
            this.gSlot.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gSlot;
        private LabeledNumBox lnbSlot;
        private LabeledNumBox lnbSDIS;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnApply;
    }
}
