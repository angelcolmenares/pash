namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;
    using System.Threading;

    [Serializable]
    public class ProviderInvocationException : RuntimeException
    {
        [NonSerialized]
        private System.Management.Automation.ErrorRecord _errorRecord;
        [NonSerialized]
        private string _message;
        [NonSerialized]
        internal System.Management.Automation.ProviderInfo _providerInfo;

        public ProviderInvocationException()
        {
        }

        public ProviderInvocationException(string message) : base(message)
        {
            this._message = message;
        }

        internal ProviderInvocationException(System.Management.Automation.ProviderInfo provider, Exception innerException) : base(RuntimeException.RetrieveMessage(innerException), innerException)
        {
            this._message = base.Message;
            this._providerInfo = provider;
            IContainsErrorRecord record = innerException as IContainsErrorRecord;
            if ((record != null) && (record.ErrorRecord != null))
            {
                this._errorRecord = new System.Management.Automation.ErrorRecord(record.ErrorRecord, innerException);
            }
            else
            {
                this._errorRecord = new System.Management.Automation.ErrorRecord(innerException, "ErrorRecordNotSpecified", ErrorCategory.InvalidOperation, null);
            }
        }

        internal ProviderInvocationException(System.Management.Automation.ProviderInfo provider, System.Management.Automation.ErrorRecord errorRecord) : base(RuntimeException.RetrieveMessage(errorRecord), RuntimeException.RetrieveException(errorRecord))
        {
            if (errorRecord == null)
            {
                throw new ArgumentNullException("errorRecord");
            }
            this._message = base.Message;
            this._providerInfo = provider;
            this._errorRecord = errorRecord;
        }

        protected ProviderInvocationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ProviderInvocationException(string message, Exception innerException) : base(message, innerException)
        {
            this._message = message;
        }

        internal ProviderInvocationException(string errorId, string resourceStr, System.Management.Automation.ProviderInfo provider, string path, Exception innerException) : this(errorId, resourceStr, provider, path, innerException, true)
        {
        }

        internal ProviderInvocationException(string errorId, string resourceStr, System.Management.Automation.ProviderInfo provider, string path, Exception innerException, bool useInnerExceptionMessage) : base(RetrieveMessage(errorId, resourceStr, provider, path, innerException), innerException)
        {
            this._providerInfo = provider;
            this._message = base.Message;
            Exception replaceParentContainsErrorRecordException = null;
            if (useInnerExceptionMessage)
            {
                replaceParentContainsErrorRecordException = innerException;
            }
            else
            {
                replaceParentContainsErrorRecordException = new ParentContainsErrorRecordException(this);
            }
            IContainsErrorRecord record = innerException as IContainsErrorRecord;
            if ((record != null) && (record.ErrorRecord != null))
            {
                this._errorRecord = new System.Management.Automation.ErrorRecord(record.ErrorRecord, replaceParentContainsErrorRecordException);
            }
            else
            {
                this._errorRecord = new System.Management.Automation.ErrorRecord(replaceParentContainsErrorRecordException, errorId, ErrorCategory.InvalidOperation, null);
            }
        }

        private static string RetrieveMessage(string errorId, string resourceStr, System.Management.Automation.ProviderInfo provider, string path, Exception innerException)
        {
            if (innerException == null)
            {
                return "";
            }
            if (string.IsNullOrEmpty(errorId))
            {
                return RuntimeException.RetrieveMessage(innerException);
            }
            if (provider == null)
            {
                return RuntimeException.RetrieveMessage(innerException);
            }
            string str = resourceStr;
            if (string.IsNullOrEmpty(str))
            {
                return RuntimeException.RetrieveMessage(innerException);
            }
            if (path == null)
            {
                return string.Format(Thread.CurrentThread.CurrentCulture, str, new object[] { provider.Name, RuntimeException.RetrieveMessage(innerException) });
            }
            return string.Format(Thread.CurrentThread.CurrentCulture, str, new object[] { provider.Name, path, RuntimeException.RetrieveMessage(innerException) });
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
                if (!string.IsNullOrEmpty(this._message))
                {
                    return this._message;
                }
                return base.Message;
            }
        }

        public System.Management.Automation.ProviderInfo ProviderInfo
        {
            get
            {
                return this._providerInfo;
            }
        }
    }
}

