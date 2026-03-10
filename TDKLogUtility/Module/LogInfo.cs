using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDKLogUtility.Module
{
    /// <summary>
	/// Summary description for LogInfo.
	/// </summary>
    public class LogInfo
    {
        private StreamWriter Writer = null;
        private bool bLogRequired;
        int logPart = 0;
        private long totalLength = 0;
        private int logMaxLength = 30 * 1024 * 1024;//30MB
        private int bufferSize = 4 * 1024;//4KB
        private string logPath;
        private string logKey;

        public LogInfo(string sPath, string sKeyName, bool bLogReqd, int bufferSizeInKB)
        {
            bLogRequired = bLogReqd;
            BufferSizeInKB = bufferSizeInKB;
            CreateStreamWriter(sPath, sKeyName);
        }

        #region Public Methods
        public void Close()
        {
            if (Writer != null)
            {
                Writer.Flush();
                Writer.Close();
                Writer = null;
            }
        }

        public void WriteLine(string sMessage)
        {
            if (Writer == null) return;
            Writer.WriteLine(sMessage);

            totalLength += sMessage.Length;
            if (totalLength >= logMaxLength)
                CreateStreamWriter(logPath, logKey);
        }

        public void Flush()
        {
            Writer.Flush();
        }

        public void CreateStreamWriter(string sPath, string sKeyName)
        {
            logPath = sPath;
            logKey = sKeyName;

            if (Writer != null)
                this.Close();
            if (!bLogRequired)
                return;

            //check if file exist
            string szFileName = GetCurrentDayFileNameNoExt(sPath, sKeyName) + ".log";
            if (!Directory.Exists(szFileName))
            {
                Directory.CreateDirectory(sPath);
            }
            if (!File.Exists(szFileName))
            {
                logPart = 0;
                Writer = new StreamWriter(File.Open(szFileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read), System.Text.Encoding.UTF8, bufferSize);
            }
            else
            {
                while (true)
                {
                    if (logPart == 0)
                    {
                        FileInfo info = new FileInfo(szFileName);
                        if (info.Length > logMaxLength)
                            logPart++;
                        else
                        {
                            Writer = new StreamWriter(File.Open(szFileName, FileMode.Append, FileAccess.Write, FileShare.Read),
                                                        System.Text.Encoding.UTF8,
                                                        bufferSize);
                            break;
                        }
                    }
                    else
                    {
                        string path_filename = GetCurrentDayFileNameNoExt(sPath, sKeyName) + "_" + logPart.ToString("000") + ".log";
                        if (File.Exists(path_filename))
                        {
                            FileInfo info = new FileInfo(path_filename);
                            if (info.Length > logMaxLength)
                                logPart++;
                            else
                            {
                                Writer = new StreamWriter(File.Open(path_filename, FileMode.Append, FileAccess.Write, FileShare.Read),
                                                            System.Text.Encoding.UTF8,
                                                            bufferSize);
                                break;
                            }
                        }
                        else
                        {
                            Writer = new StreamWriter(File.Open(path_filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read),
                                                        System.Text.Encoding.UTF8,
                                                        bufferSize);
                            break;
                        }
                    }
                }
            }
            totalLength = Writer.BaseStream.Length;
        }
        #endregion

        #region Private Methods
        private string GetCurrentDayFileNameNoExt(string sPath, string sLogName)
        {
            return sPath + "\\" + sLogName + "_" + DateTime.Now.Date.ToString("u").Substring(0, 10);
        }
        #endregion

        #region Public Properties
        public bool LogRequired
        {
            get { return bLogRequired; }
            set { bLogRequired = value; }
        }

        public int BufferSizeInKB
        {
            set { bufferSize = value * 1024; }
        }
        /// <summary>
        /// Set max log size(MB)
        /// </summary>
        public int MaxLogSize
        {
            set { logMaxLength = value * 1024 * 1024; }
        }

        public string Key
        {
            get { return logKey; }
        }
        #endregion
    }
}
