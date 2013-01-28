namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, GeneratedCode("xsd", "4.0.30319.17361"), DesignerCategory("code"), XmlType(AnonymousType=true, Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11"), DebuggerStepThrough]
    internal class StaticCmdletMetadataCmdletMetadata : CommonCmdletMetadata
    {
        private string defaultCmdletParameterSetField;

        [XmlAttribute]
        public string DefaultCmdletParameterSet
        {
            get
            {
                return this.defaultCmdletParameterSetField;
            }
            set
            {
                this.defaultCmdletParameterSetField = value;
            }
        }
    }
}

