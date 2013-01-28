namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class GetValueInvocationException : GetValueException
    {
        internal const string ExceptionWhenGettingMsg = "ExceptionWhenGetting";

        public GetValueInvocationException() : base(typeof(GetValueInvocationException).FullName)
        {
        }

        public GetValueInvocationException(string message) : base(message)
        {
        }

        protected GetValueInvocationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public GetValueInvocationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal GetValueInvocationException(string errorId, Exception innerException, string resourceString, params object[] arguments) : base(errorId, innerException, resourceString, arguments)
        {
        }
    }
}

