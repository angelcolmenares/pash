namespace System.Data.Services.Parsing
{
    using System;
    using System.Data.Services;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), DebuggerDisplay("{Id} @ {Position}: [{Text}]")]
    internal struct Token
    {
        internal static readonly Token GreaterThan;
        internal static readonly Token EqualsTo;
        internal static readonly Token LessThan;
        internal TokenId Id;
        internal string Text;
        internal int Position;
        internal bool IsComparisonOperator
        {
            get
            {
                if (this.Id != TokenId.Identifier)
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
                if (this.Id != TokenId.Identifier)
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
                if ((((this.Id != TokenId.BinaryLiteral) && (this.Id != TokenId.BooleanLiteral)) && ((this.Id != TokenId.DateTimeLiteral) && (this.Id != TokenId.GuidLiteral))) && (((this.Id != TokenId.DateTimeOffsetLiteral) && (this.Id != TokenId.TimeLiteral)) && (this.Id != TokenId.StringLiteral)))
                {
                    return ExpressionLexer.IsNumeric(this.Id);
                }
                return true;
            }
        }
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} @ {1}: [{2}]", new object[] { this.Id, this.Position, this.Text });
        }

        internal string GetIdentifier()
        {
            if (this.Id != TokenId.Identifier)
            {
                throw DataServiceException.CreateSyntaxError(Strings.RequestQueryParser_IdentifierExpected(this.Position));
            }
            return this.Text;
        }

        internal bool IdentifierIs(string id)
        {
            return ((this.Id == TokenId.Identifier) && (this.Text == id));
        }

        static Token()
        {
            Token token = new Token {
                Text = "gt",
                Id = TokenId.Identifier,
                Position = 0
            };
            GreaterThan = token;
            Token token2 = new Token {
                Text = "eq",
                Id = TokenId.Identifier,
                Position = 0
            };
            EqualsTo = token2;
            Token token3 = new Token {
                Text = "lt",
                Id = TokenId.Identifier,
                Position = 0
            };
            LessThan = token3;
        }
    }
}

