namespace System.Management.Automation.Help
{
    using Microsoft.PowerShell.Commands;
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using System.Xml;
    using System.Xml.Schema;

    internal class UpdatableHelpSystem : IDisposable
    {
        private UpdatableHelpCommandBase _cmdlet;
        private bool _completed = false;
        private AutoResetEvent _completionEvent = new AutoResetEvent(false);
        private string _currentModule;
        private Collection<Exception> _errors = new Collection<Exception>();
        private Collection<UpdatableHelpProgressEventArgs> _progressEvents = new Collection<UpdatableHelpProgressEventArgs>();
        private bool _stopping = false;
        private object _syncObject = new object();
        private System.Net.WebClient _webClient = new System.Net.WebClient();
        internal const string DefaultSourcePathRegKey = "DefaultSourcePath";
        internal const string DefaultSourcePathRegPath = @"Software\Policies\Microsoft\Windows\PowerShell\UpdatableHelp";
        internal const string DisablePromptToUpdateHelpRegKey = "DisablePromptToUpdateHelp";
        internal const string DisablePromptToUpdateHelpRegPath = @"Software\Microsoft\PowerShell";
        internal const string DisablePromptToUpdateHelpRegPath32 = @"Software\Wow6432Node\Microsoft\PowerShell";
        private static string HelpInfoXmlNamespace = "http://schemas.microsoft.com/powershell/help/2010/05";
        private static string HelpInfoXmlSchema = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n            <xs:schema attributeFormDefault=\"unqualified\" elementFormDefault=\"qualified\"\r\n                targetNamespace=\"http://schemas.microsoft.com/powershell/help/2010/05\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\r\n                <xs:element name=\"HelpInfo\">\r\n                    <xs:complexType>\r\n                        <xs:sequence>\r\n                            <xs:element name=\"HelpContentURI\" type=\"xs:string\" minOccurs=\"1\" maxOccurs=\"1\" />\r\n                            <xs:element name=\"SupportedUICultures\" minOccurs=\"1\" maxOccurs=\"1\">\r\n                                <xs:complexType>\r\n                                    <xs:sequence>\r\n                                        <xs:element name=\"UICulture\" minOccurs=\"1\" maxOccurs=\"unbounded\">\r\n                                            <xs:complexType>\r\n                                                <xs:sequence>\r\n                                                    <xs:element name=\"UICultureName\" type=\"xs:language\" minOccurs=\"1\" maxOccurs=\"1\" />\r\n                                                    <xs:element name=\"UICultureVersion\" type=\"xs:string\" minOccurs=\"1\" maxOccurs=\"1\" />\r\n                                                </xs:sequence>\r\n                                            </xs:complexType>\r\n                                        </xs:element>\r\n                                    </xs:sequence>\r\n                                </xs:complexType>\r\n                            </xs:element>\r\n                        </xs:sequence>\r\n                    </xs:complexType>\r\n                </xs:element>\r\n            </xs:schema>";

        internal event EventHandler<UpdatableHelpProgressEventArgs> OnProgressChanged;

        internal UpdatableHelpSystem(UpdatableHelpCommandBase cmdlet, bool useDefaultCredentials)
        {
            this._cmdlet = cmdlet;
            this._webClient.UseDefaultCredentials = useDefaultCredentials;
            this._webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(this.HandleDownloadProgressChanged);
            this._webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(this.HandleDownloadFileCompleted);
        }

        internal void CancelDownload()
        {
            if (this._webClient.IsBusy)
            {
                this._webClient.CancelAsync();
                this._completed = true;
                this._completionEvent.Set();
            }
            this._stopping = true;
        }

