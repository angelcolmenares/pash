namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, DesignerCategory("code"), DebuggerStepThrough, GeneratedCode("xsd", "4.0.30319.17361"), XmlType(Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11")]
    internal class InstanceCmdletMetadata
    {
        private CommonCmdletMetadata cmdletMetadataField;
        private Microsoft.PowerShell.Cmdletization.Xml.GetCmdletParameters getCmdletParametersField;
        private InstanceMethodMetadata methodField;

        public CommonCmdletMetadata CmdletMetadata
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

        public InstanceMethodMetadata Method
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

