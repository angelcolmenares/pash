namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation;

    internal abstract class ControlInfoData : PacketInfoData
    {
        public GroupingEntry groupingEntry;

        public ControlInfoData(string clsid) : base(clsid)
        {
        }

        internal override void Deserialize(PSObject so, FormatObjectDeserializer deserializer)
        {
            base.Deserialize(so, deserializer);
            this.groupingEntry = (GroupingEntry) deserializer.DeserializeMemberObject(so, "groupingEntry");
        }
    }
}

