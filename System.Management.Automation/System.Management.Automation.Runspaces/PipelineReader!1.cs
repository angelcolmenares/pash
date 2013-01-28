namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Management.Automation.Internal;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public abstract class PipelineReader<T>
    {
        public abstract event EventHandler DataReady;

        protected PipelineReader()
        {
        }

        public abstract void Close();
        internal IEnumerator<T> GetReadEnumerator()
        {
        Label_PostSwitchInIterator:;
            if (!this.EndOfPipeline)
            {
                T objA = this.Read();
                if (!object.Equals(objA, AutomationNull.Value))
                {
                    yield return objA;
                    goto Label_PostSwitchInIterator;
                }
            }
        }

        public abstract Collection<T> NonBlockingRead();
        public abstract Collection<T> NonBlockingRead(int maxRequested);
        public abstract T Peek();
        public abstract T Read();
        public abstract Collection<T> Read(int count);
        public abstract Collection<T> ReadToEnd();

        public abstract int Count { get; }

        public abstract bool EndOfPipeline { get; }

        public abstract bool IsOpen { get; }

        public abstract int MaxCapacity { get; }

        public abstract System.Threading.WaitHandle WaitHandle { get; }

        
    }
}

