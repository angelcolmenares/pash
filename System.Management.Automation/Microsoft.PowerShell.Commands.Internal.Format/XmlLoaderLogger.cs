namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Management.Automation;

    internal class XmlLoaderLogger : IDisposable
    {
        private List<XmlLoaderLoggerEntry> entries = new List<XmlLoaderLoggerEntry>();
        [TraceSource("FormatFileLoading", "Loading format files")]
        private static PSTraceSource formatFileLoadingtracer = PSTraceSource.GetTracer("FormatFileLoading", "Loading format files", false);
        private bool hasErrors;
        private bool saveInMemory = true;

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        internal void LogEntry(XmlLoaderLoggerEntry entry)
        {
            if (entry.entryType == XmlLoaderLoggerEntry.EntryType.Error)
            {
                this.hasErrors = true;
            }
            if (this.saveInMemory)
            {
                this.entries.Add(entry);
            }
            if ((formatFileLoadingtracer.Options | PSTraceSourceOptions.WriteLine) != PSTraceSourceOptions.None)
            {
                this.WriteToTracer(entry);
            }
        }

        private void WriteToTracer(XmlLoaderLoggerEntry entry)
        {
            if (entry.entryType == XmlLoaderLoggerEntry.EntryType.Error)
            {
                formatFileLoadingtracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "ERROR:\r\n FilePath: {0}\r\n XPath: {1}\r\n Message = {2}", new object[] { entry.filePath, entry.xPath, entry.message }), new object[0]);
            }
            else if (entry.entryType == XmlLoaderLoggerEntry.EntryType.Trace)
            {
                formatFileLoadingtracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "TRACE:\r\n FilePath: {0}\r\n XPath: {1}\r\n Message = {2}", new object[] { entry.filePath, entry.xPath, entry.message }), new object[0]);
            }
        }

        internal bool HasErrors
        {
            get
            {
                return this.hasErrors;
            }
        }

        internal List<XmlLoaderLoggerEntry> LogEntries
        {
            get
            {
                return this.entries;
            }
        }
    }
}

