namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    internal class MemberAssignableValue : IAssignableValue
    {
        private Expression GetPropertyExpr(Compiler compiler)
        {
            return compiler.Compile(this.MemberExpression.Member);
        }

        private Expression GetTargetExpr(Compiler compiler)
        {
            return compiler.Compile(this.MemberExpression.Expression);
        }

        public Expression GetValue(Compiler compiler, List<Expression> exprs, List<ParameterExpression> temps)
        {
            Expression targetExpr = this.GetTargetExpr(compiler);
            ParameterExpression item = Expression.Parameter(targetExpr.Type);
            temps.Add(item);
            this.CachedTarget = item;
            exprs.Add(Expression.Assign(item, targetExpr));
            StringConstantExpressionAst member = this.MemberExpression.Member as StringConstantExpressionAst;
            if (member != null)
            {
                return Expression.Dynamic(PSGetMemberBinder.Get(member.Value, this.MemberExpression.Static), typeof(object), item);
            }
            ParameterExpression expression4 = Expression.Parameter(this.GetPropertyExpr(compiler).Type);
            temps.Add(expression4);
            exprs.Add(Expression.Assign(expression4, compiler.Compile(this.MemberExpression.Member)));
            this.CachedPropertyExpr = expression4;
            return Expression.Dynamic(PSGetDynamicMemberBinder.Get(this.MemberExpression.Static), typeof(object), item, expression4);
        }

        public Expression SetValue(Compiler compiler, Expression rhs)
        {
            StringConstantExpressionAst member = this.MemberExpression.Member as StringConstantExpressionAst;
            if (member != null)
            {
                return Expression.Dynamic(PSSetMemberBinder.Get(member.Value, this.MemberExpression.Static), typeof(object), this.CachedTarget ?? this.GetTargetExpr(compiler), rhs);
            }
            return Expression.Dynamic(PSSetDynamicMemberBinder.Get(this.MemberExpression.Static), typeof(object), this.CachedTarget ?? this.GetTargetExpr(compiler), this.CachedPropertyExpr ?? this.GetPropertyExpr(compiler), rhs);
        }

        private Expression CachedPropertyExpr { get; set; }

        private Expression CachedTarget { get; set; }

        internal MemberExpressionAst MemberExpression { get; set; }
    }
}

