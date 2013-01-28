namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class HelpCategoryInvalidException : ArgumentException, IContainsErrorRecord
    {
        private System.Management.Automation.ErrorRecord _errorRecord;
        private string _helpCategory;

        public HelpCategoryInvalidException()
        {
            this._helpCategory = System.Management.Automation.HelpCategory.None.ToString();
            this.CreateErrorRecord();
        }

        public HelpCategoryInvalidException(string helpCategory)
        {
            this._helpCategory = System.Management.Automation.HelpCategory.None.ToString();
            this._helpCategory = helpCategory;
            this.CreateErrorRecord();
        }

        protected HelpCategoryInvalidException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._helpCategory = System.Management.Automation.HelpCategory.None.ToString();
            this._helpCategory = info.GetString("HelpCategory");
            this.CreateErrorRecord();
        }

        public HelpCategoryInvalidException(string helpCategory, Exception innerException) : base((innerException != null) ? innerException.Message : string.Empty, innerException)
        {
            this._helpCategory = System.Management.Automation.HelpCategory.None.ToString();
            this._helpCategory = helpCategory;
            this.CreateErrorRecord();
        }

        private void CreateErrorRecord()
        {
            this._errorRecord = new System.Management.Automation.ErrorRecord(new ParentContainsErrorRecordException(this), "HelpCategoryInvalid", ErrorCategory.InvalidArgument, null);
            this._errorRecord.ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "HelpErrors", "HelpCategoryInvalid", new object[] { this._helpCategory });
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw PSTraceSource.NewArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("HelpCategory", this._helpCategory);
        }

        public System.Management.Automation.ErrorRecord ErrorRecord
        {
            get
            {
                return this._errorRecord;
            }
        }

        public string HelpCategory
        {
            get
            {
                return this._helpCategory;
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

