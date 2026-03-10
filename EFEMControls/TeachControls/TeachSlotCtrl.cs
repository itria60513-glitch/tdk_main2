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
    public partial class TeachSlotCtrl : UserControl
    {
        #region Fields and Properties

        private EFEMStation _station;
        public EFEMStation CurrentStation
        {
            get { return _station; }
            set { _station = value; }
        }

        private int _Slot;
        private int _SlotDistance;

        private string _ProgressKey = string.Empty;

        #endregion

        #region Contructor

        public TeachSlotCtrl()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        private void TPool_Apply(object parameters)
        {
            try
            {
                ArrayList list = (ArrayList)parameters;
                int Slot = (int)list[0];
                int SlotDistance = (int)list[1];

                //HRESULT hr = GUIBasic.Instance().MachineControl.SetStationParameter(_station, "SLOT", Convert.ToInt16(lnbSlot.Value));
                HRESULT hr = GUIBasic.Instance().MachineControl.SetStationParameter(_station, "SLOT", Convert.ToInt16(Slot));
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to set number of slots in station.\n  -->Reason: " + hr._message);
                    return;
                }

                //hr = GUIBasic.Instance().MachineControl.SetStationParameter(_station, "lnbSDIS", Convert.ToInt16(lnbSDIS.Value));
                hr = GUIBasic.Instance().MachineControl.SetStationParameter(_station, "lnbSDIS", Convert.ToInt16(SlotDistance));
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to set slot interval distance.\n  -->Reason: " + hr._message);
                }
            }
            catch (Exception e)
            {
                GUIBasic.Instance().ProgressException("Exception occured during applying slot condition. \n  -->Reason: " + e.Message);
            }
            finally
            {
                GUIBasic.Instance().ProgressAbortEvent -= new GUIBasic.ProgressAbort(TeachSlotCtrl_ProgressAbortEvent);
                GUIBasic.Instance().ProgressEnd(_ProgressKey);
            }
        }

        private void TPool_Refresh(object parameters)
        {
            try
            {
                HRESULT hr = null;

                hr = GUIBasic.Instance().MachineControl.GetStationParameter(_station, "SLOT", ref _Slot);
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to get number of slots in station.\n  -->Reason: " + hr._message);
                    return;
                    //this.Invoke(new MethodInvoker(delegate { lnbSlot.Value = _Slot; })); //default value
                }
                else
                    this.Invoke(new MethodInvoker(delegate {lnbSlot.Value = _Slot;  }));
                    

                hr = GUIBasic.Instance().MachineControl.GetStationParameter(_station, "lnbSDIS", ref _SlotDistance);
                if (hr != null)
                {
                    GUIBasic.Instance().ProgressException("Fail to get slot interval distance.\n  -->Reason: " + hr._message);
                    //this.Invoke(new MethodInvoker(delegate { lnbSlot.Value = _SlotDistance; })); //default value
                }
                else
                    this.Invoke(new MethodInvoker(delegate { lnbSlot.Value = _SlotDistance; }));
               
            }
            catch (Exception e)
            {
                GUIBasic.Instance().ProgressException("Exception occured during refreshing slot condition. \n  -->Reason: " + e.Message);
            }
            finally
            {
                GUIBasic.Instance().ProgressAbortEvent -= new GUIBasic.ProgressAbort(TeachSlotCtrl_ProgressAbortEvent);
                GUIBasic.Instance().ProgressEnd(_ProgressKey);
            }
        }

        #endregion

        #region Events
 
        private void btnApply_Click(object sender, EventArgs e)
        {
            GUIBasic.Instance().ProgressAbortEvent += new GUIBasic.ProgressAbort(TeachSlotCtrl_ProgressAbortEvent);
            _ProgressKey = GUIBasic.Instance().ProgressStart("To apply motion condition to robot controller", string.Format("Controller is writing..."), 0, true, false);
            ArrayList list = new ArrayList();
            list.Add(lnbSlot.Value);
            list.Add(lnbSDIS.Value);
            ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_Apply), list);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            GUIBasic.Instance().ProgressAbortEvent += new GUIBasic.ProgressAbort(TeachSlotCtrl_ProgressAbortEvent);
            _ProgressKey = GUIBasic.Instance().ProgressStart("To refresh motion condition from robot controller", string.Format("Controller is reading..."), 0, true, false);
            ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_Refresh), null);
        }

        private void TeachSlotCtrl_ProgressAbortEvent()
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
