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
using EFEM.VariableCenter;

namespace EFEM.GUIControls
{
    public partial class StatusMessageCtrl : UserControl
    {
        private int maxLines = 200;
        private bool _isPausing = false;
        private List<ListViewItem> items = new List<ListViewItem>();
        private List<ListViewItem> queueItems = new List<ListViewItem>(); //Pausing by user
        public event EventHandler OnTitleDoubleClick = null;
        private bool _isSizeChanged = false;

        public delegate void delStatusBoxShow(bool isShow);
        public event delStatusBoxShow OnStatusBoxShowHide = null;

        private void NotifyStatusBoxShowHide(bool isShow)
        {
            delStatusBoxShow handler = OnStatusBoxShowHide;
            if (handler != null)
            {
                handler(isShow);
            }
        }

        public StatusMessageCtrl()
        {
            InitializeComponent();

            listStatusHistory.Items.Clear();
            listStatusHistory.Columns.Clear();
            listStatusHistory.View = View.Details;
            listStatusHistory.Columns.Add("DateTime");
            listStatusHistory.Columns.Add("Message");
            listStatusHistory.Columns[0].Width = 80;
            listStatusHistory.Columns[0].TextAlign = HorizontalAlignment.Right;
            listStatusHistory.Columns[1].Width = -2; //-2: Autosize
            listStatusHistory.HeaderStyle = ColumnHeaderStyle.None;
            listStatusHistory.SizeChanged += new EventHandler(listStatusHistory_SizeChanged);
        }

        void listStatusHistory_SizeChanged(object sender, EventArgs e)
        {
            //Resizing on runtime will crash causing the .Net bug in Virtual Mode
            if (!_isSizeChanged)
            {
                _isSizeChanged = true;
                listStatusHistory.Columns[1].Width = listStatusHistory.Width - System.Windows.Forms.SystemInformation.VerticalScrollBarWidth - 10 - listStatusHistory.Columns[0].Width;
            }
        }

