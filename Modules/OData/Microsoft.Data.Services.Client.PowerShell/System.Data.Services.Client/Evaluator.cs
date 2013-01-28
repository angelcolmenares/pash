namespace System.Data.Services.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal static class Evaluator
    {
        private static bool CanBeEvaluatedLocally(Expression expression)
        {
            return (((expression.NodeType != ExpressionType.Parameter) && (expression.NodeType != ExpressionType.Lambda)) && (expression.NodeType != ((ExpressionType) 0x2710)));
        }

        internal static Expression PartialEval(Expression expression)
        {
            return PartialEval(expression, new Func<Expression, bool>(Evaluator.CanBeEvaluatedLocally));
        }

        internal static Expression PartialEval(Expression expression, Func<Expression, bool> canBeEvaluated)
        {
            Nominator nominator = new Nominator(canBeEvaluated);
            return new SubtreeEvaluator(nominator.Nominate(expression)).Eval(expression);
        }

        internal class Nominator : DataServiceALinqExpressionVisitor
        {
            private HashSet<Expression> candidates;
            private bool cannotBeEvaluated;
            private Func<Expression, bool> functionCanBeEvaluated;

            internal Nominator(Func<Expression, bool> functionCanBeEvaluated)
            {
                this.functionCanBeEvaluated = functionCanBeEvaluated;
            }

            internal HashSet<Expression> Nominate(Expression expression)
            {
                this.candidates = new HashSet<Expression>(EqualityComparer<Expression>.Default);
                this.Visit(expression);
                return this.candidates;
            }

            internal override Expression Visit(Expression expression)
            {
                if (expression != null)
                {
                    bool cannotBeEvaluated = this.cannotBeEvaluated;
                    this.cannotBeEvaluated = false;
                    base.Visit(expression);
                    if (!this.cannotBeEvaluated)
                    {
                        if (this.functionCanBeEvaluated(expression))
                        {
                            this.candidates.Add(expression);
                        }
                        else
                        {
                            this.cannotBeEvaluated = true;
                        }
                    }
                    this.cannotBeEvaluated |= cannotBeEvaluated;
                }
                return expression;
            }
        }

        internal class SubtreeEvaluator : DataServiceALinqExpressionVisitor
        {
            private HashSet<Expression> candidates;

            internal SubtreeEvaluator(HashSet<Expression> candidates)
            {
                this.candidates = candidates;
            }

            internal Expression Eval(Expression exp)
            {
                return this.Visit(exp);
            }

            private static Expression Evaluate(Expression e)
            {
                if (e.NodeType == ExpressionType.Constant)
                {
                    return e;
                }
                object obj2 = Expression.Lambda(e, new ParameterExpression[0]).Compile().DynamicInvoke(null);
                Type type = e.Type;
                if (((obj2 != null) && type.IsArray) && (type.GetElementType() == obj2.GetType().GetElementType()))
                {
                    type = obj2.GetType();
                }
                return Expression.Constant(obj2, type);
            }

            internal override Expression Visit(Expression exp)
            {
                if (exp == null)
                {
                    return null;
                }
                if (this.candidates.Contains(exp))
                {
                    return Evaluate(exp);
                }
                return base.Visit(exp);
            }
        }
    }
}

