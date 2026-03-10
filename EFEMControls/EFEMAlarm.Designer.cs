namespace EFEM.GUIControls
{
    partial class EFEMAlarm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EFEMAlarm));
            this.tcAlarm = new System.Windows.Forms.TabControl();
            this.tabActive = new System.Windows.Forms.TabPage();
            this.dgAlarm = new System.Windows.Forms.DataGridView();
            this.bindingNavigatorActive = new System.Windows.Forms.BindingNavigator(this.components);
            this.bindingNavigatorCountItem = new System.Windows.Forms.ToolStripLabel();
            this.bindingNavigatorMoveFirstItem = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorMovePreviousItem = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.bindingNavigatorPositionItem = new System.Windows.Forms.ToolStripTextBox();
            this.bindingNavigatorSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.bindingNavigatorMoveNextItem = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorMoveLastItem = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnRefreshActive = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorDeleteItem = new System.Windows.Forms.ToolStripButton();
            this.tabHistory = new System.Windows.Forms.TabPage();
            this.dgHistory = new System.Windows.Forms.DataGridView();
            this.gFilter = new System.Windows.Forms.GroupBox();
            this.alarmHistoryFilterCtrl1 = new EFEM.GUIControls.AlarmHistoryFilterCtrl();
            this.bindingNavigatorHistory = new System.Windows.Forms.BindingNavigator(this.components);
            this.bindingNavigatorCountItem1 = new System.Windows.Forms.ToolStripLabel();
            this.bindingNavigatorMoveFirstItem1 = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorMovePreviousItem1 = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.bindingNavigatorPositionItem1 = new System.Windows.Forms.ToolStripTextBox();
            this.bindingNavigatorSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.bindingNavigatorMoveNextItem1 = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorMoveLastItem1 = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.btnRefreshHistory = new System.Windows.Forms.ToolStripButton();
            this.btnEnableFilter = new System.Windows.Forms.ToolStripButton();
            this.bindingSourceActive = new System.Windows.Forms.BindingSource(this.components);
            this.bindingSourceHistory = new System.Windows.Forms.BindingSource(this.components);
            this.tcAlarm.SuspendLayout();
            this.tabActive.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgAlarm)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingNavigatorActive)).BeginInit();
            this.bindingNavigatorActive.SuspendLayout();
            this.tabHistory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgHistory)).BeginInit();
            this.gFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingNavigatorHistory)).BeginInit();
            this.bindingNavigatorHistory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceActive)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceHistory)).BeginInit();
            this.SuspendLayout();
            // 
            // tcAlarm
            // 
            this.tcAlarm.Controls.Add(this.tabActive);
            this.tcAlarm.Controls.Add(this.tabHistory);
            this.tcAlarm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcAlarm.Location = new System.Drawing.Point(0, 0);
            this.tcAlarm.Name = "tcAlarm";
            this.tcAlarm.SelectedIndex = 0;
            this.tcAlarm.Size = new System.Drawing.Size(610, 580);
            this.tcAlarm.TabIndex = 0;
            this.tcAlarm.Resize += new System.EventHandler(this.tcAlarm_Resize);
            // 
            // tabActive
            // 
            this.tabActive.Controls.Add(this.dgAlarm);
            this.tabActive.Controls.Add(this.bindingNavigatorActive);
            this.tabActive.Location = new System.Drawing.Point(4, 22);
            this.tabActive.Name = "tabActive";
            this.tabActive.Padding = new System.Windows.Forms.Padding(3);
            this.tabActive.Size = new System.Drawing.Size(602, 554);
            this.tabActive.TabIndex = 0;
            this.tabActive.Text = "Active";
            this.tabActive.UseVisualStyleBackColor = true;
            // 
            // dgAlarm
            // 
            this.dgAlarm.AllowUserToAddRows = false;
            this.dgAlarm.AllowUserToDeleteRows = false;
            this.dgAlarm.AllowUserToResizeRows = false;
            this.dgAlarm.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgAlarm.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgAlarm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgAlarm.Location = new System.Drawing.Point(3, 28);
            this.dgAlarm.MultiSelect = false;
            this.dgAlarm.Name = "dgAlarm";
            this.dgAlarm.ReadOnly = true;
            this.dgAlarm.RowHeadersVisible = false;
            this.dgAlarm.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgAlarm.Size = new System.Drawing.Size(596, 523);
            this.dgAlarm.TabIndex = 1;
            // 
            // bindingNavigatorActive
            // 
            this.bindingNavigatorActive.AddNewItem = null;
            this.bindingNavigatorActive.CountItem = this.bindingNavigatorCountItem;
            this.bindingNavigatorActive.DeleteItem = null;
            this.bindingNavigatorActive.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bindingNavigatorMoveFirstItem,
            this.bindingNavigatorMovePreviousItem,
            this.bindingNavigatorSeparator,
            this.bindingNavigatorPositionItem,
            this.bindingNavigatorCountItem,
            this.bindingNavigatorSeparator1,
            this.bindingNavigatorMoveNextItem,
            this.bindingNavigatorMoveLastItem,
            this.bindingNavigatorSeparator2,
            this.btnRefreshActive,
            this.bindingNavigatorDeleteItem});
            this.bindingNavigatorActive.Location = new System.Drawing.Point(3, 3);
            this.bindingNavigatorActive.MoveFirstItem = this.bindingNavigatorMoveFirstItem;
            this.bindingNavigatorActive.MoveLastItem = this.bindingNavigatorMoveLastItem;
            this.bindingNavigatorActive.MoveNextItem = this.bindingNavigatorMoveNextItem;
            this.bindingNavigatorActive.MovePreviousItem = this.bindingNavigatorMovePreviousItem;
            this.bindingNavigatorActive.Name = "bindingNavigatorActive";
            this.bindingNavigatorActive.PositionItem = this.bindingNavigatorPositionItem;
            this.bindingNavigatorActive.Size = new System.Drawing.Size(596, 25);
            this.bindingNavigatorActive.TabIndex = 0;
            this.bindingNavigatorActive.Text = "bindingNavigator1";
            // 
            // bindingNavigatorCountItem
            // 
            this.bindingNavigatorCountItem.Name = "bindingNavigatorCountItem";
            this.bindingNavigatorCountItem.Size = new System.Drawing.Size(35, 22);
            this.bindingNavigatorCountItem.Text = "of {0}";
            this.bindingNavigatorCountItem.ToolTipText = "Total number of items";
            // 
            // bindingNavigatorMoveFirstItem
            // 
            this.bindingNavigatorMoveFirstItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorMoveFirstItem.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorMoveFirstItem.Image")));
            this.bindingNavigatorMoveFirstItem.Name = "bindingNavigatorMoveFirstItem";
            this.bindingNavigatorMoveFirstItem.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorMoveFirstItem.Size = new System.Drawing.Size(23, 22);
            this.bindingNavigatorMoveFirstItem.Text = "Move first";
            // 
            // bindingNavigatorMovePreviousItem
            // 
            this.bindingNavigatorMovePreviousItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorMovePreviousItem.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorMovePreviousItem.Image")));
            this.bindingNavigatorMovePreviousItem.Name = "bindingNavigatorMovePreviousItem";
            this.bindingNavigatorMovePreviousItem.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorMovePreviousItem.Size = new System.Drawing.Size(23, 22);
            this.bindingNavigatorMovePreviousItem.Text = "Move previous";
            // 
            // bindingNavigatorSeparator
            // 
            this.bindingNavigatorSeparator.Name = "bindingNavigatorSeparator";
            this.bindingNavigatorSeparator.Size = new System.Drawing.Size(6, 25);
            // 
            // bindingNavigatorPositionItem
            // 
            this.bindingNavigatorPositionItem.AccessibleName = "Position";
            this.bindingNavigatorPositionItem.AutoSize = false;
            this.bindingNavigatorPositionItem.Name = "bindingNavigatorPositionItem";
            this.bindingNavigatorPositionItem.Size = new System.Drawing.Size(50, 23);
            this.bindingNavigatorPositionItem.Text = "0";
            this.bindingNavigatorPositionItem.ToolTipText = "Current position";
            this.bindingNavigatorPositionItem.EnabledChanged += new System.EventHandler(this.bindingNavigatorPositionItem_EnabledChanged);
            this.bindingNavigatorPositionItem.TextChanged += new System.EventHandler(this.bindingNavigatorPositionItem_TextChanged);
            // 
            // bindingNavigatorSeparator1
            // 
            this.bindingNavigatorSeparator1.Name = "bindingNavigatorSeparator1";
            this.bindingNavigatorSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // bindingNavigatorMoveNextItem
            // 
            this.bindingNavigatorMoveNextItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorMoveNextItem.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorMoveNextItem.Image")));
            this.bindingNavigatorMoveNextItem.Name = "bindingNavigatorMoveNextItem";
            this.bindingNavigatorMoveNextItem.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorMoveNextItem.Size = new System.Drawing.Size(23, 22);
            this.bindingNavigatorMoveNextItem.Text = "Move next";
            // 
            // bindingNavigatorMoveLastItem
            // 
            this.bindingNavigatorMoveLastItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorMoveLastItem.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorMoveLastItem.Image")));
            this.bindingNavigatorMoveLastItem.Name = "bindingNavigatorMoveLastItem";
            this.bindingNavigatorMoveLastItem.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorMoveLastItem.Size = new System.Drawing.Size(23, 22);
            this.bindingNavigatorMoveLastItem.Text = "Move last";
            // 
            // bindingNavigatorSeparator2
            // 
            this.bindingNavigatorSeparator2.Name = "bindingNavigatorSeparator2";
            this.bindingNavigatorSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // btnRefreshActive
            // 
            this.btnRefreshActive.Image = global::EFEM.GUIControls.Properties.Resources.arrow_refresh;
            this.btnRefreshActive.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRefreshActive.Name = "btnRefreshActive";
            this.btnRefreshActive.Size = new System.Drawing.Size(66, 22);
            this.btnRefreshActive.Text = "Refresh";
            this.btnRefreshActive.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // bindingNavigatorDeleteItem
            // 
            this.bindingNavigatorDeleteItem.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorDeleteItem.Image")));
            this.bindingNavigatorDeleteItem.Name = "bindingNavigatorDeleteItem";
            this.bindingNavigatorDeleteItem.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorDeleteItem.Size = new System.Drawing.Size(89, 22);
            this.bindingNavigatorDeleteItem.Text = "Clear Alarm";
            this.bindingNavigatorDeleteItem.Click += new System.EventHandler(this.bindingNavigatorDeleteItem_Click);
            // 
            // tabHistory
            // 
            this.tabHistory.Controls.Add(this.dgHistory);
            this.tabHistory.Controls.Add(this.gFilter);
            this.tabHistory.Controls.Add(this.bindingNavigatorHistory);
            this.tabHistory.Location = new System.Drawing.Point(4, 22);
            this.tabHistory.Name = "tabHistory";
            this.tabHistory.Padding = new System.Windows.Forms.Padding(3);
            this.tabHistory.Size = new System.Drawing.Size(602, 554);
            this.tabHistory.TabIndex = 1;
            this.tabHistory.Text = "History";
            this.tabHistory.UseVisualStyleBackColor = true;
            // 
            // dgHistory
            // 
            this.dgHistory.AllowUserToAddRows = false;
            this.dgHistory.AllowUserToDeleteRows = false;
            this.dgHistory.AllowUserToOrderColumns = true;
            this.dgHistory.AllowUserToResizeRows = false;
            this.dgHistory.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgHistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgHistory.Location = new System.Drawing.Point(3, 176);
            this.dgHistory.Name = "dgHistory";
            this.dgHistory.ReadOnly = true;
            this.dgHistory.RowHeadersVisible = false;
            this.dgHistory.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgHistory.Size = new System.Drawing.Size(596, 375);
            this.dgHistory.TabIndex = 2;
            // 
            // gFilter
            // 
            this.gFilter.Controls.Add(this.alarmHistoryFilterCtrl1);
            this.gFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this.gFilter.Location = new System.Drawing.Point(3, 28);
            this.gFilter.Name = "gFilter";
            this.gFilter.Size = new System.Drawing.Size(596, 148);
            this.gFilter.TabIndex = 1;
            this.gFilter.TabStop = false;
            this.gFilter.Text = "Filter Settings";
            this.gFilter.Visible = false;
            // 
            // alarmHistoryFilterCtrl1
            // 
            this.alarmHistoryFilterCtrl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.alarmHistoryFilterCtrl1.EnableFilter = false;
            this.alarmHistoryFilterCtrl1.Location = new System.Drawing.Point(3, 16);
            this.alarmHistoryFilterCtrl1.Name = "alarmHistoryFilterCtrl1";
            this.alarmHistoryFilterCtrl1.RDMode = false;
            this.alarmHistoryFilterCtrl1.Size = new System.Drawing.Size(590, 129);
            this.alarmHistoryFilterCtrl1.Source = null;
            this.alarmHistoryFilterCtrl1.TabIndex = 0;
            // 
            // bindingNavigatorHistory
            // 
            this.bindingNavigatorHistory.AddNewItem = null;
            this.bindingNavigatorHistory.CountItem = this.bindingNavigatorCountItem1;
            this.bindingNavigatorHistory.DeleteItem = null;
            this.bindingNavigatorHistory.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bindingNavigatorMoveFirstItem1,
            this.bindingNavigatorMovePreviousItem1,
            this.bindingNavigatorSeparator3,
            this.bindingNavigatorPositionItem1,
            this.bindingNavigatorCountItem1,
            this.bindingNavigatorSeparator4,
            this.bindingNavigatorMoveNextItem1,
            this.bindingNavigatorMoveLastItem1,
            this.bindingNavigatorSeparator5,
            this.btnRefreshHistory,
            this.btnEnableFilter});
            this.bindingNavigatorHistory.Location = new System.Drawing.Point(3, 3);
            this.bindingNavigatorHistory.MoveFirstItem = this.bindingNavigatorMoveFirstItem1;
            this.bindingNavigatorHistory.MoveLastItem = this.bindingNavigatorMoveLastItem1;
            this.bindingNavigatorHistory.MoveNextItem = this.bindingNavigatorMoveNextItem1;
            this.bindingNavigatorHistory.MovePreviousItem = this.bindingNavigatorMovePreviousItem1;
            this.bindingNavigatorHistory.Name = "bindingNavigatorHistory";
            this.bindingNavigatorHistory.PositionItem = this.bindingNavigatorPositionItem1;
            this.bindingNavigatorHistory.Size = new System.Drawing.Size(596, 25);
            this.bindingNavigatorHistory.TabIndex = 0;
            this.bindingNavigatorHistory.Text = "bindingNavigator2";
            // 
            // bindingNavigatorCountItem1
            // 
            this.bindingNavigatorCountItem1.Name = "bindingNavigatorCountItem1";
            this.bindingNavigatorCountItem1.Size = new System.Drawing.Size(35, 22);
            this.bindingNavigatorCountItem1.Text = "of {0}";
            this.bindingNavigatorCountItem1.ToolTipText = "Total number of items";
            // 
            // bindingNavigatorMoveFirstItem1
            // 
            this.bindingNavigatorMoveFirstItem1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorMoveFirstItem1.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorMoveFirstItem1.Image")));
            this.bindingNavigatorMoveFirstItem1.Name = "bindingNavigatorMoveFirstItem1";
            this.bindingNavigatorMoveFirstItem1.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorMoveFirstItem1.Size = new System.Drawing.Size(23, 22);
            this.bindingNavigatorMoveFirstItem1.Text = "Move first";
            // 
            // bindingNavigatorMovePreviousItem1
            // 
            this.bindingNavigatorMovePreviousItem1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorMovePreviousItem1.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorMovePreviousItem1.Image")));
            this.bindingNavigatorMovePreviousItem1.Name = "bindingNavigatorMovePreviousItem1";
            this.bindingNavigatorMovePreviousItem1.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorMovePreviousItem1.Size = new System.Drawing.Size(23, 22);
            this.bindingNavigatorMovePreviousItem1.Text = "Move previous";
            // 
            // bindingNavigatorSeparator3
            // 
            this.bindingNavigatorSeparator3.Name = "bindingNavigatorSeparator3";
            this.bindingNavigatorSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // bindingNavigatorPositionItem1
            // 
            this.bindingNavigatorPositionItem1.AccessibleName = "Position";
            this.bindingNavigatorPositionItem1.AutoSize = false;
            this.bindingNavigatorPositionItem1.Name = "bindingNavigatorPositionItem1";
            this.bindingNavigatorPositionItem1.Size = new System.Drawing.Size(50, 23);
            this.bindingNavigatorPositionItem1.Text = "0";
            this.bindingNavigatorPositionItem1.ToolTipText = "Current position";
            // 
            // bindingNavigatorSeparator4
            // 
            this.bindingNavigatorSeparator4.Name = "bindingNavigatorSeparator4";
            this.bindingNavigatorSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // bindingNavigatorMoveNextItem1
            // 
            this.bindingNavigatorMoveNextItem1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorMoveNextItem1.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorMoveNextItem1.Image")));
            this.bindingNavigatorMoveNextItem1.Name = "bindingNavigatorMoveNextItem1";
            this.bindingNavigatorMoveNextItem1.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorMoveNextItem1.Size = new System.Drawing.Size(23, 22);
            this.bindingNavigatorMoveNextItem1.Text = "Move next";
            // 
            // bindingNavigatorMoveLastItem1
            // 
            this.bindingNavigatorMoveLastItem1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorMoveLastItem1.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorMoveLastItem1.Image")));
            this.bindingNavigatorMoveLastItem1.Name = "bindingNavigatorMoveLastItem1";
            this.bindingNavigatorMoveLastItem1.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorMoveLastItem1.Size = new System.Drawing.Size(23, 22);
            this.bindingNavigatorMoveLastItem1.Text = "Move last";
            // 
            // bindingNavigatorSeparator5
            // 
            this.bindingNavigatorSeparator5.Name = "bindingNavigatorSeparator5";
            this.bindingNavigatorSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // btnRefreshHistory
            // 
            this.btnRefreshHistory.Image = global::EFEM.GUIControls.Properties.Resources.arrow_refresh;
            this.btnRefreshHistory.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRefreshHistory.Name = "btnRefreshHistory";
            this.btnRefreshHistory.Size = new System.Drawing.Size(66, 22);
            this.btnRefreshHistory.Text = "Refresh";
            this.btnRefreshHistory.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // btnEnableFilter
            // 
            this.btnEnableFilter.CheckOnClick = true;
            this.btnEnableFilter.Enabled = false;
            this.btnEnableFilter.Image = global::EFEM.GUIControls.Properties.Resources.filter_disable;
            this.btnEnableFilter.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnEnableFilter.Name = "btnEnableFilter";
            this.btnEnableFilter.Size = new System.Drawing.Size(109, 22);
            this.btnEnableFilter.Text = "Filter - Disabled";
            this.btnEnableFilter.Click += new System.EventHandler(this.btnEnableFilter_Click);
            // 
            // EFEMAlarm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tcAlarm);
            this.Name = "EFEMAlarm";
            this.Size = new System.Drawing.Size(610, 580);
            this.tcAlarm.ResumeLayout(false);
            this.tabActive.ResumeLayout(false);
            this.tabActive.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgAlarm)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingNavigatorActive)).EndInit();
            this.bindingNavigatorActive.ResumeLayout(false);
            this.bindingNavigatorActive.PerformLayout();
            this.tabHistory.ResumeLayout(false);
            this.tabHistory.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgHistory)).EndInit();
            this.gFilter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.bindingNavigatorHistory)).EndInit();
            this.bindingNavigatorHistory.ResumeLayout(false);
            this.bindingNavigatorHistory.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceActive)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceHistory)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tcAlarm;
        private System.Windows.Forms.TabPage tabActive;
        private System.Windows.Forms.DataGridView dgAlarm;
        private System.Windows.Forms.TabPage tabHistory;
        private System.Windows.Forms.DataGridView dgHistory;
        private System.Windows.Forms.BindingNavigator bindingNavigatorActive;
        private System.Windows.Forms.ToolStripLabel bindingNavigatorCountItem;
        private System.Windows.Forms.ToolStripButton bindingNavigatorDeleteItem;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMoveFirstItem;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMovePreviousItem;
        private System.Windows.Forms.ToolStripSeparator bindingNavigatorSeparator;
        private System.Windows.Forms.ToolStripTextBox bindingNavigatorPositionItem;
        private System.Windows.Forms.ToolStripSeparator bindingNavigatorSeparator1;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMoveNextItem;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMoveLastItem;
        private System.Windows.Forms.ToolStripSeparator bindingNavigatorSeparator2;
        private System.Windows.Forms.BindingNavigator bindingNavigatorHistory;
        private System.Windows.Forms.ToolStripLabel bindingNavigatorCountItem1;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMoveFirstItem1;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMovePreviousItem1;
        private System.Windows.Forms.ToolStripSeparator bindingNavigatorSeparator3;
        private System.Windows.Forms.ToolStripTextBox bindingNavigatorPositionItem1;
        private System.Windows.Forms.ToolStripSeparator bindingNavigatorSeparator4;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMoveNextItem1;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMoveLastItem1;
        private System.Windows.Forms.ToolStripSeparator bindingNavigatorSeparator5;
        private System.Windows.Forms.BindingSource bindingSourceActive;
        private System.Windows.Forms.BindingSource bindingSourceHistory;
        private System.Windows.Forms.ToolStripButton btnRefreshActive;
        private System.Windows.Forms.ToolStripButton btnRefreshHistory;
        private System.Windows.Forms.GroupBox gFilter;
        private AlarmHistoryFilterCtrl alarmHistoryFilterCtrl1;
        private System.Windows.Forms.ToolStripButton btnEnableFilter;

    }
}
