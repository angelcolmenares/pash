namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public class ContinueStatementAst : StatementAst
    {
        public ContinueStatementAst(IScriptExtent extent, ExpressionAst label) : base(extent)
        {
            if (label != null)
            {
                this.Label = label;
                base.SetParent(label);
            }
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitContinueStatement(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            return Ast.EmptyPSTypeNameArray;
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitContinueStatement(this);
            if (action == AstVisitAction.SkipChildren)
            {
                return AstVisitAction.Continue;
            }
            if ((action == AstVisitAction.Continue) && (this.Label != null))
            {
                action = this.Label.InternalVisit(visitor);
            }
            return action;
        }

        public ExpressionAst Label { get; private set; }
    }
}

