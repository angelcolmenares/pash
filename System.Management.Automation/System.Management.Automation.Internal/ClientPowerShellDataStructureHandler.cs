namespace System.Management.Automation.Internal
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Remoting.Client;
    using System.Threading;

    internal class ClientPowerShellDataStructureHandler
    {
        private Exception _sessionClosedReason;
        protected Guid clientPowerShellId;
        protected Guid clientRunspacePoolId;
        private int connectionState = 1;
        private object inputSyncObject = new object();
        private BaseClientCommandTransportManager transportManager;

        internal event EventHandler<RemoteDataEventArgs<Exception>> BrokenNotificationFromRunspacePool;

        internal event EventHandler<EventArgs> CloseCompleted;

        internal event EventHandler<RemoteDataEventArgs<Exception>> ClosedNotificationFromRunspacePool;

        internal event EventHandler<RemoteDataEventArgs<Exception>> ConnectCompleted;

        internal event EventHandler<RemoteDataEventArgs<ErrorRecord>> ErrorReceived;

        internal event EventHandler<RemoteDataEventArgs<RemoteHostCall>> HostCallReceived;

        internal event EventHandler<RemoteDataEventArgs<InformationalMessage>> InformationalMessageReceived;

        internal event EventHandler<RemoteDataEventArgs<PSInvocationStateInfo>> InvocationStateInfoReceived;

        internal event EventHandler<RemoteDataEventArgs<object>> OutputReceived;

        internal event EventHandler<RemoteDataEventArgs<Exception>> ReconnectCompleted;

        internal event EventHandler RemoveAssociation;

        internal event EventHandler<ConnectionStatusEventArgs> RobustConnectionNotification;

        internal ClientPowerShellDataStructureHandler(BaseClientCommandTransportManager transportManager, Guid clientRunspacePoolId, Guid clientPowerShellId)
        {
            this.transportManager = transportManager;
            this.clientRunspacePoolId = clientRunspacePoolId;
            this.clientPowerShellId = clientPowerShellId;
            transportManager.SignalCompleted += new EventHandler<EventArgs>(this.OnSignalCompleted);
        }

        internal void CloseConnectionAsync(Exception sessionCloseReason)
        {
            this._sessionClosedReason = sessionCloseReason;
            this.transportManager.CloseCompleted += delegate (object source, EventArgs args) {
                if (this.CloseCompleted != null)
                {
                    EventArgs e = (args == EventArgs.Empty) ? new RemoteSessionStateEventArgs(new RemoteSessionStateInfo(RemoteSessionState.Closed, this._sessionClosedReason)) : args;
                    this.CloseCompleted(this, e);
                }
                this.transportManager.Dispose();
            };
            this.transportManager.CloseAsync();
        }

        internal void ConnectAsync()
        {
            Interlocked.CompareExchange(ref this.connectionState, 5, 3);
            this.SetupTransportManager(false);
            this.transportManager.ConnectAsync();
        }

        internal void HandleConnectCompleted(object sender, EventArgs args)
        {
            Interlocked.CompareExchange(ref this.connectionState, 1, 5);
            this.ConnectCompleted.SafeInvoke<RemoteDataEventArgs<Exception>>(this, new RemoteDataEventArgs<Exception>(null));
        }

        private void HandleDelayStreamRequestProcessed(object sender, EventArgs e)
        {
            this.ProcessDisconnect(null);
        }

        private void HandleInputDataReady(object sender, EventArgs e)
        {
            lock (this.inputSyncObject)
            {
                ObjectStreamBase inputstream = sender as ObjectStreamBase;
                this.WriteInput(inputstream);
            }
        }

        internal void HandleReconnectCompleted(object sender, EventArgs args)
        {
            Interlocked.CompareExchange(ref this.connectionState, 1, 4);
            this.ReconnectCompleted.SafeInvoke<RemoteDataEventArgs<Exception>>(this, new RemoteDataEventArgs<Exception>(null));
        }

        internal void HandleTransportError(object sender, TransportErrorOccuredEventArgs e)
        {
            PSInvocationStateInfo data = new PSInvocationStateInfo(PSInvocationState.Failed, e.Exception);
            this.InvocationStateInfoReceived.SafeInvoke<RemoteDataEventArgs<PSInvocationStateInfo>>(this, new RemoteDataEventArgs<PSInvocationStateInfo>(data));
        }

        private void OnSignalCompleted(object sender, EventArgs e)
        {
            PSRemotingDataStructureException reason = new PSRemotingDataStructureException(RemotingErrorIdStrings.PipelineStopped);
            this.InvocationStateInfoReceived.SafeInvoke<RemoteDataEventArgs<PSInvocationStateInfo>>(this, new RemoteDataEventArgs<PSInvocationStateInfo>(new PSInvocationStateInfo(PSInvocationState.Stopped, reason)));
        }

        internal void ProcessDisconnect(RunspacePoolStateInfo rsStateInfo)
        {
            PSInvocationStateInfo data = new PSInvocationStateInfo(PSInvocationState.Disconnected, (rsStateInfo != null) ? rsStateInfo.Reason : null);
            this.InvocationStateInfoReceived.SafeInvoke<RemoteDataEventArgs<PSInvocationStateInfo>>(this, new RemoteDataEventArgs<PSInvocationStateInfo>(data));
            Interlocked.CompareExchange(ref this.connectionState, 3, 1);
        }

        internal void ProcessReceivedData(RemoteDataObject<PSObject> receivedData)
        {
            if (receivedData.PowerShellId != this.clientPowerShellId)
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.PipelineIdsDoNotMatch, new object[] { receivedData.PowerShellId, this.clientPowerShellId });
            }
            switch (receivedData.DataType)
            {
                case RemotingDataType.PowerShellOutput:
                {
                    object powerShellOutput = RemotingDecoder.GetPowerShellOutput(receivedData.Data);
                    this.OutputReceived.SafeInvoke<RemoteDataEventArgs<object>>(this, new RemoteDataEventArgs<object>(powerShellOutput));
                    return;
                }
                case RemotingDataType.PowerShellErrorRecord:
                {
                    ErrorRecord powerShellError = RemotingDecoder.GetPowerShellError(receivedData.Data);
                    this.ErrorReceived.SafeInvoke<RemoteDataEventArgs<ErrorRecord>>(this, new RemoteDataEventArgs<ErrorRecord>(powerShellError));
                    return;
                }
                case RemotingDataType.PowerShellStateInfo:
                {
                    PSInvocationStateInfo powerShellStateInfo = RemotingDecoder.GetPowerShellStateInfo(receivedData.Data);
                    this.InvocationStateInfoReceived.SafeInvoke<RemoteDataEventArgs<PSInvocationStateInfo>>(this, new RemoteDataEventArgs<PSInvocationStateInfo>(powerShellStateInfo));
                    return;
                }
                case RemotingDataType.PowerShellDebug:
                {
                    DebugRecord powerShellDebug = RemotingDecoder.GetPowerShellDebug(receivedData.Data);
                    this.InformationalMessageReceived.SafeInvoke<RemoteDataEventArgs<InformationalMessage>>(this, new RemoteDataEventArgs<InformationalMessage>(new InformationalMessage(powerShellDebug, RemotingDataType.PowerShellDebug)));
                    return;
                }
                case RemotingDataType.PowerShellVerbose:
                {
                    VerboseRecord powerShellVerbose = RemotingDecoder.GetPowerShellVerbose(receivedData.Data);
                    this.InformationalMessageReceived.SafeInvoke<RemoteDataEventArgs<InformationalMessage>>(this, new RemoteDataEventArgs<InformationalMessage>(new InformationalMessage(powerShellVerbose, RemotingDataType.PowerShellVerbose)));
                    return;
                }
                case RemotingDataType.PowerShellWarning:
                {
                    WarningRecord powerShellWarning = RemotingDecoder.GetPowerShellWarning(receivedData.Data);
                    this.InformationalMessageReceived.SafeInvoke<RemoteDataEventArgs<InformationalMessage>>(this, new RemoteDataEventArgs<InformationalMessage>(new InformationalMessage(powerShellWarning, RemotingDataType.PowerShellWarning)));
                    return;
                }
                case ((RemotingDataType) 0x4100a):
                case ((RemotingDataType) 0x4100b):
                case ((RemotingDataType) 0x4100c):
                case ((RemotingDataType) 0x4100d):
                case ((RemotingDataType) 0x4100e):
                case ((RemotingDataType) 0x4100f):
                    break;

                case RemotingDataType.PowerShellProgress:
                {
                    ProgressRecord powerShellProgress = RemotingDecoder.GetPowerShellProgress(receivedData.Data);
                    this.InformationalMessageReceived.SafeInvoke<RemoteDataEventArgs<InformationalMessage>>(this, new RemoteDataEventArgs<InformationalMessage>(new InformationalMessage(powerShellProgress, RemotingDataType.PowerShellProgress)));
                    return;
                }
                case RemotingDataType.RemoteHostCallUsingPowerShellHost:
                {
                    RemoteHostCall data = RemoteHostCall.Decode(receivedData.Data);
                    this.HostCallReceived.SafeInvoke<RemoteDataEventArgs<RemoteHostCall>>(this, new RemoteDataEventArgs<RemoteHostCall>(data));
                    break;
                }
                default:
                    return;
            }
        }

        internal void ProcessRobustConnectionNotification(ConnectionStatusEventArgs e)
        {
            this.RobustConnectionNotification.SafeInvoke<ConnectionStatusEventArgs>(this, e);
        }

        internal void RaiseRemoveAssociationEvent()
        {
            this.RemoveAssociation.SafeInvoke(this, EventArgs.Empty);
        }

        internal void ReconnectAsync()
        {
            if (Interlocked.CompareExchange(ref this.connectionState, 4, 3) == 3)
            {
                this.transportManager.ReconnectAsync();
            }
        }

        private void SendDataAsync(RemoteDataObject data)
        {
            RemoteDataObject<object> obj2 = data;
            this.transportManager.DataToBeSentCollection.Add<object>(obj2);
        }

        internal void SendHostResponseToServer(RemoteHostResponse hostResponse)
        {
            RemoteDataObject<PSObject> data = RemoteDataObject<PSObject>.CreateFrom(RemotingDestination.InvalidDestination | RemotingDestination.Server, RemotingDataType.RemotePowerShellHostResponseData, this.clientRunspacePoolId, this.clientPowerShellId, hostResponse.Encode());
            this.transportManager.DataToBeSentCollection.Add<PSObject>(data, DataPriorityType.PromptResponse);
        }

        internal void SendInput(ObjectStreamBase inputstream)
        {
            if (!inputstream.IsOpen && (inputstream.Count == 0))
            {
                lock (this.inputSyncObject)
                {
                    this.SendDataAsync(RemotingEncoder.GeneratePowerShellInputEnd(this.clientRunspacePoolId, this.clientPowerShellId));
                    return;
                }
            }
            lock (this.inputSyncObject)
            {
                inputstream.DataReady += new EventHandler(this.HandleInputDataReady);
                this.WriteInput(inputstream);
            }
        }

        internal void SendStopPowerShellMessage()
        {
            this.transportManager.CryptoHelper.CompleteKeyExchange();
            this.transportManager.SendStopSignal();
        }

        internal void SetStateToFailed(Exception reason)
        {
            this.BrokenNotificationFromRunspacePool.SafeInvoke<RemoteDataEventArgs<Exception>>(this, new RemoteDataEventArgs<Exception>(reason));
        }

        internal void SetStateToStopped(Exception reason)
        {
            this.ClosedNotificationFromRunspacePool.SafeInvoke<RemoteDataEventArgs<Exception>>(this, new RemoteDataEventArgs<Exception>(reason));
        }

        private void SetupTransportManager(bool inDisconnectMode)
        {
            this.transportManager.WSManTransportErrorOccured += new EventHandler<TransportErrorOccuredEventArgs>(this.HandleTransportError);
            this.transportManager.ReconnectCompleted += new EventHandler<EventArgs>(this.HandleReconnectCompleted);
            this.transportManager.ConnectCompleted += new EventHandler<EventArgs>(this.HandleConnectCompleted);
            this.transportManager.DelayStreamRequestProcessed += new EventHandler<EventArgs>(this.HandleDelayStreamRequestProcessed);
            this.transportManager.startInDisconnectedMode = inDisconnectMode;
        }

        internal void Start(ClientRemoteSessionDSHandlerStateMachine stateMachine, bool inDisconnectMode)
        {
            this.SetupTransportManager(inDisconnectMode);
            this.transportManager.CreateAsync();
        }

        private void WriteInput(ObjectStreamBase inputstream)
        {
            foreach (object obj2 in inputstream.ObjectReader.NonBlockingRead(0x7fffffff))
            {
                this.SendDataAsync(RemotingEncoder.GeneratePowerShellInput(obj2, this.clientRunspacePoolId, this.clientPowerShellId));
            }
            if (!inputstream.IsOpen)
            {
                foreach (object obj3 in inputstream.ObjectReader.NonBlockingRead(0x7fffffff))
                {
                    this.SendDataAsync(RemotingEncoder.GeneratePowerShellInput(obj3, this.clientRunspacePoolId, this.clientPowerShellId));
                }
                inputstream.DataReady -= new EventHandler(this.HandleInputDataReady);
                this.SendDataAsync(RemotingEncoder.GeneratePowerShellInputEnd(this.clientRunspacePoolId, this.clientPowerShellId));
            }
        }

        internal Guid PowerShellId
        {
            get
            {
                return this.clientPowerShellId;
            }
        }

        internal BaseClientCommandTransportManager TransportManager
        {
            get
            {
                return this.transportManager;
            }
        }

        private enum connectionStates
        {
            Connected = 1,
            Connecting = 5,
            Disconnected = 3,
            Reconnecting = 4
        }
    }
}

