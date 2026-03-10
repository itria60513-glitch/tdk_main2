using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Communication.Connector.Enum;

namespace Communication.Connector
{
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
            get { return innerHash.IsReadOnly; }
        }
        public ExceptionInfo this[string key]
        {
            get
            {
                if (innerHash.ContainsKey(key))
                {
                    return (ExceptionInfo)innerHash[key];
                }

                // SBISW-31726
                // Below will execute when using key not defined in ExceptionInfo.xml.
                // We need provide formatted message let alarm system can identify.
                // Also provide additional caller information to trace afterwards.
                string message = $"[Undefined] Key is {key}. Please feedback to sw developer and enhance it.";
                var caller = new StackTrace(1).GetFrame(0).GetMethod();
                return CreateErrorDefaultExceptionInfo(message, caller);
            }
            set { innerHash[key] = value; }
        }

        object IDictionary.this[object key]
        {
            get
            {
                if (key is string stringKey)
                {
                    return this[stringKey];
                }

                // SBISW-31726
                // Below will execute when using type not allowed.
                // We need provide formatted message let alarm system can identify.
                // Also provide additional caller information to trace afterwards.
                string message = $"[Undefined] Key should be string type but get {key.GetType()} type";
                var caller = new StackTrace(1).GetFrame(0).GetMethod();
                return CreateErrorDefaultExceptionInfo(message, caller);
            }
            set
            {
                if (!(key is string stringKey))
                {
                    return;
                }

                if (!(value is ExceptionInfo exceptionInfo))
                {
                    return;
                }

                this[stringKey] = exceptionInfo;
            }
        }
        public System.Collections.ICollection Values
        {
            get { return innerHash.Values; }
        }
        public System.Collections.ICollection Keys
        {
            get { return innerHash.Keys; }
        }
        public bool IsFixedSize
        {
            get { return innerHash.IsFixedSize; }
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
            get { return innerHash.IsSynchronized; }
        }
        public int Count
        {
            get { return innerHash.Count; }
        }
        public object SyncRoot
        {
            get { return innerHash.SyncRoot; }
        }
        #endregion

        private static ExceptionInfo CreateErrorDefaultExceptionInfo(string message, MethodBase caller)
        {
            string remedy =
                $"[Undefined][Caller] Name is {caller.Name}. [Module] Name is {caller.Module.Name}";
            return new ExceptionInfo(
                ExceptionInfo.MaxExceptionCategoryId,
                ExceptionInfo.MaxExceptionKeyId,
                message,
                ExCategory.ex_Notify,
                ExMode.ex_Auto,
                remedy,
                uint.MaxValue);
        }
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
            get { return (string)innerEnumerator.Key; }
        }

        object IDictionaryEnumerator.Key
        {
            get { return Key; }
        }

        public ExceptionInfo Value
        {
            get { return (ExceptionInfo)innerEnumerator.Value; }
        }

        object IDictionaryEnumerator.Value
        {
            get { return Value; }
        }

        public System.Collections.DictionaryEntry Entry
        {
            get { return innerEnumerator.Entry; }
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
            get { return innerEnumerator.Current; }
        }

        public DictionaryEntry Current
        {
            get { return Entry; }
        }

        #endregion
    }
}
