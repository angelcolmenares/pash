namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public class BlockStatementAst : StatementAst
    {
        public BlockStatementAst(IScriptExtent extent, Token kind, StatementBlockAst body) : base(extent)
        {
            if ((kind == null) || (body == null))
            {
                throw PSTraceSource.NewArgumentNullException((kind == null) ? "kind" : "body");
            }
            if ((kind.Kind != TokenKind.Sequence) && (kind.Kind != TokenKind.Parallel))
            {
                throw PSTraceSource.NewArgumentException("kind");
            }
            this.Kind = kind;
            this.Body = body;
            base.SetParent(body);
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitBlockStatement(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            return this.Body.GetInferredType(context);
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitBlockStatement(this);
            switch (action)
            {
                case AstVisitAction.SkipChildren:
                    return AstVisitAction.Continue;

                case AstVisitAction.Continue:
                    action = this.Body.InternalVisit(visitor);
                    break;
            }
            return action;
        }

        public StatementBlockAst Body { get; private set; }

        public Token Kind { get; private set; }
    }
}

