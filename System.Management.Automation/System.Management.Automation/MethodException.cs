namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class MethodException : ExtendedTypeSystemException
    {
        internal const string MethodAmbiguousExceptionMsg = "MethodAmbiguousException";
        internal const string MethodArgumentConversionExceptionMsg = "MethodArgumentConversionException";
        internal const string MethodArgumentCountExceptionMsg = "MethodArgumentCountException";
        internal const string NonRefArgumentToRefParameterMsg = "NonRefArgumentToRefParameter";
        internal const string RefArgumentToNonRefParameterMsg = "RefArgumentToNonRefParameter";

        public MethodException() : base(typeof(MethodException).FullName)
        {
        }

        public MethodException(string message) : base(message)
        {
        }

        protected MethodException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public MethodException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal MethodException(string errorId, Exception innerException, string resourceString, params object[] arguments) : base(errorId, innerException, resourceString, arguments)
        {
        }
    }
}

