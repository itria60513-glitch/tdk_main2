using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace EFEM.GUIControls
{
    public partial class StatusIndication : UserControl
    {
        public EventHandler OnUserClick;
        private StatusType _status = StatusType.None;
        private ToolTip m_toolTip = null;

        public StatusIndication()
        {
            InitializeComponent();
            Status = StatusType.None;
        }

        public StatusType Status
        {
            set
            {
                switch (value)
                {
                    case StatusType.Red:
                    case StatusType.Yellow:
                    case StatusType.Green:
                    case StatusType.Bule:
                    case StatusType.Stop:
                    case StatusType.Check:
                    case StatusType.Error:
                        {
                            this._status = value;
                            panelImage.BackgroundImage = imageList.Images[(int)value];
                            panelImage.Visible = true;
                            break;
                        }
                    default:
                        {
                            this._status = StatusType.None;
                            panelImage.Visible = false;
                            break;
                        }
                }
            }
            get
            {
                return this._status;
            }
        }

        public override string Text
        {
            get { return labelCaption.Text; }
            set
            {
                labelCaption.Text = value;
            }
        }

        public void SetToopTipMsg(string msg, ToolTipIcon icon)
        {
            if (string.IsNullOrWhiteSpace(msg))
            {
                if (m_toolTip != null)
                {
                    m_toolTip.ToolTipTitle = null;
                    m_toolTip.SetToolTip(labelCaption, null);
                    m_toolTip.SetToolTip(panelImage, null);
                }
            }
            else
            {
                if (m_toolTip == null)
                    m_toolTip = new ToolTip();

                m_toolTip.ToolTipTitle = Text;
                m_toolTip.ToolTipIcon = icon;
                m_toolTip.SetToolTip(labelCaption, msg);
                m_toolTip.SetToolTip(panelImage, msg);
            }
        }

        void panelImage_DoubleClick(object sender, EventArgs e)
        {
            if (OnUserClick != null)
                OnUserClick(sender, e);
        }

        private void labelCaption_DoubleClick(object sender, EventArgs e)
        {
            if (OnUserClick != null)
                OnUserClick(sender, e);
        }
    }

    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
    public class ToolStripStatusIndication : ToolStripControlHost
    {
        public ToolStripStatusIndication() : base(new StatusIndication()) { }

        public void AddOnUserClickEvent(EventHandler userEvent)
        {
            StatusIndicationControl.OnUserClick += userEvent;
        }

        public void RemoveOnUserClickEvent(EventHandler userEvent)
        {
            StatusIndicationControl.OnUserClick -= userEvent;
        }

        public StatusIndication StatusIndicationControl
        {
            get
            {
                return Control as StatusIndication;
            }
        }

        public StatusType Status
        {
            get { return StatusIndicationControl.Status; }
            set { StatusIndicationControl.Status = value; }
        }

        public override string Text
        {
            get { return StatusIndicationControl.Text; }
            set
            {
                StatusIndicationControl.Text = value;
            }
        }

        public void SetToopTipMsg(string msg, ToolTipIcon icon)
        {
            StatusIndicationControl.SetToopTipMsg(msg, icon);
        }
    }

    public enum StatusType
    {
        None = -1,
        Red = 0,
        Yellow = 1,
        Green = 2,
        Bule = 3,
        Stop = 4,
        Check = 5,
        Error = 6
    }

}
