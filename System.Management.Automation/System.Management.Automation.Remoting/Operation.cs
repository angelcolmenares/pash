namespace System.Management.Automation.Remoting
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Threading;

    internal class Operation : IThrottleOperation
    {
        private bool done = false;
        private int sleepTime = 100;
        private ThreadStart workerThreadDelegate;
        private Thread workerThreadStart;
        private Thread workerThreadStop;

        internal event EventHandler<EventArgs> EventHandler
        {
            add
            {
                bool flag = null == this.InternalEvent;
                this.InternalEvent += value;
                if (flag)
                {
                    this.OperationComplete += new EventHandler<OperationStateEventArgs>(this.Operation_OperationComplete);
                }
            }
            remove
            {
                this.InternalEvent -= value;
            }
        }

        internal event EventHandler<EventArgs> InternalEvent;

        internal override event EventHandler<OperationStateEventArgs> OperationComplete;

        internal Operation()
        {
            this.workerThreadDelegate = new ThreadStart(this.WorkerThreadMethodStart);
            this.workerThreadStart = new Thread(this.workerThreadDelegate);
            this.workerThreadDelegate = new ThreadStart(this.WorkerThreadMethodStop);
            this.workerThreadStop = new Thread(this.workerThreadDelegate);
        }

        internal static void AddOperation(object operation, ThrottleManager throttleManager)
        {
            throttleManager.AddOperation((IThrottleOperation) operation);
        }

        private void Operation_OperationComplete(object sender, OperationStateEventArgs e)
        {
            this.InternalEvent.SafeInvoke<EventArgs>(sender, e);
        }

        internal override void StartOperation()
        {
            this.workerThreadStart.Start();
        }

        internal override void StopOperation()
        {
            this.workerThreadStop.Start();
        }

        internal static void SubmitOperations(List<object> operations, ThrottleManager throttleManager)
        {
            List<IThrottleOperation> list = new List<IThrottleOperation>();
            foreach (object obj2 in operations)
            {
                list.Add((IThrottleOperation) obj2);
            }
            throttleManager.SubmitOperations(list);
        }

        private void WorkerThreadMethodStart()
        {
            Thread.Sleep(this.sleepTime);
            this.done = true;
            OperationStateEventArgs eventArgs = new OperationStateEventArgs {
                OperationState = OperationState.StartComplete
            };
            this.OperationComplete.SafeInvoke<OperationStateEventArgs>(this, eventArgs);
        }

        private void WorkerThreadMethodStop()
        {
            this.workerThreadStart.Abort();
            OperationStateEventArgs eventArgs = new OperationStateEventArgs {
                OperationState = OperationState.StopComplete
            };
            this.OperationComplete.SafeInvoke<OperationStateEventArgs>(this, eventArgs);
        }

        public bool Done
        {
            get
            {
                return this.done;
            }
            set
            {
                this.done = value;
            }
        }

        public int SleepTime
        {
            get
            {
                return this.sleepTime;
            }
            set
            {
                this.sleepTime = value;
            }
        }
    }
}

