namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;

    public class WriteAliasCommandBase : PSCmdlet
    {
        private string aliasValue;
        private string description = string.Empty;
        private bool force;
        private string name;
        private ScopedItemOptions options;
        private bool passThru;
        private string scope;

        [Parameter]
        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = value;
            }
        }

        [Parameter]
        public SwitchParameter Force
        {
            get
            {
                return this.force;
            }
            set
            {
                this.force = (bool) value;
            }
        }

        [Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true)]
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        [Parameter]
        public ScopedItemOptions Option
        {
            get
            {
                return this.options;
            }
            set
            {
                this.options = value;
            }
        }

        [Parameter]
        public SwitchParameter PassThru
        {
            get
            {
                return this.passThru;
            }
            set
            {
                this.passThru = (bool) value;
            }
        }

        [Parameter]
        public string Scope
        {
            get
            {
                return this.scope;
            }
            set
            {
                this.scope = value;
            }
        }

        [Parameter(Position=1, Mandatory=true, ValueFromPipelineByPropertyName=true)]
        public string Value
        {
            get
            {
                return this.aliasValue;
            }
            set
            {
                this.aliasValue = value;
            }
        }
    }
}

