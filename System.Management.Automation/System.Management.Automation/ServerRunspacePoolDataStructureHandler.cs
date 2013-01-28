namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Remoting.Server;
    using System.Management.Automation.Runspaces;
    using System.Threading;

    internal class ServerRunspacePoolDataStructureHandler
    {
        private Dictionary<Guid, ServerPowerShellDataStructureHandler> associatedShells = new Dictionary<Guid, ServerPowerShellDataStructureHandler>();
        private object associationSyncObject = new object();
        private Guid clientRunspacePoolId;
        private AbstractServerSessionTransportManager transportManager;

        internal event EventHandler<RemoteDataEventArgs<RemoteDataObject<PSObject>>> CreateAndInvokePowerShell;

        internal event EventHandler<RemoteDataEventArgs<PSObject>> GetAvailableRunspacesReceived;

        internal event EventHandler<RemoteDataEventArgs<RemoteDataObject<PSObject>>> GetCommandMetadata;

        internal event EventHandler<RemoteDataEventArgs<RemoteHostResponse>> HostResponseReceived;

        internal event EventHandler<RemoteDataEventArgs<PSObject>> SetMaxRunspacesReceived;

        internal event EventHandler<RemoteDataEventArgs<PSObject>> SetMinRunspacesReceived;

        internal ServerRunspacePoolDataStructureHandler(ServerRunspacePoolDriver driver, AbstractServerSessionTransportManager transportManager)
        {
            this.clientRunspacePoolId = driver.InstanceId;
            this.transportManager = transportManager;
        }

        internal ServerPowerShellDataStructureHandler CreatePowerShellDataStructureHandler(Guid instanceId, Guid runspacePoolId, RemoteStreamOptions remoteStreamOptions, PowerShell localPowerShell)
        {
            AbstractServerTransportManager transportManager = this.transportManager;
            if (instanceId != Guid.Empty)
            {
                transportManager = this.transportManager.GetCommandTransportManager(instanceId);
            }
            ServerPowerShellDataStructureHandler handler = new ServerPowerShellDataStructureHandler(instanceId, runspacePoolId, remoteStreamOptions, transportManager, localPowerShell);
            lock (this.associationSyncObject)
            {
                this.associatedShells.Add(handler.PowerShellId, handler);
            }
            handler.RemoveAssociation += new EventHandler(this.HandleRemoveAssociation);
            return handler;
        }

        internal void DispatchMessageToPowerShell(RemoteDataObject<PSObject> rcvdData)
        {
            ServerPowerShellDataStructureHandler associatedPowerShellDataStructureHandler = this.GetAssociatedPowerShellDataStructureHandler(rcvdData.PowerShellId);
            if (associatedPowerShellDataStructureHandler != null)
            {
                associatedPowerShellDataStructureHandler.ProcessReceivedData(rcvdData);
            }
        }

        internal ServerPowerShellDataStructureHandler GetAssociatedPowerShellDataStructureHandler(Guid clientPowerShellId)
        {
            ServerPowerShellDataStructureHandler handler = null;
            lock (this.associationSyncObject)
            {
                if (!this.associatedShells.TryGetValue(clientPowerShellId, out handler))
                {
                    handler = null;
                }
            }
            return handler;
        }

        internal ServerPowerShellDataStructureHandler GetPowerShellDataStructureHandler()
        {
            lock (this.associationSyncObject)
            {
                if (this.associatedShells.Count > 0)
                {
                    foreach (object obj2 in this.associatedShells.Values)
                    {
                        ServerPowerShellDataStructureHandler handler = obj2 as ServerPowerShellDataStructureHandler;
                        if (handler != null)
                        {
                            return handler;
                        }
                    }
                }
            }
            return null;
        }

        private void HandleRemoveAssociation(object sender, EventArgs e)
        {
            ServerPowerShellDataStructureHandler handler = sender as ServerPowerShellDataStructureHandler;
            lock (this.associationSyncObject)
            {
                this.associatedShells.Remove(handler.PowerShellId);
            }
            this.transportManager.RemoveCommandTransportManager(handler.PowerShellId);
        }

        internal void ProcessConnect()
        {
            List<ServerPowerShellDataStructureHandler> list;
            lock (this.associationSyncObject)
            {
                list = new List<ServerPowerShellDataStructureHandler>(this.associatedShells.Values);
            }
            foreach (ServerPowerShellDataStructureHandler handler in list)
            {
                handler.ProcessConnect();
            }
        }

        internal void ProcessReceivedData(RemoteDataObject<PSObject> receivedData)
        {
            if (receivedData == null)
            {
                throw PSTraceSource.NewArgumentNullException("receivedData");
            }
            switch (receivedData.DataType)
            {
                case RemotingDataType.SetMaxRunspaces:
                    this.SetMaxRunspacesReceived.SafeInvoke<RemoteDataEventArgs<PSObject>>(this, new RemoteDataEventArgs<PSObject>(receivedData.Data));
                    return;

                case RemotingDataType.SetMinRunspaces:
                    this.SetMinRunspacesReceived.SafeInvoke<RemoteDataEventArgs<PSObject>>(this, new RemoteDataEventArgs<PSObject>(receivedData.Data));
                    return;

                case RemotingDataType.RunspacePoolOperationResponse:
                case RemotingDataType.RunspacePoolStateInfo:
                case RemotingDataType.PSEventArgs:
                case RemotingDataType.ApplicationPrivateData:
                    break;

                case RemotingDataType.CreatePowerShell:
                    this.CreateAndInvokePowerShell.SafeInvoke<RemoteDataEventArgs<RemoteDataObject<PSObject>>>(this, new RemoteDataEventArgs<RemoteDataObject<PSObject>>(receivedData));
                    return;

                case RemotingDataType.AvailableRunspaces:
                    this.GetAvailableRunspacesReceived.SafeInvoke<RemoteDataEventArgs<PSObject>>(this, new RemoteDataEventArgs<PSObject>(receivedData.Data));
                    break;

                case RemotingDataType.GetCommandMetadata:
                    this.GetCommandMetadata.SafeInvoke<RemoteDataEventArgs<RemoteDataObject<PSObject>>>(this, new RemoteDataEventArgs<RemoteDataObject<PSObject>>(receivedData));
                    return;

                case RemotingDataType.RemoteRunspaceHostResponseData:
                {
                    RemoteHostResponse data = RemoteHostResponse.Decode(receivedData.Data);
                    this.transportManager.ReportExecutionStatusAsRunning();
                    this.HostResponseReceived.SafeInvoke<RemoteDataEventArgs<RemoteHostResponse>>(this, new RemoteDataEventArgs<RemoteHostResponse>(data));
                    return;
                }
                default:
                    return;
            }
        }

        internal void SendApplicationPrivateDataToClient(PSPrimitiveDictionary applicationPrivateData, RemoteSessionCapability serverCapability)
        {
            PSPrimitiveDictionary dictionary = PSPrimitiveDictionary.CloneAndAddPSVersionTable(applicationPrivateData);
            PSPrimitiveDictionary dictionary2 = (PSPrimitiveDictionary) dictionary["PSVersionTable"];
            dictionary2["PSRemotingProtocolVersion"] = serverCapability.ProtocolVersion;
            dictionary2["SerializationVersion"] = serverCapability.SerializationVersion;
            dictionary2["PSVersion"] = serverCapability.PSVersion;
            RemoteDataObject data = RemotingEncoder.GenerateApplicationPrivateData(this.clientRunspacePoolId, dictionary);
            this.SendDataAsync(data);
        }

        private void SendDataAsync(RemoteDataObject data)
        {
            this.transportManager.SendDataToClient(data, true, false);
        }

        internal void SendPSEventArgsToClient(PSEventArgs e)
        {
            RemoteDataObject data = RemotingEncoder.GeneratePSEventArgs(this.clientRunspacePoolId, e);
            this.SendDataAsync(data);
        }

        internal void SendResponseToClient(long callId, object response)
        {
            RemoteDataObject data = RemotingEncoder.GenerateRunspacePoolOperationResponse(this.clientRunspacePoolId, response, callId);
            this.SendDataAsync(data);
        }

        internal void SendStateInfoToClient(RunspacePoolStateInfo stateInfo)
        {
            RemoteDataObject data = RemotingEncoder.GenerateRunspacePoolStateInfo(this.clientRunspacePoolId, stateInfo);
            this.SendDataAsync(data);
        }

        internal System.Management.Automation.Runspaces.TypeTable TypeTable
        {
            get
            {
                return this.transportManager.TypeTable;
            }
            set
            {
                this.transportManager.TypeTable = value;
            }
        }
    }
}

