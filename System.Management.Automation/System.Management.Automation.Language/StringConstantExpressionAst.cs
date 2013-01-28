namespace System.Management.Automation.Language
{
    using System;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public class StringConstantExpressionAst : ConstantExpressionAst
    {
        internal StringConstantExpressionAst(StringToken token) : base(token.Extent, token.Value)
        {
            this.StringConstantType = MapTokenKindToStringContantKind(token);
        }

        public StringConstantExpressionAst(IScriptExtent extent, string value, System.Management.Automation.Language.StringConstantType stringConstantType) : base(extent, value)
        {
            if (value == null)
            {
                throw PSTraceSource.NewArgumentNullException("value");
            }
            this.StringConstantType = stringConstantType;
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitStringConstantExpression(this);
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitStringConstantExpression(this);
            if (action != AstVisitAction.SkipChildren)
            {
                return action;
            }
            return AstVisitAction.Continue;
        }

        internal static System.Management.Automation.Language.StringConstantType MapTokenKindToStringContantKind(Token token)
        {
            switch (token.Kind)
            {
                case TokenKind.Generic:
                    return System.Management.Automation.Language.StringConstantType.BareWord;

                case TokenKind.StringLiteral:
                    return System.Management.Automation.Language.StringConstantType.SingleQuoted;

                case TokenKind.StringExpandable:
                    return System.Management.Automation.Language.StringConstantType.DoubleQuoted;

                case TokenKind.HereStringLiteral:
                    return System.Management.Automation.Language.StringConstantType.SingleQuotedHereString;

                case TokenKind.HereStringExpandable:
                    return System.Management.Automation.Language.StringConstantType.DoubleQuotedHereString;
            }
            throw PSTraceSource.NewInvalidOperationException();
        }

        public override Type StaticType
        {
            get
            {
                return typeof(string);
            }
        }

        public System.Management.Automation.Language.StringConstantType StringConstantType { get; private set; }

        public string Value
        {
            get
            {
                return (string) base.Value;
            }
        }
    }
}

