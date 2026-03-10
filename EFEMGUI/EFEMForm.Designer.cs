namespace EFEMGUI
{
    partial class EFEMForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EFEMForm));
            this.efemMainControl = new EFEM.GUIControls.EFEMMainControl();
            this.SuspendLayout();
            // 
            // efemMainControl
            // 
            this.efemMainControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.efemMainControl.Location = new System.Drawing.Point(0, 0);
            this.efemMainControl.Name = "efemMainControl";
            this.efemMainControl.Size = new System.Drawing.Size(1070, 573);
            this.efemMainControl.TabIndex = 0;
            // 
            // EFEMForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1070, 573);
            this.Controls.Add(this.efemMainControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "EFEMForm";
            this.Text = "EFEM GUI";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ResumeLayout(false);

        }

        #endregion

        private EFEM.GUIControls.EFEMMainControl efemMainControl;
    }
}

