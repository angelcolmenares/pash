namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, XmlType(Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11"), GeneratedCode("xsd", "4.0.30319.17361"), DebuggerStepThrough, DesignerCategory("code")]
    internal class StaticMethodParameterMetadata : CommonMethodParameterMetadata
    {
        private Microsoft.PowerShell.Cmdletization.Xml.CmdletOutputMetadata cmdletOutputMetadataField;
        private CmdletParameterMetadataForStaticMethodParameter cmdletParameterMetadataField;

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

        public CmdletParameterMetadataForStaticMethodParameter CmdletParameterMetadata
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

