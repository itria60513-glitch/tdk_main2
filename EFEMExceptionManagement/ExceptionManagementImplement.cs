using System;
using System.Xml;
using System.IO;
using System.Collections;
using System.Runtime.Serialization;
using System.Threading;
using EFEM.FileUtilities;
using EFEM.LogUtilities;
using EFEM.DataCenter;
using System.Text;

namespace EFEM.ExceptionManagements
{
    [Serializable]
    public class ExceptionManagement : AbstractExceptionManagement
    {
        private string objectName = ConstVC.ObjectName.ExceptionManagement;
        private static AbstractExceptionManagement _instance = null;

        public static AbstractExceptionManagement GetUniqueInstance()
        {
            if (_instance == null)
                _instance = new ExceptionManagement();

            return _instance;
        }

        private ExceptionManagement()
        {
            _log = LogUtility.GetUniqueInstance();
            _fu = FileUtility.GetUniqueInstance();
            WriteLog(LogHeadType.System_NewStart, "");

            XmlTextReader xtr = null;
            // Load ExceptionInfo.xml
            try
            {
                xtr = new XmlTextReader(ConstVC.FilePath.ConfigFolder + ConstVC.FilePath.EFEMExceptionInfo);
                xtr.WhitespaceHandling = WhitespaceHandling.None;
                xdExInfo = new XmlDocument();
                xdExInfo.Load(xtr);
                xtr.Close();
            }
            catch (Exception ex)
            {
                string errMsg = "Failed to load ExceptionInfo.xml. Reason: " + ex.Message;
                WriteLog(LogHeadType.Exception, errMsg);
                throw new Exception(errMsg);
            }

            // Load AlarmHistory.xml
            try
            {
                xtr = new XmlTextReader(ConstVC.FilePath.ConfigFolder + ConstVC.FilePath.EFEMAlarmHistory);
                xtr.WhitespaceHandling = WhitespaceHandling.None;
                xdExHistory = new XmlDocument();
                xdExHistory.Load(xtr);
                xtr.Close();
            }
            catch (Exception ex)
            {
                string errMsg = "Failed to load EFEMAlarmHistory.xml. Reason: " + ex.Message;
                WriteLog(LogHeadType.Exception, errMsg);
                
                //try again
                try
                {
                    if (xtr != null)
                        xtr.Close();

                    RecoverAlarmHistory();

                    xtr = new XmlTextReader(ConstVC.FilePath.ConfigFolder + ConstVC.FilePath.EFEMAlarmHistory);
                    xtr.WhitespaceHandling = WhitespaceHandling.None;
                    xdExHistory = new XmlDocument();
                    xdExHistory.Load(xtr);
                    xtr.Close();
                }
                catch (Exception ex2)
                {
                    errMsg = "Failed to recover EFEMAlarmHistory.xml. Reason: " + ex2.Message;
                    WriteLog(LogHeadType.Exception, errMsg);
                    throw new Exception(errMsg);
                }
            }

            ObjDictionary = _fu.GetObjectDictionary();
            ExDictionary = new ExceptionDictionary();
            foreach (DictionaryEntry obj in ObjDictionary)
            {
                UInt16 agent = Convert.ToUInt16((string)obj.Key);
                XmlDocument xd = xdExInfo;
                XmlNode xnod = xd.DocumentElement;
                XmlNode xnodSection;
                if (xnod.HasChildNodes)
                {
                    xnodSection = xnod.FirstChild;
                    while (xnodSection != null)
                    {
                        if (xnodSection.Name == (string)obj.Value)
                            break;
                        xnodSection = xnodSection.NextSibling;
                    }
                    if (xnodSection == null)
                    {
                        string str = string.Format("Unable to find the object [{0}] information in {1}.", obj.Value, ConstVC.FilePath.EFEMExceptionInfo);
                        throw new ApplicationException(str);
                    }
                    xnodSection = xnodSection.FirstChild;
                    while (xnodSection != null)
                    {
                        XmlNamedNodeMap mapAttributes = xnodSection.Attributes;
                        if (mapAttributes == null || mapAttributes.Count != 8)
                        {
                            string str = string.Format("Wrong fromat of {0}.", ConstVC.FilePath.EFEMExceptionInfo);
                            throw new ApplicationException(str);
                        }
                        XmlNode xnod1 = mapAttributes.Item(0);
                        XmlNode xnod2 = mapAttributes.Item(1);
                        XmlNode xnod3 = mapAttributes.Item(2);
                        XmlNode xnod4 = mapAttributes.Item(3);
                        XmlNode xnod5 = mapAttributes.Item(4);
                        XmlNode xnod6 = mapAttributes.Item(5);
                        XmlNode xnod7 = mapAttributes.Item(6);
                        XmlNode xnod8 = mapAttributes.Item(7);
                        if (xnod1.Name != "key" ||
                            xnod2.Name != "id" || xnod3.Name != "message" || xnod4.Name != "category" ||
                            xnod5.Name != "mode" || xnod6.Name != "remedy" || xnod7.Name != "maperrid" || xnod8.Name != "errid")
                        {
                            string str = string.Format("Wrong fromat of {0}.", ConstVC.FilePath.EFEMExceptionInfo);
                            throw new ApplicationException(str);
                        }

                        ExceptionInfo info = new ExceptionInfo(agent,
                            Convert.ToUInt16(xnod2.Value),
                            xnod3.Value,
                            (ExCategory)Convert.ToInt32(xnod4.Value),
                            (ExMode)Convert.ToUInt32(xnod5.Value),
                            xnod6.Value, Convert.ToUInt32(xnod7.Value), Convert.ToUInt32(xnod8.Value));
                        ExDictionary.Add(info.ALID.ToString(), info);

                        xnodSection = xnodSection.NextSibling;
                    }
                }
            }
            _ExQueue = new Hashtable();

            _xml_ReadAlarmHistory();
            _xml_ReadAlarmStatistics();
        }


