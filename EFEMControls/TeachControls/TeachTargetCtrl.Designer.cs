namespace EFEM.GUIControls
{
    partial class TeachTargetCtrl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TeachTargetCtrl));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tXY = new System.Windows.Forms.TabPage();
            this.TargetX = new EFEM.GUIControls.TeachControls.TeachTargetUnitCtrl();
            this.TargetY = new EFEM.GUIControls.TeachControls.TeachTargetUnitCtrl();
            this.tJoint = new System.Windows.Forms.TabPage();
            this.TargetJ2 = new EFEM.GUIControls.TeachControls.TeachTargetUnitCtrl();
            this.TargetJ4 = new EFEM.GUIControls.TeachControls.TeachTargetUnitCtrl();
            this.gTest = new System.Windows.Forms.GroupBox();
            this.btnReadCurrentPos = new System.Windows.Forms.Button();
            this.btnReadTaughtPos = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.TargetO = new EFEM.GUIControls.TeachControls.TeachApproachAngleCtrl();
            this.TargetJ7 = new EFEM.GUIControls.TeachControls.TeachTargetUnitCtrl();
            this.TargetJ6 = new EFEM.GUIControls.TeachControls.TeachTargetUnitCtrl();
            this.TargetZ = new EFEM.GUIControls.TeachControls.TeachTargetUnitCtrl();
            this.teachApproachAngleCtrl1 = new EFEM.GUIControls.TeachControls.TeachApproachAngleCtrl();
            this.tabControl1.SuspendLayout();
            this.tXY.SuspendLayout();
            this.tJoint.SuspendLayout();
            this.gTest.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tXY);
            this.tabControl1.Controls.Add(this.tJoint);
            this.tabControl1.Location = new System.Drawing.Point(3, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(431, 414);
            this.tabControl1.TabIndex = 7;
            // 
            // tXY
            // 
            this.tXY.Controls.Add(this.TargetX);
            this.tXY.Controls.Add(this.TargetY);
            this.tXY.Location = new System.Drawing.Point(4, 22);
            this.tXY.Name = "tXY";
            this.tXY.Padding = new System.Windows.Forms.Padding(3);
            this.tXY.Size = new System.Drawing.Size(423, 388);
            this.tXY.TabIndex = 0;
            this.tXY.Text = "XY Coordinate";
            this.tXY.UseVisualStyleBackColor = true;
            // 
            // TargetX
            // 
            this.TargetX.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TargetX.BackgroundImage")));
            this.TargetX.Caption = "X axis";
            this.TargetX.CurrentStation = EFEMInterface.EFEMStation.NONE;
            this.TargetX.Location = new System.Drawing.Point(6, 3);
            this.TargetX.Mode = EFEM.GUIControls.TeachControls.TeachTargetUnitCtrl.UnitMode.Coordinate;
            this.TargetX.Name = "TargetX";
            this.TargetX.Size = new System.Drawing.Size(201, 379);
            this.TargetX.TabIndex = 5;
            this.TargetX.Target = EFEM.GUIControls.TeachControls.TeachTargetUnitCtrl.TargetItem.LX0;
            // 
            // TargetY
            // 
            this.TargetY.BackgroundImage = global::EFEM.GUIControls.Properties.Resources.mm_Y;
            this.TargetY.Caption = "Y axis";
            this.TargetY.CurrentStation = EFEMInterface.EFEMStation.NONE;
            this.TargetY.Location = new System.Drawing.Point(213, 3);
            this.TargetY.Mode = EFEM.GUIControls.TeachControls.TeachTargetUnitCtrl.UnitMode.Coordinate;
            this.TargetY.Name = "TargetY";
            this.TargetY.Size = new System.Drawing.Size(201, 379);
            this.TargetY.TabIndex = 5;
            this.TargetY.Target = EFEM.GUIControls.TeachControls.TeachTargetUnitCtrl.TargetItem.LY0;
            // 
            // tJoint
            // 
            this.tJoint.Controls.Add(this.TargetJ2);
            this.tJoint.Controls.Add(this.TargetJ4);
            this.tJoint.Location = new System.Drawing.Point(4, 22);
            this.tJoint.Name = "tJoint";
            this.tJoint.Padding = new System.Windows.Forms.Padding(3);
            this.tJoint.Size = new System.Drawing.Size(423, 388);
            this.tJoint.TabIndex = 1;
            this.tJoint.Text = "Joint Coordinate";
            this.tJoint.UseVisualStyleBackColor = true;
            // 
            // TargetJ2
            // 
            this.TargetJ2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TargetJ2.BackgroundImage")));
            this.TargetJ2.Caption = "Lower Link Axis (J2)";
            this.TargetJ2.CurrentStation = EFEMInterface.EFEMStation.NONE;
            this.TargetJ2.Location = new System.Drawing.Point(6, 3);
            this.TargetJ2.Mode = EFEM.GUIControls.TeachControls.TeachTargetUnitCtrl.UnitMode.Joint;
            this.TargetJ2.Name = "TargetJ2";
            this.TargetJ2.Size = new System.Drawing.Size(201, 379);
            this.TargetJ2.TabIndex = 7;
            this.TargetJ2.Target = EFEM.GUIControls.TeachControls.TeachTargetUnitCtrl.TargetItem.J2;
            // 
            // TargetJ4
            // 
            this.TargetJ4.BackgroundImage = global::EFEM.GUIControls.Properties.Resources.J4;
            this.TargetJ4.Caption = "Upper Link Axis (J4)";
            this.TargetJ4.CurrentStation = EFEMInterface.EFEMStation.NONE;
            this.TargetJ4.Location = new System.Drawing.Point(213, 3);
            this.TargetJ4.Mode = EFEM.GUIControls.TeachControls.TeachTargetUnitCtrl.UnitMode.Joint;
            this.TargetJ4.Name = "TargetJ4";
            this.TargetJ4.Size = new System.Drawing.Size(201, 379);
            this.TargetJ4.TabIndex = 6;
            this.TargetJ4.Target = EFEM.GUIControls.TeachControls.TeachTargetUnitCtrl.TargetItem.J4;
            // 
            // gTest
            // 
            this.gTest.Controls.Add(this.btnReadCurrentPos);
            this.gTest.Controls.Add(this.btnReadTaughtPos);
            this.gTest.Controls.Add(this.btnApply);
            this.gTest.Location = new System.Drawing.Point(1056, 308);
            this.gTest.Name = "gTest";
            this.gTest.Size = new System.Drawing.Size(198, 99);
            this.gTest.TabIndex = 16;
            this.gTest.TabStop = false;
            this.gTest.Text = "Robot Commander";
            // 
            // btnReadCurrentPos
            // 
            this.btnReadCurrentPos.Location = new System.Drawing.Point(98, 18);
            this.btnReadCurrentPos.Name = "btnReadCurrentPos";
            this.btnReadCurrentPos.Size = new System.Drawing.Size(75, 34);
            this.btnReadCurrentPos.TabIndex = 1;
            this.btnReadCurrentPos.Text = "Refresh Current Pos";
            this.btnReadCurrentPos.UseVisualStyleBackColor = true;
            this.btnReadCurrentPos.Click += new System.EventHandler(this.btnReadCurrentPos_Click);
            // 
            // btnReadTaughtPos
            // 
            this.btnReadTaughtPos.Location = new System.Drawing.Point(17, 18);
            this.btnReadTaughtPos.Name = "btnReadTaughtPos";
            this.btnReadTaughtPos.Size = new System.Drawing.Size(75, 34);
            this.btnReadTaughtPos.TabIndex = 1;
            this.btnReadTaughtPos.Text = "Refresh Taught Pos";
            this.btnReadTaughtPos.UseVisualStyleBackColor = true;
            this.btnReadTaughtPos.Click += new System.EventHandler(this.btnReadTaughtPos_Click);
            // 
            // btnApply
            // 
            this.btnApply.Image = global::EFEM.GUIControls.Properties.Resources.apply;
            this.btnApply.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnApply.Location = new System.Drawing.Point(17, 57);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(156, 40);
            this.btnApply.TabIndex = 0;
            this.btnApply.Text = "Apply ";
            this.btnApply.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 163);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(77, 52);
            this.button1.TabIndex = 2;
            this.button1.Text = "Read Current Pos";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // TargetO
            // 
            this.TargetO.BackgroundImage = global::EFEM.GUIControls.Properties.Resources.ApproachAngle;
            this.TargetO.Caption = "Approach Angle (O)";
            this.TargetO.CurrentAngle = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.TargetO.CurrentStation = EFEMInterface.EFEMStation.NONE;
            this.TargetO.Location = new System.Drawing.Point(1056, 28);
            this.TargetO.Name = "TargetO";
            this.TargetO.Size = new System.Drawing.Size(201, 280);
            this.TargetO.TabIndex = 17;
            // 
            // TargetJ7
            // 
            this.TargetJ7.BackgroundImage = global::EFEM.GUIControls.Properties.Resources.H2;
            this.TargetJ7.Caption = "Upper Hand H2 (J7)";
            this.TargetJ7.CurrentStation = EFEMInterface.EFEMStation.NONE;
            this.TargetJ7.Location = new System.Drawing.Point(850, 28);
            this.TargetJ7.Mode = EFEM.GUIControls.TeachControls.TeachTargetUnitCtrl.UnitMode.Joint;
            this.TargetJ7.Name = "TargetJ7";
            this.TargetJ7.Size = new System.Drawing.Size(201, 379);
            this.TargetJ7.TabIndex = 7;
            this.TargetJ7.Target = EFEM.GUIControls.TeachControls.TeachTargetUnitCtrl.TargetItem.J7;
            // 
            // TargetJ6
            // 
            this.TargetJ6.BackgroundImage = global::EFEM.GUIControls.Properties.Resources.H1;
            this.TargetJ6.Caption = "Lower Hand H1 (J6)";
            this.TargetJ6.CurrentStation = EFEMInterface.EFEMStation.NONE;
            this.TargetJ6.Location = new System.Drawing.Point(643, 28);
            this.TargetJ6.Mode = EFEM.GUIControls.TeachControls.TeachTargetUnitCtrl.UnitMode.Joint;
            this.TargetJ6.Name = "TargetJ6";
            this.TargetJ6.Size = new System.Drawing.Size(201, 379);
            this.TargetJ6.TabIndex = 6;
            this.TargetJ6.Target = EFEM.GUIControls.TeachControls.TeachTargetUnitCtrl.TargetItem.J6;
            // 
            // TargetZ
            // 
            this.TargetZ.BackgroundImage = global::EFEM.GUIControls.Properties.Resources.dualmde_Z;
            this.TargetZ.Caption = "Z axis (J3)";
            this.TargetZ.CurrentStation = EFEMInterface.EFEMStation.NONE;
            this.TargetZ.Location = new System.Drawing.Point(439, 28);
            this.TargetZ.Mode = EFEM.GUIControls.TeachControls.TeachTargetUnitCtrl.UnitMode.Joint;
            this.TargetZ.Name = "TargetZ";
            this.TargetZ.Size = new System.Drawing.Size(201, 379);
            this.TargetZ.TabIndex = 6;
            this.TargetZ.Target = EFEM.GUIControls.TeachControls.TeachTargetUnitCtrl.TargetItem.J3;
            // 
            // teachApproachAngleCtrl1
            // 
            this.teachApproachAngleCtrl1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("teachApproachAngleCtrl1.BackgroundImage")));
            this.teachApproachAngleCtrl1.Caption = "Approach Angle (O)";
            this.teachApproachAngleCtrl1.CurrentAngle = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.teachApproachAngleCtrl1.CurrentStation = EFEMInterface.EFEMStation.NONE;
            this.teachApproachAngleCtrl1.Location = new System.Drawing.Point(1056, 28);
            this.teachApproachAngleCtrl1.Name = "teachApproachAngleCtrl1";
            this.teachApproachAngleCtrl1.Size = new System.Drawing.Size(201, 280);
            this.teachApproachAngleCtrl1.TabIndex = 17;
            // 
            // TeachTargetCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TargetO);
            this.Controls.Add(this.gTest);
            this.Controls.Add(this.TargetJ7);
            this.Controls.Add(this.TargetJ6);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.TargetZ);
            this.Name = "TeachTargetCtrl";
            this.Size = new System.Drawing.Size(1260, 422);
            this.tabControl1.ResumeLayout(false);
            this.tXY.ResumeLayout(false);
            this.tJoint.ResumeLayout(false);
            this.gTest.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private TeachControls.TeachTargetUnitCtrl TargetZ;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tXY;
        private TeachControls.TeachTargetUnitCtrl TargetX;
        private TeachControls.TeachTargetUnitCtrl TargetY;
        private System.Windows.Forms.TabPage tJoint;
        private TeachControls.TeachTargetUnitCtrl TargetJ2;
        private TeachControls.TeachTargetUnitCtrl TargetJ4;
        private TeachControls.TeachTargetUnitCtrl TargetJ6;
        private TeachControls.TeachTargetUnitCtrl TargetJ7;
        private System.Windows.Forms.GroupBox gTest;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnReadTaughtPos;
        private System.Windows.Forms.Button btnReadCurrentPos;
        private TeachControls.TeachApproachAngleCtrl TargetO;
        private TeachControls.TeachApproachAngleCtrl teachApproachAngleCtrl1;

    }
}
