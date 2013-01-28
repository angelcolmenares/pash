namespace System.Management.Automation.Tracing
{
    using System;
    using System.Diagnostics.Eventing;

    public sealed class PowerShellChannelWriter : BaseChannelWriter
    {
        private PowerShellTraceKeywords _keywords;
        private static readonly EventProvider _provider = new EventProvider(new Guid("A0C1853B-5C40-4b15-8766-3CF1C58F985A"));
        private readonly PowerShellTraceChannel _traceChannel;
        private bool disposed;

        internal PowerShellChannelWriter(PowerShellTraceChannel traceChannel, PowerShellTraceKeywords keywords)
        {
            this._traceChannel = traceChannel;
            this._keywords = keywords;
        }

        public override void Dispose()
        {
            if (!this.disposed)
            {
                GC.SuppressFinalize(this);
                this.disposed = true;
            }
        }

        private bool Trace(PowerShellTraceEvent traceEvent, PowerShellTraceLevel level, PowerShellTraceOperationCode operationCode, PowerShellTraceTask task, params object[] args)
        {
            EventDescriptor eventDescriptor = new EventDescriptor((int) traceEvent, 1, (byte) this._traceChannel, (byte) level, (byte) operationCode, (int) task, (long) this._keywords);
            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == null)
                    {
                        args[i] = string.Empty;
                    }
                }
            }
            return _provider.WriteEvent(ref eventDescriptor, args);
        }

        public override bool TraceCritical(PowerShellTraceEvent traceEvent, PowerShellTraceOperationCode operationCode, PowerShellTraceTask task, params object[] args)
        {
            return this.Trace(traceEvent, PowerShellTraceLevel.Critical, operationCode, task, args);
        }

        public override bool TraceDebug(PowerShellTraceEvent traceEvent, PowerShellTraceOperationCode operationCode, PowerShellTraceTask task, params object[] args)
        {
            return this.Trace(traceEvent, PowerShellTraceLevel.Informational, operationCode, task, args);
        }

        public override bool TraceError(PowerShellTraceEvent traceEvent, PowerShellTraceOperationCode operationCode, PowerShellTraceTask task, params object[] args)
        {
            return this.Trace(traceEvent, PowerShellTraceLevel.Error, operationCode, task, args);
        }

        public override bool TraceInformational(PowerShellTraceEvent traceEvent, PowerShellTraceOperationCode operationCode, PowerShellTraceTask task, params object[] args)
        {
            return this.Trace(traceEvent, PowerShellTraceLevel.Informational, operationCode, task, args);
        }

        public override bool TraceLogAlways(PowerShellTraceEvent traceEvent, PowerShellTraceOperationCode operationCode, PowerShellTraceTask task, params object[] args)
        {
            return this.Trace(traceEvent, PowerShellTraceLevel.LogAlways, operationCode, task, args);
        }

        public override bool TraceVerbose(PowerShellTraceEvent traceEvent, PowerShellTraceOperationCode operationCode, PowerShellTraceTask task, params object[] args)
        {
            return this.Trace(traceEvent, PowerShellTraceLevel.Verbose, operationCode, task, args);
        }

        public override bool TraceWarning(PowerShellTraceEvent traceEvent, PowerShellTraceOperationCode operationCode, PowerShellTraceTask task, params object[] args)
        {
            return this.Trace(traceEvent, PowerShellTraceLevel.Warning, operationCode, task, args);
        }

        public override PowerShellTraceKeywords Keywords
        {
            get
            {
                return this._keywords;
            }
            set
            {
                this._keywords = value;
            }
        }
    }
}

