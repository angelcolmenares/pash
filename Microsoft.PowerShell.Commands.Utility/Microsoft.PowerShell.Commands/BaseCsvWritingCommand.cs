namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;

    public abstract class BaseCsvWritingCommand : PSCmdlet
    {
        private char _delimiter;
        private bool _noTypeInformation;
        private SwitchParameter _useculture;

        protected BaseCsvWritingCommand()
        {
        }

        protected override void BeginProcessing()
        {
            this._delimiter = ImportExportCSVHelper.SetDelimiter(this, base.ParameterSetName, this._delimiter, (bool) this._useculture);
        }

        public virtual void WriteCsvLine(string line)
        {
        }

        [Parameter(Position=1, ParameterSetName="Delimiter"), ValidateNotNull]
        public char Delimiter
        {
            get
            {
                return this._delimiter;
            }
            set
            {
                this._delimiter = value;
            }
        }

        public abstract PSObject InputObject { get; set; }

        [Parameter, Alias(new string[] { "NTI" })]
        public SwitchParameter NoTypeInformation
        {
            get
            {
                return this._noTypeInformation;
            }
            set
            {
                this._noTypeInformation = (bool) value;
            }
        }

        [Parameter(ParameterSetName="UseCulture")]
        public SwitchParameter UseCulture
        {
            get
            {
                return this._useculture;
            }
            set
            {
                this._useculture = value;
            }
        }
    }
}

