namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Security;
    using System.Xml;

    [OutputType(new Type[] { typeof(SelectXmlInfo) }), Cmdlet("Select", "Xml", DefaultParameterSetName="Xml", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135255")]
    public class SelectXmlCommand : PSCmdlet
    {
        private Hashtable _namespace;
        private string[] content;
        private bool isLiteralPath;
        private string[] path;
        private XmlNode[] xml;
        private string xpath;

        private XmlNamespaceManager AddNameSpaceTable(string parametersetname, XmlDocument xDoc, Hashtable namespacetable)
        {
            XmlNamespaceManager manager;
            if (parametersetname.Equals("Xml"))
            {
                XmlNameTable nameTable = new NameTable();
                manager = new XmlNamespaceManager(nameTable);
            }
            else
            {
                manager = new XmlNamespaceManager(xDoc.NameTable);
            }
            foreach (DictionaryEntry entry in namespacetable)
            {
                try
                {
                    string prefix = entry.Key.ToString();
                    manager.AddNamespace(prefix, entry.Value.ToString());
                }
                catch (NullReferenceException)
                {
                    InvalidOperationException exception = new InvalidOperationException(StringUtil.Format(UtilityCommonStrings.SearchXMLPrefixNullError, new object[0]));
                    ErrorRecord errorRecord = new ErrorRecord(exception, "PrefixError", ErrorCategory.InvalidOperation, namespacetable);
                    base.WriteError(errorRecord);
                }
                catch (ArgumentNullException)
                {
                    InvalidOperationException exception2 = new InvalidOperationException(StringUtil.Format(UtilityCommonStrings.SearchXMLPrefixNullError, new object[0]));
                    ErrorRecord record2 = new ErrorRecord(exception2, "PrefixError", ErrorCategory.InvalidOperation, namespacetable);
                    base.WriteError(record2);
                }
            }
            return manager;
        }

        protected override void ProcessRecord()
        {
            if (base.ParameterSetName.Equals("Xml", StringComparison.OrdinalIgnoreCase))
            {
                foreach (XmlNode node in this.Xml)
                {
                    this.ProcessXmlNode(node, string.Empty);
                }
            }
            else if (base.ParameterSetName.Equals("Path", StringComparison.OrdinalIgnoreCase) || base.ParameterSetName.Equals("LiteralPath", StringComparison.OrdinalIgnoreCase))
            {
                List<string> list = new List<string>();
                foreach (string str in this.path)
                {
                    if (this.isLiteralPath)
                    {
                        string unresolvedProviderPathFromPSPath = base.GetUnresolvedProviderPathFromPSPath(str);
                        list.Add(unresolvedProviderPathFromPSPath);
                    }
                    else
                    {
                        ProviderInfo info;
                        Collection<string> resolvedProviderPathFromPSPath = base.GetResolvedProviderPathFromPSPath(str, out info);
                        if (!info.NameEquals(base.Context.ProviderNames.FileSystem))
                        {
                            InvalidOperationException exception = new InvalidOperationException(StringUtil.Format(UtilityCommonStrings.FileOpenError, info.FullName));
                            ErrorRecord errorRecord = new ErrorRecord(exception, "ProcessingFile", ErrorCategory.InvalidOperation, str);
                            base.WriteError(errorRecord);
                        }
                        else
                        {
                            list.AddRange(resolvedProviderPathFromPSPath);
                        }
                    }
                }
                foreach (string str4 in list)
                {
                    this.ProcessXmlFile(str4);
                }
            }
            else if (base.ParameterSetName.Equals("Content", StringComparison.OrdinalIgnoreCase))
            {
                foreach (string str5 in this.content)
                {
                    XmlDocument document;
                    try
                    {
                        document = (XmlDocument) LanguagePrimitives.ConvertTo(str5, typeof(XmlDocument), CultureInfo.InvariantCulture);
                    }
                    catch (PSInvalidCastException exception2)
                    {
                        base.WriteError(exception2.ErrorRecord);
                        continue;
                    }
                    this.ProcessXmlNode(document, string.Empty);
                }
            }
        }

        private void ProcessXmlFile(string filePath)
        {
            try
            {
                XmlDocument xmlNode = InternalDeserializer.LoadUnsafeXmlDocument(new FileInfo(filePath), true, null);
                this.ProcessXmlNode(xmlNode, filePath);
            }
            catch (NotSupportedException exception)
            {
                this.WriteFileReadError(filePath, exception);
            }
            catch (IOException exception2)
            {
                this.WriteFileReadError(filePath, exception2);
            }
            catch (SecurityException exception3)
            {
                this.WriteFileReadError(filePath, exception3);
            }
            catch (UnauthorizedAccessException exception4)
            {
                this.WriteFileReadError(filePath, exception4);
            }
            catch (XmlException exception5)
            {
                this.WriteFileReadError(filePath, exception5);
            }
            catch (InvalidOperationException exception6)
            {
                this.WriteFileReadError(filePath, exception6);
            }
        }

        private void ProcessXmlNode(XmlNode xmlNode, string filePath)
        {
            XmlNodeList list;
            if (this._namespace != null)
            {
                XmlNamespaceManager nsmgr = this.AddNameSpaceTable(base.ParameterSetName, xmlNode as XmlDocument, this._namespace);
                list = xmlNode.SelectNodes(this.xpath, nsmgr);
            }
            else
            {
                list = xmlNode.SelectNodes(this.xpath);
            }
            this.WriteResults(list, filePath);
        }

        private void WriteFileReadError(string filePath, Exception exception)
        {
            ArgumentException exception2 = new ArgumentException(string.Format(CultureInfo.InvariantCulture, UtilityCommonStrings.FileReadError, new object[] { filePath, exception.Message }), exception);
            ErrorRecord errorRecord = new ErrorRecord(exception2, "ProcessingFile", ErrorCategory.InvalidArgument, filePath);
            base.WriteError(errorRecord);
        }

        private void WriteResults(XmlNodeList foundXmlNodes, string filePath)
        {
            foreach (XmlNode node in foundXmlNodes)
            {
                SelectXmlInfo sendToPipeline = new SelectXmlInfo {
                    Node = node,
                    Pattern = this.xpath,
                    Path = filePath
                };
                base.WriteObject(sendToPipeline);
            }
        }

        [ValidateNotNullOrEmpty, Parameter(Mandatory=true, ValueFromPipeline=true, ParameterSetName="Content")]
        public string[] Content
        {
            get
            {
                return this.content;
            }
            set
            {
                this.content = value;
            }
        }

        [Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="LiteralPath"), Alias(new string[] { "PSPath" }), ValidateNotNullOrEmpty]
        public string[] LiteralPath
        {
            get
            {
                return this.path;
            }
            set
            {
                this.path = value;
                this.isLiteralPath = true;
            }
        }

        [ValidateNotNullOrEmpty, Parameter]
        public Hashtable Namespace
        {
            get
            {
                return this._namespace;
            }
            set
            {
                this._namespace = value;
            }
        }

        [Parameter(Position=1, Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="Path"), ValidateNotNullOrEmpty]
        public string[] Path
        {
            get
            {
                return this.path;
            }
            set
            {
                this.path = value;
            }
        }

        [Parameter(Position=1, Mandatory=true, ValueFromPipelineByPropertyName=true, ValueFromPipeline=true, ParameterSetName="Xml"), ValidateNotNullOrEmpty, Alias(new string[] { "Node" })]
        public XmlNode[] Xml
        {
            get
            {
                return this.xml;
            }
            set
            {
                this.xml = value;
            }
        }

        [Parameter(Mandatory=true, Position=0), ValidateNotNullOrEmpty]
        public string XPath
        {
            get
            {
                return this.xpath;
            }
            set
            {
                this.xpath = value;
            }
        }
    }
}

