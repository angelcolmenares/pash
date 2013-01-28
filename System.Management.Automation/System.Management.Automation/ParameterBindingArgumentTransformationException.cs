namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Language;
    using System.Runtime.Serialization;

    [Serializable]
    internal class ParameterBindingArgumentTransformationException : ParameterBindingException
    {
        protected ParameterBindingArgumentTransformationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        internal ParameterBindingArgumentTransformationException(ErrorCategory errorCategory, InvocationInfo invocationInfo, IScriptExtent errorPosition, string parameterName, Type parameterType, Type typeSpecified, string resourceBaseName, string errorIdAndResourceId, params object[] args) : base(errorCategory, invocationInfo, errorPosition, parameterName, parameterType, typeSpecified, resourceBaseName, errorIdAndResourceId, args)
        {
        }

        internal ParameterBindingArgumentTransformationException(Exception innerException, ErrorCategory errorCategory, InvocationInfo invocationInfo, IScriptExtent errorPosition, string parameterName, Type parameterType, Type typeSpecified, string resourceBaseName, string errorIdAndResourceId, params object[] args) : base(innerException, errorCategory, invocationInfo, errorPosition, parameterName, parameterType, typeSpecified, resourceBaseName, errorIdAndResourceId, args)
        {
        }
    }
}

