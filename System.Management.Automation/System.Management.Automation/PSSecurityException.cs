namespace System.Management.Automation
{
    using System;
    using System.Reflection;
    using System.Runtime.Serialization;

    [Serializable]
    public class PSSecurityException : RuntimeException
    {
        private System.Management.Automation.ErrorRecord _errorRecord;
        private string _message;

        public PSSecurityException()
        {
            this._errorRecord = new System.Management.Automation.ErrorRecord(new ParentContainsErrorRecordException(this), "UnauthorizedAccess", ErrorCategory.SecurityError, null);
            this._errorRecord.ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "MshSecurityManager", "CanNotRun", new object[0]);
            this._message = this._errorRecord.ErrorDetails.Message;
        }

        public PSSecurityException(string message) : base(message)
        {
            this._message = message;
            this._errorRecord = new System.Management.Automation.ErrorRecord(new ParentContainsErrorRecordException(this), "UnauthorizedAccess", ErrorCategory.SecurityError, null);
            this._errorRecord.ErrorDetails = new ErrorDetails(message);
        }

        protected PSSecurityException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._errorRecord = new System.Management.Automation.ErrorRecord(new ParentContainsErrorRecordException(this), "UnauthorizedAccess", ErrorCategory.SecurityError, null);
            this._errorRecord.ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "MshSecurityManager", "CanNotRun", new object[0]);
            this._message = this._errorRecord.ErrorDetails.Message;
        }

        public PSSecurityException(string message, Exception innerException) : base(message, innerException)
        {
            this._errorRecord = new System.Management.Automation.ErrorRecord(new ParentContainsErrorRecordException(this), "UnauthorizedAccess", ErrorCategory.SecurityError, null);
            this._errorRecord.ErrorDetails = new ErrorDetails(message);
            this._message = this._errorRecord.ErrorDetails.Message;
        }

        public override System.Management.Automation.ErrorRecord ErrorRecord
        {
            get
            {
                if (this._errorRecord == null)
                {
                    this._errorRecord = new System.Management.Automation.ErrorRecord(new ParentContainsErrorRecordException(this), "UnauthorizedAccess", ErrorCategory.SecurityError, null);
                }
                return this._errorRecord;
            }
        }

        public override string Message
        {
            get
            {
                return this._message;
            }
        }
    }
}

