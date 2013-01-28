namespace System.Management.Automation.Language
{
    using System;
    using System.Management.Automation;

    internal sealed class PipeObjectPair : AstParameterArgumentPair
    {
        internal PipeObjectPair(string parameterName, Type pipeObjType)
        {
            if (parameterName == null)
            {
                throw PSTraceSource.NewArgumentNullException("parameterName");
            }
            base.Parameter = null;
            base.ParameterArgumentType = AstParameterArgumentType.PipeObject;
            base.ParameterSpecified = true;
            base.ArgumentSpecified = true;
            base.ParameterName = parameterName;
            base.ParameterText = parameterName;
            base.ArgumentType = pipeObjType;
        }
    }
}

