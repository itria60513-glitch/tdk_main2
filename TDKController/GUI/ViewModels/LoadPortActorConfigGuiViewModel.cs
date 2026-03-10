using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TDKController.GUI.ViewModels
{
    class LoadPortActorConfigGuiViewModel : ViewModelBase
    {
        public LoadPortActorConfigGuiViewModel(SynchronizationContext ctx = null) : base(ctx)
        {

        }

        private string _comm;
        public string Comm
        {
            get => _comm;
            set => SetProperty(ref _comm, value);
        }

        private int _infTimeOuts;
        public int INFTimeouts
        {
            get => _infTimeOuts;
            set => SetProperty(ref _infTimeOuts, value);
        }

        private int _ackTimeouts;
        public int ACKTimeouts
        {
            get => _ackTimeouts;
            set => SetProperty(ref _ackTimeouts, value);
        }


    }
}
