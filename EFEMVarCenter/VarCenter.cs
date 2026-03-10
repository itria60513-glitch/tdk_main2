using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Xml;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using EFEM.DataCenter;
using EFEM.LogUtilities;

namespace EFEM.VariableCenter
{
    public enum Sender
    {
        Host = 0,
        Ctrl = 1,
        FFU = 2,
    }

    public enum MsgType
    {
        NONE = -1,
        INFO = 0,
        STATUS = 1,
        SEND = 2,
        RECV = 3
    }

    //public enum CommPackageType
    //{
    //    Send = 0,
    //    RecSuccess = 1,
    //    RecError = 2
    //}

    public delegate void CommunicationCallbackEventHandler(Sender sender, MsgType type, string commPackage, bool isErr = false);
    public delegate void VariableCenterCallbackEventHandler(string VariableName, object Value);

    public abstract class AbstractVariableCenter_Sink
    {
        public void VariableCenterCallback(string VariableName, object Value)
        {
            VariableCenterCallback_Sink(VariableName, Value);
        }
        abstract protected void VariableCenterCallback_Sink(string VariableName, object Value);
    }

    public struct NotExistVariable
    {
        public override string ToString()
        {
            return "VariableNotExist";
        }
    }

    public class SubscribersList
    {
        private Hashtable table = new Hashtable();
        private string variableName = "";
        private string subscriberList = "";
        public SubscribersList(string varName, string list)
        {
            try
            {
                subscriberList = list;
                string[] str = subscriberList.Split(',');
                variableName = varName;
                foreach (string s in str)
                {
                    this.AddSubscriber(s);
                }
            }
            catch (Exception ex)
            {
                Exception e = new Exception("[SubscribersList]" + ex.Message, ex);
                throw e;
            }
        }
        public string VariableName
        {
            set { this.variableName = value; }
            get { return this.variableName; }
        }
        public void AddSubscriber(string name)
        {
            if (!table.ContainsKey(name))
            {
                table.Add(name, variableName);
            }
        }
        public bool isSubscriber(object id)
        {
            if (id == null) return false;
            return isSubscriber(id.ToString());
        }
        public bool isSubscriber(string id)
        {
            if (subscriberList.Length == 0 || table.ContainsKey(id))
            {
                return true;
            }
            return false;
        }
    }

    public struct SystemVariable
    {
        private object variableValue;

        public SystemVariable(object val)
        {
            variableValue = val;
        }

        public object VariableValue
        {
            get { return variableValue; }
            set { variableValue = value; }
        }
    }

    public class EFEMVariableCenter : IDisposable
    {
        #region Private members
        private bool disposed = false;
        private string ObjectName = ConstVC.ObjectName.VariableCenter;
        private Hashtable VariableDictionary = new Hashtable();
        private Hashtable VariableSubscriberList = new Hashtable();
        private Hashtable VariableSubscriberMapIds = new Hashtable();
        private Hashtable VariableValues = new Hashtable();
        private string VariableCenterXML = ConstVC.FilePath.ConfigFolder + ConstVC.FilePath.EFEMVariableCenter;
        public event VariableCenterCallbackEventHandler VariableCenterCallback = null;
        public event CommunicationCallbackEventHandler CommunicationCallback = null;
        private AbstractLogUtility _log = null;
        #endregion

        #region static members
        private static NotExistVariable notExistError = new NotExistVariable();
        private static object LockObj = new object();
        private static EFEMVariableCenter instance = null;
        #endregion

        private EFEMVariableCenter()
        {
            _log = LogUtility.GetUniqueInstance();
            WriteLog(LogHeadType.System_NewStart, "");
        }

        public static EFEMVariableCenter GetUniqueInstance()
        {
            if (instance == null)
                instance = new EFEMVariableCenter();

            return instance;
        }

        public bool IsNotExistVariableReturn(object rtnValue)
        {
            return (rtnValue == null) || (rtnValue is NotExistVariable);
        }

        #region Private Member Functions

