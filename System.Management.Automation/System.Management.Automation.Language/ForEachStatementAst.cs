namespace System.Management.Automation.Language
{
    using System;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public class ForEachStatementAst : LoopStatementAst
    {
        public ForEachStatementAst(IScriptExtent extent, string label, ForEachFlags flags, VariableExpressionAst variable, PipelineBaseAst expression, StatementBlockAst body) : base(extent, label, expression, body)
        {
            if ((expression == null) || (variable == null))
            {
                throw PSTraceSource.NewArgumentNullException((expression == null) ? "expression" : "variablePath");
            }
            this.Flags = flags;
            this.Variable = variable;
            base.SetParent(variable);
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitForEachStatement(this);
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitForEachStatement(this);
            switch (action)
            {
                case AstVisitAction.SkipChildren:
                    return AstVisitAction.Continue;

                case AstVisitAction.Continue:
                    action = this.Variable.InternalVisit(visitor);
                    break;
            }
            if (action == AstVisitAction.Continue)
            {
                action = base.Condition.InternalVisit(visitor);
            }
            if (action == AstVisitAction.Continue)
            {
                action = base.Body.InternalVisit(visitor);
            }
            return action;
        }

        public ForEachFlags Flags { get; private set; }

        public VariableExpressionAst Variable { get; private set; }
    }
}

