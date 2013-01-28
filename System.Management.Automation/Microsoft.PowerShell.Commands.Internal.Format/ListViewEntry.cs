namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;

    internal sealed class ListViewEntry : FormatEntryInfo
    {
        internal const string CLSID = "cf58f450baa848ef8eb3504008be6978";
        public List<ListViewField> listViewFieldList;

        public ListViewEntry() : base("cf58f450baa848ef8eb3504008be6978")
        {
            this.listViewFieldList = new List<ListViewField>();
        }

        internal override void Deserialize(PSObject so, FormatObjectDeserializer deserializer)
        {
            base.Deserialize(so, deserializer);
            FormatInfoDataListDeserializer<ListViewField>.ReadList(so, "listViewFieldList", this.listViewFieldList, deserializer);
        }
    }
}

