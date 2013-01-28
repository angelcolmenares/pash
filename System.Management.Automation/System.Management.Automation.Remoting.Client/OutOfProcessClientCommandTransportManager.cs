namespace System.Management.Automation.Remoting.Client
{
    using System;
    using System.Globalization;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces.Internal;
    using System.Management.Automation.Tracing;
    using System.Timers;

    internal class OutOfProcessClientCommandTransportManager : BaseClientCommandTransportManager
    {
        private System.Management.Automation.Remoting.PrioritySendDataCollection.OnDataAvailableCallback onDataAvailableToSendCallback;
        private Timer signalTimeOutTimer;
        private OutOfProcessTextWriter stdInWriter;

        internal OutOfProcessClientCommandTransportManager(ClientRemotePowerShell cmd, bool noInput, OutOfProcessClientSessionTransportManager sessnTM, OutOfProcessTextWriter stdInWriter) : base(cmd, sessnTM.CryptoHelper, sessnTM)
        {
            this.stdInWriter = stdInWriter;
            this.onDataAvailableToSendCallback = new System.Management.Automation.Remoting.PrioritySendDataCollection.OnDataAvailableCallback(this.OnDataAvailableCallback);
            this.signalTimeOutTimer = new Timer(60000.0);
            this.signalTimeOutTimer.Elapsed += new ElapsedEventHandler(this.OnSignalTimeOutTimerElapsed);
        }

        internal override void CloseAsync()
        {
            lock (base.syncObject)
            {
                if (base.isClosed)
                {
                    return;
                }
                base.isClosed = true;
            }
            base.CloseAsync();
            PSEtwLog.LogAnalyticInformational(PSEventId.WSManCloseCommand, PSOpcode.Disconnect, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { base.RunspacePoolInstanceId.ToString(), this.powershellInstanceId.ToString() });
            if (this.stdInWriter != null)
            {
                this.stdInWriter.WriteLine(OutOfProcessUtils.CreateClosePacket(base.powershellInstanceId));
            }
        }

        internal override void ConnectAsync()
        {
            throw new NotImplementedException(RemotingErrorIdStrings.IPCTransportConnectError);
        }

        internal override void CreateAsync()
        {
            PSEtwLog.LogAnalyticInformational(PSEventId.WSManCreateCommand, PSOpcode.Connect, PSTask.CreateRunspace, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { base.RunspacePoolInstanceId.ToString(), this.powershellInstanceId.ToString() });
            this.stdInWriter.WriteLine(OutOfProcessUtils.CreateCommandPacket(base.powershellInstanceId));
        }

        internal override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            if (isDisposing)
            {
                this.StopSignalTimerAndDecrementOperations();
                this.signalTimeOutTimer.Dispose();
            }
        }

        internal void OnCloseCmdCompleted()
        {
            PSEtwLog.LogAnalyticInformational(PSEventId.WSManCloseCommandCallbackReceived, PSOpcode.Disconnect, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { base.RunspacePoolInstanceId.ToString(), this.powershellInstanceId.ToString() });
            base.RaiseCloseCompleted();
        }

        internal void OnCreateCmdCompleted()
        {
            PSEtwLog.LogAnalyticInformational(PSEventId.WSManCreateCommandCallbackReceived, PSOpcode.Connect, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { base.RunspacePoolInstanceId.ToString(), this.powershellInstanceId.ToString() });
            lock (base.syncObject)
            {
                if (base.isClosed)
                {
                    BaseClientTransportManager.tracer.WriteLine("Client Session TM: Transport manager is closed. So returning", new object[0]);
                }
                else
                {
                    this.SendOneItem();
                }
            }
        }

        private void OnDataAvailableCallback(byte[] data, DataPriorityType priorityType)
        {
            BaseClientTransportManager.tracer.WriteLine("Received data from dataToBeSent store.", new object[0]);
            this.SendData(data, priorityType);
        }

        internal void OnRemoteCmdDataReceived(byte[] rawData, string stream)
        {
            object[] args = new object[] { base.RunspacePoolInstanceId.ToString(), this.powershellInstanceId.ToString(), rawData.Length.ToString(CultureInfo.InvariantCulture) };
            PSEtwLog.LogAnalyticInformational(PSEventId.WSManReceiveShellOutputExCallbackReceived, PSOpcode.Receive, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, args);
            if (base.isClosed)
            {
                BaseClientTransportManager.tracer.WriteLine("Client Command TM: Transport manager is closed. So returning", new object[0]);
            }
            else
            {
                this.ProcessRawData(rawData, stream);
            }
        }

        internal void OnRemoteCmdSendCompleted()
        {
            PSEtwLog.LogAnalyticInformational(PSEventId.WSManSendShellInputExCallbackReceived, PSOpcode.Connect, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { base.RunspacePoolInstanceId.ToString(), this.powershellInstanceId.ToString() });
            lock (base.syncObject)
            {
                if (base.isClosed)
                {
                    BaseClientTransportManager.tracer.WriteLine("Client Command TM: Transport manager is closed. So returning", new object[0]);
                    return;
                }
            }
            this.SendOneItem();
        }

        internal void OnRemoteCmdSignalCompleted()
        {
            PSEtwLog.LogAnalyticInformational(PSEventId.WSManSignalCallbackReceived, PSOpcode.Disconnect, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { base.RunspacePoolInstanceId.ToString(), this.powershellInstanceId.ToString() });
            this.StopSignalTimerAndDecrementOperations();
            if (!base.isClosed)
            {
                base.EnqueueAndStartProcessingThread(null, null, true);
            }
        }

        internal void OnSignalTimeOutTimerElapsed(object source, ElapsedEventArgs e)
        {
            this.StopSignalTimerAndDecrementOperations();
            if (!base.isClosed)
            {
                PSRemotingTransportException exception = new PSRemotingTransportException(RemotingErrorIdStrings.IPCSignalTimedOut);
                this.RaiseErrorHandler(new TransportErrorOccuredEventArgs(exception, TransportMethodEnum.ReceiveShellOutputEx));
            }
        }

        internal override void ProcessPrivateData(object privateData)
        {
            if ((bool) privateData)
            {
                base.RaiseSignalCompleted();
            }
        }

        private void SendData(byte[] data, DataPriorityType priorityType)
        {
            object[] args = new object[] { base.RunspacePoolInstanceId.ToString(), this.powershellInstanceId.ToString(), data.Length.ToString(CultureInfo.InvariantCulture) };
            PSEtwLog.LogAnalyticInformational(PSEventId.WSManSendShellInputEx, PSOpcode.Send, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, args);
            lock (base.syncObject)
            {
                if (!base.isClosed)
                {
                    this.stdInWriter.WriteLine(OutOfProcessUtils.CreateDataPacket(data, priorityType, base.powershellInstanceId));
                }
            }
        }

        private void SendOneItem()
        {
            byte[] data = null;
            DataPriorityType priorityType = DataPriorityType.Default;
            if (base.serializedPipeline.Length > 0L)
            {
                data = base.serializedPipeline.ReadOrRegisterCallback(null);
            }
            else
            {
                data = base.dataToBeSent.ReadOrRegisterCallback(this.onDataAvailableToSendCallback, out priorityType);
            }
            if (data != null)
            {
                this.SendData(data, priorityType);
            }
        }

        internal override void SendStopSignal()
        {
            PSEtwLog.LogAnalyticInformational(PSEventId.WSManSignal, PSOpcode.Disconnect, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { base.RunspacePoolInstanceId.ToString(), this.powershellInstanceId.ToString(), "stopsignal" });
            base.CloseAsync();
            this.stdInWriter.WriteLine(OutOfProcessUtils.CreateSignalPacket(base.powershellInstanceId));
            this.signalTimeOutTimer.Start();
        }

        private void StopSignalTimerAndDecrementOperations()
        {
            lock (base.syncObject)
            {
                if (this.signalTimeOutTimer.Enabled)
                {
                    this.signalTimeOutTimer.Stop();
                }
            }
        }
    }
}

