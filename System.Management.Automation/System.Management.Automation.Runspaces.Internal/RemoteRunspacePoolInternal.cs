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
    using System.Management.Automation.Tracing;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class RemoteRunspacePoolInternal : RunspacePoolInternal, IDisposable
    {
        private PSPrimitiveDictionary applicationArguments;
        private PSPrimitiveDictionary applicationPrivateData;
        private ManualResetEvent applicationPrivateDataReceived;
        private bool availableForConnection;
        private bool canReconnect;
        private RunspacePoolAsyncResult closeAsyncResult;
        private Exception closingReason;
        private ConnectCommandInfo[] connectCommands;
        private RunspaceConnectionInfo connectionInfo;
        private ClientRunspacePoolDataStructureHandler dataStructureHandler;
        private RunspacePoolAsyncResult disconnectAsyncResult;
        private DispatchTable<object> dispatchTable;
        private string friendlyName;
        private bool isDisposed;
        private RunspacePoolAsyncResult openAsyncResult;
        private RunspacePoolAsyncResult reconnectAsyncResult;

        internal event EventHandler<RemoteDataEventArgs<RemoteHostCall>> HostCallReceived;

        internal event EventHandler<CreateCompleteEventArgs> SessionCreateCompleted;

        internal event EventHandler<RemoteDataEventArgs<Uri>> URIRedirectionReported;

        internal RemoteRunspacePoolInternal(Guid instanceId, string name, bool isDisconnected, ConnectCommandInfo[] connectCommands, RunspaceConnectionInfo connectionInfo, PSHost host, TypeTable typeTable) : base(1, 1)
        {
            this.applicationPrivateDataReceived = new ManualResetEvent(false);
            this.friendlyName = string.Empty;
            if (connectCommands == null)
            {
                throw PSTraceSource.NewArgumentNullException("ConnectCommandInfo[]");
            }
            if (connectionInfo == null)
            {
                throw PSTraceSource.NewArgumentNullException("WSManConnectionInfo");
            }
            if (connectionInfo is WSManConnectionInfo)
            {
                this.connectionInfo = ((WSManConnectionInfo) connectionInfo).Copy();
            }
            base.instanceId = instanceId;
            base.minPoolSz = -1;
            base.maxPoolSz = -1;
            PSEtwLog.LogOperationalVerbose(PSEventId.RunspacePoolConstructor, PSOpcode.Constructor, PSTask.CreateRunspace, PSKeyword.UseAlwaysOperational, new object[] { instanceId.ToString(), this.minPoolSz.ToString(CultureInfo.InvariantCulture), this.maxPoolSz.ToString(CultureInfo.InvariantCulture) });
            this.connectCommands = connectCommands;
            this.Name = name;
            base.host = host;
            this.dispatchTable = new DispatchTable<object>();
            this.SetRunspacePoolState(new RunspacePoolStateInfo(RunspacePoolState.Disconnected, null));
            this.CreateDSHandler(typeTable);
            this.availableForConnection = isDisconnected;
        }

        internal RemoteRunspacePoolInternal(int minRunspaces, int maxRunspaces, TypeTable typeTable, PSHost host, PSPrimitiveDictionary applicationArguments, RunspaceConnectionInfo connectionInfo, string name = null) : base(minRunspaces, maxRunspaces)
        {
            this.applicationPrivateDataReceived = new ManualResetEvent(false);
            this.friendlyName = string.Empty;
            if (connectionInfo == null)
            {
                throw PSTraceSource.NewArgumentNullException("WSManConnectionInfo");
            }
            PSEtwLog.LogOperationalVerbose(PSEventId.RunspacePoolConstructor, PSOpcode.Constructor, PSTask.CreateRunspace, PSKeyword.UseAlwaysOperational, new object[] { this.instanceId.ToString(), this.minPoolSz.ToString(CultureInfo.InvariantCulture), this.maxPoolSz.ToString(CultureInfo.InvariantCulture) });
            if (connectionInfo is WSManConnectionInfo)
            {
                this.connectionInfo = ((WSManConnectionInfo) connectionInfo).Copy();
            }
            else if (connectionInfo is NewProcessConnectionInfo)
            {
                this.connectionInfo = ((NewProcessConnectionInfo) connectionInfo).Copy();
            }
            base.host = host;
            this.applicationArguments = applicationArguments;
            this.availableForConnection = false;
            this.dispatchTable = new DispatchTable<object>();
            if (!string.IsNullOrEmpty(name))
            {
                this.Name = name;
            }
            this.CreateDSHandler(typeTable);
        }

		public void SetHost (PSHost host)
		{
			base.host = host;
		}

        internal void AddRemotePowerShellDSHandler(Guid psShellInstanceId, ClientPowerShellDataStructureHandler psDSHandler)
        {
            this.dataStructureHandler.AddRemotePowerShellDSHandler(psShellInstanceId, psDSHandler);
        }

        public override IAsyncResult BeginClose(AsyncCallback callback, object asyncState)
        {
            bool flag = false;
            bool flag2 = false;
            RunspacePoolStateInfo stateInfo = new RunspacePoolStateInfo(RunspacePoolState.BeforeOpen, null);
            RunspacePoolAsyncResult closeAsyncResult = null;
            lock (base.syncObject)
            {
                if ((base.stateInfo.State == RunspacePoolState.Closed) || (base.stateInfo.State == RunspacePoolState.Broken))
                {
                    flag2 = true;
                    closeAsyncResult = new RunspacePoolAsyncResult(base.instanceId, callback, asyncState, false);
                }
                else if (base.stateInfo.State == RunspacePoolState.BeforeOpen)
                {
                    stateInfo = base.stateInfo = new RunspacePoolStateInfo(RunspacePoolState.Closed, null);
                    flag = true;
                    flag2 = true;
                    this.closeAsyncResult = null;
                    closeAsyncResult = new RunspacePoolAsyncResult(base.instanceId, callback, asyncState, false);
                }
                else if ((base.stateInfo.State == RunspacePoolState.Opened) || (base.stateInfo.State == RunspacePoolState.Opening))
                {
                    stateInfo = base.stateInfo = new RunspacePoolStateInfo(RunspacePoolState.Closing, null);
                    this.closeAsyncResult = new RunspacePoolAsyncResult(base.instanceId, callback, asyncState, false);
                    closeAsyncResult = this.closeAsyncResult;
                    flag = true;
                }
                else if (((base.stateInfo.State == RunspacePoolState.Disconnected) || (base.stateInfo.State == RunspacePoolState.Disconnecting)) || (base.stateInfo.State == RunspacePoolState.Connecting))
                {
                    this.closeAsyncResult = new RunspacePoolAsyncResult(base.instanceId, callback, asyncState, false);
                    closeAsyncResult = this.closeAsyncResult;
                }
                else if (base.stateInfo.State == RunspacePoolState.Closing)
                {
                    return this.closeAsyncResult;
                }
            }
            if (flag)
            {
                base.RaiseStateChangeEvent(stateInfo);
            }
            if (!flag2)
            {
                this.dataStructureHandler.CloseRunspacePoolAsync();
                return closeAsyncResult;
            }
            closeAsyncResult.SetAsCompleted(null);
            return closeAsyncResult;
        }

        public override IAsyncResult BeginConnect(AsyncCallback callback, object state)
        {
            RunspacePoolState state2;
            if (!this.AvailableForConnection)
            {
                throw PSTraceSource.NewInvalidOperationException("RunspacePoolStrings", "CannotConnect", new object[0]);
            }
            bool flag = false;
            lock (base.syncObject)
            {
                state2 = base.stateInfo.State;
                if (state2 == RunspacePoolState.Disconnected)
                {
                    RunspacePoolStateInfo newStateInfo = new RunspacePoolStateInfo(RunspacePoolState.Connecting, null);
                    this.SetRunspacePoolState(newStateInfo);
                    flag = true;
                }
            }
            if (flag)
            {
                base.RaiseStateChangeEvent(base.stateInfo);
            }
            flag = false;
            if (state2 == RunspacePoolState.Disconnected)
            {
                RunspacePoolAsyncResult reconnectAsyncResult;
                if (this.canReconnect)
                {
                    this.dataStructureHandler.ReconnectPoolAsync();
                    this.reconnectAsyncResult = new RunspacePoolAsyncResult(base.instanceId, callback, state, false);
                    reconnectAsyncResult = this.reconnectAsyncResult;
                }
                else
                {
                    this.dataStructureHandler.ConnectPoolAsync();
                    this.openAsyncResult = new RunspacePoolAsyncResult(base.instanceId, callback, state, false);
                    reconnectAsyncResult = this.openAsyncResult;
                }
                if (flag)
                {
                    base.RaiseStateChangeEvent(base.stateInfo);
                }
                return reconnectAsyncResult;
            }
            InvalidRunspacePoolStateException exception = new InvalidRunspacePoolStateException(StringUtil.Format(RunspacePoolStrings.InvalidRunspacePoolState, RunspacePoolState.Disconnected, base.stateInfo.State), base.stateInfo.State, RunspacePoolState.Disconnected);
            throw exception;
        }

        public override IAsyncResult BeginDisconnect(AsyncCallback callback, object state)
        {
            RunspacePoolState state2;
            if (!this.CanDisconnect)
            {
                throw PSTraceSource.NewInvalidOperationException("RunspacePoolStrings", "DisconnectNotSupportedOnServer", new object[0]);
            }
            bool flag = false;
            lock (base.syncObject)
            {
                state2 = base.stateInfo.State;
                if (state2 == RunspacePoolState.Opened)
                {
                    RunspacePoolStateInfo newStateInfo = new RunspacePoolStateInfo(RunspacePoolState.Disconnecting, null);
                    this.SetRunspacePoolState(newStateInfo);
                    flag = true;
                }
            }
            if (flag)
            {
                base.RaiseStateChangeEvent(base.stateInfo);
            }
            if (state2 == RunspacePoolState.Opened)
            {
                this.disconnectAsyncResult = new RunspacePoolAsyncResult(base.instanceId, callback, state, false);
                this.dataStructureHandler.DisconnectPoolAsync();
                return this.disconnectAsyncResult;
            }
            InvalidRunspacePoolStateException exception = new InvalidRunspacePoolStateException(StringUtil.Format(RunspacePoolStrings.InvalidRunspacePoolState, RunspacePoolState.Opened, base.stateInfo.State), base.stateInfo.State, RunspacePoolState.Opened);
            throw exception;
        }

        public override void Close()
        {
            IAsyncResult asyncResult = this.BeginClose(null, null);
            this.EndClose(asyncResult);
        }

        public override void Connect()
        {
            IAsyncResult asyncResult = this.BeginConnect(null, null);
            this.EndConnect(asyncResult);
        }

        protected override IAsyncResult CoreOpen(bool isAsync, AsyncCallback callback, object asyncState)
        {
            PSEtwLog.SetActivityIdForCurrentThread(base.InstanceId);
            PSEtwLog.LogOperationalVerbose(PSEventId.RunspacePoolOpen, PSOpcode.Open, PSTask.CreateRunspace, PSKeyword.UseAlwaysOperational, new object[0]);
            lock (base.syncObject)
            {
                base.AssertIfStateIsBeforeOpen();
                base.stateInfo = new RunspacePoolStateInfo(RunspacePoolState.Opening, null);
            }
            base.RaiseStateChangeEvent(base.stateInfo);
            RunspacePoolAsyncResult result = new RunspacePoolAsyncResult(base.instanceId, callback, asyncState, true);
            this.openAsyncResult = result;
            this.dataStructureHandler.CreateRunspacePoolAndOpenAsync();
            return result;
        }

        public override Collection<PowerShell> CreateDisconnectedPowerShells(RunspacePool runspacePool)
        {
            Collection<PowerShell> collection = new Collection<PowerShell>();
            if (this.ConnectCommands == null)
            {
                throw new InvalidRunspacePoolStateException(StringUtil.Format(RunspacePoolStrings.CannotReconstructCommands, this.Name));
            }
            foreach (ConnectCommandInfo info in this.ConnectCommands)
            {
                collection.Add(new PowerShell(info, runspacePool));
            }
            return collection;
        }

        private void CreateDSHandler(TypeTable typeTable)
        {
            this.dataStructureHandler = new ClientRunspacePoolDataStructureHandler(this, typeTable);
            this.dataStructureHandler.RemoteHostCallReceived += new EventHandler<RemoteDataEventArgs<RemoteHostCall>>(this.HandleRemoteHostCalls);
            this.dataStructureHandler.StateInfoReceived += new EventHandler<RemoteDataEventArgs<RunspacePoolStateInfo>>(this.HandleStateInfoReceived);
            this.dataStructureHandler.RSPoolInitInfoReceived += new EventHandler<RemoteDataEventArgs<RunspacePoolInitInfo>>(this.HandleInitInfoReceived);
            this.dataStructureHandler.ApplicationPrivateDataReceived += new EventHandler<RemoteDataEventArgs<PSPrimitiveDictionary>>(this.HandleApplicationPrivateDataReceived);
            this.dataStructureHandler.SessionClosing += new EventHandler<RemoteDataEventArgs<Exception>>(this.HandleSessionClosing);
            this.dataStructureHandler.SessionClosed += new EventHandler<RemoteDataEventArgs<Exception>>(this.HandleSessionClosed);
            this.dataStructureHandler.SetMaxMinRunspacesResponseRecieved += new EventHandler<RemoteDataEventArgs<PSObject>>(this.HandleResponseReceived);
            this.dataStructureHandler.URIRedirectionReported += new EventHandler<RemoteDataEventArgs<Uri>>(this.HandleURIDirectionReported);
            this.dataStructureHandler.PSEventArgsReceived += new EventHandler<RemoteDataEventArgs<PSEventArgs>>(this.HandlePSEventArgsReceived);
            this.dataStructureHandler.SessionDisconnected += new EventHandler<RemoteDataEventArgs<Exception>>(this.HandleSessionDisconnected);
            this.dataStructureHandler.SessionReconnected += new EventHandler<RemoteDataEventArgs<Exception>>(this.HandleSessionReconnected);
            this.dataStructureHandler.SessionRCDisconnecting += new EventHandler<RemoteDataEventArgs<Exception>>(this.HandleSessionRCDisconnecting);
            this.dataStructureHandler.SessionCreateCompleted += new EventHandler<CreateCompleteEventArgs>(this.HandleSessionCreateCompleted);
        }

        internal virtual void CreatePowerShellOnServerAndInvoke(ClientRemotePowerShell shell)
        {
            this.dataStructureHandler.CreatePowerShellOnServerAndInvoke(shell);
            if (!shell.NoInput)
            {
                shell.SendInput();
            }
        }

        public override void Disconnect()
        {
            IAsyncResult asyncResult = this.BeginDisconnect(null, null);
            this.EndDisconnect(asyncResult);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!this.isDisposed)
            {
                this.isDisposed = true;
                this.dataStructureHandler.Dispose(disposing);
                this.applicationPrivateDataReceived.Close();
            }
        }

        public override void EndConnect(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw PSTraceSource.NewArgumentNullException("asyncResult");
            }
            RunspacePoolAsyncResult result = asyncResult as RunspacePoolAsyncResult;
            if (((result == null) || (result.OwnerId != base.instanceId)) || result.IsAssociatedWithAsyncOpen)
            {
                throw PSTraceSource.NewArgumentException("asyncResult", RunspacePoolInternal.resBaseName, "AsyncResultNotOwned", new object[] { "IAsyncResult", "BeginOpen" });
            }
            result.EndInvoke();
        }

        public override void EndDisconnect(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw PSTraceSource.NewArgumentNullException("asyncResult");
            }
            RunspacePoolAsyncResult result = asyncResult as RunspacePoolAsyncResult;
            if (((result == null) || (result.OwnerId != base.instanceId)) || result.IsAssociatedWithAsyncOpen)
            {
                throw PSTraceSource.NewArgumentException("asyncResult", RunspacePoolInternal.resBaseName, "AsyncResultNotOwned", new object[] { "IAsyncResult", "BeginOpen" });
            }
            result.EndInvoke();
        }

        internal override PSPrimitiveDictionary GetApplicationPrivateData()
        {
            if ((base.RunspacePoolStateInfo.State == RunspacePoolState.Disconnected) && !this.applicationPrivateDataReceived.WaitOne(0, false))
            {
                return null;
            }
            return this.applicationPrivateData;
        }

        internal override int GetAvailableRunspaces()
        {
            long callId = 0L;
            lock (base.syncObject)
            {
                if (base.stateInfo.State == RunspacePoolState.Opened)
                {
                    callId = this.DispatchTable.CreateNewCallId();
                }
                else
                {
                    if ((base.stateInfo.State != RunspacePoolState.BeforeOpen) && (base.stateInfo.State != RunspacePoolState.Opening))
                    {
                        throw new InvalidOperationException(HostInterfaceExceptionsStrings.RunspacePoolNotOpened);
                    }
                    return base.maxPoolSz;
                }
                this.dataStructureHandler.SendGetAvailableRunspacesToServer(callId);
            }
            return (int) this.dispatchTable.GetResponse(callId, 0);
        }

        public override RunspacePoolCapability GetCapabilities()
        {
            RunspacePoolCapability capability = RunspacePoolCapability.Default;
            if (this.CanDisconnect)
            {
                capability |= RunspacePoolCapability.SupportsDisconnect;
            }
            return capability;
        }

        internal static RunspacePool[] GetRemoteRunspacePools(RunspaceConnectionInfo connectionInfo, PSHost host, TypeTable typeTable)
        {
            WSManConnectionInfo wsmanConnectionInfo = connectionInfo as WSManConnectionInfo;
            if (wsmanConnectionInfo == null)
            {
                throw new NotSupportedException();
            }
            List<RunspacePool> list = new List<RunspacePool>();
            foreach (PSObject obj2 in RemoteRunspacePoolEnumeration.GetRemotePools(wsmanConnectionInfo))
            {
                WSManConnectionInfo info2 = wsmanConnectionInfo.Copy();
                PSPropertyInfo info3 = obj2.Properties["ShellId"];
                PSPropertyInfo info4 = obj2.Properties["State"];
                PSPropertyInfo info5 = obj2.Properties["Name"];
                PSPropertyInfo info6 = obj2.Properties["ResourceUri"];
                if (((info3 != null) && (info4 != null)) && ((info5 != null) && (info6 != null)))
                {
                    string name = info5.Value.ToString();
                    string str2 = info6.Value.ToString();
                    bool isDisconnected = info4.Value.ToString().Equals("Disconnected", StringComparison.OrdinalIgnoreCase);
                    Guid shellId = Guid.Parse(info3.Value.ToString());
                    if (str2.StartsWith("http://schemas.microsoft.com/powershell/", StringComparison.OrdinalIgnoreCase))
                    {
                        Collection<PSObject> remoteCommands;
                        UpdateWSManConnectionInfo(info2, obj2);
                        info2.EnableNetworkAccess = true;
                        List<ConnectCommandInfo> list2 = new List<ConnectCommandInfo>();
                        try
                        {
                            remoteCommands = RemoteRunspacePoolEnumeration.GetRemoteCommands(shellId, info2);
                        }
                        catch (CmdletInvocationException exception)
                        {
                            if ((exception.InnerException == null) || !(exception.InnerException is InvalidOperationException))
                            {
                                throw;
                            }
                            continue;
                        }
                        foreach (PSObject obj3 in remoteCommands)
                        {
                            PSPropertyInfo info7 = obj3.Properties["CommandId"];
                            PSPropertyInfo info8 = obj3.Properties["CommandLine"];
                            if (info7 != null)
                            {
                                string cmdStr = (info8 != null) ? info8.Value.ToString() : string.Empty;
                                Guid cmdId = Guid.Parse(info7.Value.ToString());
                                list2.Add(new ConnectCommandInfo(cmdId, cmdStr));
                            }
                        }
                        RunspacePool item = new RunspacePool(isDisconnected, shellId, name, list2.ToArray(), info2, host, typeTable);
                        list.Add(item);
                    }
                }
            }
            return list.ToArray();
        }

        private static bool GetTimeIntValue(string timeString, out int value)
        {
            if (timeString != null)
            {
                string str = timeString.Replace("PT", "").Replace("S", "");
                try
                {
                    int num = (int) (Convert.ToDouble(str, CultureInfo.InvariantCulture) * 1000.0);
                    if (num > 0)
                    {
                        value = num;
                        return true;
                    }
                }
                catch (FormatException)
                {
                }
                catch (OverflowException)
                {
                }
            }
            value = 0;
            return false;
        }

        internal void HandleApplicationPrivateDataReceived(object sender, RemoteDataEventArgs<PSPrimitiveDictionary> eventArgs)
        {
            this.SetApplicationPrivateData(eventArgs.Data);
        }

        internal void HandleInitInfoReceived(object sender, RemoteDataEventArgs<RunspacePoolInitInfo> eventArgs)
        {
            RunspacePoolStateInfo newStateInfo = new RunspacePoolStateInfo(RunspacePoolState.Opened, null);
            bool flag = false;
            lock (base.syncObject)
            {
                base.minPoolSz = eventArgs.Data.MinRunspaces;
                base.maxPoolSz = eventArgs.Data.MaxRunspaces;
                if (base.stateInfo.State == RunspacePoolState.Connecting)
                {
                    flag = true;
                    this.SetRunspacePoolState(newStateInfo);
                }
            }
            if (flag)
            {
                base.RaiseStateChangeEvent(newStateInfo);
                this.SetOpenAsCompleted();
            }
        }

        private void HandlePSEventArgsReceived(object sender, RemoteDataEventArgs<PSEventArgs> e)
        {
            this.OnForwardEvent(e.Data);
        }

        internal void HandleRemoteHostCalls(object sender, RemoteDataEventArgs<RemoteHostCall> eventArgs)
        {
            if (this.HostCallReceived != null)
            {
                this.HostCallReceived.SafeInvoke<RemoteDataEventArgs<RemoteHostCall>>(sender, eventArgs);
            }
            else
            {
                RemoteHostCall data = eventArgs.Data;
                if (data.IsVoidMethod)
                {
                    data.ExecuteVoidMethod(base.host);
                }
                else
                {
                    RemoteHostResponse hostResponse = data.ExecuteNonVoidMethod(base.host);
                    this.dataStructureHandler.SendHostResponseToServer(hostResponse);
                }
            }
        }

        private void HandleResponseReceived(object sender, RemoteDataEventArgs<PSObject> eventArgs)
        {
            PSObject data = eventArgs.Data;
            object propertyValue = RemotingDecoder.GetPropertyValue<object>(data, "SetMinMaxRunspacesResponse");
            long callId = RemotingDecoder.GetPropertyValue<long>(data, "ci");
            this.dispatchTable.SetResponse(callId, propertyValue);
        }

        private void HandleSessionClosed(object sender, RemoteDataEventArgs<Exception> eventArgs)
        {
            RunspacePoolState state;
            RunspacePoolStateInfo info;
            if (eventArgs.Data != null)
            {
                this.closingReason = eventArgs.Data;
            }
            lock (base.syncObject)
            {
                state = base.stateInfo.State;
                switch (state)
                {
                    case RunspacePoolState.Opening:
                    case RunspacePoolState.Opened:
                    case RunspacePoolState.Disconnecting:
                    case RunspacePoolState.Disconnected:
                    case RunspacePoolState.Connecting:
                        this.SetRunspacePoolState(new RunspacePoolStateInfo(RunspacePoolState.Broken, this.closingReason));
                        break;

                    case RunspacePoolState.Closing:
                        this.SetRunspacePoolState(new RunspacePoolStateInfo(RunspacePoolState.Closed, this.closingReason));
                        break;
                }
                info = new RunspacePoolStateInfo(base.stateInfo.State, base.stateInfo.Reason);
            }
            try
            {
                base.RaiseStateChangeEvent(info);
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
            }
            switch (state)
            {
                case RunspacePoolState.Disconnecting:
                case RunspacePoolState.Disconnected:
                    this.SetDisconnectAsCompleted();
                    break;
                default:
                    if (state == RunspacePoolState.Connecting)
                    {
                        this.SetReconnectAsCompleted();
                    }
                    break;
            }
            this.SetCloseAsCompleted();
        }

        private void HandleSessionClosing(object sender, RemoteDataEventArgs<Exception> eventArgs)
        {
            this.closingReason = eventArgs.Data;
        }

        private void HandleSessionCreateCompleted(object sender, CreateCompleteEventArgs eventArgs)
        {
            if (eventArgs != null)
            {
				this.connectionInfo.IdleTimeout = eventArgs.ConnectionInfo.IdleTimeout;
                this.connectionInfo.MaxIdleTimeout = eventArgs.ConnectionInfo.MaxIdleTimeout;
                WSManConnectionInfo connectionInfo = this.connectionInfo as WSManConnectionInfo;
                if (connectionInfo != null)
                {
                    connectionInfo.OutputBufferingMode = ((WSManConnectionInfo) eventArgs.ConnectionInfo).OutputBufferingMode;
                }
            }
            this.SessionCreateCompleted.SafeInvoke<CreateCompleteEventArgs>(this, eventArgs);
        }

        private void HandleSessionDisconnected(object sender, RemoteDataEventArgs<Exception> eventArgs)
        {
            bool flag = false;
            lock (base.syncObject)
            {
                if (base.stateInfo.State == RunspacePoolState.Disconnecting)
                {
                    this.SetRunspacePoolState(new RunspacePoolStateInfo(RunspacePoolState.Disconnected, eventArgs.Data));
                    flag = true;
                }
                this.canReconnect = true;
            }
            if (flag)
            {
                base.RaiseStateChangeEvent(base.stateInfo);
                this.SetDisconnectAsCompleted();
            }
        }

        private void HandleSessionRCDisconnecting(object sender, RemoteDataEventArgs<Exception> e)
        {
            lock (base.syncObject)
            {
                this.SetRunspacePoolState(new RunspacePoolStateInfo(RunspacePoolState.Disconnecting, e.Data));
            }
            base.RaiseStateChangeEvent(base.stateInfo);
        }

        private void HandleSessionReconnected(object sender, RemoteDataEventArgs<Exception> eventArgs)
        {
            bool flag = false;
            lock (base.syncObject)
            {
                if (base.stateInfo.State == RunspacePoolState.Connecting)
                {
                    this.SetRunspacePoolState(new RunspacePoolStateInfo(RunspacePoolState.Opened, null));
                    flag = true;
                }
            }
            if (flag)
            {
                base.RaiseStateChangeEvent(base.stateInfo);
                this.SetReconnectAsCompleted();
            }
        }

        internal void HandleStateInfoReceived(object sender, RemoteDataEventArgs<RunspacePoolStateInfo> eventArgs)
        {
            RunspacePoolStateInfo data = eventArgs.Data;
            bool flag = false;
            if (data.State == RunspacePoolState.Opened)
            {
                lock (base.syncObject)
                {
                    if (base.stateInfo.State == RunspacePoolState.Opening)
                    {
                        this.SetRunspacePoolState(data);
                        flag = true;
                    }
                }
                if (flag)
                {
                    base.RaiseStateChangeEvent(base.stateInfo);
                    this.SetOpenAsCompleted();
                }
            }
            else if ((data.State == RunspacePoolState.Closed) || (data.State == RunspacePoolState.Broken))
            {
                bool flag3 = false;
                lock (base.syncObject)
                {
                    if ((base.stateInfo.State == RunspacePoolState.Closed) || (base.stateInfo.State == RunspacePoolState.Broken))
                    {
                        return;
                    }
                    if (((base.stateInfo.State == RunspacePoolState.Opening) || (base.stateInfo.State == RunspacePoolState.Opened)) || (base.stateInfo.State == RunspacePoolState.Closing))
                    {
                        flag3 = true;
                        this.SetRunspacePoolState(data);
                    }
                }
                if (flag3 && (this.closeAsyncResult == null))
                {
                    this.dataStructureHandler.CloseRunspacePoolAsync();
                }
            }
        }

        private void HandleURIDirectionReported(object sender, RemoteDataEventArgs<Uri> eventArgs)
        {
            WSManConnectionInfo connectionInfo = this.connectionInfo as WSManConnectionInfo;
            if (connectionInfo != null)
            {
                connectionInfo.ConnectionUri = eventArgs.Data;
                this.URIRedirectionReported.SafeInvoke<RemoteDataEventArgs<Uri>>(this, eventArgs);
            }
        }

        public override void Open()
        {
            IAsyncResult asyncResult = base.BeginOpen(null, null);
            base.EndOpen(asyncResult);
        }

        internal override void PropagateApplicationPrivateData(Runspace runspace)
        {
            if (this.applicationPrivateDataReceived.WaitOne(0, false))
            {
                runspace.SetApplicationPrivateData(this.GetApplicationPrivateData());
            }
        }

        internal void SetApplicationPrivateData(PSPrimitiveDictionary applicationPrivateData)
        {
            lock (base.syncObject)
            {
                if (!this.applicationPrivateDataReceived.WaitOne(0, false))
                {
                    this.applicationPrivateData = applicationPrivateData;
                    this.applicationPrivateDataReceived.Set();
                    foreach (Runspace runspace in base.runspaceList)
                    {
                        runspace.SetApplicationPrivateData(applicationPrivateData);
                    }
                }
            }
        }

        private void SetCloseAsCompleted()
        {
            this.DispatchTable.AbortAllCalls();
            if (this.closeAsyncResult != null)
            {
                this.closeAsyncResult.SetAsCompleted(base.stateInfo.Reason);
                this.closeAsyncResult = null;
            }
            else
            {
                this.SetOpenAsCompleted();
            }
        }

        private void SetDisconnectAsCompleted()
        {
            if ((this.disconnectAsyncResult != null) && !this.disconnectAsyncResult.IsCompleted)
            {
                this.disconnectAsyncResult.SetAsCompleted(base.stateInfo.Reason);
                this.disconnectAsyncResult = null;
            }
        }

        internal override bool SetMaxRunspaces(int maxRunspaces)
        {
            bool response = false;
            long callId = 0L;
            lock (base.syncObject)
            {
                if (((maxRunspaces < base.minPoolSz) || (maxRunspaces == base.maxPoolSz)) || (((base.stateInfo.State == RunspacePoolState.Closed) || (base.stateInfo.State == RunspacePoolState.Closing)) || (base.stateInfo.State == RunspacePoolState.Broken)))
                {
                    return false;
                }
                if ((base.stateInfo.State == RunspacePoolState.BeforeOpen) || (base.stateInfo.State == RunspacePoolState.Disconnected))
                {
                    base.maxPoolSz = maxRunspaces;
                    return true;
                }
                callId = this.DispatchTable.CreateNewCallId();
                this.dataStructureHandler.SendSetMaxRunspacesToServer(maxRunspaces, callId);
            }
            response = (bool) this.DispatchTable.GetResponse(callId, false);
            if (response)
            {
                lock (base.syncObject)
                {
                    base.maxPoolSz = maxRunspaces;
                }
            }
            return response;
        }

        internal override bool SetMinRunspaces(int minRunspaces)
        {
            bool response = false;
            long callId = 0L;
            lock (base.syncObject)
            {
                if ((((minRunspaces < 1) || (minRunspaces > base.maxPoolSz)) || ((minRunspaces == base.minPoolSz) || (base.stateInfo.State == RunspacePoolState.Closed))) || ((base.stateInfo.State == RunspacePoolState.Closing) || (base.stateInfo.State == RunspacePoolState.Broken)))
                {
                    return false;
                }
                if ((base.stateInfo.State == RunspacePoolState.BeforeOpen) || (base.stateInfo.State == RunspacePoolState.Disconnected))
                {
                    base.minPoolSz = minRunspaces;
                    return true;
                }
                callId = this.DispatchTable.CreateNewCallId();
                this.dataStructureHandler.SendSetMinRunspacesToServer(minRunspaces, callId);
            }
            response = (bool) this.DispatchTable.GetResponse(callId, false);
            if (response)
            {
                lock (base.syncObject)
                {
                    base.minPoolSz = minRunspaces;
                }
            }
            return response;
        }

        private void SetOpenAsCompleted()
        {
            if ((this.openAsyncResult != null) && !this.openAsyncResult.IsCompleted)
            {
                this.openAsyncResult.SetAsCompleted(base.stateInfo.Reason);
                this.openAsyncResult = null;
            }
        }

        private void SetReconnectAsCompleted()
        {
            if ((this.reconnectAsyncResult != null) && !this.reconnectAsyncResult.IsCompleted)
            {
                this.reconnectAsyncResult.SetAsCompleted(base.stateInfo.Reason);
                this.reconnectAsyncResult = null;
            }
        }

        private void SetRunspacePoolState(RunspacePoolStateInfo newStateInfo)
        {
            this.SetRunspacePoolState(newStateInfo, false);
        }

        private void SetRunspacePoolState(RunspacePoolStateInfo newStateInfo, bool raiseEvents)
        {
            base.stateInfo = newStateInfo;
            this.availableForConnection = (base.stateInfo.State == RunspacePoolState.Disconnected) || (base.stateInfo.State == RunspacePoolState.Opened);
            if (raiseEvents)
            {
                base.RaiseStateChangeEvent(newStateInfo);
            }
        }

        private static void UpdateWSManConnectionInfo(WSManConnectionInfo wsmanConnectionInfo, PSObject rsInfoObject)
        {
            int num;
            int num2;
            PSPropertyInfo info = rsInfoObject.Properties["IdleTimeOut"];
            PSPropertyInfo info2 = rsInfoObject.Properties["BufferMode"];
            PSPropertyInfo info3 = rsInfoObject.Properties["ResourceUri"];
            PSPropertyInfo info4 = rsInfoObject.Properties["Locale"];
            PSPropertyInfo info5 = rsInfoObject.Properties["DataLocale"];
            PSPropertyInfo info6 = rsInfoObject.Properties["CompressionMode"];
            PSPropertyInfo info7 = rsInfoObject.Properties["Encoding"];
            PSPropertyInfo info8 = rsInfoObject.Properties["ProfileLoaded"];
            PSPropertyInfo info9 = rsInfoObject.Properties["MaxIdleTimeout"];
            if ((info != null) && GetTimeIntValue(info.Value as string, out num))
            {
                wsmanConnectionInfo.IdleTimeout = num;
            }
            if (info2 != null)
            {
                OutputBufferingMode mode;
                string str = info2.Value as string;
                if ((str != null) && Enum.TryParse<OutputBufferingMode>(str, out mode))
                {
                    wsmanConnectionInfo.OutputBufferingMode = mode;
                }
            }
            if (info3 != null)
            {
                string str2 = info3.Value as string;
                if (str2 != null)
                {
                    wsmanConnectionInfo.ShellUri = str2;
                }
            }
            if (info4 != null)
            {
                string name = info4.Value as string;
                if (name != null)
                {
                    try
                    {
                        wsmanConnectionInfo.UICulture = new CultureInfo(name);
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }
            if (info5 != null)
            {
                string str4 = info5.Value as string;
                if (str4 != null)
                {
                    try
                    {
                        wsmanConnectionInfo.Culture = new CultureInfo(str4);
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }
            if (info6 != null)
            {
                string str5 = info6.Value as string;
                if (str5 != null)
                {
                    wsmanConnectionInfo.UseCompression = !str5.Equals("NoCompression", StringComparison.OrdinalIgnoreCase);
                }
            }
            if (info7 != null)
            {
                string str6 = info7.Value as string;
                if (str6 != null)
                {
                    wsmanConnectionInfo.UseUTF16 = str6.Equals("UTF16", StringComparison.OrdinalIgnoreCase);
                }
            }
            if (info8 != null)
            {
                string str7 = info8.Value as string;
                if (str7 != null)
                {
                    wsmanConnectionInfo.NoMachineProfile = !str7.Equals("Yes", StringComparison.OrdinalIgnoreCase);
                }
            }
            if ((info9 != null) && GetTimeIntValue(info9.Value as string, out num2))
            {
                wsmanConnectionInfo.MaxIdleTimeout = num2;
            }
        }

        internal PSPrimitiveDictionary ApplicationArguments
        {
            get
            {
                return this.applicationArguments;
            }
        }

        internal bool AvailableForConnection
        {
            get
            {
                return this.availableForConnection;
            }
        }

        internal bool CanDisconnect
        {
            get
            {
                Version pSRemotingProtocolVersion = this.PSRemotingProtocolVersion;
                if ((pSRemotingProtocolVersion == null) || (this.dataStructureHandler == null))
                {
                    return false;
                }
                return ((pSRemotingProtocolVersion >= RemotingConstants.ProtocolVersionWin8RTM) && this.dataStructureHandler.EndpointSupportsDisconnect);
            }
        }

        internal ConnectCommandInfo[] ConnectCommands
        {
            get
            {
                return this.connectCommands;
            }
            set
            {
                this.connectCommands = value;
            }
        }

        public override RunspaceConnectionInfo ConnectionInfo
        {
            get
            {
                return this.connectionInfo;
            }
        }

        internal ClientRunspacePoolDataStructureHandler DataStructureHandler
        {
            get
            {
                return this.dataStructureHandler;
            }
        }

        private DispatchTable<object> DispatchTable
        {
            get
            {
                return this.dispatchTable;
            }
        }

        internal PSHost Host
        {
            get
            {
                return base.host;
            }
        }

        internal int MaxRetryConnectionTime
        {
            get
            {
                if (this.dataStructureHandler == null)
                {
                    return 0;
                }
                return this.dataStructureHandler.MaxRetryConnectionTime;
            }
        }

        internal string Name
        {
            get
            {
                return this.friendlyName;
            }
            set
            {
                if (value == null)
                {
                    this.friendlyName = string.Empty;
                }
                else
                {
                    this.friendlyName = value;
                }
            }
        }

        internal Version PSRemotingProtocolVersion
        {
            get
            {
                Version result = null;
                PSPrimitiveDictionary applicationPrivateData = this.GetApplicationPrivateData();
                if (applicationPrivateData != null)
                {
                    PSPrimitiveDictionary.TryPathGet<Version>(applicationPrivateData, out result, new string[] { "PSVersionTable", "PSRemotingProtocolVersion" });
                }
                return result;
            }
        }

        public override System.Management.Automation.Runspaces.RunspacePoolAvailability RunspacePoolAvailability
        {
            get
            {
                if (base.stateInfo.State == RunspacePoolState.Disconnected)
                {
                    return (this.AvailableForConnection ? System.Management.Automation.Runspaces.RunspacePoolAvailability.None : System.Management.Automation.Runspaces.RunspacePoolAvailability.Busy);
                }
                return base.RunspacePoolAvailability;
            }
        }
    }
}

