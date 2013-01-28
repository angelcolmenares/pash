namespace System.Management.Automation.Remoting
{
    using System;

    internal abstract class IThrottleOperation
    {
        private bool _ignoreStop;

        internal abstract event EventHandler<OperationStateEventArgs> OperationComplete;

        protected IThrottleOperation()
        {
        }

        internal abstract void StartOperation();
        internal abstract void StopOperation();

        internal bool IgnoreStop
        {
            get
            {
                return this._ignoreStop;
            }
            set
            {
                this._ignoreStop = true;
            }
        }
    }
}

