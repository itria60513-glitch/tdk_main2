using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFEM.GUIControls.GeneralControls.Models
{

    public enum NodeViewKind
    {
        None,
        Comm,
        Log,
        DIO,
        LoadPort,
        N2Nozzle
    }

    public class ViewRequestEventArgs : EventArgs
    {
        public NodeViewKind Kind { get; }
        public object Context { get; }

        public ViewRequestEventArgs(NodeViewKind kind, object context = null)
        {
            Kind = kind;
            Context = context;
        }
    }

    public interface IRefreshable
    {
        void RefreshData(object context);
    }

}
