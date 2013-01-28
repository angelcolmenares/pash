namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Remoting.Client;
    using System.Management.Automation.Runspaces;
    using System.Runtime.InteropServices;

    [Cmdlet("Receive", "PSSession", SupportsShouldProcess=true, DefaultParameterSetName="Session", HelpUri="http://go.microsoft.com/fwlink/?LinkID=217037", RemotingCapability=RemotingCapability.OwnedByCommand)]
    public class ReceivePSSessionCommand : PSRemotingCmdlet
    {
        private Job _job;
        private RemotePipeline _remotePipeline;
        private bool _stopProcessing;
        private object _syncObject = new object();
        private bool allowRedirection;
        private string appName;
        private AuthenticationMechanism authentication;
        private const string ComputerInstanceIdParameterSet = "ComputerInstanceId";
        private string computerName;
        private const string ComputerSessionNameParameterSet = "ComputerSessionName";
        private const string ConnectionUriInstanceIdParameterSet = "ConnectionUriInstanceId";
        private const string ConnectionUriSessionNameParameterSet = "ConnectionUriSessionName";
        private int id;
        private const string IdParameterSet = "Id";
        private Guid instanceId;
        private const string InstanceIdParameterSet = "InstanceId";
        private string jobName = string.Empty;
        private string name;
        private const string NameParameterSet = "SessionName";
        private Microsoft.PowerShell.Commands.OutTarget outputMode;
        private int port;
        private PSCredential psCredential;
        private PSSession remotePSSessionInfo;
        private PSSessionOption sessionOption;
        private string shell;
        private string thumbprint;
        private Uri uris;
        private SwitchParameter useSSL;

        private PSSession ConnectSession(PSSession session, out Exception ex)
        {
            ex = null;
            if ((session == null) || ((session.Runspace.RunspaceStateInfo.State != RunspaceState.Opened) && (session.Runspace.RunspaceStateInfo.State != RunspaceState.Disconnected)))
            {
                return null;
            }
            if (session.Runspace.RunspaceStateInfo.State != RunspaceState.Opened)
            {
                try
                {
                    session.Runspace.Connect();
                }
                catch (PSInvalidOperationException exception)
                {
                    ex = exception;
                }
                catch (InvalidRunspaceStateException exception2)
                {
                    ex = exception2;
                }
                catch (RuntimeException exception3)
                {
                    ex = exception3;
                }
                if (ex != null)
                {
                    return null;
                }
            }
            return session;
        }

        private void ConnectSessionToHost(PSSession session, PSRemotingJob job = null)
        {
            RemoteRunspace runspace = session.Runspace as RemoteRunspace;
            if (job != null)
            {
                lock (this._syncObject)
                {
                    this._job = job;
                }
                using (job)
                {
                    Job job2 = job.ChildJobs[0];
                    job.ConnectJobs();
                    do
                    {
                        job2.Results.WaitHandle.WaitOne();
                        foreach (PSStreamObject obj2 in job2.ReadAll())
                        {
                            if (obj2 != null)
                            {
                                obj2.WriteStreamObject(this, false);
                            }
                        }
                    }
                    while (!job.IsFinishedState(job.JobStateInfo.State));
                }
                lock (this._syncObject)
                {
                    this._job = null;
                }
            }
            else if (runspace.RemoteCommand != null)
            {
                lock (this._syncObject)
                {
                    this._remotePipeline = (RemotePipeline) session.Runspace.CreateDisconnectedPipeline();
                }
                using (this._remotePipeline)
                {
                    this._remotePipeline.ConnectAsync();
                    runspace.RunspacePool.RemoteRunspacePoolInternal.ConnectCommands = null;
                    while (!this._remotePipeline.Output.EndOfPipeline)
                    {
                        if (this._stopProcessing)
                        {
                            break;
                        }
                        this._remotePipeline.Output.WaitHandle.WaitOne();
                        while (this._remotePipeline.Output.Count > 0)
                        {
                            if (this._stopProcessing)
                            {
                                continue;
                            }
                            PSObject psObject = this._remotePipeline.Output.Read();
                            this.WriteRemoteObject(psObject, session);
                        }
                    }
                    if (this._remotePipeline.Error.Count > 0)
                    {
                        while (!this._remotePipeline.Error.EndOfPipeline)
                        {
                            object obj4 = this._remotePipeline.Error.Read();
                            if (obj4 is Collection<ErrorRecord>)
                            {
                                Collection<ErrorRecord> collection = (Collection<ErrorRecord>) obj4;
                                foreach (ErrorRecord record in collection)
                                {
                                    base.WriteError(record);
                                }
                            }
                            else if (obj4 is ErrorRecord)
                            {
                                base.WriteError((ErrorRecord) obj4);
                            }
                        }
                    }
                    this._remotePipeline.PipelineFinishedEvent.WaitOne();
                    if (this._remotePipeline.PipelineStateInfo.State == PipelineState.Failed)
                    {
                        string pipelineFailedWithoutReason;
                        Exception reason = this._remotePipeline.PipelineStateInfo.Reason;
                        if ((reason != null) && !string.IsNullOrEmpty(reason.Message))
                        {
                            pipelineFailedWithoutReason = StringUtil.Format(RemotingErrorIdStrings.PipelineFailedWithReason, reason.Message);
                        }
                        else
                        {
                            pipelineFailedWithoutReason = RemotingErrorIdStrings.PipelineFailedWithoutReason;
                        }
                        ErrorRecord errorRecord = new ErrorRecord(new RuntimeException(pipelineFailedWithoutReason, reason), "ReceivePSSessionPipelineFailed", ErrorCategory.OperationStopped, this._remotePipeline);
                        base.WriteError(errorRecord);
                    }
                }
                lock (this._syncObject)
                {
                    this._remotePipeline = null;
                }
            }
        }

        private void ConnectSessionToJob(PSSession session, PSRemotingJob job = null)
        {
            bool flag = false;
            if (job == null)
            {
                List<IThrottleOperation> helpers = new List<IThrottleOperation>();
                Pipeline pipeline = session.Runspace.CreateDisconnectedPipeline();
                helpers.Add(new DisconnectedJobOperation(pipeline));
                job = new PSRemotingJob(helpers, 0, this.JobName, false);
                job.PSJobTypeName = InvokeCommandCommand.RemoteJobType;
                job.HideComputerName = false;
                flag = true;
            }
            if (job.JobStateInfo.State == JobState.Disconnected)
            {
                job.ConnectJob(session.Runspace.InstanceId);
                if (flag)
                {
                    base.JobRepository.Add(job);
                }
            }
            base.WriteObject(job);
        }

        private PSRemotingJob FindJobForSession(PSSession session)
        {
            PSRemotingJob job = null;
            RemoteRunspace runspace = session.Runspace as RemoteRunspace;
            if ((runspace == null) || (runspace.RemoteCommand != null))
            {
                return null;
            }
            foreach (Job job2 in base.JobRepository.Jobs)
            {
                if (!(job2 is PSRemotingJob))
                {
                    continue;
                }
                foreach (PSRemotingChildJob job3 in job2.ChildJobs)
                {
                    if (job3.Runspace.InstanceId.Equals(session.InstanceId) && (job3.JobStateInfo.State == JobState.Disconnected))
                    {
                        job = (PSRemotingJob) job2;
                        break;
                    }
                }
                if (job != null)
                {
                    return job;
                }
            }
            return job;
        }

        private void GetAndConnectSessionCommand()
        {
            PSSession sessionById = null;
            if (base.ParameterSetName == "Session")
            {
                sessionById = this.Session;
            }
            else if (base.ParameterSetName == "Id")
            {
                sessionById = this.GetSessionById(this.Id);
                if (sessionById == null)
                {
                    this.WriteInvalidArgumentError(PSRemotingErrorId.RemoteRunspaceNotAvailableForSpecifiedSessionId, RemotingErrorIdStrings.RemoteRunspaceNotAvailableForSpecifiedSessionId, this.Id);
                    return;
                }
            }
            else if (base.ParameterSetName == "SessionName")
            {
                sessionById = this.GetSessionByName(this.Name);
                if (sessionById == null)
                {
                    this.WriteInvalidArgumentError(PSRemotingErrorId.RemoteRunspaceNotAvailableForSpecifiedName, RemotingErrorIdStrings.RemoteRunspaceNotAvailableForSpecifiedName, this.Name);
                    return;
                }
            }
            else if (base.ParameterSetName == "InstanceId")
            {
                sessionById = this.GetSessionByInstanceId(this.InstanceId);
                if (sessionById == null)
                {
                    this.WriteInvalidArgumentError(PSRemotingErrorId.RemoteRunspaceNotAvailableForSpecifiedRunspaceId, RemotingErrorIdStrings.RemoteRunspaceNotAvailableForSpecifiedRunspaceId, this.InstanceId);
                    return;
                }
            }
            if (base.ShouldProcess(sessionById.Name, "Receive"))
            {
                Exception exception;
                if (this.ConnectSession(sessionById, out exception) == null)
                {
                    PSSession session = sessionById;
                    sessionById = this.TryGetSessionFromServer(session);
                    if (sessionById == null)
                    {
                        string message = StringUtil.Format(RemotingErrorIdStrings.RunspaceCannotBeConnected, session.Name);
                        base.WriteError(new ErrorRecord(new ArgumentException(message, exception), "ReceivePSSessionCannotConnectSession", ErrorCategory.InvalidOperation, session));
                        return;
                    }
                }
                PSRemotingJob job = this.FindJobForSession(sessionById);
                if (job != null)
                {
                    if (this.OutTarget == Microsoft.PowerShell.Commands.OutTarget.Host)
                    {
                        this.ConnectSessionToHost(sessionById, job);
                    }
                    else
                    {
                        this.ConnectSessionToJob(sessionById, job);
                    }
                }
                else if (this.OutTarget == Microsoft.PowerShell.Commands.OutTarget.Job)
                {
                    this.ConnectSessionToJob(sessionById, null);
                }
                else
                {
                    this.ConnectSessionToHost(sessionById, null);
                }
                if (sessionById.Runspace.RunspaceStateInfo.State != RunspaceState.Disconnected)
                {
                    base.RunspaceRepository.AddOrReplace(sessionById);
                }
            }
        }

        private WSManConnectionInfo GetConnectionObject()
        {
            WSManConnectionInfo connectionInfo = new WSManConnectionInfo();
            if ((base.ParameterSetName == "ComputerSessionName") || (base.ParameterSetName == "ComputerInstanceId"))
            {
                string str = this.UseSSL.IsPresent ? "https" : "http";
                connectionInfo.Scheme = str;
                connectionInfo.ComputerName = base.ResolveComputerName(this.ComputerName);
                connectionInfo.AppName = this.ApplicationName;
                connectionInfo.ShellUri = this.ConfigurationName;
                connectionInfo.Port = this.Port;
                if (this.CertificateThumbprint != null)
                {
                    connectionInfo.CertificateThumbprint = this.CertificateThumbprint;
                }
                else
                {
                    connectionInfo.Credential = this.Credential;
                }
                connectionInfo.AuthenticationMechanism = this.Authentication;
                this.UpdateConnectionInfo(connectionInfo);
                return connectionInfo;
            }
            connectionInfo.ConnectionUri = this.ConnectionUri;
            connectionInfo.ShellUri = this.ConfigurationName;
            if (this.CertificateThumbprint != null)
            {
                connectionInfo.CertificateThumbprint = this.CertificateThumbprint;
            }
            else
            {
                connectionInfo.Credential = this.Credential;
            }
            connectionInfo.AuthenticationMechanism = this.Authentication;
            this.UpdateConnectionInfo(connectionInfo);
            return connectionInfo;
        }

        private PSSession GetSessionById(int id)
        {
            foreach (PSSession session in base.RunspaceRepository.Runspaces)
            {
                if (session.Id == id)
                {
                    return session;
                }
            }
            return null;
        }

        private PSSession GetSessionByInstanceId(Guid instanceId)
        {
            foreach (PSSession session in base.RunspaceRepository.Runspaces)
            {
                if (instanceId.Equals(session.InstanceId))
                {
                    return session;
                }
            }
            return null;
        }

        private PSSession GetSessionByName(string name)
        {
            WildcardPattern pattern = new WildcardPattern(name, WildcardOptions.IgnoreCase);
            foreach (PSSession session in base.RunspaceRepository.Runspaces)
            {
                if (pattern.IsMatch(session.Name))
                {
                    return session;
                }
            }
            return null;
        }

        protected override void ProcessRecord()
        {
            if ((base.ParameterSetName == "ComputerSessionName") || (base.ParameterSetName == "ConnectionUriSessionName"))
            {
                this.QueryForAndConnectCommands(this.Name, Guid.Empty);
            }
            else if ((base.ParameterSetName == "ComputerInstanceId") || (base.ParameterSetName == "ConnectionUriInstanceId"))
            {
                this.QueryForAndConnectCommands(string.Empty, this.InstanceId);
            }
            else
            {
                this.GetAndConnectSessionCommand();
            }
        }

        private void QueryForAndConnectCommands(string name, Guid instanceId)
        {
            Runspace[] runspaceArray;
            WSManConnectionInfo connectionObject = this.GetConnectionObject();
            try
            {
                runspaceArray = Runspace.GetRunspaces(connectionObject, base.Host, QueryRunspaces.BuiltInTypesTable);
            }
            catch (RuntimeException exception)
            {
                int num;
                string message = StringUtil.Format(RemotingErrorIdStrings.QueryForRunspacesFailed, connectionObject.ComputerName, QueryRunspaces.ExtractMessage(exception.InnerException, out num));
                string fQEIDFromTransportError = WSManTransportManagerUtils.GetFQEIDFromTransportError(num, "ReceivePSSessionQueryForSessionFailed");
                Exception exception2 = new RuntimeException(message, exception.InnerException);
                ErrorRecord errorRecord = new ErrorRecord(exception2, fQEIDFromTransportError, ErrorCategory.InvalidOperation, connectionObject);
                base.WriteError(errorRecord);
                return;
            }
            string str3 = null;
            if (!string.IsNullOrEmpty(this.ConfigurationName))
            {
                str3 = (this.ConfigurationName.IndexOf("http://schemas.microsoft.com/powershell/", StringComparison.OrdinalIgnoreCase) != -1) ? this.ConfigurationName : ("http://schemas.microsoft.com/powershell/" + this.ConfigurationName);
            }
            foreach (Runspace runspace in runspaceArray)
            {
                if (this._stopProcessing)
                {
                    return;
                }
                if (str3 != null)
                {
                    WSManConnectionInfo connectionInfo = runspace.ConnectionInfo as WSManConnectionInfo;
                    if ((connectionInfo != null) && !str3.Equals(connectionInfo.ShellUri, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }
                bool flag = false;
                if (!string.IsNullOrEmpty(name) && (string.Compare(name, ((RemoteRunspace) runspace).RunspacePool.RemoteRunspacePoolInternal.Name, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    flag = true;
                }
                else if (instanceId.Equals(runspace.InstanceId))
                {
                    flag = true;
                }
                if (flag && base.ShouldProcess(((RemoteRunspace) runspace).Name, "Receive"))
                {
                    Exception exception3;
                    PSSession item = base.RunspaceRepository.GetItem(runspace.InstanceId);
                    PSSession session2 = this.ConnectSession(item, out exception3);
                    if (session2 != null)
                    {
                        base.RunspaceRepository.AddOrReplace(session2);
                        PSRemotingJob job = this.FindJobForSession(session2);
                        if (this.OutTarget == Microsoft.PowerShell.Commands.OutTarget.Host)
                        {
                            this.ConnectSessionToHost(session2, job);
                            return;
                        }
                        this.ConnectSessionToJob(session2, job);
                        return;
                    }
                    PSSession session = new PSSession(runspace as RemoteRunspace);
                    session2 = this.ConnectSession(session, out exception3);
                    if (session2 != null)
                    {
                        if (item != null)
                        {
                            session2 = item.InsertRunspace(session2.Runspace as RemoteRunspace) ? item : session2;
                        }
                        base.RunspaceRepository.AddOrReplace(session2);
                        if (this.OutTarget == Microsoft.PowerShell.Commands.OutTarget.Job)
                        {
                            this.ConnectSessionToJob(session2, null);
                            return;
                        }
                        this.ConnectSessionToHost(session2, null);
                        return;
                    }
                    string str4 = StringUtil.Format(RemotingErrorIdStrings.RunspaceCannotBeConnected, session.Name);
                    base.WriteError(new ErrorRecord(new ArgumentException(str4, exception3), "ReceivePSSessionCannotConnectSession", ErrorCategory.InvalidOperation, session));
                    return;
                }
            }
        }

        protected override void StopProcessing()
        {
            RemotePipeline pipeline;
            Job job;
            lock (this._syncObject)
            {
                this._stopProcessing = true;
                pipeline = this._remotePipeline;
                job = this._job;
            }
            if (pipeline != null)
            {
                pipeline.StopAsync();
            }
            if (job != null)
            {
                job.StopJob();
            }
        }

        private PSSession TryGetSessionFromServer(PSSession session)
        {
            if (session.Runspace is RemoteRunspace)
            {
                RemoteRunspace remoteRunspace = null;
                foreach (Runspace runspace2 in Runspace.GetRunspaces(session.Runspace.ConnectionInfo, base.Host, QueryRunspaces.BuiltInTypesTable))
                {
                    if (runspace2.InstanceId == session.Runspace.InstanceId)
                    {
                        remoteRunspace = runspace2 as RemoteRunspace;
                        break;
                    }
                }
                if (remoteRunspace != null)
                {
                    session = session.InsertRunspace(remoteRunspace) ? session : new PSSession(remoteRunspace);
                    return session;
                }
            }
            return null;
        }

        private void UpdateConnectionInfo(WSManConnectionInfo connectionInfo)
        {
            if ((base.ParameterSetName != "ConnectionUriInstanceId") && (base.ParameterSetName != "ConnectionUriSessionName"))
            {
                connectionInfo.MaximumConnectionRedirectionCount = 0;
            }
            if (!this.allowRedirection)
            {
                connectionInfo.MaximumConnectionRedirectionCount = 0;
            }
            if (this.SessionOption != null)
            {
                connectionInfo.SetSessionOptions(this.SessionOption);
            }
        }

        private void WriteInvalidArgumentError(PSRemotingErrorId errorId, string resourceString, object errorArgument)
        {
            string message = base.GetMessage(resourceString, new object[] { errorArgument });
            base.WriteError(new ErrorRecord(new ArgumentException(message), errorId.ToString(), ErrorCategory.InvalidArgument, errorArgument));
        }

        private void WriteRemoteObject(PSObject psObject, PSSession session)
        {
            if (psObject != null)
            {
                if (psObject.Properties[RemotingConstants.ComputerNameNoteProperty] == null)
                {
                    psObject.Properties.Add(new PSNoteProperty(RemotingConstants.ComputerNameNoteProperty, session.ComputerName));
                }
                if (psObject.Properties[RemotingConstants.RunspaceIdNoteProperty] == null)
                {
                    psObject.Properties.Add(new PSNoteProperty(RemotingConstants.RunspaceIdNoteProperty, session.InstanceId));
                }
                if (psObject.Properties[RemotingConstants.ShowComputerNameNoteProperty] == null)
                {
                    psObject.Properties.Add(new PSNoteProperty(RemotingConstants.ShowComputerNameNoteProperty, true));
                }
                base.WriteObject(psObject);
            }
        }

        [Parameter(ParameterSetName="ConnectionUriSessionName"), Parameter(ParameterSetName="ConnectionUriInstanceId")]
        public SwitchParameter AllowRedirection
        {
            get
            {
                return this.allowRedirection;
            }
            set
            {
                this.allowRedirection = (bool) value;
            }
        }

        [Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ComputerSessionName"), Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ComputerInstanceId")]
        public string ApplicationName
        {
            get
            {
                return this.appName;
            }
            set
            {
                this.appName = base.ResolveAppName(value);
            }
        }

        [Parameter(ParameterSetName="ComputerSessionName"), Parameter(ParameterSetName="ComputerInstanceId"), Parameter(ParameterSetName="ConnectionUriSessionName"), Parameter(ParameterSetName="ConnectionUriInstanceId")]
        public AuthenticationMechanism Authentication
        {
            get
            {
                return this.authentication;
            }
            set
            {
                this.authentication = value;
                PSRemotingBaseCmdlet.ValidateSpecifiedAuthentication(this.Credential, this.CertificateThumbprint, this.Authentication);
            }
        }

        [Parameter(ParameterSetName="ConnectionUriSessionName"), Parameter(ParameterSetName="ComputerSessionName"), Parameter(ParameterSetName="ComputerInstanceId"), Parameter(ParameterSetName="ConnectionUriInstanceId")]
        public string CertificateThumbprint
        {
            get
            {
                return this.thumbprint;
            }
            set
            {
                this.thumbprint = value;
                PSRemotingBaseCmdlet.ValidateSpecifiedAuthentication(this.Credential, this.CertificateThumbprint, this.Authentication);
            }
        }

        [ValidateNotNullOrEmpty, Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ComputerSessionName"), Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ComputerInstanceId"), Alias(new string[] { "Cn" })]
        public string ComputerName
        {
            get
            {
                return this.computerName;
            }
            set
            {
                this.computerName = value;
            }
        }

        [Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ConnectionUriInstanceId"), Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ComputerSessionName"), Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ComputerInstanceId"), Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ConnectionUriSessionName")]
        public string ConfigurationName
        {
            get
            {
                return this.shell;
            }
            set
            {
                this.shell = base.ResolveShell(value);
            }
        }

        [Alias(new string[] { "URI", "CU" }), Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ConnectionUriSessionName"), Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ConnectionUriInstanceId"), ValidateNotNullOrEmpty]
        public Uri ConnectionUri
        {
            get
            {
                return this.uris;
            }
            set
            {
                this.uris = value;
            }
        }

        [Credential, Parameter(ParameterSetName="ComputerInstanceId"), Parameter(ParameterSetName="ComputerSessionName"), Parameter(ParameterSetName="ConnectionUriSessionName"), Parameter(ParameterSetName="ConnectionUriInstanceId")]
        public PSCredential Credential
        {
            get
            {
                return this.psCredential;
            }
            set
            {
                this.psCredential = value;
                PSRemotingBaseCmdlet.ValidateSpecifiedAuthentication(this.Credential, this.CertificateThumbprint, this.Authentication);
            }
        }

        [Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, ValueFromPipeline=true, ParameterSetName="Id")]
        public int Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="ComputerInstanceId"), Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, ValueFromPipeline=true, ParameterSetName="InstanceId"), Parameter(Mandatory=true, ParameterSetName="ConnectionUriInstanceId"), ValidateNotNullOrEmpty]
        public Guid InstanceId
        {
            get
            {
                return this.instanceId;
            }
            set
            {
                this.instanceId = value;
            }
        }

        [Parameter(ParameterSetName="InstanceId"), Parameter(ParameterSetName="Session"), Parameter(ParameterSetName="Id"), Parameter(ParameterSetName="ConnectionUriInstanceId"), ValidateNotNullOrEmpty, Parameter(ParameterSetName="ComputerInstanceId"), Parameter(ParameterSetName="ComputerSessionName"), Parameter(ParameterSetName="ConnectionUriSessionName"), Parameter(ParameterSetName="SessionName")]
        public string JobName
        {
            get
            {
                return this.jobName;
            }
            set
            {
                this.jobName = value;
            }
        }

        [Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, ValueFromPipeline=true, ParameterSetName="SessionName"), Parameter(Mandatory=true, ParameterSetName="ComputerSessionName"), Parameter(Mandatory=true, ParameterSetName="ConnectionUriSessionName"), ValidateNotNullOrEmpty]
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        [Parameter(ParameterSetName="SessionName"), Parameter(ParameterSetName="Session"), Parameter(ParameterSetName="Id"), Parameter(ParameterSetName="InstanceId"), Parameter(ParameterSetName="ComputerInstanceId"), Parameter(ParameterSetName="ComputerSessionName"), Parameter(ParameterSetName="ConnectionUriSessionName"), Parameter(ParameterSetName="ConnectionUriInstanceId")]
        public Microsoft.PowerShell.Commands.OutTarget OutTarget
        {
            get
            {
                return this.outputMode;
            }
            set
            {
                this.outputMode = value;
            }
        }

        [Parameter(ParameterSetName="ComputerInstanceId"), Parameter(ParameterSetName="ComputerSessionName"), ValidateRange(1, 0xffff)]
        public int Port
        {
            get
            {
                return this.port;
            }
            set
            {
                this.port = value;
            }
        }

        [ValidateNotNullOrEmpty, Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, ValueFromPipeline=true, ParameterSetName="Session")]
        public PSSession Session
        {
            get
            {
                return this.remotePSSessionInfo;
            }
            set
            {
                this.remotePSSessionInfo = value;
            }
        }

        [Parameter(ParameterSetName="ConnectionUriSessionName"), Parameter(ParameterSetName="ComputerSessionName"), Parameter(ParameterSetName="ComputerInstanceId"), Parameter(ParameterSetName="ConnectionUriInstanceId")]
        public PSSessionOption SessionOption
        {
            get
            {
                return this.sessionOption;
            }
            set
            {
                this.sessionOption = value;
            }
        }

        [Parameter(ParameterSetName="ComputerInstanceId"), Parameter(ParameterSetName="ComputerSessionName")]
        public SwitchParameter UseSSL
        {
            get
            {
                return this.useSSL;
            }
            set
            {
                this.useSSL = value;
            }
        }
    }
}

