namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Language;
    using System.Runtime.Serialization;

    [Serializable]
    public class JobFailedException : Exception
    {
        private ScriptExtent displayScriptPosition;
        private Exception reason;

        public JobFailedException()
        {
        }

        public JobFailedException(string message) : base(message)
        {
        }

        public JobFailedException(Exception innerException, ScriptExtent displayScriptPosition)
        {
            this.reason = innerException;
            this.displayScriptPosition = displayScriptPosition;
        }

        protected JobFailedException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
            this.reason = (Exception) serializationInfo.GetValue("Reason", typeof(Exception));
            this.displayScriptPosition = (ScriptExtent) serializationInfo.GetValue("DisplayScriptPosition", typeof(ScriptExtent));
        }

        public JobFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("Reason", this.reason);
            info.AddValue("DisplayScriptPosition", this.displayScriptPosition);
        }

        public ScriptExtent DisplayScriptPosition
        {
            get
            {
                return this.displayScriptPosition;
            }
        }

        public override string Message
        {
            get
            {
                return this.Reason.Message;
            }
        }

        public Exception Reason
        {
            get
            {
                return this.reason;
            }
        }
    }
}

