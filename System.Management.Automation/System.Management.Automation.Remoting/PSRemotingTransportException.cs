namespace System.Management.Automation.Remoting
{
    using System;
    using System.Management.Automation;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class PSRemotingTransportException : RuntimeException
    {
        private int _errorCode;
        private string _transportMessage;

        public PSRemotingTransportException() : base(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.DefaultRemotingExceptionMessage, new object[] { typeof(PSRemotingTransportException).FullName }))
        {
            this.SetDefaultErrorRecord();
        }

        public PSRemotingTransportException(string message) : base(message)
        {
            this.SetDefaultErrorRecord();
        }

        protected PSRemotingTransportException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            if (info == null)
            {
                throw new PSArgumentNullException("info");
            }
            this._errorCode = info.GetInt32("ErrorCode");
            this._transportMessage = info.GetString("TransportMessage");
        }

        public PSRemotingTransportException(string message, Exception innerException) : base(message, innerException)
        {
            this.SetDefaultErrorRecord();
        }

        internal PSRemotingTransportException(Exception innerException, string resourceString, params object[] args) : base(PSRemotingErrorInvariants.FormatResourceString(resourceString, args), innerException)
        {
            this.SetDefaultErrorRecord();
        }

        internal PSRemotingTransportException(PSRemotingErrorId errorId, string resourceString, params object[] args) : base(PSRemotingErrorInvariants.FormatResourceString(resourceString, args))
        {
            this.SetDefaultErrorRecord();
            this._errorCode = (int) errorId;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new PSArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("ErrorCode", this._errorCode);
            info.AddValue("TransportMessage", this._transportMessage);
        }

        protected void SetDefaultErrorRecord()
        {
            base.SetErrorCategory(ErrorCategory.ResourceUnavailable);
            base.SetErrorId(typeof(PSRemotingDataStructureException).FullName);
        }

        public int ErrorCode
        {
            get
            {
                return this._errorCode;
            }
            set
            {
                this._errorCode = value;
            }
        }

        public string TransportMessage
        {
            get
            {
                return this._transportMessage;
            }
            set
            {
                this._transportMessage = value;
            }
        }
    }
}