        private bool RecoverAlarmHistory()
        {
            WriteLog(LogHeadType.MethodEnter, "", "RecoverAlarmHistory()");

            try
            {
                if (File.Exists(ConstVC.FilePath.ConfigFolder + ConstVC.FilePath.EFEMAlarmHistory))
                {
                    string sourceName = ConstVC.FilePath.ConfigFolder + ConstVC.FilePath.EFEMAlarmHistory;
                    string tail = DateTime.Now.Ticks.ToString();
                    string targetName = sourceName + "." + tail;
                    File.Move(sourceName, targetName);

                    if (File.Exists(ConstVC.FilePath.ConfigFolder + ConstVC.FilePath.EFEMAlarmHistory))
                        throw new Exception("Failed to rename current AlarmHistory file.");
                    else
                    {
                        WriteLog(LogHeadType.Info, string.Format("{0} has been renamed to {1}", ConstVC.FilePath.EFEMAlarmHistory, ConstVC.FilePath.EFEMAlarmHistory + "." + tail));
                    }
                }

                WriteLog(LogHeadType.Info, "Creating empty AlarmHistory...");

                #region Create empty Alarm History
                StringBuilder xsch = new StringBuilder();
                xsch.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                xsch.AppendLine("<!-- Alarm History -->");
                xsch.AppendLine("<Alarm>");
                xsch.AppendLine("  <History>");
                xsch.AppendLine("  </History>");
                xsch.AppendLine("  <Statistics>");
                xsch.AppendLine("  </Statistics>");
                xsch.AppendLine("</Alarm>");

                StreamWriter xwriter = new StreamWriter(new FileStream(ConstVC.FilePath.ConfigFolder + ConstVC.FilePath.EFEMAlarmHistory, FileMode.Create), System.Text.Encoding.UTF8);
                xwriter.Write(xsch);
                xwriter.Flush();
                xwriter.Close();
                #endregion

                WriteLog(LogHeadType.MethodEnter, "Success.", "RecoverAlarmHistory()");
                return true;
            }
            catch (Exception ex)
            {
                WriteLog(LogHeadType.MethodEnter, "Fail. Reason: " + ex.Message, "RecoverAlarmHistory()");
                return false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            WriteLog(LogHeadType.MethodEnter, "Dispose()");

            if (!this.disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                }
            }

            if (_ExQueue.Count > 0)
            {
                WriteLog(LogHeadType.Info, "--->Clearing all alarms...");
                ArrayList al = new ArrayList();
                foreach (ExceptionInfo info in _ExQueue.Values)
                    al.Add(info.ALID);
                for (int i = 0; i < al.Count; i++)
                    AlarmClear((uint)al[i]);
                al.Clear();
            }

            _event.Set();
            disposed = true;
        }

