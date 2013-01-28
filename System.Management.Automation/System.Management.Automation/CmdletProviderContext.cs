namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation.Provider;
    using System.Runtime.InteropServices;

    internal sealed class CmdletProviderContext
    {
        private Collection<string> _exclude;
        private string _filter;
        private Collection<string> _include;
        private CommandOrigin _origin;
        private Collection<ErrorRecord> accumulatedErrorObjects;
        private Collection<PSObject> accumulatedObjects;
        private Cmdlet command;
        private CmdletProviderContext copiedContext;
        private PSCredential credentials;
        private PSDriveInfo drive;
        private object dynamicParameters;
        private System.Management.Automation.ExecutionContext executionContext;
        private bool force;
        private CmdletProvider providerInstance;
        private bool stopping;
        private Collection<CmdletProviderContext> stopReferrals;
        private bool streamErrors;
        private bool streamObjects;
        private bool suppressWildcardExpansion;
        [TraceSource("CmdletProviderContext", "The context under which a core command is being run.")]
        private static PSTraceSource tracer = PSTraceSource.GetTracer("CmdletProviderContext", "The context under which a core command is being run.");

        internal CmdletProviderContext(Cmdlet command)
        {
            this.credentials = PSCredential.Empty;
            this._origin = CommandOrigin.Internal;
            this.accumulatedObjects = new Collection<PSObject>();
            this.accumulatedErrorObjects = new Collection<ErrorRecord>();
            this.stopReferrals = new Collection<CmdletProviderContext>();
            if (command == null)
            {
                throw PSTraceSource.NewArgumentNullException("command");
            }
            this.command = command;
            this._origin = command.CommandOrigin;
            if (command.Context == null)
            {
                throw PSTraceSource.NewArgumentException("command.Context");
            }
            this.executionContext = command.Context;
            this.streamObjects = true;
            this.streamErrors = true;
        }

        internal CmdletProviderContext(CmdletProviderContext contextToCopyFrom)
        {
            this.credentials = PSCredential.Empty;
            this._origin = CommandOrigin.Internal;
            this.accumulatedObjects = new Collection<PSObject>();
            this.accumulatedErrorObjects = new Collection<ErrorRecord>();
            this.stopReferrals = new Collection<CmdletProviderContext>();
            if (contextToCopyFrom == null)
            {
                throw PSTraceSource.NewArgumentNullException("contextToCopyFrom");
            }
            this.executionContext = contextToCopyFrom.ExecutionContext;
            this.command = contextToCopyFrom.command;
            if (contextToCopyFrom.Credential != null)
            {
                this.credentials = contextToCopyFrom.Credential;
            }
            this.drive = contextToCopyFrom.Drive;
            this.force = (bool) contextToCopyFrom.Force;
            this.CopyFilters(contextToCopyFrom);
            this.suppressWildcardExpansion = contextToCopyFrom.SuppressWildcardExpansion;
            this.dynamicParameters = contextToCopyFrom.DynamicParameters;
            this._origin = contextToCopyFrom._origin;
            this.stopping = contextToCopyFrom.Stopping;
            contextToCopyFrom.StopReferrals.Add(this);
            this.copiedContext = contextToCopyFrom;
        }

        internal CmdletProviderContext(System.Management.Automation.ExecutionContext executionContext)
        {
            this.credentials = PSCredential.Empty;
            this._origin = CommandOrigin.Internal;
            this.accumulatedObjects = new Collection<PSObject>();
            this.accumulatedErrorObjects = new Collection<ErrorRecord>();
            this.stopReferrals = new Collection<CmdletProviderContext>();
            if (executionContext == null)
            {
                throw PSTraceSource.NewArgumentNullException("executionContext");
            }
            this.executionContext = executionContext;
            this._origin = CommandOrigin.Internal;
            this.drive = executionContext.EngineSessionState.CurrentDrive;
            if ((executionContext.CurrentCommandProcessor != null) && (executionContext.CurrentCommandProcessor.Command is Cmdlet))
            {
                this.command = (Cmdlet) executionContext.CurrentCommandProcessor.Command;
            }
        }

        internal CmdletProviderContext(System.Management.Automation.ExecutionContext executionContext, CommandOrigin origin)
        {
            this.credentials = PSCredential.Empty;
            this._origin = CommandOrigin.Internal;
            this.accumulatedObjects = new Collection<PSObject>();
            this.accumulatedErrorObjects = new Collection<ErrorRecord>();
            this.stopReferrals = new Collection<CmdletProviderContext>();
            if (executionContext == null)
            {
                throw PSTraceSource.NewArgumentNullException("executionContext");
            }
            this.executionContext = executionContext;
            this._origin = origin;
        }

        internal CmdletProviderContext(PSCmdlet command, PSCredential credentials)
        {
            this.credentials = PSCredential.Empty;
            this._origin = CommandOrigin.Internal;
            this.accumulatedObjects = new Collection<PSObject>();
            this.accumulatedErrorObjects = new Collection<ErrorRecord>();
            this.stopReferrals = new Collection<CmdletProviderContext>();
            if (command == null)
            {
                throw PSTraceSource.NewArgumentNullException("command");
            }
            this.command = command;
            this._origin = command.CommandOrigin;
            if (credentials != null)
            {
                this.credentials = credentials;
            }
            if (command.Host == null)
            {
                throw PSTraceSource.NewArgumentException("command.Host");
            }
            if (command.Context == null)
            {
                throw PSTraceSource.NewArgumentException("command.Context");
            }
            this.executionContext = command.Context;
            this.streamObjects = true;
            this.streamErrors = true;
        }

        internal CmdletProviderContext(PSCmdlet command, PSCredential credentials, PSDriveInfo drive)
        {
            this.credentials = PSCredential.Empty;
            this._origin = CommandOrigin.Internal;
            this.accumulatedObjects = new Collection<PSObject>();
            this.accumulatedErrorObjects = new Collection<ErrorRecord>();
            this.stopReferrals = new Collection<CmdletProviderContext>();
            if (command == null)
            {
                throw PSTraceSource.NewArgumentNullException("command");
            }
            this.command = command;
            this._origin = command.CommandOrigin;
            if (credentials != null)
            {
                this.credentials = credentials;
            }
            this.drive = drive;
            if (command.Host == null)
            {
                throw PSTraceSource.NewArgumentException("command.Host");
            }
            if (command.Context == null)
            {
                throw PSTraceSource.NewArgumentException("command.Context");
            }
            this.executionContext = command.Context;
            this.streamObjects = true;
            this.streamErrors = true;
        }

        private void CopyFilters(CmdletProviderContext context)
        {
            this._include = context.Include;
            this._exclude = context.Exclude;
            this._filter = context.Filter;
        }

        internal Collection<ErrorRecord> GetAccumulatedErrorObjects()
        {
            Collection<ErrorRecord> accumulatedErrorObjects = this.accumulatedErrorObjects;
            this.accumulatedErrorObjects = new Collection<ErrorRecord>();
            return accumulatedErrorObjects;
        }

        internal Collection<PSObject> GetAccumulatedObjects()
        {
            Collection<PSObject> accumulatedObjects = this.accumulatedObjects;
            this.accumulatedObjects = new Collection<PSObject>();
            return accumulatedObjects;
        }

        internal bool HasErrors()
        {
            bool flag = false;
            if ((this.accumulatedErrorObjects != null) && (this.accumulatedErrorObjects.Count > 0))
            {
                flag = true;
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        internal void RemoveStopReferral()
        {
            if (this.copiedContext != null)
            {
                this.copiedContext.StopReferrals.Remove(this);
            }
        }

        internal void SetFilters(Collection<string> include, Collection<string> exclude, string filter)
        {
            this._include = include;
            this._exclude = exclude;
            this._filter = filter;
        }

        internal bool ShouldContinue(string query, string caption)
        {
            bool flag = true;
            if (this.command != null)
            {
                flag = this.command.ShouldContinue(query, caption);
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        internal bool ShouldContinue(string query, string caption, ref bool yesToAll, ref bool noToAll)
        {
            bool flag = true;
            if (this.command != null)
            {
                flag = this.command.ShouldContinue(query, caption, ref yesToAll, ref noToAll);
            }
            else
            {
                yesToAll = false;
                noToAll = false;
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        internal bool ShouldProcess(string target)
        {
            bool flag = true;
            if (this.command != null)
            {
                flag = this.command.ShouldProcess(target);
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        internal bool ShouldProcess(string target, string action)
        {
            bool flag = true;
            if (this.command != null)
            {
                flag = this.command.ShouldProcess(target, action);
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        internal bool ShouldProcess(string verboseDescription, string verboseWarning, string caption)
        {
            bool flag = true;
            if (this.command != null)
            {
                flag = this.command.ShouldProcess(verboseDescription, verboseWarning, caption);
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        internal bool ShouldProcess(string verboseDescription, string verboseWarning, string caption, out ShouldProcessReason shouldProcessReason)
        {
            bool flag = true;
            if (this.command != null)
            {
                flag = this.command.ShouldProcess(verboseDescription, verboseWarning, caption, out shouldProcessReason);
            }
            else
            {
                shouldProcessReason = ShouldProcessReason.None;
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        internal void StopProcessing()
        {
            this.stopping = true;
            if (this.providerInstance != null)
            {
                this.providerInstance.StopProcessing();
            }
            foreach (CmdletProviderContext context in this.StopReferrals)
            {
                context.StopProcessing();
            }
        }

        internal void ThrowFirstErrorOrDoNothing()
        {
            this.ThrowFirstErrorOrDoNothing(true);
        }

        internal void ThrowFirstErrorOrDoNothing(bool wrapExceptionInProviderException)
        {
            if (this.HasErrors())
            {
                Collection<ErrorRecord> accumulatedErrorObjects = this.GetAccumulatedErrorObjects();
                if ((accumulatedErrorObjects != null) && (accumulatedErrorObjects.Count > 0))
                {
                    if (!wrapExceptionInProviderException)
                    {
                        throw accumulatedErrorObjects[0].Exception;
                    }
                    ProviderInfo provider = null;
                    if (this.ProviderInstance != null)
                    {
                        provider = this.ProviderInstance.ProviderInfo;
                    }
                    ProviderInvocationException exception = new ProviderInvocationException(provider, accumulatedErrorObjects[0]);
                    MshLog.LogProviderHealthEvent(this.ExecutionContext, (provider != null) ? provider.Name : "unknown provider", exception, Severity.Warning);
                    throw exception;
                }
            }
        }

        public bool TransactionAvailable()
        {
            return ((this.command != null) && this.command.TransactionAvailable());
        }

        internal void WriteDebug(string text)
        {
            if (this.command != null)
            {
                this.command.WriteDebug(text);
            }
        }

        internal void WriteError(ErrorRecord errorRecord)
        {
            if (this.Stopping)
            {
                PipelineStoppedException exception = new PipelineStoppedException();
                throw exception;
            }
            if (this.streamErrors)
            {
                if (this.command == null)
                {
                    throw PSTraceSource.NewInvalidOperationException("SessionStateStrings", "ErrorStreamingNotEnabled", new object[0]);
                }
                tracer.WriteLine("Writing error package to command error pipe", new object[0]);
                this.command.WriteError(errorRecord);
            }
            else
            {
                this.accumulatedErrorObjects.Add(errorRecord);
                if ((errorRecord.ErrorDetails != null) && (errorRecord.ErrorDetails.TextLookupError != null))
                {
                    Exception textLookupError = errorRecord.ErrorDetails.TextLookupError;
                    errorRecord.ErrorDetails.TextLookupError = null;
                    MshLog.LogProviderHealthEvent(this.ExecutionContext, this.ProviderInstance.ProviderInfo.Name, textLookupError, Severity.Warning);
                }
            }
        }

        internal void WriteErrorsToContext(CmdletProviderContext errorContext)
        {
            if (errorContext == null)
            {
                throw PSTraceSource.NewArgumentNullException("errorContext");
            }
            if (this.HasErrors())
            {
                foreach (ErrorRecord record in this.GetAccumulatedErrorObjects())
                {
                    errorContext.WriteError(record);
                }
            }
        }

        internal void WriteObject(object obj)
        {
            if (this.Stopping)
            {
                PipelineStoppedException exception = new PipelineStoppedException();
                throw exception;
            }
            if (this.streamObjects)
            {
                if (this.command == null)
                {
                    throw PSTraceSource.NewInvalidOperationException("SessionStateStrings", "OutputStreamingNotEnabled", new object[0]);
                }
                tracer.WriteLine("Writing to command pipeline", new object[0]);
                this.command.WriteObject(obj);
            }
            else
            {
                tracer.WriteLine("Writing to accumulated objects", new object[0]);
                PSObject item = PSObject.AsPSObject(obj);
                this.accumulatedObjects.Add(item);
            }
        }

        internal void WriteProgress(ProgressRecord record)
        {
            if (this.command != null)
            {
                this.command.WriteProgress(record);
            }
        }

        internal void WriteVerbose(string text)
        {
            if (this.command != null)
            {
                this.command.WriteVerbose(text);
            }
        }

        internal void WriteWarning(string text)
        {
            if (this.command != null)
            {
                this.command.WriteWarning(text);
            }
        }

        internal PSCredential Credential
        {
            get
            {
                PSCredential credentials = this.credentials;
                if ((this.credentials == null) && (this.drive != null))
                {
                    credentials = this.drive.Credential;
                }
                return credentials;
            }
        }

        public PSTransactionContext CurrentPSTransaction
        {
            get
            {
                if (this.command != null)
                {
                    return this.command.CurrentPSTransaction;
                }
                return null;
            }
        }

        internal PSDriveInfo Drive
        {
            get
            {
                return this.drive;
            }
            set
            {
                this.drive = value;
            }
        }

        internal object DynamicParameters
        {
            get
            {
                return this.dynamicParameters;
            }
            set
            {
                this.dynamicParameters = value;
            }
        }

        internal Collection<string> Exclude
        {
            get
            {
                return this._exclude;
            }
        }

        internal System.Management.Automation.ExecutionContext ExecutionContext
        {
            get
            {
                return this.executionContext;
            }
        }

        internal string Filter
        {
            get
            {
                return this._filter;
            }
            set
            {
                this._filter = value;
            }
        }

        internal SwitchParameter Force
        {
            get
            {
                return this.force;
            }
            set
            {
                this.force = (bool) value;
            }
        }

        internal Collection<string> Include
        {
            get
            {
                return this._include;
            }
        }

        internal InvocationInfo MyInvocation
        {
            get
            {
                return this.command.MyInvocation;
            }
        }

        internal CommandOrigin Origin
        {
            get
            {
                return this._origin;
            }
        }

        internal bool PassThru
        {
            get
            {
                return this.streamObjects;
            }
            set
            {
                this.streamObjects = value;
            }
        }

        internal CmdletProvider ProviderInstance
        {
            get
            {
                return this.providerInstance;
            }
            set
            {
                this.providerInstance = value;
            }
        }

        internal bool Stopping
        {
            get
            {
                return this.stopping;
            }
        }

        internal Collection<CmdletProviderContext> StopReferrals
        {
            get
            {
                return this.stopReferrals;
            }
        }

        public bool SuppressWildcardExpansion
        {
            get
            {
                return this.suppressWildcardExpansion;
            }
            internal set
            {
                this.suppressWildcardExpansion = value;
            }
        }

        internal bool UseTransaction
        {
            get
            {
                if ((this.command != null) && (this.command.CommandRuntime != null))
                {
                    MshCommandRuntime commandRuntime = this.command.CommandRuntime as MshCommandRuntime;
                    if (commandRuntime != null)
                    {
                        return (bool) commandRuntime.UseTransaction;
                    }
                }
                return false;
            }
        }
    }
}

