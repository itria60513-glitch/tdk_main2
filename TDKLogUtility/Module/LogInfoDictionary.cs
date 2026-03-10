using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDKLogUtility.Module
{
    /// <summary>
	/// Summary description for RemotingObjectDictionary.
	/// </summary>
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
}
