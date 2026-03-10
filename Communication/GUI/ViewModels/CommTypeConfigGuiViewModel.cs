using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Communication.GUI.ViewModels
{
    public class CommTypeOptionVM : ViewModelBase
    {
        public string Key { get; } 

        private string _choice;
        public string Choice
        {
            get => _choice;
            set => SetProperty(ref _choice, value);
        }

        private readonly IReadOnlyList<string> _available;
        public IReadOnlyList<string> Available => _available;

        public CommTypeOptionVM(string key,
            IEnumerable<string> available,
            string initial = null,
            SynchronizationContext ctx = null) : base(ctx)
        {
            Key = key ?? string.Empty;
            _available = (available ?? new[] { "TCPIP", "RS232" }).ToArray();

            _choice = string.IsNullOrWhiteSpace(initial)
                ? _available[0]
                : (_available.Contains(initial) ? initial : _available[0]);
        }
    }

    public class CommTypeConfigGuiViewModel : ViewModelBase
    {
        public CommTypeConfigGuiViewModel(SynchronizationContext ctx = null) : base(ctx) { }

        public BindingList<CommTypeOptionVM> Items { get; } = new BindingList<CommTypeOptionVM>();

        private string[] _options = new[] { "TCPIP", "RS232" };
        public IReadOnlyList<string> Options => _options;

        public void LoadFromConfig(IDictionary<string, string> config)
        {
            Items.RaiseListChangedEvents = false;
            Items.Clear();

            foreach (var kv in config)
            {
                Items.Add(new CommTypeOptionVM(kv.Key, _options, kv.Value, _ctx));
            }

            Items.RaiseListChangedEvents = true;
            Items.ResetBindings();
        }


        public Dictionary<string, string> ToConfig()
            => Items.ToDictionary(i => i.Key, i => i.Choice);
    }
}
