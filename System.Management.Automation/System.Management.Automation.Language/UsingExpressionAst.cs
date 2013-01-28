namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public class UsingExpressionAst : ExpressionAst
    {
        internal const string UsingPrefix = "__using_";

        public UsingExpressionAst(IScriptExtent extent, ExpressionAst expressionAst) : base(extent)
        {
            if (expressionAst == null)
            {
                throw PSTraceSource.NewArgumentNullException("expressionAst");
            }
            this.SubExpression = expressionAst;
            base.SetParent(this.SubExpression);
            this.RuntimeUsingIndex = -1;
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitUsingExpression(this);
        }

        public static VariableExpressionAst ExtractUsingVariable(UsingExpressionAst usingExpressionAst)
        {
            if (usingExpressionAst == null)
            {
                throw new ArgumentNullException("usingExpressionAst");
            }
            return ExtractUsingVariableImpl(usingExpressionAst);
        }

        private static VariableExpressionAst ExtractUsingVariableImpl(ExpressionAst expression)
        {
            VariableExpressionAst subExpression;
            UsingExpressionAst ast = expression as UsingExpressionAst;
            if (ast != null)
            {
                subExpression = ast.SubExpression as VariableExpressionAst;
                if (subExpression != null)
                {
                    return subExpression;
                }
                return ExtractUsingVariableImpl(ast.SubExpression);
            }
            IndexExpressionAst ast3 = expression as IndexExpressionAst;
            if (ast3 != null)
            {
                subExpression = ast3.Target as VariableExpressionAst;
                if (subExpression != null)
                {
                    return subExpression;
                }
                return ExtractUsingVariableImpl(ast3.Target);
            }
            MemberExpressionAst ast4 = expression as MemberExpressionAst;
            if (ast4 == null)
            {
                return null;
            }
            subExpression = ast4.Expression as VariableExpressionAst;
            if (subExpression != null)
            {
                return subExpression;
            }
            return ExtractUsingVariableImpl(ast4.Expression);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            return this.SubExpression.GetInferredType(context);
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitUsingExpression(this);
            switch (action)
            {
                case AstVisitAction.SkipChildren:
                    return AstVisitAction.Continue;

                case AstVisitAction.Continue:
                    action = this.SubExpression.InternalVisit(visitor);
                    break;
            }
            return action;
        }

        internal int RuntimeUsingIndex { get; set; }

        public ExpressionAst SubExpression { get; private set; }
    }
}

