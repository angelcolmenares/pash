namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Runtime.Serialization;

    [Serializable]
    public class RunspaceOpenModuleLoadException : RuntimeException
    {
        private PSDataCollection<ErrorRecord> _errors;

        public RunspaceOpenModuleLoadException() : base(typeof(ScriptBlockToPowerShellNotSupportedException).FullName)
        {
        }

        public RunspaceOpenModuleLoadException(string message) : base(message)
        {
        }

        protected RunspaceOpenModuleLoadException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        internal RunspaceOpenModuleLoadException(string moduleName, PSDataCollection<ErrorRecord> errors) : base(StringUtil.Format(RunspaceStrings.ErrorLoadingModulesOnRunspaceOpen, moduleName, (((errors != null) && (errors.Count > 0)) && (errors[0] != null)) ? errors[0].ToString() : string.Empty), null)
        {
            this._errors = errors;
            base.SetErrorId("ErrorLoadingModulesOnRunspaceOpen");
            base.SetErrorCategory(ErrorCategory.OpenError);
        }

        public RunspaceOpenModuleLoadException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new PSArgumentNullException("info");
            }
            base.GetObjectData(info, context);
        }

        public PSDataCollection<ErrorRecord> ErrorRecords
        {
            get
            {
                return this._errors;
            }
        }
    }
}

