namespace Communication.GUI
{
    partial class TestCommunicationGui
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SendCommandBtn = new System.Windows.Forms.Button();
            this.CommandText = new System.Windows.Forms.TextBox();
            this.MessageTB = new System.Windows.Forms.RichTextBox();
            this.Rs232CB = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 125F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 125F));
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.SendCommandBtn, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.CommandText, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.MessageTB, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.Rs232CB, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(496, 280);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(4, 46);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(117, 46);
            this.label2.TabIndex = 3;
            this.label2.Text = "RS232";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(4, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(117, 46);
            this.label1.TabIndex = 0;
            this.label1.Text = "Loadport Command";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SendCommandBtn
            // 
            this.SendCommandBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SendCommandBtn.ForeColor = System.Drawing.Color.Red;
            this.SendCommandBtn.Location = new System.Drawing.Point(375, 5);
            this.SendCommandBtn.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.SendCommandBtn.Name = "SendCommandBtn";
            this.SendCommandBtn.Size = new System.Drawing.Size(117, 36);
            this.SendCommandBtn.TabIndex = 1;
            this.SendCommandBtn.Text = "Disconnected";
            this.SendCommandBtn.UseVisualStyleBackColor = true;
            this.SendCommandBtn.Click += new System.EventHandler(this.SendCommandBtn_Click);
            // 
            // CommandText
            // 
            this.CommandText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CommandText.Location = new System.Drawing.Point(129, 5);
            this.CommandText.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.CommandText.Name = "CommandText";
            this.CommandText.Size = new System.Drawing.Size(238, 26);
            this.CommandText.TabIndex = 2;
            // 
            // MessageTB
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.MessageTB, 3);
            this.MessageTB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MessageTB.Location = new System.Drawing.Point(5, 97);
            this.MessageTB.Margin = new System.Windows.Forms.Padding(5);
            this.MessageTB.Name = "MessageTB";
            this.MessageTB.Size = new System.Drawing.Size(486, 178);
            this.MessageTB.TabIndex = 5;
            this.MessageTB.Text = "";
            // 
            // Rs232CB
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.Rs232CB, 2);
            this.Rs232CB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Rs232CB.FormattingEnabled = true;
            this.Rs232CB.Location = new System.Drawing.Point(135, 56);
            this.Rs232CB.Margin = new System.Windows.Forms.Padding(10);
            this.Rs232CB.Name = "Rs232CB";
            this.Rs232CB.Size = new System.Drawing.Size(351, 28);
            this.Rs232CB.TabIndex = 6;
            this.Rs232CB.SelectedIndexChanged += new System.EventHandler(this.Rs232CB_SelectedIndexChanged);
            // 
            // TestCommunicationGui
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "TestCommunicationGui";
            this.Size = new System.Drawing.Size(496, 280);
            this.Load += new System.EventHandler(this.TestCommunicationGui_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button SendCommandBtn;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox CommandText;
        private System.Windows.Forms.RichTextBox MessageTB;
        private System.Windows.Forms.ComboBox Rs232CB;
    }
}
