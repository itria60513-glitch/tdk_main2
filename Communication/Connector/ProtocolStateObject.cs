using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Communication.Connector
{
    public class ProtocolStateObject
    {
        public ProtocolStateObject(int buffersize)
        {
            if (buffersize < 1024)
                buffersize = 1024;
            BufferSize = buffersize;
            buffer = new byte[BufferSize];
        }

        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public int BufferSize;
        // Receive buffer.
        public byte[] buffer;
    }
}
