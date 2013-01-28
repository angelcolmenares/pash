namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;

    [Cmdlet("Remove", "PSBreakpoint", SupportsShouldProcess=true, DefaultParameterSetName="Breakpoint", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113375")]
    public class RemovePSBreakpointCommand : PSBreakpointCommandBase
    {
        protected override void ProcessBreakpoint(Breakpoint breakpoint)
        {
            base.Context.Debugger.RemoveBreakpoint(breakpoint);
        }
    }
}

