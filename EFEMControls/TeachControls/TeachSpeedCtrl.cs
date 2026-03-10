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
using EFEM.DataCenter;
using EFEM.ExceptionManagements;
using System.Threading;

namespace EFEM.GUIControls.TeachControls
{
    public partial class TeachSpeedCtrl : UserControl
    {

        #region Fields and Properties

        private EFEMStation _station;
        public EFEMStation CurrentStation
        {
            get { return _station;}
            set { _station = value;}
        }

        //private float _robotspeed;                //Robot motion speed
        //private object _SPDGERR;             //Evacuation speed when getting wafer is failed in GETS operation
        //private object _GERRRET;             //Evacuation (ON) / No evacuation (OFF) after failing to get wafer in GETS operation
        //private object _SPDPERR;             //Evacuation speed when getting wafer is failed in PUTS operation
        //private object _PERRRET;             //Evacuation (ON) / No evacuation (OFF) after failing to get wafer in PUTS operation
        //private object _SPDUPDN;            //Z axis evelating speed when placing wafer is failed 
        //private int _SPDZU1;                     //Z axis ascent rate, acceleration/deceleration speed to taught height when getting wafer
        //private int _SPDZD1;                     //Z axis drop rate, acceleration/deceleration speed to taught height when putting wafer

        private string _ProgressKey = string.Empty;

        #endregion

        #region Constructor

        public TeachSpeedCtrl()
        {
            InitializeComponent();
            IniTooltip();
        }
        
        #endregion

        #region Methods

        private void IniTooltip()
        {
            string sInfo;

            //Wafer sensing mode
            ToolTip ttInfo_WfrSensingMde = new ToolTip();
            ttInfo_WfrSensingMde.ToolTipTitle = "Under Maintenance Mode";
            sInfo = "Set sensing mode";
            ttInfo_WfrSensingMde.SetToolTip(btnInfo_WfrSensingMde, sInfo);

            //Robot motion speed
            ToolTip ttInfo_RbtSpd = new ToolTip();
            ttInfo_RbtSpd.ToolTipTitle = "Robot motion speed";
            sInfo = "Set Robot motion speed";
            ttInfo_RbtSpd.SetToolTip(btnInfo_RbtSpd, sInfo);

            //Evacuation speed when getting wafer is failed in GETS operation
            ToolTip ttInfo_GETSspd = new ToolTip();
            ttInfo_GETSspd.ToolTipTitle = "Evacuation speed as GETS failed";
            sInfo = " Evacuation speed when getting wafer is failed in GETS operation";
            ttInfo_GETSspd.SetToolTip(btnInfo_GETSspd, sInfo);

            //Evacuation speed when placing wafer is failed in PUTS operation
            ToolTip ttInfo_PUTSspd = new ToolTip();
            ttInfo_PUTSspd.ToolTipTitle = "Evacuation speed as PUTS failed";
            sInfo = " Evacuation speed when placing wafer is failed in PUTS operation";
            ttInfo_PUTSspd.SetToolTip(btnInfo_PUTSspd, sInfo);

            //Z axis evelating speed when placing wafer is failed 
            ToolTip ttInfo_Zspd = new ToolTip();
            ttInfo_Zspd.ToolTipTitle = "Evacuation speed as failed";
            sInfo = " Z axis evelating speed when placing wafer is failed";
            ttInfo_Zspd.SetToolTip(btnInfo_Zspd, sInfo);
        }

        public void InitWfrSensingMde()
        {
            WaferSensingMode maxValue = ExtendMethods.GetMaxValue<WaferSensingMode>();
            cbWaferSensingMode.Items.Clear();

            for (int i = 1; i <= (int)maxValue; i++)
            {
                cbWaferSensingMode.Items.Add(string.Format("{0} : {1}", i, ((WaferSensingMode)i).ToString()));
            }

            WaferSensingMode curValue = WaferSensingMode.Unknwon;
            if (GUIBasic.Instance().MachineControl.GetWaferSensingMode(true, ref curValue) != null)
                curValue = GUIBasic.Instance().MachineControl.DefaultWaferSensingMode;

            cbWaferSensingMode.SelectedIndex = (int)curValue - 1;
        }

