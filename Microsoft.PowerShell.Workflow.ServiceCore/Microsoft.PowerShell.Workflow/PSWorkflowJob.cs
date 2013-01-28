using Microsoft.PowerShell.Activities;
using System;
using System.Activities;
using System.Activities.Hosting;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Management.Automation;
using System.Management.Automation.PerformanceData;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Sqm;
using System.Management.Automation.Tracing;
using System.Threading;

namespace Microsoft.PowerShell.Workflow
{
	public class PSWorkflowJob : Job2
	{
		private const string ClassNameTrace = "PSWorkflowJob";

		private readonly PowerShellTraceSource _tracer;

		private readonly static Tracer StructuredTracer;

		private readonly object _syncObject;

		private readonly object _resumeErrorSyncObject;

		private bool _isDisposed;

		private string _statusMessage;

		private string _location;

		private readonly WorkflowJobDefinition _definition;

		private readonly PSWorkflowRuntime _runtime;

		private JobState _previousState;

		private PSWorkflowInstance _workflowInstance;

		private readonly static PSPerfCountersMgr _perfCountersMgr;

		private readonly Dictionary<Guid, Exception> _resumeErrors;

		private Dictionary<string, object> _workflowParameters;

		private Dictionary<string, object> _psWorkflowCommonParameters;

		private Dictionary<string, object> _jobMetadata;

		private Dictionary<string, object> _privateMetadata;

		private PSDataCollection<PSObject> _inputCollection;

		private ManualResetEvent _jobRunning;

		private ManualResetEvent _jobSuspendedOrAborted;

		private bool _starting;

		private bool _stopCalled;

		private bool _suspending;

		private bool _resuming;

		internal bool? IsSuspendable;

		private bool _unloadStreamsOnPersistentState;

		private bool _hasMoreDataOnDisk;

		private bool wfSuspendInProgress;

		private List<string> listOfLabels;

		public override bool HasMoreData
		{
			get
			{
				if (this._hasMoreDataOnDisk || base.Output.IsOpen || base.Output.Count > 0 || base.Error.IsOpen || base.Error.Count > 0 || base.Verbose.IsOpen || base.Verbose.Count > 0 || base.Debug.IsOpen || base.Debug.Count > 0 || base.Warning.IsOpen || base.Warning.Count > 0 || base.Progress.IsOpen)
				{
					return true;
				}
				else
				{
					return base.Progress.Count > 0;
				}
			}
		}

		internal Dictionary<string, object> JobMetadata
		{
			get
			{
				return this._jobMetadata;
			}
			set
			{
				this._jobMetadata = value;
			}
		}

		private ManualResetEvent JobRunning
		{
			get
			{
				if (this._jobRunning == null)
				{
					lock (this._syncObject)
					{
						if (this._jobRunning == null)
						{
							this.AssertNotDisposed();
							this._jobRunning = new ManualResetEvent(false);
						}
					}
				}
				return this._jobRunning;
			}
		}

		private ManualResetEvent JobSuspendedOrAborted
		{
			get
			{
				if (this._jobSuspendedOrAborted == null)
				{
					lock (this._syncObject)
					{
						if (this._jobSuspendedOrAborted == null)
						{
							this.AssertNotDisposed();
							this._jobSuspendedOrAborted = new ManualResetEvent(false);
						}
					}
				}
				return this._jobSuspendedOrAborted;
			}
		}

		public override string Location
		{
			get
			{
				return this._location;
			}
		}

		public Action<PSWorkflowJob, ReadOnlyCollection<BookmarkInfo>> OnIdle
		{
			get;
			set;
		}

		public Func<PSWorkflowJob, ReadOnlyCollection<BookmarkInfo>, bool, PSPersistableIdleAction> OnPersistableIdleAction
		{
			get;
			set;
		}

		public Action<PSWorkflowJob> OnUnloaded
		{
			get;
			set;
		}

		internal Dictionary<string, object> PrivateMetadata
		{
			get
			{
				return this._privateMetadata;
			}
		}

		internal Dictionary<string, object> PSWorkflowCommonParameters
		{
			get
			{
				return this._psWorkflowCommonParameters;
			}
		}

		public PSWorkflowInstance PSWorkflowInstance
		{
			get
			{
				return this._workflowInstance;
			}
			internal set
			{
				this._workflowInstance = value;
			}
		}

		internal WaitHandle Running
		{
			get
			{
				return this.JobRunning;
			}
		}

		public override string StatusMessage
		{
			get
			{
				return this._statusMessage;
			}
		}

		internal WaitHandle SuspendedOrAborted
		{
			get
			{
				return this.JobSuspendedOrAborted;
			}
		}

		internal bool SynchronousExecution
		{
			get;
			set;
		}

		internal Guid WorkflowGuid
		{
			get
			{
				return this._workflowInstance.Id;
			}
		}

		private Guid WorkflowGuidForTraces
		{
			get
			{
				if (this._workflowInstance == null)
				{
					return Guid.Empty;
				}
				else
				{
					return this._workflowInstance.Id;
				}
			}
		}

		internal bool WorkflowInstanceLoaded
		{
			get;
			set;
		}

		internal Dictionary<string, object> WorkflowParameters
		{
			get
			{
				return this._workflowParameters;
			}
		}

		static PSWorkflowJob()
		{
			PSWorkflowJob.StructuredTracer = new Tracer();
			PSWorkflowJob._perfCountersMgr = PSPerfCountersMgr.Instance;
		}

		internal PSWorkflowJob(PSWorkflowRuntime runtime, JobInvocationInfo specification) : base(PSWorkflowJob.Validate(specification).Command)
		{
			this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
			this._syncObject = new object();
			this._resumeErrorSyncObject = new object();
			this._statusMessage = string.Empty;
			this._location = string.Empty;
			this._resumeErrors = new Dictionary<Guid, Exception>();
			this._workflowParameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			this._psWorkflowCommonParameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			this._jobMetadata = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			this._privateMetadata = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			this.IsSuspendable = null;
			this.listOfLabels = new List<string>();
			base.StartParameters = specification.Parameters;
			this._definition = WorkflowJobDefinition.AsWorkflowJobDefinition(specification.Definition);
			this._runtime = runtime;
			this.CommonInit();
		}

		internal PSWorkflowJob(PSWorkflowRuntime runtime, JobInvocationInfo specification, Guid JobInstanceId) : base(PSWorkflowJob.Validate(specification).Command, specification.Definition.Name, JobInstanceId)
		{
			this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
			this._syncObject = new object();
			this._resumeErrorSyncObject = new object();
			this._statusMessage = string.Empty;
			this._location = string.Empty;
			this._resumeErrors = new Dictionary<Guid, Exception>();
			this._workflowParameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			this._psWorkflowCommonParameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			this._jobMetadata = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			this._privateMetadata = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			this.IsSuspendable = null;
			this.listOfLabels = new List<string>();
			base.StartParameters = specification.Parameters;
			this._definition = WorkflowJobDefinition.AsWorkflowJobDefinition(specification.Definition);
			this._runtime = runtime;
			this.CommonInit();
		}

		internal PSWorkflowJob(PSWorkflowRuntime runtime, string command, string name) : base(command, name)
		{
			this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
			this._syncObject = new object();
			this._resumeErrorSyncObject = new object();
			this._statusMessage = string.Empty;
			this._location = string.Empty;
			this._resumeErrors = new Dictionary<Guid, Exception>();
			this._workflowParameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			this._psWorkflowCommonParameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			this._jobMetadata = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			this._privateMetadata = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			this.IsSuspendable = null;
			this.listOfLabels = new List<string>();
			this._runtime = runtime;
			this.CommonInit();
		}

		internal PSWorkflowJob(PSWorkflowRuntime runtime, string command, string name, JobIdentifier token) : base(command, name, token)
		{
			this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
			this._syncObject = new object();
			this._resumeErrorSyncObject = new object();
			this._statusMessage = string.Empty;
			this._location = string.Empty;
			this._resumeErrors = new Dictionary<Guid, Exception>();
			this._workflowParameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			this._psWorkflowCommonParameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			this._jobMetadata = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			this._privateMetadata = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			this.IsSuspendable = null;
			this.listOfLabels = new List<string>();
			this._runtime = runtime;
			this.CommonInit();
		}

