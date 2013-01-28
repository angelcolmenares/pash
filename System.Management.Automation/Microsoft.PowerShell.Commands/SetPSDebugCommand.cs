namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;

    [Cmdlet("Set", "PSDebug", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113398")]
    public sealed class SetPSDebugCommand : PSCmdlet
    {
        private bool off;
        private bool? step;
        private bool? strict;
        private int trace = -1;

        protected override void BeginProcessing()
        {
            if (this.off)
            {
                base.Context.Debugger.DisableTracing();
                base.Context.EngineSessionState.GlobalScope.StrictModeVersion = null;
            }
            else
            {
                if ((this.trace >= 0) || this.step.HasValue)
                {
                    base.Context.Debugger.EnableTracing(this.trace, this.step);
                }
                if (this.strict.HasValue)
                {
                    base.Context.EngineSessionState.GlobalScope.StrictModeVersion = new Version(this.strict.Value ? 1 : 0, 0);
                }
            }
        }

        [Parameter(ParameterSetName="off")]
        public SwitchParameter Off
        {
            get
            {
                return this.off;
            }
            set
            {
                this.off = (bool) value;
            }
        }

        [Parameter(ParameterSetName="on")]
        public SwitchParameter Step
        {
            get
            {
                return this.step.Value;
            }
            set
            {
                this.step = new bool?((bool) value);
            }
        }

        [Parameter(ParameterSetName="on")]
        public SwitchParameter Strict
        {
            get
            {
                return this.strict.Value;
            }
            set
            {
                this.strict = new bool?((bool) value);
            }
        }

        [Parameter(ParameterSetName="on"), ValidateRange(0, 2)]
        public int Trace
        {
            get
            {
                return this.trace;
            }
            set
            {
                this.trace = value;
            }
        }
    }
}