        private void TPool_Apply(object parameters)
        {
            try
            {
                ArrayList list = (ArrayList)parameters;
                float robotSpeed = Convert.ToSingle(list[0]);
                float evacuationSpeedG = Convert.ToSingle(list[1]);
                bool IsEnableGevacuation = (bool)list[2];
                float evacuationSpeedP = Convert.ToSingle(list[3]);
                bool IsEnablePevacuation = (bool)list[4];
                float evacuationSpeedZ = Convert.ToSingle(list[5]);
                int indexWfrSensing = (int)list[6];

                //Robot motion speed
                //HRESULT hr = GUIBasic.Instance().MachineControl.SetRobotSpeed(TargetMode.MaintenanceMode, (float)lnbSpeedR.Value);
                HRESULT hr = GUIBasic.Instance().MachineControl.SetRobotSpeed(TargetMode.MaintenanceMode, robotSpeed);

                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to set robot speed.\n  -->Reason: " + hr._message);
                    return;
                }

                //Evacuation speed when getting wafer is failed in GETS operation
                //hr = GUIBasic.Instance().MachineControl.SetParameter("SPDGERR", lnbEvacuationSpdG.Value);
                hr = GUIBasic.Instance().MachineControl.SetParameter("SPDGERR", evacuationSpeedG);
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to set evacuation speed  when getting wafer is failed in GETS operation.\n  -->Reason: " + hr._message);
                    return;
                }

                //Evacuation (ON) / No evacuation (OFF) after failing to get wafer in GETS operation
                //hr = GUIBasic.Instance().MachineControl.SetParameter("GERRRET", ckbEnableG.Checked);
                hr = GUIBasic.Instance().MachineControl.SetParameter("GERRRET", IsEnableGevacuation);
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to set evacuation ON/OFF after failing to get wafer in GETS operation.\n  -->Reason: " + hr._message);
                    return;
                }

