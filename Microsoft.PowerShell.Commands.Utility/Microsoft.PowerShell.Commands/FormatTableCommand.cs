namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;
    using System.Management.Automation;

    [Cmdlet("Format", "Table", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113303")]
    public class FormatTableCommand : OuterFormatTableBase
    {
        public FormatTableCommand()
        {
            base.implementation = new InnerFormatShapeCommand(FormatShape.Table);
        }
    }
}

