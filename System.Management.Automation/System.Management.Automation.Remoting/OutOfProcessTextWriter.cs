namespace System.Management.Automation.Remoting
{
    using System;
    using System.IO;

    internal sealed class OutOfProcessTextWriter
    {
        private bool isStopped;
        private object syncObject = new object();
        private TextWriter writer;

        internal OutOfProcessTextWriter(TextWriter writerToWrap)
        {
            this.writer = writerToWrap;
        }

        internal void StopWriting()
        {
            this.isStopped = true;
        }

        internal void WriteLine(string data)
        {
            if (!this.isStopped)
            {
                lock (this.syncObject)
                {
                    this.writer.WriteLine(data);
                }
            }
        }
    }
}

