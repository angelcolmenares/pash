namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class PSDebugContext
    {
        internal PSDebugContext(System.Management.Automation.InvocationInfo invocationInfo, List<Breakpoint> breakpoints)
        {
            this.InvocationInfo = invocationInfo;
            this.Breakpoints = breakpoints.ToArray();
        }

        public Breakpoint[] Breakpoints { get; private set; }

        public System.Management.Automation.InvocationInfo InvocationInfo { get; private set; }
    }
}

