namespace System.Management.Automation.Runspaces.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;
    using System.Threading;

    internal class ClientRemotePowerShell : IDisposable
    {
        protected Guid clientRunspacePoolId;
        protected string computerName;
        private PSConnectionRetryStatus connectionRetryStatus;
        protected ClientPowerShellDataStructureHandler dataStructureHandler;
        protected ObjectStreamBase errorstream;
        protected PSHost hostToUse;
        protected PSInformationalBuffers informationalBuffers;
        protected bool initialized;
        protected ObjectStreamBase inputstream;
        protected bool noInput;
        protected ObjectStreamBase outputstream;
        protected RemoteRunspacePoolInternal runspacePool;
        protected PSInvocationSettings settings;
        protected System.Management.Automation.PowerShell shell;
        private Queue<PSInvocationStateInfo> stateInfoQueue = new Queue<PSInvocationStateInfo>();
        protected bool stopCalled;
        [TraceSource("CRPS", "ClientRemotePowerShell")]
        private static PSTraceSource tracer = PSTraceSource.GetTracer("CRPS", "ClientRemotePowerShellBase");
        protected const string WRITE_DEBUG_LINE = "WriteDebugLine";
        protected const string WRITE_PROGRESS = "WriteProgress";
        protected const string WRITE_VERBOSE_LINE = "WriteVerboseLine";
        protected const string WRITE_WARNING_LINE = "WriteWarningLine";

        internal event EventHandler<RemoteDataEventArgs<RemoteHostCall>> HostCallReceived;

        internal event EventHandler<PSConnectionRetryStatusEventArgs> RCConnectionNotification;

        internal ClientRemotePowerShell(System.Management.Automation.PowerShell shell, RemoteRunspacePoolInternal runspacePool)
        {
            this.shell = shell;
            this.clientRunspacePoolId = runspacePool.InstanceId;
            this.runspacePool = runspacePool;
            this.computerName = runspacePool.ConnectionInfo.ComputerName;
        }

        private void CheckAndCloseRunspaceAfterStop(Exception ex)
        {
            PSRemotingTransportException exception = ex as PSRemotingTransportException;
            if ((exception != null) && ((exception.ErrorCode == -2144108526) || (exception.ErrorCode == -2144108250)))
            {
                object runspaceConnection = this.shell.GetRunspaceConnection();
                if (runspaceConnection is Runspace)
                {
                    Runspace runspace = (Runspace) runspaceConnection;
                    if (runspace.RunspaceStateInfo.State == RunspaceState.Opened)
                    {
                        try
                        {
                            runspace.Close();
                        }
                        catch (PSRemotingTransportException)
                        {
                        }
                    }
                }
                else if (runspaceConnection is RunspacePool)
                {
                    RunspacePool pool = (RunspacePool) runspaceConnection;
                    if (pool.RunspacePoolStateInfo.State == RunspacePoolState.Opened)
                    {
                        try
                        {
                            pool.Close();
                        }
                        catch (PSRemotingTransportException)
                        {
                        }
                    }
                }
            }
        }

        internal void Clear()
        {
            this.initialized = false;
        }

        internal void ConnectAsync(ConnectCommandInfo connectCmdInfo)
        {
            if (connectCmdInfo == null)
            {
                this.dataStructureHandler.ReconnectAsync();
            }
            else
            {
                this.shell.RunspacePool.RemoteRunspacePoolInternal.AddRemotePowerShellDSHandler(this.InstanceId, this.dataStructureHandler);
                this.dataStructureHandler.ConnectAsync();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
        }

        private void ExecuteHostCall(RemoteHostCall hostcall)
        {
            if (hostcall.IsVoidMethod)
            {
                if (hostcall.IsSetShouldExitOrPopRunspace)
                {
                    this.shell.ClearRemotePowerShell();
                }
                hostcall.ExecuteVoidMethod(this.hostToUse);
            }
            else
            {
                RemoteHostResponse hostResponse = hostcall.ExecuteNonVoidMethod(this.hostToUse);
                this.dataStructureHandler.SendHostResponseToServer(hostResponse);
            }
        }

        internal static void ExitHandler(object sender, RemoteDataEventArgs<RemoteHostCall> eventArgs)
        {
            RemoteHostCall data = eventArgs.Data;
            if (!data.IsSetShouldExitOrPopRunspace)
            {
                ((ClientRemotePowerShell) sender).ExecuteHostCall(data);
            }
        }

        private void HandleBrokenNotificationFromRunspacePool(object sender, RemoteDataEventArgs<Exception> eventArgs)
        {
            this.UnblockCollections();
            this.dataStructureHandler.RaiseRemoveAssociationEvent();
            if (this.stopCalled)
            {
                this.stopCalled = false;
                this.SetStateInfo(new PSInvocationStateInfo(PSInvocationState.Stopped, eventArgs.Data));
            }
            else
            {
                this.SetStateInfo(new PSInvocationStateInfo(PSInvocationState.Failed, eventArgs.Data));
            }
        }

        private void HandleCloseCompleted(object sender, EventArgs args)
        {
            this.dataStructureHandler.RaiseRemoveAssociationEvent();
            if (this.stateInfoQueue.Count != 0)
            {
                while (this.stateInfoQueue.Count > 0)
                {
                    PSInvocationStateInfo stateInfo = this.stateInfoQueue.Dequeue();
                    this.SetStateInfo(stateInfo);
                }
            }
            else if (!this.IsFinished(this.shell.InvocationStateInfo.State))
            {
                RemoteSessionStateEventArgs args2 = args as RemoteSessionStateEventArgs;
                Exception reason = (args2 != null) ? args2.SessionStateInfo.Reason : null;
                PSInvocationState state = (this.shell.InvocationStateInfo.State == PSInvocationState.Disconnected) ? PSInvocationState.Failed : PSInvocationState.Stopped;
                this.SetStateInfo(new PSInvocationStateInfo(state, reason));
            }
        }

        private void HandleCloseNotificationFromRunspacePool(object sender, RemoteDataEventArgs<Exception> eventArgs)
        {
            this.UnblockCollections();
            this.dataStructureHandler.RaiseRemoveAssociationEvent();
            this.SetStateInfo(new PSInvocationStateInfo(PSInvocationState.Stopped, eventArgs.Data));
        }

        private void HandleConnectCompleted(object sender, RemoteDataEventArgs<Exception> e)
        {
            this.SetStateInfo(new PSInvocationStateInfo(PSInvocationState.Running, null));
        }

        private void HandleErrorReceived(object sender, RemoteDataEventArgs<ErrorRecord> eventArgs)
        {
            using (tracer.TraceEventHandlers())
            {
                this.shell.SetHadErrors(true);
                this.errorstream.Write(eventArgs.Data);
            }
        }

        private void HandleHostCallReceived(object sender, RemoteDataEventArgs<RemoteHostCall> eventArgs)
        {
            using (tracer.TraceEventHandlers())
            {
                Collection<RemoteHostCall> collection = eventArgs.Data.PerformSecurityChecksOnHostMessage(this.computerName);
                if (this.HostCallReceived != null)
                {
                    if (collection.Count > 0)
                    {
                        foreach (RemoteHostCall call in collection)
                        {
                            RemoteDataEventArgs<RemoteHostCall> args = new RemoteDataEventArgs<RemoteHostCall>(call);
                            this.HostCallReceived.SafeInvoke<RemoteDataEventArgs<RemoteHostCall>>(this, args);
                        }
                    }
                    this.HostCallReceived.SafeInvoke<RemoteDataEventArgs<RemoteHostCall>>(this, eventArgs);
                }
                else
                {
                    if (collection.Count > 0)
                    {
                        foreach (RemoteHostCall call2 in collection)
                        {
                            this.ExecuteHostCall(call2);
                        }
                    }
                    this.ExecuteHostCall(eventArgs.Data);
                }
            }
        }

        private void HandleInformationalMessageReceived(object sender, RemoteDataEventArgs<InformationalMessage> eventArgs)
        {
            using (tracer.TraceEventHandlers())
            {
                InformationalMessage data = eventArgs.Data;
                switch (data.DataType)
                {
                    case RemotingDataType.PowerShellDebug:
                        this.informationalBuffers.AddDebug((DebugRecord) data.Message);
                        return;

                    case RemotingDataType.PowerShellVerbose:
                        this.informationalBuffers.AddVerbose((VerboseRecord) data.Message);
                        return;

                    case RemotingDataType.PowerShellWarning:
                        this.informationalBuffers.AddWarning((WarningRecord) data.Message);
                        return;

                    case RemotingDataType.PowerShellProgress:
                        break;

                    default:
                        return;
                }
                ProgressRecord item = (ProgressRecord) LanguagePrimitives.ConvertTo(data.Message, typeof(ProgressRecord), CultureInfo.InvariantCulture);
                this.informationalBuffers.AddProgress(item);
            }
        }

        private void HandleInvocationStateInfoReceived(object sender, RemoteDataEventArgs<PSInvocationStateInfo> eventArgs)
        {
            using (tracer.TraceEventHandlers())
            {
                PSInvocationStateInfo data = eventArgs.Data;
                if (data.State == PSInvocationState.Disconnected)
                {
                    this.SetStateInfo(data);
                }
                else if (((data.State == PSInvocationState.Stopped) || (data.State == PSInvocationState.Failed)) || (data.State == PSInvocationState.Completed))
                {
                    this.UnblockCollections();
                    if (this.stopCalled)
                    {
                        this.stopCalled = false;
                        this.stateInfoQueue.Enqueue(new PSInvocationStateInfo(PSInvocationState.Stopped, data.Reason));
                        this.CheckAndCloseRunspaceAfterStop(data.Reason);
                    }
                    else
                    {
                        this.stateInfoQueue.Enqueue(data);
                    }
                    this.dataStructureHandler.CloseConnectionAsync(null);
                }
            }
        }

        private void HandleOutputReceived(object sender, RemoteDataEventArgs<object> eventArgs)
        {
            using (tracer.TraceEventHandlers())
            {
                object data = eventArgs.Data;
                try
                {
                    this.outputstream.Write(data);
                }
                catch (PSInvalidCastException exception)
                {
                    this.shell.SetStateChanged(new PSInvocationStateInfo(PSInvocationState.Failed, exception));
                }
            }
        }

        private void HandleRobustConnectionNotification(object sender, ConnectionStatusEventArgs e)
        {
            PSConnectionRetryStatusEventArgs eventArgs = null;
            WarningRecord infoRecord = null;
            ErrorRecord record2 = null;
            int maxRetryConnectionTime = this.runspacePool.MaxRetryConnectionTime;
            int num2 = maxRetryConnectionTime / 0xea60;
            switch (e.Notification)
            {
                case ConnectionStatus.NetworkFailureDetected:
                    infoRecord = new WarningRecord("PowerShellNetworkFailureDetected", StringUtil.Format(RemotingErrorIdStrings.RCNetworkFailureDetected, this.computerName, num2));
                    eventArgs = new PSConnectionRetryStatusEventArgs(PSConnectionRetryStatus.NetworkFailureDetected, this.computerName, maxRetryConnectionTime, infoRecord);
                    break;

                case ConnectionStatus.ConnectionRetryAttempt:
                    infoRecord = new WarningRecord("PowerShellConnectionRetryAttempt", StringUtil.Format(RemotingErrorIdStrings.RCConnectionRetryAttempt, this.computerName));
                    eventArgs = new PSConnectionRetryStatusEventArgs(PSConnectionRetryStatus.ConnectionRetryAttempt, this.computerName, maxRetryConnectionTime, infoRecord);
                    break;

                case ConnectionStatus.ConnectionRetrySucceeded:
                    infoRecord = new WarningRecord("PowerShellConnectionRetrySucceeded", StringUtil.Format(RemotingErrorIdStrings.RCReconnectSucceeded, this.computerName));
                    eventArgs = new PSConnectionRetryStatusEventArgs(PSConnectionRetryStatus.ConnectionRetrySucceeded, this.computerName, num2, infoRecord);
                    break;

                case ConnectionStatus.AutoDisconnectStarting:
                    infoRecord = new WarningRecord("PowerShellNetworkFailedStartDisconnect", StringUtil.Format(RemotingErrorIdStrings.RCAutoDisconnectingWarning, this.computerName));
                    eventArgs = new PSConnectionRetryStatusEventArgs(PSConnectionRetryStatus.AutoDisconnectStarting, this.computerName, num2, infoRecord);
                    break;

                case ConnectionStatus.AutoDisconnectSucceeded:
                    infoRecord = new WarningRecord("PowerShellAutoDisconnectSucceeded", StringUtil.Format(RemotingErrorIdStrings.RCAutoDisconnected, this.computerName));
                    eventArgs = new PSConnectionRetryStatusEventArgs(PSConnectionRetryStatus.AutoDisconnectSucceeded, this.computerName, num2, infoRecord);
                    break;

                case ConnectionStatus.InternalErrorAbort:
                {
                    RuntimeException exception = new RuntimeException(StringUtil.Format(RemotingErrorIdStrings.RCInternalError, this.computerName));
                    record2 = new ErrorRecord(exception, "PowerShellNetworkOrDisconnectFailed", ErrorCategory.InvalidOperation, this);
                    eventArgs = new PSConnectionRetryStatusEventArgs(PSConnectionRetryStatus.InternalErrorAbort, this.computerName, num2, record2);
                    break;
                }
            }
            if (eventArgs != null)
            {
                this.connectionRetryStatus = eventArgs.Notification;
                if (infoRecord != null)
                {
                    RemotingWarningRecord message = new RemotingWarningRecord(infoRecord, new OriginInfo(this.computerName, this.InstanceId));
                    this.HandleInformationalMessageReceived(this, new RemoteDataEventArgs<InformationalMessage>(new InformationalMessage(message, RemotingDataType.PowerShellWarning)));
                    RemoteHostCall data = new RemoteHostCall(-100L, RemoteHostMethodId.WriteWarningLine, new object[] { infoRecord.Message });
                    try
                    {
                        this.HandleHostCallReceived(this, new RemoteDataEventArgs<RemoteHostCall>(data));
                    }
                    catch (PSNotImplementedException)
                    {
                    }
                }
                if (record2 != null)
                {
                    RemotingErrorRecord record4 = new RemotingErrorRecord(record2, new OriginInfo(this.computerName, this.InstanceId));
                    this.HandleErrorReceived(this, new RemoteDataEventArgs<ErrorRecord>(record4));
                }
                this.RCConnectionNotification.SafeInvoke<PSConnectionRetryStatusEventArgs>(this, eventArgs);
            }
        }

        internal void Initialize(ObjectStreamBase inputstream, ObjectStreamBase outputstream, ObjectStreamBase errorstream, PSInformationalBuffers informationalBuffers, PSInvocationSettings settings)
        {
            this.initialized = true;
            this.informationalBuffers = informationalBuffers;
            this.InputStream = inputstream;
            this.errorstream = errorstream;
            this.outputstream = outputstream;
            this.settings = settings;
            if ((settings == null) || (settings.Host == null))
            {
                this.hostToUse = this.runspacePool.Host;
            }
            else
            {
                this.hostToUse = settings.Host;
            }
            this.dataStructureHandler = this.runspacePool.DataStructureHandler.CreatePowerShellDataStructureHandler(this);
            this.dataStructureHandler.InvocationStateInfoReceived += new EventHandler<RemoteDataEventArgs<PSInvocationStateInfo>>(this.HandleInvocationStateInfoReceived);
            this.dataStructureHandler.OutputReceived += new EventHandler<RemoteDataEventArgs<object>>(this.HandleOutputReceived);
            this.dataStructureHandler.ErrorReceived += new EventHandler<RemoteDataEventArgs<ErrorRecord>>(this.HandleErrorReceived);
            this.dataStructureHandler.InformationalMessageReceived += new EventHandler<RemoteDataEventArgs<InformationalMessage>>(this.HandleInformationalMessageReceived);
            this.dataStructureHandler.HostCallReceived += new EventHandler<RemoteDataEventArgs<RemoteHostCall>>(this.HandleHostCallReceived);
            this.dataStructureHandler.ClosedNotificationFromRunspacePool += new EventHandler<RemoteDataEventArgs<Exception>>(this.HandleCloseNotificationFromRunspacePool);
            this.dataStructureHandler.BrokenNotificationFromRunspacePool += new EventHandler<RemoteDataEventArgs<Exception>>(this.HandleBrokenNotificationFromRunspacePool);
            this.dataStructureHandler.ConnectCompleted += new EventHandler<RemoteDataEventArgs<Exception>>(this.HandleConnectCompleted);
            this.dataStructureHandler.ReconnectCompleted += new EventHandler<RemoteDataEventArgs<Exception>>(this.HandleConnectCompleted);
            this.dataStructureHandler.RobustConnectionNotification += new EventHandler<ConnectionStatusEventArgs>(this.HandleRobustConnectionNotification);
            this.dataStructureHandler.CloseCompleted += new EventHandler<EventArgs>(this.HandleCloseCompleted);
        }

        private bool IsFinished(PSInvocationState state)
        {
            if ((state != PSInvocationState.Completed) && (state != PSInvocationState.Failed))
            {
                return (state == PSInvocationState.Stopped);
            }
            return true;
        }

        internal void SendInput()
        {
            this.dataStructureHandler.SendInput(this.inputstream);
        }

        internal void SetStateInfo(PSInvocationStateInfo stateInfo)
        {
            this.shell.SetStateChanged(stateInfo);
        }

        internal void StopAsync()
        {
            PSConnectionRetryStatus connectionRetryStatus = this.connectionRetryStatus;
            if (((connectionRetryStatus == PSConnectionRetryStatus.NetworkFailureDetected) || (connectionRetryStatus == PSConnectionRetryStatus.ConnectionRetryAttempt)) && (this.runspacePool.RunspacePoolStateInfo.State == RunspacePoolState.Opened))
            {
                this.runspacePool.BeginDisconnect(null, null);
            }
            else
            {
                this.stopCalled = true;
                this.dataStructureHandler.SendStopPowerShellMessage();
            }
        }

        internal void UnblockCollections()
        {
            this.shell.ClearRemotePowerShell();
            this.outputstream.Close();
            this.errorstream.Close();
            if (this.inputstream != null)
            {
                this.inputstream.Close();
            }
        }

        internal PSConnectionRetryStatus ConnectionRetryStatus
        {
            get
            {
                return this.connectionRetryStatus;
            }
        }

        internal ClientPowerShellDataStructureHandler DataStructureHandler
        {
            get
            {
                return this.dataStructureHandler;
            }
        }

        internal bool Initialized
        {
            get
            {
                return this.initialized;
            }
        }

        internal ObjectStreamBase InputStream
        {
            get
            {
                return this.inputstream;
            }
            set
            {
                this.inputstream = value;
                if ((this.inputstream != null) && (this.inputstream.IsOpen || (this.inputstream.Count > 0)))
                {
                    this.noInput = false;
                }
                else
                {
                    this.noInput = true;
                }
            }
        }

        internal Guid InstanceId
        {
            get
            {
                return this.PowerShell.InstanceId;
            }
        }

        internal bool NoInput
        {
            get
            {
                return this.noInput;
            }
        }

        internal ObjectStreamBase OutputStream
        {
            get
            {
                return this.outputstream;
            }
            set
            {
                this.outputstream = value;
            }
        }

        internal System.Management.Automation.PowerShell PowerShell
        {
            get
            {
                return this.shell;
            }
        }

        internal PSInvocationSettings Settings
        {
            get
            {
                return this.settings;
            }
        }
    }
}

