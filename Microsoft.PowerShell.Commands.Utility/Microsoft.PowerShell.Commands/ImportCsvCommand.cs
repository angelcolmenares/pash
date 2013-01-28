namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.IO;
    using System.Management.Automation;

    [Cmdlet("Import", "Csv", DefaultParameterSetName="Delimiter", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113341")]
    public sealed class ImportCsvCommand : PSCmdlet
    {
        private bool _alreadyWarnedUnspecifiedNames;
        private char _delimiter;
        private string _encoding;
        private string[] _header;
        private string[] _paths;
        private bool _useculture;
        private bool isLiteralPath;
        private bool specifiedPath;

        protected override void BeginProcessing()
        {
            this._delimiter = ImportExportCSVHelper.SetDelimiter(this, base.ParameterSetName, this._delimiter, this._useculture);
        }

        protected override void ProcessRecord()
        {
            if (!(this.specifiedPath ^ this.isLiteralPath))
            {
                InvalidOperationException exception = new InvalidOperationException(CsvCommandStrings.CannotSpecifyPathAndLiteralPath);
                ErrorRecord errorRecord = new ErrorRecord(exception, "CannotSpecifyPathAndLiteralPath", ErrorCategory.InvalidData, null);
                base.ThrowTerminatingError(errorRecord);
            }
            if (this._paths != null)
            {
                foreach (string str in this._paths)
                {
                    using (StreamReader reader = PathUtils.OpenStreamReader(this, str, this.Encoding, this.isLiteralPath))
                    {
                        ImportCsvHelper helper = new ImportCsvHelper(this, this._delimiter, this._header, null, reader);
                        try
                        {
                            helper.Import(ref this._alreadyWarnedUnspecifiedNames);
                        }
                        catch (ExtendedTypeSystemException exception2)
                        {
                            ErrorRecord record2 = new ErrorRecord(exception2, "AlreadyPresentPSMemberInfoInternalCollectionAdd", ErrorCategory.NotSpecified, null);
                            base.ThrowTerminatingError(record2);
                        }
                    }
                }
            }
        }

        [ValidateNotNull, Parameter(Position=1, ParameterSetName="Delimiter")]
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

        [ValidateSet(new string[] { "Unicode", "UTF7", "UTF8", "ASCII", "UTF32", "BigEndianUnicode", "Default", "OEM" }), Parameter]
        public string Encoding
        {
            get
            {
                return this._encoding;
            }
            set
            {
                this._encoding = value;
            }
        }

        [Parameter(Mandatory=false), ValidateNotNullOrEmpty]
        public string[] Header
        {
            get
            {
                return this._header;
            }
            set
            {
                this._header = value;
            }
        }

        [Alias(new string[] { "PSPath" }), ValidateNotNullOrEmpty, Parameter(ValueFromPipelineByPropertyName=true)]
        public string[] LiteralPath
        {
            get
            {
                return this._paths;
            }
            set
            {
                this._paths = value;
                this.isLiteralPath = true;
            }
        }

        [Parameter(Position=0, ValueFromPipeline=true), ValidateNotNullOrEmpty]
        public string[] Path
        {
            get
            {
                return this._paths;
            }
            set
            {
                this._paths = value;
                this.specifiedPath = true;
            }
        }

        [Parameter(ParameterSetName="UseCulture", Mandatory=true), ValidateNotNull]
        public SwitchParameter UseCulture
        {
            get
            {
                return this._useculture;
            }
            set
            {
                this._useculture = (bool) value;
            }
        }
    }
}