                //Evacuation speed when placing wafer is failed in PUTS operation
                //hr = GUIBasic.Instance().MachineControl.SetParameter("SPDPERR", lnbEvacuationSpdP.Value);
                hr = GUIBasic.Instance().MachineControl.SetParameter("SPDPERR", evacuationSpeedP);
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to set evacuation speed  when placing wafer is failed in PUTS operation.\n  -->Reason: " + hr._message);
                    return;
                }

                //Evacuation (ON) / No evacuation (OFF) after failing to get wafer in PUTS operation
                //hr = GUIBasic.Instance().MachineControl.SetParameter("PERRRET", ckbEnableP.Checked);
                hr = GUIBasic.Instance().MachineControl.SetParameter("PERRRET", IsEnablePevacuation);
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to set evacuation ON/OFF after failing to get wafer in PUTS operation.\n  -->Reason: " + hr._message);
                    return;
                }

                //Z axis evelating speed when placing wafer is failed 
                //hr = GUIBasic.Instance().MachineControl.SetParameter("SPDUPDN", lnbEvacuationSpdZ.Value);
                hr = GUIBasic.Instance().MachineControl.SetParameter("SPDUPDN", evacuationSpeedZ);
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to set Z axis evelating speed when placing wafer.\n  -->Reason: " + hr._message);
                    return;
                }

                //Wafer sensing mode
                if (indexWfrSensing == -1)
                    return;
                else
                {
                    hr = GUIBasic.Instance().MachineControl.SetWaferSensingMode(true, (WaferSensingMode)(indexWfrSensing + 1));
                    if (hr != null)
                    {
                        GUIBasic.Instance().ProgressException("Fail to set Wafer Sensing Mode.\n  -->Reason: " + hr._message);
                        return;
                    }

                    WaferSensingMode rtnValue = WaferSensingMode.Unknwon;
                    GUIBasic.Instance().MachineControl.GetWaferSensingMode(true, ref rtnValue);
                    if (hr != null)
                    {
                        GUIBasic.Instance().ProgressException("Fail to get Wafer Sensing Mode.\n  -->Reason: " + hr._message);
                        return;
                    }

                    if ((int)rtnValue != (indexWfrSensing + 1))
                    {
                        if (hr != null)
                        {
                            GUIBasic.Instance().ProgressException("Fail to update Wafer Sensing Mode.\n  -->Reason: " + hr._message);
                            return;
                        }

                        if ((int)rtnValue >= 0)
                            this.Invoke(new MethodInvoker(delegate { cbWaferSensingMode.SelectedIndex = (int)rtnValue - 1; }));

                    }
                    else
                    {
                        GUIBasic.Instance().FileUtility.WritePrivateProfileString("EFEMControl", "WaferSensingMode_Maintenace", ((WaferSensingMode)(indexWfrSensing + 1)).ToString());
                    }
                }

            }
            catch (Exception e)
            {
                GUIBasic.Instance().ProgressException("Exception occured during applying configuration. \n  -->Reason: " + e.Message);
            }
            finally
            {
                GUIBasic.Instance().ProgressAbortEvent -= new GUIBasic.ProgressAbort(TeachSpeedCtrl_ProgressAbortEvent);
                GUIBasic.Instance().ProgressEnd(_ProgressKey);
            }

        }

        private void TPool_Refresh(object data)
        {
            try
            {

                float robotspeed = float.NaN;                //Robot motion speed                
                object SPDGERR = null;            //Evacuation speed when getting wafer is failed in GETS operation
                object GERRRET = null;           //Evacuation (ON) / No evacuation (OFF) after failing to get wafer in GETS operation
                object SPDPERR = null;            //Evacuation speed when getting wafer is failed in PUTS operation
                object PERRRET = null;            //Evacuation (ON) / No evacuation (OFF) after failing to get wafer in PUTS operation
                object SPDUPDN = null;            //Z axis evelating speed when placing wafer is failed 

                //Robot motion speed
                HRESULT hr = GUIBasic.Instance().MachineControl.GetRobotSpeed(TargetMode.MaintenanceMode, ref robotspeed);
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to get robot speed.\n  -->Reason: " + hr._message);
                }
                this.Invoke(new MethodInvoker(delegate { lnbSpeedR.Value = Convert.ToDecimal(robotspeed); }));
                

                //Evacuation speed when getting wafer is failed in GETS operation
                hr = GUIBasic.Instance().MachineControl.GetParameter("SPDGERR", ref SPDGERR);
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to get evacuation speed when getting wafer is failed in GETS operation.\n  -->Reason: " + hr._message);
                }
                this.Invoke(new MethodInvoker(delegate { lnbEvacuationSpdG.Value = Convert.ToDecimal(SPDGERR); }));
                

                //Evacuation (ON) / No evacuation (OFF) after failing to get wafer in GETS operation
                hr = GUIBasic.Instance().MachineControl.GetParameter("GERRRET", ref GERRRET);
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to get evacuation ON/OFF after failing to get wafer in GETS operation.\n  -->Reason: " + hr._message);
                }
                this.Invoke(new MethodInvoker(delegate { ckbEnableG.Checked = Convert.ToBoolean(GERRRET); }));
                

                //Evacuation speed when placing wafer is failed in PUTS operation              
                hr = GUIBasic.Instance().MachineControl.GetParameter("SPDPERR", ref SPDPERR);
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to get evacuation speed when placing wafer is failed in PUTS operation.\n  -->Reason: " + hr._message);
                }
                this.Invoke(new MethodInvoker(delegate { lnbEvacuationSpdP.Value = Convert.ToDecimal(SPDPERR); }));
                

                //Evacuation (ON) / No evacuation (OFF) after failing to get wafer in PUTS operation
                hr = GUIBasic.Instance().MachineControl.GetParameter("PERRRET", ref PERRRET);
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to get evacuation ON/OFF after failing to get wafer in PUTS operation.\n  -->Reason: " + hr._message);
                }
                this.Invoke(new MethodInvoker(delegate { ckbEnableP.Checked = Convert.ToBoolean(PERRRET); }));
               

                //Z axis evelating speed when placing wafer is failed
                hr = GUIBasic.Instance().MachineControl.GetParameter("SPDUPDN", ref SPDUPDN);
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to get Z axis evelating speed when placing wafer is failed.\n  -->Reason: " + hr._message);
                }
                this.Invoke(new MethodInvoker(delegate { lnbEvacuationSpdZ.Value = Convert.ToDecimal(SPDUPDN); }));
                
            }
            catch (Exception e)
            {
                GUIBasic.Instance().ProgressException("Exception occured during refreshing seepd.\n  -->Reason: " + e.Message);
            }
            finally
            {
                GUIBasic.Instance().ProgressAbortEvent -= new GUIBasic.ProgressAbort(TeachSpeedCtrl_ProgressAbortEvent);
                GUIBasic.Instance().ProgressEnd(_ProgressKey);
            }            
        }

        #endregion

        #region Events

        private void btnApply_Click(object sender, EventArgs e)
        {
            GUIBasic.Instance().ProgressAbortEvent += new GUIBasic.ProgressAbort(TeachSpeedCtrl_ProgressAbortEvent);
            _ProgressKey = GUIBasic.Instance().ProgressStart("To apply motion speed to robot controller", string.Format("Controller is writing..."), 0, true, false);
            ArrayList list = new ArrayList();
            list.Add(lnbSpeedR.Value);
            list.Add(lnbEvacuationSpdG.Value);
            list.Add(ckbEnableG.Checked);
            list.Add(lnbEvacuationSpdP.Value);
            list.Add(ckbEnableP.Checked);
            list.Add(lnbEvacuationSpdZ.Value);
            list.Add(cbWaferSensingMode.SelectedIndex);

            ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_Apply), list);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            GUIBasic.Instance().ProgressAbortEvent += new GUIBasic.ProgressAbort(TeachSpeedCtrl_ProgressAbortEvent);
            _ProgressKey = GUIBasic.Instance().ProgressStart("To get motion speed  from robot controller", string.Format("Controller is reading..."), 0, true, false);
            ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_Refresh), null);
        }

        private void TeachSpeedCtrl_ProgressAbortEvent()
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
