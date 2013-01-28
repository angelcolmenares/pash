namespace System.Management.Automation.Language
{
    using System;
    using System.Management.Automation;

    internal sealed class SwitchPair : AstParameterArgumentPair
    {
        internal SwitchPair(CommandParameterAst parameterAst)
        {
            if (parameterAst == null)
            {
                throw PSTraceSource.NewArgumentNullException("parameterAst");
            }
            base.Parameter = parameterAst;
            base.ParameterArgumentType = AstParameterArgumentType.Switch;
            base.ParameterSpecified = true;
            base.ArgumentSpecified = true;
            base.ParameterName = parameterAst.ParameterName;
            base.ParameterText = parameterAst.ParameterName;
            base.ArgumentType = typeof(bool);
        }

        public bool Argument
        {
            get
            {
                return true;
            }
        }
    }
}