		internal PSWorkflowJob(PSWorkflowRuntime runtime, string command, string name, Guid instanceId) : base(command, name, instanceId)
		{
			this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
			this._syncObject = new object();
			this._resumeErrorSyncObject = new object();
			this._statusMessage = string.Empty;
			this._location = string.Empty;
			this._resumeErrors = new Dictionary<Guid, Exception>();
			this._workflowParameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			this._psWorkflowCommonParameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			this._jobMetadata = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			this._privateMetadata = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			this.IsSuspendable = null;
			this.listOfLabels = new List<string>();
			this._runtime = runtime;
			this.CommonInit();
		}

		private void AssertNotDisposed()
		{
			if (!this._isDisposed)
			{
				return;
			}
			else
			{
				throw new ObjectDisposedException("PSWorkflowJob");
			}
		}

		private void AssertValidState(JobState expectedState)
		{
			this.AssertNotDisposed();
			lock (base.SyncRoot)
			{
				if (base.JobStateInfo.State != expectedState)
				{
					throw new InvalidJobStateException(base.JobStateInfo.State, Resources.JobCannotBeStarted);
				}
			}
		}

		internal bool CheckAndAddStateChangedEventHandler(EventHandler<JobStateEventArgs> handler, JobState expectedState)
		{
			bool flag;
			lock (base.SyncRoot)
			{
				if (base.JobStateInfo.State == expectedState || base.JobStateInfo.State == JobState.Running)
				{
					base.StateChanged += handler;
					flag = true;
				}
				else
				{
					flag = false;
				}
			}
			return flag;
		}

		private void CommonInit()
		{
			base.PSJobTypeName = "PSWorkflowJob";
			base.StateChanged += new EventHandler<JobStateEventArgs>(this.HandleMyStateChanged);
			this._tracer.WriteMessage("PSWorkflowJob", "CommonInit", this.WorkflowGuidForTraces, this, "Construction/initialization", new string[0]);
		}

		internal void ConfigureWorkflowHandlers()
		{
			this._workflowInstance.OnCompletedDelegate = new Action<object>(this.OnWorkflowCompleted);
			this._workflowInstance.OnSuspenedDelegate = new Action<object>(this.OnWorkflowSuspended);
			this._workflowInstance.OnStoppedDelegate = new Action<object>(this.OnWorkflowStopped);
			this._workflowInstance.OnAbortedDelegate = new Action<Exception, object>(this.OnWorkflowAborted);
			this._workflowInstance.OnFaultedDelegate = new Action<Exception, object>(this.OnWorkflowFaulted);
			this._workflowInstance.OnIdleDelegate = new Action<ReadOnlyCollection<BookmarkInfo>, object>(this.OnWorkflowIdle);
			this._workflowInstance.OnPersistableIdleActionDelegate = new Func<ReadOnlyCollection<BookmarkInfo>, bool, object, PSPersistableIdleAction>(this.OnWorkflowPersistableIdleAction);
			this._workflowInstance.OnUnloadedDelegate = new Action<object>(this.OnWorkflowUnloaded);
		}

		protected override void Dispose(bool disposing)
		{
			if (!this._isDisposed)
			{
				lock (base.SyncRoot)
				{
					if (!this._isDisposed)
					{
						this._isDisposed = true;
					}
					else
					{
						return;
					}
				}
				if (disposing)
				{
					try
					{
						if (this._workflowInstance != null)
						{
							this._workflowInstance.Dispose();
						}
						base.StateChanged -= new EventHandler<JobStateEventArgs>(this.HandleMyStateChanged);
						if (this._jobRunning != null)
						{
							this._jobRunning.Close();
						}
						if (this._jobSuspendedOrAborted != null)
						{
							this._jobSuspendedOrAborted.Dispose();
						}
						this._tracer.Dispose();
					}
					finally
					{
						base.Dispose(true);
					}
				}
				return;
			}
			else
			{
				return;
			}
		}

		internal bool DoAbortJob(string reason)
		{
			bool flag;
			if (!this._isDisposed)
			{
				bool flag1 = false;
				lock (base.SyncRoot)
				{
					if (!this._isDisposed)
					{
						if (this._isDisposed || base.JobStateInfo.State == JobState.Suspending || base.JobStateInfo.State == JobState.Suspended || this._suspending)
						{
							flag = false;
						}
						else
						{
							if (this._starting)
							{
								flag1 = true;
							}
							this._tracer.WriteMessage("PSWorkflowJob", "DoAbortJob", this.WorkflowGuidForTraces, this, "BEGIN", new string[0]);
							this._suspending = true;
							if (base.JobStateInfo.State == JobState.Running || base.JobStateInfo.State == JobState.NotStarted)
							{
								this.DoSetJobState(JobState.Suspending, null);
								lock (base.SyncRoot)
								{
									this._resuming = false;
								}
								if (flag1)
								{
									this.JobRunning.WaitOne();
								}
								this._workflowInstance.CheckForTerminalAction();
								this._workflowInstance.AbortInstance(reason);
								this._tracer.WriteMessage("PSWorkflowJob", "DoAbortJob", this.WorkflowGuidForTraces, this, "END", new string[0]);
								return true;
							}
							else
							{
								this._tracer.WriteMessage("PSWorkflowJob", "DoAbortJob", this.WorkflowGuidForTraces, this, "InvalidJobState", new string[0]);
								throw new InvalidJobStateException(base.JobStateInfo.State, Resources.SuspendNotValidState);
							}
						}
					}
					else
					{
						flag = false;
					}
				}
				return flag;
			}
			else
			{
				return false;
			}
		}

