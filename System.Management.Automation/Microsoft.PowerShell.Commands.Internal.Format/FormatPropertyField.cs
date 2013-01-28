namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation;

    internal sealed class FormatPropertyField : FormatValue
    {
        public int alignment;
        internal const string CLSID = "78b102e894f742aca8c1d6737b6ff86a";
        public string propertyValue;

        public FormatPropertyField() : base("78b102e894f742aca8c1d6737b6ff86a")
        {
        }

        internal override void Deserialize(PSObject so, FormatObjectDeserializer deserializer)
        {
            base.Deserialize(so, deserializer);
            this.propertyValue = deserializer.DeserializeStringMemberVariable(so, "propertyValue");
            this.alignment = deserializer.DeserializeIntMemberVariable(so, "alignment");
        }
    }
}

