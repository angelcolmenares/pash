namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class PSArgumentOutOfRangeException : ArgumentOutOfRangeException, IContainsErrorRecord
    {
        private string _errorId;
        private System.Management.Automation.ErrorRecord _errorRecord;

        public PSArgumentOutOfRangeException()
        {
            this._errorId = "ArgumentOutOfRange";
        }

        public PSArgumentOutOfRangeException(string paramName) : base(paramName)
        {
            this._errorId = "ArgumentOutOfRange";
        }

        protected PSArgumentOutOfRangeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._errorId = "ArgumentOutOfRange";
            this._errorId = info.GetString("ErrorId");
        }

        public PSArgumentOutOfRangeException(string message, Exception innerException) : base(message, innerException)
        {
            this._errorId = "ArgumentOutOfRange";
        }

        public PSArgumentOutOfRangeException(string paramName, object actualValue, string message) : base(paramName, actualValue, message)
        {
            this._errorId = "ArgumentOutOfRange";
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
                    this._errorRecord = new System.Management.Automation.ErrorRecord(new ParentContainsErrorRecordException(this), this._errorId, ErrorCategory.InvalidArgument, null);
                }
                return this._errorRecord;
            }
        }
    }
}

