namespace System.Management.Automation
{
    using Microsoft.PowerShell.Commands;
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Sqm;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    internal class CommandDiscovery
    {
        private bool _cmdletCacheInitialized;
        private ExecutionContext _context;
        private HashSet<string> activeCommandNotFound = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private HashSet<string> activeModuleSearch = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private HashSet<string> activePostCommand = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private HashSet<string> activePreLookup = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private LookupPathCollection cachedLookupPaths;
        private Collection<string> cachedPath;
        private Collection<string> cachedPathExt;
        private static Collection<string> cachedPathExtensions;
        private Dictionary<string, ScriptInfo> cachedScriptInfo;
        [TraceSource("CommandDiscovery", "Traces the discovery of cmdlets, scripts, functions, applications, etc.")]
        internal static PSTraceSource discoveryTracer = PSTraceSource.GetTracer("CommandDiscovery", "Traces the discovery of cmdlets, scripts, functions, applications, etc.", false);
        private string pathCacheKey;
        private string pathExtCacheKey;
        private static string pathExtensionsCacheKey;

        internal CommandDiscovery(ExecutionContext context)
        {
            if (context == null)
            {
                throw PSTraceSource.NewArgumentNullException("context");
            }
            this._context = context;
            discoveryTracer.ShowHeaders = false;
            this.cachedScriptInfo = new Dictionary<string, ScriptInfo>(StringComparer.OrdinalIgnoreCase);
            this.LoadScriptInfo();
        }

        internal CmdletInfo AddCmdletInfoToCache(string name, CmdletInfo newCmdletInfo, bool isGlobal)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            if (newCmdletInfo == null)
            {
                throw PSTraceSource.NewArgumentNullException("cmdlet");
            }
            if (isGlobal)
            {
                return this._context.EngineSessionState.ModuleScope.AddCmdletToCache(newCmdletInfo.Name, newCmdletInfo, CommandOrigin.Internal, this._context);
            }
            return this._context.EngineSessionState.CurrentScope.AddCmdletToCache(newCmdletInfo.Name, newCmdletInfo, CommandOrigin.Internal, this._context);
        }

        private void AddCmdletToCache(CmdletConfigurationEntry entry)
        {
            if (!this.IsSpecialCmdlet(entry.ImplementingType))
            {
                CmdletInfo newCmdletInfo = this.NewCmdletInfo(entry, SessionStateEntryVisibility.Public);
                this.AddCmdletInfoToCache(newCmdletInfo.Name, newCmdletInfo, true);
            }
        }

        internal void AddSessionStateCmdletEntryToCache(SessionStateCmdletEntry entry)
        {
            this.AddSessionStateCmdletEntryToCache(entry, false);
        }

        internal void AddSessionStateCmdletEntryToCache(SessionStateCmdletEntry entry, bool local)
        {
            if (!this.IsSpecialCmdlet(entry.ImplementingType))
            {
                CmdletInfo newCmdletInfo = this.NewCmdletInfo(entry);
                this.AddCmdletInfoToCache(newCmdletInfo.Name, newCmdletInfo, !local);
            }
        }

        private static bool AreInstalledRequiresVersionsCompatible(Version requires, Version installed)
        {
            return ((requires.Major == installed.Major) && (requires.Minor <= installed.Minor));
        }

        private static string BuildPSSnapInDisplayName(PSSnapInSpecification PSSnapin)
        {
            if (PSSnapin.Version != null)
            {
                return StringUtil.Format(DiscoveryExceptions.PSSnapInNameVersion, PSSnapin.Name, PSSnapin.Version);
            }
            return PSSnapin.Name;
        }

        private static CommandProcessorBase CreateCommandProcessorForScript(ExternalScriptInfo scriptInfo, ExecutionContext context, bool useNewScope, SessionStateInternal sessionState)
        {
            sessionState = sessionState ?? (scriptInfo.ScriptBlock.SessionStateInternal ?? context.EngineSessionState);
            CommandProcessorBase base2 = GetScriptAsCmdletProcessor(scriptInfo, context, useNewScope, true, sessionState);
            if (base2 != null)
            {
                return base2;
            }
            return new DlrScriptCommandProcessor(scriptInfo, context, useNewScope, sessionState);
        }

        internal static CommandProcessorBase CreateCommandProcessorForScript(FunctionInfo functionInfo, ExecutionContext context, bool useNewScope, SessionStateInternal sessionState)
        {
            sessionState = sessionState ?? (functionInfo.ScriptBlock.SessionStateInternal ?? context.EngineSessionState);
            CommandProcessorBase base2 = GetScriptAsCmdletProcessor(functionInfo, context, useNewScope, false, sessionState);
            if (base2 != null)
            {
                return base2;
            }
            return new DlrScriptCommandProcessor(functionInfo, context, useNewScope, sessionState);
        }

        internal static CommandProcessorBase CreateCommandProcessorForScript(ScriptBlock scriptblock, ExecutionContext context, bool useNewScope, SessionStateInternal sessionState)
        {
            sessionState = sessionState ?? (scriptblock.SessionStateInternal ?? context.EngineSessionState);
            if (scriptblock.UsesCmdletBinding)
            {
                FunctionInfo scriptCommandInfo = new FunctionInfo("", scriptblock, context);
                return GetScriptAsCmdletProcessor(scriptCommandInfo, context, useNewScope, false, sessionState);
            }
            return new DlrScriptCommandProcessor(scriptblock, context, useNewScope, CommandOrigin.Internal, sessionState);
        }

