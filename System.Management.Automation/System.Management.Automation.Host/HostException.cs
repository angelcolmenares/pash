namespace System.Management.Automation.Host
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Runtime.Serialization;

    [Serializable]
    public class HostException : RuntimeException
    {
        public HostException() : base(StringUtil.Format(HostInterfaceExceptionsStrings.DefaultCtorMessageTemplate, typeof(HostException).FullName))
        {
            this.SetDefaultErrorRecord();
        }

        public HostException(string message) : base(message)
        {
            this.SetDefaultErrorRecord();
        }

        protected HostException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public HostException(string message, Exception innerException) : base(message, innerException)
        {
            this.SetDefaultErrorRecord();
        }

        public HostException(string message, Exception innerException, string errorId, ErrorCategory errorCategory) : base(message, innerException)
        {
            base.SetErrorId(errorId);
            base.SetErrorCategory(errorCategory);
        }

        private void SetDefaultErrorRecord()
        {
            base.SetErrorCategory(ErrorCategory.ResourceUnavailable);
            base.SetErrorId(typeof(HostException).FullName);
        }
    }
}

