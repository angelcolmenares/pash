namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation;

    internal sealed class WideViewHeaderInfo : ShapeInfo
    {
        internal const string CLSID = "b2e2775d33d544c794d0081f27021b5c";
        public int columns;

        public WideViewHeaderInfo() : base("b2e2775d33d544c794d0081f27021b5c")
        {
        }

        internal override void Deserialize(PSObject so, FormatObjectDeserializer deserializer)
        {
            base.Deserialize(so, deserializer);
            this.columns = deserializer.DeserializeIntMemberVariable(so, "columns");
        }
    }
}

