namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class RunspaceConfigurationTypeException : SystemException, IContainsErrorRecord
    {
        private string _assemblyName;
        private System.Management.Automation.ErrorRecord _errorRecord;
        private string _typeName;

        public RunspaceConfigurationTypeException()
        {
            this._assemblyName = "";
            this._typeName = "";
        }

        public RunspaceConfigurationTypeException(string message) : base(message)
        {
            this._assemblyName = "";
            this._typeName = "";
        }

        protected RunspaceConfigurationTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._assemblyName = "";
            this._typeName = "";
            this._typeName = info.GetString("TypeName");
            this._assemblyName = info.GetString("AssemblyName");
        }

        public RunspaceConfigurationTypeException(string message, Exception innerException) : base(message, innerException)
        {
            this._assemblyName = "";
            this._typeName = "";
        }

        internal RunspaceConfigurationTypeException(string assemblyName, string typeName)
        {
            this._assemblyName = "";
            this._typeName = "";
            this._assemblyName = assemblyName;
            this._typeName = typeName;
            this.CreateErrorRecord();
        }

        internal RunspaceConfigurationTypeException(string assemblyName, string typeName, Exception innerException) : base(innerException.Message, innerException)
        {
            this._assemblyName = "";
            this._typeName = "";
            this._assemblyName = assemblyName;
            this._typeName = typeName;
            this.CreateErrorRecord();
        }

        private void CreateErrorRecord()
        {
            if (!string.IsNullOrEmpty(this._assemblyName) && !string.IsNullOrEmpty(this._typeName))
            {
                this._errorRecord = new System.Management.Automation.ErrorRecord(new ParentContainsErrorRecordException(this), "UndefinedRunspaceConfigurationType", ErrorCategory.ResourceUnavailable, null);
                this._errorRecord.ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "MiniShellErrors", "UndefinedRunspaceConfigurationType", new object[] { this._assemblyName, this._typeName });
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
            info.AddValue("TypeName", this._typeName);
            info.AddValue("AssemblyName", this._assemblyName);
        }

        public string AssemblyName
        {
            get
            {
                return this._assemblyName;
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

        public string TypeName
        {
            get
            {
                return this._typeName;
            }
        }
    }
}

