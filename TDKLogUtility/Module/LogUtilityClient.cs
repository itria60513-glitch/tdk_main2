using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading;
using System.Timers;
using System.Xml;
using System.Xml.Linq;
using TDKLogUtility.Module;
using TDKLogUtility.Module_Config;


namespace LogUtility
{
    public class LogUtilityClient : AbstractClient, ILogUtility, IDisposable
    {
        #region Paramters
        private static object instanceLock = new object();
        private static LogUtilityClient uniqueInstance;
        private static PreciseDatetime pDateTime = new PreciseDatetime();
        private ILogUtilityFactory Factory = null;
        private ILogUtility LogUtilityInstance = null;
        private MyCallBackClass myCallback = null;
        private string xmlFilePath = @"D:\TDKConfig\LogConfig.xml";
        private string mainDirectory = @"d:\hmi2010\log";
        private int bufferSize = -1;
        private int maxLogSize = 30;
        private LogInfoDictionary logWriterDict = new LogInfoDictionary();
        private DateTime dtToday = DateTime.Today;
        private Hashtable LogList = null;
        private bool disposed = false;

        private int siNodaysBeforeToday = 7;
        public int iLocalBufferWriteTimeMinute = 10;
        private System.Timers.Timer TimeForceWrites = null;
        private System.Timers.Timer TimeClearLog = null;

        #endregion
        #region Events
        public event ForceWritesEventHandler ForceWritesEvent;
        public event BufferSizeChangedEventHandler BufferSizeChangedEvent;
        public event MainDirectoryChangedEventHandler MainDirectoryChangedEvent;
        public event LogListChangedEventHandler LogListChangedEvent;
        #endregion
        #region Public Properties
        public int BufferSize
        {
            set
            {
                if (bufferSize != value)
                {
                    bufferSize = value;
                    UpdateDictionaryFileName(true);
                }
            }
            get { return bufferSize; }
        }

        public string MainDirectory
        {
            set
            {
                if (mainDirectory != value)
                {
                    mainDirectory = value;
                    UpdateDictionaryFileName(true);
                }
            }
            get { return mainDirectory; }
        }
        public string MainLogDirectory { get; set; }
        public int BufferSizeInKB { get; set; }
        public int DaysForPerservingLog
        {
            get { return siNodaysBeforeToday; }
        }
        public int AutoFlushTimerMinutes
        {
            get { return iLocalBufferWriteTimeMinute; }

            set
            {
                iLocalBufferWriteTimeMinute = value;
                if (TimeForceWrites != null && TimeForceWrites.Interval != iLocalBufferWriteTimeMinute * 60000)
                {
                    TimeForceWrites.Stop();
                    TimeForceWrites.Interval = iLocalBufferWriteTimeMinute * 60000;
                    TimeForceWrites.Start();
                }

                ServiceSelfLog("LogService", LogHeadType.Info, string.Format("AutoForceWriteTimerMinutes = {0}", value));
            }
        }
        public Hashtable ActiveLogList
        {
            set
            {
                Monitor.Enter(logWriterDict.SyncRoot);
                try
                {
                    LogList = value;
                    ArrayList removeList = new ArrayList();
                    foreach (string key in logWriterDict.Keys)
                    {
                        if (!LogList.ContainsKey(key))
                            removeList.Add(key);
                    }
                    for (int i = 0; i < removeList.Count; i++)
                    {
                        string key = (string)removeList[i];
                        ((LogInfo)logWriterDict[key]).Close();
                        logWriterDict.Remove(key);
                    }
                }
                catch { }
                finally
                {
                    Monitor.Exit(logWriterDict.SyncRoot);
                }
            }
            get => LogList;
        }

        public bool IsEnableDebugLog
        {
            get
            {
                if (LogUtilityInstance == null)
                    return false;

                return LogUtilityInstance.IsEnableDebugLog;
            }
        }

        public ILogUtility InstanceLogUtility
        {
            set
            {
                LogUtilityInstance = value;
            }
        }
        public int MaxLogSize
        {
            set
            {
                maxLogSize = value;
                foreach (string key in logWriterDict.Keys)
                {
                    LogInfo LogInfoObj = (LogInfo)logWriterDict[key];
                    Monitor.Enter(LogInfoObj);
                    try
                    {
                        LogInfoObj.MaxLogSize = value;
                    }
                    finally
                    {
                        Monitor.Exit(LogInfoObj);
                    }
                }
            }
            get => maxLogSize;
        }
        #endregion
        #region Constructors
        /// <summary>
        /// This operation implements the logic for returning the unique instance of the Singleton pattern.
        /// </summary>
        new public static LogUtilityClient GetUniqueInstance(string machinename, int port)
        {
            if (uniqueInstance == null)
            {
                lock (instanceLock)
                {
                    if (uniqueInstance == null)
                    {
                        uniqueInstance = new LogUtilityClient(machinename, port);
                    }
                }
            }

            return uniqueInstance;
        }

