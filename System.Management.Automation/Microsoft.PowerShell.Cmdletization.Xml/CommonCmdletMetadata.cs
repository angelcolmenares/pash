namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, DesignerCategory("code"), GeneratedCode("xsd", "4.0.30319.17361"), XmlType(Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11"), DebuggerStepThrough]
    internal class CommonCmdletMetadata
    {
        private string[] aliasesField;
        private Microsoft.PowerShell.Cmdletization.Xml.ConfirmImpact confirmImpactField;
        private bool confirmImpactFieldSpecified;
        private string helpUriField;
        private string nounField;
        private string verbField;

        [XmlAttribute]
        public string[] Aliases
        {
            get
            {
                return this.aliasesField;
            }
            set
            {
                this.aliasesField = value;
            }
        }

        [XmlAttribute]
        public Microsoft.PowerShell.Cmdletization.Xml.ConfirmImpact ConfirmImpact
        {
            get
            {
                return this.confirmImpactField;
            }
            set
            {
                this.confirmImpactField = value;
            }
        }

        [XmlIgnore]
        public bool ConfirmImpactSpecified
        {
            get
            {
                return this.confirmImpactFieldSpecified;
            }
            set
            {
                this.confirmImpactFieldSpecified = value;
            }
        }

        [XmlAttribute(DataType="anyURI")]
        public string HelpUri
        {
            get
            {
                return this.helpUriField;
            }
            set
            {
                this.helpUriField = value;
            }
        }

        [XmlAttribute]
        public string Noun
        {
            get
            {
                return this.nounField;
            }
            set
            {
                this.nounField = value;
            }
        }

        [XmlAttribute]
        public string Verb
        {
            get
            {
                return this.verbField;
            }
            set
            {
                this.verbField = value;
            }
        }
    }
}

