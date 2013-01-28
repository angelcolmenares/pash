namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public abstract class JobSourceAdapter
    {
        private string _name = string.Empty;

        protected JobSourceAdapter()
        {
        }

        public abstract Job2 GetJobByInstanceId(Guid instanceId, bool recurse);
        public abstract Job2 GetJobBySessionId(int id, bool recurse);
        public abstract IList<Job2> GetJobs();
        public abstract IList<Job2> GetJobsByCommand(string command, bool recurse);
        public abstract IList<Job2> GetJobsByFilter(Dictionary<string, object> filter, bool recurse);
        public abstract IList<Job2> GetJobsByName(string name, bool recurse);
        public abstract IList<Job2> GetJobsByState(JobState state, bool recurse);
        public Job2 NewJob(JobDefinition definition)
        {
            return this.NewJob(new JobInvocationInfo(definition, new Dictionary<string, object>()));
        }

        public abstract Job2 NewJob(JobInvocationInfo specification);
        public virtual Job2 NewJob(string definitionName, string definitionPath)
        {
            return null;
        }

        public virtual void PersistJob(Job2 job)
        {
        }

        public abstract void RemoveJob(Job2 job);
        protected JobIdentifier RetrieveJobIdForReuse(Guid instanceId)
        {
            return JobManager.GetJobIdentifier(instanceId, base.GetType().Name);
        }

        public void StoreJobIdForReuse(Job2 job, bool recurse)
        {
            if (job == null)
            {
                PSTraceSource.NewArgumentNullException("job", "remotingerroridstrings", "JobSourceAdapterCannotSaveNullJob", new object[0]);
            }
            JobManager.SaveJobId(job.InstanceId, job.Id, base.GetType().Name);
            if ((recurse && (job.ChildJobs != null)) && (job.ChildJobs.Count > 0))
            {
                Hashtable duplicateDetector = new Hashtable();
                duplicateDetector.Add(job.InstanceId, job.InstanceId);
                foreach (Job job2 in job.ChildJobs)
                {
                    Job2 job3 = job2 as Job2;
                    if (job3 != null)
                    {
                        this.StoreJobIdForReuseHelper(duplicateDetector, job3, true);
                    }
                }
            }
        }

        private void StoreJobIdForReuseHelper(Hashtable duplicateDetector, Job2 job, bool recurse)
        {
            if (!duplicateDetector.ContainsKey(job.InstanceId))
            {
                duplicateDetector.Add(job.InstanceId, job.InstanceId);
                JobManager.SaveJobId(job.InstanceId, job.Id, base.GetType().Name);
                if (recurse && (job.ChildJobs != null))
                {
                    foreach (Job job2 in job.ChildJobs)
                    {
                        Job2 job3 = job2 as Job2;
                        if (job3 != null)
                        {
                            this.StoreJobIdForReuseHelper(duplicateDetector, job3, recurse);
                        }
                    }
                }
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }
    }
}

