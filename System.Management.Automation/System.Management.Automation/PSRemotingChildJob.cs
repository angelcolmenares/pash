namespace System.Management.Automation
{
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Remoting.Client;
    using System.Management.Automation.Runspaces;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class PSRemotingChildJob : Job
    {
        private bool cleanupDone;
        private bool disconnectedBlocked;
        private bool doFinishCalled;
        private ErrorRecord failureErrorRecord;
        private ExecutionCmdletHelper helper;
        private bool hideComputerName;
        private bool isDisposed;
        private RemotePipeline remotePipeline;
        private System.Management.Automation.Runspaces.Runspace remoteRunspace;
        private bool stopIsCalled;
        protected object SyncObject;
        private ThrottleManager throttleManager;

        internal event EventHandler JobUnblocked;

        protected PSRemotingChildJob()
        {
            this.hideComputerName = true;
            this.SyncObject = new object();
        }

        internal PSRemotingChildJob(ExecutionCmdletHelper helper, ThrottleManager throttleManager, bool aggregateResults = false)
        {
            this.hideComputerName = true;
            this.SyncObject = new object();
            base.UsesResultsCollection = true;
            this.helper = helper;
            this.remotePipeline = helper.Pipeline as RemotePipeline;
            this.remoteRunspace = helper.Pipeline.Runspace;
            this.throttleManager = throttleManager;
            if (aggregateResults)
            {
                this.AggregateResultsFromHelper(helper);
            }
            else
            {
                this.remotePipeline.StateChanged += new EventHandler<PipelineStateEventArgs>(this.HandlePipelineStateChanged);
                this.remotePipeline.Output.DataReady += new EventHandler(this.HandleOutputReady);
                this.remotePipeline.Error.DataReady += new EventHandler(this.HandleErrorReady);
            }
            IThrottleOperation operation = helper;
            operation.OperationComplete += new EventHandler<OperationStateEventArgs>(this.HandleOperationComplete);
            base.SetJobState(JobState.Disconnected, null);
        }

        internal PSRemotingChildJob(string remoteCommand, ExecutionCmdletHelper helper, ThrottleManager throttleManager) : base(remoteCommand)
        {
            this.hideComputerName = true;
            this.SyncObject = new object();
            base.UsesResultsCollection = true;
            this.helper = helper;
            this.remoteRunspace = helper.Pipeline.Runspace;
            this.remotePipeline = helper.Pipeline as RemotePipeline;
            this.throttleManager = throttleManager;
            RemoteRunspace remoteRunspace = this.remoteRunspace as RemoteRunspace;
            if ((remoteRunspace != null) && (remoteRunspace.RunspaceStateInfo.State == RunspaceState.BeforeOpen))
            {
                remoteRunspace.URIRedirectionReported += new EventHandler<RemoteDataEventArgs<Uri>>(this.HandleURIDirectionReported);
            }
            this.AggregateResultsFromHelper(helper);
            this.RegisterThrottleComplete(throttleManager);
        }

        protected void AggregateResultsFromHelper(ExecutionCmdletHelper helper)
        {
            Pipeline pipeline = helper.Pipeline;
            pipeline.Output.DataReady += new EventHandler(this.HandleOutputReady);
            pipeline.Error.DataReady += new EventHandler(this.HandleErrorReady);
            pipeline.StateChanged += new EventHandler<PipelineStateEventArgs>(this.HandlePipelineStateChanged);
            RemotePipeline pipeline2 = pipeline as RemotePipeline;
            pipeline2.MethodExecutorStream.DataReady += new EventHandler(this.HandleHostCalls);
            pipeline2.PowerShell.Streams.Progress.DataAdded += new EventHandler<DataAddedEventArgs>(this.HandleProgressAdded);
            pipeline2.PowerShell.Streams.Warning.DataAdded += new EventHandler<DataAddedEventArgs>(this.HandleWarningAdded);
            pipeline2.PowerShell.Streams.Verbose.DataAdded += new EventHandler<DataAddedEventArgs>(this.HandleVerboseAdded);
            pipeline2.PowerShell.Streams.Debug.DataAdded += new EventHandler<DataAddedEventArgs>(this.HandleDebugAdded);
            pipeline2.IsMethodExecutorStreamEnabled = true;
            IThrottleOperation operation = helper;
            operation.OperationComplete += new EventHandler<OperationStateEventArgs>(this.HandleOperationComplete);
        }

        internal void ConnectAsync()
        {
            if (base.JobStateInfo.State != JobState.Disconnected)
            {
                throw new InvalidJobStateException(base.JobStateInfo.State);
            }
            this.remotePipeline.ConnectAsync();
        }

        protected void DeterminedAndSetJobState(ExecutionCmdletHelper helper)
        {
            Exception exception;
            this.ProcessJobFailure(helper, out exception, out this.failureErrorRecord);
            if (exception != null)
            {
                base.SetJobState(JobState.Failed, exception);
            }
            else
            {
                switch (helper.Pipeline.PipelineStateInfo.State)
                {
                    case PipelineState.NotStarted:
                        base.SetJobState(JobState.Stopped);
                        return;

                    case PipelineState.Completed:
                        base.SetJobState(JobState.Completed);
                        return;
                }
                base.SetJobState(JobState.Stopped);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !this.isDisposed)
            {
                lock (this.SyncObject)
                {
                    if (this.isDisposed)
                    {
                        return;
                    }
                    this.isDisposed = true;
                }
                try
                {
                    this.DoCleanupOnFinished();
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }
        }

        protected virtual void DoCleanupOnFinished()
        {
            bool flag = false;
            if (!this.cleanupDone)
            {
                lock (this.SyncObject)
                {
                    if (!this.cleanupDone)
                    {
                        this.cleanupDone = true;
                        flag = true;
                    }
                }
            }
            if (flag)
            {
                this.StopAggregateResultsFromHelper(this.helper);
                this.helper.OperationComplete -= new EventHandler<OperationStateEventArgs>(this.HandleOperationComplete);
                this.UnregisterThrottleComplete(this.throttleManager);
                this.throttleManager = null;
            }
        }

        protected virtual void DoFinish()
        {
            if (!this.doFinishCalled)
            {
                lock (this.SyncObject)
                {
                    if (this.doFinishCalled)
                    {
                        return;
                    }
                    this.doFinishCalled = true;
                }
                this.DeterminedAndSetJobState(this.helper);
                this.DoCleanupOnFinished();
            }
        }

        private PowerShell GetPipelinePowerShell(RemotePipeline pipeline, Guid instanceId)
        {
            if (pipeline != null)
            {
                return pipeline.PowerShell;
            }
            return this.GetPowerShell(instanceId);
        }

        internal PowerShell GetPowerShell()
        {
            PowerShell powerShell = null;
            if (this.remotePipeline != null)
            {
                powerShell = this.remotePipeline.PowerShell;
            }
            return powerShell;
        }

        internal virtual PowerShell GetPowerShell(Guid instanceId)
        {
            throw PSTraceSource.NewInvalidOperationException();
        }

        private void HandleDebugAdded(object sender, DataAddedEventArgs eventArgs)
        {
            int index = eventArgs.Index;
            PowerShell pipelinePowerShell = this.GetPipelinePowerShell(this.remotePipeline, eventArgs.PowerShellInstanceId);
            if (pipelinePowerShell != null)
            {
                base.Debug.Add(pipelinePowerShell.Streams.Debug[index]);
            }
        }

        private void HandleErrorReady(object sender, EventArgs eventArgs)
        {
            PSDataCollectionPipelineReader<ErrorRecord, object> reader = sender as PSDataCollectionPipelineReader<ErrorRecord, object>;
            foreach (object obj2 in reader.NonBlockingRead())
            {
                ErrorRecord errorRecord = obj2 as ErrorRecord;
                if (errorRecord != null)
                {
                    OriginInfo originInfo = new OriginInfo(reader.ComputerName, reader.RunspaceId);
                    RemotingErrorRecord record2 = new RemotingErrorRecord(errorRecord, originInfo) {
                        PreserveInvocationInfoOnce = true
                    };
                    this.WriteError(record2);
                }
            }
        }

        private void HandleHostCalls(object sender, EventArgs eventArgs)
        {
            ObjectStream stream = sender as ObjectStream;
            if (stream != null)
            {
                Collection<object> collection = stream.NonBlockingRead(stream.Count);
                lock (this.SyncObject)
                {
                    foreach (ClientMethodExecutor executor in collection)
                    {
                        base.Results.Add(new PSStreamObject(PSStreamObjectType.MethodExecutor, executor));
                        if (executor.RemoteHostCall.CallId != -100L)
                        {
                            base.SetJobState(JobState.Blocked, null);
                        }
                    }
                }
            }
        }

        protected virtual void HandleOperationComplete(object sender, OperationStateEventArgs stateEventArgs)
        {
            ExecutionCmdletHelper helper = sender as ExecutionCmdletHelper;
            this.DeterminedAndSetJobState(helper);
        }

        private void HandleOutputReady(object sender, EventArgs eventArgs)
        {
            PSDataCollectionPipelineReader<PSObject, PSObject> reader = sender as PSDataCollectionPipelineReader<PSObject, PSObject>;
            foreach (PSObject obj2 in reader.NonBlockingRead())
            {
                if (obj2 != null)
                {
                    if (obj2.Properties[RemotingConstants.ComputerNameNoteProperty] != null)
                    {
                        obj2.Properties.Remove(RemotingConstants.ComputerNameNoteProperty);
                    }
                    if (obj2.Properties[RemotingConstants.RunspaceIdNoteProperty] != null)
                    {
                        obj2.Properties.Remove(RemotingConstants.RunspaceIdNoteProperty);
                    }
                    obj2.Properties.Add(new PSNoteProperty(RemotingConstants.ComputerNameNoteProperty, reader.ComputerName));
                    obj2.Properties.Add(new PSNoteProperty(RemotingConstants.RunspaceIdNoteProperty, reader.RunspaceId));
                    if (obj2.Properties[RemotingConstants.ShowComputerNameNoteProperty] == null)
                    {
                        PSNoteProperty member = new PSNoteProperty(RemotingConstants.ShowComputerNameNoteProperty, !this.hideComputerName);
                        obj2.Properties.Add(member);
                    }
                }
                this.WriteObject(obj2);
            }
        }

        protected virtual void HandlePipelineStateChanged(object sender, PipelineStateEventArgs e)
        {
            if ((this.remoteRunspace != null) && (e.PipelineStateInfo.State != PipelineState.Running))
            {
                ((RemoteRunspace) this.remoteRunspace).URIRedirectionReported -= new EventHandler<RemoteDataEventArgs<Uri>>(this.HandleURIDirectionReported);
            }
            PipelineState state = e.PipelineStateInfo.State;
            if (state != PipelineState.Running)
            {
                if (state != PipelineState.Disconnected)
                {
                    return;
                }
            }
            else
            {
                if (this.disconnectedBlocked)
                {
                    this.disconnectedBlocked = false;
                    base.SetJobState(JobState.Blocked);
                    return;
                }
                base.SetJobState(JobState.Running);
                return;
            }
            this.disconnectedBlocked = base.JobStateInfo.State == JobState.Blocked;
            base.SetJobState(JobState.Disconnected);
        }

        private void HandleProgressAdded(object sender, DataAddedEventArgs eventArgs)
        {
            int index = eventArgs.Index;
            PowerShell pipelinePowerShell = this.GetPipelinePowerShell(this.remotePipeline, eventArgs.PowerShellInstanceId);
            if (pipelinePowerShell != null)
            {
                base.Progress.Add(pipelinePowerShell.Streams.Progress[index]);
            }
        }

        private void HandleThrottleComplete(object sender, EventArgs eventArgs)
        {
            this.DoFinish();
        }

        protected void HandleURIDirectionReported(object sender, RemoteDataEventArgs<Uri> eventArgs)
        {
            string message = StringUtil.Format(RemotingErrorIdStrings.URIRedirectWarningToHost, eventArgs.Data.OriginalString);
            this.WriteWarning(message);
        }

        private void HandleVerboseAdded(object sender, DataAddedEventArgs eventArgs)
        {
            int index = eventArgs.Index;
            PowerShell pipelinePowerShell = this.GetPipelinePowerShell(this.remotePipeline, eventArgs.PowerShellInstanceId);
            if (pipelinePowerShell != null)
            {
                base.Verbose.Add(pipelinePowerShell.Streams.Verbose[index]);
            }
        }

        private void HandleWarningAdded(object sender, DataAddedEventArgs eventArgs)
        {
            int index = eventArgs.Index;
            PowerShell pipelinePowerShell = this.GetPipelinePowerShell(this.remotePipeline, eventArgs.PowerShellInstanceId);
            if (pipelinePowerShell != null)
            {
                WarningRecord item = pipelinePowerShell.Streams.Warning[index];
                base.Warning.Add(item);
                base.Results.Add(new PSStreamObject(PSStreamObjectType.WarningRecord, item));
            }
        }

        protected void ProcessJobFailure(ExecutionCmdletHelper helper, out Exception failureException, out ErrorRecord failureErrorRecord)
        {
            RemotePipeline pipeline = helper.Pipeline as RemotePipeline;
            RemoteRunspace runspace = pipeline.GetRunspace() as RemoteRunspace;
            failureException = null;
            failureErrorRecord = null;
            if (helper.InternalException != null)
            {
                string errorId = "RemotePipelineExecutionFailed";
                failureException = helper.InternalException;
                if ((failureException is InvalidRunspaceStateException) || (failureException is InvalidRunspacePoolStateException))
                {
                    errorId = "InvalidSessionState";
                    if (!string.IsNullOrEmpty(failureException.Source))
                    {
                        errorId = string.Format(CultureInfo.InvariantCulture, "{0},{1}", new object[] { errorId, failureException.Source });
                    }
                }
                failureErrorRecord = new ErrorRecord(helper.InternalException, errorId, ErrorCategory.OperationStopped, helper);
            }
            else if (runspace.RunspaceStateInfo.State == RunspaceState.Broken)
            {
                failureException = runspace.RunspaceStateInfo.Reason;
                object computerName = runspace.ConnectionInfo.ComputerName;
                string str2 = null;
                PSRemotingTransportException exception = failureException as PSRemotingTransportException;
                string fQEIDFromTransportError = WSManTransportManagerUtils.GetFQEIDFromTransportError((exception != null) ? exception.ErrorCode : 0, "PSSessionStateBroken");
                if (exception != null)
                {
                    str2 = "[" + runspace.ConnectionInfo.ComputerName + "] ";
                    if (exception.ErrorCode == -2144108135)
                    {
                        string str4 = PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.URIRedirectionReported, new object[] { exception.Message, "MaximumConnectionRedirectionCount", "PSSessionOption", "AllowRedirection" });
                        str2 = str2 + str4;
                    }
                    else if (!string.IsNullOrEmpty(exception.Message))
                    {
                        str2 = str2 + exception.Message;
                    }
                    else if (!string.IsNullOrEmpty(exception.TransportMessage))
                    {
                        str2 = str2 + exception.TransportMessage;
                    }
                }
                if (failureException == null)
                {
                    failureException = new RuntimeException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.RemoteRunspaceOpenUnknownState, new object[] { runspace.RunspaceStateInfo.State }));
                }
                failureErrorRecord = new ErrorRecord(failureException, computerName, fQEIDFromTransportError, ErrorCategory.OpenError, null, null, null, null, null, str2, null);
            }
            else if (pipeline.PipelineStateInfo.State == PipelineState.Failed)
            {
                object targetObject = runspace.ConnectionInfo.ComputerName;
                failureException = pipeline.PipelineStateInfo.Reason;
                if (failureException != null)
                {
                    RemoteException exception2 = failureException as RemoteException;
                    ErrorRecord errorRecord = null;
                    if (exception2 != null)
                    {
                        errorRecord = exception2.ErrorRecord;
                    }
                    else
                    {
                        errorRecord = new ErrorRecord(pipeline.PipelineStateInfo.Reason, "JobFailure", ErrorCategory.OperationStopped, targetObject);
                    }
                    string str5 = ((RemoteRunspace) pipeline.GetRunspace()).ConnectionInfo.ComputerName;
                    Guid instanceId = pipeline.GetRunspace().InstanceId;
                    OriginInfo originInfo = new OriginInfo(str5, instanceId);
                    failureErrorRecord = new RemotingErrorRecord(errorRecord, originInfo);
                }
            }
        }

        protected void RegisterThrottleComplete(ThrottleManager throttleManager)
        {
            throttleManager.ThrottleComplete += new EventHandler<EventArgs>(this.HandleThrottleComplete);
        }

        protected void RemoveAggreateCallbacksFromHelper(ExecutionCmdletHelper helper)
        {
            Pipeline pipeline = helper.Pipeline;
            pipeline.Output.DataReady -= new EventHandler(this.HandleOutputReady);
            pipeline.Error.DataReady -= new EventHandler(this.HandleErrorReady);
            pipeline.StateChanged -= new EventHandler<PipelineStateEventArgs>(this.HandlePipelineStateChanged);
            RemotePipeline pipeline2 = pipeline as RemotePipeline;
            pipeline2.MethodExecutorStream.DataReady -= new EventHandler(this.HandleHostCalls);
            if (pipeline2.PowerShell != null)
            {
                pipeline2.PowerShell.Streams.Progress.DataAdded -= new EventHandler<DataAddedEventArgs>(this.HandleProgressAdded);
                pipeline2.PowerShell.Streams.Warning.DataAdded -= new EventHandler<DataAddedEventArgs>(this.HandleWarningAdded);
                pipeline2.PowerShell.Streams.Verbose.DataAdded -= new EventHandler<DataAddedEventArgs>(this.HandleVerboseAdded);
                pipeline2.PowerShell.Streams.Debug.DataAdded -= new EventHandler<DataAddedEventArgs>(this.HandleDebugAdded);
                pipeline2.IsMethodExecutorStreamEnabled = false;
            }
        }

        protected void StopAggregateResultsFromHelper(ExecutionCmdletHelper helper)
        {
            this.RemoveAggreateCallbacksFromHelper(helper);
            helper.Pipeline.Dispose();
        }

        public override void StopJob()
        {
            if ((!this.isDisposed && !this.stopIsCalled) && !base.IsFinishedState(base.JobStateInfo.State))
            {
                lock (this.SyncObject)
                {
                    if ((this.isDisposed || this.stopIsCalled) || base.IsFinishedState(base.JobStateInfo.State))
                    {
                        return;
                    }
                    this.stopIsCalled = true;
                }
                this.throttleManager.StopOperation(this.helper);
                base.Finished.WaitOne();
            }
        }

        internal void UnblockJob()
        {
            base.SetJobState(JobState.Running, null);
            this.JobUnblocked.SafeInvoke(this, EventArgs.Empty);
        }

        protected void UnregisterThrottleComplete(ThrottleManager throttleManager)
        {
            throttleManager.ThrottleComplete -= new EventHandler<EventArgs>(this.HandleThrottleComplete);
        }

        internal override bool CanDisconnect
        {
            get
            {
                RemoteRunspace remoteRunspace = this.remoteRunspace as RemoteRunspace;
                if (remoteRunspace == null)
                {
                    return false;
                }
                return remoteRunspace.CanDisconnect;
            }
        }

        internal bool DisconnectedAndBlocked
        {
            get
            {
                return this.disconnectedBlocked;
            }
        }

        internal ErrorRecord FailureErrorRecord
        {
            get
            {
                return this.failureErrorRecord;
            }
        }

        public override bool HasMoreData
        {
            get
            {
                if (!base.Results.IsOpen)
                {
                    return (base.Results.Count > 0);
                }
                return true;
            }
        }

        internal ExecutionCmdletHelper Helper
        {
            get
            {
                return this.helper;
            }
        }

        internal bool HideComputerName
        {
            get
            {
                return this.hideComputerName;
            }
            set
            {
                this.hideComputerName = value;
                foreach (Job job in base.ChildJobs)
                {
                    PSRemotingChildJob job2 = job as PSRemotingChildJob;
                    if (job2 != null)
                    {
                        job2.HideComputerName = value;
                    }
                }
            }
        }

        public override string Location
        {
            get
            {
                if (this.remoteRunspace == null)
                {
                    return string.Empty;
                }
                return this.remoteRunspace.ConnectionInfo.ComputerName;
            }
        }

        public System.Management.Automation.Runspaces.Runspace Runspace
        {
            get
            {
                return this.remoteRunspace;
            }
        }

        public override string StatusMessage
        {
            get
            {
                return "";
            }
        }
    }
}