		private void DoLabeledResumeJob(string label)
		{
			bool flag = false;
			lock (base.SyncRoot)
			{
				if (!this._isDisposed)
				{
					this._tracer.WriteMessage("PSWorkflowJob", "DoLabeledResumeJob", this.WorkflowGuidForTraces, this, "BEGIN", new string[0]);
					if (this.wfSuspendInProgress)
					{
					}
					this.listOfLabels.Add(label);
				}
				else
				{
					return;
				}
			}
			try
			{
				if (flag)
				{
					this.JobSuspendedOrAborted.WaitOne();
				}
				lock (base.SyncRoot)
				{
					this.wfSuspendInProgress = false;
					if (base.JobStateInfo.State == JobState.Suspended || base.JobStateInfo.State == JobState.Running)
					{
						if (base.JobStateInfo.State != JobState.Running && !this._resuming)
						{
							this._workflowInstance.DoLoadInstanceForReactivation();
						}
						this._workflowInstance.ValidateIfLabelExists(label);
						this._resuming = true;
					}
					else
					{
						this._tracer.WriteMessage("PSWorkflowJob", "DoLabeledResumeJob", this.WorkflowGuidForTraces, this, "InvalidJobState", new string[0]);
						throw new InvalidJobStateException(base.JobStateInfo.State, Resources.ResumeNotValidState);
					}
				}
				base.LoadJobStreams();
				this.DoSetJobState(JobState.Running, null);
				PSWorkflowJob.StructuredTracer.WorkflowResuming(this._workflowInstance.Id);
				this._workflowInstance.ResumeInstance(label);
				lock (base.SyncRoot)
				{
					this._suspending = false;
				}
			}
			finally
			{
				lock (base.SyncRoot)
				{
					this.listOfLabels.Remove(label);
				}
			}
			PSWorkflowJob.StructuredTracer.WorkflowResumed(this._workflowInstance.Id);
			PSWorkflowJob._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 3, (long)1, true);
			PSWorkflowJob._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 4, (long)1, true);
			this._tracer.WriteMessage("PSWorkflowJob", "DoLabeledResumeJob", this.WorkflowGuidForTraces, this, "END", new string[0]);
		}

		protected override void DoLoadJobStreams()
		{
			lock (this._syncObject)
			{
				bool flag = this.IsFinishedState(base.JobStateInfo.State);
				this.InitializeWithWorkflow(this.PSWorkflowInstance, flag);
				this._hasMoreDataOnDisk = false;
			}
		}

		protected virtual void DoResumeBookmark(Bookmark bookmark, object state)
		{
			if (!this._isDisposed)
			{
				lock (base.SyncRoot)
				{
					if (!this._isDisposed)
					{
						if (this.IsFinishedState(base.JobStateInfo.State) || base.JobStateInfo.State == JobState.Stopping)
						{
							this._tracer.WriteMessage("PSWorkflowJob", "DoResumeBookmark", this.WorkflowGuidForTraces, this, "InvalidJobState to resume a bookmark", new string[0]);
							throw new InvalidJobStateException(base.JobStateInfo.State, Resources.ResumeNotValidState);
						}
					}
					else
					{
						return;
					}
				}
				this.DoSetJobState(JobState.Running, null);
				this._workflowInstance.ResumeBookmark(bookmark, state);
				return;
			}
			else
			{
				return;
			}
		}

		private void DoResumeJob(object state)
		{
			string empty;
			if (!this._isDisposed)
			{
				Tuple<string, ManualResetEvent> tuple = state as Tuple<string, ManualResetEvent>;
				if (tuple != null)
				{
					string item1 = tuple.Item1;
					empty = item1;
					if (item1 == null)
					{
						empty = string.Empty;
					}
				}
				else
				{
					empty = string.Empty;
				}
				string str = empty;
				try
				{
					if (string.IsNullOrEmpty(str))
					{
						lock (base.SyncRoot)
						{
							if (this._isDisposed || base.JobStateInfo.State == JobState.Running || this._resuming)
							{
								return;
							}
							else
							{
								this._tracer.WriteMessage("PSWorkflowJob", "DoResumeJob", this.WorkflowGuidForTraces, this, "BEGIN", new string[0]);
								if (base.JobStateInfo.State == JobState.Suspended)
								{
									this._resuming = true;
								}
								else
								{
									this._tracer.WriteMessage("PSWorkflowJob", "DoResumeJob", this.WorkflowGuidForTraces, this, "InvalidJobState", new string[0]);
									throw new InvalidJobStateException(base.JobStateInfo.State, Resources.ResumeNotValidState);
								}
							}
						}
						this._workflowInstance.DoLoadInstanceForReactivation();
						base.LoadJobStreams();
						this.DoSetJobState(JobState.Running, null);
						PSWorkflowJob.StructuredTracer.WorkflowResuming(this._workflowInstance.Id);
						this._workflowInstance.ResumeInstance(str);
						lock (base.SyncRoot)
						{
							this._suspending = false;
							this.wfSuspendInProgress = false;
						}
						PSWorkflowJob.StructuredTracer.WorkflowResumed(this._workflowInstance.Id);
						PSWorkflowJob._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 3, (long)1, true);
						PSWorkflowJob._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 4, (long)1, true);
						this._tracer.WriteMessage("PSWorkflowJob", "DoResumeJob", this.WorkflowGuidForTraces, this, "END", new string[0]);
					}
					else
					{
						this.DoLabeledResumeJob(str);
					}
				}
				finally
				{
					if (tuple != null && tuple.Item2 != null)
					{
						tuple.Item2.Set();
					}
				}
				return;
			}
			else
			{
				return;
			}
		}

		private void DoResumeJobAsync(object state)
		{
			this._tracer.WriteMessage("PSWorkflowJob", "DoResumeJobAsync", this.WorkflowGuidForTraces, this, "", new string[0]);
			string str = state as string;
			AsyncOperation asyncOperation = AsyncOperationManager.CreateOperation(null);
			PSWorkflowJob.JobActionWorkerDelegate jobActionWorkerDelegate = new PSWorkflowJob.JobActionWorkerDelegate(this.JobActionWorker);
			jobActionWorkerDelegate.BeginInvoke(asyncOperation, PSWorkflowJob.ActionType.Resume, string.Empty, str, null, null);
		}

		private void DoResumeJobCatchException(object state)
		{
			Tuple<object, Guid> tuple = state as Tuple<object, Guid>;
			try
			{
				this.DoResumeJob(tuple.Item1);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				lock (this._resumeErrorSyncObject)
				{
					if (!this._resumeErrors.ContainsKey(tuple.Item2))
					{
						this._resumeErrors.Add(tuple.Item2, exception);
					}
				}
			}
		}

		private bool DoSetJobState(JobState state, Exception reason = null)
		{
			bool flag;
			string str;
			if (this.IsFinishedState(this._previousState) || this._isDisposed)
			{
				return false;
			}
			else
			{
				lock (this._syncObject)
				{
					if (this.IsFinishedState(this._previousState) || this._isDisposed)
					{
						flag = false;
					}
					else
					{
						if (this._previousState != JobState.Stopping || state != JobState.Suspended)
						{
							if (state != this._previousState && PSWorkflowJob.StructuredTracer.IsEnabled)
							{
								PSWorkflowJob.StructuredTracer.JobStateChanged(base.Id, base.InstanceId, state.ToString(), this._previousState.ToString());
							}
							this._previousState = state;
							if (this._workflowInstance != null)
							{
								this._workflowInstance.PSWorkflowContext.JobMetadata.Remove("Reason");
								if (reason != null)
								{
									if (PSWorkflowJob.StructuredTracer.IsEnabled)
									{
										PSWorkflowJob.StructuredTracer.JobError(base.Id, base.InstanceId, Tracer.GetExceptionString(reason));
									}
									this._workflowInstance.PSWorkflowContext.JobMetadata.Add("Reason", reason);
								}
							}
							PowerShellTraceSource powerShellTraceSource = this._tracer;
							string str1 = "PSWorkflowJob";
							string str2 = "DoSetJobState";
							Guid workflowGuidForTraces = this.WorkflowGuidForTraces;
							PSWorkflowJob pSWorkflowJob = this;
							string str3 = "Setting state to {0}, Setting Reason to exception: {1}";
							string[] strArrays = new string[2];
							strArrays[0] = (object)state.ToString();
							string[] strArrays1 = strArrays;
							int num = 1;
							if (reason == null)
							{
								str = null;
							}
							else
							{
								str = reason.ToString();
							}
							strArrays1[num] = str;
							powerShellTraceSource.WriteMessage(str1, str2, workflowGuidForTraces, pSWorkflowJob, str3, strArrays);
							this._workflowInstance.State = state;
							base.SetJobState(state, reason);
							this._tracer.WriteMessage("PSWorkflowJob", "DoSetJobState", this.WorkflowGuidForTraces, this, "Done setting state", new string[0]);
							return true;
						}
						else
						{
							flag = false;
						}
					}
				}
				return flag;
			}
		}

		private void DoStartJobAsync(object state)
		{
			this._tracer.WriteMessage("PSWorkflowJob", "DoStartJobAsync", this.WorkflowGuidForTraces, this, "", new string[0]);
			AsyncOperation asyncOperation = AsyncOperationManager.CreateOperation(null);
			PSWorkflowJob.JobActionWorkerDelegate jobActionWorkerDelegate = new PSWorkflowJob.JobActionWorkerDelegate(this.JobActionWorker);
			jobActionWorkerDelegate.BeginInvoke(asyncOperation, PSWorkflowJob.ActionType.Start, string.Empty, string.Empty, null, null);
		}

		private void DoStartJobLogic(object state)
		{
			if (!this._isDisposed)
			{
				this._tracer.WriteMessage("PSWorkflowJob", "DoStartJobLogic", this.WorkflowGuidForTraces, this, "BEGIN", new string[0]);
				PSWorkflowJob.StructuredTracer.BeginJobLogic(base.InstanceId);
				lock (base.SyncRoot)
				{
					this.AssertValidState(JobState.NotStarted);
					if (this._starting || this._suspending || this._resuming)
					{
						return;
					}
					else
					{
						this._starting = true;
					}
				}
				this.DoSetJobState(JobState.Running, null);
				this._tracer.WriteMessage("PSWorkflowJob", "DoStartJobLogic", this.WorkflowGuidForTraces, this, "ready to start", new string[0]);
				this._workflowInstance.ExecuteInstance();
				PSWorkflowJob.StructuredTracer.WorkflowExecutionStarted(this._workflowInstance.Id, string.Empty);
				this._tracer.WriteMessage("PSWorkflowJob", "DoStartJobLogic", this.WorkflowGuidForTraces, this, "END", new string[0]);
				return;
			}
			else
			{
				return;
			}
		}

		private void DoStopJob()
		{
			if (this._isDisposed || this.IsFinishedState(base.JobStateInfo.State) || JobState.Stopping == base.JobStateInfo.State || this._stopCalled)
			{
				return;
			}
			else
			{
				bool flag = false;
				bool flag1 = false;
				bool flag2 = false;
				lock (base.SyncRoot)
				{
					if (this._isDisposed || this.IsFinishedState(base.JobStateInfo.State) || JobState.Stopping == base.JobStateInfo.State || this._stopCalled)
					{
						return;
					}
					else
					{
						if (!this._suspending)
						{
							if (this._starting || this._resuming)
							{
							}
						}
						else
						{
						}
						this._tracer.WriteMessage("PSWorkflowJob", "DoStopJob", this.WorkflowGuidForTraces, this, "BEGIN", new string[0]);
						this._stopCalled = true;
						if (base.JobStateInfo.State == JobState.Running)
						{
						}
					}
				}
				if (!flag)
				{
					this.DoSetJobState(JobState.Stopping, null);
					this._workflowInstance.State = JobState.Stopped;
					this._workflowInstance.PerformTaskAtTerminalState();
					this.DoSetJobState(JobState.Stopped, null);
				}
				else
				{
					if (!flag2)
					{
						if (flag1)
						{
							this.JobRunning.WaitOne();
						}
					}
					else
					{
						this.JobSuspendedOrAborted.WaitOne();
					}
					this.DoSetJobState(JobState.Stopping, null);
					PSWorkflowJob.StructuredTracer.CancellingWorkflowExecution(this._workflowInstance.Id);
					this._workflowInstance.StopInstance();
				}
				PSWorkflowJob.StructuredTracer.WorkflowExecutionCancelled(this._workflowInstance.Id);
				PSWorkflowJob._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 7, (long)1, true);
				PSWorkflowJob._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 8, (long)1, true);
				PSWorkflowJob._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 13, (long)1, true);
				PSWorkflowJob._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 14, (long)1, true);
				this._tracer.WriteMessage("PSWorkflowJob", "DoStopJob", this.WorkflowGuidForTraces, this, "END", new string[0]);
				return;
			}
		}

		internal bool DoSuspendJob()
		{
			bool flag;
			bool flag1 = true;
			if (!this._isDisposed)
			{
				bool flag2 = false;
				bool flag3 = false;
				lock (base.SyncRoot)
				{
					if (this._isDisposed || base.JobStateInfo.State == JobState.Suspending || base.JobStateInfo.State == JobState.Suspended || this._suspending)
					{
						flag = flag1;
					}
					else
					{
						if (!this.IsSuspendable.HasValue || !this.IsSuspendable.HasValue || this.IsSuspendable.Value)
						{
							if (this._starting)
							{
								flag2 = true;
							}
							this._tracer.WriteMessage("PSWorkflowJob", "DoSuspendJob", this.WorkflowGuidForTraces, this, "BEGIN", new string[0]);
							this._suspending = true;
							if (base.JobStateInfo.State == JobState.Running || base.JobStateInfo.State == JobState.NotStarted)
							{
								if (base.JobStateInfo.State == JobState.NotStarted)
								{
									flag3 = true;
								}
								if (flag2)
								{
									this.JobRunning.WaitOne();
								}
								this.DoSetJobState(JobState.Suspending, null);
								lock (base.SyncRoot)
								{
									this._resuming = false;
								}
								this._workflowInstance.SuspendInstance(flag3);
								this._tracer.WriteMessage("PSWorkflowJob", "DoSuspendJob", this.WorkflowGuidForTraces, this, "END", new string[0]);
								return flag1;
							}
							else
							{
								this._tracer.WriteMessage("PSWorkflowJob", "DoSuspendJob", this.WorkflowGuidForTraces, this, "InvalidJobState", new string[0]);
								throw new InvalidJobStateException(base.JobStateInfo.State, Resources.SuspendNotValidState);
							}
						}
						else
						{
							this._tracer.WriteMessage("PSWorkflowJob", "DoSuspendJob", this.WorkflowGuidForTraces, this, "The job is not suspendable.", new string[0]);
							throw new InvalidOperationException(Resources.ErrorMessageForPersistence);
						}
					}
				}
				return flag;
			}
			else
			{
				return flag1;
			}
		}

		private bool DoTerminateJob(string reason)
		{
			bool flag;
			if (!this._isDisposed)
			{
				bool flag1 = false;
				lock (base.SyncRoot)
				{
					if (!this._isDisposed)
					{
						this._tracer.WriteMessage("PSWorkflowJob", "DoTerminateJob", this.WorkflowGuidForTraces, this, "BEGIN", new string[0]);
						if (base.JobStateInfo.State != JobState.Running)
						{
							if (base.JobStateInfo.State == JobState.Suspended)
							{
								this._tracer.WriteMessage("Trying to load and terminate suspended workflow");
								this._workflowInstance.DoLoadInstanceForReactivation();
								this._workflowInstance.TerminateInstance(reason);
								flag1 = true;
							}
						}
						else
						{
							this._tracer.WriteMessage("trying to terminate running workflow job");
							this._workflowInstance.CheckForTerminalAction();
							this._workflowInstance.TerminateInstance(reason);
							flag1 = true;
						}
						this._tracer.WriteMessage("PSWorkflowJob", "DoTerminateJob", this.WorkflowGuidForTraces, this, "END", new string[0]);
						return flag1;
					}
					else
					{
						flag = false;
					}
				}
				return flag;
			}
			else
			{
				return false;
			}
		}

		protected override void DoUnloadJobStreams()
		{
			if (this._workflowInstance != null)
			{
				lock (this._syncObject)
				{
					if (this._workflowInstance != null)
					{
						if (this._workflowInstance.SaveStreamsIfNecessary())
						{
							this._hasMoreDataOnDisk = true;
							this._workflowInstance.DisposeStreams();
						}
					}
				}
				return;
			}
			else
			{
				return;
			}
		}

		internal void EnableStreamUnloadOnPersistentState()
		{
			this._unloadStreamsOnPersistentState = true;
		}

		public PSPersistableIdleAction GetPersistableIdleAction(ReadOnlyCollection<BookmarkInfo> bookmarks, bool externalSuspendRequest)
		{
			PSPersistableIdleAction pSPersistableIdleAction;
			PSPersistableIdleAction persistableIdleAction = this.PSWorkflowInstance.GetPersistableIdleAction(bookmarks, externalSuspendRequest);
			if (persistableIdleAction != PSPersistableIdleAction.Suspend || this.listOfLabels.Count <= 0)
			{
				lock (base.SyncRoot)
				{
					if (persistableIdleAction != PSPersistableIdleAction.Suspend || this.listOfLabels.Count <= 0)
					{
						if (persistableIdleAction == PSPersistableIdleAction.Suspend)
						{
							this.wfSuspendInProgress = true;
						}
						pSPersistableIdleAction = persistableIdleAction;
					}
					else
					{
						pSPersistableIdleAction = PSPersistableIdleAction.None;
					}
				}
				return pSPersistableIdleAction;
			}
			else
			{
				return PSPersistableIdleAction.None;
			}
		}

		private void HandleMyStateChanged(object sender, JobStateEventArgs e)
		{
			object obj = null;
			string str;
			string[] strArrays = new string[2];
			strArrays[0] = e.JobStateInfo.State.ToString();
			strArrays[1] = e.PreviousJobStateInfo.State.ToString();
			this._tracer.WriteMessage("PSWorkflowJob", "HandleMyStateChanged", this.WorkflowGuidForTraces, this, "NewState: {0}; OldState: {1}", strArrays);
			bool flag = false;
			if (e.PreviousJobStateInfo.State == JobState.NotStarted)
			{
				base.PSBeginTime = new DateTime?(DateTime.Now);
				if (this._definition != null)
				{
					if (this._definition.IsScriptWorkflow)
					{
						str = "script";
					}
					else
					{
						str = "xaml";
					}
					PSSQMAPI.IncrementWorkflowType(str);
				}
				PSSQMAPI.NoteWorkflowStart(base.InstanceId);
			}
			JobState state = e.JobStateInfo.State;
			switch (state)
			{
				case JobState.Running:
				{
					lock (base.SyncRoot)
					{
						this._suspending = false;
						this._resuming = false;
						this.wfSuspendInProgress = false;
					}
					lock (this._syncObject)
					{
						this.JobRunning.Set();
						if (this._jobSuspendedOrAborted != null)
						{
							this.JobSuspendedOrAborted.Reset();
						}
						this._statusMessage = string.Empty;
						goto Label0;
					}
				}
				case JobState.Completed:
				case JobState.Failed:
				case JobState.Stopped:
				{
					base.PSEndTime = new DateTime?(DateTime.Now);
					if (this.JobMetadata.TryGetValue("ParentInstanceId", out obj) && obj as Guid != null)
					{
						PSSQMAPI.IncrementWorkflowStateData((Guid)obj, e.JobStateInfo.State);
					}
					PSSQMAPI.NoteWorkflowEnd(base.InstanceId);
					lock (this._syncObject)
					{
						PSWorkflowJob.StructuredTracer.EndJobLogic(base.InstanceId);
						this.JobSuspendedOrAborted.Set();
						this.JobRunning.Set();
					}
					flag = true;
					goto Label0;
				}
				case JobState.Blocked:
				{
				Label0:
					if (!flag || !this._unloadStreamsOnPersistentState)
					{
						return;
					}
					else
					{
						this._tracer.WriteMessage("PSWorkflowJob", "HandleMyStateChanged", this.WorkflowGuidForTraces, this, "BEGIN Unloading streams from memory", new string[0]);
						this.SelfUnloadJobStreams();
						this._tracer.WriteMessage("PSWorkflowJob", "HandleMyStateChanged", this.WorkflowGuidForTraces, this, "END Unloading streams from memory", new string[0]);
						return;
					}
				}
				case JobState.Suspended:
				{
					lock (base.SyncRoot)
					{
						this._suspending = false;
						this._resuming = false;
						this.wfSuspendInProgress = false;
					}
					base.PSEndTime = new DateTime?(DateTime.Now);
					lock (this._syncObject)
					{
						this.JobSuspendedOrAborted.Set();
						this.JobRunning.Reset();
					}
					flag = true;
					goto Label0;
				}
				default:
				{
					goto Label0;
				}
			}
		}

		private void InitializeWithWorkflow(PSWorkflowInstance instance, bool closeStreams = false)
		{
			this._tracer.WriteMessage("PSWorkflowJob", "InitializeWithWorkflow", this.WorkflowGuidForTraces, this, "Setting streams", new string[0]);
			PSWorkflowJob pSWorkflowJob = this;
			PSDataCollection<PSObject> outputStream = instance.Streams.OutputStream;
			PSDataCollection<PSObject> pSObjects = outputStream;
			if (outputStream == null)
			{
				pSObjects = new PSDataCollection<PSObject>();
			}
			pSWorkflowJob.Output = pSObjects;
			PSWorkflowJob pSWorkflowJob1 = this;
			PSDataCollection<ProgressRecord> progressStream = instance.Streams.ProgressStream;
			PSDataCollection<ProgressRecord> progressRecords = progressStream;
			if (progressStream == null)
			{
				progressRecords = new PSDataCollection<ProgressRecord>();
			}
			pSWorkflowJob1.Progress = progressRecords;
			PSWorkflowJob pSWorkflowJob2 = this;
			PSDataCollection<WarningRecord> warningStream = instance.Streams.WarningStream;
			PSDataCollection<WarningRecord> warningRecords = warningStream;
			if (warningStream == null)
			{
				warningRecords = new PSDataCollection<WarningRecord>();
			}
			pSWorkflowJob2.Warning = warningRecords;
			PSWorkflowJob pSWorkflowJob3 = this;
			PSDataCollection<ErrorRecord> errorStream = instance.Streams.ErrorStream;
			PSDataCollection<ErrorRecord> errorRecords = errorStream;
			if (errorStream == null)
			{
				errorRecords = new PSDataCollection<ErrorRecord>();
			}
			pSWorkflowJob3.Error = errorRecords;
			PSWorkflowJob pSWorkflowJob4 = this;
			PSDataCollection<DebugRecord> debugStream = instance.Streams.DebugStream;
			PSDataCollection<DebugRecord> debugRecords = debugStream;
			if (debugStream == null)
			{
				debugRecords = new PSDataCollection<DebugRecord>();
			}
			pSWorkflowJob4.Debug = debugRecords;
			PSWorkflowJob pSWorkflowJob5 = this;
			PSDataCollection<VerboseRecord> verboseStream = instance.Streams.VerboseStream;
			PSDataCollection<VerboseRecord> verboseRecords = verboseStream;
			if (verboseStream == null)
			{
				verboseRecords = new PSDataCollection<VerboseRecord>();
			}
			pSWorkflowJob5.Verbose = verboseRecords;
			if (closeStreams)
			{
				base.Output.Complete();
				base.Progress.Complete();
				base.Warning.Complete();
				base.Error.Complete();
				base.Debug.Complete();
				base.Verbose.Complete();
				return;
			}
			else
			{
				return;
			}
		}

		internal bool IsFinishedState(JobState state)
		{
			if (state == JobState.Completed || state == JobState.Failed)
			{
				return true;
			}
			else
			{
				return state == JobState.Stopped;
			}
		}

		private void JobActionAsyncCompleted(object operationState)
		{
			if (!this._isDisposed)
			{
				PSWorkflowJob.AsyncCompleteContainer asyncCompleteContainer = operationState as PSWorkflowJob.AsyncCompleteContainer;
				string[] str = new string[1];
				str[0] = asyncCompleteContainer.Action.ToString();
				this._tracer.WriteMessage("PSWorkflowJob", "JobActionAsyncCompleted", this.WorkflowGuidForTraces, this, "operation: {0}", str);
				try
				{
					PSWorkflowJob.ActionType action = asyncCompleteContainer.Action;
					switch (action)
					{
						case PSWorkflowJob.ActionType.Start:
						{
							if (asyncCompleteContainer.EventArgs.Error == null)
							{
								this.JobRunning.WaitOne();
							}
							this.OnStartJobCompleted(asyncCompleteContainer.EventArgs);
							break;
						}
						case PSWorkflowJob.ActionType.Stop:
						{
							if (asyncCompleteContainer.EventArgs.Error == null)
							{
								base.Finished.WaitOne();
							}
							this.OnStopJobCompleted(asyncCompleteContainer.EventArgs);
							break;
						}
						case PSWorkflowJob.ActionType.Suspend:
						{
							if (asyncCompleteContainer.EventArgs.Error == null)
							{
								this.JobSuspendedOrAborted.WaitOne();
							}
							this.OnSuspendJobCompleted(asyncCompleteContainer.EventArgs);
							break;
						}
						case PSWorkflowJob.ActionType.Resume:
						{
							if (asyncCompleteContainer.EventArgs.Error == null)
							{
								this.JobRunning.WaitOne();
							}
							this.OnResumeJobCompleted(asyncCompleteContainer.EventArgs);
							break;
						}
						case PSWorkflowJob.ActionType.Abort:
						{
							if (asyncCompleteContainer.EventArgs.Error == null)
							{
								this.JobSuspendedOrAborted.WaitOne();
							}
							this.OnSuspendJobCompleted(asyncCompleteContainer.EventArgs);
							break;
						}
					}
				}
				catch (ObjectDisposedException objectDisposedException)
				{
				}
				return;
			}
			else
			{
				return;
			}
		}

		private void JobActionWorker(AsyncOperation asyncOp, PSWorkflowJob.ActionType action, string reason, string label)
		{
			Exception exception = null;
			try
			{
				PSWorkflowJob.ActionType actionType = action;
				switch (actionType)
				{
					case PSWorkflowJob.ActionType.Start:
					{
						this.DoStartJobLogic(null);
						break;
					}
					case PSWorkflowJob.ActionType.Stop:
					{
						this.DoStopJob();
						break;
					}
					case PSWorkflowJob.ActionType.Suspend:
					{
						this.DoSuspendJob();
						break;
					}
					case PSWorkflowJob.ActionType.Resume:
					{
						this.DoResumeJob(label);
						break;
					}
					case PSWorkflowJob.ActionType.Abort:
					{
						this.DoAbortJob(reason);
						break;
					}
					case PSWorkflowJob.ActionType.Terminate:
					{
						this.DoTerminateJob(reason);
						break;
					}
				}
			}
			catch (Exception exception2)
			{
				Exception exception1 = exception2;
				exception = exception1;
			}
			AsyncCompletedEventArgs asyncCompletedEventArg = new AsyncCompletedEventArgs(exception, false, asyncOp.UserSuppliedState);
			PSWorkflowJob.AsyncCompleteContainer asyncCompleteContainer = new PSWorkflowJob.AsyncCompleteContainer();
			asyncCompleteContainer.EventArgs = asyncCompletedEventArg;
			asyncCompleteContainer.Action = action;
			PSWorkflowJob.AsyncCompleteContainer asyncCompleteContainer1 = asyncCompleteContainer;
			asyncOp.PostOperationCompleted(new SendOrPostCallback(this.JobActionAsyncCompleted), asyncCompleteContainer1);
		}

		internal void LoadWorkflow(CommandParameterCollection commandParameterCollection, Activity activity, string xaml)
		{
			bool flag = false;
			string workflowXaml;
			string runtimeAssemblyPath;
			this._tracer.WriteMessage("PSWorkflowJob", "LoadWorkflow", this.WorkflowGuidForTraces, this, "BEGIN", new string[0]);
			if (activity == null)
			{
				activity = DefinitionCache.Instance.GetActivityFromCache(this._definition, out flag);
				if (activity == null)
				{
					throw new InvalidOperationException(Resources.ActivityNotCached);
				}
			}
			if (!string.IsNullOrEmpty(xaml))
			{
				workflowXaml = xaml;
				runtimeAssemblyPath = null;
			}
			else
			{
				workflowXaml = DefinitionCache.Instance.GetWorkflowXaml(this._definition);
				runtimeAssemblyPath = DefinitionCache.Instance.GetRuntimeAssemblyPath(this._definition);
			}
			this._location = null;
			this.SortStartParameters(activity as DynamicActivity, commandParameterCollection);
			if (string.IsNullOrEmpty(this._location))
			{
				this._location = "localhost";
			}
			if (this._jobMetadata.ContainsKey("Location"))
			{
				this._jobMetadata.Remove("Location");
			}
			this._jobMetadata.Add("Location", this._location);
			PSWorkflowDefinition pSWorkflowDefinition = new PSWorkflowDefinition(activity, workflowXaml, runtimeAssemblyPath);
			PSWorkflowContext pSWorkflowContext = new PSWorkflowContext(this._workflowParameters, this._psWorkflowCommonParameters, this._jobMetadata, this._privateMetadata);
			this._workflowInstance = this._runtime.Configuration.CreatePSWorkflowInstance(pSWorkflowDefinition, pSWorkflowContext, this._inputCollection, this);
			this.ConfigureWorkflowHandlers();
			this._tracer.WriteMessage("PSWorkflowJob", "LoadWorkflow", this.WorkflowGuidForTraces, this, "Calling instance loader", new string[0]);
			this._workflowInstance.CreateInstance();
			this.InitializeWithWorkflow(this._workflowInstance, false);
			this.WorkflowInstanceLoaded = true;
			this._tracer.WriteMessage("PSWorkflowJob", "LoadWorkflow", this.WorkflowGuidForTraces, this, "END", new string[0]);
		}

		private void OnWorkflowAborted(Exception e, object sender)
		{
			this._tracer.WriteMessage("PSWorkflowJob", "OnWorkflowAborted", this.WorkflowGuidForTraces, this, "BEGIN", new string[0]);
			this.DoSetJobState(JobState.Suspended, e);
			PSWorkflowJob.StructuredTracer.WorkflowExecutionAborted(this._workflowInstance.Id);
			PSWorkflowJob._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 1, (long)1, true);
			PSWorkflowJob._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 2, (long)1, true);
			PSWorkflowJob._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 13, (long)1, true);
			PSWorkflowJob._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 14, (long)1, true);
			this._tracer.WriteMessage("PSWorkflowJob", "OnWorkflowAborted", this.WorkflowGuidForTraces, this, "END", new string[0]);
		}

		private void OnWorkflowCompleted(object sender)
		{
			this._tracer.WriteMessage("PSWorkflowJob", "OnWorkflowCompleted", this.WorkflowGuidForTraces, this, "BEGIN", new string[0]);
			this.DoSetJobState(JobState.Completed, null);
			PSWorkflowJob.StructuredTracer.WorkflowExecutionFinished(this._workflowInstance.Id);
			PSWorkflowJob._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 9, (long)1, true);
			PSWorkflowJob._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 10, (long)1, true);
			PSWorkflowJob._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 13, (long)1, true);
			PSWorkflowJob._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 14, (long)1, true);
			this._tracer.WriteMessage("PSWorkflowJob", "OnWorkflowCompleted", this.WorkflowGuidForTraces, this, "END", new string[0]);
		}

		private void OnWorkflowFaulted(Exception e, object sender)
		{
			this._tracer.WriteMessage("PSWorkflowJob", "OnWorkflowFaulted", this.WorkflowGuidForTraces, this, "BEGIN", new string[0]);
			PSWorkflowJob.StructuredTracer.WorkflowExecutionError(this._workflowInstance.Id, string.Concat(Tracer.GetExceptionString(e), Environment.NewLine, e));
			this.DoSetJobState(JobState.Failed, e);
			PSWorkflowJob._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 1, (long)1, true);
			PSWorkflowJob._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 2, (long)1, true);
			PSWorkflowJob._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 13, (long)1, true);
			PSWorkflowJob._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 14, (long)1, true);
			this._tracer.WriteMessage("PSWorkflowJob", "OnWorkflowFaulted", this.WorkflowGuidForTraces, this, "END", new string[0]);
		}

		private void OnWorkflowIdle(ReadOnlyCollection<BookmarkInfo> bookmarks, object sender)
		{
			this._tracer.WriteMessage("PSWorkflowJob", "OnWorkflowIdle", this.WorkflowGuidForTraces, this, "BEGIN", new string[0]);
			if (this.OnIdle != null)
			{
				this.OnIdle(this, bookmarks);
			}
			this._tracer.WriteMessage("PSWorkflowJob", "OnWorkflowIdle", this.WorkflowGuidForTraces, this, "END", new string[0]);
		}

		private PSPersistableIdleAction OnWorkflowPersistableIdleAction(ReadOnlyCollection<BookmarkInfo> bookmarks, bool externalSuspendRequest, object sender)
		{
			this._tracer.WriteMessage("PSWorkflowJob", "OnWorkflowPersistIdleAction", this.WorkflowGuidForTraces, this, "BEGIN", new string[0]);
			PSPersistableIdleAction pSPersistableIdleAction = PSPersistableIdleAction.NotDefined;
			if (this.OnPersistableIdleAction != null)
			{
				pSPersistableIdleAction = this.OnPersistableIdleAction(this, bookmarks, externalSuspendRequest);
			}
			this._tracer.WriteMessage("PSWorkflowJob", "OnWorkflowPersistIdleAction", this.WorkflowGuidForTraces, this, "END", new string[0]);
			return pSPersistableIdleAction;
		}

		private void OnWorkflowStopped(object sender)
		{
			this._tracer.WriteMessage("PSWorkflowJob", "OnWorkflowStopped", this.WorkflowGuidForTraces, this, "BEGIN", new string[0]);
			this.DoSetJobState(JobState.Stopped, null);
			PSWorkflowJob.StructuredTracer.WorkflowExecutionCancelled(this._workflowInstance.Id);
			PSWorkflowJob._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 7, (long)1, true);
			PSWorkflowJob._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 8, (long)1, true);
			PSWorkflowJob._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 13, (long)1, true);
			PSWorkflowJob._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 14, (long)1, true);
			this._tracer.WriteMessage("PSWorkflowJob", "OnWorkflowStopped", this.WorkflowGuidForTraces, this, "END", new string[0]);
		}

		private void OnWorkflowSuspended(object sender)
		{
			this._tracer.WriteMessage("PSWorkflowJob", "OnWorkflowSuspended", this.WorkflowGuidForTraces, this, "BEGIN", new string[0]);
			if (this.DoSetJobState(JobState.Suspended, null))
			{
				PSWorkflowJob.StructuredTracer.WorkflowUnloaded(this._workflowInstance.Id);
				PSWorkflowJob._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 11, (long)1, true);
				PSWorkflowJob._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 12, (long)1, true);
				PSWorkflowJob._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 13, (long)1, true);
				PSWorkflowJob._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 14, (long)1, true);
			}
			this._tracer.WriteMessage("PSWorkflowJob", "OnWorkflowSuspended", this.WorkflowGuidForTraces, this, "END", new string[0]);
		}

		private void OnWorkflowUnloaded(object sender)
		{
			this._tracer.WriteMessage("PSWorkflowJob", "OnWorkflowUnloaded", this.WorkflowGuidForTraces, this, "BEGIN", new string[0]);
			if (this.OnUnloaded != null)
			{
				this.OnUnloaded(this);
			}
			this._tracer.WriteMessage("PSWorkflowJob", "OnWorkflowUnloaded", this.WorkflowGuidForTraces, this, "END", new string[0]);
		}

		internal void RestoreFromWorkflowInstance(PSWorkflowInstance instance)
		{
			object obj = null;
			Exception error;
			this._tracer.WriteMessage("PSWorkflowJob", "RestoreFromWorkflowInstance", this.WorkflowGuidForTraces, this, "BEGIN", new string[0]);
			if (!instance.PSWorkflowContext.JobMetadata.TryGetValue("Reason", out obj))
			{
				error = instance.Error;
			}
			else
			{
				error = obj as Exception;
			}
			this._workflowParameters = instance.PSWorkflowContext.WorkflowParameters;
			this._psWorkflowCommonParameters = instance.PSWorkflowContext.PSWorkflowCommonParameters;
			this._jobMetadata = instance.PSWorkflowContext.JobMetadata;
			this._privateMetadata = instance.PSWorkflowContext.PrivateMetadata;
			if (instance.PSWorkflowContext.JobMetadata.TryGetValue("Location", out obj))
			{
				this._location = obj as string;
			}
			if (instance.PSWorkflowContext.JobMetadata.TryGetValue("StatusMessage", out obj))
			{
				this._statusMessage = obj as string;
			}
			if (instance.State == JobState.Suspended)
			{
				this._statusMessage = Resources.SuspendedJobRecoveredFromPreviousSession;
			}
			lock (this._syncObject)
			{
				this._hasMoreDataOnDisk = true;
			}
			this.DoSetJobState(instance.State, error);
			this._tracer.WriteMessage("PSWorkflowJob", "RestoreFromWorkflowInstance", this.WorkflowGuidForTraces, this, "END", new string[0]);
		}

		public void ResumeBookmark(Bookmark bookmark, object state)
		{
			if (bookmark != null)
			{
				this.DoResumeBookmark(bookmark, state);
				return;
			}
			else
			{
				throw new ArgumentNullException("bookmark");
			}
		}

		public void ResumeBookmark(Bookmark bookmark, bool supportDisconnectedStreams, PowerShellStreams<PSObject, PSObject> streams)
		{
			if (bookmark != null)
			{
				if (streams != null)
				{
					PSResumableActivityContext pSResumableActivityContext = new PSResumableActivityContext(streams);
					pSResumableActivityContext.SupportDisconnectedStreams = supportDisconnectedStreams;
					pSResumableActivityContext.Failed = false;
					pSResumableActivityContext.Error = null;
					this.DoResumeBookmark(bookmark, pSResumableActivityContext);
					return;
				}
				else
				{
					throw new ArgumentNullException("streams");
				}
			}
			else
			{
				throw new ArgumentNullException("bookmark");
			}
		}

		public void ResumeBookmark(Bookmark bookmark, bool supportDisconnectedStreams, PowerShellStreams<PSObject, PSObject> streams, Exception exception)
		{
			if (bookmark != null)
			{
				if (streams != null)
				{
					if (exception != null)
					{
						PSResumableActivityContext pSResumableActivityContext = new PSResumableActivityContext(streams);
						pSResumableActivityContext.SupportDisconnectedStreams = supportDisconnectedStreams;
						pSResumableActivityContext.Failed = true;
						pSResumableActivityContext.Error = exception;
						this.DoResumeBookmark(bookmark, pSResumableActivityContext);
						return;
					}
					else
					{
						throw new ArgumentNullException("exception");
					}
				}
				else
				{
					throw new ArgumentNullException("streams");
				}
			}
			else
			{
				throw new ArgumentNullException("bookmark");
			}
		}

		public override void ResumeJob()
		{
			this.ResumeJob(null);
		}

		public virtual void ResumeJob(string label)
		{
			Exception exception = null;
			this.AssertNotDisposed();
			Guid guid = Guid.NewGuid();
			ManualResetEvent manualResetEvent = new ManualResetEvent(false);
			this._statusMessage = Resources.JobQueuedForResume;
			this._runtime.JobManager.SubmitOperation(this, new Action<object>(this.DoResumeJobCatchException), new Tuple<object, Guid>(new Tuple<string, ManualResetEvent>(label, manualResetEvent), guid), JobState.Suspended);
			manualResetEvent.WaitOne();
			manualResetEvent.Dispose();
			lock (this._resumeErrorSyncObject)
			{
				if (this._resumeErrors.TryGetValue(guid, out exception))
				{
					this._resumeErrors.Remove(guid);
					throw exception;
				}
			}
		}

		public override void ResumeJobAsync()
		{
			try
			{
				this.AssertNotDisposed();
				this._runtime.JobManager.SubmitOperation(this, new Action<object>(this.DoResumeJobAsync), null, JobState.Suspended);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				this.OnResumeJobCompleted(new AsyncCompletedEventArgs(exception, false, null));
			}
		}

		public virtual void ResumeJobAsync(string label)
		{
			try
			{
				this.AssertNotDisposed();
				this._runtime.JobManager.SubmitOperation(this, new Action<object>(this.DoResumeJobAsync), label, JobState.Suspended);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				this.OnResumeJobCompleted(new AsyncCompletedEventArgs(exception, false, null));
			}
		}

		private void SelfUnloadJobStreams()
		{
			if (!this._hasMoreDataOnDisk)
			{
				lock (base.SyncRoot)
				{
					if (!this._hasMoreDataOnDisk)
					{
						base.UnloadJobStreams();
					}
				}
				return;
			}
			else
			{
				return;
			}
		}

		private void SortStartParameters(DynamicActivity dynamicActivity, CommandParameterCollection parameters)
		{
			uint num = 0;
			bool flag;
			bool flag1;
			string str;
			object baseObject;
			if (dynamicActivity == null)
			{
				flag = false;
			}
			else
			{
				flag = dynamicActivity.Properties.Contains("PSComputerName");
			}
			bool flag2 = flag;
			if (dynamicActivity == null)
			{
				flag1 = false;
			}
			else
			{
				flag1 = dynamicActivity.Properties.Contains("PSPrivateMetadata");
			}
			bool flag3 = flag1;
			this._jobMetadata.Add("WorkflowTakesPrivateMetadata", flag3);
			if (parameters != null)
			{
				foreach (CommandParameter parameter in parameters)
				{
					PowerShellTraceSource powerShellTraceSource = this._tracer;
					string str1 = "PSWorkflowJob";
					string str2 = "SortStartParameters";
					Guid workflowGuidForTraces = this.WorkflowGuidForTraces;
					PSWorkflowJob pSWorkflowJob = this;
					string str3 = "Found parameter; {0}; {1}";
					string[] name = new string[2];
					name[0] = parameter.Name;
					string[] strArrays = name;
					int num1 = 1;
					if (parameter.Value == null)
					{
						str = null;
					}
					else
					{
						str = parameter.Value.ToString();
					}
					strArrays[num1] = str;
					powerShellTraceSource.WriteMessage(str1, str2, workflowGuidForTraces, pSWorkflowJob, str3, name);
					string name1 = parameter.Name;
					string str4 = name1;
					if (name1 != null)
					{
						if (str4 == "PSComputerName")
						{
							if (!flag2)
							{
								string value = parameter.Value as string;
								this._location = value;
								string[] strArrays1 = LanguagePrimitives.ConvertTo<string[]>(parameter.Value);
								this._psWorkflowCommonParameters[parameter.Name] = strArrays1;
								PSSQMAPI.NoteWorkflowCommonParametersValues(parameter.Name, (int)strArrays1.Length);
								continue;
							}
							else
							{
								this._location = "localhost";
								this._workflowParameters[parameter.Name] = LanguagePrimitives.ConvertTo<string[]>(parameter.Value);
								continue;
							}
						}
						else if (str4 == "PSPrivateMetadata")
						{
							Hashtable hashtables = parameter.Value as Hashtable;
							if (hashtables == null)
							{
								continue;
							}
							IDictionaryEnumerator enumerator = hashtables.GetEnumerator();
							while (enumerator.MoveNext())
							{
								this._privateMetadata.Add(enumerator.Key.ToString(), enumerator.Value);
							}
							if (flag3)
							{
								this._workflowParameters.Add(parameter.Name, parameter.Value);
							}
							PSSQMAPI.NoteWorkflowCommonParametersValues(parameter.Name, hashtables.Count);
							continue;
						}
						else if (str4 == "PSInputCollection")
						{
							if (parameter.Value as PSObject != null)
							{
								baseObject = ((PSObject)parameter.Value).BaseObject;
							}
							else
							{
								baseObject = parameter.Value;
							}
							object obj = baseObject;
							if (obj as PSDataCollection<PSObject> == null)
							{
								PSDataCollection<PSObject> pSObjects = new PSDataCollection<PSObject>();
								IEnumerator enumerator1 = LanguagePrimitives.GetEnumerator(obj);
								if (enumerator1 == null)
								{
									pSObjects.Add(PSObject.AsPSObject(parameter.Value));
								}
								else
								{
									while (enumerator1.MoveNext())
									{
										pSObjects.Add(PSObject.AsPSObject(enumerator1.Current));
									}
								}
								this._inputCollection = pSObjects;
								continue;
							}
							else
							{
								this._inputCollection = obj as PSDataCollection<PSObject>;
								continue;
							}
						}
						else if (str4 == "PSParameterCollection")
						{
							continue;
						}
						else if (str4 == "PSRunningTimeoutSec" || str4 == "PSElapsedTimeoutSec" || str4 == "PSConnectionRetryCount" || str4 == "PSActionRetryCount" || str4 == "PSConnectionRetryIntervalSec" || str4 == "PSActionRetryIntervalSec")
						{
							this._psWorkflowCommonParameters.Add(parameter.Name, parameter.Value);
							if (!LanguagePrimitives.TryConvertTo<uint>(parameter.Value, out num))
							{
								continue;
							}
							PSSQMAPI.NoteWorkflowCommonParametersValues(parameter.Name, num);
							continue;
						}
						else if (str4 == "PSPersist" || str4 == "PSCredential" || str4 == "PSPort" || str4 == "PSUseSSL" || str4 == "PSConfigurationName" || str4 == "PSApplicationName" || str4 == "PSConnectionURI" || str4 == "PSSessionOption" || str4 == "PSAuthentication" || str4 == "PSAuthenticationLevel" || str4 == "PSCertificateThumbprint" || str4 == "PSAllowRedirection" || str4 == "Verbose" || str4 == "Debug" || str4 == "ErrorAction" || str4 == "WarningAction" || str4 == "PSWorkflowErrorAction")
						{
							this._psWorkflowCommonParameters.Add(parameter.Name, parameter.Value);
							PSSQMAPI.IncrementWorkflowCommonParameterPresent(parameter.Name);
							continue;
						}
						else if (str4 == "PSSenderInfo" || str4 == "PSWorkflowRoot" || str4 == "PSCurrentDirectory")
						{
							this._psWorkflowCommonParameters.Add(parameter.Name, parameter.Value);
							continue;
						}
					}
					this._workflowParameters.Add(parameter.Name, parameter.Value);
					if (parameter.Value == null)
					{
						continue;
					}
					PSSQMAPI.IncrementWorkflowSpecificParameterType(parameter.Value.GetType());
				}
			}
			this._psWorkflowCommonParameters.Add("WorkflowCommandName", this._definition.Command);
		}

		public override void StartJob()
		{
			this.AssertValidState(JobState.NotStarted);
			this._runtime.JobManager.SubmitOperation(this, new Action<object>(this.DoStartJobLogic), null, JobState.NotStarted);
			this.JobRunning.WaitOne();
		}

		public override void StartJobAsync()
		{
			try
			{
				this.AssertValidState(JobState.NotStarted);
				this._runtime.JobManager.SubmitOperation(this, new Action<object>(this.DoStartJobAsync), null, JobState.NotStarted);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				this.OnStartJobCompleted(new AsyncCompletedEventArgs(exception, false, null));
			}
		}

		public override void StopJob()
		{
			this.AssertNotDisposed();
			this.DoStopJob();
			base.Finished.WaitOne();
		}

		public override void StopJob(bool force, string reason)
		{
			this.AssertNotDisposed();
			if (!force)
			{
				this.DoStopJob();
				base.Finished.WaitOne();
			}
			else
			{
				if (this.DoTerminateJob(reason))
				{
					base.Finished.WaitOne();
					return;
				}
			}
		}

		public override void StopJobAsync()
		{
			this.AssertNotDisposed();
			this._tracer.WriteMessage("PSWorkflowJob", "StopJobAsync", this.WorkflowGuidForTraces, this, "", new string[0]);
			AsyncOperation asyncOperation = AsyncOperationManager.CreateOperation(null);
			PSWorkflowJob.JobActionWorkerDelegate jobActionWorkerDelegate = new PSWorkflowJob.JobActionWorkerDelegate(this.JobActionWorker);
			jobActionWorkerDelegate.BeginInvoke(asyncOperation, PSWorkflowJob.ActionType.Stop, string.Empty, string.Empty, null, null);
		}

		public override void StopJobAsync(bool force, string reason)
		{
			this.AssertNotDisposed();
			this._tracer.WriteMessage("PSWorkflowJob", "StopJobAsync", this.WorkflowGuidForTraces, this, "", new string[0]);
			AsyncOperation asyncOperation = AsyncOperationManager.CreateOperation(null);
			PSWorkflowJob.JobActionWorkerDelegate jobActionWorkerDelegate = new PSWorkflowJob.JobActionWorkerDelegate(this.JobActionWorker);
			PSWorkflowJob.ActionType actionType = PSWorkflowJob.ActionType.Stop;
			if (force)
			{
				actionType = PSWorkflowJob.ActionType.Terminate;
			}
			jobActionWorkerDelegate.BeginInvoke(asyncOperation, actionType, reason, string.Empty, null, null);
		}

		public override void SuspendJob()
		{
			this.AssertNotDisposed();
			this.DoSuspendJob();
			this.JobSuspendedOrAborted.WaitOne();
		}

		public override void SuspendJob(bool force, string reason)
		{
			this.AssertNotDisposed();
			if (!force)
			{
				this.DoSuspendJob();
				this.JobSuspendedOrAborted.WaitOne();
				return;
			}
			else
			{
				this.DoAbortJob(reason);
				this.JobSuspendedOrAborted.WaitOne();
				return;
			}
		}

		public override void SuspendJobAsync()
		{
			this.AssertNotDisposed();
			this._tracer.WriteMessage("PSWorkflowJob", "SuspendJobAsync", this.WorkflowGuidForTraces, this, "", new string[0]);
			AsyncOperation asyncOperation = AsyncOperationManager.CreateOperation(null);
			PSWorkflowJob.JobActionWorkerDelegate jobActionWorkerDelegate = new PSWorkflowJob.JobActionWorkerDelegate(this.JobActionWorker);
			jobActionWorkerDelegate.BeginInvoke(asyncOperation, PSWorkflowJob.ActionType.Suspend, string.Empty, string.Empty, null, null);
		}

		public override void SuspendJobAsync(bool force, string reason)
		{
			this.AssertNotDisposed();
			this._tracer.WriteMessage("PSWorkflowJob", "SuspendJobAsync", this.WorkflowGuidForTraces, this, "", new string[0]);
			AsyncOperation asyncOperation = AsyncOperationManager.CreateOperation(null);
			PSWorkflowJob.JobActionWorkerDelegate jobActionWorkerDelegate = new PSWorkflowJob.JobActionWorkerDelegate(this.JobActionWorker);
			PSWorkflowJob.ActionType actionType = PSWorkflowJob.ActionType.Suspend;
			if (force)
			{
				actionType = PSWorkflowJob.ActionType.Abort;
			}
			jobActionWorkerDelegate.BeginInvoke(asyncOperation, actionType, reason, string.Empty, null, null);
		}

		public override void UnblockJob()
		{
			throw new NotSupportedException();
		}

		public override void UnblockJobAsync()
		{
			throw new NotSupportedException();
		}

		private static JobInvocationInfo Validate(JobInvocationInfo specification)
		{
			if (specification != null)
			{
				if (specification.Definition != null)
				{
					if (!string.IsNullOrEmpty(specification.Definition.Command))
					{
						return specification;
					}
					else
					{
						throw new ArgumentException(Resources.UninitializedSpecification, "specification");
					}
				}
				else
				{
					throw new ArgumentException(Resources.UninitializedSpecification, "specification");
				}
			}
			else
			{
				throw new ArgumentNullException("specification");
			}
		}

		private enum ActionType
		{
			Start,
			Stop,
			Suspend,
			Resume,
			Abort,
			Terminate
		}

		private class AsyncCompleteContainer
		{
			internal AsyncCompletedEventArgs EventArgs;

			internal PSWorkflowJob.ActionType Action;

			public AsyncCompleteContainer()
			{
			}
		}

		private delegate void JobActionWorkerDelegate(AsyncOperation asyncOp, PSWorkflowJob.ActionType action, string reason, string label);
	}
}