namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Language;
    using System.Runtime.Serialization;

    [Serializable]
    internal class ParameterBindingValidationException : ParameterBindingException
    {
        private readonly bool _swallowException;

        protected ParameterBindingValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        internal ParameterBindingValidationException(ErrorCategory errorCategory, InvocationInfo invocationInfo, IScriptExtent errorPosition, string parameterName, Type parameterType, Type typeSpecified, string resourceBaseName, string errorIdAndResourceId, params object[] args) : base(errorCategory, invocationInfo, errorPosition, parameterName, parameterType, typeSpecified, resourceBaseName, errorIdAndResourceId, args)
        {
        }

        internal ParameterBindingValidationException(Exception innerException, ErrorCategory errorCategory, InvocationInfo invocationInfo, IScriptExtent errorPosition, string parameterName, Type parameterType, Type typeSpecified, string resourceBaseName, string errorIdAndResourceId, params object[] args) : base(innerException, errorCategory, invocationInfo, errorPosition, parameterName, parameterType, typeSpecified, resourceBaseName, errorIdAndResourceId, args)
        {
            ValidationMetadataException exception = innerException as ValidationMetadataException;
            if ((exception != null) && exception.SwallowException)
            {
                this._swallowException = true;
            }
        }

        internal bool SwallowException
        {
            get
            {
                return this._swallowException;
            }
        }
    }
}

