namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class SessionStateOverflowException : SessionStateException
    {
        public SessionStateOverflowException()
        {
        }

        public SessionStateOverflowException(string message) : base(message)
        {
        }

        protected SessionStateOverflowException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SessionStateOverflowException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal SessionStateOverflowException(string itemName, SessionStateCategory sessionStateCategory, string errorIdAndResourceId, string resourceStr, params object[] messageArgs) : base(itemName, sessionStateCategory, errorIdAndResourceId, resourceStr, ErrorCategory.InvalidOperation, messageArgs)
        {
        }
    }
}

