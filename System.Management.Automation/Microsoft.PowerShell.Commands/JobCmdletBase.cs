namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Remoting;

    public class JobCmdletBase : PSRemotingCmdlet
    {
        internal const string CommandParameter = "Command";
        internal const string CommandParameterSet = "CommandParameterSet";
        private string[] commands;
        private Hashtable filter;
        internal const string FilterParameter = "Filter";
        internal const string FilterParameterSet = "FilterParameterSet";
        internal const string InstanceIdParameter = "InstanceId";
        internal const string InstanceIdParameterSet = "InstanceIdParameterSet";
        private Guid[] instanceIds;
        internal const string JobParameter = "Job";
        internal const string JobParameterSet = "JobParameterSet";
        private JobState jobstate;
        internal const string NameParameter = "Name";
        internal const string NameParameterSet = "NameParameterSet";
        private string[] names;
        internal const string SessionIdParameter = "SessionId";
        internal const string SessionIdParameterSet = "SessionIdParameterSet";
        private int[] sessionIds;
        internal const string StateParameter = "State";
        internal const string StateParameterSet = "StateParameterSet";

        protected override void BeginProcessing()
        {
        }

        private bool CheckIfJob2CanBeRemoved(bool checkForRemove, string parameterName, Job2 job2, string resourceString, params object[] args)
        {
            if (!checkForRemove)
            {
                return true;
            }
            if (job2.IsFinishedState(job2.JobStateInfo.State))
            {
                return true;
            }
            Exception exception = new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(resourceString, args), parameterName);
            base.WriteError(new ErrorRecord(exception, "JobObjectNotFinishedCannotBeRemoved", ErrorCategory.InvalidOperation, job2));
            return false;
        }

        private bool CheckJobCanBeRemoved(Job job, string parameterName, string resourceString, params object[] list)
        {
            if (job.IsFinishedState(job.JobStateInfo.State))
            {
                return true;
            }
            Exception exception = new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(resourceString, list), parameterName);
            base.WriteError(new ErrorRecord(exception, "JobObjectNotFinishedCannotBeRemoved", ErrorCategory.InvalidOperation, job));
            return false;
        }

        internal List<Job> CopyJobsToList(Job[] jobs, bool writeobject, bool checkIfJobCanBeRemoved)
        {
            List<Job> list = new List<Job>();
            if (jobs != null)
            {
                foreach (Job job in jobs)
                {
                    if (!checkIfJobCanBeRemoved || this.CheckJobCanBeRemoved(job, "Job", RemotingErrorIdStrings.JobWithSpecifiedSessionIdNotCompleted, new object[] { job.Id }))
                    {
                        if (writeobject)
                        {
                            base.WriteObject(job);
                        }
                        else
                        {
                            list.Add(job);
                        }
                    }
                }
            }
            return list;
        }

        internal List<Job> FindJobsMatchingByCommand(bool writeobject)
        {
            List<Job> list = new List<Job>();
            if (this.commands != null)
            {
                List<Job> list2 = new List<Job>();
                list2.AddRange(base.JobRepository.Jobs);
                foreach (string str in this.commands)
                {
                    List<Job2> list3 = base.JobManager.GetJobsByCommand(str, this, false, false, false, null);
                    if (list3 != null)
                    {
                        foreach (Job2 job in list3)
                        {
                            list2.Add(job);
                        }
                    }
                    foreach (Job job2 in list2)
                    {
                        WildcardPattern pattern = new WildcardPattern(str, WildcardOptions.IgnoreCase);
                        string input = job2.Command.Trim();
                        if (input.Equals(str.Trim(), StringComparison.OrdinalIgnoreCase) || pattern.IsMatch(input))
                        {
                            if (writeobject)
                            {
                                base.WriteObject(job2);
                            }
                            else
                            {
                                list.Add(job2);
                            }
                        }
                    }
                }
            }
            return list;
        }

        internal List<Job> FindJobsMatchingByFilter(bool writeobject)
        {
            List<Job> list = new List<Job>();
            List<Job> matches = new List<Job>();
            this.FindJobsMatchingByFilterHelper(matches, base.JobRepository.Jobs);
            Dictionary<string, object> filter = new Dictionary<string, object>();
            foreach (string str in this.filter.Keys)
            {
                filter.Add(str, this.filter[str]);
            }
            List<Job2> list3 = base.JobManager.GetJobsByFilter(filter, this, false, false, true);
            if (list3 != null)
            {
                foreach (Job2 job in list3)
                {
                    matches.Add(job);
                }
            }
            foreach (Job job2 in matches)
            {
                if (writeobject)
                {
                    base.WriteObject(job2);
                }
                else
                {
                    list.Add(job2);
                }
            }
            return list;
        }

        private bool FindJobsMatchingByFilterHelper(List<Job> matches, List<Job> jobsToSearch)
        {
            return false;
        }

        internal List<Job> FindJobsMatchingByInstanceId(bool recurse, bool writeobject, bool writeErrorOnNoMatch, bool checkIfJobCanBeRemoved)
        {
            List<Job> matches = new List<Job>();
            Hashtable duplicateDetector = new Hashtable();
            if (this.instanceIds != null)
            {
                foreach (Guid guid in this.instanceIds)
                {
                    duplicateDetector.Clear();
                    bool flag = this.FindJobsMatchingByInstanceIdHelper(matches, base.JobRepository.Jobs, guid, duplicateDetector, recurse, writeobject, checkIfJobCanBeRemoved);
                    Job2 job = base.JobManager.GetJobByInstanceId(guid, this, false, writeobject, recurse);
                    bool flag2 = job != null;
                    if (flag2 && this.CheckIfJob2CanBeRemoved(checkIfJobCanBeRemoved, "InstanceId", job, RemotingErrorIdStrings.JobWithSpecifiedInstanceIdNotCompleted, new object[] { job.Id, job.InstanceId }))
                    {
                        matches.Add(job);
                    }
                    if (!(flag || flag2) && writeErrorOnNoMatch)
                    {
                        Exception exception = PSTraceSource.NewArgumentException("InstanceId", "RemotingErrorIdStrings", PSRemotingErrorId.JobWithSpecifiedInstanceIdNotFound.ToString(), new object[] { guid });
                        base.WriteError(new ErrorRecord(exception, "JobWithSpecifiedInstanceIdNotFound", ErrorCategory.ObjectNotFound, guid));
                    }
                }
            }
            return matches;
        }

        private bool FindJobsMatchingByInstanceIdHelper(List<Job> matches, IList<Job> jobsToSearch, Guid instanceId, Hashtable duplicateDetector, bool recurse, bool writeobject, bool checkIfJobCanBeRemoved)
        {
            bool flag = false;
            foreach (Job job in jobsToSearch)
            {
                if (!duplicateDetector.ContainsKey(job.Id))
                {
                    duplicateDetector.Add(job.Id, job.Id);
                    if (job.InstanceId == instanceId)
                    {
                        flag = true;
                        if (!checkIfJobCanBeRemoved || this.CheckJobCanBeRemoved(job, "InstanceId", RemotingErrorIdStrings.JobWithSpecifiedInstanceIdNotCompleted, new object[] { job.Id, job.InstanceId }))
                        {
                            if (writeobject)
                            {
                                base.WriteObject(job);
                            }
                            else
                            {
                                matches.Add(job);
                            }
                            break;
                        }
                    }
                }
            }
            if (!flag && recurse)
            {
                foreach (Job job2 in jobsToSearch)
                {
                    if ((job2.ChildJobs != null) && (job2.ChildJobs.Count > 0))
                    {
                        flag = this.FindJobsMatchingByInstanceIdHelper(matches, job2.ChildJobs, instanceId, duplicateDetector, recurse, writeobject, checkIfJobCanBeRemoved);
                        if (flag)
                        {
                            return flag;
                        }
                    }
                }
            }
            return flag;
        }

        internal List<Job> FindJobsMatchingByName(bool recurse, bool writeobject, bool writeErrorOnNoMatch, bool checkIfJobCanBeRemoved)
        {
            List<Job> matches = new List<Job>();
            Hashtable duplicateDetector = new Hashtable();
            if (this.names != null)
            {
                foreach (string str in this.names)
                {
                    if (!string.IsNullOrEmpty(str))
                    {
                        bool flag = false;
                        duplicateDetector.Clear();
                        flag = this.FindJobsMatchingByNameHelper(matches, base.JobRepository.Jobs, str, duplicateDetector, recurse, writeobject, checkIfJobCanBeRemoved);
                        List<Job2> list2 = base.JobManager.GetJobsByName(str, this, false, writeobject, recurse, null);
                        bool flag2 = (list2 != null) && (list2.Count > 0);
                        if (flag2)
                        {
                            foreach (Job2 job in list2)
                            {
                                if (this.CheckIfJob2CanBeRemoved(checkIfJobCanBeRemoved, "Name", job, RemotingErrorIdStrings.JobWithSpecifiedNameNotCompleted, new object[] { job.Id, job.Name }))
                                {
                                    matches.Add(job);
                                }
                            }
                        }
                        if ((!(flag || flag2) && writeErrorOnNoMatch) && !WildcardPattern.ContainsWildcardCharacters(str))
                        {
                            Exception exception = PSTraceSource.NewArgumentException("Name", "RemotingErrorIdStrings", PSRemotingErrorId.JobWithSpecifiedNameNotFound.ToString(), new object[] { str });
                            base.WriteError(new ErrorRecord(exception, "JobWithSpecifiedNameNotFound", ErrorCategory.ObjectNotFound, str));
                        }
                    }
                }
            }
            return matches;
        }

        private bool FindJobsMatchingByNameHelper(List<Job> matches, IList<Job> jobsToSearch, string name, Hashtable duplicateDetector, bool recurse, bool writeobject, bool checkIfJobCanBeRemoved)
        {
            bool flag = false;
            WildcardPattern pattern = new WildcardPattern(name, WildcardOptions.IgnoreCase | WildcardOptions.Compiled);
            foreach (Job job in jobsToSearch)
            {
                if (!duplicateDetector.ContainsKey(job.Id))
                {
                    duplicateDetector.Add(job.Id, job.Id);
                    if (pattern.IsMatch(job.Name))
                    {
                        flag = true;
                        if (!checkIfJobCanBeRemoved || this.CheckJobCanBeRemoved(job, "Name", RemotingErrorIdStrings.JobWithSpecifiedNameNotCompleted, new object[] { job.Id, job.Name }))
                        {
                            if (writeobject)
                            {
                                base.WriteObject(job);
                            }
                            else
                            {
                                matches.Add(job);
                            }
                        }
                    }
                    if (((job.ChildJobs != null) && (job.ChildJobs.Count > 0)) && (recurse && this.FindJobsMatchingByNameHelper(matches, job.ChildJobs, name, duplicateDetector, recurse, writeobject, checkIfJobCanBeRemoved)))
                    {
                        flag = true;
                    }
                }
            }
            return flag;
        }

        internal List<Job> FindJobsMatchingBySessionId(bool recurse, bool writeobject, bool writeErrorOnNoMatch, bool checkIfJobCanBeRemoved)
        {
            List<Job> matches = new List<Job>();
            if (this.sessionIds != null)
            {
                Hashtable duplicateDetector = new Hashtable();
                foreach (int num in this.sessionIds)
                {
                    bool flag = this.FindJobsMatchingBySessionIdHelper(matches, base.JobRepository.Jobs, num, duplicateDetector, recurse, writeobject, checkIfJobCanBeRemoved);
                    Job2 job = base.JobManager.GetJobById(num, this, false, writeobject, recurse);
                    bool flag2 = job != null;
                    if (flag2 && this.CheckIfJob2CanBeRemoved(checkIfJobCanBeRemoved, "SessionId", job, RemotingErrorIdStrings.JobWithSpecifiedSessionIdNotCompleted, new object[] { job.Id }))
                    {
                        matches.Add(job);
                    }
                    if (!(flag || flag2) && writeErrorOnNoMatch)
                    {
                        Exception exception = PSTraceSource.NewArgumentException("SessionId", "RemotingErrorIdStrings", PSRemotingErrorId.JobWithSpecifiedSessionIdNotFound.ToString(), new object[] { num });
                        base.WriteError(new ErrorRecord(exception, "JobWithSpecifiedSessionNotFound", ErrorCategory.ObjectNotFound, num));
                    }
                }
            }
            return matches;
        }

        private bool FindJobsMatchingBySessionIdHelper(List<Job> matches, IList<Job> jobsToSearch, int sessionId, Hashtable duplicateDetector, bool recurse, bool writeobject, bool checkIfJobCanBeRemoved)
        {
            bool flag = false;
            foreach (Job job in jobsToSearch)
            {
                if (job.Id == sessionId)
                {
                    flag = true;
                    if (!checkIfJobCanBeRemoved || this.CheckJobCanBeRemoved(job, "SessionId", RemotingErrorIdStrings.JobWithSpecifiedSessionIdNotCompleted, new object[] { job.Id }))
                    {
                        if (writeobject)
                        {
                            base.WriteObject(job);
                        }
                        else
                        {
                            matches.Add(job);
                        }
                        break;
                    }
                }
            }
            if (!flag && recurse)
            {
                foreach (Job job2 in jobsToSearch)
                {
                    if ((job2.ChildJobs != null) && (job2.ChildJobs.Count > 0))
                    {
                        flag = this.FindJobsMatchingBySessionIdHelper(matches, job2.ChildJobs, sessionId, duplicateDetector, recurse, writeobject, checkIfJobCanBeRemoved);
                        if (flag)
                        {
                            return flag;
                        }
                    }
                }
            }
            return flag;
        }

        internal List<Job> FindJobsMatchingByState(bool writeobject)
        {
            List<Job> list = new List<Job>();
            List<Job> list2 = new List<Job>();
            list2.AddRange(base.JobRepository.Jobs);
            List<Job2> list3 = base.JobManager.GetJobsByState(this.jobstate, this, false, false, false, null);
            if (list3 != null)
            {
                foreach (Job2 job in list3)
                {
                    list2.Add(job);
                }
            }
            foreach (Job job2 in list2)
            {
                if (job2.JobStateInfo.State == this.jobstate)
                {
                    if (writeobject)
                    {
                        base.WriteObject(job2);
                    }
                    else
                    {
                        list.Add(job2);
                    }
                }
            }
            return list;
        }

        [ValidateNotNullOrEmpty, Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="CommandParameterSet")]
        public virtual string[] Command
        {
            get
            {
                return this.commands;
            }
            set
            {
                this.commands = value;
            }
        }

        [ValidateNotNullOrEmpty, Parameter(Mandatory=true, Position=0, ValueFromPipelineByPropertyName=true, ParameterSetName="FilterParameterSet")]
        public virtual Hashtable Filter
        {
            get
            {
                return this.filter;
            }
            set
            {
                this.filter = value;
            }
        }

        [Parameter(ValueFromPipelineByPropertyName=true, Position=0, Mandatory=true, ParameterSetName="SessionIdParameterSet"), ValidateNotNullOrEmpty]
        public virtual int[] Id
        {
            get
            {
                return this.sessionIds;
            }
            set
            {
                this.sessionIds = value;
            }
        }

        [ValidateNotNullOrEmpty, Parameter(ValueFromPipelineByPropertyName=true, Position=0, Mandatory=true, ParameterSetName="InstanceIdParameterSet")]
        public Guid[] InstanceId
        {
            get
            {
                return this.instanceIds;
            }
            set
            {
                this.instanceIds = value;
            }
        }

        [ValidateNotNullOrEmpty, Parameter(ValueFromPipelineByPropertyName=true, Position=0, Mandatory=true, ParameterSetName="NameParameterSet")]
        public string[] Name
        {
            get
            {
                return this.names;
            }
            set
            {
                this.names = value;
            }
        }

        [Parameter(Mandatory=true, Position=0, ValueFromPipelineByPropertyName=true, ParameterSetName="StateParameterSet")]
        public virtual JobState State
        {
            get
            {
                return this.jobstate;
            }
            set
            {
                this.jobstate = value;
            }
        }
    }
}

