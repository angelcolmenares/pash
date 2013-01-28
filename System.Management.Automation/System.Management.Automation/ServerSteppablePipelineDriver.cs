namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;
    using System.Threading;

    internal class ServerSteppablePipelineDriver
    {
        private bool addToHistory;
        private ApartmentState apartmentState;
        private Guid clientPowerShellId;
        private Guid clientRunspacePoolId;
        private ServerPowerShellDataStructureHandler dsHandler;
        private ServerSteppablePipelineSubscriber eventSubscriber;
        private PSDataCollection<object> input;
        private IEnumerator<object> inputEnumerator;
        private bool isProcessingInput;
        private bool isPulsed;
        private PowerShell localPowerShell;
        private bool noInput;
        private PSDataCollection<object> powershellInput;
        private ServerRemoteHost remoteHost;
        private System.Management.Automation.RemoteStreamOptions remoteStreamOptions;
        private PSInvocationState stateOfSteppablePipeline;
        private System.Management.Automation.SteppablePipeline steppablePipeline;
        private object syncObject = new object();
        private int totalObjectsProcessedSoFar;

        internal ServerSteppablePipelineDriver(PowerShell powershell, bool noInput, Guid clientPowerShellId, Guid clientRunspacePoolId, ServerRunspacePoolDriver runspacePoolDriver, ApartmentState apartmentState, HostInfo hostInfo, System.Management.Automation.RemoteStreamOptions streamOptions, bool addToHistory, Runspace rsToUse, ServerSteppablePipelineSubscriber eventSubscriber, PSDataCollection<object> powershellInput)
        {
            this.localPowerShell = powershell;
            this.clientPowerShellId = clientPowerShellId;
            this.clientRunspacePoolId = clientRunspacePoolId;
            this.remoteStreamOptions = streamOptions;
            this.apartmentState = apartmentState;
            this.noInput = noInput;
            this.addToHistory = addToHistory;
            this.eventSubscriber = eventSubscriber;
            this.powershellInput = powershellInput;
            this.input = new PSDataCollection<object>();
            this.inputEnumerator = this.input.GetEnumerator();
            this.input.ReleaseOnEnumeration = true;
            this.dsHandler = runspacePoolDriver.DataStructureHandler.CreatePowerShellDataStructureHandler(clientPowerShellId, clientRunspacePoolId, this.remoteStreamOptions, null);
            this.remoteHost = this.dsHandler.GetHostAssociatedWithPowerShell(hostInfo, runspacePoolDriver.ServerRemoteHost);
            this.dsHandler.InputEndReceived += new EventHandler(this.HandleInputEndReceived);
            this.dsHandler.InputReceived += new EventHandler<RemoteDataEventArgs<object>>(this.HandleInputReceived);
            this.dsHandler.StopPowerShellReceived += new EventHandler(this.HandleStopReceived);
            this.dsHandler.HostResponseReceived += new EventHandler<RemoteDataEventArgs<RemoteHostResponse>>(this.HandleHostResponseReceived);
            this.dsHandler.OnSessionConnected += new EventHandler(this.HandleSessionConnected);
            if (rsToUse == null)
            {
                throw PSTraceSource.NewInvalidOperationException("RemotingErrorIdStrings", "NestedPipelineMissingRunspace", new object[0]);
            }
            this.localPowerShell.Runspace = rsToUse;
            eventSubscriber.SubscribeEvents(this);
            this.stateOfSteppablePipeline = PSInvocationState.NotStarted;
        }

        internal void CheckAndPulseForProcessing(bool complete)
        {
            if (complete)
            {
                this.eventSubscriber.FireHandleProcessRecord(this);
            }
            else if (!this.isPulsed)
            {
                bool flag = false;
                lock (this.syncObject)
                {
                    if (this.isPulsed)
                    {
                        return;
                    }
                    if (!this.isProcessingInput && (this.input.Count > this.totalObjectsProcessedSoFar))
                    {
                        flag = true;
                        this.isPulsed = true;
                    }
                }
                if (flag && (this.stateOfSteppablePipeline == PSInvocationState.Running))
                {
                    this.eventSubscriber.FireHandleProcessRecord(this);
                }
            }
        }

        internal void HandleHostResponseReceived(object sender, RemoteDataEventArgs<RemoteHostResponse> eventArgs)
        {
            this.remoteHost.ServerMethodExecutor.HandleRemoteHostResponseFromClient(eventArgs.Data);
        }

        internal void HandleInputEndReceived(object sender, EventArgs eventArgs)
        {
            this.input.Complete();
            this.CheckAndPulseForProcessing(true);
            if (this.powershellInput != null)
            {
                this.powershellInput.Pulse();
            }
        }

        private void HandleInputReceived(object sender, RemoteDataEventArgs<object> eventArgs)
        {
            if (this.input != null)
            {
                lock (this.syncObject)
                {
                    this.input.Add(eventArgs.Data);
                }
                this.CheckAndPulseForProcessing(false);
                if (this.powershellInput != null)
                {
                    this.powershellInput.Pulse();
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
            lock (this.syncObject)
            {
                this.stateOfSteppablePipeline = PSInvocationState.Stopping;
            }
            this.PerformStop();
            if (this.powershellInput != null)
            {
                this.powershellInput.Pulse();
            }
        }

        internal void PerformStop()
        {
            bool flag = false;
            lock (this.syncObject)
            {
                if (!this.isProcessingInput && (this.stateOfSteppablePipeline == PSInvocationState.Stopping))
                {
                    flag = true;
                }
            }
            if (flag)
            {
                this.SetState(PSInvocationState.Stopped, new PipelineStoppedException());
            }
        }

        internal void SetState(PSInvocationState newState, Exception reason)
        {
            PSInvocationState notStarted = PSInvocationState.NotStarted;
            bool flag = false;
            lock (this.syncObject)
            {
                switch (this.stateOfSteppablePipeline)
                {
                    case PSInvocationState.NotStarted:
                        goto Label_00C7;

                    case PSInvocationState.Running:
                        switch (newState)
                        {
                            case PSInvocationState.NotStarted:
                                throw new InvalidOperationException();

                            case PSInvocationState.Stopping:
                                goto Label_0091;

                            case PSInvocationState.Stopped:
                            case PSInvocationState.Completed:
                            case PSInvocationState.Failed:
                                goto Label_0095;
                        }
                        goto Label_00C7;

                    case PSInvocationState.Stopping:
                        switch (newState)
                        {
                            case PSInvocationState.Stopped:
                                goto Label_00BB;

                            case PSInvocationState.Completed:
                            case PSInvocationState.Failed:
                                goto Label_00B5;
                        }
                        throw new InvalidOperationException();

                    default:
                        goto Label_00C7;
                }
                notStarted = newState;
                goto Label_00C7;
            Label_0091:
                notStarted = newState;
                goto Label_00C7;
            Label_0095:
                notStarted = newState;
                flag = true;
                goto Label_00C7;
            Label_00B5:
                notStarted = PSInvocationState.Stopped;
                flag = true;
                goto Label_00C7;
            Label_00BB:
                notStarted = newState;
                flag = true;
            Label_00C7:
                this.stateOfSteppablePipeline = notStarted;
            }
            if (flag)
            {
                this.dsHandler.SendStateChangedInformationToClient(new PSInvocationStateInfo(notStarted, reason));
            }
            if (((this.stateOfSteppablePipeline == PSInvocationState.Completed) || (this.stateOfSteppablePipeline == PSInvocationState.Stopped)) || (this.stateOfSteppablePipeline == PSInvocationState.Failed))
            {
                this.dsHandler.RaiseRemoveAssociationEvent();
            }
        }

        internal void Start()
        {
            this.stateOfSteppablePipeline = PSInvocationState.Running;
            this.eventSubscriber.FireStartSteppablePipeline(this);
            if (this.powershellInput != null)
            {
                this.powershellInput.Pulse();
            }
        }

        internal ServerPowerShellDataStructureHandler DataStructureHandler
        {
            get
            {
                return this.dsHandler;
            }
        }

        internal PSDataCollection<object> Input
        {
            get
            {
                return this.input;
            }
        }

        internal IEnumerator<object> InputEnumerator
        {
            get
            {
                return this.inputEnumerator;
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

        internal bool NoInput
        {
            get
            {
                return this.noInput;
            }
        }

        internal PSInvocationState PipelineState
        {
            get
            {
                return this.stateOfSteppablePipeline;
            }
        }

        internal bool ProcessingInput
        {
            get
            {
                return this.isProcessingInput;
            }
            set
            {
                this.isProcessingInput = value;
            }
        }

        internal bool Pulsed
        {
            get
            {
                return this.isPulsed;
            }
            set
            {
                this.isPulsed = value;
            }
        }

        internal ServerRemoteHost RemoteHost
        {
            get
            {
                return this.remoteHost;
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

        internal System.Management.Automation.SteppablePipeline SteppablePipeline
        {
            get
            {
                return this.steppablePipeline;
            }
            set
            {
                this.steppablePipeline = value;
            }
        }

        internal object SyncObject
        {
            get
            {
                return this.syncObject;
            }
        }

        internal int TotalObjectsProcessed
        {
            get
            {
                return this.totalObjectsProcessedSoFar;
            }
            set
            {
                this.totalObjectsProcessedSoFar = value;
            }
        }
    }
}

