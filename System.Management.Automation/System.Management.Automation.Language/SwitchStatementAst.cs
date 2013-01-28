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

    public class SwitchStatementAst : LabeledStatementAst
    {
        private static readonly Tuple<ExpressionAst, StatementBlockAst>[] EmptyClauseArray = new Tuple<ExpressionAst, StatementBlockAst>[0];

        public SwitchStatementAst(IScriptExtent extent, string label, PipelineBaseAst condition, SwitchFlags flags, IEnumerable<Tuple<ExpressionAst, StatementBlockAst>> clauses, StatementBlockAst @default) : base(extent, label, condition)
        {
            if (((clauses == null) || !clauses.Any<Tuple<ExpressionAst, StatementBlockAst>>()) && (@default == null))
            {
                throw PSTraceSource.NewArgumentException("clauses");
            }
            this.Flags = flags;
            this.Clauses = new ReadOnlyCollection<Tuple<ExpressionAst, StatementBlockAst>>(((clauses != null) && clauses.Any<Tuple<ExpressionAst, StatementBlockAst>>()) ? ((IList<Tuple<ExpressionAst, StatementBlockAst>>) clauses.ToArray<Tuple<ExpressionAst, StatementBlockAst>>()) : ((IList<Tuple<ExpressionAst, StatementBlockAst>>) EmptyClauseArray));
            base.SetParents<ExpressionAst, StatementBlockAst>(this.Clauses);
            if (@default != null)
            {
                this.Default = @default;
                base.SetParent(@default);
            }
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitSwitchStatement(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            foreach (PSTypeName iteratorVariable0 in from clause in this.Clauses select clause.Item2.GetInferredType(context))
            {
                yield return iteratorVariable0;
            }
            if (this.Default != null)
            {
                foreach (PSTypeName iteratorVariable1 in this.Default.GetInferredType(context))
                {
                    yield return iteratorVariable1;
                }
            }
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitSwitchStatement(this);
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
                foreach (Tuple<ExpressionAst, StatementBlockAst> tuple in this.Clauses)
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
            }
            if ((action == AstVisitAction.Continue) && (this.Default != null))
            {
                action = this.Default.InternalVisit(visitor);
            }
            return action;
        }

        public ReadOnlyCollection<Tuple<ExpressionAst, StatementBlockAst>> Clauses { get; private set; }

        public StatementBlockAst Default { get; private set; }

        public SwitchFlags Flags { get; private set; }

        
    }
}

