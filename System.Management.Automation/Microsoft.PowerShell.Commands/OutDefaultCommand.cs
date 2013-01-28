namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Host;

    [Cmdlet("Out", "Default", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113362", RemotingCapability=RemotingCapability.None)]
    public class OutDefaultCommand : FrontEndCommandBase
    {
        public OutDefaultCommand()
        {
            base.implementation = new OutputManagerInner();
        }

        protected override void BeginProcessing()
        {
            ConsoleLineOutput output = new ConsoleLineOutput(base.Host.UI, false, !PSHost.IsStdOutputRedirected, new TerminatingErrorContext(this));
            ((OutputManagerInner) base.implementation).LineOutput = output;
            MshCommandRuntime commandRuntime = base.CommandRuntime as MshCommandRuntime;
            if (commandRuntime != null)
            {
                commandRuntime.MergeUnclaimedPreviousErrorResults = true;
            }
            base.BeginProcessing();
        }
    }
}

