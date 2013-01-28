namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, DesignerCategory("code"), GeneratedCode("xsd", "4.0.30319.17361"), XmlType(Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11"), DebuggerStepThrough]
    internal class CmdletOutputMetadata
    {
        private object errorCodeField;
        private string pSNameField;

        public object ErrorCode
        {
            get
            {
                return this.errorCodeField;
            }
            set
            {
                this.errorCodeField = value;
            }
        }

        [XmlAttribute]
        public string PSName
        {
            get
            {
                return this.pSNameField;
            }
            set
            {
                this.pSNameField = value;
            }
        }
    }
}

