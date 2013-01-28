namespace System.Management.Automation.Remoting.Server
{
    using System;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Tracing;
    using System.Security.Principal;
    using System.Threading;

    internal sealed class OutOfProcessMediator
    {
        private string _initialCommand;
        private int _inProgressCommandsCount;
        private object _syncObject = new object();
        private ManualResetEvent allcmdsClosedEvent;
        private OutOfProcessUtils.DataProcessingDelegates callbacks;
        private OutOfProcessTextWriter originalStdErr;
        private TextReader originalStdIn = Console.In;
        private OutOfProcessTextWriter originalStdOut;
        private OutOfProcessServerSessionTransportManager sessionTM;
        private static OutOfProcessMediator SingletonInstance;
        private static object SyncObject = new object();
        private PowerShellTraceSource tracer = PowerShellTraceSourceFactory.GetTraceSource();

        private OutOfProcessMediator()
        {
            Console.SetIn(TextReader.Null);
            this.originalStdOut = new OutOfProcessTextWriter(Console.Out);
            Console.SetOut(TextWriter.Null);
            this.originalStdErr = new OutOfProcessTextWriter(Console.Error);
            Console.SetError(TextWriter.Null);
            this.callbacks = new OutOfProcessUtils.DataProcessingDelegates();
            this.callbacks.DataPacketReceived = (OutOfProcessUtils.DataPacketReceived) Delegate.Combine(this.callbacks.DataPacketReceived, new OutOfProcessUtils.DataPacketReceived(this.OnDataPacketReceived));
            this.callbacks.DataAckPacketReceived = (OutOfProcessUtils.DataAckPacketReceived) Delegate.Combine(this.callbacks.DataAckPacketReceived, new OutOfProcessUtils.DataAckPacketReceived(this.OnDataAckPacketReceived));
            this.callbacks.CommandCreationPacketReceived = (OutOfProcessUtils.CommandCreationPacketReceived) Delegate.Combine(this.callbacks.CommandCreationPacketReceived, new OutOfProcessUtils.CommandCreationPacketReceived(this.OnCommandCreationPacketReceived));
            this.callbacks.CommandCreationAckReceived = (OutOfProcessUtils.CommandCreationAckReceived) Delegate.Combine(this.callbacks.CommandCreationAckReceived, new OutOfProcessUtils.CommandCreationAckReceived(this.OnCommandCreationAckReceived));
            this.callbacks.ClosePacketReceived = (OutOfProcessUtils.ClosePacketReceived) Delegate.Combine(this.callbacks.ClosePacketReceived, new OutOfProcessUtils.ClosePacketReceived(this.OnClosePacketReceived));
            this.callbacks.CloseAckPacketReceived = (OutOfProcessUtils.CloseAckPacketReceived) Delegate.Combine(this.callbacks.CloseAckPacketReceived, new OutOfProcessUtils.CloseAckPacketReceived(this.OnCloseAckPacketReceived));
            this.callbacks.SignalPacketReceived = (OutOfProcessUtils.SignalPacketReceived) Delegate.Combine(this.callbacks.SignalPacketReceived, new OutOfProcessUtils.SignalPacketReceived(this.OnSignalPacketReceived));
            this.callbacks.SignalAckPacketReceived = (OutOfProcessUtils.SignalAckPacketReceived) Delegate.Combine(this.callbacks.SignalAckPacketReceived, new OutOfProcessUtils.SignalAckPacketReceived(this.OnSignalAckPacketReceived));
            this.allcmdsClosedEvent = new ManualResetEvent(true);
        }

        internal static void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception exceptionObject = (Exception) args.ExceptionObject;
            PSEtwLog.LogOperationalError(PSEventId.AppDomainUnhandledException, PSOpcode.Close, PSTask.None, PSKeyword.UseAlwaysOperational, new object[] { exceptionObject.GetType().ToString(), exceptionObject.Message, exceptionObject.StackTrace });
            PSEtwLog.LogAnalyticError(PSEventId.AppDomainUnhandledException_Analytic, PSOpcode.Close, PSTask.None, PSKeyword.ManagedPlugin | PSKeyword.UseAlwaysAnalytic, new object[] { exceptionObject.GetType().ToString(), exceptionObject.Message, exceptionObject.StackTrace });
        }

        private OutOfProcessServerSessionTransportManager CreateSessionTransportManager()
        {
            WindowsIdentity current = WindowsIdentity.GetCurrent();
            PSPrincipal userPrincipal = new PSPrincipal(new PSIdentity("", true, current.Name, null), current);
            PSSenderInfo senderInfo = new PSSenderInfo(userPrincipal, "http://localhost");
            OutOfProcessServerSessionTransportManager transportManager = new OutOfProcessServerSessionTransportManager(this.originalStdOut);
            ServerRemoteSession.CreateServerRemoteSession(senderInfo, this._initialCommand, transportManager);
            return transportManager;
        }

        private void OnCloseAckPacketReceived(Guid psGuid)
        {
            throw new PSRemotingTransportException(PSRemotingErrorId.IPCUnknownElementReceived, RemotingErrorIdStrings.IPCUnknownElementReceived, new object[] { "CloseAck" });
        }

        private void OnClosePacketReceived(Guid psGuid)
        {
            PowerShellTraceSource traceSource = PowerShellTraceSourceFactory.GetTraceSource();
            if (psGuid == Guid.Empty)
            {
                traceSource.WriteMessage("BEGIN calling close on session transport manager");
                bool flag = false;
                lock (this._syncObject)
                {
                    if (this._inProgressCommandsCount > 0)
                    {
                        flag = true;
                    }
                }
                if (flag)
                {
                    this.allcmdsClosedEvent.WaitOne();
                }
                lock (this._syncObject)
                {
                    traceSource.WriteMessage(string.Concat(new object[] { "OnClosePacketReceived, in progress commands count should be zero : ", this._inProgressCommandsCount, ", psGuid : ", psGuid.ToString() }));
                    if (this.sessionTM != null)
                    {
                        this.sessionTM.Close(null);
                    }
                    traceSource.WriteMessage("END calling close on session transport manager");
                    this.sessionTM = null;
                    goto Label_01D5;
                }
            }
            traceSource.WriteMessage("Closing command with GUID " + psGuid.ToString());
            AbstractServerTransportManager commandTransportManager = null;
            lock (this._syncObject)
            {
                commandTransportManager = this.sessionTM.GetCommandTransportManager(psGuid);
            }
            if (commandTransportManager != null)
            {
                commandTransportManager.Close(null);
            }
            lock (this._syncObject)
            {
                traceSource.WriteMessage(string.Concat(new object[] { "OnClosePacketReceived, in progress commands count should be greater than zero : ", this._inProgressCommandsCount, ", psGuid : ", psGuid.ToString() }));
                this._inProgressCommandsCount--;
                if (this._inProgressCommandsCount == 0)
                {
                    this.allcmdsClosedEvent.Set();
                }
            }
        Label_01D5:
            this.originalStdOut.WriteLine(OutOfProcessUtils.CreateCloseAckPacket(psGuid));
        }

        private void OnCommandCreationAckReceived(Guid psGuid)
        {
            throw new PSRemotingTransportException(PSRemotingErrorId.IPCUnknownElementReceived, RemotingErrorIdStrings.IPCUnknownElementReceived, new object[] { "CommandAck" });
        }

        private void OnCommandCreationPacketReceived(Guid psGuid)
        {
            lock (this._syncObject)
            {
                this.sessionTM.CreateCommandTransportManager(psGuid);
                if (this._inProgressCommandsCount == 0)
                {
                    this.allcmdsClosedEvent.Reset();
                }
                this._inProgressCommandsCount++;
                this.tracer.WriteMessage(string.Concat(new object[] { "OutOfProcessMediator.OnCommandCreationPacketReceived, in progress command count : ", this._inProgressCommandsCount, " psGuid : ", psGuid.ToString() }));
            }
        }

        private void OnDataAckPacketReceived(Guid psGuid)
        {
            throw new PSRemotingTransportException(PSRemotingErrorId.IPCUnknownElementReceived, RemotingErrorIdStrings.IPCUnknownElementReceived, new object[] { "DataAck" });
        }

        private void OnDataPacketReceived(byte[] rawData, string stream, Guid psGuid)
        {
            string str = "stdin";
            if (stream.Equals(DataPriorityType.PromptResponse.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                str = "pr";
            }
            if (Guid.Empty == psGuid)
            {
                lock (this._syncObject)
                {
                    this.sessionTM.ProcessRawData(rawData, str);
                    return;
                }
            }
            AbstractServerTransportManager commandTransportManager = null;
            lock (this._syncObject)
            {
                commandTransportManager = this.sessionTM.GetCommandTransportManager(psGuid);
            }
            if (commandTransportManager != null)
            {
                commandTransportManager.ProcessRawData(rawData, str);
            }
            else
            {
                this.originalStdOut.WriteLine(OutOfProcessUtils.CreateDataAckPacket(psGuid));
            }
        }

        private void OnSignalAckPacketReceived(Guid psGuid)
        {
            throw new PSRemotingTransportException(PSRemotingErrorId.IPCUnknownElementReceived, RemotingErrorIdStrings.IPCUnknownElementReceived, new object[] { "SignalAck" });
        }

        private void OnSignalPacketReceived(Guid psGuid)
        {
            if (psGuid == Guid.Empty)
            {
                throw new PSRemotingTransportException(PSRemotingErrorId.IPCNoSignalForSession, RemotingErrorIdStrings.IPCNoSignalForSession, new object[] { "Signal" });
            }
            AbstractServerTransportManager commandTransportManager = null;
            lock (this._syncObject)
            {
                commandTransportManager = this.sessionTM.GetCommandTransportManager(psGuid);
            }
            if (commandTransportManager != null)
            {
                commandTransportManager.Close(null);
            }
            this.originalStdOut.WriteLine(OutOfProcessUtils.CreateSignalAckPacket(psGuid));
        }

        private void ProcessingThreadStart(object state)
        {
            try
            {
                string data = state as string;
                OutOfProcessUtils.ProcessData(data, this.callbacks);
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                PSEtwLog.LogOperationalError(PSEventId.TransportError, PSOpcode.Open, PSTask.None, PSKeyword.UseAlwaysOperational, new object[] { Guid.Empty.ToString(), Guid.Empty.ToString(), 0xfa0, exception.Message, exception.StackTrace });
                PSEtwLog.LogAnalyticError(PSEventId.TransportError_Analytic, PSOpcode.Open, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { Guid.Empty.ToString(), Guid.Empty.ToString(), 0xfa0, exception.Message, exception.StackTrace });
                this.originalStdErr.WriteLine(exception.Message + exception.StackTrace);
                Environment.Exit(0xfa0);
            }
        }

        internal static void Run(string initialCommand)
        {
            lock (SyncObject)
            {
                if (SingletonInstance != null)
                {
                    return;
                }
                SingletonInstance = new OutOfProcessMediator();
            }
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OutOfProcessMediator.AppDomainUnhandledException);
            SingletonInstance.Start(initialCommand);
        }

        private void Start(string initialCommand)
        {
            this._initialCommand = initialCommand;
            this.sessionTM = this.CreateSessionTransportManager();
            try
            {
                while (true)
                {
                    string str = this.originalStdIn.ReadLine();
                    lock (this._syncObject)
                    {
                        if (this.sessionTM == null)
                        {
                            this.sessionTM = this.CreateSessionTransportManager();
                        }
                    }
                    if (string.IsNullOrEmpty(str))
                    {
                        lock (this._syncObject)
                        {
                            this.sessionTM.Close(null);
                            this.sessionTM = null;
                        }
                        throw new PSRemotingTransportException(PSRemotingErrorId.IPCUnknownElementReceived, RemotingErrorIdStrings.IPCUnknownElementReceived, new object[] { string.Empty });
                    }
                    ThreadPool.QueueUserWorkItem(new WaitCallback(this.ProcessingThreadStart), str);
                }
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                PSEtwLog.LogOperationalError(PSEventId.TransportError, PSOpcode.Open, PSTask.None, PSKeyword.UseAlwaysOperational, new object[] { Guid.Empty.ToString(), Guid.Empty.ToString(), 0xfa0, exception.Message, exception.StackTrace });
                PSEtwLog.LogAnalyticError(PSEventId.TransportError_Analytic, PSOpcode.Open, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { Guid.Empty.ToString(), Guid.Empty.ToString(), 0xfa0, exception.Message, exception.StackTrace });
                this.originalStdErr.WriteLine(exception.Message);
                Environment.Exit(0xfa0);
            }
        }
    }
}

