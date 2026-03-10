using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDKTestSocketServer.Config
{
    internal class TDKTestServerConfig
    {
        public string Port { get; set; }
        public List<Tuple<string, string>> Command { get; set; }

        public string StatusMessage { get; set; } = "";

    }
}
