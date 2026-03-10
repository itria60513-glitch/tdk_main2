using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Communication.Interface;

namespace Communication.Protocol
{
    /// <summary>
    /// -The ConcreteStrategy implements the Algorithm using the Strategy interface.
    /// 
    /// 
    /// </summary>
    public class DefaultProtocol : IProtocol
    {
        public DefaultProtocol()
        {
            m_queue = new CircularQueue();
        }
        public int AddOutFrameInfo(ref byte[] byteArray, int intSize)
        {
            return intSize;
        }
        public int AddOutFrameInfoWithFakeHeader(ref byte[] byteArray, int intSize)
        {
            return intSize;
        }
        public void Purge()
        {
            m_queue.purge();
        }
        public int BufferSize
        {
            get
            {
                return m_queue.QueueSize;
            }
        }
        public int Push(byte[] byteArray, int intSize)
        {
            /*if(intSize == 0)
                return 0;
            int iter, iLength=0;
            for(iter=0; iter<intSize; iter++)
            {
                if(!m_queue.push_back(byteArray[iter]))
                    break;
                else
                    iLength++;
            }
            return iLength;*/
            return m_queue.push_array(byteArray, intSize);
        }
        public int Pop(ref byte[] byteArray)
        {
            int size = 0;
            while (m_queue.size != 0)
            {
                byteArray[size] = m_queue.pop_front();
                size++;
            }
            return size;
        }
        public (bool,byte[]) VerifyInFrameStructure(byte[] byteArray, int intSize)
        {
            return (true, byteArray);
        }
        public object DeviceCode
        {
            set
            {
                _DeviceCode = (char)value;
            }
        }
        public object HostCode
        {
            set
            {
                _HostCode = (char)value;
            }
        }
        private CircularQueue m_queue;
        private char _DeviceCode, _HostCode;

        #region Event Declarations
        public event LogEventHandler LoggingRequest;
        private void Fire_LoggingRequest(int category, string msg)
        {
            if (LoggingRequest != null)
                LoggingRequest(category, msg);
        }
        #endregion
    }
}
