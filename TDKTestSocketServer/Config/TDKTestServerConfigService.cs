#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace TDKTestSocketServer.Config
{
    internal static class TDKTestServerConfigService
    {


        public static Task<TDKTestServerConfig> ReadFromFileAsync(string xmlPath,string loadServer, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(xmlPath))
                throw new ArgumentException(@"xmlPath can not be blank.", nameof(xmlPath));

            if (!File.Exists(xmlPath))
                throw new FileNotFoundException("XML File not found.", xmlPath);

            using var stream = File.OpenRead(xmlPath);

            return ReadFromStreamAsync(stream, loadServer, cancellationToken);
        }

        private static Task<TDKTestServerConfig> ReadFromStreamAsync(Stream stream, string loadServer, CancellationToken cancellationToken = default)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));
            if (!stream.CanRead) throw new ArgumentException(@"Stream can not read.", nameof(stream));

            return Task.FromResult(ReadFromStreamInternal(stream, loadServer, cancellationToken));
        }

        private static TDKTestServerConfig ReadFromStreamInternal(Stream stream, string loadServer, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var settings = new XmlReaderSettings
            {
                Async = false,
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                IgnoreWhitespace = true
            };

            using var reader = XmlReader.Create(stream, settings);

            var doc = XDocument.Load(reader, LoadOptions.None);
            return ParseDocument(doc, loadServer);
        }

        private static TDKTestServerConfig ParseDocument(XDocument doc, string loadServer)
        {
            var root = doc.Root ?? throw new InvalidDataException("XML loss root.");
            var rootConfig = root.Elements().FirstOrDefault(x => x.Name.LocalName == loadServer);
            if (!string.Equals(root.Name.LocalName, @"TDKServerConfig", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Main root should be <TDKServerConfig>，but actual were <{root.Name.LocalName}>.");
            if (!string.Equals(rootConfig.Name.LocalName, loadServer, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Config root should be <{loadServer}>，but actual were <{rootConfig.Name.LocalName}>.");

            var port = (string?)rootConfig.Attribute("Port") ?? throw new InvalidDataException("root loss Port property.");

            var commands = rootConfig
                           .Elements("Content")
                           .Select(e =>
                           {
                               var req = ((string?)e.Attribute("Request"))?.Trim();
                               var resp = ((string?)e.Attribute("Response"))?.Trim();

                               if (string.IsNullOrWhiteSpace(req))
                                   throw new InvalidDataException("Which <Content> loss Request property or blank.");
                               if (string.IsNullOrWhiteSpace(resp))
                                   throw new InvalidDataException("Which <Content> loss Response property or blank.");

                               return Tuple.Create(req!, resp!);
                           })
                           .ToList();

            return new TDKTestServerConfig
            {
                Port = port.Trim(),
                Command = commands
            };
        }

        /// <summary>
        /// Write TdkLogConfig back to xml file
        /// </summary>
        public static void SaveLoadport(string xmlPath, TDKTestServerConfig cfg)
        {
            if (!File.Exists(xmlPath))
                throw new FileNotFoundException("config file not found.", xmlPath);

            var doc = XDocument.Load(xmlPath);
            var root = doc.Root ?? throw new InvalidDataException("XML lost root");

            var loadport = root.Element("Loadport");
            
            if (loadport != null)
            {
                loadport.SetAttributeValue("Port", cfg.Port);

                foreach (var command in cfg.Command)
                {
                    var req = command.Item1;
                    var resp = command.Item2;
                    var respAttributes = loadport.Elements("Content")
                                                 .FirstOrDefault(x => (string?)x.Attribute("Request") == req);

                    if (respAttributes != null)
                    {
                        respAttributes.SetAttributeValue("Response", resp);
                    }
                    else
                    {
                        loadport.Add(new XElement("Content", 
                            new XAttribute("Request", req), 
                            new XAttribute("Response", resp)));
                    }
                }
            }

            doc.Save(xmlPath);
        }
    }
}
