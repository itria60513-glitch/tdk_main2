namespace EFEM.GUIControls.StatusControls
{
    partial class RobotStatusCtrl
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
            this.gRobotStatus = new System.Windows.Forms.GroupBox();
            this.gPosition = new System.Windows.Forms.GroupBox();
            this.lJ2 = new System.Windows.Forms.Label();
            this.tJ2pos = new System.Windows.Forms.TextBox();
            this.lJ4pos = new System.Windows.Forms.Label();
            this.tJ4pos = new System.Windows.Forms.TextBox();
            this.lJ6pos = new System.Windows.Forms.Label();
            this.tJ7pos = new System.Windows.Forms.TextBox();
            this.tHand = new System.Windows.Forms.TabControl();
            this.tabH1 = new System.Windows.Forms.TabPage();
            this.tHA1 = new System.Windows.Forms.TextBox();
            this.bsH1Chuck = new EFEM.GUIControls.BuldedStatus();
            this.lHA1 = new System.Windows.Forms.Label();
            this.tZ1pos = new System.Windows.Forms.TextBox();
            this.lZ1 = new System.Windows.Forms.Label();
            this.tY1pos = new System.Windows.Forms.TextBox();
            this.lY1pos = new System.Windows.Forms.Label();
            this.tX1pos = new System.Windows.Forms.TextBox();
            this.bsWaferOnH1 = new EFEM.GUIControls.BuldedStatus();
            this.lX1pos = new System.Windows.Forms.Label();
            this.tabH2 = new System.Windows.Forms.TabPage();
            this.bsH2Chuck = new EFEM.GUIControls.BuldedStatus();
            this.tHA2 = new System.Windows.Forms.TextBox();
            this.lX2pos = new System.Windows.Forms.Label();
            this.lHA2 = new System.Windows.Forms.Label();
            this.tX2pos = new System.Windows.Forms.TextBox();
            this.tZ2pos = new System.Windows.Forms.TextBox();
            this.lY2pos = new System.Windows.Forms.Label();
            this.lZ2pos = new System.Windows.Forms.Label();
            this.tY2pos = new System.Windows.Forms.TextBox();
            this.bsWaferOnH2 = new EFEM.GUIControls.BuldedStatus();
            this.tJ6pos = new System.Windows.Forms.TextBox();
            this.lJ7pos = new System.Windows.Forms.Label();
            this.tRNST = new System.Windows.Forms.TextBox();
            this.lRNST = new System.Windows.Forms.Label();
            this.tRobotMode = new System.Windows.Forms.TextBox();
            this.lRobotMode = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lbRobotStatus = new EFEM.GUIControls.LedBulb();
            this.tRobotStatus = new System.Windows.Forms.TextBox();
            this.lRobotStatus = new System.Windows.Forms.Label();
            this.tRobotSpeed = new System.Windows.Forms.TextBox();
            this.lRobotSpeed = new System.Windows.Forms.Label();
            this.gRobotStatus.SuspendLayout();
            this.gPosition.SuspendLayout();
            this.tHand.SuspendLayout();
            this.tabH1.SuspendLayout();
            this.tabH2.SuspendLayout();
            this.SuspendLayout();
            // 
            // gRobotStatus
            // 
            this.gRobotStatus.Controls.Add(this.tRobotSpeed);
            this.gRobotStatus.Controls.Add(this.lRobotSpeed);
            this.gRobotStatus.Controls.Add(this.gPosition);
            this.gRobotStatus.Controls.Add(this.tRNST);
            this.gRobotStatus.Controls.Add(this.lRNST);
            this.gRobotStatus.Controls.Add(this.tRobotMode);
            this.gRobotStatus.Controls.Add(this.lRobotMode);
            this.gRobotStatus.Controls.Add(this.panel1);
            this.gRobotStatus.Controls.Add(this.lbRobotStatus);
            this.gRobotStatus.Controls.Add(this.tRobotStatus);
            this.gRobotStatus.Controls.Add(this.lRobotStatus);
            this.gRobotStatus.Location = new System.Drawing.Point(3, 3);
            this.gRobotStatus.Name = "gRobotStatus";
            this.gRobotStatus.Size = new System.Drawing.Size(866, 340);
            this.gRobotStatus.TabIndex = 0;
            this.gRobotStatus.TabStop = false;
            this.gRobotStatus.Text = "Robot Status";
            // 
            // gPosition
            // 
            this.gPosition.Controls.Add(this.lJ2);
            this.gPosition.Controls.Add(this.tJ2pos);
            this.gPosition.Controls.Add(this.lJ4pos);
            this.gPosition.Controls.Add(this.tJ4pos);
            this.gPosition.Controls.Add(this.lJ6pos);
            this.gPosition.Controls.Add(this.tJ7pos);
            this.gPosition.Controls.Add(this.tHand);
            this.gPosition.Controls.Add(this.tJ6pos);
            this.gPosition.Controls.Add(this.lJ7pos);
            this.gPosition.Location = new System.Drawing.Point(8, 82);
            this.gPosition.Name = "gPosition";
            this.gPosition.Size = new System.Drawing.Size(605, 247);
            this.gPosition.TabIndex = 34;
            this.gPosition.TabStop = false;
            this.gPosition.Text = "Position";
            // 
            // lJ2
            // 
            this.lJ2.AutoSize = true;
            this.lJ2.Location = new System.Drawing.Point(16, 30);
            this.lJ2.Name = "lJ2";
            this.lJ2.Size = new System.Drawing.Size(176, 13);
            this.lJ2.TabIndex = 3;
            this.lJ2.Text = "Lower Link Axis data [J2] (degree) : ";
            // 
            // tJ2pos
            // 
            this.tJ2pos.Location = new System.Drawing.Point(208, 27);
            this.tJ2pos.Name = "tJ2pos";
            this.tJ2pos.ReadOnly = true;
            this.tJ2pos.Size = new System.Drawing.Size(90, 20);
            this.tJ2pos.TabIndex = 19;
            // 
            // lJ4pos
            // 
            this.lJ4pos.AutoSize = true;
            this.lJ4pos.Location = new System.Drawing.Point(16, 84);
            this.lJ4pos.Name = "lJ4pos";
            this.lJ4pos.Size = new System.Drawing.Size(176, 13);
            this.lJ4pos.TabIndex = 22;
            this.lJ4pos.Text = "Upper Link Axis data [J4] (degree) : ";
            // 
            // tJ4pos
            // 
            this.tJ4pos.Location = new System.Drawing.Point(208, 81);
            this.tJ4pos.Name = "tJ4pos";
            this.tJ4pos.ReadOnly = true;
            this.tJ4pos.Size = new System.Drawing.Size(90, 20);
            this.tJ4pos.TabIndex = 23;
            // 
            // lJ6pos
            // 
            this.lJ6pos.AutoSize = true;
            this.lJ6pos.Location = new System.Drawing.Point(16, 147);
            this.lJ6pos.Name = "lJ6pos";
            this.lJ6pos.Size = new System.Drawing.Size(177, 13);
            this.lJ6pos.TabIndex = 24;
            this.lJ6pos.Text = "Lower Hand H1 data [J6] (degree) : ";
            // 
            // tJ7pos
            // 
            this.tJ7pos.Location = new System.Drawing.Point(208, 199);
            this.tJ7pos.Name = "tJ7pos";
            this.tJ7pos.ReadOnly = true;
            this.tJ7pos.Size = new System.Drawing.Size(90, 20);
            this.tJ7pos.TabIndex = 27;
            // 
            // tHand
            // 
            this.tHand.Controls.Add(this.tabH1);
            this.tHand.Controls.Add(this.tabH2);
            this.tHand.Location = new System.Drawing.Point(317, 19);
            this.tHand.Name = "tHand";
            this.tHand.SelectedIndex = 0;
            this.tHand.Size = new System.Drawing.Size(282, 218);
            this.tHand.TabIndex = 2;
            // 
            // tabH1
            // 
            this.tabH1.Controls.Add(this.tHA1);
            this.tabH1.Controls.Add(this.bsH1Chuck);
            this.tabH1.Controls.Add(this.lHA1);
            this.tabH1.Controls.Add(this.tZ1pos);
            this.tabH1.Controls.Add(this.lZ1);
            this.tabH1.Controls.Add(this.tY1pos);
            this.tabH1.Controls.Add(this.lY1pos);
            this.tabH1.Controls.Add(this.tX1pos);
            this.tabH1.Controls.Add(this.bsWaferOnH1);
            this.tabH1.Controls.Add(this.lX1pos);
            this.tabH1.Location = new System.Drawing.Point(4, 22);
            this.tabH1.Name = "tabH1";
            this.tabH1.Padding = new System.Windows.Forms.Padding(3);
            this.tabH1.Size = new System.Drawing.Size(274, 192);
            this.tabH1.TabIndex = 0;
            this.tabH1.Text = "Hand1";
            this.tabH1.UseVisualStyleBackColor = true;
            // 
            // tHA1
            // 
            this.tHA1.Location = new System.Drawing.Point(138, 157);
            this.tHA1.Name = "tHA1";
            this.tHA1.ReadOnly = true;
            this.tHA1.Size = new System.Drawing.Size(109, 20);
            this.tHA1.TabIndex = 10;
            // 
            // bsH1Chuck
            // 
            this.bsH1Chuck.Caption = "Wafer Chucked";
            this.bsH1Chuck.Location = new System.Drawing.Point(138, 16);
            this.bsH1Chuck.Name = "bsH1Chuck";
            this.bsH1Chuck.On = false;
            this.bsH1Chuck.Size = new System.Drawing.Size(125, 26);
            this.bsH1Chuck.TabIndex = 34;
            // 
            // lHA1
            // 
            this.lHA1.AutoSize = true;
            this.lHA1.Location = new System.Drawing.Point(16, 160);
            this.lHA1.Name = "lHA1";
            this.lHA1.Size = new System.Drawing.Size(114, 13);
            this.lHA1.TabIndex = 9;
            this.lHA1.Text = "Hand Angle (degree) : ";
            // 
            // tZ1pos
            // 
            this.tZ1pos.Location = new System.Drawing.Point(105, 122);
            this.tZ1pos.Name = "tZ1pos";
            this.tZ1pos.ReadOnly = true;
            this.tZ1pos.Size = new System.Drawing.Size(142, 20);
            this.tZ1pos.TabIndex = 8;
            // 
            // lZ1
            // 
            this.lZ1.AutoSize = true;
            this.lZ1.Location = new System.Drawing.Point(16, 125);
            this.lZ1.Name = "lZ1";
            this.lZ1.Size = new System.Drawing.Size(87, 13);
            this.lZ1.TabIndex = 7;
            this.lZ1.Text = "Z position (mm) : ";
            // 
            // tY1pos
            // 
            this.tY1pos.Location = new System.Drawing.Point(105, 89);
            this.tY1pos.Name = "tY1pos";
            this.tY1pos.ReadOnly = true;
            this.tY1pos.Size = new System.Drawing.Size(142, 20);
            this.tY1pos.TabIndex = 6;
            // 
            // lY1pos
            // 
            this.lY1pos.AutoSize = true;
            this.lY1pos.Location = new System.Drawing.Point(16, 92);
            this.lY1pos.Name = "lY1pos";
            this.lY1pos.Size = new System.Drawing.Size(87, 13);
            this.lY1pos.TabIndex = 5;
            this.lY1pos.Text = "Y position (mm) : ";
            // 
            // tX1pos
            // 
            this.tX1pos.Location = new System.Drawing.Point(105, 54);
            this.tX1pos.Name = "tX1pos";
            this.tX1pos.ReadOnly = true;
            this.tX1pos.Size = new System.Drawing.Size(142, 20);
            this.tX1pos.TabIndex = 4;
            // 
            // bsWaferOnH1
            // 
            this.bsWaferOnH1.Caption = "Wafer Presence";
            this.bsWaferOnH1.Location = new System.Drawing.Point(16, 16);
            this.bsWaferOnH1.Name = "bsWaferOnH1";
            this.bsWaferOnH1.On = false;
            this.bsWaferOnH1.Size = new System.Drawing.Size(125, 26);
            this.bsWaferOnH1.TabIndex = 30;
            // 
            // lX1pos
            // 
            this.lX1pos.AutoSize = true;
            this.lX1pos.Location = new System.Drawing.Point(16, 57);
            this.lX1pos.Name = "lX1pos";
            this.lX1pos.Size = new System.Drawing.Size(87, 13);
            this.lX1pos.TabIndex = 3;
            this.lX1pos.Text = "X position (mm) : ";
            // 
            // tabH2
            // 
            this.tabH2.Controls.Add(this.bsH2Chuck);
            this.tabH2.Controls.Add(this.tHA2);
            this.tabH2.Controls.Add(this.lX2pos);
            this.tabH2.Controls.Add(this.lHA2);
            this.tabH2.Controls.Add(this.tX2pos);
            this.tabH2.Controls.Add(this.tZ2pos);
            this.tabH2.Controls.Add(this.lY2pos);
            this.tabH2.Controls.Add(this.lZ2pos);
            this.tabH2.Controls.Add(this.tY2pos);
            this.tabH2.Controls.Add(this.bsWaferOnH2);
            this.tabH2.Location = new System.Drawing.Point(4, 22);
            this.tabH2.Name = "tabH2";
            this.tabH2.Padding = new System.Windows.Forms.Padding(3);
            this.tabH2.Size = new System.Drawing.Size(274, 192);
            this.tabH2.TabIndex = 1;
            this.tabH2.Text = "Hand2";
            this.tabH2.UseVisualStyleBackColor = true;
            // 
            // bsH2Chuck
            // 
            this.bsH2Chuck.Caption = "Wafer Chucked";
            this.bsH2Chuck.Location = new System.Drawing.Point(138, 16);
            this.bsH2Chuck.Name = "bsH2Chuck";
            this.bsH2Chuck.On = false;
            this.bsH2Chuck.Size = new System.Drawing.Size(125, 26);
            this.bsH2Chuck.TabIndex = 34;
            // 
            // tHA2
            // 
            this.tHA2.Location = new System.Drawing.Point(137, 157);
            this.tHA2.Name = "tHA2";
            this.tHA2.ReadOnly = true;
            this.tHA2.Size = new System.Drawing.Size(110, 20);
            this.tHA2.TabIndex = 18;
            // 
            // lX2pos
            // 
            this.lX2pos.AutoSize = true;
            this.lX2pos.Location = new System.Drawing.Point(16, 57);
            this.lX2pos.Name = "lX2pos";
            this.lX2pos.Size = new System.Drawing.Size(87, 13);
            this.lX2pos.TabIndex = 11;
            this.lX2pos.Text = "X position (mm) : ";
            // 
            // lHA2
            // 
            this.lHA2.AutoSize = true;
            this.lHA2.Location = new System.Drawing.Point(16, 160);
            this.lHA2.Name = "lHA2";
            this.lHA2.Size = new System.Drawing.Size(114, 13);
            this.lHA2.TabIndex = 17;
            this.lHA2.Text = "Hand Angle (degree) : ";
            // 
            // tX2pos
            // 
            this.tX2pos.Location = new System.Drawing.Point(105, 54);
            this.tX2pos.Name = "tX2pos";
            this.tX2pos.ReadOnly = true;
            this.tX2pos.Size = new System.Drawing.Size(142, 20);
            this.tX2pos.TabIndex = 12;
            // 
            // tZ2pos
            // 
            this.tZ2pos.Location = new System.Drawing.Point(105, 122);
            this.tZ2pos.Name = "tZ2pos";
            this.tZ2pos.ReadOnly = true;
            this.tZ2pos.Size = new System.Drawing.Size(142, 20);
            this.tZ2pos.TabIndex = 16;
            // 
            // lY2pos
            // 
            this.lY2pos.AutoSize = true;
            this.lY2pos.Location = new System.Drawing.Point(16, 92);
            this.lY2pos.Name = "lY2pos";
            this.lY2pos.Size = new System.Drawing.Size(87, 13);
            this.lY2pos.TabIndex = 13;
            this.lY2pos.Text = "Y position (mm) : ";
            // 
            // lZ2pos
            // 
            this.lZ2pos.AutoSize = true;
            this.lZ2pos.Location = new System.Drawing.Point(16, 125);
            this.lZ2pos.Name = "lZ2pos";
            this.lZ2pos.Size = new System.Drawing.Size(87, 13);
            this.lZ2pos.TabIndex = 15;
            this.lZ2pos.Text = "Z position (mm) : ";
            // 
            // tY2pos
            // 
            this.tY2pos.Location = new System.Drawing.Point(105, 89);
            this.tY2pos.Name = "tY2pos";
            this.tY2pos.ReadOnly = true;
            this.tY2pos.Size = new System.Drawing.Size(142, 20);
            this.tY2pos.TabIndex = 14;
            // 
            // bsWaferOnH2
            // 
            this.bsWaferOnH2.Caption = "Wafer Presence";
            this.bsWaferOnH2.Location = new System.Drawing.Point(16, 16);
            this.bsWaferOnH2.Name = "bsWaferOnH2";
            this.bsWaferOnH2.On = false;
            this.bsWaferOnH2.Size = new System.Drawing.Size(125, 26);
            this.bsWaferOnH2.TabIndex = 31;
            // 
            // tJ6pos
            // 
            this.tJ6pos.Location = new System.Drawing.Point(208, 144);
            this.tJ6pos.Name = "tJ6pos";
            this.tJ6pos.ReadOnly = true;
            this.tJ6pos.Size = new System.Drawing.Size(90, 20);
            this.tJ6pos.TabIndex = 25;
            // 
            // lJ7pos
            // 
            this.lJ7pos.AutoSize = true;
            this.lJ7pos.Location = new System.Drawing.Point(16, 202);
            this.lJ7pos.Name = "lJ7pos";
            this.lJ7pos.Size = new System.Drawing.Size(177, 13);
            this.lJ7pos.TabIndex = 26;
            this.lJ7pos.Text = "Upper Hand H2 data [J7] (degree) : ";
            // 
            // tRNST
            // 
            this.tRNST.Location = new System.Drawing.Point(532, 63);
            this.tRNST.Name = "tRNST";
            this.tRNST.ReadOnly = true;
            this.tRNST.Size = new System.Drawing.Size(76, 20);
            this.tRNST.TabIndex = 33;
            this.tRNST.Text = "Home";
            // 
            // lRNST
            // 
            this.lRNST.AutoSize = true;
            this.lRNST.Location = new System.Drawing.Point(440, 66);
            this.lRNST.Name = "lRNST";
            this.lRNST.Size = new System.Drawing.Size(86, 13);
            this.lRNST.TabIndex = 32;
            this.lRNST.Text = "Nearest Station :";
            // 
            // tRobotMode
            // 
            this.tRobotMode.Location = new System.Drawing.Point(329, 63);
            this.tRobotMode.Name = "tRobotMode";
            this.tRobotMode.ReadOnly = true;
            this.tRobotMode.Size = new System.Drawing.Size(90, 20);
            this.tRobotMode.TabIndex = 33;
            this.tRobotMode.Text = "<Unknown>";
            // 
            // lRobotMode
            // 
            this.lRobotMode.AutoSize = true;
            this.lRobotMode.Location = new System.Drawing.Point(251, 66);
            this.lRobotMode.Name = "lRobotMode";
            this.lRobotMode.Size = new System.Drawing.Size(69, 13);
            this.lRobotMode.TabIndex = 32;
            this.lRobotMode.Text = "Robot Mode:";
            // 
            // panel1
            // 
            this.panel1.BackgroundImage = global::EFEM.GUIControls.Properties.Resources.RobotMap;
            this.panel1.Location = new System.Drawing.Point(631, 13);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(224, 316);
            this.panel1.TabIndex = 29;
            // 
            // lbRobotStatus
            // 
            this.lbRobotStatus.Location = new System.Drawing.Point(191, 24);
            this.lbRobotStatus.Name = "lbRobotStatus";
            this.lbRobotStatus.On = false;
            this.lbRobotStatus.Size = new System.Drawing.Size(28, 23);
            this.lbRobotStatus.TabIndex = 28;
            this.lbRobotStatus.Text = "ledBulb1";
            // 
            // tRobotStatus
            // 
            this.tRobotStatus.Location = new System.Drawing.Point(94, 27);
            this.tRobotStatus.Name = "tRobotStatus";
            this.tRobotStatus.ReadOnly = true;
            this.tRobotStatus.Size = new System.Drawing.Size(90, 20);
            this.tRobotStatus.TabIndex = 19;
            this.tRobotStatus.Text = "Unknown";
            // 
            // lRobotStatus
            // 
            this.lRobotStatus.AutoSize = true;
            this.lRobotStatus.Location = new System.Drawing.Point(16, 30);
            this.lRobotStatus.Name = "lRobotStatus";
            this.lRobotStatus.Size = new System.Drawing.Size(72, 13);
            this.lRobotStatus.TabIndex = 0;
            this.lRobotStatus.Text = "Robot Status:";
            // 
            // tRobotSpeed
            // 
            this.tRobotSpeed.Location = new System.Drawing.Point(329, 27);
            this.tRobotSpeed.Name = "tRobotSpeed";
            this.tRobotSpeed.ReadOnly = true;
            this.tRobotSpeed.Size = new System.Drawing.Size(90, 20);
            this.tRobotSpeed.TabIndex = 36;
            this.tRobotSpeed.Text = "N/A";
            // 
            // lRobotSpeed
            // 
            this.lRobotSpeed.AutoSize = true;
            this.lRobotSpeed.Location = new System.Drawing.Point(251, 30);
            this.lRobotSpeed.Name = "lRobotSpeed";
            this.lRobotSpeed.Size = new System.Drawing.Size(73, 13);
            this.lRobotSpeed.TabIndex = 35;
            this.lRobotSpeed.Text = "Robot Speed:";
            // 
            // RobotStatusCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gRobotStatus);
            this.Name = "RobotStatusCtrl";
            this.Size = new System.Drawing.Size(880, 350);
            this.gRobotStatus.ResumeLayout(false);
            this.gRobotStatus.PerformLayout();
            this.gPosition.ResumeLayout(false);
            this.gPosition.PerformLayout();
            this.tHand.ResumeLayout(false);
            this.tabH1.ResumeLayout(false);
            this.tabH1.PerformLayout();
            this.tabH2.ResumeLayout(false);
            this.tabH2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gRobotStatus;
        private System.Windows.Forms.Label lRobotStatus;
        private System.Windows.Forms.TabControl tHand;
        private System.Windows.Forms.TabPage tabH1;
        private System.Windows.Forms.Label lX1pos;
        private System.Windows.Forms.TabPage tabH2;
        private System.Windows.Forms.TextBox tY1pos;
        private System.Windows.Forms.Label lY1pos;
        private System.Windows.Forms.TextBox tX1pos;
        private System.Windows.Forms.TextBox tHA1;
        private System.Windows.Forms.Label lHA1;
        private System.Windows.Forms.TextBox tZ1pos;
        private System.Windows.Forms.Label lZ1;
        private System.Windows.Forms.TextBox tJ7pos;
        private System.Windows.Forms.Label lJ7pos;
        private System.Windows.Forms.TextBox tJ6pos;
        private System.Windows.Forms.Label lJ6pos;
        private System.Windows.Forms.TextBox tJ4pos;
        private System.Windows.Forms.Label lJ4pos;
        private System.Windows.Forms.TextBox tJ2pos;
        private System.Windows.Forms.Label lJ2;
        private LedBulb lbRobotStatus;
        private System.Windows.Forms.TextBox tRobotStatus;
        private System.Windows.Forms.Panel panel1;
        private BuldedStatus bsWaferOnH1;
        private System.Windows.Forms.TextBox tRobotMode;
        private System.Windows.Forms.Label lRobotMode;
        private BuldedStatus bsH1Chuck;
        private System.Windows.Forms.TextBox tRNST;
        private System.Windows.Forms.Label lRNST;
        private System.Windows.Forms.GroupBox gPosition;
        private BuldedStatus bsH2Chuck;
        private System.Windows.Forms.TextBox tHA2;
        private System.Windows.Forms.Label lX2pos;
        private System.Windows.Forms.Label lHA2;
        private System.Windows.Forms.TextBox tX2pos;
        private System.Windows.Forms.TextBox tZ2pos;
        private BuldedStatus bsWaferOnH2;
        private System.Windows.Forms.Label lY2pos;
        private System.Windows.Forms.Label lZ2pos;
        private System.Windows.Forms.TextBox tY2pos;
        private System.Windows.Forms.TextBox tRobotSpeed;
        private System.Windows.Forms.Label lRobotSpeed;
    }
}
