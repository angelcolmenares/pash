namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, GeneratedCode("xsd", "4.0.30319.17361"), DebuggerStepThrough, DesignerCategory("code"), XmlType(AnonymousType=true, Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11")]
    internal class CmdletParameterMetadataValidateRange
    {
        private string maxField;
        private string minField;

        [XmlAttribute(DataType="integer")]
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

        [XmlAttribute(DataType="integer")]
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