        /// <summary>
        /// This attribute stores the instance of the Singleton class.
        /// </summary>
        protected LogUtilityClient(string machinename, int port) : base(machinename, port)
        {
            try
            {
                ConnectToServer(machinename, port);
                #region Setup Timer
                if (TimeForceWrites == null)
                {
                    TimeForceWrites = new System.Timers.Timer();
                    TimeForceWrites.Elapsed += new ElapsedEventHandler(TimeForceWrites_Elapsed);
                    TimeForceWrites.Interval = iLocalBufferWriteTimeMinute * 60000;
                    TimeForceWrites.Start();
                }
                if(TimeClearLog == null)
                {
                    TimeClearLog = new System.Timers.Timer();
                    TimeClearLog.Elapsed += TimeClearLog_Elapsed;
                    TimeClearLog.Interval = 864000000; //one day
                    TimeClearLog.Start();
                }
                #endregion
                #region Load log param from xml file
                TdkLogConfigStorage.CreateXmlFile(xmlFilePath);
                var cfg = TdkLogConfigStorage.Load(xmlFilePath);
                mainDirectory = cfg.MainDirectory;
                bufferSize = cfg.BufferSize;
                Hashtable ht = new Hashtable();
                foreach( var name in cfg.LogNames)
                {
                    ht.Add(name, name);
                }
                ActiveLogList = ht;
                siNodaysBeforeToday = cfg.LogDeleteBufferDays;
                AutoFlushTimerMinutes = cfg.AutoFlushPeriod;
                MaxLogSize = cfg.MaxLogSize;
                #endregion Load log param from xml file
            }
            catch (Exception ex)
            {
                string str1 = string.Format("Unable to start proxy of LogUtility application at port {0} on machine {1}.\n{2}", port, machinename, ex.Message);
                throw new ApplicationException(str1);
            }
        }





        public LogUtilityClient() : base("", 0)
        {
            try
            {
                ConnectToServer("", 0);
                #region Setup Timer
                if (TimeForceWrites == null)
                {
                    TimeForceWrites = new System.Timers.Timer();
                    TimeForceWrites.Elapsed += new ElapsedEventHandler(TimeForceWrites_Elapsed);
                    TimeForceWrites.Interval = iLocalBufferWriteTimeMinute * 60000;
                    TimeForceWrites.Start();
                }
                if (TimeClearLog == null)
                {
                    TimeClearLog = new System.Timers.Timer();
                    TimeClearLog.Elapsed += TimeClearLog_Elapsed;
                    TimeClearLog.Interval = 864000000; //one day
                    TimeClearLog.Start();
                }
                #endregion
                #region Load log param from xml file
                TdkLogConfigStorage.CreateXmlFile(xmlFilePath);
                var cfg = TdkLogConfigStorage.Load(xmlFilePath);
                mainDirectory = cfg.MainDirectory;
                bufferSize = cfg.BufferSize;
                Hashtable ht = new Hashtable();
                foreach (var name in cfg.LogNames)
                {
                    ht.Add(name, name);
                }
                ActiveLogList = ht;
                siNodaysBeforeToday = cfg.LogDeleteBufferDays;
                AutoFlushTimerMinutes = cfg.AutoFlushPeriod;
                MaxLogSize = cfg.MaxLogSize;
                #endregion Load log param from xml file
            }
            catch (Exception ex)
            {
                string str1 = string.Format("Unable to start proxy of LogUtility application at port {0} on machine {1}.\n{2}", "", 0, ex.Message);
                throw new ApplicationException(str1);
            }
        }

