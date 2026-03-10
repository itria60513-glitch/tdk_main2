using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using EFEM.DataCenter;

namespace EFEM.LogUtilities
{
    #region delegates
    public delegate void ForceWritesEventHandler();
    public delegate void BufferSizeChangedEventHandler(int bufferSize);
    public delegate void MainDirectoryChangedEventHandler(string directory);
    public delegate void LogListChangedEventHandler(Hashtable list);
    #endregion

    [AttributeUsage(AttributeTargets.Field)]
    public class LogStringAttribute : Attribute
    {
        public string Representation;
        private static Dictionary<LogHeadType, string> headLT = new Dictionary<LogHeadType, string>();

        internal LogStringAttribute(string representation)
        {
            this.Representation = representation;
        }

        static LogStringAttribute()
        {
            initHeadLookup();
        }

        public override string ToString()
        {
            return this.Representation;
        }

        private static void initHeadLookup()
        {
            headLT.Clear();

            foreach (LogHeadType head in Enum.GetValues(typeof(LogHeadType)))
            {
                string sHead = "";
                MemberInfo[] members = typeof(LogHeadType).GetMember(head.ToString());
                if (members != null && members.Length > 0)
                {
                    object[] attributes = members[0].GetCustomAttributes(typeof(LogStringAttribute), false);
                    if (attributes.Length > 0)
                        sHead = ((LogStringAttribute)attributes[0]).ToString();
                    else
                        sHead = "";
                }
                else
                    sHead = "";

                headLT.Add(head, sHead);
            }
        }

        public static string LookUpHead(LogHeadType logHeadType)
        {
            return headLT[logHeadType];
        }

        public static string GetLogHeadString(LogHeadType logHeadType, string Remark = null)
        {
            if (string.IsNullOrWhiteSpace(Remark))
                return ("[" + headLT[logHeadType] + "]");
            else
                return ("[" + headLT[logHeadType] + ": " + Remark + "]");
        }

        //public static string GetLogHeadString(LogHeadType logHeadType, string Remark = null)
        //{
        //    MemberInfo[] members = typeof(LogHeadType).GetMember(logHeadType.ToString());
        //    return (GetMemberString(members, Remark));
        //}

        //private static string GetMemberString(MemberInfo[] members, string Remark = null)
        //{
        //    if (members != null && members.Length > 0)
        //    {
        //        object[] attributes = members[0].GetCustomAttributes(typeof(LogStringAttribute), false);
        //        if (attributes.Length > 0)
        //        {
        //            if (string.IsNullOrWhiteSpace(Remark))
        //                return ("[" + (LogStringAttribute)attributes[0]).ToString() + "]";
        //            else
        //                return ("[" + (LogStringAttribute)attributes[0]).ToString() + ": " + Remark + "]";
        //        }
        //        else
        //        {
        //            return "";
        //        }
        //    }
        //    else
        //        return "";
        //}
    }

    #region LogHeadType
    public enum LogHeadType
    {
        [LogString("")]
        None = 0,
        [LogString("-------------------- NEW START -------------------")]
        System_NewStart,
        [LogString("--------------------- RESTART --------------------")]
        System_Restart,
        [LogString("---------------------- RESET ---------------------")]
        System_Reset,
        [LogString("INIT")]
        Initialize,
        [LogString("EQUIPMENT INITIATED")]
        Initialize_Equipment,
        [LogString("USER INITIATED")]
        Initialize_User,
        [LogString("INFO")]
        Info,
        [LogString("ERROR")]
        Error,
        [LogString("WARNING")]
        Warning,
        [LogString("ALARM")]
        Alarm,
        [LogString("EXCEPTION")]
        Exception,
        [LogString("SOFTWARE_EXCEPTION")]
        Exception_Software,
        [LogString("EXCEPTION SET")]
        Exception_Set,
        [LogString("EXCEPTION CLEAR")]
        Exception_Clear,
        [LogString("EX_NOTIFY")]
        Exception_Notify,
        [LogString("EX_WARNING")]
        Exception_Warning,
        [LogString("EX_ERROR")]
        Exception_Error,
        [LogString("EX_ALARM")]
        Exception_Alarm,
        [LogString("NOTIFY")]
        Notify,
        [LogString("KEEP EYES ON")]
        KeepEyesOn,
        [LogString("ABORT")]
        Abort,
        [LogString("EVENT")]
        Event,
        [LogString("METHOD")]
        MethodEnter,
        [LogString("END_METHOD")]
        MethodExit,
        [LogString("CALL")]
        CallStart,
        [LogString("END_CALL")]
        CallEnd,
        [LogString("REGION")]
        RegionStart,
        [LogString("END_REGION")]
        RegionEnd,
        [LogString("DATA")]
        Data,
        [LogString("DATA RECEIVED")]
        Data_Received,
        [LogString("DATA SENT")]
        Data_Sent,
        [LogString("*SECURED")]
        Secured,
    }
    #endregion LogHeadType

