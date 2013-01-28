namespace System.Management.Automation.Language
{
    using System;
    using System.Management.Automation;

    public class WhileStatementAst : LoopStatementAst
    {
        public WhileStatementAst(IScriptExtent extent, string label, PipelineBaseAst condition, StatementBlockAst body) : base(extent, label, condition, body)
        {
            if (condition == null)
            {
                throw PSTraceSource.NewArgumentNullException("condition");
            }
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitWhileStatement(this);
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitWhileStatement(this);
            switch (action)
            {
                case AstVisitAction.SkipChildren:
                    return AstVisitAction.Continue;

                case AstVisitAction.Continue:
                    action = base.Condition.InternalVisit(visitor);
                    break;
            }
            if (action == AstVisitAction.Continue)
            {
                action = base.Body.InternalVisit(visitor);
            }
            return action;
        }
    }
}

