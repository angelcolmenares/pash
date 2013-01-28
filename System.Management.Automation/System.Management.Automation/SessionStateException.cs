namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;
    using System.Resources;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class SessionStateException : RuntimeException
    {
        private ErrorCategory _errorCategory;
        private string _errorId;
        private System.Management.Automation.ErrorRecord _errorRecord;
        private string _itemName;
        private System.Management.Automation.SessionStateCategory _sessionStateCategory;

        public SessionStateException()
        {
            this._itemName = string.Empty;
            this._errorId = "SessionStateException";
            this._errorCategory = ErrorCategory.InvalidArgument;
        }

        public SessionStateException(string message) : base(message)
        {
            this._itemName = string.Empty;
            this._errorId = "SessionStateException";
            this._errorCategory = ErrorCategory.InvalidArgument;
        }

        protected SessionStateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._itemName = string.Empty;
            this._errorId = "SessionStateException";
            this._errorCategory = ErrorCategory.InvalidArgument;
            this._sessionStateCategory = (System.Management.Automation.SessionStateCategory) info.GetInt32("SessionStateCategory");
        }

        public SessionStateException(string message, Exception innerException) : base(message, innerException)
        {
            this._itemName = string.Empty;
            this._errorId = "SessionStateException";
            this._errorCategory = ErrorCategory.InvalidArgument;
        }

        internal SessionStateException(string itemName, System.Management.Automation.SessionStateCategory sessionStateCategory, string errorIdAndResourceId, string resourceStr, ErrorCategory errorCategory, params object[] messageArgs) : base(BuildMessage(itemName, errorIdAndResourceId, resourceStr, messageArgs))
        {
            this._itemName = string.Empty;
            this._errorId = "SessionStateException";
            this._errorCategory = ErrorCategory.InvalidArgument;
            this._itemName = itemName;
            this._sessionStateCategory = sessionStateCategory;
            this._errorId = errorIdAndResourceId;
            this._errorCategory = errorCategory;
        }

        private static string BuildMessage(string itemName, string resourceId, string resourceStr, params object[] messageArgs)
        {
            try
            {
                object[] objArray;
                if ((messageArgs != null) && (0 < messageArgs.Length))
                {
                    objArray = new object[messageArgs.Length + 1];
                    objArray[0] = itemName;
                    messageArgs.CopyTo(objArray, 1);
                }
                else
                {
                    objArray = new object[] { itemName };
                }
                return StringUtil.Format(resourceStr, objArray);
            }
            catch (MissingManifestResourceException exception)
            {
                return StringUtil.Format(SessionStateStrings.ResourceStringLoadError, new object[] { itemName, "SessionStateStrings", resourceId, exception.Message });
            }
            catch (FormatException exception2)
            {
                return StringUtil.Format(SessionStateStrings.ResourceStringFormatError, new object[] { itemName, "SessionStateStrings", resourceId, exception2.Message });
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
            info.AddValue("SessionStateCategory", (int) this._sessionStateCategory);
        }

        public override System.Management.Automation.ErrorRecord ErrorRecord
        {
            get
            {
                if (this._errorRecord == null)
                {
                    this._errorRecord = new System.Management.Automation.ErrorRecord(new ParentContainsErrorRecordException(this), this._errorId, this._errorCategory, this._itemName);
                }
                return this._errorRecord;
            }
        }

        public string ItemName
        {
            get
            {
                return this._itemName;
            }
        }

        public System.Management.Automation.SessionStateCategory SessionStateCategory
        {
            get
            {
                return this._sessionStateCategory;
            }
        }
    }
}

