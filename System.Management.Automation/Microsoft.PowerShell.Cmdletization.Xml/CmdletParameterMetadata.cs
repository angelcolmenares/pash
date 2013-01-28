namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, DesignerCategory("code"), DebuggerStepThrough, XmlInclude(typeof(CmdletParameterMetadataForGetCmdletParameter)), XmlInclude(typeof(CmdletParameterMetadataForGetCmdletFilteringParameter)), XmlInclude(typeof(CmdletParameterMetadataForInstanceMethodParameter)), XmlInclude(typeof(CmdletParameterMetadataForStaticMethodParameter)), XmlType(Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11"), GeneratedCode("xsd", "4.0.30319.17361")]
    internal class CmdletParameterMetadata
    {
        private string[] aliasesField;
        private object allowEmptyCollectionField;
        private object allowEmptyStringField;
        private object allowNullField;
        private bool isMandatoryField;
        private bool isMandatoryFieldSpecified;
        private string positionField;
        private string pSNameField;
        private CmdletParameterMetadataValidateCount validateCountField;
        private CmdletParameterMetadataValidateLength validateLengthField;
        private object validateNotNullField;
        private object validateNotNullOrEmptyField;
        private CmdletParameterMetadataValidateRange validateRangeField;
        private string[] validateSetField;

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

        public object AllowEmptyCollection
        {
            get
            {
                return this.allowEmptyCollectionField;
            }
            set
            {
                this.allowEmptyCollectionField = value;
            }
        }

        public object AllowEmptyString
        {
            get
            {
                return this.allowEmptyStringField;
            }
            set
            {
                this.allowEmptyStringField = value;
            }
        }

        public object AllowNull
        {
            get
            {
                return this.allowNullField;
            }
            set
            {
                this.allowNullField = value;
            }
        }

        [XmlAttribute]
        public bool IsMandatory
        {
            get
            {
                return this.isMandatoryField;
            }
            set
            {
                this.isMandatoryField = value;
            }
        }

        [XmlIgnore]
        public bool IsMandatorySpecified
        {
            get
            {
                return this.isMandatoryFieldSpecified;
            }
            set
            {
                this.isMandatoryFieldSpecified = value;
            }
        }

        [XmlAttribute(DataType="nonNegativeInteger")]
        public string Position
        {
            get
            {
                return this.positionField;
            }
            set
            {
                this.positionField = value;
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

        public CmdletParameterMetadataValidateCount ValidateCount
        {
            get
            {
                return this.validateCountField;
            }
            set
            {
                this.validateCountField = value;
            }
        }

        public CmdletParameterMetadataValidateLength ValidateLength
        {
            get
            {
                return this.validateLengthField;
            }
            set
            {
                this.validateLengthField = value;
            }
        }

        public object ValidateNotNull
        {
            get
            {
                return this.validateNotNullField;
            }
            set
            {
                this.validateNotNullField = value;
            }
        }

        public object ValidateNotNullOrEmpty
        {
            get
            {
                return this.validateNotNullOrEmptyField;
            }
            set
            {
                this.validateNotNullOrEmptyField = value;
            }
        }

        public CmdletParameterMetadataValidateRange ValidateRange
        {
            get
            {
                return this.validateRangeField;
            }
            set
            {
                this.validateRangeField = value;
            }
        }

        [XmlArrayItem("AllowedValue", IsNullable=false)]
        public string[] ValidateSet
        {
            get
            {
                return this.validateSetField;
            }
            set
            {
                this.validateSetField = value;
            }
        }
    }
}