        ~LogUtilityClient()
        {
            if (TimeForceWrites != null)
            {
                TimeForceWrites.Elapsed -= new ElapsedEventHandler(TimeForceWrites_Elapsed);
                TimeForceWrites.Close();
            }

            if (TimeClearLog != null)
            {
                TimeClearLog.Elapsed -= TimeClearLog_Elapsed;
                TimeClearLog.Close();
            }
            
            Dispose(false);
        }
        #endregion Constructors
        override public ILogUtility CreateInstance()
        {
            try
            {
                return Factory.CreateInstance();
            }
            catch (Exception ex)
            {
                string str1 = string.Format("Unable to connect LogUtility application.\n{0}", ex.Message);
                throw new ApplicationException(str1);
            }
        }

        private void ConnectToServer(string machinename, int port)
        {
            if (LogUtilityInstance == null)
            {
                Process[] proc = Process.GetProcessesByName("LogUtility");
                if (proc.Length > 0)
                {
                    IDictionary props = new Hashtable();
                    props["name"] = "LogUtilityClient";
                    props["port"] = 0;
                    string strLocal = DnsSvr.ResolveIP(Environment.MachineName);
                    if (strLocal != null)
                    {
                        props["machineName"] = strLocal;   // Avoid slow DNS domain name resolution
                        props["bindTo"] = strLocal;        // Select NIC to bind with TransportSink
                    }
                    BinaryServerFormatterSinkProvider SrvFormatter = new BinaryServerFormatterSinkProvider();
                    SrvFormatter.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;
                    TcpChannel channel = new TcpChannel(props, new BinaryClientFormatterSinkProvider(), SrvFormatter);
                    ChannelServices.RegisterChannel(channel, false);

                    string str = String.Format("tcp://{0}:{1}/LogFactory", machinename, port);
                    Factory = (ILogUtilityFactory)Activator.GetObject(typeof(ILogUtilityFactory), str);
                    LogUtilityInstance = (ILogUtility)this.CreateInstance();
                    mainDirectory = LogUtilityInstance.MainLogDirectory;
                    bufferSize = LogUtilityInstance.BufferSizeInKB;
                    LogList = LogUtilityInstance.ActiveLogList;
                    myCallback = new MyCallBackClass(this);
                    LogUtilityInstance.ForceWritesEvent += new ForceWritesEventHandler(myCallback.ForceWritesEvent);
                    LogUtilityInstance.BufferSizeChangedEvent += new BufferSizeChangedEventHandler(myCallback.BufferSizeChangedEvent);
                    LogUtilityInstance.MainDirectoryChangedEvent += new MainDirectoryChangedEventHandler(myCallback.MainDirectoryChangedEvent);
                    LogUtilityInstance.LogListChangedEvent += new LogListChangedEventHandler(myCallback.LogListChangedEvent);
                }
                else
                {
                    GetMainDirectory();
                    bufferSize = 1;
                    LogList = Hashtable.Synchronized(new Hashtable());
                }
            }
        }

        public bool WriteLogWithSecured(string szLogKey, LogHeadType enLogType, LogCateType enCateType, string szLogMessage, string[] SecuredSections, string szRemark = null)
        {
            StringBuilder SecuredLogMessage = new StringBuilder(szLogMessage);
            foreach (string KeyWord in SecuredSections)
            {
                if (!String.IsNullOrWhiteSpace(KeyWord))
                    SecuredLogMessage = SecuredLogMessage.Replace(KeyWord, SecuredKeyword(KeyWord));
            }

            WriteLog(szLogKey, enLogType, enCateType, SecuredLogMessage.ToString(), szRemark);

            return true;
        }

        public bool WriteLogWithSecured(string szLogKey, LogHeadType enLogType, string szLogMessage, string[] SecuredSections, string szRemark = null)
        {
            WriteLogWithSecured(szLogKey, enLogType, LogCateType.None, szLogMessage, SecuredSections, szRemark);
            return true;
        }

        private string SecuredKeyword(string Keyword)
        {
            return (LogStringAttribute.GetLogHeadString(LogHeadType.Secured, Keyword));
        }

