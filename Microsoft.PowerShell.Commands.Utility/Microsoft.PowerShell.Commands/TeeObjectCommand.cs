namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;
    using System.Management.Automation;

    [Cmdlet("Tee", "Object", DefaultParameterSetName="File", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113417")]
    public sealed class TeeObjectCommand : PSCmdlet, IDisposable
    {
        private bool alreadyDisposed;
        private bool append;
        private CommandWrapper commandWrapper;
        private string fileName;
        private PSObject inputObject;
        private string variable;

        protected override void BeginProcessing()
        {
            this.commandWrapper = new CommandWrapper();
            if (string.Equals(base.ParameterSetName, "File", StringComparison.OrdinalIgnoreCase))
            {
                this.commandWrapper.Initialize(base.Context, "out-file", typeof(OutFileCommand));
                this.commandWrapper.AddNamedParameter("filepath", this.fileName);
                this.commandWrapper.AddNamedParameter("append", this.append);
            }
            else if (string.Equals(base.ParameterSetName, "LiteralFile", StringComparison.OrdinalIgnoreCase))
            {
                this.commandWrapper.Initialize(base.Context, "out-file", typeof(OutFileCommand));
                this.commandWrapper.AddNamedParameter("LiteralPath", this.fileName);
                this.commandWrapper.AddNamedParameter("append", this.append);
            }
            else
            {
                this.commandWrapper.Initialize(base.Context, "set-variable", typeof(SetVariableCommand));
                this.commandWrapper.AddNamedParameter("name", this.variable);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool isDisposing)
        {
            if (!this.alreadyDisposed)
            {
                this.alreadyDisposed = true;
                if (isDisposing && (this.commandWrapper != null))
                {
                    this.commandWrapper.Dispose();
                    this.commandWrapper = null;
                }
            }
        }

        protected override void EndProcessing()
        {
            this.commandWrapper.ShutDown();
        }

        ~TeeObjectCommand()
        {
            this.Dispose(false);
        }

        protected override void ProcessRecord()
        {
            this.commandWrapper.Process(this.inputObject);
            base.WriteObject(this.inputObject);
        }

        [Parameter(ParameterSetName="File")]
        public SwitchParameter Append
        {
            get
            {
                return this.append;
            }
            set
            {
                this.append = (bool) value;
            }
        }

        [Parameter(Mandatory=true, Position=0, ParameterSetName="File")]
        public string FilePath
        {
            get
            {
                return this.fileName;
            }
            set
            {
                this.fileName = value;
            }
        }

        [Parameter(ValueFromPipeline=true)]
        public PSObject InputObject
        {
            get
            {
                return this.inputObject;
            }
            set
            {
                this.inputObject = value;
            }
        }

        [Alias(new string[] { "PSPath" }), Parameter(Mandatory=true, ParameterSetName="LiteralFile")]
        public string LiteralPath
        {
            get
            {
                return this.fileName;
            }
            set
            {
                this.fileName = value;
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="Variable")]
        public string Variable
        {
            get
            {
                return this.variable;
            }
            set
            {
                this.variable = value;
            }
        }
    }
}

