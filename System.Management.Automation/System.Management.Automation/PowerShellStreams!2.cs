namespace System.Management.Automation
{
    using System;

    public sealed class PowerShellStreams<TInput, TOutput> : IDisposable
    {
        private PSDataCollection<DebugRecord> debugStream;
        private bool disposed;
        private PSDataCollection<ErrorRecord> errorStream;
        private PSDataCollection<TInput> inputStream;
        private PSDataCollection<TOutput> outputStream;
        private PSDataCollection<ProgressRecord> progressStream;
        private readonly object syncLock;
        private PSDataCollection<VerboseRecord> verboseStream;
        private PSDataCollection<WarningRecord> warningStream;

        public PowerShellStreams()
        {
            this.syncLock = new object();
            this.inputStream = null;
            this.outputStream = null;
            this.errorStream = null;
            this.warningStream = null;
            this.progressStream = null;
            this.verboseStream = null;
            this.debugStream = null;
            this.disposed = false;
        }

        public PowerShellStreams(PSDataCollection<TInput> pipelineInput)
        {
            this.syncLock = new object();
            if (pipelineInput == null)
            {
                this.inputStream = new PSDataCollection<TInput>();
            }
            else
            {
                this.inputStream = pipelineInput;
            }
            this.inputStream.Complete();
            this.outputStream = new PSDataCollection<TOutput>();
            this.errorStream = new PSDataCollection<ErrorRecord>();
            this.warningStream = new PSDataCollection<WarningRecord>();
            this.progressStream = new PSDataCollection<ProgressRecord>();
            this.verboseStream = new PSDataCollection<VerboseRecord>();
            this.debugStream = new PSDataCollection<DebugRecord>();
            this.disposed = false;
        }

        public void CloseAll()
        {
            if (!this.disposed)
            {
                lock (this.syncLock)
                {
                    if (!this.disposed)
                    {
                        this.outputStream.Complete();
                        this.errorStream.Complete();
                        this.warningStream.Complete();
                        this.progressStream.Complete();
                        this.verboseStream.Complete();
                        this.debugStream.Complete();
                    }
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                lock (this.syncLock)
                {
                    if (!this.disposed)
                    {
                        if (disposing)
                        {
                            this.inputStream.Dispose();
                            this.outputStream.Dispose();
                            this.errorStream.Dispose();
                            this.warningStream.Dispose();
                            this.progressStream.Dispose();
                            this.verboseStream.Dispose();
                            this.debugStream.Dispose();
                            this.inputStream = null;
                            this.outputStream = null;
                            this.errorStream = null;
                            this.warningStream = null;
                            this.progressStream = null;
                            this.verboseStream = null;
                            this.debugStream = null;
                        }
                        this.disposed = true;
                    }
                }
            }
        }

        public PSDataCollection<DebugRecord> DebugStream
        {
            get
            {
                return this.debugStream;
            }
            set
            {
                this.debugStream = value;
            }
        }

        public PSDataCollection<ErrorRecord> ErrorStream
        {
            get
            {
                return this.errorStream;
            }
            set
            {
                this.errorStream = value;
            }
        }

        public PSDataCollection<TInput> InputStream
        {
            get
            {
                return this.inputStream;
            }
            set
            {
                this.inputStream = value;
            }
        }

        public PSDataCollection<TOutput> OutputStream
        {
            get
            {
                return this.outputStream;
            }
            set
            {
                this.outputStream = value;
            }
        }

        public PSDataCollection<ProgressRecord> ProgressStream
        {
            get
            {
                return this.progressStream;
            }
            set
            {
                this.progressStream = value;
            }
        }

        public PSDataCollection<VerboseRecord> VerboseStream
        {
            get
            {
                return this.verboseStream;
            }
            set
            {
                this.verboseStream = value;
            }
        }

        public PSDataCollection<WarningRecord> WarningStream
        {
            get
            {
                return this.warningStream;
            }
            set
            {
                this.warningStream = value;
            }
        }
    }
}

