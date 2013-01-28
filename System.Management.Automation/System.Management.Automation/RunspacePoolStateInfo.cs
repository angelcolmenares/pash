namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Runspaces;

    public sealed class RunspacePoolStateInfo
    {
        private Exception reason;
        private RunspacePoolState state;

        public RunspacePoolStateInfo(RunspacePoolState state, Exception reason)
        {
            this.state = state;
            this.reason = reason;
        }

        public Exception Reason
        {
            get
            {
                return this.reason;
            }
        }

        public RunspacePoolState State
        {
            get
            {
                return this.state;
            }
        }
    }
}

