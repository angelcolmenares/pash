namespace System.Management.Automation.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Tracing;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;

    internal class PipelineProcessor : IDisposable
    {
        private List<CommandProcessorBase> _commands = new List<CommandProcessorBase>();
        private bool _linkedErrorOutput;
        private bool _linkedSuccessOutput;
        internal InternalCommand _permittedToWrite;
        internal Thread _permittedToWriteThread;
        internal bool _permittedToWriteToPipeline;
        private List<PipelineProcessor> _redirectionPipes;
        private bool disposed;
        private bool executionFailed;
        private SessionStateScope executionScope;
        private bool executionStarted;
        private PipelineWriter externalErrorOutput;
        private PipelineReader<object> externalInputPipe;
        private PipelineWriter externalSuccessOutput;
        private Exception firstTerminatingError;
        private System.Management.Automation.Runspaces.LocalPipeline localPipeline;
        private List<string> logBuffer = new List<string>();
        internal System.Security.SecurityContext SecurityContext = System.Security.SecurityContext.Capture();
        private bool stopping;
        private object StopReasonLock = new object();
        private bool terminatingErrorLogged;
        private bool topLevel;

        internal int Add(CommandProcessorBase commandProcessor)
        {
            commandProcessor.CommandRuntime.PipelineProcessor = this;
            return this.AddCommand(commandProcessor, this._commands.Count, false);
        }

        internal int AddCommand(CommandProcessorBase commandProcessor, int readFromCommand, bool readErrorQueue)
        {
            if (commandProcessor == null)
            {
                throw PSTraceSource.NewArgumentNullException("commandProcessor");
            }
            if (this._commands == null)
            {
                throw PSTraceSource.NewInvalidOperationException();
            }
            if (this.disposed)
            {
                throw PSTraceSource.NewObjectDisposedException("PipelineProcessor");
            }
            if (this.executionStarted)
            {
                throw PSTraceSource.NewInvalidOperationException("PipelineStrings", "ExecutionAlreadyStarted", new object[0]);
            }
            if (commandProcessor.AddedToPipelineAlready)
            {
                throw PSTraceSource.NewInvalidOperationException("PipelineStrings", "CommandProcessorAlreadyUsed", new object[0]);
            }
            if (this._commands.Count == 0)
            {
                if (readFromCommand != 0)
                {
                    throw PSTraceSource.NewArgumentException("readFromCommand", "PipelineStrings", "FirstCommandCannotHaveInput", new object[0]);
                }
                commandProcessor.AddedToPipelineAlready = true;
            }
            else
            {
                if ((readFromCommand > this._commands.Count) || (readFromCommand <= 0))
                {
                    throw PSTraceSource.NewArgumentException("readFromCommand", "PipelineStrings", "InvalidCommandNumber", new object[0]);
                }
                CommandProcessorBase base2 = this._commands[readFromCommand - 1];
                if ((base2 == null) || (base2.CommandRuntime == null))
                {
                    throw PSTraceSource.NewInvalidOperationException();
                }
                Pipe pipe = readErrorQueue ? base2.CommandRuntime.ErrorOutputPipe : base2.CommandRuntime.OutputPipe;
                if (pipe == null)
                {
                    throw PSTraceSource.NewInvalidOperationException();
                }
                if (pipe.DownstreamCmdlet != null)
                {
                    throw PSTraceSource.NewInvalidOperationException("PipelineStrings", "PipeAlreadyTaken", new object[0]);
                }
                commandProcessor.AddedToPipelineAlready = true;
                commandProcessor.CommandRuntime.InputPipe = pipe;
                pipe.DownstreamCmdlet = commandProcessor;
                if (commandProcessor.CommandRuntime.MergeUnclaimedPreviousErrorResults)
                {
                    for (int i = 0; i < this._commands.Count; i++)
                    {
                        base2 = this._commands[i];
                        if ((base2 == null) || (base2.CommandRuntime == null))
                        {
                            throw PSTraceSource.NewInvalidOperationException();
                        }
                        if ((base2.CommandRuntime.ErrorOutputPipe.DownstreamCmdlet == null) && (base2.CommandRuntime.ErrorOutputPipe.ExternalWriter == null))
                        {
                            base2.CommandRuntime.ErrorOutputPipe = pipe;
                        }
                    }
                }
            }
            this._commands.Add(commandProcessor);
            commandProcessor.CommandRuntime.PipelineProcessor = this;
            return this._commands.Count;
        }

        internal void AddRedirectionPipe(PipelineProcessor pipelineProcessor)
        {
            if (pipelineProcessor == null)
            {
                throw PSTraceSource.NewArgumentNullException("pipelineProcessor");
            }
            if (this._redirectionPipes == null)
            {
                this._redirectionPipes = new List<PipelineProcessor>();
            }
            this._redirectionPipes.Add(pipelineProcessor);
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
                    this.DisposeCommands();
                    this.localPipeline = null;
                    this.externalSuccessOutput = null;
                    this.externalErrorOutput = null;
                    this.executionScope = null;
                    this.SecurityContext.Dispose();
                    this.SecurityContext = null;
                    this.logBuffer = null;
                }
                this.disposed = true;
            }
        }

        private void DisposeCommands()
        {
            this.stopping = true;
            this.FlushLog();
            if (this._commands != null)
            {
                for (int i = 0; i < this._commands.Count; i++)
                {
                    CommandProcessorBase base2 = this._commands[i];
                    if (base2 != null)
                    {
                        try
                        {
                            base2.CommandRuntime.RemoveVariableListsInPipe();
                            base2.Dispose();
                        }
                        catch (Exception exception)
                        {
                            CommandProcessorBase.CheckForSevereException(exception);
                            InvocationInfo myInvocation = null;
                            if (base2.Command != null)
                            {
                                myInvocation = base2.Command.MyInvocation;
                            }
                            ProviderInvocationException innerException = exception as ProviderInvocationException;
                            if (innerException != null)
                            {
                                exception = new CmdletProviderInvocationException(innerException, myInvocation);
                            }
                            else
                            {
                                exception = new CmdletInvocationException(exception, myInvocation);
                                MshLog.LogCommandHealthEvent(base2.Command.Context, exception, Severity.Warning);
                            }
                            this.RecordFailure(exception, base2.Command);
                        }
                    }
                }
            }
            this._commands = null;
            if (this._redirectionPipes != null)
            {
                foreach (PipelineProcessor processor in this._redirectionPipes)
                {
                    try
                    {
                        if (processor != null)
                        {
                            processor.Dispose();
                        }
                    }
                    catch (Exception exception3)
                    {
                        CommandProcessorBase.CheckForSevereException(exception3);
                    }
                }
            }
            this._redirectionPipes = null;
        }

        internal Array DoComplete()
        {
            if (this.Stopping)
            {
                throw new PipelineStoppedException();
            }
            if (!this.executionStarted)
            {
                throw PSTraceSource.NewInvalidOperationException("PipelineStrings", "PipelineNotStarted", new object[0]);
            }
            Exception firstTerminatingError = null;
            try
            {
                this.DoCompleteCore(null);
                Hashtable errorResults = new Hashtable();
                return this.RetrieveResults(errorResults);
            }
            catch (RuntimeException exception2)
            {
                if (this.firstTerminatingError != null)
                {
                    firstTerminatingError = this.firstTerminatingError;
                }
                else
                {
                    firstTerminatingError = exception2;
                }
                this.LogExecutionException(firstTerminatingError);
            }
            catch (InvalidComObjectException exception3)
            {
                if (this.firstTerminatingError != null)
                {
                    firstTerminatingError = this.firstTerminatingError;
                }
                else
                {
                    firstTerminatingError = new RuntimeException(StringUtil.Format(ParserStrings.InvalidComObjectException, exception3.Message), exception3);
                    ((RuntimeException) firstTerminatingError).SetErrorId("InvalidComObjectException");
                }
                this.LogExecutionException(firstTerminatingError);
            }
            finally
            {
                this.DisposeCommands();
            }
            RuntimeException.LockStackTrace(firstTerminatingError);
            throw firstTerminatingError;
        }

        private void DoCompleteCore(CommandProcessorBase commandRequestingUpstreamCommandsToStop)
        {
            for (int i = 0; i < this._commands.Count; i++)
            {
                CommandProcessorBase objB = this._commands[i];
                if (objB == null)
                {
                    throw PSTraceSource.NewInvalidOperationException();
                }
                if (object.ReferenceEquals(commandRequestingUpstreamCommandsToStop, objB))
                {
                    commandRequestingUpstreamCommandsToStop = null;
                }
                else if (commandRequestingUpstreamCommandsToStop == null)
                {
                    try
                    {
                        objB.DoComplete();
                    }
                    catch (PipelineStoppedException)
                    {
                        StopUpstreamCommandsException firstTerminatingError = this.firstTerminatingError as StopUpstreamCommandsException;
                        if (firstTerminatingError == null)
                        {
                            throw;
                        }
                        this.firstTerminatingError = null;
                        commandRequestingUpstreamCommandsToStop = firstTerminatingError.RequestingCommandProcessor;
                    }
                    EtwActivity.SetActivityId(objB.PipelineActivityId);
                    MshLog.LogCommandLifecycleEvent(objB.Command.Context, CommandState.Stopped, objB.Command.MyInvocation);
                }
            }
            if (this.firstTerminatingError != null)
            {
                this.LogExecutionException(this.firstTerminatingError);
                throw this.firstTerminatingError;
            }
        }

        private Array DoStepItems(object input, Hashtable errorResults, bool enumerate)
        {
            Array array;
            if (this.Stopping)
            {
                throw new PipelineStoppedException();
            }
            try
            {
                this.Start(true);
                this.Inject(input, enumerate);
                if (this.firstTerminatingError != null)
                {
                    throw this.firstTerminatingError;
                }
                array = this.RetrieveResults(errorResults);
            }
            catch (PipelineStoppedException)
            {
                this.DisposeCommands();
                if (this.firstTerminatingError != null)
                {
                    throw this.firstTerminatingError;
                }
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                this.DisposeCommands();
                throw;
            }
            return array;
        }

        internal Array Execute()
        {
            return this.Execute(null);
        }

        internal Array Execute(Array input)
        {
            return this.SynchronousExecute(input, null);
        }

        ~PipelineProcessor()
        {
            this.Dispose(false);
        }

        internal void FlushLog()
        {
            if (((this._commands != null) && (this._commands.Count > 0)) && (this.logBuffer.Count != 0))
            {
                MshLog.LogPipelineExecutionDetailEvent(this._commands[0].Command.Context, this.logBuffer, this._commands[0].Command.MyInvocation);
            }
        }

        internal void ForgetFailure()
        {
            this.firstTerminatingError = null;
        }

        private string GetCommand(Exception exception)
        {
            IContainsErrorRecord record = exception as IContainsErrorRecord;
            if ((record != null) && (record.ErrorRecord != null))
            {
                return this.GetCommand(record.ErrorRecord.InvocationInfo);
            }
            return "";
        }

        private string GetCommand(InvocationInfo invocationInfo)
        {
            if ((invocationInfo != null) && (invocationInfo.MyCommand != null))
            {
                return invocationInfo.MyCommand.Name;
            }
            return "";
        }

        private void Inject(object input, bool enumerate)
        {
            CommandProcessorBase base2 = this._commands[0];
            if ((base2 == null) || (base2.CommandRuntime == null))
            {
                throw PSTraceSource.NewInvalidOperationException("PipelineStrings", "PipelineExecuteRequiresAtLeastOneCommand", new object[0]);
            }
            if (input != AutomationNull.Value)
            {
                if (enumerate)
                {
                    IEnumerator enumeratorToProcess = LanguagePrimitives.GetEnumerator(input);
                    if (enumeratorToProcess != null)
                    {
                        base2.CommandRuntime.InputPipe = new Pipe(enumeratorToProcess);
                    }
                    else
                    {
                        base2.CommandRuntime.InputPipe.Add(input);
                    }
                }
                else
                {
                    base2.CommandRuntime.InputPipe.Add(input);
                }
            }
            base2.DoExecute();
        }

        internal void LinkPipelineErrorOutput(Pipe pipeToUse)
        {
            for (int i = 0; i < this._commands.Count; i++)
            {
                CommandProcessorBase base2 = this._commands[i];
                if ((base2 == null) || (base2.CommandRuntime == null))
                {
                    throw PSTraceSource.NewInvalidOperationException();
                }
                Pipe errorOutputPipe = base2.CommandRuntime.ErrorOutputPipe;
                if (base2.CommandRuntime.ErrorOutputPipe.DownstreamCmdlet == null)
                {
                    base2.CommandRuntime.ErrorOutputPipe = pipeToUse;
                }
            }
            this._linkedErrorOutput = true;
        }

        internal void LinkPipelineSuccessOutput(Pipe pipeToUse)
        {
            CommandProcessorBase base2 = this._commands[this._commands.Count - 1];
            if ((base2 == null) || (base2.CommandRuntime == null))
            {
                throw PSTraceSource.NewInvalidOperationException();
            }
            base2.CommandRuntime.OutputPipe = pipeToUse;
            this._linkedSuccessOutput = true;
        }

        internal void LogExecutionError(InvocationInfo invocationInfo, ErrorRecord errorRecord)
        {
            if (errorRecord != null)
            {
                string item = StringUtil.Format(PipelineStrings.PipelineExecutionNonTerminatingError, this.GetCommand(invocationInfo), errorRecord.ToString());
                this.logBuffer.Add(item);
            }
        }

        internal void LogExecutionException(Exception exception)
        {
            this.executionFailed = true;
            if (!this.terminatingErrorLogged)
            {
                this.terminatingErrorLogged = true;
                if ((exception != null) && this.NeedToLog())
                {
                    string item = StringUtil.Format(PipelineStrings.PipelineExecutionTerminatingError, this.GetCommand(exception), exception.Message);
                    this.logBuffer.Add(item);
                }
            }
        }

        internal void LogExecutionInfo(InvocationInfo invocationInfo, string text)
        {
            string item = StringUtil.Format(PipelineStrings.PipelineExecutionInformation, this.GetCommand(invocationInfo), text);
            this.logBuffer.Add(item);
        }

        internal void LogExecutionParameterBinding(InvocationInfo invocationInfo, string parameterName, string parameterValue)
        {
            string item = StringUtil.Format(PipelineStrings.PipelineExecutionParameterBinding, new object[] { this.GetCommand(invocationInfo), parameterName, parameterValue });
            this.logBuffer.Add(item);
        }

        private bool NeedToLog()
        {
            if (this._commands != null)
            {
                foreach (CommandProcessorBase base2 in this._commands)
                {
                    MshCommandRuntime commandRuntime = base2.Command.commandRuntime as MshCommandRuntime;
                    if ((commandRuntime != null) && commandRuntime.LogPipelineExecutionDetail)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool RecordFailure(Exception e, InternalCommand command)
        {
            bool stopping = false;
            lock (this.StopReasonLock)
            {
                if (this.firstTerminatingError == null)
                {
                    RuntimeException.LockStackTrace(e);
                    this.firstTerminatingError = e;
                }
                else if ((!(this.firstTerminatingError is PipelineStoppedException) && (command != null)) && (command.Context != null))
                {
                    Exception innerException = e;
                    while (((innerException is TargetInvocationException) || (innerException is CmdletInvocationException)) && (innerException.InnerException != null))
                    {
                        innerException = innerException.InnerException;
                    }
                    if (!(innerException is PipelineStoppedException))
                    {
                        InvalidOperationException exception = new InvalidOperationException(StringUtil.Format(PipelineStrings.SecondFailure, new object[] { this.firstTerminatingError.GetType().Name, this.firstTerminatingError.StackTrace, innerException.GetType().Name, innerException.StackTrace }), innerException);
                        MshLog.LogCommandHealthEvent(command.Context, exception, Severity.Warning);
                    }
                }
                stopping = this.stopping;
                this.stopping = true;
            }
            return !stopping;
        }

        private Array RetrieveResults(Hashtable errorResults)
        {
            if (!this._linkedErrorOutput)
            {
                for (int i = 0; i < this._commands.Count; i++)
                {
                    CommandProcessorBase base2 = this._commands[i];
                    if ((base2 == null) || (base2.CommandRuntime == null))
                    {
                        throw PSTraceSource.NewInvalidOperationException();
                    }
                    Pipe errorOutputPipe = base2.CommandRuntime.ErrorOutputPipe;
                    if ((errorOutputPipe.DownstreamCmdlet == null) && !errorOutputPipe.Empty)
                    {
                        if (errorResults != null)
                        {
                            errorResults.Add(i + 1, errorOutputPipe.ToArray());
                        }
                        errorOutputPipe.Clear();
                    }
                }
            }
            if (this._linkedSuccessOutput)
            {
                return MshCommandRuntime.StaticEmptyArray;
            }
            CommandProcessorBase base3 = this._commands[this._commands.Count - 1];
            if ((base3 == null) || (base3.CommandRuntime == null))
            {
                throw PSTraceSource.NewInvalidOperationException();
            }
            Array resultsAsArray = base3.CommandRuntime.GetResultsAsArray();
            base3.CommandRuntime.OutputPipe.Clear();
            if (resultsAsArray == null)
            {
                return MshCommandRuntime.StaticEmptyArray;
            }
            return resultsAsArray;
        }

        private void SetExternalErrorOutput()
        {
            if (this.ExternalErrorOutput != null)
            {
                for (int i = 0; i < this._commands.Count; i++)
                {
                    CommandProcessorBase base2 = this._commands[i];
                    Pipe errorOutputPipe = base2.CommandRuntime.ErrorOutputPipe;
                    if (!errorOutputPipe.IsRedirected)
                    {
                        errorOutputPipe.ExternalWriter = this.ExternalErrorOutput;
                    }
                }
            }
        }

        private void SetupOutErrorVariable()
        {
            for (int i = 0; i < this._commands.Count; i++)
            {
                CommandProcessorBase base2 = this._commands[i];
                if ((base2 == null) || (base2.CommandRuntime == null))
                {
                    throw PSTraceSource.NewInvalidOperationException();
                }
                base2.CommandRuntime.SetupOutVariable();
                base2.CommandRuntime.SetupErrorVariable();
                base2.CommandRuntime.SetupWarningVariable();
            }
        }

        private void Start(bool incomingStream)
        {
            if (this.disposed)
            {
                throw PSTraceSource.NewObjectDisposedException("PipelineProcessor");
            }
            if (this.Stopping)
            {
                throw new PipelineStoppedException();
            }
            if (!this.executionStarted)
            {
                if ((this._commands == null) || (this._commands.Count == 0))
                {
                    throw PSTraceSource.NewInvalidOperationException("PipelineStrings", "PipelineExecuteRequiresAtLeastOneCommand", new object[0]);
                }
                CommandProcessorBase base2 = this._commands[0];
                if ((base2 == null) || (base2.CommandRuntime == null))
                {
                    throw PSTraceSource.NewInvalidOperationException("PipelineStrings", "PipelineExecuteRequiresAtLeastOneCommand", new object[0]);
                }
                if (this.executionScope == null)
                {
                    this.executionScope = base2.Context.EngineSessionState.CurrentScope;
                }
                CommandProcessorBase base3 = this._commands[this._commands.Count - 1];
                if ((base3 == null) || (base3.CommandRuntime == null))
                {
                    throw PSTraceSource.NewInvalidOperationException();
                }
                if (this.ExternalSuccessOutput != null)
                {
                    base3.CommandRuntime.OutputPipe.ExternalWriter = this.ExternalSuccessOutput;
                }
                this.SetExternalErrorOutput();
                if ((this.ExternalInput == null) && !incomingStream)
                {
                    base2.CommandRuntime.IsClosed = true;
                }
                IDictionary variableValue = base2.Context.GetVariableValue(SpecialVariables.PSDefaultParameterValuesVarPath, false) as IDictionary;
                this.executionStarted = true;
                int[] numArray = new int[this._commands.Count + 1];
                for (int i = 0; i < this._commands.Count; i++)
                {
                    CommandProcessorBase base4 = this._commands[i];
                    if (base4 == null)
                    {
                        throw PSTraceSource.NewInvalidOperationException();
                    }
                    Guid engineActivityId = base4.Context.CurrentRunspace.EngineActivityId;
                    Guid activityId = EtwActivity.CreateActivityId();
                    EtwActivity.SetActivityId(activityId);
                    base4.PipelineActivityId = activityId;
                    MshLog.LogCommandLifecycleEvent(base4.Context, CommandState.Started, base4.Command.MyInvocation);
                    InvocationInfo myInvocation = base4.Command.MyInvocation;
                    myInvocation.PipelinePosition = i + 1;
                    myInvocation.PipelineLength = this._commands.Count;
                    myInvocation.PipelineIterationInfo = numArray;
                    base4.DoPrepare(variableValue);
                    myInvocation.ExpectingInput = base4.IsPipelineInputExpected();
                }
                this.SetupOutErrorVariable();
                for (int j = 0; j < this._commands.Count; j++)
                {
                    this._commands[j].DoBegin();
                }
            }
        }

        internal void StartStepping(bool expectInput)
        {
            try
            {
                this.Start(expectInput);
                if (this.firstTerminatingError != null)
                {
                    throw this.firstTerminatingError;
                }
            }
            catch (PipelineStoppedException)
            {
                this.DisposeCommands();
                if (this.firstTerminatingError != null)
                {
                    throw this.firstTerminatingError;
                }
                throw;
            }
        }

        internal Array Step(object input)
        {
            return this.DoStepItems(input, null, false);
        }

        internal Array Step(object input, Hashtable errorResults)
        {
            if (errorResults == null)
            {
                throw PSTraceSource.NewArgumentNullException("errorResults");
            }
            return this.DoStepItems(input, errorResults, false);
        }

        internal Array StepArray(Array input)
        {
            return this.DoStepItems(input, null, true);
        }

        internal Array StepArray(Array input, Hashtable errorResults)
        {
            if (errorResults == null)
            {
                throw PSTraceSource.NewArgumentNullException("errorResults");
            }
            return this.DoStepItems(input, errorResults, true);
        }

        internal void Stop()
        {
            if (this.RecordFailure(new PipelineStoppedException(), null))
            {
                List<CommandProcessorBase> list = this._commands;
                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        CommandProcessorBase base2 = list[i];
                        if (base2 == null)
                        {
                            throw PSTraceSource.NewInvalidOperationException();
                        }
                        try
                        {
                            base2.Command.DoStopProcessing();
                        }
                        catch (Exception exception)
                        {
                            CommandProcessorBase.CheckForSevereException(exception);
                        }
                    }
                }
            }
        }

        internal Array SynchronousExecute(Array input, Hashtable errorResults)
        {
            if (input == null)
            {
                return this.SynchronousExecuteEnumerate(AutomationNull.Value, errorResults, true);
            }
            return this.SynchronousExecuteEnumerate(input, errorResults, true);
        }

        internal Array SynchronousExecuteEnumerate(object input, Hashtable errorResults, bool enumerate)
        {
            if (this.Stopping)
            {
                throw new PipelineStoppedException();
            }
            Exception exception = null;
            try
            {
                CommandProcessorBase commandRequestingUpstreamCommandsToStop = null;
                try
                {
                    this.Start(input != AutomationNull.Value);
                    CommandProcessorBase base3 = this._commands[0];
                    if (this.ExternalInput != null)
                    {
                        base3.CommandRuntime.InputPipe.ExternalReader = this.ExternalInput;
                    }
                    this.Inject(input, enumerate);
                }
                catch (PipelineStoppedException)
                {
                    StopUpstreamCommandsException firstTerminatingError = this.firstTerminatingError as StopUpstreamCommandsException;
                    if (firstTerminatingError == null)
                    {
                        throw;
                    }
                    this.firstTerminatingError = null;
                    commandRequestingUpstreamCommandsToStop = firstTerminatingError.RequestingCommandProcessor;
                }
                this.DoCompleteCore(commandRequestingUpstreamCommandsToStop);
                if (this._redirectionPipes != null)
                {
                    foreach (PipelineProcessor processor in this._redirectionPipes)
                    {
                        processor.DoCompleteCore(null);
                    }
                }
                return this.RetrieveResults(errorResults);
            }
            catch (RuntimeException exception3)
            {
                if (this.firstTerminatingError != null)
                {
                    exception = this.firstTerminatingError;
                }
                else
                {
                    exception = exception3;
                }
                this.LogExecutionException(exception);
            }
            catch (InvalidComObjectException exception4)
            {
                if (this.firstTerminatingError != null)
                {
                    exception = this.firstTerminatingError;
                }
                else
                {
                    exception = new RuntimeException(StringUtil.Format(ParserStrings.InvalidComObjectException, exception4.Message), exception4);
                    ((RuntimeException) exception).SetErrorId("InvalidComObjectException");
                }
                this.LogExecutionException(exception);
            }
            finally
            {
                this.DisposeCommands();
            }
            RuntimeException.LockStackTrace(exception);
            throw exception;
        }

        internal List<CommandProcessorBase> Commands
        {
            get
            {
                return this._commands;
            }
        }

        internal bool ExecutionFailed
        {
            get
            {
                return this.executionFailed;
            }
            set
            {
                this.executionFailed = value;
            }
        }

        internal SessionStateScope ExecutionScope
        {
            get
            {
                return this.executionScope;
            }
            set
            {
                this.executionScope = value;
            }
        }

        internal bool ExecutionStarted
        {
            get
            {
                return this.executionStarted;
            }
        }

        internal PipelineWriter ExternalErrorOutput
        {
            get
            {
                return this.externalErrorOutput;
            }
            set
            {
                if (this.executionStarted)
                {
                    throw PSTraceSource.NewInvalidOperationException("PipelineStrings", "ExecutionAlreadyStarted", new object[0]);
                }
                this.externalErrorOutput = value;
            }
        }

        internal PipelineReader<object> ExternalInput
        {
            get
            {
                return this.externalInputPipe;
            }
            set
            {
                if (this.executionStarted)
                {
                    throw PSTraceSource.NewInvalidOperationException("PipelineStrings", "ExecutionAlreadyStarted", new object[0]);
                }
                this.externalInputPipe = value;
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
                if (this.executionStarted)
                {
                    throw PSTraceSource.NewInvalidOperationException("PipelineStrings", "ExecutionAlreadyStarted", new object[0]);
                }
                this.externalSuccessOutput = value;
            }
        }

        internal System.Management.Automation.Runspaces.LocalPipeline LocalPipeline
        {
            get
            {
                return this.localPipeline;
            }
            set
            {
                this.localPipeline = value;
            }
        }

        internal bool Stopping
        {
            get
            {
                return ((this.localPipeline != null) && this.localPipeline.IsStopping);
            }
        }

        internal bool TopLevel
        {
            get
            {
                return this.topLevel;
            }
            set
            {
                this.topLevel = value;
            }
        }
    }
}

