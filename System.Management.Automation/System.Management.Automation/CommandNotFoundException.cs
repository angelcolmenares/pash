namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;
    using System.Resources;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class CommandNotFoundException : RuntimeException
    {
        private ErrorCategory _errorCategory;
        private string _errorId;
        private System.Management.Automation.ErrorRecord _errorRecord;
        private string commandName;

        public CommandNotFoundException()
        {
            this.commandName = string.Empty;
            this._errorId = "CommandNotFoundException";
            this._errorCategory = ErrorCategory.ObjectNotFound;
        }

        public CommandNotFoundException(string message) : base(message)
        {
            this.commandName = string.Empty;
            this._errorId = "CommandNotFoundException";
            this._errorCategory = ErrorCategory.ObjectNotFound;
        }

        protected CommandNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.commandName = string.Empty;
            this._errorId = "CommandNotFoundException";
            this._errorCategory = ErrorCategory.ObjectNotFound;
            if (info == null)
            {
                throw new PSArgumentNullException("info");
            }
            this.commandName = info.GetString("CommandName");
        }

        public CommandNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
            this.commandName = string.Empty;
            this._errorId = "CommandNotFoundException";
            this._errorCategory = ErrorCategory.ObjectNotFound;
        }

        internal CommandNotFoundException(string commandName, Exception innerException, string errorIdAndResourceId, string resourceStr, params object[] messageArgs) : base(BuildMessage(commandName, errorIdAndResourceId, resourceStr, messageArgs), innerException)
        {
            this.commandName = string.Empty;
            this._errorId = "CommandNotFoundException";
            this._errorCategory = ErrorCategory.ObjectNotFound;
            this.commandName = commandName;
            this._errorId = errorIdAndResourceId;
        }

        private static string BuildMessage(string commandName, string resourceId, string resourceStr, params object[] messageArgs)
        {
            try
            {
                object[] objArray;
                if ((messageArgs != null) && (0 < messageArgs.Length))
                {
                    objArray = new object[messageArgs.Length + 1];
                    objArray[0] = commandName;
                    messageArgs.CopyTo(objArray, 1);
                }
                else
                {
                    objArray = new object[] { commandName };
                }
                return StringUtil.Format(resourceStr, objArray);
            }
            catch (MissingManifestResourceException exception)
            {
                return StringUtil.Format(SessionStateStrings.ResourceStringLoadError, new object[] { commandName, "DiscoveryExceptions", resourceId, exception.Message });
            }
            catch (FormatException exception2)
            {
                return StringUtil.Format(SessionStateStrings.ResourceStringFormatError, new object[] { commandName, "DiscoveryExceptions", resourceId, exception2.Message });
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
            info.AddValue("CommandName", this.commandName);
        }

        public string CommandName
        {
            get
            {
                return this.commandName;
            }
            set
            {
                this.commandName = value;
            }
        }

        public override System.Management.Automation.ErrorRecord ErrorRecord
        {
            get
            {
                if (this._errorRecord == null)
                {
                    this._errorRecord = new System.Management.Automation.ErrorRecord(new ParentContainsErrorRecordException(this), this._errorId, this._errorCategory, this.commandName);
                }
                return this._errorRecord;
            }
        }
    }
}

