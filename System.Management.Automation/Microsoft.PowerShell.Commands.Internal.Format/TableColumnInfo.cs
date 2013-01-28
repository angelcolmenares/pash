namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation;

    internal sealed class TableColumnInfo : FormatInfoData
    {
        public int alignment;
        internal const string CLSID = "7572aa4155ec4558817a615acf7dd92e";
        public string label;
        public string propertyName;
        public int width {
			get { return _width; }
			set 
			{ 
				_width = value; 
			}
		}

		private int _width;

        public TableColumnInfo() : base("7572aa4155ec4558817a615acf7dd92e")
        {
            this.alignment = 1;
        }

        internal override void Deserialize(PSObject so, FormatObjectDeserializer deserializer)
        {
            base.Deserialize(so, deserializer);
            this.width = deserializer.DeserializeIntMemberVariable(so, "width");
            this.alignment = deserializer.DeserializeIntMemberVariable(so, "alignment");
            this.label = deserializer.DeserializeStringMemberVariable(so, "label");
            this.propertyName = deserializer.DeserializeStringMemberVariable(so, "propertyName");
        }
    }
}

