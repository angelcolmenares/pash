namespace System.Management.Automation
{
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Language;
    using System.Management.Automation.Runspaces;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public sealed class PSModuleInfo
    {
        private ModuleAccessMode _accessMode;
        private static readonly ConcurrentDictionary<string, string> _appdomainModulePathCache = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        internal static string[] _builtinVariables = new string[] { 
            "_", "this", "input", "args", "true", "false", "null", "MaximumErrorCount", "MaximumVariableCount", "MaximumFunctionCount", "MaximumAliasCount", "PSDefaultParameterValues", "MaximumDriveCount", "Error", "PSScriptRoot", "PSCommandPath", 
            "MyInvocation", "ExecutionContext", "StackTrace"
         };
        private readonly List<CmdletInfo> _compiledExports;
        internal IScriptExtent _definitionExtent;
        private string _description;
        internal Dictionary<string, string> _detectedAliasExports;
        internal List<string> _detectedCmdletExports;
        internal List<string> _detectedFunctionExports;
        internal List<string> _detectedWorkflowExports;
        private ReadOnlyCollection<string> _exportedFormatFiles;
        private ReadOnlyCollection<string> _exportedTypeFiles;
        private List<string> _fileList;
        private System.Guid _guid;
        private string _helpInfoUri;
        private bool _logPipelineExecutionDetails;
        private string _moduleBase;
        private bool _moduleHasPrivateMembers;
        private Collection<object> _moduleList;
        private System.Management.Automation.ModuleType _moduleType;
        private string _name;
        private readonly List<PSModuleInfo> _nestedModules;
        private string _path;
        private ReadOnlyCollection<PSModuleInfo> _readonlyNestedModules;
        private ReadOnlyCollection<PSModuleInfo> _readonlyRequiredModules;
        private ReadOnlyCollection<ModuleSpecification> _readonlyRequiredModulesSpecification;
        private Collection<string> _requiredAssemblies;
        private List<PSModuleInfo> _requiredModules;
        private List<ModuleSpecification> _requiredModulesSpecification;
        private List<string> _scripts;
        private System.Management.Automation.SessionState _sessionState;
        private System.Version _version;
        internal Collection<string> DeclaredAliasExports;
        internal Collection<string> DeclaredCmdletExports;
        internal Collection<string> DeclaredFunctionExports;
        internal Collection<string> DeclaredVariableExports;
        internal Collection<string> DeclaredWorkflowExports;
        internal const string DynamicModulePrefixString = "__DynamicModule_";

        public PSModuleInfo(bool linkToGlobal) : this(LocalPipeline.GetExecutionContextFromTLS(), linkToGlobal)
        {
        }

        public PSModuleInfo(ScriptBlock scriptBlock)
        {
            this._name = string.Empty;
            this._path = string.Empty;
            this._description = string.Empty;
            this._version = new System.Version(0, 0);
            this._detectedFunctionExports = new List<string>();
            this._detectedWorkflowExports = new List<string>();
            this._detectedCmdletExports = new List<string>();
            this._compiledExports = new List<CmdletInfo>();
            this._fileList = new List<string>();
            this._moduleList = new Collection<object>();
            this._nestedModules = new List<PSModuleInfo>();
            this._scripts = new List<string>();
            this._requiredAssemblies = new Collection<string>();
            this._requiredModules = new List<PSModuleInfo>();
            this._requiredModulesSpecification = new List<ModuleSpecification>();
            this._detectedAliasExports = new Dictionary<string, string>();
            this._exportedFormatFiles = new ReadOnlyCollection<string>(new List<string>());
            this._exportedTypeFiles = new ReadOnlyCollection<string>(new List<string>());
            if (scriptBlock == null)
            {
                throw PSTraceSource.NewArgumentException("scriptBlock");
            }
            ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            if (executionContextFromTLS == null)
            {
                throw new InvalidOperationException("PSModuleInfo");
            }
            SetDefaultDynamicNameAndPath(this);
            this._sessionState = new System.Management.Automation.SessionState(executionContextFromTLS, true, true);
            this._sessionState.Internal.Module = this;
            SessionStateInternal engineSessionState = executionContextFromTLS.EngineSessionState;
            try
            {
                executionContextFromTLS.EngineSessionState = this._sessionState.Internal;
                executionContextFromTLS.SetVariable(SpecialVariables.PSScriptRootVarPath, this._path);
                scriptBlock = scriptBlock.Clone(true);
                scriptBlock.SessionState = this._sessionState;
                Pipe outputPipe = new Pipe {
                    NullPipe = true
                };
                scriptBlock.InvokeWithPipe(false, ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe, AutomationNull.Value, AutomationNull.Value, AutomationNull.Value, outputPipe, null, new object[0]);
            }
            finally
            {
                executionContextFromTLS.EngineSessionState = engineSessionState;
            }
        }

        internal PSModuleInfo(ExecutionContext context, bool linkToGlobal)
        {
            this._name = string.Empty;
            this._path = string.Empty;
            this._description = string.Empty;
            this._version = new System.Version(0, 0);
            this._detectedFunctionExports = new List<string>();
            this._detectedWorkflowExports = new List<string>();
            this._detectedCmdletExports = new List<string>();
            this._compiledExports = new List<CmdletInfo>();
            this._fileList = new List<string>();
            this._moduleList = new Collection<object>();
            this._nestedModules = new List<PSModuleInfo>();
            this._scripts = new List<string>();
            this._requiredAssemblies = new Collection<string>();
            this._requiredModules = new List<PSModuleInfo>();
            this._requiredModulesSpecification = new List<ModuleSpecification>();
            this._detectedAliasExports = new Dictionary<string, string>();
            this._exportedFormatFiles = new ReadOnlyCollection<string>(new List<string>());
            this._exportedTypeFiles = new ReadOnlyCollection<string>(new List<string>());
            if (context == null)
            {
                throw new InvalidOperationException("PSModuleInfo");
            }
            SetDefaultDynamicNameAndPath(this);
            this._sessionState = new System.Management.Automation.SessionState(context, true, linkToGlobal);
            this._sessionState.Internal.Module = this;
        }

        internal PSModuleInfo(string path, ExecutionContext context, System.Management.Automation.SessionState sessionState) : this(null, path, context, sessionState)
        {
        }

        internal PSModuleInfo(string name, string path, ExecutionContext context, System.Management.Automation.SessionState sessionState)
        {
            this._name = string.Empty;
            this._path = string.Empty;
            this._description = string.Empty;
            this._version = new System.Version(0, 0);
            this._detectedFunctionExports = new List<string>();
            this._detectedWorkflowExports = new List<string>();
            this._detectedCmdletExports = new List<string>();
            this._compiledExports = new List<CmdletInfo>();
            this._fileList = new List<string>();
            this._moduleList = new Collection<object>();
            this._nestedModules = new List<PSModuleInfo>();
            this._scripts = new List<string>();
            this._requiredAssemblies = new Collection<string>();
            this._requiredModules = new List<PSModuleInfo>();
            this._requiredModulesSpecification = new List<ModuleSpecification>();
            this._detectedAliasExports = new Dictionary<string, string>();
            this._exportedFormatFiles = new ReadOnlyCollection<string>(new List<string>());
            this._exportedTypeFiles = new ReadOnlyCollection<string>(new List<string>());
            if (path != null)
            {
                string resolvedPath = ModuleCmdletBase.GetResolvedPath(path, context);
                this._path = resolvedPath ?? path;
            }
            this._sessionState = sessionState;
            if (sessionState != null)
            {
                sessionState.Internal.Module = this;
            }
            if (name == null)
            {
                this._name = ModuleIntrinsics.GetModuleName(this._path);
            }
            else
            {
                this._name = name;
            }
        }

        internal void AddDetectedAliasExport(string name, string value)
        {
            this._detectedAliasExports[name] = value;
        }

        internal void AddDetectedCmdletExport(string cmdlet)
        {
            if (!this._detectedCmdletExports.Contains(cmdlet))
            {
                this._detectedCmdletExports.Add(cmdlet);
            }
        }

        internal void AddDetectedFunctionExport(string name)
        {
            if (!this._detectedFunctionExports.Contains(name))
            {
                this._detectedFunctionExports.Add(name);
            }
        }

        internal void AddDetectedWorkflowExport(string name)
        {
            if (!this._detectedWorkflowExports.Contains(name))
            {
                this._detectedWorkflowExports.Add(name);
            }
        }

        internal void AddExportedCmdlet(CmdletInfo cmdlet)
        {
            this._compiledExports.Add(cmdlet);
        }

        private static void AddModuleToList(PSModuleInfo module, List<PSModuleInfo> moduleList)
        {
            foreach (PSModuleInfo info in moduleList)
            {
                if (info.Path.Equals(module.Path, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }
            moduleList.Add(module);
        }

        internal void AddNestedModule(PSModuleInfo nestedModule)
        {
            AddModuleToList(nestedModule, this._nestedModules);
        }

        internal void AddRequiredAssembly(string assembly)
        {
            this._requiredAssemblies.Add(assembly);
        }

        internal void AddRequiredModule(PSModuleInfo requiredModule)
        {
            AddModuleToList(requiredModule, this._requiredModules);
        }

        internal void AddRequiredModuleSpecification(ModuleSpecification requiredModuleSpecification)
        {
            this._requiredModulesSpecification.Add(requiredModuleSpecification);
        }

        internal void AddScript(string s)
        {
            this._scripts.Add(s);
        }

        internal static void AddToAppDomainLevelModuleCache(string moduleName, string path, bool force)
        {
            Func<string, string, string> updateValueFactory = null;
            if (force)
            {
                if (updateValueFactory == null)
                {
                    updateValueFactory = (modulename, oldPath) => path;
                }
                _appdomainModulePathCache.AddOrUpdate(moduleName, path, updateValueFactory);
            }
            else
            {
                _appdomainModulePathCache.TryAdd(moduleName, path);
            }
        }

        internal void AddToFileList(string file)
        {
            this._fileList.Add(file);
        }

        internal void AddToModuleList(object m)
        {
            this._moduleList.Add(m);
        }

        public PSObject AsCustomObject()
        {
            if (this._sessionState == null)
            {
                throw PSTraceSource.NewInvalidOperationException("Modules", "InvalidOperationOnBinaryModule", new object[0]);
            }
            PSObject obj2 = new PSObject();
            foreach (KeyValuePair<string, FunctionInfo> pair in this.ExportedFunctions)
            {
                FunctionInfo info = pair.Value;
                if (info != null)
                {
                    PSScriptMethod member = new PSScriptMethod(info.Name, info.ScriptBlock);
                    obj2.Members.Add(member);
                }
            }
            foreach (KeyValuePair<string, PSVariable> pair2 in this.ExportedVariables)
            {
                PSVariable variable = pair2.Value;
                if (variable != null)
                {
                    PSVariableProperty property = new PSVariableProperty(variable);
                    obj2.Members.Add(property);
                }
            }
            return obj2;
        }

        internal void CaptureLocals()
        {
            if (this._sessionState == null)
            {
                throw PSTraceSource.NewInvalidOperationException("Modules", "InvalidOperationOnBinaryModule", new object[0]);
            }
            ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            MutableTuple localsTuple = executionContextFromTLS.EngineSessionState.CurrentScope.LocalsTuple;
            IEnumerable<PSVariable> values = executionContextFromTLS.EngineSessionState.CurrentScope.Variables.Values;
            if (localsTuple != null)
            {
                Dictionary<string, PSVariable> result = new Dictionary<string, PSVariable>();
                localsTuple.GetVariableTable(result, false);
                values = result.Values.Concat<PSVariable>(values);
            }
            foreach (PSVariable variable in values)
            {
                try
                {
                    if (variable.Options == ScopedItemOptions.None)
                    {
                        PSVariable variable2 = new PSVariable(variable.Name, variable.Value, variable.Options, variable.Attributes, variable.Description);
                        this._sessionState.Internal.NewVariable(variable2, false);
                    }
                }
                catch (SessionStateException)
                {
                }
            }
        }

        public static void ClearAppDomainLevelModulePathCache()
        {
            _appdomainModulePathCache.Clear();
        }

        public PSModuleInfo Clone()
        {
            PSModuleInfo info = (PSModuleInfo) base.MemberwiseClone();
            info._fileList = new List<string>(this.FileList);
            info._moduleList = new Collection<object>(this._moduleList);
            foreach (PSModuleInfo info2 in this.NestedModules)
            {
                info.AddNestedModule(info2);
            }
            info._readonlyNestedModules = new ReadOnlyCollection<PSModuleInfo>(this.NestedModules);
            info._readonlyRequiredModules = new ReadOnlyCollection<PSModuleInfo>(this.RequiredModules);
            info._readonlyRequiredModulesSpecification = new ReadOnlyCollection<ModuleSpecification>(this.RequiredModulesSpecification);
            info._requiredAssemblies = new Collection<string>(this._requiredAssemblies);
            info._requiredModulesSpecification = new List<ModuleSpecification>();
            info._requiredModules = new List<PSModuleInfo>();
            foreach (PSModuleInfo info3 in this._requiredModules)
            {
                info.AddRequiredModule(info3);
            }
            foreach (ModuleSpecification specification in this._requiredModulesSpecification)
            {
                info.AddRequiredModuleSpecification(specification);
            }
            info._scripts = new List<string>(this.Scripts);
            info._sessionState = this.SessionState;
            return info;
        }

        public object Invoke(ScriptBlock sb, params object[] args)
        {
            object obj2;
            if (sb == null)
            {
                return null;
            }
            SessionStateInternal sessionStateInternal = sb.SessionStateInternal;
            try
            {
                sb.SessionStateInternal = this._sessionState.Internal;
                obj2 = sb.InvokeReturnAsIs(args);
            }
            finally
            {
                sb.SessionStateInternal = sessionStateInternal;
            }
            return obj2;
        }

        public ScriptBlock NewBoundScriptBlock(ScriptBlock scriptBlockToBind)
        {
            ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            return this.NewBoundScriptBlock(scriptBlockToBind, executionContextFromTLS);
        }

        internal ScriptBlock NewBoundScriptBlock(ScriptBlock scriptBlockToBind, ExecutionContext context)
        {
            ScriptBlock block;
            if ((this._sessionState == null) || (context == null))
            {
                throw PSTraceSource.NewInvalidOperationException("Modules", "InvalidOperationOnBinaryModule", new object[0]);
            }
            lock (context.EngineSessionState)
            {
                SessionStateInternal engineSessionState = context.EngineSessionState;
                try
                {
                    context.EngineSessionState = this._sessionState.Internal;
                    block = scriptBlockToBind.Clone(true);
                    block.SessionState = this._sessionState;
                }
                finally
                {
                    context.EngineSessionState = engineSessionState;
                }
            }
            return block;
        }

        internal static bool RemoveFromAppDomainLevelCache(string moduleName)
        {
            string str;
            return _appdomainModulePathCache.TryRemove(moduleName, out str);
        }

        internal static string ResolveUsingAppDomainLevelModuleCache(string moduleName)
        {
            string str;
            if (_appdomainModulePathCache.TryGetValue(moduleName, out str))
            {
                return str;
            }
            return string.Empty;
        }

        internal static void SetDefaultDynamicNameAndPath(PSModuleInfo module)
        {
            string str = System.Guid.NewGuid().ToString();
            module._path = str;
            module._name = "__DynamicModule_" + str;
        }

        internal void SetExportedFormatFiles(ReadOnlyCollection<string> files)
        {
            this._exportedFormatFiles = files;
        }

        internal void SetExportedTypeFiles(ReadOnlyCollection<string> files)
        {
            this._exportedTypeFiles = files;
        }

        internal void SetGuid(System.Guid guid)
        {
            this._guid = guid;
        }

        internal void SetHelpInfoUri(string uri)
        {
            this._helpInfoUri = uri;
        }

        internal void SetModuleBase(string moduleBase)
        {
            this._moduleBase = moduleBase;
        }

        internal void SetModuleType(System.Management.Automation.ModuleType moduleType)
        {
            this._moduleType = moduleType;
        }

        internal void SetName(string name)
        {
            this._name = name;
        }

        internal void SetVersion(System.Version version)
        {
            this._version = version;
        }

        public override string ToString()
        {
            return this.Name;
        }

        public ModuleAccessMode AccessMode
        {
            get
            {
                return this._accessMode;
            }
            set
            {
                if (this._accessMode == ModuleAccessMode.Constant)
                {
                    throw PSTraceSource.NewInvalidOperationException();
                }
                this._accessMode = value;
            }
        }

        public string Author { get; internal set; }

        public System.Version ClrVersion { get; internal set; }

        public string CompanyName { get; internal set; }

        internal List<CmdletInfo> CompiledExports
        {
            get
            {
                if (((this._sessionState != null) && (this._sessionState.Internal.ExportedCmdlets != null)) && (this._sessionState.Internal.ExportedCmdlets.Count > 0))
                {
                    foreach (CmdletInfo info in this._sessionState.Internal.ExportedCmdlets)
                    {
                        this._compiledExports.Add(info);
                    }
                    this._sessionState.Internal.ExportedCmdlets.Clear();
                }
                return this._compiledExports;
            }
        }

        public string Copyright { get; internal set; }

        public string Definition
        {
            get
            {
                if (this._definitionExtent != null)
                {
                    return this._definitionExtent.Text;
                }
                return string.Empty;
            }
        }

        public string Description
        {
            get
            {
                return this._description;
            }
            set
            {
                this._description = value ?? string.Empty;
            }
        }

        public System.Version DotNetFrameworkVersion { get; internal set; }

        public Dictionary<string, AliasInfo> ExportedAliases
        {
            get
            {
                Dictionary<string, AliasInfo> dictionary = new Dictionary<string, AliasInfo>(StringComparer.OrdinalIgnoreCase);
                if ((this.DeclaredAliasExports != null) && (this.DeclaredAliasExports.Count > 0))
                {
                    foreach (string str in this.DeclaredAliasExports)
                    {
                        AliasInfo info = new AliasInfo(str, null, null);
                        info.SetModule(this);
                        dictionary.Add(str, info);
                    }
                    return dictionary;
                }
                if (this._sessionState == null)
                {
                    if (this._detectedAliasExports.Count > 0)
                    {
                        foreach (string str2 in this._detectedAliasExports.Keys)
                        {
                            if (!dictionary.ContainsKey(str2))
                            {
                                AliasInfo info2 = new AliasInfo(str2, this._detectedAliasExports[str2], null);
                                info2.SetModule(this);
                                dictionary.Add(str2, info2);
                            }
                        }
                    }
                    return dictionary;
                }
                foreach (AliasInfo info3 in this._sessionState.Internal.ExportedAliases)
                {
                    dictionary.Add(info3.Name, info3);
                }
                return dictionary;
            }
        }

        public Dictionary<string, CmdletInfo> ExportedCmdlets
        {
            get
            {
                Dictionary<string, CmdletInfo> dictionary = new Dictionary<string, CmdletInfo>(StringComparer.OrdinalIgnoreCase);
                if ((this.DeclaredCmdletExports != null) && (this.DeclaredCmdletExports.Count > 0))
                {
                    foreach (string str in this.DeclaredCmdletExports)
                    {
                        CmdletInfo info = new CmdletInfo(str, null, null, null, null);
                        info.SetModule(this);
                        dictionary.Add(str, info);
                    }
                    return dictionary;
                }
                if ((this.DeclaredCmdletExports == null) || (this.DeclaredCmdletExports.Count != 0))
                {
                    if ((this.CompiledExports != null) && (this.CompiledExports.Count > 0))
                    {
                        foreach (CmdletInfo info2 in this.CompiledExports)
                        {
                            dictionary.Add(info2.Name, info2);
                        }
                        return dictionary;
                    }
                    foreach (string str2 in this._detectedCmdletExports)
                    {
                        if (!dictionary.ContainsKey(str2))
                        {
                            CmdletInfo info3 = new CmdletInfo(str2, null, null, null, null);
                            info3.SetModule(this);
                            dictionary.Add(str2, info3);
                        }
                    }
                }
                return dictionary;
            }
        }

        public Dictionary<string, CommandInfo> ExportedCommands
        {
            get
            {
                Dictionary<string, CommandInfo> dictionary = new Dictionary<string, CommandInfo>(StringComparer.OrdinalIgnoreCase);
                Dictionary<string, CmdletInfo> exportedCmdlets = this.ExportedCmdlets;
                if (exportedCmdlets != null)
                {
                    foreach (KeyValuePair<string, CmdletInfo> pair in exportedCmdlets)
                    {
                        dictionary[pair.Key] = pair.Value;
                    }
                }
                Dictionary<string, FunctionInfo> exportedFunctions = this.ExportedFunctions;
                if (exportedFunctions != null)
                {
                    foreach (KeyValuePair<string, FunctionInfo> pair2 in exportedFunctions)
                    {
                        dictionary[pair2.Key] = pair2.Value;
                    }
                }
                Dictionary<string, FunctionInfo> exportedWorkflows = this.ExportedWorkflows;
                if (exportedWorkflows != null)
                {
                    foreach (KeyValuePair<string, FunctionInfo> pair3 in exportedWorkflows)
                    {
                        dictionary[pair3.Key] = pair3.Value;
                    }
                }
                Dictionary<string, AliasInfo> exportedAliases = this.ExportedAliases;
                if (exportedAliases != null)
                {
                    foreach (KeyValuePair<string, AliasInfo> pair4 in exportedAliases)
                    {
                        dictionary[pair4.Key] = pair4.Value;
                    }
                }
                return dictionary;
            }
        }

        public ReadOnlyCollection<string> ExportedFormatFiles
        {
            get
            {
                return this._exportedFormatFiles;
            }
        }

        public Dictionary<string, FunctionInfo> ExportedFunctions
        {
            get
            {
                Dictionary<string, FunctionInfo> dictionary = new Dictionary<string, FunctionInfo>(StringComparer.OrdinalIgnoreCase);
                if ((this.DeclaredFunctionExports != null) && (this.DeclaredFunctionExports.Count > 0))
                {
                    foreach (string str in this.DeclaredFunctionExports)
                    {
                        FunctionInfo info = new FunctionInfo(str, ScriptBlock.Create(""), null);
                        info.SetModule(this);
                        dictionary.Add(str, info);
                    }
                    return dictionary;
                }
                if ((this.DeclaredFunctionExports == null) || (this.DeclaredFunctionExports.Count != 0))
                {
                    if (this._sessionState != null)
                    {
                        if (this._sessionState.Internal.ExportedFunctions != null)
                        {
                            foreach (FunctionInfo info2 in this._sessionState.Internal.ExportedFunctions)
                            {
                                if (!dictionary.ContainsKey(info2.Name))
                                {
                                    dictionary.Add(info2.Name, info2);
                                }
                            }
                        }
                        return dictionary;
                    }
                    foreach (string str2 in this._detectedFunctionExports)
                    {
                        if (!dictionary.ContainsKey(str2))
                        {
                            FunctionInfo info3 = new FunctionInfo(str2, ScriptBlock.Create(""), null);
                            info3.SetModule(this);
                            dictionary.Add(str2, info3);
                        }
                    }
                }
                return dictionary;
            }
        }

        public ReadOnlyCollection<string> ExportedTypeFiles
        {
            get
            {
                return this._exportedTypeFiles;
            }
        }

        public Dictionary<string, PSVariable> ExportedVariables
        {
            get
            {
                Dictionary<string, PSVariable> dictionary = new Dictionary<string, PSVariable>(StringComparer.OrdinalIgnoreCase);
                if ((this.DeclaredVariableExports != null) && (this.DeclaredVariableExports.Count > 0))
                {
                    foreach (string str in this.DeclaredVariableExports)
                    {
                        dictionary.Add(str, null);
                    }
                    return dictionary;
                }
                if ((this._sessionState != null) && (this._sessionState.Internal.ExportedVariables != null))
                {
                    foreach (PSVariable variable in this._sessionState.Internal.ExportedVariables)
                    {
                        dictionary.Add(variable.Name, variable);
                    }
                }
                return dictionary;
            }
        }

        public Dictionary<string, FunctionInfo> ExportedWorkflows
        {
            get
            {
                Dictionary<string, FunctionInfo> dictionary = new Dictionary<string, FunctionInfo>(StringComparer.OrdinalIgnoreCase);
                if ((this.DeclaredWorkflowExports != null) && (this.DeclaredWorkflowExports.Count > 0))
                {
                    foreach (string str in this.DeclaredWorkflowExports)
                    {
                        WorkflowInfo info = new WorkflowInfo(str, ScriptBlock.Create(""), null);
                        info.SetModule(this);
                        dictionary.Add(str, info);
                    }
                }
                if ((this.DeclaredWorkflowExports == null) || (this.DeclaredWorkflowExports.Count != 0))
                {
                    if (this._sessionState == null)
                    {
                        foreach (string str2 in this._detectedWorkflowExports)
                        {
                            if (!dictionary.ContainsKey(str2))
                            {
                                WorkflowInfo info2 = new WorkflowInfo(str2, ScriptBlock.Create(""), null);
                                info2.SetModule(this);
                                dictionary.Add(str2, info2);
                            }
                        }
                        return dictionary;
                    }
                    foreach (WorkflowInfo info3 in this._sessionState.Internal.ExportedWorkflows)
                    {
                        dictionary.Add(info3.Name, info3);
                    }
                }
                return dictionary;
            }
        }

        public IEnumerable<string> FileList
        {
            get
            {
                return this._fileList;
            }
        }

        public System.Guid Guid
        {
            get
            {
                return this._guid;
            }
        }

        internal bool HadErrorsLoading { get; set; }

        public string HelpInfoUri
        {
            get
            {
                return this._helpInfoUri;
            }
        }

        public bool LogPipelineExecutionDetails
        {
            get
            {
                return this._logPipelineExecutionDetails;
            }
            set
            {
                this._logPipelineExecutionDetails = value;
            }
        }

        public string ModuleBase
        {
            get
            {
                return (this._moduleBase ?? (this._moduleBase = !string.IsNullOrEmpty(this._path) ? System.IO.Path.GetDirectoryName(this._path) : string.Empty));
            }
        }

        internal bool ModuleHasPrivateMembers
        {
            get
            {
                return this._moduleHasPrivateMembers;
            }
            set
            {
                this._moduleHasPrivateMembers = value;
            }
        }

        public IEnumerable<object> ModuleList
        {
            get
            {
                return this._moduleList;
            }
        }

        public System.Management.Automation.ModuleType ModuleType
        {
            get
            {
                return this._moduleType;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public ReadOnlyCollection<PSModuleInfo> NestedModules
        {
            get
            {
                return (this._readonlyNestedModules ?? (this._readonlyNestedModules = new ReadOnlyCollection<PSModuleInfo>(this._nestedModules)));
            }
        }

        public ScriptBlock OnRemove { get; set; }

        public string Path
        {
            get
            {
                return this._path;
            }
            internal set
            {
                this._path = value;
            }
        }

        public string PowerShellHostName { get; internal set; }

        public System.Version PowerShellHostVersion { get; internal set; }

        public System.Version PowerShellVersion { get; internal set; }

        public object PrivateData { get; set; }

        public System.Reflection.ProcessorArchitecture ProcessorArchitecture { get; internal set; }

        public IEnumerable<string> RequiredAssemblies
        {
            get
            {
                return this._requiredAssemblies;
            }
        }

        public ReadOnlyCollection<PSModuleInfo> RequiredModules
        {
            get
            {
                return (this._readonlyRequiredModules ?? (this._readonlyRequiredModules = new ReadOnlyCollection<PSModuleInfo>(this._requiredModules)));
            }
        }

        internal ReadOnlyCollection<ModuleSpecification> RequiredModulesSpecification
        {
            get
            {
                return (this._readonlyRequiredModulesSpecification ?? (this._readonlyRequiredModulesSpecification = new ReadOnlyCollection<ModuleSpecification>(this._requiredModulesSpecification)));
            }
        }

        public string RootModule { get; internal set; }

        internal string RootModuleForManifest { get; set; }

        public IEnumerable<string> Scripts
        {
            get
            {
                return this._scripts;
            }
        }

        public System.Management.Automation.SessionState SessionState
        {
            get
            {
                return this._sessionState;
            }
            set
            {
                this._sessionState = value;
            }
        }

        public static bool UseAppDomainLevelModuleCache
        {
            get;
            set;
        }

        public System.Version Version
        {
            get
            {
                return this._version;
            }
        }
    }
}

