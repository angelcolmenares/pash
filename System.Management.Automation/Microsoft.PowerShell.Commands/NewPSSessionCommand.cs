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
    using System.Management.Automation.Runspaces.Internal;
    using System.Runtime.InteropServices;
    using System.Threading;

    [Cmdlet("New", "PSSession", DefaultParameterSetName="ComputerName", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135237", RemotingCapability=RemotingCapability.OwnedByCommand), OutputType(new Type[] { typeof(PSSession) })]
    public class NewPSSessionCommand : PSRemotingBaseCmdlet, IDisposable
    {
        private string _defaultFQEID = "PSSessionOpenFailed";
        private Collection<List<IThrottleOperation>> allOperations = new Collection<List<IThrottleOperation>>();
        private string[] computerNames;
        private SwitchParameter enableNetworkAccess;
        private string[] names;
        private ManualResetEvent operationsComplete = new ManualResetEvent(true);
        private PSSession[] remoteRunspaceInfos;
        private ObjectStream stream = new ObjectStream();
        private ThrottleManager throttleManager = new ThrottleManager();
        private List<RemoteRunspace> toDispose = new List<RemoteRunspace>();

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            this.operationsComplete.Reset();
            this.throttleManager.ThrottleLimit = this.ThrottleLimit;
            this.throttleManager.ThrottleComplete += new EventHandler<EventArgs>(this.HandleThrottleComplete);
        }

        private List<RemoteRunspace> CreateRunspacesWhenComputerNameParameterSpecified()
        {
            string[] strArray;
            List<RemoteRunspace> list = new List<RemoteRunspace>();
            base.ResolveComputerNames(this.ComputerName, out strArray);
            base.ValidateComputerName(strArray);
            for (int i = 0; i < strArray.Length; i++)
            {
                try
                {
                    int num2;
                    WSManConnectionInfo connectionInfo = null;
                    connectionInfo = new WSManConnectionInfo();
                    string str = this.UseSSL.IsPresent ? "https" : "http";
                    connectionInfo.ComputerName = strArray[i];
                    connectionInfo.Port = this.Port;
                    connectionInfo.AppName = this.ApplicationName;
                    connectionInfo.ShellUri = this.ConfigurationName;
                    connectionInfo.Scheme = str;
                    if (this.CertificateThumbprint != null)
                    {
                        connectionInfo.CertificateThumbprint = this.CertificateThumbprint;
                    }
                    else
                    {
                        connectionInfo.Credential = this.Credential;
                    }
                    connectionInfo.AuthenticationMechanism = this.Authentication;
                    base.UpdateConnectionInfo(connectionInfo);
                    connectionInfo.EnableNetworkAccess = (bool) this.EnableNetworkAccess;
                    string runspaceName = this.GetRunspaceName(i, out num2);
                    RemoteRunspace item = new RemoteRunspace(Utils.GetTypeTableFromExecutionContextTLS(), connectionInfo, base.Host, this.SessionOption.ApplicationArguments, runspaceName, num2);
                    list.Add(item);
                }
                catch (UriFormatException exception)
                {
                    PipelineWriter objectWriter = this.stream.ObjectWriter;
                    ErrorRecord errorRecord = new ErrorRecord(exception, "CreateRemoteRunspaceFailed", ErrorCategory.InvalidArgument, strArray[i]);
                    Action<Cmdlet> action = delegate (Cmdlet cmdlet) {
                        cmdlet.WriteError(errorRecord);
                    };
                    objectWriter.Write(action);
                }
            }
            return list;
        }

        private List<RemoteRunspace> CreateRunspacesWhenRunspaceParameterSpecified()
        {
            List<RemoteRunspace> list = new List<RemoteRunspace>();
            base.ValidateRemoteRunspacesSpecified();
            int rsIndex = 0;
            foreach (PSSession session in this.remoteRunspaceInfos)
            {
                if ((session == null) || (session.Runspace == null))
                {
                    base.ThrowTerminatingError(new ErrorRecord(new ArgumentNullException("PSSession"), "PSSessionArgumentNull", ErrorCategory.InvalidArgument, null));
                }
                else
                {
                    try
                    {
                        int num2;
                        RemoteRunspace runspace = (RemoteRunspace) session.Runspace;
                        WSManConnectionInfo connectionInfo = runspace.ConnectionInfo as WSManConnectionInfo;
                        WSManConnectionInfo info2 = null;
                        if (connectionInfo != null)
                        {
                            info2 = connectionInfo.Copy();
                            info2.EnableNetworkAccess = info2.EnableNetworkAccess || (this.EnableNetworkAccess != false);
                        }
                        else
                        {
                            Uri uri = WSManConnectionInfo.ExtractPropertyAsWsManConnectionInfo<Uri>(runspace.ConnectionInfo, "ConnectionUri", null);
                            string shellUri = WSManConnectionInfo.ExtractPropertyAsWsManConnectionInfo<string>(runspace.ConnectionInfo, "ShellUri", string.Empty);
                            info2 = new WSManConnectionInfo(uri, shellUri, runspace.ConnectionInfo.Credential);
                            base.UpdateConnectionInfo(info2);
                            info2.EnableNetworkAccess = (bool) this.EnableNetworkAccess;
                        }
                        RemoteRunspacePoolInternal remoteRunspacePoolInternal = runspace.RunspacePool.RemoteRunspacePoolInternal;
                        TypeTable typeTable = null;
                        if (((remoteRunspacePoolInternal != null) && (remoteRunspacePoolInternal.DataStructureHandler != null)) && (remoteRunspacePoolInternal.DataStructureHandler.TransportManager != null))
                        {
                            typeTable = remoteRunspacePoolInternal.DataStructureHandler.TransportManager.Fragmentor.TypeTable;
                        }
                        string runspaceName = this.GetRunspaceName(rsIndex, out num2);
                        RemoteRunspace item = new RemoteRunspace(typeTable, info2, base.Host, this.SessionOption.ApplicationArguments, runspaceName, num2);
                        list.Add(item);
                    }
                    catch (UriFormatException exception)
                    {
                        PipelineWriter objectWriter = this.stream.ObjectWriter;
                        ErrorRecord errorRecord = new ErrorRecord(exception, "CreateRemoteRunspaceFailed", ErrorCategory.InvalidArgument, session);
                        Action<Cmdlet> action = delegate (Cmdlet cmdlet) {
                            cmdlet.WriteError(errorRecord);
                        };
                        objectWriter.Write(action);
                    }
                }
                rsIndex++;
            }
            return list;
        }

        private List<RemoteRunspace> CreateRunspacesWhenUriParameterSpecified()
        {
            List<RemoteRunspace> list = new List<RemoteRunspace>();
            for (int i = 0; i < this.ConnectionUri.Length; i++)
            {
                try
                {
                    int num2;
                    WSManConnectionInfo connectionInfo = new WSManConnectionInfo {
                        ConnectionUri = this.ConnectionUri[i],
                        ShellUri = this.ConfigurationName
                    };
                    if (this.CertificateThumbprint != null)
                    {
                        connectionInfo.CertificateThumbprint = this.CertificateThumbprint;
                    }
                    else
                    {
                        connectionInfo.Credential = this.Credential;
                    }
                    connectionInfo.AuthenticationMechanism = this.Authentication;
                    base.UpdateConnectionInfo(connectionInfo);
                    connectionInfo.EnableNetworkAccess = (bool) this.EnableNetworkAccess;
                    string runspaceName = this.GetRunspaceName(i, out num2);
                    RemoteRunspace item = new RemoteRunspace(Utils.GetTypeTableFromExecutionContextTLS(), connectionInfo, base.Host, this.SessionOption.ApplicationArguments, runspaceName, num2);
                    list.Add(item);
                }
                catch (UriFormatException exception)
                {
                    this.WriteErrorCreateRemoteRunspaceFailed(exception, this.ConnectionUri[i]);
                }
                catch (InvalidOperationException exception2)
                {
                    this.WriteErrorCreateRemoteRunspaceFailed(exception2, this.ConnectionUri[i]);
                }
                catch (ArgumentException exception3)
                {
                    this.WriteErrorCreateRemoteRunspaceFailed(exception3, this.ConnectionUri[i]);
                }
                catch (NotSupportedException exception4)
                {
                    this.WriteErrorCreateRemoteRunspaceFailed(exception4, this.ConnectionUri[i]);
                }
            }
            return list;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.throttleManager.Dispose();
                this.operationsComplete.WaitOne();
                this.operationsComplete.Close();
                this.throttleManager.ThrottleComplete -= new EventHandler<EventArgs>(this.HandleThrottleComplete);
                this.throttleManager = null;
                foreach (RemoteRunspace runspace in this.toDispose)
                {
                    runspace.Dispose();
                }
                foreach (List<IThrottleOperation> list in this.allOperations)
                {
                    foreach (OpenRunspaceOperation operation in list)
                    {
                        operation.Dispose();
                    }
                }
                this.stream.Dispose();
            }
        }

        protected override void EndProcessing()
        {
            this.throttleManager.EndSubmitOperations();
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

        private string GetRunspaceName(int rsIndex, out int rsId)
        {
            string str = PSSession.GenerateRunspaceName(out rsId);
            if ((this.names != null) && (rsIndex < this.names.Length))
            {
                str = this.names[rsIndex];
            }
            return str;
        }

        private void HandleRunspaceStateChanged(object sender, OperationStateEventArgs stateEventArgs)
        {
            ErrorRecord errorRecord;
            PSRemotingTransportException exception2;
            string str;
            if (sender == null)
            {
                throw PSTraceSource.NewArgumentNullException("sender");
            }
            if (stateEventArgs == null)
            {
                throw PSTraceSource.NewArgumentNullException("stateEventArgs");
            }
            RunspaceStateEventArgs baseEvent = stateEventArgs.BaseEvent as RunspaceStateEventArgs;
            RunspaceState state = baseEvent.RunspaceStateInfo.State;
            OpenRunspaceOperation operation = sender as OpenRunspaceOperation;
            RemoteRunspace operatedRunspace = operation.OperatedRunspace;
            if (operatedRunspace != null)
            {
                operatedRunspace.URIRedirectionReported -= new EventHandler<RemoteDataEventArgs<Uri>>(this.HandleURIDirectionReported);
            }
            PipelineWriter objectWriter = this.stream.ObjectWriter;
            Exception reason = baseEvent.RunspaceStateInfo.Reason;
            switch (state)
            {
                case RunspaceState.Opened:
                {
                    PSSession remoteRunspaceInfo = new PSSession(operatedRunspace);
                    base.RunspaceRepository.Add(remoteRunspaceInfo);
                    Action<Cmdlet> action = cmdlet => cmdlet.WriteObject(remoteRunspaceInfo);
                    if (objectWriter.IsOpen)
                    {
                        objectWriter.Write(action);
                    }
                    return;
                }
                case RunspaceState.Closed:
                {
                    Uri uri = WSManConnectionInfo.ExtractPropertyAsWsManConnectionInfo<Uri>(operatedRunspace.ConnectionInfo, "ConnectionUri", null);
                    string message = base.GetMessage(RemotingErrorIdStrings.RemoteRunspaceClosed, new object[] { (uri != null) ? uri.AbsoluteUri : string.Empty });
                    Action<Cmdlet> action3 = cmdlet => cmdlet.WriteVerbose(message);
                    if (objectWriter.IsOpen)
                    {
                        objectWriter.Write(action3);
                    }
                    if (reason != null)
                    {
                        ErrorRecord errorRecord2 = new ErrorRecord(reason, "PSSessionStateClosed", ErrorCategory.OpenError, operatedRunspace);
                        Action<Cmdlet> action4 = cmdlet => cmdlet.WriteError(errorRecord2);
                        if (objectWriter.IsOpen)
                        {
                            objectWriter.Write(action4);
                        }
                    }
                    return;
                }
                case RunspaceState.Closing:
                    return;

                case RunspaceState.Broken:
                    exception2 = reason as PSRemotingTransportException;
                    str = null;
                    if (exception2 != null)
                    {
                        OpenRunspaceOperation operation2 = sender as OpenRunspaceOperation;
                        if (operation2 != null)
                        {
                            string computerName = operation2.OperatedRunspace.ConnectionInfo.ComputerName;
                            if (exception2.ErrorCode != -2144108135)
                            {
                                str = "[" + computerName + "] ";
                                if (!string.IsNullOrEmpty(exception2.Message))
                                {
                                    str = str + exception2.Message;
                                }
                                else if (!string.IsNullOrEmpty(exception2.TransportMessage))
                                {
                                    str = str + exception2.TransportMessage;
                                }
                                break;
                            }
                            string str3 = PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.URIRedirectionReported, new object[] { exception2.Message, "MaximumConnectionRedirectionCount", "PSSessionOption", "AllowRedirection" });
                            str = "[" + computerName + "] " + str3;
                        }
                    }
                    break;

                default:
                    return;
            }
            PSRemotingDataStructureException exception3 = reason as PSRemotingDataStructureException;
            if (exception3 != null)
            {
                OpenRunspaceOperation operation3 = sender as OpenRunspaceOperation;
                if (operation3 != null)
                {
                    string str4 = operation3.OperatedRunspace.ConnectionInfo.ComputerName;
                    str = "[" + str4 + "] " + exception3.Message;
                }
            }
            if (reason == null)
            {
                reason = new RuntimeException(base.GetMessage(RemotingErrorIdStrings.RemoteRunspaceOpenUnknownState, new object[] { state }));
            }
            string fQEIDFromTransportError = WSManTransportManagerUtils.GetFQEIDFromTransportError((exception2 != null) ? exception2.ErrorCode : 0, this._defaultFQEID);
            errorRecord = new ErrorRecord(reason, operatedRunspace, fQEIDFromTransportError, ErrorCategory.OpenError, null, null, null, null, null, str, null);
            Action<Cmdlet> action2 = cmdlet => cmdlet.WriteError(errorRecord);
            if (objectWriter.IsOpen)
            {
                objectWriter.Write(action2);
            }
            this.toDispose.Add(operatedRunspace);
        }

        private void HandleThrottleComplete(object sender, EventArgs eventArgs)
        {
            this.stream.ObjectWriter.Close();
            this.operationsComplete.Set();
        }

        private void HandleURIDirectionReported(object sender, RemoteDataEventArgs<Uri> eventArgs)
        {
            string message = StringUtil.Format(RemotingErrorIdStrings.URIRedirectWarningToHost, eventArgs.Data.OriginalString);
            Action<Cmdlet> action = cmdlet => cmdlet.WriteWarning(message);
            this.stream.Write(action);
        }

        private void OnRunspacePSEventReceived(object sender, PSEventArgs e)
        {
            if (base.Events != null)
            {
                base.Events.AddForwardedEvent(e);
            }
        }

        protected override void ProcessRecord()
        {
            List<RemoteRunspace> list = null;
            List<IThrottleOperation> operations = new List<IThrottleOperation>();
            string parameterSetName = base.ParameterSetName;
            if (parameterSetName != null)
            {
                if (!(parameterSetName == "Session"))
                {
                    if (parameterSetName == "Uri")
                    {
                        list = this.CreateRunspacesWhenUriParameterSpecified();
                        goto Label_0060;
                    }
                    if (parameterSetName == "ComputerName")
                    {
                        list = this.CreateRunspacesWhenComputerNameParameterSpecified();
                        goto Label_0060;
                    }
                }
                else
                {
                    list = this.CreateRunspacesWhenRunspaceParameterSpecified();
                    goto Label_0060;
                }
            }
            list = new List<RemoteRunspace>();
        Label_0060:
            foreach (RemoteRunspace runspace in list)
            {
                runspace.Events.ReceivedEvents.PSEventReceived += new PSEventReceivedEventHandler(this.OnRunspacePSEventReceived);
                OpenRunspaceOperation item = new OpenRunspaceOperation(runspace);
                item.OperationComplete += new EventHandler<OperationStateEventArgs>(this.HandleRunspaceStateChanged);
                runspace.URIRedirectionReported += new EventHandler<RemoteDataEventArgs<Uri>>(this.HandleURIDirectionReported);
                operations.Add(item);
            }
            this.throttleManager.SubmitOperations(operations);
            this.allOperations.Add(operations);
            foreach (object obj2 in this.stream.ObjectReader.NonBlockingRead())
            {
                base.WriteStreamObject((Action<Cmdlet>) obj2);
            }
        }

        protected override void StopProcessing()
        {
            this.stream.ObjectWriter.Close();
            this.throttleManager.StopAllOperations();
        }

        private void WriteErrorCreateRemoteRunspaceFailed(Exception e, Uri uri)
        {
            PipelineWriter objectWriter = this.stream.ObjectWriter;
            ErrorRecord errorRecord = new ErrorRecord(e, "CreateRemoteRunspaceFailed", ErrorCategory.InvalidArgument, uri);
            Action<Cmdlet> action = cmdlet => cmdlet.WriteError(errorRecord);
            objectWriter.Write(action);
        }

        [Parameter(Position=0, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ComputerName"), Alias(new string[] { "Cn" }), ValidateNotNullOrEmpty]
        public override string[] ComputerName
        {
            get
            {
                return this.computerNames;
            }
            set
            {
                this.computerNames = value;
            }
        }

        [Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ComputerName"), Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Uri"), Credential]
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

        [Parameter]
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

        [Parameter]
        public string[] Name
        {
            get
            {
                return this.names;
            }
            set
            {
                this.names = value;
            }
        }

        [Parameter(Position=0, ValueFromPipelineByPropertyName=true, ValueFromPipeline=true, ParameterSetName="Session"), ValidateNotNullOrEmpty]
        public override PSSession[] Session
        {
            get
            {
                return this.remoteRunspaceInfos;
            }
            set
            {
                this.remoteRunspaceInfos = value;
            }
        }
    }
}

