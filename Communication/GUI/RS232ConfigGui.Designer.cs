namespace Communication.GUI
{
    partial class RS232SettingGUI
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panelMain = new System.Windows.Forms.Panel();
            this.gComponent = new System.Windows.Forms.GroupBox();
            this.StopBitCombo = new System.Windows.Forms.ComboBox();
            this.DataBitCombo = new System.Windows.Forms.ComboBox();
            this.ParityCombo = new System.Windows.Forms.ComboBox();
            this.BaudRateCombo = new System.Windows.Forms.ComboBox();
            this.ComPortCombo = new System.Windows.Forms.ComboBox();
            this.Stopbits = new System.Windows.Forms.Label();
            this.Databits = new System.Windows.Forms.Label();
            this.Baud = new System.Windows.Forms.Label();
            this.Port = new System.Windows.Forms.Label();
            this.Parity = new System.Windows.Forms.Label();
            this.button_save = new System.Windows.Forms.Button();
            this.button_default_setting = new System.Windows.Forms.Button();
            this.btn_Apply = new System.Windows.Forms.Button();
            this.panelMain.SuspendLayout();
            this.gComponent.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.gComponent);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(320, 275);
            this.panelMain.TabIndex = 1;
            // 
            // gComponent
            // 
            this.gComponent.Controls.Add(this.StopBitCombo);
            this.gComponent.Controls.Add(this.DataBitCombo);
            this.gComponent.Controls.Add(this.ParityCombo);
            this.gComponent.Controls.Add(this.BaudRateCombo);
            this.gComponent.Controls.Add(this.ComPortCombo);
            this.gComponent.Controls.Add(this.Stopbits);
            this.gComponent.Controls.Add(this.Databits);
            this.gComponent.Controls.Add(this.Baud);
            this.gComponent.Controls.Add(this.Port);
            this.gComponent.Controls.Add(this.Parity);
            this.gComponent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gComponent.Location = new System.Drawing.Point(0, 0);
            this.gComponent.Name = "gComponent";
            this.gComponent.Size = new System.Drawing.Size(320, 275);
            this.gComponent.TabIndex = 1;
            this.gComponent.TabStop = false;
            this.gComponent.Text = "RS232 Setting";
            // 
            // StopBitCombo
            // 
            this.StopBitCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.StopBitCombo.Items.AddRange(new object[] {
            "1",
            "2"});
            this.StopBitCombo.Location = new System.Drawing.Point(167, 230);
            this.StopBitCombo.Name = "StopBitCombo";
            this.StopBitCombo.Size = new System.Drawing.Size(80, 21);
            this.StopBitCombo.TabIndex = 9;
            this.StopBitCombo.SelectedIndexChanged += new System.EventHandler(this.ComboBox_SelectedIndexChanged);
            // 
            // DataBitCombo
            // 
            this.DataBitCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DataBitCombo.Items.AddRange(new object[] {
            "5",
            "6",
            "7",
            "8"});
            this.DataBitCombo.Location = new System.Drawing.Point(167, 180);
            this.DataBitCombo.Name = "DataBitCombo";
            this.DataBitCombo.Size = new System.Drawing.Size(80, 21);
            this.DataBitCombo.TabIndex = 7;
            this.DataBitCombo.SelectedIndexChanged += new System.EventHandler(this.ComboBox_SelectedIndexChanged);
            // 
            // ParityCombo
            // 
            this.ParityCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ParityCombo.Items.AddRange(new object[] {
            "NO",
            "ODD",
            "EVEN",
            "MARK",
            "SPACE"});
            this.ParityCombo.Location = new System.Drawing.Point(167, 130);
            this.ParityCombo.Name = "ParityCombo";
            this.ParityCombo.Size = new System.Drawing.Size(80, 21);
            this.ParityCombo.TabIndex = 5;
            this.ParityCombo.SelectedIndexChanged += new System.EventHandler(this.ComboBox_SelectedIndexChanged);
            // 
            // BaudRateCombo
            // 
            this.BaudRateCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.BaudRateCombo.Items.AddRange(new object[] {
            "9600",
            "19200",
            "28800",
            "38400",
            "57600",
            "115200"});
            this.BaudRateCombo.Location = new System.Drawing.Point(167, 80);
            this.BaudRateCombo.Name = "BaudRateCombo";
            this.BaudRateCombo.Size = new System.Drawing.Size(80, 21);
            this.BaudRateCombo.TabIndex = 3;
            this.BaudRateCombo.SelectedIndexChanged += new System.EventHandler(this.ComboBox_SelectedIndexChanged);
            // 
            // ComPortCombo
            // 
            this.ComPortCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComPortCombo.Items.AddRange(new object[] {
            "COM1",
            "COM2",
            "COM3",
            "COM4",
            "COM5",
            "COM6",
            "COM7",
            "COM8",
            "COM9",
            "COM10",
            "COM11",
            "COM12",
            "COM13",
            "COM14",
            "COM15",
            "COM16",
            "COM17",
            "COM18"});
            this.ComPortCombo.Location = new System.Drawing.Point(167, 30);
            this.ComPortCombo.Name = "ComPortCombo";
            this.ComPortCombo.Size = new System.Drawing.Size(80, 21);
            this.ComPortCombo.TabIndex = 1;
            this.ComPortCombo.SelectedIndexChanged += new System.EventHandler(this.ComboBox_SelectedIndexChanged);
            // 
            // Stopbits
            // 
            this.Stopbits.Location = new System.Drawing.Point(59, 233);
            this.Stopbits.Name = "Stopbits";
            this.Stopbits.Size = new System.Drawing.Size(52, 34);
            this.Stopbits.TabIndex = 8;
            this.Stopbits.Text = "Stopbits";
            this.Stopbits.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // Databits
            // 
            this.Databits.Location = new System.Drawing.Point(59, 183);
            this.Databits.Name = "Databits";
            this.Databits.Size = new System.Drawing.Size(52, 34);
            this.Databits.TabIndex = 6;
            this.Databits.Text = "Databits";
            this.Databits.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // Baud
            // 
            this.Baud.Location = new System.Drawing.Point(59, 83);
            this.Baud.Name = "Baud";
            this.Baud.Size = new System.Drawing.Size(52, 34);
            this.Baud.TabIndex = 2;
            this.Baud.Text = "Baud";
            this.Baud.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // Port
            // 
            this.Port.Location = new System.Drawing.Point(59, 33);
            this.Port.Name = "Port";
            this.Port.Size = new System.Drawing.Size(52, 34);
            this.Port.TabIndex = 0;
            this.Port.Text = "Port";
            this.Port.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // Parity
            // 
            this.Parity.Location = new System.Drawing.Point(59, 133);
            this.Parity.Name = "Parity";
            this.Parity.Size = new System.Drawing.Size(52, 34);
            this.Parity.TabIndex = 4;
            this.Parity.Text = "Parity";
            this.Parity.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // button_save
            // 
            this.button_save.Location = new System.Drawing.Point(100, 295);
            this.button_save.Name = "button_save";
            this.button_save.Size = new System.Drawing.Size(75, 23);
            this.button_save.TabIndex = 3;
            this.button_save.Text = "Save";
            this.button_save.UseVisualStyleBackColor = true;
            this.button_save.Click += new System.EventHandler(this.button_save_Click);
            // 
            // button_default_setting
            // 
            this.button_default_setting.Location = new System.Drawing.Point(191, 295);
            this.button_default_setting.Name = "button_default_setting";
            this.button_default_setting.Size = new System.Drawing.Size(126, 23);
            this.button_default_setting.TabIndex = 4;
            this.button_default_setting.Text = "Reset to Default Value";
            this.button_default_setting.UseVisualStyleBackColor = true;
            this.button_default_setting.Click += new System.EventHandler(this.button_default_setting_Click);
            // 
            // btn_Apply
            // 
            this.btn_Apply.Location = new System.Drawing.Point(8, 295);
            this.btn_Apply.Name = "btn_Apply";
            this.btn_Apply.Size = new System.Drawing.Size(75, 23);
            this.btn_Apply.TabIndex = 2;
            this.btn_Apply.Text = "Apply";
            this.btn_Apply.UseVisualStyleBackColor = true;
            this.btn_Apply.Click += new System.EventHandler(this.btn_Apply_Click);
            // 
            // RS232SettingGUI
            // 
            this.Controls.Add(this.btn_Apply);
            this.Controls.Add(this.button_default_setting);
            this.Controls.Add(this.button_save);
            this.Controls.Add(this.panelMain);
            this.Name = "RS232SettingGUI";
            this.Size = new System.Drawing.Size(320, 330);
            this.panelMain.ResumeLayout(false);
            this.gComponent.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.GroupBox gComponent;
        private System.Windows.Forms.Button button_save;
        private System.Windows.Forms.Button button_default_setting;
        private System.Windows.Forms.Label Port;
        private System.Windows.Forms.Label Parity;
        private System.Windows.Forms.Label Baud;
        private System.Windows.Forms.Label Databits;
        private System.Windows.Forms.Label Stopbits;
        private System.Windows.Forms.ComboBox StopBitCombo;
        private System.Windows.Forms.ComboBox DataBitCombo;
        private System.Windows.Forms.ComboBox ParityCombo;
        private System.Windows.Forms.ComboBox BaudRateCombo;
        private System.Windows.Forms.ComboBox ComPortCombo;
        private System.Windows.Forms.Button btn_Apply;
    }
}