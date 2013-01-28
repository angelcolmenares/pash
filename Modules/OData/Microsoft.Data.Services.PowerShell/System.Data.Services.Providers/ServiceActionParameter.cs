namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;

    [DebuggerVisualizer("ServiceActionParameter={Name}")]
    internal class ServiceActionParameter : OperationParameter
    {
        internal static readonly ReadOnlyCollection<ServiceActionParameter> EmptyServiceActionParameterCollection = new ReadOnlyCollection<ServiceActionParameter>(new ServiceActionParameter[0]);

        public ServiceActionParameter(string name, ResourceType parameterType) : base(name, parameterType)
        {
        }
    }
}

