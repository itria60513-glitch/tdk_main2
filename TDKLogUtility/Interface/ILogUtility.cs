using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TDKLogUtility.Module
{
    /// <summary>
    /// Summary description for LogUtility.
    /// </summary>
    /// 
    #region delegates
    public delegate void ForceWritesEventHandler();
    public delegate void BufferSizeChangedEventHandler(int bufferSize);
    public delegate void MainDirectoryChangedEventHandler(string directory);
    public delegate void LogListChangedEventHandler(Hashtable list);
    #endregion

    public interface ILogUtility
    {
        #region Methods
        bool WriteLog(string szKey, string szLogMessage);
        bool WriteLog(string szKey, LogHeadType enLogType, string szLogMessage);
        bool WriteLog(string szKey, LogHeadType enLogType, string szLogMessage, string szRemark);
        bool WriteLog(string szKey, LogHeadType enLogType, LogCateType enCateType, string szLogMessage, string szRemark = null);
        bool WriteLogWithSecured(string szLogKey, LogHeadType enLogType, string szLogMessage, string[] SecuredSections, string szRemark = null);
        bool WriteLogWithSecured(string szLogKey, LogHeadType enLogType, LogCateType enCateType, string szLogMessage, string[] SecuredSections, string szRemark = null);
        bool IsEnableDebugLog { get; }
        string MainLogDirectory { get; set; }
        int BufferSizeInKB { get; set; }
        int AutoFlushTimerMinutes { get; set; }
        int DaysForPerservingLog { get; }
        Hashtable ActiveLogList { get; }
        
        #endregion

        #region Events
        event ForceWritesEventHandler ForceWritesEvent;
        event BufferSizeChangedEventHandler BufferSizeChangedEvent;
        event MainDirectoryChangedEventHandler MainDirectoryChangedEvent;
        event LogListChangedEventHandler LogListChangedEvent;
        #endregion
    }

    public abstract class RemotelyDelegatableObject : MarshalByRefObjectEx
    {
        protected abstract void InternalForceWritesEvent();
        public void ForceWritesEvent()
        {
            InternalForceWritesEvent();
        }

        protected abstract void InternalBufferSizeChangedEvent(int size);
        public void BufferSizeChangedEvent(int size)
        {
            InternalBufferSizeChangedEvent(size);
        }

        protected abstract void InternalMainDirectoryChangedEvent(string directory);
        public void MainDirectoryChangedEvent(string directory)
        {
            InternalMainDirectoryChangedEvent(directory);
        }

        protected abstract void InternalLogListChangedEvent(Hashtable list);
        public void LogListChangedEvent(Hashtable list)
        {
            InternalLogListChangedEvent(list);
        }
    }
}
