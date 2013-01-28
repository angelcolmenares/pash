namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;

    internal abstract class XmlLoaderBase : IDisposable
    {
        private int currentErrorCount;
        protected DisplayResourceManagerCache displayResourceManagerCache;
        private Stack executionStack = new Stack();
        protected MshExpressionFactory expressionFactory;
        private DatabaseLoadingInfo loadingInfo = new DatabaseLoadingInfo();
        private XmlLoaderLogger logger = new XmlLoaderLogger();
        private bool logStackActivity;
        private int maxNumberOfErrors = 30;
        [TraceSource("XmlLoaderBase", "XmlLoaderBase")]
        private static PSTraceSource tracer = PSTraceSource.GetTracer("XmlLoaderBase", "XmlLoaderBase");
        private bool verifyStringResources = true;

        protected XmlLoaderBase()
        {
        }

        protected string ComputeCurrentXPath()
        {
            StringBuilder builder = new StringBuilder();
            foreach (XmlLoaderStackFrame frame in this.executionStack)
            {
                builder.Insert(0, "/");
                if (frame.index != -1)
                {
                    builder.Insert(1, string.Format(CultureInfo.InvariantCulture, "{0}[{1}]", new object[] { frame.node.Name, frame.index + 1 }));
                }
                else
                {
                    builder.Insert(1, frame.node.Name);
                }
            }
            if (builder.Length <= 0)
            {
                return null;
            }
            return builder.ToString();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && (this.logger != null))
            {
                this.logger.Dispose();
                this.logger = null;
            }
        }

        internal string GetMandatoryAttributeValue(System.Xml.XmlAttribute a)
        {
            if (string.IsNullOrEmpty(a.Value))
            {
                this.ReportEmptyAttribute(a);
                return null;
            }
            return a.Value;
        }

        internal string GetMandatoryInnerText(System.Xml.XmlNode n)
        {
            if (string.IsNullOrEmpty(n.InnerText))
            {
                this.ReportEmptyNode(n);
                return null;
            }
            return n.InnerText;
        }

        protected static bool IsFilteredOutNode(System.Xml.XmlNode n)
        {
            return ((n is XmlComment) || (n is XmlWhitespace));
        }

        protected XmlDocument LoadXmlDocumentFromFileLoadingInfo(AuthorizationManager authorizationManager, PSHost host, out bool isFullyTrusted)
        {
            XmlDocument document2;
            ExternalScriptInfo commandInfo = new ExternalScriptInfo(this.FilePath, this.FilePath);
            string scriptContents = commandInfo.ScriptContents;
            isFullyTrusted = false;
            if (((PSLanguageMode) commandInfo.DefiningLanguageMode) == PSLanguageMode.FullLanguage)
            {
                isFullyTrusted = true;
            }
            if (authorizationManager != null)
            {
                try
                {
                    authorizationManager.ShouldRunInternal(commandInfo, CommandOrigin.Internal, host);
                }
                catch (PSSecurityException exception)
                {
                    string message = StringUtil.Format(TypesXmlStrings.ValidationException, new object[] { string.Empty, this.FilePath, exception.Message });
                    this.ReportLogEntryHelper(message, XmlLoaderLoggerEntry.EntryType.Error, true);
                    return null;
                }
            }
            try
            {
                XmlDocument document = InternalDeserializer.LoadUnsafeXmlDocument(scriptContents, true, null);
                this.ReportTrace("XmlDocument loaded OK");
                document2 = document;
            }
            catch (XmlException exception2)
            {
                this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.ErrorInFile, this.FilePath, exception2.Message));
                this.ReportTrace("XmlDocument discarded");
                document2 = null;
            }
            catch (Exception exception3)
            {
                CommandProcessorBase.CheckForSevereException(exception3);
                throw;
            }
            return document2;
        }

        internal bool MatchAttributeName(System.Xml.XmlAttribute a, string s)
        {
            if (string.Equals(a.Name, s, StringComparison.Ordinal))
            {
                return true;
            }
            if (string.Equals(a.Name, s, StringComparison.OrdinalIgnoreCase))
            {
                string format = "XML attribute differ in case only {0} {1}";
                this.ReportTrace(string.Format(CultureInfo.InvariantCulture, format, new object[] { a.Name, s }));
                return true;
            }
            return false;
        }

        internal bool MatchNodeName(System.Xml.XmlNode n, string s)
        {
            return this.MatchNodeNameHelper(n, s, false);
        }

        private bool MatchNodeNameHelper(System.Xml.XmlNode n, string s, bool allowAttributes)
        {
            bool flag = false;
            if (string.Equals(n.Name, s, StringComparison.Ordinal))
            {
                flag = true;
            }
            else if (string.Equals(n.Name, s, StringComparison.OrdinalIgnoreCase))
            {
                string format = "XML tag differ in case only {0} {1}";
                this.ReportTrace(string.Format(CultureInfo.InvariantCulture, format, new object[] { n.Name, s }));
                flag = true;
            }
            if (flag && !allowAttributes)
            {
                XmlElement element = n as XmlElement;
                if ((element != null) && (element.Attributes.Count > 0))
                {
                    this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.AttributesNotAllowed, new object[] { this.ComputeCurrentXPath(), this.FilePath, n.Name }));
                }
            }
            return flag;
        }

        internal bool MatchNodeNameWithAttributes(System.Xml.XmlNode n, string s)
        {
            return this.MatchNodeNameHelper(n, s, true);
        }

        internal void ProcessDuplicateAlternateNode(string node1, string node2)
        {
            this.ReportLogEntryHelper(StringUtil.Format(FormatAndOutXmlLoadingStrings.MutuallyExclusiveNode, new object[] { this.ComputeCurrentXPath(), this.FilePath, node1, node2 }), XmlLoaderLoggerEntry.EntryType.Error, false);
        }

        internal void ProcessDuplicateAlternateNode(System.Xml.XmlNode n, string node1, string node2)
        {
            this.ReportLogEntryHelper(StringUtil.Format(FormatAndOutXmlLoadingStrings.ThreeMutuallyExclusiveNode, new object[] { this.ComputeCurrentXPath(), this.FilePath, n.Name, node1, node2 }), XmlLoaderLoggerEntry.EntryType.Error, false);
        }

        internal void ProcessDuplicateNode(System.Xml.XmlNode n)
        {
            this.ReportLogEntryHelper(StringUtil.Format(FormatAndOutXmlLoadingStrings.DuplicatedNode, this.ComputeCurrentXPath(), this.FilePath), XmlLoaderLoggerEntry.EntryType.Error, false);
        }

        protected void ProcessUnknownAttribute(System.Xml.XmlAttribute a)
        {
            this.ReportIllegalXmlAttribute(a);
        }

        protected void ProcessUnknownNode(System.Xml.XmlNode n)
        {
            if (!IsFilteredOutNode(n))
            {
                this.ReportIllegalXmlNode(n);
            }
        }

        private void RemoveStackFrame()
        {
            if (this.logStackActivity)
            {
                this.WriteStackLocation("Exit");
            }
            this.executionStack.Pop();
        }

        protected void ReportEmptyAttribute(System.Xml.XmlAttribute a)
        {
            this.ReportLogEntryHelper(StringUtil.Format(FormatAndOutXmlLoadingStrings.EmptyAttribute, new object[] { this.ComputeCurrentXPath(), this.FilePath, a.Name }), XmlLoaderLoggerEntry.EntryType.Error, false);
        }

        protected void ReportEmptyNode(System.Xml.XmlNode n)
        {
            this.ReportLogEntryHelper(StringUtil.Format(FormatAndOutXmlLoadingStrings.EmptyNode, new object[] { this.ComputeCurrentXPath(), this.FilePath, n.Name }), XmlLoaderLoggerEntry.EntryType.Error, false);
        }

        protected void ReportError(string message)
        {
            this.ReportLogEntryHelper(message, XmlLoaderLoggerEntry.EntryType.Error, false);
        }

        protected void ReportErrorForLoadingFromObjectModel(string message, string typeName)
        {
            XmlLoaderLoggerEntry entry = new XmlLoaderLoggerEntry {
                entryType = XmlLoaderLoggerEntry.EntryType.Error,
                message = message
            };
            this.logger.LogEntry(entry);
            this.currentErrorCount++;
            if (this.currentErrorCount >= this.maxNumberOfErrors)
            {
                if (this.maxNumberOfErrors > 1)
                {
                    XmlLoaderLoggerEntry entry2 = new XmlLoaderLoggerEntry {
                        entryType = XmlLoaderLoggerEntry.EntryType.Error,
                        message = StringUtil.Format(FormatAndOutXmlLoadingStrings.TooManyErrorsInFormattingData, typeName)
                    };
                    this.logger.LogEntry(entry2);
                    this.currentErrorCount++;
                }
                TooManyErrorsException exception = new TooManyErrorsException {
                    errorCount = this.currentErrorCount
                };
                throw exception;
            }
        }

        private void ReportIllegalXmlAttribute(System.Xml.XmlAttribute a)
        {
            this.ReportLogEntryHelper(StringUtil.Format(FormatAndOutXmlLoadingStrings.UnknownAttribute, new object[] { this.ComputeCurrentXPath(), this.FilePath, a.Name }), XmlLoaderLoggerEntry.EntryType.Error, false);
        }

        private void ReportIllegalXmlNode(System.Xml.XmlNode n)
        {
            this.ReportLogEntryHelper(StringUtil.Format(FormatAndOutXmlLoadingStrings.UnknownNode, new object[] { this.ComputeCurrentXPath(), this.FilePath, n.Name }), XmlLoaderLoggerEntry.EntryType.Error, false);
        }

        private void ReportLogEntryHelper(string message, XmlLoaderLoggerEntry.EntryType entryType, bool failToLoadFile = false)
        {
            string str = this.ComputeCurrentXPath();
            XmlLoaderLoggerEntry entry = new XmlLoaderLoggerEntry {
                entryType = entryType,
                filePath = this.FilePath,
                xPath = str,
                message = message
            };
            if (failToLoadFile)
            {
                entry.failToLoadFile = true;
            }
            this.logger.LogEntry(entry);
            if (entryType == XmlLoaderLoggerEntry.EntryType.Error)
            {
                this.currentErrorCount++;
                if (this.currentErrorCount >= this.maxNumberOfErrors)
                {
                    if (this.maxNumberOfErrors > 1)
                    {
                        XmlLoaderLoggerEntry entry2 = new XmlLoaderLoggerEntry {
                            entryType = XmlLoaderLoggerEntry.EntryType.Error,
                            filePath = this.FilePath,
                            xPath = str,
                            message = StringUtil.Format(FormatAndOutXmlLoadingStrings.TooManyErrors, this.FilePath)
                        };
                        this.logger.LogEntry(entry2);
                        this.currentErrorCount++;
                    }
                    TooManyErrorsException exception = new TooManyErrorsException {
                        errorCount = this.currentErrorCount
                    };
                    throw exception;
                }
            }
        }

        protected void ReportMissingAttribute(string name)
        {
            this.ReportLogEntryHelper(StringUtil.Format(FormatAndOutXmlLoadingStrings.MissingAttribute, new object[] { this.ComputeCurrentXPath(), this.FilePath, name }), XmlLoaderLoggerEntry.EntryType.Error, false);
        }

        protected void ReportMissingNode(string name)
        {
            this.ReportLogEntryHelper(StringUtil.Format(FormatAndOutXmlLoadingStrings.MissingNode, new object[] { this.ComputeCurrentXPath(), this.FilePath, name }), XmlLoaderLoggerEntry.EntryType.Error, false);
        }

        protected void ReportMissingNodes(string[] names)
        {
            string str = string.Join(", ", names);
            this.ReportLogEntryHelper(StringUtil.Format(FormatAndOutXmlLoadingStrings.MissingNodeFromList, new object[] { this.ComputeCurrentXPath(), this.FilePath, str }), XmlLoaderLoggerEntry.EntryType.Error, false);
        }

        protected void ReportTrace(string message)
        {
            this.ReportLogEntryHelper(message, XmlLoaderLoggerEntry.EntryType.Trace, false);
        }

        protected void SetDatabaseLoadingInfo(XmlFileLoadInfo info)
        {
            this.loadingInfo.fileDirectory = info.fileDirectory;
            this.loadingInfo.filePath = info.filePath;
        }

        protected void SetLoadingInfoIsFullyTrusted(bool isFullyTrusted)
        {
            this.loadingInfo.isFullyTrusted = isFullyTrusted;
        }

        protected IDisposable StackFrame(System.Xml.XmlNode n)
        {
            return this.StackFrame(n, -1);
        }

        protected IDisposable StackFrame(System.Xml.XmlNode n, int index)
        {
            XmlLoaderStackFrame frame = new XmlLoaderStackFrame(this, n, index);
            this.executionStack.Push(frame);
            if (this.logStackActivity)
            {
                this.WriteStackLocation("Enter");
            }
            return frame;
        }

        protected bool VerifyNodeHasNoChildren(System.Xml.XmlNode n)
        {
            if (n.ChildNodes.Count == 0)
            {
                return true;
            }
            if ((n.ChildNodes.Count == 1) && (n.ChildNodes[0] is XmlText))
            {
                return true;
            }
            this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.NoChildrenAllowed, new object[] { this.ComputeCurrentXPath(), this.FilePath, n.Name }));
            return false;
        }

        private void WriteStackLocation(string label)
        {
            this.ReportTrace(label);
        }

        protected string FilePath
        {
            get
            {
                return this.loadingInfo.filePath;
            }
        }

        internal bool HasErrors
        {
            get
            {
                return this.logger.HasErrors;
            }
        }

        protected DatabaseLoadingInfo LoadingInfo
        {
            get
            {
                return new DatabaseLoadingInfo { filePath = this.loadingInfo.filePath, fileDirectory = this.loadingInfo.fileDirectory, isFullyTrusted = this.loadingInfo.isFullyTrusted };
            }
        }

        internal List<XmlLoaderLoggerEntry> LogEntries
        {
            get
            {
                return this.logger.LogEntries;
            }
        }

        internal bool VerifyStringResources
        {
            get
            {
                return this.verifyStringResources;
            }
        }

        private sealed class XmlLoaderStackFrame : IDisposable
        {
            internal int index = -1;
            private XmlLoaderBase loader;
            internal System.Xml.XmlNode node;

            internal XmlLoaderStackFrame(XmlLoaderBase loader, System.Xml.XmlNode n, int index)
            {
                this.loader = loader;
                this.node = n;
                this.index = index;
            }

            public void Dispose()
            {
                if (this.loader != null)
                {
                    this.loader.RemoveStackFrame();
                    this.loader = null;
                }
            }
        }
    }
}

