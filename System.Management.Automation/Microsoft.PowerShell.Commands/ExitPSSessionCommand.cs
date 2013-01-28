namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Remoting;

    [Cmdlet("Exit", "PSSession", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135212")]
    public class ExitPSSessionCommand : PSRemotingCmdlet
    {
        protected override void ProcessRecord()
        {
            IHostSupportsInteractiveSession host = base.Host as IHostSupportsInteractiveSession;
            if (host == null)
            {
                base.WriteError(new ErrorRecord(new ArgumentException(base.GetMessage(RemotingErrorIdStrings.HostDoesNotSupportPushRunspace)), PSRemotingErrorId.HostDoesNotSupportPushRunspace.ToString(), ErrorCategory.InvalidArgument, null));
            }
            else
            {
                host.PopRunspace();
            }
        }
    }
}