        public bool WriteLog(string szLogKey, LogHeadType enLogType, LogCateType enCateType, string szLogMessage, string szRemark = null)
        {
            if (bufferSize <= 0)
                return false;

            if (dtToday != DateTime.Today)
                UpdateDictionaryFileName(false);

            if (LogUtilityInstance == null && !LogList.ContainsKey(szLogKey))
                LogList[szLogKey.ToUpper()] = szLogKey;

            szLogKey = szLogKey.ToUpper();
            LogInfo log = null;
            Monitor.Enter(logWriterDict.SyncRoot);
            try
            {
                if (!logWriterDict.ContainsKey(szLogKey))
                {
                    if (LogList != null && LogList.ContainsKey(szLogKey))
                    {
                        log = new LogInfo(mainDirectory, (string)LogList[szLogKey], true, bufferSize);
                        logWriterDict[szLogKey] = log;
                        if (LogUtilityInstance == null)
                            log.WriteLine(FormatMessage(LogHeadType.Info, LogCateType.None, "****** LogUtility Offline Mode ******"));
                    }
                }
                else
                {
                    log = (LogInfo)logWriterDict[szLogKey];
                }

                if (log == null) throw new NullReferenceException($"{nameof(LogInfo)} can't be null");

                log.WriteLine(FormatMessage(enLogType, enCateType, szLogMessage, szRemark));
                if (LogUtilityInstance == null)
                    log.Flush();
            }
            catch { }
            finally
            {
                Monitor.Exit(logWriterDict.SyncRoot);
                
            }
            return true;
        }

        public virtual bool WriteLog(string szLogKey, LogHeadType enLogType, string szLogMessage, string szRemark = null)
        {
            WriteLog(szLogKey, enLogType, LogCateType.None, szLogMessage, szRemark);
            return true;
        }
        public virtual bool WriteLog(string szLogKey, LogHeadType enLogType, string szLogMessage)
        {
            WriteLog(szLogKey, enLogType, LogCateType.None, szLogMessage, null);
            return true;
        }
        public bool WriteLog(string szLogKey, string szLogMessage)
        {
            if (bufferSize <= 0)
                return false;

            if (dtToday != DateTime.Today)
                UpdateDictionaryFileName(false);

            if (LogUtilityInstance == null && !LogList.ContainsKey(szLogKey))
                LogList[szLogKey.ToUpper()] = szLogKey;

            szLogKey = szLogKey.ToUpper();
            LogInfo log = null;
            Monitor.Enter(logWriterDict.SyncRoot);
            try
            {
                if (!logWriterDict.ContainsKey(szLogKey))
                {
                    if (LogList != null && LogList.ContainsKey(szLogKey))
                    {
                        log = new LogInfo(mainDirectory, (string)LogList[szLogKey], true, bufferSize);
                        logWriterDict[szLogKey] = log;
                        if (LogUtilityInstance == null)
                            log.WriteLine(FormatMessage("****** LogUtility Offline Mode ******"));
                    }
                    else
                    {
                       
                        return false;
                    }
                }
                else
                {
                    log = (LogInfo)logWriterDict[szLogKey];
                }

                if (log == null) throw new NullReferenceException($"{nameof(LogInfo)} can't be null");

                log.WriteLine(FormatMessage(szLogMessage));
                if (LogUtilityInstance == null)
                    log.Flush();
            }
            catch { }
            finally
            {
                Monitor.Exit(logWriterDict.SyncRoot);
            }

            return true;
        }

        public void WriteData(string szLogKey, string szLogMessage, byte[] byPtBuf, int length)
        {
            StringBuilder sb = new StringBuilder();
            //sb.Append("[DATA] " + szLogMessage + " ");
            sb.Append(szLogMessage + " ");
            for (int i = 0; i < Math.Min(byPtBuf.Length, length); i++)
            {
                if ((byPtBuf[i] >= (byte)33 && byPtBuf[i] <= (byte)122) || byPtBuf[i] == (byte)0x20)
                    sb.AppendFormat("{0}", (char)byPtBuf[i]);
                else
                    sb.AppendFormat("({0})", byPtBuf[i].ToString("X"));
            }
            sb.AppendFormat("  Length:{0}", Math.Min(byPtBuf.Length, length));
            WriteLog(szLogKey, LogHeadType.Data, sb.ToString());
        }

        public void SystemEventLog(int EventId, string Msg)
        {
            if (LogUtilityInstance != null)
            {
                if (EventId == 199)
                {
                    //RMS data
                    LogUtilityInstance.WriteLog("RMSData", LogHeadType.Data, Msg);
                }
                else
                {
                    LogUtilityInstance.WriteLog("SystemEvents", LogHeadType.Event, Msg, EventId.ToString("00"));
                }
            }
            //LogUtilityInstance.WriteLog("SystemEvents", string.Format("Evt:{0} {1}", EventId.ToString("00"), Msg));
        }

