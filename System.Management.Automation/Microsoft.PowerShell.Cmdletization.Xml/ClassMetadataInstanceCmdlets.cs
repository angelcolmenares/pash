namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, DebuggerStepThrough, DesignerCategory("code"), XmlType(AnonymousType=true, Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11"), GeneratedCode("xsd", "4.0.30319.17361")]
    internal class ClassMetadataInstanceCmdlets
    {
        private InstanceCmdletMetadata[] cmdletField;
        private GetCmdletMetadata getCmdletField;
        private Microsoft.PowerShell.Cmdletization.Xml.GetCmdletParameters getCmdletParametersField;

        [XmlElement("Cmdlet")]
        public InstanceCmdletMetadata[] Cmdlet
        {
            get
            {
                return this.cmdletField;
            }
            set
            {
                this.cmdletField = value;
            }
        }

        public GetCmdletMetadata GetCmdlet
        {
            get
            {
                return this.getCmdletField;
            }
            set
            {
                this.getCmdletField = value;
            }
        }

        public Microsoft.PowerShell.Cmdletization.Xml.GetCmdletParameters GetCmdletParameters
        {
            get
            {
                return this.getCmdletParametersField;
            }
            set
            {
                this.getCmdletParametersField = value;
            }
        }
    }
}

