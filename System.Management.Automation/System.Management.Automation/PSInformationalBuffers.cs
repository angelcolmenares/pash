namespace System.Management.Automation
{
    using System;

    internal sealed class PSInformationalBuffers
    {
        internal PSDataCollection<DebugRecord> debug;
        internal PSDataCollection<ProgressRecord> progress;
        private Guid psInstanceId;
        internal PSDataCollection<VerboseRecord> verbose;
        private PSDataCollection<WarningRecord> warning;

        internal PSInformationalBuffers(Guid psInstanceId)
        {
            this.psInstanceId = psInstanceId;
            this.progress = new PSDataCollection<ProgressRecord>();
            this.verbose = new PSDataCollection<VerboseRecord>();
            this.debug = new PSDataCollection<DebugRecord>();
            this.warning = new PSDataCollection<WarningRecord>();
        }

        internal void AddDebug(DebugRecord item)
        {
            if (this.debug != null)
            {
                this.debug.InternalAdd(this.psInstanceId, item);
            }
        }

        internal void AddProgress(ProgressRecord item)
        {
            if (this.progress != null)
            {
                this.progress.InternalAdd(this.psInstanceId, item);
            }
        }

        internal void AddVerbose(VerboseRecord item)
        {
            if (this.verbose != null)
            {
                this.verbose.InternalAdd(this.psInstanceId, item);
            }
        }

        internal void AddWarning(WarningRecord item)
        {
            if (this.warning != null)
            {
                this.warning.InternalAdd(this.psInstanceId, item);
            }
        }

        internal PSDataCollection<DebugRecord> Debug
        {
            get
            {
                return this.debug;
            }
            set
            {
                this.debug = value;
            }
        }

        internal PSDataCollection<ProgressRecord> Progress
        {
            get
            {
                return this.progress;
            }
            set
            {
                this.progress = value;
            }
        }

        internal PSDataCollection<VerboseRecord> Verbose
        {
            get
            {
                return this.verbose;
            }
            set
            {
                this.verbose = value;
            }
        }

        internal PSDataCollection<WarningRecord> Warning
        {
            get
            {
                return this.warning;
            }
            set
            {
                this.warning = value;
            }
        }
    }
}

