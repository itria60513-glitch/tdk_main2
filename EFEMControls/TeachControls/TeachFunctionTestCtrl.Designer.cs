namespace EFEM.GUIControls.TeachControls
{
    partial class TeachFunctionTestCtrl
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
            this.gFunctionTest = new System.Windows.Forms.GroupBox();
            this.gSlot = new System.Windows.Forms.GroupBox();
            this.lnbSlotNumber = new EFEM.GUIControls.LabeledNumBox();
            this.lnbSlotInterval = new EFEM.GUIControls.LabeledNumBox();
            this.gRobot = new System.Windows.Forms.GroupBox();
            this.lnbSpeed = new EFEM.GUIControls.LabeledNumBox();
            this.btnGetWafer = new System.Windows.Forms.Button();
            this.btnPutWafer = new System.Windows.Forms.Button();
            this.gFunctionTest.SuspendLayout();
            this.gSlot.SuspendLayout();
            this.gRobot.SuspendLayout();
            this.SuspendLayout();
            // 
            // gFunctionTest
            // 
            this.gFunctionTest.Controls.Add(this.gRobot);
            this.gFunctionTest.Controls.Add(this.gSlot);
            this.gFunctionTest.Location = new System.Drawing.Point(12, 14);
            this.gFunctionTest.Name = "gFunctionTest";
            this.gFunctionTest.Size = new System.Drawing.Size(454, 187);
            this.gFunctionTest.TabIndex = 0;
            this.gFunctionTest.TabStop = false;
            this.gFunctionTest.Text = "Function Test";
            // 
            // gSlot
            // 
            this.gSlot.Controls.Add(this.lnbSlotInterval);
            this.gSlot.Controls.Add(this.lnbSlotNumber);
            this.gSlot.Location = new System.Drawing.Point(18, 29);
            this.gSlot.Name = "gSlot";
            this.gSlot.Size = new System.Drawing.Size(200, 150);
            this.gSlot.TabIndex = 0;
            this.gSlot.TabStop = false;
            this.gSlot.Text = "Slot Setting";
            // 
            // lnbSlotNumber
            // 
            this.lnbSlotNumber.Caption = "Slot Number";
            this.lnbSlotNumber.Location = new System.Drawing.Point(19, 34);
            this.lnbSlotNumber.Name = "lnbSlotNumber";
            this.lnbSlotNumber.Size = new System.Drawing.Size(150, 34);
            this.lnbSlotNumber.TabIndex = 1;
            this.lnbSlotNumber.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // lnbSlotInterval
            // 
            this.lnbSlotInterval.Caption = "Slot Interval";
            this.lnbSlotInterval.Location = new System.Drawing.Point(19, 95);
            this.lnbSlotInterval.Name = "lnbSlotInterval";
            this.lnbSlotInterval.Size = new System.Drawing.Size(150, 34);
            this.lnbSlotInterval.TabIndex = 2;
            this.lnbSlotInterval.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // gRobot
            // 
            this.gRobot.Controls.Add(this.btnPutWafer);
            this.gRobot.Controls.Add(this.btnGetWafer);
            this.gRobot.Controls.Add(this.lnbSpeed);
            this.gRobot.Location = new System.Drawing.Point(236, 29);
            this.gRobot.Name = "gRobot";
            this.gRobot.Size = new System.Drawing.Size(200, 150);
            this.gRobot.TabIndex = 0;
            this.gRobot.TabStop = false;
            this.gRobot.Text = "Robot Operation";
            // 
            // lnbSpeed
            // 
            this.lnbSpeed.Caption = "Speed(%)";
            this.lnbSpeed.Location = new System.Drawing.Point(19, 19);
            this.lnbSpeed.Name = "lnbSpeed";
            this.lnbSpeed.Size = new System.Drawing.Size(150, 34);
            this.lnbSpeed.TabIndex = 1;
            this.lnbSpeed.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // btnGetWafer
            // 
            this.btnGetWafer.Location = new System.Drawing.Point(19, 68);
            this.btnGetWafer.Name = "btnGetWafer";
            this.btnGetWafer.Size = new System.Drawing.Size(150, 23);
            this.btnGetWafer.TabIndex = 7;
            this.btnGetWafer.Text = "Get Wafer Test";
            this.btnGetWafer.UseVisualStyleBackColor = true;
            // 
            // btnPutWafer
            // 
            this.btnPutWafer.Location = new System.Drawing.Point(19, 106);
            this.btnPutWafer.Name = "btnPutWafer";
            this.btnPutWafer.Size = new System.Drawing.Size(150, 23);
            this.btnPutWafer.TabIndex = 7;
            this.btnPutWafer.Text = "Put Wafer Test";
            this.btnPutWafer.UseVisualStyleBackColor = true;
            // 
            // TeachFunctionTestCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gFunctionTest);
            this.Name = "TeachFunctionTestCtrl";
            this.Size = new System.Drawing.Size(482, 208);
            this.gFunctionTest.ResumeLayout(false);
            this.gSlot.ResumeLayout(false);
            this.gRobot.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gFunctionTest;
        private System.Windows.Forms.GroupBox gSlot;
        private System.Windows.Forms.GroupBox gRobot;
        private LabeledNumBox lnbSpeed;
        private LabeledNumBox lnbSlotInterval;
        private LabeledNumBox lnbSlotNumber;
        private System.Windows.Forms.Button btnPutWafer;
        private System.Windows.Forms.Button btnGetWafer;
    }
}
