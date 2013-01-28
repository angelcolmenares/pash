namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class PipelineClosedException : RuntimeException
    {
        public PipelineClosedException()
        {
        }

        public PipelineClosedException(string message) : base(message)
        {
        }

        protected PipelineClosedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public PipelineClosedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

