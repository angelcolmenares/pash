namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;

    public class DebuggerStopEventArgs : EventArgs
    {
        internal DebuggerStopEventArgs(System.Management.Automation.InvocationInfo invocationInfo, List<Breakpoint> breakpoints)
        {
            this.InvocationInfo = invocationInfo;
            this.Breakpoints = new ReadOnlyCollection<Breakpoint>(breakpoints);
            this.ResumeAction = DebuggerResumeAction.Continue;
        }

        public ReadOnlyCollection<Breakpoint> Breakpoints { get; private set; }

        public System.Management.Automation.InvocationInfo InvocationInfo { get; private set; }

        public DebuggerResumeAction ResumeAction { get; set; }
    }
}

