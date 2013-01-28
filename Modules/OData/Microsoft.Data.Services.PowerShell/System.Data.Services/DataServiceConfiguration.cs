using Microsoft.Data.Edm;

namespace System.Data.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Common;
    using System.Data.Services.Parsing;
    using System.Data.Services.Providers;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;

    internal sealed class DataServiceConfiguration : IDataServiceConfiguration
    {
        private bool accessEnabledForAllResourceTypes;
        private readonly HashSet<string> accessEnabledResourceTypes;
        private bool configurationSealed;
        private readonly System.Data.Services.DataServiceBehavior dataServiceBehavior;
        private int defaultPageSize;
        private bool disableValidationOnMetadataWrite;
        private readonly List<Type> knownTypes;
        private int maxBatchCount;
        private int maxChangeSetCount;
        private int maxExpandCount;
        private int maxExpandDepth;
        private int maxObjectCountOnInsert;
        private int maxResultsPerCollection;
        private readonly Dictionary<string, int> pageSizes;
        private IDataServiceMetadataProvider provider;
        private readonly Dictionary<string, List<MethodInfo>> readAuthorizationMethods;
        private readonly Dictionary<string, EntitySetRights> resourceRights;
        private EntitySetRights rightsForUnspecifiedResourceContainer;
        private ServiceActionRights rightsForUnspecifiedServiceAction;
        private ServiceOperationRights rightsForUnspecifiedServiceOperation;
        private readonly Dictionary<string, ServiceActionRights> serviceActionRights;
        private readonly Dictionary<string, ServiceOperationRights> serviceOperationRights;
        private bool typeConversion;
        private bool useVerboseErrors;
        private readonly Dictionary<string, List<MethodInfo>> writeAuthorizationMethods;

        internal DataServiceConfiguration(IDataServiceMetadataProvider provider)
        {
            this.provider = provider;
            this.resourceRights = new Dictionary<string, EntitySetRights>(EqualityComparer<string>.Default);
            this.serviceOperationRights = new Dictionary<string, ServiceOperationRights>(EqualityComparer<string>.Default);
            this.serviceActionRights = new Dictionary<string, ServiceActionRights>(EqualityComparer<string>.Default);
            this.pageSizes = new Dictionary<string, int>(EqualityComparer<string>.Default);
            this.rightsForUnspecifiedResourceContainer = EntitySetRights.None;
            this.rightsForUnspecifiedServiceOperation = ServiceOperationRights.None;
            this.rightsForUnspecifiedServiceAction = ServiceActionRights.None;
            this.knownTypes = new List<Type>();
            this.maxBatchCount = 0x7fffffff;
            this.maxChangeSetCount = 0x7fffffff;
            this.maxExpandCount = 0x7fffffff;
            this.maxExpandDepth = 0x7fffffff;
            this.maxResultsPerCollection = 0x7fffffff;
            this.maxObjectCountOnInsert = 0x7fffffff;
            this.readAuthorizationMethods = new Dictionary<string, List<MethodInfo>>(EqualityComparer<string>.Default);
            this.writeAuthorizationMethods = new Dictionary<string, List<MethodInfo>>(EqualityComparer<string>.Default);
            this.accessEnabledResourceTypes = new HashSet<string>(EqualityComparer<string>.Default);
            this.dataServiceBehavior = new System.Data.Services.DataServiceBehavior();
            this.typeConversion = true;
        }

        private static void AppendRight(EntitySetRights entitySetRights, EntitySetRights test, string name, StringBuilder builder)
        {
            if ((entitySetRights & test) != EntitySetRights.None)
            {
                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }
                builder.Append(name);
            }
        }

        private int CheckNonNegativeProperty(int value, string propertyName)
        {
            this.CheckNotSealed();
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException("value", value, System.Data.Services.Strings.PropertyRequiresNonNegativeNumber(propertyName));
            }
            return value;
        }

        private void CheckNotSealed()
        {
            if (this.configurationSealed)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.DataServiceConfiguration_NoChangesAllowed("InitializeService"));
            }
        }

        private static void CheckParameterIsNotOut(MethodInfo method, ParameterInfo parameter)
        {
            if (parameter.IsOut)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.DataService_ParameterIsOut(method.DeclaringType.FullName, method.Name, parameter.Name));
            }
        }

        private static void CheckQueryInterceptorSignature(Type type, MethodInfo method, ResourceSet container)
        {
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != 0)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.DataService_QueryInterceptorIncorrectParameterCount(method.Name, type.FullName, parameters.Length));
            }
            Type type2 = typeof(Func<,>).MakeGenericType(new Type[] { container.ResourceType.InstanceType, typeof(bool) });
            Type type3 = typeof(Expression<>).MakeGenericType(new Type[] { type2 });
            Type returnType = method.ReturnType;
            if (returnType == typeof(void))
            {
                throw new InvalidOperationException(System.Data.Services.Strings.DataService_AuthorizationMethodVoid(method.Name, type.FullName, type3));
            }
            if (!type3.IsAssignableFrom(returnType))
            {
                Type type5 = typeof(Func<,>).MakeGenericType(new Type[] { container.ResourceType.InstanceType, typeof(bool?) });
                if (!typeof(Expression<>).MakeGenericType(new Type[] { type5 }).IsAssignableFrom(returnType))
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.DataService_AuthorizationReturnTypeNotAssignable(method.Name, type.FullName, returnType.FullName, type3.FullName));
                }
            }
        }

        internal static void CheckResourceRights(ResourceSetWrapper container, EntitySetRights requiredRights)
        {
            if ((requiredRights & container.Rights) == EntitySetRights.None)
            {
                throw DataServiceException.CreateForbidden();
            }
        }

        internal static void CheckResourceRightsForRead(ResourceSetWrapper container, bool singleResult)
        {
            EntitySetRights requiredRights = singleResult ? EntitySetRights.ReadSingle : EntitySetRights.ReadMultiple;
            CheckResourceRights(container, requiredRights);
        }

        internal static void CheckServiceOperationRights(OperationWrapper operation, bool singleResult)
        {
            if (operation.ResultKind != ServiceOperationResultKind.Void)
            {
                ServiceOperationRights requiredRights = singleResult ? ServiceOperationRights.ReadSingle : ServiceOperationRights.ReadMultiple;
                CheckServiceOperationRights(operation, requiredRights);
            }
        }

        internal static void CheckServiceOperationRights(OperationWrapper operation, ServiceOperationRights requiredRights)
        {
            ServiceOperationRights serviceOperationRights = operation.ServiceOperationRights;
            if ((requiredRights & serviceOperationRights) == ServiceOperationRights.None)
            {
                throw DataServiceException.CreateForbidden();
            }
        }

        internal static Expression ComposeQueryInterceptors(IDataService service, ResourceSetWrapper container)
        {
            MethodInfo[] queryInterceptors = container.QueryInterceptors;
            if ((queryInterceptors == null) || (queryInterceptors.Length == 0))
            {
                return null;
            }
            LambdaExpression expression = null;
            for (int i = 0; i < queryInterceptors.Length; i++)
            {
                Expression expression2;
                try
                {
                    expression2 = (Expression) queryInterceptors[i].Invoke(service.Instance, WebUtil.EmptyObjectArray);
                }
                catch (TargetInvocationException exception)
                {
                    ErrorHandler.HandleTargetInvocationException(exception);
                    throw;
                }
                if (expression2 == null)
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.DataService_AuthorizationReturnedNullQuery(queryInterceptors[i].Name, queryInterceptors[i].DeclaringType.FullName));
                }
                LambdaExpression expression3 = (LambdaExpression) expression2;
                if (expression == null)
                {
                    expression = expression3;
                }
                else
                {
                    ParameterExpression newExpression = expression.Parameters[0];
                    Expression right = ParameterReplacerVisitor.Replace(expression3.Body, expression3.Parameters[0], newExpression);
                    expression = Expression.Lambda(Expression.And(expression.Body, right), new ParameterExpression[] { newExpression });
                }
            }
            return expression;
        }

        internal static Expression ComposeResourceContainer(IDataService service, ResourceSetWrapper container, Expression queryExpression)
        {
            MethodInfo[] queryInterceptors = container.QueryInterceptors;
            if (queryInterceptors != null)
            {
                for (int i = 0; i < queryInterceptors.Length; i++)
                {
                    Expression expression;
                    try
                    {
                        expression = (Expression) queryInterceptors[i].Invoke(service.Instance, WebUtil.EmptyObjectArray);
                    }
                    catch (TargetInvocationException exception)
                    {
                        ErrorHandler.HandleTargetInvocationException(exception);
                        throw;
                    }
                    if (expression == null)
                    {
                        throw new InvalidOperationException(System.Data.Services.Strings.DataService_AuthorizationReturnedNullQuery(queryInterceptors[i].Name, queryInterceptors[i].DeclaringType.FullName));
                    }
                    queryExpression = queryExpression.QueryableWhere((LambdaExpression) expression);
                }
            }
            return queryExpression;
        }

        public void EnableTypeAccess(string typeName)
        {
            WebUtil.CheckStringArgumentNullOrEmpty(typeName, "typeName");
            this.CheckNotSealed();
            if (typeName == "*")
            {
                this.accessEnabledForAllResourceTypes = true;
            }
            else
            {
                ResourceType type;
                if (!this.provider.TryResolveResourceType(typeName, out type) || (type == null))
                {
                    throw new ArgumentException(System.Data.Services.Strings.DataServiceConfiguration_ResourceTypeNameNotFound(typeName), "typeName");
                }
                if (type.ResourceTypeKind != ResourceTypeKind.ComplexType)
                {
                    throw new ArgumentException(System.Data.Services.Strings.DataServiceConfiguration_NotComplexType(typeName), "typeName");
                }
                this.accessEnabledResourceTypes.Add(typeName);
            }
        }

        internal IEnumerable<string> GetAccessEnabledResourceTypes()
        {
            return this.accessEnabledResourceTypes;
        }

        internal static string GetAllowedMethods(DataServiceConfiguration configuration, RequestDescription description)
        {
            if ((description.TargetKind == RequestTargetKind.Metadata) || (description.TargetKind == RequestTargetKind.ServiceDirectory))
            {
                return "GET";
            }
            if (description.TargetKind == RequestTargetKind.Batch)
            {
                return "POST";
            }
            int indexOfTargetEntityResource = description.GetIndexOfTargetEntityResource();
            ResourceSetWrapper targetContainer = description.SegmentInfos[indexOfTargetEntityResource].TargetContainer;
            return GetAllowedMethods(configuration, targetContainer, description);
        }

        internal static string GetAllowedMethods(DataServiceConfiguration configuration, ResourceSetWrapper container, RequestDescription description)
        {
            if (container == null)
            {
                return null;
            }
            StringBuilder builder = new StringBuilder();
            EntitySetRights resourceSetRights = configuration.GetResourceSetRights(container.ResourceSet);
            if (description.IsSingleResult)
            {
                AppendRight(resourceSetRights, EntitySetRights.ReadSingle, "GET", builder);
                AppendRight(resourceSetRights, EntitySetRights.WriteReplace, "PUT", builder);
                if (description.TargetKind != RequestTargetKind.MediaResource)
                {
                    AppendRight(resourceSetRights, EntitySetRights.WriteMerge, "MERGE", builder);
                    AppendRight(resourceSetRights, EntitySetRights.WriteMerge, "PATCH", builder);
                    AppendRight(resourceSetRights, EntitySetRights.WriteDelete, "DELETE", builder);
                }
            }
            else
            {
                AppendRight(resourceSetRights, EntitySetRights.ReadMultiple, "GET", builder);
                AppendRight(resourceSetRights, EntitySetRights.WriteAppend, "POST", builder);
            }
            return builder.ToString();
        }

        internal IEnumerable<Type> GetKnownTypes()
        {
            return this.knownTypes;
        }

        internal MethodInfo[] GetReadAuthorizationMethods(ResourceSet resourceSet)
        {
            List<MethodInfo> list;
            if (this.readAuthorizationMethods.TryGetValue(resourceSet.Name, out list))
            {
                return list.ToArray();
            }
            return null;
        }

        internal int GetResourceSetPageSize(ResourceSet container)
        {
            int defaultPageSize;
            if (!this.pageSizes.TryGetValue(container.Name, out defaultPageSize))
            {
                defaultPageSize = this.defaultPageSize;
            }
            return defaultPageSize;
        }

        internal EntitySetRights GetResourceSetRights(ResourceSet container)
        {
            EntitySetRights rightsForUnspecifiedResourceContainer;
            if (!this.resourceRights.TryGetValue(container.Name, out rightsForUnspecifiedResourceContainer))
            {
                rightsForUnspecifiedResourceContainer = this.rightsForUnspecifiedResourceContainer;
            }
            return rightsForUnspecifiedResourceContainer;
        }

        internal ServiceActionRights GetServiceActionRights(ServiceAction serviceAction)
        {
            ServiceActionRights rightsForUnspecifiedServiceAction;
            if (!this.serviceActionRights.TryGetValue(serviceAction.Name, out rightsForUnspecifiedServiceAction))
            {
                rightsForUnspecifiedServiceAction = this.rightsForUnspecifiedServiceAction;
            }
            return rightsForUnspecifiedServiceAction;
        }

        internal ServiceOperationRights GetServiceOperationRights(ServiceOperation serviceOperation)
        {
            ServiceOperationRights rightsForUnspecifiedServiceOperation;
            if (!this.serviceOperationRights.TryGetValue(serviceOperation.Name, out rightsForUnspecifiedServiceOperation))
            {
                rightsForUnspecifiedServiceOperation = this.rightsForUnspecifiedServiceOperation;
            }
            return rightsForUnspecifiedServiceOperation;
        }

        internal MethodInfo[] GetWriteAuthorizationMethods(ResourceSet resourceSet)
        {
            List<MethodInfo> list;
            if (this.writeAuthorizationMethods.TryGetValue(resourceSet.Name, out list))
            {
                return list.ToArray();
            }
            return null;
        }

        internal void Initialize(Type type)
        {
            this.InvokeStaticInitialization(type);
            this.RegisterCallbacks(type);
        }

        private void InvokeStaticInitialization(Type type)
        {
            while (type != null)
            {
                MethodInfo info = type.GetMethod("InitializeService", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new Type[] { typeof(IDataServiceConfiguration) }, null) ?? type.GetMethod("InitializeService", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new Type[] { typeof(DataServiceConfiguration) }, null);
                if ((info != null) && (info.ReturnType == typeof(void)))
                {
                    ParameterInfo[] parameters = info.GetParameters();
                    if ((parameters.Length == 1) && !parameters[0].IsOut)
                    {
                        object[] objArray = new object[] { this };
                        try
                        {
                            info.Invoke(null, objArray);
                        }
                        catch (TargetInvocationException exception)
                        {
                            ErrorHandler.HandleTargetInvocationException(exception);
                            throw;
                        }
                        return;
                    }
                }
                type = type.BaseType;
            }
        }

        private void RegisterCallbacks(Type type)
        {
            while (type != null)
            {
                foreach (MethodInfo info in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    QueryInterceptorAttribute[] customAttributes = (QueryInterceptorAttribute[]) info.GetCustomAttributes(typeof(QueryInterceptorAttribute), true);
                    foreach (QueryInterceptorAttribute attribute in customAttributes)
                    {
                        ResourceSet set;
                        if (!this.provider.TryResolveResourceSet(attribute.EntitySetName, out set) || (set == null))
                        {
                            throw new InvalidOperationException(System.Data.Services.Strings.DataService_AttributeEntitySetNotFound(attribute.EntitySetName, info.Name, type.FullName));
                        }
                        CheckQueryInterceptorSignature(type, info, set);
                        if (!info.IsAbstract)
                        {
                            if (!this.readAuthorizationMethods.ContainsKey(set.Name))
                            {
                                this.readAuthorizationMethods[set.Name] = new List<MethodInfo>();
                            }
                            this.readAuthorizationMethods[set.Name].Add(info);
                        }
                    }
                    ChangeInterceptorAttribute[] attributeArray2 = (ChangeInterceptorAttribute[]) info.GetCustomAttributes(typeof(ChangeInterceptorAttribute), true);
                    foreach (ChangeInterceptorAttribute attribute2 in attributeArray2)
                    {
                        ResourceSet set2;
                        if (!this.provider.TryResolveResourceSet(attribute2.EntitySetName, out set2) || (set2 == null))
                        {
                            throw new InvalidOperationException(System.Data.Services.Strings.DataService_AttributeEntitySetNotFound(attribute2.EntitySetName, info.Name, type.FullName));
                        }
                        ParameterInfo[] parameters = info.GetParameters();
                        if (parameters.Length != 2)
                        {
                            throw new InvalidOperationException(System.Data.Services.Strings.DataService_ChangeInterceptorIncorrectParameterCount(info.Name, type.FullName, parameters.Length));
                        }
                        CheckParameterIsNotOut(info, parameters[0]);
                        CheckParameterIsNotOut(info, parameters[1]);
                        Type parameterType = parameters[0].ParameterType;
                        if (!parameterType.IsAssignableFrom(set2.ResourceType.InstanceType))
                        {
                            throw new InvalidOperationException(System.Data.Services.Strings.DataService_AuthorizationParameterNotAssignable(parameters[0].Name, info.Name, type.FullName, parameterType.FullName, set2.ResourceType.InstanceType));
                        }
                        if (parameters[1].ParameterType != typeof(UpdateOperations))
                        {
                            throw new InvalidOperationException(System.Data.Services.Strings.DataService_AuthorizationParameterNotResourceAction(parameters[1].Name, info.Name, type.FullName, typeof(UpdateOperations).FullName));
                        }
                        Type returnType = info.ReturnType;
                        if (returnType != typeof(void))
                        {
                            throw new InvalidOperationException(System.Data.Services.Strings.DataService_AuthorizationMethodNotVoid(info.Name, type.FullName, returnType.FullName));
                        }
                        if (!info.IsAbstract)
                        {
                            if (!this.writeAuthorizationMethods.ContainsKey(set2.Name))
                            {
                                this.writeAuthorizationMethods[set2.Name] = new List<MethodInfo>();
                            }
                            this.writeAuthorizationMethods[set2.Name].Add(info);
                        }
                    }
                }
                type = type.BaseType;
            }
        }

        public void RegisterKnownType(Type type)
        {
            this.CheckNotSealed();
            this.knownTypes.Add(type);
        }

        internal void Seal()
        {
            this.configurationSealed = true;
            this.provider = null;
        }

        public void SetEntitySetAccessRule(string name, EntitySetRights rights)
        {
            this.CheckNotSealed();
            if (name == null)
            {
                throw System.Data.Services.Error.ArgumentNull("name");
            }
            WebUtil.CheckResourceContainerRights(rights, "rights");
            if (name == "*")
            {
                this.rightsForUnspecifiedResourceContainer = rights;
            }
            else
            {
                ResourceSet set;
                if (!this.provider.TryResolveResourceSet(name, out set) || (set == null))
                {
                    throw new ArgumentException(System.Data.Services.Strings.DataServiceConfiguration_ResourceSetNameNotFound(name), "name");
                }
                this.resourceRights[set.Name] = rights;
            }
        }

        public void SetEntitySetPageSize(string name, int size)
        {
            WebUtil.CheckArgumentNull<string>(name, "name");
            if (size < 0)
            {
                throw new ArgumentOutOfRangeException("size", size, System.Data.Services.Strings.DataService_SDP_PageSizeMustbeNonNegative(size, name));
            }
            if (size == 0x7fffffff)
            {
                size = 0;
            }
            if (this.MaxResultsPerCollection != 0x7fffffff)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.DataService_SDP_PageSizeWithMaxResultsPerCollection);
            }
            this.CheckNotSealed();
            if (name == "*")
            {
                this.defaultPageSize = size;
            }
            else
            {
                ResourceSet set;
                if (!this.provider.TryResolveResourceSet(name, out set) || (set == null))
                {
                    throw new ArgumentException(System.Data.Services.Strings.DataServiceConfiguration_ResourceSetNameNotFound(name), "name");
                }
                this.pageSizes[set.Name] = size;
            }
        }

        public void SetServiceActionAccessRule(string name, ServiceActionRights rights)
        {
            this.CheckNotSealed();
            WebUtil.CheckStringArgumentNullOrEmpty(name, "name");
            WebUtil.CheckServiceActionRights(rights, "rights");
            if (name == "*")
            {
                this.rightsForUnspecifiedServiceAction = rights;
            }
            else
            {
                this.serviceActionRights[name] = rights;
            }
        }

        public void SetServiceOperationAccessRule(string name, ServiceOperationRights rights)
        {
            this.CheckNotSealed();
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            WebUtil.CheckServiceOperationRights(rights, "rights");
            if (name == "*")
            {
                this.rightsForUnspecifiedServiceOperation = rights;
            }
            else
            {
                ServiceOperation operation;
                if (!this.provider.TryResolveServiceOperation(name, out operation) || (operation == null))
                {
                    throw new ArgumentException(System.Data.Services.Strings.DataServiceConfiguration_ServiceNameNotFound(name), "name");
                }
                this.serviceOperationRights[operation.Name] = rights;
            }
        }

        internal void ValidateServerOptions()
        {
            if ((this.DataServiceBehavior.MaxProtocolVersion == DataServiceProtocolVersion.V1) && this.IsPageSizeDefined)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.DataServiceConfiguration_ServerPagingNotSupportedInV1Server);
            }
        }

        internal bool AccessEnabledForAllResourceTypes
        {
            [DebuggerStepThrough]
            get
            {
                return this.accessEnabledForAllResourceTypes;
            }
        }

        public Func<IEdmModel, IEnumerable<IEdmModel>> AnnotationsBuilder { get; set; }

        public System.Data.Services.DataServiceBehavior DataServiceBehavior
        {
            get
            {
                return this.dataServiceBehavior;
            }
        }

        public bool DisableValidationOnMetadataWrite
        {
            get
            {
                return this.disableValidationOnMetadataWrite;
            }
            set
            {
                this.CheckNotSealed();
                this.disableValidationOnMetadataWrite = value;
            }
        }

        public bool EnableTypeConversion
        {
            get
            {
                return this.typeConversion;
            }
            set
            {
                this.CheckNotSealed();
                this.typeConversion = value;
            }
        }

        private bool IsPageSizeDefined
        {
            get
            {
                if (this.pageSizes.Count <= 0)
                {
                    return (this.defaultPageSize > 0);
                }
                return true;
            }
        }

        public int MaxBatchCount
        {
            get
            {
                return this.maxBatchCount;
            }
            set
            {
                this.maxBatchCount = this.CheckNonNegativeProperty(value, "MaxBatchCount");
            }
        }

        public int MaxChangesetCount
        {
            get
            {
                return this.maxChangeSetCount;
            }
            set
            {
                this.maxChangeSetCount = this.CheckNonNegativeProperty(value, "MaxChangesetCount");
            }
        }

        public int MaxExpandCount
        {
            get
            {
                return this.maxExpandCount;
            }
            set
            {
                this.maxExpandCount = this.CheckNonNegativeProperty(value, "MaxExpandCount");
            }
        }

        public int MaxExpandDepth
        {
            get
            {
                return this.maxExpandDepth;
            }
            set
            {
                this.maxExpandDepth = this.CheckNonNegativeProperty(value, "MaxExpandDepth");
            }
        }

        public int MaxObjectCountOnInsert
        {
            get
            {
                return this.maxObjectCountOnInsert;
            }
            set
            {
                this.maxObjectCountOnInsert = this.CheckNonNegativeProperty(value, "MaxObjectCountOnInsert");
            }
        }

        public int MaxResultsPerCollection
        {
            get
            {
                return this.maxResultsPerCollection;
            }
            set
            {
                if (this.IsPageSizeDefined)
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.DataService_SDP_PageSizeWithMaxResultsPerCollection);
                }
                this.maxResultsPerCollection = this.CheckNonNegativeProperty(value, "MaxResultsPerCollection");
            }
        }

        public bool UseVerboseErrors
        {
            get
            {
                return this.useVerboseErrors;
            }
            set
            {
                this.CheckNotSealed();
                this.useVerboseErrors = value;
            }
        }
    }
}

