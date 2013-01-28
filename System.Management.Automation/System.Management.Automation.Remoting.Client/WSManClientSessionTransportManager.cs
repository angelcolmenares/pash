namespace System.Management.Automation.Remoting.Client
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Runspaces.Internal;
    using System.Management.Automation.Tracing;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;
    using System.Xml;

    internal sealed class WSManClientSessionTransportManager : BaseClientSessionTransportManager
    {
        private WSManConnectionInfo _connectionInfo;
        private WSManNativeApi.WSManShellAsync closeSessionCompleted;
        private WSManNativeApi.WSManShellAsync connectSessionCallback;
        private WSManNativeApi.WSManShellAsync createSessionCallback;
        private GCHandle createSessionCallbackGCHandle;
        private WSManNativeApi.WSManShellAsync disconnectSessionCompleted;
        internal const int MAX_URI_REDIRECTION_COUNT = 5;
        internal const string MAX_URI_REDIRECTION_COUNT_VARIABLE = "WSManMaxRedirectionCount";
        private int maxRetryTime;
        private bool noCompression;
        private bool noMachineProfile;
        private System.Management.Automation.Remoting.PrioritySendDataCollection.OnDataAvailableCallback onDataAvailableToSendCallback;
        private WSManNativeApi.WSManData openContent;
        private static Delegate protocolVersionRedirect = null;
        private WSManNativeApi.WSManShellAsync receivedFromRemote;
        private WSManNativeApi.WSManShellAsync reconnectSessionCompleted;
        private const string resBaseName = "remotingerroridstrings";
        private WSManNativeApi.WSManShellAsync sendToRemoteCompleted;
        private static WSManNativeApi.WSManShellAsyncCallback sessionCloseCallback;
        private static WSManNativeApi.WSManShellAsyncCallback sessionConnectCallback;
        private long sessionContextID;
        private static WSManNativeApi.WSManShellAsyncCallback sessionCreateCallback;
        private static WSManNativeApi.WSManShellAsyncCallback sessionDisconnectCallback;
        private string sessionName;
        private static WSManNativeApi.WSManShellAsyncCallback sessionReceiveCallback;
        private static WSManNativeApi.WSManShellAsyncCallback sessionReconnectCallback;
        private static WSManNativeApi.WSManShellAsyncCallback sessionSendCallback;
        private static Delegate sessionSendRedirect = null;
        private static Dictionary<long, WSManClientSessionTransportManager> SessionTMHandles = new Dictionary<long, WSManClientSessionTransportManager>();
        private static long SessionTMSeed;
        private WSManTransportManagerUtils.tmStartModes startMode;
        private bool supportsDisconnect;
        internal static WSManAPIStaticData wsManApiStaticData = new WSManAPIStaticData();
        private IntPtr wsManRecieveOperationHandle;
        private IntPtr wsManSendOperationHandle;
        private IntPtr wsManSessionHandle;
        private IntPtr wsManShellOperationHandle;

        internal event EventHandler<EventArgs> RobustConnectionsCompleted;

        internal event EventHandler<EventArgs> RobustConnectionsInitiated;

        static WSManClientSessionTransportManager()
        {
            WSManNativeApi.WSManShellCompletionFunction callback = new WSManNativeApi.WSManShellCompletionFunction(WSManClientSessionTransportManager.OnCreateSessionCallback);
            sessionCreateCallback = new WSManNativeApi.WSManShellAsyncCallback(callback);
            WSManNativeApi.WSManShellCompletionFunction function2 = new WSManNativeApi.WSManShellCompletionFunction(WSManClientSessionTransportManager.OnCloseSessionCompleted);
            sessionCloseCallback = new WSManNativeApi.WSManShellAsyncCallback(function2);
            WSManNativeApi.WSManShellCompletionFunction function3 = new WSManNativeApi.WSManShellCompletionFunction(WSManClientSessionTransportManager.OnRemoteSessionDataReceived);
            sessionReceiveCallback = new WSManNativeApi.WSManShellAsyncCallback(function3);
            WSManNativeApi.WSManShellCompletionFunction function4 = new WSManNativeApi.WSManShellCompletionFunction(WSManClientSessionTransportManager.OnRemoteSessionSendCompleted);
            sessionSendCallback = new WSManNativeApi.WSManShellAsyncCallback(function4);
            WSManNativeApi.WSManShellCompletionFunction function5 = new WSManNativeApi.WSManShellCompletionFunction(WSManClientSessionTransportManager.OnRemoteSessionDisconnectCompleted);
            sessionDisconnectCallback = new WSManNativeApi.WSManShellAsyncCallback(function5);
            WSManNativeApi.WSManShellCompletionFunction function6 = new WSManNativeApi.WSManShellCompletionFunction(WSManClientSessionTransportManager.OnRemoteSessionReconnectCompleted);
            sessionReconnectCallback = new WSManNativeApi.WSManShellAsyncCallback(function6);
            WSManNativeApi.WSManShellCompletionFunction function7 = new WSManNativeApi.WSManShellCompletionFunction(WSManClientSessionTransportManager.OnRemoteSessionConnectCallback);
            sessionConnectCallback = new WSManNativeApi.WSManShellAsyncCallback(function7);
        }

        internal WSManClientSessionTransportManager(Guid runspacePoolInstanceId, WSManConnectionInfo connectionInfo, PSRemotingCryptoHelper cryptoHelper, string sessionName) : base(runspacePoolInstanceId, cryptoHelper)
        {
            this.startMode = WSManTransportManagerUtils.tmStartModes.None;
            base.CryptoHelper = cryptoHelper;
            base.dataToBeSent.Fragmentor = base.Fragmentor;
            this.sessionName = sessionName;
            base.ReceivedDataCollection.MaximumReceivedDataSize = null;
            base.ReceivedDataCollection.MaximumReceivedObjectSize = connectionInfo.MaximumReceivedObjectSize;
            this.onDataAvailableToSendCallback = new System.Management.Automation.Remoting.PrioritySendDataCollection.OnDataAvailableCallback(this.OnDataAvailableCallback);
            this.Initialize(connectionInfo.ConnectionUri, connectionInfo);
        }

        private static void AddSessionTransportManager(long sessnTMId, WSManClientSessionTransportManager sessnTransportManager)
        {
            lock (SessionTMHandles)
            {
                SessionTMHandles.Add(sessnTMId, sessnTransportManager);
            }
        }

        internal void AdjustForProtocolVariations(Version serverProtocolVersion)
        {
            if (serverProtocolVersion <= RemotingConstants.ProtocolVersionWin7RTM)
            {
                int num;
                WSManNativeApi.WSManGetSessionOptionAsDword(this.wsManSessionHandle, WSManNativeApi.WSManSessionOption.WSMAN_OPTION_MAX_ENVELOPE_SIZE_KB, out num);
                if (num == 500)
                {
                    int num3;
                    int errorCode = WSManNativeApi.WSManSetSessionOption(this.wsManSessionHandle, WSManNativeApi.WSManSessionOption.WSMAN_OPTION_MAX_ENVELOPE_SIZE_KB, new WSManNativeApi.WSManDataDWord(150));
                    if (errorCode != 0)
                    {
                        PSInvalidOperationException exception = new PSInvalidOperationException(WSManNativeApi.WSManGetErrorMessage(wsManApiStaticData.WSManAPIHandle, errorCode));
                        throw exception;
                    }
                    WSManNativeApi.WSManGetSessionOptionAsDword(this.wsManSessionHandle, WSManNativeApi.WSManSessionOption.WSMAN_OPTION_SHELL_MAX_DATA_SIZE_PER_MESSAGE_KB, out num3);
                    base.Fragmentor.FragmentSize = num3 << 10;
                }
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
            bool flag = false;
            lock (base.syncObject)
            {
                if (base.isClosed)
                {
                    return;
                }
                if (this.startMode == WSManTransportManagerUtils.tmStartModes.None)
                {
                    flag = true;
                }
                else if (((this.startMode == WSManTransportManagerUtils.tmStartModes.Create) || (this.startMode == WSManTransportManagerUtils.tmStartModes.Connect)) && (IntPtr.Zero == this.wsManShellOperationHandle))
                {
                    flag = true;
                }
                base.isClosed = true;
            }
            base.CloseAsync();
            if (!flag)
            {
                PSEtwLog.LogAnalyticInformational(PSEventId.WSManCloseShell, PSOpcode.Disconnect, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { base.RunspacePoolInstanceId.ToString() });
                this.closeSessionCompleted = new WSManNativeApi.WSManShellAsync(new IntPtr(this.sessionContextID), sessionCloseCallback);
                WSManNativeApi.WSManCloseShell(this.wsManShellOperationHandle, 0, (IntPtr) this.closeSessionCompleted);
            }
            else
            {
                try
                {
                    base.RaiseCloseCompleted();
                }
                finally
                {
                    RemoveSessionTransportManager(this.sessionContextID);
                }
            }
        }

        private void CloseSessionAndClearResources()
        {
            BaseClientTransportManager.tracer.WriteLine("Clearing session with session context: {0} Operation Context: {1}", new object[] { this.sessionContextID, this.wsManShellOperationHandle });
            IntPtr state = this.wsManSessionHandle;
            this.wsManSessionHandle = IntPtr.Zero;
            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate (object s) {
                IntPtr wsManSessionHandle = (IntPtr) s;
                if (IntPtr.Zero != wsManSessionHandle)
                {
                    WSManNativeApi.WSManCloseSession(wsManSessionHandle, 0);
                }
            }), state);
            RemoveSessionTransportManager(this.sessionContextID);
            if (this.closeSessionCompleted != null)
            {
                this.closeSessionCompleted.Dispose();
                this.closeSessionCompleted = null;
            }
            if (this.createSessionCallback != null)
            {
                this.createSessionCallbackGCHandle.Free();
                this.createSessionCallback.Dispose();
                this.createSessionCallback = null;
            }
            if (this.connectSessionCallback != null)
            {
                this.connectSessionCallback.Dispose();
                this.connectSessionCallback = null;
            }
        }

        internal override void ConnectAsync()
        {
            base.ReceivedDataCollection.PrepareForStreamConnect();
            if (this.openContent == null)
            {
                DataPriorityType type;
                byte[] inArray = base.dataToBeSent.ReadOrRegisterCallback(null, out type);
                if (inArray != null)
                {
                    string data = string.Format(CultureInfo.InvariantCulture, "<{0} xmlns=\"{1}\">{2}</{0}>", new object[] { "connectXml", "http://schemas.microsoft.com/powershell", Convert.ToBase64String(inArray, Base64FormattingOptions.None) });
                    this.openContent = new WSManNativeApi.WSManData(data);
                }
                if (base.dataToBeSent.ReadOrRegisterCallback(null, out type) != null)
                {
                    return;
                }
            }
            this.sessionContextID = GetNextSessionTMHandleId();
            AddSessionTransportManager(this.sessionContextID, this);
            this.supportsDisconnect = true;
            this.connectSessionCallback = new WSManNativeApi.WSManShellAsync(new IntPtr(this.sessionContextID), sessionConnectCallback);
            lock (base.syncObject)
            {
                if (base.isClosed)
                {
                    return;
                }
                this.startMode = WSManTransportManagerUtils.tmStartModes.Connect;
                int flags = 0;
                flags |= (this._connectionInfo.OutputBufferingMode == OutputBufferingMode.Block) ? 8 : 0;
                flags |= (this._connectionInfo.OutputBufferingMode == OutputBufferingMode.Drop) ? 4 : 0;
                WSManNativeApi.WSManConnectShellEx(this.wsManSessionHandle, flags, this._connectionInfo.ShellUri, base.RunspacePoolInstanceId.ToString().ToUpper(CultureInfo.InvariantCulture), IntPtr.Zero, (IntPtr) this.openContent, (IntPtr) this.connectSessionCallback, ref this.wsManShellOperationHandle);
            }
            if (this.wsManShellOperationHandle == IntPtr.Zero)
            {
                TransportErrorOccuredEventArgs eventArgs = WSManTransportManagerUtils.ConstructTransportErrorEventArgs(wsManApiStaticData.WSManAPIHandle, this, new WSManNativeApi.WSManError(), TransportMethodEnum.ConnectShellEx, RemotingErrorIdStrings.ConnectExFailed, new object[] { this.ConnectionInfo.ComputerName });
                this.ProcessWSManTransportError(eventArgs);
            }
        }

        internal override void CreateAsync()
        {
            List<WSManNativeApi.WSManOption> list = new List<WSManNativeApi.WSManOption>(wsManApiStaticData.CommonOptionSet);
            if (protocolVersionRedirect != null)
            {
                string str = (string) protocolVersionRedirect.DynamicInvoke(new object[0]);
                list.Clear();
                WSManNativeApi.WSManOption item = new WSManNativeApi.WSManOption {
                    name = "protocolversion",
                    value = str,
                    mustComply = true
                };
                list.Add(item);
            }
            int serverIdleTimeOut = (this._connectionInfo.IdleTimeout > 0) ? ((int) this._connectionInfo.IdleTimeout) : int.MaxValue;
            WSManNativeApi.WSManShellStartupInfo startupInfo = new WSManNativeApi.WSManShellStartupInfo(wsManApiStaticData.InputStreamSet, wsManApiStaticData.OutputStreamSet, serverIdleTimeOut, this.sessionName);
            if (this.openContent == null)
            {
                DataPriorityType type;
                byte[] inArray = base.dataToBeSent.ReadOrRegisterCallback(null, out type);
                bool flag = true;
                if (sessionSendRedirect != null)
                {
                    object[] objArray2 = new object[2];
                    objArray2[1] = inArray;
                    object[] objArray = objArray2;
                    flag = (bool) sessionSendRedirect.DynamicInvoke(objArray);
                    inArray = (byte[]) objArray[0];
                }
                if (!flag)
                {
                    return;
                }
                if (inArray != null)
                {
                    string data = string.Format(CultureInfo.InvariantCulture, "<{0} xmlns=\"{1}\">{2}</{0}>", new object[] { "creationXml", "http://schemas.microsoft.com/powershell", Convert.ToBase64String(inArray, Base64FormattingOptions.None) });
                    this.openContent = new WSManNativeApi.WSManData(data);
                }
            }
            this.sessionContextID = GetNextSessionTMHandleId();
            AddSessionTransportManager(this.sessionContextID, this);
            PSEtwLog.LogAnalyticInformational(PSEventId.WSManCreateShell, PSOpcode.Connect, PSTask.CreateRunspace, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { base.RunspacePoolInstanceId.ToString() });
            this.createSessionCallback = new WSManNativeApi.WSManShellAsync(new IntPtr(this.sessionContextID), sessionCreateCallback);
            this.createSessionCallbackGCHandle = GCHandle.Alloc(this.createSessionCallback);
            try
            {
                lock (base.syncObject)
                {
                    if (base.isClosed)
                    {
                        return;
                    }
                    this.startMode = WSManTransportManagerUtils.tmStartModes.Create;
                    if (this.noMachineProfile)
                    {
                        WSManNativeApi.WSManOption option2 = new WSManNativeApi.WSManOption {
                            name = "WINRS_NOPROFILE",
                            mustComply = true,
                            value = "1"
                        };
                        list.Add(option2);
                    }
                    int flags = this.noCompression ? 1 : 0;
                    flags |= (this._connectionInfo.OutputBufferingMode == OutputBufferingMode.Block) ? 8 : 0;
                    flags |= (this._connectionInfo.OutputBufferingMode == OutputBufferingMode.Drop) ? 4 : 0;
                    using (WSManNativeApi.WSManOptionSet set = new WSManNativeApi.WSManOptionSet(list.ToArray()))
                    {
                        WSManNativeApi.WSManCreateShellEx(this.wsManSessionHandle, flags, this._connectionInfo.ShellUri, base.RunspacePoolInstanceId.ToString().ToUpper(CultureInfo.InvariantCulture), startupInfo, set, this.openContent, (IntPtr) this.createSessionCallback, ref this.wsManShellOperationHandle);
                    }
                }
                if (this.wsManShellOperationHandle == IntPtr.Zero)
                {
                    TransportErrorOccuredEventArgs eventArgs = WSManTransportManagerUtils.ConstructTransportErrorEventArgs(wsManApiStaticData.WSManAPIHandle, this, new WSManNativeApi.WSManError(), TransportMethodEnum.CreateShellEx, RemotingErrorIdStrings.ConnectExFailed, new object[] { this.ConnectionInfo.ComputerName });
                    this.ProcessWSManTransportError(eventArgs);
                }
            }
            finally
            {
                startupInfo.Dispose();
            }
        }

        internal override BaseClientCommandTransportManager CreateClientCommandTransportManager(RunspaceConnectionInfo connectionInfo, ClientRemotePowerShell cmd, bool noInput)
        {
            return new WSManClientCommandTransportManager(connectionInfo as WSManConnectionInfo, this.wsManShellOperationHandle, cmd, noInput, this);
        }

        internal override void DisconnectAsync()
        {
            int serverIdleTimeOut = (this._connectionInfo.IdleTimeout > 0) ? ((int) this._connectionInfo.IdleTimeout) : int.MaxValue;
            WSManNativeApi.WSManShellDisconnectInfo info = new WSManNativeApi.WSManShellDisconnectInfo(serverIdleTimeOut);
            this.disconnectSessionCompleted = new WSManNativeApi.WSManShellAsync(new IntPtr(this.sessionContextID), sessionDisconnectCallback);
            try
            {
                lock (base.syncObject)
                {
                    if (!base.isClosed)
                    {
                        int flags = 0;
                        flags |= (this._connectionInfo.OutputBufferingMode == OutputBufferingMode.Block) ? 8 : 0;
                        flags |= (this._connectionInfo.OutputBufferingMode == OutputBufferingMode.Drop) ? 4 : 0;
                        WSManNativeApi.WSManDisconnectShellEx(this.wsManShellOperationHandle, flags, (IntPtr) info, (IntPtr) this.disconnectSessionCompleted);
                    }
                }
            }
            finally
            {
                info.Dispose();
            }
        }

        internal override void Dispose(bool isDisposing)
        {
            BaseClientTransportManager.tracer.WriteLine("Disposing session with session context: {0} Operation Context: {1}", new object[] { this.sessionContextID, this.wsManShellOperationHandle });
            this.CloseSessionAndClearResources();
            if (isDisposing && (this.openContent != null))
            {
                this.openContent.Dispose();
                this.openContent = null;
            }
            base.Dispose(isDisposing);
        }

        private static long GetNextSessionTMHandleId()
        {
            return Interlocked.Increment(ref SessionTMSeed);
        }

        private static bool HandleRobustConnectionCallback(int flags, WSManClientSessionTransportManager sessionTM)
        {
            if ((((flags != 0x40) && (flags != 0x100)) && ((flags != 0x200) && (flags != 0x400))) && ((flags != 0x800) && (flags != 0x1000)))
            {
                return false;
            }
            if (flags == 0x100)
            {
                try
                {
                    sessionTM.RobustConnectionsInitiated.SafeInvoke<EventArgs>(sessionTM, EventArgs.Empty);
                }
                catch (ObjectDisposedException)
                {
                }
            }
            sessionTM.QueueRobustConnectionNotification(flags);
            if (((flags == 0x40) || (flags == 0x400)) || (flags == 0x1000))
            {
                try
                {
                    sessionTM.RobustConnectionsCompleted.SafeInvoke<EventArgs>(sessionTM, EventArgs.Empty);
                }
                catch (ObjectDisposedException)
                {
                }
            }
            return true;
        }

        private void Initialize(Uri connectionUri, WSManConnectionInfo connectionInfo)
        {
            WSManNativeApi.BaseWSManAuthenticationCredentials credentials;
            int num2;
            this._connectionInfo = connectionInfo;
            bool isSSLSpecified = false;
            string originalString = connectionUri.OriginalString;
            if ((connectionUri == connectionInfo.ConnectionUri) && connectionInfo.UseDefaultWSManPort)
            {
                originalString = WSManConnectionInfo.GetConnectionString(connectionInfo.ConnectionUri, out isSSLSpecified);
            }
            string str2 = string.Empty;
            if (PSSessionConfigurationData.IsServerManager)
            {
                str2 = ";MSP=7a83d074-bb86-4e52-aa3e-6cc73cc066c8";
            }
            if (string.IsNullOrEmpty(connectionUri.Query))
            {
                originalString = string.Format(CultureInfo.InvariantCulture, "{0}?PSVersion={1}{2}", new object[] { originalString.TrimEnd(new char[] { '/' }), PSVersionInfo.PSVersion, str2 });
            }
            else
            {
                originalString = string.Format(CultureInfo.InvariantCulture, "{0};PSVersion={1}{2}", new object[] { originalString, PSVersionInfo.PSVersion, str2 });
            }
            if (connectionInfo.CertificateThumbprint != null)
            {
                credentials = new WSManNativeApi.WSManCertificateThumbprintCredentials(connectionInfo.CertificateThumbprint);
            }
            else
            {
                string userName = null;
                SecureString pwd = null;
                if ((connectionInfo.Credential != null) && !string.IsNullOrEmpty(connectionInfo.Credential.UserName))
                {
                    userName = connectionInfo.Credential.UserName;
                    pwd = connectionInfo.Credential.Password;
                }
                WSManNativeApi.WSManUserNameAuthenticationCredentials credentials2 = new WSManNativeApi.WSManUserNameAuthenticationCredentials(userName, pwd, connectionInfo.WSManAuthenticationMechanism);
                credentials = credentials2;
            }
            WSManNativeApi.WSManUserNameAuthenticationCredentials authCredentials = null;
            if (connectionInfo.ProxyCredential != null)
            {
                WSManNativeApi.WSManAuthenticationMechanism authMechanism = WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_NEGOTIATE;
                string str5 = null;
                SecureString password = null;
                switch (connectionInfo.ProxyAuthentication)
                {
                    case AuthenticationMechanism.Basic:
                        authMechanism = WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_BASIC;
                        break;

                    case AuthenticationMechanism.Negotiate:
                        authMechanism = WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_NEGOTIATE;
                        break;

                    case AuthenticationMechanism.Digest:
                        authMechanism = WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_DIGEST;
                        break;
                }
                if (!string.IsNullOrEmpty(connectionInfo.ProxyCredential.UserName))
                {
                    str5 = connectionInfo.ProxyCredential.UserName;
                    password = connectionInfo.ProxyCredential.Password;
                }
                authCredentials = new WSManNativeApi.WSManUserNameAuthenticationCredentials(str5, password, authMechanism);
            }
            WSManNativeApi.WSManProxyInfo info = (connectionInfo.ProxyAccessType == ProxyAccessType.None) ? null : new WSManNativeApi.WSManProxyInfo(connectionInfo.ProxyAccessType, authCredentials);
            int errorCode = 0;
            try
            {
                errorCode = WSManNativeApi.WSManCreateSession(wsManApiStaticData.WSManAPIHandle, originalString, 0, (IntPtr) credentials.GetMarshalledObject(), (info == null) ? IntPtr.Zero : ((IntPtr) info), ref this.wsManSessionHandle);
            }
			catch(Exception ex)
			{

			}
            finally
            {
                if (authCredentials != null)
                {
                    authCredentials.Dispose();
                }
                if (info != null)
                {
                    info.Dispose();
                }
                if (credentials != null)
                {
                    credentials.Dispose();
                }
            }
            if (errorCode != 0)
            {
                PSInvalidOperationException exception = new PSInvalidOperationException(WSManNativeApi.WSManGetErrorMessage(wsManApiStaticData.WSManAPIHandle, errorCode));
                throw exception;
            }
            WSManNativeApi.WSManGetSessionOptionAsDword(this.wsManSessionHandle, WSManNativeApi.WSManSessionOption.WSMAN_OPTION_SHELL_MAX_DATA_SIZE_PER_MESSAGE_KB, out num2);
            base.Fragmentor.FragmentSize = num2 << 10;
            WSManNativeApi.WSManGetSessionOptionAsDword(this.wsManSessionHandle, WSManNativeApi.WSManSessionOption.WSMAN_OPTION_MAX_RETRY_TIME, out this.maxRetryTime);
            base.dataToBeSent.Fragmentor = base.Fragmentor;
            this.noCompression = !connectionInfo.UseCompression;
            this.noMachineProfile = connectionInfo.NoMachineProfile;
            if (isSSLSpecified)
            {
                this.SetWSManSessionOption(WSManNativeApi.WSManSessionOption.WSMAN_OPTION_USE_SSL, 1);
            }
            if (connectionInfo.NoEncryption)
            {
                this.SetWSManSessionOption(WSManNativeApi.WSManSessionOption.WSMAN_OPTION_UNENCRYPTED_MESSAGES, 1);
            }
            if (connectionInfo.AllowImplicitCredentialForNegotiate)
            {
                errorCode = WSManNativeApi.WSManSetSessionOption(this.wsManSessionHandle, WSManNativeApi.WSManSessionOption.WSMAN_OPTION_ALLOW_NEGOTIATE_IMPLICIT_CREDENTIALS, new WSManNativeApi.WSManDataDWord(1));
            }
            if (connectionInfo.UseUTF16)
            {
                this.SetWSManSessionOption(WSManNativeApi.WSManSessionOption.WSMAN_OPTION_UTF16, 1);
            }
            if (connectionInfo.SkipCACheck)
            {
                this.SetWSManSessionOption(WSManNativeApi.WSManSessionOption.WSMAN_OPTION_SKIP_CA_CHECK, 1);
            }
            if (connectionInfo.SkipCNCheck)
            {
                this.SetWSManSessionOption(WSManNativeApi.WSManSessionOption.WSMAN_OPTION_SKIP_CN_CHECK, 1);
            }
            if (connectionInfo.SkipRevocationCheck)
            {
                this.SetWSManSessionOption(WSManNativeApi.WSManSessionOption.WSMAN_OPTION_SKIP_REVOCATION_CHECK, 1);
            }
            if (connectionInfo.IncludePortInSPN)
            {
                this.SetWSManSessionOption(WSManNativeApi.WSManSessionOption.WSMAN_OPTION_ENABLE_SPN_SERVER_PORT, 1);
            }
            this.SetWSManSessionOption(WSManNativeApi.WSManSessionOption.WSMAN_OPTION_USE_INTERACTIVE_TOKEN, connectionInfo.EnableNetworkAccess ? 1 : 0);
            string name = connectionInfo.UICulture.Name;
            if (!string.IsNullOrEmpty(name))
            {
                this.SetWSManSessionOption(WSManNativeApi.WSManSessionOption.WSMAN_OPTION_UI_LANGUAGE, name);
            }
            string str9 = connectionInfo.Culture.Name;
            if (!string.IsNullOrEmpty(str9))
            {
                this.SetWSManSessionOption(WSManNativeApi.WSManSessionOption.WSMAN_OPTION_LOCALE, str9);
            }
            this.SetDefaultTimeOut(connectionInfo.OperationTimeout);
            this.SetConnectTimeOut(connectionInfo.OpenTimeout);
            this.SetCloseTimeOut(connectionInfo.CancelTimeout);
            this.SetSignalTimeOut(connectionInfo.CancelTimeout);
        }

        private static void OnCloseSessionCompleted(IntPtr operationContext, int flags, IntPtr error, IntPtr shellOperationHandle, IntPtr commandOperationHandle, IntPtr operationHandle, IntPtr data)
        {
            BaseClientTransportManager.tracer.WriteLine("Client Session TM: CloseShell callback received", new object[0]);
            long sessnTMId = 0L;
            WSManClientSessionTransportManager sessnTransportManager = null;
            if (!TryGetSessionTransportManager(operationContext, out sessnTransportManager, out sessnTMId))
            {
                BaseClientTransportManager.tracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Unable to find a transport manager for context {0}.", new object[] { sessnTMId }), new object[0]);
            }
            else
            {
                PSEtwLog.LogAnalyticInformational(PSEventId.WSManCloseShellCallbackReceived, PSOpcode.Disconnect, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { sessnTransportManager.RunspacePoolInstanceId.ToString() });
                if (IntPtr.Zero != error)
                {
                    WSManNativeApi.WSManError errorStruct = WSManNativeApi.WSManError.UnMarshal(error);
                    if (errorStruct.errorCode != 0)
                    {
                        BaseClientTransportManager.tracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Got error with error code {0}. Message {1}", new object[] { errorStruct.errorCode, errorStruct.errorDetail }), new object[0]);
                        TransportErrorOccuredEventArgs eventArgs = WSManTransportManagerUtils.ConstructTransportErrorEventArgs(wsManApiStaticData.WSManAPIHandle, sessnTransportManager, errorStruct, TransportMethodEnum.CloseShellOperationEx, RemotingErrorIdStrings.CloseExCallBackError, new object[] { WSManTransportManagerUtils.ParseEscapeWSManErrorMessage(errorStruct.errorDetail) });
                        sessnTransportManager.RaiseErrorHandler(eventArgs);
                        return;
                    }
                }
                sessnTransportManager.RaiseCloseCompleted();
            }
        }

        private static void OnCreateSessionCallback(IntPtr operationContext, int flags, IntPtr error, IntPtr shellOperationHandle, IntPtr commandOperationHandle, IntPtr operationHandle, IntPtr data)
        {
            BaseClientTransportManager.tracer.WriteLine("Client Session TM: CreateShell callback received", new object[0]);
            long sessnTMId = 0L;
            WSManClientSessionTransportManager sessnTransportManager = null;
            if (!TryGetSessionTransportManager(operationContext, out sessnTransportManager, out sessnTMId))
            {
                BaseClientTransportManager.tracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Unable to find a transport manager for context {0}.", new object[] { sessnTMId }), new object[0]);
            }
            else if (!HandleRobustConnectionCallback(flags, sessnTransportManager))
            {
                PSEtwLog.LogAnalyticInformational(PSEventId.WSManCreateShellCallbackReceived, PSOpcode.Connect, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { sessnTransportManager.RunspacePoolInstanceId.ToString() });
                sessnTransportManager.wsManShellOperationHandle = shellOperationHandle;
                lock (sessnTransportManager.syncObject)
                {
                    if (sessnTransportManager.isClosed)
                    {
                        return;
                    }
                }
                if (IntPtr.Zero != error)
                {
                    WSManNativeApi.WSManError errorStruct = WSManNativeApi.WSManError.UnMarshal(error);
                    if (errorStruct.errorCode != 0)
                    {
                        BaseClientTransportManager.tracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Got error with error code {0}. Message {1}", new object[] { errorStruct.errorCode, errorStruct.errorDetail }), new object[0]);
                        TransportErrorOccuredEventArgs eventArgs = WSManTransportManagerUtils.ConstructTransportErrorEventArgs(wsManApiStaticData.WSManAPIHandle, sessnTransportManager, errorStruct, TransportMethodEnum.CreateShellEx, RemotingErrorIdStrings.ConnectExCallBackError, new object[] { sessnTransportManager.ConnectionInfo.ComputerName, WSManTransportManagerUtils.ParseEscapeWSManErrorMessage(errorStruct.errorDetail) });
                        sessnTransportManager.ProcessWSManTransportError(eventArgs);
                        return;
                    }
                }
                sessnTransportManager.supportsDisconnect = (flags & 0x20) != 0;
                if (sessnTransportManager.openContent != null)
                {
                    sessnTransportManager.openContent.Dispose();
                    sessnTransportManager.openContent = null;
                }
                if (data != IntPtr.Zero)
                {
                    WSManNativeApi.WSManCreateShellDataResult result = WSManNativeApi.WSManCreateShellDataResult.UnMarshal(data);
                    if (result.data != null)
                    {
                        string str = result.data;
                        sessnTransportManager.ProcessShellData(str);
                    }
                }
                lock (sessnTransportManager.syncObject)
                {
                    if (sessnTransportManager.isClosed)
                    {
                        BaseClientTransportManager.tracer.WriteLine("Client Session TM: Transport manager is closed. So returning", new object[0]);
                        return;
                    }
                    sessnTransportManager.RaiseCreateCompleted(new CreateCompleteEventArgs(sessnTransportManager.ConnectionInfo.Copy()));
                    sessnTransportManager.StartReceivingData();
                }
                sessnTransportManager.SendOneItem();
            }
        }

        private void OnDataAvailableCallback(byte[] data, DataPriorityType priorityType)
        {
            BaseClientTransportManager.tracer.WriteLine("Received data to be sent from the callback.", new object[0]);
            this.SendData(data, priorityType);
        }

        private static void OnRemoteSessionConnectCallback(IntPtr operationContext, int flags, IntPtr error, IntPtr shellOperationHandle, IntPtr commandOperationHandle, IntPtr operationHandle, IntPtr data)
        {
            BaseClientTransportManager.tracer.WriteLine("Client Session TM: Connect callback received", new object[0]);
            long sessnTMId = 0L;
            WSManClientSessionTransportManager sessnTransportManager = null;
            if (!TryGetSessionTransportManager(operationContext, out sessnTransportManager, out sessnTMId))
            {
                BaseClientTransportManager.tracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Unable to find a transport manager for context {0}.", new object[] { sessnTMId }), new object[0]);
            }
            else if (!HandleRobustConnectionCallback(flags, sessnTransportManager))
            {
                if (IntPtr.Zero != error)
                {
                    WSManNativeApi.WSManError errorStruct = WSManNativeApi.WSManError.UnMarshal(error);
                    if (errorStruct.errorCode != 0)
                    {
                        BaseClientTransportManager.tracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Got error with error code {0}. Message {1}", new object[] { errorStruct.errorCode, errorStruct.errorDetail }), new object[0]);
                        TransportErrorOccuredEventArgs eventArgs = WSManTransportManagerUtils.ConstructTransportErrorEventArgs(wsManApiStaticData.WSManAPIHandle, sessnTransportManager, errorStruct, TransportMethodEnum.ConnectShellEx, RemotingErrorIdStrings.ConnectExCallBackError, new object[] { sessnTransportManager.ConnectionInfo.ComputerName, WSManTransportManagerUtils.ParseEscapeWSManErrorMessage(errorStruct.errorDetail) });
                        sessnTransportManager.ProcessWSManTransportError(eventArgs);
                        return;
                    }
                }
                if (sessnTransportManager.openContent != null)
                {
                    sessnTransportManager.openContent.Dispose();
                    sessnTransportManager.openContent = null;
                }
                lock (sessnTransportManager.syncObject)
                {
                    if (sessnTransportManager.isClosed)
                    {
                        BaseClientTransportManager.tracer.WriteLine("Client Session TM: Transport manager is closed. So returning", new object[0]);
                        return;
                    }
                }
                WSManNativeApi.WSManConnectDataResult result = WSManNativeApi.WSManConnectDataResult.UnMarshal(data);
                if (result.data != null)
                {
                    string s = result.data;
                    XmlReaderSettings settings = new XmlReaderSettings {
                        CheckCharacters = false,
                        IgnoreComments = true,
                        IgnoreProcessingInstructions = true,
                        XmlResolver = null,
                        ConformanceLevel = ConformanceLevel.Fragment,
                        MaxCharactersFromEntities = 0x400L
                    };
                    byte[] buffer = Convert.FromBase64String(XmlReader.Create(new StringReader(s), settings).ReadElementString("connectResponseXml"));
                    sessnTransportManager.ProcessRawData(buffer, "stdout");
                }
                sessnTransportManager.SendOneItem();
                sessnTransportManager.RaiseConnectCompleted();
            }
        }

        private static void OnRemoteSessionDataReceived(IntPtr operationContext, int flags, IntPtr error, IntPtr shellOperationHandle, IntPtr commandOperationHandle, IntPtr operationHandle, IntPtr data)
        {
            BaseClientTransportManager.tracer.WriteLine("Client Session TM: OnRemoteDataReceived callback.", new object[0]);
            long sessnTMId = 0L;
            WSManClientSessionTransportManager sessnTransportManager = null;
            if (!TryGetSessionTransportManager(operationContext, out sessnTransportManager, out sessnTMId))
            {
                BaseClientTransportManager.tracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Unable to find a transport manager for context {0}.", new object[] { sessnTMId }), new object[0]);
            }
            else
            {
                sessnTransportManager.ClearReceiveOrSendResources(flags, false);
                if (sessnTransportManager.isClosed)
                {
                    BaseClientTransportManager.tracer.WriteLine("Client Session TM: Transport manager is closed. So returning", new object[0]);
                }
                else if (!shellOperationHandle.Equals(sessnTransportManager.wsManShellOperationHandle))
                {
                    PSRemotingTransportException e = new PSRemotingTransportException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.ReceiveExFailed, new object[] { sessnTransportManager.ConnectionInfo.ComputerName }));
                    TransportErrorOccuredEventArgs eventArgs = new TransportErrorOccuredEventArgs(e, TransportMethodEnum.ReceiveShellOutputEx);
                    sessnTransportManager.ProcessWSManTransportError(eventArgs);
                }
                else
                {
                    if (IntPtr.Zero != error)
                    {
                        WSManNativeApi.WSManError errorStruct = WSManNativeApi.WSManError.UnMarshal(error);
                        if (errorStruct.errorCode != 0)
                        {
                            BaseClientTransportManager.tracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Got error with error code {0}. Message {1}", new object[] { errorStruct.errorCode, errorStruct.errorDetail }), new object[0]);
                            TransportErrorOccuredEventArgs args2 = WSManTransportManagerUtils.ConstructTransportErrorEventArgs(wsManApiStaticData.WSManAPIHandle, sessnTransportManager, errorStruct, TransportMethodEnum.ReceiveShellOutputEx, RemotingErrorIdStrings.ReceiveExCallBackError, new object[] { sessnTransportManager.ConnectionInfo.ComputerName, WSManTransportManagerUtils.ParseEscapeWSManErrorMessage(errorStruct.errorDetail) });
                            sessnTransportManager.ProcessWSManTransportError(args2);
                            return;
                        }
                    }
                    WSManNativeApi.WSManReceiveDataResult result = WSManNativeApi.WSManReceiveDataResult.UnMarshal(data);
                    if (result.data != null)
                    {
                        BaseClientTransportManager.tracer.WriteLine("Session Received Data : {0}", new object[] { result.data.Length });
                        object[] args = new object[] { sessnTransportManager.RunspacePoolInstanceId.ToString(), Guid.Empty.ToString(), result.data.Length.ToString(CultureInfo.InvariantCulture) };
                        PSEtwLog.LogAnalyticInformational(PSEventId.WSManReceiveShellOutputExCallbackReceived, PSOpcode.Receive, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, args);
                        sessnTransportManager.ProcessRawData(result.data, result.stream);
                    }
                }
            }
        }

        private static void OnRemoteSessionDisconnectCompleted(IntPtr operationContext, int flags, IntPtr error, IntPtr shellOperationHandle, IntPtr commandOperationHandle, IntPtr operationHandle, IntPtr data)
        {
            BaseClientTransportManager.tracer.WriteLine("Client Session TM: CreateShell callback received", new object[0]);
            long sessnTMId = 0L;
            WSManClientSessionTransportManager sessnTransportManager = null;
            if (!TryGetSessionTransportManager(operationContext, out sessnTransportManager, out sessnTMId))
            {
                BaseClientTransportManager.tracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Unable to find a transport manager for context {0}.", new object[] { sessnTMId }), new object[0]);
            }
            else
            {
                if (sessnTransportManager.disconnectSessionCompleted != null)
                {
                    sessnTransportManager.disconnectSessionCompleted.Dispose();
                    sessnTransportManager.disconnectSessionCompleted = null;
                }
                if (IntPtr.Zero != error)
                {
                    WSManNativeApi.WSManError errorStruct = WSManNativeApi.WSManError.UnMarshal(error);
                    if (errorStruct.errorCode != 0)
                    {
                        BaseClientTransportManager.tracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Got error with error code {0}. Message {1}", new object[] { errorStruct.errorCode, errorStruct.errorDetail }), new object[0]);
                        TransportErrorOccuredEventArgs eventArgs = WSManTransportManagerUtils.ConstructTransportErrorEventArgs(wsManApiStaticData.WSManAPIHandle, sessnTransportManager, errorStruct, TransportMethodEnum.DisconnectShellEx, RemotingErrorIdStrings.DisconnectShellExFailed, new object[] { sessnTransportManager.ConnectionInfo.ComputerName, WSManTransportManagerUtils.ParseEscapeWSManErrorMessage(errorStruct.errorDetail) });
                        sessnTransportManager.ProcessWSManTransportError(eventArgs);
                        return;
                    }
                }
                lock (sessnTransportManager.syncObject)
                {
                    if (sessnTransportManager.isClosed)
                    {
                        BaseClientTransportManager.tracer.WriteLine("Client Session TM: Transport manager is closed. So returning", new object[0]);
                    }
                    else
                    {
                        sessnTransportManager.EnqueueAndStartProcessingThread(null, null, new CompletionEventArgs(CompletionNotification.DisconnectCompleted));
                    }
                }
            }
        }

        private static void OnRemoteSessionReconnectCompleted(IntPtr operationContext, int flags, IntPtr error, IntPtr shellOperationHandle, IntPtr commandOperationHandle, IntPtr operationHandle, IntPtr data)
        {
            BaseClientTransportManager.tracer.WriteLine("Client Session TM: CreateShell callback received", new object[0]);
            long sessnTMId = 0L;
            WSManClientSessionTransportManager sessnTransportManager = null;
            if (!TryGetSessionTransportManager(operationContext, out sessnTransportManager, out sessnTMId))
            {
                BaseClientTransportManager.tracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Unable to find a transport manager for context {0}.", new object[] { sessnTMId }), new object[0]);
            }
            else
            {
                if (sessnTransportManager.reconnectSessionCompleted != null)
                {
                    sessnTransportManager.reconnectSessionCompleted.Dispose();
                    sessnTransportManager.reconnectSessionCompleted = null;
                }
                if (IntPtr.Zero != error)
                {
                    WSManNativeApi.WSManError errorStruct = WSManNativeApi.WSManError.UnMarshal(error);
                    if (errorStruct.errorCode != 0)
                    {
                        BaseClientTransportManager.tracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Got error with error code {0}. Message {1}", new object[] { errorStruct.errorCode, errorStruct.errorDetail }), new object[0]);
                        TransportErrorOccuredEventArgs eventArgs = WSManTransportManagerUtils.ConstructTransportErrorEventArgs(wsManApiStaticData.WSManAPIHandle, sessnTransportManager, errorStruct, TransportMethodEnum.ReconnectShellEx, RemotingErrorIdStrings.ReconnectShellExCallBackErrr, new object[] { sessnTransportManager.ConnectionInfo.ComputerName, WSManTransportManagerUtils.ParseEscapeWSManErrorMessage(errorStruct.errorDetail) });
                        sessnTransportManager.ProcessWSManTransportError(eventArgs);
                        return;
                    }
                }
                lock (sessnTransportManager.syncObject)
                {
                    if (sessnTransportManager.isClosed)
                    {
                        BaseClientTransportManager.tracer.WriteLine("Client Session TM: Transport manager is closed. So returning", new object[0]);
                    }
                    else
                    {
                        sessnTransportManager.RaiseReconnectCompleted();
                    }
                }
            }
        }

        private static void OnRemoteSessionSendCompleted(IntPtr operationContext, int flags, IntPtr error, IntPtr shellOperationHandle, IntPtr commandOperationHandle, IntPtr operationHandle, IntPtr data)
        {
            BaseClientTransportManager.tracer.WriteLine("Client Session TM: SendComplete callback received", new object[0]);
            long sessnTMId = 0L;
            WSManClientSessionTransportManager sessnTransportManager = null;
            if (!TryGetSessionTransportManager(operationContext, out sessnTransportManager, out sessnTMId))
            {
                BaseClientTransportManager.tracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Unable to find a transport manager for context {0}.", new object[] { sessnTMId }), new object[0]);
            }
            else
            {
                PSEtwLog.LogAnalyticInformational(PSEventId.WSManSendShellInputExCallbackReceived, PSOpcode.Connect, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { sessnTransportManager.RunspacePoolInstanceId.ToString(), Guid.Empty.ToString() });
                if (!shellOperationHandle.Equals(sessnTransportManager.wsManShellOperationHandle))
                {
                    PSRemotingTransportException e = new PSRemotingTransportException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.SendExFailed, new object[] { sessnTransportManager.ConnectionInfo.ComputerName }));
                    TransportErrorOccuredEventArgs eventArgs = new TransportErrorOccuredEventArgs(e, TransportMethodEnum.SendShellInputEx);
                    sessnTransportManager.ProcessWSManTransportError(eventArgs);
                }
                else
                {
                    sessnTransportManager.ClearReceiveOrSendResources(flags, true);
                    if (sessnTransportManager.isClosed)
                    {
                        BaseClientTransportManager.tracer.WriteLine("Client Session TM: Transport manager is closed. So returning", new object[0]);
                    }
                    else
                    {
                        if (IntPtr.Zero != error)
                        {
                            WSManNativeApi.WSManError errorStruct = WSManNativeApi.WSManError.UnMarshal(error);
                            if ((errorStruct.errorCode != 0) && (errorStruct.errorCode != 0x3e3))
                            {
                                BaseClientTransportManager.tracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Got error with error code {0}. Message {1}", new object[] { errorStruct.errorCode, errorStruct.errorDetail }), new object[0]);
                                TransportErrorOccuredEventArgs args2 = WSManTransportManagerUtils.ConstructTransportErrorEventArgs(wsManApiStaticData.WSManAPIHandle, sessnTransportManager, errorStruct, TransportMethodEnum.SendShellInputEx, RemotingErrorIdStrings.SendExCallBackError, new object[] { sessnTransportManager.ConnectionInfo.ComputerName, WSManTransportManagerUtils.ParseEscapeWSManErrorMessage(errorStruct.errorDetail) });
                                sessnTransportManager.ProcessWSManTransportError(args2);
                                return;
                            }
                        }
                        sessnTransportManager.SendOneItem();
                    }
                }
            }
        }

        internal override void PrepareForRedirection()
        {
            this.closeSessionCompleted = new WSManNativeApi.WSManShellAsync(new IntPtr(this.sessionContextID), sessionCloseCallback);
            WSManNativeApi.WSManCloseShell(this.wsManShellOperationHandle, 0, (IntPtr) this.closeSessionCompleted);
        }

        internal override void ProcessPrivateData(object privateData)
        {
            ConnectionStatusEventArgs args = privateData as ConnectionStatusEventArgs;
            if (args != null)
            {
                base.RaiseRobustConnectionNotification(args);
            }
            else
            {
                CompletionEventArgs args2 = privateData as CompletionEventArgs;
                if ((args2 != null) && (args2.Notification == CompletionNotification.DisconnectCompleted))
                {
                    base.RaiseDisconnectCompleted();
                }
            }
        }

        private void ProcessShellData(string data)
        {
            try
            {
                XmlReaderSettings settings = InternalDeserializer.XmlReaderSettingsForUntrustedXmlDocument.Clone();
                settings.MaxCharactersFromEntities = 0x400L;
                settings.MaxCharactersInDocument = 0x7800L;
                settings.DtdProcessing = DtdProcessing.Prohibit;
                using (XmlReader reader = XmlReader.Create(new StringReader(data), settings))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            if (reader.LocalName.Equals("IdleTimeOut", StringComparison.OrdinalIgnoreCase) || reader.LocalName.Equals("MaxIdleTimeOut", StringComparison.OrdinalIgnoreCase))
                            {
                                bool flag = true;
                                if (reader.LocalName.Equals("MaxIdleTimeOut", StringComparison.OrdinalIgnoreCase))
                                {
                                    flag = false;
                                }
                                string str = reader.ReadElementString();
                                int index = str.IndexOf('.');
                                try
                                {
                                    int num2 = (Convert.ToInt32(str.Substring(2, index - 2), NumberFormatInfo.InvariantInfo) * 0x3e8) + Convert.ToInt32(str.Substring(index + 1, 3), NumberFormatInfo.InvariantInfo);
                                    if (flag)
                                    {
                                        this._connectionInfo.IdleTimeout = num2;
                                    }
                                    else
                                    {
                                        this._connectionInfo.MaxIdleTimeout = num2;
                                    }
                                }
                                catch (InvalidCastException)
                                {
                                }
                            }
                            else if (reader.LocalName.Equals("BufferMode", StringComparison.OrdinalIgnoreCase))
                            {
                                string str2 = reader.ReadElementString();
                                if (str2.Equals("Block", StringComparison.OrdinalIgnoreCase))
                                {
                                    this._connectionInfo.OutputBufferingMode = OutputBufferingMode.Block;
                                    continue;
                                }
                                if (str2.Equals("Drop", StringComparison.OrdinalIgnoreCase))
                                {
                                    this._connectionInfo.OutputBufferingMode = OutputBufferingMode.Drop;
                                }
                            }
                        }
                    }
                }
            }
            catch (XmlException)
            {
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
            PSEtwLog.LogOperationalError(PSEventId.TransportError, PSOpcode.Open, PSTask.None, PSKeyword.UseAlwaysOperational, new object[] { base.RunspacePoolInstanceId.ToString(), Guid.Empty.ToString(), eventArgs.Exception.ErrorCode.ToString(CultureInfo.InvariantCulture), eventArgs.Exception.Message, stackTrace });
            PSEtwLog.LogAnalyticError(PSEventId.TransportError_Analytic, PSOpcode.Open, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { base.RunspacePoolInstanceId.ToString(), Guid.Empty.ToString(), eventArgs.Exception.ErrorCode.ToString(CultureInfo.InvariantCulture), eventArgs.Exception.Message, stackTrace });
            base.RaiseErrorHandler(eventArgs);
        }

        internal override void ReconnectAsync()
        {
            base.ReceivedDataCollection.PrepareForStreamConnect();
            this.reconnectSessionCompleted = new WSManNativeApi.WSManShellAsync(new IntPtr(this.sessionContextID), sessionReconnectCallback);
            lock (base.syncObject)
            {
                if (!base.isClosed)
                {
                    int flags = 0;
                    flags |= (this._connectionInfo.OutputBufferingMode == OutputBufferingMode.Block) ? 8 : 0;
                    flags |= (this._connectionInfo.OutputBufferingMode == OutputBufferingMode.Drop) ? 4 : 0;
                    WSManNativeApi.WSManReconnectShellEx(this.wsManShellOperationHandle, flags, (IntPtr) this.reconnectSessionCompleted);
                }
            }
        }

        internal override void Redirect(Uri newUri, RunspaceConnectionInfo connectionInfo)
        {
            this.CloseSessionAndClearResources();
            BaseClientTransportManager.tracer.WriteLine("Redirecting to URI: {0}", new object[] { newUri });
            PSEtwLog.LogAnalyticInformational(PSEventId.URIRedirection, PSOpcode.Connect, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { base.RunspacePoolInstanceId.ToString(), newUri.ToString() });
            this.Initialize(newUri, (WSManConnectionInfo) connectionInfo);
            this.startMode = WSManTransportManagerUtils.tmStartModes.None;
            this.CreateAsync();
        }

        private static void RemoveSessionTransportManager(long sessnTMId)
        {
            lock (SessionTMHandles)
            {
                if (SessionTMHandles.ContainsKey(sessnTMId))
                {
                    SessionTMHandles[sessnTMId] = null;
                    SessionTMHandles.Remove(sessnTMId);
                }
            }
        }

        private void SendData(byte[] data, DataPriorityType priorityType)
        {
            BaseClientTransportManager.tracer.WriteLine("Session sending data of size : {0}", new object[] { data.Length });
            byte[] buffer = data;
            bool flag = true;
            if (sessionSendRedirect != null)
            {
                object[] objArray3 = new object[2];
                objArray3[1] = buffer;
                object[] args = objArray3;
                flag = (bool) sessionSendRedirect.DynamicInvoke(args);
                buffer = (byte[]) args[0];
            }
            if (flag)
            {
                using (WSManNativeApi.WSManData data2 = new WSManNativeApi.WSManData(buffer))
                {
                    PSEtwLog.LogAnalyticInformational(PSEventId.WSManSendShellInputEx, PSOpcode.Send, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { base.RunspacePoolInstanceId.ToString(), Guid.Empty.ToString(), data2.BufferLength.ToString(CultureInfo.InvariantCulture) });
                    lock (base.syncObject)
                    {
                        if (base.isClosed)
                        {
                            BaseClientTransportManager.tracer.WriteLine("Client Session TM: Transport manager is closed. So returning", new object[0]);
                        }
                        else
                        {
                            this.sendToRemoteCompleted = new WSManNativeApi.WSManShellAsync(new IntPtr(this.sessionContextID), sessionSendCallback);
                            WSManNativeApi.WSManSendShellInputEx(this.wsManShellOperationHandle, IntPtr.Zero, 0, (priorityType == DataPriorityType.Default) ? "stdin" : "pr", data2, (IntPtr) this.sendToRemoteCompleted, ref this.wsManSendOperationHandle);
                        }
                    }
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

        internal void SetCloseTimeOut(int milliseconds)
        {
            using (BaseClientTransportManager.tracer.TraceMethod("Setting CloseShell timeout: {0} milliseconds", new object[] { milliseconds }))
            {
                int errorCode = WSManNativeApi.WSManSetSessionOption(this.wsManSessionHandle, WSManNativeApi.WSManSessionOption.WSMAN_OPTION_TIMEOUTMS_CLOSE_SHELL_OPERATION, new WSManNativeApi.WSManDataDWord(milliseconds));
                if (errorCode != 0)
                {
                    PSInvalidOperationException exception = new PSInvalidOperationException(WSManNativeApi.WSManGetErrorMessage(wsManApiStaticData.WSManAPIHandle, errorCode));
                    throw exception;
                }
            }
        }

        internal void SetConnectTimeOut(int milliseconds)
        {
            using (BaseClientTransportManager.tracer.TraceMethod("Setting CreateShell timeout: {0} milliseconds", new object[] { milliseconds }))
            {
                int errorCode = WSManNativeApi.WSManSetSessionOption(this.wsManSessionHandle, WSManNativeApi.WSManSessionOption.WSMAN_OPTION_TIMEOUTMS_CREATE_SHELL, new WSManNativeApi.WSManDataDWord(milliseconds));
                if (errorCode != 0)
                {
                    PSInvalidOperationException exception = new PSInvalidOperationException(WSManNativeApi.WSManGetErrorMessage(wsManApiStaticData.WSManAPIHandle, errorCode));
                    throw exception;
                }
            }
        }

        internal void SetDefaultTimeOut(int milliseconds)
        {
            using (BaseClientTransportManager.tracer.TraceMethod("Setting Default timeout: {0} milliseconds", new object[] { milliseconds }))
            {
                int errorCode = WSManNativeApi.WSManSetSessionOption(this.wsManSessionHandle, WSManNativeApi.WSManSessionOption.WSMAN_OPTION_DEFAULT_OPERATION_TIMEOUTMS, new WSManNativeApi.WSManDataDWord(milliseconds));
                if (errorCode != 0)
                {
                    PSInvalidOperationException exception = new PSInvalidOperationException(WSManNativeApi.WSManGetErrorMessage(wsManApiStaticData.WSManAPIHandle, errorCode));
                    throw exception;
                }
            }
        }

        internal void SetReceiveTimeOut(int milliseconds)
        {
            using (BaseClientTransportManager.tracer.TraceMethod("Setting ReceiveShellOutput timeout: {0} milliseconds", new object[] { milliseconds }))
            {
                int errorCode = WSManNativeApi.WSManSetSessionOption(this.wsManSessionHandle, WSManNativeApi.WSManSessionOption.WSMAN_OPTION_TIMEOUTMS_RECEIVE_SHELL_OUTPUT, new WSManNativeApi.WSManDataDWord(milliseconds));
                if (errorCode != 0)
                {
                    PSInvalidOperationException exception = new PSInvalidOperationException(WSManNativeApi.WSManGetErrorMessage(wsManApiStaticData.WSManAPIHandle, errorCode));
                    throw exception;
                }
            }
        }

        internal void SetSendTimeOut(int milliseconds)
        {
            using (BaseClientTransportManager.tracer.TraceMethod("Setting SendShellInput timeout: {0} milliseconds", new object[] { milliseconds }))
            {
                int errorCode = WSManNativeApi.WSManSetSessionOption(this.wsManSessionHandle, WSManNativeApi.WSManSessionOption.WSMAN_OPTION_TIMEOUTMS_SEND_SHELL_INPUT, new WSManNativeApi.WSManDataDWord(milliseconds));
                if (errorCode != 0)
                {
                    PSInvalidOperationException exception = new PSInvalidOperationException(WSManNativeApi.WSManGetErrorMessage(wsManApiStaticData.WSManAPIHandle, errorCode));
                    throw exception;
                }
            }
        }

        internal void SetSignalTimeOut(int milliseconds)
        {
            using (BaseClientTransportManager.tracer.TraceMethod("Setting SignalShell timeout: {0} milliseconds", new object[] { milliseconds }))
            {
                int errorCode = WSManNativeApi.WSManSetSessionOption(this.wsManSessionHandle, WSManNativeApi.WSManSessionOption.WSMAN_OPTION_TIMEOUTMS_SIGNAL_SHELL, new WSManNativeApi.WSManDataDWord(milliseconds));
                if (errorCode != 0)
                {
                    PSInvalidOperationException exception = new PSInvalidOperationException(WSManNativeApi.WSManGetErrorMessage(wsManApiStaticData.WSManAPIHandle, errorCode));
                    throw exception;
                }
            }
        }

        internal void SetWSManSessionOption(WSManNativeApi.WSManSessionOption option, int dwordData)
        {
            int errorCode = WSManNativeApi.WSManSetSessionOption(this.wsManSessionHandle, option, new WSManNativeApi.WSManDataDWord(dwordData));
            if (errorCode != 0)
            {
                PSInvalidOperationException exception = new PSInvalidOperationException(WSManNativeApi.WSManGetErrorMessage(wsManApiStaticData.WSManAPIHandle, errorCode));
                throw exception;
            }
        }

        internal void SetWSManSessionOption(WSManNativeApi.WSManSessionOption option, string stringData)
        {
            using (WSManNativeApi.WSManData data = new WSManNativeApi.WSManData(stringData))
            {
                int errorCode = WSManNativeApi.WSManSetSessionOption(this.wsManSessionHandle, option, (IntPtr) data);
                if (errorCode != 0)
                {
                    PSInvalidOperationException exception = new PSInvalidOperationException(WSManNativeApi.WSManGetErrorMessage(wsManApiStaticData.WSManAPIHandle, errorCode));
                    throw exception;
                }
            }
        }

		internal void ResetReceivedData ()
		{
			this.receiveDataInitiated = false;
		}

        internal override void StartReceivingData()
        {
            lock (base.syncObject)
            {
                if (base.isClosed)
                {
                    BaseClientTransportManager.tracer.WriteLine("Client Session TM: Transport manager is closed. So returning", new object[0]);
                }
                else if (base.receiveDataInitiated)
				{
                    BaseClientTransportManager.tracer.WriteLine("Client Session TM: ReceiveData has already been called.", new object[0]);
				}
                else
                {
                    base.receiveDataInitiated = true;
                    BaseClientTransportManager.tracer.WriteLine("Client Session TM: Placing Receive request using WSManReceiveShellOutputEx", new object[0]);
                    PSEtwLog.LogAnalyticInformational(PSEventId.WSManReceiveShellOutputEx, PSOpcode.Receive, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { base.RunspacePoolInstanceId.ToString(), Guid.Empty.ToString() });
                    this.receivedFromRemote = new WSManNativeApi.WSManShellAsync(new IntPtr(this.sessionContextID), sessionReceiveCallback);
                    WSManNativeApi.WSManReceiveShellOutputEx(this.wsManShellOperationHandle, IntPtr.Zero, 0, (IntPtr) wsManApiStaticData.OutputStreamSet, (IntPtr) this.receivedFromRemote, ref this.wsManRecieveOperationHandle);
                }
            }
        }

        private static bool TryGetSessionTransportManager(IntPtr operationContext, out WSManClientSessionTransportManager sessnTransportManager, out long sessnTMId)
        {
            sessnTMId = operationContext.ToInt64();
            sessnTransportManager = null;
            lock (SessionTMHandles)
            {
                return SessionTMHandles.TryGetValue(sessnTMId, out sessnTransportManager);
            }
        }

        internal WSManConnectionInfo ConnectionInfo
        {
            get
            {
                return this._connectionInfo;
            }
        }

        internal int MaxRetryConnectionTime
        {
            get
            {
                return this.maxRetryTime;
            }
        }

        internal IntPtr SessionHandle
        {
            get
            {
                return this.wsManSessionHandle;
            }
        }

        internal bool SupportsDisconnect
        {
            get
            {
                return this.supportsDisconnect;
            }
        }

        private class CompletionEventArgs : EventArgs
        {
            private WSManClientSessionTransportManager.CompletionNotification _notification;

            internal CompletionEventArgs(WSManClientSessionTransportManager.CompletionNotification notification)
            {
                this._notification = notification;
            }

            internal WSManClientSessionTransportManager.CompletionNotification Notification
            {
                get
                {
                    return this._notification;
                }
            }
        }

        private enum CompletionNotification
        {
            DisconnectCompleted
        }

        internal class WSManAPIStaticData : IDisposable
        {
            private List<WSManNativeApi.WSManOption> commonOptionSet;
            private int errorCode;
            private IntPtr handle = IntPtr.Zero;
            private WSManNativeApi.WSManStreamIDSet inputStreamSet;
            private WSManNativeApi.WSManStreamIDSet outputStreamSet;

            internal WSManAPIStaticData()
            {
                this.errorCode = WSManNativeApi.WSManInitialize(1, ref this.handle);
                this.inputStreamSet = new WSManNativeApi.WSManStreamIDSet(new string[] { "stdin", "pr" });
                this.outputStreamSet = new WSManNativeApi.WSManStreamIDSet(new string[] { "stdout" });
                WSManNativeApi.WSManOption item = new WSManNativeApi.WSManOption {
                    name = "protocolversion",
                    value = RemotingConstants.ProtocolVersion.ToString(),
                    mustComply = true
                };
                this.commonOptionSet = new List<WSManNativeApi.WSManOption>();
                this.commonOptionSet.Add(item);
            }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool isDisposing)
            {
                if (!isDisposing)
                {
                    this.inputStreamSet.Dispose();
                    this.outputStreamSet.Dispose();
                    if (IntPtr.Zero != this.handle)
                    {
                        WSManNativeApi.WSManDeinitialize(this.handle, 0);
                        this.handle = IntPtr.Zero;
                    }
                }
            }

            ~WSManAPIStaticData()
            {
                this.Dispose(false);
            }

            internal List<WSManNativeApi.WSManOption> CommonOptionSet
            {
                get
                {
                    return this.commonOptionSet;
                }
            }

            internal int ErrorCode
            {
                get
                {
                    return this.errorCode;
                }
            }

            internal WSManNativeApi.WSManStreamIDSet InputStreamSet
            {
                get
                {
                    return this.inputStreamSet;
                }
            }

            internal WSManNativeApi.WSManStreamIDSet OutputStreamSet
            {
                get
                {
                    return this.outputStreamSet;
                }
            }

            internal IntPtr WSManAPIHandle
            {
                get
                {
                    return this.handle;
                }
            }
        }
    }
}

