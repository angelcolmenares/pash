namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    [Cmdlet("Out", "Null", SupportsShouldProcess=false, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113366", RemotingCapability=RemotingCapability.None)]
    public class OutNullCommand : PSCmdlet
    {
        private PSObject inputObject = AutomationNull.Value;

        protected override void ProcessRecord()
        {
        }

        [Parameter(ValueFromPipeline=true)]
        public PSObject InputObject
        {
            get
            {
                return this.inputObject;
            }
            set
            {
                this.inputObject = value;
            }
        }
    }
}

