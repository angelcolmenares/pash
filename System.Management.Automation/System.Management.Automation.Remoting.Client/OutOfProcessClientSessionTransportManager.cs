namespace System.Management.Automation.Remoting.Client
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Runspaces.Internal;
    using System.Management.Automation.Tracing;
    using System.Timers;

    internal class OutOfProcessClientSessionTransportManager : BaseClientSessionTransportManager
    {
        private bool _processCreated;
        private PowerShellProcessInstance _processInstance;
        private PowerShellTraceSource _tracer;
        private Timer closeTimeOutTimer;
        private Dictionary<Guid, OutOfProcessClientCommandTransportManager> cmdTransportManagers;
        private NewProcessConnectionInfo connectionInfo;
        private OutOfProcessUtils.DataProcessingDelegates dataProcessingCallbacks;
        private System.Management.Automation.Remoting.PrioritySendDataCollection.OnDataAvailableCallback onDataAvailableToSendCallback;
        private Process serverProcess;
        private OutOfProcessTextWriter stdInWriter;

        internal OutOfProcessClientSessionTransportManager(Guid runspaceId, NewProcessConnectionInfo connectionInfo, PSRemotingCryptoHelper cryptoHelper) : base(runspaceId, cryptoHelper)
        {
            this._processCreated = true;
            this.onDataAvailableToSendCallback = new System.Management.Automation.Remoting.PrioritySendDataCollection.OnDataAvailableCallback(this.OnDataAvailableCallback);
            this.cmdTransportManagers = new Dictionary<Guid, OutOfProcessClientCommandTransportManager>();
            this.connectionInfo = connectionInfo;
            this.dataProcessingCallbacks = new OutOfProcessUtils.DataProcessingDelegates();
            this.dataProcessingCallbacks.DataPacketReceived = (OutOfProcessUtils.DataPacketReceived) Delegate.Combine(this.dataProcessingCallbacks.DataPacketReceived, new OutOfProcessUtils.DataPacketReceived(this.OnDataPacketReceived));
            this.dataProcessingCallbacks.DataAckPacketReceived = (OutOfProcessUtils.DataAckPacketReceived) Delegate.Combine(this.dataProcessingCallbacks.DataAckPacketReceived, new OutOfProcessUtils.DataAckPacketReceived(this.OnDataAckPacketReceived));
            this.dataProcessingCallbacks.CommandCreationPacketReceived = (OutOfProcessUtils.CommandCreationPacketReceived) Delegate.Combine(this.dataProcessingCallbacks.CommandCreationPacketReceived, new OutOfProcessUtils.CommandCreationPacketReceived(this.OnCommandCreationPacketReceived));
            this.dataProcessingCallbacks.CommandCreationAckReceived = (OutOfProcessUtils.CommandCreationAckReceived) Delegate.Combine(this.dataProcessingCallbacks.CommandCreationAckReceived, new OutOfProcessUtils.CommandCreationAckReceived(this.OnCommandCreationAckReceived));
            this.dataProcessingCallbacks.SignalPacketReceived = (OutOfProcessUtils.SignalPacketReceived) Delegate.Combine(this.dataProcessingCallbacks.SignalPacketReceived, new OutOfProcessUtils.SignalPacketReceived(this.OnSignalPacketReceived));
            this.dataProcessingCallbacks.SignalAckPacketReceived = (OutOfProcessUtils.SignalAckPacketReceived) Delegate.Combine(this.dataProcessingCallbacks.SignalAckPacketReceived, new OutOfProcessUtils.SignalAckPacketReceived(this.OnSiganlAckPacketReceived));
            this.dataProcessingCallbacks.ClosePacketReceived = (OutOfProcessUtils.ClosePacketReceived) Delegate.Combine(this.dataProcessingCallbacks.ClosePacketReceived, new OutOfProcessUtils.ClosePacketReceived(this.OnClosePacketReceived));
            this.dataProcessingCallbacks.CloseAckPacketReceived = (OutOfProcessUtils.CloseAckPacketReceived) Delegate.Combine(this.dataProcessingCallbacks.CloseAckPacketReceived, new OutOfProcessUtils.CloseAckPacketReceived(this.OnCloseAckReceived));
            base.dataToBeSent.Fragmentor = base.Fragmentor;
            base.ReceivedDataCollection.MaximumReceivedDataSize = null;
            base.ReceivedDataCollection.MaximumReceivedObjectSize = 0xa00000;
            this.closeTimeOutTimer = new Timer(60000.0);
            this.closeTimeOutTimer.Elapsed += new ElapsedEventHandler(this.OnCloseTimeOutTimerElapsed);
            this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
        }

        private void AddCommandTransportManager(Guid key, OutOfProcessClientCommandTransportManager cmdTM)
        {
            lock (base.syncObject)
            {
                if (base.isClosed)
                {
                    this._tracer.WriteMessage("OutOfProcessClientSessionTransportManager.AddCommandTransportManager, Adding command transport on closed session, RunSpacePool Id : " + base.RunspacePoolInstanceId);
                }
                else
                {
                    this.cmdTransportManagers.Add(key, cmdTM);
                }
            }
        }

        internal override void CloseAsync()
        {
            bool flag = false;
            lock (base.syncObject)
            {
                if (base.isClosed)
                {
                    return;
                }
                base.isClosed = true;
                if (this.stdInWriter == null)
                {
                    flag = true;
                }
            }
            base.CloseAsync();
            if (!flag)
            {
                PSEtwLog.LogAnalyticInformational(PSEventId.WSManCloseShell, PSOpcode.Disconnect, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { base.RunspacePoolInstanceId.ToString() });
                this._tracer.WriteMessage(string.Concat(new object[] { "OutOfProcessClientSessionTransportManager.CloseAsync, when sending close session packet, progress command count should be zero, current cmd count: ", this.cmdTransportManagers.Count, ", RunSpacePool Id : ", base.RunspacePoolInstanceId }));
                this.stdInWriter.WriteLine(OutOfProcessUtils.CreateClosePacket(Guid.Empty));
                this.closeTimeOutTimer.Start();
            }
            else
            {
                base.RaiseCloseCompleted();
            }
        }

        internal override void ConnectAsync()
        {
            throw new NotImplementedException(RemotingErrorIdStrings.IPCTransportConnectError);
        }

        internal override void CreateAsync()
        {
            if (this.connectionInfo != null)
            {
                this._processInstance = this.connectionInfo.Process ?? new PowerShellProcessInstance(this.connectionInfo.PSVersion, this.connectionInfo.Credential, this.connectionInfo.InitializationScript, this.connectionInfo.RunAs32);
                if (this.connectionInfo.Process != null)
                {
                    this._processCreated = false;
                }
            }
            PSEtwLog.LogAnalyticInformational(PSEventId.WSManCreateShell, PSOpcode.Connect, PSTask.CreateRunspace, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { base.RunspacePoolInstanceId.ToString() });
            try
            {
                lock (base.syncObject)
                {
                    if (base.isClosed)
                    {
                        return;
                    }
                    this.serverProcess = this._processInstance.Process;
                    if (this._processInstance.RunspacePool != null)
                    {
                        this._processInstance.RunspacePool.Close();
                        this._processInstance.RunspacePool.Dispose();
                    }
                    this.stdInWriter = this._processInstance.StdInWriter;
                    this.serverProcess.OutputDataReceived += new DataReceivedEventHandler(this.OnOutputDataReceived);
                    this.serverProcess.ErrorDataReceived += new DataReceivedEventHandler(this.OnErrorDataReceived);
                    this.serverProcess.Exited += new EventHandler(this.OnExited);
                    this._processInstance.Start();
                    if (this.stdInWriter != null)
                    {
                        this.serverProcess.CancelErrorRead();
                        this.serverProcess.CancelOutputRead();
                    }
                    this.serverProcess.BeginOutputReadLine();
                    this.serverProcess.BeginErrorReadLine();
                    this.stdInWriter = new OutOfProcessTextWriter(this.serverProcess.StandardInput);
                    this._processInstance.StdInWriter = this.stdInWriter;
                }
            }
            catch (Win32Exception exception)
            {
                PSRemotingTransportException e = new PSRemotingTransportException(exception, RemotingErrorIdStrings.IPCExceptionLaunchingProcess, new object[] { exception.Message }) {
                    ErrorCode = exception.ErrorCode
                };
                TransportErrorOccuredEventArgs eventArgs = new TransportErrorOccuredEventArgs(e, TransportMethodEnum.CreateShellEx);
                this.RaiseErrorHandler(eventArgs);
                return;
            }
            catch (Exception exception3)
            {
                CommandProcessorBase.CheckForSevereException(exception3);
                PSRemotingTransportException exception4 = new PSRemotingTransportException(PSRemotingErrorId.IPCExceptionLaunchingProcess, RemotingErrorIdStrings.IPCExceptionLaunchingProcess, new object[] { exception3.Message });
                TransportErrorOccuredEventArgs args2 = new TransportErrorOccuredEventArgs(exception4, TransportMethodEnum.CreateShellEx);
                this.RaiseErrorHandler(args2);
                return;
            }
            this.SendOneItem();
        }

        internal override BaseClientCommandTransportManager CreateClientCommandTransportManager(RunspaceConnectionInfo connectionInfo, ClientRemotePowerShell cmd, bool noInput)
        {
            OutOfProcessClientCommandTransportManager cmdTM = new OutOfProcessClientCommandTransportManager(cmd, noInput, this, this.stdInWriter);
            this.AddCommandTransportManager(cmd.InstanceId, cmdTM);
            return cmdTM;
        }

        internal override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            if (isDisposing)
            {
                this.cmdTransportManagers.Clear();
                this.closeTimeOutTimer.Dispose();
                this.KillServerProcess();
                if ((this.serverProcess != null) && this._processCreated)
                {
                    this.serverProcess.Dispose();
                }
            }
        }

        private OutOfProcessClientCommandTransportManager GetCommandTransportManager(Guid key)
        {
            lock (base.syncObject)
            {
                OutOfProcessClientCommandTransportManager manager = null;
                this.cmdTransportManagers.TryGetValue(key, out manager);
                return manager;
            }
        }

        private void KillServerProcess()
        {
            if (this.serverProcess != null)
            {
                try
                {
                    if (!this.serverProcess.HasExited)
                    {
                        this.serverProcess.Exited -= new EventHandler(this.OnExited);
                        if (this._processCreated)
                        {
                            this.serverProcess.CancelOutputRead();
                            this.serverProcess.CancelErrorRead();
                            this.serverProcess.Kill();
                        }
                        this.serverProcess.OutputDataReceived -= new DataReceivedEventHandler(this.OnOutputDataReceived);
                        this.serverProcess.ErrorDataReceived -= new DataReceivedEventHandler(this.OnErrorDataReceived);
                    }
                }
                catch (Win32Exception)
                {
                    try
                    {
                        Process processById = Process.GetProcessById(this.serverProcess.Id);
                        if (this._processCreated)
                        {
                            processById.Kill();
                        }
                    }
                    catch (Exception exception)
                    {
                        CommandProcessorBase.CheckForSevereException(exception);
                    }
                }
                catch (Exception exception2)
                {
                    CommandProcessorBase.CheckForSevereException(exception2);
                }
            }
        }

        private void OnCloseAckReceived(Guid psGuid)
        {
            int count;
            lock (base.syncObject)
            {
                count = this.cmdTransportManagers.Count;
            }
            if (psGuid == Guid.Empty)
            {
                this._tracer.WriteMessage(string.Concat(new object[] { "OutOfProcessClientSessionTransportManager.OnCloseAckReceived, progress command count after CLOSE ACK should be zero = ", count, " psGuid : ", psGuid.ToString() }));
                this.OnCloseSessionCompleted();
            }
            else
            {
                this._tracer.WriteMessage(string.Concat(new object[] { "OutOfProcessClientSessionTransportManager.OnCloseAckReceived, in progress command count should be greater than zero: ", count, ", RunSpacePool Id : ", base.RunspacePoolInstanceId, ", psGuid : ", psGuid.ToString() }));
                OutOfProcessClientCommandTransportManager commandTransportManager = this.GetCommandTransportManager(psGuid);
                if (commandTransportManager != null)
                {
                    commandTransportManager.OnCloseCmdCompleted();
                }
            }
        }

        private void OnClosePacketReceived(Guid psGuid)
        {
            throw new PSRemotingTransportException(PSRemotingErrorId.IPCUnknownElementReceived, RemotingErrorIdStrings.IPCUnknownElementReceived, new object[] { "Close" });
        }

        private void OnCloseSessionCompleted()
        {
            this.closeTimeOutTimer.Stop();
            base.RaiseCloseCompleted();
            this.KillServerProcess();
        }

        internal void OnCloseTimeOutTimerElapsed(object source, ElapsedEventArgs e)
        {
            this.closeTimeOutTimer.Stop();
            PSRemotingTransportException exception = new PSRemotingTransportException(PSRemotingErrorId.IPCCloseTimedOut, RemotingErrorIdStrings.IPCCloseTimedOut, new object[0]);
            this.RaiseErrorHandler(new TransportErrorOccuredEventArgs(exception, TransportMethodEnum.CloseShellOperationEx));
        }

        private void OnCommandCreationAckReceived(Guid psGuid)
        {
            OutOfProcessClientCommandTransportManager commandTransportManager = this.GetCommandTransportManager(psGuid);
            if (commandTransportManager == null)
            {
                throw new PSRemotingTransportException(PSRemotingErrorId.IPCUnknownCommandGuid, RemotingErrorIdStrings.IPCUnknownCommandGuid, new object[] { psGuid.ToString(), "CommandAck" });
            }
            commandTransportManager.OnCreateCmdCompleted();
            this._tracer.WriteMessage(string.Concat(new object[] { "OutOfProcessClientSessionTransportManager.OnCommandCreationAckReceived, in progress command count after cmd creation ACK : ", this.cmdTransportManagers.Count, ", psGuid : ", psGuid.ToString() }));
        }

        private void OnCommandCreationPacketReceived(Guid psGuid)
        {
            throw new PSRemotingTransportException(PSRemotingErrorId.IPCUnknownElementReceived, RemotingErrorIdStrings.IPCUnknownElementReceived, new object[] { "Command" });
        }

        private void OnDataAckPacketReceived(Guid psGuid)
        {
            if (psGuid == Guid.Empty)
            {
                this.OnRemoteSessionSendCompleted();
            }
            else
            {
                OutOfProcessClientCommandTransportManager commandTransportManager = this.GetCommandTransportManager(psGuid);
                if (commandTransportManager != null)
                {
                    commandTransportManager.OnRemoteCmdSendCompleted();
                }
            }
        }

        private void OnDataAvailableCallback(byte[] data, DataPriorityType priorityType)
        {
            BaseClientTransportManager.tracer.WriteLine("Received data to be sent from the callback.", new object[0]);
            this.SendData(data, priorityType);
        }

        private void OnDataPacketReceived(byte[] rawData, string stream, Guid psGuid)
        {
            string str = "stdout";
            if (stream.Equals(DataPriorityType.PromptResponse.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                str = "pr";
            }
            if (psGuid == Guid.Empty)
            {
                object[] args = new object[] { base.RunspacePoolInstanceId.ToString(), Guid.Empty.ToString(), rawData.Length.ToString(CultureInfo.InvariantCulture) };
                PSEtwLog.LogAnalyticInformational(PSEventId.WSManReceiveShellOutputExCallbackReceived, PSOpcode.Receive, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, args);
                base.ProcessRawData(rawData, str);
            }
            else
            {
                OutOfProcessClientCommandTransportManager commandTransportManager = this.GetCommandTransportManager(psGuid);
                if (commandTransportManager != null)
                {
                    commandTransportManager.OnRemoteCmdDataReceived(rawData, str);
                }
            }
        }

        private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            lock (base.syncObject)
            {
                if (base.isClosed)
                {
                    return;
                }
            }
            PSRemotingTransportException exception = new PSRemotingTransportException(PSRemotingErrorId.IPCServerProcessReportedError, RemotingErrorIdStrings.IPCServerProcessReportedError, new object[] { e.Data });
            this.RaiseErrorHandler(new TransportErrorOccuredEventArgs(exception, TransportMethodEnum.Unknown));
        }

        private void OnExited(object sender, EventArgs e)
        {
            TransportMethodEnum unknown = TransportMethodEnum.Unknown;
            lock (base.syncObject)
            {
                if (base.isClosed)
                {
                    unknown = TransportMethodEnum.CloseShellOperationEx;
                }
                this.stdInWriter.StopWriting();
            }
            PSRemotingTransportException exception = new PSRemotingTransportException(PSRemotingErrorId.IPCServerProcessExited, RemotingErrorIdStrings.IPCServerProcessExited, new object[0]);
            this.RaiseErrorHandler(new TransportErrorOccuredEventArgs(exception, unknown));
        }

        private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            try
            {
                OutOfProcessUtils.ProcessData(e.Data, this.dataProcessingCallbacks);
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                PSRemotingTransportException exception2 = new PSRemotingTransportException(PSRemotingErrorId.IPCErrorProcessingServerData, RemotingErrorIdStrings.IPCErrorProcessingServerData, new object[] { exception.Message });
                this.RaiseErrorHandler(new TransportErrorOccuredEventArgs(exception2, TransportMethodEnum.ReceiveShellOutputEx));
            }
        }

        private void OnRemoteSessionSendCompleted()
        {
            PSEtwLog.LogAnalyticInformational(PSEventId.WSManSendShellInputExCallbackReceived, PSOpcode.Connect, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { base.RunspacePoolInstanceId.ToString(), Guid.Empty.ToString() });
            this.SendOneItem();
        }

        private void OnSiganlAckPacketReceived(Guid psGuid)
        {
            if (psGuid == Guid.Empty)
            {
                throw new PSRemotingTransportException(PSRemotingErrorId.IPCNoSignalForSession, RemotingErrorIdStrings.IPCNoSignalForSession, new object[] { "SignalAck" });
            }
            OutOfProcessClientCommandTransportManager commandTransportManager = this.GetCommandTransportManager(psGuid);
            if (commandTransportManager != null)
            {
                commandTransportManager.OnRemoteCmdSignalCompleted();
            }
        }

        private void OnSignalPacketReceived(Guid psGuid)
        {
            throw new PSRemotingTransportException(PSRemotingErrorId.IPCUnknownElementReceived, RemotingErrorIdStrings.IPCUnknownElementReceived, new object[] { "Signal" });
        }

        internal override void RemoveCommandTransportManager(Guid key)
        {
            lock (base.syncObject)
            {
                if (this.cmdTransportManagers.ContainsKey(key))
                {
                    this.cmdTransportManagers.Remove(key);
                }
                else
                {
                    this._tracer.WriteMessage("key does not exist to remove from cmdTransportManagers");
                }
            }
        }

        private void SendData(byte[] data, DataPriorityType priorityType)
        {
            object[] args = new object[] { base.RunspacePoolInstanceId.ToString(), Guid.Empty.ToString(), data.Length.ToString(CultureInfo.InvariantCulture) };
            PSEtwLog.LogAnalyticInformational(PSEventId.WSManSendShellInputEx, PSOpcode.Send, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, args);
            lock (base.syncObject)
            {
                if (!base.isClosed)
                {
                    this.stdInWriter.WriteLine(OutOfProcessUtils.CreateDataPacket(data, priorityType, Guid.Empty));
                }
            }
        }

        private void SendOneItem()
        {
            DataPriorityType type;
            byte[] data = base.dataToBeSent.ReadOrRegisterCallback(this.onDataAvailableToSendCallback, out type);
            if (data != null)
            {
                this.SendData(data, type);
            }
        }
    }
}

