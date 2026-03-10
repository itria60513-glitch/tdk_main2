namespace TDKController.GUI
{
    partial class N2NozzleSettingGUI
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
            this.gComponent = new System.Windows.Forms.GroupBox();
            this.cmb_N2NComm = new System.Windows.Forms.ComboBox();
            this.lN2NComm = new System.Windows.Forms.Label();
            this.btn_Apply = new System.Windows.Forms.Button();
            this.button_default_setting = new System.Windows.Forms.Button();
            this.button_save = new System.Windows.Forms.Button();
            this.gComponent.SuspendLayout();
            this.SuspendLayout();
            // 
            // gComponent
            // 
            this.gComponent.Controls.Add(this.cmb_N2NComm);
            this.gComponent.Controls.Add(this.lN2NComm);
            this.gComponent.Dock = System.Windows.Forms.DockStyle.Top;
            this.gComponent.Location = new System.Drawing.Point(0, 0);
            this.gComponent.Name = "gComponent";
            this.gComponent.Size = new System.Drawing.Size(320, 275);
            this.gComponent.TabIndex = 0;
            this.gComponent.TabStop = false;
            this.gComponent.Text = "N2Nozzle Setting";
            // 
            // cmb_N2NComm
            // 
            this.cmb_N2NComm.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_N2NComm.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18"});
            this.cmb_N2NComm.Location = new System.Drawing.Point(167, 30);
            this.cmb_N2NComm.Name = "cmb_N2NComm";
            this.cmb_N2NComm.Size = new System.Drawing.Size(80, 21);
            this.cmb_N2NComm.TabIndex = 2;
            this.cmb_N2NComm.SelectedIndexChanged += new System.EventHandler(this.ComboBox_SelectedIndexChanged);
            // 
            // lN2NComm
            // 
            this.lN2NComm.Location = new System.Drawing.Point(43, 33);
            this.lN2NComm.Name = "lN2NComm";
            this.lN2NComm.Size = new System.Drawing.Size(98, 34);
            this.lN2NComm.TabIndex = 1;
            this.lN2NComm.Text = "N2Nozzle Comm";
            this.lN2NComm.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // btn_Apply
            // 
            this.btn_Apply.Location = new System.Drawing.Point(8, 293);
            this.btn_Apply.Name = "btn_Apply";
            this.btn_Apply.Size = new System.Drawing.Size(75, 23);
            this.btn_Apply.TabIndex = 1;
            this.btn_Apply.Text = "Apply";
            this.btn_Apply.UseVisualStyleBackColor = true;
            this.btn_Apply.Click += new System.EventHandler(this.btn_Apply_Click);
            // 
            // button_default_setting
            // 
            this.button_default_setting.Location = new System.Drawing.Point(191, 293);
            this.button_default_setting.Name = "button_default_setting";
            this.button_default_setting.Size = new System.Drawing.Size(126, 23);
            this.button_default_setting.TabIndex = 3;
            this.button_default_setting.Text = "Reset to Default Value";
            this.button_default_setting.UseVisualStyleBackColor = true;
            this.button_default_setting.Click += new System.EventHandler(this.button_default_setting_Click);
            // 
            // button_save
            // 
            this.button_save.Location = new System.Drawing.Point(100, 293);
            this.button_save.Name = "button_save";
            this.button_save.Size = new System.Drawing.Size(75, 23);
            this.button_save.TabIndex = 2;
            this.button_save.Text = "Save";
            this.button_save.UseVisualStyleBackColor = true;
            this.button_save.Click += new System.EventHandler(this.button_save_Click);
            // 
            // N2NozzleSettingGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btn_Apply);
            this.Controls.Add(this.gComponent);
            this.Controls.Add(this.button_default_setting);
            this.Controls.Add(this.button_save);
            this.Name = "N2NozzleSettingGUI";
            this.Size = new System.Drawing.Size(320, 330);
            this.gComponent.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gComponent;
        private System.Windows.Forms.Label lN2NComm;
        private System.Windows.Forms.Button btn_Apply;
        private System.Windows.Forms.Button button_default_setting;
        private System.Windows.Forms.Button button_save;
        private System.Windows.Forms.ComboBox cmb_N2NComm;
    }
}