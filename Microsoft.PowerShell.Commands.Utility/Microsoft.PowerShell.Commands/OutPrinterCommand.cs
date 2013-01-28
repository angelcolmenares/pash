namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;
    using System.Management.Automation;

    [Cmdlet("Out", "Printer", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113367")]
    public class OutPrinterCommand : FrontEndCommandBase
    {
        private string printerName;

        public OutPrinterCommand()
        {
            base.implementation = new OutputManagerInner();
        }

        protected override void BeginProcessing()
        {
            OutputManagerInner implementation = (OutputManagerInner) base.implementation;
            implementation.LineOutput = this.InstantiateLineOutputInterface();
            base.BeginProcessing();
        }

        private LineOutput InstantiateLineOutputInterface()
        {
            return new PrinterLineOutput(this.printerName);
        }

        [Parameter(Position=0), Alias(new string[] { "PrinterName" })]
        public string Name
        {
            get
            {
                return this.printerName;
            }
            set
            {
                this.printerName = value;
            }
        }
    }
}

