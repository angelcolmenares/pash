namespace System.Management.Automation.Sqm
{
    using Microsoft.PowerShell;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    public static class PSSQMAPI
    {
        private static readonly Dictionary<string, int> cmdletData;
        private static readonly Dictionary<int, int> dataValueCache;
        private static bool isWinSQMEnabled;
        private static bool isWorkflowHost = false;
        private static readonly Dictionary<Guid, long> runspaceDurationData;
        private static long startedAtTick;
        private static readonly object syncObject = new object();
        private static readonly long timeValueThreshold;
        private static readonly Dictionary<string, int> workflowCommonParameterData;
        private static readonly Dictionary<string, int> workflowData;
        private static readonly Dictionary<Guid, long> workflowExecutionDurationData;
        private static readonly Dictionary<string, int> workflowOotbActivityData;
        private static readonly Dictionary<string, int> workflowSpecificParameterTypeData;
        private static Dictionary<Guid, Tuple<int, int, int, int>> workflowStateData;
        private static readonly Dictionary<string, int> workflowTypeData;

        static PSSQMAPI()
        {
            if (WinSQMWrapper.IsWinSqmOptedIn())
            {
                dataValueCache = new Dictionary<int, int>();
                cmdletData = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                workflowData = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                workflowCommonParameterData = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                workflowOotbActivityData = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                workflowSpecificParameterTypeData = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                workflowStateData = new Dictionary<Guid, Tuple<int, int, int, int>>();
                workflowTypeData = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                runspaceDurationData = new Dictionary<Guid, long>();
                workflowExecutionDurationData = new Dictionary<Guid, long>();
                timeValueThreshold = 0x23c34600L;
                AppDomain.CurrentDomain.ProcessExit += new EventHandler(PSSQMAPI.CurrentDomain_ProcessExit);
                startedAtTick = DateTime.Now.Ticks;
                isWinSQMEnabled = true;
            }
        }

        private static void CurrentDomain_ProcessExit(object source, EventArgs args)
        {
            LogAllDataSuppressExceptions();
        }

        private static void FlushDataSuppressExceptions()
        {
            try
            {
                int key = 0x2098;
                if (dataValueCache.ContainsKey(key))
                {
                    WinSQMWrapper.WinSqmSet(key, dataValueCache[key]);
                    dataValueCache.Remove(key);
                }
                WinSQMWrapper.WinSqmIncrement(dataValueCache);
                dataValueCache.Clear();
                WriteWorkflowStateDataToStreamAndClear();
                WriteAllDataToStreamAndClear(cmdletData, 0x2665);
                WriteAllDataToStreamAndClear(workflowData, 0x2662);
                WriteAllDataToStreamAndClear(workflowTypeData, 0x2696);
                WriteAllDataToStreamAndClear(workflowCommonParameterData, 0x2660);
                WriteAllDataToStreamAndClear(workflowOotbActivityData, 0x2661);
                WriteAllDataToStreamAndClear(workflowSpecificParameterTypeData, 0x2663);
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
            }
        }

        public static void IncrementData(CmdletInfo cmdlet)
        {
            if (isWinSQMEnabled && ((cmdlet.PSSnapIn != null) && cmdlet.PSSnapIn.IsDefault))
            {
                IncrementDataPoint(cmdlet.Name);
            }
        }

        public static void IncrementData(CommandTypes cmdType)
        {
            if (!isWinSQMEnabled)
            {
                return;
            }
            PSSqmDataPoint cmdlet = PSSqmDataPoint.Cmdlet;
            CommandTypes types = cmdType;
            if (types <= CommandTypes.ExternalScript)
            {
                switch (types)
                {
                    case CommandTypes.Alias:
                        cmdlet = PSSqmDataPoint.Alias;
                        goto Label_008B;

                    case CommandTypes.Function:
                        cmdlet = PSSqmDataPoint.Function;
                        goto Label_008B;

                    case (CommandTypes.Function | CommandTypes.Alias):
                        return;

                    case CommandTypes.Filter:
                        cmdlet = PSSqmDataPoint.Filter;
                        goto Label_008B;

                    case CommandTypes.Cmdlet:
                        cmdlet = PSSqmDataPoint.Cmdlet;
                        goto Label_008B;

                    case CommandTypes.ExternalScript:
                        cmdlet = PSSqmDataPoint.ExternalScript;
                        goto Label_008B;
                }
                return;
            }
            if (types != CommandTypes.Application)
            {
                if (types != CommandTypes.Script)
                {
                    if (types != CommandTypes.Workflow)
                    {
                        return;
                    }
                    cmdlet = PSSqmDataPoint.Function;
                }
                else
                {
                    cmdlet = PSSqmDataPoint.Script;
                }
            }
            else
            {
                cmdlet = PSSqmDataPoint.Application;
            }
        Label_008B:
            IncrementDataPoint((int) cmdlet);
        }

        private static void IncrementDataPoint(string cmdletName)
        {
            lock (syncObject)
            {
                int num;
                cmdletData.TryGetValue(cmdletName, out num);
                num++;
                cmdletData[cmdletName] = num;
            }
        }

        public static void IncrementDataPoint(int dataPoint)
        {
            if (isWinSQMEnabled)
            {
                lock (syncObject)
                {
                    int num;
                    dataValueCache.TryGetValue(dataPoint, out num);
                    num++;
                    dataValueCache[dataPoint] = num;
                }
            }
        }

        public static void IncrementWorkflowActivityPresent(string activityName)
        {
            if (isWinSQMEnabled)
            {
                lock (syncObject)
                {
                    int num;
                    workflowOotbActivityData.TryGetValue(activityName, out num);
                    num++;
                    workflowOotbActivityData[activityName] = num;
                }
            }
        }

        public static void IncrementWorkflowCommonParameterPresent(string parameterName)
        {
            if (isWinSQMEnabled)
            {
                lock (syncObject)
                {
                    int num;
                    workflowCommonParameterData.TryGetValue(parameterName, out num);
                    num++;
                    workflowCommonParameterData[parameterName] = num;
                }
            }
        }

        public static void IncrementWorkflowExecuted(string workflowName)
        {
            if (isWinSQMEnabled)
            {
                lock (syncObject)
                {
                    int num;
                    workflowData.TryGetValue(workflowName.GetHashCode().ToString(CultureInfo.InvariantCulture), out num);
                    num++;
                    workflowData[workflowName.GetHashCode().ToString(CultureInfo.InvariantCulture)] = num;
                }
            }
        }

        public static void IncrementWorkflowSpecificParameterType(Type parameterType)
        {
            if (isWinSQMEnabled)
            {
                lock (syncObject)
                {
                    int num;
                    IncrementDataPoint((int) 0x265e);
                    workflowSpecificParameterTypeData.TryGetValue(parameterType.FullName, out num);
                    num++;
                    workflowSpecificParameterTypeData[parameterType.FullName] = num;
                }
            }
        }

        public static void IncrementWorkflowStateData(Guid parentJobInstanceId, JobState state)
        {
            if (isWinSQMEnabled)
            {
                lock (syncObject)
                {
                    int num;
                    int num2;
                    int num3;
                    int num4;
                    if (workflowStateData.ContainsKey(parentJobInstanceId))
                    {
                        Tuple<int, int, int, int> tuple = workflowStateData[parentJobInstanceId];
                        num = tuple.Item1;
                        if (num != 0)
                        {
                            num2 = tuple.Item2;
                            num3 = tuple.Item3;
                            num4 = tuple.Item4;
                            switch (state)
                            {
                                case JobState.Completed:
                                    num2++;
                                    goto Label_008A;

                                case JobState.Failed:
                                    num3++;
                                    goto Label_008A;

                                case JobState.Stopped:
                                    num4++;
                                    goto Label_008A;
                            }
                        }
                    }
                    goto Label_00B1;
                Label_008A:
                    num--;
                    workflowStateData[parentJobInstanceId] = new Tuple<int, int, int, int>(num, num2, num3, num4);
                Label_00B1:;
                }
            }
        }

        public static void IncrementWorkflowType(string workflowType)
        {
            if (isWinSQMEnabled)
            {
                lock (syncObject)
                {
                    int num;
                    workflowTypeData.TryGetValue(workflowType, out num);
                    num++;
                    workflowTypeData[workflowType] = num;
                }
            }
        }

        public static void InitiateWorkflowStateDataTracking(Job parentJob)
        {
            if ((isWinSQMEnabled && !parentJob.IsFinishedState(parentJob.JobStateInfo.State)) && (parentJob.ChildJobs.Count != 0))
            {
                lock (syncObject)
                {
                    if (!workflowStateData.ContainsKey(parentJob.InstanceId))
                    {
                        workflowStateData.Add(parentJob.InstanceId, new Tuple<int, int, int, int>((int) parentJob.ChildJobs.Count, 0, 0, 0));
                        foreach (Job job in parentJob.ChildJobs)
                        {
                            if (job.IsFinishedState(job.JobStateInfo.State))
                            {
                                IncrementWorkflowStateData(parentJob.InstanceId, job.JobStateInfo.State);
                            }
                        }
                    }
                }
            }
        }

        public static void LogAllDataSuppressExceptions()
        {
            if (isWinSQMEnabled)
            {
                lock (syncObject)
                {
                    long ticks = DateTime.Now.Ticks;
                    long num2 = ticks - startedAtTick;
                    if (num2 >= timeValueThreshold)
                    {
                        if (isWorkflowHost)
                        {
                            double totalMinutes = TimeSpan.FromTicks(num2).TotalMinutes;
                            if (totalMinutes > 4294967295)
                            {
                                dataValueCache.Add(0x265d, int.MaxValue);
                            }
                            else
                            {
                                dataValueCache.Add(0x265d, (int) totalMinutes);
                            }
                        }
                        FlushDataSuppressExceptions();
                        startedAtTick = ticks;
                    }
                }
            }
        }

        public static void NoteRunspaceEnd(Guid rsInstanceId)
        {
            if (isWinSQMEnabled)
            {
                long ticks = DateTime.Now.Ticks;
                long num2 = ticks;
                lock (syncObject)
                {
                    if (!runspaceDurationData.ContainsKey(rsInstanceId))
                    {
                        return;
                    }
                    num2 = runspaceDurationData[rsInstanceId];
                    runspaceDurationData.Remove(rsInstanceId);
                }
                try
                {
                    long num3 = ticks - num2;
                    if (num3 >= timeValueThreshold)
                    {
                        TimeSpan span = new TimeSpan(num3);
                        WinSQMWrapper.WinSqmAddToStream(0x2666, span.TotalMinutes.ToString(CultureInfo.InvariantCulture));
                    }
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                }
            }
        }

        public static void NoteRunspaceStart(Guid rsInstanceId)
        {
            if (isWinSQMEnabled)
            {
                lock (syncObject)
                {
                    runspaceDurationData[rsInstanceId] = DateTime.Now.Ticks;
                }
            }
        }

        public static void NoteSessionConfigurationIdleTimeout(int idleTimeout)
        {
            if (isWinSQMEnabled)
            {
                try
                {
                    WinSQMWrapper.WinSqmAddToStream(0x209f, idleTimeout.ToString(CultureInfo.InvariantCulture));
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                }
            }
        }

        public static void NoteSessionConfigurationOutputBufferingMode(string optBufferingMode)
        {
            if (isWinSQMEnabled)
            {
                try
                {
                    WinSQMWrapper.WinSqmAddToStream(0x20b8, optBufferingMode);
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                }
            }
        }

        public static void NoteWorkflowCommonParametersValues(string parameterName, int data)
        {
            if (isWinSQMEnabled)
            {
                try
                {
                    WinSQMWrapper.WinSqmAddToStream(0x268d, parameterName, data);
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                }
            }
        }

        public static void NoteWorkflowEnd(Guid workflowInstanceId)
        {
            if (isWinSQMEnabled)
            {
                long num2;
                long ticks = DateTime.Now.Ticks;
                lock (syncObject)
                {
                    if (!workflowExecutionDurationData.ContainsKey(workflowInstanceId))
                    {
                        return;
                    }
                    num2 = workflowExecutionDurationData[workflowInstanceId];
                    workflowExecutionDurationData.Remove(workflowInstanceId);
                }
                try
                {
                    long num3 = ticks - num2;
                    TimeSpan span = new TimeSpan(num3);
                    WinSQMWrapper.WinSqmAddToStream(0x265c, span.TotalMinutes.ToString(CultureInfo.InvariantCulture));
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                }
            }
        }

        public static void NoteWorkflowEndpointConfiguration(string quotaName, int data)
        {
            if (isWinSQMEnabled)
            {
                try
                {
                    WinSQMWrapper.WinSqmAddToStream(0x2699, quotaName, data);
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                }
            }
        }

        public static void NoteWorkflowOutputStreamSize(int size, string streamType)
        {
            if (isWinSQMEnabled)
            {
                try
                {
                    WinSQMWrapper.WinSqmAddToStream(0x269a, streamType, size);
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                }
            }
        }

        public static void NoteWorkflowStart(Guid workflowInstanceId)
        {
            if (isWinSQMEnabled)
            {
                lock (syncObject)
                {
                    isWorkflowHost = true;
                    workflowExecutionDurationData[workflowInstanceId] = DateTime.Now.Ticks;
                }
            }
        }

        public static void UpdateExecutionPolicy(string shellId, ExecutionPolicy executionPolicy)
        {
            if (isWinSQMEnabled && shellId.Equals(Utils.DefaultPowerShellShellID, StringComparison.OrdinalIgnoreCase))
            {
                lock (syncObject)
                {
                    dataValueCache[0x2098] = (int) executionPolicy;
                }
            }
        }

        public static void UpdateWorkflowsConcurrentExecution(int numberWorkflows)
        {
            if (isWinSQMEnabled)
            {
                lock (syncObject)
                {
                    int num;
                    isWorkflowHost = true;
                    dataValueCache.TryGetValue(0x2697, out num);
                    if (num < numberWorkflows)
                    {
                        dataValueCache[0x2697] = numberWorkflows;
                    }
                }
            }
        }

        private static void WriteAllDataToStreamAndClear(Dictionary<string, int> data, int dataPoint)
        {
            foreach (string str in data.Keys)
            {
                WinSQMWrapper.WinSqmAddToStream(dataPoint, str, data[str]);
            }
            data.Clear();
        }

        private static void WriteWorkflowStateDataToStreamAndClear()
        {
            Dictionary<Guid, Tuple<int, int, int, int>> dictionary = new Dictionary<Guid, Tuple<int, int, int, int>>();
            foreach (KeyValuePair<Guid, Tuple<int, int, int, int>> pair in workflowStateData)
            {
                if (pair.Value.Item1 != 0)
                {
                    dictionary.Add(pair.Key, pair.Value);
                }
                else
                {
                    int num = (pair.Value.Item2 + pair.Value.Item3) + pair.Value.Item4;
                    if (num != 0)
                    {
                        WinSQMWrapper.WinSqmAddToStream(0x2698, JobState.Completed.ToString(), Convert.ToInt32((double) ((((double) pair.Value.Item2) / ((double) num)) * 100.0)));
                        WinSQMWrapper.WinSqmAddToStream(0x2698, JobState.Failed.ToString(), Convert.ToInt32((double) ((((double) pair.Value.Item3) / ((double) num)) * 100.0)));
                        WinSQMWrapper.WinSqmAddToStream(0x2698, JobState.Stopped.ToString(), Convert.ToInt32((double) ((((double) pair.Value.Item4) / ((double) num)) * 100.0)));
                    }
                }
            }
            workflowStateData = dictionary;
        }
    }
}

