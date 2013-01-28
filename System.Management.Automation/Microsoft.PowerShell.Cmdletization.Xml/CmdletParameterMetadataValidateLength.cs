namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, GeneratedCode("xsd", "4.0.30319.17361"), XmlType(AnonymousType=true, Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11"), DebuggerStepThrough, DesignerCategory("code")]
    internal class CmdletParameterMetadataValidateLength
    {
        private string maxField;
        private string minField;

        [XmlAttribute(DataType="nonNegativeInteger")]
        public string Max
        {
            get
            {
                return this.maxField;
            }
            set
            {
                this.maxField = value;
            }
        }

        [XmlAttribute(DataType="nonNegativeInteger")]
        public string Min
        {
            get
            {
                return this.minField;
            }
            set
            {
                this.minField = value;
            }
        }
    }
}

