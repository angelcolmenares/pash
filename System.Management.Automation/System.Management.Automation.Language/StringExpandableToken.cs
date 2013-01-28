namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;

    public class StringExpandableToken : StringToken
    {
        private readonly string _formatString;
        private ReadOnlyCollection<Token> _nestedTokens;

        internal StringExpandableToken(InternalScriptExtent scriptExtent, TokenKind tokenKind, string value, string formatString, List<Token> nestedTokens, TokenFlags flags) : base(scriptExtent, tokenKind, flags, value)
        {
            if ((nestedTokens != null) && nestedTokens.Any<Token>())
            {
                this._nestedTokens = new ReadOnlyCollection<Token>(nestedTokens.ToArray());
            }
            this._formatString = formatString;
        }

        internal override string ToDebugString(int indent)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToDebugString(indent));
            if (this._nestedTokens != null)
            {
                ToDebugString(this._nestedTokens, sb, indent);
            }
            return sb.ToString();
        }

        internal static void ToDebugString(ReadOnlyCollection<Token> nestedTokens, StringBuilder sb, int indent)
        {
            foreach (Token token in nestedTokens)
            {
                sb.Append(Environment.NewLine);
                sb.Append(token.ToDebugString(indent + 4));
            }
        }

        internal string FormatString
        {
            get
            {
                return this._formatString;
            }
        }

        public ReadOnlyCollection<Token> NestedTokens
        {
            get
            {
                return this._nestedTokens;
            }
            internal set
            {
                this._nestedTokens = value;
            }
        }
    }
}

