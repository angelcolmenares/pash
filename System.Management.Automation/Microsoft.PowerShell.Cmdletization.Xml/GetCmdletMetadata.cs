namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, XmlType(Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11"), GeneratedCode("xsd", "4.0.30319.17361"), DebuggerStepThrough, DesignerCategory("code")]
    internal class GetCmdletMetadata
    {
        private CommonCmdletMetadata cmdletMetadataField;
        private Microsoft.PowerShell.Cmdletization.Xml.GetCmdletParameters getCmdletParametersField;

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
    }
}

