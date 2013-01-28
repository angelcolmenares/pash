namespace System.Management.Automation
{
    using System;
    using System.Threading;

    internal class BatchInvocationContext
    {
        private PSCommand command;
        private AutoResetEvent completionEvent;
        private PSDataCollection<PSObject> output;

        internal BatchInvocationContext(PSCommand command, PSDataCollection<PSObject> output)
        {
            this.command = command;
            this.output = output;
            this.completionEvent = new AutoResetEvent(false);
        }

        internal void Signal()
        {
            this.completionEvent.Set();
        }

        internal void Wait()
        {
            this.completionEvent.WaitOne();
        }

        internal PSCommand Command
        {
            get
            {
                return this.command;
            }
        }

        internal PSDataCollection<PSObject> Output
        {
            get
            {
                return this.output;
            }
        }
    }
}

