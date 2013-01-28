using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Services.Providers;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.Management.Odata.Core
{
	internal class MovableWhereFinder : ExpressionVisitor
	{
		private IQueryable<DSResource> resourceRoot;

		private ResourceType baseResourceType;

		public MethodCallExpression MovableWhereExpression
		{
			get;
			private set;
		}

		public MovableWhereFinder(Expression tree, IQueryable<DSResource> resourceRoot, ResourceType baseResourceType)
		{
			this.resourceRoot = resourceRoot;
			this.baseResourceType = baseResourceType;
			this.MovableWhereExpression = null;
			this.Visit(tree);
		}

		private bool DoesPredicateContainNavOrDerivedProperties(Expression expression, ResourceType type)
		{
			bool flag;
			FilterPropertyFinder filterPropertyFinder = new FilterPropertyFinder(expression);
			if (filterPropertyFinder.IsCompleteExpressionParsed)
			{
				List<KeyValuePair<ResourceProperty, object>> filterProperties = filterPropertyFinder.FilterProperties;
				bool flag1 = filterProperties.Any<KeyValuePair<ResourceProperty, object>>((KeyValuePair<ResourceProperty, object> it) => {
					if ((it.Key.Kind & ResourcePropertyKind.ResourceSetReference) == ResourcePropertyKind.ResourceSetReference)
					{
						return true;
					}
					else
					{
						return (it.Key.Kind & ResourcePropertyKind.ResourceReference) == ResourcePropertyKind.ResourceReference;
					}
				}
				);
				if (!flag1)
				{
					List<KeyValuePair<ResourceProperty, object>>.Enumerator enumerator = filterPropertyFinder.FilterProperties.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							Func<ResourceProperty, bool> func = null;
							KeyValuePair<ResourceProperty, object> current = enumerator.Current;
							ReadOnlyCollection<ResourceProperty> properties = this.baseResourceType.Properties;
							if (func == null)
							{
								func = (ResourceProperty it) => {
									KeyValuePair<ResourceProperty, object> keyValuePair = current;
									return it.Name == keyValuePair.Key.Name;
								}
								;
							}
							if (properties.FirstOrDefault<ResourceProperty>(func) != null)
							{
								continue;
							}
							flag = true;
							return flag;
						}
						return false;
					}
					finally
					{
						enumerator.Dispose();
					}
					return flag;
				}
				else
				{
					return true;
				}
			}
			else
			{
				return true;
			}
		}

		protected override Expression VisitMethodCall(MethodCallExpression expression)
		{
			if (!(expression.Method.Name == "Where") || expression.Arguments[0] == this.resourceRoot)
			{
				if ((expression.Method.Name == "Select" || expression.Method.Name == "SelectMany") && this.DoesPredicateContainNavOrDerivedProperties(expression.Arguments[1], this.baseResourceType))
				{
					this.MovableWhereExpression = null;
				}
			}
			else
			{
				MethodCallExpression item = expression.Arguments[0] as MethodCallExpression;
				if (item != null)
				{
					if (item.Method.Name != "Where")
					{
						if (!this.DoesPredicateContainNavOrDerivedProperties(expression.Arguments[1], this.baseResourceType))
						{
							this.MovableWhereExpression = expression;
						}
					}
					else
					{
						return base.VisitMethodCall(expression);
					}
				}
				else
				{
					return base.VisitMethodCall(expression);
				}
			}
			return base.VisitMethodCall(expression);
		}
	}
}