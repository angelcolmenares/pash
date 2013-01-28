namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, GeneratedCode("xsd", "4.0.30319.17361"), DesignerCategory("code"), XmlType(Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11"), DebuggerStepThrough]
    internal class TypeMetadata
    {
        private string eTSTypeField;
        private string pSTypeField;

        [XmlAttribute]
        public string ETSType
        {
            get
            {
                return this.eTSTypeField;
            }
            set
            {
                this.eTSTypeField = value;
            }
        }

        [XmlAttribute]
        public string PSType
        {
            get
            {
                return this.pSTypeField;
            }
            set
            {
                this.pSTypeField = value;
            }
        }
    }
}

