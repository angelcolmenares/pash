namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;

    [Cmdlet("ConvertTo", "Csv", DefaultParameterSetName="Delimiter", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135203", RemotingCapability=RemotingCapability.None), OutputType(new Type[] { typeof(string) })]
    public sealed class ConvertToCsvCommand : BaseCsvWritingCommand
    {
        private PSObject _object;
        private IList<string> _propertyNames;
        private ExportCsvHelper helper;

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            this.helper = new ExportCsvHelper(this, base.Delimiter);
        }

        protected override void ProcessRecord()
        {
            if (this.InputObject != null)
            {
                if (this._propertyNames == null)
                {
                    this._propertyNames = this.helper.BuildPropertyNames(this.InputObject, this._propertyNames);
                    if (base.NoTypeInformation == 0)
                    {
                        this.WriteCsvLine(this.helper.GetTypeString(this.InputObject));
                    }
                    string str = this.helper.ConvertPropertyNamesCSV(this._propertyNames);
                    if (!str.Equals(""))
                    {
                        this.WriteCsvLine(str);
                    }
                }
                string line = this.helper.ConvertPSObjectToCSV(this.InputObject, this._propertyNames);
                if (line != "")
                {
                    this.WriteCsvLine(line);
                }
            }
        }

        public override void WriteCsvLine(string line)
        {
            base.WriteObject(line);
        }

        [Parameter(ValueFromPipeline=true, Mandatory=true, ValueFromPipelineByPropertyName=true, Position=0)]
        public override PSObject InputObject
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
    }
}

