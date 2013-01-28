namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.IO;
    using System.Management.Automation;

    internal static class InputFileOpenModeConversion
    {
        internal static FileMode Convert(OpenMode openMode)
        {
            return SessionStateUtilities.GetFileModeFromOpenMode(openMode);
        }
    }
}

