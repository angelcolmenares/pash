namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Diagnostics;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    [OutputType(new Type[] { typeof(TimeSpan) }), Cmdlet("Measure", "Command", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113348", RemotingCapability=RemotingCapability.None)]
    public sealed class MeasureCommandCommand : PSCmdlet
    {
        private PSObject inputObject = AutomationNull.Value;
        private ScriptBlock script;
        private Stopwatch stopWatch = new Stopwatch();

        protected override void EndProcessing()
        {
            base.WriteObject(this.stopWatch.Elapsed);
        }

        protected override void ProcessRecord()
        {
            this.stopWatch.Start();
            Pipe outputPipe = new Pipe {
                NullPipe = true
            };
            this.script.InvokeWithPipe(false, ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe, this.InputObject, new object[0], AutomationNull.Value, outputPipe, null, new object[0]);
            this.stopWatch.Stop();
        }

        [Parameter(Position=0, Mandatory=true)]
        public ScriptBlock Expression
        {
            get
            {
                return this.script;
            }
            set
            {
                this.script = value;
            }
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

