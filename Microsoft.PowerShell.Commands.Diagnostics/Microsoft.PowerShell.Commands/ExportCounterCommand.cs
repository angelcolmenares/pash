namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Diagnostics.Common;
    using Microsoft.PowerShell.Commands.GetCounter;
    using Microsoft.Powershell.Commands.GetCounter.PdhNative;
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Management.Automation;
    using System.Reflection;
    using System.Resources;

    [Cmdlet("Export", "Counter", DefaultParameterSetName="ExportCounterSet", HelpUri="http://go.microsoft.com/fwlink/?LinkID=138337")]
    public sealed class ExportCounterCommand : PSCmdlet
    {
        private SwitchParameter _circular;
        private PerformanceCounterSampleSet[] _counterSampleSets = new PerformanceCounterSampleSet[0];
        private SwitchParameter _force;
        private string _format = "BLG";
        private long _maxSize;
        private PdhLogFileType _outputFormat = PdhLogFileType.PDH_LOG_TYPE_BINARY;
        private string _path;
        private PdhHelper _pdhHelper;
        private bool _queryInitialized;
        private string _resolvedPath;
        private ResourceManager _resourceMgr;
        private bool _stopping;

        protected override void BeginProcessing()
        {
            this._resourceMgr = new ResourceManager("GetEventResources", Assembly.GetExecutingAssembly());
            if ((Environment.OSVersion.Version.Major < 6) || ((Environment.OSVersion.Version.Major == 6) && (Environment.OSVersion.Version.Minor < 1)))
            {
                Exception exception = new Exception(this._resourceMgr.GetString("ExportCtrWin7Required"));
                base.ThrowTerminatingError(new ErrorRecord(exception, "ExportCtrWin7Required", ErrorCategory.NotImplemented, null));
            }
            this._pdhHelper = new PdhHelper(Environment.OSVersion.Version.Major < 6);
            this.ValidateFormat();
            if (this.Circular.IsPresent && (this._maxSize == 0))
            {
                Exception exception2 = new Exception(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("CounterCircularNoMaxSize"), new object[0]));
                base.WriteError(new ErrorRecord(exception2, "CounterCircularNoMaxSize", ErrorCategory.InvalidResult, null));
            }
            long res = this._pdhHelper.ConnectToDataSource();
            if (res != 0)
            {
                this.ReportPdhError(res, true);
            }
            res = this._pdhHelper.OpenQuery();
            if (res != 0)
            {
                this.ReportPdhError(res, true);
            }
        }

        protected override void EndProcessing()
        {
            this._pdhHelper.Dispose();
        }

        protected override void ProcessRecord()
        {
            this.ResolvePath();
            long res = 0;
            if (!this._queryInitialized)
            {
                if (this._format.ToLower(CultureInfo.InvariantCulture).Equals("blg"))
                {
                    res = this._pdhHelper.AddRelogCounters(this._counterSampleSets[0]);
                }
                else
                {
                    res = this._pdhHelper.AddRelogCountersPreservingPaths(this._counterSampleSets[0]);
                }
                if (res != 0)
                {
                    this.ReportPdhError(res, true);
                }
                res = this._pdhHelper.OpenLogForWriting(this._resolvedPath, this._outputFormat, this.Force.IsPresent, (this._maxSize * 0x400) * 0x400, this.Circular.IsPresent, null);
                if (res == 0xc0000bd2L)
                {
                    Exception exception = new Exception(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("CounterFileExists"), new object[] { this._resolvedPath }));
                    base.ThrowTerminatingError(new ErrorRecord(exception, "CounterFileExists", ErrorCategory.InvalidResult, null));
                }
                else if (res == 0xc0000bc9L)
                {
                    Exception exception2 = new Exception(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("FileCreateFailed"), new object[] { this._resolvedPath }));
                    base.ThrowTerminatingError(new ErrorRecord(exception2, "FileCreateFailed", ErrorCategory.InvalidResult, null));
                }
                else if (res == 0xc0000bcaL)
                {
                    Exception exception3 = new Exception(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("FileOpenFailed"), new object[] { this._resolvedPath }));
                    base.ThrowTerminatingError(new ErrorRecord(exception3, "FileOpenFailed", ErrorCategory.InvalidResult, null));
                }
                else if (res != 0)
                {
                    this.ReportPdhError(res, true);
                }
                this._queryInitialized = true;
            }
            foreach (PerformanceCounterSampleSet set in this._counterSampleSets)
            {
                this._pdhHelper.ResetRelogValues();
                foreach (PerformanceCounterSample sample in set.CounterSamples)
                {
                    bool bUnknownPath = false;
                    res = this._pdhHelper.SetCounterValue(sample, out bUnknownPath);
                    if (bUnknownPath)
                    {
                        Exception exception4 = new Exception(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("CounterExportSampleNotInInitialSet"), new object[] { sample.Path, this._resolvedPath }));
                        base.WriteError(new ErrorRecord(exception4, "CounterExportSampleNotInInitialSet", ErrorCategory.InvalidResult, null));
                    }
                    else if (res != 0)
                    {
                        this.ReportPdhError(res, true);
                    }
                }
                res = this._pdhHelper.WriteRelogSample(set.Timestamp);
                if (res != 0)
                {
                    this.ReportPdhError(res, true);
                }
                if (this._stopping)
                {
                    return;
                }
            }
        }

        private void ReportPdhError(long res, bool bTerminate)
        {
            string str;
            if (CommonUtilities.FormatMessageFromModule(res, "pdh.dll", out str) != 0)
            {
                str = string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("CounterApiError"), new object[] { res });
            }
            Exception exception = new Exception(str);
            if (bTerminate)
            {
                base.ThrowTerminatingError(new ErrorRecord(exception, "CounterApiError", ErrorCategory.InvalidResult, null));
            }
            else
            {
                base.WriteError(new ErrorRecord(exception, "CounterApiError", ErrorCategory.InvalidResult, null));
            }
        }

        private void ResolvePath()
        {
            try
            {
                Collection<PathInfo> resolvedPSPathFromPSPath = null;
                resolvedPSPathFromPSPath = base.SessionState.Path.GetResolvedPSPathFromPSPath(this._path);
                if (resolvedPSPathFromPSPath.Count > 1)
                {
                    Exception exception = new Exception(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("ExportDestPathAmbiguous"), new object[] { this._path }));
                    base.ThrowTerminatingError(new ErrorRecord(exception, "ExportDestPathAmbiguous", ErrorCategory.InvalidArgument, null));
                }
                foreach (PathInfo info in resolvedPSPathFromPSPath)
                {
                    this._resolvedPath = info.ProviderPath;
                }
            }
            catch (ItemNotFoundException exception2)
            {
                this._resolvedPath = exception2.ItemName;
            }
        }

        protected override void StopProcessing()
        {
            this._stopping = true;
            this._pdhHelper.Dispose();
        }

        private void ValidateFormat()
        {
            switch (this._format.ToLower(CultureInfo.InvariantCulture))
            {
                case "blg":
                    this._outputFormat = PdhLogFileType.PDH_LOG_TYPE_BINARY;
                    return;

                case "csv":
                    this._outputFormat = PdhLogFileType.PDH_LOG_TYPE_CSV;
                    return;

                case "tsv":
                    this._outputFormat = PdhLogFileType.PDH_LOG_TYPE_TSV;
                    return;
            }
            Exception exception = new Exception(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("CounterInvalidFormat"), new object[] { this._format }));
            base.ThrowTerminatingError(new ErrorRecord(exception, "CounterInvalidFormat", ErrorCategory.InvalidArgument, null));
        }

        [Parameter(HelpMessageBaseName="GetEventResources")]
        public SwitchParameter Circular
        {
            get
            {
                return this._circular;
            }
            set
            {
                this._circular = value;
            }
        }

        [Parameter(Mandatory=false, ValueFromPipeline=false, ValueFromPipelineByPropertyName=false, HelpMessageBaseName="GetEventResources"), ValidateNotNull]
        public string FileFormat
        {
            get
            {
                return this._format;
            }
            set
            {
                this._format = value;
            }
        }

        [Parameter(HelpMessageBaseName="GetEventResources")]
        public SwitchParameter Force
        {
            get
            {
                return this._force;
            }
            set
            {
                this._force = value;
            }
        }

        [Parameter(Mandatory=true, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, HelpMessageBaseName="GetEventResources"), SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Scope="member", Target="Microsoft.PowerShell.Commands.ExportCounterCommand.InputObject", Justification="A PerformanceCounterSampleSet[] is required here because Powershell supports arrays natively.")]
        public PerformanceCounterSampleSet[] InputObject
        {
            get
            {
                return this._counterSampleSets;
            }
            set
            {
                this._counterSampleSets = value;
            }
        }

        [Parameter(HelpMessageBaseName="GetEventResources")]
        public long MaxSize
        {
            get
            {
                return this._maxSize;
            }
            set
            {
                this._maxSize = value;
            }
        }

        [Alias(new string[] { "PSPath" }), Parameter(Mandatory=true, Position=0, ValueFromPipelineByPropertyName=true, HelpMessageBaseName="GetEventResources")]
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

