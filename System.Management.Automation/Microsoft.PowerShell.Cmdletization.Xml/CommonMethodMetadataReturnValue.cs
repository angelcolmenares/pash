namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, XmlType(AnonymousType=true, Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11"), DesignerCategory("code"), GeneratedCode("xsd", "4.0.30319.17361"), DebuggerStepThrough]
    internal class CommonMethodMetadataReturnValue
    {
        private Microsoft.PowerShell.Cmdletization.Xml.CmdletOutputMetadata cmdletOutputMetadataField;
        private TypeMetadata typeField;

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

