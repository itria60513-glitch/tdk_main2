using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDKLogUtility.Module_Config
{

    public sealed class TdkLogConfig
    {
        public string HmiVer { get; set; }

        public string MainDirectory { get; set; } = string.Empty;
        public int BufferSize { get; set; } = 8192;
        public int LogDeleteBufferDays { get; set; } = 30;
        public int AutoFlushPeriod { get; set; } = 10;
        public int LevelControl { get; set; } = 5;
        public int MaxLogSize { get; set; } = 30;

        public string LogProcessGroupName { get; set; }
        public List<LogNameEntry> LogNames { get; } = new List<LogNameEntry>();
    }

    public sealed class LogNameEntry
    {
        public string Process { get; set; } = string.Empty;
        public string Remark { get; set; }
    }

}
