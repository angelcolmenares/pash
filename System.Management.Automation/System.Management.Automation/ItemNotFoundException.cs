namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class ItemNotFoundException : SessionStateException
    {
        public ItemNotFoundException()
        {
        }

        public ItemNotFoundException(string message) : base(message)
        {
        }

        protected ItemNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ItemNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal ItemNotFoundException(string path, string errorIdAndResourceId, string resourceStr) : base(path, SessionStateCategory.Drive, errorIdAndResourceId, resourceStr, ErrorCategory.ObjectNotFound, new object[0])
        {
        }
    }
}

