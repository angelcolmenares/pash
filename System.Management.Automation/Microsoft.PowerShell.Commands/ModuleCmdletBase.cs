namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell;
    using Microsoft.PowerShell.Cmdletization;
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Security;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Xml;

    public class ModuleCmdletBase : PSCmdlet
    {
        private bool _addToAppDomainLevelCache;
        private List<WildcardPattern> _aliasPatterns;
        private object[] _arguments;
        private bool _baseAsCustomObject;
        private List<WildcardPattern> _cmdletPatterns;
        private bool _disableNameChecking = true;
        private bool _force;
        private List<WildcardPattern> _functionPatterns;
        private bool _global;
        private List<WildcardPattern> _matchAll;
        private Version _minimumVersion;
        private bool _passThru;
        private string _prefix = string.Empty;
        private Version _requiredVersion;
        private List<WildcardPattern> _variablePatterns;
        private static Dictionary<string, List<string>> binaryAnalysisCache = new Dictionary<string, List<string>>();
        private Dictionary<string, PSModuleInfo> currentlyProcessingModules = new Dictionary<string, PSModuleInfo>();
        internal static string[] ModuleManifestMembers = new string[] { 
            "ModuleToProcess", "NestedModules", "GUID", "Author", "CompanyName", "Copyright", "ModuleVersion", "Description", "PowerShellVersion", "PowerShellHostName", "PowerShellHostVersion", "CLRVersion", "DotNetFrameworkVersion", "ProcessorArchitecture", "RequiredModules", "TypesToProcess", 
            "FormatsToProcess", "ScriptsToProcess", "PrivateData", "RequiredAssemblies", "ModuleList", "FileList", "FunctionsToExport", "VariablesToExport", "AliasesToExport", "CmdletsToExport", "HelpInfoURI", "RootModule", "DefaultCommandPrefix"
         };
        private static string[] ModuleVersionMembers = new string[] { "ModuleName", "GUID", "ModuleVersion" };
        private static string[] PermittedCmdlets = new string[] { "Import-LocalizedData", "ConvertFrom-StringData", "Write-Host", "Out-Host", "Join-Path" };
        private static Dictionary<string, PSModuleInfo> scriptAnalysisCache = new Dictionary<string, PSModuleInfo>();
        private static List<string> ServiceCoreAssemblyCmdlets = new List<string>(new string[] { @"Microsoft.PowerShell.Workflow.ServiceCore\Import-PSWorkflow", @"Microsoft.PowerShell.Workflow.ServiceCore\New-PSWorkflowExecutionOption" });
        private readonly string ServiceCoreAssemblyFullName = "Microsoft.Powershell.Workflow.ServiceCore, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, processorArchitecture=MSIL";
        private readonly string ServiceCoreAssemblyShortName = "Microsoft.Powershell.Workflow.ServiceCore";

        internal static void AddModuleToModuleTables(ExecutionContext context, SessionStateInternal targetSessionState, PSModuleInfo module)
        {
            if (!context.Modules.ModuleTable.ContainsKey(module.Path))
            {
                context.Modules.ModuleTable.Add(module.Path, module);
            }
            if (!targetSessionState.ModuleTable.ContainsKey(module.Path))
            {
                targetSessionState.ModuleTable.Add(module.Path, module);
                targetSessionState.ModuleTableKeys.Add(module.Path);
            }
            if (targetSessionState.Module != null)
            {
                targetSessionState.Module.AddNestedModule(module);
            }
        }

        internal static string AddPrefixToCommandName(string commandName, string prefix)
        {
            if (!string.IsNullOrEmpty(prefix))
            {
                string str;
                string str2;
                if (CmdletInfo.SplitCmdletName(commandName, out str, out str2))
                {
                    commandName = str + "-" + prefix + str2;
                    return commandName;
                }
                commandName = prefix + commandName;
            }
            return commandName;
        }

        private PSModuleInfo AnalyzeScriptFile(string filename, bool force, ExecutionContext context)
        {
            if (scriptAnalysisCache.ContainsKey(filename))
            {
                return scriptAnalysisCache[filename].Clone();
            }
            PSModuleInfo module = new PSModuleInfo(filename, null, null);
            if (!force)
            {
                Dictionary<string, List<CommandTypes>> dictionary = AnalysisCache.GetExportedCommands(filename, true, context);
                if (dictionary != null)
                {
                    foreach (string str in dictionary.Keys)
                    {
                        foreach (CommandTypes types in dictionary[str])
                        {
                            if ((types & CommandTypes.Alias) == CommandTypes.Alias)
                            {
                                module.AddDetectedAliasExport(str, null);
                            }
                            else if ((types & CommandTypes.Workflow) == CommandTypes.Workflow)
                            {
                                module.AddDetectedWorkflowExport(str);
                            }
                            else if ((types & CommandTypes.Function) == CommandTypes.Function)
                            {
                                module.AddDetectedFunctionExport(str);
                            }
                            else if ((types & CommandTypes.Cmdlet) == CommandTypes.Cmdlet)
                            {
                                module.AddDetectedCmdletExport(str);
                            }
                            else
                            {
                                module.AddDetectedFunctionExport(str);
                            }
                        }
                    }
                    scriptAnalysisCache[filename] = module;
                    return module;
                }
            }
            ScriptAnalysis analysis = new ScriptAnalysis(filename);
            List<WildcardPattern> patterns = new List<WildcardPattern>();
            foreach (string str2 in analysis.DiscoveredCommandFilters)
            {
                patterns.Add(new WildcardPattern(str2));
            }
            foreach (string str3 in analysis.DiscoveredExports)
            {
                if (SessionStateUtilities.MatchesAnyWildcardPattern(str3, patterns, true) && !HasInvalidCharacters(str3.Replace("-", "")))
                {
                    module.AddDetectedFunctionExport(str3);
                }
            }
            foreach (string str4 in analysis.DiscoveredAliases.Keys)
            {
                if (!HasInvalidCharacters(str4.Replace("-", "")))
                {
                    module.AddDetectedAliasExport(str4, analysis.DiscoveredAliases[str4]);
                }
            }
            if (analysis.AddsSelfToPath)
            {
                string directoryName = Path.GetDirectoryName(filename);
                try
                {
                    foreach (string str6 in Directory.GetFiles(directoryName, "*.ps1"))
                    {
                        module.AddDetectedFunctionExport(Path.GetFileNameWithoutExtension(str6));
                    }
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
            foreach (RequiredModuleInfo info2 in analysis.DiscoveredModules)
            {
                string name = info2.Name;
                List<PSModuleInfo> list2 = new List<PSModuleInfo>();
                if (Path.HasExtension(name) && !Path.IsPathRooted(name))
                {
                    name = Path.Combine(Path.GetDirectoryName(filename), name);
                    PSModuleInfo item = this.CreateModuleInfoForGetModule(name, true);
                    if (item != null)
                    {
                        list2.Add(item);
                    }
                }
                else
                {
                    list2.AddRange(this.GetModule(new string[] { name }, false, true));
                }
                if ((list2 != null) && (list2.Count != 0))
                {
                    List<WildcardPattern> list3 = new List<WildcardPattern>();
                    foreach (string str9 in info2.CommandsToPostFilter)
                    {
                        list3.Add(new WildcardPattern(str9));
                    }
                    foreach (PSModuleInfo info4 in list2)
                    {
                        foreach (string str10 in info4.ExportedFunctions.Keys)
                        {
                            if ((SessionStateUtilities.MatchesAnyWildcardPattern(str10, list3, true) && SessionStateUtilities.MatchesAnyWildcardPattern(str10, patterns, true)) && !HasInvalidCharacters(str10.Replace("-", "")))
                            {
                                module.AddDetectedFunctionExport(str10);
                            }
                        }
                        foreach (string str11 in info4.ExportedCmdlets.Keys)
                        {
                            if ((SessionStateUtilities.MatchesAnyWildcardPattern(str11, list3, true) && SessionStateUtilities.MatchesAnyWildcardPattern(str11, patterns, true)) && !HasInvalidCharacters(str11.Replace("-", "")))
                            {
                                module.AddDetectedCmdletExport(str11);
                            }
                        }
                        foreach (string str12 in info4.ExportedAliases.Keys)
                        {
                            if (SessionStateUtilities.MatchesAnyWildcardPattern(str12, list3, true) && SessionStateUtilities.MatchesAnyWildcardPattern(str12, patterns, true))
                            {
                                module.AddDetectedAliasExport(str12, info4.ExportedAliases[str12].Definition);
                            }
                        }
                    }
                }
            }
            if (!module.HadErrorsLoading)
            {
                AnalysisCache.CacheExportedCommands(module, true, context);
            }
            else
            {
                ModuleIntrinsics.Tracer.WriteLine("Caching skipped for " + module.Name + " because it had errors while loading.", new object[0]);
            }
            scriptAnalysisCache[filename] = module;
            return module;
        }

        private static void AnalyzeSnapinDomainHelper()
        {
            string data = (string) AppDomain.CurrentDomain.GetData("PathToProcess");
            bool isModuleLoad = (bool) AppDomain.CurrentDomain.GetData("IsModuleLoad");
            Dictionary<string, SessionStateCmdletEntry> cmdlets = null;
            Dictionary<string, SessionStateProviderEntry> providers = null;
            string helpFile = null;
            try
            {
                Assembly assembly = null;
                try
                {
                    if (Path.IsPathRooted(data))
                    {
						Exception error = null;
						assembly = InitialSessionState.LoadAssemblyFromFile (data);
                    }
                    else
                    {
                        Exception error = null;
                        assembly = ExecutionContext.LoadAssembly(data, null, out error);
                    }
                }
                catch (Exception exception2)
                {
                    CommandProcessorBase.CheckForSevereException(exception2);
                }
                if (assembly != null)
                {
                    PSSnapInHelpers.AnalyzePSSnapInAssembly(assembly, assembly.Location, null, null, isModuleLoad, out cmdlets, out providers, out helpFile);
                }
            }
            catch (Exception exception3)
            {
                CommandProcessorBase.CheckForSevereException(exception3);
            }
            List<string> list = new List<string>();
            if (cmdlets != null)
            {
                foreach (SessionStateCmdletEntry entry in cmdlets.Values)
                {
                    list.Add(entry.Name);
                }
            }
            AppDomain.CurrentDomain.SetData("DetectedCmdlets", list);
        }

        private void ClearAnalysisCaches()
        {
            binaryAnalysisCache = new Dictionary<string, List<string>>();
            scriptAnalysisCache = new Dictionary<string, PSModuleInfo>();
        }

        internal static bool CommandFound(string commandName, SessionStateInternal sessionStateInternal)
        {
            bool flag;
            EventHandler<CommandLookupEventArgs> commandNotFoundAction = sessionStateInternal.ExecutionContext.EngineIntrinsics.InvokeCommand.CommandNotFoundAction;
            try
            {
                sessionStateInternal.ExecutionContext.EngineIntrinsics.InvokeCommand.CommandNotFoundAction = null;
                CommandSearcher searcher = new CommandSearcher(commandName, SearchResolutionOptions.CommandNameIsPattern | SearchResolutionOptions.ResolveFunctionPatterns | SearchResolutionOptions.ResolveAliasPatterns, CommandTypes.Cmdlet | CommandTypes.Function | CommandTypes.Alias, sessionStateInternal.ExecutionContext);
                if (!searcher.MoveNext())
                {
                    return false;
                }
                flag = true;
            }
            finally
            {
                sessionStateInternal.ExecutionContext.EngineIntrinsics.InvokeCommand.CommandNotFoundAction = commandNotFoundAction;
            }
            return flag;
        }

        private PSModuleInfo CreateModuleInfoForGetModule(string file, bool refresh)
        {
            if (this.currentlyProcessingModules.ContainsKey(file))
            {
                return this.currentlyProcessingModules[file];
            }
            this.currentlyProcessingModules[file] = null;
            PSModuleInfo moduleInfo = new PSModuleInfo(file, null, null);
            string extension = Path.GetExtension(file);
            ManifestProcessingFlags nullOnFirstError = ManifestProcessingFlags.NullOnFirstError;
            if (refresh)
            {
                nullOnFirstError |= ManifestProcessingFlags.Force;
            }
            try
            {
                if (extension.Equals(".psd1", StringComparison.OrdinalIgnoreCase))
                {
                    string str2;
                    moduleInfo.SetModuleType(ModuleType.Manifest);
                    ExternalScriptInfo scriptInfo = this.GetScriptInfoForFile(file, out str2, true);
                    moduleInfo = this.LoadModuleManifest(scriptInfo, nullOnFirstError, null, null);
                }
                else
                {
                    ImportModuleOptions options = new ImportModuleOptions();
                    bool found = false;
                    moduleInfo = this.LoadModule(file, null, string.Empty, null, ref options, nullOnFirstError, out found);
                }
                if (moduleInfo == null)
                {
                    moduleInfo = new PSModuleInfo(file, null, null) {
                        HadErrorsLoading = true
                    };
                }
                if (extension.Equals(".psd1", StringComparison.OrdinalIgnoreCase))
                {
                    if (moduleInfo.RootModuleForManifest != null)
                    {
                        if (moduleInfo.RootModuleForManifest.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                        {
                            moduleInfo.SetModuleType(ModuleType.Binary);
                        }
                        else if (moduleInfo.RootModuleForManifest.EndsWith(".psm1", StringComparison.OrdinalIgnoreCase))
                        {
                            moduleInfo.SetModuleType(ModuleType.Script);
                        }
                        else if (moduleInfo.RootModuleForManifest.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase))
                        {
                            moduleInfo.SetModuleType(ModuleType.Workflow);
                        }
                        else if (moduleInfo.RootModuleForManifest.EndsWith(".cdxml", StringComparison.OrdinalIgnoreCase))
                        {
                            moduleInfo.SetModuleType(ModuleType.Cim);
                        }
                        else
                        {
                            moduleInfo.SetModuleType(ModuleType.Manifest);
                        }
                    }
                    else
                    {
                        moduleInfo.SetModuleType(ModuleType.Manifest);
                    }
                    moduleInfo.RootModule = moduleInfo.RootModuleForManifest;
                }
                else if (extension.Equals(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    moduleInfo.SetModuleType(ModuleType.Binary);
                }
                else if (extension.Equals(".xaml", StringComparison.OrdinalIgnoreCase))
                {
                    moduleInfo.SetModuleType(ModuleType.Workflow);
                }
                else if (extension.Equals(".cdxml"))
                {
                    string str3;
                    moduleInfo.SetModuleType(ModuleType.Cim);
                    StringReader cmdletizationXmlReader = new StringReader(this.GetScriptInfoForFile(file, out str3, true).ScriptContents);
                    new ScriptWriter(cmdletizationXmlReader, str3, "Microsoft.PowerShell.Cmdletization.Cim.CimCmdletAdapter, Microsoft.PowerShell.Commands.Management, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", base.MyInvocation, ScriptWriter.GenerationOptions.HelpXml).PopulatePSModuleInfo(moduleInfo);
                }
                else
                {
                    moduleInfo.SetModuleType(ModuleType.Script);
                }
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                if (moduleInfo == null)
                {
                    moduleInfo = new PSModuleInfo(file, null, null);
                }
            }
            this.currentlyProcessingModules[file] = moduleInfo;
            return moduleInfo;
        }

        private ExternalScriptInfo FindLocalizedModuleManifest(string path)
        {
            string directoryName = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);
            string str3 = null;
            for (CultureInfo info2 = CultureInfo.CurrentUICulture; (info2 != null) && !string.IsNullOrEmpty(info2.Name); info2 = info2.Parent)
            {
                StringBuilder builder = new StringBuilder(directoryName);
                builder.Append(@"\");
                builder.Append(info2.Name);
                builder.Append(@"\");
                builder.Append(fileName);
                string str4 = builder.ToString();
                if (File.Exists(str4))
                {
                    str3 = str4;
                    break;
                }
            }
            ExternalScriptInfo info3 = null;
            if (str3 != null)
            {
                info3 = new ExternalScriptInfo(Path.GetFileName(str3), str3);
            }
            return info3;
        }

        internal string FixupFileName(string moduleBase, string name, string extension)
        {
			string str = ResolveRootedFilePath(name, base.Context);
            if (string.IsNullOrEmpty(str))
            {
                str = Path.Combine(moduleBase, name);
            }
            string str2 = ResolveRootedFilePath(str, base.Context);
            string str3 = Path.GetExtension(name);
            string location = !string.IsNullOrEmpty(str2) ? str2 : str;
            if (string.IsNullOrEmpty(str3))
            {
                location = location + extension;
            }
			/*
            if (!string.IsNullOrEmpty(str3) && str3.Equals(".dll", StringComparison.OrdinalIgnoreCase))
            {
                Exception error = null;
				Assembly assembly = File.Exists (str2) ? ExecutionContext.LoadAssembly(null, str2, out error) : ExecutionContext.LoadAssembly(name, null, out error);
                if (assembly != null)
                {
                    location = assembly.Location;
                }
            }
            */
            return location;
        }

        private static ErrorRecord GenerateInvalidModuleMemberErrorRecord(string manifestElement, string moduleManifestPath, Exception e)
        {
            return new ErrorRecord(new ArgumentException(StringUtil.Format(Modules.ModuleManifestInvalidManifestMember, new object[] { manifestElement, e.Message, moduleManifestPath })), "Modules_InvalidManifest", ErrorCategory.ResourceUnavailable, moduleManifestPath);
        }

        private void GetAllAvailableModules(string directory, List<PSModuleInfo> availableModules, bool refresh)
        {
            List<string> availableModuleFiles = new List<string>();
            ModuleUtils.GetAllAvailableModuleFiles(directory, availableModuleFiles);
            foreach (string str in availableModuleFiles)
            {
                PSModuleInfo item = this.CreateModuleInfoForGetModule(str, refresh);
                if (item != null)
                {
                    availableModules.Add(item);
                }
            }
        }

        private static Version GetAssemblyVersionNumber(Assembly assemblyToLoad)
        {
            try
            {
                AssemblyName name = new AssemblyName(assemblyToLoad.FullName);
                return name.Version;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                return new Version();
            }
        }

        private Dictionary<string, List<PSModuleInfo>> GetAvailableLocallyModulesCore(string[] name, bool all, bool refresh)
        {
            Dictionary<string, List<PSModuleInfo>> dictionary = new Dictionary<string, List<PSModuleInfo>>();
            List<string> modulePath = (List<string>) ModuleIntrinsics.GetModulePath(false, base.Context);
            foreach (string str in ModuleIntrinsics.GetModulePath(false, base.Context))
            {
                try
                {
                    List<PSModuleInfo> availableModules = new List<PSModuleInfo>();
                    if (all)
                    {
                        this.GetAllAvailableModules(str, availableModules, refresh);
                    }
                    else
                    {
                        this.GetDefaultAvailableModules(name, str, availableModules, modulePath, refresh);
                    }
                    if (!dictionary.ContainsKey(str))
                    {
                        dictionary.Add(str, (from m in availableModules
                            orderby m.Name
                            select m).ToList<PSModuleInfo>());
                    }
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
            return dictionary;
        }

        private static List<string> GetCmdletsFromBinaryModuleImplementation(string path, ManifestProcessingFlags manifestProcessingFlags, bool reprocess)
        {
            if (binaryAnalysisCache.ContainsKey(path))
            {
                return binaryAnalysisCache[path];
            }
            AppDomain domain = AppDomain.CreateDomain("ReflectionDomain");
            domain.SetData("PathToProcess", path);
            domain.SetData("IsModuleLoad", (manifestProcessingFlags & ManifestProcessingFlags.LoadElements) == ManifestProcessingFlags.LoadElements);
            domain.DoCallBack(new CrossAppDomainDelegate(ModuleCmdletBase.AnalyzeSnapinDomainHelper));
            List<string> data = (List<string>) domain.GetData("DetectedCmdlets");
            AppDomain.Unload(domain);
            if ((data.Count == 0) && Path.IsPathRooted(path) && !reprocess)
            {
                data = GetCmdletsFromBinaryModuleImplementation(path, manifestProcessingFlags, true);
            }
            binaryAnalysisCache[path] = data;
            return data;
        }

        private void GetDefaultAvailableModules(string[] name, string directory, List<PSModuleInfo> availableModules, List<string> modulePaths, bool refresh)
        {
            List<string> availableModuleFiles = new List<string>();
            IEnumerable<WildcardPattern> patterns = SessionStateUtilities.CreateWildcardsFromStrings(name, WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase);
            ModuleUtils.GetDefaultAvailableModuleFiles(directory, availableModuleFiles, modulePaths);
            foreach (string str in availableModuleFiles)
            {
                if (SessionStateUtilities.MatchesAnyWildcardPattern(Path.GetFileNameWithoutExtension(str), patterns, true))
                {
                    PSModuleInfo module = this.CreateModuleInfoForGetModule(str, refresh);
                    if (module != null)
                    {
                        if (!module.HadErrorsLoading)
                        {
                            AnalysisCache.CacheExportedCommands(module, refresh, base.Context);
                        }
                        else
                        {
                            ModuleIntrinsics.Tracer.WriteLine("Caching skipped for " + module.Name + " because it had errors while loading.", new object[0]);
                        }
                        availableModules.Add(module);
                    }
                }
            }
            this.ClearAnalysisCaches();
        }

        private string GetDefaultPrefix(PSModuleInfo module)
        {
            string str = string.Empty;
            string extension = Path.GetExtension(module.Path);
            if (!string.IsNullOrEmpty(extension) && extension.Equals(".psd1", StringComparison.OrdinalIgnoreCase))
            {
                string str3;
                ExternalScriptInfo scriptInfo = this.GetScriptInfoForFile(module.Path, out str3, true);
                bool containedErrors = false;
                Hashtable data = null;
                Hashtable localizedData = null;
                if (!this.LoadModuleManifestData(scriptInfo, ManifestProcessingFlags.NullOnFirstError, out data, out localizedData, ref containedErrors) || !data.Contains("DefaultCommandPrefix"))
                {
                    return str;
                }
                if ((localizedData != null) && localizedData.Contains("DefaultCommandPrefix"))
                {
                    str = (string) LanguagePrimitives.ConvertTo(localizedData["DefaultCommandPrefix"], typeof(string), CultureInfo.InvariantCulture);
                }
                if (string.IsNullOrEmpty(str))
                {
                    str = (string) LanguagePrimitives.ConvertTo(data["DefaultCommandPrefix"], typeof(string), CultureInfo.InvariantCulture);
                }
            }
            return str;
        }

        private ErrorRecord GetErrorRecordIfUnsupportedRootCdxmlAndNestedModuleScenario(Hashtable data, string moduleManifestPath, string rootModulePath)
        {
            if (rootModulePath == null)
            {
                return null;
            }
            if (!rootModulePath.EndsWith(".cdxml", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            if (!data.ContainsKey("NestedModules"))
            {
                return null;
            }
            string str = data.ContainsKey("ModuleToProcess") ? "ModuleToProcess" : "RootModule";
            return new ErrorRecord(new InvalidOperationException(StringUtil.Format(Modules.CmdletizationDoesSupportRexportingNestedModules, new object[] { str, moduleManifestPath, rootModulePath })), "Modules_CmdletizationDoesSupportRexportingNestedModules", ErrorCategory.InvalidOperation, moduleManifestPath);
        }

        private bool GetListOfFilesFromData(Hashtable data, string moduleManifestPath, string key, ManifestProcessingFlags manifestProcessingFlags, string moduleBase, string extension, bool verifyFilesExist, out List<string> list)
        {
            List<string> list2;
            list = null;
            if (!this.GetListOfStringsFromData(data, moduleManifestPath, key, manifestProcessingFlags, out list2))
            {
                return false;
            }
            if (list2 != null)
            {
                list = new List<string>();
                foreach (string str in list2)
                {
                    try
                    {
                        string path = this.FixupFileName(moduleBase, str, extension);
                        if (verifyFilesExist && !File.Exists(path))
                        {
                            throw new FileNotFoundException(StringUtil.Format(SessionStateStrings.PathNotFound, path), path);
                        }
                        list.Add(path);
                    }
                    catch (Exception exception)
                    {
                        CommandProcessorBase.CheckForSevereException(exception);
                        if ((manifestProcessingFlags & ManifestProcessingFlags.WriteErrors) != 0)
                        {
                            base.ThrowTerminatingError(GenerateInvalidModuleMemberErrorRecord(key, moduleManifestPath, exception));
                        }
                        list = null;
                        WriteInvalidManifestMemberError(this, key, moduleManifestPath, exception, manifestProcessingFlags);
                        return false;
                    }
                }
            }
            return true;
        }

        internal bool GetListOfStringsFromData(Hashtable data, string moduleManifestPath, string key, ManifestProcessingFlags manifestProcessingFlags, out List<string> list)
        {
            list = null;
            if (data.Contains(key) && (data[key] != null))
            {
                try
                {
                    string[] collection = (string[]) LanguagePrimitives.ConvertTo(data[key], typeof(string[]), CultureInfo.InvariantCulture);
                    list = new List<string>(collection);
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                    WriteInvalidManifestMemberError(this, key, moduleManifestPath, exception, manifestProcessingFlags);
                    return false;
                }
            }
            return true;
        }

        private bool GetListOfWildcardsFromData(Hashtable data, string moduleManifestPath, string key, ManifestProcessingFlags manifestProcessingFlags, out List<WildcardPattern> list)
        {
            List<string> list2;
            list = null;
            if (!this.GetListOfStringsFromData(data, moduleManifestPath, key, manifestProcessingFlags, out list2))
            {
                return false;
            }
            if (list2 != null)
            {
                list = new List<WildcardPattern>();
                foreach (string str in list2)
                {
                    if (!string.IsNullOrEmpty(str))
                    {
                        try
                        {
                            list.Add(new WildcardPattern(str, WildcardOptions.IgnoreCase));
                        }
                        catch (Exception exception)
                        {
                            CommandProcessorBase.CheckForSevereException(exception);
                            list = null;
                            WriteInvalidManifestMemberError(this, key, moduleManifestPath, exception, manifestProcessingFlags);
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        internal List<PSModuleInfo> GetModule(string[] name, bool all, bool refresh)
        {
            List<PSModuleInfo> list = new List<PSModuleInfo>();
            IEnumerable<WildcardPattern> patterns = SessionStateUtilities.CreateWildcardsFromStrings(name, WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase);
            foreach (KeyValuePair<string, List<PSModuleInfo>> pair in this.GetAvailableLocallyModulesCore(name, all, refresh))
            {
                foreach (PSModuleInfo info in pair.Value)
                {
                    if (SessionStateUtilities.MatchesAnyWildcardPattern(info.Name, patterns, true))
                    {
                        list.Add(info);
                    }
                }
            }
            return list;
        }

        internal static Collection<PSModuleInfo> GetModuleIfAvailable(ModuleSpecification requiredModule, Runspace rsToUse = null)
        {
            Collection<PSModuleInfo> collection = new Collection<PSModuleInfo>();
            Collection<PSModuleInfo> collection2 = null;
            PowerShell shell = null;
            if (rsToUse == null)
            {
                shell = PowerShell.Create(RunspaceMode.CurrentRunspace);
            }
            else
            {
                shell = PowerShell.Create();
                shell.Runspace = rsToUse;
            }
            using (shell)
            {
                if (requiredModule.Name.EndsWith(".psd1", StringComparison.OrdinalIgnoreCase))
                {
                    shell.AddCommand("Test-ModuleManifest");
                    shell.AddParameter("Path", requiredModule.Name);
                }
                else
                {
                    shell.AddCommand("Get-Module");
                    shell.AddParameter("Name", requiredModule.Name);
                    shell.AddParameter("ListAvailable");
                }
                collection2 = shell.Invoke<PSModuleInfo>();
            }
            foreach (PSModuleInfo info in collection2)
            {
                if (!requiredModule.Guid.HasValue || requiredModule.Guid.Value.Equals(info.Guid))
                {
                    if (requiredModule.Version != null)
                    {
                        if (requiredModule.Version <= info.Version)
                        {
                            collection.Add(info);
                        }
                    }
                    else
                    {
                        collection.Add(info);
                    }
                }
            }
            return collection;
        }

        internal static ModuleLoggingGroupPolicyStatus GetModuleLoggingInformation (ExecutionPolicyScope scope, out IEnumerable<string> moduleNames)
		{
			moduleNames = null;
			ModuleLoggingGroupPolicyStatus undefined = ModuleLoggingGroupPolicyStatus.Undefined;
			if (PowerShellConfiguration.IsWindows) {
				switch (scope) {
				case ExecutionPolicyScope.UserPolicy:
					try {
						using (RegistryKey key2 = Registry.CurrentUser.OpenSubKey(@"Software\Xamarin\PowerShell\ModuleLogging")) { //"Software\Policies\Microsoft\Windows\PowerShell\ModuleLogging"
							if (key2 != null) {
								undefined = GetModuleLoggingValue ("EnableModuleLogging", key2, out moduleNames);
								key2.Close ();
							}
						}
					} catch (SecurityException) {
					}
					return undefined;

				case ExecutionPolicyScope.MachinePolicy:
					try {
						using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Xamarin\PowerShell\ModuleLogging")) {//"Software\Policies\Microsoft\Windows\PowerShell\ModuleLogging"
							if (key != null) {
								undefined = GetModuleLoggingValue ("EnableModuleLogging", key, out moduleNames);
								key.Close ();
							}
						}
					} catch (SecurityException) {
					}
					return undefined;
				}
			} else {
				undefined = PowerShellConfiguration.ModuleLogging.EnableModuleLogging;
				moduleNames = PowerShellConfiguration.ModuleLogging.ModuleNames;
			}
            return undefined;
        }

        private static ModuleLoggingGroupPolicyStatus GetModuleLoggingValue(string groupPolicyValue, RegistryKey key, out IEnumerable<string> moduleNames)
        {
            ModuleLoggingGroupPolicyStatus undefined = ModuleLoggingGroupPolicyStatus.Undefined;
            moduleNames = new List<string>();
            if (key != null)
            {
                object obj2 = key.GetValue(groupPolicyValue);
                if (obj2 == null)
                {
                    return undefined;
                }
                if (string.Equals(obj2.ToString(), "0", StringComparison.OrdinalIgnoreCase))
                {
                    return ModuleLoggingGroupPolicyStatus.Disabled;
                }
                if (!string.Equals(obj2.ToString(), "1", StringComparison.OrdinalIgnoreCase))
                {
                    return undefined;
                }
                undefined = ModuleLoggingGroupPolicyStatus.Enabled;
                try
                {
                    using (RegistryKey key2 = key.OpenSubKey("ModuleNames"))
                    {
                        if (key2 != null)
                        {
                            string[] valueNames = key2.GetValueNames();
                            if ((valueNames != null) && (valueNames.Length > 0))
                            {
                                moduleNames = new List<string>(valueNames);
                            }
                        }
                    }
                }
                catch (SecurityException)
                {
                }
            }
            return undefined;
        }

        internal static string GetResolvedPath(string filePath, ExecutionContext context)
        {
            ProviderInfo provider = null;
            Collection<string> collection;
            if (((context != null) && (context.EngineSessionState != null)) && context.EngineSessionState.IsProviderLoaded(context.ProviderNames.FileSystem))
            {
                try
                {
                    collection = context.SessionState.Path.GetResolvedProviderPathFromPSPath(filePath, true, out provider);
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                    return null;
                }
                if ((provider == null) || !provider.NameEquals(context.ProviderNames.FileSystem))
                {
                    return null;
                }
            }
            else
            {
                collection = new Collection<string> {
                    filePath
                };
            }
            if (((collection != null) && (collection.Count >= 1)) && (collection.Count <= 1))
            {
                return collection[0];
            }
            return null;
        }

        internal bool GetScalarFromData<T>(Hashtable data, string moduleManifestPath, string key, ManifestProcessingFlags manifestProcessingFlags, out T result)
        {
            object valueToConvert = data[key];
            if ((valueToConvert == null) || ((valueToConvert is string) && string.IsNullOrEmpty((string) valueToConvert)))
            {
                result = default(T);
                return true;
            }
            try
            {
                result = (T) LanguagePrimitives.ConvertTo(valueToConvert, typeof(T), CultureInfo.InvariantCulture);
                return true;
            }
            catch (PSInvalidCastException exception)
            {
                result = default(T);
                if ((manifestProcessingFlags & ManifestProcessingFlags.WriteErrors) != 0)
                {
                    ArgumentException exception2 = new ArgumentException(StringUtil.Format(Modules.ModuleManifestInvalidValue, new object[] { key, exception.Message, moduleManifestPath }));
                    ErrorRecord errorRecord = new ErrorRecord(exception2, "Modules_InvalidManifest", ErrorCategory.ResourceUnavailable, moduleManifestPath);
                    base.WriteError(errorRecord);
                }
                return false;
            }
        }

        internal ExternalScriptInfo GetScriptInfoForFile(string fileName, out string scriptName, bool checkExecutionPolicy)
        {
            scriptName = Path.GetFileName(fileName);
            ExternalScriptInfo commandInfo = new ExternalScriptInfo(scriptName, fileName, base.Context);
            if (!scriptName.EndsWith(".psd1", StringComparison.OrdinalIgnoreCase))
            {
                if (checkExecutionPolicy)
                {
                    base.Context.AuthorizationManager.ShouldRunInternal(commandInfo, CommandOrigin.Runspace, base.Context.EngineHostInterface);
                }
                else
                {
                    base.Context.AuthorizationManager.ShouldRunInternal(commandInfo, CommandOrigin.Internal, base.Context.EngineHostInterface);
                }
                CommandDiscovery.VerifyPSVersion(commandInfo);
                commandInfo.SignatureChecked = true;
            }
            return commandInfo;
        }

        private static bool HasInvalidCharacters(string commandName)
        {
            foreach (char ch in commandName)
            {
                switch (ch)
                {
                    case '"':
                    case '#':
                    case '$':
                    case '%':
                    case '&':
                    case '\'':
                    case '(':
                    case ')':
                    case '*':
                    case '+':
                    case ',':
                    case '-':
                    case '/':
                    case ':':
                    case ';':
                    case '<':
                    case '=':
                    case '>':
                    case '?':
                    case '@':
                    case '[':
                    case '\\':
                    case ']':
                    case '^':
                    case '`':
                    case '{':
                    case '|':
                    case '}':
                    case '~':
                        return true;
                }
            }
            return false;
        }

        private static bool HasRequiredModulesCyclicReference(string currentModuleName, List<ModuleSpecification> requiredModules, IEnumerable<PSModuleInfo> moduleInfoList, Dictionary<ModuleSpecification, List<ModuleSpecification>> nonCyclicRequiredModules, out ErrorRecord error)
        {
            error = null;
            if ((requiredModules != null) && (requiredModules.Count != 0))
            {
                foreach (ModuleSpecification specification in requiredModules)
                {
                    if (nonCyclicRequiredModules.ContainsKey(specification))
                    {
                        PSModuleInfo info = null;
                        foreach (PSModuleInfo info2 in moduleInfoList)
                        {
                            if (info2.Name.Equals(currentModuleName, StringComparison.OrdinalIgnoreCase))
                            {
                                info = info2;
                                break;
                            }
                        }
                        MissingMemberException exception = new MissingMemberException(StringUtil.Format(Modules.RequiredModulesCyclicDependency, new object[] { currentModuleName, specification.Name, info.Path }));
                        error = new ErrorRecord(exception, "Modules_InvalidManifest", ErrorCategory.ResourceUnavailable, info.Path);
                        return true;
                    }
                    Collection<PSModuleInfo> moduleIfAvailable = GetModuleIfAvailable(specification, null);
                    List<ModuleSpecification> list = new List<ModuleSpecification>();
                    string name = null;
                    if (moduleIfAvailable.Count == 1)
                    {
                        name = moduleIfAvailable[0].Name;
                        foreach (ModuleSpecification specification2 in moduleIfAvailable[0].RequiredModulesSpecification)
                        {
                            list.Add(specification2);
                        }
                        if (list.Count > 0)
                        {
                            nonCyclicRequiredModules.Add(specification, list);
                        }
                    }
                    if (HasRequiredModulesCyclicReference(name, list, moduleIfAvailable, nonCyclicRequiredModules, out error))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static void ImportFunctionsOrWorkflows(FunctionInfo func, SessionStateInternal targetSessionState, PSModuleInfo sourceModule, List<WildcardPattern> functionPatterns, bool noPatternsSpecified, string prefix, ImportModuleOptions options, bool usePrefix, ref bool checkVerb, ref bool checkNoun, Dictionary<string, string> original2prefixedName, ModuleCmdletBase cmdlet, bool isImportModulePrivate, bool isFunction)
        {
            string text = null;
            if (SessionStateUtilities.MatchesAnyWildcardPattern(func.Name, functionPatterns, noPatternsSpecified))
            {
                string commandName = AddPrefixToCommandName(func.Name, prefix);
                if (options.NoClobber && CommandFound(commandName, targetSessionState))
                {
                    text = StringUtil.Format(Modules.ImportModuleNoClobberForFunction, func.Name);
                    cmdlet.WriteVerbose(text);
                }
                else
                {
                    FunctionInfo command = (options.Local ? targetSessionState.CurrentScope : targetSessionState.ModuleScope).SetFunction(commandName, func.ScriptBlock, func, false, CommandOrigin.Internal, targetSessionState.ExecutionContext);
                    SetCommandVisibility(isImportModulePrivate, command);
                    command.SetModule(sourceModule);
                    func.IsImported = true;
                    if (usePrefix)
                    {
                        original2prefixedName.Add(func.Name, commandName);
                        func.Prefix = prefix;
                        command.Prefix = prefix;
                    }
                    ValidateCommandName(cmdlet, command.Name, sourceModule.Name, ref checkNoun, ref checkVerb);
                    if (func.CommandType == CommandTypes.Workflow)
                    {
                        text = StringUtil.Format(Modules.ImportingWorkflow, commandName);
                    }
                    else
                    {
                        text = StringUtil.Format(Modules.ImportingFunction, commandName);
                    }
                    cmdlet.WriteVerbose(text);
                }
            }
        }

        protected internal void ImportModuleMembers(PSModuleInfo sourceModule, string prefix)
        {
            ImportModuleOptions options = new ImportModuleOptions();
            ImportModuleMembers(this, this.TargetSessionState.Internal, sourceModule, prefix, this.BaseFunctionPatterns, this.BaseCmdletPatterns, this.BaseVariablePatterns, this.BaseAliasPatterns, options);
        }

        protected internal void ImportModuleMembers(PSModuleInfo sourceModule, string prefix, ImportModuleOptions options)
        {
            ImportModuleMembers(this, this.TargetSessionState.Internal, sourceModule, prefix, this.BaseFunctionPatterns, this.BaseCmdletPatterns, this.BaseVariablePatterns, this.BaseAliasPatterns, options);
        }

        internal static void ImportModuleMembers(ModuleCmdletBase cmdlet, SessionStateInternal targetSessionState, PSModuleInfo sourceModule, string prefix, List<WildcardPattern> functionPatterns, List<WildcardPattern> cmdletPatterns, List<WildcardPattern> variablePatterns, List<WildcardPattern> aliasPatterns, ImportModuleOptions options)
        {
            if (sourceModule == null)
            {
                throw PSTraceSource.NewArgumentNullException("sourceModule");
            }
            bool isImportModulePrivate = false;
            if (cmdlet.CommandInfo.Visibility == SessionStateEntryVisibility.Private)
            {
                isImportModulePrivate = true;
            }
            bool usePrefix = !string.IsNullOrEmpty(prefix);
            bool checkVerb = !cmdlet.BaseDisableNameChecking;
            bool checkNoun = !cmdlet.BaseDisableNameChecking;
            if (targetSessionState.Module != null)
            {
                bool flag5 = false;
                foreach (PSModuleInfo info in targetSessionState.Module.NestedModules)
                {
                    if (info.Path.Equals(sourceModule.Path, StringComparison.OrdinalIgnoreCase))
                    {
                        flag5 = true;
                    }
                }
                if (!flag5)
                {
                    targetSessionState.Module.AddNestedModule(sourceModule);
                }
            }
            SessionStateInternal sessionState = null;
            if (sourceModule.SessionState != null)
            {
                sessionState = sourceModule.SessionState.Internal;
            }
            bool defaultValue = (((functionPatterns == null) && (variablePatterns == null)) && (aliasPatterns == null)) && (cmdletPatterns == null);
            Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string text = null;
            foreach (CmdletInfo info2 in sourceModule.CompiledExports)
            {
                if (SessionStateUtilities.MatchesAnyWildcardPattern(info2.Name, cmdletPatterns, defaultValue))
                {
                    if (options.NoClobber && CommandFound(info2.Name, targetSessionState))
                    {
                        text = StringUtil.Format(Modules.ImportModuleNoClobberForCmdlet, info2.Name);
                        cmdlet.WriteVerbose(text);
                    }
                    else
                    {
                        CmdletInfo command = new CmdletInfo(AddPrefixToCommandName(info2.Name, prefix), info2.ImplementingType, info2.HelpFile, info2.PSSnapIn, cmdlet.Context);
                        SetCommandVisibility(isImportModulePrivate, command);
                        command.SetModule(sourceModule);
                        if (usePrefix)
                        {
                            dictionary.Add(info2.Name, command.Name);
                            info2.Prefix = prefix;
                            command.Prefix = prefix;
                        }
                        ValidateCommandName(cmdlet, command.Name, sourceModule.Name, ref checkVerb, ref checkNoun);
                        (options.Local ? targetSessionState.CurrentScope : targetSessionState.ModuleScope).AddCmdletToCache(command.Name, command, CommandOrigin.Internal, targetSessionState.ExecutionContext);
                        info2.IsImported = true;
                        text = StringUtil.Format(Modules.ImportingCmdlet, command.Name);
                        cmdlet.WriteVerbose(text);
                    }
                }
            }
            if (sessionState != null)
            {
                foreach (FunctionInfo info4 in sourceModule.ExportedFunctions.Values)
                {
                    ImportFunctionsOrWorkflows(info4, targetSessionState, sourceModule, functionPatterns, defaultValue, prefix, options, usePrefix, ref checkVerb, ref checkNoun, dictionary, cmdlet, isImportModulePrivate, true);
                }
                foreach (FunctionInfo info5 in sourceModule.ExportedWorkflows.Values)
                {
                    ImportFunctionsOrWorkflows(info5, targetSessionState, sourceModule, functionPatterns, defaultValue, prefix, options, usePrefix, ref checkVerb, ref checkNoun, dictionary, cmdlet, isImportModulePrivate, false);
                }
                foreach (PSVariable variable in sourceModule.ExportedVariables.Values)
                {
                    if (SessionStateUtilities.MatchesAnyWildcardPattern(variable.Name, variablePatterns, defaultValue))
                    {
                        if (options.NoClobber && (targetSessionState.ModuleScope.GetVariable(variable.Name) != null))
                        {
                            text = StringUtil.Format(Modules.ImportModuleNoClobberForVariable, variable.Name);
                            cmdlet.WriteVerbose(text);
                        }
                        else
                        {
                            variable.SetModule(sourceModule);
                            PSVariable variable2 = (options.Local ? targetSessionState.CurrentScope : targetSessionState.ModuleScope).NewVariable(variable, true, sessionState);
                            SetCommandVisibility(isImportModulePrivate, variable2);
                            text = StringUtil.Format(Modules.ImportingVariable, variable.Name);
                            cmdlet.WriteVerbose(text);
                        }
                    }
                }
                foreach (AliasInfo info6 in sourceModule.ExportedAliases.Values)
                {
                    if (SessionStateUtilities.MatchesAnyWildcardPattern(info6.Name, aliasPatterns, defaultValue))
                    {
                        string definition;
                        string commandName = AddPrefixToCommandName(info6.Name, prefix);
                        if (!usePrefix || !dictionary.TryGetValue(info6.Definition, out definition))
                        {
                            definition = info6.Definition;
                        }
                        if (options.NoClobber && CommandFound(commandName, targetSessionState))
                        {
                            text = StringUtil.Format(Modules.ImportModuleNoClobberForAlias, commandName);
                            cmdlet.WriteVerbose(text);
                        }
                        else
                        {
                            AliasInfo info7 = new AliasInfo(commandName, definition, cmdlet.Context);
                            SetCommandVisibility(isImportModulePrivate, info7);
                            info7.SetModule(sourceModule);
                            if (usePrefix)
                            {
                                dictionary.Add(info6.Name, info7.Name);
                                info6.Prefix = prefix;
                                info7.Prefix = prefix;
                            }
                            (options.Local ? targetSessionState.CurrentScope : targetSessionState.ModuleScope).SetAliasItem(info7, false, CommandOrigin.Internal);
                            info6.IsImported = true;
                            text = StringUtil.Format(Modules.ImportingAlias, info7.Name);
                            cmdlet.WriteVerbose(text);
                        }
                    }
                }
            }
        }

        private static PSModuleInfo ImportRequiredModule(ExecutionContext context, ModuleSpecification requiredModule, Version moduleVersion, Guid? moduleGuid, out ErrorRecord error)
        {
            error = null;
            PSModuleInfo info = null;
            using (PowerShell shell = PowerShell.Create(RunspaceMode.CurrentRunspace))
            {
                shell.AddCommand("Import-Module");
                shell.AddParameter("Name", requiredModule.Name);
                shell.AddParameter("Version", moduleVersion);
                shell.Invoke();
                if ((shell.Streams.Error != null) && (shell.Streams.Error.Count > 0))
                {
                    error = shell.Streams.Error[0];
                    return info;
                }
                bool wrongVersion = false;
                bool wrongGuid = false;
                string name = requiredModule.Name;
                string str2 = string.Empty;
                if (name.EndsWith(".psd1", StringComparison.OrdinalIgnoreCase))
                {
                    str2 = name;
                    name = Path.GetFileNameWithoutExtension(name);
                }
                ModuleSpecification specification = new ModuleSpecification(name);
                if (requiredModule.Guid.HasValue)
                {
                    specification.Guid = new Guid?(requiredModule.Guid.Value);
                }
                if (requiredModule.Version != null)
                {
                    specification.Version = requiredModule.Version;
                }
                bool loaded = false;
                info = IsModuleLoaded(context, specification, out wrongVersion, out wrongGuid, out loaded) as PSModuleInfo;
                if (info != null)
                {
                    return info;
                }
                string message = StringUtil.Format(Modules.RequiredModuleNotFound, name);
                if (!string.IsNullOrEmpty(str2))
                {
                    MissingMemberException exception = new MissingMemberException(message);
                    error = new ErrorRecord(exception, "Modules_InvalidManifest", ErrorCategory.ResourceUnavailable, str2);
                    return info;
                }
                InvalidOperationException exception2 = new InvalidOperationException(message);
                error = new ErrorRecord(exception2, "Modules_RequiredModuleNotLoadedWithoutManifest", ErrorCategory.InvalidOperation, requiredModule);
            }
            return info;
        }

        internal bool IsModuleAlreadyLoaded(PSModuleInfo alreadyLoadedModule)
        {
            if (alreadyLoadedModule == null)
            {
                return false;
            }
            if ((this.BaseRequiredVersion != null) && !alreadyLoadedModule.Version.Equals(this.BaseRequiredVersion))
            {
                return false;
            }
            if ((this.BaseMinimumVersion != null) && (alreadyLoadedModule.Version < this.BaseMinimumVersion))
            {
                return false;
            }
            return true;
        }

        internal PSModuleInfo IsModuleImportUnnecessaryBecauseModuleIsAlreadyLoaded(string modulePath, string prefix, ImportModuleOptions options)
        {
            PSModuleInfo info;
            if (!base.Context.Modules.ModuleTable.TryGetValue(modulePath, out info) || !this.IsModuleAlreadyLoaded(info))
            {
                return null;
            }
            if (this.BaseForce)
            {
                this.RemoveModule(info);
                return null;
            }
            if (string.IsNullOrEmpty(prefix) && File.Exists(info.Path))
            {
                string defaultPrefix = this.GetDefaultPrefix(info);
                if (!string.IsNullOrEmpty(defaultPrefix))
                {
                    prefix = defaultPrefix;
                }
            }
            AddModuleToModuleTables(base.Context, this.TargetSessionState.Internal, info);
            this.ImportModuleMembers(info, prefix, options);
            if (this.BaseAsCustomObject)
            {
                if (info.ModuleType != ModuleType.Script)
                {
                    InvalidOperationException exception = new InvalidOperationException(StringUtil.Format(Modules.CantUseAsCustomObjectWithBinaryModule, info.Path));
                    ErrorRecord errorRecord = new ErrorRecord(exception, "Modules_CantUseAsCustomObjectWithBinaryModule", ErrorCategory.PermissionDenied, null);
                    base.WriteError(errorRecord);
                    return info;
                }
                base.WriteObject(info.AsCustomObject());
                return info;
            }
            if (this.BasePassThru)
            {
                base.WriteObject(info);
            }
            return info;
        }

        internal static object IsModuleLoaded(ExecutionContext context, ModuleSpecification requiredModule, out bool wrongVersion, out bool wrongGuid, out bool loaded)
        {
            loaded = false;
            object obj2 = null;
            wrongVersion = false;
            wrongGuid = false;
            string name = requiredModule.Name;
            Guid? guid = requiredModule.Guid;
            Version version = requiredModule.Version;
            foreach (PSModuleInfo info in context.Modules.GetModules(new string[] { "*" }, false))
            {
                if (name.Equals(info.Name, StringComparison.OrdinalIgnoreCase))
                {
                    if (!guid.HasValue || guid.Value.Equals(info.Guid))
                    {
                        if (version != null)
                        {
                            if (version <= info.Version)
                            {
                                obj2 = info;
                                loaded = true;
                                break;
                            }
                            wrongVersion = true;
                            continue;
                        }
                        obj2 = info;
                        loaded = true;
                        break;
                    }
                    wrongGuid = true;
                }
            }
            if ((obj2 == null) && InitialSessionState.IsEngineModule(requiredModule.Name))
            {
                using (PowerShell shell = PowerShell.Create(RunspaceMode.CurrentRunspace))
                {
                    Collection<PSSnapInInfo> collection = null;
                    shell.AddCommand("Get-PSSnapin");
                    shell.AddParameter("Name", requiredModule.Name);
                    try
                    {
                        collection = shell.Invoke<PSSnapInInfo>();
                    }
                    catch (Exception exception)
                    {
                        CommandProcessorBase.CheckForSevereException(exception);
                    }
                    if ((collection != null) && (collection.Count > 0))
                    {
                        obj2 = collection[0];
                        loaded = true;
                    }
                }
            }
            return obj2;
        }

        internal static bool IsPrefixedCommand(CommandInfo commandInfo)
        {
            string str;
            string str2;
            return (CmdletInfo.SplitCmdletName(commandInfo.Name, out str, out str2) ? str2.StartsWith(commandInfo.Prefix, StringComparison.OrdinalIgnoreCase) : commandInfo.Name.StartsWith(commandInfo.Prefix, StringComparison.OrdinalIgnoreCase));
        }

        internal static bool IsRooted(string filePath)
        {
            if (((!Path.IsPathRooted(filePath) && !filePath.StartsWith(@".\", StringComparison.Ordinal)) && (!filePath.StartsWith("./", StringComparison.Ordinal) && !filePath.StartsWith(@"..\", StringComparison.Ordinal))) && ((!filePath.StartsWith("../", StringComparison.Ordinal) && !filePath.StartsWith("~/", StringComparison.Ordinal)) && !filePath.StartsWith(@"~\", StringComparison.Ordinal)))
            {
                return (filePath.IndexOf(":", StringComparison.Ordinal) >= 0);
            }
            return true;
        }

        internal PSModuleInfo LoadBinaryModule(bool trySnapInName, string moduleName, string fileName, Assembly assemblyToLoad, string moduleBase, SessionState ss, ImportModuleOptions options, ManifestProcessingFlags manifestProcessingFlags, string prefix, bool loadTypes, bool loadFormats, out bool found)
        {
            return this.LoadBinaryModule(null, trySnapInName, moduleName, fileName, assemblyToLoad, moduleBase, ss, options, manifestProcessingFlags, prefix, loadTypes, loadFormats, out found, null, false);
        }

        internal PSModuleInfo LoadBinaryModule(PSModuleInfo parentModule, bool trySnapInName, string moduleName, string fileName, Assembly assemblyToLoad, string moduleBase, SessionState ss, ImportModuleOptions options, ManifestProcessingFlags manifestProcessingFlags, string prefix, bool loadTypes, bool loadFormats, out bool found, string shortModuleName, bool disableFormatUpdates)
        {
            PSModuleInfo m = null;
            if ((string.IsNullOrEmpty(moduleName) && string.IsNullOrEmpty(fileName)) && (assemblyToLoad == null))
            {
                throw PSTraceSource.NewArgumentNullException("moduleName,fileName,assemblyToLoad");
            }
            InitialSessionState state = InitialSessionState.Create();
            List<string> cmdletsFromBinaryModuleImplementation = null;
            Assembly assembly = null;
            Exception error = null;
            bool flag = false;
            string path = string.Empty;
            Version assemblyVersionNumber = new Version(0, 0, 0, 0);
            if (assemblyToLoad != null)
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    path = fileName;
                }
                else
                {
                    path = assemblyToLoad.Location;
                    if (string.IsNullOrEmpty(fileName))
                    {
                        fileName = assemblyToLoad.FullName;
                    }
                }
                if (string.IsNullOrEmpty(moduleName))
                {
                    moduleName = "dynamic_code_module_" + assemblyToLoad.GetName();
                }
                if ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) == ManifestProcessingFlags.LoadElements)
                {
                    if ((parentModule != null) && InitialSessionState.IsEngineModule(parentModule.Name))
                    {
                        state.ImportCmdletsFromAssembly(assemblyToLoad, parentModule);
                    }
                    else
                    {
                        state.ImportCmdletsFromAssembly(assemblyToLoad, null);
                    }
                }
                assemblyVersionNumber = GetAssemblyVersionNumber(assemblyToLoad);
            }
            else
            {
                if (trySnapInName && PSSnapInInfo.IsPSSnapinIdValid(moduleName))
                {
                    PSSnapInInfo info2 = null;
                    try
                    {
                        if ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) == ManifestProcessingFlags.LoadElements)
                        {
                            PSSnapInException exception;
                            info2 = state.ImportPSSnapIn(moduleName, out exception);
                        }
                    }
                    catch (PSArgumentException)
                    {
                    }
                    if (info2 != null)
                    {
                        flag = true;
                        if (string.IsNullOrEmpty(fileName))
                        {
                            path = info2.AbsoluteModulePath;
                        }
                        else
                        {
                            path = fileName;
                        }
                        assemblyVersionNumber = info2.Version;
                        if (!loadTypes)
                        {
                            state.Types.Reset();
                        }
                        if (!loadFormats)
                        {
                            state.Formats.Reset();
                        }
                    }
                }
                if (!flag)
                {
                    if ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) == ManifestProcessingFlags.LoadElements)
                    {
                        assembly = base.Context.AddAssembly(moduleName, fileName, out error);
                        if (assembly == null)
                        {
                            if (error != null)
                            {
                                throw error;
                            }
                            found = false;
                            return null;
                        }
                        assemblyVersionNumber = GetAssemblyVersionNumber(assembly);
                        if (string.IsNullOrEmpty(fileName))
                        {
                            path = assembly.Location;
                        }
                        else
                        {
                            path = fileName;
                        }
                        if ((parentModule != null) && InitialSessionState.IsEngineModule(parentModule.Name))
                        {
                            state.ImportCmdletsFromAssembly(assembly, parentModule);
                        }
                        else
                        {
                            state.ImportCmdletsFromAssembly(assembly, null);
                        }
                    }
                    else
                    {
                        string str2 = fileName;
                        path = fileName;
                        if (str2 == null)
                        {
                            str2 = Path.Combine(moduleBase, moduleName);
                        }
                        cmdletsFromBinaryModuleImplementation = GetCmdletsFromBinaryModuleImplementation(str2, manifestProcessingFlags, false);
                    }
                }
            }
            found = true;
            if (string.IsNullOrEmpty(shortModuleName))
            {
                m = new PSModuleInfo(moduleName, path, base.Context, ss);
            }
            else
            {
                m = new PSModuleInfo(shortModuleName, path, base.Context, ss);
            }
            m.SetModuleType(ModuleType.Binary);
            m.SetModuleBase(moduleBase);
            m.SetVersion(assemblyVersionNumber);
            if ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) == ManifestProcessingFlags.LoadElements)
            {
                this.SetModuleLoggingInformation(m);
            }
            List<string> list2 = new List<string>();
            foreach (SessionStateTypeEntry entry in state.Types)
            {
                list2.Add(entry.FileName);
            }
            if (list2.Count > 0)
            {
                m.SetExportedTypeFiles(new ReadOnlyCollection<string>(list2));
            }
            List<string> list3 = new List<string>();
            foreach (SessionStateFormatEntry entry2 in state.Formats)
            {
                list3.Add(entry2.FileName);
            }
            if (list3.Count > 0)
            {
                m.SetExportedFormatFiles(new ReadOnlyCollection<string>(list3));
            }
            foreach (SessionStateProviderEntry entry3 in state.Providers)
            {
                if ((parentModule != null) && InitialSessionState.IsEngineModule(parentModule.Name))
                {
                    entry3.SetModule(parentModule);
                }
                else
                {
                    entry3.SetModule(m);
                }
            }
            if (state.Commands != null)
            {
                foreach (SessionStateCommandEntry entry4 in state.Commands)
                {
                    entry4.SetModule(m);
                    SessionStateCmdletEntry entry5 = entry4 as SessionStateCmdletEntry;
                    if (ss != null)
                    {
                        ss.Internal.ExportedCmdlets.Add(CommandDiscovery.NewCmdletInfo(entry5, base.Context));
                    }
                    else
                    {
                        m.AddExportedCmdlet(CommandDiscovery.NewCmdletInfo(entry5, base.Context));
                    }
                }
            }
            if (cmdletsFromBinaryModuleImplementation != null)
            {
                foreach (string str3 in cmdletsFromBinaryModuleImplementation)
                {
                    m.AddDetectedCmdletExport(str3);
                }
            }
            if (this.BaseCmdletPatterns != null)
            {
                InitialSessionStateEntryCollection<SessionStateCommandEntry> commands = state.Commands;
                for (int i = commands.Count - 1; i >= 0; i--)
                {
                    SessionStateCommandEntry entry6 = commands[i];
                    if (entry6 != null)
                    {
                        string name = entry6.Name;
                        if (!string.IsNullOrEmpty(name) && !SessionStateUtilities.MatchesAnyWildcardPattern(name, this.BaseCmdletPatterns, false))
                        {
                            commands.RemoveItem(i);
                        }
                    }
                }
            }
            foreach (SessionStateCommandEntry entry7 in state.Commands)
            {
                entry7.Name = AddPrefixToCommandName(entry7.Name, prefix);
            }
            SessionStateInternal engineSessionState = base.Context.EngineSessionState;
            if ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) == ManifestProcessingFlags.LoadElements)
            {
                try
                {
                    if (ss != null)
                    {
                        base.Context.EngineSessionState = ss.Internal;
                    }
                    if (disableFormatUpdates)
                    {
                        state.DisableFormatUpdates = true;
                    }
                    state.Bind(base.Context, true, m, options.NoClobber, options.Local);
                    Type[] exportedTypes = new Type[0];
                    if (assembly != null)
                    {
                        exportedTypes = assembly.GetExportedTypes();
                    }
                    else if (assemblyToLoad != null)
                    {
                        exportedTypes = assemblyToLoad.GetExportedTypes();
                    }
                    foreach (Type type in exportedTypes)
                    {
                        if ((typeof(JobSourceAdapter).IsAssignableFrom(type) && !typeof(JobSourceAdapter).Equals(type)) && !base.JobManager.IsRegistered(type.Name))
                        {
                            base.JobManager.RegisterJobSourceAdapter(type);
                        }
                    }
                }
                finally
                {
                    base.Context.EngineSessionState = engineSessionState;
                }
            }
            string str5 = m.Name + @"\";
            bool checkVerb = !this._disableNameChecking;
            bool checkNoun = !this._disableNameChecking;
            foreach (SessionStateCommandEntry entry8 in state.Commands)
            {
                if (entry8._isImported)
                {
                    try
                    {
                        if ((entry8 is SessionStateCmdletEntry) || (entry8 is SessionStateFunctionEntry))
                        {
                            ValidateCommandName(this, entry8.Name, m.Name, ref checkVerb, ref checkNoun);
                        }
                        CommandInvocationIntrinsics.GetCmdlet(str5 + entry8.Name, base.Context);
                    }
                    catch (CommandNotFoundException exception3)
                    {
                        base.WriteError(exception3.ErrorRecord);
                    }
                    if (!string.Equals(entry8.Name, "import-psworkflow", StringComparison.OrdinalIgnoreCase))
                    {
                        string text = StringUtil.Format(Modules.ImportingCmdlet, entry8.Name);
                        base.WriteVerbose(text);
                    }
                }
                else
                {
                    string str8 = StringUtil.Format(Modules.ImportModuleNoClobberForCmdlet, entry8.Name);
                    base.WriteVerbose(str8);
                }
            }
            if ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) == ManifestProcessingFlags.LoadElements)
            {
                AddModuleToModuleTables(base.Context, this.TargetSessionState.Internal, m);
            }
            return m;
        }

        internal PSModuleInfo LoadModule(string fileName, string moduleBase, string prefix, SessionState ss, ref ImportModuleOptions options, ManifestProcessingFlags manifestProcessingFlags, out bool found)
        {
            bool moduleFileFound = false;
            return this.LoadModule(null, fileName, moduleBase, prefix, ss, null, ref options, manifestProcessingFlags, out found, out moduleFileFound);
        }

        internal PSModuleInfo LoadModule(PSModuleInfo parentModule, string fileName, string moduleBase, string prefix, SessionState ss, object privateData, ref ImportModuleOptions options, ManifestProcessingFlags manifestProcessingFlags, out bool found, out bool moduleFileFound)
        {
            if (!File.Exists(fileName))
            {
                found = false;
                moduleFileFound = false;
                return null;
            }
            string extension = Path.GetExtension(fileName);
            if ((this.BaseMinimumVersion != null) || (this.BaseRequiredVersion != null))
            {
                if (string.IsNullOrEmpty(extension) || !extension.Equals(".psd1", StringComparison.OrdinalIgnoreCase))
                {
                    found = false;
                    moduleFileFound = false;
                    return null;
                }
                if (base.Context.Modules.ModuleTable.ContainsKey(fileName) && (base.Context.Modules.ModuleTable[fileName].Version >= this.BaseMinimumVersion))
                {
                    found = false;
                    moduleFileFound = false;
                    return null;
                }
            }
            PSModuleInfo m = null;
            found = false;
            ExternalScriptInfo scriptInfo = null;
            string moduleBeingProcessed = base.Context.ModuleBeingProcessed;
            try
            {
                string str2;
                base.Context.PreviousModuleProcessed = base.Context.ModuleBeingProcessed;
                base.Context.ModuleBeingProcessed = fileName;
                string text = StringUtil.Format(Modules.LoadingModule, fileName);
                base.WriteVerbose(text);
                moduleFileFound = true;
                if (extension.Equals(".psm1", StringComparison.OrdinalIgnoreCase))
                {
                    if ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) != ManifestProcessingFlags.LoadElements)
                    {
                        if (ShouldProcessScriptModule(parentModule, ref found))
                        {
                            bool force = (manifestProcessingFlags & ManifestProcessingFlags.Force) == ManifestProcessingFlags.Force;
                            m = this.AnalyzeScriptFile(fileName, force, base.Context);
                            found = true;
                        }
                        goto Label_0920;
                    }
                    scriptInfo = this.GetScriptInfoForFile(fileName, out str2, true);
                    try
                    {
                        base.Context.Modules.IncrementModuleNestingDepth(this, scriptInfo.Path);
                        try
                        {
                            m = base.Context.Modules.CreateModule(fileName, scriptInfo, base.MyInvocation.ScriptPosition, ss, privateData, this._arguments);
                            m.SetModuleBase(moduleBase);
                            this.SetModuleLoggingInformation(m);
                            if (!m.SessionState.Internal.UseExportList)
                            {
                                ModuleIntrinsics.ExportModuleMembers(this, m.SessionState.Internal, this.MatchAll, this.MatchAll, null, null, options.ServiceCoreAutoAdded ? ServiceCoreAssemblyCmdlets : null);
                            }
                            if ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) == ManifestProcessingFlags.LoadElements)
                            {
                                this.ImportModuleMembers(m, prefix, options);
                                AddModuleToModuleTables(base.Context, this.TargetSessionState.Internal, m);
                            }
                            found = true;
                            if (this.BaseAsCustomObject)
                            {
                                base.WriteObject(m.AsCustomObject());
                            }
                            else if (this.BasePassThru)
                            {
                                base.WriteObject(m);
                            }
                        }
                        catch (RuntimeException exception)
                        {
                            if (ManifestProcessingFlags.WriteErrors == (manifestProcessingFlags & ManifestProcessingFlags.WriteErrors))
                            {
                                exception.ErrorRecord.PreserveInvocationInfoOnce = true;
                                base.WriteError(exception.ErrorRecord);
                            }
                        }
                        goto Label_0920;
                    }
                    finally
                    {
                        base.Context.Modules.DecrementModuleNestingCount();
                    }
                }
                if (extension.Equals(".ps1", StringComparison.OrdinalIgnoreCase))
                {
                    if ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) != ManifestProcessingFlags.LoadElements)
                    {
                        if (ShouldProcessScriptModule(parentModule, ref found))
                        {
                            bool flag4 = (manifestProcessingFlags & ManifestProcessingFlags.Force) == ManifestProcessingFlags.Force;
                            m = this.AnalyzeScriptFile(fileName, flag4, base.Context);
                            found = true;
                        }
                        goto Label_0920;
                    }
                    m = new PSModuleInfo(ModuleIntrinsics.GetModuleName(fileName), fileName, base.Context, ss);
                    scriptInfo = this.GetScriptInfoForFile(fileName, out str2, true);
                    text = StringUtil.Format(Modules.DottingScriptFile, fileName);
                    base.WriteVerbose(text);
                    try
                    {
                        found = true;
                        InvocationInfo variableValue = (InvocationInfo) base.Context.GetVariableValue(SpecialVariables.MyInvocationVarPath);
                        object obj2 = base.Context.GetVariableValue(SpecialVariables.PSScriptRootVarPath);
                        object obj3 = base.Context.GetVariableValue(SpecialVariables.PSCommandPathVarPath);
                        try
                        {
                            InvocationInfo invocationInfo = new InvocationInfo(scriptInfo, scriptInfo.ScriptBlock.Ast.Extent, base.Context);
                            scriptInfo.ScriptBlock.InvokeWithPipe(false, ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe, AutomationNull.Value, AutomationNull.Value, AutomationNull.Value, ((MshCommandRuntime) base.CommandRuntime).OutputPipe, invocationInfo, this.BaseArgumentList ?? new object[0]);
                        }
                        finally
                        {
                            if (base.Context.EngineSessionState.CurrentScope.LocalsTuple != null)
                            {
                                base.Context.EngineSessionState.CurrentScope.LocalsTuple.SetAutomaticVariable(AutomaticVariable.PSScriptRoot, obj2, base.Context);
                                base.Context.EngineSessionState.CurrentScope.LocalsTuple.SetAutomaticVariable(AutomaticVariable.PSCommandPath, obj3, base.Context);
                                base.Context.EngineSessionState.CurrentScope.LocalsTuple.SetAutomaticVariable(AutomaticVariable.MyInvocation, variableValue, base.Context);
                            }
                        }
                        AddModuleToModuleTables(base.Context, this.TargetSessionState.Internal, m);
                        if (this.BaseAsCustomObject)
                        {
                            base.WriteObject(m.AsCustomObject());
                        }
                        else if (this.BasePassThru)
                        {
                            base.WriteObject(m);
                        }
                        goto Label_0920;
                    }
                    catch (RuntimeException exception2)
                    {
                        if (ManifestProcessingFlags.WriteErrors == (manifestProcessingFlags & ManifestProcessingFlags.WriteErrors))
                        {
                            exception2.ErrorRecord.PreserveInvocationInfoOnce = true;
                            base.WriteError(exception2.ErrorRecord);
                        }
                        goto Label_0920;
                    }
                    catch (ExitException exception3)
                    {
                        int argument = (int) exception3.Argument;
                        base.Context.SetVariable(SpecialVariables.LastExitCodeVarPath, argument);
                        goto Label_0920;
                    }
                }
                if (extension.Equals(".psd1", StringComparison.OrdinalIgnoreCase))
                {
                    scriptInfo = this.GetScriptInfoForFile(fileName, out str2, true);
                    found = true;
                    m = this.LoadModuleManifest(scriptInfo, manifestProcessingFlags, this.BaseMinimumVersion, this.BaseRequiredVersion, ref options);
                    if (m != null)
                    {
                        if ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) == ManifestProcessingFlags.LoadElements)
                        {
                            AddModuleToModuleTables(base.Context, this.TargetSessionState.Internal, m);
                        }
                        if (this.BasePassThru)
                        {
                            base.WriteObject(m);
                        }
                    }
                    else if ((this.BaseMinimumVersion != null) || (this.BaseRequiredVersion != null))
                    {
                        found = false;
                    }
                }
                else if (extension.Equals(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    m = this.LoadBinaryModule(false, ModuleIntrinsics.GetModuleName(fileName), fileName, null, moduleBase, ss, options, manifestProcessingFlags, prefix, true, true, out found);
                    if (found = m != null)
                    {
                        if ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) == ManifestProcessingFlags.LoadElements)
                        {
                            AddModuleToModuleTables(base.Context, this.TargetSessionState.Internal, m);
                        }
                        if (this.BaseAsCustomObject)
                        {
                            InvalidOperationException exception4 = new InvalidOperationException(StringUtil.Format(Modules.CantUseAsCustomObjectWithBinaryModule, fileName));
                            ErrorRecord errorRecord = new ErrorRecord(exception4, "Modules_CantUseAsCustomObjectWithBinaryModule", ErrorCategory.PermissionDenied, null);
                            base.WriteError(errorRecord);
                        }
                        else if (this.BasePassThru)
                        {
                            base.WriteObject(m);
                        }
                    }
                }
                else if (extension.Equals(".xaml", StringComparison.OrdinalIgnoreCase))
                {
                    if (Utils.IsRunningFromSysWOW64())
                    {
                        throw new NotSupportedException(AutomationExceptions.WorkflowDoesNotSupportWOW64);
                    }
                    scriptInfo = this.GetScriptInfoForFile(fileName, out str2, true);
                    ImportModuleOptions options2 = new ImportModuleOptions();
                    List<string> workflowsToProcess = new List<string> {
                        fileName
                    };
                    found = true;
                    if ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) != ManifestProcessingFlags.LoadElements)
                    {
                        m = new PSModuleInfo(ModuleIntrinsics.GetModuleName(fileName), fileName, null, null);
                        this.ProcessWorkflowsToProcess(moduleBase, workflowsToProcess, new List<string>(), new List<string>(), null, m, options2);
                    }
                    else
                    {
                        if (ss == null)
                        {
                            ss = new SessionState(base.Context, true, true);
                        }
                        m = new PSModuleInfo(ModuleIntrinsics.GetModuleName(fileName), fileName, base.Context, ss);
                        ss.Internal.Module = m;
                        m.PrivateData = privateData;
                        m.SetModuleType(ModuleType.Workflow);
                        m.SetModuleBase(moduleBase);
                        options2.ServiceCoreAutoAdded = true;
                        this.LoadServiceCoreModule(m, string.Empty, ss, options2, manifestProcessingFlags, true, out found);
                        this.ProcessWorkflowsToProcess(moduleBase, workflowsToProcess, new List<string>(), new List<string>(), ss, m, options2);
                        if ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) != 0)
                        {
                            this.ImportModuleMembers(m, prefix, options);
                        }
                        AddModuleToModuleTables(base.Context, this.TargetSessionState.Internal, m);
                    }
                    if (this.BaseAsCustomObject)
                    {
                        base.WriteObject(m.AsCustomObject());
                    }
                    else if (this.BasePassThru)
                    {
                        base.WriteObject(m);
                    }
                }
                else
                {
                    if (extension.Equals(".cdxml", StringComparison.OrdinalIgnoreCase))
                    {
                        found = true;
                        try
                        {
                            string moduleName = ModuleIntrinsics.GetModuleName(fileName);
                            scriptInfo = this.GetScriptInfoForFile(fileName, out str2, true);
                            try
                            {
                                StringReader cmdletizationXmlReader = new StringReader(scriptInfo.ScriptContents);
                                StringWriter output = new StringWriter(CultureInfo.InvariantCulture);
                                ScriptWriter writer2 = new ScriptWriter(cmdletizationXmlReader, moduleName, "Microsoft.PowerShell.Cmdletization.Cim.CimCmdletAdapter, Microsoft.PowerShell.Commands.Management, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", base.MyInvocation, ScriptWriter.GenerationOptions.HelpXml);
                                if ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) != ManifestProcessingFlags.LoadElements)
                                {
                                    m = new PSModuleInfo(fileName, null, null);
                                    writer2.PopulatePSModuleInfo(m);
                                    writer2.ReportExportedCommands(m);
                                }
                                else
                                {
                                    ArrayList list2;
                                    writer2.WriteScriptModule(output);
                                    ScriptBlock scriptBlock = ScriptBlock.Create(base.Context, output.ToString());
                                    scriptBlock.LanguageMode = 0;
                                    m = base.Context.Modules.CreateModule(moduleName, fileName, scriptBlock, ss, out list2, this._arguments);
                                    m.SetModuleBase(moduleBase);
                                    writer2.PopulatePSModuleInfo(m);
                                    writer2.ReportExportedCommands(m);
                                    this.ImportModuleMembers(m, prefix, options);
                                    AddModuleToModuleTables(base.Context, this.TargetSessionState.Internal, m);
                                }
                            }
                            catch (Exception exception5)
                            {
                                CommandProcessorBase.CheckForSevereException(exception5);
                                throw new XmlException(string.Format(CultureInfo.InvariantCulture, CmdletizationCoreResources.ExportCimCommand_ErrorInCmdletizationXmlFile, new object[] { fileName, exception5.Message }), exception5);
                            }
                            if (this.BaseAsCustomObject)
                            {
                                base.WriteObject(m.AsCustomObject());
                            }
                            else if (this.BasePassThru)
                            {
                                base.WriteObject(m);
                            }
                            goto Label_0920;
                        }
                        catch (RuntimeException exception6)
                        {
                            if (ManifestProcessingFlags.WriteErrors == (manifestProcessingFlags & ManifestProcessingFlags.WriteErrors))
                            {
                                exception6.ErrorRecord.PreserveInvocationInfoOnce = true;
                                base.WriteError(exception6.ErrorRecord);
                            }
                            goto Label_0920;
                        }
                    }
                    found = true;
                    InvalidOperationException exception7 = new InvalidOperationException(StringUtil.Format(Modules.InvalidModuleExtension, extension, fileName));
                    ErrorRecord record2 = new ErrorRecord(exception7, "Modules_InvalidModuleExtension", ErrorCategory.PermissionDenied, null);
                    base.WriteError(record2);
                }
            }
            finally
            {
                base.Context.ModuleBeingProcessed = moduleBeingProcessed;
            }
        Label_0920:
            if ((PSModuleInfo.UseAppDomainLevelModuleCache && (m != null)) && ((moduleBase == null) && this.AddToAppDomainLevelCache))
            {
                PSModuleInfo.AddToAppDomainLevelModuleCache(m.Name, fileName, this.BaseForce);
            }
            return m;
        }

        internal PSModuleInfo LoadModuleManifest(ExternalScriptInfo scriptInfo, ManifestProcessingFlags manifestProcessingFlags, Version version, Version requiredVersion)
        {
            ImportModuleOptions options = new ImportModuleOptions();
            return this.LoadModuleManifest(scriptInfo, manifestProcessingFlags, version, requiredVersion, ref options);
        }

        internal PSModuleInfo LoadModuleManifest(ExternalScriptInfo scriptInfo, ManifestProcessingFlags manifestProcessingFlags, Version version, Version requiredVersion, ref ImportModuleOptions options)
        {
            bool containedErrors = false;
            Hashtable data = null;
            Hashtable localizedData = null;
            if (!this.LoadModuleManifestData(scriptInfo, manifestProcessingFlags, out data, out localizedData, ref containedErrors))
            {
                return null;
            }
            return this.LoadModuleManifest(scriptInfo.Path, scriptInfo, data, localizedData, manifestProcessingFlags, version, requiredVersion, ref options, ref containedErrors);
        }

        internal PSModuleInfo LoadModuleManifest(string moduleManifestPath, ExternalScriptInfo scriptInfo, Hashtable data, Hashtable localizedData, ManifestProcessingFlags manifestProcessingFlags, Version version, Version requiredVersion, ref ImportModuleOptions options, ref bool containedErrors)
        {
            Version version2;
            Version version3;
            string str15;
            Version version5;
            ProcessorArchitecture architecture;
            Version version7;
            Version version9;
            Guid? nullable;
            ModuleSpecification[] specificationArray;
            ModuleSpecification[] specificationArray2;
            List<WildcardPattern> matchAll;
            List<WildcardPattern> list7;
            List<WildcardPattern> list8;
            List<WildcardPattern> list9;
            List<string> list10;
            List<string> list13;
            List<string> list14;
            List<string> list15;
            List<string> list16;
            ModuleSpecification[] specificationArray3;
            SessionState state2;
            string directoryName = Path.GetDirectoryName(moduleManifestPath);
            if ((manifestProcessingFlags & (ManifestProcessingFlags.WriteWarnings | ManifestProcessingFlags.LoadElements | ManifestProcessingFlags.WriteErrors)) != 0)
            {
                base.Context.ModuleBeingProcessed = moduleManifestPath;
            }
            List<string> workflowsToProcess = new List<string>();
            List<string> dependentWorkflows = new List<string>();
            string result = null;
            if (!this.GetScalarFromData<string>(data, moduleManifestPath, "ModuleToProcess", manifestProcessingFlags, out result))
            {
                containedErrors = true;
                if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                {
                    return null;
                }
            }
            string str4 = null;
            if (!this.GetScalarFromData<string>(data, moduleManifestPath, "RootModule", manifestProcessingFlags, out str4))
            {
                containedErrors = true;
                if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                {
                    return null;
                }
            }
            if ((!string.IsNullOrEmpty(result) && ((string.IsNullOrEmpty(base.Context.ModuleBeingProcessed) || !base.Context.ModuleBeingProcessed.Equals(moduleManifestPath, StringComparison.OrdinalIgnoreCase)) || !base.Context.ModuleBeingProcessed.Equals(base.Context.PreviousModuleProcessed, StringComparison.OrdinalIgnoreCase))) && ((manifestProcessingFlags & ManifestProcessingFlags.WriteWarnings) != 0))
            {
                base.WriteWarning(Modules.ModuleToProcessFieldDeprecated);
            }
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(str4))
            {
                if (((string.IsNullOrEmpty(base.Context.ModuleBeingProcessed) || !base.Context.ModuleBeingProcessed.Equals(moduleManifestPath, StringComparison.OrdinalIgnoreCase)) || !base.Context.ModuleBeingProcessed.Equals(base.Context.PreviousModuleProcessed, StringComparison.OrdinalIgnoreCase)) && ((manifestProcessingFlags & ManifestProcessingFlags.WriteErrors) != 0))
                {
                    InvalidOperationException exception = new InvalidOperationException(StringUtil.Format(Modules.ModuleManifestCannotContainBothModuleToProcessAndRootModule, moduleManifestPath));
                    ErrorRecord errorRecord = new ErrorRecord(exception, "Modules_ModuleManifestCannotContainBothModuleToProcessAndRootModule", ErrorCategory.InvalidOperation, moduleManifestPath);
                    base.WriteError(errorRecord);
                }
                if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                {
                    return null;
                }
            }
            string path = (result != null) ? result : str4;
            bool flag = false;
            string str6 = path;
            if (string.Equals(Path.GetExtension(path), ".xaml", StringComparison.OrdinalIgnoreCase))
            {
                if (WildcardPattern.ContainsWildcardCharacters(path))
                {
                    PSInvalidOperationException exception2 = PSTraceSource.NewInvalidOperationException("Modules", "WildCardNotAllowedInModuleToProcessAndInNestedModules", new object[] { moduleManifestPath });
                    exception2.SetErrorId("Modules_WildCardNotAllowedInModuleToProcessAndInNestedModules");
                    throw exception2;
                }
                workflowsToProcess.Add(path);
                path = null;
                flag = true;
            }
            string str7 = null;
            if (!this.GetScalarFromData<string>(data, moduleManifestPath, "DefaultCommandPrefix", manifestProcessingFlags, out str7))
            {
                containedErrors = true;
                if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                {
                    return null;
                }
            }
            string prefix = string.Empty;
            if (!string.IsNullOrEmpty(str7))
            {
                prefix = str7;
            }
            if (!string.IsNullOrEmpty(this.BasePrefix))
            {
                prefix = this.BasePrefix;
            }
            if (!string.IsNullOrEmpty(path))
            {
                if (WildcardPattern.ContainsWildcardCharacters(path))
                {
                    PSInvalidOperationException exception3 = PSTraceSource.NewInvalidOperationException("Modules", "WildCardNotAllowedInModuleToProcessAndInNestedModules", new object[] { moduleManifestPath });
                    exception3.SetErrorId("Modules_WildCardNotAllowedInModuleToProcessAndInNestedModules");
                    throw exception3;
                }
                PSModuleInfo info = null;
                string str9 = this.FixupFileName(directoryName, path, null);
                string extension = Path.GetExtension(str9);
                if (!string.IsNullOrEmpty(extension) && ModuleIntrinsics.IsPowerShellModuleExtension(extension))
                {
                    base.Context.Modules.ModuleTable.TryGetValue(str9, out info);
                }
                else
                {
                    foreach (string str11 in ModuleIntrinsics.PSModuleExtensions)
                    {
                        str9 = this.FixupFileName(directoryName, path, str11);
                        base.Context.Modules.ModuleTable.TryGetValue(str9, out info);
                        if (info != null)
                        {
                            break;
                        }
                    }
                }
                if (((info != null) && ((this.BaseRequiredVersion == null) || info.Version.Equals(this.BaseRequiredVersion))) && (((this.BaseMinimumVersion == null) || (info.Version >= this.BaseMinimumVersion)) && ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) == ManifestProcessingFlags.LoadElements)))
                {
                    if (!this.BaseForce)
                    {
                        AddModuleToModuleTables(base.Context, this.TargetSessionState.Internal, info);
                        this.ImportModuleMembers(info, prefix, options);
                        return info;
                    }
                    if (File.Exists(str9))
                    {
                        this.RemoveModule(info);
                    }
                }
            }
            string str12 = string.Empty;
            if (!this.GetScalarFromData<string>(data, moduleManifestPath, "Author", manifestProcessingFlags, out str12))
            {
                containedErrors = true;
                if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                {
                    return null;
                }
            }
            string str13 = string.Empty;
            if (!this.GetScalarFromData<string>(data, moduleManifestPath, "CompanyName", manifestProcessingFlags, out str13))
            {
                containedErrors = true;
                if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                {
                    return null;
                }
            }
            string str14 = string.Empty;
            if (!this.GetScalarFromData<string>(data, moduleManifestPath, "Copyright", manifestProcessingFlags, out str14))
            {
                containedErrors = true;
                if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                {
                    return null;
                }
            }
            if (!this.GetScalarFromData<Version>(data, moduleManifestPath, "ModuleVersion", manifestProcessingFlags, out version2))
            {
                containedErrors = true;
                if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                {
                    return null;
                }
            }
            else if (version2 == null)
            {
                containedErrors = true;
                if ((manifestProcessingFlags & ManifestProcessingFlags.WriteErrors) != 0)
                {
                    MissingMemberException exception4 = new MissingMemberException(StringUtil.Format(Modules.ModuleManifestMissingModuleVersion, moduleManifestPath));
                    ErrorRecord record2 = new ErrorRecord(exception4, "Modules_InvalidManifest", ErrorCategory.ResourceUnavailable, moduleManifestPath);
                    base.WriteError(record2);
                }
                if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                {
                    return null;
                }
            }
            else if ((requiredVersion != null) && !version2.Equals(requiredVersion))
            {
                if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                {
                    return null;
                }
            }
            else if ((version2 < version) && ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0))
            {
                return null;
            }
            if (!this.GetScalarFromData<Version>(data, moduleManifestPath, "PowerShellVersion", manifestProcessingFlags, out version3))
            {
                containedErrors = true;
                if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                {
                    return null;
                }
            }
            else if (version3 != null)
            {
                Version pSVersion = PSVersionInfo.PSVersion;
                if (pSVersion < version3)
                {
                    containedErrors = true;
                    if ((manifestProcessingFlags & ManifestProcessingFlags.WriteErrors) != 0)
                    {
                        InvalidOperationException exception5 = new InvalidOperationException(StringUtil.Format(Modules.ModuleManifestInsufficientPowerShellVersion, new object[] { pSVersion, moduleManifestPath, version3 }));
                        ErrorRecord record3 = new ErrorRecord(exception5, "Modules_InsufficientPowerShellVersion", ErrorCategory.ResourceUnavailable, moduleManifestPath);
                        base.WriteError(record3);
                    }
                    if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                    {
                        return null;
                    }
                }
            }
            if (!this.GetScalarFromData<string>(data, moduleManifestPath, "PowerShellHostName", manifestProcessingFlags, out str15))
            {
                containedErrors = true;
                if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                {
                    return null;
                }
            }
            else if (str15 != null)
            {
                string name = base.Context.InternalHost.Name;
                if (!string.Equals(name, str15, StringComparison.OrdinalIgnoreCase))
                {
                    containedErrors = true;
                    if ((manifestProcessingFlags & ManifestProcessingFlags.WriteErrors) != 0)
                    {
                        InvalidOperationException exception6 = new InvalidOperationException(StringUtil.Format(Modules.InvalidPowerShellHostName, new object[] { name, moduleManifestPath, str15 }));
                        ErrorRecord record4 = new ErrorRecord(exception6, "Modules_InvalidPowerShellHostName", ErrorCategory.ResourceUnavailable, moduleManifestPath);
                        base.WriteError(record4);
                    }
                    if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                    {
                        return null;
                    }
                }
            }
            if (!this.GetScalarFromData<Version>(data, moduleManifestPath, "PowerShellHostVersion", manifestProcessingFlags, out version5))
            {
                containedErrors = true;
                if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                {
                    return null;
                }
            }
            else if (version5 != null)
            {
                Version version6 = base.Context.InternalHost.Version;
                if (version6 < version5)
                {
                    containedErrors = true;
                    if ((manifestProcessingFlags & ManifestProcessingFlags.WriteErrors) != 0)
                    {
                        string str17 = base.Context.InternalHost.Name;
                        InvalidOperationException exception7 = new InvalidOperationException(StringUtil.Format(Modules.InvalidPowerShellHostVersion, new object[] { str17, version6, moduleManifestPath, version5 }));
                        ErrorRecord record5 = new ErrorRecord(exception7, "Modules_InsufficientPowerShellHostVersion", ErrorCategory.ResourceUnavailable, moduleManifestPath);
                        base.WriteError(record5);
                    }
                    if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                    {
                        return null;
                    }
                }
            }
            if (!this.GetScalarFromData<ProcessorArchitecture>(data, moduleManifestPath, "ProcessorArchitecture", manifestProcessingFlags, out architecture))
            {
                containedErrors = true;
                if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                {
                    return null;
                }
            }
            else
            {
                switch (architecture)
                {
                    case ProcessorArchitecture.None:
                    case ProcessorArchitecture.MSIL:
                        goto Label_0753;
                }
                bool isRunningOnArm = false;
                ProcessorArchitecture processorArchitecture = PsUtils.GetProcessorArchitecture(out isRunningOnArm);
                if (((processorArchitecture != architecture) && !isRunningOnArm) || (isRunningOnArm && !architecture.ToString().Equals(PsUtils.ArmArchitecture, StringComparison.OrdinalIgnoreCase)))
                {
                    containedErrors = true;
                    if ((manifestProcessingFlags & ManifestProcessingFlags.WriteErrors) != 0)
                    {
                        string str18 = isRunningOnArm ? PsUtils.ArmArchitecture : processorArchitecture.ToString();
                        InvalidOperationException exception8 = new InvalidOperationException(StringUtil.Format(Modules.InvalidProcessorArchitecture, new object[] { str18, moduleManifestPath, architecture }));
                        ErrorRecord record6 = new ErrorRecord(exception8, "Modules_InvalidProcessorArchitecture", ErrorCategory.ResourceUnavailable, moduleManifestPath);
                        base.WriteError(record6);
                    }
                    if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                    {
                        return null;
                    }
                }
            }
        Label_0753:
            if (!this.GetScalarFromData<Version>(data, moduleManifestPath, "CLRVersion", manifestProcessingFlags, out version7))
            {
                containedErrors = true;
                if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                {
                    return null;
                }
            }
            else if (version7 != null)
            {
                Version version8 = Environment.Version;
                if (version8 < version7)
                {
                    containedErrors = true;
                    if ((manifestProcessingFlags & ManifestProcessingFlags.WriteErrors) != 0)
                    {
                        InvalidOperationException exception9 = new InvalidOperationException(StringUtil.Format(Modules.ModuleManifestInsufficientCLRVersion, new object[] { version8, moduleManifestPath, version7 }));
                        ErrorRecord record7 = new ErrorRecord(exception9, "Modules_InsufficientCLRVersion", ErrorCategory.ResourceUnavailable, moduleManifestPath);
                        base.WriteError(record7);
                    }
                    if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                    {
                        return null;
                    }
                }
            }
            if (!this.GetScalarFromData<Version>(data, moduleManifestPath, "DotNetFrameworkVersion", manifestProcessingFlags, out version9))
            {
                containedErrors = true;
                if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                {
                    return null;
                }
            }
            else if (version9 != null)
            {
                bool higherThanKnownHighestVersion = false;
                if (!Utils.IsNetFrameworkVersionSupported(version9, out higherThanKnownHighestVersion))
                {
                    containedErrors = true;
                    if ((manifestProcessingFlags & ManifestProcessingFlags.WriteErrors) != 0)
                    {
                        InvalidOperationException exception10 = new InvalidOperationException(StringUtil.Format(Modules.InvalidDotNetFrameworkVersion, moduleManifestPath, version9));
                        ErrorRecord record8 = new ErrorRecord(exception10, "Modules_InsufficientDotNetFrameworkVersion", ErrorCategory.ResourceUnavailable, moduleManifestPath);
                        base.WriteError(record8);
                    }
                    if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                    {
                        return null;
                    }
                }
                else if (higherThanKnownHighestVersion)
                {
                    string text = StringUtil.Format(Modules.CannotDetectNetFrameworkVersion, version9);
                    base.WriteVerbose(text);
                }
            }
            if (!this.GetScalarFromData<Guid?>(data, moduleManifestPath, "guid", manifestProcessingFlags, out nullable))
            {
                containedErrors = true;
                if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                {
                    return null;
                }
            }
            string str20 = null;
            this.GetScalarFromData<string>(data, moduleManifestPath, "HelpInfoURI", manifestProcessingFlags, out str20);
            List<PSModuleInfo> list3 = new List<PSModuleInfo>();
            List<PSModuleInfo> list4 = new List<PSModuleInfo>();
            if (!this.GetScalarFromData<ModuleSpecification[]>(data, moduleManifestPath, "RequiredModules", manifestProcessingFlags, out specificationArray))
            {
                containedErrors = true;
                if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                {
                    return null;
                }
            }
            else if (specificationArray != null)
            {
                if ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) != 0)
                {
                    PSModuleInfo currentModule = new PSModuleInfo(moduleManifestPath, base.Context, null);
                    if (nullable.HasValue)
                    {
                        currentModule.SetGuid(nullable.Value);
                    }
                    if (version2 != null)
                    {
                        currentModule.SetVersion(version2);
                    }
                    foreach (ModuleSpecification specification in specificationArray)
                    {
                        ErrorRecord error = null;
                        PSModuleInfo item = this.LoadRequiredModule(currentModule, specification, moduleManifestPath, manifestProcessingFlags, containedErrors, out error);
                        if ((item == null) && (error != null))
                        {
                            base.WriteError(error);
                            return null;
                        }
                        if (item != null)
                        {
                            list3.Add(item);
                        }
                    }
                }
                else
                {
                    PSModuleInfo info4 = null;
                    foreach (ModuleSpecification specification2 in specificationArray)
                    {
                        info4 = new PSModuleInfo(specification2.Name, base.Context, null);
                        if (specification2.Guid.HasValue)
                        {
                            info4.SetGuid(specification2.Guid.Value);
                        }
                        info4.SetVersion(specification2.Version);
                        list4.Add(info4);
                    }
                }
            }
            if (!this.GetScalarFromData<ModuleSpecification[]>(data, moduleManifestPath, "NestedModules", manifestProcessingFlags, out specificationArray2))
            {
                containedErrors = true;
                if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                {
                    return null;
                }
            }
            List<ModuleSpecification> list5 = new List<ModuleSpecification>();
            if ((specificationArray2 != null) && (specificationArray2.Length > 0))
            {
                foreach (ModuleSpecification specification3 in specificationArray2)
                {
                    if (WildcardPattern.ContainsWildcardCharacters(specification3.Name))
                    {
                        PSInvalidOperationException exception11 = PSTraceSource.NewInvalidOperationException("Modules", "WildCardNotAllowedInModuleToProcessAndInNestedModules", new object[] { moduleManifestPath });
                        exception11.SetErrorId("Modules_WildCardNotAllowedInModuleToProcessAndInNestedModules");
                        throw exception11;
                    }
                    if (string.Equals(Path.GetExtension(specification3.Name), ".xaml", StringComparison.OrdinalIgnoreCase))
                    {
                        workflowsToProcess.Add(specification3.Name);
                    }
                    else
                    {
                        list5.Add(specification3);
                    }
                }
                Array.Clear(specificationArray2, 0, specificationArray2.Length);
                specificationArray2 = null;
            }
            object privateData = null;
            if (data.Contains("PrivateData"))
            {
                privateData = data["PrivateData"];
            }
            if (!this.GetListOfWildcardsFromData(data, moduleManifestPath, "FunctionsToExport", manifestProcessingFlags, out matchAll))
            {
                containedErrors = true;
                if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                {
                    return null;
                }
            }
            if (!this.GetListOfWildcardsFromData(data, moduleManifestPath, "VariablesToExport", manifestProcessingFlags, out list7))
            {
                containedErrors = true;
                if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                {
                    return null;
                }
            }
            if (!this.GetListOfWildcardsFromData(data, moduleManifestPath, "AliasesToExport", manifestProcessingFlags, out list8))
            {
                containedErrors = true;
                if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                {
                    return null;
                }
            }
            if (!this.GetListOfWildcardsFromData(data, moduleManifestPath, "CmdletsToExport", manifestProcessingFlags, out list9))
            {
                containedErrors = true;
                if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                {
                    return null;
                }
            }
            InitialSessionState state = null;
            if ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) != 0)
            {
                state = InitialSessionState.Create();
                if (base.Context.InitialSessionState != null)
                {
                    state.DisableFormatUpdates = base.Context.InitialSessionState.DisableFormatUpdates;
                }
                state.ThrowOnRunspaceOpenError = true;
            }
            bool flag4 = false;
            bool flag5 = false;
            List<string> list11 = new List<string>();
            List<string> assemblyList = new List<string>();
            if (!this.GetListOfStringsFromData(data, moduleManifestPath, "RequiredAssemblies", manifestProcessingFlags, out list10))
            {
                containedErrors = true;
                if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                {
                    return null;
                }
            }
            else
            {
                if ((list10 != null) && (list10.Count > 0))
                {
                    foreach (string str21 in list10)
                    {
                        if (string.Equals(Path.GetExtension(str21), ".xaml", StringComparison.OrdinalIgnoreCase))
                        {
                            dependentWorkflows.Add(str21);
                        }
                        else
                        {
                            list11.Add(str21);
                        }
                    }
                }
                if ((list11 != null) && ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) != 0))
                {
                    foreach (string str22 in list11)
                    {
                        if (WildcardPattern.ContainsWildcardCharacters(str22))
                        {
                            PSInvalidOperationException exception12 = PSTraceSource.NewInvalidOperationException("Modules", "WildCardNotAllowedInRequiredAssemblies", new object[] { moduleManifestPath });
                            exception12.SetErrorId("Modules_WildCardNotAllowedInRequiredAssemblies");
                            throw exception12;
                        }
                        string str23 = this.FixupFileName(directoryName, str22, ".dll");
                        string str24 = StringUtil.Format(Modules.LoadingFile, "Assembly", str23);
                        base.WriteVerbose(str24);
                        state.Assemblies.Add(new SessionStateAssemblyEntry(str22, str23));
                        assemblyList.Add(str23);
                        flag4 = true;
                    }
                }
            }
            if (!this.GetListOfFilesFromData(data, moduleManifestPath, "TypesToProcess", manifestProcessingFlags, directoryName, ".ps1xml", true, out list13))
            {
                containedErrors = true;
                if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                {
                    return null;
                }
            }
            else if ((list13 != null) && ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) != 0))
            {
                foreach (string str25 in list13)
                {
                    string str26 = StringUtil.Format(Modules.LoadingFile, "TypesToProcess", str25);
                    base.WriteVerbose(str26);
                    if (base.Context.RunspaceConfiguration != null)
                    {
                        base.Context.RunspaceConfiguration.Types.Append(new TypeConfigurationEntry(str25));
                        flag5 = true;
                        continue;
                    }
                    bool flag6 = false;
                    string str27 = ResolveRootedFilePath(str25, base.Context) ?? str25;
                    foreach (SessionStateTypeEntry entry in base.Context.InitialSessionState.Types)
                    {
                        if (entry.FileName != null)
                        {
                            string str28 = ResolveRootedFilePath(entry.FileName, base.Context) ?? entry.FileName;
                            if (str28.Equals(str27, StringComparison.OrdinalIgnoreCase))
                            {
                                flag6 = true;
                                break;
                            }
                        }
                    }
                    if (!flag6)
                    {
                        state.Types.Add(new SessionStateTypeEntry(str25));
                        flag4 = true;
                    }
                }
            }
            if (!this.GetListOfFilesFromData(data, moduleManifestPath, "FormatsToProcess", manifestProcessingFlags, directoryName, ".ps1xml", true, out list14))
            {
                containedErrors = true;
                if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                {
                    return null;
                }
            }
            else if ((list14 != null) && ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) != 0))
            {
                foreach (string str29 in list14)
                {
                    string str30 = StringUtil.Format(Modules.LoadingFile, "FormatsToProcess", str29);
                    base.WriteVerbose(str30);
                    if (base.Context.RunspaceConfiguration != null)
                    {
                        base.Context.RunspaceConfiguration.Formats.Append(new FormatConfigurationEntry(str29));
                        flag5 = true;
                        continue;
                    }
                    bool flag7 = false;
                    foreach (SessionStateFormatEntry entry2 in base.Context.InitialSessionState.Formats)
                    {
                        if ((entry2.FileName != null) && entry2.FileName.Equals(str29, StringComparison.OrdinalIgnoreCase))
                        {
                            flag7 = true;
                            break;
                        }
                    }
                    if (!flag7)
                    {
                        state.Formats.Add(new SessionStateFormatEntry(str29));
                        flag4 = true;
                    }
                }
            }
            if (!this.GetListOfFilesFromData(data, moduleManifestPath, "ScriptsToProcess", manifestProcessingFlags, directoryName, ".ps1", true, out list15))
            {
                containedErrors = true;
                if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                {
                    return null;
                }
            }
            else if ((list15 != null) && ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) != 0))
            {
                foreach (string str31 in list15)
                {
                    if (!Path.GetExtension(str31).Equals(".ps1", StringComparison.OrdinalIgnoreCase))
                    {
                        InvalidOperationException e = new InvalidOperationException(StringUtil.Format(Modules.ScriptsToProcessIncorrectExtension, str31));
                        WriteInvalidManifestMemberError(this, "ScriptsToProcess", moduleManifestPath, e, manifestProcessingFlags);
                        containedErrors = true;
                        if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                        {
                            return null;
                        }
                    }
                }
            }
            string str33 = string.Empty;
            if (data.Contains("Description"))
            {
                if ((localizedData != null) && localizedData.Contains("Description"))
                {
                    str33 = (string) LanguagePrimitives.ConvertTo(localizedData["Description"], typeof(string), CultureInfo.InvariantCulture);
                }
                if (string.IsNullOrEmpty(str33))
                {
                    str33 = (string) LanguagePrimitives.ConvertTo(data["Description"], typeof(string), CultureInfo.InvariantCulture);
                }
            }
            if (!this.GetListOfFilesFromData(data, moduleManifestPath, "FileList", manifestProcessingFlags, directoryName, "", false, out list16))
            {
                containedErrors = true;
                if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                {
                    return null;
                }
            }
            if (!this.GetScalarFromData<ModuleSpecification[]>(data, moduleManifestPath, "ModuleList", manifestProcessingFlags, out specificationArray3))
            {
                containedErrors = true;
                if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                {
                    return null;
                }
            }
            if ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) != 0)
            {
                if (flag4)
                {
                    try
                    {
                        state.Bind(base.Context, true);
                    }
                    catch (Exception exception14)
                    {
                        CommandProcessorBase.CheckForSevereException(exception14);
                        this.RemoveTypesAndFormatting(list14, list13);
                        ErrorRecord record10 = new ErrorRecord(exception14, "FormatXmlUpdateException", ErrorCategory.InvalidOperation, null);
                        if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                        {
                            base.ThrowTerminatingError(record10);
                        }
                        else if ((manifestProcessingFlags & ManifestProcessingFlags.WriteErrors) != 0)
                        {
                            base.WriteError(record10);
                        }
                    }
                }
                if (flag5)
                {
                    try
                    {
                        base.Context.CurrentRunspace.RunspaceConfiguration.Types.Update(true);
                        base.Context.CurrentRunspace.RunspaceConfiguration.Formats.Update(true);
                    }
                    catch (Exception exception15)
                    {
                        CommandProcessorBase.CheckForSevereException(exception15);
                        this.RemoveTypesAndFormatting(list14, list13);
                        ErrorRecord record11 = new ErrorRecord(exception15, "FormatXmlUpdateException", ErrorCategory.InvalidOperation, null);
                        if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                        {
                            base.ThrowTerminatingError(record11);
                        }
                        else if ((manifestProcessingFlags & ManifestProcessingFlags.WriteErrors) != 0)
                        {
                            base.WriteError(record11);
                        }
                    }
                }
            }
            if ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) != 0)
            {
                state2 = new SessionState(base.Context, true, true);
            }
            else
            {
                state2 = null;
            }
            PSModuleInfo m = new PSModuleInfo(moduleManifestPath, base.Context, state2);
            m.SetModuleType(ModuleType.Manifest);
            if ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) != 0)
            {
                this.SetModuleLoggingInformation(m);
            }
            if ((list4 != null) && (list4.Count > 0))
            {
                foreach (PSModuleInfo info6 in list4)
                {
                    m.AddRequiredModule(info6);
                }
            }
            if (state2 != null)
            {
                state2.Internal.SetVariable(SpecialVariables.PSScriptRootVarPath, Path.GetDirectoryName(moduleManifestPath), true, CommandOrigin.Internal);
                state2.Internal.SetVariable(SpecialVariables.PSCommandPathVarPath, moduleManifestPath, true, CommandOrigin.Internal);
                state2.Internal.Module = m;
                if (matchAll == null)
                {
                    matchAll = this.MatchAll;
                }
                if (list9 == null)
                {
                    list9 = this.MatchAll;
                }
                if (list7 == null)
                {
                    list7 = this.MatchAll;
                }
                if (list8 == null)
                {
                    list8 = this.MatchAll;
                }
            }
            m.Description = str33;
            m.PrivateData = privateData;
            m.SetExportedTypeFiles(new ReadOnlyCollection<string>(list13 ?? new List<string>()));
            m.SetExportedFormatFiles(new ReadOnlyCollection<string>(list14 ?? new List<string>()));
            m.SetVersion(version2);
            m.Author = str12;
            m.CompanyName = str13;
            m.Copyright = str14;
            m.DotNetFrameworkVersion = version9;
            m.ClrVersion = version7;
            m.PowerShellHostName = str15;
            m.PowerShellHostVersion = version5;
            m.PowerShellVersion = version3;
            m.ProcessorArchitecture = architecture;
            if (list11 != null)
            {
                foreach (string str34 in list11)
                {
                    m.AddRequiredAssembly(str34);
                }
            }
            if (list16 != null)
            {
                foreach (string str35 in list16)
                {
                    m.AddToFileList(str35);
                }
            }
            if (specificationArray3 != null)
            {
                foreach (ModuleSpecification specification4 in specificationArray3)
                {
                    m.AddToModuleList(specification4);
                }
            }
            if (list15 != null)
            {
                foreach (string str36 in list15)
                {
                    m.AddScript(str36);
                }
            }
            m.RootModule = str6;
            m.RootModuleForManifest = str6;
            if (nullable.HasValue)
            {
                m.SetGuid(nullable.Value);
            }
            if (str20 != null)
            {
                m.SetHelpInfoUri(str20);
            }
            foreach (PSModuleInfo info7 in list3)
            {
                m.AddRequiredModule(info7);
            }
            if (specificationArray != null)
            {
                foreach (ModuleSpecification specification5 in specificationArray)
                {
                    m.AddRequiredModuleSpecification(specification5);
                }
            }
            bool flag8 = false;
            if ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) == 0)
            {
                if (matchAll != null)
                {
                    m.DeclaredFunctionExports = new Collection<string>();
                    if (matchAll.Count > 0)
                    {
                        foreach (WildcardPattern pattern in matchAll)
                        {
                            string str37 = pattern.Pattern;
                            if (!WildcardPattern.ContainsWildcardCharacters(str37))
                            {
                                m.DeclaredFunctionExports.Add(AddPrefixToCommandName(str37, str7));
                            }
                            else
                            {
                                flag8 = true;
                            }
                        }
                        if (m.DeclaredFunctionExports.Count == 0)
                        {
                            m.DeclaredFunctionExports = null;
                        }
                    }
                    else
                    {
                        flag8 = true;
                    }
                }
                else
                {
                    flag8 = true;
                }
                if (list9 != null)
                {
                    m.DeclaredCmdletExports = new Collection<string>();
                    if (list9.Count > 0)
                    {
                        foreach (WildcardPattern pattern2 in list9)
                        {
                            string str38 = pattern2.Pattern;
                            if (!WildcardPattern.ContainsWildcardCharacters(str38))
                            {
                                m.DeclaredCmdletExports.Add(AddPrefixToCommandName(str38, str7));
                            }
                            else
                            {
                                flag8 = true;
                            }
                        }
                        if (m.DeclaredCmdletExports.Count == 0)
                        {
                            m.DeclaredCmdletExports = null;
                        }
                    }
                    else
                    {
                        flag8 = true;
                    }
                }
                else
                {
                    flag8 = true;
                }
                if (list8 != null)
                {
                    m.DeclaredAliasExports = new Collection<string>();
                    if (list8.Count > 0)
                    {
                        foreach (WildcardPattern pattern3 in list8)
                        {
                            string str39 = pattern3.Pattern;
                            if (!WildcardPattern.ContainsWildcardCharacters(str39))
                            {
                                m.DeclaredAliasExports.Add(AddPrefixToCommandName(str39, str7));
                            }
                            else
                            {
                                flag8 = true;
                            }
                        }
                        if (m.DeclaredAliasExports.Count == 0)
                        {
                            m.DeclaredAliasExports = null;
                        }
                    }
                    else
                    {
                        flag8 = true;
                    }
                }
                else
                {
                    flag8 = true;
                }
                if (list7 != null)
                {
                    m.DeclaredVariableExports = new Collection<string>();
                    if (list7.Count > 0)
                    {
                        foreach (WildcardPattern pattern4 in list7)
                        {
                            string str40 = pattern4.Pattern;
                            if (!WildcardPattern.ContainsWildcardCharacters(str40))
                            {
                                m.DeclaredVariableExports.Add(str40);
                            }
                        }
                        if (m.DeclaredVariableExports.Count == 0)
                        {
                            m.DeclaredVariableExports = null;
                        }
                    }
                }
                if (!flag8)
                {
                    return m;
                }
            }
            if (list15 != null)
            {
                foreach (string str41 in list15)
                {
                    bool found = false;
                    PSModuleInfo info8 = this.LoadModule(str41, directoryName, string.Empty, null, ref options, manifestProcessingFlags, out found);
                    if (found && (state2 == null))
                    {
                        foreach (string str42 in info8.ExportedCmdlets.Keys)
                        {
                            m.AddDetectedCmdletExport(str42);
                        }
                        foreach (string str43 in info8.ExportedFunctions.Keys)
                        {
                            m.AddDetectedFunctionExport(str43);
                        }
                        foreach (string str44 in info8.ExportedAliases.Keys)
                        {
                            m.AddDetectedAliasExport(str44, info8.ExportedAliases[str44].Definition);
                        }
                    }
                }
            }
            if (list5 != null)
            {
                if ((state2 == null) && ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) == ManifestProcessingFlags.LoadElements))
                {
                    containedErrors = true;
                    if ((manifestProcessingFlags & ManifestProcessingFlags.WriteErrors) != 0)
                    {
                        ErrorRecord record12 = new ErrorRecord(new ArgumentException(StringUtil.Format(Modules.ModuleManifestNestedModulesCantGoWithModuleToProcess, moduleManifestPath)), "Modules_BinaryModuleAndNestedModules", ErrorCategory.InvalidArgument, moduleManifestPath);
                        base.WriteError(record12);
                    }
                }
                bool basePassThru = this.BasePassThru;
                this.BasePassThru = false;
                List<WildcardPattern> baseVariablePatterns = this.BaseVariablePatterns;
                this.BaseVariablePatterns = this.MatchAll;
                List<WildcardPattern> baseFunctionPatterns = this.BaseFunctionPatterns;
                this.BaseFunctionPatterns = this.MatchAll;
                List<WildcardPattern> baseAliasPatterns = this.BaseAliasPatterns;
                this.BaseAliasPatterns = this.MatchAll;
                List<WildcardPattern> baseCmdletPatterns = this.BaseCmdletPatterns;
                this.BaseCmdletPatterns = this.MatchAll;
                bool baseDisableNameChecking = this.BaseDisableNameChecking;
                this.BaseDisableNameChecking = true;
                SessionStateInternal engineSessionState = base.Context.EngineSessionState;
                try
                {
                    ManifestProcessingFlags flags1 = manifestProcessingFlags & ManifestProcessingFlags.LoadElements;
                    if (state2 != null)
                    {
                        base.Context.EngineSessionState = state2.Internal;
                    }
                    ImportModuleOptions nestedModuleOptions = new ImportModuleOptions();
                    foreach (ModuleSpecification specification6 in list5)
                    {
                        PSModuleInfo info9;
                        bool flag12 = false;
                        bool baseGlobal = this.BaseGlobal;
                        this.BaseGlobal = false;
                        string shortModuleName = null;
                        if (specification6.Name == this.ServiceCoreAssemblyFullName)
                        {
                            shortModuleName = this.ServiceCoreAssemblyShortName;
                        }
                        if (string.Equals(specification6.Name, this.ServiceCoreAssemblyFullName, StringComparison.OrdinalIgnoreCase) || string.Equals(specification6.Name, this.ServiceCoreAssemblyShortName, StringComparison.OrdinalIgnoreCase))
                        {
                            info9 = this.LoadServiceCoreModule(m, directoryName, null, nestedModuleOptions, manifestProcessingFlags, false, out flag12);
                        }
                        else
                        {
                            info9 = this.LoadModuleNamedInManifest(m, specification6, directoryName, true, string.Empty, null, nestedModuleOptions, manifestProcessingFlags, true, true, privateData, out flag12, shortModuleName);
                        }
                        this.BaseGlobal = baseGlobal;
                        if (flag12)
                        {
                            if (((state2 == null) && (info9 != null)) && !string.Equals(info9.Name, this.ServiceCoreAssemblyShortName, StringComparison.OrdinalIgnoreCase))
                            {
                                foreach (string str47 in info9.ExportedCmdlets.Keys)
                                {
                                    m.AddDetectedCmdletExport(str47);
                                }
                                foreach (string str48 in info9.ExportedFunctions.Keys)
                                {
                                    m.AddDetectedFunctionExport(str48);
                                }
                                foreach (string str49 in info9.ExportedAliases.Keys)
                                {
                                    m.AddDetectedAliasExport(str49, info9.ExportedAliases[str49].Definition);
                                }
                            }
                            if (info9 != null)
                            {
                                m.AddNestedModule(info9);
                            }
                        }
                        else
                        {
                            containedErrors = true;
                            string message = StringUtil.Format(Modules.ManifestMemberNotFound, new object[] { specification6.Name, "NestedModules", moduleManifestPath });
                            FileNotFoundException innerException = new FileNotFoundException(message);
                            PSInvalidOperationException exception17 = new PSInvalidOperationException(message, innerException, "Modules_ModuleFileNotFound", ErrorCategory.ResourceUnavailable, ModuleIntrinsics.GetModuleName(moduleManifestPath));
                            throw exception17;
                        }
                    }
                    if (flag)
                    {
                        m.SetModuleType(ModuleType.Workflow);
                    }
                    if ((workflowsToProcess != null) && (workflowsToProcess.Count > 0))
                    {
                        scriptInfo.ValidateScriptInfo(base.Host);
                        nestedModuleOptions.ServiceCoreAutoAdded = true;
                        if ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) != ManifestProcessingFlags.LoadElements)
                        {
                            this.ProcessWorkflowsToProcess(directoryName, workflowsToProcess, new List<string>(), new List<string>(), null, m, nestedModuleOptions);
                        }
                        else
                        {
                            bool flag14 = this.BaseGlobal;
                            this.BaseGlobal = false;
                            bool flag15 = false;
                            foreach (string str51 in workflowsToProcess)
                            {
                                List<string> list21 = new List<string> {
                                    str51
                                };
                                SessionState sessionState = new SessionState(base.Context, true, true);
                                PSModuleInfo parentModule = new PSModuleInfo(ModuleIntrinsics.GetModuleName(str51), str51, base.Context, sessionState);
                                sessionState.Internal.Module = parentModule;
                                parentModule.PrivateData = privateData;
                                parentModule.SetModuleType(ModuleType.Workflow);
                                parentModule.SetModuleBase(directoryName);
                                this.LoadServiceCoreModule(parentModule, string.Empty, sessionState, nestedModuleOptions, manifestProcessingFlags, true, out flag15);
                                this.ProcessWorkflowsToProcess(directoryName, list21, dependentWorkflows, assemblyList, sessionState, parentModule, nestedModuleOptions);
                                if ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) != 0)
                                {
                                    this.ImportModuleMembers(parentModule, this.BasePrefix, options);
                                }
                                AddModuleToModuleTables(base.Context, this.TargetSessionState.Internal, parentModule);
                                m.AddNestedModule(parentModule);
                            }
                            this.BaseGlobal = flag14;
                        }
                    }
                }
                catch (Exception)
                {
                    this.RemoveTypesAndFormatting(list14, list13);
                    throw;
                }
                finally
                {
                    base.Context.EngineSessionState = engineSessionState;
                    this.BasePassThru = basePassThru;
                    this.BaseVariablePatterns = baseVariablePatterns;
                    this.BaseFunctionPatterns = baseFunctionPatterns;
                    this.BaseAliasPatterns = baseAliasPatterns;
                    this.BaseCmdletPatterns = baseCmdletPatterns;
                    this.BaseDisableNameChecking = baseDisableNameChecking;
                }
            }
            if (path != null)
            {
                PSModuleInfo info11;
                bool flag16 = this.BasePassThru;
                this.BasePassThru = false;
                List<WildcardPattern> list22 = this.BaseVariablePatterns;
                this.BaseVariablePatterns = new List<WildcardPattern>();
                List<WildcardPattern> list23 = this.BaseFunctionPatterns;
                this.BaseFunctionPatterns = new List<WildcardPattern>();
                List<WildcardPattern> list24 = this.BaseAliasPatterns;
                this.BaseAliasPatterns = new List<WildcardPattern>();
                List<WildcardPattern> list25 = this.BaseCmdletPatterns;
                this.BaseCmdletPatterns = new List<WildcardPattern>();
                try
                {
                    bool flag17 = false;
                    info11 = this.LoadModuleNamedInManifest(null, new ModuleSpecification(path), directoryName, false, prefix, state2, options, manifestProcessingFlags, (list13 == null) || (0 == list13.Count), (list14 == null) || (0 == list14.Count), privateData, out flag17, null);
                    if (!flag17 || (info11 == null))
                    {
                        containedErrors = true;
                        string str52 = StringUtil.Format(Modules.ManifestMemberNotFound, new object[] { path, "ModuleToProcess/RootModule", moduleManifestPath });
                        FileNotFoundException exception18 = new FileNotFoundException(str52);
                        PSInvalidOperationException exception19 = new PSInvalidOperationException(str52, exception18, "Modules_ModuleFileNotFound", ErrorCategory.ResourceUnavailable, ModuleIntrinsics.GetModuleName(moduleManifestPath));
                        throw exception19;
                    }
                    ErrorRecord record13 = this.GetErrorRecordIfUnsupportedRootCdxmlAndNestedModuleScenario(data, moduleManifestPath, info11.Path);
                    if (record13 != null)
                    {
                        containedErrors = true;
                        this.RemoveModule(info11);
                        PSInvalidOperationException exception20 = new PSInvalidOperationException(record13.Exception.Message, record13.Exception, record13.FullyQualifiedErrorId, ErrorCategory.InvalidOperation, moduleManifestPath);
                        throw exception20;
                    }
                }
                catch (Exception)
                {
                    this.RemoveTypesAndFormatting(list14, list13);
                    throw;
                }
                finally
                {
                    this.BasePassThru = flag16;
                    this.BaseVariablePatterns = list22;
                    this.BaseFunctionPatterns = list23;
                    this.BaseAliasPatterns = list24;
                    this.BaseCmdletPatterns = list25;
                }
                if ((info11.SessionState == null) && (state2 != null))
                {
                    info11.SessionState = state2;
                    state2.Internal.Module = info11;
                }
                else if ((info11.SessionState != null) && (state2 == null))
                {
                    state2 = info11.SessionState;
                }
                info11.SetName(m.Name);
                foreach (PSModuleInfo info12 in m.NestedModules)
                {
                    info11.AddNestedModule(info12);
                }
                foreach (PSModuleInfo info13 in m.RequiredModules)
                {
                    info11.AddRequiredModule(info13);
                }
                info11.SetVersion(m.Version);
                if (string.IsNullOrEmpty(info11.Description))
                {
                    info11.Description = str33;
                }
                if (info11.Version.Equals(new Version(0, 0)))
                {
                    info11.SetVersion(version2);
                }
                if (info11.Guid.Equals(Guid.Empty) && nullable.HasValue)
                {
                    info11.SetGuid(nullable.Value);
                }
                if ((info11.HelpInfoUri == null) && (str20 != null))
                {
                    info11.SetHelpInfoUri(str20);
                }
                if (specificationArray != null)
                {
                    foreach (ModuleSpecification specification7 in specificationArray)
                    {
                        info11.AddRequiredModuleSpecification(specification7);
                    }
                }
                if (info11.PrivateData == null)
                {
                    info11.PrivateData = m.PrivateData;
                }
                if (state2 == null)
                {
                    info11.Path = m.Path;
                }
                if (string.IsNullOrEmpty(info11.Author))
                {
                    info11.Author = str12;
                }
                if (string.IsNullOrEmpty(info11.CompanyName))
                {
                    info11.CompanyName = str13;
                }
                if (string.IsNullOrEmpty(info11.Copyright))
                {
                    info11.Copyright = str14;
                }
                if (info11.PowerShellVersion == null)
                {
                    info11.PowerShellVersion = version3;
                }
                if (string.IsNullOrEmpty(info11.PowerShellHostName))
                {
                    info11.PowerShellHostName = str15;
                }
                if (info11.PowerShellHostVersion == null)
                {
                    info11.PowerShellHostVersion = version5;
                }
                if (info11.DotNetFrameworkVersion == null)
                {
                    info11.DotNetFrameworkVersion = version9;
                }
                if (info11.ClrVersion == null)
                {
                    info11.ClrVersion = version7;
                }
                if (((info11.FileList == null) || (info11.FileList.LongCount<string>() == 0L)) && (list16 != null))
                {
                    foreach (string str53 in list16)
                    {
                        info11.AddToFileList(str53);
                    }
                }
                if (((info11.ModuleList == null) || (info11.ModuleList.LongCount<object>() == 0L)) && (specificationArray3 != null))
                {
                    foreach (ModuleSpecification specification8 in specificationArray3)
                    {
                        info11.AddToModuleList(specification8);
                    }
                }
                if (info11.ProcessorArchitecture == ProcessorArchitecture.None)
                {
                    info11.ProcessorArchitecture = architecture;
                }
                if (((info11.RequiredAssemblies == null) || (info11.RequiredAssemblies.LongCount<string>() == 0L)) && (list11 != null))
                {
                    foreach (string str54 in list11)
                    {
                        info11.AddRequiredAssembly(str54);
                    }
                }
                if (((info11.Scripts == null) || (info11.Scripts.LongCount<string>() == 0L)) && (list15 != null))
                {
                    foreach (string str55 in list15)
                    {
                        info11.AddScript(str55);
                    }
                }
                if (info11.RootModuleForManifest == null)
                {
                    info11.RootModuleForManifest = m.RootModuleForManifest;
                }
                if ((info11.DeclaredCmdletExports == null) || (info11.DeclaredCmdletExports.Count == 0))
                {
                    info11.DeclaredCmdletExports = m.DeclaredCmdletExports;
                }
                if (m._detectedCmdletExports != null)
                {
                    foreach (string str56 in m._detectedCmdletExports)
                    {
                        info11.AddDetectedCmdletExport(str56);
                    }
                }
                if ((info11.DeclaredFunctionExports == null) || (info11.DeclaredFunctionExports.Count == 0))
                {
                    info11.DeclaredFunctionExports = m.DeclaredFunctionExports;
                }
                if (m._detectedFunctionExports != null)
                {
                    foreach (string str57 in m._detectedFunctionExports)
                    {
                        info11.AddDetectedFunctionExport(str57);
                    }
                }
                if ((info11.DeclaredAliasExports == null) || (info11.DeclaredAliasExports.Count == 0))
                {
                    info11.DeclaredAliasExports = m.DeclaredAliasExports;
                }
                if (m._detectedAliasExports != null)
                {
                    foreach (string str58 in m._detectedAliasExports.Keys)
                    {
                        info11.AddDetectedAliasExport(str58, m._detectedAliasExports[str58]);
                    }
                }
                if ((info11.DeclaredVariableExports == null) || (info11.DeclaredVariableExports.Count == 0))
                {
                    info11.DeclaredVariableExports = m.DeclaredVariableExports;
                }
                if (m._detectedWorkflowExports != null)
                {
                    foreach (string str59 in m._detectedWorkflowExports)
                    {
                        info11.AddDetectedWorkflowExport(str59);
                    }
                }
                if ((info11.DeclaredWorkflowExports == null) || (info11.DeclaredWorkflowExports.Count == 0))
                {
                    info11.DeclaredWorkflowExports = m.DeclaredWorkflowExports;
                }
                if (m.ExportedTypeFiles.Count > 0)
                {
                    info11.SetExportedTypeFiles(m.ExportedTypeFiles);
                }
                if (m.ExportedFormatFiles.Count > 0)
                {
                    info11.SetExportedFormatFiles(m.ExportedFormatFiles);
                }
                m = info11;
                if (m.ModuleType == ModuleType.Binary)
                {
                    if ((list9 != null) && (state2 != null))
                    {
                        m.ExportedCmdlets.Clear();
                        if (state2 != null)
                        {
                            ModuleIntrinsics.ExportModuleMembers(this, state2.Internal, matchAll, list9, list8, list7, null);
                        }
                    }
                }
                else
                {
                    if ((state2 != null) && !state2.Internal.UseExportList)
                    {
                        ModuleIntrinsics.ExportModuleMembers(this, state2.Internal, this.MatchAll, this.MatchAll, null, null, options.ServiceCoreAutoAdded ? ServiceCoreAssemblyCmdlets : null);
                    }
                    if (matchAll != null)
                    {
                        if (state2 != null)
                        {
                            UpdateCommandCollection<FunctionInfo>(state2.Internal.ExportedFunctions, matchAll);
                        }
                        else
                        {
                            Collection<string> list = new Collection<string>();
                            if (m.DeclaredFunctionExports != null)
                            {
                                foreach (string str60 in m.DeclaredFunctionExports)
                                {
                                    list.Add(str60);
                                }
                            }
                            UpdateCommandCollection(list, matchAll);
                        }
                    }
                    if (list9 != null)
                    {
                        if (state2 != null)
                        {
                            UpdateCommandCollection<CmdletInfo>(m.CompiledExports, list9);
                        }
                        else
                        {
                            UpdateCommandCollection(m.DeclaredCmdletExports, list9);
                        }
                    }
                    if (list8 != null)
                    {
                        if (state2 != null)
                        {
                            UpdateCommandCollection<AliasInfo>(state2.Internal.ExportedAliases, list8);
                        }
                        else
                        {
                            UpdateCommandCollection(m.DeclaredAliasExports, list8);
                        }
                    }
                    if ((list7 != null) && (state2 != null))
                    {
                        List<PSVariable> collection = new List<PSVariable>();
                        foreach (PSVariable variable in state2.Internal.ExportedVariables)
                        {
                            if (SessionStateUtilities.MatchesAnyWildcardPattern(variable.Name, list7, false))
                            {
                                collection.Add(variable);
                            }
                        }
                        state2.Internal.ExportedVariables.Clear();
                        state2.Internal.ExportedVariables.AddRange(collection);
                    }
                }
            }
            else if (state2 != null)
            {
                ModuleIntrinsics.ExportModuleMembers(this, state2.Internal, matchAll, list9, list8, list7, options.ServiceCoreAutoAdded ? ServiceCoreAssemblyCmdlets : null);
            }
            if ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) != 0)
            {
                this.ImportModuleMembers(m, prefix, options);
            }
            return m;
        }

        private Hashtable LoadModuleManifestData(ExternalScriptInfo scriptInfo, string[] validMembers, ManifestProcessingFlags manifestProcessingFlags, ref bool containedErrors)
        {
            try
            {
                return this.LoadModuleManifestData(scriptInfo.Path, scriptInfo.ScriptBlock, validMembers, manifestProcessingFlags, ref containedErrors);
            }
            catch (RuntimeException exception)
            {
                if ((manifestProcessingFlags & ManifestProcessingFlags.WriteErrors) != 0)
                {
                    MissingMemberException exception2 = new MissingMemberException(StringUtil.Format(Modules.InvalidModuleManifest, scriptInfo.Path, exception.Message));
                    ErrorRecord errorRecord = new ErrorRecord(exception2, "Modules_InvalidManifest", ErrorCategory.ResourceUnavailable, scriptInfo.Path);
                    base.WriteError(errorRecord);
                }
                containedErrors = true;
                return null;
            }
        }

        internal bool LoadModuleManifestData(ExternalScriptInfo scriptInfo, ManifestProcessingFlags manifestProcessingFlags, out Hashtable data, out Hashtable localizedData, ref bool containedErrors)
        {
            localizedData = null;
            data = this.LoadModuleManifestData(scriptInfo, ModuleManifestMembers, manifestProcessingFlags, ref containedErrors);
            if (data == null)
            {
                return false;
            }
            ExternalScriptInfo info = this.FindLocalizedModuleManifest(scriptInfo.Path);
            localizedData = null;
            if (info != null)
            {
                localizedData = this.LoadModuleManifestData(info, null, manifestProcessingFlags, ref containedErrors);
                if (localizedData == null)
                {
                    return false;
                }
            }
            return true;
        }

        internal Hashtable LoadModuleManifestData(string moduleManifestPath, ScriptBlock scriptBlock, string[] validMembers, ManifestProcessingFlags manifestProcessingFlags, ref bool containedErrors)
        {
            object obj2;
            try
            {
                List<string> allowedVariables = new List<string> { "PSScriptRoot" };
                scriptBlock.CheckRestrictedLanguage(PermittedCmdlets, allowedVariables, true);
            }
            catch (RuntimeException exception)
            {
                if ((manifestProcessingFlags & ManifestProcessingFlags.WriteErrors) != 0)
                {
                    MissingMemberException exception2 = new MissingMemberException(StringUtil.Format(Modules.InvalidModuleManifest, moduleManifestPath, exception.Message));
                    ErrorRecord errorRecord = new ErrorRecord(exception2, "Modules_InvalidManifest", ErrorCategory.ResourceUnavailable, moduleManifestPath);
                    base.WriteError(errorRecord);
                }
                containedErrors = true;
                return null;
            }
            object variableValue = base.Context.GetVariableValue(SpecialVariables.PSScriptRootVarPath);
            object newValue = base.Context.GetVariableValue(SpecialVariables.PSCommandPathVarPath);
            ArrayList list2 = (ArrayList) base.Context.GetVariableValue(SpecialVariables.ErrorVarPath);
            int count = list2.Count;
            try
            {
                base.Context.SetVariable(SpecialVariables.PSScriptRootVarPath, Path.GetDirectoryName(moduleManifestPath));
                base.Context.SetVariable(SpecialVariables.PSCommandPathVarPath, moduleManifestPath);
                obj2 = PSObject.Base(scriptBlock.InvokeReturnAsIs(new object[0]));
            }
            finally
            {
                base.Context.SetVariable(SpecialVariables.PSScriptRootVarPath, variableValue);
                base.Context.SetVariable(SpecialVariables.PSCommandPathVarPath, newValue);
                if ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) != ManifestProcessingFlags.LoadElements)
                {
                    while (list2.Count > count)
                    {
                        list2.RemoveAt(0);
                    }
                }
            }
            Hashtable data = obj2 as Hashtable;
            if (data == null)
            {
                if ((manifestProcessingFlags & ManifestProcessingFlags.WriteErrors) != 0)
                {
                    ArgumentException exception3 = new ArgumentException(StringUtil.Format(Modules.EmptyModuleManifest, moduleManifestPath));
                    ErrorRecord record2 = new ErrorRecord(exception3, "Modules_InvalidManifest", ErrorCategory.ResourceUnavailable, moduleManifestPath);
                    base.WriteError(record2);
                }
                containedErrors = true;
                return null;
            }
            if ((validMembers != null) && !this.ValidateManifestHash(data, validMembers, moduleManifestPath, manifestProcessingFlags))
            {
                containedErrors = true;
                if ((manifestProcessingFlags & ManifestProcessingFlags.NullOnFirstError) != 0)
                {
                    return null;
                }
            }
            return data;
        }

        private PSModuleInfo LoadModuleNamedInManifest(PSModuleInfo parentModule, ModuleSpecification moduleSpecification, string moduleBase, bool searchModulePath, string prefix, SessionState ss, ImportModuleOptions options, ManifestProcessingFlags manifestProcessingFlags, bool loadTypesFiles, bool loadFormatFiles, object privateData, out bool found, string shortModuleName)
        {
            PSModuleInfo module = null;
            PSModuleInfo nestedModuleInfoIfAvailable = null;
            PSModuleInfo info3;
            found = false;
            bool moduleFileFound = false;
            bool flag2 = false;
            Version baseMinimumVersion = this.BaseMinimumVersion;
            Version baseRequiredVersion = this.BaseRequiredVersion;
            string str = ResolveRootedFilePath(moduleSpecification.Name, base.Context);
            if (string.IsNullOrEmpty(str))
            {
                str = Path.Combine(moduleBase, moduleSpecification.Name);
            }
            else
            {
                flag2 = true;
            }
            string extension = Path.GetExtension(moduleSpecification.Name);
            try
            {
                base.Context.Modules.IncrementModuleNestingDepth(this, str);
                this.BaseMinimumVersion = null;
                this.BaseRequiredVersion = null;
                if (!ModuleIntrinsics.IsPowerShellModuleExtension(extension))
                {
                    extension = null;
                }
                if (extension == null)
                {
                    if (this.VerifyIfNestedModuleIsAvailable(moduleSpecification, str, extension, out nestedModuleInfoIfAvailable))
                    {
                        module = this.LoadUsingExtensions(parentModule, moduleSpecification.Name, str, extension, moduleBase, prefix, ss, options, manifestProcessingFlags, out found, out moduleFileFound);
                    }
                    if (!found && !moduleFileFound)
                    {
                        string rootedModulePath = Path.Combine(str, moduleSpecification.Name);
                        string str4 = Path.Combine(moduleBase, moduleSpecification.Name);
                        if (this.VerifyIfNestedModuleIsAvailable(moduleSpecification, rootedModulePath, extension, out nestedModuleInfoIfAvailable))
                        {
                            module = this.LoadUsingExtensions(parentModule, moduleSpecification.Name, rootedModulePath, extension, str4, prefix, ss, options, manifestProcessingFlags, out found, out moduleFileFound);
                        }
                    }
                }
                else
                {
                    if (this.VerifyIfNestedModuleIsAvailable(moduleSpecification, str, extension, out nestedModuleInfoIfAvailable))
                    {
                        module = this.LoadModule(parentModule, str, moduleBase, prefix, ss, privateData, ref options, manifestProcessingFlags, out found, out moduleFileFound);
                    }
                    if (!found && !moduleFileFound)
                    {
                        string str5 = Path.Combine(str, moduleSpecification.Name);
                        string str6 = Path.Combine(moduleBase, moduleSpecification.Name);
                        if (this.VerifyIfNestedModuleIsAvailable(moduleSpecification, str5, extension, out nestedModuleInfoIfAvailable))
                        {
                            module = this.LoadModule(parentModule, str5, str6, prefix, ss, privateData, ref options, manifestProcessingFlags, out found, out moduleFileFound);
                        }
                    }
                }
                if (!found && flag2)
                {
                    return null;
                }
                if ((searchModulePath && !found) && (!moduleFileFound && this.VerifyIfNestedModuleIsAvailable(moduleSpecification, null, null, out nestedModuleInfoIfAvailable)))
                {
                    IEnumerable<string> modulePath = null;
                    if (nestedModuleInfoIfAvailable != null)
                    {
                        modulePath = new string[] { Path.GetDirectoryName(nestedModuleInfoIfAvailable.ModuleBase), nestedModuleInfoIfAvailable.ModuleBase };
                    }
                    else
                    {
                        modulePath = ModuleIntrinsics.GetModulePath(false, base.Context);
                    }
                    found = this.LoadUsingModulePath(parentModule, found, modulePath, moduleSpecification.Name, ss, options, manifestProcessingFlags, out module);
                }
                if ((!found && !moduleSpecification.Guid.HasValue) && (moduleSpecification.Version == null))
                {
                    bool flag3 = true;
                    if (((parentModule != null) && ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) != ManifestProcessingFlags.LoadElements)) && ((parentModule.ExportedCmdlets != null) && (parentModule.ExportedCmdlets.Count > 0)))
                    {
                        flag3 = false;
                        foreach (string str7 in parentModule.ExportedCmdlets.Keys)
                        {
                            if (WildcardPattern.ContainsWildcardCharacters(str7))
                            {
                                flag3 = true;
                                break;
                            }
                        }
                        found = true;
                    }
                    if (flag3)
                    {
                        module = this.LoadBinaryModule(parentModule, true, moduleSpecification.Name, null, null, moduleBase, ss, options, manifestProcessingFlags, prefix, loadTypesFiles, loadFormatFiles, out found, shortModuleName, false);
                        if ((module != null) && ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) == ManifestProcessingFlags.LoadElements))
                        {
                            AddModuleToModuleTables(base.Context, this.TargetSessionState.Internal, module);
                        }
                    }
                }
                info3 = module;
            }
            finally
            {
                this.BaseMinimumVersion = baseMinimumVersion;
                this.BaseRequiredVersion = baseRequiredVersion;
                base.Context.Modules.DecrementModuleNestingCount();
            }
            return info3;
        }

        internal static PSModuleInfo LoadRequiredModule(ExecutionContext context, PSModuleInfo currentModule, ModuleSpecification requiredModuleSpecification, string moduleManifestPath, ManifestProcessingFlags manifestProcessingFlags, out ErrorRecord error)
        {
            error = null;
            string name = requiredModuleSpecification.Name;
            Guid? guid = requiredModuleSpecification.Guid;
            Version moduleVersion = requiredModuleSpecification.Version;
            PSModuleInfo info = null;
            bool wrongVersion = false;
            bool wrongGuid = false;
            bool loaded = false;
            object obj2 = IsModuleLoaded(context, requiredModuleSpecification, out wrongVersion, out wrongGuid, out loaded);
            if (obj2 == null)
            {
                PSModuleInfo info2 = null;
                Collection<PSModuleInfo> moduleIfAvailable = GetModuleIfAvailable(requiredModuleSpecification, null);
                if ((moduleIfAvailable != null) && (moduleIfAvailable.Count > 0))
                {
                    info2 = moduleIfAvailable[0];
                    Dictionary<ModuleSpecification, List<ModuleSpecification>> nonCyclicRequiredModules = new Dictionary<ModuleSpecification, List<ModuleSpecification>>(new ModuleSpecificationComparer());
                    if (currentModule != null)
                    {
                        nonCyclicRequiredModules.Add(new ModuleSpecification(currentModule), new List<ModuleSpecification> { requiredModuleSpecification });
                    }
                    Collection<PSModuleInfo> moduleInfoList = new Collection<PSModuleInfo> {
                        info2
                    };
                    if (!HasRequiredModulesCyclicReference(info2.Name, new List<ModuleSpecification>(info2.RequiredModulesSpecification), moduleInfoList, nonCyclicRequiredModules, out error))
                    {
                        return ImportRequiredModule(context, requiredModuleSpecification, moduleVersion, guid, out error);
                    }
                    if (moduleManifestPath != null)
                    {
                        MissingMemberException exception = null;
                        if ((error != null) && (error.Exception != null))
                        {
                            exception = new MissingMemberException(error.Exception.Message);
                        }
                        error = new ErrorRecord(exception, "Modules_InvalidManifest", ErrorCategory.ResourceUnavailable, moduleManifestPath);
                    }
                    return info;
                }
                if (moduleManifestPath != null)
                {
                    if ((manifestProcessingFlags & ManifestProcessingFlags.WriteErrors) != 0)
                    {
                        string str2;
                        if (wrongVersion)
                        {
                            str2 = StringUtil.Format(Modules.RequiredModuleNotLoadedWrongVersion, new object[] { moduleManifestPath, name, moduleVersion });
                        }
                        else if (wrongGuid)
                        {
                            str2 = StringUtil.Format(Modules.RequiredModuleNotLoadedWrongGuid, new object[] { moduleManifestPath, name, guid.Value });
                        }
                        else
                        {
                            str2 = StringUtil.Format(Modules.RequiredModuleNotLoaded, moduleManifestPath, name);
                        }
                        MissingMemberException exception2 = new MissingMemberException(str2);
                        error = new ErrorRecord(exception2, "Modules_InvalidManifest", ErrorCategory.ResourceUnavailable, moduleManifestPath);
                    }
                    return info;
                }
                MissingMemberException exception3 = new MissingMemberException(StringUtil.Format(Modules.RequiredModuleNotFound, requiredModuleSpecification.Name));
                error = new ErrorRecord(exception3, "Modules_RequiredModuleNotFound", ErrorCategory.ResourceUnavailable, null);
                return info;
            }
            if (obj2 is PSModuleInfo)
            {
                info = (PSModuleInfo) obj2;
            }
            return info;
        }

        internal PSModuleInfo LoadRequiredModule(PSModuleInfo currentModule, ModuleSpecification requiredModule, string moduleManifestPath, ManifestProcessingFlags manifestProcessingFlags, bool containedErrors, out ErrorRecord error)
        {
            error = null;
            if (!containedErrors)
            {
                return LoadRequiredModule(base.Context, currentModule, requiredModule, moduleManifestPath, manifestProcessingFlags, out error);
            }
            return null;
        }

        private PSModuleInfo LoadServiceCoreModule(PSModuleInfo parentModule, string moduleBase, SessionState ss, ImportModuleOptions nestedModuleOptions, ManifestProcessingFlags manifestProcessingFlags, bool addToParentModueIfFound, out bool found)
        {
            PSModuleInfo info2;
            SessionStateInternal engineSessionState = base.Context.EngineSessionState;
            if (ss != null)
            {
                base.Context.EngineSessionState = ss.Internal;
            }
            try
            {
                found = false;
                bool baseGlobal = this.BaseGlobal;
                this.BaseGlobal = false;
                PSModuleInfo nestedModule = this.LoadBinaryModule(parentModule, false, this.ServiceCoreAssemblyFullName, null, null, moduleBase, ss, nestedModuleOptions, manifestProcessingFlags, string.Empty, true, true, out found, this.ServiceCoreAssemblyShortName, true);
                this.BaseGlobal = baseGlobal;
                if (found)
                {
                    if (addToParentModueIfFound)
                    {
                        parentModule.AddNestedModule(nestedModule);
                    }
                    return nestedModule;
                }
                string message = StringUtil.Format(Modules.ManifestMemberNotFound, new object[] { this.ServiceCoreAssemblyFullName, "NestedModules", parentModule.Name });
                FileNotFoundException innerException = new FileNotFoundException(message);
                PSInvalidOperationException exception2 = new PSInvalidOperationException(message, innerException, "Modules_ModuleFileNotFound", ErrorCategory.ResourceUnavailable, parentModule.Name);
                throw exception2;
            }
            finally
            {
                base.Context.EngineSessionState = engineSessionState;
            }
            return info2;
        }

        internal PSModuleInfo LoadUsingExtensions(PSModuleInfo parentModule, string moduleName, string fileBaseName, string extension, string moduleBase, string prefix, SessionState ss, ImportModuleOptions options, ManifestProcessingFlags manifestProcessingFlags, out bool found)
        {
            bool moduleFileFound = false;
            return this.LoadUsingExtensions(parentModule, moduleName, fileBaseName, extension, moduleBase, prefix, ss, options, manifestProcessingFlags, out found, out moduleFileFound);
        }

        internal PSModuleInfo LoadUsingExtensions(PSModuleInfo parentModule, string moduleName, string fileBaseName, string extension, string moduleBase, string prefix, SessionState ss, ImportModuleOptions options, ManifestProcessingFlags manifestProcessingFlags, out bool found, out bool moduleFileFound)
        {
            string[] pSModuleExtensions;
            moduleFileFound = false;
            if (!string.IsNullOrEmpty(extension))
            {
                pSModuleExtensions = new string[] { extension };
            }
            else
            {
                pSModuleExtensions = ModuleIntrinsics.PSModuleExtensions;
            }
            foreach (string str in pSModuleExtensions)
            {
                string resolvedPath = GetResolvedPath(fileBaseName + str, base.Context);
                if ((resolvedPath != null) && (string.IsNullOrEmpty(base.Context.ModuleBeingProcessed) || !base.Context.ModuleBeingProcessed.Equals(resolvedPath, StringComparison.OrdinalIgnoreCase)))
                {
                    PSModuleInfo info;
                    base.Context.Modules.ModuleTable.TryGetValue(resolvedPath, out info);
                    if ((((!this.BaseForce && (info != null)) && ((this.BaseRequiredVersion == null) || info.Version.Equals(this.BaseRequiredVersion))) && ((this.BaseMinimumVersion == null) || (info.Version >= this.BaseMinimumVersion))) && ((manifestProcessingFlags & ManifestProcessingFlags.LoadElements) == ManifestProcessingFlags.LoadElements))
                    {
                        moduleFileFound = true;
                        info = base.Context.Modules.ModuleTable[resolvedPath];
                        if (string.IsNullOrEmpty(prefix))
                        {
                            string defaultPrefix = this.GetDefaultPrefix(info);
                            if (!string.IsNullOrEmpty(defaultPrefix))
                            {
                                prefix = defaultPrefix;
                            }
                        }
                        AddModuleToModuleTables(base.Context, this.TargetSessionState.Internal, info);
                        this.ImportModuleMembers(info, prefix, options);
                        if (this.BaseAsCustomObject)
                        {
                            if (info.ModuleType != ModuleType.Script)
                            {
                                InvalidOperationException exception = new InvalidOperationException(StringUtil.Format(Modules.CantUseAsCustomObjectWithBinaryModule, info.Path));
                                ErrorRecord errorRecord = new ErrorRecord(exception, "Modules_CantUseAsCustomObjectWithBinaryModule", ErrorCategory.PermissionDenied, null);
                                base.WriteError(errorRecord);
                            }
                            else
                            {
                                base.WriteObject(info.AsCustomObject());
                            }
                        }
                        else if (this.BasePassThru)
                        {
                            base.WriteObject(info);
                        }
                        found = true;
                        return info;
                    }
                    if (File.Exists(resolvedPath))
                    {
                        moduleFileFound = true;
                        if ((this.BaseForce && (info != null)) && ((this.BaseRequiredVersion == null) || info.Version.Equals(this.BaseRequiredVersion)))
                        {
                            this.RemoveModule(info);
                        }
                        info = this.LoadModule(parentModule, resolvedPath, moduleBase, prefix, ss, null, ref options, manifestProcessingFlags, out found, out moduleFileFound);
                        if (found)
                        {
                            return info;
                        }
                    }
                }
            }
            found = false;
            return null;
        }

        internal bool LoadUsingModulePath(bool found, IEnumerable<string> modulePath, string name, SessionState ss, ImportModuleOptions options, ManifestProcessingFlags manifestProcessingFlags, out PSModuleInfo module)
        {
            return this.LoadUsingModulePath(null, found, modulePath, name, ss, options, manifestProcessingFlags, out module);
        }

        internal bool LoadUsingModulePath(PSModuleInfo parentModule, bool found, IEnumerable<string> modulePath, string name, SessionState ss, ImportModuleOptions options, ManifestProcessingFlags manifestProcessingFlags, out PSModuleInfo module)
        {
            string str2;
            string extension = Path.GetExtension(name);
            module = null;
            if (string.IsNullOrEmpty(extension) || !ModuleIntrinsics.IsPowerShellModuleExtension(extension))
            {
                str2 = name;
                extension = null;
            }
            else
            {
                str2 = name.Substring(0, name.Length - extension.Length);
            }
            foreach (string str3 in modulePath)
            {
                string str4 = Path.Combine(str3, str2);
                if (name.IndexOfAny(Utils.DirectorySeparators) == -1)
                {
                    str4 = Path.Combine(str4, str2);
                }
                else if (Directory.Exists(str4))
                {
                    str4 = Path.Combine(str4, Path.GetFileName(str2));
                }
                module = this.LoadUsingExtensions(parentModule, name, str4, extension, null, this.BasePrefix, ss, options, manifestProcessingFlags, out found);
                if (found)
                {
                    if (module != null)
                    {
                        if (((module.ExportedWorkflows != null) && (module.ExportedWorkflows.Count > 0)) && Utils.IsRunningFromSysWOW64())
                        {
                            throw new NotSupportedException(AutomationExceptions.WorkflowDoesNotSupportWOW64);
                        }
                        bool flag = (manifestProcessingFlags & ManifestProcessingFlags.LoadElements) == ManifestProcessingFlags.LoadElements;
                        bool flag2 = (manifestProcessingFlags & ManifestProcessingFlags.Force) == ManifestProcessingFlags.Force;
                        bool force = flag || flag2;
                        AnalysisCache.CacheExportedCommands(module, force, base.Context);
                    }
                    return found;
                }
            }
            return found;
        }

        private void ProcessWorkflowsToProcess(string moduleBase, List<string> workflowsToProcess, List<string> dependentWorkflows, List<string> assemblyList, SessionState ss, PSModuleInfo manifestInfo, ImportModuleOptions options)
        {
            if (ss != null)
            {
                if ((workflowsToProcess != null) && (workflowsToProcess.Count > 0))
                {
                    if (((SystemPolicy.GetSystemLockdownPolicy() == SystemEnforcementMode.Enforce) || (base.Context.LanguageMode == PSLanguageMode.ConstrainedLanguage)) && !SystemPolicy.XamlWorkflowSupported)
                    {
                        throw new NotSupportedException(Modules.XamlWorkflowsNotSupported);
                    }
                    SessionStateInternal engineSessionState = base.Context.EngineSessionState;
                    try
                    {
                        base.Context.EngineSessionState = ss.Internal;
                        if ((dependentWorkflows != null) && (dependentWorkflows.Count > 0))
                        {
                            ScriptBlock block = ScriptBlock.Create(base.Context, "param($files, $dependentFiles, $assemblyList) Microsoft.PowerShell.Workflow.ServiceCore\\Import-PSWorkflow -Path \"$files\" -DependentWorkflow $dependentFiles -DependentAssemblies $assemblyList -Force:$" + this.BaseForce);
                            List<string> list = new List<string>(this.ResolveDependentWorkflowFiles(moduleBase, dependentWorkflows));
                            foreach (string str in this.ResolveWorkflowFiles(moduleBase, workflowsToProcess))
                            {
                                base.WriteVerbose(StringUtil.Format(Modules.LoadingWorkflow, str));
                                block.Invoke(new object[] { str, list.ToArray(), assemblyList.ToArray() });
                            }
                        }
                        else
                        {
                            ScriptBlock block2 = ScriptBlock.Create(base.Context, "param($files, $dependentFiles) Microsoft.PowerShell.Workflow.ServiceCore\\Import-PSWorkflow -Path \"$files\" -Force:$" + this.BaseForce);
                            foreach (string str2 in this.ResolveWorkflowFiles(moduleBase, workflowsToProcess))
                            {
                                base.WriteVerbose(StringUtil.Format(Modules.LoadingWorkflow, str2));
                                block2.Invoke(new object[] { str2 });
                            }
                        }
                        ModuleIntrinsics.ExportModuleMembers(this, ss.Internal, this.MatchAll, this.MatchAll, this.MatchAll, this.MatchAll, options.ServiceCoreAutoAdded ? ServiceCoreAssemblyCmdlets : null);
                    }
                    finally
                    {
                        base.Context.EngineSessionState = engineSessionState;
                    }
                }
            }
            else if (workflowsToProcess != null)
            {
                foreach (string str3 in workflowsToProcess)
                {
                    manifestInfo.AddDetectedWorkflowExport(Path.GetFileNameWithoutExtension(str3));
                }
            }
        }

        internal void RemoveModule(PSModuleInfo module)
        {
            this.RemoveModule(module, null);
        }

        internal void RemoveModule(PSModuleInfo module, string moduleNameInRemoveModuleCmdlet)
        {
            bool isTopLevelModule = false;
            if (this.ShouldModuleBeRemoved(module, moduleNameInRemoveModuleCmdlet, out isTopLevelModule) && base.Context.Modules.ModuleTable.ContainsKey(module.Path))
            {
                if (module.OnRemove != null)
                {
                    module.OnRemove.InvokeUsingCmdlet(this, true, ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe, null, null, null, new object[] { module });
                }
                List<string> list = new List<string>();
                foreach (KeyValuePair<string, List<CmdletInfo>> pair in base.Context.EngineSessionState.GetCmdletTable())
                {
                    List<CmdletInfo> list2 = pair.Value;
                    for (int i = list2.Count - 1; i >= 0; i--)
                    {
                        if ((list2[i].Module != null) && list2[i].Module.Path.Equals(module.Path, StringComparison.OrdinalIgnoreCase))
                        {
                            string name = list2[i].Name;
                            list2.RemoveAt(i);
                            base.Context.EngineSessionState.RemoveCmdlet(name, i, true);
                        }
                    }
                    if (list2.Count == 0)
                    {
                        list.Add(pair.Key);
                    }
                }
                foreach (string str2 in list)
                {
                    base.Context.EngineSessionState.RemoveCmdletEntry(str2, true);
                }
                if (module.ModuleType == ModuleType.Binary)
                {
                    Dictionary<string, List<ProviderInfo>> providers = base.Context.TopLevelSessionState.Providers;
                    List<string> list3 = new List<string>();
                    foreach (KeyValuePair<string, List<ProviderInfo>> pair2 in providers)
                    {
                        for (int j = pair2.Value.Count - 1; j >= 0; j--)
                        {
                            ProviderInfo pi = pair2.Value[j];
                            if (pi.ImplementingType.Assembly.Location.Equals(module.Path, StringComparison.OrdinalIgnoreCase))
                            {
                                InitialSessionState.RemoveAllDrivesForProvider(pi, base.Context.TopLevelSessionState);
                                if (base.Context.EngineSessionState != base.Context.TopLevelSessionState)
                                {
                                    InitialSessionState.RemoveAllDrivesForProvider(pi, base.Context.EngineSessionState);
                                }
                                foreach (PSModuleInfo info2 in base.Context.Modules.ModuleTable.Values)
                                {
                                    if (info2.SessionState != null)
                                    {
                                        SessionStateInternal internal2 = info2.SessionState.Internal;
                                        if ((internal2 != base.Context.TopLevelSessionState) && (internal2 != base.Context.EngineSessionState))
                                        {
                                            InitialSessionState.RemoveAllDrivesForProvider(pi, base.Context.EngineSessionState);
                                        }
                                    }
                                }
                                pair2.Value.RemoveAt(j);
                            }
                        }
                        if (pair2.Value.Count == 0)
                        {
                            list3.Add(pair2.Key);
                        }
                    }
                    foreach (string str3 in list3)
                    {
                        providers.Remove(str3);
                    }
                }
                SessionStateInternal engineSessionState = base.Context.EngineSessionState;
                if (module.SessionState != null)
                {
                    foreach (DictionaryEntry entry in engineSessionState.GetFunctionTable())
                    {
                        FunctionInfo info3 = (FunctionInfo) entry.Value;
                        if ((info3.Module != null) && info3.Module.Path.Equals(module.Path, StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                engineSessionState.RemoveFunction(info3.Name, true);
                                string text = StringUtil.Format(Modules.RemovingImportedFunction, info3.Name);
                                base.WriteVerbose(text);
                            }
                            catch (SessionStateUnauthorizedAccessException exception)
                            {
                                InvalidOperationException exception2 = new InvalidOperationException(StringUtil.Format(Modules.UnableToRemoveModuleMember, new object[] { info3.Name, module.Name, exception.Message }), exception);
                                ErrorRecord errorRecord = new ErrorRecord(exception2, "Modules_MemberNotRemoved", ErrorCategory.PermissionDenied, info3.Name);
                                base.WriteError(errorRecord);
                            }
                        }
                    }
                    foreach (PSVariable variable in module.SessionState.Internal.ExportedVariables)
                    {
                        PSVariable variable2 = engineSessionState.GetVariable(variable.Name);
                        if ((variable2 != null) && (variable2 == variable))
                        {
                            engineSessionState.RemoveVariable(variable2, this.BaseForce);
                            string str6 = StringUtil.Format(Modules.RemovingImportedVariable, variable2.Name);
                            base.WriteVerbose(str6);
                        }
                    }
                    foreach (KeyValuePair<string, AliasInfo> pair3 in engineSessionState.GetAliasTable())
                    {
                        AliasInfo info4 = pair3.Value;
                        if ((info4.Module != null) && info4.Module.Path.Equals(module.Path, StringComparison.OrdinalIgnoreCase))
                        {
                            engineSessionState.RemoveAlias(info4.Name, true);
                            string str7 = StringUtil.Format(Modules.RemovingImportedAlias, info4.Name);
                            base.WriteVerbose(str7);
                        }
                    }
                }
                this.RemoveTypesAndFormatting(module.ExportedFormatFiles, module.ExportedTypeFiles);
                base.Context.HelpSystem.ResetHelpProviders();
                foreach (KeyValuePair<string, PSModuleInfo> pair4 in base.Context.Modules.ModuleTable)
                {
                    PSModuleInfo info5 = pair4.Value;
                    if ((info5.SessionState != null) && info5.SessionState.Internal.ModuleTable.ContainsKey(module.Path))
                    {
                        info5.SessionState.Internal.ModuleTable.Remove(module.Path);
                        info5.SessionState.Internal.ModuleTableKeys.Remove(module.Path);
                    }
                }
                if (isTopLevelModule)
                {
                    base.Context.TopLevelSessionState.ModuleTable.Remove(module.Path);
                    base.Context.TopLevelSessionState.ModuleTableKeys.Remove(module.Path);
                }
                base.Context.Modules.ModuleTable.Remove(module.Path);
                PSModuleInfo.RemoveFromAppDomainLevelCache(module.Name);
            }
        }

        internal static string RemovePrefixFromCommandName(string commandName, string prefix)
        {
            string str = commandName;
            if (!string.IsNullOrEmpty(prefix))
            {
                string str2;
                string str3;
                if (CmdletInfo.SplitCmdletName(commandName, out str2, out str3))
                {
                    if (str3.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        string str4 = str3.Substring(prefix.Length, str3.Length - prefix.Length);
                        str = str2 + "-" + str4;
                    }
                    return str;
                }
                if (commandName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    str = commandName.Substring(prefix.Length, commandName.Length - prefix.Length);
                }
            }
            return str;
        }

        private void RemoveTypesAndFormatting(IEnumerable<string> formatFilesToRemove, IEnumerable<string> typeFilesToRemove)
        {
            try
            {
                if (base.Context.InitialSessionState != null)
                {
                    bool refreshTypeAndFormatSetting = base.Context.InitialSessionState.RefreshTypeAndFormatSetting;
                    try
                    {
                        base.Context.InitialSessionState.RefreshTypeAndFormatSetting = true;
                        InitialSessionState.RemoveTypesAndFormats(base.Context, formatFilesToRemove, typeFilesToRemove);
                        return;
                    }
                    finally
                    {
                        base.Context.InitialSessionState.RefreshTypeAndFormatSetting = refreshTypeAndFormatSetting;
                    }
                }
                if ((formatFilesToRemove != null) && formatFilesToRemove.Any<string>())
                {
                    HashSet<string> set = new HashSet<string>(formatFilesToRemove, StringComparer.OrdinalIgnoreCase);
                    List<int> list = new List<int>();
                    for (int i = 0; i < base.Context.RunspaceConfiguration.Formats.Count; i++)
                    {
                        string fileName = base.Context.RunspaceConfiguration.Formats[i].FileName;
                        if ((fileName != null) && set.Contains(fileName))
                        {
                            list.Add(i);
                        }
                    }
                    for (int j = list.Count - 1; j >= 0; j--)
                    {
                        base.Context.RunspaceConfiguration.Formats.RemoveItem(list[j]);
                    }
                    base.Context.RunspaceConfiguration.Formats.Update();
                }
                if ((typeFilesToRemove != null) && typeFilesToRemove.Any<string>())
                {
                    HashSet<string> set2 = new HashSet<string>(typeFilesToRemove, StringComparer.OrdinalIgnoreCase);
                    List<int> list2 = new List<int>();
                    for (int k = 0; k < base.Context.RunspaceConfiguration.Types.Count; k++)
                    {
                        string item = base.Context.RunspaceConfiguration.Types[k].FileName;
                        if ((item != null) && set2.Contains(item))
                        {
                            list2.Add(k);
                        }
                    }
                    for (int m = list2.Count - 1; m >= 0; m--)
                    {
                        base.Context.RunspaceConfiguration.Types.RemoveItem(list2[m]);
                    }
                    base.Context.RunspaceConfiguration.Types.Update();
                }
            }
            catch (RuntimeException exception)
            {
                string fullyQualifiedErrorId = exception.ErrorRecord.FullyQualifiedErrorId;
                if (!fullyQualifiedErrorId.Equals("ErrorsUpdatingTypes", StringComparison.Ordinal) && !fullyQualifiedErrorId.Equals("ErrorsUpdatingFormats", StringComparison.Ordinal))
                {
                    throw;
                }
            }
        }

        private ICollection<string> ResolveDependentWorkflowFiles(string moduleBase, List<string> dependentWorkflowsToProcess)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if ((dependentWorkflowsToProcess.Count == 1) && string.Equals(Path.GetExtension(dependentWorkflowsToProcess[0]), ".dll", StringComparison.OrdinalIgnoreCase))
            {
                string path = dependentWorkflowsToProcess[0];
                if (!Path.IsPathRooted(path))
                {
                    path = Path.Combine(moduleBase, path);
                }
                dictionary[path] = path;
            }
            else
            {
                foreach (string str2 in dependentWorkflowsToProcess)
                {
                    if (string.Equals(Path.GetExtension(str2), ".xaml", StringComparison.OrdinalIgnoreCase))
                    {
                        string str3 = str2;
                        if (!Path.IsPathRooted(str3))
                        {
                            str3 = Path.Combine(moduleBase, str3);
                        }
                        foreach (string str4 in base.Context.SessionState.Path.GetResolvedProviderPathFromProviderPath(str3, base.Context.ProviderNames.FileSystem))
                        {
                            dictionary[str4] = str4;
                        }
                    }
                    else
                    {
                        PSInvalidOperationException exception = PSTraceSource.NewInvalidOperationException("Modules", "InvalidWorkflowExtensionDuringManifestProcessing", new object[] { str2 });
                        exception.SetErrorId("Modules_InvalidWorkflowExtensionDuringManifestProcessing");
                        throw exception;
                    }
                }
            }
            return dictionary.Values;
        }

        internal static string ResolveRootedFilePath(string filePath, ExecutionContext context)
        {
            if (!IsRooted(filePath))
            {
                return null;
            }
            ProviderInfo provider = null;
            Collection<string> targetObject = null;
            if (context.EngineSessionState.IsProviderLoaded(context.ProviderNames.FileSystem))
            {
                try
                {
                    targetObject = context.SessionState.Path.GetResolvedProviderPathFromPSPath(filePath, out provider);
                }
                catch (ItemNotFoundException)
                {
                    return null;
                }
                if (!provider.NameEquals(context.ProviderNames.FileSystem))
                {
                    throw InterpreterError.NewInterpreterException(filePath, typeof(RuntimeException), null, "FileOpenError", ParserStrings.FileOpenError, new object[] { provider.FullName });
                }
            }
            if ((targetObject == null) || (targetObject.Count < 1))
            {
                return null;
            }
            if (targetObject.Count > 1)
            {
                throw InterpreterError.NewInterpreterException(targetObject, typeof(RuntimeException), null, "AmbiguousPath", ParserStrings.AmbiguousPath, new object[0]);
            }
            return targetObject[0];
        }

        private ICollection<string> ResolveWorkflowFiles(string moduleBase, List<string> workflowsToProcess)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (string str in workflowsToProcess)
            {
                if (string.Equals(Path.GetExtension(str), ".xaml", StringComparison.OrdinalIgnoreCase))
                {
                    string path = str;
                    if (!Path.IsPathRooted(path))
                    {
                        path = Path.Combine(moduleBase, path);
                    }
                    foreach (string str3 in base.Context.SessionState.Path.GetResolvedProviderPathFromProviderPath(path, base.Context.ProviderNames.FileSystem))
                    {
                        dictionary[str3] = str3;
                    }
                }
                else
                {
                    PSInvalidOperationException exception = PSTraceSource.NewInvalidOperationException("Modules", "InvalidWorkflowExtensionDuringManifestProcessing", new object[] { str });
                    exception.SetErrorId("Modules_InvalidWorkflowExtensionDuringManifestProcessing");
                    throw exception;
                }
            }
            return dictionary.Values;
        }

        private static void SetCommandVisibility(bool isImportModulePrivate, IHasSessionStateEntryVisibility command)
        {
            if (isImportModulePrivate)
            {
                command.Visibility = SessionStateEntryVisibility.Private;
            }
        }

        internal void SetModuleLoggingInformation(PSModuleInfo m)
        {
            foreach (ExecutionPolicyScope scope in SecuritySupport.ExecutionPolicyScopePreferences)
            {
                IEnumerable<string> enumerable;
                ModuleLoggingGroupPolicyStatus moduleLoggingInformation = GetModuleLoggingInformation(scope, out enumerable);
                if (moduleLoggingInformation != ModuleLoggingGroupPolicyStatus.Undefined)
                {
                    this.SetModuleLoggingInformation(moduleLoggingInformation, m, enumerable);
                    return;
                }
            }
        }

        private void SetModuleLoggingInformation(ModuleLoggingGroupPolicyStatus status, PSModuleInfo m, IEnumerable<string> moduleNames)
        {
            ModuleIntrinsics.GetSystemwideModulePath();
            if (((status & ModuleLoggingGroupPolicyStatus.Enabled) != ModuleLoggingGroupPolicyStatus.Undefined) && (moduleNames != null))
            {
                foreach (string str in moduleNames)
                {
                    if (string.Equals(m.Name, str, StringComparison.OrdinalIgnoreCase))
                    {
                        m.LogPipelineExecutionDetails = true;
                    }
                    else if (WildcardPattern.ContainsWildcardCharacters(str))
                    {
                        WildcardPattern pattern = new WildcardPattern(str, WildcardOptions.IgnoreCase);
                        if (pattern.IsMatch(m.Name))
                        {
                            m.LogPipelineExecutionDetails = true;
                        }
                    }
                }
            }
        }

        private bool ShouldModuleBeRemoved(PSModuleInfo module, string moduleNameInRemoveModuleCmdlet, out bool isTopLevelModule)
        {
            isTopLevelModule = false;
            if (base.Context.TopLevelSessionState.ModuleTable.ContainsKey(module.Path))
            {
                isTopLevelModule = true;
                if ((moduleNameInRemoveModuleCmdlet != null) && !module.Name.Equals(moduleNameInRemoveModuleCmdlet, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool ShouldProcessScriptModule(PSModuleInfo parentModule, ref bool found)
        {
            bool flag = true;
            if (((parentModule != null) && flag) && ((parentModule.ExportedFunctions != null) && (parentModule.ExportedFunctions.Count > 0)))
            {
                flag = false;
                foreach (string str in parentModule.ExportedFunctions.Keys)
                {
                    if (WildcardPattern.ContainsWildcardCharacters(str))
                    {
                        flag = true;
                        break;
                    }
                }
                found = true;
            }
            return flag;
        }

        private static void UpdateCommandCollection<T>(List<T> list, List<WildcardPattern> patterns) where T: CommandInfo
        {
            List<T> collection = new List<T>();
            foreach (T local in list)
            {
                if (SessionStateUtilities.MatchesAnyWildcardPattern(local.Name, patterns, false))
                {
                    collection.Add(local);
                }
            }
            list.Clear();
            list.AddRange(collection);
        }

        private static void UpdateCommandCollection(Collection<string> list, List<WildcardPattern> patterns)
        {
            if (list != null)
            {
                List<string> list2 = new List<string>();
                foreach (WildcardPattern pattern in patterns)
                {
                    if (!WildcardPattern.ContainsWildcardCharacters(pattern.Pattern) && !list.Contains<string>(pattern.Pattern, StringComparer.OrdinalIgnoreCase))
                    {
                        list.Add(pattern.Pattern);
                    }
                }
                foreach (string str in list)
                {
                    if (SessionStateUtilities.MatchesAnyWildcardPattern(str, patterns, false))
                    {
                        list2.Add(str);
                    }
                }
                list.Clear();
                foreach (string str2 in list2)
                {
                    list.Add(str2);
                }
            }
        }

        private static void ValidateCommandName(ModuleCmdletBase cmdlet, string commandName, string moduleName, ref bool checkVerb, ref bool checkNoun)
        {
            string str;
            string str2;
            if (CmdletInfo.SplitCmdletName(commandName, out str, out str2))
            {
                string importingNonStandardNoun;
                if ((!Verbs.IsStandard(str) && !commandName.Equals("Sort-Object", StringComparison.OrdinalIgnoreCase)) && !commandName.Equals("Tee-Object", StringComparison.OrdinalIgnoreCase))
                {
                    if (checkVerb)
                    {
                        checkVerb = false;
                        importingNonStandardNoun = StringUtil.Format(Modules.ImportingNonStandardVerb, moduleName);
                        cmdlet.WriteWarning(importingNonStandardNoun);
                    }
                    string[] strArray = Verbs.SuggestedAlternates(str);
                    if (strArray == null)
                    {
                        importingNonStandardNoun = StringUtil.Format(Modules.ImportingNonStandardVerbVerbose, commandName, moduleName);
                        cmdlet.WriteVerbose(importingNonStandardNoun);
                    }
                    else
                    {
                        string listSeparator = ExtendedTypeSystem.ListSeparator;
                        StringBuilder builder = new StringBuilder();
                        foreach (string str5 in strArray)
                        {
                            builder.Append(str5);
                            builder.Append(listSeparator);
                        }
                        builder.Remove(builder.Length - listSeparator.Length, listSeparator.Length);
                        importingNonStandardNoun = StringUtil.Format(Modules.ImportingNonStandardVerbVerboseSuggestion, new object[] { commandName, builder, moduleName });
                        cmdlet.WriteVerbose(importingNonStandardNoun);
                    }
                }
                if (HasInvalidCharacters(str2))
                {
                    if (checkNoun)
                    {
                        importingNonStandardNoun = Modules.ImportingNonStandardNoun;
                        cmdlet.WriteWarning(importingNonStandardNoun);
                        checkNoun = false;
                    }
                    importingNonStandardNoun = StringUtil.Format(Modules.ImportingNonStandardNounVerbose, commandName, moduleName);
                    cmdlet.WriteVerbose(importingNonStandardNoun);
                }
            }
        }

        private bool ValidateManifestHash(Hashtable data, string[] validMembers, string moduleManifestPath, ManifestProcessingFlags manifestProcessingFlags)
        {
            bool flag = true;
            StringBuilder builder = new StringBuilder();
            foreach (string str in data.Keys)
            {
                bool flag2 = false;
                foreach (string str2 in validMembers)
                {
                    if (str.Equals(str2, StringComparison.OrdinalIgnoreCase))
                    {
                        flag2 = true;
                    }
                }
                if (!flag2)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(", ");
                    }
                    builder.Append("'");
                    builder.Append(str);
                    builder.Append("'");
                }
            }
            if (builder.Length > 0)
            {
                Version version;
                flag = false;
                if ((manifestProcessingFlags & ManifestProcessingFlags.WriteErrors) == 0)
                {
                    return flag;
                }
                Version pSVersion = PSVersionInfo.PSVersion;
                if (this.GetScalarFromData<Version>(data, moduleManifestPath, "PowerShellVersion", manifestProcessingFlags, out version) && (pSVersion < version))
                {
                    InvalidOperationException exception = new InvalidOperationException(StringUtil.Format(Modules.ModuleManifestInsufficientPowerShellVersion, new object[] { pSVersion, moduleManifestPath, version }));
                    ErrorRecord record = new ErrorRecord(exception, "Modules_InsufficientPowerShellVersion", ErrorCategory.ResourceUnavailable, moduleManifestPath);
                    base.WriteError(record);
                    return flag;
                }
                StringBuilder builder2 = new StringBuilder("'");
                builder2.Append(validMembers[0]);
                for (int i = 1; i < validMembers.Length; i++)
                {
                    builder2.Append("', '");
                    builder2.Append(validMembers[i]);
                }
                builder2.Append("'");
                InvalidOperationException exception2 = new InvalidOperationException(StringUtil.Format(Modules.InvalidModuleManifestMember, new object[] { moduleManifestPath, builder2, builder }));
                ErrorRecord errorRecord = new ErrorRecord(exception2, "Modules_InvalidManifestMember", ErrorCategory.InvalidData, moduleManifestPath);
                base.WriteError(errorRecord);
            }
            return flag;
        }

        internal bool VerifyIfNestedModuleIsAvailable(ModuleSpecification nestedModuleSpec, string rootedModulePath, string extension, out PSModuleInfo nestedModuleInfoIfAvailable)
        {
            nestedModuleInfoIfAvailable = null;
            if (nestedModuleSpec.Guid.HasValue || (nestedModuleSpec.Version != null))
            {
                if (!string.IsNullOrEmpty(extension) && !string.Equals(extension, ".psd1"))
                {
                    return false;
                }
                string str = rootedModulePath;
                if (string.IsNullOrEmpty(extension))
                {
                    str = rootedModulePath + ".psd1";
                }
                ModuleSpecification requiredModule = new ModuleSpecification(string.IsNullOrEmpty(rootedModulePath) ? nestedModuleSpec.Name : str);
                if (nestedModuleSpec.Guid.HasValue)
                {
                    requiredModule.Guid = new Guid?(nestedModuleSpec.Guid.Value);
                }
                if (nestedModuleSpec.Version != null)
                {
                    requiredModule.Version = nestedModuleSpec.Version;
                }
                Collection<PSModuleInfo> moduleIfAvailable = GetModuleIfAvailable(requiredModule, null);
                if (moduleIfAvailable.Count != 1)
                {
                    return false;
                }
                nestedModuleInfoIfAvailable = moduleIfAvailable[0];
            }
            return true;
        }

        private static void WriteInvalidManifestMemberError(PSCmdlet cmdlet, string manifestElement, string moduleManifestPath, Exception e, ManifestProcessingFlags manifestProcessingFlags)
        {
            CommandProcessorBase.CheckForSevereException(e);
            if ((manifestProcessingFlags & ManifestProcessingFlags.WriteErrors) != 0)
            {
                ErrorRecord errorRecord = GenerateInvalidModuleMemberErrorRecord(manifestElement, moduleManifestPath, e);
                cmdlet.WriteError(errorRecord);
            }
        }

        protected bool AddToAppDomainLevelCache
        {
            get
            {
                return this._addToAppDomainLevelCache;
            }
            set
            {
                this._addToAppDomainLevelCache = value;
            }
        }

        internal List<WildcardPattern> BaseAliasPatterns
        {
            get
            {
                return this._aliasPatterns;
            }
            set
            {
                this._aliasPatterns = value;
            }
        }

        protected object[] BaseArgumentList
        {
            get
            {
                return this._arguments;
            }
            set
            {
                this._arguments = value;
            }
        }

        internal bool BaseAsCustomObject
        {
            get
            {
                return this._baseAsCustomObject;
            }
            set
            {
                this._baseAsCustomObject = value;
            }
        }

        internal List<WildcardPattern> BaseCmdletPatterns
        {
            get
            {
                return this._cmdletPatterns;
            }
            set
            {
                this._cmdletPatterns = value;
            }
        }

        protected bool BaseDisableNameChecking
        {
            get
            {
                return this._disableNameChecking;
            }
            set
            {
                this._disableNameChecking = value;
            }
        }

        internal bool BaseForce
        {
            get
            {
                return this._force;
            }
            set
            {
                this._force = value;
            }
        }

        internal List<WildcardPattern> BaseFunctionPatterns
        {
            get
            {
                return this._functionPatterns;
            }
            set
            {
                this._functionPatterns = value;
            }
        }

        internal bool BaseGlobal
        {
            get
            {
                return this._global;
            }
            set
            {
                this._global = value;
            }
        }

        internal Version BaseMinimumVersion
        {
            get
            {
                return this._minimumVersion;
            }
            set
            {
                this._minimumVersion = value;
            }
        }

        internal bool BasePassThru
        {
            get
            {
                return this._passThru;
            }
            set
            {
                this._passThru = value;
            }
        }

        internal string BasePrefix
        {
            get
            {
                return this._prefix;
            }
            set
            {
                this._prefix = value;
            }
        }

        internal Version BaseRequiredVersion
        {
            get
            {
                return this._requiredVersion;
            }
            set
            {
                this._requiredVersion = value;
            }
        }

        internal List<WildcardPattern> BaseVariablePatterns
        {
            get
            {
                return this._variablePatterns;
            }
            set
            {
                this._variablePatterns = value;
            }
        }

        internal List<WildcardPattern> MatchAll
        {
            get
            {
                if (this._matchAll == null)
                {
                    this._matchAll = new List<WildcardPattern>();
                    this._matchAll.Add(new WildcardPattern("*", WildcardOptions.IgnoreCase));
                }
                return this._matchAll;
            }
        }

        internal SessionState TargetSessionState
        {
            get
            {
                if (this.BaseGlobal)
                {
                    return base.Context.TopLevelSessionState.PublicSessionState;
                }
                return base.Context.SessionState;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal protected struct ImportModuleOptions
        {
            internal bool NoClobber;
            internal bool Local;
            internal bool ServiceCoreAutoAdded;
        }

        [Flags]
        internal enum ManifestProcessingFlags
        {
            Force = 0x10,
            LoadElements = 4,
            NullOnFirstError = 2,
            WriteErrors = 1,
            WriteWarnings = 8
        }

        [Flags]
        internal enum ModuleLoggingGroupPolicyStatus
        {
            Undefined,
            Enabled,
            Disabled
        }
    }
}

