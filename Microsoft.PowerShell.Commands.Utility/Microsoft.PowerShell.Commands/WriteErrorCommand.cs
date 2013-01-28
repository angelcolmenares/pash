namespace Microsoft.PowerShell.Commands
{
    using System.Management.Automation;

    [Cmdlet("Write", "Error", DefaultParameterSetName="NoException", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113425", RemotingCapability=RemotingCapability.None)]
    public sealed class WriteErrorCommand : WriteOrThrowErrorCommand
    {
    }
}

