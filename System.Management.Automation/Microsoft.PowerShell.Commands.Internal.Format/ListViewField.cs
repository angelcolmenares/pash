namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation;

    internal sealed class ListViewField : FormatInfoData
    {
        internal const string CLSID = "b761477330ce4fb2a665999879324d73";
        public FormatPropertyField formatPropertyField;
        public string label;
        public string propertyName;

        public ListViewField() : base("b761477330ce4fb2a665999879324d73")
        {
            this.formatPropertyField = new FormatPropertyField();
        }

        internal override void Deserialize(PSObject so, FormatObjectDeserializer deserializer)
        {
            base.Deserialize(so, deserializer);
            this.label = deserializer.DeserializeStringMemberVariable(so, "label");
            this.propertyName = deserializer.DeserializeStringMemberVariable(so, "propertyName");
            this.formatPropertyField = (FormatPropertyField) deserializer.DeserializeMandatoryMemberObject(so, "formatPropertyField");
        }
    }
}

