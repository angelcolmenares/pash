namespace System.Management.Automation.Tracing
{
    using System;

    public abstract class BaseChannelWriter : IDisposable
    {
        private bool disposed;

        protected BaseChannelWriter()
        {
        }

        public virtual void Dispose()
        {
            if (!this.disposed)
            {
                GC.SuppressFinalize(this);
                this.disposed = true;
            }
        }

        public virtual bool TraceCritical(PowerShellTraceEvent traceEvent, PowerShellTraceOperationCode operationCode, PowerShellTraceTask task, params object[] args)
        {
            return true;
        }

        public virtual bool TraceDebug(PowerShellTraceEvent traceEvent, PowerShellTraceOperationCode operationCode, PowerShellTraceTask task, params object[] args)
        {
            return true;
        }

        public virtual bool TraceError(PowerShellTraceEvent traceEvent, PowerShellTraceOperationCode operationCode, PowerShellTraceTask task, params object[] args)
        {
            return true;
        }

        public virtual bool TraceInformational(PowerShellTraceEvent traceEvent, PowerShellTraceOperationCode operationCode, PowerShellTraceTask task, params object[] args)
        {
            return true;
        }

        public virtual bool TraceLogAlways(PowerShellTraceEvent traceEvent, PowerShellTraceOperationCode operationCode, PowerShellTraceTask task, params object[] args)
        {
            return true;
        }

        public virtual bool TraceVerbose(PowerShellTraceEvent traceEvent, PowerShellTraceOperationCode operationCode, PowerShellTraceTask task, params object[] args)
        {
            return true;
        }

        public virtual bool TraceWarning(PowerShellTraceEvent traceEvent, PowerShellTraceOperationCode operationCode, PowerShellTraceTask task, params object[] args)
        {
            return true;
        }

        public virtual PowerShellTraceKeywords Keywords
        {
            get
            {
                return PowerShellTraceKeywords.None;
            }
            set
            {
            }
        }
    }
}

