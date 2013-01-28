namespace System.Management.Automation
{
    using System;
    using System.Runtime.CompilerServices;

    public class BreakpointUpdatedEventArgs : EventArgs
    {
        internal BreakpointUpdatedEventArgs(System.Management.Automation.Breakpoint breakpoint, BreakpointUpdateType updateType)
        {
            this.Breakpoint = breakpoint;
            this.UpdateType = updateType;
        }

        public System.Management.Automation.Breakpoint Breakpoint { get; private set; }

        public BreakpointUpdateType UpdateType { get; private set; }
    }
}

