namespace System.Management.Automation.Language
{
    using System;
    using System.Management.Automation;

    internal sealed class FakePair : AstParameterArgumentPair
    {
        internal FakePair(CommandParameterAst parameterAst)
        {
            if (parameterAst == null)
            {
                throw PSTraceSource.NewArgumentNullException("parameterAst");
            }
            base.Parameter = parameterAst;
            base.ParameterArgumentType = AstParameterArgumentType.Fake;
            base.ParameterSpecified = true;
            base.ArgumentSpecified = true;
            base.ParameterName = parameterAst.ParameterName;
            base.ParameterText = parameterAst.ParameterName;
            base.ArgumentType = typeof(object);
        }
    }
}

