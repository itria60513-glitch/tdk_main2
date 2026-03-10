using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Communication.GUI.ViewModels
{
    class TCPConfigGuiViewModel : ViewModelBase
    {
        public TCPConfigGuiViewModel(SynchronizationContext ctx = null) : base(ctx)
        {

        }

        private string _ipAddress;
        public string IpAddress
        {
            get => _ipAddress;
            set => SetProperty(ref _ipAddress, value);
        }

        private string _port;
        public string Port
        {
            get => _port;
            set => SetProperty(ref _port, value);
        }

    }
}
