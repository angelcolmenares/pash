namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;

    public class FileSystemContentReaderDynamicParameters : FileSystemContentDynamicParametersBase
    {
        private string delimiter = "\n";
        private bool delimiterSpecified;
        private bool isRaw;
        private bool wait;

        [Parameter]
        public string Delimiter
        {
            get
            {
                return this.delimiter;
            }
            set
            {
                this.delimiterSpecified = true;
                this.delimiter = value;
            }
        }

        public bool DelimiterSpecified
        {
            get
            {
                return this.delimiterSpecified;
            }
        }

        [Parameter]
        public SwitchParameter Raw
        {
            get
            {
                return this.isRaw;
            }
            set
            {
                this.isRaw = (bool) value;
            }
        }

        [Parameter]
        public SwitchParameter Wait
        {
            get
            {
                return this.wait;
            }
            set
            {
                this.wait = (bool) value;
            }
        }
    }
}

