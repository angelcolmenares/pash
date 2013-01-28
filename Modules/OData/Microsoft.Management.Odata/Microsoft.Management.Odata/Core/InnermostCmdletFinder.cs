using Microsoft.Management.Odata.Schema;
using System;
using System.Data.Services.Providers;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.Management.Odata.Core
{
	internal class InnermostCmdletFinder : ExpressionVisitor
	{
		private Expression cmdletExpression;

		private ExpressionCategory category;

		private IQueryable<DSResource> resourceRoot;

		private ResourceProperty navigationProperty;

		private EntityMetadata entityMetadata;

		public InnermostCmdletFinder()
		{
		}

		public bool GetInnermostCmdletExpression(Expression tree, IQueryable<DSResource> resourceRoot, EntityMetadata entityMetadata, out Expression innermost, out ExpressionCategory category)
		{
			this.category = ExpressionCategory.Unhandled;
			this.cmdletExpression = null;
			this.resourceRoot = resourceRoot;
			this.entityMetadata = entityMetadata;
			this.Visit(tree);
			innermost = this.cmdletExpression;
			category = this.category;
			return (int)category != 8;
		}

		protected override Expression VisitConstant(ConstantExpression expression)
		{
			object value = expression.Value;
			IQueryable<DSResource> dSResources = value as IQueryable<DSResource>;
			if (dSResources == this.resourceRoot)
			{
				this.cmdletExpression = expression;
				this.category = ExpressionCategory.ResourceRoot;
			}
			return expression;
		}

		protected override Expression VisitMethodCall(MethodCallExpression expression)
		{
			if (expression.Method.Name != "Where")
			{
				if (expression.Method.Name != "Select")
				{
					if (expression.Method.Name != "SelectMany")
					{
						this.navigationProperty = null;
					}
					else
					{
						this.cmdletExpression = expression;
						this.category = ExpressionCategory.SelectNavProperty;
						this.navigationProperty = ExpressionHelper.GetResourcePropertyFromSequence(expression.Arguments[1]);
					}
				}
				else
				{
					this.cmdletExpression = expression;
					this.category = ExpressionCategory.SelectExpansion;
					this.navigationProperty = null;
				}
			}
			else
			{
				if (!ExpressionHelper.IsResourceRoot(expression.Arguments[0], this.resourceRoot))
				{
					if (!ExpressionHelper.IsNestedWhereClause(expression.Arguments[0], this.resourceRoot) || !ExpressionHelper.IsPropertyComparisonPredicate(expression.Arguments[1]))
					{
						this.cmdletExpression = expression;
						this.category = ExpressionCategory.WhereOfResultSet;
						this.navigationProperty = null;
					}
					else
					{
						this.cmdletExpression = expression;
						if (this.navigationProperty.IsNavPropertyHasGetReferenceCmdlet(this.entityMetadata))
						{
							this.category = ExpressionCategory.NestedPropertyComparisonsInsideNavPropertyWithGetRefCmdlet;
						}
						else
						{
							this.category = ExpressionCategory.NestedPropertyComparisons;
						}
						return expression;
					}
				}
				else
				{
					this.cmdletExpression = expression;
					if (this.navigationProperty.IsNavPropertyHasGetReferenceCmdlet(this.entityMetadata))
					{
						this.category = ExpressionCategory.WhereInsideNavPropertyWithGetRefCmdlet;
					}
					else
					{
						this.category = ExpressionCategory.WhereOfResourceRoot;
					}
					return expression;
				}
			}
			this.Visit(expression.Arguments[0]);
			return expression;
		}
	}
}