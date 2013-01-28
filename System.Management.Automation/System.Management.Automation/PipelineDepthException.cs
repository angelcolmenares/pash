namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class PipelineDepthException : SystemException, IContainsErrorRecord
    {
        private System.Management.Automation.ErrorRecord errorRecord;

        public PipelineDepthException() : base(GetErrorText.PipelineDepthException)
        {
        }

        public PipelineDepthException(string message) : base(message)
        {
        }

        protected PipelineDepthException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public PipelineDepthException(string message, Exception innerException) : base(message, innerException)
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

