namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public class BinaryExpressionAst : ExpressionAst
    {
        internal static readonly PSTypeName[] BoolTypeNameArray = new PSTypeName[] { new PSTypeName(typeof(bool)) };

        public BinaryExpressionAst(IScriptExtent extent, ExpressionAst left, TokenKind @operator, ExpressionAst right, IScriptExtent errorPosition) : base(extent)
        {
            if ((@operator.GetTraits() & TokenFlags.BinaryOperator) == TokenFlags.None)
            {
                throw PSTraceSource.NewArgumentException("operator");
            }
            if (((left == null) || (right == null)) || (errorPosition == null))
            {
                throw PSTraceSource.NewArgumentNullException((left == null) ? "left" : ((right == null) ? "right" : "errorPosition"));
            }
            this.Left = left;
            base.SetParent(left);
            this.Operator = @operator;
            this.Right = right;
            base.SetParent(right);
            this.ErrorPosition = errorPosition;
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitBinaryExpression(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            switch (this.Operator)
            {
                case TokenKind.And:
                case TokenKind.Or:
                case TokenKind.Xor:
                case TokenKind.Is:
                    return BoolTypeNameArray;
            }
            return this.Left.GetInferredType(context);
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitBinaryExpression(this);
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

        public ExpressionAst Right { get; private set; }

        public override Type StaticType
        {
            get
            {
                switch (this.Operator)
                {
                    case TokenKind.And:
                    case TokenKind.Or:
                    case TokenKind.Xor:
                    case TokenKind.Is:
                        return typeof(bool);
                }
                return typeof(object);
            }
        }
    }
}

