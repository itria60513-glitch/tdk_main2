using Communication.Config;
using Communication.Interface;
using Communication.Connector.Enum;
using EFEM.DataCenter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TDKLogUtility.Module;

namespace Communication.Connector
{
    public class TcpipConnector : MarshalByRefObjectEx, IConnector
    {
        #region Private Data
        private IProtocol _protocol;
        private IConnectorConfig _config = null;
        private string _objName = "TdkTcpipConnector";
        private bool _IsInitialized = true;
        private ExceptionDictionary _ExDictionary = null;
        private ILogUtility _log = null;
        private bool _bindNIC = false;
        private string _ipNIC = "";
        private byte[] byInBuffer;
        private readonly ITcpIpDprs _tcpIpDprs;
        private object _socketLock = new object();
        private readonly string xmlPath = "D:\\TDKConfig\\ExceptionInfo.xml";
        #endregion

        #region Property
        public bool BindNIC
        {
            get { return _bindNIC; }
            set { _bindNIC = value; }
        }

        public string NICIPAddress
        {
            get { return _ipNIC; }
        }
        public string Name
        {
            set { _objName = value; }
            get { return _objName; }
        }

        public IProtocol Protocol
        {
            set { this._protocol = value; }
            get { return this._protocol; }
        }

        public bool IsConnected
        {
            get;
            set;
        }
        #endregion

        #region Constructors

        public TcpipConnector(IProtocol protocol, IConnectorConfig config, ILogUtility log) : this(protocol, null,config, log)
        {
            this._protocol = protocol;
            this._config = config;
            _ExDictionary = LoadTcpipEntriesFromXml(xmlPath);
        }
        public TcpipConnector(IProtocol protocol, ITcpIpDprs tcpIpDprs, IConnectorConfig config, ILogUtility log)
        {
            if (tcpIpDprs == null)
            {
                tcpIpDprs = new TcpIpDprs();
            }
            this._protocol = protocol;
            _tcpIpDprs = tcpIpDprs;
            this._config = config;
            this._log = log;
            byInBuffer = new byte[protocol.BufferSize];
            config.ModifyValueChangedEvent += ModifyValueChanged;
            _ExDictionary = LoadTcpipEntriesFromXml(xmlPath);
        }
        #endregion

        #region Public Methods
        

        public HRESULT Connect()
        {
            lock (_socketLock)
            {
                if (!_IsInitialized || _tcpIpDprs.Handle != null)
                {
                    WriteLog(12, "Connect()");
                    if (!_IsInitialized)
                    {
                        if (_config == null)
                            WriteLog(10, string.Format("The name [{0}] could not be found in TCPIP.xml", _objName));

                        HRESULT hr = _ExDictionary["NOT_YET_INIT"].hRESULT;
                        WriteLog((int)hr._category, hr._message, hr.ALID);
                        IsConnected = false;
                        return MAKEHR(hr);
                    }
                    if (_tcpIpDprs.Handle != null)
                    {
                        WriteLog(10, "Connection is in use. Call Disconnect() first.");

                        HRESULT hr = _ExDictionary["FAIL_TO_CONNECT"].hRESULT;
                        WriteLog((int)hr._category, hr._message, hr.ALID);
                        IsConnected = false;
                        return MAKEHR(hr);
                    }
                }

                WriteLog(12, string.Format("Connect(ip:{0}, port:{1})", _config.Ip, _config.Port));
                _tcpIpDprs.New_Socket();
                _tcpIpDprs.Handle.ReceiveBufferSize = _protocol.BufferSize;
                if (_bindNIC && _ipNIC != "")
                {
                    try
                    {
                        _tcpIpDprs.Bind_Socket(new IPEndPoint(IPAddress.Parse(_ipNIC), 0));
                    }
                    catch (Exception exBind)
                    {
                        IsConnected = false;
                        WriteLog(10, "--->Failed to bind NIC : " + _ipNIC);
                        WriteLog(10, "--->" + exBind.Message);
                        WriteLog(10, "--->Bypassed NIC Binding.");
                    }
                }

                try
                {
                    _tcpIpDprs.Connect_Socket(new IPEndPoint(IPAddress.Parse(_config.Ip), Convert.ToInt32(_config.Port)));
                    var sobj = new ProtocolStateObject(_protocol.BufferSize) { workSocket = _tcpIpDprs.Handle };
                    _tcpIpDprs.BeginReceive_Socket(sobj.workSocket, sobj.buffer, 0, sobj.buffer.Length, 0, new AsyncCallback(_callback_Receive), sobj);
                    IsConnected = true;
                }
                catch (Exception ex)
                {
                    IsConnected = false;
                    WriteLog(4, ex.Message + ex.StackTrace);
                    WriteLog(4, "[Recovery] Release socket associated resources...");

                    _tcpIpDprs.Handle.Close();
                    _tcpIpDprs.Handle.Dispose();
                    _tcpIpDprs.Handle = null;

                    HRESULT hr = _ExDictionary["FAIL_TO_CONNECT"].hRESULT;
                    WriteLog((int)hr._category, hr._message, hr.ALID);
                    return MAKEHR(hr);
                }
                return null;
            }
        }

