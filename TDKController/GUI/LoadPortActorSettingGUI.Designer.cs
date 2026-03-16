namespace TDKController.GUI
{
    partial class LoadPortSettingGUI
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
            this.lABSSec = new System.Windows.Forms.Label();
            this.txt_INFTimeout = new System.Windows.Forms.TextBox();
            this.lINFTimeout = new System.Windows.Forms.Label();
            this.lACKSec = new System.Windows.Forms.Label();
            this.txt_ACKTimeout = new System.Windows.Forms.TextBox();
            this.lACKTimeout = new System.Windows.Forms.Label();
            this.cmb_LPActor = new System.Windows.Forms.ComboBox();
            this.lLPActor = new System.Windows.Forms.Label();
            this.btn_Apply = new System.Windows.Forms.Button();
            this.button_default_setting = new System.Windows.Forms.Button();
            this.button_save = new System.Windows.Forms.Button();
            this.gComponent.SuspendLayout();
            this.SuspendLayout();
            // 
            // gComponent
            // 
            this.gComponent.Controls.Add(this.lABSSec);
            this.gComponent.Controls.Add(this.txt_INFTimeout);
            this.gComponent.Controls.Add(this.lINFTimeout);
            this.gComponent.Controls.Add(this.lACKSec);
            this.gComponent.Controls.Add(this.txt_ACKTimeout);
            this.gComponent.Controls.Add(this.lACKTimeout);
            this.gComponent.Controls.Add(this.cmb_LPActor);
            this.gComponent.Controls.Add(this.lLPActor);
            this.gComponent.Dock = System.Windows.Forms.DockStyle.Top;
            this.gComponent.Location = new System.Drawing.Point(0, 0);
            this.gComponent.Name = "gComponent";
            this.gComponent.Size = new System.Drawing.Size(320, 275);
            this.gComponent.TabIndex = 0;
            this.gComponent.TabStop = false;
            this.gComponent.Text = "LoadPort Setting";
            // 
            // lABSSec
            // 
            this.lABSSec.Location = new System.Drawing.Point(271, 133);
            this.lABSSec.Name = "lABSSec";
            this.lABSSec.Size = new System.Drawing.Size(32, 20);
            this.lABSSec.TabIndex = 8;
            this.lABSSec.Text = "Secs";
            // 
            // txt_INFTimeout
            // 
            this.txt_INFTimeout.Location = new System.Drawing.Point(167, 130);
            this.txt_INFTimeout.Name = "txt_INFTimeout";
            this.txt_INFTimeout.Size = new System.Drawing.Size(89, 20);
            this.txt_INFTimeout.TabIndex = 7;
            this.txt_INFTimeout.TextChanged += new System.EventHandler(this.TextBox_ValueChanged);
            // 
            // lINFTimeout
            // 
            this.lINFTimeout.Location = new System.Drawing.Point(33, 133);
            this.lINFTimeout.Name = "lINFTimeout";
            this.lINFTimeout.Size = new System.Drawing.Size(98, 34);
            this.lINFTimeout.TabIndex = 6;
            this.lINFTimeout.Text = "INF Timeout";
            this.lINFTimeout.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lACKSec
            // 
            this.lACKSec.Location = new System.Drawing.Point(271, 80);
            this.lACKSec.Name = "lACKSec";
            this.lACKSec.Size = new System.Drawing.Size(32, 20);
            this.lACKSec.TabIndex = 5;
            this.lACKSec.Text = "Secs";
            // 
            // txt_ACKTimeout
            // 
            this.txt_ACKTimeout.Location = new System.Drawing.Point(167, 77);
            this.txt_ACKTimeout.Name = "txt_ACKTimeout";
            this.txt_ACKTimeout.Size = new System.Drawing.Size(89, 20);
            this.txt_ACKTimeout.TabIndex = 4;
            this.txt_ACKTimeout.TextChanged += new System.EventHandler(this.TextBox_ValueChanged);
            // 
            // lACKTimeout
            // 
            this.lACKTimeout.Location = new System.Drawing.Point(33, 80);
            this.lACKTimeout.Name = "lACKTimeout";
            this.lACKTimeout.Size = new System.Drawing.Size(98, 34);
            this.lACKTimeout.TabIndex = 3;
            this.lACKTimeout.Text = "ACK Timeout";
            this.lACKTimeout.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // cmb_LPActor
            // 
            this.cmb_LPActor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_LPActor.Items.AddRange(new object[] {
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
            this.cmb_LPActor.Location = new System.Drawing.Point(167, 30);
            this.cmb_LPActor.Name = "cmb_LPActor";
            this.cmb_LPActor.Size = new System.Drawing.Size(80, 21);
            this.cmb_LPActor.TabIndex = 2;
            this.cmb_LPActor.SelectedIndexChanged += new System.EventHandler(this.ComboBox_SelectedIndexChanged);
            // 
            // lLPActor
            // 
            this.lLPActor.Location = new System.Drawing.Point(33, 33);
            this.lLPActor.Name = "lLPActor";
            this.lLPActor.Size = new System.Drawing.Size(98, 34);
            this.lLPActor.TabIndex = 1;
            this.lLPActor.Text = "LoadPort Comm";
            this.lLPActor.TextAlign = System.Drawing.ContentAlignment.TopRight;
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
            // LoadPortSettingGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btn_Apply);
            this.Controls.Add(this.gComponent);
            this.Controls.Add(this.button_default_setting);
            this.Controls.Add(this.button_save);
            this.Name = "LoadPortSettingGUI";
            this.Size = new System.Drawing.Size(320, 330);
            this.gComponent.ResumeLayout(false);
            this.gComponent.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gComponent;
        private System.Windows.Forms.Label lLPActor;
        private System.Windows.Forms.Button btn_Apply;
        private System.Windows.Forms.Button button_default_setting;
        private System.Windows.Forms.Button button_save;
        private System.Windows.Forms.ComboBox cmb_LPActor;
        private System.Windows.Forms.Label lACKTimeout;
        private System.Windows.Forms.TextBox txt_ACKTimeout;
        private System.Windows.Forms.Label lABSSec;
        private System.Windows.Forms.TextBox txt_INFTimeout;
        private System.Windows.Forms.Label lINFTimeout;
        private System.Windows.Forms.Label lACKSec;
    }
}