namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Management.Automation.Internal;
    using System.Resources;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    public abstract class Cmdlet : InternalCommand
    {
        private string _parameterSetName = "";

        protected Cmdlet()
        {
        }

        protected virtual void BeginProcessing()
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
            }
        }

        internal override void DoBeginProcessing()
        {
            MshCommandRuntime commandRuntime = this.CommandRuntime as MshCommandRuntime;
            if (((commandRuntime != null) && (commandRuntime.UseTransaction != false)) && !base.Context.TransactionManager.HasTransaction)
            {
                string noTransactionStarted = TransactionStrings.NoTransactionStarted;
                if (base.Context.TransactionManager.IsLastTransactionCommitted)
                {
                    noTransactionStarted = TransactionStrings.NoTransactionStartedFromCommit;
                }
                else if (base.Context.TransactionManager.IsLastTransactionRolledBack)
                {
                    noTransactionStarted = TransactionStrings.NoTransactionStartedFromRollback;
                }
                throw new InvalidOperationException(noTransactionStarted);
            }
            this.BeginProcessing();
        }

        internal override void DoEndProcessing()
        {
            this.EndProcessing();
        }

        internal override void DoProcessRecord()
        {
            this.ProcessRecord();
        }

        internal override void DoStopProcessing()
        {
            this.StopProcessing();
        }

        protected virtual void EndProcessing()
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
            }
        }

        public virtual string GetResourceString(string baseName, string resourceId)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                if (string.IsNullOrEmpty(baseName))
                {
                    throw PSTraceSource.NewArgumentNullException("baseName");
                }
                if (string.IsNullOrEmpty(resourceId))
                {
                    throw PSTraceSource.NewArgumentNullException("resourceId");
                }
                ResourceManager resourceManager = ResourceManagerCache.GetResourceManager(base.GetType().Assembly, baseName);
                string str = null;
                try
                {
                    str = resourceManager.GetString(resourceId, Thread.CurrentThread.CurrentUICulture);
                }
                catch (MissingManifestResourceException)
                {
                    throw PSTraceSource.NewArgumentException("baseName", "GetErrorText", "ResourceBaseNameFailure", new object[] { baseName });
                }
                if (str == null)
                {
                    throw PSTraceSource.NewArgumentException("resourceId", "GetErrorText", "ResourceIdFailure", new object[] { resourceId });
                }
                return str;
            }
        }

        internal ArrayList GetResults()
        {
            if (this is PSCmdlet)
            {
                throw new InvalidOperationException(CommandBaseStrings.CannotInvokePSCmdletsDirectly);
            }
            ArrayList outputArrayList = new ArrayList();
            if (base.commandRuntime == null)
            {
                this.CommandRuntime = new DefaultCommandRuntime(outputArrayList);
            }
            this.BeginProcessing();
            this.ProcessRecord();
            this.EndProcessing();
            return outputArrayList;
        }

        public IEnumerable<T> Invoke<T>()
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                ArrayList results = this.GetResults();
                for (int i = 0; i < results.Count; i++)
                {
                    yield return (T) results[i];
                }
            }
        }

        public IEnumerable Invoke()
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                ArrayList results = this.GetResults();
                for (int i = 0; i < results.Count; i++)
                {
                    yield return results[i];
                }
            }
        }

        protected virtual void ProcessRecord()
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
            }
        }

        internal void SetParameterSetName(string parameterSetName)
        {
            this._parameterSetName = parameterSetName;
        }

        public bool ShouldContinue(string query, string caption)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                if (base.commandRuntime != null)
                {
                    return base.commandRuntime.ShouldContinue(query, caption);
                }
                return true;
            }
        }

        public bool ShouldContinue(string query, string caption, ref bool yesToAll, ref bool noToAll)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                if (base.commandRuntime != null)
                {
                    return base.commandRuntime.ShouldContinue(query, caption, ref yesToAll, ref noToAll);
                }
                return true;
            }
        }

        public bool ShouldProcess(string target)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                if (base.commandRuntime != null)
                {
                    return base.commandRuntime.ShouldProcess(target);
                }
                return true;
            }
        }

        public bool ShouldProcess(string target, string action)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                if (base.commandRuntime != null)
                {
                    return base.commandRuntime.ShouldProcess(target, action);
                }
                return true;
            }
        }

        public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                if (base.commandRuntime != null)
                {
                    return base.commandRuntime.ShouldProcess(verboseDescription, verboseWarning, caption);
                }
                return true;
            }
        }

        public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption, out ShouldProcessReason shouldProcessReason)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                if (base.commandRuntime != null)
                {
                    return base.commandRuntime.ShouldProcess(verboseDescription, verboseWarning, caption, out shouldProcessReason);
                }
                shouldProcessReason = ShouldProcessReason.None;
                return true;
            }
        }

        protected virtual void StopProcessing()
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
            }
        }

        public void ThrowTerminatingError(ErrorRecord errorRecord)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                if (errorRecord == null)
                {
                    throw new ArgumentNullException("errorRecord");
                }
                if (base.commandRuntime == null)
                {
                    if (errorRecord.Exception != null)
                    {
                        throw errorRecord.Exception;
                    }
                    throw new InvalidOperationException(errorRecord.ToString());
                }
                base.commandRuntime.ThrowTerminatingError(errorRecord);
            }
        }

        public bool TransactionAvailable()
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                if (base.commandRuntime == null)
                {
                    throw new NotImplementedException("TransactionAvailable");
                }
                return base.commandRuntime.TransactionAvailable();
            }
        }

        public void WriteCommandDetail(string text)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                if (base.commandRuntime == null)
                {
                    throw new NotImplementedException("WriteCommandDetail");
                }
                base.commandRuntime.WriteCommandDetail(text);
            }
        }

        public void WriteDebug(string text)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                if (base.commandRuntime == null)
                {
                    throw new NotImplementedException("WriteDebug");
                }
                base.commandRuntime.WriteDebug(text);
            }
        }

        public void WriteError(ErrorRecord errorRecord)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                if (base.commandRuntime == null)
                {
                    throw new NotImplementedException("WriteError");
                }
                base.commandRuntime.WriteError(errorRecord);
            }
        }

        public void WriteObject(object sendToPipeline)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                if (base.commandRuntime == null)
                {
                    throw new NotImplementedException("WriteObject");
                }
                base.commandRuntime.WriteObject(sendToPipeline);
            }
        }

        public void WriteObject(object sendToPipeline, bool enumerateCollection)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                if (base.commandRuntime == null)
                {
                    throw new NotImplementedException("WriteObject");
                }
                base.commandRuntime.WriteObject(sendToPipeline, enumerateCollection);
            }
        }

        public void WriteProgress(ProgressRecord progressRecord)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                if (base.commandRuntime == null)
                {
                    throw new NotImplementedException("WriteProgress");
                }
                base.commandRuntime.WriteProgress(progressRecord);
            }
        }

        internal void WriteProgress(long sourceId, ProgressRecord progressRecord)
        {
            if (base.commandRuntime == null)
            {
                throw new NotImplementedException("WriteProgress");
            }
            base.commandRuntime.WriteProgress(sourceId, progressRecord);
        }

        public void WriteVerbose(string text)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                if (base.commandRuntime == null)
                {
                    throw new NotImplementedException("WriteVerbose");
                }
                base.commandRuntime.WriteVerbose(text);
            }
        }

        public void WriteWarning(string text)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                if (base.commandRuntime == null)
                {
                    throw new NotImplementedException("WriteWarning");
                }
                base.commandRuntime.WriteWarning(text);
            }
        }

        internal string _ParameterSetName
        {
            get
            {
                return this._parameterSetName;
            }
        }

        public ICommandRuntime CommandRuntime
        {
            get
            {
                using (PSTransactionManager.GetEngineProtectionScope())
                {
                    return base.commandRuntime;
                }
            }
            set
            {
                using (PSTransactionManager.GetEngineProtectionScope())
                {
                    base.commandRuntime = value;
                }
            }
        }

        public PSTransactionContext CurrentPSTransaction
        {
            get
            {
                if (base.commandRuntime == null)
                {
                    throw new NotImplementedException("CurrentPSTransaction");
                }
                return base.commandRuntime.CurrentPSTransaction;
            }
        }

        public bool Stopping
        {
            get
            {
                using (PSTransactionManager.GetEngineProtectionScope())
                {
                    return base.IsStopping;
                }
            }
        }

        
    }
}

