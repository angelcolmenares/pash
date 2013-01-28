namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public class SubExpressionAst : ExpressionAst
    {
        public SubExpressionAst(IScriptExtent extent, StatementBlockAst statementBlock) : base(extent)
        {
            if (statementBlock == null)
            {
                throw PSTraceSource.NewArgumentNullException("statementBlock");
            }
            this.SubExpression = statementBlock;
            base.SetParent(statementBlock);
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitSubExpression(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            return this.SubExpression.GetInferredType(context);
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitSubExpression(this);
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

        public StatementBlockAst SubExpression { get; private set; }
    }
}

