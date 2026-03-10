using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DIO.GUI.ViewModels
{
    class DIOSettingGuiViewModel : ViewModelBase
    {
        public DIOSettingGuiViewModel(SynchronizationContext ctx = null) : base(ctx)
        {

        }

        private int _type;
        public int Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        private string _index;
        public string Index
        {
            get => _index;
            set => SetProperty(ref _index, value);
        }

        private int _doMaxPort;
        public int DOMaxPort
        {
            get => _doMaxPort;
            set => SetProperty(ref _doMaxPort, value);
        }

        private int _diMaxPort;
        public int DIMaxPort
        {
            get => _diMaxPort;
            set => SetProperty(ref _diMaxPort, value);
        }

        private int _pinCountPerPort;

        public int PINCountPerPort
        {
            get => _pinCountPerPort;
            set => SetProperty(ref _pinCountPerPort, value);
        }


    }
}
