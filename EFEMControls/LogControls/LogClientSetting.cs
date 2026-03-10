using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml.Linq;

namespace EFEM.GUIControls
{
    public partial class LogClientSetting : UserControl
    {        

        private DataSet dsReadLogObject;
        private DataTable dtLogListDataSource;

        private DataGridViewCheckBoxColumn dgvcbIsActive;
        private DataGridViewTextBoxColumn dgvcbLogFileName;

        public LogClientSetting()
        {
            InitializeComponent();
            InitializeGUIControl();
        }
        
        public void InitializeGUIControl()
        {

            dgvcbLogFileName = new DataGridViewTextBoxColumn();
            dgvcbLogFileName.HeaderText = "Log File Name";
            dgvcbLogFileName.Name = "FileName";
            dgvcbLogFileName.ReadOnly = true;

            dgvcbIsActive = new DataGridViewCheckBoxColumn();
            dgvcbIsActive.HeaderText = "Is Active";
            dgvcbIsActive.Name = "IsActive";
            dgvcbIsActive.FalseValue = "false";            
            dgvcbIsActive.IndeterminateValue = "";            
            dgvcbIsActive.TrueValue = "true";
            dgvcbIsActive.ReadOnly = false;
            
            dgLogList.Columns.Clear();

            dgLogList.Columns.AddRange(new DataGridViewColumn[] {
                    dgvcbLogFileName,
                    dgvcbIsActive            
                });
            
            DataTable dtData = new DataTable("LogObject");
            dtData.Columns.AddRange(new DataColumn[] {                                        
                    new DataColumn("File Name", typeof(String)),
                    new DataColumn("Is Active", typeof(bool)),
                });

            DataTable_BindingData(dtData);
            DataTable_Update();
        }

        private void DataTable_BindingData(DataTable CalDataTable)
        {
            if (CalDataTable == null)
            {
                GUIBasic.Instance().ShowMessageOnTop("Incorrect DataTable");
                return;
            }

            dgLogList.DataSource = null;

            dtLogListDataSource = CalDataTable;

            if (dgLogList.Tag == null)
            {
                dgLogList.Tag = true;
                dgLogList.Columns.Clear();
            }

            dgLogList.DataSource = dtLogListDataSource;

        }  

        /// <summary>
        /// Update DataSource data from LogObject.xml
        /// </summary>
        public void DataTable_Update()
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { DataTable_Update(); };
                this.Invoke(del);
            }
            else
            {

                try
                {

                    XDocument xdoc = GUIBasic.Instance().Log.GetLogListXDocument();
                    dsReadLogObject = new DataSet();
                    dsReadLogObject.ReadXml(xdoc.CreateReader());

                    DataTable_ClearData();

                    if (dsReadLogObject.Tables != null && dsReadLogObject.Tables.Count > 0)
                    {
                        foreach (DataRow dr in dsReadLogObject.Tables[0].Rows)
                        {

                            DataRow drData = dtLogListDataSource.NewRow();
                            drData[0] = dr[0]; // FileBaseName
                            drData[1] = (string.Compare(dr[1].ToString(), "true", true) == 0) ? "true" : "false";//IsActive

                            dtLogListDataSource.Rows.Add(drData);
                        }
                    }

                }
                catch (Exception ex)
                {
                    GUIBasic.Instance().WriteLog(LogUtilities.LogHeadType.Exception, "Exception occurred in [LogClientSetting]DataTable_Update. Reason: " + ex.Message + ex.StackTrace);

                }
                finally
                {
                    dgLogList.PerformLayout();
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
                if (dtLogListDataSource != null)
                    dtLogListDataSource.Clear();
            }
        }

