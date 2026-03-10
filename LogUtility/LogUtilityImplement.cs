using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.IO;
using System.Timers;
using System.Diagnostics;
using System.Threading;
using System.Reflection;
using Microsoft.Win32;
using EFEM.DataCenter;

namespace EFEM.LogUtilities
{
    public class LogUtility : AbstractLogUtility
    {
        private static AbstractLogUtility uniqueInstance = null;

        #region Even Declaration
        public event ForceWritesEventHandler ForceWritesEvent = null;
        public event BufferSizeChangedEventHandler BufferSizeChangedEvent = null;
        public event MainDirectoryChangedEventHandler MainDirectoryChangedEvent = null;
        public event LogListChangedEventHandler LogListChangedEvent = null;
        #endregion

        #region Data Members
        private bool disposed = false;
        private LogLevel _curLogLevel = LogLevel.ALL;
        public string sMainDirectory = "";
        public int siNodaysBeforeToday = 7;
        public int iLocalBufferKB = -1;
        public int iLocalBufferWriteTimeMinute = 10;

        private object LockLogXML = new object();
        private string m_szLogXML = ConstVC.LogConfig.LogXMLFileFullName;
        private string m_szLogXMLSchema = ConstVC.LogConfig.LogXSDFileFullName;

        private object LockDelete = new object();
        private object LockLogObjectXML = new object();
        private string m_szLogObjectXML = ConstVC.LogConfig.LogObjectXMLFileFullName;
        private string m_szLogObjectXMLSchema = ConstVC.LogConfig.LogObjectXSDFileFullName;
        private LogInfoDictionary LogInfoDictionaryObj = new LogInfoDictionary();
        private Hashtable LogList = null;

        private DateTime dtToday = DateTime.Today;
        private System.Timers.Timer TimeForceWrites = null;
        private System.Timers.Timer TimeForceClear = null;
        private PreciseDatetime pDateTime = new PreciseDatetime();

        //HashSet has the 100000 times efficiency than List<>
        //private HashSet<string> headLookup = new HashSet<string>();
        #endregion

        #region Constructor

        public static AbstractLogUtility GetUniqueInstance()
        {
            if (uniqueInstance == null)
            {
                uniqueInstance = new LogUtility();
                ((LogUtility)uniqueInstance).ServiceSelfLog(LogLevel.DEBUG, LogHeadType.System_NewStart, null);
            }
            return uniqueInstance;
        }

        public LogLevel LogLevelMode
        {
            get { return this._curLogLevel; }
            set 
            { 
                this._curLogLevel = value;
                ServiceSelfLog(LogLevel.INFO, LogHeadType.Info, string.Format("LogLevelMode = {0}:{1}", (int)value, value));
            }
        }

        private bool NeedLog(LogLevel level)
        {
            if (LogLevelMode == LogLevel.OFF || level == LogLevel.OFF)
                return false;
            else if (level == LogLevel.ALL)
                return true;
            else
                return (LogLevelMode >= level);
        }

