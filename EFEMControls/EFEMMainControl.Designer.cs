namespace EFEM.GUIControls
{
    partial class EFEMMainControl
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
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lActiveAlarm = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusAlarmCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusEmpty = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusIndInit = new EFEM.GUIControls.ToolStripStatusIndication();
            this.StatusIndHost = new EFEM.GUIControls.ToolStripStatusIndication();
            this.StatusIndRobot = new EFEM.GUIControls.ToolStripStatusIndication();
            this.StatusIndAligner = new EFEM.GUIControls.ToolStripStatusIndication();
            this.StatusIndDIO0 = new EFEM.GUIControls.ToolStripStatusIndication();
            this.StatusIndFFU = new EFEM.GUIControls.ToolStripStatusIndication();
            this.lWaferSensing = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusWaferSensing = new System.Windows.Forms.ToolStripStatusLabel();
            this.lEFEMStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusEFEM = new System.Windows.Forms.ToolStripStatusLabel();
            this.lOnOffline = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusOnline = new System.Windows.Forms.ToolStripStatusLabel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.tlpInformation = new System.Windows.Forms.TableLayoutPanel();
            this.statusMessageCtrl = new EFEM.GUIControls.StatusMessageCtrl();
            this.panelUser = new System.Windows.Forms.Panel();
            this.lLoginStatus = new System.Windows.Forms.Label();
            this.btnLockSystem = new System.Windows.Forms.Button();
            this.btnUserLogOut = new System.Windows.Forms.Button();
            this.btnUserLogin = new System.Windows.Forms.Button();
            this.lUserTitle = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabPageInit = new System.Windows.Forms.TabPage();
            this.startupController = new EFEM.GUIControls.StartupComponentCtrl();
            this.tabPageOperation = new System.Windows.Forms.TabPage();
            this.tabOperation = new System.Windows.Forms.TabControl();
            this.tabStatus = new System.Windows.Forms.TabPage();
            this.efemStatusCtrl = new EFEM.GUIControls.EFEMStatusCtrl();
            this.tabConfiguration = new System.Windows.Forms.TabPage();
            this.tdkConfigControl1 = new EFEM.GUIControls.TDKConfigControl();
            this.statusStrip1.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.tlpInformation.SuspendLayout();
            this.panelUser.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tabMain.SuspendLayout();
            this.tabPageInit.SuspendLayout();
            this.tabPageOperation.SuspendLayout();
            this.tabOperation.SuspendLayout();
            this.tabStatus.SuspendLayout();
            this.tabConfiguration.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lActiveAlarm,
            this.StatusAlarmCount,
            this.StatusEmpty,
            this.StatusIndInit,
            this.StatusIndHost,
            this.StatusIndRobot,
            this.StatusIndAligner,
            this.StatusIndDIO0,
            this.StatusIndFFU,
            this.lWaferSensing,
            this.StatusWaferSensing,
            this.lEFEMStatus,
            this.StatusEFEM,
            this.lOnOffline,
            this.StatusOnline});
            this.statusStrip1.Location = new System.Drawing.Point(0, 641);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.ShowItemToolTips = true;
            this.statusStrip1.Size = new System.Drawing.Size(1280, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lActiveAlarm
            // 
            this.lActiveAlarm.Name = "lActiveAlarm";
            this.lActiveAlarm.Size = new System.Drawing.Size(78, 17);
            this.lActiveAlarm.Text = "Active Alarm:";
            // 
            // StatusAlarmCount
            // 
            this.StatusAlarmCount.AutoSize = false;
            this.StatusAlarmCount.BackColor = System.Drawing.Color.Green;
            this.StatusAlarmCount.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.StatusAlarmCount.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)));
            this.StatusAlarmCount.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this.StatusAlarmCount.ForeColor = System.Drawing.Color.White;
            this.StatusAlarmCount.Name = "StatusAlarmCount";
            this.StatusAlarmCount.Size = new System.Drawing.Size(40, 17);
            this.StatusAlarmCount.Text = "0";
            this.StatusAlarmCount.DoubleClick += new System.EventHandler(this.StatusAlarmCount_DoubleClick);
            // 
            // StatusEmpty
            // 
            this.StatusEmpty.Name = "StatusEmpty";
            this.StatusEmpty.Size = new System.Drawing.Size(109, 17);
            this.StatusEmpty.Spring = true;
            // 
            // StatusIndInit
            // 
            this.StatusIndInit.Name = "StatusIndInit";
            this.StatusIndInit.Size = new System.Drawing.Size(93, 20);
            this.StatusIndInit.Status = EFEM.GUIControls.StatusType.Error;
            this.StatusIndInit.Text = "Initialization";
            // 
            // StatusIndHost
            // 
            this.StatusIndHost.MergeIndex = 0;
            this.StatusIndHost.Name = "StatusIndHost";
            this.StatusIndHost.Size = new System.Drawing.Size(89, 20);
            this.StatusIndHost.Status = EFEM.GUIControls.StatusType.Stop;
            this.StatusIndHost.Text = "Host Status";
            // 
            // StatusIndRobot
            // 
            this.StatusIndRobot.MergeIndex = 0;
            this.StatusIndRobot.Name = "StatusIndRobot";
            this.StatusIndRobot.Size = new System.Drawing.Size(96, 20);
            this.StatusIndRobot.Status = EFEM.GUIControls.StatusType.Stop;
            this.StatusIndRobot.Text = "Robot Status";
            // 
            // StatusIndAligner
            // 
            this.StatusIndAligner.MergeIndex = 0;
            this.StatusIndAligner.Name = "StatusIndAligner";
            this.StatusIndAligner.Size = new System.Drawing.Size(102, 20);
            this.StatusIndAligner.Status = EFEM.GUIControls.StatusType.Stop;
            this.StatusIndAligner.Text = "Aligner Status";
            // 
            // StatusIndDIO0
            // 
            this.StatusIndDIO0.MergeIndex = 0;
            this.StatusIndDIO0.Name = "StatusIndDIO0";
            this.StatusIndDIO0.Size = new System.Drawing.Size(76, 20);
            this.StatusIndDIO0.Status = EFEM.GUIControls.StatusType.Stop;
            this.StatusIndDIO0.Text = "IO Status";
            // 
            // StatusIndFFU
            // 
            this.StatusIndFFU.MergeIndex = 0;
            this.StatusIndFFU.Name = "StatusIndFFU";
            this.StatusIndFFU.Size = new System.Drawing.Size(84, 20);
            this.StatusIndFFU.Status = EFEM.GUIControls.StatusType.Stop;
            this.StatusIndFFU.Text = "FFU Status";
            // 
            // lWaferSensing
            // 
            this.lWaferSensing.Name = "lWaferSensing";
            this.lWaferSensing.Size = new System.Drawing.Size(85, 17);
            this.lWaferSensing.Text = "Wafer Sensing:";
            // 
            // StatusWaferSensing
            // 
            this.StatusWaferSensing.AutoSize = false;
            this.StatusWaferSensing.BackColor = System.Drawing.Color.Red;
            this.StatusWaferSensing.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.StatusWaferSensing.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)));
            this.StatusWaferSensing.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this.StatusWaferSensing.ForeColor = System.Drawing.Color.White;
            this.StatusWaferSensing.Name = "StatusWaferSensing";
            this.StatusWaferSensing.Size = new System.Drawing.Size(90, 17);
            this.StatusWaferSensing.Text = "<Unknown>";
            // 
            // lEFEMStatus
            // 
            this.lEFEMStatus.Name = "lEFEMStatus";
            this.lEFEMStatus.Size = new System.Drawing.Size(74, 17);
            this.lEFEMStatus.Text = "EFEM Status:";
            // 
            // StatusEFEM
            // 
            this.StatusEFEM.AutoSize = false;
            this.StatusEFEM.BackColor = System.Drawing.Color.Red;
            this.StatusEFEM.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.StatusEFEM.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)));
            this.StatusEFEM.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this.StatusEFEM.ForeColor = System.Drawing.Color.White;
            this.StatusEFEM.Name = "StatusEFEM";
            this.StatusEFEM.Size = new System.Drawing.Size(90, 17);
            this.StatusEFEM.Text = "<Unknown>";
            // 
            // lOnOffline
            // 
            this.lOnOffline.Name = "lOnOffline";
            this.lOnOffline.Size = new System.Drawing.Size(69, 17);
            this.lOnOffline.Text = "Host Mode:";
            // 
            // StatusOnline
            // 
            this.StatusOnline.AutoSize = false;
            this.StatusOnline.BackColor = System.Drawing.Color.Red;
            this.StatusOnline.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.StatusOnline.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)));
            this.StatusOnline.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this.StatusOnline.DoubleClickEnabled = true;
            this.StatusOnline.ForeColor = System.Drawing.Color.White;
            this.StatusOnline.Name = "StatusOnline";
            this.StatusOnline.Size = new System.Drawing.Size(90, 17);
            this.StatusOnline.Text = "Offline";
            // 
            // panelBottom
            // 
            this.panelBottom.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelBottom.Controls.Add(this.tlpInformation);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 525);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(1280, 116);
            this.panelBottom.TabIndex = 0;
            // 
            // tlpInformation
            // 
            this.tlpInformation.ColumnCount = 5;
            this.tlpInformation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 300F));
            this.tlpInformation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 250F));
            this.tlpInformation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpInformation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpInformation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpInformation.Controls.Add(this.statusMessageCtrl, 3, 0);
            this.tlpInformation.Controls.Add(this.panelUser, 1, 0);
            this.tlpInformation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpInformation.Location = new System.Drawing.Point(0, 0);
            this.tlpInformation.Name = "tlpInformation";
            this.tlpInformation.RowCount = 1;
            this.tlpInformation.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpInformation.Size = new System.Drawing.Size(1276, 112);
            this.tlpInformation.TabIndex = 0;
            // 
            // statusMessageCtrl
            // 
            this.statusMessageCtrl.Caption = "Status Message";
            this.statusMessageCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statusMessageCtrl.IsPausing = false;
            this.statusMessageCtrl.Location = new System.Drawing.Point(553, 3);
            this.statusMessageCtrl.Name = "statusMessageCtrl";
            this.statusMessageCtrl.Size = new System.Drawing.Size(680, 106);
            this.statusMessageCtrl.TabIndex = 1;
            // 
            // panelUser
            // 
            this.panelUser.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelUser.Controls.Add(this.lLoginStatus);
            this.panelUser.Controls.Add(this.btnLockSystem);
            this.panelUser.Controls.Add(this.btnUserLogOut);
            this.panelUser.Controls.Add(this.btnUserLogin);
            this.panelUser.Controls.Add(this.lUserTitle);
            this.panelUser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelUser.Location = new System.Drawing.Point(303, 3);
            this.panelUser.Name = "panelUser";
            this.panelUser.Size = new System.Drawing.Size(244, 106);
            this.panelUser.TabIndex = 3;
            // 
            // lLoginStatus
            // 
            this.lLoginStatus.AutoSize = true;
            this.lLoginStatus.ForeColor = System.Drawing.Color.Red;
            this.lLoginStatus.Location = new System.Drawing.Point(10, 85);
            this.lLoginStatus.Name = "lLoginStatus";
            this.lLoginStatus.Size = new System.Drawing.Size(118, 13);
            this.lLoginStatus.TabIndex = 4;
            this.lLoginStatus.Text = "Status: <Not logged in>";
            // 
            // btnLockSystem
            // 
            this.btnLockSystem.Image = global::EFEM.GUIControls.Properties.Resources.Lock;
            this.btnLockSystem.Location = new System.Drawing.Point(127, 27);
            this.btnLockSystem.Name = "btnLockSystem";
            this.btnLockSystem.Size = new System.Drawing.Size(112, 49);
            this.btnLockSystem.TabIndex = 3;
            this.btnLockSystem.Text = "Lock GUI";
            this.btnLockSystem.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnLockSystem.UseVisualStyleBackColor = true;
            this.btnLockSystem.Click += new System.EventHandler(this.btnLockSystem_Click);
            this.btnLockSystem.Visible = false;
            // 
            // btnUserLogOut
            // 
            this.btnUserLogOut.Location = new System.Drawing.Point(7, 53);
            this.btnUserLogOut.Name = "btnUserLogOut";
            this.btnUserLogOut.Size = new System.Drawing.Size(114, 23);
            this.btnUserLogOut.TabIndex = 2;
            this.btnUserLogOut.Text = "Logout";
            this.btnUserLogOut.UseVisualStyleBackColor = true;
            this.btnUserLogOut.Click += new System.EventHandler(this.btnUserLogOut_Click);
            this.btnUserLogOut.Visible = false;
            // 
            // btnUserLogin
            // 
            this.btnUserLogin.Location = new System.Drawing.Point(7, 27);
            this.btnUserLogin.Name = "btnUserLogin";
            this.btnUserLogin.Size = new System.Drawing.Size(114, 23);
            this.btnUserLogin.TabIndex = 1;
            this.btnUserLogin.Text = "Login";
            this.btnUserLogin.UseVisualStyleBackColor = true;
            this.btnUserLogin.Click += new System.EventHandler(this.btnUserLogin_Click);
            this.btnUserLogin.Visible = false;
            // 
            // lUserTitle
            // 
            this.lUserTitle.BackColor = System.Drawing.Color.RoyalBlue;
            this.lUserTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lUserTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lUserTitle.ForeColor = System.Drawing.Color.White;
            this.lUserTitle.Location = new System.Drawing.Point(0, 0);
            this.lUserTitle.Name = "lUserTitle";
            this.lUserTitle.Size = new System.Drawing.Size(242, 24);
            this.lUserTitle.TabIndex = 0;
            this.lUserTitle.Text = "Account Control";
            this.lUserTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.tabMain);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1280, 525);
            this.panel2.TabIndex = 2;
            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.tabPageInit);
            this.tabMain.Controls.Add(this.tabPageOperation);
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.Location = new System.Drawing.Point(0, 0);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(1280, 525);
            this.tabMain.TabIndex = 0;
            // 
            // tabPageInit
            // 
            this.tabPageInit.Controls.Add(this.startupController);
            this.tabPageInit.Location = new System.Drawing.Point(4, 22);
            this.tabPageInit.Name = "tabPageInit";
            this.tabPageInit.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageInit.Size = new System.Drawing.Size(1272, 499);
            this.tabPageInit.TabIndex = 0;
            this.tabPageInit.Text = "Startup";
            this.tabPageInit.UseVisualStyleBackColor = true;
            // 
            // startupController
            // 
            this.startupController.Dock = System.Windows.Forms.DockStyle.Fill;
            this.startupController.Location = new System.Drawing.Point(3, 3);
            this.startupController.Name = "startupController";
            this.startupController.Size = new System.Drawing.Size(1266, 493);
            this.startupController.TabIndex = 0;
            // 
            // tabPageOperation
            // 
            this.tabPageOperation.Controls.Add(this.tabOperation);
            this.tabPageOperation.Location = new System.Drawing.Point(4, 22);
            this.tabPageOperation.Name = "tabPageOperation";
            this.tabPageOperation.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageOperation.Size = new System.Drawing.Size(1272, 499);
            this.tabPageOperation.TabIndex = 1;
            this.tabPageOperation.Text = "Operation";
            this.tabPageOperation.UseVisualStyleBackColor = true;
            // 
            // tabOperation
            // 
            this.tabOperation.Controls.Add(this.tabStatus);
            this.tabOperation.Controls.Add(this.tabConfiguration);
            this.tabOperation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabOperation.Location = new System.Drawing.Point(3, 3);
            this.tabOperation.Name = "tabOperation";
            this.tabOperation.SelectedIndex = 0;
            this.tabOperation.Size = new System.Drawing.Size(1266, 493);
            this.tabOperation.TabIndex = 0;
            this.tabOperation.SelectedIndexChanged += new System.EventHandler(this.tabOperation_SelectedIndexChanged);
            // 
            // tabStatus
            // 
            this.tabStatus.AutoScroll = true;
            this.tabStatus.Controls.Add(this.efemStatusCtrl);
            this.tabStatus.Location = new System.Drawing.Point(4, 22);
            this.tabStatus.Name = "tabStatus";
            this.tabStatus.Padding = new System.Windows.Forms.Padding(3);
            this.tabStatus.Size = new System.Drawing.Size(1258, 467);
            this.tabStatus.TabIndex = 0;
            this.tabStatus.Text = "EFEM Status";
            this.tabStatus.UseVisualStyleBackColor = true;
            // 
            // efemStatusCtrl
            // 
            this.efemStatusCtrl.AutoScroll = true;
            this.efemStatusCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.efemStatusCtrl.Location = new System.Drawing.Point(3, 3);
            this.efemStatusCtrl.Name = "efemStatusCtrl";
            this.efemStatusCtrl.Size = new System.Drawing.Size(1252, 461);
            this.efemStatusCtrl.TabIndex = 0;
            // 
            // tabConfiguration
            // 
            this.tabConfiguration.AutoScroll = true;
            this.tabConfiguration.Controls.Add(this.tdkConfigControl1);
            this.tabConfiguration.Location = new System.Drawing.Point(4, 22);
            this.tabConfiguration.Name = "tabConfiguration";
            this.tabConfiguration.Padding = new System.Windows.Forms.Padding(3);
            this.tabConfiguration.Size = new System.Drawing.Size(1258, 467);
            this.tabConfiguration.TabIndex = 2;
            this.tabConfiguration.Text = "Configuration";
            this.tabConfiguration.UseVisualStyleBackColor = true;
            // 
            // tdkConfigControl1
            // 
            this.tdkConfigControl1.AutoScroll = true;
            this.tdkConfigControl1.Location = new System.Drawing.Point(0, 0);
            this.tdkConfigControl1.Name = "tdkConfigControl1";
            this.tdkConfigControl1.Size = new System.Drawing.Size(1249, 792);
            this.tdkConfigControl1.TabIndex = 0;
            // 
            // EFEMMainControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.statusStrip1);
            this.Name = "EFEMMainControl";
            this.Size = new System.Drawing.Size(1280, 663);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panelBottom.ResumeLayout(false);
            this.tlpInformation.ResumeLayout(false);
            this.panelUser.ResumeLayout(false);
            this.panelUser.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.tabMain.ResumeLayout(false);
            this.tabPageInit.ResumeLayout(false);
            this.tabPageOperation.ResumeLayout(false);
            this.tabOperation.ResumeLayout(false);
            this.tabStatus.ResumeLayout(false);
            this.tabConfiguration.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabPageInit;
        private System.Windows.Forms.TabPage tabPageOperation;
        private StartupComponentCtrl startupController;
        private ToolStripStatusIndication StatusIndInit;
        private ToolStripStatusIndication StatusIndHost;
        private ToolStripStatusIndication StatusIndRobot;
        private ToolStripStatusIndication StatusIndAligner;
        private System.Windows.Forms.ToolStripStatusLabel StatusEFEM;
        private System.Windows.Forms.ToolStripStatusLabel StatusEmpty;
        private System.Windows.Forms.ToolStripStatusLabel StatusOnline;
        private ToolStripStatusIndication StatusIndDIO0;
        private System.Windows.Forms.ToolStripStatusLabel lWaferSensing;
        private System.Windows.Forms.ToolStripStatusLabel StatusWaferSensing;
        private System.Windows.Forms.ToolStripStatusLabel lEFEMStatus;
        private System.Windows.Forms.ToolStripStatusLabel lOnOffline;
        private ToolStripStatusIndication StatusIndFFU;
        private System.Windows.Forms.ToolStripStatusLabel lActiveAlarm;
        private System.Windows.Forms.ToolStripStatusLabel StatusAlarmCount;
        private System.Windows.Forms.TableLayoutPanel tlpInformation;
        private StatusMessageCtrl statusMessageCtrl;
        private System.Windows.Forms.Panel panelUser;
        private System.Windows.Forms.Label lLoginStatus;
        private System.Windows.Forms.Button btnLockSystem;
        private System.Windows.Forms.Button btnUserLogOut;
        private System.Windows.Forms.Button btnUserLogin;
        private System.Windows.Forms.Label lUserTitle;
        private System.Windows.Forms.TabControl tabOperation;
        private System.Windows.Forms.TabPage tabStatus;
        public EFEMStatusCtrl efemStatusCtrl;
        private System.Windows.Forms.TabPage tabConfiguration;
        private TDKConfigControl tdkConfigControl1;
    }
}
