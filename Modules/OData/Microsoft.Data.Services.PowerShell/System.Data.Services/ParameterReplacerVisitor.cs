namespace System.Data.Services
{
    using System;
    using System.Linq.Expressions;

    internal class ParameterReplacerVisitor : ALinqExpressionVisitor
    {
        private readonly Expression newExpression;
        private readonly ParameterExpression oldParameter;

        private ParameterReplacerVisitor(ParameterExpression oldParameter, Expression newExpression)
        {
            this.oldParameter = oldParameter;
            this.newExpression = newExpression;
        }

        internal static Expression Replace(Expression expression, ParameterExpression oldParameter, Expression newExpression)
        {
            return new ParameterReplacerVisitor(oldParameter, newExpression).Visit(expression);
        }

        internal override Expression VisitParameter(ParameterExpression p)
        {
            if (p != this.oldParameter)
            {
                return p;
            }
            return this.newExpression;
        }
    }
}

