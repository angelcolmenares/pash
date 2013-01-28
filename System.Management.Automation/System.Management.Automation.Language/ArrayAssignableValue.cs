namespace System.Management.Automation.Language
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    internal class ArrayAssignableValue : IAssignableValue
    {
        public Expression GetValue(Compiler compiler, List<Expression> exprs, List<ParameterExpression> temps)
        {
            return null;
        }

        public Expression SetValue(Compiler compiler, Expression rhs)
        {
            ParameterExpression left = Expression.Variable(rhs.Type);
            int count = this.ArrayLiteral.Elements.Count;
            List<Expression> expressions = new List<Expression> {
                Expression.Assign(left, rhs)
            };
            for (int i = 0; i < count; i++)
            {
                Expression expression2 = Expression.Call(left, CachedReflectionInfo.IList_get_Item, new Expression[] { ExpressionCache.Constant(i) });
                ExpressionAst ast = this.ArrayLiteral.Elements[i];
                ArrayLiteralAst pureExpression = ast as ArrayLiteralAst;
                if (ast is ParenExpressionAst)
                {
                    pureExpression = ((ParenExpressionAst) ast).Pipeline.GetPureExpression() as ArrayLiteralAst;
                }
                if (pureExpression != null)
                {
                    expression2 = Expression.Dynamic(PSArrayAssignmentRHSBinder.Get(pureExpression.Elements.Count), typeof(IList), expression2);
                }
                expressions.Add(compiler.ReduceAssignment((ISupportsAssignment) ast, TokenKind.Equals, expression2));
            }
            expressions.Add(left);
            return Expression.Block(new ParameterExpression[] { left }, expressions);
        }

        internal ArrayLiteralAst ArrayLiteral { get; set; }
    }
}