        ~ExceptionManagement()
        {
            Dispose(false);
        }

        #region Inherited Functions
        public ArrayList InstantiateObjects()
        {
            ArrayList al = new ArrayList();
            FileInfo info = new FileInfo(ConstVC.FilePath.ConfigFolder + ConstVC.FilePath.EFEMAlarmHistory);
            if (!info.Exists)
                al.Add(string.Format("{0} does not exist.", ConstVC.FilePath.EFEMAlarmHistory));
            else if (info.IsReadOnly)
                al.Add(string.Format("{0} is read-only.", ConstVC.FilePath.EFEMAlarmHistory));
            if (al.Count > 0)
                return al;
            else
                return null;
        }
        //public ArrayList EstablishCommunications()
        //{
        //    return null;
        //}
        //public ArrayList DownloadParameters()
        //{
        //    return null;
        //}
        public ArrayList Initialize()
        {
            _eventExit.Reset();
            _event.Reset();
            return null;
        }
        #endregion

        #region Public Interface
        public bool AlarmSet(string TimeStamp, UInt32 ALID)
        {
            return InterAlarmSet(TimeStamp, ALID, "", true);
        }
        public bool AlarmSet(string TimeStamp, UInt32 ALID, bool ifPopupMessageBox)
        {
            return InterAlarmSet(TimeStamp, ALID, "", ifPopupMessageBox);
        }
        public bool AlarmSet(string TimeStamp, UInt32 ALID, string AdditionalMessage)
        {
            return InterAlarmSet(TimeStamp, ALID, AdditionalMessage, true);
        }
        private bool InterAlarmSet(string TimeStamp, UInt32 ALID, string AdditionalMessage, bool ifPopupMessageBox)
        {
            Monitor.Enter(_locker);
            try
            {
                // -------------------------
                // Search ALID in dictionary.
                ExceptionInfo info = ExDictionary[ALID.ToString()];
                // If cannot find
                if (info == null)
                {
                    WriteLog(LogHeadType.Error, string.Format("AlarmSet(TimeStamp:{0}, ALID:{1}) - Failed. Invalid ALID..", TimeStamp, ALID));
                    return false;
                }

                // --------------------------
                // Search ALID in queue.
                if (_ExQueue.Contains(ALID.ToString()))
                    return true;

                // Add the item into queue.
                ExceptionInfo qItem;
                if (string.IsNullOrWhiteSpace(AdditionalMessage))
                    qItem = new ExceptionInfo(info);
                else
                    qItem = new ExceptionInfo(info.agent, info.id,
                                                info.mesage + " - " + AdditionalMessage,
                                                info.category, info.mode, info.remedy, info.MapErrID, info.ErrID);
                qItem.Timestamp_set = TimeStamp;
                qItem.state = ExStateReporting.POSTED;
                try
                {
                    _ExQueue.Add(qItem.ALID.ToString(), qItem);
                }
                catch (Exception e)
                {
                    WriteLog(LogHeadType.Error, e.StackTrace, "InterAlarmSet()");
                }
                // --------------------------

                Fire_AlarmReportSendGUI((byte)0x80, qItem, TimeStamp, ifPopupMessageBox);

                // Update alarm statistics data
                if (_ExStatistics.ContainsKey(qItem.ALID))
                {
                    _ExStatistics[qItem.ALID] = (int)_ExStatistics[qItem.ALID] + 1;
                    _xml_UpdateAlarmStatistics(qItem.ALID, (int)_ExStatistics[qItem.ALID]);
                }
                else
                {
                    _xml_UpdateAlarmStatistics(qItem.ALID, 1);
                    _ExStatistics.Add(qItem.ALID, 1);
                }

                WriteLog(1, qItem);
                return true;
            }
            finally
            {
                Monitor.Exit(_locker);
            }
        }

