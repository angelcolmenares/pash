namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    [Cmdlet("Disconnect", "PSSession", SupportsShouldProcess=true, DefaultParameterSetName="Session", HelpUri="http://go.microsoft.com/fwlink/?LinkID=210605", RemotingCapability=RemotingCapability.OwnedByCommand), OutputType(new Type[] { typeof(PSSession) })]
    public class DisconnectPSSessionCommand : PSRunspaceCmdlet, IDisposable
    {
        private ManualResetEvent operationsComplete = new ManualResetEvent(true);
        private PSSession[] remotePSSessionInfo;
        private System.Management.Automation.Remoting.PSSessionOption sessionOption;
        private ObjectStream stream = new ObjectStream();
        private int throttleLimit;
        private ThrottleManager throttleManager = new ThrottleManager();

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            this.throttleManager.ThrottleLimit = this.ThrottleLimit;
            this.throttleManager.ThrottleComplete += new EventHandler<EventArgs>(this.HandleThrottleDisconnectComplete);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.throttleManager.Dispose();
                this.operationsComplete.WaitOne();
                this.operationsComplete.Close();
                this.throttleManager.ThrottleComplete -= new EventHandler<EventArgs>(this.HandleThrottleDisconnectComplete);
                this.stream.Dispose();
            }
        }

        protected override void EndProcessing()
        {
            this.throttleManager.EndSubmitOperations();
            this.operationsComplete.WaitOne();
            while (!this.stream.ObjectReader.EndOfPipeline)
            {
                object obj2 = this.stream.ObjectReader.Read();
                base.WriteStreamObject((Action<Cmdlet>) obj2);
            }
        }

        private string GetLocalhostWithNetworkAccessEnabled(Dictionary<Guid, PSSession> psSessions)
        {
            StringBuilder builder = new StringBuilder();
            foreach (PSSession session in psSessions.Values)
            {
                WSManConnectionInfo connectionInfo = session.Runspace.ConnectionInfo as WSManConnectionInfo;
                if (connectionInfo.IsLocalhostAndNetworkAccess)
                {
                    builder.Append(session.Name + ", ");
                }
            }
            if (builder.Length > 0)
            {
                builder.Remove(builder.Length - 2, 2);
            }
            return builder.ToString();
        }

        private void HandleThrottleDisconnectComplete(object sender, EventArgs eventArgs)
        {
            this.stream.ObjectWriter.Close();
            this.operationsComplete.Set();
        }

        protected override void ProcessRecord()
        {
            List<IThrottleOperation> operations = new List<IThrottleOperation>();
            try
            {
                Dictionary<Guid, PSSession> matchingRunspaces;
                if (base.ParameterSetName == "Session")
                {
                    if ((this.remotePSSessionInfo == null) || (this.remotePSSessionInfo.Length == 0))
                    {
                        return;
                    }
                    matchingRunspaces = new Dictionary<Guid, PSSession>();
                    foreach (PSSession session in this.remotePSSessionInfo)
                    {
                        matchingRunspaces.Add(session.InstanceId, session);
                    }
                }
                else
                {
                    matchingRunspaces = base.GetMatchingRunspaces(false, true);
                }
                string localhostWithNetworkAccessEnabled = this.GetLocalhostWithNetworkAccessEnabled(matchingRunspaces);
                if (!string.IsNullOrEmpty(localhostWithNetworkAccessEnabled))
                {
                    base.WriteWarning(StringUtil.Format(RemotingErrorIdStrings.EnableNetworkAccessWarning, localhostWithNetworkAccessEnabled));
                }
                foreach (PSSession session2 in matchingRunspaces.Values)
                {
                    if (base.ShouldProcess(session2.Name, "Disconnect"))
                    {
                        if (session2.Runspace.RunspaceStateInfo.State == RunspaceState.Opened)
                        {
                            if (this.sessionOption != null)
                            {
                                session2.Runspace.ConnectionInfo.SetSessionOptions(this.sessionOption);
                            }
                            if (this.ValidateIdleTimeout(session2))
                            {
                                DisconnectRunspaceOperation item = new DisconnectRunspaceOperation(session2, this.stream);
                                operations.Add(item);
                            }
                        }
                        else if (session2.Runspace.RunspaceStateInfo.State != RunspaceState.Disconnected)
                        {
                            Exception exception = new RuntimeException(StringUtil.Format(RemotingErrorIdStrings.RunspaceCannotBeDisconnected, session2.Name));
                            ErrorRecord errorRecord = new ErrorRecord(exception, "CannotDisconnectSessionWhenNotOpened", ErrorCategory.InvalidOperation, session2);
                            base.WriteError(errorRecord);
                        }
                        else
                        {
                            base.WriteObject(session2);
                        }
                    }
                }
            }
            catch (PSRemotingDataStructureException)
            {
                this.operationsComplete.Set();
                throw;
            }
            catch (PSRemotingTransportException)
            {
                this.operationsComplete.Set();
                throw;
            }
            catch (RemoteException)
            {
                this.operationsComplete.Set();
                throw;
            }
            catch (InvalidRunspaceStateException)
            {
                this.operationsComplete.Set();
                throw;
            }
            if (operations.Count > 0)
            {
                this.operationsComplete.Reset();
                this.throttleManager.SubmitOperations(operations);
                foreach (object obj2 in this.stream.ObjectReader.NonBlockingRead())
                {
                    base.WriteStreamObject((Action<Cmdlet>) obj2);
                }
            }
        }

        protected override void StopProcessing()
        {
            this.stream.ObjectWriter.Close();
            this.throttleManager.StopAllOperations();
        }

        private bool ValidateIdleTimeout(PSSession session)
        {
            int idleTimeout = session.Runspace.ConnectionInfo.IdleTimeout;
            int maxIdleTimeout = session.Runspace.ConnectionInfo.MaxIdleTimeout;
            int num3 = 0xea60;
            if ((idleTimeout == -1) || ((idleTimeout <= maxIdleTimeout) && (idleTimeout >= num3)))
            {
                return true;
            }
            ErrorRecord errorRecord = new ErrorRecord(new RuntimeException(StringUtil.Format(RemotingErrorIdStrings.CannotDisconnectSessionWithInvalidIdleTimeout, new object[] { session.Name, idleTimeout / 0x3e8, maxIdleTimeout / 0x3e8, num3 / 0x3e8 })), "CannotDisconnectSessionWithInvalidIdleTimeout", ErrorCategory.InvalidArgument, session);
            base.WriteError(errorRecord);
            return false;
        }

        public override string[] ComputerName { get; set; }

        [Parameter(ParameterSetName="Session"), Parameter(ParameterSetName="Name"), Parameter(ParameterSetName="Id"), ValidateRange(0, 0x7fffffff), Parameter(ParameterSetName="InstanceId")]
        public int IdleTimeoutSec
        {
            get
            {
                return this.PSSessionOption.IdleTimeout.Seconds;
            }
            set
            {
                this.PSSessionOption.IdleTimeout = TimeSpan.FromSeconds((double) value);
            }
        }

        [Parameter(ParameterSetName="Session"), Parameter(ParameterSetName="InstanceId"), Parameter(ParameterSetName="Name"), Parameter(ParameterSetName="Id")]
        public System.Management.Automation.Runspaces.OutputBufferingMode OutputBufferingMode
        {
            get
            {
                return this.PSSessionOption.OutputBufferingMode;
            }
            set
            {
                this.PSSessionOption.OutputBufferingMode = value;
            }
        }

        private System.Management.Automation.Remoting.PSSessionOption PSSessionOption
        {
            get
            {
                if (this.sessionOption == null)
                {
                    this.sessionOption = new System.Management.Automation.Remoting.PSSessionOption();
                }
                return this.sessionOption;
            }
        }

        [Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, ValueFromPipeline=true, ParameterSetName="Session"), ValidateNotNullOrEmpty]
        public PSSession[] Session
        {
            get
            {
                return this.remotePSSessionInfo;
            }
            set
            {
                this.remotePSSessionInfo = value;
            }
        }

        [Parameter(ParameterSetName="Session"), Parameter(ParameterSetName="InstanceId"), Parameter(ParameterSetName="Name"), Parameter(ParameterSetName="Id")]
        public int ThrottleLimit
        {
            get
            {
                return this.throttleLimit;
            }
            set
            {
                this.throttleLimit = value;
            }
        }

        private class DisconnectRunspaceOperation : IThrottleOperation
        {
            private PSSession remoteSession;
            private ObjectStream writeStream;

            internal override event EventHandler<OperationStateEventArgs> OperationComplete;

            internal DisconnectRunspaceOperation(PSSession session, ObjectStream stream)
            {
                this.remoteSession = session;
                this.writeStream = stream;
                this.remoteSession.Runspace.StateChanged += new EventHandler<RunspaceStateEventArgs>(this.StateCallBackHandler);
            }

            private void SendStartComplete()
            {
                OperationStateEventArgs eventArgs = new OperationStateEventArgs {
                    OperationState = OperationState.StartComplete
                };
                this.OperationComplete.SafeInvoke<OperationStateEventArgs>(this, eventArgs);
            }

            private void SendStopComplete()
            {
                OperationStateEventArgs eventArgs = new OperationStateEventArgs {
                    OperationState = OperationState.StopComplete
                };
                this.OperationComplete.SafeInvoke<OperationStateEventArgs>(this, eventArgs);
            }

            internal override void StartOperation()
            {
                bool flag = true;
                try
                {
                    this.remoteSession.Runspace.DisconnectAsync();
                }
                catch (InvalidRunspacePoolStateException exception)
                {
                    flag = false;
                    this.WriteDisconnectFailed(exception);
                }
                catch (PSInvalidOperationException exception2)
                {
                    flag = false;
                    this.WriteDisconnectFailed(exception2);
                }
                if (!flag)
                {
                    this.remoteSession.Runspace.StateChanged -= new EventHandler<RunspaceStateEventArgs>(this.StateCallBackHandler);
                    this.SendStartComplete();
                }
            }

            private void StateCallBackHandler(object sender, RunspaceStateEventArgs eArgs)
            {
                if (eArgs.RunspaceStateInfo.State != RunspaceState.Disconnecting)
                {
                    if (eArgs.RunspaceStateInfo.State == RunspaceState.Disconnected)
                    {
                        this.WriteDisconnectedPSSession();
                    }
                    else
                    {
                        this.WriteDisconnectFailed(null);
                    }
                    this.remoteSession.Runspace.StateChanged -= new EventHandler<RunspaceStateEventArgs>(this.StateCallBackHandler);
                    this.SendStartComplete();
                }
            }

            internal override void StopOperation()
            {
                this.remoteSession.Runspace.StateChanged -= new EventHandler<RunspaceStateEventArgs>(this.StateCallBackHandler);
                this.SendStopComplete();
            }

            private void WriteDisconnectedPSSession()
            {
                Action<Cmdlet> action2 = null;
                if (this.writeStream.ObjectWriter.IsOpen)
                {
                    if (action2 == null)
                    {
                        action2 = cmdlet => cmdlet.WriteObject(this.remoteSession);
                    }
                    Action<Cmdlet> action = action2;
                    this.writeStream.ObjectWriter.Write(action);
                }
            }

            private void WriteDisconnectFailed(Exception e = null)
            {
                if (this.writeStream.ObjectWriter.IsOpen)
                {
                    string str;
                    if ((e != null) && !string.IsNullOrWhiteSpace(e.Message))
                    {
                        str = StringUtil.Format(RemotingErrorIdStrings.RunspaceDisconnectFailedWithReason, this.remoteSession.InstanceId, e.Message);
                    }
                    else
                    {
                        str = StringUtil.Format(RemotingErrorIdStrings.RunspaceDisconnectFailed, this.remoteSession.InstanceId);
                    }
                    Exception exception = new RuntimeException(str, e);
                    ErrorRecord errorRecord = new ErrorRecord(exception, "PSSessionDisconnectFailed", ErrorCategory.InvalidOperation, this.remoteSession);
                    Action<Cmdlet> action = cmdlet => cmdlet.WriteError(errorRecord);
                    this.writeStream.ObjectWriter.Write(action);
                }
            }
        }
    }
}

