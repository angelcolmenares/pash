namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class HelpNotFoundException : SystemException, IContainsErrorRecord
    {
        private System.Management.Automation.ErrorRecord _errorRecord;
        private string _helpTopic;

        public HelpNotFoundException()
        {
            this._helpTopic = "";
            this.CreateErrorRecord();
        }

        public HelpNotFoundException(string helpTopic)
        {
            this._helpTopic = "";
            this._helpTopic = helpTopic;
            this.CreateErrorRecord();
        }

        protected HelpNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._helpTopic = "";
            this._helpTopic = info.GetString("HelpTopic");
            this.CreateErrorRecord();
        }

        public HelpNotFoundException(string helpTopic, Exception innerException) : base((innerException != null) ? innerException.Message : string.Empty, innerException)
        {
            this._helpTopic = "";
            this._helpTopic = helpTopic;
            this.CreateErrorRecord();
        }

        private void CreateErrorRecord()
        {
            this._errorRecord = new System.Management.Automation.ErrorRecord(new ParentContainsErrorRecordException(this), "HelpNotFound", ErrorCategory.ResourceUnavailable, null);
            this._errorRecord.ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "HelpErrors", "HelpNotFound", new object[] { this._helpTopic });
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw PSTraceSource.NewArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("HelpTopic", this._helpTopic);
        }

        public System.Management.Automation.ErrorRecord ErrorRecord
        {
            get
            {
                return this._errorRecord;
            }
        }

        public string HelpTopic
        {
            get
            {
                return this._helpTopic;
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

