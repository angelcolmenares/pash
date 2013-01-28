namespace System.Data.Services
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    internal sealed class DataServiceProcessingPipeline
    {
        public event EventHandler<EventArgs> ProcessedChangeset;

        public event EventHandler<DataServiceProcessingPipelineEventArgs> ProcessedRequest;

        public event EventHandler<EventArgs> ProcessingChangeset;

        public event EventHandler<DataServiceProcessingPipelineEventArgs> ProcessingRequest;

        [Conditional("DEBUG")]
        internal void AssertAndUpdateDebugStateAtDispose()
        {
        }

        [Conditional("DEBUG")]
        internal void AssertAndUpdateDebugStateAtGetService()
        {
        }

        [Conditional("DEBUG")]
        internal void AssertAndUpdateDebugStateAtInvokeServiceAction(IDataService dataService)
        {
        }

        [Conditional("DEBUG")]
        internal void AssertAndUpdateDebugStateAtOnStartProcessingRequest()
        {
        }

        [Conditional("DEBUG")]
        internal void AssertAndUpdateDebugStateAtSaveChanges()
        {
        }

        [Conditional("DEBUG")]
        internal void AssertDebugStateAtExecuteExpression(IDataService dataService)
        {
        }

        [Conditional("DEBUG")]
        internal void AssertDebugStateDuringRequestProcessing(DataServiceOperationContext operationContext)
        {
        }

        [Conditional("DEBUG")]
        internal void AssertInitialDebugState()
        {
        }

        internal void InvokeProcessedChangeset(object sender, EventArgs e)
        {
            if (this.ProcessedChangeset != null)
            {
                this.ProcessedChangeset(sender, e);
            }
        }

        internal void InvokeProcessedRequest(object sender, DataServiceProcessingPipelineEventArgs e)
        {
            if (this.ProcessedRequest != null)
            {
                this.ProcessedRequest(sender, e);
            }
        }

        internal void InvokeProcessingChangeset(object sender, EventArgs e)
        {
            if (this.ProcessingChangeset != null)
            {
                this.ProcessingChangeset(sender, e);
            }
        }

        internal void InvokeProcessingRequest(object sender, DataServiceProcessingPipelineEventArgs e)
        {
            if (this.ProcessingRequest != null)
            {
                this.ProcessingRequest(sender, e);
            }
        }

        [Conditional("DEBUG")]
        internal void ResetDebugState()
        {
        }
    }
}