        public bool AlarmClear(UInt32 ALID)
        {
            Monitor.Enter(_locker);
            string TimeStamp = this.GetTimeStamp();
            try
            {
                // --------------------------
                // Search ALID in queue.
                if (!_ExQueue.Contains(ALID.ToString()))
                {
                    WriteLog(LogHeadType.Error, string.Format("AlarmClear(TimeStamp:{0}, ALID:{1}) - Failed. This alarm is not active.", TimeStamp, ALID));
                    return false;
                }
                ExceptionInfo qItem = (ExceptionInfo)_ExQueue[ALID.ToString()];
                qItem.Timestamp_clear = TimeStamp;
                qItem.state = ExStateReporting.CLEARED;

                FireAlarmCleared(ALID); //Clear the Alarm in MachineControl
                Fire_AlarmReportSendGUI((byte)0x00, qItem, TimeStamp, false);

                // Remove alarm from queue
                _ExQueue.Remove(qItem.ALID.ToString());

                // Move alarm to history & updata xml
                _ExHistoryList.Insert(0, qItem);
                _xml_AddAlarmHistory(qItem);

                WriteLog(0, qItem);
                return true;
            }
            finally
            {
                Monitor.Exit(_locker);
            }
        }

        public bool AlarmAcknowledge(string TimeStamp, UInt32 ALID)
        {
            Monitor.Enter(_locker);
            TimeStamp = this.GetTimeStamp();
            try
            {
                // --------------------------
                // Search ALID in queue.
                if (!_ExQueue.Contains(ALID.ToString()))
                    return false;
                ExceptionInfo qItem = (ExceptionInfo)_ExQueue[ALID.ToString()];
                if (qItem.state == ExStateReporting.ACKNOWLEDGED) return true;
                // --------------------------

                qItem.Timestamp_ack = TimeStamp;
                qItem.state = ExStateReporting.ACKNOWLEDGED;

                Fire_AlarmAckByUser(qItem.ALID, qItem.DateTime_ACK);

                WriteLog(2, qItem);
                return true;
            }
            finally
            {
                Monitor.Exit(_locker);
            }
        }

        public Hashtable ListActiveAlarmRequest()
        {
            Monitor.Enter(_locker);
            try
            {
                return _ExQueue;
            }
            finally
            {
                Monitor.Exit(_locker);
            }
        }

        public ArrayList ListHistoryAlarmRequest()
        {
            Monitor.Enter(_locker);
            try
            {
                return _ExHistoryList;
            }
            finally
            {
                Monitor.Exit(_locker);
            }
        }

        public string GetTimeStamp()
        {
            DateTime dt = DateTime.Now;
            string str = string.Format("{0}{1}{2}{3}{4}{5}{6}", dt.Year.ToString(), dt.Month.ToString("00"), dt.Day.ToString("00"), dt.Hour.ToString("00"), dt.Minute.ToString("00"), dt.Second.ToString("00"), Convert.ToInt32(dt.Millisecond / 10).ToString("00"));
            return str;
        }

        public bool IsAnyAlarmExist
        {
            get
            {
                if (_ExQueue == null)
                    return false;
                else
                    return (_ExQueue.Count > 0);
            }
        }

        public bool IfAutoClearAlarm(UInt32 ALID)
        {
            ExceptionInfo info = ExDictionary[ALID.ToString()];
            if (info != null && info.mode == ExMode.Auto)
                return true;
            return false;
        }
        #endregion

