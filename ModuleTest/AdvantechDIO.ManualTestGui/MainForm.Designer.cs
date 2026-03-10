namespace AdvantechDIO.ManualTestGui
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.GroupBox grpDevice1;
        private System.Windows.Forms.Button btnConnectDev1;
        private System.Windows.Forms.Button btnDisconnectDev1;
        private System.Windows.Forms.Label lblDev1State;
        private System.Windows.Forms.GroupBox grpDevice0;
        private System.Windows.Forms.Button btnConnectDev0;
        private System.Windows.Forms.Button btnDisconnectDev0;
        private System.Windows.Forms.Label lblDev0State;
        private System.Windows.Forms.Button btnDev0SnapStart;
        private System.Windows.Forms.Button btnDev0SnapStop;
        private System.Windows.Forms.Button btnDev0GetDiPort;
        private System.Windows.Forms.Button btnDev0GetDiBit;
        private System.Windows.Forms.Button btnDev0GetDoPort;
        private System.Windows.Forms.Button btnDev0GetDoBit;
        private System.Windows.Forms.Button btnDev0SetDoPort;
        private System.Windows.Forms.Button btnDev0SetDoBit;
        private System.Windows.Forms.Button btnDev1SnapStart;
        private System.Windows.Forms.Button btnDev1SnapStop;
        private System.Windows.Forms.Button btnDev1GetDiPort;
        private System.Windows.Forms.Button btnDev1GetDiBit;
        private System.Windows.Forms.Button btnDev1GetDoPort;
        private System.Windows.Forms.Button btnDev1GetDoBit;
        private System.Windows.Forms.Button btnDev1SetDoPort;
        private System.Windows.Forms.Button btnDev1SetDoBit;
        private System.Windows.Forms.Label lblDev0DiPort;
        private System.Windows.Forms.TextBox txtDev0DiPort;
        private System.Windows.Forms.Label lblDev0DiBit;
        private System.Windows.Forms.TextBox txtDev0DiBit;
        private System.Windows.Forms.Label lblDev0DoPort;
        private System.Windows.Forms.TextBox txtDev0DoPort;
        private System.Windows.Forms.Label lblDev0DoBit;
        private System.Windows.Forms.TextBox txtDev0DoBit;
        private System.Windows.Forms.Label lblDev0DoValue;
        private System.Windows.Forms.TextBox txtDev0DoValue;
        private System.Windows.Forms.Label lblDev1DiPort;
        private System.Windows.Forms.TextBox txtDev1DiPort;
        private System.Windows.Forms.Label lblDev1DiBit;
        private System.Windows.Forms.TextBox txtDev1DiBit;
        private System.Windows.Forms.Label lblDev1DoPort;
        private System.Windows.Forms.TextBox txtDev1DoPort;
        private System.Windows.Forms.Label lblDev1DoBit;
        private System.Windows.Forms.TextBox txtDev1DoBit;
        private System.Windows.Forms.Label lblDev1DoValue;
        private System.Windows.Forms.TextBox txtDev1DoValue;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ListBox lstStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.grpDevice1 = new System.Windows.Forms.GroupBox();
            this.btnConnectDev1 = new System.Windows.Forms.Button();
            this.btnDisconnectDev1 = new System.Windows.Forms.Button();
            this.lblDev1State = new System.Windows.Forms.Label();
            this.grpDevice0 = new System.Windows.Forms.GroupBox();
            this.btnConnectDev0 = new System.Windows.Forms.Button();
            this.btnDisconnectDev0 = new System.Windows.Forms.Button();
            this.lblDev0State = new System.Windows.Forms.Label();
            this.btnDev0SnapStart = new System.Windows.Forms.Button();
            this.btnDev0SnapStop = new System.Windows.Forms.Button();
            this.btnDev0GetDiPort = new System.Windows.Forms.Button();
            this.btnDev0GetDiBit = new System.Windows.Forms.Button();
            this.btnDev0GetDoPort = new System.Windows.Forms.Button();
            this.btnDev0GetDoBit = new System.Windows.Forms.Button();
            this.btnDev0SetDoPort = new System.Windows.Forms.Button();
            this.btnDev0SetDoBit = new System.Windows.Forms.Button();
            this.btnDev1SnapStart = new System.Windows.Forms.Button();
            this.btnDev1SnapStop = new System.Windows.Forms.Button();
            this.btnDev1GetDiPort = new System.Windows.Forms.Button();
            this.btnDev1GetDiBit = new System.Windows.Forms.Button();
            this.btnDev1GetDoPort = new System.Windows.Forms.Button();
            this.btnDev1GetDoBit = new System.Windows.Forms.Button();
            this.btnDev1SetDoPort = new System.Windows.Forms.Button();
            this.btnDev1SetDoBit = new System.Windows.Forms.Button();
            this.lblDev0DiPort = new System.Windows.Forms.Label();
            this.txtDev0DiPort = new System.Windows.Forms.TextBox();
            this.lblDev0DiBit = new System.Windows.Forms.Label();
            this.txtDev0DiBit = new System.Windows.Forms.TextBox();
            this.lblDev0DoPort = new System.Windows.Forms.Label();
            this.txtDev0DoPort = new System.Windows.Forms.TextBox();
            this.lblDev0DoBit = new System.Windows.Forms.Label();
            this.txtDev0DoBit = new System.Windows.Forms.TextBox();
            this.lblDev0DoValue = new System.Windows.Forms.Label();
            this.txtDev0DoValue = new System.Windows.Forms.TextBox();
            this.lblDev1DiPort = new System.Windows.Forms.Label();
            this.txtDev1DiPort = new System.Windows.Forms.TextBox();
            this.lblDev1DiBit = new System.Windows.Forms.Label();
            this.txtDev1DiBit = new System.Windows.Forms.TextBox();
            this.lblDev1DoPort = new System.Windows.Forms.Label();
            this.txtDev1DoPort = new System.Windows.Forms.TextBox();
            this.lblDev1DoBit = new System.Windows.Forms.Label();
            this.txtDev1DoBit = new System.Windows.Forms.TextBox();
            this.lblDev1DoValue = new System.Windows.Forms.Label();
            this.txtDev1DoValue = new System.Windows.Forms.TextBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lstStatus = new System.Windows.Forms.ListBox();
            this.grpDevice1.SuspendLayout();
            this.grpDevice0.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpDevice1  (Right Panel)
            // 
            this.grpDevice1.Controls.Add(this.btnDev1SetDoBit);
            this.grpDevice1.Controls.Add(this.btnDev1SetDoPort);
            this.grpDevice1.Controls.Add(this.btnDev1GetDoBit);
            this.grpDevice1.Controls.Add(this.btnDev1GetDoPort);
            this.grpDevice1.Controls.Add(this.btnDev1GetDiBit);
            this.grpDevice1.Controls.Add(this.btnDev1GetDiPort);
            this.grpDevice1.Controls.Add(this.btnDev1SnapStop);
            this.grpDevice1.Controls.Add(this.btnDev1SnapStart);
            this.grpDevice1.Controls.Add(this.txtDev1DoValue);
            this.grpDevice1.Controls.Add(this.lblDev1DoValue);
            this.grpDevice1.Controls.Add(this.txtDev1DoBit);
            this.grpDevice1.Controls.Add(this.lblDev1DoBit);
            this.grpDevice1.Controls.Add(this.txtDev1DoPort);
            this.grpDevice1.Controls.Add(this.lblDev1DoPort);
            this.grpDevice1.Controls.Add(this.txtDev1DiBit);
            this.grpDevice1.Controls.Add(this.lblDev1DiBit);
            this.grpDevice1.Controls.Add(this.txtDev1DiPort);
            this.grpDevice1.Controls.Add(this.lblDev1DiPort);
            this.grpDevice1.Controls.Add(this.lblDev1State);
            this.grpDevice1.Controls.Add(this.btnDisconnectDev1);
            this.grpDevice1.Controls.Add(this.btnConnectDev1);
            this.grpDevice1.Location = new System.Drawing.Point(434, 12);
            this.grpDevice1.Name = "grpDevice1";
            this.grpDevice1.Size = new System.Drawing.Size(390, 330);
            this.grpDevice1.TabIndex = 0;
            this.grpDevice1.TabStop = false;
            this.grpDevice1.Text = "Device 1";
            // 
            // btnConnectDev1
            // 
            this.btnConnectDev1.Location = new System.Drawing.Point(10, 22);
            this.btnConnectDev1.Name = "btnConnectDev1";
            this.btnConnectDev1.Size = new System.Drawing.Size(88, 28);
            this.btnConnectDev1.TabIndex = 0;
            this.btnConnectDev1.Text = "Connect";
            this.btnConnectDev1.UseVisualStyleBackColor = true;
            this.btnConnectDev1.Click += new System.EventHandler(this.BtnConnectDev1_Click);
            // 
            // btnDisconnectDev1
            // 
            this.btnDisconnectDev1.Location = new System.Drawing.Point(106, 22);
            this.btnDisconnectDev1.Name = "btnDisconnectDev1";
            this.btnDisconnectDev1.Size = new System.Drawing.Size(88, 28);
            this.btnDisconnectDev1.TabIndex = 1;
            this.btnDisconnectDev1.Text = "Disconnect";
            this.btnDisconnectDev1.UseVisualStyleBackColor = true;
            this.btnDisconnectDev1.Click += new System.EventHandler(this.BtnDisconnectDev1_Click);
            // 
            // lblDev1State
            // 
            this.lblDev1State.AutoSize = true;
            this.lblDev1State.Location = new System.Drawing.Point(204, 28);
            this.lblDev1State.Name = "lblDev1State";
            this.lblDev1State.Size = new System.Drawing.Size(72, 13);
            this.lblDev1State.TabIndex = 2;
            this.lblDev1State.Text = "Disconnected";
            // 
            // btnDev1SnapStart
            // 
            this.btnDev1SnapStart.Location = new System.Drawing.Point(10, 52);
            this.btnDev1SnapStart.Name = "btnDev1SnapStart";
            this.btnDev1SnapStart.Size = new System.Drawing.Size(88, 23);
            this.btnDev1SnapStart.TabIndex = 3;
            this.btnDev1SnapStart.Tag = "DesignPreview";
            this.btnDev1SnapStart.Text = "SnapStart";
            this.btnDev1SnapStart.UseVisualStyleBackColor = true;
            // 
            // btnDev1SnapStop
            // 
            this.btnDev1SnapStop.Location = new System.Drawing.Point(104, 52);
            this.btnDev1SnapStop.Name = "btnDev1SnapStop";
            this.btnDev1SnapStop.Size = new System.Drawing.Size(88, 23);
            this.btnDev1SnapStop.TabIndex = 4;
            this.btnDev1SnapStop.Tag = "DesignPreview";
            this.btnDev1SnapStop.Text = "SnapStop";
            this.btnDev1SnapStop.UseVisualStyleBackColor = true;
            // 
            // btnDev1GetDiPort
            // 
            this.btnDev1GetDiPort.Location = new System.Drawing.Point(172, 85);
            this.btnDev1GetDiPort.Name = "btnDev1GetDiPort";
            this.btnDev1GetDiPort.Size = new System.Drawing.Size(88, 23);
            this.btnDev1GetDiPort.TabIndex = 5;
            this.btnDev1GetDiPort.Tag = "DesignPreview";
            this.btnDev1GetDiPort.Text = "Get DI Port";
            this.btnDev1GetDiPort.UseVisualStyleBackColor = true;
            // 
            // btnDev1GetDiBit
            // 
            this.btnDev1GetDiBit.Location = new System.Drawing.Point(266, 85);
            this.btnDev1GetDiBit.Name = "btnDev1GetDiBit";
            this.btnDev1GetDiBit.Size = new System.Drawing.Size(88, 23);
            this.btnDev1GetDiBit.TabIndex = 6;
            this.btnDev1GetDiBit.Tag = "DesignPreview";
            this.btnDev1GetDiBit.Text = "Get DI Bit";
            this.btnDev1GetDiBit.UseVisualStyleBackColor = true;
            // 
            // btnDev1GetDoPort
            // 
            this.btnDev1GetDoPort.Location = new System.Drawing.Point(10, 154);
            this.btnDev1GetDoPort.Name = "btnDev1GetDoPort";
            this.btnDev1GetDoPort.Size = new System.Drawing.Size(88, 23);
            this.btnDev1GetDoPort.TabIndex = 7;
            this.btnDev1GetDoPort.Tag = "DesignPreview";
            this.btnDev1GetDoPort.Text = "Get DO Port";
            this.btnDev1GetDoPort.UseVisualStyleBackColor = true;
            // 
            // btnDev1GetDoBit
            // 
            this.btnDev1GetDoBit.Location = new System.Drawing.Point(104, 154);
            this.btnDev1GetDoBit.Name = "btnDev1GetDoBit";
            this.btnDev1GetDoBit.Size = new System.Drawing.Size(88, 23);
            this.btnDev1GetDoBit.TabIndex = 8;
            this.btnDev1GetDoBit.Tag = "DesignPreview";
            this.btnDev1GetDoBit.Text = "Get DO Bit";
            this.btnDev1GetDoBit.UseVisualStyleBackColor = true;
            // 
            // btnDev1SetDoPort
            // 
            this.btnDev1SetDoPort.Location = new System.Drawing.Point(10, 178);
            this.btnDev1SetDoPort.Name = "btnDev1SetDoPort";
            this.btnDev1SetDoPort.Size = new System.Drawing.Size(88, 23);
            this.btnDev1SetDoPort.TabIndex = 9;
            this.btnDev1SetDoPort.Tag = "DesignPreview";
            this.btnDev1SetDoPort.Text = "Set DO Port";
            this.btnDev1SetDoPort.UseVisualStyleBackColor = true;
            // 
            // btnDev1SetDoBit
            // 
            this.btnDev1SetDoBit.Location = new System.Drawing.Point(104, 178);
            this.btnDev1SetDoBit.Name = "btnDev1SetDoBit";
            this.btnDev1SetDoBit.Size = new System.Drawing.Size(88, 23);
            this.btnDev1SetDoBit.TabIndex = 10;
            this.btnDev1SetDoBit.Tag = "DesignPreview";
            this.btnDev1SetDoBit.Text = "Set DO Bit";
            this.btnDev1SetDoBit.UseVisualStyleBackColor = true;
            // 
            // lblDev1DiPort
            // 
            this.lblDev1DiPort.AutoSize = true;
            this.lblDev1DiPort.Location = new System.Drawing.Point(10, 88);
            this.lblDev1DiPort.Name = "lblDev1DiPort";
            this.lblDev1DiPort.Size = new System.Drawing.Size(42, 13);
            this.lblDev1DiPort.TabIndex = 11;
            this.lblDev1DiPort.Tag = "DesignPreview";
            this.lblDev1DiPort.Text = "DI Port";
            // 
            // txtDev1DiPort
            // 
            this.txtDev1DiPort.Location = new System.Drawing.Point(58, 85);
            this.txtDev1DiPort.Name = "txtDev1DiPort";
            this.txtDev1DiPort.Size = new System.Drawing.Size(36, 20);
            this.txtDev1DiPort.TabIndex = 12;
            this.txtDev1DiPort.Tag = "DesignPreview";
            this.txtDev1DiPort.Text = "0";
            // 
            // lblDev1DiBit
            // 
            this.lblDev1DiBit.AutoSize = true;
            this.lblDev1DiBit.Location = new System.Drawing.Point(103, 88);
            this.lblDev1DiBit.Name = "lblDev1DiBit";
            this.lblDev1DiBit.Size = new System.Drawing.Size(19, 13);
            this.lblDev1DiBit.TabIndex = 13;
            this.lblDev1DiBit.Tag = "DesignPreview";
            this.lblDev1DiBit.Text = "Bit";
            // 
            // txtDev1DiBit
            // 
            this.txtDev1DiBit.Location = new System.Drawing.Point(126, 85);
            this.txtDev1DiBit.Name = "txtDev1DiBit";
            this.txtDev1DiBit.Size = new System.Drawing.Size(36, 20);
            this.txtDev1DiBit.TabIndex = 14;
            this.txtDev1DiBit.Tag = "DesignPreview";
            this.txtDev1DiBit.Text = "0";
            // 
            // lblDev1DoPort
            // 
            this.lblDev1DoPort.AutoSize = true;
            this.lblDev1DoPort.Location = new System.Drawing.Point(10, 133);
            this.lblDev1DoPort.Name = "lblDev1DoPort";
            this.lblDev1DoPort.Size = new System.Drawing.Size(44, 13);
            this.lblDev1DoPort.TabIndex = 15;
            this.lblDev1DoPort.Tag = "DesignPreview";
            this.lblDev1DoPort.Text = "DO Port";
            // 
            // txtDev1DoPort
            // 
            this.txtDev1DoPort.Location = new System.Drawing.Point(58, 130);
            this.txtDev1DoPort.Name = "txtDev1DoPort";
            this.txtDev1DoPort.Size = new System.Drawing.Size(36, 20);
            this.txtDev1DoPort.TabIndex = 16;
            this.txtDev1DoPort.Tag = "DesignPreview";
            this.txtDev1DoPort.Text = "0";
            // 
            // lblDev1DoBit
            // 
            this.lblDev1DoBit.AutoSize = true;
            this.lblDev1DoBit.Location = new System.Drawing.Point(103, 133);
            this.lblDev1DoBit.Name = "lblDev1DoBit";
            this.lblDev1DoBit.Size = new System.Drawing.Size(19, 13);
            this.lblDev1DoBit.TabIndex = 17;
            this.lblDev1DoBit.Tag = "DesignPreview";
            this.lblDev1DoBit.Text = "Bit";
            // 
            // txtDev1DoBit
            // 
            this.txtDev1DoBit.Location = new System.Drawing.Point(126, 130);
            this.txtDev1DoBit.Name = "txtDev1DoBit";
            this.txtDev1DoBit.Size = new System.Drawing.Size(36, 20);
            this.txtDev1DoBit.TabIndex = 18;
            this.txtDev1DoBit.Tag = "DesignPreview";
            this.txtDev1DoBit.Text = "0";
            // 
            // lblDev1DoValue
            // 
            this.lblDev1DoValue.AutoSize = true;
            this.lblDev1DoValue.Location = new System.Drawing.Point(171, 133);
            this.lblDev1DoValue.Name = "lblDev1DoValue";
            this.lblDev1DoValue.Size = new System.Drawing.Size(34, 13);
            this.lblDev1DoValue.TabIndex = 19;
            this.lblDev1DoValue.Tag = "DesignPreview";
            this.lblDev1DoValue.Text = "Value";
            // 
            // txtDev1DoValue
            // 
            this.txtDev1DoValue.Location = new System.Drawing.Point(209, 130);
            this.txtDev1DoValue.Name = "txtDev1DoValue";
            this.txtDev1DoValue.Size = new System.Drawing.Size(36, 20);
            this.txtDev1DoValue.TabIndex = 20;
            this.txtDev1DoValue.Tag = "DesignPreview";
            this.txtDev1DoValue.Text = "0";
            // 
            // grpDevice0  (Left Panel)
            // 
            this.grpDevice0.Controls.Add(this.btnDev0SetDoBit);
            this.grpDevice0.Controls.Add(this.btnDev0SetDoPort);
            this.grpDevice0.Controls.Add(this.btnDev0GetDoBit);
            this.grpDevice0.Controls.Add(this.btnDev0GetDoPort);
            this.grpDevice0.Controls.Add(this.btnDev0GetDiBit);
            this.grpDevice0.Controls.Add(this.btnDev0GetDiPort);
            this.grpDevice0.Controls.Add(this.btnDev0SnapStop);
            this.grpDevice0.Controls.Add(this.btnDev0SnapStart);
            this.grpDevice0.Controls.Add(this.txtDev0DoValue);
            this.grpDevice0.Controls.Add(this.lblDev0DoValue);
            this.grpDevice0.Controls.Add(this.txtDev0DoBit);
            this.grpDevice0.Controls.Add(this.lblDev0DoBit);
            this.grpDevice0.Controls.Add(this.txtDev0DoPort);
            this.grpDevice0.Controls.Add(this.lblDev0DoPort);
            this.grpDevice0.Controls.Add(this.txtDev0DiBit);
            this.grpDevice0.Controls.Add(this.lblDev0DiBit);
            this.grpDevice0.Controls.Add(this.txtDev0DiPort);
            this.grpDevice0.Controls.Add(this.lblDev0DiPort);
            this.grpDevice0.Controls.Add(this.lblDev0State);
            this.grpDevice0.Controls.Add(this.btnDisconnectDev0);
            this.grpDevice0.Controls.Add(this.btnConnectDev0);
            this.grpDevice0.Location = new System.Drawing.Point(12, 12);
            this.grpDevice0.Name = "grpDevice0";
            this.grpDevice0.Size = new System.Drawing.Size(410, 330);
            this.grpDevice0.TabIndex = 1;
            this.grpDevice0.TabStop = false;
            this.grpDevice0.Text = "Device 0";
            // 
            // btnConnectDev0
            // 
            this.btnConnectDev0.Location = new System.Drawing.Point(10, 22);
            this.btnConnectDev0.Name = "btnConnectDev0";
            this.btnConnectDev0.Size = new System.Drawing.Size(88, 28);
            this.btnConnectDev0.TabIndex = 0;
            this.btnConnectDev0.Text = "Connect";
            this.btnConnectDev0.UseVisualStyleBackColor = true;
            this.btnConnectDev0.Click += new System.EventHandler(this.BtnConnectDev0_Click);
            // 
            // btnDisconnectDev0
            // 
            this.btnDisconnectDev0.Location = new System.Drawing.Point(106, 22);
            this.btnDisconnectDev0.Name = "btnDisconnectDev0";
            this.btnDisconnectDev0.Size = new System.Drawing.Size(88, 28);
            this.btnDisconnectDev0.TabIndex = 1;
            this.btnDisconnectDev0.Text = "Disconnect";
            this.btnDisconnectDev0.UseVisualStyleBackColor = true;
            this.btnDisconnectDev0.Click += new System.EventHandler(this.BtnDisconnectDev0_Click);
            // 
            // lblDev0State
            // 
            this.lblDev0State.AutoSize = true;
            this.lblDev0State.Location = new System.Drawing.Point(204, 28);
            this.lblDev0State.Name = "lblDev0State";
            this.lblDev0State.Size = new System.Drawing.Size(72, 13);
            this.lblDev0State.TabIndex = 2;
            this.lblDev0State.Text = "Disconnected";
            // 
            // btnDev0SnapStart
            // 
            this.btnDev0SnapStart.Location = new System.Drawing.Point(10, 52);
            this.btnDev0SnapStart.Name = "btnDev0SnapStart";
            this.btnDev0SnapStart.Size = new System.Drawing.Size(88, 23);
            this.btnDev0SnapStart.TabIndex = 3;
            this.btnDev0SnapStart.Tag = "DesignPreview";
            this.btnDev0SnapStart.Text = "SnapStart";
            this.btnDev0SnapStart.UseVisualStyleBackColor = true;
            // 
            // btnDev0SnapStop
            // 
            this.btnDev0SnapStop.Location = new System.Drawing.Point(104, 52);
            this.btnDev0SnapStop.Name = "btnDev0SnapStop";
            this.btnDev0SnapStop.Size = new System.Drawing.Size(88, 23);
            this.btnDev0SnapStop.TabIndex = 4;
            this.btnDev0SnapStop.Tag = "DesignPreview";
            this.btnDev0SnapStop.Text = "SnapStop";
            this.btnDev0SnapStop.UseVisualStyleBackColor = true;
            // 
            // btnDev0GetDiPort
            // 
            this.btnDev0GetDiPort.Location = new System.Drawing.Point(172, 85);
            this.btnDev0GetDiPort.Name = "btnDev0GetDiPort";
            this.btnDev0GetDiPort.Size = new System.Drawing.Size(88, 23);
            this.btnDev0GetDiPort.TabIndex = 5;
            this.btnDev0GetDiPort.Tag = "DesignPreview";
            this.btnDev0GetDiPort.Text = "Get DI Port";
            this.btnDev0GetDiPort.UseVisualStyleBackColor = true;
            // 
            // btnDev0GetDiBit
            // 
            this.btnDev0GetDiBit.Location = new System.Drawing.Point(266, 85);
            this.btnDev0GetDiBit.Name = "btnDev0GetDiBit";
            this.btnDev0GetDiBit.Size = new System.Drawing.Size(88, 23);
            this.btnDev0GetDiBit.TabIndex = 6;
            this.btnDev0GetDiBit.Tag = "DesignPreview";
            this.btnDev0GetDiBit.Text = "Get DI Bit";
            this.btnDev0GetDiBit.UseVisualStyleBackColor = true;
            // 
            // btnDev0GetDoPort
            // 
            this.btnDev0GetDoPort.Location = new System.Drawing.Point(10, 154);
            this.btnDev0GetDoPort.Name = "btnDev0GetDoPort";
            this.btnDev0GetDoPort.Size = new System.Drawing.Size(88, 23);
            this.btnDev0GetDoPort.TabIndex = 7;
            this.btnDev0GetDoPort.Tag = "DesignPreview";
            this.btnDev0GetDoPort.Text = "Get DO Port";
            this.btnDev0GetDoPort.UseVisualStyleBackColor = true;
            // 
            // btnDev0GetDoBit
            // 
            this.btnDev0GetDoBit.Location = new System.Drawing.Point(104, 154);
            this.btnDev0GetDoBit.Name = "btnDev0GetDoBit";
            this.btnDev0GetDoBit.Size = new System.Drawing.Size(88, 23);
            this.btnDev0GetDoBit.TabIndex = 8;
            this.btnDev0GetDoBit.Tag = "DesignPreview";
            this.btnDev0GetDoBit.Text = "Get DO Bit";
            this.btnDev0GetDoBit.UseVisualStyleBackColor = true;
            // 
            // btnDev0SetDoPort
            // 
            this.btnDev0SetDoPort.Location = new System.Drawing.Point(10, 178);
            this.btnDev0SetDoPort.Name = "btnDev0SetDoPort";
            this.btnDev0SetDoPort.Size = new System.Drawing.Size(88, 23);
            this.btnDev0SetDoPort.TabIndex = 9;
            this.btnDev0SetDoPort.Tag = "DesignPreview";
            this.btnDev0SetDoPort.Text = "Set DO Port";
            this.btnDev0SetDoPort.UseVisualStyleBackColor = true;
            // 
            // btnDev0SetDoBit
            // 
            this.btnDev0SetDoBit.Location = new System.Drawing.Point(104, 178);
            this.btnDev0SetDoBit.Name = "btnDev0SetDoBit";
            this.btnDev0SetDoBit.Size = new System.Drawing.Size(88, 23);
            this.btnDev0SetDoBit.TabIndex = 10;
            this.btnDev0SetDoBit.Tag = "DesignPreview";
            this.btnDev0SetDoBit.Text = "Set DO Bit";
            this.btnDev0SetDoBit.UseVisualStyleBackColor = true;
            // 
            // lblDev0DiPort
            // 
            this.lblDev0DiPort.AutoSize = true;
            this.lblDev0DiPort.Location = new System.Drawing.Point(10, 88);
            this.lblDev0DiPort.Name = "lblDev0DiPort";
            this.lblDev0DiPort.Size = new System.Drawing.Size(42, 13);
            this.lblDev0DiPort.TabIndex = 11;
            this.lblDev0DiPort.Tag = "DesignPreview";
            this.lblDev0DiPort.Text = "DI Port";
            // 
            // txtDev0DiPort
            // 
            this.txtDev0DiPort.Location = new System.Drawing.Point(58, 85);
            this.txtDev0DiPort.Name = "txtDev0DiPort";
            this.txtDev0DiPort.Size = new System.Drawing.Size(36, 20);
            this.txtDev0DiPort.TabIndex = 12;
            this.txtDev0DiPort.Tag = "DesignPreview";
            this.txtDev0DiPort.Text = "0";
            // 
            // lblDev0DiBit
            // 
            this.lblDev0DiBit.AutoSize = true;
            this.lblDev0DiBit.Location = new System.Drawing.Point(103, 88);
            this.lblDev0DiBit.Name = "lblDev0DiBit";
            this.lblDev0DiBit.Size = new System.Drawing.Size(19, 13);
            this.lblDev0DiBit.TabIndex = 13;
            this.lblDev0DiBit.Tag = "DesignPreview";
            this.lblDev0DiBit.Text = "Bit";
            // 
            // txtDev0DiBit
            // 
            this.txtDev0DiBit.Location = new System.Drawing.Point(126, 85);
            this.txtDev0DiBit.Name = "txtDev0DiBit";
            this.txtDev0DiBit.Size = new System.Drawing.Size(36, 20);
            this.txtDev0DiBit.TabIndex = 14;
            this.txtDev0DiBit.Tag = "DesignPreview";
            this.txtDev0DiBit.Text = "0";
            // 
            // lblDev0DoPort
            // 
            this.lblDev0DoPort.AutoSize = true;
            this.lblDev0DoPort.Location = new System.Drawing.Point(10, 133);
            this.lblDev0DoPort.Name = "lblDev0DoPort";
            this.lblDev0DoPort.Size = new System.Drawing.Size(44, 13);
            this.lblDev0DoPort.TabIndex = 15;
            this.lblDev0DoPort.Tag = "DesignPreview";
            this.lblDev0DoPort.Text = "DO Port";
            // 
            // txtDev0DoPort
            // 
            this.txtDev0DoPort.Location = new System.Drawing.Point(58, 130);
            this.txtDev0DoPort.Name = "txtDev0DoPort";
            this.txtDev0DoPort.Size = new System.Drawing.Size(36, 20);
            this.txtDev0DoPort.TabIndex = 16;
            this.txtDev0DoPort.Tag = "DesignPreview";
            this.txtDev0DoPort.Text = "0";
            // 
            // lblDev0DoBit
            // 
            this.lblDev0DoBit.AutoSize = true;
            this.lblDev0DoBit.Location = new System.Drawing.Point(103, 133);
            this.lblDev0DoBit.Name = "lblDev0DoBit";
            this.lblDev0DoBit.Size = new System.Drawing.Size(19, 13);
            this.lblDev0DoBit.TabIndex = 17;
            this.lblDev0DoBit.Tag = "DesignPreview";
            this.lblDev0DoBit.Text = "Bit";
            // 
            // txtDev0DoBit
            // 
            this.txtDev0DoBit.Location = new System.Drawing.Point(126, 130);
            this.txtDev0DoBit.Name = "txtDev0DoBit";
            this.txtDev0DoBit.Size = new System.Drawing.Size(36, 20);
            this.txtDev0DoBit.TabIndex = 18;
            this.txtDev0DoBit.Tag = "DesignPreview";
            this.txtDev0DoBit.Text = "0";
            // 
            // lblDev0DoValue
            // 
            this.lblDev0DoValue.AutoSize = true;
            this.lblDev0DoValue.Location = new System.Drawing.Point(171, 133);
            this.lblDev0DoValue.Name = "lblDev0DoValue";
            this.lblDev0DoValue.Size = new System.Drawing.Size(34, 13);
            this.lblDev0DoValue.TabIndex = 19;
            this.lblDev0DoValue.Tag = "DesignPreview";
            this.lblDev0DoValue.Text = "Value";
            // 
            // txtDev0DoValue
            // 
            this.txtDev0DoValue.Location = new System.Drawing.Point(209, 130);
            this.txtDev0DoValue.Name = "txtDev0DoValue";
            this.txtDev0DoValue.Size = new System.Drawing.Size(36, 20);
            this.txtDev0DoValue.TabIndex = 20;
            this.txtDev0DoValue.Tag = "DesignPreview";
            this.txtDev0DoValue.Text = "0";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 351);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(63, 13);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "Status / Log";
            // 
            // lstStatus
            // 
            this.lstStatus.FormattingEnabled = true;
            this.lstStatus.HorizontalScrollbar = true;
            this.lstStatus.Location = new System.Drawing.Point(12, 369);
            this.lstStatus.Name = "lstStatus";
            this.lstStatus.Size = new System.Drawing.Size(812, 130);
            this.lstStatus.TabIndex = 3;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(836, 512);
            this.Controls.Add(this.lstStatus);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.grpDevice0);
            this.Controls.Add(this.grpDevice1);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AdvantechDIO Manual Test";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.grpDevice1.ResumeLayout(false);
            this.grpDevice1.PerformLayout();
            this.grpDevice0.ResumeLayout(false);
            this.grpDevice0.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
