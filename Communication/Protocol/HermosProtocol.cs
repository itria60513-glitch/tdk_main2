using Communication.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Communication.Protocol
{
    public class HermosProtocol : IProtocol
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
        public HermosProtocol()
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
            if (byteArray == null || intSize < 0 || intSize > byteArray.Length)
                return -1; 

            var len = intSize;
            var total = len + 8; 
            var frame = new byte[total];
            var highNibble = (byte)((len >> 4) & 0x0F);
            var lowNibble = (byte)(len & 0x0F);

            frame[0] = (byte)'S';
            frame[1] = ToHexChar(highNibble);
            frame[2] = ToHexChar(lowNibble);

            Buffer.BlockCopy(byteArray, 0, frame, 3, len);

            frame[3 + len] = 0x0D;
            int iend = 4 + len;
            int xorChk = 0;
            for (int i = 0; i < iend; i++)
                xorChk ^= frame[i];

            byte csXor = (byte)xorChk;
            frame[iend] = ToHexChar((byte)((csXor >> 4) & 0x0F));
            frame[iend + 1] = ToHexChar((byte)(csXor & 0x0F));

            int sumChk = 0;
            for (int i = 0; i < iend; i++)
                sumChk += frame[i];

            byte csSum = (byte)sumChk;
            frame[iend + 2] = ToHexChar((byte)((csSum >> 4) & 0x0F));
            frame[iend + 3] = ToHexChar((byte)(csSum & 0x0F));

            byteArray = frame;
            return total;

        }
        public int AddOutFrameInfoWithFakeHeader(ref byte[] byteArray, int intSize)
        {
            if (byteArray == null || intSize < 0 || intSize > byteArray.Length)
                return -1;

            var len = intSize;
            var total = len + 8;
            var frame = new byte[total];
            var highNibble = (byte)((len >> 4) & 0x0F);
            var lowNibble = (byte)(len & 0x0F);

            frame[0] = (byte)'R';
            frame[1] = ToHexChar(highNibble);
            frame[2] = ToHexChar(lowNibble);

            Buffer.BlockCopy(byteArray, 0, frame, 3, len);

            frame[3 + len] = 0x0D;
            int iend = 4 + len;
            int xorChk = 0;
            for (int i = 0; i < iend; i++)
                xorChk ^= frame[i];

            byte csXor = (byte)xorChk;
            frame[iend] = ToHexChar((byte)((csXor >> 4) & 0x0F));
            frame[iend + 1] = ToHexChar((byte)(csXor & 0x0F));

            int sumChk = 0;
            for (int i = 0; i < iend; i++)
                sumChk += frame[i];

            byte csSum = (byte)sumChk;
            frame[iend + 2] = ToHexChar((byte)((csSum >> 4) & 0x0F));
            frame[iend + 3] = ToHexChar((byte)(csSum & 0x0F));

            byteArray = frame;
            return total;
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

            if (buffer == null || size <= 0 || size > buffer.Length)
                return (false, buffer);

            var len = size;

            if (len < 8)
                return (false, buffer);

            if (buffer[0] != (byte)'S')
                return (false, buffer);

            if (buffer[len - 5] != 0x0D) 
                return (false, buffer);

            int sumInt = 0;
            for (int i = 0; i < len - 4; i++)
                sumInt += buffer[i];

            var sum = (byte)sumInt;
            var sumHighAscii = ToHexAscii((byte)((sum >> 4) & 0x0F));
            var sumLowAscii = ToHexAscii((byte)(sum & 0x0F));

            if (buffer[len - 2] != sumHighAscii || buffer[len - 1] != sumLowAscii)
                return (false, buffer);

            byte x = 0;
            for (int i = 0; i < len - 4; i++)
                x ^= buffer[i];

            byte xorHighAscii = ToHexAscii((byte)((x >> 4) & 0x0F));
            byte xorLowAscii = ToHexAscii((byte)(x & 0x0F));

            if (buffer[len - 4] != xorHighAscii || buffer[len - 3] != xorLowAscii)
                return (false, buffer);

            var command = (char)buffer[3];

            if (!(command == 'x' || command == 'w' || command == 'n' || command == 'v' || command == 'e'))
                return (false, buffer);

            int infoLen = len - 10;
            if (infoLen > 0 && infoLen > (MAXINFOLEN - 10))
            {
                return (false, buffer);
            }
            byte[] result = new byte[len - 8];
            Buffer.BlockCopy(buffer, 0, result, 0, result.Length);
            return (true, result);

        }
        #endregion Public Method 

        #region Private Method

        private static byte ToHexChar(byte nibble)
        {
            return (byte)(nibble < 10 ? ('0' + nibble) : ('A' + (nibble - 10)));
        }

        private static byte ToHexAscii(byte nibble /* 0..15 */)
        {
            return (byte)(nibble < 10 ? ('0' + nibble) : ('A' + (nibble - 10)));
        }

        #endregion

    }
}
