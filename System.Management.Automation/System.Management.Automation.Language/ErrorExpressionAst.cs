namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class ErrorExpressionAst : ExpressionAst
    {
        internal ErrorExpressionAst(IScriptExtent extent, IEnumerable<Ast> nestedAsts = null) : base(extent)
        {
            if ((nestedAsts != null) && nestedAsts.Any<Ast>())
            {
                this.NestedAst = new ReadOnlyCollection<Ast>(nestedAsts.ToArray<Ast>());
                base.SetParents(this.NestedAst);
            }
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitErrorExpression(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            return this.NestedAst.SelectMany(x => x.GetInferredType(context));
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitErrorExpression(this);
            if (action == AstVisitAction.SkipChildren)
            {
                return AstVisitAction.Continue;
            }
            if ((action == AstVisitAction.Continue) && (this.NestedAst != null))
            {
                foreach (Ast ast in this.NestedAst)
                {
                    action = ast.InternalVisit(visitor);
                    if (action != AstVisitAction.Continue)
                    {
                        return action;
                    }
                }
            }
            return action;
        }

        public ReadOnlyCollection<Ast> NestedAst { get; private set; }
    }
}

