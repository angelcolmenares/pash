namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Remoting.Server;
    using System.Management.Automation.Runspaces;
    using System.Threading;

    internal class ServerPowerShellDataStructureHandler
    {
        private Guid clientPowerShellId;
        private Guid clientRunspacePoolId;
        private Runspace rsUsedToInvokePowerShell;
        private RemoteStreamOptions streamSerializationOptions;
        private AbstractServerTransportManager transportManager;

        internal event EventHandler<RemoteDataEventArgs<RemoteHostResponse>> HostResponseReceived;

        internal event EventHandler InputEndReceived;

        internal event EventHandler<RemoteDataEventArgs<object>> InputReceived;

        internal event EventHandler OnSessionConnected;

        internal event EventHandler RemoveAssociation;

        internal event EventHandler StopPowerShellReceived;

        internal ServerPowerShellDataStructureHandler(Guid instanceId, Guid runspacePoolId, RemoteStreamOptions remoteStreamOptions, AbstractServerTransportManager transportManager, PowerShell localPowerShell)
        {
            this.clientPowerShellId = instanceId;
            this.clientRunspacePoolId = runspacePoolId;
            this.transportManager = transportManager;
            this.streamSerializationOptions = remoteStreamOptions;
            transportManager.Closing += new EventHandler(this.HandleTransportClosing);
            if (localPowerShell != null)
            {
                localPowerShell.RunspaceAssigned += new EventHandler<PSEventArgs<Runspace>>(this.LocalPowerShell_RunspaceAssigned);
            }
        }

        internal ServerRemoteHost GetHostAssociatedWithPowerShell(HostInfo powerShellHostInfo, ServerRemoteHost runspaceServerRemoteHost)
        {
            HostInfo hostInfo;
            if (powerShellHostInfo.UseRunspaceHost)
            {
                hostInfo = runspaceServerRemoteHost.HostInfo;
            }
            else
            {
                hostInfo = powerShellHostInfo;
            }
            return new ServerRemoteHost(this.clientRunspacePoolId, this.clientPowerShellId, hostInfo, this.transportManager);
        }

        private void HandleTransportClosing(object sender, EventArgs args)
        {
            this.StopPowerShellReceived.SafeInvoke(this, args);
        }

        private void LocalPowerShell_RunspaceAssigned(object sender, PSEventArgs<Runspace> e)
        {
            this.rsUsedToInvokePowerShell = e.Args;
        }

        internal void Prepare()
        {
            if (this.clientPowerShellId != Guid.Empty)
            {
                this.transportManager.Prepare();
            }
        }

        internal void ProcessConnect()
        {
            this.OnSessionConnected.SafeInvoke(this, EventArgs.Empty);
        }

        internal void ProcessReceivedData(RemoteDataObject<PSObject> receivedData)
        {
            if (receivedData == null)
            {
                throw PSTraceSource.NewArgumentNullException("receivedData");
            }
            switch (receivedData.DataType)
            {
                case RemotingDataType.PowerShellInput:
                    this.InputReceived.SafeInvoke<RemoteDataEventArgs<object>>(this, new RemoteDataEventArgs<object>(receivedData.Data));
                    return;

                case RemotingDataType.PowerShellInputEnd:
                    this.InputEndReceived.SafeInvoke(this, EventArgs.Empty);
                    return;

                case RemotingDataType.StopPowerShell:
                    this.StopPowerShellReceived.SafeInvoke(this, EventArgs.Empty);
                    return;

                case RemotingDataType.RemotePowerShellHostResponseData:
                {
                    RemoteHostResponse data = RemoteHostResponse.Decode(receivedData.Data);
                    this.transportManager.ReportExecutionStatusAsRunning();
                    this.HostResponseReceived.SafeInvoke<RemoteDataEventArgs<RemoteHostResponse>>(this, new RemoteDataEventArgs<RemoteHostResponse>(data));
                    return;
                }
            }
        }

        internal void RaiseRemoveAssociationEvent()
        {
            this.RemoveAssociation.SafeInvoke(this, EventArgs.Empty);
        }

        private void SendDataAsync(RemoteDataObject data)
        {
            this.transportManager.SendDataToClient(data, false, false);
        }

        internal void SendDebugRecordToClient(DebugRecord record)
        {
            record.SerializeExtendedInfo = (this.streamSerializationOptions & RemoteStreamOptions.AddInvocationInfoToDebugRecord) != 0;
            this.SendDataAsync(RemotingEncoder.GeneratePowerShellInformational(record, this.clientRunspacePoolId, this.clientPowerShellId, RemotingDataType.PowerShellDebug));
        }

        internal void SendErrorRecordToClient(ErrorRecord errorRecord)
        {
            errorRecord.SerializeExtendedInfo = (this.streamSerializationOptions & RemoteStreamOptions.AddInvocationInfoToErrorRecord) != 0;
            this.SendDataAsync(RemotingEncoder.GeneratePowerShellError(errorRecord, this.clientRunspacePoolId, this.clientPowerShellId));
        }

        internal void SendOutputDataToClient(PSObject data)
        {
            this.SendDataAsync(RemotingEncoder.GeneratePowerShellOutput(data, this.clientPowerShellId, this.clientRunspacePoolId));
        }

        internal void SendProgressRecordToClient(ProgressRecord record)
        {
            this.SendDataAsync(RemotingEncoder.GeneratePowerShellInformational(record, this.clientRunspacePoolId, this.clientPowerShellId));
        }

        internal void SendStateChangedInformationToClient(PSInvocationStateInfo stateInfo)
        {
            this.SendDataAsync(RemotingEncoder.GeneratePowerShellStateInfo(stateInfo, this.clientPowerShellId, this.clientRunspacePoolId));
            if (this.clientPowerShellId != Guid.Empty)
            {
                this.transportManager.Closing -= new EventHandler(this.HandleTransportClosing);
                this.transportManager.Close(null);
            }
        }

        internal void SendVerboseRecordToClient(VerboseRecord record)
        {
            record.SerializeExtendedInfo = (this.streamSerializationOptions & RemoteStreamOptions.AddInvocationInfoToVerboseRecord) != 0;
            this.SendDataAsync(RemotingEncoder.GeneratePowerShellInformational(record, this.clientRunspacePoolId, this.clientPowerShellId, RemotingDataType.PowerShellVerbose));
        }

        internal void SendWarningRecordToClient(WarningRecord record)
        {
            record.SerializeExtendedInfo = (this.streamSerializationOptions & RemoteStreamOptions.AddInvocationInfoToWarningRecord) != 0;
            this.SendDataAsync(RemotingEncoder.GeneratePowerShellInformational(record, this.clientRunspacePoolId, this.clientPowerShellId, RemotingDataType.PowerShellWarning));
        }

        internal Guid PowerShellId
        {
            get
            {
                return this.clientPowerShellId;
            }
        }

        internal Runspace RunspaceUsedToInvokePowerShell
        {
            get
            {
                return this.rsUsedToInvokePowerShell;
            }
        }
    }
}

