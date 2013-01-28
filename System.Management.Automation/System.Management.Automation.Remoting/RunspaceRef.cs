namespace System.Management.Automation.Remoting
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Runspaces.Internal;
    using System.Runtime.InteropServices;

    internal class RunspaceRef
    {
        private object _localSyncObject;
        private ObjectRef<System.Management.Automation.Runspaces.Runspace> _runspaceRef;
        private bool _stopInvoke;
        private static RobustConnectionProgress RCProgress = new RobustConnectionProgress();

        internal RunspaceRef(System.Management.Automation.Runspaces.Runspace runspace)
        {
            this._runspaceRef = new ObjectRef<System.Management.Automation.Runspaces.Runspace>(runspace);
            this._stopInvoke = false;
            this._localSyncObject = new object();
        }

        internal Pipeline CreateNestedPipeline()
        {
            return this._runspaceRef.Value.CreateNestedPipeline();
        }

        internal Pipeline CreatePipeline()
        {
            return this._runspaceRef.Value.CreatePipeline();
        }

        internal Pipeline CreatePipeline(string line, bool addToHistory, bool useNestedPipelines)
        {
            Pipeline pipeline = null;
            EventHandler<DataAddedEventArgs> handler = null;
            if (this.IsRunspaceOverridden)
            {
                if (((this._runspaceRef.Value is RemoteRunspace) && !string.IsNullOrEmpty(line)) && string.Equals(line.Trim(), "exit", StringComparison.OrdinalIgnoreCase))
                {
                    line = "Exit-PSSession";
                }
                PSCommand command = this.ParsePsCommandUsingScriptBlock(line, null);
                if (command != null)
                {
                    pipeline = useNestedPipelines ? this._runspaceRef.Value.CreateNestedPipeline(command.Commands[0].CommandText, addToHistory) : this._runspaceRef.Value.CreatePipeline(command.Commands[0].CommandText, addToHistory);
                    pipeline.Commands.Clear();
                    foreach (Command command2 in command.Commands)
                    {
                        pipeline.Commands.Add(command2);
                    }
                }
            }
            if (pipeline == null)
            {
                pipeline = useNestedPipelines ? this._runspaceRef.Value.CreateNestedPipeline(line, addToHistory) : this._runspaceRef.Value.CreatePipeline(line, addToHistory);
            }
            RemotePipeline pipeline2 = pipeline as RemotePipeline;
            if (this.IsRunspaceOverridden && (pipeline2 != null))
            {
                PowerShell powerShell = pipeline2.PowerShell;
                if (powerShell.RemotePowerShell != null)
                {
                    powerShell.RemotePowerShell.RCConnectionNotification += new EventHandler<PSConnectionRetryStatusEventArgs>(this.HandleRCConnectionNotification);
                }
                if (handler == null)
                {
                    handler = delegate (object sender, DataAddedEventArgs eventArgs) {
                        RemoteRunspace runspace = this._runspaceRef.Value as RemoteRunspace;
                        PSDataCollection<ErrorRecord> datas = sender as PSDataCollection<ErrorRecord>;
                        if (((runspace != null) && (datas != null)) && (runspace.RunspacePool.RemoteRunspacePoolInternal.Host != null))
                        {
                            foreach (ErrorRecord record in datas.ReadAll())
                            {
                                runspace.RunspacePool.RemoteRunspacePoolInternal.Host.UI.WriteErrorLine(record.ToString());
                            }
                        }
                    };
                }
                powerShell.ErrorBuffer.DataAdded += handler;
            }
            pipeline.SetHistoryString(line);
            return pipeline;
        }

        internal PSCommand CreatePsCommand(string line, bool isScript, bool? useNewScope)
        {
            if (!this.IsRunspaceOverridden)
            {
                return this.CreatePsCommandNotOverriden(line, isScript, useNewScope);
            }
            PSCommand command = this.ParsePsCommandUsingScriptBlock(line, useNewScope);
            if (command == null)
            {
                return this.CreatePsCommandNotOverriden(line, isScript, useNewScope);
            }
            return command;
        }

        private PSCommand CreatePsCommandNotOverriden(string line, bool isScript, bool? useNewScope)
        {
            PSCommand command = new PSCommand();
            if (isScript)
            {
                if (useNewScope.HasValue)
                {
                    command.AddScript(line, useNewScope.Value);
                    return command;
                }
                command.AddScript(line);
                return command;
            }
            if (useNewScope.HasValue)
            {
                command.AddCommand(line, useNewScope.Value);
                return command;
            }
            command.AddCommand(line);
            return command;
        }

        private void HandleHostCall(object sender, RemoteDataEventArgs<RemoteHostCall> eventArgs)
        {
            ClientRemotePowerShell.ExitHandler(sender, eventArgs);
        }

        private void HandleRCConnectionNotification(object sender, PSConnectionRetryStatusEventArgs e)
        {
            switch (e.Notification)
            {
                case PSConnectionRetryStatus.NetworkFailureDetected:
                    this.StartProgressBar((long) sender.GetHashCode(), e.ComputerName, e.MaxRetryConnectionTime / 0x3e8);
                    return;

                case PSConnectionRetryStatus.ConnectionRetryAttempt:
                    break;

                case PSConnectionRetryStatus.ConnectionRetrySucceeded:
                case PSConnectionRetryStatus.AutoDisconnectStarting:
                    this.StopProgressBar((long) sender.GetHashCode());
                    return;

                case PSConnectionRetryStatus.AutoDisconnectSucceeded:
                case PSConnectionRetryStatus.InternalErrorAbort:
                    this.WriteRCFailedError();
                    this.StopProgressBar((long) sender.GetHashCode());
                    break;

                default:
                    return;
            }
        }

        internal void Override(RemoteRunspace remoteRunspace)
        {
            bool isRunspacePushed = false;
            this.Override(remoteRunspace, null, out isRunspacePushed);
        }

        internal void Override(RemoteRunspace remoteRunspace, object syncObject, out bool isRunspacePushed)
        {
            lock (this._localSyncObject)
            {
                this._stopInvoke = false;
            }
            try
            {
                if (syncObject != null)
                {
                    lock (syncObject)
                    {
                        this._runspaceRef.Override(remoteRunspace);
                        isRunspacePushed = true;
                        goto Label_0063;
                    }
                }
                this._runspaceRef.Override(remoteRunspace);
                isRunspacePushed = true;
            Label_0063:
                using (PowerShell shell = PowerShell.Create())
                {
                    shell.AddCommand("Get-Command");
                    shell.AddParameter("Name", new string[] { "Out-Default", "Exit-PSSession" });
                    shell.Runspace = this._runspaceRef.Value;
                    bool flag2 = this._runspaceRef.Value.GetRemoteProtocolVersion() == RemotingConstants.ProtocolVersionWin7RC;
                    shell.IsGetCommandMetadataSpecialPipeline = !flag2;
                    int num = flag2 ? 2 : 3;
                    shell.RemotePowerShell.HostCallReceived += new EventHandler<RemoteDataEventArgs<RemoteHostCall>>(this.HandleHostCall);
                    IAsyncResult asyncResult = shell.BeginInvoke();
                    PSDataCollection<PSObject> datas = new PSDataCollection<PSObject>();
                    while (!this._stopInvoke)
                    {
                        asyncResult.AsyncWaitHandle.WaitOne(0x3e8);
                        if (asyncResult.IsCompleted)
                        {
                            datas = shell.EndInvoke(asyncResult);
                            break;
                        }
                    }
                    if ((shell.Streams.Error.Count > 0) || (datas.Count < num))
                    {
                        throw RemoteHostExceptions.NewRemoteRunspaceDoesNotSupportPushRunspaceException();
                    }
                    return;
                }
            }
            catch (Exception)
            {
                this._runspaceRef.Revert();
                isRunspacePushed = false;
                throw;
            }
        }

        private PSCommand ParsePsCommandUsingScriptBlock(string line, bool? useLocalScope)
        {
            try
            {
                ExecutionContext executionContext = this._runspaceRef.OldValue.ExecutionContext;
                return ScriptBlock.Create(executionContext, line).GetPowerShell(executionContext, useLocalScope, new object[0]).Commands;
            }
            catch (ScriptBlockToPowerShellNotSupportedException exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
            }
            catch (RuntimeException exception2)
            {
                CommandProcessorBase.CheckForSevereException(exception2);
            }
            return null;
        }

        internal void Revert()
        {
            this._runspaceRef.Revert();
            lock (this._localSyncObject)
            {
                this._stopInvoke = true;
            }
        }

        private void StartProgressBar(long sourceId, string computerName, int totalSeconds)
        {
            RemoteRunspace runspace = this._runspaceRef.Value as RemoteRunspace;
            if (runspace != null)
            {
                RCProgress.StartProgress(sourceId, computerName, totalSeconds, runspace.RunspacePool.RemoteRunspacePoolInternal.Host);
            }
        }

        private void StopProgressBar(long sourceId)
        {
            RCProgress.StopProgress(sourceId);
        }

        private void WriteRCFailedError()
        {
            RemoteRunspace runspace = this._runspaceRef.Value as RemoteRunspace;
            if ((runspace != null) && (runspace.RunspacePool.RemoteRunspacePoolInternal.Host != null))
            {
                runspace.RunspacePool.RemoteRunspacePoolInternal.Host.UI.WriteErrorLine(StringUtil.Format(RemotingErrorIdStrings.RCAutoDisconnectingError, runspace.ConnectionInfo.ComputerName));
            }
        }

        internal bool IsRunspaceOverridden
        {
            get
            {
                return this._runspaceRef.IsOverridden;
            }
        }

        internal System.Management.Automation.Runspaces.Runspace OldRunspace
        {
            get
            {
                return this._runspaceRef.OldValue;
            }
        }

        internal System.Management.Automation.Runspaces.Runspace Runspace
        {
            get
            {
                return this._runspaceRef.Value;
            }
        }
    }
}

