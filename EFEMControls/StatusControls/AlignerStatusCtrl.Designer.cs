namespace EFEM.GUIControls.StatusControls
{
    partial class AlignerStatusCtrl
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
            this.lbAlignerStatus = new EFEM.GUIControls.LedBulb();
            this.tAlignerStatus = new System.Windows.Forms.TextBox();
            this.lAlignerStatus = new System.Windows.Forms.Label();
            this.gAligner = new System.Windows.Forms.GroupBox();
            this.bsAlignerChuck = new EFEM.GUIControls.BuldedStatus();
            this.lJ1 = new System.Windows.Forms.Label();
            this.tJ1pos = new System.Windows.Forms.TextBox();
            this.bsWaferOnAligner = new EFEM.GUIControls.BuldedStatus();
            this.tAlignerSpeed = new System.Windows.Forms.TextBox();
            this.lAlignerSpeed = new System.Windows.Forms.Label();
            this.gAligner.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbAlignerStatus
            // 
            this.lbAlignerStatus.Location = new System.Drawing.Point(181, 20);
            this.lbAlignerStatus.Name = "lbAlignerStatus";
            this.lbAlignerStatus.On = false;
            this.lbAlignerStatus.Size = new System.Drawing.Size(28, 23);
            this.lbAlignerStatus.TabIndex = 31;
            this.lbAlignerStatus.Text = "ledBulb1";
            // 
            // tAlignerStatus
            // 
            this.tAlignerStatus.Location = new System.Drawing.Point(84, 23);
            this.tAlignerStatus.Name = "tAlignerStatus";
            this.tAlignerStatus.ReadOnly = true;
            this.tAlignerStatus.Size = new System.Drawing.Size(90, 20);
            this.tAlignerStatus.TabIndex = 30;
            this.tAlignerStatus.Text = "<Unknown>";
            // 
            // lAlignerStatus
            // 
            this.lAlignerStatus.AutoSize = true;
            this.lAlignerStatus.Location = new System.Drawing.Point(6, 26);
            this.lAlignerStatus.Name = "lAlignerStatus";
            this.lAlignerStatus.Size = new System.Drawing.Size(75, 13);
            this.lAlignerStatus.TabIndex = 29;
            this.lAlignerStatus.Text = "Aligner Status:";
            // 
            // gAligner
            // 
            this.gAligner.Controls.Add(this.tAlignerSpeed);
            this.gAligner.Controls.Add(this.lAlignerSpeed);
            this.gAligner.Controls.Add(this.bsAlignerChuck);
            this.gAligner.Controls.Add(this.lJ1);
            this.gAligner.Controls.Add(this.tJ1pos);
            this.gAligner.Controls.Add(this.bsWaferOnAligner);
            this.gAligner.Controls.Add(this.lAlignerStatus);
            this.gAligner.Controls.Add(this.lbAlignerStatus);
            this.gAligner.Controls.Add(this.tAlignerStatus);
            this.gAligner.Location = new System.Drawing.Point(3, 3);
            this.gAligner.Name = "gAligner";
            this.gAligner.Size = new System.Drawing.Size(331, 183);
            this.gAligner.TabIndex = 32;
            this.gAligner.TabStop = false;
            this.gAligner.Text = "Aligner Status";
            // 
            // bsAlignerChuck
            // 
            this.bsAlignerChuck.Caption = "Wafer Chucked";
            this.bsAlignerChuck.Location = new System.Drawing.Point(181, 136);
            this.bsAlignerChuck.Name = "bsAlignerChuck";
            this.bsAlignerChuck.On = false;
            this.bsAlignerChuck.Size = new System.Drawing.Size(125, 26);
            this.bsAlignerChuck.TabIndex = 36;
            // 
            // lJ1
            // 
            this.lJ1.AutoSize = true;
            this.lJ1.Location = new System.Drawing.Point(6, 69);
            this.lJ1.Name = "lJ1";
            this.lJ1.Size = new System.Drawing.Size(185, 13);
            this.lJ1.TabIndex = 32;
            this.lJ1.Text = "Rotating part of aligner [J1] (degree) : ";
            // 
            // tJ1pos
            // 
            this.tJ1pos.Location = new System.Drawing.Point(195, 66);
            this.tJ1pos.Name = "tJ1pos";
            this.tJ1pos.ReadOnly = true;
            this.tJ1pos.Size = new System.Drawing.Size(90, 20);
            this.tJ1pos.TabIndex = 33;
            this.tJ1pos.Text = "N/A";
            // 
            // bsWaferOnAligner
            // 
            this.bsWaferOnAligner.Caption = "Wafer Presence";
            this.bsWaferOnAligner.Location = new System.Drawing.Point(13, 136);
            this.bsWaferOnAligner.Name = "bsWaferOnAligner";
            this.bsWaferOnAligner.On = false;
            this.bsWaferOnAligner.Size = new System.Drawing.Size(125, 26);
            this.bsWaferOnAligner.TabIndex = 35;
            // 
            // tAlignerSpeed
            // 
            this.tAlignerSpeed.Location = new System.Drawing.Point(195, 95);
            this.tAlignerSpeed.Name = "tAlignerSpeed";
            this.tAlignerSpeed.ReadOnly = true;
            this.tAlignerSpeed.Size = new System.Drawing.Size(90, 20);
            this.tAlignerSpeed.TabIndex = 38;
            this.tAlignerSpeed.Text = "N/A";
            // 
            // lAlignerSpeed
            // 
            this.lAlignerSpeed.AutoSize = true;
            this.lAlignerSpeed.Location = new System.Drawing.Point(10, 98);
            this.lAlignerSpeed.Name = "lAlignerSpeed";
            this.lAlignerSpeed.Size = new System.Drawing.Size(76, 13);
            this.lAlignerSpeed.TabIndex = 37;
            this.lAlignerSpeed.Text = "Aligner Speed:";
            // 
            // AlignerStatusCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gAligner);
            this.Name = "AlignerStatusCtrl";
            this.Size = new System.Drawing.Size(339, 189);
            this.gAligner.ResumeLayout(false);
            this.gAligner.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private LedBulb lbAlignerStatus;
        private System.Windows.Forms.TextBox tAlignerStatus;
        private System.Windows.Forms.Label lAlignerStatus;
        private System.Windows.Forms.GroupBox gAligner;
        private System.Windows.Forms.Label lJ1;
        private System.Windows.Forms.TextBox tJ1pos;
        private BuldedStatus bsAlignerChuck;
        private BuldedStatus bsWaferOnAligner;
        private System.Windows.Forms.TextBox tAlignerSpeed;
        private System.Windows.Forms.Label lAlignerSpeed;
    }
}
