namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public class CommandExpressionAst : CommandBaseAst
    {
        public CommandExpressionAst(IScriptExtent extent, ExpressionAst expression, IEnumerable<RedirectionAst> redirections) : base(extent, redirections)
        {
            if (expression == null)
            {
                throw PSTraceSource.NewArgumentNullException("expression");
            }
            this.Expression = expression;
            base.SetParent(expression);
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitCommandExpression(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            return this.Expression.GetInferredType(context);
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitCommandExpression(this);
            switch (action)
            {
                case AstVisitAction.SkipChildren:
                    return AstVisitAction.Continue;

                case AstVisitAction.Continue:
                    action = this.Expression.InternalVisit(visitor);
                    break;
            }
            if (action == AstVisitAction.Continue)
            {
                foreach (RedirectionAst ast in base.Redirections)
                {
                    if (action == AstVisitAction.Continue)
                    {
                        action = ast.InternalVisit(visitor);
                    }
                }
            }
            return action;
        }

        public ExpressionAst Expression { get; private set; }
    }
}

