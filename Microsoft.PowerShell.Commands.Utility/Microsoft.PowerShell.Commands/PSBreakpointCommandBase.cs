namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    public abstract class PSBreakpointCommandBase : PSCmdlet
    {
        private System.Management.Automation.Breakpoint[] _breakpoints;
        private int[] _ids;

        protected PSBreakpointCommandBase()
        {
        }

        protected abstract void ProcessBreakpoint(System.Management.Automation.Breakpoint breakpoint);
        protected override void ProcessRecord()
        {
            if (base.ParameterSetName.Equals("Breakpoint", StringComparison.OrdinalIgnoreCase))
            {
                foreach (System.Management.Automation.Breakpoint breakpoint in this._breakpoints)
                {
                    if (base.ShouldProcess(breakpoint.ToString()))
                    {
                        this.ProcessBreakpoint(breakpoint);
                    }
                }
            }
            else
            {
                foreach (int num in this._ids)
                {
                    System.Management.Automation.Breakpoint breakpoint2 = base.Context.Debugger.GetBreakpoint(num);
                    if (breakpoint2 == null)
                    {
                        base.WriteError(new ErrorRecord(new ArgumentException(StringUtil.Format(UtilityDebuggerStrings.BreakpointIdNotFound, num)), "PSBreakpoint:BreakpointIdNotFound", ErrorCategory.InvalidArgument, null));
                    }
                    else if (base.ShouldProcess(breakpoint2.ToString()))
                    {
                        this.ProcessBreakpoint(breakpoint2);
                    }
                }
            }
        }

        [Parameter(ParameterSetName="Breakpoint", ValueFromPipeline=true, Position=0, Mandatory=true), ValidateNotNull]
        public System.Management.Automation.Breakpoint[] Breakpoint
        {
            get
            {
                return this._breakpoints;
            }
            set
            {
                this._breakpoints = value;
            }
        }

        [Parameter(ParameterSetName="Id", ValueFromPipelineByPropertyName=true, Position=0, Mandatory=true), ValidateNotNull]
        public int[] Id
        {
            get
            {
                return this._ids;
            }
            set
            {
                this._ids = value;
            }
        }
    }
}

