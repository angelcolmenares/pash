namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Language;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class RuntimeException : SystemException, IContainsErrorRecord
    {
        private ErrorCategory _errorCategory;
        private string _errorId;
        private System.Management.Automation.ErrorRecord _errorRecord;
        private Token _errorToken;
        private string _overrideStackTrace;
        private object _targetObject;
        private bool suppressPromptInInterpreter;
        private bool thrownByThrowStatement;

        public RuntimeException()
        {
            this._errorId = "RuntimeException";
        }

        public RuntimeException(string message) : base(message)
        {
            this._errorId = "RuntimeException";
        }

        protected RuntimeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._errorId = "RuntimeException";
            this._errorId = info.GetString("ErrorId");
            this._errorCategory = (ErrorCategory) info.GetInt32("ErrorCategory");
        }

        public RuntimeException(string message, Exception innerException) : base(message, innerException)
        {
            this._errorId = "RuntimeException";
        }

        public RuntimeException(string message, Exception innerException, System.Management.Automation.ErrorRecord errorRecord) : base(message, innerException)
        {
            this._errorId = "RuntimeException";
            this._errorRecord = errorRecord;
        }

        internal RuntimeException(ErrorCategory errorCategory, InvocationInfo invocationInfo, IScriptExtent errorPosition, string errorIdAndResourceId, string message, Exception innerException) : base(message, innerException)
        {
            this._errorId = "RuntimeException";
            this.SetErrorCategory(errorCategory);
            this.SetErrorId(errorIdAndResourceId);
            if ((errorPosition == null) && (invocationInfo != null))
            {
                errorPosition = invocationInfo.ScriptPosition;
            }
            if (invocationInfo != null)
            {
                this._errorRecord = new System.Management.Automation.ErrorRecord(new ParentContainsErrorRecordException(this), this._errorId, this._errorCategory, this._targetObject);
                this._errorRecord.SetInvocationInfo(new InvocationInfo(invocationInfo.MyCommand, errorPosition));
            }
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new PSArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("ErrorId", this._errorId);
            info.AddValue("ErrorCategory", (int) this._errorCategory);
        }

        internal static void LockStackTrace(Exception e)
        {
            RuntimeException exception = e as RuntimeException;
            if ((exception != null) && string.IsNullOrEmpty(exception._overrideStackTrace))
            {
                string stackTrace = exception.StackTrace;
                if (!string.IsNullOrEmpty(stackTrace))
                {
                    exception._overrideStackTrace = stackTrace;
                }
            }
        }

        internal static Exception RetrieveException(System.Management.Automation.ErrorRecord errorRecord)
        {
            if (errorRecord == null)
            {
                return null;
            }
            return errorRecord.Exception;
        }

        internal static string RetrieveMessage(Exception e)
        {
            if (e == null)
            {
                return "";
            }
            IContainsErrorRecord record = e as IContainsErrorRecord;
            if (record != null)
            {
                System.Management.Automation.ErrorRecord errorRecord = record.ErrorRecord;
                if (errorRecord == null)
                {
                    return e.Message;
                }
                ErrorDetails errorDetails = errorRecord.ErrorDetails;
                if (errorDetails == null)
                {
                    return e.Message;
                }
                string message = errorDetails.Message;
                if (!string.IsNullOrEmpty(message))
                {
                    return message;
                }
            }
            return e.Message;
        }

        internal static string RetrieveMessage(System.Management.Automation.ErrorRecord errorRecord)
        {
            if (errorRecord == null)
            {
                return "";
            }
            if ((errorRecord.ErrorDetails != null) && !string.IsNullOrEmpty(errorRecord.ErrorDetails.Message))
            {
                return errorRecord.ErrorDetails.Message;
            }
            if (errorRecord.Exception == null)
            {
                return "";
            }
            return errorRecord.Exception.Message;
        }

        internal void SetErrorCategory(ErrorCategory errorCategory)
        {
            if (this._errorCategory != errorCategory)
            {
                this._errorCategory = errorCategory;
                this._errorRecord = null;
            }
        }

        internal void SetErrorId(string errorId)
        {
            if (this._errorId != errorId)
            {
                this._errorId = errorId;
                this._errorRecord = null;
            }
        }

        internal void SetTargetObject(object targetObject)
        {
            this._targetObject = targetObject;
            if (this._errorRecord != null)
            {
                this._errorRecord.SetTargetObject(targetObject);
            }
        }

        public virtual System.Management.Automation.ErrorRecord ErrorRecord
        {
            get
            {
                if (this._errorRecord == null)
                {
                    this._errorRecord = new System.Management.Automation.ErrorRecord(new ParentContainsErrorRecordException(this), this._errorId, this._errorCategory, this._targetObject);
                }
                return this._errorRecord;
            }
        }

        internal Token ErrorToken
        {
            get
            {
                return this._errorToken;
            }
            set
            {
                this._errorToken = value;
            }
        }

        public override string StackTrace
        {
            get
            {
                if (!string.IsNullOrEmpty(this._overrideStackTrace))
                {
                    return this._overrideStackTrace;
                }
                return base.StackTrace;
            }
        }

        internal bool SuppressPromptInInterpreter
        {
            get
            {
                return this.suppressPromptInInterpreter;
            }
            set
            {
                this.suppressPromptInInterpreter = value;
            }
        }

        public bool WasThrownFromThrowStatement
        {
            get
            {
                return this.thrownByThrowStatement;
            }
            set
            {
                this.thrownByThrowStatement = value;
                if (this._errorRecord != null)
                {
                    RuntimeException exception = this._errorRecord.Exception as RuntimeException;
                    if (exception != null)
                    {
                        exception.WasThrownFromThrowStatement = value;
                    }
                }
            }
        }
    }
}

