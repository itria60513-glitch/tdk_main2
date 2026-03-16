using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using EFEM.DataCenter;
using EFEM.ExceptionManagements;
using EFEM.LogUtilities;


namespace EFEM.GUIControls
{
    public partial class RobotAlarmFormCtrl : UserControl
    {
        //public event ExceptionEvent OnRobotExceptionEvent;
        
        private DataTable dtRobotAlarmDataSource;

        public RobotAlarmFormCtrl()
        {
            InitializeComponent();
        }  

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            UpdateIsChecking(true);

            bool bReGetAll = true;//[True:Re-Get all error history],[False:only get latest error]

            ThreadPool.QueueUserWorkItem(new WaitCallback(TPool_UpdateApply), (object)bReGetAll);
        }

        private void TPool_UpdateApply(object bReGetAll)
        {
            try
            {
                DataTable_Update((bool)bReGetAll);
            }
            catch (Exception)
            {
         
            }
            finally
            {
                UpdateIsChecking(false);
            }
        }

         private void UpdateIsChecking(bool bInProgress)
         {
             if (this.InvokeRequired)
             {
                 MethodInvoker del = delegate { UpdateIsChecking(bInProgress); };
                 this.Invoke(del);
             }
             else
             {
                 btnUpdate.Enabled = !bInProgress;
             }
         }

         void RobotAlarmFormCtrl_OnRobotAlarmUpdate(bool bReGetAllErrList)
         {
             //only need to get latest error information.             
             DataTable_Update(bReGetAllErrList);
         }

         #region Data Table
         private DataTable DataTable_Initialize(string TableName)
        {
            DataTable dtData = new DataTable(TableName);

            dtData.Columns.AddRange(new DataColumn[] {                    
                    new DataColumn("DateTime", typeof(System.String)),
                    new DataColumn("ErrorCode", typeof(System.String)),
                    new DataColumn("Message", typeof(System.String)),                   
                });

            return dtData;
        }

        private void DataTable_BindingData(DataTable CalDataTable)
        {
            if (CalDataTable == null)
            {
                GUIBasic.Instance().ShowMessageOnTop("Incorrect DataTable");
                return;
            }

            dgRobotAlarm.DataSource = null;

            dtRobotAlarmDataSource = CalDataTable;

            if (dgRobotAlarm.Tag == null)
            {
                dgRobotAlarm.Tag = true;
                dgRobotAlarm.Columns.Clear();
            }
            
            dgRobotAlarm.DataSource = CalDataTable;
            dgRobotAlarm.StretchLastColumn();
        }

        public void DataTable_Update(bool reGetAll = true)
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { DataTable_Update(reGetAll); };
                this.Invoke(del);
            }
            else
            {
                try
                {
                    DataTable_ClearData();                                    
                }
                catch (Exception ex)
                {
                    GUIBasic.Instance().WriteLog(LogUtilities.LogHeadType.Exception, "Exception occurred in [RobotAlarm]DataTable_Update. Reason: " + ex.Message + ex.StackTrace);
                    GUIBasic.Instance().ShowMessageOnTop("[RobotAlarm]DataTable_Update Cause Exception ! " + ex.Message + ex.StackTrace);
                }
                finally
                {
                    //dgRobotAlarm.AutoResizeColumns(); 
                    dgRobotAlarm.PerformLayout();
                }

            }

        }

        private void DataTable_ClearData()
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { DataTable_ClearData(); };
                this.Invoke(del);
            }
            else
            {
                dtRobotAlarmDataSource.Clear();               
            }
        }
        #endregion


    }
}
