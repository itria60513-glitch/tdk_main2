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

namespace EFEM.GUIControls
{
    public partial class TeachTargetCtrl : UserControl
    {

        #region Fields and Properties

        private List<TeachPosition> tplst = new List<TeachPosition>();

        private EFEMStation _station;
        private int _Index = 0;

        public EFEMStation CurrentStation
        {
            get { return _station; }
            set 
            {
                _station = value;
                int count = tplst.Count;

                for (int i = 0; i < count; i++)
                {
                    if (tplst[i]._station == _station)
                    {
                        _Index = i;
                        break;
                    }
                }

                //Free Run mode check
                btnApply.Enabled = btnReadTaughtPos.Enabled = (_station == EFEMStation.NONE) ? false : true;
                TargetJ2.CurrentStation = TargetJ4.CurrentStation = TargetJ6.CurrentStation = TargetJ7.CurrentStation = TargetO.CurrentStation = _station;
                TargetX.CurrentStation = TargetY.CurrentStation = TargetZ.CurrentStation = _station;
            }
        }

        private string _ProgressKey = string.Empty;

        #endregion

        #region Constructor

        public TeachTargetCtrl()
        {
            InitializeComponent();
            InitailTeachPos();
        }

        private void InitailTeachPos()
        {
            foreach (EFEMStation item in Enum.GetValues(typeof(EFEMStation)))
            {
                TeachPosition tptmp = new TeachPosition();
                tptmp._station =item;
                tptmp.GetCurrentRobotPos();
                tptmp.GetCurrentRobotXYZPos();
                tptmp.GetTaughtJointPos();
                tptmp.GetTaughtXYZPos();
                tplst.Add(tptmp);
            }
        }

        #endregion

        #region Methods

        private void TPool_Apply(object data)
        {
            try
            {
                HRESULT hr = null;

                // Set Taught Position
                hr = tplst[_Index].SetTaughtPos();
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to apply taught positions in Joint coordinate.\n  -->Reason: " + hr._message);
                    return;
                }

                hr = tplst[_Index].SetTaughtXYZPos();
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to apply taught positions in XYZ coordinate.\n  -->Reason: " + hr._message);
                }

