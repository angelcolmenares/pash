namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;

    [Cmdlet("Out", "File", SupportsShouldProcess=true, DefaultParameterSetName="ByPath", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113363")]
    public class OutFileCommand : FrontEndCommandBase
    {
        private bool append;
        private string encoding;
        private string fileName;
        private bool force;
        private FileStream fs;
        private bool isLiteralPath;
        private bool noclobber;
        private bool processRecordExecuted;
        private FileInfo readOnlyFileInfo;
        private StreamWriter sw;
        private int? width = null;

        public OutFileCommand()
        {
            base.implementation = new OutputManagerInner();
        }

        protected override void BeginProcessing()
        {
            OutputManagerInner implementation = (OutputManagerInner) base.implementation;
            implementation.LineOutput = this.InstantiateLineOutputInterface();
            if (this.sw != null)
            {
                base.BeginProcessing();
            }
        }

        private void CleanUp()
        {
            if (this.fs != null)
            {
                this.fs.Close();
                this.fs = null;
            }
            if (this.readOnlyFileInfo != null)
            {
                this.readOnlyFileInfo.Attributes |= System.IO.FileAttributes.ReadOnly;
                this.readOnlyFileInfo = null;
            }
        }

        protected override void EndProcessing()
        {
            if (this.processRecordExecuted && (this.sw != null))
            {
                base.EndProcessing();
                this.sw.Flush();
                this.CleanUp();
            }
        }

        private LineOutput InstantiateLineOutputInterface()
        {
            string action = StringUtil.Format(FormatAndOut_out_xxx.OutFile_Action, new object[0]);
            if (base.ShouldProcess(this.FilePath, action))
            {
                PathUtils.MasterStreamOpen(this, this.FilePath, this.encoding, false, (bool) this.Append, (bool) this.Force, (bool) this.NoClobber, out this.fs, out this.sw, out this.readOnlyFileInfo, this.isLiteralPath);
            }
            else
            {
                return null;
            }
            int columns = 80;
            if (this.width.HasValue)
            {
                columns = this.width.Value;
            }
            else
            {
                try
                {
                    columns = base.Host.UI.RawUI.BufferSize.Width - 1;
                }
                catch (HostException)
                {
                }
            }
            return new TextWriterLineOutput(this.sw, columns);
        }

        protected override void InternalDispose()
        {
            base.InternalDispose();
            this.CleanUp();
        }

        protected override void ProcessRecord()
        {
            this.processRecordExecuted = true;
            if (this.sw != null)
            {
                base.ProcessRecord();
                this.sw.Flush();
            }
        }

        [Parameter]
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

        [ValidateNotNullOrEmpty, ValidateSet(new string[] { "unknown", "string", "unicode", "bigendianunicode", "utf8", "utf7", "utf32", "ascii", "default", "oem" }), Parameter(Position=1)]
        public string Encoding
        {
            get
            {
                return this.encoding;
            }
            set
            {
                this.encoding = value;
            }
        }

        [Parameter(Mandatory=true, Position=0, ParameterSetName="ByPath")]
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

        [Parameter]
        public SwitchParameter Force
        {
            get
            {
                return this.force;
            }
            set
            {
                this.force = (bool) value;
            }
        }

        [Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ByLiteralPath"), Alias(new string[] { "PSPath" })]
        public string LiteralPath
        {
            get
            {
                return this.fileName;
            }
            set
            {
                this.fileName = value;
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

        [ValidateRange(2, 0x7fffffff), Parameter]
        public int Width
        {
            get
            {
                if (!this.width.HasValue)
                {
                    return 0;
                }
                return this.width.Value;
            }
            set
            {
                this.width = new int?(value);
            }
        }
    }
}

