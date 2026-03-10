using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communication.Protocol
{
    public class CircularQueue
    {
        public CircularQueue()
        {
            m_dataarray = new byte[3096];
            m_queuesize = 3096;
            m_head = m_tail = 0;
        }
        public CircularQueue(int buffersize)
        {
            m_dataarray = new byte[buffersize];
            m_queuesize = buffersize;
            m_head = m_tail = 0;
        }
        public void backward(ref int pointer)
        {
            pointer = pointer - 1;
            if (pointer < 0)
                pointer = m_queuesize - 1;
        }
        public void forward(ref int pointer)
        {
            pointer = pointer + 1;
            if (pointer == m_queuesize)
                pointer = 0;
        }
        public byte item(int index)
        {
            if (index >= m_queuesize)
                return 0;

            int temp = m_head;

            lock (_lock)
            {
                temp = temp + index;
                if (temp >= m_queuesize)
                    temp -= m_queuesize;
                return m_dataarray[temp];
            }
        }
        public byte pop_front()
        {
            if (this.size == 0)
                return 0;

            lock (_lock)
            {
                byte temp = m_dataarray[m_head];
                this.forward(ref m_head);
                return temp;
            }
        }
        public int pop_array(ref byte[] arrayItem, int length)
        {
            lock (_lock)
            {
                if (NumberOfBytesInQueue < length)
                {
                    return 0;
                }

                for (int i = 0; i < length; i++)
                {
                    arrayItem[i] = m_dataarray[m_head];
                    this.forward(ref m_head);
                }
                return length;
            }
        }
        public void purge()
        {
            lock (_lock)
            {
                m_head = m_tail = 0;
            }
        }
        public bool push_back(byte byteItem)
        {
            lock (_lock)
            {
                m_dataarray[m_tail] = byteItem;
                this.forward(ref m_tail);
                if (m_tail == m_head)
                {
                    this.backward(ref m_tail);
                    return false;
                }
                return true;
            }
        }
        public int push_array(byte[] arrayItem, int length)
        {
            lock (_lock)
            {
                if (length > (m_queuesize - NumberOfBytesInQueue) || length <= 0)
                    return 0;
                for (int i = 0; i < length; i++)
                {
                    m_dataarray[m_tail] = arrayItem[i];
                    this.forward(ref m_tail);
                }
                return length;
            }
        }
        public int size
        {
            get
            {
                lock (_lock)
                {
                    return NumberOfBytesInQueue;
                }
            }
        }
        private int NumberOfBytesInQueue
        {
            get
            {
                if (m_tail >= m_head)
                    return (m_tail - m_head);
                else
                    return (m_queuesize - m_head + m_tail);
            }
        }
        public int QueueSize
        {
            get
            {
                return m_queuesize;
            }
        }

        private byte[] m_dataarray;
        private int m_head;
        private int m_queuesize;
        private int m_tail;
        private object _lock = new object();
    }
}
