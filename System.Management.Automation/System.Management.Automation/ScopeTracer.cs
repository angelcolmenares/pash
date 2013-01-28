namespace System.Management.Automation
{
    using System;
    using System.Text;
    using System.Threading;

    internal class ScopeTracer : IDisposable
    {
        private PSTraceSourceOptions _flag;
        private string _leavingScopeFormatter;
        private string _scopeName;
        private PSTraceSource _tracer;

        internal ScopeTracer(PSTraceSource tracer, PSTraceSourceOptions flag, string scopeOutputFormatter, string leavingScopeFormatter, string scopeName)
        {
            this._tracer = tracer;
            this.ScopeTracerHelper(flag, scopeOutputFormatter, leavingScopeFormatter, scopeName, "", new object[0]);
        }

        internal ScopeTracer(PSTraceSource tracer, PSTraceSourceOptions flag, string scopeOutputFormatter, string leavingScopeFormatter, string scopeName, string format, params object[] args)
        {
            this._tracer = tracer;
            if (format != null)
            {
                this.ScopeTracerHelper(flag, scopeOutputFormatter, leavingScopeFormatter, scopeName, format, args);
            }
            else
            {
                this.ScopeTracerHelper(flag, scopeOutputFormatter, leavingScopeFormatter, scopeName, "", new object[0]);
            }
        }

        public void Dispose()
        {
            PSTraceSource.ThreadIndentLevel--;
            if (!string.IsNullOrEmpty(this._leavingScopeFormatter))
            {
                this._tracer.OutputLine(this._flag, this._leavingScopeFormatter, new object[] { this._scopeName });
            }
            GC.SuppressFinalize(this);
        }

        internal void ScopeTracerHelper(PSTraceSourceOptions flag, string scopeOutputFormatter, string leavingScopeFormatter, string scopeName, string format, params object[] args)
        {
            this._flag = flag;
            this._scopeName = scopeName;
            this._leavingScopeFormatter = leavingScopeFormatter;
            StringBuilder builder = new StringBuilder();
            if (!string.IsNullOrEmpty(scopeOutputFormatter))
            {
                builder.AppendFormat(Thread.CurrentThread.CurrentCulture, scopeOutputFormatter, new object[] { this._scopeName });
            }
            if (!string.IsNullOrEmpty(format))
            {
                builder.AppendFormat(Thread.CurrentThread.CurrentCulture, format, args);
            }
            this._tracer.OutputLine(this._flag, builder.ToString(), new object[0]);
            PSTraceSource.ThreadIndentLevel++;
        }
    }
}

