namespace System.Management.Automation
{
    using Microsoft.PowerShell;
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Internal.Host;
    using System.Management.Automation.Language;
    using System.Management.Automation.Runspaces;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    internal class ExecutionContext
    {
        private Dictionary<string, Assembly> _assemblyCache;
        private bool _assemblyCacheInitialized;
        private static bool _assemblyEventHandlerSet = false;
        private System.Management.Automation.AuthorizationManager _authorizationManager;
        private System.Management.Automation.Debugger _debugger;
        internal int _debuggingMode;
        private AutomationEngine _engine;
        private System.Management.Automation.EngineIntrinsics _engineIntrinsics;
        private SessionStateInternal _engineSessionState;
        private System.Management.Automation.EngineState _engineState;
        private PipelineWriter _externalErrorOutput;
        private PipelineWriter _externalProgressOutput;
        private TypeInfoDataBaseManager _formatDBManager;
        private object _formatInfo;
        private System.Management.Automation.HelpSystem _helpSystem;
        private System.Management.Automation.Runspaces.InitialSessionState _initialSessionState;
        private PSLanguageMode _languageMode;
        private System.Management.Automation.LocationGlobber _locationGlobber;
        private string _moduleBeingProcessed;
        private ModuleIntrinsics _modules;
        private string _previousModuleProcessed;
        private bool _questionMarkVariableValue;
        private System.Management.Automation.Runspaces.RunspaceConfiguration _runspaceConfiguration;
        private bool _scriptCommandProcessorShouldRethrowExit;
        private string _shellId;
        private SessionStateInternal _topLevelSessionState;
        private System.Management.Automation.Runspaces.TypeTable _typeTable;
        private CommandFactory commandFactory;
        private CommandProcessorBase currentCommandProcessor;
        private Runspace currentRunspace;
        private int debugTraceLevel;
        private bool debugTraceStep;
        private PSLocalEventManager eventManager;
        private bool exceptionHandlerInEnclosingStatementBlock;
        private Pipe expressionDebugOutputPipe;
        private Pipe expressionVerboseOutputPipe;
        private Pipe expressionWarningOutputPipe;
        private PipelineWriter externalSuccessOutput;
        private bool ignoreScriptDebug;
        private static object lockObject = new object();
        private System.Management.Automation.LogContextCache logContextCache;
        private System.Management.Automation.Internal.Host.InternalHost myHostInterface;
        private System.Management.Automation.ProviderNames providerNames;
        private Pipe shellFunctionErrorOutputPipe;
        private Pipe topLevelPipe;
        internal PSTransactionManager transactionManager;

        internal ExecutionContext(AutomationEngine engine, PSHost hostInterface, System.Management.Automation.Runspaces.InitialSessionState initialSessionState)
        {
            this.ignoreScriptDebug = true;
            this.logContextCache = new System.Management.Automation.LogContextCache();
            this._questionMarkVariableValue = true;
            this._initialSessionState = initialSessionState;
            this._authorizationManager = initialSessionState.AuthorizationManager;
            this.InitializeCommon(engine, hostInterface);
        }

        internal ExecutionContext(AutomationEngine engine, PSHost hostInterface, System.Management.Automation.Runspaces.RunspaceConfiguration runspaceConfiguration)
        {
            this.ignoreScriptDebug = true;
            this.logContextCache = new System.Management.Automation.LogContextCache();
            this._questionMarkVariableValue = true;
            this._runspaceConfiguration = runspaceConfiguration;
            this._authorizationManager = runspaceConfiguration.AuthorizationManager;
            this.InitializeCommon(engine, hostInterface);
        }

        internal Assembly AddAssembly(string name, string filename, out Exception error)
        {
            Assembly assembly = LoadAssembly(name, filename, out error);
            if (assembly == null)
            {
                return null;
            }
            if (!this._assemblyCache.ContainsKey(assembly.FullName))
            {
                this._assemblyCache.Add(assembly.FullName, assembly);
                if (this._assemblyCache.ContainsKey(assembly.GetName().Name))
                {
                    return assembly;
                }
                this._assemblyCache.Add(assembly.GetName().Name, assembly);
            }
            return assembly;
        }

        internal void AppendDollarError(object obj)
        {
            ErrorRecord record = obj as ErrorRecord;
            if ((record != null) || (obj is Exception))
            {
                ArrayList dollarErrorVariable = this.DollarErrorVariable as ArrayList;
                if (dollarErrorVariable != null)
                {
                    if (dollarErrorVariable.Count > 0)
                    {
                        if (dollarErrorVariable[0] == obj)
                        {
                            return;
                        }
                        ErrorRecord record2 = dollarErrorVariable[0] as ErrorRecord;
                        if (((record2 != null) && (record != null)) && (record2.Exception == record.Exception))
                        {
                            return;
                        }
                    }
                    object fastValue = this.EngineSessionState.CurrentScope.ErrorCapacity.FastValue;
                    if (fastValue != null)
                    {
                        try
                        {
                            fastValue = LanguagePrimitives.ConvertTo(fastValue, typeof(int), CultureInfo.InvariantCulture);
                        }
                        catch (PSInvalidCastException)
                        {
                        }
                        catch (OverflowException)
                        {
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                    int num = (fastValue is int) ? ((int) fastValue) : 0x100;
                    if (0 > num)
                    {
                        num = 0;
                    }
                    else if (0x8000 < num)
                    {
                        num = 0x8000;
                    }
                    if (0 >= num)
                    {
                        dollarErrorVariable.Clear();
                    }
                    else
                    {
                        int count = dollarErrorVariable.Count - (num - 1);
                        if (0 < count)
                        {
                            dollarErrorVariable.RemoveRange(num - 1, count);
                        }
                        dollarErrorVariable.Insert(0, obj);
                    }
                }
            }
        }

        internal static void CheckStackDepth()
        {
            try
            {
                RuntimeHelpers.EnsureSufficientExecutionStack();
            }
            catch (InsufficientExecutionStackException)
            {
                throw new ScriptCallDepthException();
            }
        }

        internal CommandProcessorBase CreateCommand(string command, bool dotSource)
        {
            if (this.commandFactory == null)
            {
                this.commandFactory = new CommandFactory(this);
            }
            CommandProcessorBase base2 = this.commandFactory.CreateCommand(command, this.EngineSessionState.CurrentScope.ScopeOrigin, new bool?(!dotSource));
            if ((base2 != null) && (base2 is ScriptCommandProcessorBase))
            {
                base2.Command.CommandOriginInternal = CommandOrigin.Internal;
            }
            return base2;
        }

        internal bool GetBooleanPreference(VariablePath preferenceVariablePath, bool defaultPref, out bool defaultUsed)
        {
            CmdletProviderContext context = null;
            SessionStateScope scope = null;
            object valueToConvert = this.EngineSessionState.GetVariableValue(preferenceVariablePath, out context, out scope);
            if (valueToConvert == null)
            {
                defaultUsed = true;
                return defaultPref;
            }
            bool result = defaultPref;
            defaultUsed = !LanguagePrimitives.TryConvertTo<bool>(valueToConvert, out result);
            if (!defaultUsed)
            {
                return result;
            }
            return defaultPref;
        }

        internal T GetEnumPreference<T>(VariablePath preferenceVariablePath, T defaultPref, out bool defaultUsed)
        {
            CmdletProviderContext context = null;
            SessionStateScope scope = null;
            object obj2 = this.EngineSessionState.GetVariableValue(preferenceVariablePath, out context, out scope);
            if (obj2 is T)
            {
                if (obj2 is ActionPreference)
                {
                    ActionPreference o = (ActionPreference) obj2;
                    if (o == ActionPreference.Ignore)
                    {
                        this.EngineSessionState.SetVariableValue(preferenceVariablePath.UserPath, defaultPref);
                        throw new NotSupportedException(StringUtil.Format(ErrorPackage.UnsupportedPreferenceError, o));
                    }
                }
                T local = (T) obj2;
                defaultUsed = false;
                return local;
            }
            defaultUsed = true;
            T local2 = defaultPref;
            if (obj2 != null)
            {
                try
                {
                    string str2 = obj2 as string;
                    if (str2 != null)
                    {
                        local2 = (T) Enum.Parse(typeof(T), str2, true);
                        defaultUsed = false;
                        return local2;
                    }
                    local2 = (T) obj2;
                    defaultUsed = false;
                }
                catch (InvalidCastException)
                {
                }
                catch (ArgumentException)
                {
                }
            }
            return local2;
        }

        internal object GetVariableValue(VariablePath path)
        {
            CmdletProviderContext context;
            SessionStateScope scope;
            return this._engineSessionState.GetVariableValue(path, out context, out scope);
        }

        internal object GetVariableValue(VariablePath path, object defaultValue)
        {
            CmdletProviderContext context;
            SessionStateScope scope;
            return (this._engineSessionState.GetVariableValue(path, out context, out scope) ?? defaultValue);
        }

        private void InitializeCommon(AutomationEngine engine, PSHost hostInterface)
        {
            this._engine = engine;
            if (!_assemblyEventHandlerSet)
            {
                lock (lockObject)
                {
                    if (!_assemblyEventHandlerSet)
                    {
                        AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ExecutionContext.PowerShellAssemblyResolveHandler);
                        _assemblyEventHandlerSet = true;
                    }
                }
            }
            this._debugger = new System.Management.Automation.Debugger(this);
            this.eventManager = new PSLocalEventManager(this);
            this.transactionManager = new PSTransactionManager();
            this.myHostInterface = hostInterface as System.Management.Automation.Internal.Host.InternalHost;
            if (this.myHostInterface == null)
            {
                this.myHostInterface = new System.Management.Automation.Internal.Host.InternalHost(hostInterface, this);
            }
            this._assemblyCache = new Dictionary<string, Assembly>();
            this._topLevelSessionState = this._engineSessionState = new SessionStateInternal(this);
            if (this._authorizationManager == null)
            {
                this._authorizationManager = new System.Management.Automation.AuthorizationManager(null);
            }
            this._modules = new ModuleIntrinsics(this);
        }

        private bool IsModuleCommandCurrentlyRunning(out Cmdlet command, out string errorId)
        {
            command = null;
            errorId = null;
            bool flag = false;
            if (this.CurrentCommandProcessor != null)
            {
                CommandInfo commandInfo = this.CurrentCommandProcessor.CommandInfo;
                if ((string.Equals(commandInfo.Name, "Import-Module", StringComparison.OrdinalIgnoreCase) || string.Equals(commandInfo.Name, "Remove-Module", StringComparison.OrdinalIgnoreCase)) && (commandInfo.CommandType.Equals(CommandTypes.Cmdlet) && System.Management.Automation.Runspaces.InitialSessionState.CoreModule.Equals(commandInfo.ModuleName, StringComparison.OrdinalIgnoreCase)))
                {
                    flag = true;
                    command = (Cmdlet) this.CurrentCommandProcessor.Command;
                    errorId = string.Equals(commandInfo.Name, "Import-Module", StringComparison.OrdinalIgnoreCase) ? "Module_ImportModuleError" : "Module_RemoveModuleError";
                }
            }
            return flag;
        }

        internal bool IsStrictVersion(int majorVersion)
        {
            for (SessionStateScope scope = this.EngineSessionState.CurrentScope; scope != null; scope = scope.Parent)
            {
                if (scope.StrictModeVersion != null)
                {
                    return (scope.StrictModeVersion.Major >= majorVersion);
                }
                if (scope == this.EngineSessionState.ModuleScope)
                {
                    break;
                }
            }
            return false;
        }

        internal static bool IsStrictVersion(ExecutionContext context, int majorVersion)
        {
            if (context == null)
            {
                context = LocalPipeline.GetExecutionContextFromTLS();
            }
            if (context == null)
            {
                return false;
            }
            return context.IsStrictVersion(majorVersion);
        }

        internal bool IsTopLevelPipe(Pipe pipeToCheck)
        {
            return (pipeToCheck == this.topLevelPipe);
        }

        internal static Assembly LoadAssembly(string name, string filename, out Exception error)
        {
            Assembly assembly = null;
            error = null;
            string assemblyString = null;
			if (!string.IsNullOrEmpty(name) || !string.IsNullOrEmpty (filename))
            {
                try
                {
					if (!string.IsNullOrEmpty (filename) && File.Exists (filename))
					{
						assembly = Assembly.LoadFrom (filename);
					}
					else if (!string.IsNullOrEmpty (name)) {
						assemblyString = name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ? Path.GetFileNameWithoutExtension(name) : name;
                    	assembly = Assembly.Load(assemblyString);
					}
                }
                catch (FileNotFoundException exception)
                {
                    error = exception;
                }
                catch (FileLoadException exception2)
                {
                    error = exception2;
                    return null;
                }
                catch (BadImageFormatException exception3)
                {
                    error = exception3;
                    return null;
                }
                catch (SecurityException exception4)
                {
                    error = exception4;
                    return null;
                }
            }
            if (assembly != null)
            {
                return assembly;
            }
            if (!string.IsNullOrEmpty(filename))
            {
                error = null;
                try
                {
                    return Assembly.LoadFrom(filename);
                }
                catch (FileNotFoundException exception5)
                {
                    error = exception5;
                }
                catch (FileLoadException exception6)
                {
                    error = exception6;
                    return null;
                }
                catch (BadImageFormatException exception7)
                {
                    error = exception7;
                    return null;
                }
                catch (SecurityException exception8)
                {
                    error = exception8;
                    return null;
                }
            }
            if (!string.IsNullOrEmpty(assemblyString))
            {
                error = null;
                try
                {
                    return Assembly.LoadWithPartialName(assemblyString);
                }
                catch (FileNotFoundException exception9)
                {
                    error = exception9;
                }
                catch (FileLoadException exception10)
                {
                    error = exception10;
                }
                catch (BadImageFormatException exception11)
                {
                    error = exception11;
                }
                catch (SecurityException exception12)
                {
                    error = exception12;
                }
            }
            return null;
        }

        internal void PopPipelineProcessor(bool fromSteppablePipeline)
        {
            if (this.currentRunspace != null)
            {
                LocalPipeline currentlyRunningPipeline = (LocalPipeline) ((RunspaceBase) this.currentRunspace).GetCurrentlyRunningPipeline();
                if (currentlyRunningPipeline != null)
                {
                    currentlyRunningPipeline.Stopper.Pop(fromSteppablePipeline);
                }
            }
        }

        private static Assembly PowerShellAssemblyResolveHandler(object sender, ResolveEventArgs args)
        {
            ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            if (((executionContextFromTLS != null) && (executionContextFromTLS._assemblyCache != null)) && executionContextFromTLS._assemblyCache.ContainsKey(args.Name))
            {
                return executionContextFromTLS._assemblyCache[args.Name];
            }
            return null;
        }

        internal void PushPipelineProcessor(PipelineProcessor pp)
        {
            if (this.currentRunspace != null)
            {
                LocalPipeline currentlyRunningPipeline = (LocalPipeline) ((RunspaceBase) this.currentRunspace).GetCurrentlyRunningPipeline();
                if (currentlyRunningPipeline != null)
                {
                    currentlyRunningPipeline.Stopper.Push(pp);
                }
            }
        }

        internal Pipe RedirectErrorPipe(Pipe newPipe)
        {
            Pipe shellFunctionErrorOutputPipe = this.shellFunctionErrorOutputPipe;
            this.ShellFunctionErrorOutputPipe = newPipe;
            return shellFunctionErrorOutputPipe;
        }

        internal void RemoveAssembly(string name)
        {
            if (this._assemblyCache.ContainsKey(name))
            {
                Assembly assembly = this._assemblyCache[name];
                if (assembly != null)
                {
                    this._assemblyCache.Remove(name);
                    this._assemblyCache.Remove(assembly.GetName().Name);
                }
            }
        }

        internal void ReportEngineStartupError(Exception e)
        {
            try
            {
                Cmdlet cmdlet;
                string str;
                if (this.IsModuleCommandCurrentlyRunning(out cmdlet, out str))
                {
                    ErrorRecord errorRecord = null;
                    RuntimeException replaceParentContainsErrorRecordException = e as RuntimeException;
                    errorRecord = (replaceParentContainsErrorRecordException != null) ? new ErrorRecord(replaceParentContainsErrorRecordException.ErrorRecord, replaceParentContainsErrorRecordException) : new ErrorRecord(e, str, ErrorCategory.OperationStopped, null);
                    cmdlet.WriteError(errorRecord);
                }
                else
                {
                    PSHost engineHostInterface = this.EngineHostInterface;
                    if (engineHostInterface != null)
                    {
                        PSHostUserInterface uI = engineHostInterface.UI;
                        if (uI != null)
                        {
                            uI.WriteErrorLine(e.Message);
                        }
                    }
                }
            }
            catch (Exception exception2)
            {
                CommandProcessorBase.CheckForSevereException(exception2);
            }
        }

        internal void ReportEngineStartupError(ErrorRecord errorRecord)
        {
            try
            {
                Cmdlet cmdlet;
                string str;
                if (this.IsModuleCommandCurrentlyRunning(out cmdlet, out str))
                {
                    cmdlet.WriteError(errorRecord);
                }
                else
                {
                    PSHost engineHostInterface = this.EngineHostInterface;
                    if (engineHostInterface != null)
                    {
                        PSHostUserInterface uI = engineHostInterface.UI;
                        if (uI != null)
                        {
                            uI.WriteErrorLine(errorRecord.ToString());
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
            }
        }

        internal void ReportEngineStartupError(string error)
        {
            try
            {
                Cmdlet cmdlet;
                string str;
                if (this.IsModuleCommandCurrentlyRunning(out cmdlet, out str))
                {
                    RuntimeException replaceParentContainsErrorRecordException = InterpreterError.NewInterpreterException(null, typeof(RuntimeException), null, str, "{0}", new object[] { error });
                    cmdlet.WriteError(new ErrorRecord(replaceParentContainsErrorRecordException.ErrorRecord, replaceParentContainsErrorRecordException));
                }
                else
                {
                    PSHost engineHostInterface = this.EngineHostInterface;
                    if (engineHostInterface != null)
                    {
                        PSHostUserInterface uI = engineHostInterface.UI;
                        if (uI != null)
                        {
                            uI.WriteErrorLine(error);
                        }
                    }
                }
            }
            catch (Exception exception2)
            {
                CommandProcessorBase.CheckForSevereException(exception2);
            }
        }

        internal void ReportEngineStartupError(string resourceString, params object[] arguments)
        {
            try
            {
                Cmdlet cmdlet;
                string str;
                if (this.IsModuleCommandCurrentlyRunning(out cmdlet, out str))
                {
                    RuntimeException replaceParentContainsErrorRecordException = InterpreterError.NewInterpreterException(null, typeof(RuntimeException), null, str, resourceString, arguments);
                    cmdlet.WriteError(new ErrorRecord(replaceParentContainsErrorRecordException.ErrorRecord, replaceParentContainsErrorRecordException));
                }
                else
                {
                    PSHost engineHostInterface = this.EngineHostInterface;
                    if (engineHostInterface != null)
                    {
                        PSHostUserInterface uI = engineHostInterface.UI;
                        if (uI != null)
                        {
                            uI.WriteErrorLine(StringUtil.Format(resourceString, arguments));
                        }
                    }
                }
            }
            catch (Exception exception2)
            {
                CommandProcessorBase.CheckForSevereException(exception2);
            }
        }

        internal void ResetManagers()
        {
            this._debugger = new System.Management.Automation.Debugger(this);
            if (this.eventManager != null)
            {
                this.eventManager.Dispose();
            }
            this.eventManager = new PSLocalEventManager(this);
            if (this.transactionManager != null)
            {
                this.transactionManager.Dispose();
            }
            this.transactionManager = new PSTransactionManager();
        }

        internal void ResetRedirection()
        {
            this.shellFunctionErrorOutputPipe = null;
            this.topLevelPipe = null;
        }

        internal void ResetShellFunctionErrorOutputPipe()
        {
            this.topLevelPipe = null;
            this.shellFunctionErrorOutputPipe = null;
        }

        internal void RestoreErrorPipe(Pipe pipe)
        {
            this.shellFunctionErrorOutputPipe = pipe;
        }

        internal void RunspaceClosingNotification()
        {
            if (this.RunspaceConfiguration != null)
            {
                this.RunspaceConfiguration.Unbind(this);
            }
            this.EngineSessionState.RunspaceClosingNotification();
            if (this.eventManager != null)
            {
                this.eventManager.Dispose();
            }
            this.eventManager = null;
            if (this.transactionManager != null)
            {
                this.transactionManager.Dispose();
            }
            this.transactionManager = null;
        }

        internal SavedContextData SaveContextData()
        {
            return new SavedContextData(this);
        }

        internal void SetVariable(VariablePath path, object newValue)
        {
            this._engineSessionState.SetVariable(path, newValue, true, CommandOrigin.Internal);
        }

        internal void UpdateAssemblyCache()
        {
            string str = "";
            if (this.RunspaceConfiguration != null)
            {
                if (!this._assemblyCacheInitialized)
                {
                    foreach (AssemblyConfigurationEntry entry in this.RunspaceConfiguration.Assemblies)
                    {
                        Exception error = null;
                        this.AddAssembly(entry.Name, entry.FileName, out error);
                        if (error != null)
                        {
                            str = str + "\n" + error.Message;
                        }
                    }
                    this._assemblyCacheInitialized = true;
                }
                else
                {
                    foreach (AssemblyConfigurationEntry entry2 in this.RunspaceConfiguration.Assemblies.UpdateList)
                    {
                        switch (entry2.Action)
                        {
                            case UpdateAction.Add:
                            {
                                Exception exception2 = null;
                                this.AddAssembly(entry2.Name, entry2.FileName, out exception2);
                                if (exception2 != null)
                                {
                                    str = str + "\n" + exception2.Message;
                                }
                                break;
                            }
                            case UpdateAction.Remove:
                                this.RemoveAssembly(entry2.Name);
                                break;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(str))
                {
                    throw new RuntimeException(StringUtil.Format(MiniShellErrors.UpdateAssemblyErrors, str));
                }
            }
        }

        internal Dictionary<string, Assembly> AssemblyCache
        {
            get
            {
                return this._assemblyCache;
            }
        }

        internal System.Management.Automation.AuthorizationManager AuthorizationManager
        {
            get
            {
                return this._authorizationManager;
            }
        }

        internal System.Management.Automation.CommandDiscovery CommandDiscovery
        {
            get
            {
                return this._engine.CommandDiscovery;
            }
        }

        internal ConfirmImpact ConfirmPreferenceVariable
        {
            get
            {
                bool defaultUsed = false;
                return this.GetEnumPreference<ConfirmImpact>(SpecialVariables.ConfirmPreferenceVarPath, ConfirmImpact.High, out defaultUsed);
            }
            set
            {
                this.EngineSessionState.SetVariable(SpecialVariables.ConfirmPreferenceVarPath, LanguagePrimitives.ConvertTo(value, typeof(ConfirmImpact), CultureInfo.InvariantCulture), true, CommandOrigin.Internal);
            }
        }

        internal CommandProcessorBase CurrentCommandProcessor
        {
            get
            {
                return this.currentCommandProcessor;
            }
            set
            {
                this.currentCommandProcessor = value;
            }
        }

        internal RuntimeException CurrentExceptionBeingHandled { get; set; }

        internal bool CurrentPipelineStopping
        {
            get
            {
                if (this.currentRunspace == null)
                {
                    return false;
                }
                LocalPipeline currentlyRunningPipeline = (LocalPipeline) ((RunspaceBase) this.currentRunspace).GetCurrentlyRunningPipeline();
                if (currentlyRunningPipeline == null)
                {
                    return false;
                }
                return currentlyRunningPipeline.IsStopping;
            }
        }

        internal Runspace CurrentRunspace
        {
            get
            {
                return this.currentRunspace;
            }
            set
            {
                this.currentRunspace = value;
            }
        }

        internal System.Management.Automation.Debugger Debugger
        {
            get
            {
                return this._debugger;
            }
        }

        internal ActionPreference DebugPreferenceVariable
        {
            get
            {
                bool defaultUsed = false;
                return this.GetEnumPreference<ActionPreference>(SpecialVariables.DebugPreferenceVarPath, ActionPreference.SilentlyContinue, out defaultUsed);
            }
            set
            {
                this.EngineSessionState.SetVariable(SpecialVariables.DebugPreferenceVarPath, LanguagePrimitives.ConvertTo(value, typeof(ActionPreference), CultureInfo.InvariantCulture), true, CommandOrigin.Internal);
            }
        }

        internal object DollarErrorVariable
        {
            get
            {
                CmdletProviderContext context = null;
                SessionStateScope scope = null;
                if (!this.eventManager.IsExecutingEventAction)
                {
                    return this.EngineSessionState.GetVariableValue(SpecialVariables.ErrorVarPath, out context, out scope);
                }
                return this.EngineSessionState.GetVariableValue(SpecialVariables.EventErrorVarPath, out context, out scope);
            }
            set
            {
                this.EngineSessionState.SetVariable(SpecialVariables.ErrorVarPath, value, true, CommandOrigin.Internal);
            }
        }

        internal AutomationEngine Engine
        {
            get
            {
                return this._engine;
            }
        }

        internal System.Management.Automation.Internal.Host.InternalHost EngineHostInterface
        {
            get
            {
                return this.myHostInterface;
            }
        }

        internal System.Management.Automation.EngineIntrinsics EngineIntrinsics
        {
            get
            {
                if (this._engineIntrinsics == null)
                {
                    this._engineIntrinsics = new System.Management.Automation.EngineIntrinsics(this);
                }
                return this._engineIntrinsics;
            }
        }

        internal SessionStateInternal EngineSessionState
        {
            get
            {
                return this._engineSessionState;
            }
            set
            {
                this._engineSessionState = value;
            }
        }

        internal System.Management.Automation.EngineState EngineState
        {
            get
            {
                return this._engineState;
            }
            set
            {
                this._engineState = value;
            }
        }

        internal ActionPreference ErrorActionPreferenceVariable
        {
            get
            {
                bool defaultUsed = false;
                return this.GetEnumPreference<ActionPreference>(SpecialVariables.ErrorActionPreferenceVarPath, ActionPreference.Continue, out defaultUsed);
            }
            set
            {
                this.EngineSessionState.SetVariable(SpecialVariables.ErrorActionPreferenceVarPath, LanguagePrimitives.ConvertTo(value, typeof(ActionPreference), CultureInfo.InvariantCulture), true, CommandOrigin.Internal);
            }
        }

        internal PSLocalEventManager Events
        {
            get
            {
                return this.eventManager;
            }
        }

        internal bool ExceptionHandlerInEnclosingStatementBlock
        {
            get
            {
                return this.exceptionHandlerInEnclosingStatementBlock;
            }
            set
            {
                this.exceptionHandlerInEnclosingStatementBlock = value;
            }
        }

        internal Pipe ExpressionDebugOutputPipe
        {
            get
            {
                return this.expressionDebugOutputPipe;
            }
            set
            {
                this.expressionDebugOutputPipe = value;
            }
        }

        internal Pipe ExpressionVerboseOutputPipe
        {
            get
            {
                return this.expressionVerboseOutputPipe;
            }
            set
            {
                this.expressionVerboseOutputPipe = value;
            }
        }

        internal Pipe ExpressionWarningOutputPipe
        {
            get
            {
                return this.expressionWarningOutputPipe;
            }
            set
            {
                this.expressionWarningOutputPipe = value;
            }
        }

        internal PipelineWriter ExternalErrorOutput
        {
            get
            {
                return this._externalErrorOutput;
            }
            set
            {
                this._externalErrorOutput = value;
            }
        }

        internal PipelineWriter ExternalProgressOutput
        {
            get
            {
                return this._externalProgressOutput;
            }
            set
            {
                this._externalProgressOutput = value;
            }
        }

        internal PipelineWriter ExternalSuccessOutput
        {
            get
            {
                return this.externalSuccessOutput;
            }
            set
            {
                this.externalSuccessOutput = value;
            }
        }

        internal TypeInfoDataBaseManager FormatDBManager
        {
            get
            {
                if ((this.RunspaceConfiguration != null) && (this.RunspaceConfiguration.FormatDBManager != null))
                {
                    return this.RunspaceConfiguration.FormatDBManager;
                }
                if (this._formatDBManager == null)
                {
                    this._formatDBManager = new TypeInfoDataBaseManager();
                    this._formatDBManager.Update(this.AuthorizationManager, this.EngineHostInterface);
                    if (this.InitialSessionState != null)
                    {
                        this._formatDBManager.DisableFormatTableUpdates = this.InitialSessionState.DisableFormatUpdates;
                    }
                }
                return this._formatDBManager;
            }
            set
            {
                if (this.RunspaceConfiguration != null)
                {
                    throw new NotImplementedException("set_FormatDBManager()");
                }
                this._formatDBManager = value;
            }
        }

        internal object FormatInfo
        {
            get
            {
                return this._formatInfo;
            }
            set
            {
                this._formatInfo = value;
            }
        }

        internal static bool HasEverUsedConstrainedLanguage
        {
            get;
            set;
        }

        internal bool HasRunspaceEverUsedConstrainedLanguageMode { get; private set; }

        internal System.Management.Automation.HelpSystem HelpSystem
        {
            get
            {
                if (this._helpSystem == null)
                {
                    this._helpSystem = new System.Management.Automation.HelpSystem(this);
                }
                return this._helpSystem;
            }
        }

        internal bool IgnoreScriptDebug
        {
            get
            {
                return this.ignoreScriptDebug;
            }
            set
            {
                this.ignoreScriptDebug = value;
            }
        }

        internal System.Management.Automation.Runspaces.InitialSessionState InitialSessionState
        {
            get
            {
                return this._initialSessionState;
            }
        }

        internal System.Management.Automation.Internal.Host.InternalHost InternalHost
        {
            get
            {
                return this.myHostInterface;
            }
        }

        internal bool IsSingleShell
        {
            get
            {
                return ((this.RunspaceConfiguration is RunspaceConfigForSingleShell) || (this.InitialSessionState != null));
            }
        }

        internal PSLanguageMode LanguageMode
        {
            get
            {
                return this._languageMode;
            }
            set
            {
                if (value == PSLanguageMode.ConstrainedLanguage)
                {
                    HasEverUsedConstrainedLanguage = true;
                    this.HasRunspaceEverUsedConstrainedLanguageMode = true;
                    PSSetMemberBinder.InvalidateCache();
                    PSInvokeMemberBinder.InvalidateCache();
                    PSConvertBinder.InvalidateCache();
                    PSBinaryOperationBinder.InvalidateCache();
                    PSGetIndexBinder.InvalidateCache();
                    PSSetIndexBinder.InvalidateCache();
                }
                LanguagePrimitives.ResetCaches(null);
                this._languageMode = value;
            }
        }

        internal System.Management.Automation.LocationGlobber LocationGlobber
        {
            get
            {
                this._locationGlobber = new System.Management.Automation.LocationGlobber(this.SessionState);
                return this._locationGlobber;
            }
        }

        internal System.Management.Automation.LogContextCache LogContextCache
        {
            get
            {
                return this.logContextCache;
            }
        }

        internal string ModuleBeingProcessed
        {
            get
            {
                return this._moduleBeingProcessed;
            }
            set
            {
                this._moduleBeingProcessed = value;
            }
        }

        internal ModuleIntrinsics Modules
        {
            get
            {
                return this._modules;
            }
        }

        internal string PreviousModuleProcessed
        {
            get
            {
                return this._previousModuleProcessed;
            }
            set
            {
                this._previousModuleProcessed = value;
            }
        }

        internal System.Management.Automation.ProviderNames ProviderNames
        {
            get
            {
                if (this.providerNames == null)
                {
                    if (this.IsSingleShell)
                    {
                        this.providerNames = new SingleShellProviderNames();
                    }
                    else
                    {
                        this.providerNames = new CustomShellProviderNames();
                    }
                }
                return this.providerNames;
            }
        }

        internal int PSDebugTraceLevel
        {
            get
            {
                if (!this.ignoreScriptDebug)
                {
                    return this.debugTraceLevel;
                }
                return 0;
            }
            set
            {
                this.debugTraceLevel = value;
            }
        }

        internal bool PSDebugTraceStep
        {
            get
            {
                return (!this.ignoreScriptDebug && this.debugTraceStep);
            }
            set
            {
                this.debugTraceStep = value;
            }
        }

        internal bool PSWorkflowModuleLoadingInProgress { get; set; }

        internal bool QuestionMarkVariableValue
        {
            get
            {
                return this._questionMarkVariableValue;
            }
            set
            {
                this._questionMarkVariableValue = value;
            }
        }

        internal System.Management.Automation.Runspaces.RunspaceConfiguration RunspaceConfiguration
        {
            get
            {
                return this._runspaceConfiguration;
            }
        }

        internal bool ScriptCommandProcessorShouldRethrowExit
        {
            get
            {
                return this._scriptCommandProcessorShouldRethrowExit;
            }
            set
            {
                this._scriptCommandProcessorShouldRethrowExit = value;
            }
        }

        internal System.Management.Automation.SessionState SessionState
        {
            get
            {
                return this._engineSessionState.PublicSessionState;
            }
        }

        internal Pipe ShellFunctionErrorOutputPipe
        {
            get
            {
                return this.shellFunctionErrorOutputPipe;
            }
            set
            {
                if (this.topLevelPipe == null)
                {
                    this.topLevelPipe = value;
                }
                this.shellFunctionErrorOutputPipe = value;
            }
        }

        internal string ShellID
        {
            get
            {
                if (this._shellId == null)
                {
                    if ((this._authorizationManager is PSAuthorizationManager) && !string.IsNullOrEmpty(this._authorizationManager.ShellId))
                    {
                        this._shellId = this._authorizationManager.ShellId;
                    }
                    else if ((this._runspaceConfiguration != null) && !string.IsNullOrEmpty(this._runspaceConfiguration.ShellId))
                    {
                        this._shellId = this._runspaceConfiguration.ShellId;
                    }
                    else
                    {
                        this._shellId = Utils.DefaultPowerShellShellID;
                    }
                }
                return this._shellId;
            }
        }

        internal bool ShouldTraceStatement
        {
            get
            {
                if (this.ignoreScriptDebug)
                {
                    return false;
                }
                if (this.debugTraceLevel <= 0)
                {
                    return this.debugTraceStep;
                }
                return true;
            }
        }

        internal SessionStateInternal TopLevelSessionState
        {
            get
            {
                return this._topLevelSessionState;
            }
        }

        internal PSTransactionManager TransactionManager
        {
            get
            {
                return this.transactionManager;
            }
        }

        internal System.Management.Automation.Runspaces.TypeTable TypeTable
        {
            get
            {
                if ((this.RunspaceConfiguration != null) && (this.RunspaceConfiguration.TypeTable != null))
                {
                    return this.RunspaceConfiguration.TypeTable;
                }
                if (this._typeTable == null)
                {
                    this._typeTable = new System.Management.Automation.Runspaces.TypeTable();
                }
                return this._typeTable;
            }
            set
            {
                if (this.RunspaceConfiguration != null)
                {
                    throw new NotImplementedException("set_TypeTable()");
                }
                this._typeTable = value;
            }
        }

        internal bool UseFullLanguageModeInDebugger
        {
            get
            {
                if (this._initialSessionState == null)
                {
                    return false;
                }
                return this._initialSessionState.UseFullLanguageModeInDebugger;
            }
        }

        internal ActionPreference VerbosePreferenceVariable
        {
            get
            {
                bool defaultUsed = false;
                return this.GetEnumPreference<ActionPreference>(SpecialVariables.VerbosePreferenceVarPath, ActionPreference.SilentlyContinue, out defaultUsed);
            }
            set
            {
                this.EngineSessionState.SetVariable(SpecialVariables.VerbosePreferenceVarPath, LanguagePrimitives.ConvertTo(value, typeof(ActionPreference), CultureInfo.InvariantCulture), true, CommandOrigin.Internal);
            }
        }

        internal ActionPreference WarningActionPreferenceVariable
        {
            get
            {
                bool defaultUsed = false;
                return this.GetEnumPreference<ActionPreference>(SpecialVariables.WarningPreferenceVarPath, ActionPreference.Continue, out defaultUsed);
            }
            set
            {
                this.EngineSessionState.SetVariable(SpecialVariables.WarningPreferenceVarPath, LanguagePrimitives.ConvertTo(value, typeof(ActionPreference), CultureInfo.InvariantCulture), true, CommandOrigin.Internal);
            }
        }

        internal object WhatIfPreferenceVariable
        {
            get
            {
                CmdletProviderContext context = null;
                SessionStateScope scope = null;
                return this.EngineSessionState.GetVariableValue(SpecialVariables.WhatIfPreferenceVarPath, out context, out scope);
            }
            set
            {
                this.EngineSessionState.SetVariable(SpecialVariables.WhatIfPreferenceVarPath, value, true, CommandOrigin.Internal);
            }
        }

        internal class SavedContextData
        {
            private bool IgnoreScriptDebug;
            private int PSDebug;
            private Pipe ShellFunctionErrorOutputPipe;
            private bool StepScript;
            private Pipe TopLevelPipe;

            public SavedContextData(ExecutionContext context)
            {
                this.StepScript = context.PSDebugTraceStep;
                this.IgnoreScriptDebug = context.IgnoreScriptDebug;
                this.PSDebug = context.PSDebugTraceLevel;
                this.ShellFunctionErrorOutputPipe = context.ShellFunctionErrorOutputPipe;
                this.TopLevelPipe = context.topLevelPipe;
            }

            public void RestoreContextData(ExecutionContext context)
            {
                context.PSDebugTraceStep = this.StepScript;
                context.IgnoreScriptDebug = this.IgnoreScriptDebug;
                context.PSDebugTraceLevel = this.PSDebug;
                context.ShellFunctionErrorOutputPipe = this.ShellFunctionErrorOutputPipe;
                context.topLevelPipe = this.TopLevelPipe;
            }
        }
    }
}

