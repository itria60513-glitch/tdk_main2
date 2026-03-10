using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EFEMInterface;

namespace EFEM.GUIControls
{
    public partial class TeachMap : UserControl
    {
        #region Defines

        //public enum Destination
        //{ 
        //    LoadPort1 = 0,  //P1,P2
        //    LoadPort2 = 1,  //P3,P4
        //    LoadPort3 = 2,  //P5,P6
        //    Aligner = 3,       //A1
        //    LoadLock = 4,   //P13,P14
        //}

        #endregion

        #region Fields and Properties

        private EFEMStation _station;

        public EFEMStation CurrentStation
        {
            get { return _station; }
            set { _station = value;}
        }

        #endregion

        #region Constructor

        public TeachMap()
        {
            InitializeComponent();
            cmbDestination.SelectedIndex = 0;
        }
        
        #endregion

        #region Events

        private void cmbDestination_DrawItem(object sender, DrawItemEventArgs e)
        {
              // By using Sender, one method could handle multiple ComboBoxes
              ComboBox cbx = sender as ComboBox;
              if (cbx != null)
              {
                // Always draw the background
                e.DrawBackground();

                // Drawing one of the items?
                if (e.Index >= 0)
                {
                  // Set the string alignment.  Choices are Center, Near and Far
                  StringFormat sf = new StringFormat();
                  sf.LineAlignment = StringAlignment.Center;
                  sf.Alignment = StringAlignment.Center;

                  // Set the Brush to ComboBox ForeColor to maintain any ComboBox color settings
                  // Assumes Brush is solid
                  Brush brush = new SolidBrush(cbx.ForeColor);

                  // If drawing highlighted selection, change brush
                  if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                    brush = SystemBrushes.HighlightText;

                  // Draw the string
                  e.Graphics.DrawString(cbx.Items[e.Index].ToString(), cbx.Font, brush, e.Bounds, sf);
                }
              }
        }

        public event EventHandler UpdateCurrentStation;

        private void cmbDestination_SelectedIndexChanged(object sender, EventArgs e)
        {
            pBackground.BackColor = pLP1.BackColor = pLP2.BackColor = pLP3.BackColor = pAligner.BackColor = pLL.BackColor = SystemColors.Control;

            if (cmbDestination.SelectedIndex == 0)
                _station = EFEMStation.NONE;    //Free Run Mode
            else
            _station = (EFEMStation)Enum.Parse(typeof(EFEMStation), cmbDestination.SelectedItem.ToString());

            switch (_station)
            {
                case EFEMStation.NONE:
                    pLP1.BackColor = System.Drawing.Color.Pink;
                    pLP2.BackColor = System.Drawing.Color.Pink;
                    pLP3.BackColor = System.Drawing.Color.Pink;
                    pAligner.BackColor = System.Drawing.Color.Pink;
                    pLL.BackColor = System.Drawing.Color.Pink;
                    pBackground.BackColor = System.Drawing.Color.Pink;
                    break;
                case EFEMStation.LP1:
                    pLP1.BackColor = System.Drawing.Color.Pink;
                    break;
                case EFEMStation.LP2:
                    pLP2.BackColor = System.Drawing.Color.Pink;
                    break;
                case EFEMStation.LP3:
                    pLP3.BackColor = System.Drawing.Color.Pink;
                    break;
                case EFEMStation.ALIGNER:
                    pAligner.BackColor = System.Drawing.Color.Pink;
                    break;
                case EFEMStation.LL2:
                    pLL.BackColor = System.Drawing.Color.Pink;
                    break;
                default:
                    break;
            }

            if (this.UpdateCurrentStation != null)
                this.UpdateCurrentStation(new object(), new EventArgs());
        }

        #endregion

    }
}