    #region LogLevel
    public enum LogLevel
    {
        [HMIDescription("0: Disable")]
        OFF = 0,
        [HMIDescription("1: Error")]
        ERROR = 1,
        [HMIDescription("2: Warning")]
        WARN = 2,
        [HMIDescription("3: Info")]
        INFO = 3,
        [HMIDescription("4: Debug")]
        DEBUG = 4,
        [HMIDescription("5: All")]
        ALL = 5,
    }
    #endregion

    public interface AbstractLogUtility : IDisposable
    {
        LogLevel LogLevelMode { get; set; }
        Hashtable ActiveLogList { get; }
        int AutoFlushTimerMinutes { get; set; }
        int BufferSizeInKB { get; set; }
        int DaysForPerservingLog { get; }
        string MainLogDirectory { get; }

        event BufferSizeChangedEventHandler BufferSizeChangedEvent;
        event ForceWritesEventHandler ForceWritesEvent;
        event LogListChangedEventHandler LogListChangedEvent;
        event MainDirectoryChangedEventHandler MainDirectoryChangedEvent;

        void ForceWrite();
        void ClearLogs();
        XDocument GetLogListXDocument();
        XDocument GetLogParamXDocument();
        string SetLogListXDocument(XDocument xdoc);
        void SetLogParamXDocument(XDocument xdoc);
        bool WriteLog(string szKey, LogLevel level, LogHeadType enLogType, string szLogMessage, string szRemark = null);
        bool WriteLogWithSecured(string szLogKey, LogLevel lwvel, LogHeadType enLogType, string szLogMessage, string[] SecuredSections, string szRemark = null);
    }

    [Serializable]
    public class LogInfoDictionary : IDictionary
    {
        public Hashtable innerHash;

        #region Constructors
        public LogInfoDictionary()
        {
            innerHash = Hashtable.Synchronized(new Hashtable());
        }
        public LogInfoDictionary(LogInfoDictionary original)
        {
            innerHash = Hashtable.Synchronized(new Hashtable(original.innerHash));
        }
        public LogInfoDictionary(IDictionary dictionary)
        {
            innerHash = Hashtable.Synchronized(new Hashtable(dictionary));
        }
        public LogInfoDictionary(int capacity)
        {
            innerHash = Hashtable.Synchronized(new Hashtable(capacity));
        }
        #endregion

        #region Implementation of IDictionary
        public LogInfoDictionaryEnumerator GetEnumerator()
        {
            return new LogInfoDictionaryEnumerator(this);
        }
        System.Collections.IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new LogInfoDictionaryEnumerator(this);
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

        public bool ContainsKey(string key)
        {
            return innerHash.ContainsKey(key);
        }


