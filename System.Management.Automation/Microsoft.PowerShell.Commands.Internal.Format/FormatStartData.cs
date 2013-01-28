namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation;

    internal sealed class FormatStartData : StartData
    {
        public AutosizeInfo autosizeInfo;
        internal const string CLSID = "033ecb2bc07a4d43b5ef94ed5a35d280";
        public PageFooterEntry pageFooterEntry;
        public PageHeaderEntry pageHeaderEntry;

        public FormatStartData() : base("033ecb2bc07a4d43b5ef94ed5a35d280")
        {
        }

        internal override void Deserialize(PSObject so, FormatObjectDeserializer deserializer)
        {
            base.Deserialize(so, deserializer);
            deserializer.VerifyDataNotNull(base.shapeInfo, "shapeInfo");
            this.pageHeaderEntry = (PageHeaderEntry) deserializer.DeserializeMemberObject(so, "pageHeaderEntry");
            this.pageFooterEntry = (PageFooterEntry) deserializer.DeserializeMemberObject(so, "pageFooterEntry");
            this.autosizeInfo = (AutosizeInfo) deserializer.DeserializeMemberObject(so, "autosizeInfo");
        }
    }
}

