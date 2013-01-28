namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation;

    internal sealed class FrameInfo : FormatInfoData
    {
        internal const string CLSID = "091C9E762E33499eBE318901B6EFB733";
        public int firstLine;
        public int leftIndentation;
        public int rightIndentation;

        public FrameInfo() : base("091C9E762E33499eBE318901B6EFB733")
        {
        }

        internal override void Deserialize(PSObject so, FormatObjectDeserializer deserializer)
        {
            base.Deserialize(so, deserializer);
            this.leftIndentation = deserializer.DeserializeIntMemberVariable(so, "leftIndentation");
            this.rightIndentation = deserializer.DeserializeIntMemberVariable(so, "rightIndentation");
            this.firstLine = deserializer.DeserializeIntMemberVariable(so, "firstLine");
        }
    }
}

