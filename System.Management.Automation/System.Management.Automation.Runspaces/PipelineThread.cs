namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Threading;

    internal class PipelineThread : IDisposable
    {
        private bool closed;
        private Thread worker;
        private ThreadStart workItem;
        private AutoResetEvent workItemReady;

        internal PipelineThread(ApartmentState apartmentState)
        {
            this.worker = new Thread(new ThreadStart(this.WorkerProc), LocalPipeline.MaxStack);
            this.workItem = null;
            this.workItemReady = new AutoResetEvent(false);
            this.closed = false;
            if (apartmentState != ApartmentState.Unknown)
            {
                this.worker.SetApartmentState(apartmentState);
            }
        }

        internal void Close()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            if (!this.closed)
            {
                this.closed = true;
                this.workItemReady.Set();
                if ((this.worker.ThreadState != ThreadState.Unstarted) && (Thread.CurrentThread != this.worker))
                {
                    this.worker.Join();
                }
                this.workItemReady.Close();
                GC.SuppressFinalize(this);
            }
        }

        ~PipelineThread()
        {
            this.Dispose();
        }

        internal void Start(ThreadStart workItem)
        {
            if (!this.closed)
            {
                this.workItem = workItem;
                this.workItemReady.Set();
                if (this.worker.ThreadState == ThreadState.Unstarted)
                {
                    this.worker.Start();
                }
            }
        }

        private void WorkerProc()
        {
            while (!this.closed)
            {
                this.workItemReady.WaitOne();
                if (!this.closed)
                {
                    this.workItem();
                }
            }
        }

        internal Thread Worker
        {
            get
            {
                return this.worker;
            }
        }
    }
}

