namespace System.Management.Automation.Remoting
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Remoting.Server;

    internal class ServerMethodExecutor
    {
        private Guid _clientPowerShellId;
        private Guid _clientRunspacePoolId;
        private RemotingDataType _remoteHostCallDataType;
        private ServerDispatchTable _serverDispatchTable;
        private AbstractServerTransportManager _transportManager;
        private const long DefaultClientPipelineId = -1L;

        internal ServerMethodExecutor(Guid clientRunspacePoolId, Guid clientPowerShellId, AbstractServerTransportManager transportManager)
        {
            this._clientRunspacePoolId = clientRunspacePoolId;
            this._clientPowerShellId = clientPowerShellId;
            this._transportManager = transportManager;
            this._remoteHostCallDataType = (clientPowerShellId == Guid.Empty) ? RemotingDataType.RemoteHostCallUsingRunspaceHost : RemotingDataType.RemoteHostCallUsingPowerShellHost;
            this._serverDispatchTable = new ServerDispatchTable();
        }

        internal void AbortAllCalls()
        {
            this._serverDispatchTable.AbortAllCalls();
        }

        internal T ExecuteMethod<T>(RemoteHostMethodId methodId)
        {
            return this.ExecuteMethod<T>(methodId, new object[0]);
        }

        internal T ExecuteMethod<T>(RemoteHostMethodId methodId, object[] parameters)
        {
            long callId = this._serverDispatchTable.CreateNewCallId();
            RemoteDataObject<PSObject> data = RemoteDataObject<PSObject>.CreateFrom(RemotingDestination.InvalidDestination | RemotingDestination.Client, this._remoteHostCallDataType, this._clientRunspacePoolId, this._clientPowerShellId, new RemoteHostCall(callId, methodId, parameters).Encode());
            this._transportManager.SendDataToClient<PSObject>(data, false, true);
            RemoteHostResponse response = this._serverDispatchTable.GetResponse(callId, null);
            if (response == null)
            {
                throw RemoteHostExceptions.NewRemoteHostCallFailedException(methodId);
            }
            response.SimulateExecution();
            return (T) response.SimulateExecution();
        }

        internal void ExecuteVoidMethod(RemoteHostMethodId methodId)
        {
            this.ExecuteVoidMethod(methodId, new object[0]);
        }

        internal void ExecuteVoidMethod(RemoteHostMethodId methodId, object[] parameters)
        {
            long callId = -100L;
            RemoteDataObject<PSObject> data = RemoteDataObject<PSObject>.CreateFrom(RemotingDestination.InvalidDestination | RemotingDestination.Client, this._remoteHostCallDataType, this._clientRunspacePoolId, this._clientPowerShellId, new RemoteHostCall(callId, methodId, parameters).Encode());
            this._transportManager.SendDataToClient<PSObject>(data, false, false);
        }

        internal void HandleRemoteHostResponseFromClient(RemoteHostResponse remoteHostResponse)
        {
            this._serverDispatchTable.SetResponse(remoteHostResponse.CallId, remoteHostResponse);
        }
    }
}

