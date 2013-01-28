namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, GeneratedCode("xsd", "4.0.30319.17361"), DesignerCategory("code"), DebuggerStepThrough, XmlType(Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11")]
    internal class CmdletParameterMetadataForInstanceMethodParameter : CmdletParameterMetadata
    {
        private bool valueFromPipelineByPropertyNameField;
        private bool valueFromPipelineByPropertyNameFieldSpecified;

        [XmlAttribute]
        public bool ValueFromPipelineByPropertyName
        {
            get
            {
                return this.valueFromPipelineByPropertyNameField;
            }
            set
            {
                this.valueFromPipelineByPropertyNameField = value;
            }
        }

        [XmlIgnore]
        public bool ValueFromPipelineByPropertyNameSpecified
        {
            get
            {
                return this.valueFromPipelineByPropertyNameFieldSpecified;
            }
            set
            {
                this.valueFromPipelineByPropertyNameFieldSpecified = value;
            }
        }
    }
}

