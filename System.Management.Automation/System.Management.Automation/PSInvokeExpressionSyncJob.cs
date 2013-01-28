namespace System.Management.Automation
{
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;

    internal class PSInvokeExpressionSyncJob : PSRemotingChildJob
    {
        private bool cleanupDone;
        private bool doFinishCalled;
        private List<ExecutionCmdletHelper> helpers = new List<ExecutionCmdletHelper>();
        private int pipelineDisconnectedCount;
        private int pipelineFinishedCount;
        private Dictionary<Guid, PowerShell> powershells = new Dictionary<Guid, PowerShell>();
        private ThrottleManager throttleManager;

        internal PSInvokeExpressionSyncJob(List<IThrottleOperation> operations, ThrottleManager throttleManager)
        {
            base.UsesResultsCollection = true;
            base.Results.AddRef();
            this.throttleManager = throttleManager;
            base.RegisterThrottleComplete(this.throttleManager);
            foreach (IThrottleOperation operation in operations)
            {
                ExecutionCmdletHelper item = operation as ExecutionCmdletHelper;
                RemoteRunspace runspace = item.Pipeline.Runspace as RemoteRunspace;
                if ((runspace != null) && (runspace.RunspaceStateInfo.State == RunspaceState.BeforeOpen))
                {
                    runspace.URIRedirectionReported += new EventHandler<RemoteDataEventArgs<Uri>>(this.HandleURIDirectionReported);
                    runspace.StateChanged += new EventHandler<RunspaceStateEventArgs>(this.HandleRunspaceStateChanged);
                }
                this.helpers.Add(item);
                base.AggregateResultsFromHelper(item);
                RemotePipeline pipeline = item.Pipeline as RemotePipeline;
                this.powershells.Add(pipeline.PowerShell.InstanceId, pipeline.PowerShell);
            }
        }

        private void CheckForAndSetDisconnectedState(PipelineState pipelineState)
        {
            bool flag;
            lock (base.SyncObject)
            {
                if (this.IsTerminalState())
                {
                    return;
                }
                switch (pipelineState)
                {
                    case PipelineState.Stopped:
                    case PipelineState.Completed:
                    case PipelineState.Failed:
                        this.pipelineFinishedCount++;
                        break;

                    case PipelineState.Disconnected:
                        this.pipelineDisconnectedCount++;
                        break;
                }
                flag = ((this.pipelineFinishedCount + this.pipelineDisconnectedCount) == this.helpers.Count) && (this.pipelineDisconnectedCount > 0);
            }
            if (flag)
            {
                base.SetJobState(JobState.Disconnected);
            }
        }

        internal PSRemotingJob CreateDisconnectedRemotingJob()
        {
            List<IThrottleOperation> helpers = new List<IThrottleOperation>();
            foreach (ExecutionCmdletHelper helper in this.helpers)
            {
                if (helper.Pipeline.PipelineStateInfo.State == PipelineState.Disconnected)
                {
                    base.RemoveAggreateCallbacksFromHelper(helper);
                    helpers.Add(new DisconnectedJobOperation(helper.Pipeline));
                }
            }
            if (helpers.Count == 0)
            {
                return null;
            }
            return new PSRemotingJob(helpers, 0, base.Name, true);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        protected override void DoCleanupOnFinished()
        {
            bool flag = false;
            if (!this.cleanupDone)
            {
                lock (base.SyncObject)
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
                foreach (ExecutionCmdletHelper helper in this.helpers)
                {
                    base.StopAggregateResultsFromHelper(helper);
                }
                base.UnregisterThrottleComplete(this.throttleManager);
                base.Results.DecrementRef();
            }
        }

        protected override void DoFinish()
        {
            if (!this.doFinishCalled)
            {
                lock (base.SyncObject)
                {
                    if (this.doFinishCalled)
                    {
                        return;
                    }
                    this.doFinishCalled = true;
                }
                foreach (ExecutionCmdletHelper helper in this.helpers)
                {
                    base.DeterminedAndSetJobState(helper);
                }
                if ((this.helpers.Count == 0) && (base.JobStateInfo.State == JobState.NotStarted))
                {
                    base.SetJobState(JobState.Completed);
                }
                this.DoCleanupOnFinished();
            }
        }

        internal override PowerShell GetPowerShell(Guid instanceId)
        {
            PowerShell shell = null;
            this.powershells.TryGetValue(instanceId, out shell);
            return shell;
        }

        internal Collection<PowerShell> GetPowerShells()
        {
            Collection<PowerShell> collection = new Collection<PowerShell>();
            foreach (PowerShell shell in this.powershells.Values)
            {
                collection.Add(shell);
            }
            return collection;
        }

        protected override void HandleOperationComplete(object sender, OperationStateEventArgs stateEventArgs)
        {
            Exception exception;
            ErrorRecord record;
            ExecutionCmdletHelper helper = sender as ExecutionCmdletHelper;
            base.ProcessJobFailure(helper, out exception, out record);
            if (record != null)
            {
                this.WriteError(record);
            }
        }

        protected override void HandlePipelineStateChanged(object sender, PipelineStateEventArgs e)
        {
            PipelineState pipelineState = e.PipelineStateInfo.State;
            switch (pipelineState)
            {
                case PipelineState.Running:
                    base.SetJobState(JobState.Running);
                    return;

                case PipelineState.Stopping:
                    break;

                case PipelineState.Stopped:
                case PipelineState.Completed:
                case PipelineState.Failed:
                case PipelineState.Disconnected:
                    this.CheckForAndSetDisconnectedState(pipelineState);
                    break;

                default:
                    return;
            }
        }

        private void HandleRunspaceStateChanged(object sender, RunspaceStateEventArgs e)
        {
            RemoteRunspace runspace = sender as RemoteRunspace;
            if ((runspace != null) && (e.RunspaceStateInfo.State != RunspaceState.Opening))
            {
                runspace.URIRedirectionReported -= new EventHandler<RemoteDataEventArgs<Uri>>(this.HandleURIDirectionReported);
                runspace.StateChanged -= new EventHandler<RunspaceStateEventArgs>(this.HandleRunspaceStateChanged);
            }
        }

        internal bool IsTerminalState()
        {
            if (!base.IsFinishedState(base.JobStateInfo.State))
            {
                return (base.JobStateInfo.State == JobState.Disconnected);
            }
            return true;
        }

        internal void StartOperations(List<IThrottleOperation> operations)
        {
            this.throttleManager.SubmitOperations(operations);
            this.throttleManager.EndSubmitOperations();
        }

        public override void StopJob()
        {
            this.throttleManager.StopAllOperations();
        }
    }
}