        #region XML
        public ExceptionDictionary GetExceptionDictionary(string objectname)
        {
            Monitor.Enter(_xmlLocker);
            try
            {
                UInt16 agent = 0;
                foreach (DictionaryEntry obj in ObjDictionary)
                {
                    if ((string)obj.Value == objectname)
                        agent = Convert.ToUInt16(obj.Key);
                }
                if (agent == 0)
                {
                    string str = string.Format("Undefined object name [{0}] in ObjectInfo.xml.", objectname);
                    throw new ApplicationException(str);
                }
                XmlDocument xd = xdExInfo;
                XmlNode xnod = xd.DocumentElement;
                XmlNode xnodSection;
                ExceptionDictionary dictionary = new ExceptionDictionary();
                if (xnod.HasChildNodes)
                {
                    xnodSection = xnod.FirstChild;
                    while (xnodSection != null)
                    {
                        if (xnodSection.Name == objectname)
                            break;
                        xnodSection = xnodSection.NextSibling;
                    }
                    if (xnodSection == null)
                    {
                        string str = string.Format("Unable to find the object [{0}] information in ExceptionInfo.xml.", objectname);
                        throw new ApplicationException(str);
                    }
                    xnodSection = xnodSection.FirstChild;
                    while (xnodSection != null)
                    {
                        XmlNamedNodeMap mapAttributes = xnodSection.Attributes;
                        if (mapAttributes == null || mapAttributes.Count != 8)
                        {
                            string str = string.Format("Wrong fromat of ExceptionInfo.xml.");
                            throw new ApplicationException(str);
                        }
                        XmlNode xnod1 = mapAttributes.Item(0);
                        XmlNode xnod2 = mapAttributes.Item(1);
                        XmlNode xnod3 = mapAttributes.Item(2);
                        XmlNode xnod4 = mapAttributes.Item(3);
                        XmlNode xnod5 = mapAttributes.Item(4);
                        XmlNode xnod6 = mapAttributes.Item(5);
                        XmlNode xnod7 = mapAttributes.Item(6);
                        XmlNode xnod8 = mapAttributes.Item(7);
                        if (xnod1.Name != "key" ||
                            xnod2.Name != "id" || xnod3.Name != "message" || xnod4.Name != "category" ||
                            xnod5.Name != "mode" || xnod6.Name != "remedy" || xnod7.Name != "maperrid" || xnod8.Name != "errid")
                        {
                            string str = string.Format("Wrong fromat of ExceptionInfo.xml.");
                            throw new ApplicationException(str);
                        }
                        dictionary.Add(xnod1.Value, new ExceptionInfo(agent,
                            Convert.ToUInt16(xnod2.Value),
                            xnod3.Value,
                            (ExCategory)Convert.ToInt32(xnod4.Value),
                            (ExMode)Convert.ToUInt32(xnod5.Value),
                            xnod6.Value, Convert.ToUInt32(xnod7.Value),
                            Convert.ToUInt32(xnod8.Value)
                            ));
                        xnodSection = xnodSection.NextSibling;
                    }
                }
                return dictionary;
            }
            finally
            {
                Monitor.Exit(_xmlLocker);
            }
        }

        private void _xml_ReadAlarmHistory()
        {
            _xmlHisLocker.AcquireReaderLock(Timeout.Infinite);
            try
            {
                _ExHistoryList.Clear();
                XmlDocument xd = xdExHistory;
                XmlNode xnod = xd.DocumentElement;
                XmlNode xnodSection;
                ExceptionDictionary dictionary = new ExceptionDictionary();
                if (xnod.HasChildNodes)
                {
                    xnodSection = xnod.FirstChild;
                    while (xnodSection != null)
                    {
                        if (xnodSection.Name == "History")
                            break;
                        xnodSection = xnodSection.NextSibling;
                    }
                    if (xnodSection == null)
                        return;
                    xnodSection = xnodSection.FirstChild;
                    while (xnodSection != null)
                    {
                        XmlNamedNodeMap mapAttributes = xnodSection.Attributes;
                        if (mapAttributes == null || mapAttributes.Count != 7)
                        {
                            xnodSection = xnodSection.NextSibling;
                            continue;
                        }
                        XmlNode xnod1 = mapAttributes.Item(0);
                        XmlNode xnod2 = mapAttributes.Item(1);
                        XmlNode xnod3 = mapAttributes.Item(2);
                        XmlNode xnod4 = mapAttributes.Item(3);
                        XmlNode xnod5 = mapAttributes.Item(4);
                        XmlNode xnod6 = mapAttributes.Item(5);
                        XmlNode xnod7 = mapAttributes.Item(6);
                        if (xnod1.Name != "agent" ||
                            xnod2.Name != "id" || xnod3.Name != "message" || xnod4.Name != "category" ||
                            xnod5.Name != "mode" || xnod6.Name != "settime" || xnod7.Name != "cleartime")
                        {
                            xnodSection = xnodSection.NextSibling;
                            continue;
                        }
                        ExceptionInfo info = new ExceptionInfo(Convert.ToUInt16(xnod1.Value),
                                                                Convert.ToUInt16(xnod2.Value),
                                                                xnod3.Value,
                                                                (ExCategory)Convert.ToInt32(xnod4.Value),
                                                                (ExMode)Convert.ToUInt32(xnod5.Value),
                                                                "",
                                                                (uint)0, (uint)0);
                        info.Timestamp_set = xnod6.Value;
                        info.Timestamp_clear = xnod7.Value;
                        _ExHistoryList.Add(info);
                        xnodSection = xnodSection.NextSibling;
                    }
                }
                return;
            }
            finally
            {
                _xmlHisLocker.ReleaseReaderLock();
            }
        }

