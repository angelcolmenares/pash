namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal abstract class ShapeInfo : FormatInfoData
    {
        public ShapeInfo(string clsid) : base(clsid)
        {
        }
    }
}

