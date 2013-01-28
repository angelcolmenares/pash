namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.IO;
    using System.Management.Automation;

    internal sealed class GetChildDynamicParameters
    {
        private bool attributeDirectory;
        private bool attributeFile;
        private bool attributeHidden;
        private bool attributeReadOnly;
        private bool attributeSystem;
        private FlagsExpression<FileAttributes> evaluator;

        [Parameter]
        public FlagsExpression<FileAttributes> Attributes
        {
            get
            {
                return this.evaluator;
            }
            set
            {
                this.evaluator = value;
            }
        }

        [Parameter, Alias(new string[] { "ad", "d" })]
        public SwitchParameter Directory
        {
            get
            {
                return this.attributeDirectory;
            }
            set
            {
                this.attributeDirectory = (bool) value;
            }
        }

        [Alias(new string[] { "af" }), Parameter]
        public SwitchParameter File
        {
            get
            {
                return this.attributeFile;
            }
            set
            {
                this.attributeFile = (bool) value;
            }
        }

        [Parameter, Alias(new string[] { "ah", "h" })]
        public SwitchParameter Hidden
        {
            get
            {
                return this.attributeHidden;
            }
            set
            {
                this.attributeHidden = (bool) value;
            }
        }

        [Parameter, Alias(new string[] { "ar" })]
        public SwitchParameter ReadOnly
        {
            get
            {
                return this.attributeReadOnly;
            }
            set
            {
                this.attributeReadOnly = (bool) value;
            }
        }

        [Parameter, Alias(new string[] { "as" })]
        public SwitchParameter System
        {
            get
            {
                return this.attributeSystem;
            }
            set
            {
                this.attributeSystem = (bool) value;
            }
        }
    }
}

