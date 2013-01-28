namespace System.Management.Automation.Runspaces
{
    using System;

    internal sealed class GetRunspaceAsyncResult : AsyncResult
    {
        private bool isActive;
        private System.Management.Automation.Runspaces.Runspace runspace;

        internal GetRunspaceAsyncResult(Guid ownerId, AsyncCallback callback, object state) : base(ownerId, callback, state)
        {
            this.isActive = true;
        }

        internal void DoComplete(object state)
        {
            base.SetAsCompleted(null);
        }

        internal bool IsActive
        {
            get
            {
                lock (base.SyncObject)
                {
                    return this.isActive;
                }
            }
            set
            {
                lock (base.SyncObject)
                {
                    this.isActive = value;
                }
            }
        }

        internal System.Management.Automation.Runspaces.Runspace Runspace
        {
            get
            {
                return this.runspace;
            }
            set
            {
                this.runspace = value;
            }
        }
    }
}

