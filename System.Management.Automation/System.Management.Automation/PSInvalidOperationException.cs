namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class PSInvalidOperationException : InvalidOperationException, IContainsErrorRecord
    {
        private ErrorCategory _errorCategory;
        private string _errorId;
        private System.Management.Automation.ErrorRecord _errorRecord;
        private object _target;

        public PSInvalidOperationException()
        {
            this._errorId = "InvalidOperation";
            this._errorCategory = ErrorCategory.InvalidOperation;
        }

        public PSInvalidOperationException(string message) : base(message)
        {
            this._errorId = "InvalidOperation";
            this._errorCategory = ErrorCategory.InvalidOperation;
        }

        protected PSInvalidOperationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._errorId = "InvalidOperation";
            this._errorCategory = ErrorCategory.InvalidOperation;
            this._errorId = info.GetString("ErrorId");
        }

        public PSInvalidOperationException(string message, Exception innerException) : base(message, innerException)
        {
            this._errorId = "InvalidOperation";
            this._errorCategory = ErrorCategory.InvalidOperation;
        }

        internal PSInvalidOperationException(string message, Exception innerException, string errorId, ErrorCategory errorCategory, object target) : base(message, innerException)
        {
            this._errorId = "InvalidOperation";
            this._errorCategory = ErrorCategory.InvalidOperation;
            this._errorId = errorId;
            this._errorCategory = errorCategory;
            this._target = target;
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

        internal void SetErrorId(string errorId)
        {
            this._errorId = errorId;
        }

        public System.Management.Automation.ErrorRecord ErrorRecord
        {
            get
            {
                if (this._errorRecord == null)
                {
                    this._errorRecord = new System.Management.Automation.ErrorRecord(new ParentContainsErrorRecordException(this), this._errorId, this._errorCategory, this._target);
                }
                return this._errorRecord;
            }
        }
    }
}

