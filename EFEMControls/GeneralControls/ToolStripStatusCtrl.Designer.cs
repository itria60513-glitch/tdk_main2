namespace EFEM.GUIControls
{
    partial class StatusIndication
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StatusIndication));
            this.labelCaption = new System.Windows.Forms.Label();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.panelImage = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // labelCaption
            // 
            this.labelCaption.AutoSize = true;
            this.labelCaption.Dock = System.Windows.Forms.DockStyle.Left;
            this.labelCaption.Location = new System.Drawing.Point(0, 0);
            this.labelCaption.Name = "labelCaption";
            this.labelCaption.Size = new System.Drawing.Size(65, 12);
            this.labelCaption.TabIndex = 0;
            this.labelCaption.Text = "EFEM Status";
            this.labelCaption.DoubleClick += new System.EventHandler(this.labelCaption_DoubleClick);
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "Red.ico");
            this.imageList.Images.SetKeyName(1, "Yellow.ico");
            this.imageList.Images.SetKeyName(2, "Green.ico");
            this.imageList.Images.SetKeyName(3, "Blue.ico");
            this.imageList.Images.SetKeyName(4, "Stop.ico");
            this.imageList.Images.SetKeyName(5, "Check.ico");
            this.imageList.Images.SetKeyName(6, "Error.ico");
            // 
            // panelImage
            // 
            this.panelImage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.panelImage.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelImage.Location = new System.Drawing.Point(81, 0);
            this.panelImage.Name = "panelImage";
            this.panelImage.Padding = new System.Windows.Forms.Padding(5);
            this.panelImage.Size = new System.Drawing.Size(20, 17);
            this.panelImage.TabIndex = 1;
            this.panelImage.DoubleClick += new System.EventHandler(this.panelImage_DoubleClick);
            // 
            // StatusIndication
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.panelImage);
            this.Controls.Add(this.labelCaption);
            this.Name = "StatusIndication";
            this.Size = new System.Drawing.Size(101, 17);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelCaption;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.Panel panelImage;
    }
}
