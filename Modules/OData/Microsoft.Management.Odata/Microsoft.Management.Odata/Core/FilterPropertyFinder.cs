using Microsoft.Management.Odata.Common;
using System;
using System.Collections.Generic;
using System.Data.Services.Providers;
using System.Linq.Expressions;

namespace Microsoft.Management.Odata.Core
{
	internal class FilterPropertyFinder : ExpressionVisitor
	{
		public List<KeyValuePair<ResourceProperty, object>> FilterProperties
		{
			get;
			private set;
		}

		public bool IsCompleteExpressionParsed
		{
			get;
			private set;
		}

		public FilterPropertyFinder(Expression expression)
		{
			this.FilterProperties = new List<KeyValuePair<ResourceProperty, object>>();
			this.IsCompleteExpressionParsed = true;
			this.Visit(expression);
		}

		protected override Expression VisitBinary(BinaryExpression binaryExpr)
		{
			ResourceProperty resourceProperty = null;
			object obj = null;
			if (!ExpressionHelper.ContainsComparisonOperator(binaryExpr))
			{
				if (!ExpressionHelper.ContainsLogicalOperator(binaryExpr))
				{
					this.IsCompleteExpressionParsed = false;
				}
			}
			else
			{
				if (!ExpressionHelper.GetPropertyNameAndValue(binaryExpr, out resourceProperty, out obj))
				{
					TraceHelper.Current.DebugMessage("Expression is too complex to execute");
					this.IsCompleteExpressionParsed = false;
				}
				else
				{
					this.FilterProperties.Add(new KeyValuePair<ResourceProperty, object>(resourceProperty, obj));
				}
			}
			return base.VisitBinary(binaryExpr);
		}

		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			if (node.Method.Name == "GetValue" || node.Method.Name == "GetSequenceValue")
			{
				ConstantExpression item = node.Arguments[1] as ConstantExpression;
				if (item != null)
				{
					ResourceProperty value = item.Value as ResourceProperty;
					if (value != null)
					{
						this.FilterProperties.Add(new KeyValuePair<ResourceProperty, object>(value, null));
					}
				}
			}
			return base.VisitMethodCall(node);
		}
	}
}