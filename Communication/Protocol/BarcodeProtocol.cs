using Communication.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Communication.Protocol
{
    public class BarcodeProtocol: IProtocol
    {
        #region Private Data
        private CircularQueue m_queue;
        private object _monitor = new object();
        private int last_index = 0;
        private readonly int PARAMLEN = 50;
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
        public BarcodeProtocol()
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
            intSize += 1;
            byte[] newArray = new byte[intSize];
            for (int i = 0; i < intSize - 1; i++)
                newArray[i] = byteArray[i];
            byteArray = newArray;
            byteArray[intSize - 1] = (byte)0xD;
            return intSize;
        }
        public int AddOutFrameInfoWithFakeHeader(ref byte[] byteArray, int intSize)
        {
            intSize += 1;
            byte[] newArray = new byte[intSize];
            for (int i = 0; i < intSize - 1; i++)
                newArray[i] = byteArray[i];
            byteArray = newArray;
            byteArray[intSize - 1] = (byte)0xC;
            return intSize;
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
        public (bool,byte[]) VerifyInFrameStructure(byte[] buffer, int size)
        {
            if (buffer[size - 1] == 0x0D)
            {
                byte[] result = new byte[buffer.Length - 1];
                Buffer.BlockCopy(buffer, 0, result, 0, result.Length);
                return (true, result);
            }

            return (false, buffer);
        }
        #endregion Public Method 
    }
}
