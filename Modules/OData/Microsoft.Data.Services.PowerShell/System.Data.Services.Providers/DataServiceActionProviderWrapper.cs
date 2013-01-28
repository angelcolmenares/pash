namespace System.Data.Services.Providers
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Data.Services.Common;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class DataServiceActionProviderWrapper
    {
        private IDataServiceActionProvider actionProvider;
        private bool attemptedToLoadActionProvider;
        private readonly IDataService dataService;
        private static readonly IEnumerable<OperationWrapper> EmptyServiceOperationWrapperEnumeration = new OperationWrapper[0];
        private Dictionary<ResourceType, IEnumerable<OperationWrapper>> serviceActionByResourceTypeCache;

        public DataServiceActionProviderWrapper(IDataService dataService)
        {
            this.dataService = dataService;
        }

        public bool AdvertiseServiceAction(DataServiceOperationContext operationContext, OperationWrapper serviceAction, object resourceInstance, bool resourceInstanceInFeed, ref ODataAction actionToSerialize)
        {
            return this.ActionProvider.AdvertiseServiceAction(operationContext, serviceAction.ServiceAction, resourceInstance, resourceInstanceInFeed, ref actionToSerialize);
        }

        public Expression CreateInvokable(DataServiceOperationContext operationContext, OperationWrapper serviceAction, Expression[] parameterTokens)
        {
            return Expression.Call(DataServiceExecutionProviderMethods.CreateServiceActionInvokableMethodInfo, Expression.Constant(operationContext, typeof(DataServiceOperationContext)), Expression.Constant(this.ActionProvider, typeof(IDataServiceActionProvider)), Expression.Constant(serviceAction.ServiceAction, typeof(ServiceAction)), Expression.NewArrayInit(typeof(object), parameterTokens));
        }

        internal static IDataServiceInvokable CreateInvokableFromSegment(SegmentInfo actionSegment)
        {
            return actionSegment.RequestEnumerable.OfType<IDataServiceInvokable>().Single<IDataServiceInvokable>();
        }

        public IEnumerable<OperationWrapper> GetServiceActions(DataServiceOperationContext operationContext)
        {
            if (this.TryLoadActionProvider())
            {
                IEnumerable<ServiceAction> serviceActions = this.actionProvider.GetServiceActions(operationContext);
                if (serviceActions != null)
                {
                    foreach (ServiceAction iteratorVariable1 in serviceActions)
                    {
                        OperationWrapper iteratorVariable2 = this.dataService.Provider.ValidateOperation(iteratorVariable1);
                        if (iteratorVariable2 != null)
                        {
                            yield return iteratorVariable2;
                        }
                    }
                }
            }
        }

        public IEnumerable<OperationWrapper> GetServiceActionsByBindingParameterType(DataServiceOperationContext operationContext, ResourceType bindingParameterType)
        {
            Func<ServiceAction, OperationWrapper> selector = null;
            IEnumerable<OperationWrapper> emptyServiceOperationWrapperEnumeration = EmptyServiceOperationWrapperEnumeration;
            HashSet<string> existingActionNames = new HashSet<string>(EqualityComparer<string>.Default);
            do
            {
                IEnumerable<OperationWrapper> enumerable2;
                if (!this.ServiceActionByResourceTypeCache.TryGetValue(bindingParameterType, out enumerable2))
                {
                    if (this.TryLoadActionProvider())
                    {
                        IEnumerable<ServiceAction> serviceActionsByBindingParameterType = this.actionProvider.GetServiceActionsByBindingParameterType(operationContext, bindingParameterType);
                        if ((serviceActionsByBindingParameterType != null) && serviceActionsByBindingParameterType.Any<ServiceAction>())
                        {
                            if (selector == null)
                            {
                                selector = serviceAction => this.ValidateCanAdvertiseServiceAction(bindingParameterType, serviceAction, existingActionNames);
                            }
                            enumerable2 = (from serviceOperationWrapper in serviceActionsByBindingParameterType.Select<ServiceAction, OperationWrapper>(selector)
                                where serviceOperationWrapper != null
                                select serviceOperationWrapper).ToArray<OperationWrapper>();
                        }
                    }
                    if (enumerable2 == null)
                    {
                        enumerable2 = EmptyServiceOperationWrapperEnumeration;
                    }
                    this.ServiceActionByResourceTypeCache[bindingParameterType] = enumerable2;
                }
                if (enumerable2.Any<OperationWrapper>())
                {
                    emptyServiceOperationWrapperEnumeration = emptyServiceOperationWrapperEnumeration.Concat<OperationWrapper>(enumerable2);
                }
                bindingParameterType = bindingParameterType.BaseType;
            }
            while (bindingParameterType != null);
            return emptyServiceOperationWrapperEnumeration;
        }

        internal static bool IsServiceActionRequest(RequestDescription description)
        {
            return IsServiceActionSegment(description.LastSegmentInfo);
        }

        internal static bool IsServiceActionSegment(SegmentInfo segment)
        {
            return ((segment.Operation != null) && (segment.Operation.Kind == OperationKind.Action));
        }

        internal static void ResolveActionResult(SegmentInfo actionSegment)
        {
            object result = CreateInvokableFromSegment(actionSegment).GetResult();
            if (((result != null) && actionSegment.SingleResult) && !(result is IQueryable))
            {
                result = new object[] { result };
            }
            actionSegment.RequestEnumerable = (IEnumerable) result;
        }

        private bool TryLoadActionProvider()
        {
            if (!this.attemptedToLoadActionProvider && (this.dataService.Provider.Configuration.DataServiceBehavior.MaxProtocolVersion >= DataServiceProtocolVersion.V3))
            {
                this.actionProvider = this.dataService.Provider.GetService<IDataServiceActionProvider>();
                this.attemptedToLoadActionProvider = true;
            }
            return (this.actionProvider != null);
        }

        public OperationWrapper TryResolveServiceAction(DataServiceOperationContext operationContext, string serviceActionName)
        {
            ServiceAction action;
            OperationWrapper wrapper;
            if (this.dataService.Provider.OperationWrapperCache.TryGetValue(serviceActionName, out wrapper))
            {
                if ((wrapper != null) && (wrapper.Kind == OperationKind.Action))
                {
                    return wrapper;
                }
                return null;
            }
            if ((!string.IsNullOrEmpty(serviceActionName) && this.TryLoadActionProvider()) && this.actionProvider.TryResolveServiceAction(operationContext, serviceActionName, out action))
            {
                return this.dataService.Provider.ValidateOperation(action);
            }
            return null;
        }

        private OperationWrapper ValidateCanAdvertiseServiceAction(ResourceType resourceType, ServiceAction serviceAction, HashSet<string> existingActionNames)
        {
            if (serviceAction == null)
            {
                return null;
            }
            if (!existingActionNames.Add(serviceAction.Name))
            {
                throw new DataServiceException(500, System.Data.Services.Strings.DataServiceActionProviderWrapper_DuplicateAction(serviceAction.Name));
            }
            ServiceActionParameter bindingParameter = serviceAction.BindingParameter;
            if (bindingParameter == null)
            {
                throw new DataServiceException(500, System.Data.Services.Strings.DataServiceActionProviderWrapper_ServiceActionBindingParameterNull(serviceAction.Name));
            }
            ResourceType parameterType = bindingParameter.ParameterType;
            if (!parameterType.IsAssignableFrom(resourceType))
            {
                throw new DataServiceException(500, System.Data.Services.Strings.DataServiceActionProviderWrapper_ResourceTypeMustBeAssignableToBindingParameterResourceType(serviceAction.Name, parameterType.FullName, resourceType.FullName));
            }
            return this.dataService.Provider.ValidateOperation(serviceAction);
        }

        private IDataServiceActionProvider ActionProvider
        {
            get
            {
                return this.actionProvider;
            }
        }

        private Dictionary<ResourceType, IEnumerable<OperationWrapper>> ServiceActionByResourceTypeCache
        {
            [DebuggerStepThrough]
            get
            {
                return (this.serviceActionByResourceTypeCache ?? (this.serviceActionByResourceTypeCache = new Dictionary<ResourceType, IEnumerable<OperationWrapper>>(EqualityComparer<ResourceType>.Default)));
            }
        }

        
    }
}

