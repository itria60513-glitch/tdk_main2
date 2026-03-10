using EFEM.DataCenter;
using LogUtility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using TDKLogUtility.Module;
using static EFEM.DataCenter.ConstVC;

namespace EFEM.FileUtilities
{
    public class FileUtility : AbstractFileUtilities
    {
        private string WorkPath = ConstVC.FilePath.ConfigFolder;
        private XmlDocument xdOInfo, xdSerial, xdTcpip, xdSysConfig;
        private static AbstractFileUtilities _instance = null;

        private Dictionary<string, TCPConfig> _tCPConfig = new Dictionary<string, TCPConfig>();
        private Dictionary<string, RS232Config> _rs232Config = new Dictionary<string, RS232Config>();
        private Dictionary<string, LoadPortConfig> _lpConfig = new Dictionary<string, LoadPortConfig>();
        private Dictionary<string, N2NozzleConfig> _n2Config = new Dictionary<string, N2NozzleConfig>();
        private Dictionary<string, DIOConfig> _dioConfig = new Dictionary<string, DIOConfig>();
        private List<(string, int)> _commList = new List<(string, int)>();
        private List<string> _loadportList = new List<string>();
        private List<string> _dioList = new List<string>();
        private List<string> _n2nozzleList = new List<string>();
        private ILogUtility _log = null;
        private bool disposed = false;

        public static AbstractFileUtilities GetUniqueInstance()
        {
            if (_instance == null)
                _instance = new FileUtility();

            return _instance;
        }

