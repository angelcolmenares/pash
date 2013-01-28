namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal sealed class DatabaseLoadingInfo
    {
        internal string fileDirectory;
        internal string filePath;
        internal bool isFullyTrusted;
        internal DateTime loadTime = DateTime.Now;
        internal string xPath;
    }
}

