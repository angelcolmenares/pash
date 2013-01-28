namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class PropertyNotFoundException : ExtendedTypeSystemException
    {
        public PropertyNotFoundException() : base(typeof(PropertyNotFoundException).FullName)
        {
        }

        public PropertyNotFoundException(string message) : base(message)
        {
        }

        protected PropertyNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public PropertyNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal PropertyNotFoundException(string errorId, Exception innerException, string resourceString, params object[] arguments) : base(errorId, innerException, resourceString, arguments)
        {
        }
    }
}

