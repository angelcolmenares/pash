namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, XmlType(Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11"), GeneratedCode("xsd", "4.0.30319.17361"), DebuggerStepThrough, DesignerCategory("code")]
    internal class Association
    {
        private AssociationAssociatedInstance associatedInstanceField;
        private string association1Field;
        private string resultRoleField;
        private string sourceRoleField;

        public AssociationAssociatedInstance AssociatedInstance
        {
            get
            {
                return this.associatedInstanceField;
            }
            set
            {
                this.associatedInstanceField = value;
            }
        }

        [XmlAttribute("Association")]
        public string Association1
        {
            get
            {
                return this.association1Field;
            }
            set
            {
                this.association1Field = value;
            }
        }

        [XmlAttribute]
        public string ResultRole
        {
            get
            {
                return this.resultRoleField;
            }
            set
            {
                this.resultRoleField = value;
            }
        }

        [XmlAttribute]
        public string SourceRole
        {
            get
            {
                return this.sourceRoleField;
            }
            set
            {
                this.sourceRoleField = value;
            }
        }
    }
}