        public void InitAll()
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { InitAll(); };
                this.Invoke(del);
            }
        }

        private string TimeStamp
        {
            get { return PreciseDatetime.Now.ToString(ConstVC.Display.StatusInfoDateTimeFormat); }
        }

        public bool IsStatusBoxShowing
        {
            get
            {
                return true;
            }
        }


        public string Caption
        {
            get { return lTitle.Text; }
            set { lTitle.Text = value; }
        }

        private object _lockPause = new object();
        public bool IsPausing
        {
            get
            {
                lock (_lockPause)
                {
                    return _isPausing;
                }
            }
            set
            {
                lock (_lockPause)
                {
                    if (_isPausing != value)
                    {
                        _isPausing = value;

                        if (!value)
                        {
                            //Force flush message in queue
                            AddStatusMessage(null, MsgType.NONE, false);
                        }
                    }
                }
            }
        }

        private ReaderWriterLock _itemLocker = new ReaderWriterLock();
        public void AddStatusMessage(string message, MsgType st = MsgType.INFO, bool isError = false)
        {
            _itemLocker.AcquireWriterLock(Timeout.Infinite);
            bool InvalidateNeed = false;
            int ensureIndex = 0;// items.Count - 1;

            try
            {
                List<ListViewItem> owner = _isPausing ? queueItems : items;

                if (IsPausing)
                {
                    if (queueItems.Count >= maxLines)
                        queueItems.RemoveAt(0);
                }
                else
                {
                    int totalCount = items.Count + queueItems.Count;
                    int NeedRemoveCount = totalCount - maxLines;

                    if (NeedRemoveCount > 0)
                    {
                        if (NeedRemoveCount > items.Count)
                            NeedRemoveCount = items.Count;

                        items.RemoveRange(0, NeedRemoveCount);
                        InvalidateNeed = true;
                    }

                    if (queueItems.Count > 0)
                    {
                        items.AddRange(queueItems);
                        queueItems.Clear();
                    }
                }

                if (!string.IsNullOrWhiteSpace(message))
                {
                    ListViewItem item;

                    if (st != MsgType.NONE)
                        item = new ListViewItem(new string[] { TimeStamp + " :", string.Format("[{0}] {1}", st, message) });
                    else
                        item = new ListViewItem(new string[] { TimeStamp + " :", message });

                    if (isError)
                        item.BackColor = Color.LightPink;
                    else if (st == MsgType.SEND || st == MsgType.STATUS)
                        item.BackColor = Color.LightSkyBlue;

                    owner.Add(item);
                }
            }
            finally
            {
                _itemLocker.ReleaseWriterLock();

                if (!IsPausing)
                {
                    ensureIndex = items.Count - 1;
                    UpdateDisplay(items.Count, ensureIndex, InvalidateNeed);
                    //listStatusHistory.Columns[1].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                }
            }
        }

        private void UpdateDisplay(int virtualSize, int ensureVisIndex, bool InvalidateNeed)
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { UpdateDisplay(virtualSize, ensureVisIndex, InvalidateNeed); };
                this.Invoke(del);
            }
            else
            {
                listStatusHistory.VirtualListSize = virtualSize;

                if (ensureVisIndex >= 0)
                    listStatusHistory.EnsureVisible(ensureVisIndex);

                if (InvalidateNeed)
                    listStatusHistory.Invalidate();
            }
        }

        private void SetVirtualSize(int size)
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { SetVirtualSize(size); };
                this.Invoke(del);
            }
            else
            {
                listStatusHistory.VirtualListSize = size;
            }
        }

        public void ClearAllMessage()
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { ClearAllMessage(); };
                this.Invoke(del);
            }
            else
            {
                //lock (_lockItem)
                _itemLocker.AcquireWriterLock(Timeout.Infinite);
                try
                {
                    SetVirtualSize(0);
                    items.Clear();
                    queueItems.Clear();
                }
                finally
                {
                    _itemLocker.ReleaseWriterLock();
                }
            }
        }

        public void CopyListViewToClipboard()
        {
            ListView lv = listStatusHistory;
            StringBuilder buffer = new StringBuilder();
            for (int i = 0; i < lv.Columns.Count; i++)
            {
                buffer.Append(lv.Columns[i].Text);
                buffer.Append("\t");
            }

            buffer.Append("\r\n");
            for (int i = 0; i < lv.Items.Count; i++)
            {
                for (int j = 0; j < lv.Columns.Count; j++)
                {
                    buffer.Append(lv.Items[i].SubItems[j].Text);
                    buffer.Append("\t");
                }
                buffer.Append("\r\n");
            }

            Clipboard.SetText(buffer.ToString());
        }

        private void cbPause_CheckedChanged(object sender, EventArgs e)
        {
            IsPausing = cbPause.Checked;           
        }

        private void copyAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyListViewToClipboard();
        }

        private void clearAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearAllMessage();
        }

        private const uint LVM_ISITEMVISIBLE = 0x1000 + 182;

        public bool ItemIsVisible(int itemIndex)
        {
            return (uint)NativeMethods.SendMessage(listStatusHistory.Handle, LVM_ISITEMVISIBLE, itemIndex, (IntPtr)0) != 0;
        }
        
        private readonly ListViewItem _blankListItem = new ListViewItem(new string[] { "NO INFO", "NO INFO" });
        private void listStatusHistory_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { listStatusHistory_RetrieveVirtualItem(sender, e); };
                this.Invoke(del);
            }
            else
            {
                _itemLocker.AcquireReaderLock(Timeout.Infinite);
                try
                {
                    //if (!ItemIsVisible(e.ItemIndex) || e.ItemIndex >= items.Count)
                    if (e.ItemIndex < 0 || e.ItemIndex >= items.Count)
                    {
                        e.Item = _blankListItem;
                        return;
                    }

                    var anItem = items[e.ItemIndex];
                    if (anItem == null)
                    {
                        e.Item = _blankListItem;
                        e.Item.BackColor = Color.Gray;
                    }
                    e.Item = anItem;
                }
                finally
                {
                    _itemLocker.ReleaseReaderLock();
                }
            }


            //int index = e.ItemIndex;
            //_itemLocker.AcquireReaderLock(Timeout.Infinite);
            //try
            //{
            //    if (index >= items.Count)
            //    {
            //        e.Item = new ListViewItem(new string[] { "NO INFO", "NO INFO" });
            //        e.Item.BackColor = Color.Gray;
            //    }
            //    else
            //        e.Item = items[index];
            //}
            //finally
            //{
            //    _itemLocker.ReleaseReaderLock();
            //}
        }

        private void lTitle_DoubleClick(object sender, EventArgs e)
        {
            if (OnTitleDoubleClick != null)
            {
                OnTitleDoubleClick(sender, e);
            }
        }

        private void lMonitor_DoubleClick(object sender, EventArgs e)
        {
            OnTitleDoubleClick(sender, e);
        }

        private void listStatusHistory_DoubleClick(object sender, EventArgs e)
        {
            listStatusHistory.Columns[1].Width = listStatusHistory.Width - System.Windows.Forms.SystemInformation.VerticalScrollBarWidth - 10 - listStatusHistory.Columns[0].Width;
        }
    }
}
