namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;

    [Cmdlet("Write", "Debug", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113424", RemotingCapability=RemotingCapability.None)]
    public sealed class WriteDebugCommand : PSCmdlet
    {
        private string message;

        protected override void ProcessRecord()
        {
            MshCommandRuntime commandRuntime = base.CommandRuntime as MshCommandRuntime;
            if (commandRuntime != null)
            {
                DebugRecord record = new DebugRecord(this.Message);
                InvocationInfo variableValue = base.GetVariableValue("MyInvocation") as InvocationInfo;
                if (variableValue != null)
                {
                    record.SetInvocationInfo(variableValue);
                }
                commandRuntime.WriteDebug(record, false);
            }
            else
            {
                base.WriteDebug(this.Message);
            }
        }

        [Parameter(Position=0, Mandatory=true, ValueFromPipeline=true), Alias(new string[] { "Msg" }), AllowEmptyString]
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

