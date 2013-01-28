namespace Microsoft.Data.OData.Query
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Library;
    using Microsoft.Data.OData;
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;

    [DebuggerDisplay("ExpressionLexer ({text} @ {textPos} [{token}]")]
    internal class ExpressionLexer
    {
        private char ch;
        private const char SingleSuffixLower = 'f';
        private const char SingleSuffixUpper = 'F';
        private readonly string text;
        private readonly int textLen;
        private int textPos;
        private ExpressionToken token;

        internal ExpressionLexer(string expression, bool moveToFirstToken)
        {
            this.text = expression;
            this.textLen = this.text.Length;
            this.SetTextPos(0);
            if (moveToFirstToken)
            {
                this.NextToken();
            }
        }

        private void HandleTypePrefixedLiterals()
        {
            if ((this.token.Kind == ExpressionTokenKind.Identifier) && (this.ch == '\''))
            {
                ExpressionTokenKind dateTimeLiteral;
                string text = this.token.Text;
                if (string.Equals(text, "datetime", StringComparison.OrdinalIgnoreCase))
                {
                    dateTimeLiteral = ExpressionTokenKind.DateTimeLiteral;
                }
                else if (string.Equals(text, "datetimeoffset", StringComparison.OrdinalIgnoreCase))
                {
                    dateTimeLiteral = ExpressionTokenKind.DateTimeOffsetLiteral;
                }
                else if (string.Equals(text, "time", StringComparison.OrdinalIgnoreCase))
                {
                    dateTimeLiteral = ExpressionTokenKind.TimeLiteral;
                }
                else if (string.Equals(text, "guid", StringComparison.OrdinalIgnoreCase))
                {
                    dateTimeLiteral = ExpressionTokenKind.GuidLiteral;
                }
                else if (string.Equals(text, "binary", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "X", StringComparison.OrdinalIgnoreCase))
                {
                    dateTimeLiteral = ExpressionTokenKind.BinaryLiteral;
                }
                else if (string.Equals(text, "geography", StringComparison.OrdinalIgnoreCase))
                {
                    dateTimeLiteral = ExpressionTokenKind.GeographyLiteral;
                }
                else if (string.Equals(text, "geometry", StringComparison.OrdinalIgnoreCase))
                {
                    dateTimeLiteral = ExpressionTokenKind.GeometryLiteral;
                }
                else
                {
                    return;
                }
                int position = this.token.Position;
                do
                {
                    this.NextChar();
                }
                while ((this.ch != '\0') && (this.ch != '\''));
                if (this.ch == '\0')
                {
                    throw ParseError(Microsoft.Data.OData.Strings.ExpressionLexer_UnterminatedLiteral(this.textPos, this.text));
                }
                this.NextChar();
                this.token.Kind = dateTimeLiteral;
                this.token.Text = this.text.Substring(position, this.textPos - position);
            }
        }

        private static bool IsInfinityLiteralDouble(string text)
        {
            return (string.CompareOrdinal(text, 0, "INF", 0, text.Length) == 0);
        }

        private static bool IsInfinityLiteralSingle(string text)
        {
            if ((text.Length != 4) || ((text[3] != 'f') && (text[3] != 'F')))
            {
                return false;
            }
            return (string.CompareOrdinal(text, 0, "INF", 0, 3) == 0);
        }

        private static bool IsInfinityOrNaNDouble(string tokenText)
        {
            if (tokenText.Length == 3)
            {
                if (tokenText[0] == "INF"[0])
                {
                    return IsInfinityLiteralDouble(tokenText);
                }
                if (tokenText[0] == "NaN"[0])
                {
                    return (string.CompareOrdinal(tokenText, 0, "NaN", 0, 3) == 0);
                }
            }
            return false;
        }

        private static bool IsInfinityOrNanSingle(string tokenText)
        {
            if (tokenText.Length == 4)
            {
                if (tokenText[0] == "INF"[0])
                {
                    return IsInfinityLiteralSingle(tokenText);
                }
                if (tokenText[0] == "NaN"[0])
                {
                    if ((tokenText[3] != 'f') && (tokenText[3] != 'F'))
                    {
                        return false;
                    }
                    return (string.CompareOrdinal(tokenText, 0, "NaN", 0, 3) == 0);
                }
            }
            return false;
        }

        private static bool IsLiteralType(ExpressionTokenKind tokenKind)
        {
            switch (tokenKind)
            {
                case ExpressionTokenKind.NullLiteral:
                case ExpressionTokenKind.BooleanLiteral:
                case ExpressionTokenKind.StringLiteral:
                case ExpressionTokenKind.IntegerLiteral:
                case ExpressionTokenKind.Int64Literal:
                case ExpressionTokenKind.SingleLiteral:
                case ExpressionTokenKind.DateTimeLiteral:
                case ExpressionTokenKind.DateTimeOffsetLiteral:
                case ExpressionTokenKind.TimeLiteral:
                case ExpressionTokenKind.DecimalLiteral:
                case ExpressionTokenKind.DoubleLiteral:
                case ExpressionTokenKind.GuidLiteral:
                case ExpressionTokenKind.BinaryLiteral:
                case ExpressionTokenKind.GeographyLiteral:
                case ExpressionTokenKind.GeometryLiteral:
                    return true;
            }
            return false;
        }

        internal static bool IsNumeric(ExpressionTokenKind id)
        {
            if (((id != ExpressionTokenKind.IntegerLiteral) && (id != ExpressionTokenKind.DecimalLiteral)) && ((id != ExpressionTokenKind.DoubleLiteral) && (id != ExpressionTokenKind.Int64Literal)))
            {
                return (id == ExpressionTokenKind.SingleLiteral);
            }
            return true;
        }

        private void NextChar()
        {
            if (this.textPos < this.textLen)
            {
                this.textPos++;
            }
            this.ch = (this.textPos < this.textLen) ? this.text[this.textPos] : '\0';
        }

        internal ExpressionToken NextToken()
        {
            Exception error = null;
            ExpressionToken token = this.NextTokenImplementation(out error);
            if (error != null)
            {
                throw error;
            }
            return token;
        }

        private ExpressionToken NextTokenImplementation(out Exception error)
        {
            ExpressionTokenKind minus;
            error = null;
            while (char.IsWhiteSpace(this.ch))
            {
                this.NextChar();
            }
            int textPos = this.textPos;
            switch (this.ch)
            {
                case '\'':
                {
                    char ch = this.ch;
                    do
                    {
                        this.NextChar();
                        while ((this.textPos < this.textLen) && (this.ch != ch))
                        {
                            this.NextChar();
                        }
                        if (this.textPos == this.textLen)
                        {
                            error = ParseError(Microsoft.Data.OData.Strings.ExpressionLexer_UnterminatedStringLiteral(this.textPos, this.text));
                        }
                        this.NextChar();
                    }
                    while (this.ch == ch);
                    minus = ExpressionTokenKind.StringLiteral;
                    goto Label_0283;
                }
                case '(':
                    this.NextChar();
                    minus = ExpressionTokenKind.OpenParen;
                    goto Label_0283;

                case ')':
                    this.NextChar();
                    minus = ExpressionTokenKind.CloseParen;
                    goto Label_0283;

                case '*':
                    this.NextChar();
                    minus = ExpressionTokenKind.Star;
                    goto Label_0283;

                case ',':
                    this.NextChar();
                    minus = ExpressionTokenKind.Comma;
                    goto Label_0283;

                case '-':
                {
                    bool flag = (this.textPos + 1) < this.textLen;
                    if (!flag || !char.IsDigit(this.text[this.textPos + 1]))
                    {
                        if (flag && (this.text[textPos + 1] == "INF"[0]))
                        {
                            this.NextChar();
                            this.ParseIdentifier();
                            string text = this.text.Substring(textPos + 1, (this.textPos - textPos) - 1);
                            if (IsInfinityLiteralDouble(text))
                            {
                                minus = ExpressionTokenKind.DoubleLiteral;
                                goto Label_0283;
                            }
                            if (IsInfinityLiteralSingle(text))
                            {
                                minus = ExpressionTokenKind.SingleLiteral;
                                goto Label_0283;
                            }
                            this.SetTextPos(textPos);
                        }
                        break;
                    }
                    this.NextChar();
                    minus = this.ParseFromDigit();
                    if (IsNumeric(minus))
                    {
                        goto Label_0283;
                    }
                    this.SetTextPos(textPos);
                    break;
                }
                case '.':
                    this.NextChar();
                    minus = ExpressionTokenKind.Dot;
                    goto Label_0283;

                case '/':
                    this.NextChar();
                    minus = ExpressionTokenKind.Slash;
                    goto Label_0283;

                case '=':
                    this.NextChar();
                    minus = ExpressionTokenKind.Equal;
                    goto Label_0283;

                case '?':
                    this.NextChar();
                    minus = ExpressionTokenKind.Question;
                    goto Label_0283;

                default:
                    if (char.IsLetter(this.ch) || (this.ch == '_'))
                    {
                        this.ParseIdentifier();
                        minus = ExpressionTokenKind.Identifier;
                    }
                    else if (char.IsDigit(this.ch))
                    {
                        minus = this.ParseFromDigit();
                    }
                    else if (this.textPos == this.textLen)
                    {
                        minus = ExpressionTokenKind.End;
                    }
                    else
                    {
                        error = ParseError(Microsoft.Data.OData.Strings.ExpressionLexer_InvalidCharacter(this.ch, this.textPos, this.text));
                        minus = ExpressionTokenKind.Unknown;
                    }
                    goto Label_0283;
            }
            this.NextChar();
            minus = ExpressionTokenKind.Minus;
        Label_0283:
            this.token.Kind = minus;
            this.token.Text = this.text.Substring(textPos, this.textPos - textPos);
            this.token.Position = textPos;
            this.HandleTypePrefixedLiterals();
            if (this.token.Kind == ExpressionTokenKind.Identifier)
            {
                if (IsInfinityOrNaNDouble(this.token.Text))
                {
                    this.token.Kind = ExpressionTokenKind.DoubleLiteral;
                }
                else if (IsInfinityOrNanSingle(this.token.Text))
                {
                    this.token.Kind = ExpressionTokenKind.SingleLiteral;
                }
                else if ((this.token.Text == "true") || (this.token.Text == "false"))
                {
                    this.token.Kind = ExpressionTokenKind.BooleanLiteral;
                }
                else if (this.token.Text == "null")
                {
                    this.token.Kind = ExpressionTokenKind.NullLiteral;
                }
            }
            return this.token;
        }

        internal static Exception ParseError(string message)
        {
            return new ODataException(message);
        }

        private ExpressionTokenKind ParseFromDigit()
        {
            ExpressionTokenKind integerLiteral;
            char ch = this.ch;
            this.NextChar();
            if (((ch == '0') && (this.ch == 'x')) || (this.ch == 'X'))
            {
                integerLiteral = ExpressionTokenKind.BinaryLiteral;
                do
                {
                    this.NextChar();
                }
                while (UriPrimitiveTypeParser.IsCharHexDigit(this.ch));
                return integerLiteral;
            }
            integerLiteral = ExpressionTokenKind.IntegerLiteral;
            while (char.IsDigit(this.ch))
            {
                this.NextChar();
            }
            if (this.ch == '.')
            {
                integerLiteral = ExpressionTokenKind.DoubleLiteral;
                this.NextChar();
                this.ValidateDigit();
                do
                {
                    this.NextChar();
                }
                while (char.IsDigit(this.ch));
            }
            if ((this.ch == 'E') || (this.ch == 'e'))
            {
                integerLiteral = ExpressionTokenKind.DoubleLiteral;
                this.NextChar();
                if ((this.ch == '+') || (this.ch == '-'))
                {
                    this.NextChar();
                }
                this.ValidateDigit();
                do
                {
                    this.NextChar();
                }
                while (char.IsDigit(this.ch));
            }
            if ((this.ch == 'M') || (this.ch == 'm'))
            {
                integerLiteral = ExpressionTokenKind.DecimalLiteral;
                this.NextChar();
                return integerLiteral;
            }
            if ((this.ch == 'd') || (this.ch == 'D'))
            {
                integerLiteral = ExpressionTokenKind.DoubleLiteral;
                this.NextChar();
                return integerLiteral;
            }
            if ((this.ch == 'L') || (this.ch == 'l'))
            {
                integerLiteral = ExpressionTokenKind.Int64Literal;
                this.NextChar();
                return integerLiteral;
            }
            if ((this.ch == 'f') || (this.ch == 'F'))
            {
                integerLiteral = ExpressionTokenKind.SingleLiteral;
                this.NextChar();
            }
            return integerLiteral;
        }

        private void ParseIdentifier()
        {
            do
            {
                this.NextChar();
            }
            while (char.IsLetterOrDigit(this.ch) || (this.ch == '_'));
        }

        private object ParseNullLiteral()
        {
            this.NextToken();
            ODataUriNullValue value2 = new ODataUriNullValue();
            if (this.ExpressionText != "null")
            {
                int num = ("'".Length * 2) + "null".Length;
                int startIndex = "'".Length + "null".Length;
                value2.TypeName = this.ExpressionText.Substring(startIndex, this.ExpressionText.Length - num);
            }
            return value2;
        }

        private object ParseTypedLiteral(IEdmPrimitiveTypeReference targetTypeReference)
        {
            object obj2;
            if (!UriPrimitiveTypeParser.TryUriStringToPrimitive(this.CurrentToken.Text, targetTypeReference, out obj2))
            {
                throw ParseError(Microsoft.Data.OData.Strings.UriQueryExpressionParser_UnrecognizedLiteral(targetTypeReference.FullName(), this.CurrentToken.Text, this.CurrentToken.Position, this.ExpressionText));
            }
            this.NextToken();
            return obj2;
        }

        internal ExpressionToken PeekNextToken()
        {
            ExpressionToken token;
            Exception exception;
            this.TryPeekNextToken(out token, out exception);
            if (exception != null)
            {
                throw exception;
            }
            return token;
        }

        internal string ReadDottedIdentifier()
        {
            this.ValidateToken(ExpressionTokenKind.Identifier);
            StringBuilder builder = null;
            string text = this.CurrentToken.Text;
            this.NextToken();
            while (this.CurrentToken.Kind == ExpressionTokenKind.Dot)
            {
                this.NextToken();
                this.ValidateToken(ExpressionTokenKind.Identifier);
                if (builder == null)
                {
                    builder = new StringBuilder(text, (text.Length + 1) + this.CurrentToken.Text.Length);
                }
                builder.Append('.');
                builder.Append(this.CurrentToken.Text);
                this.NextToken();
            }
            if (builder != null)
            {
                text = builder.ToString();
            }
            return text;
        }

        internal object ReadLiteralToken()
        {
            this.NextToken();
            if (!IsLiteralType(this.CurrentToken.Kind))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ExpressionLexer_ExpectedLiteralToken(this.CurrentToken.Text));
            }
            return this.TryParseLiteral();
        }

        private void SetTextPos(int pos)
        {
            this.textPos = pos;
            this.ch = (this.textPos < this.textLen) ? this.text[this.textPos] : '\0';
        }

        private object TryParseLiteral()
        {
            switch (this.CurrentToken.Kind)
            {
                case ExpressionTokenKind.NullLiteral:
                    return this.ParseNullLiteral();

                case ExpressionTokenKind.BooleanLiteral:
                    return this.ParseTypedLiteral(EdmCoreModel.Instance.GetBoolean(false));

                case ExpressionTokenKind.StringLiteral:
                    return this.ParseTypedLiteral(EdmCoreModel.Instance.GetString(true));

                case ExpressionTokenKind.IntegerLiteral:
                    return this.ParseTypedLiteral(EdmCoreModel.Instance.GetInt32(false));

                case ExpressionTokenKind.Int64Literal:
                    return this.ParseTypedLiteral(EdmCoreModel.Instance.GetInt64(false));

                case ExpressionTokenKind.SingleLiteral:
                    return this.ParseTypedLiteral(EdmCoreModel.Instance.GetSingle(false));

                case ExpressionTokenKind.DateTimeLiteral:
                    return this.ParseTypedLiteral(EdmCoreModel.Instance.GetTemporal(EdmPrimitiveTypeKind.DateTime, false));

                case ExpressionTokenKind.DateTimeOffsetLiteral:
                    return this.ParseTypedLiteral(EdmCoreModel.Instance.GetDateTimeOffset(false));

                case ExpressionTokenKind.TimeLiteral:
                    return this.ParseTypedLiteral(EdmCoreModel.Instance.GetTemporal(EdmPrimitiveTypeKind.Time, false));

                case ExpressionTokenKind.DecimalLiteral:
                    return this.ParseTypedLiteral(EdmCoreModel.Instance.GetDecimal(false));

                case ExpressionTokenKind.DoubleLiteral:
                    return this.ParseTypedLiteral(EdmCoreModel.Instance.GetDouble(false));

                case ExpressionTokenKind.GuidLiteral:
                    return this.ParseTypedLiteral(EdmCoreModel.Instance.GetGuid(false));

                case ExpressionTokenKind.BinaryLiteral:
                    return this.ParseTypedLiteral(EdmCoreModel.Instance.GetBinary(true));

                case ExpressionTokenKind.GeographyLiteral:
                    return this.ParseTypedLiteral(EdmCoreModel.Instance.GetSpatial(EdmPrimitiveTypeKind.Geography, false));

                case ExpressionTokenKind.GeometryLiteral:
                    return this.ParseTypedLiteral(EdmCoreModel.Instance.GetSpatial(EdmPrimitiveTypeKind.Geometry, false));
            }
            return null;
        }

        internal bool TryPeekNextToken(out ExpressionToken resultToken, out Exception error)
        {
            int textPos = this.textPos;
            char ch = this.ch;
            ExpressionToken token = this.token;
            resultToken = this.NextTokenImplementation(out error);
            this.textPos = textPos;
            this.ch = ch;
            this.token = token;
            return (error == null);
        }

        private void ValidateDigit()
        {
            if (!char.IsDigit(this.ch))
            {
                throw ParseError(Microsoft.Data.OData.Strings.ExpressionLexer_DigitExpected(this.textPos, this.text));
            }
        }

        internal void ValidateToken(ExpressionTokenKind t)
        {
            if (this.token.Kind != t)
            {
                throw ParseError(Microsoft.Data.OData.Strings.ExpressionLexer_SyntaxError(this.textPos, this.text));
            }
        }

        internal ExpressionToken CurrentToken
        {
            get
            {
                return this.token;
            }
            set
            {
                this.token = value;
            }
        }

        internal string ExpressionText
        {
            get
            {
                return this.text;
            }
        }

        internal int Position
        {
            get
            {
                return this.token.Position;
            }
        }
    }
}

