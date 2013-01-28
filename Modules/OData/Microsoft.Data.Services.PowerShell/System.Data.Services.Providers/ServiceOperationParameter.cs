namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.ObjectModel;
    using System.Data.Services;
    using System.Diagnostics;

    [DebuggerVisualizer("ServiceOperationParameter={Name}")]
    internal class ServiceOperationParameter : OperationParameter
    {
        internal static readonly ReadOnlyCollection<ServiceOperationParameter> EmptyServiceOperationParameterCollection = new ReadOnlyCollection<ServiceOperationParameter>(new ServiceOperationParameter[0]);

        public ServiceOperationParameter(string name, ResourceType parameterType) : base(name, parameterType)
        {
            WebUtil.CheckArgumentNull<ResourceType>(parameterType, "parameterType");
            if (parameterType.ResourceTypeKind != ResourceTypeKind.Primitive)
            {
                throw new ArgumentException(Strings.ServiceOperationParameter_TypeNotSupported(name, parameterType.FullName), "parameterType");
            }
        }
    }
}