        private void _xml_AddAlarmHistory(ExceptionInfo info)
        {
            _xmlHisLocker.AcquireWriterLock(Timeout.Infinite);
            try
            {
                XmlDocument xd = xdExHistory;
                XmlNode xnod = xd.DocumentElement;
                XmlNode xnodSection;
                ExceptionDictionary dictionary = new ExceptionDictionary();
                if (xnod.HasChildNodes)
                {
                    xnodSection = xnod.FirstChild;
                    while (xnodSection != null)
                    {
                        if (xnodSection.Name == "History")
                            break;
                        xnodSection = xnodSection.NextSibling;
                    }
                    if (xnodSection == null)
                        return;

                    if (xnodSection.ChildNodes.Count >= 1000)
                    {
                        xnodSection.RemoveChild(xnodSection.LastChild);
                    }

                    XmlNode xnodChild = xnodSection.FirstChild;

                    XmlElement elem = xd.CreateElement("HAlarm");
                    elem.SetAttribute("agent", info.agent.ToString());
                    elem.SetAttribute("id", info.id.ToString());
                    elem.SetAttribute("message", info.mesage);
                    elem.SetAttribute("category", ((int)info.category).ToString());
                    elem.SetAttribute("mode", ((int)info.mode).ToString());
                    elem.SetAttribute("settime", info.Timestamp_set);
                    elem.SetAttribute("cleartime", info.Timestamp_clear);
                    xnodSection.InsertBefore(elem, xnodChild);

                    XmlTextWriter xtw = new XmlTextWriter(ConstVC.FilePath.ConfigFolder + ConstVC.FilePath.EFEMAlarmHistory, System.Text.Encoding.UTF8);
                    xtw.Formatting = Formatting.Indented;
                    xd.WriteTo(xtw);
                    xtw.Flush();
                    xtw.Close();
                }
                return;
            }
            finally
            {
                _xmlHisLocker.ReleaseWriterLock();
            }
        }

        private void _xml_ReadAlarmStatistics()
        {
            _xmlHisLocker.AcquireReaderLock(Timeout.Infinite);
            try
            {
                _ExStatistics.Clear();
                XmlDocument xd = xdExHistory;
                XmlNode xnod = xd.DocumentElement;
                XmlNode xnodSection;
                ExceptionDictionary dictionary = new ExceptionDictionary();
                if (xnod.HasChildNodes)
                {
                    xnodSection = xnod.FirstChild;
                    while (xnodSection != null)
                    {
                        if (xnodSection.Name == "Statistics")
                            break;
                        xnodSection = xnodSection.NextSibling;
                    }
                    if (xnodSection == null)
                        return;
                    xnodSection = xnodSection.FirstChild;
                    while (xnodSection != null)
                    {
                        XmlNamedNodeMap mapAttributes = xnodSection.Attributes;
                        if (mapAttributes == null || mapAttributes.Count != 2)
                        {
                            xnodSection = xnodSection.NextSibling;
                            continue;
                        }
                        XmlNode xnod1 = mapAttributes.Item(0);
                        XmlNode xnod2 = mapAttributes.Item(1);
                        if (xnod1.Name != "alid" || xnod2.Name != "count")
                        {
                            xnodSection = xnodSection.NextSibling;
                            continue;
                        }
                        _ExStatistics.Add(Convert.ToUInt32(xnod1.Value), Convert.ToInt32(xnod2.Value));
                        xnodSection = xnodSection.NextSibling;
                    }
                }
                return;
            }
            finally
            {
                _xmlHisLocker.ReleaseReaderLock();
            }
        }

