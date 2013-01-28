namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation;

    internal sealed class WideViewEntry : FormatEntryInfo
    {
        internal const string CLSID = "59bf79de63354a7b9e4d1697940ff188";
        public FormatPropertyField formatPropertyField;

        public WideViewEntry() : base("59bf79de63354a7b9e4d1697940ff188")
        {
            this.formatPropertyField = new FormatPropertyField();
        }

        internal override void Deserialize(PSObject so, FormatObjectDeserializer deserializer)
        {
            base.Deserialize(so, deserializer);
            this.formatPropertyField = (FormatPropertyField) deserializer.DeserializeMandatoryMemberObject(so, "formatPropertyField");
        }
    }
}

