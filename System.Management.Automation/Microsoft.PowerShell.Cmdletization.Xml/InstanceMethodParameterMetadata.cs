namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, GeneratedCode("xsd", "4.0.30319.17361"), DebuggerStepThrough, DesignerCategory("code"), XmlType(Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11")]
    internal class InstanceMethodParameterMetadata : CommonMethodParameterMetadata
    {
        private Microsoft.PowerShell.Cmdletization.Xml.CmdletOutputMetadata cmdletOutputMetadataField;
        private CmdletParameterMetadataForInstanceMethodParameter cmdletParameterMetadataField;

        public Microsoft.PowerShell.Cmdletization.Xml.CmdletOutputMetadata CmdletOutputMetadata
        {
            get
            {
                return this.cmdletOutputMetadataField;
            }
            set
            {
                this.cmdletOutputMetadataField = value;
            }
        }

        public CmdletParameterMetadataForInstanceMethodParameter CmdletParameterMetadata
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
    }
}

