namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, DesignerCategory("code"), GeneratedCode("xsd", "4.0.30319.17361"), XmlType(Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11"), DebuggerStepThrough, XmlInclude(typeof(InstanceMethodParameterMetadata)), XmlInclude(typeof(StaticMethodParameterMetadata))]
    internal class CommonMethodParameterMetadata
    {
        private string defaultValueField;
        private string parameterNameField;
        private TypeMetadata typeField;

        [XmlAttribute]
        public string DefaultValue
        {
            get
            {
                return this.defaultValueField;
            }
            set
            {
                this.defaultValueField = value;
            }
        }

        [XmlAttribute]
        public string ParameterName
        {
            get
            {
                return this.parameterNameField;
            }
            set
            {
                this.parameterNameField = value;
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

