using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Communication.Connector;
using Communication.Protocol;
using EFEM.FileUtilities;
using LogUtility;

namespace Communication.GUI
{
    internal class TcpHermosProtocolTest
    {
        public TcpHermosProtocolTest()
        {
            var hermosProtocol = new HermosProtocol();
            var config = new TCPConnectorConfig(new TCPConfig("8.8.8.8", "80"));
            var log = new LogUtilityClient();
            var tcp = new TcpipConnector(hermosProtocol, config, log);
            tcp.Connect();
        }
    }
}
