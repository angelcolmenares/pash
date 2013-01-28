namespace Microsoft.Data.Spatial
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Spatial;

    internal class WellKnownTextLexer : TextLexerBase
    {
        public WellKnownTextLexer(TextReader text) : base(text)
        {
        }

        protected override bool MatchTokenType(char nextChar, int? activeTokenType, out int tokenType)
        {
            switch (nextChar)
            {
                case '\t':
                case '\n':
                case '\r':
                case ' ':
                    tokenType = 8;
                    return false;

                case '(':
                    tokenType = 4;
                    return true;

                case ')':
                    tokenType = 5;
                    return true;

                case '+':
                case '-':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    tokenType = 2;
                    return false;

                case ',':
                    tokenType = 7;
                    return true;

                case '.':
                    tokenType = 6;
                    return true;

                case ';':
                    tokenType = 3;
                    return true;

                case '=':
                    tokenType = 1;
                    return true;

                case 'E':
                case 'e':
                    if (activeTokenType == 2)
                    {
                        tokenType = 2;
                    }
                    else
                    {
                        tokenType = 0;
                    }
                    return false;
            }
            if (((nextChar < 'A') || (nextChar > 'Z')) && ((nextChar < 'a') || (nextChar > 'z')))
            {
                throw new FormatException(Strings.WellKnownText_UnexpectedCharacter(nextChar));
            }
            tokenType = 0;
            return false;
        }
    }
}

