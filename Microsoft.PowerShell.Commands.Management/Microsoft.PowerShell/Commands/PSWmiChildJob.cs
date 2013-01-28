using System;
using System.Collections;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Remoting;

namespace Microsoft.PowerShell.Commands
{
	internal class PSWmiChildJob : Job
	{
		private string computerName;

		private WmiAsyncCmdletHelper helper;

		private ThrottleManager throttleManager;

		private object syncObject;

		private int sinkCompleted;

		private bool bJobFailed;

		private bool bAtLeastOneObject;

		private ArrayList wmiSinkArray;

		private string statusMessage;

		private bool isDisposed;

		public override bool HasMoreData
		{
			get
			{
				if (base.Results.IsOpen)
				{
					return true;
				}
				else
				{
					return base.Results.Count > 0;
				}
			}
		}

		public override string Location
		{
			get
			{
				return this.computerName;
			}
		}

		public override string StatusMessage
		{
			get
			{
				return this.statusMessage;
			}
		}

		internal PSWmiChildJob(Cmdlet cmds, string computerName, ThrottleManager throttleManager) : base(null, null)
		{
			this.syncObject = new object();
			this.statusMessage = "test";
			base.UsesResultsCollection = true;
			this.computerName = computerName;
			this.throttleManager = throttleManager;
			this.wmiSinkArray = new ArrayList();
			ManagementOperationObserver managementOperationObserver = new ManagementOperationObserver();
			this.wmiSinkArray.Add(managementOperationObserver);
			PSWmiChildJob pSWmiChildJob = this;
			pSWmiChildJob.sinkCompleted = pSWmiChildJob.sinkCompleted + 1;
			managementOperationObserver.ObjectReady += new ObjectReadyEventHandler(this.NewObject);
			managementOperationObserver.Completed += new CompletedEventHandler(this.JobDone);
			this.helper = new WmiAsyncCmdletHelper(this, cmds, computerName, managementOperationObserver);
			this.helper.WmiOperationState += new EventHandler<WmiJobStateEventArgs>(this.HandleWMIState);
			this.helper.ShutdownComplete += new EventHandler<EventArgs>(this.JobDoneForWin32Shutdown);
			base.SetJobState(JobState.NotStarted);
			IThrottleOperation throttleOperation = this.helper;
			throttleOperation.OperationComplete += new EventHandler<OperationStateEventArgs>(this.HandleOperationComplete);
			throttleManager.ThrottleComplete += new EventHandler<EventArgs>(this.HandleThrottleComplete);
			throttleManager.AddOperation(throttleOperation);
		}

		internal PSWmiChildJob(Cmdlet cmds, string computerName, ThrottleManager throttleManager, int count) : base(null, null)
		{
			this.syncObject = new object();
			this.statusMessage = "test";
			base.UsesResultsCollection = true;
			this.computerName = computerName;
			this.throttleManager = throttleManager;
			this.wmiSinkArray = new ArrayList();
			ManagementOperationObserver managementOperationObserver = new ManagementOperationObserver();
			this.wmiSinkArray.Add(managementOperationObserver);
			PSWmiChildJob pSWmiChildJob = this;
			pSWmiChildJob.sinkCompleted = pSWmiChildJob.sinkCompleted + count;
			managementOperationObserver.ObjectReady += new ObjectReadyEventHandler(this.NewObject);
			managementOperationObserver.Completed += new CompletedEventHandler(this.JobDone);
			this.helper = new WmiAsyncCmdletHelper(this, cmds, computerName, managementOperationObserver, count);
			this.helper.WmiOperationState += new EventHandler<WmiJobStateEventArgs>(this.HandleWMIState);
			this.helper.ShutdownComplete += new EventHandler<EventArgs>(this.JobDoneForWin32Shutdown);
			base.SetJobState(JobState.NotStarted);
			IThrottleOperation throttleOperation = this.helper;
			throttleOperation.OperationComplete += new EventHandler<OperationStateEventArgs>(this.HandleOperationComplete);
			throttleManager.ThrottleComplete += new EventHandler<EventArgs>(this.HandleThrottleComplete);
			throttleManager.AddOperation(throttleOperation);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !this.isDisposed)
			{
				this.isDisposed = true;
				base.Dispose(disposing);
			}
		}

		internal ManagementOperationObserver GetNewSink()
		{
			ManagementOperationObserver managementOperationObserver = new ManagementOperationObserver();
			this.wmiSinkArray.Add(managementOperationObserver);
			lock (this.syncObject)
			{
				PSWmiChildJob pSWmiChildJob = this;
				pSWmiChildJob.sinkCompleted = pSWmiChildJob.sinkCompleted + 1;
			}
			managementOperationObserver.ObjectReady += new ObjectReadyEventHandler(this.NewObject);
			managementOperationObserver.Completed += new CompletedEventHandler(this.JobDone);
			return managementOperationObserver;
		}

