namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;

    internal sealed class FormatEntry : FormatValue
    {
        internal const string CLSID = "fba029a113a5458d932a2ed4871fadf2";
        public List<FormatValue> formatValueList;
        public FrameInfo frameInfo;

        public FormatEntry() : base("fba029a113a5458d932a2ed4871fadf2")
        {
            this.formatValueList = new List<FormatValue>();
        }

        internal override void Deserialize(PSObject so, FormatObjectDeserializer deserializer)
        {
            base.Deserialize(so, deserializer);
            FormatInfoDataListDeserializer<FormatValue>.ReadList(so, "formatValueList", this.formatValueList, deserializer);
            this.frameInfo = (FrameInfo) deserializer.DeserializeMemberObject(so, "frameInfo");
        }
    }
}

