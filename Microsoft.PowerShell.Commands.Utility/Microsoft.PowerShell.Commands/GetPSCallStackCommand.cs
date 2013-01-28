namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;

    [OutputType(new Type[] { typeof(CallStackFrame) }), Cmdlet("Get", "PSCallStack", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113326")]
    public class GetPSCallStackCommand : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            foreach (CallStackFrame frame in base.Context.Debugger.GetCallStack())
            {
                base.WriteObject(frame);
            }
        }
    }
}

