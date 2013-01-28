namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Diagnostics.Common;
    using Microsoft.PowerShell.Commands.GetCounter;
    using Microsoft.Powershell.Commands.GetCounter.PdhNative;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Management.Automation;
    using System.Reflection;
    using System.Resources;
    using System.Threading;

    [Cmdlet("Get", "Counter", DefaultParameterSetName="GetCounterSet", HelpUri="http://go.microsoft.com/fwlink/?LinkID=138335")]
    public sealed class GetCounterCommand : PSCmdlet
    {
        private List<string> _accumulatedCounters = new List<string>();
        private EventWaitHandle _cancelEventArrived = new EventWaitHandle(false, EventResetMode.ManualReset);
        private string[] _computerName = new string[0];
        private bool _continuous;
        private string[] _counter = new string[] { @"\network interface(*)\bytes total/sec", @"\processor(_total)\% processor time", @"\memory\% committed bytes in use", @"\memory\cache faults/sec", @"\physicaldisk(_total)\% disk time", @"\physicaldisk(_total)\current disk queue length" };
        private bool _defaultCounters = true;
        private string[] _listSet = new string[] { "*" };
        private long _maxSamples = 1L;
        private bool _maxSamplesSpecified;
        private PdhHelper _pdhHelper;
        private ResourceManager _resourceMgr;
        private int _sampleInterval = 1;
        private const long KEEP_ON_SAMPLING = -1L;

        private void AccumulatePipelineCounters()
        {
            this._accumulatedCounters.AddRange(this._counter);
        }

        protected override void BeginProcessing()
        {
            this._resourceMgr = new ResourceManager("GetEventResources", Assembly.GetExecutingAssembly());
            this._pdhHelper = new PdhHelper(Environment.OSVersion.Version.Major < 6);
            long res = this._pdhHelper.ConnectToDataSource();
            if (res != 0)
            {
                this.ReportPdhError(res, true);
            }
            else if (this.Continuous.IsPresent && this._maxSamplesSpecified)
            {
                Exception exception = new Exception(string.Format(CultureInfo.CurrentCulture, this._resourceMgr.GetString("CounterContinuousOrMaxSamples"), new object[0]));
                base.ThrowTerminatingError(new ErrorRecord(exception, "CounterContinuousOrMaxSamples", ErrorCategory.InvalidArgument, null));
            }
        }

        private List<string> CombineMachinesAndCounterPaths()
        {
            List<string> list = new List<string>();
            if (this._computerName.Length == 0)
            {
                list.AddRange(this._accumulatedCounters);
                return list;
            }
            foreach (string str in this._accumulatedCounters)
            {
                if (str.StartsWith(@"\\", StringComparison.OrdinalIgnoreCase))
                {
                    list.Add(str);
                }
                else
                {
                    foreach (string str2 in this._computerName)
                    {
                        if (str2.StartsWith(@"\\", StringComparison.OrdinalIgnoreCase))
                        {
                            list.Add(str2 + @"\" + str);
                        }
                        else
                        {
                            list.Add(@"\\" + str2 + @"\" + str);
                        }
                    }
                }
            }
            return list;
        }

        protected override void EndProcessing()
        {
            if (base.ParameterSetName == "GetCounterSet")
            {
                this.ProcessGetCounter();
            }
            this._pdhHelper.Dispose();
        }

        private void ProcessGetCounter()
        {
            List<string> list = this.CombineMachinesAndCounterPaths();
            long res = 0;
            StringCollection validPaths = new StringCollection();
            foreach (string str in list)
            {
                StringCollection strings2;
                string localizedPath = str;
                if (this._defaultCounters)
                {
                    res = this._pdhHelper.TranslateLocalCounterPath(str, out localizedPath);
                    if (res != 0)
                    {
                        Exception exception = new Exception(string.Format(CultureInfo.CurrentCulture, this._resourceMgr.GetString("CounterPathTranslationFailed"), new object[] { res }));
                        base.WriteError(new ErrorRecord(exception, "CounterPathTranslationFailed", ErrorCategory.InvalidResult, null));
                        localizedPath = str;
                    }
                }
                res = this._pdhHelper.ExpandWildCardPath(localizedPath, out strings2);
                if (res != 0)
                {
                    base.WriteDebug("Could not expand path " + localizedPath);
                    this.ReportPdhError(res, false);
                }
                else
                {
                    foreach (string str4 in strings2)
                    {
                        if (!this._pdhHelper.IsPathValid(str4))
                        {
                            Exception exception2 = new Exception(string.Format(CultureInfo.CurrentCulture, this._resourceMgr.GetString("CounterPathIsInvalid"), new object[] { localizedPath }));
                            base.WriteError(new ErrorRecord(exception2, "CounterPathIsInvalid", ErrorCategory.InvalidResult, null));
                        }
                        else
                        {
                            validPaths.Add(str4);
                        }
                    }
                }
            }
            if (validPaths.Count != 0)
            {
                res = this._pdhHelper.OpenQuery();
                if (res != 0)
                {
                    this.ReportPdhError(res, false);
                }
                res = this._pdhHelper.AddCounters(ref validPaths, true);
                if (res != 0)
                {
                    this.ReportPdhError(res, true);
                }
                else
                {
                    bool bSkipReading = true;
                    long num2 = 0;
                    if (this.Continuous.IsPresent)
                    {
                        this._maxSamples = -1L;
                    }
                    do
                    {
                        PerformanceCounterSampleSet set;
                        res = this._pdhHelper.ReadNextSet(out set, bSkipReading);
                        switch (res)
                        {
                            case 0:
                                if (!bSkipReading)
                                {
                                    this.WriteSampleSetObject(set);
                                    num2++;
                                }
                                bSkipReading = false;
                                break;

                            case 0x800007d5L:
                            case 0xc0000bc6L:
                                this.ReportPdhError(res, false);
                                bSkipReading = true;
                                num2++;
                                break;

                            default:
                                this.ReportPdhError(res, true);
                                return;
                        }
                    }
                    while (((this._maxSamples == -1L) || (num2 < this._maxSamples)) && !this._cancelEventArrived.WaitOne((int) (this._sampleInterval * 0x3e8), true));
                }
            }
        }

        private void ProcessListSet()
        {
            if (this._computerName.Length == 0)
            {
                this.ProcessListSetPerMachine(null);
            }
            else
            {
                foreach (string str in this._computerName)
                {
                    this.ProcessListSetPerMachine(str);
                }
            }
        }

        private void ProcessListSetPerMachine(string machine)
        {
            StringCollection objectNames = new StringCollection();
            long res = this._pdhHelper.EnumObjects(machine, ref objectNames);
            if (res != 0)
            {
                Exception exception = new Exception(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("NoCounterSetsOnComputer"), new object[] { machine, res }));
                base.WriteError(new ErrorRecord(exception, "NoCounterSetsOnComputer", ErrorCategory.InvalidResult, machine));
            }
            else
            {
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
                            res = this._pdhHelper.EnumObjectItems(machine, str3, ref counterNames, ref instanceNames);
                            if (res == 0xc0000bdbL)
                            {
                                Exception exception2 = new Exception(string.Format(CultureInfo.InvariantCulture, this._resourceMgr.GetString("CounterSetEnumAccessDenied"), new object[] { str3 }));
                                base.WriteError(new ErrorRecord(exception2, "CounterSetEnumAccessDenied", ErrorCategory.InvalidResult, null));
                            }
                            else if (res != 0)
                            {
                                this.ReportPdhError(res, false);
                            }
                            else
                            {
                                string[] strArray = new string[instanceNames.Count];
                                int num2 = 0;
                                foreach (string str5 in instanceNames)
                                {
                                    strArray[num2++] = str5;
                                }
                                if ((strArray.Length == 1) && (strArray[0].Length == 0))
                                {
                                    strArray[0] = "*";
                                }
                                Dictionary<string, string[]> counterInstanceMapping = new Dictionary<string, string[]>();
                                foreach (string str6 in counterNames)
                                {
                                    if (!counterInstanceMapping.ContainsKey(str6))
                                    {
                                        counterInstanceMapping.Add(str6, strArray);
                                    }
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
                                string counterSetHelp = this._pdhHelper.GetCounterSetHelp(machine, str3);
                                CounterSet sendToPipeline = new CounterSet(str3, machine, unknown, counterSetHelp, ref counterInstanceMapping);
                                base.WriteObject(sendToPipeline);
                                flag = true;
                            }
                        }
                    }
                    if (!flag)
                    {
                        string format = this._resourceMgr.GetString("NoMatchingCounterSetsFound");
                        Exception exception3 = new Exception(string.Format(CultureInfo.InvariantCulture, format, new object[] { (machine == null) ? "localhost" : machine, str2 }));
                        base.WriteError(new ErrorRecord(exception3, "NoMatchingCounterSetsFound", ErrorCategory.ObjectNotFound, null));
                    }
                }
            }
        }

        protected override void ProcessRecord()
        {
            try
            {
                string parameterSetName = base.ParameterSetName;
                if (parameterSetName != null)
                {
                    if (!(parameterSetName == "ListSetSet"))
                    {
                        if (parameterSetName == "GetCounterSet")
                        {
                            goto Label_002E;
                        }
                    }
                    else
                    {
                        this.ProcessListSet();
                    }
                }
                return;
            Label_002E:
                this.AccumulatePipelineCounters();
            }
            catch (Exception exception)
            {
                base.ThrowTerminatingError(new ErrorRecord(exception, "CounterApiError", ErrorCategory.InvalidResult, null));
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

        protected override void StopProcessing()
        {
            this._cancelEventArrived.Set();
            this._pdhHelper.Dispose();
        }

        private void WriteSampleSetObject(PerformanceCounterSampleSet set)
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
            base.WriteObject(set);
        }

        [Parameter(ValueFromPipeline=false, ValueFromPipelineByPropertyName=false, HelpMessageBaseName="GetEventResources", HelpMessageResourceId="ComputerNameParamHelp"), ValidateNotNull, AllowEmptyCollection, Alias(new string[] { "Cn" }), SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Scope="member", Target="Microsoft.PowerShell.Commands.GetCounterCommand.ComputerName", Justification="A string[] is required here because that is the type Powershell supports")]
        public string[] ComputerName
        {
            get
            {
                return this._computerName;
            }
            set
            {
                this._computerName = value;
            }
        }

        [Parameter(ParameterSetName="GetCounterSet")]
        public SwitchParameter Continuous
        {
            get
            {
                return this._continuous;
            }
            set
            {
                this._continuous = (bool) value;
            }
        }

        [Parameter(Position=0, ParameterSetName="GetCounterSet", ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, HelpMessageBaseName="GetEventResources"), SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Scope="member", Target="Microsoft.PowerShell.Commands.GetCounterCommand.ListSet", Justification="A string[] is required here because that is the type Powershell supports"), AllowEmptyCollection]
        public string[] Counter
        {
            get
            {
                return this._counter;
            }
            set
            {
                this._counter = value;
                this._defaultCounters = false;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Scope="member", Target="Microsoft.PowerShell.Commands.GetCounterCommand.ListSet", Justification="A string[] is required here because that is the type Powershell supports"), Parameter(Position=0, Mandatory=true, ParameterSetName="ListSetSet", ValueFromPipeline=true, ValueFromPipelineByPropertyName=false, HelpMessageBaseName="GetEventResources"), AllowEmptyCollection]
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
                this._maxSamplesSpecified = true;
            }
        }

        [ValidateRange(1, 0x7fffffff), Parameter(ParameterSetName="GetCounterSet", ValueFromPipeline=false, ValueFromPipelineByPropertyName=false, HelpMessageBaseName="GetEventResources")]
        public int SampleInterval
        {
            get
            {
                return this._sampleInterval;
            }
            set
            {
                this._sampleInterval = value;
            }
        }
    }
}

