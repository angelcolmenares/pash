namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class PSNotSupportedException : NotSupportedException, IContainsErrorRecord
    {
        private string _errorId;
        private System.Management.Automation.ErrorRecord _errorRecord;

        public PSNotSupportedException()
        {
            this._errorId = "NotSupported";
        }

        public PSNotSupportedException(string message) : base(message)
        {
            this._errorId = "NotSupported";
        }

        protected PSNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._errorId = "NotSupported";
            this._errorId = info.GetString("ErrorId");
        }

        public PSNotSupportedException(string message, Exception innerException) : base(message, innerException)
        {
            this._errorId = "NotSupported";
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new PSArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("ErrorId", this._errorId);
        }

        public System.Management.Automation.ErrorRecord ErrorRecord
        {
            get
            {
                if (this._errorRecord == null)
                {
                    this._errorRecord = new System.Management.Automation.ErrorRecord(new ParentContainsErrorRecordException(this), this._errorId, ErrorCategory.NotImplemented, null);
                }
                return this._errorRecord;
            }
        }
    }
}

