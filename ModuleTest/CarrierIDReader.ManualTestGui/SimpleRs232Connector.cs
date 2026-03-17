using System;
using System.IO.Ports;
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

        public SimpleRs232Connector(IProtocol protocol, string comPort, int baudRate, int parity, int dataBits, int stopBits)
        {
            _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            _comPort = comPort ?? throw new ArgumentNullException(nameof(comPort));
            _baudRate = baudRate;
            _parity = (Parity)parity;
            _dataBits = dataBits;
            _stopBits = stopBits == 2 ? StopBits.Two : StopBits.One;
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

            _port = new SerialPort(_comPort, _baudRate, _parity, _dataBits, _stopBits);
            _port.DtrEnable = true;
            _port.RtsEnable = false;
            _port.ReadBufferSize = _protocol.BufferSize;
            _port.DataReceived += Port_DataReceived;
            _port.Open();
            IsConnected = true;
            return null;
        }

        public HRESULT Send(byte[] byPtBuf, int length)
        {
            if (_port == null || !_port.IsOpen)
                throw new InvalidOperationException("Port is not open.");

            length = _protocol.AddOutFrameInfo(ref byPtBuf, length);
            if (length <= 0)
                throw new InvalidOperationException("Protocol rejected the outgoing payload.");

            _port.Write(byPtBuf, 0, length);
            return null;
        }

        public void Disconnect()
        {
            if (_port != null)
            {
                _port.DataReceived -= Port_DataReceived;
                if (_port.IsOpen)
                    _port.Close();
                _port.Dispose();
                _port = null;
                IsConnected = false;
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
                            DataReceived?.Invoke(verifyResult.Item2, verifyResult.Item2.Length);
                        }
                    }
                }
                while (readLength > 0);
            }
            catch
            {
            }
        }
    }
}