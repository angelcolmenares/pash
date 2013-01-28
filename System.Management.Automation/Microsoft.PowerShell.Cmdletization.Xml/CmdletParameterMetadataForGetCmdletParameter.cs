namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, XmlInclude(typeof(CmdletParameterMetadataForGetCmdletFilteringParameter)), GeneratedCode("xsd", "4.0.30319.17361"), DebuggerStepThrough, DesignerCategory("code"), XmlType(Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11")]
    internal class CmdletParameterMetadataForGetCmdletParameter : CmdletParameterMetadata
    {
        private string[] cmdletParameterSetsField;
        private bool valueFromPipelineByPropertyNameField;
        private bool valueFromPipelineByPropertyNameFieldSpecified;
        private bool valueFromPipelineField;
        private bool valueFromPipelineFieldSpecified;

        [XmlAttribute]
        public string[] CmdletParameterSets
        {
            get
            {
                return this.cmdletParameterSetsField;
            }
            set
            {
                this.cmdletParameterSetsField = value;
            }
        }

        [XmlAttribute]
        public bool ValueFromPipeline
        {
            get
            {
                return this.valueFromPipelineField;
            }
            set
            {
                this.valueFromPipelineField = value;
            }
        }

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

        [XmlIgnore]
        public bool ValueFromPipelineSpecified
        {
            get
            {
                return this.valueFromPipelineFieldSpecified;
            }
            set
            {
                this.valueFromPipelineFieldSpecified = value;
            }
        }
    }
}

