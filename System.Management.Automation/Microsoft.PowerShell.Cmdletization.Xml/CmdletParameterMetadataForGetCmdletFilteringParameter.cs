namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, DebuggerStepThrough, GeneratedCode("xsd", "4.0.30319.17361"), DesignerCategory("code"), XmlType(Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11")]
    internal class CmdletParameterMetadataForGetCmdletFilteringParameter : CmdletParameterMetadataForGetCmdletParameter
    {
        private bool errorOnNoMatchField;
        private bool errorOnNoMatchFieldSpecified;

        [XmlAttribute]
        public bool ErrorOnNoMatch
        {
            get
            {
                return this.errorOnNoMatchField;
            }
            set
            {
                this.errorOnNoMatchField = value;
            }
        }

        [XmlIgnore]
        public bool ErrorOnNoMatchSpecified
        {
            get
            {
                return this.errorOnNoMatchFieldSpecified;
            }
            set
            {
                this.errorOnNoMatchFieldSpecified = value;
            }
        }
    }
}

