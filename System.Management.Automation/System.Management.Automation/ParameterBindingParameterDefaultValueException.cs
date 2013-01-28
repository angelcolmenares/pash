namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Language;
    using System.Runtime.Serialization;

    [Serializable]
    internal class ParameterBindingParameterDefaultValueException : ParameterBindingException
    {
        protected ParameterBindingParameterDefaultValueException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        internal ParameterBindingParameterDefaultValueException(ErrorCategory errorCategory, InvocationInfo invocationInfo, IScriptExtent errorPosition, string parameterName, Type parameterType, Type typeSpecified, string resourceBaseName, string errorIdAndResourceId, params object[] args) : base(errorCategory, invocationInfo, errorPosition, parameterName, parameterType, typeSpecified, resourceBaseName, errorIdAndResourceId, args)
        {
        }

        internal ParameterBindingParameterDefaultValueException(Exception innerException, ErrorCategory errorCategory, InvocationInfo invocationInfo, IScriptExtent errorPosition, string parameterName, Type parameterType, Type typeSpecified, string resourceBaseName, string errorIdAndResourceId, params object[] args) : base(innerException, errorCategory, invocationInfo, errorPosition, parameterName, parameterType, typeSpecified, resourceBaseName, errorIdAndResourceId, args)
        {
        }
    }
}

