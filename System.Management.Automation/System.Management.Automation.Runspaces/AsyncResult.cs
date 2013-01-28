namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Threading;

    internal class AsyncResult : IAsyncResult
    {
        private AsyncCallback callback;
        private ManualResetEvent completedWaitHandle;
        private System.Exception exception;
        private bool isCompleted;
        private Guid ownerId;
        private object state;
        private object syncObject = new object();

        internal AsyncResult(Guid ownerId, AsyncCallback callback, object state)
        {
            this.ownerId = ownerId;
            this.callback = callback;
            this.state = state;
        }

        internal void EndInvoke()
        {
            this.AsyncWaitHandle.WaitOne();
            this.AsyncWaitHandle.Close();
            this.completedWaitHandle = null;
            if (this.exception != null)
            {
                throw this.exception;
            }
        }

        internal void Release()
        {
            if (!this.isCompleted)
            {
                this.isCompleted = true;
                this.SignalWaitHandle();
            }
        }

        internal void SetAsCompleted(System.Exception exception)
        {
            if (!this.isCompleted)
            {
                lock (this.syncObject)
                {
                    if (this.isCompleted)
                    {
                        return;
                    }
                    this.exception = exception;
                    this.isCompleted = true;
                    this.SignalWaitHandle();
                }
                if (this.callback != null)
                {
                    this.callback(this);
                }
            }
        }

        internal void SignalWaitHandle()
        {
            lock (this.syncObject)
            {
                if (this.completedWaitHandle != null)
                {
                    this.completedWaitHandle.Set();
                }
            }
        }

        public object AsyncState
        {
            get
            {
                return this.state;
            }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (this.completedWaitHandle == null)
                {
                    lock (this.syncObject)
                    {
                        if (this.completedWaitHandle == null)
                        {
                            this.completedWaitHandle = new ManualResetEvent(this.isCompleted);
                        }
                    }
                }
                return this.completedWaitHandle;
            }
        }

        internal AsyncCallback Callback
        {
            get
            {
                return this.callback;
            }
        }

        public bool CompletedSynchronously
        {
            get
            {
                return false;
            }
        }

        internal System.Exception Exception
        {
            get
            {
                return this.exception;
            }
        }

        public bool IsCompleted
        {
            get
            {
                return this.isCompleted;
            }
        }

        internal Guid OwnerId
        {
            get
            {
                return this.ownerId;
            }
        }

        internal object SyncObject
        {
            get
            {
                return this.syncObject;
            }
        }
    }
}

