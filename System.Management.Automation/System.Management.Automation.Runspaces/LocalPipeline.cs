namespace System.Management.Automation.Runspaces
{
    using Microsoft.PowerShell;
    using Microsoft.PowerShell.Commands;
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Internal.Host;
    using System.Management.Automation.Sqm;
    using System.Management.Automation.Tracing;
    using System.Security;
    using System.Security.Principal;
    using System.Threading;

    internal sealed class LocalPipeline : PipelineBase
    {
        private bool _disposed;
        private long _historyIdForThisPipeline;
        private List<long> _invokeHistoryIds;
        private DateTime _pipelineStartTime;
        private PipelineStopper _stopper;
        private PipelineWriter oldExternalErrorOutput;
        private PipelineWriter oldExternalSuccessOutput;
        private bool useExternalInput;

        internal LocalPipeline(LocalPipeline pipeline) : base(pipeline)
        {
            this._historyIdForThisPipeline = -1L;
            this._invokeHistoryIds = new List<long>();
            this._stopper = new PipelineStopper(this);
            this.InitStreams();
        }

        internal LocalPipeline(System.Management.Automation.Runspaces.LocalRunspace runspace, string command, bool addToHistory, bool isNested) : base(runspace, command, addToHistory, isNested)
        {
            this._historyIdForThisPipeline = -1L;
            this._invokeHistoryIds = new List<long>();
            this._stopper = new PipelineStopper(this);
            this.InitStreams();
        }

        internal LocalPipeline(System.Management.Automation.Runspaces.LocalRunspace runspace, CommandCollection command, bool addToHistory, bool isNested, ObjectStreamBase inputStream, ObjectStreamBase outputStream, ObjectStreamBase errorStream, PSInformationalBuffers infoBuffers) : base(runspace, command, addToHistory, isNested, inputStream, outputStream, errorStream, infoBuffers)
        {
            this._historyIdForThisPipeline = -1L;
            this._invokeHistoryIds = new List<long>();
            this._stopper = new PipelineStopper(this);
            this.InitStreams();
        }

        private void AddHistoryEntry(bool skipIfLocked)
        {
            if (base.AddToHistory)
            {
                this.LocalRunspace.History.AddEntry(base.InstanceId, base.HistoryString, base.PipelineState, this._pipelineStartTime, DateTime.Now, skipIfLocked);
            }
        }

        internal void AddHistoryEntryFromAddHistoryCmdlet()
        {
            if ((this._historyIdForThisPipeline == -1L) && base.AddToHistory)
            {
                this._historyIdForThisPipeline = this.LocalRunspace.History.AddEntry(base.InstanceId, base.HistoryString, base.PipelineState, this._pipelineStartTime, DateTime.Now, false);
            }
        }

        internal void AddToInvokeHistoryEntryList(HistoryInfo entry)
        {
            this._invokeHistoryIds.Add(entry.Id);
        }

        private void ClearStreams()
        {
            if (this.LocalRunspace.ExecutionContext != null)
            {
                this.LocalRunspace.ExecutionContext.ExternalErrorOutput = this.oldExternalErrorOutput;
                this.LocalRunspace.ExecutionContext.ExternalSuccessOutput = this.oldExternalSuccessOutput;
            }
        }

        public override Pipeline Copy()
        {
            if (this._disposed)
            {
                throw PSTraceSource.NewObjectDisposedException("pipeline");
            }
            return new LocalPipeline(this);
        }

        private PipelineProcessor CreatePipelineProcessor()
        {
            PipelineProcessor processor2;
            CommandCollection commands = base.Commands;
            if ((commands == null) || (commands.Count == 0))
            {
                throw PSTraceSource.NewInvalidOperationException("RunspaceStrings", "NoCommandInPipeline", new object[0]);
            }
            PipelineProcessor processor = new PipelineProcessor {
                TopLevel = true
            };
            bool flag = false;
            try
            {
                foreach (Command command in commands)
                {
                    CommandProcessorBase base2;
                    if (command.CommandInfo == null)
                    {
                        base2 = command.CreateCommandProcessor(this.LocalRunspace.ExecutionContext, this.LocalRunspace.CommandFactory, base.AddToHistory, this.IsNested ? CommandOrigin.Internal : CommandOrigin.Runspace);
                    }
                    else
                    {
                        CmdletInfo commandInfo = (CmdletInfo) command.CommandInfo;
                        base2 = new CommandProcessor(commandInfo, this.LocalRunspace.ExecutionContext);
                        PSSQMAPI.IncrementData(commandInfo.CommandType);
                        base2.Command.CommandOriginInternal = CommandOrigin.Internal;
                        base2.Command.MyInvocation.InvocationName = commandInfo.Name;
                        if (command.Parameters != null)
                        {
                            foreach (CommandParameter parameter in command.Parameters)
                            {
                                CommandParameterInternal internal2 = CommandParameter.ToCommandParameterInternal(parameter, false);
                                base2.AddParameter(internal2);
                            }
                        }
                    }
                    base2.RedirectShellErrorOutputPipe = base.RedirectShellErrorOutputPipe;
                    processor.Add(base2);
                }
                processor2 = processor;
            }
            catch (RuntimeException)
            {
                flag = true;
                throw;
            }
            catch (Exception exception)
            {
                flag = true;
                CommandProcessorBase.CheckForSevereException(exception);
                throw new RuntimeException(PipelineStrings.CannotCreatePipeline, exception);
            }
            finally
            {
                if (flag)
                {
                    base.SetHadErrors(true);
                    processor.Dispose();
                }
            }
            return processor2;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!this._disposed)
                {
                    this._disposed = true;
                    if (disposing)
                    {
                        this.Stop();
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        internal static System.Management.Automation.ExecutionContext GetExecutionContextFromTLS()
        {
            Runspace defaultRunspace = Runspace.DefaultRunspace;
            if (defaultRunspace == null)
            {
                return null;
            }
            return defaultRunspace.ExecutionContext;
        }

        protected override void ImplementStop(bool syncCall)
        {
            if (syncCall)
            {
                this.StopHelper();
            }
            else
            {
                new Thread(new ThreadStart(this.StopThreadProc)).Start();
            }
        }

        private void InitStreams()
        {
            if (this.LocalRunspace.ExecutionContext != null)
            {
                this.oldExternalErrorOutput = this.LocalRunspace.ExecutionContext.ExternalErrorOutput;
                this.oldExternalSuccessOutput = this.LocalRunspace.ExecutionContext.ExternalSuccessOutput;
                this.LocalRunspace.ExecutionContext.ExternalErrorOutput = base.ErrorStream.ObjectWriter;
                this.LocalRunspace.ExecutionContext.ExternalSuccessOutput = base.OutputStream.ObjectWriter;
            }
        }

        private void InvokeHelper()
        {
            PipelineProcessor item = null;
            try
            {
                base.RaisePipelineStateEvents();
                this.RecordPipelineStartTime();
                try
                {
                    item = this.CreatePipelineProcessor();
                }
                catch (Exception exception)
                {
                    if (base.SetPipelineSessionState)
                    {
                        base.SetHadErrors(true);
                        this.Runspace.ExecutionContext.AppendDollarError(exception);
                    }
                    throw;
                }
                if (this.useExternalInput)
                {
                    item.ExternalInput = base.InputStream.ObjectReader;
                }
                item.ExternalSuccessOutput = base.OutputStream.ObjectWriter;
                item.ExternalErrorOutput = base.ErrorStream.ObjectWriter;
                if (!this.IsChild)
                {
                    this.LocalRunspace.ExecutionContext.InternalHost.InternalUI.SetInformationalMessageBuffers(base.InformationalBuffers);
                }
                bool questionMarkVariableValue = true;
                bool exceptionHandlerInEnclosingStatementBlock = this.LocalRunspace.ExecutionContext.ExceptionHandlerInEnclosingStatementBlock;
                this.LocalRunspace.ExecutionContext.ExceptionHandlerInEnclosingStatementBlock = false;
                try
                {
                    this._stopper.Push(item);
                    if (!base.AddToHistory)
                    {
                        questionMarkVariableValue = this.LocalRunspace.ExecutionContext.QuestionMarkVariableValue;
                        this.LocalRunspace.ExecutionContext.IgnoreScriptDebug = true;
                    }
                    else
                    {
                        this.LocalRunspace.ExecutionContext.IgnoreScriptDebug = false;
                    }
                    if (!this.IsNested && !base.IsPulsePipeline)
                    {
                        this.LocalRunspace.ExecutionContext.ResetRedirection();
                    }
                    try
                    {
                        item.Execute();
                        base.SetHadErrors(item.ExecutionFailed);
                    }
                    catch (ExitException exception2)
                    {
                        base.SetHadErrors(item.ExecutionFailed);
                        int newValue = 1;
                        if (this.IsNested)
                        {
                            try
                            {
                                newValue = (int) exception2.Argument;
                                this.LocalRunspace.ExecutionContext.SetVariable(SpecialVariables.LastExitCodeVarPath, newValue);
                                return;
                            }
                            finally
                            {
                                try
                                {
                                    this.LocalRunspace.ExecutionContext.EngineHostInterface.ExitNestedPrompt();
                                }
                                catch (ExitNestedPromptException)
                                {
                                }
                            }
                        }
                        try
                        {
                            newValue = (int) exception2.Argument;
                        }
                        finally
                        {
                            this.LocalRunspace.ExecutionContext.EngineHostInterface.SetShouldExit(newValue);
                        }
                    }
                    catch (ExitNestedPromptException)
                    {
                    }
                    catch (FlowControlException)
                    {
                    }
                    catch (Exception)
                    {
                        base.SetHadErrors(true);
                        throw;
                    }
                }
                finally
                {
                    if ((item != null) && (item.Commands != null))
                    {
                        for (int i = 0; i < item.Commands.Count; i++)
                        {
                            CommandProcessorBase base2 = item.Commands[i];
                            EtwActivity.SetActivityId(base2.PipelineActivityId);
                            MshLog.LogCommandLifecycleEvent(base2.Context, CommandState.Terminated, base2.Command.MyInvocation);
                        }
                    }
                    PSLocalEventManager events = this.LocalRunspace.Events as PSLocalEventManager;
                    if (events != null)
                    {
                        events.ProcessPendingActions();
                    }
                    this.LocalRunspace.ExecutionContext.ExceptionHandlerInEnclosingStatementBlock = exceptionHandlerInEnclosingStatementBlock;
                    if (!this.IsChild)
                    {
                        this.LocalRunspace.ExecutionContext.InternalHost.InternalUI.SetInformationalMessageBuffers(null);
                    }
                    this._stopper.Pop(false);
                    if (!base.AddToHistory)
                    {
                        this.LocalRunspace.ExecutionContext.QuestionMarkVariableValue = questionMarkVariableValue;
                    }
                }
            }
            catch (FlowControlException)
            {
            }
            finally
            {
                if (item != null)
                {
                    item.Dispose();
                    item = null;
                }
            }
        }

        private void InvokeThreadProc()
        {
            bool flag = false;
            Runspace defaultRunspace = Runspace.DefaultRunspace;
            try
            {
                WindowsImpersonationContext context = null;
                try
                {
                    if ((base.InvocationSettings != null) && base.InvocationSettings.FlowImpersonationPolicy)
                    {
                        context = new WindowsIdentity(base.InvocationSettings.WindowsIdentityToImpersonate.Token).Impersonate();
                    }
                    if ((base.InvocationSettings != null) && (base.InvocationSettings.Host != null))
                    {
                        InternalHost host = base.InvocationSettings.Host as InternalHost;
                        if (host != null)
                        {
                            this.LocalRunspace.ExecutionContext.InternalHost.SetHostRef(host.ExternalHost);
                        }
                        else
                        {
                            this.LocalRunspace.ExecutionContext.InternalHost.SetHostRef(base.InvocationSettings.Host);
                        }
                    }
                    if (this.LocalRunspace.ExecutionContext.InternalHost.ExternalHost.ShouldSetThreadUILanguageToZero)
                    {
                        NativeCultureResolver.SetThreadUILanguage(0);
                    }
                    Runspace.DefaultRunspace = this.LocalRunspace;
                    this.InvokeHelper();
                    base.SetPipelineState(PipelineState.Completed);
                }
                finally
                {
                    if (context != null)
                    {
                        try
                        {
                            context.Undo();
                            context.Dispose();
                            context = null;
                        }
                        catch (SecurityException)
                        {
                        }
                    }
                }
            }
            catch (PipelineStoppedException exception)
            {
                base.SetPipelineState(PipelineState.Stopped, exception);
            }
            catch (RuntimeException exception2)
            {
                flag = exception2 is IncompleteParseException;
                base.SetPipelineState(PipelineState.Failed, exception2);
                base.SetHadErrors(true);
            }
            catch (ScriptCallDepthException exception3)
            {
                base.SetPipelineState(PipelineState.Failed, exception3);
                base.SetHadErrors(true);
            }
            catch (SecurityException exception4)
            {
                base.SetPipelineState(PipelineState.Failed, exception4);
                base.SetHadErrors(true);
            }
            catch (ThreadAbortException exception5)
            {
                base.SetPipelineState(PipelineState.Failed, exception5);
                base.SetHadErrors(true);
            }
            catch (HaltCommandException)
            {
                base.SetPipelineState(PipelineState.Completed);
            }
            finally
            {
                if (((base.InvocationSettings != null) && (base.InvocationSettings.Host != null)) && this.LocalRunspace.ExecutionContext.InternalHost.IsHostRefSet)
                {
                    this.LocalRunspace.ExecutionContext.InternalHost.RevertHostRef();
                }
                Runspace.DefaultRunspace = defaultRunspace;
                if (!flag)
                {
                    try
                    {
                        bool inBreakpoint = this.LocalRunspace.ExecutionContext.Debugger.InBreakpoint;
                        if (this._historyIdForThisPipeline == -1L)
                        {
                            this.AddHistoryEntry(inBreakpoint);
                        }
                        else
                        {
                            this.UpdateHistoryEntryAddedByAddHistoryCmdlet(inBreakpoint);
                        }
                    }
                    catch (TerminateException)
                    {
                    }
                }
                if (base.OutputStream.IsOpen && !this.IsChild)
                {
                    try
                    {
                        base.OutputStream.Close();
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                }
                if (base.ErrorStream.IsOpen && !this.IsChild)
                {
                    try
                    {
                        base.ErrorStream.Close();
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                }
                if (base.InputStream.IsOpen && !this.IsChild)
                {
                    try
                    {
                        base.InputStream.Close();
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                }
                this.ClearStreams();
                this.LocalRunspace.RemoveFromRunningPipelineList(this);
                if (!base.SyncInvokeCall)
                {
                    base.RaisePipelineStateEvents();
                }
            }
        }

        internal bool PresentInInvokeHistoryEntryList(HistoryInfo entry)
        {
            return this._invokeHistoryIds.Contains(entry.Id);
        }

        internal static int ReadRegistryInt (string policyValueName, int defaultValue)
		{
			if (PowerShellConfiguration.IsWindows) {
				RegistryKey key;
				try {
					key = Registry.LocalMachine.OpenSubKey (Utils.GetRegistryConfigurationPrefix ());
				} catch (SecurityException) {
					return defaultValue;
				}
				if (key != null) {
					object obj2;
					try {
						obj2 = key.GetValue (policyValueName);
					} catch (SecurityException) {
						return defaultValue;
					}
					if (obj2 is int) {
						return (int)obj2;
					}
				}
				return defaultValue;
			}
			return PowerShellConfiguration.GetPolicyValue (policyValueName, defaultValue);
        }

        private void RecordPipelineStartTime()
        {
            this._pipelineStartTime = DateTime.Now;
        }

        internal void RemoveFromInvokeHistoryEntryList(HistoryInfo entry)
        {
            this._invokeHistoryIds.Remove(entry.Id);
        }

        internal override void SetHistoryString(string historyString)
        {
            base.HistoryString = historyString;
        }

        private void SetupInvokeThread(Thread invokeThread, bool changeName)
        {
            base.NestedPipelineExecutionThread = invokeThread;
            invokeThread.CurrentCulture = this.LocalRunspace.ExecutionContext.EngineHostInterface.CurrentCulture;
            invokeThread.CurrentUICulture = this.LocalRunspace.ExecutionContext.EngineHostInterface.CurrentUICulture;
            if ((invokeThread.Name == null) && changeName)
            {
                invokeThread.Name = "Pipeline Execution Thread";
            }
        }

        protected override void StartPipelineExecution()
        {
            Thread thread;
            ApartmentState apartmentState;
            if (this._disposed)
            {
                throw PSTraceSource.NewObjectDisposedException("pipeline");
            }
            this.useExternalInput = base.InputStream.IsOpen || (base.InputStream.Count > 0);
			switch ((this.IsNested ? PSThreadOptions.UseCurrentThread : this.LocalRunspace.ThreadOptions))
            {
                case PSThreadOptions.Default:
                case PSThreadOptions.UseNewThread:
                    thread = new Thread(new ThreadStart(this.InvokeThreadProc), MaxStack);
                    this.SetupInvokeThread(thread, true);
                    if ((base.InvocationSettings == null) || (base.InvocationSettings.ApartmentState == ApartmentState.Unknown))
                    {
                        apartmentState = this.LocalRunspace.ApartmentState;
                        break;
                    }
                    apartmentState = base.InvocationSettings.ApartmentState;
                    break;

                case PSThreadOptions.ReuseThread:
                    if (!this.IsNested)
                    {
                        PipelineThread pipelineThread = this.LocalRunspace.GetPipelineThread();
                        this.SetupInvokeThread(pipelineThread.Worker, true);
                        pipelineThread.Start(new ThreadStart(this.InvokeThreadProc));
                        return;
                    }
                    this.SetupInvokeThread(Thread.CurrentThread, true);
                    this.InvokeThreadProc();
                    return;

                case PSThreadOptions.UseCurrentThread:
                {
                    Thread nestedPipelineExecutionThread = base.NestedPipelineExecutionThread;
                    CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
                    CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;
                    try
                    {
                        this.SetupInvokeThread(Thread.CurrentThread, false);
                        this.InvokeThreadProc();
                    }
                    finally
                    {
                        base.NestedPipelineExecutionThread = nestedPipelineExecutionThread;
                        Thread.CurrentThread.CurrentCulture = currentCulture;
                        Thread.CurrentThread.CurrentUICulture = currentUICulture;
                    }
                    return;
                }
                default:
                    return;
            }
            if (apartmentState != ApartmentState.Unknown)
            {
                thread.SetApartmentState(apartmentState);
            }
            thread.Start();
        }

        private void StopHelper()
        {
            this.LocalRunspace.StopNestedPipelines(this);
            if (base.InputStream.IsOpen)
            {
                try
                {
                    base.InputStream.Close();
                }
                catch (ObjectDisposedException)
                {
                }
            }
            this._stopper.Stop();
            base.PipelineFinishedEvent.WaitOne();
        }

        private void StopThreadProc()
        {
            this.StopHelper();
        }

        internal void UpdateHistoryEntryAddedByAddHistoryCmdlet(bool skipIfLocked)
        {
            if (base.AddToHistory && (this._historyIdForThisPipeline != -1L))
            {
                this.LocalRunspace.History.UpdateEntry(this._historyIdForThisPipeline, base.PipelineState, DateTime.Now, skipIfLocked);
            }
        }

        internal bool IsStopping
        {
            get
            {
                return this._stopper.IsStopping;
            }
        }

        private System.Management.Automation.Runspaces.LocalRunspace LocalRunspace
        {
            get
            {
                return (System.Management.Automation.Runspaces.LocalRunspace) this.Runspace;
            }
        }

        internal static int MaxStack
        {
            get
            {
                int num = ReadRegistryInt("PipelineMaxStackSizeMB", 10);
                if (num < 10)
                {
                    num = 10;
                }
                else if (num > 100)
                {
                    num = 100;
                }
                return (num * 0xf4240);
            }
        }

        internal PipelineStopper Stopper
        {
            get
            {
                return this._stopper;
            }
        }
    }
}

