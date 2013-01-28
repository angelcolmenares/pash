namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Host;

    [OutputType(new Type[] { typeof(PSHost) }), Cmdlet("Get", "Host", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113318", RemotingCapability=RemotingCapability.None)]
    public class GetHostCommand : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            base.WriteObject(base.Host);
        }
    }
}