        private static CommandProcessorBase CreateCommandProcessorForScript(ScriptInfo scriptInfo, ExecutionContext context, bool useNewScope, SessionStateInternal sessionState)
        {
            sessionState = sessionState ?? (scriptInfo.ScriptBlock.SessionStateInternal ?? context.EngineSessionState);
            CommandProcessorBase base2 = GetScriptAsCmdletProcessor(scriptInfo, context, useNewScope, true, sessionState);
            if (base2 != null)
            {
                return base2;
            }
            return new DlrScriptCommandProcessor(scriptInfo, context, useNewScope, sessionState);
        }

        internal CommandProcessorBase CreateScriptProcessorForMiniShell(ExternalScriptInfo scriptInfo, bool useLocalScope, SessionStateInternal sessionState)
        {
            VerifyPSVersion(scriptInfo);
            this.VerifyRequiredModules(scriptInfo);
            if (string.IsNullOrEmpty(scriptInfo.RequiresApplicationID))
            {
                if ((scriptInfo.RequiresPSSnapIns != null) && scriptInfo.RequiresPSSnapIns.Any<PSSnapInSpecification>())
                {
                    Collection<string> pSSnapinNames = GetPSSnapinNames(scriptInfo.RequiresPSSnapIns);
                    ScriptRequiresException exception = new ScriptRequiresException(scriptInfo.Name, pSSnapinNames, "ScriptRequiresMissingPSSnapIns", true);
                    throw exception;
                }
                return CreateCommandProcessorForScript(scriptInfo, this._context, useLocalScope, sessionState);
            }
            if (string.Equals(this._context.ShellID, scriptInfo.RequiresApplicationID, StringComparison.OrdinalIgnoreCase))
            {
                return CreateCommandProcessorForScript(scriptInfo, this._context, useLocalScope, sessionState);
            }
            string shellPathFromRegistry = GetShellPathFromRegistry(scriptInfo.RequiresApplicationID);
            ScriptRequiresException exception2 = new ScriptRequiresException(scriptInfo.Name, scriptInfo.RequiresApplicationID, shellPathFromRegistry, "ScriptRequiresUnmatchedShellId");
            throw exception2;
        }

        private CommandProcessorBase CreateScriptProcessorForSingleShell(ExternalScriptInfo scriptInfo, ExecutionContext context, bool useLocalScope, SessionStateInternal sessionState)
        {
            VerifyPSVersion(scriptInfo);
            this.VerifyRequiredModules(scriptInfo);
            IEnumerable<PSSnapInSpecification> requiresPSSnapIns = scriptInfo.RequiresPSSnapIns;
            if ((requiresPSSnapIns != null) && requiresPSSnapIns.Any<PSSnapInSpecification>())
            {
                Collection<string> requiresMissingPSSnapIns = null;
                VerifyRequiredSnapins(requiresPSSnapIns, context, out requiresMissingPSSnapIns);
                if (requiresMissingPSSnapIns != null)
                {
                    ScriptRequiresException exception = new ScriptRequiresException(scriptInfo.Name, requiresMissingPSSnapIns, "ScriptRequiresMissingPSSnapIns", true);
                    throw exception;
                }
            }
            else if (!string.IsNullOrEmpty(scriptInfo.RequiresApplicationID))
            {
                GetShellPathFromRegistry(scriptInfo.RequiresApplicationID);
                ScriptRequiresException exception2 = new ScriptRequiresException(scriptInfo.Name, string.Empty, string.Empty, "RequiresShellIDInvalidForSingleShell");
                throw exception2;
            }
            return CreateCommandProcessorForScript(scriptInfo, this._context, useLocalScope, sessionState);
        }

        internal HashSet<string> GetAllowedExtensionsFromPathExt()
        {
            HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string environmentVariable = Environment.GetEnvironmentVariable("PATHEXT");
            discoveryTracer.WriteLine("PATHEXT: {0}", new object[] { environmentVariable });
            if (((environmentVariable == null) || !string.Equals(this.pathExtCacheKey, environmentVariable, StringComparison.OrdinalIgnoreCase)) || (this.cachedPathExt == null))
            {
                this.pathExtCacheKey = environmentVariable;
                if (this.pathExtCacheKey != null)
                {
                    string[] strArray = environmentVariable.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    if (strArray == null)
                    {
                        return set;
                    }
                    this.cachedPathExt = new Collection<string>();
                    foreach (string str2 in strArray)
                    {
                        string item = str2.TrimStart(new char[0]);
                        this.cachedPathExt.Add(item);
                        set.Add(item);
                    }
                }
                return set;
            }
            foreach (string str4 in this.cachedPathExt)
            {
                set.Add(str4);
            }
            return set;
        }

