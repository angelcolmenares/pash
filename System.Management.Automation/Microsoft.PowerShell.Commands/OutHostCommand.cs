namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Host;

    [Cmdlet("Out", "Host", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113365", RemotingCapability=RemotingCapability.None)]
    public class OutHostCommand : FrontEndCommandBase
    {
        private bool paging;

        public OutHostCommand()
        {
            base.implementation = new OutputManagerInner();
        }

        protected override void BeginProcessing()
        {
            ConsoleLineOutput output = new ConsoleLineOutput(base.Host.UI, this.paging, !PSHost.IsStdOutputRedirected, new TerminatingErrorContext(this));
            ((OutputManagerInner) base.implementation).LineOutput = output;
            base.BeginProcessing();
        }

        [Parameter]
        public SwitchParameter Paging
        {
            get
            {
                return this.paging;
            }
            set
            {
                this.paging = (bool) value;
            }
        }
    }
}

