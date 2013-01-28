namespace System.Management.Automation
{
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Language;
    using System.Management.Automation.Provider;
    using System.Management.Automation.Runspaces;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.AccessControl;
    using System.Text;
    using System.Threading;

    internal sealed class SessionStateInternal
    {
        private List<string> _applications;
        private static char[] _charactersInvalidInDriveName = new char[] { ':', '/', '\\', '.', '~' };
        private System.Management.Automation.ExecutionContext _context;
        private List<AliasInfo> _exportedAliases;
        private List<CmdletInfo> _exportedCmdlets;
        private List<FunctionInfo> _exportedFunctions;
        private List<PSVariable> _exportedVariables;
        private List<WorkflowInfo> _exportedWorkflows;
        private readonly SessionStateScope _globalScope;
        private ProviderIntrinsics _invokeProvider;
        private PSModuleInfo _module;
        private readonly SessionStateScope _moduleScope;
        private Dictionary<string, PSModuleInfo> _moduleTable;
        private Dictionary<string, List<ProviderInfo>> _providers;
        private Dictionary<ProviderInfo, PSDriveInfo> _providersCurrentWorkingDrive;
        private bool _providersInitialized;
        private SessionState _publicSessionState;
        private List<string> _scripts;
        private bool _useExportList;
        private PSDriveInfo currentDrive;
        private SessionStateScope currentScope;
        private string defaultStackName;
        private LocationGlobber globberPrivate;
        internal List<string> ModuleTableKeys;
        private const string startingDefaultStackName = "default";
        [TraceSource("SessionState", "SessionState Class")]
        private static PSTraceSource tracer = PSTraceSource.GetTracer("SessionState", "SessionState Class");
        private Dictionary<string, Stack<PathInfo>> workingLocationStack;

        internal SessionStateInternal(System.Management.Automation.ExecutionContext context) : this(null, false, context)
        {
        }

        internal SessionStateInternal(SessionStateInternal parent, bool linkToGlobal, System.Management.Automation.ExecutionContext context)
        {
            this.ModuleTableKeys = new List<string>();
            this._moduleTable = new Dictionary<string, PSModuleInfo>(StringComparer.OrdinalIgnoreCase);
            this._scripts = new List<string>(new string[] { "*" });
            this._applications = new List<string>(new string[] { "*" });
            this._exportedCmdlets = new List<CmdletInfo>();
            this._exportedAliases = new List<AliasInfo>();
            this._exportedFunctions = new List<FunctionInfo>();
            this._exportedWorkflows = new List<WorkflowInfo>();
            this.defaultStackName = "default";
            this._providers = new Dictionary<string, List<ProviderInfo>>(100, StringComparer.OrdinalIgnoreCase);
            this._providersCurrentWorkingDrive = new Dictionary<ProviderInfo, PSDriveInfo>();
            this._exportedVariables = new List<PSVariable>();
            if (context == null)
            {
                throw PSTraceSource.NewArgumentNullException("context");
            }
            this._context = context;
            this.workingLocationStack = new Dictionary<string, Stack<PathInfo>>(StringComparer.OrdinalIgnoreCase);
            this._globalScope = new SessionStateScope(null);
            this._moduleScope = this._globalScope;
            this.currentScope = this._globalScope;
            this.InitializeSessionStateInternalSpecialVariables(false);
            this._globalScope.ScriptScope = this._globalScope;
            if (parent != null)
            {
                this._globalScope.Parent = parent.GlobalScope;
                this.CopyProviders(parent);
                if ((this.Providers != null) && (this.Providers.Count > 0))
                {
                    this.CurrentDrive = parent.CurrentDrive;
                }
                if (linkToGlobal)
                {
                    this._globalScope = parent.GlobalScope;
                }
            }
            else
            {
                this.currentScope.LocalsTuple = MutableTuple.MakeTuple(Compiler.DottedLocalsTupleType, Compiler.DottedLocalsNameIndexMap);
            }
        }

        internal void AddBuiltInAliases()
        {
            foreach (SessionStateAliasEntry entry in InitialSessionState.BuiltInAliases)
            {
                this.AddSessionStateEntry(entry);
            }
        }

        internal void AddBuiltInEntries(bool addSetStrictMode)
        {
            this.AddBuiltInVariables();
            this.AddBuiltInFunctions();
            this.AddBuiltInAliases();
            if (addSetStrictMode)
            {
                SessionStateFunctionEntry entry = new SessionStateFunctionEntry("Set-StrictMode", "");
                this.AddSessionStateEntry(entry);
            }
        }

        internal void AddBuiltInFunctions()
        {
            foreach (SessionStateFunctionEntry entry in InitialSessionState.BuiltInFunctions)
            {
                this.AddSessionStateEntry(entry);
            }
        }

        internal void AddBuiltInVariables()
        {
            foreach (SessionStateVariableEntry entry in InitialSessionState.BuiltInVariables)
            {
                this.AddSessionStateEntry(entry);
            }
        }

        private ProviderInfo AddProvider(ProviderConfigurationEntry providerConfig)
        {
            return this.AddProvider(providerConfig.ImplementingType, providerConfig.Name, providerConfig.HelpFileName, providerConfig.PSSnapIn, null);
        }

        private ProviderInfo AddProvider(Type implementingType, string name, string helpFileName, PSSnapInInfo psSnapIn, PSModuleInfo module)
        {
            ProviderInfo provider = null;
            try
            {
                provider = new ProviderInfo(new SessionState(this), implementingType, name, helpFileName, psSnapIn);
                provider.SetModule(module);
                this.NewProvider(provider);
                MshLog.LogProviderLifecycleEvent(this.ExecutionContext, provider.Name, ProviderState.Started);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (SessionStateException exception)
            {
                if (exception.GetType() == typeof(SessionStateException))
                {
                    throw;
                }
                this.ExecutionContext.ReportEngineStartupError(exception);
            }
            catch (Exception exception2)
            {
                CommandProcessorBase.CheckForSevereException(exception2);
                this.ExecutionContext.ReportEngineStartupError(exception2);
            }
            return provider;
        }

        private string AddQualifier(string path, string qualifier, bool isProviderQualified, bool isDriveQualified)
        {
            string str = path;
            string format = "{1}";
            if (isProviderQualified)
            {
                format = "{0}::{1}";
            }
            else if (isDriveQualified)
            {
                format = "{0}:{1}";
            }
            return string.Format(CultureInfo.InvariantCulture, format, new object[] { qualifier, path });
        }

        internal void AddSessionStateEntry(SessionStateAliasEntry entry)
        {
            AliasInfo alias = new AliasInfo(entry.Name, entry.Definition, this.ExecutionContext, entry.Options) {
                Visibility = entry.Visibility
            };
            alias.SetModule(entry.Module);
            if (!string.IsNullOrEmpty(entry.Description))
            {
                alias.Description = entry.Description;
            }
            this.SetAliasItemAtScope(alias, "global", true, CommandOrigin.Internal);
        }

        internal void AddSessionStateEntry(SessionStateApplicationEntry entry)
        {
            this.Applications.Add(entry.Path);
        }

        internal void AddSessionStateEntry(SessionStateCmdletEntry entry)
        {
            this.AddSessionStateEntry(entry, false);
        }

        internal void AddSessionStateEntry(SessionStateFunctionEntry entry)
        {
            ScriptBlock function = entry.ScriptBlock.Clone(false);
            if (function.IsSingleFunctionDefinition(entry.Name))
            {
                throw PSTraceSource.NewArgumentException("entry");
            }
            FunctionInfo info = this.SetFunction(entry.Name, function, null, entry.Options, false, CommandOrigin.Internal, this.ExecutionContext, entry.HelpFile, true);
            info.Visibility = entry.Visibility;
            info.SetModule(entry.Module);
            info.ScriptBlock.LanguageMode = 0;
        }

        internal void AddSessionStateEntry(SessionStateProviderEntry providerEntry)
        {
            this.AddProvider(providerEntry.ImplementingType, providerEntry.Name, providerEntry.HelpFileName, providerEntry.PSSnapIn, providerEntry.Module);
        }

        internal void AddSessionStateEntry(SessionStateScriptEntry entry)
        {
            this.Scripts.Add(entry.Path);
        }

        internal void AddSessionStateEntry(SessionStateVariableEntry entry)
        {
            PSVariable variable = new PSVariable(entry.Name, entry.Value, entry.Options, entry.Attributes, entry.Description) {
                Visibility = entry.Visibility
            };
            this.SetVariableAtScope(variable, "global", true, CommandOrigin.Internal);
        }

        internal void AddSessionStateEntry(InitialSessionState initialSessionState, SessionStateWorkflowEntry entry)
        {
            IAstToWorkflowConverter astToWorkflowConverterAndEnsureWorkflowModuleLoaded = Utils.GetAstToWorkflowConverterAndEnsureWorkflowModuleLoaded(null);
            WorkflowInfo workflowInfo = entry.WorkflowInfo;
            if (workflowInfo == null)
            {
                workflowInfo = astToWorkflowConverterAndEnsureWorkflowModuleLoaded.CompileWorkflow(entry.Name, entry.Definition, initialSessionState);
            }
            WorkflowInfo info2 = new WorkflowInfo(workflowInfo);
            info2 = this.SetWorkflowRaw(info2, CommandOrigin.Internal);
            info2.Visibility = entry.Visibility;
            info2.SetModule(entry.Module);
        }

        internal void AddSessionStateEntry(SessionStateCmdletEntry entry, bool local)
        {
            this.ExecutionContext.CommandDiscovery.AddSessionStateCmdletEntryToCache(entry, local);
        }

        internal PSDriveInfo AutomountBuiltInDrive(string name)
        {
            MountDefaultDrive(name, this._context);
            return this.GetDrive(name, false);
        }

        private PSDriveInfo AutomountFileSystemDrive(DriveInfo systemDriveInfo)
        {
            PSDriveInfo newDrive = null;
            if (!this.IsProviderLoaded(this.ExecutionContext.ProviderNames.FileSystem))
            {
                tracer.WriteLine("The {0} provider is not loaded", new object[] { this.ExecutionContext.ProviderNames.FileSystem });
                return newDrive;
            }
            try
            {
                DriveCmdletProvider driveProviderInstance = this.GetDriveProviderInstance(this.ExecutionContext.ProviderNames.FileSystem);
                if (driveProviderInstance == null)
                {
                    return newDrive;
                }
				string name = OSHelper.IsUnix ? systemDriveInfo.Name : systemDriveInfo.Name.Substring(0, 1);
                string description = string.Empty;
                try
                {
                    description = systemDriveInfo.VolumeLabel;
                }
                catch (UnauthorizedAccessException)
                {
                }
                PSDriveInfo drive = new PSDriveInfo(name, driveProviderInstance.ProviderInfo, systemDriveInfo.RootDirectory.FullName, description, null) {
                    IsAutoMounted = true
                };
                CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
                drive.DriveBeingCreated = true;
                newDrive = this.ValidateDriveWithProvider(driveProviderInstance, drive, context, false);
                drive.DriveBeingCreated = false;
                if ((newDrive != null) && !context.HasErrors())
                {
                    this._globalScope.NewDrive(newDrive);
                }
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                MshLog.LogProviderHealthEvent(this.ExecutionContext, this.ExecutionContext.ProviderNames.FileSystem, exception, Severity.Warning);
            }
            return newDrive;
        }

        private PSDriveInfo AutomountFileSystemDrive(string name)
        {
            PSDriveInfo info = null;
            if (name.Length == 1)
            {
                try
                {
                    DriveInfo systemDriveInfo = new DriveInfo(name);
                    info = this.AutomountFileSystemDrive(systemDriveInfo);
                }
                catch (LoopFlowException)
                {
                    throw;
                }
                catch (PipelineStoppedException)
                {
                    throw;
                }
                catch (ActionPreferenceStopException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                }
            }
            return info;
        }

        private bool CanRemoveDrive(PSDriveInfo drive, CmdletProviderContext context)
        {
            if (context == null)
            {
                throw PSTraceSource.NewArgumentNullException("context");
            }
            if (drive == null)
            {
                throw PSTraceSource.NewArgumentNullException("drive");
            }
            tracer.WriteLine("Drive name = {0}", new object[] { drive.Name });
            context.Drive = drive;
            DriveCmdletProvider driveProviderInstance = this.GetDriveProviderInstance(drive.Provider);
            bool flag = false;
            PSDriveInfo info = null;
            try
            {
                info = driveProviderInstance.RemoveDrive(drive, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("RemoveDriveProviderException", SessionStateStrings.RemoveDriveProviderException, driveProviderInstance.ProviderInfo, null, exception);
            }
            if ((info != null) && (string.Compare(info.Name, drive.Name, true, Thread.CurrentThread.CurrentCulture) == 0))
            {
                flag = true;
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        internal SessionStateEntryVisibility CheckApplicationVisibility(string applicationPath)
        {
            return this.checkPathVisibility(this._applications, applicationPath);
        }

        private SessionStateEntryVisibility checkPathVisibility(List<string> list, string path)
        {
            if (((list != null) && (list.Count != 0)) && !string.IsNullOrEmpty(path))
            {
                if (list.Contains("*"))
                {
                    return SessionStateEntryVisibility.Public;
                }
                foreach (string str in list)
                {
                    if (string.Equals(str, path, StringComparison.OrdinalIgnoreCase))
                    {
                        return SessionStateEntryVisibility.Public;
                    }
                }
            }
            return SessionStateEntryVisibility.Private;
        }

        internal SessionStateEntryVisibility CheckScriptVisibility(string scriptPath)
        {
            return this.checkPathVisibility(this._scripts, scriptPath);
        }

        internal void ClearContent(string[] paths, CmdletProviderContext context)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            ProviderInfo info = null;
            CmdletProvider providerInstance = null;
            foreach (string str in paths)
            {
                if (str == null)
                {
                    PSTraceSource.NewArgumentNullException("paths");
                }
                foreach (string str2 in this.Globber.GetGlobbedProviderPathsFromMonadPath(str, false, context, out info, out providerInstance))
                {
                    this.ClearContentPrivate(providerInstance, str2, context);
                }
            }
        }

        internal void ClearContent(string[] paths, bool force, bool literalPath)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext) {
                Force = force,
                SuppressWildcardExpansion = literalPath
            };
            this.ClearContent(paths, context);
            context.ThrowFirstErrorOrDoNothing();
        }

        internal object ClearContentDynamicParameters(string path, CmdletProviderContext context)
        {
            if (path != null)
            {
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                CmdletProviderContext context2 = new CmdletProviderContext(context);
                context2.SetFilters(new Collection<string>(), new Collection<string>(), null);
                Collection<string> collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context2, out info, out providerInstance);
                if (collection.Count > 0)
                {
                    return this.ClearContentDynamicParameters(providerInstance, collection[0], context2);
                }
            }
            return null;
        }

        private object ClearContentDynamicParameters(CmdletProvider providerInstance, string path, CmdletProviderContext context)
        {
            object obj2 = null;
            try
            {
                obj2 = providerInstance.ClearContentDynamicParameters(path, context);
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("ClearContentDynamicParametersProviderException", SessionStateStrings.ClearContentDynamicParametersProviderException, providerInstance.ProviderInfo, path, exception);
            }
            return obj2;
        }

        private void ClearContentPrivate(CmdletProvider providerInstance, string path, CmdletProviderContext context)
        {
            try
            {
                providerInstance.ClearContent(path, context);
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("ClearContentProviderException", SessionStateStrings.ClearContentProviderException, providerInstance.ProviderInfo, path, exception);
            }
        }

        internal void ClearItem(string[] paths, CmdletProviderContext context)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            ProviderInfo info = null;
            CmdletProvider providerInstance = null;
            foreach (string str in paths)
            {
                if (str == null)
                {
                    throw PSTraceSource.NewArgumentNullException("paths");
                }
                Collection<string> collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(str, false, context, out info, out providerInstance);
                if (collection != null)
                {
                    foreach (string str2 in collection)
                    {
                        this.ClearItemPrivate(providerInstance, str2, context);
                    }
                }
            }
        }

        internal Collection<PSObject> ClearItem(string[] paths, bool force, bool literalPath)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext) {
                Force = force,
                SuppressWildcardExpansion = literalPath
            };
            this.ClearItem(paths, context);
            context.ThrowFirstErrorOrDoNothing();
            return context.GetAccumulatedObjects();
        }

        internal object ClearItemDynamicParameters(string path, CmdletProviderContext context)
        {
            if (path != null)
            {
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                CmdletProviderContext context2 = new CmdletProviderContext(context);
                context2.SetFilters(new Collection<string>(), new Collection<string>(), null);
                Collection<string> collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context2, out info, out providerInstance);
                if (collection.Count > 0)
                {
                    return this.ClearItemDynamicParameters(providerInstance, collection[0], context2);
                }
            }
            return null;
        }

        private object ClearItemDynamicParameters(CmdletProvider providerInstance, string path, CmdletProviderContext context)
        {
            ItemCmdletProvider itemProviderInstance = GetItemProviderInstance(providerInstance);
            object obj2 = null;
            try
            {
                obj2 = itemProviderInstance.ClearItemDynamicParameters(path, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("ClearItemProviderException", SessionStateStrings.ClearItemProviderException, itemProviderInstance.ProviderInfo, path, exception);
            }
            return obj2;
        }

        private void ClearItemPrivate(CmdletProvider providerInstance, string path, CmdletProviderContext context)
        {
            ItemCmdletProvider itemProviderInstance = GetItemProviderInstance(providerInstance);
            try
            {
                itemProviderInstance.ClearItem(path, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("ClearItemProviderException", SessionStateStrings.ClearItemProviderException, itemProviderInstance.ProviderInfo, path, exception);
            }
        }

        internal void ClearProperty(string[] paths, Collection<string> propertyToClear, CmdletProviderContext context)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            if (propertyToClear == null)
            {
                throw PSTraceSource.NewArgumentNullException("propertyToClear");
            }
            foreach (string str in paths)
            {
                if (str == null)
                {
                    throw PSTraceSource.NewArgumentNullException("paths");
                }
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                foreach (string str2 in this.Globber.GetGlobbedProviderPathsFromMonadPath(str, false, context, out info, out providerInstance))
                {
                    this.ClearPropertyPrivate(providerInstance, str2, propertyToClear, context);
                }
            }
        }

        internal void ClearProperty(string[] paths, Collection<string> propertyToClear, bool force, bool literalPath)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            if (propertyToClear == null)
            {
                throw PSTraceSource.NewArgumentNullException("propertyToClear");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext) {
                Force = force,
                SuppressWildcardExpansion = literalPath
            };
            this.ClearProperty(paths, propertyToClear, context);
            context.ThrowFirstErrorOrDoNothing();
        }

        internal object ClearPropertyDynamicParameters(string path, Collection<string> propertyToClear, CmdletProviderContext context)
        {
            if (path != null)
            {
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                CmdletProviderContext context2 = new CmdletProviderContext(context);
                context2.SetFilters(new Collection<string>(), new Collection<string>(), null);
                Collection<string> collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context2, out info, out providerInstance);
                if (collection.Count > 0)
                {
                    return this.ClearPropertyDynamicParameters(providerInstance, collection[0], propertyToClear, context2);
                }
            }
            return null;
        }

        private object ClearPropertyDynamicParameters(CmdletProvider providerInstance, string path, Collection<string> propertyToClear, CmdletProviderContext context)
        {
            object obj2 = null;
            try
            {
                obj2 = providerInstance.ClearPropertyDynamicParameters(path, propertyToClear, context);
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("ClearPropertyDynamicParametersProviderException", SessionStateStrings.ClearPropertyDynamicParametersProviderException, providerInstance.ProviderInfo, path, exception);
            }
            return obj2;
        }

        private void ClearPropertyPrivate(CmdletProvider providerInstance, string path, Collection<string> propertyToClear, CmdletProviderContext context)
        {
            try
            {
                providerInstance.ClearProperty(path, propertyToClear, context);
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("ClearPropertyProviderException", SessionStateStrings.ClearPropertyProviderException, providerInstance.ProviderInfo, path, exception);
            }
        }

        internal void CopyItem(string[] paths, string copyPath, bool recurse, CopyContainers copyContainers, CmdletProviderContext context)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            if (copyPath == null)
            {
                copyPath = string.Empty;
            }
            PSDriveInfo drive = null;
            ProviderInfo info2 = null;
            string path = this.Globber.GetProviderPath(copyPath, context, out info2, out drive);
            tracer.WriteLine("providerDestinationPath = {0}", new object[] { path });
            ProviderInfo info3 = null;
            CmdletProvider providerInstance = null;
            foreach (string str2 in paths)
            {
                if (str2 == null)
                {
                    throw PSTraceSource.NewArgumentNullException("paths");
                }
                Collection<string> targetObject = this.Globber.GetGlobbedProviderPathsFromMonadPath(str2, false, context, out info3, out providerInstance);
                if (info3 != info2)
                {
                    ArgumentException exception = PSTraceSource.NewArgumentException("path", "SessionStateStrings", "CopyItemSourceAndDestinationNotSameProvider", new object[0]);
                    context.WriteError(new ErrorRecord(exception, "CopyItemSourceAndDestinationNotSameProvider", ErrorCategory.InvalidArgument, targetObject));
                    break;
                }
                bool flag = this.IsItemContainer(providerInstance, path, context);
                tracer.WriteLine("destinationIsContainer = {0}", new object[] { flag });
                foreach (string str3 in targetObject)
                {
                    if (context.Stopping)
                    {
                        break;
                    }
                    bool flag2 = this.IsItemContainer(providerInstance, str3, context);
                    tracer.WriteLine("sourcIsContainer = {0}", new object[] { flag2 });
                    if (flag2)
                    {
                        if (flag)
                        {
                            if (!recurse && (copyContainers == CopyContainers.CopyChildrenOfTargetContainer))
                            {
                                Exception exception2 = PSTraceSource.NewArgumentException("path", "SessionStateStrings", "CopyContainerToContainerWithoutRecurseOrContainer", new object[0]);
                                context.WriteError(new ErrorRecord(exception2, "CopyContainerToContainerWithoutRecurseOrContainer", ErrorCategory.InvalidArgument, str3));
                            }
                            else if (recurse && (copyContainers == CopyContainers.CopyChildrenOfTargetContainer))
                            {
                                this.CopyRecurseToSingleContainer(providerInstance, str3, path, context);
                            }
                            else
                            {
                                this.CopyItem(providerInstance, str3, path, recurse, context);
                            }
                        }
                        else if (this.ItemExists(providerInstance, path, context))
                        {
                            Exception exception3 = PSTraceSource.NewArgumentException("path", "SessionStateStrings", "CopyContainerItemToLeafError", new object[0]);
                            context.WriteError(new ErrorRecord(exception3, "CopyContainerItemToLeafError", ErrorCategory.InvalidArgument, str3));
                        }
                        else
                        {
                            this.CopyItem(providerInstance, str3, path, recurse, context);
                        }
                    }
                    else
                    {
                        this.CopyItem(providerInstance, str3, path, recurse, context);
                    }
                }
            }
        }

        private void CopyItem(CmdletProvider providerInstance, string path, string copyPath, bool recurse, CmdletProviderContext context)
        {
            ContainerCmdletProvider containerProviderInstance = GetContainerProviderInstance(providerInstance);
            try
            {
                containerProviderInstance.CopyItem(path, copyPath, recurse, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("CopyItemProviderException", SessionStateStrings.CopyItemProviderException, containerProviderInstance.ProviderInfo, path, exception);
            }
        }

        internal Collection<PSObject> CopyItem(string[] paths, string copyPath, bool recurse, CopyContainers copyContainers, bool force, bool literalPath)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            if (copyPath == null)
            {
                copyPath = string.Empty;
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext) {
                Force = force,
                SuppressWildcardExpansion = literalPath
            };
            this.CopyItem(paths, copyPath, recurse, copyContainers, context);
            context.ThrowFirstErrorOrDoNothing();
            return context.GetAccumulatedObjects();
        }

        internal object CopyItemDynamicParameters(string path, string destination, bool recurse, CmdletProviderContext context)
        {
            if (path != null)
            {
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                CmdletProviderContext context2 = new CmdletProviderContext(context);
                context2.SetFilters(new Collection<string>(), new Collection<string>(), null);
                Collection<string> collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context2, out info, out providerInstance);
                if (collection.Count > 0)
                {
                    return this.CopyItemDynamicParameters(providerInstance, collection[0], destination, recurse, context2);
                }
            }
            return null;
        }

        private object CopyItemDynamicParameters(CmdletProvider providerInstance, string path, string destination, bool recurse, CmdletProviderContext context)
        {
            ContainerCmdletProvider containerProviderInstance = GetContainerProviderInstance(providerInstance);
            object obj2 = null;
            try
            {
                obj2 = containerProviderInstance.CopyItemDynamicParameters(path, destination, recurse, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("CopyItemDynamicParametersProviderException", SessionStateStrings.CopyItemDynamicParametersProviderException, containerProviderInstance.ProviderInfo, path, exception);
            }
            return obj2;
        }

        internal void CopyProperty(string[] sourcePaths, string sourceProperty, string destinationPath, string destinationProperty, CmdletProviderContext context)
        {
            if (sourcePaths == null)
            {
                throw PSTraceSource.NewArgumentNullException("sourcePaths");
            }
            if (sourceProperty == null)
            {
                throw PSTraceSource.NewArgumentNullException("sourceProperty");
            }
            if (destinationPath == null)
            {
                throw PSTraceSource.NewArgumentNullException("destinationPath");
            }
            if (destinationProperty == null)
            {
                throw PSTraceSource.NewArgumentNullException("destinationProperty");
            }
            foreach (string str in sourcePaths)
            {
                if (str == null)
                {
                    throw PSTraceSource.NewArgumentNullException("sourcePaths");
                }
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                Collection<string> collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(str, false, context, out info, out providerInstance);
                if (collection.Count > 0)
                {
                    Collection<string> include = context.Include;
                    Collection<string> exclude = context.Exclude;
                    string filter = context.Filter;
                    context.SetFilters(new Collection<string>(), new Collection<string>(), null);
                    Collection<string> collection4 = this.Globber.GetGlobbedProviderPathsFromMonadPath(destinationPath, false, context, out info, out providerInstance);
                    context.SetFilters(include, exclude, filter);
                    foreach (string str3 in collection)
                    {
                        foreach (string str4 in collection4)
                        {
                            this.CopyProperty(providerInstance, str3, sourceProperty, str4, destinationProperty, context);
                        }
                    }
                }
            }
        }

        internal Collection<PSObject> CopyProperty(string[] sourcePaths, string sourceProperty, string destinationPath, string destinationProperty, bool force, bool literalPath)
        {
            if (sourcePaths == null)
            {
                throw PSTraceSource.NewArgumentNullException("sourcePaths");
            }
            if (sourceProperty == null)
            {
                throw PSTraceSource.NewArgumentNullException("sourceProperty");
            }
            if (destinationPath == null)
            {
                throw PSTraceSource.NewArgumentNullException("destinationPath");
            }
            if (destinationProperty == null)
            {
                throw PSTraceSource.NewArgumentNullException("destinationProperty");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext) {
                Force = force,
                SuppressWildcardExpansion = literalPath
            };
            this.CopyProperty(sourcePaths, sourceProperty, destinationPath, destinationProperty, context);
            context.ThrowFirstErrorOrDoNothing();
            return context.GetAccumulatedObjects();
        }

        private void CopyProperty(CmdletProvider providerInstance, string sourcePath, string sourceProperty, string destinationPath, string destinationProperty, CmdletProviderContext context)
        {
            try
            {
                providerInstance.CopyProperty(sourcePath, sourceProperty, destinationPath, destinationProperty, context);
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("CopyPropertyProviderException", SessionStateStrings.CopyPropertyProviderException, providerInstance.ProviderInfo, sourcePath, exception);
            }
        }

        internal object CopyPropertyDynamicParameters(string path, string sourceProperty, string destinationPath, string destinationProperty, CmdletProviderContext context)
        {
            if (path != null)
            {
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                CmdletProviderContext context2 = new CmdletProviderContext(context);
                context2.SetFilters(new Collection<string>(), new Collection<string>(), null);
                Collection<string> collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context2, out info, out providerInstance);
                if (collection.Count > 0)
                {
                    return this.CopyPropertyDynamicParameters(providerInstance, collection[0], sourceProperty, destinationPath, destinationProperty, context2);
                }
            }
            return null;
        }

        private object CopyPropertyDynamicParameters(CmdletProvider providerInstance, string path, string sourceProperty, string destinationPath, string destinationProperty, CmdletProviderContext context)
        {
            object obj2 = null;
            try
            {
                obj2 = providerInstance.CopyPropertyDynamicParameters(path, sourceProperty, destinationPath, destinationProperty, context);
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("CopyPropertyDynamicParametersProviderException", SessionStateStrings.CopyPropertyDynamicParametersProviderException, providerInstance.ProviderInfo, path, exception);
            }
            return obj2;
        }

        internal void CopyProviders(SessionStateInternal ss)
        {
            if ((ss != null) && (ss.Providers != null))
            {
                this._providers = new Dictionary<string, List<ProviderInfo>>();
                foreach (KeyValuePair<string, List<ProviderInfo>> pair in ss._providers)
                {
                    this._providers.Add(pair.Key, pair.Value);
                }
            }
        }

        private void CopyRecurseToSingleContainer(CmdletProvider providerInstance, string sourcePath, string destinationPath, CmdletProviderContext context)
        {
            ContainerCmdletProvider containerProviderInstance = GetContainerProviderInstance(providerInstance);
            foreach (string str in this.GetChildNames(new string[] { sourcePath }, ReturnContainers.ReturnMatchingContainers, true, false, false))
            {
                if (context.Stopping)
                {
                    break;
                }
                string path = this.MakePath(providerInstance.ProviderInfo, sourcePath, str, context);
                this.CopyItem(containerProviderInstance, path, destinationPath, false, context);
            }
        }

        private void DoGetChildNamesManually(CmdletProvider providerInstance, string providerPath, string relativePath, ReturnContainers returnContainers, Collection<WildcardPattern> includeMatcher, Collection<WildcardPattern> excludeMatcher, CmdletProviderContext context, bool recurse)
        {
            string path = this.MakePath(providerInstance, providerPath, relativePath, context);
            CmdletProviderContext context2 = new CmdletProviderContext(context);
            try
            {
                this.GetChildNames(providerInstance, path, ReturnContainers.ReturnMatchingContainers, context2);
                foreach (PSObject obj2 in context2.GetAccumulatedObjects())
                {
                    if (context.Stopping)
                    {
                        return;
                    }
                    string baseObject = obj2.BaseObject as string;
                    if (((baseObject != null) && SessionStateUtilities.MatchesAnyWildcardPattern(baseObject, includeMatcher, true)) && !SessionStateUtilities.MatchesAnyWildcardPattern(baseObject, excludeMatcher, false))
                    {
                        string str3 = this.MakePath(providerInstance, relativePath, baseObject, context);
                        context.WriteObject(str3);
                    }
                }
                if (recurse)
                {
                    this.GetChildNames(providerInstance, path, ReturnContainers.ReturnAllContainers, context2);
                    foreach (PSObject obj3 in context2.GetAccumulatedObjects())
                    {
                        if (context.Stopping)
                        {
                            return;
                        }
                        string child = obj3.BaseObject as string;
                        if (child != null)
                        {
                            string str5 = this.MakePath(providerInstance, relativePath, child, context);
                            string str6 = this.MakePath(providerInstance, providerPath, str5, context);
                            if (this.IsItemContainer(providerInstance, str6, context))
                            {
                                this.DoGetChildNamesManually(providerInstance, providerPath, str5, returnContainers, includeMatcher, excludeMatcher, context, true);
                            }
                        }
                    }
                }
            }
            finally
            {
                context2.RemoveStopReferral();
            }
        }

        private void DoManualGetChildItems(CmdletProvider providerInstance, string path, bool recurse, CmdletProviderContext context, bool skipIsItemContainerCheck = false)
        {
            Collection<WildcardPattern> patterns = SessionStateUtilities.CreateWildcardsFromStrings(context.Include, WildcardOptions.IgnoreCase);
            Collection<WildcardPattern> collection2 = SessionStateUtilities.CreateWildcardsFromStrings(context.Exclude, WildcardOptions.IgnoreCase);
            if (skipIsItemContainerCheck || this.IsItemContainer(providerInstance, path, context))
            {
                CmdletProviderContext context2 = new CmdletProviderContext(context);
                Collection<PSObject> accumulatedObjects = null;
                Dictionary<string, bool> dictionary = null;
                try
                {
                    this.GetChildNames(providerInstance, path, recurse ? ReturnContainers.ReturnAllContainers : ReturnContainers.ReturnMatchingContainers, context2);
                    context2.WriteErrorsToContext(context);
                    accumulatedObjects = context2.GetAccumulatedObjects();
                    if (recurse && providerInstance.IsFilterSet())
                    {
                        context2.RemoveStopReferral();
                        context2 = new CmdletProviderContext(context);
                        Collection<PSObject> collection4 = new Collection<PSObject>();
                        dictionary = new Dictionary<string, bool>();
                        this.GetChildNames(providerInstance, path, ReturnContainers.ReturnMatchingContainers, context2);
                        foreach (PSObject obj2 in context2.GetAccumulatedObjects())
                        {
                            string baseObject = obj2.BaseObject as string;
                            if (baseObject != null)
                            {
                                dictionary[baseObject] = true;
                            }
                        }
                    }
                }
                finally
                {
                    context2.RemoveStopReferral();
                }
                for (int i = 0; i < accumulatedObjects.Count; i++)
                {
                    if (context.Stopping)
                    {
                        return;
                    }
                    string child = accumulatedObjects[i].BaseObject as string;
                    if (child != null)
                    {
                        string str3 = this.MakePath(providerInstance, path, child, context);
                        if (str3 != null)
                        {
                            if (SessionStateUtilities.MatchesAnyWildcardPattern(child, patterns, true) && !SessionStateUtilities.MatchesAnyWildcardPattern(child, collection2, false))
                            {
                                bool flag2 = true;
                                if (dictionary != null)
                                {
                                    bool flag3 = false;
                                    flag2 = dictionary.TryGetValue(child, out flag3);
                                }
                                if (flag2)
                                {
                                    this.GetItemPrivate(providerInstance, str3, context);
                                }
                            }
                            if (this.IsItemContainer(providerInstance, str3, context) && recurse)
                            {
                                if (context.Stopping)
                                {
                                    return;
                                }
                                this.DoManualGetChildItems(providerInstance, str3, recurse, context, true);
                            }
                        }
                    }
                }
            }
            else
            {
                string text = path;
                text = this.GetChildName(providerInstance, path, context, true);
                if (SessionStateUtilities.MatchesAnyWildcardPattern(text, patterns, true) && !SessionStateUtilities.MatchesAnyWildcardPattern(text, collection2, false))
                {
                    this.GetItemPrivate(providerInstance, path, context);
                }
            }
        }

        internal Collection<PSDriveInfo> Drives(string scope)
        {
            Dictionary<string, PSDriveInfo> dictionary = new Dictionary<string, PSDriveInfo>();
            SessionStateScope currentScope = this.currentScope;
            if (!string.IsNullOrEmpty(scope))
            {
                currentScope = this.GetScopeByID(scope);
            }
            SessionStateScopeEnumerator enumerator = new SessionStateScopeEnumerator(currentScope);
            foreach (SessionStateScope scope3 in (IEnumerable<SessionStateScope>) enumerator)
            {
                foreach (PSDriveInfo info in scope3.Drives)
                {
                    if (info != null)
                    {
                        bool flag = true;
                        if (info.IsAutoMounted)
                        {
                            flag = this.ValidateOrRemoveAutoMountedDrive(info, scope3);
                        }
                        if (flag && !dictionary.ContainsKey(info.Name))
                        {
                            dictionary[info.Name] = info;
                        }
                    }
                }
                if ((scope != null) && (scope.Length > 0))
                {
                    break;
                }
            }
            try
            {
                foreach (DriveInfo info2 in DriveInfo.GetDrives())
                {
                    if ((info2 != null) && (info2.DriveType != DriveType.Fixed))
                    {
                        string key = OSHelper.IsUnix ? info2.Name : info2.Name.Substring(0, 1);
                        if (!dictionary.ContainsKey(key))
                        {
                            PSDriveInfo info3 = this.AutomountFileSystemDrive(info2);
                            if (info3 != null)
                            {
                                dictionary[info3.Name] = info3;
                            }
                        }
                    }
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            Collection<PSDriveInfo> collection = new Collection<PSDriveInfo>();
            foreach (PSDriveInfo info4 in dictionary.Values)
            {
                collection.Add(info4);
            }
            return collection;
        }

        internal AliasInfo GetAlias(string aliasName)
        {
            return this.GetAlias(aliasName, CommandOrigin.Internal);
        }

        internal AliasInfo GetAlias(string aliasName, CommandOrigin origin)
        {
            AliasInfo valueToCheck = null;
            if (!string.IsNullOrEmpty(aliasName))
            {
                SessionStateScopeEnumerator enumerator = new SessionStateScopeEnumerator(this.currentScope);
                foreach (SessionStateScope scope in (IEnumerable<SessionStateScope>) enumerator)
                {
                    valueToCheck = scope.GetAlias(aliasName);
                    if (valueToCheck != null)
                    {
                        SessionState.ThrowIfNotVisible(origin, valueToCheck);
                        if (((valueToCheck.Options & ScopedItemOptions.Private) == ScopedItemOptions.None) || (scope == this.currentScope))
                        {
                            return valueToCheck;
                        }
                        valueToCheck = null;
                    }
                }
            }
            return valueToCheck;
        }

        internal AliasInfo GetAliasAtScope(string aliasName, string scopeID)
        {
            AliasInfo alias = null;
            if (!string.IsNullOrEmpty(aliasName))
            {
                SessionStateScope scopeByID = this.GetScopeByID(scopeID);
                alias = scopeByID.GetAlias(aliasName);
                if (((alias != null) && ((alias.Options & ScopedItemOptions.Private) != ScopedItemOptions.None)) && (scopeByID != this.currentScope))
                {
                    alias = null;
                }
            }
            return alias;
        }

        internal IEnumerable<string> GetAliasesByCommandName(string command)
        {
            SessionStateScopeEnumerator iteratorVariable0 = new SessionStateScopeEnumerator(this.currentScope);
            foreach (SessionStateScope iteratorVariable1 in (IEnumerable<SessionStateScope>) iteratorVariable0)
            {
                foreach (string iteratorVariable2 in iteratorVariable1.GetAliasesByCommandName(command))
                {
                    yield return iteratorVariable2;
                }
            }
        }

        internal IDictionary<string, AliasInfo> GetAliasTable()
        {
            Dictionary<string, AliasInfo> dictionary = new Dictionary<string, AliasInfo>(StringComparer.OrdinalIgnoreCase);
            SessionStateScopeEnumerator enumerator = new SessionStateScopeEnumerator(this.currentScope);
            foreach (SessionStateScope scope in (IEnumerable<SessionStateScope>) enumerator)
            {
                foreach (AliasInfo info in scope.AliasTable)
                {
                    if (!dictionary.ContainsKey(info.Name) && (((info.Options & ScopedItemOptions.Private) == ScopedItemOptions.None) || (scope == this.currentScope)))
                    {
                        dictionary.Add(info.Name, info);
                    }
                }
            }
            return dictionary;
        }

        internal IDictionary<string, AliasInfo> GetAliasTableAtScope(string scopeID)
        {
            Dictionary<string, AliasInfo> dictionary = new Dictionary<string, AliasInfo>(StringComparer.OrdinalIgnoreCase);
            SessionStateScope scopeByID = this.GetScopeByID(scopeID);
            foreach (AliasInfo info in scopeByID.AliasTable)
            {
                if (((info.Options & ScopedItemOptions.Private) == ScopedItemOptions.None) || (scopeByID == this.currentScope))
                {
                    dictionary.Add(info.Name, info);
                }
            }
            return dictionary;
        }

        internal object GetAutomaticVariableValue(AutomaticVariable variable)
        {
            SessionStateScopeEnumerator enumerator = new SessionStateScopeEnumerator(this.CurrentScope);
            object automaticVariableValue = AutomationNull.Value;
            foreach (SessionStateScope scope in (IEnumerable<SessionStateScope>) enumerator)
            {
                automaticVariableValue = scope.GetAutomaticVariableValue(variable);
                if (automaticVariableValue != AutomationNull.Value)
                {
                    return automaticVariableValue;
                }
            }
            return automaticVariableValue;
        }

        internal void GetChildItems(string path, bool recurse, CmdletProviderContext context)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            if (context == null)
            {
                throw PSTraceSource.NewArgumentNullException("context");
            }
            ProviderInfo info = null;
            if (recurse || LocationGlobber.ShouldPerformGlobbing(path, context))
            {
                bool flag = false;
                try
                {
                    if ((recurse && ((context.Include == null) || (context.Include.Count == 0))) && (!string.IsNullOrEmpty(path) && !this.IsItemContainer(path)))
                    {
                        string childName = this.GetChildName(path, context);
                        if (!string.Equals(childName, "*", StringComparison.OrdinalIgnoreCase) && (context.Include != null))
                        {
                            context.Include.Add(childName);
                            flag = true;
                        }
                        path = path.Substring(0, path.Length - childName.Length);
                    }
                    Collection<string> include = context.Include;
                    Collection<string> exclude = context.Exclude;
                    string filter = context.Filter;
                    if (recurse)
                    {
                        context.SetFilters(new Collection<string>(), new Collection<string>(), null);
                    }
                    CmdletProvider providerInstance = null;
                    Collection<string> collection3 = null;
                    try
                    {
                        collection3 = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, false, context, out info, out providerInstance);
                    }
                    finally
                    {
                        context.SetFilters(include, exclude, filter);
                    }
                    if (recurse)
                    {
                        this.GetContainerProviderInstance(info);
                    }
                    bool flag2 = !LocationGlobber.StringContainsGlobCharacters(path);
                    if ((((recurse && !flag2) && ((include != null) && (include.Count == 0))) || ((include != null) && (include.Count > 0))) || ((exclude != null) && (exclude.Count > 0)))
                    {
                        foreach (string str4 in collection3)
                        {
                            if (context.Stopping)
                            {
                                return;
                            }
                            this.DoManualGetChildItems(providerInstance, str4, recurse, context, false);
                        }
                    }
                    else
                    {
                        foreach (string str5 in collection3)
                        {
                            if (context.Stopping)
                            {
                                return;
                            }
                            if ((flag2 || recurse) && this.IsItemContainer(providerInstance, str5, context))
                            {
                                this.GetChildItems(providerInstance, str5, recurse, context);
                            }
                            else
                            {
                                this.GetItemPrivate(providerInstance, str5, context);
                            }
                        }
                    }
                }
                finally
                {
                    if (flag)
                    {
                        context.Include.Clear();
                    }
                }
            }
            else
            {
                PSDriveInfo drive = null;
                path = this.Globber.GetProviderPath(path, context, out info, out drive);
                if (drive != null)
                {
                    context.Drive = drive;
                }
                ContainerCmdletProvider containerProviderInstance = this.GetContainerProviderInstance(info);
                if ((path != null) && this.ItemExists(containerProviderInstance, path, context))
                {
                    if (this.IsItemContainer(containerProviderInstance, path, context))
                    {
                        this.GetChildItems(containerProviderInstance, path, recurse, context);
                    }
                    else
                    {
                        this.GetItemPrivate(containerProviderInstance, path, context);
                    }
                }
                else
                {
                    ItemNotFoundException exception = new ItemNotFoundException(path, "PathNotFound", SessionStateStrings.PathNotFound);
                    throw exception;
                }
            }
        }

        internal Collection<PSObject> GetChildItems(string[] paths, bool recurse, bool force, bool literalPath)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext) {
                Force = force,
                SuppressWildcardExpansion = literalPath
            };
            foreach (string str in paths)
            {
                if (str == null)
                {
                    throw PSTraceSource.NewArgumentNullException("paths");
                }
                this.GetChildItems(str, recurse, context);
            }
            context.ThrowFirstErrorOrDoNothing();
            return context.GetAccumulatedObjects();
        }

        private void GetChildItems(CmdletProvider providerInstance, string path, bool recurse, CmdletProviderContext context)
        {
            ContainerCmdletProvider containerProviderInstance = GetContainerProviderInstance(providerInstance);
            try
            {
                containerProviderInstance.GetChildItems(path, recurse, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("GetChildrenProviderException", SessionStateStrings.GetChildrenProviderException, containerProviderInstance.ProviderInfo, path, exception);
            }
        }

        internal object GetChildItemsDynamicParameters(string path, bool recurse, CmdletProviderContext context)
        {
            if (path != null)
            {
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                this.Globber.GetProviderPath(path, out info);
                if (!this.HasGetChildItemDynamicParameters(info))
                {
                    return null;
                }
                CmdletProviderContext context2 = new CmdletProviderContext(context);
                context2.SetFilters(new Collection<string>(), new Collection<string>(), null);
                Collection<string> collection = null;
                try
                {
                    collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context2, out info, out providerInstance);
                }
                catch (ItemNotFoundException)
                {
                    if (providerInstance == null)
                    {
                        throw;
                    }
                }
                if ((collection != null) && (collection.Count > 0))
                {
                    return this.GetChildItemsDynamicParameters(providerInstance, collection[0], recurse, context2);
                }
                if (providerInstance != null)
                {
                    PSDriveInfo drive = null;
                    string str = this.Globber.GetProviderPath(path, context, out info, out drive);
                    if (str != null)
                    {
                        return this.GetChildItemsDynamicParameters(providerInstance, str, recurse, context2);
                    }
                }
            }
            return null;
        }

        private object GetChildItemsDynamicParameters(CmdletProvider providerInstance, string path, bool recurse, CmdletProviderContext context)
        {
            ContainerCmdletProvider containerProviderInstance = GetContainerProviderInstance(providerInstance);
            object obj2 = null;
            try
            {
                obj2 = containerProviderInstance.GetChildItemsDynamicParameters(path, recurse, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("GetChildrenDynamicParametersProviderException", SessionStateStrings.GetChildrenDynamicParametersProviderException, containerProviderInstance.ProviderInfo, path, exception);
            }
            return obj2;
        }

        internal string GetChildName(string path)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
            string childName = this.GetChildName(path, context);
            context.ThrowFirstErrorOrDoNothing();
            tracer.WriteLine("result = {0}", new object[] { childName });
            return childName;
        }

        internal string GetChildName(string path, CmdletProviderContext context)
        {
            return this.GetChildName(path, context, false);
        }

        private string GetChildName(ProviderInfo provider, string path, CmdletProviderContext context)
        {
            CmdletProvider providerInstance = provider.CreateInstance();
            return this.GetChildName(providerInstance, path, context, true);
        }

        internal string GetChildName(string path, CmdletProviderContext context, bool useDefaultProvider)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            PSDriveInfo drive = null;
            ProviderInfo provider = null;
            string str = null;
            try
            {
                str = this.Globber.GetProviderPath(path, context, out provider, out drive);
            }
            catch (System.Management.Automation.DriveNotFoundException)
            {
                if (!useDefaultProvider)
                {
                    throw;
                }
                provider = this.PublicSessionState.Internal.GetSingleProvider("FileSystem");
				if (Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix)
				{
					str = path;
				}
				else {
                	str = path.Replace('/', '\\').TrimEnd(new char[] { '\\' });
				}
            }
            if (drive != null)
            {
                context.Drive = drive;
            }
            return this.GetChildName(provider, str, context);
        }

        private string GetChildName(CmdletProvider providerInstance, string path, CmdletProviderContext context, bool acceptNonContainerProviders)
        {
            string childName = null;
            NavigationCmdletProvider navigationProviderInstance = GetNavigationProviderInstance(providerInstance, acceptNonContainerProviders);
            if (navigationProviderInstance == null)
            {
                return path;
            }
            try
            {
                childName = navigationProviderInstance.GetChildName(path, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("GetChildNameProviderException", SessionStateStrings.GetChildNameProviderException, navigationProviderInstance.ProviderInfo, path, exception);
            }
            return childName;
        }

        private void GetChildNames(CmdletProvider providerInstance, string path, ReturnContainers returnContainers, CmdletProviderContext context)
        {
            ContainerCmdletProvider containerProviderInstance = GetContainerProviderInstance(providerInstance);
            try
            {
                containerProviderInstance.GetChildNames(path, returnContainers, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("GetChildNamesProviderException", SessionStateStrings.GetChildNamesProviderException, containerProviderInstance.ProviderInfo, path, exception);
            }
        }

        internal void GetChildNames(string path, ReturnContainers returnContainers, bool recurse, CmdletProviderContext context)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            Collection<WildcardPattern> includeMatcher = SessionStateUtilities.CreateWildcardsFromStrings(context.Include, WildcardOptions.IgnoreCase);
            Collection<WildcardPattern> excludeMatcher = SessionStateUtilities.CreateWildcardsFromStrings(context.Exclude, WildcardOptions.IgnoreCase);
            if (LocationGlobber.ShouldPerformGlobbing(path, context))
            {
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                CmdletProviderContext context2 = new CmdletProviderContext(context);
                context2.SetFilters(new Collection<string>(), new Collection<string>(), null);
                Collection<string> collection3 = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, false, context2, out info, out providerInstance);
                if (context2.Drive != null)
                {
                    context.Drive = context2.Drive;
                }
                bool flag = LocationGlobber.StringContainsGlobCharacters(path);
                foreach (string str in collection3)
                {
                    if (context.Stopping)
                    {
                        break;
                    }
                    if ((!flag || recurse) && this.IsItemContainer(providerInstance, str, context))
                    {
                        this.DoGetChildNamesManually(providerInstance, str, string.Empty, returnContainers, includeMatcher, excludeMatcher, context, recurse);
                    }
                    else if (providerInstance is NavigationCmdletProvider)
                    {
                        string text = this.GetChildName(providerInstance, str, context, false);
                        bool flag2 = SessionStateUtilities.MatchesAnyWildcardPattern(text, includeMatcher, true);
                        bool flag3 = SessionStateUtilities.MatchesAnyWildcardPattern(text, excludeMatcher, false);
                        if (flag2 && !flag3)
                        {
                            context.WriteObject(text);
                        }
                    }
                    else
                    {
                        context.WriteObject(str);
                    }
                }
            }
            else
            {
                ProviderInfo provider = null;
                PSDriveInfo drive = null;
                string str3 = this.Globber.GetProviderPath(path, context, out provider, out drive);
                ContainerCmdletProvider containerProviderInstance = this.GetContainerProviderInstance(provider);
                if (drive != null)
                {
                    context.Drive = drive;
                }
                if (!containerProviderInstance.ItemExists(str3, context))
                {
                    ItemNotFoundException exception = new ItemNotFoundException(str3, "PathNotFound", SessionStateStrings.PathNotFound);
                    throw exception;
                }
                if (recurse)
                {
                    this.DoGetChildNamesManually(containerProviderInstance, str3, string.Empty, returnContainers, includeMatcher, excludeMatcher, context, recurse);
                }
                else
                {
                    this.GetChildNames(containerProviderInstance, str3, returnContainers, context);
                }
            }
        }

        internal Collection<string> GetChildNames(string[] paths, ReturnContainers returnContainers, bool recurse, bool force, bool literalPath)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext) {
                Force = force,
                SuppressWildcardExpansion = literalPath
            };
            foreach (string str in paths)
            {
                if (str == null)
                {
                    throw PSTraceSource.NewArgumentNullException("paths");
                }
                this.GetChildNames(str, returnContainers, recurse, context);
            }
            context.ThrowFirstErrorOrDoNothing();
            Collection<PSObject> accumulatedObjects = context.GetAccumulatedObjects();
            Collection<string> collection2 = new Collection<string>();
            foreach (PSObject obj2 in accumulatedObjects)
            {
                collection2.Add(obj2.BaseObject as string);
            }
            return collection2;
        }

        internal object GetChildNamesDynamicParameters(string path, CmdletProviderContext context)
        {
            if (path == null)
            {
                return null;
            }
            ProviderInfo info = null;
            CmdletProvider providerInstance = null;
            CmdletProviderContext context2 = new CmdletProviderContext(context);
            context2.SetFilters(new Collection<string>(), new Collection<string>(), null);
            Collection<string> collection = null;
            try
            {
                collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context2, out info, out providerInstance);
            }
            catch (ItemNotFoundException)
            {
                if (providerInstance == null)
                {
                    throw;
                }
            }
            object obj2 = null;
            if ((collection != null) && (collection.Count > 0))
            {
                return this.GetChildNamesDynamicParameters(providerInstance, collection[0], context2);
            }
            if (providerInstance != null)
            {
                PSDriveInfo drive = null;
                string str = this.Globber.GetProviderPath(path, context, out info, out drive);
                if (str != null)
                {
                    obj2 = this.GetChildNamesDynamicParameters(providerInstance, str, context2);
                }
            }
            return obj2;
        }

        private object GetChildNamesDynamicParameters(CmdletProvider providerInstance, string path, CmdletProviderContext context)
        {
            ContainerCmdletProvider containerProviderInstance = GetContainerProviderInstance(providerInstance);
            object childNamesDynamicParameters = null;
            try
            {
                childNamesDynamicParameters = containerProviderInstance.GetChildNamesDynamicParameters(path, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("GetChildNamesDynamicParametersProviderException", SessionStateStrings.GetChildNamesDynamicParametersProviderException, containerProviderInstance.ProviderInfo, path, exception);
            }
            return childNamesDynamicParameters;
        }

        internal CmdletInfo GetCmdlet(string cmdletName)
        {
            return this.GetCmdlet(cmdletName, CommandOrigin.Internal);
        }

        internal CmdletInfo GetCmdlet(string cmdletName, CommandOrigin origin)
        {
            CmdletInfo valueToCheck = null;
            if (!string.IsNullOrEmpty(cmdletName))
            {
                SessionStateScopeEnumerator enumerator = new SessionStateScopeEnumerator(this.currentScope);
                foreach (SessionStateScope scope in (IEnumerable<SessionStateScope>) enumerator)
                {
                    valueToCheck = scope.GetCmdlet(cmdletName);
                    if (valueToCheck != null)
                    {
                        SessionState.ThrowIfNotVisible(origin, valueToCheck);
                        if (((valueToCheck.Options & ScopedItemOptions.Private) == ScopedItemOptions.None) || (scope == this.currentScope))
                        {
                            return valueToCheck;
                        }
                        valueToCheck = null;
                    }
                }
            }
            return valueToCheck;
        }

        internal CmdletInfo GetCmdletAtScope(string cmdletName, string scopeID)
        {
            CmdletInfo cmdlet = null;
            if (!string.IsNullOrEmpty(cmdletName))
            {
                SessionStateScope scopeByID = this.GetScopeByID(scopeID);
                cmdlet = scopeByID.GetCmdlet(cmdletName);
                if (((cmdlet != null) && ((cmdlet.Options & ScopedItemOptions.Private) != ScopedItemOptions.None)) && (scopeByID != this.currentScope))
                {
                    cmdlet = null;
                }
            }
            return cmdlet;
        }

        internal IDictionary<string, List<CmdletInfo>> GetCmdletTable()
        {
            Dictionary<string, List<CmdletInfo>> dictionary = new Dictionary<string, List<CmdletInfo>>(StringComparer.OrdinalIgnoreCase);
            SessionStateScopeEnumerator enumerator = new SessionStateScopeEnumerator(this.currentScope);
            foreach (SessionStateScope scope in (IEnumerable<SessionStateScope>) enumerator)
            {
                foreach (KeyValuePair<string, List<CmdletInfo>> pair in scope.CmdletTable)
                {
                    if (!dictionary.ContainsKey(pair.Key))
                    {
                        List<CmdletInfo> list = new List<CmdletInfo>();
                        foreach (CmdletInfo info in pair.Value)
                        {
                            if (((info.Options & ScopedItemOptions.Private) == ScopedItemOptions.None) || (scope == this.currentScope))
                            {
                                list.Add(info);
                            }
                        }
                        dictionary.Add(pair.Key, list);
                    }
                }
            }
            return dictionary;
        }

        internal IDictionary<string, List<CmdletInfo>> GetCmdletTableAtScope(string scopeID)
        {
            Dictionary<string, List<CmdletInfo>> dictionary = new Dictionary<string, List<CmdletInfo>>(StringComparer.OrdinalIgnoreCase);
            SessionStateScope scopeByID = this.GetScopeByID(scopeID);
            foreach (KeyValuePair<string, List<CmdletInfo>> pair in scopeByID.CmdletTable)
            {
                List<CmdletInfo> list = new List<CmdletInfo>();
                foreach (CmdletInfo info in pair.Value)
                {
                    if (((info.Options & ScopedItemOptions.Private) == ScopedItemOptions.None) || (scopeByID == this.currentScope))
                    {
                        list.Add(info);
                    }
                }
                dictionary.Add(pair.Key, list);
            }
            return dictionary;
        }

        private static ContainerCmdletProvider GetContainerProviderInstance(CmdletProvider providerInstance)
        {
            if (providerInstance == null)
            {
                throw PSTraceSource.NewArgumentNullException("providerInstance");
            }
            ContainerCmdletProvider provider = providerInstance as ContainerCmdletProvider;
            if (provider == null)
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "ContainerCmdletProvider_NotSupported", new object[0]);
            }
            return provider;
        }

        internal ContainerCmdletProvider GetContainerProviderInstance(ProviderInfo provider)
        {
            if (provider == null)
            {
                throw PSTraceSource.NewArgumentNullException("provider");
            }
            ContainerCmdletProvider providerInstance = this.GetProviderInstance(provider) as ContainerCmdletProvider;
            if (providerInstance == null)
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "ContainerCmdletProvider_NotSupported", new object[0]);
            }
            return providerInstance;
        }

        internal ContainerCmdletProvider GetContainerProviderInstance(string providerId)
        {
            if (providerId == null)
            {
                throw PSTraceSource.NewArgumentNullException("providerId");
            }
            ContainerCmdletProvider providerInstance = this.GetProviderInstance(providerId) as ContainerCmdletProvider;
            if (providerInstance == null)
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "ContainerCmdletProvider_NotSupported", new object[0]);
            }
            return providerInstance;
        }

        internal Collection<IContentReader> GetContentReader(string[] paths, CmdletProviderContext context)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            ProviderInfo info = null;
            CmdletProvider providerInstance = null;
            Collection<IContentReader> collection = new Collection<IContentReader>();
            foreach (string str in paths)
            {
                if (str == null)
                {
                    throw PSTraceSource.NewArgumentNullException("paths");
                }
                foreach (string str2 in this.Globber.GetGlobbedProviderPathsFromMonadPath(str, false, context, out info, out providerInstance))
                {
                    IContentReader item = this.GetContentReaderPrivate(providerInstance, str2, context);
                    if (item != null)
                    {
                        collection.Add(item);
                    }
                    context.ThrowFirstErrorOrDoNothing(true);
                }
            }
            return collection;
        }

        internal Collection<IContentReader> GetContentReader(string[] paths, bool force, bool literalPath)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext) {
                Force = force,
                SuppressWildcardExpansion = literalPath
            };
            Collection<IContentReader> contentReader = this.GetContentReader(paths, context);
            context.ThrowFirstErrorOrDoNothing();
            return contentReader;
        }

        internal object GetContentReaderDynamicParameters(string path, CmdletProviderContext context)
        {
            if (path != null)
            {
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                CmdletProviderContext context2 = new CmdletProviderContext(context);
                context2.SetFilters(new Collection<string>(), new Collection<string>(), null);
                Collection<string> collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context2, out info, out providerInstance);
                if (collection.Count > 0)
                {
                    return this.GetContentReaderDynamicParameters(providerInstance, collection[0], context2);
                }
            }
            return null;
        }

        private object GetContentReaderDynamicParameters(CmdletProvider providerInstance, string path, CmdletProviderContext context)
        {
            object contentReaderDynamicParameters = null;
            try
            {
                contentReaderDynamicParameters = providerInstance.GetContentReaderDynamicParameters(path, context);
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("GetContentReaderDynamicParametersProviderException", SessionStateStrings.GetContentReaderDynamicParametersProviderException, providerInstance.ProviderInfo, path, exception);
            }
            return contentReaderDynamicParameters;
        }

        private IContentReader GetContentReaderPrivate(CmdletProvider providerInstance, string path, CmdletProviderContext context)
        {
            IContentReader contentReader = null;
            try
            {
                contentReader = providerInstance.GetContentReader(path, context);
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("GetContentReaderProviderException", SessionStateStrings.GetContentReaderProviderException, providerInstance.ProviderInfo, path, exception);
            }
            return contentReader;
        }

        internal Collection<IContentWriter> GetContentWriter(string[] paths, CmdletProviderContext context)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            ProviderInfo info = null;
            CmdletProvider providerInstance = null;
            Collection<IContentWriter> collection = new Collection<IContentWriter>();
            foreach (string str in paths)
            {
                if (str == null)
                {
                    throw PSTraceSource.NewArgumentNullException("paths");
                }
                foreach (string str2 in this.Globber.GetGlobbedProviderPathsFromMonadPath(str, true, context, out info, out providerInstance))
                {
                    IContentWriter item = this.GetContentWriterPrivate(providerInstance, str2, context);
                    if (item != null)
                    {
                        collection.Add(item);
                    }
                }
            }
            return collection;
        }

        internal Collection<IContentWriter> GetContentWriter(string[] paths, bool force, bool literalPath)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext) {
                Force = force,
                SuppressWildcardExpansion = literalPath
            };
            Collection<IContentWriter> contentWriter = this.GetContentWriter(paths, context);
            context.ThrowFirstErrorOrDoNothing();
            return contentWriter;
        }

        internal object GetContentWriterDynamicParameters(string path, CmdletProviderContext context)
        {
            if (path != null)
            {
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                CmdletProviderContext context2 = new CmdletProviderContext(context);
                context2.SetFilters(new Collection<string>(), new Collection<string>(), null);
                Collection<string> collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context2, out info, out providerInstance);
                if (collection.Count > 0)
                {
                    return this.GetContentWriterDynamicParameters(providerInstance, collection[0], context2);
                }
            }
            return null;
        }

        private object GetContentWriterDynamicParameters(CmdletProvider providerInstance, string path, CmdletProviderContext context)
        {
            object contentWriterDynamicParameters = null;
            try
            {
                contentWriterDynamicParameters = providerInstance.GetContentWriterDynamicParameters(path, context);
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("GetContentWriterDynamicParametersProviderException", SessionStateStrings.GetContentWriterDynamicParametersProviderException, providerInstance.ProviderInfo, path, exception);
            }
            return contentWriterDynamicParameters;
        }

        private IContentWriter GetContentWriterPrivate(CmdletProvider providerInstance, string path, CmdletProviderContext context)
        {
            IContentWriter contentWriter = null;
            try
            {
                contentWriter = providerInstance.GetContentWriter(path, context);
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("GetContentWriterProviderException", SessionStateStrings.GetContentWriterProviderException, providerInstance.ProviderInfo, path, exception);
            }
            return contentWriter;
        }

        internal PSDriveInfo GetDrive(string name)
        {
            return this.GetDrive(name, true);
        }

        private PSDriveInfo GetDrive(string name, bool automount)
        {
            if (name == null)
            {
                throw PSTraceSource.NewArgumentNullException("name");
            }
            PSDriveInfo drive = null;
            SessionStateScopeEnumerator enumerator = new SessionStateScopeEnumerator(this.CurrentScope);
            int num = 0;
            foreach (SessionStateScope scope in (IEnumerable<SessionStateScope>) enumerator)
            {
                drive = scope.GetDrive(name);
                if (drive != null)
                {
                    if (drive.IsAutoMounted)
                    {
                        if (drive.IsAutoMountedManuallyRemoved)
                        {
                            System.Management.Automation.DriveNotFoundException exception = new System.Management.Automation.DriveNotFoundException(name, "DriveNotFound", SessionStateStrings.DriveNotFound);
                            throw exception;
                        }
                        if (!this.ValidateOrRemoveAutoMountedDrive(drive, scope))
                        {
                            drive = null;
                        }
                    }
                    if (drive != null)
                    {
                        tracer.WriteLine("Drive found in scope {0}", new object[] { num });
                        break;
                    }
                }
                num++;
            }
            if ((drive == null) && automount)
            {
                drive = this.AutomountBuiltInDrive(name);
            }
            if ((drive == null) && (this == this._context.TopLevelSessionState))
            {
                drive = this.AutomountFileSystemDrive(name);
            }
            if (drive == null)
            {
                System.Management.Automation.DriveNotFoundException exception2 = new System.Management.Automation.DriveNotFoundException(name, "DriveNotFound", SessionStateStrings.DriveNotFound);
                throw exception2;
            }
            return drive;
        }

        internal PSDriveInfo GetDrive(string name, string scopeID)
        {
            if (name == null)
            {
                throw PSTraceSource.NewArgumentNullException("name");
            }
            PSDriveInfo drive = null;
            if (!string.IsNullOrEmpty(scopeID))
            {
                SessionStateScope scopeByID = this.GetScopeByID(scopeID);
                drive = scopeByID.GetDrive(name);
                if (drive != null)
                {
                    if (drive.IsAutoMounted && !this.ValidateOrRemoveAutoMountedDrive(drive, scopeByID))
                    {
                        drive = null;
                    }
                    return drive;
                }
                if (scopeByID == this._globalScope)
                {
                    drive = this.AutomountFileSystemDrive(name);
                }
                return drive;
            }
            SessionStateScopeEnumerator enumerator = new SessionStateScopeEnumerator(this.CurrentScope);
            foreach (SessionStateScope scope in (IEnumerable<SessionStateScope>) enumerator)
            {
                drive = scope.GetDrive(name);
                if (drive != null)
                {
                    if (drive.IsAutoMounted && !this.ValidateOrRemoveAutoMountedDrive(drive, scope))
                    {
                        drive = null;
                    }
                    if (drive != null)
                    {
                        break;
                    }
                }
            }
            if (drive == null)
            {
                drive = this.AutomountFileSystemDrive(name);
            }
            return drive;
        }

        private static DriveCmdletProvider GetDriveProviderInstance(CmdletProvider providerInstance)
        {
            if (providerInstance == null)
            {
                throw PSTraceSource.NewArgumentNullException("providerInstance");
            }
            DriveCmdletProvider provider = providerInstance as DriveCmdletProvider;
            if (provider == null)
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "DriveCmdletProvider_NotSupported", new object[0]);
            }
            return provider;
        }

        internal DriveCmdletProvider GetDriveProviderInstance(ProviderInfo provider)
        {
            if (provider == null)
            {
                throw PSTraceSource.NewArgumentNullException("provider");
            }
            DriveCmdletProvider providerInstance = this.GetProviderInstance(provider) as DriveCmdletProvider;
            if (providerInstance == null)
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "DriveCmdletProvider_NotSupported", new object[0]);
            }
            return providerInstance;
        }

        internal DriveCmdletProvider GetDriveProviderInstance(string providerId)
        {
            if (providerId == null)
            {
                throw PSTraceSource.NewArgumentNullException("providerId");
            }
            DriveCmdletProvider providerInstance = this.GetProviderInstance(providerId) as DriveCmdletProvider;
            if (providerInstance == null)
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "DriveCmdletProvider_NotSupported", new object[0]);
            }
            return providerInstance;
        }

        internal Collection<PSDriveInfo> GetDrivesForProvider(string providerId)
        {
            if (string.IsNullOrEmpty(providerId))
            {
                return this.Drives(null);
            }
            this.GetSingleProvider(providerId);
            Collection<PSDriveInfo> collection = new Collection<PSDriveInfo>();
            foreach (PSDriveInfo info in this.Drives(null))
            {
                if ((info != null) && info.Provider.NameEquals(providerId))
                {
                    collection.Add(info);
                }
            }
            return collection;
        }

        internal FunctionInfo GetFunction(string name)
        {
            return this.GetFunction(name, CommandOrigin.Internal);
        }

        internal FunctionInfo GetFunction(string name, CommandOrigin origin)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            FunctionInfo current = null;
            FunctionLookupPath lookupPath = new FunctionLookupPath(name);
            FunctionScopeItemSearcher searcher = new FunctionScopeItemSearcher(this, lookupPath, origin);
            if (searcher.MoveNext())
            {
                current = (FunctionInfo)searcher.Current;
            }
            return current;
        }

        internal IDictionary GetFunctionTable()
        {
            SessionStateScopeEnumerator enumerator = new SessionStateScopeEnumerator(this.currentScope);
            Dictionary<string, FunctionInfo> dictionary = new Dictionary<string, FunctionInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (SessionStateScope scope in (IEnumerable<SessionStateScope>) enumerator)
            {
                foreach (FunctionInfo info in scope.FunctionTable.Values)
                {
                    if (!dictionary.ContainsKey(info.Name))
                    {
                        dictionary.Add(info.Name, info);
                    }
                }
            }
            return dictionary;
        }

        internal IDictionary<string, FunctionInfo> GetFunctionTableAtScope(string scopeID)
        {
            Dictionary<string, FunctionInfo> dictionary = new Dictionary<string, FunctionInfo>(StringComparer.OrdinalIgnoreCase);
            SessionStateScope scopeByID = this.GetScopeByID(scopeID);
            foreach (FunctionInfo info in scopeByID.FunctionTable.Values)
            {
                ScopedItemOptions none = ScopedItemOptions.None;
                FunctionInfo info2 = info;
                if (info2 != null)
                {
                    none = info2.Options;
                }
                else
                {
                    none = ((FilterInfo) info).Options;
                }
                if (((none & ScopedItemOptions.Private) == ScopedItemOptions.None) || (scopeByID == this.currentScope))
                {
                    dictionary.Add(info.Name, info);
                }
            }
            return dictionary;
        }

        internal void GetItem(string[] paths, CmdletProviderContext context)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            ProviderInfo info = null;
            CmdletProvider providerInstance = null;
            foreach (string str in paths)
            {
                if (str == null)
                {
                    throw PSTraceSource.NewArgumentNullException("paths");
                }
                foreach (string str2 in this.Globber.GetGlobbedProviderPathsFromMonadPath(str, false, context, out info, out providerInstance))
                {
                    this.GetItemPrivate(providerInstance, str2, context);
                }
            }
        }

        internal Collection<PSObject> GetItem(string[] paths, bool force, bool literalPath)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext) {
                Force = force,
                SuppressWildcardExpansion = literalPath
            };
            this.GetItem(paths, context);
            context.ThrowFirstErrorOrDoNothing();
            return context.GetAccumulatedObjects();
        }

        internal object GetItemDynamicParameters(string path, CmdletProviderContext context)
        {
            if (path != null)
            {
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                CmdletProviderContext context2 = new CmdletProviderContext(context);
                context2.SetFilters(new Collection<string>(), new Collection<string>(), null);
                Collection<string> collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context2, out info, out providerInstance);
                if (collection.Count > 0)
                {
                    return this.GetItemDynamicParameters(providerInstance, collection[0], context2);
                }
            }
            return null;
        }

        private object GetItemDynamicParameters(CmdletProvider providerInstance, string path, CmdletProviderContext context)
        {
            ItemCmdletProvider itemProviderInstance = GetItemProviderInstance(providerInstance);
            object itemDynamicParameters = null;
            try
            {
                itemDynamicParameters = itemProviderInstance.GetItemDynamicParameters(path, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("GetItemDynamicParametersProviderException", SessionStateStrings.GetItemDynamicParametersProviderException, itemProviderInstance.ProviderInfo, path, exception);
            }
            return itemDynamicParameters;
        }

        private void GetItemPrivate(CmdletProvider providerInstance, string path, CmdletProviderContext context)
        {
            ItemCmdletProvider itemProviderInstance = GetItemProviderInstance(providerInstance);
            try
            {
                itemProviderInstance.GetItem(path, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("GetItemProviderException", SessionStateStrings.GetItemProviderException, itemProviderInstance.ProviderInfo, path, exception);
            }
        }

        private static ItemCmdletProvider GetItemProviderInstance(CmdletProvider providerInstance)
        {
            if (providerInstance == null)
            {
                throw PSTraceSource.NewArgumentNullException("providerInstance");
            }
            ItemCmdletProvider provider = providerInstance as ItemCmdletProvider;
            if (provider == null)
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "ItemCmdletProvider_NotSupported", new object[0]);
            }
            return provider;
        }

        internal ItemCmdletProvider GetItemProviderInstance(ProviderInfo provider)
        {
            if (provider == null)
            {
                throw PSTraceSource.NewArgumentNullException("provider");
            }
            ItemCmdletProvider providerInstance = this.GetProviderInstance(provider) as ItemCmdletProvider;
            if (providerInstance == null)
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "ItemCmdletProvider_NotSupported", new object[0]);
            }
            return providerInstance;
        }

        internal ItemCmdletProvider GetItemProviderInstance(string providerId)
        {
            if (providerId == null)
            {
                throw PSTraceSource.NewArgumentNullException("providerId");
            }
            ItemCmdletProvider providerInstance = this.GetProviderInstance(providerId) as ItemCmdletProvider;
            if (providerInstance == null)
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "ItemCmdletProvider_NotSupported", new object[0]);
            }
            return providerInstance;
        }

        internal PathInfo GetNamespaceCurrentLocation(string namespaceID)
        {
            if (namespaceID == null)
            {
                throw PSTraceSource.NewArgumentNullException("namespaceID");
            }
            PSDriveInfo info = null;
            if (namespaceID.Length == 0)
            {
                this.ProvidersCurrentWorkingDrive.TryGetValue(this.CurrentDrive.Provider, out info);
            }
            else
            {
                this.ProvidersCurrentWorkingDrive.TryGetValue(this.GetSingleProvider(namespaceID), out info);
            }
            if (info == null)
            {
                System.Management.Automation.DriveNotFoundException exception = new System.Management.Automation.DriveNotFoundException(namespaceID, "DriveNotFound", SessionStateStrings.DriveNotFound);
                throw exception;
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext) {
                Drive = info
            };
            string path = null;
            if (info.Hidden)
            {
                if (LocationGlobber.IsProviderDirectPath(info.CurrentLocation))
                {
                    path = info.CurrentLocation;
                }
                else
                {
                    path = LocationGlobber.GetProviderQualifiedPath(info.CurrentLocation, info.Provider);
                }
            }
            else
            {
                path = LocationGlobber.GetDriveQualifiedPath(info.CurrentLocation, info);
            }
            PathInfo info2 = new PathInfo(info, info.Provider, path, new SessionState(this));
            tracer.WriteLine("result = {0}", new object[] { info2 });
            return info2;
        }

        internal NavigationCmdletProvider GetNavigationProviderInstance(ProviderInfo provider)
        {
            if (provider == null)
            {
                throw PSTraceSource.NewArgumentNullException("provider");
            }
            NavigationCmdletProvider providerInstance = this.GetProviderInstance(provider) as NavigationCmdletProvider;
            if (providerInstance == null)
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "NavigationCmdletProvider_NotSupported", new object[0]);
            }
            return providerInstance;
        }

        private static NavigationCmdletProvider GetNavigationProviderInstance(CmdletProvider providerInstance, bool acceptNonContainerProviders)
        {
            if (providerInstance == null)
            {
                throw PSTraceSource.NewArgumentNullException("providerInstance");
            }
            NavigationCmdletProvider provider = providerInstance as NavigationCmdletProvider;
            if ((provider == null) && !acceptNonContainerProviders)
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "NavigationCmdletProvider_NotSupported", new object[0]);
            }
            return provider;
        }

        internal string GetParentPath(string path, string root)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
            string str = this.GetParentPath(path, root, context);
            context.ThrowFirstErrorOrDoNothing();
            tracer.WriteLine("result = {0}", new object[] { str });
            return str;
        }

        internal string GetParentPath(string path, string root, CmdletProviderContext context)
        {
            return this.GetParentPath(path, root, context, false);
        }

        internal string GetParentPath(CmdletProvider providerInstance, string path, string root, CmdletProviderContext context)
        {
            NavigationCmdletProvider navigationProviderInstance = GetNavigationProviderInstance(providerInstance, false);
            string str = null;
            try
            {
                str = navigationProviderInstance.GetParentPath(path, root, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("GetParentPathProviderException", SessionStateStrings.GetParentPathProviderException, navigationProviderInstance.ProviderInfo, path, exception);
            }
            return str;
        }

        internal string GetParentPath(ProviderInfo provider, string path, string root, CmdletProviderContext context)
        {
            CmdletProvider providerInstance = this.GetProviderInstance(provider);
            return this.GetParentPath(providerInstance, path, root, context);
        }

        internal string GetParentPath(string path, string root, CmdletProviderContext context, bool useDefaultProvider)
        {
            string str4;
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            CmdletProviderContext context2 = new CmdletProviderContext(context);
            try
            {
                PSDriveInfo drive = null;
                ProviderInfo provider = null;
                try
                {
                    this.Globber.GetProviderPath(path, context2, out provider, out drive);
                }
                catch (System.Management.Automation.DriveNotFoundException)
                {
                    if (!useDefaultProvider)
                    {
                        throw;
                    }
                    provider = this.PublicSessionState.Internal.GetSingleProvider("FileSystem");
                }
                if (context2.HasErrors())
                {
                    context2.WriteErrorsToContext(context);
                    return null;
                }
                if (drive != null)
                {
                    context.Drive = drive;
                }
                bool isProviderQualified = false;
                bool isDriveQualified = false;
                string qualifier = null;
                string str2 = this.RemoveQualifier(path, out qualifier, out isProviderQualified, out isDriveQualified);
                string str3 = this.GetParentPath(provider, str2, root, context);
                if (!string.IsNullOrEmpty(qualifier) && !string.IsNullOrEmpty(str3))
                {
                    str3 = this.AddQualifier(str3, qualifier, isProviderQualified, isDriveQualified);
                }
                tracer.WriteLine("result = {0}", new object[] { str3 });
                str4 = str3;
            }
            finally
            {
                context2.RemoveStopReferral();
            }
            return str4;
        }

        internal static ISecurityDescriptorCmdletProvider GetPermissionProviderInstance(CmdletProvider providerInstance)
        {
            if (providerInstance == null)
            {
                throw PSTraceSource.NewArgumentNullException("providerInstance");
            }
            ISecurityDescriptorCmdletProvider provider = providerInstance as ISecurityDescriptorCmdletProvider;
            if (provider == null)
            {
                throw PSTraceSource.NewNotSupportedException("ProviderBaseSecurity", "ISecurityDescriptorCmdletProvider_NotSupported", new object[0]);
            }
            return provider;
        }

        private static string GetPossibleMatches(Collection<ProviderInfo> matchingProviders)
        {
            StringBuilder builder = new StringBuilder();
            foreach (ProviderInfo info in matchingProviders)
            {
                builder.AppendFormat(" {0}", info.FullName);
            }
            return builder.ToString();
        }

        internal Collection<PSObject> GetProperty(string[] paths, Collection<string> providerSpecificPickList, bool literalPath)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext) {
                SuppressWildcardExpansion = literalPath
            };
            this.GetProperty(paths, providerSpecificPickList, context);
            context.ThrowFirstErrorOrDoNothing();
            return context.GetAccumulatedObjects();
        }

        internal void GetProperty(string[] paths, Collection<string> providerSpecificPickList, CmdletProviderContext context)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            foreach (string str in paths)
            {
                if (str == null)
                {
                    throw PSTraceSource.NewArgumentNullException("paths");
                }
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                foreach (string str2 in this.Globber.GetGlobbedProviderPathsFromMonadPath(str, false, context, out info, out providerInstance))
                {
                    this.GetPropertyPrivate(providerInstance, str2, providerSpecificPickList, context);
                }
            }
        }

        internal object GetPropertyDynamicParameters(string path, Collection<string> providerSpecificPickList, CmdletProviderContext context)
        {
            if (path != null)
            {
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                CmdletProviderContext context2 = new CmdletProviderContext(context);
                context2.SetFilters(new Collection<string>(), new Collection<string>(), null);
                Collection<string> collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context2, out info, out providerInstance);
                if (collection.Count > 0)
                {
                    return this.GetPropertyDynamicParameters(providerInstance, collection[0], providerSpecificPickList, context2);
                }
            }
            return null;
        }

        private object GetPropertyDynamicParameters(CmdletProvider providerInstance, string path, Collection<string> providerSpecificPickList, CmdletProviderContext context)
        {
            object obj2 = null;
            try
            {
                obj2 = providerInstance.GetPropertyDynamicParameters(path, providerSpecificPickList, context);
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("GetPropertyDynamicParametersProviderException", SessionStateStrings.GetPropertyDynamicParametersProviderException, providerInstance.ProviderInfo, path, exception);
            }
            return obj2;
        }

        private void GetPropertyPrivate(CmdletProvider providerInstance, string path, Collection<string> providerSpecificPickList, CmdletProviderContext context)
        {
            try
            {
                providerInstance.GetProperty(path, providerSpecificPickList, context);
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("GetPropertyProviderException", SessionStateStrings.GetPropertyProviderException, providerInstance.ProviderInfo, path, exception);
            }
        }

        internal Collection<ProviderInfo> GetProvider(PSSnapinQualifiedName providerName)
        {
            Collection<ProviderInfo> collection = new Collection<ProviderInfo>();
            if (providerName == null)
            {
                ProviderNotFoundException exception = new ProviderNotFoundException(providerName.ToString(), SessionStateCategory.CmdletProvider, "ProviderNotFound", SessionStateStrings.ProviderNotFound, new object[0]);
                throw exception;
            }
			string pName = null;
            List<ProviderInfo> list = null;
            if (!this.Providers.TryGetValue(providerName.ShortName, out list))
            {
                MountDefaultDrive(providerName.ShortName, this._context);
				pName = (OSHelper.IsUnix && providerName.ShortName == "/") ? "FileSystem" : providerName.ShortName;
				if (pName == "env") { pName = "Environment"; }
				else if (pName == "cert") { pName = "Certificate"; }
				else if (pName == "reg") { pName = "Registry"; }
                if (!this.Providers.TryGetValue(pName, out list))
                {
                    ProviderNotFoundException exception2 = new ProviderNotFoundException(providerName.ToString(), SessionStateCategory.CmdletProvider, "ProviderNotFound", SessionStateStrings.ProviderNotFound, new object[0]);
                    throw exception2;
                }
            }
			if (string.IsNullOrEmpty (pName)) pName = providerName.PSSnapInName;
            if (this.ExecutionContext.IsSingleShell && !string.IsNullOrEmpty(providerName.PSSnapInName))
            {
                foreach (ProviderInfo info in list)
                {
                    if (string.Equals(info.PSSnapInName, pName, StringComparison.OrdinalIgnoreCase) || string.Equals(info.ModuleName, pName, StringComparison.OrdinalIgnoreCase))
                    {
                        collection.Add(info);
                    }
                }
                return collection;
            }
            foreach (ProviderInfo info2 in list)
            {
                collection.Add(info2);
            }
            return collection;
        }

        internal Collection<ProviderInfo> GetProvider(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            PSSnapinQualifiedName instance = PSSnapinQualifiedName.GetInstance(name);
            if (instance == null)
            {
                ProviderNotFoundException exception = new ProviderNotFoundException(name, SessionStateCategory.CmdletProvider, "ProviderNotFoundBadFormat", SessionStateStrings.ProviderNotFoundBadFormat, new object[0]);
                throw exception;
            }
            return this.GetProvider(instance);
        }

        internal CmdletProvider GetProviderInstance(ProviderInfo provider)
        {
            if (provider == null)
            {
                throw PSTraceSource.NewArgumentNullException("provider");
            }
            return provider.CreateInstance();
        }

        internal CmdletProvider GetProviderInstance(string providerId)
        {
            if (providerId == null)
            {
                throw PSTraceSource.NewArgumentNullException("providerId");
            }
            ProviderInfo singleProvider = this.GetSingleProvider(providerId);
            return this.GetProviderInstance(singleProvider);
        }

        private string GetProviderName(ProviderConfigurationEntry entry)
        {
            string name = entry.Name;
            if (entry.PSSnapIn != null)
            {
                name = string.Format(CultureInfo.InvariantCulture, @"{0}\{1}", new object[] { entry.PSSnapIn.Name, entry.Name });
            }
            return name;
        }

        private string GetProviderRootFromSpecifiedRoot(string root, ProviderInfo provider)
        {
            string str = root;
            SessionState state = new SessionState(this._context.TopLevelSessionState);
            Collection<string> resolvedProviderPathFromPSPath = null;
            ProviderInfo info = null;
            try
            {
                resolvedProviderPathFromPSPath = state.Path.GetResolvedProviderPathFromPSPath(root, out info);
                if (((resolvedProviderPathFromPSPath != null) && (resolvedProviderPathFromPSPath.Count == 1)) && provider.NameEquals(info.FullName))
                {
                    ProviderIntrinsics intrinsics = new ProviderIntrinsics(this);
                    if (intrinsics.Item.Exists(root))
                    {
                        str = resolvedProviderPathFromPSPath[0];
                    }
                }
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (System.Management.Automation.DriveNotFoundException)
            {
            }
            catch (ProviderNotFoundException)
            {
            }
            catch (ItemNotFoundException)
            {
            }
            catch (NotSupportedException)
            {
            }
            catch (InvalidOperationException)
            {
            }
            catch (ProviderInvocationException)
            {
            }
            catch (ArgumentException)
            {
            }
            return str;
        }

        internal SessionStateScope GetScopeByID(int scopeID)
        {
            SessionStateScope currentScope = this.currentScope;
            int actualValue = scopeID;
            while ((scopeID > 0) && (currentScope != null))
            {
                currentScope = currentScope.Parent;
                scopeID--;
            }
            if ((currentScope == null) && (scopeID >= 0))
            {
                throw PSTraceSource.NewArgumentOutOfRangeException("scopeID", actualValue, "SessionStateStrings", "ScopeIDExceedsAvailableScopes", new object[] { actualValue });
            }
            return currentScope;
        }

        internal SessionStateScope GetScopeByID(string scopeID)
        {
            SessionStateScope currentScope = this.currentScope;
            if (!string.IsNullOrEmpty(scopeID))
            {
                if (string.Equals(scopeID, "GLOBAL", StringComparison.OrdinalIgnoreCase))
                {
                    return this._globalScope;
                }
                if (string.Equals(scopeID, "LOCAL", StringComparison.OrdinalIgnoreCase))
                {
                    return this.currentScope;
                }
                if (string.Equals(scopeID, "PRIVATE", StringComparison.OrdinalIgnoreCase))
                {
                    return this.currentScope;
                }
                if (string.Equals(scopeID, "SCRIPT", StringComparison.OrdinalIgnoreCase))
                {
                    return this.currentScope.ScriptScope;
                }
                try
                {
                    int num = int.Parse(scopeID, Thread.CurrentThread.CurrentCulture);
                    if (num < 0)
                    {
                        throw PSTraceSource.NewArgumentOutOfRangeException("scopeID", scopeID);
                    }
                    currentScope = this.GetScopeByID(num) ?? this.currentScope;
                }
                catch (FormatException)
                {
                    throw PSTraceSource.NewArgumentException("scopeID");
                }
                catch (OverflowException)
                {
                    throw PSTraceSource.NewArgumentOutOfRangeException("scopeID", scopeID);
                }
            }
            return currentScope;
        }

        private void GetScopeVariableTable(SessionStateScope scope, Dictionary<string, PSVariable> result, bool includePrivate)
        {
            foreach (KeyValuePair<string, PSVariable> pair in scope.Variables)
            {
                if (!result.ContainsKey(pair.Key))
                {
                    PSVariable variable = pair.Value;
                    if (!variable.IsPrivate || includePrivate)
                    {
                        result.Add(pair.Key, variable);
                    }
                }
            }
            foreach (MutableTuple tuple in scope.DottedScopes)
            {
                tuple.GetVariableTable(result, includePrivate);
            }
            if (scope.LocalsTuple != null)
            {
                scope.LocalsTuple.GetVariableTable(result, includePrivate);
            }
        }

        internal Collection<PSObject> GetSecurityDescriptor(string path, AccessControlSections sections)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
            this.GetSecurityDescriptor(path, sections, context);
            context.ThrowFirstErrorOrDoNothing();
            Collection<PSObject> accumulatedObjects = context.GetAccumulatedObjects();
            if (accumulatedObjects == null)
            {
                accumulatedObjects = new Collection<PSObject>();
            }
            return accumulatedObjects;
        }

        internal void GetSecurityDescriptor(string path, AccessControlSections sections, CmdletProviderContext context)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            ProviderInfo info = null;
            CmdletProvider providerInstance = null;
            foreach (string str in this.Globber.GetGlobbedProviderPathsFromMonadPath(path, false, context, out info, out providerInstance))
            {
                this.GetSecurityDescriptor(providerInstance, str, sections, context);
            }
        }

        private void GetSecurityDescriptor(CmdletProvider providerInstance, string path, AccessControlSections sections, CmdletProviderContext context)
        {
            GetPermissionProviderInstance(providerInstance);
            try
            {
                providerInstance.GetSecurityDescriptor(path, sections, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("GetSecurityDescriptorProviderException", SessionStateStrings.GetSecurityDescriptorProviderException, providerInstance.ProviderInfo, path, exception);
            }
        }

        internal ProviderInfo GetSingleProvider(string name)
        {
            Collection<ProviderInfo> provider = this.GetProvider(name);
            if (provider.Count == 1)
            {
                return provider[0];
            }
            if (provider.Count == 0)
            {
                ProviderNotFoundException exception = new ProviderNotFoundException(name, SessionStateCategory.CmdletProvider, "ProviderNotFound", SessionStateStrings.ProviderNotFound, new object[0]);
                throw exception;
            }
            throw NewAmbiguousProviderName(name, provider);
        }

        internal PSVariable GetVariable(string name)
        {
            return this.GetVariable(name, CommandOrigin.Internal);
        }

        internal PSVariable GetVariable(string name, CommandOrigin origin)
        {
            if (name == null)
            {
                throw PSTraceSource.NewArgumentNullException("name");
            }
            VariablePath variablePath = new VariablePath(name, VariablePathFlags.Unqualified | VariablePathFlags.Variable);
            SessionStateScope scope = null;
            return this.GetVariableItem(variablePath, out scope, origin);
        }

        internal PSVariable GetVariableAtScope(string name, string scopeID)
        {
            if (name == null)
            {
                throw PSTraceSource.NewArgumentNullException("name");
            }
            VariablePath path = new VariablePath(name);
            SessionStateScope scopeByID = null;
            scopeByID = this.GetScopeByID(scopeID);
            PSVariable variable = null;
            if (path.IsVariable)
            {
                variable = scopeByID.GetVariable(path.QualifiedName);
            }
            return variable;
        }

        internal PSVariable GetVariableItem(VariablePath variablePath, out SessionStateScope scope)
        {
            return this.GetVariableItem(variablePath, out scope, CommandOrigin.Internal);
        }

        internal PSVariable GetVariableItem(VariablePath variablePath, out SessionStateScope scope, CommandOrigin origin)
        {
            scope = null;
            if (variablePath == null)
            {
                throw PSTraceSource.NewArgumentNullException("variablePath");
            }
            VariableScopeItemSearcher searcher = new VariableScopeItemSearcher(this, variablePath, origin);
            PSVariable current = null;
            if (searcher.MoveNext())
            {
                current = (PSVariable)searcher.Current;
                scope = searcher.CurrentLookupScope;
            }
            return current;
        }

        internal IDictionary<string, PSVariable> GetVariableTable()
        {
            SessionStateScopeEnumerator enumerator = new SessionStateScopeEnumerator(this.currentScope);
            Dictionary<string, PSVariable> result = new Dictionary<string, PSVariable>(StringComparer.OrdinalIgnoreCase);
            foreach (SessionStateScope scope in (IEnumerable<SessionStateScope>) enumerator)
            {
                this.GetScopeVariableTable(scope, result, scope == this.currentScope);
            }
            return result;
        }

        internal IDictionary<string, PSVariable> GetVariableTableAtScope(string scopeID)
        {
            Dictionary<string, PSVariable> result = new Dictionary<string, PSVariable>(StringComparer.OrdinalIgnoreCase);
            this.GetScopeVariableTable(this.GetScopeByID(scopeID), result, true);
            return result;
        }

        internal object GetVariableValue(string name)
        {
            if (name == null)
            {
                throw PSTraceSource.NewArgumentNullException("name");
            }
            VariablePath variablePath = new VariablePath(name);
            CmdletProviderContext context = null;
            SessionStateScope scope = null;
            return this.GetVariableValue(variablePath, out context, out scope);
        }

        internal object GetVariableValue(string name, object defaultValue)
        {
            object variableValue = this.GetVariableValue(name);
            if (variableValue == null)
            {
                variableValue = defaultValue;
            }
            return variableValue;
        }

        internal object GetVariableValue(VariablePath variablePath, out CmdletProviderContext context, out SessionStateScope scope)
        {
            context = null;
            scope = null;
            object obj2 = null;
            if (variablePath.IsVariable)
            {
                PSVariable variableItem = this.GetVariableItem(variablePath, out scope);
                if (variableItem != null)
                {
                    obj2 = variableItem.Value;
                }
                return obj2;
            }
            return this.GetVariableValueFromProvider(variablePath, out context, out scope, this.currentScope.ScopeOrigin);
        }

        internal object GetVariableValueAtScope(string name, string scopeID)
        {
            if (name == null)
            {
                throw PSTraceSource.NewArgumentNullException("name");
            }
            VariablePath path = new VariablePath(name);
            SessionStateScope scopeByID = null;
            scopeByID = this.GetScopeByID(scopeID);
            object obj2 = null;
            if (path.IsVariable)
            {
                obj2 = scopeByID.GetVariable(path.QualifiedName);
            }
            else
            {
                PSDriveInfo drive = scopeByID.GetDrive(path.DriveName);
                if (drive != null)
                {
                    CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext) {
                        Drive = drive
                    };
                    Collection<IContentReader> contentReader = null;
                    try
                    {
                        contentReader = this.GetContentReader(new string[] { path.QualifiedName }, context);
                    }
                    catch (ItemNotFoundException)
                    {
                        return null;
                    }
                    catch (System.Management.Automation.DriveNotFoundException)
                    {
                        return null;
                    }
                    catch (ProviderNotFoundException)
                    {
                        return null;
                    }
                    catch (NotImplementedException exception)
                    {
                        ProviderInfo provider = null;
                        this.Globber.GetProviderPath(path.QualifiedName, out provider);
                        throw this.NewProviderInvocationException("ProviderCannotBeUsedAsVariable", SessionStateStrings.ProviderCannotBeUsedAsVariable, provider, path.QualifiedName, exception, false);
                    }
                    catch (NotSupportedException exception2)
                    {
                        ProviderInfo info3 = null;
                        this.Globber.GetProviderPath(path.QualifiedName, out info3);
                        throw this.NewProviderInvocationException("ProviderCannotBeUsedAsVariable", SessionStateStrings.ProviderCannotBeUsedAsVariable, info3, path.QualifiedName, exception2, false);
                    }
                    if ((contentReader == null) || (contentReader.Count == 0))
                    {
                        return null;
                    }
                    if (contentReader.Count > 1)
                    {
                        foreach (IContentReader reader in contentReader)
                        {
                            reader.Close();
                        }
                        PSArgumentException e = PSTraceSource.NewArgumentException("path", "SessionStateStrings", "VariablePathResolvedToMultiple", new object[] { name });
                        ProviderInfo info4 = null;
                        this.Globber.GetProviderPath(path.QualifiedName, out info4);
                        throw this.NewProviderInvocationException("ProviderVariableSyntaxInvalid", SessionStateStrings.ProviderVariableSyntaxInvalid, info4, path.QualifiedName, e);
                    }
                    IContentReader reader2 = contentReader[0];
                    try
                    {
                        IList list = reader2.Read(-1L);
                        if (list != null)
                        {
                            if (list.Count == 0)
                            {
                                obj2 = null;
                            }
                            else if (list.Count == 1)
                            {
                                obj2 = list[0];
                            }
                            else
                            {
                                obj2 = list;
                            }
                        }
                    }
                    catch (Exception exception4)
                    {
                        ProviderInfo info5 = null;
                        this.Globber.GetProviderPath(path.QualifiedName, out info5);
                        CommandProcessorBase.CheckForSevereException(exception4);
                        ProviderInvocationException exception5 = new ProviderInvocationException("ProviderContentReadError", SessionStateStrings.ProviderContentReadError, info5, path.QualifiedName, exception4);
                        throw exception5;
                    }
                    finally
                    {
                        reader2.Close();
                    }
                }
            }
            if (obj2 != null)
            {
                PSVariable variable = obj2 as PSVariable;
                if (variable != null)
                {
                    return variable.Value;
                }
                try
                {
                    DictionaryEntry entry = (DictionaryEntry) obj2;
                    obj2 = entry.Value;
                }
                catch (InvalidCastException)
                {
                }
            }
            return obj2;
        }

        internal object GetVariableValueFromProvider(VariablePath variablePath, out CmdletProviderContext context, out SessionStateScope scope, CommandOrigin origin)
        {
            scope = null;
            if (variablePath == null)
            {
                throw PSTraceSource.NewArgumentNullException("variablePath");
            }
            context = null;
            DriveScopeItemSearcher searcher = new DriveScopeItemSearcher(this, variablePath);
            object obj2 = null;
            if (searcher.MoveNext())
            {
                PSDriveInfo current = (PSDriveInfo)searcher.Current;
                if (current == null)
                {
                    return obj2;
                }
                context = new CmdletProviderContext(this.ExecutionContext, origin);
                context.Drive = current;
                Collection<IContentReader> contentReader = null;
                try
                {
                    contentReader = this.GetContentReader(new string[] { variablePath.QualifiedName }, context);
                }
                catch (ItemNotFoundException)
                {
                    return obj2;
                }
                catch (System.Management.Automation.DriveNotFoundException)
                {
                    return obj2;
                }
                catch (ProviderNotFoundException)
                {
                    return obj2;
                }
                catch (NotImplementedException exception)
                {
                    ProviderInfo provider = null;
                    this.Globber.GetProviderPath(variablePath.QualifiedName, out provider);
                    throw this.NewProviderInvocationException("ProviderCannotBeUsedAsVariable", SessionStateStrings.ProviderCannotBeUsedAsVariable, provider, variablePath.QualifiedName, exception, false);
                }
                catch (NotSupportedException exception2)
                {
                    ProviderInfo info3 = null;
                    this.Globber.GetProviderPath(variablePath.QualifiedName, out info3);
                    throw this.NewProviderInvocationException("ProviderCannotBeUsedAsVariable", SessionStateStrings.ProviderCannotBeUsedAsVariable, info3, variablePath.QualifiedName, exception2, false);
                }
                if ((contentReader == null) || (contentReader.Count == 0))
                {
                    return obj2;
                }
                if (contentReader.Count > 1)
                {
                    foreach (IContentReader reader in contentReader)
                    {
                        reader.Close();
                    }
                    PSArgumentException e = PSTraceSource.NewArgumentException("path", "SessionStateStrings", "VariablePathResolvedToMultiple", new object[] { variablePath.QualifiedName });
                    ProviderInfo info4 = null;
                    this.Globber.GetProviderPath(variablePath.QualifiedName, out info4);
                    throw this.NewProviderInvocationException("ProviderVariableSyntaxInvalid", SessionStateStrings.ProviderVariableSyntaxInvalid, info4, variablePath.QualifiedName, e);
                }
                IContentReader reader2 = contentReader[0];
                try
                {
                    IList list = reader2.Read(-1L);
                    if (list == null)
                    {
                        return obj2;
                    }
                    if (list.Count == 0)
                    {
                        return null;
                    }
                    if (list.Count == 1)
                    {
                        return list[0];
                    }
                    return list;
                }
                catch (Exception exception4)
                {
                    ProviderInfo info5 = null;
                    this.Globber.GetProviderPath(variablePath.QualifiedName, out info5);
                    CommandProcessorBase.CheckForSevereException(exception4);
                    ProviderInvocationException exception5 = new ProviderInvocationException("ProviderContentReadError", SessionStateStrings.ProviderContentReadError, info5, variablePath.QualifiedName, exception4);
                    throw exception5;
                }
                finally
                {
                    reader2.Close();
                }
            }
            return obj2;
        }

        internal bool HasChildItems(string path, CmdletProviderContext context)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            ProviderInfo info = null;
            CmdletProvider providerInstance = null;
            Collection<string> collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, false, context, out info, out providerInstance);
            bool flag = false;
            foreach (string str in collection)
            {
                flag = this.HasChildItems(providerInstance, str, context);
                if (flag)
                {
                    break;
                }
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        internal bool HasChildItems(string providerId, string path)
        {
            bool flag = false;
            if (string.IsNullOrEmpty(providerId))
            {
                throw PSTraceSource.NewArgumentException("providerId");
            }
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
            flag = this.HasChildItems(providerId, path, context);
            context.ThrowFirstErrorOrDoNothing();
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        private bool HasChildItems(CmdletProvider providerInstance, string path, CmdletProviderContext context)
        {
            ContainerCmdletProvider containerProviderInstance = GetContainerProviderInstance(providerInstance);
            bool flag = false;
            try
            {
                flag = containerProviderInstance.HasChildItems(path, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("HasChildItemsProviderException", SessionStateStrings.HasChildItemsProviderException, containerProviderInstance.ProviderInfo, path, exception);
            }
            return flag;
        }

        internal bool HasChildItems(string path, bool force, bool literalPath)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext) {
                Force = force,
                SuppressWildcardExpansion = literalPath
            };
            bool flag = this.HasChildItems(path, context);
            context.ThrowFirstErrorOrDoNothing();
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        internal bool HasChildItems(string providerId, string path, CmdletProviderContext context)
        {
            ContainerCmdletProvider containerProviderInstance = this.GetContainerProviderInstance(providerId);
            bool flag = this.HasChildItems(containerProviderInstance, path, context);
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        private bool HasGetChildItemDynamicParameters(ProviderInfo providerInfo)
        {
            Type implementingType = providerInfo.ImplementingType;
            MethodInfo method = null;
            do
            {
                method = implementingType.GetMethod("GetChildItemsDynamicParameters", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                implementingType = implementingType.BaseType;
            }
            while (((method == null) && (implementingType != null)) && (implementingType != typeof(ContainerCmdletProvider)));
            return (method != null);
        }

        internal void InitializeFixedVariables()
        {
            PSVariable variable = new PSVariable("Host", this._context.EngineHostInterface, ScopedItemOptions.AllScope | ScopedItemOptions.Constant, RunspaceInit.PSHostDescription);
            this.GlobalScope.SetVariable(variable.Name, variable, false, true, this, CommandOrigin.Internal, true);
            string environmentVariable = Environment.GetEnvironmentVariable("HomeDrive");
            string str2 = Environment.GetEnvironmentVariable("HomePath");
            string str3 = environmentVariable + str2;
            variable = new PSVariable("HOME", str3, ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly, RunspaceInit.HOMEDescription);
            this.GlobalScope.SetVariable(variable.Name, variable, false, true, this, CommandOrigin.Internal, true);
            variable = new PSVariable("ExecutionContext", this._context.EngineIntrinsics, ScopedItemOptions.AllScope | ScopedItemOptions.Constant, RunspaceInit.ExecutionContextDescription);
            this.GlobalScope.SetVariable(variable.Name, variable, false, true, this, CommandOrigin.Internal, true);
            variable = new PSVariable("PSVersionTable", PSVersionInfo.GetPSVersionTable(), ScopedItemOptions.AllScope | ScopedItemOptions.Constant, RunspaceInit.PSVersionTableDescription);
            this.GlobalScope.SetVariable(variable.Name, variable, false, true, this, CommandOrigin.Internal, true);
            Process currentProcess = Process.GetCurrentProcess();
            variable = new PSVariable("PID", currentProcess.Id, ScopedItemOptions.AllScope | ScopedItemOptions.Constant, RunspaceInit.PIDDescription);
            this.GlobalScope.SetVariable(variable.Name, variable, false, true, this, CommandOrigin.Internal, true);
            variable = new PSCultureVariable();
            this.GlobalScope.SetVariable(variable.Name, variable, false, true, this, CommandOrigin.Internal, true);
            variable = new PSUICultureVariable();
            this.GlobalScope.SetVariable(variable.Name, variable, false, true, this, CommandOrigin.Internal, true);
            string shellID = this._context.ShellID;
            variable = new PSVariable("ShellId", shellID, ScopedItemOptions.AllScope | ScopedItemOptions.Constant, RunspaceInit.MshShellIdDescription);
            this.GlobalScope.SetVariable(variable.Name, variable, false, true, this, CommandOrigin.Internal, true);
            string applicationBase = "";
            try
            {
                applicationBase = Utils.GetApplicationBase(shellID);
            }
            catch (SecurityException)
            {
            }
            variable = new PSVariable("PSHOME", applicationBase, ScopedItemOptions.AllScope | ScopedItemOptions.Constant, RunspaceInit.PSHOMEDescription);
            this.GlobalScope.SetVariable(variable.Name, variable, false, true, this, CommandOrigin.Internal, true);
            this.SetConsoleVariable();
        }

        internal void InitializeProvider(CmdletProvider providerInstance, ProviderInfo provider, CmdletProviderContext context)
        {
            if (provider == null)
            {
                throw PSTraceSource.NewArgumentNullException("provider");
            }
            if (context == null)
            {
                context = new CmdletProviderContext(this.ExecutionContext);
            }
            List<PSDriveInfo> list = new List<PSDriveInfo>();
            DriveCmdletProvider driveProviderInstance = GetDriveProviderInstance(providerInstance);
            if (driveProviderInstance != null)
            {
                try
                {
                    Collection<PSDriveInfo> collection = driveProviderInstance.InitializeDefaultDrives(context);
                    if ((collection != null) && (collection.Count > 0))
                    {
                        list.AddRange(collection);
                        this.ProvidersCurrentWorkingDrive[provider] = collection[0];
                    }
                }
                catch (LoopFlowException)
                {
                    throw;
                }
                catch (PipelineStoppedException)
                {
                    throw;
                }
                catch (ActionPreferenceStopException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                    ProviderInvocationException exception2 = this.NewProviderInvocationException("InitializeDefaultDrivesException", SessionStateStrings.InitializeDefaultDrivesException, provider, string.Empty, exception);
                    context.WriteError(new ErrorRecord(exception2, "InitializeDefaultDrivesException", ErrorCategory.InvalidOperation, provider));
                }
            }
            if ((list != null) && (list.Count > 0))
            {
                foreach (PSDriveInfo info in list)
                {
                    if ((info != null) && provider.NameEquals(info.Provider.FullName))
                    {
                        try
                        {
                            PSDriveInfo newDrive = this.ValidateDriveWithProvider(driveProviderInstance, info, context, false);
                            if (newDrive != null)
                            {
                                this._globalScope.NewDrive(newDrive);
                            }
                        }
                        catch (SessionStateException exception3)
                        {
                            context.WriteError(exception3.ErrorRecord);
                        }
                    }
                }
            }
        }

        internal void InitializeSessionStateInternalSpecialVariables(bool clearVariablesTable)
        {
            if (clearVariablesTable)
            {
                this._globalScope.Variables.Clear();
                this._globalScope.AddSessionStateScopeDefaultVariables();
            }
            PSVariable variable = new PSVariable("Error", new ArrayList(), ScopedItemOptions.Constant);
            this._globalScope.SetVariable(variable.Name, variable, false, false, this, CommandOrigin.Internal, true);
            Collection<Attribute> attributes = new Collection<Attribute> {
                new ArgumentTypeConverterAttribute(new Type[] { typeof(DefaultParameterDictionary) })
            };
            PSVariable variable2 = new PSVariable("PSDefaultParameterValues", new DefaultParameterDictionary(), ScopedItemOptions.None, attributes, RunspaceInit.PSDefaultParameterValuesDescription);
            this._globalScope.SetVariable(variable2.Name, variable2, false, false, this, CommandOrigin.Internal, true);
        }

        internal void InvokeDefaultAction(string[] paths, bool literalPath)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext) {
                SuppressWildcardExpansion = literalPath
            };
            this.InvokeDefaultAction(paths, context);
            context.ThrowFirstErrorOrDoNothing();
        }

        internal void InvokeDefaultAction(string[] paths, CmdletProviderContext context)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            ProviderInfo info = null;
            CmdletProvider providerInstance = null;
            foreach (string str in paths)
            {
                if (str == null)
                {
                    throw PSTraceSource.NewArgumentNullException("paths");
                }
                Collection<string> collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(str, false, context, out info, out providerInstance);
                if (collection != null)
                {
                    foreach (string str2 in collection)
                    {
                        this.InvokeDefaultActionPrivate(providerInstance, str2, context);
                    }
                }
            }
        }

        internal object InvokeDefaultActionDynamicParameters(string path, CmdletProviderContext context)
        {
            if (path != null)
            {
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                CmdletProviderContext context2 = new CmdletProviderContext(context);
                context2.SetFilters(new Collection<string>(), new Collection<string>(), null);
                Collection<string> collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context2, out info, out providerInstance);
                if (collection.Count > 0)
                {
                    return this.InvokeDefaultActionDynamicParameters(providerInstance, collection[0], context2);
                }
            }
            return null;
        }

        private object InvokeDefaultActionDynamicParameters(CmdletProvider providerInstance, string path, CmdletProviderContext context)
        {
            ItemCmdletProvider itemProviderInstance = GetItemProviderInstance(providerInstance);
            object obj2 = null;
            try
            {
                obj2 = itemProviderInstance.InvokeDefaultActionDynamicParameters(path, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("InvokeDefaultActionDynamicParametersProviderException", SessionStateStrings.InvokeDefaultActionDynamicParametersProviderException, itemProviderInstance.ProviderInfo, path, exception);
            }
            return obj2;
        }

        private void InvokeDefaultActionPrivate(CmdletProvider providerInstance, string path, CmdletProviderContext context)
        {
            ItemCmdletProvider itemProviderInstance = GetItemProviderInstance(providerInstance);
            try
            {
                itemProviderInstance.InvokeDefaultAction(path, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("InvokeDefaultActionProviderException", SessionStateStrings.InvokeDefaultActionProviderException, itemProviderInstance.ProviderInfo, path, exception);
            }
        }

        internal bool IsCurrentLocationOrAncestor(string path, CmdletProviderContext context)
        {
            bool flag = false;
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            PSDriveInfo drive = null;
            ProviderInfo provider = null;
            string strA = this.Globber.GetProviderPath(path, context, out provider, out drive);
            if (drive != null)
            {
                tracer.WriteLine("Tracing drive", new object[0]);
                drive.Trace();
            }
            if (drive != null)
            {
                context.Drive = drive;
            }
            if (drive == this.CurrentDrive)
            {
                CmdletProviderContext context2 = new CmdletProviderContext(context);
                try
                {
                    strA = this.NormalizeRelativePath(path, null, context2);
                }
                catch (NotSupportedException)
                {
                }
                catch (LoopFlowException)
                {
                    throw;
                }
                catch (PipelineStoppedException)
                {
                    throw;
                }
                catch (ActionPreferenceStopException)
                {
                    throw;
                }
                finally
                {
                    context2.RemoveStopReferral();
                }
                if (context2.HasErrors())
                {
                    context2.ThrowFirstErrorOrDoNothing();
                }
                tracer.WriteLine("Provider path = {0}", new object[] { strA });
                PSDriveInfo info3 = null;
                ProviderInfo info4 = null;
                string strB = this.Globber.GetProviderPath(".", context, out info4, out info3);
                tracer.WriteLine("Current working path = {0}", new object[] { strB });
                tracer.WriteLine("Comparing {0} to {1}", new object[] { strA, strB });
                if (string.Compare(strA, strB, true, Thread.CurrentThread.CurrentCulture) == 0)
                {
                    tracer.WriteLine("The path is the current working directory", new object[0]);
                    flag = true;
                }
                else
                {
                    string str3 = strB;
                    while (str3.Length > 0)
                    {
                        str3 = this.GetParentPath(drive.Provider, str3, string.Empty, context);
                        tracer.WriteLine("Comparing {0} to {1}", new object[] { str3, strA });
                        if (string.Compare(str3, strA, true, Thread.CurrentThread.CurrentCulture) == 0)
                        {
                            tracer.WriteLine("The path is a parent of the current working directory: {0}", new object[] { str3 });
                            flag = true;
                            break;
                        }
                    }
                }
            }
            else
            {
                tracer.WriteLine("Drives are not the same", new object[0]);
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        internal bool IsItemContainer(string path)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
            bool flag = this.IsItemContainer(path, context);
            context.ThrowFirstErrorOrDoNothing();
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        internal bool IsItemContainer(string path, CmdletProviderContext context)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            ProviderInfo info = null;
            CmdletProvider providerInstance = null;
            bool flag = false;
            try
            {
                foreach (string str in this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context, out info, out providerInstance))
                {
                    flag = this.IsItemContainer(providerInstance, str, context);
                    if (!flag)
                    {
                        goto Label_0066;
                    }
                }
            }
            catch (ItemNotFoundException)
            {
                flag = false;
            }
        Label_0066:;
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        private bool IsItemContainer(CmdletProvider providerInstance, string path, CmdletProviderContext context)
        {
            bool flag = false;
            NavigationCmdletProvider navigationProviderInstance = null;
            try
            {
                navigationProviderInstance = GetNavigationProviderInstance(providerInstance, false);
                try
                {
                    flag = navigationProviderInstance.IsItemContainer(path, context);
                }
                catch (LoopFlowException)
                {
                    throw;
                }
                catch (PipelineStoppedException)
                {
                    throw;
                }
                catch (ActionPreferenceStopException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                    throw this.NewProviderInvocationException("IsItemContainerProviderException", SessionStateStrings.IsItemContainerProviderException, navigationProviderInstance.ProviderInfo, path, exception);
                }
            }
            catch (NotSupportedException)
            {
                try
                {
                    GetContainerProviderInstance(providerInstance);
                    if (path.Length == 0)
                    {
                        flag = true;
                    }
                    else
                    {
                        flag = false;
                    }
                }
                catch (NotSupportedException)
                {
                    flag = false;
                }
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        internal bool IsProviderLoaded(string name)
        {
            bool flag = false;
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            try
            {
                flag = this.GetSingleProvider(name) != null;
            }
            catch (ProviderNotFoundException)
            {
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        private static bool IsValidDriveName (string name)
		{
			bool flag = true;
			if (string.IsNullOrEmpty (name)) {
				return false;
			}
			if (Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix) {
				flag = true;
			}
            else if (name.IndexOfAny(_charactersInvalidInDriveName) >= 0)
            {
                flag = false;
            }
            return flag;
        }

        internal bool IsValidPath(string path)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
            bool flag = this.IsValidPath(path, context);
            context.ThrowFirstErrorOrDoNothing();
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        internal bool IsValidPath(string path, CmdletProviderContext context)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            ProviderInfo info = null;
            PSDriveInfo drive = null;
            string str = this.Globber.GetProviderPath(path, context, out info, out drive);
            ItemCmdletProvider itemProviderInstance = this.GetItemProviderInstance(info);
            bool flag = this.IsValidPath(itemProviderInstance, str, context);
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        private bool IsValidPath(CmdletProvider providerInstance, string path, CmdletProviderContext context)
        {
            ItemCmdletProvider itemProviderInstance = GetItemProviderInstance(providerInstance);
            bool flag = false;
            try
            {
                flag = itemProviderInstance.IsValidPath(path, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("IsValidPathProviderException", SessionStateStrings.IsValidPathProviderException, itemProviderInstance.ProviderInfo, path, exception);
            }
            return flag;
        }

        internal bool ItemExists(string path, CmdletProviderContext context)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            ProviderInfo info = null;
            CmdletProvider providerInstance = null;
            bool flag = false;
            try
            {
                foreach (string str in this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context, out info, out providerInstance))
                {
                    flag = this.ItemExists(providerInstance, str, context);
                    if (flag)
                    {
                        goto Label_0066;
                    }
                }
            }
            catch (ItemNotFoundException)
            {
                flag = false;
            }
        Label_0066:;
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        internal bool ItemExists(CmdletProvider providerInstance, string path, CmdletProviderContext context)
        {
            ItemCmdletProvider itemProviderInstance = GetItemProviderInstance(providerInstance);
            bool flag = false;
            try
            {
                flag = itemProviderInstance.ItemExists(path, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("ItemExistsProviderException", SessionStateStrings.ItemExistsProviderException, itemProviderInstance.ProviderInfo, path, exception);
            }
            return flag;
        }

        internal bool ItemExists(string path, bool force, bool literalPath)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext) {
                Force = force,
                SuppressWildcardExpansion = literalPath
            };
            bool flag = this.ItemExists(path, context);
            context.ThrowFirstErrorOrDoNothing();
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        internal object ItemExistsDynamicParameters(string path, CmdletProviderContext context)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            ProviderInfo info = null;
            CmdletProvider providerInstance = null;
            CmdletProviderContext context2 = new CmdletProviderContext(context);
            context2.SetFilters(new Collection<string>(), new Collection<string>(), null);
            Collection<string> collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context2, out info, out providerInstance);
            if (collection.Count > 0)
            {
                return this.ItemExistsDynamicParameters(providerInstance, collection[0], context2);
            }
            return null;
        }

        private object ItemExistsDynamicParameters(CmdletProvider providerInstance, string path, CmdletProviderContext context)
        {
            ContainerCmdletProvider containerProviderInstance = GetContainerProviderInstance(providerInstance);
            object obj2 = null;
            try
            {
                obj2 = containerProviderInstance.ItemExistsDynamicParameters(path, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("ItemExistsDynamicParametersProviderException", SessionStateStrings.ItemExistsDynamicParametersProviderException, containerProviderInstance.ProviderInfo, path, exception);
            }
            return obj2;
        }

        internal PathInfoStack LocationStack(string stackName)
        {
            if (string.IsNullOrEmpty(stackName))
            {
                stackName = this.defaultStackName;
            }
            Stack<PathInfo> stack = null;
            if (!this.workingLocationStack.TryGetValue(stackName, out stack))
            {
                if (!string.Equals(stackName, "default", StringComparison.OrdinalIgnoreCase))
                {
                    throw PSTraceSource.NewArgumentException("stackName");
                }
                stack = new Stack<PathInfo>();
            }
            return new PathInfoStack(stackName, stack);
        }

        internal string MakePath(string parent, string child)
        {
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
            return this.MakePath(parent, child, context);
        }

        internal string MakePath(string parent, string child, CmdletProviderContext context)
        {
            string path = null;
            if (context == null)
            {
                throw PSTraceSource.NewArgumentNullException("context");
            }
            if ((parent == null) && (child == null))
            {
                throw PSTraceSource.NewArgumentException("parent");
            }
            ProviderInfo provider = null;
            if (this.CurrentDrive != null)
            {
                provider = this.CurrentDrive.Provider;
            }
            if (context.Drive == null)
            {
                bool flag = LocationGlobber.IsProviderQualifiedPath(parent);
                bool flag2 = LocationGlobber.IsAbsolutePath(parent);
                if (flag || flag2)
                {
                    PSDriveInfo drive = null;
                    this.Globber.GetProviderPath(parent, context, out provider, out drive);
                    if ((drive == null) && flag)
                    {
                        drive = provider.HiddenDrive;
                    }
                    context.Drive = drive;
                }
                else
                {
                    context.Drive = this.CurrentDrive;
                }
                path = this.MakePath(provider, parent, child, context);
                if (flag2)
                {
                    path = LocationGlobber.GetDriveQualifiedPath(path, context.Drive);
                }
                else if (flag)
                {
                    path = LocationGlobber.GetProviderQualifiedPath(path, provider);
                }
            }
            else
            {
                provider = context.Drive.Provider;
                path = this.MakePath(provider, parent, child, context);
            }
            tracer.WriteLine("result = {0}", new object[] { path });
            return path;
        }

        internal string MakePath(CmdletProvider providerInstance, string parent, string child, CmdletProviderContext context)
        {
            NavigationCmdletProvider provider = providerInstance as NavigationCmdletProvider;
            if (provider != null)
            {
                try
                {
                    return provider.MakePath(parent, child, context);
                }
                catch (LoopFlowException)
                {
                    throw;
                }
                catch (PipelineStoppedException)
                {
                    throw;
                }
                catch (ActionPreferenceStopException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                    throw this.NewProviderInvocationException("MakePathProviderException", SessionStateStrings.MakePathProviderException, provider.ProviderInfo, parent, exception);
                }
            }
            if (!(providerInstance is ContainerCmdletProvider))
            {
                throw PSTraceSource.NewNotSupportedException();
            }
            return child;
        }

        internal string MakePath(ProviderInfo provider, string parent, string child, CmdletProviderContext context)
        {
            CmdletProvider providerInstance = provider.CreateInstance();
            return this.MakePath(providerInstance, parent, child, context);
        }

        internal static void MountDefaultDrive(string name, System.Management.Automation.ExecutionContext context)
        {
            if (CommandDiscovery.GetCommandDiscoveryPreference(context, SpecialVariables.PSModuleAutoLoadingPreferenceVarPath, "PSModuleAutoLoadingPreference") != PSModuleAutoLoadingPreference.None)
            {
                string str = null;
                if (string.Equals("Cert", name, StringComparison.OrdinalIgnoreCase) || string.Equals("Certificate", name, StringComparison.OrdinalIgnoreCase))
                {
                    str = "Microsoft.PowerShell.Security";
                }
                else if (string.Equals("WSMan", name, StringComparison.OrdinalIgnoreCase))
                {
                    str = "Microsoft.WSMan.Management";
                }
                if (!string.IsNullOrEmpty(str))
                {
                    tracer.WriteLine("Auto-mounting built-in drive: {0}", new object[] { name });
                    CommandInfo commandInfo = new CmdletInfo("Import-Module", typeof(ImportModuleCommand), null, null, context);
                    Command command = new Command(commandInfo);
                    tracer.WriteLine("Attempting to load module: {0}", new object[] { str });
                    try
                    {
                        PowerShell.Create(RunspaceMode.CurrentRunspace).AddCommand(command).AddParameter("Name", str).AddParameter("Scope", "GLOBAL").AddParameter("ErrorAction", ActionPreference.Ignore).AddParameter("WarningAction", ActionPreference.Ignore).AddParameter("Verbose", false).AddParameter("Debug", false).Invoke();
                    }
                    catch (Exception exception)
                    {
                        CommandProcessorBase.CheckForSevereException(exception);
                    }
                }
            }
        }

        internal void MoveItem(string[] paths, string destination, CmdletProviderContext context)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            if (destination == null)
            {
                throw PSTraceSource.NewArgumentNullException("destination");
            }
            ProviderInfo info = null;
            CmdletProvider providerInstance = null;
            Collection<PathInfo> collection = this.Globber.GetGlobbedMonadPathsFromMonadPath(destination, true, context, out providerInstance);
            if (collection.Count > 1)
            {
                ArgumentException exception = PSTraceSource.NewArgumentException("destination", "SessionStateStrings", "MoveItemOneDestination", new object[0]);
                context.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.InvalidArgument, destination));
            }
            else
            {
                foreach (string str in paths)
                {
                    if (str == null)
                    {
                        throw PSTraceSource.NewArgumentNullException("paths");
                    }
                    Collection<string> targetObject = this.Globber.GetGlobbedProviderPathsFromMonadPath(str, false, context, out info, out providerInstance);
                    if (((targetObject.Count > 1) && (collection.Count > 0)) && !this.IsItemContainer(collection[0].Path))
                    {
                        ArgumentException exception2 = PSTraceSource.NewArgumentException("path", "SessionStateStrings", "MoveItemPathMultipleDestinationNotContainer", new object[0]);
                        context.WriteError(new ErrorRecord(exception2, exception2.GetType().FullName, ErrorCategory.InvalidArgument, collection[0]));
                    }
                    else
                    {
                        PSDriveInfo drive = null;
                        ProviderInfo provider = null;
                        CmdletProviderContext context2 = new CmdletProviderContext(this.ExecutionContext);
                        string str2 = null;
                        if (collection.Count > 0)
                        {
                            str2 = this.Globber.GetProviderPath(collection[0].Path, context2, out provider, out drive);
                        }
                        else
                        {
                            str2 = this.Globber.GetProviderPath(destination, context2, out provider, out drive);
                        }
                        if (!string.Equals(info.FullName, provider.FullName, StringComparison.OrdinalIgnoreCase))
                        {
                            ArgumentException exception3 = PSTraceSource.NewArgumentException("destination", "SessionStateStrings", "MoveItemSourceAndDestinationNotSameProvider", new object[0]);
                            context.WriteError(new ErrorRecord(exception3, exception3.GetType().FullName, ErrorCategory.InvalidArgument, targetObject));
                        }
                        else
                        {
                            foreach (string str3 in targetObject)
                            {
                                this.MoveItemPrivate(providerInstance, str3, str2, context);
                            }
                        }
                    }
                }
            }
        }

        internal Collection<PSObject> MoveItem(string[] paths, string destination, bool force, bool literalPath)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext) {
                Force = force,
                SuppressWildcardExpansion = literalPath
            };
            this.MoveItem(paths, destination, context);
            context.ThrowFirstErrorOrDoNothing();
            return context.GetAccumulatedObjects();
        }

        internal object MoveItemDynamicParameters(string path, string destination, CmdletProviderContext context)
        {
            if (path != null)
            {
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                CmdletProviderContext context2 = new CmdletProviderContext(context);
                context2.SetFilters(new Collection<string>(), new Collection<string>(), null);
                Collection<string> collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context2, out info, out providerInstance);
                if (collection.Count > 0)
                {
                    return this.MoveItemDynamicParameters(providerInstance, collection[0], destination, context2);
                }
            }
            return null;
        }

        private object MoveItemDynamicParameters(CmdletProvider providerInstance, string path, string destination, CmdletProviderContext context)
        {
            NavigationCmdletProvider navigationProviderInstance = GetNavigationProviderInstance(providerInstance, false);
            object obj2 = null;
            try
            {
                obj2 = navigationProviderInstance.MoveItemDynamicParameters(path, destination, context);
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("MoveItemDynamicParametersProviderException", SessionStateStrings.MoveItemDynamicParametersProviderException, navigationProviderInstance.ProviderInfo, path, exception);
            }
            return obj2;
        }

        private void MoveItemPrivate(CmdletProvider providerInstance, string path, string destination, CmdletProviderContext context)
        {
            NavigationCmdletProvider navigationProviderInstance = GetNavigationProviderInstance(providerInstance, false);
            try
            {
                navigationProviderInstance.MoveItem(path, destination, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("MoveItemProviderException", SessionStateStrings.MoveItemProviderException, navigationProviderInstance.ProviderInfo, path, exception);
            }
        }

        internal void MoveProperty(string[] sourcePaths, string sourceProperty, string destinationPath, string destinationProperty, CmdletProviderContext context)
        {
            if (sourcePaths == null)
            {
                throw PSTraceSource.NewArgumentNullException("sourcePaths");
            }
            if (sourceProperty == null)
            {
                throw PSTraceSource.NewArgumentNullException("sourceProperty");
            }
            if (destinationPath == null)
            {
                throw PSTraceSource.NewArgumentNullException("destinationPath");
            }
            if (destinationProperty == null)
            {
                throw PSTraceSource.NewArgumentNullException("destinationProperty");
            }
            ProviderInfo info = null;
            CmdletProvider providerInstance = null;
            CmdletProviderContext context2 = new CmdletProviderContext(context);
            context2.SetFilters(new Collection<string>(), new Collection<string>(), null);
            Collection<string> targetObject = this.Globber.GetGlobbedProviderPathsFromMonadPath(destinationPath, false, context2, out info, out providerInstance);
            if (targetObject.Count > 1)
            {
                ArgumentException exception = PSTraceSource.NewArgumentException("destinationPath", "SessionStateStrings", "MovePropertyDestinationResolveToSingle", new object[0]);
                context.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.InvalidArgument, targetObject));
            }
            else
            {
                foreach (string str in sourcePaths)
                {
                    if (str == null)
                    {
                        throw PSTraceSource.NewArgumentNullException("sourcePaths");
                    }
                    foreach (string str2 in this.Globber.GetGlobbedProviderPathsFromMonadPath(str, false, context, out info, out providerInstance))
                    {
                        this.MoveProperty(providerInstance, str2, sourceProperty, targetObject[0], destinationProperty, context);
                    }
                }
            }
        }

        internal Collection<PSObject> MoveProperty(string[] sourcePaths, string sourceProperty, string destinationPath, string destinationProperty, bool force, bool literalPath)
        {
            if (sourcePaths == null)
            {
                throw PSTraceSource.NewArgumentNullException("sourcePaths");
            }
            if (sourceProperty == null)
            {
                throw PSTraceSource.NewArgumentNullException("sourceProperty");
            }
            if (destinationPath == null)
            {
                throw PSTraceSource.NewArgumentNullException("destinationPath");
            }
            if (destinationProperty == null)
            {
                throw PSTraceSource.NewArgumentNullException("destinationProperty");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext) {
                Force = force,
                SuppressWildcardExpansion = literalPath
            };
            this.MoveProperty(sourcePaths, sourceProperty, destinationPath, destinationProperty, context);
            context.ThrowFirstErrorOrDoNothing();
            return context.GetAccumulatedObjects();
        }

        private void MoveProperty(CmdletProvider providerInstance, string sourcePath, string sourceProperty, string destinationPath, string destinationProperty, CmdletProviderContext context)
        {
            try
            {
                providerInstance.MoveProperty(sourcePath, sourceProperty, destinationPath, destinationProperty, context);
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("MovePropertyProviderException", SessionStateStrings.MovePropertyProviderException, providerInstance.ProviderInfo, sourcePath, exception);
            }
        }

        internal object MovePropertyDynamicParameters(string path, string sourceProperty, string destinationPath, string destinationProperty, CmdletProviderContext context)
        {
            if (path != null)
            {
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                CmdletProviderContext context2 = new CmdletProviderContext(context);
                context2.SetFilters(new Collection<string>(), new Collection<string>(), null);
                Collection<string> collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context2, out info, out providerInstance);
                if (collection.Count > 0)
                {
                    return this.MovePropertyDynamicParameters(providerInstance, collection[0], sourceProperty, destinationPath, destinationProperty, context2);
                }
            }
            return null;
        }

        private object MovePropertyDynamicParameters(CmdletProvider providerInstance, string path, string sourceProperty, string destinationPath, string destinationProperty, CmdletProviderContext context)
        {
            object obj2 = null;
            try
            {
                obj2 = providerInstance.MovePropertyDynamicParameters(path, sourceProperty, destinationPath, destinationProperty, context);
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("MovePropertyDynamicParametersProviderException", SessionStateStrings.MovePropertyDynamicParametersProviderException, providerInstance.ProviderInfo, path, exception);
            }
            return obj2;
        }

        internal static ProviderNameAmbiguousException NewAmbiguousProviderName(string name, Collection<ProviderInfo> matchingProviders)
        {
            string possibleMatches = GetPossibleMatches(matchingProviders);
            return new ProviderNameAmbiguousException(name, "ProviderNameAmbiguous", SessionStateStrings.ProviderNameAmbiguous, matchingProviders, new object[] { possibleMatches });
        }

        internal PSDriveInfo NewDrive(PSDriveInfo drive, string scopeID)
        {
            if (drive == null)
            {
                throw PSTraceSource.NewArgumentNullException("drive");
            }
            PSDriveInfo baseObject = null;
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
            this.NewDrive(drive, scopeID, context);
            context.ThrowFirstErrorOrDoNothing();
            Collection<PSObject> accumulatedObjects = context.GetAccumulatedObjects();
            if (((accumulatedObjects != null) && (accumulatedObjects.Count > 0)) && !accumulatedObjects[0].immediateBaseObjectIsEmpty)
            {
                baseObject = (PSDriveInfo) accumulatedObjects[0].BaseObject;
            }
            return baseObject;
        }

        internal void NewDrive(PSDriveInfo drive, string scopeID, CmdletProviderContext context)
        {
            if (drive == null)
            {
                throw PSTraceSource.NewArgumentNullException("drive");
            }
            if (context == null)
            {
                throw PSTraceSource.NewArgumentNullException("context");
            }
            if (!IsValidDriveName(drive.Name))
            {
                throw PSTraceSource.NewArgumentException("drive.Name", "SessionStateStrings", "DriveNameIllegalCharacters", new object[0]);
            }
            PSDriveInfo newDrive = this.ValidateDriveWithProvider(drive, context, true);
            if (newDrive != null)
            {
                if (string.Compare(newDrive.Name, drive.Name, true, Thread.CurrentThread.CurrentCulture) != 0)
                {
                    throw this.NewProviderInvocationException("NewDriveProviderFailed", SessionStateStrings.NewDriveProviderFailed, drive.Provider, drive.Root, PSTraceSource.NewArgumentException("root"));
                }
                try
                {
                    SessionStateScope currentScope = this.currentScope;
                    if (!string.IsNullOrEmpty(scopeID))
                    {
                        currentScope = this.GetScopeByID(scopeID);
                    }
                    currentScope.NewDrive(newDrive);
                }
                catch (ArgumentException exception2)
                {
                    context.WriteError(new ErrorRecord(exception2, "NewDriveError", ErrorCategory.InvalidArgument, newDrive));
                    return;
                }
                catch (SessionStateException)
                {
                    throw;
                }
                if (this.ProvidersCurrentWorkingDrive[drive.Provider] == null)
                {
                    this.ProvidersCurrentWorkingDrive[drive.Provider] = drive;
                }
                context.WriteObject(newDrive);
            }
        }

        internal object NewDriveDynamicParameters(string providerId, CmdletProviderContext context)
        {
            if (providerId == null)
            {
                return null;
            }
            DriveCmdletProvider driveProviderInstance = this.GetDriveProviderInstance(providerId);
            object obj2 = null;
            try
            {
                obj2 = driveProviderInstance.NewDriveDynamicParameters(context);
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("NewDriveDynamicParametersProviderException", SessionStateStrings.NewDriveDynamicParametersProviderException, driveProviderInstance.ProviderInfo, null, exception);
            }
            return obj2;
        }

        internal Collection<PSObject> NewItem(string[] paths, string name, string type, object content, bool force)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext) {
                Force = force
            };
            this.NewItem(paths, name, type, content, context);
            context.ThrowFirstErrorOrDoNothing();
            return context.GetAccumulatedObjects();
        }

        internal void NewItem(string[] paths, string name, string type, object content, CmdletProviderContext context)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            foreach (string str in paths)
            {
                if (str == null)
                {
                    PSTraceSource.NewArgumentNullException("paths");
                }
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                Collection<string> collection = new Collection<string>();
                if (string.IsNullOrEmpty(name))
                {
                    PSDriveInfo info2;
                    string item = this.Globber.GetProviderPath(str, context, out info, out info2);
                    providerInstance = this.GetProviderInstance(info);
                    collection.Add(item);
                }
                else
                {
                    collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(str, true, context, out info, out providerInstance);
                }
                foreach (string str3 in collection)
                {
                    string path = str3;
                    if (!string.IsNullOrEmpty(name))
                    {
                        path = this.MakePath(providerInstance, str3, name, context);
                    }
                    if ((context.ExecutionContext.HasRunspaceEverUsedConstrainedLanguageMode && (providerInstance is FunctionProvider)) && string.Equals(type, "Directory", StringComparison.OrdinalIgnoreCase))
                    {
                        throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "DriveCmdletProvider_NotSupported", new object[0]);
                    }
                    this.NewItemPrivate(providerInstance, path, type, content, context);
                }
            }
        }

        internal object NewItemDynamicParameters(string path, string type, object newItemValue, CmdletProviderContext context)
        {
            if (path != null)
            {
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                CmdletProviderContext context2 = new CmdletProviderContext(context);
                context2.SetFilters(new Collection<string>(), new Collection<string>(), null);
                Collection<string> collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context2, out info, out providerInstance);
                if (collection.Count > 0)
                {
                    return this.NewItemDynamicParameters(providerInstance, collection[0], type, newItemValue, context2);
                }
            }
            return null;
        }

        private object NewItemDynamicParameters(CmdletProvider providerInstance, string path, string type, object newItemValue, CmdletProviderContext context)
        {
            ContainerCmdletProvider containerProviderInstance = GetContainerProviderInstance(providerInstance);
            object obj2 = null;
            try
            {
                obj2 = containerProviderInstance.NewItemDynamicParameters(path, type, newItemValue, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("NewItemDynamicParametersProviderException", SessionStateStrings.NewItemDynamicParametersProviderException, containerProviderInstance.ProviderInfo, path, exception);
            }
            return obj2;
        }

        private void NewItemPrivate(CmdletProvider providerInstance, string path, string type, object content, CmdletProviderContext context)
        {
            ContainerCmdletProvider containerProviderInstance = GetContainerProviderInstance(providerInstance);
            try
            {
                containerProviderInstance.NewItem(path, type, content, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("NewItemProviderException", SessionStateStrings.NewItemProviderException, containerProviderInstance.ProviderInfo, path, exception);
            }
        }

        internal void NewProperty(string[] paths, string property, string type, object value, CmdletProviderContext context)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            if (property == null)
            {
                throw PSTraceSource.NewArgumentNullException("property");
            }
            ProviderInfo info = null;
            CmdletProvider providerInstance = null;
            foreach (string str in paths)
            {
                if (str == null)
                {
                    throw PSTraceSource.NewArgumentNullException("paths");
                }
                foreach (string str2 in this.Globber.GetGlobbedProviderPathsFromMonadPath(str, false, context, out info, out providerInstance))
                {
                    this.NewProperty(providerInstance, str2, property, type, value, context);
                }
            }
        }

        internal Collection<PSObject> NewProperty(string[] paths, string property, string type, object value, bool force, bool literalPath)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            if (property == null)
            {
                throw PSTraceSource.NewArgumentNullException("property");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext) {
                Force = force,
                SuppressWildcardExpansion = literalPath
            };
            this.NewProperty(paths, property, type, value, context);
            context.ThrowFirstErrorOrDoNothing();
            return context.GetAccumulatedObjects();
        }

        private void NewProperty(CmdletProvider providerInstance, string path, string property, string type, object value, CmdletProviderContext context)
        {
            try
            {
                providerInstance.NewProperty(path, property, type, value, context);
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("NewPropertyProviderException", SessionStateStrings.NewPropertyProviderException, providerInstance.ProviderInfo, path, exception);
            }
        }

        internal object NewPropertyDynamicParameters(string path, string propertyName, string type, object value, CmdletProviderContext context)
        {
            if (path != null)
            {
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                CmdletProviderContext context2 = new CmdletProviderContext(context);
                context2.SetFilters(new Collection<string>(), new Collection<string>(), null);
                Collection<string> collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context2, out info, out providerInstance);
                if (collection.Count > 0)
                {
                    return this.NewPropertyDynamicParameters(providerInstance, collection[0], propertyName, type, value, context2);
                }
            }
            return null;
        }

        private object NewPropertyDynamicParameters(CmdletProvider providerInstance, string path, string propertyName, string type, object value, CmdletProviderContext context)
        {
            object obj2 = null;
            try
            {
                obj2 = providerInstance.NewPropertyDynamicParameters(path, propertyName, type, value, context);
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("NewPropertyDynamicParametersProviderException", SessionStateStrings.NewPropertyDynamicParametersProviderException, providerInstance.ProviderInfo, path, exception);
            }
            return obj2;
        }

        internal ProviderInfo NewProvider(ProviderInfo provider)
        {
            if (provider == null)
            {
                throw PSTraceSource.NewArgumentNullException("provider");
            }
            ProviderInfo info = this.ProviderExists(provider);
            if (info != null)
            {
                if (info.ImplementingType == provider.ImplementingType)
                {
                    return info;
                }
                SessionStateException exception = new SessionStateException(provider.Name, SessionStateCategory.CmdletProvider, "CmdletProviderAlreadyExists", SessionStateStrings.CmdletProviderAlreadyExists, ErrorCategory.ResourceExists, new object[0]);
                throw exception;
            }
            CmdletProvider providerInstance = provider.CreateInstance();
            CmdletProviderContext cmdletProviderContext = new CmdletProviderContext(this.ExecutionContext);
            ProviderInfo providerInfoToSet = null;
            try
            {
                providerInfoToSet = providerInstance.Start(provider, cmdletProviderContext);
                providerInstance.SetProviderInformation(providerInfoToSet);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception exception2)
            {
                CommandProcessorBase.CheckForSevereException(exception2);
                throw this.NewProviderInvocationException("ProviderStartException", SessionStateStrings.ProviderStartException, provider, null, exception2);
            }
            cmdletProviderContext.ThrowFirstErrorOrDoNothing(true);
            if (providerInfoToSet == null)
            {
                throw PSTraceSource.NewInvalidOperationException("SessionStateStrings", "InvalidProviderInfoNull", new object[0]);
            }
            if (providerInfoToSet != provider)
            {
                if (!string.Equals(providerInfoToSet.Name, provider.Name, StringComparison.OrdinalIgnoreCase))
                {
                    throw PSTraceSource.NewInvalidOperationException("SessionStateStrings", "InvalidProviderInfo", new object[0]);
                }
                provider = providerInfoToSet;
            }
            try
            {
                this.NewProviderEntry(provider);
            }
            catch (ArgumentException)
            {
                SessionStateException exception3 = new SessionStateException(provider.Name, SessionStateCategory.CmdletProvider, "CmdletProviderAlreadyExists", SessionStateStrings.CmdletProviderAlreadyExists, ErrorCategory.ResourceExists, new object[0]);
                throw exception3;
            }
            this.ProvidersCurrentWorkingDrive.Add(provider, null);
            bool flag = false;
            try
            {
                this.InitializeProvider(providerInstance, provider, cmdletProviderContext);
                cmdletProviderContext.ThrowFirstErrorOrDoNothing(true);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                flag = true;
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                flag = true;
                throw;
            }
            catch (NotSupportedException)
            {
                flag = false;
            }
            catch (SessionStateException)
            {
                flag = true;
                throw;
            }
            finally
            {
                if (flag)
                {
                    this.Providers.Remove(provider.Name.ToString());
                    this.ProvidersCurrentWorkingDrive.Remove(provider);
                    provider = null;
                }
            }
            return provider;
        }

        private void NewProviderEntry(ProviderInfo provider)
        {
            bool flag = false;
            if (!this.Providers.ContainsKey(provider.Name))
            {
                this.Providers.Add(provider.Name, new List<ProviderInfo>());
            }
            else
            {
                List<ProviderInfo> list = this.Providers[provider.Name];
                foreach (ProviderInfo info in list)
                {
                    if ((string.IsNullOrEmpty(provider.PSSnapInName) && string.Equals(info.Name, provider.Name, StringComparison.OrdinalIgnoreCase)) && info.GetType().Equals(provider.GetType()))
                    {
                        flag = true;
                    }
                    else if (string.Equals(info.PSSnapInName, provider.PSSnapInName, StringComparison.OrdinalIgnoreCase))
                    {
                        flag = true;
                    }
                }
            }
            if (!flag)
            {
                this.Providers[provider.Name].Add(provider);
            }
        }

        internal ProviderInvocationException NewProviderInvocationException(string resourceId, string resourceStr, ProviderInfo provider, string path, Exception e)
        {
            return this.NewProviderInvocationException(resourceId, resourceStr, provider, path, e, true);
        }

        internal ProviderInvocationException NewProviderInvocationException(string resourceId, string resourceStr, ProviderInfo provider, string path, Exception e, bool useInnerExceptionErrorMessage)
        {
            ProviderInvocationException exception = e as ProviderInvocationException;
            if (exception != null)
            {
                exception._providerInfo = provider;
                return exception;
            }
            exception = new ProviderInvocationException(resourceId, resourceStr, provider, path, e, useInnerExceptionErrorMessage);
            MshLog.LogProviderHealthEvent(this._context, provider.Name, exception, Severity.Warning);
            return exception;
        }

        internal SessionStateScope NewScope(bool isScriptScope)
        {
            SessionStateScope scope = new SessionStateScope(this.currentScope);
            if (isScriptScope)
            {
                scope.ScriptScope = scope;
            }
            return scope;
        }

        internal ObjectSecurity NewSecurityDescriptorFromPath(string path, AccessControlSections sections)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            ProviderInfo info = null;
            CmdletProvider providerInstance = null;
            Collection<string> collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, false, out info, out providerInstance);
            if (collection.Count != 1)
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            return this.NewSecurityDescriptorFromPath(providerInstance, collection[0], sections);
        }

        private ObjectSecurity NewSecurityDescriptorFromPath(CmdletProvider providerInstance, string path, AccessControlSections sections)
        {
            ObjectSecurity security = null;
            ISecurityDescriptorCmdletProvider permissionProviderInstance = GetPermissionProviderInstance(providerInstance);
            try
            {
                security = permissionProviderInstance.NewSecurityDescriptorFromPath(path, sections);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("NewSecurityDescriptorProviderException", SessionStateStrings.GetSecurityDescriptorProviderException, providerInstance.ProviderInfo, path, exception);
            }
            return security;
        }

        internal ObjectSecurity NewSecurityDescriptorOfType(CmdletProvider providerInstance, string type, AccessControlSections sections)
        {
            ObjectSecurity security = null;
            if (type == null)
            {
                throw PSTraceSource.NewArgumentNullException("type");
            }
            if (providerInstance == null)
            {
                throw PSTraceSource.NewArgumentNullException("providerInstance");
            }
            ISecurityDescriptorCmdletProvider permissionProviderInstance = GetPermissionProviderInstance(providerInstance);
            try
            {
                security = permissionProviderInstance.NewSecurityDescriptorOfType(type, sections);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("NewSecurityDescriptorProviderException", SessionStateStrings.GetSecurityDescriptorProviderException, providerInstance.ProviderInfo, type, exception);
            }
            return security;
        }

        internal ObjectSecurity NewSecurityDescriptorOfType(string providerId, string type, AccessControlSections sections)
        {
            CmdletProvider providerInstance = this.GetProviderInstance(providerId);
            return this.NewSecurityDescriptorOfType(providerInstance, type, sections);
        }

        internal object NewVariable(PSVariable variable, bool force)
        {
            if ((variable == null) || string.IsNullOrEmpty(variable.Name))
            {
                throw PSTraceSource.NewArgumentException("variable");
            }
            return this.CurrentScope.NewVariable(variable, force, this);
        }

        internal object NewVariableAtScope(PSVariable variable, string scopeID, bool force)
        {
            if ((variable == null) || string.IsNullOrEmpty(variable.Name))
            {
                throw PSTraceSource.NewArgumentException("variable");
            }
            return this.GetScopeByID(scopeID).NewVariable(variable, force, this);
        }

        internal string NormalizeRelativePath(string path, string basePath)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
            string str = this.NormalizeRelativePath(path, basePath, context);
            context.ThrowFirstErrorOrDoNothing();
            tracer.WriteLine("result = {0}", new object[] { str });
            return str;
        }

        internal string NormalizeRelativePath(string path, string basePath, CmdletProviderContext context)
        {
            string str2;
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            CmdletProviderContext context2 = new CmdletProviderContext(context);
            try
            {
                PSDriveInfo drive = null;
                ProviderInfo provider = null;
                string str = this.Globber.GetProviderPath(path, context2, out provider, out drive);
                if (context2.HasErrors())
                {
                    context2.WriteErrorsToContext(context);
                    return null;
                }
                if ((str == null) || (provider == null))
                {
                    Exception exception = PSTraceSource.NewArgumentException("path");
                    context.WriteError(new ErrorRecord(exception, "NormalizePathNullResult", ErrorCategory.InvalidArgument, path));
                    return null;
                }
                if (drive != null)
                {
                    context.Drive = drive;
                    if (((this.GetProviderInstance(provider) is NavigationCmdletProvider) && !string.IsNullOrEmpty(drive.Root)) && path.StartsWith(drive.Root, StringComparison.OrdinalIgnoreCase))
                    {
                        str = path;
                    }
                }
                str2 = this.NormalizeRelativePath(provider, str, basePath, context);
            }
            finally
            {
                context2.RemoveStopReferral();
            }
            return str2;
        }

        internal string NormalizeRelativePath(ProviderInfo provider, string path, string basePath, CmdletProviderContext context)
        {
            CmdletProvider providerInstance = this.GetProviderInstance(provider);
            NavigationCmdletProvider provider3 = providerInstance as NavigationCmdletProvider;
            if (provider3 != null)
            {
                try
                {
                    path = provider3.NormalizeRelativePath(path, basePath, context);
                    return path;
                }
                catch (LoopFlowException)
                {
                    throw;
                }
                catch (PipelineStoppedException)
                {
                    throw;
                }
                catch (ActionPreferenceStopException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                    throw this.NewProviderInvocationException("NormalizeRelativePathProviderException", SessionStateStrings.NormalizeRelativePathProviderException, provider3.ProviderInfo, path, exception);
                }
            }
            if (!(providerInstance is ContainerCmdletProvider))
            {
                throw PSTraceSource.NewNotSupportedException();
            }
            return path;
        }

        internal PathInfo PopLocation(string stackName)
        {
            if (string.IsNullOrEmpty(stackName))
            {
                stackName = this.defaultStackName;
            }
            if (WildcardPattern.ContainsWildcardCharacters(stackName))
            {
                bool flag = false;
                WildcardPattern pattern = new WildcardPattern(stackName, WildcardOptions.IgnoreCase);
                foreach (string str in this.workingLocationStack.Keys)
                {
                    if (pattern.IsMatch(str))
                    {
                        if (flag)
                        {
                            throw PSTraceSource.NewArgumentException("stackName", "SessionStateStrings", "StackNameResolvedToMultiple", new object[] { stackName });
                        }
                        flag = true;
                        stackName = str;
                    }
                }
            }
            PathInfo currentLocation = this.CurrentLocation;
            try
            {
                Stack<PathInfo> stack = null;
                if (!this.workingLocationStack.TryGetValue(stackName, out stack))
                {
                    if (!string.Equals(stackName, "default", StringComparison.OrdinalIgnoreCase))
                    {
                        throw PSTraceSource.NewArgumentException("stackName", "SessionStateStrings", "StackNotFound", new object[] { stackName });
                    }
                    return null;
                }
                PathInfo info2 = stack.Pop();
                string mshQualifiedPath = LocationGlobber.GetMshQualifiedPath(WildcardPattern.Escape(info2.Path), info2.GetDrive());
                currentLocation = this.SetLocation(mshQualifiedPath);
                if ((stack.Count == 0) && !string.Equals(stackName, "default", StringComparison.OrdinalIgnoreCase))
                {
                    this.workingLocationStack.Remove(stackName);
                }
            }
            catch (InvalidOperationException)
            {
            }
            tracer.WriteLine("result = {0}", new object[] { currentLocation });
            return currentLocation;
        }

        private ProviderInfo ProviderExists(ProviderInfo provider)
        {
            List<ProviderInfo> list = null;
            if (this.Providers.TryGetValue(provider.Name, out list))
            {
                foreach (ProviderInfo info in list)
                {
                    if (provider.NameEquals(info.FullName))
                    {
                        return info;
                    }
                }
            }
            return null;
        }

        internal void PushCurrentLocation(string stackName)
        {
            if (string.IsNullOrEmpty(stackName))
            {
                stackName = this.defaultStackName;
            }
            ProviderInfo provider = this.CurrentDrive.Provider;
            string mshQualifiedPath = LocationGlobber.GetMshQualifiedPath(this.CurrentDrive.CurrentLocation, this.CurrentDrive);
            PathInfo item = new PathInfo(this.CurrentDrive, provider, mshQualifiedPath, new SessionState(this));
            tracer.WriteLine("Pushing drive: {0} directory: {1}", new object[] { this.CurrentDrive.Name, mshQualifiedPath });
            Stack<PathInfo> stack = null;
            if (!this.workingLocationStack.TryGetValue(stackName, out stack))
            {
                stack = new Stack<PathInfo>();
                this.workingLocationStack[stackName] = stack;
            }
            stack.Push(item);
        }

        internal void RemoveAlias(string aliasName, bool force)
        {
            if (string.IsNullOrEmpty(aliasName))
            {
                throw PSTraceSource.NewArgumentException("aliasName");
            }
            SessionStateScopeEnumerator enumerator = new SessionStateScopeEnumerator(this.currentScope);
            foreach (SessionStateScope scope in (IEnumerable<SessionStateScope>) enumerator)
            {
                AliasInfo alias = scope.GetAlias(aliasName);
                if (alias != null)
                {
                    if (((alias.Options & ScopedItemOptions.Private) != ScopedItemOptions.None) && (scope != this.currentScope))
                    {
                        alias = null;
                    }
                    else
                    {
                        scope.RemoveAlias(aliasName, force);
                        break;
                    }
                }
            }
        }

        internal void RemoveCmdlet(string name, int index, bool force)
        {
            this.RemoveCmdlet(name, index, force, CommandOrigin.Internal);
        }

        internal void RemoveCmdlet(string name, int index, bool force, CommandOrigin origin)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            SessionStateScopeEnumerator enumerator = new SessionStateScopeEnumerator(this.currentScope);
            foreach (SessionStateScope scope in (IEnumerable<SessionStateScope>) enumerator)
            {
                CmdletInfo cmdlet = scope.GetCmdlet(name);
                if (cmdlet != null)
                {
                    if (((cmdlet.Options & ScopedItemOptions.Private) != ScopedItemOptions.None) && (scope != this.currentScope))
                    {
                        cmdlet = null;
                    }
                    else
                    {
                        scope.RemoveCmdlet(name, index, force);
                        break;
                    }
                }
            }
        }

        internal void RemoveCmdletEntry(string name, bool force)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            SessionStateScopeEnumerator enumerator = new SessionStateScopeEnumerator(this.currentScope);
            foreach (SessionStateScope scope in (IEnumerable<SessionStateScope>) enumerator)
            {
                CmdletInfo cmdlet = scope.GetCmdlet(name);
                if (cmdlet != null)
                {
                    if (((cmdlet.Options & ScopedItemOptions.Private) != ScopedItemOptions.None) && (scope != this.currentScope))
                    {
                        cmdlet = null;
                    }
                    else
                    {
                        scope.RemoveCmdletEntry(name, force);
                        break;
                    }
                }
            }
        }

        internal void RemoveDrive(PSDriveInfo drive, bool force, string scopeID)
        {
            if (drive == null)
            {
                throw PSTraceSource.NewArgumentNullException("drive");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
            this.RemoveDrive(drive, force, scopeID, context);
            if (context.HasErrors() && !force)
            {
                context.ThrowFirstErrorOrDoNothing();
            }
        }

        internal void RemoveDrive(string driveName, bool force, string scopeID)
        {
            if (driveName == null)
            {
                throw PSTraceSource.NewArgumentNullException("driveName");
            }
            PSDriveInfo drive = this.GetDrive(driveName, scopeID);
            if (drive == null)
            {
                System.Management.Automation.DriveNotFoundException exception = new System.Management.Automation.DriveNotFoundException(driveName, "DriveNotFound", SessionStateStrings.DriveNotFound);
                throw exception;
            }
            this.RemoveDrive(drive, force, scopeID);
        }

        internal void RemoveDrive(PSDriveInfo drive, bool force, string scopeID, CmdletProviderContext context)
        {
            bool flag = false;
            try
            {
                flag = this.CanRemoveDrive(drive, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (ProviderInvocationException)
            {
                if (!force)
                {
                    throw;
                }
            }
            if (flag || force)
            {
                if (!string.IsNullOrEmpty(scopeID))
                {
                    this.GetScopeByID(scopeID).RemoveDrive(drive);
                    if (this.ProvidersCurrentWorkingDrive[drive.Provider] == drive)
                    {
                        this.ProvidersCurrentWorkingDrive[drive.Provider] = null;
                    }
                }
                else
                {
                    SessionStateScopeEnumerator enumerator = new SessionStateScopeEnumerator(this.CurrentScope);
                    foreach (SessionStateScope scope in (IEnumerable<SessionStateScope>) enumerator)
                    {
                        try
                        {
                            PSDriveInfo info = scope.GetDrive(drive.Name);
                            if (info != null)
                            {
                                scope.RemoveDrive(drive);
                                if (this.ProvidersCurrentWorkingDrive[drive.Provider] == info)
                                {
                                    this.ProvidersCurrentWorkingDrive[drive.Provider] = null;
                                }
                                break;
                            }
                        }
                        catch (ArgumentException)
                        {
                        }
                    }
                }
            }
            else
            {
                PSInvalidOperationException replaceParentContainsErrorRecordException = PSTraceSource.NewInvalidOperationException("SessionStateStrings", "DriveRemovalPreventedByProvider", new object[] { drive.Name, drive.Provider });
                context.WriteError(new ErrorRecord(replaceParentContainsErrorRecordException.ErrorRecord, replaceParentContainsErrorRecordException));
            }
        }

        internal void RemoveDrive(string driveName, bool force, string scopeID, CmdletProviderContext context)
        {
            if (driveName == null)
            {
                throw PSTraceSource.NewArgumentNullException("driveName");
            }
            PSDriveInfo drive = this.GetDrive(driveName, scopeID);
            if (drive == null)
            {
                System.Management.Automation.DriveNotFoundException replaceParentContainsErrorRecordException = new System.Management.Automation.DriveNotFoundException(driveName, "DriveNotFound", SessionStateStrings.DriveNotFound);
                context.WriteError(new ErrorRecord(replaceParentContainsErrorRecordException.ErrorRecord, replaceParentContainsErrorRecordException));
            }
            else
            {
                this.RemoveDrive(drive, force, scopeID, context);
            }
        }

        internal void RemoveFunction(string name, bool force)
        {
            this.RemoveFunction(name, force, CommandOrigin.Internal);
        }

        internal void RemoveFunction(string name, PSModuleInfo module)
        {
            FunctionInfo function = this.GetFunction(name);
            if (((function != null) && (function.ScriptBlock != null)) && ((function.ScriptBlock.File != null) && function.ScriptBlock.File.Equals(module.Path, StringComparison.OrdinalIgnoreCase)))
            {
                this.RemoveFunction(name, true);
            }
        }

        internal void RemoveFunction(string name, bool force, CommandOrigin origin)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            SessionStateScope currentScope = this.currentScope;
            FunctionLookupPath lookupPath = new FunctionLookupPath(name);
            FunctionScopeItemSearcher searcher = new FunctionScopeItemSearcher(this, lookupPath, origin);
            if (searcher.MoveNext())
            {
                currentScope = searcher.CurrentLookupScope;
            }
            currentScope.RemoveFunction(name, force);
        }

        internal void RemoveItem(string[] paths, bool recurse, CmdletProviderContext context)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            foreach (string str in paths)
            {
                if (str == null)
                {
                    throw PSTraceSource.NewArgumentNullException("paths");
                }
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                foreach (string str2 in this.Globber.GetGlobbedProviderPathsFromMonadPath(str, false, context, out info, out providerInstance))
                {
                    this.RemoveItem(providerInstance, str2, recurse, context);
                }
            }
        }

        internal void RemoveItem(CmdletProvider providerInstance, string path, bool recurse, CmdletProviderContext context)
        {
            ContainerCmdletProvider containerProviderInstance = GetContainerProviderInstance(providerInstance);
            try
            {
                containerProviderInstance.RemoveItem(path, recurse, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("RemoveItemProviderException", SessionStateStrings.RemoveItemProviderException, containerProviderInstance.ProviderInfo, path, exception);
            }
        }

        internal void RemoveItem(string[] paths, bool recurse, bool force, bool literalPath)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext) {
                Force = force,
                SuppressWildcardExpansion = literalPath
            };
            this.RemoveItem(paths, recurse, context);
            context.ThrowFirstErrorOrDoNothing();
        }

        internal void RemoveItem(string providerId, string path, bool recurse, CmdletProviderContext context)
        {
            CmdletProvider providerInstance = this.GetProviderInstance(providerId);
            this.RemoveItem(providerInstance, path, recurse, context);
        }

        internal object RemoveItemDynamicParameters(string path, bool recurse, CmdletProviderContext context)
        {
            if (path != null)
            {
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                CmdletProviderContext context2 = new CmdletProviderContext(context);
                context2.SetFilters(new Collection<string>(), new Collection<string>(), null);
                Collection<string> collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context2, out info, out providerInstance);
                if (collection.Count > 0)
                {
                    return this.RemoveItemDynamicParameters(providerInstance, collection[0], recurse, context2);
                }
            }
            return null;
        }

        private object RemoveItemDynamicParameters(CmdletProvider providerInstance, string path, bool recurse, CmdletProviderContext context)
        {
            ContainerCmdletProvider containerProviderInstance = GetContainerProviderInstance(providerInstance);
            object obj2 = null;
            try
            {
                obj2 = containerProviderInstance.RemoveItemDynamicParameters(path, recurse, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("RemoveItemProviderException", SessionStateStrings.RemoveItemProviderException, containerProviderInstance.ProviderInfo, path, exception);
            }
            return obj2;
        }

        internal void RemoveProperty(string[] paths, string property, CmdletProviderContext context)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            if (property == null)
            {
                throw PSTraceSource.NewArgumentNullException("property");
            }
            foreach (string str in paths)
            {
                if (str == null)
                {
                    throw PSTraceSource.NewArgumentNullException("paths");
                }
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                foreach (string str2 in this.Globber.GetGlobbedProviderPathsFromMonadPath(str, false, context, out info, out providerInstance))
                {
                    this.RemoveProperty(providerInstance, str2, property, context);
                }
            }
        }

        private void RemoveProperty(CmdletProvider providerInstance, string path, string property, CmdletProviderContext context)
        {
            try
            {
                providerInstance.RemoveProperty(path, property, context);
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("RemovePropertyProviderException", SessionStateStrings.RemovePropertyProviderException, providerInstance.ProviderInfo, path, exception);
            }
        }

        internal void RemoveProperty(string[] paths, string property, bool force, bool literalPath)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            if (property == null)
            {
                throw PSTraceSource.NewArgumentNullException("property");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext) {
                Force = force,
                SuppressWildcardExpansion = literalPath
            };
            this.RemoveProperty(paths, property, context);
            context.ThrowFirstErrorOrDoNothing();
        }

        internal object RemovePropertyDynamicParameters(string path, string propertyName, CmdletProviderContext context)
        {
            if (path != null)
            {
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                CmdletProviderContext context2 = new CmdletProviderContext(context);
                context2.SetFilters(new Collection<string>(), new Collection<string>(), null);
                Collection<string> collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context2, out info, out providerInstance);
                if (collection.Count > 0)
                {
                    return this.RemovePropertyDynamicParameters(providerInstance, collection[0], propertyName, context2);
                }
            }
            return null;
        }

        private object RemovePropertyDynamicParameters(CmdletProvider providerInstance, string path, string propertyName, CmdletProviderContext context)
        {
            object obj2 = null;
            try
            {
                obj2 = providerInstance.RemovePropertyDynamicParameters(path, propertyName, context);
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("RemovePropertyDynamicParametersProviderException", SessionStateStrings.RemovePropertyDynamicParametersProviderException, providerInstance.ProviderInfo, path, exception);
            }
            return obj2;
        }

        private void RemoveProvider(ProviderConfigurationEntry entry)
        {
            try
            {
                CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
                string providerName = this.GetProviderName(entry);
                this.RemoveProvider(providerName, true, context);
                context.ThrowFirstErrorOrDoNothing();
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                this.ExecutionContext.ReportEngineStartupError(exception);
            }
        }

        internal void RemoveProvider(string providerName, bool force, CmdletProviderContext context)
        {
            if (context == null)
            {
                throw PSTraceSource.NewArgumentNullException("context");
            }
            if (string.IsNullOrEmpty(providerName))
            {
                throw PSTraceSource.NewArgumentException("providerName");
            }
            bool flag = false;
            ProviderInfo singleProvider = null;
            try
            {
                singleProvider = this.GetSingleProvider(providerName);
            }
            catch (ProviderNotFoundException)
            {
                return;
            }
            try
            {
                CmdletProvider providerInstance = this.GetProviderInstance(singleProvider);
                if (providerInstance == null)
                {
                    ProviderNotFoundException replaceParentContainsErrorRecordException = new ProviderNotFoundException(providerName, SessionStateCategory.CmdletProvider, "ProviderNotFound", SessionStateStrings.ProviderNotFound, new object[0]);
                    context.WriteError(new ErrorRecord(replaceParentContainsErrorRecordException.ErrorRecord, replaceParentContainsErrorRecordException));
                    flag = true;
                }
                else
                {
                    int num = 0;
                    foreach (PSDriveInfo info2 in this.GetDrivesForProvider(providerName))
                    {
                        if (info2 != null)
                        {
                            num++;
                            break;
                        }
                    }
                    if (num > 0)
                    {
                        if (force)
                        {
                            foreach (PSDriveInfo info3 in this.GetDrivesForProvider(providerName))
                            {
                                if (info3 != null)
                                {
                                    this.RemoveDrive(info3, true, null);
                                }
                            }
                        }
                        else
                        {
                            flag = true;
                            SessionStateException exception2 = new SessionStateException(providerName, SessionStateCategory.CmdletProvider, "RemoveDrivesBeforeRemovingProvider", SessionStateStrings.RemoveDrivesBeforeRemovingProvider, ErrorCategory.InvalidOperation, new object[0]);
                            context.WriteError(new ErrorRecord(exception2.ErrorRecord, exception2));
                            return;
                        }
                    }
                    try
                    {
                        providerInstance.Stop(context);
                    }
                    catch (LoopFlowException)
                    {
                        throw;
                    }
                    catch (PipelineStoppedException)
                    {
                        throw;
                    }
                    catch (ActionPreferenceStopException)
                    {
                        throw;
                    }
                }
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception3)
            {
                CommandProcessorBase.CheckForSevereException(exception3);
                flag = true;
                context.WriteError(new ErrorRecord(exception3, "RemoveProviderUnexpectedException", ErrorCategory.InvalidArgument, providerName));
            }
            finally
            {
                if (force || !flag)
                {
                    MshLog.LogProviderLifecycleEvent(this.ExecutionContext, providerName, ProviderState.Stopped);
                    this.RemoveProviderFromCollection(singleProvider);
                    this.ProvidersCurrentWorkingDrive.Remove(singleProvider);
                }
            }
        }

        private void RemoveProviderFromCollection(ProviderInfo provider)
        {
            if (this.Providers.ContainsKey(provider.Name))
            {
                List<ProviderInfo> list = this.Providers[provider.Name];
                if ((list.Count == 1) && list[0].NameEquals(provider.FullName))
                {
                    this.Providers.Remove(provider.Name);
                }
                else
                {
                    list.Remove(provider);
                }
            }
        }

        private string RemoveQualifier(string path, out string qualifier, out bool isProviderQualified, out bool isDriveQualified)
        {
            string str = path;
            qualifier = null;
            isProviderQualified = false;
            isDriveQualified = false;
            if (LocationGlobber.IsProviderQualifiedPath(path, out qualifier))
            {
                isProviderQualified = true;
                int index = path.IndexOf("::", StringComparison.Ordinal);
                if (index != -1)
                {
                    str = path.Substring(index + 2);
                }
            }
            else if (this.Globber.IsAbsolutePath(path, out qualifier))
            {
                isDriveQualified = true;
                str = path.Substring(qualifier.Length + 1);
            }
            tracer.WriteLine("result = {0}", new object[] { str });
            return str;
        }

        internal void RemoveScope(SessionStateScope scope)
        {
            if (scope == this._globalScope)
            {
                SessionStateUnauthorizedAccessException exception = new SessionStateUnauthorizedAccessException("GLOBAL", SessionStateCategory.Scope, "GlobalScopeCannotRemove", SessionStateStrings.GlobalScopeCannotRemove);
                throw exception;
            }
            foreach (PSDriveInfo info in scope.Drives)
            {
                if (info != null)
                {
                    CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
                    try
                    {
                        this.CanRemoveDrive(info, context);
                    }
                    catch (LoopFlowException)
                    {
                        throw;
                    }
                    catch (PipelineStoppedException)
                    {
                        throw;
                    }
                    catch (ActionPreferenceStopException)
                    {
                        throw;
                    }
                    catch (Exception exception2)
                    {
                        CommandProcessorBase.CheckForSevereException(exception2);
                    }
                }
            }
            scope.RemoveAllDrives();
            if ((scope == this.currentScope) && (this.currentScope.Parent != null))
            {
                this.currentScope = this.currentScope.Parent;
            }
            scope.Parent = null;
        }

        internal void RemoveVariable(PSVariable variable)
        {
            this.RemoveVariable(variable, false);
        }

        internal void RemoveVariable(string name)
        {
            this.RemoveVariable(name, false);
        }

        internal void RemoveVariable(PSVariable variable, bool force)
        {
            if (variable == null)
            {
                throw PSTraceSource.NewArgumentNullException("variable");
            }
            VariablePath variablePath = new VariablePath(variable.Name);
            SessionStateScope scope = null;
            if (this.GetVariableItem(variablePath, out scope) != null)
            {
                scope.RemoveVariable(variablePath.QualifiedName, force);
            }
        }

        internal void RemoveVariable(string name, bool force)
        {
            if (name == null)
            {
                throw PSTraceSource.NewArgumentNullException("name");
            }
            VariablePath variablePath = new VariablePath(name);
            SessionStateScope scope = null;
            if (variablePath.IsVariable)
            {
                if (this.GetVariableItem(variablePath, out scope) != null)
                {
                    scope.RemoveVariable(variablePath.QualifiedName, force);
                }
            }
            else
            {
                CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext) {
                    Force = force
                };
                this.RemoveItem(new string[] { variablePath.QualifiedName }, false, context);
                context.ThrowFirstErrorOrDoNothing();
            }
        }

        internal void RemoveVariableAtScope(PSVariable variable, string scopeID)
        {
            this.RemoveVariableAtScope(variable, scopeID, false);
        }

        internal void RemoveVariableAtScope(string name, string scopeID)
        {
            this.RemoveVariableAtScope(name, scopeID, false);
        }

        internal void RemoveVariableAtScope(PSVariable variable, string scopeID, bool force)
        {
            if (variable == null)
            {
                throw PSTraceSource.NewArgumentNullException("variable");
            }
            VariablePath path = new VariablePath(variable.Name);
            this.GetScopeByID(scopeID).RemoveVariable(path.QualifiedName, force);
        }

        internal void RemoveVariableAtScope(string name, string scopeID, bool force)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            VariablePath path = new VariablePath(name);
            SessionStateScope scopeByID = null;
            scopeByID = this.GetScopeByID(scopeID);
            if (path.IsVariable)
            {
                scopeByID.RemoveVariable(path.QualifiedName, force);
            }
            else
            {
                PSDriveInfo drive = scopeByID.GetDrive(path.DriveName);
                if (drive != null)
                {
                    CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext) {
                        Drive = drive,
                        Force = force
                    };
                    this.RemoveItem(new string[] { path.QualifiedName }, false, context);
                    context.ThrowFirstErrorOrDoNothing();
                }
            }
        }

        internal Collection<PSObject> RenameItem(string path, string newName, bool force)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext) {
                Force = force
            };
            this.RenameItem(path, newName, context);
            context.ThrowFirstErrorOrDoNothing();
            return context.GetAccumulatedObjects();
        }

        internal void RenameItem(string path, string newName, CmdletProviderContext context)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            ProviderInfo info = null;
            CmdletProvider providerInstance = null;
            Collection<string> targetObject = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, false, context, out info, out providerInstance);
            if (targetObject.Count == 1)
            {
                this.RenameItem(providerInstance, targetObject[0], newName, context);
            }
            else
            {
                ArgumentException exception = PSTraceSource.NewArgumentException("path", "SessionStateStrings", "RenameMultipleItemError", new object[0]);
                context.WriteError(new ErrorRecord(exception, "RenameMultipleItemError", ErrorCategory.InvalidArgument, targetObject));
            }
        }

        private void RenameItem(CmdletProvider providerInstance, string path, string newName, CmdletProviderContext context)
        {
            ContainerCmdletProvider containerProviderInstance = GetContainerProviderInstance(providerInstance);
            try
            {
                containerProviderInstance.RenameItem(path, newName, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("RenameItemProviderException", SessionStateStrings.RenameItemProviderException, containerProviderInstance.ProviderInfo, path, exception);
            }
        }

        internal object RenameItemDynamicParameters(string path, string newName, CmdletProviderContext context)
        {
            if (path != null)
            {
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                CmdletProviderContext context2 = new CmdletProviderContext(context);
                context2.SetFilters(new Collection<string>(), new Collection<string>(), null);
                Collection<string> collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context2, out info, out providerInstance);
                if (collection.Count > 0)
                {
                    return this.RenameItemDynamicParameters(providerInstance, collection[0], newName, context2);
                }
            }
            return null;
        }

        private object RenameItemDynamicParameters(CmdletProvider providerInstance, string path, string newName, CmdletProviderContext context)
        {
            ContainerCmdletProvider containerProviderInstance = GetContainerProviderInstance(providerInstance);
            object obj2 = null;
            try
            {
                obj2 = containerProviderInstance.RenameItemDynamicParameters(path, newName, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("RenameItemDynamicParametersProviderException", SessionStateStrings.RenameItemDynamicParametersProviderException, containerProviderInstance.ProviderInfo, path, exception);
            }
            return obj2;
        }

        internal void RenameProperty(string[] paths, string sourceProperty, string destinationProperty, CmdletProviderContext context)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            if (sourceProperty == null)
            {
                throw PSTraceSource.NewArgumentNullException("sourceProperty");
            }
            if (destinationProperty == null)
            {
                throw PSTraceSource.NewArgumentNullException("destinationProperty");
            }
            foreach (string str in paths)
            {
                if (str == null)
                {
                    throw PSTraceSource.NewArgumentNullException("paths");
                }
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                foreach (string str2 in this.Globber.GetGlobbedProviderPathsFromMonadPath(str, false, context, out info, out providerInstance))
                {
                    this.RenameProperty(providerInstance, str2, sourceProperty, destinationProperty, context);
                }
            }
        }

        private void RenameProperty(CmdletProvider providerInstance, string sourcePath, string sourceProperty, string destinationProperty, CmdletProviderContext context)
        {
            try
            {
                providerInstance.RenameProperty(sourcePath, sourceProperty, destinationProperty, context);
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("RenamePropertyProviderException", SessionStateStrings.RenamePropertyProviderException, providerInstance.ProviderInfo, sourcePath, exception);
            }
        }

        internal Collection<PSObject> RenameProperty(string[] sourcePaths, string sourceProperty, string destinationProperty, bool force, bool literalPath)
        {
            if (sourcePaths == null)
            {
                throw PSTraceSource.NewArgumentNullException("sourcePaths");
            }
            if (sourceProperty == null)
            {
                throw PSTraceSource.NewArgumentNullException("sourceProperty");
            }
            if (destinationProperty == null)
            {
                throw PSTraceSource.NewArgumentNullException("destinationProperty");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext) {
                Force = force,
                SuppressWildcardExpansion = literalPath
            };
            this.RenameProperty(sourcePaths, sourceProperty, destinationProperty, context);
            context.ThrowFirstErrorOrDoNothing();
            return context.GetAccumulatedObjects();
        }

        internal object RenamePropertyDynamicParameters(string path, string sourceProperty, string destinationProperty, CmdletProviderContext context)
        {
            if (path != null)
            {
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                CmdletProviderContext context2 = new CmdletProviderContext(context);
                context2.SetFilters(new Collection<string>(), new Collection<string>(), null);
                Collection<string> collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context2, out info, out providerInstance);
                if (collection.Count > 0)
                {
                    return this.RenamePropertyDynamicParameters(providerInstance, collection[0], sourceProperty, destinationProperty, context2);
                }
            }
            return null;
        }

        private object RenamePropertyDynamicParameters(CmdletProvider providerInstance, string path, string sourceProperty, string destinationProperty, CmdletProviderContext context)
        {
            object obj2 = null;
            try
            {
                obj2 = providerInstance.RenamePropertyDynamicParameters(path, sourceProperty, destinationProperty, context);
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("RenamePropertyDynamicParametersProviderException", SessionStateStrings.RenamePropertyDynamicParametersProviderException, providerInstance.ProviderInfo, path, exception);
            }
            return obj2;
        }

        internal void RunspaceClosingNotification()
        {
            if ((this != this._context.TopLevelSessionState) && (this.Providers.Count > 0))
            {
                CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
                Collection<string> collection = new Collection<string>();
                foreach (string str in this.Providers.Keys)
                {
                    collection.Add(str);
                }
                foreach (string str2 in collection)
                {
                    this.RemoveProvider(str2, true, context);
                }
            }
        }

        internal AliasInfo SetAliasItem(AliasInfo alias, bool force, CommandOrigin origin)
        {
            if (alias == null)
            {
                throw PSTraceSource.NewArgumentNullException("alias");
            }
            return this.currentScope.SetAliasItem(alias, force, origin);
        }

        internal AliasInfo SetAliasItemAtScope(AliasInfo alias, string scopeID, bool force)
        {
            return this.SetAliasItemAtScope(alias, scopeID, force, CommandOrigin.Internal);
        }

        internal AliasInfo SetAliasItemAtScope(AliasInfo alias, string scopeID, bool force, CommandOrigin origin)
        {
            if (alias == null)
            {
                throw PSTraceSource.NewArgumentNullException("alias");
            }
            if (string.Equals(scopeID, "PRIVATE", StringComparison.OrdinalIgnoreCase))
            {
                alias.Options |= ScopedItemOptions.Private;
            }
            return this.GetScopeByID(scopeID).SetAliasItem(alias, force, origin);
        }

        internal AliasInfo SetAliasValue(string aliasName, string value, bool force)
        {
            return this.SetAliasValue(aliasName, value, force, CommandOrigin.Internal);
        }

        internal AliasInfo SetAliasValue(string aliasName, string value, bool force, CommandOrigin origin)
        {
            if (string.IsNullOrEmpty(aliasName))
            {
                throw PSTraceSource.NewArgumentException("aliasName");
            }
            if (string.IsNullOrEmpty(value))
            {
                throw PSTraceSource.NewArgumentException("value");
            }
            return this.currentScope.SetAliasValue(aliasName, value, this.ExecutionContext, force, origin);
        }

        internal AliasInfo SetAliasValue(string aliasName, string value, ScopedItemOptions options, bool force)
        {
            return this.SetAliasValue(aliasName, value, options, force, CommandOrigin.Internal);
        }

        internal AliasInfo SetAliasValue(string aliasName, string value, ScopedItemOptions options, bool force, CommandOrigin origin)
        {
            if (string.IsNullOrEmpty(aliasName))
            {
                throw PSTraceSource.NewArgumentException("aliasName");
            }
            if (string.IsNullOrEmpty(value))
            {
                throw PSTraceSource.NewArgumentException("value");
            }
            return this.currentScope.SetAliasValue(aliasName, value, options, this.ExecutionContext, force, origin);
        }

        internal void SetConsoleVariable()
        {
            string filename = string.Empty;
            RunspaceConfigForSingleShell runspaceConfiguration = this._context.RunspaceConfiguration as RunspaceConfigForSingleShell;
            if (((runspaceConfiguration != null) && (runspaceConfiguration.ConsoleInfo != null)) && !string.IsNullOrEmpty(runspaceConfiguration.ConsoleInfo.Filename))
            {
                filename = runspaceConfiguration.ConsoleInfo.Filename;
            }
            PSVariable variable = new PSVariable("ConsoleFileName", filename, ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly, RunspaceInit.ConsoleDescription);
            this.GlobalScope.SetVariable(variable.Name, variable, false, true, this, CommandOrigin.Internal, true);
        }

        internal PathInfoStack SetDefaultLocationStack(string stackName)
        {
            if (string.IsNullOrEmpty(stackName))
            {
                stackName = "default";
            }
            if (!this.workingLocationStack.ContainsKey(stackName))
            {
                if (string.Equals(stackName, "default", StringComparison.OrdinalIgnoreCase))
                {
                    return new PathInfoStack("default", new Stack<PathInfo>());
                }
                ItemNotFoundException exception = new ItemNotFoundException(stackName, "StackNotFound", SessionStateStrings.PathNotFound);
                throw exception;
            }
            this.defaultStackName = stackName;
            Stack<PathInfo> locationStack = this.workingLocationStack[this.defaultStackName];
            if (locationStack != null)
            {
                return new PathInfoStack(this.defaultStackName, locationStack);
            }
            return null;
        }

        internal FunctionInfo SetFunction(string name, ScriptBlock function, bool force)
        {
            return this.SetFunction(name, function, null, force, CommandOrigin.Internal);
        }

        internal FunctionInfo SetFunction(string name, ScriptBlock function, FunctionInfo originalFunction, bool force, CommandOrigin origin)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            if (function == null)
            {
                throw PSTraceSource.NewArgumentNullException("function");
            }
            string itemName = name;
            FunctionLookupPath lookupPath = new FunctionLookupPath(name);
            name = lookupPath.UnqualifiedPath;
            if (string.IsNullOrEmpty(name))
            {
                SessionStateException exception = new SessionStateException(itemName, SessionStateCategory.Function, "ScopedFunctionMustHaveName", SessionStateStrings.ScopedFunctionMustHaveName, ErrorCategory.InvalidArgument, new object[0]);
                throw exception;
            }
            ScopedItemOptions none = ScopedItemOptions.None;
            if (lookupPath.IsPrivate)
            {
                none |= ScopedItemOptions.Private;
            }
            FunctionScopeItemSearcher searcher = new FunctionScopeItemSearcher(this, lookupPath, origin);
            SessionStateScope initialScope = searcher.InitialScope;
            if (searcher.MoveNext())
            {
                initialScope = searcher.CurrentLookupScope;
                name = searcher.Name;
                if (!lookupPath.IsPrivate)
                {
                    return initialScope.SetFunction(name, function, force, origin, this.ExecutionContext);
                }
                FunctionInfo info2 = initialScope.GetFunction(name);
                FunctionInfo info3 = info2;
                if (info3 != null)
                {
                    none |= info3.Options;
                }
                else
                {
                    none |= ((FilterInfo) info2).Options;
                }
                return initialScope.SetFunction(name, function, originalFunction, none, force, origin, this.ExecutionContext);
            }
            if (lookupPath.IsPrivate)
            {
                return initialScope.SetFunction(name, function, originalFunction, none, force, origin, this.ExecutionContext);
            }
            return initialScope.SetFunction(name, function, force, origin, this.ExecutionContext);
        }

        internal FunctionInfo SetFunction(string name, ScriptBlock function, FunctionInfo originalFunction, ScopedItemOptions options, bool force, CommandOrigin origin)
        {
            return this.SetFunction(name, function, originalFunction, options, force, origin, this.ExecutionContext, null);
        }

        internal FunctionInfo SetFunction(string name, ScriptBlock function, FunctionInfo originalFunction, ScopedItemOptions options, bool force, CommandOrigin origin, string helpFile)
        {
            return this.SetFunction(name, function, originalFunction, options, force, origin, this.ExecutionContext, helpFile, false);
        }

        internal FunctionInfo SetFunction(string name, ScriptBlock function, FunctionInfo originalFunction, ScopedItemOptions options, bool force, CommandOrigin origin, System.Management.Automation.ExecutionContext context, string helpFile)
        {
            return this.SetFunction(name, function, originalFunction, options, force, origin, context, helpFile, false);
        }

        internal FunctionInfo SetFunction(string name, ScriptBlock function, FunctionInfo originalFunction, ScopedItemOptions options, bool force, CommandOrigin origin, System.Management.Automation.ExecutionContext context, string helpFile, bool isPreValidated)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            if (function == null)
            {
                throw PSTraceSource.NewArgumentNullException("function");
            }
            string itemName = name;
            FunctionLookupPath lookupPath = new FunctionLookupPath(name);
            name = lookupPath.UnqualifiedPath;
            if (string.IsNullOrEmpty(name))
            {
                SessionStateException exception = new SessionStateException(itemName, SessionStateCategory.Function, "ScopedFunctionMustHaveName", SessionStateStrings.ScopedFunctionMustHaveName, ErrorCategory.InvalidArgument, new object[0]);
                throw exception;
            }
            if (lookupPath.IsPrivate)
            {
                options |= ScopedItemOptions.Private;
            }
            FunctionScopeItemSearcher searcher = new FunctionScopeItemSearcher(this, lookupPath, origin);
            return searcher.InitialScope.SetFunction(name, function, originalFunction, options, force, origin, context, helpFile);
        }

        internal FunctionInfo SetFunctionRaw(string name, ScriptBlock function, CommandOrigin origin)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            if (function == null)
            {
                throw PSTraceSource.NewArgumentNullException("function");
            }
            string itemName = name;
            FunctionLookupPath lookupPath = new FunctionLookupPath(name);
            name = lookupPath.UnqualifiedPath;
            if (string.IsNullOrEmpty(name))
            {
                SessionStateException exception = new SessionStateException(itemName, SessionStateCategory.Function, "ScopedFunctionMustHaveName", SessionStateStrings.ScopedFunctionMustHaveName, ErrorCategory.InvalidArgument, new object[0]);
                throw exception;
            }
            ScopedItemOptions none = ScopedItemOptions.None;
            if (lookupPath.IsPrivate)
            {
                none |= ScopedItemOptions.Private;
            }
            FunctionScopeItemSearcher searcher = new FunctionScopeItemSearcher(this, lookupPath, origin);
            return searcher.InitialScope.SetFunction(name, function, null, none, false, origin, this.ExecutionContext);
        }

        internal void SetItem(string[] paths, object value, CmdletProviderContext context)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            foreach (string str in paths)
            {
                if (str == null)
                {
                    throw PSTraceSource.NewArgumentNullException("paths");
                }
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                Collection<string> collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(str, true, context, out info, out providerInstance);
                if (collection != null)
                {
                    foreach (string str2 in collection)
                    {
                        this.SetItem(providerInstance, str2, value, context);
                    }
                }
            }
        }

        internal Collection<PSObject> SetItem(string[] paths, object value, bool force, bool literalPath)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext) {
                Force = force,
                SuppressWildcardExpansion = literalPath
            };
            this.SetItem(paths, value, context);
            context.ThrowFirstErrorOrDoNothing();
            return context.GetAccumulatedObjects();
        }

        private void SetItem(CmdletProvider providerInstance, string path, object value, CmdletProviderContext context)
        {
            ItemCmdletProvider itemProviderInstance = GetItemProviderInstance(providerInstance);
            try
            {
                itemProviderInstance.SetItem(path, value, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("SetItemProviderException", SessionStateStrings.SetItemProviderException, itemProviderInstance.ProviderInfo, path, exception);
            }
        }

        internal object SetItemDynamicParameters(string path, object value, CmdletProviderContext context)
        {
            if (path != null)
            {
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                CmdletProviderContext context2 = new CmdletProviderContext(context);
                context2.SetFilters(new Collection<string>(), new Collection<string>(), null);
                Collection<string> collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context2, out info, out providerInstance);
                if (collection.Count > 0)
                {
                    return this.SetItemDynamicParameters(providerInstance, collection[0], value, context2);
                }
            }
            return null;
        }

        private object SetItemDynamicParameters(CmdletProvider providerInstance, string path, object value, CmdletProviderContext context)
        {
            ItemCmdletProvider itemProviderInstance = GetItemProviderInstance(providerInstance);
            object obj2 = null;
            try
            {
                obj2 = itemProviderInstance.SetItemDynamicParameters(path, value, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("SetItemDynamicParametersProviderException", SessionStateStrings.SetItemDynamicParametersProviderException, itemProviderInstance.ProviderInfo, path, exception);
            }
            return obj2;
        }

        internal PathInfo SetLocation(string path)
        {
            return this.SetLocation(path, null);
        }

        internal PathInfo SetLocation (string path, CmdletProviderContext context)
		{
			if (string.IsNullOrEmpty (path) && OSHelper.IsUnix)
				path = "/";
			else if (path == null) {
				throw PSTraceSource.NewArgumentNullException ("path");
			}
			string str = path;
			string driveName = null;
			ProviderInfo provider = null;
			string providerId = null;
			PSDriveInfo currentDrive = this.CurrentDrive;
			if (LocationGlobber.IsHomePath (path)) {
				path = this.Globber.GetHomeRelativePath (path);
			}

			if (LocationGlobber.IsProviderDirectPath (path)) {
				provider = this.CurrentLocation.Provider;
				PSDriveInfo drive = null;
				if (PathIntrinsics.FinDriveFromPath (path, provider, out drive))
				{
					this.CurrentDrive = drive;
				}
				else if (LocationGlobber.IsProviderQualifiedPath (path, out providerId)) {
					provider = this.GetSingleProvider (providerId);
					if (PathIntrinsics.FinDriveFromPath (path, provider, out drive))
					{
						this.CurrentDrive = drive;
					}
					else if (this.Globber.IsAbsolutePath(path, out driveName))
					{
						drive = this.GetDrive(driveName);
						this.CurrentDrive = drive;
					}
				}
			} else if (LocationGlobber.IsProviderQualifiedPath (path, out providerId)) {
				provider = this.GetSingleProvider (providerId);
				PSDriveInfo drive = null;
				if (PathIntrinsics.FinDriveFromPath (path, provider, out drive))
				{
					this.CurrentDrive = drive;
				}
				else if (this.Globber.IsAbsolutePath(path, out driveName))
				{
					drive = this.GetDrive(driveName);
					this.CurrentDrive = drive;
				}
			} 
			else if (this.Globber.IsAbsolutePath(path, out driveName))
			{
				PSDriveInfo drive = this.GetDrive(driveName);
				this.CurrentDrive = drive;
			}
            if (context == null)
            {
                context = new CmdletProviderContext(this.ExecutionContext);
            }
            if (this.CurrentDrive != null)
            {
                context.Drive = this.CurrentDrive;
            }
            CmdletProvider providerInstance = null;
            Collection<PathInfo> collection = null;
            try
            {
                collection = this.Globber.GetGlobbedMonadPathsFromMonadPath(path, false, context, out providerInstance);

			}
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                this.CurrentDrive = currentDrive;
                throw;
            }
            if (collection.Count == 0)
            {
                this.CurrentDrive = currentDrive;
                throw new ItemNotFoundException(path, "PathNotFound", SessionStateStrings.PathNotFound);
            }
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;
            for (int i = 0; i < collection.Count; i++)
            {
                CmdletProviderContext context2 = new CmdletProviderContext(context);
                PathInfo info4 = collection[i];
                string str4 = path;
                try
                {
                    string str5 = null;
                    flag4 = LocationGlobber.IsProviderQualifiedPath(info4.Path, out str5);
                    if (flag4)
                    {
                        string str6 = LocationGlobber.RemoveProviderQualifier(info4.Path);
                        try
                        {
                            str4 = this.NormalizeRelativePath(this.GetSingleProvider(str5), str6, string.Empty, context2);
                            goto Label_01DF;
                        }
                        catch (NotSupportedException)
                        {
                            goto Label_01DF;
                        }
                        catch (LoopFlowException)
                        {
                            throw;
                        }
                        catch (PipelineStoppedException)
                        {
                            throw;
                        }
                        catch (ActionPreferenceStopException)
                        {
                            throw;
                        }
                        catch (Exception exception2)
                        {
                            CommandProcessorBase.CheckForSevereException(exception2);
                            this.CurrentDrive = currentDrive;
                            throw;
                        }
                    }
                    try
                    {
                    	str4 = this.NormalizeRelativePath(info4.Path, this.CurrentDrive.Root, context2);
                    }
                    catch (NotSupportedException)
                    {
                    }
                    catch (LoopFlowException)
                    {
                        throw;
                    }
                    catch (PipelineStoppedException)
                    {
                        throw;
                    }
                    catch (ActionPreferenceStopException)
                    {
                        throw;
                    }
                    catch (Exception exception3)
                    {
                        CommandProcessorBase.CheckForSevereException(exception3);
                        this.CurrentDrive = currentDrive;
                        throw;
                    }
                Label_01DF:
                    if (context2.HasErrors())
                    {
                        this.CurrentDrive = currentDrive;
                        context2.ThrowFirstErrorOrDoNothing();
                    }
                }
                finally
                {
                    context2.RemoveStopReferral();
                }
                bool flag5 = false;
                CmdletProviderContext context3 = new CmdletProviderContext(context) {
                    SuppressWildcardExpansion = true
                };
                try
                {
                    flag5 = this.IsItemContainer(info4.Path, context3);
                    if (context3.HasErrors())
                    {
                        this.CurrentDrive = currentDrive;
                        context3.ThrowFirstErrorOrDoNothing();
                    }
                }
                catch (NotSupportedException)
                {
                    if (str4.Length == 0)
                    {
                        flag5 = true;
                    }
                }
                finally
                {
                    context3.RemoveStopReferral();
                }
                if (flag5)
                {
                    if (flag)
                    {
                        this.CurrentDrive = currentDrive;
                        throw PSTraceSource.NewArgumentException("path", "SessionStateStrings", "PathResolvedToMultiple", new object[] { str });
                    }
                    path = str4;
                    flag2 = true;
                    flag3 = flag4;
                    flag = true;
                }
            }
            if (flag2)
            {
				if (!OSHelper.IsUnix)
				{
	                if (!LocationGlobber.IsProviderDirectPath(path))
	                {
	                    char ch = '\\';
	                    if (path.StartsWith(ch.ToString(), StringComparison.CurrentCulture) && !flag3)
	                    {
	                        path = path.Substring(1);
	                    }
	                }
				}
				tracer.WriteLine("New working path = {0}", new object[] { path });
				this.CurrentDrive.CurrentLocation = path;
            }
            else
            {
                this.CurrentDrive = currentDrive;
                throw new ItemNotFoundException(str, "PathNotFound", SessionStateStrings.PathNotFound);
            }
            this.ProvidersCurrentWorkingDrive[this.CurrentDrive.Provider] = this.CurrentDrive;
            this.SetVariable(SpecialVariables.PWDVarPath, this.CurrentLocation, false, true, CommandOrigin.Internal);
            return this.CurrentLocation;
        }

        internal void SetProperty(string[] paths, PSObject property, CmdletProviderContext context)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            if (property == null)
            {
                throw PSTraceSource.NewArgumentNullException("property");
            }
            foreach (string str in paths)
            {
                if (str == null)
                {
                    throw PSTraceSource.NewArgumentNullException("paths");
                }
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                Collection<string> collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(str, false, context, out info, out providerInstance);
                if (collection != null)
                {
                    foreach (string str2 in collection)
                    {
                        this.SetPropertyPrivate(providerInstance, str2, property, context);
                    }
                }
            }
        }

        internal Collection<PSObject> SetProperty(string[] paths, PSObject property, bool force, bool literalPath)
        {
            if (paths == null)
            {
                throw PSTraceSource.NewArgumentNullException("paths");
            }
            if (property == null)
            {
                throw PSTraceSource.NewArgumentNullException("properties");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext) {
                Force = force,
                SuppressWildcardExpansion = literalPath
            };
            this.SetProperty(paths, property, context);
            context.ThrowFirstErrorOrDoNothing();
            return context.GetAccumulatedObjects();
        }

        internal object SetPropertyDynamicParameters(string path, PSObject propertyValue, CmdletProviderContext context)
        {
            if (path != null)
            {
                ProviderInfo info = null;
                CmdletProvider providerInstance = null;
                CmdletProviderContext context2 = new CmdletProviderContext(context);
                context2.SetFilters(new Collection<string>(), new Collection<string>(), null);
                Collection<string> collection = this.Globber.GetGlobbedProviderPathsFromMonadPath(path, true, context2, out info, out providerInstance);
                if (collection.Count > 0)
                {
                    return this.SetPropertyDynamicParameters(providerInstance, collection[0], propertyValue, context2);
                }
            }
            return null;
        }

        private object SetPropertyDynamicParameters(CmdletProvider providerInstance, string path, PSObject propertyValue, CmdletProviderContext context)
        {
            object obj2 = null;
            try
            {
                obj2 = providerInstance.SetPropertyDynamicParameters(path, propertyValue, context);
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("SetPropertyDynamicParametersProviderException", SessionStateStrings.SetPropertyDynamicParametersProviderException, providerInstance.ProviderInfo, path, exception);
            }
            return obj2;
        }

        private void SetPropertyPrivate(CmdletProvider providerInstance, string path, PSObject property, CmdletProviderContext context)
        {
            try
            {
                providerInstance.SetProperty(path, property, context);
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw this.NewProviderInvocationException("SetPropertyProviderException", SessionStateStrings.SetPropertyProviderException, providerInstance.ProviderInfo, path, exception);
            }
        }

        internal Collection<PSObject> SetSecurityDescriptor(string path, ObjectSecurity securityDescriptor)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            if (securityDescriptor == null)
            {
                throw PSTraceSource.NewArgumentNullException("securityDescriptor");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
            this.SetSecurityDescriptor(path, securityDescriptor, context);
            context.ThrowFirstErrorOrDoNothing();
            Collection<PSObject> accumulatedObjects = context.GetAccumulatedObjects();
            if (accumulatedObjects == null)
            {
                accumulatedObjects = new Collection<PSObject>();
            }
            return accumulatedObjects;
        }

        internal void SetSecurityDescriptor(string path, ObjectSecurity securityDescriptor, CmdletProviderContext context)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            if (securityDescriptor == null)
            {
                throw PSTraceSource.NewArgumentNullException("securityDescriptor");
            }
            ProviderInfo info = null;
            CmdletProvider providerInstance = null;
            foreach (string str in this.Globber.GetGlobbedProviderPathsFromMonadPath(path, false, context, out info, out providerInstance))
            {
                this.SetSecurityDescriptor(providerInstance, str, securityDescriptor, context);
            }
        }

        private void SetSecurityDescriptor(CmdletProvider providerInstance, string path, ObjectSecurity securityDescriptor, CmdletProviderContext context)
        {
            GetPermissionProviderInstance(providerInstance);
            try
            {
                providerInstance.SetSecurityDescriptor(path, securityDescriptor, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (PrivilegeNotHeldException exception)
            {
                context.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.PermissionDenied, path));
            }
            catch (UnauthorizedAccessException exception2)
            {
                context.WriteError(new ErrorRecord(exception2, exception2.GetType().FullName, ErrorCategory.PermissionDenied, path));
            }
            catch (NotSupportedException exception3)
            {
                context.WriteError(new ErrorRecord(exception3, exception3.GetType().FullName, ErrorCategory.InvalidOperation, path));
            }
            catch (SystemException exception4)
            {
                CommandProcessorBase.CheckForSevereException(exception4);
                context.WriteError(new ErrorRecord(exception4, exception4.GetType().FullName, ErrorCategory.InvalidOperation, path));
            }
            catch (Exception exception5)
            {
                CommandProcessorBase.CheckForSevereException(exception5);
                throw this.NewProviderInvocationException("SetSecurityDescriptorProviderException", SessionStateStrings.SetSecurityDescriptorProviderException, providerInstance.ProviderInfo, path, exception5);
            }
        }

        internal object SetVariable(PSVariable variable, bool force, CommandOrigin origin)
        {
            if ((variable == null) || string.IsNullOrEmpty(variable.Name))
            {
                throw PSTraceSource.NewArgumentException("variable");
            }
            VariablePath variablePath = new VariablePath(variable.Name, VariablePathFlags.Unqualified | VariablePathFlags.Variable);
            return this.SetVariable(variablePath, variable, false, force, origin);
        }

        internal object SetVariable(VariablePath variablePath, object newValue, bool asValue, CommandOrigin origin)
        {
            return this.SetVariable(variablePath, newValue, asValue, false, origin);
        }

        internal object SetVariable(VariablePath variablePath, object newValue, bool asValue, bool force, CommandOrigin origin)
        {
            if (variablePath == null)
            {
                throw PSTraceSource.NewArgumentNullException("variablePath");
            }
            CmdletProviderContext context = null;
            SessionStateScope currentScope = null;
            if (variablePath.IsVariable)
            {
                if (variablePath.IsLocal || variablePath.IsUnscopedVariable)
                {
                    currentScope = this.currentScope;
                }
                else if (variablePath.IsScript)
                {
                    currentScope = this.currentScope.ScriptScope;
                }
                else if (variablePath.IsGlobal)
                {
                    currentScope = this._globalScope;
                }
                else if (variablePath.IsPrivate)
                {
                    currentScope = this.currentScope;
                }
                PSVariable variable = currentScope.SetVariable(variablePath.QualifiedName, newValue, asValue, force, this, origin, false);
                if (variablePath.IsPrivate && (variable != null))
                {
                    variable.Options |= ScopedItemOptions.Private;
                }
                return variable;
            }
            this.GetVariableValue(variablePath, out context, out currentScope);
            Collection<IContentWriter> contentWriter = null;
            try
            {
                if (context != null)
                {
                    try
                    {
                        CmdletProviderContext context2 = new CmdletProviderContext(context);
                        this.ClearContent(new string[] { variablePath.QualifiedName }, context2);
                    }
                    catch (NotSupportedException)
                    {
                    }
                    catch (ItemNotFoundException)
                    {
                    }
                    contentWriter = this.GetContentWriter(new string[] { variablePath.QualifiedName }, context);
                    context.ThrowFirstErrorOrDoNothing(true);
                }
                else
                {
                    try
                    {
                        this.ClearContent(new string[] { variablePath.QualifiedName }, false, false);
                    }
                    catch (NotSupportedException)
                    {
                    }
                    catch (ItemNotFoundException)
                    {
                    }
                    contentWriter = this.GetContentWriter(new string[] { variablePath.QualifiedName }, false, false);
                }
            }
            catch (NotImplementedException exception)
            {
                ProviderInfo provider = null;
                this.Globber.GetProviderPath(variablePath.QualifiedName, out provider);
                throw this.NewProviderInvocationException("ProviderCannotBeUsedAsVariable", SessionStateStrings.ProviderCannotBeUsedAsVariable, provider, variablePath.QualifiedName, exception, false);
            }
            catch (NotSupportedException exception2)
            {
                ProviderInfo info2 = null;
                this.Globber.GetProviderPath(variablePath.QualifiedName, out info2);
                throw this.NewProviderInvocationException("ProviderCannotBeUsedAsVariable", SessionStateStrings.ProviderCannotBeUsedAsVariable, info2, variablePath.QualifiedName, exception2, false);
            }
            if ((contentWriter == null) || (contentWriter.Count == 0))
            {
                ItemNotFoundException exception3 = new ItemNotFoundException(variablePath.QualifiedName, "PathNotFound", SessionStateStrings.PathNotFound);
                throw exception3;
            }
            if (contentWriter.Count > 1)
            {
                foreach (IContentWriter writer in contentWriter)
                {
                    writer.Close();
                }
                PSArgumentException e = PSTraceSource.NewArgumentException("path", "SessionStateStrings", "VariablePathResolvedToMultiple", new object[] { variablePath.QualifiedName });
                ProviderInfo info3 = null;
                this.Globber.GetProviderPath(variablePath.QualifiedName, out info3);
                throw this.NewProviderInvocationException("ProviderVariableSyntaxInvalid", SessionStateStrings.ProviderVariableSyntaxInvalid, info3, variablePath.QualifiedName, e);
            }
            IContentWriter writer2 = contentWriter[0];
            IList content = newValue as IList;
            if (content == null)
            {
                content = new object[] { newValue };
            }
            try
            {
                writer2.Write(content);
            }
            catch (Exception exception5)
            {
                ProviderInfo info4 = null;
                this.Globber.GetProviderPath(variablePath.QualifiedName, out info4);
                CommandProcessorBase.CheckForSevereException(exception5);
                ProviderInvocationException exception6 = new ProviderInvocationException("ProviderContentWriteError", SessionStateStrings.ProviderContentWriteError, info4, variablePath.QualifiedName, exception5);
                throw exception6;
            }
            finally
            {
                writer2.Close();
            }
            return null;
        }

        internal object SetVariableAtScope(PSVariable variable, string scopeID, bool force, CommandOrigin origin)
        {
            if ((variable == null) || string.IsNullOrEmpty(variable.Name))
            {
                throw PSTraceSource.NewArgumentException("variable");
            }
            return this.GetScopeByID(scopeID).SetVariable(variable.Name, variable, false, force, this, origin, false);
        }

        internal void SetVariableValue(string name, object newValue)
        {
            this.SetVariableValue(name, newValue, CommandOrigin.Internal);
        }

        internal void SetVariableValue(string name, object newValue, CommandOrigin origin)
        {
            if (name == null)
            {
                throw PSTraceSource.NewArgumentNullException("name");
            }
            VariablePath variablePath = new VariablePath(name);
            this.SetVariable(variablePath, newValue, true, origin);
        }

        internal WorkflowInfo SetWorkflowRaw(WorkflowInfo workflowInfo, CommandOrigin origin)
        {
            string name = workflowInfo.Name;
            string unqualifiedPath = name;
            FunctionLookupPath lookupPath = new FunctionLookupPath(unqualifiedPath);
            unqualifiedPath = lookupPath.UnqualifiedPath;
            if (string.IsNullOrEmpty(unqualifiedPath))
            {
                SessionStateException exception = new SessionStateException(name, SessionStateCategory.Function, "ScopedFunctionMustHaveName", SessionStateStrings.ScopedFunctionMustHaveName, ErrorCategory.InvalidArgument, new object[0]);
                throw exception;
            }
            ScopedItemOptions none = ScopedItemOptions.None;
            if (lookupPath.IsPrivate)
            {
                none |= ScopedItemOptions.Private;
            }
            FunctionScopeItemSearcher searcher = new FunctionScopeItemSearcher(this, lookupPath, origin);
            workflowInfo.ScriptBlock.LanguageMode = 0;
            return (WorkflowInfo) searcher.InitialScope.SetFunction(unqualifiedPath, workflowInfo.ScriptBlock, null, none, false, origin, this.ExecutionContext, null, (arg1, arg2, arg3, arg4, arg5, arg6) => workflowInfo);
        }

        internal void UpdateProviders()
        {
            if (this.ExecutionContext.RunspaceConfiguration == null)
            {
                throw PSTraceSource.NewInvalidOperationException();
            }
            if ((this == this._context.TopLevelSessionState) && !this._providersInitialized)
            {
                foreach (ProviderConfigurationEntry entry in this.ExecutionContext.RunspaceConfiguration.Providers)
                {
                    this.AddProvider(entry);
                }
                this._providersInitialized = true;
            }
            else
            {
                foreach (ProviderConfigurationEntry entry2 in this.ExecutionContext.RunspaceConfiguration.Providers.UpdateList)
                {
                    switch (entry2.Action)
                    {
                        case UpdateAction.Add:
                            this.AddProvider(entry2);
                            break;

                        case UpdateAction.Remove:
                            this.RemoveProvider(entry2);
                            break;
                    }
                }
            }
        }

        private PSDriveInfo ValidateDriveWithProvider(PSDriveInfo drive, CmdletProviderContext context, bool resolvePathIfPossible)
        {
            DriveCmdletProvider driveProviderInstance = this.GetDriveProviderInstance(drive.Provider);
            return this.ValidateDriveWithProvider(driveProviderInstance, drive, context, resolvePathIfPossible);
        }

        private PSDriveInfo ValidateDriveWithProvider(DriveCmdletProvider driveProvider, PSDriveInfo drive, CmdletProviderContext context, bool resolvePathIfPossible)
        {
            drive.DriveBeingCreated = true;
            if ((this.CurrentDrive != null) && resolvePathIfPossible)
            {
                string providerRootFromSpecifiedRoot = this.GetProviderRootFromSpecifiedRoot(drive.Root, drive.Provider);
                if (providerRootFromSpecifiedRoot != null)
                {
                    drive.SetRoot(providerRootFromSpecifiedRoot);
                }
            }
            PSDriveInfo info = null;
            try
            {
                info = driveProvider.NewDrive(drive, context);
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                ProviderInvocationException replaceParentContainsErrorRecordException = this.NewProviderInvocationException("NewDriveProviderException", SessionStateStrings.NewDriveProviderException, driveProvider.ProviderInfo, drive.Root, exception);
                context.WriteError(new ErrorRecord(replaceParentContainsErrorRecordException.ErrorRecord, replaceParentContainsErrorRecordException));
            }
            finally
            {
                drive.DriveBeingCreated = false;
            }
            return info;
        }

        private bool ValidateOrRemoveAutoMountedDrive(PSDriveInfo drive, SessionStateScope scope)
        {
            bool flag = true;
            try
            {
                DriveInfo info = new DriveInfo(drive.Name);
                flag = info.DriveType != DriveType.NoRootDirectory;
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (ActionPreferenceStopException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                flag = false;
            }
            if (!flag)
            {
                DriveCmdletProvider driveProviderInstance = null;
                try
                {
                    driveProviderInstance = this.GetDriveProviderInstance(this.ExecutionContext.ProviderNames.FileSystem);
                }
                catch (NotSupportedException)
                {
                }
                catch (ProviderNotFoundException)
                {
                }
                if (driveProviderInstance == null)
                {
                    return flag;
                }
                CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
                try
                {
                    driveProviderInstance.RemoveDrive(drive, context);
                }
                catch (Exception exception2)
                {
                    CommandProcessorBase.CheckForSevereException(exception2);
                }
                scope.RemoveDrive(drive);
            }
            return flag;
        }

        public List<string> Applications
        {
            get
            {
                return this._applications;
            }
        }

        internal PSDriveInfo CurrentDrive
        {
            get
            {
                if (this != this._context.TopLevelSessionState)
                {
                    return this._context.TopLevelSessionState.CurrentDrive;
                }
                return this.currentDrive;
            }
            set
            {
                if (value == null)
                {
                    throw PSTraceSource.NewArgumentNullException("value");
                }
                if (this != this._context.TopLevelSessionState)
                {
                    this._context.TopLevelSessionState.CurrentDrive = value;
                }
                else
                {
                    this.currentDrive = value;
                }
            }
        }

        internal PathInfo CurrentLocation
        {
            get
            {
                if (this.CurrentDrive == null)
                {
                    throw PSTraceSource.NewInvalidOperationException();
                }
                PathInfo info = new PathInfo(this.CurrentDrive, this.CurrentDrive.Provider, this.CurrentDrive.CurrentLocation, new SessionState(this));
                tracer.WriteLine("result = {0}", new object[] { info });
                return info;
            }
        }

        internal SessionStateScope CurrentScope
        {
            get
            {
                return this.currentScope;
            }
            set
            {
                this.currentScope = value;
            }
        }

        internal System.Management.Automation.ExecutionContext ExecutionContext
        {
            get
            {
                return this._context;
            }
        }

        internal List<AliasInfo> ExportedAliases
        {
            get
            {
                return this._exportedAliases;
            }
        }

        internal List<CmdletInfo> ExportedCmdlets
        {
            get
            {
                return this._exportedCmdlets;
            }
        }

        internal List<FunctionInfo> ExportedFunctions
        {
            get
            {
                return this._exportedFunctions;
            }
        }

        internal List<PSVariable> ExportedVariables
        {
            get
            {
                return this._exportedVariables;
            }
        }

        internal List<WorkflowInfo> ExportedWorkflows
        {
            get
            {
                return this._exportedWorkflows;
            }
        }

        internal SessionStateScope GlobalScope
        {
            get
            {
                return this._globalScope;
            }
        }

        internal LocationGlobber Globber
        {
            get
            {
                if (this.globberPrivate == null)
                {
                    this.globberPrivate = this._context.LocationGlobber;
                }
                return this.globberPrivate;
            }
        }

        internal ProviderIntrinsics InvokeProvider
        {
            get
            {
                if (this._invokeProvider == null)
                {
                    this._invokeProvider = new ProviderIntrinsics(this);
                }
                return this._invokeProvider;
            }
        }

        internal PSLanguageMode LanguageMode
        {
            get
            {
                return this._context.LanguageMode;
            }
            set
            {
                this._context.LanguageMode = value;
            }
        }

        internal PSModuleInfo Module
        {
            get
            {
                return this._module;
            }
            set
            {
                this._module = value;
            }
        }

        internal SessionStateScope ModuleScope
        {
            get
            {
                return this._moduleScope;
            }
        }

        internal Dictionary<string, PSModuleInfo> ModuleTable
        {
            get
            {
                return this._moduleTable;
            }
        }

        internal int ProviderCount
        {
            get
            {
                int num = 0;
                foreach (List<ProviderInfo> list in this.Providers.Values)
                {
                    num += list.Count;
                }
                return num;
            }
        }

        internal IEnumerable<ProviderInfo> ProviderList
        {
            get
            {
                Collection<ProviderInfo> collection = new Collection<ProviderInfo>();
                foreach (List<ProviderInfo> list in this.Providers.Values)
                {
                    foreach (ProviderInfo info in list)
                    {
                        collection.Add(info);
                    }
                }
                return collection;
            }
        }

        internal Dictionary<string, List<ProviderInfo>> Providers
        {
            get
            {
                if (this == this._context.TopLevelSessionState)
                {
                    return this._providers;
                }
                return this._context.TopLevelSessionState.Providers;
            }
        }

        internal Dictionary<ProviderInfo, PSDriveInfo> ProvidersCurrentWorkingDrive
        {
            get
            {
                if (this == this._context.TopLevelSessionState)
                {
                    return this._providersCurrentWorkingDrive;
                }
                return this._context.TopLevelSessionState.ProvidersCurrentWorkingDrive;
            }
        }

        internal SessionState PublicSessionState
        {
            get
            {
                if (this._publicSessionState == null)
                {
                    this._publicSessionState = new SessionState(this);
                }
                return this._publicSessionState;
            }
            set
            {
                this._publicSessionState = value;
            }
        }

        public List<string> Scripts
        {
            get
            {
                return this._scripts;
            }
        }

        internal SessionStateScope ScriptScope
        {
            get
            {
                return this.currentScope.ScriptScope;
            }
        }

        internal bool UseExportList
        {
            get
            {
                return this._useExportList;
            }
            set
            {
                this._useExportList = value;
            }
        }

        internal bool UseFullLanguageModeInDebugger
        {
            get
            {
                return this._context.UseFullLanguageModeInDebugger;
            }
        }

        
    }
}

