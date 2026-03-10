using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using EFEM.DataCenter;
using EFEM.FileUtilities;
using EFEM.GUIControls.GeneralControls.Models;

namespace EFEM.GUIControls.GeneralControls
{
    public partial class TreeView : UserControl
    {

        public event EventHandler<ViewRequestEventArgs> ViewRequested;
        public TreeView()
        {
            InitializeComponent();
            Initialize();
            treeView1.AfterSelect += TreeView1_AfterSelect;

        }

        private void Initialize()
        {
            AbstractFileUtilities _fu = FileUtility.GetUniqueInstance();
            List<(string, int)> commList = new List<(string, int)>();
            List<string> dioList = new List<string>();
            List<string> loadPortList = new List<string>();
            List<string> n2NozzleList = new List<string>();

            #region Tree Node Create
            var node = new TreeNode();
            var comm = new TreeNode("Communication") { Tag = new ViewRequestEventArgs(NodeViewKind.None) }; ;
            commList = _fu.CommunicationLoad();

            foreach (var child in commList)
            {
                node = new TreeNode(child.Item1) { Tag = new ViewRequestEventArgs(NodeViewKind.Comm) };
                comm.Nodes.Add(node);
            }

            treeView1.Nodes.Add(comm);

            var log = new TreeNode("Log") { Tag = new ViewRequestEventArgs(NodeViewKind.Log) };

            treeView1.Nodes.Add(log);

            var dio = new TreeNode("DIO") { Tag = new ViewRequestEventArgs(NodeViewKind.None) };
            dioList = _fu.DIOLoad();
            foreach (var child in dioList)
            {
                node = new TreeNode(child) { Tag = new ViewRequestEventArgs(NodeViewKind.DIO) };
                dio.Nodes.Add(node);
            }

            treeView1.Nodes.Add(dio);

            var loadPort = new TreeNode("LoadPort") { Tag = new ViewRequestEventArgs(NodeViewKind.None) };
            loadPortList = _fu.LoadPortActorLoad();
            foreach (var child in loadPortList)
            {
                node = new TreeNode(child) { Tag = new ViewRequestEventArgs(NodeViewKind.LoadPort) };
                loadPort.Nodes.Add(node);
            }
            treeView1.Nodes.Add(loadPort);

            var n2Nozzle = new TreeNode("N2Nozzle") { Tag = new ViewRequestEventArgs(NodeViewKind.None) };
            n2NozzleList = _fu.N2NozzleLoad();
            foreach (var child in n2NozzleList)
            {
                node = new TreeNode(child) { Tag = new ViewRequestEventArgs(NodeViewKind.N2Nozzle) };
                n2Nozzle.Nodes.Add(node);
            }
            treeView1.Nodes.Add(n2Nozzle);


            #endregion


        }




        private void TreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ViewRequestEventArgs args;

            if (e.Node != null && e.Node.Tag is ViewRequestEventArgs ve)
            {
                if (ve.Context == null)
                    args = new ViewRequestEventArgs(ve.Kind, e.Node.Text);
                else
                    args = ve;
            }
            else
            {
                args = new ViewRequestEventArgs(NodeViewKind.Comm, e.Node != null ? e.Node.Text : null);
            }

            var handler = ViewRequested;
            if (handler != null)
                handler(this, args);
        }



    }
}
