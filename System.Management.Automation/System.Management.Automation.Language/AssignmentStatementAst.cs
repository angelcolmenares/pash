namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public class AssignmentStatementAst : PipelineBaseAst
    {
        public AssignmentStatementAst(IScriptExtent extent, ExpressionAst left, TokenKind @operator, StatementAst right, IScriptExtent errorPosition) : base(extent)
        {
            if (((left == null) || (right == null)) || (errorPosition == null))
            {
                throw PSTraceSource.NewArgumentNullException((left == null) ? "left" : ((right == null) ? "right" : "errorPosition"));
            }
            if ((@operator.GetTraits() & TokenFlags.AssignmentOperator) == TokenFlags.None)
            {
                throw PSTraceSource.NewArgumentException("operator");
            }
            PipelineAst ast = right as PipelineAst;
            if ((ast != null) && (ast.PipelineElements.Count == 1))
            {
                CommandExpressionAst ast2 = ast.PipelineElements[0] as CommandExpressionAst;
                if (ast2 != null)
                {
                    right = ast2;
                    right.ClearParent();
                }
            }
            this.Operator = @operator;
            this.Left = left;
            base.SetParent(left);
            this.Right = right;
            base.SetParent(right);
            this.ErrorPosition = errorPosition;
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitAssignmentStatement(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            return this.Left.GetInferredType(context);
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitAssignmentStatement(this);
            switch (action)
            {
                case AstVisitAction.SkipChildren:
                    return AstVisitAction.Continue;

                case AstVisitAction.Continue:
                    action = this.Left.InternalVisit(visitor);
                    break;
            }
            if (action == AstVisitAction.Continue)
            {
                action = this.Right.InternalVisit(visitor);
            }
            return action;
        }

        public IScriptExtent ErrorPosition { get; private set; }

        public ExpressionAst Left { get; private set; }

        public TokenKind Operator { get; private set; }

        public StatementAst Right { get; private set; }
    }
}

