namespace Microsoft.PowerShell.Commands
{
    using Microsoft.Win32;
    using System;
    using System.Management.Automation;

    public class RegistryProviderSetItemDynamicParameter
    {
        private RegistryValueKind type;

        [Parameter(ValueFromPipelineByPropertyName=true)]
        public RegistryValueKind Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }
    }
}

