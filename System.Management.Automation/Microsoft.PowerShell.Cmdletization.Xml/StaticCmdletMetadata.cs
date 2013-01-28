namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, GeneratedCode("xsd", "4.0.30319.17361"), DesignerCategory("code"), XmlType(Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11"), DebuggerStepThrough]
    internal class StaticCmdletMetadata
    {
        private StaticCmdletMetadataCmdletMetadata cmdletMetadataField;
        private StaticMethodMetadata[] methodField;

        public StaticCmdletMetadataCmdletMetadata CmdletMetadata
        {
            get
            {
                return this.cmdletMetadataField;
            }
            set
            {
                this.cmdletMetadataField = value;
            }
        }

        [XmlElement("Method")]
        public StaticMethodMetadata[] Method
        {
            get
            {
                return this.methodField;
            }
            set
            {
                this.methodField = value;
            }
        }
    }
}

