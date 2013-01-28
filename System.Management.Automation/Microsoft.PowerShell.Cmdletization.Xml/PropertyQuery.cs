namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, DebuggerStepThrough, GeneratedCode("xsd", "4.0.30319.17361"), XmlInclude(typeof(WildcardablePropertyQuery)), DesignerCategory("code"), XmlType(Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11")]
    internal class PropertyQuery
    {
        private CmdletParameterMetadataForGetCmdletFilteringParameter cmdletParameterMetadataField;

        public CmdletParameterMetadataForGetCmdletFilteringParameter CmdletParameterMetadata
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

