namespace EFEM.GUIControls
{
    partial class StartupComponentCtrl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StartupComponentCtrl));
            this.lCurrentStatus = new System.Windows.Forms.Label();
            this.dataGridViewResult = new System.Windows.Forms.DataGridView();
            this.Image = new System.Windows.Forms.DataGridViewImageColumn();
            this.Status = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ObjectName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ErrorMsg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageListStatus = new System.Windows.Forms.ImageList(this.components);
            this.panelMain = new System.Windows.Forms.Panel();
            this.btnContinue = new System.Windows.Forms.Button();
            this.timerBlinking = new System.Windows.Forms.Timer(this.components);
            this.panelDataGrid = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewResult)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.panelMain.SuspendLayout();
            this.panelDataGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // lCurrentStatus
            // 
            this.lCurrentStatus.BackColor = System.Drawing.SystemColors.Control;
            this.lCurrentStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lCurrentStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lCurrentStatus.Location = new System.Drawing.Point(0, 0);
            this.lCurrentStatus.Name = "lCurrentStatus";
            this.lCurrentStatus.Size = new System.Drawing.Size(654, 44);
            this.lCurrentStatus.TabIndex = 0;
            this.lCurrentStatus.Text = "<Unknown>";
            this.lCurrentStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lCurrentStatus.Visible = false;
            // 
            // dataGridViewResult
            // 
            this.dataGridViewResult.AccessibleRole = System.Windows.Forms.AccessibleRole.TitleBar;
            this.dataGridViewResult.AllowUserToAddRows = false;
            this.dataGridViewResult.AllowUserToDeleteRows = false;
            this.dataGridViewResult.AllowUserToResizeRows = false;
            this.dataGridViewResult.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewResult.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Image,
            this.Status,
            this.ObjectName,
            this.ErrorMsg});
            this.dataGridViewResult.ContextMenuStrip = this.contextMenuStrip1;
            this.dataGridViewResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewResult.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewResult.MultiSelect = false;
            this.dataGridViewResult.Name = "dataGridViewResult";
            this.dataGridViewResult.RowHeadersVisible = false;
            this.dataGridViewResult.ShowEditingIcon = false;
            this.dataGridViewResult.Size = new System.Drawing.Size(783, 213);
            this.dataGridViewResult.TabIndex = 0;
            this.dataGridViewResult.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewResult_CellDoubleClick);
            this.dataGridViewResult.DefaultValuesNeeded += new System.Windows.Forms.DataGridViewRowEventHandler(this.dataGridViewResult_DefaultValuesNeeded);
            // 
            // Image
            // 
            this.Image.Frozen = true;
            this.Image.HeaderText = "";
            this.Image.Name = "Image";
            this.Image.ReadOnly = true;
            this.Image.Width = 50;
            // 
            // Status
            // 
            this.Status.Frozen = true;
            this.Status.HeaderText = "Status";
            this.Status.Name = "Status";
            this.Status.ReadOnly = true;
            this.Status.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // ObjectName
            // 
            this.ObjectName.Frozen = true;
            this.ObjectName.HeaderText = "Object Name";
            this.ObjectName.Name = "ObjectName";
            this.ObjectName.ReadOnly = true;
            this.ObjectName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ObjectName.Width = 150;
            // 
            // ErrorMsg
            // 
            this.ErrorMsg.Frozen = true;
            this.ErrorMsg.HeaderText = "Error Message";
            this.ErrorMsg.Name = "ErrorMsg";
            this.ErrorMsg.ReadOnly = true;
            this.ErrorMsg.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ErrorMsg.Width = 600;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyAllToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(120, 26);
            // 
            // copyAllToolStripMenuItem
            // 
            this.copyAllToolStripMenuItem.Name = "copyAllToolStripMenuItem";
            this.copyAllToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            this.copyAllToolStripMenuItem.Text = "Copy All";
            this.copyAllToolStripMenuItem.Click += new System.EventHandler(this.copyAllToolStripMenuItem_Click);
            // 
            // imageListStatus
            // 
            this.imageListStatus.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListStatus.ImageStream")));
            this.imageListStatus.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListStatus.Images.SetKeyName(0, "wait.png");
            this.imageListStatus.Images.SetKeyName(1, "run.png");
            this.imageListStatus.Images.SetKeyName(2, "success.png");
            this.imageListStatus.Images.SetKeyName(3, "error.png");
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.lCurrentStatus);
            this.panelMain.Controls.Add(this.btnContinue);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(783, 44);
            this.panelMain.TabIndex = 2;
            // 
            // btnContinue
            // 
            this.btnContinue.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnContinue.Location = new System.Drawing.Point(654, 0);
            this.btnContinue.Name = "btnContinue";
            this.btnContinue.Size = new System.Drawing.Size(129, 44);
            this.btnContinue.TabIndex = 1;
            this.btnContinue.Text = "Continue";
            this.btnContinue.UseVisualStyleBackColor = true;
            this.btnContinue.Visible = false;
            this.btnContinue.Click += new System.EventHandler(this.btnContinue_Click);
            // 
            // timerBlinking
            // 
            this.timerBlinking.Interval = 500;
            this.timerBlinking.Tick += new System.EventHandler(this.timerBlinking_Tick);
            // 
            // panelDataGrid
            // 
            this.panelDataGrid.Controls.Add(this.dataGridViewResult);
            this.panelDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDataGrid.Location = new System.Drawing.Point(0, 44);
            this.panelDataGrid.Name = "panelDataGrid";
            this.panelDataGrid.Size = new System.Drawing.Size(783, 213);
            this.panelDataGrid.TabIndex = 3;
            // 
            // StartupComponentCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelDataGrid);
            this.Controls.Add(this.panelMain);
            this.Name = "StartupComponentCtrl";
            this.Size = new System.Drawing.Size(783, 257);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewResult)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.panelMain.ResumeLayout(false);
            this.panelDataGrid.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lCurrentStatus;
        private System.Windows.Forms.DataGridView dataGridViewResult;
        private System.Windows.Forms.ImageList imageListStatus;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Button btnContinue;
        private System.Windows.Forms.Timer timerBlinking;
        private System.Windows.Forms.Panel panelDataGrid;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem copyAllToolStripMenuItem;
        private System.Windows.Forms.DataGridViewImageColumn Image;
        private System.Windows.Forms.DataGridViewTextBoxColumn Status;
        private System.Windows.Forms.DataGridViewTextBoxColumn ObjectName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ErrorMsg;
    }
}