        protected bool WriteLog(LogHeadType enLogType, string szLogMessage, string szRemark = null)
        {
            if (_log != null)
            {
                return WriteLog(LogHelper.GetDefaultLogLevel(enLogType), enLogType, szLogMessage, szRemark);
            }
            else
                return false;
        }

        protected bool WriteLog(LogLevel level, LogHeadType enLogType, string szLogMessage, string szRemark = null)
        {
            if (_log != null)
                return _log.WriteLog(ObjectName, level, enLogType, szLogMessage, szRemark);
            else
                return false;
        }

        private ArrayList InitializeDictionary()
        {
            ArrayList List = new ArrayList();
            try
            {
                WriteLog(LogHeadType.MethodEnter, "", "InitializeDictionary()");

                // read the xml file
                Monitor.Enter(LockObj);
                XmlDataDocument VariableDataDoc = new XmlDataDocument();
                XmlTextReader Reader = new XmlTextReader(VariableCenterXML);
                Reader.WhitespaceHandling = WhitespaceHandling.None;

                DataSet Set = new DataSet();
                Set = VariableDataDoc.DataSet;
                Set.ReadXmlSchema(Reader);
                Reader.Close();

                Reader = new XmlTextReader(VariableCenterXML);
                VariableDataDoc.Load(Reader);
                Reader.Close();

                XmlNode xnod = VariableDataDoc.DocumentElement;
                XmlNode xnodWorking;

                if (xnod.HasChildNodes)
                {
                    xnodWorking = xnod.FirstChild;
                    while (xnodWorking != null)
                    {
                        if (xnodWorking.NodeType == XmlNodeType.Element)
                        {
                            string skey;
                            string sSubscriberList;
                            XmlNamedNodeMap mapAttributes = xnodWorking.Attributes;

                            if (mapAttributes == null || (mapAttributes.Count != 2 && mapAttributes.Count != 3))
                            {
                                string str = string.Format("Wrong fromat of {0}", ConstVC.FilePath.EFEMVariableCenter);
                                throw new ApplicationException(str);
                            }
                            XmlNode xnod1 = mapAttributes.Item(0);
                            XmlNode xnod2 = mapAttributes.Item(1);

                            if (xnod1.Value.Contains("MappingName"))
                            {
                                //get mapping Name with mapId
                                //key = name, value = id
                                VariableSubscriberMapIds.Add(mapAttributes.Item(2).Value, xnod2.Value);
                            }
                            else
                            {
                                // get the key
                                skey = xnod1.Value;
                                sSubscriberList = xnod2.Value;

                                // set the structure to the dictionary
                                VariableDictionary[skey] = new SystemVariable(null);

                                // set subscriber list
                                this.VariableSubscriberList[skey] = new SubscribersList(skey, sSubscriberList);
                            }
                        }
                        xnodWorking = xnodWorking.NextSibling;
                    }
                }

                WriteLog(LogHeadType.MethodExit, "Success", "InitializeDictionary()");
            }
            catch (Exception e)
            {
                WriteLog(LogHeadType.MethodExit, "Fail. Reason:" + e.Message + e.StackTrace, "InitializeDictionary()");
                List.Add(e.Message);
            }

            Monitor.Exit(LockObj);

            return List;
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                }
            }
            disposed = true;
        }
        #endregion

        #region Public methods

        #region basic methods
        public ArrayList InstantiateObjects()
        {
            try
            {
                return InitializeDictionary();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region Event Fire Handle

        public void FireCommunicationCallBack(Sender sender, MsgType type, string commPackage, bool isErr, bool IsAsync = true)
        {
            if (CommunicationCallback != null)
            {
                if (!IsAsync)
                {
                    CommunicationCallback(sender, type, commPackage, isErr);
                }
                else
                {
                    foreach (CommunicationCallbackEventHandler action in CommunicationCallback.GetInvocationList())
                    {
                        try
                        {
                            action.BeginInvoke(sender, type, commPackage, isErr, null, null);
                        }
                        catch (Exception e)
                        {
                            WriteLog(LogHeadType.Exception,
                                string.Format("Exception occurs in FireCommunicationCallBack(). Delegate Source: {0}. Reason: {1}", action.ToString(), e.Message));
                        }
                    }
                }
            }
        }

        public void FireCallback(string VariableName, object Value)
        {
            string s = String.Format("FireCallback called with variable = " + VariableName + " and value = " + ExtendMethods.ToStringHelper(Value));
            WriteLog(LogHeadType.Info, s);

            Hashtable table = new Hashtable();
            table[VariableName] = Value;

            ThreadPool.QueueUserWorkItem(new WaitCallback(FireCallbackThreadPool), (object)table);

        }

        public void FireCallback(Hashtable table)
        {
            string s = String.Format("FireCallback called with hash table");
            WriteLog(LogHeadType.Info, s);

            ThreadPool.QueueUserWorkItem(new WaitCallback(FireCallbackThreadPool), (object)table);
        }

        private void FireCallbackThreadPool(object Obj)
        {
            try
            {
                Hashtable table = (Hashtable)Obj;
                foreach (DictionaryEntry obj in table)
                {
                    #region Check Variable Value

                    if (VariableValues.ContainsKey(obj.Key))
                    {
                        if (VariableValues[obj.Key].Equals(obj.Value))
                        {
                            //same value , don't fire callback
                            continue;
                        }
                        else
                        {
                            VariableValues.Remove(obj.Key);
                        }
                    }
                    VariableValues.Add(obj.Key, obj.Value);
                    #endregion

                    if (!VariableDictionary.ContainsKey(obj.Key))
                        addNewVariable(obj.Key.ToString(), obj.Value);
                }
            }
            catch (Exception e)
            {
                WriteLog(LogHeadType.Exception_Software, "FireCallbackThreadPool thrown an exception " + e.Message);
            }
        }

        private void CheckCallBackThreadTable()
        {
            if (VariableCenterCallback != null)
            {
                Delegate[] invcList;
                invcList = VariableCenterCallback.GetInvocationList();
                //remove old del and add new del
                bool cur_found = false;
                ArrayList list = new ArrayList();
                list.AddRange(threadTable.Keys);
                foreach (Delegate cur_del in list)
                {
                    foreach (Delegate del in invcList)
                    {
                        if (cur_del == del)
                        {
                            cur_found = true;
                            break;
                        }

                    }
                    if (!cur_found)
                    {
                        threadTable.Remove(cur_del);
                        this.WriteLog(LogHeadType.Info, "***** Remove old Delegate: " + cur_del.Target.ToString());
                    }
                }
                foreach (Delegate del in invcList)
                {
                    string debug = del.Target.ToString();
                    if (threadTable[del] == null)
                    {
                        //prepare and add new thread
                        checkAndPrepareThreadTable(del);
                        this.WriteLog(LogHeadType.Info, "***** Add new Delegate: " + del.Target.ToString());
                    }
                }
            }
            return;
        }
        private Exception FireEvent(string key, object data)
        {
            this.enqueueValue(key, data, true);
            return null;
        }
        private Exception FireEvent(Hashtable table)
        {
            this.enqueueValue(table);
            return null;
        }

        private void _thread_FireEvent(object Obj)
        {
            ArrayList al = (ArrayList)Obj;
            string key = (string)al[0];
            object data = (object)al[1];

            object subscriberList = this.VariableSubscriberList[key];
            if (VariableCenterCallback != null)
            {
                Delegate[] invcList;
                invcList = VariableCenterCallback.GetInvocationList();
                foreach (Delegate del in invcList)
                {
                    string name = del.Target.GetType().ToString();
                    try
                    {
                        if (subscriberList == null ||
                            ((SubscribersList)subscriberList).isSubscriber(this.VariableSubscriberMapIds[name]))
                        {
                            del.Method.Invoke(del.Target, new object[] { key, data });
                        }
                    }
                    catch (Exception e)
                    {
                        VariableCenterCallback = (VariableCenterCallbackEventHandler)Delegate.Remove(VariableCenterCallback, del);
                        WriteLog(LogHeadType.Exception_Software, string.Format("Fire Callback Event key = {0}, value={1}", key, data == null ? "null" : data));
                        WriteLog(LogHeadType.Exception_Software, "Variablecenter encountered and error while firing callback and removed the entry from the list " + name + "\r\n");
                        WriteLog(LogHeadType.Exception_Software, e.Message + "\r\n" + e.StackTrace);
                        if (e.InnerException != null)
                        {
                            WriteLog(LogHeadType.Exception_Software, e.InnerException.Message + "\r\n" + e.InnerException.StackTrace);
                        }
                    }
                }
            }
        }

        #endregion

        #region Set and Get Value

        private void addNewVariable(string VariableName, object Value)
        {
            SystemVariable Var = new SystemVariable(Value);
            VariableDictionary[VariableName] = Var;
            VariableSubscriberList[VariableName] = new SubscribersList(VariableName, ConstVC.VariableCenter.SubscriberForNewVariable);
            WriteLog(LogHeadType.Info, string.Format("--->New variable ({0}) added in list.", VariableName));
        }

        public Exception SetValue(string VariableName, object Value)
        {
            string s = String.Format("SetValue called with VariableName = {0} and value = {1}", VariableName, ExtendMethods.ToStringHelper(Value));
            WriteLog(LogHeadType.Info, s);

            try
            {
                if (VariableDictionary.ContainsKey(VariableName))
                {
                    SystemVariable Var = new SystemVariable(null);
                    lock (this)
                    {
                        Var = (SystemVariable)VariableDictionary[VariableName];
                        Var.VariableValue = Value;
                        VariableDictionary[VariableName] = Var;
                    }
                }
                else
                {
                    addNewVariable(VariableName, Value);
                }
            }
            catch (Exception e)
            {
                return e;
            }


            return null;
        }

        public Exception SetValue(Hashtable table)
        {
            try
            {
                string s = String.Format("Setvalue called with hashtable size = {0}", table.Count);
                WriteLog(LogHeadType.Info, s);

                SystemVariable Var = new SystemVariable(null);
                foreach (DictionaryEntry obj in table)
                {
                    if (VariableDictionary.ContainsKey(obj.Key))
                    {
                        lock (this)
                        {
                            Var = (SystemVariable)VariableDictionary[obj.Key];
                            Var.VariableValue = obj.Value;

                            VariableDictionary[obj.Key] = Var;
                        }
                    }
                    else
                    {
                        addNewVariable(obj.Key.ToString(), obj.Value);
                    }
                }
            }
            catch (Exception e)
            {
                return e;
            }

            return null;
        }

        public bool IsVariableExist(string VariableName)
        {
            return VariableDictionary.ContainsKey(VariableName);
        }

        public object GetValue(string VariableName)
        {
            object Value = null;

            WriteLog(LogHeadType.Info, "GetValue method called for Variable Name = " + VariableName);

            if (VariableDictionary.ContainsKey(VariableName))
            {
                SystemVariable Var = new SystemVariable(null);
                Var = (SystemVariable)VariableDictionary[VariableName];
                Value = Var.VariableValue;
                string smessage;
                smessage = String.Format("Variable Name = {0}, Value = {1}", VariableName, ExtendMethods.ToStringHelper(Var.VariableValue));
                WriteLog(LogHeadType.Info, smessage);
            }
            else
            {
                return notExistError;
            }

            return Value;
        }

        public ArrayList GetAllVariableName()
        {
            try
            {
                ArrayList list = new ArrayList();
                foreach (object s in VariableDictionary.Keys)
                {
                    list.Add(s.ToString());
                }
                return list;
            }
            catch (Exception ex)
            {
                this.WriteLog(LogHeadType.Exception_Software, "Exception in GetAllVariableName(). Reason: " + ex.Message + ex.StackTrace);
            }
            return null;
        }

        public Hashtable GetValues(ArrayList variableList)
        {
            try
            {
                Hashtable ht = new Hashtable();
                foreach (object obj in variableList)
                {
                    if (VariableDictionary.ContainsKey(obj.ToString()))
                    {
                        try
                        {
                            SystemVariable Var = new SystemVariable(null);
                            Var = (SystemVariable)VariableDictionary[obj.ToString()];
                            ht.Add(obj.ToString(), Var.VariableValue);
                        }
                        catch (Exception ex)
                        {
                            this.WriteLog(LogHeadType.Exception_Software, ex.Message + ex.StackTrace);
                            ht.Add(obj.ToString(), null);
                        }
                    }
                    else
                    {
                        ht.Add(obj.ToString(), null);
                    }
                }
                return ht;
            }
            catch (Exception ex)
            {
                this.WriteLog(LogHeadType.Exception_Software, "Exception in GetValues(). Reason: " + ex.Message + ex.StackTrace);
            }
            return null;
        }

        public Exception SetValueAndFireCallback(string VariableName, object Value, bool ForceReFire = true)
        {
            string s = String.Format("SetAndFire: VariableName = {0}, Value = {1}", VariableName, ExtendMethods.ToStringHelper(Value));
            WriteLog(LogHeadType.Info, s);

            bool NeedFireEvent = false;
            if (!VariableDictionary.ContainsKey(VariableName))
            {
                addNewVariable(VariableName, Value);
                NeedFireEvent = true;
            }

            SystemVariable Var = new SystemVariable(null);
            Var = (SystemVariable)VariableDictionary[VariableName];
            if (Var.VariableValue != null && !NeedFireEvent)
                NeedFireEvent = !(Var.VariableValue.Equals(Value));

            Var.VariableValue = Value;
            VariableDictionary[VariableName] = Var;

            CheckCallBackThreadTable();

            if (ForceReFire || NeedFireEvent)
                FireEvent(VariableName, Value);
            return null;
        }

        public Exception SetValueAndFireCallback(Hashtable table)
        {
            string s = String.Format("SetValueAndFireCallback called");
            WriteLog(LogHeadType.Info, s);
            CheckCallBackThreadTable();
            foreach (DictionaryEntry obj in table)
            {
                if (VariableDictionary.ContainsKey(obj.Key))
                {
                    SystemVariable Var = new SystemVariable(null);
                    Var = (SystemVariable)VariableDictionary[obj.Key];
                    Var.VariableValue = obj.Value;
                    VariableDictionary[obj.Key] = Var;
                }
                else
                {
                    WriteLog(LogHeadType.Exception_Software, string.Format("---> Invalid variable name {0}", (string)obj.Key));
                }
            }
            FireEvent(table);
            return null;
        }

        #endregion

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Fire Even using Queue

        private Hashtable threadTable = new Hashtable();
        private ArrayList delegateList = new ArrayList();
        private void checkAndPrepareThreadTable(Delegate del)
        {
            if (threadTable[del] == null)
            {
                ArrayList list = new ArrayList();
                // Creates and initializes a new Queue.
                Queue newQ = new Queue();
                // Creates a synchronized wrapper around the Queue.
                Queue newSyncdQ = Queue.Synchronized(newQ);
                list.Add(newSyncdQ);

                ManualResetEvent runEvent = new ManualResetEvent(false);
                list.Add(runEvent);

                Thread newThread = new Thread(new ParameterizedThreadStart(Thread_FireEvent));
                list.Add(newThread);

                threadTable[del] = list;
                //delegateList.Add(del);

                //Add by Lawrence (set as background thread)
                newThread.IsBackground = true;
                newThread.Start(del);
            }
        }
        private void enqueueValue(Hashtable table)
        {
            foreach (DictionaryEntry obj in table)
            {
                enqueueValue((string)obj.Key, obj.Value, false);
            }
            FireEventInQueue();
            return;
        }
        private void enqueueValue(string VariableName, object Value, bool needFireEvent)
        {
            addValueToQueue(VariableName, Value);
            if (needFireEvent)
            {
                FireEventInQueue();
            }

            return;
        }
        private void addValueToQueue(string VariableName, object Value)
        {
            ArrayList al = new ArrayList();
            al.Add(VariableName);
            al.Add(Value);

            ArrayList keyList = new ArrayList();
            keyList.AddRange(threadTable.Keys);

            foreach (Delegate del in keyList)
            {
                if (threadTable[del] != null)
                {
                    ArrayList list = (ArrayList)threadTable[del];
                    Queue newSyncdQ = (Queue)list[0];

                    object subscriberList = this.VariableSubscriberList[VariableName];
                    string name = del.Target.GetType().ToString();
                    try
                    {
                        if (subscriberList == null ||
                            ((SubscribersList)subscriberList).isSubscriber(this.VariableSubscriberMapIds[name]))
                        {
                            newSyncdQ.Enqueue(al);
                        }
                    }
                    catch (Exception e)
                    {
                        WriteLog(LogHeadType.Exception_Software, "Exception in addValueToQueue(). Reason: " + e.Message + "\r\n" + e.StackTrace);
                    }
                }
            }
        }
        private void FireEventInQueue()
        {
            foreach (ArrayList list in threadTable.Values)
            {
                ManualResetEvent runEvent = (ManualResetEvent)list[1];
                runEvent.Set();
            }
        }
        private void Thread_FireEvent(object obj)
        {
            Delegate del = (Delegate)obj;
            bool run = true;
            Random random = new Random(del.GetHashCode());
            int errorCount = 0;
            while (run)
            {
                if (threadTable[del] != null)
                {
                    ArrayList list = (ArrayList)threadTable[del];
                    Queue q = (Queue)list[0];
                    ManualResetEvent runEvent = (ManualResetEvent)list[1];
                    while (run)
                    {
                        if (q.Count > 0 && run)
                        {
                            //fire event
                            ArrayList varList = (ArrayList)q.Dequeue();
                            object key = varList[0];
                            object data = varList[1];
                            try
                            {
                                del.Method.Invoke(del.Target, new object[] { key, data });
                            }
                            catch (Exception e)
                            {
                                errorCount++;
                                if (errorCount >= 3)
                                {
                                    //stop Thread
                                    run = false;
                                    try
                                    {
                                        VariableCenterCallback = (VariableCenterCallbackEventHandler)Delegate.Remove(VariableCenterCallback, del);
                                    }
                                    catch (Exception ex) { }

                                    //log error
                                    WriteLog(LogHeadType.Exception_Software, string.Format("Fire Callback Event key = {0}, value={1}", key, data == null ? "null" : data));
                                    string name = del.Target.GetType().ToString();
                                    WriteLog(LogHeadType.Exception_Software, "Variablecenter encountered and error while firing callback and removed the entry from the list " + name + "\r\n");
                                    WriteLog(LogHeadType.Exception_Software, e.Message + "\r\n" + e.StackTrace);
                                    if (e.InnerException != null)
                                    {
                                        WriteLog(LogHeadType.Exception_Software, e.InnerException.Message + "\r\n" + e.InnerException.StackTrace);
                                    }
                                }
                            }
                        }
                        else
                        {
                            //wait here for new item in queue
                            runEvent.Reset();
                            //try to scramble all different threads' timing...
                            runEvent.WaitOne(200 + random.Next(10, 100), false);
                        }
                    }
                }
                Thread.Sleep(250);
                if (!run)
                {
                    Thread.Sleep(1000);
                    threadTable.Remove(del);
                }
            }
        }
        private void removeDelegate(Delegate del)
        {
            VariableCenterCallback = (VariableCenterCallbackEventHandler)Delegate.Remove(VariableCenterCallback, del);
            threadTable[del] = null;
        }
        #endregion
    }

}
