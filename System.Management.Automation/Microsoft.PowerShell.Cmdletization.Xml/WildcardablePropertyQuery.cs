namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, GeneratedCode("xsd", "4.0.30319.17361"), DebuggerStepThrough, DesignerCategory("code"), XmlType(Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11")]
    internal class WildcardablePropertyQuery : PropertyQuery
    {
        private bool allowGlobbingField;
        private bool allowGlobbingFieldSpecified;

        [XmlAttribute]
        public bool AllowGlobbing
        {
            get
            {
                return this.allowGlobbingField;
            }
            set
            {
                this.allowGlobbingField = value;
            }
        }

        [XmlIgnore]
        public bool AllowGlobbingSpecified
        {
            get
            {
                return this.allowGlobbingFieldSpecified;
            }
            set
            {
                this.allowGlobbingFieldSpecified = value;
            }
        }
    }
}

