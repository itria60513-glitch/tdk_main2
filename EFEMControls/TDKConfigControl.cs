using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Communication.GUI;
using EFEM.FileUtilities;
using EFEM.GUIControls.GeneralControls.Models;
using Microsoft.VisualBasic.ApplicationServices;
using DIO.GUI;
using TDKController.GUI;
using TDKLogUtility.GUI;

namespace EFEM.GUIControls
{
    public partial class TDKConfigControl : UserControl
    {
        private UserControl _currentView;
        private List<(string, int)> _commList = new List<(string, int)>();
        private List<string> _loadPortList = new List<string>();
        private List<string> _dioList = new List<string>();
        private List<string> _n2NozzleList = new List<string>();
        private List<(string, UserControl)> _userControlList = new List<(string, UserControl)>();

        public TDKConfigControl()
        {
            InitializeComponent();

            try
            {

                treeView1.ViewRequested += Nav_ViewRequested;
                AbstractFileUtilities _fu = FileUtility.GetUniqueInstance();
                _commList = _fu.GetCommList();
                _loadPortList = _fu.GetLoadPortList();
                _dioList = _fu.GetDIOList();
                _n2NozzleList = _fu.GetN2NozzleList();

            }

            catch (Exception)
            {

            }


        }

        public void InitAll()
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { InitAll(); };
                this.Invoke(del);
            }
            else
            {

            }
        }

        private void Nav_ViewRequested(object sender, ViewRequestEventArgs e)
        {
            ShowView(e.Kind, e.Context);
        }

        private void ShowView(NodeViewKind kind, object context)
        {
            if(_currentView != null)
                _currentView.Visible = false;
            UserControl newView;
            newView = CreateByRule(kind, context);
            if (kind != NodeViewKind.None)
            {
                newView.Visible = true;
                var r = newView as IRefreshable;
                if (r != null)
                    r.RefreshData(context);
            }

            _currentView = newView;
        }


        private UserControl CreateByRule(NodeViewKind kind, object context)
        {
            var selection = context as string;
            var found = _userControlList.FirstOrDefault(x => x.Item1.Equals(selection));
            if (found.Item1 != null)
            {
                return found.Item2;
            }
            else
            {
                UserControl newView;
                switch (kind)
                {
                    case NodeViewKind.Comm:
                        var _commName = _commList.FirstOrDefault(x => x.Item1.Equals(selection));
                        if (_commName.Item2 == 1)
                        {
                            newView = new TCPIPSettingGUI(selection);
                        }
                        else
                        {
                            newView = new RS232SettingGUI(selection);
                        }
                        break;
                    case NodeViewKind.Log:
                        newView = new LogSettingGUI();
                        break;
                    case NodeViewKind.DIO:
                        newView = new DIOSettingGUI(selection);
                        break;
                    case NodeViewKind.LoadPort:
                        newView = new LoadPortSettingGUI(selection);
                        break;
                    case NodeViewKind.N2Nozzle:
                        newView = new N2NozzleSettingGUI(selection);
                        break;
                    default:
                        return null;
                }
                newView.Visible = false;
                newView.Dock = DockStyle.Fill;
                panel1.Controls.Add(newView);
                newView.BringToFront();
                _userControlList.Add((selection, newView));
                return newView;
            }
        }

    }
}
