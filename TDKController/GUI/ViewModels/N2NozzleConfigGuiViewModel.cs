using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TDKController.GUI.ViewModels
{
    class N2NozzleConfigGuiViewModel : ViewModelBase
    {
        public N2NozzleConfigGuiViewModel(SynchronizationContext ctx = null) : base(ctx)
        {

        }

        private string _comm;
        public string Comm
        {
            get => _comm;
            set => SetProperty(ref _comm, value);
        }

    }
}
