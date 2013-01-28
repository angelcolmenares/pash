namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Threading;

    public abstract class PipelineWriter
    {
        protected PipelineWriter()
        {
        }

        public abstract void Close();
        public abstract void Flush();
        public abstract int Write(object obj);
        public abstract int Write(object obj, bool enumerateCollection);

        public abstract int Count { get; }

        public abstract bool IsOpen { get; }

        public abstract int MaxCapacity { get; }

        public abstract System.Threading.WaitHandle WaitHandle { get; }
    }
}

