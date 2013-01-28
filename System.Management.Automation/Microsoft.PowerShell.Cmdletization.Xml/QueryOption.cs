namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, DebuggerStepThrough, XmlType(Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11"), GeneratedCode("xsd", "4.0.30319.17361"), DesignerCategory("code")]
    internal class QueryOption
    {
        private CmdletParameterMetadataForGetCmdletParameter cmdletParameterMetadataField;
        private string optionNameField;
        private TypeMetadata typeField;

        public CmdletParameterMetadataForGetCmdletParameter CmdletParameterMetadata
        {
            get
            {
                return this.cmdletParameterMetadataField;
            }
            set
            {
                this.cmdletParameterMetadataField = value;
            }
        }

        [XmlAttribute]
        public string OptionName
        {
            get
            {
                return this.optionNameField;
            }
            set
            {
                this.optionNameField = value;
            }
        }

        public TypeMetadata Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }
    }
}

