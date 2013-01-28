namespace Microsoft.Powershell.Commands.GetCounter.PdhNative
{
    using Microsoft.PowerShell.Commands.GetCounter;
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    internal class PdhHelper : IDisposable
    {
        private Dictionary<string, CounterHandleNInstance> m_ConsumerPathToHandleAndInstanceMap = new Dictionary<string, CounterHandleNInstance>();
        private bool m_firstReading = true;
        private PdhSafeDataSourceHandle m_hDataSource;
        private PdhSafeLogHandle m_hOutputLog;
        private PdhSafeQueryHandle m_hQuery;
        private bool m_isPreVista;
        private Dictionary<string, CounterHandleNInstance> m_ReloggerPathToHandleAndInstanceMap = new Dictionary<string, CounterHandleNInstance>();

        public PdhHelper(bool isPreVista)
        {
            this.m_isPreVista = isPreVista;
        }

        public long AddCounters(ref StringCollection validPaths, bool bFlushOldCounters)
        {
            if (bFlushOldCounters)
            {
                this.m_ConsumerPathToHandleAndInstanceMap.Clear();
            }
            bool flag = false;
            long num = 0;
            foreach (string str in validPaths)
            {
                IntPtr ptr;
                num = PdhAddCounter(this.m_hQuery, str, IntPtr.Zero, out ptr);
                if (num == 0)
                {
                    CounterHandleNInstance instance = new CounterHandleNInstance {
                        hCounter = ptr,
                        InstanceName = null
                    };
                    PDH_COUNTER_PATH_ELEMENTS pCounterPathElements = new PDH_COUNTER_PATH_ELEMENTS();
                    num = this.ParsePath(str, ref pCounterPathElements);
                    if ((num == 0) && (pCounterPathElements.InstanceName != null))
                    {
                        instance.InstanceName = pCounterPathElements.InstanceName.ToLower(CultureInfo.InvariantCulture);
                    }
                    if (!this.m_ConsumerPathToHandleAndInstanceMap.ContainsKey(str.ToLower(CultureInfo.InvariantCulture)))
                    {
                        this.m_ConsumerPathToHandleAndInstanceMap.Add(str.ToLower(CultureInfo.InvariantCulture), instance);
                    }
                    flag = true;
                }
            }
            if (!flag)
            {
                return num;
            }
            return 0;
        }

        public long AddRelogCounters(PerformanceCounterSampleSet sampleSet)
        {
            long num = 0;
            Dictionary<string, List<PerformanceCounterSample>> dictionary = new Dictionary<string, List<PerformanceCounterSample>>();
            foreach (PerformanceCounterSample sample in sampleSet.CounterSamples)
            {
                PDH_COUNTER_PATH_ELEMENTS pCounterPathElements = new PDH_COUNTER_PATH_ELEMENTS();
                num = this.ParsePath(sample.Path, ref pCounterPathElements);
                if (num == 0)
                {
                    List<PerformanceCounterSample> list;
                    string str = pCounterPathElements.MachineName.ToLower(CultureInfo.InvariantCulture);
                    string str2 = pCounterPathElements.ObjectName.ToLower(CultureInfo.InvariantCulture);
                    string str3 = pCounterPathElements.CounterName.ToLower(CultureInfo.InvariantCulture);
                    string key = @"\\" + str + @"\" + str2 + @"\" + str3;
                    if (dictionary.TryGetValue(key, out list))
                    {
                        dictionary[key].Add(sample);
                    }
                    else
                    {
                        List<PerformanceCounterSample> list2 = new List<PerformanceCounterSample> {
                            sample
                        };
                        dictionary.Add(key, list2);
                    }
                }
            }
            foreach (string str5 in dictionary.Keys)
            {
                IntPtr ptr;
                string path = dictionary[str5][0].Path;
                if (dictionary[str5].Count > 1)
                {
                    num = this.MakeAllInstancePath(dictionary[str5][0].Path, out path);
                    if (num != 0)
                    {
                        continue;
                    }
                }
                num = PdhAddRelogCounter(this.m_hQuery, path, (long) dictionary[str5][0].CounterType, dictionary[str5][0].DefaultScale, dictionary[str5][0].TimeBase, out ptr);
                if (num == 0)
                {
                    foreach (PerformanceCounterSample sample2 in dictionary[str5])
                    {
                        PDH_COUNTER_PATH_ELEMENTS pdh_counter_path_elements2 = new PDH_COUNTER_PATH_ELEMENTS();
                        num = this.ParsePath(sample2.Path, ref pdh_counter_path_elements2);
                        if (num == 0)
                        {
                            CounterHandleNInstance instance = new CounterHandleNInstance {
                                hCounter = ptr
                            };
                            if (pdh_counter_path_elements2.InstanceName != null)
                            {
                                instance.InstanceName = pdh_counter_path_elements2.InstanceName.ToLower(CultureInfo.InvariantCulture);
                            }
                            if (!this.m_ReloggerPathToHandleAndInstanceMap.ContainsKey(sample2.Path.ToLower(CultureInfo.InvariantCulture)))
                            {
                                this.m_ReloggerPathToHandleAndInstanceMap.Add(sample2.Path.ToLower(CultureInfo.InvariantCulture), instance);
                            }
                        }
                    }
                }
            }
            if (this.m_ReloggerPathToHandleAndInstanceMap.Keys.Count <= 0)
            {
                return num;
            }
            return 0;
        }

        public long AddRelogCountersPreservingPaths(PerformanceCounterSampleSet sampleSet)
        {
            long num = 0;
            foreach (PerformanceCounterSample sample in sampleSet.CounterSamples)
            {
                PDH_COUNTER_PATH_ELEMENTS pCounterPathElements = new PDH_COUNTER_PATH_ELEMENTS();
                num = this.ParsePath(sample.Path, ref pCounterPathElements);
                if (num == 0)
                {
                    IntPtr ptr;
                    num = PdhAddRelogCounter(this.m_hQuery, sample.Path, (long) sample.CounterType, sample.DefaultScale, sample.TimeBase, out ptr);
                    if (num == 0)
                    {
                        CounterHandleNInstance instance = new CounterHandleNInstance {
                            hCounter = ptr
                        };
                        if (pCounterPathElements.InstanceName != null)
                        {
                            instance.InstanceName = pCounterPathElements.InstanceName.ToLower(CultureInfo.InvariantCulture);
                        }
                        if (!this.m_ReloggerPathToHandleAndInstanceMap.ContainsKey(sample.Path.ToLower(CultureInfo.InvariantCulture)))
                        {
                            this.m_ReloggerPathToHandleAndInstanceMap.Add(sample.Path.ToLower(CultureInfo.InvariantCulture), instance);
                        }
                    }
                }
            }
            if (this.m_ReloggerPathToHandleAndInstanceMap.Keys.Count <= 0)
            {
                return num;
            }
            return 0;
        }

        public long ConnectToDataSource()
        {
            if ((this.m_hDataSource != null) && !this.m_hDataSource.IsInvalid)
            {
                this.m_hDataSource.Dispose();
            }
            long num = PdhBindInputDataSource(out this.m_hDataSource, null);
            if (num != 0)
            {
                return num;
            }
            return 0;
        }

        public long ConnectToDataSource(StringCollection blgFileNames)
        {
            if (blgFileNames.Count == 1)
            {
                return this.ConnectToDataSource(blgFileNames[0]);
            }
            string dataSourceName = "";
            foreach (string str2 in blgFileNames)
            {
                dataSourceName = dataSourceName + str2 + '\0';
            }
            dataSourceName = dataSourceName + '\0';
            return this.ConnectToDataSource(dataSourceName);
        }

        public long ConnectToDataSource(string dataSourceName)
        {
            if ((this.m_hDataSource != null) && !this.m_hDataSource.IsInvalid)
            {
                this.m_hDataSource.Dispose();
            }
            return PdhBindInputDataSource(out this.m_hDataSource, dataSourceName);
        }

        public void Dispose()
        {
            if ((this.m_hDataSource != null) && !this.m_hDataSource.IsInvalid)
            {
                this.m_hDataSource.Dispose();
            }
            if ((this.m_hOutputLog != null) && !this.m_hOutputLog.IsInvalid)
            {
                this.m_hOutputLog.Dispose();
            }
            if ((this.m_hQuery != null) && !this.m_hQuery.IsInvalid)
            {
                this.m_hQuery.Dispose();
            }
            GC.SuppressFinalize(this);
        }

        public long EnumBlgFilesMachines(ref StringCollection machineNames)
        {
            IntPtr pcchBufferLength = new IntPtr(0);
            long num = PdhEnumMachinesH(this.m_hDataSource, IntPtr.Zero, ref pcchBufferLength);
            if (num == 0x800007d2L)
            {
                IntPtr mszMachineNameList = Marshal.AllocHGlobal((int) (pcchBufferLength.ToInt32() * 2));
                try
                {
                    num = PdhEnumMachinesH(this.m_hDataSource, mszMachineNameList, ref pcchBufferLength);
                    if (num == 0)
                    {
                        this.ReadPdhMultiString(ref mszMachineNameList, pcchBufferLength.ToInt32(), ref machineNames);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(mszMachineNameList);
                }
            }
            return num;
        }

        public long EnumObjectItems(string machineName, string objectName, ref StringCollection counterNames, ref StringCollection instanceNames)
        {
            IntPtr pcchCounterListLength = new IntPtr(0);
            IntPtr pcchInstanceListLength = new IntPtr(0);
            long num = PdhEnumObjectItemsH(this.m_hDataSource, machineName, objectName, IntPtr.Zero, ref pcchCounterListLength, IntPtr.Zero, ref pcchInstanceListLength, 400, 0);
            switch (num)
            {
                case 0x800007d1L:
                    instanceNames.Clear();
                    return 0;

                case 0xc0000bb8L:
                    counterNames.Clear();
                    return 0;

                case 0x800007d2L:
                {
                    int num2 = pcchCounterListLength.ToInt32();
                    IntPtr mszCounterList = (num2 > 0) ? Marshal.AllocHGlobal((int) (num2 * 2)) : IntPtr.Zero;
                    if (num2 < 0)
                    {
                        pcchCounterListLength = new IntPtr(0);
                    }
                    num2 = pcchInstanceListLength.ToInt32();
                    IntPtr mszInstanceList = (num2 > 0) ? Marshal.AllocHGlobal((int) (num2 * 2)) : IntPtr.Zero;
                    if (num2 < 0)
                    {
                        pcchInstanceListLength = new IntPtr(0);
                    }
                    try
                    {
                        num = PdhEnumObjectItemsH(this.m_hDataSource, machineName, objectName, mszCounterList, ref pcchCounterListLength, mszInstanceList, ref pcchInstanceListLength, 400, 0);
                        if (num == 0)
                        {
                            this.ReadPdhMultiString(ref mszCounterList, pcchCounterListLength.ToInt32(), ref counterNames);
                            if (mszInstanceList != IntPtr.Zero)
                            {
                                this.ReadPdhMultiString(ref mszInstanceList, pcchInstanceListLength.ToInt32(), ref instanceNames);
                            }
                        }
                    }
                    finally
                    {
                        if (mszCounterList != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(mszCounterList);
                        }
                        if (mszInstanceList != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(mszInstanceList);
                        }
                    }
                    break;
                }
            }
            return num;
        }

        public long EnumObjects(string machineName, ref StringCollection objectNames)
        {
            IntPtr pcchBufferLength = new IntPtr(0);
            long num = PdhEnumObjectsH(this.m_hDataSource, machineName, IntPtr.Zero, ref pcchBufferLength, 400, false);
            if (num == 0x800007d2L)
            {
                IntPtr mszObjectList = Marshal.AllocHGlobal((int) (pcchBufferLength.ToInt32() * 2));
                try
                {
                    num = PdhEnumObjectsH(this.m_hDataSource, machineName, mszObjectList, ref pcchBufferLength, 400, false);
                    if (num == 0)
                    {
                        this.ReadPdhMultiString(ref mszObjectList, pcchBufferLength.ToInt32(), ref objectNames);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(mszObjectList);
                }
            }
            return num;
        }

        public long ExpandWildCardPath(string path, out StringCollection expandedPaths)
        {
            expandedPaths = new StringCollection();
            IntPtr pcchPathListLength = new IntPtr(0);
            long num = PdhExpandWildCardPathH(this.m_hDataSource, path, IntPtr.Zero, ref pcchPathListLength, 4);
            if (num == 0x800007d2L)
            {
                IntPtr mszExpandedPathList = Marshal.AllocHGlobal((int) (pcchPathListLength.ToInt32() * 2));
                try
                {
                    num = PdhExpandWildCardPathH(this.m_hDataSource, path, mszExpandedPathList, ref pcchPathListLength, 4);
                    if (num == 0)
                    {
                        this.ReadPdhMultiString(ref mszExpandedPathList, pcchPathListLength.ToInt32(), ref expandedPaths);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(mszExpandedPathList);
                }
            }
            return num;
        }

        private long GetCounterInfoPlus(IntPtr hCounter, out long counterType, out long defaultScale, out ulong timeBase)
        {
            long num = 0;
            counterType = 0;
            defaultScale = 0;
            timeBase = 0L;
            IntPtr pdwBufferSize = new IntPtr(0);
            num = PdhGetCounterInfo(hCounter, false, ref pdwBufferSize, IntPtr.Zero);
            if (num == 0x800007d2L)
            {
                IntPtr lpBuffer = Marshal.AllocHGlobal(pdwBufferSize.ToInt32());
                try
                {
                    if ((PdhGetCounterInfo(hCounter, false, ref pdwBufferSize, lpBuffer) == 0) && (lpBuffer != IntPtr.Zero))
                    {
                        counterType = (long) Marshal.ReadInt32(lpBuffer, 4);
                        defaultScale = (long) Marshal.ReadInt32(lpBuffer, 20);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(lpBuffer);
                }
                num = PdhGetCounterTimeBase(hCounter, out timeBase);
                if (num != 0)
                {
                    return num;
                }
            }
            return num;
        }

        public string GetCounterSetHelp(string szMachineName, string szObjectName)
        {
            if (this.m_isPreVista)
            {
                return string.Empty;
            }
            return Marshal.PtrToStringUni(PdhGetExplainText(szMachineName, szObjectName, null));
        }

        public long GetFilesSummary(out CounterFileInfo summary)
        {
            IntPtr pdwNumEntries = new IntPtr(0);
            PDH_TIME_INFO structure = new PDH_TIME_INFO();
            IntPtr pdwBufferSize = new IntPtr(Marshal.SizeOf(structure));
            long num = PdhGetDataSourceTimeRangeH(this.m_hDataSource, ref pdwNumEntries, ref structure, ref pdwBufferSize);
            if (num != 0)
            {
                summary = new CounterFileInfo();
                return num;
            }
            summary = new CounterFileInfo(new DateTime(DateTime.FromFileTimeUtc(structure.StartTime).Ticks, DateTimeKind.Local), new DateTime(DateTime.FromFileTimeUtc(structure.EndTime).Ticks, DateTimeKind.Local), structure.SampleCount);
            return num;
        }

        public long GetValidPaths(string machineName, string objectName, ref StringCollection counters, ref StringCollection instances, ref StringCollection validPaths)
        {
            PDH_COUNTER_PATH_ELEMENTS pathElts = new PDH_COUNTER_PATH_ELEMENTS {
                MachineName = machineName,
                ObjectName = objectName
            };
            foreach (string str in counters)
            {
                pathElts.CounterName = str;
                if (instances.Count == 0)
                {
                    string str2;
                    if (this.IsPathValid(ref pathElts, out str2))
                    {
                        validPaths.Add(str2);
                    }
                }
                else
                {
                    foreach (string str3 in instances)
                    {
                        string str4;
                        pathElts.InstanceName = str3;
                        pathElts.InstanceIndex = 0;
                        if (this.IsPathValid(ref pathElts, out str4))
                        {
                            validPaths.Add(str4);
                        }
                    }
                }
            }
            return 0;
        }

        public long GetValidPathsFromFiles(ref StringCollection validPaths)
        {
            StringCollection machineNames = new StringCollection();
            long num = this.EnumBlgFilesMachines(ref machineNames);
            if (num == 0)
            {
                foreach (string str in machineNames)
                {
                    StringCollection objectNames = new StringCollection();
                    num = this.EnumObjects(str, ref objectNames);
                    if (num != 0)
                    {
                        return num;
                    }
                    foreach (string str2 in objectNames)
                    {
                        StringCollection counterNames = new StringCollection();
                        StringCollection instanceNames = new StringCollection();
                        num = this.EnumObjectItems(str, str2, ref counterNames, ref instanceNames);
                        if (num != 0)
                        {
                            return num;
                        }
                        num = this.GetValidPaths(str, str2, ref counterNames, ref instanceNames, ref validPaths);
                        if (num != 0)
                        {
                            return num;
                        }
                    }
                }
            }
            return num;
        }

        public bool IsPathValid(string path)
        {
            if (!this.m_isPreVista)
            {
                return (PdhValidatePathEx(this.m_hDataSource, path) == 0);
            }
            return (PdhValidatePath(path) == 0);
        }

        private bool IsPathValid(ref PDH_COUNTER_PATH_ELEMENTS pathElts, out string outPath)
        {
            bool flag = false;
            outPath = "";
            IntPtr pcchBufferSize = new IntPtr(0);
            if (PdhMakeCounterPath(ref pathElts, IntPtr.Zero, ref pcchBufferSize, 0) != 0x800007d2L)
            {
                return false;
            }
            IntPtr szFullPathBuffer = Marshal.AllocHGlobal((int) (pcchBufferSize.ToInt32() * 2));
            try
            {
                if (PdhMakeCounterPath(ref pathElts, szFullPathBuffer, ref pcchBufferSize, 0) != 0)
                {
                    return flag;
                }
                outPath = Marshal.PtrToStringUni(szFullPathBuffer);
                if (!this.m_isPreVista)
                {
                    return (PdhValidatePathEx(this.m_hDataSource, outPath) == 0);
                }
                flag = PdhValidatePath(outPath) == 0;
            }
            finally
            {
                Marshal.FreeHGlobal(szFullPathBuffer);
            }
            return flag;
        }

        public long LookupPerfNameByIndex(string machineName, long index, out string locName)
        {
            int pcchNameBufferSize = 0x100;
            IntPtr szNameBuffer = Marshal.AllocHGlobal((int) (pcchNameBufferSize * 2));
            locName = "";
            long num2 = 0;
            try
            {
                num2 = PdhLookupPerfNameByIndex(machineName, index, szNameBuffer, ref pcchNameBufferSize);
                switch (num2)
                {
                    case 0x800007d2L:
                        Marshal.FreeHGlobal(szNameBuffer);
                        szNameBuffer = Marshal.AllocHGlobal((int) (pcchNameBufferSize * 2));
                        num2 = PdhLookupPerfNameByIndex(machineName, index, szNameBuffer, ref pcchNameBufferSize);
                        break;

                    case 0:
                        locName = Marshal.PtrToStringUni(szNameBuffer);
                        break;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(szNameBuffer);
            }
            return num2;
        }

        private long MakeAllInstancePath(string origPath, out string unifiedPath)
        {
            unifiedPath = origPath;
            PDH_COUNTER_PATH_ELEMENTS pCounterPathElements = new PDH_COUNTER_PATH_ELEMENTS();
            long num = this.ParsePath(origPath, ref pCounterPathElements);
            if (num != 0)
            {
                return num;
            }
            return this.MakePath(pCounterPathElements, out unifiedPath, true);
        }

        private long MakePath(PDH_COUNTER_PATH_ELEMENTS pathElts, out string outPath, bool bWildcardInstances)
        {
            outPath = "";
            IntPtr pcchBufferSize = new IntPtr(0);
            if (bWildcardInstances)
            {
                pathElts.InstanceIndex = 0;
                pathElts.InstanceName = "*";
                pathElts.ParentInstance = null;
            }
            long num = PdhMakeCounterPath(ref pathElts, IntPtr.Zero, ref pcchBufferSize, 0);
            if (num == 0x800007d2L)
            {
                IntPtr szFullPathBuffer = Marshal.AllocHGlobal((int) (pcchBufferSize.ToInt32() * 2));
                try
                {
                    num = PdhMakeCounterPath(ref pathElts, szFullPathBuffer, ref pcchBufferSize, 0);
                    if (num == 0)
                    {
                        outPath = Marshal.PtrToStringUni(szFullPathBuffer);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(szFullPathBuffer);
                }
            }
            return num;
        }

        public long OpenLogForWriting(string logName, PdhLogFileType logFileType, bool bOverwrite, long maxSize, bool bCircular, string caption)
        {
            int dwAccessFlags = 0x20000;
            dwAccessFlags |= bCircular ? 0x2000000 : 0;
            dwAccessFlags |= bOverwrite ? 2 : 1;
            return PdhOpenLog(logName, dwAccessFlags, ref logFileType, this.m_hQuery, maxSize, caption, out this.m_hOutputLog);
        }

        public long OpenQuery()
        {
            return PdhOpenQueryH(this.m_hDataSource, IntPtr.Zero, out this.m_hQuery);
        }

        private long ParsePath(string fullPath, ref PDH_COUNTER_PATH_ELEMENTS pCounterPathElements)
        {
            IntPtr pdwBufferSize = new IntPtr(0);
            long num = PdhParseCounterPath(fullPath, IntPtr.Zero, ref pdwBufferSize, 0);
            switch (num)
            {
                case 0x800007d2L:
                case 0:
                {
                    IntPtr ptr2 = Marshal.AllocHGlobal(pdwBufferSize.ToInt32());
                    try
                    {
                        num = PdhParseCounterPath(fullPath, ptr2, ref pdwBufferSize, 0);
                        if (num == 0)
                        {
                            pCounterPathElements = (PDH_COUNTER_PATH_ELEMENTS) Marshal.PtrToStructure(ptr2, typeof(PDH_COUNTER_PATH_ELEMENTS));
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(ptr2);
                    }
                    break;
                }
            }
            return num;
        }

        [DllImport("pdh.dll", CharSet=CharSet.Unicode)]
        private static extern long PdhAddCounter(PdhSafeQueryHandle queryHandle, string counterPath, IntPtr userData, out IntPtr counterHandle);
        [DllImport("pdh.dll", CharSet=CharSet.Unicode)]
        private static extern long PdhAddRelogCounter(PdhSafeQueryHandle queryHandle, string counterPath, long counterType, long counterDefaultScale, ulong timeBase, out IntPtr counterHandle);
        [DllImport("pdh.dll", CharSet=CharSet.Unicode)]
        private static extern long PdhBindInputDataSource(out PdhSafeDataSourceHandle phDataSource, string szLogFileNameList);
        [DllImport("pdh.dll")]
        internal static extern long PdhCloseLog(IntPtr logHandle, long dwFlags);
        [DllImport("pdh.dll")]
        internal static extern long PdhCloseQuery(IntPtr queryHandle);
        [DllImport("pdh.dll")]
        private static extern long PdhCollectQueryData(PdhSafeQueryHandle queryHandle);
        [DllImport("pdh.dll")]
        private static extern long PdhCollectQueryDataWithTime(PdhSafeQueryHandle queryHandle, ref long pllTimeStamp);
        [DllImport("pdh.dll", CharSet=CharSet.Unicode)]
        private static extern long PdhEnumMachinesH(PdhSafeDataSourceHandle hDataSource, IntPtr mszMachineNameList, ref IntPtr pcchBufferLength);
        [DllImport("pdh.dll", CharSet=CharSet.Unicode)]
        private static extern long PdhEnumObjectItemsH(PdhSafeDataSourceHandle hDataSource, string szMachineName, string szObjectName, IntPtr mszCounterList, ref IntPtr pcchCounterListLength, IntPtr mszInstanceList, ref IntPtr pcchInstanceListLength, long dwDetailLevel, long dwFlags);
        [DllImport("pdh.dll", CharSet=CharSet.Unicode)]
        private static extern long PdhEnumObjectsH(PdhSafeDataSourceHandle hDataSource, string szMachineName, IntPtr mszObjectList, ref IntPtr pcchBufferLength, long dwDetailLevel, bool bRefresh);
        [DllImport("pdh.dll", CharSet=CharSet.Unicode)]
        private static extern long PdhExpandWildCardPathH(PdhSafeDataSourceHandle hDataSource, string szWildCardPath, IntPtr mszExpandedPathList, ref IntPtr pcchPathListLength, long dwFlags);
        [DllImport("pdh.dll", CharSet=CharSet.Unicode)]
        private static extern long PdhGetCounterInfo(IntPtr hCounter, [MarshalAs(UnmanagedType.U1)] bool bRetrieveExplainText, ref IntPtr pdwBufferSize, IntPtr lpBuffer);
        [DllImport("pdh.dll", CharSet=CharSet.Unicode)]
        private static extern long PdhGetCounterTimeBase(IntPtr hCounter, out ulong pTimeBase);
        [DllImport("pdh.dll", CharSet=CharSet.Unicode)]
        private static extern long PdhGetDataSourceTimeRangeH(PdhSafeDataSourceHandle hDataSource, ref IntPtr pdwNumEntries, ref PDH_TIME_INFO pInfo, ref IntPtr pdwBufferSize);
        [DllImport("pdh.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Unicode)]
        private static extern IntPtr PdhGetExplainText(string szMachineName, string szObjectName, string szCounterName);
        [DllImport("pdh.dll", CharSet=CharSet.Unicode)]
        private static extern long PdhGetFormattedCounterValue(IntPtr counterHandle, long dwFormat, out IntPtr lpdwType, out PDH_FMT_COUNTERVALUE_DOUBLE pValue);
        [DllImport("pdh.dll", CharSet=CharSet.Unicode)]
        private static extern long PdhGetRawCounterValue(IntPtr hCounter, out IntPtr lpdwType, out PDH_RAW_COUNTER pValue);
        [DllImport("pdh.dll", CharSet=CharSet.Unicode)]
        private static extern long PdhLookupPerfNameByIndex(string szMachineName, long dwNameIndex, IntPtr szNameBuffer, ref int pcchNameBufferSize);
        [DllImport("pdh.dll", CharSet=CharSet.Unicode)]
        private static extern long PdhMakeCounterPath(ref PDH_COUNTER_PATH_ELEMENTS pCounterPathElements, IntPtr szFullPathBuffer, ref IntPtr pcchBufferSize, long dwFlags);
        [DllImport("pdh.dll", CharSet=CharSet.Unicode)]
        private static extern long PdhOpenLog(string szLogFileName, int dwAccessFlags, ref PdhLogFileType lpdwLogType, PdhSafeQueryHandle hQuery, long dwMaxSize, string szUserCaption, out PdhSafeLogHandle phLog);
        [DllImport("pdh.dll")]
        private static extern long PdhOpenQueryH(PdhSafeDataSourceHandle hDataSource, IntPtr dwUserData, out PdhSafeQueryHandle phQuery);
        [DllImport("pdh.dll", CharSet=CharSet.Unicode)]
        private static extern long PdhParseCounterPath(string szFullPathBuffer, IntPtr pCounterPathElements, ref IntPtr pdwBufferSize, long dwFlags);
        [DllImport("pdh.dll", CharSet=CharSet.Unicode)]
        private static extern void PdhResetRelogCounterValues(PdhSafeLogHandle LogHandle);
        [DllImport("pdh.dll", CharSet=CharSet.Unicode)]
        private static extern long PdhSetCounterValue(IntPtr CounterHandle, ref PDH_RAW_COUNTER Value, string InstanceName);
        [DllImport("pdh.dll", CharSet=CharSet.Unicode)]
        private static extern long PdhSetQueryTimeRange(PdhSafeQueryHandle hQuery, ref PDH_TIME_INFO pInfo);
        [DllImport("pdh.dll", CharSet=CharSet.Unicode)]
        private static extern long PdhValidatePath(string szFullPathBuffer);
        [DllImport("pdh.dll", CharSet=CharSet.Unicode)]
        private static extern long PdhValidatePathEx(PdhSafeDataSourceHandle hDataSource, string szFullPathBuffer);
        [DllImport("pdh.dll")]
        private static extern long PdhWriteRelogSample(PdhSafeLogHandle LogHandle, long Timestamp);
        public long ReadNextSet(out PerformanceCounterSampleSet nextSet, bool bSkipReading)
        {
            if (this.m_isPreVista)
            {
                return this.ReadNextSetPreVista(out nextSet, bSkipReading);
            }
            long num = 0;
            nextSet = null;
            long pllTimeStamp = 0L;
            num = PdhCollectQueryDataWithTime(this.m_hQuery, ref pllTimeStamp);
            if (bSkipReading)
            {
                return num;
            }
            if ((num != 0) && (num != 0x800007d5L))
            {
                return num;
            }
            DateTime now = DateTime.Now;
            if (num != 0x800007d5L)
            {
                now = new DateTime(DateTime.FromFileTimeUtc(pllTimeStamp).Ticks, DateTimeKind.Local);
            }
            PerformanceCounterSample[] counterSamples = new PerformanceCounterSample[this.m_ConsumerPathToHandleAndInstanceMap.Count];
            long num3 = 0;
            long num4 = 0;
            long num5 = 0;
            foreach (string str in this.m_ConsumerPathToHandleAndInstanceMap.Keys)
            {
                PDH_RAW_COUNTER pdh_raw_counter;
                IntPtr lpdwType = new IntPtr(0);
                long counterType = 0x40030403;
                long defaultScale = 0;
                ulong timeBase = 0L;
                IntPtr hCounter = this.m_ConsumerPathToHandleAndInstanceMap[str].hCounter;
                this.GetCounterInfoPlus(hCounter, out counterType, out defaultScale, out timeBase);
                num = PdhGetRawCounterValue(hCounter, out lpdwType, out pdh_raw_counter);
                if (num != 0)
                {
                    counterSamples[num3++] = new PerformanceCounterSample(str, this.m_ConsumerPathToHandleAndInstanceMap[str].InstanceName, 0.0, 0L, 0L, 0, PerformanceCounterType.RawBase, defaultScale, timeBase, now, (ulong) now.ToFileTime(), (pdh_raw_counter.CStatus == 0) ? num : pdh_raw_counter.CStatus);
                    num4++;
                    num5 = num;
                }
                else
                {
                    PDH_FMT_COUNTERVALUE_DOUBLE pdh_fmt_countervalue_double;
                    long fileTime = (pdh_raw_counter.TimeStamp.dwHighDateTime << 0x20) + ((long) ((ulong) pdh_raw_counter.TimeStamp.dwLowDateTime));
                    DateTime timeStamp = new DateTime(DateTime.FromFileTimeUtc(fileTime).Ticks, DateTimeKind.Local);
                    num = PdhGetFormattedCounterValue(hCounter, 0x8200, out lpdwType, out pdh_fmt_countervalue_double);
                    if (num != 0)
                    {
                        counterSamples[num3++] = new PerformanceCounterSample(str, this.m_ConsumerPathToHandleAndInstanceMap[str].InstanceName, 0.0, (ulong) pdh_raw_counter.FirstValue, (ulong) pdh_raw_counter.SecondValue, pdh_raw_counter.MultiCount, (PerformanceCounterType) counterType, defaultScale, timeBase, timeStamp, (ulong) fileTime, (pdh_fmt_countervalue_double.CStatus == 0) ? num : pdh_raw_counter.CStatus);
                        num4++;
                        num5 = num;
                    }
                    else
                    {
                        counterSamples[num3++] = new PerformanceCounterSample(str, this.m_ConsumerPathToHandleAndInstanceMap[str].InstanceName, pdh_fmt_countervalue_double.doubleValue, (ulong) pdh_raw_counter.FirstValue, (ulong) pdh_raw_counter.SecondValue, pdh_raw_counter.MultiCount, (PerformanceCounterType) lpdwType.ToInt32(), defaultScale, timeBase, timeStamp, (ulong) fileTime, pdh_fmt_countervalue_double.CStatus);
                    }
                }
            }
            nextSet = new PerformanceCounterSampleSet(now, counterSamples, this.m_firstReading);
            this.m_firstReading = false;
            if (num4 == counterSamples.Length)
            {
                return num5;
            }
            return 0;
        }

        public long ReadNextSetPreVista(out PerformanceCounterSampleSet nextSet, bool bSkipReading)
        {
            long num = 0;
            nextSet = null;
            num = PdhCollectQueryData(this.m_hQuery);
            if (bSkipReading)
            {
                return num;
            }
            if ((num != 0) && (num != 0x800007d5L))
            {
                return num;
            }
            PerformanceCounterSample[] counterSamples = new PerformanceCounterSample[this.m_ConsumerPathToHandleAndInstanceMap.Count];
            long num2 = 0;
            long num3 = 0;
            long num4 = 0;
            DateTime now = DateTime.Now;
            foreach (string str in this.m_ConsumerPathToHandleAndInstanceMap.Keys)
            {
                PDH_RAW_COUNTER pdh_raw_counter;
                IntPtr lpdwType = new IntPtr(0);
                long counterType = 0x40030403;
                long defaultScale = 0;
                ulong timeBase = 0L;
                IntPtr hCounter = this.m_ConsumerPathToHandleAndInstanceMap[str].hCounter;
                this.GetCounterInfoPlus(hCounter, out counterType, out defaultScale, out timeBase);
                num = PdhGetRawCounterValue(hCounter, out lpdwType, out pdh_raw_counter);
                switch (num)
                {
                    case 0xc0000bc6L:
                    case 0x800007d5L:
                        counterSamples[num2++] = new PerformanceCounterSample(str, this.m_ConsumerPathToHandleAndInstanceMap[str].InstanceName, 0.0, 0L, 0L, 0, PerformanceCounterType.RawBase, defaultScale, timeBase, DateTime.Now, (ulong) DateTime.Now.ToFileTime(), pdh_raw_counter.CStatus);
                        num3++;
                        num4 = num;
                        break;

                    default:
                    {
                        PDH_FMT_COUNTERVALUE_DOUBLE pdh_fmt_countervalue_double;
                        if (num != 0)
                        {
                            return num;
                        }
                        long fileTime = (pdh_raw_counter.TimeStamp.dwHighDateTime << 0x20) + ((long) ((ulong) pdh_raw_counter.TimeStamp.dwLowDateTime));
                        now = new DateTime(DateTime.FromFileTimeUtc(fileTime).Ticks, DateTimeKind.Local);
                        num = PdhGetFormattedCounterValue(hCounter, 0x8200, out lpdwType, out pdh_fmt_countervalue_double);
                        switch (num)
                        {
                            case 0xc0000bc6L:
                            case 0x800007d5L:
                            {
                                counterSamples[num2++] = new PerformanceCounterSample(str, this.m_ConsumerPathToHandleAndInstanceMap[str].InstanceName, 0.0, (ulong) pdh_raw_counter.FirstValue, (ulong) pdh_raw_counter.SecondValue, pdh_raw_counter.MultiCount, (PerformanceCounterType) counterType, defaultScale, timeBase, now, (ulong) fileTime, pdh_fmt_countervalue_double.CStatus);
                                num3++;
                                num4 = num;
                                continue;
                            }
                        }
                        if (num != 0)
                        {
                            return num;
                        }
                        counterSamples[num2++] = new PerformanceCounterSample(str, this.m_ConsumerPathToHandleAndInstanceMap[str].InstanceName, pdh_fmt_countervalue_double.doubleValue, (ulong) pdh_raw_counter.FirstValue, (ulong) pdh_raw_counter.SecondValue, pdh_raw_counter.MultiCount, (PerformanceCounterType) lpdwType.ToInt32(), defaultScale, timeBase, now, (ulong) fileTime, pdh_fmt_countervalue_double.CStatus);
                        break;
                    }
                }
            }
            nextSet = new PerformanceCounterSampleSet(now, counterSamples, this.m_firstReading);
            this.m_firstReading = false;
            if (num3 == counterSamples.Length)
            {
                return num4;
            }
            return 0;
        }

        private void ReadPdhMultiString(ref IntPtr strNative, int strSize, ref StringCollection strColl)
        {
            int ofs = 0;
            string str = "";
            while (ofs <= ((strSize * 2) - 4))
            {
                int num2 = Marshal.ReadInt32((IntPtr) strNative, ofs);
                if (num2 == 0)
                {
                    break;
                }
                str = str + ((char) num2);
                ofs += 2;
            }
            str = str.TrimEnd(new char[1]);
            strColl.AddRange(str.Split(new char[1]));
        }

        public void ResetRelogValues()
        {
            PdhResetRelogCounterValues(this.m_hOutputLog);
        }

        public long SetCounterValue(PerformanceCounterSample sample, out bool bUnknownPath)
        {
            bUnknownPath = false;
            string key = sample.Path.ToLower(CultureInfo.InvariantCulture);
            if (!this.m_ReloggerPathToHandleAndInstanceMap.ContainsKey(key))
            {
                bUnknownPath = true;
                return 0;
            }
            PDH_RAW_COUNTER pdh_raw_counter = new PDH_RAW_COUNTER {
                FirstValue = (long) sample.RawValue,
                SecondValue = (long) sample.SecondValue,
                MultiCount = sample.MultipleCount
            };
            DateTime time2 = new DateTime(sample.Timestamp.Ticks, DateTimeKind.Utc);
            pdh_raw_counter.TimeStamp.dwHighDateTime = (int) (((ulong) (time2.ToFileTimeUtc() >> 0x20)) & 0xffffffffL);
            DateTime time4 = new DateTime(sample.Timestamp.Ticks, DateTimeKind.Utc);
            pdh_raw_counter.TimeStamp.dwLowDateTime = (int) (((ulong) time4.ToFileTimeUtc()) & 0xffffffffL);
            pdh_raw_counter.CStatus = sample.Status;
            return PdhSetCounterValue(this.m_ReloggerPathToHandleAndInstanceMap[key].hCounter, ref pdh_raw_counter, this.m_ReloggerPathToHandleAndInstanceMap[key].InstanceName);
        }

        public long SetQueryTimeRange(DateTime startTime, DateTime endTime)
        {
            PDH_TIME_INFO pInfo = new PDH_TIME_INFO();
            if ((startTime != DateTime.MinValue) && (startTime.Kind == DateTimeKind.Local))
            {
                startTime = new DateTime(startTime.Ticks, DateTimeKind.Utc);
            }
            pInfo.StartTime = (startTime == DateTime.MinValue) ? 0L : startTime.ToFileTimeUtc();
            if ((endTime != DateTime.MaxValue) && (endTime.Kind == DateTimeKind.Local))
            {
                endTime = new DateTime(endTime.Ticks, DateTimeKind.Utc);
            }
            pInfo.EndTime = (endTime == DateTime.MaxValue) ? 0x7fffffffffffffffL : endTime.ToFileTimeUtc();
            pInfo.SampleCount = 0;
            return PdhSetQueryTimeRange(this.m_hQuery, ref pInfo);
        }

        public long TranslateLocalCounterPath(string englishPath, out string localizedPath)
        {
            string str5;
            string str6;
            long num = 0;
            localizedPath = "";
            PDH_COUNTER_PATH_ELEMENTS pCounterPathElements = new PDH_COUNTER_PATH_ELEMENTS();
            num = this.ParsePath(englishPath, ref pCounterPathElements);
            if (num != 0)
            {
                return num;
            }
            string str = pCounterPathElements.MachineName.ToLower(CultureInfo.InvariantCulture).TrimStart(new char[] { '\\' });
            string str2 = pCounterPathElements.CounterName.ToLower(CultureInfo.InvariantCulture);
            string str3 = pCounterPathElements.ObjectName.ToLower(CultureInfo.InvariantCulture);
            string[] strArray = (string[]) Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Perflib\009").GetValue("Counter");
            int num2 = -1;
            int num3 = -1;
            for (long i = 1; i < strArray.Length; i++)
            {
                string str4 = strArray[i];
                if (str4.ToLower(CultureInfo.InvariantCulture) == str2)
                {
                    try
                    {
                        num2 = Convert.ToInt32(strArray[(int) ((IntPtr) (i - 1))], CultureInfo.InvariantCulture);
                        goto Label_0114;
                    }
                    catch (Exception)
                    {
                        return 0xc0000bc4;
                    }
                }
                if (str4.ToLower(CultureInfo.InvariantCulture) == str3)
                {
                    try
                    {
                        num3 = Convert.ToInt32(strArray[(int) ((IntPtr) (i - 1))], CultureInfo.InvariantCulture);
                    }
                    catch (Exception)
                    {
                        return 0xc0000bc4;
                    }
                }
            Label_0114:
                if ((num2 != -1) && (num3 != -1))
                {
                    break;
                }
            }
            if ((num2 == -1) || (num3 == -1))
            {
                return 0xc0000bc4;
            }
            num = this.LookupPerfNameByIndex(pCounterPathElements.MachineName, (long) num3, out str5);
            if (num != 0)
            {
                return num;
            }
            pCounterPathElements.ObjectName = str5;
            num = this.LookupPerfNameByIndex(pCounterPathElements.MachineName, (long) num2, out str6);
            if (num != 0)
            {
                return num;
            }
            pCounterPathElements.CounterName = str6;
            return this.MakePath(pCounterPathElements, out localizedPath, false);
        }

        public long WriteRelogSample(DateTime timeStamp)
        {
            DateTime time = new DateTime(timeStamp.Ticks, DateTimeKind.Utc);
            return PdhWriteRelogSample(this.m_hOutputLog, time.ToFileTimeUtc());
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        private struct PDH_COUNTER_PATH_ELEMENTS
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string MachineName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string ObjectName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string InstanceName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string ParentInstance;
            public long InstanceIndex;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string CounterName;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PDH_FMT_COUNTERVALUE_DOUBLE
        {
            public long CStatus;
            public double doubleValue;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PDH_FMT_COUNTERVALUE_LARGE
        {
            public long CStatus;
            public long largeValue;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PDH_FMT_COUNTERVALUE_UNICODE
        {
            public long CStatus;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string WideStringValue;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PDH_RAW_COUNTER
        {
            public long CStatus;
            public System.Runtime.InteropServices.ComTypes.FILETIME TimeStamp;
            public long FirstValue;
            public long SecondValue;
            public long MultiCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PDH_TIME_INFO
        {
            public long StartTime;
            public long EndTime;
            public long SampleCount;
        }
    }
}

