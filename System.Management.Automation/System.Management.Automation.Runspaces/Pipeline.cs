namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Management.Automation;

    public abstract class Pipeline : IDisposable
    {
        private CommandCollection _commands;
        private bool _hadErrors;
        private PSInvocationSettings _invocationSettings;
        private long _pipelineId;
        private bool _redirectShellErrorOutputPipe;
        private bool _setPipelineSessionState;

        public abstract event EventHandler<PipelineStateEventArgs> StateChanged;

        internal Pipeline(System.Management.Automation.Runspaces.Runspace runspace) : this(runspace, new CommandCollection())
        {
        }

        internal Pipeline(System.Management.Automation.Runspaces.Runspace runspace, CommandCollection command)
        {
            this._setPipelineSessionState = true;
            if (runspace == null)
            {
                PSTraceSource.NewArgumentNullException("runspace");
            }
            this._pipelineId = runspace.GeneratePipelineId();
            this._commands = command;
        }

        public abstract Collection<PSObject> Connect();
        public abstract void ConnectAsync();
        public abstract Pipeline Copy();
        public void Dispose()
        {
            this.Dispose(!this.IsChild);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public Collection<PSObject> Invoke()
        {
            return this.Invoke(null);
        }

        public abstract Collection<PSObject> Invoke(IEnumerable input);
        public abstract void InvokeAsync();
        internal abstract void InvokeAsyncAndDisconnect();
        internal void SetCommandCollection(CommandCollection commands)
        {
            this._commands = commands;
        }

        internal void SetHadErrors(bool status)
        {
            this._hadErrors = this._hadErrors || status;
        }

        internal abstract void SetHistoryString(string historyString);
        public abstract void Stop();
        public abstract void StopAsync();

        public CommandCollection Commands
        {
            get
            {
                return this._commands;
            }
        }

        public abstract PipelineReader<object> Error { get; }

        public virtual bool HadErrors
        {
            get
            {
                return this._hadErrors;
            }
        }

        public abstract PipelineWriter Input { get; }

        public long InstanceId
        {
            get
            {
                return this._pipelineId;
            }
        }

        internal PSInvocationSettings InvocationSettings
        {
            get
            {
                return this._invocationSettings;
            }
            set
            {
                this._invocationSettings = value;
            }
        }

        internal virtual bool IsChild
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public abstract bool IsNested { get; }

        public abstract PipelineReader<PSObject> Output { get; }

        public abstract System.Management.Automation.Runspaces.PipelineStateInfo PipelineStateInfo { get; }

        internal bool RedirectShellErrorOutputPipe
        {
            get
            {
                return this._redirectShellErrorOutputPipe;
            }
            set
            {
                this._redirectShellErrorOutputPipe = value;
            }
        }

        public abstract System.Management.Automation.Runspaces.Runspace Runspace { get; }

        public bool SetPipelineSessionState
        {
            get
            {
                return this._setPipelineSessionState;
            }
            set
            {
                this._setPipelineSessionState = value;
            }
        }
    }
}

