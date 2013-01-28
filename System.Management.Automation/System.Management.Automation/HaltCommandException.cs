namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;
    using System.Runtime.Serialization;

    [Serializable]
    public class HaltCommandException : SystemException
    {
        public HaltCommandException() : base(StringUtil.Format(AutomationExceptions.HaltCommandException, new object[0]))
        {
        }

        public HaltCommandException(string message) : base(message)
        {
        }

        protected HaltCommandException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public HaltCommandException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

