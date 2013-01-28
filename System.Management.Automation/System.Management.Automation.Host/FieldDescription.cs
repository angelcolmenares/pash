namespace System.Management.Automation.Host
{
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation;

    public class FieldDescription
    {
        private PSObject defaultValue;
        private string helpMessage = "";
        private bool isFromRemoteHost;
        private bool isMandatory = true;
        private string label = "";
        private Collection<Attribute> metadata = new Collection<Attribute>();
        private bool modifiedByRemotingProtocol;
        private readonly string name;
        private const string NullOrEmptyErrorTemplateResource = "NullOrEmptyErrorTemplate";
        private string parameterAssemblyFullName;
        private string parameterTypeFullName;
        private string parameterTypeName;
        private const string StringsBaseName = "DescriptionsStrings";

        public FieldDescription(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name", "DescriptionsStrings", "NullOrEmptyErrorTemplate", new object[] { "name" });
            }
            this.name = name;
        }

        internal void SetParameterAssemblyFullName(string fullNameOfAssembly)
        {
            if (string.IsNullOrEmpty(fullNameOfAssembly))
            {
                throw PSTraceSource.NewArgumentException("fullNameOfAssembly", "DescriptionsStrings", "NullOrEmptyErrorTemplate", new object[] { "fullNameOfAssembly" });
            }
            this.parameterAssemblyFullName = fullNameOfAssembly;
        }

        public void SetParameterType(Type parameterType)
        {
            if (parameterType == null)
            {
                throw PSTraceSource.NewArgumentNullException("parameterType");
            }
            this.SetParameterTypeName(parameterType.Name);
            this.SetParameterTypeFullName(parameterType.FullName);
            this.SetParameterAssemblyFullName(parameterType.AssemblyQualifiedName);
        }

        internal void SetParameterTypeFullName(string fullNameOfType)
        {
            if (string.IsNullOrEmpty(fullNameOfType))
            {
                throw PSTraceSource.NewArgumentException("fullNameOfType", "DescriptionsStrings", "NullOrEmptyErrorTemplate", new object[] { "fullNameOfType" });
            }
            this.parameterTypeFullName = fullNameOfType;
        }

        internal void SetParameterTypeName(string nameOfType)
        {
            if (string.IsNullOrEmpty(nameOfType))
            {
                throw PSTraceSource.NewArgumentException("nameOfType", "DescriptionsStrings", "NullOrEmptyErrorTemplate", new object[] { "nameOfType" });
            }
            this.parameterTypeName = nameOfType;
        }

        public Collection<Attribute> Attributes
        {
            get
            {
                if (this.metadata == null)
                {
                    this.metadata = new Collection<Attribute>();
                }
                return this.metadata;
            }
        }

        public PSObject DefaultValue
        {
            get
            {
                return this.defaultValue;
            }
            set
            {
                this.defaultValue = value;
            }
        }

        public string HelpMessage
        {
            get
            {
                return this.helpMessage;
            }
            set
            {
                if (value == null)
                {
                    throw PSTraceSource.NewArgumentNullException("value");
                }
                this.helpMessage = value;
            }
        }

        internal bool IsFromRemoteHost
        {
            get
            {
                return this.isFromRemoteHost;
            }
            set
            {
                this.isFromRemoteHost = value;
            }
        }

        public bool IsMandatory
        {
            get
            {
                return this.isMandatory;
            }
            set
            {
                this.isMandatory = value;
            }
        }

        public string Label
        {
            get
            {
                return this.label;
            }
            set
            {
                if (value == null)
                {
                    throw PSTraceSource.NewArgumentNullException("value");
                }
                this.label = value;
            }
        }

        internal bool ModifiedByRemotingProtocol
        {
            get
            {
                return this.modifiedByRemotingProtocol;
            }
            set
            {
                this.modifiedByRemotingProtocol = value;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public string ParameterAssemblyFullName
        {
            get
            {
                if (string.IsNullOrEmpty(this.parameterAssemblyFullName))
                {
                    Type parameterType = Type.GetType("System.String");
                    this.SetParameterType(parameterType);
                }
                return this.parameterAssemblyFullName;
            }
        }

        public string ParameterTypeFullName
        {
            get
            {
                if (string.IsNullOrEmpty(this.parameterTypeFullName))
                {
                    Type parameterType = Type.GetType("System.String");
                    this.SetParameterType(parameterType);
                }
                return this.parameterTypeFullName;
            }
        }

        public string ParameterTypeName
        {
            get
            {
                if (string.IsNullOrEmpty(this.parameterTypeName))
                {
                    Type parameterType = Type.GetType("System.String");
                    this.SetParameterType(parameterType);
                }
                return this.parameterTypeName;
            }
        }
    }
}

