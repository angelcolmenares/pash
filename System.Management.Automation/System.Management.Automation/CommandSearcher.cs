namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Management.Automation.Provider;

    internal class CommandSearcher : IEnumerable<CommandInfo>, IEnumerable, IEnumerator<CommandInfo>, IDisposable, IEnumerator
    {
        private System.Management.Automation.CommandOrigin _commandOrigin = System.Management.Automation.CommandOrigin.Internal;
        private ExecutionContext _context;
        private CommandInfo _currentMatch;
        private static readonly char[] _pathSeparators = new char[] { '\\', '/', ':' };
        private bool canDoPathLookup;
        private CanDoPathLookupResult canDoPathLookupResult;
        private string commandName;
        private SearchResolutionOptions commandResolutionOptions;
        private CommandTypes commandTypes = CommandTypes.All;
        private SearchState currentState;
        private IEnumerator<AliasInfo> matchingAlias;
        private IEnumerator<CmdletInfo> matchingCmdlet;
        private IEnumerator<CommandInfo> matchingFunctionEnumerator;
        private IEnumerator<string> matchingScript;
        private CommandPathSearch pathSearcher;

        internal CommandSearcher(string commandName, SearchResolutionOptions options, CommandTypes commandTypes, ExecutionContext context)
        {
            this.commandName = commandName;
            this._context = context;
            this.commandResolutionOptions = options;
            this.commandTypes = commandTypes;
            this.Reset();
        }

        private static CanDoPathLookupResult CanDoPathLookup(string possiblePath)
        {
            CanDoPathLookupResult yes = CanDoPathLookupResult.Yes;
            if (WildcardPattern.ContainsWildcardCharacters(possiblePath))
            {
                return CanDoPathLookupResult.WildcardCharacters;
            }
            try
            {
                if (!string.IsNullOrEmpty(possiblePath))
                {
                    if (Path.IsPathRooted(possiblePath.Replace("\"", "")))
                    {
                        return CanDoPathLookupResult.PathIsRooted;
                    }
                }
            }
            catch (ArgumentException)
            {
                return CanDoPathLookupResult.IllegalCharacters;
            }
            if (possiblePath.IndexOfAny(Utils.DirectorySeparators) != -1)
            {
                return CanDoPathLookupResult.DirectorySeparator;
            }
            if (possiblePath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                yes = CanDoPathLookupResult.IllegalCharacters;
            }
            return yes;
        }

        private static bool checkPath(string path, string commandName)
        {
            return path.StartsWith(commandName, StringComparison.OrdinalIgnoreCase);
        }

        internal IEnumerable<string> ConstructSearchPatternsFromName(string name)
        {
            Collection<string> collection = new Collection<string>();
            bool flag = false;
            if (!string.IsNullOrEmpty(Path.GetExtension(name)))
            {
                collection.Add(name);
                flag = true;
            }
            if ((this.commandTypes & CommandTypes.ExternalScript) != 0)
            {
                collection.Add(name + ".ps1");
                collection.Add(name + ".psm1");
                collection.Add(name + ".psd1");
            }
            if ((this.commandTypes & CommandTypes.Application) != 0)
            {
                foreach (string str in CommandDiscovery.PathExtensions)
                {
                    collection.Add(name + str);
                }
            }
            if (!flag)
            {
                collection.Add(name);
            }
            return collection;
        }

        public void Dispose()
        {
            if (this.pathSearcher != null)
            {
                this.pathSearcher.Dispose();
                this.pathSearcher = null;
            }
            this.Reset();
            GC.SuppressFinalize(this);
        }

        private string DoPowerShellRelativePathLookup()
        {
            string str = null;
            if (((this._context.EngineSessionState == null) || (this._context.EngineSessionState.ProviderCount <= 0)) || ((this.commandName[0] != '.') && (this.commandName[0] != '~')))
            {
                return str;
            }
            using (CommandDiscovery.discoveryTracer.TraceScope("{0} appears to be a relative path. Trying to resolve relative path", new object[] { this.commandName }))
            {
                return this.ResolvePSPath(this.commandName);
            }
        }

        private AliasInfo GetAliasFromModules(string command)
        {
            AliasInfo info = null;
            if (command.IndexOf('\\') > 0)
            {
                PSSnapinQualifiedName instance = PSSnapinQualifiedName.GetInstance(command);
                if ((instance != null) && !string.IsNullOrEmpty(instance.PSSnapInName))
                {
                    PSModuleInfo importedModuleByName = this.GetImportedModuleByName(instance.PSSnapInName);
                    if (importedModuleByName != null)
                    {
                        importedModuleByName.ExportedAliases.TryGetValue(instance.ShortName, out info);
                    }
                }
            }
            return info;
        }

        private CommandInfo GetFunction(string function)
        {
            CommandInfo info = this._context.EngineSessionState.GetFunction(function);
            if (info != null)
            {
                if (info is FilterInfo)
                {
                    CommandDiscovery.discoveryTracer.WriteLine("Filter found: {0}", new object[] { function });
                    return info;
                }
                CommandDiscovery.discoveryTracer.WriteLine("Function found: {0}", new object[] { function });
                return info;
            }
            return this.GetFunctionFromModules(function);
        }

        private CommandInfo GetFunctionFromModules(string command)
        {
            FunctionInfo info = null;
            if (command.IndexOf('\\') > 0)
            {
                PSSnapinQualifiedName instance = PSSnapinQualifiedName.GetInstance(command);
                if ((instance != null) && !string.IsNullOrEmpty(instance.PSSnapInName))
                {
                    PSModuleInfo importedModuleByName = this.GetImportedModuleByName(instance.PSSnapInName);
                    if (importedModuleByName != null)
                    {
                        importedModuleByName.ExportedFunctions.TryGetValue(instance.ShortName, out info);
                    }
                }
            }
            return info;
        }

        private PSModuleInfo GetImportedModuleByName(string moduleName)
        {
            PSModuleInfo info = null;
            List<PSModuleInfo> modules = this._context.Modules.GetModules(new string[] { moduleName }, false);
            if (modules != null)
            {
                if (modules.Count == 1)
                {
                    if (modules[0].ModuleType != ModuleType.Binary)
                    {
                        info = modules[0];
                    }
                    return info;
                }
                foreach (PSModuleInfo info2 in modules)
                {
                    if (info2.ModuleType != ModuleType.Binary)
                    {
                        return info2;
                    }
                }
            }
            return info;
        }

        private CommandInfo GetInfoFromPath(string path)
        {
            CommandInfo info = null;
            if (!File.Exists(path))
            {
                CommandDiscovery.discoveryTracer.TraceError("The path does not exist: {0}", new object[] { path });
            }
            else
            {
                string a = null;
                try
                {
                    a = Path.GetExtension(path);
                }
                catch (ArgumentException)
                {
                }
                if (a == null)
                {
                    info = null;
                }
                else if (string.Equals(a, ".ps1", StringComparison.OrdinalIgnoreCase))
                {
                    if ((this.commandTypes & CommandTypes.ExternalScript) != 0)
                    {
                        string fileName = Path.GetFileName(path);
                        CommandDiscovery.discoveryTracer.WriteLine("Command Found: path ({0}) is a script with name: {1}", new object[] { path, fileName });
                        info = new ExternalScriptInfo(fileName, path, this._context);
                    }
                }
                else if ((this.commandTypes & CommandTypes.Application) != 0)
                {
                    string name = Path.GetFileName(path);
                    CommandDiscovery.discoveryTracer.WriteLine("Command Found: path ({0}) is an application with name: {1}", new object[] { path, name });
                    info = new ApplicationInfo(name, path, this._context);
                }
            }
            if ((info != null) && ((((PSLanguageMode) info.DefiningLanguageMode) == PSLanguageMode.ConstrainedLanguage) && (this._context.LanguageMode == PSLanguageMode.FullLanguage)))
            {
                info = null;
            }
            return info;
        }

        private CommandInfo GetNextAlias()
        {
            CommandInfo current = null;
            if ((this.commandResolutionOptions & SearchResolutionOptions.ResolveAliasPatterns) != SearchResolutionOptions.None)
            {
                if (this.matchingAlias == null)
                {
                    Collection<AliasInfo> collection = new Collection<AliasInfo>();
                    WildcardPattern pattern = new WildcardPattern(this.commandName, WildcardOptions.IgnoreCase);
                    foreach (KeyValuePair<string, AliasInfo> pair in this._context.EngineSessionState.GetAliasTable())
                    {
                        if (pattern.IsMatch(pair.Key))
                        {
                            collection.Add(pair.Value);
                        }
                    }
                    AliasInfo aliasFromModules = this.GetAliasFromModules(this.commandName);
                    if (aliasFromModules != null)
                    {
                        collection.Add(aliasFromModules);
                    }
                    this.matchingAlias = collection.GetEnumerator();
                }
                if (!this.matchingAlias.MoveNext())
                {
                    this.currentState = SearchState.SearchingFunctions;
                    this.matchingAlias = null;
                }
                else
                {
                    current = this.matchingAlias.Current;
                }
            }
            else
            {
                this.currentState = SearchState.SearchingFunctions;
                current = this._context.EngineSessionState.GetAlias(this.commandName) ?? this.GetAliasFromModules(this.commandName);
            }
            if ((current != null) && ((((PSLanguageMode) current.DefiningLanguageMode) == PSLanguageMode.ConstrainedLanguage) && (this._context.LanguageMode == PSLanguageMode.FullLanguage)))
            {
                current = null;
            }
            if (current != null)
            {
                CommandDiscovery.discoveryTracer.WriteLine("Alias found: {0}  {1}", new object[] { current.Name, current.Definition });
            }
            return current;
        }

        private ScriptInfo GetNextBuiltinScript()
        {
            ScriptInfo scriptInfo = null;
            if ((this.commandResolutionOptions & SearchResolutionOptions.CommandNameIsPattern) != SearchResolutionOptions.None)
            {
                if (this.matchingScript == null)
                {
                    Collection<string> collection = new Collection<string>();
                    WildcardPattern pattern = new WildcardPattern(this.commandName, WildcardOptions.IgnoreCase);
                    WildcardPattern pattern2 = new WildcardPattern(this.commandName + ".ps1", WildcardOptions.IgnoreCase);
                    foreach (string str in this._context.CommandDiscovery.ScriptCache.Keys)
                    {
                        if (pattern.IsMatch(str) || pattern2.IsMatch(str))
                        {
                            collection.Add(str);
                        }
                    }
                    this.matchingScript = collection.GetEnumerator();
                }
                if (!this.matchingScript.MoveNext())
                {
                    this.currentState = SearchState.StartSearchingForExternalCommands;
                    this.matchingScript = null;
                }
                else
                {
                    scriptInfo = this._context.CommandDiscovery.GetScriptInfo(this.matchingScript.Current);
                }
            }
            else
            {
                this.currentState = SearchState.StartSearchingForExternalCommands;
                scriptInfo = this._context.CommandDiscovery.GetScriptInfo(this.commandName) ?? this._context.CommandDiscovery.GetScriptInfo(this.commandName + ".ps1");
            }
            if (scriptInfo != null)
            {
                CommandDiscovery.discoveryTracer.WriteLine("Script found: {0}", new object[] { scriptInfo.Name });
            }
            return scriptInfo;
        }

        private CmdletInfo GetNextCmdlet()
        {
            CmdletInfo result = null;
            if (this.matchingCmdlet == null)
            {
                if ((this.commandResolutionOptions & SearchResolutionOptions.CommandNameIsPattern) != SearchResolutionOptions.None)
                {
                    Collection<CmdletInfo> collection = new Collection<CmdletInfo>();
                    PSSnapinQualifiedName instance = PSSnapinQualifiedName.GetInstance(this.commandName);
                    if (instance == null)
                    {
                        return result;
                    }
                    WildcardPattern pattern = new WildcardPattern(instance.ShortName, WildcardOptions.IgnoreCase);
                    foreach (List<CmdletInfo> list in this._context.EngineSessionState.GetCmdletTable().Values)
                    {
                        foreach (CmdletInfo info2 in list)
                        {
                            if (pattern.IsMatch(info2.Name) && (string.IsNullOrEmpty(instance.PSSnapInName) || instance.PSSnapInName.Equals(info2.ModuleName, StringComparison.OrdinalIgnoreCase)))
                            {
                                collection.Add(info2);
                            }
                        }
                    }
                    this.matchingCmdlet = collection.GetEnumerator();
                }
                else
                {
                    this.matchingCmdlet = this._context.CommandDiscovery.GetCmdletInfo(this.commandName, (this.commandResolutionOptions & SearchResolutionOptions.SearchAllScopes) != SearchResolutionOptions.None);
                }
            }
            if (!this.matchingCmdlet.MoveNext())
            {
                this.currentState = SearchState.SearchingBuiltinScripts;
                this.matchingCmdlet = null;
            }
            else
            {
                result = this.matchingCmdlet.Current;
            }
            return traceResult(result);
        }

        private CommandInfo GetNextFromPath()
        {
            CommandInfo infoFromPath = null;
            CommandDiscovery.discoveryTracer.WriteLine("The name appears to be a qualified path: {0}", new object[] { this.commandName });
            CommandDiscovery.discoveryTracer.WriteLine("Trying to resolve the path as an PSPath", new object[0]);
            Collection<string> collection = new Collection<string>();
            try
            {
                CmdletProvider provider;
                ProviderInfo info2;
                collection = this._context.LocationGlobber.GetGlobbedProviderPathsFromMonadPath(this.commandName, false, out info2, out provider);
            }
            catch (ItemNotFoundException)
            {
                CommandDiscovery.discoveryTracer.TraceError("The path could not be found: {0}", new object[] { this.commandName });
            }
            catch (System.Management.Automation.DriveNotFoundException)
            {
                CommandDiscovery.discoveryTracer.TraceError("A drive could not be found for the path: {0}", new object[] { this.commandName });
            }
            catch (ProviderNotFoundException)
            {
                CommandDiscovery.discoveryTracer.TraceError("A provider could not be found for the path: {0}", new object[] { this.commandName });
            }
            catch (InvalidOperationException)
            {
                CommandDiscovery.discoveryTracer.TraceError("The path specified a home directory, but the provider home directory was not set. {0}", new object[] { this.commandName });
            }
            catch (ProviderInvocationException exception)
            {
                CommandDiscovery.discoveryTracer.TraceError("The provider associated with the path '{0}' encountered an error: {1}", new object[] { this.commandName, exception.Message });
            }
            catch (PSNotSupportedException)
            {
                CommandDiscovery.discoveryTracer.TraceError("The provider associated with the path '{0}' does not implement ContainerCmdletProvider", new object[] { this.commandName });
            }
            if (collection.Count > 1)
            {
                CommandDiscovery.discoveryTracer.TraceError("The path resolved to more than one result so this path cannot be used.", new object[0]);
                return infoFromPath;
            }
            if ((collection.Count == 1) && File.Exists(collection[0]))
            {
                string path = collection[0];
                CommandDiscovery.discoveryTracer.WriteLine("Path resolved to: {0}", new object[] { path });
                infoFromPath = this.GetInfoFromPath(path);
            }
            return infoFromPath;
        }

        private CommandInfo GetNextFunction()
        {
            CommandInfo current = null;
            if ((this.commandResolutionOptions & SearchResolutionOptions.ResolveFunctionPatterns) != SearchResolutionOptions.None)
            {
                if (this.matchingFunctionEnumerator == null)
                {
                    Collection<CommandInfo> collection = new Collection<CommandInfo>();
                    WildcardPattern pattern = new WildcardPattern(this.commandName, WildcardOptions.IgnoreCase);
                    foreach (DictionaryEntry entry in this._context.EngineSessionState.GetFunctionTable())
                    {
                        if (pattern.IsMatch((string) entry.Key))
                        {
                            collection.Add((CommandInfo) entry.Value);
                        }
                    }
                    CommandInfo functionFromModules = this.GetFunctionFromModules(this.commandName);
                    if (functionFromModules != null)
                    {
                        collection.Add(functionFromModules);
                    }
                    this.matchingFunctionEnumerator = collection.GetEnumerator();
                }
                if (!this.matchingFunctionEnumerator.MoveNext())
                {
                    this.currentState = SearchState.SearchingCmdlets;
                    this.matchingFunctionEnumerator = null;
                }
                else
                {
                    current = this.matchingFunctionEnumerator.Current;
                }
            }
            else
            {
                this.currentState = SearchState.SearchingCmdlets;
                current = this.GetFunction(this.commandName);
            }
            if ((current != null) && ((((PSLanguageMode) current.DefiningLanguageMode) == PSLanguageMode.ConstrainedLanguage) && (this._context.LanguageMode == PSLanguageMode.FullLanguage)))
            {
                current = null;
            }
            return current;
        }

        private static bool IsQualifiedPSPath(string commandName)
        {
            return (((LocationGlobber.IsAbsolutePath(commandName) || LocationGlobber.IsProviderQualifiedPath(commandName)) || LocationGlobber.IsHomePath(commandName)) || LocationGlobber.IsProviderDirectPath(commandName));
        }

        public bool MoveNext()
        {
            this._currentMatch = null;
            if (this.currentState == SearchState.SearchingAliases)
            {
                this._currentMatch = this.SearchForAliases();
                if ((this._currentMatch != null) && SessionState.IsVisible(this._commandOrigin, this._currentMatch))
                {
                    return true;
                }
                this._currentMatch = null;
                this.currentState = SearchState.SearchingFunctions;
            }
            if (this.currentState == SearchState.SearchingFunctions)
            {
                this._currentMatch = this.SearchForFunctions();
                if (this._currentMatch != null)
                {
                    return true;
                }
                this.currentState = SearchState.SearchingCmdlets;
            }
            if (this.currentState == SearchState.SearchingCmdlets)
            {
                this._currentMatch = this.SearchForCmdlets();
                if (this._currentMatch != null)
                {
                    return true;
                }
                this.currentState = SearchState.SearchingBuiltinScripts;
            }
            if (this.currentState == SearchState.SearchingBuiltinScripts)
            {
                this._currentMatch = this.SearchForBuiltinScripts();
                if (this._currentMatch != null)
                {
                    return true;
                }
                this.currentState = SearchState.StartSearchingForExternalCommands;
            }
            if (this.currentState == SearchState.StartSearchingForExternalCommands)
            {
                if ((this.commandTypes & (CommandTypes.Application | CommandTypes.ExternalScript)) == 0)
                {
                    return false;
                }
                if ((this._commandOrigin == System.Management.Automation.CommandOrigin.Runspace) && (this.commandName.IndexOfAny(_pathSeparators) >= 0))
                {
                    bool flag = false;
                    if (((this._context.EngineSessionState.Applications.Count == 1) && this._context.EngineSessionState.Applications[0].Equals("*", StringComparison.OrdinalIgnoreCase)) || ((this._context.EngineSessionState.Scripts.Count == 1) && this._context.EngineSessionState.Scripts[0].Equals("*", StringComparison.OrdinalIgnoreCase)))
                    {
                        flag = true;
                    }
                    else
                    {
                        foreach (string str in this._context.EngineSessionState.Applications)
                        {
                            if (checkPath(str, this.commandName))
                            {
                                flag = true;
                                break;
                            }
                        }
                        if (!flag)
                        {
                            foreach (string str2 in this._context.EngineSessionState.Scripts)
                            {
                                if (checkPath(str2, this.commandName))
                                {
                                    flag = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (!flag)
                    {
                        return false;
                    }
                }
                this.currentState = SearchState.PowerShellPathResolution;
                this._currentMatch = this.ProcessBuiltinScriptState();
                if (this._currentMatch != null)
                {
                    this.currentState = SearchState.QualifiedFileSystemPath;
                    return true;
                }
            }
            if (this.currentState == SearchState.PowerShellPathResolution)
            {
                this.currentState = SearchState.QualifiedFileSystemPath;
                this._currentMatch = this.ProcessPathResolutionState();
                if (this._currentMatch != null)
                {
                    return true;
                }
            }
            if ((this.currentState == SearchState.QualifiedFileSystemPath) || (this.currentState == SearchState.PathSearch))
            {
                this._currentMatch = this.ProcessQualifiedFileSystemState();
                if (this._currentMatch != null)
                {
                    return true;
                }
            }
            if (this.currentState == SearchState.PathSearch)
            {
                this.currentState = SearchState.PowerShellRelativePath;
                this._currentMatch = this.ProcessPathSearchState();
                if (this._currentMatch != null)
                {
                    return true;
                }
            }
            return false;
        }

        private CommandInfo ProcessBuiltinScriptState()
        {
            CommandInfo nextFromPath = null;
            if (((this._context.EngineSessionState != null) && (this._context.EngineSessionState.ProviderCount > 0)) && IsQualifiedPSPath(this.commandName))
            {
                nextFromPath = this.GetNextFromPath();
            }
            return nextFromPath;
        }

        private CommandInfo ProcessPathResolutionState()
        {
            CommandInfo infoFromPath = null;
            try
            {
                if (string.IsNullOrEmpty(this.commandName)) return infoFromPath;
                if (!Path.IsPathRooted(this.commandName.Replace("\"", "")) || !File.Exists(this.commandName.Replace("\"", "")))
                {
                    return infoFromPath;
                }
                try
                {
                    infoFromPath = this.GetInfoFromPath(this.commandName);
                }
                catch (FileLoadException)
                {
                }
                catch (FormatException)
                {
                }
                catch (MetadataException)
                {
                }
            }
            catch (ArgumentException)
            {
            }
            return infoFromPath;
        }

        private CommandInfo ProcessPathSearchState()
        {
            CommandInfo infoFromPath = null;
            string str = this.DoPowerShellRelativePathLookup();
            if (!string.IsNullOrEmpty(str))
            {
                infoFromPath = this.GetInfoFromPath(str);
            }
            return infoFromPath;
        }

        private CommandInfo ProcessQualifiedFileSystemState()
        {
            try
            {
                this.setupPathSearcher();
            }
            catch (ArgumentException)
            {
                this.currentState = SearchState.NoMoreMatches;
                throw;
            }
            catch (PathTooLongException)
            {
                this.currentState = SearchState.NoMoreMatches;
                throw;
            }
            CommandInfo infoFromPath = null;
            this.currentState = SearchState.PathSearch;
            if (this.canDoPathLookup)
            {
                try
                {
                    while ((infoFromPath == null) && this.pathSearcher.MoveNext())
                    {
                        infoFromPath = this.GetInfoFromPath(this.pathSearcher.Current);
                    }
                }
                catch (InvalidOperationException)
                {
                }
            }
            return infoFromPath;
        }

        public void Reset()
        {
            if (this._commandOrigin == System.Management.Automation.CommandOrigin.Runspace)
            {
                if (this._context.EngineSessionState.Applications.Count == 0)
                {
                    this.commandTypes &= ~CommandTypes.Application;
                }
                if (this._context.EngineSessionState.Scripts.Count == 0)
                {
                    this.commandTypes &= ~CommandTypes.ExternalScript;
                }
            }
            if (this.pathSearcher != null)
            {
                this.pathSearcher.Reset();
            }
            this._currentMatch = null;
            this.currentState = SearchState.SearchingAliases;
            this.matchingAlias = null;
            this.matchingCmdlet = null;
            this.matchingScript = null;
        }

        private string ResolvePSPath(string path)
        {
            string str = null;
            try
            {
                ProviderInfo info;
                string providerPath = this._context.LocationGlobber.GetProviderPath(path, out info);
                if (info.NameEquals(this._context.ProviderNames.FileSystem))
                {
                    str = providerPath;
                    CommandDiscovery.discoveryTracer.WriteLine("The relative path was resolved to: {0}", new object[] { str });
                    return str;
                }
                CommandDiscovery.discoveryTracer.TraceError("The relative path was not a file system path. {0}", new object[] { path });
            }
            catch (InvalidOperationException)
            {
                CommandDiscovery.discoveryTracer.TraceError("The home path was not specified for the provider. {0}", new object[] { path });
            }
            catch (ProviderInvocationException exception)
            {
                CommandDiscovery.discoveryTracer.TraceError("While resolving the path, \"{0}\", an error was encountered by the provider: {1}", new object[] { path, exception.Message });
            }
            catch (ItemNotFoundException)
            {
                CommandDiscovery.discoveryTracer.TraceError("The path does not exist: {0}", new object[] { path });
            }
            catch (System.Management.Automation.DriveNotFoundException exception2)
            {
                CommandDiscovery.discoveryTracer.TraceError("The drive does not exist: {0}", new object[] { exception2.ItemName });
            }
            return str;
        }

        private CommandInfo SearchForAliases()
        {
            CommandInfo nextAlias = null;
            if ((this._context.EngineSessionState != null) && ((this.commandTypes & CommandTypes.Alias) != 0))
            {
                nextAlias = this.GetNextAlias();
            }
            return nextAlias;
        }

        private CommandInfo SearchForBuiltinScripts()
        {
            CommandInfo nextBuiltinScript = null;
            if ((this.commandTypes & CommandTypes.Script) != 0)
            {
                nextBuiltinScript = this.GetNextBuiltinScript();
            }
            return nextBuiltinScript;
        }

        private CommandInfo SearchForCmdlets()
        {
            CommandInfo nextCmdlet = null;
            if ((this.commandTypes & CommandTypes.Cmdlet) != 0)
            {
                nextCmdlet = this.GetNextCmdlet();
            }
            return nextCmdlet;
        }

        private CommandInfo SearchForFunctions()
        {
            CommandInfo nextFunction = null;
            if ((this._context.EngineSessionState != null) && ((this.commandTypes & (CommandTypes.Workflow | CommandTypes.Filter | CommandTypes.Function)) != 0))
            {
                nextFunction = this.GetNextFunction();
            }
            return nextFunction;
        }

        private void setupPathSearcher()
        {
            if (this.pathSearcher == null)
            {
                HashSet<string> allowedExtensions = new HashSet<string>();
                allowedExtensions = this._context.CommandDiscovery.GetAllowedExtensionsFromPathExt();
                allowedExtensions.Add(".ps1");
                if ((this.commandResolutionOptions & SearchResolutionOptions.CommandNameIsPattern) != SearchResolutionOptions.None)
                {
                    this.canDoPathLookup = true;
                    this.canDoPathLookupResult = CanDoPathLookupResult.Yes;
                    Collection<string> patterns = new Collection<string> {
                        this.commandName
                    };
                    this.pathSearcher = new CommandPathSearch(patterns, this._context.CommandDiscovery.GetLookupDirectoryPaths(), allowedExtensions, false, this._context);
                }
                else
                {
                    this.canDoPathLookupResult = CanDoPathLookup(this.commandName);
                    if (this.canDoPathLookupResult == CanDoPathLookupResult.Yes)
                    {
                        this.canDoPathLookup = true;
                        this.pathSearcher = new CommandPathSearch(this.ConstructSearchPatternsFromName(this.commandName), this._context.CommandDiscovery.GetLookupDirectoryPaths(), allowedExtensions, false, this._context);
                    }
                    else if (this.canDoPathLookupResult == CanDoPathLookupResult.PathIsRooted)
                    {
                        this.canDoPathLookup = true;
                        string directoryName = Path.GetDirectoryName(this.commandName);
                        Collection<string> lookupPaths = new Collection<string> {
                            directoryName
                        };
                        CommandDiscovery.discoveryTracer.WriteLine("The path is rooted, so only doing the lookup in the specified directory: {0}", new object[] { directoryName });
                        string fileName = Path.GetFileName(this.commandName);
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            this.pathSearcher = new CommandPathSearch(this.ConstructSearchPatternsFromName(fileName), lookupPaths, allowedExtensions, false, this._context);
                        }
                        else
                        {
                            this.canDoPathLookup = false;
                        }
                    }
                    else if (this.canDoPathLookupResult == CanDoPathLookupResult.DirectorySeparator)
                    {
                        this.canDoPathLookup = true;
                        string path = Path.GetDirectoryName(this.commandName);
                        path = this.ResolvePSPath(path);
                        CommandDiscovery.discoveryTracer.WriteLine("The path is relative, so only doing the lookup in the specified directory: {0}", new object[] { path });
                        if (path == null)
                        {
                            this.canDoPathLookup = false;
                        }
                        else
                        {
                            Collection<string> collection5 = new Collection<string> {
                                path
                            };
                            string str4 = Path.GetFileName(this.commandName);
                            if (!string.IsNullOrEmpty(str4))
                            {
                                this.pathSearcher = new CommandPathSearch(this.ConstructSearchPatternsFromName(str4), collection5, allowedExtensions, false, this._context);
                            }
                            else
                            {
                                this.canDoPathLookup = false;
                            }
                        }
                    }
                }
            }
        }

        IEnumerator<CommandInfo> IEnumerable<CommandInfo>.GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        private static CmdletInfo traceResult(CmdletInfo result)
        {
            if (result != null)
            {
                CommandDiscovery.discoveryTracer.WriteLine("Cmdlet found: {0}  {1}", new object[] { result.Name, result.ImplementingType });
            }
            return result;
        }

        internal System.Management.Automation.CommandOrigin CommandOrigin
        {
            get
            {
                return this._commandOrigin;
            }
            set
            {
                this._commandOrigin = value;
            }
        }

        CommandInfo IEnumerator<CommandInfo>.Current
        {
            get
            {
                if (((this.currentState == SearchState.SearchingAliases) && (this._currentMatch == null)) || ((this.currentState == SearchState.NoMoreMatches) || (this._currentMatch == null)))
                {
                    throw PSTraceSource.NewInvalidOperationException();
                }
                return this._currentMatch;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return this.Current;
            }
        }

        public CommandInfo Current
        {
            get { return this._currentMatch; } 
        }


        private enum CanDoPathLookupResult
        {
            Yes,
            PathIsRooted,
            WildcardCharacters,
            DirectorySeparator,
            IllegalCharacters
        }

        private enum SearchState
        {
            SearchingAliases,
            SearchingFunctions,
            SearchingCmdlets,
            SearchingBuiltinScripts,
            StartSearchingForExternalCommands,
            PowerShellPathResolution,
            QualifiedFileSystemPath,
            PathSearch,
            GetPathSearch,
            PowerShellRelativePath,
            NoMoreMatches
        }
    }
}

