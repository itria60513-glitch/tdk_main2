namespace DIO.GUI
{
    partial class DIOSettingGUI
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
            this.tb_DOPortMax = new System.Windows.Forms.TextBox();
            this.tb_DIPortMax = new System.Windows.Forms.TextBox();
            this.cmb_Index = new System.Windows.Forms.ComboBox();
            this.cmb_Type = new System.Windows.Forms.ComboBox();
            this.lDOPortMax = new System.Windows.Forms.Label();
            this.lIndex = new System.Windows.Forms.Label();
            this.lType = new System.Windows.Forms.Label();
            this.lDIPortMax = new System.Windows.Forms.Label();
            this.button_save = new System.Windows.Forms.Button();
            this.button_default_setting = new System.Windows.Forms.Button();
            this.btn_Apply = new System.Windows.Forms.Button();
            this.lPinCountPerPort = new System.Windows.Forms.Label();
            this.tb_PinCountPerPort = new System.Windows.Forms.TextBox();
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
            this.gComponent.Controls.Add(this.tb_PinCountPerPort);
            this.gComponent.Controls.Add(this.lPinCountPerPort);
            this.gComponent.Controls.Add(this.tb_DOPortMax);
            this.gComponent.Controls.Add(this.tb_DIPortMax);
            this.gComponent.Controls.Add(this.cmb_Index);
            this.gComponent.Controls.Add(this.cmb_Type);
            this.gComponent.Controls.Add(this.lDOPortMax);
            this.gComponent.Controls.Add(this.lIndex);
            this.gComponent.Controls.Add(this.lType);
            this.gComponent.Controls.Add(this.lDIPortMax);
            this.gComponent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gComponent.Location = new System.Drawing.Point(0, 0);
            this.gComponent.Name = "gComponent";
            this.gComponent.Size = new System.Drawing.Size(320, 275);
            this.gComponent.TabIndex = 1;
            this.gComponent.TabStop = false;
            this.gComponent.Text = "IO Board Setting";
            // 
            // tb_DOPortMax
            // 
            this.tb_DOPortMax.Location = new System.Drawing.Point(167, 180);
            this.tb_DOPortMax.Name = "tb_DOPortMax";
            this.tb_DOPortMax.Size = new System.Drawing.Size(80, 20);
            this.tb_DOPortMax.TabIndex = 8;
            this.tb_DOPortMax.TextChanged += new System.EventHandler(this.TextBox_ValueChanged);
            // 
            // tb_DIPortMax
            // 
            this.tb_DIPortMax.Location = new System.Drawing.Point(167, 130);
            this.tb_DIPortMax.Name = "tb_DIPortMax";
            this.tb_DIPortMax.Size = new System.Drawing.Size(80, 20);
            this.tb_DIPortMax.TabIndex = 7;
            this.tb_DIPortMax.TextChanged += new System.EventHandler(this.TextBox_ValueChanged);
            // 
            // cmb_Index
            // 
            this.cmb_Index.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_Index.Items.AddRange(new object[] {
            "0",
            "1"});
            this.cmb_Index.Location = new System.Drawing.Point(167, 80);
            this.cmb_Index.Name = "cmb_Index";
            this.cmb_Index.Size = new System.Drawing.Size(80, 21);
            this.cmb_Index.TabIndex = 3;
            this.cmb_Index.SelectedIndexChanged += new System.EventHandler(this.ComboBox_SelectedIndexChanged);
            // 
            // cmb_Type
            // 
            this.cmb_Type.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_Type.Items.AddRange(new object[] {
            "Advan",
            "Virtual"});
            this.cmb_Type.Location = new System.Drawing.Point(167, 30);
            this.cmb_Type.Name = "cmb_Type";
            this.cmb_Type.Size = new System.Drawing.Size(80, 21);
            this.cmb_Type.TabIndex = 1;
            this.cmb_Type.SelectedIndexChanged += new System.EventHandler(this.ComboBox_SelectedIndexChanged);
            // 
            // lDOPortMax
            // 
            this.lDOPortMax.Location = new System.Drawing.Point(38, 183);
            this.lDOPortMax.Name = "lDOPortMax";
            this.lDOPortMax.Size = new System.Drawing.Size(93, 34);
            this.lDOPortMax.TabIndex = 6;
            this.lDOPortMax.Text = "DOPortMax";
            this.lDOPortMax.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lIndex
            // 
            this.lIndex.Location = new System.Drawing.Point(79, 83);
            this.lIndex.Name = "lIndex";
            this.lIndex.Size = new System.Drawing.Size(52, 34);
            this.lIndex.TabIndex = 2;
            this.lIndex.Text = "Index";
            this.lIndex.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lType
            // 
            this.lType.Location = new System.Drawing.Point(79, 33);
            this.lType.Name = "lType";
            this.lType.Size = new System.Drawing.Size(52, 34);
            this.lType.TabIndex = 0;
            this.lType.Text = "Type";
            this.lType.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lDIPortMax
            // 
            this.lDIPortMax.Location = new System.Drawing.Point(69, 133);
            this.lDIPortMax.Name = "lDIPortMax";
            this.lDIPortMax.Size = new System.Drawing.Size(62, 34);
            this.lDIPortMax.TabIndex = 4;
            this.lDIPortMax.Text = "DIPortMax";
            this.lDIPortMax.TextAlign = System.Drawing.ContentAlignment.TopRight;
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
            // lPinCountPerPort
            // 
            this.lPinCountPerPort.Location = new System.Drawing.Point(38, 233);
            this.lPinCountPerPort.Name = "lPinCountPerPort";
            this.lPinCountPerPort.Size = new System.Drawing.Size(93, 34);
            this.lPinCountPerPort.TabIndex = 9;
            this.lPinCountPerPort.Text = "PinCountPerPort";
            this.lPinCountPerPort.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // tb_PinCountPerPort
            // 
            this.tb_PinCountPerPort.Location = new System.Drawing.Point(167, 230);
            this.tb_PinCountPerPort.Name = "tb_PinCountPerPort";
            this.tb_PinCountPerPort.Size = new System.Drawing.Size(80, 20);
            this.tb_PinCountPerPort.TabIndex = 10;
            this.tb_PinCountPerPort.TextChanged += new System.EventHandler(this.TextBox_ValueChanged);
            // 
            // DIOSettingGUI
            // 
            this.Controls.Add(this.btn_Apply);
            this.Controls.Add(this.button_default_setting);
            this.Controls.Add(this.button_save);
            this.Controls.Add(this.panelMain);
            this.Name = "DIOSettingGUI";
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
        private System.Windows.Forms.Label lType;
        private System.Windows.Forms.Label lDIPortMax;
        private System.Windows.Forms.Label lIndex;
        private System.Windows.Forms.ComboBox cmb_Index;
        private System.Windows.Forms.ComboBox cmb_Type;
        private System.Windows.Forms.Button btn_Apply;
        private System.Windows.Forms.Label lDOPortMax;
        private System.Windows.Forms.TextBox tb_DOPortMax;
        private System.Windows.Forms.TextBox tb_DIPortMax;
        private System.Windows.Forms.TextBox tb_PinCountPerPort;
        private System.Windows.Forms.Label lPinCountPerPort;
    }
}