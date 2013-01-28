namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.ObjectModel;

    internal sealed class XmlFileLoadInfo
    {
        internal Collection<string> errors;
        internal string fileDirectory;
        internal string filePath;
        internal string psSnapinName;

        internal XmlFileLoadInfo()
        {
        }

        internal XmlFileLoadInfo(string dir, string path, Collection<string> errors, string psSnapinName)
        {
            this.fileDirectory = dir;
            this.filePath = path;
            this.errors = errors;
            this.psSnapinName = psSnapinName;
        }
    }
}

