namespace EFEM.GUIControls.TeachControls
{
    partial class TeachTargetUnitCtrl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TeachTargetUnitCtrl));
            this.lTaught = new System.Windows.Forms.Label();
            this.tTaught = new System.Windows.Forms.TextBox();
            this.lCurrent = new System.Windows.Forms.Label();
            this.gTargetCaption = new System.Windows.Forms.GroupBox();
            this.tCurrent = new System.Windows.Forms.TextBox();
            this.gMotionCtrl = new System.Windows.Forms.GroupBox();
            this.btnInfo = new System.Windows.Forms.Button();
            this.lnbStep = new EFEM.GUIControls.LabeledNumBox();
            this.btnNeg = new System.Windows.Forms.Button();
            this.btnPos = new System.Windows.Forms.Button();
            this.pIllustration = new System.Windows.Forms.Panel();
            this.gTargetCaption.SuspendLayout();
            this.gMotionCtrl.SuspendLayout();
            this.SuspendLayout();
            // 
            // lTaught
            // 
            this.lTaught.AllowDrop = true;
            this.lTaught.AutoSize = true;
            this.lTaught.Location = new System.Drawing.Point(11, 25);
            this.lTaught.Name = "lTaught";
            this.lTaught.Size = new System.Drawing.Size(83, 13);
            this.lTaught.TabIndex = 1;
            this.lTaught.Text = "Taught position:";
            // 
            // tTaught
            // 
            this.tTaught.Location = new System.Drawing.Point(98, 22);
            this.tTaught.Name = "tTaught";
            this.tTaught.ReadOnly = true;
            this.tTaught.Size = new System.Drawing.Size(90, 20);
            this.tTaught.TabIndex = 1;
            // 
            // lCurrent
            // 
            this.lCurrent.AutoSize = true;
            this.lCurrent.Location = new System.Drawing.Point(11, 51);
            this.lCurrent.Name = "lCurrent";
            this.lCurrent.Size = new System.Drawing.Size(83, 13);
            this.lCurrent.TabIndex = 1;
            this.lCurrent.Text = "Current position:";
            // 
            // gTargetCaption
            // 
            this.gTargetCaption.Controls.Add(this.tCurrent);
            this.gTargetCaption.Controls.Add(this.gMotionCtrl);
            this.gTargetCaption.Controls.Add(this.lTaught);
            this.gTargetCaption.Controls.Add(this.tTaught);
            this.gTargetCaption.Controls.Add(this.lCurrent);
            this.gTargetCaption.Controls.Add(this.pIllustration);
            this.gTargetCaption.Location = new System.Drawing.Point(3, 3);
            this.gTargetCaption.Name = "gTargetCaption";
            this.gTargetCaption.Size = new System.Drawing.Size(196, 374);
            this.gTargetCaption.TabIndex = 2;
            this.gTargetCaption.TabStop = false;
            this.gTargetCaption.Text = "Target Caption";
            // 
            // tCurrent
            // 
            this.tCurrent.Location = new System.Drawing.Point(98, 51);
            this.tCurrent.Name = "tCurrent";
            this.tCurrent.ReadOnly = true;
            this.tCurrent.Size = new System.Drawing.Size(90, 20);
            this.tCurrent.TabIndex = 10;
            // 
            // gMotionCtrl
            // 
            this.gMotionCtrl.Controls.Add(this.btnInfo);
            this.gMotionCtrl.Controls.Add(this.lnbStep);
            this.gMotionCtrl.Controls.Add(this.btnNeg);
            this.gMotionCtrl.Controls.Add(this.btnPos);
            this.gMotionCtrl.Location = new System.Drawing.Point(14, 265);
            this.gMotionCtrl.Name = "gMotionCtrl";
            this.gMotionCtrl.Size = new System.Drawing.Size(170, 103);
            this.gMotionCtrl.TabIndex = 9;
            this.gMotionCtrl.TabStop = false;
            this.gMotionCtrl.Text = "Motion Control";
            // 
            // btnInfo
            // 
            this.btnInfo.AutoSize = true;
            this.btnInfo.BackColor = System.Drawing.Color.Transparent;
            this.btnInfo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnInfo.FlatAppearance.BorderSize = 0;
            this.btnInfo.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnInfo.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnInfo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInfo.Image = global::EFEM.GUIControls.Properties.Resources.information;
            this.btnInfo.Location = new System.Drawing.Point(138, 4);
            this.btnInfo.Name = "btnInfo";
            this.btnInfo.Size = new System.Drawing.Size(26, 26);
            this.btnInfo.TabIndex = 7;
            this.btnInfo.UseVisualStyleBackColor = false;
            // 
            // lnbStep
            // 
            this.lnbStep.Caption = "Unit [mm]";
            this.lnbStep.DecimalPlaces = 2;
            this.lnbStep.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.lnbStep.Location = new System.Drawing.Point(6, 19);
            this.lnbStep.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            131072});
            this.lnbStep.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.lnbStep.Name = "lnbStep";
            this.lnbStep.Size = new System.Drawing.Size(157, 34);
            this.lnbStep.TabIndex = 3;
            this.lnbStep.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // btnNeg
            // 
            this.btnNeg.Image = global::EFEM.GUIControls.Properties.Resources.minus;
            this.btnNeg.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnNeg.Location = new System.Drawing.Point(6, 59);
            this.btnNeg.Name = "btnNeg";
            this.btnNeg.Size = new System.Drawing.Size(73, 38);
            this.btnNeg.TabIndex = 1;
            this.btnNeg.Text = "Negative";
            this.btnNeg.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnNeg.UseVisualStyleBackColor = true;
            this.btnNeg.Click += new System.EventHandler(this.btnNeg_Click);
            // 
            // btnPos
            // 
            this.btnPos.Image = ((System.Drawing.Image)(resources.GetObject("btnPos.Image")));
            this.btnPos.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnPos.Location = new System.Drawing.Point(90, 59);
            this.btnPos.Name = "btnPos";
            this.btnPos.Size = new System.Drawing.Size(73, 38);
            this.btnPos.TabIndex = 1;
            this.btnPos.Text = "Positive";
            this.btnPos.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnPos.UseVisualStyleBackColor = true;
            this.btnPos.Click += new System.EventHandler(this.btnPos_Click);
            // 
            // pIllustration
            // 
            this.pIllustration.BackColor = System.Drawing.Color.White;
            this.pIllustration.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pIllustration.BackgroundImage")));
            this.pIllustration.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pIllustration.Location = new System.Drawing.Point(14, 89);
            this.pIllustration.Name = "pIllustration";
            this.pIllustration.Size = new System.Drawing.Size(170, 170);
            this.pIllustration.TabIndex = 1;
            // 
            // TeachTargetUnitCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gTargetCaption);
            this.Name = "TeachTargetUnitCtrl";
            this.Size = new System.Drawing.Size(201, 381);
            this.gTargetCaption.ResumeLayout(false);
            this.gTargetCaption.PerformLayout();
            this.gMotionCtrl.ResumeLayout(false);
            this.gMotionCtrl.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private LabeledNumBox lnbStep;
        private System.Windows.Forms.Button btnPos;
        private System.Windows.Forms.Label lTaught;
        private System.Windows.Forms.TextBox tTaught;
        private System.Windows.Forms.Button btnNeg;
        private System.Windows.Forms.Label lCurrent;
        private System.Windows.Forms.Panel pIllustration;
        private System.Windows.Forms.GroupBox gTargetCaption;
        private System.Windows.Forms.GroupBox gMotionCtrl;
        private System.Windows.Forms.Button btnInfo;
        private System.Windows.Forms.TextBox tCurrent;
    }
}