        public void SystemEventLogWithSecured(int EventId, string szLogMessage, string[] SecuredSections)
        {
            if (LogUtilityInstance != null)
                LogUtilityInstance.WriteLogWithSecured("SystemEvents", LogHeadType.Event, szLogMessage, SecuredSections, EventId.ToString("00"));
        }
        private void ServiceSelfLog(string szLogKey, LogHeadType enLogType, string szLogMessage, string szRemark = null)
        {
            WriteLog(szLogKey, enLogType, szLogMessage, szRemark);
        }
        //Obsolete method
        [System.Obsolete("use WriteLogViaServer(string szLogKey, LogHeadType enLogType, string szLogMessage)", false)]
        public void WriteLogViaServer(string szLogKey, string szLogMessage)
        {
            if (LogUtilityInstance != null)
                LogUtilityInstance.WriteLog(szLogKey, szLogMessage);
        }

        public void WriteLogViaServer(string szLogKey, LogHeadType enLogType, string szLogMessage, string szRemark = null)
        {
            if (LogUtilityInstance != null)
                LogUtilityInstance.WriteLog(szLogKey, enLogType, szLogMessage, szRemark);
        }

        public void WriteLogViaServer(string szLogKey, LogHeadType enLogType, LogCateType enCateType, string szLogMessage, string szRemark = null)
        {
            if (LogUtilityInstance != null)
                LogUtilityInstance.WriteLog(szLogKey, enLogType, enCateType, szLogMessage, szRemark);
        }

        public void WriteLogWithSecuredViaServer(string szLogKey, LogHeadType enLogType, string szLogMessage, string[] SecuredSections, string szRemark = null)
        {
            if (LogUtilityInstance != null)
                LogUtilityInstance.WriteLogWithSecured(szLogKey, enLogType, szLogMessage, SecuredSections, szRemark);
        }

        public void WriteLogWithSecuredViaServer(string szLogKey, LogHeadType enLogType, LogCateType enCateType, string szLogMessage, string[] SecuredSections, string szRemark = null)
        {
            if (LogUtilityInstance != null)
                LogUtilityInstance.WriteLogWithSecured(szLogKey, enLogType, enCateType, szLogMessage, SecuredSections, szRemark);
        }

        public void ClearLogs()
        {
            if (string.IsNullOrWhiteSpace(mainDirectory))
                return;
                ServiceSelfLog("LogService", LogHeadType.MethodEnter, "ClearLogs check DaysForPerservingLog = " + DaysForPerservingLog.ToString());

                

                DateTime LastDate, CurDate, date1;
                // get the current date
                CurDate = DateTime.Now.Date;
                date1 = CurDate.AddDays(-DaysForPerservingLog);

                DirectoryInfo dir = new DirectoryInfo(mainDirectory);
                foreach (FileInfo f in dir.GetFiles("*.log", SearchOption.TopDirectoryOnly))
                {
                    // get the last write datetime
                    LastDate = f.LastWriteTime;
                    if (LastDate < date1)
                    {
                        ServiceSelfLog("LogService", LogHeadType.Info, "ClearLogs start DaysForPerservingLog = " + DaysForPerservingLog.ToString());
                        try
                        {
                            ServiceSelfLog("LogService", LogHeadType.Info, string.Format("Delete file: {0}, LastWriteTime:{1}", f.FullName, LastDate));
                            File.Delete(f.FullName);
                            ServiceSelfLog("LogService", LogHeadType.Info, "  ==> Success");
                        }
                        catch (Exception ex)
                        {
                            string msg = ex.Message;
                            ServiceSelfLog("LogService", LogHeadType.Info, "  ==> Fail. Reason: " + ex.Message);
                        }
                        ServiceSelfLog("LogService", LogHeadType.Info, "ClearLogs end DaysForPerservingLog = " + DaysForPerservingLog.ToString());
                    }
                }

                ServiceSelfLog("LogService", LogHeadType.MethodExit, "ClearLogs()");
        }

