using Communication.Config;
using Communication.Connector.Enum;
using Communication.Interface;
using Communication.Protocol;
using EFEM.DataCenter;
using EFEM.FileUtilities;
using LogUtility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using TDKLogUtility.Module;
using SerialPort = System.IO.Ports.SerialPort;
namespace Communication.Connector
{
    public class Rs232Connector: IConnector
    {
        #region Private Data
        private Mutex mut = new Mutex();
        private IProtocol _protocol;
        private SerialPort handle = null;
        private IConnectorConfig _config = null;
        private string _objName = "Carrier1";
        private ExceptionDictionary _ExDictionary = null;
        private ILogUtility _log = null;
        private readonly string xmlPath = "D:\\TDKConfig\\ExceptionInfo.xml";
        #endregion

        #region Property
        public IProtocol Protocol
        {
            set { this._protocol = value; }
            get { return this._protocol; }
        }
        public string Name
        {
            set { _objName = value; }
            get { return _objName; }
        }
        public bool IsConnected
        {
            get;
            set;
        }
        #endregion Property

        #region Constructors
        public Rs232Connector(ILogUtility log, IConnectorConfig config)
        {
            this._protocol = new DefaultProtocol();
            _log = log;
            _config = config;
            _ExDictionary = LoadRs232EntriesFromXml(xmlPath);
        }
        public Rs232Connector(IProtocol protocol, ILogUtility log, IConnectorConfig config)
        {
            this._protocol = protocol;
            _log = log;
            _config = config;
            _ExDictionary = LoadRs232EntriesFromXml(xmlPath);
        }
        #endregion

        #region Event Declarations
        public event ReceivedDataEventHandler DataReceived;
        private void Fire_DataReceived(byte[] byData, int Length)
        {
            if (DataReceived != null)
            {
                DataReceived(byData, Length);
                WriteLog("Event forwarded==> ", byData, Length);
            }
            else
            {
                WriteLog("Cannot forward an event.", byData, Length);
            }
        }
        #endregion

        #region Event Sink
        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int readBytes;
                byte[] byInBuffer = new byte[_protocol.BufferSize];
                if (handle.BytesToRead > 0)
                {
                    readBytes = handle.Read(byInBuffer, 0, _protocol.BufferSize);
                    this._protocol.Push(byInBuffer, readBytes);
                }

