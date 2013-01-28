namespace System.Management.Automation.Language
{
    using System;
    using System.Globalization;

    public class NumberToken : Token
    {
        private readonly object _value;

        internal NumberToken(InternalScriptExtent scriptExtent, object value, TokenFlags tokenFlags) : base(scriptExtent, TokenKind.Number, tokenFlags)
        {
            this._value = value;
        }

        internal override string ToDebugString(int indent)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}{1}: <{2}> Value:<{3}> Type:<{4}>", new object[] { new string(' ', indent), base.Kind, base.Text, this._value, this._value.GetType().Name });
        }

        public object Value
        {
            get
            {
                return this._value;
            }
        }
    }
}