        public void UpdateLogSetting(string  mainDir,int keepDay,int flushPeriod)
        {
            MainDirectory = mainDir;
            siNodaysBeforeToday = keepDay;
            AutoFlushTimerMinutes = flushPeriod;
            var cfg = TdkLogConfigStorage.Load(xmlFilePath);
            cfg.MainDirectory = mainDir;
            cfg.LogDeleteBufferDays = keepDay;
            cfg.AutoFlushPeriod = flushPeriod;
            TdkLogConfigStorage.Save(xmlFilePath, cfg);
        }
        public void UpdateLogInfo(int maxLogSize,int bufferSize)
        {
            MaxLogSize = maxLogSize;
            BufferSize = bufferSize;
            var cfg = TdkLogConfigStorage.Load(xmlFilePath);
            cfg.MaxLogSize = maxLogSize;
            cfg.BufferSize = bufferSize;
            TdkLogConfigStorage.Save(xmlFilePath, cfg);
        }
        public void ApplyLogSetting(string mainDir, int keepDay, int flushPeriod)
        {
            MainDirectory = mainDir;
            siNodaysBeforeToday = keepDay;
            AutoFlushTimerMinutes = flushPeriod;
        }
        public void ApplyLogInfo(int maxLogSize, int bufferSize)
        {
            MaxLogSize = maxLogSize;
            BufferSize = bufferSize;
        }
        public void ResetLogParams()
        {
            var cfg = TdkLogConfigStorage.Load(xmlFilePath);
            cfg.MainDirectory = "D:\\TDKLog\\";
            cfg.LogDeleteBufferDays = 120;
            cfg.AutoFlushPeriod = 10;
            cfg.MaxLogSize = 30;
            cfg.BufferSize = 8;
            TdkLogConfigStorage.Save(xmlFilePath, cfg);
            ApplyLogSetting(cfg.MainDirectory, cfg.LogDeleteBufferDays, cfg.AutoFlushPeriod);
            ApplyLogInfo(cfg.MaxLogSize, cfg.BufferSize);
        }
        
        #region LogUtility Callback Implementations
        public void ForceWritesImpl()
        {
            TimeForceWrites.Stop();

            if (dtToday != DateTime.Today)
                UpdateDictionaryFileName(false);

            Monitor.Enter(logWriterDict.SyncRoot);
            try
            {
                foreach (string szLogKey in logWriterDict.Keys)
                    ((LogInfo)logWriterDict[szLogKey]).Flush();
            }
            finally
            {
                Monitor.Exit(logWriterDict.SyncRoot);
            }
            TimeForceWrites.Start();
        }
        #endregion

        #region Private Methods
        private void UpdateDictionaryFileName(bool updateAnyway)
        {
            Monitor.Enter(logWriterDict.SyncRoot);
            try
            {
                if (!updateAnyway && dtToday == DateTime.Today)
                    return;

                foreach (string key in logWriterDict.Keys)
                {
                    LogInfo LogInfoObj = (LogInfo)logWriterDict[key];
                    Monitor.Enter(LogInfoObj);
                    try
                    {
                        LogInfoObj.BufferSizeInKB = bufferSize;
                        LogInfoObj.CreateStreamWriter(mainDirectory, LogInfoObj.Key);
                    }
                    finally
                    {
                        Monitor.Exit(LogInfoObj);
                    }
                }
                //update current date
                dtToday = DateTime.Today;
            }
            catch { }
            finally
            {
                Monitor.Exit(logWriterDict.SyncRoot);
            }
        }


        private string FormatMessage(LogHeadType eLogType, LogCateType enCateType, string szLogMessage, string szRemark = null)
        {
            string szConvertedString;
            DateTime dtNow = pDateTime.Now;
            String format = "MM/dd/yyyy HH:mm:ss:fffffff";
            String CateString = (enCateType == LogCateType.None) ? "" : LogStringAttribute.GetLogHeadString(enCateType);
            szConvertedString = dtNow.ToString(format);
            int tId = Thread.CurrentThread.ManagedThreadId;

            szConvertedString = szConvertedString + " : " +
                                LogStringAttribute.GetLogHeadString(eLogType, szRemark) +
                                CateString + " " +
                                szLogMessage;
            szConvertedString = szConvertedString + " , ThreadId : " + tId;
            return szConvertedString;
        }

        //Obsolete method
        [System.Obsolete("use FormatMessage(LogHeadType eLogType, LogCateType enCateType, string szLogMessage)", false)]
        private string FormatMessage(string szLogMessage)
        {
            string szConvertedString;
            DateTime dtNow = pDateTime.Now;
            String format = "MM/dd/yyyy HH:mm:ss:fffffff";
            szConvertedString = dtNow.ToString(format);
            int tId = Thread.CurrentThread.ManagedThreadId;
            szConvertedString = szConvertedString + " : " + szLogMessage + " , ThreadId : " + tId;
            //szConvertedString = szConvertedString + " : " + szLogMessage;
            return szConvertedString;
        }

