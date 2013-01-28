namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;

    public class WriteOrThrowErrorCommand : PSCmdlet
    {
        private string _activity = "";
        private ErrorCategory _category;
        private string _errorId = "";
        private System.Management.Automation.ErrorRecord _errorRecord;
        private string _message;
        private string _reason = "";
        private string _recommendedAction = "";
        private System.Exception _recordException;
        private string _targetName = "";
        private object _targetObject;
        private string _targetType = "";

        protected override void ProcessRecord()
        {
            System.Management.Automation.ErrorRecord errorRecord = this.ErrorRecord;
            if (errorRecord != null)
            {
                errorRecord = new System.Management.Automation.ErrorRecord(errorRecord, null);
            }
            else
            {
                System.Exception exception = this.Exception;
                string message = this.Message;
                if (exception == null)
                {
                    exception = new WriteErrorException(message);
                }
                string errorId = this.ErrorId;
                if (string.IsNullOrEmpty(errorId))
                {
                    errorId = exception.GetType().FullName;
                }
                errorRecord = new System.Management.Automation.ErrorRecord(exception, errorId, this.Category, this.TargetObject);
                if ((this.Exception != null) && !string.IsNullOrEmpty(message))
                {
                    errorRecord.ErrorDetails = new ErrorDetails(message);
                }
            }
            string recommendedAction = this.RecommendedAction;
            if (!string.IsNullOrEmpty(recommendedAction))
            {
                if (errorRecord.ErrorDetails == null)
                {
                    errorRecord.ErrorDetails = new ErrorDetails(errorRecord.ToString());
                }
                errorRecord.ErrorDetails.RecommendedAction = recommendedAction;
            }
            if (!string.IsNullOrEmpty(this.CategoryActivity))
            {
                errorRecord.CategoryInfo.Activity = this.CategoryActivity;
            }
            if (!string.IsNullOrEmpty(this.CategoryReason))
            {
                errorRecord.CategoryInfo.Reason = this.CategoryReason;
            }
            if (!string.IsNullOrEmpty(this.CategoryTargetName))
            {
                errorRecord.CategoryInfo.TargetName = this.CategoryTargetName;
            }
            if (!string.IsNullOrEmpty(this.CategoryTargetType))
            {
                errorRecord.CategoryInfo.TargetType = this.CategoryTargetType;
            }
            InvocationInfo variableValue = base.GetVariableValue("MyInvocation") as InvocationInfo;
            if (variableValue != null)
            {
                errorRecord.SetInvocationInfo(variableValue);
                errorRecord.PreserveInvocationInfoOnce = true;
                errorRecord.CategoryInfo.Activity = "Write-Error";
            }
            base.WriteError(errorRecord);
        }

        [Parameter(ParameterSetName="WithException"), Parameter(ParameterSetName="NoException")]
        public ErrorCategory Category
        {
            get
            {
                return this._category;
            }
            set
            {
                this._category = value;
            }
        }

        [Alias(new string[] { "Activity" }), Parameter]
        public string CategoryActivity
        {
            get
            {
                return this._activity;
            }
            set
            {
                this._activity = value;
            }
        }

        [Parameter, Alias(new string[] { "Reason" })]
        public string CategoryReason
        {
            get
            {
                return this._reason;
            }
            set
            {
                this._reason = value;
            }
        }

        [Alias(new string[] { "TargetName" }), Parameter]
        public string CategoryTargetName
        {
            get
            {
                return this._targetName;
            }
            set
            {
                this._targetName = value;
            }
        }

        [Alias(new string[] { "TargetType" }), Parameter]
        public string CategoryTargetType
        {
            get
            {
                return this._targetType;
            }
            set
            {
                this._targetType = value;
            }
        }

        [Parameter(ParameterSetName="WithException"), Parameter(ParameterSetName="NoException")]
        public string ErrorId
        {
            get
            {
                return this._errorId;
            }
            set
            {
                this._errorId = value;
            }
        }

        [Parameter(ParameterSetName="ErrorRecord", Mandatory=true)]
        public System.Management.Automation.ErrorRecord ErrorRecord
        {
            get
            {
                return this._errorRecord;
            }
            set
            {
                this._errorRecord = value;
            }
        }

        [Parameter(ParameterSetName="WithException", Mandatory=true)]
        public System.Exception Exception
        {
            get
            {
                return this._recordException;
            }
            set
            {
                this._recordException = value;
            }
        }

        [Parameter(ParameterSetName="WithException"), AllowEmptyString, AllowNull, Parameter(Position=0, ParameterSetName="NoException", Mandatory=true, ValueFromPipeline=true), Alias(new string[] { "Msg" })]
        public string Message
        {
            get
            {
                return this._message;
            }
            set
            {
                this._message = value;
            }
        }

        [Parameter]
        public string RecommendedAction
        {
            get
            {
                return this._recommendedAction;
            }
            set
            {
                this._recommendedAction = value;
            }
        }

        [Parameter(ParameterSetName="NoException"), Parameter(ParameterSetName="WithException")]
        public object TargetObject
        {
            get
            {
                return this._targetObject;
            }
            set
            {
                this._targetObject = value;
            }
        }
    }
}

