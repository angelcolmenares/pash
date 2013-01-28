namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;

    [Cmdlet("Import", "Clixml", SupportsPaging=true, DefaultParameterSetName="ByPath", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113340")]
    public sealed class ImportClixmlCommand : PSCmdlet, IDisposable
    {
        private string[] _paths;
        private bool disposed;
        private ImportXmlHelper helper;
        private bool isLiteralPath;

        public void Dispose()
        {
            if (!this.disposed)
            {
                GC.SuppressFinalize(this);
                if (this.helper != null)
                {
                    this.helper.Dispose();
                    this.helper = null;
                }
                this.disposed = true;
            }
        }

        protected override void ProcessRecord()
        {
            if (this._paths != null)
            {
                foreach (string str in this._paths)
                {
                    this.helper = new ImportXmlHelper(str, this, this.isLiteralPath);
                    this.helper.Import();
                }
            }
        }

        protected override void StopProcessing()
        {
            base.StopProcessing();
            this.helper.Stop();
        }

        [Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ByLiteralPath"), Alias(new string[] { "PSPath" })]
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

        [Parameter(Position=0, Mandatory=true, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ByPath")]
        public string[] Path
        {
            get
            {
                return this._paths;
            }
            set
            {
                this._paths = value;
            }
        }
    }
}

