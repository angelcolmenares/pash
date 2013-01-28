namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;

    [Cmdlet("Write", "Warning", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113430", RemotingCapability=RemotingCapability.None)]
    public sealed class WriteWarningCommand : PSCmdlet
    {
        private string message;

        protected override void ProcessRecord()
        {
            MshCommandRuntime commandRuntime = base.CommandRuntime as MshCommandRuntime;
            if (commandRuntime != null)
            {
                WarningRecord record = new WarningRecord(this.Message);
                InvocationInfo variableValue = base.GetVariableValue("MyInvocation") as InvocationInfo;
                if (variableValue != null)
                {
                    record.SetInvocationInfo(variableValue);
                }
                commandRuntime.WriteWarning(record, false);
            }
            else
            {
                base.WriteWarning(this.Message);
            }
        }

        [Parameter(Position=0, Mandatory=true, ValueFromPipeline=true), AllowEmptyString, Alias(new string[] { "Msg" })]
        public string Message
        {
            get
            {
                return this.message;
            }
            set
            {
                this.message = value;
            }
        }
    }
}

