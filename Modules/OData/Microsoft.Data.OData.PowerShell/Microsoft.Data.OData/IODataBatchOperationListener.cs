namespace Microsoft.Data.OData
{
    using System;
    using System.Threading.Tasks;

    internal interface IODataBatchOperationListener
    {
        void BatchOperationContentStreamDisposed();
        void BatchOperationContentStreamRequested();
        Task BatchOperationContentStreamRequestedAsync();
    }
}

