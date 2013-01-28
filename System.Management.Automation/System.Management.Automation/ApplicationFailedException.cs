namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class ApplicationFailedException : RuntimeException
    {
        private const string errorIdString = "NativeCommandFailed";

        public ApplicationFailedException()
        {
            base.SetErrorId("NativeCommandFailed");
            base.SetErrorCategory(ErrorCategory.ResourceUnavailable);
        }

        public ApplicationFailedException(string message) : base(message)
        {
            base.SetErrorId("NativeCommandFailed");
            base.SetErrorCategory(ErrorCategory.ResourceUnavailable);
        }

        protected ApplicationFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ApplicationFailedException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorId("NativeCommandFailed");
            base.SetErrorCategory(ErrorCategory.ResourceUnavailable);
        }

        internal ApplicationFailedException(string message, string errorId) : base(message)
        {
            base.SetErrorId(errorId);
            base.SetErrorCategory(ErrorCategory.ResourceUnavailable);
        }

        internal ApplicationFailedException(string message, string errorId, Exception innerException) : base(message, innerException)
        {
            base.SetErrorId(errorId);
            base.SetErrorCategory(ErrorCategory.ResourceUnavailable);
        }
    }
}

