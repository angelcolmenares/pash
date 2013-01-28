namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;

    [Cmdlet("Get", "Job", DefaultParameterSetName="SessionIdParameterSet", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113328"), OutputType(new Type[] { typeof(Job) })]
    public class GetJobCommand : JobCmdletBase
    {
        private DateTime _afterTime;
        private DateTime _beforeTime;
        private JobState _childJobState;
        private bool _hasMoreData;
        private SwitchParameter _includeChildJob;
        private int _newestCount;

        private List<Job> ApplyHasMoreDataFiltering(List<Job> jobList)
        {
            if (!base.MyInvocation.BoundParameters.ContainsKey("HasMoreData"))
            {
                return jobList;
            }
            List<Job> list = new List<Job>();
            foreach (Job job in jobList)
            {
                if (job.HasMoreData == this._hasMoreData)
                {
                    list.Add(job);
                }
            }
            return list;
        }

        private List<Job> ApplyTimeFiltering(List<Job> jobList)
        {
            List<Job> list;
            bool flag = base.MyInvocation.BoundParameters.ContainsKey("Before");
            bool flag2 = base.MyInvocation.BoundParameters.ContainsKey("After");
            bool flag3 = base.MyInvocation.BoundParameters.ContainsKey("Newest");
            if ((!flag && !flag2) && !flag3)
            {
                return jobList;
            }
            if (flag || flag2)
            {
                list = new List<Job>();
                foreach (Job job in jobList)
                {
                    if (job.PSEndTime == DateTime.MinValue)
                    {
                        continue;
                    }
                    if (flag && flag2)
                    {
                        DateTime? nullable2 = job.PSEndTime;
                        DateTime time2 = this._beforeTime;
                        if (nullable2.HasValue ? (nullable2.GetValueOrDefault() < time2) : false)
                        {
                            DateTime? nullable3 = job.PSEndTime;
                            DateTime time3 = this._afterTime;
                            if (nullable3.HasValue ? (nullable3.GetValueOrDefault() > time3) : false)
                            {
                                list.Add(job);
                            }
                        }
                        continue;
                    }
                    if (flag)
                    {
                        DateTime? nullable4 = job.PSEndTime;
                        DateTime time4 = this._beforeTime;
                        if (nullable4.HasValue ? (nullable4.GetValueOrDefault() < time4) : false)
                        {
                            goto Label_016E;
                        }
                    }
                    if (!flag2)
                    {
                        continue;
                    }
                    DateTime? nullable5 = job.PSEndTime;
                    DateTime time5 = this._afterTime;
                    if (!(nullable5.HasValue ? (nullable5.GetValueOrDefault() > time5) : false))
                    {
                        continue;
                    }
                Label_016E:
                    list.Add(job);
                }
            }
            else
            {
                list = jobList;
            }
            if (!flag3 || (list.Count == 0))
            {
                return list;
            }
            list.Sort(delegate (Job firstJob, Job secondJob) {
                DateTime? pSEndTime = firstJob.PSEndTime;
                DateTime? nullable2 = secondJob.PSEndTime;
                if ((pSEndTime.HasValue & nullable2.HasValue) ? ( (pSEndTime.GetValueOrDefault() > nullable2.GetValueOrDefault())) : (false))
                {
                    return -1;
                }
                DateTime? nullable3 = firstJob.PSEndTime;
                DateTime? nullable4 = secondJob.PSEndTime;
                if ((nullable3.HasValue & nullable4.HasValue) ? ( (nullable3.GetValueOrDefault() < nullable4.GetValueOrDefault())) : (false))
                {
                    return 1;
                }
                return 0;
            });
            List<Job> list2 = new List<Job>();
            int num = 0;
            foreach (Job job2 in list)
            {
                if (++num > this._newestCount)
                {
                    return list2;
                }
                if (!list2.Contains(job2))
                {
                    list2.Add(job2);
                }
            }
            return list2;
        }

        private List<Job> FindChildJobs(List<Job> jobList)
        {
            bool flag = base.MyInvocation.BoundParameters.ContainsKey("ChildJobState");
            bool flag2 = base.MyInvocation.BoundParameters.ContainsKey("IncludeChildJob");
            List<Job> list = new List<Job>();
            if (flag || flag2)
            {
                if (!flag && flag2)
                {
                    foreach (Job job in jobList)
                    {
                        if ((job.ChildJobs != null) && (job.ChildJobs.Count > 0))
                        {
                            list.AddRange(job.ChildJobs);
                        }
                    }
                    return list;
                }
                foreach (Job job2 in jobList)
                {
                    foreach (Job job3 in job2.ChildJobs)
                    {
                        if (job3.JobStateInfo.State == this._childJobState)
                        {
                            list.Add(job3);
                        }
                    }
                }
            }
            return list;
        }

        protected List<Job> FindJobs()
        {
            List<Job> jobList = new List<Job>();
            switch (base.ParameterSetName)
            {
                case "NameParameterSet":
                    jobList.AddRange(base.FindJobsMatchingByName(true, false, true, false));
                    break;

                case "InstanceIdParameterSet":
                    jobList.AddRange(base.FindJobsMatchingByInstanceId(true, false, true, false));
                    break;

                case "SessionIdParameterSet":
                    if (this.Id == null)
                    {
                        jobList.AddRange(base.JobRepository.Jobs);
                        jobList.AddRange(base.JobManager.GetJobs(this, true, false, null));
                        break;
                    }
                    jobList.AddRange(base.FindJobsMatchingBySessionId(true, false, true, false));
                    break;

                case "CommandParameterSet":
                    jobList.AddRange(base.FindJobsMatchingByCommand(false));
                    break;

                case "StateParameterSet":
                    jobList.AddRange(base.FindJobsMatchingByState(false));
                    break;

                case "FilterParameterSet":
                    jobList.AddRange(base.FindJobsMatchingByFilter(false));
                    break;
            }
            jobList.AddRange(this.FindChildJobs(jobList));
            jobList = this.ApplyHasMoreDataFiltering(jobList);
            return this.ApplyTimeFiltering(jobList);
        }

        protected override void ProcessRecord()
        {
            List<Job> sendToPipeline = this.FindJobs();
            sendToPipeline.Sort(delegate (Job x, Job y) {
                if (x == null)
                {
                    return -1;
                }
                return x.Id.CompareTo((y != null) ? y.Id : 1);
            });
            base.WriteObject(sendToPipeline, true);
        }

        [Parameter(ParameterSetName="CommandParameterSet"), Parameter(ParameterSetName="SessionIdParameterSet"), Parameter(ParameterSetName="InstanceIdParameterSet"), Parameter(ParameterSetName="NameParameterSet"), Parameter(ParameterSetName="StateParameterSet")]
        public DateTime After
        {
            get
            {
                return this._afterTime;
            }
            set
            {
                this._afterTime = value;
            }
        }

        [Parameter(ParameterSetName="NameParameterSet"), Parameter(ParameterSetName="SessionIdParameterSet"), Parameter(ParameterSetName="InstanceIdParameterSet"), Parameter(ParameterSetName="CommandParameterSet"), Parameter(ParameterSetName="StateParameterSet")]
        public DateTime Before
        {
            get
            {
                return this._beforeTime;
            }
            set
            {
                this._beforeTime = value;
            }
        }

        [Parameter(ParameterSetName="StateParameterSet"), Parameter(ParameterSetName="InstanceIdParameterSet"), Parameter(ParameterSetName="NameParameterSet"), Parameter(ParameterSetName="CommandParameterSet"), Parameter(ParameterSetName="SessionIdParameterSet")]
        public JobState ChildJobState
        {
            get
            {
                return this._childJobState;
            }
            set
            {
                this._childJobState = value;
            }
        }

        [Parameter(ParameterSetName="InstanceIdParameterSet"), Parameter(ParameterSetName="NameParameterSet"), Parameter(ParameterSetName="CommandParameterSet"), Parameter(ParameterSetName="SessionIdParameterSet"), Parameter(ParameterSetName="StateParameterSet")]
        public bool HasMoreData
        {
            get
            {
                return this._hasMoreData;
            }
            set
            {
                this._hasMoreData = value;
            }
        }

        [ValidateNotNullOrEmpty, Parameter(ValueFromPipelineByPropertyName=true, Position=0, ParameterSetName="SessionIdParameterSet")]
        public override int[] Id
        {
            get
            {
                return base.Id;
            }
            set
            {
                base.Id = value;
            }
        }

        [Parameter(ParameterSetName="StateParameterSet"), Parameter(ParameterSetName="CommandParameterSet"), Parameter(ParameterSetName="NameParameterSet"), Parameter(ParameterSetName="SessionIdParameterSet"), Parameter(ParameterSetName="InstanceIdParameterSet")]
        public SwitchParameter IncludeChildJob
        {
            get
            {
                return this._includeChildJob;
            }
            set
            {
                this._includeChildJob = value;
            }
        }

        [Parameter(ParameterSetName="CommandParameterSet"), Parameter(ParameterSetName="NameParameterSet"), Parameter(ParameterSetName="StateParameterSet"), Parameter(ParameterSetName="SessionIdParameterSet"), Parameter(ParameterSetName="InstanceIdParameterSet")]
        public int Newest
        {
            get
            {
                return this._newestCount;
            }
            set
            {
                this._newestCount = value;
            }
        }
    }
}

