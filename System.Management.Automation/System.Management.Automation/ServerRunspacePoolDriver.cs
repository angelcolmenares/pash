namespace System.Management.Automation
{
    using Microsoft.PowerShell;
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Remoting.Server;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Security;
    using System.Threading;

    internal class ServerRunspacePoolDriver
    {
        private bool? _initialSessionStateIncludesGetCommandWithListImportedSwitch;
        private object _initialSessionStateIncludesGetCommandWithListImportedSwitchLock = new object();
        private PSPrimitiveDictionary applicationPrivateData;
        private Dictionary<Guid, ServerPowerShellDriver> associatedShells = new Dictionary<Guid, ServerPowerShellDriver>();
        private Guid clientRunspacePoolId;
        internal EventHandler<EventArgs> Closed;
        private ConfigurationDataFromXML configData;
        private Hashtable configHash;
        private ServerRunspacePoolDataStructureHandler dsHandler;
        private ServerSteppablePipelineSubscriber eventSubscriber = new ServerSteppablePipelineSubscriber();
        private PSDataCollection<object> inputCollection;
        private bool isClosed;
        private System.Management.Automation.Runspaces.RunspacePool localRunspacePool;
        private System.Management.Automation.Remoting.ServerRemoteHost remoteHost;
        private Runspace rsToUseForSteppablePipeline;
        private RemoteSessionCapability serverCapability;

        internal ServerRunspacePoolDriver(Guid clientRunspacePoolId, int minRunspaces, int maxRunspaces, PSThreadOptions threadOptions, ApartmentState apartmentState, HostInfo hostInfo, InitialSessionState initialSessionState, PSPrimitiveDictionary applicationPrivateData, ConfigurationDataFromXML configData, AbstractServerSessionTransportManager transportManager, bool isAdministrator, RemoteSessionCapability serverCapability, Hashtable configHash)
        {
            this.serverCapability = serverCapability;
            System.Management.Automation.Remoting.ServerRemoteHost host = new System.Management.Automation.Remoting.ServerRemoteHost(clientRunspacePoolId, Guid.Empty, hostInfo, transportManager);
            this.remoteHost = host;
            this.configData = configData;
            this.configHash = configHash;
            this.applicationPrivateData = applicationPrivateData;
            this.localRunspacePool = RunspaceFactory.CreateRunspacePool(minRunspaces, maxRunspaces, initialSessionState, host);
            PSThreadOptions options = configData.ShellThreadOptions.HasValue ? configData.ShellThreadOptions.Value : PSThreadOptions.UseCurrentThread;
            if ((threadOptions == PSThreadOptions.Default) || (threadOptions == options))
            {
                this.localRunspacePool.ThreadOptions = options;
            }
            else
            {
                if (!isAdministrator)
                {
                    throw new InvalidOperationException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.MustBeAdminToOverrideThreadOptions, new object[0]));
                }
                this.localRunspacePool.ThreadOptions = threadOptions;
            }
            ApartmentState state = configData.ShellThreadApartmentState.HasValue ? configData.ShellThreadApartmentState.Value : ApartmentState.Unknown;
            if ((apartmentState == ApartmentState.Unknown) || (apartmentState == state))
            {
                this.localRunspacePool.ApartmentState = state;
            }
            else
            {
                this.localRunspacePool.ApartmentState = apartmentState;
            }
            this.clientRunspacePoolId = clientRunspacePoolId;
            this.dsHandler = new ServerRunspacePoolDataStructureHandler(this, transportManager);
            this.localRunspacePool.StateChanged += new EventHandler<RunspacePoolStateChangedEventArgs>(this.HandleRunspacePoolStateChanged);
            this.localRunspacePool.ForwardEvent += new EventHandler<PSEventArgs>(this.HandleRunspacePoolForwardEvent);
            this.localRunspacePool.RunspaceCreated += new EventHandler<RunspaceCreatedEventArgs>(this.HandleRunspaceCreated);
            this.localRunspacePool.RunspaceCreated += new EventHandler<RunspaceCreatedEventArgs>(this.HandleRunspaceCreatedForTypeTable);
            this.dsHandler.CreateAndInvokePowerShell += new EventHandler<RemoteDataEventArgs<RemoteDataObject<PSObject>>>(this.HandleCreateAndInvokePowerShell);
            this.dsHandler.GetCommandMetadata += new EventHandler<RemoteDataEventArgs<RemoteDataObject<PSObject>>>(this.HandleGetCommandMetadata);
            this.dsHandler.HostResponseReceived += new EventHandler<RemoteDataEventArgs<RemoteHostResponse>>(this.HandleHostResponseReceived);
            this.dsHandler.SetMaxRunspacesReceived += new EventHandler<RemoteDataEventArgs<PSObject>>(this.HandleSetMaxRunspacesReceived);
            this.dsHandler.SetMinRunspacesReceived += new EventHandler<RemoteDataEventArgs<PSObject>>(this.HandleSetMinRunspacesReceived);
            this.dsHandler.GetAvailableRunspacesReceived += new EventHandler<RemoteDataEventArgs<PSObject>>(this.HandleGetAvailalbeRunspacesReceived);
        }

        internal void Close()
        {
            if (!this.isClosed)
            {
                this.isClosed = true;
                this.localRunspacePool.Close();
                this.localRunspacePool.StateChanged -= new EventHandler<RunspacePoolStateChangedEventArgs>(this.HandleRunspacePoolStateChanged);
                this.localRunspacePool.ForwardEvent -= new EventHandler<PSEventArgs>(this.HandleRunspacePoolForwardEvent);
                this.localRunspacePool.Dispose();
                this.localRunspacePool = null;
                if (this.rsToUseForSteppablePipeline != null)
                {
                    this.rsToUseForSteppablePipeline.Close();
                    this.rsToUseForSteppablePipeline.Dispose();
                    this.rsToUseForSteppablePipeline = null;
                }
                this.Closed.SafeInvoke<EventArgs>(this, EventArgs.Empty);
            }
        }

        private IEnumerable<WildcardPattern> CreateKeyPatternList(string pattern)
        {
            WildcardOptions options = WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase;
            return SessionStateUtilities.CreateWildcardsFromStrings(new string[] { pattern }, options);
        }

        private bool DoesInitialSessionStateIncludeGetCommandWithListImportedSwitch()
        {
            if (!this._initialSessionStateIncludesGetCommandWithListImportedSwitch.HasValue)
            {
                lock (this._initialSessionStateIncludesGetCommandWithListImportedSwitchLock)
                {
                    if (!this._initialSessionStateIncludesGetCommandWithListImportedSwitch.HasValue)
                    {
                        bool flag = false;
                        InitialSessionState initialSessionState = this.RunspacePool.InitialSessionState;
                        if (initialSessionState != null)
                        {
                            IEnumerable<SessionStateCommandEntry> source = from entry in initialSessionState.Commands["Get-Command"]
                                where entry.Visibility == SessionStateEntryVisibility.Public
                                select entry;
                            SessionStateFunctionEntry e = source.OfType<SessionStateFunctionEntry>().FirstOrDefault<SessionStateFunctionEntry>();
                            if (e != null)
                            {
                                if (e.ScriptBlock.ParameterMetadata.BindableParameters.ContainsKey("ListImported"))
                                {
                                    flag = true;
                                }
                            }
                            else
                            {
                                SessionStateCmdletEntry entry2 = source.OfType<SessionStateCmdletEntry>().FirstOrDefault<SessionStateCmdletEntry>();
                                if ((entry2 != null) && entry2.ImplementingType.Equals(typeof(GetCommandCommand)))
                                {
                                    flag = true;
                                }
                            }
                        }
                        this._initialSessionStateIncludesGetCommandWithListImportedSwitch = new bool?(flag);
                    }
                }
            }
            return this._initialSessionStateIncludesGetCommandWithListImportedSwitch.Value;
        }

        private void HandleCreateAndInvokePowerShell (object sender, RemoteDataEventArgs<RemoteDataObject<PSObject>> eventArgs)
		{
			RemoteDataObject<PSObject> data = eventArgs.Data;
			HostInfo hostInfo = RemotingDecoder.GetHostInfo (data.Data);
			ApartmentState apartmentState = RemotingDecoder.GetApartmentState (data.Data);
			RemoteStreamOptions remoteStreamOptions = RemotingDecoder.GetRemoteStreamOptions (data.Data);

			PowerShell powerShell = RemotingDecoder.GetPowerShell (data.Data);
            bool noInput = RemotingDecoder.GetNoInput(data.Data);
            bool addToHistory = RemotingDecoder.GetAddToHistory(data.Data);
            bool isNested = false;
            if (this.serverCapability.ProtocolVersion >= RemotingConstants.ProtocolVersionWin8RTM)
            {
                isNested = RemotingDecoder.GetIsNested(data.Data);
            }
            if (isNested)
            {
                if (this.dsHandler.GetAssociatedPowerShellDataStructureHandler(powerShell.InstanceId) != null)
                {
                    throw new InvalidOperationException("NestedPipeline is not supported in this release.");
                }
                powerShell.SetIsNested(false);
                if ((this.localRunspacePool.GetMaxRunspaces() == 1) && (this.dsHandler.GetPowerShellDataStructureHandler() != null))
                {
                    new ServerSteppablePipelineDriver(powerShell, noInput, data.PowerShellId, data.RunspacePoolId, this, apartmentState, hostInfo, remoteStreamOptions, addToHistory, this.rsToUseForSteppablePipeline, this.eventSubscriber, this.inputCollection).Start();
                    return;
                }
            }
            ServerPowerShellDriver driver2 = new ServerPowerShellDriver(powerShell, null, noInput, data.PowerShellId, data.RunspacePoolId, this, apartmentState, hostInfo, remoteStreamOptions, addToHistory, null);
            this.inputCollection = driver2.InputCollection;
            driver2.Start();
        }

        private void HandleGetAvailalbeRunspacesReceived(object sender, RemoteDataEventArgs<PSObject> eventArgs)
        {
            long callId = (long) ((PSNoteProperty) eventArgs.Data.Properties["ci"]).Value;
            int availableRunspaces = this.localRunspacePool.GetAvailableRunspaces();
            this.dsHandler.SendResponseToClient(callId, availableRunspaces);
        }

        private void HandleGetCommandMetadata(object sender, RemoteDataEventArgs<RemoteDataObject<PSObject>> eventArgs)
        {
            RemoteDataObject<PSObject> data = eventArgs.Data;
            PowerShell commandDiscoveryPipeline = RemotingDecoder.GetCommandDiscoveryPipeline(data.Data);
            if (this.DoesInitialSessionStateIncludeGetCommandWithListImportedSwitch())
            {
                commandDiscoveryPipeline.AddParameter("ListImported", true);
            }
            commandDiscoveryPipeline.AddParameter("ErrorAction", "SilentlyContinue").AddCommand("Measure-Object").AddCommand("Select-Object").AddParameter("Property", "Count");
            PowerShell extraPowerShell = RemotingDecoder.GetCommandDiscoveryPipeline(data.Data);
            if (this.DoesInitialSessionStateIncludeGetCommandWithListImportedSwitch())
            {
                extraPowerShell.AddParameter("ListImported", true);
            }
            extraPowerShell.AddCommand("Select-Object").AddParameter("Property", new string[] { "Name", "Namespace", "HelpUri", "CommandType", "ResolvedCommandName", "OutputType", "Parameters" });
            HostInfo hostInfo = new HostInfo(null) {
                UseRunspaceHost = true
            };
            new ServerPowerShellDriver(commandDiscoveryPipeline, extraPowerShell, true, data.PowerShellId, data.RunspacePoolId, this, ApartmentState.Unknown, hostInfo, 0, false, null).Start();
        }

        private void HandleHostResponseReceived(object sender, RemoteDataEventArgs<RemoteHostResponse> eventArgs)
        {
            this.remoteHost.ServerMethodExecutor.HandleRemoteHostResponseFromClient(eventArgs.Data);
        }

        private void HandleRunspaceCreated(object sender, RunspaceCreatedEventArgs args)
        {
            bool flag = false;
            if (SystemPolicy.GetSystemLockdownPolicy() == SystemEnforcementMode.Enforce)
            {
                args.Runspace.ExecutionContext.LanguageMode = PSLanguageMode.ConstrainedLanguage;
                flag = true;
            }
            try
            {
                string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                args.Runspace.ExecutionContext.EngineSessionState.SetLocation(folderPath);
            }
            catch (ArgumentException)
            {
            }
            catch (ProviderNotFoundException)
            {
            }
            catch (DriveNotFoundException)
            {
            }
            catch (ProviderInvocationException)
            {
            }
            if (this.configHash != null)
            {
                if (this.configHash.ContainsKey(ConfigFileContants.EnvironmentVariables))
                {
                    Hashtable hashtable = this.configHash[ConfigFileContants.EnvironmentVariables] as Hashtable;
                    if (hashtable != null)
                    {
                        foreach (DictionaryEntry entry in hashtable)
                        {
                            string introduced76 = entry.Key.ToString();
                            this.InvokeScript(new Command(StringUtil.Format("$env:{0} = \"{1}\"", introduced76, entry.Value.ToString()), true, false), args);
                        }
                    }
                }
                if (this.configHash.ContainsKey(ConfigFileContants.VariableDefinitions))
                {
                    Hashtable[] hashtableArray = DISCPowerShellConfiguration.TryGetHashtableArray(this.configHash[ConfigFileContants.VariableDefinitions]);
                    if (hashtableArray != null)
                    {
                        foreach (Hashtable hashtable2 in hashtableArray)
                        {
                            if (hashtable2.ContainsKey(ConfigFileContants.VariableValueToken))
                            {
                                string str2 = DISCPowerShellConfiguration.TryGetValue(hashtable2, ConfigFileContants.VariableNameToken);
                                ScriptBlock block = hashtable2[ConfigFileContants.VariableValueToken] as ScriptBlock;
                                if (!string.IsNullOrEmpty(str2) && (block != null))
                                {
                                    block.SessionStateInternal = args.Runspace.ExecutionContext.EngineSessionState;
                                    PowerShell powershell = PowerShell.Create();
                                    powershell.AddCommand(new Command("Invoke-Command")).AddParameter("ScriptBlock", block).AddParameter("NoNewScope");
                                    powershell.AddCommand(new Command("Set-Variable")).AddParameter("Name", str2);
                                    this.InvokePowerShell(powershell, args);
                                }
                            }
                        }
                    }
                }
                if (this.configHash.ContainsKey(ConfigFileContants.ScriptsToProcess))
                {
                    string[] strArray = DISCPowerShellConfiguration.TryGetStringArray(this.configHash[ConfigFileContants.ScriptsToProcess]);
                    if (strArray != null)
                    {
                        foreach (string str3 in strArray)
                        {
                            if (!string.IsNullOrEmpty(str3))
                            {
                                this.InvokeScript(new Command(str3, true, false), args);
                            }
                        }
                    }
                }
                bool flag2 = false;
                if (this.configHash.ContainsKey(ConfigFileContants.VisibleAliases))
                {
                    string[] strArray2 = DISCPowerShellConfiguration.TryGetStringArray(this.configHash[ConfigFileContants.VisibleAliases]);
                    if (strArray2 != null)
                    {
                        flag2 = true;
                        foreach (KeyValuePair<string, AliasInfo> pair in args.Runspace.ExecutionContext.EngineSessionState.GetAliasTable())
                        {
                            bool flag3 = false;
                            foreach (string str4 in strArray2)
                            {
                                if (!string.IsNullOrEmpty(str4))
                                {
                                    IEnumerable<WildcardPattern> patternList = this.CreateKeyPatternList(str4);
                                    if (this.MatchKeyPattern(patternList, pair.Key))
                                    {
                                        pair.Value.Visibility = SessionStateEntryVisibility.Public;
                                        flag3 = true;
                                    }
                                }
                            }
                            if (!flag3)
                            {
                                pair.Value.Visibility = SessionStateEntryVisibility.Private;
                            }
                        }
                    }
                }
                if (this.configHash.ContainsKey(ConfigFileContants.VisibleCmdlets))
                {
                    string[] strArray3 = DISCPowerShellConfiguration.TryGetStringArray(this.configHash[ConfigFileContants.VisibleCmdlets]);
                    if (strArray3 != null)
                    {
                        flag2 = true;
                        foreach (KeyValuePair<string, List<CmdletInfo>> pair2 in args.Runspace.ExecutionContext.EngineSessionState.GetCmdletTable())
                        {
                            bool flag4 = false;
                            foreach (string str5 in strArray3)
                            {
                                if (!string.IsNullOrEmpty(str5))
                                {
                                    IEnumerable<WildcardPattern> enumerable2 = this.CreateKeyPatternList(str5);
                                    if (this.MatchKeyPattern(enumerable2, pair2.Key))
                                    {
                                        foreach (CmdletInfo info in pair2.Value)
                                        {
                                            info.Visibility = SessionStateEntryVisibility.Public;
                                            flag4 = true;
                                        }
                                    }
                                }
                            }
                            if (!flag4)
                            {
                                foreach (CmdletInfo info2 in pair2.Value)
                                {
                                    info2.Visibility = SessionStateEntryVisibility.Private;
                                }
                            }
                        }
                    }
                }
                List<string> list = new List<string>();
                bool flag5 = false;
                if (this.configHash.ContainsKey(ConfigFileContants.VisibleFunctions))
                {
                    string[] strArray4 = DISCPowerShellConfiguration.TryGetStringArray(this.configHash[ConfigFileContants.VisibleFunctions]);
                    if (strArray4 != null)
                    {
                        flag2 = true;
                        flag5 = true;
                        list.AddRange(strArray4);
                    }
                }
                if (!flag5 && this.configHash.ContainsKey(ConfigFileContants.FunctionDefinitions))
                {
                    Hashtable[] hashtableArray2 = DISCPowerShellConfiguration.TryGetHashtableArray(this.configHash[ConfigFileContants.FunctionDefinitions]);
                    if (hashtableArray2 != null)
                    {
                        foreach (Hashtable hashtable3 in hashtableArray2)
                        {
                            string str6 = DISCPowerShellConfiguration.TryGetValue(hashtable3, ConfigFileContants.FunctionNameToken);
                            if (!string.IsNullOrEmpty(str6))
                            {
                                list.Add(str6);
                            }
                        }
                    }
                }
                string str7 = DISCPowerShellConfiguration.TryGetValue(this.configHash, ConfigFileContants.SessionType);
                if (!string.IsNullOrEmpty(str7))
                {
                    SessionType type = (SessionType) Enum.Parse(typeof(SessionType), str7, true);
                    if (type == SessionType.RestrictedRemoteServer)
                    {
                        list.Add("Get-Command");
                        list.Add("Get-FormatData");
                        list.Add("Select-Object");
                        list.Add("Get-Help");
                        list.Add("Measure-Object");
                        list.Add("Out-Default");
                        list.Add("Exit-PSSession");
                    }
                }
                if (list.Count > 0)
                {
                    foreach (DictionaryEntry entry2 in args.Runspace.ExecutionContext.EngineSessionState.GetFunctionTable())
                    {
                        bool flag6 = false;
                        string key = entry2.Key.ToString();
                        FunctionInfo info3 = entry2.Value as FunctionInfo;
                        if (info3 != null)
                        {
                            foreach (string str9 in list)
                            {
                                if (!string.IsNullOrEmpty(str9))
                                {
                                    IEnumerable<WildcardPattern> enumerable3 = this.CreateKeyPatternList(str9);
                                    if (this.MatchKeyPattern(enumerable3, key))
                                    {
                                        info3.Visibility = SessionStateEntryVisibility.Public;
                                        flag6 = true;
                                    }
                                }
                            }
                            if (!flag6 && flag5)
                            {
                                info3.Visibility = SessionStateEntryVisibility.Private;
                            }
                        }
                    }
                }
                if (this.configHash.ContainsKey(ConfigFileContants.VisibleProviders))
                {
                    string[] strArray5 = DISCPowerShellConfiguration.TryGetStringArray(this.configHash[ConfigFileContants.VisibleProviders]);
                    if (strArray5 != null)
                    {
                        flag2 = true;
                        IDictionary<string, List<ProviderInfo>> providers = args.Runspace.ExecutionContext.EngineSessionState.Providers;
                        Collection<string> collection = new Collection<string>();
                        foreach (KeyValuePair<string, List<ProviderInfo>> pair3 in providers)
                        {
                            bool flag7 = false;
                            foreach (string str10 in strArray5)
                            {
                                if (!string.IsNullOrEmpty(str10))
                                {
                                    IEnumerable<WildcardPattern> enumerable4 = this.CreateKeyPatternList(str10);
                                    if (this.MatchKeyPattern(enumerable4, pair3.Key))
                                    {
                                        flag7 = true;
                                    }
                                }
                            }
                            if (!flag7)
                            {
                                collection.Add(pair3.Key);
                            }
                        }
                        foreach (string str11 in collection)
                        {
                            args.Runspace.ExecutionContext.EngineSessionState.Providers.Remove(str11);
                        }
                    }
                }
                if (flag2)
                {
                    CmdletInfo cmdlet = args.Runspace.ExecutionContext.SessionState.InvokeCommand.GetCmdlet(@"Microsoft.PowerShell.Core\Import-Module");
                    IDictionary<string, AliasInfo> aliasTable = args.Runspace.ExecutionContext.EngineSessionState.GetAliasTable();
                    PSModuleAutoLoadingPreference preference = CommandDiscovery.GetCommandDiscoveryPreference(args.Runspace.ExecutionContext, SpecialVariables.PSModuleAutoLoadingPreferenceVarPath, "PSModuleAutoLoadingPreference");
                    bool flag8 = (cmdlet != null) && (cmdlet.Visibility != SessionStateEntryVisibility.Private);
                    bool flag9 = ((aliasTable != null) && aliasTable.ContainsKey("ipmo")) && (aliasTable["ipmo"].Visibility != SessionStateEntryVisibility.Private);
                    if ((flag8 || flag9) && (preference == PSModuleAutoLoadingPreference.None))
                    {
                        throw new PSInvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.DISCVisibilityAndAutoLoadingCannotBeBothSpecified, new object[] { "Import-Module", "ipmo", ConfigFileContants.VisibleCmdlets, ConfigFileContants.VisibleAliases, ConfigFileContants.VisibleFunctions, ConfigFileContants.VisibleProviders }));
                    }
                }
                if (this.configHash.ContainsKey(ConfigFileContants.LanguageMode))
                {
                    PSLanguageMode mode = (PSLanguageMode) Enum.Parse(typeof(PSLanguageMode), this.configHash[ConfigFileContants.LanguageMode].ToString(), true);
                    if (flag && (mode != PSLanguageMode.ConstrainedLanguage))
                    {
                        throw new PSInvalidOperationException(RemotingErrorIdStrings.CannotCreateRunspaceInconsistentState);
                    }
                    args.Runspace.ExecutionContext.LanguageMode = mode;
                }
                if (this.configHash.ContainsKey(ConfigFileContants.ExecutionPolicy))
                {
                    ExecutionPolicy policy = (ExecutionPolicy) Enum.Parse(typeof(ExecutionPolicy), this.configHash[ConfigFileContants.ExecutionPolicy].ToString(), true);
                    string shellID = args.Runspace.ExecutionContext.ShellID;
                    SecuritySupport.SetExecutionPolicy(ExecutionPolicyScope.Process, policy, shellID);
                }
            }
            Command cmdToRun = null;
            if (!string.IsNullOrEmpty(this.configData.StartupScript))
            {
                cmdToRun = new Command(this.configData.StartupScript, false, false);
            }
            else if (!string.IsNullOrEmpty(this.configData.InitializationScriptForOutOfProcessRunspace))
            {
                cmdToRun = new Command(this.configData.InitializationScriptForOutOfProcessRunspace, true, false);
            }
            if (cmdToRun != null)
            {
                this.InvokeScript(cmdToRun, args);
                if (this.localRunspacePool.RunspacePoolStateInfo.State == RunspacePoolState.Opening)
                {
                    object valueToConvert = args.Runspace.SessionStateProxy.PSVariable.GetValue("global:PSApplicationPrivateData");
                    if (valueToConvert != null)
                    {
                        this.applicationPrivateData = (PSPrimitiveDictionary) LanguagePrimitives.ConvertTo(valueToConvert, typeof(PSPrimitiveDictionary), true, CultureInfo.InvariantCulture, null);
                    }
                }
            }
        }

        private void HandleRunspaceCreatedForTypeTable(object sender, RunspaceCreatedEventArgs args)
        {
            this.dsHandler.TypeTable = args.Runspace.ExecutionContext.TypeTable;
            this.localRunspacePool.RunspaceCreated -= new EventHandler<RunspaceCreatedEventArgs>(this.HandleRunspaceCreatedForTypeTable);
            this.rsToUseForSteppablePipeline = args.Runspace;
        }

        private void HandleRunspacePoolForwardEvent(object sender, PSEventArgs e)
        {
            if (e.ForwardEvent)
            {
                this.dsHandler.SendPSEventArgsToClient(e);
            }
        }

        private void HandleRunspacePoolStateChanged(object sender, RunspacePoolStateChangedEventArgs eventArgs)
        {
            RunspacePoolState state = eventArgs.RunspacePoolStateInfo.State;
            Exception reason = eventArgs.RunspacePoolStateInfo.Reason;
            switch (state)
            {
                case RunspacePoolState.Opened:
                    this.SendApplicationPrivateDataToClient();
                    this.dsHandler.SendStateInfoToClient(new RunspacePoolStateInfo(state, reason));
                    return;

                case RunspacePoolState.Closed:
                case RunspacePoolState.Closing:
                case RunspacePoolState.Broken:
                    this.dsHandler.SendStateInfoToClient(new RunspacePoolStateInfo(state, reason));
                    return;
            }
        }

        private void HandleSetMaxRunspacesReceived(object sender, RemoteDataEventArgs<PSObject> eventArgs)
        {
            PSObject data = eventArgs.Data;
            int maxRunspaces = (int) ((PSNoteProperty) data.Properties["MaxRunspaces"]).Value;
            long callId = (long) ((PSNoteProperty) data.Properties["ci"]).Value;
            bool response = this.localRunspacePool.SetMaxRunspaces(maxRunspaces);
            this.dsHandler.SendResponseToClient(callId, response);
        }

        private void HandleSetMinRunspacesReceived(object sender, RemoteDataEventArgs<PSObject> eventArgs)
        {
            PSObject data = eventArgs.Data;
            int minRunspaces = (int) ((PSNoteProperty) data.Properties["MinRunspaces"]).Value;
            long callId = (long) ((PSNoteProperty) data.Properties["ci"]).Value;
            bool response = this.localRunspacePool.SetMinRunspaces(minRunspaces);
            this.dsHandler.SendResponseToClient(callId, response);
        }

        private PSDataCollection<PSObject> InvokePowerShell(PowerShell powershell, RunspaceCreatedEventArgs args)
        {
            string str;
            HostInfo hostInfo = this.remoteHost.HostInfo;
            IAsyncResult asyncResult = new ServerPowerShellDriver(powershell, null, true, Guid.Empty, this.InstanceId, this, args.Runspace.ApartmentState, hostInfo, RemoteStreamOptions.AddInvocationInfo, false, args.Runspace).Start();
            PSDataCollection<PSObject> datas = powershell.EndInvoke(asyncResult);
            ArrayList dollarErrorVariable = (ArrayList) powershell.Runspace.GetExecutionContext.DollarErrorVariable;
            if (dollarErrorVariable.Count <= 0)
            {
                return datas;
            }
            ErrorRecord record = dollarErrorVariable[0] as ErrorRecord;
            if (record != null)
            {
                str = record.ToString();
            }
            else
            {
                Exception exception = dollarErrorVariable[0] as Exception;
                if (exception != null)
                {
                    str = (exception.Message != null) ? exception.Message : string.Empty;
                }
                else
                {
                    str = string.Empty;
                }
            }
            throw PSTraceSource.NewInvalidOperationException("RemotingErrorIdStrings", PSRemotingErrorId.StartupScriptThrewTerminatingError.ToString(), new object[] { str });
        }

        private PSDataCollection<PSObject> InvokeScript(Command cmdToRun, RunspaceCreatedEventArgs args)
        {
            cmdToRun.MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);
            PowerShell powershell = PowerShell.Create();
            powershell.AddCommand(cmdToRun).AddCommand("out-default");
            return this.InvokePowerShell(powershell, args);
        }

        private bool MatchKeyPattern(IEnumerable<WildcardPattern> patternList, string key)
        {
            return SessionStateUtilities.MatchesAnyWildcardPattern(key, patternList, true);
        }

        internal void SendApplicationPrivateDataToClient()
        {
            this.dsHandler.SendApplicationPrivateDataToClient(this.applicationPrivateData, this.serverCapability);
        }

        internal void Start()
        {
            this.localRunspacePool.Open();
        }

        internal ServerRunspacePoolDataStructureHandler DataStructureHandler
        {
            get
            {
                return this.dsHandler;
            }
        }

        internal Guid InstanceId
        {
            get
            {
                return this.clientRunspacePoolId;
            }
        }

        internal System.Management.Automation.Runspaces.RunspacePool RunspacePool
        {
            get
            {
                return this.localRunspacePool;
            }
        }

        internal System.Management.Automation.Remoting.ServerRemoteHost ServerRemoteHost
        {
            get
            {
                return this.remoteHost;
            }
        }
    }
}

