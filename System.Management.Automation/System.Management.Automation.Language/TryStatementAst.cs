namespace System.Management.Automation.Language
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class TryStatementAst : StatementAst
    {
        private static readonly ReadOnlyCollection<CatchClauseAst> EmptyCatchClauses = new ReadOnlyCollection<CatchClauseAst>(new CatchClauseAst[0]);

        public TryStatementAst(IScriptExtent extent, StatementBlockAst body, IEnumerable<CatchClauseAst> catchClauses, StatementBlockAst @finally) : base(extent)
        {
            if (body == null)
            {
                throw PSTraceSource.NewArgumentNullException("body");
            }
            if (((catchClauses == null) || !catchClauses.Any<CatchClauseAst>()) && (@finally == null))
            {
                throw PSTraceSource.NewArgumentException("catchClauses");
            }
            this.Body = body;
            base.SetParent(body);
            if ((catchClauses != null) && catchClauses.Any<CatchClauseAst>())
            {
                this.CatchClauses = new ReadOnlyCollection<CatchClauseAst>(catchClauses.ToArray<CatchClauseAst>());
                base.SetParents((IEnumerable<Ast>) this.CatchClauses);
            }
            else
            {
                this.CatchClauses = EmptyCatchClauses;
            }
            if (@finally != null)
            {
                this.Finally = @finally;
                base.SetParent(@finally);
            }
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitTryStatement(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            foreach (PSTypeName iteratorVariable0 in this.Body.GetInferredType(context))
            {
                yield return iteratorVariable0;
            }
            foreach (PSTypeName iteratorVariable1 in from clause in this.CatchClauses select clause.Body.GetInferredType(context))
            {
                yield return iteratorVariable1;
            }
            if (this.Finally != null)
            {
                foreach (PSTypeName iteratorVariable2 in this.Finally.GetInferredType(context))
                {
                    yield return iteratorVariable2;
                }
            }
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitTryStatement(this);
            switch (action)
            {
                case AstVisitAction.SkipChildren:
                    return AstVisitAction.Continue;

                case AstVisitAction.Continue:
                    action = this.Body.InternalVisit(visitor);
                    break;
            }
            if (action == AstVisitAction.Continue)
            {
                foreach (CatchClauseAst ast in this.CatchClauses)
                {
                    action = ast.InternalVisit(visitor);
                    if (action != AstVisitAction.Continue)
                    {
                        break;
                    }
                }
            }
            if ((action == AstVisitAction.Continue) && (this.Finally != null))
            {
                action = this.Finally.InternalVisit(visitor);
            }
            return action;
        }

        public StatementBlockAst Body { get; private set; }

        public ReadOnlyCollection<CatchClauseAst> CatchClauses { get; private set; }

        public StatementBlockAst Finally { get; private set; }

        
    }
}

