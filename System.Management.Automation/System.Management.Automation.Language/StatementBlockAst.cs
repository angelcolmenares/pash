namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public class StatementBlockAst : Ast
    {
        public StatementBlockAst(IScriptExtent extent, IEnumerable<StatementAst> statements, IEnumerable<TrapStatementAst> traps) : base(extent)
        {
            if (statements == null)
            {
                throw PSTraceSource.NewArgumentNullException("statements");
            }
            this.Statements = new ReadOnlyCollection<StatementAst>(statements.ToArray<StatementAst>());
            base.SetParents((IEnumerable<Ast>) this.Statements);
            if ((traps != null) && traps.Any<TrapStatementAst>())
            {
                this.Traps = new ReadOnlyCollection<TrapStatementAst>(traps.ToArray<TrapStatementAst>());
                base.SetParents((IEnumerable<Ast>) this.Traps);
            }
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitStatementBlock(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            return this.Statements.SelectMany(x => x.GetInferredType(context));
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitStatementBlock(this);
            return InternalVisit(visitor, this.Traps, this.Statements, action);
        }

        internal static AstVisitAction InternalVisit(AstVisitor visitor, ReadOnlyCollection<TrapStatementAst> traps, ReadOnlyCollection<StatementAst> statements, AstVisitAction action)
        {
            if (action == AstVisitAction.SkipChildren)
            {
                return AstVisitAction.Continue;
            }
            if ((action == AstVisitAction.Continue) && (traps != null))
            {
                foreach (TrapStatementAst ast in traps)
                {
                    action = ast.InternalVisit(visitor);
                    if (action != AstVisitAction.Continue)
                    {
                        break;
                    }
                }
            }
            if ((action == AstVisitAction.Continue) && (statements != null))
            {
                foreach (StatementAst ast2 in statements)
                {
                    action = ast2.InternalVisit(visitor);
                    if (action != AstVisitAction.Continue)
                    {
                        return action;
                    }
                }
            }
            return action;
        }

        public ReadOnlyCollection<StatementAst> Statements { get; private set; }

        public ReadOnlyCollection<TrapStatementAst> Traps { get; private set; }
    }
}

