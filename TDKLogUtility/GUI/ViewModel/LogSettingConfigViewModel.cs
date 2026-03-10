using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TDKLogUtility.GUI.ViewModels;

namespace TDKController.GUI.ViewModels
{
    class LogSettingConfigViewModel : ViewModelBase
    {
        public LogSettingConfigViewModel(SynchronizationContext ctx = null) : base(ctx)
        {

        }

        private string _directory;
        public string MainDirectory
        {
            get => _directory;
            set => SetProperty(ref _directory, value);
        }

        private int _bufferSize;
        public int BufferSize
        {
            get => _bufferSize;
            set => SetProperty(ref _bufferSize, value);
        }

        private int _flushPeriod;
        public int AutoFlushPeriod
        {
            get => _flushPeriod;
            set => SetProperty(ref _flushPeriod, value);
        }

        private int _keepingDays;
        public int LogKeepingDays
        {
            get => _keepingDays;
            set => SetProperty(ref _keepingDays, value);
        }

        private int _maxStorage;
        public int MaxStorage
        {
            get => _maxStorage;
            set => SetProperty(ref _maxStorage, value);
        }



    }
}
