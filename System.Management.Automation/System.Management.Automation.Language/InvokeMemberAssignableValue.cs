namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    internal class InvokeMemberAssignableValue : IAssignableValue
    {
        private IEnumerable<ParameterExpression> _argExprTemps;
        private ParameterExpression _targetExprTemp;

        private IEnumerable<Expression> GetArgumentExprs(Compiler compiler)
        {
            if (this._argExprTemps != null)
            {
                return this._argExprTemps;
            }
            if (this.InvokeMemberExpressionAst.Arguments != null)
            {
                return this.InvokeMemberExpressionAst.Arguments.Select<ExpressionAst, Expression>(new Func<ExpressionAst, Expression>(compiler.Compile)).ToArray<Expression>();
            }
            return new Expression[0];
        }

        private Expression GetTargetExpr(Compiler compiler)
        {
            return (this._targetExprTemp ?? compiler.Compile(this.InvokeMemberExpressionAst.Expression));
        }

        public Expression GetValue(Compiler compiler, List<Expression> exprs, List<ParameterExpression> temps)
        {
            PSMethodInvocationConstraints invokeMemberConstraints = Compiler.GetInvokeMemberConstraints(this.InvokeMemberExpressionAst);
            Expression targetExpr = this.GetTargetExpr(compiler);
            this._targetExprTemp = Expression.Variable(targetExpr.Type);
            exprs.Add(Expression.Assign(this._targetExprTemp, targetExpr));
            IEnumerable<Expression> argumentExprs = this.GetArgumentExprs(compiler);
            this._argExprTemps = (from arg in argumentExprs select Expression.Variable(arg.Type)).ToArray<ParameterExpression>();
            exprs.AddRange((IEnumerable<Expression>) argumentExprs.Zip<Expression, ParameterExpression, BinaryExpression>(this._argExprTemps, (arg, temp) => Expression.Assign(temp, arg)));
            temps.Add(this._targetExprTemp);
            temps.AddRange(this._argExprTemps);
            StringConstantExpressionAst member = this.InvokeMemberExpressionAst.Member as StringConstantExpressionAst;
            if (member == null)
            {
                throw new NotImplementedException("invoke method dynamic name");
            }
            return Compiler.InvokeMember(member.Value, invokeMemberConstraints, this._targetExprTemp, this._argExprTemps, false, false);
        }

        public Expression SetValue(Compiler compiler, Expression rhs)
        {
            PSMethodInvocationConstraints invokeMemberConstraints = Compiler.GetInvokeMemberConstraints(this.InvokeMemberExpressionAst);
            StringConstantExpressionAst member = this.InvokeMemberExpressionAst.Member as StringConstantExpressionAst;
            Expression targetExpr = this.GetTargetExpr(compiler);
            IEnumerable<Expression> argumentExprs = this.GetArgumentExprs(compiler);
            if (member == null)
            {
                throw new NotImplementedException("invoke method dynamic name");
            }
            return Compiler.InvokeMember(member.Value, invokeMemberConstraints, targetExpr, argumentExprs.Append<Expression>(rhs), false, true);
        }

        internal System.Management.Automation.Language.InvokeMemberExpressionAst InvokeMemberExpressionAst { get; set; }
    }
}

