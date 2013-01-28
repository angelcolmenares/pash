namespace System.Management.Automation.Language
{
    using System;
    using System.Globalization;

    public abstract class StringToken : Token
    {
        private readonly string _value;

        internal StringToken(InternalScriptExtent scriptExtent, TokenKind kind, TokenFlags tokenFlags, string value) : base(scriptExtent, kind, tokenFlags)
        {
            this._value = value;
        }

        internal override string ToDebugString(int indent)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}{1}: <{2}> Value:<{3}>", new object[] { new string(' ', indent), base.Kind, base.Text, this.Value });
        }

        public string Value
        {
            get
            {
                return this._value;
            }
        }
    }
}