        private LogUtility()
        {
            try
            {
                XDocument xdocParam, xdocLogs;

                #region Check XML schema of Parameters
                if (!File.Exists(m_szLogXMLSchema))
                {
                    StringBuilder xsch = new StringBuilder();

                    xsch.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    xsch.AppendLine("<xs:schema xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" attributeFormDefault=\"unqualified\" elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">");
                    xsch.AppendLine("  <xs:element name=\"LogParam\">");
                    xsch.AppendLine("    <xs:complexType>");
                    xsch.AppendLine("      <xs:sequence>");
                    xsch.AppendLine("        <xs:element name=\"MainDirectory\" type=\"xs:string\" maxOccurs=\"1\" minOccurs=\"1\"/>");
                    xsch.AppendLine("        <xs:element name=\"LogDeleteBufferDays\" type=\"xs:unsignedInt\" maxOccurs=\"1\" minOccurs=\"1\"/>");
                    xsch.AppendLine("        <xs:element name=\"BufferSize\" type=\"xs:unsignedInt\" maxOccurs=\"1\" minOccurs=\"1\"/>");
                    xsch.AppendLine("        <xs:element name=\"AutoFlushPeriod\" type=\"xs:unsignedInt\" maxOccurs=\"1\" minOccurs=\"1\"/>");
                    xsch.AppendLine("        <xs:element name=\"LevelControl\" type=\"xs:unsignedInt\" maxOccurs=\"1\" minOccurs=\"1\"/>");
                    xsch.AppendLine("      </xs:sequence>");
                    xsch.AppendLine("    </xs:complexType>");
                    xsch.AppendLine("  </xs:element>");
                    xsch.AppendLine("</xs:schema>");

                    StreamWriter xwriter = new StreamWriter(new FileStream(m_szLogXMLSchema, FileMode.Create), System.Text.Encoding.UTF8);
                    xwriter.Write(xsch);
                    xwriter.Flush();
                    xwriter.Close();
                }
                #endregion
                #region Check XML of Parameters
                if (!File.Exists(m_szLogXML))
                {
                    XDocument doc = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XElement("LogParam",
                        new XAttribute("{http://www.w3.org/2000/xmlns/}xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                        new XAttribute("{http://www.w3.org/2001/XMLSchema-instance}noNamespaceSchemaLocation", "Log.xsd"),
                        new XElement("MainDirectory", ConstVC.LogConfig.LogFolder),
                        new XElement("LogDeleteBufferDays", 30),
                        new XElement("BufferSize", 8),
                        new XElement("AutoFlushPeriod", 10),
                        new XElement("LevelControl", (int)LogLevel.INFO)
                        ));

                    XmlTextWriter xtw = new XmlTextWriter(m_szLogXML, System.Text.Encoding.UTF8);
                    xtw.Formatting = Formatting.Indented;
                    doc.WriteTo(xtw);
                    xtw.Flush();
                    xtw.Close();
                }
                #endregion
                #region Valid XML schema of Parameters
                {
                    xdocParam = GetLogParamXDocument();
                    XmlSchemaSet schemas = new XmlSchemaSet();
                    schemas.Add("", m_szLogXMLSchema);
                    xdocParam.Validate(schemas, null);
                }
                #endregion

                #region Check XML Schema of LogList
                if (!File.Exists(m_szLogObjectXMLSchema))
                {
                    StringBuilder xsch = new StringBuilder();
                    xsch.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    xsch.AppendLine("<xs:schema xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" attributeFormDefault=\"unqualified\" elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">");
                    xsch.AppendLine("  <xs:element name=\"LogObjects\">");
                    xsch.AppendLine("    <xs:complexType>");
                    xsch.AppendLine("      <xs:choice>");
                    xsch.AppendLine("        <xs:element maxOccurs=\"unbounded\" name=\"LogObject\">");
                    xsch.AppendLine("          <xs:complexType>");
                    xsch.AppendLine("            <xs:attribute name=\"FileBaseName\" type=\"xs:string\" use=\"required\" />");
                    xsch.AppendLine("            <xs:attribute name=\"Active\" type=\"xs:boolean\" use=\"required\" />");
                    xsch.AppendLine("          </xs:complexType>");
                    xsch.AppendLine("        </xs:element>");
                    xsch.AppendLine("      </xs:choice>");
                    xsch.AppendLine("    </xs:complexType>");
                    xsch.AppendLine("  </xs:element>");
                    xsch.AppendLine("</xs:schema>");

                    StreamWriter xwriter = new StreamWriter(new FileStream(m_szLogObjectXMLSchema, FileMode.Create), System.Text.Encoding.UTF8);
                    xwriter.Write(xsch);
                    xwriter.Flush();
                    xwriter.Close();
                }
                #endregion
                #region Valid XML schema of LogList
                {
                    xdocLogs = GetLogListXDocument();
                    XmlSchemaSet schemas = new XmlSchemaSet();
                    schemas.Add("", m_szLogObjectXMLSchema);
                    xdocLogs.Validate(schemas, null);
                }
                #endregion

                ReadLogListXML(xdocLogs);
                ReadLogParametersXML(xdocParam);
            }
            catch (Exception e)
            {
                throw e;
            }

            #region Setup Timer
            if (TimeForceWrites == null)
            {
                TimeForceWrites = new System.Timers.Timer();
                TimeForceWrites.Elapsed += new ElapsedEventHandler(TimeForceWrites_Elapsed);
                TimeForceWrites.Interval = iLocalBufferWriteTimeMinute * 60000;
                TimeForceWrites.Start();
            }
            #endregion

            ServiceSelfLog(LogHeadType.MethodExit, "Constructor()");
        }
        #endregion

        #region Public Inherited Methods
        public ArrayList InstantiateObjects() { return null; }
        public ArrayList EstablishCommunications() { return null; }
        public ArrayList DownloadParameters() { return null; }
        public ArrayList Initialize() { return null; }
        #endregion

        #region Private Member Functions
        private void CheckLogDirectory()
        {
            string pathRoot;
            pathRoot = Path.GetPathRoot(sMainDirectory);
            if (Directory.Exists(pathRoot))
            {
                System.IO.DriveInfo drive = new DriveInfo(pathRoot);
                if (drive.DriveType != DriveType.Fixed)
                {
                    throw new Exception(string.Format("Log root directory {0} has to be on fixed hard dirve!", pathRoot));
                }
                if (!Directory.Exists(sMainDirectory))
                {
                    Directory.CreateDirectory(sMainDirectory);
                }
            }
            else
            {
                throw new Exception(string.Format("Root directory {0} does not exist!", pathRoot));
            }
        }

        private void UpdateDictionaryFileName(bool updateAnyway)
        {
            Monitor.Enter(LogInfoDictionaryObj.SyncRoot);
            try
            {
                if (!updateAnyway && dtToday == DateTime.Today)
                    return;

                foreach (string key in LogInfoDictionaryObj.Keys)
                {
                    try
                    {
                        LogInfo LogInfoObj = (LogInfo)LogInfoDictionaryObj[key];
                        Monitor.Enter(LogInfoObj);
                        try
                        {
                            LogInfoObj.BufferSizeInKB = iLocalBufferKB;
                            LogInfoObj.CreateStreamWriter(sMainDirectory, LogInfoObj.Key);
                        }
                        finally
                        {
                            Monitor.Exit(LogInfoObj);
                        }
                    }
                    catch { }
                }
                //update current date
                dtToday = DateTime.Today;
            }
            catch { }
            finally
            {
                Monitor.Exit(LogInfoDictionaryObj.SyncRoot);
            }
        }

        private void ReadLogListXML(XDocument xdoc)
        {
            try
            {
                LogList = Hashtable.Synchronized(new Hashtable());
                IEnumerable<XNode> logNodes =
                    from log in xdoc.Elements("LogObjects").Nodes()
                    where log.NodeType == XmlNodeType.Element
                    select log;
                foreach (XElement node in logNodes)
                {
                    if (Convert.ToBoolean(node.Attribute("Active").Value))
                        LogList[node.Attribute("FileBaseName").Value.ToUpper()] = node.Attribute("FileBaseName").Value;
                }

                if (!LogList.ContainsKey(ConstVC.ObjectName.SystemEvents.ToUpper()))
                    LogList[ConstVC.ObjectName.SystemEvents.ToUpper()] = ConstVC.ObjectName.SystemEvents;

                if (!LogList.ContainsKey(ConstVC.ObjectName.LogService.ToUpper()))
                    LogList[ConstVC.ObjectName.LogService.ToUpper()] = ConstVC.ObjectName.LogService;

            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw e;
            }
        }

        private void ReadLogParametersXML(XDocument xdoc)
        {
            try
            {
                IEnumerable<XNode> paraNodes =
                    from para in xdoc.Elements("LogParam").Nodes()
                    where para.NodeType == XmlNodeType.Element
                    select para;
                foreach (XElement node in paraNodes)
                {
                    if (node.Name.ToString().Equals("MainDirectory"))
                    {
                        MainLogDirectory = node.Value;
                    }
                    else if (node.Name.ToString().Equals("LogDeleteBufferDays"))
                    {
                        siNodaysBeforeToday = Convert.ToInt32(node.Value);
                        if (siNodaysBeforeToday < 1)
                            siNodaysBeforeToday = 1;
                    }
                    else if (node.Name.ToString().Equals("BufferSize"))
                    {
                        BufferSizeInKB = Convert.ToInt32(node.Value);
                    }
                    else if (node.Name.ToString().Equals("AutoFlushPeriod"))
                    {
                        AutoFlushTimerMinutes = Convert.ToInt32(node.Value);
                    }
                    else if (node.Name.ToString().Equals("LevelControl"))//"LevelControl"
                    {
                        int tmp = Convert.ToInt32(node.Value);
                        if (tmp < (int) LogLevel.OFF)
                            tmp = (int)LogLevel.OFF;
                        else if (tmp > (int) LogLevel.ALL)
                            tmp = (int)LogLevel.ALL;

                        LogLevelMode = (LogLevel)tmp;
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw e;
            }
            finally { }
        }

        private string FormatMessage(LogHeadType eLogType, string szLogMessage, string szRemark = null)
        {
            DateTime dtNow = PreciseDatetime.Now;
            StringBuilder sbLog = new StringBuilder(dtNow.ToString(ConstVC.LogConfig.DateTimeFormat));
            if (string.IsNullOrWhiteSpace(szRemark))
            {
                string LogHead = LogStringAttribute.LookUpHead(eLogType);
                if (string.IsNullOrWhiteSpace(LogHead))
                {
                    sbLog.Append(" : ")
                         .Append(szLogMessage);
                }
                else
                {
                    sbLog.Append(" : [")
                         .Append(LogStringAttribute.LookUpHead(eLogType))
                         .Append("] ")
                         .Append(szLogMessage);
                }
            }
            else
            {
                string LogHead = LogStringAttribute.LookUpHead(eLogType);
                if (string.IsNullOrWhiteSpace(LogHead))
                {
                    sbLog.Append(" : [")
                         .Append(szRemark)
                         .Append("] ")
                         .Append(szLogMessage);
                }
                else
                {
                    sbLog.Append(" : [")
                         .Append(LogStringAttribute.LookUpHead(eLogType))
                         .Append(": ").Append(szRemark)
                         .Append("] ")
                         .Append(szLogMessage);
                }
            }

            return sbLog.ToString();
        }

        private string FormatMessage(string szLogMessage)
        {
            string szConvertedString;
            DateTime dtNow = PreciseDatetime.Now;
            String format = ConstVC.LogConfig.DateTimeFormat; // "MM/dd/yyyy HH:mm:ss:fffffff";
            szConvertedString = dtNow.ToString(format);

            szConvertedString = szConvertedString + " : " + szLogMessage;
            return szConvertedString;
        }

        private void ServiceSelfLog(LogLevel level, LogHeadType eLogType, string Msg, string szRemark = null)
        {
            WriteLog(ConstVC.ObjectName.LogService, level, eLogType, Msg, szRemark);
        }

        private void ServiceSelfLog(LogHeadType eLogType, string Msg, string szRemark = null)
        {
            WriteLog(ConstVC.ObjectName.LogService, LogLevel.DEBUG, eLogType, Msg, szRemark);
        }

        private void SystemEventLog(int EventId, string Msg)
        {
            //Event type should be log all the time
            WriteLog(ConstVC.ObjectName.SystemEvents, LogLevel.ALL, LogHeadType.Event, Msg, EventId.ToString("000"));
        }
        #endregion

        #region Public Member Functions
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    ServiceSelfLog(LogHeadType.MethodEnter, "LogUtility", "Dispose()");
                    ForceWrite();
                }
            }
            disposed = true;
        }

        public void ForceWrite()
        {
            TimeForceWrites.Stop();
            try
            {
                try
                {
                    if (ForceWritesEvent != null)
                    {
                        foreach (ForceWritesEventHandler fw in ForceWritesEvent.GetInvocationList())
                        {
                            try
                            {
                                fw.Method.Invoke(fw.Target, new object[] { });
                            }
                            catch
                            {
                                ForceWritesEvent = (ForceWritesEventHandler)Delegate.Remove(ForceWritesEvent, fw);
                            }
                        }
                    }
                }
                catch { }

                if (dtToday != DateTime.Today)
                    UpdateDictionaryFileName(false);

                Monitor.Enter(LogInfoDictionaryObj.SyncRoot);
                try
                {
                    foreach (DictionaryEntry Entry in LogInfoDictionaryObj)
                        ((LogInfo)Entry.Value).Flush();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
                finally
                {
                    Monitor.Exit(LogInfoDictionaryObj.SyncRoot);

                }
            }
            finally
            {
                TimeForceWrites.Start();
                //ServiceSelfLog(LogHeadType.MethodExit, null, "ForceWrite()");
            }
        }

        public bool WriteLogWithSecured(string szLogKey, LogLevel level, LogHeadType enLogType, string szLogMessage, string[] SecuredSections, string szRemark = null)
        {
            if (!NeedLog(level))
                return true;

            StringBuilder SecuredLogMessage = new StringBuilder(szLogMessage);
            foreach (string KeyWord in SecuredSections)
            {
                if (!String.IsNullOrWhiteSpace(KeyWord))
                    SecuredLogMessage = SecuredLogMessage.Replace(KeyWord, SecuredKeyword(KeyWord));
            }

            return (WriteLog(szLogKey, level, enLogType, SecuredLogMessage.ToString(), szRemark));
        }


        private string SecuredKeyword(string Keyword)
        {
            return (LogStringAttribute.GetLogHeadString(LogHeadType.Secured, Keyword));
        }

        public bool WriteLog(string szKey, LogLevel level, LogHeadType enLogType, string szLogMessage, string szRemark = null)
        {
            if (!NeedLog(level))
                return true;

            if (iLocalBufferKB <= 0)
                return false;

            if (dtToday != DateTime.Today)
                UpdateDictionaryFileName(false);

            szKey = szKey.ToUpper();
            LogInfo log = null;
            Monitor.Enter(LogInfoDictionaryObj.SyncRoot);
            try
            {
                if (!LogInfoDictionaryObj.ContainsKey(szKey))
                {
                    if (LogList != null && LogList.ContainsKey(szKey))
                    {
                        log = new LogInfo(sMainDirectory, (string)LogList[szKey], true, iLocalBufferKB);
                        LogInfoDictionaryObj[szKey] = log;
                    }
                    else
                        return false;
                }
                else
                {
                    log = (LogInfo)LogInfoDictionaryObj[szKey];
                }
                log.WriteLine(FormatMessage(enLogType, szLogMessage, szRemark));
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                Monitor.Exit(LogInfoDictionaryObj.SyncRoot);
            }
        }

        public XDocument GetLogListXDocument()
        {
            Monitor.Enter(LockLogObjectXML);
            try
            {
                XmlTextReader reader = new XmlTextReader(m_szLogObjectXML);
                XDocument xdoc = XDocument.Load(reader);
                reader.Close();
                return xdoc;
            }
            finally
            {
                Monitor.Exit(LockLogObjectXML);
            }
        }
        public string SetLogListXDocument(XDocument xdoc)
        {
            Monitor.Enter(LockLogObjectXML);
            try
            {
                if (xdoc.Declaration == null)
                {
                    xdoc.Declaration = new XDeclaration("1.0", "utf-8", "yes");
                    xdoc.Root.Add(new XAttribute("{http://www.w3.org/2000/xmlns/}xsi", "http://www.w3.org/2001/XMLSchema-instance"));
                    xdoc.Root.Add(new XAttribute("{http://www.w3.org/2001/XMLSchema-instance}noNamespaceSchemaLocation", "LogObjectInfo.xsd"));
                }

                XmlSchemaSet schemas = new XmlSchemaSet();
                schemas.Add("", m_szLogObjectXMLSchema);
                xdoc.Validate(schemas, null);

                XmlTextWriter xtw = new XmlTextWriter(m_szLogObjectXML, System.Text.Encoding.UTF8);
                xtw.Formatting = Formatting.Indented;
                xdoc.WriteTo(xtw);
                xtw.Flush();
                xtw.Close();
            }
            catch (Exception e)
            {
                return e.Message;
            }
            finally
            {
                Monitor.Exit(LockLogObjectXML);
            }

            Monitor.Enter(LogInfoDictionaryObj.SyncRoot);
            try
            {
                ReadLogListXML(xdoc);
                ArrayList removeList = new ArrayList();
                foreach (string key in LogInfoDictionaryObj.Keys)
                {
                    if (!LogList.ContainsKey(key))
                        removeList.Add(key);
                }
                for (int i = 0; i < removeList.Count; i++)
                {
                    string key = (string)removeList[i];
                    ((LogInfo)LogInfoDictionaryObj[key]).Close();
                    LogInfoDictionaryObj.Remove(key);
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
            finally
            {
                Monitor.Exit(LogInfoDictionaryObj.SyncRoot);
            }

            if (this.LogListChangedEvent != null)
            {
                foreach (LogListChangedEventHandler bs in LogListChangedEvent.GetInvocationList())
                {
                    try
                    {
                        bs.Method.Invoke(bs.Target, new object[] { LogList });
                    }
                    catch
                    {
                        LogListChangedEvent = (LogListChangedEventHandler)Delegate.Remove(LogListChangedEvent, bs);
                    }
                }
            }
            return null;
        }

        public XDocument GetLogParamXDocument()
        {
            Monitor.Enter(LockLogXML);
            try
            {
                XmlTextReader reader = new XmlTextReader(m_szLogXML);
                XDocument xdoc = XDocument.Load(reader);
                reader.Close();
                return xdoc;
            }
            finally
            {
                Monitor.Exit(LockLogXML);
            }
        }
        public void SetLogParamXDocument(XDocument xdoc)
        {
            Monitor.Enter(LockLogXML);
            try
            {
                if (xdoc.Declaration == null)
                {
                    xdoc.Declaration = new XDeclaration("1.0", "utf-8", "yes");
                    xdoc.Root.Add(new XAttribute("{http://www.w3.org/2000/xmlns/}xsi", "http://www.w3.org/2001/XMLSchema-instance"));
                    xdoc.Root.Add(new XAttribute("{http://www.w3.org/2001/XMLSchema-instance}noNamespaceSchemaLocation", "Log.xsd"));
                }
                XmlTextWriter xtw = new XmlTextWriter(m_szLogXML, System.Text.Encoding.UTF8);
                xtw.Formatting = Formatting.Indented;
                xdoc.WriteTo(xtw);
                xtw.Flush();
                xtw.Close();
                ReadLogParametersXML(xdoc);
            }
            finally
            {
                Monitor.Exit(LockLogXML);
            }
        }
        #endregion

        #region Public Properties
        public string MainLogDirectory
        {
            get { return sMainDirectory; }

            set
            {
                if (sMainDirectory != value)
                {
                    if (sMainDirectory == "")
                    {
                        sMainDirectory = value;
                        CheckLogDirectory();
                    }
                    else
                    {
                        sMainDirectory = value;
                        CheckLogDirectory();
                        UpdateDictionaryFileName(true);

                        if (this.MainDirectoryChangedEvent != null)
                        {
                            foreach (MainDirectoryChangedEventHandler bs in MainDirectoryChangedEvent.GetInvocationList())
                            {
                                try
                                {
                                    bs.Method.Invoke(bs.Target, new object[] { sMainDirectory });
                                }
                                catch
                                {
                                    MainDirectoryChangedEvent = (MainDirectoryChangedEventHandler)Delegate.Remove(MainDirectoryChangedEvent, bs);
                                }
                            }
                        }
                    }
                }
            }
        }

        public int BufferSizeInKB
        {
            get { return iLocalBufferKB; }

            set
            {
                if (iLocalBufferKB != value)
                {
                    if (iLocalBufferKB == -1)
                        iLocalBufferKB = value;
                    else
                    {
                        iLocalBufferKB = value;
                        UpdateDictionaryFileName(true);

                        if (this.BufferSizeChangedEvent != null)
                        {
                            foreach (BufferSizeChangedEventHandler bs in BufferSizeChangedEvent.GetInvocationList())
                            {
                                try
                                {
                                    bs.Method.Invoke(bs.Target, new object[] { iLocalBufferKB });
                                }
                                catch
                                {
                                    BufferSizeChangedEvent = (BufferSizeChangedEventHandler)Delegate.Remove(BufferSizeChangedEvent, bs);
                                }
                            }
                        }
                    }
                }

                ServiceSelfLog(LogLevel.INFO, LogHeadType.Info, string.Format("BufferSizeInLines = {0}", value));
            }
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

                ServiceSelfLog(LogLevel.INFO, LogHeadType.Info, string.Format("AutoForceWriteTimerMinutes = {0}", value));
            }
        }

        public void ClearLogs()
        {
            if (string.IsNullOrWhiteSpace(MainLogDirectory))
                return;

            lock (LockDelete)
            {
                ServiceSelfLog(LogLevel.INFO, LogHeadType.MethodEnter, "ClearLogs() DaysForPerservingLog = " + DaysForPerservingLog.ToString());

                if (TimeForceClear == null)
                    TimeForceClear = new System.Timers.Timer();

                //start timer to automatic delete log
                TimeForceClear.Stop();
                TimeForceClear.Interval = 43200000; // 12 hrs in ms
                TimeForceClear.Elapsed += new ElapsedEventHandler(TimeForceClear_Elapsed);
                TimeForceClear.Start();

                DateTime LastDate, CurDate, date1;
                // get the current date
                CurDate = DateTime.Now.Date;
                date1 = CurDate.AddDays(-DaysForPerservingLog);

                DirectoryInfo dir = new DirectoryInfo(MainLogDirectory);
                foreach (FileInfo f in dir.GetFiles("*.log", SearchOption.TopDirectoryOnly))
                {
                    // get the last write datetime
                    LastDate = f.LastWriteTime;
                    if (LastDate < date1)
                    {
                        try
                        {
                            ServiceSelfLog(LogLevel.INFO, LogHeadType.Info, string.Format("Delete file: {0}, LastWriteTime:{1}", f.FullName, LastDate));
                            File.Delete(f.FullName);
                            ServiceSelfLog(LogLevel.INFO, LogHeadType.Info, "  ==> Success");
                        }
                        catch (Exception ex)
                        {
                            string msg = ex.Message;
                            ServiceSelfLog(LogLevel.INFO, LogHeadType.Info, "  ==> Fail. Reason: " + ex.Message);
                        }
                    }
                }

                ServiceSelfLog(LogLevel.INFO, LogHeadType.MethodExit, "ClearLogs()");
            }
        }

        void TimeForceClear_Elapsed(object sender, ElapsedEventArgs e)
        {
            TimeForceClear.Elapsed -= new ElapsedEventHandler(TimeForceClear_Elapsed);
            this.ClearLogs();
        }

        public int DaysForPerservingLog
        {
            get { return siNodaysBeforeToday; }
        }

        public Hashtable ActiveLogList
        {
            get { return LogList; }
        }
        #endregion

        #region Timer Callback Implementation
        void TimeForceWrites_Elapsed(object sender, ElapsedEventArgs e)
        {
            ForceWrite();
        }
        #endregion
    }

    public class LogHelper
    {
        AbstractLogUtility _log = null;
        private string _objectName = ConstVC.ObjectName.LogService;

        private LogHelper()
        {
        }

        public LogHelper(string ObjectName)
        {
            _log = LogUtility.GetUniqueInstance();
            if (!string.IsNullOrWhiteSpace(ObjectName))
                _objectName = ObjectName;
        }

        public static LogLevel GetDefaultLogLevel(LogHeadType enLogType)
        {
            switch (enLogType)
            {
                case LogHeadType.Error:
                case LogHeadType.Exception:
                case LogHeadType.Exception_Alarm:
                case LogHeadType.Exception_Clear:
                case LogHeadType.Exception_Error:
                case LogHeadType.Exception_Notify:
                case LogHeadType.Exception_Set:
                case LogHeadType.Exception_Warning:
                case LogHeadType.Alarm:
                    return LogLevel.ERROR;
                case LogHeadType.Warning:
                    return LogLevel.WARN;

                default:
                    return LogLevel.INFO;
            }
        }

        private bool WriteLog(LogHeadType enLogType, string szLogMessage, string szRemark = null)
        {
            return WriteLog(GetDefaultLogLevel(enLogType), enLogType, szLogMessage, szRemark);
        }

        public bool WriteLog(LogLevel level, LogHeadType enLogType, string szLogMessage, string szRemark = null)
        {
            if (_log != null)
                return _log.WriteLog(_objectName, level, enLogType, szLogMessage, szRemark);
            else
                return false;
        }
    }
}
