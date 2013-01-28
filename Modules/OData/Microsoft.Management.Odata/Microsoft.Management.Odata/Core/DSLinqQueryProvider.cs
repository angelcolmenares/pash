using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.Schema;
using Microsoft.Management.Odata.Tracing;
using System;
using System.Collections.Generic;
using System.Data.Services.Providers;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CSharp.RuntimeBinder;
using System.Runtime.CompilerServices;

namespace Microsoft.Management.Odata.Core
{
	internal class DSLinqQueryProvider : IQueryProvider
	{
		private Microsoft.Management.Odata.Schema.Schema schema;

		private DataServiceQueryProvider.ResultSetCollection resultSets;

		private DataServiceQueryProvider.ResultSet initialResourceRoot;

		private IQueryable<DSResource> initialQueryable;

		private UserContext userContext;

		private string membershipId;

		private DSLinqQueryProvider(Microsoft.Management.Odata.Schema.Schema schema, ResourceType root, UserContext userContext, string membershipId, DataServiceQueryProvider.ResultSetCollection resultSets)
		{
			this.schema = schema;
			this.initialResourceRoot = new DataServiceQueryProvider.ResultSet(root);
			this.initialQueryable = this.initialResourceRoot.AsQueryable<DSResource>();
			this.resultSets = resultSets;
			this.userContext = userContext;
			this.membershipId = membershipId;
		}

		public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
		{
			return new DSLinqQuery<TElement>(this, expression);
		}

		public IQueryable CreateQuery(Expression expression)
		{
			IQueryable queryables;
			using (OperationTracer operationTracer = new OperationTracer("CreateQuery"))
			{
				Type enumerableElementType = TypeSystem.GetIEnumerableElementType(expression.Type);
				Type[] typeArray = new Type[1];
				typeArray[0] = enumerableElementType;
				Type type = typeof(DSLinqQuery<>).MakeGenericType(typeArray);
				object[] objArray = new object[2];
				objArray[0] = this;
				objArray[1] = expression;
				object[] objArray1 = objArray;
				Type[] typeArray1 = new Type[2];
				typeArray1[0] = typeof(DSLinqQueryProvider);
				typeArray1[1] = typeof(Expression);
				ConstructorInfo constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, typeArray1, null);
				queryables = (IQueryable)constructor.Invoke(objArray1);
			}
			return queryables;
		}

		internal static IQueryable CreateQuery(Microsoft.Management.Odata.Schema.Schema schema, ResourceType type, UserContext userContext, string membershipId, DataServiceQueryProvider.ResultSetCollection resultSets)
		{
			DSLinqQueryProvider dSLinqQueryProvider = new DSLinqQueryProvider(schema, type, userContext, membershipId, resultSets);
			return dSLinqQueryProvider.CreateRootQuery();
		}

		internal IQueryable CreateRootQuery()
		{
			return this.CreateQuery(Expression.Constant(this.initialQueryable));
		}

		internal object EvaluateByLinqToObjects(Expression expression)
		{
			IQueryable<DSResource> dSResources = this.initialResourceRoot.AsQueryable<DSResource>();
			DSMethodTranslatingVisitor dSMethodTranslatingVisitor = new DSMethodTranslatingVisitor(this.resultSets);
			expression = dSMethodTranslatingVisitor.Visit(expression);
			object obj = null;
			var genericTypeDef = expression.Type.GetGenericTypeDefinition();

			if (!expression.Type.IsGenericType || !(genericTypeDef == typeof(IQueryable<>)) && !(genericTypeDef == typeof(EnumerableQuery<>)) && !(genericTypeDef.Name == "QueryableEnumerable`1"))
			{
				obj = dSResources.Provider.Execute(expression);
			}
			else
			{
				obj = dSResources.Provider.CreateQuery(expression);
			}
			return obj;
		}

		public TResult Execute<TResult>(Expression expression)
		{
			bool name;
			if (typeof(TResult).Name == "IEnumerable`1")
			{
				name = true;
			}
			else
			{
				name = typeof(TResult).Name == "IEnumerable";
			}
			bool flag = name;
			return (TResult)this.ProcessExpression(expression, flag);
		}

		public object Execute(Expression expression)
		{
			throw new NotImplementedException();
		}