                do
                {
                    readBytes = this._protocol.Pop(ref byInBuffer);
                    if (readBytes > 0)
                    {
                        var verifyResult = _protocol.VerifyInFrameStructure(byInBuffer, readBytes);
                        if (verifyResult.Item1)
                        {
                            WriteLog("Received==> ", verifyResult.Item2, verifyResult.Item2.Length);
                            Fire_DataReceived(verifyResult.Item2, verifyResult.Item2.Length);
                        }
                    }
                }
                while (readBytes > 0);
            }
            catch (Exception ex)
            {
                WriteLog(4, "Received [Exception]==> " + ex.Message + ex.StackTrace);
            }
        }
        #endregion

        #region Log Services
        private void WriteLog(int category, string msg)
        {
            if (_log == null)
                return;
            string head;
            switch (category)
            {
                case 4:
                    head = String.Format("[SOFT_EXCEPTION] {0}", msg);
                    break;
                case 10:
                    head = String.Format("[INFO] {0}", msg);
                    break;
                case 11:
                    head = String.Format("[KEEP EYES ON] {0}", msg);
                    break;
                case 12:
                    head = String.Format("[CALL] {0}", msg);
                    break;
                case 100:
                    head = msg;
                    break;
                default:
                    return;
            }
            _log.WriteLog("Rs232-" + _objName, head);
        }

        private void WriteLog(int category, string msg, uint alid)
        {
            if (_log == null)
                return;
            string head;
            switch (category)
            {
                case 0:
                    head = string.Format("[EX_NOTIFY][{0}] {1}", alid, msg);
                    break;
                case 1:
                    head = string.Format("[EX_WARNING][{0}] {1}", alid, msg);
                    break;
                case 2:
                    head = string.Format("[EX_ERROR][{0}] {1}", alid, msg);
                    break;
                case 3:
                    head = string.Format("[EX_ALARM][{0}] {1}", alid, msg);
                    break;
                default:
                    return;
            }
            _log.WriteLog("Rs232-" + _objName, head);
        }

        private void WriteLog(string msg, byte[] byPtBuf, int length)
        {
            if (_log == null)
                return;
            string head = "[DATA] " + msg + " ";
            int len = length;
            if (len > 1024)
                len = 1024;
            for (int i = 0; i < len; i++)
            {
                string cc;
                if ((byPtBuf[i] >= (byte)33 && byPtBuf[i] <= (byte)122) || byPtBuf[i] == (byte)0x20)
                    cc = string.Format("{0}", (char)byPtBuf[i]);
                else
                    cc = string.Format("({0})", byPtBuf[i].ToString("X"));
                head = string.Format("{0}{1}", head, cc);
            }
            string aa = string.Format("  Length:{0}", length);
            head = string.Format("{0}{1}", head, aa);
            _log.WriteLog("Rs232-" + _objName, head);
        }
        #endregion

        #region Public Methods
        public bool TryParseStopBits(int value, out StopBits result)
        {
            if (System.Enum.IsDefined(typeof(StopBits), value))
            {
                result = (StopBits)value;
                return true;
            }

            result = StopBits.None;
            return false;
        }

        public HRESULT Connect()
        {
            mut.WaitOne();
            try
            {
                if (handle != null)
                {
                    WriteLog(12, "Connect()");

                    if (handle != null)
                    {
                        WriteLog(10, "Port is in use. Call Disconnect() first.");

                        HRESULT hr = _ExDictionary["FAIL_TO_OPEN_PORT"].hRESULT;
                        WriteLog((int)hr._category, hr._message, hr.ALID);
                        return MAKEHR(hr);
                    }
                }

                WriteLog(12, string.Format("Connect(port:{0}, baud rate:{1}, parity:{2}, data bits:{3}, stop bitd:{4})",
                                            _config.Comport, (int)_config.Baud, _config.Parity.ToString(),
                                            (int)_config.DataBits, _config.StopBits.ToString()));
                handle = new System.IO.Ports.SerialPort(_config.Comport, (int)_config.Baud, ParseParity(_config.Parity),
                    (int)_config.DataBits, ParseStopBits(_config.StopBits));
                handle.DtrEnable = true;
                handle.RtsEnable = false;

                try
                {
                    handle.ReadBufferSize = _protocol.BufferSize;
                    handle.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);//This DataReceived is .NET Serial Event
                    handle.Open();
                    IsConnected = true;
                }
                catch (Exception ex)
                {
                    IsConnected = false;
                    WriteLog(4, ex.Message + ex.StackTrace);
                    WriteLog(4, "[Recovery] Release SerialPort associated resources...");
                    handle.DataReceived -= new SerialDataReceivedEventHandler(port_DataReceived);
                    handle.Close();
                    handle.Dispose();
                    handle = null;
                    HRESULT hr = _ExDictionary["FAIL_TO_OPEN_PORT"].hRESULT;
                    WriteLog((int)hr._category, hr._message, hr.ALID);
                    return MAKEHR(hr);
                }
                return null;
            }
            finally
            {
                mut.ReleaseMutex();
            }
        }

        public void Disconnect()
        {
            mut.WaitOne();
            try
            {
                WriteLog(12, "Disconnect()");
                if (handle != null)
                {
                    handle.DataReceived -= new SerialDataReceivedEventHandler(port_DataReceived);
                    if (handle.IsOpen)
                        handle.Close();
                    handle.Dispose();
                    this._protocol.Purge();
                    handle = null;
                }
            }
            finally
            {
                IsConnected = false;
                mut.ReleaseMutex();
            }
        }

        public HRESULT Send(byte[] byPtBuf, int Length)
        {
            if (Length == 0)
                return null;

            mut.WaitOne();
            try
            {
                if (handle == null || !handle.IsOpen)
                {
                    HRESULT hr = _ExDictionary["PORT_NOT_OPEN"].hRESULT;
                    WriteLog((int)hr._category, "Send() " + hr._message, hr.ALID);
                    return MAKEHR(hr);
                }

                Length = this._protocol.AddOutFrameInfo(ref byPtBuf, Length);

                try
                {
                    WriteLog("Sending==> ", byPtBuf, Length);
                    handle.Write(byPtBuf, 0, Length);
                }
                catch (Exception ex)
                {
                    WriteLog(4, ex.Message + ex.StackTrace);

                    HRESULT hr = _ExDictionary["FAIL_TO_SEND_DATA"].hRESULT;
                    WriteLog((int)hr._category, hr._message, hr.ALID);
                    return MAKEHR(hr);
                }
                return null;
            }
            finally
            {
                mut.ReleaseMutex();
            }
        }
        #endregion

        #region Private Methods
        private Parity ParseParity(int val) => TryParseParity(val, out var p)
            ? p
            : Parity.None;

        private StopBits ParseStopBits(int val) => TryParseStopBits(val, out var s)
            ? s
            : StopBits.None;

        private HRESULT MAKEHR(HRESULT hr)
        {
            hr._extramessage = string.Format("Target device: {0}", _objName);
            return hr;
        }
        #region XML methods
        private static int ParseInt(string? s) => int.TryParse(s, out var v) ? v : 0;

        private static ushort ParseUShort(string? s) => ushort.TryParse(s, out var v) ? v : (ushort)0;

        private bool TryParseParity(int value, out Parity result)
        {
            if (System.Enum.IsDefined(typeof(Parity), value))
            {
                result = (Parity)value;
                return true;
            }

            result = Parity.None;
            return false;
        }

        private ExceptionDictionary LoadRs232EntriesFromXml(string xmlPath)
        {
            if (!File.Exists(xmlPath))
                throw new FileNotFoundException($"XML file not found：{xmlPath}");

            var doc = XDocument.Load(xmlPath);

            var entries = doc.Root?
                             .Element("Rs232")?
                             .Elements("Rs232Entry")
                             .Select(e => new ExceptionInfoConfigTcpip
                             {
                                 Key = (string?)e.Attribute("key") ?? "",
                                 Id = ParseUShort((string?)e.Attribute("id")),
                                 Message = (string?)e.Attribute("message") ?? "",
                                 Category = ParseUShort((string?)e.Attribute("category")),
                                 Mode = ParseInt((string?)e.Attribute("mode")),
                                 Remedy = (string?)e.Attribute("remedy") ?? "",
                                 MapErrId = ParseInt((string?)e.Attribute("maperrid")),
                                 ErrId = ParseInt((string?)e.Attribute("errid")),
                             })
                             .ToList() ?? new List<ExceptionInfoConfigTcpip>();

            var dict = new ExceptionDictionary();

            foreach (var item in entries)
            {
                if (string.IsNullOrWhiteSpace(item.Key))
                    continue;

                var exInfo = ToExceptionInfo(item);

                if (dict.Contains(item.Key))
                {
                    dict[item.Key] = exInfo;
                }
                else
                {
                    dict.Add(item.Key, exInfo);
                }
            }

            return dict;
        }

        private static ExceptionInfo ToExceptionInfo(ExceptionInfoConfigTcpip src)
        {
            return new ExceptionInfo(
                src.Category,
                src.Id,
                src.Message,
                (ExCategory)src.Category,
                (ExMode)src.Mode,
                src.Remedy,
                (uint)(src.MapErrId < 0 ? 0 : src.MapErrId));
        }
        #endregion XML methods
        #endregion Private Methods
    }
}