        private void _xml_UpdateAlarmStatistics(uint alid, int count)
        {
            _xmlHisLocker.AcquireWriterLock(Timeout.Infinite);
            try
            {
                XmlDocument xd = xdExHistory;
                XmlNode xnod = xd.DocumentElement;
                XmlNode xnodSection;
                ExceptionDictionary dictionary = new ExceptionDictionary();
                if (xnod.HasChildNodes)
                {
                    xnodSection = xnod.FirstChild;
                    while (xnodSection != null)
                    {
                        if (xnodSection.Name == "Statistics")
                            break;
                        xnodSection = xnodSection.NextSibling;
                    }
                    if (xnodSection == null)
                        return;

                    if (_ExStatistics.ContainsKey(alid))
                    {
                        xnodSection = xnodSection.FirstChild;
                        while (xnodSection != null)
                        {
                            XmlNamedNodeMap mapAttributes = xnodSection.Attributes;
                            if (mapAttributes == null || mapAttributes.Count != 2)
                            {
                                xnodSection = xnodSection.NextSibling;
                                continue;
                            }
                            XmlNode xnod1 = mapAttributes.Item(0);
                            XmlNode xnod2 = mapAttributes.Item(1);
                            if (xnod1.Name != "alid" || xnod2.Name != "count")
                            {
                                xnodSection = xnodSection.NextSibling;
                                continue;
                            }
                            if (Convert.ToUInt32(xnod1.Value) == alid)
                            {
                                xnod2.Value = count.ToString();
                                break;
                            }
                            else
                                xnodSection = xnodSection.NextSibling;
                        }
                    }
                    else
                    {
                        XmlElement elem = xd.CreateElement("SAlarm");
                        elem.SetAttribute("alid", alid.ToString());
                        elem.SetAttribute("count", count.ToString());
                        xnodSection.AppendChild(elem);
                    }

                    XmlTextWriter xtw = new XmlTextWriter(ConstVC.FilePath.ConfigFolder + ConstVC.FilePath.EFEMAlarmHistory, System.Text.Encoding.UTF8);
                    xtw.Formatting = Formatting.Indented;
                    xd.WriteTo(xtw);
                    xtw.Flush();
                    xtw.Close();
                }
                return;
            }
            finally
            {
                _xmlHisLocker.ReleaseWriterLock();
            }
        }
        #endregion

        #region Events
        // GUI Sink
        public event AlarmReportSendGUIEventHandler AlarmReportSendGUI;
        private void Fire_AlarmReportSendGUI(byte ALCD, ExceptionInfo ExInfo, string TIMESTAMP, bool ifPopupMessageBox, bool IsAsync = true)
        {
            if (AlarmReportSendGUI != null)
            {
                if (!IsAsync)
                {
                    AlarmReportSendGUI(ALCD, ExInfo, TIMESTAMP, ifPopupMessageBox);
                }
                else
                {
                    foreach (AlarmReportSendGUIEventHandler action in AlarmReportSendGUI.GetInvocationList())
                    {
                        try
                        {
                            action.BeginInvoke(ALCD, ExInfo, TIMESTAMP, ifPopupMessageBox, null, null);
                        }
                        catch (Exception e)
                        {
                            WriteLog(LogHeadType.Exception,
                                string.Format("Exception occurs in Fire_AlarmReportSendGUI(). Delegate Source: {0}. Reason: {1}", action.ToString(), e.Message));
                        }
                    }
                }
            }
        }


        // Variable Center Sink
        public event SetValueAndFireCallbackEventHandler SetValueAndFireCallback;
        public void Fire_SetValueAndFireCallback(Hashtable table)
        {
            if (SetValueAndFireCallback == null)
                return;
            SetValueAndFireCallback(table);
        }
        public event GetValueEventHandler GetValue;
        public object Fire_GetValue(string VariableName)
        {
            if (GetValue == null)
                return null;
            return GetValue(VariableName);
        }

