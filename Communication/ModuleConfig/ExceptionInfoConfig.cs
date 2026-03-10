using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communication.Config
{
    internal class ExceptionInfoConfig
    {
    }

    public class ExceptionInfoConfigTcpip
    {
        public string Key { get; set; } = "";
        public ushort Id { get; set; }
        public string Message { get; set; } = "";
        public ushort Category { get; set; }
        public int Mode { get; set; }
        public string Remedy { get; set; } = "";
        public int MapErrId { get; set; }
        public int ErrId { get; set; }

        public override string ToString()
            => $"[{Id}] {Key} | Cat={Category}, Mode={Mode} | {Message} | Remedy={Remedy} | MapErrId={MapErrId}, ErrId={ErrId}";
    }

}
