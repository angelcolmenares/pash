namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;
    using System.Management.Automation;

    [Cmdlet("Format", "Default")]
    public class FormatDefaultCommand : FrontEndCommandBase
    {
        public FormatDefaultCommand()
        {
            base.implementation = new InnerFormatShapeCommand(FormatShape.Undefined);
        }
    }
}

