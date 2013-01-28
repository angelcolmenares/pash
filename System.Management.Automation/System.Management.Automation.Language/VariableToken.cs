namespace System.Management.Automation.Language
{
    using System;
    using System.Globalization;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public class VariableToken : Token
    {
        internal VariableToken(InternalScriptExtent scriptExtent, System.Management.Automation.VariablePath path, TokenFlags tokenFlags, bool splatted) : base(scriptExtent, splatted ? TokenKind.SplattedVariable : TokenKind.Variable, tokenFlags)
        {
            this.VariablePath = path;
        }

        internal override string ToDebugString(int indent)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}{1}: <{2}> Name:<{3}>", new object[] { new string(' ', indent), base.Kind, base.Text, this.Name });
        }

        public string Name
        {
            get
            {
                return this.VariablePath.UnqualifiedPath;
            }
        }

        public System.Management.Automation.VariablePath VariablePath { get; private set; }
    }
}

