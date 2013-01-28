namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class PSSnapInException : RuntimeException
    {
        private System.Management.Automation.ErrorRecord _errorRecord;
        private string _PSSnapin;
        private string _reason;
        private bool _warning;

        public PSSnapInException()
        {
            this._PSSnapin = "";
            this._reason = "";
        }

        public PSSnapInException(string message) : base(message)
        {
            this._PSSnapin = "";
            this._reason = "";
        }

        protected PSSnapInException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._PSSnapin = "";
            this._reason = "";
            this._PSSnapin = info.GetString("PSSnapIn");
            this._reason = info.GetString("Reason");
            this.CreateErrorRecord();
        }

        public PSSnapInException(string message, Exception innerException) : base(message, innerException)
        {
            this._PSSnapin = "";
            this._reason = "";
        }

        internal PSSnapInException(string PSSnapin, string message)
        {
            this._PSSnapin = "";
            this._reason = "";
            this._PSSnapin = PSSnapin;
            this._reason = message;
            this.CreateErrorRecord();
        }

        internal PSSnapInException(string PSSnapin, string message, bool warning)
        {
            this._PSSnapin = "";
            this._reason = "";
            this._PSSnapin = PSSnapin;
            this._reason = message;
            this._warning = warning;
            this.CreateErrorRecord();
        }

        internal PSSnapInException(string PSSnapin, string message, Exception exception) : base(message, exception)
        {
            this._PSSnapin = "";
            this._reason = "";
            this._PSSnapin = PSSnapin;
            this._reason = message;
            this.CreateErrorRecord();
        }

        private void CreateErrorRecord()
        {
            if (!string.IsNullOrEmpty(this._PSSnapin) && !string.IsNullOrEmpty(this._reason))
            {
                if (this._warning)
                {
                    this._errorRecord = new System.Management.Automation.ErrorRecord(new ParentContainsErrorRecordException(this), "PSSnapInLoadWarning", ErrorCategory.ResourceUnavailable, null);
                    this._errorRecord.ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "ConsoleInfoErrorStrings", "PSSnapInLoadWarning", new object[] { this._PSSnapin, this._reason });
                }
                else
                {
                    this._errorRecord = new System.Management.Automation.ErrorRecord(new ParentContainsErrorRecordException(this), "PSSnapInLoadFailure", ErrorCategory.ResourceUnavailable, null);
                    this._errorRecord.ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "ConsoleInfoErrorStrings", "PSSnapInLoadFailure", new object[] { this._PSSnapin, this._reason });
                }
            }
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw PSTraceSource.NewArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("PSSnapIn", this._PSSnapin);
            info.AddValue("Reason", this._reason);
        }

        public override System.Management.Automation.ErrorRecord ErrorRecord
        {
            get
            {
                return this._errorRecord;
            }
        }

        public override string Message
        {
            get
            {
                if (this._errorRecord != null)
                {
                    return this._errorRecord.ToString();
                }
                return base.Message;
            }
        }
    }
}

