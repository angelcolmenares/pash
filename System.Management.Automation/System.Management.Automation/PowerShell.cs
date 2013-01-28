namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Runspaces.Internal;
    using System.Security.Principal;
    using System.Threading;

    public sealed class PowerShell : IDisposable
    {
        private bool _hadErrors;
        private PSCommand backupPSCommand;
        private PowerShellAsyncResult batchAsyncResult;
        private PSInvocationSettings batchInvocationSettings;
        private bool commandInvokedSynchronously;
        private ConnectCommandInfo connectCmdInfo;
        private PSDataStreams dataStreams;
        private PSDataCollection<ErrorRecord> errorBuffer;
        private bool errorBufferOwner;
        private Collection<PSCommand> extraCommands;
        private string historyString;
        private PSInformationalBuffers informationalBuffers;
        private Guid instanceId;
        private PSInvocationStateInfo invocationStateInfo;
        private PowerShellAsyncResult invokeAsyncResult;
        private bool isBatching;
        private bool isChild;
        private bool isDisposed;
        private bool isGetCommandMetadataSpecialPipeline;
        private bool isNested;
        private PSDataCollection<PSObject> outputBuffer;
        private bool outputBufferOwner;
        private PSCommand psCommand;
        private bool redirectShellErrorOutputPipe;
        private ClientRemotePowerShell remotePowerShell;
        private static string resBaseName = "PowerShellStrings";
        private object rsConnection;
        private bool runningExtraCommands;
        private System.Management.Automation.Runspaces.Runspace runspace;
        private bool runspaceOwner;
        private System.Management.Automation.Runspaces.RunspacePool runspacePool;
        private PowerShellAsyncResult stopAsyncResult;
        private bool stopBatchExecution;
        private object syncObject;
        private Worker worker;

        public event EventHandler<PSInvocationStateChangedEventArgs> InvocationStateChanged;

        internal event EventHandler<PSEventArgs<System.Management.Automation.Runspaces.Runspace>> RunspaceAssigned;

        internal PowerShell(ConnectCommandInfo connectCmdInfo, object rsConnection) : this(new PSCommand(), null, rsConnection)
        {
            this.extraCommands = new Collection<PSCommand>();
            this.runningExtraCommands = false;
            this.AddCommand(connectCmdInfo.Command);
            this.connectCmdInfo = connectCmdInfo;
            this.instanceId = this.connectCmdInfo.CommandId;
            this.invocationStateInfo = new PSInvocationStateInfo(PSInvocationState.Disconnected, null);
            if (rsConnection is RemoteRunspace)
            {
                this.runspace = rsConnection as System.Management.Automation.Runspaces.Runspace;
                this.runspacePool = ((RemoteRunspace) rsConnection).RunspacePool;
            }
            else if (rsConnection is System.Management.Automation.Runspaces.RunspacePool)
            {
                this.runspacePool = (System.Management.Automation.Runspaces.RunspacePool) rsConnection;
            }
            this.remotePowerShell = new ClientRemotePowerShell(this, this.runspacePool.RemoteRunspacePoolInternal);
        }

        private PowerShell(PSCommand command, Collection<PSCommand> extraCommands, object rsConnection)
        {
            this.outputBufferOwner = true;
            this.errorBufferOwner = true;
            this.syncObject = new object();
            this.redirectShellErrorOutputPipe = true;
            this.extraCommands = (extraCommands == null) ? new Collection<PSCommand>() : extraCommands;
            this.runningExtraCommands = false;
            this.psCommand = command;
            this.psCommand.Owner = this;
            RemoteRunspace runspace = rsConnection as RemoteRunspace;
            this.rsConnection = (runspace != null) ? runspace.RunspacePool : rsConnection;
            this.instanceId = Guid.NewGuid();
            this.invocationStateInfo = new PSInvocationStateInfo(PSInvocationState.NotStarted, null);
			this.outputBuffer = null;
            this.outputBufferOwner = true;
            this.errorBuffer = new PSDataCollection<ErrorRecord>();
            this.errorBufferOwner = true;
            this.informationalBuffers = new PSInformationalBuffers(this.instanceId);
            this.dataStreams = new PSDataStreams(this);
        }

        internal PowerShell(ObjectStreamBase inputstream, ObjectStreamBase outputstream, ObjectStreamBase errorstream, System.Management.Automation.Runspaces.RunspacePool runspacePool)
        {
            this.outputBufferOwner = true;
            this.errorBufferOwner = true;
            this.syncObject = new object();
            this.redirectShellErrorOutputPipe = true;
            this.extraCommands = new Collection<PSCommand>();
            this.runningExtraCommands = false;
            this.rsConnection = runspacePool;
            this.instanceId = Guid.NewGuid();
            this.invocationStateInfo = new PSInvocationStateInfo(PSInvocationState.NotStarted, null);
            this.informationalBuffers = new PSInformationalBuffers(this.instanceId);
            this.dataStreams = new PSDataStreams(this);
            PSDataCollectionStream<PSObject> stream = (PSDataCollectionStream<PSObject>) outputstream;
            this.outputBuffer = stream.ObjectStore;
            PSDataCollectionStream<ErrorRecord> stream2 = (PSDataCollectionStream<ErrorRecord>) errorstream;
            this.errorBuffer = stream2.ObjectStore;
            if ((runspacePool != null) && (runspacePool.RemoteRunspacePoolInternal != null))
            {
                this.remotePowerShell = new ClientRemotePowerShell(this, runspacePool.RemoteRunspacePoolInternal);
            }
        }

        internal PowerShell(ConnectCommandInfo connectCmdInfo, ObjectStreamBase inputstream, ObjectStreamBase outputstream, ObjectStreamBase errorstream, System.Management.Automation.Runspaces.RunspacePool runspacePool) : this(inputstream, outputstream, errorstream, runspacePool)
        {
            this.extraCommands = new Collection<PSCommand>();
            this.runningExtraCommands = false;
            this.psCommand = new PSCommand();
            this.psCommand.Owner = this;
            this.runspacePool = runspacePool;
            this.AddCommand(connectCmdInfo.Command);
            this.connectCmdInfo = connectCmdInfo;
            this.instanceId = this.connectCmdInfo.CommandId;
            this.invocationStateInfo = new PSInvocationStateInfo(PSInvocationState.Disconnected, null);
            this.remotePowerShell = new ClientRemotePowerShell(this, runspacePool.RemoteRunspacePoolInternal);
        }

        public PowerShell AddArgument(object value)
        {
            lock (this.syncObject)
            {
                if (this.psCommand.Commands.Count == 0)
                {
                    throw PSTraceSource.NewInvalidOperationException(resBaseName, "ParameterRequiresCommand", new object[0]);
                }
                this.AssertChangesAreAccepted();
                this.psCommand.AddArgument(value);
                return this;
            }
        }

        public PowerShell AddCommand(CommandInfo commandInfo)
        {
            if (commandInfo == null)
            {
                throw PSTraceSource.NewArgumentNullException("commandInfo");
            }
            Command command = new Command(commandInfo);
            this.psCommand.AddCommand(command);
            return this;
        }

        internal PowerShell AddCommand(Command command)
        {
            lock (this.syncObject)
            {
                this.AssertChangesAreAccepted();
                this.psCommand.AddCommand(command);
                return this;
            }
        }

        public PowerShell AddCommand(string cmdlet)
        {
            lock (this.syncObject)
            {
                this.AssertChangesAreAccepted();
                this.psCommand.AddCommand(cmdlet);
                return this;
            }
        }

        public PowerShell AddCommand(string cmdlet, bool useLocalScope)
        {
            lock (this.syncObject)
            {
                this.AssertChangesAreAccepted();
                this.psCommand.AddCommand(cmdlet, useLocalScope);
                return this;
            }
        }

        public PowerShell AddParameter(string parameterName)
        {
            lock (this.syncObject)
            {
                if (this.psCommand.Commands.Count == 0)
                {
                    throw PSTraceSource.NewInvalidOperationException(resBaseName, "ParameterRequiresCommand", new object[0]);
                }
                this.AssertChangesAreAccepted();
                this.psCommand.AddParameter(parameterName);
                return this;
            }
        }

        public PowerShell AddParameter(string parameterName, object value)
        {
            lock (this.syncObject)
            {
                if (this.psCommand.Commands.Count == 0)
                {
                    throw PSTraceSource.NewInvalidOperationException(resBaseName, "ParameterRequiresCommand", new object[0]);
                }
                this.AssertChangesAreAccepted();
                this.psCommand.AddParameter(parameterName, value);
                return this;
            }
        }

        public PowerShell AddParameters(IDictionary parameters)
        {
            lock (this.syncObject)
            {
                if (parameters == null)
                {
                    throw PSTraceSource.NewArgumentNullException("parameters");
                }
                if (this.psCommand.Commands.Count == 0)
                {
                    throw PSTraceSource.NewInvalidOperationException(resBaseName, "ParameterRequiresCommand", new object[0]);
                }
                this.AssertChangesAreAccepted();
                foreach (DictionaryEntry entry in parameters)
                {
                    string key = entry.Key as string;
                    if (key == null)
                    {
                        throw PSTraceSource.NewArgumentException("parameters", resBaseName, "KeyMustBeString", new object[0]);
                    }
                    this.psCommand.AddParameter(key, entry.Value);
                }
                return this;
            }
        }

        public PowerShell AddParameters(IList parameters)
        {
            lock (this.syncObject)
            {
                if (parameters == null)
                {
                    throw PSTraceSource.NewArgumentNullException("parameters");
                }
                if (this.psCommand.Commands.Count == 0)
                {
                    throw PSTraceSource.NewInvalidOperationException(resBaseName, "ParameterRequiresCommand", new object[0]);
                }
                this.AssertChangesAreAccepted();
                foreach (object obj2 in parameters)
                {
                    this.psCommand.AddParameter(null, obj2);
                }
                return this;
            }
        }

        public PowerShell AddScript(string script)
        {
            lock (this.syncObject)
            {
                this.AssertChangesAreAccepted();
                this.psCommand.AddScript(script);
                return this;
            }
        }

        public PowerShell AddScript(string script, bool useLocalScope)
        {
            lock (this.syncObject)
            {
                this.AssertChangesAreAccepted();
                this.psCommand.AddScript(script, useLocalScope);
                return this;
            }
        }

        public PowerShell AddStatement()
        {
            lock (this.syncObject)
            {
                if (this.psCommand.Commands.Count != 0)
                {
                    this.AssertChangesAreAccepted();
                    this.psCommand.Commands[this.psCommand.Commands.Count - 1].IsEndOfStatement = true;
                }
                return this;
            }
        }

        private void AppendExceptionToErrorStream(Exception e)
        {
            IContainsErrorRecord record = e as IContainsErrorRecord;
            if ((record != null) && (record.ErrorRecord != null))
            {
                this.Streams.Error.Add(record.ErrorRecord);
            }
            else
            {
                this.Streams.Error.Add(new ErrorRecord(e, "InvalidOperation", ErrorCategory.InvalidOperation, null));
            }
        }

        public PSJobProxy AsJobProxy()
        {
            if (this.Commands.Commands.Count == 0)
            {
                throw PSTraceSource.NewInvalidOperationException(resBaseName, "GetJobForCommandRequiresACommand", new object[0]);
            }
            if (this.Commands.Commands.Count > 1)
            {
                throw PSTraceSource.NewInvalidOperationException(resBaseName, "GetJobForCommandNotSupported", new object[0]);
            }
            bool flag = false;
            foreach (CommandParameter parameter in this.Commands.Commands[0].Parameters)
            {
                if (string.Compare(parameter.Name, "AsJob", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    flag = true;
                }
            }
            if (!flag)
            {
                this.AddParameter("AsJob");
            }
            PSJobProxy proxy = new PSJobProxy(this.Commands.Commands[0].CommandText);
            proxy.InitializeJobProxy(this.Commands, this.Runspace, this.RunspacePool);
            return proxy;
        }

        internal void AssertChangesAreAccepted()
        {
            lock (this.syncObject)
            {
                this.AssertNotDisposed();
                if (this.IsCommandRunning() || this.IsDisconnected())
                {
                    throw new InvalidPowerShellStateException(this.InvocationStateInfo.State);
                }
            }
        }

        private void AssertExecutionNotStarted()
        {
            this.AssertNotDisposed();
            if (this.IsCommandRunning())
            {
                throw new InvalidOperationException(StringUtil.Format(PowerShellStrings.ExecutionAlreadyStarted, new object[0]));
            }
            if (this.IsDisconnected())
            {
                throw new InvalidOperationException(StringUtil.Format(PowerShellStrings.ExecutionDisconnected, new object[0]));
            }
            if (this.invocationStateInfo.State == PSInvocationState.Stopping)
            {
                throw new InvalidOperationException(StringUtil.Format(PowerShellStrings.ExecutionStopping, new object[0]));
            }
        }

        private void AssertNotDisposed()
        {
            if (this.isDisposed)
            {
                throw PSTraceSource.NewObjectDisposedException("PowerShell");
            }
        }

        private void BatchInvocationCallback(IAsyncResult result)
        {
            PSDataCollection<PSObject> objs = null;
            try
            {
                objs = this.EndInvoke(result);
                if (objs == null)
                {
                    objs = this.batchAsyncResult.Output;
                }
                this.DoRemainingBatchCommands(objs);
            }
            catch (ActionPreferenceStopException exception)
            {
                this.batchAsyncResult.SetAsCompleted(exception);
            }
            catch (Exception exception2)
            {
                this.runningExtraCommands = false;
                CommandProcessorBase.CheckForSevereException(exception2);
                this.SetHadErrors(true);
                if ((this.batchInvocationSettings != null) && (((ActionPreference) this.batchInvocationSettings.ErrorActionPreference) == ActionPreference.Stop))
                {
                    this.batchAsyncResult.SetAsCompleted(exception2);
                    return;
                }
                if (this.batchInvocationSettings == null)
                {
                    switch (((ActionPreference) this.Runspace.SessionStateProxy.GetVariable("ErrorActionPreference")))
                    {
                        case ActionPreference.SilentlyContinue:
                        case ActionPreference.Continue:
                            this.AppendExceptionToErrorStream(exception2);
                            break;

                        case ActionPreference.Stop:
                            this.batchAsyncResult.SetAsCompleted(exception2);
                            return;
                    }
                }
                else if (((ActionPreference) this.batchInvocationSettings.ErrorActionPreference) != ActionPreference.Ignore)
                {
                    this.AppendExceptionToErrorStream(exception2);
                }
                if (objs == null)
                {
                    objs = this.batchAsyncResult.Output;
                }
                this.DoRemainingBatchCommands(objs);
            }
            finally
            {
                if (this.isBatching)
                {
                    this.EndAsyncBatchExecution();
                }
            }
        }

        private void BatchInvocationWorkItem(object state)
        {
            BatchInvocationContext context = state as BatchInvocationContext;
            PSCommand psCommand = this.psCommand;
            try
            {
                this.psCommand = context.Command;
                if (this.psCommand == this.extraCommands[this.extraCommands.Count - 1])
                {
                    this.runningExtraCommands = false;
                }
                try
                {
                    IAsyncResult asyncResult = this.CoreInvokeAsync<object, PSObject>(null, context.Output, this.batchInvocationSettings, null, this.batchAsyncResult.AsyncState, context.Output);
                    this.EndInvoke(asyncResult);
                }
                catch (ActionPreferenceStopException exception)
                {
                    this.stopBatchExecution = true;
                    this.batchAsyncResult.SetAsCompleted(exception);
                    return;
                }
                catch (Exception exception2)
                {
                    CommandProcessorBase.CheckForSevereException(exception2);
                    this.SetHadErrors(true);
                    if ((this.batchInvocationSettings != null) && (((ActionPreference) this.batchInvocationSettings.ErrorActionPreference) == ActionPreference.Stop))
                    {
                        this.stopBatchExecution = true;
                        this.AppendExceptionToErrorStream(exception2);
                        this.batchAsyncResult.SetAsCompleted(null);
                        return;
                    }
                    if (this.batchInvocationSettings == null)
                    {
                        switch (((ActionPreference) this.Runspace.SessionStateProxy.GetVariable("ErrorActionPreference")))
                        {
                            case ActionPreference.SilentlyContinue:
                            case ActionPreference.Continue:
                                this.AppendExceptionToErrorStream(exception2);
                                goto Label_0176;

                            case ActionPreference.Stop:
                                this.batchAsyncResult.SetAsCompleted(exception2);
                                return;

                            case ActionPreference.Inquire:
                            case ActionPreference.Ignore:
                                goto Label_0176;
                        }
                    }
                    else if (((ActionPreference) this.batchInvocationSettings.ErrorActionPreference) != ActionPreference.Ignore)
                    {
                        this.AppendExceptionToErrorStream(exception2);
                    }
                }
            Label_0176:
                if (this.psCommand == this.extraCommands[this.extraCommands.Count - 1])
                {
                    this.batchAsyncResult.SetAsCompleted(null);
                }
            }
            finally
            {
                this.psCommand = psCommand;
                context.Signal();
            }
        }

        private void BeginAsyncBatchExecution()
        {
            this.backupPSCommand = this.psCommand.Clone();
            this.extraCommands.Clear();
            PSCommand item = new PSCommand {
                Owner = this
            };
            foreach (Command command2 in this.psCommand.Commands)
            {
                if (command2.IsEndOfStatement)
                {
                    item.Commands.Add(command2);
                    this.extraCommands.Add(item);
                    item = new PSCommand {
                        Owner = this
                    };
                }
                else
                {
                    item.Commands.Add(command2);
                }
            }
            if (item.Commands.Count != 0)
            {
                this.extraCommands.Add(item);
            }
            this.psCommand = this.extraCommands[0];
        }

        private IAsyncResult BeginBatchInvoke<TInput, TOutput>(PSDataCollection<TInput> input, PSDataCollection<TOutput> output, PSInvocationSettings settings, AsyncCallback callback, object state)
        {
            PSDataCollection<PSObject> asyncResultOutput = output as PSDataCollection<PSObject>;
            if (asyncResultOutput == null)
            {
                throw PSTraceSource.NewInvalidOperationException();
            }
            if (this.isBatching)
            {
                this.BeginAsyncBatchExecution();
            }
            System.Management.Automation.Runspaces.RunspacePool rsConnection = this.rsConnection as System.Management.Automation.Runspaces.RunspacePool;
            if (((rsConnection != null) && rsConnection.IsRemote) && RemotingDecoder.ServerSupportsBatchInvocation(this.runspace))
            {
                try
                {
                    return this.CoreInvokeAsync<TInput, TOutput>(input, output, settings, callback, state, asyncResultOutput);
                }
                finally
                {
                    if (this.isBatching)
                    {
                        this.EndAsyncBatchExecution();
                    }
                }
            }
            this.runningExtraCommands = true;
            this.batchInvocationSettings = settings;
            this.batchAsyncResult = new PowerShellAsyncResult(this.instanceId, callback, state, asyncResultOutput, true);
            this.CoreInvokeAsync<TInput, TOutput>(input, output, settings, new AsyncCallback(this.BatchInvocationCallback), state, asyncResultOutput);
            return this.batchAsyncResult;
        }

        public IAsyncResult BeginInvoke()
        {
            return this.BeginInvoke<object>(null, null, null, null);
        }

        public IAsyncResult BeginInvoke<T>(PSDataCollection<T> input)
        {
            return this.BeginInvoke<T>(input, null, null, null);
        }

        public IAsyncResult BeginInvoke<TInput, TOutput>(PSDataCollection<TInput> input, PSDataCollection<TOutput> output)
        {
            return this.BeginInvoke<TInput, TOutput>(input, output, null, null, null);
        }

        public IAsyncResult BeginInvoke<T>(PSDataCollection<T> input, PSInvocationSettings settings, AsyncCallback callback, object state)
        {
            this.DetermineIsBatching();
            if (this.outputBuffer != null)
            {
                if (!this.isBatching && (this.extraCommands.Count == 0))
                {
                    return this.CoreInvokeAsync<T, PSObject>(input, this.outputBuffer, settings, callback, state, null);
                }
                return this.BeginBatchInvoke<T, PSObject>(input, this.outputBuffer, settings, callback, state);
            }
            PSDataCollection<PSObject> output = new PSDataCollection<PSObject>();
            this.outputBufferOwner = true;
            if (!this.isBatching && (this.extraCommands.Count == 0))
            {
                return this.CoreInvokeAsync<T, PSObject>(input, output, settings, callback, state, output);
            }
            return this.BeginBatchInvoke<T, PSObject>(input, output, settings, callback, state);
        }

        public IAsyncResult BeginInvoke<TInput, TOutput>(PSDataCollection<TInput> input, PSDataCollection<TOutput> output, PSInvocationSettings settings, AsyncCallback callback, object state)
        {
            if (output == null)
            {
                throw PSTraceSource.NewArgumentNullException("output");
            }
            this.DetermineIsBatching();
            if (!this.isBatching && (this.extraCommands.Count == 0))
            {
                return this.CoreInvokeAsync<TInput, TOutput>(input, output, settings, callback, state, null);
            }
            return this.BeginBatchInvoke<TInput, TOutput>(input, output, settings, callback, state);
        }

        public IAsyncResult BeginStop(AsyncCallback callback, object state)
        {
            return this.CoreStop(false, callback, state);
        }

        private void CheckRunspacePoolAndConnect()
        {
            RemoteRunspacePoolInternal remoteRunspacePoolInternal = null;
            if (this.rsConnection is RemoteRunspace)
            {
                remoteRunspacePoolInternal = (this.rsConnection as RemoteRunspace).RunspacePool.RemoteRunspacePoolInternal;
            }
            else if (this.rsConnection is System.Management.Automation.Runspaces.RunspacePool)
            {
                remoteRunspacePoolInternal = (this.rsConnection as System.Management.Automation.Runspaces.RunspacePool).RemoteRunspacePoolInternal;
            }
            if (remoteRunspacePoolInternal == null)
            {
                throw new InvalidOperationException(PowerShellStrings.CannotConnect);
            }
            if (remoteRunspacePoolInternal.RunspacePoolStateInfo.State == RunspacePoolState.Disconnected)
            {
                remoteRunspacePoolInternal.Connect();
            }
            if (remoteRunspacePoolInternal.RunspacePoolStateInfo.State != RunspacePoolState.Opened)
            {
                throw new InvalidRunspacePoolStateException(RunspacePoolStrings.InvalidRunspacePoolState, remoteRunspacePoolInternal.RunspacePoolStateInfo.State, RunspacePoolState.Opened);
            }
        }

        internal void ClearRemotePowerShell()
        {
            lock (this.syncObject)
            {
                if (this.remotePowerShell != null)
                {
                    this.remotePowerShell.Clear();
                }
            }
        }

        private void CloseInputBufferOnReconnection(PSInvocationState previousState)
        {
            if ((((previousState == PSInvocationState.Disconnected) && this.commandInvokedSynchronously) && ((this.remotePowerShell.InputStream != null) && this.remotePowerShell.InputStream.IsOpen)) && (this.remotePowerShell.InputStream.Count > 0))
            {
                this.remotePowerShell.InputStream.Close();
            }
        }

        private List<PSObject> CommandsAsListOfPSObjects(CommandCollection commands, Version psRPVersion)
        {
            List<PSObject> list = new List<PSObject>(commands.Count);
            foreach (Command command in commands)
            {
                list.Add(command.ToPSObjectForRemoting(psRPVersion));
            }
            return list;
        }

        public Collection<PSObject> Connect()
        {
            this.commandInvokedSynchronously = true;
            PowerShellAsyncResult result2 = this.ConnectAsync() as PowerShellAsyncResult;
            result2.EndInvoke();
            if (result2.Output != null)
            {
                return result2.Output.ReadAll();
            }
            return new Collection<PSObject>();
        }

        public IAsyncResult ConnectAsync()
        {
            if (this.invocationStateInfo.State != PSInvocationState.Disconnected)
            {
                throw new InvalidPowerShellStateException(this.invocationStateInfo.State);
            }
            this.CheckRunspacePoolAndConnect();
            if (this.connectCmdInfo != null)
            {
                if (!this.remotePowerShell.Initialized)
                {
                    ObjectStreamBase inputstream = new ObjectStream();
                    inputstream.Close();
                    if (this.outputBuffer == null)
                    {
                        this.outputBuffer = new PSDataCollection<PSObject>();
                    }
                    ObjectStreamBase outputstream = new PSDataCollectionStream<PSObject>(this.instanceId, this.outputBuffer);
                    this.remotePowerShell.Initialize(inputstream, outputstream, new PSDataCollectionStream<ErrorRecord>(this.instanceId, this.errorBuffer), this.informationalBuffers, null);
                }
                this.invokeAsyncResult = new PowerShellAsyncResult(this.instanceId, null, null, this.outputBuffer, true);
            }
            try
            {
                this.remotePowerShell.ConnectAsync(this.connectCmdInfo);
            }
            catch (Exception exception)
            {
                this.invokeAsyncResult = null;
                this.SetStateChanged(new PSInvocationStateInfo(PSInvocationState.Failed, exception));
                InvalidRunspacePoolStateException exception2 = exception as InvalidRunspacePoolStateException;
                if ((exception2 != null) && (this.runspace != null))
                {
                    throw exception2.ToInvalidRunspaceStateException();
                }
                throw;
            }
            return this.invokeAsyncResult;
        }

        private void CoreInvoke<TInput, TOutput>(PSDataCollection<TInput> input, PSDataCollection<TOutput> output, PSInvocationSettings settings)
        {
            bool flag = false;
            this.DetermineIsBatching();
            if (this.isBatching)
            {
                this.BeginAsyncBatchExecution();
            }
            this.SetHadErrors(false);
            System.Management.Automation.Runspaces.RunspacePool rsConnection = this.rsConnection as System.Management.Automation.Runspaces.RunspacePool;
            if ((rsConnection != null) && rsConnection.IsRemote)
            {
                if (RemotingDecoder.ServerSupportsBatchInvocation(this.runspace))
                {
                    try
                    {
                        this.CoreInvokeRemoteHelper<TInput, TOutput>(input, output, settings);
                    }
                    finally
                    {
                        if (this.isBatching)
                        {
                            this.EndAsyncBatchExecution();
                        }
                    }
                    return;
                }
                flag = true;
            }
            if (this.isBatching)
            {
                try
                {
                    foreach (PSCommand command in this.extraCommands)
                    {
                        if (this.psCommand != this.extraCommands[this.extraCommands.Count - 1])
                        {
                            this.runningExtraCommands = true;
                        }
                        else
                        {
                            this.runningExtraCommands = false;
                        }
                        try
                        {
                            this.psCommand = command;
                            if (flag)
                            {
                                this.CoreInvokeRemoteHelper<TInput, TOutput>(input, output, settings);
                            }
                            else
                            {
                                this.CoreInvokeHelper<TInput, TOutput>(input, output, settings);
                            }
                        }
                        catch (ActionPreferenceStopException)
                        {
                            throw;
                        }
                        catch (Exception exception)
                        {
                            CommandProcessorBase.CheckForSevereException(exception);
                            this.SetHadErrors(true);
                            if ((settings != null) && (((ActionPreference) settings.ErrorActionPreference) == ActionPreference.Stop))
                            {
                                throw;
                            }
                            if ((settings == null) || (((ActionPreference) settings.ErrorActionPreference) != ActionPreference.Ignore))
                            {
                                IContainsErrorRecord record = exception as IContainsErrorRecord;
                                if ((record != null) && (record.ErrorRecord != null))
                                {
                                    this.Streams.Error.Add(record.ErrorRecord);
                                }
                                else
                                {
                                    this.Streams.Error.Add(new ErrorRecord(exception, "InvalidOperation", ErrorCategory.InvalidOperation, null));
                                }
                            }
                        }
                    }
                    return;
                }
                finally
                {
                    this.runningExtraCommands = false;
                    if (this.isBatching)
                    {
                        this.EndAsyncBatchExecution();
                    }
                }
            }
            this.runningExtraCommands = false;
            if (flag)
            {
                this.CoreInvokeRemoteHelper<TInput, TOutput>(input, output, settings);
            }
            else
            {
                this.CoreInvokeHelper<TInput, TOutput>(input, output, settings);
            }
        }

        private void CoreInvoke<TOutput>(IEnumerable input, PSDataCollection<TOutput> output, PSInvocationSettings settings)
        {
            PSDataCollection<object> datas = null;
            if (input != null)
            {
                datas = new PSDataCollection<object>();
                foreach (object obj2 in input)
                {
                    datas.Add(obj2);
                }
                datas.Complete();
            }
            this.CoreInvoke<object, TOutput>(datas, output, settings);
        }

        private IAsyncResult CoreInvokeAsync<TInput, TOutput>(PSDataCollection<TInput> input, PSDataCollection<TOutput> output, PSInvocationSettings settings, AsyncCallback callback, object state, PSDataCollection<PSObject> asyncResultOutput)
        {
            System.Management.Automation.Runspaces.RunspacePool rsConnection = this.rsConnection as System.Management.Automation.Runspaces.RunspacePool;
            this.Prepare<TInput, TOutput>(input, output, settings, (rsConnection == null) || !rsConnection.IsRemote);
            this.invokeAsyncResult = new PowerShellAsyncResult(this.instanceId, callback, state, asyncResultOutput, true);
            try
            {
                if (this.isNested && ((rsConnection == null) || !rsConnection.IsRemote))
                {
                    throw PSTraceSource.NewInvalidOperationException(resBaseName, "NestedPowerShellInvokeAsync", new object[0]);
                }
                if (rsConnection != null)
                {
                    this.VerifyThreadSettings(settings, rsConnection.ApartmentState, rsConnection.ThreadOptions, rsConnection.IsRemote);
                    rsConnection.AssertPoolIsOpen();
                    if (rsConnection.IsRemote)
                    {
                        this.worker = null;
                        lock (this.syncObject)
                        {
                            this.AssertExecutionNotStarted();
                            this.invocationStateInfo = new PSInvocationStateInfo(PSInvocationState.Running, null);
                            ObjectStreamBase inputstream = null;
                            if (input != null)
                            {
                                inputstream = new PSDataCollectionStream<TInput>(this.instanceId, input);
                            }
                            if (!this.remotePowerShell.Initialized)
                            {
                                if (inputstream == null)
                                {
                                    inputstream = new ObjectStream();
                                    inputstream.Close();
                                }
                                this.remotePowerShell.Initialize(inputstream, new PSDataCollectionStream<TOutput>(this.instanceId, output), new PSDataCollectionStream<ErrorRecord>(this.instanceId, this.errorBuffer), this.informationalBuffers, settings);
                            }
                            else
                            {
                                if (inputstream != null)
                                {
                                    this.remotePowerShell.InputStream = inputstream;
                                }
                                if (output != null)
                                {
                                    this.remotePowerShell.OutputStream = new PSDataCollectionStream<TOutput>(this.instanceId, output);
                                }
                            }
                            rsConnection.RemoteRunspacePoolInternal.CreatePowerShellOnServerAndInvoke(this.remotePowerShell);
                        }
                        this.RaiseStateChangeEvent(this.invocationStateInfo.Clone());
                    }
                    else
                    {
                        this.worker.GetRunspaceAsyncResult = rsConnection.BeginGetRunspace(new AsyncCallback(this.worker.RunspaceAvailableCallback), null);
                    }
                }
                else
                {
                    LocalRunspace rsToUse = this.rsConnection as LocalRunspace;
                    if (rsToUse != null)
                    {
                        this.VerifyThreadSettings(settings, rsToUse.ApartmentState, rsToUse.ThreadOptions, false);
                        if (rsToUse.RunspaceStateInfo.State != RunspaceState.Opened)
                        {
                            InvalidRunspaceStateException exception = new InvalidRunspaceStateException(StringUtil.Format(PowerShellStrings.InvalidRunspaceState, RunspaceState.Opened, rsToUse.RunspaceStateInfo.State), rsToUse.RunspaceStateInfo.State, RunspaceState.Opened);
                            throw exception;
                        }
                        this.worker.CreateRunspaceIfNeededAndDoWork(rsToUse, false);
                    }
                    else
                    {
                        ThreadPool.QueueUserWorkItem(new WaitCallback(this.worker.CreateRunspaceIfNeededAndDoWork), this.rsConnection);
                    }
                }
            }
            catch (Exception exception2)
            {
                this.invokeAsyncResult = null;
                this.SetStateChanged(new PSInvocationStateInfo(PSInvocationState.Failed, exception2));
                InvalidRunspacePoolStateException exception3 = exception2 as InvalidRunspacePoolStateException;
                if ((exception3 != null) && (this.runspace != null))
                {
                    throw exception3.ToInvalidRunspaceStateException();
                }
                throw;
            }
            return this.invokeAsyncResult;
        }

        private void CoreInvokeHelper<TInput, TOutput>(PSDataCollection<TInput> input, PSDataCollection<TOutput> output, PSInvocationSettings settings)
        {
            System.Management.Automation.Runspaces.RunspacePool rsConnection = this.rsConnection as System.Management.Automation.Runspaces.RunspacePool;
            this.Prepare<TInput, TOutput>(input, output, settings, true);
            try
            {
                System.Management.Automation.Runspaces.Runspace rsToUse = null;
                if (!this.isNested)
                {
                    if (rsConnection != null)
                    {
                        this.VerifyThreadSettings(settings, rsConnection.ApartmentState, rsConnection.ThreadOptions, false);
                        this.worker.GetRunspaceAsyncResult = rsConnection.BeginGetRunspace(null, null);
                        this.worker.GetRunspaceAsyncResult.AsyncWaitHandle.WaitOne();
                        rsToUse = rsConnection.EndGetRunspace(this.worker.GetRunspaceAsyncResult);
                    }
                    else
                    {
                        rsToUse = this.rsConnection as System.Management.Automation.Runspaces.Runspace;
                        if (rsToUse != null)
                        {
                            this.VerifyThreadSettings(settings, rsToUse.ApartmentState, rsToUse.ThreadOptions, false);
                            if (rsToUse.RunspaceStateInfo.State != RunspaceState.Opened)
                            {
                                InvalidRunspaceStateException exception = new InvalidRunspaceStateException(StringUtil.Format(PowerShellStrings.InvalidRunspaceState, RunspaceState.Opened, rsToUse.RunspaceStateInfo.State), rsToUse.RunspaceStateInfo.State, RunspaceState.Opened);
                                throw exception;
                            }
                        }
                    }
                    this.worker.CreateRunspaceIfNeededAndDoWork(rsToUse, true);
                }
                else
                {
                    rsToUse = this.rsConnection as System.Management.Automation.Runspaces.Runspace;
                    this.worker.ConstructPipelineAndDoWork(rsToUse, true);
                }
            }
            catch (Exception exception2)
            {
                this.SetStateChanged(new PSInvocationStateInfo(PSInvocationState.Failed, exception2));
                InvalidRunspacePoolStateException exception3 = exception2 as InvalidRunspacePoolStateException;
                if ((exception3 != null) && (this.runspace != null))
                {
                    throw exception3.ToInvalidRunspaceStateException();
                }
                throw;
            }
        }

        private void CoreInvokeRemoteHelper<TInput, TOutput>(PSDataCollection<TInput> input, PSDataCollection<TOutput> output, PSInvocationSettings settings)
        {
            IAsyncResult result = this.CoreInvokeAsync<TInput, TOutput>(input, output, settings, null, null, null);
            this.commandInvokedSynchronously = true;
            (result as PowerShellAsyncResult).EndInvoke();
            if ((PSInvocationState.Failed == this.invocationStateInfo.State) && (this.invocationStateInfo.Reason != null))
            {
                throw this.invocationStateInfo.Reason;
            }
        }

        private IAsyncResult CoreStop(bool isSyncCall, AsyncCallback callback, object state)
        {
            bool flag = false;
            bool flag2 = false;
            Queue<PSInvocationStateInfo> queue = new Queue<PSInvocationStateInfo>();
            lock (this.syncObject)
            {
                switch (this.invocationStateInfo.State)
                {
                    case PSInvocationState.NotStarted:
                        this.invocationStateInfo = new PSInvocationStateInfo(PSInvocationState.Stopping, null);
                        queue.Enqueue(new PSInvocationStateInfo(PSInvocationState.Stopped, null));
                        break;

                    case PSInvocationState.Running:
                        this.invocationStateInfo = new PSInvocationStateInfo(PSInvocationState.Stopping, null);
                        flag = true;
                        break;

                    case PSInvocationState.Stopping:
                    case PSInvocationState.Stopped:
                    case PSInvocationState.Completed:
                    case PSInvocationState.Failed:
                        this.stopAsyncResult = new PowerShellAsyncResult(this.instanceId, callback, state, null, false);
                        this.stopAsyncResult.SetAsCompleted(null);
                        return this.stopAsyncResult;

                    case PSInvocationState.Disconnected:
                        this.invocationStateInfo = new PSInvocationStateInfo(PSInvocationState.Failed, null);
                        flag2 = true;
                        break;
                }
                this.stopAsyncResult = new PowerShellAsyncResult(this.instanceId, callback, state, null, false);
            }
            if (flag2)
            {
                if (this.invokeAsyncResult != null)
                {
                    this.invokeAsyncResult.SetAsCompleted(null);
                }
                this.stopAsyncResult.SetAsCompleted(null);
                this.RaiseStateChangeEvent(this.invocationStateInfo.Clone());
                return this.stopAsyncResult;
            }
            this.RaiseStateChangeEvent(this.invocationStateInfo.Clone());
            bool flag3 = false;
            System.Management.Automation.Runspaces.RunspacePool rsConnection = this.rsConnection as System.Management.Automation.Runspaces.RunspacePool;
            if ((rsConnection != null) && rsConnection.IsRemote)
            {
                if ((this.remotePowerShell != null) && this.remotePowerShell.Initialized)
                {
                    this.remotePowerShell.StopAsync();
                    if (isSyncCall)
                    {
                        this.stopAsyncResult.AsyncWaitHandle.WaitOne();
                    }
                }
                else
                {
                    flag3 = true;
                }
            }
            else if (flag)
            {
                this.worker.Stop(isSyncCall);
            }
            else
            {
                flag3 = true;
            }
            if (flag3)
            {
                if (isSyncCall)
                {
                    this.StopHelper(queue);
                }
                else
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(this.StopThreadProc), queue);
                }
            }
            return this.stopAsyncResult;
        }

        public static PowerShell Create()
        {
            return new PowerShell(new PSCommand(), null, null);
        }

        public static PowerShell Create(RunspaceMode runspace)
        {
            switch (runspace)
            {
                case RunspaceMode.CurrentRunspace:
                    if (System.Management.Automation.Runspaces.Runspace.DefaultRunspace == null)
                    {
                        throw new InvalidOperationException(PowerShellStrings.NoDefaultRunspaceForPSCreate);
                    }
                    return new PowerShell(new PSCommand(), null, System.Management.Automation.Runspaces.Runspace.DefaultRunspace) { isChild = true, isNested = true, IsRunspaceOwner = false, runspace = System.Management.Automation.Runspaces.Runspace.DefaultRunspace };

                case RunspaceMode.NewRunspace:
                    return new PowerShell(new PSCommand(), null, null);
            }
            return null;
        }

        public static PowerShell Create(InitialSessionState initialSessionState)
        {
            PowerShell shell = Create();
            shell.Runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            shell.Runspace.Open();
            return shell;
        }

        private static PowerShell Create(bool isNested, PSCommand psCommand, Collection<PSCommand> extraCommands)
        {
            return new PowerShell(psCommand, extraCommands, null) { isNested = isNested };
        }

        public PowerShell CreateNestedPowerShell()
        {
            if ((this.worker == null) || (this.worker.CurrentlyRunningPipeline == null))
            {
                throw PSTraceSource.NewInvalidOperationException(resBaseName, "InvalidStateCreateNested", new object[0]);
            }
            return new PowerShell(new PSCommand(), null, this.worker.CurrentlyRunningPipeline.Runspace) { isNested = true };
        }

        private void DetermineIsBatching()
        {
            foreach (Command command in this.psCommand.Commands)
            {
                if (command.IsEndOfStatement)
                {
                    this.isBatching = true;
                    return;
                }
            }
            this.isBatching = false;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (this.syncObject)
                {
                    if (this.isDisposed)
                    {
                        return;
                    }
                }
                if (this.invocationStateInfo.State == PSInvocationState.Running)
                {
                    this.Stop();
                }
                lock (this.syncObject)
                {
                    this.isDisposed = true;
                }
                if ((this.outputBuffer != null) && this.outputBufferOwner)
                {
                    this.outputBuffer.Dispose();
                }
                if ((this.errorBuffer != null) && this.errorBufferOwner)
                {
                    this.errorBuffer.Dispose();
                }
                if (this.runspaceOwner)
                {
                    this.runspace.Dispose();
                }
                if (this.remotePowerShell != null)
                {
                    this.remotePowerShell.Dispose();
                }
                this.invokeAsyncResult = null;
                this.stopAsyncResult = null;
            }
        }

        private void DoRemainingBatchCommands(PSDataCollection<PSObject> objs)
        {
            if (this.extraCommands.Count > 1)
            {
                for (int i = 1; i < this.extraCommands.Count; i++)
                {
                    if (this.stopBatchExecution)
                    {
                        return;
                    }
                    BatchInvocationContext state = new BatchInvocationContext(this.extraCommands[i], objs);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(this.BatchInvocationWorkItem), state);
                    state.Wait();
                }
            }
        }

        private void EndAsyncBatchExecution()
        {
            this.psCommand = this.backupPSCommand;
        }

        public PSDataCollection<PSObject> EndInvoke(IAsyncResult asyncResult)
        {
            PSDataCollection<PSObject> output;
            try
            {
                if (asyncResult == null)
                {
                    throw PSTraceSource.NewArgumentNullException("asyncResult");
                }
                PowerShellAsyncResult result = asyncResult as PowerShellAsyncResult;
                if (((result == null) || (result.OwnerId != this.instanceId)) || !result.IsAssociatedWithAsyncInvoke)
                {
                    throw PSTraceSource.NewArgumentException("asyncResult", resBaseName, "AsyncResultNotOwned", new object[] { "IAsyncResult", "BeginInvoke" });
                }
                result.EndInvoke();
                output = result.Output;
            }
            catch (InvalidRunspacePoolStateException exception)
            {
                this.SetHadErrors(true);
                if (this.runspace != null)
                {
                    throw exception.ToInvalidRunspaceStateException();
                }
                throw;
            }
            return output;
        }

        public void EndStop(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw PSTraceSource.NewArgumentNullException("asyncResult");
            }
            PowerShellAsyncResult result = asyncResult as PowerShellAsyncResult;
            if (((result == null) || (result.OwnerId != this.instanceId)) || result.IsAssociatedWithAsyncInvoke)
            {
                throw PSTraceSource.NewArgumentException("asyncResult", resBaseName, "AsyncResultNotOwned", new object[] { "IAsyncResult", "BeginStop" });
            }
            result.EndInvoke();
        }

        internal static PowerShell FromPSObjectForRemoting(PSObject powerShellAsPSObject)
        {
            if (powerShellAsPSObject == null)
            {
                throw PSTraceSource.NewArgumentNullException("powerShellAsPSObject");
            }
            Collection<PSCommand> extraCommands = null;
            if (powerShellAsPSObject.Properties.Match("ExtraCmds").Count > 0)
            {
                extraCommands = new Collection<PSCommand>();
                foreach (PSObject obj2 in RemotingDecoder.EnumerateListProperty<PSObject>(powerShellAsPSObject, "ExtraCmds"))
                {
                    PSCommand item = null;
                    foreach (PSObject obj3 in RemotingDecoder.EnumerateListProperty<PSObject>(obj2, "Cmds"))
                    {
                        Command command = Command.FromPSObjectForRemoting(obj3);
                        if (item == null)
                        {
                            item = new PSCommand(command);
                        }
                        else
                        {
                            item.AddCommand(command);
                        }
                    }
                    extraCommands.Add(item);
                }
            }
			
			object privateData = RemotingDecoder.GetPropertyValue<object>(powerShellAsPSObject, "PrivateData");

            PSCommand psCommand = null;
            foreach (PSObject obj4 in RemotingDecoder.EnumerateListProperty<PSObject>(powerShellAsPSObject, "Cmds"))
            {
                Command command4 = Command.FromPSObjectForRemoting(obj4);
                if (psCommand == null)
                {
                    psCommand = new PSCommand(command4);
                }
                else
                {
                    psCommand.AddCommand(command4);
                }
            }


			bool isNested = RemotingDecoder.GetPropertyValue<bool>(powerShellAsPSObject, "IsNested");
            PowerShell shell = Create(isNested, psCommand, extraCommands);
            shell.HistoryString = RemotingDecoder.GetPropertyValue<string>(powerShellAsPSObject, "History");
            shell.RedirectShellErrorOutputPipe = RemotingDecoder.GetPropertyValue<bool>(powerShellAsPSObject, "RedirectShellErrorOutputPipe");
            return shell;
        }

        internal void GenerateNewInstanceId()
        {
            this.instanceId = Guid.NewGuid();
        }

        internal System.Management.Automation.ExecutionContext GetContextFromTLS()
        {
            System.Management.Automation.ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            if (executionContextFromTLS != null)
            {
                return executionContextFromTLS;
            }
            string original = (this.Commands.Commands.Count > 0) ? this.Commands.Commands[0].CommandText : null;
            PSInvalidOperationException exception = null;
            if (original != null)
            {
                original = ErrorCategoryInfo.Ellipsize(Thread.CurrentThread.CurrentUICulture, original);
                exception = PSTraceSource.NewInvalidOperationException("PowerShellStrings", "CommandInvokedFromWrongThreadWithCommand", new object[] { original });
            }
            else
            {
                exception = PSTraceSource.NewInvalidOperationException("PowerShellStrings", "CommandInvokedFromWrongThreadWithoutCommand", new object[0]);
            }
            exception.SetErrorId("CommandInvokedFromWrongThread");
            throw exception;
        }

        internal object GetRunspaceConnection()
        {
            return this.rsConnection;
        }

        internal SteppablePipeline GetSteppablePipeline()
        {
            System.Management.Automation.ExecutionContext contextFromTLS = this.GetContextFromTLS();
            return this.GetSteppablePipeline(contextFromTLS, CommandOrigin.Runspace);
        }

        private SteppablePipeline GetSteppablePipeline(System.Management.Automation.ExecutionContext context, CommandOrigin commandOrigin)
        {
            if (this.Commands.Commands.Count == 0)
            {
                return null;
            }
            PipelineProcessor pipeline = new PipelineProcessor();
            bool flag = false;
            try
            {
                foreach (Command command in this.Commands.Commands)
                {
                    CommandProcessorBase commandProcessor = command.CreateCommandProcessor(System.Management.Automation.Runspaces.Runspace.DefaultRunspace.ExecutionContext, ((LocalRunspace) System.Management.Automation.Runspaces.Runspace.DefaultRunspace).CommandFactory, false, this.isNested ? CommandOrigin.Internal : CommandOrigin.Runspace);
                    commandProcessor.RedirectShellErrorOutputPipe = this.redirectShellErrorOutputPipe;
                    pipeline.Add(commandProcessor);
                }
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
                    pipeline.Dispose();
                }
            }
            return new SteppablePipeline(context, pipeline);
        }

        internal void InitForRemotePipeline(CommandCollection command, ObjectStreamBase inputstream, ObjectStreamBase outputstream, ObjectStreamBase errorstream, PSInvocationSettings settings, bool redirectShellErrorOutputPipe)
        {
            this.psCommand = new PSCommand(command[0]);
            this.psCommand.Owner = this;
            for (int i = 1; i < command.Count; i++)
            {
                this.AddCommand(command[i]);
            }
            this.redirectShellErrorOutputPipe = redirectShellErrorOutputPipe;
            if (this.remotePowerShell == null)
            {
                this.remotePowerShell = new ClientRemotePowerShell(this, ((System.Management.Automation.Runspaces.RunspacePool) this.rsConnection).RemoteRunspacePoolInternal);
            }
            this.remotePowerShell.Initialize(inputstream, outputstream, errorstream, this.informationalBuffers, settings);
        }

        internal void InitForRemotePipelineConnect(ObjectStreamBase inputstream, ObjectStreamBase outputstream, ObjectStreamBase errorstream, PSInvocationSettings settings, bool redirectShellErrorOutputPipe)
        {
            this.CheckRunspacePoolAndConnect();
            if (this.invocationStateInfo.State != PSInvocationState.Disconnected)
            {
                throw new InvalidPowerShellStateException(this.invocationStateInfo.State);
            }
            this.redirectShellErrorOutputPipe = redirectShellErrorOutputPipe;
            if (this.remotePowerShell == null)
            {
                this.remotePowerShell = new ClientRemotePowerShell(this, ((System.Management.Automation.Runspaces.RunspacePool) this.rsConnection).RemoteRunspacePoolInternal);
            }
            if (!this.remotePowerShell.Initialized)
            {
                this.remotePowerShell.Initialize(inputstream, outputstream, errorstream, this.informationalBuffers, settings);
            }
        }

        private void InternalClearSuppressExceptions()
        {
            lock (this.syncObject)
            {
                if (this.worker != null)
                {
                    this.worker.InternalClearSuppressExceptions();
                }
            }
        }

        public Collection<PSObject> Invoke()
        {
            return this.Invoke(null, null);
        }

        public Collection<T> Invoke<T>()
        {
            Collection<T> output = new Collection<T>();
            this.Invoke<T>(null, output, null);
            return output;
        }

        public Collection<PSObject> Invoke(IEnumerable input)
        {
            return this.Invoke(input, null);
        }

        public Collection<T> Invoke<T>(IEnumerable input)
        {
            Collection<T> output = new Collection<T>();
            this.Invoke<T>(input, output, null);
            return output;
        }

        public void Invoke<T>(IEnumerable input, IList<T> output)
        {
            this.Invoke<T>(input, output, null);
        }

        public Collection<PSObject> Invoke(IEnumerable input, PSInvocationSettings settings)
        {
            Collection<PSObject> listToUse = new Collection<PSObject>();
            PSDataCollection<PSObject> output = new PSDataCollection<PSObject>(listToUse);
            this.CoreInvoke<PSObject>(input, output, settings);
            return listToUse;
        }

        public Collection<T> Invoke<T>(IEnumerable input, PSInvocationSettings settings)
        {
            Collection<T> output = new Collection<T>();
            this.Invoke<T>(input, output, settings);
            return output;
        }

        public void Invoke<T>(IEnumerable input, IList<T> output, PSInvocationSettings settings)
        {
            if (output == null)
            {
                throw PSTraceSource.NewArgumentNullException("output");
            }
            PSDataCollection<T> datas = new PSDataCollection<T>(output);
            this.CoreInvoke<T>(input, datas, settings);
        }

        public void Invoke<TInput, TOutput>(PSDataCollection<TInput> input, PSDataCollection<TOutput> output, PSInvocationSettings settings)
        {
            if (output == null)
            {
                throw PSTraceSource.NewArgumentNullException("output");
            }
            this.CoreInvoke<TInput, TOutput>(input, output, settings);
        }

        private bool IsCommandRunning()
        {
            return (this.InvocationStateInfo.State == PSInvocationState.Running);
        }

        private bool IsDisconnected()
        {
            return (this.InvocationStateInfo.State == PSInvocationState.Disconnected);
        }

        private void PipelineStateChanged(object source, PipelineStateEventArgs stateEventArgs)
        {
            PSInvocationStateInfo stateInfo = new PSInvocationStateInfo(stateEventArgs.PipelineStateInfo);
            this.SetStateChanged(stateInfo);
        }

        private void Prepare<TInput, TOutput>(PSDataCollection<TInput> input, PSDataCollection<TOutput> output, PSInvocationSettings settings, bool shouldCreateWorker)
        {
            lock (this.syncObject)
            {
                if (((this.psCommand == null) || (this.psCommand.Commands == null)) || (this.psCommand.Commands.Count == 0))
                {
                    throw PSTraceSource.NewInvalidOperationException(resBaseName, "NoCommandToInvoke", new object[0]);
                }
                this.AssertExecutionNotStarted();
                if (shouldCreateWorker)
                {
                    ObjectStreamBase base2;
                    this.invocationStateInfo = new PSInvocationStateInfo(PSInvocationState.Running, null);
                    if ((settings != null) && settings.FlowImpersonationPolicy)
                    {
						settings.WindowsIdentityToImpersonate = WindowsIdentity.GetCurrent(); //TODO: Not Implemented WindowsIdentity.GetCurrent(false);
                    }
                    if (input != null)
                    {
                        base2 = new PSDataCollectionStream<TInput>(this.instanceId, input);
                    }
                    else
                    {
                        base2 = new ObjectStream();
                        base2.Close();
                    }
                    ObjectStreamBase outputStream = new PSDataCollectionStream<TOutput>(this.instanceId, output);
                    this.worker = new Worker(base2, outputStream, settings, this);
                }
            }
            if (shouldCreateWorker)
            {
                this.RaiseStateChangeEvent(this.invocationStateInfo.Clone());
            }
        }

        private void RaiseStateChangeEvent(PSInvocationStateInfo stateInfo)
        {
            if (this.runspace is RemoteRunspace)
            {
                this.runspace.UpdateRunspaceAvailability(this.invocationStateInfo.State, true);
            }
            this.InvocationStateChanged.SafeInvoke<PSInvocationStateChangedEventArgs>(this, new PSInvocationStateChangedEventArgs(stateInfo));
        }

        internal void SetHadErrors(bool status)
        {
            this._hadErrors = status;
        }

        internal void SetIsNested(bool isNested)
        {
            this.AssertChangesAreAccepted();
            this.isNested = isNested;
        }

        private void SetRunspace(System.Management.Automation.Runspaces.Runspace runspace, bool owner)
        {
            RemoteRunspace runspace2 = runspace as RemoteRunspace;
            if (runspace2 == null)
            {
                this.rsConnection = runspace;
            }
            else
            {
                this.rsConnection = runspace2.RunspacePool;
                if (this.remotePowerShell != null)
                {
                    this.remotePowerShell.Clear();
                    this.remotePowerShell.Dispose();
                }
                this.remotePowerShell = new ClientRemotePowerShell(this, runspace2.RunspacePool.RemoteRunspacePoolInternal);
            }
            this.runspace = runspace;
            this.runspaceOwner = owner;
            this.runspacePool = null;
        }

        internal void SetStateChanged(PSInvocationStateInfo stateInfo)
        {
            PSInvocationState state;
            PowerShellAsyncResult result;
            PowerShellAsyncResult stopAsyncResult;
            PSInvocationStateInfo info = stateInfo;
            if ((this.worker != null) && (this.worker.CurrentlyRunningPipeline != null))
            {
                this.SetHadErrors(this.worker.CurrentlyRunningPipeline.HadErrors);
            }
            lock (this.syncObject)
            {
                state = this.invocationStateInfo.State;
                switch (this.invocationStateInfo.State)
                {
                    case PSInvocationState.Running:
                        if (stateInfo.State != PSInvocationState.Running)
                        {
                            goto Label_00C1;
                        }
                        return;

                    case PSInvocationState.Stopping:
                        if ((stateInfo.State != PSInvocationState.Running) && (stateInfo.State != PSInvocationState.Stopping))
                        {
                            break;
                        }
                        return;

                    case PSInvocationState.Stopped:
                    case PSInvocationState.Completed:
                    case PSInvocationState.Failed:
                        return;

                    default:
                        goto Label_00C1;
                }
                if ((stateInfo.State == PSInvocationState.Completed) || (stateInfo.State == PSInvocationState.Failed))
                {
                    info = new PSInvocationStateInfo(PSInvocationState.Stopped, stateInfo.Reason);
                }
            Label_00C1:
                result = this.invokeAsyncResult;
                stopAsyncResult = this.stopAsyncResult;
                this.invocationStateInfo = info;
            }
            bool flag = false;
            switch (this.invocationStateInfo.State)
            {
                case PSInvocationState.Running:
                    this.CloseInputBufferOnReconnection(state);
                    this.RaiseStateChangeEvent(this.invocationStateInfo.Clone());
                    return;

                case PSInvocationState.Stopping:
                    this.RaiseStateChangeEvent(this.invocationStateInfo.Clone());
                    return;

                case PSInvocationState.Stopped:
                case PSInvocationState.Completed:
                case PSInvocationState.Failed:
                    this.InternalClearSuppressExceptions();
                    try
                    {
                        try
                        {
                            if (this.runningExtraCommands)
                            {
                                if (result != null)
                                {
                                    result.SetAsCompleted(this.invocationStateInfo.Reason);
                                }
                                this.RaiseStateChangeEvent(this.invocationStateInfo.Clone());
                            }
                            else
                            {
                                this.RaiseStateChangeEvent(this.invocationStateInfo.Clone());
                                if (result != null)
                                {
                                    result.SetAsCompleted(this.invocationStateInfo.Reason);
                                }
                            }
                            if (stopAsyncResult != null)
                            {
                                stopAsyncResult.SetAsCompleted(null);
                            }
                        }
                        catch (Exception)
                        {
                            flag = true;
                            this.SetHadErrors(true);
                            throw;
                        }
                        return;
                    }
                    finally
                    {
                        if (flag && (stopAsyncResult != null))
                        {
                            stopAsyncResult.Release();
                        }
                    }
                    break;

                case PSInvocationState.Disconnected:
                    break;

                default:
                    return;
            }
            try
            {
                if (this.commandInvokedSynchronously && (result != null))
                {
                    result.SetAsCompleted(new RuntimeException(PowerShellStrings.DiscOnSyncCommand));
                }
                if (stopAsyncResult != null)
                {
                    stopAsyncResult.SetAsCompleted(null);
                }
                if (state != PSInvocationState.Disconnected)
                {
                    this.RaiseStateChangeEvent(this.invocationStateInfo.Clone());
                }
            }
            catch (Exception)
            {
                flag = true;
                this.SetHadErrors(true);
                throw;
            }
            finally
            {
                if (flag && (stopAsyncResult != null))
                {
                    stopAsyncResult.Release();
                }
            }
            this.connectCmdInfo = null;
        }

        public void Stop()
        {
            try
            {
                this.CoreStop(true, null, null).AsyncWaitHandle.WaitOne();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void StopHelper(object state)
        {
            Queue<PSInvocationStateInfo> queue = state as Queue<PSInvocationStateInfo>;
            while (queue.Count > 0)
            {
                PSInvocationStateInfo stateInfo = queue.Dequeue();
                this.SetStateChanged(stateInfo);
            }
            this.InternalClearSuppressExceptions();
        }

        private void StopThreadProc(object state)
        {
            try
            {
                this.StopHelper(state);
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw;
            }
        }

        internal PSObject ToPSObjectForRemoting()
        {
            PSObject obj2 = RemotingEncoder.CreateEmptyPSObject();
            Version pSRemotingProtocolVersion = RemotingEncoder.GetPSRemotingProtocolVersion(this.rsConnection as System.Management.Automation.Runspaces.RunspacePool);
            if (RemotingDecoder.ServerSupportsBatchInvocation(this.runspace) && (this.extraCommands.Count > 0))
            {
                List<PSObject> list = new List<PSObject>(this.extraCommands.Count);
                foreach (PSCommand command in this.extraCommands)
                {
                    PSObject item = RemotingEncoder.CreateEmptyPSObject();
                    item.Properties.Add(new PSNoteProperty("Cmds", this.CommandsAsListOfPSObjects(command.Commands, pSRemotingProtocolVersion)));
                    list.Add(item);
                }
                obj2.Properties.Add(new PSNoteProperty("ExtraCmds", list));
            }
			ArrayList privateData = new ArrayList();
			privateData.Add ("Mono");
			obj2.Properties.Add(new PSNoteProperty("PrivateData", privateData));

            List<PSObject> list2 = this.CommandsAsListOfPSObjects(this.Commands.Commands, pSRemotingProtocolVersion);
            obj2.Properties.Add(new PSNoteProperty("Cmds", list2));
            obj2.Properties.Add(new PSNoteProperty("IsNested", this.IsNested));
            obj2.Properties.Add(new PSNoteProperty("History", this.historyString));
            obj2.Properties.Add(new PSNoteProperty("RedirectShellErrorOutputPipe", this.RedirectShellErrorOutputPipe));
            return obj2;
        }

        private void VerifyThreadSettings(PSInvocationSettings settings, ApartmentState runspaceApartmentState, PSThreadOptions runspaceThreadOptions, bool isRemote)
        {
            ApartmentState apartmentState;
            if ((settings != null) && (settings.ApartmentState != ApartmentState.Unknown))
            {
                apartmentState = settings.ApartmentState;
            }
            else
            {
                apartmentState = runspaceApartmentState;
            }
            if (runspaceThreadOptions == PSThreadOptions.ReuseThread)
            {
                if (apartmentState != runspaceApartmentState)
                {
                    throw new InvalidOperationException(PowerShellStrings.ApartmentStateMismatch);
                }
            }
            else if (((runspaceThreadOptions == PSThreadOptions.UseCurrentThread) && !isRemote) && ((apartmentState != ApartmentState.Unknown) && (apartmentState != Thread.CurrentThread.GetApartmentState())))
            {
                throw new InvalidOperationException(PowerShellStrings.ApartmentStateMismatchCurrentThread);
            }
        }

        public PSCommand Commands
        {
            get
            {
                return this.psCommand;
            }
            set
            {
                if (value == null)
                {
                    throw PSTraceSource.NewArgumentNullException("Command");
                }
                lock (this.syncObject)
                {
                    this.AssertChangesAreAccepted();
                    this.psCommand = value.Clone();
                    this.psCommand.Owner = this;
                }
            }
        }

        internal PSDataCollection<DebugRecord> DebugBuffer
        {
            get
            {
                return this.informationalBuffers.Debug;
            }
            set
            {
                if (value == null)
                {
                    throw PSTraceSource.NewArgumentNullException("Debug");
                }
                lock (this.syncObject)
                {
                    this.AssertChangesAreAccepted();
                    this.informationalBuffers.Debug = value;
                }
            }
        }

        internal PSDataCollection<ErrorRecord> ErrorBuffer
        {
            get
            {
                return this.errorBuffer;
            }
            set
            {
                if (value == null)
                {
                    throw PSTraceSource.NewArgumentNullException("Error");
                }
                lock (this.syncObject)
                {
                    this.AssertChangesAreAccepted();
                    this.errorBuffer = value;
                    this.errorBufferOwner = false;
                }
            }
        }

        internal bool ErrorBufferOwner
        {
            get
            {
                return this.errorBufferOwner;
            }
            set
            {
                this.errorBufferOwner = value;
            }
        }

        internal Collection<PSCommand> ExtraCommands
        {
            get
            {
                return this.extraCommands;
            }
        }

        public bool HadErrors
        {
            get
            {
                return this._hadErrors;
            }
        }

        public string HistoryString
        {
            get
            {
                return this.historyString;
            }
            set
            {
                this.historyString = value;
            }
        }

        internal PSInformationalBuffers InformationalBuffers
        {
            get
            {
                return this.informationalBuffers;
            }
        }

        public Guid InstanceId
        {
            get
            {
                return this.instanceId;
            }
        }

        public PSInvocationStateInfo InvocationStateInfo
        {
            get
            {
                return this.invocationStateInfo;
            }
        }

        internal bool IsChild
        {
            get
            {
                return this.isChild;
            }
        }

        internal bool IsGetCommandMetadataSpecialPipeline
        {
            get
            {
                return this.isGetCommandMetadataSpecialPipeline;
            }
            set
            {
                this.isGetCommandMetadataSpecialPipeline = value;
            }
        }

        public bool IsNested
        {
            get
            {
                return this.isNested;
            }
        }

        public bool IsRunspaceOwner
        {
            get
            {
                return this.runspaceOwner;
            }
            internal set
            {
                this.runspaceOwner = value;
            }
        }

        internal bool OutputBufferOwner
        {
            get
            {
                return this.outputBufferOwner;
            }
            set
            {
                this.outputBufferOwner = value;
            }
        }

        internal PSDataCollection<ProgressRecord> ProgressBuffer
        {
            get
            {
                return this.informationalBuffers.Progress;
            }
            set
            {
                if (value == null)
                {
                    throw PSTraceSource.NewArgumentNullException("Progress");
                }
                lock (this.syncObject)
                {
                    this.AssertChangesAreAccepted();
                    this.informationalBuffers.Progress = value;
                }
            }
        }

        internal bool RedirectShellErrorOutputPipe
        {
            get
            {
                return this.redirectShellErrorOutputPipe;
            }
            set
            {
                this.redirectShellErrorOutputPipe = value;
            }
        }

        internal ClientRemotePowerShell RemotePowerShell
        {
            get
            {
                return this.remotePowerShell;
            }
        }

        internal bool RunningExtraCommands
        {
            get
            {
                return this.runningExtraCommands;
            }
        }

        public System.Management.Automation.Runspaces.Runspace Runspace
        {
            get
            {
                if ((this.runspace == null) && (this.runspacePool == null))
                {
                    lock (this.syncObject)
                    {
                        if ((this.runspace == null) && (this.runspacePool == null))
                        {
                            this.AssertChangesAreAccepted();
                            this.SetRunspace(RunspaceFactory.CreateRunspace(), true);
                            this.Runspace.Open();
                        }
                    }
                }
                return this.runspace;
            }
            set
            {
                lock (this.syncObject)
                {
                    this.AssertChangesAreAccepted();
                    if ((this.runspace != null) && this.runspaceOwner)
                    {
                        this.runspace.Dispose();
                        this.runspace = null;
                        this.runspaceOwner = false;
                    }
                    this.SetRunspace(value, false);
                }
            }
        }

        public System.Management.Automation.Runspaces.RunspacePool RunspacePool
        {
            get
            {
                return this.runspacePool;
            }
            set
            {
                lock (this.syncObject)
                {
                    this.AssertChangesAreAccepted();
                    if ((this.runspace != null) && this.runspaceOwner)
                    {
                        this.runspace.Dispose();
                        this.runspace = null;
                        this.runspaceOwner = false;
                    }
                    this.rsConnection = value;
                    this.runspacePool = value;
                    if (this.runspacePool.IsRemote)
                    {
                        if (this.remotePowerShell != null)
                        {
                            this.remotePowerShell.Clear();
                            this.remotePowerShell.Dispose();
                        }
                        this.remotePowerShell = new ClientRemotePowerShell(this, this.runspacePool.RemoteRunspacePoolInternal);
                    }
                    this.runspace = null;
                }
            }
        }

        public PSDataStreams Streams
        {
            get
            {
                return this.dataStreams;
            }
        }

        internal PSDataCollection<VerboseRecord> VerboseBuffer
        {
            get
            {
                return this.informationalBuffers.Verbose;
            }
            set
            {
                if (value == null)
                {
                    throw PSTraceSource.NewArgumentNullException("Verbose");
                }
                lock (this.syncObject)
                {
                    this.AssertChangesAreAccepted();
                    this.informationalBuffers.Verbose = value;
                }
            }
        }

        internal PSDataCollection<WarningRecord> WarningBuffer
        {
            get
            {
                return this.informationalBuffers.Warning;
            }
            set
            {
                if (value == null)
                {
                    throw PSTraceSource.NewArgumentNullException("Warning");
                }
                lock (this.syncObject)
                {
                    this.AssertChangesAreAccepted();
                    this.informationalBuffers.Warning = value;
                }
            }
        }

        private sealed class Worker
        {
            private Pipeline currentlyRunningPipeline;
            private ObjectStreamBase errorStream;
            private IAsyncResult getRunspaceAsyncResult;
            private ObjectStreamBase inputStream;
            private bool isNotActive;
            private ObjectStreamBase outputStream;
            private PSInvocationSettings settings;
            private PowerShell shell;
            private object syncObject = new object();

            internal Worker(ObjectStreamBase inputStream, ObjectStreamBase outputStream, PSInvocationSettings settings, PowerShell shell)
            {
                this.inputStream = inputStream;
                this.outputStream = outputStream;
                this.errorStream = new PSDataCollectionStream<ErrorRecord>(shell.instanceId, shell.errorBuffer);
                this.settings = settings;
                this.shell = shell;
            }

            internal bool ConstructPipelineAndDoWork(Runspace rs, bool performSyncInvoke)
            {
                this.shell.RunspaceAssigned.SafeInvoke<PSEventArgs<Runspace>>(this, new PSEventArgs<Runspace>(rs));
                LocalRunspace runspace = rs as LocalRunspace;
                lock (this.syncObject)
                {
                    if (this.isNotActive)
                    {
                        return false;
                    }
                    if (runspace == null)
                    {
                        throw PSTraceSource.NewNotImplementedException();
                    }
                    LocalPipeline pipeline = new LocalPipeline(runspace, this.shell.Commands.Commands, (this.settings != null) && this.settings.AddToHistory, this.shell.IsNested, this.inputStream, this.outputStream, this.errorStream, this.shell.informationalBuffers) {
                        IsChild = this.shell.IsChild
                    };
                    if (!string.IsNullOrEmpty(this.shell.HistoryString))
                    {
                        pipeline.SetHistoryString(this.shell.HistoryString);
                    }
                    pipeline.RedirectShellErrorOutputPipe = this.shell.RedirectShellErrorOutputPipe;
                    this.currentlyRunningPipeline = pipeline;
                    this.currentlyRunningPipeline.StateChanged += new EventHandler<PipelineStateEventArgs>(this.shell.PipelineStateChanged);
                }
                this.currentlyRunningPipeline.InvocationSettings = this.settings;
                if (performSyncInvoke)
                {
                    this.currentlyRunningPipeline.Invoke();
                }
                else
                {
                    this.currentlyRunningPipeline.InvokeAsync();
                }
                return true;
            }

            internal void CreateRunspaceIfNeededAndDoWork(object state)
            {
                Runspace rsToUse = state as Runspace;
                this.CreateRunspaceIfNeededAndDoWork(rsToUse, false);
            }

            internal void CreateRunspaceIfNeededAndDoWork(Runspace rsToUse, bool isSync)
            {
                try
                {
                    if (!(rsToUse is LocalRunspace))
                    {
                        lock (this.shell.syncObject)
                        {
                            if (this.shell.runspace != null)
                            {
                                rsToUse = this.shell.runspace;
                            }
                            else
                            {
                                Runspace runspace = null;
                                if ((this.settings != null) && (this.settings.Host != null))
                                {
                                    runspace = RunspaceFactory.CreateRunspace(this.settings.Host);
                                }
                                else
                                {
                                    runspace = RunspaceFactory.CreateRunspace();
                                }
                                this.shell.SetRunspace(runspace, true);
                                rsToUse = (LocalRunspace) runspace;
                                rsToUse.Open();
                            }
                        }
                    }
                    this.ConstructPipelineAndDoWork(rsToUse, isSync);
                }
                catch (Exception exception)
                {
                    lock (this.syncObject)
                    {
                        if (this.isNotActive)
                        {
                            return;
                        }
                        this.isNotActive = true;
                    }
                    this.shell.PipelineStateChanged(this, new PipelineStateEventArgs(new PipelineStateInfo(PipelineState.Failed, exception)));
                    if (isSync)
                    {
                        throw;
                    }
                    CommandProcessorBase.CheckForSevereException(exception);
                }
            }

            internal void InternalClearSuppressExceptions()
            {
                try
                {
                    if ((this.settings != null) && (this.settings.WindowsIdentityToImpersonate != null))
                    {
                        this.settings.WindowsIdentityToImpersonate.Dispose();
                        this.settings.WindowsIdentityToImpersonate = null;
                    }
                    this.inputStream.Close();
                    this.outputStream.Close();
                    this.errorStream.Close();
                    if (this.currentlyRunningPipeline == null)
                    {
                        return;
                    }
                    this.currentlyRunningPipeline.StateChanged -= new EventHandler<PipelineStateEventArgs>(this.shell.PipelineStateChanged);
                    if ((this.getRunspaceAsyncResult == null) && (this.shell.rsConnection == null))
                    {
                        this.currentlyRunningPipeline.Runspace.Close();
                    }
                    else
                    {
                        RunspacePool rsConnection = this.shell.rsConnection as RunspacePool;
                        if (rsConnection != null)
                        {
                            rsConnection.ReleaseRunspace(this.currentlyRunningPipeline.Runspace);
                        }
                    }
                    this.currentlyRunningPipeline.Dispose();
                }
                catch (ArgumentException)
                {
                }
                catch (InvalidOperationException)
                {
                }
                catch (InvalidRunspaceStateException)
                {
                }
                catch (InvalidRunspacePoolStateException)
                {
                }
                this.currentlyRunningPipeline = null;
            }

            internal void RunspaceAvailableCallback(IAsyncResult asyncResult)
            {
                try
                {
                    RunspacePool rsConnection = this.shell.rsConnection as RunspacePool;
                    Runspace rs = rsConnection.EndGetRunspace(asyncResult);
                    if (!this.ConstructPipelineAndDoWork(rs, false))
                    {
                        rsConnection.ReleaseRunspace(rs);
                    }
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                    lock (this.syncObject)
                    {
                        if (this.isNotActive)
                        {
                            return;
                        }
                        this.isNotActive = true;
                    }
                    this.shell.PipelineStateChanged(this, new PipelineStateEventArgs(new PipelineStateInfo(PipelineState.Failed, exception)));
                }
            }

            internal void Stop(bool isSyncCall)
            {
                lock (this.syncObject)
                {
                    if (this.isNotActive)
                    {
                        return;
                    }
                    this.isNotActive = true;
                    if (this.currentlyRunningPipeline != null)
                    {
                        if (isSyncCall)
                        {
                            this.currentlyRunningPipeline.Stop();
                        }
                        else
                        {
                            this.currentlyRunningPipeline.StopAsync();
                        }
                        return;
                    }
                    if (this.getRunspaceAsyncResult != null)
                    {
                        (this.shell.rsConnection as RunspacePool).CancelGetRunspace(this.getRunspaceAsyncResult);
                    }
                }
                Queue<PSInvocationStateInfo> state = new Queue<PSInvocationStateInfo>();
                state.Enqueue(new PSInvocationStateInfo(PSInvocationState.Stopped, null));
                if (isSyncCall)
                {
                    this.shell.StopHelper(state);
                }
                else
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(this.shell.StopThreadProc), state);
                }
            }

            internal Pipeline CurrentlyRunningPipeline
            {
                get
                {
                    return this.currentlyRunningPipeline;
                }
            }

            internal IAsyncResult GetRunspaceAsyncResult
            {
                get
                {
                    return this.getRunspaceAsyncResult;
                }
                set
                {
                    this.getRunspaceAsyncResult = value;
                }
            }
        }
    }
}

