namespace EFEM.GUIControls.TeachControls
{
    partial class TeachStationParametersCtrl
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
            this.gStationParameters = new System.Windows.Forms.GroupBox();
            this.teachZspeedCtrl = new EFEM.GUIControls.TeachControls.TeachZspeedCtrl();
            this.teachMotionConditionCtrl = new EFEM.GUIControls.TeachControls.TeachMotionConditionCtrl();
            this.teachSlotCtrl = new EFEM.GUIControls.TeachControls.TeachSlotCtrl();
            this.gStationParameters.SuspendLayout();
            this.SuspendLayout();
            // 
            // gStationParameters
            // 
            this.gStationParameters.Controls.Add(this.teachZspeedCtrl);
            this.gStationParameters.Controls.Add(this.teachMotionConditionCtrl);
            this.gStationParameters.Controls.Add(this.teachSlotCtrl);
            this.gStationParameters.Location = new System.Drawing.Point(3, 3);
            this.gStationParameters.Name = "gStationParameters";
            this.gStationParameters.Size = new System.Drawing.Size(517, 400);
            this.gStationParameters.TabIndex = 15;
            this.gStationParameters.TabStop = false;
            this.gStationParameters.Text = "Station Parameters";
            // 
            // teachZspeedCtrl
            // 
            this.teachZspeedCtrl.CurrentStation = EFEMInterface.EFEMStation.NONE;
            this.teachZspeedCtrl.Location = new System.Drawing.Point(8, 14);
            this.teachZspeedCtrl.Name = "teachZspeedCtrl";
            this.teachZspeedCtrl.Size = new System.Drawing.Size(225, 101);
            this.teachZspeedCtrl.TabIndex = 21;
            // 
            // teachMotionConditionCtrl
            // 
            this.teachMotionConditionCtrl.CurrentStation = EFEMInterface.EFEMStation.NONE;
            this.teachMotionConditionCtrl.Location = new System.Drawing.Point(4, 112);
            this.teachMotionConditionCtrl.Name = "teachMotionConditionCtrl";
            this.teachMotionConditionCtrl.Size = new System.Drawing.Size(502, 283);
            this.teachMotionConditionCtrl.TabIndex = 20;
            // 
            // teachSlotCtrl
            // 
            this.teachSlotCtrl.CurrentStation = EFEMInterface.EFEMStation.NONE;
            this.teachSlotCtrl.Location = new System.Drawing.Point(234, 10);
            this.teachSlotCtrl.Name = "teachSlotCtrl";
            this.teachSlotCtrl.Size = new System.Drawing.Size(269, 102);
            this.teachSlotCtrl.TabIndex = 0;
            // 
            // TeachStationParametersCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gStationParameters);
            this.Name = "TeachStationParametersCtrl";
            this.Size = new System.Drawing.Size(525, 404);
            this.gStationParameters.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gStationParameters;
        private TeachSlotCtrl teachSlotCtrl;
        private TeachMotionConditionCtrl teachMotionConditionCtrl;
        private TeachZspeedCtrl teachZspeedCtrl;
    }
}
