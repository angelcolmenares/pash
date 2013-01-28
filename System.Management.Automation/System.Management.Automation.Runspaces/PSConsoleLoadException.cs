namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Text;

    [Serializable]
    public class PSConsoleLoadException : SystemException, IContainsErrorRecord
    {
        private string _consoleFileName;
        private System.Management.Automation.ErrorRecord _errorRecord;
        private Collection<PSSnapInException> _PSSnapInExceptions;

        public PSConsoleLoadException()
        {
            this._consoleFileName = "";
            this._PSSnapInExceptions = new Collection<PSSnapInException>();
        }

        public PSConsoleLoadException(string message) : base(message)
        {
            this._consoleFileName = "";
            this._PSSnapInExceptions = new Collection<PSSnapInException>();
        }

        internal PSConsoleLoadException(MshConsoleInfo consoleInfo, Collection<PSSnapInException> exceptions)
        {
            this._consoleFileName = "";
            this._PSSnapInExceptions = new Collection<PSSnapInException>();
            if (!string.IsNullOrEmpty(consoleInfo.Filename))
            {
                this._consoleFileName = consoleInfo.Filename;
            }
            if (exceptions != null)
            {
                this._PSSnapInExceptions = exceptions;
            }
            this.CreateErrorRecord();
        }

        protected PSConsoleLoadException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._consoleFileName = "";
            this._PSSnapInExceptions = new Collection<PSSnapInException>();
            this._consoleFileName = info.GetString("ConsoleFileName");
            this.CreateErrorRecord();
        }

        public PSConsoleLoadException(string message, Exception innerException) : base(message, innerException)
        {
            this._consoleFileName = "";
            this._PSSnapInExceptions = new Collection<PSSnapInException>();
        }

        private void CreateErrorRecord()
        {
            StringBuilder builder = new StringBuilder();
            if (this.PSSnapInExceptions != null)
            {
                foreach (PSSnapInException exception in this.PSSnapInExceptions)
                {
                    builder.Append("\n");
                    builder.Append(exception.Message);
                }
            }
            this._errorRecord = new System.Management.Automation.ErrorRecord(new ParentContainsErrorRecordException(this), "ConsoleLoadFailure", ErrorCategory.ResourceUnavailable, null);
            this._errorRecord.ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "ConsoleInfoErrorStrings", "ConsoleLoadFailure", new object[] { this._consoleFileName, builder.ToString() });
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw PSTraceSource.NewArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("ConsoleFileName", this._consoleFileName);
        }

        public System.Management.Automation.ErrorRecord ErrorRecord
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

        internal Collection<PSSnapInException> PSSnapInExceptions
        {
            get
            {
                return this._PSSnapInExceptions;
            }
        }
    }
}

