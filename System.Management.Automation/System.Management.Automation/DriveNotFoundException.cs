namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class DriveNotFoundException : SessionStateException
    {
        public DriveNotFoundException()
        {
        }

        public DriveNotFoundException(string message) : base(message)
        {
        }

        protected DriveNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public DriveNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal DriveNotFoundException(string itemName, string errorIdAndResourceId, string resourceStr) : base(itemName, SessionStateCategory.Drive, errorIdAndResourceId, resourceStr, ErrorCategory.ObjectNotFound, new object[0])
        {
        }
    }
}

