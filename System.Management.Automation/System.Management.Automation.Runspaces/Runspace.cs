namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces.Internal;
    using System.Threading;
    using System.Transactions;

    public abstract class Runspace : IDisposable
    {
        private Guid _engineActivityId = Guid.Empty;
        private Guid _instanceId = Guid.NewGuid();
        private long _pipelineIdSeed;
        private bool _skipUserProfile;
        private System.Threading.ApartmentState apartmentState = System.Threading.ApartmentState.Unknown;
        internal const System.Threading.ApartmentState DefaultApartmentState = System.Threading.ApartmentState.Unknown;
        [ThreadStatic]
        private static Runspace ThreadSpecificDefaultRunspace;

        public abstract event EventHandler<RunspaceAvailabilityEventArgs> AvailabilityChanged;

        public abstract event EventHandler<RunspaceStateEventArgs> StateChanged;

        internal Runspace()
        {
        }

        public void ClearBaseTransaction()
        {
            this.ExecutionContext.TransactionManager.ClearBaseTransaction();
        }

        public abstract void Close();
        public abstract void CloseAsync();
        public abstract void Connect();
        public abstract void ConnectAsync();
        public abstract Pipeline CreateDisconnectedPipeline();
        public abstract PowerShell CreateDisconnectedPowerShell();
        public abstract Pipeline CreateNestedPipeline();
        public abstract Pipeline CreateNestedPipeline(string command, bool addToHistory);
        public abstract Pipeline CreatePipeline();
        public abstract Pipeline CreatePipeline(string command);
        public abstract Pipeline CreatePipeline(string command, bool addToHistory);
        public abstract void Disconnect();
        public abstract void DisconnectAsync();
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        internal long GeneratePipelineId()
        {
            return Interlocked.Increment(ref this._pipelineIdSeed);
        }

        public abstract PSPrimitiveDictionary GetApplicationPrivateData();
        public abstract RunspaceCapability GetCapabilities();
        internal abstract Pipeline GetCurrentlyRunningPipeline();
        internal System.Version GetRemoteProtocolVersion()
        {
            System.Version version;
            if (PSPrimitiveDictionary.TryPathGet<System.Version>(this.GetApplicationPrivateData(), out version, new string[] { "PSVersionTable", "PSRemotingProtocolVersion" }))
            {
                return version;
            }
            return RemotingConstants.ProtocolVersion;
        }

        public static Runspace[] GetRunspaces(RunspaceConnectionInfo connectionInfo)
        {
            return GetRunspaces(connectionInfo, null, null);
        }

        public static Runspace[] GetRunspaces(RunspaceConnectionInfo connectionInfo, PSHost host)
        {
            return GetRunspaces(connectionInfo, host, null);
        }

        public static Runspace[] GetRunspaces(RunspaceConnectionInfo connectionInfo, PSHost host, TypeTable typeTable)
        {
            return RemoteRunspace.GetRemoteRunspaces(connectionInfo, host, typeTable);
        }

        internal abstract System.Management.Automation.Runspaces.SessionStateProxy GetSessionStateProxy();
        protected abstract void OnAvailabilityChanged(RunspaceAvailabilityEventArgs e);
        public abstract void Open();
        public abstract void OpenAsync();
        internal void RaiseAvailabilityChangedEvent(System.Management.Automation.Runspaces.RunspaceAvailability availability)
        {
            this.OnAvailabilityChanged(new RunspaceAvailabilityEventArgs(availability));
        }

        public virtual void ResetRunspaceState()
        {
            throw new NotImplementedException("ResetRunspaceState");
        }

        internal abstract void SetApplicationPrivateData(PSPrimitiveDictionary applicationPrivateData);
        public void SetBaseTransaction(CommittableTransaction transaction)
        {
            this.ExecutionContext.TransactionManager.SetBaseTransaction(transaction, RollbackSeverity.Error);
        }

        public void SetBaseTransaction(CommittableTransaction transaction, RollbackSeverity severity)
        {
            this.ExecutionContext.TransactionManager.SetBaseTransaction(transaction, severity);
        }

        internal void UpdateRunspaceAvailability(PSInvocationState invocationState, bool raiseEvent)
        {
            switch (invocationState)
            {
                case PSInvocationState.NotStarted:
                    this.UpdateRunspaceAvailability(PipelineState.NotStarted, raiseEvent);
                    return;

                case PSInvocationState.Running:
                    this.UpdateRunspaceAvailability(PipelineState.Running, raiseEvent);
                    return;

                case PSInvocationState.Stopping:
                    this.UpdateRunspaceAvailability(PipelineState.Stopping, raiseEvent);
                    return;

                case PSInvocationState.Stopped:
                    this.UpdateRunspaceAvailability(PipelineState.Stopped, raiseEvent);
                    return;

                case PSInvocationState.Completed:
                    this.UpdateRunspaceAvailability(PipelineState.Completed, raiseEvent);
                    return;

                case PSInvocationState.Failed:
                    this.UpdateRunspaceAvailability(PipelineState.Failed, raiseEvent);
                    return;

                case PSInvocationState.Disconnected:
                    this.UpdateRunspaceAvailability(PipelineState.Disconnected, raiseEvent);
                    return;
            }
        }

        internal void UpdateRunspaceAvailability(PipelineState pipelineState, bool raiseEvent)
        {
            System.Management.Automation.Runspaces.RunspaceAvailability runspaceAvailability = this.RunspaceAvailability;
            switch (runspaceAvailability)
            {
                case System.Management.Automation.Runspaces.RunspaceAvailability.None:
                    if (pipelineState == PipelineState.Running)
                    {
                        this.RunspaceAvailability = System.Management.Automation.Runspaces.RunspaceAvailability.Busy;
                    }
                    break;

                case System.Management.Automation.Runspaces.RunspaceAvailability.Available:
                    switch (pipelineState)
                    {
                        case PipelineState.Running:
                            this.RunspaceAvailability = System.Management.Automation.Runspaces.RunspaceAvailability.Busy;
                            goto Label_0181;

                        case PipelineState.Disconnected:
                            this.RunspaceAvailability = System.Management.Automation.Runspaces.RunspaceAvailability.None;
                            goto Label_0181;
                    }
                    break;

                case System.Management.Automation.Runspaces.RunspaceAvailability.AvailableForNestedCommand:
                    switch (pipelineState)
                    {
                        case PipelineState.Running:
                            this.RunspaceAvailability = System.Management.Automation.Runspaces.RunspaceAvailability.Busy;
                            goto Label_0181;

                        case PipelineState.Completed:
                            this.RunspaceAvailability = this.InNestedPrompt ? System.Management.Automation.Runspaces.RunspaceAvailability.AvailableForNestedCommand : System.Management.Automation.Runspaces.RunspaceAvailability.Available;
                            goto Label_0181;
                    }
                    break;

                case System.Management.Automation.Runspaces.RunspaceAvailability.Busy:
                    switch (pipelineState)
                    {
                        case PipelineState.Stopped:
                        case PipelineState.Completed:
                        case PipelineState.Failed:
                            if (this.InNestedPrompt || (!(this is RemoteRunspace) && this.Debugger.InBreakpoint))
                            {
                                this.RunspaceAvailability = System.Management.Automation.Runspaces.RunspaceAvailability.AvailableForNestedCommand;
                            }
                            else
                            {
                                RemoteRunspace runspace = this as RemoteRunspace;
                                ConnectCommandInfo info = (runspace != null) ? runspace.RemoteCommand : null;
                                if (((pipelineState == PipelineState.Completed) || (pipelineState == PipelineState.Failed)) && (info != null))
                                {
                                    runspace.RunspacePool.RemoteRunspacePoolInternal.ConnectCommands = null;
                                    info = null;
                                }
                                Pipeline currentlyRunningPipeline = this.GetCurrentlyRunningPipeline();
                                if (currentlyRunningPipeline == null)
                                {
                                    if (info == null)
                                    {
                                        if (runspace != null)
                                        {
                                            this.RunspaceAvailability = runspace.RunspacePool.RemoteRunspacePoolInternal.AvailableForConnection ? System.Management.Automation.Runspaces.RunspaceAvailability.Available : System.Management.Automation.Runspaces.RunspaceAvailability.Busy;
                                        }
                                        else
                                        {
                                            this.RunspaceAvailability = System.Management.Automation.Runspaces.RunspaceAvailability.Available;
                                        }
                                    }
                                }
                                else if (currentlyRunningPipeline.PipelineStateInfo.State == PipelineState.Running)
                                {
                                    this.RunspaceAvailability = System.Management.Automation.Runspaces.RunspaceAvailability.Busy;
                                }
                                else
                                {
                                    this.RunspaceAvailability = System.Management.Automation.Runspaces.RunspaceAvailability.Available;
                                }
                            }
                            goto Label_0181;

                        case PipelineState.Disconnected:
                            this.RunspaceAvailability = System.Management.Automation.Runspaces.RunspaceAvailability.None;
                            goto Label_0181;
                    }
                    break;
            }
        Label_0181:
            if (raiseEvent && (this.RunspaceAvailability != runspaceAvailability))
            {
                this.OnAvailabilityChanged(new RunspaceAvailabilityEventArgs(this.RunspaceAvailability));
            }
        }

        internal void UpdateRunspaceAvailability(System.Management.Automation.Runspaces.RunspaceAvailability availability, bool raiseEvent)
        {
            System.Management.Automation.Runspaces.RunspaceAvailability runspaceAvailability = this.RunspaceAvailability;
            this.RunspaceAvailability = availability;
            if (raiseEvent && (this.RunspaceAvailability != runspaceAvailability))
            {
                this.OnAvailabilityChanged(new RunspaceAvailabilityEventArgs(this.RunspaceAvailability));
            }
        }

        protected void UpdateRunspaceAvailability(RunspaceState runspaceState, bool raiseEvent)
        {
            System.Management.Automation.Runspaces.RunspaceAvailability runspaceAvailability = this.RunspaceAvailability;
            RemoteRunspace runspace = this as RemoteRunspace;
            ConnectCommandInfo info = (runspace != null) ? runspace.RemoteCommand : null;
            switch (runspaceAvailability)
            {
                case System.Management.Automation.Runspaces.RunspaceAvailability.None:
                    if (runspaceState == RunspaceState.Opened)
                    {
                        this.RunspaceAvailability = ((info == null) && (this.GetCurrentlyRunningPipeline() == null)) ? System.Management.Automation.Runspaces.RunspaceAvailability.Available : System.Management.Automation.Runspaces.RunspaceAvailability.Busy;
                    }
                    break;

                case System.Management.Automation.Runspaces.RunspaceAvailability.Available:
                case System.Management.Automation.Runspaces.RunspaceAvailability.AvailableForNestedCommand:
                case System.Management.Automation.Runspaces.RunspaceAvailability.Busy:
                    switch (runspaceState)
                    {
                        case RunspaceState.Closed:
                        case RunspaceState.Closing:
                        case RunspaceState.Broken:
                        case RunspaceState.Disconnected:
                            this.RunspaceAvailability = System.Management.Automation.Runspaces.RunspaceAvailability.None;
                            goto Label_007D;
                    }
                    break;
            }
        Label_007D:
            if (raiseEvent && (this.RunspaceAvailability != runspaceAvailability))
            {
                this.OnAvailabilityChanged(new RunspaceAvailabilityEventArgs(this.RunspaceAvailability));
            }
        }

        public System.Threading.ApartmentState ApartmentState
        {
            get
            {
                return this.apartmentState;
            }
            set
            {
                if (this.RunspaceStateInfo.State != RunspaceState.BeforeOpen)
                {
                    throw new InvalidRunspaceStateException(StringUtil.Format(RunspaceStrings.ChangePropertyAfterOpen, new object[0]));
                }
                this.apartmentState = value;
            }
        }

        public abstract RunspaceConnectionInfo ConnectionInfo { get; }

        public System.Management.Automation.Debugger Debugger
        {
            get
            {
                return this.GetExecutionContext.Debugger;
            }
        }

        public static Runspace DefaultRunspace
        {
            get
            {
                return ThreadSpecificDefaultRunspace;
            }
            set
            {
                ThreadSpecificDefaultRunspace = value;
            }
        }

        internal Guid EngineActivityId
        {
            get
            {
                return this._engineActivityId;
            }
            set
            {
                this._engineActivityId = value;
            }
        }

        public abstract PSEventManager Events { get; }

        internal System.Management.Automation.ExecutionContext ExecutionContext
        {
            get
            {
                return this.GetExecutionContext;
            }
        }

        internal abstract System.Management.Automation.ExecutionContext GetExecutionContext { get; }

        internal abstract bool HasAvailabilityChangedSubscribers { get; }

        public abstract System.Management.Automation.Runspaces.InitialSessionState InitialSessionState { get; }

        internal abstract bool InNestedPrompt { get; }

        public Guid InstanceId
        {
            get
            {
                return this._instanceId;
            }
            internal set
            {
                this._instanceId = value;
            }
        }

        public abstract System.Management.Automation.JobManager JobManager { get; }

        public abstract RunspaceConnectionInfo OriginalConnectionInfo { get; }

        public abstract System.Management.Automation.Runspaces.RunspaceAvailability RunspaceAvailability { get; protected set; }

        public abstract System.Management.Automation.Runspaces.RunspaceConfiguration RunspaceConfiguration { get; }

        public abstract System.Management.Automation.Runspaces.RunspaceStateInfo RunspaceStateInfo { get; }

        public System.Management.Automation.Runspaces.SessionStateProxy SessionStateProxy
        {
            get
            {
                return this.GetSessionStateProxy();
            }
        }

        internal bool SkipUserProfile
        {
            get
            {
                return this._skipUserProfile;
            }
            set
            {
                this._skipUserProfile = value;
            }
        }

        public abstract PSThreadOptions ThreadOptions { get; set; }

        public abstract System.Version Version { get; }
    }
}

