namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, DebuggerStepThrough, XmlType(Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11"), DesignerCategory("code"), GeneratedCode("xsd", "4.0.30319.17361")]
    internal class PropertyMetadata
    {
        private ItemsChoiceType[] itemsElementNameField;
        private PropertyQuery[] itemsField;
        private string propertyNameField;
        private TypeMetadata typeField;

        [XmlElement("MaxValueQuery", typeof(PropertyQuery)), XmlElement("ExcludeQuery", typeof(WildcardablePropertyQuery)), XmlChoiceIdentifier("ItemsElementName"), XmlElement("RegularQuery", typeof(WildcardablePropertyQuery)), XmlElement("MinValueQuery", typeof(PropertyQuery))]
        public PropertyQuery[] Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }

        [XmlElement("ItemsElementName"), XmlIgnore]
        public ItemsChoiceType[] ItemsElementName
        {
            get
            {
                return this.itemsElementNameField;
            }
            set
            {
                this.itemsElementNameField = value;
            }
        }

        [XmlAttribute]
        public string PropertyName
        {
            get
            {
                return this.propertyNameField;
            }
            set
            {
                this.propertyNameField = value;
            }
        }

        public TypeMetadata Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }
    }
}

