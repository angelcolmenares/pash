namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Tracing;
    using System.Threading;

    public abstract class Job2 : Job
    {
        private List<CommandParameterCollection> _parameters;
        private readonly object _syncobject;
        private readonly PowerShellTraceSource _tracer;
        private const int ResumeJobOperation = 4;
        private const int StartJobOperation = 1;
        private const int StopJobOperation = 2;
        private const int SuspendJobOperation = 3;
        private const int UnblockJobOperation = 5;

        public event EventHandler<AsyncCompletedEventArgs> ResumeJobCompleted;

        public event EventHandler<AsyncCompletedEventArgs> StartJobCompleted;

        public event EventHandler<AsyncCompletedEventArgs> StopJobCompleted;

        public event EventHandler<AsyncCompletedEventArgs> SuspendJobCompleted;

        public event EventHandler<AsyncCompletedEventArgs> UnblockJobCompleted;

        protected Job2()
        {
            this._syncobject = new object();
            this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
        }

        protected Job2(string command) : base(command)
        {
            this._syncobject = new object();
            this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
        }

        protected Job2(string command, string name) : base(command, name)
        {
            this._syncobject = new object();
            this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
        }

        protected Job2(string command, string name, IList<Job> childJobs) : base(command, name, childJobs)
        {
            this._syncobject = new object();
            this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
        }

        protected Job2(string command, string name, Guid instanceId) : base(command, name, instanceId)
        {
            this._syncobject = new object();
            this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
        }

        protected Job2(string command, string name, JobIdentifier token) : base(command, name, token)
        {
            this._syncobject = new object();
            this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
        }

        protected virtual void OnResumeJobCompleted(AsyncCompletedEventArgs eventArgs)
        {
            this.RaiseCompletedHandler(4, eventArgs);
        }

        protected virtual void OnStartJobCompleted(AsyncCompletedEventArgs eventArgs)
        {
            this.RaiseCompletedHandler(1, eventArgs);
        }

        protected virtual void OnStopJobCompleted(AsyncCompletedEventArgs eventArgs)
        {
            this.RaiseCompletedHandler(2, eventArgs);
        }

        protected virtual void OnSuspendJobCompleted(AsyncCompletedEventArgs eventArgs)
        {
            this.RaiseCompletedHandler(3, eventArgs);
        }

        protected virtual void OnUnblockJobCompleted(AsyncCompletedEventArgs eventArgs)
        {
            this.RaiseCompletedHandler(5, eventArgs);
        }

        private void RaiseCompletedHandler(int operation, AsyncCompletedEventArgs eventArgs)
        {
            EventHandler<AsyncCompletedEventArgs> startJobCompleted = null;
            switch (operation)
            {
                case 1:
                    startJobCompleted = this.StartJobCompleted;
                    break;

                case 2:
                    startJobCompleted = this.StopJobCompleted;
                    break;

                case 3:
                    startJobCompleted = this.SuspendJobCompleted;
                    break;

                case 4:
                    startJobCompleted = this.ResumeJobCompleted;
                    break;

                case 5:
                    startJobCompleted = this.UnblockJobCompleted;
                    break;
            }
            try
            {
                if (startJobCompleted != null)
                {
                    startJobCompleted(this, eventArgs);
                }
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                this._tracer.TraceException(exception);
            }
        }

        public abstract void ResumeJob();
        public abstract void ResumeJobAsync();
        protected void SetJobState(JobState state, Exception reason)
        {
            base.SetJobState(state, reason);
        }

        public abstract void StartJob();
        public abstract void StartJobAsync();
        public abstract void StopJob(bool force, string reason);
        public abstract void StopJobAsync();
        public abstract void StopJobAsync(bool force, string reason);
        public abstract void SuspendJob();
        public abstract void SuspendJob(bool force, string reason);
        public abstract void SuspendJobAsync();
        public abstract void SuspendJobAsync(bool force, string reason);
        public abstract void UnblockJob();
        public abstract void UnblockJobAsync();

        public List<CommandParameterCollection> StartParameters
        {
            get
            {
                if (this._parameters == null)
                {
                    lock (this._syncobject)
                    {
                        if (this._parameters == null)
                        {
                            this._parameters = new List<CommandParameterCollection>();
                        }
                    }
                }
                return this._parameters;
            }
            set
            {
                if (value == null)
                {
                    throw PSTraceSource.NewArgumentNullException("value");
                }
                lock (this._syncobject)
                {
                    this._parameters = value;
                }
            }
        }

        protected object SyncRoot
        {
            get
            {
                return base.syncObject;
            }
        }
    }
}

