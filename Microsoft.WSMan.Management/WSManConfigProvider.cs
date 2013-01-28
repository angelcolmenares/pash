using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.WSMan.Management
{
    [CmdletProvider("WSMan", ProviderCapabilities.Credentials)]
    public sealed class WSManConfigProvider : NavigationCmdletProvider, ICmdletProviderSupportsHelp
    {
        // Fields
        private bool clearItemIsCalled;
        private static readonly string[] PKeyCertMapping = new string[] { "Issuer", "Subject", "Uri" };
        private static readonly string[] PKeyListener = new string[] { "Address", "Transport" };
        private static readonly string[] PKeyPlugin = new string[] { "Name" };
        private static readonly List<string> ppqWarningConfigurations = new List<string> { "idletimeoutms", "maxprocessespershell", "maxmemorypershellmb", "maxshellsperuser", "maxconcurrentusers" };
        private static readonly string[] WinRmRootConfigs = new string[] { "Client", "Service", "Shell", "Listener", "Plugin", "ClientCertificate" };
        private static readonly string[] WinrmRootName = new string[] { "winrm/Config" };
        private Dictionary<string, XmlDocument> enumarateMapping = new Dictionary<string, XmlDocument>();
        private Dictionary<string, string> getMapping = new Dictionary<string, string>();
        private static readonly List<string> globalWarningConfigurations = new List<string> { "maxconcurrentoperationsperuser", "idletimeout", "maxprocessespershell", "maxmemorypershellmb", "maxshellsperuser", "maxconcurrentusers" };
        private static readonly List<string> globalWarningUris = new List<string> { (WinrmRootName[0].ToString() + ((char) 0x2f) + "Winrs"), (WinrmRootName[0].ToString() + ((char) 0x2f) + "Service") };
        private WSManHelper helper = new WSManHelper();
        private PSObject objPluginNames;
        

        // Methods
        private void AssertError(string ErrorMessage, bool IsWSManError)
        {
            if (IsWSManError)
            {
                XmlDocument document = new XmlDocument();
                document.LoadXml(ErrorMessage);
                foreach (XmlNode node in document.GetElementsByTagName("f:Message"))
                {
                    InvalidOperationException exception = new InvalidOperationException(node.InnerText);
                    ErrorRecord errorRecord = new ErrorRecord(exception, "WsManError", ErrorCategory.InvalidOperation, null);
                    base.ThrowTerminatingError(errorRecord);
                }
            }
            else
            {
                InvalidOperationException exception2 = new InvalidOperationException(ErrorMessage);
                ErrorRecord record2 = new ErrorRecord(exception2, "WsManError", ErrorCategory.InvalidOperation, null);
                base.ThrowTerminatingError(record2);
            }
        }

        private PSObject BuildHostLevelPSObjectArrayList(object objSessionObject, string uri, bool IsWsManLevel)
        {
            PSObject obj2 = new PSObject();
            if (IsWsManLevel)
            {
                foreach (string str in WSManHelper.GetSessionObjCache().Keys)
                {
                    obj2.Properties.Add(new PSNoteProperty(str, "Container"));
                }
                return obj2;
            }
            if (objSessionObject != null)
            {
                foreach (XmlNode node in this.GetResourceValue(objSessionObject, uri, null).ChildNodes)
                {
                    foreach (XmlNode node2 in node.ChildNodes)
                    {
                        if ((node2.ChildNodes.Count == 0) || node2.FirstChild.Name.Equals("#text", StringComparison.OrdinalIgnoreCase))
                        {
                            obj2.Properties.Add(new PSNoteProperty(node2.LocalName, node2.InnerText));
                        }
                    }
                }
            }
            foreach (string str2 in WinRmRootConfigs)
            {
                obj2.Properties.Add(new PSNoteProperty(str2, "Container"));
            }
            return obj2;
        }

        private bool CheckPkeysArray(Hashtable values, string value, string[] pkeys)
        {
            bool flag = false;
            if (values != null)
            {
                foreach (string str in pkeys)
                {
                    if (values.Contains(str))
                    {
                        flag = true;
                    }
                }
                return flag;
            }
            if (!string.IsNullOrEmpty(value))
            {
                foreach (string str2 in pkeys)
                {
                    if (str2.Equals(value, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return flag;
        }

        private bool CheckValidContainerOrPath(string path)
        {
            if (path.Length == 0)
            {
                return true;
            }
            char ch = '\\';
            if (path.EndsWith(ch.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                path = path.Remove(path.LastIndexOf('\\'));
            }
            string name = string.Empty;
            string str2 = string.Empty;
            char ch2 = '\\';
            if (path.Contains(ch2.ToString()))
            {
                name = path.Substring(path.LastIndexOf('\\') + 1);
            }
            else
            {
                name = path;
            }
            string hostName = this.GetHostName(path);
            if (string.IsNullOrEmpty(hostName))
            {
                return false;
            }
            if (this.IsPathLocalMachine(hostName) && !this.IsWSManServiceRunning())
            {
                WSManHelper.ThrowIfNotAdministrator();
                this.StartWSManService((bool) base.Force);
            }
            string str4 = this.NormalizePath(path, hostName);
            if (string.IsNullOrEmpty(str4))
            {
                return false;
            }
            lock (WSManHelper.AutoSession)
            {
                object obj2 = null;
                WSManHelper.GetSessionObjCache().TryGetValue(hostName, out obj2);
                str2 = hostName + '\\';
                if (path.Equals(hostName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                if (path.StartsWith(str2 + "Plugin", StringComparison.OrdinalIgnoreCase))
                {
                    if (path.Equals(str2 + "Plugin", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    XmlDocument xmlPlugins = this.FindResourceValue(obj2, str4, null);
                    string currentPluginName = string.Empty;
                    this.GetPluginNames(xmlPlugins, out this.objPluginNames, out currentPluginName, path);
                    if (string.IsNullOrEmpty(currentPluginName))
                    {
                        return false;
                    }
                    string resourceURI = str4 + "?Name=" + currentPluginName;
                    XmlDocument xmldoc = this.GetResourceValue(obj2, resourceURI, null);
                    PSObject obj3 = this.ProcessPluginConfigurationLevel(xmldoc, false);
                    ArrayList arrSecurity = null;
                    ArrayList list2 = this.ProcessPluginResourceLevel(xmldoc, out arrSecurity);
                    ArrayList list3 = this.ProcessPluginInitParamLevel(xmldoc);
                    str2 = string.Concat(new object[] { str2, "Plugin", '\\', currentPluginName });
                    if (path.Equals(str2, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    if (path.StartsWith(str2 + '\\' + "Quotas", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    if (!path.StartsWith(str2 + '\\' + "Resources", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!path.StartsWith(str2 + '\\' + "InitializationParameters", StringComparison.OrdinalIgnoreCase))
                        {
                            if (obj3.Properties.Match(name).Count > 0)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            if (path.Equals(str2 + '\\' + "InitializationParameters", StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                            if (list3 != null)
                            {
                                foreach (PSObject obj6 in list3)
                                {
                                    if (obj6.Properties.Match(name).Count > 0)
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (path.Equals(str2 + '\\' + "Resources", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                        if (list2 != null)
                        {
                            str2 = str2 + '\\' + "Resources";
                            foreach (PSObject obj4 in list2)
                            {
                                string str7 = obj4.Properties["ResourceDir"].Value.ToString();
                                if (path.StartsWith(str2 + '\\' + str7, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (path.Equals(str2 + '\\' + str7, StringComparison.OrdinalIgnoreCase))
                                    {
                                        return true;
                                    }
                                    if (obj4.Properties.Match(name).Count > 0)
                                    {
                                        return true;
                                    }
                                    if (path.StartsWith(string.Concat(new object[] { str2, '\\', str7, '\\', "Security" }), StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (path.Equals(string.Concat(new object[] { str2, '\\', str7, '\\', "Security" }), StringComparison.OrdinalIgnoreCase))
                                        {
                                            return true;
                                        }
                                        str2 = string.Concat(new object[] { str2, '\\', str7, '\\', "Security", '\\' });
                                        if (arrSecurity == null)
                                        {
                                            return false;
                                        }
                                        foreach (PSObject obj5 in arrSecurity)
                                        {
                                            string str8 = obj5.Properties["SecurityDIR"].Value.ToString();
                                            if (path.Equals(str2 + str8, StringComparison.OrdinalIgnoreCase))
                                            {
                                                return true;
                                            }
                                            if (obj5.Properties.Match(name).Count > 0)
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (path.StartsWith(str2 + "Listener", StringComparison.OrdinalIgnoreCase))
                    {
                        return this.ItemExistListenerOrClientCertificate(obj2, str4, path, "Listener", hostName);
                    }
                    if (path.StartsWith(str2 + "ClientCertificate", StringComparison.OrdinalIgnoreCase))
                    {
                        return this.ItemExistListenerOrClientCertificate(obj2, str4, path, "ClientCertificate", hostName);
                    }
                    return this.ContainResourceValue(obj2, str4, name, path, hostName);
                }
                return false;
            }
        }

        protected override void ClearItem(string path)
        {
            this.clearItemIsCalled = true;
            this.SetItem(path, string.Empty);
            this.clearItemIsCalled = false;
        }

        private string ConstructCapabilityXml(object[] capabilities)
        {
            StringBuilder builder = new StringBuilder("");
            foreach (object obj2 in capabilities)
            {
                builder.Append("<Capability");
                builder.Append(' ');
                builder.Append("Type" + '=');
                builder.Append('"' + obj2.ToString() + '"');
                builder.Append('>');
                builder.Append("</Capability>");
            }
            return builder.ToString();
        }

        private string ConstructInitParamsXml(PSObject objinputparams, ArrayList initparams)
        {
            StringBuilder builder = new StringBuilder("");
            if ((objinputparams != null) || (initparams != null))
            {
                builder.Append("<InitializationParameters>");
                if (objinputparams != null)
                {
                    foreach (PSPropertyInfo info in objinputparams.Properties)
                    {
                        builder.Append("<Param");
                        builder.Append(' ');
                        builder.Append("Name");
                        builder.Append('=');
                        builder.Append('"' + info.Name + '"');
                        builder.Append(' ');
                        builder.Append("Value");
                        builder.Append('=');
                        builder.Append('"' + info.Value.ToString() + '"');
                        builder.Append("/>");
                    }
                }
                else
                {
                    foreach (PSObject obj2 in initparams)
                    {
                        foreach (PSPropertyInfo info2 in obj2.Properties)
                        {
                            builder.Append("<Param");
                            builder.Append(' ');
                            builder.Append("Name");
                            builder.Append('=');
                            builder.Append('"' + info2.Name + '"');
                            builder.Append(' ');
                            builder.Append("Value");
                            builder.Append('=');
                            builder.Append('"' + info2.Value.ToString() + '"');
                            builder.Append("/>");
                        }
                    }
                }
                builder.Append("</InitializationParameters>");
            }
            return builder.ToString();
        }

        private string ConstructPluginXml(PSObject objinputparam, string ResourceURI, string host, string Operation, ArrayList resources, ArrayList securities, ArrayList initParams)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("<PlugInConfiguration ");
            builder.Append("xmlns=");
            builder.Append('"' + "http://schemas.microsoft.com/wbem/wsman/1/config/PluginConfiguration" + '"');
            if (objinputparam != null)
            {
                foreach (PSPropertyInfo info in objinputparam.Properties)
                {
                    builder.Append(' ');
                    if (this.IsValueOfParamList(info.Name, WSManStringLiterals.NewItemPluginConfigParams))
                    {
                        builder.Append(info.Name);
                        builder.Append('=');
                        if ("RunAsPassword".Equals(info.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            info.Value = this.GetStringFromSecureString(info.Value);
                        }
                        builder.Append('"' + info.Value.ToString() + '"');
                    }
                }
            }
            builder.Append('>');
            if (Operation.Equals("New"))
            {
                if (objinputparam != null)
                {
                    builder.Append(this.ConstructResourceXml(objinputparam, null, null));
                }
                else
                {
                    builder.Append(this.ConstructResourceXml(null, resources, null));
                }
            }
            else if (Operation.Equals("Set"))
            {
                if (initParams != null)
                {
                    builder.Append(this.ConstructInitParamsXml(null, initParams));
                }
                if (resources != null)
                {
                    builder.Append(this.ConstructResourceXml(null, resources, securities));
                }
            }
            builder.Append("</PlugInConfiguration>");
            return builder.ToString();
        }

        private string ConstructResourceXml(PSObject objinputparams, ArrayList resources, ArrayList securities)
        {
            StringBuilder builder = new StringBuilder("");
            if ((objinputparams != null) || (resources != null))
            {
                object[] capabilities = null;
                builder.Append("<Resources>");
                if (objinputparams != null)
                {
                    builder.Append("<Resource");
                    foreach (PSPropertyInfo info in objinputparams.Properties)
                    {
                        builder.Append(' ');
                        if (this.IsValueOfParamList(info.Name, WSManStringLiterals.NewItemResourceParams))
                        {
                            if (info.Name.Equals("Resource") || info.Name.Equals("ResourceUri"))
                            {
                                builder.Append(string.Concat(new object[] { "ResourceUri", '=', '"', info.Value.ToString(), '"' }));
                            }
                            else if (info.Name.Equals("Capability"))
                            {
                                capabilities = (object[]) info.Value;
                            }
                            else
                            {
                                builder.Append(info.Name);
                                builder.Append('=');
                                builder.Append('"' + info.Value.ToString() + '"');
                            }
                        }
                    }
                    builder.Append('>');
                    if (securities != null)
                    {
                        builder.Append(this.ConstructSecurityXml(null, securities, string.Empty));
                    }
                    builder.Append(this.ConstructCapabilityXml(capabilities));
                    builder.Append("</Resource>");
                }
                else
                {
                    foreach (PSObject obj2 in resources)
                    {
                        builder.Append("<Resource");
                        foreach (PSPropertyInfo info2 in obj2.Properties)
                        {
                            builder.Append(' ');
                            if (this.IsValueOfParamList(info2.Name, WSManStringLiterals.NewItemResourceParams))
                            {
                                if (info2.Name.Equals("Resource") || info2.Name.Equals("ResourceUri"))
                                {
                                    builder.Append(string.Concat(new object[] { "ResourceUri", '=', '"', info2.Value.ToString(), '"' }));
                                }
                                else if (info2.Name.Equals("Capability"))
                                {
                                    if (info2.Value.GetType().FullName.Equals("System.String"))
                                    {
                                        capabilities = new object[] { info2.Value };
                                    }
                                    else
                                    {
                                        capabilities = (object[]) info2.Value;
                                    }
                                }
                                else
                                {
                                    builder.Append(info2.Name);
                                    builder.Append('=');
                                    builder.Append('"' + info2.Value.ToString() + '"');
                                }
                            }
                        }
                        builder.Append('>');
                        if (securities != null)
                        {
                            builder.Append(this.ConstructSecurityXml(null, securities, obj2.Properties["ResourceDir"].Value.ToString()));
                        }
                        builder.Append(this.ConstructCapabilityXml(capabilities));
                        builder.Append("</Resource>");
                    }
                }
                builder.Append("</Resources>");
            }
            return builder.ToString();
        }

        private string ConstructSecurityXml(PSObject objinputparams, ArrayList securities, string strResourceIdentity)
        {
            StringBuilder builder = new StringBuilder("");
            if ((objinputparams != null) || (securities != null))
            {
                if (objinputparams != null)
                {
                    builder.Append("<Security");
                    foreach (PSPropertyInfo info in objinputparams.Properties)
                    {
                        builder.Append(' ');
                        if (this.IsValueOfParamList(info.Name, WSManStringLiterals.NewItemSecurityParams))
                        {
                            builder.Append(info.Name);
                            builder.Append('=');
                            builder.Append('"' + info.Value.ToString() + '"');
                        }
                    }
                    builder.Append('>');
                    builder.Append("</Security>");
                }
                else
                {
                    foreach (PSObject obj2 in securities)
                    {
                        if (obj2.Properties["ResourceDir"].Value.ToString().Equals(strResourceIdentity))
                        {
                            builder.Append("<Security");
                            foreach (PSPropertyInfo info2 in obj2.Properties)
                            {
                                builder.Append(' ');
                                if (this.IsValueOfParamList(info2.Name, WSManStringLiterals.NewItemSecurityParams))
                                {
                                    builder.Append(info2.Name);
                                    builder.Append('=');
                                    builder.Append('"' + info2.Value.ToString() + '"');
                                }
                            }
                            builder.Append('>');
                            builder.Append("</Security>");
                        }
                    }
                }
            }
            return builder.ToString();
        }

        private bool ContainResourceValue(object sessionobj, string ResourceURI, string childname, string path, string host)
        {
            bool flag = false;
            string str = string.Empty;
            try
            {
                if ((ResourceURI.Contains("Listener") || ResourceURI.Contains("Plugin")) || ResourceURI.Contains("Service/certmapping"))
                {
                    object o = ((IWSManSession) sessionobj).Enumerate(ResourceURI, "", "", 0);
                    while (!((IWSManEnumerator) o).AtEndOfStream)
                    {
                        str = str + ((IWSManEnumerator) o).ReadItem();
                    }
                    if ((str != "") && !string.IsNullOrEmpty(str))
                    {
                        str = "<WsManResults>" + str + "</WsManResults>";
                    }
                    Marshal.ReleaseComObject(o);
                }
                else
                {
                    str = this.GetResourceValueInXml((IWSManSession) sessionobj, ResourceURI, null);
                }
                if (string.IsNullOrEmpty(str))
                {
                    return false;
                }
                XmlDocument resourcexmldocument = new XmlDocument();
                resourcexmldocument.LoadXml(str.ToLower(CultureInfo.InvariantCulture));
                if (this.SearchXml(resourcexmldocument, childname, ResourceURI, path, host).Count > 0)
                {
                    flag = true;
                }
            }
            catch (COMException)
            {
                flag = false;
            }
            return flag;
        }

        private PSObject ConvertToPSObject(XmlNode xmlnode)
        {
            PSObject obj2 = new PSObject();
            foreach (XmlNode node in xmlnode.ChildNodes)
            {
                if ((node.ChildNodes.Count == 0) || node.FirstChild.Name.Equals("#text", StringComparison.OrdinalIgnoreCase))
                {
                    XmlAttribute attribute = null;
                    foreach (XmlAttribute attribute2 in node.Attributes)
                    {
                        if (attribute2.LocalName.Equals("Source", StringComparison.OrdinalIgnoreCase))
                        {
                            attribute = attribute2;
                            break;
                        }
                    }
                    obj2.Properties.Add(new PSNoteProperty(node.LocalName, node.InnerText));
                    if (attribute != null)
                    {
                        string name = node.LocalName + "___Source";
                        obj2.Properties.Remove(name);
                        obj2.Properties.Add(new PSNoteProperty(name, attribute.Value));
                    }
                    continue;
                }
                obj2.Properties.Add(new PSNoteProperty(node.LocalName, "Container"));
            }
            return obj2;
        }

        private void CreateResourceValue(object sessionobj, string ResourceURI, string resource, Hashtable cmdlinevalues)
        {
            try
            {
                if ((ResourceURI.Contains("Listener") || ResourceURI.Contains("Plugin")) || ResourceURI.Contains("Service/certmapping"))
                {
                    ResourceURI = this.GetURIWithFilter(ResourceURI, cmdlinevalues);
                    ((IWSManSession) sessionobj).Create(ResourceURI, resource, 0);
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(((IWSManSession) sessionobj).Error))
                {
                    this.AssertError(((IWSManSession) sessionobj).Error, true);
                }
            }
        }

        private void DeleteResourceValue(object sessionobj, string ResourceURI, Hashtable cmdlinevalues, bool recurse)
        {
            try
            {
                if ((ResourceURI.Contains("Plugin") || ResourceURI.Contains("Listener")) || ResourceURI.Contains("Service/certmapping"))
                {
                    if (cmdlinevalues != null)
                    {
                        ResourceURI = this.GetURIWithFilter(ResourceURI, cmdlinevalues);
                    }
                    ((IWSManSession) sessionobj).Delete(ResourceURI, 0);
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(((IWSManSession) sessionobj).Error))
                {
                    this.AssertError(((IWSManSession) sessionobj).Error, true);
                }
            }
        }

        private XmlDocument EnumerateResourceValue(object sessionobj, string ResourceURI)
        {
            XmlDocument document = null;
            if (!this.enumarateMapping.TryGetValue(ResourceURI, out document))
            {
                try
                {
                    object o = ((IWSManSession) sessionobj).Enumerate(ResourceURI, "", "", 0);
                    string str = string.Empty;
                    while (!((IWSManEnumerator) o).AtEndOfStream)
                    {
                        str = str + ((IWSManEnumerator) o).ReadItem();
                    }
                    Marshal.ReleaseComObject(o);
                    if (!string.IsNullOrEmpty(str))
                    {
                        document = new XmlDocument();
                        str = "<WsManResults>" + str + "</WsManResults>";
                        document.LoadXml(str);
                        this.enumarateMapping.Add(ResourceURI, document);
                    }
                }
                finally
                {
                    if (!string.IsNullOrEmpty(((IWSManSession) sessionobj).Error))
                    {
                        this.AssertError(((IWSManSession) sessionobj).Error, true);
                    }
                }
            }
            return document;
        }

        private string EscapeValuesForXML(string value)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i <= (value.Length - 1); i++)
            {
                switch (value[i])
                {
                    case '&':
                    {
                        builder.Append("&amp;");
                        continue;
                    }
                    case '\'':
                    {
                        builder.Append("&apos;");
                        continue;
                    }
                    case '"':
                    {
                        builder.Append("&quot;");
                        continue;
                    }
                    case '<':
                    {
                        builder.Append("&lt;");
                        continue;
                    }
                    case '>':
                    {
                        builder.Append("&gt;");
                        continue;
                    }
                }
                builder.Append(value[i]);
            }
            return builder.ToString();
        }

        private XmlDocument FindResourceValue(object sessionobj, string ResourceURI, Hashtable cmdlinevalues)
        {
            if ((ResourceURI.Contains("Listener") || ResourceURI.Contains("Plugin")) || ResourceURI.Contains("Service/certmapping"))
            {
                if ((cmdlinevalues == null) || (cmdlinevalues.Count == 0))
                {
                    return this.EnumerateResourceValue(sessionobj, ResourceURI);
                }
                return this.GetResourceValue(sessionobj, ResourceURI, cmdlinevalues);
            }
            return this.GetResourceValue(sessionobj, ResourceURI, cmdlinevalues);
        }

        private void GenerateObjectNameAndKeys(Hashtable InputAttributes, string ResourceURI, string ContainerItem, out string ItemName, out string[] keys)
        {
            StringBuilder builder = new StringBuilder();
            string str = string.Empty;
            foreach (DictionaryEntry entry in InputAttributes)
            {
                if (this.IsPKey(entry.Key.ToString(), ResourceURI))
                {
                    builder.Append(entry.Key.ToString());
                    builder.Append('=');
                    builder.Append(entry.Value.ToString());
                    str = string.Concat(new object[] { str, entry.Key.ToString(), '=', entry.Value.ToString(), "|" });
                }
                else if (ContainerItem.Equals("Listener", StringComparison.OrdinalIgnoreCase) && entry.Key.ToString().Equals("Port", StringComparison.OrdinalIgnoreCase))
                {
                    builder.Append(entry.Key.ToString());
                    builder.Append('=');
                    builder.Append(entry.Value.ToString());
                }
            }
            str = str.Substring(0, str.LastIndexOf('|'));
            ItemName = ContainerItem + "_" + Math.Abs(builder.ToString().GetHashCode());
            keys = str.Split(new char[] { '|' });
        }

        private void GetChildItemOrNamesForListenerOrCertMapping(XmlDocument xmlResource, string ListenerOrCerMapping, string path, string host, ProviderMethods methodname, bool recurse)
        {
            Hashtable hashtable;
            Hashtable hashtable2;
            if (ListenerOrCerMapping.Equals("ClientCertificate"))
            {
                this.ProcessCertMappingObjects(xmlResource, out hashtable, out hashtable2);
            }
            else if (ListenerOrCerMapping.Equals("Listener"))
            {
                this.ProcessListernerObjects(xmlResource, out hashtable, out hashtable2);
            }
            else
            {
                return;
            }
            if ((hashtable != null) && (hashtable2 != null))
            {
                if (path.EndsWith(host + '\\' + ListenerOrCerMapping, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (string str in hashtable2.Keys)
                    {
                        switch (methodname)
                        {
                            case ProviderMethods.GetChildItems:
                            {
                                PSObject psobject = new PSObject();
                                psobject.Properties.Add(new PSNoteProperty(str, "Container"));
                                this.WritePSObjectPropertiesAsWSManElementObjects(psobject, path, (string[]) hashtable2[str], null, WsManElementObjectTypes.WSManConfigContainerElement, recurse);
                                break;
                            }
                            case ProviderMethods.GetChildNames:
                                base.WriteItemObject(str, path, true);
                                break;
                        }
                    }
                }
                else
                {
                    string str2 = path.Substring(path.LastIndexOf('\\') + 1);
                    if (methodname.Equals(ProviderMethods.GetChildItems))
                    {
                        this.WritePSObjectPropertiesAsWSManElementObjects((PSObject) hashtable[str2], path, null, null, WsManElementObjectTypes.WSManConfigLeafElement, recurse);
                    }
                    else if (methodname.Equals(ProviderMethods.GetChildNames))
                    {
                        foreach (PSPropertyInfo info in ((PSObject) hashtable[str2]).Properties)
                        {
                            base.WriteItemObject(info.Name, path + '\\' + info.Name, false);
                        }
                    }
                }
            }
        }

        protected override void GetChildItems(string path, bool recurse)
        {
            this.GetChildItemsOrNames(path, ProviderMethods.GetChildItems, recurse);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private void GetChildItemsOrNames(string path, ProviderMethods methodname, bool recurse)
        {
            Dictionary<string, object> sessionObjCache = WSManHelper.GetSessionObjCache();
            if (path.Length == 0)
            {
                switch (methodname)
                {
                    case ProviderMethods.GetChildItems:
                    {
                        PSObject psobject = this.BuildHostLevelPSObjectArrayList(null, "", true);
                        this.WritePSObjectPropertiesAsWSManElementObjects(psobject, "WSMan", null, "ComputerLevel", WsManElementObjectTypes.WSManConfigContainerElement, recurse);
                        return;
                    }
                    case ProviderMethods.GetChildNames:
                        foreach (string str in sessionObjCache.Keys)
                        {
                            base.WriteItemObject(str, "WSMan", true);
                        }
                        return;
                }
            }
            else
            {
                char ch = '\\';
                if (path.EndsWith(ch.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    path = path.Remove(path.LastIndexOf('\\'));
                }
                string hostName = this.GetHostName(path);
                if (string.IsNullOrEmpty(hostName))
                {
                    throw new InvalidOperationException("InvalidPath");
                }
                if (this.IsPathLocalMachine(hostName) && !this.IsWSManServiceRunning())
                {
                    if (methodname.Equals(ProviderMethods.GetChildItems))
                    {
                        WSManHelper.ThrowIfNotAdministrator();
                        this.StartWSManService((bool) base.Force);
                    }
                    else if (methodname.Equals(ProviderMethods.GetChildNames))
                    {
                        this.AssertError("WinRMServiceError", false);
                    }
                }
                lock (WSManHelper.AutoSession)
                {
                    object obj3;
                    XmlDocument document;
                    string str5;
                    string str6;
                    sessionObjCache.TryGetValue(hostName, out obj3);
                    string uri = this.NormalizePath(path, hostName);
                    string str4 = hostName + '\\';
                    if (path.EndsWith(hostName, StringComparison.OrdinalIgnoreCase))
                    {
                        PSObject obj4 = this.BuildHostLevelPSObjectArrayList(obj3, uri, false);
                        switch (methodname)
                        {
                            case ProviderMethods.GetChildItems:
                                this.WritePSObjectPropertiesAsWSManElementObjects(obj4, path, null, null, WsManElementObjectTypes.WSManConfigLeafElement, recurse);
                                break;

                            case ProviderMethods.GetChildNames:
                                this.WritePSObjectPropertyNames(obj4, path);
                                break;
                        }
                    }
                    else
                    {
                        document = this.FindResourceValue(obj3, uri, null);
                        if ((document != null) && document.HasChildNodes)
                        {
                            if (path.Contains(str4 + "Listener"))
                            {
                                this.GetChildItemOrNamesForListenerOrCertMapping(document, "Listener", path, hostName, methodname, recurse);
                            }
                            else if (path.Contains(str4 + "ClientCertificate"))
                            {
                                this.GetChildItemOrNamesForListenerOrCertMapping(document, "ClientCertificate", path, hostName, methodname, recurse);
                            }
                            else
                            {
                                if (!path.Contains(str4 + "Plugin"))
                                {
                                    goto Label_0A51;
                                }
                                str5 = string.Empty;
                                this.GetPluginNames(document, out this.objPluginNames, out str5, path);
                                if (!path.EndsWith(str4 + "Plugin", StringComparison.OrdinalIgnoreCase))
                                {
                                    goto Label_0302;
                                }
                                switch (methodname)
                                {
                                    case ProviderMethods.GetChildItems:
                                        foreach (PSPropertyInfo info in this.objPluginNames.Properties)
                                        {
                                            PSObject obj5 = new PSObject();
                                            obj5.Properties.Add(new PSNoteProperty(info.Name, info.Value));
                                            this.WritePSObjectPropertiesAsWSManElementObjects(obj5, path, new string[] { "Name=" + info.Name }, null, WsManElementObjectTypes.WSManConfigContainerElement, recurse);
                                        }
                                        break;

                                    case ProviderMethods.GetChildNames:
                                        goto Label_02F0;
                                }
                            }
                        }
                    }
                    goto Label_0B27;
                Label_02F0:
                    this.WritePSObjectPropertyNames(this.objPluginNames, path);
                    goto Label_0B27;
                Label_0302:
                    str6 = uri + "?Name=" + str5;
                    XmlDocument xmldoc = this.GetResourceValue(obj3, str6, null);
                    if (xmldoc != null)
                    {
                        PSObject obj6 = this.ProcessPluginConfigurationLevel(xmldoc, true);
                        ArrayList arrSecurity = null;
                        ArrayList list2 = this.ProcessPluginResourceLevel(xmldoc, out arrSecurity);
                        ArrayList list3 = this.ProcessPluginInitParamLevel(xmldoc);
                        str4 = str4 + "Plugin" + '\\';
                        if (path.EndsWith(str4 + str5, StringComparison.OrdinalIgnoreCase))
                        {
                            switch (methodname)
                            {
                                case ProviderMethods.GetChildItems:
                                    this.WritePSObjectPropertiesAsWSManElementObjects(obj6, path, null, null, WsManElementObjectTypes.WSManConfigLeafElement, recurse);
                                    break;

                                case ProviderMethods.GetChildNames:
                                    this.WritePSObjectPropertyNames(obj6, path);
                                    break;
                            }
                        }
                        else if (path.EndsWith("Quotas", StringComparison.OrdinalIgnoreCase))
                        {
                            XmlNodeList elementsByTagName = xmldoc.GetElementsByTagName("Quotas");
                            if (elementsByTagName.Count > 0)
                            {
                                XmlNode node = elementsByTagName[0];
                                foreach (XmlAttribute attribute in node.Attributes)
                                {
                                    string str7 = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", new object[] { path, '\\', attribute.Name });
                                    if (methodname == ProviderMethods.GetChildNames)
                                    {
                                        base.WriteItemObject(attribute.Name, str7, false);
                                    }
                                    else
                                    {
                                        PSObject item = this.GetItemPSObjectWithTypeName(attribute.Name, attribute.Value.GetType().ToString(), attribute.Value, null, null, WsManElementObjectTypes.WSManConfigLeafElement, null);
                                        base.WriteItemObject(item, str7, false);
                                    }
                                }
                            }
                        }
                        else if (path.Contains(string.Concat(new object[] { str4, str5, '\\', "Resources" })))
                        {
                            str4 = str4 + str5 + '\\';
                            if (path.EndsWith(str4 + "Resources", StringComparison.OrdinalIgnoreCase) && (list2 != null))
                            {
                                foreach (PSObject obj8 in list2)
                                {
                                    switch (methodname)
                                    {
                                        case ProviderMethods.GetChildItems:
                                        {
                                            string[] keys = new string[] { "Uri" + '=' + obj8.Properties["ResourceURI"].Value.ToString() };
                                            PSObject obj9 = new PSObject();
                                            obj9.Properties.Add(new PSNoteProperty(obj8.Properties["ResourceDir"].Value.ToString(), "Container"));
                                            this.WritePSObjectPropertiesAsWSManElementObjects(obj9, path, keys, null, WsManElementObjectTypes.WSManConfigContainerElement, recurse);
                                            break;
                                        }
                                        case ProviderMethods.GetChildNames:
                                            base.WriteItemObject(obj8.Properties["ResourceDir"].Value.ToString(), path, true);
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                str4 = str4 + "Resources" + '\\';
                                int index = path.IndexOf('\\', str4.Length);
                                string str8 = string.Empty;
                                if (index == -1)
                                {
                                    str8 = path.Substring(str4.Length);
                                }
                                else
                                {
                                    str8 = path.Substring(str4.Length, path.IndexOf('\\', str4.Length) - str4.Length);
                                }
                                if ((list2 != null) && path.Contains(str4 + str8))
                                {
                                    if (path.EndsWith(str4 + str8, StringComparison.OrdinalIgnoreCase))
                                    {
                                        foreach (PSObject obj10 in list2)
                                        {
                                            if (str8.Equals(obj10.Properties["ResourceDir"].Value.ToString()))
                                            {
                                                obj10.Properties.Remove("ResourceDir");
                                                switch (methodname)
                                                {
                                                    case ProviderMethods.GetChildItems:
                                                        this.WritePSObjectPropertiesAsWSManElementObjects(obj10, path, null, null, WsManElementObjectTypes.WSManConfigLeafElement, recurse);
                                                        break;

                                                    case ProviderMethods.GetChildNames:
                                                        this.WritePSObjectPropertyNames(obj10, path);
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        str4 = str4 + str8 + '\\';
                                        if ((path.EndsWith(str4 + "Security", StringComparison.OrdinalIgnoreCase) || path.Contains('\\' + "Security_")) && (arrSecurity != null))
                                        {
                                            foreach (PSObject obj11 in arrSecurity)
                                            {
                                                if (str8.Equals(obj11.Properties["ResourceDir"].Value.ToString(), StringComparison.OrdinalIgnoreCase))
                                                {
                                                    if (path.EndsWith(str4 + "Security", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        obj11.Properties.Remove("ResourceDir");
                                                        switch (methodname)
                                                        {
                                                            case ProviderMethods.GetChildItems:
                                                            {
                                                                string str9 = "Uri" + '=' + obj11.Properties["Uri"].Value.ToString();
                                                                PSObject obj12 = new PSObject();
                                                                obj12.Properties.Add(new PSNoteProperty(obj11.Properties["SecurityDIR"].Value.ToString(), "Container"));
                                                                this.WritePSObjectPropertiesAsWSManElementObjects(obj12, path, new string[] { str9 }, null, WsManElementObjectTypes.WSManConfigContainerElement, recurse);
                                                                break;
                                                            }
                                                            case ProviderMethods.GetChildNames:
                                                                base.WriteItemObject(obj11.Properties["SecurityDIR"].Value.ToString(), path, true);
                                                                break;
                                                        }
                                                    }
                                                    else if (path.Substring(path.LastIndexOf('\\') + 1, path.Length - (path.LastIndexOf('\\') + 1)).Equals(obj11.Properties["SecurityDIR"].Value.ToString(), StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        obj11.Properties.Remove("ResourceDir");
                                                        obj11.Properties.Remove("SecurityDIR");
                                                        switch (methodname)
                                                        {
                                                            case ProviderMethods.GetChildItems:
                                                                this.WritePSObjectPropertiesAsWSManElementObjects(obj11, path, null, null, WsManElementObjectTypes.WSManConfigLeafElement, recurse);
                                                                break;

                                                            case ProviderMethods.GetChildNames:
                                                                this.WritePSObjectPropertyNames(obj11, path);
                                                                break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (path.EndsWith(string.Concat(new object[] { hostName, '\\', "Plugin", '\\', str5, '\\', "InitializationParameters" }), StringComparison.OrdinalIgnoreCase) && (list3 != null))
                        {
                            foreach (PSObject obj13 in list3)
                            {
                                switch (methodname)
                                {
                                    case ProviderMethods.GetChildItems:
                                        this.WritePSObjectPropertiesAsWSManElementObjects(obj13, path, null, "InitParams", WsManElementObjectTypes.WSManConfigLeafElement, recurse);
                                        break;

                                    case ProviderMethods.GetChildNames:
                                        this.WritePSObjectPropertyNames(obj13, path);
                                        break;
                                }
                            }
                        }
                    }
                    goto Label_0B27;
                Label_0A51:
                    if (((path.EndsWith("Service", StringComparison.OrdinalIgnoreCase) || path.EndsWith("TrustedHosts", StringComparison.OrdinalIgnoreCase)) || (path.EndsWith("Client", StringComparison.OrdinalIgnoreCase) || path.EndsWith("DefaultPorts", StringComparison.OrdinalIgnoreCase))) || (path.EndsWith("Auth", StringComparison.OrdinalIgnoreCase) || path.EndsWith("Shell", StringComparison.OrdinalIgnoreCase)))
                    {
                        foreach (XmlNode node2 in document.ChildNodes)
                        {
                            PSObject obj14 = this.ConvertToPSObject(node2);
                            switch (methodname)
                            {
                                case ProviderMethods.GetChildItems:
                                    this.WritePSObjectPropertiesAsWSManElementObjects(obj14, path, null, null, WsManElementObjectTypes.WSManConfigLeafElement, recurse);
                                    break;

                                case ProviderMethods.GetChildNames:
                                    this.WritePSObjectPropertyNames(obj14, path);
                                    break;
                            }
                        }
                    }
                Label_0B27:;
                }
            }
        }

        private void GetChildItemsRecurse(string path, string childname, ProviderMethods methodname, bool recurse)
        {
            if (path.Equals("WSMan"))
            {
                path = childname;
            }
            else
            {
                path = path + '\\' + childname;
            }
            if (this.HasChildItems(path))
            {
                this.GetChildItemsOrNames(path, ProviderMethods.GetChildItems, recurse);
            }
        }

        protected override string GetChildName(string path)
        {
            string childName = string.Empty;
            int num = path.LastIndexOf('\\');
            string hostname = string.Empty;
            if (num == -1)
            {
                childName = path;
                hostname = path;
            }
            else
            {
                childName = path.Substring(num + 1);
                hostname = this.GetHostName(path);
            }
            return this.GetCorrectCaseOfName(childName, hostname, path);
        }

        protected override void GetChildNames(string path, ReturnContainers returnContainers)
        {
            this.GetChildItemsOrNames(path, ProviderMethods.GetChildNames, false);
        }

        private string GetCorrectCaseOfName(string ChildName, string hostname, string path)
        {
            string str = ChildName;
            if (ChildName != null)
            {
                if (!ChildName.Contains("_"))
                {
                    if (ChildName.Equals("Quotas", StringComparison.OrdinalIgnoreCase))
                    {
                        return "Quotas";
                    }
                    if (ChildName.Equals("Plugin", StringComparison.OrdinalIgnoreCase))
                    {
                        return "Plugin";
                    }
                    if (ChildName.Equals("Resources", StringComparison.OrdinalIgnoreCase))
                    {
                        return "Resources";
                    }
                    if (ChildName.Equals("Security", StringComparison.OrdinalIgnoreCase))
                    {
                        return "Security";
                    }
                    if (ChildName.Equals("Service", StringComparison.OrdinalIgnoreCase))
                    {
                        return "Service";
                    }
                    if (ChildName.Equals("Shell", StringComparison.OrdinalIgnoreCase))
                    {
                        return "Shell";
                    }
                    if (ChildName.Equals("TrustedHosts", StringComparison.OrdinalIgnoreCase))
                    {
                        return "TrustedHosts";
                    }
                    if (ChildName.Equals("Auth", StringComparison.OrdinalIgnoreCase))
                    {
                        return "Auth";
                    }
                    if (ChildName.Equals("Client", StringComparison.OrdinalIgnoreCase))
                    {
                        return "Client";
                    }
                    if (ChildName.Equals("ClientCertificate", StringComparison.OrdinalIgnoreCase))
                    {
                        return "ClientCertificate";
                    }
                    if (ChildName.Equals("DefaultPorts", StringComparison.OrdinalIgnoreCase))
                    {
                        return "DefaultPorts";
                    }
                    if (ChildName.Equals("InitializationParameters", StringComparison.OrdinalIgnoreCase))
                    {
                        return "InitializationParameters";
                    }
                    if (ChildName.Equals("Listener", StringComparison.OrdinalIgnoreCase))
                    {
                        return "Listener";
                    }
                    if (string.IsNullOrEmpty(hostname))
                    {
                        return str;
                    }
                    Dictionary<string, object> sessionObjCache = WSManHelper.GetSessionObjCache();
                    if (ChildName.Equals(hostname, StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (string str2 in sessionObjCache.Keys)
                        {
                            if (ChildName.Equals(str2, StringComparison.OrdinalIgnoreCase))
                            {
                                str = str2;
                            }
                        }
                        return str;
                    }
                    if (!path.Contains(hostname + '\\' + "Plugin"))
                    {
                        return str;
                    }
                    if (this.IsPathLocalMachine(hostname) && !this.IsWSManServiceRunning())
                    {
                        WSManHelper.ThrowIfNotAdministrator();
                        this.StartWSManService((bool) base.Force);
                    }
                    string resourceURI = this.NormalizePath(path, hostname);
                    lock (WSManHelper.AutoSession)
                    {
                        object obj2;
                        sessionObjCache.TryGetValue(hostname, out obj2);
                        XmlDocument xmlPlugins = this.FindResourceValue(obj2, resourceURI, null);
                        if (xmlPlugins != null)
                        {
                            string currentPluginName = string.Empty;
                            this.GetPluginNames(xmlPlugins, out this.objPluginNames, out currentPluginName, path);
                            if (path.EndsWith(string.Concat(new object[] { hostname, '\\', "Plugin", '\\', currentPluginName }), StringComparison.OrdinalIgnoreCase))
                            {
                                str = currentPluginName;
                            }
                        }
                        return str;
                    }
                }
                if (ChildName.StartsWith("Listener", StringComparison.OrdinalIgnoreCase))
                {
                    str = "Listener_" + ChildName.Substring(ChildName.IndexOf('_') + 1);
                }
                if (ChildName.StartsWith("Resource", StringComparison.OrdinalIgnoreCase))
                {
                    str = "Resource_" + ChildName.Substring(ChildName.IndexOf('_') + 1);
                }
                if (ChildName.StartsWith("Security", StringComparison.OrdinalIgnoreCase))
                {
                    str = "Security_" + ChildName.Substring(ChildName.IndexOf('_') + 1);
                }
                if (ChildName.StartsWith("ClientCertificate", StringComparison.OrdinalIgnoreCase))
                {
                    str = "ClientCertificate_" + ChildName.Substring(ChildName.IndexOf('_') + 1);
                }
            }
            return str;
        }

        private string GetCorrectCaseOfPath(string path)
        {
            char ch = '\\';
            if (!path.Contains(ch.ToString()))
            {
                return this.GetChildName(path);
            }
            string[] strArray = path.Split(new char[] { '\\' });
            StringBuilder builder = new StringBuilder();
            bool flag = true;
            StringBuilder builder2 = new StringBuilder();
            foreach (string str in strArray)
            {
                if (flag)
                {
                    flag = false;
                    builder2.Append(this.GetChildName(str));
                    builder.Append(builder2);
                }
                else
                {
                    builder2.Append('\\');
                    builder2.Append(str);
                    builder.Append('\\');
                    builder.Append(this.GetChildName(builder2.ToString()));
                }
            }
            return builder.ToString();
        }

        private string GetFilterString(Hashtable cmdlinevalues, string[] pkey)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string str in pkey)
            {
                if (cmdlinevalues.Contains(str))
                {
                    builder.Append(str);
                    builder.Append("=");
                    builder.Append(cmdlinevalues[str].ToString());
                    builder.Append("+");
                }
            }
            if (builder.ToString().EndsWith("+", StringComparison.OrdinalIgnoreCase))
            {
                builder.Remove(builder.ToString().Length - 1, 1);
            }
            return builder.ToString();
        }

        private string GetHostName(string path)
        {
            string key = path;
            try
            {
                char ch = '\\';
                if (path.Contains(ch.ToString()))
                {
                    key = path.Substring(0, path.IndexOf('\\'));
                }
                if (!WSManHelper.GetSessionObjCache().ContainsKey(key))
                {
                    key = null;
                }
            }
            catch (ArgumentNullException exception)
            {
                ErrorRecord errorRecord = new ErrorRecord(exception, "ArgumentNullException", ErrorCategory.InvalidArgument, null);
                base.WriteError(errorRecord);
            }
            return key;
        }

        private string GetInputStringForCreate(string ResourceURI, Hashtable value, string host)
        {
            string str2 = string.Empty;
            StringBuilder builder = new StringBuilder();
            if (value.Count > 0)
            {
                foreach (string str3 in value.Keys)
                {
                    if (!this.IsPKey(str3, ResourceURI))
                    {
                        builder.Append("<p:");
                        builder.Append(str3);
                        if (value[str3] == null)
                        {
                            builder.Append(" ");
                            builder.Append("xsi:nil=\"true\"");
                            str2 = " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"";
                        }
                        builder.Append(">");
                        builder.Append(this.EscapeValuesForXML(value[str3].ToString()));
                        builder.Append("</p:");
                        builder.Append(str3);
                        builder.Append(">");
                    }
                }
            }
            string rootNodeName = this.GetRootNodeName(ResourceURI);
            return ("<p:" + rootNodeName + " xmlns:p=\"" + this.SetSchemaPath(ResourceURI) + ".xsd\"" + str2 + ">" + builder.ToString() + "</p:" + rootNodeName + ">");
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        protected override void GetItem(string path)
        {
            string str = string.Empty;
            if ((path.Length == 0) && string.IsNullOrEmpty(str))
            {
                base.WriteItemObject(this.GetItemPSObjectWithTypeName("WSMan", "Container", null, null, null, WsManElementObjectTypes.WSManConfigElement, null), "WSMan", true);
            }
            else
            {
                char ch = '\\';
                if (path.Contains(ch.ToString()))
                {
                    str = path.Substring(path.LastIndexOf('\\') + 1);
                }
                else
                {
                    str = path;
                }
                Dictionary<string, object> sessionObjCache = WSManHelper.GetSessionObjCache();
                if (str.Equals(path, StringComparison.OrdinalIgnoreCase))
                {
                    if (sessionObjCache.ContainsKey(str))
                    {
                        base.WriteItemObject(this.GetItemPSObjectWithTypeName(str, "Container", null, null, "ComputerLevel", WsManElementObjectTypes.WSManConfigContainerElement, null), "WSMan" + '\\' + str, true);
                    }
                }
                else
                {
                    path = path.Substring(0, path.LastIndexOf(str, StringComparison.OrdinalIgnoreCase));
                    string hostName = this.GetHostName(path);
                    string resourceURI = this.NormalizePath(path, hostName);
                    lock (WSManHelper.AutoSession)
                    {
                        object obj2;
                        sessionObjCache.TryGetValue(hostName, out obj2);
                        XmlDocument xmlResource = this.FindResourceValue(obj2, resourceURI, null);
                        if (xmlResource != null)
                        {
                            char ch2 = '\\';
                            if (path.EndsWith(ch2.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                path = path.Remove(path.LastIndexOf('\\'));
                            }
                            string str4 = hostName + '\\';
                            if (path.Contains(str4 + "Listener"))
                            {
                                this.GetItemListenerOrCertMapping(path, xmlResource, "Listener", str, hostName);
                            }
                            else if (path.Contains(str4 + "ClientCertificate"))
                            {
                                this.GetItemListenerOrCertMapping(path, xmlResource, "ClientCertificate", str, hostName);
                            }
                            else if (path.Contains(str4 + "Plugin"))
                            {
                                string currentPluginName = string.Empty;
                                this.GetPluginNames(xmlResource, out this.objPluginNames, out currentPluginName, path);
                                if (path.EndsWith(str4 + "Plugin", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        base.WriteItemObject(this.GetItemPSObjectWithTypeName(this.objPluginNames.Properties[str].Name, this.objPluginNames.Properties[str].Value.ToString(), null, new string[] { "Name=" + this.objPluginNames.Properties[str].Name }, null, WsManElementObjectTypes.WSManConfigContainerElement, null), path + '\\' + str, true);
                                    }
                                    catch (PSArgumentNullException)
                                    {
                                    }
                                    catch (NullReferenceException)
                                    {
                                    }
                                }
                                else
                                {
                                    str4 = str4 + "Plugin" + '\\';
                                    string str6 = resourceURI + "?Name=" + currentPluginName;
                                    XmlDocument xmldoc = this.GetResourceValue(obj2, str6, null);
                                    if (xmldoc != null)
                                    {
                                        PSObject obj3 = this.ProcessPluginConfigurationLevel(xmldoc, true);
                                        ArrayList arrSecurity = null;
                                        ArrayList list2 = this.ProcessPluginResourceLevel(xmldoc, out arrSecurity);
                                        ArrayList list3 = this.ProcessPluginInitParamLevel(xmldoc);
                                        try
                                        {
                                            if (path.Contains(str4 + currentPluginName))
                                            {
                                                if (path.EndsWith(str4 + currentPluginName, StringComparison.OrdinalIgnoreCase))
                                                {
                                                    if (!obj3.Properties[str].Value.ToString().Equals("Container"))
                                                    {
                                                        base.WriteItemObject(this.GetItemPSObjectWithTypeName(obj3.Properties[str].Name, obj3.Properties[str].TypeNameOfValue, obj3.Properties[str].Value, null, null, WsManElementObjectTypes.WSManConfigLeafElement, null), path + '\\' + obj3.Properties[str].Name, false);
                                                    }
                                                    else
                                                    {
                                                        base.WriteItemObject(this.GetItemPSObjectWithTypeName(obj3.Properties[str].Name, obj3.Properties[str].Value.ToString(), null, null, null, WsManElementObjectTypes.WSManConfigLeafElement, null), path + '\\' + obj3.Properties[str].Name, true);
                                                    }
                                                }
                                                str4 = str4 + currentPluginName + '\\';
                                                if (path.Contains(str4 + "Resources"))
                                                {
                                                    if (list2 != null)
                                                    {
                                                        if (path.EndsWith(str4 + "Resources", StringComparison.OrdinalIgnoreCase))
                                                        {
                                                            foreach (PSObject obj4 in list2)
                                                            {
                                                                if (obj4.Properties["ResourceDir"].Value.ToString().Equals(str))
                                                                {
                                                                    base.WriteItemObject(this.GetItemPSObjectWithTypeName(str, "Container", null, new string[] { "ResourceURI=" + obj4.Properties["ResourceUri"].Value.ToString() }, null, WsManElementObjectTypes.WSManConfigContainerElement, null), path + '\\' + str, true);
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            str4 = str4 + "Resources" + '\\';
                                                            int index = path.IndexOf('\\', str4.Length);
                                                            string str7 = string.Empty;
                                                            if (index == -1)
                                                            {
                                                                str7 = path.Substring(str4.Length);
                                                            }
                                                            else
                                                            {
                                                                str7 = path.Substring(str4.Length, path.IndexOf('\\', str4.Length) - str4.Length);
                                                            }
                                                            if (path.Contains(str4 + str7))
                                                            {
                                                                if (path.EndsWith(str4 + str7, StringComparison.OrdinalIgnoreCase))
                                                                {
                                                                    foreach (PSObject obj5 in list2)
                                                                    {
                                                                        if (str7.Equals(obj5.Properties["ResourceDir"].Value.ToString(), StringComparison.OrdinalIgnoreCase))
                                                                        {
                                                                            obj5.Properties.Remove("ResourceDir");
                                                                            if (obj5.Properties[str].Value.ToString().Equals("Container"))
                                                                            {
                                                                                base.WriteItemObject(this.GetItemPSObjectWithTypeName(obj5.Properties[str].Name, obj5.Properties[str].Value.ToString(), null, null, null, WsManElementObjectTypes.WSManConfigLeafElement, null), path + '\\' + obj5.Properties[str].Name, true);
                                                                            }
                                                                            else
                                                                            {
                                                                                base.WriteItemObject(this.GetItemPSObjectWithTypeName(obj5.Properties[str].Name, obj5.Properties[str].TypeNameOfValue, obj5.Properties[str].Value, null, null, WsManElementObjectTypes.WSManConfigLeafElement, null), path + '\\' + obj5.Properties[str].Name, false);
                                                                            }
                                                                            goto Label_0C34;
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    str4 = str4 + str7 + '\\';
                                                                    if (path.Contains(str4 + "Security") && (arrSecurity != null))
                                                                    {
                                                                        foreach (PSObject obj6 in arrSecurity)
                                                                        {
                                                                            if (path.EndsWith("Security", StringComparison.OrdinalIgnoreCase))
                                                                            {
                                                                                base.WriteItemObject(this.GetItemPSObjectWithTypeName(obj6.Properties["SecurityDIR"].Value.ToString(), "Container", null, new string[] { "Uri=" + obj6.Properties["Uri"].Value.ToString() }, null, WsManElementObjectTypes.WSManConfigContainerElement, null), path + '\\' + obj6.Properties["SecurityDIR"].Value.ToString(), true);
                                                                            }
                                                                            else if (path.Substring(path.LastIndexOf('\\') + 1, path.Length - (path.LastIndexOf('\\') + 1)).Equals(obj6.Properties["SecurityDIR"].Value.ToString()))
                                                                            {
                                                                                obj6.Properties.Remove("SecurityDIR");
                                                                                base.WriteItemObject(this.GetItemPSObjectWithTypeName(obj6.Properties[str].Name, obj6.Properties[str].TypeNameOfValue, obj6.Properties[str].Value, null, null, WsManElementObjectTypes.WSManConfigLeafElement, null), path + '\\' + obj6.Properties[str].Name, false);
                                                                                goto Label_0C34;
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else if (path.EndsWith(string.Concat(new object[] { hostName, '\\', "Plugin", '\\', currentPluginName, '\\', "InitializationParameters" }), StringComparison.OrdinalIgnoreCase))
                                                {
                                                    if (list3 != null)
                                                    {
                                                        foreach (PSObject obj7 in list3)
                                                        {
                                                            if (obj7.Properties.Match(str, PSMemberTypes.NoteProperty).Count > 0)
                                                            {
                                                                base.WriteItemObject(this.GetItemPSObjectWithTypeName(obj7.Properties[str].Name, obj7.Properties[str].TypeNameOfValue, obj7.Properties[str].Value, null, "InitParams", WsManElementObjectTypes.WSManConfigLeafElement, null), path + '\\' + obj7.Properties[str].Name, false);
                                                            }
                                                        }
                                                    }
                                                }
                                                else if (path.EndsWith("Quotas", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    XmlNodeList elementsByTagName = xmldoc.GetElementsByTagName("Quotas");
                                                    if (elementsByTagName.Count > 0)
                                                    {
                                                        XmlNode node = elementsByTagName[0];
                                                        foreach (XmlAttribute attribute in node.Attributes)
                                                        {
                                                            if (str.Equals(attribute.Name, StringComparison.OrdinalIgnoreCase))
                                                            {
                                                                PSObject item = this.GetItemPSObjectWithTypeName(attribute.Name, attribute.Value.GetType().ToString(), attribute.Value, null, null, WsManElementObjectTypes.WSManConfigLeafElement, null);
                                                                string str9 = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", new object[] { path, '\\', attribute.Name });
                                                                base.WriteItemObject(item, str9, false);
                                                                goto Label_0C34;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        catch (PSArgumentNullException)
                                        {
                                        }
                                        catch (NullReferenceException)
                                        {
                                        }
                                    }
                                }
                            }
                            else
                            {
                                try
                                {
                                    PSObject input = null;
                                    if (!resourceURI.Equals(WinrmRootName[0].ToString(), StringComparison.OrdinalIgnoreCase))
                                    {
                                        foreach (XmlNode node2 in xmlResource.ChildNodes)
                                        {
                                            input = this.ConvertToPSObject(node2);
                                        }
                                    }
                                    else
                                    {
                                        input = this.BuildHostLevelPSObjectArrayList(obj2, resourceURI, false);
                                    }
                                    if (input != null)
                                    {
                                        if (input.Properties[str].Value.ToString().Equals("Container"))
                                        {
                                            base.WriteItemObject(this.GetItemPSObjectWithTypeName(input.Properties[str].Name, input.Properties[str].Value.ToString(), null, null, null, WsManElementObjectTypes.WSManConfigLeafElement, null), path + '\\' + input.Properties[str].Name, true);
                                        }
                                        else
                                        {
                                            base.WriteItemObject(this.GetItemPSObjectWithTypeName(input.Properties[str].Name, input.Properties[str].TypeNameOfValue, input.Properties[str].Value, null, null, WsManElementObjectTypes.WSManConfigLeafElement, input), path + '\\' + input.Properties[str].Name, false);
                                        }
                                    }
                                }
                                catch (PSArgumentNullException)
                                {
                                }
                                catch (NullReferenceException)
                                {
                                }
                            }
                        }
                    Label_0C34:;
                    }
                }
            }
        }

        private void GetItemListenerOrCertMapping(string path, XmlDocument xmlResource, string ContainerListenerOrClientCert, string childname, string host)
        {
            Hashtable hashtable;
            Hashtable hashtable2;
            if (ContainerListenerOrClientCert.Equals("Listener", StringComparison.OrdinalIgnoreCase))
            {
                this.ProcessListernerObjects(xmlResource, out hashtable, out hashtable2);
            }
            else if (ContainerListenerOrClientCert.Equals("ClientCertificate", StringComparison.OrdinalIgnoreCase))
            {
                this.ProcessCertMappingObjects(xmlResource, out hashtable, out hashtable2);
            }
            else
            {
                return;
            }
            if ((hashtable != null) && (hashtable2 != null))
            {
                if (path.EndsWith(host + '\\' + ContainerListenerOrClientCert, StringComparison.OrdinalIgnoreCase))
                {
                    if (hashtable.ContainsKey(childname))
                    {
                        base.WriteItemObject(this.GetItemPSObjectWithTypeName(childname, "Container", null, (string[]) hashtable2[childname], null, WsManElementObjectTypes.WSManConfigContainerElement, null), path + '\\' + childname, true);
                    }
                }
                else
                {
                    string str = path.Substring(path.LastIndexOf('\\') + 1);
                    try
                    {
                        base.WriteItemObject(this.GetItemPSObjectWithTypeName(((PSObject) hashtable[str]).Properties[childname].Name, ((PSObject) hashtable[str]).Properties[childname].TypeNameOfValue, ((PSObject) hashtable[str]).Properties[childname].Value, null, null, WsManElementObjectTypes.WSManConfigLeafElement, null), path + '\\' + childname, false);
                    }
                    catch (PSArgumentException)
                    {
                    }
                }
            }
        }

        private PSObject GetItemPSObjectWithTypeName(string Name, string TypeNameOfElement, object Value, string[] keys, string ExtendedTypeName, WsManElementObjectTypes WSManElementObjectType, PSObject input = null)
        {
            PSObject obj2 = null;
            if (WSManElementObjectType.Equals(WsManElementObjectTypes.WSManConfigElement))
            {
                WSManConfigElement element = new WSManConfigElement(Name, TypeNameOfElement);
                obj2 = new PSObject(element);
            }
            if (WSManElementObjectType.Equals(WsManElementObjectTypes.WSManConfigContainerElement))
            {
                WSManConfigContainerElement element2 = new WSManConfigContainerElement(Name, TypeNameOfElement, keys);
                obj2 = new PSObject(element2);
            }
            if (WSManElementObjectType.Equals(WsManElementObjectTypes.WSManConfigLeafElement))
            {
                object sourceOfValue = null;
                if (input != null)
                {
                    string str = Name + "___Source";
                    if (input.Properties[str] != null)
                    {
                        sourceOfValue = input.Properties[str].Value;
                    }
                }
                WSManConfigLeafElement element3 = new WSManConfigLeafElement(Name, Value, TypeNameOfElement, sourceOfValue);
                obj2 = new PSObject(element3);
            }
            if (!string.IsNullOrEmpty(ExtendedTypeName))
            {
                StringBuilder builder = new StringBuilder("");
                if (obj2 != null)
                {
                    builder.Append(obj2.ImmediateBaseObject.GetType().FullName);
                    builder.Append("#");
                    builder.Append(ExtendedTypeName);
                    obj2.TypeNames.Insert(0, builder.ToString());
                }
            }
            return obj2;
        }

        private PSObject GetItemValue(string path)
        {
            if (string.IsNullOrEmpty(path) || (path.Length == 0))
            {
                throw new ArgumentNullException(path);
            }
            char ch = '\\';
            if (path.EndsWith(ch.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                path = path.Remove(path.LastIndexOf('\\'));
            }
            string hostName = this.GetHostName(path);
            if (string.IsNullOrEmpty(hostName))
            {
                throw new InvalidOperationException("InvalidPath");
            }
            if (this.IsPathLocalMachine(hostName) && !this.IsWSManServiceRunning())
            {
                this.AssertError("WinRMServiceError", false);
            }
            lock (WSManHelper.AutoSession)
            {
                object obj2;
                WSManHelper.GetSessionObjCache().TryGetValue(hostName, out obj2);
                string uri = this.NormalizePath(path, hostName);
                string str3 = hostName + '\\';
                if (path.EndsWith(hostName, StringComparison.OrdinalIgnoreCase))
                {
                    return this.BuildHostLevelPSObjectArrayList(obj2, uri, false);
                }
                XmlDocument xmlPlugins = this.FindResourceValue(obj2, uri, null);
                if ((xmlPlugins == null) || !xmlPlugins.HasChildNodes)
                {
                    return null;
                }
                if (path.Contains(str3 + "Listener"))
                {
                    throw new NotSupportedException();
                }
                if (path.Contains(str3 + "ClientCertificate"))
                {
                    throw new NotSupportedException();
                }
                if (path.Contains(str3 + "Plugin"))
                {
                    string currentPluginName = string.Empty;
                    this.GetPluginNames(xmlPlugins, out this.objPluginNames, out currentPluginName, path);
                    if (path.EndsWith(str3 + "Plugin", StringComparison.OrdinalIgnoreCase))
                    {
                        return this.objPluginNames;
                    }
                    string resourceURI = uri + "?Name=" + currentPluginName;
                    XmlDocument xmldoc = this.GetResourceValue(obj2, resourceURI, null);
                    if (xmldoc == null)
                    {
                        return null;
                    }
                    this.ProcessPluginConfigurationLevel(xmldoc, false);
                    ArrayList arrSecurity = null;
                    ArrayList list2 = this.ProcessPluginResourceLevel(xmldoc, out arrSecurity);
                    this.ProcessPluginInitParamLevel(xmldoc);
                    str3 = (str3 + "Plugin" + '\\') + currentPluginName + '\\';
                    if (list2 == null)
                    {
                        return null;
                    }
                    str3 = str3 + "Resources" + '\\';
                    int index = path.IndexOf('\\', str3.Length);
                    string str6 = string.Empty;
                    if (index == -1)
                    {
                        str6 = path.Substring(str3.Length);
                    }
                    else
                    {
                        str6 = path.Substring(str3.Length, path.IndexOf('\\', str3.Length) - str3.Length);
                    }
                    if (path.EndsWith(str3 + str6, StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (PSObject obj4 in list2)
                        {
                            if (str6.Equals(obj4.Properties["ResourceDir"].Value.ToString()))
                            {
                                obj4.Properties.Remove("ResourceDir");
                                return obj4;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private int GetPluginNames(XmlDocument xmlPlugins, out PSObject PluginNames, out string CurrentPluginName, string path)
        {
            PluginNames = new PSObject();
            CurrentPluginName = string.Empty;
            if (!path.Contains('\\' + "Plugin"))
            {
                return 0;
            }
            string[] strArray = path.Split(new char[] { '\\' });
            XmlNodeList elementsByTagName = xmlPlugins.GetElementsByTagName("PlugInConfiguration");
            foreach (XmlElement element in elementsByTagName)
            {
                for (int i = 0; i <= (element.Attributes.Count - 1); i++)
                {
                    if (element.Attributes[i].LocalName.Equals("Name"))
                    {
                        PluginNames.Properties.Add(new PSNoteProperty(element.Attributes[i].Value, "Container"));
                        if ((strArray.Length >= 3) && strArray[2].Equals(element.Attributes[i].Value, StringComparison.InvariantCultureIgnoreCase))
                        {
                            CurrentPluginName = element.Attributes[i].Value;
                        }
                    }
                }
            }
            return elementsByTagName.Count;
        }

        private XmlDocument GetResourceValue(object sessionobj, string ResourceURI, Hashtable cmdlinevalues)
        {
            XmlDocument document = null;
            string xml = this.GetResourceValueInXml(sessionobj, ResourceURI, cmdlinevalues);
            document = new XmlDocument();
            document.LoadXml(xml);
            return document;
        }

        private string GetResourceValueInXml(object sessionobj, string ResourceURI, Hashtable cmdlinevalues)
        {
            string str2;
            try
            {
                ResourceURI = this.GetURIWithFilter(ResourceURI, cmdlinevalues);
                string str = string.Empty;
                if (!this.getMapping.TryGetValue(ResourceURI, out str))
                {
                    str = ((IWSManSession) sessionobj).Get(ResourceURI, 0);
                    this.getMapping.Add(ResourceURI, str);
                }
                str2 = str;
            }
            finally
            {
                if (!string.IsNullOrEmpty(((IWSManSession) sessionobj).Error))
                {
                    this.AssertError(((IWSManSession) sessionobj).Error, true);
                }
            }
            return str2;
        }

        private string GetRootNodeName(string ResourceURI)
        {
            string str = "";
            if (ResourceURI.Contains("?"))
            {
                ResourceURI = ResourceURI.Split(new char[] { '?' }).GetValue(0).ToString();
            }
            string pattern = "([a-z_][-a-z0-9._]*)$";
            MatchCollection matchs = new Regex(pattern, RegexOptions.IgnoreCase).Matches(ResourceURI);
            if (matchs.Count > 0)
            {
                str = matchs[0].Value.ToString();
            }
            return str;
        }

        private string GetStringFromSecureString(object propertyValue)
        {
            SecureString s = propertyValue as SecureString;
            string str2 = string.Empty;
            if (s != null)
            {
                IntPtr ptr = Marshal.SecureStringToBSTR(s);
                str2 = Marshal.PtrToStringAuto(ptr);
                Marshal.ZeroFreeBSTR(ptr);
            }
            return str2;
        }

        private string GetURIWithFilter(string uri, Hashtable cmdlinevalues)
        {
            StringBuilder builder = new StringBuilder(uri);
            if (cmdlinevalues != null)
            {
                if (uri.Contains("Config/Listener"))
                {
                    builder.Append("?");
                    builder.Append(this.GetFilterString(cmdlinevalues, PKeyListener));
                }
                else if (uri.Contains("Config/Service/certmapping"))
                {
                    builder.Append("?");
                    builder.Append(this.GetFilterString(cmdlinevalues, PKeyCertMapping));
                }
                else if (uri.Contains("Config/Plugin"))
                {
                    builder.Append("?");
                    builder.Append(this.GetFilterString(cmdlinevalues, PKeyPlugin));
                }
            }
            return builder.ToString();
        }

        protected override bool HasChildItems(string path)
        {
            string str = string.Empty;
            string str2 = string.Empty;
            if ((path.Length == 0) && string.IsNullOrEmpty(str))
            {
                return true;
            }
            char ch = '\\';
            if (path.EndsWith(ch.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                path = path.Remove(path.LastIndexOf('\\'));
            }
            char ch2 = '\\';
            if (path.Contains(ch2.ToString()))
            {
                str = path.Substring(path.LastIndexOf('\\') + 1);
            }
            Dictionary<string, object> sessionObjCache = WSManHelper.GetSessionObjCache();
            if (sessionObjCache.ContainsKey(path))
            {
                return true;
            }
            string hostName = this.GetHostName(path);
            if (this.IsPathLocalMachine(hostName) && !this.IsWSManServiceRunning())
            {
                WSManHelper.ThrowIfNotAdministrator();
                this.StartWSManService((bool) base.Force);
            }
            string resourceURI = this.NormalizePath(path, hostName);
            lock (WSManHelper.AutoSession)
            {
                object obj2;
                sessionObjCache.TryGetValue(hostName, out obj2);
                str2 = hostName + '\\';
                if (resourceURI.Contains("Listener"))
                {
                    XmlDocument xmlListeners = this.EnumerateResourceValue(obj2, resourceURI);
                    if (xmlListeners != null)
                    {
                        Hashtable hashtable;
                        Hashtable hashtable2;
                        this.ProcessListernerObjects(xmlListeners, out hashtable2, out hashtable);
                        if (hashtable2.Count > 0)
                        {
                            return true;
                        }
                    }
                }
                else if (resourceURI.Contains("Service/certmapping"))
                {
                    Hashtable hashtable3;
                    Hashtable hashtable4;
                    XmlDocument xmlCerts = this.EnumerateResourceValue(obj2, resourceURI);
                    if (xmlCerts == null)
                    {
                        return true;
                    }
                    this.ProcessCertMappingObjects(xmlCerts, out hashtable4, out hashtable3);
                    if (hashtable4.Count > 0)
                    {
                        return true;
                    }
                }
                else if (resourceURI.Contains("Plugin"))
                {
                    str2 = str2 + "Plugin";
                    XmlDocument xmlPlugins = this.FindResourceValue(obj2, resourceURI, null);
                    string currentPluginName = string.Empty;
                    int num = this.GetPluginNames(xmlPlugins, out this.objPluginNames, out currentPluginName, path);
                    if (path.Equals(str2))
                    {
                        return (num > 0);
                    }
                    str2 = str2 + '\\' + currentPluginName;
                    if (path.EndsWith(str2, StringComparison.OrdinalIgnoreCase) && (this.objPluginNames != null))
                    {
                        return (this.objPluginNames.Properties.Match(currentPluginName).Count > 0);
                    }
                    string str6 = resourceURI + "?Name=" + currentPluginName;
                    XmlDocument xmldoc = this.GetResourceValue(obj2, str6, null);
                    ArrayList arrSecurity = null;
                    ArrayList list2 = this.ProcessPluginResourceLevel(xmldoc, out arrSecurity);
                    ArrayList list3 = this.ProcessPluginInitParamLevel(xmldoc);
                    str2 = str2 + '\\';
                    if ((path.EndsWith(str2 + "Resources", StringComparison.OrdinalIgnoreCase) && (list2 != null)) && (list2.Count > 0))
                    {
                        return true;
                    }
                    if ((path.EndsWith(str2 + "InitializationParameters", StringComparison.OrdinalIgnoreCase) && (list3 != null)) && (list3.Count > 0))
                    {
                        return true;
                    }
                    if (path.EndsWith(str2 + "Quotas", StringComparison.OrdinalIgnoreCase))
                    {
                        XmlNodeList elementsByTagName = xmldoc.GetElementsByTagName("Quotas");
                        if (elementsByTagName.Count > 0)
                        {
                            XmlNode node = elementsByTagName[0];
                            return (node.Attributes.Count > 0);
                        }
                        return false;
                    }
                    if (list2 != null)
                    {
                        foreach (PSObject obj3 in list2)
                        {
                            string str7 = obj3.Properties["ResourceDir"].Value.ToString();
                            if (path.Contains(str7))
                            {
                                str2 = str2 + "Resources" + '\\';
                                if (path.EndsWith(str2 + str7, StringComparison.OrdinalIgnoreCase))
                                {
                                    return true;
                                }
                                str2 = str2 + str7 + '\\';
                                if (path.Contains(str2 + "Security"))
                                {
                                    if ((path.EndsWith(str2 + "Security", StringComparison.OrdinalIgnoreCase) && (arrSecurity != null)) && (arrSecurity.Count > 0))
                                    {
                                        return true;
                                    }
                                    str2 = str2 + "Security" + '\\';
                                    if (path.Contains(str2 + "Security_"))
                                    {
                                        if (arrSecurity == null)
                                        {
                                            return false;
                                        }
                                        foreach (PSObject obj4 in arrSecurity)
                                        {
                                            string str8 = obj4.Properties["SecurityDIR"].Value.ToString();
                                            if (path.EndsWith(str8, StringComparison.OrdinalIgnoreCase))
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    string str9 = this.GetResourceValueInXml(obj2, resourceURI, null);
                    XmlDocument resourcexmldocument = new XmlDocument();
                    resourcexmldocument.LoadXml(str9.ToLower(CultureInfo.InvariantCulture));
                    XmlNodeList nodes = this.SearchXml(resourcexmldocument, str, resourceURI, path, hostName);
                    if (nodes != null)
                    {
                        return this.IsItemContainer(nodes);
                    }
                }
                return false;
            }
        }

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            return new Collection<PSDriveInfo> { new PSDriveInfo("WSMan", base.ProviderInfo, string.Empty, this.helper.GetResourceMsgFromResourcetext("ConfigStorage"), null) };
        }

        protected override bool IsItemContainer(string path)
        {
            string str = string.Empty;
            string str2 = string.Empty;
            if ((path.Length == 0) && string.IsNullOrEmpty(str))
            {
                return true;
            }
            char ch = '\\';
            if (path.EndsWith(ch.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                path = path.Remove(path.LastIndexOf('\\'));
            }
            char ch2 = '\\';
            if (path.Contains(ch2.ToString()))
            {
                str = path.Substring(path.LastIndexOf('\\') + 1);
            }
            Dictionary<string, object> sessionObjCache = WSManHelper.GetSessionObjCache();
            if (sessionObjCache.ContainsKey(path))
            {
                return true;
            }
            string hostName = this.GetHostName(path);
            string resourceURI = this.NormalizePath(path, hostName);
            lock (WSManHelper.AutoSession)
            {
                object obj2;
                sessionObjCache.TryGetValue(hostName, out obj2);
                str2 = hostName + '\\';
                if (resourceURI.Contains("Listener"))
                {
                    str2 = str2 + "Listener";
                    if (path.EndsWith(str2, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    XmlDocument xmlListeners = this.EnumerateResourceValue(obj2, resourceURI);
                    if (xmlListeners != null)
                    {
                        Hashtable hashtable;
                        Hashtable hashtable2;
                        this.ProcessListernerObjects(xmlListeners, out hashtable2, out hashtable);
                        if (hashtable.Contains(str))
                        {
                            return true;
                        }
                    }
                }
                else if (resourceURI.Contains("Service/certmapping"))
                {
                    str2 = str2 + "ClientCertificate";
                    if (path.EndsWith(str2, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    XmlDocument xmlCerts = this.EnumerateResourceValue(obj2, resourceURI);
                    if (xmlCerts != null)
                    {
                        Hashtable hashtable3;
                        Hashtable hashtable4;
                        this.ProcessCertMappingObjects(xmlCerts, out hashtable4, out hashtable3);
                        if (hashtable3.Contains(str))
                        {
                            return true;
                        }
                    }
                }
                else if (resourceURI.Contains("Plugin"))
                {
                    str2 = str2 + "Plugin";
                    if (path.EndsWith(str2, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    str2 = str2 + '\\';
                    XmlDocument xmlPlugins = this.FindResourceValue(obj2, resourceURI, null);
                    string currentPluginName = string.Empty;
                    this.GetPluginNames(xmlPlugins, out this.objPluginNames, out currentPluginName, path);
                    str2 = str2 + currentPluginName;
                    if (path.EndsWith(currentPluginName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    string str6 = resourceURI + "?Name=" + currentPluginName;
                    XmlDocument xmldoc = this.GetResourceValue(obj2, str6, null);
                    ArrayList arrSecurity = null;
                    ArrayList list2 = this.ProcessPluginResourceLevel(xmldoc, out arrSecurity);
                    if (path.EndsWith(str2, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    str2 = str2 + '\\';
                    if (path.EndsWith(str2 + "Resources", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    if (path.EndsWith(str2 + "InitializationParameters", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    if (path.EndsWith(str2 + "Quotas", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    if ((list2 == null) || (list2.Count == 0))
                    {
                        return false;
                    }
                    foreach (PSObject obj3 in list2)
                    {
                        string str7 = obj3.Properties["ResourceDir"].Value.ToString();
                        if (path.Contains(str7))
                        {
                            str2 = str2 + "Resources" + '\\';
                            if (path.EndsWith(str2 + str7, StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                            str2 = str2 + str7 + '\\';
                            if (path.Contains(str2 + "Security"))
                            {
                                if (path.EndsWith(str2 + "Security", StringComparison.OrdinalIgnoreCase))
                                {
                                    return true;
                                }
                                str2 = str2 + "Security" + '\\';
                                if (path.Contains(str2 + "Security_"))
                                {
                                    if (arrSecurity == null)
                                    {
                                        return false;
                                    }
                                    foreach (PSObject obj4 in arrSecurity)
                                    {
                                        string str8 = obj4.Properties["SecurityDIR"].Value.ToString();
                                        if (path.EndsWith(str8, StringComparison.OrdinalIgnoreCase))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    string str9 = this.GetResourceValueInXml(obj2, resourceURI, null);
                    XmlDocument resourcexmldocument = new XmlDocument();
                    resourcexmldocument.LoadXml(str9.ToLower(CultureInfo.InvariantCulture));
                    XmlNodeList nodes = this.SearchXml(resourcexmldocument, str, resourceURI, path, hostName);
                    return this.IsItemContainer(nodes);
                }
                return false;
            }
        }

        private bool IsItemContainer(XmlNodeList nodes)
        {
            bool flag = false;
            if (((nodes.Count != 0) && (nodes[0].ChildNodes.Count != 0)) && !nodes[0].FirstChild.Name.Equals("#text", StringComparison.OrdinalIgnoreCase))
            {
                flag = true;
            }
            return flag;
        }

        private bool IsPathLocalMachine(string host)
        {
            bool flag = false;
            if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            {
                flag = true;
            }
            if (!flag && host.Equals(Dns.GetHostName(), StringComparison.OrdinalIgnoreCase))
            {
                flag = true;
            }
            if (!flag)
            {
                IPHostEntry hostEntry = Dns.GetHostEntry("localhost");
                if (host.Equals(hostEntry.HostName, StringComparison.OrdinalIgnoreCase))
                {
                    flag = true;
                }
                if (!flag)
                {
                    foreach (IPAddress address in hostEntry.AddressList)
                    {
                        if (address.ToString().Equals(host, StringComparison.OrdinalIgnoreCase))
                        {
                            flag = true;
                        }
                    }
                }
            }
            if (!flag)
            {
                foreach (IPAddress address2 in Dns.GetHostAddresses(Dns.GetHostName()))
                {
                    if (address2.ToString().Equals(host, StringComparison.OrdinalIgnoreCase))
                    {
                        flag = true;
                    }
                }
            }
            return flag;
        }

        private bool IsPKey(string value, string ResourceURI)
        {
            bool flag = false;
            if (ResourceURI.Contains("Listener"))
            {
                return this.CheckPkeysArray(null, value, PKeyListener);
            }
            if (ResourceURI.Contains("Plugin"))
            {
                return this.CheckPkeysArray(null, value, PKeyPlugin);
            }
            if (ResourceURI.Contains("Service/certmapping"))
            {
                flag = this.CheckPkeysArray(null, value, PKeyCertMapping);
            }
            return flag;
        }

        protected override bool IsValidPath(string path)
        {
            return this.CheckValidContainerOrPath(path);
        }

        private bool IsValueOfParamList(string name, string[] paramcontainer)
        {
            foreach (string str in paramcontainer)
            {
                if (str.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsWSManServiceRunning()
        {
            ServiceController controller = new ServiceController("WinRM");
            return ((controller != null) && controller.Status.Equals(ServiceControllerStatus.Running));
        }

        private bool ItemExistListenerOrClientCertificate(object sessionobj, string ResourceURI, string path, string parentListenerOrCert, string host)
        {
            XmlDocument xmlCerts = this.EnumerateResourceValue(sessionobj, ResourceURI);
            if (path.Equals(host + '\\' + parentListenerOrCert, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (xmlCerts == null)
            {
                return false;
            }
            Hashtable keyscache = null;
            Hashtable certcache = null;
            if (parentListenerOrCert.Equals("ClientCertificate", StringComparison.OrdinalIgnoreCase))
            {
                this.ProcessCertMappingObjects(xmlCerts, out certcache, out keyscache);
            }
            else
            {
                this.ProcessListernerObjects(xmlCerts, out certcache, out keyscache);
            }
            int startIndex = (host + '\\' + parentListenerOrCert).Length + 1;
            string str2 = path.Substring(startIndex);
            string key = null;
            startIndex = str2.IndexOf('\\');
            if (startIndex == -1)
            {
                key = str2;
            }
            else
            {
                key = str2.Substring(0, startIndex);
            }
            if (!certcache.Contains(key))
            {
                return false;
            }
            if (startIndex == -1)
            {
                return true;
            }
            PSObject obj2 = (PSObject) certcache[key];
            key = str2.Substring(startIndex + 1);
            if (key.IndexOf('\\') != -1)
            {
                return false;
            }
            return (obj2.Properties.Match(key).Count > 0);
        }

        protected override bool ItemExists(string path)
        {
            return this.CheckValidContainerOrPath(path);
        }

        protected override string MakePath(string parent, string child)
        {
            char ch = '\\';
            if (child.EndsWith(ch.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                child = child.Remove(child.LastIndexOf('\\'));
            }
            if ((parent.Equals("Listener", StringComparison.OrdinalIgnoreCase) && child.StartsWith(parent, StringComparison.OrdinalIgnoreCase)) && !child.StartsWith(parent + "_", StringComparison.OrdinalIgnoreCase))
            {
                child = child.Remove(0, parent.Length);
            }
            string path = string.Empty;
            string str2 = string.Empty;
            string childName = string.Empty;
            if (parent.Length != 0)
            {
                path = parent + '\\' + child;
            }
            else
            {
                path = child;
            }
            if (path.Length != 0)
            {
                str2 = path.Substring(path.LastIndexOf('\\') + 1);
                childName = this.GetChildName(path);
            }
            if (str2.Equals(childName, StringComparison.OrdinalIgnoreCase))
            {
                char ch2 = '\\';
                if (child.Contains(ch2.ToString()))
                {
                    child = child.Substring(0, child.LastIndexOf('\\'));
                    child = child + '\\' + childName;
                }
                else
                {
                    child = childName;
                }
            }
            string str4 = base.MakePath(parent, child);
            return this.GetCorrectCaseOfPath(str4);
        }

        protected override PSDriveInfo NewDrive(PSDriveInfo drive)
        {
            if (drive == null)
            {
                return null;
            }
            if (!string.IsNullOrEmpty(drive.Root))
            {
                this.AssertError(this.helper.GetResourceMsgFromResourcetext("NewDriveRootDoesNotExist"), false);
                return null;
            }
            return drive;
        }

        protected override void NewItem(string path, string itemTypeName, object newItemValue)
        {
            char ch = '\\';
            if (path.EndsWith(ch.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                path = path.Remove(path.LastIndexOf('\\'));
            }
            else
            {
                if (path.Length != 0)
                {
                    char ch2 = '\\';
                    if (path.Contains(ch2.ToString()))
                    {
                        string hostName = this.GetHostName(path);
                        if (string.IsNullOrEmpty(hostName))
                        {
                            throw new ArgumentException(this.helper.GetResourceMsgFromResourcetext("InvalidPath"));
                        }
                        if (this.IsPathLocalMachine(hostName) && !this.IsWSManServiceRunning())
                        {
                            WSManHelper.ThrowIfNotAdministrator();
                            this.StartWSManService((bool) base.Force);
                        }
                        string uri = this.NormalizePath(path, hostName);
                        lock (WSManHelper.AutoSession)
                        {
                            object obj2;
                            Dictionary<string, object> sessionObjCache = WSManHelper.GetSessionObjCache();
                            sessionObjCache.TryGetValue(hostName, out obj2);
                            string str3 = hostName + '\\';
                            if (path.Contains(str3 + "Plugin"))
                            {
                                this.NewItemPluginOrPluginChild(obj2, path, hostName, uri);
                            }
                            else if (path.EndsWith(str3 + "Listener", StringComparison.OrdinalIgnoreCase))
                            {
                                WSManProvidersListenerParameters dynamicParameters = base.DynamicParameters as WSManProvidersListenerParameters;
                                Hashtable inputParams = new Hashtable();
                                inputParams.Add("Address", dynamicParameters.Address);
                                inputParams.Add("Transport", dynamicParameters.Transport);
                                inputParams.Add("Enabled", dynamicParameters.Enabled);
                                if (dynamicParameters.HostName != null)
                                {
                                    inputParams.Add("Hostname", dynamicParameters.HostName);
                                }
                                if (dynamicParameters.URLPrefix != null)
                                {
                                    inputParams.Add("URLPrefix", dynamicParameters.URLPrefix);
                                }
                                if (dynamicParameters.IsPortSpecified)
                                {
                                    inputParams.Add("Port", dynamicParameters.Port);
                                }
                                if (dynamicParameters.CertificateThumbPrint != null)
                                {
                                    inputParams.Add("CertificateThumbPrint", dynamicParameters.CertificateThumbPrint);
                                }
                                this.NewItemContainerListenerOrCertMapping(obj2, path, uri, hostName, inputParams, "Listener", this.helper.GetResourceMsgFromResourcetext("NewItemShouldContinueListenerQuery"), this.helper.GetResourceMsgFromResourcetext("NewItemShouldContinueListenerCaption"));
                            }
                            else if (path.EndsWith(str3 + "ClientCertificate", StringComparison.OrdinalIgnoreCase))
                            {
                                WSManProviderClientCertificateParameters parameters2 = base.DynamicParameters as WSManProviderClientCertificateParameters;
                                sessionObjCache.TryGetValue(hostName, out obj2);
                                Hashtable hashtable2 = new Hashtable();
                                hashtable2.Add("Issuer", parameters2.Issuer);
                                hashtable2.Add("Subject", parameters2.Subject);
                                hashtable2.Add("Uri", parameters2.URI);
                                if (base.Credential.UserName != null)
                                {
                                    NetworkCredential networkCredential = base.Credential.GetNetworkCredential();
                                    hashtable2.Add("UserName", networkCredential.UserName);
                                    hashtable2.Add("Password", networkCredential.Password);
                                }
                                hashtable2.Add("Enabled", parameters2.Enabled);
                                this.NewItemContainerListenerOrCertMapping(obj2, path, uri, hostName, hashtable2, "ClientCertificate", this.helper.GetResourceMsgFromResourcetext("NewItemShouldContinueClientCertQuery"), this.helper.GetResourceMsgFromResourcetext("NewItemShouldContinueClientCertCaption"));
                            }
                            else
                            {
                                this.AssertError(this.helper.GetResourceMsgFromResourcetext("NewItemNotSupported"), false);
                            }
                        }
                        return;
                    }
                }
                this.NewItemCreateComputerConnection(path);
            }
        }

        private void NewItemContainerListenerOrCertMapping(object sessionobj, string path, string uri, string host, Hashtable InputParams, string ContainerListenerOrCertMapping, string ShouldContinueQuery, string ShouldContinueCaption)
        {
            if ((base.Force != 0) || base.ShouldContinue(ShouldContinueQuery, ShouldContinueCaption))
            {
                string resource = this.GetInputStringForCreate(uri, InputParams, host);
                this.CreateResourceValue(sessionobj, uri, resource, InputParams);
                XmlDocument xmlCerts = this.GetResourceValue(sessionobj, uri, InputParams);
                Hashtable certcache = null;
                Hashtable keyscache = null;
                if (ContainerListenerOrCertMapping.Equals("ClientCertificate"))
                {
                    this.ProcessCertMappingObjects(xmlCerts, out certcache, out keyscache);
                }
                else if (ContainerListenerOrCertMapping.Equals("Listener"))
                {
                    this.ProcessListernerObjects(xmlCerts, out certcache, out keyscache);
                }
                if ((certcache != null) && (certcache.Count > 0))
                {
                    foreach (DictionaryEntry entry in certcache)
                    {
                        base.WriteItemObject(this.GetItemPSObjectWithTypeName(entry.Key.ToString(), "Container", null, (string[]) keyscache[entry.Key], string.Empty, WsManElementObjectTypes.WSManConfigContainerElement, null), path + '\\' + entry.Key.ToString(), true);
                    }
                }
            }
        }

        private void NewItemCreateComputerConnection(string Name)
        {
            this.helper = new WSManHelper(this);
            WSManProviderNewItemComputerParameters dynamicParameters = base.DynamicParameters as WSManProviderNewItemComputerParameters;
            string parameterSetName = "ComputerName";
            if (dynamicParameters != null)
            {
                if (dynamicParameters.ConnectionURI != null)
                {
                    parameterSetName = "URI";
                }
                this.helper.CreateWsManConnection(parameterSetName, dynamicParameters.ConnectionURI, dynamicParameters.Port, Name, dynamicParameters.ApplicationName, (bool) dynamicParameters.UseSSL, dynamicParameters.Authentication, dynamicParameters.SessionOption, base.Credential, dynamicParameters.CertificateThumbprint);
                if (dynamicParameters.ConnectionURI != null)
                {
                    Name = dynamicParameters.ConnectionURI.OriginalString.Split(new string[] { string.Concat(new object[] { ":", dynamicParameters.Port, "/", dynamicParameters.ApplicationName }) }, StringSplitOptions.None)[0].Split(new string[] { "//" }, StringSplitOptions.None)[1].Trim();
                }
                base.WriteItemObject(this.GetItemPSObjectWithTypeName(Name, "Container", null, null, "ComputerLevel", WsManElementObjectTypes.WSManConfigContainerElement, null), "WSMan" + '\\' + Name, true);
            }
            else
            {
                dynamicParameters = new WSManProviderNewItemComputerParameters();
                this.helper.CreateWsManConnection(parameterSetName, dynamicParameters.ConnectionURI, dynamicParameters.Port, Name, dynamicParameters.ApplicationName, (bool) dynamicParameters.UseSSL, dynamicParameters.Authentication, dynamicParameters.SessionOption, base.Credential, dynamicParameters.CertificateThumbprint);
                base.WriteItemObject(this.GetItemPSObjectWithTypeName(Name, "Container", null, null, "ComputerLevel", WsManElementObjectTypes.WSManConfigContainerElement, null), "WSMan" + '\\' + Name, true);
            }
        }

        protected override object NewItemDynamicParameters(string path, string itemTypeName, object newItemValue)
        {
            if (path.Length == 0)
            {
                return new WSManProviderNewItemComputerParameters();
            }
            string str2 = this.GetHostName(path) + '\\';
            if (path.EndsWith(str2 + "Plugin", StringComparison.OrdinalIgnoreCase))
            {
                return new WSManProviderNewItemPluginParameters();
            }
            if (path.EndsWith('\\' + "InitializationParameters", StringComparison.OrdinalIgnoreCase))
            {
                return new WSManProviderInitializeParameters();
            }
            if (path.EndsWith('\\' + "Resources", StringComparison.OrdinalIgnoreCase))
            {
                return new WSManProviderNewItemResourceParameters();
            }
            if (path.EndsWith('\\' + "Security", StringComparison.OrdinalIgnoreCase))
            {
                return new WSManProviderNewItemSecurityParameters();
            }
            if (path.EndsWith(str2 + "Listener", StringComparison.OrdinalIgnoreCase))
            {
                return new WSManProvidersListenerParameters();
            }
            if (path.EndsWith(str2 + "ClientCertificate", StringComparison.OrdinalIgnoreCase))
            {
                return new WSManProviderClientCertificateParameters();
            }
            return null;
        }

        private void NewItemPluginOrPluginChild(object sessionobj, string path, string host, string uri)
        {
            PSObject objinputparam = new PSObject();
            string name = string.Empty;
            string resource = string.Empty;
            string str3 = host + '\\' + "Plugin";
            if (!path.Equals(str3) && (path.IndexOf('\\', str3.Length + 1) == -1))
            {
                name = path.Substring(path.LastIndexOf('\\') + 1);
                path = path.Remove(path.LastIndexOf('\\'));
            }
            if (path.EndsWith(str3, StringComparison.OrdinalIgnoreCase))
            {
                WSManProviderNewItemPluginParameters dynamicParameters = base.DynamicParameters as WSManProviderNewItemPluginParameters;
                if (dynamicParameters != null)
                {
                    if (string.IsNullOrEmpty(dynamicParameters.File))
                    {
                        objinputparam.Properties.Add(new PSNoteProperty("Name", dynamicParameters.Plugin));
                        objinputparam.Properties.Add(new PSNoteProperty("Filename", dynamicParameters.FileName));
                        objinputparam.Properties.Add(new PSNoteProperty("Resource", dynamicParameters.Resource));
                        objinputparam.Properties.Add(new PSNoteProperty("SDKVersion", dynamicParameters.SDKVersion));
                        objinputparam.Properties.Add(new PSNoteProperty("Capability", dynamicParameters.Capability));
                        if (dynamicParameters.RunAsCredential != null)
                        {
                            objinputparam.Properties.Add(new PSNoteProperty("RunAsUser", dynamicParameters.RunAsCredential.UserName));
                            objinputparam.Properties.Add(new PSNoteProperty("RunAsPassword", dynamicParameters.RunAsCredential.Password));
                        }
                        if (dynamicParameters.AutoRestart != 0)
                        {
                            objinputparam.Properties.Add(new PSNoteProperty("AutoRestart", dynamicParameters.AutoRestart));
                        }
                        if (dynamicParameters.ProcessIdleTimeoutSec.HasValue)
                        {
                            objinputparam.Properties.Add(new PSNoteProperty("ProcessIdleTimeoutSec", dynamicParameters.ProcessIdleTimeoutSec.Value));
                        }
                        if (dynamicParameters.UseSharedProcess != 0)
                        {
                            objinputparam.Properties.Add(new PSNoteProperty("UseSharedProcess", dynamicParameters.UseSharedProcess));
                        }
                        if (dynamicParameters.XMLRenderingType != null)
                        {
                            objinputparam.Properties.Add(new PSNoteProperty("XmlRenderingType", dynamicParameters.XMLRenderingType));
                        }
                        else
                        {
                            objinputparam.Properties.Add(new PSNoteProperty("XmlRenderingType", "Text"));
                        }
                        resource = this.ConstructPluginXml(objinputparam, uri, host, "New", null, null, null);
                        name = dynamicParameters.Plugin;
                    }
                    else
                    {
                        resource = this.ReadFile(dynamicParameters.File);
                    }
                }
                else
                {
                    ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(this.helper.GetResourceMsgFromResourcetext("NewItemNotSupported")), "InvalidOperationException", ErrorCategory.InvalidOperation, null);
                    base.WriteError(errorRecord);
                    return;
                }
                string resourceURI = uri + "?Name=" + name;
                this.CreateResourceValue(sessionobj, resourceURI, resource, null);
                base.WriteItemObject(this.GetItemPSObjectWithTypeName(name, "Container", null, new string[] { "Name=" + name }, string.Empty, WsManElementObjectTypes.WSManConfigContainerElement, null), path + '\\' + name, true);
            }
            else
            {
                string str5 = string.Empty;
                string paramName = string.Empty;
                string[] keys = null;
                int startIndex = (path.LastIndexOf(str3 + '\\', StringComparison.OrdinalIgnoreCase) + str3.Length) + 1;
                if (path.IndexOf('\\', startIndex) != -1)
                {
                    str5 = path.Substring(startIndex, path.IndexOf('\\', startIndex) - startIndex);
                }
                else
                {
                    str5 = path.Substring(startIndex);
                }
                string str7 = uri + "?Name=" + str5;
                XmlDocument xmldoc = this.GetResourceValue(sessionobj, str7, null);
                ArrayList arrSecurity = null;
                PSObject obj3 = this.ProcessPluginConfigurationLevel(xmldoc, false);
                ArrayList resources = this.ProcessPluginResourceLevel(xmldoc, out arrSecurity);
                ArrayList initParams = this.ProcessPluginInitParamLevel(xmldoc);
                str3 = string.Concat(new object[] { str3, '\\', str5, '\\' });
                if (path.Contains(str3 + "Resources"))
                {
                    str3 = str3 + "Resources";
                    if (path.EndsWith(str3, StringComparison.OrdinalIgnoreCase))
                    {
                        WSManProviderNewItemResourceParameters parameters2 = base.DynamicParameters as WSManProviderNewItemResourceParameters;
                        if (parameters2 != null)
                        {
                            objinputparam.Properties.Add(new PSNoteProperty("Resource", parameters2.ResourceUri));
                            objinputparam.Properties.Add(new PSNoteProperty("Capability", parameters2.Capability));
                            resource = this.ConstructResourceXml(objinputparam, null, null);
                            XmlDocument document2 = new XmlDocument();
                            document2.LoadXml(resource);
                            ArrayList list4 = null;
                            ArrayList list5 = this.ProcessPluginResourceLevel(document2, out list4);
                            paramName = ((PSObject) list5[0]).Properties["ResourceDir"].Value.ToString();
                            keys = new string[] { "Uri=" + ((PSObject) list5[0]).Properties["ResourceURI"].Value.ToString() };
                            if (resources != null)
                            {
                                resources.Add(list5[0]);
                            }
                            else
                            {
                                resources = list5;
                            }
                        }
                    }
                    int index = path.IndexOf('\\', str3.Length);
                    string uniqueResourceID = string.Empty;
                    if (index != -1)
                    {
                        uniqueResourceID = path.Substring(str3.Length + 1, path.IndexOf('\\', str3.Length + 1) - (str3.Length + 1));
                    }
                    str3 = str3 + '\\' + uniqueResourceID;
                    if (path.EndsWith(str3 + '\\' + "Security", StringComparison.OrdinalIgnoreCase))
                    {
                        if (base.Force == 0)
                        {
                            string resourceMsgFromResourcetext = this.helper.GetResourceMsgFromResourcetext("ShouldContinueSecurityQuery");
                            resourceMsgFromResourcetext = string.Format(CultureInfo.CurrentCulture, resourceMsgFromResourcetext, new object[] { str5 });
                            if (!base.ShouldContinue(resourceMsgFromResourcetext, this.helper.GetResourceMsgFromResourcetext("ShouldContinueSecurityCaption")))
                            {
                                return;
                            }
                        }
                        PSObject itemValue = this.GetItemValue(str3);
                        if ((itemValue == null) || (itemValue.Properties["ResourceUri"] == null))
                        {
                            string errorMessage = this.helper.FormatResourceMsgFromResourcetext("ResourceURIMissingInResourceDir", new object[] { "ResourceUri", str3 });
                            this.AssertError(errorMessage, false);
                            return;
                        }
                        WSManProviderNewItemSecurityParameters parameters3 = base.DynamicParameters as WSManProviderNewItemSecurityParameters;
                        objinputparam.Properties.Add(new PSNoteProperty("Uri", itemValue.Properties["ResourceUri"].Value));
                        objinputparam.Properties.Add(new PSNoteProperty("Sddl", parameters3.Sddl));
                        resource = this.ConstructSecurityXml(objinputparam, null, string.Empty);
                        XmlDocument xmlSecurity = new XmlDocument();
                        xmlSecurity.LoadXml(resource);
                        ArrayList list6 = new ArrayList();
                        list6 = this.ProcessPluginSecurityLevel(list6, xmlSecurity, uniqueResourceID, itemValue.Properties["ResourceUri"].Value.ToString());
                        paramName = ((PSObject) list6[0]).Properties["SecurityDIR"].Value.ToString();
                        keys = new string[] { "Uri=" + ((PSObject) list6[0]).Properties["Uri"].Value.ToString() };
                        if (arrSecurity != null)
                        {
                            arrSecurity.Add(list6[0]);
                        }
                        else
                        {
                            arrSecurity = list6;
                        }
                    }
                }
                if (path.EndsWith(str3 + "InitializationParameters", StringComparison.OrdinalIgnoreCase))
                {
                    WSManProviderInitializeParameters parameters4 = base.DynamicParameters as WSManProviderInitializeParameters;
                    objinputparam.Properties.Add(new PSNoteProperty(parameters4.ParamName, parameters4.ParamValue));
                    resource = this.ConstructInitParamsXml(objinputparam, null);
                    XmlDocument document4 = new XmlDocument();
                    document4.LoadXml(resource);
                    ArrayList list7 = this.ProcessPluginInitParamLevel(document4);
                    paramName = parameters4.ParamName;
                    if (initParams != null)
                    {
                        initParams.Add(list7[0]);
                    }
                    else
                    {
                        initParams = this.ProcessPluginInitParamLevel(document4);
                    }
                }
                resource = this.ConstructPluginXml(obj3, uri, host, "Set", resources, arrSecurity, initParams);
                try
                {
                    ((IWSManSession) sessionobj).Put(uri + "?Name=" + str5, resource.ToString(), 0);
                    if (path.EndsWith(str3 + "InitializationParameters", StringComparison.OrdinalIgnoreCase))
                    {
                        base.WriteItemObject(this.GetItemPSObjectWithTypeName(objinputparam.Properties[paramName].Name, objinputparam.Properties[paramName].TypeNameOfValue, objinputparam.Properties[paramName].Value, null, "InitParams", WsManElementObjectTypes.WSManConfigLeafElement, null), path + '\\' + objinputparam.Properties[paramName].Name, false);
                    }
                    else
                    {
                        base.WriteItemObject(this.GetItemPSObjectWithTypeName(paramName, "Container", null, keys, string.Empty, WsManElementObjectTypes.WSManConfigContainerElement, null), path + '\\' + paramName, true);
                    }
                }
                finally
                {
                    if (!string.IsNullOrEmpty(((IWSManSession) sessionobj).Error))
                    {
                        this.AssertError(((IWSManSession) sessionobj).Error, true);
                    }
                }
            }
        }

        private string NormalizePath(string path, string host)
        {
            string str = string.Empty;
            if (!path.StartsWith(host, StringComparison.OrdinalIgnoreCase))
            {
                return str;
            }
            char ch = '\\';
            if (path.EndsWith(ch.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                path = path.TrimEnd(new char[] { '\\' });
            }
            if (path.Equals(host, StringComparison.OrdinalIgnoreCase))
            {
                return WinrmRootName[0].ToString();
            }
            str = path.Substring(host.Length).Replace('\\', '/');
            string str2 = host + '\\';
            if (path.StartsWith(str2 + "ClientCertificate", StringComparison.OrdinalIgnoreCase))
            {
                return (WinrmRootName[0].ToString() + '/' + "Service/certmapping");
            }
            if (path.StartsWith(str2 + "Plugin", StringComparison.OrdinalIgnoreCase))
            {
                return (WinrmRootName[0].ToString() + '/' + "Plugin");
            }
            if (path.StartsWith(str2 + "Shell", StringComparison.OrdinalIgnoreCase))
            {
                return (WinrmRootName[0].ToString() + '/' + "Winrs");
            }
            if (path.StartsWith(str2 + "Listener", StringComparison.OrdinalIgnoreCase))
            {
                return (WinrmRootName[0].ToString() + '/' + "Listener");
            }
            if ((!path.Equals(str2 + "Service", StringComparison.OrdinalIgnoreCase) && !path.Equals(str2 + "Client", StringComparison.OrdinalIgnoreCase)) && (!path.EndsWith("DefaultPorts", StringComparison.OrdinalIgnoreCase) && !path.EndsWith("Auth", StringComparison.OrdinalIgnoreCase)))
            {
                int startIndex = str.LastIndexOf('/');
                if (startIndex != -1)
                {
                    str = str.Remove(startIndex);
                }
            }
            return (WinrmRootName[0].ToString() + str);
        }

        private void ProcessCertMappingObjects(XmlDocument xmlCerts, out Hashtable Certcache, out Hashtable Keyscache)
        {
            Hashtable hashtable = new Hashtable();
            Hashtable hashtable2 = new Hashtable();
            XmlNodeList elementsByTagName = xmlCerts.GetElementsByTagName("cfg:CertMapping");
            if (elementsByTagName == null)
            {
                Certcache = null;
                Keyscache = null;
            }
            else
            {
                foreach (XmlNode node in elementsByTagName)
                {
                    Hashtable inputAttributes = new Hashtable();
                    PSObject obj2 = new PSObject();
                    string[] keys = null;
                    string itemName = string.Empty;
                    foreach (XmlNode node2 in node.ChildNodes)
                    {
                        inputAttributes.Add(node2.LocalName, node2.InnerText);
                        obj2.Properties.Add(new PSNoteProperty(node2.LocalName, node2.InnerText));
                    }
                    this.GenerateObjectNameAndKeys(inputAttributes, "Service/certmapping", "ClientCertificate", out itemName, out keys);
                    hashtable.Add(itemName, obj2);
                    hashtable2.Add(itemName, keys);
                }
                Certcache = hashtable;
                Keyscache = hashtable2;
            }
        }

        private void ProcessListernerObjects(XmlDocument xmlListeners, out Hashtable listenercache, out Hashtable Keyscache)
        {
            Hashtable hashtable = new Hashtable();
            Hashtable hashtable2 = new Hashtable();
            XmlNodeList elementsByTagName = xmlListeners.GetElementsByTagName("cfg:Listener");
            if (elementsByTagName == null)
            {
                listenercache = null;
                Keyscache = null;
            }
            else
            {
                foreach (XmlNode node in elementsByTagName)
                {
                    Hashtable inputAttributes = new Hashtable();
                    PSObject obj2 = new PSObject();
                    string[] keys = null;
                    string itemName = string.Empty;
                    foreach (XmlNode node2 in node.ChildNodes)
                    {
                        if (node2.LocalName.Equals("ListeningOn"))
                        {
                            string name = node2.LocalName + "_" + Math.Abs(node2.InnerText.GetHashCode());
                            obj2.Properties.Add(new PSNoteProperty(name, node2.InnerText));
                            inputAttributes.Add(name, node2.InnerText);
                        }
                        else
                        {
                            inputAttributes.Add(node2.LocalName, node2.InnerText);
                            obj2.Properties.Add(new PSNoteProperty(node2.LocalName, node2.InnerText));
                        }
                    }
                    this.GenerateObjectNameAndKeys(inputAttributes, "Listener", "Listener", out itemName, out keys);
                    hashtable.Add(itemName, obj2);
                    hashtable2.Add(itemName, keys);
                }
                listenercache = hashtable;
                Keyscache = hashtable2;
            }
        }

        private PSObject ProcessPluginConfigurationLevel(XmlDocument xmldoc, bool setRunasPasswordAsSecureString = false)
        {
            PSObject obj2 = null;
            if (xmldoc != null)
            {
                XmlNodeList elementsByTagName = xmldoc.GetElementsByTagName("PlugInConfiguration");
                if (elementsByTagName.Count > 0)
                {
                    obj2 = new PSObject();
                    XmlAttributeCollection attributes = elementsByTagName.Item(0).Attributes;
                    XmlNode namedItem = attributes.GetNamedItem("RunAsUser");
                    bool flag = (namedItem != null) && !string.IsNullOrEmpty(namedItem.Value);
                    for (int i = 0; i <= (attributes.Count - 1); i++)
                    {
                        if ((string.Equals(attributes[i].LocalName, "RunAsPassword", StringComparison.OrdinalIgnoreCase) && flag) && setRunasPasswordAsSecureString)
                        {
                            obj2.Properties.Add(new PSNoteProperty(attributes[i].LocalName, new SecureString()));
                        }
                        else
                        {
                            obj2.Properties.Add(new PSNoteProperty(attributes[i].LocalName, attributes[i].Value));
                        }
                    }
                }
                if (obj2 != null)
                {
                    obj2.Properties.Add(new PSNoteProperty("InitializationParameters", "Container"));
                    obj2.Properties.Add(new PSNoteProperty("Resources", "Container"));
                    obj2.Properties.Add(new PSNoteProperty("Quotas", "Container"));
                }
            }
            return obj2;
        }

        private ArrayList ProcessPluginInitParamLevel(XmlDocument xmldoc)
        {
            ArrayList list = null;
            if (xmldoc != null)
            {
                XmlNodeList elementsByTagName = xmldoc.GetElementsByTagName("Param");
                if (elementsByTagName.Count <= 0)
                {
                    return list;
                }
                list = new ArrayList();
                foreach (XmlElement element in elementsByTagName)
                {
                    PSObject obj2 = new PSObject();
                    XmlAttributeCollection attributes = element.Attributes;
                    string name = string.Empty;
                    string str2 = string.Empty;
                    for (int i = 0; i <= (attributes.Count - 1); i++)
                    {
                        if (attributes[i].LocalName.Equals("Name", StringComparison.OrdinalIgnoreCase))
                        {
                            name = attributes[i].Value;
                        }
                        if (attributes[i].LocalName.Equals("Value", StringComparison.OrdinalIgnoreCase))
                        {
                            str2 = SecurityElement.Escape(attributes[i].Value);
                        }
                    }
                    obj2.Properties.Add(new PSNoteProperty(name, str2));
                    list.Add(obj2);
                }
            }
            return list;
        }

        private ArrayList ProcessPluginResourceLevel(XmlDocument xmldoc, out ArrayList arrSecurity)
        {
            ArrayList list = null;
            ArrayList list2 = null;
            if (xmldoc != null)
            {
                XmlNodeList elementsByTagName = xmldoc.GetElementsByTagName("Resource");
                if (elementsByTagName.Count > 0)
                {
                    list = new ArrayList();
                    list2 = new ArrayList();
                    foreach (XmlElement element in elementsByTagName)
                    {
                        PSObject obj2 = new PSObject();
                        string str = string.Empty;
                        XmlAttributeCollection attributes = element.Attributes;
                        bool flag = false;
                        bool flag2 = false;
                        string parentResourceUri = string.Empty;
                        for (int i = 0; i <= (attributes.Count - 1); i++)
                        {
                            if (attributes[i].LocalName.Equals("ResourceUri", StringComparison.OrdinalIgnoreCase))
                            {
                                parentResourceUri = attributes[i].Value;
                                str = "Resource_" + Convert.ToString(Math.Abs(attributes[i].Value.GetHashCode()), CultureInfo.InvariantCulture);
                                obj2.Properties.Add(new PSNoteProperty("ResourceDir", str));
                            }
                            if (attributes[i].LocalName.Equals("ExactMatch", StringComparison.OrdinalIgnoreCase))
                            {
                                obj2.Properties.Add(new PSNoteProperty(attributes[i].LocalName, attributes[i].Value));
                                flag = true;
                            }
                            else if (attributes[i].LocalName.Equals("SupportsOptions", StringComparison.OrdinalIgnoreCase))
                            {
                                obj2.Properties.Add(new PSNoteProperty(attributes[i].LocalName, attributes[i].Value));
                                flag2 = true;
                            }
                            else
                            {
                                obj2.Properties.Add(new PSNoteProperty(attributes[i].LocalName, attributes[i].Value));
                            }
                        }
                        if (!flag)
                        {
                            obj2.Properties.Add(new PSNoteProperty("ExactMatch", false));
                        }
                        if (!flag2)
                        {
                            obj2.Properties.Add(new PSNoteProperty("SupportsOptions", false));
                        }
                        XmlDocument xmlSecurity = new XmlDocument();
                        xmlSecurity.LoadXml("<Capabilities>" + element.InnerXml + "</Capabilities>");
                        XmlNodeList list4 = xmlSecurity.GetElementsByTagName("Capability");
                        object[] objArray = null;
                        if (list4.Count > 0)
                        {
                            objArray = new object[list4.Count];
                            for (int j = 0; j < list4.Count; j++)
                            {
                                objArray.SetValue(list4[j].Attributes["Type"].Value, j);
                            }
                        }
                        obj2.Properties.Add(new PSNoteProperty("Capability", objArray));
                        obj2.Properties.Add(new PSNoteProperty("Security", "Container"));
                        list2 = this.ProcessPluginSecurityLevel(list2, xmlSecurity, str, parentResourceUri);
                        list.Add(obj2);
                    }
                }
            }
            arrSecurity = list2;
            return list;
        }

        private ArrayList ProcessPluginSecurityLevel(ArrayList arrSecurity, XmlDocument xmlSecurity, string UniqueResourceID, string ParentResourceUri)
        {
            if (xmlSecurity != null)
            {
                XmlNodeList elementsByTagName = xmlSecurity.GetElementsByTagName("Security");
                if (elementsByTagName.Count <= 0)
                {
                    return arrSecurity;
                }
                foreach (XmlElement element in elementsByTagName)
                {
                    bool flag = false;
                    PSObject obj2 = new PSObject();
                    XmlAttributeCollection attributes = element.Attributes;
                    for (int i = 0; i <= (attributes.Count - 1); i++)
                    {
                        if (attributes[i].LocalName.Equals("Uri", StringComparison.OrdinalIgnoreCase))
                        {
                            obj2.Properties.Add(new PSNoteProperty("SecurityDIR", "Security_" + Math.Abs(UniqueResourceID.GetHashCode())));
                        }
                        if (attributes[i].LocalName.Equals("ExactMatch", StringComparison.OrdinalIgnoreCase))
                        {
                            obj2.Properties.Add(new PSNoteProperty(attributes[i].LocalName, attributes[i].Value));
                            flag = true;
                        }
                        else
                        {
                            obj2.Properties.Add(new PSNoteProperty(attributes[i].LocalName, attributes[i].Value));
                        }
                    }
                    if (!flag)
                    {
                        obj2.Properties.Add(new PSNoteProperty("ExactMatch", false));
                    }
                    obj2.Properties.Add(new PSNoteProperty("ResourceDir", UniqueResourceID));
                    obj2.Properties.Add(new PSNoteProperty("ParentResourceUri", ParentResourceUri));
                    arrSecurity.Add(obj2);
                }
            }
            return arrSecurity;
        }

        private void PutResourceValue(object sessionobj, string ResourceURI, Hashtable value, string host)
        {
            XmlDocument document = null;
            try
            {
                document = this.GetResourceValue(sessionobj, ResourceURI, value);
                if (document != null)
                {
                    bool flag = false;
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(document.NameTable);
                    string uri = this.SetSchemaPath(ResourceURI);
                    nsmgr.AddNamespace("cfg", uri);
                    string xpath = this.SetXPathString(ResourceURI);
                    XmlNodeList list = document.SelectNodes(xpath, nsmgr);
                    if ((list.Count == 1) && (list != null))
                    {
                        XmlNode node = list.Item(0);
                        if (node.HasChildNodes)
                        {
                            for (int i = 0; i < node.ChildNodes.Count; i++)
                            {
                                if ((node.ChildNodes[i].ChildNodes.Count == 0) || node.ChildNodes[i].FirstChild.Name.Equals("#text", StringComparison.OrdinalIgnoreCase))
                                {
                                    foreach (string str3 in value.Keys)
                                    {
                                        if (!this.IsPKey(str3, ResourceURI) && node.ChildNodes[i].LocalName.Equals(str3, StringComparison.OrdinalIgnoreCase))
                                        {
                                            node.ChildNodes[i].InnerText = value[str3].ToString();
                                            flag = true;
                                        }
                                    }
                                }
                            }
                            if (flag)
                            {
                                ResourceURI = this.GetURIWithFilter(ResourceURI, value);
                                ((IWSManSession) sessionobj).Put(ResourceURI, node.OuterXml.ToString(), 0);
                            }
                            else
                            {
                                this.AssertError(this.helper.GetResourceMsgFromResourcetext("ItemDoesNotExist"), false);
                            }
                        }
                    }
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(((IWSManSession) sessionobj).Error))
                {
                    this.AssertError(((IWSManSession) sessionobj).Error, true);
                }
            }
        }

        private string ReadFile(string path)
        {
            string str = string.Empty;
            try
            {
                str = File.ReadAllText(base.SessionState.Path.GetUnresolvedProviderPathFromPSPath(path), Encoding.UTF8);
            }
            catch (ArgumentNullException exception)
            {
                ErrorRecord errorRecord = new ErrorRecord(exception, "ArgumentNullException", ErrorCategory.InvalidOperation, null);
                base.WriteError(errorRecord);
            }
            catch (UnauthorizedAccessException exception2)
            {
                ErrorRecord record2 = new ErrorRecord(exception2, "UnauthorizedAccessException", ErrorCategory.InvalidOperation, null);
                base.WriteError(record2);
            }
            catch (NotSupportedException exception3)
            {
                ErrorRecord record3 = new ErrorRecord(exception3, "NotSupportedException", ErrorCategory.InvalidOperation, null);
                base.WriteError(record3);
            }
            catch (FileNotFoundException exception4)
            {
                ErrorRecord record4 = new ErrorRecord(exception4, "FileNotFoundException", ErrorCategory.InvalidOperation, null);
                base.WriteError(record4);
            }
            catch (DirectoryNotFoundException exception5)
            {
                ErrorRecord record5 = new ErrorRecord(exception5, "DirectoryNotFoundException", ErrorCategory.InvalidOperation, null);
                base.WriteError(record5);
            }
            catch (SecurityException exception6)
            {
                ErrorRecord record6 = new ErrorRecord(exception6, "SecurityException", ErrorCategory.InvalidOperation, null);
                base.WriteError(record6);
            }
            return str;
        }

        protected override PSDriveInfo RemoveDrive(PSDriveInfo drive)
        {
            WSManHelper.ReleaseSessions();
            return drive;
        }

        protected override void RemoveItem(string path, bool recurse)
        {
            bool flag = true;
            if (path.Length == 0)
            {
                this.AssertError(this.helper.GetResourceMsgFromResourcetext("RemoveItemNotSupported"), false);
            }
            else
            {
                string computer = string.Empty;
                char ch = '\\';
                if (path.Contains(ch.ToString()))
                {
                    computer = path.Substring(path.LastIndexOf('\\') + 1);
                }
                else
                {
                    computer = path;
                }
                if (computer.Equals(path, StringComparison.OrdinalIgnoreCase))
                {
                    if (computer.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                    {
                        this.AssertError(this.helper.GetResourceMsgFromResourcetext("LocalHost"), false);
                    }
                    this.helper.RemoveFromDictionary(computer);
                }
                else
                {
                    path = path.Substring(0, path.LastIndexOf(computer, StringComparison.OrdinalIgnoreCase));
                    string hostName = this.GetHostName(path);
                    string resourceURI = this.NormalizePath(path, hostName);
                    if (this.IsPathLocalMachine(hostName) && !this.IsWSManServiceRunning())
                    {
                        WSManHelper.ThrowIfNotAdministrator();
                        this.StartWSManService((bool) base.Force);
                    }
                    lock (WSManHelper.AutoSession)
                    {
                        object obj2;
                        WSManHelper.GetSessionObjCache().TryGetValue(hostName, out obj2);
                        char ch2 = '\\';
                        if (path.EndsWith(ch2.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            path = path.Remove(path.LastIndexOf('\\'));
                        }
                        string resource = string.Empty;
                        string str5 = string.Empty;
                        str5 = hostName + '\\';
                        if (path.Contains(str5 + "Plugin"))
                        {
                            if (path.EndsWith(str5 + "Plugin", StringComparison.OrdinalIgnoreCase))
                            {
                                string str6 = resourceURI + "?Name=" + computer;
                                this.DeleteResourceValue(obj2, str6, null, recurse);
                                goto Label_0476;
                            }
                            str5 = str5 + "Plugin";
                            int startIndex = 0;
                            string str7 = null;
                            startIndex = (path.LastIndexOf(str5 + '\\', StringComparison.OrdinalIgnoreCase) + str5.Length) + 1;
                            if (path.IndexOf('\\', startIndex) != -1)
                            {
                                str7 = path.Substring(startIndex, path.IndexOf('\\', startIndex) - startIndex);
                            }
                            else
                            {
                                str7 = path.Substring(startIndex);
                            }
                            string str8 = resourceURI + "?Name=" + str7;
                            XmlDocument xmldoc = this.GetResourceValue(obj2, str8, null);
                            PSObject objinputparam = this.ProcessPluginConfigurationLevel(xmldoc, false);
                            ArrayList arrSecurity = null;
                            ArrayList resourceArray = this.ProcessPluginResourceLevel(xmldoc, out arrSecurity);
                            ArrayList list3 = this.ProcessPluginInitParamLevel(xmldoc);
                            str5 = string.Concat(new object[] { str5, '\\', str7, '\\' });
                            if (path.Contains(str5 + "Resources"))
                            {
                                if (path.EndsWith(str5 + "Resources", StringComparison.OrdinalIgnoreCase))
                                {
                                    flag = false;
                                    resourceArray = this.RemoveItemfromResourceArray(resourceArray, computer, "", "ResourceDir");
                                }
                                if (flag)
                                {
                                    str5 = str5 + "Resources" + '\\';
                                    int index = path.IndexOf('\\', str5.Length);
                                    string str9 = string.Empty;
                                    if (index == -1)
                                    {
                                        str9 = path.Substring(str5.Length);
                                    }
                                    else
                                    {
                                        str9 = path.Substring(str5.Length, path.IndexOf('\\', str5.Length) - str5.Length);
                                    }
                                    str5 = str5 + str9 + '\\';
                                    if (path.EndsWith(str5 + "Security", StringComparison.OrdinalIgnoreCase))
                                    {
                                        flag = false;
                                        arrSecurity = this.RemoveItemfromResourceArray(arrSecurity, computer, "", "SecurityDIR");
                                    }
                                }
                            }
                            else if (path.EndsWith(str5 + "InitializationParameters", StringComparison.OrdinalIgnoreCase))
                            {
                                flag = false;
                                list3 = this.RemoveItemfromResourceArray(list3, computer, "InitParams", "");
                            }
                            if (flag)
                            {
                                this.AssertError(this.helper.GetResourceMsgFromResourcetext("RemoveItemNotSupported"), false);
                                goto Label_0476;
                            }
                            resource = this.ConstructPluginXml(objinputparam, resourceURI, hostName, "Set", resourceArray, arrSecurity, list3);
                            try
                            {
                                ((IWSManSession) obj2).Put(resourceURI + "?Name=" + str7, resource, 0);
                            }
                            finally
                            {
                                if (!string.IsNullOrEmpty(((IWSManSession) obj2).Error))
                                {
                                    this.AssertError(((IWSManSession) obj2).Error, true);
                                }
                            }
                        }
                        else if (path.EndsWith(str5 + "Listener", StringComparison.OrdinalIgnoreCase))
                        {
                            flag = false;
                            this.RemoveListenerOrCertMapping(obj2, resourceURI, computer, PKeyListener, true);
                        }
                        else if (path.EndsWith(str5 + "ClientCertificate", StringComparison.OrdinalIgnoreCase))
                        {
                            flag = false;
                            this.RemoveListenerOrCertMapping(obj2, resourceURI, computer, PKeyCertMapping, false);
                        }
                        if (flag)
                        {
                            this.AssertError(this.helper.GetResourceMsgFromResourcetext("RemoveItemNotSupported"), false);
                        }
                    Label_0476:;
                    }
                }
            }
        }

        private ArrayList RemoveItemfromResourceArray(ArrayList resourceArray, string ChildName, string type, string property)
        {
            if (resourceArray != null)
            {
                bool flag = false;
                int index = 0;
                foreach (PSObject obj2 in resourceArray)
                {
                    if (type.Equals("InitParams"))
                    {
                        if (obj2.Properties.Match(ChildName).Count <= 0)
                        {
                            goto Label_0068;
                        }
                        flag = true;
                        break;
                    }
                    if (obj2.Properties[property].Value.ToString().Equals(ChildName, StringComparison.OrdinalIgnoreCase))
                    {
                        flag = true;
                        break;
                    }
                Label_0068:
                    index++;
                }
                if (flag)
                {
                    resourceArray.RemoveAt(index);
                }
            }
            return resourceArray;
        }

        private void RemoveListenerOrCertMapping(object sessionobj, string WsManUri, string childname, string[] primarykeys, bool IsListener)
        {
            XmlDocument xmlCerts = this.EnumerateResourceValue(sessionobj, WsManUri);
            if (xmlCerts != null)
            {
                Hashtable hashtable;
                Hashtable hashtable2;
                if (!IsListener)
                {
                    this.ProcessCertMappingObjects(xmlCerts, out hashtable2, out hashtable);
                }
                else
                {
                    this.ProcessListernerObjects(xmlCerts, out hashtable2, out hashtable);
                }
                if (hashtable.Contains(childname))
                {
                    PSObject obj2 = (PSObject) hashtable2[childname];
                    Hashtable cmdlinevalues = new Hashtable();
                    foreach (string str in primarykeys)
                    {
                        cmdlinevalues.Add(str, obj2.Properties[str].Value);
                    }
                    this.DeleteResourceValue(sessionobj, WsManUri, cmdlinevalues, false);
                }
            }
        }

        private XmlNodeList SearchXml(XmlDocument resourcexmldocument, string searchitem, string ResourceURI, string path, string host)
        {
            XmlNodeList list = null;
            try
            {
                string xpath = string.Empty;
                if (ResourceURI.EndsWith("Winrs", StringComparison.OrdinalIgnoreCase) && path.Equals(host + '\\' + "Shell", StringComparison.OrdinalIgnoreCase))
                {
                    searchitem = "Winrs".ToLower(CultureInfo.InvariantCulture);
                }
                if (ResourceURI.EndsWith("Config", StringComparison.OrdinalIgnoreCase) || !ResourceURI.EndsWith(searchitem, StringComparison.OrdinalIgnoreCase))
                {
                    xpath = "/*/*[local-name()=\"" + searchitem.ToLower(CultureInfo.InvariantCulture) + "\"]";
                }
                else
                {
                    xpath = "/*[local-name()=\"" + searchitem.ToLower(CultureInfo.InvariantCulture) + "\"]";
                }
                list = resourcexmldocument.SelectNodes(xpath);
            }
            catch (XPathException exception)
            {
                ErrorRecord errorRecord = new ErrorRecord(exception, "XPathException", ErrorCategory.InvalidArgument, null);
                base.WriteError(errorRecord);
            }
            return list;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        protected override void SetItem(string path, object value)
        {
            if (value == null)
            {
                throw new ArgumentException(this.helper.GetResourceMsgFromResourcetext("value"));
            }
            string str = string.Empty;
            if ((path.Length == 0) && string.IsNullOrEmpty(str))
            {
                this.AssertError(this.helper.GetResourceMsgFromResourcetext("SetItemNotSupported"), false);
            }
            else
            {
                char ch = '\\';
                if (path.Contains(ch.ToString()))
                {
                    str = path.Substring(path.LastIndexOf('\\') + 1);
                }
                else
                {
                    str = path;
                }
                if (str.Equals(path, StringComparison.OrdinalIgnoreCase))
                {
                    this.AssertError(this.helper.GetResourceMsgFromResourcetext("SetItemNotSupported"), false);
                }
                else
                {
                    if (!this.clearItemIsCalled)
                    {
                        value = this.ValidateAndGetUserObject(str, value);
                        if (value == null)
                        {
                            return;
                        }
                    }
                    else if (string.Equals(str, "RunAsPassword", StringComparison.OrdinalIgnoreCase))
                    {
                        this.AssertError(this.helper.GetResourceMsgFromResourcetext("ClearItemOnRunAsPassword"), false);
                        return;
                    }
                    string verboseDescription = string.Format(CultureInfo.CurrentUICulture, this.helper.GetResourceMsgFromResourcetext("SetItemWhatIfAndConfirmText"), new object[] { path, value });
                    if (base.ShouldProcess(verboseDescription, "", ""))
                    {
                        path = path.Substring(0, path.LastIndexOf(str, StringComparison.OrdinalIgnoreCase));
                        string hostName = this.GetHostName(path);
                        string resourceURI = this.NormalizePath(path, hostName);
                        if (this.IsPathLocalMachine(hostName) && !this.IsWSManServiceRunning())
                        {
                            WSManHelper.ThrowIfNotAdministrator();
                            this.StartWSManService((bool) base.Force);
                        }
                        bool flag = false;
                        lock (WSManHelper.AutoSession)
                        {
                            object obj2;
                            Dictionary<string, object> sessionObjCache = WSManHelper.GetSessionObjCache();
                            sessionObjCache.TryGetValue(hostName, out obj2);
                            List<string> list = new List<string>();
                            char ch2 = '\\';
                            if (path.EndsWith(ch2.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                path = path.Remove(path.LastIndexOf('\\'));
                            }
                            string str5 = hostName + '\\';
                            if (path.Contains(str5 + "Listener"))
                            {
                                this.SetItemListenerOrClientCertificate(obj2, resourceURI, PKeyListener, str, value, path, "Listener", hostName);
                            }
                            else if (path.Contains(str5 + "ClientCertificate"))
                            {
                                this.SetItemListenerOrClientCertificate(obj2, resourceURI, PKeyCertMapping, str, value, path, "ClientCertificate", hostName);
                            }
                            else
                            {
                                if (path.Contains(str5 + "Plugin"))
                                {
                                    if (path.EndsWith(str5 + "Plugin", StringComparison.OrdinalIgnoreCase))
                                    {
                                        this.AssertError(this.helper.GetResourceMsgFromResourcetext("SetItemNotSupported"), false);
                                    }
                                    try
                                    {
                                        XmlDocument xmlPlugins = this.FindResourceValue(obj2, resourceURI, null);
                                        string currentPluginName = string.Empty;
                                        this.GetPluginNames(xmlPlugins, out this.objPluginNames, out currentPluginName, path);
                                        if (string.IsNullOrEmpty(currentPluginName))
                                        {
                                            if (!this.clearItemIsCalled)
                                            {
                                                this.AssertError(this.helper.GetResourceMsgFromResourcetext("ItemDoesNotExist"), false);
                                            }
                                            goto Label_0CC5;
                                        }
                                        string str7 = resourceURI + "?Name=" + currentPluginName;
                                        CurrentConfigurations configurations = new CurrentConfigurations((IWSManSession) obj2);
                                        string responseOfGet = this.GetResourceValueInXml((IWSManSession) obj2, str7, null);
                                        configurations.RefreshCurrentConfiguration(responseOfGet);
                                        XmlDocument rootDocument = configurations.RootDocument;
                                        ArrayList arrSecurity = null;
                                        ArrayList list3 = this.ProcessPluginResourceLevel(rootDocument, out arrSecurity);
                                        ArrayList list4 = this.ProcessPluginInitParamLevel(rootDocument);
                                        try
                                        {
                                            configurations.RemoveOneConfiguration("./attribute::xml:lang");
                                        }
                                        catch (ArgumentException)
                                        {
                                        }
                                        str5 = str5 + "Plugin" + '\\';
                                        if (path.EndsWith(str5 + currentPluginName, StringComparison.OrdinalIgnoreCase))
                                        {
                                            if ("RunAsUser".Equals(str, StringComparison.OrdinalIgnoreCase))
                                            {
                                                PSCredential credential = value as PSCredential;
                                                if (credential != null)
                                                {
                                                    value = credential.UserName;
                                                    configurations.UpdateOneConfiguration(".", "RunAsPassword", this.GetStringFromSecureString(credential.Password));
                                                }
                                            }
                                            if ("RunAsPassword".Equals(str, StringComparison.OrdinalIgnoreCase))
                                            {
                                                if (string.IsNullOrEmpty(configurations.GetOneConfiguration(string.Format(CultureInfo.InvariantCulture, "./attribute::{0}", new object[] { "RunAsUser" }))))
                                                {
                                                    this.AssertError(this.helper.GetResourceMsgFromResourcetext("SetItemOnRunAsPasswordNoRunAsUser"), false);
                                                }
                                                value = this.GetStringFromSecureString(value);
                                            }
                                            configurations.UpdateOneConfiguration(".", str, value.ToString());
                                        }
                                        str5 = str5 + currentPluginName + '\\';
                                        if (path.Contains(str5 + "Resources"))
                                        {
                                            if (path.EndsWith(str5 + "Resources", StringComparison.OrdinalIgnoreCase))
                                            {
                                                this.AssertError(this.helper.GetResourceMsgFromResourcetext("SetItemNotSupported"), false);
                                                goto Label_0CC5;
                                            }
                                            str5 = str5 + "Resources" + '\\';
                                            int index = path.IndexOf('\\', str5.Length);
                                            string str9 = string.Empty;
                                            if (index == -1)
                                            {
                                                str9 = path.Substring(str5.Length);
                                            }
                                            else
                                            {
                                                str9 = path.Substring(str5.Length, path.IndexOf('\\', str5.Length) - str5.Length);
                                            }
                                            if (path.Contains(str5 + str9))
                                            {
                                                if (path.EndsWith(str5 + str9, StringComparison.OrdinalIgnoreCase))
                                                {
                                                    if (list3 == null)
                                                    {
                                                        goto Label_0CC5;
                                                    }
                                                    if (str.Equals("ResourceUri", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        this.AssertError(this.helper.GetResourceMsgFromResourcetext("NoChangeValue"), false);
                                                        goto Label_0CC5;
                                                    }
                                                    foreach (PSObject obj3 in list3)
                                                    {
                                                        if (str9.Equals(obj3.Properties["ResourceDir"].Value.ToString(), StringComparison.OrdinalIgnoreCase))
                                                        {
                                                            string pathToNodeFromRoot = string.Format(CultureInfo.InvariantCulture, "{0}:{1}/{0}:{2}[attribute::{3}='{4}']", new object[] { "defaultNameSpace", "Resources", "Resource", "ResourceUri", obj3.Properties["ResourceUri"].Value.ToString() });
                                                            if (!obj3.Properties[str].Value.ToString().Equals("Container"))
                                                            {
                                                                configurations.UpdateOneConfiguration(pathToNodeFromRoot, str, value.ToString());
                                                            }
                                                            else
                                                            {
                                                                this.AssertError(this.helper.GetResourceMsgFromResourcetext("NoChangeValue"), false);
                                                            }
                                                        }
                                                    }
                                                }
                                                str5 = str5 + str9 + '\\';
                                                if (path.Contains(str5 + "Security"))
                                                {
                                                    if (path.EndsWith(str5 + "Security", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        this.AssertError(this.helper.GetResourceMsgFromResourcetext("NoChangeValue"), false);
                                                        goto Label_0CC5;
                                                    }
                                                    if (arrSecurity == null)
                                                    {
                                                        goto Label_0CC5;
                                                    }
                                                    if (path.Contains(string.Concat(new object[] { str5, "Security", '\\', "Security_" })))
                                                    {
                                                        flag = true;
                                                        foreach (PSObject obj4 in arrSecurity)
                                                        {
                                                            if (path.Substring(path.LastIndexOf('\\') + 1, path.Length - (path.LastIndexOf('\\') + 1)).Equals(obj4.Properties["SecurityDIR"].Value.ToString(), StringComparison.OrdinalIgnoreCase))
                                                            {
                                                                if (base.Force == 0)
                                                                {
                                                                    string resourceMsgFromResourcetext = this.helper.GetResourceMsgFromResourcetext("ShouldContinueSecurityQuery");
                                                                    resourceMsgFromResourcetext = string.Format(CultureInfo.CurrentCulture, resourceMsgFromResourcetext, new object[] { currentPluginName });
                                                                    if (!base.ShouldContinue(resourceMsgFromResourcetext, this.helper.GetResourceMsgFromResourcetext("ShouldContinueSecurityCaption")))
                                                                    {
                                                                        goto Label_0CC5;
                                                                    }
                                                                }
                                                                string str13 = string.Format(CultureInfo.InvariantCulture, "{0}:{1}/{0}:{2}[@{6}='{7}']/{0}:{3}[@{4}='{5}']", new object[] { "defaultNameSpace", "Resources", "Resource", "Security", "Uri", obj4.Properties["Uri"].Value.ToString(), "ResourceUri", obj4.Properties["ParentResourceUri"].Value.ToString() });
                                                                configurations.UpdateOneConfiguration(str13, str, value.ToString());
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else if (path.EndsWith(str5 + "InitializationParameters", StringComparison.OrdinalIgnoreCase))
                                        {
                                            foreach (PSObject obj5 in list4)
                                            {
                                                if (obj5.Properties[str] != null)
                                                {
                                                    string str14 = string.Format(CultureInfo.InvariantCulture, "{0}:{1}/{0}:{2}[@{3}='{4}']", new object[] { "defaultNameSpace", "InitializationParameters", "Param", "Name", obj5.Properties[str].Name });
                                                    configurations.UpdateOneConfiguration(str14, "Value", value.ToString());
                                                    break;
                                                }
                                            }
                                        }
                                        else if (path.EndsWith(str5 + "Quotas", StringComparison.OrdinalIgnoreCase))
                                        {
                                            string str15 = string.Format(CultureInfo.InvariantCulture, "{0}:{1}", new object[] { "defaultNameSpace", "Quotas" });
                                            configurations.UpdateOneConfiguration(str15, str, value.ToString());
                                            if (ppqWarningConfigurations.Contains(str.ToLowerInvariant()))
                                            {
                                                string str16 = str;
                                                if (str.Equals("IdleTimeoutms", StringComparison.InvariantCultureIgnoreCase))
                                                {
                                                    str16 = "IdleTimeout";
                                                }
                                                string str17 = string.Format(CultureInfo.InvariantCulture, @"{0}:\{1}\{2}\{3}", new object[] { "WSMan", hostName, "Shell", str16 });
                                                list.Add(string.Format(this.helper.GetResourceMsgFromResourcetext("SetItemWarnigForPPQ"), str17));
                                            }
                                        }
                                        sessionObjCache.TryGetValue(hostName, out obj2);
                                        string resourceUri = string.Format(CultureInfo.InvariantCulture, "{0}?Name={1}", new object[] { resourceURI, currentPluginName });
                                        try
                                        {
                                            configurations.PutConfiguraitonOnServer(resourceUri);
                                            if (!flag)
                                            {
                                                if (this.IsPathLocalMachine(hostName))
                                                {
                                                    list.Add(this.helper.GetResourceMsgFromResourcetext("SetItemServiceRestartWarning"));
                                                }
                                                else
                                                {
                                                    list.Add(string.Format(this.helper.GetResourceMsgFromResourcetext("SetItemServiceRestartWarningRemote"), hostName));
                                                }
                                            }
                                        }
                                        finally
                                        {
                                            if (!string.IsNullOrEmpty(configurations.ServerSession.Error))
                                            {
                                                this.AssertError(configurations.ServerSession.Error, true);
                                            }
                                        }
                                        goto Label_0C82;
                                    }
                                    catch (PSArgumentException)
                                    {
                                        this.AssertError(this.helper.GetResourceMsgFromResourcetext("ItemDoesNotExist"), false);
                                        goto Label_0CC5;
                                    }
                                    catch (PSArgumentNullException)
                                    {
                                        this.AssertError(this.helper.GetResourceMsgFromResourcetext("ItemDoesNotExist"), false);
                                        goto Label_0CC5;
                                    }
                                    catch (NullReferenceException)
                                    {
                                        this.AssertError(this.helper.GetResourceMsgFromResourcetext("ItemDoesNotExist"), false);
                                        goto Label_0CC5;
                                    }
                                }
                                try
                                {
                                    Hashtable hashtable = new Hashtable();
                                    if (str.Equals("TrustedHosts", StringComparison.OrdinalIgnoreCase) || str.Equals("RootSDDL", StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (value.GetType().FullName.Equals("System.String"))
                                        {
                                            if (base.Force == 0)
                                            {
                                                string query = "";
                                                string caption = this.helper.GetResourceMsgFromResourcetext("SetItemGeneralSecurityCaption");
                                                if (str.Equals("TrustedHosts", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    query = this.helper.GetResourceMsgFromResourcetext("SetItemTrustedHostsWarningQuery");
                                                }
                                                else if (str.Equals("RootSDDL", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    query = this.helper.GetResourceMsgFromResourcetext("SetItemRootSDDLWarningQuery");
                                                }
                                                if (!base.ShouldContinue(query, caption))
                                                {
                                                    goto Label_0CC5;
                                                }
                                            }
                                            WSManProviderSetItemDynamicParameters dynamicParameters = base.DynamicParameters as WSManProviderSetItemDynamicParameters;
                                            if (((dynamicParameters != null) && (dynamicParameters.Concatenate != 0)) && !string.IsNullOrEmpty(value.ToString()))
                                            {
                                                value = this.SplitAndUpdateStringUsingDelimiter(obj2, resourceURI, str, value.ToString(), ",");
                                            }
                                            hashtable.Add(str, value);
                                        }
                                        else
                                        {
                                            this.AssertError(this.helper.GetResourceMsgFromResourcetext("TrustedHostValueTypeError"), false);
                                        }
                                    }
                                    else
                                    {
                                        hashtable.Add(str, value);
                                        if (globalWarningUris.Contains(resourceURI) && globalWarningConfigurations.Contains(str.ToLowerInvariant()))
                                        {
                                            list.Add(string.Format(this.helper.GetResourceMsgFromResourcetext("SetItemWarningForGlobalQuota"), value));
                                        }
                                    }
                                    this.PutResourceValue(obj2, resourceURI, hashtable, hostName);
                                }
                                catch (COMException exception)
                                {
                                    this.AssertError(exception.Message, false);
                                    goto Label_0CC5;
                                }
                            }
                        Label_0C82:
                            foreach (string str21 in list)
                            {
                                base.WriteWarning(str21);
                            }
                        Label_0CC5:;
                        }
                    }
                }
            }
        }

        protected override object SetItemDynamicParameters(string path, object value)
        {
            if (path.Length != 0)
            {
                string hostName = this.GetHostName(path);
                if (path.EndsWith(hostName + '\\' + "Client", StringComparison.OrdinalIgnoreCase))
                {
                    return new WSManProviderSetItemDynamicParameters();
                }
                if (path.EndsWith(string.Concat(new object[] { hostName, '\\', "Client", '\\', "TrustedHosts" }), StringComparison.OrdinalIgnoreCase))
                {
                    return new WSManProviderSetItemDynamicParameters();
                }
            }
            return null;
        }

        private void SetItemListenerOrClientCertificate(object sessionObj, string ResourceURI, string[] PrimaryKeys, string childName, object value, string path, string parent, string host)
        {
            Hashtable listenercache = null;
            Hashtable keyscache = null;
            XmlDocument xmlListeners = this.EnumerateResourceValue(sessionObj, ResourceURI);
            if (xmlListeners == null)
            {
                this.AssertError(this.helper.GetResourceMsgFromResourcetext("InvalidPath"), false);
            }
            if (ResourceURI.EndsWith("Listener", StringComparison.OrdinalIgnoreCase))
            {
                this.ProcessListernerObjects(xmlListeners, out listenercache, out keyscache);
            }
            else if (ResourceURI.EndsWith("Service/certmapping", StringComparison.OrdinalIgnoreCase))
            {
                this.ProcessCertMappingObjects(xmlListeners, out listenercache, out keyscache);
            }
            if (path.EndsWith(host + '\\' + parent, StringComparison.OrdinalIgnoreCase))
            {
                this.AssertError(this.helper.GetResourceMsgFromResourcetext("SetItemNotSupported"), false);
            }
            else if ((base.Force != 0) || base.ShouldContinue(this.helper.GetResourceMsgFromResourcetext("SetItemShouldContinueQuery"), this.helper.GetResourceMsgFromResourcetext("SetItemShouldContinueCaption")))
            {
                string str = path.Substring(path.LastIndexOf('\\') + 1);
                try
                {
                    Hashtable hashtable3 = new Hashtable();
                    hashtable3.Add(childName, value);
                    foreach (string str2 in PrimaryKeys)
                    {
                        hashtable3.Add(str2, ((PSObject) listenercache[str]).Properties[str2].Value);
                    }
                    this.PutResourceValue(sessionObj, ResourceURI, hashtable3, host);
                }
                catch (COMException exception)
                {
                    ErrorRecord errorRecord = new ErrorRecord(exception, "COMException", ErrorCategory.InvalidOperation, null);
                    base.WriteError(errorRecord);
                }
            }
        }

        private string SetSchemaPath(string uri)
        {
            uri = uri.Remove(0, WinrmRootName[0].Length);
            if (uri.Contains("Plugin"))
            {
                return "http://schemas.microsoft.com/wbem/wsman/1/config/plugin";
            }
            if (uri.Contains("ClientCertificate"))
            {
                uri = uri.Replace("ClientCertificate", "/service/certmapping");
                return ("http://schemas.microsoft.com/wbem/wsman/1/config" + uri.ToLower(CultureInfo.InvariantCulture));
            }
            return ("http://schemas.microsoft.com/wbem/wsman/1/config" + uri.ToLower(CultureInfo.InvariantCulture));
        }

        private string SetXPathString(string uri)
        {
            char ch = '/';
            string str = uri.Substring(uri.LastIndexOf(ch.ToString(), StringComparison.OrdinalIgnoreCase) + 1);
            if (str.Equals("Winrs", StringComparison.OrdinalIgnoreCase))
            {
                str = "Winrs";
            }
            else if (str.Equals("Auth", StringComparison.OrdinalIgnoreCase))
            {
                str = "Auth";
            }
            else if (str.Equals("certmapping", StringComparison.OrdinalIgnoreCase))
            {
                str = "CertMapping";
            }
            else if (str.Equals("Service", StringComparison.OrdinalIgnoreCase))
            {
                str = "Service";
            }
            else if (str.Equals("DefaultPorts", StringComparison.OrdinalIgnoreCase))
            {
                str = "DefaultPorts";
            }
            else if (str.Equals("Plugin", StringComparison.OrdinalIgnoreCase))
            {
                str = "Plugin";
            }
            return ("/cfg:" + str);
        }

        private string SplitAndUpdateStringUsingDelimiter(object sessionobj, string uri, string childname, string value, string Delimiter)
        {
            XmlDocument document = this.GetResourceValue(sessionobj, uri, null);
            PSObject obj2 = null;
            foreach (XmlNode node in document.ChildNodes)
            {
                obj2 = this.ConvertToPSObject(node);
            }
            string str = string.Empty;
            try
            {
                if (obj2 != null)
                {
                    str = obj2.Properties[childname].Value.ToString();
                }
                if (!string.IsNullOrEmpty(str))
                {
                    string[] array = str.Split(new string[] { Delimiter }, StringSplitOptions.None);
                    foreach (string str2 in value.Split(new string[] { Delimiter }, StringSplitOptions.None))
                    {
                        if (Array.IndexOf<string>(array, str2) == -1)
                        {
                            str = str + Delimiter + str2;
                        }
                    }
                    return str;
                }
                str = value;
            }
            catch (PSArgumentException)
            {
            }
            return str;
        }

        private void StartWSManService(bool force)
        {
            try
            {
                if (!((bool) ScriptBlock.Create(string.Format(CultureInfo.InvariantCulture, "\r\nfunction Start-WSManServiceD15A7957836142a18627D7E1D342DD82\r\n{{\r\n[CmdletBinding()]\r\nparam(\r\n    [Parameter()]\r\n    [bool]\r\n    $Force,\r\n   \r\n    [Parameter()]\r\n    [string]\r\n    $captionForStart,\r\n        \r\n    [Parameter()]\r\n    [string]\r\n    $queryForStart)\r\n\r\n    begin\r\n    {{    \r\n        if ($force -or $pscmdlet.ShouldContinue($queryForStart, $captionForStart))\r\n        {{\r\n            Restart-Service WinRM -Force\r\n            return $true\r\n        }}\r\n        return $false    \r\n    }} #end of Begin block\r\n}}\r\n$_ | Start-WSManServiceD15A7957836142a18627D7E1D342DD82 -force $args[0] -captionForStart $args[1] -queryForStart $args[2]\r\n", new object[0])).Invoke(new object[] { force, this.helper.GetResourceMsgFromResourcetext("WSManServiceStartCaption"), this.helper.GetResourceMsgFromResourcetext("WSManServiceStartQuery") })[0].ImmediateBaseObject))
                {
                    this.AssertError(this.helper.GetResourceMsgFromResourcetext("WinRMServiceError"), false);
                }
            }
            catch (CmdletInvocationException)
            {
            }
        }

        string ICmdletProviderSupportsHelp.GetHelpMaml(string helpItemName, string path)
        {
            int num = path.LastIndexOf(@"\", StringComparison.OrdinalIgnoreCase);
            if (num != -1)
            {
                string str9;
                string str = path.Substring(num + 1);
                if (((str9 = helpItemName) == null) || (((!(str9 == "New-Item") && !(str9 == "Get-Item")) && (!(str9 == "Set-Item") && !(str9 == "Clear-Item"))) && !(str9 == "Remove-Item")))
                {
                    return string.Empty;
                }
                XmlDocument document = new XmlDocument();
                CultureInfo currentUICulture = base.Host.CurrentUICulture;
                string str2 = (base.ProviderInfo.PSSnapIn != null) ? base.ProviderInfo.PSSnapIn.ApplicationBase : base.ProviderInfo.Module.ModuleBase;
                string filename = null;
                do
                {
                    string str4 = Path.Combine(str2, currentUICulture.Name);
                    if (Directory.Exists(str4) && File.Exists(str4 + @"\" + base.ProviderInfo.HelpFile))
                    {
                        filename = str4 + @"\" + base.ProviderInfo.HelpFile;
                        break;
                    }
                    currentUICulture = currentUICulture.Parent;
                }
                while (currentUICulture != currentUICulture.Parent);
                if (filename != null)
                {
                    try
                    {
                        document.Load(filename);
                    }
                    catch (XmlException)
                    {
                        return string.Empty;
                    }
                    catch (PathTooLongException)
                    {
                        return string.Empty;
                    }
                    catch (IOException)
                    {
                        return string.Empty;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        return string.Empty;
                    }
                    catch (NotSupportedException)
                    {
                        return string.Empty;
                    }
                    catch (SecurityException)
                    {
                        return string.Empty;
                    }
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(document.NameTable);
                    nsmgr.AddNamespace("command", "http://schemas.microsoft.com/maml/dev/command/2004/10");
                    string str5 = helpItemName.Split(new char[] { '-' })[0];
                    string str6 = helpItemName.Substring(helpItemName.IndexOf('-') + 1);
                    string xpath = "/helpItems/providerHelp/CmdletHelpPaths/CmdletHelpPath[@ID='" + str + "']/command:command/command:details[command:verb='" + str5 + "' and command:noun='" + str6 + "']";
                    XmlNode node = null;
                    try
                    {
                        node = document.SelectSingleNode(xpath, nsmgr);
                    }
                    catch (XPathException)
                    {
                        return string.Empty;
                    }
                    if (node != null)
                    {
                        return node.ParentNode.OuterXml;
                    }
                }
            }
            return string.Empty;
        }

        private object ValidateAndGetUserObject(string configurationName, object value)
        {
            PSObject obj2 = value as PSObject;
            if (configurationName.Equals("RunAsPassword", StringComparison.OrdinalIgnoreCase))
            {
                if ((obj2 != null) && (obj2.BaseObject is SecureString))
                {
                    return (obj2.BaseObject as SecureString);
                }
                string str = string.Format(this.helper.GetResourceMsgFromResourcetext("InvalidValueType"), "RunAsPassword", typeof(SecureString).FullName);
                this.AssertError(str, false);
                return null;
            }
            if (!configurationName.Equals("RunAsUser", StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }
            if ((obj2 != null) && (obj2.BaseObject is PSCredential))
            {
                return (obj2.BaseObject as PSCredential);
            }
            string errorMessage = string.Format(this.helper.GetResourceMsgFromResourcetext("InvalidValueType"), "RunAsUser", typeof(PSCredential).FullName);
            this.AssertError(errorMessage, false);
            return null;
        }

        private void WritePSObjectPropertiesAsWSManElementObjects(PSObject psobject, string path, string[] keys, string ExtendedTypeName, WsManElementObjectTypes WSManElementObjectType, bool recurse)
        {
            PSObject item = null;
            Collection<string> collection = new Collection<string>();
            foreach (PSPropertyInfo info in psobject.Properties)
            {
                if (!info.Name.EndsWith("___Source"))
                {
                    if (WSManElementObjectType.Equals(WsManElementObjectTypes.WSManConfigElement))
                    {
                        WSManConfigElement element = new WSManConfigElement(info.Name, info.Value.ToString());
                        item = new PSObject(element);
                    }
                    if (WSManElementObjectType.Equals(WsManElementObjectTypes.WSManConfigContainerElement))
                    {
                        WSManConfigContainerElement element2 = new WSManConfigContainerElement(info.Name, info.Value.ToString(), keys);
                        item = new PSObject(element2);
                    }
                    if (WSManElementObjectType.Equals(WsManElementObjectTypes.WSManConfigLeafElement))
                    {
                        string str = info.Name + "___Source";
                        object sourceOfValue = null;
                        if (psobject.Properties[str] != null)
                        {
                            sourceOfValue = psobject.Properties[str].Value;
                        }
                        WSManConfigLeafElement element3 = null;
                        if (!info.Value.ToString().Equals("Container"))
                        {
                            element3 = new WSManConfigLeafElement(info.Name, info.Value, info.TypeNameOfValue, sourceOfValue);
                        }
                        else
                        {
                            element3 = new WSManConfigLeafElement(info.Name, null, info.Value.ToString(), null);
                        }
                        if (element3 != null)
                        {
                            item = new PSObject(element3);
                        }
                    }
                    if (!string.IsNullOrEmpty(ExtendedTypeName))
                    {
                        StringBuilder builder = new StringBuilder("");
                        if (item != null)
                        {
                            builder.Append(item.ImmediateBaseObject.GetType().FullName);
                            builder.Append("#");
                            builder.Append(ExtendedTypeName);
                            item.TypeNames.Insert(0, builder.ToString());
                        }
                    }
                    if (!info.Value.ToString().Equals("Container"))
                    {
                        string str2 = "WSMan".Equals(path, StringComparison.OrdinalIgnoreCase) ? info.Name : (path + '\\' + info.Name);
                        base.WriteItemObject(item, str2, false);
                    }
                    else
                    {
                        string str3 = "WSMan".Equals(path, StringComparison.OrdinalIgnoreCase) ? info.Name : (path + '\\' + info.Name);
                        base.WriteItemObject(item, str3, true);
                        if (recurse)
                        {
                            collection.Add(info.Name);
                        }
                    }
                }
            }
            if (recurse)
            {
                foreach (string str4 in collection)
                {
                    this.GetChildItemsRecurse(path, str4, ProviderMethods.GetChildItems, recurse);
                }
            }
        }

        private void WritePSObjectPropertyNames(PSObject psobject, string path)
        {
            foreach (PSPropertyInfo info in psobject.Properties)
            {
                if (!info.Value.ToString().Equals("Container"))
                {
                    base.WriteItemObject(info.Name, path + '\\' + info.Name, true);
                }
                else
                {
                    base.WriteItemObject(info.Name, path + '\\' + info.Name, false);
                }
            }
        }

        // Nested Types
        private enum ProviderMethods
        {
            GetChildItems,
            GetChildNames
        }

        private enum WsManElementObjectTypes
        {
            WSManConfigElement,
            WSManConfigContainerElement,
            WSManConfigLeafElement
        }
    }
}