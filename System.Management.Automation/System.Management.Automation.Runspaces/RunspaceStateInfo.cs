namespace System.Management.Automation.Runspaces
{
    using System;

    public sealed class RunspaceStateInfo
    {
        private Exception _reason;
        private RunspaceState _state;

        internal RunspaceStateInfo(RunspaceState state) : this(state, null)
        {
        }

        internal RunspaceStateInfo(RunspaceStateInfo runspaceStateInfo)
        {
            this._state = runspaceStateInfo.State;
            this._reason = runspaceStateInfo.Reason;
        }

        internal RunspaceStateInfo(RunspaceState state, Exception reason)
        {
            this._state = state;
            this._reason = reason;
        }

        internal RunspaceStateInfo Clone()
        {
            return new RunspaceStateInfo(this);
        }

        public override string ToString()
        {
            return this._state.ToString();
        }

        public Exception Reason
        {
            get
            {
                return this._reason;
            }
        }

        public RunspaceState State
        {
            get
            {
                return this._state;
            }
        }
    }
}

