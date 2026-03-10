using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EFEMInterface;
using EFEM.ExceptionManagements;
using System.Threading;

namespace EFEM.GUIControls.TeachControls
{
    public partial class TeachStationParametersCtrl : UserControl
    {
        #region Fields and Properties

        private EFEMStation _station;
        public EFEMStation CurrentStation
        {
            get { return _station; }
            set 
            {
                _station = value;
                teachMotionConditionCtrl.CurrentStation = value;
                teachSlotCtrl.CurrentStation = value;
                teachZspeedCtrl.CurrentStation = value;

                teachZspeedCtrl.Enabled = teachMotionConditionCtrl.Enabled = teachSlotCtrl.Enabled = (_station == EFEMStation.NONE) ? false : true;
            }
        }       
 
        #endregion

        #region Constructor

        public TeachStationParametersCtrl()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods
        


        #endregion

        #region Events
        
        #endregion


    }
}
