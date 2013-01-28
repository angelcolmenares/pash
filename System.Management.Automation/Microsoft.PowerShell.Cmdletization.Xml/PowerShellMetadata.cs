namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, GeneratedCode("xsd", "4.0.30319.17361"), XmlType(AnonymousType=true, Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11"), XmlRoot(Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11", IsNullable=false), DebuggerStepThrough, DesignerCategory("code")]
    internal class PowerShellMetadata
    {
        private ClassMetadata classField;
        private EnumMetadataEnum[] enumsField;

        public ClassMetadata Class
        {
            get
            {
                return this.classField;
            }
            set
            {
                this.classField = value;
            }
        }

        [XmlArrayItem("Enum", IsNullable=false)]
        public EnumMetadataEnum[] Enums
        {
            get
            {
                return this.enumsField;
            }
            set
            {
                this.enumsField = value;
            }
        }
    }
}