        public void Disconnect()
        {
            lock (_socketLock)
            {
                WriteLog(12, "Disconnect()");
                if (_tcpIpDprs.Handle != null)
                {
                    if (_tcpIpDprs.Connected_Socket)
                        _tcpIpDprs.Handle.Shutdown(SocketShutdown.Both);
                    _tcpIpDprs.Handle.Close();
                    _tcpIpDprs.Handle.Dispose();
                    this._protocol.Purge();
                    _tcpIpDprs.Handle = null;
                }
            }
        }

        public virtual HRESULT Send(byte[] byPtBuf, int length)
        {
            if (length == 0)
                return null;

            lock (_socketLock)
            {
                if (!_IsInitialized)
                {
                    HRESULT hr = _ExDictionary["NOT_YET_INIT"].hRESULT;
                    WriteLog((int)hr._category, "Send() " + hr._message, hr.ALID);
                    return MAKEHR(hr);
                }
                if (_tcpIpDprs.Handle == null || !_tcpIpDprs.Connected_Socket)
                {
                    HRESULT hr = _ExDictionary["NOT_YET_CONNECT"].hRESULT;
                    WriteLog((int)hr._category, "Send() " + hr._message, hr.ALID);
                    return MAKEHR(hr);
                }

                length = this._protocol.AddOutFrameInfo(ref byPtBuf, length);

                try
                {
                    WriteLog("Sending==> ", byPtBuf, length);
                    int bytes = _tcpIpDprs.Send_Socket(byPtBuf, length, SocketFlags.None);
                    if (bytes != length)
                    {
                        HRESULT hr = _ExDictionary["FAIL_TO_SEND"].hRESULT;
                        WriteLog((int)hr._category, hr._message, hr.ALID);
                        return MAKEHR(hr);
                    }
                }
                catch (Exception ex)
                {
                    WriteLog(4, ex.Message + ex.StackTrace);
                    //WriteLog(4, "[Recovery] Close socket connection...");
                    //Disconnect();

                    HRESULT hr = _ExDictionary["FAIL_TO_SEND"].hRESULT;
                    WriteLog((int)hr._category, hr._message, hr.ALID);
                    return MAKEHR(hr);
                }
                return null;
            }
        }

        private HRESULT MAKEHR(HRESULT hr)
        {
            hr._extramessage = string.Format("Target device: {0}", _objName);
            return hr;
        }

        private ExceptionDictionary LoadTcpipEntriesFromXml(string xmlPath)
        {
            if (!File.Exists(xmlPath))
                throw new FileNotFoundException($"XML file not found：{xmlPath}");

            var doc = XDocument.Load(xmlPath);

            var entries = doc.Root?
                             .Element("Tcpip")?
                             .Elements("TcpipEntry")
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


        private static int ParseInt(string? s) => int.TryParse(s, out var v) ? v : 0;
        private static ushort ParseUShort(string? s) => ushort.TryParse(s, out var v) ? v : (ushort)0;
        #endregion

        #region Log Services
        protected void WriteLog(int category, string msg)
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
            _log.WriteLog("Tcpip-" + _objName, head);
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
            _log.WriteLog("Tcpip-" + _objName, head);
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
            _log.WriteLog("Tcpip-" + _objName, head);
        }
        #endregion

        #region Event Declarations
        public event ReceivedDataEventHandler DataReceived;

        protected virtual void Fire_DataReceived(byte[] byData, int length)
        {
            DataReceived?.Invoke(byData, length);
        }
        #endregion

        #region Event Sink
        private void _callback_Receive(IAsyncResult result)
        {
            var sobj = (ProtocolStateObject)result.AsyncState;
            try
            {
                if (sobj.workSocket == null || !sobj.workSocket.Connected)
                {
                    WriteLog(10, "Received==> Socket disposed");
                    return;
                }
            }
            catch
            {
                WriteLog(10, "Received==> Socket disposed 2");
                return;
            }
            try
            {
                int readBytes = sobj.workSocket.EndReceive(result);
                if (readBytes > 0)
                    this._protocol.Push(sobj.buffer, readBytes);

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
                        else
                        {
                            var res = Encoding.UTF8.GetBytes("[Wrong Format]Verify Receive Error");
                            Fire_DataReceived(res, res.Length);
                            WriteLog("Received [Wrong Format]==> ", byInBuffer, readBytes);
                        }

                        if (_protocol is IBufferProtocol bufferProtocol)
                        {
                            bufferProtocol.ReturnBufferToPool(byInBuffer);
                        }
                    }
                }
                while (readBytes > 0);
            }
            catch (Exception ex)
            {
                var res = Encoding.UTF8.GetBytes("[Exception]Receive Error");
                Fire_DataReceived(res, res.Length);
                WriteLog(4, "Received [Exception]==> " + ex.Message + ex.StackTrace);
            }
            finally
            {
                try
                {
                    if (sobj.workSocket != null && sobj.workSocket.Connected)
                        sobj.workSocket.BeginReceive(sobj.buffer, 0, sobj.buffer.Length, 0, new AsyncCallback(_callback_Receive), sobj);
                }
                catch { }
            }
        }

        private void ModifyValueChanged(object sender, EventArgs e)
        {
            Connect();
        }
        #endregion

    }
}
