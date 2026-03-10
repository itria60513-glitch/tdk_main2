namespace EFEM.GUIControls
{
    partial class RobotAlarmFormCtrl
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
            this.dgRobotAlarm = new System.Windows.Forms.DataGridView();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.dgRobotAlarm)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgRobotAlarm
            // 
            this.dgRobotAlarm.AllowUserToAddRows = false;
            this.dgRobotAlarm.AllowUserToDeleteRows = false;
            this.dgRobotAlarm.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgRobotAlarm.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgRobotAlarm.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgRobotAlarm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgRobotAlarm.Location = new System.Drawing.Point(0, 0);
            this.dgRobotAlarm.MultiSelect = false;
            this.dgRobotAlarm.Name = "dgRobotAlarm";
            this.dgRobotAlarm.ReadOnly = true;
            this.dgRobotAlarm.RowHeadersVisible = false;
            this.dgRobotAlarm.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.dgRobotAlarm.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgRobotAlarm.Size = new System.Drawing.Size(603, 539);
            this.dgRobotAlarm.TabIndex = 0;
            // 
            // btnUpdate
            // 
            this.btnUpdate.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnUpdate.Image = global::EFEM.GUIControls.Properties.Resources.arrow_refresh;
            this.btnUpdate.Location = new System.Drawing.Point(0, 0);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(88, 41);
            this.btnUpdate.TabIndex = 0;
            this.btnUpdate.Text = "Refresh";
            this.btnUpdate.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnUpdate);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 539);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(603, 41);
            this.panel1.TabIndex = 1;
            // 
            // RobotAlarmFormCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dgRobotAlarm);
            this.Controls.Add(this.panel1);
            this.Name = "RobotAlarmFormCtrl";
            this.Size = new System.Drawing.Size(603, 580);
            ((System.ComponentModel.ISupportInitialize)(this.dgRobotAlarm)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgRobotAlarm;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Panel panel1;
    }
}
