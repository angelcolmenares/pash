namespace System.Management.Automation.Internal
{
    using System;
    using System.Management.Automation;

    public sealed class ShouldProcessParameters
    {
        private MshCommandRuntime commandRuntime;

        internal ShouldProcessParameters(MshCommandRuntime commandRuntime)
        {
            if (commandRuntime == null)
            {
                throw PSTraceSource.NewArgumentNullException("commandRuntime");
            }
            this.commandRuntime = commandRuntime;
        }

        [Parameter, Alias(new string[] { "cf" })]
        public SwitchParameter Confirm
        {
            get
            {
                return this.commandRuntime.Confirm;
            }
            set
            {
                this.commandRuntime.Confirm = value;
            }
        }

        [Alias(new string[] { "wi" }), Parameter]
        public SwitchParameter WhatIf
        {
            get
            {
                return this.commandRuntime.WhatIf;
            }
            set
            {
                this.commandRuntime.WhatIf = value;
            }
        }
    }
}