        private void GetMainDirectory()
        {
            string m_szLogXML = @"d:\hmi2010\Config\System\Log.xml";
            if (File.Exists(m_szLogXML))
            {
                XmlTextReader reader = new XmlTextReader(m_szLogXML);
                XDocument xdoc = XDocument.Load(reader);
                reader.Close();

                IEnumerable<XNode> paraNodes =
                    from para in xdoc.Elements("LogParam").Nodes()
                    where para.NodeType == XmlNodeType.Element && ((XElement)para).Name.ToString().Equals("MainDirectory")
                    select para;
                if (paraNodes != null)
                {
                    XElement xe = (XElement)paraNodes.First();
                    mainDirectory = xe.Value;
                }
            }

            string pathRoot;
            pathRoot = Path.GetPathRoot(mainDirectory);
            if (Directory.Exists(pathRoot))
            {
                System.IO.DriveInfo drive = new DriveInfo(pathRoot);
                if (drive.DriveType != DriveType.Fixed)
                {
                    throw new Exception(string.Format("Log root directory {0} has to be on fixed hard dirve!", pathRoot));
                }
                if (!Directory.Exists(mainDirectory))
                {
                    Directory.CreateDirectory(mainDirectory);
                }
            }
            else
            {
                throw new Exception(string.Format("Root directory {0} does not exist!", pathRoot));
            }
        }
        #endregion

        #region Timer Callback Implementation
        void TimeForceWrites_Elapsed(object sender, ElapsedEventArgs e)
        {
            ForceWritesImpl();
        }
        private void TimeClearLog_Elapsed(object sender, ElapsedEventArgs e)
        {
            ClearLogs();
        }
        #endregion
        
        #region " Implement Dispose "

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (LogUtilityInstance != null)
                    {
                        LogUtilityInstance.ForceWritesEvent -= new ForceWritesEventHandler(myCallback.ForceWritesEvent);
                        LogUtilityInstance.BufferSizeChangedEvent -= new BufferSizeChangedEventHandler(myCallback.BufferSizeChangedEvent);
                        LogUtilityInstance.MainDirectoryChangedEvent -= new MainDirectoryChangedEventHandler(myCallback.MainDirectoryChangedEvent);
                        LogUtilityInstance.LogListChangedEvent -= new LogListChangedEventHandler(myCallback.LogListChangedEvent);
                    }

                    uniqueInstance = null;

                    // Call the appropriate methods to clean up
                    // unmanaged resources here.
                    // If disposing is false,
                    // only the following code is executed.

                    try
                    {
                        ForceWritesImpl();
                        foreach (LogInfo log in logWriterDict.Values)
                            log.Close();
                    }
                    catch { }

                    // Note disposing has been done.
                    disposed = true;
                }
            }
        }

        #endregion
    }

    public class MyCallBackClass : RemotelyDelegatableObject
    {
        LogUtilityClient client = null;

        public MyCallBackClass(LogUtilityClient parent)
        {
            client = parent;
        }

        protected override void InternalForceWritesEvent()
        {
            client.ForceWritesImpl();
        }
        protected override void InternalBufferSizeChangedEvent(int size)
        {
            client.BufferSize = size;
        }
        protected override void InternalMainDirectoryChangedEvent(string directory)
        {
            client.MainDirectory = directory;
        }
        protected override void InternalLogListChangedEvent(Hashtable list)
        {
            if (list != null)
                client.ActiveLogList = list;
            else
                client.ActiveLogList = new Hashtable();
        }
    }

    public class PreciseDatetime
    {
        // using DateTime.Now resulted in many many log events with the same timestamp.
        // use static variables in case there are many instances of this class in use in the same program
        // (that way they will all be in sync)
        private static readonly Stopwatch myStopwatch = new Stopwatch();
        private static System.DateTime myStopwatchStartTime;

        static PreciseDatetime()
        {
            Reset();

            try
            {
                // In case the system clock gets updated
                SystemEvents.TimeChanged += SystemEvents_TimeChanged;
            }
            catch (Exception)
            {
            }
        }

        static void SystemEvents_TimeChanged(object sender, EventArgs e)
        {
            Reset();
        }

        static public void Reset()
        {
            myStopwatchStartTime = System.DateTime.Now;
            myStopwatch.Restart();
        }

        public System.DateTime Now { get { return myStopwatchStartTime.Add(myStopwatch.Elapsed); } }
    }
}
