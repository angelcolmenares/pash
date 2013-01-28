namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    internal class PipelineStopper
    {
        private LocalPipeline _localPipeline;
        private Stack<PipelineProcessor> _stack = new Stack<PipelineProcessor>();
        private bool _stopping;
        private object _syncRoot = new object();

        internal PipelineStopper(LocalPipeline localPipeline)
        {
            this._localPipeline = localPipeline;
        }

        internal void Pop(bool fromSteppablePipeline)
        {
            lock (this._syncRoot)
            {
                if (!this._stopping && (this._stack.Count > 0))
                {
                    PipelineProcessor processor = this._stack.Pop();
                    if ((fromSteppablePipeline && processor.ExecutionFailed) && (this._stack.Count > 0))
                    {
                        this._stack.Peek().ExecutionFailed = true;
                    }
                    if ((this._stack.Count == 1) && (this._localPipeline != null))
                    {
                        this._localPipeline.SetHadErrors(processor.ExecutionFailed);
                    }
                }
            }
        }

        internal void Push(PipelineProcessor item)
        {
            if (item == null)
            {
                throw PSTraceSource.NewArgumentNullException("item");
            }
            lock (this._syncRoot)
            {
                if (this._stopping)
                {
                    PipelineStoppedException exception = new PipelineStoppedException();
                    throw exception;
                }
                this._stack.Push(item);
            }
            item.LocalPipeline = this._localPipeline;
        }

        internal void Stop()
        {
            PipelineProcessor[] processorArray;
            lock (this._syncRoot)
            {
                if (this._stopping)
                {
                    return;
                }
                this._stopping = true;
                processorArray = this._stack.ToArray();
            }
            if (processorArray.Length > 0)
            {
                PipelineProcessor processor = processorArray[processorArray.Length - 1];
                if ((processor != null) && (this._localPipeline != null))
                {
                    this._localPipeline.SetHadErrors(processor.ExecutionFailed);
                }
            }
            foreach (PipelineProcessor processor2 in processorArray)
            {
                processor2.Stop();
            }
        }

        internal bool IsStopping
        {
            get
            {
                return this._stopping;
            }
            set
            {
                this._stopping = value;
            }
        }
    }
}

