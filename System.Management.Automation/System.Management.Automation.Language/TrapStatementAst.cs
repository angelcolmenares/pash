namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public class TrapStatementAst : StatementAst
    {
        public TrapStatementAst(IScriptExtent extent, TypeConstraintAst trapType, StatementBlockAst body) : base(extent)
        {
            if (body == null)
            {
                throw PSTraceSource.NewArgumentNullException("body");
            }
            if (trapType != null)
            {
                this.TrapType = trapType;
                base.SetParent(trapType);
            }
            this.Body = body;
            base.SetParent(body);
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitTrap(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            return this.Body.GetInferredType(context);
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitTrap(this);
            if (action == AstVisitAction.SkipChildren)
            {
                return AstVisitAction.Continue;
            }
            if ((action == AstVisitAction.Continue) && (this.TrapType != null))
            {
                action = this.TrapType.InternalVisit(visitor);
            }
            if (action == AstVisitAction.Continue)
            {
                action = this.Body.InternalVisit(visitor);
            }
            return action;
        }

        public StatementBlockAst Body { get; private set; }

        public TypeConstraintAst TrapType { get; private set; }
    }
}

