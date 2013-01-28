namespace System.Management.Automation.Language
{
    using System;
    using System.Globalization;

    public class ParameterToken : Token
    {
        private readonly string _parameterName;
        private readonly bool _usedColon;

        internal ParameterToken(InternalScriptExtent scriptExtent, string parameterName, bool usedColon) : base(scriptExtent, TokenKind.Parameter, TokenFlags.None)
        {
            this._parameterName = parameterName;
            this._usedColon = usedColon;
        }

        internal override string ToDebugString(int indent)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}{1}: <-{2}{3}>", new object[] { new string(' ', indent), base.Kind, this._parameterName, this._usedColon ? ":" : "" });
        }

        public string ParameterName
        {
            get
            {
                return this._parameterName;
            }
        }

        public bool UsedColon
        {
            get
            {
                return this._usedColon;
            }
        }
    }
}

