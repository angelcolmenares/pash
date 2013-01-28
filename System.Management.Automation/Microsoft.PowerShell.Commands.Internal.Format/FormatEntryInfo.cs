namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal abstract class FormatEntryInfo : FormatInfoData
    {
        public FormatEntryInfo(string clsid) : base(clsid)
        {
        }
    }
}

