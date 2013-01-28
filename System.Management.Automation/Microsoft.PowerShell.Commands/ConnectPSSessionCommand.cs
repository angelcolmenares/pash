using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;
using System.Management.Automation.Remoting;
using System.Management.Automation.Remoting.Client;
using System.Management.Automation.Runspaces;
using System.Threading;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Connect", "PSSession", SupportsShouldProcess = true, DefaultParameterSetName = "Name", HelpUri = "http://go.microsoft.com/fwlink/?LinkID=210604", RemotingCapability = RemotingCapability.OwnedByCommand)]
    [OutputType(new Type[] { typeof(PSSession) })]
    public class ConnectPSSessionCommand : PSRunspaceCmdlet, IDisposable
    {
        private const string ComputerNameGuidParameterSet = "ComputerNameGuid";

        private const string ConnectionUriParameterSet = "ConnectionUri";

        private const string ConnectionUriGuidParameterSet = "ConnectionUriGuid";

        private PSSession[] remotePSSessionInfo;

        private string[] computerNames;

        private string appName;

        private string shell;

        private Uri[] uris;

        private bool allowRedirection;

        private PSCredential psCredential;

        private AuthenticationMechanism authentication;

        private string thumbprint;

        private int port;

        private SwitchParameter useSSL;

        private PSSessionOption sessionOption;

        private int throttleLimit;

        private Collection<PSSession> allSessions;

        private ThrottleManager throttleManager;

        private ManualResetEvent operationsComplete;

        private QueryRunspaces queryRunspaces;

        private ObjectStream stream;

        private ThrottleManager retryThrottleManager;

        private Collection<PSSession> failedSessions;

        [Parameter(ParameterSetName = "ConnectionUri")]
        [Parameter(ParameterSetName = "ConnectionUriGuid")]
        public SwitchParameter AllowRedirection
        {
            get
            {
                return this.allowRedirection;
            }
            set
            {
                this.allowRedirection = value;
            }
        }

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "ComputerName")]
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "ComputerNameGuid")]
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

        [Parameter(ParameterSetName = "ConnectionUriGuid")]
        [Parameter(ParameterSetName = "ComputerName")]
        [Parameter(ParameterSetName = "ComputerNameGuid")]
        [Parameter(ParameterSetName = "ConnectionUri")]
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

        [Parameter(ParameterSetName = "ConnectionUriGuid")]
        [Parameter(ParameterSetName = "ConnectionUri")]
        [Parameter(ParameterSetName = "ComputerNameGuid")]
        [Parameter(ParameterSetName = "ComputerName")]
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

        [Alias(new string[] { "Cn" })]
        [Parameter(Position = 0, ParameterSetName = "ComputerName", Mandatory = true)]
        [Parameter(ParameterSetName = "ComputerNameGuid", Mandatory = true)]
        [ValidateNotNullOrEmpty]
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

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "ConnectionUriGuid")]
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "ComputerName")]
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "ComputerNameGuid")]
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "ConnectionUri")]
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

        [Alias(new string[] { "URI", "CU" })]
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ConnectionUri")]
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ConnectionUriGuid")]
        [ValidateNotNullOrEmpty]
        public Uri[] ConnectionUri
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

        [Credential]
        [Parameter(ParameterSetName = "ConnectionUri")]
        [Parameter(ParameterSetName = "ConnectionUriGuid")]
        [Parameter(ParameterSetName = "ComputerName")]
        [Parameter(ParameterSetName = "ComputerNameGuid")]
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

        [Parameter(ParameterSetName = "ComputerNameGuid", Mandatory = true)]
        [Parameter(ParameterSetName = "ConnectionUriGuid", Mandatory = true)]
        [Parameter(ParameterSetName = "InstanceId", Mandatory = true)]
        [ValidateNotNull]
        public override Guid[] InstanceId
        {
            get
            {
                return base.InstanceId;
            }
            set
            {
                base.InstanceId = value;
            }
        }

        [Parameter(ParameterSetName = "Name", Mandatory = true)]
        [Parameter(ParameterSetName = "ComputerName")]
        [Parameter(ParameterSetName = "ConnectionUri")]
        [ValidateNotNullOrEmpty]
        public override string[] Name
        {
            get
            {
                return base.Name;
            }
            set
            {
                base.Name = value;
            }
        }

        [Parameter(ParameterSetName = "ComputerNameGuid")]
        [Parameter(ParameterSetName = "ComputerName")]
        [ValidateRange(1, 0xffff)]
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

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, ValueFromPipeline = true, ParameterSetName = "Session")]
        [ValidateNotNullOrEmpty]
        public PSSession[] Session
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

        [Parameter(ParameterSetName = "ConnectionUriGuid")]
        [Parameter(ParameterSetName = "ComputerName")]
        [Parameter(ParameterSetName = "ComputerNameGuid")]
        [Parameter(ParameterSetName = "ConnectionUri")]
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

        [Parameter(ParameterSetName = "Name")]
        [Parameter(ParameterSetName = "Session")]
        [Parameter(ParameterSetName = "ComputerName")]
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "ComputerNameGuid")]
        [Parameter(ParameterSetName = "ConnectionUri")]
        [Parameter(ParameterSetName = "ConnectionUriGuid")]
        [Parameter(ParameterSetName = "InstanceId")]
        public int ThrottleLimit
        {
            get
            {
                return this.throttleLimit;
            }
            set
            {
                this.throttleLimit = value;
            }
        }

        [Parameter(ParameterSetName = "ComputerName")]
        [Parameter(ParameterSetName = "ComputerNameGuid")]
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

        public ConnectPSSessionCommand()
        {
            this.allSessions = new Collection<PSSession>();
            this.throttleManager = new ThrottleManager();
            this.operationsComplete = new ManualResetEvent(true);
            this.queryRunspaces = new QueryRunspaces();
            this.stream = new ObjectStream();
            this.retryThrottleManager = new ThrottleManager();
            this.failedSessions = new Collection<PSSession>();
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            this.throttleManager.ThrottleLimit = this.ThrottleLimit;
            this.throttleManager.ThrottleComplete += new EventHandler<EventArgs>(this.HandleThrottleConnectComplete);
        }

        private Collection<PSSession> CollectDisconnectedSessions(ConnectPSSessionCommand.OverrideParameter overrideParam = 0)
        {
            Collection<PSSession> pSSessions = new Collection<PSSession>();
            if (base.ParameterSetName != "Session")
            {
                Dictionary<Guid, PSSession> matchingRunspaces = null;
                ConnectPSSessionCommand.OverrideParameter overrideParameter = overrideParam;
                switch (overrideParameter)
                {
                    case ConnectPSSessionCommand.OverrideParameter.None:
                        {
                            matchingRunspaces = base.GetMatchingRunspaces(false, true);
                            break;
                        }
                    case ConnectPSSessionCommand.OverrideParameter.Name:
                        {
                            matchingRunspaces = base.GetMatchingRunspacesByName(false, true);
                            break;
                        }
                    case ConnectPSSessionCommand.OverrideParameter.InstanceId:
                        {
                            matchingRunspaces = base.GetMatchingRunspacesByRunspaceId(false, true);
                            break;
                        }
                }
                if (matchingRunspaces != null)
                {
                    foreach (PSSession value in matchingRunspaces.Values)
                    {
                        pSSessions.Add(value);
                    }
                }
            }
            else
            {
                if (this.remotePSSessionInfo != null)
                {
                    PSSession[] pSSessionArray = this.remotePSSessionInfo;
                    for (int i = 0; i < (int)pSSessionArray.Length; i++)
                    {
                        PSSession pSSession = pSSessionArray[i];
                        pSSessions.Add(pSSession);
                    }
                }
            }
            return pSSessions;
        }

        private void ConnectSessions(Collection<PSSession> psSessions)
        {
            List<IThrottleOperation> throttleOperations = new List<IThrottleOperation>();
            foreach (PSSession psSession in psSessions)
            {
                if (base.ShouldProcess(psSession.Name, "Connect"))
                {
                    if (psSession.Runspace.RunspaceStateInfo.State != RunspaceState.Disconnected || psSession.Runspace.RunspaceAvailability != RunspaceAvailability.None)
                    {
                        if (psSession.Runspace.RunspaceStateInfo.State == RunspaceState.Opened)
                        {
                            base.WriteObject(psSession);
                        }
                        else
                        {
                            string str = StringUtil.Format(RemotingErrorIdStrings.RunspaceCannotBeConnected, psSession.Name);
                            Exception runtimeException = new RuntimeException(str);
                            ErrorRecord errorRecord = new ErrorRecord(runtimeException, "PSSessionConnectFailed", ErrorCategory.InvalidOperation, psSession);
                            base.WriteError(errorRecord);
                        }
                    }
                    else
                    {
                        this.UpdateConnectionInfo(psSession.Runspace.ConnectionInfo as WSManConnectionInfo);
                        ConnectPSSessionCommand.ConnectRunspaceOperation connectRunspaceOperation = new ConnectPSSessionCommand.ConnectRunspaceOperation(psSession, this.stream, base.Host, null, this.failedSessions);
                        throttleOperations.Add(connectRunspaceOperation);
                    }
                }
                this.allSessions.Add(psSession);
            }
            if (throttleOperations.Count > 0)
            {
                this.operationsComplete.Reset();
                this.throttleManager.SubmitOperations(throttleOperations);
                Collection<object> objs = this.stream.ObjectReader.NonBlockingRead();
                foreach (object obj in objs)
                {
                    base.WriteStreamObject((Action<Cmdlet>)obj);
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
                this.throttleManager.Dispose();
                this.operationsComplete.WaitOne();
                this.operationsComplete.Close();
                this.throttleManager.ThrottleComplete -= new EventHandler<EventArgs>(this.HandleThrottleConnectComplete);
                this.retryThrottleManager.Dispose();
                this.stream.Dispose();
            }
        }

        protected override void EndProcessing()
        {
            this.throttleManager.EndSubmitOperations();
            this.operationsComplete.WaitOne();
            if (this.failedSessions.Count > 0)
            {
                this.RetryFailedSessions();
            }
            while (this.stream.ObjectReader.Count > 0)
            {
                object obj = this.stream.ObjectReader.Read();
                base.WriteStreamObject((Action<Cmdlet>)obj);
            }
            this.stream.ObjectWriter.Close();
            foreach (PSSession allSession in this.allSessions)
            {
                if (allSession.Runspace.RunspaceStateInfo.State != RunspaceState.Opened)
                {
                    continue;
                }
                base.RunspaceRepository.AddOrReplace(allSession);
            }
        }

        private Collection<WSManConnectionInfo> GetConnectionObjects()
        {
            string str;
            Collection<WSManConnectionInfo> wSManConnectionInfos = new Collection<WSManConnectionInfo>();
            if (base.ParameterSetName == "ComputerName" || base.ParameterSetName == "ComputerNameGuid")
            {
                SwitchParameter useSSL = this.UseSSL;
                if (useSSL.IsPresent)
                {
                    str = "https";
                }
                else
                {
                    str = "http";
                }
                string str1 = str;
                string[] computerName = this.ComputerName;
                for (int i = 0; i < (int)computerName.Length; i++)
                {
                    string str2 = computerName[i];
                    WSManConnectionInfo wSManConnectionInfo = new WSManConnectionInfo();
                    wSManConnectionInfo.Scheme = str1;
                    wSManConnectionInfo.ComputerName = base.ResolveComputerName(str2);
                    wSManConnectionInfo.AppName = this.ApplicationName;
                    wSManConnectionInfo.ShellUri = this.ConfigurationName;
                    wSManConnectionInfo.Port = this.Port;
                    if (this.CertificateThumbprint == null)
                    {
                        wSManConnectionInfo.Credential = this.Credential;
                    }
                    else
                    {
                        wSManConnectionInfo.CertificateThumbprint = this.CertificateThumbprint;
                    }
                    wSManConnectionInfo.AuthenticationMechanism = this.Authentication;
                    this.UpdateConnectionInfo(wSManConnectionInfo);
                    wSManConnectionInfos.Add(wSManConnectionInfo);
                }
            }
            else
            {
                if (base.ParameterSetName == "ConnectionUri" || base.ParameterSetName == "ConnectionUriGuid")
                {
                    Uri[] connectionUri = this.ConnectionUri;
                    for (int j = 0; j < (int)connectionUri.Length; j++)
                    {
                        Uri uri = connectionUri[j];
                        WSManConnectionInfo configurationName = new WSManConnectionInfo();
                        configurationName.ConnectionUri = uri;
                        configurationName.ShellUri = this.ConfigurationName;
                        if (this.CertificateThumbprint == null)
                        {
                            configurationName.Credential = this.Credential;
                        }
                        else
                        {
                            configurationName.CertificateThumbprint = this.CertificateThumbprint;
                        }
                        configurationName.AuthenticationMechanism = this.Authentication;
                        this.UpdateConnectionInfo(configurationName);
                        wSManConnectionInfos.Add(configurationName);
                    }
                }
            }
            return wSManConnectionInfos;
        }

        private void HandleThrottleConnectComplete(object sender, EventArgs eventArgs)
        {
            this.operationsComplete.Set();
        }

        protected override void ProcessRecord()
        {
            Collection<PSSession> pSSessions = new Collection<PSSession>();
            try
            {
                if (base.ParameterSetName == "ComputerName" || base.ParameterSetName == "ComputerNameGuid" || base.ParameterSetName == "ConnectionUri" || base.ParameterSetName == "ConnectionUriGuid")
                {
                    pSSessions = this.QueryForDisconnectedSessions();
                }
                else
                {
                    pSSessions = this.CollectDisconnectedSessions(ConnectPSSessionCommand.OverrideParameter.None);
                }
            }
            catch (PSRemotingDataStructureException pSRemotingDataStructureException)
            {
                this.operationsComplete.Set();
                throw;
            }
            catch (PSRemotingTransportException pSRemotingTransportException)
            {
                this.operationsComplete.Set();
                throw;
            }
            catch (RemoteException remoteException)
            {
                this.operationsComplete.Set();
                throw;
            }
            catch (InvalidRunspaceStateException invalidRunspaceStateException)
            {
                this.operationsComplete.Set();
                throw;
            }
            this.ConnectSessions(pSSessions);
        }

        private Collection<PSSession> QueryForDisconnectedSessions()
        {
            Collection<WSManConnectionInfo> connectionObjects = this.GetConnectionObjects();
            Collection<PSSession> disconnectedSessions = this.queryRunspaces.GetDisconnectedSessions(connectionObjects, base.Host, this.stream, base.RunspaceRepository, this.throttleLimit, SessionFilterState.Disconnected, this.InstanceId, this.Name, this.ConfigurationName);
            Collection<object> objs = this.stream.ObjectReader.NonBlockingRead();
            foreach (object obj in objs)
            {
                base.WriteStreamObject((Action<Cmdlet>)obj);
            }
            return disconnectedSessions;
        }

        private void RetryFailedSessions()
        {
            EventHandler<EventArgs> eventHandler = null;
            using (ManualResetEvent manualResetEvent = new ManualResetEvent(false))
            {
                Collection<PSSession> pSSessions = new Collection<PSSession>();
                List<IThrottleOperation> throttleOperations = new List<IThrottleOperation>();
                this.retryThrottleManager.ThrottleLimit = this.ThrottleLimit;
                ThrottleManager throttleManager = this.retryThrottleManager;
                if (eventHandler == null)
                {
                    eventHandler = (object sender, EventArgs eventArgs) => manualResetEvent.Set();
                }
                throttleManager.ThrottleComplete += eventHandler;
                foreach (PSSession failedSession in this.failedSessions)
                {
                    throttleOperations.Add(new ConnectPSSessionCommand.ConnectRunspaceOperation(failedSession, this.stream, base.Host, new QueryRunspaces(), pSSessions));
                }
                this.retryThrottleManager.SubmitOperations(throttleOperations);
                this.retryThrottleManager.EndSubmitOperations();
                manualResetEvent.WaitOne();
                foreach (PSSession pSSession in pSSessions)
                {
                    base.RunspaceRepository.AddOrReplace(pSSession);
                }
            }
        }

        protected override void StopProcessing()
        {
            this.stream.ObjectWriter.Close();
            this.queryRunspaces.StopAllOperations();
            this.throttleManager.StopAllOperations();
            this.retryThrottleManager.StopAllOperations();
        }

        private void UpdateConnectionInfo(WSManConnectionInfo connectionInfo)
        {
            if (base.ParameterSetName != "ConnectionUri" && base.ParameterSetName != "ConnectionUriGuid")
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

        private class ConnectRunspaceOperation : IThrottleOperation
        {
            private PSSession _session;

            private PSSession _oldSession;

            private ObjectStream _writeStream;

            private Collection<PSSession> _retryList;

            private PSHost _host;

            private QueryRunspaces _queryRunspaces;

            private static object s_LockObject;

            static ConnectRunspaceOperation()
            {
                ConnectPSSessionCommand.ConnectRunspaceOperation.s_LockObject = new object();
            }

            internal ConnectRunspaceOperation(PSSession session, ObjectStream stream, PSHost host, QueryRunspaces queryRunspaces, Collection<PSSession> retryList)
            {
                this._session = session;
                this._writeStream = stream;
                this._host = host;
                this._queryRunspaces = queryRunspaces;
                this._retryList = retryList;
                this._session.Runspace.StateChanged += new EventHandler<RunspaceStateEventArgs>(this.StateCallBackHandler);
            }

            internal PSSession QueryForSession(PSSession session)
            {
                Collection<WSManConnectionInfo> wSManConnectionInfos = new Collection<WSManConnectionInfo>();
                wSManConnectionInfos.Add(session.Runspace.ConnectionInfo as WSManConnectionInfo);
                Exception runtimeException = null;
                Collection<PSSession> disconnectedSessions = null;
                try
                {
                    Guid[] instanceId = new Guid[1];
                    instanceId[0] = session.InstanceId;
                    disconnectedSessions = this._queryRunspaces.GetDisconnectedSessions(wSManConnectionInfos, this._host, this._writeStream, null, 0, SessionFilterState.Disconnected, instanceId, null, null);
                }
                catch (RuntimeException runtimeException2)
                {
                    RuntimeException runtimeException1 = runtimeException2;
                    runtimeException = runtimeException1;
                }
                if (runtimeException == null)
                {
                    if (disconnectedSessions.Count == 1)
                    {
                        return disconnectedSessions[0];
                    }
                    else
                    {
                        runtimeException = new RuntimeException(StringUtil.Format(RemotingErrorIdStrings.CannotFindSessionForConnect, session.Name, session.ComputerName));
                        this.WriteConnectFailed(runtimeException, session);
                        return null;
                    }
                }
                else
                {
                    this.WriteConnectFailed(runtimeException, session);
                    return null;
                }
            }

            private void SendStartComplete()
            {
                OperationStateEventArgs operationStateEventArg = new OperationStateEventArgs();
                operationStateEventArg.OperationState = OperationState.StartComplete;
                this.OperationComplete.SafeInvoke<OperationStateEventArgs>(this, operationStateEventArg);
            }

            private void SendStopComplete()
            {
                OperationStateEventArgs operationStateEventArg = new OperationStateEventArgs();
                operationStateEventArg.OperationState = OperationState.StopComplete;
                this.OperationComplete.SafeInvoke<OperationStateEventArgs>(this, operationStateEventArg);
            }

            internal override void StartOperation()
            {
                bool flag = true;
                Exception exception = null;
                try
                {
                    if (this._queryRunspaces == null)
                    {
                        this._session.Runspace.ConnectAsync();
                    }
                    else
                    {
                        PSSession pSSession = this.QueryForSession(this._session);
                        if (pSSession == null)
                        {
                            flag = false;
                        }
                        else
                        {
                            this._session.Runspace.StateChanged -= new EventHandler<RunspaceStateEventArgs>(this.StateCallBackHandler);
                            this._oldSession = this._session;
                            this._session = pSSession;
                            this._session.Runspace.StateChanged += new EventHandler<RunspaceStateEventArgs>(this.StateCallBackHandler);
                            this._session.Runspace.ConnectAsync();
                        }
                    }
                }
                catch (PSInvalidOperationException pSInvalidOperationException1)
                {
                    PSInvalidOperationException pSInvalidOperationException = pSInvalidOperationException1;
                    exception = pSInvalidOperationException;
                }
                catch (InvalidRunspacePoolStateException invalidRunspacePoolStateException1)
                {
                    InvalidRunspacePoolStateException invalidRunspacePoolStateException = invalidRunspacePoolStateException1;
                    exception = invalidRunspacePoolStateException;
                }
                catch (RuntimeException runtimeException1)
                {
                    RuntimeException runtimeException = runtimeException1;
                    exception = runtimeException;
                }
                if (exception != null)
                {
                    flag = false;
                    this.WriteConnectFailed(exception, this._session);
                }
                if (!flag)
                {
                    this._session.Runspace.StateChanged -= new EventHandler<RunspaceStateEventArgs>(this.StateCallBackHandler);
                    this.SendStartComplete();
                }
            }

            private void StateCallBackHandler(object sender, RunspaceStateEventArgs eArgs)
            {
                if (eArgs.RunspaceStateInfo.State == RunspaceState.Connecting || eArgs.RunspaceStateInfo.State == RunspaceState.Disconnecting || eArgs.RunspaceStateInfo.State == RunspaceState.Disconnected)
                {
                    return;
                }
                else
                {
                    if (eArgs.RunspaceStateInfo.State != RunspaceState.Opened)
                    {
                        bool flag = true;
                        if (this._queryRunspaces == null)
                        {
                            PSRemotingTransportException reason = eArgs.RunspaceStateInfo.Reason as PSRemotingTransportException;
                            if (reason != null && reason.ErrorCode == -2144108083)
                            {
                                lock (ConnectPSSessionCommand.ConnectRunspaceOperation.s_LockObject)
                                {
                                    this._retryList.Add(this._session);
                                }
                                flag = false;
                            }
                        }
                        if (flag)
                        {
                            this.WriteConnectFailed(eArgs.RunspaceStateInfo.Reason, this._session);
                        }
                    }
                    else
                    {
                        this.WriteConnectedPSSession();
                    }
                    this._session.Runspace.StateChanged -= new EventHandler<RunspaceStateEventArgs>(this.StateCallBackHandler);
                    this.SendStartComplete();
                    return;
                }
            }

            internal override void StopOperation()
            {
                if (this._queryRunspaces != null)
                {
                    this._queryRunspaces.StopAllOperations();
                }
                this._session.Runspace.StateChanged -= new EventHandler<RunspaceStateEventArgs>(this.StateCallBackHandler);
                this.SendStopComplete();
            }

            private void WriteConnectedPSSession()
            {
                Action<Cmdlet> action = null;
                if (this._queryRunspaces != null)
                {
                    lock (ConnectPSSessionCommand.ConnectRunspaceOperation.s_LockObject)
                    {
                        if (this._oldSession == null || !this._oldSession.InsertRunspace(this._session.Runspace as RemoteRunspace))
                        {
                            this._retryList.Add(this._session);
                        }
                        else
                        {
                            this._session = this._oldSession;
                            this._retryList.Add(this._oldSession);
                        }
                    }
                }
                if (this._writeStream.ObjectWriter.IsOpen)
                {
                    if (action == null)
                    {
                        action = (Cmdlet cmdlet) => cmdlet.WriteObject(this._session);
                    }
                    Action<Cmdlet> action1 = action;
                    this._writeStream.ObjectWriter.Write(action1);
                }
            }

            private void WriteConnectFailed(Exception e, PSSession session)
            {
                Exception runtimeException;
                if (this._writeStream.ObjectWriter.IsOpen)
                {
                    string fQEIDFromTransportError = "PSSessionConnectFailed";
                    if (e == null || string.IsNullOrEmpty(e.Message))
                    {
                        runtimeException = new RuntimeException(StringUtil.Format(RemotingErrorIdStrings.RunspaceConnectFailed, session.Name, session.Runspace.RunspaceStateInfo.State.ToString()), null);
                    }
                    else
                    {
                        PSRemotingTransportException pSRemotingTransportException = e as PSRemotingTransportException;
                        if (pSRemotingTransportException != null)
                        {
                            fQEIDFromTransportError = WSManTransportManagerUtils.GetFQEIDFromTransportError(pSRemotingTransportException.ErrorCode, fQEIDFromTransportError);
                        }
                        runtimeException = new RuntimeException(StringUtil.Format(RemotingErrorIdStrings.RunspaceConnectFailedWithMessage, session.Name, e.Message), e);
                    }
                    ErrorRecord errorRecord = new ErrorRecord(runtimeException, fQEIDFromTransportError, ErrorCategory.InvalidOperation, null);
                    Action<Cmdlet> action = (Cmdlet cmdlet) => cmdlet.WriteError(errorRecord);
                    this._writeStream.ObjectWriter.Write(action);
                }
            }

            internal override event EventHandler<OperationStateEventArgs> OperationComplete;
        }

        private enum OverrideParameter
        {
            None,
            Name,
            InstanceId
        }
    }
}