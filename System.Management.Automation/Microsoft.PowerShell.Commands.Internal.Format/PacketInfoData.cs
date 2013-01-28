namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal abstract class PacketInfoData : FormatInfoData
    {
        public PacketInfoData(string clsid) : base(clsid)
        {
        }
    }
}

