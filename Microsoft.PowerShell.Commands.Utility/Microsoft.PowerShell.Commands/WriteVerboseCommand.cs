namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;

    [Cmdlet("Write", "Verbose", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113429", RemotingCapability=RemotingCapability.None)]
    public sealed class WriteVerboseCommand : PSCmdlet
    {
        private string message;

        protected override void ProcessRecord()
        {
            MshCommandRuntime commandRuntime = base.CommandRuntime as MshCommandRuntime;
            if (commandRuntime != null)
            {
                VerboseRecord record = new VerboseRecord(this.Message);
                InvocationInfo variableValue = base.GetVariableValue("MyInvocation") as InvocationInfo;
                if (variableValue != null)
                {
                    record.SetInvocationInfo(variableValue);
                }
                commandRuntime.WriteVerbose(record, false);
            }
            else
            {
                base.WriteVerbose(this.Message);
            }
        }

        [Alias(new string[] { "Msg" }), Parameter(Position=0, Mandatory=true, ValueFromPipeline=true), AllowEmptyString]
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

