namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.IO;
    using System.Management.Automation;
    using System.Xml;

    [Cmdlet("Export", "Clixml", SupportsShouldProcess=true, DefaultParameterSetName="ByPath", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113297")]
    public sealed class ExportClixmlCommand : PSCmdlet, IDisposable
    {
        private int _depth;
        private bool _disposed;
        private string _encoding = "Unicode";
        private bool _force;
        private FileStream _fs;
        private PSObject _object;
        private string _path;
        private Serializer _serializer;
        private XmlWriter _xw;
        private bool isLiteralPath;
        private bool noclobber;
        private FileInfo readOnlyFileInfo;

        protected override void BeginProcessing()
        {
            this.CreateFileStream();
        }

        private void CleanUp()
        {
            if (this._fs != null)
            {
                if (this._xw != null)
                {
                    this._xw.Close();
                    this._xw = null;
                }
                this._fs.Close();
                this._fs = null;
            }
            if (this.readOnlyFileInfo != null)
            {
                this.readOnlyFileInfo.Attributes |= System.IO.FileAttributes.ReadOnly;
                this.readOnlyFileInfo = null;
            }
        }

        private void CreateFileStream()
        {
            if (base.ShouldProcess(this.Path))
            {
                StreamWriter writer;
                PathUtils.MasterStreamOpen(this, this.Path, this.Encoding, false, false, (bool) this.Force, (bool) this.NoClobber, out this._fs, out writer, out this.readOnlyFileInfo, this.isLiteralPath);
                XmlWriterSettings settings = new XmlWriterSettings {
                    CloseOutput = true,
                    Encoding = writer.Encoding,
                    Indent = true,
                    OmitXmlDeclaration = true
                };
                this._xw = XmlWriter.Create(writer, settings);
                if (this._depth == 0)
                {
                    this._serializer = new Serializer(this._xw);
                }
                else
                {
                    this._serializer = new Serializer(this._xw, this._depth, true);
                }
            }
        }

        public void Dispose()
        {
            if (!this._disposed)
            {
                this.CleanUp();
            }
            this._disposed = true;
        }

        protected override void EndProcessing()
        {
            if (this._serializer != null)
            {
                this._serializer.Done();
                this._serializer = null;
            }
            this.CleanUp();
        }

        protected override void ProcessRecord()
        {
            if (this._serializer != null)
            {
                this._serializer.Serialize(this._object);
            }
            this._xw.Flush();
        }

        protected override void StopProcessing()
        {
            base.StopProcessing();
            this._serializer.Stop();
        }

        [Parameter, ValidateRange(1, 0x7fffffff)]
        public int Depth
        {
            get
            {
                return this._depth;
            }
            set
            {
                this._depth = value;
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

        [AllowNull, Parameter(ValueFromPipeline=true, Mandatory=true)]
        public PSObject InputObject
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

        [Alias(new string[] { "PSPath" }), Parameter(Mandatory=true, ParameterSetName="ByLiteralPath")]
        public string LiteralPath
        {
            get
            {
                return this._path;
            }
            set
            {
                this._path = value;
                this.isLiteralPath = true;
            }
        }

        [Alias(new string[] { "NoOverwrite" }), Parameter]
        public SwitchParameter NoClobber
        {
            get
            {
                return this.noclobber;
            }
            set
            {
                this.noclobber = (bool) value;
            }
        }

        [Parameter(Mandatory=true, Position=0, ParameterSetName="ByPath")]
        public string Path
        {
            get
            {
                return this._path;
            }
            set
            {
                this._path = value;
            }
        }
    }
}

