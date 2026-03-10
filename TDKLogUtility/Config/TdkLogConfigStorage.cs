using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TDKLogUtility.Module_Config
{


    public sealed class TdkLogConfigStorage
    {
        public static void CreateXmlFile(string filePath)
        {
            var dir = Path.GetDirectoryName(filePath);
            if(!Directory.Exists(dir)) 
                Directory.CreateDirectory(dir);
            if (File.Exists(filePath))
                return;
            StringBuilder xsch = new StringBuilder();
            WriteDefaultContentByVer1(xsch);

            StreamWriter xwriter = new StreamWriter(new FileStream(filePath, FileMode.Create), System.Text.Encoding.UTF8);
            xwriter.Write(xsch);
            xwriter.Flush();
            xwriter.Close();
        }

        private static StringBuilder WriteDefaultContentByVer1(StringBuilder xsch)
        {
            xsch.AppendLine("<TDKLogConfig HmiVer=\"1.0\">");
            xsch.AppendLine("  <Config Name=\"MainDirectory\" Value=\"D:\\TDKLog\\\" />");
            xsch.AppendLine("  <Config Name=\"BufferSize\" Value=\"8\" />");
            xsch.AppendLine("  <Config Name=\"LogDeleteBufferDays\" Value=\"120\" />");
            xsch.AppendLine("  <Config Name=\"AutoFlushPeriod\" Value=\"10\" />");
            xsch.AppendLine("  <Config Name=\"MaxLogSize\" Value=\"30\" />");
            xsch.AppendLine("  <LogProcessName Name=\"TDK\">");
            xsch.AppendLine("    <LogName Process=\"TDK\" Remark=\"N/A\" />");
            xsch.AppendLine("    <LogName Process=\"Communication\" Remark=\"N/A\" />");
            xsch.AppendLine("  </LogProcessName>");
            xsch.AppendLine("</TDKLogConfig>");

            return xsch;
        }
        /// <summary>
        /// Read data from xml file.
        /// </summary>
        public static TdkLogConfig Load(string xmlPath)
        {
            if (!File.Exists(xmlPath))
                throw new FileNotFoundException("config file not found.", xmlPath);

            var doc = XDocument.Load(xmlPath);
            var root = doc.Root ?? throw new InvalidDataException("XML lost root");
            bool modified = false;

            string hmiVer = EnsureAttributeValue(root, "HmiVer", string.Empty, ref modified);
            string mainDir = EnsureConfigValue(root, "MainDirectory", @"D:\TDKLog\", ref modified);
            string bufferSizeStr = EnsureConfigValue(root, "BufferSize", "8", ref modified);
            string logDeleteDaysStr = EnsureConfigValue(root, "LogDeleteBufferDays", "120", ref modified);
            string autoFlushPeriodStr = EnsureConfigValue(root, "AutoFlushPeriod", "10", ref modified);
            string levelControlStr = EnsureConfigValue(root, "LevelControl", "5", ref modified);
            string maxLogSizeStr = EnsureConfigValue(root, "MaxLogSize", "30", ref modified);

            var logProcessEl = root.Element("LogProcessName");
            if (logProcessEl == null)
            {
                logProcessEl = new XElement("LogProcessName");
                root.Add(logProcessEl);
                modified = true;
            }
            string logProcGroupName = EnsureAttributeValue(logProcessEl, "Name", "TDK", ref modified);


            var cfg = new TdkLogConfig
            {
                HmiVer = hmiVer,
                MainDirectory = mainDir,
                BufferSize = ParseInt(bufferSizeStr, 8192),
                LogDeleteBufferDays = ParseInt(logDeleteDaysStr, 30),
                AutoFlushPeriod = ParseInt(autoFlushPeriodStr, 10),
                LevelControl = ParseInt(levelControlStr, 5),
                MaxLogSize = ParseInt(maxLogSizeStr, 30),
                LogProcessGroupName = logProcGroupName
            };

            foreach(var ln in logProcessEl.Elements("LogName"))
            {
                cfg.LogNames.Add(new LogNameEntry
                {
                    Process = (string)ln.Attribute("Process") ?? string.Empty,
                    Remark = (string)ln.Attribute("Remark") ?? string.Empty
                });
            }
            Validate(cfg);

            if (modified)
                doc.Save(xmlPath);

            return cfg;
        }

        /// <summary>
        /// Write TdkLogConfig back to xml file
        /// </summary>
        public static void Save(string xmlPath, TdkLogConfig cfg)
        {
            if (!File.Exists(xmlPath))
                throw new FileNotFoundException("config file not found.", xmlPath);

            var doc = XDocument.Load(xmlPath);
            var root = doc.Root ?? throw new InvalidDataException("XML lost root");

            root.SetAttributeValue("HmiVer", cfg.HmiVer);

            foreach (var node in root.Elements("Config"))
            {
                var name = (string)node.Attribute("Name");
                if (string.IsNullOrEmpty(name)) continue;

                switch (name)
                {
                    case "MainDirectory": node.SetAttributeValue("Value", cfg.MainDirectory); break;
                    case "BufferSize": node.SetAttributeValue("Value", cfg.BufferSize); break;
                    case "LogDeleteBufferDays": node.SetAttributeValue("Value", cfg.LogDeleteBufferDays); break;
                    case "AutoFlushPeriod": node.SetAttributeValue("Value", cfg.AutoFlushPeriod); break;
                    case "LevelControl": node.SetAttributeValue("Value", cfg.LevelControl); break;
                    case "MaxLogSize": node.SetAttributeValue("Value", cfg.MaxLogSize); break;
                }
            }

            var logProc = root.Element("LogProcessName");
            if (logProc != null && (string)logProc.Attribute("Name") == cfg.LogProcessGroupName)
            {
                foreach (var ln in logProc.Elements("LogName"))
                {
                    var process = (string)ln.Attribute("Process");
                    var match = cfg.LogNames.FirstOrDefault(x => x.Process == process);
                    if (match != null)
                    {
                        ln.SetAttributeValue("Remark", match.Remark);
                    }
                }
            }

            doc.Save(xmlPath);
        }
        /// <summary>
        /// Ensure element had value, it will add default value when element is null.
        /// </summary>
        /// <param name="parent">xml root</param>
        /// <param name="elementName">element name</param>
        /// <param name="defaultValue">default setting value</param>
        /// <param name="modified">attribute is been modified or not</param>
        /// <returns>element value</returns>

        private static string EnsureConfigValue(XElement root, string name, string defaultValue, ref bool modified)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name is required", nameof(name));

            var cfgEl = FindConfigByName(root, name);

            if (cfgEl == null)
            {
                cfgEl = new XElement("Config", new XAttribute("Name", name), new XAttribute("Value", defaultValue));
                root.AddFirst(cfgEl);
                modified = true;
                return defaultValue;
            }

            var valAttr = cfgEl.Attribute("Value");
            if (valAttr == null || string.IsNullOrWhiteSpace(valAttr.Value))
            {
                cfgEl.SetAttributeValue("Value", defaultValue);
                modified = true;
                return defaultValue;
            }

            return valAttr.Value;
        }


        private static XElement FindConfigByName(XElement root, string name)
        {
            foreach (var el in root.Elements("Config"))
            {
                var n = (string)el.Attribute("Name");
                if (string.Equals(n, name, StringComparison.OrdinalIgnoreCase))
                    return el;
            }
            return null;
        }

        /// <summary>
        /// Ensure attribute had value, it will add default value when attribute is null.
        /// </summary>
        private static string EnsureAttributeValue(XElement element, string attrName, string defaultValue, ref bool modified)
        {
            var attr = element.Attribute(attrName);
            if (attr == null)
            {
                element.Add(new XAttribute(attrName, defaultValue));
                modified = true;
                return defaultValue;
            }

            if (string.IsNullOrWhiteSpace(attr.Value))
            {
                attr.Value = defaultValue;
                modified = true;
                return defaultValue;
            }

            return attr.Value;
        }

        private static int ParseInt(string s, int defaultValue) =>
            int.TryParse(s, out var v) ? v : defaultValue;

        private static void Validate(TdkLogConfig cfg)
        {
            if (cfg.BufferSize <= 0)
                cfg.BufferSize = 8192;

            if (cfg.LogDeleteBufferDays < 0)
                cfg.LogDeleteBufferDays = 0;

            if (cfg.AutoFlushPeriod < 0)
                cfg.AutoFlushPeriod = 0;

            if (string.IsNullOrWhiteSpace(cfg.MainDirectory))
                throw new InvalidDataException("MainDirectory is missing or null");
        }

    }

}
