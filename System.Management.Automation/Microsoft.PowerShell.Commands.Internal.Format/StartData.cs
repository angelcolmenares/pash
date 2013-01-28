namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation;

    internal abstract class StartData : ControlInfoData
    {
        public ShapeInfo shapeInfo;

        public StartData(string clsid) : base(clsid)
        {
        }

        internal override void Deserialize(PSObject so, FormatObjectDeserializer deserializer)
        {
            base.Deserialize(so, deserializer);
            this.shapeInfo = (ShapeInfo) deserializer.DeserializeMemberObject(so, "shapeInfo");
        }
    }
}

