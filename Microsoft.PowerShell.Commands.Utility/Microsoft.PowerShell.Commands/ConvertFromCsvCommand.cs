namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Text;

    [Cmdlet("ConvertFrom", "Csv", DefaultParameterSetName="Delimiter", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135201", RemotingCapability=RemotingCapability.None)]
    public sealed class ConvertFromCsvCommand : PSCmdlet
    {
        private bool _alreadyWarnedUnspecifiedNames;
        private char _delimiter;
        private string[] _header;
        private PSObject[] _object;
        private string _typeName;
        private SwitchParameter _useculture;

        protected override void BeginProcessing()
        {
            this._delimiter = ImportExportCSVHelper.SetDelimiter(this, base.ParameterSetName, this._delimiter, (bool) this._useculture);
        }

        protected override void ProcessRecord()
        {
            foreach (PSObject obj2 in this._object)
            {
                using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(obj2.ToString())))
                {
                    using (StreamReader reader = new StreamReader(stream, Encoding.Unicode))
                    {
                        ImportCsvHelper helper = new ImportCsvHelper(this, this._delimiter, this._header, this._typeName, reader);
                        try
                        {
                            helper.Import(ref this._alreadyWarnedUnspecifiedNames);
                        }
                        catch (ExtendedTypeSystemException exception)
                        {
                            ErrorRecord errorRecord = new ErrorRecord(exception, "AlreadyPresentPSMemberInfoInternalCollectionAdd", ErrorCategory.NotSpecified, null);
                            base.ThrowTerminatingError(errorRecord);
                        }
                        if ((this._header == null) && (helper.Header != null))
                        {
                            this._header = helper.Header.ToArray<string>();
                        }
                        if ((this._typeName == null) && (helper.TypeName != null))
                        {
                            this._typeName = helper.TypeName;
                        }
                    }
                }
            }
        }

        [Parameter(Position=1, ParameterSetName="Delimiter"), ValidateNotNull, ValidateNotNullOrEmpty]
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

        [ValidateNotNull, Parameter(Mandatory=false), ValidateNotNullOrEmpty]
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

        [Parameter(ValueFromPipeline=true, Mandatory=true, ValueFromPipelineByPropertyName=true, Position=0), ValidateNotNullOrEmpty, ValidateNotNull]
        public PSObject[] InputObject
        {
            get
            {
                return this._object;
            }
            set
            {
                this._object = value;
            }
        }

        [Parameter(ParameterSetName="UseCulture", Mandatory=true), ValidateNotNullOrEmpty, ValidateNotNull]
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

