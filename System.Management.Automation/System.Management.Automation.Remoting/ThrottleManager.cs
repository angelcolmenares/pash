namespace System.Management.Automation.Remoting
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Threading;

    internal class ThrottleManager : IDisposable
    {
        private static int DEFAULT_THROTTLE_LIMIT = 0x20;
        private List<IThrottleOperation> operationsQueue = new List<IThrottleOperation>();
        private List<IThrottleOperation> startOperationQueue = new List<IThrottleOperation>();
        private List<IThrottleOperation> stopOperationQueue = new List<IThrottleOperation>();
        private bool stopping;
        private bool submitComplete;
        private object syncObject = new object();
        private static int THROTTLE_LIMIT_MAX = 0x7fffffff;
        private int throttleLimit = DEFAULT_THROTTLE_LIMIT;

        internal event EventHandler<EventArgs> ThrottleComplete;

        internal void AddOperation(IThrottleOperation operation)
        {
            lock (this.syncObject)
            {
                if (this.submitComplete)
                {
                    throw new InvalidOperationException();
                }
                this.operationsQueue.Add(operation);
            }
            this.StartOperationsFromQueue();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.StopAllOperations();
            }
        }

        internal void EndSubmitOperations()
        {
            lock (this.syncObject)
            {
                this.submitComplete = true;
            }
            this.RaiseThrottleManagerEvents();
        }

        private void OperationCompleteHandler(object source, OperationStateEventArgs stateEventArgs)
        {
            lock (this.syncObject)
            {
                IThrottleOperation item = source as IThrottleOperation;
                int index = -1;
                if (stateEventArgs.OperationState == OperationState.StartComplete)
                {
                    index = this.startOperationQueue.IndexOf(item);
                    if (index != -1)
                    {
                        this.startOperationQueue.RemoveAt(index);
                    }
                }
                else
                {
                    index = this.startOperationQueue.IndexOf(item);
                    if (index != -1)
                    {
                        this.startOperationQueue.RemoveAt(index);
                    }
                    index = this.stopOperationQueue.IndexOf(item);
                    if (index != -1)
                    {
                        this.stopOperationQueue.RemoveAt(index);
                    }
                    item.IgnoreStop = true;
                }
            }
            this.RaiseThrottleManagerEvents();
            this.StartOneOperationFromQueue();
        }

        private void RaiseThrottleManagerEvents()
        {
            bool flag = false;
            lock (this.syncObject)
            {
                if ((this.submitComplete && (this.startOperationQueue.Count == 0)) && ((this.stopOperationQueue.Count == 0) && (this.operationsQueue.Count == 0)))
                {
                    flag = true;
                }
            }
            if (flag)
            {
                this.ThrottleComplete.SafeInvoke<EventArgs>(this, EventArgs.Empty);
            }
        }

        private void StartOneOperationFromQueue()
        {
            IThrottleOperation item = null;
            lock (this.syncObject)
            {
                if (this.operationsQueue.Count > 0)
                {
                    item = this.operationsQueue[0];
                    this.operationsQueue.RemoveAt(0);
                    item.OperationComplete += new EventHandler<OperationStateEventArgs>(this.OperationCompleteHandler);
                    this.startOperationQueue.Add(item);
                }
            }
            if (item != null)
            {
                item.StartOperation();
            }
        }

        private void StartOperationsFromQueue()
        {
            int count = 0;
            int num2 = 0;
            lock (this.syncObject)
            {
                count = this.startOperationQueue.Count;
                num2 = this.operationsQueue.Count;
            }
            int num3 = this.throttleLimit - count;
            if (num3 > 0)
            {
                int num4 = (num3 > num2) ? num2 : num3;
                for (int i = 0; i < num4; i++)
                {
                    this.StartOneOperationFromQueue();
                }
            }
        }

        internal void StopAllOperations()
        {
            bool flag = false;
            lock (this.syncObject)
            {
                if (!this.stopping)
                {
                    this.stopping = true;
                }
                else
                {
                    flag = true;
                }
            }
            if (flag)
            {
                this.RaiseThrottleManagerEvents();
            }
            else
            {
                IThrottleOperation[] operationArray;
                lock (this.syncObject)
                {
                    this.submitComplete = true;
                    this.operationsQueue.Clear();
                    operationArray = new IThrottleOperation[this.startOperationQueue.Count];
                    this.startOperationQueue.CopyTo(operationArray);
                    foreach (IThrottleOperation operation in operationArray)
                    {
                        this.stopOperationQueue.Add(operation);
                        operation.IgnoreStop = true;
                    }
                }
                foreach (IThrottleOperation operation2 in operationArray)
                {
                    operation2.StopOperation();
                }
                this.RaiseThrottleManagerEvents();
            }
        }

        internal void StopOperation(IThrottleOperation operation)
        {
            if (!operation.IgnoreStop)
            {
                if (this.operationsQueue.IndexOf(operation) != -1)
                {
                    lock (this.syncObject)
                    {
                        if (this.operationsQueue.IndexOf(operation) != -1)
                        {
                            this.operationsQueue.Remove(operation);
                            this.RaiseThrottleManagerEvents();
                            return;
                        }
                    }
                }
                lock (this.syncObject)
                {
                    this.stopOperationQueue.Add(operation);
                    operation.IgnoreStop = true;
                }
                operation.StopOperation();
            }
        }

        internal void SubmitOperations(List<IThrottleOperation> operations)
        {
            lock (this.syncObject)
            {
                if (this.submitComplete)
                {
                    throw new InvalidOperationException();
                }
                foreach (IThrottleOperation operation in operations)
                {
                    this.operationsQueue.Add(operation);
                }
            }
            this.StartOperationsFromQueue();
        }

        internal int ThrottleLimit
        {
            get
            {
                return this.throttleLimit;
            }
            set
            {
                if ((value > 0) && (value <= THROTTLE_LIMIT_MAX))
                {
                    this.throttleLimit = value;
                }
            }
        }
    }
}

