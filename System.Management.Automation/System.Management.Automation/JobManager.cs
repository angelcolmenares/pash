namespace System.Management.Automation
{
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Tracing;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;

    public sealed class JobManager
    {
        private readonly Dictionary<string, JobSourceAdapter> _sourceAdapters = new Dictionary<string, JobSourceAdapter>();
        private readonly object _syncObject = new object();
        private static readonly Dictionary<Guid, KeyValuePair<int, string>> JobIdsForReuse = new Dictionary<Guid, KeyValuePair<int, string>>();
        private static readonly object SyncObject = new object();
        private readonly PowerShellTraceSource Tracer = PowerShellTraceSourceFactory.GetTraceSource();

        internal JobManager()
        {
        }

        private JobSourceAdapter AssertAndReturnJobSourceAdapter(string adapterTypeName)
        {
            JobSourceAdapter adapter;
            lock (this._syncObject)
            {
                if (!this._sourceAdapters.TryGetValue(adapterTypeName, out adapter))
                {
                    throw new InvalidOperationException(ResourceManagerCache.GetResourceString("RemotingErrorIdStrings", "JobSourceAdapterNotFound"));
                }
            }
            return adapter;
        }

        private static List<Job2> CallJobFilter(JobSourceAdapter sourceAdapter, object filter, FilterType filterType, bool recurse)
        {
            IList<Job2> jobsByCommand;
            List<Job2> list = new List<Job2>();
            switch (filterType)
            {
                case FilterType.Command:
                    jobsByCommand = sourceAdapter.GetJobsByCommand((string) filter, recurse);
                    break;

                case FilterType.Filter:
                    jobsByCommand = sourceAdapter.GetJobsByFilter((Dictionary<string, object>) filter, recurse);
                    break;

                case FilterType.Name:
                    jobsByCommand = sourceAdapter.GetJobsByName((string) filter, recurse);
                    break;

                case FilterType.State:
                    jobsByCommand = sourceAdapter.GetJobsByState((JobState) filter, recurse);
                    break;

                default:
                    jobsByCommand = sourceAdapter.GetJobs();
                    break;
            }
            if (jobsByCommand != null)
            {
                list.AddRange(jobsByCommand);
            }
            return list;
        }

        private bool CheckTypeNames(JobSourceAdapter sourceAdapter, string[] jobSourceAdapterTypes)
        {
            if ((jobSourceAdapterTypes == null) || (jobSourceAdapterTypes.Length == 0))
            {
                return true;
            }
            string adapterName = this.GetAdapterName(sourceAdapter);
            foreach (string str2 in jobSourceAdapterTypes)
            {
                WildcardPattern pattern = new WildcardPattern(str2, WildcardOptions.IgnoreCase);
                if (pattern.IsMatch(adapterName))
                {
                    return true;
                }
            }
            return false;
        }

        private string GetAdapterName(JobSourceAdapter sourceAdapter)
        {
            if (string.IsNullOrEmpty(sourceAdapter.Name))
            {
                return sourceAdapter.GetType().ToString();
            }
            return sourceAdapter.Name;
        }

        private List<Job2> GetFilteredJobs(object filter, FilterType filterType, Cmdlet cmdlet, bool writeErrorOnException, bool writeObject, bool recurse, string[] jobSourceAdapterTypes)
        {
            List<Job2> list = new List<Job2>();
            lock (this._syncObject)
            {
                foreach (JobSourceAdapter adapter in this._sourceAdapters.Values)
                {
                    List<Job2> collection = null;
                    if (this.CheckTypeNames(adapter, jobSourceAdapterTypes))
                    {
                        try
                        {
                            collection = CallJobFilter(adapter, filter, filterType, recurse);
                        }
                        catch (Exception exception)
                        {
                            this.Tracer.TraceException(exception);
                            CommandProcessorBase.CheckForSevereException(exception);
                            WriteErrorOrWarning(writeErrorOnException, cmdlet, exception, "JobSourceAdapterGetJobsError", adapter);
                        }
                        if (collection != null)
                        {
                            list.AddRange(collection);
                        }
                    }
                }
            }
            if (writeObject)
            {
                foreach (Job2 job in list)
                {
                    cmdlet.WriteObject(job);
                }
            }
            return list;
        }

        internal Job2 GetJobById(int id, Cmdlet cmdlet, bool writeErrorOnException, bool writeObject, bool recurse)
        {
            return this.GetJobThroughId<int>(Guid.Empty, id, cmdlet, writeErrorOnException, writeObject, recurse);
        }

        internal Job2 GetJobByInstanceId(Guid instanceId, Cmdlet cmdlet, bool writeErrorOnException, bool writeObject, bool recurse)
        {
            return this.GetJobThroughId<Guid>(instanceId, 0, cmdlet, writeErrorOnException, writeObject, recurse);
        }

        internal static JobIdentifier GetJobIdentifier(Guid instanceId, string typeName)
        {
            lock (SyncObject)
            {
                if (JobIdsForReuse.ContainsKey(instanceId))
                {
                    KeyValuePair<int, string> pair = JobIdsForReuse[instanceId];
                    if (pair.Value.Equals(typeName))
                    {
                        KeyValuePair<int, string> pair2 = JobIdsForReuse[instanceId];
                        return new JobIdentifier(pair2.Key, instanceId);
                    }
                }
                return null;
            }
        }

        internal List<Job2> GetJobs(Cmdlet cmdlet, bool writeErrorOnException, bool writeObject, string[] jobSourceAdapterTypes)
        {
            return this.GetFilteredJobs(null, FilterType.None, cmdlet, writeErrorOnException, writeObject, false, jobSourceAdapterTypes);
        }

        internal List<Job2> GetJobsByCommand(string command, Cmdlet cmdlet, bool writeErrorOnException, bool writeObject, bool recurse, string[] jobSourceAdapterTypes)
        {
            return this.GetFilteredJobs(command, FilterType.Command, cmdlet, writeErrorOnException, writeObject, recurse, jobSourceAdapterTypes);
        }

        internal List<Job2> GetJobsByFilter(Dictionary<string, object> filter, Cmdlet cmdlet, bool writeErrorOnException, bool writeObject, bool recurse)
        {
            return this.GetFilteredJobs(filter, FilterType.Filter, cmdlet, writeErrorOnException, writeObject, recurse, null);
        }

        internal List<Job2> GetJobsByName(string name, Cmdlet cmdlet, bool writeErrorOnException, bool writeObject, bool recurse, string[] jobSourceAdapterTypes)
        {
            return this.GetFilteredJobs(name, FilterType.Name, cmdlet, writeErrorOnException, writeObject, recurse, jobSourceAdapterTypes);
        }

        internal List<Job2> GetJobsByState(JobState state, Cmdlet cmdlet, bool writeErrorOnException, bool writeObject, bool recurse, string[] jobSourceAdapterTypes)
        {
            return this.GetFilteredJobs(state, FilterType.State, cmdlet, writeErrorOnException, writeObject, recurse, jobSourceAdapterTypes);
        }

        private JobSourceAdapter GetJobSourceAdapter(JobDefinition definition)
        {
            string jobSourceAdapterTypeName;
            JobSourceAdapter adapter;
            if (!string.IsNullOrEmpty(definition.JobSourceAdapterTypeName))
            {
                jobSourceAdapterTypeName = definition.JobSourceAdapterTypeName;
            }
            else
            {
                if (definition.JobSourceAdapterType == null)
                {
                    throw new InvalidOperationException(ResourceManagerCache.GetResourceString("RemotingErrorIdStrings", "JobSourceAdapterNotFound"));
                }
                jobSourceAdapterTypeName = definition.JobSourceAdapterType.Name;
            }
            bool flag = false;
            lock (this._syncObject)
            {
                flag = this._sourceAdapters.TryGetValue(jobSourceAdapterTypeName, out adapter);
            }
            if (flag)
            {
                return adapter;
            }
            if (string.IsNullOrEmpty(definition.ModuleName))
            {
                throw new InvalidOperationException(ResourceManagerCache.GetResourceString("RemotingErrorIdStrings", "JobSourceAdapterNotFound"));
            }
            Exception innerException = null;
            try
            {
                InitialSessionState initialSessionState = InitialSessionState.CreateDefault2();
                initialSessionState.Commands.Clear();
                initialSessionState.Formats.Clear();
                initialSessionState.Commands.Add(new SessionStateCmdletEntry("Import-Module", typeof(ImportModuleCommand), null));
                using (PowerShell shell = PowerShell.Create(initialSessionState))
                {
                    shell.AddCommand("Import-Module");
                    shell.AddParameter("Name", definition.ModuleName);
                    shell.Invoke();
                    if (shell.ErrorBuffer.Count > 0)
                    {
                        innerException = shell.ErrorBuffer[0].Exception;
                    }
                }
            }
            catch (RuntimeException exception2)
            {
                innerException = exception2;
            }
            catch (InvalidOperationException exception3)
            {
                innerException = exception3;
            }
            catch (ScriptCallDepthException exception4)
            {
                innerException = exception4;
            }
            catch (SecurityException exception5)
            {
                innerException = exception5;
            }
            catch (ThreadAbortException exception6)
            {
                innerException = exception6;
            }
            if (innerException != null)
            {
                throw new InvalidOperationException(ResourceManagerCache.GetResourceString("RemotingErrorIdStrings", "JobSourceAdapterNotFound"), innerException);
            }
            return this.AssertAndReturnJobSourceAdapter(jobSourceAdapterTypeName);
        }

        private Job2 GetJobThroughId<T>(Guid guid, int id, Cmdlet cmdlet, bool writeErrorOnException, bool writeObject, bool recurse)
        {
            Job2 sendToPipeline = null;
            lock (this._syncObject)
            {
                foreach (JobSourceAdapter adapter in this._sourceAdapters.Values)
                {
                    try
                    {
                        if (typeof(T) == typeof(Guid))
                        {
                            sendToPipeline = adapter.GetJobByInstanceId(guid, recurse);
                        }
                        else if (typeof(T) == typeof(int))
                        {
                            sendToPipeline = adapter.GetJobBySessionId(id, recurse);
                        }
                    }
                    catch (Exception exception)
                    {
                        this.Tracer.TraceException(exception);
                        CommandProcessorBase.CheckForSevereException(exception);
                        WriteErrorOrWarning(writeErrorOnException, cmdlet, exception, "JobSourceAdapterGetJobByInstanceIdError", adapter);
                    }
                    if (sendToPipeline != null)
                    {
                        if (writeObject)
                        {
                            cmdlet.WriteObject(sendToPipeline);
                        }
                        return sendToPipeline;
                    }
                }
            }
            return null;
        }

        internal List<Job2> GetJobToStart(string definitionName, string definitionPath, string definitionType, Cmdlet cmdlet, bool writeErrorOnException)
        {
            List<Job2> list = new List<Job2>();
            WildcardPattern pattern = (definitionType != null) ? new WildcardPattern(definitionType, WildcardOptions.IgnoreCase) : null;
            lock (this._syncObject)
            {
                foreach (JobSourceAdapter adapter in this._sourceAdapters.Values)
                {
                    try
                    {
                        if (pattern != null)
                        {
                            string adapterName = this.GetAdapterName(adapter);
                            if (!pattern.IsMatch(adapterName))
                            {
                                continue;
                            }
                        }
                        Job2 item = adapter.NewJob(definitionName, definitionPath);
                        if (item != null)
                        {
                            list.Add(item);
                        }
                        if (pattern != null)
                        {
                            return list;
                        }
                    }
                    catch (Exception exception)
                    {
                        this.Tracer.TraceException(exception);
                        CommandProcessorBase.CheckForSevereException(exception);
                        WriteErrorOrWarning(writeErrorOnException, cmdlet, exception, "JobSourceAdapterGetJobByInstanceIdError", adapter);
                    }
                }
                return list;
            }
            return list;
        }

        internal List<string> GetLoadedAdapterNames(string[] adapterTypeNames)
        {
            List<string> list = new List<string>();
            lock (this._syncObject)
            {
                foreach (JobSourceAdapter adapter in this._sourceAdapters.Values)
                {
                    if (this.CheckTypeNames(adapter, adapterTypeNames))
                    {
                        list.Add(this.GetAdapterName(adapter));
                    }
                }
            }
            return list;
        }

        internal bool IsJobFromAdapter(Guid id, string name)
        {
            lock (this._syncObject)
            {
                foreach (JobSourceAdapter adapter in this._sourceAdapters.Values)
                {
                    if (adapter.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        return (adapter.GetJobByInstanceId(id, false) != null);
                    }
                }
            }
            return false;
        }

        public bool IsRegistered(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return false;
            }
            lock (this._syncObject)
            {
                return this._sourceAdapters.ContainsKey(typeName);
            }
        }

        public Job2 NewJob(JobDefinition definition)
        {
            Job2 job;
            if (definition == null)
            {
                throw new ArgumentNullException("definition");
            }
            JobSourceAdapter jobSourceAdapter = this.GetJobSourceAdapter(definition);
            try
            {
                job = jobSourceAdapter.NewJob(definition);
            }
            catch (Exception exception)
            {
                this.Tracer.TraceException(exception);
                CommandProcessorBase.CheckForSevereException(exception);
                throw;
            }
            return job;
        }

        public Job2 NewJob(JobInvocationInfo specification)
        {
            if (specification == null)
            {
                throw new ArgumentNullException("specification");
            }
            if (specification.Definition == null)
            {
                throw new ArgumentException(ResourceManagerCache.GetResourceString("RemotingErrorIdStrings", "NewJobSpecificationError"), "specification");
            }
            JobSourceAdapter jobSourceAdapter = this.GetJobSourceAdapter(specification.Definition);
            Job2 job = null;
            try
            {
                job = jobSourceAdapter.NewJob(specification);
            }
            catch (Exception exception)
            {
                this.Tracer.TraceException(exception);
                CommandProcessorBase.CheckForSevereException(exception);
                throw;
            }
            return job;
        }

        public void PersistJob(Job2 job, JobDefinition definition)
        {
            if (job == null)
            {
                throw new PSArgumentNullException("job");
            }
            if (definition == null)
            {
                throw new PSArgumentNullException("definition");
            }
            JobSourceAdapter jobSourceAdapter = this.GetJobSourceAdapter(definition);
            try
            {
                jobSourceAdapter.PersistJob(job);
            }
            catch (Exception exception)
            {
                this.Tracer.TraceException(exception);
                CommandProcessorBase.CheckForSevereException(exception);
                throw;
            }
        }

        internal void RegisterJobSourceAdapter(Type jobSourceAdapterType)
        {
            object obj2 = null;
            if ((jobSourceAdapterType.FullName != null) && jobSourceAdapterType.FullName.EndsWith("WorkflowJobSourceAdapter", StringComparison.OrdinalIgnoreCase))
            {
                obj2 = jobSourceAdapterType.GetMethod("GetInstance").Invoke(null, null);
            }
            else
            {
                ConstructorInfo constructor = jobSourceAdapterType.GetConstructor(Type.EmptyTypes);
                if (!constructor.IsPublic)
                {
                    throw new InvalidOperationException(ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "JobManagerRegistrationConstructorError", new object[] { jobSourceAdapterType.FullName }));
                }
                try
                {
                    obj2 = constructor.Invoke(null);
                }
                catch (MemberAccessException exception)
                {
                    this.Tracer.TraceException(exception);
                    throw;
                }
                catch (TargetInvocationException exception2)
                {
                    this.Tracer.TraceException(exception2);
                    throw;
                }
                catch (TargetParameterCountException exception3)
                {
                    this.Tracer.TraceException(exception3);
                    throw;
                }
                catch (NotSupportedException exception4)
                {
                    this.Tracer.TraceException(exception4);
                    throw;
                }
                catch (SecurityException exception5)
                {
                    this.Tracer.TraceException(exception5);
                    throw;
                }
            }
            if (obj2 != null)
            {
                lock (this._syncObject)
                {
                    this._sourceAdapters.Add(jobSourceAdapterType.Name, (JobSourceAdapter) obj2);
                }
            }
        }

        internal void RemoveJob(int sessionJobId, Cmdlet cmdlet, bool writeErrorOnException)
        {
            Job2 job = this.GetJobById(sessionJobId, cmdlet, writeErrorOnException, false, false);
            this.RemoveJob(job, cmdlet, false, false);
        }

        internal void RemoveJob(Job2 job, Cmdlet cmdlet, bool writeErrorOnException, bool throwExceptions = false)
        {
            bool flag = false;
            lock (this._syncObject)
            {
                foreach (JobSourceAdapter adapter in this._sourceAdapters.Values)
                {
                    Job2 jobByInstanceId = null;
                    try
                    {
                        jobByInstanceId = adapter.GetJobByInstanceId(job.InstanceId, true);
                    }
                    catch (Exception exception)
                    {
                        this.Tracer.TraceException(exception);
                        CommandProcessorBase.CheckForSevereException(exception);
                        if (throwExceptions)
                        {
                            throw;
                        }
                        WriteErrorOrWarning(writeErrorOnException, cmdlet, exception, "JobSourceAdapterGetJobError", adapter);
                    }
                    if (jobByInstanceId != null)
                    {
                        flag = true;
                        this.RemoveJobIdForReuse(jobByInstanceId);
                        try
                        {
                            adapter.RemoveJob(job);
                        }
                        catch (Exception exception2)
                        {
                            this.Tracer.TraceException(exception2);
                            CommandProcessorBase.CheckForSevereException(exception2);
                            if (throwExceptions)
                            {
                                throw;
                            }
                            WriteErrorOrWarning(writeErrorOnException, cmdlet, exception2, "JobSourceAdapterRemoveJobError", adapter);
                        }
                    }
                }
            }
            if (!flag && throwExceptions)
            {
                throw new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.ItemNotFoundInRepository, new object[] { "Job repository", job.InstanceId.ToString() }));
            }
        }

        private void RemoveJobIdForReuse(Job job)
        {
            Hashtable duplicateDetector = new Hashtable();
            duplicateDetector.Add(job.Id, job.Id);
            this.RemoveJobIdForReuseHelper(duplicateDetector, job);
        }

        private void RemoveJobIdForReuseHelper(Hashtable duplicateDetector, Job job)
        {
            lock (SyncObject)
            {
                if (JobIdsForReuse.ContainsKey(job.InstanceId))
                {
                    JobIdsForReuse.Remove(job.InstanceId);
                }
            }
            foreach (Job job2 in job.ChildJobs)
            {
                if (!duplicateDetector.ContainsKey(job2.Id))
                {
                    duplicateDetector.Add(job2.Id, job2.Id);
                    this.RemoveJobIdForReuse(job2);
                }
            }
        }

        internal static void SaveJobId(Guid instanceId, int id, string typeName)
        {
            lock (SyncObject)
            {
                if (!JobIdsForReuse.ContainsKey(instanceId))
                {
                    JobIdsForReuse.Add(instanceId, new KeyValuePair<int, string>(id, typeName));
                }
            }
        }

        private static void WriteErrorOrWarning(bool writeErrorOnException, Cmdlet cmdlet, Exception exception, string identifier, JobSourceAdapter sourceAdapter)
        {
            try
            {
                if (writeErrorOnException)
                {
                    cmdlet.WriteError(new ErrorRecord(exception, identifier, ErrorCategory.OpenError, sourceAdapter));
                }
                else
                {
                    cmdlet.WriteWarning(ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "JobSourceAdapterError", new object[] { exception.Message, sourceAdapter.Name }));
                }
            }
            catch (Exception)
            {
            }
        }

        private enum FilterType
        {
            None,
            Command,
            Filter,
            Name,
            State
        }
    }
}

