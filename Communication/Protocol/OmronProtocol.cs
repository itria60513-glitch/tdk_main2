using Communication.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Communication.Protocol
{
    public class OmronProtocol : IProtocol
    {
        #region Private Data
        private CircularQueue m_queue;
        private object _monitor = new object();
        private int last_index = 0;
        private readonly int MAXINFOLEN = 40;
        #endregion Private Data

        #region Property
        public int BufferSize
        {
            get
            {
                return m_queue.QueueSize;
            }
        }
        #endregion Property

        #region Constructor
        public OmronProtocol()
        {
            m_queue = new CircularQueue();
        }
        #endregion Constructor

        #region Event Declarations
        public event LogEventHandler LoggingRequest;
        private void Fire_LoggingRequest(int category, string msg)
        {
            if (LoggingRequest != null)
                LoggingRequest(category, msg);
        }
        #endregion

        #region Public Method
        public int AddOutFrameInfo(ref byte[] byteArray, int intSize)
        {
            var len = byteArray.Length;
            var frame = new byte[intSize + 1];
            Buffer.BlockCopy(byteArray, 0, frame, 0, len);
            byteArray[intSize] = 0x0D;
            byteArray = frame;
            return intSize + 1;
        }
        public int AddOutFrameInfoWithFakeHeader(ref byte[] byteArray, int intSize)
        {
            var len = byteArray.Length;
            var frame = new byte[intSize + 1];
            Buffer.BlockCopy(byteArray, 0, frame, 0, len);
            byteArray[intSize] = 0x0C;
            byteArray = frame;
            return intSize + 1;
        }
        public void Purge()
        {
            Monitor.Enter(_monitor);
            try
            {
                m_queue.purge();
            }
            finally
            {
                Monitor.Exit(_monitor);
            }
        }

        public int Push(byte[] byteArray, int intSize)
        {
            Monitor.Enter(_monitor);
            try
            {
                return m_queue.push_array(byteArray, intSize);
            }
            finally
            {
                Monitor.Exit(_monitor);
            }
        }

        public int Pop(ref byte[] byteArray)
        {
            Monitor.Enter(_monitor);
            try
            {
                var queuesize = m_queue.size;
                if (queuesize < 1)
                    return 0;

                int size = 0;
                for (size = last_index; size < queuesize - 1; size++)
                {
                    if (m_queue.item(size) == 0x0D)
                    {
                        last_index = 0;
                        return m_queue.pop_array(ref byteArray, size + 1);
                    }
                }
                last_index = queuesize - 1;
                return 0;
            }
            finally
            {
                Monitor.Exit(_monitor);
            }
        }
        public (bool, byte[]) VerifyInFrameStructure(byte[] buffer, int size)
        {
            var len = buffer.Length;
            
            if (len<3 || buffer[len-1]!=0x0D)
                return (false, buffer);
            byte[] result = new byte[buffer.Length - 1];
            Buffer.BlockCopy(buffer, 0, result, 0, result.Length);
            return (true, result);
        }
        #endregion Public Method 
    }
}
