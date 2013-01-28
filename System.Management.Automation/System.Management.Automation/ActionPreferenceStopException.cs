namespace System.Management.Automation
{
    using System;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class ActionPreferenceStopException : RuntimeException
    {
        private System.Management.Automation.ErrorRecord _errorRecord;

        public ActionPreferenceStopException() : this(GetErrorText.ActionPreferenceStop)
        {
        }

        internal ActionPreferenceStopException(System.Management.Automation.ErrorRecord error) : this(RuntimeException.RetrieveMessage(error))
        {
            if (error == null)
            {
                throw new ArgumentNullException("error");
            }
            this._errorRecord = error;
        }

        public ActionPreferenceStopException(string message) : base(message)
        {
            base.SetErrorCategory(ErrorCategory.OperationStopped);
            base.SetErrorId("ActionPreferenceStop");
            base.SuppressPromptInInterpreter = true;
        }

        protected ActionPreferenceStopException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            if (info.GetBoolean("HasErrorRecord"))
            {
                this._errorRecord = (System.Management.Automation.ErrorRecord) info.GetValue("ErrorRecord", typeof(System.Management.Automation.ErrorRecord));
            }
            base.SuppressPromptInInterpreter = true;
        }

        public ActionPreferenceStopException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCategory(ErrorCategory.OperationStopped);
            base.SetErrorId("ActionPreferenceStop");
            base.SuppressPromptInInterpreter = true;
        }

        internal ActionPreferenceStopException(InvocationInfo invocationInfo, string baseName, string resourceId, params object[] args) : this(ResourceManagerCache.FormatResourceString(Assembly.GetCallingAssembly(), baseName, resourceId, args))
        {
            this.ErrorRecord.SetInvocationInfo(invocationInfo);
        }

        internal ActionPreferenceStopException(InvocationInfo invocationInfo, System.Management.Automation.ErrorRecord errorRecord, string baseName, string resourceId, params object[] args) : this(invocationInfo, baseName, resourceId, args)
        {
            if (errorRecord == null)
            {
                throw new ArgumentNullException("errorRecord");
            }
            this._errorRecord = errorRecord;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            if (info != null)
            {
                bool flag = null != this._errorRecord;
                info.AddValue("HasErrorRecord", flag);
                if (flag)
                {
                    info.AddValue("ErrorRecord", this._errorRecord);
                }
            }
            base.SuppressPromptInInterpreter = true;
        }

        public override System.Management.Automation.ErrorRecord ErrorRecord
        {
            get
            {
                if (this._errorRecord == null)
                {
                    return base.ErrorRecord;
                }
                return this._errorRecord;
            }
        }
    }
}

