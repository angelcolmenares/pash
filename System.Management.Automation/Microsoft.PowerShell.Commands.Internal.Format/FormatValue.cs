namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal abstract class FormatValue : FormatInfoData
    {
        public FormatValue(string clsid) : base(clsid)
        {
        }
    }
}