        internal UpdatableHelpInfo CreateHelpInfo(string xml, string moduleName, Guid moduleGuid, string currentCulture, string pathOverride, bool verbose)
        {
            XmlDocument document = null;
            try
            {
                document = this.CreateValidXmlDocument(xml, HelpInfoXmlNamespace, HelpInfoXmlSchema, new ValidationEventHandler(this.HelpInfoValidationHandler), true);
            }
            catch (XmlException exception)
            {
                throw new UpdatableHelpSystemException("HelpInfoXmlValidationFailure", exception.Message, ErrorCategory.InvalidData, null, exception);
            }
            string resolvedUri = pathOverride;
            string innerText = document["HelpInfo"]["HelpContentURI"].InnerText;
            if (string.IsNullOrEmpty(pathOverride))
            {
                resolvedUri = this.ResolveUri(innerText, verbose);
            }
            XmlNodeList childNodes = document["HelpInfo"]["SupportedUICultures"].ChildNodes;
            CultureSpecificUpdatableHelp[] cultures = new CultureSpecificUpdatableHelp[childNodes.Count];
            for (int i = 0; i < childNodes.Count; i++)
            {
                cultures[i] = new CultureSpecificUpdatableHelp(new CultureInfo(childNodes[i]["UICultureName"].InnerText), new Version(childNodes[i]["UICultureVersion"].InnerText));
            }
            UpdatableHelpInfo info = new UpdatableHelpInfo(innerText, cultures);
            if (!string.IsNullOrEmpty(currentCulture))
            {
                WildcardOptions options = WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase;
                IEnumerable<WildcardPattern> patterns = SessionStateUtilities.CreateWildcardsFromStrings(new string[] { currentCulture }, options);
                for (int j = 0; j < cultures.Length; j++)
                {
                    if (SessionStateUtilities.MatchesAnyWildcardPattern(cultures[j].Culture.Name, patterns, true))
                    {
                        info.HelpContentUriCollection.Add(new UpdatableHelpUri(moduleName, moduleGuid, cultures[j].Culture, resolvedUri));
                    }
                }
            }
            if (!string.IsNullOrEmpty(currentCulture) && (info.HelpContentUriCollection.Count == 0))
            {
                throw new UpdatableHelpSystemException("HelpCultureNotSupported", StringUtil.Format(HelpDisplayStrings.HelpCultureNotSupported, currentCulture, info.GetSupportedCultures()), ErrorCategory.InvalidOperation, null, null);
            }
            return info;
        }

        private XmlDocument CreateValidXmlDocument(string xml, string ns, string schema, ValidationEventHandler handler, bool helpInfo)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.Schemas.Add(ns, new XmlTextReader(new StringReader(schema)));
            settings.ValidationType = ValidationType.Schema;
            XmlReader reader = XmlReader.Create(new StringReader(xml), settings);
            XmlDocument document = new XmlDocument();
            try
            {
                document.Load(reader);
                document.Validate(handler);
            }
            catch (XmlSchemaValidationException exception)
            {
                if (helpInfo)
                {
                    throw new UpdatableHelpSystemException("HelpInfoXmlValidationFailure", StringUtil.Format(HelpDisplayStrings.HelpInfoXmlValidationFailure, exception.Message), ErrorCategory.InvalidData, null, exception);
                }
                throw new UpdatableHelpSystemException("HelpContentXmlValidationFailure", StringUtil.Format(HelpDisplayStrings.HelpContentXmlValidationFailure, exception.Message), ErrorCategory.InvalidData, null, exception);
            }
            return document;
        }

        public void Dispose()
        {
            this._completionEvent.Close();
            this._webClient.Dispose();
            GC.SuppressFinalize(this);
        }

        internal bool DownloadAndInstallHelpContent(UpdatableHelpCommandType commandType, System.Management.Automation.ExecutionContext context, Collection<string> destPaths, string fileName, CultureInfo culture, string helpContentUri, string xsdPath, out Collection<string> installed)
        {
            if (this._stopping)
            {
                installed = new Collection<string>();
                return false;
            }
            string path = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            if (!this.DownloadHelpContent(commandType, path, helpContentUri, fileName, culture.Name))
            {
                installed = new Collection<string>();
                return false;
            }
            this.InstallHelpContent(commandType, context, path, destPaths, fileName, path, culture, xsdPath, out installed);
            return true;
        }

