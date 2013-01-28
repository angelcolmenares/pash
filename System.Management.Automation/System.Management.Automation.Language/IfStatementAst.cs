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

    public class IfStatementAst : StatementAst
    {
        public IfStatementAst(IScriptExtent extent, IEnumerable<Tuple<PipelineBaseAst, StatementBlockAst>> clauses, StatementBlockAst elseClause) : base(extent)
        {
            if ((clauses == null) || !clauses.Any<Tuple<PipelineBaseAst, StatementBlockAst>>())
            {
                throw PSTraceSource.NewArgumentException("clauses");
            }
            this.Clauses = new ReadOnlyCollection<Tuple<PipelineBaseAst, StatementBlockAst>>(clauses.ToArray<Tuple<PipelineBaseAst, StatementBlockAst>>());
            base.SetParents<PipelineBaseAst, StatementBlockAst>(this.Clauses);
            if (elseClause != null)
            {
                this.ElseClause = elseClause;
                base.SetParent(elseClause);
            }
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitIfStatement(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            foreach (PSTypeName iteratorVariable0 in from clause in this.Clauses select clause.Item2.GetInferredType(context))
            {
                yield return iteratorVariable0;
            }
            if (this.ElseClause != null)
            {
                foreach (PSTypeName iteratorVariable1 in this.ElseClause.GetInferredType(context))
                {
                    yield return iteratorVariable1;
                }
            }
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitIfStatement(this);
            switch (action)
            {
                case AstVisitAction.SkipChildren:
                    return AstVisitAction.Continue;

                case AstVisitAction.Continue:
                    foreach (Tuple<PipelineBaseAst, StatementBlockAst> tuple in this.Clauses)
                    {
                        action = tuple.Item1.InternalVisit(visitor);
                        if (action != AstVisitAction.Continue)
                        {
                            break;
                        }
                        action = tuple.Item2.InternalVisit(visitor);
                        if (action != AstVisitAction.Continue)
                        {
                            break;
                        }
                    }
                    break;
            }
            if ((action == AstVisitAction.Continue) && (this.ElseClause != null))
            {
                action = this.ElseClause.InternalVisit(visitor);
            }
            return action;
        }

        public ReadOnlyCollection<Tuple<PipelineBaseAst, StatementBlockAst>> Clauses { get; private set; }

        public StatementBlockAst ElseClause { get; private set; }

        
    }
}

