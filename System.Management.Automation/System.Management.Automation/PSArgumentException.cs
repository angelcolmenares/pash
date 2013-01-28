namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class PSArgumentException : ArgumentException, IContainsErrorRecord
    {
        private string _errorId;
        private System.Management.Automation.ErrorRecord _errorRecord;
        private string _message;

        public PSArgumentException()
        {
            this._errorId = "Argument";
        }

        public PSArgumentException(string message) : base(message)
        {
            this._errorId = "Argument";
        }

        protected PSArgumentException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._errorId = "Argument";
            this._errorId = info.GetString("ErrorId");
            this._message = info.GetString("PSArgumentException_MessageOverride");
        }

        public PSArgumentException(string message, Exception innerException) : base(message, innerException)
        {
            this._errorId = "Argument";
            this._message = message;
        }

        public PSArgumentException(string message, string paramName) : base(message, paramName)
        {
            this._errorId = "Argument";
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
            info.AddValue("PSArgumentException_MessageOverride", this._message);
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

