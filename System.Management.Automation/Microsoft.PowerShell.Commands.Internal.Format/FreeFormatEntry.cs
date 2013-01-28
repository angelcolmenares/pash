namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;

    internal abstract class FreeFormatEntry : FormatEntryInfo
    {
        public List<FormatValue> formatValueList;

        public FreeFormatEntry(string clsid) : base(clsid)
        {
            this.formatValueList = new List<FormatValue>();
        }

        internal override void Deserialize(PSObject so, FormatObjectDeserializer deserializer)
        {
            base.Deserialize(so, deserializer);
            FormatInfoDataListDeserializer<FormatValue>.ReadList(so, "formatValueList", this.formatValueList, deserializer);
        }
    }
}

