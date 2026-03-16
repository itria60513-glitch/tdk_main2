namespace CarrierIDReader.ManualTestGui
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.GroupBox grpReaderSelect;
        private System.Windows.Forms.Label lblReaderType;
        private System.Windows.Forms.ComboBox cboReaderType;
        private System.Windows.Forms.Label lblComPort;
        private System.Windows.Forms.ComboBox cboComPort;

        private System.Windows.Forms.GroupBox grpSerialSettings;
        private System.Windows.Forms.Label lblBaudRate;
        private System.Windows.Forms.ComboBox cboBaudRate;
        private System.Windows.Forms.Label lblParity;
        private System.Windows.Forms.ComboBox cboParity;
        private System.Windows.Forms.Label lblDataBits;
        private System.Windows.Forms.ComboBox cboDataBits;
        private System.Windows.Forms.Label lblStopBits;
        private System.Windows.Forms.ComboBox cboStopBits;

        private System.Windows.Forms.GroupBox grpReaderParams;
        private System.Windows.Forms.Label lblTimeout;
        private System.Windows.Forms.TextBox txtTimeout;
        private System.Windows.Forms.Label lblRetryCount;
        private System.Windows.Forms.TextBox txtRetryCount;

        private System.Windows.Forms.GroupBox grpConnection;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.Label lblConnState;

        private System.Windows.Forms.GroupBox grpOperations;
        private System.Windows.Forms.Label lblPage;
        private System.Windows.Forms.TextBox txtPage;
        private System.Windows.Forms.Label lblCarrierId;
        private System.Windows.Forms.TextBox txtCarrierId;
        private System.Windows.Forms.Button btnRead;
        private System.Windows.Forms.Button btnWrite;
        private System.Windows.Forms.Label lblResult;
        private System.Windows.Forms.TextBox txtResult;

        private System.Windows.Forms.GroupBox grpStatusLog;
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
            this.grpReaderSelect = new System.Windows.Forms.GroupBox();
            this.lblReaderType = new System.Windows.Forms.Label();
            this.cboReaderType = new System.Windows.Forms.ComboBox();
            this.lblComPort = new System.Windows.Forms.Label();
            this.cboComPort = new System.Windows.Forms.ComboBox();
            this.grpSerialSettings = new System.Windows.Forms.GroupBox();
            this.lblBaudRate = new System.Windows.Forms.Label();
            this.cboBaudRate = new System.Windows.Forms.ComboBox();
            this.lblParity = new System.Windows.Forms.Label();
            this.cboParity = new System.Windows.Forms.ComboBox();
            this.lblDataBits = new System.Windows.Forms.Label();
            this.cboDataBits = new System.Windows.Forms.ComboBox();
            this.lblStopBits = new System.Windows.Forms.Label();
            this.cboStopBits = new System.Windows.Forms.ComboBox();
            this.grpReaderParams = new System.Windows.Forms.GroupBox();
            this.lblTimeout = new System.Windows.Forms.Label();
            this.txtTimeout = new System.Windows.Forms.TextBox();
            this.lblRetryCount = new System.Windows.Forms.Label();
            this.txtRetryCount = new System.Windows.Forms.TextBox();
            this.grpConnection = new System.Windows.Forms.GroupBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnDisconnect = new System.Windows.Forms.Button();
            this.lblConnState = new System.Windows.Forms.Label();
            this.grpOperations = new System.Windows.Forms.GroupBox();
            this.lblPage = new System.Windows.Forms.Label();
            this.txtPage = new System.Windows.Forms.TextBox();
            this.lblCarrierId = new System.Windows.Forms.Label();
            this.txtCarrierId = new System.Windows.Forms.TextBox();
            this.btnRead = new System.Windows.Forms.Button();
            this.btnWrite = new System.Windows.Forms.Button();
            this.lblResult = new System.Windows.Forms.Label();
            this.txtResult = new System.Windows.Forms.TextBox();
            this.grpStatusLog = new System.Windows.Forms.GroupBox();
            this.lstStatus = new System.Windows.Forms.ListBox();
            this.grpReaderSelect.SuspendLayout();
            this.grpSerialSettings.SuspendLayout();
            this.grpReaderParams.SuspendLayout();
            this.grpConnection.SuspendLayout();
            this.grpOperations.SuspendLayout();
            this.grpStatusLog.SuspendLayout();
            this.SuspendLayout();
            // ============================================================
            // grpReaderSelect — Reader Type & COM Port
            // ============================================================
            this.grpReaderSelect.Controls.Add(this.lblReaderType);
            this.grpReaderSelect.Controls.Add(this.cboReaderType);
            this.grpReaderSelect.Controls.Add(this.lblComPort);
            this.grpReaderSelect.Controls.Add(this.cboComPort);
            this.grpReaderSelect.Location = new System.Drawing.Point(12, 12);
            this.grpReaderSelect.Name = "grpReaderSelect";
            this.grpReaderSelect.Size = new System.Drawing.Size(560, 52);
            this.grpReaderSelect.TabIndex = 0;
            this.grpReaderSelect.TabStop = false;
            this.grpReaderSelect.Text = "Reader Selection";
            //
            // lblReaderType
            //
            this.lblReaderType.AutoSize = true;
            this.lblReaderType.Location = new System.Drawing.Point(10, 22);
            this.lblReaderType.Name = "lblReaderType";
            this.lblReaderType.Size = new System.Drawing.Size(70, 13);
            this.lblReaderType.Text = "Reader Type";
            //
            // cboReaderType
            //
            this.cboReaderType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboReaderType.Location = new System.Drawing.Point(85, 19);
            this.cboReaderType.Name = "cboReaderType";
            this.cboReaderType.Size = new System.Drawing.Size(175, 21);
            this.cboReaderType.TabIndex = 0;
            //
            // lblComPort
            //
            this.lblComPort.AutoSize = true;
            this.lblComPort.Location = new System.Drawing.Point(280, 22);
            this.lblComPort.Name = "lblComPort";
            this.lblComPort.Size = new System.Drawing.Size(53, 13);
            this.lblComPort.Text = "COM Port";
            //
            // cboComPort
            //
            this.cboComPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboComPort.Location = new System.Drawing.Point(345, 19);
            this.cboComPort.Name = "cboComPort";
            this.cboComPort.Size = new System.Drawing.Size(90, 21);
            this.cboComPort.TabIndex = 1;
            // ============================================================
            // grpSerialSettings — Baud / Parity / DataBits / StopBits
            // ============================================================
            this.grpSerialSettings.Controls.Add(this.lblBaudRate);
            this.grpSerialSettings.Controls.Add(this.cboBaudRate);
            this.grpSerialSettings.Controls.Add(this.lblParity);
            this.grpSerialSettings.Controls.Add(this.cboParity);
            this.grpSerialSettings.Controls.Add(this.lblDataBits);
            this.grpSerialSettings.Controls.Add(this.cboDataBits);
            this.grpSerialSettings.Controls.Add(this.lblStopBits);
            this.grpSerialSettings.Controls.Add(this.cboStopBits);
            this.grpSerialSettings.Location = new System.Drawing.Point(12, 68);
            this.grpSerialSettings.Name = "grpSerialSettings";
            this.grpSerialSettings.Size = new System.Drawing.Size(560, 52);
            this.grpSerialSettings.TabIndex = 1;
            this.grpSerialSettings.TabStop = false;
            this.grpSerialSettings.Text = "Serial Port Settings";
            //
            // lblBaudRate
            //
            this.lblBaudRate.AutoSize = true;
            this.lblBaudRate.Location = new System.Drawing.Point(10, 22);
            this.lblBaudRate.Name = "lblBaudRate";
            this.lblBaudRate.Size = new System.Drawing.Size(58, 13);
            this.lblBaudRate.Text = "Baud Rate";
            //
            // cboBaudRate
            //
            this.cboBaudRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboBaudRate.Location = new System.Drawing.Point(75, 19);
            this.cboBaudRate.Name = "cboBaudRate";
            this.cboBaudRate.Size = new System.Drawing.Size(80, 21);
            this.cboBaudRate.TabIndex = 0;
            //
            // lblParity
            //
            this.lblParity.AutoSize = true;
            this.lblParity.Location = new System.Drawing.Point(170, 22);
            this.lblParity.Name = "lblParity";
            this.lblParity.Size = new System.Drawing.Size(33, 13);
            this.lblParity.Text = "Parity";
            //
            // cboParity
            //
            this.cboParity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboParity.Location = new System.Drawing.Point(210, 19);
            this.cboParity.Name = "cboParity";
            this.cboParity.Size = new System.Drawing.Size(75, 21);
            this.cboParity.TabIndex = 1;
            //
            // lblDataBits
            //
            this.lblDataBits.AutoSize = true;
            this.lblDataBits.Location = new System.Drawing.Point(300, 22);
            this.lblDataBits.Name = "lblDataBits";
            this.lblDataBits.Size = new System.Drawing.Size(50, 13);
            this.lblDataBits.Text = "Data Bits";
            //
            // cboDataBits
            //
            this.cboDataBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDataBits.Location = new System.Drawing.Point(358, 19);
            this.cboDataBits.Name = "cboDataBits";
            this.cboDataBits.Size = new System.Drawing.Size(50, 21);
            this.cboDataBits.TabIndex = 2;
            //
            // lblStopBits
            //
            this.lblStopBits.AutoSize = true;
            this.lblStopBits.Location = new System.Drawing.Point(423, 22);
            this.lblStopBits.Name = "lblStopBits";
            this.lblStopBits.Size = new System.Drawing.Size(49, 13);
            this.lblStopBits.Text = "Stop Bits";
            //
            // cboStopBits
            //
            this.cboStopBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboStopBits.Location = new System.Drawing.Point(480, 19);
            this.cboStopBits.Name = "cboStopBits";
            this.cboStopBits.Size = new System.Drawing.Size(50, 21);
            this.cboStopBits.TabIndex = 3;
            // ============================================================
            // grpReaderParams — Timeout & Retry Count
            // ============================================================
            this.grpReaderParams.Controls.Add(this.lblTimeout);
            this.grpReaderParams.Controls.Add(this.txtTimeout);
            this.grpReaderParams.Controls.Add(this.lblRetryCount);
            this.grpReaderParams.Controls.Add(this.txtRetryCount);
            this.grpReaderParams.Location = new System.Drawing.Point(12, 124);
            this.grpReaderParams.Name = "grpReaderParams";
            this.grpReaderParams.Size = new System.Drawing.Size(560, 52);
            this.grpReaderParams.TabIndex = 2;
            this.grpReaderParams.TabStop = false;
            this.grpReaderParams.Text = "Reader Parameters";
            //
            // lblTimeout
            //
            this.lblTimeout.AutoSize = true;
            this.lblTimeout.Location = new System.Drawing.Point(10, 22);
            this.lblTimeout.Name = "lblTimeout";
            this.lblTimeout.Size = new System.Drawing.Size(67, 13);
            this.lblTimeout.Text = "Timeout (ms)";
            //
            // txtTimeout
            //
            this.txtTimeout.Location = new System.Drawing.Point(85, 19);
            this.txtTimeout.Name = "txtTimeout";
            this.txtTimeout.Size = new System.Drawing.Size(70, 20);
            this.txtTimeout.TabIndex = 0;
            this.txtTimeout.Text = "10000";
            //
            // lblRetryCount
            //
            this.lblRetryCount.AutoSize = true;
            this.lblRetryCount.Location = new System.Drawing.Point(180, 22);
            this.lblRetryCount.Name = "lblRetryCount";
            this.lblRetryCount.Size = new System.Drawing.Size(68, 13);
            this.lblRetryCount.Text = "Retry Count";
            //
            // txtRetryCount
            //
            this.txtRetryCount.Location = new System.Drawing.Point(255, 19);
            this.txtRetryCount.Name = "txtRetryCount";
            this.txtRetryCount.Size = new System.Drawing.Size(50, 20);
            this.txtRetryCount.TabIndex = 1;
            this.txtRetryCount.Text = "8";
            // ============================================================
            // grpConnection — Create / Dispose Reader
            // ============================================================
            this.grpConnection.Controls.Add(this.btnConnect);
            this.grpConnection.Controls.Add(this.btnDisconnect);
            this.grpConnection.Controls.Add(this.lblConnState);
            this.grpConnection.Location = new System.Drawing.Point(12, 180);
            this.grpConnection.Name = "grpConnection";
            this.grpConnection.Size = new System.Drawing.Size(560, 55);
            this.grpConnection.TabIndex = 3;
            this.grpConnection.TabStop = false;
            this.grpConnection.Text = "Connection";
            //
            // btnConnect
            //
            this.btnConnect.Location = new System.Drawing.Point(10, 20);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(110, 28);
            this.btnConnect.TabIndex = 0;
            this.btnConnect.Text = "Create Reader";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.BtnConnect_Click);
            //
            // btnDisconnect
            //
            this.btnDisconnect.Location = new System.Drawing.Point(130, 20);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(110, 28);
            this.btnDisconnect.TabIndex = 1;
            this.btnDisconnect.Text = "Dispose Reader";
            this.btnDisconnect.UseVisualStyleBackColor = true;
            this.btnDisconnect.Click += new System.EventHandler(this.BtnDisconnect_Click);
            //
            // lblConnState
            //
            this.lblConnState.AutoSize = true;
            this.lblConnState.Location = new System.Drawing.Point(260, 26);
            this.lblConnState.Name = "lblConnState";
            this.lblConnState.Size = new System.Drawing.Size(60, 13);
            this.lblConnState.Text = "No Reader";
            this.lblConnState.ForeColor = System.Drawing.Color.Gray;
            this.lblConnState.Font = new System.Drawing.Font(this.Font.FontFamily, 9F, System.Drawing.FontStyle.Bold);
            // ============================================================
            // grpOperations — Read / Write Carrier ID
            // ============================================================
            this.grpOperations.Controls.Add(this.lblPage);
            this.grpOperations.Controls.Add(this.txtPage);
            this.grpOperations.Controls.Add(this.lblCarrierId);
            this.grpOperations.Controls.Add(this.txtCarrierId);
            this.grpOperations.Controls.Add(this.btnRead);
            this.grpOperations.Controls.Add(this.btnWrite);
            this.grpOperations.Controls.Add(this.lblResult);
            this.grpOperations.Controls.Add(this.txtResult);
            this.grpOperations.Location = new System.Drawing.Point(12, 240);
            this.grpOperations.Name = "grpOperations";
            this.grpOperations.Size = new System.Drawing.Size(560, 120);
            this.grpOperations.TabIndex = 4;
            this.grpOperations.TabStop = false;
            this.grpOperations.Text = "Read / Write Operations";
            //
            // lblPage
            //
            this.lblPage.AutoSize = true;
            this.lblPage.Location = new System.Drawing.Point(10, 25);
            this.lblPage.Name = "lblPage";
            this.lblPage.Size = new System.Drawing.Size(32, 13);
            this.lblPage.Text = "Page";
            //
            // txtPage
            //
            this.txtPage.Location = new System.Drawing.Point(50, 22);
            this.txtPage.Name = "txtPage";
            this.txtPage.Size = new System.Drawing.Size(50, 20);
            this.txtPage.TabIndex = 0;
            this.txtPage.Text = "1";
            //
            // lblCarrierId
            //
            this.lblCarrierId.AutoSize = true;
            this.lblCarrierId.Location = new System.Drawing.Point(120, 25);
            this.lblCarrierId.Name = "lblCarrierId";
            this.lblCarrierId.Size = new System.Drawing.Size(53, 13);
            this.lblCarrierId.Text = "Carrier ID";
            //
            // txtCarrierId
            //
            this.txtCarrierId.Location = new System.Drawing.Point(185, 22);
            this.txtCarrierId.Name = "txtCarrierId";
            this.txtCarrierId.Size = new System.Drawing.Size(365, 20);
            this.txtCarrierId.TabIndex = 1;
            //
            // btnRead
            //
            this.btnRead.Location = new System.Drawing.Point(10, 52);
            this.btnRead.Name = "btnRead";
            this.btnRead.Size = new System.Drawing.Size(100, 28);
            this.btnRead.TabIndex = 2;
            this.btnRead.Text = "Read (GetID)";
            this.btnRead.UseVisualStyleBackColor = true;
            this.btnRead.Click += new System.EventHandler(this.BtnRead_Click);
            //
            // btnWrite
            //
            this.btnWrite.Location = new System.Drawing.Point(120, 52);
            this.btnWrite.Name = "btnWrite";
            this.btnWrite.Size = new System.Drawing.Size(100, 28);
            this.btnWrite.TabIndex = 3;
            this.btnWrite.Text = "Write (SetID)";
            this.btnWrite.UseVisualStyleBackColor = true;
            this.btnWrite.Click += new System.EventHandler(this.BtnWrite_Click);
            //
            // lblResult
            //
            this.lblResult.AutoSize = true;
            this.lblResult.Location = new System.Drawing.Point(10, 90);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(37, 13);
            this.lblResult.Text = "Result";
            //
            // txtResult
            //
            this.txtResult.Location = new System.Drawing.Point(50, 87);
            this.txtResult.Name = "txtResult";
            this.txtResult.ReadOnly = true;
            this.txtResult.Size = new System.Drawing.Size(500, 20);
            this.txtResult.TabIndex = 4;
            // ============================================================
            // grpStatusLog — Log Output
            // ============================================================
            this.grpStatusLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpStatusLog.Controls.Add(this.lstStatus);
            this.grpStatusLog.Location = new System.Drawing.Point(12, 366);
            this.grpStatusLog.Name = "grpStatusLog";
            this.grpStatusLog.Size = new System.Drawing.Size(560, 200);
            this.grpStatusLog.TabIndex = 5;
            this.grpStatusLog.TabStop = false;
            this.grpStatusLog.Text = "Status Log";
            //
            // lstStatus
            //
            this.lstStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstStatus.FormattingEnabled = true;
            this.lstStatus.Location = new System.Drawing.Point(8, 18);
            this.lstStatus.Name = "lstStatus";
            this.lstStatus.Size = new System.Drawing.Size(544, 173);
            this.lstStatus.TabIndex = 0;
            // ============================================================
            // MainForm
            // ============================================================
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 581);
            this.Controls.Add(this.grpReaderSelect);
            this.Controls.Add(this.grpSerialSettings);
            this.Controls.Add(this.grpReaderParams);
            this.Controls.Add(this.grpConnection);
            this.Controls.Add(this.grpOperations);
            this.Controls.Add(this.grpStatusLog);
            this.MinimumSize = new System.Drawing.Size(600, 620);
            this.Name = "MainForm";
            this.Text = "CarrierIDReader Manual Test";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.grpReaderSelect.ResumeLayout(false);
            this.grpReaderSelect.PerformLayout();
            this.grpSerialSettings.ResumeLayout(false);
            this.grpSerialSettings.PerformLayout();
            this.grpReaderParams.ResumeLayout(false);
            this.grpReaderParams.PerformLayout();
            this.grpConnection.ResumeLayout(false);
            this.grpConnection.PerformLayout();
            this.grpOperations.ResumeLayout(false);
            this.grpOperations.PerformLayout();
            this.grpStatusLog.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
