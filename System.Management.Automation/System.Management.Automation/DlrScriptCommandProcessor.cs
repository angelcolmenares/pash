namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Language;
    using System.Reflection;

    internal sealed class DlrScriptCommandProcessor : ScriptCommandProcessorBase
    {
        private bool _argsBound;
        private FunctionContext _functionContext;
        private readonly ArrayList _input;
        private MutableTuple _localsTuple;
        private bool _runOptimizedCode;
        private ScriptBlock _scriptBlock;

        internal DlrScriptCommandProcessor(ExternalScriptInfo scriptInfo, ExecutionContext context, bool useNewScope, SessionStateInternal sessionState) : base(scriptInfo, context, useNewScope, sessionState)
        {
            this._input = new ArrayList();
            this.Init();
        }

        internal DlrScriptCommandProcessor(FunctionInfo functionInfo, ExecutionContext context, bool useNewScope, SessionStateInternal sessionState) : base(functionInfo, context, useNewScope, sessionState)
        {
            this._input = new ArrayList();
            this.Init();
        }

        internal DlrScriptCommandProcessor(ScriptInfo scriptInfo, ExecutionContext context, bool useNewScope, SessionStateInternal sessionState) : base(scriptInfo, context, useNewScope, sessionState)
        {
            this._input = new ArrayList();
            this.Init();
        }

        internal DlrScriptCommandProcessor(ScriptBlock scriptBlock, ExecutionContext context, bool useNewScope, CommandOrigin origin, SessionStateInternal sessionState) : base(scriptBlock, context, useNewScope, origin, sessionState)
        {
            this._input = new ArrayList();
            this.Init();
        }

        internal override void Complete()
        {
            if (!base._exitWasCalled)
            {
                if (this._scriptBlock.HasProcessBlock && base.IsPipelineInputExpected())
                {
                    this.DoProcessRecordWithInput();
                }
                if (this._scriptBlock.HasEndBlock)
                {
                    Action<FunctionContext> clause = this._runOptimizedCode ? this._scriptBlock.EndBlock : this._scriptBlock.UnoptimizedEndBlock;
                    if (base.CommandRuntime.InputPipe.ExternalReader == null)
                    {
                        if (base.IsPipelineInputExpected())
                        {
                            while (this.Read())
                            {
                                this._input.Add(base.Command.CurrentPipelineObject);
                            }
                        }
                        this.RunClause(clause, AutomationNull.Value, this._input);
                    }
                    else
                    {
                        this.RunClause(clause, AutomationNull.Value, base.CommandRuntime.InputPipe.ExternalReader.GetReadEnumerator());
                    }
                }
            }
        }

        internal override void DoBegin()
        {
            if (!base.RanBeginAlready)
            {
                base.RanBeginAlready = true;
                base.SetCurrentScopeToExecutionScope();
                CommandProcessorBase currentCommandProcessor = base.Context.CurrentCommandProcessor;
                try
                {
                    base.Context.CurrentCommandProcessor = this;
                    if (this._scriptBlock.HasBeginBlock)
                    {
                        this.RunClause(this._runOptimizedCode ? this._scriptBlock.BeginBlock : this._scriptBlock.UnoptimizedBeginBlock, AutomationNull.Value, this._input);
                    }
                }
                finally
                {
                    base.Context.CurrentCommandProcessor = currentCommandProcessor;
                    base.RestorePreviousScope();
                }
            }
        }

        private void DoProcessRecordWithInput()
        {
            Action<FunctionContext> clause = this._runOptimizedCode ? this._scriptBlock.ProcessBlock : this._scriptBlock.UnoptimizedProcessBlock;
            while (this.Read())
            {
                this._input.Add(base.Command.CurrentPipelineObject);
                base.Command.MyInvocation.PipelineIterationInfo[base.Command.MyInvocation.PipelinePosition]++;
                this.RunClause(clause, base.Command.CurrentPipelineObject, this._input);
                this._input.Clear();
            }
        }

        private void EnterScope()
        {
            if (!this._argsBound)
            {
                this._argsBound = true;
                base.ScriptParameterBinderController.BindCommandLineParameters(base.arguments);
                this._localsTuple.SetAutomaticVariable(AutomaticVariable.PSBoundParameters, base.ScriptParameterBinderController.CommandLineParameters.GetValueToBindToPSBoundParameters(), base._context);
            }
        }

        private void Init()
        {
            this._scriptBlock = base._scriptBlock;
            this._runOptimizedCode = this._scriptBlock.Compile((base._context._debuggingMode <= 0) && base.UseLocalScope);
            this._localsTuple = this._scriptBlock.MakeLocalsTuple(this._runOptimizedCode);
        }

        protected override void OnRestorePreviousScope()
        {
            if (!base.UseLocalScope)
            {
                base.CommandSessionState.CurrentScope.DottedScopes.Pop();
            }
        }

        protected override void OnSetCurrentScope()
        {
            if (!base.UseLocalScope)
            {
                base.CommandSessionState.CurrentScope.DottedScopes.Push(this._localsTuple);
            }
        }

        internal override void Prepare(IDictionary psDefaultParameterValues)
        {
            if (base.UseLocalScope)
            {
                base.CommandScope.LocalsTuple = this._localsTuple;
            }
            this._localsTuple.SetAutomaticVariable(AutomaticVariable.MyInvocation, base.Command.MyInvocation, base._context);
            this._scriptBlock.SetPSScriptRootAndPSCommandPath(this._localsTuple, base._context);
            FunctionContext context = new FunctionContext {
                _executionContext = base._context,
                _outputPipe = base.commandRuntime.OutputPipe,
                _localsTuple = this._localsTuple,
                _scriptBlock = this._scriptBlock,
                _sequencePoints = this._scriptBlock.SequencePoints
            };
            this._functionContext = context;
        }

        internal override void ProcessRecord()
        {
            if (!base._exitWasCalled)
            {
                if (!base.RanBeginAlready)
                {
                    base.RanBeginAlready = true;
                    if (this._scriptBlock.HasBeginBlock)
                    {
                        this.RunClause(this._runOptimizedCode ? this._scriptBlock.BeginBlock : this._scriptBlock.UnoptimizedBeginBlock, AutomationNull.Value, this._input);
                    }
                }
                if (this._scriptBlock.HasProcessBlock)
                {
                    if (!base.IsPipelineInputExpected())
                    {
                        this.RunClause(this._runOptimizedCode ? this._scriptBlock.ProcessBlock : this._scriptBlock.UnoptimizedProcessBlock, null, this._input);
                    }
                    else
                    {
                        this.DoProcessRecordWithInput();
                    }
                }
                else if (base.IsPipelineInputExpected() && (base.CommandRuntime.InputPipe.ExternalReader == null))
                {
                    while (this.Read())
                    {
                        this._input.Add(base.Command.CurrentPipelineObject);
                    }
                }
            }
        }

        private void RunClause(Action<FunctionContext> clause, object dollarUnderbar, object inputToProcess)
        {
            ExecutionContext.CheckStackDepth();
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
                CommandOrigin scopeOrigin = base.Context.EngineSessionState.CurrentScope.ScopeOrigin;
                try
                {
                    base.Context.EngineSessionState.CurrentScope.ScopeOrigin = base._dontUseScopeCommandOrigin ? CommandOrigin.Internal : base.Command.CommandOrigin;
                    if (nullable2.HasValue)
                    {
                        base.Context.LanguageMode = nullable2.Value;
                    }
                    this.EnterScope();
                    if (base.commandRuntime.ErrorMergeTo == MshCommandRuntime.MergeDataStream.Output)
                    {
                        base.Context.RedirectErrorPipe(base.commandRuntime.OutputPipe);
                    }
                    else if (base.commandRuntime.ErrorOutputPipe.IsRedirected)
                    {
                        base.Context.RedirectErrorPipe(base.commandRuntime.ErrorOutputPipe);
                    }
                    if (dollarUnderbar != AutomationNull.Value)
                    {
                        this._localsTuple.SetAutomaticVariable(AutomaticVariable.Underbar, dollarUnderbar, base._context);
                    }
                    if (inputToProcess != AutomationNull.Value)
                    {
                        if (inputToProcess == null)
                        {
                            inputToProcess = MshCommandRuntime.StaticEmptyArray.GetEnumerator();
                        }
                        else
                        {
                            IList list = inputToProcess as IList;
                            inputToProcess = (list != null) ? list.GetEnumerator() : LanguagePrimitives.GetEnumerator(inputToProcess);
                        }
                        this._localsTuple.SetAutomaticVariable(AutomaticVariable.Input, inputToProcess, base._context);
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
                    base.Context.EngineSessionState.CurrentScope.ScopeOrigin = scopeOrigin;
                }
            }
            catch (ExitException exception2)
            {
                if (!base.FromScriptFile || base._rethrowExitException)
                {
                    throw;
                }
                base._exitWasCalled = true;
                int argument = (int) exception2.Argument;
                base.Command.Context.SetVariable(SpecialVariables.LastExitCodeVarPath, argument);
                if (argument != 0)
                {
                    base.commandRuntime.PipelineProcessor.ExecutionFailed = true;
                }
            }
            catch (FlowControlException)
            {
                throw;
            }
            catch (RuntimeException exception3)
            {
                base.ManageScriptException(exception3);
                throw;
            }
            catch (Exception exception4)
            {
                CommandProcessorBase.CheckForSevereException(exception4);
                throw base.ManageInvocationException(exception4);
            }
        }
    }
}

