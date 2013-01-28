namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;

    public class FunctionProviderDynamicParameters
    {
        private ScopedItemOptions options;
        private bool optionsSet;

        [Parameter]
        public ScopedItemOptions Options
        {
            get
            {
                return this.options;
            }
            set
            {
                this.optionsSet = true;
                this.options = value;
            }
        }

        internal bool OptionsSet
        {
            get
            {
                return this.optionsSet;
            }
        }
    }
}