        /// <summary>
        /// Convert current DataSource to LogObject.xsd DataTable format.
        /// </summary>
        private void DataTable_ConvertRowsData()
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { DataTable_ConvertRowsData(); };
                this.Invoke(del);
            }
            else
            {
                try
                {
                    if (dsReadLogObject.Tables.Count > 0)
                    {
                        //1.Clear whole RowsData in DataTable[0]
                        dsReadLogObject.Tables[0].Rows.Clear();

                        //2.Insert each current DataSource Rows into dsReadLogObject DataSet.(Later will use DataSet to WriteXml)
                        foreach (DataRow item in dtLogListDataSource.Rows)
                        {

                            DataRow drData = dsReadLogObject.Tables[0].NewRow();
                            drData[0] = item[0].ToString();// FileBaseName
                            drData[1] = string.Compare(item[1].ToString(), "true", true) == 0 ? "true" : "false";//IsActive

                            dsReadLogObject.Tables[0].Rows.Add(drData);
                        }
                    }
                }
                catch (Exception ex)
                {
                    GUIBasic.Instance().WriteLog(LogUtilities.LogHeadType.Exception, "Exception occurred in [LogClientSetting]DataTable_ConvertRowsData. Reason: " + ex.Message + ex.StackTrace);

                }
                finally
                {
                }
            }
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            DataTable_ApplyNewSetting();
        }
        private void DataTable_ApplyNewSetting()
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { DataTable_ApplyNewSetting(); };
                this.Invoke(del);
            }
            else
            {
                string ShowMsg = null;

                try
                {
                    btnApply.Enabled = false;

                    StringWriter sw = new StringWriter();

                    DataTable_ConvertRowsData();

                    dsReadLogObject.WriteXml(sw);

                    XDocument xdoc = XDocument.Parse(sw.ToString());
                    string strResult = GUIBasic.Instance().Log.SetLogListXDocument(xdoc);

                    if (strResult != null)
                    {
                        ShowMsg = string.Format("[Apply Setting Failed!!!]");                        
                        lbShowMsg.BackColor = Color.LightPink;

                        GUIBasic.Instance().WriteLog(LogUtilities.LogHeadType.Exception, "Exception occurred in [LogClientSetting]SetLogListXDocument. Reason: " + strResult);
                    }
                    else
                    {
                        ShowMsg = string.Format("[Apply Setting Successful!!!]");
                        lbShowMsg.BackColor = Color.SkyBlue;
                    }
                }
                catch (Exception ex)
                {
                    GUIBasic.Instance().WriteLog(LogUtilities.LogHeadType.Exception, "Exception occurred in [LogClientSetting]DataTable_ApplyNewSetting. Reason: " + ex.Message + ex.StackTrace);

                }
                finally
                {
                    lbShowMsg.Text = ShowMsg;
                    lbShowMsg.Visible = true;

                    btnApply.Enabled = true;
                }
            }
        }            

        private void btnReload_Click(object sender, EventArgs e)
        {

            DataTable_ReloadSetting();

        }

        private void DataTable_ReloadSetting()
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { DataTable_ReloadSetting(); };
                this.Invoke(del);
            }
            else
            {
                string ShowMsg = null;

                try
                {
                    btnReload.Enabled= false;
                    DataTable_Update();

                    if (dtLogListDataSource.Rows.Count > 0)
                    {
                        ShowMsg = string.Format("[Reload Setting Completed]");
                        lbShowMsg.BackColor = Color.SkyBlue;                        
                    }
                    else
                    {
                        ShowMsg = string.Format("[Reload Setting Failed]");
                        lbShowMsg.BackColor = Color.LightPink;

                        GUIBasic.Instance().WriteLog(LogUtilities.LogHeadType.Exception, "Exception occurred in [LogClientSetting]DataTable_ReloadSetting. Reason: " + "dtLogListDataSource.Rows == 0");
                    }
                    
                }
                catch (Exception ex)
                {
                    GUIBasic.Instance().WriteLog(LogUtilities.LogHeadType.Exception, "Exception occurred in [LogClientSetting]DataTable_ReloadSetting. Reason: " + ex.Message + ex.StackTrace);

                }
                finally
                {
                    lbShowMsg.Text = ShowMsg;
                    lbShowMsg.Visible = true;

                    btnReload.Enabled = true;
                }
            }
        }

        #region For Reserve

        public void InitializeGUIControl_XML()
        {

            XDocument xdoc = GUIBasic.Instance().Log.GetLogListXDocument();
            dsReadLogObject = new DataSet();
            dsReadLogObject.ReadXml(xdoc.CreateReader());

            DataTable_BindingData(dsReadLogObject.Tables[0]);

        }

        

        #endregion

        


    }
}
