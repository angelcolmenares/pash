namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation;

    internal sealed class RawTextFormatEntry : FormatEntryInfo
    {
        internal const string CLSID = "29ED81BA914544d4BC430F027EE053E9";
        public string text;

        public RawTextFormatEntry() : base("29ED81BA914544d4BC430F027EE053E9")
        {
        }

        internal override void Deserialize(PSObject so, FormatObjectDeserializer deserializer)
        {
            base.Deserialize(so, deserializer);
            this.text = deserializer.DeserializeStringMemberVariableRaw(so, "text");
        }
    }
}

