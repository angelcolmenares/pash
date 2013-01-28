namespace System.Management.Automation.Language
{
    using System;
    using System.Runtime.CompilerServices;

    public class ForStatementAst : LoopStatementAst
    {
        public ForStatementAst(IScriptExtent extent, string label, PipelineBaseAst initializer, PipelineBaseAst condition, PipelineBaseAst iterator, StatementBlockAst body) : base(extent, label, condition, body)
        {
            if (initializer != null)
            {
                this.Initializer = initializer;
                base.SetParent(initializer);
            }
            if (iterator != null)
            {
                this.Iterator = iterator;
                base.SetParent(iterator);
            }
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitForStatement(this);
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitForStatement(this);
            if (action == AstVisitAction.SkipChildren)
            {
                return AstVisitAction.Continue;
            }
            if ((action == AstVisitAction.Continue) && (this.Initializer != null))
            {
                action = this.Initializer.InternalVisit(visitor);
            }
            if ((action == AstVisitAction.Continue) && (base.Condition != null))
            {
                action = base.Condition.InternalVisit(visitor);
            }
            if ((action == AstVisitAction.Continue) && (this.Iterator != null))
            {
                action = this.Iterator.InternalVisit(visitor);
            }
            if (action == AstVisitAction.Continue)
            {
                action = base.Body.InternalVisit(visitor);
            }
            return action;
        }

        public PipelineBaseAst Initializer { get; private set; }

        public PipelineBaseAst Iterator { get; private set; }
    }
}

