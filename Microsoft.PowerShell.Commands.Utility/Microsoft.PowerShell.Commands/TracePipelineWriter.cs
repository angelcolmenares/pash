namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Threading;

    internal class TracePipelineWriter : PipelineWriter
    {
        private TraceListenerCommandBase cmdlet;
        private bool isOpen = true;
        private Collection<PSTraceSource> matchingSources = new Collection<PSTraceSource>();
        private bool writeError;

        internal TracePipelineWriter(TraceListenerCommandBase cmdlet, bool writeError, Collection<PSTraceSource> matchingSources)
        {
            if (cmdlet == null)
            {
                throw new ArgumentNullException("cmdlet");
            }
            if (matchingSources == null)
            {
                throw new ArgumentNullException("matchingSources");
            }
            this.cmdlet = cmdlet;
            this.writeError = writeError;
            this.matchingSources = matchingSources;
        }

        public override void Close()
        {
            if (this.isOpen)
            {
                this.Flush();
                this.isOpen = false;
            }
        }

        private static ErrorRecord ConvertToErrorRecord(object obj)
        {
            ErrorRecord record = null;
            PSObject obj2 = obj as PSObject;
            if (obj2 != null)
            {
                object baseObject = obj2.BaseObject;
                if (!(baseObject is PSCustomObject))
                {
                    obj = baseObject;
                }
            }
            ErrorRecord record2 = obj as ErrorRecord;
            if (record2 != null)
            {
                record = record2;
            }
            return record;
        }

        public override void Flush()
        {
        }

        public override int Write(object obj)
        {
            this.cmdlet.ResetTracing(this.matchingSources);
            if (this.writeError)
            {
                ErrorRecord errorRecord = ConvertToErrorRecord(obj);
                if (errorRecord != null)
                {
                    this.cmdlet.WriteError(errorRecord);
                }
            }
            else
            {
                this.cmdlet.WriteObject(obj);
            }
            this.cmdlet.TurnOnTracing(this.matchingSources, false);
            return 1;
        }

        public override int Write(object obj, bool enumerateCollection)
        {
            this.cmdlet.ResetTracing(this.matchingSources);
            int num = 0;
            if (this.writeError)
            {
                if (enumerateCollection)
                {
                    foreach (object obj2 in LanguagePrimitives.GetEnumerable(obj))
                    {
                        ErrorRecord errorRecord = ConvertToErrorRecord(obj2);
                        if (errorRecord != null)
                        {
                            num++;
                            this.cmdlet.WriteError(errorRecord);
                        }
                    }
                }
                else
                {
                    ErrorRecord record2 = ConvertToErrorRecord(obj);
                    if (record2 != null)
                    {
                        num++;
                        this.cmdlet.WriteError(record2);
                    }
                }
            }
            else
            {
                num++;
                this.cmdlet.WriteObject(obj, enumerateCollection);
            }
            this.cmdlet.TurnOnTracing(this.matchingSources, false);
            return num;
        }

        public override int Count
        {
            get
            {
                return 0;
            }
        }

        public override bool IsOpen
        {
            get
            {
                return this.isOpen;
            }
        }

        public override int MaxCapacity
        {
            get
            {
                return 0x7fffffff;
            }
        }

        public override System.Threading.WaitHandle WaitHandle
        {
            get
            {
                return null;
            }
        }
    }
}

