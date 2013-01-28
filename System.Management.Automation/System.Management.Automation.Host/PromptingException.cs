namespace System.Management.Automation.Host
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Runtime.Serialization;

    [Serializable]
    public class PromptingException : HostException
    {
        public PromptingException() : base(StringUtil.Format(HostInterfaceExceptionsStrings.DefaultCtorMessageTemplate, typeof(PromptingException).FullName))
        {
            this.SetDefaultErrorRecord();
        }

        public PromptingException(string message) : base(message)
        {
            this.SetDefaultErrorRecord();
        }

        protected PromptingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public PromptingException(string message, Exception innerException) : base(message, innerException)
        {
            this.SetDefaultErrorRecord();
        }

        public PromptingException(string message, Exception innerException, string errorId, ErrorCategory errorCategory) : base(message, innerException, errorId, errorCategory)
        {
        }

        private void SetDefaultErrorRecord()
        {
            base.SetErrorCategory(ErrorCategory.ResourceUnavailable);
            base.SetErrorId(typeof(PromptingException).FullName);
        }
    }
}

