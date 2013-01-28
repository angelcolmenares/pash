namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation.Language;
    using System.Management.Automation.Runspaces;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Text;

    [Serializable]
    public class ErrorRecord : ISerializable
    {
        internal string _activityOverride;
        internal ErrorCategory _category;
        private ErrorCategoryInfo _categoryInfo;
        private System.Exception _error;
        private System.Management.Automation.ErrorDetails _errorDetails;
        private string _errorId;
        private System.Management.Automation.InvocationInfo _invocationInfo;
        private bool _isSerialized;
        internal string _reasonOverride;
        private string _scriptStackTrace;
        internal string _serializedErrorCategoryMessageOverride;
        private string _serializedFullyQualifiedErrorId;
        private object _target;
        internal string _targetNameOverride;
        internal string _targetTypeOverride;
        private ReadOnlyCollection<int> pipelineIterationInfo;
        private bool preserveInvocationInfoOnce;
        private bool serializeExtendedInfo;

        private ErrorRecord()
        {
            this.pipelineIterationInfo = new ReadOnlyCollection<int>(new int[0]);
        }

        public ErrorRecord(ErrorRecord errorRecord, System.Exception replaceParentContainsErrorRecordException)
        {
            this.pipelineIterationInfo = new ReadOnlyCollection<int>(new int[0]);
            if (errorRecord == null)
            {
                throw new PSArgumentNullException("errorRecord");
            }
            if ((replaceParentContainsErrorRecordException != null) && (errorRecord.Exception is ParentContainsErrorRecordException))
            {
                this._error = replaceParentContainsErrorRecordException;
            }
            else
            {
                this._error = errorRecord.Exception;
            }
            this._target = errorRecord.TargetObject;
            this._errorId = errorRecord._errorId;
            this._category = errorRecord._category;
            this._activityOverride = errorRecord._activityOverride;
            this._reasonOverride = errorRecord._reasonOverride;
            this._targetNameOverride = errorRecord._targetNameOverride;
            this._targetTypeOverride = errorRecord._targetTypeOverride;
            if (errorRecord.ErrorDetails != null)
            {
                this._errorDetails = new System.Management.Automation.ErrorDetails(errorRecord.ErrorDetails);
            }
            this.SetInvocationInfo(errorRecord._invocationInfo);
            this._scriptStackTrace = errorRecord._scriptStackTrace;
            this._serializedFullyQualifiedErrorId = errorRecord._serializedFullyQualifiedErrorId;
        }

        protected ErrorRecord(SerializationInfo info, StreamingContext context)
        {
            this.pipelineIterationInfo = new ReadOnlyCollection<int>(new int[0]);
            PSObject serializedErrorRecord = PSObject.ConstructPSObjectFromSerializationInfo(info, context);
            this.ConstructFromPSObjectForRemoting(serializedErrorRecord);
        }

        public ErrorRecord(System.Exception exception, string errorId, ErrorCategory errorCategory, object targetObject)
        {
            this.pipelineIterationInfo = new ReadOnlyCollection<int>(new int[0]);
            if (exception == null)
            {
                throw PSTraceSource.NewArgumentNullException("exception");
            }
            if (errorId == null)
            {
                errorId = "";
            }
            this._error = exception;
            this._errorId = errorId;
            this._category = errorCategory;
            this._target = targetObject;
        }

        internal ErrorRecord(System.Exception exception, object targetObject, string fullyQualifiedErrorId, ErrorCategory errorCategory, string errorCategory_Activity, string errorCategory_Reason, string errorCategory_TargetName, string errorCategory_TargetType, string errorCategory_Message, string errorDetails_Message, string errorDetails_RecommendedAction)
        {
            this.pipelineIterationInfo = new ReadOnlyCollection<int>(new int[0]);
            this.PopulateProperties(exception, targetObject, fullyQualifiedErrorId, errorCategory, errorCategory_Activity, errorCategory_Reason, errorCategory_TargetName, errorCategory_TargetType, errorDetails_Message, errorDetails_Message, errorDetails_RecommendedAction);
        }

        private void ConstructFromPSObjectForRemoting(PSObject serializedErrorRecord)
        {
            if (serializedErrorRecord == null)
            {
                throw PSTraceSource.NewArgumentNullException("serializedErrorRecord");
            }
            PSObject propertyValue = RemotingDecoder.GetPropertyValue<PSObject>(serializedErrorRecord, "Exception");
            object targetObject = RemotingDecoder.GetPropertyValue<object>(serializedErrorRecord, "TargetObject");
            PSObject serializedRemoteInvocationInfo = RemotingDecoder.GetPropertyValue<PSObject>(serializedErrorRecord, "InvocationInfo");
            string str = null;
            if (propertyValue != null)
            {
                PSPropertyInfo info = propertyValue.Properties["Message"];
                if (info != null)
                {
                    str = info.Value as string;
                }
            }
            string fullyQualifiedErrorId = RemotingDecoder.GetPropertyValue<string>(serializedErrorRecord, "FullyQualifiedErrorId");
            if (fullyQualifiedErrorId == null)
            {
                fullyQualifiedErrorId = "fullyQualifiedErrorId";
            }
            ErrorCategory errorCategory = RemotingDecoder.GetPropertyValue<ErrorCategory>(serializedErrorRecord, "errorCategory_Category");
            string str3 = RemotingDecoder.GetPropertyValue<string>(serializedErrorRecord, "ErrorCategory_Activity");
            string str4 = RemotingDecoder.GetPropertyValue<string>(serializedErrorRecord, "ErrorCategory_Reason");
            string str5 = RemotingDecoder.GetPropertyValue<string>(serializedErrorRecord, "ErrorCategory_TargetName");
            string str6 = RemotingDecoder.GetPropertyValue<string>(serializedErrorRecord, "ErrorCategory_TargetType");
            string str7 = RemotingDecoder.GetPropertyValue<string>(serializedErrorRecord, "ErrorCategory_Message");
            string noteValue = GetNoteValue(serializedErrorRecord, "ErrorDetails_Message") as string;
            string str9 = GetNoteValue(serializedErrorRecord, "ErrorDetails_RecommendedAction") as string;
            RemoteException exception = new RemoteException((str != null) ? str : str7, propertyValue, serializedRemoteInvocationInfo);
            this.PopulateProperties(exception, targetObject, fullyQualifiedErrorId, errorCategory, str3, str4, str5, str6, str7, noteValue, str9);
            exception.SetRemoteErrorRecord(this);
            this.serializeExtendedInfo = RemotingDecoder.GetPropertyValue<bool>(serializedErrorRecord, "SerializeExtendedInfo");
            if (this.serializeExtendedInfo)
            {
                this._invocationInfo = new System.Management.Automation.InvocationInfo(serializedErrorRecord);
                ArrayList list = RemotingDecoder.GetPropertyValue<ArrayList>(serializedErrorRecord, "PipelineIterationInfo");
                if (list != null)
                {
                    this.pipelineIterationInfo = new ReadOnlyCollection<int>((int[]) list.ToArray(typeof(int)));
                }
            }
            else
            {
                this._invocationInfo = null;
            }
        }

        internal static ErrorRecord FromPSObjectForRemoting(PSObject serializedErrorRecord)
        {
            ErrorRecord record = new ErrorRecord();
            record.ConstructFromPSObjectForRemoting(serializedErrorRecord);
            return record;
        }

        private string GetInvocationTypeName()
        {
            System.Management.Automation.InvocationInfo invocationInfo = this.InvocationInfo;
            if (invocationInfo == null)
            {
                return "";
            }
            CommandInfo myCommand = invocationInfo.MyCommand;
            if (myCommand == null)
            {
                return "";
            }
            if (myCommand is IScriptCommandInfo)
            {
                return myCommand.Name;
            }
            CmdletInfo info4 = myCommand as CmdletInfo;
            if (info4 == null)
            {
                return "";
            }
            return info4.ImplementingType.FullName;
        }

        private static object GetNoteValue(PSObject mshObject, string note)
        {
            PSNoteProperty property = mshObject.Properties[note] as PSNoteProperty;
            if (property != null)
            {
                return property.Value;
            }
            return null;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info != null)
            {
                PSObject dest = RemotingEncoder.CreateEmptyPSObject();
                this.ToPSObjectForRemoting(dest, true);
                dest.GetObjectData(info, context);
            }
        }

        internal void LockScriptStackTrace()
        {
            if (this._scriptStackTrace == null)
            {
                ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
                if (executionContextFromTLS != null)
                {
                    StringBuilder builder = new StringBuilder();
                    IEnumerable<CallStackFrame> callStack = executionContextFromTLS.Debugger.GetCallStack();
                    bool flag = true;
                    foreach (CallStackFrame frame in callStack)
                    {
                        if (!flag)
                        {
                            builder.Append(Environment.NewLine);
                        }
                        flag = false;
                        builder.Append(frame.ToString());
                    }
                    this._scriptStackTrace = builder.ToString();
                }
            }
        }

        internal static string NotNull(string s)
        {
            if (s == null)
            {
                return "";
            }
            return s;
        }

        private void PopulateProperties(System.Exception exception, object targetObject, string fullyQualifiedErrorId, ErrorCategory errorCategory, string errorCategory_Activity, string errorCategory_Reason, string errorCategory_TargetName, string errorCategory_TargetType, string errorCategory_Message, string errorDetails_Message, string errorDetails_RecommendedAction)
        {
            if (exception == null)
            {
                throw PSTraceSource.NewArgumentNullException("exception");
            }
            if (fullyQualifiedErrorId == null)
            {
                throw PSTraceSource.NewArgumentNullException("fullyQualifiedErrorId");
            }
            this._isSerialized = true;
            this._error = exception;
            this._target = targetObject;
            this._serializedFullyQualifiedErrorId = fullyQualifiedErrorId;
            this._category = errorCategory;
            this._activityOverride = errorCategory_Activity;
            this._reasonOverride = errorCategory_Reason;
            this._targetNameOverride = errorCategory_TargetName;
            this._targetTypeOverride = errorCategory_TargetType;
            this._serializedErrorCategoryMessageOverride = errorCategory_Message;
            if (errorDetails_Message != null)
            {
                this._errorDetails = new System.Management.Automation.ErrorDetails(errorDetails_Message);
                if (errorDetails_RecommendedAction != null)
                {
                    this._errorDetails.RecommendedAction = errorDetails_RecommendedAction;
                }
            }
        }

        internal void SetInvocationInfo(System.Management.Automation.InvocationInfo invocationInfo)
        {
            IScriptExtent displayScriptPosition = null;
            if (this._invocationInfo != null)
            {
                displayScriptPosition = this._invocationInfo.DisplayScriptPosition;
            }
            if (invocationInfo != null)
            {
                this._invocationInfo = new System.Management.Automation.InvocationInfo(invocationInfo.MyCommand, invocationInfo.ScriptPosition);
                this._invocationInfo.InvocationName = invocationInfo.InvocationName;
                if (invocationInfo.MyCommand == null)
                {
                    this._invocationInfo.HistoryId = invocationInfo.HistoryId;
                }
            }
            if (displayScriptPosition != null)
            {
                this._invocationInfo.DisplayScriptPosition = displayScriptPosition;
            }
            this.LockScriptStackTrace();
            if ((invocationInfo != null) && (invocationInfo.PipelineIterationInfo != null))
            {
                int[] list = (int[]) invocationInfo.PipelineIterationInfo.Clone();
                this.pipelineIterationInfo = new ReadOnlyCollection<int>(list);
            }
        }

        internal void SetTargetObject(object target)
        {
            this._target = target;
        }

        internal void ToPSObjectForRemoting(PSObject dest)
        {
            this.ToPSObjectForRemoting(dest, this.SerializeExtendedInfo);
        }

        private void ToPSObjectForRemoting(PSObject dest, bool serializeExtInfo)
        {
            RemotingEncoder.ValueGetterDelegate<string> valueGetter = null;
            RemotingEncoder.ValueGetterDelegate<string> delegate3 = null;
            RemotingEncoder.ValueGetterDelegate<object> delegate4 = null;
            RemotingEncoder.AddNoteProperty<System.Exception>(dest, "Exception", () => this.Exception);
            RemotingEncoder.AddNoteProperty<object>(dest, "TargetObject", () => this.TargetObject);
            RemotingEncoder.AddNoteProperty<string>(dest, "FullyQualifiedErrorId", () => this.FullyQualifiedErrorId);
            RemotingEncoder.AddNoteProperty<System.Management.Automation.InvocationInfo>(dest, "InvocationInfo", () => this.InvocationInfo);
            RemotingEncoder.AddNoteProperty<int>(dest, "ErrorCategory_Category", () => (int) this.CategoryInfo.Category);
            RemotingEncoder.AddNoteProperty<string>(dest, "ErrorCategory_Activity", () => this.CategoryInfo.Activity);
            RemotingEncoder.AddNoteProperty<string>(dest, "ErrorCategory_Reason", () => this.CategoryInfo.Reason);
            RemotingEncoder.AddNoteProperty<string>(dest, "ErrorCategory_TargetName", () => this.CategoryInfo.TargetName);
            RemotingEncoder.AddNoteProperty<string>(dest, "ErrorCategory_TargetType", () => this.CategoryInfo.TargetType);
            RemotingEncoder.AddNoteProperty<string>(dest, "ErrorCategory_Message", () => this.CategoryInfo.GetMessage(CultureInfo.CurrentCulture));
            if (this.ErrorDetails != null)
            {
                if (valueGetter == null)
                {
                    valueGetter = () => this.ErrorDetails.Message;
                }
                RemotingEncoder.AddNoteProperty<string>(dest, "ErrorDetails_Message", valueGetter);
                if (delegate3 == null)
                {
                    delegate3 = () => this.ErrorDetails.RecommendedAction;
                }
                RemotingEncoder.AddNoteProperty<string>(dest, "ErrorDetails_RecommendedAction", delegate3);
            }
            if (!serializeExtInfo || (this.InvocationInfo == null))
            {
                RemotingEncoder.AddNoteProperty<bool>(dest, "SerializeExtendedInfo", () => false);
            }
            else
            {
                RemotingEncoder.AddNoteProperty<bool>(dest, "SerializeExtendedInfo", () => true);
                this.InvocationInfo.ToPSObjectForRemoting(dest);
                if (delegate4 == null)
                {
                    delegate4 = () => this.PipelineIterationInfo;
                }
                RemotingEncoder.AddNoteProperty<object>(dest, "PipelineIterationInfo", delegate4);
            }
        }

        public override string ToString()
        {
            if ((this.ErrorDetails != null) && !string.IsNullOrEmpty(this.ErrorDetails.Message))
            {
                return this.ErrorDetails.Message;
            }
            if (this.Exception == null)
            {
                return base.ToString();
            }
            if (!string.IsNullOrEmpty(this.Exception.Message))
            {
                return this.Exception.Message;
            }
            return this.Exception.ToString();
        }

        internal virtual ErrorRecord WrapException(System.Exception replaceParentContainsErrorRecordException)
        {
            return new ErrorRecord(this, replaceParentContainsErrorRecordException);
        }

        public ErrorCategoryInfo CategoryInfo
        {
            get
            {
                if (this._categoryInfo == null)
                {
                    this._categoryInfo = new ErrorCategoryInfo(this);
                }
                return this._categoryInfo;
            }
        }

        public System.Management.Automation.ErrorDetails ErrorDetails
        {
            get
            {
                return this._errorDetails;
            }
            set
            {
                this._errorDetails = value;
            }
        }

        public System.Exception Exception
        {
            get
            {
                return this._error;
            }
        }

        public string FullyQualifiedErrorId
        {
            get
            {
                if (this._serializedFullyQualifiedErrorId != null)
                {
                    return this._serializedFullyQualifiedErrorId;
                }
                string invocationTypeName = this.GetInvocationTypeName();
                string str2 = (string.IsNullOrEmpty(invocationTypeName) || string.IsNullOrEmpty(this._errorId)) ? "" : ",";
                return (NotNull(this._errorId) + str2 + NotNull(invocationTypeName));
            }
        }

        public System.Management.Automation.InvocationInfo InvocationInfo
        {
            get
            {
                return this._invocationInfo;
            }
        }

        internal bool IsSerialized
        {
            get
            {
                return this._isSerialized;
            }
        }

        public ReadOnlyCollection<int> PipelineIterationInfo
        {
            get
            {
                return this.pipelineIterationInfo;
            }
        }

        internal bool PreserveInvocationInfoOnce
        {
            get
            {
                return this.preserveInvocationInfoOnce;
            }
            set
            {
                this.preserveInvocationInfoOnce = value;
            }
        }

        public string ScriptStackTrace
        {
            get
            {
                return this._scriptStackTrace;
            }
        }

        internal bool SerializeExtendedInfo
        {
            get
            {
                return this.serializeExtendedInfo;
            }
            set
            {
                this.serializeExtendedInfo = value;
            }
        }

        public object TargetObject
        {
            get
            {
                return this._target;
            }
        }
    }
}

