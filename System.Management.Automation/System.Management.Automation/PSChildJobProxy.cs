namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Tracing;
    using System.Runtime.InteropServices;
    using System.Threading;

    public sealed class PSChildJobProxy : Job2
    {
        private bool _disposed;
        private string _location;
        private string _statusMessage;
        private readonly object _syncObject;
        private readonly PowerShellTraceSource _tracer;
        private const string ClassNameTrace = "PSChildJobProxy";
        private const string ResBaseName = "PowerShellStrings";
        private static Tracer StructuredTracer = new Tracer();

        public event EventHandler<JobDataAddedEventArgs> JobDataAdded;

        internal PSChildJobProxy(string command, PSObject o) : base(command)
        {
            string str;
            this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
            this._syncObject = new object();
            PSJobProxy.TryGetJobPropertyValue<string>(o, "StatusMessage", out this._statusMessage);
            PSJobProxy.TryGetJobPropertyValue<string>(o, "Location", out this._location);
            PSJobProxy.TryGetJobPropertyValue<string>(o, "Name", out str);
            base.Name = str;
            base.Output.DataAdded += new EventHandler<DataAddedEventArgs>(this.OutputAdded);
            base.Error.DataAdded += new EventHandler<DataAddedEventArgs>(this.ErrorAdded);
            base.Warning.DataAdded += new EventHandler<DataAddedEventArgs>(this.WarningAdded);
            base.Verbose.DataAdded += new EventHandler<DataAddedEventArgs>(this.VerboseAdded);
            base.Progress.DataAdded += new EventHandler<DataAddedEventArgs>(this.ProgressAdded);
            base.Debug.DataAdded += new EventHandler<DataAddedEventArgs>(this.DebugAdded);
        }

        internal void AssignDisconnectedState()
        {
            this.DoSetJobState(JobState.Disconnected, null);
        }

        private void DebugAdded(object sender, DataAddedEventArgs e)
        {
            this.OnJobDataAdded(new JobDataAddedEventArgs(this, PowerShellStreamType.Debug, e.Index));
        }

        protected override void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                lock (this._syncObject)
                {
                    if (this._disposed)
                    {
                        return;
                    }
                    this._disposed = true;
                    base.Output.DataAdded -= new EventHandler<DataAddedEventArgs>(this.OutputAdded);
                    base.Error.DataAdded -= new EventHandler<DataAddedEventArgs>(this.ErrorAdded);
                    base.Warning.DataAdded -= new EventHandler<DataAddedEventArgs>(this.WarningAdded);
                    base.Verbose.DataAdded -= new EventHandler<DataAddedEventArgs>(this.VerboseAdded);
                    base.Progress.DataAdded -= new EventHandler<DataAddedEventArgs>(this.ProgressAdded);
                    base.Debug.DataAdded -= new EventHandler<DataAddedEventArgs>(this.DebugAdded);
                }
                base.Dispose(disposing);
            }
        }

        internal void DoSetJobState(JobState state, Exception reason = null)
        {
            if (!this._disposed)
            {
                try
                {
                    this._tracer.WriteMessage("PSChildJobProxy", "DoSetJobState", Guid.Empty, this, "BEGIN Set job state to {0} and call event handlers", new string[] { state.ToString() });
                    StructuredTracer.BeginProxyChildJobEventHandler(base.InstanceId);
                    base.SetJobState(state, reason);
                    StructuredTracer.EndProxyJobEventHandler(base.InstanceId);
                    this._tracer.WriteMessage("PSChildJobProxy", "DoSetJobState", Guid.Empty, this, "END Set job state to {0} and call event handlers", new string[] { state.ToString() });
                }
                catch (ObjectDisposedException)
                {
                }
            }
        }

        private void ErrorAdded(object sender, DataAddedEventArgs e)
        {
            this.OnJobDataAdded(new JobDataAddedEventArgs(this, PowerShellStreamType.Error, e.Index));
        }

        private void OnJobDataAdded(JobDataAddedEventArgs eventArgs)
        {
            try
            {
                this._tracer.WriteMessage("PSChildJobProxy", "OnJobDataAdded", Guid.Empty, this, "BEGIN call event handlers", new string[0]);
                this.JobDataAdded.SafeInvoke<JobDataAddedEventArgs>(this, eventArgs);
                this._tracer.WriteMessage("PSChildJobProxy", "OnJobDataAdded", Guid.Empty, this, "END call event handlers", new string[0]);
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                this._tracer.WriteMessage("PSChildJobProxy", "OnJobDataAdded", Guid.Empty, this, "END Exception thrown in JobDataAdded handler", new string[0]);
                this._tracer.TraceException(exception);
            }
        }

        private void OutputAdded(object sender, DataAddedEventArgs e)
        {
            this.OnJobDataAdded(new JobDataAddedEventArgs(this, PowerShellStreamType.Output, e.Index));
        }

        private void ProgressAdded(object sender, DataAddedEventArgs e)
        {
            this.OnJobDataAdded(new JobDataAddedEventArgs(this, PowerShellStreamType.Progress, e.Index));
        }

        public override void ResumeJob()
        {
            throw PSTraceSource.NewNotSupportedException("PowerShellStrings", "ProxyChildJobControlNotSupported", new object[0]);
        }

        public override void ResumeJobAsync()
        {
            throw PSTraceSource.NewNotSupportedException("PowerShellStrings", "ProxyChildJobControlNotSupported", new object[0]);
        }

        public override void StartJob()
        {
            throw PSTraceSource.NewNotSupportedException("PowerShellStrings", "ProxyChildJobControlNotSupported", new object[0]);
        }

        public override void StartJobAsync()
        {
            throw PSTraceSource.NewNotSupportedException("PowerShellStrings", "ProxyChildJobControlNotSupported", new object[0]);
        }

        public override void StopJob()
        {
            throw PSTraceSource.NewNotSupportedException("PowerShellStrings", "ProxyChildJobControlNotSupported", new object[0]);
        }

        public override void StopJob(bool force, string reason)
        {
            throw PSTraceSource.NewNotSupportedException("PowerShellStrings", "ProxyChildJobControlNotSupported", new object[0]);
        }

        public override void StopJobAsync()
        {
            throw PSTraceSource.NewNotSupportedException("PowerShellStrings", "ProxyChildJobControlNotSupported", new object[0]);
        }

        public override void StopJobAsync(bool force, string reason)
        {
            throw PSTraceSource.NewNotSupportedException("PowerShellStrings", "ProxyChildJobControlNotSupported", new object[0]);
        }

        public override void SuspendJob()
        {
            throw PSTraceSource.NewNotSupportedException("PowerShellStrings", "ProxyChildJobControlNotSupported", new object[0]);
        }

        public override void SuspendJob(bool force, string reason)
        {
            throw PSTraceSource.NewNotSupportedException("PowerShellStrings", "ProxyChildJobControlNotSupported", new object[0]);
        }

        public override void SuspendJobAsync()
        {
            throw PSTraceSource.NewNotSupportedException("PowerShellStrings", "ProxyChildJobControlNotSupported", new object[0]);
        }

        public override void SuspendJobAsync(bool force, string reason)
        {
            throw PSTraceSource.NewNotSupportedException("PowerShellStrings", "ProxyChildJobControlNotSupported", new object[0]);
        }

        public override void UnblockJob()
        {
            throw PSTraceSource.NewNotSupportedException("PowerShellStrings", "ProxyChildJobControlNotSupported", new object[0]);
        }

        public override void UnblockJobAsync()
        {
            throw PSTraceSource.NewNotSupportedException("PowerShellStrings", "ProxyChildJobControlNotSupported", new object[0]);
        }

        private void VerboseAdded(object sender, DataAddedEventArgs e)
        {
            this.OnJobDataAdded(new JobDataAddedEventArgs(this, PowerShellStreamType.Verbose, e.Index));
        }

        private void WarningAdded(object sender, DataAddedEventArgs e)
        {
            this.OnJobDataAdded(new JobDataAddedEventArgs(this, PowerShellStreamType.Warning, e.Index));
        }

        public override bool HasMoreData
        {
            get
            {
                if ((((!base.Output.IsOpen && (base.Output.Count <= 0)) && (!base.Error.IsOpen && (base.Error.Count <= 0))) && ((!base.Verbose.IsOpen && (base.Verbose.Count <= 0)) && (!base.Debug.IsOpen && (base.Debug.Count <= 0)))) && ((!base.Warning.IsOpen && (base.Warning.Count <= 0)) && !base.Progress.IsOpen))
                {
                    return (base.Progress.Count > 0);
                }
                return true;
            }
        }

        public override string Location
        {
            get
            {
                return this._location;
            }
        }

        public override string StatusMessage
        {
            get
            {
                return this._statusMessage;
            }
        }
    }
}

