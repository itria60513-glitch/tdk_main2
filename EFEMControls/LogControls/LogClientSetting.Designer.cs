namespace EFEM.GUIControls
{
    partial class LogClientSetting
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
            this.gbLogList = new System.Windows.Forms.GroupBox();
            this.dgLogList = new System.Windows.Forms.DataGridView();
            this.lbShowMsg = new System.Windows.Forms.Label();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnReload = new System.Windows.Forms.Button();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gbLogList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgLogList)).BeginInit();
            this.SuspendLayout();
            // 
            // gbLogList
            // 
            this.gbLogList.Controls.Add(this.dgLogList);
            this.gbLogList.Controls.Add(this.lbShowMsg);
            this.gbLogList.Controls.Add(this.btnApply);
            this.gbLogList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbLogList.Location = new System.Drawing.Point(0, 0);
            this.gbLogList.Name = "gbLogList";
            this.gbLogList.Size = new System.Drawing.Size(351, 411);
            this.gbLogList.TabIndex = 0;
            this.gbLogList.TabStop = false;
            this.gbLogList.Text = "Log Object List";
            // 
            // dgLogList
            // 
            this.dgLogList.AllowUserToAddRows = false;
            this.dgLogList.AllowUserToDeleteRows = false;
            this.dgLogList.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgLogList.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgLogList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgLogList.Location = new System.Drawing.Point(18, 19);
            this.dgLogList.MultiSelect = false;
            this.dgLogList.Name = "dgLogList";
            this.dgLogList.RowHeadersVisible = false;
            this.dgLogList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgLogList.Size = new System.Drawing.Size(316, 328);
            this.dgLogList.TabIndex = 0;
            // 
            // lbShowMsg
            // 
            this.lbShowMsg.AutoSize = true;
            this.lbShowMsg.BackColor = System.Drawing.Color.LightPink;
            this.lbShowMsg.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbShowMsg.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbShowMsg.Location = new System.Drawing.Point(18, 352);
            this.lbShowMsg.Name = "lbShowMsg";
            this.lbShowMsg.Size = new System.Drawing.Size(70, 18);
            this.lbShowMsg.TabIndex = 1;
            this.lbShowMsg.Text = "Message:";
            this.lbShowMsg.Visible = false;
            // 
            // btnApply
            // 
            this.btnApply.Image = global::EFEM.GUIControls.Properties.Resources.apply;
            this.btnApply.Location = new System.Drawing.Point(224, 376);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(110, 29);
            this.btnApply.TabIndex = 2;
            this.btnApply.Text = "Apply Settings";
            this.btnApply.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // btnReload
            // 
            this.btnReload.Image = global::EFEM.GUIControls.Properties.Resources.arrow_refresh;
            this.btnReload.Location = new System.Drawing.Point(108, 376);
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(110, 29);
            this.btnReload.TabIndex = 1;
            this.btnReload.Text = "Reload Settings";
            this.btnReload.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnReload.Click += new System.EventHandler(this.btnReload_Click);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.FillWeight = 169.5432F;
            this.dataGridViewTextBoxColumn1.HeaderText = "File Name";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            this.dataGridViewTextBoxColumn1.Width = 228;
            // 
            // LogClientSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnReload);
            this.Controls.Add(this.gbLogList);
            this.Name = "LogClientSetting";
            this.Size = new System.Drawing.Size(351, 411);
            this.gbLogList.ResumeLayout(false);
            this.gbLogList.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgLogList)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbLogList;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Label lbShowMsg;
        private System.Windows.Forms.DataGridView dgLogList;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.Button btnReload;
    }
}
