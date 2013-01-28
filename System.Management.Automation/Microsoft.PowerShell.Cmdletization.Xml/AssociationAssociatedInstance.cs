namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, DebuggerStepThrough, GeneratedCode("xsd", "4.0.30319.17361"), DesignerCategory("code"), XmlType(AnonymousType=true, Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11")]
    internal class AssociationAssociatedInstance
    {
        private CmdletParameterMetadataForGetCmdletFilteringParameter cmdletParameterMetadataField;
        private TypeMetadata typeField;

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

