namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class ParentContainsErrorRecordException : SystemException
    {
        private string message;
        private readonly Exception wrapperException;

        public ParentContainsErrorRecordException()
        {
        }

        public ParentContainsErrorRecordException(Exception wrapperException)
        {
            this.wrapperException = wrapperException;
        }

        public ParentContainsErrorRecordException(string message)
        {
            this.message = message;
        }

        protected ParentContainsErrorRecordException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.message = info.GetString("ParentContainsErrorRecordException_Message");
        }

        public ParentContainsErrorRecordException(string message, Exception innerException) : base(message, innerException)
        {
            this.message = message;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new PSArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("ParentContainsErrorRecordException_Message", this.Message);
        }

        public override string Message
        {
            get
            {
                if (this.message == null)
                {
                    this.message = (this.wrapperException != null) ? this.wrapperException.Message : string.Empty;
                }
                return this.message;
            }
        }
    }
}

