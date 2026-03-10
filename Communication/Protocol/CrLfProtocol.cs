using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Communication.Interface;

namespace Communication.Protocol
{
    public class CrLfProtocol : IProtocol
	{
        #region Private Data
        private CircularQueue m_queue;
        private object _monitor = new object();
        private int last_index = 0;
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
		public CrLfProtocol()
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
				intSize += 2;
				byte[] newArray = new byte[intSize];
				for(int i=0; i<intSize-2; i++)
					newArray[i] = byteArray[i];
				byteArray = newArray;
				byteArray[intSize-2] = (byte)0xD;
				byteArray[intSize-1] = (byte)0xA;
			return intSize;
		}
        public int AddOutFrameInfoWithFakeHeader(ref byte[] byteArray, int intSize)
        {
            intSize += 2;
            byte[] newArray = new byte[intSize];
            for (int i = 0; i < intSize - 2; i++)
                newArray[i] = byteArray[i];
            byteArray = newArray;
            byteArray[intSize - 2] = (byte)0xC;
            byteArray[intSize - 1] = (byte)0xA;
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
				int queuesize = m_queue.size;
				if(queuesize < 2)
					return 0;

				int size = 0;
				for(size=last_index; size<queuesize-1; size++)
				{
					if((m_queue.item(size)==0x0D && m_queue.item(size+1)==0x0A) || 
						(m_queue.item(size)==0x0A && m_queue.item(size+1)==0x0D))
					{
						last_index = 0;
						return m_queue.pop_array(ref byteArray, size+2);
					}
				}
				last_index = queuesize-1;
				return 0;
			}
			finally
			{
				Monitor.Exit(_monitor);
			}
		}
		public (bool, byte[]) VerifyInFrameStructure(byte[] byteArray, int intSize)
		{
			if(intSize < 2)
				return (false, byteArray);
            byte[] result = new byte[byteArray.Length - 2];
            Buffer.BlockCopy(byteArray, 0, result, 0, result.Length);

            if (byteArray[intSize-2]==0xD && byteArray[intSize-1]==0xA)
				return (true, result);
			else if(byteArray[intSize-2]==0xA && byteArray[intSize-1]==0xD)
				return (true, result);
			else
			{
				Fire_LoggingRequest(11, "Cannot find CR or LF in the end of message.");
				return (false, byteArray);
			}
		}
        #endregion Public Method
    }
}
