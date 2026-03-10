namespace Communication.GUI
{
    partial class TCPIPSettingGUI
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
            this.TCPAddressEdit = new System.Windows.Forms.TextBox();
            this.TCPPortEdit = new System.Windows.Forms.TextBox();
            this.lPort = new System.Windows.Forms.Label();
            this.lAddress = new System.Windows.Forms.Label();
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
            this.gComponent.Controls.Add(this.TCPAddressEdit);
            this.gComponent.Controls.Add(this.TCPPortEdit);
            this.gComponent.Controls.Add(this.lPort);
            this.gComponent.Controls.Add(this.lAddress);
            this.gComponent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gComponent.Location = new System.Drawing.Point(0, 0);
            this.gComponent.Name = "gComponent";
            this.gComponent.Size = new System.Drawing.Size(320, 275);
            this.gComponent.TabIndex = 0;
            this.gComponent.TabStop = false;
            this.gComponent.Text = "TCPIP Setting";
            // 
            // TCPAddressEdit
            // 
            this.TCPAddressEdit.Location = new System.Drawing.Point(155, 37);
            this.TCPAddressEdit.Name = "TCPAddressEdit";
            this.TCPAddressEdit.Size = new System.Drawing.Size(120, 20);
            this.TCPAddressEdit.TabIndex = 1;
            this.TCPAddressEdit.TextChanged += new System.EventHandler(this.TCPAddressEdit_ValueChanged);
            // 
            // TCPPortEdit
            // 
            this.TCPPortEdit.Location = new System.Drawing.Point(155, 87);
            this.TCPPortEdit.Name = "TCPPortEdit";
            this.TCPPortEdit.Size = new System.Drawing.Size(120, 20);
            this.TCPPortEdit.TabIndex = 3;
            this.TCPPortEdit.TextChanged += new System.EventHandler(this.TCPPortEdit_ValueChanged);
            // 
            // lPort
            // 
            this.lPort.Location = new System.Drawing.Point(63, 90);
            this.lPort.Name = "lPort";
            this.lPort.Size = new System.Drawing.Size(58, 32);
            this.lPort.TabIndex = 2;
            this.lPort.Text = "Port";
            this.lPort.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lAddress
            // 
            this.lAddress.Location = new System.Drawing.Point(63, 40);
            this.lAddress.Name = "lAddress";
            this.lAddress.Size = new System.Drawing.Size(58, 32);
            this.lAddress.TabIndex = 0;
            this.lAddress.Text = "IP Address";
            this.lAddress.TextAlign = System.Drawing.ContentAlignment.TopRight;
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
            // TCPIPSettingGUI
            // 
            this.Controls.Add(this.btn_Apply);
            this.Controls.Add(this.button_default_setting);
            this.Controls.Add(this.button_save);
            this.Controls.Add(this.panelMain);
            this.Name = "TCPIPSettingGUI";
            this.Size = new System.Drawing.Size(320, 330);
            this.panelMain.ResumeLayout(false);
            this.gComponent.ResumeLayout(false);
            this.gComponent.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.GroupBox gComponent;
        private System.Windows.Forms.Button button_save;
        private System.Windows.Forms.Button button_default_setting;
        private System.Windows.Forms.TextBox TCPPortEdit;
        private System.Windows.Forms.Label lPort;
        private System.Windows.Forms.Label lAddress;
        private System.Windows.Forms.TextBox TCPAddressEdit;
        private System.Windows.Forms.Button btn_Apply;
    }
}