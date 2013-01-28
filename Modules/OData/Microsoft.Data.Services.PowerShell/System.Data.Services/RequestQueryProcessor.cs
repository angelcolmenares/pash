namespace System.Data.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Internal;
    using System.Data.Services.Parsing;
    using System.Data.Services.Providers;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    [StructLayout(LayoutKind.Sequential)]
    internal struct RequestQueryProcessor
    {
        private static readonly MethodInfo ApplyExpansionsMethodInfo;
        private readonly RequestDescription description;
        private readonly bool filterQueryApplicable;
        private readonly IDataService service;
        private readonly bool setQueryApplicable;
        private readonly bool pagingApplicable;
        private bool appliedCustomPaging;
        private List<ExpandSegmentCollection> expandPaths;
        private List<List<string>> expandPathsAsText;
        private RootProjectionNode rootProjectionNode;
        private RequestQueryParser.ExpressionParser orderingParser;
        private OrderingInfo topLevelOrderingInfo;
        private bool orderApplied;
        private int? skipCount;
        private int? topCount;
        private Expression queryExpression;
        private RequestQueryProcessor(IDataService service, RequestDescription description)
        {
            this.service = service;
            this.description = description;
            this.orderApplied = false;
            this.skipCount = null;
            this.topCount = null;
            this.queryExpression = description.RequestExpression;
            this.filterQueryApplicable = (((description.TargetKind == RequestTargetKind.Resource) || (description.TargetKind == RequestTargetKind.OpenProperty)) || (description.TargetKind == RequestTargetKind.ComplexObject)) || (description.CountOption == RequestQueryCountOption.ValueOnly);
            this.setQueryApplicable = ((description.TargetKind == RequestTargetKind.Resource) && !description.IsSingleResult) || (description.CountOption == RequestQueryCountOption.ValueOnly);
            this.pagingApplicable = (((description.TargetKind == RequestTargetKind.Resource) && !description.IsSingleResult) && ((description.CountOption != RequestQueryCountOption.ValueOnly) && !description.IsRequestForEnumServiceOperation)) && ((service.OperationContext.Host.HttpVerb == HttpVerbs.GET) || (description.SegmentInfos[0].TargetSource == RequestTargetSource.ServiceOperation));
            this.appliedCustomPaging = false;
            this.expandPaths = null;
            this.expandPathsAsText = null;
            this.rootProjectionNode = null;
            this.orderingParser = null;
            this.topLevelOrderingInfo = null;
        }

        private bool IsStandardPaged
        {
            get
            {
                return ((this.pagingApplicable && !this.IsCustomPaged) && this.IsPageSizeDefined);
            }
        }
        private bool IsPageSizeDefined
        {
            get
            {
                return (this.description.LastSegmentInfo.TargetContainer.PageSize > 0);
            }
        }
        private bool IsCustomPaged
        {
            get
            {
                return this.service.PagingProvider.IsCustomPagedForQuery;
            }
        }
        internal static void CheckEmptyQueryArguments(IDataService service, bool checkForOnlyV2QueryParameters)
        {
            DataServiceHostWrapper host = service.OperationContext.Host;
            if ((!checkForOnlyV2QueryParameters && (((!string.IsNullOrEmpty(host.GetQueryStringItem("$expand")) || !string.IsNullOrEmpty(host.GetQueryStringItem("$filter"))) || (!string.IsNullOrEmpty(host.GetQueryStringItem("$orderby")) || !string.IsNullOrEmpty(host.GetQueryStringItem("$skip")))) || !string.IsNullOrEmpty(host.GetQueryStringItem("$top")))) || ((!string.IsNullOrEmpty(host.GetQueryStringItem("$inlinecount")) || !string.IsNullOrEmpty(host.GetQueryStringItem("$select"))) || !string.IsNullOrEmpty(host.GetQueryStringItem("$skiptoken"))))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestQueryProcessor_QueryNoOptionsApplicable);
            }
        }

        internal static void CheckEmptySetQueryArguments(IDataService service)
        {
            DataServiceHostWrapper host = service.OperationContext.Host;
            if ((!string.IsNullOrEmpty(host.GetQueryStringItem("$orderby")) || !string.IsNullOrEmpty(host.GetQueryStringItem("$skip"))) || (!string.IsNullOrEmpty(host.GetQueryStringItem("$top")) || !string.IsNullOrEmpty(host.GetQueryStringItem("$inlinecount"))))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestQueryProcessor_QuerySetOptionsNotApplicable);
            }
        }

        internal static void CheckV2EmptyQueryArguments(IDataService service)
        {
            CheckEmptyQueryArguments(service, service.Provider.IsV1Provider);
        }

        internal static Expression ComposePropertyNavigation(Expression expression, LambdaExpression filterLambda, bool propagateNull, bool isSingleResult)
        {
            Expression nullLiteral = ExpressionUtils.NullLiteral;
            if (isSingleResult)
            {
                Expression right = ParameterReplacerVisitor.Replace(filterLambda.Body, filterLambda.Parameters[0], expression);
                Expression test = propagateNull ? Expression.AndAlso(Expression.NotEqual(expression, nullLiteral), right) : right;
                Expression ifTrue = expression;
                Expression ifFalse = Expression.Constant(null, ifTrue.Type);
                return Expression.Condition(test, ifTrue, ifFalse);
            }
            Type targetType = filterLambda.Parameters[0].Type;
            Expression expression7 = expression.EnumerableWhere(filterLambda);
            if (propagateNull)
            {
                Expression expression8 = Expression.Equal(expression, nullLiteral);
                Expression expression9 = expression7;
                Expression expression10 = ExpressionUtils.EnumerableEmpty(targetType);
                return Expression.Condition(expression8, expression10, expression9, expression10.Type);
            }
            return expression7;
        }

        internal static RequestDescription ProcessQuery(IDataService service, RequestDescription description)
        {
            if ((service.OperationContext.Host.HttpVerb != HttpVerbs.GET) && (description.SegmentInfos[0].TargetSource != RequestTargetSource.ServiceOperation))
            {
                CheckV2EmptyQueryArguments(service);
            }
            if (((description.RequestExpression == null) || DataServiceActionProviderWrapper.IsServiceActionRequest(description)) || !typeof(IQueryable).IsAssignableFrom(description.RequestExpression.Type))
            {
                CheckEmptyQueryArguments(service, false);
                return description;
            }
            RequestQueryProcessor processor = new RequestQueryProcessor(service, description);
            return processor.ProcessQuery();
        }

        private static List<List<string>> ReadExpandOrSelect(string value, bool select, IDataService dataService)
        {
            List<List<string>> list = new List<List<string>>();
            List<string> list2 = null;
            ExpressionLexer lexer = new ExpressionLexer(value);
            while (lexer.CurrentToken.Id != TokenId.End)
            {
                string text;
                bool flag = false;
                if (select && (lexer.CurrentToken.Id == TokenId.Star))
                {
                    text = lexer.CurrentToken.Text;
                    lexer.NextToken();
                    flag = true;
                }
                else if (select)
                {
                    bool flag2;
                    text = lexer.ReadDottedIdentifier(true);
                    if (dataService.Provider.GetNameFromContainerQualifiedName(text, out flag2) == "*")
                    {
                        flag = true;
                    }
                }
                else
                {
                    text = lexer.ReadDottedIdentifier(false);
                }
                if (list2 == null)
                {
                    list2 = new List<string> {
                        
                    };
                }
                list2.Add(text);
                TokenId id = lexer.CurrentToken.Id;
                if (id != TokenId.End)
                {
                    if (flag || (id != TokenId.Slash))
                    {
                        lexer.ValidateToken(TokenId.Comma);
                        list2 = null;
                    }
                    lexer.NextToken();
                }
            }
            return list;
        }

        private static ExpandedProjectionNode ApplyProjectionForProperty(ExpandedProjectionNode parentNode, string propertyName, ResourceProperty property, ResourceType targetResourceType, bool lastPathSegment)
        {
            if (property != null)
            {
                switch (property.TypeKind)
                {
                    case ResourceTypeKind.ComplexType:
                        if (!lastPathSegment)
                        {
                            throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestQueryProcessor_ComplexPropertyAsInnerSelectSegment(targetResourceType.FullName, propertyName));
                        }
                        break;

                    case ResourceTypeKind.Primitive:
                        if (!lastPathSegment)
                        {
                            if (property.IsOfKind(ResourcePropertyKind.Stream))
                            {
                                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestQueryProcessor_NamedStreamMustBeLastSegmentInSelect(propertyName));
                            }
                            throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestQueryProcessor_PrimitivePropertyUsedAsNavigationProperty(targetResourceType.FullName, propertyName));
                        }
                        break;

                    case ResourceTypeKind.Collection:
                        if (!lastPathSegment)
                        {
                            throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestQueryProcessor_CollectionPropertyAsInnerSelectSegment(targetResourceType.FullName, propertyName));
                        }
                        break;
                }
            }
            ExpandedProjectionNode node = parentNode.AddProjectionNode(propertyName, property, targetResourceType, lastPathSegment);
            if (lastPathSegment && (node != null))
            {
                node.ProjectionFound = true;
                node.MarkSubtreeAsProjected();
            }
            return node;
        }

        private RootProjectionNode GetRootProjectionNode()
        {
            if (this.rootProjectionNode == null)
            {
                this.rootProjectionNode = new RootProjectionNode(this.description.LastSegmentInfo.TargetContainer, this.topLevelOrderingInfo, null, this.skipCount, this.topCount, null, this.expandPaths, this.description.TargetResourceType);
            }
            return this.rootProjectionNode;
        }

        private void CheckExpandPaths()
        {
            if (this.expandPathsAsText.Count > 0)
            {
                if (this.queryExpression == null)
                {
                    throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestQueryProcessor_QueryExpandOptionNotApplicable);
                }
                if (this.description.TargetResourceType.ResourceTypeKind == ResourceTypeKind.Collection)
                {
                    throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestQueryProcessor_QueryExpandOptionNotApplicable);
                }
            }
            this.expandPaths = new List<ExpandSegmentCollection>(this.expandPathsAsText.Count);
            for (int i = this.expandPathsAsText.Count - 1; i >= 0; i--)
            {
                ExpandSegmentCollection item = this.CheckSingleExpandPath(this.expandPathsAsText[i]);
                if (item == null)
                {
                    this.expandPathsAsText.RemoveAt(i);
                }
                else
                {
                    this.expandPaths.Add(item);
                    ExpandedProjectionNode rootProjectionNode = this.GetRootProjectionNode();
                    for (int j = 0; j < item.Count; j++)
                    {
                        ExpandSegment segment = item[j];
                        ExpandedProjectionNode node2 = rootProjectionNode.AddExpandedNode(segment);
                        this.GetRootProjectionNode().ExpansionOnDerivedTypesSpecified |= rootProjectionNode.HasExpandedPropertyOnDerivedType;
                        rootProjectionNode = node2;
                    }
                    this.GetRootProjectionNode().ExpansionsSpecified = true;
                }
            }
        }

        private ExpandSegmentCollection CheckSingleExpandPath(List<string> path)
        {
            ResourceType targetResourceType = this.description.TargetResourceType;
            ResourceSetWrapper targetContainer = this.description.LastSegmentInfo.TargetContainer;
            ExpandSegmentCollection segments = new ExpandSegmentCollection(path.Count);
            bool flag = false;
            bool previousSegmentIsTypeSegment = false;
            for (int i = 0; i < path.Count; i++)
            {
                string propertyName = path[i];
                ResourcePropertyKind stream = ResourcePropertyKind.Stream;
                ResourceProperty navigationProperty = targetResourceType.TryResolvePropertyName(propertyName, stream);
                if (navigationProperty == null)
                {
                    ResourceType type2 = WebUtil.ResolveTypeIdentifier(this.service.Provider, propertyName, targetResourceType, previousSegmentIsTypeSegment);
                    if (type2 == null)
                    {
                        if (targetResourceType.IsOpenType)
                        {
                            throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.OpenNavigationPropertiesNotSupportedOnOpenTypes(propertyName));
                        }
						throw DataServiceException.CreateSyntaxError(System.Data.Services.Strings.RequestUriProcessor_PropertyNotFound(targetResourceType.FullName, propertyName));
                    }
                    this.description.VerifyProtocolVersion(RequestDescription.Version3Dot0, this.service);
                    targetResourceType = type2;
                    previousSegmentIsTypeSegment = true;
                }
                else
                {
                    previousSegmentIsTypeSegment = false;
                    if (navigationProperty.TypeKind == ResourceTypeKind.EntityType)
                    {
                        targetContainer = this.service.Provider.GetContainer(targetContainer, targetResourceType, navigationProperty);
                        if (targetContainer == null)
                        {
                            throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_InvalidPropertyNameSpecified(navigationProperty.Name, targetResourceType.FullName));
                        }
                        bool singleResult = navigationProperty.Kind == ResourcePropertyKind.ResourceReference;
                        DataServiceConfiguration.CheckResourceRightsForRead(targetContainer, singleResult);
                        Expression filter = DataServiceConfiguration.ComposeQueryInterceptors(this.service, targetContainer);
                        if (((targetContainer.PageSize != 0) && !singleResult) && !this.IsCustomPaged)
                        {
                            OrderingInfo orderingInfo = new OrderingInfo(true);
                            ParameterExpression expression = Expression.Parameter(targetContainer.ResourceType.InstanceType, "p");
                            foreach (ResourceProperty property2 in targetContainer.GetKeyPropertiesForOrderBy())
                            {
                                Expression expression3;
                                if (property2.CanReflectOnInstanceTypeProperty)
                                {
                                    expression3 = Expression.Property(expression, targetContainer.ResourceType.GetPropertyInfo(property2));
                                }
                                else
                                {
                                    expression3 = Expression.Convert(Expression.Call(null, DataServiceProviderMethods.GetValueMethodInfo, expression, Expression.Constant(property2)), property2.Type);
                                }
                                orderingInfo.Add(new OrderingExpression(Expression.Lambda(expression3, new ParameterExpression[] { expression }), true));
                            }
                            segments.Add(new ExpandSegment(navigationProperty.Name, filter, targetContainer.PageSize, targetContainer, targetResourceType, navigationProperty, orderingInfo));
                            this.description.VerifyProtocolVersion(RequestDescription.Version2Dot0, this.service);
                            this.description.VerifyAndRaiseResponseVersion(RequestDescription.Version2Dot0, this.service);
                        }
                        else
                        {
                            if (!singleResult && this.IsCustomPaged)
                            {
                                this.CheckAndApplyCustomPaging(null);
                            }
                            segments.Add(new ExpandSegment(navigationProperty.Name, filter, this.service.Configuration.MaxResultsPerCollection, targetContainer, targetResourceType, navigationProperty, null));
                        }
                        this.description.UpdateAndCheckEpmFeatureVersion(targetContainer, this.service);
                        this.description.UpdateVersions(this.service.OperationContext.Host.RequestAccept, targetContainer, this.service);
                        flag = false;
                        targetResourceType = navigationProperty.ResourceType;
                    }
                    else
                    {
                        flag = true;
                    }
                }
            }
            if (previousSegmentIsTypeSegment)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestQueryProcessor_QueryParametersPathCannotEndInTypeIdentifier("$expand", targetResourceType.FullName));
            }
            if (!flag)
            {
                return segments;
            }
            return null;
        }

        private void CheckFilterQueryApplicable()
        {
            if (!this.filterQueryApplicable)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestQueryProcessor_QueryFilterOptionNotApplicable);
            }
        }

        private void CheckSetQueryApplicable()
        {
            if (!this.setQueryApplicable)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestQueryProcessor_QuerySetOptionsNotApplicable);
            }
        }

        private void ProcessExpand()
        {
            string queryStringItem = this.service.OperationContext.Host.GetQueryStringItem("$expand");
            this.expandPathsAsText = string.IsNullOrEmpty(queryStringItem) ? new List<List<string>>() : ReadExpandOrSelect(queryStringItem, false, this.service);
            this.CheckExpandPaths();
            this.service.InternalApplyingExpansions(this.queryExpression, this.expandPaths);
        }

        private void ApplyProjectionsToExpandTree(List<List<string>> selectPathsAsText)
        {
            for (int i = selectPathsAsText.Count - 1; i >= 0; i--)
            {
                List<string> list = selectPathsAsText[i];
                ExpandedProjectionNode rootProjectionNode = this.GetRootProjectionNode();
                ResourceType type = null;
                for (int j = 0; j < list.Count; j++)
                {
                    bool flag2;
                    string containerQualifiedName = list[j];
                    bool lastPathSegment = j == (list.Count - 1);
                    rootProjectionNode.ProjectionFound = true;
                    if (containerQualifiedName == "*")
                    {
                        rootProjectionNode.ProjectAllImmediateProperties = true;
                        break;
                    }
                    if (this.service.Provider.GetNameFromContainerQualifiedName(containerQualifiedName, out flag2) == "*")
                    {
                        rootProjectionNode.ProjectAllImmediateOperations = true;
                        break;
                    }
                    ResourceType previousSegmentResourceType = type ?? rootProjectionNode.ResourceType;
                    ResourceProperty property = previousSegmentResourceType.TryResolvePropertyName(containerQualifiedName);
                    if (property == null)
                    {
                        type = WebUtil.ResolveTypeIdentifier(this.service.Provider, containerQualifiedName, previousSegmentResourceType, type != null);
                        if (type != null)
                        {
                            this.description.VerifyProtocolVersion(RequestDescription.Version3Dot0, this.service);
                            continue;
                        }
                        Func<OperationWrapper, bool> predicate = null;
                        string serviceActionName = this.service.Provider.GetNameFromContainerQualifiedName(containerQualifiedName, out flag2);
                        OperationWrapper operation = null;
                        if (!previousSegmentResourceType.IsOpenType || flag2)
                        {
                            if (predicate == null)
                            {
                                predicate = o => o.Name == serviceActionName;
                            }
                            operation = this.service.ActionProvider.GetServiceActionsByBindingParameterType(this.service.OperationContext, previousSegmentResourceType).SingleOrDefault<OperationWrapper>(predicate);
                        }
                        if (operation != null)
                        {
                            rootProjectionNode.AddOperation(operation);
                            if (!lastPathSegment)
                            {
                                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestQueryProcessor_ServiceActionMustBeLastSegmentInSelect(containerQualifiedName));
                            }
                            continue;
                        }
                        if (!previousSegmentResourceType.IsOpenType)
                        {
							throw DataServiceException.CreateSyntaxError(System.Data.Services.Strings.RequestUriProcessor_PropertyNotFound(previousSegmentResourceType.FullName, containerQualifiedName));
                        }
                        if (!lastPathSegment)
                        {
                            throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.OpenNavigationPropertiesNotSupportedOnOpenTypes(containerQualifiedName));
                        }
                    }
                    rootProjectionNode = ApplyProjectionForProperty(rootProjectionNode, containerQualifiedName, property, previousSegmentResourceType, lastPathSegment);
                    type = null;
                }
                if (type != null)
                {
                    throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestQueryProcessor_QueryParametersPathCannotEndInTypeIdentifier("$select", type.FullName));
                }
            }
        }

        private void ProcessSelect()
        {
            List<List<string>> list;
            string queryStringItem = this.service.OperationContext.Host.GetQueryStringItem("$select");
            if (!string.IsNullOrEmpty(queryStringItem))
            {
                if (!this.service.Configuration.DataServiceBehavior.AcceptProjectionRequests)
                {
                    throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataServiceConfiguration_ProjectionsNotAccepted);
                }
                list = ReadExpandOrSelect(queryStringItem, true, this.service);
            }
            else
            {
                if (this.rootProjectionNode != null)
                {
                    this.rootProjectionNode.MarkSubtreeAsProjected();
                }
                return;
            }
            if ((list.Count > 0) && (this.queryExpression == null))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestQueryProcessor_QuerySelectOptionNotApplicable);
            }
            if ((this.description.TargetResourceType == null) || (this.description.TargetResourceType.ResourceTypeKind != ResourceTypeKind.EntityType))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestQueryProcessor_QuerySelectOptionNotApplicable);
            }
            if (this.description.SegmentInfos.Any<SegmentInfo>(si => si.TargetKind == RequestTargetKind.Link))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestQueryProcessor_QuerySelectOptionNotApplicable);
            }
            this.description.VerifyProtocolVersion(RequestDescription.Version2Dot0, this.service);
            this.description.VerifyRequestVersion(RequestDescription.Version2Dot0, this.service);
            this.GetRootProjectionNode().ProjectionsSpecified = true;
            this.ApplyProjectionsToExpandTree(list);
            if (this.rootProjectionNode != null)
            {
                this.rootProjectionNode.RemoveNonProjectedNodes();
                this.rootProjectionNode.ApplyWildcardsAndSort(this.service.Provider);
            }
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        private void GenerateQueryResult()
        {
            if (this.description.CountOption == RequestQueryCountOption.ValueOnly)
            {
                this.description.VerifyProtocolVersion(RequestDescription.Version2Dot0, this.service);
                this.description.VerifyRequestVersion(RequestDescription.Version2Dot0, this.service);
                this.description.VerifyAndRaiseResponseVersion(RequestDescription.Version2Dot0, this.service);
                this.queryExpression = this.queryExpression.QueryableLongCount();
            }
            else if (this.rootProjectionNode != null)
            {
                IExpandProvider service = this.service.Provider.GetService<IExpandProvider>();
                if (service != null)
                {
                    if (this.IsStandardPaged)
                    {
                        throw new DataServiceException(500, System.Data.Services.Strings.DataService_SDP_TopLevelPagedResultWithOldExpandProvider);
                    }
                    if (this.rootProjectionNode.ProjectionsSpecified)
                    {
                        throw new DataServiceException(500, System.Data.Services.Strings.DataService_Projections_ProjectionsWithOldExpandProvider);
                    }
                    if (this.rootProjectionNode.ExpansionOnDerivedTypesSpecified)
                    {
                        throw new DataServiceException(500, System.Data.Services.Strings.DataService_DerivedExpansions_OldExpandProvider);
                    }
                    this.ProcessOrderBy();
                    this.ProcessSkipAndTop();
                    this.queryExpression = Expression.Call(Expression.Constant(service), ApplyExpansionsMethodInfo, this.queryExpression, Expression.Constant(this.rootProjectionNode.ExpandPaths));
                    this.rootProjectionNode.UseExpandPathsForSerialization = true;
                }
                else
                {
                    IProjectionProvider projectionProvider = this.service.Provider.ProjectionProvider;
                    if (projectionProvider == null)
                    {
                        projectionProvider = new BasicExpandProvider(this.service.Provider, false, true);
                    }
                    this.queryExpression = Expression.Call(null, DataServiceExecutionProviderMethods.ApplyProjectionsMethodInfo, Expression.Constant(projectionProvider, typeof(object)), this.queryExpression, Expression.Constant(this.rootProjectionNode, typeof(object)));
                }
            }
            else if (!string.IsNullOrEmpty(this.service.OperationContext.Host.GetQueryStringItem("$expand")))
            {
                this.ProjectSkipTokenForNonExpand();
                this.ProcessOrderBy();
                this.ProcessSkipAndTop();
            }
        }

        private void ProcessFilter()
        {
            string queryStringItem = this.service.OperationContext.Host.GetQueryStringItem("$filter");
            if (!string.IsNullOrEmpty(queryStringItem))
            {
                this.CheckFilterQueryApplicable();
                this.queryExpression = RequestQueryParser.Where(this.service, this.description, this.queryExpression, queryStringItem);
            }
        }

        private void ProcessSkipToken()
        {
            string queryStringItem = this.service.OperationContext.Host.GetQueryStringItem("$skiptoken");
            if (this.pagingApplicable)
            {
                if (this.IsCustomPaged)
                {
                    this.ApplyCustomPaging(queryStringItem);
                }
                else
                {
                    this.ApplyStandardPaging(queryStringItem);
                }
            }
            else if (!string.IsNullOrEmpty(queryStringItem))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestQueryProcessor_SkipTokenNotAllowed);
            }
        }

        private void ApplyStandardPaging(string skipToken)
        {
            if (!string.IsNullOrEmpty(skipToken))
            {
                KeyInstance instance;
                if (!this.IsPageSizeDefined)
                {
                    throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestQueryProcessor_SkipTokenSupportedOnPagedSets);
                }
                WebUtil.CheckSyntaxValid(KeyInstance.TryParseNullableTokens(skipToken, out instance));
                if (this.topLevelOrderingInfo.OrderingExpressions.Count != instance.PositionalValues.Count)
                {
                    throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataService_SDP_SkipTokenNotMatchingOrdering(instance.PositionalValues.Count, skipToken, this.topLevelOrderingInfo.OrderingExpressions.Count));
                }
                this.queryExpression = this.queryExpression.QueryableWhere(this.orderingParser.BuildSkipTokenFilter(this.topLevelOrderingInfo, instance));
                this.description.VerifyProtocolVersion(RequestDescription.Version2Dot0, this.service);
                this.description.VerifyRequestVersion(RequestDescription.Version2Dot0, this.service);
                this.description.VerifyAndRaiseResponseVersion(RequestDescription.Version2Dot0, this.service);
            }
        }

        private void ApplyCustomPaging(string skipToken)
        {
            if (!string.IsNullOrEmpty(skipToken))
            {
                KeyInstance instance;
                WebUtil.CheckSyntaxValid(KeyInstance.TryParseNullableTokens(skipToken, out instance));
                ParameterExpression parameterForIt = Expression.Parameter(this.description.LastSegmentInfo.TargetResourceType.InstanceType, "it");
                RequestQueryParser.ExpressionParser parser = new RequestQueryParser.ExpressionParser(this.service, this.description, parameterForIt, string.Empty);
                object[] skipTokenValues = new object[instance.PositionalValues.Count];
                int num = 0;
                foreach (object obj2 in instance.PositionalValues)
                {
                    skipTokenValues[num++] = parser.ParseSkipTokenLiteral((string) obj2);
                }
                this.CheckAndApplyCustomPaging(skipTokenValues);
                this.description.VerifyProtocolVersion(RequestDescription.Version2Dot0, this.service);
                this.description.VerifyRequestVersion(RequestDescription.Version2Dot0, this.service);
            }
            else
            {
                this.CheckAndApplyCustomPaging(null);
            }
        }

        private void ProcessOrderBy()
        {
            if (this.topLevelOrderingInfo.OrderingExpressions.Count > 0)
            {
                this.queryExpression = RequestQueryParser.OrderBy(this.service, this.queryExpression, this.topLevelOrderingInfo);
                this.orderApplied = true;
            }
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        private void ProcessCount()
        {
            string queryStringItem = this.service.OperationContext.Host.GetQueryStringItem("$inlinecount");
            if (!string.IsNullOrEmpty(queryStringItem))
            {
                if (!this.service.Configuration.DataServiceBehavior.AcceptCountRequests)
                {
                    throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataServiceConfiguration_CountNotAccepted);
                }
                queryStringItem = queryStringItem.TrimStart(new char[0]);
                if (!queryStringItem.Equals("none"))
                {
                    if (this.service.OperationContext.Host.HttpVerb != HttpVerbs.GET)
                    {
                        throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestQueryProcessor_RequestVerbCannotCountError);
                    }
                    if (this.description.CountOption == RequestQueryCountOption.ValueOnly)
                    {
                        throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestQueryProcessor_InlineCountWithValueCount);
                    }
                    this.CheckSetQueryApplicable();
                    if (queryStringItem != "allpages")
                    {
                        throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestQueryProcessor_InvalidCountOptionError);
                    }
                    Expression requestExpression = this.queryExpression.QueryableLongCount();
                    this.description.CountValue = (long) this.service.ExecutionProvider.Execute(requestExpression);
                    this.description.CountOption = RequestQueryCountOption.Inline;
                    this.description.VerifyProtocolVersion(RequestDescription.Version2Dot0, this.service);
                    this.description.VerifyRequestVersion(RequestDescription.Version2Dot0, this.service);
                    this.description.VerifyAndRaiseResponseVersion(RequestDescription.Version2Dot0, this.service);
                }
            }
        }

        private void ObtainOrderingExpressions()
        {
            StringBuilder builder = new StringBuilder(this.service.OperationContext.Host.GetQueryStringItem("$orderby"));
            if (builder.Length > 0)
            {
                this.CheckSetQueryApplicable();
            }
            ResourceType targetResourceType = this.description.TargetResourceType;
            this.topLevelOrderingInfo = new OrderingInfo(this.IsStandardPaged);
            if ((this.IsStandardPaged || this.topCount.HasValue) || this.skipCount.HasValue)
            {
                string str = (builder.Length > 0) ? "," : string.Empty;
                foreach (ResourceProperty property in this.description.TargetResourceSet.GetKeyPropertiesForOrderBy())
                {
                    builder.Append(str).Append(property.Name).Append(' ').Append("asc");
                    str = ",";
                }
            }
            string str2 = builder.ToString();
            if (!string.IsNullOrEmpty(str2))
            {
                ParameterExpression parameterForIt = Expression.Parameter(targetResourceType.InstanceType, "element");
                this.orderingParser = new RequestQueryParser.ExpressionParser(this.service, this.description, parameterForIt, str2);
                foreach (OrderingExpression expression2 in this.orderingParser.ParseOrdering())
                {
                    this.topLevelOrderingInfo.Add(new OrderingExpression(Expression.Lambda(expression2.Expression, new ParameterExpression[] { parameterForIt }), expression2.IsAscending));
                }
                if (this.IsStandardPaged)
                {
                    this.description.SkipTokenExpressionCount = this.topLevelOrderingInfo.OrderingExpressions.Count;
                    this.description.SkipTokenProperties = NeedSkipTokenVisitor.CollectSkipTokenProperties(this.topLevelOrderingInfo, targetResourceType);
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        private RequestDescription ProcessQuery()
        {
            this.ObtainSkipTopCounts();
            this.ObtainOrderingExpressions();
            this.ProcessFilter();
            this.ProcessCount();
            this.ProcessSkipToken();
            if (string.IsNullOrEmpty(this.service.OperationContext.Host.GetQueryStringItem("$expand")) && string.IsNullOrEmpty(this.service.OperationContext.Host.GetQueryStringItem("$select")))
            {
                this.ProjectSkipTokenForNonExpand();
                this.ProcessOrderBy();
                this.ProcessSkipAndTop();
            }
            else if (this.description.CountOption == RequestQueryCountOption.ValueOnly)
            {
                this.ProcessOrderBy();
                this.ProcessSkipAndTop();
            }
            this.ProcessExpand();
            this.ProcessSelect();
            this.GenerateQueryResult();
            return new RequestDescription(this.description, this.queryExpression, this.rootProjectionNode);
        }

        private void ProjectSkipTokenForNonExpand()
        {
            if (this.IsStandardPaged && (this.description.SkipTokenProperties == null))
            {
                Type type = this.queryExpression.ElementType();
                ParameterExpression expandParameter = Expression.Parameter(type, "p");
                StringBuilder skipTokenDescription = new StringBuilder();
                Type skipTokenWrapperTypeAndDescription = this.GetSkipTokenWrapperTypeAndDescription(skipTokenDescription);
                MemberBinding[] bindings = this.GetSkipTokenBindings(skipTokenWrapperTypeAndDescription, skipTokenDescription.ToString(), expandParameter);
                Type wrapperType = WebUtil.GetWrapperType(new Type[] { type, skipTokenWrapperTypeAndDescription }, null);
                MemberBinding[] bindingArray2 = new MemberBinding[] { Expression.Bind(wrapperType.GetProperty("ExpandedElement"), expandParameter), Expression.Bind(wrapperType.GetProperty("Description"), Expression.Constant("$skiptoken")), Expression.Bind(wrapperType.GetProperty("ProjectedProperty0"), Expression.MemberInit(Expression.New(skipTokenWrapperTypeAndDescription), bindings)) };
                LambdaExpression selector = Expression.Lambda(Expression.MemberInit(Expression.New(wrapperType), bindingArray2), new ParameterExpression[] { expandParameter });
                this.queryExpression = this.queryExpression.QueryableSelect(selector);
                this.UpdateOrderingInfoWithSkipTokenWrapper(wrapperType);
            }
        }

        private Type GetSkipTokenWrapperTypeAndDescription(StringBuilder skipTokenDescription)
        {
            Type[] wrapperParameters = new Type[this.topLevelOrderingInfo.OrderingExpressions.Count + 1];
            wrapperParameters[0] = this.queryExpression.ElementType();
            int num = 0;
            string str = string.Empty;
            foreach (OrderingExpression expression in this.topLevelOrderingInfo.OrderingExpressions)
            {
                wrapperParameters[num + 1] = ((LambdaExpression) expression.Expression).Body.Type;
                skipTokenDescription.Append(str).Append("SkipTokenProperty" + num.ToString(CultureInfo.InvariantCulture));
                str = ",";
                num++;
            }
            return WebUtil.GetWrapperType(wrapperParameters, new Func<object, string>(System.Data.Services.Strings.BasicExpandProvider_SDP_UnsupportedOrderingExpressionBreadth));
        }

        private MemberBinding[] GetSkipTokenBindings(Type skipTokenWrapperType, string skipTokenDescription, ParameterExpression expandParameter)
        {
            MemberBinding[] bindingArray = new MemberBinding[this.topLevelOrderingInfo.OrderingExpressions.Count + 2];
            bindingArray[0] = Expression.Bind(skipTokenWrapperType.GetProperty("ExpandedElement"), expandParameter);
            bindingArray[1] = Expression.Bind(skipTokenWrapperType.GetProperty("Description"), Expression.Constant(skipTokenDescription.ToString()));
            int num = 0;
            foreach (OrderingExpression expression in this.topLevelOrderingInfo.OrderingExpressions)
            {
                LambdaExpression expression2 = (LambdaExpression) expression.Expression;
                Expression expression3 = ParameterReplacerVisitor.Replace(expression2.Body, expression2.Parameters[0], expandParameter);
                MemberInfo property = skipTokenWrapperType.GetProperty("ProjectedProperty" + num.ToString(CultureInfo.InvariantCulture));
                bindingArray[num + 2] = Expression.Bind(property, expression3);
                num++;
            }
            return bindingArray;
        }

        private void UpdateOrderingInfoWithSkipTokenWrapper(Type resultWrapperType)
        {
            OrderingInfo info = new OrderingInfo(true);
            ParameterExpression expression = Expression.Parameter(resultWrapperType, "w");
            foreach (OrderingExpression expression2 in this.topLevelOrderingInfo.OrderingExpressions)
            {
                LambdaExpression expression3 = (LambdaExpression) expression2.Expression;
                Expression body = ParameterReplacerVisitor.Replace(expression3.Body, expression3.Parameters[0], Expression.MakeMemberAccess(expression, resultWrapperType.GetProperty("ExpandedElement")));
                info.Add(new OrderingExpression(Expression.Lambda(body, new ParameterExpression[] { expression }), expression2.IsAscending));
            }
            this.topLevelOrderingInfo = info;
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        private void ProcessSkipAndTop()
        {
            if (this.skipCount.HasValue)
            {
                this.queryExpression = this.queryExpression.QueryableSkip(this.skipCount.Value);
            }
            if (this.topCount.HasValue)
            {
                this.queryExpression = this.queryExpression.QueryableTake(this.topCount.Value);
            }
        }

        private void ObtainSkipTopCounts()
        {
            int num;
            if (this.ReadSkipOrTopArgument("$skip", out num))
            {
                this.skipCount = new int?(num);
            }
            int pageSize = 0;
            if (this.IsStandardPaged)
            {
                pageSize = this.description.LastSegmentInfo.TargetContainer.PageSize;
            }
            if (this.ReadSkipOrTopArgument("$top", out num))
            {
                this.topCount = new int?(num);
                if (this.IsStandardPaged && (pageSize < this.topCount.Value))
                {
                    this.description.VerifyProtocolVersion(RequestDescription.Version2Dot0, this.service);
                    this.description.VerifyAndRaiseResponseVersion(RequestDescription.Version2Dot0, this.service);
                    this.topCount = new int?(pageSize);
                }
            }
            else if (this.IsStandardPaged)
            {
                this.description.VerifyProtocolVersion(RequestDescription.Version2Dot0, this.service);
                this.description.VerifyAndRaiseResponseVersion(RequestDescription.Version2Dot0, this.service);
                this.topCount = new int?(pageSize);
            }
            if (this.topCount.HasValue || this.skipCount.HasValue)
            {
                this.CheckSetQueryApplicable();
            }
        }

        private bool ReadSkipOrTopArgument(string queryItem, out int count)
        {
            string queryStringItem = this.service.OperationContext.Host.GetQueryStringItem(queryItem);
            if (string.IsNullOrEmpty(queryStringItem))
            {
                count = 0;
                return false;
            }
            if (!int.TryParse(queryStringItem, NumberStyles.Integer, CultureInfo.InvariantCulture, out count))
            {
                throw DataServiceException.CreateSyntaxError(System.Data.Services.Strings.RequestQueryProcessor_IncorrectArgumentFormat(queryItem, queryStringItem));
            }
            return true;
        }

        private void CheckAndApplyCustomPaging(object[] skipTokenValues)
        {
            if (!this.appliedCustomPaging)
            {
                MethodInfo method = DataServiceExecutionProviderMethods.SetContinuationTokenMethodInfo.MakeGenericMethod(new Type[] { this.queryExpression.ElementType() });
                this.queryExpression = Expression.Call(null, method, new Expression[] { Expression.Constant(this.service.PagingProvider.PagingProviderInterface, typeof(IDataServicePagingProvider)), this.queryExpression, Expression.Constant(this.description.LastSegmentInfo.TargetResourceType), Expression.Constant(skipTokenValues, typeof(object[])) });
                this.description.VerifyProtocolVersion(RequestDescription.Version2Dot0, this.service);
                this.description.VerifyAndRaiseResponseVersion(RequestDescription.Version2Dot0, this.service);
                this.appliedCustomPaging = true;
            }
        }

        static RequestQueryProcessor()
        {
            ApplyExpansionsMethodInfo = typeof(IExpandProvider).GetMethod("ApplyExpansions", BindingFlags.Public | BindingFlags.Instance);
        }
    }
}

