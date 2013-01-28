namespace System.Management.Automation.Remoting.Client
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Runspaces.Internal;
    using System.Management.Automation.Tracing;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class WSManClientCommandTransportManager : BaseClientCommandTransportManager
    {
        private readonly WSManClientSessionTransportManager _sessnTm;
        private SendDataChunk chunkToSend;
        private WSManNativeApi.WSManShellAsync closeCmdCompleted;
        private static WSManNativeApi.WSManShellAsyncCallback cmdCloseCallback;
        private static WSManNativeApi.WSManShellAsyncCallback cmdConnectCallback;
        private long cmdContextId;
        private static WSManNativeApi.WSManShellAsyncCallback cmdCreateCallback;
        private string cmdLine;
        private static WSManNativeApi.WSManShellAsyncCallback cmdReceiveCallback;
        private static WSManNativeApi.WSManShellAsyncCallback cmdReconnectCallback;
        private static WSManNativeApi.WSManShellAsyncCallback cmdSendCallback;
        private static WSManNativeApi.WSManShellAsyncCallback cmdSignalCallback;
        private IntPtr cmdSignalOperationHandle;
        private static Dictionary<long, WSManClientCommandTransportManager> CmdTMHandles = new Dictionary<long, WSManClientCommandTransportManager>();
        private static long CmdTMSeed;
        private static Delegate commandCodeSendRedirect = null;
        private static Delegate commandSendRedirect = null;
        private WSManNativeApi.WSManShellAsync connectCmdCompleted;
        private WSManNativeApi.WSManShellAsync createCmdCompleted;
        private GCHandle createCmdCompletedGCHandle;
        private bool isCreateCallbackReceived;
        private bool isDisconnectedOnInvoke;
        private bool isDisconnectPending;
        private bool isSendingInput;
        private bool isStopSignalPending;
        private System.Management.Automation.Remoting.PrioritySendDataCollection.OnDataAvailableCallback onDataAvailableToSendCallback;
        private WSManNativeApi.WSManShellAsync receivedFromRemote;
        private WSManNativeApi.WSManShellAsync reconnectCmdCompleted;
        private WSManNativeApi.WSManShellAsync sendToRemoteCompleted;
        private bool shouldStartReceivingData;
        private WSManNativeApi.WSManShellAsync signalCmdCompleted;
        internal const string StopSignal = "powershell/signal/crtl_c";
        private IntPtr wsManCmdOperationHandle;
        private IntPtr wsManRecieveOperationHandle;
        private IntPtr wsManSendOperationHandle;
        private IntPtr wsManShellOperationHandle;

        static WSManClientCommandTransportManager()
        {
            WSManNativeApi.WSManShellCompletionFunction callback = new WSManNativeApi.WSManShellCompletionFunction(WSManClientCommandTransportManager.OnCreateCmdCompleted);
            cmdCreateCallback = new WSManNativeApi.WSManShellAsyncCallback(callback);
            WSManNativeApi.WSManShellCompletionFunction function2 = new WSManNativeApi.WSManShellCompletionFunction(WSManClientCommandTransportManager.OnCloseCmdCompleted);
            cmdCloseCallback = new WSManNativeApi.WSManShellAsyncCallback(function2);
            WSManNativeApi.WSManShellCompletionFunction function3 = new WSManNativeApi.WSManShellCompletionFunction(WSManClientCommandTransportManager.OnRemoteCmdDataReceived);
            cmdReceiveCallback = new WSManNativeApi.WSManShellAsyncCallback(function3);
            WSManNativeApi.WSManShellCompletionFunction function4 = new WSManNativeApi.WSManShellCompletionFunction(WSManClientCommandTransportManager.OnRemoteCmdSendCompleted);
            cmdSendCallback = new WSManNativeApi.WSManShellAsyncCallback(function4);
            WSManNativeApi.WSManShellCompletionFunction function5 = new WSManNativeApi.WSManShellCompletionFunction(WSManClientCommandTransportManager.OnRemoteCmdSignalCompleted);
            cmdSignalCallback = new WSManNativeApi.WSManShellAsyncCallback(function5);
            WSManNativeApi.WSManShellCompletionFunction function6 = new WSManNativeApi.WSManShellCompletionFunction(WSManClientCommandTransportManager.OnReconnectCmdCompleted);
            cmdReconnectCallback = new WSManNativeApi.WSManShellAsyncCallback(function6);
            WSManNativeApi.WSManShellCompletionFunction function7 = new WSManNativeApi.WSManShellCompletionFunction(WSManClientCommandTransportManager.OnConnectCmdCompleted);
            cmdConnectCallback = new WSManNativeApi.WSManShellAsyncCallback(function7);
        }

        internal WSManClientCommandTransportManager(WSManConnectionInfo connectionInfo, IntPtr wsManShellOperationHandle, ClientRemotePowerShell shell, bool noInput, WSManClientSessionTransportManager sessnTM) : base(shell, sessnTM.CryptoHelper, sessnTM)
        {
            this.wsManShellOperationHandle = wsManShellOperationHandle;
            base.ReceivedDataCollection.MaximumReceivedDataSize = connectionInfo.MaximumReceivedDataSizePerCommand;
            base.ReceivedDataCollection.MaximumReceivedObjectSize = connectionInfo.MaximumReceivedObjectSize;
            this.cmdLine = shell.PowerShell.Commands.Commands.GetCommandStringForHistory();
            this.onDataAvailableToSendCallback = new System.Management.Automation.Remoting.PrioritySendDataCollection.OnDataAvailableCallback(this.OnDataAvailableCallback);
            this._sessnTm = sessnTM;
            sessnTM.RobustConnectionsInitiated += new EventHandler<EventArgs>(this.HandleRobustConnectionsIntiated);
            sessnTM.RobustConnectionsCompleted += new EventHandler<EventArgs>(this.HandleRobusConnectionsCompleted);
        }

        private static void AddCmdTransportManager(long cmdTMId, WSManClientCommandTransportManager cmdTransportManager)
        {
            lock (CmdTMHandles)
            {
                CmdTMHandles.Add(cmdTMId, cmdTransportManager);
            }
        }

        internal void ClearReceiveOrSendResources(int flags, bool shouldClearSend)
        {
            if (shouldClearSend)
            {
                if (this.sendToRemoteCompleted != null)
                {
                    this.sendToRemoteCompleted.Dispose();
                    this.sendToRemoteCompleted = null;
                }
                if (IntPtr.Zero != this.wsManSendOperationHandle)
                {
                    WSManNativeApi.WSManCloseOperation(this.wsManSendOperationHandle, 0);
                    this.wsManSendOperationHandle = IntPtr.Zero;
                }
            }
            else if (flags == 1)
            {
                if (IntPtr.Zero != this.wsManRecieveOperationHandle)
                {
                    WSManNativeApi.WSManCloseOperation(this.wsManRecieveOperationHandle, 0);
                    this.wsManRecieveOperationHandle = IntPtr.Zero;
                }
                if (this.receivedFromRemote != null)
                {
                    this.receivedFromRemote.Dispose();
                    this.receivedFromRemote = null;
                }
            }
        }

        internal override void CloseAsync()
        {
            BaseClientTransportManager.tracer.WriteLine("Closing command with command context: {0} Operation Context {1}", new object[] { this.cmdContextId, this.wsManCmdOperationHandle });
            bool flag = false;
            lock (base.syncObject)
            {
                if (base.isClosed)
                {
                    return;
                }
                base.isClosed = true;
                if (IntPtr.Zero == this.wsManCmdOperationHandle)
                {
                    flag = true;
                }
            }
            base.CloseAsync();
            if (!flag)
            {
                PSEtwLog.LogAnalyticInformational(PSEventId.WSManCloseCommand, PSOpcode.Disconnect, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { base.RunspacePoolInstanceId.ToString(), this.powershellInstanceId.ToString() });
                this.closeCmdCompleted = new WSManNativeApi.WSManShellAsync(new IntPtr(this.cmdContextId), cmdCloseCallback);
                WSManNativeApi.WSManCloseCommand(this.wsManCmdOperationHandle, 0, (IntPtr) this.closeCmdCompleted);
            }
            else
            {
                try
                {
                    base.RaiseCloseCompleted();
                }
                finally
                {
                    RemoveCmdTransportManager(this.cmdContextId);
                }
            }
        }

        internal override void ConnectAsync()
        {
            base.ReceivedDataCollection.PrepareForStreamConnect();
            base.serializedPipeline.Read();
            this.cmdContextId = GetNextCmdTMHandleId();
            AddCmdTransportManager(this.cmdContextId, this);
            this.connectCmdCompleted = new WSManNativeApi.WSManShellAsync(new IntPtr(this.cmdContextId), cmdConnectCallback);
            this.reconnectCmdCompleted = new WSManNativeApi.WSManShellAsync(new IntPtr(this.cmdContextId), cmdReconnectCallback);
            lock (base.syncObject)
            {
                if (base.isClosed)
                {
                    return;
                }
                WSManNativeApi.WSManConnectShellCommandEx(this.wsManShellOperationHandle, 0, base.PowershellInstanceId.ToString().ToUpper(CultureInfo.InvariantCulture), IntPtr.Zero, IntPtr.Zero, (IntPtr) this.connectCmdCompleted, ref this.wsManCmdOperationHandle);
            }
            if (this.wsManCmdOperationHandle == IntPtr.Zero)
            {
                PSRemotingTransportException e = new PSRemotingTransportException(RemotingErrorIdStrings.RunShellCommandExFailed);
                TransportErrorOccuredEventArgs eventArgs = new TransportErrorOccuredEventArgs(e, TransportMethodEnum.ConnectShellCommandEx);
                this.ProcessWSManTransportError(eventArgs);
            }
        }

        internal override void CreateAsync()
        {
            byte[] firstArgument = base.serializedPipeline.ReadOrRegisterCallback(null);
			if (firstArgument == null) firstArgument = new byte[0];
            bool flag = true;
            if (commandCodeSendRedirect != null)
            {
                object[] objArray2 = new object[2];
                objArray2[1] = firstArgument;
                object[] objArray = objArray2;
                flag = (bool) commandCodeSendRedirect.DynamicInvoke(objArray);
                firstArgument = (byte[]) objArray[0];
            }
            if (!flag)
            {
                return;
            }
            WSManNativeApi.WSManCommandArgSet set = new WSManNativeApi.WSManCommandArgSet(firstArgument);
            this.cmdContextId = GetNextCmdTMHandleId();
            AddCmdTransportManager(this.cmdContextId, this);
            PSEtwLog.LogAnalyticInformational(PSEventId.WSManCreateCommand, PSOpcode.Connect, PSTask.CreateRunspace, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { base.RunspacePoolInstanceId.ToString(), this.powershellInstanceId.ToString() });
            this.createCmdCompleted = new WSManNativeApi.WSManShellAsync(new IntPtr(this.cmdContextId), cmdCreateCallback);
            this.createCmdCompletedGCHandle = GCHandle.Alloc(this.createCmdCompleted);
            this.reconnectCmdCompleted = new WSManNativeApi.WSManShellAsync(new IntPtr(this.cmdContextId), cmdReconnectCallback);
            using (set)
            {
                lock (base.syncObject)
                {
                    if (!base.isClosed)
                    {
                        WSManNativeApi.WSManRunShellCommandEx(this.wsManShellOperationHandle, 0, base.PowershellInstanceId.ToString().ToUpper(CultureInfo.InvariantCulture), ((this.cmdLine == null) || (this.cmdLine.Length == 0)) ? " " : ((this.cmdLine.Length <= 0x100) ? this.cmdLine : this.cmdLine.Substring(0, 0xff)), (IntPtr) set, IntPtr.Zero, (IntPtr) this.createCmdCompleted, ref this.wsManCmdOperationHandle);
                        BaseClientTransportManager.tracer.WriteLine("Started cmd with command context : {0} Operation context: {1}", new object[] { this.cmdContextId, this.wsManCmdOperationHandle });
                    }
                }
            }
            if (this.wsManCmdOperationHandle == IntPtr.Zero)
            {
                PSRemotingTransportException e = new PSRemotingTransportException(RemotingErrorIdStrings.RunShellCommandExFailed);
                TransportErrorOccuredEventArgs eventArgs = new TransportErrorOccuredEventArgs(e, TransportMethodEnum.RunShellCommandEx);
                this.ProcessWSManTransportError(eventArgs);
            }
        }

        internal override void Dispose (bool isDisposing)
		{
			BaseClientTransportManager.tracer.WriteLine ("Disposing command with command context: {0} Operation Context: {1}", new object[] {
				this.cmdContextId,
				this.wsManCmdOperationHandle
			});
			base.Dispose (isDisposing);
			RemoveCmdTransportManager (this.cmdContextId);
			if (this._sessnTm != null) {
				this._sessnTm.RobustConnectionsInitiated -= new EventHandler<EventArgs> (this.HandleRobustConnectionsIntiated);
				this._sessnTm.RobustConnectionsCompleted -= new EventHandler<EventArgs> (this.HandleRobusConnectionsCompleted);
			}
            if (this.closeCmdCompleted != null)
            {
                this.closeCmdCompleted.Dispose();
                this.closeCmdCompleted = null;
            }
            if (this.reconnectCmdCompleted != null)
            {
                this.reconnectCmdCompleted.Dispose();
                this.reconnectCmdCompleted = null;
            }
            this.wsManCmdOperationHandle = IntPtr.Zero;
        }

        private static long GetNextCmdTMHandleId()
        {
            return Interlocked.Increment(ref CmdTMSeed);
        }

        private void HandleRobusConnectionsCompleted(object sender, EventArgs e)
        {
            base.ResumeQueue();
        }

        private void HandleRobustConnectionsIntiated(object sender, EventArgs e)
        {
            base.SuspendQueue();
        }

        private static void OnCloseCmdCompleted(IntPtr operationContext, int flags, IntPtr error, IntPtr shellOperationHandle, IntPtr commandOperationHandle, IntPtr operationHandle, IntPtr data)
        {
            BaseClientTransportManager.tracer.WriteLine("OnCloseCmdCompleted callback received for operation context {0}", new object[] { commandOperationHandle });
            long cmdTMId = 0L;
            WSManClientCommandTransportManager cmdTransportManager = null;
            if (!TryGetCmdTransportManager(operationContext, out cmdTransportManager, out cmdTMId))
            {
                BaseClientTransportManager.tracer.WriteLine("OnCloseCmdCompleted: Unable to find a transport manager for the command context {0}.", new object[] { cmdTMId });
            }
            else
            {
                BaseClientTransportManager.tracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Close completed callback received for command: {0}", new object[] { cmdTransportManager.cmdContextId }), new object[0]);
                PSEtwLog.LogAnalyticInformational(PSEventId.WSManCloseCommandCallbackReceived, PSOpcode.Disconnect, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { cmdTransportManager.RunspacePoolInstanceId.ToString(), cmdTransportManager.powershellInstanceId.ToString() });
                if (cmdTransportManager.isDisconnectPending)
                {
                    cmdTransportManager.RaiseReadyForDisconnect();
                }
                cmdTransportManager.RaiseCloseCompleted();
            }
        }

        private static void OnConnectCmdCompleted(IntPtr operationContext, int flags, IntPtr error, IntPtr shellOperationHandle, IntPtr commandOperationHandle, IntPtr operationHandle, IntPtr data)
        {
            BaseClientTransportManager.tracer.WriteLine("OnConnectCmdCompleted callback received", new object[0]);
            long cmdTMId = 0L;
            WSManClientCommandTransportManager cmdTransportManager = null;
            if (!TryGetCmdTransportManager(operationContext, out cmdTransportManager, out cmdTMId))
            {
                BaseClientTransportManager.tracer.WriteLine("OnConnectCmdCompleted: Unable to find a transport manager for the command context {0}.", new object[] { cmdTMId });
            }
            else
            {
                if (cmdTransportManager.connectCmdCompleted != null)
                {
                    cmdTransportManager.connectCmdCompleted.Dispose();
                    cmdTransportManager.connectCmdCompleted = null;
                }
                cmdTransportManager.wsManCmdOperationHandle = commandOperationHandle;
                if (IntPtr.Zero != error)
                {
                    WSManNativeApi.WSManError errorStruct = WSManNativeApi.WSManError.UnMarshal(error);
                    if (errorStruct.errorCode != 0)
                    {
                        BaseClientTransportManager.tracer.WriteLine("OnConnectCmdCompleted callback: WSMan reported an error: {0}", new object[] { errorStruct.errorDetail });
                        TransportErrorOccuredEventArgs eventArgs = WSManTransportManagerUtils.ConstructTransportErrorEventArgs(WSManClientSessionTransportManager.wsManApiStaticData.WSManAPIHandle, null, errorStruct, TransportMethodEnum.ReconnectShellCommandEx, RemotingErrorIdStrings.ReconnectShellCommandExCallBackError, new object[] { WSManTransportManagerUtils.ParseEscapeWSManErrorMessage(errorStruct.errorDetail) });
                        cmdTransportManager.ProcessWSManTransportError(eventArgs);
                        return;
                    }
                }
                lock (cmdTransportManager.syncObject)
                {
                    if (cmdTransportManager.isClosed)
                    {
                        BaseClientTransportManager.tracer.WriteLine("Client Session TM: Transport manager is closed. So returning", new object[0]);
                        if (cmdTransportManager.isDisconnectPending)
                        {
                            cmdTransportManager.RaiseReadyForDisconnect();
                        }
                        return;
                    }
                    if (cmdTransportManager.isDisconnectPending)
                    {
                        cmdTransportManager.RaiseReadyForDisconnect();
                        return;
                    }
                    cmdTransportManager.isCreateCallbackReceived = true;
                    if (cmdTransportManager.isStopSignalPending)
                    {
                        cmdTransportManager.SendStopSignal();
                    }
                }
                cmdTransportManager.SendOneItem();
                cmdTransportManager.RaiseConnectCompleted();
                cmdTransportManager.StartReceivingData();
            }
        }

        private static void OnCreateCmdCompleted(IntPtr operationContext, int flags, IntPtr error, IntPtr shellOperationHandle, IntPtr commandOperationHandle, IntPtr operationHandle, IntPtr data)
        {
            BaseClientTransportManager.tracer.WriteLine("OnCreateCmdCompleted callback received", new object[0]);
            long cmdTMId = 0L;
            WSManClientCommandTransportManager cmdTransportManager = null;
            if (!TryGetCmdTransportManager(operationContext, out cmdTransportManager, out cmdTMId))
            {
                BaseClientTransportManager.tracer.WriteLine("OnCreateCmdCompleted: Unable to find a transport manager for the command context {0}.", new object[] { cmdTMId });
            }
            else
            {
                PSEtwLog.LogAnalyticInformational(PSEventId.WSManCreateCommandCallbackReceived, PSOpcode.Connect, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { cmdTransportManager.RunspacePoolInstanceId.ToString(), cmdTransportManager.powershellInstanceId.ToString() });
                if (cmdTransportManager.createCmdCompleted != null)
                {
                    cmdTransportManager.createCmdCompletedGCHandle.Free();
                    cmdTransportManager.createCmdCompleted.Dispose();
                    cmdTransportManager.createCmdCompleted = null;
                }
                cmdTransportManager.wsManCmdOperationHandle = commandOperationHandle;
                if (IntPtr.Zero != error)
                {
                    WSManNativeApi.WSManError errorStruct = WSManNativeApi.WSManError.UnMarshal(error);
                    if (errorStruct.errorCode != 0)
                    {
                        BaseClientTransportManager.tracer.WriteLine("OnCreateCmdCompleted callback: WSMan reported an error: {0}", new object[] { errorStruct.errorDetail });
                        TransportErrorOccuredEventArgs eventArgs = WSManTransportManagerUtils.ConstructTransportErrorEventArgs(WSManClientSessionTransportManager.wsManApiStaticData.WSManAPIHandle, null, errorStruct, TransportMethodEnum.RunShellCommandEx, RemotingErrorIdStrings.RunShellCommandExCallBackError, new object[] { WSManTransportManagerUtils.ParseEscapeWSManErrorMessage(errorStruct.errorDetail) });
                        cmdTransportManager.ProcessWSManTransportError(eventArgs);
                        return;
                    }
                }
                lock (cmdTransportManager.syncObject)
                {
                    cmdTransportManager.isCreateCallbackReceived = true;
                    if (cmdTransportManager.isClosed)
                    {
                        BaseClientTransportManager.tracer.WriteLine("Client Session TM: Transport manager is closed. So returning", new object[0]);
                        if (cmdTransportManager.isDisconnectPending)
                        {
                            cmdTransportManager.RaiseReadyForDisconnect();
                        }
                    }
                    else if (cmdTransportManager.isDisconnectPending)
                    {
                        cmdTransportManager.RaiseReadyForDisconnect();
                    }
                    else
                    {
                        if (cmdTransportManager.serializedPipeline.Length == 0L)
                        {
                            cmdTransportManager.shouldStartReceivingData = true;
                        }
                        cmdTransportManager.SendOneItem();
                        if (cmdTransportManager.isStopSignalPending)
                        {
                            cmdTransportManager.SendStopSignal();
                        }
                    }
                }
            }
        }

        private void OnDataAvailableCallback(byte[] data, DataPriorityType priorityType)
        {
            BaseClientTransportManager.tracer.WriteLine("Received data from dataToBeSent store.", new object[0]);
            this.chunkToSend = new SendDataChunk(data, priorityType);
            this.SendOneItem();
        }

        private static void OnReconnectCmdCompleted(IntPtr operationContext, int flags, IntPtr error, IntPtr shellOperationHandle, IntPtr commandOperationHandle, IntPtr operationHandle, IntPtr data)
        {
            long cmdTMId = 0L;
            WSManClientCommandTransportManager cmdTransportManager = null;
            if (!TryGetCmdTransportManager(operationContext, out cmdTransportManager, out cmdTMId))
            {
                BaseClientTransportManager.tracer.WriteLine("Unable to find a transport manager for the given command context {0}.", new object[] { cmdTMId });
            }
            else if (!shellOperationHandle.Equals(cmdTransportManager.wsManShellOperationHandle) || !commandOperationHandle.Equals(cmdTransportManager.wsManCmdOperationHandle))
            {
                BaseClientTransportManager.tracer.WriteLine("Cmd Signal callback: ShellOperationHandles are not the same as the signal is initiated with", new object[0]);
                PSRemotingTransportException e = new PSRemotingTransportException(RemotingErrorIdStrings.ReconnectShellCommandExCallBackError);
                TransportErrorOccuredEventArgs eventArgs = new TransportErrorOccuredEventArgs(e, TransportMethodEnum.ReconnectShellCommandEx);
                cmdTransportManager.ProcessWSManTransportError(eventArgs);
            }
            else
            {
                if (IntPtr.Zero != error)
                {
                    WSManNativeApi.WSManError errorStruct = WSManNativeApi.WSManError.UnMarshal(error);
                    if (errorStruct.errorCode != 0)
                    {
                        BaseClientTransportManager.tracer.WriteLine("OnReconnectCmdCompleted callback: WSMan reported an error: {0}", new object[] { errorStruct.errorDetail });
                        TransportErrorOccuredEventArgs args2 = WSManTransportManagerUtils.ConstructTransportErrorEventArgs(WSManClientSessionTransportManager.wsManApiStaticData.WSManAPIHandle, null, errorStruct, TransportMethodEnum.ReconnectShellCommandEx, RemotingErrorIdStrings.ReconnectShellCommandExCallBackError, new object[] { WSManTransportManagerUtils.ParseEscapeWSManErrorMessage(errorStruct.errorDetail) });
                        cmdTransportManager.ProcessWSManTransportError(args2);
                        return;
                    }
                }
                cmdTransportManager.shouldStartReceivingData = true;
                cmdTransportManager.SendOneItem();
                cmdTransportManager.RaiseReconnectCompleted();
            }
        }

        private static void OnRemoteCmdDataReceived(IntPtr operationContext, int flags, IntPtr error, IntPtr shellOperationHandle, IntPtr commandOperationHandle, IntPtr operationHandle, IntPtr data)
        {
            BaseClientTransportManager.tracer.WriteLine("Remote Command DataReceived callback.", new object[0]);
            long cmdTMId = 0L;
            WSManClientCommandTransportManager cmdTransportManager = null;
            if (!TryGetCmdTransportManager(operationContext, out cmdTransportManager, out cmdTMId))
            {
                BaseClientTransportManager.tracer.WriteLine("Unable to find a transport manager for the given command context {0}.", new object[] { cmdTMId });
            }
            else if (!shellOperationHandle.Equals(cmdTransportManager.wsManShellOperationHandle) || !commandOperationHandle.Equals(cmdTransportManager.wsManCmdOperationHandle))
            {
                BaseClientTransportManager.tracer.WriteLine("CmdReceive callback: ShellOperationHandles are not the same as the Receive is initiated with", new object[0]);
                PSRemotingTransportException e = new PSRemotingTransportException(RemotingErrorIdStrings.CommandReceiveExFailed);
                TransportErrorOccuredEventArgs eventArgs = new TransportErrorOccuredEventArgs(e, TransportMethodEnum.ReceiveCommandOutputEx);
                cmdTransportManager.ProcessWSManTransportError(eventArgs);
            }
            else
            {
                cmdTransportManager.ClearReceiveOrSendResources(flags, false);
                if (cmdTransportManager.isClosed)
                {
                    BaseClientTransportManager.tracer.WriteLine("Client Command TM: Transport manager is closed. So returning", new object[0]);
                }
                else
                {
                    if (IntPtr.Zero != error)
                    {
                        WSManNativeApi.WSManError errorStruct = WSManNativeApi.WSManError.UnMarshal(error);
                        if (errorStruct.errorCode != 0)
                        {
                            BaseClientTransportManager.tracer.WriteLine("CmdReceive callback: WSMan reported an error: {0}", new object[] { errorStruct.errorDetail });
                            TransportErrorOccuredEventArgs args2 = WSManTransportManagerUtils.ConstructTransportErrorEventArgs(WSManClientSessionTransportManager.wsManApiStaticData.WSManAPIHandle, null, errorStruct, TransportMethodEnum.ReceiveCommandOutputEx, RemotingErrorIdStrings.CommandReceiveExCallBackError, new object[] { errorStruct.errorDetail });
                            cmdTransportManager.ProcessWSManTransportError(args2);
                            return;
                        }
                    }
                    if (flags == 0x2000)
                    {
                        cmdTransportManager.isDisconnectedOnInvoke = true;
                        cmdTransportManager.RaiseDelayStreamProcessedEvent();
                    }
                    else
                    {
                        WSManNativeApi.WSManReceiveDataResult result = WSManNativeApi.WSManReceiveDataResult.UnMarshal(data);
                        if (result.data != null)
                        {
                            BaseClientTransportManager.tracer.WriteLine("Cmd Received Data : {0}", new object[] { result.data.Length });
                            object[] args = new object[] { cmdTransportManager.RunspacePoolInstanceId.ToString(), cmdTransportManager.powershellInstanceId.ToString(), result.data.Length.ToString(CultureInfo.InvariantCulture) };
                            PSEtwLog.LogAnalyticInformational(PSEventId.WSManReceiveShellOutputExCallbackReceived, PSOpcode.Receive, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, args);
                            cmdTransportManager.ProcessRawData(result.data, result.stream);
                        }
                    }
                }
            }
        }

        private static void OnRemoteCmdSendCompleted(IntPtr operationContext, int flags, IntPtr error, IntPtr shellOperationHandle, IntPtr commandOperationHandle, IntPtr operationHandle, IntPtr data)
        {
            BaseClientTransportManager.tracer.WriteLine("SendComplete callback received", new object[0]);
            long cmdTMId = 0L;
            WSManClientCommandTransportManager cmdTransportManager = null;
            if (!TryGetCmdTransportManager(operationContext, out cmdTransportManager, out cmdTMId))
            {
                BaseClientTransportManager.tracer.WriteLine("Unable to find a transport manager for the command context {0}.", new object[] { cmdTMId });
            }
            else
            {
                cmdTransportManager.isSendingInput = false;
                PSEtwLog.LogAnalyticInformational(PSEventId.WSManSendShellInputExCallbackReceived, PSOpcode.Connect, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { cmdTransportManager.RunspacePoolInstanceId.ToString(), cmdTransportManager.powershellInstanceId.ToString() });
                if (!shellOperationHandle.Equals(cmdTransportManager.wsManShellOperationHandle) || !commandOperationHandle.Equals(cmdTransportManager.wsManCmdOperationHandle))
                {
                    BaseClientTransportManager.tracer.WriteLine("SendShellInputEx callback: ShellOperationHandles are not the same as the Send is initiated with", new object[0]);
                    PSRemotingTransportException e = new PSRemotingTransportException(RemotingErrorIdStrings.CommandSendExFailed);
                    TransportErrorOccuredEventArgs eventArgs = new TransportErrorOccuredEventArgs(e, TransportMethodEnum.CommandInputEx);
                    cmdTransportManager.ProcessWSManTransportError(eventArgs);
                }
                else
                {
                    cmdTransportManager.ClearReceiveOrSendResources(flags, true);
                    if (cmdTransportManager.isClosed)
                    {
                        BaseClientTransportManager.tracer.WriteLine("Client Command TM: Transport manager is closed. So returning", new object[0]);
                        if (cmdTransportManager.isDisconnectPending)
                        {
                            cmdTransportManager.RaiseReadyForDisconnect();
                        }
                    }
                    else
                    {
                        if (IntPtr.Zero != error)
                        {
                            WSManNativeApi.WSManError errorStruct = WSManNativeApi.WSManError.UnMarshal(error);
                            if ((errorStruct.errorCode != 0) && (errorStruct.errorCode != 0x3e3))
                            {
                                BaseClientTransportManager.tracer.WriteLine("CmdSend callback: WSMan reported an error: {0}", new object[] { errorStruct.errorDetail });
                                TransportErrorOccuredEventArgs args2 = WSManTransportManagerUtils.ConstructTransportErrorEventArgs(WSManClientSessionTransportManager.wsManApiStaticData.WSManAPIHandle, null, errorStruct, TransportMethodEnum.CommandInputEx, RemotingErrorIdStrings.CommandSendExCallBackError, new object[] { WSManTransportManagerUtils.ParseEscapeWSManErrorMessage(errorStruct.errorDetail) });
                                cmdTransportManager.ProcessWSManTransportError(args2);
                                return;
                            }
                        }
						cmdTransportManager.shouldStartReceivingData = true;
                        cmdTransportManager.SendOneItem();
                    }
                }
            }
        }

        private static void OnRemoteCmdSignalCompleted(IntPtr operationContext, int flags, IntPtr error, IntPtr shellOperationHandle, IntPtr commandOperationHandle, IntPtr operationHandle, IntPtr data)
        {
            BaseClientTransportManager.tracer.WriteLine("Signal Completed callback received.", new object[0]);
            long cmdTMId = 0L;
            WSManClientCommandTransportManager cmdTransportManager = null;
            if (!TryGetCmdTransportManager(operationContext, out cmdTransportManager, out cmdTMId))
            {
                BaseClientTransportManager.tracer.WriteLine("Unable to find a transport manager for the given command context {0}.", new object[] { cmdTMId });
            }
            else
            {
                PSEtwLog.LogAnalyticInformational(PSEventId.WSManSignalCallbackReceived, PSOpcode.Disconnect, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { cmdTransportManager.RunspacePoolInstanceId.ToString(), cmdTransportManager.powershellInstanceId.ToString() });
                if (!shellOperationHandle.Equals(cmdTransportManager.wsManShellOperationHandle) || !commandOperationHandle.Equals(cmdTransportManager.wsManCmdOperationHandle))
                {
                    BaseClientTransportManager.tracer.WriteLine("Cmd Signal callback: ShellOperationHandles are not the same as the signal is initiated with", new object[0]);
                    PSRemotingTransportException e = new PSRemotingTransportException(RemotingErrorIdStrings.CommandSendExFailed);
                    TransportErrorOccuredEventArgs eventArgs = new TransportErrorOccuredEventArgs(e, TransportMethodEnum.CommandInputEx);
                    cmdTransportManager.ProcessWSManTransportError(eventArgs);
                }
                else
                {
                    if (IntPtr.Zero != cmdTransportManager.cmdSignalOperationHandle)
                    {
                        WSManNativeApi.WSManCloseOperation(cmdTransportManager.cmdSignalOperationHandle, 0);
                        cmdTransportManager.cmdSignalOperationHandle = IntPtr.Zero;
                    }
                    if (cmdTransportManager.signalCmdCompleted != null)
                    {
                        cmdTransportManager.signalCmdCompleted.Dispose();
                        cmdTransportManager.signalCmdCompleted = null;
                    }
                    if (cmdTransportManager.isClosed)
                    {
                        BaseClientTransportManager.tracer.WriteLine("Client Command TM: Transport manager is closed. So returning", new object[0]);
                    }
                    else
                    {
                        if (IntPtr.Zero != error)
                        {
                            WSManNativeApi.WSManError errorStruct = WSManNativeApi.WSManError.UnMarshal(error);
                            if (errorStruct.errorCode != 0)
                            {
                                BaseClientTransportManager.tracer.WriteLine("Cmd Signal callback: WSMan reported an error: {0}", new object[] { errorStruct.errorDetail });
                                TransportErrorOccuredEventArgs args2 = WSManTransportManagerUtils.ConstructTransportErrorEventArgs(WSManClientSessionTransportManager.wsManApiStaticData.WSManAPIHandle, null, errorStruct, TransportMethodEnum.CommandInputEx, RemotingErrorIdStrings.CommandSendExCallBackError, new object[] { WSManTransportManagerUtils.ParseEscapeWSManErrorMessage(errorStruct.errorDetail) });
                                cmdTransportManager.ProcessWSManTransportError(args2);
                                return;
                            }
                        }
                        cmdTransportManager.EnqueueAndStartProcessingThread(null, null, true);
                    }
                }
            }
        }

        internal override void PrepareForConnect()
        {
            this.isDisconnectPending = false;
        }

        internal override void PrepareForDisconnect()
        {
            this.isDisconnectPending = true;
            if ((base.isClosed || this.isDisconnectedOnInvoke) || ((this.isCreateCallbackReceived && (base.serializedPipeline.Length == 0L)) && !this.isSendingInput))
            {
                base.RaiseReadyForDisconnect();
            }
        }

        internal override void ProcessPrivateData(object privateData)
        {
            if ((bool) privateData)
            {
                base.RaiseSignalCompleted();
            }
        }

        internal void ProcessWSManTransportError(TransportErrorOccuredEventArgs eventArgs)
        {
            base.EnqueueAndStartProcessingThread(null, eventArgs, null);
        }

        internal override void RaiseErrorHandler(TransportErrorOccuredEventArgs eventArgs)
        {
            string stackTrace;
            if (!string.IsNullOrEmpty(eventArgs.Exception.StackTrace))
            {
                stackTrace = eventArgs.Exception.StackTrace;
            }
            else if ((eventArgs.Exception.InnerException != null) && !string.IsNullOrEmpty(eventArgs.Exception.InnerException.StackTrace))
            {
                stackTrace = eventArgs.Exception.InnerException.StackTrace;
            }
            else
            {
                stackTrace = string.Empty;
            }
            PSEtwLog.LogOperationalError(PSEventId.TransportError, PSOpcode.Open, PSTask.None, PSKeyword.UseAlwaysOperational, new object[] { base.RunspacePoolInstanceId.ToString(), this.powershellInstanceId.ToString(), eventArgs.Exception.ErrorCode.ToString(CultureInfo.InvariantCulture), eventArgs.Exception.Message, stackTrace });
            PSEtwLog.LogAnalyticError(PSEventId.TransportError_Analytic, PSOpcode.Open, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { base.RunspacePoolInstanceId.ToString(), this.powershellInstanceId.ToString(), eventArgs.Exception.ErrorCode.ToString(CultureInfo.InvariantCulture), eventArgs.Exception.Message, stackTrace });
            base.RaiseErrorHandler(eventArgs);
        }

        internal override void ReconnectAsync()
        {
            base.ReceivedDataCollection.PrepareForStreamConnect();
            lock (base.syncObject)
            {
                if (!base.isClosed)
                {
                    WSManNativeApi.WSManReconnectShellCommandEx(this.wsManCmdOperationHandle, 0, (IntPtr) this.reconnectCmdCompleted);
                }
            }
        }

        private static void RemoveCmdTransportManager(long cmdTMId)
        {
            lock (CmdTMHandles)
            {
                if (CmdTMHandles.ContainsKey(cmdTMId))
                {
                    CmdTMHandles[cmdTMId] = null;
                    CmdTMHandles.Remove(cmdTMId);
                }
            }
        }

        private void SendData(byte[] data, DataPriorityType priorityType)
        {
            BaseClientTransportManager.tracer.WriteLine("Command sending data of size : {0}", new object[] { data.Length });
            byte[] buffer = data;
            bool flag = true;
            if (commandSendRedirect != null)
            {
                object[] objArray3 = new object[2];
                objArray3[1] = buffer;
                object[] args = objArray3;
                flag = (bool) commandSendRedirect.DynamicInvoke(args);
                buffer = (byte[]) args[0];
            }
            if (flag)
            {
                using (WSManNativeApi.WSManData data2 = new WSManNativeApi.WSManData(buffer))
                {
                    PSEtwLog.LogAnalyticInformational(PSEventId.WSManSendShellInputEx, PSOpcode.Send, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { base.RunspacePoolInstanceId.ToString(), this.powershellInstanceId.ToString(), data2.BufferLength.ToString(CultureInfo.InvariantCulture) });
                    lock (base.syncObject)
                    {
                        if (base.isClosed)
                        {
                            BaseClientTransportManager.tracer.WriteLine("Client Session TM: Transport manager is closed. So returning", new object[0]);
                        }
                        else
                        {
                            this.sendToRemoteCompleted = new WSManNativeApi.WSManShellAsync(new IntPtr(this.cmdContextId), cmdSendCallback);
                            WSManNativeApi.WSManSendShellInputEx(this.wsManShellOperationHandle, this.wsManCmdOperationHandle, 0, (priorityType == DataPriorityType.Default) ? "stdin" : "pr", data2, (IntPtr) this.sendToRemoteCompleted, ref this.wsManSendOperationHandle);
                        }
                    }
                }
            }
        }

        internal void SendOneItem()
        {
            if (this.isDisconnectPending)
            {
                base.RaiseReadyForDisconnect();
            }
            else
            {
                byte[] data = null;
                DataPriorityType priorityType = DataPriorityType.Default;
                if (base.serializedPipeline.Length > 0L)
                {
                    data = base.serializedPipeline.ReadOrRegisterCallback(null);
                    if (base.serializedPipeline.Length == 0L)
                    {
                        this.shouldStartReceivingData = true;
                    }
                }
                else if (this.chunkToSend != null)
                {
                    data = this.chunkToSend.Data;
                    priorityType = this.chunkToSend.Type;
                    this.chunkToSend = null;
                }
                else
                {
                    data = base.dataToBeSent.ReadOrRegisterCallback(this.onDataAvailableToSendCallback, out priorityType);
                }
                if (data != null)
                {
                    this.isSendingInput = true;
                    this.SendData(data, priorityType);
                }
                if (this.shouldStartReceivingData)
                {
                    this.StartReceivingData();
                }
            }
        }

        internal override void SendStopSignal()
        {
            lock (base.syncObject)
            {
                if (!base.isClosed)
                {
                    if (!this.isCreateCallbackReceived)
                    {
                        this.isStopSignalPending = true;
                    }
                    else
                    {
                        this.isStopSignalPending = false;
                        BaseClientTransportManager.tracer.WriteLine("Sending stop signal with command context: {0} Operation Context {1}", new object[] { this.cmdContextId, this.wsManCmdOperationHandle });
						PSEtwLog.LogAnalyticInformational(PSEventId.WSManSignal, PSOpcode.Disconnect, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { base.RunspacePoolInstanceId.ToString(), this.powershellInstanceId.ToString(), StopSignal });
                        this.signalCmdCompleted = new WSManNativeApi.WSManShellAsync(new IntPtr(this.cmdContextId), cmdSignalCallback);
						WSManNativeApi.WSManSignalShellEx(this.wsManShellOperationHandle, this.wsManCmdOperationHandle, 0, StopSignal, (IntPtr) this.signalCmdCompleted, ref this.cmdSignalOperationHandle);
                    }
                }
            }
        }

        internal override void StartReceivingData()
        {
            PSEtwLog.LogAnalyticInformational(PSEventId.WSManReceiveShellOutputEx, PSOpcode.Receive, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { base.RunspacePoolInstanceId.ToString(), this.powershellInstanceId.ToString() });
            this.shouldStartReceivingData = false;
            lock (base.syncObject)
            {
                if (base.isClosed)
                {
                    BaseClientTransportManager.tracer.WriteLine("Client Session TM: Transport manager is closed. So returning", new object[0]);
                }
                else if (base.receiveDataInitiated)
                {
                    BaseClientTransportManager.tracer.WriteLine("Client Session TM: Command ReceiveData has already been called.", new object[0]);
                }
                else
                {
                    base.receiveDataInitiated = true;
                    this.receivedFromRemote = new WSManNativeApi.WSManShellAsync(new IntPtr(this.cmdContextId), cmdReceiveCallback);
                    WSManNativeApi.WSManReceiveShellOutputEx(this.wsManShellOperationHandle, this.wsManCmdOperationHandle, base.startInDisconnectedMode ? 0x10 : 0, (IntPtr) WSManClientSessionTransportManager.wsManApiStaticData.OutputStreamSet, (IntPtr) this.receivedFromRemote, ref this.wsManRecieveOperationHandle);
                }
            }
        }

        private static bool TryGetCmdTransportManager(IntPtr operationContext, out WSManClientCommandTransportManager cmdTransportManager, out long cmdTMId)
        {
            cmdTMId = operationContext.ToInt64();
            cmdTransportManager = null;
            lock (CmdTMHandles)
            {
                return CmdTMHandles.TryGetValue(cmdTMId, out cmdTransportManager);
            }
        }

        private class SendDataChunk
        {
            private byte[] data;
            private DataPriorityType type;

            public SendDataChunk(byte[] data, DataPriorityType type)
            {
                this.data = data;
                this.type = type;
            }

            public byte[] Data
            {
                get
                {
                    return this.data;
                }
            }

            public DataPriorityType Type
            {
                get
                {
                    return this.type;
                }
            }
        }
    }
}

