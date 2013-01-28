namespace System.Data.Services.Providers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Data.Services.Internal;
    using System.Data.Services.Parsing;
    using System.Data.Services.Serializers;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class BasicExpandProvider : IProjectionProvider
    {
        private readonly bool castToObject;
        private readonly bool expanded;
        private readonly DataServiceProviderWrapper provider;

        internal BasicExpandProvider(DataServiceProviderWrapper provider, bool expanded, bool castToObject)
        {
            this.provider = provider;
            this.expanded = expanded;
            this.castToObject = castToObject;
        }

        private static Expression ApplyOrderBy(Expression queryExpression, LambdaExpression orderExpression, bool firstOrder, bool isAscending)
        {
            if (firstOrder)
            {
                if (!isAscending)
                {
                    return queryExpression.QueryableOrderByDescending(orderExpression);
                }
                return queryExpression.QueryableOrderBy(orderExpression);
            }
            if (!isAscending)
            {
                return queryExpression.QueryableThenByDescending(orderExpression);
            }
            return queryExpression.QueryableThenBy(orderExpression);
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        internal static Expression ApplyOrderSkipTakeOnTopLevelResultAfterProjection(Expression queryExpression, OrderingInfo orderingInfo, int? skipCount, int? takeCount, ExpandNode root)
        {
            if (orderingInfo != null)
            {
                ParameterExpression expression = Expression.Parameter(queryExpression.ElementType(), "p");
                Expression newExpression = expression;
                if (root.RequiresExpandedWrapper)
                {
                    newExpression = Expression.Property(expression, "ExpandedElement");
                }
                bool firstOrder = true;
                foreach (OrderingExpression expression3 in orderingInfo.OrderingExpressions)
                {
                    LambdaExpression expression4 = (LambdaExpression) expression3.Expression;
                    LambdaExpression orderExpression = Expression.Lambda(ParameterReplacerVisitor.Replace(expression4.Body, expression4.Parameters[0], newExpression), new ParameterExpression[] { expression });
                    queryExpression = ApplyOrderBy(queryExpression, orderExpression, firstOrder, expression3.IsAscending);
                    firstOrder = false;
                }
            }
            return ApplySkipTakeOnTopLevelResult(queryExpression, skipCount, takeCount);
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        internal static Expression ApplyOrderSkipTakeOnTopLevelResultBeforeProjections(Expression queryExpression, OrderingInfo orderingInfo, int? skipCount, int? takeCount)
        {
            if ((orderingInfo != null) && (orderingInfo.OrderingExpressions.Count > 0))
            {
                ParameterExpression newExpression = Expression.Parameter(queryExpression.ElementType(), "p");
                bool firstOrder = true;
                foreach (OrderingExpression expression2 in orderingInfo.OrderingExpressions)
                {
                    LambdaExpression expression = (LambdaExpression) expression2.Expression;
                    LambdaExpression orderExpression = Expression.Lambda(ParameterReplacerVisitor.Replace(expression.Body, expression.Parameters[0], newExpression), new ParameterExpression[] { newExpression });
                    queryExpression = ApplyOrderBy(queryExpression, orderExpression, firstOrder, expression2.IsAscending);
                    firstOrder = false;
                }
            }
            return ApplySkipTakeOnTopLevelResult(queryExpression, skipCount, takeCount);
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public IQueryable ApplyProjections(IQueryable source, RootProjectionNode rootProjectionNode)
        {
            Expression expression = source.Expression;
            ExpandNode root = ExpandNode.BuildExpansionAndProjectionTree(this, rootProjectionNode);
            bool flag = ((root.IsV1Compatible && this.provider.IsV1Provider) && (!rootProjectionNode.ProjectionsSpecified && !rootProjectionNode.OrderingInfo.IsPaged)) && !rootProjectionNode.ExpansionOnDerivedTypesSpecified;
            root.AssignTypeForExpected(expression.ElementType(), false);
            if (!flag)
            {
                expression = ApplyOrderSkipTakeOnTopLevelResultBeforeProjections(expression, rootProjectionNode.OrderingInfo, rootProjectionNode.SkipCount, rootProjectionNode.TakeCount);
            }
            Expression queryExpression = root.BuildProjectionQuery(expression);
            if (flag)
            {
                queryExpression = ApplyOrderSkipTakeOnTopLevelResultAfterProjection(queryExpression, rootProjectionNode.OrderingInfo, rootProjectionNode.SkipCount, rootProjectionNode.TakeCount, root);
            }
            IQueryable queryable = queryExpression.CreateQuery(source.Provider);
            if (!WebUtil.IsExpandedWrapperType(queryable.ElementType))
            {
                return ProjectedWrapper.WrapQueryable(queryable);
            }
            Type type = typeof(ExpandedQueryable<>).MakeGenericType(new Type[] { queryable.ElementType });
            object[] args = new object[] { queryable };
            return (IQueryable) Activator.CreateInstance(type, args);
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        internal static Expression ApplySkipTakeOnTopLevelResult(Expression queryExpression, int? skipCount, int? takeCount)
        {
            if (skipCount.HasValue)
            {
                queryExpression = queryExpression.QueryableSkip(skipCount.Value);
            }
            if (takeCount.HasValue)
            {
                queryExpression = queryExpression.QueryableTake(takeCount.Value);
            }
            return queryExpression;
        }

        internal DataServiceProviderWrapper Provider
        {
            get
            {
                return this.provider;
            }
        }

        internal class AddPropertyAccessesAsProjectedPropertiesVisitor : PropertyAccessVisitor
        {
            private readonly Dictionary<Expression, BasicExpandProvider.ExpandNode> expandNodeAnnotations;

            private AddPropertyAccessesAsProjectedPropertiesVisitor(Dictionary<Expression, BasicExpandProvider.ExpandNode> expandNodeAnnotations)
            {
                this.expandNodeAnnotations = expandNodeAnnotations;
            }

            internal static void AddPropertyAccessesAsProjectedProperties(Expression expression, Dictionary<Expression, BasicExpandProvider.ExpandNode> expandNodeAnnotations)
            {
                new BasicExpandProvider.AddPropertyAccessesAsProjectedPropertiesVisitor(expandNodeAnnotations).Visit(expression);
            }

            protected override bool ProcessPropertyAccess(string propertyName, ref Expression operandExpression, ref Expression accessExpression)
            {
                BasicExpandProvider.ExpandNode node;
                this.Visit(operandExpression);
                if (this.expandNodeAnnotations.TryGetValue(operandExpression, out node))
                {
                    ResourcePropertyKind stream = ResourcePropertyKind.Stream;
                    ResourceProperty property = node.BaseResourceType.TryResolvePropertyName(propertyName, stream);
                    node.AddProjectedProperty(propertyName, property, node.BaseResourceType, (property == null) ? null : DataServiceProviderWrapper.GetDeclaringTypeForProperty(node.BaseResourceType, property, null));
                }
                return true;
            }
        }

        internal abstract class ExpandedEnumerator
        {
            protected ExpandedEnumerator()
            {
            }

			protected abstract IEnumerator GetInnerEnumerator();

			internal IEnumerator GetInternalInnerEnumerator ()
			{
				return GetInnerEnumerator();
			}

            internal static IEnumerator UnwrapEnumerator(IEnumerator enumerator)
            {
                BasicExpandProvider.ExpandedEnumerator enumerator2 = enumerator as BasicExpandProvider.ExpandedEnumerator;
                if (enumerator2 == null)
                {
                    return ProjectedWrapper.UnwrapEnumerator(enumerator);
                }
                return enumerator2.GetInnerEnumerator();
            }
        }

		internal static class ExpandedEnumeratorEx
		{
			internal static IEnumerator UnwrapEnumerator(IEnumerator enumerator)
			{
				BasicExpandProvider.ExpandedEnumerator enumerator2 = enumerator as BasicExpandProvider.ExpandedEnumerator;
				if (enumerator2 == null)
				{
					return ProjectedWrapper.UnwrapEnumerator(enumerator);
				}
				return enumerator2.GetInternalInnerEnumerator();
			}
		}


        internal class ExpandedEnumerator<TWrapper> : BasicExpandProvider.ExpandedEnumerator, IEnumerator, IDisposable, IExpandedResult where TWrapper: IExpandedResult
        {
            private readonly IEnumerator<TWrapper> e;

            internal ExpandedEnumerator(IEnumerator<TWrapper> enumerator)
            {
                WebUtil.CheckArgumentNull<IEnumerator<TWrapper>>(enumerator, "enumerator");
                this.e = enumerator;
            }

            public void Dispose()
            {
                this.e.Dispose();
                GC.SuppressFinalize(this);
            }

            public object GetExpandedPropertyValue(string name)
            {
                return this.e.Current.GetExpandedPropertyValue(name);
            }

            protected override IEnumerator GetInnerEnumerator()
            {
                return this.e;
            }

            public bool MoveNext()
            {
                return this.e.MoveNext();
            }

            public void Reset()
            {
                throw System.Data.Services.Error.NotImplemented();
            }

            public object Current
            {
                get
                {
                    return this.e.Current.ExpandedElement;
                }
            }

            public object ExpandedElement
            {
                get
                {
                    return this.Current;
                }
            }
        }

        internal class ExpandedQueryable<TWrapper> : IQueryable, IEnumerable where TWrapper: IExpandedResult
        {
            private readonly IQueryable<TWrapper> source;

            public ExpandedQueryable(IQueryable<TWrapper> source)
            {
                this.source = source;
            }

            public IEnumerator GetEnumerator()
            {
                return new BasicExpandProvider.ExpandedEnumerator<TWrapper>(this.source.GetEnumerator());
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public Type ElementType
            {
                get
                {
                    return this.source.ElementType;
                }
            }

            public System.Linq.Expressions.Expression Expression
            {
                get
                {
                    return this.source.Expression;
                }
            }

            public IQueryProvider Provider
            {
                get
                {
                    throw System.Data.Services.Error.NotSupported();
                }
            }
        }

        [DebuggerDisplay("ExpandNode {Node}")]
        internal class ExpandNode
        {
            private readonly List<BasicExpandProvider.ExpandNode> children;
            private Type elementType;
            private Type enumeratedType;
            private readonly BasicExpandProvider expandProvider;
            private bool hasFilterWithin;
            private bool isRoot;
            private bool isV1Compatible;
            private readonly bool needSkipToken;
            private readonly ExpandedProjectionNode Node;
            private readonly System.Data.Services.Providers.OrderingInfo orderingInfo;
            private List<BasicExpandProvider.ProjectedProperty> projectedProperties;
            private readonly List<BasicExpandProvider.ProjectedProperty> projectedPropertyCandidates;
            private bool requiresExpandedWrapper;
            private List<ResourceType> resourceTypes;
            private bool singleResult;

            internal ExpandNode(ExpandedProjectionNode node, BasicExpandProvider provider)
            {
                this.Node = node;
                this.expandProvider = provider;
                this.orderingInfo = this.Node.OrderingInfo;
                this.needSkipToken = NeedSkipTokenVisitor.IsSkipTokenRequired(this.orderingInfo);
                this.children = new List<BasicExpandProvider.ExpandNode>();
                this.isV1Compatible = true;
                if (!node.ProjectAllProperties)
                {
                    this.projectedPropertyCandidates = new List<BasicExpandProvider.ProjectedProperty>();
                }
            }

            private static Expression AccessOpenProperty(Expression source, string propertyName)
            {
                return Expression.Call(null, OpenTypeMethods.GetValueOpenPropertyMethodInfo, source, Expression.Constant(propertyName));
            }

            private static Expression AccessProperty(Expression source, ResourceType resourceType, ResourceProperty property, bool nullPropagationRequired)
            {
                Expression expression;
                if (property.CanReflectOnInstanceTypeProperty)
                {
                    expression = Expression.Property(source, resourceType.GetPropertyInfo(property));
                }
                else if (property.Kind == ResourcePropertyKind.ResourceSetReference)
                {
                    MethodInfo method = DataServiceProviderMethods.GetSequenceValueMethodInfo.MakeGenericMethod(new Type[] { property.ResourceType.InstanceType });
                    expression = Expression.Call(null, method, source, Expression.Constant(property));
                }
                else if (property.Kind == ResourcePropertyKind.Collection)
                {
                    CollectionResourceType type = (CollectionResourceType) property.ResourceType;
                    MethodInfo info2 = DataServiceProviderMethods.GetSequenceValueMethodInfo.MakeGenericMethod(new Type[] { type.ItemType.InstanceType });
                    expression = Expression.Call(null, info2, source, Expression.Constant(property));
                }
                else
                {
                    expression = Expression.Convert(Expression.Call(null, DataServiceProviderMethods.GetValueMethodInfo, source, Expression.Constant(property)), property.Type);
                }
                if (!nullPropagationRequired)
                {
                    return expression;
                }
                if (!WebUtil.TypeAllowsNull(expression.Type))
                {
                    expression = Expression.Convert(expression, WebUtil.GetTypeAllowingNull(expression.Type));
                }
                return Expression.Condition(Expression.Equal(source, Expression.Constant(null, source.Type)), Expression.Constant(null, expression.Type), expression);
            }

            internal void AddProjectedProperty(ResourceProperty property, ResourceType targetResourceType, ResourceType declaringType)
            {
                this.AddProjectedProperty(property.Name, property, targetResourceType, declaringType);
            }

            internal void AddProjectedProperty(string propertyName, ResourceProperty property, ResourceType targetResourceType, ResourceType declaringType)
            {
                Func<BasicExpandProvider.ProjectedProperty, bool> predicate = null;
                Func<BasicExpandProvider.ProjectedProperty, bool> func2 = null;
                BasicExpandProvider.ProjectedProperty newProjectedProperty = new BasicExpandProvider.ProjectedProperty(propertyName, property, targetResourceType, declaringType);
                BasicExpandProvider.ProjectedProperty property2 = null;
                if (property != null)
                {
                    if (predicate == null)
                    {
                        predicate = p => BasicExpandProvider.ProjectedProperty.Equals(p, newProjectedProperty);
                    }
                    property2 = this.projectedPropertyCandidates.FirstOrDefault<BasicExpandProvider.ProjectedProperty>(predicate);
                    if (property2 != null)
                    {
                        property2.TargetResourceType = targetResourceType;
                    }
                }
                else
                {
                    if (func2 == null)
                    {
                        func2 = p => BasicExpandProvider.ProjectedProperty.Equals(p, newProjectedProperty);
                    }
                    foreach (BasicExpandProvider.ProjectedProperty property3 in this.projectedPropertyCandidates.Where<BasicExpandProvider.ProjectedProperty>(func2))
                    {
                        if (targetResourceType.IsAssignableFrom(property3.TargetResourceType))
                        {
                            property3.TargetResourceType = targetResourceType;
                            property2 = property3;
                            break;
                        }
                    }
                }
                if (property2 == null)
                {
                    this.projectedPropertyCandidates.Add(newProjectedProperty);
                }
            }

            private Expression ApplyOrderTakeOnInnerSegment(Expression expression)
            {
                if (!this.NeedsStandardPaging)
                {
                    return expression;
                }
                ParameterExpression newExpression = Expression.Parameter(expression.ElementType(), "p");
                bool flag = true;
                foreach (OrderingExpression expression3 in this.OrderingInfo.OrderingExpressions)
                {
                    LambdaExpression expression4 = (LambdaExpression) expression3.Expression;
                    LambdaExpression keySelector = Expression.Lambda(ParameterReplacerVisitor.Replace(expression4.Body, expression4.Parameters[0], newExpression), new ParameterExpression[] { newExpression });
                    expression = flag ? (expression3.IsAscending ? expression.EnumerableOrderBy(keySelector) : expression.EnumerableOrderByDescending(keySelector)) : (expression3.IsAscending ? expression.EnumerableThenBy(keySelector) : expression.EnumerableThenByDescending(keySelector));
                    flag = false;
                }
                return expression.EnumerableTake(this.Node.MaxResultsExpected.Value);
            }

            internal void ApplyProjections()
            {
                if (!this.Node.ProjectAllProperties && !(from n in this.Node.Nodes
                    where (n.Property != null) && n.Property.IsOfKind(ResourcePropertyKind.Stream)
                    select n).Any<ProjectionNode>())
                {
                    this.PopulateResourceTypes();
                    foreach (ProjectionNode node in this.Node.Nodes)
                    {
                        if ((node.Property == null) || (node.Property.TypeKind != ResourceTypeKind.EntityType))
                        {
                            this.AddProjectedProperty(node.PropertyName, node.Property, node.TargetResourceType, (node.Property == null) ? null : DataServiceProviderWrapper.GetDeclaringTypeForProperty(node.TargetResourceType, node.Property, null));
                        }
                        else if (node is ExpandedProjectionNode)
                        {
                            this.AddProjectedProperty(node.Property, node.TargetResourceType, DataServiceProviderWrapper.GetDeclaringTypeForProperty(node.TargetResourceType, node.Property, null));
                        }
                    }
                    foreach (ResourceProperty property in this.BaseResourceType.KeyProperties)
                    {
                        this.AddProjectedProperty(property, this.BaseResourceType, DataServiceProviderWrapper.GetDeclaringTypeForProperty(this.BaseResourceType, property, null));
                    }
                    if (((this.OrderingInfo != null) && this.OrderingInfo.IsPaged) && !this.needSkipToken)
                    {
                        foreach (OrderingExpression expression in this.OrderingInfo.OrderingExpressions)
                        {
                            LambdaExpression expression2 = (LambdaExpression) expression.Expression;
                            Dictionary<Expression, BasicExpandProvider.ExpandNode> expandNodeAnnotations = BasicExpandProvider.ExpandNodeAnnotationVisitor.AnnotateExpression(expression2.Body, expression2.Parameters[0], this);
                            BasicExpandProvider.AddPropertyAccessesAsProjectedPropertiesVisitor.AddPropertyAccessesAsProjectedProperties(expression2.Body, expandNodeAnnotations);
                        }
                    }
                    bool flag = false;
                    foreach (ResourceType type in this.resourceTypes)
                    {
                        foreach (ResourceProperty property2 in this.ExpandProvider.Provider.GetETagProperties(this.Node.ResourceSetWrapper.Name, type))
                        {
                            this.AddProjectedProperty(property2, this.BaseResourceType, DataServiceProviderWrapper.GetDeclaringTypeForProperty(type, property2, null));
                        }
                        if (type.HasEntityPropertyMappings)
                        {
                            foreach (EpmSourcePathSegment segment in type.EpmSourceTree.Root.SubProperties)
                            {
                                ResourcePropertyKind stream = ResourcePropertyKind.Stream;
                                ResourceProperty property3 = type.TryResolvePropertyName(segment.PropertyName, stream);
                                this.AddProjectedProperty(segment.PropertyName, property3, this.BaseResourceType, (property3 == null) ? null : DataServiceProviderWrapper.GetDeclaringTypeForProperty(type, property3, null));
                            }
                        }
                        if (type.IsMediaLinkEntry)
                        {
                            flag = true;
                        }
                    }
                    using (List<ResourceType>.Enumerator enumerator7 = this.resourceTypes.GetEnumerator())
                    {
                        Func<BasicExpandProvider.ProjectedProperty, bool> predicate = null;
                        ResourceType resourceType;
                        while (enumerator7.MoveNext())
                        {
                            resourceType = enumerator7.Current;
                            if (predicate == null)
                            {
                                predicate = p => (p.Property == null) && p.TargetResourceType.IsAssignableFrom(resourceType);
                            }
                            foreach (BasicExpandProvider.ProjectedProperty property4 in this.projectedPropertyCandidates.Where<BasicExpandProvider.ProjectedProperty>(predicate).ToList<BasicExpandProvider.ProjectedProperty>())
                            {
                                ResourcePropertyKind exceptKind = ResourcePropertyKind.Stream;
                                ResourceProperty property5 = resourceType.TryResolvePropertyName(property4.Name, exceptKind);
                                if (property5 != null)
                                {
                                    this.AddProjectedProperty(property4.Name, property5, property4.TargetResourceType, DataServiceProviderWrapper.GetDeclaringTypeForProperty(resourceType, property5, null));
                                }
                            }
                        }
                    }
                    if (!flag)
                    {
                        this.projectedProperties = new List<BasicExpandProvider.ProjectedProperty>(this.projectedPropertyCandidates.Count);
                        this.projectedProperties.AddRange(this.projectedPropertyCandidates);
                    }
                }
                foreach (BasicExpandProvider.ExpandNode node3 in this.children)
                {
                    node3.ApplyProjections();
                }
            }

            private Expression ApplySegmentFilter(Expression expression)
            {
                if (!this.isRoot)
                {
                    if (this.singleResult && (this.Node.Filter != null))
                    {
                        LambdaExpression filter = (LambdaExpression) this.Node.Filter;
                        expression = RequestQueryProcessor.ComposePropertyNavigation(expression, filter, this.ExpandProvider.Provider.NullPropagationRequired, true);
                        return expression;
                    }
                    if (this.singleResult)
                    {
                        return expression;
                    }
                    if (this.Node.Filter != null)
                    {
                        expression = expression.EnumerableWhere((LambdaExpression) this.Node.Filter);
                    }
                    if (this.Node.MaxResultsExpected.HasValue && (this.Node.ResourceSetWrapper.PageSize == 0))
                    {
                        expression = expression.EnumerableTake(this.Node.MaxResultsExpected.Value + 1);
                    }
                }
                return expression;
            }

            internal void AssignTypeForExpected(Type enumeratedType, bool singleResult)
            {
                this.enumeratedType = enumeratedType;
                this.singleResult = singleResult;
                bool flag = false;
                bool flag2 = false;
                foreach (BasicExpandProvider.ExpandNode node in this.children)
                {
                    ResourceProperty property = node.Node.Property;
                    if (property == null)
                    {
                        throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BasicExpandProvider_ExpandNotSupportedForOpenProperties);
                    }
                    Type instanceType = property.ResourceType.InstanceType;
                    node.AssignTypeForExpected(instanceType, property.Kind != ResourcePropertyKind.ResourceSetReference);
                    if (node.RequiresWrapper)
                    {
                        flag = true;
                    }
                    if (node.hasFilterWithin)
                    {
                        flag2 = true;
                    }
                }
                bool flag3 = !this.isRoot && ((this.Node.Filter != null) || (this.Node.MaxResultsExpected.HasValue && !this.singleResult));
                this.hasFilterWithin = flag3 || flag2;
                this.requiresExpandedWrapper = ((this.needSkipToken || flag) || flag2) || (!this.ExpandProvider.expanded && (this.children.Count > 0));
                if (this.RequiresExpandedWrapper)
                {
                    foreach (BasicExpandProvider.ExpandNode node2 in this.children)
                    {
                        if (this.projectedProperties != null)
                        {
                            int projectedPropertyIndex = this.GetProjectedPropertyIndex(node2.Node.Property);
                            if (projectedPropertyIndex != -1)
                            {
                                this.projectedProperties.RemoveAt(projectedPropertyIndex);
                            }
                        }
                    }
                }
                if (this.RequiresWrapper)
                {
                    this.SetWrapperElementType();
                }
                else
                {
                    this.elementType = this.enumeratedType;
                }
            }

            private static MemberAssignment BindByName(Type type, string propertyName, Expression source)
            {
                return Expression.Bind(type.GetProperty(propertyName), source);
            }

            internal static BasicExpandProvider.ExpandNode BuildExpansionAndProjectionTree(BasicExpandProvider provider, ExpandedProjectionNode rootProjectionNode)
            {
                BasicExpandProvider.ExpandNode node = new BasicExpandProvider.ExpandNode(rootProjectionNode, provider) {
                    isRoot = true
                };
                node.CreateChildren();
                node.ApplyProjections();
                return node;
            }

            private static LambdaExpression BuildLambdaExpression(Type delegateType, Expression body, IEnumerable<ParameterExpression> parameters)
            {
                return Expression.Lambda(delegateType, body, parameters);
            }

            private Expression BuildProjectionExpression(Expression expression)
            {
                expression = this.ApplySegmentFilter(expression);
                expression = this.ApplyOrderTakeOnInnerSegment(expression);
                if (this.RequiresWrapper)
                {
                    Expression expression3;
                    Type type = this.singleResult ? expression.Type : BaseServiceProvider.GetIEnumerableElement(expression.Type);
                    Expression source = this.singleResult ? expression : Expression.Parameter(type, "p");
                    if (this.RequiresExpandedWrapper)
                    {
                        expression3 = this.BuildProjectionExpressionForExpandedWrapper(source);
                    }
                    else
                    {
                        expression3 = this.BuildProjectionExpressionForProjectedWrapper(source);
                    }
                    if (this.singleResult)
                    {
                        expression = expression3;
                        return expression;
                    }
                    Expression expression4 = expression;
                    Type[] typeArguments = new Type[] { source.Type, expression3.Type };
                    LambdaExpression selector = BuildLambdaExpression(typeof(Func<,>).MakeGenericType(typeArguments), expression3, new ParameterExpression[] { (ParameterExpression) source });
                    expression = expression4.EnumerableSelect(selector);
                }
                return expression;
            }

            private Expression BuildProjectionExpressionForExpandedWrapper(Expression source)
            {
                MemberBinding[] bindings = new MemberBinding[(this.children.Count + 3) + (this.needSkipToken ? 1 : 0)];
                StringBuilder builder = new StringBuilder();
                StringBuilder builder2 = null;
                bindings[0] = BindByName(this.elementType, "ExpandedElement", this.BuildProjectionExpressionForProjectedWrapper(source));
                for (int i = 0; i < this.children.Count; i++)
                {
                    BasicExpandProvider.ExpandNode node = this.children[i];
                    if (i > 0)
                    {
                        builder.Append(',');
                    }
                    Expression expression = source;
                    string str = string.Empty;
                    if (this.BaseResourceType != node.Node.TargetResourceType)
                    {
                        expression = ExpressionUtils.GenerateTypeAsExpression(source, node.Node.TargetResourceType);
                        str = node.Node.TargetResourceType.FullName + "/";
                        builder.Append(str);
                    }
                    Expression ifFalse = node.BuildProjectionExpression(AccessProperty(expression, node.Node.TargetResourceType, node.Node.Property, false));
                    string propertyName = node.Node.PropertyName;
                    builder.Append(propertyName);
                    if (node.Node.Property.IsOfKind(ResourcePropertyKind.ResourceReference))
                    {
                        if (builder2 == null)
                        {
                            builder2 = new StringBuilder();
                        }
                        else
                        {
                            builder2.Append(",");
                        }
                        if (!string.IsNullOrEmpty(str))
                        {
                            builder2.Append(str);
                        }
                        builder2.Append(propertyName);
                    }
                    if (this.singleResult && this.ExpandProvider.Provider.NullPropagationRequired)
                    {
                        ifFalse = Expression.Condition(Expression.Equal(source, Expression.Constant(null, source.Type)), Expression.Constant(null, ifFalse.Type), ifFalse);
                    }
                    if ((expression != source) && this.ExpandProvider.Provider.NullPropagationRequired)
                    {
                        ifFalse = Expression.Condition(Expression.Equal(expression, Expression.Constant(null, expression.Type)), Expression.Constant(null, ifFalse.Type), ifFalse);
                    }
                    bindings[i + 3] = BindByName(this.elementType, "ProjectedProperty" + i.ToString(CultureInfo.InvariantCulture), ifFalse);
                }
                if (this.needSkipToken)
                {
                    Type type = this.elementType.GetGenericArguments().Skip<Type>((this.children.Count + 1)).First<Type>();
                    MemberBinding[] bindingArray2 = new MemberBinding[this.OrderingInfo.OrderingExpressions.Count + 2];
                    bindingArray2[0] = BindByName(type, "ExpandedElement", Expression.Constant(null, typeof(string)));
                    StringBuilder builder3 = new StringBuilder();
                    for (int j = 0; j < this.OrderingInfo.OrderingExpressions.Count; j++)
                    {
                        builder3.Append("SkipTokenProperty" + j.ToString(CultureInfo.InvariantCulture)).Append(",");
                        LambdaExpression expression3 = (LambdaExpression) this.OrderingInfo.OrderingExpressions[j].Expression;
                        Expression expression4 = ParameterReplacerVisitor.Replace(expression3.Body, expression3.Parameters[0], source);
                        MemberInfo property = type.GetProperty("ProjectedProperty" + j.ToString(CultureInfo.InvariantCulture));
                        bindingArray2[j + 2] = Expression.Bind(property, expression4);
                    }
                    bindingArray2[1] = BindByName(type, "Description", Expression.Constant(builder3.Remove(builder3.Length - 1, 1).ToString()));
                    Expression expression5 = Expression.MemberInit(Expression.New(type), bindingArray2);
                    if (builder.Length > 0)
                    {
                        builder.Append(',');
                    }
                    builder.Append("$skiptoken");
                    bindings[this.children.Count + 3] = BindByName(this.elementType, "ProjectedProperty" + this.children.Count.ToString(CultureInfo.InvariantCulture), expression5);
                }
                bindings[1] = BindByName(this.elementType, "Description", Expression.Constant(builder.ToString()));
                string str3 = (builder2 != null) ? builder2.ToString() : string.Empty;
                bindings[2] = BindByName(this.elementType, "ReferenceDescription", Expression.Constant(str3));
                return Expression.MemberInit(Expression.New(this.elementType), bindings);
            }

            private Expression BuildProjectionExpressionForProjectedWrapper(Expression source)
            {
                if (!this.RequiresProjectedWrapper)
                {
                    return source;
                }
                Type projectedWrapperType = ProjectedWrapper.GetProjectedWrapperType(this.projectedProperties.Count);
                Expression ifFalse = null;
                foreach (ResourceType type2 in this.resourceTypes)
                {
                    StringBuilder builder;
                    Expression[] expressionArray;
                    this.PrepareProjectionBindingForResourceType(type2, source, out builder, out expressionArray);
                    if (!this.ExpandProvider.Provider.NullPropagationRequired)
                    {
                        expressionArray[0] = Expression.Condition(Expression.Equal(source, Expression.Constant(null, source.Type)), Expression.Constant(string.Empty, typeof(string)), Expression.Constant(type2.FullName, typeof(string)));
                    }
                    else
                    {
                        expressionArray[0] = Expression.Constant(type2.FullName, typeof(string));
                    }
                    expressionArray[1] = Expression.Constant(builder.ToString());
                    Expression ifTrue = Expression.MemberInit(Expression.New(projectedWrapperType), ProjectedWrapper.Bind(expressionArray, projectedWrapperType));
                    if (ifFalse == null)
                    {
                        ifFalse = ifTrue;
                    }
                    else
                    {
                        Expression test = this.ExpandProvider.Provider.IsV1Provider ? ((Expression) Expression.TypeIs(source, type2.InstanceType)) : ((Expression) Expression.Call(null, DataServiceProviderMethods.TypeIsMethodInfo, source, Expression.Constant(type2)));
                        ifFalse = Expression.Condition(test, ifTrue, ifFalse);
                    }
                }
                if (this.ExpandProvider.Provider.NullPropagationRequired)
                {
                    ifFalse = Expression.Condition(Expression.Equal(source, Expression.Constant(null, source.Type)), Expression.Constant(null, ifFalse.Type), ifFalse);
                }
                return ifFalse;
            }

            internal Expression BuildProjectionQuery(Expression queryExpression)
            {
                MethodCallExpression expression = this.BuildProjectionExpression(queryExpression) as MethodCallExpression;
                if (((expression != null) && !this.singleResult) && this.RequiresWrapper)
                {
                    LambdaExpression selector = (expression.Arguments[1].NodeType == ExpressionType.Quote) ? ((LambdaExpression) ((UnaryExpression) expression.Arguments[1]).Operand) : ((LambdaExpression) expression.Arguments[1]);
                    return queryExpression.QueryableSelect(selector);
                }
                return queryExpression;
            }

            internal void CreateChildren()
            {
                if (!this.isRoot && (!this.Node.ProjectAllProperties || (this.OrderingInfo != null)))
                {
                    this.isV1Compatible = false;
                }
                foreach (ProjectionNode node in this.Node.Nodes)
                {
                    ExpandedProjectionNode node2 = node as ExpandedProjectionNode;
                    if (node2 != null)
                    {
                        BasicExpandProvider.ExpandNode item = new BasicExpandProvider.ExpandNode(node2, this.ExpandProvider);
                        this.children.Add(item);
                        item.CreateChildren();
                        this.isV1Compatible &= item.isV1Compatible;
                    }
                }
            }

            internal BasicExpandProvider.ExpandNode FindChild(string name)
            {
                foreach (BasicExpandProvider.ExpandNode node in this.children)
                {
                    if (string.Equals(node.Node.PropertyName, name, StringComparison.Ordinal))
                    {
                        return node;
                    }
                }
                return null;
            }

            internal int GetProjectedPropertyIndex(ResourceProperty property)
            {
                return this.projectedProperties.FindIndex(projectedProperty => projectedProperty.Property == property);
            }

            private Type GetProjectedWrapperType()
            {
                return ProjectedWrapper.GetProjectedWrapperType(this.projectedProperties.Count);
            }

            private void PopulateResourceTypes()
            {
                this.resourceTypes = new List<ResourceType>();
                this.resourceTypes.Add(this.BaseResourceType);
                if (this.ExpandProvider.Provider.HasDerivedTypes(this.BaseResourceType))
                {
                    this.resourceTypes.AddRange(this.ExpandProvider.Provider.GetDerivedTypes(this.BaseResourceType));
                }
                this.resourceTypes.Sort(delegate (ResourceType x, ResourceType y) {
                    if (x == y)
                    {
                        return 0;
                    }
                    if (!x.IsAssignableFrom(y))
                    {
                        return 1;
                    }
                    return -1;
                });
            }

            private void PrepareProjectionBindingForResourceType(ResourceType resourceType, Expression source, out StringBuilder propertyNameList, out Expression[] bindingExpressions)
            {
                propertyNameList = new StringBuilder();
                bindingExpressions = new Expression[this.projectedProperties.Count + 2];
                for (int i = 0; i < this.projectedProperties.Count; i++)
                {
                    BasicExpandProvider.ProjectedProperty property = this.projectedProperties[i];
                    Expression expression = null;
                    if (property.TargetResourceType.IsAssignableFrom(resourceType))
                    {
                        ResourcePropertyKind stream = ResourcePropertyKind.Stream;
                        ResourceProperty property2 = resourceType.TryResolvePropertyName(property.Name, stream);
                        Expression instance = source;
                        if (property.TargetResourceType == this.BaseResourceType)
                        {
                            instance = Expression.TypeAs(source, resourceType.InstanceType);
                        }
                        if (property.Property != null)
                        {
                            if (property2 == property.Property)
                            {
                                if (property.TargetResourceType != this.BaseResourceType)
                                {
                                    instance = ExpressionUtils.GenerateTypeAsExpression(instance, property.TargetResourceType);
                                }
                                expression = AccessProperty(instance, resourceType, property2, false);
                                if (!WebUtil.TypeAllowsNull(expression.Type))
                                {
                                    expression = Expression.Convert(expression, WebUtil.GetTypeAllowingNull(property2.Type));
                                }
                            }
                        }
                        else if ((property2 == null) && resourceType.IsOpenType)
                        {
                            expression = AccessOpenProperty(instance, property.Name);
                        }
                    }
                    if (i > 0)
                    {
                        propertyNameList.Append(',');
                    }
                    if (expression == null)
                    {
                        if (property.Property != null)
                        {
                            expression = Expression.Constant(null, WebUtil.GetTypeAllowingNull(property.Property.Type));
                        }
                        else
                        {
                            expression = Expression.Constant(null, typeof(object));
                        }
                    }
                    else
                    {
                        propertyNameList.Append(property.Name);
                    }
                    if (this.ExpandProvider.castToObject && (expression.Type != typeof(object)))
                    {
                        expression = Expression.Convert(expression, typeof(object));
                    }
                    bindingExpressions[i + 2] = expression;
                }
            }

            private void SetWrapperElementType()
            {
                if (this.RequiresExpandedWrapper)
                {
                    Type[] wrapperParameters = new Type[(1 + this.children.Count) + (this.needSkipToken ? 1 : 0)];
                    int num = 0;
                    wrapperParameters[num++] = this.RequiresProjectedWrapper ? this.GetProjectedWrapperType() : this.enumeratedType;
                    for (int i = 0; i < this.children.Count; i++)
                    {
                        wrapperParameters[num++] = this.children[i].ProjectedType;
                    }
                    if (this.needSkipToken)
                    {
                        Type[] typeArray2 = new Type[this.OrderingInfo.OrderingExpressions.Count + 1];
                        typeArray2[0] = typeof(string);
                        for (int j = 0; j < this.OrderingInfo.OrderingExpressions.Count; j++)
                        {
                            typeArray2[j + 1] = ((LambdaExpression) this.OrderingInfo.OrderingExpressions[j].Expression).Body.Type;
                        }
                        wrapperParameters[num++] = WebUtil.GetWrapperType(typeArray2, new Func<object, string>(System.Data.Services.Strings.BasicExpandProvider_SDP_UnsupportedOrderingExpressionBreadth));
                    }
                    this.elementType = WebUtil.GetWrapperType(wrapperParameters, new Func<object, string>(System.Data.Services.Strings.BasicExpandProvider_UnsupportedExpandBreadth));
                }
                else
                {
                    this.elementType = this.GetProjectedWrapperType();
                }
            }

            internal ResourceType BaseResourceType
            {
                get
                {
                    return this.Node.ResourceType;
                }
            }

            internal BasicExpandProvider ExpandProvider
            {
                get
                {
                    return this.expandProvider;
                }
            }

            internal bool IsV1Compatible
            {
                get
                {
                    return this.isV1Compatible;
                }
            }

            private bool NeedsStandardPaging
            {
                get
                {
                    return (((!this.singleResult && !this.isRoot) && (this.Node.OrderingInfo != null)) && this.Node.OrderingInfo.IsPaged);
                }
            }

            internal System.Data.Services.Providers.OrderingInfo OrderingInfo
            {
                get
                {
                    return this.orderingInfo;
                }
            }

            internal Type ProjectedType
            {
                get
                {
                    if (!this.singleResult)
                    {
                        return typeof(IEnumerable<>).MakeGenericType(new Type[] { this.elementType });
                    }
                    return this.elementType;
                }
            }

            internal bool RequiresExpandedWrapper
            {
                get
                {
                    return this.requiresExpandedWrapper;
                }
            }

            private bool RequiresProjectedWrapper
            {
                get
                {
                    return (this.projectedProperties != null);
                }
            }

            private bool RequiresWrapper
            {
                get
                {
                    if (!this.RequiresExpandedWrapper)
                    {
                        return this.RequiresProjectedWrapper;
                    }
                    return true;
                }
            }
        }

        internal class ExpandNodeAnnotationVisitor : PropertyAccessVisitor
        {
            private readonly Dictionary<Expression, BasicExpandProvider.ExpandNode> expandNodeAnnotations;
            private readonly ParameterExpression parameter;
            private readonly BasicExpandProvider.ExpandNode parameterExpandNode;

            private ExpandNodeAnnotationVisitor(ParameterExpression parameter, BasicExpandProvider.ExpandNode parameterExpandNode)
            {
                this.parameter = parameter;
                this.parameterExpandNode = parameterExpandNode;
                this.expandNodeAnnotations = new Dictionary<Expression, BasicExpandProvider.ExpandNode>(ReferenceEqualityComparer<Expression>.Instance);
            }

            internal static Dictionary<Expression, BasicExpandProvider.ExpandNode> AnnotateExpression(Expression expression, ParameterExpression parameter, BasicExpandProvider.ExpandNode parameterExpandNode)
            {
                BasicExpandProvider.ExpandNodeAnnotationVisitor visitor = new BasicExpandProvider.ExpandNodeAnnotationVisitor(parameter, parameterExpandNode);
                visitor.Visit(expression);
                return visitor.expandNodeAnnotations;
            }

            private BasicExpandProvider.ExpandNode GetExpandNodeAnnotation(Expression expression)
            {
                BasicExpandProvider.ExpandNode node;
                if (this.expandNodeAnnotations.TryGetValue(expression, out node))
                {
                    return node;
                }
                return null;
            }

            protected override bool ProcessPropertyAccess(string propertyName, ref Expression operandExpression, ref Expression accessExpression)
            {
                this.Visit(operandExpression);
                BasicExpandProvider.ExpandNode expandNodeAnnotation = this.GetExpandNodeAnnotation(operandExpression);
                if (expandNodeAnnotation != null)
                {
                    BasicExpandProvider.ExpandNode annotation = expandNodeAnnotation.FindChild(propertyName);
                    if (annotation != null)
                    {
                        this.SetExpandNodeAnnotation(accessExpression, annotation);
                    }
                }
                return true;
            }

            private void SetExpandNodeAnnotation(Expression expression, BasicExpandProvider.ExpandNode annotation)
            {
                this.expandNodeAnnotations[expression] = annotation;
            }

            internal override Expression VisitConditional(ConditionalExpression c)
            {
                base.VisitConditional(c);
                BasicExpandProvider.ExpandNode expandNodeAnnotation = this.GetExpandNodeAnnotation(c.IfTrue);
                BasicExpandProvider.ExpandNode node2 = this.GetExpandNodeAnnotation(c.IfFalse);
                if (((expandNodeAnnotation == null) || (node2 == null)) && ((expandNodeAnnotation != null) || (node2 != null)))
                {
                    this.SetExpandNodeAnnotation(c, (expandNodeAnnotation != null) ? expandNodeAnnotation : node2);
                }
                return c;
            }

            internal override Expression VisitParameter(ParameterExpression p)
            {
                if (p == this.parameter)
                {
                    this.SetExpandNodeAnnotation(p, this.parameterExpandNode);
                }
                return p;
            }

            internal override Expression VisitUnary(UnaryExpression u)
            {
                base.VisitUnary(u);
                switch (u.NodeType)
                {
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                    case ExpressionType.Quote:
                    case ExpressionType.TypeAs:
                    {
                        BasicExpandProvider.ExpandNode expandNodeAnnotation = this.GetExpandNodeAnnotation(u.Operand);
                        if (expandNodeAnnotation != null)
                        {
                            this.SetExpandNodeAnnotation(u, expandNodeAnnotation);
                        }
                        return u;
                    }
                }
                return u;
            }
        }

        [DebuggerDisplay("ProjectedProperty {Name}")]
        internal class ProjectedProperty
        {
            private readonly ResourceType declaringResourceType;
            private readonly string name;
            private readonly ResourceProperty property;
            private ResourceType targetResourceType;

            public ProjectedProperty(string name, ResourceProperty property, ResourceType targetResourceType, ResourceType declaringResourceType)
            {
                this.name = name;
                this.property = property;
                this.targetResourceType = targetResourceType;
                this.declaringResourceType = declaringResourceType;
            }

            public static bool Equals(BasicExpandProvider.ProjectedProperty x, BasicExpandProvider.ProjectedProperty y)
            {
                WebUtil.CheckArgumentNull<BasicExpandProvider.ProjectedProperty>(x, "x");
                WebUtil.CheckArgumentNull<BasicExpandProvider.ProjectedProperty>(y, "y");
                return ((string.Equals(x.name, y.name, StringComparison.Ordinal) && (x.property == y.property)) && (x.declaringResourceType == y.declaringResourceType));
            }

            public string Name
            {
                get
                {
                    return this.name;
                }
            }

            public ResourceProperty Property
            {
                get
                {
                    return this.property;
                }
            }

            public ResourceType TargetResourceType
            {
                get
                {
                    return this.targetResourceType;
                }
                set
                {
                    this.targetResourceType = value;
                }
            }
        }
    }
}

