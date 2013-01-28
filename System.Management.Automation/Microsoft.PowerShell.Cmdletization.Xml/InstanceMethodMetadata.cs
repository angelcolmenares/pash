namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, GeneratedCode("xsd", "4.0.30319.17361"), DebuggerStepThrough, DesignerCategory("code"), XmlType(Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11")]
    internal class InstanceMethodMetadata : CommonMethodMetadata
    {
        private InstanceMethodParameterMetadata[] parametersField;

        [XmlArrayItem("Parameter", IsNullable=false)]
        public InstanceMethodParameterMetadata[] Parameters
        {
            get
            {
                return this.parametersField;
            }
            set
            {
                this.parametersField = value;
            }
        }
    }
}

