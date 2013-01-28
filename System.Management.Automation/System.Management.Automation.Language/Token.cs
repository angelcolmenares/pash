namespace System.Management.Automation.Language
{
    using System;
    using System.Globalization;

    public class Token
    {
        private TokenKind _kind;
        private readonly InternalScriptExtent _scriptExtent;
        private System.Management.Automation.Language.TokenFlags _tokenFlags;

        internal Token(InternalScriptExtent scriptExtent, TokenKind kind, System.Management.Automation.Language.TokenFlags tokenFlags)
        {
            this._scriptExtent = scriptExtent;
            this._kind = kind;
            this._tokenFlags = tokenFlags | kind.GetTraits();
        }

        internal void SetIsCommandArgument()
        {
            if (this._kind != TokenKind.Identifier)
            {
                this._kind = TokenKind.Generic;
            }
        }

        internal virtual string ToDebugString(int indent)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}{1}: <{2}>", new object[] { new string(' ', indent), this._kind, this.Text });
        }

        public override string ToString()
        {
            if (this._kind != TokenKind.EndOfInput)
            {
                return this.Text;
            }
            return "<eof>";
        }

        public IScriptExtent Extent
        {
            get
            {
                return this._scriptExtent;
            }
        }

        public bool HasError
        {
            get
            {
                return ((this._tokenFlags & System.Management.Automation.Language.TokenFlags.TokenInError) != System.Management.Automation.Language.TokenFlags.None);
            }
        }

        public TokenKind Kind
        {
            get
            {
                return this._kind;
            }
        }

        public string Text
        {
            get
            {
                return this._scriptExtent.Text;
            }
        }

        public System.Management.Automation.Language.TokenFlags TokenFlags
        {
            get
            {
                return this._tokenFlags;
            }
            internal set
            {
                this._tokenFlags = value;
            }
        }
    }
}

