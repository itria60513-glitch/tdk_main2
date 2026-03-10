namespace EFEM.GUIControls.TeachControls
{
    partial class TeachAlignerCtrl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TeachAlignerCtrl));
            this.gAligner = new System.Windows.Forms.GroupBox();
            this.gAngle = new System.Windows.Forms.GroupBox();
            this.tabMotionType = new System.Windows.Forms.TabControl();
            this.tRelative = new System.Windows.Forms.TabPage();
            this.lnbStepAngle = new EFEM.GUIControls.LabeledNumBox();
            this.btnPos = new System.Windows.Forms.Button();
            this.btnNeg = new System.Windows.Forms.Button();
            this.tAbsolute = new System.Windows.Forms.TabPage();
            this.lnbABSAngle = new EFEM.GUIControls.LabeledNumBox();
            this.btnMovABS = new System.Windows.Forms.Button();
            this.btnRefreshAlnPos = new System.Windows.Forms.Button();
            this.lnbAlignAngle = new EFEM.GUIControls.LabeledNumBox();
            this.btnApplyAlignAngle = new System.Windows.Forms.Button();
            this.tCurrent = new System.Windows.Forms.TextBox();
            this.lCurrent = new System.Windows.Forms.Label();
            this.pIllustration = new System.Windows.Forms.Panel();
            this.shapeContainer1 = new Microsoft.VisualBasic.PowerPacks.ShapeContainer();
            this.lineShape4 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.lineShape3 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.gHome = new System.Windows.Forms.GroupBox();
            this.bsHome = new EFEM.GUIControls.BuldedStatus();
            this.btnHomeAligner = new System.Windows.Forms.Button();
            this.gSpeed = new System.Windows.Forms.GroupBox();
            this.lnbAlignerSpeed = new EFEM.GUIControls.LabeledNumBox();
            this.btnApplyAlnSpd = new System.Windows.Forms.Button();
            this.btnRefreshAlnSpd = new System.Windows.Forms.Button();
            this.gWafer = new System.Windows.Forms.GroupBox();
            this.cbHandSelection = new System.Windows.Forms.ComboBox();
            this.lHand = new System.Windows.Forms.Label();
            this.btnReAlign = new System.Windows.Forms.Button();
            this.btnHold = new System.Windows.Forms.Button();
            this.btnAlign = new System.Windows.Forms.Button();
            this.btnWfrPresence = new System.Windows.Forms.Button();
            this.btnRelease = new System.Windows.Forms.Button();
            this.bsWfrPresence = new EFEM.GUIControls.BuldedStatus();
            this.bsHoldwfr = new EFEM.GUIControls.BuldedStatus();
            this.shapeContainer2 = new Microsoft.VisualBasic.PowerPacks.ShapeContainer();
            this.lineShape2 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.lineShape1 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.gAlignerServo = new System.Windows.Forms.GroupBox();
            this.btnAlignerServo = new System.Windows.Forms.Button();
            this.bsAlignerServo = new EFEM.GUIControls.BuldedStatus();
            this.gAligner.SuspendLayout();
            this.gAngle.SuspendLayout();
            this.tabMotionType.SuspendLayout();
            this.tRelative.SuspendLayout();
            this.tAbsolute.SuspendLayout();
            this.gHome.SuspendLayout();
            this.gSpeed.SuspendLayout();
            this.gWafer.SuspendLayout();
            this.gAlignerServo.SuspendLayout();
            this.SuspendLayout();
            // 
            // gAligner
            // 
            this.gAligner.Controls.Add(this.gAngle);
            this.gAligner.Controls.Add(this.gHome);
            this.gAligner.Controls.Add(this.gSpeed);
            this.gAligner.Controls.Add(this.gWafer);
            this.gAligner.Controls.Add(this.gAlignerServo);
            this.gAligner.Location = new System.Drawing.Point(3, 3);
            this.gAligner.Name = "gAligner";
            this.gAligner.Size = new System.Drawing.Size(522, 394);
            this.gAligner.TabIndex = 0;
            this.gAligner.TabStop = false;
            this.gAligner.Text = "Aligner (J1)";
            // 
            // gAngle
            // 
            this.gAngle.Controls.Add(this.tabMotionType);
            this.gAngle.Controls.Add(this.btnRefreshAlnPos);
            this.gAngle.Controls.Add(this.lnbAlignAngle);
            this.gAngle.Controls.Add(this.btnApplyAlignAngle);
            this.gAngle.Controls.Add(this.tCurrent);
            this.gAngle.Controls.Add(this.lCurrent);
            this.gAngle.Controls.Add(this.pIllustration);
            this.gAngle.Controls.Add(this.shapeContainer1);
            this.gAngle.Location = new System.Drawing.Point(198, 19);
            this.gAngle.Name = "gAngle";
            this.gAngle.Size = new System.Drawing.Size(321, 284);
            this.gAngle.TabIndex = 45;
            this.gAngle.TabStop = false;
            this.gAngle.Text = "Angle";
            // 
            // tabMotionType
            // 
            this.tabMotionType.Controls.Add(this.tRelative);
            this.tabMotionType.Controls.Add(this.tAbsolute);
            this.tabMotionType.Location = new System.Drawing.Point(182, 140);
            this.tabMotionType.Name = "tabMotionType";
            this.tabMotionType.SelectedIndex = 0;
            this.tabMotionType.Size = new System.Drawing.Size(136, 137);
            this.tabMotionType.TabIndex = 47;
            // 
            // tRelative
            // 
            this.tRelative.Controls.Add(this.lnbStepAngle);
            this.tRelative.Controls.Add(this.btnPos);
            this.tRelative.Controls.Add(this.btnNeg);
            this.tRelative.Location = new System.Drawing.Point(4, 22);
            this.tRelative.Name = "tRelative";
            this.tRelative.Padding = new System.Windows.Forms.Padding(3);
            this.tRelative.Size = new System.Drawing.Size(128, 111);
            this.tRelative.TabIndex = 0;
            this.tRelative.Text = "Relative";
            this.tRelative.UseVisualStyleBackColor = true;
            // 
            // lnbStepAngle
            // 
            this.lnbStepAngle.Caption = "Unit [degree]";
            this.lnbStepAngle.DecimalPlaces = 2;
            this.lnbStepAngle.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.lnbStepAngle.Location = new System.Drawing.Point(8, 16);
            this.lnbStepAngle.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            131072});
            this.lnbStepAngle.Minimum = new decimal(new int[] {
            999999,
            0,
            0,
            -2147352576});
            this.lnbStepAngle.Name = "lnbStepAngle";
            this.lnbStepAngle.Size = new System.Drawing.Size(112, 34);
            this.lnbStepAngle.TabIndex = 3;
            this.lnbStepAngle.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // btnPos
            // 
            this.btnPos.Image = ((System.Drawing.Image)(resources.GetObject("btnPos.Image")));
            this.btnPos.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnPos.Location = new System.Drawing.Point(67, 63);
            this.btnPos.Name = "btnPos";
            this.btnPos.Size = new System.Drawing.Size(58, 38);
            this.btnPos.TabIndex = 48;
            this.btnPos.Text = "Positive";
            this.btnPos.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnPos.UseVisualStyleBackColor = true;
            this.btnPos.Click += new System.EventHandler(this.btnPos_Click);
            // 
            // btnNeg
            // 
            this.btnNeg.Image = global::EFEM.GUIControls.Properties.Resources.minus;
            this.btnNeg.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnNeg.Location = new System.Drawing.Point(6, 64);
            this.btnNeg.Name = "btnNeg";
            this.btnNeg.Size = new System.Drawing.Size(58, 38);
            this.btnNeg.TabIndex = 1;
            this.btnNeg.Text = "Negative";
            this.btnNeg.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnNeg.UseVisualStyleBackColor = true;
            this.btnNeg.Click += new System.EventHandler(this.btnNeg_Click);
            // 
            // tAbsolute
            // 
            this.tAbsolute.Controls.Add(this.lnbABSAngle);
            this.tAbsolute.Controls.Add(this.btnMovABS);
            this.tAbsolute.Location = new System.Drawing.Point(4, 22);
            this.tAbsolute.Name = "tAbsolute";
            this.tAbsolute.Padding = new System.Windows.Forms.Padding(3);
            this.tAbsolute.Size = new System.Drawing.Size(128, 111);
            this.tAbsolute.TabIndex = 1;
            this.tAbsolute.Text = "Absolute";
            this.tAbsolute.UseVisualStyleBackColor = true;
            // 
            // lnbABSAngle
            // 
            this.lnbABSAngle.Caption = "Unit [degree]";
            this.lnbABSAngle.DecimalPlaces = 2;
            this.lnbABSAngle.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.lnbABSAngle.Location = new System.Drawing.Point(8, 16);
            this.lnbABSAngle.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            131072});
            this.lnbABSAngle.Minimum = new decimal(new int[] {
            999999,
            0,
            0,
            -2147352576});
            this.lnbABSAngle.Name = "lnbABSAngle";
            this.lnbABSAngle.Size = new System.Drawing.Size(112, 34);
            this.lnbABSAngle.TabIndex = 50;
            this.lnbABSAngle.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // btnMovABS
            // 
            this.btnMovABS.Location = new System.Drawing.Point(27, 67);
            this.btnMovABS.Name = "btnMovABS";
            this.btnMovABS.Size = new System.Drawing.Size(58, 38);
            this.btnMovABS.TabIndex = 47;
            this.btnMovABS.Text = "Move";
            this.btnMovABS.UseVisualStyleBackColor = true;
            this.btnMovABS.Click += new System.EventHandler(this.btnMovABS_Click);
            // 
            // btnRefreshAlnPos
            // 
            this.btnRefreshAlnPos.Image = global::EFEM.GUIControls.Properties.Resources.refresh;
            this.btnRefreshAlnPos.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnRefreshAlnPos.Location = new System.Drawing.Point(213, 11);
            this.btnRefreshAlnPos.Name = "btnRefreshAlnPos";
            this.btnRefreshAlnPos.Size = new System.Drawing.Size(58, 40);
            this.btnRefreshAlnPos.TabIndex = 48;
            this.btnRefreshAlnPos.Text = "Refresh";
            this.btnRefreshAlnPos.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnRefreshAlnPos.UseVisualStyleBackColor = true;
            this.btnRefreshAlnPos.Click += new System.EventHandler(this.btnRefreshAlnPos_Click);
            // 
            // lnbAlignAngle
            // 
            this.lnbAlignAngle.Caption = "Align Angle";
            this.lnbAlignAngle.DecimalPlaces = 2;
            this.lnbAlignAngle.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.lnbAlignAngle.Location = new System.Drawing.Point(21, 60);
            this.lnbAlignAngle.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            131072});
            this.lnbAlignAngle.Minimum = new decimal(new int[] {
            999999,
            0,
            0,
            -2147352576});
            this.lnbAlignAngle.Name = "lnbAlignAngle";
            this.lnbAlignAngle.Size = new System.Drawing.Size(177, 34);
            this.lnbAlignAngle.TabIndex = 48;
            this.lnbAlignAngle.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // btnApplyAlignAngle
            // 
            this.btnApplyAlignAngle.Image = global::EFEM.GUIControls.Properties.Resources.apply;
            this.btnApplyAlignAngle.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnApplyAlignAngle.Location = new System.Drawing.Point(213, 60);
            this.btnApplyAlignAngle.Name = "btnApplyAlignAngle";
            this.btnApplyAlignAngle.Size = new System.Drawing.Size(58, 40);
            this.btnApplyAlignAngle.TabIndex = 30;
            this.btnApplyAlignAngle.Text = "Apply";
            this.btnApplyAlignAngle.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnApplyAlignAngle.UseVisualStyleBackColor = true;
            this.btnApplyAlignAngle.Click += new System.EventHandler(this.btnApplyAlignAngle_Click);
            // 
            // tCurrent
            // 
            this.tCurrent.Location = new System.Drawing.Point(105, 21);
            this.tCurrent.Name = "tCurrent";
            this.tCurrent.ReadOnly = true;
            this.tCurrent.Size = new System.Drawing.Size(90, 20);
            this.tCurrent.TabIndex = 46;
            // 
            // lCurrent
            // 
            this.lCurrent.AutoSize = true;
            this.lCurrent.Location = new System.Drawing.Point(18, 21);
            this.lCurrent.Name = "lCurrent";
            this.lCurrent.Size = new System.Drawing.Size(74, 13);
            this.lCurrent.TabIndex = 42;
            this.lCurrent.Text = "Current Angle:";
            // 
            // pIllustration
            // 
            this.pIllustration.BackColor = System.Drawing.Color.White;
            this.pIllustration.BackgroundImage = global::EFEM.GUIControls.Properties.Resources.Aligner;
            this.pIllustration.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pIllustration.Location = new System.Drawing.Point(7, 109);
            this.pIllustration.Name = "pIllustration";
            this.pIllustration.Size = new System.Drawing.Size(170, 170);
            this.pIllustration.TabIndex = 43;
            // 
            // shapeContainer1
            // 
            this.shapeContainer1.Location = new System.Drawing.Point(3, 16);
            this.shapeContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.shapeContainer1.Name = "shapeContainer1";
            this.shapeContainer1.Shapes.AddRange(new Microsoft.VisualBasic.PowerPacks.Shape[] {
            this.lineShape4,
            this.lineShape3});
            this.shapeContainer1.Size = new System.Drawing.Size(315, 265);
            this.shapeContainer1.TabIndex = 49;
            this.shapeContainer1.TabStop = false;
            // 
            // lineShape4
            // 
            this.lineShape4.BorderColor = System.Drawing.Color.LightGray;
            this.lineShape4.Name = "lineShape4";
            this.lineShape4.X1 = 3;
            this.lineShape4.X2 = 397;
            this.lineShape4.Y1 = 87;
            this.lineShape4.Y2 = 85;
            // 
            // lineShape3
            // 
            this.lineShape3.BorderColor = System.Drawing.Color.LightGray;
            this.lineShape3.Name = "lineShape3";
            this.lineShape3.X1 = 1;
            this.lineShape3.X2 = 395;
            this.lineShape3.Y1 = 38;
            this.lineShape3.Y2 = 36;
            // 
            // gHome
            // 
            this.gHome.Controls.Add(this.bsHome);
            this.gHome.Controls.Add(this.btnHomeAligner);
            this.gHome.Location = new System.Drawing.Point(6, 332);
            this.gHome.Name = "gHome";
            this.gHome.Size = new System.Drawing.Size(189, 55);
            this.gHome.TabIndex = 36;
            this.gHome.TabStop = false;
            this.gHome.Text = "Home";
            // 
            // bsHome
            // 
            this.bsHome.Caption = "Home";
            this.bsHome.Location = new System.Drawing.Point(17, 19);
            this.bsHome.Name = "bsHome";
            this.bsHome.On = false;
            this.bsHome.Size = new System.Drawing.Size(95, 26);
            this.bsHome.TabIndex = 37;
            // 
            // btnHomeAligner
            // 
            this.btnHomeAligner.Image = global::EFEM.GUIControls.Properties.Resources.home;
            this.btnHomeAligner.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnHomeAligner.Location = new System.Drawing.Point(125, 10);
            this.btnHomeAligner.Name = "btnHomeAligner";
            this.btnHomeAligner.Size = new System.Drawing.Size(58, 40);
            this.btnHomeAligner.TabIndex = 36;
            this.btnHomeAligner.Text = "Home";
            this.btnHomeAligner.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnHomeAligner.UseVisualStyleBackColor = true;
            this.btnHomeAligner.Click += new System.EventHandler(this.btnHomeAligner_Click);
            // 
            // gSpeed
            // 
            this.gSpeed.Controls.Add(this.lnbAlignerSpeed);
            this.gSpeed.Controls.Add(this.btnApplyAlnSpd);
            this.gSpeed.Controls.Add(this.btnRefreshAlnSpd);
            this.gSpeed.Location = new System.Drawing.Point(201, 304);
            this.gSpeed.Name = "gSpeed";
            this.gSpeed.Size = new System.Drawing.Size(314, 84);
            this.gSpeed.TabIndex = 40;
            this.gSpeed.TabStop = false;
            this.gSpeed.Text = "Motion Speed";
            // 
            // lnbAlignerSpeed
            // 
            this.lnbAlignerSpeed.Caption = "Speed (%)";
            this.lnbAlignerSpeed.DecimalPlaces = 2;
            this.lnbAlignerSpeed.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.lnbAlignerSpeed.Location = new System.Drawing.Point(14, 19);
            this.lnbAlignerSpeed.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.lnbAlignerSpeed.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.lnbAlignerSpeed.Name = "lnbAlignerSpeed";
            this.lnbAlignerSpeed.Size = new System.Drawing.Size(172, 34);
            this.lnbAlignerSpeed.TabIndex = 47;
            this.lnbAlignerSpeed.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // btnApplyAlnSpd
            // 
            this.btnApplyAlnSpd.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnApplyAlnSpd.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnApplyAlnSpd.Image = global::EFEM.GUIControls.Properties.Resources.apply;
            this.btnApplyAlnSpd.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnApplyAlnSpd.Location = new System.Drawing.Point(250, 19);
            this.btnApplyAlnSpd.Name = "btnApplyAlnSpd";
            this.btnApplyAlnSpd.Size = new System.Drawing.Size(58, 40);
            this.btnApplyAlnSpd.TabIndex = 39;
            this.btnApplyAlnSpd.Text = "Apply";
            this.btnApplyAlnSpd.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnApplyAlnSpd.UseVisualStyleBackColor = true;
            this.btnApplyAlnSpd.Click += new System.EventHandler(this.btnApplyAlnSpd_Click);
            // 
            // btnRefreshAlnSpd
            // 
            this.btnRefreshAlnSpd.Image = global::EFEM.GUIControls.Properties.Resources.refresh;
            this.btnRefreshAlnSpd.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnRefreshAlnSpd.Location = new System.Drawing.Point(189, 19);
            this.btnRefreshAlnSpd.Name = "btnRefreshAlnSpd";
            this.btnRefreshAlnSpd.Size = new System.Drawing.Size(58, 40);
            this.btnRefreshAlnSpd.TabIndex = 24;
            this.btnRefreshAlnSpd.Text = "Refresh";
            this.btnRefreshAlnSpd.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnRefreshAlnSpd.UseVisualStyleBackColor = true;
            this.btnRefreshAlnSpd.Click += new System.EventHandler(this.btnRefreshAlnSpd_Click);
            // 
            // gWafer
            // 
            this.gWafer.Controls.Add(this.cbHandSelection);
            this.gWafer.Controls.Add(this.lHand);
            this.gWafer.Controls.Add(this.btnReAlign);
            this.gWafer.Controls.Add(this.btnHold);
            this.gWafer.Controls.Add(this.btnAlign);
            this.gWafer.Controls.Add(this.btnWfrPresence);
            this.gWafer.Controls.Add(this.btnRelease);
            this.gWafer.Controls.Add(this.bsWfrPresence);
            this.gWafer.Controls.Add(this.bsHoldwfr);
            this.gWafer.Controls.Add(this.shapeContainer2);
            this.gWafer.Location = new System.Drawing.Point(6, 19);
            this.gWafer.Name = "gWafer";
            this.gWafer.Size = new System.Drawing.Size(189, 246);
            this.gWafer.TabIndex = 32;
            this.gWafer.TabStop = false;
            this.gWafer.Text = "Wafer Operation";
            // 
            // cbHandSelection
            // 
            this.cbHandSelection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbHandSelection.FormattingEnabled = true;
            this.cbHandSelection.Items.AddRange(new object[] {
            "Lower Hand H1",
            "Upper Hand H2"});
            this.cbHandSelection.Location = new System.Drawing.Point(10, 213);
            this.cbHandSelection.Name = "cbHandSelection";
            this.cbHandSelection.Size = new System.Drawing.Size(109, 21);
            this.cbHandSelection.TabIndex = 34;
            // 
            // lHand
            // 
            this.lHand.AutoSize = true;
            this.lHand.Location = new System.Drawing.Point(43, 197);
            this.lHand.Name = "lHand";
            this.lHand.Size = new System.Drawing.Size(36, 13);
            this.lHand.TabIndex = 33;
            this.lHand.Text = "Hand:";
            // 
            // btnReAlign
            // 
            this.btnReAlign.Location = new System.Drawing.Point(125, 205);
            this.btnReAlign.Name = "btnReAlign";
            this.btnReAlign.Size = new System.Drawing.Size(58, 35);
            this.btnReAlign.TabIndex = 7;
            this.btnReAlign.Text = "Re-Align";
            this.btnReAlign.UseVisualStyleBackColor = true;
            this.btnReAlign.Click += new System.EventHandler(this.btnReAlign_Click);
            // 
            // btnHold
            // 
            this.btnHold.Location = new System.Drawing.Point(124, 68);
            this.btnHold.Name = "btnHold";
            this.btnHold.Size = new System.Drawing.Size(58, 35);
            this.btnHold.TabIndex = 21;
            this.btnHold.Text = "Hold";
            this.btnHold.UseVisualStyleBackColor = true;
            this.btnHold.Click += new System.EventHandler(this.btnHold_Click);
            // 
            // btnAlign
            // 
            this.btnAlign.Location = new System.Drawing.Point(125, 164);
            this.btnAlign.Name = "btnAlign";
            this.btnAlign.Size = new System.Drawing.Size(58, 35);
            this.btnAlign.TabIndex = 7;
            this.btnAlign.Text = "Align";
            this.btnAlign.UseVisualStyleBackColor = true;
            this.btnAlign.Click += new System.EventHandler(this.btnAlign_Click);
            // 
            // btnWfrPresence
            // 
            this.btnWfrPresence.Location = new System.Drawing.Point(124, 12);
            this.btnWfrPresence.Name = "btnWfrPresence";
            this.btnWfrPresence.Size = new System.Drawing.Size(58, 35);
            this.btnWfrPresence.TabIndex = 30;
            this.btnWfrPresence.Text = "Report";
            this.btnWfrPresence.UseVisualStyleBackColor = true;
            this.btnWfrPresence.Click += new System.EventHandler(this.btnWfrPresence_Click);
            // 
            // btnRelease
            // 
            this.btnRelease.Location = new System.Drawing.Point(125, 109);
            this.btnRelease.Name = "btnRelease";
            this.btnRelease.Size = new System.Drawing.Size(58, 35);
            this.btnRelease.TabIndex = 22;
            this.btnRelease.Text = "Release";
            this.btnRelease.UseVisualStyleBackColor = true;
            this.btnRelease.Click += new System.EventHandler(this.btnRelease_Click);
            // 
            // bsWfrPresence
            // 
            this.bsWfrPresence.Caption = "Wafer Presence";
            this.bsWfrPresence.Location = new System.Drawing.Point(10, 19);
            this.bsWfrPresence.Name = "bsWfrPresence";
            this.bsWfrPresence.On = false;
            this.bsWfrPresence.Size = new System.Drawing.Size(112, 26);
            this.bsWfrPresence.TabIndex = 31;
            // 
            // bsHoldwfr
            // 
            this.bsHoldwfr.Caption = "Hold Wafer";
            this.bsHoldwfr.Location = new System.Drawing.Point(10, 82);
            this.bsHoldwfr.Name = "bsHoldwfr";
            this.bsHoldwfr.On = false;
            this.bsHoldwfr.Size = new System.Drawing.Size(112, 26);
            this.bsHoldwfr.TabIndex = 23;
            // 
            // shapeContainer2
            // 
            this.shapeContainer2.Location = new System.Drawing.Point(3, 16);
            this.shapeContainer2.Margin = new System.Windows.Forms.Padding(0);
            this.shapeContainer2.Name = "shapeContainer2";
            this.shapeContainer2.Shapes.AddRange(new Microsoft.VisualBasic.PowerPacks.Shape[] {
            this.lineShape2,
            this.lineShape1});
            this.shapeContainer2.Size = new System.Drawing.Size(183, 227);
            this.shapeContainer2.TabIndex = 35;
            this.shapeContainer2.TabStop = false;
            // 
            // lineShape2
            // 
            this.lineShape2.BorderColor = System.Drawing.Color.LightGray;
            this.lineShape2.Name = "lineShape2";
            this.lineShape2.X1 = 1;
            this.lineShape2.X2 = 195;
            this.lineShape2.Y1 = 137;
            this.lineShape2.Y2 = 137;
            // 
            // lineShape1
            // 
            this.lineShape1.BorderColor = System.Drawing.Color.LightGray;
            this.lineShape1.Name = "lineShape1";
            this.lineShape1.X1 = 2;
            this.lineShape1.X2 = 190;
            this.lineShape1.Y1 = 38;
            this.lineShape1.Y2 = 38;
            // 
            // gAlignerServo
            // 
            this.gAlignerServo.Controls.Add(this.btnAlignerServo);
            this.gAlignerServo.Controls.Add(this.bsAlignerServo);
            this.gAlignerServo.Location = new System.Drawing.Point(6, 271);
            this.gAlignerServo.Name = "gAlignerServo";
            this.gAlignerServo.Size = new System.Drawing.Size(189, 55);
            this.gAlignerServo.TabIndex = 35;
            this.gAlignerServo.TabStop = false;
            this.gAlignerServo.Text = "Servo";
            // 
            // btnAlignerServo
            // 
            this.btnAlignerServo.Location = new System.Drawing.Point(125, 11);
            this.btnAlignerServo.Name = "btnAlignerServo";
            this.btnAlignerServo.Size = new System.Drawing.Size(58, 35);
            this.btnAlignerServo.TabIndex = 28;
            this.btnAlignerServo.Text = "ON";
            this.btnAlignerServo.UseVisualStyleBackColor = true;
            this.btnAlignerServo.Click += new System.EventHandler(this.btnAlignerServo_Click);
            // 
            // bsAlignerServo
            // 
            this.bsAlignerServo.Caption = "Servo";
            this.bsAlignerServo.Location = new System.Drawing.Point(17, 20);
            this.bsAlignerServo.Name = "bsAlignerServo";
            this.bsAlignerServo.On = false;
            this.bsAlignerServo.Size = new System.Drawing.Size(95, 26);
            this.bsAlignerServo.TabIndex = 21;
            // 
            // TeachAlignerCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gAligner);
            this.Name = "TeachAlignerCtrl";
            this.Size = new System.Drawing.Size(525, 404);
            this.gAligner.ResumeLayout(false);
            this.gAngle.ResumeLayout(false);
            this.gAngle.PerformLayout();
            this.tabMotionType.ResumeLayout(false);
            this.tRelative.ResumeLayout(false);
            this.tAbsolute.ResumeLayout(false);
            this.gHome.ResumeLayout(false);
            this.gSpeed.ResumeLayout(false);
            this.gWafer.ResumeLayout(false);
            this.gWafer.PerformLayout();
            this.gAlignerServo.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gAligner;
        private System.Windows.Forms.Button btnReAlign;
        private System.Windows.Forms.Button btnAlign;
        private System.Windows.Forms.Button btnRefreshAlnSpd;
        private System.Windows.Forms.Button btnRelease;
        private BuldedStatus bsHoldwfr;
        private System.Windows.Forms.Button btnHold;
        private BuldedStatus bsWfrPresence;
        private System.Windows.Forms.Button btnWfrPresence;
        private System.Windows.Forms.GroupBox gWafer;
        private System.Windows.Forms.GroupBox gAlignerServo;
        private System.Windows.Forms.Button btnAlignerServo;
        private BuldedStatus bsAlignerServo;
        private System.Windows.Forms.Button btnApplyAlignAngle;
        private System.Windows.Forms.Button btnHomeAligner;
        private BuldedStatus bsHome;
        private System.Windows.Forms.Button btnApplyAlnSpd;
        private System.Windows.Forms.GroupBox gSpeed;
        private System.Windows.Forms.TextBox tCurrent;
        private System.Windows.Forms.GroupBox gAngle;
        private LabeledNumBox lnbStepAngle;
        private System.Windows.Forms.Button btnNeg;
        private System.Windows.Forms.Label lCurrent;
        private System.Windows.Forms.Panel pIllustration;
        private System.Windows.Forms.Button btnMovABS;
        private System.Windows.Forms.GroupBox gHome;
        private LabeledNumBox lnbAlignerSpeed;
        private System.Windows.Forms.Button btnPos;
        private System.Windows.Forms.ComboBox cbHandSelection;
        private System.Windows.Forms.Label lHand;
        private System.Windows.Forms.TabControl tabMotionType;
        private System.Windows.Forms.TabPage tRelative;
        private System.Windows.Forms.TabPage tAbsolute;
        private System.Windows.Forms.Button btnRefreshAlnPos;
        private LabeledNumBox lnbAlignAngle;
        private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer1;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape3;
        private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer2;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape2;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape1;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape4;
        private LabeledNumBox lnbABSAngle;
    }
}
