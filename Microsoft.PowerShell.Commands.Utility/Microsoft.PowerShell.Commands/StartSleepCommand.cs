namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;
    using System.Threading;

    [Cmdlet("Start", "Sleep", DefaultParameterSetName="Seconds", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113407")]
    public sealed class StartSleepCommand : PSCmdlet, IDisposable
    {
        private bool disposed;
        private int milliseconds;
        private int seconds;
        private bool stopping;
        private object syncObject = new object();
        private ManualResetEvent waitHandle;

        public void Dispose()
        {
            if (!this.disposed)
            {
                if (this.waitHandle != null)
                {
                    this.waitHandle.Close();
                    this.waitHandle = null;
                }
                this.disposed = true;
            }
        }

        protected override void ProcessRecord()
        {
            int milliSecondsToSleep = 0;
            string parameterSetName = base.ParameterSetName;
            if (parameterSetName != null)
            {
                if (!(parameterSetName == "Seconds"))
                {
                    if (parameterSetName == "Milliseconds")
                    {
                        milliSecondsToSleep = this.Milliseconds;
                    }
                }
                else
                {
                    milliSecondsToSleep = this.Seconds * 0x3e8;
                }
            }
            this.Sleep(milliSecondsToSleep);
        }

        private void Sleep(int milliSecondsToSleep)
        {
            lock (this.syncObject)
            {
                if (!this.stopping)
                {
                    this.waitHandle = new ManualResetEvent(false);
                }
            }
            if (this.waitHandle != null)
            {
                this.waitHandle.WaitOne(new TimeSpan(0, 0, 0, 0, milliSecondsToSleep), true);
            }
        }

        protected override void StopProcessing()
        {
            lock (this.syncObject)
            {
                this.stopping = true;
                if (this.waitHandle != null)
                {
                    this.waitHandle.Set();
                }
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="Milliseconds", ValueFromPipelineByPropertyName=true), ValidateRange(0, 0x7fffffff)]
        public int Milliseconds
        {
            get
            {
                return this.milliseconds;
            }
            set
            {
                this.milliseconds = value;
            }
        }

        [ValidateRange(0, 0x20c49b), Parameter(Position=0, Mandatory=true, ParameterSetName="Seconds", ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
        public int Seconds
        {
            get
            {
                return this.seconds;
            }
            set
            {
                this.seconds = value;
            }
        }
    }
}

