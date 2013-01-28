namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation;

    internal sealed class AutosizeInfo : FormatInfoData
    {
        internal const string CLSID = "a27f094f0eec4d64845801a4c06a32ae";
        public int objectCount;

        public AutosizeInfo() : base("a27f094f0eec4d64845801a4c06a32ae")
        {
        }

        internal override void Deserialize(PSObject so, FormatObjectDeserializer deserializer)
        {
            base.Deserialize(so, deserializer);
            this.objectCount = deserializer.DeserializeIntMemberVariable(so, "objectCount");
        }
    }
}

