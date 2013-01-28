namespace Microsoft.Data.OData.Query
{
    using Microsoft.Data.OData;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), DebuggerDisplay("{Kind} @ {Position}: [{Text}]")]
    internal struct ExpressionToken
    {
        internal static readonly ExpressionToken GreaterThan;
        internal static readonly ExpressionToken EqualsTo;
        internal static readonly ExpressionToken LessThan;
        internal ExpressionTokenKind Kind;
        internal string Text;
        internal int Position;
        internal bool IsComparisonOperator
        {
            get
            {
                if (this.Kind != ExpressionTokenKind.Identifier)
                {
                    return false;
                }
                if (((!(this.Text == "eq") && !(this.Text == "ne")) && (!(this.Text == "lt") && !(this.Text == "gt"))) && !(this.Text == "le"))
                {
                    return (this.Text == "ge");
                }
                return true;
            }
        }
        internal bool IsEqualityOperator
        {
            get
            {
                if (this.Kind != ExpressionTokenKind.Identifier)
                {
                    return false;
                }
                if (!(this.Text == "eq"))
                {
                    return (this.Text == "ne");
                }
                return true;
            }
        }
        internal bool IsKeyValueToken
        {
            get
            {
                if (((((this.Kind != ExpressionTokenKind.BinaryLiteral) && (this.Kind != ExpressionTokenKind.BooleanLiteral)) && ((this.Kind != ExpressionTokenKind.DateTimeLiteral) && (this.Kind != ExpressionTokenKind.DateTimeOffsetLiteral))) && (((this.Kind != ExpressionTokenKind.TimeLiteral) && (this.Kind != ExpressionTokenKind.GuidLiteral)) && ((this.Kind != ExpressionTokenKind.StringLiteral) && (this.Kind != ExpressionTokenKind.GeographyLiteral)))) && (this.Kind != ExpressionTokenKind.GeometryLiteral))
                {
                    return ExpressionLexer.IsNumeric(this.Kind);
                }
                return true;
            }
        }
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} @ {1}: [{2}]", new object[] { this.Kind, this.Position, this.Text });
        }

        internal string GetIdentifier()
        {
            if (this.Kind != ExpressionTokenKind.Identifier)
            {
                throw ExpressionLexer.ParseError(Strings.ExpressionToken_IdentifierExpected(this.Position));
            }
            return this.Text;
        }

        internal bool IdentifierIs(string id)
        {
            return ((this.Kind == ExpressionTokenKind.Identifier) && (this.Text == id));
        }

        static ExpressionToken()
        {
            ExpressionToken token = new ExpressionToken {
                Text = "gt",
                Kind = ExpressionTokenKind.Identifier,
                Position = 0
            };
            GreaterThan = token;
            ExpressionToken token2 = new ExpressionToken {
                Text = "eq",
                Kind = ExpressionTokenKind.Identifier,
                Position = 0
            };
            EqualsTo = token2;
            ExpressionToken token3 = new ExpressionToken {
                Text = "lt",
                Kind = ExpressionTokenKind.Identifier,
                Position = 0
            };
            LessThan = token3;
        }
    }
}