        public event AlarmClearedEvent OnAlarmCleared;
        private void FireAlarmCleared(uint ALID)
        {
            if (OnAlarmCleared != null)
            {
                foreach (AlarmClearedEvent action in OnAlarmCleared.GetInvocationList())
                {
                    try
                    {
                        action.BeginInvoke(ALID, null, null);
                    }
                    catch (Exception e)
                    {
                        WriteLog(LogHeadType.Exception,
                            string.Format("Exception occurs in FireAlarmCleared(). Delegate Source: {0}. Reason: {1}", action.ToString(), e.Message));
                    }
                }
            }
        }


        public event AlarmAckEvent OnAckAlarm;
        private void Fire_AlarmAckByUser(uint ALID, DateTime ackTime, bool IsAsync = true)
        {
            if (OnAckAlarm != null)
            {
                if (!IsAsync)
                {
                    OnAckAlarm(ALID, ackTime);
                }
                else
                {
                    foreach (AlarmAckEvent action in OnAckAlarm.GetInvocationList())
                    {
                        try
                        {
                            action.BeginInvoke(ALID, ackTime, null, null);
                        }
                        catch (Exception e)
                        {
                            WriteLog(LogHeadType.Exception,
                                string.Format("Exception occurs in Fire_AlarmAckByUser(). Delegate Source: {0}. Reason: {1}", action.ToString(), e.Message));
                        }
                    }
                }
            }
        }

        #endregion

        #region Private methods
        private void WriteLog(int category, ExceptionInfo info)
        {
            if (_log == null)
                return;
            try
            {
                string remark = null;
                string head;
                switch (category)
                {
                    case 0:
                        head = String.Format("[{0}] [{1}] [ALID:{2}] {3}", info.Timestamp_clear, (string)ObjDictionary[info.agent.ToString()], info.ALID, info.ALTX);
                        remark = "Claer";
                        break;
                    case 1:
                        head = String.Format("[{0}] [{1}] [ALID:{2}] {3}", info.Timestamp_set, (string)ObjDictionary[info.agent.ToString()], info.ALID, info.ALTX);
                        remark = "Set";
                        break;
                    case 2:
                        head = String.Format("[{0}] [{1}] [ALID:{2}] {3}", info.Timestamp_ack, (string)ObjDictionary[info.agent.ToString()], info.ALID, info.ALTX);
                        remark = "Ack";
                        break;
                    default:
                        return;
                }
                _log.WriteLog(objectName, LogLevel.ERROR, LogHeadType.Exception, head, remark);
            }
            catch (Exception ee)
            {
                WriteLog(LogHeadType.Exception, "WriteLog()-" + ee.Message);
            }
        }

        private bool WriteLog(LogHeadType enLogType, string szLogMessage, string szRemark = null)
        {
            if (_log != null)
            {
                return WriteLog(LogHelper.GetDefaultLogLevel(enLogType), enLogType, szLogMessage, szRemark);
            }
            else
                return false;
        }

        private bool WriteLog(LogLevel level, LogHeadType enLogType, string szLogMessage, string szRemark = null)
        {
            if (_log != null)
                return _log.WriteLog(objectName, level, enLogType, szLogMessage, szRemark);
            else
                return false;
        }
        #endregion

        #region Property
        public bool AlarmState
        {
            get { return (_ExQueue.Count > 0); }
        }
        #endregion

        #region Private Data
        private ExceptionDictionary ExDictionary = null;
        private Hashtable _ExQueue = null;
        private ArrayList _ExHistoryList = new ArrayList();
        private Hashtable ObjDictionary = null;
        private Hashtable _ExStatistics = new Hashtable();
        private XmlDocument xdExInfo, xdExHistory;
        private bool disposed = false;
        private AbstractLogUtility _log = null;
        private AbstractFileUtilities _fu = null;
        private static object _locker = new object();
        private static object _xmlLocker = new object();
        private static ReaderWriterLock _xmlHisLocker = new ReaderWriterLock();
        private AutoResetEvent _event = new AutoResetEvent(false);
        private AutoResetEvent _eventExit = new AutoResetEvent(false);
        #endregion
    }

    public class NumericComparerDescent : IComparer
    {
        public int Compare(object a, object b)
        {
            if ((int)a > (int)b)
                return -1;
            if ((int)a < (int)b)
                return 1;
            else
                return 0;
        }
    }
}
