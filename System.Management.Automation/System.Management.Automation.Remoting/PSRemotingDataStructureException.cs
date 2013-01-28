namespace System.Management.Automation.Remoting
{
    using System;
    using System.Management.Automation;
    using System.Runtime.Serialization;

    [Serializable]
    public class PSRemotingDataStructureException : RuntimeException
    {
        public PSRemotingDataStructureException() : base(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.DefaultRemotingExceptionMessage, new object[] { typeof(PSRemotingDataStructureException).FullName }))
        {
            this.SetDefaultErrorRecord();
        }

        public PSRemotingDataStructureException(string message) : base(message)
        {
            this.SetDefaultErrorRecord();
        }

        protected PSRemotingDataStructureException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        internal PSRemotingDataStructureException(string resourceString, params object[] args) : base(PSRemotingErrorInvariants.FormatResourceString(resourceString, args))
        {
            this.SetDefaultErrorRecord();
        }

        public PSRemotingDataStructureException(string message, Exception innerException) : base(message, innerException)
        {
            this.SetDefaultErrorRecord();
        }

        internal PSRemotingDataStructureException(Exception innerException, string resourceString, params object[] args) : base(PSRemotingErrorInvariants.FormatResourceString(resourceString, args), innerException)
        {
            this.SetDefaultErrorRecord();
        }

        private void SetDefaultErrorRecord()
        {
            base.SetErrorCategory(ErrorCategory.ResourceUnavailable);
            base.SetErrorId(typeof(PSRemotingDataStructureException).FullName);
        }
    }
}

