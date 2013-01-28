namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public class CatchClauseAst : Ast
    {
        private static readonly ReadOnlyCollection<TypeConstraintAst> EmptyCatchTypes = new ReadOnlyCollection<TypeConstraintAst>(new TypeConstraintAst[0]);

        public CatchClauseAst(IScriptExtent extent, IEnumerable<TypeConstraintAst> catchTypes, StatementBlockAst body) : base(extent)
        {
            if (body == null)
            {
                throw PSTraceSource.NewArgumentNullException("body");
            }
            if ((catchTypes != null) && catchTypes.Any<TypeConstraintAst>())
            {
                this.CatchTypes = new ReadOnlyCollection<TypeConstraintAst>(catchTypes.ToArray<TypeConstraintAst>());
                base.SetParents((IEnumerable<Ast>) this.CatchTypes);
            }
            else
            {
                this.CatchTypes = EmptyCatchTypes;
            }
            this.Body = body;
            base.SetParent(body);
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitCatchClause(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            return this.Body.GetInferredType(context);
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitCatchClause(this);
            if (action == AstVisitAction.SkipChildren)
            {
                return AstVisitAction.Continue;
            }
            foreach (TypeConstraintAst ast in this.CatchTypes)
            {
                if (action != AstVisitAction.Continue)
                {
                    break;
                }
                action = ast.InternalVisit(visitor);
            }
            if (action == AstVisitAction.Continue)
            {
                action = this.Body.InternalVisit(visitor);
            }
            return action;
        }

        public StatementBlockAst Body { get; private set; }

        public ReadOnlyCollection<TypeConstraintAst> CatchTypes { get; private set; }

        public bool IsCatchAll
        {
            get
            {
                return (this.CatchTypes.Count == 0);
            }
        }
    }
}

