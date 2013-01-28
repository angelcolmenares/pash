namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;

    [OutputType(new Type[] { typeof(Breakpoint) }), Cmdlet("Disable", "PSBreakpoint", SupportsShouldProcess=true, DefaultParameterSetName="Breakpoint", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113294")]
    public class DisablePSBreakpointCommand : PSBreakpointCommandBase
    {
        private bool _passThru;

        protected override void ProcessBreakpoint(Breakpoint breakpoint)
        {
            base.Context.Debugger.DisableBreakpoint(breakpoint);
            if (this._passThru)
            {
                base.WriteObject(breakpoint);
            }
        }

        [Parameter]
        public SwitchParameter PassThru
        {
            get
            {
                return this._passThru;
            }
            set
            {
                this._passThru = (bool) value;
            }
        }
    }
}

