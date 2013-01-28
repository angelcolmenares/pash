namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Runspaces;

    internal sealed class PowerShellAsyncResult : AsyncResult
    {
        private bool isAssociatedWithAsyncInvoke;
        private PSDataCollection<PSObject> output;

        internal PowerShellAsyncResult(Guid ownerId, AsyncCallback callback, object state, PSDataCollection<PSObject> output, bool isCalledFromBeginInvoke) : base(ownerId, callback, state)
        {
            this.isAssociatedWithAsyncInvoke = isCalledFromBeginInvoke;
            this.output = output;
        }

        internal bool IsAssociatedWithAsyncInvoke
        {
            get
            {
                return this.isAssociatedWithAsyncInvoke;
            }
        }

        internal PSDataCollection<PSObject> Output
        {
            get
            {
                return this.output;
            }
        }
    }
}

