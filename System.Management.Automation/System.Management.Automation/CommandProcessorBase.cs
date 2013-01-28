namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Language;
    using System.Management.Automation.Runspaces;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal abstract class CommandProcessorBase : IDisposable
    {
        internal bool _addedToPipelineAlready;
        private SessionStateInternal _commandSessionState;
        protected ExecutionContext _context;
        protected bool _fromScriptFile;
        private Guid _pipelineActivityId;
        private SessionStateInternal _previousCommandSessionState;
        private SessionStateScope _previousScope;
        private bool _redirectShellErrorOutputPipe;
        protected bool _useLocalScope;
        private static bool alreadyFailing;
        internal Collection<CommandParameterInternal> arguments;
        private InternalCommand command;
        private System.Management.Automation.CommandInfo commandInfo;
        protected MshCommandRuntime commandRuntime;
        private bool disposed;
        private bool firstCallToRead;
        internal bool RanBeginAlready;

        internal CommandProcessorBase()
        {
            this._pipelineActivityId = Guid.Empty;
            this.arguments = new Collection<CommandParameterInternal>();
            this.firstCallToRead = true;
        }

        internal CommandProcessorBase(System.Management.Automation.CommandInfo commandInfo)
        {
            this._pipelineActivityId = Guid.Empty;
            this.arguments = new Collection<CommandParameterInternal>();
            this.firstCallToRead = true;
            if (commandInfo == null)
            {
                throw PSTraceSource.NewArgumentNullException("commandInfo");
            }
            this.commandInfo = commandInfo;
        }

        internal void AddParameter(CommandParameterInternal parameter)
        {
            this.arguments.Add(parameter);
        }

        internal static void CheckForSevereException(Exception e)
        {
            if ((e is AccessViolationException) || (e is StackOverflowException))
            {
                try
                {
                    if (!alreadyFailing)
                    {
                        alreadyFailing = true;
                        MshLog.LogCommandHealthEvent(LocalPipeline.GetExecutionContextFromTLS(), e, Severity.Critical);
                    }
                }
                finally
                {
                    WindowsErrorReporting.FailFast(e);
                }
            }
        }

        internal virtual void Complete()
        {
            this.ProcessRecord();
            try
            {
                using (this.commandRuntime.AllowThisCommandToWrite(true))
                {
                    using (ParameterBinderBase.bindingTracer.TraceScope("CALLING EndProcessing", new object[0]))
                    {
                        this.Command.DoEndProcessing();
                    }
                }
            }
            catch (Exception exception)
            {
                CheckForSevereException(exception);
                throw this.ManageInvocationException(exception);
            }
        }

        internal static CommandProcessorBase CreateGetHelpCommandProcessor(ExecutionContext context, string helpTarget, HelpCategory helpCategory)
        {
            if (context == null)
            {
                throw PSTraceSource.NewArgumentNullException("context");
            }
            if (string.IsNullOrEmpty(helpTarget))
            {
                throw PSTraceSource.NewArgumentNullException("helpTarget");
            }
            CommandProcessorBase base2 = context.CreateCommand("get-help", false);
            CommandParameterInternal parameter = CommandParameterInternal.CreateParameterWithArgument(PositionUtilities.EmptyExtent, "Name", "-Name:", PositionUtilities.EmptyExtent, helpTarget, false);
            base2.AddParameter(parameter);
            parameter = CommandParameterInternal.CreateParameterWithArgument(PositionUtilities.EmptyExtent, "Category", "-Category:", PositionUtilities.EmptyExtent, helpCategory.ToString(), false);
            base2.AddParameter(parameter);
            return base2;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    IDisposable command = this.Command as IDisposable;
                    if (command != null)
                    {
                        command.Dispose();
                    }
                }
                this.disposed = true;
            }
        }

        internal virtual void DoBegin()
        {
            if (!this.RanBeginAlready)
            {
                this.RanBeginAlready = true;
                Pipe shellFunctionErrorOutputPipe = this._context.ShellFunctionErrorOutputPipe;
                CommandProcessorBase currentCommandProcessor = this._context.CurrentCommandProcessor;
                try
                {
                    if (this.RedirectShellErrorOutputPipe || (this._context.ShellFunctionErrorOutputPipe != null))
                    {
                        this._context.ShellFunctionErrorOutputPipe = this.commandRuntime.ErrorOutputPipe;
                    }
                    this._context.CurrentCommandProcessor = this;
                    using (this.commandRuntime.AllowThisCommandToWrite(true))
                    {
                        using (ParameterBinderBase.bindingTracer.TraceScope("CALLING BeginProcessing", new object[0]))
                        {
                            this.SetCurrentScopeToExecutionScope();
                            if ((this.Context._debuggingMode > 0) && !(this.Command is PSScriptCmdlet))
                            {
                                this.Context.Debugger.CheckCommand(this.Command.MyInvocation);
                            }
                            this.Command.DoBeginProcessing();
                        }
                    }
                }
                catch (Exception exception)
                {
                    CheckForSevereException(exception);
                    throw this.ManageInvocationException(exception);
                }
                finally
                {
                    this._context.ShellFunctionErrorOutputPipe = shellFunctionErrorOutputPipe;
                    this._context.CurrentCommandProcessor = currentCommandProcessor;
                    this.RestorePreviousScope();
                }
            }
        }

        internal void DoComplete()
        {
            Pipe shellFunctionErrorOutputPipe = this._context.ShellFunctionErrorOutputPipe;
            CommandProcessorBase currentCommandProcessor = this._context.CurrentCommandProcessor;
            try
            {
                if (this.RedirectShellErrorOutputPipe || (this._context.ShellFunctionErrorOutputPipe != null))
                {
                    this._context.ShellFunctionErrorOutputPipe = this.commandRuntime.ErrorOutputPipe;
                }
                this._context.CurrentCommandProcessor = this;
                this.SetCurrentScopeToExecutionScope();
                this.Complete();
            }
            finally
            {
                this.OnRestorePreviousScope();
                this._context.ShellFunctionErrorOutputPipe = shellFunctionErrorOutputPipe;
                this._context.CurrentCommandProcessor = currentCommandProcessor;
                if (this._useLocalScope && (this.CommandScope != null))
                {
                    this._commandSessionState.RemoveScope(this.CommandScope);
                }
                if (this._previousScope != null)
                {
                    this._commandSessionState.CurrentScope = this._previousScope;
                }
                if (this._previousCommandSessionState != null)
                {
                    this.Context.EngineSessionState = this._previousCommandSessionState;
                }
            }
        }

        internal void DoExecute()
        {
            ExecutionContext.CheckStackDepth();
            CommandProcessorBase currentCommandProcessor = this._context.CurrentCommandProcessor;
            try
            {
                this.Context.CurrentCommandProcessor = this;
                this.SetCurrentScopeToExecutionScope();
                this.ProcessRecord();
            }
            finally
            {
                this.Context.CurrentCommandProcessor = currentCommandProcessor;
                this.RestorePreviousScope();
            }
        }

        internal void DoPrepare(IDictionary psDefaultParameterValues)
        {
            CommandProcessorBase currentCommandProcessor = this._context.CurrentCommandProcessor;
            try
            {
                this.Context.CurrentCommandProcessor = this;
                this.SetCurrentScopeToExecutionScope();
                this.Prepare(psDefaultParameterValues);
            }
            catch (Exception exception)
            {
                CheckForSevereException(exception);
                if (this._useLocalScope)
                {
                    this._commandSessionState.RemoveScope(this.CommandScope);
                }
                throw;
            }
            finally
            {
                this.Context.CurrentCommandProcessor = currentCommandProcessor;
                this.RestorePreviousScope();
            }
        }

        ~CommandProcessorBase()
        {
            this.Dispose(false);
        }

        internal void ForgetScriptException()
        {
            if ((this.Command != null) && (this.commandRuntime.PipelineProcessor != null))
            {
                this.commandRuntime.PipelineProcessor.ForgetFailure();
            }
        }

        internal virtual bool IsHelpRequested(out string helpTarget, out HelpCategory helpCategory)
        {
            helpTarget = null;
            helpCategory = HelpCategory.None;
            return false;
        }

        internal bool IsPipelineInputExpected()
        {
            return this.commandRuntime.IsPipelineInputExpected;
        }

        internal PipelineStoppedException ManageInvocationException(Exception e)
        {
            PipelineStoppedException exception4;
            try
            {
                if (this.Command != null)
                {
                    ProviderInvocationException innerException = e as ProviderInvocationException;
                    if (innerException != null)
                    {
                        e = new CmdletProviderInvocationException(innerException, this.Command.MyInvocation);
                    }
                    else if (((!(e is PipelineStoppedException) && !(e is CmdletInvocationException)) && (!(e is ActionPreferenceStopException) && !(e is HaltCommandException))) && (!(e is FlowControlException) && !(e is ScriptCallDepthException)))
                    {
                        RuntimeException exception2 = e as RuntimeException;
                        if ((exception2 == null) || !exception2.WasThrownFromThrowStatement)
                        {
                            e = new CmdletInvocationException(e, this.Command.MyInvocation);
                        }
                    }
                    if (this.commandRuntime.UseTransaction != 0)
                    {
                        bool flag = false;
                        for (Exception exception3 = e; exception3 != null; exception3 = exception3.InnerException)
                        {
                            if (exception3 is TimeoutException)
                            {
                                flag = true;
                                break;
                            }
                        }
                        if (flag)
                        {
                            ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(TransactionStrings.TransactionTimedOut), "TRANSACTION_TIMEOUT", ErrorCategory.InvalidOperation, e);
                            errorRecord.SetInvocationInfo(this.Command.MyInvocation);
                            e = new CmdletInvocationException(errorRecord);
                        }
                        if (this._context.TransactionManager.HasTransaction && (this._context.TransactionManager.RollbackPreference != RollbackSeverity.Never))
                        {
                            this.Context.TransactionManager.Rollback(true);
                        }
                    }
                    return (PipelineStoppedException) this.commandRuntime.ManageException(e);
                }
                exception4 = new PipelineStoppedException();
            }
            catch (Exception)
            {
                throw;
            }
            return exception4;
        }

        internal void ManageScriptException(RuntimeException e)
        {
            if ((this.Command != null) && (this.commandRuntime.PipelineProcessor != null))
            {
                this.commandRuntime.PipelineProcessor.RecordFailure(e, this.Command);
                if (!(e is PipelineStoppedException) && !e.WasThrownFromThrowStatement)
                {
                    this.commandRuntime.AppendErrorToVariables(e);
                }
            }
            throw new PipelineStoppedException();
        }

        protected virtual void OnRestorePreviousScope()
        {
        }

        protected virtual void OnSetCurrentScope()
        {
        }

        internal abstract void Prepare(IDictionary psDefaultParameterValues);
        internal abstract void ProcessRecord();
        internal virtual bool Read()
        {
            if (this.firstCallToRead)
            {
                this.firstCallToRead = false;
            }
            object obj2 = this.commandRuntime.InputPipe.Retrieve();
            if (obj2 == AutomationNull.Value)
            {
                return false;
            }
            if (this.Command.MyInvocation.PipelinePosition == 1)
            {
                this.Command.MyInvocation.PipelineIterationInfo[0]++;
            }
            this.Command.CurrentPipelineObject = LanguagePrimitives.AsPSObjectOrNull(obj2);
            return true;
        }

        internal void RestorePreviousScope()
        {
            this.OnRestorePreviousScope();
            this.Context.EngineSessionState = this._previousCommandSessionState;
            if (this._previousScope != null)
            {
                this._commandSessionState.CurrentScope = this._previousScope;
            }
        }

        internal void SetCurrentScopeToExecutionScope()
        {
            if (this._commandSessionState == null)
            {
                this._commandSessionState = this.Context.EngineSessionState;
            }
            this._previousScope = this._commandSessionState.CurrentScope;
            this._previousCommandSessionState = this.Context.EngineSessionState;
            this.Context.EngineSessionState = this._commandSessionState;
            this._commandSessionState.CurrentScope = this.CommandScope;
            this.OnSetCurrentScope();
        }

        public override string ToString()
        {
            if (this.commandInfo != null)
            {
                return this.commandInfo.ToString();
            }
            return "<NullCommandInfo>";
        }

        protected static void ValidateCompatibleLanguageMode(ScriptBlock scriptBlock, PSLanguageMode languageMode, InvocationInfo invocationInfo)
        {
            if (scriptBlock.LanguageMode.HasValue)
            {
                PSLanguageMode? nullable2 = scriptBlock.LanguageMode;
                PSLanguageMode mode = languageMode;
                if (((((PSLanguageMode) nullable2.GetValueOrDefault()) != mode) || !nullable2.HasValue) && ((languageMode == PSLanguageMode.RestrictedLanguage) || (languageMode == PSLanguageMode.ConstrainedLanguage)))
                {
                    ErrorRecord errorRecord = new ErrorRecord(new NotSupportedException(DiscoveryExceptions.DotSourceNotSupported), "DotSourceNotSupported", ErrorCategory.InvalidOperation, null);
                    errorRecord.SetInvocationInfo(invocationInfo);
                    throw new CmdletInvocationException(errorRecord);
                }
            }
        }

        internal bool AddedToPipelineAlready
        {
            get
            {
                return this._addedToPipelineAlready;
            }
            set
            {
                this._addedToPipelineAlready = value;
            }
        }

        internal InternalCommand Command
        {
            get
            {
                return this.command;
            }
            set
            {
                if (value != null)
                {
                    value.commandRuntime = this.commandRuntime;
                    if (this.command != null)
                    {
                        value.CommandInfo = this.command.CommandInfo;
                    }
                    if ((value.Context == null) && (this._context != null))
                    {
                        value.Context = this._context;
                    }
                }
                this.command = value;
            }
        }

        internal System.Management.Automation.CommandInfo CommandInfo
        {
            get
            {
                return this.commandInfo;
            }
            set
            {
                this.commandInfo = value;
            }
        }

        internal MshCommandRuntime CommandRuntime
        {
            get
            {
                return this.commandRuntime;
            }
            set
            {
                this.commandRuntime = value;
            }
        }

        protected internal SessionStateScope CommandScope { get; protected set; }

        internal SessionStateInternal CommandSessionState
        {
            get
            {
                return this._commandSessionState;
            }
            set
            {
                this._commandSessionState = value;
            }
        }

        internal ExecutionContext Context
        {
            get
            {
                return this._context;
            }
            set
            {
                this._context = value;
            }
        }

        public bool FromScriptFile
        {
            get
            {
                return this._fromScriptFile;
            }
        }

        internal Guid PipelineActivityId
        {
            get
            {
                return this._pipelineActivityId;
            }
            set
            {
                this._pipelineActivityId = value;
            }
        }

        internal bool RedirectShellErrorOutputPipe
        {
            get
            {
                return this._redirectShellErrorOutputPipe;
            }
            set
            {
                this._redirectShellErrorOutputPipe = value;
            }
        }

        internal bool UseLocalScope
        {
            get
            {
                return this._useLocalScope;
            }
            set
            {
                this._useLocalScope = value;
            }
        }
    }
}

