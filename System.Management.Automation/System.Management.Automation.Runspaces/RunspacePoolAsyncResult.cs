namespace System.Management.Automation.Runspaces
{
    using System;

    internal sealed class RunspacePoolAsyncResult : AsyncResult
    {
        private bool isAssociatedWithAsyncOpen;

        internal RunspacePoolAsyncResult(Guid ownerId, AsyncCallback callback, object state, bool isCalledFromOpenAsync) : base(ownerId, callback, state)
        {
            this.isAssociatedWithAsyncOpen = isCalledFromOpenAsync;
        }

        internal bool IsAssociatedWithAsyncOpen
        {
            get
            {
                return this.isAssociatedWithAsyncOpen;
            }
        }
    }
}

