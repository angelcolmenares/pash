using System;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Text;

namespace Microsoft.PowerShell.Commands
{
	internal class PSWmiJob : Job
	{
		private const string WMIJobType = "WmiJob";

		private bool atleastOneChildJobFailed;

		private int finishedChildJobsCount;

		private int blockedChildJobsCount;

		private bool _stopIsCalled;

		private string statusMessage;

		private bool moreData;

		private bool isDisposed;

		private ThrottleManager throttleManager;

		private object syncObject;

		public override bool HasMoreData
		{
			get
			{
				bool flag = false;
				int num = 0;
				while (num < base.ChildJobs.Count)
				{
					if (!base.ChildJobs[num].HasMoreData)
					{
						num++;
					}
					else
					{
						flag = true;
						break;
					}
				}
				this.moreData = flag;
				return this.moreData;
			}
		}

		public override string Location
		{
			get
			{
				return this.ConstructLocation();
			}
		}

		public override string StatusMessage
		{
			get
			{
				return this.statusMessage;
			}
		}

		internal PSWmiJob(Cmdlet cmds, string[] computerName, int throttleLimt, string command) : base(command, null)
		{
			this.throttleManager = new ThrottleManager();
			this.syncObject = new object();
			base.PSJobTypeName = "WmiJob";
			this.throttleManager.ThrottleLimit = throttleLimt;
			for (int i = 0; i < (int)computerName.Length; i++)
			{
				PSWmiChildJob pSWmiChildJob = new PSWmiChildJob(cmds, computerName[i], this.throttleManager);
				pSWmiChildJob.StateChanged += new EventHandler<JobStateEventArgs>(this.HandleChildJobStateChanged);
				pSWmiChildJob.JobUnblocked += new EventHandler(this.HandleJobUnblocked);
				base.ChildJobs.Add(pSWmiChildJob);
			}
			this.CommonInit(throttleLimt);
		}

		internal PSWmiJob(Cmdlet cmds, string[] computerName, int throttleLimit, string command, int count) : base(command, null)
		{
			this.throttleManager = new ThrottleManager();
			this.syncObject = new object();
			base.PSJobTypeName = "WmiJob";
			this.throttleManager.ThrottleLimit = throttleLimit;
			for (int i = 0; i < (int)computerName.Length; i++)
			{
				PSWmiChildJob pSWmiChildJob = new PSWmiChildJob(cmds, computerName[i], this.throttleManager, count);
				pSWmiChildJob.StateChanged += new EventHandler<JobStateEventArgs>(this.HandleChildJobStateChanged);
				pSWmiChildJob.JobUnblocked += new EventHandler(this.HandleJobUnblocked);
				base.ChildJobs.Add(pSWmiChildJob);
			}
			this.CommonInit(throttleLimit);
		}

		private void CommonInit(int throttleLimit)
		{
			base.CloseAllStreams();
			base.SetJobState(JobState.Running);
			this.throttleManager.EndSubmitOperations();
		}

		private string ConstructLocation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (PSWmiChildJob childJob in base.ChildJobs)
			{
				stringBuilder.Append(childJob.Location);
				stringBuilder.Append(",");
			}
			stringBuilder.Remove(stringBuilder.Length - 1, 1);
			return stringBuilder.ToString();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !this.isDisposed)
			{
				this.isDisposed = true;
				try
				{
					if (!base.IsFinishedState(base.JobStateInfo.State))
					{
						this.StopJob();
					}
					this.throttleManager.Dispose();
					foreach (Job childJob in base.ChildJobs)
					{
						childJob.Dispose();
					}
				}
				finally
				{
					base.Dispose(disposing);
				}
			}
		}

		private void HandleChildJobStateChanged(object sender, JobStateEventArgs e)
		{
			if (e.JobStateInfo.State != JobState.Blocked)
			{
				if (!base.IsFinishedState(e.JobStateInfo.State) || e.JobStateInfo.State == JobState.NotStarted)
				{
					return;
				}
				else
				{
					if (e.JobStateInfo.State == JobState.Failed)
					{
						this.atleastOneChildJobFailed = true;
					}
					bool flag = false;
					lock (this.syncObject)
					{
						PSWmiJob pSWmiJob = this;
						pSWmiJob.finishedChildJobsCount = pSWmiJob.finishedChildJobsCount + 1;
						if (this.finishedChildJobsCount == base.ChildJobs.Count)
						{
						}
					}
					if (flag)
					{
						if (!this.atleastOneChildJobFailed)
						{
							if (!this._stopIsCalled)
							{
								base.SetJobState(JobState.Completed);
							}
							else
							{
								base.SetJobState(JobState.Stopped);
								return;
							}
						}
						else
						{
							base.SetJobState(JobState.Failed);
							return;
						}
					}
					return;
				}
			}
			else
			{
				lock (this.syncObject)
				{
					PSWmiJob pSWmiJob1 = this;
					pSWmiJob1.blockedChildJobsCount = pSWmiJob1.blockedChildJobsCount + 1;
				}
				base.SetJobState(JobState.Blocked, null);
				return;
			}
		}

		private void HandleJobUnblocked(object sender, EventArgs eventArgs)
		{
			bool flag = false;
			lock (this.syncObject)
			{
				PSWmiJob pSWmiJob = this;
				pSWmiJob.blockedChildJobsCount = pSWmiJob.blockedChildJobsCount - 1;
				if (this.blockedChildJobsCount == 0)
				{
				}
			}
			if (flag)
			{
				base.SetJobState(JobState.Running, null);
			}
		}

		private void SetStatusMessage()
		{
			this.statusMessage = "test";
		}

		public override void StopJob()
		{
			if (!base.IsFinishedState(base.JobStateInfo.State))
			{
				this._stopIsCalled = true;
				this.throttleManager.StopAllOperations();
				base.Finished.WaitOne();
			}
		}
	}
}