		internal DataServiceQueryProvider.ResultSet GetAssociatedInstances(IQueryable<DSResource> source, ResourceProperty property)
		{
			EntityMetadata entityMetadatum = null;
			DataServiceQueryProvider.ResultSet resultSet;
			using (OperationTracer operationTracer = new OperationTracer("ProcessExpression"))
			{
				if (this.schema.EntityMetadataDictionary.TryGetValue(property.ResourceType.FullName, out entityMetadatum))
				{
					DataServiceQueryProvider.ResultSet resultSet1 = new DataServiceQueryProvider.ResultSet(property.ResourceType);
					IEnumerable<DSResource> uniqueKeys = this.GetUniqueKeys(source, property);
					foreach (DSResource uniqueKey in uniqueKeys)
					{
						if (!uniqueKey.ContainsNonKeyProperties)
						{
							ICommand command = DataServiceController.Current.GetCommand(CommandType.Read, this.userContext, property.ResourceType, entityMetadatum, this.membershipId);
							using (command)
							{
								UriParametersHelper.AddParametersToCommand(command, DataServiceController.Current.GetCurrentResourceUri());
								foreach (ResourceProperty keyProperty in property.ResourceType.KeyProperties)
								{
									if (command.AddFieldParameter(keyProperty.Name, uniqueKey.GetValue(keyProperty.Name, null)))
									{
										continue;
									}
									object[] name = new object[2];
									name[0] = property.ResourceType.Name;
									name[1] = keyProperty.Name;
									string str = string.Format(CultureInfo.CurrentCulture, Resources.KeyParameterFailed, name);
									throw new NotImplementedException(str);
								}
								try
								{
									DataServiceController.Current.QuotaSystem.CheckCmdletExecutionQuota(this.userContext);
									IEnumerator<DSResource> enumerator = command.InvokeAsync(new List<DSResource>().AsQueryable<DSResource>().Expression, true);
									while (enumerator.MoveNext())
									{
										resultSet1.Add(enumerator.Current);
									}
								}
								catch (Exception exception1)
								{
									Exception exception = exception1;
									Tracer tracer = new Tracer();
									tracer.ExceptionMessage(exception.Message);
									throw;
								}
							}
						}
						else
						{
							resultSet1.Add(uniqueKey);
						}
					}
					resultSet = resultSet1;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = property.Name;
					throw new UnauthorizedAccessException(ExceptionHelpers.GetExceptionMessage(Resources.NoAccessToNavProperty, objArray));
				}
			}
			return resultSet;
		}

		internal EntityMetadata GetEntityMetadata(ResourceType type)
		{
			return this.schema.EntityMetadataDictionary[type.FullName];
		}

		internal IQueryable<DSResource> GetInitialQueryable()
		{
			return this.initialQueryable;
		}

		internal ResourceType GetInitialResourceType()
		{
			return this.initialResourceRoot.ResourceType;
		}

		internal static List<ResourceProperty> GetRefPropertiesUsed(Expression expression)
		{
			PropertyReferenceFinder propertyReferenceFinder = new PropertyReferenceFinder();
			propertyReferenceFinder.Visit(expression);
			return propertyReferenceFinder.Properties;
		}

		private IEnumerable<DSResource> GetUniqueKeys(IQueryable<DSResource> source, ResourceProperty property)
		{
			DSResource.KeyEqualityComparer keyEqualityComparer = new DSResource.KeyEqualityComparer();
			List<DSResource> dSResources = new List<DSResource>();
			foreach (DSResource dSResource in source)
			{
				if (property.Kind != ResourcePropertyKind.ResourceReference)
				{
					if (property.Kind != ResourcePropertyKind.ResourceSetReference)
					{
						throw new ArgumentException("the property is not a reference property", property.Name);
					}
					else
					{
						object value = dSResource.GetValue(property.Name, this.resultSets);
						IEnumerable<DSResource> dSResources1 = value as IEnumerable<DSResource>;
						if (dSResources1 == null)
						{
							object[] name = new object[1];
							name[0] = property.Name;
							throw new UnauthorizedAccessException(ExceptionHelpers.GetExceptionMessage(Resources.NoAccessToNavProperty, name));
						}
						else
						{
							dSResources.AddRange(dSResources1);
						}
					}
				}
				else
				{
					DSResource value1 = dSResource.GetValue(property.Name, this.resultSets) as DSResource;
					if (value1 == null)
					{
						continue;
					}
					dSResources.Add(value1);
				}
			}
			return dSResources.Distinct<DSResource>(keyEqualityComparer);
		}

