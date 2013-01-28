namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;
    using System.Security.Principal;
    using System.Threading;

    internal class ServerPowerShellDriver
    {
        private bool addToHistory;
        private ApartmentState apartmentState;
        private Guid clientPowerShellId;
        private Guid clientRunspacePoolId;
        private bool[] datasent = new bool[2];
        private ServerPowerShellDataStructureHandler dsHandler;
        private PowerShell extraPowerShell;
        private bool extraPowerShellAlreadyScheduled;
        private PSDataCollection<object> input;
        private PowerShell localPowerShell;
        private PSDataCollection<PSObject> localPowerShellOutput;
        private bool noInput;
        private ServerRemoteHost remoteHost;
        private System.Management.Automation.RemoteStreamOptions remoteStreamOptions;
        private object syncObject = new object();

        internal ServerPowerShellDriver(PowerShell powershell, PowerShell extraPowerShell, bool noInput, Guid clientPowerShellId, Guid clientRunspacePoolId, ServerRunspacePoolDriver runspacePoolDriver, ApartmentState apartmentState, HostInfo hostInfo, System.Management.Automation.RemoteStreamOptions streamOptions, bool addToHistory, Runspace rsToUse)
        {
            this.clientPowerShellId = clientPowerShellId;
            this.clientRunspacePoolId = clientRunspacePoolId;
            this.remoteStreamOptions = streamOptions;
            this.apartmentState = apartmentState;
            this.localPowerShell = powershell;
            this.extraPowerShell = extraPowerShell;
            this.localPowerShellOutput = new PSDataCollection<PSObject>();
            this.noInput = noInput;
            this.addToHistory = addToHistory;
            this.dsHandler = runspacePoolDriver.DataStructureHandler.CreatePowerShellDataStructureHandler(clientPowerShellId, clientRunspacePoolId, this.remoteStreamOptions, this.localPowerShell);
            this.remoteHost = this.dsHandler.GetHostAssociatedWithPowerShell(hostInfo, runspacePoolDriver.ServerRemoteHost);
            if (!noInput)
            {
                this.input = new PSDataCollection<object>();
                this.input.ReleaseOnEnumeration = true;
                this.input.IdleEvent += new EventHandler<EventArgs>(this.HandleIdleEvent);
            }
            this.RegisterPipelineOutputEventHandlers(this.localPowerShellOutput);
            if (this.localPowerShell != null)
            {
                this.RegisterPowerShellEventHandlers(this.localPowerShell);
                this.datasent[0] = false;
            }
            if (extraPowerShell != null)
            {
                this.RegisterPowerShellEventHandlers(extraPowerShell);
                this.datasent[1] = false;
            }
            this.RegisterDataStructureHandlerEventHandlers(this.dsHandler);
            if (rsToUse != null)
            {
                this.localPowerShell.Runspace = rsToUse;
                if (extraPowerShell != null)
                {
                    extraPowerShell.Runspace = rsToUse;
                }
            }
            else
            {
                this.localPowerShell.RunspacePool = runspacePoolDriver.RunspacePool;
                if (extraPowerShell != null)
                {
                    extraPowerShell.RunspacePool = runspacePoolDriver.RunspacePool;
                }
            }
        }

        private void HandleDebugAdded(object sender, DataAddedEventArgs eventArgs)
        {
            int index = eventArgs.Index;
            lock (this.syncObject)
            {
                int num2 = !this.extraPowerShellAlreadyScheduled ? 0 : 1;
                if ((num2 == 0) && !this.datasent[num2])
                {
                    DebugRecord record = this.localPowerShell.Streams.Debug[index];
                    this.localPowerShell.Streams.Debug.RemoveAt(index);
                    this.dsHandler.SendDebugRecordToClient(record);
                }
            }
        }

        private void HandleErrorDataAdded(object sender, DataAddedEventArgs e)
        {
            int index = e.Index;
            lock (this.syncObject)
            {
                int num2 = !this.extraPowerShellAlreadyScheduled ? 0 : 1;
                if ((num2 == 0) && !this.datasent[num2])
                {
                    ErrorRecord errorRecord = this.localPowerShell.Streams.Error[index];
                    this.localPowerShell.Streams.Error.RemoveAt(index);
                    this.dsHandler.SendErrorRecordToClient(errorRecord);
                }
            }
        }

        private void HandleHostResponseReceived(object sender, RemoteDataEventArgs<RemoteHostResponse> eventArgs)
        {
            this.remoteHost.ServerMethodExecutor.HandleRemoteHostResponseFromClient(eventArgs.Data);
        }

        private void HandleIdleEvent(object sender, EventArgs args)
        {
            Runspace runspaceUsedToInvokePowerShell = this.dsHandler.RunspaceUsedToInvokePowerShell;
            if (runspaceUsedToInvokePowerShell != null)
            {
                PSLocalEventManager events = runspaceUsedToInvokePowerShell.Events as PSLocalEventManager;
                if (events != null)
                {
                    foreach (PSEventSubscriber subscriber in events.Subscribers)
                    {
                        events.DrainPendingActions(subscriber);
                    }
                }
            }
        }

        private void HandleInputEndReceived(object sender, EventArgs eventArgs)
        {
            if (this.input != null)
            {
                this.input.Complete();
            }
        }

        private void HandleInputReceived(object sender, RemoteDataEventArgs<object> eventArgs)
        {
            if (this.input != null)
            {
                this.input.Add(eventArgs.Data);
            }
        }

        private void HandleOutputDataAdded(object sender, DataAddedEventArgs e)
        {
            int index = e.Index;
            lock (this.syncObject)
            {
                int num2 = !this.extraPowerShellAlreadyScheduled ? 0 : 1;
                if (!this.datasent[num2])
                {
                    PSObject data = this.localPowerShellOutput[index];
                    this.localPowerShellOutput.RemoveAt(index);
                    this.dsHandler.SendOutputDataToClient(data);
                }
            }
        }

        private void HandlePowerShellInvocationStateChanged(object sender, PSInvocationStateChangedEventArgs eventArgs)
        {
            PSInvocationState state = eventArgs.InvocationStateInfo.State;
            switch (state)
            {
                case PSInvocationState.Stopping:
                    this.remoteHost.ServerMethodExecutor.AbortAllCalls();
                    return;

                case PSInvocationState.Stopped:
                case PSInvocationState.Completed:
                case PSInvocationState.Failed:
                    if (!this.localPowerShell.RunningExtraCommands)
                    {
                        this.SendRemainingData();
                        if (((state == PSInvocationState.Completed) && (this.extraPowerShell != null)) && !this.extraPowerShellAlreadyScheduled)
                        {
                            this.extraPowerShellAlreadyScheduled = true;
                            this.Start(false);
                        }
                        else
                        {
                            this.dsHandler.RaiseRemoveAssociationEvent();
                            this.dsHandler.SendStateChangedInformationToClient(eventArgs.InvocationStateInfo);
                            this.UnregisterPowerShellEventHandlers(this.localPowerShell);
                            if (this.extraPowerShell != null)
                            {
                                this.UnregisterPowerShellEventHandlers(this.extraPowerShell);
                            }
                            this.UnregisterDataStructureHandlerEventHandlers(this.dsHandler);
                            this.UnregisterPipelineOutputEventHandlers(this.localPowerShellOutput);
                        }
                        return;
                    }
                    return;
            }
        }

        private void HandleProgressAdded(object sender, DataAddedEventArgs eventArgs)
        {
            int index = eventArgs.Index;
            lock (this.syncObject)
            {
                int num2 = !this.extraPowerShellAlreadyScheduled ? 0 : 1;
                if ((num2 == 0) && !this.datasent[num2])
                {
                    ProgressRecord record = this.localPowerShell.Streams.Progress[index];
                    this.localPowerShell.Streams.Progress.RemoveAt(index);
                    this.dsHandler.SendProgressRecordToClient(record);
                }
            }
        }

        private void HandleSessionConnected(object sender, EventArgs eventArgs)
        {
            if (this.input != null)
            {
                this.input.Complete();
            }
        }

        private void HandleStopReceived(object sender, EventArgs eventArgs)
        {
            if (((this.localPowerShell.InvocationStateInfo.State != PSInvocationState.Stopped) && (this.localPowerShell.InvocationStateInfo.State != PSInvocationState.Completed)) && ((this.localPowerShell.InvocationStateInfo.State != PSInvocationState.Failed) && (this.localPowerShell.InvocationStateInfo.State != PSInvocationState.Stopping)))
            {
                this.localPowerShell.Stop();
            }
            if ((((this.extraPowerShell != null) && (this.extraPowerShell.InvocationStateInfo.State != PSInvocationState.Stopped)) && ((this.extraPowerShell.InvocationStateInfo.State != PSInvocationState.Completed) && (this.extraPowerShell.InvocationStateInfo.State != PSInvocationState.Failed))) && (this.extraPowerShell.InvocationStateInfo.State != PSInvocationState.Stopping))
            {
                this.extraPowerShell.Stop();
            }
        }

        private void HandleVerboseAdded(object sender, DataAddedEventArgs eventArgs)
        {
            int index = eventArgs.Index;
            lock (this.syncObject)
            {
                int num2 = !this.extraPowerShellAlreadyScheduled ? 0 : 1;
                if ((num2 == 0) && !this.datasent[num2])
                {
                    VerboseRecord record = this.localPowerShell.Streams.Verbose[index];
                    this.localPowerShell.Streams.Verbose.RemoveAt(index);
                    this.dsHandler.SendVerboseRecordToClient(record);
                }
            }
        }

        private void HandleWarningAdded(object sender, DataAddedEventArgs eventArgs)
        {
            int index = eventArgs.Index;
            lock (this.syncObject)
            {
                int num2 = !this.extraPowerShellAlreadyScheduled ? 0 : 1;
                if ((num2 == 0) && !this.datasent[num2])
                {
                    WarningRecord record = this.localPowerShell.Streams.Warning[index];
                    this.localPowerShell.Streams.Warning.RemoveAt(index);
                    this.dsHandler.SendWarningRecordToClient(record);
                }
            }
        }

        private void RegisterDataStructureHandlerEventHandlers(ServerPowerShellDataStructureHandler dsHandler)
        {
            dsHandler.InputEndReceived += new EventHandler(this.HandleInputEndReceived);
            dsHandler.InputReceived += new EventHandler<RemoteDataEventArgs<object>>(this.HandleInputReceived);
            dsHandler.StopPowerShellReceived += new EventHandler(this.HandleStopReceived);
            dsHandler.HostResponseReceived += new EventHandler<RemoteDataEventArgs<RemoteHostResponse>>(this.HandleHostResponseReceived);
            dsHandler.OnSessionConnected += new EventHandler(this.HandleSessionConnected);
        }

        private void RegisterPipelineOutputEventHandlers(PSDataCollection<PSObject> pipelineOutput)
        {
            pipelineOutput.DataAdded += new EventHandler<DataAddedEventArgs>(this.HandleOutputDataAdded);
        }

        private void RegisterPowerShellEventHandlers(PowerShell powerShell)
        {
            powerShell.InvocationStateChanged += new EventHandler<PSInvocationStateChangedEventArgs>(this.HandlePowerShellInvocationStateChanged);
            powerShell.Streams.Error.DataAdded += new EventHandler<DataAddedEventArgs>(this.HandleErrorDataAdded);
            powerShell.Streams.Debug.DataAdded += new EventHandler<DataAddedEventArgs>(this.HandleDebugAdded);
            powerShell.Streams.Verbose.DataAdded += new EventHandler<DataAddedEventArgs>(this.HandleVerboseAdded);
            powerShell.Streams.Warning.DataAdded += new EventHandler<DataAddedEventArgs>(this.HandleWarningAdded);
            powerShell.Streams.Progress.DataAdded += new EventHandler<DataAddedEventArgs>(this.HandleProgressAdded);
        }

        private void SendRemainingData()
        {
            int index = !this.extraPowerShellAlreadyScheduled ? 0 : 1;
            lock (this.syncObject)
            {
                this.datasent[index] = true;
            }
            try
            {
                for (int i = 0; i < this.localPowerShellOutput.Count; i++)
                {
                    PSObject data = this.localPowerShellOutput[i];
                    this.dsHandler.SendOutputDataToClient(data);
                }
                this.localPowerShellOutput.Clear();
                for (int j = 0; j < this.localPowerShell.Streams.Error.Count; j++)
                {
                    ErrorRecord errorRecord = this.localPowerShell.Streams.Error[j];
                    this.dsHandler.SendErrorRecordToClient(errorRecord);
                }
                this.localPowerShell.Streams.Error.Clear();
            }
            finally
            {
                lock (this.syncObject)
                {
                    this.datasent[index] = true;
                }
            }
        }

        internal IAsyncResult Start()
        {
            return this.Start(true);
        }

        private IAsyncResult Start(bool startMainPowerShell)
        {
            if (startMainPowerShell)
            {
                this.dsHandler.Prepare();
            }
            PSInvocationSettings settings = new PSInvocationSettings {
                ApartmentState = this.apartmentState,
                Host = this.remoteHost
            };
			TokenImpersonationLevel impersonation = OSHelper.IsUnix ? TokenImpersonationLevel.Identification : WindowsIdentity.GetCurrent().ImpersonationLevel;
            switch (impersonation)
            {
                case TokenImpersonationLevel.Impersonation:
                case TokenImpersonationLevel.Delegation:
                    settings.FlowImpersonationPolicy = true;
                    break;

                default:
                    settings.FlowImpersonationPolicy = false;
                    break;
            }
            settings.AddToHistory = this.addToHistory;
            if (startMainPowerShell)
            {
                return this.localPowerShell.BeginInvoke<object, PSObject>(this.input, this.localPowerShellOutput, settings, null, null);
            }
            return this.extraPowerShell.BeginInvoke<object, PSObject>(this.input, this.localPowerShellOutput, settings, null, null);
        }

        private void UnregisterDataStructureHandlerEventHandlers(ServerPowerShellDataStructureHandler dsHandler)
        {
            dsHandler.InputEndReceived -= new EventHandler(this.HandleInputEndReceived);
            dsHandler.InputReceived -= new EventHandler<RemoteDataEventArgs<object>>(this.HandleInputReceived);
            dsHandler.StopPowerShellReceived -= new EventHandler(this.HandleStopReceived);
            dsHandler.HostResponseReceived -= new EventHandler<RemoteDataEventArgs<RemoteHostResponse>>(this.HandleHostResponseReceived);
            dsHandler.OnSessionConnected -= new EventHandler(this.HandleSessionConnected);
        }

        private void UnregisterPipelineOutputEventHandlers(PSDataCollection<PSObject> pipelineOutput)
        {
            pipelineOutput.DataAdded -= new EventHandler<DataAddedEventArgs>(this.HandleOutputDataAdded);
        }

        private void UnregisterPowerShellEventHandlers(PowerShell powerShell)
        {
            powerShell.InvocationStateChanged -= new EventHandler<PSInvocationStateChangedEventArgs>(this.HandlePowerShellInvocationStateChanged);
            powerShell.Streams.Error.DataAdded -= new EventHandler<DataAddedEventArgs>(this.HandleErrorDataAdded);
            powerShell.Streams.Debug.DataAdded -= new EventHandler<DataAddedEventArgs>(this.HandleDebugAdded);
            powerShell.Streams.Verbose.DataAdded -= new EventHandler<DataAddedEventArgs>(this.HandleVerboseAdded);
            powerShell.Streams.Warning.DataAdded -= new EventHandler<DataAddedEventArgs>(this.HandleWarningAdded);
            powerShell.Streams.Progress.DataAdded -= new EventHandler<DataAddedEventArgs>(this.HandleProgressAdded);
        }

        internal ServerPowerShellDataStructureHandler DataStructureHandler
        {
            get
            {
                return this.dsHandler;
            }
        }

        internal PSDataCollection<object> InputCollection
        {
            get
            {
                return this.input;
            }
        }

        internal Guid InstanceId
        {
            get
            {
                return this.clientPowerShellId;
            }
        }

        internal PowerShell LocalPowerShell
        {
            get
            {
                return this.localPowerShell;
            }
        }

        internal System.Management.Automation.RemoteStreamOptions RemoteStreamOptions
        {
            get
            {
                return this.remoteStreamOptions;
            }
        }

        internal Guid RunspacePoolId
        {
            get
            {
                return this.clientRunspacePoolId;
            }
        }
    }
}

