namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    internal class IndexAssignableValue : IAssignableValue
    {
        private ParameterExpression _indexExprTemp;
        private ParameterExpression _targetExprTemp;

        private Expression GetIndexExpr(Compiler compiler)
        {
            return (this._indexExprTemp ?? compiler.Compile(this.IndexExpressionAst.Index));
        }

        private PSMethodInvocationConstraints GetInvocationConstraints()
        {
            return Compiler.CombineTypeConstraintForMethodResolution(Compiler.GetTypeConstraintForMethodResolution(this.IndexExpressionAst.Target), Compiler.GetTypeConstraintForMethodResolution(this.IndexExpressionAst.Index));
        }

        private Expression GetTargetExpr(Compiler compiler)
        {
            return (this._targetExprTemp ?? compiler.Compile(this.IndexExpressionAst.Target));
        }

        public Expression GetValue(Compiler compiler, List<Expression> exprs, List<ParameterExpression> temps)
        {
            Expression right = compiler.Compile(this.IndexExpressionAst.Target);
            this._targetExprTemp = Expression.Variable(right.Type);
            temps.Add(this._targetExprTemp);
            exprs.Add(Expression.Assign(this._targetExprTemp, right));
            ExpressionAst index = this.IndexExpressionAst.Index;
            ArrayLiteralAst ast2 = index as ArrayLiteralAst;
            PSMethodInvocationConstraints invocationConstraints = this.GetInvocationConstraints();
            if (ast2 != null)
            {
                return Expression.Dynamic(PSGetIndexBinder.Get(ast2.Elements.Count, invocationConstraints, true), typeof(object), ast2.Elements.Select<ExpressionAst, Expression>(new Func<ExpressionAst, Expression>(compiler.Compile)).Prepend<Expression>(this._targetExprTemp));
            }
            Expression expression3 = compiler.Compile(index);
            this._indexExprTemp = Expression.Variable(expression3.Type);
            temps.Add(this._indexExprTemp);
            exprs.Add(Expression.Assign(this._indexExprTemp, expression3));
            return Expression.Dynamic(PSGetIndexBinder.Get(1, invocationConstraints, true), typeof(object), this._targetExprTemp, this._indexExprTemp);
        }

        public Expression SetValue(Compiler compiler, Expression rhs)
        {
            Expression expression3;
            ParameterExpression element = Expression.Variable(rhs.Type);
            ArrayLiteralAst index = this.IndexExpressionAst.Index as ArrayLiteralAst;
            PSMethodInvocationConstraints invocationConstraints = this.GetInvocationConstraints();
            Expression targetExpr = this.GetTargetExpr(compiler);
            if (index != null)
            {
                expression3 = Expression.Dynamic(PSSetIndexBinder.Get(index.Elements.Count, invocationConstraints), typeof(object), index.Elements.Select<ExpressionAst, Expression>(new Func<ExpressionAst, Expression>(compiler.Compile)).Prepend<Expression>(targetExpr).Append<Expression>(element));
            }
            else
            {
                expression3 = Expression.Dynamic(PSSetIndexBinder.Get(1, invocationConstraints), typeof(object), targetExpr, this.GetIndexExpr(compiler), element);
            }
            return Expression.Block(new ParameterExpression[] { element }, new Expression[] { Expression.Assign(element, rhs), expression3, element });
        }

        internal System.Management.Automation.Language.IndexExpressionAst IndexExpressionAst { get; set; }
    }
}

