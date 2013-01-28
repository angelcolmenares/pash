namespace System.Data.Services
{
    using System;
    using System.Diagnostics;

    internal sealed class DataServiceProcessingPipelineEventArgs : EventArgs
    {
        private readonly DataServiceOperationContext operationContext;

        internal DataServiceProcessingPipelineEventArgs(DataServiceOperationContext operationContext)
        {
            this.operationContext = operationContext;
        }

        public DataServiceOperationContext OperationContext
        {
            [DebuggerStepThrough]
            get
            {
                return this.operationContext;
            }
        }
    }
}

