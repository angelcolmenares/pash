namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Language;
    using System.Reflection;
    using System.Threading;

    internal sealed class PSScriptCmdlet : PSCmdlet, IDynamicParameters, IDisposable
    {
        private MshCommandRuntime _commandRuntime;
        private bool _disposed;
        private bool _exitWasCalled;
        private readonly bool _fromScriptFile;
        private readonly FunctionContext _functionContext;
        private readonly ArrayList _input = new ArrayList();
        private readonly MutableTuple _localsTuple;
        private bool _rethrowExitException;
        private readonly bool _runOptimized;
        private readonly ScriptBlock _scriptBlock;
        private readonly bool _useLocalScope;

        internal event EventHandler DisposingEvent;

        internal event EventHandler StoppingEvent;

        public PSScriptCmdlet(ScriptBlock scriptBlock, bool useNewScope, bool fromScriptFile, System.Management.Automation.ExecutionContext context)
        {
            this._scriptBlock = scriptBlock;
            this._useLocalScope = useNewScope;
            this._fromScriptFile = fromScriptFile;
            this._runOptimized = this._scriptBlock.Compile((context._debuggingMode <= 0) && useNewScope);
            this._localsTuple = this._scriptBlock.MakeLocalsTuple(this._runOptimized);
            this._localsTuple.SetAutomaticVariable(AutomaticVariable.PSCmdlet, this, context);
            this._scriptBlock.SetPSScriptRootAndPSCommandPath(this._localsTuple, context);
            FunctionContext context2 = new FunctionContext {
                _localsTuple = this._localsTuple,
                _scriptBlock = this._scriptBlock,
                _sequencePoints = this._scriptBlock.SequencePoints,
                _executionContext = context
            };
            this._functionContext = context2;
            this._rethrowExitException = context.ScriptCommandProcessorShouldRethrowExit;
            context.ScriptCommandProcessorShouldRethrowExit = false;
        }

        protected override void BeginProcessing()
        {
            this._commandRuntime = (MshCommandRuntime) base.commandRuntime;
            this._functionContext._outputPipe = this._commandRuntime.OutputPipe;
            this.SetPreferenceVariables();
            if (this._scriptBlock.HasBeginBlock)
            {
                this.RunClause(this._runOptimized ? this._scriptBlock.BeginBlock : this._scriptBlock.UnoptimizedBeginBlock, AutomationNull.Value, this._input);
            }
        }

        public void Dispose()
        {
            if (!this._disposed)
            {
                this.DisposingEvent.SafeInvoke(this, EventArgs.Empty);
                base.commandRuntime = null;
                base.currentObjectInPipeline = null;
                this._input.Clear();
                base.InternalDispose(true);
                this._disposed = true;
            }
        }

        internal override void DoEndProcessing()
        {
            if (!this._exitWasCalled && this._scriptBlock.HasEndBlock)
            {
                this.RunClause(this._runOptimized ? this._scriptBlock.EndBlock : this._scriptBlock.UnoptimizedEndBlock, AutomationNull.Value, this._input.ToArray());
            }
        }

        internal override void DoProcessRecord()
        {
            if (!this._exitWasCalled)
            {
                this._input.Add(base.CurrentPipelineObject);
                if (this._scriptBlock.HasProcessBlock)
                {
                    PSObject dollarUnderbar = (base.CurrentPipelineObject == AutomationNull.Value) ? null : base.CurrentPipelineObject;
                    this.RunClause(this._runOptimized ? this._scriptBlock.ProcessBlock : this._scriptBlock.UnoptimizedProcessBlock, dollarUnderbar, this._input);
                    this._input.Clear();
                }
            }
        }

        private void EnterScope()
        {
            this._commandRuntime.SetVariableListsInPipe();
            if (!this._useLocalScope)
            {
                base.Context.SessionState.Internal.CurrentScope.DottedScopes.Push(this._localsTuple);
            }
        }

        private void ExitScope()
        {
            this._commandRuntime.RemoveVariableListsInPipe();
            if (!this._useLocalScope)
            {
                base.Context.SessionState.Internal.CurrentScope.DottedScopes.Pop();
            }
        }

        public object GetDynamicParameters()
        {
            this._commandRuntime = (MshCommandRuntime) base.commandRuntime;
            if (this._scriptBlock.HasDynamicParameters)
            {
                ArrayList resultList = new ArrayList();
                this._functionContext._outputPipe = new Pipe(resultList);
                this.RunClause(this._runOptimized ? this._scriptBlock.DynamicParamBlock : this._scriptBlock.UnoptimizedDynamicParamBlock, AutomationNull.Value, AutomationNull.Value);
                if (resultList.Count > 1)
                {
                    throw PSTraceSource.NewInvalidOperationException("AutomationExceptions", "DynamicParametersWrongType", new object[] { PSObject.ToStringParser(base.Context, resultList) });
                }
                if (resultList.Count != 0)
                {
                    return PSObject.Base(resultList[0]);
                }
            }
            return null;
        }

        public void PrepareForBinding(SessionStateScope scope, CommandLineParameters commandLineParameters)
        {
            if (this._useLocalScope && (scope.LocalsTuple == null))
            {
                scope.LocalsTuple = this._localsTuple;
            }
            this._localsTuple.SetAutomaticVariable(AutomaticVariable.PSBoundParameters, commandLineParameters.GetValueToBindToPSBoundParameters(), base.Context);
            this._localsTuple.SetAutomaticVariable(AutomaticVariable.MyInvocation, base.MyInvocation, base.Context);
        }

        private void RunClause(Action<FunctionContext> clause, object dollarUnderbar, object inputToProcess)
        {
            Pipe shellFunctionErrorOutputPipe = base.Context.ShellFunctionErrorOutputPipe;
            PSLanguageMode? nullable = null;
            PSLanguageMode? nullable2 = null;
            if (this._scriptBlock.LanguageMode.HasValue)
            {
                PSLanguageMode? languageMode = this._scriptBlock.LanguageMode;
                PSLanguageMode mode = base.Context.LanguageMode;
                if ((((PSLanguageMode) languageMode.GetValueOrDefault()) != mode) || !languageMode.HasValue)
                {
                    nullable = new PSLanguageMode?(base.Context.LanguageMode);
                    nullable2 = this._scriptBlock.LanguageMode;
                }
            }
            try
            {
                try
                {
                    this.EnterScope();
                    if (this._commandRuntime.ErrorMergeTo == MshCommandRuntime.MergeDataStream.Output)
                    {
                        base.Context.RedirectErrorPipe(this._commandRuntime.OutputPipe);
                    }
                    else if (this._commandRuntime.ErrorOutputPipe.IsRedirected)
                    {
                        base.Context.RedirectErrorPipe(this._commandRuntime.ErrorOutputPipe);
                    }
                    if (dollarUnderbar != AutomationNull.Value)
                    {
                        this._localsTuple.SetAutomaticVariable(AutomaticVariable.Underbar, dollarUnderbar, base.Context);
                    }
                    if (inputToProcess != AutomationNull.Value)
                    {
                        this._localsTuple.SetAutomaticVariable(AutomaticVariable.Input, inputToProcess, base.Context);
                    }
                    if (nullable2.HasValue)
                    {
                        base.Context.LanguageMode = nullable2.Value;
                    }
                    clause(this._functionContext);
                }
                catch (TargetInvocationException exception)
                {
                    throw exception.InnerException;
                }
                finally
                {
                    base.Context.RestoreErrorPipe(shellFunctionErrorOutputPipe);
                    if (nullable.HasValue)
                    {
                        base.Context.LanguageMode = nullable.Value;
                    }
                    this.ExitScope();
                }
            }
            catch (ExitException exception2)
            {
                if (!this._fromScriptFile || this._rethrowExitException)
                {
                    throw;
                }
                this._exitWasCalled = true;
                int argument = (int) exception2.Argument;
                base.Context.SetVariable(SpecialVariables.LastExitCodeVarPath, argument);
                if (argument != 0)
                {
                    this._commandRuntime.PipelineProcessor.ExecutionFailed = true;
                }
            }
            catch (TerminateException)
            {
                throw;
            }
            catch (RuntimeException)
            {
                throw;
            }
            catch (Exception exception3)
            {
                CommandProcessorBase.CheckForSevereException(exception3);
                throw;
            }
        }

        private void SetPreferenceVariables()
        {
            if (this._commandRuntime.IsDebugFlagSet)
            {
                this._localsTuple.SetPreferenceVariable(PreferenceVariable.Debug, this._commandRuntime.Debug ? ActionPreference.Inquire : ActionPreference.SilentlyContinue);
            }
            if (this._commandRuntime.IsVerboseFlagSet)
            {
                this._localsTuple.SetPreferenceVariable(PreferenceVariable.Verbose, this._commandRuntime.Verbose ? ActionPreference.Continue : ActionPreference.SilentlyContinue);
            }
            if (this._commandRuntime.IsErrorActionSet)
            {
                this._localsTuple.SetPreferenceVariable(PreferenceVariable.Error, this._commandRuntime.ErrorAction);
            }
            if (this._commandRuntime.IsWarningActionSet)
            {
                this._localsTuple.SetPreferenceVariable(PreferenceVariable.Warning, this._commandRuntime.WarningPreference);
            }
            if (this._commandRuntime.IsWhatIfFlagSet)
            {
                this._localsTuple.SetPreferenceVariable(PreferenceVariable.WhatIf, this._commandRuntime.WhatIf);
            }
            if (this._commandRuntime.IsConfirmFlagSet)
            {
                this._localsTuple.SetPreferenceVariable(PreferenceVariable.Confirm, (this._commandRuntime.Confirm != 0) ? ConfirmImpact.Low : ConfirmImpact.None);
            }
        }

        protected override void StopProcessing()
        {
            this.StoppingEvent.SafeInvoke(this, EventArgs.Empty);
            base.StopProcessing();
        }
    }
}

