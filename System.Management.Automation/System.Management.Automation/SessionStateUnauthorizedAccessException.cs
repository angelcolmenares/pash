namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class SessionStateUnauthorizedAccessException : SessionStateException
    {
        public SessionStateUnauthorizedAccessException()
        {
        }

        public SessionStateUnauthorizedAccessException(string message) : base(message)
        {
        }

        protected SessionStateUnauthorizedAccessException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SessionStateUnauthorizedAccessException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal SessionStateUnauthorizedAccessException(string itemName, SessionStateCategory sessionStateCategory, string errorIdAndResourceId, string resourceStr) : base(itemName, sessionStateCategory, errorIdAndResourceId, resourceStr, ErrorCategory.WriteError, new object[0])
        {
        }
    }
}

