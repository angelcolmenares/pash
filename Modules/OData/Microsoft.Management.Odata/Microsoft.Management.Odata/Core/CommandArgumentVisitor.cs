using System;
using System.Collections.Generic;
using System.Data.Services.Providers;
using System.Linq.Expressions;

namespace Microsoft.Management.Odata.Core
{
	internal class CommandArgumentVisitor : ExpressionVisitor
	{
		private ICommand command;

		private ExpressionType binaryExpressionRoot;

		private List<KeyValuePair<string, object>> parameterValues;

		private int orElseNodeCount;

		public CommandArgumentVisitor(ICommand command)
		{
			this.command = command;
			this.binaryExpressionRoot = ExpressionType.Default;
			this.parameterValues = new List<KeyValuePair<string, object>>();
		}

		private void HandleLastOrElse()
		{
			if (this.binaryExpressionRoot == ExpressionType.OrElse && this.parameterValues.Count == this.orElseNodeCount + 1)
			{
				KeyValuePair<string, object> item = this.parameterValues[0];
				string key = item.Key;
				bool flag = this.parameterValues.TrueForAll((KeyValuePair<string, object> it) => string.Equals(it.Key, key, StringComparison.OrdinalIgnoreCase));
				if (flag)
				{
					List<object> objs = new List<object>();
					this.parameterValues.ForEach((KeyValuePair<string, object> it) => objs.Add(it.Value));
					this.command.AddArrayFieldParameter(key, objs);
				}
			}
			this.binaryExpressionRoot = ExpressionType.Default;
			this.parameterValues.Clear();
			this.orElseNodeCount = 0;
		}

		protected override Expression VisitBinary(BinaryExpression binaryExpr)
		{
			ResourceProperty resourceProperty = null;
			object obj = null;
			if (binaryExpr.NodeType != ExpressionType.AndAlso)
			{
				if (binaryExpr.NodeType != ExpressionType.OrElse)
				{
					if (!ExpressionHelper.IsPropertyEqualityCheck(binaryExpr, out resourceProperty, out obj))
					{
						this.binaryExpressionRoot = binaryExpr.NodeType;
						return binaryExpr;
					}
					else
					{
						if (this.binaryExpressionRoot != ExpressionType.OrElse)
						{
							if (this.binaryExpressionRoot == ExpressionType.AndAlso || this.binaryExpressionRoot == ExpressionType.Default)
							{
								bool flag = this.command.AddFieldParameter(resourceProperty.Name, obj);
								if (!flag)
								{
									return base.VisitBinary(binaryExpr);
								}
								else
								{
									return Expression.Constant(true);
								}
							}
							else
							{
								return base.VisitBinary(binaryExpr);
							}
						}
						else
						{
							this.parameterValues.Add(new KeyValuePair<string, object>(resourceProperty.Name, obj));
							return base.VisitBinary(binaryExpr);
						}
					}
				}
				else
				{
					bool flag1 = false;
					if (this.binaryExpressionRoot == ExpressionType.OrElse)
					{
						CommandArgumentVisitor commandArgumentVisitor = this;
						commandArgumentVisitor.orElseNodeCount = commandArgumentVisitor.orElseNodeCount + 1;
					}
					else
					{
						this.binaryExpressionRoot = ExpressionType.OrElse;
						this.parameterValues.Clear();
						this.orElseNodeCount = 1;
						flag1 = true;
					}
					Expression expression = base.VisitBinary(binaryExpr);
					if (flag1)
					{
						this.HandleLastOrElse();
					}
					return expression;
				}
			}
			else
			{
				this.binaryExpressionRoot = ExpressionType.AndAlso;
				return base.VisitBinary(binaryExpr);
			}
		}

		protected override Expression VisitUnary(UnaryExpression unaryExpr)
		{
			if (unaryExpr.NodeType != ExpressionType.Quote)
			{
				return unaryExpr;
			}
			else
			{
				return base.VisitUnary(unaryExpr);
			}
		}
	}
}