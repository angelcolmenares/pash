namespace System.Data.Services.Internal
{
    using System;
    using System.Data.Services;
    using System.Data.Services.Providers;
    using System.Linq.Expressions;
    using System.Reflection;

    internal abstract class PropertyAccessVisitor : ALinqExpressionVisitor
    {
        protected PropertyAccessVisitor()
        {
        }

        protected abstract bool ProcessPropertyAccess(string propertyName, ref Expression operandExpression, ref Expression accessExpression);
        internal override Expression VisitMemberAccess(MemberExpression m)
        {
            if (m.Member.MemberType == MemberTypes.Property)
            {
                Expression operandExpression = m.Expression;
                Expression accessExpression = m;
                if (this.ProcessPropertyAccess(m.Member.Name, ref operandExpression, ref accessExpression))
                {
                    return (accessExpression ?? Expression.MakeMemberAccess(operandExpression, m.Member));
                }
            }
            return base.VisitMemberAccess(m);
        }

        internal override Expression VisitMethodCall(MethodCallExpression m)
        {
            string propertyName = null;
            if ((m.Method.IsGenericMethod && (m.Method.GetGenericMethodDefinition() == DataServiceProviderMethods.GetSequenceValueMethodInfo)) || (m.Method == DataServiceProviderMethods.GetValueMethodInfo))
            {
                ConstantExpression expression = m.Arguments[1] as ConstantExpression;
                ResourceProperty property = expression.Value as ResourceProperty;
                propertyName = property.Name;
            }
            else if (m.Method == OpenTypeMethods.GetValueOpenPropertyMethodInfo)
            {
                ConstantExpression expression2 = m.Arguments[1] as ConstantExpression;
                propertyName = expression2.Value as string;
            }
            if (propertyName != null)
            {
                Expression operandExpression = m.Arguments[0];
                Expression accessExpression = m;
                if (this.ProcessPropertyAccess(propertyName, ref operandExpression, ref accessExpression))
                {
                    if (accessExpression == null)
                    {
                        return Expression.Call(m.Object, m.Method, operandExpression, m.Arguments[1]);
                    }
                    return accessExpression;
                }
            }
            return base.VisitMethodCall(m);
        }
    }
}

