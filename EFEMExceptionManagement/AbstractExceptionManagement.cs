using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Linq;
using System.Text;
using EFEM.FileUtilities;

namespace EFEM.ExceptionManagements
{
    public delegate void AlarmReportSendGUIEventHandler(byte ALCD, ExceptionInfo ExInfo, string TIMESTAMP, bool ifPopupMessageBox);
    public delegate void SetValueAndFireCallbackEventHandler(Hashtable table);
    public delegate object GetValueEventHandler(string VariableName);
    public delegate void AlarmClearedEvent(UInt32 ALID);
    public delegate void AlarmAckEvent(UInt32 ALID, DateTime ackTime);

    public interface AbstractExceptionManagement : IDisposable
    {
        #region public Interface
        ExceptionDictionary GetExceptionDictionary(string objectname);
        string GetTimeStamp();
        bool AlarmSet(string TimeStamp, UInt32 ALID);
        bool AlarmSet(string TimeStamp, UInt32 ALID, bool ifPopupMessageBox);
        bool AlarmSet(string TimeStamp, UInt32 ALID, string AdditionalMessage);
        bool AlarmClear(UInt32 ALID);
        bool AlarmAcknowledge(string TimeStamp, UInt32 ALID);
        Hashtable ListActiveAlarmRequest();
        ArrayList ListHistoryAlarmRequest();
        //ArrayList GetAlarmStatistics();
        //ArrayList GetAlarmStatisticsByModule();
        //bool IfAnyNonEmergencyShutdownWarning();
        bool IfAutoClearAlarm(UInt32 ALID);
        bool IsAnyAlarmExist { get; }
        #endregion

        #region Events
        event AlarmReportSendGUIEventHandler AlarmReportSendGUI;
        event SetValueAndFireCallbackEventHandler SetValueAndFireCallback;
        event GetValueEventHandler GetValue;
        event AlarmClearedEvent OnAlarmCleared;
        event AlarmAckEvent OnAckAlarm;
        #endregion

        bool AlarmState
        {
            get;
        }
    }

    public abstract class AbstractExceptionManagement4GUI_Sink
    {
        public void AlarmReportSend(byte ALCD, ExceptionInfo ExInfo, string TIMESTAMP, bool ifPopupMessageBox)
        {
            AlarmReportSend_Sink(ALCD, ExInfo, TIMESTAMP, ifPopupMessageBox);
        }
        abstract protected void AlarmReportSend_Sink(byte ALCD, ExceptionInfo ExInfo, string TIMESTAMP, bool ifPopupMessageBox);

        public void AlarmAck(uint ALID, DateTime ackTime)
        {
            AlarmAck_Sink(ALID, ackTime);
        }
        abstract protected void AlarmAck_Sink(uint ALID, DateTime ackTime);
    }

    [Serializable]
    public class ExceptionDictionary : IDictionary
    {
        public Hashtable innerHash;

        #region Constructors
        public ExceptionDictionary()
        {
            innerHash = new Hashtable();
        }
        public ExceptionDictionary(ExceptionDictionary original)
        {
            innerHash = new Hashtable(original.innerHash);
        }
        public ExceptionDictionary(IDictionary dictionary)
        {
            innerHash = new Hashtable(dictionary);
        }
        public ExceptionDictionary(int capacity)
        {
            innerHash = new Hashtable(capacity);
        }
        #endregion

        #region Implementation of IDictionary
        public ExceptionDictionaryEnumerator GetEnumerator()
        {
            return new ExceptionDictionaryEnumerator(this);
        }
        System.Collections.IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new ExceptionDictionaryEnumerator(this);
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
        public void Add(string key, ExceptionInfo value)
        {
            innerHash.Add(key, value);
        }
        void IDictionary.Add(object key, object value)
        {
            Add((string)key, (ExceptionInfo)value);
        }
        public bool IsReadOnly
        {
            get
            {
                return innerHash.IsReadOnly;
            }
        }
        public ExceptionInfo this[string key]
        {
            get
            {
                object tmp = innerHash[key];
                if (tmp == null)
                    return null;
                return (ExceptionInfo)innerHash[key];
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
                this[(string)key] = (ExceptionInfo)value;
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
        public void CopyTo(ExceptionDictionary wsc, int index)
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

                wsc.Add(keys.Current as string, values.Current as ExceptionInfo);
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
    public class ExceptionDictionaryEnumerator : IDictionaryEnumerator
    {
        private IDictionaryEnumerator innerEnumerator;

        internal ExceptionDictionaryEnumerator(ExceptionDictionary enumerable)
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
        public ExceptionInfo Value
        {
            get
            {
                return (ExceptionInfo)innerEnumerator.Value;
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
}
