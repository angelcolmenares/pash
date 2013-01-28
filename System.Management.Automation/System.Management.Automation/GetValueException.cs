namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class GetValueException : ExtendedTypeSystemException
    {
        internal const string GetWithoutGetterExceptionMsg = "GetWithoutGetterException";
        internal const string WriteOnlyProperty = "WriteOnlyProperty";

        public GetValueException() : base(typeof(GetValueException).FullName)
        {
        }

        public GetValueException(string message) : base(message)
        {
        }

        protected GetValueException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public GetValueException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal GetValueException(string errorId, Exception innerException, string resourceString, params object[] arguments) : base(errorId, innerException, resourceString, arguments)
        {
        }
    }
}

