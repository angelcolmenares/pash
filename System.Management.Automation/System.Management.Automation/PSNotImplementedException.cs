namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class PSNotImplementedException : NotImplementedException, IContainsErrorRecord
    {
        private string _errorId;
        private System.Management.Automation.ErrorRecord _errorRecord;

        public PSNotImplementedException()
        {
            this._errorId = "NotImplemented";
        }

        public PSNotImplementedException(string message) : base(message)
        {
            this._errorId = "NotImplemented";
        }

        protected PSNotImplementedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._errorId = "NotImplemented";
            this._errorId = info.GetString("ErrorId");
        }

        public PSNotImplementedException(string message, Exception innerException) : base(message, innerException)
        {
            this._errorId = "NotImplemented";
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

