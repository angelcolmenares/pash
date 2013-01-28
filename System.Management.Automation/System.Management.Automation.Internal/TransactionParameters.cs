namespace System.Management.Automation.Internal
{
    using System;
    using System.Management.Automation;

    public sealed class TransactionParameters
    {
        private MshCommandRuntime commandRuntime;

        internal TransactionParameters(MshCommandRuntime commandRuntime)
        {
            this.commandRuntime = commandRuntime;
        }

        [Parameter, Alias(new string[] { "usetx" })]
        public SwitchParameter UseTransaction
        {
            get
            {
                return this.commandRuntime.UseTransaction;
            }
            set
            {
                this.commandRuntime.UseTransaction = value;
            }
        }
    }
}