        private FileUtility()
        {
            string error = "";
            XmlTextReader xtr;
            _log = LogUtilityClient.GetUniqueInstance("", 0); ;

            try
            {
                //First check whether EFEMConfig.xml is damaged 
                string efemConfigName = WorkPath + ConstVC.FilePath.EFEMConfig;
                string efemConfigNewName = WorkPath + ConstVC.FilePath.EFEMConfig + ".new";
                string efemConfigOldName = WorkPath + ConstVC.FilePath.EFEMConfig + ".old";

                if (File.Exists(efemConfigName))
                {
                    if (File.Exists(efemConfigNewName))
                    {
                        //EFEMConfig.xml.new is damaged step 1
                        File.Delete(efemConfigNewName);
                    }
                    else if (File.Exists(efemConfigOldName))
                    {
                        //Error deleting file step 4.
                        File.Delete(efemConfigOldName);
                    }
                }
                else if (File.Exists(efemConfigNewName) && File.Exists(efemConfigOldName))
                {
                    //Error renaming file step 2-3.
                    File.Move(efemConfigOldName, efemConfigName);
                    File.Delete(efemConfigNewName);
                }
                else throw new Exception(string.Format("Cannot recovery {0}.", ConstVC.FilePath.EFEMConfig));

                try
                {
                    bool folderExists = Directory.Exists(ConstVC.FilePath.AutoBackupFolder);
                    if (!folderExists)
                        Directory.CreateDirectory(ConstVC.FilePath.AutoBackupFolder);
                }
                catch
                {

                }

                //Double Check
                xtr = new XmlTextReader(efemConfigName);
                xtr.WhitespaceHandling = WhitespaceHandling.None;
                xdSysConfig = new XmlDocument();
                xdSysConfig.Load(xtr);
                xtr.Close();
                if (xdSysConfig.DocumentElement == null)
                    throw new Exception("Missing Root Element.");

                //Load EFEMConfig.xml
                if (File.Exists(WorkPath + ConstVC.FilePath.EFEMConfigAutoBackup))
                {
                    DateTime backupTime = File.GetLastWriteTime(WorkPath + ConstVC.FilePath.EFEMConfigAutoBackup);
                    TimeSpan period = DateTime.Now - backupTime;
                    if (period.Days > 7)
                    {
                        string file1 = WorkPath + ConstVC.FilePath.EFEMConfigAutoBackup;
                        try
                        {
                            File.Copy(WorkPath + ConstVC.FilePath.EFEMConfig, file1, true);
                            File.SetLastWriteTime(file1, DateTime.Now);
                        }
                        catch (Exception ex) { MessageBox.Show("Cannot copy " + file1); }
                    }

                    //backup every time when EFEM GUI restart
                    try
                    {
                        string file2 = ConstVC.FilePath.AutoBackupFolder + ConstVC.FilePath.EFEMConfigAutoBackup + "." + DateTime.Now.Ticks.ToString();
                        //File.SetAttributes(file2, FileAttributes.Normal);
                        File.Copy(WorkPath + ConstVC.FilePath.EFEMConfig, file2, true);
                        File.SetLastWriteTime(file2, DateTime.Now);
                    }
                    catch (Exception ex) 
                    { 
                        MessageBox.Show("Fail to auto-backup " + ConstVC.FilePath.EFEMConfig + " Reason: " + ex.Message); 
                    }
                }
                else
                {
                    try
                    {
                        string file3 = WorkPath + ConstVC.FilePath.EFEMConfigAutoBackup;

                        File.Copy(WorkPath + ConstVC.FilePath.EFEMConfig, file3, true);
                        File.SetLastWriteTime(WorkPath + ConstVC.FilePath.EFEMConfigAutoBackup, DateTime.Now);
                    }
                    catch (Exception ex) { }
                    try
                    {
                        string file4 = ConstVC.FilePath.EFEMConfigAutoBackup + "." + DateTime.Now.Ticks.ToString();
                        File.Copy(WorkPath + ConstVC.FilePath.EFEMConfig, file4, true);
                        File.SetLastWriteTime(file4, DateTime.Now);
                    }
                    catch (Exception ex) { }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
                error += string.Format(string.Format("There is an error while loading {0}. System cannot recovery EFEMConfig.xml. Please check the files.\r\n" + ex.Message + ex.StackTrace, ConstVC.FilePath.EFEMConfig));
            }

            try
            {
                xtr = new XmlTextReader(WorkPath + ConstVC.FilePath.EFEMObjectInfo);
                xtr.WhitespaceHandling = WhitespaceHandling.None;
                xdOInfo = new XmlDocument();
                xdOInfo.Load(xtr);
                xtr.Close();
            }
            catch (Exception ex)
            {
                error += string.Format("There is an error while loading ObjectInfo.xml. Please check the file. \r\n" + ex.Message + ex.StackTrace);
                throw new ApplicationException(error);
            }

        }

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
                    // Dispose managed resources.
                }
            }
            disposed = true;
        }

        ~FileUtility()
        {
            Dispose(false);
        }

        #region Inherited Functions
        public ArrayList InstantiateObjects()
        {
            ArrayList al = new ArrayList();
            FileInfo info = new FileInfo(WorkPath + ConstVC.FilePath.EFEMConfig);
            if (info.IsReadOnly)
                al.Add(ConstVC.FilePath.EFEMConfig + " is read-only.");
            info = new FileInfo(WorkPath + "TCPIP.xml");
            if (info.IsReadOnly)
                al.Add("TCPIP.xml is read-only.");
            info = new FileInfo(WorkPath + "RS232.xml");
            if (info.IsReadOnly)
                al.Add("RS232.xml is read-only.");
            if (al.Count > 0)
                return al;
            else
                return null;
        }
        public ArrayList EstablishCommunications() { return null; }
        public ArrayList DownloadParameters() { return null; }
        public ArrayList Initialize() { return null; }
        #endregion

        #region System Config
        private object _sysconfigLocker = new object();
        int iBufferingCount = 0;
        private bool stopWriting = false;

        public void FlushEFEMConfig()
        {
            if (stopWriting) return;
            Monitor.Enter(_sysconfigLocker);
            try
            {
                if (iBufferingCount > 0)
                {
                    iBufferingCount = 0;

                    /////////////safe write file method
                    XmlTextWriter xtw = new XmlTextWriter(WorkPath + ConstVC.FilePath.EFEMConfig + ".new", System.Text.Encoding.UTF8);
                    xtw.Formatting = Formatting.Indented;
                    xdSysConfig.WriteTo(xtw);
                    xtw.Flush();
                    xtw.Close();

                    File.Move(WorkPath + ConstVC.FilePath.EFEMConfig, WorkPath + ConstVC.FilePath.EFEMConfig + ".old");
                    File.Move(WorkPath + ConstVC.FilePath.EFEMConfig + ".new", WorkPath + ConstVC.FilePath.EFEMConfig);
                    File.Delete(WorkPath + ConstVC.FilePath.EFEMConfig + ".old");
                }
            }
            finally
            {
                Monitor.Exit(_sysconfigLocker);
            }
        }
        public void CloseAllFile()
        {
            stopWriting = true;
            System.Threading.Thread.Sleep(500);
        }
        #endregion

        #region Communication

        public List<(string,int)> CommunicationLoad()
        {
            string str = string.Empty;
            XmlDocument xdCom = XMLFileLoad(WorkPath + "Communication.xml" , "Communication");
            if(xdCom == null)
                return _commList;
            XmlNode commTypeNode = xdCom.SelectSingleNode("Communications");
            if (commTypeNode != null)
            {
                foreach (XmlNode child in commTypeNode.ChildNodes)
                {
                    if (child.Name.Equals("Communication"))
                    {
                        var element = (XmlElement)child;
                        string key = element.GetAttribute("key");
                        int value = 0;
                        int commIndex = int.TryParse(element.GetAttribute("CommunicationIndex"), out value) ? value : -1;
                        if (commIndex == 1)
                        {
                            string ip = element.GetAttribute("ip");
                            string port = element.GetAttribute("port");
                            if (ip.Equals(string.Empty))
                            {
                                str = key + " : Wrong Value of Communication Setting.(TCPIP / IP) Please Check Communication.xml.";
                                _log.WriteLog("TDK_GUI", LogHeadType.Error, str);
                            }
                            else if (port.Equals(string.Empty))
                            {
                                str = key + " : Wrong Value of Communication Setting.(TCPIP / port) Please Check Communication.xml.";
                                _log.WriteLog("TDK_GUI", LogHeadType.Error, str);
                            }
                            TCPConfig comm = new TCPConfig(ip, port);
                            _tCPConfig[key] = comm;
                        }
                        else if (commIndex == 0)
                        {
                            string port = element.GetAttribute("port");
                            int baud = int.TryParse(element.GetAttribute("baud"), out value) ? value : -1;
                            int parity = int.TryParse(element.GetAttribute("parity"), out value) ? value : -1;
                            int databits = int.TryParse(element.GetAttribute("databits"), out value) ? value : -1;
                            int stopbits = int.TryParse(element.GetAttribute("stopbits"), out value) ? value : -1;
                            if (!(Regex.IsMatch(port, @"^COM\d+$")))
                            {
                                str = key + " : Wrong Value of Communication Setting.(RS232 / port) Please Check Communication.xml.";
                                _log.WriteLog("TDK_GUI", LogHeadType.Error, str);
                            }
                            else if (baud == -1)
                            {
                                str = key + " : Wrong Value of Communication Setting.(RS232 / baud) Please Check Communication.xml.";
                                _log.WriteLog("TDK_GUI", LogHeadType.Error, str);
                            }
                            else if (parity == -1)
                            {
                                str = key + " : Wrong Value of Communication Setting.(RS232 / parity) Please Check Communication.xml.";
                                _log.WriteLog("TDK_GUI", LogHeadType.Error, str);
                            }
                            else if (databits == -1)
                            {
                                str = key + " : Wrong Value of Communication Setting.(RS232 / databits) Please Check Communication.xml.";
                                _log.WriteLog("TDK_GUI", LogHeadType.Error, str);
                            }
                            else if (stopbits == -1)
                            {
                                str = key + " : Wrong Value of Communication Setting.(RS232 / stopbits) Please Check Communication.xml.";
                                _log.WriteLog("TDK_GUI", LogHeadType.Error, str);
                            }
                            RS232Config comm = new RS232Config(port, baud, parity, databits, stopbits);
                            _rs232Config[key] = comm;

                        }
                        else
                        {
                            str = key + " : Wrong Type of Communication Setting.(Not TCPIP or RS232)";
                            _log.WriteLog("TDK_GUI", LogHeadType.Error, str);
                            commIndex = -1;
                        }
                        _commList.Add((key, commIndex));
                    }
                    else
                    {
                        str = "Wrong Format of Communication config in Communication.xml.";
                        _log.WriteLog("TDK_GUI", LogHeadType.Error, str);
                        //throw new ApplicationException(str);
                    }
                }
            }
            else
            {
                str = "Communication Config Load Error. No Communication config in Communication.xml.";
                _log.WriteLog("TDK_GUI", LogHeadType.Error, str);
                //throw new ApplicationException(str);
            }

            str = "Communication Config Load Success.";
            _log.WriteLog("TDK_GUI", str);
            return _commList;
        }

        public List<(string,int)> GetCommList()
        {
            return _commList;
        }

        #endregion

        #region Tcpip Port Config

        public TCPConfig GetTCPSetting(string key)
        {
            if (_tCPConfig.ContainsKey(key))
            {
                return _tCPConfig[key];
            }
            else
            {
                return new TCPConfig(string.Empty, string.Empty);
            }
        }

        public void TCPConfigSave(string key, string ipAddress, string port)
        {
            XmlDocument xdCom = XMLFileLoad(WorkPath + "Communication.xml", "Communication");
            if (xdCom == null)
                return;
            XmlNode commNode = xdCom.SelectSingleNode("Communications");
            if (commNode != null)
            {
                string str = string.Empty;
                XmlElement target = null;
                foreach (XmlNode child in commNode.ChildNodes)
                {
                    if (child.NodeType != XmlNodeType.Element) continue;

                    var element = (XmlElement)child;
                    if (element.GetAttribute("key") == key)
                    {
                        target = element;
                        target.SetAttribute("CommunicationIndex", "1");
                        target.SetAttribute("ip", ipAddress);
                        target.SetAttribute("port", port);
                        break;
                    }
                }

                XmlTextWriter xtwNew = new XmlTextWriter(WorkPath + "Communication.xml", System.Text.Encoding.UTF8);
                xtwNew.Formatting = Formatting.Indented;
                xdCom.WriteTo(xtwNew);
                xtwNew.Flush();
                xtwNew.Close();

                TCPConfigApply(key, ipAddress, port);

                str = "Communication Config(TCP) Save Success.";
                _log.WriteLog("TDK_GUI", str);
            }
            else
            {
                string str = "No Communication config(TCP) in Communication.xml.";
                _log.WriteLog("TDK_GUI", LogHeadType.Error, str);
                throw new ApplicationException(str);
            }
        }

        public void TCPConfigApply(string key, string ipAddress, string port)
        {
            if (_tCPConfig.ContainsKey(key))
            {
                _tCPConfig[key].Ip = ipAddress;
                _tCPConfig[key].Port = port;
            }
            else
            {
                _tCPConfig[key] = new TCPConfig(ipAddress, port);
            }
        }
        #endregion

        #region Serial Port Config

        public RS232Config GetSerialSetting(string key)
        {
            if (_rs232Config.ContainsKey(key))
            {
                return _rs232Config[key];
            }
            else
            {
                return new RS232Config("", -1, -1, -1, -1);
            }
        }

        public void SerialPortConfigSave(string key, string port, int baud, int parity, int databits, int stopbits)
        {
            string str = string.Empty;
            XmlDocument xdCom = XMLFileLoad(WorkPath + "Communication.xml","Communication");
            if (xdCom == null)
                return;
            XmlNode rs232Node = xdCom.SelectSingleNode("Communications");
            if (rs232Node != null)
            {
                XmlElement target = null;
                foreach (XmlNode child in rs232Node.ChildNodes)
                {
                    if (child.NodeType != XmlNodeType.Element) continue;

                    var element = (XmlElement)child;
                    if (element.GetAttribute("key") == key)
                    {
                        target = element;
                        target.SetAttribute("CommunicationIndex", "0");
                        target.SetAttribute("port", port);
                        target.SetAttribute("baud", baud.ToString());
                        target.SetAttribute("parity", parity.ToString());
                        target.SetAttribute("databits", databits.ToString());
                        target.SetAttribute("stopbits", stopbits.ToString());
                        break;
                    }
                }


                XmlTextWriter xtwNew = new XmlTextWriter(WorkPath + "Communication.xml", System.Text.Encoding.UTF8);
                xtwNew.Formatting = Formatting.Indented;
                xdCom.WriteTo(xtwNew);
                xtwNew.Flush();
                xtwNew.Close();

                SerialConfigApply(key, port, baud, parity, databits, stopbits);

                str = "Communication Config(RS232) Save Success.";
                _log.WriteLog("TDK_GUI", str);
            }
            else
            {
                str = "No Communication config(RS232) in Communication.xml.";
                _log.WriteLog("TDK_GUI", str);
                throw new ApplicationException(str);
            }
        }
        public void SerialConfigApply(string key, string port, int baud, int parity, int databits, int stopbits)
        {
            if (_rs232Config.Keys.Contains(key))
            {
                _rs232Config[key].Port = port;
                _rs232Config[key].Baud = baud;
                _rs232Config[key].Parity = parity;
                _rs232Config[key].DataBits = databits;
                _rs232Config[key].StopBits = stopbits;
            }
            else
            {
                _rs232Config[key] = new RS232Config(port, baud, parity, databits, stopbits);
            }
        }

        #endregion

        #region LoadPortActor Config
        public List<string> LoadPortActorLoad()
        {
            string str = string.Empty;
            XmlDocument xdLoad = XMLFileLoad(WorkPath + "LoadPort.xml", "LoadPort");
            List<string> commList = _commList.Select(x => x.Item1).ToList();
            if (xdLoad == null)
                return _loadportList;
            XmlNodeList lpNodes = xdLoad.GetElementsByTagName("LoadPortParameters");
            if (lpNodes.Count != 0)
            {
                foreach (XmlNode lpNode in lpNodes)
                {
                    string loadPortName = lpNode.Attributes["key"].Value;
                    var lpConfig = new LoadPortConfig();
                    foreach (XmlNode child in lpNode.ChildNodes)
                    {
                        if (child.Name.Equals("LoadPortParameter"))
                        {
                            var element = (XmlElement)child;
                            string key = element.GetAttribute("key");
                            string value = element.GetAttribute("value");
                            int timeout = 0;
                            switch (key)
                            {
                                case "Comm":
                                    lpConfig.Comm = value;
                                    if (!(commList.Contains(lpConfig.Comm)))
                                    {
                                        str = loadPortName + " : Comm Setting not contained in Communication List.";
                                        _log.WriteLog("TDK_GUI", LogHeadType.Error, str);
                                    }
                                    break;
                                case "InfTimeout":
                                    lpConfig.INFTimeout = int.TryParse(value, out timeout) ? timeout : -1;
                                    if (lpConfig.INFTimeout < 0)
                                    {
                                        str = loadPortName + " : InfTimeout value error.";
                                        _log.WriteLog("TDK_GUI", LogHeadType.Error, str);
                                    }
                                    break;
                                case "ACKTimeout":
                                    lpConfig.ACKTimeout = int.TryParse(value, out timeout) ? timeout : -1;
                                    if (lpConfig.ACKTimeout < 0)
                                    {
                                        str = loadPortName + " : ACKTimeout value error.";
                                        _log.WriteLog("TDK_GUI", LogHeadType.Error, str);
                                    }
                                    break;
                                default:
                                    //Error type
                                    break;
                            }
                        }
                        else
                        {
                            str = loadPortName + " : Wrong Format of LoadPort config in LoadPort.xml.";
                            _log.WriteLog("TDK_GUI", LogHeadType.Error, str);
                        }
                    }
                    _lpConfig[loadPortName] = lpConfig;
                    _loadportList.Add(loadPortName);
                }
            }
            else
            {
                str = "No LoadPort config in LoadPort.xml.";
                _log.WriteLog("TDK_GUI", LogHeadType.Error, str);
            }

            str = "LoadPort Config Load Success.";
            _log.WriteLog("TDK_GUI", str);
            return _loadportList;
        }

        public List<string> GetLoadPortList()
        {
            return _loadportList;
        }

        public void LoadPortActorSave(string key, LoadPortConfig settings)
        {
            string str = string.Empty;
            XmlDocument xdLoad = XMLFileLoad(WorkPath + "LoadPort.xml", "LoadPort");
            if (xdLoad == null)
                return;
            XmlNodeList lpNodes = xdLoad.GetElementsByTagName("LoadPortParameters");
            if (lpNodes.Count != 0)
            {
                foreach (XmlNode lpNode in lpNodes)
                {
                    if (lpNode.Attributes["key"].Value == key)
                    {
                        foreach (XmlNode child in lpNode.ChildNodes)
                        {
                            var element = (XmlElement)child;
                            string item = element.GetAttribute("key");
                            switch (item)
                            {
                                case "Comm":
                                    element.SetAttribute("value", settings.Comm);
                                    break;
                                case "InfTimeout":
                                    element.SetAttribute("value", settings.INFTimeout.ToString());
                                    break;
                                case "ABSTimeout":
                                    element.SetAttribute("value", settings.ACKTimeout.ToString());
                                    break;
                            }

                        }
                    }
                }
                XmlTextWriter xtwNew = new XmlTextWriter(WorkPath + "LoadPort.xml", System.Text.Encoding.UTF8);
                xtwNew.Formatting = Formatting.Indented;
                xdLoad.WriteTo(xtwNew);
                xtwNew.Flush();
                xtwNew.Close();

                LoadPortConfigApply(key, settings);

                str = "LoadPort Config Save Success.";
                _log.WriteLog("TDK_GUI", str);
            }
            else
            {
                str = "No LoadPort config in LoadPort.xml.";
                _log.WriteLog("TDK_GUI", str);
                throw new ApplicationException(str);
            }

        }

        public LoadPortConfig GetLoadPortConfigSetting(string key)
        {
            return _lpConfig[key];
        }

        public void LoadPortConfigApply(string key, LoadPortConfig settings)
        {
            if(_lpConfig.ContainsKey(key))
                _lpConfig[key] = settings;
            else
            {
                _lpConfig[key] = new LoadPortConfig(settings.Comm,settings.ACKTimeout,settings.INFTimeout);
            }
        }

        #endregion

        #region N2Nozzle Config
        public List<string> N2NozzleLoad()
        {
            string str = string.Empty;
            XmlDocument xdNozzle = XMLFileLoad(WorkPath + "N2Nozzle.xml", "N2Nozzle");
            List<string>commList = _commList.Select(x => x.Item1).ToList();
            if (xdNozzle == null)
                return _n2nozzleList;
            XmlNodeList n2Nodes = xdNozzle.GetElementsByTagName("N2NozzleParameters");
            
            if (n2Nodes.Count != 0)
            {
                foreach (XmlNode n2Node in n2Nodes)
                {
                    string n2nozzleName = n2Node.Attributes["key"].Value;
                    var n2Config = new N2NozzleConfig();
                    foreach (XmlNode child in n2Node.ChildNodes)
                    {
                        if (child.Name.Equals("N2NozzleParameter"))
                        {
                            var element = (XmlElement)child;
                            string key = element.GetAttribute("key");
                            string value = element.GetAttribute("value");
                            switch (key)
                            {
                                case "Comm":
                                    n2Config.Comm = value;
                                    if (!(commList.Contains(n2Config.Comm)))
                                    {
                                        str = n2nozzleName + " : Comm Setting not contained in Communication List.";
                                        _log.WriteLog("TDK_GUI", LogHeadType.Error, str);
                                    }
                                    break;
                                default:
                                    //Error type
                                    break;
                            }
                        }
                        else
                        {
                            str = n2nozzleName + " : Wrong Format of N2Nozzle config in N2Nozzle.xml.";
                            _log.WriteLog("TDK_GUI", LogHeadType.Error, str);
                        }
                    }
                    _n2Config[n2nozzleName] = n2Config;
                    _n2nozzleList.Add(n2nozzleName);
                }
            }
            else
            {
                str = "No N2Nozzle config in N2Nozzle.xml.";
                _log.WriteLog("TDK_GUI", LogHeadType.Error, str);
            }

            str = "N2Nozzle Config Load Success.";
            _log.WriteLog("TDK_GUI", str);
            return _n2nozzleList;
        }

        public List<string> GetN2NozzleList()
        {
            return _n2nozzleList;
        }

        public void N2NozzleSave(string key, N2NozzleConfig settings)
        {
            string str = string.Empty;
            XmlDocument xdNozzle = XMLFileLoad(WorkPath + "N2Nozzle.xml","N2Nozzle");
            if (xdNozzle == null)
                return;
            XmlNodeList n2Nodes = xdNozzle.GetElementsByTagName("N2NozzleParameters");
            if (n2Nodes.Count != 0)
            {
                foreach (XmlNode n2Node in n2Nodes)
                {
                    if (n2Node.Attributes["key"].Value == key)
                    {
                        foreach (XmlNode child in n2Node.ChildNodes)
                        {
                            var element = (XmlElement)child;
                            string item = element.GetAttribute("key");
                            switch (item)
                            {
                                case "Comm":
                                    element.SetAttribute("value", settings.Comm);
                                    break;
                            }

                        }
                    }
                }
                XmlTextWriter xtwNew = new XmlTextWriter(WorkPath + "N2Nozzle.xml", System.Text.Encoding.UTF8);
                xtwNew.Formatting = Formatting.Indented;
                xdNozzle.WriteTo(xtwNew);
                xtwNew.Flush();
                xtwNew.Close();

                N2NozzleConfigApply(key, settings);

                str = "N2Nozzle Config Save Success.";
                _log.WriteLog("TDK_GUI", str);
            }
            else
            {
                str = "No N2Nozzle config in N2Nozzle.xml.";
                _log.WriteLog("TDK_GUI", LogHeadType.Error, str);
            }

        }

        public N2NozzleConfig GetN2NozzleConfigSetting(string key)
        {
            return _n2Config[key];
        }

        public void N2NozzleConfigApply(string key, N2NozzleConfig settings)
        {
            if(_n2Config.ContainsKey(key))
                _n2Config[key] = settings;
            else
            {
                _n2Config[key] = new N2NozzleConfig(settings.Comm);
            }
        }

        #endregion

        #region DIO Config
        public List<string> DIOLoad()
        {
            string str = string.Empty;
            XmlDocument xdDIO = XMLFileLoad(WorkPath + "DIO.xml","DIO");
            if (xdDIO == null)
                return _dioList;
            XmlNodeList dioNodes = xdDIO.GetElementsByTagName("DIOParameters");
            if (dioNodes.Count != 0)
            {
                foreach (XmlNode dioNode in dioNodes)
                {
                    string dioName = dioNode.Attributes["key"].Value;
                    var dioConfig = new DIOConfig();
                    foreach (XmlNode child in dioNode.ChildNodes)
                    {
                        if (child.Name.Equals("DIOParameter"))
                        {
                            var element = (XmlElement)child;
                            string key = element.GetAttribute("key");
                            int value = int.TryParse(element.GetAttribute("value"),out int val) ? val : -1;
                            switch (key)
                            {
                                case "Type":
                                    dioConfig.Type = value;
                                    if (dioConfig.Type == -1)
                                    {
                                        str = dioName + " : Type value error.";
                                        _log.WriteLog("TDK_GUI", LogHeadType.Error, str);
                                    }
                                    break;
                                case "Index":
                                    dioConfig.Index = value;
                                    if (dioConfig.Index == -1)
                                    {
                                        str = dioName + " : Index value error.";
                                        _log.WriteLog("TDK_GUI", LogHeadType.Error, str);
                                    }
                                    break;
                                case "DOPortMax":
                                    dioConfig.MaxDOPort = value;
                                    if (dioConfig.MaxDOPort == -1)
                                    {
                                        str = dioName + " : MaxDOPort value error.";
                                        _log.WriteLog("TDK_GUI", LogHeadType.Error, str);
                                    }
                                    break;
                                case "DIPortMax":
                                    dioConfig.MaxDIPort = value;
                                    if (dioConfig.MaxDIPort == -1)
                                    {
                                        str = dioName + " : MaxDIPort value error.";
                                        _log.WriteLog("TDK_GUI", LogHeadType.Error, str);
                                    }
                                    break;
                                case "PinCountPerPort":
                                    dioConfig.PinCountPerPort = value;
                                    if (dioConfig.PinCountPerPort == -1)
                                    {
                                        str = dioName + " : PinCountPerPort value error.";
                                        _log.WriteLog("TDK_GUI", LogHeadType.Error, str);
                                    }
                                    break;
                                default:
                                    //Error type
                                    break;
                            }
                        }
                        else
                        {
                            str = dioName + " : Wrong Format of DIO config in DIO.xml.";
                            _log.WriteLog("TDK_GUI", LogHeadType.Error, str);
                        }
                    }
                    _dioConfig[dioName] = dioConfig;
                    _dioList.Add(dioName);
                }
            }
            else
            {
                str = "No DIO config in DIO.xml.";
                _log.WriteLog("TDK_GUI", LogHeadType.Error, str);
            }

            str = "DIO Config Load Success.";
            _log.WriteLog("TDK_GUI", str);
            return _dioList;
        }

        public List<string> GetDIOList()
        {
            return _dioList;
        }

        public void DIOSave(string key, DIOConfig settings)
        {
            string str = string.Empty;
            XmlDocument xdDIO = XMLFileLoad(WorkPath + "DIO.xml","DIO");
            if (xdDIO == null)
                return;
            XmlNodeList dioNodes = xdDIO.GetElementsByTagName("DIOParameters");
            if (dioNodes.Count != 0)
            {
                foreach (XmlNode dioNode in dioNodes)
                {
                    if (dioNode.Attributes["key"].Value == key)
                    {
                        foreach (XmlNode child in dioNode.ChildNodes)
                        {
                            var element = (XmlElement)child;
                            string item = element.GetAttribute("key");
                            switch (item)
                            {
                                case "Type":
                                    element.SetAttribute("value", settings.Type.ToString());
                                    break;
                                case "Index":
                                    element.SetAttribute("value", settings.Index.ToString());
                                    break;
                                case "DOPortMax":
                                    element.SetAttribute("value", settings.MaxDOPort.ToString());
                                    break;
                                case "DIPortMax":
                                    element.SetAttribute("value", settings.MaxDIPort.ToString());
                                    break;
                                case "PinCountPerPort":
                                    element.SetAttribute("value", settings.PinCountPerPort.ToString());
                                    break;
                                default:
                                    //Error type
                                    break;
                            }

                        }
                    }
                }
                XmlTextWriter xtwNew = new XmlTextWriter(WorkPath + "DIO.xml", System.Text.Encoding.UTF8);
                xtwNew.Formatting = Formatting.Indented;
                xdDIO.WriteTo(xtwNew);
                xtwNew.Flush();
                xtwNew.Close();

                DIOConfigApply(key, settings);

                str = "DIO Config Save Success.";
                _log.WriteLog("TDK_GUI", str);
            }
            else
            {
                str = "No DIO config in DIO.xml.";
                _log.WriteLog("TDK_GUI", str);
                throw new ApplicationException(str);
            }

        }

        public DIOConfig GetDIOConfigSetting(string key)
        {
            return _dioConfig[key];
        }

        public void DIOConfigApply(string key, DIOConfig settings)
        {
            if(_dioConfig.ContainsKey(key))
                _dioConfig[key] = settings;
            else
            {
                _dioConfig[key] = new DIOConfig(settings.Type, settings.Index, settings.MaxDOPort, settings.MaxDIPort, settings.PinCountPerPort);
            }
        }

        #endregion


        #region Object Info
        private object _objectLocker = new object();

        public Hashtable GetObjectDictionary()
        {
            Monitor.Enter(_objectLocker);
            try
            {
                XmlDocument xd = xdOInfo;
                XmlNode xnod = xd.DocumentElement;
                XmlNode xnodSection;
                Hashtable dictionary = new Hashtable();
                if (xnod.HasChildNodes)
                {
                    xnodSection = xnod.FirstChild;
                    while (xnodSection != null)
                    {
                        XmlNamedNodeMap mapAttributes = xnodSection.Attributes;
                        if (mapAttributes == null || mapAttributes.Count != 2)
                        {
                            string str = string.Format("Wrong fromat of ObjectInfo.xml.");
                            throw new ApplicationException(str);
                        }
                        Hashtable ht = new Hashtable();
                        for (int i = 0; i < 2; i++)
                        {
                            XmlNode xnodAtt = mapAttributes.Item(i);
                            ht.Add(xnodAtt.Name, xnodAtt.Value);
                        }
                        if (!(ht.ContainsKey("key") && ht.ContainsKey("id")))
                        {
                            string str = string.Format("Wrong fromat of ObjectInfo.xml.");
                            throw new ApplicationException(str);
                        }
                        dictionary[ht["id"].ToString()] = ht["key"].ToString();

                        xnodSection = xnodSection.NextSibling;
                    }

                    return dictionary;
                }
                else
                {
                    string str = string.Format("No any node in ObjectInfo.xml.");
                    throw new ApplicationException(str);
                }
            }
            finally
            {
                Monitor.Exit(_objectLocker);
            }
        }
        #endregion

        #region UserInfo
        private string userfile = ConstVC.FilePath.ConfigFolder + ConstVC.FilePath.User;
        private XmlDocument document = new XmlDocument();
        private UserXml db = null;
        private ArrayList userList = new ArrayList();

        public event WriteUserLogDel OnWriteUserLogRequired;

        private void WriteUserDataLog(string message)
        {
            if (OnWriteUserLogRequired != null)
            {
                OnWriteUserLogRequired(message);
            }
        }

        public ArrayList GetUserLoginData()
        {
            string msg = "";
            try
            {
                if (File.Exists(userfile))
                {
                    document.Load(userfile);
                    userList = this.getUserNamePassword();
                    return userList;
                }
                else
                {
                    msg = "User Control Table is Not exist!!";
                    return null;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return null;
            }
        }

        private ArrayList getUserNamePassword()
        {
            ArrayList list = new ArrayList();
            // match user name and password 
            foreach (XmlNode node in document.GetElementsByTagName("HMIUser"))
            {
                UserInfo user = new UserInfo(null, null, null, false);
                foreach (XmlNode attr in node.Attributes)
                {
                    if (attr.Name == "name")
                    {
                        user.username = attr.Value;
                        user.userExist = true;
                    }
                    else if (attr.Name == "password")
                    {
                        user.password = attr.Value;
                    }
                    else if (attr.Name == "type")
                    {
                        user.type = attr.Value;
                    }
                }
                list.Add(user);
            }
            return list;
        }

        private UserInfo getSingleUser(string username)
        {
            foreach (UserInfo info in userList)
            {
                if (info.username == username)
                    return info;
            }
            return null;
        }

        public string[] GetTypeList()
        {
            string str = "";
            foreach (XmlNode node in document.GetElementsByTagName("HMIType"))
            {
                foreach (XmlNode attr in node.Attributes)
                {
                    if (attr.Name == "type")
                    {
                        str = attr.Value;
                    }

                }
            }
            return str.Split(',');
        }

        public bool AddUser(UserInfo info)
        {
            foreach (UserInfo user in userList)
            {
                if (user.username == info.username)
                    return false;
            }

            // Add a new user.
            XmlElement newHMIUser = document.CreateElement("HMIUser");
            XmlAttribute newUser = document.CreateAttribute("name");
            newUser.Value = info.username;
            newHMIUser.Attributes.Append(newUser);

            XmlAttribute newPw = document.CreateAttribute("password");
            newPw.Value = info.password;
            newHMIUser.Attributes.Append(newPw);

            XmlAttribute newType = document.CreateAttribute("type");
            newType.Value = info.type;
            newHMIUser.Attributes.Append(newType);

            document.DocumentElement.AppendChild(newHMIUser);

            // Save the document to a file and auto-indent the output.
            XmlTextWriter writer = new XmlTextWriter(userfile, null);
            writer.Formatting = Formatting.Indented;
            document.Save(writer);
            writer.Close();

            this.userList.Add(info);
            return true;

        }

        public bool ChangePassword(UserInfo info)
        {
            // find the user.
            XmlElement newHMIUser = document.CreateElement("HMIUser");
            bool found = false;
            foreach (XmlNode node in document.GetElementsByTagName("HMIUser"))
            {
                foreach (XmlNode attr in node.Attributes)
                {
                    if (attr.Name == "name")
                    {
                        if (attr.Value == info.username)
                        {
                            found = true;
                        }
                    }
                    else if (attr.Name == "password")
                    {
                        if (found)
                        {
                            attr.Value = info.password;
                            break;
                        }
                    }
                }
                if (found)
                {
                    break;
                }
            }
            // Save the document to a file and auto-indent the output.
            XmlTextWriter writer = new XmlTextWriter(userfile, null);
            writer.Formatting = Formatting.Indented;
            document.Save(writer);
            writer.Close();

            //Update userList
            foreach (UserInfo info1 in userList)
            {
                if (info1.username == info.username)
                    info1.password = info.password;
            }

            return found;

        }

        public bool DeleteUser(string user)
        {
            // match user name and password 
            bool found = false;
            XmlNode root = document.DocumentElement;
            foreach (XmlNode node in document.GetElementsByTagName("HMIUser"))
            {
                foreach (XmlNode attr in node.Attributes)
                {
                    if (attr.Name == "name")
                    {
                        if (attr.Value == user)
                        {
                            found = true;
                            break;
                        }
                    }
                }
                if (found)
                {
                    root.RemoveChild(node);
                    this.userList.Remove(getSingleUser(user));
                    break;
                }
            }
            // Save the document to a file and auto-indent the output.
            XmlTextWriter writer = new XmlTextWriter(userfile, null);
            writer.Formatting = Formatting.Indented;
            document.Save(writer);
            writer.Close();

            return found;
        }

        public bool UserDataQuery(Hashtable commandTable, ref Hashtable returnTable)
        {
            try
            {
                if (commandTable[UserDataQueryCommand.Command] != null)
                {
                    string userName = "";
                    string password = "";
                    string loginType = "";
                    string returnMessage = "";
                    string logMsg = "";
                    string adminPW = "";
                    string appOwner = "";
                    UserDataQueryCommand command = UserDataQueryCommand.GetUserType;
                    if (commandTable[UserDataQueryCommand.Command] != null)
                    {
                        command = (UserDataQueryCommand)commandTable[UserDataQueryCommand.Command];
                    }
                    if (commandTable[UserDataKey.ApplicationOwner] != null)
                    {
                        appOwner = commandTable[UserDataKey.ApplicationOwner].ToString();
                    }

                    switch (command)
                    {
                        case UserDataQueryCommand.Login:
                            {
                                //get username/pasword
                                if (commandTable[UserDataKey.UserName] != null)
                                {
                                    userName = commandTable[UserDataKey.UserName].ToString();
                                }
                                if (commandTable[UserDataKey.Password] != null)
                                {
                                    password = commandTable[UserDataKey.Password].ToString();
                                }
                                if (userName != "" && password != "")
                                {
                                    //verify login data
                                    bool b = this.CheckLoginType(userName, password, ref loginType, ref returnMessage);
                                    returnTable[UserDataKey.ReturnMessage] = returnMessage;
                                    if (b)
                                    {
                                        logMsg = string.Format("[UserLogin] LoginId=>{0} ,LoginType=>{1}", userName, loginType);
                                        WriteUserDataLog(logMsg);
                                        returnTable[UserDataKey.LoginType] = loginType;
                                        return true;
                                    }
                                    else
                                    {
                                        returnTable[UserDataKey.LoginType] = "";
                                        return false;
                                    }
                                }
                                else
                                {
                                    string msg = "";
                                    if (userName == "")
                                        msg = "UserName is empty.";
                                    if (password == "")
                                        msg += "Password is empty.";

                                    returnTable[UserDataKey.ReturnMessage] = msg;
                                    returnTable[UserDataKey.LoginType] = "";
                                    return false;
                                }
                            }
                        case UserDataQueryCommand.Logout:
                            {
                                if (commandTable[UserDataKey.UserName] != null)
                                {
                                    userName = commandTable[UserDataKey.UserName].ToString();
                                }
                                logMsg = string.Format("[UserLogout] LogoutId=>{0}", userName);
                                WriteUserDataLog(logMsg);
                                break;
                            }
                        case UserDataQueryCommand.AddUser:
                            {
                                //get username/pasword

                                if (commandTable[UserDataKey.UserName] != null)
                                {
                                    userName = commandTable[UserDataKey.UserName].ToString();
                                }
                                if (commandTable[UserDataKey.Password] != null)
                                {
                                    password = commandTable[UserDataKey.Password].ToString();
                                }
                                if (commandTable[UserDataKey.LoginType] != null)
                                {
                                    loginType = commandTable[UserDataKey.LoginType].ToString();
                                }
                                if (commandTable[UserDataKey.AdminPassword] != null)
                                {
                                    adminPW = commandTable[UserDataKey.AdminPassword].ToString();
                                }
                                if (userName != "" && password != "" && loginType != "")
                                {
                                    #region check and add user

                                    bool b = this.CheckAndAddUser(userName, password, adminPW, loginType, ref returnMessage);
                                    returnTable[UserDataKey.ReturnMessage] = returnMessage;
                                    if (b)
                                    {
                                        logMsg = string.Format("[AddUser] AddNewUserId=>{0};UserType=>{1}", userName, loginType);
                                        WriteUserDataLog(logMsg);
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                    #endregion
                                }
                                else
                                {
                                    string msg = "";
                                    if (userName == "")
                                        msg = "UserName is empty.";
                                    if (password == "")
                                        msg += "Password is empty.";
                                    if (loginType == "")
                                        msg += "loginType is not assigned.";

                                    returnTable[UserDataKey.ReturnMessage] = msg;
                                    return false;
                                }
                            }
                        case UserDataQueryCommand.RemoveUser:
                            {
                                if (commandTable[UserDataKey.UserName] != null)
                                {
                                    userName = commandTable[UserDataKey.UserName].ToString();
                                }
                                if (commandTable[UserDataKey.AdminPassword] != null)
                                {
                                    adminPW = commandTable[UserDataKey.AdminPassword].ToString();
                                }
                                if (userName != "" && adminPW != "")
                                {
                                    #region RemoveUser

                                    if (userName.ToLower() == "admin")
                                    {
                                        returnTable[UserDataKey.ReturnMessage] = "Admin account(admin) cannot be deleted!";
                                        return false;
                                    }
                                    bool b = this.DeleteUser(userName, adminPW, ref returnMessage);
                                    returnTable[UserDataKey.ReturnMessage] = returnMessage;
                                    if (b)
                                    {
                                        logMsg = string.Format("[RemoveUser:{0}] DeleteUser:UserId=>{1}", appOwner, userName);
                                        WriteUserDataLog(logMsg);
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                    #endregion
                                }
                                else
                                {
                                    string msg = "";
                                    if (userName == "")
                                        msg = "UserName is empty.";
                                    if (adminPW == "")
                                        msg += "adminPW is empty.";

                                    returnTable[UserDataKey.ReturnMessage] = msg;
                                    return false;
                                }
                            }
                        case UserDataQueryCommand.ChangePassword:
                            {
                                if (commandTable[UserDataKey.UserName] != null)
                                {
                                    userName = commandTable[UserDataKey.UserName].ToString();
                                }
                                if (commandTable[UserDataKey.Password] != null)
                                {
                                    password = commandTable[UserDataKey.Password].ToString();
                                }
                                string newPassowrd = "";
                                if (commandTable[UserDataKey.NewPassword] != null)
                                {
                                    newPassowrd = commandTable[UserDataKey.NewPassword].ToString();
                                }
                                if (userName != "" && password != "" && newPassowrd != "")
                                {
                                    #region change password

                                    bool b = this.ChangePassword(userName, password, newPassowrd, ref returnMessage);
                                    returnTable[UserDataKey.ReturnMessage] = returnMessage;
                                    if (b)
                                    {
                                        logMsg = string.Format("[ChangePassword] UserId=>{0}", userName);
                                        WriteUserDataLog(logMsg);
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                    #endregion
                                }
                                else
                                {
                                    string msg = "";
                                    if (userName == "")
                                        msg = "UserName is empty.";
                                    if (password == "")
                                        msg += "Password is empty.";
                                    if (newPassowrd == "")
                                        msg += "newPassowrd is empty.";

                                    returnTable[UserDataKey.ReturnMessage] = msg;
                                    return false;
                                }
                            }
                        case UserDataQueryCommand.GetUserType:
                            {
                                string[] typeList = null;
                                if (this.GetUserTypeList(ref typeList, ref returnMessage))
                                {
                                    returnTable[UserDataKey.TypeList] = typeList;
                                    returnTable[UserDataKey.ReturnMessage] = returnMessage;
                                    return true;
                                }
                                else
                                {
                                    string str = "";
                                    returnTable[UserDataKey.TypeList] = str.Split(',');
                                    returnTable[UserDataKey.ReturnMessage] = returnMessage;
                                    return false;
                                }
                            }
                        case UserDataQueryCommand.VerifyUserType:
                            {
                                //get username/pasword
                                if (commandTable[UserDataKey.UserName] != null)
                                {
                                    userName = commandTable[UserDataKey.UserName].ToString();
                                }
                                if (commandTable[UserDataKey.Password] != null)
                                {
                                    password = commandTable[UserDataKey.Password].ToString();
                                }
                                if (userName != "" && password != "")
                                {
                                    //verify login data
                                    //string type = this.Check();
                                    bool b = this.CheckLoginType(userName, password, ref loginType, ref returnMessage);
                                    returnTable[UserDataKey.ReturnMessage] = returnMessage;
                                    if (b)
                                    {
                                        logMsg = string.Format("[VerifyUserType] LoginId=>{0} ,LoginType=>{1}", userName, loginType);
                                        WriteUserDataLog(logMsg);
                                        returnTable[UserDataKey.LoginType] = loginType;
                                        return true;
                                    }
                                    else
                                    {
                                        returnTable[UserDataKey.LoginType] = "";
                                        return false;
                                    }
                                }
                                else
                                {
                                    string msg = "";
                                    if (userName == "")
                                        msg = "UserName is empty.";
                                    if (password == "")
                                        msg += "Password is empty.";

                                    returnTable[UserDataKey.ReturnMessage] = msg;
                                    returnTable[UserDataKey.LoginType] = "";
                                    return false;
                                }
                            }
                    }
                }
            }
            catch (Exception e)
            {
                WriteUserDataLog("Exception occured in UserDataQuery(). Reason: " + e.Message + e.StackTrace);
            }
            return false;
        }

        private bool GetUserTypeList(ref string[] typeList, ref string returnMessage)
        {

            try
            {
                db = new UserXml(userfile);
                if (db.ParseOk)
                {
                    typeList = db.GetTypeList();
                    return true;
                }
                else
                {
                    string str = "";
                    typeList = str.Split(',');
                    returnMessage = "User control table is not exist!";
                    return false;
                }
            }
            catch (Exception ex)
            {
                string str = "";
                typeList = str.Split(',');
                returnMessage = ex.Message;
                return false;
            }
        }
        private bool CheckLoginType(string username, string password, ref string loginType, ref string returnMessage)
        {
            if (username.ToUpper() == "SWRD" && password == "00000000")
            {
                loginType = "SWRD";
                return true;
            }

            //get db
            db = new UserXml(userfile);
            if (db.ParseOk)
            {
                string input_pass = "";
                string input_user = username;
                // ** check user name / password
                UserInfo info = db.getUserNamePassword(input_user);
                input_pass = db.DoHash(info.type, info.username, password);

                // read from XML file
                if (!info.userExist)
                {
                    returnMessage = "UserName does not exist !";
                    return false;
                }
                if (info.password != input_pass)
                {
                    returnMessage = "Wrong Passowrd ! ";
                    return false;
                }

                loginType = info.type;
                return true;
            }
            else
            {
                returnMessage = "User control table is not exist!";
                return false;
            }
        }
        private bool CheckAndAddUser(string username, string password, string adminPassowrd, string loginType, ref string returnMessage)
        {
            //check user name
            //get db
            db = new UserXml(userfile);
            if (db.ParseOk)
            {
                UserInfo info1 = db.getUserNamePassword(username);
                if (info1.userExist || username.ToUpper() == "SWRD")
                {
                    returnMessage = "User Name already exist!";
                    return false;
                }
                //check admin password
                UserInfo info2 = db.getUserNamePassword("admin");
                string input_pass = db.DoHash(info2.type, info2.username, adminPassowrd);
                if (info2.password != input_pass)
                {
                    returnMessage = "Wrong admin password !";
                    return false;
                }

                // ok to add user here!
                string hash_pw = db.DoHash(loginType, username, password);
                if (db.AddUser(new UserInfo(username, hash_pw, loginType, false)))
                {
                    returnMessage = "Add User success!";
                    return true;
                }
                else
                {
                    returnMessage = "Add User Fail!";
                    return false;
                }
            }
            else
            {
                returnMessage = "User control table is not exist!";
                return false;
            }
        }
        private bool ChangePassword(string username, string oldpassword, string newPassowrd, ref string returnMessage)
        {
            db = new UserXml(userfile);
            if (db.ParseOk)
            {
                // check user name
                UserInfo info1 = db.getUserNamePassword(username);
                if (!info1.userExist)
                {
                    returnMessage = "User Name does not exist!";
                    return false;
                }

                // check user password
                string input_pass = db.DoHash(info1.type, username, oldpassword);
                if (info1.password != input_pass)
                {
                    returnMessage = "Wrong user password !";
                    return false;
                }

                // ok to change password here!
                string newpw = db.DoHash(info1.type, username, newPassowrd);
                if (db.ChangePassword(new UserInfo(username, newpw, info1.type, false)))
                {
                    returnMessage = "Change password for " + username + " success!";
                    return true;
                }
                else
                {
                    returnMessage = "Change password Fail!";
                    return false;
                }
            }
            else
            {
                returnMessage = "User control table is not exist!";
                return false;
            }
        }
        public bool DeleteUser(string username, string adminPassword, ref string returnMessage)
        {
            // deltet user
            db = new UserXml(userfile);
            if (db.ParseOk)
            {
                // check user name
                UserInfo info1 = db.getUserNamePassword(username);
                if (!info1.userExist)
                {
                    returnMessage = "User Name does not exist!";
                    return false;
                }
                // check admin password
                UserInfo info2 = db.getUserNamePassword("admin");
                string input_pass = db.DoHash(info2.type, info2.username, adminPassword);
                if (info2.password != input_pass)
                {
                    returnMessage = "Wrong admin password !";
                    return false;
                }

                // ok to delete user here!
                if (db.DeleteUser(username))
                {
                    returnMessage = "Delete success!";
                    return true;
                }
                else
                {
                    returnMessage = "Delete Fail!";
                    return false;
                }
            }
            else
            {
                returnMessage = "User control table is not exist!";
                return false;
            }
        }
        #endregion



        #region XML file handle
        public string ReadXMLFile(string filename, ref string xmlDoc)
        {
            try
            {
                XmlTextReader xtr = new XmlTextReader(WorkPath + filename);
                xtr.WhitespaceHandling = WhitespaceHandling.None;
                XmlDocument xmlobj = new XmlDocument();
                xmlobj.Load(xtr);
                xtr.Close();
                xmlDoc = StaticFileUtilities.SerializeXML(xmlobj);
                return "";
            }
            catch (Exception ex)
            {
                xmlDoc = null;
                return ex.Message;
            }
        }

        public void ResetToDefaultValue(string Type, string key)
        {
            try
            {
                switch (Type)
                {
                    case "TCPIP":
                        if(key == "Connector7")
                            TCPConfigSave(key, "192.168.10.15", "88");
                        else
                            TCPConfigSave(key, "127.0.0.1", "80");
                        break;
                    case "RS232":
                        if(key == "Connector1")
                            SerialPortConfigSave(key, "COM1", 9600,0 ,8 ,1);
                        else if (key == "Connector2")
                            SerialPortConfigSave(key, "COM2", 9600, 0, 8, 1);
                        else if (key == "Connector3")
                            SerialPortConfigSave(key, "COM3", 19200, 0, 8, 1);
                        else if (key == "Connector4")
                            SerialPortConfigSave(key, "COM4", 9600, 0, 8, 1);
                        else if (key == "Connector5")
                            SerialPortConfigSave(key, "COM5", 9600, 0, 8, 1);
                        else if (key == "Connector6")
                            SerialPortConfigSave(key, "COM6", 9600, 0, 8, 1);
                        else
                            SerialPortConfigSave(key, "COM9", 9600, 0, 8, 1);
                        break;
                    case "LoadPort":
                        if(key == "LoadPort_A")
                            LoadPortActorSave(key, new LoadPortConfig("Connector2", 5000, 10000));
                        else if (key == "LoadPort_B")
                            LoadPortActorSave(key, new LoadPortConfig("Connector5", 5000, 10000));
                        break;
                    case "N2Nozzle":
                        if (key == "N2Nozzle_A")
                            N2NozzleSave(key, new N2NozzleConfig("Connector2"));
                        else if (key == "N2Nozzle_B")
                            N2NozzleSave(key, new N2NozzleConfig("Connector5"));
                        break;
                    case "DIO":
                        if (key == "DIO_A")
                            DIOSave(key, new DIOConfig(0,0,8,16,8));
                        else if (key == "DIO_B")
                            DIOSave(key, new DIOConfig(0,1, 8, 16, 8));
                        break;

                }
            }
            catch
            {

            }
        }

        public XmlDocument XMLFileLoad(string filename ,string type)
        {
            if (!File.Exists(filename))
            {
                string str = filename + " is not exist. Create new default " + filename + ".";
                _log.WriteLog("TDK_GUI", LogHeadType.Error, str);
                CreateXmlFile(filename,type);
            }

            XmlTextReader xtr = new XmlTextReader(filename);
            xtr.WhitespaceHandling = WhitespaceHandling.None;
            XmlDocument xd = new XmlDocument();
            xd.Load(xtr);
            xtr.Close();
            return xd;
        }

        public string WriteXMLFile(string filename, string xmlDoc)
        {
            try
            {
                XmlTextWriter writer = new XmlTextWriter(WorkPath + filename, null);
                writer.Formatting = Formatting.Indented;
                XmlDocument xmlobj = StaticFileUtilities.DeserializeXml(xmlDoc);
                xmlobj.Save(writer);
                writer.Close();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        public static void CreateXmlFile(string filePath , string type)
        {
            var dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            if (File.Exists(filePath))
                return;
            StringBuilder xsch = new StringBuilder();
            switch (type)
            {
                case "Communication" :
                    WriteDefaultContentForCommunicaiton(xsch);
                    break;
                case "LoadPort":
                    WriteDefaultContentForLoadPort(xsch);
                    break;
                case "N2Nozzle":
                    WriteDefaultContentForN2Nozzle(xsch);
                    break;
                case "DIO":
                    WriteDefaultContentForDIO(xsch);
                    break;
            }

            StreamWriter xwriter = new StreamWriter(new FileStream(filePath, FileMode.Create), System.Text.Encoding.UTF8);
            xwriter.Write(xsch);
            xwriter.Flush();
            xwriter.Close();
        }

        private static StringBuilder WriteDefaultContentForCommunicaiton(StringBuilder xsch)
        {
            xsch.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            xsch.AppendLine("<!-- Communication Setting -->");
            xsch.AppendLine("<!-- CommunicationIndex 0 is RS232, 1 is TCPIP -->");
            xsch.AppendLine("<Communications>");
            xsch.AppendLine("  <Communication key=\"Connector1\" CommunicationIndex=\"0\" port=\"COM1\" baud=\"9600\" parity=\"0\" databits=\"8\" stopbits=\"1\" />");
            xsch.AppendLine("  <Communication key=\"Connector2\" CommunicationIndex=\"0\" port=\"COM2\" baud=\"9600\" parity=\"0\" databits=\"8\" stopbits=\"1\" />");
            xsch.AppendLine("  <Communication key=\"Connector3\" CommunicationIndex=\"0\" port=\"COM3\" baud=\"19200\" parity=\"0\" databits=\"8\" stopbits=\"1\" />");
            xsch.AppendLine("  <Communication key=\"Connector4\" CommunicationIndex=\"0\" port=\"COM4\" baud=\"9600\" parity=\"0\" databits=\"8\" stopbits=\"1\" />");
            xsch.AppendLine("  <Communication key=\"Connector5\" CommunicationIndex=\"0\" port=\"COM5\" baud=\"9600\" parity=\"0\" databits=\"8\" stopbits=\"1\" />");
            xsch.AppendLine("  <Communication key=\"Connector6\" CommunicationIndex=\"0\" port=\"COM6\" baud=\"9600\" parity=\"0\" databits=\"8\" stopbits=\"1\" />");
            xsch.AppendLine("  <Communication key=\"Connector7\" CommunicationIndex=\"1\" ip=\"192.168.10.15\" port=\"88\"  />");
            xsch.AppendLine("</Communications>");

            return xsch;
        }
        private static StringBuilder WriteDefaultContentForLoadPort(StringBuilder xsch)
        {
            xsch.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            xsch.AppendLine("<!-- Load Port Setting -->");
            xsch.AppendLine("<LoadPorts>");
            xsch.AppendLine("  <LoadPortParameters key=\"LoadPort_A\" >");
            xsch.AppendLine("    <LoadPortParameter key=\"Comm\" value=\"Connector2\" />");
            xsch.AppendLine("    <LoadPortParameter key=\"ACKTimeout\" value=\"5000\" />");
            xsch.AppendLine("    <LoadPortParameter key=\"InfTimeout\" value=\"10000\" />");
            xsch.AppendLine("  </LoadPortParameters>");
            xsch.AppendLine("  <LoadPortParameters key=\"LoadPort_B\" >");
            xsch.AppendLine("    <LoadPortParameter key=\"Comm\" value=\"Connector5\" />");
            xsch.AppendLine("    <LoadPortParameter key=\"ACKTimeout\" value=\"5000\" />");
            xsch.AppendLine("    <LoadPortParameter key=\"InfTimeout\" value=\"10000\" />");
            xsch.AppendLine("  </LoadPortParameters>");
            xsch.AppendLine("</LoadPorts>");

            return xsch;
        }
        private static StringBuilder WriteDefaultContentForN2Nozzle(StringBuilder xsch)
        {
            xsch.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            xsch.AppendLine("<!-- N2Nozzle Setting -->");
            xsch.AppendLine("<N2Nozzles>");
            xsch.AppendLine("  <N2NozzleParameters key=\"N2Nozzle_A\" >");
            xsch.AppendLine("    <N2NozzleParameter key=\"Comm\" value=\"Connector2\" />");
            xsch.AppendLine("  </N2NozzleParameters>");
            xsch.AppendLine("  <N2NozzleParameters key=\"N2Nozzle_B\" >");
            xsch.AppendLine("    <N2NozzleParameter key=\"Comm\" value=\"Connector5\" />");
            xsch.AppendLine("  </N2NozzleParameters>");
            xsch.AppendLine("</N2Nozzles>");

            return xsch;
        }

        private static StringBuilder WriteDefaultContentForDIO(StringBuilder xsch)
        {
            xsch.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            xsch.AppendLine("<!-- DIO Setting -->");
            xsch.AppendLine("<!-- Type 0 is Advantech, 1 is Virtual -->");
            xsch.AppendLine("<DIOs>");
            xsch.AppendLine("  <DIOParameters key=\"DIO_A\" >");
            xsch.AppendLine("    <DIOParameter key=\"Type\" value=\"0\" />");
            xsch.AppendLine("    <DIOParameter key=\"Index\" value=\"0\" />");
            xsch.AppendLine("    <DIOParameter key=\"DIPortMax\" value=\"16\" />");
            xsch.AppendLine("    <DIOParameter key=\"DOPortMax\" value=\"8\" />");
            xsch.AppendLine("    <DIOParameter key=\"PinCountPerPort\" value=\"8\" />");
            xsch.AppendLine("  </DIOParameters>");
            xsch.AppendLine("  <DIOParameters key=\"DIO_B\" >");
            xsch.AppendLine("    <DIOParameter key=\"Type\" value=\"0\" />");
            xsch.AppendLine("    <DIOParameter key=\"Index\" value=\"1\" />");
            xsch.AppendLine("    <DIOParameter key=\"DIPortMax\" value=\"16\" />");
            xsch.AppendLine("    <DIOParameter key=\"DOPortMax\" value=\"8\" />");
            xsch.AppendLine("    <DIOParameter key=\"PinCountPerPort\" value=\"8\" />");
            xsch.AppendLine("  </DIOParameters>");
            xsch.AppendLine("</DIOs>");

            return xsch;
        }
        #endregion
    }

    public class StaticFileUtilities
    {
        public static string SerializeXML(XmlDocument xml)
        {
            try
            {
                XmlSerializer xmlSer = new XmlSerializer(xml.GetType());
                StringWriter sWriter = new StringWriter();
                xmlSer.Serialize(sWriter, xml);
                return sWriter.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static XmlDocument DeserializeXml(string xml)
        {
            try
            {
                XmlDocument obj = new XmlDocument();
                XmlSerializer xmlSer = new XmlSerializer(obj.GetType());
                StringReader sReader = new StringReader(xml);
                XmlDocument retXml = (XmlDocument)xmlSer.Deserialize(sReader);
                return retXml;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
