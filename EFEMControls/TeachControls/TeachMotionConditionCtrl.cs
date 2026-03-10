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
    public partial class TeachMotionConditionCtrl : UserControl
    {

        #region Fields and Properties

        private EFEMStation _station;
        public EFEMStation CurrentStation
        {
            get { return _station; }
            set { _station = value; }
        }

        private int _UOFF;
        private int _LOFF;
        private int _GOFF;
        private int _GCNF;
        private int _PADJ;
        private int _POFF;
        private int _PCNF;

        private string _ProgressKey = string.Empty;

        #endregion

        #region Constructor

        public TeachMotionConditionCtrl()
        {
            InitializeComponent();
            IniTooltip();
        } 
       
        #endregion

        #region Methods

        private void IniTooltip()
        {
            string sInfo;

            //UOFF
            ToolTip ttInfo_UOFF = new ToolTip();
            ttInfo_UOFF.ToolTipTitle = "Taught Position";
            sInfo = "upper path distance in Z direction";
            ttInfo_UOFF.SetToolTip(btnInfo_UOFF, sInfo);

            //UOFF
            ToolTip ttInfo_LOFF = new ToolTip();
            ttInfo_LOFF.ToolTipTitle = "Taught Position";
            sInfo = "lower path distance in Z direction";
            ttInfo_LOFF.SetToolTip(btnInfo_LOFF, sInfo);

            //GOFF
            ToolTip ttInfo_GOFF = new ToolTip();
            ttInfo_GOFF.ToolTipTitle = "Taught Position";
            sInfo = "up path distance in Y direction";
            ttInfo_GOFF.SetToolTip(btnInfo_GOFF, sInfo);

            //GCNF
            ToolTip ttInfo_GCNF = new ToolTip();
            ttInfo_GCNF.ToolTipTitle = "Taught Position-wafer grip postion";
            sInfo = "distance in Y direction";
            ttInfo_GCNF.SetToolTip(btnInfo_GCNF, sInfo);

            //PADJ
            ToolTip ttInfo_PADJ = new ToolTip();
            ttInfo_PADJ.ToolTipTitle = "Taught Position-wafer put postion";
            sInfo = "distance in Y direction";
            ttInfo_PADJ.SetToolTip(btnInfo_PADJ, sInfo);

            //POFF
            ToolTip ttInfo_POFF = new ToolTip();
            ttInfo_POFF.ToolTipTitle = "Wafer put postion";
            sInfo = "lower path bottom distance in Y direction";
            ttInfo_POFF.SetToolTip(btnInfo_POFF, sInfo);

            //PCNF
            ToolTip ttInfo_PCNF = new ToolTip();
            ttInfo_PCNF.ToolTipTitle = "Wafer put postion-gripping position";
            sInfo = "distance in Y direction";
            ttInfo_PCNF.SetToolTip(btnInfo_PCNF, sInfo);
        }

        private void TPool_Apply(object parameters)
        {
            try
            {
                ArrayList list = (ArrayList)parameters;
                int UOFF = (int)list[0];
                int LOFF = (int)list[1];
                int GOFF = (int)list[2];
                int GCNF = (int)list[3];
                int PADJ = (int)list[4];
                int POFF = (int)list[5];
                int PCNF = (int)list[6];


                //HRESULT hr = GUIBasic.Instance().MachineControl.SetStationParameter(_station, "UOFF", Convert.ToInt16(lnbUOFF.Value));
                HRESULT hr = GUIBasic.Instance().MachineControl.SetStationParameter(_station, "UOFF", Convert.ToInt16(UOFF));
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to set UOFF.\n  -->Reason: " + hr._message);
                    return;
                }

                //hr = GUIBasic.Instance().MachineControl.SetStationParameter(_station, "LOFF", Convert.ToInt16(lnbLOFF.Value));
                hr = GUIBasic.Instance().MachineControl.SetStationParameter(_station, "LOFF", Convert.ToInt16(LOFF));
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to set LOFF.\n  -->Reason: " + hr._message);
                    return;
                }

                //hr = GUIBasic.Instance().MachineControl.SetStationParameter(_station, "GOFF", Convert.ToInt16(lnbGOFF.Value));
                hr = GUIBasic.Instance().MachineControl.SetStationParameter(_station, "GOFF", Convert.ToInt16(GOFF));
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to set GOFF.\n  -->Reason: " + hr._message);
                    return;
                }

                //hr = GUIBasic.Instance().MachineControl.SetStationParameter(_station, "GCNF", Convert.ToInt16(lnbGCNF.Value));
                hr = GUIBasic.Instance().MachineControl.SetStationParameter(_station, "GCNF", Convert.ToInt16(GCNF));
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to set GCNF.\n  -->Reason: " + hr._message);
                    return;
                }

                //hr = GUIBasic.Instance().MachineControl.SetStationParameter(_station, "PADJ", Convert.ToInt16(lnbPADJ.Value));
                hr = GUIBasic.Instance().MachineControl.SetStationParameter(_station, "PADJ", Convert.ToInt16(PADJ));
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to set PADJ.\n  -->Reason: " + hr._message);
                    return;
                }

                //hr = GUIBasic.Instance().MachineControl.SetStationParameter(_station, "POFF", Convert.ToInt16(lnbPOFF.Value));
                hr = GUIBasic.Instance().MachineControl.SetStationParameter(_station, "POFF", Convert.ToInt16(POFF));
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to set POFF.\n  -->Reason: " + hr._message);
                    return;
                }

                //hr = GUIBasic.Instance().MachineControl.SetStationParameter(_station, "PCNF", Convert.ToInt16(lnbPCNF.Value));
                hr = GUIBasic.Instance().MachineControl.SetStationParameter(_station, "PCNF", Convert.ToInt16(PCNF));
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to set PCNF.\n  -->Reason: " + hr._message);
                    return;
                }
            }
            catch (Exception e)
            {
                GUIBasic.Instance().ProgressException("Exception occured during applying motion condition. \n  -->Reason: " + e.Message);
            }
            finally
            {
                GUIBasic.Instance().ProgressAbortEvent -= new GUIBasic.ProgressAbort(TeachMotionConditionCtrl_ProgressAbortEvent);
                GUIBasic.Instance().ProgressEnd(_ProgressKey);
            }
        }

        private void TPool_Refresh(object parameters)
        {
            try
            {   
                HRESULT hr = null;

                hr = GUIBasic.Instance().MachineControl.GetStationParameter(_station, "UOFF", ref _UOFF);
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to get UOFF.\n  -->Reason: " + hr._message);
                    return;
                    //this.Invoke(new MethodInvoker(delegate { lnbUOFF.Value = _UOFF; })); //Default value
                }
                else
                     this.Invoke(new MethodInvoker(delegate { lnbUOFF.Value = _UOFF;}));                

                hr = GUIBasic.Instance().MachineControl.GetStationParameter(_station, "LOFF", ref _LOFF);
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to get LOFF.\n  -->Reason: " + hr._message);
                    return;
                    //this.Invoke(new MethodInvoker(delegate { lnbLOFF.Value = _LOFF; })); //Default value
                }
                else
                    this.Invoke(new MethodInvoker(delegate { lnbLOFF.Value = _LOFF; }));            

                hr = GUIBasic.Instance().MachineControl.GetStationParameter(_station, "GOFF", ref _GOFF);
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to get GOFF.\n  -->Reason: " + hr._message);
                    return;
                    //this.Invoke(new MethodInvoker(delegate { lnbGOFF.Value = _GOFF; })); //Default value
                }
                else
                    this.Invoke(new MethodInvoker(delegate { lnbGOFF.Value = _GOFF;}));                

                hr = GUIBasic.Instance().MachineControl.GetStationParameter(_station, "GCNF", ref _GCNF);
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to get GCNF.\n  -->Reason: " + hr._message);
                    return;
                    //this.Invoke(new MethodInvoker(delegate { lnbGCNF.Value = _GCNF; })); //Default value
                }
                else
                    this.Invoke(new MethodInvoker(delegate { lnbGCNF.Value = _GCNF;}));                

                hr = GUIBasic.Instance().MachineControl.GetStationParameter(_station, "PADJ", ref  _PADJ);
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to get PADJ.\n  -->Reason: " + hr._message);
                    return;
                    //this.Invoke(new MethodInvoker(delegate { lnbPADJ.Value = _PADJ; })); //Default value
                }
                else
                    this.Invoke(new MethodInvoker(delegate { lnbPADJ.Value = _PADJ;}));                

                hr = GUIBasic.Instance().MachineControl.GetStationParameter(_station, "POFF", ref _POFF);
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to get POFF.\n  -->Reason: " + hr._message);
                    return;
                    //this.Invoke(new MethodInvoker(delegate { lnbPOFF.Value = _POFF; })); //Default value
                }
                else
                    this.Invoke(new MethodInvoker(delegate { lnbPOFF.Value = _POFF;}));                

                hr = GUIBasic.Instance().MachineControl.GetStationParameter(_station, "PCNF", ref _PCNF);
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to get PCNF.\n  -->Reason: " + hr._message);
                    return;
                    //this.Invoke(new MethodInvoker(delegate { lnbPCNF.Value = _PCNF; })); //Default value
                }
                else
                 this.Invoke(new MethodInvoker(delegate { lnbPCNF.Value = _PCNF;}));
                
            }
            catch (Exception e)
            {
                GUIBasic.Instance().ProgressException("Exception occured during refreshing motion condition. \n  -->Reason: " + e.Message);
            }
            finally
            {
                GUIBasic.Instance().ProgressAbortEvent -= new GUIBasic.ProgressAbort(TeachMotionConditionCtrl_ProgressAbortEvent);
                GUIBasic.Instance().ProgressEnd(_ProgressKey);
            }
        }


        #endregion

        #region Events

        private void tMotion_SelectedIndexChanged(object sender, EventArgs e)
        {
            pMotionIllustration.BackgroundImage = (tMotion.SelectedIndex == 0) ? EFEM.GUIControls.Properties.Resources.Gets_motion : EFEM.GUIControls.Properties.Resources.Puts_motion;
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            GUIBasic.Instance().ProgressAbortEvent += new GUIBasic.ProgressAbort(TeachMotionConditionCtrl_ProgressAbortEvent);
            _ProgressKey = GUIBasic.Instance().ProgressStart("To apply motion condition to robot controller", string.Format("Controller is writing..."), 0, true, false);
            ArrayList list = new ArrayList();
            list.Add(lnbUOFF.Value);
            list.Add(lnbLOFF.Value);
            list.Add(lnbGOFF.Value);
            list.Add(lnbGCNF.Value);
            list.Add(lnbPADJ.Value);
            list.Add(lnbPOFF.Value);
            list.Add(lnbPCNF.Value);
            ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_Apply), list);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            GUIBasic.Instance().ProgressAbortEvent += new GUIBasic.ProgressAbort(TeachMotionConditionCtrl_ProgressAbortEvent);
            _ProgressKey = GUIBasic.Instance().ProgressStart("To refresh motion condition from robot controller", string.Format("Controller is reading..."), 0, true, false);
            ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_Refresh), null);
        }

        private void TeachMotionConditionCtrl_ProgressAbortEvent()
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
