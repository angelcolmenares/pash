namespace Microsoft.Data.Spatial
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    internal abstract class TextLexerBase
    {
        private LexerToken currentToken;
        private LexerToken peekToken;
        private TextReader reader;

        protected TextLexerBase(TextReader text)
        {
            this.reader = text;
        }

        protected abstract bool MatchTokenType(char nextChar, int? currentType, out int type);
        public bool Next()
        {
            int num;
            if (this.peekToken != null)
            {
                this.currentToken = this.peekToken;
                this.peekToken = null;
                return true;
            }
            LexerToken currentToken = this.CurrentToken;
            int? currentType = null;
            StringBuilder builder = null;
            bool flag = false;
            while (!flag && ((num = this.reader.Peek()) >= 0))
            {
                int num2;
                char nextChar = (char) num;
                flag = this.MatchTokenType(nextChar, currentType, out num2);
                if (!currentType.HasValue)
                {
                    currentType = new int?(num2);
                    builder = new StringBuilder();
                    builder.Append(nextChar);
                    this.reader.Read();
                }
                else
                {
                    int? nullable2 = currentType;
                    int num3 = num2;
                    if ((nullable2.GetValueOrDefault() == num3) && nullable2.HasValue)
                    {
                        builder.Append(nextChar);
                        this.reader.Read();
                        continue;
                    }
                    flag = true;
                }
            }
            if (currentType.HasValue)
            {
                LexerToken token2 = new LexerToken {
                    Text = builder.ToString(),
                    Type = currentType.Value
                };
                this.currentToken = token2;
            }
            return (currentToken != this.currentToken);
        }

        public bool Peek(out LexerToken token)
        {
            if (this.peekToken != null)
            {
                token = this.peekToken;
                return true;
            }
            LexerToken currentToken = this.currentToken;
            if (this.Next())
            {
                this.peekToken = this.currentToken;
                token = this.currentToken;
                this.currentToken = currentToken;
                return true;
            }
            this.peekToken = null;
            token = null;
            this.currentToken = currentToken;
            return false;
        }

        public LexerToken CurrentToken
        {
            get
            {
                return this.currentToken;
            }
        }
    }
}

