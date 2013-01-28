using Microsoft.Management.Odata;
using System;
using System.Data.Services.Providers;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.Management.Odata.Core
{
	internal class DSMethodTranslatingVisitor : ExpressionVisitor
	{
		private DataServiceQueryProvider.ResultSetCollection resultSets;

		public DSMethodTranslatingVisitor(DataServiceQueryProvider.ResultSetCollection resultSets)
		{
			this.resultSets = resultSets;
		}

		protected override Expression VisitMethodCall(MethodCallExpression methodCallExpr)
		{
			if (methodCallExpr.Method != ExpressionHelper.GetValueMethodInfo)
			{
				if (!methodCallExpr.Method.IsGenericMethod || !(methodCallExpr.Method.GetGenericMethodDefinition() == ExpressionHelper.GetSequenceValueMethodInfo))
				{
					if (!methodCallExpr.Method.IsGenericMethod || !(methodCallExpr.Method == ExpressionHelper.OfTypeMethodInfo))
					{
						return base.VisitMethodCall(methodCallExpr);
					}
					else
					{
						ConstantExpression item = (ConstantExpression)methodCallExpr.Arguments[0];
						ConstantExpression constantExpression = (ConstantExpression)methodCallExpr.Arguments[1];
						IQueryable<DSResource> value = item.Value as IQueryable<DSResource>;
						var resourceType = constantExpression.Value as ResourceType;
						if (value == null || resourceType == null)
						{
							throw new ArgumentException(Resources.InvalidExpression);
						}
						else
						{
							
							ParameterExpression parameterExpression = Expression.Parameter(typeof(DSResource), "item");
							ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
							parameterExpressionArray[0] = parameterExpression;
							return Expression.Constant(value.Where<DSResource>(Expression.Lambda<Func<DSResource, bool>>(Expression.Equal(Expression.Property(parameterExpression, typeof(DSResource).GetProperty ("ResourceType").GetGetMethod ()), Expression.Field(Expression.Constant(resourceType), "resourceType")), parameterExpressionArray)).AsQueryable<DSResource>());
						}
					}
				}
				else
				{
					return Expression.Convert(Expression.Call(this.Visit(methodCallExpr.Arguments[0]), typeof(DSResource).GetMethod("GetValue"), Expression.Property(methodCallExpr.Arguments[1], "Name"), Expression.Constant(this.resultSets)), methodCallExpr.Method.ReturnType);
				}
			}
			else
			{
				return Expression.Call(this.Visit(methodCallExpr.Arguments[0]), typeof(DSResource).GetMethod("GetValue"), Expression.Property(methodCallExpr.Arguments[1], "Name"), Expression.Constant(this.resultSets));
			}
		}
	}
}