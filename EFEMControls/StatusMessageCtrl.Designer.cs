namespace EFEM.GUIControls
{
    partial class StatusMessageCtrl
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
            this.listStatusHistory = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.clearAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lTitle = new System.Windows.Forms.Label();
            this.cbPause = new System.Windows.Forms.CheckBox();
            this.panelUpper = new System.Windows.Forms.Panel();
            this.panelMain = new System.Windows.Forms.Panel();
            this.contextMenuStrip1.SuspendLayout();
            this.panelUpper.SuspendLayout();
            this.panelMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // listStatusHistory
            // 
            this.listStatusHistory.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listStatusHistory.ContextMenuStrip = this.contextMenuStrip1;
            this.listStatusHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listStatusHistory.FullRowSelect = true;
            this.listStatusHistory.GridLines = true;
            this.listStatusHistory.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listStatusHistory.HideSelection = false;
            this.listStatusHistory.Location = new System.Drawing.Point(0, 24);
            this.listStatusHistory.Name = "listStatusHistory";
            this.listStatusHistory.Size = new System.Drawing.Size(537, 273);
            this.listStatusHistory.TabIndex = 1;
            this.listStatusHistory.UseCompatibleStateImageBehavior = false;
            this.listStatusHistory.View = System.Windows.Forms.View.List;
            this.listStatusHistory.VirtualMode = true;
            this.listStatusHistory.Visible = false;
            this.listStatusHistory.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.listStatusHistory_RetrieveVirtualItem);
            this.listStatusHistory.DoubleClick += new System.EventHandler(this.listStatusHistory_DoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "DateTime";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Context";
            this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader2.Width = 9999;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyAllToolStripMenuItem,
            this.toolStripMenuItem1,
            this.clearAllToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(120, 54);
            // 
            // copyAllToolStripMenuItem
            // 
            this.copyAllToolStripMenuItem.Name = "copyAllToolStripMenuItem";
            this.copyAllToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            this.copyAllToolStripMenuItem.Text = "Copy All";
            this.copyAllToolStripMenuItem.Click += new System.EventHandler(this.copyAllToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(116, 6);
            // 
            // clearAllToolStripMenuItem
            // 
            this.clearAllToolStripMenuItem.Name = "clearAllToolStripMenuItem";
            this.clearAllToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            this.clearAllToolStripMenuItem.Text = "Clear All";
            this.clearAllToolStripMenuItem.Click += new System.EventHandler(this.clearAllToolStripMenuItem_Click);
            // 
            // lTitle
            // 
            this.lTitle.BackColor = System.Drawing.Color.RoyalBlue;
            this.lTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lTitle.ForeColor = System.Drawing.Color.White;
            this.lTitle.Location = new System.Drawing.Point(0, 0);
            this.lTitle.Name = "lTitle";
            this.lTitle.Size = new System.Drawing.Size(481, 24);
            this.lTitle.TabIndex = 0;
            this.lTitle.Text = "Status Message";
            this.lTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lTitle.DoubleClick += new System.EventHandler(this.lTitle_DoubleClick);
            // 
            // cbPause
            // 
            this.cbPause.AutoSize = true;
            this.cbPause.BackColor = System.Drawing.Color.RoyalBlue;
            this.cbPause.Dock = System.Windows.Forms.DockStyle.Right;
            this.cbPause.ForeColor = System.Drawing.Color.White;
            this.cbPause.Location = new System.Drawing.Point(481, 0);
            this.cbPause.Name = "cbPause";
            this.cbPause.Size = new System.Drawing.Size(56, 24);
            this.cbPause.TabIndex = 1;
            this.cbPause.Text = "Pause";
            this.cbPause.UseVisualStyleBackColor = false;
            this.cbPause.Visible = false;
            this.cbPause.CheckedChanged += new System.EventHandler(this.cbPause_CheckedChanged);
            // 
            // panelUpper
            // 
            this.panelUpper.Controls.Add(this.lTitle);
            this.panelUpper.Controls.Add(this.cbPause);
            this.panelUpper.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelUpper.Location = new System.Drawing.Point(0, 0);
            this.panelUpper.Name = "panelUpper";
            this.panelUpper.Size = new System.Drawing.Size(537, 24);
            this.panelUpper.TabIndex = 5;
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.listStatusHistory);
            this.panelMain.Controls.Add(this.panelUpper);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(537, 297);
            this.panelMain.TabIndex = 6;
            // 
            // StatusMessageCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelMain);
            this.Name = "StatusMessageCtrl";
            this.Size = new System.Drawing.Size(537, 297);
            this.contextMenuStrip1.ResumeLayout(false);
            this.panelUpper.ResumeLayout(false);
            this.panelUpper.PerformLayout();
            this.panelMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listStatusHistory;
        private System.Windows.Forms.Label lTitle;
        private System.Windows.Forms.CheckBox cbPause;
        private System.Windows.Forms.Panel panelUpper;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem copyAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem clearAllToolStripMenuItem;
        private System.Windows.Forms.Panel panelMain;
    }
}
