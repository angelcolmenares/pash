namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Services;
    using System.Diagnostics;
    using System.Linq;

    [DebuggerVisualizer("ServiceOperation={Name}")]
    internal class ServiceOperation : Operation
    {
        private readonly ReadOnlyCollection<ServiceOperationParameter> parameters;

        public ServiceOperation(string name, ServiceOperationResultKind resultKind, ResourceType resultType, ResourceSet resultSet, string method, IEnumerable<ServiceOperationParameter> parameters) : base(name, resultKind, GetReturnTypeFromResultType(resultType, resultKind), resultSet, null, method, parameters, OperationParameterBindingKind.Never, OperationKind.ServiceOperation)
        {
            if (base.OperationParameters == OperationParameter.EmptyOperationParameterCollection)
            {
                this.parameters = ServiceOperationParameter.EmptyServiceOperationParameterCollection;
            }
            else
            {
                this.parameters = new ReadOnlyCollection<ServiceOperationParameter>(base.OperationParameters.Cast<ServiceOperationParameter>().ToList<ServiceOperationParameter>());
            }
        }

        private static ResourceType GetReturnTypeFromResultType(ResourceType resultType, ServiceOperationResultKind resultKind)
        {
            if (((resultKind == ServiceOperationResultKind.Void) && (resultType != null)) || ((resultKind != ServiceOperationResultKind.Void) && (resultType == null)))
            {
                throw new ArgumentException(System.Data.Services.Strings.ServiceOperation_ResultTypeAndKindMustMatch("resultKind", "resultType", ServiceOperationResultKind.Void));
            }
            if ((resultType != null) && (resultType.ResourceTypeKind == ResourceTypeKind.Collection))
            {
                throw new ArgumentException(System.Data.Services.Strings.ServiceOperation_InvalidResultType(resultType.FullName));
            }
            if ((resultType != null) && (resultType.ResourceTypeKind == ResourceTypeKind.EntityCollection))
            {
                throw new ArgumentException(System.Data.Services.Strings.ServiceOperation_InvalidResultType(resultType.FullName));
            }
            if (resultType == null)
            {
                return null;
            }
            if (((resultType.ResourceTypeKind == ResourceTypeKind.Primitive) || (resultType.ResourceTypeKind == ResourceTypeKind.ComplexType)) && ((resultKind == ServiceOperationResultKind.Enumeration) || (resultKind == ServiceOperationResultKind.QueryWithMultipleResults)))
            {
                return ResourceType.GetCollectionResourceType(resultType);
            }
            if ((resultType.ResourceTypeKind == ResourceTypeKind.EntityType) && ((resultKind == ServiceOperationResultKind.Enumeration) || (resultKind == ServiceOperationResultKind.QueryWithMultipleResults)))
            {
                return ResourceType.GetEntityCollectionResourceType(resultType);
            }
            return resultType;
        }

        public ReadOnlyCollection<ServiceOperationParameter> Parameters
        {
            get
            {
                return this.parameters;
            }
        }

        public ServiceOperationResultKind ResultKind
        {
            get
            {
                return base.OperationResultKind;
            }
        }

        public ResourceType ResultType
        {
            get
            {
                return base.OperationResultType;
            }
        }
    }
}

