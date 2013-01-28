namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, DesignerCategory("code"), XmlType(Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11"), GeneratedCode("xsd", "4.0.30319.17361"), DebuggerStepThrough]
    internal class GetCmdletParameters
    {
        private string defaultCmdletParameterSetField;
        private Association[] queryableAssociationsField;
        private PropertyMetadata[] queryablePropertiesField;
        private QueryOption[] queryOptionsField;

        [XmlAttribute]
        public string DefaultCmdletParameterSet
        {
            get
            {
                return this.defaultCmdletParameterSetField;
            }
            set
            {
                this.defaultCmdletParameterSetField = value;
            }
        }

        [XmlArrayItem(IsNullable=false)]
        public Association[] QueryableAssociations
        {
            get
            {
                return this.queryableAssociationsField;
            }
            set
            {
                this.queryableAssociationsField = value;
            }
        }

        [XmlArrayItem("Property", IsNullable=false)]
        public PropertyMetadata[] QueryableProperties
        {
            get
            {
                return this.queryablePropertiesField;
            }
            set
            {
                this.queryablePropertiesField = value;
            }
        }

        [XmlArrayItem("Option", IsNullable=false)]
        public QueryOption[] QueryOptions
        {
            get
            {
                return this.queryOptionsField;
            }
            set
            {
                this.queryOptionsField = value;
            }
        }
    }
}