        bool IDictionary.Contains(object key)
        {
            return Contains((string)key);
        }
        public void Clear()
        {
            innerHash.Clear();
        }
        public void Add(string key, LogInfo value)
        {
            innerHash.Add(key, value);
        }
        void IDictionary.Add(object key, object value)
        {
            Add((string)key, (LogInfo)value);
        }
        public bool IsReadOnly
        {
            get
            {
                return innerHash.IsReadOnly;
            }
        }
        public LogInfo this[string key]
        {
            get
            {
                return (LogInfo)innerHash[key];
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
                this[(string)key] = (LogInfo)value;
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
        public void CopyTo(LogInfoDictionary wsc, int index)
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

                wsc.Add(keys.Current as string, values.Current as LogInfo);
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
    public class LogInfoDictionaryEnumerator : IDictionaryEnumerator
    {
        private IDictionaryEnumerator innerEnumerator;

        internal LogInfoDictionaryEnumerator(LogInfoDictionary enumerable)
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
        public LogInfo Value
        {
            get
            {
                return (LogInfo)innerEnumerator.Value;
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

    public class LogInfo
    {
        private StreamWriter Writer = null;
        private bool bLogRequired;
        int logPart = 0;
        private long totalLength = 0;
        private int logMaxLength = 30 * 1024 * 1024;//30MB
        private int bufferSize = 4 * 1024;//4KB
        private string logPath;
        private string logKey;

        public LogInfo(string sPath, string sKeyName, bool bLogReqd, int bufferSizeInKB)
        {
            bLogRequired = bLogReqd;
            BufferSizeInKB = bufferSizeInKB;
            CreateStreamWriter(sPath, sKeyName);
        }

        #region Public Methods
        public void Close()
        {
            if (Writer != null)
            {
                Writer.Flush();
                Writer.Close();
                Writer = null;
            }
        }

        public void WriteLine(string sMessage)
        {
            if (Writer == null) return;
            Writer.WriteLine(sMessage);

            totalLength += sMessage.Length;
            if (totalLength >= logMaxLength)
                CreateStreamWriter(logPath, logKey);
        }

        public void Flush()
        {
            Writer.Flush();
        }

        public void CreateStreamWriter(string sPath, string sKeyName)
        {
            logPath = sPath;
            logKey = sKeyName;

            if (Writer != null)
                this.Close();
            if (!bLogRequired)
                return;

            string szFileName = GetCurrentDayFileNameNoExt(sPath, sKeyName) + ".log";
            if (!File.Exists(szFileName))
            {
                logPart = 0;
                Writer = new StreamWriter(File.Open(szFileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read), System.Text.Encoding.UTF8, bufferSize);
            }
            else
            {
                while (true)
                {
                    if (logPart == 0)
                    {
                        FileInfo info = new FileInfo(szFileName);
                        if (info.Length > logMaxLength)
                            logPart++;
                        else
                        {
                            Writer = new StreamWriter(File.Open(szFileName, FileMode.Append, FileAccess.Write, FileShare.Read),
                                                        System.Text.Encoding.UTF8,
                                                        bufferSize);
                            break;
                        }
                    }
                    else
                    {
                        string path_filename = GetCurrentDayFileNameNoExt(sPath, sKeyName) + "_" + logPart.ToString("000") + ".log";
                        if (File.Exists(path_filename))
                        {
                            FileInfo info = new FileInfo(path_filename);
                            if (info.Length > logMaxLength)
                                logPart++;
                            else
                            {
                                Writer = new StreamWriter(File.Open(path_filename, FileMode.Append, FileAccess.Write, FileShare.Read),
                                                            System.Text.Encoding.UTF8,
                                                            bufferSize);
                                break;
                            }
                        }
                        else
                        {
                            Writer = new StreamWriter(File.Open(path_filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read),
                                                        System.Text.Encoding.UTF8,
                                                        bufferSize);
                            break;
                        }
                    }
                }
            }
            totalLength = Writer.BaseStream.Length;
        }
        #endregion

        #region Private Methods
        private string GetCurrentDayFileNameNoExt(string sPath, string sLogName)
        {
            return sPath + "\\" + sLogName + "_" + DateTime.Now.Date.ToString("u").Substring(0, 10);
        }
        #endregion

        #region Public Properties
        public bool LogRequired
        {
            get { return bLogRequired; }
            set { bLogRequired = value; }
        }

        public int BufferSizeInKB
        {
            set { bufferSize = value * 1024; }
        }

        public string Key
        {
            get { return logKey; }
        }
        #endregion
    }

}
