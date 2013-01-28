namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Text;
    using System.Threading;
    using System.Xml;

    internal class PSConsoleFileElement
    {
        private static readonly PSTraceSource _mshsnapinTracer = PSTraceSource.GetTracer("MshSnapinLoadUnload", "Loading and unloading mshsnapins", false);
        private const string CSCHEMAVERSION = "ConsoleSchemaVersion";
        private const string CSCHEMAVERSIONNUMBER = "1.0";
        private readonly string monadVersion;
        private const string MSHCONSOLEFILE = "PSConsoleFile";
        private readonly Collection<string> mshsnapins;
        private const string PSVERSION = "PSVersion";
        private const string SNAPIN = "PSSnapIn";
        private const string SNAPINNAME = "Name";
        private const string SNAPINS = "PSSnapIns";

        private PSConsoleFileElement(string version)
        {
            this.monadVersion = version;
            this.mshsnapins = new Collection<string>();
        }

        internal static PSConsoleFileElement CreateFromFile(string path)
        {
            _mshsnapinTracer.WriteLine("Loading console info from file {0}.", new object[] { path });
            XmlDocument document = InternalDeserializer.LoadUnsafeXmlDocument(new FileInfo(path), false, null);
            if (document["PSConsoleFile"] == null)
            {
                _mshsnapinTracer.TraceError("Console file {0} doesn't contain tag {1}.", new object[] { path, "PSConsoleFile" });
                throw new XmlException(StringUtil.Format(ConsoleInfoErrorStrings.MonadConsoleNotFound, path));
            }
            if ((document["PSConsoleFile"]["PSVersion"] == null) || string.IsNullOrEmpty(document["PSConsoleFile"]["PSVersion"].InnerText))
            {
                _mshsnapinTracer.TraceError("Console file {0} doesn't contain tag {1}.", new object[] { path, "PSVersion" });
                throw new XmlException(StringUtil.Format(ConsoleInfoErrorStrings.MonadVersionNotFound, path));
            }
            XmlElement element = document["PSConsoleFile"];
            if (element.HasAttribute("ConsoleSchemaVersion"))
            {
                if (!element.GetAttribute("ConsoleSchemaVersion").Equals("1.0", StringComparison.OrdinalIgnoreCase))
                {
                    string format = StringUtil.Format(ConsoleInfoErrorStrings.BadConsoleVersion, path);
                    string errorMessageFormat = string.Format(Thread.CurrentThread.CurrentCulture, format, new object[] { "1.0" });
                    _mshsnapinTracer.TraceError(errorMessageFormat, new object[0]);
                    throw new XmlException(errorMessageFormat);
                }
            }
            else
            {
                _mshsnapinTracer.TraceError("Console file {0} doesn't contain tag schema version.", new object[] { path });
                throw new XmlException(StringUtil.Format(ConsoleInfoErrorStrings.BadConsoleVersion, path));
            }
            element = document["PSConsoleFile"]["PSVersion"];
            PSConsoleFileElement element2 = new PSConsoleFileElement(element.InnerText.Trim());
            bool flag = false;
            bool flag2 = false;
            for (System.Xml.XmlNode node = document["PSConsoleFile"].FirstChild; node != null; node = node.NextSibling)
            {
                if (node.NodeType != XmlNodeType.Comment)
                {
                    element = node as XmlElement;
                    if (element == null)
                    {
                        throw new XmlException(ConsoleInfoErrorStrings.BadXMLFormat);
                    }
                    if (element.Name == "PSVersion")
                    {
                        if (flag2)
                        {
                            _mshsnapinTracer.TraceError("Console file {0} contains more than one  msh versions", new object[] { path });
                            throw new XmlException(StringUtil.Format(ConsoleInfoErrorStrings.MultipleMshSnapinsElementNotSupported, "PSVersion"));
                        }
                        flag2 = true;
                    }
                    else
                    {
                        if (element.Name != "PSSnapIns")
                        {
                            _mshsnapinTracer.TraceError("Tag {0} is not supported in console file", new object[] { element.Name });
                            throw new XmlException(StringUtil.Format(ConsoleInfoErrorStrings.BadXMLElementFound, new object[] { element.Name, "PSConsoleFile", "PSVersion", "PSSnapIns" }));
                        }
                        if (flag)
                        {
                            _mshsnapinTracer.TraceError("Console file {0} contains more than one mshsnapin lists", new object[] { path });
                            throw new XmlException(StringUtil.Format(ConsoleInfoErrorStrings.MultipleMshSnapinsElementNotSupported, "PSSnapIns"));
                        }
                        flag = true;
                        for (System.Xml.XmlNode node2 = element.FirstChild; node2 != null; node2 = node2.NextSibling)
                        {
                            XmlElement element3 = node2 as XmlElement;
                            if ((element3 == null) || (element3.Name != "PSSnapIn"))
                            {
                                throw new XmlException(StringUtil.Format(ConsoleInfoErrorStrings.PSSnapInNotFound, node2.Name));
                            }
                            string attribute = element3.GetAttribute("Name");
                            if (string.IsNullOrEmpty(attribute))
                            {
                                throw new XmlException(ConsoleInfoErrorStrings.IDNotFound);
                            }
                            element2.mshsnapins.Add(attribute);
                            _mshsnapinTracer.WriteLine("Found in mshsnapin {0} in console file {1}", new object[] { attribute, path });
                        }
                    }
                }
            }
            return element2;
        }

        internal static void WriteToFile(string path, Version version, IEnumerable<PSSnapInInfo> snapins)
        {
            _mshsnapinTracer.WriteLine("Saving console info to file {0}.", new object[] { path });
            XmlWriterSettings settings = new XmlWriterSettings {
                Indent = true,
                Encoding = Encoding.UTF8
            };
            using (XmlWriter writer = XmlWriter.Create(path, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("PSConsoleFile");
                writer.WriteAttributeString("ConsoleSchemaVersion", "1.0");
                writer.WriteStartElement("PSVersion");
                writer.WriteString(version.ToString());
                writer.WriteEndElement();
                writer.WriteStartElement("PSSnapIns");
                foreach (PSSnapInInfo info in snapins)
                {
                    writer.WriteStartElement("PSSnapIn");
                    writer.WriteAttributeString("Name", info.Name);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Close();
            }
            _mshsnapinTracer.WriteLine("Saving console info succeeded.", new object[0]);
        }

        internal string MonadVersion
        {
            get
            {
                return this.monadVersion;
            }
        }

        internal Collection<string> PSSnapIns
        {
            get
            {
                return this.mshsnapins;
            }
        }
    }
}

