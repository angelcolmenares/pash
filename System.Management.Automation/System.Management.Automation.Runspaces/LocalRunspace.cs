namespace System.Management.Automation.Runspaces
{
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Reflection;
    using System.Threading;

    internal sealed class LocalRunspace : RunspaceBase
    {
        private System.Management.Automation.CommandFactory _commandFactory;
        private bool _disposed;
        private AutomationEngine _engine;
        private Microsoft.PowerShell.Commands.History _history;
        private System.Management.Automation.JobManager _jobManager;
        private System.Management.Automation.JobRepository _jobRepository;
        private System.Management.Automation.RunspaceRepository _runspaceRepository;
        private PSPrimitiveDictionary applicationPrivateData;
        private PSThreadOptions createThreadOptions;
        private PipelineThread pipelineThread;
        [TraceSource("RunspaceInit", "Initialization code for Runspace")]
        private static PSTraceSource runspaceInitTracer = PSTraceSource.GetTracer("RunspaceInit", "Initialization code for Runspace", false);

        internal LocalRunspace(PSHost host, InitialSessionState initialSessionState) : base(host, initialSessionState)
        {
        }

        internal LocalRunspace(PSHost host, RunspaceConfiguration runspaceConfig) : base(host, runspaceConfig)
        {

        }

        internal LocalRunspace(PSHost host, InitialSessionState initialSessionState, bool suppressClone) : base(host, initialSessionState, suppressClone)
        {
        }

        public override void Close()
        {
            if (this.Events != null)
            {
                this.Events.GenerateEvent("PowerShell.Exiting", null, new object[0], null, true, false);
            }
            base.Close();
            if (this.pipelineThread != null)
            {
                this.pipelineThread.Close();
            }
        }

        protected override void CloseHelper(bool syncCall)
        {
            if (syncCall)
            {
                this.DoCloseHelper();
            }
            else
            {
                new Thread(new ThreadStart(this.CloseThreadProc)).Start();
            }
        }

        private void CloseOrDisconnectAllRemoteRunspaces(Func<List<RemoteRunspace>> getRunspaces)
        {
            List<RemoteRunspace> list = getRunspaces();
            if (list.Count != 0)
            {
                EventHandler<EventArgs> handler = null;
                using (ManualResetEvent remoteRunspaceCloseCompleted = new ManualResetEvent(false))
                {
                    ThrottleManager manager = new ThrottleManager();
                    if (handler == null)
                    {
                        handler = (sender, e) => remoteRunspaceCloseCompleted.Set();
                    }
                    manager.ThrottleComplete += handler;
                    foreach (RemoteRunspace runspace in list)
                    {
                        IThrottleOperation operation = new CloseOrDisconnectRunspaceOperationHelper(runspace);
                        manager.AddOperation(operation);
                    }
                    manager.EndSubmitOperations();
                    remoteRunspaceCloseCompleted.WaitOne();
                }
            }
        }

        private void CloseThreadProc()
        {
            try
            {
                this.DoCloseHelper();
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
            }
        }

        protected override Pipeline CoreCreatePipeline(string command, bool addToHistory, bool isNested)
        {
            if (this._disposed)
            {
                throw PSTraceSource.NewObjectDisposedException("runspace");
            }
            return new LocalPipeline(this, command, addToHistory, isNested);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!this._disposed)
                {
                    lock (base.SyncRoot)
                    {
                        if (this._disposed)
                        {
                            return;
                        }
                        this._disposed = true;
                    }
                    if (disposing)
                    {
                        this.Close();
                        this._engine = null;
                        this._history = null;
                        this._jobManager = null;
                        this._jobRepository = null;
                        this._runspaceRepository = null;
                        if (base.RunspaceOpening != null)
                        {
                            base.RunspaceOpening.Dispose();
                            base.RunspaceOpening = null;
                        }
                        if ((base.ExecutionContext != null) && (base.ExecutionContext.Events != null))
                        {
                            try
                            {
                                base.ExecutionContext.Events.Dispose();
                            }
                            catch (ObjectDisposedException)
                            {
                            }
                        }
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private void DoCloseHelper()
        {
            base.StopPipelines();
            this.StopOrDisconnectAllJobs();
            this.CloseOrDisconnectAllRemoteRunspaces(delegate {
                List<RemoteRunspace> list = new List<RemoteRunspace>();
                foreach (PSSession session in this.RunspaceRepository.Runspaces)
                {
                    list.Add(session.Runspace as RemoteRunspace);
                }
                return list;
            });
            this._engine.Context.RunspaceClosingNotification();
            MshLog.LogEngineLifecycleEvent(this._engine.Context, EngineState.Stopped);
            this._engine = null;
            this._commandFactory = null;
            base.SetRunspaceState(RunspaceState.Closed);
            base.RaiseRunspaceStateEvents();
        }

        protected override object DoGetVariable(string name)
        {
            if (this._disposed)
            {
                throw PSTraceSource.NewObjectDisposedException("runspace");
            }
            return this._engine.Context.EngineSessionState.GetVariableValue(name);
        }

        private void DoOpenHelper()
        {
            if (this._disposed)
            {
                throw PSTraceSource.NewObjectDisposedException("runspace");
            }
            bool startLifeCycleEventWritten = false;
            runspaceInitTracer.WriteLine("begin open runspace", new object[0]);
            try
            {
                if (this.InitialSessionState != null)
                {
                    this._engine = new AutomationEngine(base.Host, null, this.InitialSessionState);
                }
                else
                {
                    this._engine = new AutomationEngine(base.Host, this.RunspaceConfiguration, null);
                }
                this._engine.Context.CurrentRunspace = this;
                MshLog.LogEngineLifecycleEvent(this._engine.Context, EngineState.Available);
                startLifeCycleEventWritten = true;
                this._commandFactory = new System.Management.Automation.CommandFactory(this._engine.Context);
                this._history = new Microsoft.PowerShell.Commands.History(this._engine.Context);
                this._jobRepository = new System.Management.Automation.JobRepository();
                this._jobManager = new System.Management.Automation.JobManager();
                this._runspaceRepository = new System.Management.Automation.RunspaceRepository();
                runspaceInitTracer.WriteLine("initializing built-in aliases and variable information", new object[0]);
                this.InitializeDefaults();
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                runspaceInitTracer.WriteLine("Runspace open failed", new object[0]);
                this.LogEngineHealthEvent(exception);
                if (startLifeCycleEventWritten)
                {
                    MshLog.LogEngineLifecycleEvent(this._engine.Context, EngineState.Stopped);
                }
                base.SetRunspaceState(RunspaceState.Broken, exception);
                base.RaiseRunspaceStateEvents();
                throw;
            }
            base.SetRunspaceState(RunspaceState.Opened);
            base.RunspaceOpening.Set();
            base.RaiseRunspaceStateEvents();
            runspaceInitTracer.WriteLine("runspace opened successfully", new object[0]);
            string environmentVariable = Environment.GetEnvironmentVariable("PSMODULEPATH");
            if (this.InitialSessionState != null)
            {
                try
                {
                    Environment.SetEnvironmentVariable("PSMODULEPATH", ModuleIntrinsics.GetSystemwideModulePath());
                    this.ProcessImportModule(this.InitialSessionState.CoreModulesToImport, startLifeCycleEventWritten);
                    this._engine.Context.EngineSessionState.Module = null;
                }
                finally
                {
                    Environment.SetEnvironmentVariable("PSMODULEPATH", environmentVariable);
                }
                this.ProcessImportModule(this.InitialSessionState.ModuleSpecificationsToImport, startLifeCycleEventWritten);
                InitialSessionState.SetSessionStateDrive(this._engine.Context, true);
                if (this.InitialSessionState.WarmUpTabCompletionOnIdle)
                {
                    ScriptBlock action = ScriptBlock.Create("$null = [System.Management.Automation.CommandCompletion]::CompleteInput('Set-Location', 12, $null)");
                    this._engine.Context.Events.SubscribeEvent(null, null, "PowerShell.OnIdle", null, action, true, false, 1);
                }
            }
        }

        protected override void DoSetVariable(string name, object value)
        {
            if (this._disposed)
            {
                throw PSTraceSource.NewObjectDisposedException("runspace");
            }
            this._engine.Context.EngineSessionState.SetVariableValue(name, value, CommandOrigin.Internal);
        }

        public override PSPrimitiveDictionary GetApplicationPrivateData()
        {
            if (this.applicationPrivateData == null)
            {
                lock (base.SyncRoot)
                {
                    if (this.applicationPrivateData == null)
                    {
                        this.applicationPrivateData = new PSPrimitiveDictionary();
                    }
                }
            }
            return this.applicationPrivateData;
        }

        private CommandInfo GetImportModuleCommandInfo(string moduleName, ref List<ErrorRecord> errors)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            if (LoadAssemblyHelper(executingAssembly.FullName, ref errors) != null)
            {
                Exception exception;
                Type implementingType = LanguagePrimitives.ConvertStringToType("Microsoft.PowerShell.Commands.ImportModuleCommand", out exception);
                if (exception != null)
                {
                    errors.Add(new ErrorRecord(exception, "CmdletNotFoundWhileLoadingModulesOnRunspaceOpen", ErrorCategory.InvalidType, null));
                }
                if (implementingType != null)
                {
                    return new CmdletInfo("Import-Module", implementingType, null, null, this._engine.Context);
                }
                errors.Add(new ErrorRecord(PSTraceSource.NewInvalidOperationException("RunspaceStrings", "CmdletNotFoundWhileLoadingModulesOnRunspaceOpen", new object[] { moduleName }), "CmdletNotFoundWhileLoadingModulesOnRunspaceOpen", ErrorCategory.InvalidOperation, null));
            }
            return null;
        }

        private CommandInfo GetOutDefaultCommandInfo(string moduleName, ref List<ErrorRecord> errors)
        {
            CommandInfo info = null;
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            byte[] publicKeyToken = executingAssembly.GetName().GetPublicKeyToken();
            if (publicKeyToken.Length == 0)
            {
                PSArgumentException exception = PSTraceSource.NewArgumentException("PublicKeyToken", "MshSnapinInfo", "PublicKeyTokenAccessFailed", new object[0]);
                errors.Add(new ErrorRecord(exception, "PublicKeyTokenAccessFailed", ErrorCategory.InvalidOperation, null));
                return info;
            }
            string str = PSSnapInReader.ConvertByteArrayToString(publicKeyToken);
            string str2 = "neutral";
            string str3 = "MSIL";
            if (LoadAssemblyHelper(string.Format(CultureInfo.InvariantCulture, "{0}, Version={1}, Culture={2}, PublicKeyToken={3}, ProcessorArchitecture={4}", new object[] { "Microsoft.PowerShell.Commands.Utility", executingAssembly.GetName().Version, str2, str, str3 }), ref errors) != null)
            {
                Exception exception2;
                Type implementingType = LanguagePrimitives.ConvertStringToType("Microsoft.PowerShell.Commands.OutDefaultCommand", out exception2);
                if (exception2 != null)
                {
                    errors.Add(new ErrorRecord(exception2, "CmdletNotFoundWhileLoadingModulesOnRunspaceOpen", ErrorCategory.InvalidType, null));
                }
                if (implementingType != null)
                {
                    return new CmdletInfo("Out-Default", implementingType, null, null, this._engine.Context);
                }
                errors.Add(new ErrorRecord(PSTraceSource.NewInvalidOperationException("RunspaceStrings", "CmdletNotFoundWhileLoadingModulesOnRunspaceOpen", new object[] { moduleName }), "CmdletNotFoundWhileLoadingModulesOnRunspaceOpen", ErrorCategory.InvalidOperation, null));
            }
            return info;
        }

        internal PipelineThread GetPipelineThread()
        {
            if (this.pipelineThread == null)
            {
                this.pipelineThread = new PipelineThread(base.ApartmentState);
            }
            return this.pipelineThread;
        }

        private void InitializeDefaults()
        {
            SessionStateInternal engineSessionState = this._engine.Context.EngineSessionState;
            engineSessionState.InitializeFixedVariables();
            if (this.RunspaceConfiguration != null)
            {
                bool addSetStrictMode = true;
                foreach (RunspaceConfigurationEntry entry in this.RunspaceConfiguration.Cmdlets)
                {
                    if (entry.Name.Equals("Set-StrictMode", StringComparison.OrdinalIgnoreCase))
                    {
                        addSetStrictMode = false;
                        break;
                    }
                }
                engineSessionState.AddBuiltInEntries(addSetStrictMode);
            }
        }

        private static Assembly LoadAssemblyHelper(string name, ref List<ErrorRecord> errors)
        {
            Assembly assembly = null;
            try
            {
                assembly = Assembly.Load(name);
            }
            catch (FileLoadException exception)
            {
                errors.Add(new ErrorRecord(exception, "AssemblyLoadFailure", ErrorCategory.InvalidOperation, null));
            }
            catch (BadImageFormatException exception2)
            {
                errors.Add(new ErrorRecord(exception2, "AssemblyLoadFailure", ErrorCategory.InvalidOperation, null));
            }
            catch (FileNotFoundException exception3)
            {
                errors.Add(new ErrorRecord(exception3, "AssemblyLoadFailure", ErrorCategory.ResourceUnavailable, null));
            }
            return assembly;
        }

        internal void LogEngineHealthEvent(Exception exception)
        {
            this.LogEngineHealthEvent(exception, Severity.Error, 0x67, null);
        }

        internal void LogEngineHealthEvent(Exception exception, Severity severity, int id, Dictionary<string, string> additionalInfo)
        {
            LogContext logContext = new LogContext {
                EngineVersion = this.Version.ToString(),
                HostId = base.Host.InstanceId.ToString(),
                HostName = base.Host.Name,
                HostVersion = base.Host.Version.ToString(),
                RunspaceId = base.InstanceId.ToString(),
                Severity = severity.ToString()
            };
            if (this.RunspaceConfiguration == null)
            {
                logContext.ShellId = Utils.DefaultPowerShellShellID;
            }
            else
            {
                logContext.ShellId = this.RunspaceConfiguration.ShellId;
            }
            MshLog.LogEngineHealthEvent(logContext, id, exception, additionalInfo);
        }

        protected override void OpenHelper(bool syncCall)
        {
            if (syncCall)
            {
                this.DoOpenHelper();
            }
            else
            {
                new Thread(new ThreadStart(this.OpenThreadProc)).Start();
            }
        }

        private void OpenThreadProc()
        {
            try
            {
                this.DoOpenHelper();
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
            }
        }

        private void ProcessImportModule(IEnumerable moduleList, bool startLifeCycleEventWritten)
        {
            foreach (object obj2 in moduleList)
            {
                string name = obj2 as string;
                if (name != null)
                {
                    this.ProcessImportModule(name, null, startLifeCycleEventWritten);
                }
                else
                {
                    ModuleSpecification requiredModule = obj2 as ModuleSpecification;
                    if (requiredModule != null)
                    {
                        if ((requiredModule.Version == null) && !requiredModule.Guid.HasValue)
                        {
                            this.ProcessImportModule(requiredModule.Name, null, startLifeCycleEventWritten);
                        }
                        else
                        {
                            Collection<PSModuleInfo> moduleIfAvailable = ModuleCmdletBase.GetModuleIfAvailable(requiredModule, this);
                            if ((moduleIfAvailable != null) && (moduleIfAvailable.Count > 0))
                            {
                                foreach (PSModuleInfo info in moduleIfAvailable)
                                {
                                    this.ProcessImportModule(requiredModule.Name, info, startLifeCycleEventWritten);
                                }
                            }
                            else
                            {
                                RunspaceOpenModuleLoadException exception = new RunspaceOpenModuleLoadException(StringUtil.Format(Modules.RequiredModuleNotFoundWrongGuidVersion, new object[] { requiredModule.Name, requiredModule.Guid, (requiredModule.Version == null) ? "0.0.0.0" : requiredModule.Version.ToString() }));
                                this.ValidateAndThrowRunspaceOpenModuleLoadException(null, null, startLifeCycleEventWritten, requiredModule.Name, exception);
                            }
                        }
                    }
                }
            }
        }

        private void ProcessImportModule(string name, PSModuleInfo moduleInfoToLoad, bool startLifeCycleEventWritten)
        {
            using (PowerShell shell = PowerShell.Create())
            {
                List<ErrorRecord> errors = new List<ErrorRecord>();
                if (!this.InitialSessionState.ThrowOnRunspaceOpenError)
                {
                    CommandInfo importModuleCommandInfo = this.GetImportModuleCommandInfo(name, ref errors);
                    if (importModuleCommandInfo != null)
                    {
                        Command command = new Command(importModuleCommandInfo);
                        if (moduleInfoToLoad != null)
                        {
                            command.Parameters.Add("ModuleInfo", moduleInfoToLoad);
                        }
                        else
                        {
                            command.Parameters.Add("Name", name);
                        }
                        command.MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);
                        shell.AddCommand(command);
                        importModuleCommandInfo = this.GetOutDefaultCommandInfo(name, ref errors);
                        if (importModuleCommandInfo != null)
                        {
                            shell.AddCommand(new Command(importModuleCommandInfo));
                        }
                    }
                }
                else
                {
                    CommandInfo commandInfo = this.GetImportModuleCommandInfo(name, ref errors);
                    if (commandInfo != null)
                    {
                        Command command2 = new Command(commandInfo);
                        if (moduleInfoToLoad != null)
                        {
                            command2.Parameters.Add("ModuleInfo", moduleInfoToLoad);
                        }
                        else
                        {
                            command2.Parameters.Add("Name", name);
                        }
                        shell.AddCommand(command2);
                    }
                }
                if (((shell.Commands != null) && (shell.Commands.Commands != null)) && (shell.Commands.Commands.Count > 0))
                {
                    shell.Runspace = this;
                    shell.Invoke();
                }
                this.ValidateAndThrowRunspaceOpenModuleLoadException(shell, errors, startLifeCycleEventWritten, name, null);
            }
        }

        public override void ResetRunspaceState()
        {
            PSInvalidOperationException exception = null;
            if (this.InitialSessionState == null)
            {
                exception = PSTraceSource.NewInvalidOperationException();
            }
            else if (base.RunspaceState != RunspaceState.Opened)
            {
                exception = PSTraceSource.NewInvalidOperationException("RunspaceStrings", "RunspaceNotInOpenedState", new object[] { base.RunspaceState });
            }
            else if (this.RunspaceAvailability != RunspaceAvailability.Available)
            {
                exception = PSTraceSource.NewInvalidOperationException("RunspaceStrings", "ConcurrentInvokeNotAllowed", new object[0]);
            }
            if (exception != null)
            {
                exception.Source = "ResetRunspaceState";
                throw exception;
            }
            this.InitialSessionState.ResetRunspaceState(base.ExecutionContext);
            this._history = new Microsoft.PowerShell.Commands.History(base.ExecutionContext);
        }

        internal override void SetApplicationPrivateData(PSPrimitiveDictionary applicationPrivateData)
        {
            this.applicationPrivateData = applicationPrivateData;
        }

        private void StopOrDisconnectAllJobs()
        {
            List<RemoteRunspace> disconnectRunspaces;
            if (this.JobRepository.Jobs.Count != 0)
            {
                disconnectRunspaces = new List<RemoteRunspace>();
                EventHandler<EventArgs> handler = null;
                using (ManualResetEvent jobsStopCompleted = new ManualResetEvent(false))
                {
                    ThrottleManager manager = new ThrottleManager();
                    if (handler == null)
                    {
                        handler = (sender, e) => jobsStopCompleted.Set();
                    }
                    manager.ThrottleComplete += handler;
                    foreach (Job job in this.JobRepository.Jobs)
                    {
                        if (job is PSRemotingJob)
                        {
                            if (!job.CanDisconnect)
                            {
                                manager.AddOperation(new StopJobOperationHelper(job));
                            }
                            else if (job.JobStateInfo.State == JobState.Running)
                            {
                                IEnumerable<RemoteRunspace> runspaces = job.GetRunspaces();
                                if (runspaces != null)
                                {
                                    disconnectRunspaces.AddRange(runspaces);
                                }
                            }
                        }
                    }
                    manager.EndSubmitOperations();
                    jobsStopCompleted.WaitOne();
                }
                this.CloseOrDisconnectAllRemoteRunspaces(() => disconnectRunspaces);
            }
        }

        private void ValidateAndThrowRunspaceOpenModuleLoadException(PowerShell pse, List<ErrorRecord> errors, bool startLifeCycleEventWritten, string moduleName, RunspaceOpenModuleLoadException exception)
        {
            if (this.InitialSessionState.ThrowOnRunspaceOpenError)
            {
                RunspaceOpenModuleLoadException exception2 = null;
                if (exception != null)
                {
                    exception2 = exception;
                }
                else if ((pse.Streams.Error.Count > 0) || (errors.Count > 0))
                {
                    ErrorRecord record;
                    Exception exception3;
                    PSDataCollection<ErrorRecord> datas = new PSDataCollection<ErrorRecord>();
                    if (errors.Count > 0)
                    {
                        record = errors[0];
                        exception3 = record.Exception;
                        foreach (ErrorRecord record2 in errors)
                        {
                            datas.Add(record2);
                        }
                    }
                    else
                    {
                        record = pse.Streams.Error[0];
                        exception3 = record.Exception;
                        foreach (ErrorRecord record3 in pse.Streams.Error)
                        {
                            datas.Add(record3);
                        }
                    }
                    runspaceInitTracer.WriteLine("Runspace open failed while loading module '{0}': First error {1}", new object[] { moduleName, exception3 });
                    exception2 = new RunspaceOpenModuleLoadException(moduleName, datas);
                }
                if (exception2 != null)
                {
                    this.LogEngineHealthEvent(exception2);
                    if (startLifeCycleEventWritten)
                    {
                        MshLog.LogEngineLifecycleEvent(this._engine.Context, EngineState.Stopped);
                    }
                    base.SetRunspaceState(RunspaceState.Broken, exception2);
                    base.RaiseRunspaceStateEvents();
                    throw exception2;
                }
            }
        }

        internal System.Management.Automation.CommandFactory CommandFactory
        {
            get
            {
                return this._commandFactory;
            }
        }

        protected override List<string> DoApplications
        {
            get
            {
                if (this._disposed)
                {
                    throw PSTraceSource.NewObjectDisposedException("runspace");
                }
                return this._engine.Context.EngineSessionState.Applications;
            }
        }

        protected override DriveManagementIntrinsics DoDrive
        {
            get
            {
                if (this._disposed)
                {
                    throw PSTraceSource.NewObjectDisposedException("runspace");
                }
                return this._engine.Context.SessionState.Drive;
            }
        }

        protected override CommandInvocationIntrinsics DoInvokeCommand
        {
            get
            {
                if (this._disposed)
                {
                    throw PSTraceSource.NewObjectDisposedException("runspace");
                }
                return this._engine.Context.EngineIntrinsics.InvokeCommand;
            }
        }

        protected override ProviderIntrinsics DoInvokeProvider
        {
            get
            {
                if (this._disposed)
                {
                    throw PSTraceSource.NewObjectDisposedException("runspace");
                }
                return this._engine.Context.EngineIntrinsics.InvokeProvider;
            }
        }

        protected override PSLanguageMode DoLanguageMode
        {
            get
            {
                if (this._disposed)
                {
                    throw PSTraceSource.NewObjectDisposedException("runspace");
                }
                return this._engine.Context.SessionState.LanguageMode;
            }
            set
            {
                if (this._disposed)
                {
                    throw PSTraceSource.NewObjectDisposedException("runspace");
                }
                this._engine.Context.SessionState.LanguageMode = value;
            }
        }

        protected override PSModuleInfo DoModule
        {
            get
            {
                if (this._disposed)
                {
                    throw PSTraceSource.NewObjectDisposedException("runspace");
                }
                return this._engine.Context.EngineSessionState.Module;
            }
        }

        protected override PathIntrinsics DoPath
        {
            get
            {
                if (this._disposed)
                {
                    throw PSTraceSource.NewObjectDisposedException("runspace");
                }
                return this._engine.Context.SessionState.Path;
            }
        }

        protected override CmdletProviderManagementIntrinsics DoProvider
        {
            get
            {
                if (this._disposed)
                {
                    throw PSTraceSource.NewObjectDisposedException("runspace");
                }
                return this._engine.Context.SessionState.Provider;
            }
        }

        protected override PSVariableIntrinsics DoPSVariable
        {
            get
            {
                if (this._disposed)
                {
                    throw PSTraceSource.NewObjectDisposedException("runspace");
                }
                return this._engine.Context.SessionState.PSVariable;
            }
        }

        protected override List<string> DoScripts
        {
            get
            {
                if (this._disposed)
                {
                    throw PSTraceSource.NewObjectDisposedException("runspace");
                }
                return this._engine.Context.EngineSessionState.Scripts;
            }
        }

        internal AutomationEngine Engine
        {
            get
            {
                return this._engine;
            }
        }

        public override PSEventManager Events
        {
            get
            {
                System.Management.Automation.ExecutionContext getExecutionContext = this.GetExecutionContext;
                if (getExecutionContext == null)
                {
                    return null;
                }
                return getExecutionContext.Events;
            }
        }

        internal override System.Management.Automation.ExecutionContext GetExecutionContext
        {
            get
            {
                if (this._engine == null)
                {
                    return null;
                }
                return this._engine.Context;
            }
        }

        internal Microsoft.PowerShell.Commands.History History
        {
            get
            {
                return this._history;
            }
        }

        internal override bool InNestedPrompt
        {
            get
            {
                System.Management.Automation.ExecutionContext getExecutionContext = this.GetExecutionContext;
                if (getExecutionContext == null)
                {
                    return false;
                }
                return getExecutionContext.InternalHost.HostInNestedPrompt();
            }
        }

        public override System.Management.Automation.JobManager JobManager
        {
            get
            {
                return this._jobManager;
            }
        }

        internal System.Management.Automation.JobRepository JobRepository
        {
            get
            {
                return this._jobRepository;
            }
        }

        internal System.Management.Automation.RunspaceRepository RunspaceRepository
        {
            get
            {
                return this._runspaceRepository;
            }
        }

        public override PSThreadOptions ThreadOptions
        {
            get
            {
                return this.createThreadOptions;
            }
            set
            {
                lock (base.SyncRoot)
                {
                    if (value != this.createThreadOptions)
                    {
                        if ((this.RunspaceStateInfo.State != RunspaceState.BeforeOpen) && (((base.ApartmentState != ApartmentState.MTA) && (base.ApartmentState != ApartmentState.Unknown)) || (value != PSThreadOptions.ReuseThread)))
                        {
                            throw new InvalidOperationException(StringUtil.Format(RunspaceStrings.InvalidThreadOptionsChange, new object[0]));
                        }
                        this.createThreadOptions = value;
                    }
                }
            }
        }
    }
}

