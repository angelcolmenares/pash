namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation;

    internal sealed class FormatEntryData : PacketInfoData
    {
        internal const string CLSID = "27c87ef9bbda4f709f6b4002fa4af63c";
        public FormatEntryInfo formatEntryInfo;
        internal bool isHelpObject;
        public bool outOfBand;
        public WriteStreamType writeStream;

        public FormatEntryData() : base("27c87ef9bbda4f709f6b4002fa4af63c")
        {
        }

        internal override void Deserialize(PSObject so, FormatObjectDeserializer deserializer)
        {
            base.Deserialize(so, deserializer);
            this.formatEntryInfo = (FormatEntryInfo) deserializer.DeserializeMandatoryMemberObject(so, "formatEntryInfo");
            this.outOfBand = deserializer.DeserializeBoolMemberVariable(so, "outOfBand");
            this.writeStream = deserializer.DeserializeWriteStreamTypeMemberVariable(so);
            this.isHelpObject = so.IsHelpObject;
        }

        internal void SetStreamTypeFromPSObject(PSObject so)
        {
            if (PSObjectHelper.IsWriteErrorStream(so))
            {
                this.writeStream = WriteStreamType.Error;
            }
            else if (PSObjectHelper.IsWriteWarningStream(so))
            {
                this.writeStream = WriteStreamType.Warning;
            }
            else if (PSObjectHelper.IsWriteVerboseStream(so))
            {
                this.writeStream = WriteStreamType.Verbose;
            }
            else if (PSObjectHelper.IsWriteDebugStream(so))
            {
                this.writeStream = WriteStreamType.Debug;
            }
            else
            {
                this.writeStream = WriteStreamType.None;
            }
        }
    }
}

