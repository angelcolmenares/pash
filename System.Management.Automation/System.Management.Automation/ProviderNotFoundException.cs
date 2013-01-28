namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class ProviderNotFoundException : SessionStateException
    {
        public ProviderNotFoundException()
        {
        }

        public ProviderNotFoundException(string message) : base(message)
        {
        }

        protected ProviderNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ProviderNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal ProviderNotFoundException(string itemName, SessionStateCategory sessionStateCategory, string errorIdAndResourceId, string resourceStr, params object[] messageArgs) : base(itemName, sessionStateCategory, errorIdAndResourceId, resourceStr, ErrorCategory.ObjectNotFound, messageArgs)
        {
        }
    }
}

