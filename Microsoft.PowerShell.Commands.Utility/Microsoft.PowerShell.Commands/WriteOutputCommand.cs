namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;

    [Cmdlet("Write", "Output", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113427", RemotingCapability=RemotingCapability.None)]
    public sealed class WriteOutputCommand : PSCmdlet
    {
        private PSObject[] inputObjects;

        protected override void ProcessRecord()
        {
            if (this.inputObjects == null)
            {
                base.WriteObject(this.inputObjects);
            }
            else
            {
                foreach (PSObject obj2 in this.inputObjects)
                {
                    base.WriteObject(obj2, true);
                }
            }
        }

        [Parameter(Position=0, Mandatory=true, ValueFromPipeline=true, ValueFromRemainingArguments=true), AllowNull, AllowEmptyCollection]
        public PSObject[] InputObject
        {
            get
            {
                return this.inputObjects;
            }
            set
            {
                this.inputObjects = value;
            }
        }
    }
}