		private void InvokeCommandWithQuota(ICommand command, DataServiceQueryProvider.ResultSet resultSet)
		{
			try
			{
				if (command.GetType() != typeof(ReferenceInstanceBuilderCommand))
				{
					DataServiceController.Current.QuotaSystem.CheckCmdletExecutionQuota(this.userContext);
				}
				IEnumerator<DSResource> enumerator = command.InvokeAsync(new List<DSResource>().AsQueryable<DSResource>().Expression, true);
				while (enumerator.MoveNext())
				{
					resultSet.Add(enumerator.Current);
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				Tracer tracer = new Tracer();
				tracer.ExceptionMessage(exception.Message);
				throw;
			}
		}

		internal LambdaExpression InvokeFilteredGet(ICommand command, ResourceType originType, LambdaExpression filter, out DataServiceQueryProvider.ResultSet resultSet)
		{
			LambdaExpression lambdaExpression;
			using (OperationTracer operationTracer = new OperationTracer("ProcessExpression"))
			{
				LambdaExpression lambdaExpression1 = null;
				//this.schema.EntityMetadataDictionary[originType.FullName];
				resultSet = new DataServiceQueryProvider.ResultSet(originType);
				UriParametersHelper.AddParametersToCommand(command, DataServiceController.Current.GetCurrentResourceUri());
				if (filter != null)
				{
					CommandArgumentVisitor commandArgumentVisitor = new CommandArgumentVisitor(command);
					lambdaExpression1 = commandArgumentVisitor.VisitAndConvert<LambdaExpression>(filter, "InvokeFilteredGet");
				}
				this.InvokeCommandWithQuota(command, resultSet);
				lambdaExpression = lambdaExpression1;
			}
			return lambdaExpression;
		}

		public bool IsFilterOverResourceRoot(Expression expression)
		{
			return ExpressionHelper.IsNestedWhereClause(expression, this.initialQueryable);
		}

		internal Expression ProcessCmdletExpression(Expression rootExpression, Expression cmdletExpression, ExpressionCategory category, bool isEnumerable)
		{
			if (TraceHelper.IsEnabled(5))
			{
				object[] objArray = new object[6];
				objArray[0] = "ProcessCmdletExpression:\nRootExpression: ";
				objArray[1] = rootExpression;
				objArray[2] = "\nCmdletExpression: ";
				objArray[3] = cmdletExpression;
				objArray[4] = "\nExpression category:";
				objArray[5] = category;
				TraceHelper.Current.DebugMessage(string.Concat(objArray));
			}
			ExpressionCategory expressionCategory = category;
			switch (expressionCategory)
			{
				case ExpressionCategory.ResourceRoot:
				{
					return this.ProcessResourceRoot(rootExpression, cmdletExpression as ConstantExpression);
				}
				case ExpressionCategory.WhereOfResourceRoot:
				{
					return this.ProcessWhereWithResourceRoot(rootExpression, cmdletExpression as MethodCallExpression, false);
				}
				case ExpressionCategory.WhereInsideNavPropertyWithGetRefCmdlet:
				{
					return this.ProcessWhereWithResourceRoot(rootExpression, cmdletExpression as MethodCallExpression, true);
				}
				case ExpressionCategory.NestedPropertyComparisons:
				{
					return this.ProcessNestedWhereOfResourceRoot(rootExpression, cmdletExpression as MethodCallExpression, false);
				}
				case ExpressionCategory.NestedPropertyComparisonsInsideNavPropertyWithGetRefCmdlet:
				{
					return this.ProcessNestedWhereOfResourceRoot(rootExpression, cmdletExpression as MethodCallExpression, true);
				}
				case ExpressionCategory.WhereOfResultSet:
				{
					return this.ProcessSelectExpansion(rootExpression, cmdletExpression as MethodCallExpression);
				}
				case ExpressionCategory.SelectNavProperty:
				{
					return this.ProcessSelectExpansion(rootExpression, cmdletExpression as MethodCallExpression);
				}
				case ExpressionCategory.SelectExpansion:
				{
					return this.ProcessSelectExpansion(rootExpression, cmdletExpression as MethodCallExpression);
				}
			}
			throw new NotImplementedException();
		}

		internal object ProcessExpression(Expression expression, bool isEnumerable)
		{
			string empty;
			Tracer tracer = new Tracer();
			Tracer tracer1 = tracer;
			string str = "LinqQueryProvider";
			string str1 = "ProcessExpression";
			string str2 = isEnumerable.ToString();
			if (expression == null)
			{
				empty = string.Empty;
			}
			else
			{
				empty = expression.ToString();
			}
			tracer1.MethodCall2(str, str1, str2, empty);
			if (expression != null)
			{
				expression = WhereExpressionOptimizer.GetOptimizedExpression(expression, this.initialQueryable, this.initialResourceRoot.ResourceType);
				Expression expression1 = null;
				InnermostCmdletFinder innermostCmdletFinder = new InnermostCmdletFinder();
				ExpressionCategory expressionCategory = ExpressionCategory.Unhandled;
				while (innermostCmdletFinder.GetInnermostCmdletExpression(expression, this.initialQueryable, this.GetEntityMetadata(this.initialResourceRoot.ResourceType), out expression1, out expressionCategory))
				{
					expression = this.ProcessCmdletExpression(expression, expression1, expressionCategory, isEnumerable);
				}
				return this.EvaluateByLinqToObjects(expression);
			}
			else
			{
				throw new ArgumentNullException();
			}
		}

		internal Expression ProcessNestedWhereOfResourceRoot(Expression rootExpression, MethodCallExpression whereExpression, bool insideNavPropertyWithGetReferenceCmdlet = false)
		{
			ICommand referenceInstanceBuilderCommand;
			ResourceType initialResourceType = this.GetInitialResourceType();
			EntityMetadata item = this.schema.EntityMetadataDictionary[initialResourceType.FullName];
			DataServiceQueryProvider.ResultSet resultSet = new DataServiceQueryProvider.ResultSet(initialResourceType);
			Expression expression = null;
			if (insideNavPropertyWithGetReferenceCmdlet)
			{
				referenceInstanceBuilderCommand = new ReferenceInstanceBuilderCommand(initialResourceType, item);
			}
			else
			{
				referenceInstanceBuilderCommand = DataServiceController.Current.GetCommand(CommandType.Read, this.userContext, initialResourceType, item, this.membershipId);
			}
			using (referenceInstanceBuilderCommand)
			{
				UriParametersHelper.AddParametersToCommand(referenceInstanceBuilderCommand, DataServiceController.Current.GetCurrentResourceUri());
				this.TryAddingAllExpressions(rootExpression, whereExpression, referenceInstanceBuilderCommand, resultSet, out expression);
				this.InvokeCommandWithQuota(referenceInstanceBuilderCommand, resultSet);
			}
			IQueryable<DSResource> dSResources = resultSet.AsQueryable<DSResource>();
			ExpressionNodeReplacer expressionNodeReplacer = new ExpressionNodeReplacer(expression, Expression.Constant(dSResources));
			Expression expression1 = expressionNodeReplacer.Visit(rootExpression);
			return expression1;
		}

		internal Expression ProcessResourceRoot(Expression rootExpression, ConstantExpression whereExpression)
		{
			ResourceType resourceType = this.initialResourceRoot.ResourceType;
			DataServiceQueryProvider.ResultSet resultSet = null;
			EntityMetadata item = this.schema.EntityMetadataDictionary[resourceType.FullName];
			ICommand command = DataServiceController.Current.GetCommand(CommandType.Read, this.userContext, resourceType, item, this.membershipId);
			using (command)
			{
				this.InvokeFilteredGet(command, resourceType, null, out resultSet);
			}
			IQueryable<DSResource> dSResources = resultSet.AsQueryable<DSResource>();
			ExpressionNodeReplacer expressionNodeReplacer = new ExpressionNodeReplacer(whereExpression, Expression.Constant(dSResources));
			Expression expression = expressionNodeReplacer.Visit(rootExpression);
			return expression;
		}

		internal Expression ProcessWhereWithResourceRoot(Expression rootExpression, MethodCallExpression whereExpression, bool insideNavPropertyWithGetReferenceCmdlet = false)
		{
			ICommand command;
			LambdaExpression operand = (LambdaExpression)((UnaryExpression)whereExpression.Arguments[1]).Operand;
			operand = (LambdaExpression)PartialEvaluator.Eval(operand);
			ResourceType initialResourceType = this.GetInitialResourceType();
			EntityMetadata entityMetadatum = this.schema.EntityMetadataDictionary[initialResourceType.FullName];
			if (!insideNavPropertyWithGetReferenceCmdlet)
			{
				command = DataServiceController.Current.GetCommand(CommandType.Read, this.userContext, initialResourceType, entityMetadatum, this.membershipId);
			}
			else
			{
				command = new ReferenceInstanceBuilderCommand(initialResourceType, entityMetadatum);
			}
			DataServiceQueryProvider.ResultSet resultSet = null;
			using (command)
			{
				operand = this.InvokeFilteredGet(command, initialResourceType, operand, out resultSet);
			}
			DSMethodTranslatingVisitor dSMethodTranslatingVisitor = new DSMethodTranslatingVisitor(this.resultSets);
			operand = dSMethodTranslatingVisitor.VisitAndConvert<LambdaExpression>(operand, "ProcessWhereWithResourceRoot");
			Func<DSResource, bool> func = (Func<DSResource, bool>)operand.Compile();
			IQueryable<DSResource> dSResources = resultSet.Where<DSResource>((DSResource item) => func(item)).AsQueryable<DSResource>();
			ExpressionNodeReplacer expressionNodeReplacer = new ExpressionNodeReplacer(whereExpression, Expression.Constant(dSResources));
			Expression expression = expressionNodeReplacer.Visit(rootExpression);
			return expression;
		}

		internal bool TryAddingAllExpressions(Expression rootExpression, Expression baseExpression, ICommand command, DataServiceQueryProvider.ResultSet resultSet, out Expression nodeToReplace)
		{
			if (!ExpressionHelper.IsResourceRoot(baseExpression, this.initialQueryable))
			{
				MethodCallExpression methodCallExpression = baseExpression as MethodCallExpression;
				bool flag = this.TryAddingAllExpressions(rootExpression, methodCallExpression.Arguments[0], command, resultSet, out nodeToReplace);
				if (!flag)
				{
					return false;
				}
				else
				{
					LambdaExpression operand = (LambdaExpression)((UnaryExpression)methodCallExpression.Arguments[1]).Operand;
					operand = (LambdaExpression)PartialEvaluator.Eval(operand);
					CommandArgumentVisitor commandArgumentVisitor = new CommandArgumentVisitor(command);
					operand = commandArgumentVisitor.VisitAndConvert<LambdaExpression>(operand, "TryAddingAllExpressions");
					if (!ExpressionHelper.IsConstantTrue(operand.Body))
					{
						return false;
					}
					else
					{
						nodeToReplace = methodCallExpression;
						return true;
					}
				}
			}
			else
			{
				nodeToReplace = baseExpression;
				return true;
			}
		}

		internal Expression ProcessSelectExpansion(Expression rootExpression, MethodCallExpression selectExpression)
		{
			LambdaExpression operand = (LambdaExpression) ((UnaryExpression) selectExpression.Arguments[1]).Operand;
			operand = (LambdaExpression) PartialEvaluator.Eval(operand);
			List<ResourceProperty> refPropertiesUsed = GetRefPropertiesUsed(operand);
			IQueryable<DSResource> source = this.EvaluateByLinqToObjects(selectExpression.Arguments[0]) as IQueryable<DSResource>;
			foreach (ResourceProperty property in refPropertiesUsed)
			{
				ResourceType resourceType = property.ResourceType;
				DataServiceQueryProvider.ResultSet associatedInstances = this.GetAssociatedInstances(source, property);
				DataServiceQueryProvider.ResultSet set2 = null;
				if (this.resultSets.TryGetValue(resourceType.Name, out set2))
				{
					set2.Concat<DSResource>(associatedInstances);
				}
				else
				{
					this.resultSets[resourceType.Name] = associatedInstances;
				}
			}
			MethodCallExpression expression = new DSMethodTranslatingVisitor(this.resultSets).VisitAndConvert<MethodCallExpression>(selectExpression, "ProcessSelectExpansion");
			object obj2 = source.Provider.CreateQuery(expression);
			var ___Site4 = CallSite<Func<CallSite, Type, MethodCallExpression, object, ExpressionNodeReplacer>>.Create(Microsoft.CSharp.RuntimeBinder.Binder.InvokeConstructor(CSharpBinderFlags.None, typeof(DSLinqQueryProvider), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.IsStaticType | CSharpArgumentInfoFlags.UseCompileTimeType, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
			return ___Site4.Target(___Site4, typeof(ExpressionNodeReplacer), selectExpression, Expression.Constant((dynamic) obj2)).Visit(rootExpression);
		}


	}
}