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
    public partial class TeachAlignerCtrl : UserControl
    {

        #region Fields and Properties

        private string _ProgressKey = string.Empty;

        #endregion

        #region Constructor

        public TeachAlignerCtrl()
        {
            InitializeComponent();
            cbHandSelection.SelectedIndex = 0;
        }        

        #endregion

        #region Methods

        private void TPool_ApplyAlnSpd(object parameters)
        {
            try
            {
                HRESULT hr = null;
                ArrayList list = (ArrayList)parameters;
                float speed = (float)list[0];

                hr = GUIBasic.Instance().MachineControl.SetAlignerSpeed(TargetMode.MaintenanceMode, speed);
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to set aligner speed to controller.\n  -->Reason: " + hr._message);
                }

            }
            catch (Exception e)
            {
                GUIBasic.Instance().ProgressException("Exception occured during applying aligner speed to controller. \n  -->Reason: " + e.Message);
            }
            finally
            {
                GUIBasic.Instance().ProgressAbortEvent -= new GUIBasic.ProgressAbort(TeachAlignerCtrl_ProgressAbortEvent);
                GUIBasic.Instance().ProgressEnd(_ProgressKey);
            }        
        }

        private void TPool_RefreshAlnSpd(object parameters)
        {
            try
            {
                HRESULT hr = null;
                float speed = float.NaN;

                hr = GUIBasic.Instance().MachineControl.GetCurrentAlignerSpeed(ref speed);
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to get aligner speed.\n  -->Reason: " + hr._message);
                    //this.Invoke(new MethodInvoker(delegate { lnbAlignerSpeed.Value = Convert.ToDecimal(speed); })); //default value
                }
                else
                    this.Invoke(new MethodInvoker(delegate { lnbAlignerSpeed.Value = Convert.ToDecimal(speed); }));

            }
            catch (Exception e)
            {
                GUIBasic.Instance().ProgressException("Exception occured during refreshing aligner speed. \n  -->Reason: " + e.Message);
            }
            finally
            {
                GUIBasic.Instance().ProgressAbortEvent -= new GUIBasic.ProgressAbort(TeachAlignerCtrl_ProgressAbortEvent);
                GUIBasic.Instance().ProgressEnd(_ProgressKey);
            }    
        }

        private void TPool_SwitchServo(object parameters)
        {
            try
            {
                bool IsServoOff = false;
                HRESULT hr = GUIBasic.Instance().MachineControl.IsServerOff(EFEMDeviceCode.A1, ref IsServoOff);
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to get servo status.\nReason: " + hr._message);
                    return;
                }

                if (IsServoOff)
                {
                    hr = GUIBasic.Instance().MachineControl.ServoON(EFEMDeviceCode.A1);
                    if (hr == null)
                    {
                        this.Invoke(new MethodInvoker(delegate { btnAlignerServo.Text = "OFF"; }));
                        this.Invoke(new MethodInvoker(delegate { bsAlignerServo.On = true; }));
                    }
                    else
                        GUIBasic.Instance().ProgressException("Fail to turn on servo.\n  -->Reason:" + hr._message);
                }
                else
                {
                    hr = GUIBasic.Instance().MachineControl.ServoOFF(EFEMDeviceCode.A1);
                    if (hr == null)
                    {
                        this.Invoke(new MethodInvoker(delegate { btnAlignerServo.Text = "ON"; }));
                        this.Invoke(new MethodInvoker(delegate { bsAlignerServo.On = false; }));
                    }
                    else
                        GUIBasic.Instance().ProgressException("Fail to  turn off servo.\n  -->Reason:" + hr._message);
                }
            }
            catch (Exception e)
            {
                GUIBasic.Instance().ProgressException("Exception occured during switching servo. \nReason: " + e.Message);
            }
            finally
            {
                GUIBasic.Instance().ProgressAbortEvent -= new GUIBasic.ProgressAbort(TeachAlignerCtrl_ProgressAbortEvent);
                GUIBasic.Instance().ProgressEnd(_ProgressKey);
            }
        }

        private void TPool_HomeAligner(object parameters)
        {
            try
            {
                HRESULT hr = null;
                ArrayList list = (ArrayList)parameters;
                bool IncludeZ = (bool)list[0];
                bool IsNeedHome = false;

                hr = GUIBasic.Instance().MachineControl.GoHome(EFEMDeviceCode.A1, IncludeZ);
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to home aligner.\n  -->Reason: " + hr._message);
                    return;
                }

                hr = GUIBasic.Instance().MachineControl.IsNeedHome(EFEMDeviceCode.A1, ref IsNeedHome);
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to check aligner is home or not.\n  -->Reason: " + hr._message);
                }
                else
                    this.Invoke(new MethodInvoker(delegate { bsHome.On = !IsNeedHome; }));

            }
            catch (Exception e)
            {
                GUIBasic.Instance().ProgressException("Exception occured during homing aligner. \n  -->Reason: " + e.Message);
            }
            finally
            {
                GUIBasic.Instance().ProgressAbortEvent -= new GUIBasic.ProgressAbort(TeachAlignerCtrl_ProgressAbortEvent);
                GUIBasic.Instance().ProgressEnd(_ProgressKey);
            }
        }

        private void TPool_HoldWafer(object parameters)
        {
            try
            {
                HRESULT hr = null;

                hr = GUIBasic.Instance().MachineControl.HoldWafer(EFEMDeviceCode.A1, EFEMRobotArm.Lower);
                if (hr == null) this.Invoke(new MethodInvoker(delegate { bsHoldwfr.On = true; }));
                else GUIBasic.Instance().ProgressException("Fail to command Aligner to hold wafer .\n  -->Reason: " + hr._message);
            }
            catch (Exception e)
            {
                GUIBasic.Instance().ProgressException("Exception occured during holding wafer. \n  -->Reason: " + e.Message);
            }
            finally
            {
                GUIBasic.Instance().ProgressAbortEvent -= new GUIBasic.ProgressAbort(TeachAlignerCtrl_ProgressAbortEvent);
                GUIBasic.Instance().ProgressEnd(_ProgressKey);
            }
        }

        private void TPool_ReleaseWafer(object parameters)
        {
            try
            {
                HRESULT hr = null;

                hr = GUIBasic.Instance().MachineControl.ReleaseWafer(EFEMDeviceCode.A1, EFEMRobotArm.Lower);
                if (hr == null) this.Invoke(new MethodInvoker(delegate { bsHoldwfr.On = false; }));
                else GUIBasic.Instance().ProgressException("Fail to command Aligner to release wafer .\n  -->Reason: " + hr._message);

            }
            catch (Exception e)
            {
                GUIBasic.Instance().ProgressException("Exception occured during releasing wafer. \n  -->Reason: " + e.Message);
            }
            finally
            {
                GUIBasic.Instance().ProgressAbortEvent -= new GUIBasic.ProgressAbort(TeachAlignerCtrl_ProgressAbortEvent);
                GUIBasic.Instance().ProgressEnd(_ProgressKey);
            }
        }

        private void TPool_AlignWafer(object parameters)
        {
            try
            {
                HRESULT hr = null;

                hr = GUIBasic.Instance().MachineControl.AlignWafer();
                if (hr != null) GUIBasic.Instance().ProgressException("Fail to command Aligner to align wafer .\n  -->Reason: " + hr._message);

            }
            catch (Exception e)
            {
                GUIBasic.Instance().ProgressException("Exception occured during aligning wafer. \n  -->Reason: " + e.Message);
            }
            finally
            {
                GUIBasic.Instance().ProgressAbortEvent -= new GUIBasic.ProgressAbort(TeachAlignerCtrl_ProgressAbortEvent);
                GUIBasic.Instance().ProgressEnd(_ProgressKey);
            }
        }

        private void TPool_ReAlignWafer(object parameters)
        {
            try
            {
                HRESULT hr = null;
                ArrayList list = (ArrayList)parameters;
                bool IsHand1 = (bool)list[0];

                hr = GUIBasic.Instance().MachineControl.ReAlignWafer((IsHand1) ? EFEMRobotArm.Lower : EFEMRobotArm.Upper);
                if (hr != null) GUIBasic.Instance().ProgressException("Fail to command Aligner to re-align wafer .\n  -->Reason: " + hr._message);

            }
            catch (Exception e)
            {
                GUIBasic.Instance().ProgressException("Exception occured during re-aligning wafer. \n  -->Reason: " + e.Message);
            }
            finally
            {
                GUIBasic.Instance().ProgressAbortEvent -= new GUIBasic.ProgressAbort(TeachAlignerCtrl_ProgressAbortEvent);
                GUIBasic.Instance().ProgressEnd(_ProgressKey);
            }
        }

        private void TPool_ApplyAlignAngle(object parameters)
        {
            try
            {
                HRESULT hr = null;
                ArrayList list = (ArrayList)parameters;
                double angle = (double)list[0];

                hr = GUIBasic.Instance().MachineControl.SetWaferAlignAngle(angle);
                if (hr != null) GUIBasic.Instance().ProgressException("Fail to set wafer align angle.\n  -->Reason: " + hr._message);

            }
            catch (Exception e)
            {
                GUIBasic.Instance().ProgressException("Exception occured during setting wafer align angle. \n  -->Reason: " + e.Message);
            }
            finally
            {
                GUIBasic.Instance().ProgressAbortEvent -= new GUIBasic.ProgressAbort(TeachAlignerCtrl_ProgressAbortEvent);
                GUIBasic.Instance().ProgressEnd(_ProgressKey);
            }
        }

        private void TPool_RefreshAlnPos(object parameters)
        {
            try
            {
                HRESULT hr = null;
                double angle = double.NaN;

                hr = GUIBasic.Instance().MachineControl.GetCurrentAlignerPos(ref angle);
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to get aligner current position in degree unit.\n  -->Reason: " + hr._message);
                    //this.Invoke(new MethodInvoker(delegate {  tCurrent.Text = "NaN"; })); //default value
                }
                else
                    this.Invoke(new MethodInvoker(delegate { tCurrent.Text = angle.ToString(); }));

            }
            catch (Exception e)
            {
                GUIBasic.Instance().ProgressException("Exception occured during refreshing aligner position. \n  -->Reason: " + e.Message);
            }
            finally
            {
                GUIBasic.Instance().ProgressAbortEvent -= new GUIBasic.ProgressAbort(TeachAlignerCtrl_ProgressAbortEvent);
                GUIBasic.Instance().ProgressEnd(_ProgressKey);
            }
        }

        private void TPool_IsWaferPresence(object parameters)
        {
            try
            {
                HRESULT hr = null;
                bool IsWaferPresence = false;

                hr = GUIBasic.Instance().MachineControl.IsWaferPresence(EFEMDeviceCode.A1, ref IsWaferPresence);
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to get information about wafer presence.\n  -->Reason: " + hr._message);
                }
                else
                    this.Invoke(new MethodInvoker(delegate { bsWfrPresence.On = IsWaferPresence; }));

            }
            catch (Exception e)
            {
                GUIBasic.Instance().ProgressException("Exception occured during getting wafer status. \n  -->Reason: " + e.Message);
            }
            finally
            {
                GUIBasic.Instance().ProgressAbortEvent -= new GUIBasic.ProgressAbort(TeachAlignerCtrl_ProgressAbortEvent);
                GUIBasic.Instance().ProgressEnd(_ProgressKey);
            }
        }

        private void TPool_MovALN(object parameters)
        {
            try
            {
                HRESULT hr = null;
                ArrayList list = (ArrayList)parameters;
                bool IsAbs = (bool)list[0];
                double step = (double)list[1];

                hr = GUIBasic.Instance().MachineControl.MoveJoint(EFEMRobotJoint.Aligner, IsAbs,step);
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to rotate aligner relatively.\n  -->Reason: " + hr._message);
                }

            }
            catch (Exception e)
            {
                GUIBasic.Instance().ProgressException("Exception occured during rotating aligner. \n  -->Reason: " + e.Message);
            }
            finally
            {
                GUIBasic.Instance().ProgressAbortEvent -= new GUIBasic.ProgressAbort(TeachAlignerCtrl_ProgressAbortEvent);
                GUIBasic.Instance().ProgressEnd(_ProgressKey);
            }
        }

        
        #endregion

        #region Events

        private void btnApplyAlnSpd_Click(object sender, EventArgs e)
        {
            GUIBasic.Instance().ProgressAbortEvent += new GUIBasic.ProgressAbort(TeachAlignerCtrl_ProgressAbortEvent);
            _ProgressKey = GUIBasic.Instance().ProgressStart("To apply aligner speed to robot controller", string.Format("Controller is writing..."), 0, true, false);
            ArrayList list = new ArrayList();
            list.Add(lnbAlignerSpeed.Value);

            ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_ApplyAlnSpd), list);
        }

        private void btnRefreshAlnSpd_Click(object sender, EventArgs e)
        {
            GUIBasic.Instance().ProgressAbortEvent += new GUIBasic.ProgressAbort(TeachAlignerCtrl_ProgressAbortEvent);
            _ProgressKey = GUIBasic.Instance().ProgressStart("To get aligner speed from robot controller", string.Format("Controller is reading..."), 0, true, false);
            ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_RefreshAlnSpd), null);
        }

        private void btnAlignerServo_Click(object sender, EventArgs e)
        {
            GUIBasic.Instance().ProgressAbortEvent += new GUIBasic.ProgressAbort(TeachAlignerCtrl_ProgressAbortEvent);
            _ProgressKey = GUIBasic.Instance().ProgressStart("To switch servo", string.Format("Servo is swtiching..."), 0, true, false);
            ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_SwitchServo), null);
        }

        private void btnHomeAligner_Click(object sender, EventArgs e)
        {
            GUIBasic.Instance().ProgressAbortEvent += new GUIBasic.ProgressAbort(TeachAlignerCtrl_ProgressAbortEvent);
            _ProgressKey = GUIBasic.Instance().ProgressStart("To home aligner", string.Format("Aligner is homing..."), 0, true, false);
            ArrayList list = new ArrayList();
            list.Add(false); //Home aligner without including Z

            ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_HomeAligner), list);
        }

        private void btnHold_Click(object sender, EventArgs e)
        {
            GUIBasic.Instance().ProgressAbortEvent += new GUIBasic.ProgressAbort(TeachAlignerCtrl_ProgressAbortEvent);
            _ProgressKey = GUIBasic.Instance().ProgressStart("To hold wafer", string.Format("Holding wafer..."), 0, true, false);

            ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_HoldWafer), null);
        }

        private void btnRelease_Click(object sender, EventArgs e)
        {
            GUIBasic.Instance().ProgressAbortEvent += new GUIBasic.ProgressAbort(TeachAlignerCtrl_ProgressAbortEvent);
            _ProgressKey = GUIBasic.Instance().ProgressStart("To release wafer", string.Format("Releasing wafer..."), 0, true, false);

            ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_ReleaseWafer), null);
        }

        private void btnAlign_Click(object sender, EventArgs e)
        {
            GUIBasic.Instance().ProgressAbortEvent += new GUIBasic.ProgressAbort(TeachAlignerCtrl_ProgressAbortEvent);
            _ProgressKey = GUIBasic.Instance().ProgressStart("To align wafer", string.Format("Aligning wafer..."), 0, true, false);

            ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_AlignWafer), null);
        }

        private void btnReAlign_Click(object sender, EventArgs e)
        {
            GUIBasic.Instance().ProgressAbortEvent += new GUIBasic.ProgressAbort(TeachAlignerCtrl_ProgressAbortEvent);
            _ProgressKey = GUIBasic.Instance().ProgressStart("To re-align wafer", string.Format("Re-aligning wafer..."), 0, true, false);
            ArrayList list = new ArrayList();
            list.Add((cbHandSelection.SelectedIndex == 0) ? true : false);

            ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_ReAlignWafer), list);
        }

        private void btnApplyAlignAngle_Click(object sender, EventArgs e)
        {
            GUIBasic.Instance().ProgressAbortEvent += new GUIBasic.ProgressAbort(TeachAlignerCtrl_ProgressAbortEvent);
            _ProgressKey = GUIBasic.Instance().ProgressStart("To set wafer align angle ", string.Format("Setting wafer align angle..."), 0, true, false);
            ArrayList list = new ArrayList();
            list.Add(lnbAlignAngle.Value);

            ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_ApplyAlignAngle), list);
        }

        private void btnRefreshAlnPos_Click(object sender, EventArgs e)
        {
            GUIBasic.Instance().ProgressAbortEvent += new GUIBasic.ProgressAbort(TeachAlignerCtrl_ProgressAbortEvent);
            _ProgressKey = GUIBasic.Instance().ProgressStart("To get aligner current position from robot controller", string.Format("Controller is reading..."), 0, true, false);
            ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_RefreshAlnPos), null);
        }

        private void btnWfrPresence_Click(object sender, EventArgs e)
        {
            GUIBasic.Instance().ProgressAbortEvent += new GUIBasic.ProgressAbort(TeachAlignerCtrl_ProgressAbortEvent);
            _ProgressKey = GUIBasic.Instance().ProgressStart("To check wafer is on aligner", string.Format("Checking wafer presence..."), 0, true, false);
            ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_IsWaferPresence), null);
        }

        private void btnNeg_Click(object sender, EventArgs e)
        {
            GUIBasic.Instance().ProgressAbortEvent += new GUIBasic.ProgressAbort(TeachAlignerCtrl_ProgressAbortEvent);
            _ProgressKey = GUIBasic.Instance().ProgressStart("To move aligner relatively", string.Format("Aligner is rotating - {0} degree...", lnbStepAngle.Value.ToString()), 0, true, false);
            ArrayList list = new ArrayList();
            list.Add(false);
            list.Add(lnbStepAngle.Value*(-1));

            ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_MovALN), list);
        }

        private void btnPos_Click(object sender, EventArgs e)
        {
            GUIBasic.Instance().ProgressAbortEvent += new GUIBasic.ProgressAbort(TeachAlignerCtrl_ProgressAbortEvent);
            _ProgressKey = GUIBasic.Instance().ProgressStart("To move aligner relatively", string.Format("Aligner is rotating +{0} degree...", lnbStepAngle.Value.ToString()), 0, true, false);
            ArrayList list = new ArrayList();
            list.Add(false);
            list.Add(lnbStepAngle.Value);

            ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_MovALN), list);
        }

        private void btnMovABS_Click(object sender, EventArgs e)
        {
            GUIBasic.Instance().ProgressAbortEvent += new GUIBasic.ProgressAbort(TeachAlignerCtrl_ProgressAbortEvent);
            _ProgressKey = GUIBasic.Instance().ProgressStart("To move aligner absolutely", string.Format("Aligner is rotating to {0} degree...", lnbABSAngle.Value.ToString()), 0, true, false);
            ArrayList list = new ArrayList();
            list.Add(true);
            list.Add(lnbABSAngle.Value);

            ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_MovALN), list);
        }

        private void TeachAlignerCtrl_ProgressAbortEvent()
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
