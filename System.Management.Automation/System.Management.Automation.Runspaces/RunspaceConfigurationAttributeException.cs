namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class RunspaceConfigurationAttributeException : SystemException, IContainsErrorRecord
    {
        private string _assemblyName;
        private string _error;
        private System.Management.Automation.ErrorRecord _errorRecord;

        public RunspaceConfigurationAttributeException()
        {
            this._error = "";
            this._assemblyName = "";
        }

        public RunspaceConfigurationAttributeException(string message) : base(message)
        {
            this._error = "";
            this._assemblyName = "";
        }

        protected RunspaceConfigurationAttributeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._error = "";
            this._assemblyName = "";
            this._error = info.GetString("Error");
            this._assemblyName = info.GetString("AssemblyName");
            this.CreateErrorRecord();
        }

        public RunspaceConfigurationAttributeException(string message, Exception innerException) : base(message, innerException)
        {
            this._error = "";
            this._assemblyName = "";
        }

        internal RunspaceConfigurationAttributeException(string error, string assemblyName)
        {
            this._error = "";
            this._assemblyName = "";
            this._error = error;
            this._assemblyName = assemblyName;
            this.CreateErrorRecord();
        }

        internal RunspaceConfigurationAttributeException(string error, string assemblyName, Exception innerException) : base(innerException.Message, innerException)
        {
            this._error = "";
            this._assemblyName = "";
            this._error = error;
            this._assemblyName = assemblyName;
            this.CreateErrorRecord();
        }

        private void CreateErrorRecord()
        {
            if (!string.IsNullOrEmpty(this._error) && !string.IsNullOrEmpty(this._assemblyName))
            {
                this._errorRecord = new System.Management.Automation.ErrorRecord(new ParentContainsErrorRecordException(this), this._error, ErrorCategory.ResourceUnavailable, null);
                this._errorRecord.ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "MiniShellErrors", this._error, new object[] { this._assemblyName });
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
            info.AddValue("Error", this._error);
            info.AddValue("AssemblyName", this._assemblyName);
        }

        public string AssemblyName
        {
            get
            {
                return this._assemblyName;
            }
        }

        public string Error
        {
            get
            {
                return this._error;
            }
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
    }
}

