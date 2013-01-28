namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class ScriptCallDepthException : SystemException, IContainsErrorRecord
    {
        private System.Management.Automation.ErrorRecord errorRecord;

        public ScriptCallDepthException() : base(GetErrorText.ScriptCallDepthException)
        {
        }

        public ScriptCallDepthException(string message) : base(message)
        {
        }

        protected ScriptCallDepthException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ScriptCallDepthException(string message, Exception innerException) : base(message, innerException)
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        public int CallDepth
        {
            get
            {
                return 0;
            }
        }

        public System.Management.Automation.ErrorRecord ErrorRecord
        {
            get
            {
                if (this.errorRecord == null)
                {
                    this.errorRecord = new System.Management.Automation.ErrorRecord(new ParentContainsErrorRecordException(this), "CallDepthOverflow", ErrorCategory.InvalidOperation, this.CallDepth);
                }
                return this.errorRecord;
            }
        }
    }
}

