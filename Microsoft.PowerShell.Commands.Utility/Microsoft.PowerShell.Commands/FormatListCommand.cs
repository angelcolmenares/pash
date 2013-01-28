namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;
    using System.Management.Automation;

    [Cmdlet("Format", "List", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113302")]
    public class FormatListCommand : OuterFormatTableAndListBase
    {
        public FormatListCommand()
        {
            base.implementation = new InnerFormatShapeCommand(FormatShape.List);
        }
    }
}

