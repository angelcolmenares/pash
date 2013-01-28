namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;

    internal sealed class TableHeaderInfo : ShapeInfo
    {
        internal const string CLSID = "e3b7a39c089845d388b2e84c5d38f5dd";
        public bool hideHeader;
        public List<TableColumnInfo> tableColumnInfoList;

        public TableHeaderInfo() : base("e3b7a39c089845d388b2e84c5d38f5dd")
        {
            this.tableColumnInfoList = new List<TableColumnInfo>();
        }

        internal override void Deserialize(PSObject so, FormatObjectDeserializer deserializer)
        {
            base.Deserialize(so, deserializer);
            this.hideHeader = deserializer.DeserializeBoolMemberVariable(so, "hideHeader");
            FormatInfoDataListDeserializer<TableColumnInfo>.ReadList(so, "tableColumnInfoList", this.tableColumnInfoList, deserializer);
        }
    }
}

