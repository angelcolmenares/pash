namespace System.Data.Services.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;

    [DebuggerDisplay("ExpressionLexer ({text} @ {textPos} [{token}]")]
    internal class ExpressionLexer
    {
		private static readonly HashSet<UnicodeCategory> AdditionalUnicodeCategoriesForIdentifier = new HashSet<UnicodeCategory>(new UnicodeCategoryEqualityComparer()) { (UnicodeCategory)9, (UnicodeCategory)5, (UnicodeCategory)6, (UnicodeCategory)0x12, (UnicodeCategory)15 };
        private char ch;
        private bool ignoreWhitespace = true;
        private const char SingleSuffixLower = 'f';
        private const char SingleSuffixUpper = 'F';
        private readonly string text;
        private readonly int textLen;
        private int textPos;
        private Token token;

        internal ExpressionLexer(string expression)
        {
            this.text = expression;
            this.textLen = this.text.Length;
            this.SetTextPos(0);
            this.NextToken();
        }

        private void ExpandIdentifier(Func<bool> expander, bool ignoreWs)
        {
            if (this.token.Id == TokenId.Identifier)
            {
                int textPos = this.textPos;
                char ch = this.ch;
                Token token = this.token;
                bool ignoreWhitespace = this.ignoreWhitespace;
                this.ignoreWhitespace = ignoreWs;
                int position = this.token.Position;
                if (expander())
                {
                    this.token.Text = this.text.Substring(position, this.textPos - position);
                    this.token.Position = position;
                }
                else
                {
                    this.textPos = textPos;
                    this.ch = ch;
                    this.token = token;
                }
                this.ignoreWhitespace = ignoreWhitespace;
            }
        }

        internal bool ExpandIdentifierAsFunction()
        {
            this.ExpandIdentifier(delegate {
                do
                {
                }
                while (this.ExpandWhenMatch(new TokenId[] { TokenId.Dot }) && this.ExpandWhenMatch(new TokenId[] { TokenId.Identifier }));
                return (this.CurrentToken.Id == TokenId.Identifier) && (this.PeekNextToken().Id == TokenId.OpenParen);
            }, false);
            return false;
        }

        private bool ExpandWhenMatch(params TokenId[] id)
        {
            Token token = this.PeekNextToken();
            foreach (TokenId id2 in id)
            {
                if (id2 == token.Id)
                {
                    this.NextToken();
                    return true;
                }
            }
            return false;
        }

        private void HandleTypePrefixedLiterals()
        {
            if ((this.token.Id == TokenId.Identifier) && (this.ch == '\''))
            {
                TokenId dateTimeLiteral;
                string text = this.token.Text;
                if (string.Equals(text, "datetime", StringComparison.OrdinalIgnoreCase))
                {
                    dateTimeLiteral = TokenId.DateTimeLiteral;
                }
                else if (string.Equals(text, "guid", StringComparison.OrdinalIgnoreCase))
                {
                    dateTimeLiteral = TokenId.GuidLiteral;
                }
                else if ((string.Equals(text, "binary", StringComparison.OrdinalIgnoreCase) || (text == "X")) || (text == "x"))
                {
                    dateTimeLiteral = TokenId.BinaryLiteral;
                }
                else if (string.Equals(text, "geography", StringComparison.OrdinalIgnoreCase))
                {
                    dateTimeLiteral = TokenId.GeographylLiteral;
                }
                else if (string.Equals(text, "geometry", StringComparison.OrdinalIgnoreCase))
                {
                    dateTimeLiteral = TokenId.GeometryLiteral;
                }
                else if (string.Equals(text, "time", StringComparison.OrdinalIgnoreCase))
                {
                    dateTimeLiteral = TokenId.TimeLiteral;
                }
                else if (string.Equals(text, "datetimeoffset", StringComparison.OrdinalIgnoreCase))
                {
                    dateTimeLiteral = TokenId.DateTimeOffsetLiteral;
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
                    throw ParseError(Strings.RequestQueryParser_UnterminatedLiteral(this.textPos, this.text));
                }
                this.NextChar();
                this.token.Id = dateTimeLiteral;
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

        internal static bool IsNumeric(TokenId id)
        {
            if (((id != TokenId.IntegerLiteral) && (id != TokenId.DecimalLiteral)) && ((id != TokenId.DoubleLiteral) && (id != TokenId.Int64Literal)))
            {
                return (id == TokenId.SingleLiteral);
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

        internal void NextToken()
        {
            TokenId minus;
            if (this.ignoreWhitespace)
            {
                this.ParseWhitespace();
            }
            int textPos = this.textPos;
            switch (this.ch)
            {
                case '$':
                    this.NextChar();
                    if (this.ch != 'i')
                    {
                        goto Label_0279;
                    }
                    this.ParseIdentifier();
                    if (((this.textPos - textPos) != 3) || (this.text[textPos + 2] != 't'))
                    {
                        goto Label_0279;
                    }
                    minus = TokenId.Identifier;
                    goto Label_0304;

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
                            throw ParseError(Strings.RequestQueryParser_UnterminatedStringLiteral(this.textPos, this.text));
                        }
                        this.NextChar();
                    }
                    while (this.ch == ch);
                    minus = TokenId.StringLiteral;
                    goto Label_0304;
                }
                case '(':
                    this.NextChar();
                    minus = TokenId.OpenParen;
                    goto Label_0304;

                case ')':
                    this.NextChar();
                    minus = TokenId.CloseParen;
                    goto Label_0304;

                case '*':
                    this.NextChar();
                    minus = TokenId.Star;
                    goto Label_0304;

                case ',':
                    this.NextChar();
                    minus = TokenId.Comma;
                    goto Label_0304;

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
                                minus = TokenId.DoubleLiteral;
                                goto Label_0304;
                            }
                            if (IsInfinityLiteralSingle(text))
                            {
                                minus = TokenId.SingleLiteral;
                                goto Label_0304;
                            }
                            this.SetTextPos(textPos);
                        }
                        break;
                    }
                    this.NextChar();
                    minus = this.ParseFromDigit();
                    if (IsNumeric(minus))
                    {
                        goto Label_0304;
                    }
                    this.SetTextPos(textPos);
                    break;
                }
                case '.':
                    this.NextChar();
                    minus = TokenId.Dot;
                    goto Label_0304;

                case '/':
                    this.NextChar();
                    minus = TokenId.Slash;
                    goto Label_0304;

                case ':':
                    this.NextChar();
                    minus = TokenId.Colon;
                    goto Label_0304;

                case ';':
                    this.NextChar();
                    minus = TokenId.Semicolon;
                    goto Label_0304;

                case '=':
                    this.NextChar();
                    minus = TokenId.Equal;
                    goto Label_0304;

                case '?':
                    this.NextChar();
                    minus = TokenId.Question;
                    goto Label_0304;

                default:
                    if (char.IsWhiteSpace(this.ch))
                    {
                        this.ParseWhitespace();
                        minus = TokenId.WhiteSpace;
                    }
                    else if (!this.IsValidStartingCharForIdentifier)
                    {
                        if (!char.IsDigit(this.ch))
                        {
                            if (this.textPos != this.textLen)
                            {
                                throw ParseError(Strings.RequestQueryParser_InvalidCharacter(this.ch, this.textPos));
                            }
                            minus = TokenId.End;
                        }
                        else
                        {
                            minus = this.ParseFromDigit();
                        }
                    }
                    else
                    {
                        this.ParseIdentifier();
                        minus = TokenId.Identifier;
                    }
                    goto Label_0304;
            }
            this.NextChar();
            minus = TokenId.Minus;
            goto Label_0304;
        Label_0279:
            throw ParseError(Strings.RequestQueryParser_InvalidCharacter('$', textPos));
        Label_0304:
            this.token.Id = minus;
            this.token.Text = this.text.Substring(textPos, this.textPos - textPos);
            this.token.Position = textPos;
            this.HandleTypePrefixedLiterals();
            if (this.token.Id == TokenId.Identifier)
            {
                if (IsInfinityOrNaNDouble(this.token.Text))
                {
                    this.token.Id = TokenId.DoubleLiteral;
                }
                else if (IsInfinityOrNanSingle(this.token.Text))
                {
                    this.token.Id = TokenId.SingleLiteral;
                }
                else if ((this.token.Text == "true") || (this.token.Text == "false"))
                {
                    this.token.Id = TokenId.BooleanLiteral;
                }
                else if (this.token.Text == "null")
                {
                    this.token.Id = TokenId.NullLiteral;
                }
            }
        }

        private static Exception ParseError(string message)
        {
            return DataServiceException.CreateSyntaxError(message);
        }

        private TokenId ParseFromDigit()
        {
            TokenId integerLiteral;
            char ch = this.ch;
            this.NextChar();
            if (((ch == '0') && (this.ch == 'x')) || (this.ch == 'X'))
            {
                integerLiteral = TokenId.BinaryLiteral;
                do
                {
                    this.NextChar();
                }
                while (WebConvert.IsCharHexDigit(this.ch));
                return integerLiteral;
            }
            integerLiteral = TokenId.IntegerLiteral;
            while (char.IsDigit(this.ch))
            {
                this.NextChar();
            }
            if (this.ch == '.')
            {
                integerLiteral = TokenId.DoubleLiteral;
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
                integerLiteral = TokenId.DoubleLiteral;
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
                integerLiteral = TokenId.DecimalLiteral;
                this.NextChar();
                return integerLiteral;
            }
            if ((this.ch == 'd') || (this.ch == 'D'))
            {
                integerLiteral = TokenId.DoubleLiteral;
                this.NextChar();
                return integerLiteral;
            }
            if ((this.ch == 'L') || (this.ch == 'l'))
            {
                integerLiteral = TokenId.Int64Literal;
                this.NextChar();
                return integerLiteral;
            }
            if ((this.ch == 'f') || (this.ch == 'F'))
            {
                integerLiteral = TokenId.SingleLiteral;
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
            while (this.IsValidNonStartingCharForIdentifier);
        }

        private void ParseWhitespace()
        {
            while (char.IsWhiteSpace(this.ch))
            {
                this.NextChar();
            }
        }

        internal Token PeekNextToken()
        {
            int textPos = this.textPos;
            char ch = this.ch;
            Token token = this.token;
            this.NextToken();
            Token token2 = this.token;
            this.textPos = textPos;
            this.ch = ch;
            this.token = token;
            return token2;
        }

        internal string ReadDottedIdentifier()
        {
            return this.ReadDottedIdentifier(false);
        }

        internal string ReadDottedIdentifier(bool allowEndWithDotStar)
        {
            this.ValidateToken(TokenId.Identifier);
            StringBuilder builder = null;
            string text = this.CurrentToken.Text;
            this.NextToken();
            bool flag = false;
            while (this.CurrentToken.Id == TokenId.Dot)
            {
                if (flag)
                {
                    throw ParseError(Strings.RequestQueryParser_SyntaxError(this.textPos));
                }
                this.NextToken();
                if (allowEndWithDotStar && (this.CurrentToken.Id == TokenId.Star))
                {
                    flag = true;
                }
                else
                {
                    this.ValidateToken(TokenId.Identifier);
                }
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

        private void SetTextPos(int pos)
        {
            this.textPos = pos;
            this.ch = (this.textPos < this.textLen) ? this.text[this.textPos] : '\0';
        }

        private void ValidateDigit()
        {
            if (!char.IsDigit(this.ch))
            {
                throw ParseError(Strings.RequestQueryParser_DigitExpected(this.textPos));
            }
        }

        internal void ValidateToken(TokenId t)
        {
            if (this.token.Id != t)
            {
                throw ParseError(Strings.RequestQueryParser_SyntaxError(this.textPos));
            }
        }

        internal Token CurrentToken
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

        private bool IsValidNonStartingCharForIdentifier
        {
            get
            {
                if (!char.IsLetterOrDigit(this.ch))
                {
                    return AdditionalUnicodeCategoriesForIdentifier.Contains(char.GetUnicodeCategory(this.ch));
                }
                return true;
            }
        }

        private bool IsValidStartingCharForIdentifier
        {
            get
            {
                if (!char.IsLetter(this.ch) && (this.ch != '_'))
                {
                    return (char.GetUnicodeCategory(this.ch) == UnicodeCategory.LetterNumber);
                }
                return true;
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

