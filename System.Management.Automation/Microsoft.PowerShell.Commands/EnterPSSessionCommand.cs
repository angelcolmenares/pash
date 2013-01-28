namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Internal.Host;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;

    [Cmdlet("Enter", "PSSession", DefaultParameterSetName="ComputerName", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135210", RemotingCapability=RemotingCapability.OwnedByCommand)]
    public class EnterPSSessionCommand : PSRemotingBaseCmdlet
    {
        private string computerName;
        private Uri connectionUri;
        private SwitchParameter enableNetworkAccess;
        private const string IdParameterSet = "Id";
        private const string InstanceIdParameterSet = "InstanceId";
        private string name;
        private const string NameParameterSet = "Name";
        private Guid remoteRunspaceId;
        private PSSession remoteRunspaceInfo;
        private int sessionId;
        private ObjectStream stream;

        private RemoteRunspace CreateRunspaceWhenComputerNameParameterSpecified()
        {
            RemoteRunspace runspace = null;
            string computerName = base.ResolveComputerName(this.computerName);
            try
            {
                WSManConnectionInfo connectionInfo = new WSManConnectionInfo(this.UseSSL.IsPresent, computerName, this.Port, this.ApplicationName, this.ConfigurationName, this.Credential) {
                    AuthenticationMechanism = this.Authentication
                };
                base.UpdateConnectionInfo(connectionInfo);
                connectionInfo.EnableNetworkAccess = (bool) this.EnableNetworkAccess;
                runspace = this.CreateTemporaryRemoteRunspace(base.Host, connectionInfo);
            }
            catch (InvalidOperationException exception)
            {
                this.WriteErrorCreateRemoteRunspaceFailed(exception, computerName);
            }
            catch (ArgumentException exception2)
            {
                this.WriteErrorCreateRemoteRunspaceFailed(exception2, computerName);
            }
            catch (PSRemotingTransportException exception3)
            {
                this.WriteErrorCreateRemoteRunspaceFailed(exception3, computerName);
            }
            return runspace;
        }

        private RemoteRunspace CreateRunspaceWhenUriParameterSpecified()
        {
            RemoteRunspace runspace = null;
            try
            {
                WSManConnectionInfo connectionInfo = new WSManConnectionInfo(this.ConnectionUri, this.ConfigurationName, this.Credential) {
                    AuthenticationMechanism = this.Authentication
                };
                base.UpdateConnectionInfo(connectionInfo);
                connectionInfo.EnableNetworkAccess = (bool) this.EnableNetworkAccess;
                runspace = this.CreateTemporaryRemoteRunspace(base.Host, connectionInfo);
            }
            catch (UriFormatException exception)
            {
                this.WriteErrorCreateRemoteRunspaceFailed(exception, this.ConnectionUri);
            }
            catch (InvalidOperationException exception2)
            {
                this.WriteErrorCreateRemoteRunspaceFailed(exception2, this.ConnectionUri);
            }
            catch (ArgumentException exception3)
            {
                this.WriteErrorCreateRemoteRunspaceFailed(exception3, this.ConnectionUri);
            }
            catch (PSRemotingTransportException exception4)
            {
                this.WriteErrorCreateRemoteRunspaceFailed(exception4, this.ConnectionUri);
            }
            catch (NotSupportedException exception5)
            {
                this.WriteErrorCreateRemoteRunspaceFailed(exception5, this.ConnectionUri);
            }
            return runspace;
        }

        private RemoteRunspace CreateTemporaryRemoteRunspace(PSHost host, WSManConnectionInfo connectionInfo)
        {
            int num;
            string name = PSSession.GenerateRunspaceName(out num);
            RemoteRunspace runspace = new RemoteRunspace(Utils.GetTypeTableFromExecutionContextTLS(), connectionInfo, host, this.SessionOption.ApplicationArguments, name, num);
            runspace.URIRedirectionReported += new EventHandler<RemoteDataEventArgs<Uri>>(this.HandleURIDirectionReported);
            this.stream = new ObjectStream();
            try
            {
                runspace.Open();
                runspace.ShouldCloseOnPop = true;
            }
            finally
            {
                runspace.URIRedirectionReported -= new EventHandler<RemoteDataEventArgs<Uri>>(this.HandleURIDirectionReported);
                this.stream.ObjectWriter.Close();
                if (runspace.RunspaceStateInfo.State != RunspaceState.Opened)
                {
                    runspace.Dispose();
                    runspace = null;
                }
            }
            return runspace;
        }

        protected override void EndProcessing()
        {
            if (this.stream != null)
            {
                while (true)
                {
                    this.stream.ObjectReader.WaitHandle.WaitOne();
                    if (this.stream.ObjectReader.EndOfPipeline)
                    {
                        break;
                    }
                    object obj2 = this.stream.ObjectReader.Read();
                    base.WriteStreamObject((Action<Cmdlet>) obj2);
                }
            }
        }

        private RemoteRunspace GetRunspaceMatchingCondition(Predicate<PSSession> condition, PSRemotingErrorId tooFew, PSRemotingErrorId tooMany, string tooFewResourceString, string tooManyResourceString, object errorArgument)
        {
            List<PSSession> list = base.RunspaceRepository.Runspaces.FindAll(condition);
            RemoteRunspace runspace = null;
            if (list.Count == 0)
            {
                this.WriteInvalidArgumentError(tooFew, tooFewResourceString, errorArgument);
                return runspace;
            }
            if (list.Count > 1)
            {
                this.WriteInvalidArgumentError(tooMany, tooManyResourceString, errorArgument);
                return runspace;
            }
            return (RemoteRunspace) list[0].Runspace;
        }

        private RemoteRunspace GetRunspaceMatchingName(string name)
        {
            Predicate<PSSession> condition = info => info.Name.Equals(name, StringComparison.OrdinalIgnoreCase);
            PSRemotingErrorId remoteRunspaceNotAvailableForSpecifiedName = PSRemotingErrorId.RemoteRunspaceNotAvailableForSpecifiedName;
            PSRemotingErrorId remoteRunspaceHasMultipleMatchesForSpecifiedName = PSRemotingErrorId.RemoteRunspaceHasMultipleMatchesForSpecifiedName;
            string tooFewResourceString = RemotingErrorIdStrings.RemoteRunspaceNotAvailableForSpecifiedName;
            string tooManyResourceString = RemotingErrorIdStrings.RemoteRunspaceHasMultipleMatchesForSpecifiedName;
            return this.GetRunspaceMatchingCondition(condition, remoteRunspaceNotAvailableForSpecifiedName, remoteRunspaceHasMultipleMatchesForSpecifiedName, tooFewResourceString, tooManyResourceString, name);
        }

        private RemoteRunspace GetRunspaceMatchingRunspaceId(Guid remoteRunspaceId)
        {
            Predicate<PSSession> condition = info => info.InstanceId == remoteRunspaceId;
            PSRemotingErrorId remoteRunspaceNotAvailableForSpecifiedRunspaceId = PSRemotingErrorId.RemoteRunspaceNotAvailableForSpecifiedRunspaceId;
            PSRemotingErrorId remoteRunspaceHasMultipleMatchesForSpecifiedRunspaceId = PSRemotingErrorId.RemoteRunspaceHasMultipleMatchesForSpecifiedRunspaceId;
            string tooFewResourceString = RemotingErrorIdStrings.RemoteRunspaceNotAvailableForSpecifiedRunspaceId;
            string tooManyResourceString = RemotingErrorIdStrings.RemoteRunspaceHasMultipleMatchesForSpecifiedRunspaceId;
            return this.GetRunspaceMatchingCondition(condition, remoteRunspaceNotAvailableForSpecifiedRunspaceId, remoteRunspaceHasMultipleMatchesForSpecifiedRunspaceId, tooFewResourceString, tooManyResourceString, remoteRunspaceId);
        }

        private RemoteRunspace GetRunspaceMatchingSessionId(int sessionId)
        {
            Predicate<PSSession> condition = info => info.Id == sessionId;
            PSRemotingErrorId remoteRunspaceNotAvailableForSpecifiedSessionId = PSRemotingErrorId.RemoteRunspaceNotAvailableForSpecifiedSessionId;
            PSRemotingErrorId remoteRunspaceHasMultipleMatchesForSpecifiedSessionId = PSRemotingErrorId.RemoteRunspaceHasMultipleMatchesForSpecifiedSessionId;
            string tooFewResourceString = RemotingErrorIdStrings.RemoteRunspaceNotAvailableForSpecifiedSessionId;
            string tooManyResourceString = RemotingErrorIdStrings.RemoteRunspaceHasMultipleMatchesForSpecifiedSessionId;
            return this.GetRunspaceMatchingCondition(condition, remoteRunspaceNotAvailableForSpecifiedSessionId, remoteRunspaceHasMultipleMatchesForSpecifiedSessionId, tooFewResourceString, tooManyResourceString, sessionId);
        }

        private void HandleURIDirectionReported(object sender, RemoteDataEventArgs<Uri> eventArgs)
        {
            string message = StringUtil.Format(RemotingErrorIdStrings.URIRedirectWarningToHost, eventArgs.Data.OriginalString);
            Action<Cmdlet> action = cmdlet => cmdlet.WriteWarning(message);
            this.stream.Write(action);
        }

        protected override void ProcessRecord()
        {
            IHostSupportsInteractiveSession session = base.Host as IHostSupportsInteractiveSession;
            if (session == null)
            {
                base.WriteError(new ErrorRecord(new ArgumentException(base.GetMessage(RemotingErrorIdStrings.HostDoesNotSupportPushRunspace)), PSRemotingErrorId.HostDoesNotSupportPushRunspace.ToString(), ErrorCategory.InvalidArgument, null));
            }
			/*
            else if (((base.Context != null) && (base.Context.EngineHostInterface != null)) && ((base.Context.EngineHostInterface.ExternalHost != null) && (base.Context.EngineHostInterface.ExternalHost is ServerRemoteHost)))
            {
                base.WriteError(new ErrorRecord(new ArgumentException(base.GetMessage(RemotingErrorIdStrings.RemoteHostDoesNotSupportPushRunspace)), PSRemotingErrorId.RemoteHostDoesNotSupportPushRunspace.ToString(), ErrorCategory.InvalidArgument, null));
            }
			*/
			else
            {
                InternalHost targetObject = base.Host as InternalHost;
                if ((targetObject != null) && targetObject.HostInNestedPrompt())
                {
                    base.ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.HostInNestedPrompt, new object[0])), "HostInNestedPrompt", ErrorCategory.InvalidOperation, targetObject));
                }
                RemoteRunspace runspaceMatchingRunspaceId = null;
                string parameterSetName = base.ParameterSetName;
                if (parameterSetName != null)
                {
                    if (!(parameterSetName == "ComputerName"))
                    {
                        if (parameterSetName == "Uri")
                        {
                            runspaceMatchingRunspaceId = this.CreateRunspaceWhenUriParameterSpecified();
                        }
                        else if (parameterSetName == "Session")
                        {
                            runspaceMatchingRunspaceId = (RemoteRunspace) this.remoteRunspaceInfo.Runspace;
                        }
                        else if (parameterSetName == "InstanceId")
                        {
                            runspaceMatchingRunspaceId = this.GetRunspaceMatchingRunspaceId(this.InstanceId);
                        }
                        else if (parameterSetName == "Id")
                        {
                            runspaceMatchingRunspaceId = this.GetRunspaceMatchingSessionId(this.Id);
                        }
                        else if (parameterSetName == "Name")
                        {
                            runspaceMatchingRunspaceId = this.GetRunspaceMatchingName(this.Name);
                        }
                    }
                    else
                    {
                        runspaceMatchingRunspaceId = this.CreateRunspaceWhenComputerNameParameterSpecified();
                    }
                }
                if (runspaceMatchingRunspaceId != null)
                {
                    if (runspaceMatchingRunspaceId.RunspaceStateInfo.State == RunspaceState.Disconnected)
                    {
                        if (!runspaceMatchingRunspaceId.CanConnect)
                        {
                            string message = StringUtil.Format(RemotingErrorIdStrings.SessionNotAvailableForConnection, new object[0]);
                            base.WriteError(new ErrorRecord(new RuntimeException(message), "EnterPSSessionCannotConnectDisconnectedSession", ErrorCategory.InvalidOperation, runspaceMatchingRunspaceId));
                            return;
                        }
                        Exception innerException = null;
                        try
                        {
                            runspaceMatchingRunspaceId.Connect();
                        }
                        catch (PSRemotingTransportException exception2)
                        {
                            innerException = exception2;
                        }
                        catch (PSInvalidOperationException exception3)
                        {
                            innerException = exception3;
                        }
                        catch (InvalidRunspacePoolStateException exception4)
                        {
                            innerException = exception4;
                        }
                        if (innerException != null)
                        {
                            string str2 = StringUtil.Format(RemotingErrorIdStrings.SessionConnectFailed, new object[0]);
                            base.WriteError(new ErrorRecord(new RuntimeException(str2, innerException), "EnterPSSessionConnectSessionFailed", ErrorCategory.InvalidOperation, runspaceMatchingRunspaceId));
                            return;
                        }
                        if (runspaceMatchingRunspaceId.RunspaceAvailability == RunspaceAvailability.Busy)
                        {
                            string str3 = StringUtil.Format(RemotingErrorIdStrings.EnterPSSessionDisconnected, runspaceMatchingRunspaceId.Name);
                            base.WriteError(new ErrorRecord(new RuntimeException(str3, innerException), "EnterPSSessionConnectSessionNotAvailable", ErrorCategory.InvalidOperation, this.remoteRunspaceInfo));
                            runspaceMatchingRunspaceId.DisconnectAsync();
                            return;
                        }
                    }
                    if (runspaceMatchingRunspaceId.RunspaceStateInfo.State != RunspaceState.Opened)
                    {
                        if (base.ParameterSetName == "Session")
                        {
                            string str4 = (this.remoteRunspaceInfo != null) ? this.remoteRunspaceInfo.Name : string.Empty;
                            base.WriteError(new ErrorRecord(new ArgumentException(base.GetMessage(RemotingErrorIdStrings.EnterPSSessionBrokenSession, new object[] { str4, runspaceMatchingRunspaceId.ConnectionInfo.ComputerName, runspaceMatchingRunspaceId.InstanceId })), PSRemotingErrorId.PushedRunspaceMustBeOpen.ToString(), ErrorCategory.InvalidArgument, null));
                        }
                        else
                        {
                            base.WriteError(new ErrorRecord(new ArgumentException(base.GetMessage(RemotingErrorIdStrings.PushedRunspaceMustBeOpen)), PSRemotingErrorId.PushedRunspaceMustBeOpen.ToString(), ErrorCategory.InvalidArgument, null));
                        }
                    }
                    else
                    {
                        if (runspaceMatchingRunspaceId.RunspaceAvailability != RunspaceAvailability.Available)
                        {
                            base.WriteWarning(base.GetMessage(RunspaceStrings.RunspaceNotReady));
                        }
                        try
                        {
                            session.PushRunspace(runspaceMatchingRunspaceId);
                        }
                        catch (Exception)
                        {
                            if ((runspaceMatchingRunspaceId != null) && runspaceMatchingRunspaceId.ShouldCloseOnPop)
                            {
                                runspaceMatchingRunspaceId.Close();
                            }
                            throw;
                        }
                    }
                }
            }
        }

        protected override void StopProcessing()
        {
            IHostSupportsInteractiveSession host = base.Host as IHostSupportsInteractiveSession;
            if (host == null)
            {
                base.WriteError(new ErrorRecord(new ArgumentException(base.GetMessage(RemotingErrorIdStrings.HostDoesNotSupportPushRunspace)), PSRemotingErrorId.HostDoesNotSupportPushRunspace.ToString(), ErrorCategory.InvalidArgument, null));
            }
            else
            {
                host.PopRunspace();
            }
        }

        private void WriteErrorCreateRemoteRunspaceFailed(Exception exception, object argument)
        {
            PSRemotingTransportException exception2 = exception as PSRemotingTransportException;
            string str = null;
            if ((exception2 != null) && (exception2.ErrorCode == -2144108135))
            {
                str = PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.URIRedirectionReported, new object[] { exception2.Message, "MaximumConnectionRedirectionCount", "PSSessionOption", "AllowRedirection" });
            }
            ErrorRecord errorRecord = new ErrorRecord(exception, argument, "CreateRemoteRunspaceFailed", ErrorCategory.InvalidArgument, null, null, null, null, null, str, null);
            base.WriteError(errorRecord);
        }

        private void WriteInvalidArgumentError(PSRemotingErrorId errorId, string resourceString, object errorArgument)
        {
            string message = base.GetMessage(resourceString, new object[] { errorArgument });
            base.WriteError(new ErrorRecord(new ArgumentException(message), errorId.ToString(), ErrorCategory.InvalidArgument, errorArgument));
        }

        [Alias(new string[] { "Cn" }), ValidateNotNullOrEmpty, Parameter(Position=0, Mandatory=true, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ComputerName")]
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

        [Parameter(Position=1, ValueFromPipelineByPropertyName=true, ParameterSetName="Uri"), Alias(new string[] { "URI", "CU" }), ValidateNotNullOrEmpty]
        public Uri ConnectionUri
        {
            get
            {
                return this.connectionUri;
            }
            set
            {
                this.connectionUri = value;
            }
        }

        [Parameter(ParameterSetName="ComputerName"), Parameter(ParameterSetName="Uri")]
        public SwitchParameter EnableNetworkAccess
        {
            get
            {
                return this.enableNetworkAccess;
            }
            set
            {
                this.enableNetworkAccess = value;
            }
        }

        [ValidateNotNull, Parameter(Position=0, ValueFromPipelineByPropertyName=true, ParameterSetName="Id")]
        public int Id
        {
            get
            {
                return this.sessionId;
            }
            set
            {
                this.sessionId = value;
            }
        }

        [ValidateNotNull, Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="InstanceId")]
        public Guid InstanceId
        {
            get
            {
                return this.remoteRunspaceId;
            }
            set
            {
                this.remoteRunspaceId = value;
            }
        }

        [Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Name")]
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

        [Parameter(Position=0, ValueFromPipelineByPropertyName=true, ValueFromPipeline=true, ParameterSetName="Session"), ValidateNotNullOrEmpty]
        public PSSession Session
        {
            get
            {
                return this.remoteRunspaceInfo;
            }
            set
            {
                this.remoteRunspaceInfo = value;
            }
        }

        public int ThrottleLimit
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }
    }
}

