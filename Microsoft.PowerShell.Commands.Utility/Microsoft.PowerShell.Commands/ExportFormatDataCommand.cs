namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;

    [Cmdlet("Export", "FormatData", DefaultParameterSetName="ByPath", HelpUri="http://go.microsoft.com/fwlink/?LinkID=144302")]
    public class ExportFormatDataCommand : PSCmdlet
    {
        private string _filepath;
        private bool _force;
        private bool _includescriptblock;
        private bool _noclobber;
        private ExtendedTypeDefinition[] _typeDefinition;
        private List<ExtendedTypeDefinition> _typeDefinitions = new List<ExtendedTypeDefinition>();
        private bool isLiteralPath;

        protected override void EndProcessing()
        {
            FormatXMLWriter.WriteToPS1XML(this, this._typeDefinitions, this._filepath, this._force, this._noclobber, this._includescriptblock, this.isLiteralPath);
        }

        protected override void ProcessRecord()
        {
            foreach (ExtendedTypeDefinition definition in this._typeDefinition)
            {
                this._typeDefinitions.Add(definition);
            }
        }

        [Parameter]
        public SwitchParameter Force
        {
            get
            {
                return this._force;
            }
            set
            {
                this._force = (bool) value;
            }
        }

        [Parameter]
        public SwitchParameter IncludeScriptBlock
        {
            get
            {
                return this._includescriptblock;
            }
            set
            {
                this._includescriptblock = (bool) value;
            }
        }

        [Parameter(Mandatory=true, ValueFromPipeline=true)]
        public ExtendedTypeDefinition[] InputObject
        {
            get
            {
                return this._typeDefinition;
            }
            set
            {
                this._typeDefinition = value;
            }
        }

        [Parameter(ParameterSetName="ByLiteralPath", Mandatory=true), Alias(new string[] { "PSPath" })]
        public string LiteralPath
        {
            get
            {
                return this._filepath;
            }
            set
            {
                this._filepath = value;
                this.isLiteralPath = true;
            }
        }

        [Parameter, Alias(new string[] { "NoOverwrite" })]
        public SwitchParameter NoClobber
        {
            get
            {
                return this._noclobber;
            }
            set
            {
                this._noclobber = (bool) value;
            }
        }

        [Alias(new string[] { "FilePath" }), Parameter(ParameterSetName="ByPath", Mandatory=true)]
        public string Path
        {
            get
            {
                return this._filepath;
            }
            set
            {
                this._filepath = value;
            }
        }
    }
}

