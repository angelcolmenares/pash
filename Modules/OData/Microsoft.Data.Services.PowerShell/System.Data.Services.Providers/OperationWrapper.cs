namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Services;
    using System.Diagnostics;
    using System.Linq;

    [DebuggerVisualizer("OperationWrapper={Name}")]
    internal sealed class OperationWrapper
    {
        private Dictionary<ResourceType, string> actionTitleSegmentByResourceType;
        private readonly Operation operation;
        private ResourceSetWrapper resourceSet;
        private System.Data.Services.ServiceActionRights serviceActionRights;
        private System.Data.Services.ServiceOperationRights serviceOperationRights;

        public OperationWrapper(Operation operationBase)
        {
            if (!operationBase.IsReadOnly)
            {
                throw new DataServiceException(500, System.Data.Services.Strings.DataServiceProviderWrapper_ServiceOperationNotReadonly(operationBase.Name));
            }
            this.operation = operationBase;
            this.actionTitleSegmentByResourceType = new Dictionary<ResourceType, string>(EqualityComparer<ResourceType>.Default);
        }

        public void ApplyConfiguration(DataServiceConfiguration configuration, DataServiceProviderWrapper provider)
        {
            if (this.Kind == OperationKind.ServiceOperation)
            {
                this.serviceOperationRights = configuration.GetServiceOperationRights(this.ServiceOperation);
            }
            else
            {
                this.serviceActionRights = configuration.GetServiceActionRights(this.ServiceAction);
            }
            if (((this.Kind == OperationKind.ServiceOperation) && ((this.serviceOperationRights & ~System.Data.Services.ServiceOperationRights.OverrideEntitySetRights) != System.Data.Services.ServiceOperationRights.None)) || ((this.Kind == OperationKind.Action) && (this.serviceActionRights != System.Data.Services.ServiceActionRights.None)))
            {
                if (this.operation.ResourceSet != null)
                {
                    this.resourceSet = provider.TryResolveResourceSet(this.operation.ResourceSet.Name);
                    if (this.resourceSet == null)
                    {
                        throw new InvalidOperationException(System.Data.Services.Strings.OperationWrapper_OperationResourceSetNotVisible(this.Name, this.operation.ResourceSet.Name));
                    }
                }
                else if (this.ResultSetPathExpression != null)
                {
                    this.ResultSetPathExpression.InitializePathSegments(provider);
                }
            }
        }

        internal string GetActionTitleSegmentByResourceType(ResourceType resourceType, string containerName)
        {
            string str;
            if (!this.actionTitleSegmentByResourceType.TryGetValue(resourceType, out str))
            {
                bool flag = false;
                if (resourceType.IsOpenType)
                {
                    flag = true;
                }
                else
                {
                    foreach (ResourceProperty property in resourceType.Properties)
                    {
                        if (property.Name.Equals(this.Name, StringComparison.Ordinal))
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                str = flag ? (containerName + "." + this.Name) : this.Name;
                this.actionTitleSegmentByResourceType[resourceType] = str;
            }
            return str;
        }

        internal ResourceSetWrapper GetResultSet(DataServiceProviderWrapper provider, ResourceSetWrapper bindingSet)
        {
            if (this.resourceSet != null)
            {
                return this.resourceSet;
            }
            if (this.ResultSetPathExpression == null)
            {
                return null;
            }
            if (bindingSet == null)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.OperationWrapper_PathExpressionRequiresBindingSet(this.Name));
            }
            ResourceSetWrapper targetSet = this.ResultSetPathExpression.GetTargetSet(provider, bindingSet);
            if (targetSet == null)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.OperationWrapper_TargetSetFromPathExpressionNotNotVisible(this.Name, this.ResultSetPathExpression.PathExpression, bindingSet.Name));
            }
            return targetSet;
        }

        public OperationParameter BindingParameter
        {
            [DebuggerStepThrough]
            get
            {
                return this.operation.OperationBindingParameter;
            }
        }

        public bool IsVisible
        {
            [DebuggerStepThrough]
            get
            {
                return (((this.Kind == OperationKind.ServiceOperation) && ((this.serviceOperationRights & ~System.Data.Services.ServiceOperationRights.OverrideEntitySetRights) != System.Data.Services.ServiceOperationRights.None)) || ((this.Kind == OperationKind.Action) && (this.serviceActionRights != System.Data.Services.ServiceActionRights.None)));
            }
        }

        internal OperationKind Kind
        {
            [DebuggerStepThrough]
            get
            {
                return this.operation.Kind;
            }
        }

        public string Method
        {
            [DebuggerStepThrough]
            get
            {
                return this.operation.Method;
            }
        }

        public string MimeType
        {
            [DebuggerStepThrough]
            get
            {
                return this.operation.MimeType;
            }
        }

        public string Name
        {
            [DebuggerStepThrough]
            get
            {
                return this.operation.Name;
            }
        }

        internal System.Data.Services.Providers.OperationParameterBindingKind OperationParameterBindingKind
        {
            get
            {
                return this.operation.OperationParameterBindingKind;
            }
        }

        public ReadOnlyCollection<OperationParameter> Parameters
        {
            [DebuggerStepThrough]
            get
            {
                return this.operation.OperationParameters;
            }
        }

        public ResourceSetWrapper ResourceSet
        {
            [DebuggerStepThrough]
            get
            {
                return this.resourceSet;
            }
        }

        public ServiceOperationResultKind ResultKind
        {
            [DebuggerStepThrough]
            get
            {
                return this.operation.OperationResultKind;
            }
        }

        public ResourceSetPathExpression ResultSetPathExpression
        {
            [DebuggerStepThrough]
            get
            {
                return this.operation.OperationResultSetPathExpression;
            }
        }

        public ResourceType ResultType
        {
            [DebuggerStepThrough]
            get
            {
                return this.operation.OperationResultType;
            }
        }

        internal Type ReturnInstanceType
        {
            get
            {
                Type type = (this.ResultType == null) ? null : this.ResultType.InstanceType;
                if ((this.ResultKind == ServiceOperationResultKind.QueryWithMultipleResults) || (this.ResultKind == ServiceOperationResultKind.QueryWithSingleResult))
                {
                    return typeof(IQueryable<>).MakeGenericType(new Type[] { type });
                }
                if (this.ResultKind == ServiceOperationResultKind.Enumeration)
                {
                    type = typeof(IEnumerable<>).MakeGenericType(new Type[] { type });
                }
                return type;
            }
        }

        public ResourceType ReturnType
        {
            [DebuggerStepThrough]
            get
            {
                return this.operation.OperationReturnType;
            }
        }

        public System.Data.Services.Providers.ServiceAction ServiceAction
        {
            [DebuggerStepThrough]
            get
            {
                return (System.Data.Services.Providers.ServiceAction) this.operation;
            }
        }

        public System.Data.Services.ServiceActionRights ServiceActionRights
        {
            [DebuggerStepThrough]
            get
            {
                return this.serviceActionRights;
            }
        }

        public System.Data.Services.Providers.ServiceOperation ServiceOperation
        {
            [DebuggerStepThrough]
            get
            {
                return (System.Data.Services.Providers.ServiceOperation) this.operation;
            }
        }

        public System.Data.Services.ServiceOperationRights ServiceOperationRights
        {
            [DebuggerStepThrough]
            get
            {
                return this.serviceOperationRights;
            }
        }
    }
}

