namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal class ExpressionEvaluator : ALinqExpressionVisitor
    {
        private static readonly MethodInfo CreateNewArrayMethodInfo = typeof(ExpressionEvaluator).GetMethod("CreateNewArray", BindingFlags.NonPublic | BindingFlags.Static);

        private static Expression CreateNewArray<TElement>(IEnumerable<TElement> elements)
        {
            return Expression.Constant(new List<TElement>(elements).ToArray(), typeof(TElement[]));
        }

        internal static object Evaluate(Expression exp)
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();
            ConstantExpression expression = (ConstantExpression) evaluator.Visit(exp);
            return expression.Value;
        }

        internal override Expression Visit(Expression exp)
        {
            if (exp == null)
            {
                return exp;
            }
            switch (exp.NodeType)
            {
                case ExpressionType.Call:
                    return this.VisitMethodCall((MethodCallExpression) exp);

                case ExpressionType.Constant:
                    return this.VisitConstant((ConstantExpression) exp);

                case ExpressionType.Lambda:
                    return this.VisitLambda((LambdaExpression) exp);

                case ExpressionType.NewArrayInit:
                    return this.VisitNewArray((NewArrayExpression) exp);

                case ExpressionType.Quote:
                    return this.VisitUnary((UnaryExpression) exp);
            }
            throw new NotSupportedException(System.Data.Services.Strings.ALinq_UnsupportedExpression(exp.NodeType.ToString()));
        }

        internal override Expression VisitLambda(LambdaExpression lambda)
        {
            return Expression.Constant(lambda);
        }

        internal override Expression VisitMethodCall(MethodCallExpression m)
        {
            Expression expression;
            object obj2 = (m.Object == null) ? null : ((ConstantExpression) this.Visit(m.Object)).Value;
            try
            {
                expression = Expression.Constant(m.Method.Invoke(obj2, (from arg in this.VisitExpressionList(m.Arguments) select ((ConstantExpression) arg).Value).ToArray<object>()));
            }
            catch (TargetInvocationException exception)
            {
                ErrorHandler.HandleTargetInvocationException(exception);
                throw;
            }
            return expression;
        }

        internal override Expression VisitNewArray(NewArrayExpression na)
        {
            IEnumerable<Expression> enumerable = this.VisitExpressionList(na.Expressions);
            object[] parameters = new object[] { from e in enumerable select ((ConstantExpression) e).Value };
            return (Expression) CreateNewArrayMethodInfo.MakeGenericMethod(new Type[] { na.Type.GetElementType() }).Invoke(null, parameters);
        }

        internal override Expression VisitUnary(UnaryExpression u)
        {
            return this.Visit(u.Operand);
        }
    }
}

