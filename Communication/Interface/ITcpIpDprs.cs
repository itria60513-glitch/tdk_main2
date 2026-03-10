using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Communication.Interface
{
    public interface ITcpIpDprs
    {

        #region Socket
        Socket Handle { set; get; }

        bool Connected_Socket { get; }

        void New_Socket();

        void Bind_Socket(IPEndPoint ipEndPoint);

        void Connect_Socket(IPEndPoint ipEndPoint);

        void BeginReceive_Socket(Socket handle, byte[] buffer,
            int offset,
            int size,
            SocketFlags socketFlags,
            AsyncCallback callback,
            object state);

        int Send_Socket(byte[] buffer, int size, SocketFlags socketFlags);
        #endregion

    }
}
