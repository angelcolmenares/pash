namespace System.Management.Automation
{
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Collections.Generic;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;
    using System.Text;
    using System.Threading;

    internal class PSRemotingJob : Job
    {
        private bool _stopIsCalled;
        private readonly object _syncObject;
        private bool atleastOneChildJobFailed;
        private int blockedChildJobsCount;
        private int disconnectedChildJobsCount;
        private int finishedChildJobsCount;
        private bool hideComputerName;
        private bool isDisposed;
        private bool moreData;
        private string statusMessage;
        private ThrottleManager throttleManager;

        protected PSRemotingJob()
        {
            this.moreData = true;
            this.hideComputerName = true;
            this.throttleManager = new ThrottleManager();
            this._syncObject = new object();
        }

        internal PSRemotingJob(List<IThrottleOperation> helpers, int throttleLimit, string name, bool aggregateResults) : base(string.Empty, name)
        {
            this.moreData = true;
            this.hideComputerName = true;
            this.throttleManager = new ThrottleManager();
            this._syncObject = new object();
            foreach (ExecutionCmdletHelper helper in helpers)
            {
                PSRemotingChildJob item = new PSRemotingChildJob(helper, this.throttleManager, aggregateResults);
                item.StateChanged += new EventHandler<JobStateEventArgs>(this.HandleChildJobStateChanged);
                item.JobUnblocked += new EventHandler(this.HandleJobUnblocked);
                base.ChildJobs.Add(item);
            }
            base.CloseAllStreams();
            base.SetJobState(JobState.Disconnected);
            this.throttleManager.ThrottleLimit = throttleLimit;
            this.throttleManager.SubmitOperations(helpers);
            this.throttleManager.EndSubmitOperations();
        }

        internal PSRemotingJob(PSSession[] remoteRunspaceInfos, List<IThrottleOperation> runspaceHelpers, string remoteCommand, string name) : this(remoteRunspaceInfos, runspaceHelpers, remoteCommand, 0, name)
        {
        }

        internal PSRemotingJob(string[] computerNames, List<IThrottleOperation> computerNameHelpers, string remoteCommand, string name) : this(computerNames, computerNameHelpers, remoteCommand, 0, name)
        {
        }

        internal PSRemotingJob(PSSession[] remoteRunspaceInfos, List<IThrottleOperation> runspaceHelpers, string remoteCommand, int throttleLimit, string name) : base(remoteCommand, name)
        {
            this.moreData = true;
            this.hideComputerName = true;
            this.throttleManager = new ThrottleManager();
            this._syncObject = new object();
            for (int i = 0; i < remoteRunspaceInfos.Length; i++)
            {
                ExecutionCmdletHelperRunspace helper = (ExecutionCmdletHelperRunspace) runspaceHelpers[i];
                PSRemotingChildJob item = new PSRemotingChildJob(remoteCommand, helper, this.throttleManager);
                item.StateChanged += new EventHandler<JobStateEventArgs>(this.HandleChildJobStateChanged);
                item.JobUnblocked += new EventHandler(this.HandleJobUnblocked);
                base.ChildJobs.Add(item);
            }
            this.CommonInit(throttleLimit, runspaceHelpers);
        }

        internal PSRemotingJob(string[] computerNames, List<IThrottleOperation> computerNameHelpers, string remoteCommand, int throttleLimit, string name) : base(remoteCommand, name)
        {
            this.moreData = true;
            this.hideComputerName = true;
            this.throttleManager = new ThrottleManager();
            this._syncObject = new object();
            foreach (ExecutionCmdletHelperComputerName name2 in computerNameHelpers)
            {
                PSRemotingChildJob item = new PSRemotingChildJob(remoteCommand, name2, this.throttleManager);
                item.StateChanged += new EventHandler<JobStateEventArgs>(this.HandleChildJobStateChanged);
                item.JobUnblocked += new EventHandler(this.HandleJobUnblocked);
                base.ChildJobs.Add(item);
            }
            this.CommonInit(throttleLimit, computerNameHelpers);
        }

        private void CheckDisconnectedAndUpdateState(JobState newState, JobState prevState)
        {
            if (!base.IsFinishedState(base.JobStateInfo.State))
            {
                lock (this._syncObject)
                {
                    if (newState == JobState.Disconnected)
                    {
                        this.disconnectedChildJobsCount++;
                        if (prevState == JobState.Blocked)
                        {
                            this.blockedChildJobsCount--;
                        }
                        if (((this.disconnectedChildJobsCount + this.finishedChildJobsCount) + this.blockedChildJobsCount) == base.ChildJobs.Count)
                        {
                            base.SetJobState(JobState.Disconnected, null);
                        }
                    }
                    else
                    {
                        if (prevState == JobState.Disconnected)
                        {
                            this.disconnectedChildJobsCount--;
                        }
                        if ((newState == JobState.Running) && (base.JobStateInfo.State == JobState.Disconnected))
                        {
                            base.SetJobState(JobState.Running, null);
                        }
                    }
                }
            }
        }

        private void CommonInit(int throttleLimit, List<IThrottleOperation> helpers)
        {
            base.CloseAllStreams();
            base.SetJobState(JobState.Running);
            this.throttleManager.ThrottleLimit = throttleLimit;
            this.throttleManager.SubmitOperations(helpers);
            this.throttleManager.EndSubmitOperations();
        }

        internal void ConnectJob(Guid runspaceInstanceId)
        {
            List<IThrottleOperation> connectJobOperations = new List<IThrottleOperation>();
            PSRemotingChildJob job = this.FindDisconnectedChildJob(runspaceInstanceId);
            if (job != null)
            {
                connectJobOperations.Add(new ConnectJobOperation(job));
            }
            if (connectJobOperations.Count != 0)
            {
                this.SubmitAndWaitForConnect(connectJobOperations);
            }
        }

        internal void ConnectJobs()
        {
            List<IThrottleOperation> connectJobOperations = new List<IThrottleOperation>();
            foreach (PSRemotingChildJob job in base.ChildJobs)
            {
                if (job.JobStateInfo.State == JobState.Disconnected)
                {
                    connectJobOperations.Add(new ConnectJobOperation(job));
                }
            }
            if (connectJobOperations.Count != 0)
            {
                this.SubmitAndWaitForConnect(connectJobOperations);
            }
        }

        private string ConstructLocation()
        {
            StringBuilder builder = new StringBuilder();
            if (base.ChildJobs.Count > 0)
            {
                foreach (PSRemotingChildJob job in base.ChildJobs)
                {
                    builder.Append(job.Location);
                    builder.Append(",");
                }
                builder.Remove(builder.Length - 1, 1);
            }
            return builder.ToString();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !this.isDisposed)
            {
                lock (this._syncObject)
                {
                    if (this.isDisposed)
                    {
                        return;
                    }
                    this.isDisposed = true;
                }
                try
                {
                    if (!base.IsFinishedState(base.JobStateInfo.State))
                    {
                        this.StopJob();
                    }
                    foreach (Job job in base.ChildJobs)
                    {
                        job.Dispose();
                    }
                    this.throttleManager.Dispose();
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }
        }

        private PSRemotingChildJob FindDisconnectedChildJob(Guid runspaceInstanceId)
        {
            foreach (PSRemotingChildJob job2 in base.ChildJobs)
            {
                if (job2.Runspace.InstanceId.Equals(runspaceInstanceId) && (job2.JobStateInfo.State == JobState.Disconnected))
                {
                    return job2;
                }
            }
            return null;
        }

        internal PowerShell GetAssociatedPowerShellObject(Guid runspaceInstanceId)
        {
            PowerShell powerShell = null;
            PSRemotingChildJob job = this.FindDisconnectedChildJob(runspaceInstanceId);
            if (job != null)
            {
                powerShell = job.GetPowerShell();
            }
            return powerShell;
        }

        internal List<Job> GetJobsForComputer(string computerName)
        {
            List<Job> list = new List<Job>();
            foreach (Job job in base.ChildJobs)
            {
                PSRemotingChildJob item = job as PSRemotingChildJob;
                if ((job != null) && string.Equals(item.Runspace.ConnectionInfo.ComputerName, computerName, StringComparison.OrdinalIgnoreCase))
                {
                    list.Add(item);
                }
            }
            return list;
        }

        internal List<Job> GetJobsForOperation(IThrottleOperation operation)
        {
            List<Job> list = new List<Job>();
            ExecutionCmdletHelper helper = operation as ExecutionCmdletHelper;
            foreach (Job job in base.ChildJobs)
            {
                PSRemotingChildJob item = job as PSRemotingChildJob;
                if ((job != null) && item.Helper.Equals(helper))
                {
                    list.Add(item);
                }
            }
            return list;
        }

        internal List<Job> GetJobsForRunspace(PSSession runspace)
        {
            List<Job> list = new List<Job>();
            foreach (Job job in base.ChildJobs)
            {
                PSRemotingChildJob item = job as PSRemotingChildJob;
                if ((job != null) && item.Runspace.InstanceId.Equals(runspace.InstanceId))
                {
                    list.Add(item);
                }
            }
            return list;
        }

        internal override IEnumerable<RemoteRunspace> GetRunspaces()
        {
            List<RemoteRunspace> list = new List<RemoteRunspace>();
            foreach (PSRemotingChildJob job in base.ChildJobs)
            {
                list.Add(job.Runspace as RemoteRunspace);
            }
            return list;
        }

        private void HandleChildJobStateChanged(object sender, JobStateEventArgs e)
        {
            this.CheckDisconnectedAndUpdateState(e.JobStateInfo.State, e.PreviousJobStateInfo.State);
            if (e.JobStateInfo.State == JobState.Blocked)
            {
                lock (this._syncObject)
                {
                    this.blockedChildJobsCount++;
                }
                base.SetJobState(JobState.Blocked, null);
            }
            else if (base.IsFinishedState(e.JobStateInfo.State))
            {
                if (e.JobStateInfo.State == JobState.Failed)
                {
                    this.atleastOneChildJobFailed = true;
                }
                bool flag2 = false;
                lock (this._syncObject)
                {
                    this.finishedChildJobsCount++;
                    if ((this.finishedChildJobsCount + this.disconnectedChildJobsCount) == base.ChildJobs.Count)
                    {
                        flag2 = true;
                    }
                }
                if (flag2)
                {
                    if (this.disconnectedChildJobsCount > 0)
                    {
                        base.SetJobState(JobState.Disconnected);
                    }
                    else if (this.atleastOneChildJobFailed)
                    {
                        base.SetJobState(JobState.Failed);
                    }
                    else if (this._stopIsCalled)
                    {
                        base.SetJobState(JobState.Stopped);
                    }
                    else
                    {
                        base.SetJobState(JobState.Completed);
                    }
                }
            }
        }

        private void HandleJobUnblocked(object sender, EventArgs eventArgs)
        {
            bool flag = false;
            lock (this._syncObject)
            {
                this.blockedChildJobsCount--;
                if (this.blockedChildJobsCount == 0)
                {
                    flag = true;
                }
            }
            if (flag)
            {
                base.SetJobState(JobState.Running, null);
            }
        }

        internal void InternalStopJob()
        {
            if ((!this.isDisposed && !this._stopIsCalled) && !base.IsFinishedState(base.JobStateInfo.State))
            {
                lock (this._syncObject)
                {
                    if ((this.isDisposed || this._stopIsCalled) || base.IsFinishedState(base.JobStateInfo.State))
                    {
                        return;
                    }
                    this._stopIsCalled = true;
                }
                this.throttleManager.StopAllOperations();
                base.Finished.WaitOne();
            }
        }

        private void SetStatusMessage()
        {
            this.statusMessage = "test";
        }

        public override void StopJob()
        {
            if (base.JobStateInfo.State == JobState.Disconnected)
            {
                bool flag;
                try
                {
                    this.ConnectJobs();
                    flag = true;
                }
                catch (InvalidRunspaceStateException)
                {
                    flag = false;
                }
                catch (PSRemotingTransportException)
                {
                    flag = false;
                }
                catch (PSInvalidOperationException)
                {
                    flag = false;
                }
                if (!flag && base.Error.IsOpen)
                {
                    Exception exception = new RuntimeException(StringUtil.Format(RemotingErrorIdStrings.StopJobNotConnected, base.Name));
                    ErrorRecord errorRecord = new ErrorRecord(exception, "StopJobCannotConnectToServer", ErrorCategory.InvalidOperation, this);
                    this.WriteError(errorRecord);
                    return;
                }
            }
            this.InternalStopJob();
        }

        private void SubmitAndWaitForConnect(List<IThrottleOperation> connectJobOperations)
        {
            using (ThrottleManager manager = new ThrottleManager())
            {
                EventHandler<EventArgs> handler2 = null;
                using (ManualResetEvent connectResult = new ManualResetEvent(false))
                {
                    if (handler2 == null)
                    {
                        handler2 = (sender, eventArgs) => connectResult.Set();
                    }
                    EventHandler<EventArgs> handler = handler2;
                    manager.ThrottleComplete += handler;
                    try
                    {
                        manager.ThrottleLimit = 0;
                        manager.SubmitOperations(connectJobOperations);
                        manager.EndSubmitOperations();
                        connectResult.WaitOne();
                    }
                    finally
                    {
                        manager.ThrottleComplete -= handler;
                    }
                }
            }
        }

        internal override bool CanDisconnect
        {
            get
            {
                if (base.ChildJobs.Count <= 0)
                {
                    return false;
                }
                return base.ChildJobs[0].CanDisconnect;
            }
        }

        public override bool HasMoreData
        {
            get
            {
                if (this.moreData && base.IsFinishedState(base.JobStateInfo.State))
                {
                    bool flag = false;
                    for (int i = 0; i < base.ChildJobs.Count; i++)
                    {
                        if (base.ChildJobs[i].HasMoreData)
                        {
                            flag = true;
                            break;
                        }
                    }
                    this.moreData = flag;
                }
                return this.moreData;
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
                return this.ConstructLocation();
            }
        }

        public override string StatusMessage
        {
            get
            {
                return this.statusMessage;
            }
        }

        private class ConnectJobOperation : IThrottleOperation
        {
            private PSRemotingChildJob psRemoteChildJob;

            internal override event EventHandler<OperationStateEventArgs> OperationComplete;

            internal ConnectJobOperation(PSRemotingChildJob job)
            {
                this.psRemoteChildJob = job;
                this.psRemoteChildJob.StateChanged += new EventHandler<JobStateEventArgs>(this.ChildJobStateChangedHandler);
            }

            private void ChildJobStateChangedHandler(object sender, JobStateEventArgs eArgs)
            {
                if (eArgs.JobStateInfo.State != JobState.Disconnected)
                {
                    this.RemoveEventCallback();
                    this.SendStartComplete();
                }
            }

            private void RemoveEventCallback()
            {
                this.psRemoteChildJob.StateChanged -= new EventHandler<JobStateEventArgs>(this.ChildJobStateChangedHandler);
            }

            private void SendStartComplete()
            {
                OperationStateEventArgs eventArgs = new OperationStateEventArgs {
                    OperationState = OperationState.StartComplete
                };
                this.OperationComplete.SafeInvoke<OperationStateEventArgs>(this, eventArgs);
            }

            internal override void StartOperation()
            {
                bool flag = true;
                try
                {
                    this.psRemoteChildJob.ConnectAsync();
                }
                catch (InvalidJobStateException exception)
                {
                    flag = false;
                    Exception exception2 = new RuntimeException(StringUtil.Format(RemotingErrorIdStrings.JobConnectFailed, this.psRemoteChildJob.Name), exception);
                    ErrorRecord errorRecord = new ErrorRecord(exception2, "PSJobConnectFailed", ErrorCategory.InvalidOperation, this.psRemoteChildJob);
                    this.psRemoteChildJob.WriteError(errorRecord);
                }
                if (!flag)
                {
                    this.RemoveEventCallback();
                    this.SendStartComplete();
                }
            }

            internal override void StopOperation()
            {
                this.RemoveEventCallback();
                OperationStateEventArgs eventArgs = new OperationStateEventArgs {
                    OperationState = OperationState.StopComplete
                };
                this.OperationComplete.SafeInvoke<OperationStateEventArgs>(this, eventArgs);
            }
        }
    }
}

