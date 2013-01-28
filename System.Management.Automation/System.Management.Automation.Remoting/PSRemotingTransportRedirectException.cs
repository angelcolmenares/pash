namespace System.Management.Automation.Remoting
{
    using System;
    using System.Management.Automation;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class PSRemotingTransportRedirectException : PSRemotingTransportException
    {
        private string redirectLocation;

        public PSRemotingTransportRedirectException() : base(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.DefaultRemotingExceptionMessage, new object[] { typeof(PSRemotingTransportRedirectException).FullName }))
        {
            base.SetDefaultErrorRecord();
        }

        public PSRemotingTransportRedirectException(string message) : base(message)
        {
        }

        protected PSRemotingTransportRedirectException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            if (info == null)
            {
                throw new PSArgumentNullException("info");
            }
            this.redirectLocation = info.GetString("RedirectLocation");
        }

        public PSRemotingTransportRedirectException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal PSRemotingTransportRedirectException(Exception innerException, string resourceString, params object[] args) : base(innerException, resourceString, args)
        {
        }

        internal PSRemotingTransportRedirectException(string redirectLocation, PSRemotingErrorId errorId, string resourceString, params object[] args) : base(errorId, resourceString, args)
        {
            this.redirectLocation = redirectLocation;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new PSArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("RedirectLocation", this.redirectLocation);
        }

        public string RedirectLocation
        {
            get
            {
                return this.redirectLocation;
            }
        }
    }
}

