namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation;

    internal sealed class FormatTextField : FormatValue
    {
        internal const string CLSID = "b8d9e369024a43a580b9e0c9279e3354";
        public string text;

        public FormatTextField() : base("b8d9e369024a43a580b9e0c9279e3354")
        {
        }

        internal override void Deserialize(PSObject so, FormatObjectDeserializer deserializer)
        {
            base.Deserialize(so, deserializer);
            this.text = deserializer.DeserializeStringMemberVariable(so, "text");
        }
    }
}

