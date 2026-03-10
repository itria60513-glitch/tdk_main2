using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EFEM.GUIControls
{
    public partial class InitStatusErrorListForm : Form
    {
        public InitStatusErrorListForm()
        {
            InitializeComponent();
        }

        public bool AssignData(string caption, string errorList)
        {
            lCaption.Text = caption;
            dataGridViewError.Rows.Clear();

            if (string.IsNullOrWhiteSpace(errorList))
                return false;

            string[] errorItems = errorList.Split(';');

            if (errorItems.Length == 0)
                return false;
            else
            {
                foreach (string item in errorItems)
                {
                    int index = dataGridViewError.Rows.Add();
                    dataGridViewError.Rows[index].Cells["ID"].Value = (index + 1).ToString();
                    dataGridViewError.Rows[index].Cells["ErrorMsg"].Value = item.Trim();
                }

                return true;
            }
        }
    }
}
