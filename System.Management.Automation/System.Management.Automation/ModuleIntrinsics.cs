namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Language;
    using System.Runtime.InteropServices;
    using System.Security;

    internal class ModuleIntrinsics
    {
        private readonly ExecutionContext _context;
        private int _moduleNestingDepth;
        private readonly Dictionary<string, PSModuleInfo> _moduleTable = new Dictionary<string, PSModuleInfo>(StringComparer.OrdinalIgnoreCase);
        private const int MaxModuleNestingDepth = 10;
        internal static string[] PSModuleExtensions = new string[] { ".psd1", ".psm1", ".cdxml", ".xaml", ".dll" };
        internal static string[] PSModuleProcessableExtensions = new string[] { ".psd1", ".ps1", ".psm1", ".cdxml", ".xaml", ".dll" };
        [TraceSource("Modules", "Module loading and analysis")]
        internal static PSTraceSource Tracer = PSTraceSource.GetTracer("Modules", "Module loading and analysis");

        internal ModuleIntrinsics(ExecutionContext context)
        {
            this._context = context;
            SetModulePath();
        }

        internal ScriptBlock CreateBoundScriptBlock(ExecutionContext context, ScriptBlock sb, bool linkToGlobal)
        {
            PSModuleInfo info = new PSModuleInfo(context, linkToGlobal);
            return info.NewBoundScriptBlock(sb, context);
        }

        internal PSModuleInfo CreateModule(string path, ExternalScriptInfo scriptInfo, IScriptExtent scriptPosition, SessionState ss, object privateData, params object[] arguments)
        {
            ArrayList list;
            return this.CreateModuleImplementation(GetModuleName(path), path, scriptInfo, scriptPosition, ss, privateData, out list, arguments);
        }

        internal PSModuleInfo CreateModule(string name, string path, ScriptBlock scriptBlock, SessionState ss, out ArrayList results, params object[] arguments)
        {
            return this.CreateModuleImplementation(name, path, scriptBlock, null, ss, null, out results, arguments);
        }

        private PSModuleInfo CreateModuleImplementation(string name, string path, object moduleCode, IScriptExtent scriptPosition, SessionState ss, object privateData, out ArrayList result, params object[] arguments)
        {
            if (ss == null)
            {
                ss = new SessionState(this._context, true, true);
            }
            SessionStateInternal engineSessionState = this._context.EngineSessionState;
            PSModuleInfo info = new PSModuleInfo(name, path, this._context, ss);
            ss.Internal.Module = info;
            info.PrivateData = privateData;
            bool flag = false;
            int newValue = 0;
            try
            {
                ScriptBlock scriptBlock;
                this._context.EngineSessionState = ss.Internal;
                ExternalScriptInfo scriptCommandInfo = moduleCode as ExternalScriptInfo;
                if (scriptCommandInfo != null)
                {
                    scriptBlock = scriptCommandInfo.ScriptBlock;
                    this._context.Debugger.RegisterScriptFile(scriptCommandInfo);
                }
                else
                {
                    scriptBlock = moduleCode as ScriptBlock;
                    if (scriptBlock != null)
                    {
                        PSLanguageMode? languageMode = scriptBlock.LanguageMode;
                        scriptBlock = scriptBlock.Clone(true);
                        scriptBlock.LanguageMode = languageMode;
                        scriptBlock.SessionState = ss;
                    }
                    else if (moduleCode is string)
                    {
                        scriptBlock = ScriptBlock.Create(this._context, (string) moduleCode);
                    }
                }
                if (scriptBlock == null)
                {
                    throw PSTraceSource.NewInvalidOperationException();
                }
                scriptBlock.SessionStateInternal = ss.Internal;
                InvocationInfo invocationInfo = new InvocationInfo(scriptCommandInfo, scriptPosition);
                info._definitionExtent = scriptBlock.Ast.Extent;
                ArrayList resultList = new ArrayList();
                try
                {
                    Pipe outputPipe = new Pipe(resultList);
                    scriptBlock.InvokeWithPipe(false, ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe, AutomationNull.Value, AutomationNull.Value, AutomationNull.Value, outputPipe, invocationInfo, arguments ?? new object[0]);
                }
                catch (ExitException exception)
                {
                    newValue = (int) exception.Argument;
                    flag = true;
                }
                result = resultList;
            }
            finally
            {
                this._context.EngineSessionState = engineSessionState;
            }
            if (flag)
            {
                this._context.SetVariable(SpecialVariables.LastExitCodeVarPath, newValue);
            }
            return info;
        }

        internal void DecrementModuleNestingCount()
        {
            this._moduleNestingDepth--;
        }

        internal static void ExportModuleMembers(PSCmdlet cmdlet, SessionStateInternal sessionState, List<WildcardPattern> functionPatterns, List<WildcardPattern> cmdletPatterns, List<WildcardPattern> aliasPatterns, List<WildcardPattern> variablePatterns, List<string> doNotExportCmdlets)
        {
            sessionState.UseExportList = true;
            if (functionPatterns != null)
            {
                foreach (KeyValuePair<string, FunctionInfo> pair in sessionState.ModuleScope.FunctionTable)
                {
                    if (((pair.Value.Options & ScopedItemOptions.AllScope) == ScopedItemOptions.None) && SessionStateUtilities.MatchesAnyWildcardPattern(pair.Key, functionPatterns, false))
                    {
                        string str;
                        if (pair.Value.CommandType == CommandTypes.Workflow)
                        {
                            str = StringUtil.Format(Modules.ExportingWorkflow, pair.Key);
                            sessionState.ExportedWorkflows.Add((WorkflowInfo) pair.Value);
                        }
                        else
                        {
                            str = StringUtil.Format(Modules.ExportingFunction, pair.Key);
                            sessionState.ExportedFunctions.Add(pair.Value);
                        }
                        cmdlet.WriteVerbose(str);
                    }
                }
                SortAndRemoveDuplicates<FunctionInfo>(sessionState.ExportedFunctions, ci => ci.Name);
                SortAndRemoveDuplicates<WorkflowInfo>(sessionState.ExportedWorkflows, ci => ci.Name);
            }
            if (cmdletPatterns != null)
            {
                IDictionary<string, List<CmdletInfo>> cmdletTable = sessionState.ModuleScope.CmdletTable;
                if (sessionState.Module.CompiledExports.Count > 0)
                {
                    CmdletInfo[] infoArray = sessionState.Module.CompiledExports.ToArray();
                    sessionState.Module.CompiledExports.Clear();
                    CmdletInfo[] infoArray2 = infoArray;
                    for (int i = 0; i < infoArray2.Length; i++)
                    {
                        Predicate<string> match = null;
                        CmdletInfo element = infoArray2[i];
                        if (doNotExportCmdlets != null)
                        {
                            if (match == null)
                            {
                                match = cmdletName => string.Equals(element.FullName, cmdletName, StringComparison.OrdinalIgnoreCase);
                            }
                            if (doNotExportCmdlets.Exists(match))
                            {
                                continue;
                            }
                        }
                        if (SessionStateUtilities.MatchesAnyWildcardPattern(element.Name, cmdletPatterns, false))
                        {
                            string text = StringUtil.Format(Modules.ExportingCmdlet, element.Name);
                            cmdlet.WriteVerbose(text);
                            CmdletInfo item = new CmdletInfo(element.Name, element.ImplementingType, element.HelpFile, null, element.Context);
                            item.SetModule(sessionState.Module);
                            sessionState.Module.CompiledExports.Add(item);
                        }
                    }
                }
                foreach (KeyValuePair<string, List<CmdletInfo>> pair2 in cmdletTable)
                {
                    CmdletInfo cmdletToImport = pair2.Value[0];
                    if (((doNotExportCmdlets == null) || !doNotExportCmdlets.Exists(cmdletName => string.Equals(cmdletToImport.FullName, cmdletName, StringComparison.OrdinalIgnoreCase))) && SessionStateUtilities.MatchesAnyWildcardPattern(pair2.Key, cmdletPatterns, false))
                    {
                        string str3 = StringUtil.Format(Modules.ExportingCmdlet, pair2.Key);
                        cmdlet.WriteVerbose(str3);
                        CmdletInfo info2 = new CmdletInfo(cmdletToImport.Name, cmdletToImport.ImplementingType, cmdletToImport.HelpFile, null, cmdletToImport.Context);
                        info2.SetModule(sessionState.Module);
                        sessionState.Module.CompiledExports.Add(info2);
                    }
                }
                SortAndRemoveDuplicates<CmdletInfo>(sessionState.Module.CompiledExports, ci => ci.Name);
            }
            if (variablePatterns != null)
            {
                foreach (KeyValuePair<string, PSVariable> pair3 in sessionState.ModuleScope.Variables)
                {
                    if ((!pair3.Value.IsAllScope && (Array.IndexOf<string>(PSModuleInfo._builtinVariables, pair3.Key) == -1)) && SessionStateUtilities.MatchesAnyWildcardPattern(pair3.Key, variablePatterns, false))
                    {
                        string str4 = StringUtil.Format(Modules.ExportingVariable, pair3.Key);
                        cmdlet.WriteVerbose(str4);
                        sessionState.ExportedVariables.Add(pair3.Value);
                    }
                }
                SortAndRemoveDuplicates<PSVariable>(sessionState.ExportedVariables, v => v.Name);
            }
            if (aliasPatterns != null)
            {
                foreach (AliasInfo info3 in sessionState.ModuleScope.AliasTable)
                {
                    if (((info3.Options & ScopedItemOptions.AllScope) == ScopedItemOptions.None) && SessionStateUtilities.MatchesAnyWildcardPattern(info3.Name, aliasPatterns, false))
                    {
                        string str5 = StringUtil.Format(Modules.ExportingAlias, info3.Name);
                        cmdlet.WriteVerbose(str5);
                        sessionState.ExportedAliases.Add(info3);
                    }
                }
                SortAndRemoveDuplicates<AliasInfo>(sessionState.ExportedAliases, ci => ci.Name);
            }
        }

        internal List<PSModuleInfo> GetExactMatchModules(string moduleName, bool all, bool exactMatch)
        {
            if (moduleName == null)
            {
                moduleName = string.Empty;
            }
            return this.GetModuleCore(new string[] { moduleName }, all, exactMatch);
        }

        private static string GetExpandedEnvironmentVariable(string name, EnvironmentVariableTarget target)
        {
            string environmentVariable = Environment.GetEnvironmentVariable(name, target);
            if (!string.IsNullOrEmpty(environmentVariable))
            {
                environmentVariable = Environment.ExpandEnvironmentVariables(environmentVariable);
            }
            return environmentVariable;
        }

        private List<PSModuleInfo> GetModuleCore(string[] patterns, bool all, bool exactMatch)
        {
            string str = null;
            List<WildcardPattern> list = new List<WildcardPattern>();
            if (exactMatch)
            {
                str = patterns[0];
            }
            else
            {
                if (patterns == null)
                {
                    patterns = new string[] { "*" };
                }
                foreach (string str2 in patterns)
                {
                    list.Add(new WildcardPattern(str2, WildcardOptions.IgnoreCase));
                }
            }
            List<PSModuleInfo> list2 = new List<PSModuleInfo>();
            if (all)
            {
                foreach (string str3 in this.ModuleTable.Keys)
                {
                    PSModuleInfo item = this.ModuleTable[str3];
                    if ((exactMatch && item.Name.Equals(str, StringComparison.OrdinalIgnoreCase)) || (!exactMatch && SessionStateUtilities.MatchesAnyWildcardPattern(item.Name, list, false)))
                    {
                        list2.Add(item);
                    }
                }
            }
            else
            {
                Dictionary<string, bool> dictionary = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
                foreach (string str4 in this._context.EngineSessionState.ModuleTable.Keys)
                {
                    PSModuleInfo info2 = this._context.EngineSessionState.ModuleTable[str4];
                    if ((exactMatch && info2.Name.Equals(str, StringComparison.OrdinalIgnoreCase)) || (!exactMatch && SessionStateUtilities.MatchesAnyWildcardPattern(info2.Name, list, false)))
                    {
                        list2.Add(info2);
                        dictionary[str4] = true;
                    }
                }
                if (this._context.EngineSessionState != this._context.TopLevelSessionState)
                {
                    foreach (string str5 in this._context.TopLevelSessionState.ModuleTable.Keys)
                    {
                        if (!dictionary.ContainsKey(str5))
                        {
                            PSModuleInfo info3 = this.ModuleTable[str5];
                            if ((exactMatch && info3.Name.Equals(str, StringComparison.OrdinalIgnoreCase)) || (!exactMatch && SessionStateUtilities.MatchesAnyWildcardPattern(info3.Name, list, false)))
                            {
                                list2.Add(info3);
                            }
                        }
                    }
                }
            }
            return (from m in list2
                orderby m.Name
                select m).ToList<PSModuleInfo>();
        }

        internal static string GetModuleName(string path)
        {
            string str = (path == null) ? string.Empty : Path.GetFileName(path);
            string extension = Path.GetExtension(str);
            if (!string.IsNullOrEmpty(extension) && IsPowerShellModuleExtension(extension))
            {
                return str.Substring(0, str.Length - extension.Length);
            }
            return str;
        }

        internal static IEnumerable<string> GetModulePath(bool preferSystemModulePath, ExecutionContext context)
        {
            List<string> list = new List<string>();
            string environmentVariable = Environment.GetEnvironmentVariable("PSMODULEPATH");
            if (environmentVariable == null)
            {
                SetModulePath();
                environmentVariable = Environment.GetEnvironmentVariable("PSMODULEPATH");
            }
            if (preferSystemModulePath)
            {
                list.Add(GetSystemwideModulePath());
            }
            if (environmentVariable.Trim().Length != 0)
            {
                foreach (string str2 in environmentVariable.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string pattern = str2.Trim();
                    try
                    {
                        ProviderInfo provider = null;
                        if (context.EngineSessionState.IsProviderLoaded(context.ProviderNames.FileSystem))
                        {
                            IEnumerable<string> resolvedProviderPathFromPSPath = context.SessionState.Path.GetResolvedProviderPathFromPSPath(WildcardPattern.Escape(pattern), out provider);
                            if (provider.NameEquals(context.ProviderNames.FileSystem))
                            {
                                foreach (string str4 in resolvedProviderPathFromPSPath)
                                {
                                    list.Add(str4);
                                }
                            }
                        }
                        else
                        {
                            list.Add(pattern);
                        }
                    }
                    catch (ItemNotFoundException)
                    {
                    }
                    catch (System.Management.Automation.DriveNotFoundException)
                    {
                    }
                }
            }
            return list;
        }

        internal List<PSModuleInfo> GetModules(string[] patterns, bool all)
        {
            return this.GetModuleCore(patterns, all, false);
        }

        internal static string GetPersonalModulePath()
        {
            return Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), Utils.ProductNameForDirectory), Utils.ModuleDirectory);
        }

        internal static string GetSystemwideModulePath()
        {
            string str = null;
            string defaultPowerShellShellID = Utils.DefaultPowerShellShellID;
            string applicationBase = null;
            try
            {
                applicationBase = Utils.GetApplicationBase(defaultPowerShellShellID);
            }
            catch (SecurityException)
            {
            }
            if (!string.IsNullOrEmpty(applicationBase))
            {
                str = Path.Combine(applicationBase.ToLowerInvariant().Replace(@"\syswow64\", @"\system32\"), Utils.ModuleDirectory);
            }
            return str;
        }

        internal void IncrementModuleNestingDepth(PSCmdlet cmdlet, string path)
        {
            if (++this._moduleNestingDepth > 10)
            {
                InvalidOperationException exception = new InvalidOperationException(StringUtil.Format(Modules.ModuleTooDeeplyNested, path, 10));
                ErrorRecord errorRecord = new ErrorRecord(exception, "Modules_ModuleTooDeeplyNested", ErrorCategory.InvalidOperation, path);
                cmdlet.ThrowTerminatingError(errorRecord);
            }
        }

        internal static bool IsPowerShellModuleExtension(string extension)
        {
            foreach (string str in PSModuleProcessableExtensions)
            {
                if (extension.Equals(str, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        internal static void SetModulePath()
        {
            string expandedEnvironmentVariable = GetExpandedEnvironmentVariable("PSMODULEPATH", EnvironmentVariableTarget.Process);
            string str2 = GetExpandedEnvironmentVariable("PSMODULEPATH", EnvironmentVariableTarget.Machine);
            string str3 = GetExpandedEnvironmentVariable("PSMODULEPATH", EnvironmentVariableTarget.User);
            if (expandedEnvironmentVariable == null)
            {
                if (str3 == null)
                {
                    expandedEnvironmentVariable = GetPersonalModulePath();
                }
                else
                {
                    expandedEnvironmentVariable = str3;
                }
                expandedEnvironmentVariable = expandedEnvironmentVariable + ';';
                if (str2 == null)
                {
                    expandedEnvironmentVariable = expandedEnvironmentVariable + GetSystemwideModulePath();
                }
                else
                {
                    expandedEnvironmentVariable = expandedEnvironmentVariable + str2;
                }
            }
            else if (str2 != null)
            {
                if (str3 == null)
                {
                    if (!str2.Equals(expandedEnvironmentVariable, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                    expandedEnvironmentVariable = GetPersonalModulePath() + ';' + str2;
                }
                else
                {
                    string str4 = str3 + ';' + str2;
                    if ((!str4.Equals(expandedEnvironmentVariable, StringComparison.OrdinalIgnoreCase) && !str2.Equals(expandedEnvironmentVariable, StringComparison.OrdinalIgnoreCase)) && !str3.Equals(expandedEnvironmentVariable, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                    expandedEnvironmentVariable = str4;
                }
            }
            else if ((str3 != null) && str3.Equals(expandedEnvironmentVariable, StringComparison.OrdinalIgnoreCase))
            {
                expandedEnvironmentVariable = str3 + ';' + GetSystemwideModulePath();
            }
            else
            {
                return;
            }
            Environment.SetEnvironmentVariable("PSMODULEPATH", expandedEnvironmentVariable);
        }

        private static void SortAndRemoveDuplicates<T>(List<T> input, Converter<T, string> keyGetter)
        {
            input.Sort(delegate (T x, T y) {
                string strA = keyGetter(x);
                string strB = keyGetter(y);
                return string.Compare(strA, strB, true);
            });
            bool flag = true;
            string str = null;
            List<T> collection = new List<T>(input.Count);
            foreach (T local in input)
            {
                string str2 = keyGetter(local);
                if (flag || !str2.Equals(str, StringComparison.OrdinalIgnoreCase))
                {
                    collection.Add(local);
                }
                str = str2;
                flag = false;
            }
            input.Clear();
            input.AddRange(collection);
        }

        internal int ModuleNestingDepth
        {
            get
            {
                return this._moduleNestingDepth;
            }
        }

        internal Dictionary<string, PSModuleInfo> ModuleTable
        {
            get
            {
                return this._moduleTable;
            }
        }
    }
}