		private void HandleOperationComplete(object sender, OperationStateEventArgs stateEventArgs)
		{
			WmiAsyncCmdletHelper wmiAsyncCmdletHelper = (WmiAsyncCmdletHelper)sender;
			if (wmiAsyncCmdletHelper.State != WmiState.NotStarted)
			{
				if (wmiAsyncCmdletHelper.State != WmiState.Running)
				{
					if (wmiAsyncCmdletHelper.State != WmiState.Completed)
					{
						if (wmiAsyncCmdletHelper.State != WmiState.Failed)
						{
							base.SetJobState(JobState.Stopped, wmiAsyncCmdletHelper.InternalException);
							return;
						}
						else
						{
							base.SetJobState(JobState.Failed, wmiAsyncCmdletHelper.InternalException);
							return;
						}
					}
					else
					{
						base.SetJobState(JobState.Completed, wmiAsyncCmdletHelper.InternalException);
						return;
					}
				}
				else
				{
					base.SetJobState(JobState.Running, wmiAsyncCmdletHelper.InternalException);
					return;
				}
			}
			else
			{
				base.SetJobState(JobState.Stopped, wmiAsyncCmdletHelper.InternalException);
				return;
			}
		}

		private void HandleThrottleComplete(object sender, EventArgs eventArgs)
		{
			if (this.helper.State != WmiState.NotStarted)
			{
				if (this.helper.State != WmiState.Running)
				{
					if (this.helper.State != WmiState.Completed)
					{
						if (this.helper.State != WmiState.Failed)
						{
							base.SetJobState(JobState.Stopped, this.helper.InternalException);
							return;
						}
						else
						{
							base.SetJobState(JobState.Failed, this.helper.InternalException);
							return;
						}
					}
					else
					{
						base.SetJobState(JobState.Completed, this.helper.InternalException);
						return;
					}
				}
				else
				{
					base.SetJobState(JobState.Running, this.helper.InternalException);
					return;
				}
			}
			else
			{
				base.SetJobState(JobState.Stopped, this.helper.InternalException);
				return;
			}
		}

		private void HandleWMIState(object sender, WmiJobStateEventArgs stateEventArgs)
		{
			if (stateEventArgs.WmiState != WmiState.Running)
			{
				if (stateEventArgs.WmiState != WmiState.NotStarted)
				{
					if (stateEventArgs.WmiState != WmiState.Completed)
					{
						if (stateEventArgs.WmiState != WmiState.Failed)
						{
							base.SetJobState(JobState.Stopped, this.helper.InternalException);
							return;
						}
						else
						{
							base.SetJobState(JobState.Failed, this.helper.InternalException);
							return;
						}
					}
					else
					{
						base.SetJobState(JobState.Completed);
						return;
					}
				}
				else
				{
					base.SetJobState(JobState.NotStarted, this.helper.InternalException);
					return;
				}
			}
			else
			{
				base.SetJobState(JobState.Running, this.helper.InternalException);
				return;
			}
		}

		private void JobDone(object sender, CompletedEventArgs obj)
		{
			lock (this.syncObject)
			{
				PSWmiChildJob pSWmiChildJob = this;
				pSWmiChildJob.sinkCompleted = pSWmiChildJob.sinkCompleted - 1;
			}
			if (obj.Status != ManagementStatus.NoError)
			{
				this.bJobFailed = true;
			}
			if (this.sinkCompleted == 0)
			{
				this.helper.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
				if (this.bJobFailed)
				{
					this.helper.State = WmiState.Failed;
					base.SetJobState(JobState.Failed);
				}
				else
				{
					this.helper.State = WmiState.Completed;
					base.SetJobState(JobState.Completed);
					return;
				}
			}
		}

		private void JobDoneForWin32Shutdown(object sender, EventArgs arg)
		{
			lock (this.syncObject)
			{
				PSWmiChildJob pSWmiChildJob = this;
				pSWmiChildJob.sinkCompleted = pSWmiChildJob.sinkCompleted - 1;
			}
			if (this.sinkCompleted == 0)
			{
				this.helper.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
				this.helper.State = WmiState.Completed;
				base.SetJobState(JobState.Completed);
			}
		}

		private void NewObject(object sender, ObjectReadyEventArgs obj)
		{
			if (!this.bAtLeastOneObject)
			{
				this.bAtLeastOneObject = true;
			}
			this.WriteObject(obj.NewObject);
		}

		public override void StopJob()
		{
			base.AssertNotDisposed();
			this.throttleManager.StopOperation(this.helper);
			base.Finished.WaitOne();
		}

		internal void UnblockJob()
		{
			base.SetJobState(JobState.Running, null);
			this.JobUnblocked.SafeInvoke(this, EventArgs.Empty);
		}

		internal event EventHandler JobUnblocked;
	}
}