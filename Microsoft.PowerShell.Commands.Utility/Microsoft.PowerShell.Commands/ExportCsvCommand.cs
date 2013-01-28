namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;
    using System.Text;

    [Cmdlet("Export", "Csv", SupportsShouldProcess=true, DefaultParameterSetName="Delimiter", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113299")]
    public sealed class ExportCsvCommand : BaseCsvWritingCommand, IDisposable
    {
        private bool _disposed;
        private string _encodingParameter;
        private bool _force;
        private FileStream _fs;
        private PSObject _object;
        private string _path;
        private IList<string> _preexistingPropertyNames;
        private IList<string> _propertyNames;
        private StreamWriter _sw;
        private ExportCsvHelper helper;
        private bool isActuallyAppending;
        private bool isLiteralPath;
        private bool noclobber;
        private FileInfo readOnlyFileInfo;
        private bool shouldProcess;
        private bool specifiedPath;

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            if (!(this.specifiedPath ^ this.isLiteralPath))
            {
                InvalidOperationException exception = new InvalidOperationException(CsvCommandStrings.CannotSpecifyPathAndLiteralPath);
                ErrorRecord errorRecord = new ErrorRecord(exception, "CannotSpecifyPathAndLiteralPath", ErrorCategory.InvalidData, null);
                base.ThrowTerminatingError(errorRecord);
            }
            this.shouldProcess = base.ShouldProcess(this.Path);
            if (this.shouldProcess)
            {
                this.CreateFileStream();
                this.helper = new ExportCsvHelper(this, base.Delimiter);
            }
        }

        private void CleanUp()
        {
            if (this._fs != null)
            {
                if (this._sw != null)
                {
                    this._sw.Flush();
                    this._sw.Close();
                    this._sw = null;
                }
                this._fs.Close();
                this._fs = null;
                if (this.readOnlyFileInfo != null)
                {
                    this.readOnlyFileInfo.Attributes |= System.IO.FileAttributes.ReadOnly;
                }
            }
            if (this.helper != null)
            {
                this.helper.Dispose();
            }
        }

        private void CreateFileStream()
        {
            string path = PathUtils.ResolveFilePath(this.Path, this, this.isLiteralPath);
            bool flag = true;
            if ((this.Append != 0) && File.Exists(path))
            {
                using (StreamReader reader = PathUtils.OpenStreamReader(this, this.Path, this._encodingParameter, this.isLiteralPath))
                {
                    flag = reader.Peek() == -1;
                }
            }
            this.isActuallyAppending = ((this.Append != 0) && File.Exists(path)) && !flag;
            if (this.isActuallyAppending)
            {
                System.Text.Encoding currentEncoding;
                using (StreamReader reader2 = PathUtils.OpenStreamReader(this, this.Path, this._encodingParameter, this.isLiteralPath))
                {
                    ImportCsvHelper helper = new ImportCsvHelper(this, base.Delimiter, null, null, reader2);
                    helper.ReadHeader();
                    this._preexistingPropertyNames = helper.Header;
                    currentEncoding = reader2.CurrentEncoding;
                }
                PathUtils.MasterStreamOpen(this, this.Path, currentEncoding, false, (bool) this.Append, (bool) this.Force, (bool) this.NoClobber, out this._fs, out this._sw, out this.readOnlyFileInfo, this.isLiteralPath);
            }
            else
            {
                PathUtils.MasterStreamOpen(this, this.Path, this._encodingParameter ?? "ASCII", false, (bool) this.Append, (bool) this.Force, (bool) this.NoClobber, out this._fs, out this._sw, out this.readOnlyFileInfo, this.isLiteralPath);
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
            this.CleanUp();
        }

        protected override void ProcessRecord()
        {
            if (((this.InputObject != null) && (this._sw != null)) && this.shouldProcess)
            {
                if (this._propertyNames == null)
                {
                    this._propertyNames = this.helper.BuildPropertyNames(this.InputObject, this._propertyNames);
                    if (this.isActuallyAppending && (this._preexistingPropertyNames != null))
                    {
                        this.ReconcilePreexistingPropertyNames();
                    }
                    if (!this.isActuallyAppending)
                    {
                        if (base.NoTypeInformation == 0)
                        {
                            this.WriteCsvLine(this.helper.GetTypeString(this.InputObject));
                        }
                        this.WriteCsvLine(this.helper.ConvertPropertyNamesCSV(this._propertyNames));
                    }
                }
                string line = this.helper.ConvertPSObjectToCSV(this.InputObject, this._propertyNames);
                this.WriteCsvLine(line);
                this._sw.Flush();
            }
        }

        private void ReconcilePreexistingPropertyNames()
        {
            HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string str in this._propertyNames)
            {
                set.Add(str);
            }
            foreach (string str2 in this._preexistingPropertyNames)
            {
                if (!set.Contains(str2) && (this.Force == 0))
                {
                    InvalidOperationException exception = new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, CsvCommandStrings.CannotAppendCsvWithMismatchedPropertyNames, new object[] { str2, this.Path }));
                    ErrorRecord errorRecord = new ErrorRecord(exception, "CannotAppendCsvWithMismatchedPropertyNames", ErrorCategory.InvalidData, str2);
                    base.ThrowTerminatingError(errorRecord);
                }
            }
            this._propertyNames = this._preexistingPropertyNames;
            this._preexistingPropertyNames = null;
        }

        public override void WriteCsvLine(string line)
        {
            if (this._disposed)
            {
                throw PSTraceSource.NewObjectDisposedException("ExportCsvCommand");
            }
            this._sw.WriteLine(line);
        }

        [Parameter]
        public SwitchParameter Append { get; set; }

        [ValidateSet(new string[] { "Unicode", "UTF7", "UTF8", "ASCII", "UTF32", "BigEndianUnicode", "Default", "OEM" }), Parameter]
        public string Encoding
        {
            get
            {
                return this._encodingParameter;
            }
            set
            {
                this._encodingParameter = value;
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

        [Parameter(ValueFromPipeline=true, Mandatory=true, ValueFromPipelineByPropertyName=true)]
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

        [Alias(new string[] { "PSPath" }), Parameter, ValidateNotNullOrEmpty]
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

        [Parameter, Alias(new string[] { "NoOverwrite" })]
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

        [ValidateNotNullOrEmpty, Parameter(Position=0)]
        public string Path
        {
            get
            {
                return this._path;
            }
            set
            {
                this._path = value;
                this.specifiedPath = true;
            }
        }
    }
}

