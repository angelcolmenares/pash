namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;

    internal class OpenRunspaceOperation : IThrottleOperation, IDisposable
    {
        private List<EventHandler<OperationStateEventArgs>> _internalCallbacks = new List<EventHandler<OperationStateEventArgs>>();
        private object _syncObject = new object();
        private RemoteRunspace runspace;
        private bool startComplete = true;
        private bool stopComplete = true;

        internal override event EventHandler<OperationStateEventArgs> OperationComplete
        {
            add
            {
                lock (this._internalCallbacks)
                {
                    this._internalCallbacks.Add(value);
                }
            }
            remove
            {
                lock (this._internalCallbacks)
                {
                    this._internalCallbacks.Remove(value);
                }
            }
        }

        internal OpenRunspaceOperation(RemoteRunspace runspace)
        {
            this.runspace = runspace;
            this.runspace.StateChanged += new EventHandler<RunspaceStateEventArgs>(this.HandleRunspaceStateChanged);
        }

        public void Dispose()
        {
            this.runspace.StateChanged -= new EventHandler<RunspaceStateEventArgs>(this.HandleRunspaceStateChanged);
            GC.SuppressFinalize(this);
        }

        private void FireEvent(OperationStateEventArgs operationStateEventArgs)
        {
            EventHandler<OperationStateEventArgs>[] handlerArray;
            lock (this._internalCallbacks)
            {
                handlerArray = new EventHandler<OperationStateEventArgs>[this._internalCallbacks.Count];
                this._internalCallbacks.CopyTo(handlerArray);
            }
            foreach (EventHandler<OperationStateEventArgs> handler in handlerArray)
            {
                try
                {
                    handler.SafeInvoke<OperationStateEventArgs>(this, operationStateEventArgs);
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                }
            }
        }

        private void HandleRunspaceStateChanged(object source, RunspaceStateEventArgs stateEventArgs)
        {
            switch (stateEventArgs.RunspaceStateInfo.State)
            {
                case RunspaceState.BeforeOpen:
                case RunspaceState.Opening:
                case RunspaceState.Closing:
                    return;
            }
            OperationStateEventArgs operationStateEventArgs = null;
            lock (this._syncObject)
            {
                if (!this.stopComplete)
                {
                    this.stopComplete = true;
                    this.startComplete = true;
                    operationStateEventArgs = new OperationStateEventArgs {
                        BaseEvent = stateEventArgs,
                        OperationState = OperationState.StopComplete
                    };
                }
                else if (!this.startComplete)
                {
                    this.startComplete = true;
                    operationStateEventArgs = new OperationStateEventArgs {
                        BaseEvent = stateEventArgs,
                        OperationState = OperationState.StartComplete
                    };
                }
            }
            if (operationStateEventArgs != null)
            {
                this.FireEvent(operationStateEventArgs);
            }
        }

        internal override void StartOperation()
        {
            lock (this._syncObject)
            {
                this.startComplete = false;
            }
            this.runspace.OpenAsync();
        }

        internal override void StopOperation()
        {
            OperationStateEventArgs operationStateEventArgs = null;
            lock (this._syncObject)
            {
                if (this.startComplete)
                {
                    this.stopComplete = true;
                    this.startComplete = true;
                    operationStateEventArgs = new OperationStateEventArgs {
                        BaseEvent = new RunspaceStateEventArgs(this.runspace.RunspaceStateInfo),
                        OperationState = OperationState.StopComplete
                    };
                }
                else
                {
                    this.stopComplete = false;
                }
            }
            if (operationStateEventArgs != null)
            {
                this.FireEvent(operationStateEventArgs);
            }
            else
            {
                this.runspace.CloseAsync();
            }
        }

        internal RemoteRunspace OperatedRunspace
        {
            get
            {
                return this.runspace;
            }
        }
    }
}

