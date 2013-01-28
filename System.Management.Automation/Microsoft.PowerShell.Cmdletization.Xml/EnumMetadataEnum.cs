namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, XmlType(AnonymousType=true, Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11"), GeneratedCode("xsd", "4.0.30319.17361"), DebuggerStepThrough, DesignerCategory("code")]
    internal class EnumMetadataEnum
    {
        private bool bitwiseFlagsField;
        private bool bitwiseFlagsFieldSpecified;
        private string enumNameField;
        private string underlyingTypeField;
        private EnumMetadataEnumValue[] valueField;

        [XmlAttribute]
        public bool BitwiseFlags
        {
            get
            {
                return this.bitwiseFlagsField;
            }
            set
            {
                this.bitwiseFlagsField = value;
            }
        }

        [XmlIgnore]
        public bool BitwiseFlagsSpecified
        {
            get
            {
                return this.bitwiseFlagsFieldSpecified;
            }
            set
            {
                this.bitwiseFlagsFieldSpecified = value;
            }
        }

        [XmlAttribute]
        public string EnumName
        {
            get
            {
                return this.enumNameField;
            }
            set
            {
                this.enumNameField = value;
            }
        }

        [XmlAttribute]
        public string UnderlyingType
        {
            get
            {
                return this.underlyingTypeField;
            }
            set
            {
                this.underlyingTypeField = value;
            }
        }

        [XmlElement("Value")]
        public EnumMetadataEnumValue[] Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }
}

