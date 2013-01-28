namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class MethodInvocationException : MethodException
    {
        internal const string CopyToInvocationExceptionMsg = "CopyToInvocationException";
        internal const string MethodInvocationExceptionMsg = "MethodInvocationException";
        internal const string WMIMethodInvocationException = "WMIMethodInvocationException";

        public MethodInvocationException() : base(typeof(MethodInvocationException).FullName)
        {
        }

        public MethodInvocationException(string message) : base(message)
        {
        }

        protected MethodInvocationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public MethodInvocationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal MethodInvocationException(string errorId, Exception innerException, string resourceString, params object[] arguments) : base(errorId, innerException, resourceString, arguments)
        {
        }
    }
}

