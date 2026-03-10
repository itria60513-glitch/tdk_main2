namespace EFEM.GUIControls
{
    partial class TeachMap
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TeachMap));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.cmbDestination = new System.Windows.Forms.ComboBox();
            this.pBackground = new System.Windows.Forms.Panel();
            this.pLP3 = new System.Windows.Forms.Panel();
            this.pLP2 = new System.Windows.Forms.Panel();
            this.pLL = new System.Windows.Forms.Panel();
            this.pRobot = new System.Windows.Forms.Panel();
            this.pAligner = new System.Windows.Forms.Panel();
            this.pLP1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1.SuspendLayout();
            this.pBackground.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.cmbDestination, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.pBackground, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8.970976F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 91.02902F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(343, 379);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // cmbDestination
            // 
            this.cmbDestination.BackColor = System.Drawing.Color.RoyalBlue;
            this.cmbDestination.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbDestination.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbDestination.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDestination.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.cmbDestination.ForeColor = System.Drawing.Color.White;
            this.cmbDestination.FormattingEnabled = true;
            this.cmbDestination.Items.AddRange(new object[] {
            "<< Free Run >>",
            "LP1",
            "LP2",
            "LP3",
            "ALIGNER",
            "LL2"});
            this.cmbDestination.Location = new System.Drawing.Point(3, 3);
            this.cmbDestination.Name = "cmbDestination";
            this.cmbDestination.Size = new System.Drawing.Size(337, 27);
            this.cmbDestination.TabIndex = 3;
            this.cmbDestination.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.cmbDestination_DrawItem);
            this.cmbDestination.SelectedIndexChanged += new System.EventHandler(this.cmbDestination_SelectedIndexChanged);
            // 
            // pBackground
            // 
            this.pBackground.BackColor = System.Drawing.SystemColors.Control;
            this.pBackground.Controls.Add(this.pLP3);
            this.pBackground.Controls.Add(this.pLP2);
            this.pBackground.Controls.Add(this.pLL);
            this.pBackground.Controls.Add(this.pRobot);
            this.pBackground.Controls.Add(this.pAligner);
            this.pBackground.Controls.Add(this.pLP1);
            this.pBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pBackground.Location = new System.Drawing.Point(3, 36);
            this.pBackground.Name = "pBackground";
            this.pBackground.Size = new System.Drawing.Size(337, 340);
            this.pBackground.TabIndex = 4;
            // 
            // pLP3
            // 
            this.pLP3.BackgroundImage = global::EFEM.GUIControls.Properties.Resources.LP;
            this.pLP3.Location = new System.Drawing.Point(75, 9);
            this.pLP3.Name = "pLP3";
            this.pLP3.Size = new System.Drawing.Size(68, 100);
            this.pLP3.TabIndex = 2;
            // 
            // pLP2
            // 
            this.pLP2.BackgroundImage = global::EFEM.GUIControls.Properties.Resources.LP;
            this.pLP2.Location = new System.Drawing.Point(160, 9);
            this.pLP2.Name = "pLP2";
            this.pLP2.Size = new System.Drawing.Size(68, 100);
            this.pLP2.TabIndex = 1;
            // 
            // pLL
            // 
            this.pLL.BackgroundImage = global::EFEM.GUIControls.Properties.Resources.LL;
            this.pLL.Location = new System.Drawing.Point(104, 250);
            this.pLL.Name = "pLL";
            this.pLL.Size = new System.Drawing.Size(160, 80);
            this.pLL.TabIndex = 0;
            // 
            // pRobot
            // 
            this.pRobot.BackgroundImage = global::EFEM.GUIControls.Properties.Resources.Robot;
            this.pRobot.Location = new System.Drawing.Point(139, 123);
            this.pRobot.Name = "pRobot";
            this.pRobot.Size = new System.Drawing.Size(100, 117);
            this.pRobot.TabIndex = 0;
            // 
            // pAligner
            // 
            this.pAligner.BackgroundImage = global::EFEM.GUIControls.Properties.Resources.Aligner_Map;
            this.pAligner.Location = new System.Drawing.Point(4, 134);
            this.pAligner.Name = "pAligner";
            this.pAligner.Size = new System.Drawing.Size(86, 100);
            this.pAligner.TabIndex = 0;
            // 
            // pLP1
            // 
            this.pLP1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pLP1.BackgroundImage")));
            this.pLP1.Location = new System.Drawing.Point(243, 9);
            this.pLP1.Name = "pLP1";
            this.pLP1.Size = new System.Drawing.Size(68, 100);
            this.pLP1.TabIndex = 0;
            // 
            // TeachMap
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "TeachMap";
            this.Size = new System.Drawing.Size(343, 379);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.pBackground.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel pBackground;
        private System.Windows.Forms.Panel pLL;
        private System.Windows.Forms.Panel pRobot;
        private System.Windows.Forms.Panel pAligner;
        private System.Windows.Forms.Panel pLP1;
        private System.Windows.Forms.Panel pLP3;
        private System.Windows.Forms.Panel pLP2;
        private System.Windows.Forms.ComboBox cmbDestination;


    }
}
