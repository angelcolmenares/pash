namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public class UnaryExpressionAst : ExpressionAst
    {
        public UnaryExpressionAst(IScriptExtent extent, System.Management.Automation.Language.TokenKind tokenKind, ExpressionAst child) : base(extent)
        {
            if ((tokenKind.GetTraits() & TokenFlags.UnaryOperator) == TokenFlags.None)
            {
                throw PSTraceSource.NewArgumentException("tokenKind");
            }
            if (child == null)
            {
                throw PSTraceSource.NewArgumentNullException("child");
            }
            this.TokenKind = tokenKind;
            this.Child = child;
            base.SetParent(child);
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitUnaryExpression(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            if ((this.TokenKind != System.Management.Automation.Language.TokenKind.Not) && (this.TokenKind != System.Management.Automation.Language.TokenKind.Exclaim))
            {
                return this.Child.GetInferredType(context);
            }
            return BinaryExpressionAst.BoolTypeNameArray;
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitUnaryExpression(this);
            switch (action)
            {
                case AstVisitAction.SkipChildren:
                    return AstVisitAction.Continue;

                case AstVisitAction.Continue:
                    action = this.Child.InternalVisit(visitor);
                    break;
            }
            return action;
        }

        public ExpressionAst Child { get; private set; }

        public override Type StaticType
        {
            get
            {
                if ((this.TokenKind != System.Management.Automation.Language.TokenKind.Not) && (this.TokenKind != System.Management.Automation.Language.TokenKind.Exclaim))
                {
                    return typeof(object);
                }
                return typeof(bool);
            }
        }

        public System.Management.Automation.Language.TokenKind TokenKind { get; private set; }
    }
}

