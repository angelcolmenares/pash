namespace System.Management.Automation.Remoting
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting.Server;
    using System.Management.Automation.Runspaces;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.Threading;

    internal class ServerRemoteSession : RemoteSession
    {
        private string _configProviderId;
        private ServerRemoteSessionContext _context;
        private PSRemotingCryptoHelperServer _cryptoHelper;
        private string _initParameters;
        private string _initScriptForOutOfProcRS;
        private ServerRunspacePoolDriver _runspacePoolDriver;
        private PSSenderInfo _senderInfo;
        private PSSessionConfiguration _sessionConfigProvider;
        private ServerRemoteSessionDataStructureHandler _sessionDSHandler;
        [TraceSource("ServerRemoteSession", "ServerRemoteSession")]
        private static PSTraceSource _trace = PSTraceSource.GetTracer("ServerRemoteSession", "ServerRemoteSession");
        internal EventHandler<RemoteSessionStateMachineEventArgs> Closed;
        private int? maxRecvdDataSizeCommand;
        private int? maxRecvdObjectSize;

        internal ServerRemoteSession(PSSenderInfo senderInfo, string configurationProviderId, string initializationParameters, AbstractServerSessionTransportManager transportManager)
        {
            NativeCommandProcessor.IsServerSide = true;
            this._senderInfo = senderInfo;
            this._configProviderId = configurationProviderId;
            this._initParameters = initializationParameters;
            this._cryptoHelper = (PSRemotingCryptoHelperServer) transportManager.CryptoHelper;
			this._cryptoHelper.Session = this;
            this._context = new ServerRemoteSessionContext();
            this._sessionDSHandler = new ServerRemoteSessionDSHandlerlImpl(this, transportManager);
            base.BaseSessionDataStructureHandler = this._sessionDSHandler;
            this._sessionDSHandler.CreateRunspacePoolReceived += new EventHandler<RemoteDataEventArgs>(this.HandleCreateRunspacePool);
            this._sessionDSHandler.NegotiationReceived += new EventHandler<RemoteSessionNegotiationEventArgs>(this.HandleNegotiationReceived);
            this._sessionDSHandler.SessionClosing += new EventHandler<EventArgs>(this.HandleSessionDSHandlerClosing);
            this._sessionDSHandler.PublicKeyReceived += new EventHandler<RemoteDataEventArgs<string>>(this.HandlePublicKeyReceived);
            transportManager.Closing += new EventHandler(this.HandleResourceClosing);
            transportManager.ReceivedDataCollection.MaximumReceivedObjectSize = 0xa00000;
            transportManager.ReceivedDataCollection.MaximumReceivedDataSize = null;
        }

        internal void ApplyQuotaOnCommandTransportManager(AbstractServerTransportManager cmdTransportManager)
        {
            cmdTransportManager.ReceivedDataCollection.MaximumReceivedDataSize = this.maxRecvdDataSizeCommand;
            cmdTransportManager.ReceivedDataCollection.MaximumReceivedObjectSize = this.maxRecvdObjectSize;
        }

        internal void Close(RemoteSessionStateMachineEventArgs reasonForClose)
        {
            this.Closed.SafeInvoke<RemoteSessionStateMachineEventArgs>(this, reasonForClose);
            if (this._runspacePoolDriver != null)
            {
                this._runspacePoolDriver.Closed = (EventHandler<EventArgs>) Delegate.Remove(this._runspacePoolDriver.Closed, new EventHandler<EventArgs>(this.HandleResourceClosing));
            }
        }

        internal override void CompleteKeyExchange()
        {
            this._cryptoHelper.CompleteKeyExchange();
        }

        internal static ServerRemoteSession CreateServerRemoteSession(PSSenderInfo senderInfo, string initializationScriptForOutOfProcessRunspace, AbstractServerSessionTransportManager transportManager)
        {
            ServerRemoteSession session = CreateServerRemoteSession(senderInfo, "Microsoft.PowerShell", "", transportManager);
            session._initScriptForOutOfProcRS = initializationScriptForOutOfProcessRunspace;
            return session;
        }

        internal static ServerRemoteSession CreateServerRemoteSession(PSSenderInfo senderInfo, string configurationProviderId, string initializationParameters, AbstractServerSessionTransportManager transportManager)
        {
            _trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "Finding InitialSessionState provider for id : {0}", new object[] { configurationProviderId }), new object[0]);
            if (string.IsNullOrEmpty(configurationProviderId))
            {
                throw PSTraceSource.NewInvalidOperationException("remotingerroridstrings", "NonExistentInitialSessionStateProvider", new object[] { configurationProviderId });
            }
            ServerRemoteSession session = new ServerRemoteSession(senderInfo, configurationProviderId, initializationParameters, transportManager);
            RemoteSessionStateMachineEventArgs fsmEventArg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.CreateSession);
            session._sessionDSHandler.StateMachine.RaiseEvent(fsmEventArg);
            return session;
        }

        internal void DispatchInputQueueData(object sender, RemoteDataEventArgs dataEventArg)
        {
            if (dataEventArg == null)
            {
                throw PSTraceSource.NewArgumentNullException("dataEventArg");
            }
            RemoteDataObject<PSObject> receivedData = dataEventArg.ReceivedData;
            if (receivedData == null)
            {
                throw PSTraceSource.NewArgumentException("dataEventArg");
            }
            RemotingDestination destination = receivedData.Destination;
            if ((destination & this.MySelf) != this.MySelf)
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.RemotingDestinationNotForMe, new object[] { this.MySelf, destination });
            }
            RemotingTargetInterface targetInterface = receivedData.TargetInterface;
            RemotingDataType dataType = receivedData.DataType;
            RemoteSessionStateMachineEventArgs arg = null;
            switch (targetInterface)
            {
                case RemotingTargetInterface.Session:
                    switch (dataType)
                    {
                        case RemotingDataType.SessionCapability:
                            this._sessionDSHandler.RaiseDataReceivedEvent(dataEventArg);
                            return;

                        case RemotingDataType.CloseSession:
                            this._sessionDSHandler.RaiseDataReceivedEvent(dataEventArg);
                            return;

                        case RemotingDataType.CreateRunspacePool:
                            arg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.MessageReceived);
                            if (this.SessionDataStructureHandler.StateMachine.CanByPassRaiseEvent(arg))
                            {
                                arg.RemoteData = receivedData;
                                this.SessionDataStructureHandler.StateMachine.DoMessageReceived(this, arg);
                                return;
                            }
                            this.SessionDataStructureHandler.StateMachine.RaiseEvent(arg);
                            return;

                        case RemotingDataType.PublicKey:
                            this._sessionDSHandler.RaiseDataReceivedEvent(dataEventArg);
                            return;
                    }
                    return;

                case RemotingTargetInterface.RunspacePool:
                case RemotingTargetInterface.PowerShell:
                    arg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.MessageReceived);
                    if (!this.SessionDataStructureHandler.StateMachine.CanByPassRaiseEvent(arg))
                    {
                        this.SessionDataStructureHandler.StateMachine.RaiseEvent(arg);
                        return;
                    }
                    arg.RemoteData = receivedData;
                    this.SessionDataStructureHandler.StateMachine.DoMessageReceived(this, arg);
                    return;
            }
        }

        internal void ExecuteConnect(byte[] connectData, out byte[] connectResponseData)
        {
            RemoteSessionCapability sessionCapability;
            connectResponseData = null;
            Fragmentor fragmentor = new Fragmentor(0x7fffffff, null);
            Fragmentor defragmentor = fragmentor;
            int length = connectData.Length;
            if (length < 0x15)
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.ServerConnectFailedOnInputValidation);
            }
            FragmentedRemoteObject.GetFragmentId(connectData, 0);
            bool isStartFragment = FragmentedRemoteObject.GetIsStartFragment(connectData, 0);
            bool isEndFragment = FragmentedRemoteObject.GetIsEndFragment(connectData, 0);
            int blobLength = FragmentedRemoteObject.GetBlobLength(connectData, 0);
            if (blobLength > (length - 0x15))
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.ServerConnectFailedOnInputValidation);
            }
            if (!isStartFragment || !isEndFragment)
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.ServerConnectFailedOnInputValidation);
            }
            RemoteSessionState state = this.SessionDataStructureHandler.StateMachine.State;
            if ((state != RemoteSessionState.Established) && (state != RemoteSessionState.EstablishedAndKeyExchanged))
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.ServerConnectFailedOnServerStateValidation);
            }
            MemoryStream serializedDataStream = new MemoryStream();
            serializedDataStream.Write(connectData, 0x15, blobLength);
            serializedDataStream.Seek(0L, SeekOrigin.Begin);
            RemoteDataObject<PSObject> obj2 = RemoteDataObject<PSObject>.CreateFrom(serializedDataStream, defragmentor);
            if (obj2 == null)
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.ServerConnectFailedOnInputValidation);
            }
            if ((obj2.Destination != (RemotingDestination.InvalidDestination | RemotingDestination.Server)) || (obj2.DataType != RemotingDataType.SessionCapability))
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.ServerConnectFailedOnInputValidation);
            }
            int num3 = (length - 0x15) - blobLength;
            if (num3 < 0x15)
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.ServerConnectFailedOnInputValidation);
            }
            byte[] destinationArray = new byte[num3];
            Array.Copy(connectData, 0x15 + blobLength, destinationArray, 0, num3);
            FragmentedRemoteObject.GetFragmentId(destinationArray, 0);
            isStartFragment = FragmentedRemoteObject.GetIsStartFragment(destinationArray, 0);
            isEndFragment = FragmentedRemoteObject.GetIsEndFragment(destinationArray, 0);
            blobLength = FragmentedRemoteObject.GetBlobLength(destinationArray, 0);
            if (blobLength != (num3 - 0x15))
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.ServerConnectFailedOnInputValidation);
            }
            if (!isStartFragment || !isEndFragment)
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.ServerConnectFailedOnInputValidation);
            }
            serializedDataStream = new MemoryStream();
            serializedDataStream.Write(destinationArray, 0x15, blobLength);
            serializedDataStream.Seek(0L, SeekOrigin.Begin);
            RemoteDataObject<PSObject> obj3 = RemoteDataObject<PSObject>.CreateFrom(serializedDataStream, defragmentor);
            if (obj3 == null)
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.ServerConnectFailedOnServerStateValidation);
            }
            if ((obj3.Destination != (RemotingDestination.InvalidDestination | RemotingDestination.Server)) || (obj3.DataType != RemotingDataType.ConnectRunspacePool))
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.ServerConnectFailedOnInputValidation);
            }
            try
            {
                sessionCapability = RemotingDecoder.GetSessionCapability(obj2.Data);
            }
            catch (PSRemotingDataStructureException)
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.ServerConnectFailedOnInputValidation);
            }
            try
            {
                this.RunServerNegotiationAlgorithm(sessionCapability, true);
            }
            catch (PSRemotingDataStructureException exception)
            {
                throw exception;
            }
            int minRunspaces = -1;
            int maxRunspaces = -1;
            bool flag3 = false;
            if ((obj3.Data.Properties["MinRunspaces"] != null) && (obj3.Data.Properties["MinRunspaces"] != null))
            {
                try
                {
                    minRunspaces = RemotingDecoder.GetMinRunspaces(obj3.Data);
                    maxRunspaces = RemotingDecoder.GetMaxRunspaces(obj3.Data);
                    flag3 = true;
                }
                catch (PSRemotingDataStructureException)
                {
                    throw new PSRemotingDataStructureException(RemotingErrorIdStrings.ServerConnectFailedOnInputValidation);
                }
            }
            if (flag3 && (((minRunspaces == -1) || (maxRunspaces == -1)) || (minRunspaces > maxRunspaces)))
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.ServerConnectFailedOnInputValidation);
            }
            if (this._runspacePoolDriver == null)
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.ServerConnectFailedOnServerStateValidation);
            }
            if (obj3.RunspacePoolId != this._runspacePoolDriver.InstanceId)
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.ServerConnectFailedOnInputValidation);
            }
            if ((flag3 && (this._runspacePoolDriver.RunspacePool.GetMaxRunspaces() != maxRunspaces)) && (this._runspacePoolDriver.RunspacePool.GetMinRunspaces() != minRunspaces))
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.ServerConnectFailedOnMismatchedRunspacePoolProperties);
            }
            RemoteDataObject obj4 = RemotingEncoder.GenerateServerSessionCapability(this._context.ServerCapability, this._runspacePoolDriver.InstanceId);
            RemoteDataObject obj5 = RemotingEncoder.GenerateRunspacePoolInitData(this._runspacePoolDriver.InstanceId, this._runspacePoolDriver.RunspacePool.GetMaxRunspaces(), this._runspacePoolDriver.RunspacePool.GetMinRunspaces());
            SerializedDataStream streamToWriteTo = new SerializedDataStream(0x1000);
            streamToWriteTo.Enter();
            obj4.Serialize(streamToWriteTo, fragmentor);
            streamToWriteTo.Exit();
            streamToWriteTo.Enter();
            obj5.Serialize(streamToWriteTo, fragmentor);
            streamToWriteTo.Exit();
            byte[] buffer2 = streamToWriteTo.Read();
            streamToWriteTo.Dispose();
            connectResponseData = buffer2;
            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate (object s) {
                RemoteSessionStateMachineEventArgs fsmEventArg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.ConnectSession);
                this._sessionDSHandler.StateMachine.RaiseEvent(fsmEventArg);
            }));
            this._runspacePoolDriver.DataStructureHandler.ProcessConnect();
        }

        internal ServerRunspacePoolDriver GetRunspacePoolDriver(Guid clientRunspacePoolId)
        {
            if ((this._runspacePoolDriver != null) && (this._runspacePoolDriver.InstanceId == clientRunspacePoolId))
            {
                return this._runspacePoolDriver;
            }
            return null;
        }

        private void HandleCreateRunspacePool(object sender, RemoteDataEventArgs createRunspaceEventArg)
        {
            if (createRunspaceEventArg == null)
            {
                throw PSTraceSource.NewArgumentNullException("createRunspaceEventArg");
            }
            RemoteDataObject<PSObject> receivedData = createRunspaceEventArg.ReceivedData;
            if (this._context != null)
            {
				if (this._context.ClientCapability == null) {
					this._senderInfo.ClientTimeZone = this._context.ServerCapability.TimeZone;
				}
				else {
					this._senderInfo.ClientTimeZone = this._context.ClientCapability.TimeZone;
				}
            }
            this._senderInfo.ApplicationArguments = RemotingDecoder.GetApplicationArguments(receivedData.Data);
            ConfigurationDataFromXML configData = PSSessionConfiguration.LoadEndPointConfiguration(this._configProviderId, this._initParameters);
            configData.InitializationScriptForOutOfProcessRunspace = this._initScriptForOutOfProcRS;
            this.maxRecvdObjectSize = configData.MaxReceivedObjectSizeMB;
            this.maxRecvdDataSizeCommand = configData.MaxReceivedCommandSizeMB;
            DISCPowerShellConfiguration configuration = null;
            if (string.IsNullOrEmpty(configData.ConfigFilePath))
            {
                this._sessionConfigProvider = configData.CreateEndPointConfigurationInstance();
            }
            else
            {
                configuration = new DISCPowerShellConfiguration(configData.ConfigFilePath);
                this._sessionConfigProvider = configuration;
            }
            PSPrimitiveDictionary applicationPrivateData = this._sessionConfigProvider.GetApplicationPrivateData(this._senderInfo);
            InitialSessionState initialSessionState = null;
            if (configData.SessionConfigurationData != null)
            {
                try
                {
                    initialSessionState = this._sessionConfigProvider.GetInitialSessionState(configData.SessionConfigurationData, this._senderInfo, this._configProviderId);
                }
                catch (NotImplementedException)
                {
                    initialSessionState = this._sessionConfigProvider.GetInitialSessionState(this._senderInfo);
                }
            }
            else
            {
                initialSessionState = this._sessionConfigProvider.GetInitialSessionState(this._senderInfo);
            }
            if (initialSessionState == null)
            {
                throw PSTraceSource.NewInvalidOperationException("RemotingErrorIdStrings", "InitialSessionStateNull", new object[] { this._configProviderId });
            }
            initialSessionState.ThrowOnRunspaceOpenError = true;
            initialSessionState.Variables.Add(new SessionStateVariableEntry("PSSenderInfo", this._senderInfo, PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.PSSenderInfoDescription, new object[0]), ScopedItemOptions.ReadOnly));
            if ((this._senderInfo.ApplicationArguments != null) && this._senderInfo.ApplicationArguments.ContainsKey("PSversionTable"))
            {
                PSPrimitiveDictionary dictionary2 = PSObject.Base(this._senderInfo.ApplicationArguments["PSversionTable"]) as PSPrimitiveDictionary;
                if ((dictionary2 != null) && dictionary2.ContainsKey("WSManStackVersion"))
                {
                    Version version = PSObject.Base(dictionary2["WSManStackVersion"]) as Version;
                    if ((version != null) && (version.Major < 3))
                    {
                        initialSessionState.Commands.Add(new SessionStateFunctionEntry("TabExpansion", "\r\n            param($line, $lastWord)\r\n            & {\r\n                function Write-Members ($sep='.')\r\n                {\r\n                    Invoke-Expression ('$_val=' + $_expression)\r\n\r\n                    $_method = [Management.Automation.PSMemberTypes] `\r\n                        'Method,CodeMethod,ScriptMethod,ParameterizedProperty'\r\n                    if ($sep -eq '.')\r\n                    {\r\n                        $params = @{view = 'extended','adapted','base'}\r\n                    }\r\n                    else\r\n                    {\r\n                        $params = @{static=$true}\r\n                    }\r\n        \r\n                    foreach ($_m in ,$_val | Get-Member @params $_pat |\r\n                        Sort-Object membertype,name)\r\n                    {\r\n                        if ($_m.MemberType -band $_method)\r\n                        {\r\n                            # Return a method...\r\n                            $_base + $_expression + $sep + $_m.name + '('\r\n                        }\r\n                        else {\r\n                            # Return a property...\r\n                            $_base + $_expression + $sep + $_m.name\r\n                        }\r\n                    }\r\n                }\r\n\r\n                # If a command name contains any of these chars, it needs to be quoted\r\n                $_charsRequiringQuotes = ('`&@''#{}()$,;|<> ' + \"`t\").ToCharArray()\r\n\r\n                # If a variable name contains any of these characters it needs to be in braces\r\n                $_varsRequiringQuotes = ('-`&@''#{}()$,;|<> .\\/' + \"`t\").ToCharArray()\r\n\r\n                switch -regex ($lastWord)\r\n                {\r\n                    # Handle property and method expansion rooted at variables...\r\n                    # e.g. $a.b.<tab>\r\n                    '(^.*)(\\$(\\w|:|\\.)+)\\.([*\\w]*)$' {\r\n                        $_base = $matches[1]\r\n                        $_expression = $matches[2]\r\n                        $_pat = $matches[4] + '*'\r\n                        Write-Members\r\n                        break;\r\n                    }\r\n\r\n                    # Handle simple property and method expansion on static members...\r\n                    # e.g. [datetime]::n<tab>\r\n                    '(^.*)(\\[(\\w|\\.|\\+)+\\])(\\:\\:|\\.){0,1}([*\\w]*)$' {\r\n                        $_base = $matches[1]\r\n                        $_expression = $matches[2]\r\n                        $_pat = $matches[5] + '*'\r\n                        Write-Members $(if (! $matches[4]) {'::'} else {$matches[4]})\r\n                        break;\r\n                    }\r\n\r\n                    # Handle complex property and method expansion on static members\r\n                    # where there are intermediate properties...\r\n                    # e.g. [datetime]::now.d<tab>\r\n                    '(^.*)(\\[(\\w|\\.|\\+)+\\](\\:\\:|\\.)(\\w+\\.)+)([*\\w]*)$' {\r\n                        $_base = $matches[1]  # everything before the expression\r\n                        $_expression = $matches[2].TrimEnd('.') # expression less trailing '.'\r\n                        $_pat = $matches[6] + '*'  # the member to look for...\r\n                        Write-Members\r\n                        break;\r\n                    }\r\n\r\n                    # Handle variable name expansion...\r\n                    '(^.*\\$)([*\\w:]+)$' {\r\n                        $_prefix = $matches[1]\r\n                        $_varName = $matches[2]\r\n                        $_colonPos = $_varname.IndexOf(':')\r\n                        if ($_colonPos -eq -1)\r\n                        {\r\n                            $_varName = 'variable:' + $_varName\r\n                            $_provider = ''\r\n                        }\r\n                        else\r\n                        {\r\n                            $_provider = $_varname.Substring(0, $_colonPos+1)\r\n                        }\r\n\r\n                        foreach ($_v in Get-ChildItem ($_varName + '*') | sort Name)\r\n                        { \r\n                            $_nameFound = $_v.name\r\n                            $(if ($_nameFound.IndexOfAny($_varsRequiringQuotes) -eq -1) {'{0}{1}{2}'}\r\n                            else {'{0}{{{1}{2}}}'}) -f $_prefix, $_provider, $_nameFound\r\n                        }\r\n                        break;\r\n                    }\r\n\r\n                    # Do completion on parameters...\r\n                    '^-([*\\w0-9]*)' {\r\n                        $_pat = $matches[1] + '*'\r\n\r\n                        # extract the command name from the string\r\n                        # first split the string into statements and pipeline elements\r\n                        # This doesn't handle strings however.\r\n                        $_command = [regex]::Split($line, '[|;=]')[-1]\r\n\r\n                        #  Extract the trailing unclosed block e.g. ls | foreach { cp\r\n                        if ($_command -match '\\{([^\\{\\}]*)$')\r\n                        {\r\n                            $_command = $matches[1]\r\n                        }\r\n\r\n                        # Extract the longest unclosed parenthetical expression...\r\n                        if ($_command -match '\\(([^()]*)$')\r\n                        {\r\n                            $_command = $matches[1]\r\n                        }\r\n\r\n                        # take the first space separated token of the remaining string\r\n                        # as the command to look up. Trim any leading or trailing spaces\r\n                        # so you don't get leading empty elements.\r\n                        $_command = $_command.TrimEnd('-')\r\n                        $_command,$_arguments = $_command.Trim().Split()\r\n\r\n                        # now get the info object for it, -ArgumentList will force aliases to be resolved\r\n                        # it also retrieves dynamic parameters\r\n                        try\r\n                        {\r\n                            $_command = @(Get-Command -type 'Alias,Cmdlet,Function,Filter,ExternalScript' `\r\n                                -Name $_command -ArgumentList $_arguments)[0]\r\n                        }\r\n                        catch\r\n                        {\r\n                            # see if the command is an alias. If so, resolve it to the real command\r\n                            if(Test-Path alias:\\$_command)\r\n                            {\r\n                                $_command = @(Get-Command -Type Alias $_command)[0].Definition\r\n                            }\r\n\r\n                            # If we were unsuccessful retrieving the command, try again without the parameters\r\n                            $_command = @(Get-Command -type 'Cmdlet,Function,Filter,ExternalScript' `\r\n                                -Name $_command)[0]\r\n                        }\r\n\r\n                        # remove errors generated by the command not being found, and break\r\n                        if(-not $_command) { $error.RemoveAt(0); break; }\r\n\r\n                        # expand the parameter sets and emit the matching elements\r\n                        # need to use psbase.Keys in case 'keys' is one of the parameters\r\n                        # to the cmdlet\r\n                        foreach ($_n in $_command.Parameters.psbase.Keys)\r\n                        {\r\n                            if ($_n -like $_pat) { '-' + $_n }\r\n                        }\r\n                        break;\r\n                    }\r\n\r\n                    # Tab complete against history either #<pattern> or #<id>\r\n                    '^#(\\w*)' {\r\n                        $_pattern = $matches[1]\r\n                        if ($_pattern -match '^[0-9]+$')\r\n                        {\r\n                            Get-History -ea SilentlyContinue -Id $_pattern | Foreach { $_.CommandLine } \r\n                        }\r\n                        else\r\n                        {\r\n                            $_pattern = '*' + $_pattern + '*'\r\n                            Get-History -Count 32767 | Sort-Object -Descending Id| Foreach { $_.CommandLine } | where { $_ -like $_pattern }\r\n                        }\r\n                        break;\r\n                    }\r\n\r\n                    # try to find a matching command...\r\n                    default {\r\n                        # parse the script...\r\n                        $_tokens = [System.Management.Automation.PSParser]::Tokenize($line,\r\n                            [ref] $null)\r\n\r\n                        if ($_tokens)\r\n                        {\r\n                            $_lastToken = $_tokens[$_tokens.count - 1]\r\n                            if ($_lastToken.Type -eq 'Command')\r\n                            {\r\n                                $_cmd = $_lastToken.Content\r\n\r\n                                # don't look for paths...\r\n                                if ($_cmd.IndexOfAny('/\\:') -eq -1)\r\n                                {\r\n                                    # handle parsing errors - the last token string should be the last\r\n                                    # string in the line...\r\n                                    if ($lastword.Length -ge $_cmd.Length -and \r\n                                        $lastword.substring($lastword.length-$_cmd.length) -eq $_cmd)\r\n                                    {\r\n                                        $_pat = $_cmd + '*'\r\n                                        $_base = $lastword.substring(0, $lastword.length-$_cmd.length)\r\n\r\n                                        # get files in current directory first, then look for commands...\r\n                                        $( try {Resolve-Path -ea SilentlyContinue -Relative $_pat } catch {} ;\r\n                                           try { $ExecutionContext.InvokeCommand.GetCommandName($_pat, $true, $false) |\r\n                                               Sort-Object -Unique } catch {} ) |\r\n                                                   # If the command contains non-word characters (space, ) ] ; ) etc.)\r\n                                                   # then it needs to be quoted and prefixed with &\r\n                                                   ForEach-Object {\r\n                                                        if ($_.IndexOfAny($_charsRequiringQuotes) -eq -1) { $_ }\r\n                                                        elseif ($_.IndexOf('''') -ge 0) {'& ''{0}''' -f $_.Replace('''','''''') }\r\n                                                        else { '& ''{0}''' -f $_ }} |\r\n                                                   ForEach-Object {'{0}{1}' -f $_base,$_ }\r\n                                    }\r\n                                }\r\n                            }\r\n                        }\r\n                    }\r\n                }\r\n            }\r\n        "));
                    }
                }
            }
            if (!string.IsNullOrEmpty(configData.EndPointConfigurationTypeName))
            {
                this.maxRecvdObjectSize = this._sessionConfigProvider.GetMaximumReceivedObjectSize(this._senderInfo);
                this.maxRecvdDataSizeCommand = this._sessionConfigProvider.GetMaximumReceivedDataSizePerCommand(this._senderInfo);
            }
            this._sessionDSHandler.TransportManager.ReceivedDataCollection.MaximumReceivedObjectSize = this.maxRecvdObjectSize;
            Guid runspacePoolId = receivedData.RunspacePoolId;
            int minRunspaces = RemotingDecoder.GetMinRunspaces(receivedData.Data);
            int maxRunspaces = RemotingDecoder.GetMaxRunspaces(receivedData.Data);
            PSThreadOptions threadOptions = RemotingDecoder.GetThreadOptions(receivedData.Data);
            ApartmentState apartmentState = RemotingDecoder.GetApartmentState(receivedData.Data);
            HostInfo hostInfo = RemotingDecoder.GetHostInfo(receivedData.Data);
            if (this._runspacePoolDriver != null)
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.RunspaceAlreadyExists, new object[] { this._runspacePoolDriver.InstanceId });
            }
            bool isAdministrator = this._senderInfo.UserInfo.IsInRole(WindowsBuiltInRole.Administrator);
            ServerRunspacePoolDriver driver = new ServerRunspacePoolDriver(runspacePoolId, minRunspaces, maxRunspaces, threadOptions, apartmentState, hostInfo, initialSessionState, applicationPrivateData, configData, this.SessionDataStructureHandler.TransportManager, isAdministrator, this._context.ServerCapability, (configuration == null) ? null : configuration.ConfigHash);
            Interlocked.Exchange<ServerRunspacePoolDriver>(ref this._runspacePoolDriver, driver);
            this._runspacePoolDriver.Closed = (EventHandler<EventArgs>) Delegate.Combine(this._runspacePoolDriver.Closed, new EventHandler<EventArgs>(this.HandleResourceClosing));
            this._runspacePoolDriver.Start();
        }

        private void HandleNegotiationReceived(object sender, RemoteSessionNegotiationEventArgs negotiationEventArg)
        {
            if (negotiationEventArg == null)
            {
                throw PSTraceSource.NewArgumentNullException("negotiationEventArg");
            }
            try
            {
                this._context.ClientCapability = negotiationEventArg.RemoteSessionCapability;
                this.RunServerNegotiationAlgorithm(negotiationEventArg.RemoteSessionCapability, false);
                RemoteSessionStateMachineEventArgs fsmEventArg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.NegotiationSending);
                this._sessionDSHandler.StateMachine.RaiseEvent(fsmEventArg);
                RemoteSessionStateMachineEventArgs args2 = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.NegotiationCompleted);
                this._sessionDSHandler.StateMachine.RaiseEvent(args2);
            }
            catch (PSRemotingDataStructureException exception)
            {
                RemoteSessionStateMachineEventArgs args3 = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.NegotiationSending);
                this._sessionDSHandler.StateMachine.RaiseEvent(args3);
                RemoteSessionStateMachineEventArgs args4 = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.NegotiationFailed, exception);
                this._sessionDSHandler.StateMachine.RaiseEvent(args4);
            }
        }

        internal void HandlePostConnect()
        {
            if (this._runspacePoolDriver != null)
            {
                this._runspacePoolDriver.SendApplicationPrivateDataToClient();
            }
        }

        private void HandlePublicKeyReceived(object sender, RemoteDataEventArgs<string> eventArgs)
        {
            if (((this.SessionDataStructureHandler.StateMachine.State == RemoteSessionState.Established) || (this.SessionDataStructureHandler.StateMachine.State == RemoteSessionState.EstablishedAndKeyRequested)) || (this.SessionDataStructureHandler.StateMachine.State == RemoteSessionState.EstablishedAndKeyExchanged))
            {
                string data = eventArgs.Data;
                bool flag = this._cryptoHelper.ImportRemotePublicKey(data);
                RemoteSessionStateMachineEventArgs fsmEventArg = null;
                if (!flag)
                {
                    fsmEventArg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.KeyReceiveFailed);
                    this.SessionDataStructureHandler.StateMachine.RaiseEvent(fsmEventArg);
                }
                fsmEventArg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.KeyReceived);
                this.SessionDataStructureHandler.StateMachine.RaiseEvent(fsmEventArg);
            }
        }

        private void HandleResourceClosing(object sender, EventArgs args)
        {
            RemoteSessionStateMachineEventArgs fsmEventArg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.Close) {
                RemoteData = null
            };
            this._sessionDSHandler.StateMachine.RaiseEvent(fsmEventArg);
        }

        private void HandleSessionDSHandlerClosing(object sender, EventArgs eventArgs)
        {
            if (this._runspacePoolDriver != null)
            {
                this._runspacePoolDriver.Close();
            }
            if (this._sessionConfigProvider != null)
            {
                this._sessionConfigProvider.Dispose();
                this._sessionConfigProvider = null;
            }
        }

        private bool RunServerNegotiationAlgorithm(RemoteSessionCapability clientCapability, bool onConnect)
        {
            Version protocolVersion = clientCapability.ProtocolVersion;
            Version version2 = this._context.ServerCapability.ProtocolVersion;
            if (onConnect)
            {
                if (protocolVersion != version2)
                {
                    PSRemotingDataStructureException exception = new PSRemotingDataStructureException(RemotingErrorIdStrings.ServerConnectFailedOnNegotiation, new object[] { "protocolversion", protocolVersion, PSVersionInfo.BuildVersion, RemotingConstants.ProtocolVersion });
                    throw exception;
                }
            }
            else
            {
                if ((protocolVersion == RemotingConstants.ProtocolVersionWin7RTM) && (version2 == RemotingConstants.ProtocolVersionCurrent))
                {
                    version2 = RemotingConstants.ProtocolVersionWin7RTM;
                    this._context.ServerCapability.ProtocolVersion = version2;
                }
                if ((protocolVersion == RemotingConstants.ProtocolVersionWin7RC) && ((version2 == RemotingConstants.ProtocolVersionWin7RTM) || (version2 == RemotingConstants.ProtocolVersionCurrent)))
                {
                    version2 = RemotingConstants.ProtocolVersionWin7RC;
                    this._context.ServerCapability.ProtocolVersion = version2;
                }
                if ((protocolVersion.Major != version2.Major) || (protocolVersion.Minor < version2.Minor))
                {
                    PSRemotingDataStructureException exception2 = new PSRemotingDataStructureException(RemotingErrorIdStrings.ServerNegotiationFailed, new object[] { "protocolversion", protocolVersion, PSVersionInfo.BuildVersion, RemotingConstants.ProtocolVersion });
                    throw exception2;
                }
            }
            Version pSVersion = clientCapability.PSVersion;
            Version version4 = this._context.ServerCapability.PSVersion;
            if ((pSVersion.Major != version4.Major) || (pSVersion.Minor < version4.Minor))
            {
                PSRemotingDataStructureException exception3 = new PSRemotingDataStructureException(RemotingErrorIdStrings.ServerNegotiationFailed, new object[] { "PSVersion", pSVersion, PSVersionInfo.BuildVersion, RemotingConstants.ProtocolVersion });
                throw exception3;
            }
            Version serializationVersion = clientCapability.SerializationVersion;
            Version version6 = this._context.ServerCapability.SerializationVersion;
            if ((serializationVersion.Major == version6.Major) && (serializationVersion.Minor >= version6.Minor))
            {
                return true;
            }
            PSRemotingDataStructureException exception4 = new PSRemotingDataStructureException(RemotingErrorIdStrings.ServerNegotiationFailed, new object[] { "SerializationVersion", serializationVersion, PSVersionInfo.BuildVersion, RemotingConstants.ProtocolVersion });
            throw exception4;
        }

        internal void SendEncryptedSessionKey()
        {
            string encryptedSessionKey = null;
            bool flag = this._cryptoHelper.ExportEncryptedSessionKey(out encryptedSessionKey);
            RemoteSessionStateMachineEventArgs fsmEventArg = null;
            if (!flag)
            {
                fsmEventArg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.KeySendFailed);
                this.SessionDataStructureHandler.StateMachine.RaiseEvent(fsmEventArg);
            }
            this.SessionDataStructureHandler.SendEncryptedSessionKey(encryptedSessionKey);
            this.CompleteKeyExchange();
            fsmEventArg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.KeySent);
            this.SessionDataStructureHandler.StateMachine.RaiseEvent(fsmEventArg);
        }

        internal override void StartKeyExchange()
        {
            if (this.SessionDataStructureHandler.StateMachine.State == RemoteSessionState.Established)
            {
                this.SessionDataStructureHandler.SendRequestForPublicKey();
                RemoteSessionStateMachineEventArgs fsmEventArg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.KeyRequested);
                this.SessionDataStructureHandler.StateMachine.RaiseEvent(fsmEventArg);
            }
        }

        internal ServerRemoteSessionContext Context
        {
            get
            {
                return this._context;
            }
        }

		internal PSIdentity Identity {
			get { return _senderInfo.UserInfo.Identity; }
		}

        internal override RemotingDestination MySelf
        {
            get
            {
                return (RemotingDestination.InvalidDestination | RemotingDestination.Server);
            }
        }

        internal ServerRemoteSessionDataStructureHandler SessionDataStructureHandler
        {
            get
            {
                return this._sessionDSHandler;
            }
        }
    }
}

