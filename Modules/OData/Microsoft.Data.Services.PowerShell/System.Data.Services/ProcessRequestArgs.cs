namespace System.Data.Services
{
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class ProcessRequestArgs
    {
        private readonly bool isBatchOperation;
        private readonly Uri requestUri;

        internal ProcessRequestArgs(Uri requestUri, bool isBatchOperation, DataServiceOperationContext operationContext)
        {
            this.requestUri = requestUri;
            this.isBatchOperation = isBatchOperation;
            this.OperationContext = operationContext;
        }

        public bool IsBatchOperation
        {
            get
            {
                return this.isBatchOperation;
            }
        }

        public DataServiceOperationContext OperationContext { get; private set; }

        public Uri RequestUri
        {
            get
            {
                return this.requestUri;
            }
        }
    }
}

