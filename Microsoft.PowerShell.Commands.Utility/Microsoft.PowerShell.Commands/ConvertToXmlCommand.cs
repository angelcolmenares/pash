namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.IO;
    using System.Management.Automation;
    using System.Xml;

    [Cmdlet("ConvertTo", "Xml", SupportsShouldProcess=false, HelpUri="http://go.microsoft.com/fwlink/?LinkID=135204", RemotingCapability=RemotingCapability.None), OutputType(new Type[] { typeof(XmlDocument), typeof(string) })]
    public sealed class ConvertToXmlCommand : PSCmdlet, IDisposable
    {
        private string _as = "Document";
        private int _depth;
        private bool _disposed;
        private MemoryStream _ms;
        private bool _notypeinformation;
        private PSObject _object;
        private CustomSerialization _serializer;
        private XmlTextWriter _xw;

        protected override void BeginProcessing()
        {
            if (!this._as.Equals("Stream", StringComparison.OrdinalIgnoreCase))
            {
                this.CreateMemoryStream();
            }
            else
            {
                base.WriteObject("<?xml version=\"1.0\"?>");
                base.WriteObject("<Objects>");
            }
        }

        private void CleanUp()
        {
            if (this._ms != null)
            {
                if (this._xw != null)
                {
                    this._xw.Close();
                    this._xw = null;
                }
                this._ms.Close();
                this._ms = null;
            }
        }

        private void CreateMemoryStream()
        {
            this._ms = new MemoryStream();
            this._xw = new XmlTextWriter(this._ms, null);
            this._xw.Formatting = Formatting.Indented;
            if (!this._as.Equals("Stream", StringComparison.OrdinalIgnoreCase))
            {
                this._xw.WriteStartDocument();
            }
            if (this._depth == 0)
            {
                this._serializer = new CustomSerialization(this._xw, this._notypeinformation);
            }
            else
            {
                this._serializer = new CustomSerialization(this._xw, this._notypeinformation, this._depth);
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
            if (this._as.Equals("Stream", StringComparison.OrdinalIgnoreCase))
            {
                base.WriteObject("</Objects>");
            }
            else
            {
                this._ms.Position = 0L;
                if (this._as.Equals("Document", StringComparison.OrdinalIgnoreCase))
                {
                    XmlDocument sendToPipeline = new XmlDocument();
                    sendToPipeline.Load(this._ms);
                    base.WriteObject(sendToPipeline);
                }
                else if (this._as.Equals("String", StringComparison.OrdinalIgnoreCase))
                {
                    string str = new StreamReader(this._ms).ReadToEnd();
                    base.WriteObject(str);
                }
            }
            this.CleanUp();
        }

        protected override void ProcessRecord()
        {
            if (this._as.Equals("Stream", StringComparison.OrdinalIgnoreCase))
            {
                this.CreateMemoryStream();
                if (this._serializer != null)
                {
                    this._serializer.SerializeAsStream(this._object);
                }
                if (this._serializer != null)
                {
                    this._serializer.DoneAsStream();
                    this._serializer = null;
                }
                this._ms.Position = 0L;
                string sendToPipeline = new StreamReader(this._ms).ReadToEnd();
                base.WriteObject(sendToPipeline);
                this.CleanUp();
            }
            else if (this._serializer != null)
            {
                this._serializer.Serialize(this._object);
            }
        }

        protected override void StopProcessing()
        {
            this._serializer.Stop();
        }

        [ValidateSet(new string[] { "Stream", "String", "Document" }), Parameter, ValidateNotNullOrEmpty]
        public string As
        {
            get
            {
                return this._as;
            }
            set
            {
                this._as = value;
            }
        }

        [Parameter(HelpMessage="Specifies how many levels of contained objects should be included in the XML representation"), ValidateRange(1, 0x7fffffff)]
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

        [Parameter(Position=0, ValueFromPipeline=true, Mandatory=true), AllowNull]
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

        [Parameter(HelpMessage="Specifies not to include the Type information in the XML representation")]
        public SwitchParameter NoTypeInformation
        {
            get
            {
                return this._notypeinformation;
            }
            set
            {
                this._notypeinformation = (bool) value;
            }
        }
    }
}

