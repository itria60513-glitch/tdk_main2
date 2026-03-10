using Communication.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Communication.Connector
{
    public class TcpIpDprs : ITcpIpDprs
    {

        public Socket Handle { get; set; }

        public bool Connected_Socket => Handle.Connected;

        public void New_Socket()
        {
            Handle = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Handle.NoDelay = true;
        }

        public void Bind_Socket(IPEndPoint ipEndPoint)
        {
            Handle.Bind(ipEndPoint);
        }

        public void Connect_Socket(IPEndPoint ipEndPoint)
        {
            Handle.Connect(ipEndPoint);
        }

        public void BeginReceive_Socket(Socket handle, byte[] buffer, int offset, int size, SocketFlags socketFlags,
            AsyncCallback callback, object state)
        {
            handle.BeginReceive(buffer, offset, size, socketFlags, callback, state);
        }

        public int Send_Socket(byte[] buffer, int size, SocketFlags socketFlags)
        {
            return Handle.Send(buffer, size, socketFlags);
        }

    }
}
