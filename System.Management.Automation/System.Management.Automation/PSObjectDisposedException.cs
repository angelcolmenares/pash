namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class PSObjectDisposedException : ObjectDisposedException, IContainsErrorRecord
    {
        private string _errorId;
        private System.Management.Automation.ErrorRecord _errorRecord;

        public PSObjectDisposedException(string objectName) : base(objectName)
        {
            this._errorId = "ObjectDisposed";
        }

        protected PSObjectDisposedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._errorId = "ObjectDisposed";
            this._errorId = info.GetString("ErrorId");
        }

        public PSObjectDisposedException(string message, Exception innerException) : base(message, innerException)
        {
            this._errorId = "ObjectDisposed";
        }

        public PSObjectDisposedException(string objectName, string message) : base(objectName, message)
        {
            this._errorId = "ObjectDisposed";
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
                    this._errorRecord = new System.Management.Automation.ErrorRecord(new ParentContainsErrorRecordException(this), this._errorId, ErrorCategory.InvalidOperation, null);
                }
                return this._errorRecord;
            }
        }
    }
}

