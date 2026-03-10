using Communication.Interface;
using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Communication.Protocol
{
    public class LoadportProtocol : IProtocol
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
        public LoadportProtocol()
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

        #region Private Method

        private static byte NibbleToUpperHexAscii(byte nibble)
        {
            // 0..15 -> '0'..'9','A'..'F'
            return (byte)(nibble < 10 ? ('0' + nibble) : ('A' + (nibble - 10)));
        }

        #endregion Private Method

        #region Public Method
        public int AddOutFrameInfo(ref byte[] byteArray, int intSize)
        {

            if (byteArray == null || intSize < 0 || intSize > byteArray.Length)
                return -1;

            int len = intSize;
            int totalLen = len + 9;
            int dlen = len + 5;

            var frame = new byte[totalLen];

            frame[0] = 0x01;

            frame[1] = (byte)((dlen >> 8) & 0xFF);
            frame[2] = (byte)(dlen & 0xFF);

            frame[3] = (byte)'0';
            frame[4] = (byte)'0';

            Buffer.BlockCopy(byteArray, 0, frame, 5, len);

            frame[len + 5] = (byte)';';

            int checksum = 0;
            for (int i = 1; i < len + 6; i++)
                checksum += frame[i];

            byte cs = (byte)checksum;
            byte csh = (byte)((cs >> 4) & 0x0F);
            byte csl = (byte)(cs & 0x0F);

            frame[len + 6] = NibbleToUpperHexAscii(csh);
            frame[len + 7] = NibbleToUpperHexAscii(csl);

            frame[len + 8] = 0x03;

            byteArray = frame;
            return totalLen;
        }
        public int AddOutFrameInfoWithFakeHeader(ref byte[] byteArray, int intSize)
        {

            if (byteArray == null || intSize < 0 || intSize > byteArray.Length)
                return -1;

            int len = intSize;
            int totalLen = len + 9;
            int dlen = len + 5;

            var frame = new byte[totalLen];

            frame[0] = 0x02; //Fake header

            frame[1] = (byte)((dlen >> 8) & 0xFF);
            frame[2] = (byte)(dlen & 0xFF);

            frame[3] = (byte)'0';
            frame[4] = (byte)'0';

            Buffer.BlockCopy(byteArray, 0, frame, 5, len);

            frame[len + 5] = (byte)';';

            int checksum = 0;
            for (int i = 1; i < len + 6; i++)
                checksum += frame[i];

            byte cs = (byte)checksum;
            byte csh = (byte)((cs >> 4) & 0x0F);
            byte csl = (byte)(cs & 0x0F);

            frame[len + 6] = NibbleToUpperHexAscii(csh);
            frame[len + 7] = NibbleToUpperHexAscii(csl);

            frame[len + 8] = 0x03;

            byteArray = frame;
            return totalLen;
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
                int size = 0;
                while (m_queue.size != 0)
                {
                    byteArray[size] = m_queue.pop_front();
                    size++;
                }
                return size;
            }
            finally
            {
                Monitor.Exit(_monitor);
            }
        }
        public (bool, byte[]) VerifyInFrameStructure(byte[] buffer, int size)
        {
            
            if (size < 3)
            {
                Console.WriteLine("Message too short");
                return (false, buffer);
            }

            if (buffer[0] != 0x01)
            {
                Console.WriteLine("Wrong header");
                return (false, buffer);
            }

            var dataLen = (buffer[1] << 8) | buffer[2];
            if (dataLen + 4 != size)
            {
                Console.WriteLine("Wrong length");
                return (false, buffer);
            }

            var calc = 0;
            for (int i = 1; i < size - 3; i++)
                calc += buffer[i];

            string hex = ((byte)calc).ToString("X2");
            if (buffer[size - 3] != Encoding.UTF8.GetBytes(hex[0].ToString()).First() || buffer[size - 2] != Encoding.UTF8.GetBytes(hex[1].ToString()).First())
            {
                Console.WriteLine("Wrong checksum");
                return (false, buffer);
            }

            if (buffer[size - 1] != 0x03)
            {
                Console.WriteLine("Wrong tail");
                return (false, buffer);
            }

            byte[] result = new byte[size - 8];
            Buffer.BlockCopy(buffer, 5, result, 0, size - 8);
            return (true, result);
        }
        #endregion Public Method
    }
}
