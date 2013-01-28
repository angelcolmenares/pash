namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal class XmlLoaderLoggerEntry
    {
        internal EntryType entryType;
        internal bool failToLoadFile;
        internal string filePath;
        internal string message;
        internal string xPath;

        internal enum EntryType
        {
            Error,
            Trace
        }
    }
}

