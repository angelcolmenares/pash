namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Reflection;

    internal class HelpErrorTracer
    {
        private System.Management.Automation.HelpSystem _helpSystem;
        private ArrayList _traceFrames = new ArrayList();

        internal HelpErrorTracer(System.Management.Automation.HelpSystem helpSystem)
        {
            if (helpSystem == null)
            {
                throw PSTraceSource.NewArgumentNullException("HelpSystem");
            }
            this._helpSystem = helpSystem;
        }

        internal void PopFrame(TraceFrame traceFrame)
        {
            if (this._traceFrames.Count > 0)
            {
                TraceFrame frame = (TraceFrame) this._traceFrames[this._traceFrames.Count - 1];
                if (frame == traceFrame)
                {
                    this._traceFrames.RemoveAt(this._traceFrames.Count - 1);
                }
            }
        }

        internal IDisposable Trace(string helpFile)
        {
            TraceFrame frame = new TraceFrame(this, helpFile);
            this._traceFrames.Add(frame);
            return frame;
        }

        internal void TraceError(ErrorRecord errorRecord)
        {
            if (this._traceFrames.Count > 0)
            {
                ((TraceFrame) this._traceFrames[this._traceFrames.Count - 1]).TraceError(errorRecord);
            }
        }

        internal void TraceErrors(Collection<ErrorRecord> errorRecords)
        {
            if (this._traceFrames.Count > 0)
            {
                ((TraceFrame) this._traceFrames[this._traceFrames.Count - 1]).TraceErrors(errorRecords);
            }
        }

        internal System.Management.Automation.HelpSystem HelpSystem
        {
            get
            {
                return this._helpSystem;
            }
        }

        internal bool IsOn
        {
            get
            {
                return ((this._traceFrames.Count > 0) && this.HelpSystem.VerboseHelpErrors);
            }
        }

        internal sealed class TraceFrame : IDisposable
        {
            private Collection<ErrorRecord> _errors = new Collection<ErrorRecord>();
            private string _helpFile = "";
            private HelpErrorTracer _helpTracer;

            internal TraceFrame(HelpErrorTracer helpTracer, string helpFile)
            {
                this._helpTracer = helpTracer;
                this._helpFile = helpFile;
            }

            public void Dispose()
            {
                if (this._helpTracer.HelpSystem.VerboseHelpErrors && (this._errors.Count > 0))
                {
                    ErrorRecord item = new ErrorRecord(new ParentContainsErrorRecordException("Help Load Error"), "HelpLoadError", ErrorCategory.SyntaxError, null) {
                        ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "HelpErrors", "HelpLoadError", new object[] { this._helpFile, this._errors.Count })
                    };
                    this._helpTracer.HelpSystem.LastErrors.Add(item);
                    foreach (ErrorRecord record2 in this._errors)
                    {
                        this._helpTracer.HelpSystem.LastErrors.Add(record2);
                    }
                }
                this._helpTracer.PopFrame(this);
            }

            internal void TraceError(ErrorRecord errorRecord)
            {
                if (this._helpTracer.HelpSystem.VerboseHelpErrors)
                {
                    this._errors.Add(errorRecord);
                }
            }

            internal void TraceErrors(Collection<ErrorRecord> errorRecords)
            {
                if (this._helpTracer.HelpSystem.VerboseHelpErrors)
                {
                    foreach (ErrorRecord record in errorRecords)
                    {
                        this._errors.Add(record);
                    }
                }
            }
        }
    }
}

