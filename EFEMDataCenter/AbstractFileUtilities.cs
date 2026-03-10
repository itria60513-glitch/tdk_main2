using EFEM.DataCenter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace EFEM.FileUtilities
{
    public delegate void WriteUserLogDel(string message);

    public interface AbstractFileUtilities
    {
        void FlushEFEMConfig();
        void CloseAllFile();
        Hashtable GetObjectDictionary();

        event WriteUserLogDel OnWriteUserLogRequired;
        ArrayList GetUserLoginData();
        string[] GetTypeList();
        bool AddUser(UserInfo info);
        bool ChangePassword(UserInfo info);
        bool DeleteUser(string user);
        bool UserDataQuery(Hashtable commandTable, ref Hashtable returnTable);
        bool DeleteUser(string username, string adminPassword, ref string returnMessage);

        List<(string,int)> CommunicationLoad();
        List<string> LoadPortActorLoad();
        List<string> N2NozzleLoad();
        List<string> DIOLoad();

        List<(string, int)> GetCommList();
        List<string> GetLoadPortList();
        List<string> GetDIOList();
        List<string> GetN2NozzleList();

        TCPConfig GetTCPSetting(string key);
        RS232Config GetSerialSetting(string key);
        LoadPortConfig GetLoadPortConfigSetting(string key);
        N2NozzleConfig GetN2NozzleConfigSetting(string key);
        DIOConfig GetDIOConfigSetting(string key);

        void TCPConfigSave(string key, string ipAddress, string port);
        void SerialPortConfigSave(string key, string port, int baud, int parity, int databits, int stopbits);
        void LoadPortActorSave(string key, LoadPortConfig settings);
        void N2NozzleSave(string key, N2NozzleConfig settings);
        void DIOSave(string key, DIOConfig settings);

        void TCPConfigApply(string key, string ipAddress, string port);
        void SerialConfigApply(string key, string port, int baud, int parity, int databits, int stopbits);
        void LoadPortConfigApply(string key, LoadPortConfig settings);
        void N2NozzleConfigApply(string key, N2NozzleConfig settings);
        void DIOConfigApply(string key, DIOConfig settings);

        void ResetToDefaultValue(string Type, string key);


        #region XML file handle
        string ReadXMLFile(string filename, ref string xmlDoc);
        string WriteXMLFile(string filename, string xmlDoc);
        #endregion
    }

    #region TCPIP Config
    [Serializable]
    public class TcpipPortConfig
    {
        public TcpipPortConfig() { }
        public TcpipPortConfig(string ip, string port)
        {
            _ip = ip;
            _port = port;
        }
        public string _ip;
        public string _port;
    }

    public class TCPConfig
    {
        public TCPConfig(string ip, string port)
        {
            _ip = ip;
            _port = port;
        }
        private string _ip;
        private string _port;

        public string Ip
        {
            get => _ip;
            set
            {
                _ip = value;
                OnValueChanged();
            }
        }

        public string Port
        {
            get => _port;
            set
            {
                _port = value;
                OnValueChanged();
            }
        }


        public event EventHandler ValueChanged;
        protected virtual void OnValueChanged() => ValueChanged?.Invoke(this, EventArgs.Empty);


    }

    [Serializable]
    public class TCPConnectorConfig : IConnectorConfig
    {
        public TCPConnectorConfig(TCPConfig config)
        {
            Ip = config.Ip;
            Port = config.Port;


            config.ValueChanged += (s, e) =>
            {
                Ip = config.Ip;
                Port = config.Port;

                OnValueChanged();
            };

        }
        public string Comport { get; set; }
        public int Baud { get; set; }
        public int Parity { get; set; }
        public int DataBits { get; set; }
        public int StopBits { get; set; }

        public string Ip { get; set; }
        public string Port { get; set; }

        public event EventHandler ModifyValueChangedEvent;
        protected virtual void OnValueChanged() => ModifyValueChangedEvent?.Invoke(this, EventArgs.Empty);


    }


    [Serializable]
    public class TcpipPortConfigDictionary : IDictionary
    {
        public Hashtable innerHash;

        #region Constructors
        public TcpipPortConfigDictionary()
        {
            innerHash = new Hashtable();
        }
        public TcpipPortConfigDictionary(TcpipPortConfigDictionary original)
        {
            innerHash = new Hashtable(original.innerHash);
        }
        public TcpipPortConfigDictionary(IDictionary dictionary)
        {
            innerHash = new Hashtable(dictionary);
        }
        public TcpipPortConfigDictionary(int capacity)
        {
            innerHash = new Hashtable(capacity);
        }
        #endregion

        #region Implementation of IDictionary
        public TcpipPortConfigDictionaryEnumerator GetEnumerator()
        {
            return new TcpipPortConfigDictionaryEnumerator(this);
        }
        System.Collections.IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new TcpipPortConfigDictionaryEnumerator(this);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public void Remove(string key)
        {
            innerHash.Remove(key);
        }
        void IDictionary.Remove(object key)
        {
            Remove((string)key);
        }
        public bool Contains(string key)
        {
            return innerHash.Contains(key);
        }
        bool IDictionary.Contains(object key)
        {
            return Contains((string)key);
        }
        public void Clear()
        {
            innerHash.Clear();
        }
        public void Add(string key, TcpipPortConfig value)
        {
            innerHash.Add(key, value);
        }
        void IDictionary.Add(object key, object value)
        {
            Add((string)key, (TcpipPortConfig)value);
        }
        public bool IsReadOnly
        {
            get
            {
                return innerHash.IsReadOnly;
            }
        }
        public TcpipPortConfig this[string key]
        {
            get
            {
                return (TcpipPortConfig)innerHash[key];
            }
            set
            {
                innerHash[key] = value;
            }
        }
        object IDictionary.this[object key]
        {
            get
            {
                return this[(string)key];
            }
            set
            {
                this[(string)key] = (TcpipPortConfig)value;
            }
        }
        public System.Collections.ICollection Values
        {
            get
            {
                return innerHash.Values;
            }
        }
        public System.Collections.ICollection Keys
        {
            get
            {
                return innerHash.Keys;
            }
        }
        public bool IsFixedSize
        {
            get
            {
                return innerHash.IsFixedSize;
            }
        }
        #endregion

        #region Implementation of ICollection
        public void CopyTo(System.Array array, int index)
        {
            innerHash.CopyTo(array, index);
        }
        public void CopyTo(TcpipPortConfigDictionary wsc, int index)
        {
            IEnumerator keys = Keys.GetEnumerator();
            IEnumerator values = Values.GetEnumerator();

            int count = Count;

            for (int i = 0; i < index; i++)
            {
                keys.MoveNext();
                values.MoveNext();
            }

            for (int i = index; i < count; i++)
            {
                keys.MoveNext();
                values.MoveNext();

                wsc.Add(keys.Current as string, values.Current as TcpipPortConfig);
            }
        }
        public bool IsSynchronized
        {
            get
            {
                return innerHash.IsSynchronized;
            }
        }
        public int Count
        {
            get
            {
                return innerHash.Count;
            }
        }
        public object SyncRoot
        {
            get
            {
                return innerHash.SyncRoot;
            }
        }
        #endregion
    }

    [Serializable]
    public class TcpipPortConfigDictionaryEnumerator : IDictionaryEnumerator
    {
        private IDictionaryEnumerator innerEnumerator;

        internal TcpipPortConfigDictionaryEnumerator(TcpipPortConfigDictionary enumerable)
        {
            innerEnumerator = enumerable.innerHash.GetEnumerator();
        }

        #region Implementation of IDictionaryEnumerator
        public string Key
        {
            get
            {
                return (string)innerEnumerator.Key;
            }
        }
        object IDictionaryEnumerator.Key
        {
            get
            {
                return Key;
            }
        }
        public TcpipPortConfig Value
        {
            get
            {
                return (TcpipPortConfig)innerEnumerator.Value;
            }
        }
        object IDictionaryEnumerator.Value
        {
            get
            {
                return Value;
            }
        }
        public System.Collections.DictionaryEntry Entry
        {
            get
            {
                return innerEnumerator.Entry;
            }
        }
        #endregion

        #region Implementation of IEnumerator
        public void Reset()
        {
            innerEnumerator.Reset();
        }
        public bool MoveNext()
        {
            return innerEnumerator.MoveNext();
        }
        object IEnumerator.Current
        {
            get
            {
                return innerEnumerator.Current;
            }
        }
        public DictionaryEntry Current
        {
            get
            {
                return Entry;
            }
        }
        #endregion
    }

    #endregion


    #region RS232 Config
    [Serializable]
    public class RS232Config
    {
        public RS232Config(string port, int baud, int parity, int databits, int stopbits)
        {
            _port = port;
            _baud = baud;
            _parity = parity;
            _databits = databits;
            _stopbits = stopbits;
        }

        private string _port;
        private int _baud;
        private int _parity;
        private int _databits;
        private int _stopbits;

        public string Port
        {
            get => _port;
            set
            {
                _port = value;
                OnValueChanged();
            }
        }

        public int Baud
        {
            get => _baud;
            set
            {
                _baud = value;
                OnValueChanged();
            }
        }

        public int Parity
        {
            get => _parity;
            set
            {
                _parity = value;
                OnValueChanged();
            }
        }

        public int DataBits
        {
            get => _databits;
            set
            {
                _databits = value;
                OnValueChanged();
            }
        }

        public int StopBits
        {
            get => _stopbits;
            set
            {
                _stopbits = value;
                OnValueChanged();
            }
        }

        public event EventHandler ValueChanged;
        protected virtual void OnValueChanged() => ValueChanged?.Invoke(this, EventArgs.Empty);
    }

    public class RS232ConnectorConfig : IConnectorConfig
    {
        public RS232ConnectorConfig(RS232Config config)
        {
            Comport = config.Port;
            Baud = config.Baud;
            Parity = config.Parity;
            DataBits = config.DataBits;
            StopBits = config.StopBits;

            config.ValueChanged += (s, e) =>
            {
                Comport = config.Port;
                Baud = config.Baud;
                Parity = config.Parity;
                DataBits = config.DataBits;
                StopBits = config.StopBits;

                OnValueChanged();
            };
        }
        public string Comport { get; set; }
        public int Baud { get; set; }
        public int Parity { get; set; }
        public int DataBits { get; set; }
        public int StopBits { get; set; }

        public string Ip { get; set; }
        public string Port { get; set; }

        public event EventHandler ModifyValueChangedEvent;
        protected virtual void OnValueChanged() => ModifyValueChangedEvent?.Invoke(this, EventArgs.Empty);
    }


    [Serializable]
    public sealed class SerialPortConfig
    {
        public SerialPortConfig(int port, int baud, int parity, int databits, int stopbits)
        {
            if (port < 0 || port > 20)
                _port = SerialPort.None;
            else
                _port = (SerialPort)port;

            if (baud != 4800 && baud != 9600 && baud != 19200 && baud != 38400 && baud != 57600 && baud != 115200)
                _baud = SerialBaudRate.BAUD_9600;
            else
                _baud = (SerialBaudRate)baud;

            if (parity < 0 || parity > 4)
                _parity = Parity.None;
            else
                _parity = (Parity)parity;

            if (databits < 5 || databits > 8)
                _databits = SerialDataBits.EIGHT;
            else
                _databits = (SerialDataBits)databits;

            if (stopbits < 0 || stopbits > 3)
                _stopbits = StopBits.One;
            else
                _stopbits = (StopBits)stopbits;
        }

        public SerialPort _port;
        public SerialBaudRate _baud;
        public Parity _parity;
        public SerialDataBits _databits;
        public StopBits _stopbits;
    }

    public enum SerialBaudRate
    {
        BAUD_4800 = 4800,
        BAUD_9600 = 9600,
        BAUD_19200 = 19200,
        BAUD_38400 = 38400,
        BAUD_57600 = 57600,
        BAUD_115200 = 115200
    }

    public enum SerialDataBits
    {
        FIVE = 5,
        SIX = 6,
        SEVEN = 7,
        EIGHT = 8
    }

    public enum SerialPort
    {
        None = 0,
        COM1,
        COM2,
        COM3,
        COM4,
        COM5,
        COM6,
        COM7,
        COM8,
        COM9,
        COM10,
        COM11,
        COM12,
        COM13,
        COM14,
        COM15,
        COM16,
        COM17,
        COM18,
        COM19,
        COM20
    }

    [Serializable]
    public class SerialPortConfigDictionary : IDictionary
    {
        public Hashtable innerHash;

        #region Constructors
        public SerialPortConfigDictionary()
        {
            innerHash = new Hashtable();
        }
        public SerialPortConfigDictionary(SerialPortConfigDictionary original)
        {
            innerHash = new Hashtable(original.innerHash);
        }
        public SerialPortConfigDictionary(IDictionary dictionary)
        {
            innerHash = new Hashtable(dictionary);
        }
        public SerialPortConfigDictionary(int capacity)
        {
            innerHash = new Hashtable(capacity);
        }
        #endregion

        #region Implementation of IDictionary
        public SerialPortConfigDictionaryEnumerator GetEnumerator()
        {
            return new SerialPortConfigDictionaryEnumerator(this);
        }
        System.Collections.IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new SerialPortConfigDictionaryEnumerator(this);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public void Remove(string key)
        {
            innerHash.Remove(key);
        }
        void IDictionary.Remove(object key)
        {
            Remove((string)key);
        }
        public bool Contains(string key)
        {
            return innerHash.Contains(key);
        }
        bool IDictionary.Contains(object key)
        {
            return Contains((string)key);
        }
        public void Clear()
        {
            innerHash.Clear();
        }
        public void Add(string key, SerialPortConfig value)
        {
            innerHash.Add(key, value);
        }
        void IDictionary.Add(object key, object value)
        {
            Add((string)key, (SerialPortConfig)value);
        }
        public bool IsReadOnly
        {
            get
            {
                return innerHash.IsReadOnly;
            }
        }
        public SerialPortConfig this[string key]
        {
            get
            {
                return (SerialPortConfig)innerHash[key];
            }
            set
            {
                innerHash[key] = value;
            }
        }
        object IDictionary.this[object key]
        {
            get
            {
                return this[(string)key];
            }
            set
            {
                this[(string)key] = (SerialPortConfig)value;
            }
        }
        public System.Collections.ICollection Values
        {
            get
            {
                return innerHash.Values;
            }
        }
        public System.Collections.ICollection Keys
        {
            get
            {
                return innerHash.Keys;
            }
        }
        public bool IsFixedSize
        {
            get
            {
                return innerHash.IsFixedSize;
            }
        }
        #endregion

        #region Implementation of ICollection
        public void CopyTo(System.Array array, int index)
        {
            innerHash.CopyTo(array, index);
        }
        public void CopyTo(SerialPortConfigDictionary wsc, int index)
        {
            IEnumerator keys = Keys.GetEnumerator();
            IEnumerator values = Values.GetEnumerator();

            int count = Count;

            for (int i = 0; i < index; i++)
            {
                keys.MoveNext();
                values.MoveNext();
            }

            for (int i = index; i < count; i++)
            {
                keys.MoveNext();
                values.MoveNext();

                wsc.Add(keys.Current as string, values.Current as SerialPortConfig);
            }
        }
        public bool IsSynchronized
        {
            get
            {
                return innerHash.IsSynchronized;
            }
        }
        public int Count
        {
            get
            {
                return innerHash.Count;
            }
        }
        public object SyncRoot
        {
            get
            {
                return innerHash.SyncRoot;
            }
        }
        #endregion
    }

    [Serializable]
    public class SerialPortConfigDictionaryEnumerator : IDictionaryEnumerator
    {
        private IDictionaryEnumerator innerEnumerator;

        internal SerialPortConfigDictionaryEnumerator(SerialPortConfigDictionary enumerable)
        {
            innerEnumerator = enumerable.innerHash.GetEnumerator();
        }

        #region Implementation of IDictionaryEnumerator
        public string Key
        {
            get
            {
                return (string)innerEnumerator.Key;
            }
        }
        object IDictionaryEnumerator.Key
        {
            get
            {
                return Key;
            }
        }
        public SerialPortConfig Value
        {
            get
            {
                return (SerialPortConfig)innerEnumerator.Value;
            }
        }
        object IDictionaryEnumerator.Value
        {
            get
            {
                return Value;
            }
        }
        public System.Collections.DictionaryEntry Entry
        {
            get
            {
                return innerEnumerator.Entry;
            }
        }
        #endregion

        #region Implementation of IEnumerator
        public void Reset()
        {
            innerEnumerator.Reset();
        }
        public bool MoveNext()
        {
            return innerEnumerator.MoveNext();
        }
        object IEnumerator.Current
        {
            get
            {
                return innerEnumerator.Current;
            }
        }
        public DictionaryEntry Current
        {
            get
            {
                return Entry;
            }
        }
        #endregion
    }
    #endregion

    public class LoadPortConfig
    {
        public LoadPortConfig()
        {
            _comm = string.Empty;
            _ackTimeout = -1;
            _infTimeout = -1;
        }

        public LoadPortConfig(string comm, int ackTimeout, int infTimeout)
        {
            _comm = comm;
            _ackTimeout = ackTimeout;
            _infTimeout = infTimeout;
        }

        private string _comm;
        private int _ackTimeout;
        private int _infTimeout;

        public string Comm
        {
            get => _comm;
            set => _comm = value;
        }

        public int ACKTimeout
        {
            get => _ackTimeout;
            set => _ackTimeout = value;
        }

        public int INFTimeout
        {
            get => _infTimeout;
            set => _infTimeout = value;
        }

    }

    public class N2NozzleConfig
    {
        public N2NozzleConfig()
        {
            _comm = string.Empty;
        }

        public N2NozzleConfig(string comm)
        {
            _comm = comm;
        }

        private string _comm;

        public string Comm
        {
            get => _comm;
            set => _comm = value;
        }

    }

    public class DIOConfig
    {
        public DIOConfig()
        {
            _type = -1;
            _index = -1;
            _maxDoPort = -1;
            _maxDiPort = -1;
            _pinCountPerPort = -1;
        }

        public DIOConfig(int type, int index, int maxDoPort, int maxDiPort, int pinCountPerPort)
        {
            _type = type;
            _index = index;
            _maxDoPort = maxDoPort;
            _maxDiPort = maxDiPort;
            _pinCountPerPort = pinCountPerPort;
        }

        private int _type;
        private int _index;
        private int _maxDoPort;
        private int _maxDiPort;
        private int _pinCountPerPort;

        public int Type
        {
            get => _type;
            set => _type = value;
        }

        public int Index
        {
            get => _index;
            set => _index = value;
        }

        public int MaxDOPort
        {
            get => _maxDoPort;
            set => _maxDoPort = value;
        }

        public int MaxDIPort
        {
            get => _maxDiPort;
            set => _maxDiPort = value;
        }

        public int PinCountPerPort
        {
            get => _pinCountPerPort;
            set => _pinCountPerPort = value;
        }

    }
}
