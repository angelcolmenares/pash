namespace System.Management.Automation.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Remoting.Client;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Runspaces.Internal;
    using System.Management.Automation.Tracing;
    using System.Threading;

    internal class ClientRunspacePoolDataStructureHandler : IDisposable
    {
        private bool _reconnecting;
        private PSPrimitiveDictionary applicationArguments;
        private Dictionary<Guid, ClientPowerShellDataStructureHandler> associatedPowerShellDSHandlers = new Dictionary<Guid, ClientPowerShellDataStructureHandler>();
        private object associationSyncObject = new object();
        private Guid clientRunspacePoolId;
        private Exception closingReason;
        private bool createRunspaceCalled;
        private PSHost host;
        private int maxRunspaces;
        private int minRunspaces;
        private List<BaseClientCommandTransportManager> preparingForDisconnectList;
        private ClientRemoteSession remoteSession;
        private object syncObject = new object();
        private BaseClientSessionTransportManager transportManager;

        internal event EventHandler<RemoteDataEventArgs<PSPrimitiveDictionary>> ApplicationPrivateDataReceived;

        internal event EventHandler<RemoteDataEventArgs<PSEventArgs>> PSEventArgsReceived;

        internal event EventHandler<RemoteDataEventArgs<RemoteHostCall>> RemoteHostCallReceived;

        internal event EventHandler<RemoteDataEventArgs<RunspacePoolInitInfo>> RSPoolInitInfoReceived;

        internal event EventHandler<RemoteDataEventArgs<Exception>> SessionClosed;

        internal event EventHandler<RemoteDataEventArgs<Exception>> SessionClosing;

        internal event EventHandler<CreateCompleteEventArgs> SessionCreateCompleted;

        internal event EventHandler<RemoteDataEventArgs<Exception>> SessionDisconnected;

        internal event EventHandler<RemoteDataEventArgs<Exception>> SessionRCDisconnecting;

        internal event EventHandler<RemoteDataEventArgs<Exception>> SessionReconnected;

        internal event EventHandler<RemoteDataEventArgs<PSObject>> SetMaxMinRunspacesResponseRecieved;

        internal event EventHandler<RemoteDataEventArgs<RunspacePoolStateInfo>> StateInfoReceived;

        internal event EventHandler<RemoteDataEventArgs<Uri>> URIRedirectionReported;

        internal ClientRunspacePoolDataStructureHandler(RemoteRunspacePoolInternal clientRunspacePool, TypeTable typeTable)
        {
            this.clientRunspacePoolId = clientRunspacePool.InstanceId;
            this.minRunspaces = clientRunspacePool.GetMinRunspaces();
            this.maxRunspaces = clientRunspacePool.GetMaxRunspaces();
            this.host = clientRunspacePool.Host;
            this.applicationArguments = clientRunspacePool.ApplicationArguments;
            this.remoteSession = this.CreateClientRemoteSession(clientRunspacePool);
            this.transportManager = this.remoteSession.SessionDataStructureHandler.TransportManager;
            this.transportManager.TypeTable = typeTable;
            this.remoteSession.StateChanged += new EventHandler<RemoteSessionStateEventArgs>(this.HandleClientRemoteSessionStateChanged);
            this._reconnecting = false;
            this.transportManager.RobustConnectionNotification += new EventHandler<ConnectionStatusEventArgs>(this.HandleRobustConnectionNotification);
            this.transportManager.CreateCompleted += new EventHandler<CreateCompleteEventArgs>(this.HandleSessionCreateCompleted);
        }

        internal void AddRemotePowerShellDSHandler(Guid psShellInstanceId, ClientPowerShellDataStructureHandler psDSHandler)
        {
            lock (this.associationSyncObject)
            {
                if (this.associatedPowerShellDSHandlers.ContainsKey(psShellInstanceId))
                {
                    this.associatedPowerShellDSHandlers.Remove(psShellInstanceId);
                }
                this.associatedPowerShellDSHandlers.Add(psShellInstanceId, psDSHandler);
            }
            psDSHandler.RemoveAssociation += new EventHandler(this.HandleRemoveAssociation);
        }

        internal void CloseRunspacePoolAsync()
        {
            this.remoteSession.CloseAsync();
        }

        internal void ConnectPoolAsync()
        {
            this.PrepareForConnect();
            this.remoteSession.ConnectAsync();
        }

        private ClientRemoteSessionImpl CreateClientRemoteSession(RemoteRunspacePoolInternal rsPoolInternal)
        {
            return new ClientRemoteSessionImpl(rsPoolInternal, new ClientRemoteSession.URIDirectionReported(this.HandleURIDirectionReported));
        }

        internal ClientPowerShellDataStructureHandler CreatePowerShellDataStructureHandler(ClientRemotePowerShell shell)
        {
            return new ClientPowerShellDataStructureHandler(this.remoteSession.SessionDataStructureHandler.CreateClientCommandTransportManager(shell, shell.NoInput), this.clientRunspacePoolId, shell.InstanceId);
        }

        internal void CreatePowerShellOnServerAndInvoke(ClientRemotePowerShell shell)
        {
            lock (this.associationSyncObject)
            {
                this.associatedPowerShellDSHandlers.Add(shell.InstanceId, shell.DataStructureHandler);
            }
            shell.DataStructureHandler.RemoveAssociation += new EventHandler(this.HandleRemoveAssociation);
            bool inDisconnectMode = (shell.Settings != null) ? shell.Settings.InvokeAndDisconnect : false;
            if (inDisconnectMode && !this.EndpointSupportsDisconnect)
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.EndpointDoesNotSupportDisconnect);
            }
            if (this.remoteSession == null)
            {
                throw new ObjectDisposedException("ClientRunspacePoolDataStructureHandler");
            }
            shell.DataStructureHandler.Start(this.remoteSession.SessionDataStructureHandler.StateMachine, inDisconnectMode);
        }

        internal void CreateRunspacePoolAndOpenAsync()
        {
            this.remoteSession.CreateAsync();
        }

        internal void DisconnectPoolAsync()
        {
            this.PrepareForAndStartDisconnect();
        }

        internal void DispatchMessageToPowerShell(RemoteDataObject<PSObject> rcvdData)
        {
            ClientPowerShellDataStructureHandler associatedPowerShellDataStructureHandler = this.GetAssociatedPowerShellDataStructureHandler(rcvdData.PowerShellId);
            if (associatedPowerShellDataStructureHandler != null)
            {
                associatedPowerShellDataStructureHandler.ProcessReceivedData(rcvdData);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (disposing && (this.remoteSession != null))
            {
                ((ClientRemoteSessionImpl) this.remoteSession).Dispose();
                this.remoteSession = null;
            }
        }

        private ClientPowerShellDataStructureHandler GetAssociatedPowerShellDataStructureHandler(Guid clientPowerShellId)
        {
            ClientPowerShellDataStructureHandler handler = null;
            lock (this.associationSyncObject)
            {
                if (!this.associatedPowerShellDSHandlers.TryGetValue(clientPowerShellId, out handler))
                {
					handler = null;
                }
            }
            return handler;
        }

        private void HandleClientRemoteSessionStateChanged(object sender, RemoteSessionStateEventArgs e)
        {
            if (e.SessionStateInfo.State == RemoteSessionState.NegotiationSending)
            {
                if (this.createRunspaceCalled)
                {
                    return;
                }
                lock (this.syncObject)
                {
                    if (this.createRunspaceCalled)
                    {
                        return;
                    }
                    this.createRunspaceCalled = true;
                }
                PSPrimitiveDictionary applicationArguments = PSPrimitiveDictionary.CloneAndAddPSVersionTable(this.applicationArguments);
                var createPoolData = RemotingEncoder.GenerateCreateRunspacePool(this.clientRunspacePoolId, this.minRunspaces, this.maxRunspaces, this.remoteSession.RemoteRunspacePoolInternal, this.host, applicationArguments);
				this.SendDataAsync (createPoolData);
            }
            if (e.SessionStateInfo.State == RemoteSessionState.NegotiationSendingOnConnect)
            {
                this.SendDataAsync(RemotingEncoder.GenerateConnectRunspacePool(this.clientRunspacePoolId, this.minRunspaces, this.maxRunspaces));
            }
            else if (e.SessionStateInfo.State == RemoteSessionState.ClosingConnection)
            {
                List<ClientPowerShellDataStructureHandler> list;
                Exception closingReason = this.closingReason;
                if (closingReason == null)
                {
                    closingReason = e.SessionStateInfo.Reason;
                    this.closingReason = closingReason;
                }
                lock (this.associationSyncObject)
                {
                    list = new List<ClientPowerShellDataStructureHandler>(this.associatedPowerShellDSHandlers.Values);
                }
                foreach (ClientPowerShellDataStructureHandler handler in list)
                {
                    handler.CloseConnectionAsync(this.closingReason);
                }
                this.SessionClosing.SafeInvoke<RemoteDataEventArgs<Exception>>(this, new RemoteDataEventArgs<Exception>(closingReason));
            }
            else if (e.SessionStateInfo.State == RemoteSessionState.Closed)
            {
                Exception reason = this.closingReason;
                if (reason == null)
                {
                    reason = e.SessionStateInfo.Reason;
                    this.closingReason = reason;
                }
                if (reason != null)
                {
                    this.NotifyAssociatedPowerShells(new RunspacePoolStateInfo(RunspacePoolState.Broken, reason));
                }
                else
                {
                    this.NotifyAssociatedPowerShells(new RunspacePoolStateInfo(RunspacePoolState.Closed, reason));
                }
                this.SessionClosed.SafeInvoke<RemoteDataEventArgs<Exception>>(this, new RemoteDataEventArgs<Exception>(reason));
            }
            else if (e.SessionStateInfo.State == RemoteSessionState.Connected)
            {
                PSEtwLog.ReplaceActivityIdForCurrentThread(this.clientRunspacePoolId, PSEventId.OperationalTransferEventRunspacePool, PSEventId.AnalyticTransferEventRunspacePool, PSKeyword.Runspace, PSTask.CreateRunspace);
            }
            else if (e.SessionStateInfo.State == RemoteSessionState.Disconnected)
            {
                this.NotifyAssociatedPowerShells(new RunspacePoolStateInfo(RunspacePoolState.Disconnected, e.SessionStateInfo.Reason));
                this.SessionDisconnected.SafeInvoke<RemoteDataEventArgs<Exception>>(this, new RemoteDataEventArgs<Exception>(e.SessionStateInfo.Reason));
            }
            else if (this._reconnecting && (e.SessionStateInfo.State == RemoteSessionState.Established))
            {
                this.SessionReconnected.SafeInvoke<RemoteDataEventArgs<Exception>>(this, new RemoteDataEventArgs<Exception>(null));
                this._reconnecting = false;
            }
            else if (e.SessionStateInfo.State == RemoteSessionState.RCDisconnecting)
            {
                this.SessionRCDisconnecting.SafeInvoke<RemoteDataEventArgs<Exception>>(this, new RemoteDataEventArgs<Exception>(null));
            }
            else if (e.SessionStateInfo.Reason != null)
            {
                this.closingReason = e.SessionStateInfo.Reason;
            }
        }

        private void HandleReadyForDisconnect(object sender, EventArgs args)
        {
            if (sender != null)
            {
                BaseClientCommandTransportManager item = (BaseClientCommandTransportManager) sender;
                lock (this.associationSyncObject)
                {
                    if (this.preparingForDisconnectList != null)
                    {
                        if (this.preparingForDisconnectList.Contains(item))
                        {
                            this.preparingForDisconnectList.Remove(item);
                        }
                        if (this.preparingForDisconnectList.Count == 0)
                        {
                            this.preparingForDisconnectList = null;
                            ThreadPool.QueueUserWorkItem(new WaitCallback(this.StartDisconnectAsync), this.remoteSession);
                        }
                    }
                }
            }
        }

        private void HandleRemoveAssociation(object sender, EventArgs e)
        {
            ClientPowerShellDataStructureHandler handler = sender as ClientPowerShellDataStructureHandler;
            lock (this.associationSyncObject)
            {
                this.associatedPowerShellDSHandlers.Remove(handler.PowerShellId);
            }
            this.transportManager.RemoveCommandTransportManager(handler.PowerShellId);
        }

        private void HandleRobustConnectionNotification(object sender, ConnectionStatusEventArgs e)
        {
            List<ClientPowerShellDataStructureHandler> list;
            lock (this.associationSyncObject)
            {
                list = new List<ClientPowerShellDataStructureHandler>(this.associatedPowerShellDSHandlers.Values);
            }
            foreach (ClientPowerShellDataStructureHandler handler in list)
            {
                handler.ProcessRobustConnectionNotification(e);
            }
        }

        private void HandleSessionCreateCompleted(object sender, CreateCompleteEventArgs eventArgs)
        {
            this.SessionCreateCompleted.SafeInvoke<CreateCompleteEventArgs>(this, eventArgs);
        }

        private void HandleURIDirectionReported(Uri newURI)
        {
            this.URIRedirectionReported.SafeInvoke<RemoteDataEventArgs<Uri>>(this, new RemoteDataEventArgs<Uri>(newURI));
        }

        private void NotifyAssociatedPowerShells(RunspacePoolStateInfo stateInfo)
        {
            List<ClientPowerShellDataStructureHandler> list;
            if (stateInfo.State == RunspacePoolState.Disconnected)
            {
                lock (this.associationSyncObject)
                {
                    list = new List<ClientPowerShellDataStructureHandler>(this.associatedPowerShellDSHandlers.Values);
                }
                foreach (ClientPowerShellDataStructureHandler handler in list)
                {
                    handler.ProcessDisconnect(stateInfo);
                }
            }
            else if ((stateInfo.State == RunspacePoolState.Broken) || (stateInfo.State == RunspacePoolState.Closed))
            {
                lock (this.associationSyncObject)
                {
                    list = new List<ClientPowerShellDataStructureHandler>(this.associatedPowerShellDSHandlers.Values);
                    this.associatedPowerShellDSHandlers.Clear();
                }
                if (stateInfo.State == RunspacePoolState.Broken)
                {
                    foreach (ClientPowerShellDataStructureHandler handler2 in list)
                    {
                        handler2.SetStateToFailed(stateInfo.Reason);
                    }
                }
                else if (stateInfo.State == RunspacePoolState.Closed)
                {
                    foreach (ClientPowerShellDataStructureHandler handler3 in list)
                    {
                        handler3.SetStateToStopped(stateInfo.Reason);
                    }
                }
            }
        }

        private void PrepareForAndStartDisconnect()
        {
            bool flag;
            lock (this.associationSyncObject)
            {
                if (this.associatedPowerShellDSHandlers.Count == 0)
                {
                    flag = true;
                    this.preparingForDisconnectList = null;
                }
                else
                {
                    flag = false;
                    this.preparingForDisconnectList = new List<BaseClientCommandTransportManager>();
                    foreach (ClientPowerShellDataStructureHandler handler in this.associatedPowerShellDSHandlers.Values)
                    {
                        this.preparingForDisconnectList.Add(handler.TransportManager);
                        handler.TransportManager.ReadyForDisconnect += new EventHandler<EventArgs>(this.HandleReadyForDisconnect);
                    }
                }
            }
            if (flag)
            {
                this.StartDisconnectAsync(this.remoteSession);
            }
            else
            {
                List<ClientPowerShellDataStructureHandler> list;
                lock (this.associationSyncObject)
                {
                    list = new List<ClientPowerShellDataStructureHandler>(this.associatedPowerShellDSHandlers.Values);
                }
                foreach (ClientPowerShellDataStructureHandler handler2 in list)
                {
                    handler2.TransportManager.PrepareForDisconnect();
                }
            }
        }

        private void PrepareForConnect()
        {
            List<ClientPowerShellDataStructureHandler> list;
            lock (this.associationSyncObject)
            {
                list = new List<ClientPowerShellDataStructureHandler>(this.associatedPowerShellDSHandlers.Values);
            }
            foreach (ClientPowerShellDataStructureHandler handler in list)
            {
                handler.TransportManager.ReadyForDisconnect -= new EventHandler<EventArgs>(this.HandleReadyForDisconnect);
                handler.TransportManager.PrepareForConnect();
            }
        }

        internal void ProcessReceivedData(RemoteDataObject<PSObject> receivedData)
        {
            if (receivedData.RunspacePoolId != this.clientRunspacePoolId)
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.RunspaceIdsDoNotMatch, new object[] { receivedData.RunspacePoolId, this.clientRunspacePoolId });
            }
            switch (receivedData.DataType)
            {
                case RemotingDataType.RunspacePoolOperationResponse:
                    this.SetMaxMinRunspacesResponseRecieved.SafeInvoke<RemoteDataEventArgs<PSObject>>(this, new RemoteDataEventArgs<PSObject>(receivedData.Data));
                    return;

                case RemotingDataType.RunspacePoolStateInfo:
                {
                    RunspacePoolStateInfo runspacePoolStateInfo = RemotingDecoder.GetRunspacePoolStateInfo(receivedData.Data);
                    this.StateInfoReceived.SafeInvoke<RemoteDataEventArgs<RunspacePoolStateInfo>>(this, new RemoteDataEventArgs<RunspacePoolStateInfo>(runspacePoolStateInfo));
                    this.NotifyAssociatedPowerShells(runspacePoolStateInfo);
                    return;
                }
                case RemotingDataType.CreatePowerShell:
                case RemotingDataType.AvailableRunspaces:
                case RemotingDataType.GetCommandMetadata:
                    break;

                case RemotingDataType.PSEventArgs:
                {
                    PSEventArgs pSEventArgs = RemotingDecoder.GetPSEventArgs(receivedData.Data);
                    this.PSEventArgsReceived.SafeInvoke<RemoteDataEventArgs<PSEventArgs>>(this, new RemoteDataEventArgs<PSEventArgs>(pSEventArgs));
                    break;
                }
                case RemotingDataType.ApplicationPrivateData:
                {
                    PSPrimitiveDictionary applicationPrivateData = RemotingDecoder.GetApplicationPrivateData(receivedData.Data);
                    this.ApplicationPrivateDataReceived.SafeInvoke<RemoteDataEventArgs<PSPrimitiveDictionary>>(this, new RemoteDataEventArgs<PSPrimitiveDictionary>(applicationPrivateData));
                    return;
                }
                case RemotingDataType.RunspacePoolInitData:
                {
                    RunspacePoolInitInfo runspacePoolInitInfo = RemotingDecoder.GetRunspacePoolInitInfo(receivedData.Data);
                    this.RSPoolInitInfoReceived.SafeInvoke<RemoteDataEventArgs<RunspacePoolInitInfo>>(this, new RemoteDataEventArgs<RunspacePoolInitInfo>(runspacePoolInitInfo));
                    return;
                }
                case RemotingDataType.RemoteHostCallUsingRunspaceHost:
                {
                    RemoteHostCall data = RemoteHostCall.Decode(receivedData.Data);
                    this.RemoteHostCallReceived.SafeInvoke<RemoteDataEventArgs<RemoteHostCall>>(this, new RemoteDataEventArgs<RemoteHostCall>(data));
                    return;
                }
                default:
                    return;
            }
        }

        internal void ReconnectPoolAsync()
        {
            this._reconnecting = true;
            this.PrepareForConnect();
            this.remoteSession.ReconnectAsync();
        }

        private void SendDataAsync(RemoteDataObject data)
        {
            this.transportManager.DataToBeSentCollection.Add<object>(data);
        }

        internal void SendDataAsync<T>(RemoteDataObject<T> data, DataPriorityType priority)
        {
            this.transportManager.DataToBeSentCollection.Add<T>(data, priority);
        }

        internal void SendDataAsync(PSObject data, DataPriorityType priority)
        {
            RemoteDataObject<PSObject> obj2 = RemoteDataObject<PSObject>.CreateFrom(RemotingDestination.InvalidDestination | RemotingDestination.Server, RemotingDataType.InvalidDataType, this.clientRunspacePoolId, Guid.Empty, data);
            this.transportManager.DataToBeSentCollection.Add<PSObject>(obj2);
        }

        internal void SendGetAvailableRunspacesToServer(long callId)
        {
            this.SendDataAsync(RemotingEncoder.GenerateGetAvailableRunspaces(this.clientRunspacePoolId, callId));
        }

        internal void SendHostResponseToServer(RemoteHostResponse hostResponse)
        {
            this.SendDataAsync(hostResponse.Encode(), DataPriorityType.PromptResponse);
        }

        internal void SendSetMaxRunspacesToServer(int maxRunspaces, long callId)
        {
            RemoteDataObject data = RemotingEncoder.GenerateSetMaxRunspaces(this.clientRunspacePoolId, maxRunspaces, callId);
            this.SendDataAsync(data);
        }

        internal void SendSetMinRunspacesToServer(int minRunspaces, long callId)
        {
            RemoteDataObject data = RemotingEncoder.GenerateSetMinRunspaces(this.clientRunspacePoolId, minRunspaces, callId);
            this.SendDataAsync(data);
        }

        private void StartDisconnectAsync(object remoteSession)
        {
            ((ClientRemoteSession) remoteSession).DisconnectAsync();
        }

        internal bool EndpointSupportsDisconnect
        {
            get
            {
                WSManClientSessionTransportManager transportManager = this.transportManager as WSManClientSessionTransportManager;
                if (transportManager == null)
                {
                    return false;
                }
                return transportManager.SupportsDisconnect;
            }
        }

        internal int MaxRetryConnectionTime
        {
            get
            {
                if ((this.transportManager != null) && (this.transportManager is WSManClientSessionTransportManager))
                {
                    return ((WSManClientSessionTransportManager) this.transportManager).MaxRetryConnectionTime;
                }
                return 0;
            }
        }

        internal ClientRemoteSession RemoteSession
        {
            get
            {
                return this.remoteSession;
            }
        }

        internal BaseClientSessionTransportManager TransportManager
        {
            get
            {
                if (this.remoteSession != null)
                {
                    return this.remoteSession.SessionDataStructureHandler.TransportManager;
                }
                return null;
            }
        }
    }
}

