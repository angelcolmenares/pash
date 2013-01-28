namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Diagnostics.Common;
    using Microsoft.PowerShell.Commands.GetCounter;
    using Microsoft.Powershell.Commands.GetCounter.PdhNative;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Reflection;
    using System.Resources;

    [Cmdlet("Import", "Counter", DefaultParameterSetName="GetCounterSet", HelpUri="http://go.microsoft.com/fwlink/?LinkID=138338")]
    public sealed class ImportCounterCommand : PSCmdlet
    {
        private List<string> _accumulatedFileNames = new List<string>();
        private string[] _counter = new string[0];
        private DateTime _endTime = DateTime.MaxValue;
        private string[] _listSet = new string[0];
        private long _maxSamples = -1L;
        private string[] _path;
        private PdhHelper _pdhHelper;
        private StringCollection _resolvedPaths = new StringCollection();
        private ResourceManager _resourceMgr;
        private DateTime _startTime = DateTime.MinValue;
        private bool _stopping;
        private SwitchParameter _summary;
        private const long KEEP_ON_SAMPLING = -1L;

        private void AccumulatePipelineFileNames()
        {
            this._accumulatedFileNames.AddRange(this._path);
        }

        protected override void BeginProcessing()
        {
            this._resourceMgr = new ResourceManager("GetEventResources", Assembly.GetExecutingAssembly());
            this._pdhHelper = new PdhHelper(Environment.OSVersion.Version.Major < 6);
        }

        protected override void EndProcessing()
        {
            if (this.ResolveFilePaths())
            {
                this.ValidateFilePaths();
                string parameterSetName = base.ParameterSetName;
                if (parameterSetName != null)
                {
                    if (!(parameterSetName == "ListSetSet"))
                    {
                        if (parameterSetName == "GetCounterSet")
                        {
                            this.ProcessGetCounter();
                        }
                        else if (parameterSetName == "SummarySet")
                        {
                            this.ProcessSummary();
                        }
                    }
                    else
                    {
                        this.ProcessListSet();
                    }
                }
                this._pdhHelper.Dispose();
            }
        }

        private void ProcessGetCounter()
        {
            if (((this._startTime != DateTime.MinValue) || (this._endTime != DateTime.MaxValue)) && (this._startTime >= this._endTime))
            {
                Exception exception = new Exception(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("CounterInvalidDateRange"), new object[0]));
                base.ThrowTerminatingError(new ErrorRecord(exception, "CounterInvalidDateRange", ErrorCategory.InvalidArgument, null));
            }
            else
            {
                long res = this._pdhHelper.ConnectToDataSource(this._resolvedPaths);
                if (res != 0)
                {
                    this.ReportPdhError(res, true);
                }
                else
                {
                    StringCollection validPaths = new StringCollection();
                    if (this._counter.Length > 0)
                    {
                        foreach (string str2 in this._counter)
                        {
                            StringCollection strings2;
                            res = this._pdhHelper.ExpandWildCardPath(str2, out strings2);
                            if (res != 0)
                            {
                                base.WriteDebug(str2);
                                this.ReportPdhError(res, false);
                            }
                            else
                            {
                                foreach (string str3 in strings2)
                                {
                                    if (!this._pdhHelper.IsPathValid(str3))
                                    {
                                        Exception exception2 = new Exception(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("CounterPathIsInvalid"), new object[] { str2 }));
                                        base.WriteError(new ErrorRecord(exception2, "CounterPathIsInvalid", ErrorCategory.InvalidResult, null));
                                    }
                                    else
                                    {
                                        validPaths.Add(str3);
                                    }
                                }
                            }
                        }
                        if (validPaths.Count == 0)
                        {
                            return;
                        }
                    }
                    else
                    {
                        res = this._pdhHelper.GetValidPathsFromFiles(ref validPaths);
                        if (res != 0)
                        {
                            this.ReportPdhError(res, false);
                        }
                    }
                    if (validPaths.Count == 0)
                    {
                        Exception exception3 = new Exception(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("CounterPathsInFilesInvalid"), new object[0]));
                        base.ThrowTerminatingError(new ErrorRecord(exception3, "CounterPathsInFilesInvalid", ErrorCategory.InvalidResult, null));
                    }
                    res = this._pdhHelper.OpenQuery();
                    if (res != 0)
                    {
                        this.ReportPdhError(res, false);
                    }
                    if ((this._startTime != DateTime.MinValue) || (this._endTime != DateTime.MaxValue))
                    {
                        res = this._pdhHelper.SetQueryTimeRange(this._startTime, this._endTime);
                        if (res != 0)
                        {
                            this.ReportPdhError(res, true);
                        }
                    }
                    res = this._pdhHelper.AddCounters(ref validPaths, true);
                    if (res != 0)
                    {
                        this.ReportPdhError(res, true);
                    }
                    long num2 = 0;
                    while (!this._stopping)
                    {
                        PerformanceCounterSampleSet set;
                        res = this._pdhHelper.ReadNextSet(out set, false);
                        if (res == 0xc0000bccL)
                        {
                            return;
                        }
                        if ((res != 0) && (res != 0xc0000bc6L))
                        {
                            this.ReportPdhError(res, false);
                        }
                        else
                        {
                            this.WriteSampleSetObject(set, num2 == 0);
                            num2++;
                            if ((this._maxSamples != -1L) && (num2 >= this._maxSamples))
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }

        private void ProcessListSet()
        {
            long res = this._pdhHelper.ConnectToDataSource(this._resolvedPaths);
            if (res != 0)
            {
                this.ReportPdhError(res, true);
            }
            else
            {
                StringCollection machineNames = new StringCollection();
                res = this._pdhHelper.EnumBlgFilesMachines(ref machineNames);
                if (res != 0)
                {
                    this.ReportPdhError(res, true);
                }
                else
                {
                    foreach (string str in machineNames)
                    {
                        StringCollection objectNames = new StringCollection();
                        if (this._pdhHelper.EnumObjects(str, ref objectNames) != 0)
                        {
                            break;
                        }
                        new StringCollection();
                        foreach (string str2 in this._listSet)
                        {
                            bool flag = false;
                            WildcardPattern pattern = new WildcardPattern(str2, WildcardOptions.IgnoreCase);
                            foreach (string str3 in objectNames)
                            {
                                if (pattern.IsMatch(str3))
                                {
                                    StringCollection counterNames = new StringCollection();
                                    StringCollection instanceNames = new StringCollection();
                                    res = this._pdhHelper.EnumObjectItems(str, str3, ref counterNames, ref instanceNames);
                                    if (res != 0)
                                    {
                                        this.ReportPdhError(res, false);
                                    }
                                    else
                                    {
                                        string[] strArray = new string[instanceNames.Count];
                                        int num2 = 0;
                                        foreach (string str4 in instanceNames)
                                        {
                                            strArray[num2++] = str4;
                                        }
                                        Dictionary<string, string[]> counterInstanceMapping = new Dictionary<string, string[]>();
                                        foreach (string str5 in counterNames)
                                        {
                                            counterInstanceMapping.Add(str5, strArray);
                                        }
                                        PerformanceCounterCategoryType unknown = PerformanceCounterCategoryType.Unknown;
                                        if (instanceNames.Count > 1)
                                        {
                                            unknown = PerformanceCounterCategoryType.MultiInstance;
                                        }
                                        else
                                        {
                                            unknown = PerformanceCounterCategoryType.SingleInstance;
                                        }
                                        string counterSetHelp = this._pdhHelper.GetCounterSetHelp(str, str3);
                                        CounterSet sendToPipeline = new CounterSet(str3, str, unknown, counterSetHelp, ref counterInstanceMapping);
                                        base.WriteObject(sendToPipeline);
                                        flag = true;
                                    }
                                }
                            }
                            if (!flag)
                            {
                                string format = this._resourceMgr.GetString("NoMatchingCounterSetsInFile");
                                Exception exception = new Exception(string.Format(CultureInfo.InvariantCulture, format, new object[] { CommonUtilities.StringArrayToString(this._resolvedPaths), str2 }));
                                base.WriteError(new ErrorRecord(exception, "NoMatchingCounterSetsInFile", ErrorCategory.ObjectNotFound, null));
                            }
                        }
                    }
                }
            }
        }

        protected override void ProcessRecord()
        {
            this.AccumulatePipelineFileNames();
        }

        private void ProcessSummary()
        {
            long res = this._pdhHelper.ConnectToDataSource(this._resolvedPaths);
            if (res != 0)
            {
                this.ReportPdhError(res, true);
            }
            else
            {
                CounterFileInfo info;
                res = this._pdhHelper.GetFilesSummary(out info);
                if (res != 0)
                {
                    this.ReportPdhError(res, true);
                }
                else
                {
                    base.WriteObject(info);
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

        private bool ResolveFilePaths()
        {
            new StringCollection();
            foreach (string str in this._accumulatedFileNames)
            {
                Collection<PathInfo> resolvedPSPathFromPSPath = null;
                try
                {
                    resolvedPSPathFromPSPath = base.SessionState.Path.GetResolvedPSPathFromPSPath(str);
                }
                catch (PSNotSupportedException exception)
                {
                    base.WriteError(new ErrorRecord(exception, "", ErrorCategory.ObjectNotFound, str));
                    continue;
                }
                catch (System.Management.Automation.DriveNotFoundException exception2)
                {
                    base.WriteError(new ErrorRecord(exception2, "", ErrorCategory.ObjectNotFound, str));
                    continue;
                }
                catch (ProviderNotFoundException exception3)
                {
                    base.WriteError(new ErrorRecord(exception3, "", ErrorCategory.ObjectNotFound, str));
                    continue;
                }
                catch (ItemNotFoundException exception4)
                {
                    base.WriteError(new ErrorRecord(exception4, "", ErrorCategory.ObjectNotFound, str));
                    continue;
                }
                catch (Exception exception5)
                {
                    base.WriteError(new ErrorRecord(exception5, "", ErrorCategory.ObjectNotFound, str));
                    continue;
                }
                foreach (PathInfo info in resolvedPSPathFromPSPath)
                {
                    if (info.Provider.Name != "FileSystem")
                    {
                        string format = this._resourceMgr.GetString("NotAFileSystemPath");
                        Exception exception6 = new Exception(string.Format(CultureInfo.InvariantCulture, format, new object[] { str }));
                        base.WriteError(new ErrorRecord(exception6, "NotAFileSystemPath", ErrorCategory.InvalidArgument, str));
                    }
                    else
                    {
                        this._resolvedPaths.Add(info.ProviderPath.ToLower(CultureInfo.InvariantCulture));
                    }
                }
            }
            return (this._resolvedPaths.Count > 0);
        }

        protected override void StopProcessing()
        {
            this._stopping = true;
            this._pdhHelper.Dispose();
        }

        private void ValidateFilePaths()
        {
            string extension = System.IO.Path.GetExtension(this._resolvedPaths[0]);
            foreach (string str2 in this._resolvedPaths)
            {
                base.WriteVerbose(str2);
                string str3 = System.IO.Path.GetExtension(str2);
                if ((!str3.Equals(".blg", StringComparison.CurrentCultureIgnoreCase) && !str3.Equals(".csv", StringComparison.CurrentCultureIgnoreCase)) && !str3.Equals(".tsv", StringComparison.CurrentCultureIgnoreCase))
                {
                    Exception exception = new Exception(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("CounterNotALogFile"), new object[] { str2 }));
                    base.ThrowTerminatingError(new ErrorRecord(exception, "CounterNotALogFile", ErrorCategory.InvalidResult, null));
                    return;
                }
                if (!str3.Equals(extension, StringComparison.CurrentCultureIgnoreCase))
                {
                    Exception exception2 = new Exception(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("CounterNoMixedLogTypes"), new object[] { str2 }));
                    base.ThrowTerminatingError(new ErrorRecord(exception2, "CounterNoMixedLogTypes", ErrorCategory.InvalidResult, null));
                    return;
                }
            }
            if (!extension.Equals(".blg", StringComparison.CurrentCultureIgnoreCase))
            {
                if (this._resolvedPaths.Count > 1)
                {
                    Exception exception4 = new Exception(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("Counter1FileLimit"), new object[0]));
                    base.ThrowTerminatingError(new ErrorRecord(exception4, "Counter1FileLimit", ErrorCategory.InvalidResult, null));
                }
            }
            else if (this._resolvedPaths.Count > 0x20)
            {
                Exception exception3 = new Exception(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("Counter32FileLimit"), new object[0]));
                base.ThrowTerminatingError(new ErrorRecord(exception3, "Counter32FileLimit", ErrorCategory.InvalidResult, null));
            }
        }

        private void WriteSampleSetObject(PerformanceCounterSampleSet set, bool firstSet)
        {
            if (!firstSet)
            {
                foreach (PerformanceCounterSample sample in set.CounterSamples)
                {
                    if (sample.Status != 0)
                    {
                        Exception exception = new Exception(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("CounterSampleDataInvalid"), new object[0]));
                        base.WriteError(new ErrorRecord(exception, "CounterApiError", ErrorCategory.InvalidResult, null));
                        break;
                    }
                }
            }
            base.WriteObject(set);
        }

        [AllowEmptyCollection, Parameter(Mandatory=false, ParameterSetName="GetCounterSet", ValueFromPipeline=false, HelpMessageBaseName="GetEventResources"), SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Scope="member", Target="Microsoft.PowerShell.Commands.GetCounterCommand.ListSet", Justification="A string[] is required here because that is the type Powershell supports")]
        public string[] Counter
        {
            get
            {
                return this._counter;
            }
            set
            {
                this._counter = value;
            }
        }

        [Parameter(ValueFromPipeline=false, ValueFromPipelineByPropertyName=false, ParameterSetName="GetCounterSet", HelpMessageBaseName="GetEventResources")]
        public DateTime EndTime
        {
            get
            {
                return this._endTime;
            }
            set
            {
                this._endTime = value;
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="ListSetSet", ValueFromPipeline=false, ValueFromPipelineByPropertyName=false, HelpMessageBaseName="GetEventResources"), AllowEmptyCollection, SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Scope="member", Target="Microsoft.PowerShell.Commands.GetCounterCommand.ListSet", Justification="A string[] is required here because that is the type Powershell supports")]
        public string[] ListSet
        {
            get
            {
                return this._listSet;
            }
            set
            {
                this._listSet = value;
            }
        }

        [ValidateRange(1L, 0x7fffffffffffffffL), Parameter(ParameterSetName="GetCounterSet", ValueFromPipeline=false, ValueFromPipelineByPropertyName=false, HelpMessageBaseName="GetEventResources")]
        public long MaxSamples
        {
            get
            {
                return this._maxSamples;
            }
            set
            {
                this._maxSamples = value;
            }
        }

        [Alias(new string[] { "PSPath" }), SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Scope="member", Target="Microsoft.PowerShell.Commands.GetCounterCommand.ListSet", Justification="A string[] is required here because that is the type Powershell supports"), Parameter(Position=0, Mandatory=true, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, HelpMessageBaseName="GetEventResources")]
        public string[] Path
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

        [Parameter(ValueFromPipeline=false, ValueFromPipelineByPropertyName=false, ParameterSetName="GetCounterSet", HelpMessageBaseName="GetEventResources")]
        public DateTime StartTime
        {
            get
            {
                return this._startTime;
            }
            set
            {
                this._startTime = value;
            }
        }

        [Parameter(ParameterSetName="SummarySet")]
        public SwitchParameter Summary
        {
            get
            {
                return this._summary;
            }
            set
            {
                this._summary = value;
            }
        }
    }
}

