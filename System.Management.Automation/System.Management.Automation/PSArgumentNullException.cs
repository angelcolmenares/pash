namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class PSArgumentNullException : ArgumentNullException, IContainsErrorRecord
    {
        private string _errorId;
        private System.Management.Automation.ErrorRecord _errorRecord;
        private string _message;

        public PSArgumentNullException()
        {
            this._errorId = "ArgumentNull";
        }

        public PSArgumentNullException(string paramName) : base(paramName)
        {
            this._errorId = "ArgumentNull";
        }

        protected PSArgumentNullException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._errorId = "ArgumentNull";
            this._errorId = info.GetString("ErrorId");
            this._message = info.GetString("PSArgumentNullException_MessageOverride");
        }

        public PSArgumentNullException(string message, Exception innerException) : base(message, innerException)
        {
            this._errorId = "ArgumentNull";
            this._message = message;
        }

        public PSArgumentNullException(string paramName, string message) : base(paramName, message)
        {
            this._errorId = "ArgumentNull";
            this._message = message;
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
            info.AddValue("PSArgumentNullException_MessageOverride", this._message);
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

        public override string Message
        {
            get
            {
                if (!string.IsNullOrEmpty(this._message))
                {
                    return this._message;
                }
                return base.Message;
            }
        }
    }
}

