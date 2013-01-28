namespace System.Management.Automation.Internal
{
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Threading;

    internal abstract class ObjectStreamBase : IDisposable
    {
        internal event EventHandler DataReady;

        protected ObjectStreamBase()
        {
        }

        internal virtual void Close()
        {
            throw PSTraceSource.NewNotSupportedException();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
        internal void FireDataReadyEvent(object source, EventArgs args)
        {
            this.DataReady.SafeInvoke(source, args);
        }

        internal virtual void Flush()
        {
            throw PSTraceSource.NewNotSupportedException();
        }

        internal virtual Collection<object> NonBlockingRead(int maxRequested)
        {
            throw PSTraceSource.NewNotSupportedException();
        }

        internal virtual object Peek()
        {
            throw PSTraceSource.NewNotSupportedException();
        }

        internal virtual object Read()
        {
            throw PSTraceSource.NewNotSupportedException();
        }

        internal virtual Collection<object> Read(int count)
        {
            throw PSTraceSource.NewNotSupportedException();
        }

        internal virtual Collection<object> ReadToEnd()
        {
            throw PSTraceSource.NewNotSupportedException();
        }

        internal virtual int Write(object value)
        {
            return this.Write(value, false);
        }

        internal virtual int Write(object obj, bool enumerateCollection)
        {
            throw PSTraceSource.NewNotSupportedException();
        }

        internal abstract int Count { get; }

        internal abstract bool EndOfPipeline { get; }

        internal abstract bool IsOpen { get; }

        internal abstract int MaxCapacity { get; }

        internal abstract PipelineReader<object> ObjectReader { get; }

        internal abstract PipelineWriter ObjectWriter { get; }

        internal abstract PipelineReader<PSObject> PSObjectReader { get; }

        internal virtual WaitHandle ReadHandle
        {
            get
            {
                throw PSTraceSource.NewNotSupportedException();
            }
        }

        internal virtual WaitHandle WriteHandle
        {
            get
            {
                throw PSTraceSource.NewNotSupportedException();
            }
        }
    }
}

