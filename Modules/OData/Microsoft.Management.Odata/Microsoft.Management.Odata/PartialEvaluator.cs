using Microsoft.Management.Odata.Core;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Microsoft.Management.Odata
{
	internal static class PartialEvaluator
	{
		private static bool CanBeEvaluatedLocally(Expression expression)
		{
			ExpressionType nodeType = expression.NodeType;
			if (nodeType == ExpressionType.New)
			{
				return false;
			}
			else
			{
				if (nodeType != ExpressionType.Parameter)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public static Expression Eval(Expression expression)
		{
			return PartialEvaluator.Eval(expression, null, null);
		}

		public static Expression Eval(Expression expression, Func<Expression, bool> funcCanBeEvaluated)
		{
			return PartialEvaluator.Eval(expression, funcCanBeEvaluated, null);
		}

		public static Expression Eval(Expression expression, Func<Expression, bool> funcCanBeEvaluated, Func<ConstantExpression, Expression> funcPostEval)
		{
			if (funcCanBeEvaluated == null)
			{
				funcCanBeEvaluated = new Func<Expression, bool>(PartialEvaluator.CanBeEvaluatedLocally);
			}
			return PartialEvaluator.SubtreeEvaluator.Eval(PartialEvaluator.Nominator.Nominate(funcCanBeEvaluated, expression), funcPostEval, expression);
		}

		private class Nominator : ExpressionVisitor
		{
			private Func<Expression, bool> funcCanBeEvaluated;

			private HashSet<Expression> candidates;

			private bool cannotBeEvaluated;

			private Nominator(Func<Expression, bool> funcCanBeEvaluated)
			{
				this.candidates = new HashSet<Expression>();
				this.funcCanBeEvaluated = funcCanBeEvaluated;
			}

			internal static HashSet<Expression> Nominate(Func<Expression, bool> funcCanBeEvaluated, Expression expression)
			{
				PartialEvaluator.Nominator nominator = new PartialEvaluator.Nominator(funcCanBeEvaluated);
				nominator.Visit(expression);
				return nominator.candidates;
			}

			public override Expression Visit(Expression expression)
			{
				if (expression != null)
				{
					bool flag = this.cannotBeEvaluated;
					this.cannotBeEvaluated = false;
					base.Visit(expression);
					if (!this.cannotBeEvaluated)
					{
						if (!this.funcCanBeEvaluated(expression))
						{
							this.cannotBeEvaluated = true;
						}
						else
						{
							this.candidates.Add(expression);
						}
					}
					PartialEvaluator.Nominator nominator = this;
					nominator.cannotBeEvaluated = nominator.cannotBeEvaluated | flag;
				}
				return expression;
			}

			protected override Expression VisitConstant(ConstantExpression c)
			{
				return base.VisitConstant(c);
			}
		}

		private class SubtreeEvaluator : ExpressionVisitor
		{
			private HashSet<Expression> candidates;

			private Func<ConstantExpression, Expression> onEval;

			private SubtreeEvaluator(HashSet<Expression> candidates, Func<ConstantExpression, Expression> onEval)
			{
				this.candidates = candidates;
				this.onEval = onEval;
			}

			internal static Expression Eval(HashSet<Expression> candidates, Func<ConstantExpression, Expression> onEval, Expression exp)
			{
				return (new PartialEvaluator.SubtreeEvaluator(candidates, onEval)).Visit(exp);
			}

			private Expression Evaluate(Expression e)
			{
				Type type = e.Type;
				if (e.NodeType == ExpressionType.Convert)
				{
					UnaryExpression unaryExpression = (UnaryExpression)e;
					if (TypeSystem.GetNonNullableType(unaryExpression.Operand.Type) == TypeSystem.GetNonNullableType(type))
					{
						e = ((UnaryExpression)e).Operand;
					}
				}
				if (e.NodeType == ExpressionType.Constant)
				{
					if (e.Type != type)
					{
						if (TypeSystem.GetNonNullableType(e.Type) == TypeSystem.GetNonNullableType(type))
						{
							return Expression.Constant(((ConstantExpression)e).Value, type);
						}
					}
					else
					{
						return e;
					}
				}
				MemberExpression memberExpression = e as MemberExpression;
				if (memberExpression != null)
				{
					ConstantExpression expression = memberExpression.Expression as ConstantExpression;
					if (expression != null)
					{
						return this.PostEval(Expression.Constant(memberExpression.Member.GetValue(expression.Value), type));
					}
				}
				if (type.IsValueType)
				{
					e = Expression.Convert(e, typeof(object));
				}
				Expression<Func<object>> expression1 = Expression.Lambda<Func<object>>(e, new ParameterExpression[0]);
				Func<object> func = expression1.Compile();
				return this.PostEval(Expression.Constant(func(), type));
			}

			private Expression PostEval(ConstantExpression e)
			{
				if (this.onEval == null)
				{
					return e;
				}
				else
				{
					return this.onEval(e);
				}
			}

			public override Expression Visit(Expression exp)
			{
				if (exp != null)
				{
					if (!this.candidates.Contains(exp))
					{
						return base.Visit(exp);
					}
					else
					{
						return this.Evaluate(exp);
					}
				}
				else
				{
					return null;
				}
			}
		}
	}
}