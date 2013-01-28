namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;

    [OutputType(new Type[] { typeof(Breakpoint) }), Cmdlet("Enable", "PSBreakpoint", SupportsShouldProcess=true, DefaultParameterSetName="Id", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113295")]
    public class EnablePSBreakpointCommand : PSBreakpointCommandBase
    {
        private bool _passThru;

        protected override void ProcessBreakpoint(Breakpoint breakpoint)
        {
            base.Context.Debugger.EnableBreakpoint(breakpoint);
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

