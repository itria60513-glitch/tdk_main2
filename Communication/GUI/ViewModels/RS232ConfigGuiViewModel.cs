using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Communication.GUI.ViewModels
{
    class RS232ConfigGuiViewModel : ViewModelBase
    {
        public RS232ConfigGuiViewModel(SynchronizationContext ctx = null) : base(ctx)
        {

        }

        private string _portNumber;
        public string PortNumber
        {
            get => _portNumber;
            set => SetProperty(ref _portNumber, value);
        }

        private string _baudRate;
        public string BaudRate
        {
            get => _baudRate;
            set => SetProperty(ref _baudRate, value);
        }

        private string _stopBits;
        public string StopBits
        {
            get => _stopBits;
            set => SetProperty(ref _stopBits, value);
        }

        private string _dataBits;
        public string DataBits
        {
            get => _dataBits;
            set => SetProperty(ref _dataBits, value);
        }

        private int _parity;
        public int Parity
        {
            get => _parity;
            set => SetProperty(ref _parity, value);
        }
    }
}
