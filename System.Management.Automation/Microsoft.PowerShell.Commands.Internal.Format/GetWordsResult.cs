namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct GetWordsResult
    {
        internal string Word;
        internal string Delim;
    }
}

