namespace EFEM.GUIControls
{
    partial class InitStatusErrorListForm
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.lCaption = new System.Windows.Forms.Label();
            this.dataGridViewError = new System.Windows.Forms.DataGridView();
            this.ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ErrorMsg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewError)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lCaption);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(585, 37);
            this.panel1.TabIndex = 3;
            // 
            // lCaption
            // 
            this.lCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lCaption.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lCaption.Location = new System.Drawing.Point(0, 0);
            this.lCaption.Name = "lCaption";
            this.lCaption.Size = new System.Drawing.Size(585, 37);
            this.lCaption.TabIndex = 0;
            this.lCaption.Text = "<None>";
            this.lCaption.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // dataGridViewError
            // 
            this.dataGridViewError.AccessibleRole = System.Windows.Forms.AccessibleRole.TitleBar;
            this.dataGridViewError.AllowUserToAddRows = false;
            this.dataGridViewError.AllowUserToDeleteRows = false;
            this.dataGridViewError.AllowUserToResizeRows = false;
            this.dataGridViewError.BackgroundColor = System.Drawing.Color.White;
            this.dataGridViewError.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewError.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ID,
            this.ErrorMsg});
            this.dataGridViewError.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewError.Location = new System.Drawing.Point(0, 37);
            this.dataGridViewError.Name = "dataGridViewError";
            this.dataGridViewError.RowHeadersVisible = false;
            this.dataGridViewError.ShowEditingIcon = false;
            this.dataGridViewError.Size = new System.Drawing.Size(585, 215);
            this.dataGridViewError.TabIndex = 1;
            // 
            // ID
            // 
            this.ID.Frozen = true;
            this.ID.HeaderText = "ID";
            this.ID.Name = "ID";
            this.ID.ReadOnly = true;
            this.ID.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ID.Width = 50;
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
            // InitStatusErrorListForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(585, 252);
            this.Controls.Add(this.dataGridViewError);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InitStatusErrorListForm";
            this.Text = "Error List";
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewError)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridView dataGridViewError;
        private System.Windows.Forms.Label lCaption;
        private System.Windows.Forms.DataGridViewTextBoxColumn ID;
        private System.Windows.Forms.DataGridViewTextBoxColumn ErrorMsg;
    }
}