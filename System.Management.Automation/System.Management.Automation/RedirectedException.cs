namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class RedirectedException : RuntimeException
    {
        public RedirectedException()
        {
            base.SetErrorId("RedirectedException");
            base.SetErrorCategory(ErrorCategory.NotSpecified);
        }

        public RedirectedException(string message) : base(message)
        {
            base.SetErrorId("RedirectedException");
            base.SetErrorCategory(ErrorCategory.NotSpecified);
        }

        protected RedirectedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public RedirectedException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorId("RedirectedException");
            base.SetErrorCategory(ErrorCategory.NotSpecified);
        }
    }
}

