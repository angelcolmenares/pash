namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.ObjectModel;
    using System.Data.Services;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;

    [DebuggerVisualizer("OperationParameterBase={Name}")]
    internal abstract class OperationParameter
    {
        internal static readonly ReadOnlyCollection<OperationParameter> EmptyOperationParameterCollection = new ReadOnlyCollection<OperationParameter>(new OperationParameter[0]);
        private bool isReadOnly;
        private readonly string name;
        private readonly ResourceType type;

        protected internal OperationParameter(string name, ResourceType parameterType)
        {
            WebUtil.CheckStringArgumentNullOrEmpty(name, "name");
            WebUtil.CheckArgumentNull<ResourceType>(parameterType, "parameterType");
            if ((parameterType.ResourceTypeKind == ResourceTypeKind.Primitive) && (parameterType == ResourceType.GetPrimitiveResourceType(typeof(Stream))))
            {
                throw new ArgumentException(Strings.ServiceOperationParameter_TypeNotSupported(name, parameterType.FullName), "parameterType");
            }
            this.name = name;
            this.type = parameterType;
        }

        public void SetReadOnly()
        {
            if (!this.isReadOnly)
            {
                this.isReadOnly = true;
            }
        }

        public object CustomState { get; set; }

        public bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public ResourceType ParameterType
        {
            get
            {
                return this.type;
            }
        }
    }
}

