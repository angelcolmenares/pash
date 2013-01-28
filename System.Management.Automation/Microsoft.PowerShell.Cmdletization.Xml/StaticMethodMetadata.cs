namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, DesignerCategory("code"), DebuggerStepThrough, XmlType(Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11"), GeneratedCode("xsd", "4.0.30319.17361")]
    internal class StaticMethodMetadata : CommonMethodMetadata
    {
        private string cmdletParameterSetField;
        private StaticMethodParameterMetadata[] parametersField;

        [XmlAttribute]
        public string CmdletParameterSet
        {
            get
            {
                return this.cmdletParameterSetField;
            }
            set
            {
                this.cmdletParameterSetField = value;
            }
        }

        [XmlArrayItem("Parameter", IsNullable=false)]
        public StaticMethodParameterMetadata[] Parameters
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

