using System;
using System.Runtime.InteropServices;

namespace EFEM.ExceptionManagements
{
	public enum ExCategory
	{
		// An unexpected or abnormal operation (ex. service rejection) has occurred, but normally all 
		// operations can continue.
		Notify = 0,
		// An abnormal condition of equipment status nearly approached, but operation can continue.
		Warning = 1,
		// An exception condition that is not an alarm and which may support recovery actions requested
		// by a decision authority.
		Error = 2,
		// An alarm is related to any abnormal situation of the equipment that may endanger people, equipment,
		// or material being processed. An alarm may support recovery actions. Intervention is required before 
		// normal use of equipment can be resumed.
		Alarm = 3
	}

	public enum ExMode
	{
		// An exception with manual clear-up means user is responsible for clearing it from GUI. And exception 
		// agent is no more in charge of keeping its state and reporting while it is cleared.
		Manual = 0,
		// An exception with auto clear up cannot be cleared by user. The exception agent has to clear it by 
		// firing an AlarmClear message while a exception is no more existing. If an exception will last for 
		// some time (e.g. short supply of nitrogen) or require a recovery action, it should fall into this category.
		Auto = 1
	}

	public enum ExStateReporting
	{
		CLEARED = 0,
		NOTPOSTED = 1,
		POSTED = 2,
		ACKNOWLEDGED = 3
	}

	public class ALIDConverter
	{
		public ALIDConverter(UInt32 ALID)
		{
			_ALID = ALID;
			_agent = (UInt16)(_ALID/1000);
			_id = (UInt16)(_ALID - (UInt32)(_agent)*1000);
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
		public UInt32 ALID { get { return _ALID;}}
		public UInt16 agent { get { return _agent;}}
		public UInt16 id { get { return _id;}}
	}

	[Serializable]
	public class HRESULT
	{
		public UInt16 _agent, _id;
		public ExCategory _category;
		public string _message; 
		public string _timestamp;
		public string _remedy;
		public uint _maperrid;
        public uint _errid;
        public string _extramessage = "";

        public HRESULT(UInt16 agent, UInt16 id, ExCategory category, string message, string timestamp, string remedy, uint maperrid, uint errid)
		{
			_agent = agent;
			_id = id;
			_category = category;
			_message = message;
			_timestamp = timestamp;
			_remedy = remedy;
			_maperrid = maperrid;
            _errid = errid;
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

	/// <summary>
	/// Summary description for ExceptionInfo.
	/// </summary>
	[Serializable]
	public class ExceptionInfo
	{
		private UInt16 _agent, _id;
		private ExCategory _category;
		private ExMode _mode;
		private string _message; 
		private string _remedy;
        private string _extraMessage;
		private ExStateReporting _state;

        private string _timestamp_set = string.Empty;
        private string _timestamp_ack = string.Empty;
        private string _timestamp_clear = string.Empty;

        private DateTime _time_set = DateTime.Now;
        private DateTime _time_ack = DateTime.Now;
        private DateTime _time_clear = DateTime.Now;


        public DateTime DateTime_Set
        {
            get { return _time_set; }
        }

        public DateTime DateTime_ACK
        {
            get { return _time_ack; }
        }

        public DateTime DateTime_Clear
        {
            get { return _time_clear; }
        }

		public string Timestamp_set	// This attribute is for exception management use only.
        {
            get { return _timestamp_set; }
            set 
            { 
                _timestamp_set = value;
                try
                {
                    _time_set = DateTime.ParseExact(value, "yyyyMMddHHmmssff", System.Globalization.CultureInfo.InvariantCulture);
                }
                catch 
                {
                    _time_set = DateTime.Now;
                }
            }
        }

        public string Timestamp_ack	// This attribute is for exception management use only.
        {
            get { return _timestamp_ack; }
            set 
            { 
                _timestamp_ack = value;
                try
                {
                    _time_ack = DateTime.ParseExact(value, "yyyyMMddHHmmssff", System.Globalization.CultureInfo.InvariantCulture);
                }
                catch 
                {
                    _time_ack = DateTime.Now;
                }
            }
        }
		public string Timestamp_clear	// This attribute is for exception management use only.
        {
            get { return _timestamp_clear; }
            set 
            { 
                _timestamp_clear = value;
                try
                {
                    _time_clear = DateTime.ParseExact(value, "yyyyMMddHHmmssff", System.Globalization.CultureInfo.InvariantCulture);
                }
                catch 
                {
                    _time_clear = DateTime.Now;
                }
            }
        }
		private uint _maperrid;
        private uint _errid;

		public ExceptionInfo(UInt16 agent, UInt16 id, string message, ExCategory category,
                                ExMode mode, string remedy, uint maperrid, uint errid)
		{
			_agent = agent;
			_id = id;
			_message = message;
			_category = category;
			_state = ExStateReporting.CLEARED;
			_remedy = remedy;
			_maperrid = maperrid;
            _errid = errid;
			if(category==ExCategory.Alarm || category==ExCategory.Error || 
				category==ExCategory.Warning)
			{
				_mode = mode;
			}
			else
			{
				_mode = ExMode.Manual;
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
            _maperrid = a.MapErrID;
            _errid = a.ErrID;
		}

		public UInt16 id
		{
			get { return _id;}
		}

		public UInt16 agent
		{
			get { return _agent;}
		}

		public ExCategory category
		{
			get { return _category;}
		}

		public ExMode mode
		{
			get { return _mode;}
		}

		public string mesage
		{
			get { return _message;}
		}

		public string remedy
		{
			get { return _remedy;}
			set
			{
				_remedy = value;
			}
		}

        public string extraMessage
        {
            get { return _extraMessage; }
            set
            {
                _extraMessage = value;
            }
        }

		public ExStateReporting state
		{
			get { return _state;}
			set
			{
			    _state = value;
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
			get {return _message;}
		}

		public HRESULT hRESULT

		{
			get
			{
				return new HRESULT(_agent, _id, _category, _message, GetTimeStamp(), _remedy, _maperrid,_errid);
			}
		}

		public string GetTimeStamp()
		{
			DateTime dt = DateTime.Now;
			string str = string.Format("{0}{1}{2}{3}{4}{5}{6}", dt.Year.ToString(), dt.Month.ToString("00"), dt.Day.ToString("00"), dt.Hour.ToString("00"), dt.Minute.ToString("00"), dt.Second.ToString("00"), Convert.ToInt32(dt.Millisecond/10).ToString("00"));
			return str;
		}

        public uint MapErrID
		{
			get
			{
				return _maperrid;
			}
		}

        public uint ErrID
        {
            get
            {
                return _errid;
            }
        }

        public override string ToString()
        {
            return string.Format("ALID = {0}:{1}-{2}. Category: {3}, Mode: {4}", ALID, ALTX, extraMessage, _category, _mode);
        }
	}

	[Serializable]
	public class AlarmStatisticsItem
	{
		public AlarmStatisticsItem(uint alid, string msg, double per, string agent, int count)
		{
			ALID = alid;
			Message = msg;
			Percentage = per;
			Agent = agent;
			Count = count;
		}

		public uint ALID = 0;
		public string Message = "";
		public double Percentage = 0.0;
		public string Agent = "";
		public int Count = 0;
	}
}
