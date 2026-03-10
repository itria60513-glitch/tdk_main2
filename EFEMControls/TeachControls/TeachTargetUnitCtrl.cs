using System;
using System.Collections;
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
    public partial class TeachTargetUnitCtrl : UserControl
    {

        #region Defines

        public enum UnitMode
        { 
            Coordinate = 0,     //Unit [mm], LX0 and LY0
            Joint = 1               //Unit [degree], J1~J7
        }

        public enum TargetItem
        { 
            //Joint Code
            J1 = 0,      //Joint1 (Rotating part of aligner)
            J2 = 1,      //Joint2 (Lower Link axis)
            J3 = 2,      //Joint3 (Z axis)
            J4 = 3,      //Joint4 (Upper Link axis)
            J6 = 4,      //Joint6 (H1: Lower Hand axis)
            J7 = 5,      //Joint7 (H2: Upper Hand axis)

            //Coordinate Code
            LX0 = 6,    // Base coordinate X axis
            LY0 = 7,    // Base coordinate Y axis
            //LX1 = 8,    // Hand 1 Tool coordinate X axis
            //LY1 = 9,    // Hand 1 Tool coordinate Y axis
            //LX2 = 10,    // Hand 2 Tool coordinate X axis
            //LY2 = 11,    // Hand 2 Tool coordinate Y axis
            //LR1 = 12,    // Hand1 Rotation
            //LR2 = 13,    // Hand 2 Rotation
        }

        #endregion

        #region Fields and Properties

        private UnitMode _mode;
        private TargetItem _target;
        private ToolTip ttInfo = new ToolTip();
        private string _ProgressKey = string.Empty;

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

        [Category("Mode")]
        public UnitMode Mode
        {
            get { return _mode; }
            set 
            {
                _mode = value;
                string sInfo;
                switch (value)
                {
                    case UnitMode.Coordinate:
                        {
                            sInfo = "-9999.99 to 9999.99"; 
                            ttInfo.ToolTipTitle = "Unit[mm]";
                            break;
                        }
                    case UnitMode.Joint:
                        {
                            sInfo = "-180.00 to 180.00"; 
                            btnPos.Text = "CCW"; btnNeg.Text = "CW"; 

                            ttInfo.ToolTipTitle = "Unit[degree]";
                            break;
                        }
                    default:
                        {
                            ttInfo.ToolTipTitle = "Undefined mode";
                            sInfo = "N/A range";
                            break;
                        }
                }
                lnbStep.Caption = ttInfo.ToolTipTitle;
                ttInfo.SetToolTip(this.btnInfo, sInfo);
            }
        }

        [Category("Target")]
        public TargetItem Target
        {
            get { return _target; }
            set
            { 
                _target = value;

                if (_target == TargetItem.LX0) { btnPos.Text = "Right"; btnNeg.Text = "Left"; }
                if (_target == TargetItem.LY0) { btnPos.Text = "Forward"; btnNeg.Text = "Backward"; }
                if (_target == TargetItem.J3) { btnPos.Text = "Up"; btnNeg.Text = "Down"; }
            }
        }


        private double _taughtpos;
        public double TaughtPosition
        {
           get 
           {
               _taughtpos = Convert.ToDouble(tTaught.Text);
               return _taughtpos;
           }
           set 
            {
                _taughtpos = value;
                tTaught.Text = _taughtpos.ToString(); 
            }
        }

        private double _currentpos;
        public double CurrentPosition
        {
           get 
           {
              _currentpos = Convert.ToDouble(tCurrent.Text);
               return _currentpos;
               
           }
           set 
            {
                _currentpos = value;
                tCurrent.Text = _currentpos.ToString(); 
            }
        }

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

        #endregion

        #region Constructor

        public TeachTargetUnitCtrl()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        private void TPool_TeachRobot(object parameters)
        {
            try
            {
                ArrayList list = (ArrayList)parameters;
                UnitMode mde = (UnitMode)list[0];
                TargetItem tgt = (TargetItem)list[1];
                double sign = ((bool)list[2]) ? 1 : -1;
                double step = (double)lnbStep.Value * sign;
                HRESULT hr = null;


                if (mde == UnitMode.Joint) //Joint coordinate
                {
                    if (step < -180.00 || step >= 180.00)
                        return;

                    switch (tgt)
                    {
                        case TargetItem.J2:
                            hr = GUIBasic.Instance().MachineControl.MoveJoint(EFEMRobotJoint.LowerLink, false, step);
                            break;
                        case TargetItem.J3:
                            hr = GUIBasic.Instance().MachineControl.MoveJoint(EFEMRobotJoint.Z, false, step);
                            break;
                        case TargetItem.J4:
                            hr = GUIBasic.Instance().MachineControl.MoveJoint(EFEMRobotJoint.UpperLink, false, step);
                            break;
                        case TargetItem.J6:
                            hr = GUIBasic.Instance().MachineControl.MoveJoint(EFEMRobotJoint.LowerHand, false, step);
                            break;
                        case TargetItem.J7:
                            hr = GUIBasic.Instance().MachineControl.MoveJoint(EFEMRobotJoint.UpperHand, false, step);
                            break;
                        default:
                            break;
                    }

                }
                else // XY coordinate
                {
                    if (step <= -9999.99 || step >= 9999.99)
                        return;

                    if (tgt == TargetItem.LX0)
                        hr = GUIBasic.Instance().MachineControl.MoveXY(EFEMBaseCoorinate.X, false, step);
                    else
                        hr = GUIBasic.Instance().MachineControl.MoveXY(EFEMBaseCoorinate.Y, false, step);
                }

                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to perform Robot Action.\n  -->Reason: " + hr._message);
                    return;
                }


                //Update GUI values
                hr = UpdateCurrentPos(_mode, _target);
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to update GUI values.\n  -->Reason: " + hr._message);
                }

            }
            catch (Exception e)
            {
                GUIBasic.Instance().ProgressException("Exception occured during teaching.\n  -->Reason: " + e.Message);
            }
            finally
            {
                GUIBasic.Instance().ProgressAbortEvent -= new GUIBasic.ProgressAbort(TeachTargetUnitCtrl_ProgressAbortEvent);
                GUIBasic.Instance().ProgressEnd(_ProgressKey);
            }
        }

        private HRESULT UpdateCurrentPos(UnitMode mde, TargetItem tgt)
        {
            HRESULT hr = null;         
            TeachPosition tp = new TeachPosition();
            string value = string.Empty;

            hr = tp.GetCurrentRobotXYZPos();
            hr = tp.GetCurrentRobotPos();

            if (hr != null)
                value = "N/A";
            else
            {
                if (mde == UnitMode.Joint)
                {
                    switch (tgt)
                    {
                        case TargetItem.J2:
                            value = tp.current_J2.ToString();
                            break;
                        case TargetItem.J3:
                            value = tp.current_J3.ToString();
                            break;
                        case TargetItem.J4:
                            value = tp.current_J4.ToString();
                            break;
                        case TargetItem.J6:
                            value = tp.current_J6.ToString();
                            break;
                        case TargetItem.J7:
                            value = tp.current_J7.ToString();
                            break;
                        default:
                            break;
                    }
                }
                else //  XY coordinate
                {
                    if (tgt == TargetItem.LX0)
                        value = tp.current_X.ToString();
                    else
                        value = tp.current_Y.ToString();
                }
            }

            this.Invoke(new MethodInvoker(delegate { tCurrent.Text = value; })); 
            return hr;
        }

        #endregion

        #region Events

        private void btnPos_Click(object sender, EventArgs e)
        {

            GUIBasic.Instance().ProgressAbortEvent += new GUIBasic.ProgressAbort(TeachTargetUnitCtrl_ProgressAbortEvent);
            _ProgressKey = GUIBasic.Instance().ProgressStart("Robot Action", string.Format("{0} is moving...", _target.ToString()), 0, true, false);
            ArrayList list = new ArrayList();
            list.Add(_mode);
            list.Add(_target);
            list.Add(true);
            ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_TeachRobot), list);
        }

        private void btnNeg_Click(object sender, EventArgs e)
        {
            GUIBasic.Instance().ProgressAbortEvent += new GUIBasic.ProgressAbort(TeachTargetUnitCtrl_ProgressAbortEvent);
            _ProgressKey = GUIBasic.Instance().ProgressStart("Robot Action", string.Format("{0} is moving...", _target.ToString()), 0, true, false);
            ArrayList list = new ArrayList();
            list.Add(_mode);
            list.Add(_target);
            list.Add(false);
            ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_TeachRobot), list);
        }

        private void TeachTargetUnitCtrl_ProgressAbortEvent()
        {
            GUIBasic.Instance().ProgressUpdate(">>>>Aborting command...");
            HRESULT hr = GUIBasic.Instance().MachineControl.AbortAction(EFEMInterface.EFEMDeviceCode.All, true);
            if (hr == null)
                GUIBasic.Instance().ProgressUpdate(">>>>Command aborted successfully.");
            else
                GUIBasic.Instance().ProgressUpdate(">>>>Fail to abort command.\n  -->Reason: " + hr._message);
        }

        #endregion

    }
}
