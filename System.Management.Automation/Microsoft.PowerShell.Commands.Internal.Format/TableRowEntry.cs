namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;

    internal sealed class TableRowEntry : FormatEntryInfo
    {
        internal const string CLSID = "0e59526e2dd441aa91e7fc952caf4a36";
        public List<FormatPropertyField> formatPropertyFieldList;
        public bool multiLine;

        public TableRowEntry() : base("0e59526e2dd441aa91e7fc952caf4a36")
        {
            this.formatPropertyFieldList = new List<FormatPropertyField>();
        }

        internal override void Deserialize(PSObject so, FormatObjectDeserializer deserializer)
        {
            base.Deserialize(so, deserializer);
            FormatInfoDataListDeserializer<FormatPropertyField>.ReadList(so, "formatPropertyFieldList", this.formatPropertyFieldList, deserializer);
            this.multiLine = deserializer.DeserializeBoolMemberVariable(so, "multiLine");
        }
    }
}

