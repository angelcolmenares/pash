namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, DesignerCategory("code"), DebuggerStepThrough, XmlType(Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11"), XmlInclude(typeof(InstanceMethodMetadata)), XmlInclude(typeof(StaticMethodMetadata)), GeneratedCode("xsd", "4.0.30319.17361")]
    internal class CommonMethodMetadata
    {
        private string methodNameField;
        private CommonMethodMetadataReturnValue returnValueField;

        [XmlAttribute]
        public string MethodName
        {
            get
            {
                return this.methodNameField;
            }
            set
            {
                this.methodNameField = value;
            }
        }

        public CommonMethodMetadataReturnValue ReturnValue
        {
            get
            {
                return this.returnValueField;
            }
            set
            {
                this.returnValueField = value;
            }
        }
    }
}

