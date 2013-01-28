namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class PSInvalidCastException : InvalidCastException, IContainsErrorRecord
    {
        internal const string BaseName = "ExtendedTypeSystem";
        private string errorId;
        private System.Management.Automation.ErrorRecord errorRecord;
        internal const string InvalidCastCannotRetrieveStringMsg = "InvalidCastCannotRetrieveString";
        internal const string InvalidCastExceptionEnumerationMoreThanOneValueMsg = "InvalidCastExceptionEnumerationMoreThanOneValue";
        internal const string InvalidCastExceptionEnumerationNoFlagAndCommaMsg = "InvalidCastExceptionEnumerationNoFlagAndComma";
        internal const string InvalidCastExceptionEnumerationNoValueMsg = "InvalidCastExceptionEnumerationNoValue";
        internal const string InvalidCastExceptionEnumerationNullMsg = "InvalidCastExceptionEnumerationNull";
        internal const string InvalidCastExceptionMsg = "InvalidCastException";
        internal const string InvalidCastExceptionNoStringForConversionMsg = "InvalidCastExceptionNoStringForConversion";
        internal const string InvalidCastExceptionWithInnerExceptionMsg = "InvalidCastExceptionWithInnerException";
        internal const string InvalidCastFromNullMsg = "InvalidCastFromNull";
        internal const string ListSeparatorMsg = "ListSeparator";

        public PSInvalidCastException() : base(typeof(PSInvalidCastException).FullName)
        {
            this.errorId = "PSInvalidCastException";
        }

        public PSInvalidCastException(string message) : base(message)
        {
            this.errorId = "PSInvalidCastException";
        }

        protected PSInvalidCastException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.errorId = "PSInvalidCastException";
            this.errorId = info.GetString("ErrorId");
        }

        public PSInvalidCastException(string message, Exception innerException) : base(message, innerException)
        {
            this.errorId = "PSInvalidCastException";
        }

        internal PSInvalidCastException(string errorId, Exception innerException, string resourceString, params object[] arguments) : base(StringUtil.Format(resourceString, arguments), innerException)
        {
            this.errorId = "PSInvalidCastException";
            this.errorId = errorId;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new PSArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("ErrorId", this.errorId);
        }

        public System.Management.Automation.ErrorRecord ErrorRecord
        {
            get
            {
                if (this.errorRecord == null)
                {
                    this.errorRecord = new System.Management.Automation.ErrorRecord(new ParentContainsErrorRecordException(this), this.errorId, ErrorCategory.InvalidArgument, null);
                }
                return this.errorRecord;
            }
        }
    }
}

