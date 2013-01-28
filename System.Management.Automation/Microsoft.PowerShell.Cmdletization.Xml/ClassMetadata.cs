namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, DebuggerStepThrough, DesignerCategory("code"), XmlType(Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11"), GeneratedCode("xsd", "4.0.30319.17361")]
    internal class ClassMetadata
    {
        private string classNameField;
        private string classVersionField;
        private string cmdletAdapterField;
        private ClassMetadataData[] cmdletAdapterPrivateDataField;
        private string defaultNounField;
        private ClassMetadataInstanceCmdlets instanceCmdletsField;
        private StaticCmdletMetadata[] staticCmdletsField;
        private string versionField;

        [XmlAttribute]
        public string ClassName
        {
            get
            {
                return this.classNameField;
            }
            set
            {
                this.classNameField = value;
            }
        }

        [XmlAttribute]
        public string ClassVersion
        {
            get
            {
                return this.classVersionField;
            }
            set
            {
                this.classVersionField = value;
            }
        }

        [XmlAttribute]
        public string CmdletAdapter
        {
            get
            {
                return this.cmdletAdapterField;
            }
            set
            {
                this.cmdletAdapterField = value;
            }
        }

        [XmlArrayItem("Data", IsNullable=false)]
        public ClassMetadataData[] CmdletAdapterPrivateData
        {
            get
            {
                return this.cmdletAdapterPrivateDataField;
            }
            set
            {
                this.cmdletAdapterPrivateDataField = value;
            }
        }

        public string DefaultNoun
        {
            get
            {
                return this.defaultNounField;
            }
            set
            {
                this.defaultNounField = value;
            }
        }

        public ClassMetadataInstanceCmdlets InstanceCmdlets
        {
            get
            {
                return this.instanceCmdletsField;
            }
            set
            {
                this.instanceCmdletsField = value;
            }
        }

        [XmlArrayItem("Cmdlet", IsNullable=false)]
        public StaticCmdletMetadata[] StaticCmdlets
        {
            get
            {
                return this.staticCmdletsField;
            }
            set
            {
                this.staticCmdletsField = value;
            }
        }

        public string Version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }
    }
}