                //Get Taught Position
                TPool_ReadTaughtPos(data);

            }
            catch (Exception e)
            {
                GUIBasic.Instance().ProgressException("Exception occured during applying taught position.\n  -->Reason: " + e.Message);
            }
            finally
            {
                GUIBasic.Instance().ProgressAbortEvent -= new GUIBasic.ProgressAbort(TeachTargetCtrl_ProgressAbortEvent);
                GUIBasic.Instance().ProgressEnd(_ProgressKey);
            }
        }

        private void TPool_ReadTaughtPos(object data)
        {
            try
            {
                HRESULT hr = null;
                bool IsTaught = false;

                //Get Taught Position
                hr = tplst[_Index].GetTaughtXYZPos();
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to get taught positions in XYZ coordinate.\n  -->Reason: " + hr._message);
                    IsTaught = false;
                    return;
                }
                IsTaught = tplst[_Index]._IsTaught;

                hr = tplst[_Index].GetTaughtJointPos();
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to get taught positions in Joint coordinate.\n  -->Reason: " + hr._message);
                    IsTaught = false;
                }
                IsTaught &= tplst[_Index]._IsTaught;
                if (!IsTaught) { MessageBox.Show("Robot is not taught!"); return; }

                //Update GUI
                UpdateGUITaughtPos();
            }
            catch (Exception e)
            {
                GUIBasic.Instance().ProgressException("Exception occured during updating taught position.\n  -->Reason: " + e.Message);
            }
            finally
            {
                GUIBasic.Instance().ProgressAbortEvent -= new GUIBasic.ProgressAbort(TeachTargetCtrl_ProgressAbortEvent);
                GUIBasic.Instance().ProgressEnd(_ProgressKey);            
            }
        }

        private void TPool_ReadCurrentPos(object data)
        {
            try
            {
                HRESULT hr = null;

                hr = tplst[_Index].GetCurrentRobotXYZPos();
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to get current positions in XYZ coordinate.\n  -->Reason: " + hr._message);
                    return;
                }

                hr = tplst[_Index].GetCurrentRobotPos();
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to get current positions in Joint coordinate.\n  -->Reason: " + hr._message);
                }

                //Update GUI
                UpdateGUCurrentPos();
            }
            catch (Exception e)
            {
                GUIBasic.Instance().ProgressException("Exception occured during updating current position.\n  -->Reason:  " + e.Message);
            }
            finally
            {
                GUIBasic.Instance().ProgressAbortEvent -= new GUIBasic.ProgressAbort(TeachTargetCtrl_ProgressAbortEvent);
                GUIBasic.Instance().ProgressEnd(_ProgressKey); 
            }
        }

        private void UpdateGUITaughtPos()
        {
            this.Invoke(new MethodInvoker(delegate { TargetX.TaughtPosition = tplst[_Index].taught_X; }));
            this.Invoke(new MethodInvoker(delegate { TargetY.TaughtPosition = tplst[_Index].taught_Y; }));
            this.Invoke(new MethodInvoker(delegate { TargetZ.TaughtPosition = tplst[_Index].taught_Z;  }));
            this.Invoke(new MethodInvoker(delegate { TargetJ2.TaughtPosition = tplst[_Index].taught_J2;  }));
            this.Invoke(new MethodInvoker(delegate { TargetJ4.TaughtPosition = tplst[_Index].taught_J4;  }));
            this.Invoke(new MethodInvoker(delegate { TargetJ6.TaughtPosition = tplst[_Index].taught_J6; }));
            this.Invoke(new MethodInvoker(delegate { TargetJ7.TaughtPosition = tplst[_Index].taught_J7; }));
            this.Invoke(new MethodInvoker(delegate { TargetO.TaughtAngle = Convert.ToDecimal(tplst[_Index].taught_O); }));
        }

        private void UpdateGUCurrentPos()
        {
            this.Invoke(new MethodInvoker(delegate { TargetX.CurrentPosition = tplst[_Index].current_X; }));
            this.Invoke(new MethodInvoker(delegate { TargetY.CurrentPosition = tplst[_Index].current_Y; }));
            this.Invoke(new MethodInvoker(delegate { TargetZ.CurrentPosition = tplst[_Index].current_Z; }));
            this.Invoke(new MethodInvoker(delegate { TargetJ2.CurrentPosition = tplst[_Index].current_J2; }));
            this.Invoke(new MethodInvoker(delegate { TargetJ4.CurrentPosition = tplst[_Index].current_J4; }));
            this.Invoke(new MethodInvoker(delegate { TargetJ6.CurrentPosition = tplst[_Index].current_J6; }));
            this.Invoke(new MethodInvoker(delegate { TargetJ7.CurrentPosition = tplst[_Index].current_J7; }));
            this.Invoke(new MethodInvoker(delegate { TargetO.CurrentAngle = Convert.ToDecimal(tplst[_Index].current_O); }));
        }

        #endregion

        #region Events

        private void btnApply_Click(object sender, EventArgs e)
        {
            GUIBasic.Instance().ProgressAbortEvent += new GUIBasic.ProgressAbort(TeachTargetCtrl_ProgressAbortEvent);
            _ProgressKey = GUIBasic.Instance().ProgressStart("To Apply current positions being new taught positions to robot controller", string.Format("Controller is writing..."), 0, true, false);
            ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_Apply),null);
        }

        private void btnReadTaughtPos_Click(object sender, EventArgs e)
        {
            GUIBasic.Instance().ProgressAbortEvent += new GUIBasic.ProgressAbort(TeachTargetCtrl_ProgressAbortEvent);
            _ProgressKey = GUIBasic.Instance().ProgressStart("To get taught positions", string.Format("Reading from controller..."), 0, true, false);
            ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_ReadTaughtPos), null);
        }

        private void btnReadCurrentPos_Click(object sender, EventArgs e)
        {
            GUIBasic.Instance().ProgressAbortEvent += new GUIBasic.ProgressAbort(TeachTargetCtrl_ProgressAbortEvent);
            _ProgressKey = GUIBasic.Instance().ProgressStart("To get current positions", string.Format("Reading from controller..."), 0, true, false);
            ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_ReadCurrentPos), null);
        }

        private void TeachTargetCtrl_ProgressAbortEvent()
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

    class TeachPosition
    {
        #region Fields and Properties

        public EFEMStation _station;
        public bool _IsTaught = false;

        public double taught_X;
        public double taught_Y;
        public double taught_Z;
        public double taught_HA;
        public double taught_O;   //approach angle
        public double taught_J2;
        public double taught_J3;
        public double taught_J4;
        public double taught_J6;
        public double taught_J7;

        public double current_X;
        public double current_Y;
        public double current_Z;
        public double current_HA;
        public double current_O;
        public double current_J2;
        public double current_J3;
        public double current_J4;
        public double current_J6;
        public double current_J7;
        
        #endregion

        #region Methods

        public HRESULT GetTaughtXYZPos()
        {
            return GUIBasic.Instance().MachineControl.GetTaughtXYZPos(_station, ref _IsTaught, ref taught_X, ref taught_Y, ref taught_Z, ref taught_O);
        }

        public HRESULT GetTaughtJointPos()
        {
            return GUIBasic.Instance().MachineControl.GetTaughtPos(_station, ref _IsTaught, ref taught_J2, ref taught_J3, ref taught_J4, ref taught_J6, ref taught_J7);
        }

        public HRESULT GetCurrentRobotXYZPos()
        {
            HRESULT hr = null;

            //Hand1
            hr = GUIBasic.Instance().MachineControl.GetCurrentRobotXYZPos(EFEMRobotArm.Lower, ref current_X, ref current_Y, ref current_Z, ref current_HA);
            //if (hr != null) MessageBox.Show(hr._agent.ToString() + "\n" + hr._category.ToString() + "\n" + hr._message + "\n" + hr._timestamp);

            //Hand2
            //hr = GUIBasic.Instance().MachineControl.GetCurrentRobotXYZPos(EFEMRobotArm.Upper, ref current_X, ref current_Y, ref current_Z, ref current_HA);
            return hr;
        }

        public HRESULT GetCurrentRobotPos()
        {
            return GUIBasic.Instance().MachineControl.GetCurrentRobotPos(ref current_J2, ref current_J3, ref current_J4, ref current_J6, ref current_J7);
        }

        public HRESULT SetTaughtPos()
        {
            return  GUIBasic.Instance().MachineControl.SetTaughtPos(_station, current_J2, current_J3, current_J4, current_J6, current_J7);
        }

        public HRESULT SetTaughtXYZPos()
        {
            return GUIBasic.Instance().MachineControl.SetTaughtXYZPos(_station, current_X, current_Y, current_Z, current_O);
        }

        #endregion
    }
}
