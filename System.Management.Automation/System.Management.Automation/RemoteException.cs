namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class RemoteException : RuntimeException
    {
        private System.Management.Automation.ErrorRecord _remoteErrorRecord;
        [NonSerialized]
        private PSObject _serializedRemoteException;
        [NonSerialized]
        private PSObject _serializedRemoteInvocationInfo;

        public RemoteException()
        {
        }

        public RemoteException(string message) : base(message)
        {
        }

        protected RemoteException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public RemoteException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal RemoteException(string message, PSObject serializedRemoteException, PSObject serializedRemoteInvocationInfo) : base(message)
        {
            this._serializedRemoteException = serializedRemoteException;
            this._serializedRemoteInvocationInfo = serializedRemoteInvocationInfo;
        }

        internal void SetRemoteErrorRecord(System.Management.Automation.ErrorRecord remoteError)
        {
            this._remoteErrorRecord = remoteError;
        }

        public override System.Management.Automation.ErrorRecord ErrorRecord
        {
            get
            {
                if (this._remoteErrorRecord != null)
                {
                    return this._remoteErrorRecord;
                }
                return base.ErrorRecord;
            }
        }

        public PSObject SerializedRemoteException
        {
            get
            {
                return this._serializedRemoteException;
            }
        }

        public PSObject SerializedRemoteInvocationInfo
        {
            get
            {
                return this._serializedRemoteInvocationInfo;
            }
        }
    }
}

