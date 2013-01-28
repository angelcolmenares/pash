namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public class InvokeMemberExpressionAst : MemberExpressionAst, ISupportsAssignment
    {
        public InvokeMemberExpressionAst(IScriptExtent extent, ExpressionAst expression, CommandElementAst method, IEnumerable<ExpressionAst> arguments, bool @static) : base(extent, expression, method, @static)
        {
            if ((arguments != null) && arguments.Any<ExpressionAst>())
            {
                this.Arguments = new ReadOnlyCollection<ExpressionAst>(arguments.ToArray<ExpressionAst>());
                base.SetParents((IEnumerable<Ast>) this.Arguments);
            }
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitInvokeMemberExpression(this);
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitInvokeMemberExpression(this);
            switch (action)
            {
                case AstVisitAction.SkipChildren:
                    return AstVisitAction.Continue;

                case AstVisitAction.Continue:
                    action = base.Expression.InternalVisit(visitor);
                    break;
            }
            if (action == AstVisitAction.Continue)
            {
                action = base.Member.InternalVisit(visitor);
            }
            if ((action == AstVisitAction.Continue) && (this.Arguments != null))
            {
                foreach (ExpressionAst ast in this.Arguments)
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

        IAssignableValue ISupportsAssignment.GetAssignableValue()
        {
            return new InvokeMemberAssignableValue { InvokeMemberExpressionAst = this };
        }

        public ReadOnlyCollection<ExpressionAst> Arguments { get; private set; }
    }
}

