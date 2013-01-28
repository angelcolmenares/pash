namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class CmdletInvocationException : RuntimeException
    {
        private System.Management.Automation.ErrorRecord _errorRecord;

        public CmdletInvocationException()
        {
        }

        internal CmdletInvocationException(System.Management.Automation.ErrorRecord errorRecord) : base(RuntimeException.RetrieveMessage(errorRecord), RuntimeException.RetrieveException(errorRecord))
        {
            if (errorRecord == null)
            {
                throw new ArgumentNullException("errorRecord");
            }
            this._errorRecord = errorRecord;
            Exception exception = errorRecord.Exception;
        }

        public CmdletInvocationException(string message) : base(message)
        {
        }

        internal CmdletInvocationException(Exception innerException, InvocationInfo invocationInfo) : base(RuntimeException.RetrieveMessage(innerException), innerException)
        {
            if (innerException == null)
            {
                throw new ArgumentNullException("innerException");
            }
            IContainsErrorRecord record = innerException as IContainsErrorRecord;
            if ((record != null) && (record.ErrorRecord != null))
            {
                this._errorRecord = new System.Management.Automation.ErrorRecord(record.ErrorRecord, innerException);
            }
            else
            {
                this._errorRecord = new System.Management.Automation.ErrorRecord(innerException, innerException.GetType().FullName, ErrorCategory.NotSpecified, null);
            }
            this._errorRecord.SetInvocationInfo(invocationInfo);
        }

        protected CmdletInvocationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            if (info.GetBoolean("HasErrorRecord"))
            {
                this._errorRecord = (System.Management.Automation.ErrorRecord) info.GetValue("ErrorRecord", typeof(System.Management.Automation.ErrorRecord));
            }
        }

        public CmdletInvocationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new PSArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            bool flag = null != this._errorRecord;
            info.AddValue("HasErrorRecord", flag);
            if (flag)
            {
                info.AddValue("ErrorRecord", this._errorRecord);
            }
        }

        public override System.Management.Automation.ErrorRecord ErrorRecord
        {
            get
            {
                return this._errorRecord;
            }
        }
    }
}

