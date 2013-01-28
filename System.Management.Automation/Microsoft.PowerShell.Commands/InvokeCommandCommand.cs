namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Runspaces.Internal;
    using System.Runtime.CompilerServices;
    using System.Threading;

    [Cmdlet("Invoke", "Command", DefaultParameterSetName="InProcess", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135225", RemotingCapability=RemotingCapability.OwnedByCommand)]
    public class InvokeCommandCommand : PSExecutionCmdlet, IDisposable
    {
        private bool asjob;
        private bool clearInvokeCommandOnRunspace;
        private ManualResetEvent disconnectComplete;
        private bool hideComputerName;
        private const string InProcParameterSet = "InProcess";
        private PSDataCollection<object> input = new PSDataCollection<object>();
        private bool inputStreamClosed;
        private List<PipelineWriter> inputWriters = new List<PipelineWriter>();
        private Guid instanceId = Guid.NewGuid();
        private PSInvokeExpressionSyncJob job;
        private object jobSyncObject = new object();
        private string name = string.Empty;
        private bool needToCollect;
        private bool needToStartSteppablePipelineOnServer;
        private bool nojob;
        private ManualResetEvent operationsComplete = new ManualResetEvent(true);
        private bool pipelineinvoked;
        private bool propagateErrors;
        private static RobustConnectionProgress RCProgress = new RobustConnectionProgress();
        internal static readonly string RemoteJobType = "RemoteJob";
        private SteppablePipeline steppablePipeline;
        private ThrottleManager throttleManager = new ThrottleManager();

        private void AddConnectionRetryHandler(PSInvokeExpressionSyncJob job)
        {
            if (job != null)
            {
                foreach (PowerShell shell in job.GetPowerShells())
                {
                    if (shell.RemotePowerShell != null)
                    {
                        shell.RemotePowerShell.RCConnectionNotification += new EventHandler<PSConnectionRetryStatusEventArgs>(this.RCConnectionNotificationHandler);
                    }
                }
            }
        }

        protected override void BeginProcessing()
        {
            if (base.InvokeAndDisconnect && this.asjob)
            {
                throw new InvalidOperationException(RemotingErrorIdStrings.AsJobAndDisconnectedError);
            }
            if ((base.InvokeAndDisconnect && ((this.ComputerName == null) || (this.ComputerName.Length == 0))) && ((this.ConnectionUri == null) || (this.ConnectionUri.Length == 0)))
            {
                throw new InvalidOperationException(RemotingErrorIdStrings.InvokeDisconnectedWithoutComputerName);
            }
            if (base.MyInvocation.BoundParameters.ContainsKey("SessionName") && !base.InvokeAndDisconnect)
            {
                throw new InvalidOperationException(RemotingErrorIdStrings.SessionNameWithoutInvokeDisconnected);
            }
            if (base.ParameterSetName.Equals("InProcess"))
            {
                if (this.FilePath != null)
                {
                    this.ScriptBlock = base.GetScriptBlockFromFile(this.FilePath, false);
                }
                if (base.MyInvocation.ExpectingInput && !this.ScriptBlock.IsUsingDollarInput())
                {
                    try
                    {
                        this.steppablePipeline = this.ScriptBlock.GetSteppablePipeline();
                        this.steppablePipeline.Begin(this);
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }
            }
            else
            {
                base.BeginProcessing();
                foreach (IThrottleOperation operation in base.Operations)
                {
                    this.inputWriters.Add(((ExecutionCmdletHelper) operation).Pipeline.Input);
                }
                if (base.ParameterSetName.Equals("Session"))
                {
                    long instanceId = ((LocalRunspace) base.Context.CurrentRunspace).GetCurrentlyRunningPipeline().InstanceId;
                    foreach (PSSession session in this.Session)
                    {
                        RemoteRunspace runspace = (RemoteRunspace) session.Runspace;
                        if (runspace.IsAnotherInvokeCommandExecuting(this, instanceId))
                        {
                            if (((base.MyInvocation != null) && (base.MyInvocation.PipelinePosition == 1)) && !base.MyInvocation.ExpectingInput)
                            {
                                PSPrimitiveDictionary dictionary = session.ApplicationPrivateData["PSVersionTable"] as PSPrimitiveDictionary;
                                if (dictionary != null)
                                {
                                    Version version = dictionary["PSRemotingProtocolVersion"] as Version;
                                    if ((version != null) && (version >= RemotingConstants.ProtocolVersionWin8RTM))
                                    {
                                        this.needToCollect = false;
                                        this.needToStartSteppablePipelineOnServer = true;
                                        break;
                                    }
                                }
                            }
                            this.needToCollect = true;
                            this.needToStartSteppablePipelineOnServer = false;
                            break;
                        }
                    }
                }
                if (this.needToStartSteppablePipelineOnServer)
                {
                    foreach (IThrottleOperation operation2 in base.Operations)
                    {
                        ExecutionCmdletHelperRunspace runspace2 = operation2 as ExecutionCmdletHelperRunspace;
                        if (runspace2 == null)
                        {
                            break;
                        }
                        runspace2.ShouldUseSteppablePipelineOnServer = true;
                    }
                }
                else
                {
                    this.clearInvokeCommandOnRunspace = true;
                }
                this.DetermineThrowStatementBehavior();
            }
        }

        private void ClearInvokeCommandOnRunspaces()
        {
            if (base.ParameterSetName.Equals("Session"))
            {
                foreach (PSSession session in this.Session)
                {
                    ((RemoteRunspace) session.Runspace).ClearInvokeCommand();
                }
            }
        }

        private void CreateAndRunSyncJob()
        {
            lock (this.jobSyncObject)
            {
                if (!this.nojob)
                {
                    this.throttleManager.ThrottleLimit = this.ThrottleLimit;
                    this.throttleManager.ThrottleComplete += new EventHandler<EventArgs>(this.HandleThrottleComplete);
                    this.operationsComplete.Reset();
                    this.disconnectComplete = new ManualResetEvent(false);
                    this.job = new PSInvokeExpressionSyncJob(base.Operations, this.throttleManager);
                    this.job.HideComputerName = this.hideComputerName;
                    this.job.StateChanged += new EventHandler<JobStateEventArgs>(this.HandleJobStateChanged);
                    this.AddConnectionRetryHandler(this.job);
                    this.job.StartOperations(base.Operations);
                }
            }
        }

        private void DetermineThrowStatementBehavior()
        {
            if (!base.ParameterSetName.Equals("InProcess") && !this.asjob)
            {
                if (base.ParameterSetName.Equals("ComputerName") || base.ParameterSetName.Equals("FilePathComputerName"))
                {
                    if (this.ComputerName.Length == 1)
                    {
                        this.propagateErrors = true;
                    }
                }
                else if (base.ParameterSetName.Equals("Session") || base.ParameterSetName.Equals("FilePathRunspace"))
                {
                    if (this.Session.Length == 1)
                    {
                        this.propagateErrors = true;
                    }
                }
                else if ((base.ParameterSetName.Equals("Uri") || base.ParameterSetName.Equals("FilePathUri")) && (this.ConnectionUri.Length == 1))
                {
                    this.propagateErrors = true;
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.StopProcessing();
                this.operationsComplete.WaitOne();
                this.operationsComplete.Close();
                if (!this.asjob)
                {
                    if (this.job != null)
                    {
                        this.job.Dispose();
                    }
                    this.throttleManager.ThrottleComplete -= new EventHandler<EventArgs>(this.HandleThrottleComplete);
                    this.throttleManager.Dispose();
                    this.throttleManager = null;
                }
                if (this.clearInvokeCommandOnRunspace)
                {
                    this.ClearInvokeCommandOnRunspaces();
                }
                this.input.Dispose();
                lock (this.jobSyncObject)
                {
                    if (this.disconnectComplete != null)
                    {
                        this.disconnectComplete.Close();
                        this.disconnectComplete = null;
                    }
                }
            }
        }

        protected override void EndProcessing()
        {
            if (!this.needToCollect)
            {
                base.CloseAllInputStreams();
            }
            if (!this.asjob)
            {
                if (base.ParameterSetName.Equals("InProcess"))
                {
                    if (this.steppablePipeline != null)
                    {
                        this.steppablePipeline.End();
                    }
                    else
                    {
                        this.ScriptBlock.InvokeUsingCmdlet(this, this.NoNewScope == 0, System.Management.Automation.ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe, AutomationNull.Value, this.input, AutomationNull.Value, this.ArgumentList);
                    }
                }
                else if (this.job != null)
                {
                    if (base.InvokeAndDisconnect)
                    {
                        this.WaitForDisconnectAndDisposeJob();
                    }
                    else
                    {
                        this.WriteJobResults(false);
                        if (!this.asjob)
                        {
                            this.job.Dispose();
                        }
                    }
                }
                else if (this.needToCollect && base.ParameterSetName.Equals("Session"))
                {
                    this.CreateAndRunSyncJob();
                    foreach (object obj2 in this.input)
                    {
                        this.WriteInput(obj2);
                    }
                    base.CloseAllInputStreams();
                    if (base.InvokeAndDisconnect)
                    {
                        this.WaitForDisconnectAndDisposeJob();
                    }
                    else
                    {
                        this.WriteJobResults(false);
                        if (!this.asjob)
                        {
                            this.job.Dispose();
                        }
                    }
                }
            }
        }

        private List<PSSession> GetDisconnectedSessions(PSInvokeExpressionSyncJob job)
        {
            List<PSSession> list = new List<PSSession>();
            foreach (PowerShell shell in job.GetPowerShells())
            {
                string cmdStr = ((shell.Commands != null) && (shell.Commands.Commands.Count > 0)) ? shell.Commands.Commands[0].CommandText : string.Empty;
                ConnectCommandInfo info = new ConnectCommandInfo(shell.InstanceId, cmdStr);
                RunspacePool runspacePool = null;
                if (shell.RunspacePool != null)
                {
                    runspacePool = shell.RunspacePool;
                }
                else
                {
                    object runspaceConnection = shell.GetRunspaceConnection();
                    RunspacePool pool2 = runspaceConnection as RunspacePool;
                    if (pool2 != null)
                    {
                        runspacePool = pool2;
                    }
                    else
                    {
                        RemoteRunspace runspace = runspaceConnection as RemoteRunspace;
                        if (runspace != null)
                        {
                            runspacePool = runspace.RunspacePool;
                        }
                    }
                }
                if (runspacePool != null)
                {
                    if (runspacePool.RunspacePoolStateInfo.State != RunspacePoolState.Disconnected)
                    {
                        if (!base.InvokeAndDisconnect || (runspacePool.RunspacePoolStateInfo.State != RunspacePoolState.Opened))
                        {
                            continue;
                        }
                        runspacePool.Disconnect();
                    }
                    string name = runspacePool.RemoteRunspacePoolInternal.Name;
                    if (string.IsNullOrEmpty(name))
                    {
                        int num;
                        name = PSSession.GenerateRunspaceName(out num);
                    }
                    RunspacePool pool3 = new RunspacePool(true, runspacePool.RemoteRunspacePoolInternal.InstanceId, name, new ConnectCommandInfo[] { info }, runspacePool.RemoteRunspacePoolInternal.ConnectionInfo, base.Host, base.Context.TypeTable);
                    RemoteRunspace remoteRunspace = new RemoteRunspace(pool3);
                    list.Add(new PSSession(remoteRunspace));
                }
            }
            return list;
        }

        private PSSession GetPSSession(Guid runspaceId)
        {
            foreach (PSSession session in this.Session)
            {
                if (session.Runspace.InstanceId == runspaceId)
                {
                    return session;
                }
            }
            return null;
        }

        private void HandleJobStateChanged(object sender, JobStateEventArgs e)
        {
            switch (e.JobStateInfo.State)
            {
                case JobState.Disconnected:
                case JobState.Completed:
                case JobState.Stopped:
                case JobState.Failed:
                    this.job.StateChanged -= new EventHandler<JobStateEventArgs>(this.HandleJobStateChanged);
                    this.RemoveConnectionRetryHandler(sender as PSInvokeExpressionSyncJob);
                    lock (this.jobSyncObject)
                    {
                        if (this.disconnectComplete != null)
                        {
                            this.disconnectComplete.Set();
                        }
                    }
                    break;
            }
        }

        private void HandlePipelinesStopped()
        {
            bool flag = false;
            foreach (PowerShell shell in this.job.GetPowerShells())
            {
                if (((shell.RemotePowerShell != null) && (shell.RemotePowerShell.ConnectionRetryStatus != PSConnectionRetryStatus.None)) && ((shell.RemotePowerShell.ConnectionRetryStatus != PSConnectionRetryStatus.ConnectionRetrySucceeded) && (shell.RemotePowerShell.ConnectionRetryStatus != PSConnectionRetryStatus.AutoDisconnectSucceeded)))
                {
                    flag = true;
                    break;
                }
            }
            if (flag && (base.Host != null))
            {
                base.Host.UI.WriteWarningLine(RemotingErrorIdStrings.StopCommandOnRetry);
            }
        }

        private void HandleThrottleComplete(object sender, EventArgs eventArgs)
        {
            this.operationsComplete.Set();
            this.throttleManager.ThrottleComplete -= new EventHandler<EventArgs>(this.HandleThrottleComplete);
        }

        protected override void ProcessRecord()
        {
            if (!this.pipelineinvoked && !this.needToCollect)
            {
                this.pipelineinvoked = true;
                if (this.InputObject == AutomationNull.Value)
                {
                    base.CloseAllInputStreams();
                    this.inputStreamClosed = true;
                }
                if (!base.ParameterSetName.Equals("InProcess"))
                {
                    if (!this.asjob)
                    {
                        this.CreateAndRunSyncJob();
                    }
                    else
                    {
                        string parameterSetName = base.ParameterSetName;
                        if (parameterSetName != null)
                        {
                            if (!(parameterSetName == "ComputerName") && !(parameterSetName == "FilePathComputerName"))
                            {
                                if ((parameterSetName == "Session") || (parameterSetName == "FilePathRunspace"))
                                {
                                    PSRemotingJob item = new PSRemotingJob(this.Session, base.Operations, this.ScriptBlock.ToString(), this.ThrottleLimit, this.name) {
                                        PSJobTypeName = RemoteJobType,
                                        HideComputerName = this.hideComputerName
                                    };
                                    base.JobRepository.Add(item);
                                    base.WriteObject(item);
                                }
                                else if (((parameterSetName == "Uri") || (parameterSetName == "FilePathUri")) && (base.Operations.Count > 0))
                                {
                                    string[] computerNames = new string[this.ConnectionUri.Length];
                                    for (int i = 0; i < computerNames.Length; i++)
                                    {
                                        computerNames[i] = this.ConnectionUri[i].ToString();
                                    }
                                    PSRemotingJob job3 = new PSRemotingJob(computerNames, base.Operations, this.ScriptBlock.ToString(), this.ThrottleLimit, this.name) {
                                        PSJobTypeName = RemoteJobType,
                                        HideComputerName = this.hideComputerName
                                    };
                                    base.JobRepository.Add(job3);
                                    base.WriteObject(job3);
                                }
                            }
                            else if ((base.ResolvedComputerNames.Length != 0) && (base.Operations.Count > 0))
                            {
                                PSRemotingJob job = new PSRemotingJob(base.ResolvedComputerNames, base.Operations, this.ScriptBlock.ToString(), this.ThrottleLimit, this.name) {
                                    PSJobTypeName = RemoteJobType,
                                    HideComputerName = this.hideComputerName
                                };
                                base.JobRepository.Add(job);
                                base.WriteObject(job);
                            }
                        }
                    }
                }
            }
            if ((this.InputObject != AutomationNull.Value) && !this.inputStreamClosed)
            {
                if ((base.ParameterSetName.Equals("InProcess") && (this.steppablePipeline == null)) || this.needToCollect)
                {
                    this.input.Add(this.InputObject);
                }
                else if (base.ParameterSetName.Equals("InProcess") && (this.steppablePipeline != null))
                {
                    this.steppablePipeline.Process(this.InputObject);
                }
                else
                {
                    this.WriteInput(this.InputObject);
                    if (!this.asjob)
                    {
                        this.WriteJobResults(true);
                    }
                }
            }
        }

        private void RCConnectionNotificationHandler(object sender, PSConnectionRetryStatusEventArgs e)
        {
            switch (e.Notification)
            {
                case PSConnectionRetryStatus.NetworkFailureDetected:
                    this.StartProgressBar((long) sender.GetHashCode(), e.ComputerName, e.MaxRetryConnectionTime / 0x3e8);
                    return;

                case PSConnectionRetryStatus.ConnectionRetryAttempt:
                case PSConnectionRetryStatus.AutoDisconnectSucceeded:
                    break;

                case PSConnectionRetryStatus.ConnectionRetrySucceeded:
                case PSConnectionRetryStatus.AutoDisconnectStarting:
                case PSConnectionRetryStatus.InternalErrorAbort:
                    this.StopProgressBar((long) sender.GetHashCode());
                    break;

                default:
                    return;
            }
        }

        private void RemoveConnectionRetryHandler(PSInvokeExpressionSyncJob job)
        {
            this.StopProgressBar(0L);
            if (job != null)
            {
                foreach (PowerShell shell in job.GetPowerShells())
                {
                    if (shell.RemotePowerShell != null)
                    {
                        shell.RemotePowerShell.RCConnectionNotification -= new EventHandler<PSConnectionRetryStatusEventArgs>(this.RCConnectionNotificationHandler);
                    }
                }
            }
        }

        private void StartProgressBar(long sourceId, string computerName, int totalSeconds)
        {
            RCProgress.StartProgress(sourceId, computerName, totalSeconds, base.Host);
        }

        protected override void StopProcessing()
        {
            if (!base.ParameterSetName.Equals("InProcess") && !this.asjob)
            {
                bool flag = false;
                lock (this.jobSyncObject)
                {
                    if (this.job != null)
                    {
                        flag = true;
                    }
                    else
                    {
                        this.nojob = true;
                    }
                }
                if (flag)
                {
                    this.job.StopJob();
                }
                this.needToCollect = false;
            }
        }

        private void StopProgressBar(long sourceId)
        {
            RCProgress.StopProgress(sourceId);
        }

        private void WaitForDisconnectAndDisposeJob()
        {
            if (this.disconnectComplete != null)
            {
                this.disconnectComplete.WaitOne();
                if (this.job.JobStateInfo.State != JobState.Disconnected)
                {
                    this.WriteStreamObjectsFromCollection(this.job.ReadAll());
                }
                foreach (PSSession session in this.GetDisconnectedSessions(this.job))
                {
                    base.RunspaceRepository.AddOrReplace(session);
                    base.WriteObject(session);
                }
                this.job.Dispose();
            }
        }

        private void WriteInput(object inputValue)
        {
            if (this.inputWriters.Count == 0)
            {
                if (!this.asjob)
                {
                    this.WriteJobResults(false);
                }
                this.EndProcessing();
                throw new StopUpstreamCommandsException(this);
            }
            List<PipelineWriter> list = new List<PipelineWriter>();
            foreach (PipelineWriter writer in this.inputWriters)
            {
                try
                {
                    writer.Write(inputValue);
                }
                catch (PipelineClosedException)
                {
                    list.Add(writer);
                }
            }
            foreach (PipelineWriter writer2 in list)
            {
                this.inputWriters.Remove(writer2);
            }
        }

        private void WriteJobResults(bool nonblocking)
        {
            if (this.job != null)
            {
                PipelineStoppedException exception = null;
                this.job.PropagateThrows = this.propagateErrors;
                do
                {
                    if (!nonblocking)
                    {
                        if (this.disconnectComplete != null)
                        {
                            WaitHandle.WaitAny(new WaitHandle[] { this.disconnectComplete, this.job.Results.WaitHandle });
                        }
                        else
                        {
                            this.job.Results.WaitHandle.WaitOne();
                        }
                    }
                    try
                    {
                        this.WriteStreamObjectsFromCollection(this.job.ReadAll());
                    }
                    catch (PipelineStoppedException exception2)
                    {
                        exception = exception2;
                    }
                }
                while (!nonblocking && !this.job.IsTerminalState());
                try
                {
                    this.WriteStreamObjectsFromCollection(this.job.ReadAll());
                }
                catch (PipelineStoppedException exception3)
                {
                    exception = exception3;
                }
                if (exception != null)
                {
                    this.HandlePipelinesStopped();
                    throw exception;
                }
                if (this.job.JobStateInfo.State == JobState.Disconnected)
                {
                    if ((base.ParameterSetName == "Session") || (base.ParameterSetName == "FilePathRunspace"))
                    {
                        PSRemotingJob item = this.job.CreateDisconnectedRemotingJob();
                        if (item != null)
                        {
                            item.PSJobTypeName = RemoteJobType;
                            base.JobRepository.Add(item);
                            this.asjob = true;
                            foreach (Job job2 in item.ChildJobs)
                            {
                                PSRemotingChildJob job3 = job2 as PSRemotingChildJob;
                                if (job3 != null)
                                {
                                    PSSession pSSession = this.GetPSSession(job3.Runspace.InstanceId);
                                    if (pSSession != null)
                                    {
                                        this.WriteNetworkFailedError(pSSession);
                                        base.WriteWarning(StringUtil.Format(RemotingErrorIdStrings.RCDisconnectSession, new object[] { pSSession.Name, pSSession.InstanceId, pSSession.ComputerName }));
                                    }
                                }
                            }
                            base.WriteWarning(StringUtil.Format(RemotingErrorIdStrings.RCDisconnectedJob, item.Name));
                        }
                    }
                    else if ((base.ParameterSetName == "ComputerName") || (base.ParameterSetName == "FilePathComputerName"))
                    {
                        foreach (PSSession session2 in this.GetDisconnectedSessions(this.job))
                        {
                            base.RunspaceRepository.AddOrReplace(session2);
                            this.WriteNetworkFailedError(session2);
                            base.WriteWarning(StringUtil.Format(RemotingErrorIdStrings.RCDisconnectSession, new object[] { session2.Name, session2.InstanceId, session2.ComputerName }));
                            base.WriteWarning(StringUtil.Format(RemotingErrorIdStrings.RCDisconnectSessionCreated, session2.Name, session2.InstanceId));
                        }
                    }
                    this.HandleThrottleComplete(null, null);
                }
            }
        }

        private void WriteNetworkFailedError(PSSession session)
        {
            RuntimeException exception = new RuntimeException(StringUtil.Format(RemotingErrorIdStrings.RCAutoDisconnectingError, session.ComputerName));
            base.WriteError(new ErrorRecord(exception, "PowerShellNetworkFailedStartDisconnect", ErrorCategory.OperationTimeout, session));
        }

        private void WriteStreamObjectsFromCollection(IEnumerable<PSStreamObject> results)
        {
            foreach (PSStreamObject obj2 in results)
            {
                if (obj2 != null)
                {
                    obj2.WriteStreamObject(this, false);
                }
            }
        }

        [Parameter(ParameterSetName="Uri"), Parameter(ParameterSetName="FilePathUri")]
        public override SwitchParameter AllowRedirection
        {
            get
            {
                return base.AllowRedirection;
            }
            set
            {
                base.AllowRedirection = value;
            }
        }

        [Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="FilePathComputerName"), Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ComputerName")]
        public override string ApplicationName
        {
            get
            {
                return base.ApplicationName;
            }
            set
            {
                base.ApplicationName = value;
            }
        }

        [Parameter(ParameterSetName="FilePathUri"), Parameter(ParameterSetName="FilePathRunspace"), Parameter(ParameterSetName="ComputerName"), Parameter(ParameterSetName="Session"), Parameter(ParameterSetName="Uri"), Parameter(ParameterSetName="FilePathComputerName")]
        public SwitchParameter AsJob
        {
            get
            {
                return this.asjob;
            }
            set
            {
                this.asjob = (bool) value;
            }
        }

        [Parameter(ParameterSetName="ComputerName"), Parameter(ParameterSetName="FilePathComputerName"), Parameter(ParameterSetName="Uri"), Parameter(ParameterSetName="FilePathUri")]
        public override AuthenticationMechanism Authentication
        {
            get
            {
                return base.Authentication;
            }
            set
            {
                base.Authentication = value;
            }
        }

        [ValidateNotNullOrEmpty, Parameter(Position=0, ParameterSetName="ComputerName"), Parameter(Position=0, ParameterSetName="FilePathComputerName"), Alias(new string[] { "Cn" })]
        public override string[] ComputerName
        {
            get
            {
                return base.ComputerName;
            }
            set
            {
                base.ComputerName = value;
            }
        }

        [Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ComputerName"), Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="FilePathComputerName"), Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Uri"), Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="FilePathUri")]
        public override string ConfigurationName
        {
            get
            {
                return base.ConfigurationName;
            }
            set
            {
                base.ConfigurationName = value;
            }
        }

        [Parameter(Position=0, ParameterSetName="Uri"), Alias(new string[] { "URI", "CU" }), Parameter(Position=0, ParameterSetName="FilePathUri"), ValidateNotNullOrEmpty]
        public override Uri[] ConnectionUri
        {
            get
            {
                return base.ConnectionUri;
            }
            set
            {
                base.ConnectionUri = value;
            }
        }

        [Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ComputerName"), Credential, Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="FilePathUri"), Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="FilePathComputerName"), Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Uri")]
        public override PSCredential Credential
        {
            get
            {
                return base.Credential;
            }
            set
            {
                base.Credential = value;
            }
        }

        [Parameter(ParameterSetName="Uri"), Parameter(ParameterSetName="ComputerName"), Parameter(ParameterSetName="FilePathComputerName"), Parameter(ParameterSetName="FilePathUri")]
        public override SwitchParameter EnableNetworkAccess
        {
            get
            {
                return base.EnableNetworkAccess;
            }
            set
            {
                base.EnableNetworkAccess = value;
            }
        }

        [ValidateNotNull, Parameter(Position=1, Mandatory=true, ParameterSetName="FilePathRunspace"), Parameter(Position=1, Mandatory=true, ParameterSetName="FilePathUri"), Parameter(Position=1, Mandatory=true, ParameterSetName="FilePathComputerName"), Alias(new string[] { "PSPath" })]
        public override string FilePath
        {
            get
            {
                return base.FilePath;
            }
            set
            {
                base.FilePath = value;
            }
        }

        [Parameter(ParameterSetName="FilePathUri"), Parameter(ParameterSetName="FilePathRunspace"), Alias(new string[] { "HCN" }), Parameter(ParameterSetName="FilePathComputerName"), Parameter(ParameterSetName="ComputerName"), Parameter(ParameterSetName="Session"), Parameter(ParameterSetName="Uri")]
        public SwitchParameter HideComputerName
        {
            get
            {
                return this.hideComputerName;
            }
            set
            {
                this.hideComputerName = (bool) value;
            }
        }

        [Parameter(ParameterSetName="FilePathComputerName"), Parameter(ParameterSetName="FilePathUri"), Alias(new string[] { "Disconnected" }), Parameter(ParameterSetName="Uri"), Parameter(ParameterSetName="ComputerName")]
        public SwitchParameter InDisconnectedSession
        {
            get
            {
                return base.InvokeAndDisconnect;
            }
            set
            {
                base.InvokeAndDisconnect = (bool) value;
            }
        }

        [Parameter(ParameterSetName="FilePathUri"), Parameter(ParameterSetName="FilePathComputerName"), Parameter(ParameterSetName="FilePathRunspace"), Parameter(ParameterSetName="ComputerName"), Parameter(ParameterSetName="Session"), Parameter(ParameterSetName="Uri")]
        public string JobName
        {
            get
            {
                return this.name;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.name = value;
                    this.asjob = true;
                }
            }
        }

        [Parameter(ParameterSetName="InProcess")]
        public SwitchParameter NoNewScope { get; set; }

        [Parameter(ParameterSetName="ComputerName"), Parameter(ParameterSetName="FilePathComputerName"), ValidateRange(1, 0xffff)]
        public override int Port
        {
            get
            {
                return base.Port;
            }
            set
            {
                base.Port = value;
            }
        }

        [Parameter(Position=0, Mandatory=true, ParameterSetName="InProcess"), Parameter(Position=1, Mandatory=true, ParameterSetName="Session"), Parameter(Position=1, Mandatory=true, ParameterSetName="Uri"), Parameter(Position=1, Mandatory=true, ParameterSetName="ComputerName"), ValidateNotNull, Alias(new string[] { "Command" })]
        public override System.Management.Automation.ScriptBlock ScriptBlock
        {
            get
            {
                return base.ScriptBlock;
            }
            set
            {
                base.ScriptBlock = value;
            }
        }

        [Parameter(Position=0, ParameterSetName="FilePathRunspace"), Parameter(Position=0, ParameterSetName="Session"), ValidateNotNullOrEmpty]
        public override PSSession[] Session
        {
            get
            {
                return base.Session;
            }
            set
            {
                base.Session = value;
            }
        }

        [ValidateNotNullOrEmpty, Parameter(ParameterSetName="ComputerName"), Parameter(ParameterSetName="FilePathComputerName")]
        public string[] SessionName
        {
            get
            {
                return base.DisconnectedSessionName;
            }
            set
            {
                base.DisconnectedSessionName = value;
            }
        }

        [Parameter(ParameterSetName="ComputerName"), Parameter(ParameterSetName="Uri"), Parameter(ParameterSetName="FilePathComputerName"), Parameter(ParameterSetName="FilePathUri")]
        public override PSSessionOption SessionOption
        {
            get
            {
                return base.SessionOption;
            }
            set
            {
                base.SessionOption = value;
            }
        }

        [Parameter(ParameterSetName="Uri"), Parameter(ParameterSetName="FilePathComputerName"), Parameter(ParameterSetName="FilePathRunspace"), Parameter(ParameterSetName="FilePathUri"), Parameter(ParameterSetName="Session"), Parameter(ParameterSetName="ComputerName")]
        public override int ThrottleLimit
        {
            get
            {
                return base.ThrottleLimit;
            }
            set
            {
                base.ThrottleLimit = value;
            }
        }

        [Parameter(ParameterSetName="FilePathComputerName"), Parameter(ParameterSetName="ComputerName")]
        public override SwitchParameter UseSSL
        {
            get
            {
                return base.UseSSL;
            }
            set
            {
                base.UseSSL = value;
            }
        }
    }
}

