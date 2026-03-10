using EFEM.ExceptionManagements;
using System;

namespace Communication.Connector.Enum
{
    public enum ExCategory
    {
        ex_Notify,
        ex_Warning,
        ex_Error,
        ex_Alarm,
    }
    public enum ExMode
    {
        // An exception with manual clear-up means user is responsible for clearing it from GUI. And exception 
        // agent is no more in charge of keeping its state and reporting while it is cleared.
        ex_Manual = 0,
        // An exception with auto clear up cannot be cleared by user. The exception agent has to clear it by 
        // firing an AlarmClear message while a exception is no more existing. If an exception will last for 
        // some time (e.g. short supply of nitrogen) or require a recovery action, it should fall into this category.
        ex_Auto = 1
    }
    [Serializable]
    public class HRESULT
    {
        public UInt16 _agent, _id;
        public ExCategory _category;
        public string _message;
        public string _timestamp;
        public string _remedy;
        public uint _gemalid;
        public string _extramessage = "";

        public HRESULT(UInt16 agent, UInt16 id, ExCategory category, string message, string timestamp, string remedy, uint gemalid)
        {
            _agent = agent;
            _id = id;
            _category = category;
            _message = message;
            _timestamp = timestamp;
            _remedy = remedy;
            _gemalid = gemalid;
        }
        public uint ALID
        {
            get
            {
                ALIDConverter conv = new ALIDConverter(_agent, _id);
                return conv.ALID;
            }
        }

    }
    [Serializable]
    public class ExceptionInfo
    {
        /// <summary>
        /// Alarm code encode rule : {Category:2}{Key:3}
        /// Because category max digit is 2 so 99 is max category id.
        /// </summary>
        public const int MaxExceptionCategoryId = 99;

        /// <summary>
        /// Alarm code encode rule : {Category:2}{Key:3}
        /// Because key max digits is 3 so 999 is max key id.
        /// </summary>
        public const int MaxExceptionKeyId = 999;

        private UInt16 _agent, _id;
        private ExCategory _category;
        private ExMode _mode;
        private string _message;
        private string _remedy;
        private ExStateReporting _state;
        public string _timestamp_set;   // This attribute is for exception management use only.
        public string _timestamp_ack;   // This attribute is for exception management use only.
        public string _timestamp_clear; // This attribute is for exception management use only.
        private uint _gemalid;

        public ExceptionInfo(UInt16 agent, UInt16 id, string message, ExCategory category,
                                ExMode mode, string remedy, uint gemalid)
        {
            _agent = agent;
            _id = id;
            _message = message;
            _category = category;
            _state = ExStateReporting.CLEARED;
            _remedy = remedy;
            _gemalid = gemalid;
            if (category == ExCategory.ex_Alarm || category == ExCategory.ex_Error ||
                category == ExCategory.ex_Warning)
            {
                _mode = mode;
            }
            else
            {
                _mode = ExMode.ex_Manual;
            }
        }

        public ExceptionInfo(ExceptionInfo a)
        {
            _agent = a._agent;
            _id = a._id;
            _message = a._message;
            _category = a._category;
            _state = a._state;
            _remedy = a._remedy;
            _mode = a._mode;
            _gemalid = a.GemALID;
        }

        public UInt16 id
        {
            get { return _id; }
        }

        public UInt16 agent
        {
            get { return _agent; }
        }

        public ExCategory category
        {
            get { return _category; }
        }

        public ExMode mode
        {
            get { return _mode; }
        }

        public string mesage
        {
            get { return _message; }
        }

        public string remedy
        {
            get { return _remedy; }
            set
            {
                _remedy = value;
            }
        }

        public ExStateReporting state
        {
            get { return _state; }
            set
            {
                //if(_category==ExCategory.ex_Alarm || _category==ExCategory.ex_Error || 
                //	category==ExCategory.ex_Warning)
                _state = value;
                //else
                //	_state = ExStateReporting.CLEARED;
            }
        }

        public uint ALID
        {
            get
            {
                ALIDConverter conv = new ALIDConverter(_agent, _id);
                return conv.ALID;
            }
        }

        public string ALTX
        {
            get { return _message; }
        }

        public HRESULT hRESULT

        {
            get
            {
                return new HRESULT(_agent, _id, _category, _message, GetTimeStamp(), _remedy, _gemalid);
            }
        }

        public string GetTimeStamp()
        {
            DateTime dt = DateTime.Now;
            string str = string.Format("{0}{1}{2}{3}{4}{5}{6}", dt.Year.ToString(), dt.Month.ToString("00"), dt.Day.ToString("00"), dt.Hour.ToString("00"), dt.Minute.ToString("00"), dt.Second.ToString("00"), Convert.ToInt32(dt.Millisecond / 10).ToString("00"));
            return str;
        }

        public uint GemALID
        {
            get
            {
                return _gemalid;
            }
        }
    }
    public class ALIDConverter
    {
        public ALIDConverter(UInt32 ALID)
        {
            _ALID = ALID;
            _agent = (UInt16)(_ALID / 1000);
            _id = (UInt16)(_ALID - (UInt32)(_agent) * 1000);
        }
        public ALIDConverter(UInt16 agent, UInt16 id)
        {
            _agent = agent;
            _id = id;
            _ALID = (UInt32)(_agent) * 1000 + _id;
        }
        private UInt32 _ALID;
        private UInt16 _agent;
        private UInt16 _id;
        public UInt32 ALID { get { return _ALID; } }
        public UInt16 agent { get { return _agent; } }
        public UInt16 id { get { return _id; } }
    }
}