        internal IEnumerator<CmdletInfo> GetCmdletInfo(string cmdletName, bool searchAllScopes)
        {
            PSSnapinQualifiedName instance = PSSnapinQualifiedName.GetInstance(cmdletName);
            if (instance != null)
            {
                SessionStateScopeEnumerator iteratorVariable1 = new SessionStateScopeEnumerator(this._context.EngineSessionState.CurrentScope);
                foreach (SessionStateScope iteratorVariable2 in (IEnumerable<SessionStateScope>) iteratorVariable1)
                {
                    List<CmdletInfo> iteratorVariable3;
                    if (iteratorVariable2.CmdletTable.TryGetValue(instance.ShortName, out iteratorVariable3))
                    {
                        foreach (CmdletInfo iteratorVariable4 in iteratorVariable3)
                        {
                            if (!string.IsNullOrEmpty(instance.PSSnapInName))
                            {
                                if (string.Equals(iteratorVariable4.ModuleName, instance.PSSnapInName, StringComparison.OrdinalIgnoreCase))
                                {
                                    yield return iteratorVariable4;
                                    if (searchAllScopes)
                                    {
                                        continue;
                                    }
                                    break;
                                }
                                if (InitialSessionState.IsEngineModule(iteratorVariable4.ModuleName) && string.Equals(iteratorVariable4.ModuleName, InitialSessionState.GetNestedModuleDllName(instance.PSSnapInName), StringComparison.OrdinalIgnoreCase))
                                {
                                    yield return iteratorVariable4;
                                    if (!searchAllScopes)
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                yield return iteratorVariable4;
                                if (!searchAllScopes)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private int GetCmdletRemovalIndex(List<CmdletInfo> cacheEntry, string PSSnapin)
        {
            for (int i = 0; i < cacheEntry.Count; i++)
            {
                if (string.Equals(cacheEntry[i].ModuleName, PSSnapin, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return -1;
        }

        internal static PSModuleAutoLoadingPreference GetCommandDiscoveryPreference(ExecutionContext context, VariablePath variablePath, string environmentVariable)
        {
            if (context == null)
            {
                throw PSTraceSource.NewArgumentNullException("context");
            }
            object variableValue = context.GetVariableValue(variablePath);
            if (variableValue != null)
            {
                return LanguagePrimitives.ConvertTo<PSModuleAutoLoadingPreference>(variableValue);
            }
            string str = Environment.GetEnvironmentVariable(environmentVariable);
            if (!string.IsNullOrEmpty(str))
            {
                return LanguagePrimitives.ConvertTo<PSModuleAutoLoadingPreference>(str);
            }
            return PSModuleAutoLoadingPreference.All;
        }

        internal IEnumerable<string> GetCommandPathSearcher(IEnumerable<string> patterns)
        {
            return new CommandPathSearch(patterns, this.GetLookupDirectoryPaths(), this._context);
        }

        internal IEnumerable<string> GetLookupDirectoryPaths()
        {
            LookupPathCollection paths = new LookupPathCollection();
            string environmentVariable = Environment.GetEnvironmentVariable("PATH");
            discoveryTracer.WriteLine("PATH: {0}", new object[] { environmentVariable });
            if (((environmentVariable == null) || !string.Equals(this.pathCacheKey, environmentVariable, StringComparison.OrdinalIgnoreCase)) || (this.cachedPath == null))
            {
                this.cachedLookupPaths = null;
                this.pathCacheKey = environmentVariable;
                if (this.pathCacheKey != null)
                {
                    string[] strArray = this.pathCacheKey.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    if (strArray != null)
                    {
                        this.cachedPath = new Collection<string>();
                        foreach (string str2 in strArray)
                        {
                            string item = str2.TrimStart(new char[0]);
                            this.cachedPath.Add(item);
                            paths.Add(item);
                        }
                    }
                }
            }
            else
            {
                paths.AddRange(this.cachedPath);
            }
            if (this.cachedLookupPaths == null)
            {
                this.cachedLookupPaths = paths;
            }
            return this.cachedLookupPaths;
        }

        private static Collection<string> GetPSSnapinNames(IEnumerable<PSSnapInSpecification> PSSnapins)
        {
            Collection<string> collection = new Collection<string>();
            foreach (PSSnapInSpecification specification in PSSnapins)
            {
                collection.Add(BuildPSSnapInDisplayName(specification));
            }
            return collection;
        }

        private static CommandProcessorBase GetScriptAsCmdletProcessor(IScriptCommandInfo scriptCommandInfo, ExecutionContext context, bool useNewScope, bool fromScriptFile, SessionStateInternal sessionState)
        {
            if ((scriptCommandInfo.ScriptBlock == null) || !scriptCommandInfo.ScriptBlock.UsesCmdletBinding)
            {
                return null;
            }
            sessionState = sessionState ?? (scriptCommandInfo.ScriptBlock.SessionStateInternal ?? context.EngineSessionState);
            return new CommandProcessor(scriptCommandInfo, context, useNewScope, fromScriptFile, sessionState);
        }

        internal ScriptInfo GetScriptInfo(string name)
        {
            ScriptInfo info = null;
            if (this.cachedScriptInfo.ContainsKey(name))
            {
                info = this.cachedScriptInfo[name];
            }
            return info;
        }

        internal static string GetShellPathFromRegistry (string shellID)
		{
			string str = null;
			if (PowerShellConfiguration.IsWindows) {
				try {
					RegistryKey key = Registry.LocalMachine.OpenSubKey (Utils.GetRegistryConfigurationPath (shellID));
					if (key == null) {
						return str;
					}
					RegistryValueKind valueKind = key.GetValueKind ("path");
					if ((valueKind != RegistryValueKind.ExpandString) && (valueKind != RegistryValueKind.String)) {
						return str;
					}
					str = key.GetValue ("path") as string;
				} catch (SecurityException) {
				} catch (IOException) {
				} catch (ArgumentException) {
				}
			} else {
				return PowerShellConfiguration.PowerShellEngine.ApplicationBase;
			}
            return str;
        }

        private static CommandInfo InvokeCommandNotFoundHandler(string commandName, ExecutionContext context, string originalCommandName, CommandOrigin commandOrigin, CommandInfo result)
        {
            EventHandler<CommandLookupEventArgs> commandNotFoundAction = context.EngineIntrinsics.InvokeCommand.CommandNotFoundAction;
            if (commandNotFoundAction != null)
            {
                discoveryTracer.WriteLine("Executing CommandNotFoundAction: {0}", new object[] { commandName });
                try
                {
                    context.CommandDiscovery.RegisterLookupCommandInfoAction("ActiveCommandNotFound", originalCommandName);
                    CommandLookupEventArgs e = new CommandLookupEventArgs(originalCommandName, commandOrigin, context);
                    commandNotFoundAction(originalCommandName, e);
                    result = e.Command;
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                }
                finally
                {
                    context.CommandDiscovery.UnregisterLookupCommandInfoAction("ActiveCommandNotFound", originalCommandName);
                }
            }
            return result;
        }

        private bool IsSpecialCmdlet(Type implementingType)
        {
            return (string.Equals(implementingType.FullName, "Microsoft.PowerShell.Commands.OutLineOutputCommand", StringComparison.OrdinalIgnoreCase) || string.Equals(implementingType.FullName, "Microsoft.PowerShell.Commands.FormatDefaultCommand", StringComparison.OrdinalIgnoreCase));
        }

        private void LoadScriptInfo()
        {
            if (this._context.RunspaceConfiguration != null)
            {
                foreach (ScriptConfigurationEntry entry in this._context.RunspaceConfiguration.Scripts)
                {
                    try
                    {
                        this.cachedScriptInfo.Add(entry.Name, new ScriptInfo(entry.Name, ScriptBlock.Create(this._context, entry.Definition), this._context));
                    }
                    catch (ArgumentException)
                    {
                        throw PSTraceSource.NewNotSupportedException("DiscoveryExceptions", "DuplicateScriptName", new object[] { entry.Name });
                    }
                }
            }
        }

        internal CommandInfo LookupCommandInfo(string commandName)
        {
            return this.LookupCommandInfo(commandName, CommandOrigin.Internal);
        }

        internal CommandInfo LookupCommandInfo(string commandName, CommandOrigin commandOrigin)
        {
            return LookupCommandInfo(commandName, commandOrigin, this._context);
        }

        internal static CommandInfo LookupCommandInfo(string commandName, CommandOrigin commandOrigin, ExecutionContext context)
        {
            return LookupCommandInfo(commandName, CommandTypes.All, SearchResolutionOptions.None, commandOrigin, context);
        }

        internal static CommandInfo LookupCommandInfo(string commandName, CommandTypes commandTypes, SearchResolutionOptions searchResolutionOptions, CommandOrigin commandOrigin, ExecutionContext context)
        {
            if (string.IsNullOrEmpty(commandName))
            {
                return null;
            }
            CommandInfo result = null;
            string command = commandName;
            Exception lastError = null;
            CommandLookupEventArgs e = null;
            EventHandler<CommandLookupEventArgs> preCommandLookupAction = context.EngineIntrinsics.InvokeCommand.PreCommandLookupAction;
            if (preCommandLookupAction != null)
            {
                discoveryTracer.WriteLine("Executing PreCommandLookupAction: {0}", new object[] { commandName });
                try
                {
                    context.CommandDiscovery.RegisterLookupCommandInfoAction("ActivePreLookup", command);
                    e = new CommandLookupEventArgs(command, commandOrigin, context);
                    preCommandLookupAction(command, e);
                    discoveryTracer.WriteLine("PreCommandLookupAction returned: {0}", new object[] { e.Command });
                }
                catch (Exception exception2)
                {
                    CommandProcessorBase.CheckForSevereException(exception2);
                }
                finally
                {
                    context.CommandDiscovery.UnregisterLookupCommandInfoAction("ActivePreLookup", commandName);
                }
            }
            if ((e == null) || !e.StopSearch)
            {
                discoveryTracer.WriteLine("Looking up command: {0}", new object[] { commandName });
                result = TryNormalSearch(commandName, context, commandOrigin, searchResolutionOptions, commandTypes, ref lastError);
                if (result == null)
                {
                    PSModuleAutoLoadingPreference preference = GetCommandDiscoveryPreference(context, SpecialVariables.PSModuleAutoLoadingPreferenceVarPath, "PSModuleAutoLoadingPreference");
                    if (preference != PSModuleAutoLoadingPreference.None)
                    {
                        result = TryModuleAutoLoading(commandName, context, command, commandOrigin, result, ref lastError);
                    }
                    if (result == null)
                    {
                        if (preference == PSModuleAutoLoadingPreference.All)
                        {
                            result = TryModuleAutoDiscovery(commandName, context, command, commandOrigin, searchResolutionOptions, commandTypes, ref lastError);
                        }
                        if (result == null)
                        {
                            result = InvokeCommandNotFoundHandler(commandName, context, command, commandOrigin, result);
                        }
                    }
                }
            }
            else if (e.Command != null)
            {
                result = e.Command;
            }
            if (result != null)
            {
                EventHandler<CommandLookupEventArgs> postCommandLookupAction = context.EngineIntrinsics.InvokeCommand.PostCommandLookupAction;
                if (postCommandLookupAction != null)
                {
                    discoveryTracer.WriteLine("Executing PostCommandLookupAction: {0}", new object[] { command });
                    try
                    {
                        context.CommandDiscovery.RegisterLookupCommandInfoAction("ActivePostCommand", command);
                        e = new CommandLookupEventArgs(command, commandOrigin, context) {
                            Command = result
                        };
                        postCommandLookupAction(command, e);
                        if (e != null)
                        {
                            result = e.Command;
                            discoveryTracer.WriteLine("PreCommandLookupAction returned: {0}", new object[] { e.Command });
                        }
                    }
                    catch (Exception exception3)
                    {
                        CommandProcessorBase.CheckForSevereException(exception3);
                    }
                    finally
                    {
                        context.CommandDiscovery.UnregisterLookupCommandInfoAction("ActivePostCommand", command);
                    }
                }
            }
            if (result == null)
            {
                discoveryTracer.TraceError("'{0}' is not recognized as a cmdlet, function, operable program or script file.", new object[] { commandName });
                CommandNotFoundException exception4 = new CommandNotFoundException(command, lastError, "CommandNotFoundException", DiscoveryExceptions.CommandNotFoundException, new object[0]);

				throw exception4;
            }
            return result;
        }

        internal CommandProcessorBase LookupCommandProcessor(string commandName, CommandOrigin commandOrigin, bool? useLocalScope)
        {
            CommandInfo commandInfo = null;
            commandInfo = this.LookupCommandInfo(commandName, commandOrigin);
            CommandProcessorBase base2 = this.LookupCommandProcessor(commandInfo, commandOrigin, useLocalScope, null);
            base2.Command.MyInvocation.InvocationName = commandName;
            return base2;
        }

        internal CommandProcessorBase LookupCommandProcessor(CommandInfo commandInfo, CommandOrigin commandOrigin, bool? useLocalScope, SessionStateInternal sessionState)
        {
            CommandProcessorBase base2 = null;
            FunctionInfo info3;
            CommandNotFoundException exception6;
            HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            while (((commandInfo.CommandType == CommandTypes.Alias) && !set.Contains(commandInfo.Name)) && ((commandOrigin == CommandOrigin.Internal) || (commandInfo.Visibility == SessionStateEntryVisibility.Public)))
            {
                set.Add(commandInfo.Name);
                AliasInfo info = (AliasInfo) commandInfo;
                commandInfo = info.ResolvedCommand;
                if (commandInfo == null)
                {
                    commandInfo = LookupCommandInfo(info.Definition, commandOrigin, this._context);
                }
                if (commandInfo == null)
                {
                    CommandNotFoundException exception = new CommandNotFoundException(info.Name, null, "AliasNotResolvedException", DiscoveryExceptions.AliasNotResolvedException, new object[] { info.UnresolvedCommandName });
                    throw exception;
                }
                PSSQMAPI.IncrementData(CommandTypes.Alias);
            }
            ShouldRun(this._context, this._context.EngineHostInterface, commandInfo, commandOrigin);
            CommandTypes commandType = commandInfo.CommandType;
            if (commandType <= CommandTypes.ExternalScript)
            {
                switch (commandType)
                {
                    case CommandTypes.Function:
                    case CommandTypes.Filter:
                        goto Label_01E3;

                    case CommandTypes.Cmdlet:
                        base2 = new CommandProcessor((CmdletInfo) commandInfo, this._context);
                        goto Label_0260;

                    case CommandTypes.ExternalScript:
                    {
                        ExternalScriptInfo scriptInfo = (ExternalScriptInfo) commandInfo;
                        scriptInfo.SignatureChecked = true;
                        try
                        {
                            if (!this._context.IsSingleShell)
                            {
                                bool? nullable = useLocalScope;
                                base2 = this.CreateScriptProcessorForMiniShell(scriptInfo, nullable.HasValue ? nullable.GetValueOrDefault() : true, sessionState);
                            }
                            else
                            {
                                bool? nullable2 = useLocalScope;
                                base2 = this.CreateScriptProcessorForSingleShell(scriptInfo, this._context, nullable2.HasValue ? nullable2.GetValueOrDefault() : true, sessionState);
                            }
                            goto Label_0260;
                        }
                        catch (ScriptRequiresSyntaxException exception2)
                        {
                            CommandNotFoundException exception3 = new CommandNotFoundException(exception2.Message, exception2);
                            throw exception3;
                        }
                        catch (PSArgumentException exception4)
                        {
                            CommandNotFoundException exception5 = new CommandNotFoundException(commandInfo.Name, exception4, "ScriptRequiresInvalidFormat", DiscoveryExceptions.ScriptRequiresInvalidFormat, new object[0]);
                            throw exception5;
                        }
                        goto Label_01E3;
                    }
                }
                goto Label_023F;
            }
            if (commandType != CommandTypes.Application)
            {
                if (commandType == CommandTypes.Script)
                {
                    bool? nullable4 = useLocalScope;
                    base2 = CreateCommandProcessorForScript((ScriptInfo) commandInfo, this._context, nullable4.HasValue ? nullable4.GetValueOrDefault() : true, sessionState);
                    goto Label_0260;
                }
                if (commandType == CommandTypes.Workflow)
                {
                    goto Label_01E3;
                }
                goto Label_023F;
            }
            base2 = new NativeCommandProcessor((ApplicationInfo) commandInfo, this._context);
            goto Label_0260;
        Label_01E3:
            info3 = (FunctionInfo) commandInfo;
            bool? nullable3 = useLocalScope;
            base2 = CreateCommandProcessorForScript(info3, this._context, nullable3.HasValue ? nullable3.GetValueOrDefault() : true, sessionState);
            goto Label_0260;
        Label_023F:
            exception6 = new CommandNotFoundException(commandInfo.Name, null, "CommandNotFoundException", DiscoveryExceptions.CommandNotFoundException, new object[0]);
            throw exception6;
        Label_0260:
            PSSQMAPI.IncrementData(commandInfo.CommandType);
            base2.Command.CommandOriginInternal = commandOrigin;
            base2.Command.MyInvocation.InvocationName = commandInfo.Name;
            return base2;
        }

        private CmdletInfo NewCmdletInfo(SessionStateCmdletEntry entry)
        {
            return NewCmdletInfo(entry, this._context);
        }

        private CmdletInfo NewCmdletInfo(CmdletConfigurationEntry entry, SessionStateEntryVisibility visibility)
        {
            return new CmdletInfo(entry.Name, entry.ImplementingType, entry.HelpFileName, entry.PSSnapIn, this._context) { Visibility = visibility };
        }

        internal static CmdletInfo NewCmdletInfo(SessionStateCmdletEntry entry, ExecutionContext context)
        {
            CmdletInfo info = new CmdletInfo(entry.Name, entry.ImplementingType, entry.HelpFileName, entry.PSSnapIn, context) {
                Visibility = entry.Visibility
            };
            info.SetModule(entry.Module);
            return info;
        }

        internal void RegisterLookupCommandInfoAction(string currentAction, string command)
        {
            HashSet<string> activeModuleSearch = null;
            string str = currentAction;
            if (str != null)
            {
                if (!(str == "ActivePreLookup"))
                {
                    if (str == "ActiveModuleSearch")
                    {
                        activeModuleSearch = this.activeModuleSearch;
                    }
                    else if (str == "ActiveCommandNotFound")
                    {
                        activeModuleSearch = this.activeCommandNotFound;
                    }
                    else if (str == "ActivePostCommand")
                    {
                        activeModuleSearch = this.activePostCommand;
                    }
                }
                else
                {
                    activeModuleSearch = this.activePreLookup;
                }
            }
            if (activeModuleSearch.Contains(command))
            {
                throw new InvalidOperationException();
            }
            activeModuleSearch.Add(command);
        }

        private void RemoveCmdletFromCache(CmdletConfigurationEntry entry)
        {
            IDictionary<string, List<CmdletInfo>> cmdletTable = this._context.EngineSessionState.GetCmdletTable();
            if (cmdletTable.ContainsKey(entry.Name))
            {
                List<CmdletInfo> cacheEntry = cmdletTable[entry.Name];
                int cmdletRemovalIndex = this.GetCmdletRemovalIndex(cacheEntry, (entry.PSSnapIn == null) ? string.Empty : entry.PSSnapIn.Name);
                if (cmdletRemovalIndex >= 0)
                {
                    string name = cacheEntry[cmdletRemovalIndex].Name;
                    cacheEntry.RemoveAt(cmdletRemovalIndex);
                    this._context.EngineSessionState.RemoveCmdlet(name, cmdletRemovalIndex, true);
                }
                if (cacheEntry.Count == 0)
                {
                    this._context.EngineSessionState.RemoveCmdletEntry(entry.Name, true);
                }
            }
        }

        internal static void ShouldRun(ExecutionContext context, PSHost host, CommandInfo commandInfo, CommandOrigin commandOrigin)
        {
            try
            {
                if ((commandOrigin == CommandOrigin.Runspace) && (commandInfo.Visibility != SessionStateEntryVisibility.Public))
                {
                    CommandNotFoundException exception = new CommandNotFoundException(commandInfo.Name, null, "CommandNotFoundException", DiscoveryExceptions.CommandNotFoundException, new object[0]);
                    throw exception;
                }
                context.AuthorizationManager.ShouldRunInternal(commandInfo, commandOrigin, host);
            }
            catch (PSSecurityException exception2)
            {
                MshLog.LogCommandHealthEvent(context, exception2, Severity.Warning);
                MshLog.LogCommandLifecycleEvent(context, CommandState.Terminated, commandInfo.Name);
                throw;
            }
        }

        private static CommandInfo TryModuleAutoDiscovery(string commandName, ExecutionContext context, string originalCommandName, CommandOrigin commandOrigin, SearchResolutionOptions searchResolutionOptions, CommandTypes commandTypes, ref Exception lastError)
        {
            CommandInfo info = null;
            try
            {
                CmdletInfo cmdlet = context.SessionState.InvokeCommand.GetCmdlet(@"Microsoft.PowerShell.Core\Get-Module");
                if ((commandOrigin == CommandOrigin.Internal) || ((cmdlet != null) && (cmdlet.Visibility == SessionStateEntryVisibility.Public)))
                {
                    cmdlet = context.SessionState.InvokeCommand.GetCmdlet(@"Microsoft.PowerShell.Core\Import-Module");
                    if ((commandOrigin == CommandOrigin.Internal) || ((cmdlet != null) && (cmdlet.Visibility == SessionStateEntryVisibility.Public)))
                    {
                        discoveryTracer.WriteLine("Executing non module-qualified search: {0}", new object[] { commandName });
                        context.CommandDiscovery.RegisterLookupCommandInfoAction("ActiveModuleSearch", commandName);
                        foreach (string str in ModuleUtils.GetDefaultAvailableModuleFiles(true, true, context))
                        {
                            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(str);
                            Dictionary<string, List<CommandTypes>> dictionary = AnalysisCache.GetExportedCommands(str, false, context);
                            if (dictionary != null)
                            {
                                if (dictionary.ContainsKey(commandName))
                                {
                                    CommandInfo commandInfo = new CmdletInfo("Import-Module", typeof(ImportModuleCommand), null, null, context) {
                                        Visibility = cmdlet.Visibility
                                    };
                                    Command command = new Command(commandInfo);
                                    discoveryTracer.WriteLine("Found in module: {0}", new object[] { str });
                                    PowerShell shell = PowerShell.Create(RunspaceMode.CurrentRunspace).AddCommand(command).AddParameter("Name", str).AddParameter("Scope", "GLOBAL").AddParameter("PassThru").AddParameter("ErrorAction", ActionPreference.Ignore).AddParameter("WarningAction", ActionPreference.Ignore).AddParameter("Verbose", false).AddParameter("Debug", false);
                                    Collection<PSModuleInfo> collection = null;
                                    try
                                    {
                                        collection = shell.Invoke<PSModuleInfo>();
                                    }
                                    catch (Exception exception)
                                    {
                                        discoveryTracer.WriteLine("Encountered error importing module: {0}", new object[] { exception.Message });
                                        lastError = exception;
                                        CommandProcessorBase.CheckForSevereException(exception);
                                    }
                                    if ((collection == null) || (collection.Count == 0))
                                    {
                                        string resourceStr = StringUtil.Format(DiscoveryExceptions.CouldNotAutoImportMatchingModule, commandName, fileNameWithoutExtension);
                                        CommandNotFoundException exception2 = new CommandNotFoundException(originalCommandName, lastError, "CouldNotAutoloadMatchingModule", resourceStr, new object[0]);
                                        throw exception2;
                                    }
                                    info = LookupCommandInfo(commandName, commandTypes, searchResolutionOptions, commandOrigin, context);
                                }
                                if (info != null)
                                {
                                    return info;
                                }
                            }
                        }
                    }
                    return info;
                }
                return info;
            }
            catch (CommandNotFoundException)
            {
                throw;
            }
            catch (Exception exception3)
            {
                CommandProcessorBase.CheckForSevereException(exception3);
            }
            finally
            {
                context.CommandDiscovery.UnregisterLookupCommandInfoAction("ActiveModuleSearch", commandName);
            }
            return info;
        }

        private static CommandInfo TryModuleAutoLoading(string commandName, ExecutionContext context, string originalCommandName, CommandOrigin commandOrigin, CommandInfo result, ref Exception lastError)
        {
            int length = commandName.IndexOfAny(new char[] { ':', '\\' });
            if ((length == -1) || (commandName[length] == ':'))
            {
                return null;
            }
            string str = commandName.Substring(0, length);
            string str2 = commandName.Substring(length + 1, (commandName.Length - length) - 1);
            if ((string.IsNullOrEmpty(str) || string.IsNullOrEmpty(str2)) || str.EndsWith(".", StringComparison.Ordinal))
            {
                return null;
            }
            try
            {
                discoveryTracer.WriteLine("Executing module-qualified search: {0}", new object[] { commandName });
                context.CommandDiscovery.RegisterLookupCommandInfoAction("ActiveModuleSearch", commandName);
                CmdletInfo cmdlet = context.SessionState.InvokeCommand.GetCmdlet(@"Microsoft.PowerShell.Core\Import-Module");
                if ((commandOrigin == CommandOrigin.Internal) || ((cmdlet != null) && (cmdlet.Visibility == SessionStateEntryVisibility.Public)))
                {
                    List<PSModuleInfo> modules = context.Modules.GetModules(new string[] { str }, false);
                    PSModuleInfo info2 = null;
                    if ((modules == null) || (modules.Count == 0))
                    {
                        CommandInfo commandInfo = new CmdletInfo("Import-Module", typeof(ImportModuleCommand), null, null, context) {
                            Visibility = cmdlet.Visibility
                        };
                        Command command = new Command(commandInfo);
                        Collection<PSModuleInfo> collection = null;
                        discoveryTracer.WriteLine("Attempting to load module: {0}", new object[] { str });
                        try
                        {
                            collection = PowerShell.Create(RunspaceMode.CurrentRunspace).AddCommand(command).AddParameter("Name", str).AddParameter("Scope", "GLOBAL").AddParameter("ErrorAction", ActionPreference.Ignore).AddParameter("PassThru").AddParameter("WarningAction", ActionPreference.Ignore).AddParameter("Verbose", false).AddParameter("Debug", false).Invoke<PSModuleInfo>();
                        }
                        catch (Exception exception)
                        {
                            discoveryTracer.WriteLine("Encountered error importing module: {0}", new object[] { exception.Message });
                            lastError = exception;
                            CommandProcessorBase.CheckForSevereException(exception);
                        }
                        if ((collection == null) || (collection.Count == 0))
                        {
                            string resourceStr = StringUtil.Format(DiscoveryExceptions.CouldNotAutoImportModule, str);
                            CommandNotFoundException exception2 = new CommandNotFoundException(originalCommandName, lastError, "CouldNotAutoLoadModule", resourceStr, new object[0]);
                            throw exception2;
                        }
                        info2 = collection[0];
                    }
                    else
                    {
                        info2 = modules[0];
                    }
                    if (info2.ExportedCommands.ContainsKey(str2))
                    {
                        result = info2.ExportedCommands[str2];
                    }
                }
                return result;
            }
            catch (CommandNotFoundException)
            {
                throw;
            }
            catch (Exception exception3)
            {
                CommandProcessorBase.CheckForSevereException(exception3);
            }
            finally
            {
                context.CommandDiscovery.UnregisterLookupCommandInfoAction("ActiveModuleSearch", commandName);
            }
            return result;
        }

        private static CommandInfo TryNormalSearch(string commandName, ExecutionContext context, CommandOrigin commandOrigin, SearchResolutionOptions searchResolutionOptions, CommandTypes commandTypes, ref Exception lastError)
        {
            CommandInfo current = null;
            CommandSearcher searcher = new CommandSearcher(commandName, searchResolutionOptions, commandTypes, context) {
                CommandOrigin = commandOrigin
            };
            try
            {
                if (!searcher.MoveNext())
                {
                    if (!commandName.Contains("-") && !commandName.Contains(@"\"))
                    {
                        discoveryTracer.WriteLine("The command [{0}] was not found, trying again with get- prepended", new object[] { commandName });
                        commandName = "get" + '-' + commandName;
                        try
                        {
                            current = LookupCommandInfo(commandName, commandTypes, searchResolutionOptions, commandOrigin, context);
                        }
                        catch (CommandNotFoundException)
                        {
                        }
                    }
                    return current;
                }
                current = searcher.Current;
            }
            catch (ArgumentException exception)
            {
                lastError = exception;
            }
            catch (PathTooLongException exception2)
            {
                lastError = exception2;
            }
            catch (FileLoadException exception3)
            {
                lastError = exception3;
            }
            catch (FormatException exception4)
            {
                lastError = exception4;
            }
            catch (MetadataException exception5)
            {
                lastError = exception5;
            }
            return current;
        }

        internal void UnregisterLookupCommandInfoAction(string currentAction, string command)
        {
            HashSet<string> activeModuleSearch = null;
            string str = currentAction;
            if (str != null)
            {
                if (!(str == "ActivePreLookup"))
                {
                    if (str == "ActiveModuleSearch")
                    {
                        activeModuleSearch = this.activeModuleSearch;
                    }
                    else if (str == "ActiveCommandNotFound")
                    {
                        activeModuleSearch = this.activeCommandNotFound;
                    }
                    else if (str == "ActivePostCommand")
                    {
                        activeModuleSearch = this.activePostCommand;
                    }
                }
                else
                {
                    activeModuleSearch = this.activePreLookup;
                }
            }
            if (activeModuleSearch.Contains(command))
            {
                activeModuleSearch.Remove(command);
            }
        }

        internal void UpdateCmdletCache()
        {
            if (!this._cmdletCacheInitialized)
            {
                foreach (CmdletConfigurationEntry entry in this._context.RunspaceConfiguration.Cmdlets)
                {
                    this.AddCmdletToCache(entry);
                }
                this._cmdletCacheInitialized = true;
            }
            else
            {
                foreach (CmdletConfigurationEntry entry2 in this._context.RunspaceConfiguration.Cmdlets.UpdateList)
                {
                    if (entry2 != null)
                    {
                        switch (entry2.Action)
                        {
                            case UpdateAction.Add:
                                this.AddCmdletToCache(entry2);
                                break;

                            case UpdateAction.Remove:
                                this.RemoveCmdletFromCache(entry2);
                                break;
                        }
                    }
                }
            }
        }

        internal static void VerifyPSVersion(ExternalScriptInfo scriptInfo)
        {
            Version requiresPSVersion = scriptInfo.RequiresPSVersion;
            if ((requiresPSVersion != null) && !Utils.IsPSVersionSupported(requiresPSVersion))
            {
                ScriptRequiresException exception = new ScriptRequiresException(scriptInfo.Name, requiresPSVersion, PSVersionInfo.PSVersion.ToString(), "ScriptRequiresUnmatchedPSVersion");
                throw exception;
            }
        }

        private void VerifyRequiredModules(ExternalScriptInfo scriptInfo)
        {
            if (scriptInfo.RequiresModules != null)
            {
                foreach (ModuleSpecification specification in scriptInfo.RequiresModules)
                {
                    ErrorRecord error = null;
                    ModuleCmdletBase.LoadRequiredModule(this.Context, null, specification, null, ModuleCmdletBase.ManifestProcessingFlags.LoadElements | ModuleCmdletBase.ManifestProcessingFlags.WriteErrors, out error);
                    if (error != null)
                    {
                        ScriptRequiresException exception = new ScriptRequiresException(scriptInfo.Name, new Collection<string> { specification.Name }, "ScriptRequiresMissingModules", false, error);
                        throw exception;
                    }
                }
            }
        }

        private static void VerifyRequiredSnapins(IEnumerable<PSSnapInSpecification> requiresPSSnapIns, ExecutionContext context, out Collection<string> requiresMissingPSSnapIns)
        {
            requiresMissingPSSnapIns = null;
            bool flag = false;
            RunspaceConfigForSingleShell runspaceConfiguration = null;
            if (context.InitialSessionState != null)
            {
                flag = true;
            }
            else if (context.RunspaceConfiguration != null)
            {
                runspaceConfiguration = context.RunspaceConfiguration as RunspaceConfigForSingleShell;
            }
            foreach (PSSnapInSpecification specification in requiresPSSnapIns)
            {
                IEnumerable<PSSnapInInfo> source = null;
                if (flag)
                {
                    source = context.InitialSessionState.GetPSSnapIn(specification.Name);
                }
                else
                {
                    source = runspaceConfiguration.ConsoleInfo.GetPSSnapIn(specification.Name, false);
                }
                if ((source == null) || (source.Count<PSSnapInInfo>() == 0))
                {
                    if (requiresMissingPSSnapIns == null)
                    {
                        requiresMissingPSSnapIns = new Collection<string>();
                    }
                    requiresMissingPSSnapIns.Add(BuildPSSnapInDisplayName(specification));
                }
                else
                {
                    PSSnapInInfo info = source.First<PSSnapInInfo>();
                    if ((specification.Version != null) && !AreInstalledRequiresVersionsCompatible(specification.Version, info.Version))
                    {
                        if (requiresMissingPSSnapIns == null)
                        {
                            requiresMissingPSSnapIns = new Collection<string>();
                        }
                        requiresMissingPSSnapIns.Add(BuildPSSnapInDisplayName(specification));
                    }
                }
            }
        }

        internal ExecutionContext Context
        {
            get
            {
                return this._context;
            }
        }

        internal static IEnumerable<string> PathExtensions
        {
            get
            {
                Collection<string> collection = null;
                string environmentVariable = Environment.GetEnvironmentVariable("PATHEXT");
                if (((pathExtensionsCacheKey != null) && (environmentVariable != null)) && ((cachedPathExtensions != null) && environmentVariable.Equals(pathExtensionsCacheKey, StringComparison.OrdinalIgnoreCase)))
                {
                    return cachedPathExtensions;
                }
                collection = new Collection<string>();
                if (environmentVariable != null)
                {
                    string[] strArray = environmentVariable.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    if (strArray != null)
                    {
                        foreach (string str2 in strArray)
                        {
                            collection.Add(str2);
                        }
                    }
                    pathExtensionsCacheKey = environmentVariable;
                    cachedPathExtensions = collection;
                }
                return collection;
            }
        }

        internal Dictionary<string, ScriptInfo> ScriptCache
        {
            get
            {
                return this.cachedScriptInfo;
            }
        }

        
    }
}

