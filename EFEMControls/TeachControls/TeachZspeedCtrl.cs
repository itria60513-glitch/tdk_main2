using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using EFEMInterface;
using EFEM.ExceptionManagements;

namespace EFEM.GUIControls.TeachControls
{
    public partial class TeachZspeedCtrl : UserControl
    {

        #region Fields and Properties

        private EFEMStation _station;
        public EFEMStation CurrentStation
        {
            get { return _station; }
            set { _station = value; }
        }

        private string _ProgressKey = string.Empty;

        #endregion

        #region Constructor

         public TeachZspeedCtrl()
        {
            InitializeComponent();
            IniTooltip();
        }

        #endregion

        #region Methods

         private void IniTooltip()
         {
             string sInfo;

             //Z axis ascent rate, acceleration/deceleration speed to taught height when getting wafer 
             ToolTip ttInfo_ZascSpd = new ToolTip();
             ttInfo_ZascSpd.ToolTipTitle = "Z axis ascent rate";
             sInfo = " Acceleration/deceleration speed to taught height when getting wafer";
             ttInfo_ZascSpd.SetToolTip(btnInfo_ZascSpd, sInfo);

             //Z axis drop rate, acceleration/deceleration speed to taught height when putting wafer
             ToolTip ttInfo_ZdropSpd = new ToolTip();
             ttInfo_ZdropSpd.ToolTipTitle = "Z axis drop rate";
             sInfo = " Acceleration/deceleration speed to taught height when putting wafer";
             ttInfo_ZdropSpd.SetToolTip(btnInfo_ZdropSpd, sInfo);

         }

         private void TPool_Apply(object parameters)
         {
             try
             {
                 ArrayList list = (ArrayList)parameters;
                 int SPDZU = (int)list[0];
                 int SPDZD = (int)list[1];
                 
                 //Z axis ascent rate, acceleration/deceleration speed to taught height when getting wafer
                 HRESULT hr = GUIBasic.Instance().MachineControl.SetStationParameter(_station, "SPDZU1", SPDZU);
                 if (hr != null)
                 {
                     GUIBasic.Instance().ProgressException("Fail to set Z axis ascent rate.\n  -->Reason: " + hr._message);
                 }
                 //Z axis drop rate, acceleration/deceleration speed to taught height when putting wafer
                 hr = GUIBasic.Instance().MachineControl.SetStationParameter(_station, "SPDZD1", SPDZD);
                 if (hr != null)
                 {
                     GUIBasic.Instance().ProgressException("Fail to set Z axis drop rate.\n  -->Reason: " + hr._message);
                 }
             }
             catch (Exception e)
             {
                 GUIBasic.Instance().ProgressException("Exception occured during applying seepd. \n  -->Reason: " + e.Message);
             }
             finally
             {
                 GUIBasic.Instance().ProgressAbortEvent -= new GUIBasic.ProgressAbort(TeachZSpeedCtrl_ProgressAbortEvent);
                 GUIBasic.Instance().ProgressEnd(_ProgressKey);
             }

         }

         private void TPool_Refresh(object data)
         {
             try
             {
                 int SPDZU = 0;                     //Z axis ascent rate, acceleration/deceleration speed to taught height when getting wafer
                 int SPDZD = 0;                     //Z axis drop rate, acceleration/deceleration speed to taught height when putting wafer

                 //Z axis ascent rate, acceleration/deceleration speed to taught height when getting wafer
                 HRESULT hr = GUIBasic.Instance().MachineControl.GetStationParameter(_station, "SPDZU1", ref SPDZU);
                 if (hr != null)
                 {
                     GUIBasic.Instance().ProgressException("Fail to get Z axis ascent rate.\n  -->Reason: " + hr._message);
                 }
                 this.Invoke(new MethodInvoker(delegate { lnbSPDZU.Value = Convert.ToInt16(SPDZU); }));


                 //Z axis drop rate, acceleration/deceleration speed to taught height when getting wafer
                 hr = GUIBasic.Instance().MachineControl.GetStationParameter(_station, "SPDZD1", ref SPDZD);
                 if (hr != null)
                 {
                     GUIBasic.Instance().ProgressException("Fail to get Z axis drop rate.\n  -->Reason: " + hr._message);
                 }
                 this.Invoke(new MethodInvoker(delegate { lnbSPDZD.Value = Convert.ToInt16(SPDZD); }));

             }
             catch (Exception e)
             {
                 GUIBasic.Instance().ProgressException("Exception occured during refreshing seepd.\n  -->Reason: " + e.Message);
             }
             finally
             {
                 GUIBasic.Instance().ProgressAbortEvent -= new GUIBasic.ProgressAbort(TeachZSpeedCtrl_ProgressAbortEvent);
                 GUIBasic.Instance().ProgressEnd(_ProgressKey);
             }
         }

        #endregion

        #region Events

         private void btnApply_Click(object sender, EventArgs e)
         {
             GUIBasic.Instance().ProgressAbortEvent += new GUIBasic.ProgressAbort(TeachZSpeedCtrl_ProgressAbortEvent);
             _ProgressKey = GUIBasic.Instance().ProgressStart("To apply motion speed to robot controller", string.Format("Controller is writing..."), 0, true, false);
             ArrayList list = new ArrayList();
             list.Add(lnbSPDZU.Value);
             list.Add(lnbSPDZD.Value);
             ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_Apply), list);
         }

         private void btnRefresh_Click(object sender, EventArgs e)
         {
             GUIBasic.Instance().ProgressAbortEvent += new GUIBasic.ProgressAbort(TeachZSpeedCtrl_ProgressAbortEvent);
             _ProgressKey = GUIBasic.Instance().ProgressStart("To get motion speed  from robot controller", string.Format("Controller is reading..."), 0, true, false);
             ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_Refresh), null);
         }

         private void TeachZSpeedCtrl_ProgressAbortEvent()
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