        internal bool DownloadHelpContent(UpdatableHelpCommandType commandType, string path, string helpContentUri, string fileName, string culture)
        {
            if (this._stopping)
            {
                return false;
            }
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            this.OnProgressChanged(this, new UpdatableHelpProgressEventArgs(this._currentModule, commandType, StringUtil.Format(HelpDisplayStrings.UpdateProgressConnecting, new object[0]), 0));
            string uriString = helpContentUri + fileName;
            this._webClient.DownloadFileAsync(new Uri(uriString), Path.Combine(path, fileName), culture);
            this.OnProgressChanged(this, new UpdatableHelpProgressEventArgs(this._currentModule, commandType, StringUtil.Format(HelpDisplayStrings.UpdateProgressConnecting, new object[0]), 100));
            while (!this._completed || this._webClient.IsBusy)
            {
                this._completionEvent.WaitOne();
                lock (this._syncObject)
                {
                    if (this._progressEvents.Count > 0)
                    {
                        foreach (UpdatableHelpProgressEventArgs args in this._progressEvents)
                        {
                            args.CommandType = commandType;
                            this.OnProgressChanged(this, args);
                        }
                        this._progressEvents.Clear();
                    }
                    continue;
                }
            }
            return (this._errors.Count == 0);
        }

        internal void GenerateHelpInfo(string moduleName, Guid moduleGuid, string contentUri, string culture, Version version, string destPath, string fileName, bool force)
        {
            if (!this._stopping)
            {
                string path = Path.Combine(destPath, fileName);
                if (force)
                {
                    this.RemoveReadOnly(path);
                }
                UpdatableHelpInfo info = null;
                string xml = LoadStringFromPath(this._cmdlet, path, null);
                if (xml != null)
                {
                    info = this.CreateHelpInfo(xml, moduleName, moduleGuid, null, null, false);
                }
                using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8) {
                        Formatting = Formatting.Indented,
                        Indentation = 2
                    };
                    try
                    {
                        writer.WriteStartDocument();
                        writer.WriteStartElement("HelpInfo", "http://schemas.microsoft.com/powershell/help/2010/05");
                        writer.WriteStartElement("HelpContentURI");
                        writer.WriteValue(contentUri);
                        writer.WriteEndElement();
                        writer.WriteStartElement("SupportedUICultures");
                        bool flag = false;
                        if (info != null)
                        {
                            foreach (CultureSpecificUpdatableHelp help in info.UpdatableHelpItems)
                            {
                                if (help.Culture.Name.Equals(culture, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (help.Version.Equals(version))
                                    {
                                        writer.WriteStartElement("UICulture");
                                        writer.WriteStartElement("UICultureName");
                                        writer.WriteValue(help.Culture.Name);
                                        writer.WriteEndElement();
                                        writer.WriteStartElement("UICultureVersion");
                                        writer.WriteValue(help.Version.ToString());
                                        writer.WriteEndElement();
                                        writer.WriteEndElement();
                                    }
                                    else
                                    {
                                        writer.WriteStartElement("UICulture");
                                        writer.WriteStartElement("UICultureName");
                                        writer.WriteValue(culture);
                                        writer.WriteEndElement();
                                        writer.WriteStartElement("UICultureVersion");
                                        writer.WriteValue(version.ToString());
                                        writer.WriteEndElement();
                                        writer.WriteEndElement();
                                    }
                                    flag = true;
                                }
                                else
                                {
                                    writer.WriteStartElement("UICulture");
                                    writer.WriteStartElement("UICultureName");
                                    writer.WriteValue(help.Culture.Name);
                                    writer.WriteEndElement();
                                    writer.WriteStartElement("UICultureVersion");
                                    writer.WriteValue(help.Version.ToString());
                                    writer.WriteEndElement();
                                    writer.WriteEndElement();
                                }
                            }
                        }
                        if (!flag)
                        {
                            writer.WriteStartElement("UICulture");
                            writer.WriteStartElement("UICultureName");
                            writer.WriteValue(culture);
                            writer.WriteEndElement();
                            writer.WriteStartElement("UICultureVersion");
                            writer.WriteValue(version.ToString());
                            writer.WriteEndElement();
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                        writer.WriteEndDocument();
                    }
                    finally
                    {
                        writer.Close();
                    }
                }
            }
        }

