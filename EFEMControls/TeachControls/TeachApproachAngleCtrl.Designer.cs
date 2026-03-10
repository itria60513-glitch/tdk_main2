namespace EFEM.GUIControls.TeachControls
{
    partial class TeachApproachAngleCtrl
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
            this.gTargetCaption = new System.Windows.Forms.GroupBox();
            this.lnbCurrentAngle = new EFEM.GUIControls.LabeledNumBox();
            this.btnInfo = new System.Windows.Forms.Button();
            this.lTaught = new System.Windows.Forms.Label();
            this.tTaught = new System.Windows.Forms.TextBox();
            this.pIllustration = new System.Windows.Forms.Panel();
            this.gTargetCaption.SuspendLayout();
            this.SuspendLayout();
            // 
            // gTargetCaption
            // 
            this.gTargetCaption.Controls.Add(this.lnbCurrentAngle);
            this.gTargetCaption.Controls.Add(this.btnInfo);
            this.gTargetCaption.Controls.Add(this.lTaught);
            this.gTargetCaption.Controls.Add(this.tTaught);
            this.gTargetCaption.Controls.Add(this.pIllustration);
            this.gTargetCaption.Location = new System.Drawing.Point(0, 3);
            this.gTargetCaption.Name = "gTargetCaption";
            this.gTargetCaption.Size = new System.Drawing.Size(196, 269);
            this.gTargetCaption.TabIndex = 3;
            this.gTargetCaption.TabStop = false;
            this.gTargetCaption.Text = "Target Caption";
            // 
            // lnbCurrentAngle
            // 
            this.lnbCurrentAngle.Caption = "Unit [degree]";
            this.lnbCurrentAngle.DecimalPlaces = 2;
            this.lnbCurrentAngle.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.lnbCurrentAngle.Location = new System.Drawing.Point(14, 49);
            this.lnbCurrentAngle.Maximum = new decimal(new int[] {
            18000,
            0,
            0,
            131072});
            this.lnbCurrentAngle.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.lnbCurrentAngle.Name = "lnbCurrentAngle";
            this.lnbCurrentAngle.Size = new System.Drawing.Size(138, 34);
            this.lnbCurrentAngle.TabIndex = 3;
            this.lnbCurrentAngle.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
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
            this.btnInfo.Location = new System.Drawing.Point(158, 57);
            this.btnInfo.Name = "btnInfo";
            this.btnInfo.Size = new System.Drawing.Size(26, 26);
            this.btnInfo.TabIndex = 7;
            this.btnInfo.UseVisualStyleBackColor = false;
            // 
            // lTaught
            // 
            this.lTaught.AllowDrop = true;
            this.lTaught.AutoSize = true;
            this.lTaught.Location = new System.Drawing.Point(11, 25);
            this.lTaught.Name = "lTaught";
            this.lTaught.Size = new System.Drawing.Size(73, 13);
            this.lTaught.TabIndex = 1;
            this.lTaught.Text = "Taught angle:";
            // 
            // tTaught
            // 
            this.tTaught.Location = new System.Drawing.Point(98, 22);
            this.tTaught.Name = "tTaught";
            this.tTaught.ReadOnly = true;
            this.tTaught.Size = new System.Drawing.Size(90, 20);
            this.tTaught.TabIndex = 1;
            // 
            // pIllustration
            // 
            this.pIllustration.BackColor = System.Drawing.Color.White;
            this.pIllustration.BackgroundImage = global::EFEM.GUIControls.Properties.Resources.ApproachAngle;
            this.pIllustration.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pIllustration.Location = new System.Drawing.Point(14, 89);
            this.pIllustration.Name = "pIllustration";
            this.pIllustration.Size = new System.Drawing.Size(170, 170);
            this.pIllustration.TabIndex = 1;
            // 
            // TeachApproachAngleCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gTargetCaption);
            this.Name = "TeachApproachAngleCtrl";
            this.Size = new System.Drawing.Size(201, 280);
            this.gTargetCaption.ResumeLayout(false);
            this.gTargetCaption.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gTargetCaption;
        private System.Windows.Forms.Button btnInfo;
        private LabeledNumBox lnbCurrentAngle;
        private System.Windows.Forms.Label lTaught;
        private System.Windows.Forms.TextBox tTaught;
        private System.Windows.Forms.Panel pIllustration;

    }
}
