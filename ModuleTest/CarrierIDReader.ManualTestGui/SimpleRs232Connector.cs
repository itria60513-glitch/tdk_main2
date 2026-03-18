using System;
using System.IO.Ports;
using System.Text;
using Communication.Connector.Enum;
using Communication.Interface;

namespace CarrierIDReader.ManualTestGui
{
    internal sealed class SimpleRs232Connector : IConnector, IDisposable
    {
        private SerialPort _port;
        private IProtocol _protocol;
        private readonly string _comPort;
        private readonly int _baudRate;
        private readonly Parity _parity;
        private readonly int _dataBits;
        private readonly StopBits _stopBits;
        private readonly Action<string> _log;

        public SimpleRs232Connector(IProtocol protocol, string comPort, int baudRate, int parity, int dataBits, int stopBits, Action<string> log = null)
        {
            _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            _comPort = comPort ?? throw new ArgumentNullException(nameof(comPort));
            _baudRate = baudRate;
            _parity = (Parity)parity;
            _dataBits = dataBits;
            _stopBits = stopBits == 2 ? StopBits.Two : StopBits.One;
            _log = log;
        }

        public bool IsConnected { get; set; }

        public IProtocol Protocol
        {
            get => _protocol;
            set => _protocol = value ?? throw new ArgumentNullException(nameof(value));
        }

        public event ReceivedDataEventHandler DataReceived;

        public HRESULT Connect()
        {
            if (_port != null)
            {
                Disconnect();
            }

            _protocol.Purge();
            _port = new SerialPort(_comPort, _baudRate, _parity, _dataBits, _stopBits);
            _port.DtrEnable = true;
            _port.RtsEnable = false;
            _port.ReadBufferSize = _protocol.BufferSize;
            _port.DataReceived += Port_DataReceived;
            _port.Open();
            _port.DiscardInBuffer();
            _port.DiscardOutBuffer();
            IsConnected = true;
            Log(string.Format("Serial connected: Port={0}, Baud={1}, Parity={2}, DataBits={3}, StopBits={4}", _comPort, _baudRate, _parity, _dataBits, _stopBits));
            return null;
        }

        public HRESULT Send(byte[] byPtBuf, int length)
        {
            if (_port == null || !_port.IsOpen)
                throw new InvalidOperationException("Port is not open.");

            _protocol.Purge();
            _port.DiscardInBuffer();
            length = _protocol.AddOutFrameInfo(ref byPtBuf, length);
            if (length <= 0)
                throw new InvalidOperationException("Protocol rejected the outgoing payload.");

            LogFrame("TX", byPtBuf, length);
            _port.Write(byPtBuf, 0, length);
            return null;
        }

        public void Disconnect()
        {
            if (_port != null)
            {
                _port.DataReceived -= Port_DataReceived;
                if (_port.IsOpen)
                {
                    _port.DiscardInBuffer();
                    _port.DiscardOutBuffer();
                    _port.Close();
                }
                _port.Dispose();
                _port = null;
                _protocol.Purge();
                IsConnected = false;
                Log("Serial disconnected.");
            }
        }

        public void Dispose()
        {
            Disconnect();
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                byte[] inputBuffer = new byte[_protocol.BufferSize];
                if (_port.BytesToRead > 0)
                {
                    int readBytes = _port.Read(inputBuffer, 0, _protocol.BufferSize);
                    LogFrame("RX-RAW", inputBuffer, readBytes);
                    _protocol.Push(inputBuffer, readBytes);
                }

                int readLength;
                do
                {
                    readLength = _protocol.Pop(ref inputBuffer);
                    if (readLength > 0)
                    {
                        var verifyResult = _protocol.VerifyInFrameStructure(inputBuffer, readLength);
                        if (verifyResult.Item1)
                        {
                            LogFrame("RX", verifyResult.Item2, verifyResult.Item2.Length);
                            DataReceived?.Invoke(verifyResult.Item2, verifyResult.Item2.Length);
                        }
                        else
                        {
                            LogFrame("RX-INVALID", inputBuffer, readLength);
                        }
                    }
                }
                while (readLength > 0);
            }
            catch (Exception ex)
            {
                Log(string.Format("Serial receive error: {0}", ex.Message));
            }
        }

        private void Log(string message)
        {
            _log?.Invoke(message);
        }

        private void LogFrame(string direction, byte[] buffer, int length)
        {
            if (buffer == null || length <= 0)
            {
                return;
            }

            string ascii = Encoding.ASCII.GetString(buffer, 0, length)
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\0", "\\0");
            Log(string.Format("{0}: {1}", direction, ascii));
        }
    }
}