        internal IEnumerable<string> GetCurrentUICulture()
        {
            CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;
            while (true)
            {
                if ((currentUICulture == null) || string.IsNullOrEmpty(currentUICulture.Name))
                {
                    yield break;
                }
                yield return currentUICulture.Name;
                currentUICulture = currentUICulture.Parent;
            }
        }

        internal string GetDefaultSourcePath()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Policies\Microsoft\Windows\PowerShell\UpdatableHelp"))
                {
                    if (key != null)
                    {
                        object obj2 = key.GetValue("DefaultSourcePath", null, RegistryValueOptions.None);
                        if (obj2 != null)
                        {
                            return (obj2 as string);
                        }
                    }
                }
            }
            catch (SecurityException)
            {
                return null;
            }
            return null;
        }

        internal UpdatableHelpInfo GetHelpInfo(UpdatableHelpCommandType commandType, string uri, string moduleName, Guid moduleGuid, string culture)
        {
            UpdatableHelpInfo info2;
            try
            {
                this.OnProgressChanged(this, new UpdatableHelpProgressEventArgs(this._currentModule, commandType, StringUtil.Format(HelpDisplayStrings.UpdateProgressLocating, new object[0]), 0));
                info2 = this.CreateHelpInfo(this._webClient.DownloadString(uri), moduleName, moduleGuid, culture, null, true);
            }
            catch (WebException)
            {
                info2 = null;
            }
            finally
            {
                this.OnProgressChanged(this, new UpdatableHelpProgressEventArgs(this._currentModule, commandType, StringUtil.Format(HelpDisplayStrings.UpdateProgressLocating, new object[0]), 100));
            }
            return info2;
        }

        internal UpdatableHelpUri GetHelpInfoUri(UpdatableHelpModuleInfo module, CultureInfo culture)
        {
            return new UpdatableHelpUri(module.ModuleName, module.ModuleGuid, culture, this.ResolveUri(module.HelpInfoUri, false));
        }

        private void HandleDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (!this._stopping && !e.Cancelled)
            {
                if (e.Error != null)
                {
                    if (e.Error is WebException)
                    {
                        this._errors.Add(new UpdatableHelpSystemException("HelpContentNotFound", StringUtil.Format(HelpDisplayStrings.HelpContentNotFound, e.UserState.ToString()), ErrorCategory.ResourceUnavailable, null, null));
                    }
                    else
                    {
                        this._errors.Add(e.Error);
                    }
                }
                else
                {
                    lock (this._syncObject)
                    {
                        this._progressEvents.Add(new UpdatableHelpProgressEventArgs(this._currentModule, StringUtil.Format(HelpDisplayStrings.UpdateProgressDownloading, new object[0]), 100));
                    }
                }
                this._completed = true;
                this._completionEvent.Set();
            }
        }

        private void HandleDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (!this._stopping)
            {
                lock (this._syncObject)
                {
                    this._progressEvents.Add(new UpdatableHelpProgressEventArgs(this._currentModule, StringUtil.Format(HelpDisplayStrings.UpdateProgressDownloading, new object[0]), e.ProgressPercentage));
                }
                this._completionEvent.Set();
            }
        }

        private void HelpContentValidationHandler(object sender, ValidationEventArgs arg)
        {
            switch (arg.Severity)
            {
                case XmlSeverityType.Error:
                    throw new UpdatableHelpSystemException("HelpContentXmlValidationFailure", StringUtil.Format(HelpDisplayStrings.HelpContentXmlValidationFailure, new object[0]), ErrorCategory.InvalidData, null, arg.Exception);

                case XmlSeverityType.Warning:
                    return;
            }
        }

        private void HelpInfoValidationHandler(object sender, ValidationEventArgs arg)
        {
            switch (arg.Severity)
            {
                case XmlSeverityType.Error:
                    throw new UpdatableHelpSystemException("HelpInfoXmlValidationFailure", StringUtil.Format(HelpDisplayStrings.HelpInfoXmlValidationFailure, new object[0]), ErrorCategory.InvalidData, null, arg.Exception);

                case XmlSeverityType.Warning:
                    return;
            }
        }

        internal void InstallHelpContent(UpdatableHelpCommandType commandType, System.Management.Automation.ExecutionContext context, string sourcePath, Collection<string> destPaths, string fileName, string tempPath, CultureInfo culture, string xsdPath, out Collection<string> installed)
        {
            if (this._stopping)
            {
                installed = new Collection<string>();
            }
            else
            {
                if (!Directory.Exists(tempPath))
                {
                    Directory.CreateDirectory(tempPath);
                }
                try
                {
                    this.OnProgressChanged(this, new UpdatableHelpProgressEventArgs(this._currentModule, commandType, StringUtil.Format(HelpDisplayStrings.UpdateProgressInstalling, new object[0]), 0));
                    string path = Path.Combine(sourcePath, fileName);
                    if (!System.IO.File.Exists(path))
                    {
                        throw new UpdatableHelpSystemException("HelpContentNotFound", StringUtil.Format(HelpDisplayStrings.HelpContentNotFound, new object[0]), ErrorCategory.ResourceUnavailable, null, null);
                    }
                    string str2 = Path.Combine(tempPath, Path.GetFileNameWithoutExtension(fileName));
                    if (Directory.Exists(str2))
                    {
                        Directory.Delete(str2, true);
                    }
                    this.UnzipHelpContent(context, path, str2);
                    this.ValidateAndCopyHelpContent(str2, destPaths, culture.Name, xsdPath, out installed);
                }
                finally
                {
                    this.OnProgressChanged(this, new UpdatableHelpProgressEventArgs(this._currentModule, commandType, StringUtil.Format(HelpDisplayStrings.UpdateProgressInstalling, new object[0]), 100));
                    try
                    {
                        if (Directory.Exists(tempPath))
                        {
                            Directory.Delete(tempPath);
                        }
                    }
                    catch (IOException)
                    {
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }
        }

        internal static bool IsAdministrator()
        {
			if (OSHelper.IsUnix) return true;
            WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        internal static string LoadStringFromPath(PSCmdlet cmdlet, string path, PSCredential credential)
        {
            if (credential != null)
            {
                using (UpdatableHelpSystemDrive drive = new UpdatableHelpSystemDrive(cmdlet, Path.GetDirectoryName(path), credential))
                {
                    string destinationPath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetTempFileName()));
                    if (!cmdlet.InvokeProvider.Item.Exists(Path.Combine(drive.DriveName, Path.GetFileName(path))))
                    {
                        return null;
                    }
                    cmdlet.InvokeProvider.Item.Copy(new string[] { Path.Combine(drive.DriveName, Path.GetFileName(path)) }, destinationPath, false, CopyContainers.CopyTargetContainer, true, true);
                    path = destinationPath;
                }
            }
            if (System.IO.File.Exists(path))
            {
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    StreamReader reader = new StreamReader(stream);
                    return reader.ReadToEnd();
                }
            }
            return null;
        }

        private void RemoveReadOnly(string path)
        {
            if (System.IO.File.Exists(path))
            {
                System.IO.FileAttributes fileAttributes = System.IO.File.GetAttributes(path);
                if ((fileAttributes & System.IO.FileAttributes.ReadOnly) == System.IO.FileAttributes.ReadOnly)
                {
                    fileAttributes &= ~System.IO.FileAttributes.ReadOnly;
                    System.IO.File.SetAttributes(path, fileAttributes);
                }
            }
        }

        private string ResolveUri(string baseUri, bool verbose)
        {
            if (Directory.Exists(baseUri) || baseUri.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                if (verbose)
                {
                    this._cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.URIRedirectWarningToHost, baseUri));
                }
                return baseUri;
            }
            string requestUriString = baseUri;
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    if (this._stopping)
                    {
                        return requestUriString;
                    }
                    HttpWebRequest request = (HttpWebRequest) WebRequest.Create(requestUriString);
                    request.AllowAutoRedirect = false;
                    request.Timeout = 0x7530;
                    HttpWebResponse response = (HttpWebResponse) request.GetResponse();
                    WebHeaderCollection headers = response.Headers;
                    try
                    {
                        if (((response.StatusCode == HttpStatusCode.Found) || (response.StatusCode == HttpStatusCode.Found)) || ((response.StatusCode == HttpStatusCode.MovedPermanently) || (response.StatusCode == HttpStatusCode.MovedPermanently)))
                        {
                            Uri uri = new Uri(headers["Location"], UriKind.RelativeOrAbsolute);
                            if (uri.IsAbsoluteUri)
                            {
                                requestUriString = uri.ToString();
                            }
                            else
                            {
                                requestUriString = requestUriString.Replace(request.Address.AbsolutePath, uri.ToString());
                            }
                            requestUriString = requestUriString.Trim();
                            if (verbose)
                            {
                                this._cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.URIRedirectWarningToHost, requestUriString));
                            }
                            if (requestUriString.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                            {
                                return requestUriString;
                            }
                        }
                        else if (response.StatusCode == HttpStatusCode.OK)
                        {
                            if (!requestUriString.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                            {
                                throw new UpdatableHelpSystemException("InvalidHelpInfoUri", StringUtil.Format(HelpDisplayStrings.InvalidHelpInfoUri, requestUriString), ErrorCategory.InvalidOperation, null, null);
                            }
                            return requestUriString;
                        }
                    }
                    finally
                    {
                        response.Close();
                    }
                }
            }
            catch (UriFormatException exception)
            {
                throw new UpdatableHelpSystemException("InvalidUriFormat", exception.Message, ErrorCategory.InvalidData, null, exception);
            }
            throw new UpdatableHelpSystemException("TooManyRedirections", StringUtil.Format(HelpDisplayStrings.TooManyRedirections, new object[0]), ErrorCategory.InvalidOperation, null, null);
        }

        internal static void SetDisablePromptToUpdateHelp()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\PowerShell", true))
                {
                    if (key != null)
                    {
                        key.SetValue("DisablePromptToUpdateHelp", 1, RegistryValueKind.DWord);
                    }
                }
                using (RegistryKey key2 = Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\Microsoft\PowerShell", true))
                {
                    if (key2 != null)
                    {
                        key2.SetValue("DisablePromptToUpdateHelp", 1, RegistryValueKind.DWord);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (SecurityException)
            {
            }
        }

        internal static bool ShouldPromptToUpdateHelp()
        {
            bool flag;
            try
            {
                if (!IsAdministrator())
                {
                    flag = false;
                }
                else
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\PowerShell"))
                    {
                        if (key != null)
                        {
                            int num;
                            object valueToConvert = key.GetValue("DisablePromptToUpdateHelp", null, RegistryValueOptions.None);
                            if ((valueToConvert != null) && LanguagePrimitives.TryConvertTo<int>(valueToConvert, out num))
                            {
                                return (num != 1);
                            }
                            return true;
                        }
                        flag = true;
                    }
                }
            }
            catch (SecurityException)
            {
                flag = false;
            }
            return flag;
        }

        private void UnzipHelpContent(System.Management.Automation.ExecutionContext context, string srcPath, string destPath)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }
            string directoryName = Path.GetDirectoryName(srcPath);
            if (!directoryName.EndsWith(@"\", StringComparison.Ordinal))
            {
                directoryName = directoryName + @"\";
            }
            if (!destPath.EndsWith(@"\", StringComparison.Ordinal))
            {
                destPath = destPath + @"\";
            }
            if (!CabinetExtractorFactory.GetCabinetExtractor().Extract(Path.GetFileName(srcPath), directoryName, destPath))
            {
                throw new UpdatableHelpSystemException("UnableToExtract", StringUtil.Format(HelpDisplayStrings.UnzipFailure, new object[0]), ErrorCategory.InvalidOperation, null, null);
            }
            foreach (string str2 in Directory.GetFiles(destPath))
            {
                if (System.IO.File.Exists(str2))
                {
                    FileInfo info = new FileInfo(str2);
                    if ((info.Attributes & System.IO.FileAttributes.ReadOnly) == System.IO.FileAttributes.ReadOnly)
                    {
                        info.Attributes &= ~System.IO.FileAttributes.ReadOnly;
                    }
                }
            }
        }

        private void ValidateAndCopyHelpContent(string sourcePath, Collection<string> destPaths, string culture, string xsdPath, out Collection<string> installed)
        {
            installed = new Collection<string>();
            string schema = LoadStringFromPath(this._cmdlet, xsdPath, null);
            foreach (string str2 in Directory.GetFiles(sourcePath))
            {
                if (!string.Equals(Path.GetExtension(str2), ".xml", StringComparison.OrdinalIgnoreCase) && !string.Equals(Path.GetExtension(str2), ".txt", StringComparison.OrdinalIgnoreCase))
                {
                    throw new UpdatableHelpSystemException("HelpContentContainsInvalidFiles", StringUtil.Format(HelpDisplayStrings.HelpContentContainsInvalidFiles, new object[0]), ErrorCategory.InvalidData, null, null);
                }
            }
            foreach (string str3 in Directory.GetFiles(sourcePath))
            {
                if (string.Equals(Path.GetExtension(str3), ".xml", StringComparison.OrdinalIgnoreCase))
                {
                    if (schema == null)
                    {
                        throw new ItemNotFoundException(StringUtil.Format(HelpDisplayStrings.HelpContentXsdNotFound, xsdPath));
                    }
                    XmlReader reader = XmlReader.Create(new StringReader(LoadStringFromPath(this._cmdlet, str3, null)));
                    XmlDocument document = new XmlDocument();
                    document.Load(reader);
                    if ((document.ChildNodes.Count != 1) && (document.ChildNodes.Count != 2))
                    {
                        throw new UpdatableHelpSystemException("HelpContentXmlValidationFailure", StringUtil.Format(HelpDisplayStrings.HelpContentXmlValidationFailure, HelpDisplayStrings.RootElementMustBeHelpItems), ErrorCategory.InvalidData, null, null);
                    }
                    System.Xml.XmlNode node = null;
                    if ((document.DocumentElement != null) && document.DocumentElement.LocalName.Equals("providerHelp", StringComparison.OrdinalIgnoreCase))
                    {
                        node = document;
                    }
                    else if (document.ChildNodes.Count == 1)
                    {
                        if (!document.ChildNodes[0].LocalName.Equals("helpItems", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new UpdatableHelpSystemException("HelpContentXmlValidationFailure", StringUtil.Format(HelpDisplayStrings.HelpContentXmlValidationFailure, HelpDisplayStrings.RootElementMustBeHelpItems), ErrorCategory.InvalidData, null, null);
                        }
                        node = document.ChildNodes[0];
                    }
                    else if (document.ChildNodes.Count == 2)
                    {
                        if (!document.ChildNodes[1].LocalName.Equals("helpItems", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new UpdatableHelpSystemException("HelpContentXmlValidationFailure", StringUtil.Format(HelpDisplayStrings.HelpContentXmlValidationFailure, HelpDisplayStrings.RootElementMustBeHelpItems), ErrorCategory.InvalidData, null, null);
                        }
                        node = document.ChildNodes[1];
                    }
                    string o = "http://schemas.microsoft.com/maml/2004/10";
                    foreach (System.Xml.XmlNode node2 in node.ChildNodes)
                    {
                        if (node2.NodeType == XmlNodeType.Element)
                        {
                            if (!node2.LocalName.Equals("providerHelp", StringComparison.OrdinalIgnoreCase))
                            {
                                if (node2.LocalName.Equals("para", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (!node2.NamespaceURI.Equals("http://schemas.microsoft.com/maml/2004/10", StringComparison.OrdinalIgnoreCase))
                                    {
                                        throw new UpdatableHelpSystemException("HelpContentXmlValidationFailure", StringUtil.Format(HelpDisplayStrings.HelpContentXmlValidationFailure, StringUtil.Format(HelpDisplayStrings.HelpContentMustBeInTargetNamespace, o)), ErrorCategory.InvalidData, null, null);
                                    }
                                    continue;
                                }
                                if (!node2.NamespaceURI.Equals("http://schemas.microsoft.com/maml/dev/command/2004/10", StringComparison.OrdinalIgnoreCase))
                                {
                                    throw new UpdatableHelpSystemException("HelpContentXmlValidationFailure", StringUtil.Format(HelpDisplayStrings.HelpContentXmlValidationFailure, StringUtil.Format(HelpDisplayStrings.HelpContentMustBeInTargetNamespace, o)), ErrorCategory.InvalidData, null, null);
                                }
                            }
                            this.CreateValidXmlDocument(node2.OuterXml, o, schema, new ValidationEventHandler(this.HelpContentValidationHandler), false);
                        }
                    }
                }
                else if (string.Equals(Path.GetExtension(str3), ".txt", StringComparison.OrdinalIgnoreCase))
                {
                    FileStream stream = new FileStream(str3, FileMode.Open, FileAccess.Read);
                    if (stream.Length > 2L)
                    {
                        byte[] buffer = new byte[2];
                        stream.Read(buffer, 0, 2);
                        if ((buffer[0] == 0x4d) && (buffer[1] == 90))
                        {
                            throw new UpdatableHelpSystemException("HelpContentContainsInvalidFiles", StringUtil.Format(HelpDisplayStrings.HelpContentContainsInvalidFiles, new object[0]), ErrorCategory.InvalidData, null, null);
                        }
                    }
                }
                foreach (string str6 in destPaths)
                {
                    string path = Path.Combine(str6, culture);
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    string str8 = Path.Combine(path, Path.GetFileName(str3));
                    System.IO.FileAttributes? nullable = null;
                    try
                    {
                        if (System.IO.File.Exists(str8) && (this._cmdlet.Force != false))
                        {
                            FileInfo info = new FileInfo(str8);
                            if ((info.Attributes & System.IO.FileAttributes.ReadOnly) == System.IO.FileAttributes.ReadOnly)
                            {
                                nullable = new System.IO.FileAttributes?(info.Attributes);
                                info.Attributes &= ~System.IO.FileAttributes.ReadOnly;
                            }
                        }
                        System.IO.File.Copy(str3, str8, true);
                    }
                    finally
                    {
                        if (nullable.HasValue)
                        {
                            System.IO.File.SetAttributes(str8, nullable.Value);
                        }
                    }
                    installed.Add(str8);
                }
            }
        }

        internal string CurrentModule
        {
            get
            {
                return this._currentModule;
            }
            set
            {
                this._currentModule = value;
            }
        }

        internal Collection<Exception> Errors
        {
            get
            {
                return this._errors;
            }
        }

        internal System.Net.WebClient WebClient
        {
            get
            {
                return this._webClient;
            }
        }

        
    }
}

