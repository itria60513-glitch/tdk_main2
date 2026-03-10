using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EFEMInterface;

namespace EFEM.GUIControls.TeachControls
{
    public partial class TeachApproachAngleCtrl : UserControl
    {

        #region Fields and Properties

        private ToolTip ttInfo = new ToolTip();

        private EFEMStation _station;
        public EFEMStation CurrentStation
        {
            get { return _station; }
            set 
            { 
                _station = value;
                lTaught.Visible = tTaught.Visible = (_station == EFEMStation.NONE) ? false : true;
            }
        }

        public string Caption
        {
            get { return gTargetCaption.Text; }
            set { gTargetCaption.Text = value; }
        }

        public Image BackgroundImage
        {
            get { return pIllustration.BackgroundImage; }
            set { pIllustration.BackgroundImage = value; }
        }

        private Decimal _taughtangle;
        public Decimal TaughtAngle
        {
            get
            {
                _taughtangle = Convert.ToDecimal(tTaught.Text);
                return _taughtangle;
            }
            set
            {
                _taughtangle = value;
                tTaught.Text = _taughtangle.ToString();
            }
        }

        private Decimal _currentangle;
        public Decimal CurrentAngle
        {
            get
            {
                _currentangle = Convert.ToDecimal(lnbCurrentAngle.Value.ToString());
                return _currentangle;

            }
            set
            {
                _currentangle = value;
                lnbCurrentAngle.Value = _currentangle;
            }
        }

        #endregion

        #region Contructor

        public TeachApproachAngleCtrl()
        {
            InitializeComponent();

            //Attach tooltip
            ttInfo.ToolTipTitle = "Unit[degree]";
             ttInfo.SetToolTip(this.btnInfo, "-180.00 to 180.00");
        }

        #endregion

        #region Methods
        
        #endregion

        #region Events
        
        #endregion


    }
}
