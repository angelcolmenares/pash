using Microsoft.PowerShell.Activities;
using System;
using System.Activities;
using System.Activities.Hosting;
using System.Activities.Persistence;
using System.Activities.Tracking;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Tracing;
using System.Threading;
using System.Timers;

namespace Microsoft.PowerShell.Workflow
{
	internal class PSWorkflowApplicationInstance : PSWorkflowInstance
	{
		private const int NotHandled = 0;

		private const int Handled = 1;

		private const string LocalHost = "localhost";

		private readonly PowerShellTraceSource Tracer;

		private WorkflowApplication workflowApplication;

		private Guid id;

		private PSWorkflowDefinition _definition;

		private PowerShellStreams<PSObject, PSObject> _streams;

		private Exception _errorException;

		private PSWorkflowContext _metadatas;

		private PSWorkflowTimer _timers;

		private WorkflowInstanceCreationMode creationMode;

		private Dictionary<string, PSActivityContext> asyncExecutionCollection;

		private bool PersistAfterNextPSActivity;

		private bool suspendAtNextCheckpoint;

		private readonly PSWorkflowInstanceStore _stores;

		private readonly static Tracer _structuredTracer;

		private PSWorkflowJob _job;

		private bool errorExceptionLoadCalled;

		private bool callSuspendDelegate;

		private int terminalStateHandled;

		private bool _streamsDisposed;

		private HostParameterDefaults _paramDefaults;

		internal bool InternalUnloaded;

		private bool wfAppNeverLoaded;

		private bool PersistIdleTimerInProgressOrTriggered;

		private bool IsTerminalStateAction;

		private System.Timers.Timer PersistUnloadTimer;

		private object ReactivateSync;

		public override Exception Error
		{
			get
			{
				if (this._errorException == null && this.creationMode == WorkflowInstanceCreationMode.AfterCrashOrShutdown && !this.errorExceptionLoadCalled)
				{
					lock (base.SyncLock)
					{
						if (this._errorException == null && this.creationMode == WorkflowInstanceCreationMode.AfterCrashOrShutdown && !this.errorExceptionLoadCalled)
						{
							try
							{
								this.errorExceptionLoadCalled = true;
								this._stores.Load(WorkflowStoreComponents.TerminatingError);
							}
							catch (Exception exception1)
							{
								Exception exception = exception1;
								this.Tracer.WriteMessage("Exception occurred while loading the workflow terminating error");
								this.Tracer.TraceException(exception);
								this._errorException = null;
								this.Tracer.WriteMessage("Marking the job to the faulted state.");
							}
						}
					}
				}
				return this._errorException;
			}
			set
			{
				this.CheckDisposed();
				this._errorException = value;
			}
		}

		internal override Guid Id
		{
			get
			{
				return this.id;
			}
		}

		public override PSWorkflowId InstanceId
		{
			get
			{
				return new PSWorkflowId(this.id);
			}
		}

		public override PSWorkflowInstanceStore InstanceStore
		{
			get
			{
				return this._stores;
			}
		}

		private Guid JobInstanceId
		{
			get
			{
				if (this._job != null)
				{
					return this._job.InstanceId;
				}
				else
				{
					return Guid.Empty;
				}
			}
		}

		public override PSWorkflowContext PSWorkflowContext
		{
			get
			{
				if (this._metadatas == null && this.creationMode == WorkflowInstanceCreationMode.AfterCrashOrShutdown)
				{
					lock (base.SyncLock)
					{
						if (this._metadatas == null && this.creationMode == WorkflowInstanceCreationMode.AfterCrashOrShutdown)
						{
							try
							{
								this._stores.Load(WorkflowStoreComponents.Metadata);
							}
							catch (Exception exception1)
							{
								Exception exception = exception1;
								this.Tracer.WriteMessage("Exception occurred while loading the workflow metadata");
								this.Tracer.TraceException(exception);
								this._metadatas = new PSWorkflowContext();
							}
						}
					}
				}
				return this._metadatas;
			}
			set
			{
				this.CheckDisposed();
				this._metadatas = value;
			}
		}

		public override PSWorkflowDefinition PSWorkflowDefinition
		{
			get
			{
				if (this._definition == null && this.creationMode == WorkflowInstanceCreationMode.AfterCrashOrShutdown)
				{
					lock (base.SyncLock)
					{
						if (this._definition == null && this.creationMode == WorkflowInstanceCreationMode.AfterCrashOrShutdown)
						{
							try
							{
								this._stores.Load(WorkflowStoreComponents.Definition);
							}
							catch (Exception exception1)
							{
								Exception exception = exception1;
								this.Tracer.WriteMessage("Exception occurred while loading the workflow definition");
								this.Tracer.TraceException(exception);
								this._definition = new PSWorkflowDefinition(null, string.Empty, string.Empty);
							}
						}
					}
				}
				return this._definition;
			}
			set
			{
				this.CheckDisposed();
				this._definition = value;
			}
		}

		public override PSWorkflowJob PSWorkflowJob
		{
			get
			{
				return this._job;
			}
			protected internal set
			{
				this._job = value;
			}
		}

		public override PowerShellStreams<PSObject, PSObject> Streams
		{
			get
			{
				if (this._streams == null && (this._streamsDisposed || this.creationMode == WorkflowInstanceCreationMode.AfterCrashOrShutdown))
				{
					lock (base.SyncLock)
					{
						if (this._streams == null && (this._streamsDisposed || this.creationMode == WorkflowInstanceCreationMode.AfterCrashOrShutdown))
						{
							try
							{
								this._stores.Load(WorkflowStoreComponents.Streams);
							}
							catch (Exception exception1)
							{
								Exception exception = exception1;
								this.Tracer.WriteMessage("Exception occurred while loading the workflow streams");
								this.Tracer.TraceException(exception);
								this._streams = new PowerShellStreams<PSObject, PSObject>(null);
								this.Tracer.WriteMessage("Marking the job to the faulted state.");
							}
							this.RegisterHandlersForDataAdding(this._streams);
						}
					}
				}
				return this._streams;
			}
			set
			{
				this.CheckDisposed();
				if (this._streams != value)
				{
					if (this._streams != null)
					{
						this.UnregisterHandlersForDataAdding(this._streams);
						this._streams.Dispose();
					}
					this._streams = value;
					this.RegisterHandlersForDataAdding(this._streams);
					return;
				}
				else
				{
					return;
				}
			}
		}

		public override PSWorkflowTimer Timer
		{
			get
			{
				if (this._timers == null && this.creationMode == WorkflowInstanceCreationMode.AfterCrashOrShutdown)
				{
					lock (base.SyncLock)
					{
						if (this._timers == null && this.creationMode == WorkflowInstanceCreationMode.AfterCrashOrShutdown)
						{
							try
							{
								this._stores.Load(WorkflowStoreComponents.Timer);
							}
							catch (Exception exception1)
							{
								Exception exception = exception1;
								this.Tracer.WriteMessage("Exception occurred while loading the workflow timer");
								this.Tracer.TraceException(exception);
								this._timers = null;
								this.Tracer.WriteMessage("Marking the job to the faulted state.");
							}
						}
					}
				}
				return this._timers;
			}
			set
			{
				this.CheckDisposed();
				this._timers = value;
			}
		}

		static PSWorkflowApplicationInstance()
		{
			PSWorkflowApplicationInstance._structuredTracer = new Tracer();
		}

		internal PSWorkflowApplicationInstance(PSWorkflowRuntime runtime, PSWorkflowDefinition definition, PSWorkflowContext metadata, PSDataCollection<PSObject> pipelineInput, PSWorkflowJob job)
		{
			this.Tracer = PowerShellTraceSourceFactory.GetTraceSource();
			this.wfAppNeverLoaded = true;
			this.ReactivateSync = new object();
			if (runtime != null)
			{
				this.Tracer.WriteMessage("Creating Workflow instance.");
				this._definition = definition;
				this._metadatas = metadata;
				this._streams = new PowerShellStreams<PSObject, PSObject>(pipelineInput);
				this.RegisterHandlersForDataAdding(this._streams);
				this._timers = new PSWorkflowTimer(this);
				this.creationMode = WorkflowInstanceCreationMode.Normal;
				this.PersistAfterNextPSActivity = false;
				this.suspendAtNextCheckpoint = false;
				this._job = job;
				base.Runtime = runtime;
				this._stores = base.Runtime.Configuration.CreatePSWorkflowInstanceStore(this);
				this.asyncExecutionCollection = new Dictionary<string, PSActivityContext>();
				base.ForceDisableStartOrEndPersistence = false;
				return;
			}
			else
			{
				throw new ArgumentNullException("runtime");
			}
		}

		internal PSWorkflowApplicationInstance(PSWorkflowRuntime runtime, PSWorkflowId instanceId)
		{
			this.Tracer = PowerShellTraceSourceFactory.GetTraceSource();
			this.wfAppNeverLoaded = true;
			this.ReactivateSync = new object();
			if (runtime != null)
			{
				this.Tracer.WriteMessage("Creating Workflow instance after crash and shutdown workflow.");
				this._definition = null;
				this._metadatas = null;
				this._streams = null;
				this._timers = null;
				this.id = instanceId.Guid;
				this.creationMode = WorkflowInstanceCreationMode.AfterCrashOrShutdown;
				this.PersistAfterNextPSActivity = false;
				this.suspendAtNextCheckpoint = false;
				base.Runtime = runtime;
				this._stores = base.Runtime.Configuration.CreatePSWorkflowInstanceStore(this);
				this.asyncExecutionCollection = new Dictionary<string, PSActivityContext>();
				base.ForceDisableStartOrEndPersistence = false;
				return;
			}
			else
			{
				throw new ArgumentNullException("runtime");
			}
		}

		private void CheckDisposed ()
		{
			if (base.Disposed) {
				throw new PSObjectDisposedException("PSWorkflowApplicationInsnance");
			}
		}

		private bool CheckForPersistenceAfterPSActivity()
		{
			bool persistAfterNextPSActivity = this.PersistAfterNextPSActivity;
			this.PersistAfterNextPSActivity = false;
			return persistAfterNextPSActivity;
		}

		private bool CheckForStartOrEndPersistence()
		{
			bool hasValue;
			if (!base.ForceDisableStartOrEndPersistence)
			{
				if (this.PSWorkflowContext != null && this.PSWorkflowContext.PSWorkflowCommonParameters != null && this.PSWorkflowContext.PSWorkflowCommonParameters.ContainsKey("PSPersist"))
				{
					bool? item = (bool?)(this.PSWorkflowContext.PSWorkflowCommonParameters["PSPersist"] as bool?);
					if (item.HasValue)
					{
						bool? nullable = item;
						if (nullable.GetValueOrDefault())
						{
							hasValue = false;
						}
						else
						{
							hasValue = nullable.HasValue;
						}
						if (hasValue)
						{
							return false;
						}
					}
				}
				return true;
			}
			else
			{
				return false;
			}
		}

		internal override void CheckForTerminalAction()
		{
			lock (this.ReactivateSync)
			{
				this.IsTerminalStateAction = true;
				this.PersistIdleTimerInProgressOrTriggered = false;
				if (this.InternalUnloaded)
				{
					this.DoLoadInstanceForReactivation();
					this.InternalUnloaded = false;
				}
			}
		}

		private bool CheckIfBookmarkExistInCollection(string bookmarkName, ReadOnlyCollection<BookmarkInfo> bookmarks)
		{
			bool flag;
			IEnumerator<BookmarkInfo> enumerator = bookmarks.GetEnumerator();
			using (enumerator)
			{
				while (enumerator.MoveNext())
				{
					BookmarkInfo current = enumerator.Current;
					if (bookmarkName != current.BookmarkName)
					{
						continue;
					}
					flag = true;
					return flag;
				}
				return false;
			}
			return flag;
		}

		private void ConfigureAllExtensions()
		{
			this.workflowApplication.InstanceStore = this._stores.CreateInstanceStore();
			PersistenceIOParticipant persistenceIOParticipant = this._stores.CreatePersistenceIOParticipant();
			if (persistenceIOParticipant != null)
			{
				this.workflowApplication.Extensions.Add(persistenceIOParticipant);
			}
			this.workflowApplication.Extensions.Add(this.GetTrackingParticipant());
			IEnumerable<object> objs = base.Runtime.Configuration.CreateWorkflowExtensions();
			if (objs != null)
			{
				foreach (object obj in objs)
				{
					this.workflowApplication.Extensions.Add(obj);
				}
			}
			IEnumerable<Func<object>> funcs = base.Runtime.Configuration.CreateWorkflowExtensionCreationFunctions<object>();
			if (funcs != null)
			{
				foreach (Func<object> func in funcs)
				{
					this.workflowApplication.Extensions.Add<object>(func);
				}
			}
			this._paramDefaults = new HostParameterDefaults();
			if (this.PSWorkflowContext.PSWorkflowCommonParameters != null)
			{
				foreach (KeyValuePair<string, object> pSWorkflowCommonParameter in this.PSWorkflowContext.PSWorkflowCommonParameters)
				{
					if (!(pSWorkflowCommonParameter.Key != "PSRunningTimeoutSec") || !(pSWorkflowCommonParameter.Key != "PSElapsedTimeoutSec"))
					{
						continue;
					}
					this._paramDefaults.Parameters.Add(pSWorkflowCommonParameter.Key, pSWorkflowCommonParameter.Value);
				}
			}
			if (this.PSWorkflowContext.PrivateMetadata != null)
			{
				this._paramDefaults.Parameters["PSPrivateMetadata"] = this.PSWorkflowContext.PrivateMetadata;
			}
			if (this.PSWorkflowContext.JobMetadata.ContainsKey("Name"))
			{
				this._paramDefaults.Parameters["JobName"] = this.PSWorkflowContext.JobMetadata["Name"];
			}
			if (this.PSWorkflowContext.JobMetadata.ContainsKey("InstanceId"))
			{
				this._paramDefaults.Parameters["JobInstanceId"] = this.PSWorkflowContext.JobMetadata["InstanceId"];
			}
			if (this.PSWorkflowContext.JobMetadata.ContainsKey("Id"))
			{
				this._paramDefaults.Parameters["JobId"] = this.PSWorkflowContext.JobMetadata["Id"];
			}
			if (this.PSWorkflowContext.JobMetadata.ContainsKey("Command"))
			{
				this._paramDefaults.Parameters["JobCommandName"] = this.PSWorkflowContext.JobMetadata["Command"];
			}
			if (this.PSWorkflowContext.JobMetadata.ContainsKey("ParentName"))
			{
				this._paramDefaults.Parameters["ParentJobName"] = this.PSWorkflowContext.JobMetadata["ParentName"];
			}
			if (this.PSWorkflowContext.JobMetadata.ContainsKey("ParentInstanceId"))
			{
				this._paramDefaults.Parameters["ParentJobInstanceId"] = this.PSWorkflowContext.JobMetadata["ParentInstanceId"];
			}
			if (this.PSWorkflowContext.JobMetadata.ContainsKey("ParentSessionId"))
			{
				this._paramDefaults.Parameters["ParentJobId"] = this.PSWorkflowContext.JobMetadata["ParentSessionId"];
			}
			if (this.PSWorkflowContext.JobMetadata.ContainsKey("ParentCommand"))
			{
				this._paramDefaults.Parameters["ParentCommandName"] = this.PSWorkflowContext.JobMetadata["ParentCommand"];
			}
			this._paramDefaults.Parameters["WorkflowInstanceId"] = this.InstanceId;
			this._paramDefaults.Parameters["Input"] = this.Streams.InputStream;
			this._paramDefaults.Parameters["Result"] = this.Streams.OutputStream;
			this._paramDefaults.Parameters["PSError"] = this.Streams.ErrorStream;
			this._paramDefaults.Parameters["PSWarning"] = this.Streams.WarningStream;
			this._paramDefaults.Parameters["PSProgress"] = this.Streams.ProgressStream;
			this._paramDefaults.Parameters["PSVerbose"] = this.Streams.VerboseStream;
			this._paramDefaults.Parameters["PSDebug"] = this.Streams.DebugStream;
			this._paramDefaults.Runtime = base.Runtime;
			this._paramDefaults.JobInstanceId = this._job.InstanceId;
			Func<bool> func1 = new Func<bool>(this.CheckForPersistenceAfterPSActivity);
			this._paramDefaults.HostPersistenceDelegate = func1;
			Action<object> action = new Action<object>(this.ReactivateWorkflow);
			this._paramDefaults.ActivateDelegate = action;
			this._paramDefaults.AsyncExecutionCollection = this.asyncExecutionCollection;
			SymbolResolver symbolResolvers = new SymbolResolver();
			symbolResolvers.Add("ParameterDefaults", this._paramDefaults);
			this.workflowApplication.Extensions.Add(symbolResolvers);
			this.workflowApplication.Extensions.Add(this._paramDefaults);
		}

		private void ConfigureTimerOnUnload()
		{
			if (this.Timer != null)
			{
				this.Timer.StopTimer(WorkflowTimerType.RunningTimer);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (!disposing || base.Disposed)
			{
				return;
			}
			else
			{
				lock (base.SyncLock)
				{
					if (!base.Disposed)
					{
						base.Disposed = true;
						lock (this.ReactivateSync)
						{
							this.IsTerminalStateAction = true;
							this.PersistIdleTimerInProgressOrTriggered = false;
							this.InternalUnloaded = false;
						}
						this.ConfigureTimerOnUnload();
						WorkflowApplication workflowApplication = this.workflowApplication;
						this.DisposeWorkflowApplication();
						if (this._job.JobStateInfo.State == JobState.Running && workflowApplication != null)
						{
							try
							{
								workflowApplication.Abort("Disposing the job");
							}
							catch (Exception exception)
							{
							}
						}
						if (this._paramDefaults != null)
						{
							this._paramDefaults.Dispose();
							this._paramDefaults = null;
						}
						if (this._streams != null)
						{
							this.UnregisterHandlersForDataAdding(this._streams);
							this._streams.Dispose();
						}
						if (this._timers != null)
						{
							this._timers.Dispose();
						}
						this.DisposePersistUnloadTimer();
						base.Dispose(disposing);
					}
				}
				return;
			}
		}

		private void DisposePersistUnloadTimer()
		{
			if (this.PersistUnloadTimer != null)
			{
				lock (this.ReactivateSync)
				{
					if (this.PersistUnloadTimer != null)
					{
						this.PersistUnloadTimer.Elapsed -= new ElapsedEventHandler(this.PersistUnloadTimer_Elapsed);
						this.PersistUnloadTimer.Dispose();
						this.PersistUnloadTimer = null;
					}
				}
				return;
			}
			else
			{
				return;
			}
		}

		public override void DisposeStreams()
		{
			if (this._streams != null)
			{
				lock (base.SyncLock)
				{
					if (this._streams != null)
					{
						this._streamsDisposed = true;
						this.UnregisterHandlersForDataAdding(this._streams);
						this._streams.Dispose();
						this._streams = null;
					}
				}
				return;
			}
			else
			{
				return;
			}
		}

		private void DisposeWorkflowApplication()
		{
			if (this.workflowApplication != null)
			{
				WorkflowApplication workflowApplication = this.workflowApplication;
				workflowApplication.Completed = (Action<WorkflowApplicationCompletedEventArgs>)Delegate.Remove(workflowApplication.Completed, new Action<WorkflowApplicationCompletedEventArgs>(this.HandleWorkflowApplicationCompleted));
				WorkflowApplication workflowApplication1 = this.workflowApplication;
				workflowApplication1.Aborted = (Action<WorkflowApplicationAbortedEventArgs>)Delegate.Remove(workflowApplication1.Aborted, new Action<WorkflowApplicationAbortedEventArgs>(this.HandleWorkflowApplicationAborted));
				WorkflowApplication workflowApplication2 = this.workflowApplication;
				workflowApplication2.OnUnhandledException = (Func<WorkflowApplicationUnhandledExceptionEventArgs, UnhandledExceptionAction>)Delegate.Remove(workflowApplication2.OnUnhandledException, new Func<WorkflowApplicationUnhandledExceptionEventArgs, UnhandledExceptionAction>(this.HandleWorkflowApplicationUnhandledException));
				WorkflowApplication workflowApplication3 = this.workflowApplication;
				workflowApplication3.PersistableIdle = (Func<WorkflowApplicationIdleEventArgs, PersistableIdleAction>)Delegate.Remove(workflowApplication3.PersistableIdle, new Func<WorkflowApplicationIdleEventArgs, PersistableIdleAction>(this.HandleWorkflowApplicationPersistableIdle));
				WorkflowApplication workflowApplication4 = this.workflowApplication;
				workflowApplication4.Idle = (Action<WorkflowApplicationIdleEventArgs>)Delegate.Remove(workflowApplication4.Idle, new Action<WorkflowApplicationIdleEventArgs>(this.HandleWorkflowApplicationIdle));
				WorkflowApplication workflowApplication5 = this.workflowApplication;
				workflowApplication5.Unloaded = (Action<WorkflowApplicationEventArgs>)Delegate.Remove(workflowApplication5.Unloaded, new Action<WorkflowApplicationEventArgs>(this.HandleWorkflowApplicationUnloaded));
				this.workflowApplication = null;
			}
		}

		protected override void DoAbortInstance(string reason)
		{
			if (!base.Disposed)
			{
				this.workflowApplication.Abort(reason);
				return;
			}
			else
			{
				return;
			}
		}

		protected override void DoCreateInstance()
		{
			WorkflowApplication workflowApplication;
			this.CheckDisposed();
			PSWorkflowApplicationInstance._structuredTracer.LoadingWorkflowForExecution(this.id);
			lock (base.SyncLock)
			{
				this.Tracer.WriteMessage("Loading Workflow");
				if (this.PSWorkflowDefinition.Workflow != null)
				{
					PSWorkflowApplicationInstance pSWorkflowApplicationInstance = this;
					if (this.PSWorkflowContext.WorkflowParameters == null)
					{
						workflowApplication = new WorkflowApplication(this.PSWorkflowDefinition.Workflow);
					}
					else
					{
						workflowApplication = new WorkflowApplication(this.PSWorkflowDefinition.Workflow, this.PSWorkflowContext.WorkflowParameters);
					}
					pSWorkflowApplicationInstance.workflowApplication = workflowApplication;
					this.wfAppNeverLoaded = false;
					this.id = this.workflowApplication.Id;
					this.SubscribeWorkflowApplicationEvents();
					this.ConfigureAllExtensions();
					this.SetupTimers();
					this.PersistBeforeExecution();
					this.InternalUnloaded = false;
					this.Tracer.WriteMessage(string.Concat("Workflow is loaded, Guid = ", this.id.ToString("D", CultureInfo.CurrentCulture)));
				}
				else
				{
					ArgumentException argumentException = new ArgumentException(Resources.NoWorkflowProvided);
					this.Tracer.TraceException(argumentException);
					throw argumentException;
				}
			}
			PSWorkflowApplicationInstance._structuredTracer.WorkflowLoadedForExecution(this.id);
		}

		protected override void DoExecuteInstance()
		{
			if (!base.Disposed)
			{
				this.Tracer.WriteMessage("Starting workflow execution");
				PSWorkflowApplicationInstance._structuredTracer.BeginWorkflowExecution(this._job.InstanceId);
				try
				{
					this.workflowApplication.Run();
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					this.HandleWorkflowApplicationFaultedState(exception);
					return;
				}
				this.StartTimers();
				this.Tracer.WriteMessage("Workflow application started execution");
				return;
			}
			else
			{
				return;
			}
		}

		protected override PSPersistableIdleAction DoGetPersistableIdleAction(ReadOnlyCollection<BookmarkInfo> bookmarks, bool externalSuspendRequest)
		{
			if (bookmarks.Count != 0)
			{
				Collection<BookmarkInfo> bookmarkInfos = new Collection<BookmarkInfo>();
				foreach (BookmarkInfo bookmark in bookmarks)
				{
					bookmarkInfos.Add(bookmark);
				}
				if (!this.VerifyRequest(bookmarkInfos, PSActivity.PSSuspendBookmarkPrefix))
				{
					if (!this.VerifyRequest(bookmarkInfos, PSActivity.PSPersistBookmarkPrefix))
					{
						if (bookmarkInfos.Count <= 0)
						{
							return PSPersistableIdleAction.None;
						}
						else
						{
							return PSPersistableIdleAction.Unload;
						}
					}
					else
					{
						if (!externalSuspendRequest)
						{
							return PSPersistableIdleAction.Persist;
						}
						else
						{
							return PSPersistableIdleAction.Suspend;
						}
					}
				}
				else
				{
					return PSPersistableIdleAction.Suspend;
				}
			}
			else
			{
				return PSPersistableIdleAction.None;
			}
		}

		internal override void DoLoadInstanceForReactivation()
		{
			this.CheckDisposed();
			try
			{
				lock (base.SyncLock)
				{
					this.Tracer.WriteMessage("Loading for Workflow resumption.");
					if (this.PSWorkflowDefinition.Workflow != null)
					{
						this.workflowApplication = new WorkflowApplication(this.PSWorkflowDefinition.Workflow);
						this.wfAppNeverLoaded = false;
						this.SubscribeWorkflowApplicationEvents();
						this.ConfigureAllExtensions();
						this.workflowApplication.Load(this.id);
						this.InternalUnloaded = false;
						this.Tracer.WriteMessage(string.Concat("Workflow is loaded for reactivation, Guid = ", this.id.ToString("D", CultureInfo.CurrentCulture)));
					}
					else
					{
						ArgumentException argumentException = new ArgumentException(Resources.NoWorkflowProvided);
						this.Tracer.TraceException(argumentException);
						throw argumentException;
					}
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				this.Tracer.WriteMessage("PSWorkflowApplicationInstance", "DoLoadInstanceForReactivation", this.id, "There has been an exception while loading the workflow state from persistence store.", new string[0]);
				this.Tracer.TraceException(exception);
				this.HandleWorkflowApplicationFaultedState(exception);
				throw;
			}
		}

		protected override void DoPersistInstance()
		{
			if (this._job.JobStateInfo.State == JobState.Completed || this._job.JobStateInfo.State == JobState.Stopped || this._job.JobStateInfo.State == JobState.Failed)
			{
				this._stores.Save(WorkflowStoreComponents.Streams | WorkflowStoreComponents.Metadata | WorkflowStoreComponents.JobState | WorkflowStoreComponents.TerminatingError);
				return;
			}
			else
			{
				this.PersistAfterNextPSActivity = true;
				return;
			}
		}

		protected override void DoRemoveInstance()
		{
			this.CheckDisposed();
			this._stores.Delete();
		}

		protected override void DoResumeBookmark(Bookmark bookmark, object state)
		{
			if (bookmark != null)
			{
				this.ReactivateWorkflowInternal(bookmark, state, true);
				return;
			}
			else
			{
				throw new ArgumentNullException("bookmark");
			}
		}

		protected override void DoResumeInstance(string label)
		{
			string pSBookmarkPrefix;
			this.Tracer.WriteMessage("Trying to resume workflow");
			this.IsTerminalStateAction = false;
			this.suspendAtNextCheckpoint = false;
			if (string.IsNullOrEmpty(label))
			{
				pSBookmarkPrefix = PSActivity.PSBookmarkPrefix;
			}
			else
			{
				pSBookmarkPrefix = string.Concat(PSActivity.PSSuspendBookmarkPrefix, label);
			}
			string str = pSBookmarkPrefix;
			ReadOnlyCollection<BookmarkInfo> bookmarks = this.workflowApplication.GetBookmarks();
			if (bookmarks.Count <= 0)
			{
				this.workflowApplication.Run();
			}
			else
			{
				foreach (BookmarkInfo bookmark in bookmarks)
				{
					if (!bookmark.BookmarkName.StartsWith(str, StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}
					Bookmark bookmark1 = new Bookmark(bookmark.BookmarkName);
					this.workflowApplication.ResumeBookmark(bookmark1, ActivityOnResumeAction.Restart);
				}
			}
			this.StartTimerOnResume();
			this.Tracer.WriteMessage("Workflow resumed");
		}

		protected override void DoStopInstance()
		{
			lock (this.ReactivateSync)
			{
				this.IsTerminalStateAction = true;
				this.PersistIdleTimerInProgressOrTriggered = false;
			}
			if (this.workflowApplication == null)
			{
				this.StopBookMarkedWorkflow();
				return;
			}
			else
			{
				this.workflowApplication.Cancel();
				return;
			}
		}

		protected override void DoSuspendInstance(bool notStarted)
		{
			if (!notStarted || !this.CheckForStartOrEndPersistence())
			{
				this.suspendAtNextCheckpoint = true;
				return;
			}
			else
			{
				this.workflowApplication.BeginUnload(new AsyncCallback(this.OnSuspendUnloadComplete), null);
				return;
			}
		}

		protected override void DoTerminateInstance(string reason)
		{
			this.workflowApplication.Terminate(reason);
		}

		private ErrorRecord GetInnerErrorRecord(Exception exception)
		{
			IContainsErrorRecord containsErrorRecord = exception as IContainsErrorRecord;
			if (containsErrorRecord != null)
			{
				return containsErrorRecord.ErrorRecord;
			}
			else
			{
				return null;
			}
		}

		private PSWorkflowTrackingParticipant GetTrackingParticipant()
		{
			PSWorkflowTrackingParticipant pSWorkflowTrackingParticipant = new PSWorkflowTrackingParticipant();
			TrackingProfile trackingProfile = new TrackingProfile();
			trackingProfile.Name = "WorkflowTrackingProfile";
			CustomTrackingQuery customTrackingQuery = new CustomTrackingQuery();
			customTrackingQuery.Name = "*";
			customTrackingQuery.ActivityName = "*";
			trackingProfile.Queries.Add(customTrackingQuery);
			WorkflowInstanceQuery workflowInstanceQuery = new WorkflowInstanceQuery();
			workflowInstanceQuery.States.Add("Started");
			workflowInstanceQuery.States.Add("Completed");
			workflowInstanceQuery.States.Add("Persisted");
			workflowInstanceQuery.States.Add("UnhandledException");
			trackingProfile.Queries.Add(workflowInstanceQuery);
			ActivityStateQuery activityStateQuery = new ActivityStateQuery();
			activityStateQuery.ActivityName = "*";
			activityStateQuery.States.Add("*");
			activityStateQuery.Variables.Add("*");
			trackingProfile.Queries.Add(activityStateQuery);
			pSWorkflowTrackingParticipant.TrackingProfile = trackingProfile;
			PSWorkflowTrackingParticipant pSWorkflowTrackingParticipant1 = pSWorkflowTrackingParticipant;
			return pSWorkflowTrackingParticipant1;
		}

		private void HandleErrorDataAdding(object sender, DataAddingEventArgs e)
		{
			ErrorRecord itemAdded = (ErrorRecord)e.ItemAdded;
			if (itemAdded != null)
			{
				PSActivity.AddIdentifierInfoToErrorRecord(itemAdded, "localhost", this.JobInstanceId);
				return;
			}
			else
			{
				return;
			}
		}

		private void HandleInformationalDataAdding(object sender, DataAddingEventArgs e)
		{
			InformationalRecord itemAdded = (InformationalRecord)e.ItemAdded;
			if (itemAdded != null)
			{
				itemAdded.Message = PSActivity.AddIdentifierInfoToString(this.JobInstanceId, "localhost", itemAdded.Message);
				return;
			}
			else
			{
				return;
			}
		}

		private void HandleOutputDataAdding(object sender, DataAddingEventArgs e)
		{
			PSObject itemAdded = (PSObject)e.ItemAdded;
			if (itemAdded != null)
			{
				PSActivity.AddIdentifierInfoToOutput(itemAdded, this.JobInstanceId, "localhost");
				return;
			}
			else
			{
				return;
			}
		}

		private void HandlePersistence(object state)
		{
			if (!base.Disposed)
			{
				lock (this.ReactivateSync)
				{
					if (!base.Disposed)
					{
						if (!this.IsTerminalStateAction)
						{
							if (this._job.JobStateInfo.State == JobState.Running && this.workflowApplication != null)
							{
								try
								{
									this.Tracer.WriteMessage("PSWorkflowApplicationInstance", "HandlePersistence", this.id, "Persisting the workflow.", new string[0]);
									this.workflowApplication.Persist();
								}
								catch (Exception exception1)
								{
									Exception exception = exception1;
									this.Tracer.TraceException(exception);
									this.SafelyHandleFaultedState(exception);
									return;
								}
								try
								{
									foreach (BookmarkInfo bookmark in this.workflowApplication.GetBookmarks())
									{
										if (!bookmark.BookmarkName.Contains(PSActivity.PSPersistBookmarkPrefix))
										{
											continue;
										}
										this.workflowApplication.ResumeBookmark(bookmark.BookmarkName, null);
									}
								}
								catch (Exception exception3)
								{
									Exception exception2 = exception3;
									this.Tracer.WriteMessage("PSWorkflowApplicationInstance", "HandlePersistence", this.id, "There has been exception while persisting the workflow in the background thread.", new string[0]);
									this.Tracer.TraceException(exception2);
								}
							}
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

		private void HandleProgressDataAdding(object sender, DataAddingEventArgs e)
		{
			ProgressRecord itemAdded = (ProgressRecord)e.ItemAdded;
			if (itemAdded != null)
			{
				itemAdded.CurrentOperation = PSActivity.AddIdentifierInfoToString(this.JobInstanceId, "localhost", itemAdded.CurrentOperation);
				return;
			}
			else
			{
				return;
			}
		}

		private void HandleWorkflowApplicationAborted(WorkflowApplicationAbortedEventArgs e)
		{
			if (!base.Disposed)
			{
				PSWorkflowApplicationInstance._structuredTracer.Correlate();
				this.Tracer.WriteMessage("Workflow Application is completed in Aborted state.");
				if (!this.callSuspendDelegate)
				{
					if (this._job.JobStateInfo.State != JobState.Stopping)
					{
						this.Error = e.Reason;
						this.PerformCleanupAtTerminalState();
						if (base.OnAborted != null)
						{
							base.OnAborted(e.Reason, this);
						}
						return;
					}
					else
					{
						this.HandleWorkflowApplicationCanceled();
						return;
					}
				}
				else
				{
					this.callSuspendDelegate = false;
					this.HandleWorkflowApplicationFaultedState(e.Reason);
					return;
				}
			}
			else
			{
				return;
			}
		}

		private void HandleWorkflowApplicationCanceled()
		{
			if (Interlocked.CompareExchange(ref this.terminalStateHandled, 1, 0) != 1)
			{
				this.Tracer.WriteMessage("Workflow Application is completed in Canceled state.");
				this.State = JobState.Stopped;
				this.PerformTaskAtTerminalState();
				this.PerformCleanupAtTerminalState();
				if (base.OnStopped != null)
				{
					base.OnStopped(this);
				}
				return;
			}
			else
			{
				return;
			}
		}

		private void HandleWorkflowApplicationCompleted(WorkflowApplicationCompletedEventArgs e)
		{
			if (!base.Disposed)
			{
				PSWorkflowApplicationInstance._structuredTracer.Correlate();
				if (e.CompletionState == ActivityInstanceState.Closed)
				{
					if (Interlocked.CompareExchange(ref this.terminalStateHandled, 1, 0) != 1)
					{
						try
						{
							this.Tracer.WriteMessage("Workflow Application is completed and is in closed state.");
							this.Tracer.WriteMessage("Flatting out the PSDataCollection returned outputs.");
							foreach (KeyValuePair<string, object> output in e.Outputs)
							{
								if (output.Value == null)
								{
									continue;
								}
								if (output.Value as PSDataCollection<PSObject> == null)
								{
									if (output.Value as PSDataCollection<ErrorRecord> == null)
									{
										if (output.Value as PSDataCollection<WarningRecord> == null)
										{
											if (output.Value as PSDataCollection<ProgressRecord> == null)
											{
												if (output.Value as PSDataCollection<VerboseRecord> == null)
												{
													if (output.Value as PSDataCollection<DebugRecord> == null)
													{
														this.Streams.OutputStream.Add(PSObject.AsPSObject(output.Value));
													}
													else
													{
														PSDataCollection<DebugRecord> value = output.Value as PSDataCollection<DebugRecord>;
														foreach (DebugRecord debugRecord in value)
														{
															this.Streams.OutputStream.Add(PSObject.AsPSObject(debugRecord));
														}
														value.Clear();
													}
												}
												else
												{
													PSDataCollection<VerboseRecord> verboseRecords = output.Value as PSDataCollection<VerboseRecord>;
													foreach (VerboseRecord verboseRecord in verboseRecords)
													{
														this.Streams.OutputStream.Add(PSObject.AsPSObject(verboseRecord));
													}
													verboseRecords.Clear();
												}
											}
											else
											{
												PSDataCollection<ProgressRecord> progressRecords = output.Value as PSDataCollection<ProgressRecord>;
												foreach (ProgressRecord progressRecord in progressRecords)
												{
													this.Streams.OutputStream.Add(PSObject.AsPSObject(progressRecord));
												}
												progressRecords.Clear();
											}
										}
										else
										{
											PSDataCollection<WarningRecord> warningRecords = output.Value as PSDataCollection<WarningRecord>;
											foreach (WarningRecord warningRecord in warningRecords)
											{
												this.Streams.OutputStream.Add(PSObject.AsPSObject(warningRecord));
											}
											warningRecords.Clear();
										}
									}
									else
									{
										PSDataCollection<ErrorRecord> errorRecords = output.Value as PSDataCollection<ErrorRecord>;
										foreach (ErrorRecord errorRecord in errorRecords)
										{
											this.Streams.OutputStream.Add(PSObject.AsPSObject(errorRecord));
										}
										errorRecords.Clear();
									}
								}
								else
								{
									PSDataCollection<PSObject> pSObjects = output.Value as PSDataCollection<PSObject>;
									foreach (PSObject pSObject in pSObjects)
									{
										this.Streams.OutputStream.Add(pSObject);
									}
									pSObjects.Clear();
								}
							}
						}
						catch (Exception exception1)
						{
							Exception exception = exception1;
							this.Tracer.TraceException(exception);
						}
						this.State = JobState.Completed;
						this.PerformTaskAtTerminalState();
						this.PerformCleanupAtTerminalState();
						if (base.OnCompleted != null)
						{
							base.OnCompleted(this);
						}
					}
					else
					{
						return;
					}
				}
				if (e.CompletionState == ActivityInstanceState.Faulted)
				{
					this.HandleWorkflowApplicationFaultedState(e.TerminationException);
				}
				if (e.CompletionState == ActivityInstanceState.Canceled)
				{
					this.HandleWorkflowApplicationCanceled();
				}
				return;
			}
			else
			{
				return;
			}
		}

		private void HandleWorkflowApplicationFaultedState(Exception e)
		{
			if (Interlocked.CompareExchange(ref this.terminalStateHandled, 1, 0) != 1)
			{
				this.Tracer.WriteMessage("Workflow Application is completed in Faulted state.");
				if (!WorkflowJobSourceAdapter.GetInstance().IsShutdownInProgress || !(e.GetType() == typeof(RemoteException)))
				{
					this.StopAllAsyncExecutions();
					this.State = JobState.Failed;
					Exception jobFailedException = e;
					ErrorRecord innerErrorRecord = this.GetInnerErrorRecord(jobFailedException);
					if (innerErrorRecord == null)
					{
						HostSettingCommandMetadata hostCommandMetadata = null;
						if (this._paramDefaults != null)
						{
							hostCommandMetadata = this._paramDefaults.HostCommandMetadata;
						}
						if (hostCommandMetadata != null)
						{
							ScriptPosition scriptPosition = new ScriptPosition(hostCommandMetadata.CommandName, hostCommandMetadata.StartLineNumber, hostCommandMetadata.StartColumnNumber, null);
							ScriptPosition scriptPosition1 = new ScriptPosition(hostCommandMetadata.CommandName, hostCommandMetadata.EndLineNumber, hostCommandMetadata.EndColumnNumber, null);
							ScriptExtent scriptExtent = new ScriptExtent(scriptPosition, scriptPosition1);
							jobFailedException = new JobFailedException(jobFailedException, scriptExtent);
						}
					}
					else
					{
						jobFailedException = innerErrorRecord.Exception;
						if (this.PSWorkflowJob.SynchronousExecution && this.Streams.ErrorStream.IsOpen)
						{
							this.Streams.ErrorStream.Add(innerErrorRecord);
							jobFailedException = null;
						}
					}
					this.Error = jobFailedException;
					this.PerformTaskAtTerminalState();
					this.PerformCleanupAtTerminalState();
					if (base.OnFaulted != null)
					{
						base.OnFaulted(jobFailedException, this);
					}
					return;
				}
				else
				{
					this.Tracer.WriteMessage("PSWorkflowApplicationInstance", "HandleWorkflowApplicationFaultedState", this.id, "Sicne we are in shuting down mode so ignoring the remote exception", new string[0]);
					this.Tracer.TraceException(e);
					return;
				}
			}
			else
			{
				return;
			}
		}

		private void HandleWorkflowApplicationIdle(WorkflowApplicationIdleEventArgs e)
		{
			if (!base.Disposed)
			{
				PSWorkflowApplicationInstance._structuredTracer.Correlate();
				this.Tracer.WriteMessage("Workflow Application is idle.");
				if (this._job.JobStateInfo.State != JobState.Stopping)
				{
					if (base.OnIdle != null)
					{
						base.OnIdle(e.Bookmarks, this);
					}
					return;
				}
				else
				{
					this.StopBookMarkedWorkflow();
					return;
				}
			}
			else
			{
				return;
			}
		}

		private PersistableIdleAction HandleWorkflowApplicationPersistableIdle(WorkflowApplicationIdleEventArgs e)
		{
			if (!base.Disposed)
			{
				PSWorkflowApplicationInstance._structuredTracer.Correlate();
				PSPersistableIdleAction bookmarks = PSPersistableIdleAction.NotDefined;
				if (base.OnPersistableIdleAction != null)
				{
					bookmarks = base.OnPersistableIdleAction(e.Bookmarks, this.suspendAtNextCheckpoint, this);
				}
				if (bookmarks == PSPersistableIdleAction.NotDefined)
				{
					bookmarks = this._job.GetPersistableIdleAction(e.Bookmarks, this.suspendAtNextCheckpoint);
				}
				PSPersistableIdleAction pSPersistableIdleAction = bookmarks;
				switch (pSPersistableIdleAction)
				{
					case PSPersistableIdleAction.None:
					{
						return PersistableIdleAction.None;
					}
					case PSPersistableIdleAction.Persist:
					{
						ThreadPool.QueueUserWorkItem(new WaitCallback(this.HandlePersistence));
						return PersistableIdleAction.None;
					}
					case PSPersistableIdleAction.Unload:
					{
						if (base.Runtime.Configuration.PSWorkflowApplicationPersistUnloadTimeoutSec > 0)
						{
							this.StartPersistUnloadTimer(base.Runtime.Configuration.PSWorkflowApplicationPersistUnloadTimeoutSec);
							return PersistableIdleAction.None;
						}
						else
						{
							this.StartPersistUnloadWithZeroSeconds();
							return PersistableIdleAction.Unload;
						}
					}
					case PSPersistableIdleAction.Suspend:
					{
						this.callSuspendDelegate = true;
						return PersistableIdleAction.Unload;
					}
				}
				return PersistableIdleAction.None;
			}
			else
			{
				return PersistableIdleAction.None;
			}
		}

		private UnhandledExceptionAction HandleWorkflowApplicationUnhandledException(WorkflowApplicationUnhandledExceptionEventArgs e)
		{
			if (!base.Disposed)
			{
				PSWorkflowApplicationInstance._structuredTracer.Correlate();
				this.Tracer.WriteMessage("Workflow Application is completed in Unhandled exception state.");
				if (this.PSWorkflowContext.PSWorkflowCommonParameters.ContainsKey("PSWorkflowErrorAction") && this.PSWorkflowContext.PSWorkflowCommonParameters["PSWorkflowErrorAction"] != null)
				{
					WorkflowUnhandledErrorAction item = (WorkflowUnhandledErrorAction)this.PSWorkflowContext.PSWorkflowCommonParameters["PSWorkflowErrorAction"];
					WorkflowUnhandledErrorAction workflowUnhandledErrorAction = item;
					switch (workflowUnhandledErrorAction)
					{
						case WorkflowUnhandledErrorAction.Suspend:
						{
							return UnhandledExceptionAction.Abort;
						}
						case WorkflowUnhandledErrorAction.Stop:
						{
							return UnhandledExceptionAction.Cancel;
						}
						case WorkflowUnhandledErrorAction.Terminate:
						{
							return UnhandledExceptionAction.Terminate;
						}
					}
				}
				return UnhandledExceptionAction.Terminate;
			}
			else
			{
				return UnhandledExceptionAction.Terminate;
			}
		}

		private void HandleWorkflowApplicationUnloaded(WorkflowApplicationEventArgs e)
		{
			if (!base.Disposed)
			{
				PSWorkflowApplicationInstance._structuredTracer.Correlate();
				this.Tracer.WriteMessage("Workflow Application is unloaded.");
				if (this.callSuspendDelegate)
				{
					this.callSuspendDelegate = false;
					if (base.OnSuspended != null)
					{
						base.OnSuspended(this);
					}
				}
				if (base.OnUnloaded != null)
				{
					base.OnUnloaded(this);
				}
				return;
			}
			else
			{
				return;
			}
		}

		private void OnSuspendUnloadComplete(IAsyncResult asyncResult)
		{
			if (!base.Disposed)
			{
				try
				{
					this.workflowApplication.EndUnload(asyncResult);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					this.Tracer.WriteMessage("PSWorkflowInstance", "DoSuspendInstance", this.id, "Not able to unload workflow application in a given timeout.", new string[0]);
					this.Tracer.TraceException(exception);
					this.HandleWorkflowApplicationFaultedState(exception);
					return;
				}
				this.ConfigureTimerOnUnload();
				this.DisposeWorkflowApplication();
				if (base.OnSuspended != null)
				{
					base.OnSuspended(this);
				}
				return;
			}
			else
			{
				return;
			}
		}

		private void OnUnloadComplete(IAsyncResult result)
		{
			if (!base.Disposed)
			{
				try
				{
					this.workflowApplication.EndUnload(result);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					this.Tracer.WriteMessage("PSWorkflowInstance", "PersistUnloadTimer_Elapsed", this.id, "Not able to unload workflow application in a given timeout.", new string[0]);
					this.Tracer.TraceException(exception);
					return;
				}
				this.DisposeWorkflowApplication();
				return;
			}
			else
			{
				return;
			}
		}

		private void PerformCleanupAtTerminalState()
		{
			this.DisposeWorkflowApplication();
			this.ConfigureTimerOnUnload();
			if (this._streams != null)
			{
				this.UnregisterHandlersForDataAdding(this._streams);
			}
		}

		internal override void PerformTaskAtTerminalState()
		{
			if (this.PSWorkflowDefinition != null && this.PSWorkflowDefinition.Workflow != null)
			{
				this.PSWorkflowDefinition.Workflow = null;
			}
			if (this.CheckForStartOrEndPersistence())
			{
				try
				{
					this._stores.Save(WorkflowStoreComponents.Streams | WorkflowStoreComponents.Metadata | WorkflowStoreComponents.Timer | WorkflowStoreComponents.JobState | WorkflowStoreComponents.TerminatingError);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					this.Tracer.WriteMessage("Serialization exception occurred while saving workflow to persistence store");
					this.Tracer.TraceException(exception);
					if (this.Streams.ErrorStream == null || !this.Streams.ErrorStream.IsOpen)
					{
						this.Tracer.WriteMessage("Error stream is not in Open state");
					}
					else
					{
						this.Streams.ErrorStream.Add(new ErrorRecord(exception, "Workflow_Serialization_Error", ErrorCategory.ParserError, null));
					}
				}
			}
			this.Streams.CloseAll();
			if (this.Timer != null)
			{
				this.Timer.StopTimer(WorkflowTimerType.RunningTimer);
			}
			PSWorkflowApplicationInstance._structuredTracer.EndWorkflowExecution(this._job.InstanceId);
		}

		private void PersistBeforeExecution()
		{
			if (this.CheckForStartOrEndPersistence())
			{
				try
				{
					this.workflowApplication.Persist();
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					this.Tracer.TraceException(exception);
					throw;
				}
			}
		}

		private void PersistUnloadTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			lock (this.ReactivateSync)
			{
				this.DisposePersistUnloadTimer();
				if (!this.IsTerminalStateAction)
				{
					if (this.PersistIdleTimerInProgressOrTriggered)
					{
						this.InternalUnloaded = true;
						if (this.workflowApplication != null)
						{
							try
							{
								this.OnUnloadComplete(this.workflowApplication.BeginUnload(null, null));
							}
							catch (Exception exception1)
							{
								Exception exception = exception1;
								this.Tracer.WriteMessage("PSWorkflowInstance", "PersistUnloadTimer_Elapsed", this.id, "Got an exception while unloading the workflow Application.", new string[0]);
								this.Tracer.TraceException(exception);
								return;
							}
						}
					}
				}
			}
		}

		private void ReactivateWorkflow(object state)
		{
			Bookmark bookmark = state as Bookmark;
			this.ReactivateWorkflowInternal(bookmark, null, false);
		}

		private void ReactivateWorkflowInternal(Bookmark bookmark, object state, bool validateBookmark)
		{
			if (!base.Disposed)
			{
				lock (this.ReactivateSync)
				{
					if (!base.Disposed)
					{
						if (!this.IsTerminalStateAction)
						{
							if (this.InternalUnloaded || this.wfAppNeverLoaded)
							{
								this.DoLoadInstanceForReactivation();
								this.InternalUnloaded = false;
							}
							this.PersistIdleTimerInProgressOrTriggered = false;
							if (this.workflowApplication != null)
							{
								if (!validateBookmark || this.CheckIfBookmarkExistInCollection(bookmark.Name, this.workflowApplication.GetBookmarks()))
								{
									try
									{
										this.workflowApplication.ResumeBookmark(bookmark, state);
									}
									catch (Exception exception1)
									{
										Exception exception = exception1;
										this.Tracer.TraceException(exception);
										this.HandleWorkflowApplicationFaultedState(exception);
									}
								}
								else
								{
									string[] name = new string[1];
									name[0] = bookmark.Name;
									this.Tracer.WriteMessage("PSWorkflowInstance", "ReactivateWorkflowInternal", this.id, "Invalid bookmark: '{0}'.", name);
									object[] objArray = new object[1];
									objArray[0] = bookmark.Name;
									throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.InvalidBookmark, objArray));
								}
							}
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

		private void RegisterHandlersForDataAdding(PowerShellStreams<PSObject, PSObject> streams)
		{
			streams.OutputStream.DataAdding += new EventHandler<DataAddingEventArgs>(this.HandleOutputDataAdding);
			streams.ErrorStream.DataAdding += new EventHandler<DataAddingEventArgs>(this.HandleErrorDataAdding);
			streams.DebugStream.DataAdding += new EventHandler<DataAddingEventArgs>(this.HandleInformationalDataAdding);
			streams.VerboseStream.DataAdding += new EventHandler<DataAddingEventArgs>(this.HandleInformationalDataAdding);
			streams.WarningStream.DataAdding += new EventHandler<DataAddingEventArgs>(this.HandleInformationalDataAdding);
			streams.ProgressStream.DataAdding += new EventHandler<DataAddingEventArgs>(this.HandleProgressDataAdding);
		}

		private void SafelyHandleFaultedState(Exception exception)
		{
			try
			{
				this.HandleWorkflowApplicationFaultedState(exception);
			}
			catch (Exception exception2)
			{
				Exception exception1 = exception2;
				this.Tracer.WriteMessage("PSWorkflowApplicationInstance", "SafelyHandleFaultedState", this.id, "There has been exception while marking the workflow in faulted state in the background thread.", new string[0]);
				this.Tracer.TraceException(exception1);
			}
		}

		internal override bool SaveStreamsIfNecessary()
		{
			bool flag;
			if (!this.CheckForStartOrEndPersistence())
			{
				try
				{
					this._stores.Save(WorkflowStoreComponents.Streams);
					return true;
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					this.Tracer.TraceException(exception);
					flag = false;
				}
				return flag;
			}
			else
			{
				return true;
			}
		}

		private void SetupTimers()
		{
			if (this.Timer != null && this.PSWorkflowContext.PSWorkflowCommonParameters != null)
			{
				if (this.PSWorkflowContext.PSWorkflowCommonParameters.ContainsKey("PSRunningTimeoutSec"))
				{
					int num = Convert.ToInt32(this.PSWorkflowContext.PSWorkflowCommonParameters["PSRunningTimeoutSec"], CultureInfo.CurrentCulture);
					if (num > 0)
					{
						this.Timer.SetupTimer(WorkflowTimerType.RunningTimer, TimeSpan.FromSeconds((double)num));
					}
				}
				if (this.PSWorkflowContext.PSWorkflowCommonParameters.ContainsKey("PSElapsedTimeoutSec"))
				{
					int num1 = Convert.ToInt32(this.PSWorkflowContext.PSWorkflowCommonParameters["PSElapsedTimeoutSec"], CultureInfo.CurrentCulture);
					if (num1 > 0)
					{
						this.Timer.SetupTimer(WorkflowTimerType.ElapsedTimer, TimeSpan.FromSeconds((double)num1));
					}
				}
				if (this.PSWorkflowContext.PSWorkflowCommonParameters.ContainsKey("PSPersist"))
				{
					Convert.ToBoolean(this.PSWorkflowContext.PSWorkflowCommonParameters["PSPersist"], CultureInfo.CurrentCulture);
				}
			}
		}

		private void StartPersistUnloadTimer(int delaySeconds)
		{
			lock (this.ReactivateSync)
			{
				if (!this.IsTerminalStateAction)
				{
					this.PersistIdleTimerInProgressOrTriggered = true;
					this.PersistUnloadTimer = new Timer(Convert.ToDouble(delaySeconds * 0x3e8));
					this.PersistUnloadTimer.Elapsed += new ElapsedEventHandler(this.PersistUnloadTimer_Elapsed);
					this.PersistUnloadTimer.Start();
				}
			}
		}

		private void StartPersistUnloadWithZeroSeconds()
		{
			lock (this.ReactivateSync)
			{
				if (!this.IsTerminalStateAction)
				{
					this.PersistIdleTimerInProgressOrTriggered = true;
					this.InternalUnloaded = true;
				}
			}
		}

		private void StartTimerOnResume()
		{
			if (this.Timer != null)
			{
				this.Timer.StartTimer(WorkflowTimerType.RunningTimer);
			}
		}

		private void StartTimers()
		{
			if (this.Timer != null)
			{
				this.Timer.StartTimer(WorkflowTimerType.RunningTimer);
				this.Timer.StartTimer(WorkflowTimerType.ElapsedTimer);
			}
		}

		private void StopAllAsyncExecutions()
		{
			if (!base.Disposed)
			{
				if (this.asyncExecutionCollection != null && this.asyncExecutionCollection.Count > 0)
				{
					foreach (PSActivityContext list in this.asyncExecutionCollection.Values.ToList<PSActivityContext>())
					{
						if (list == null)
						{
							continue;
						}
						list.IsCanceled = true;
						this.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PowerShell activity: Executing cancel request.", new object[0]));
						list.Cancel();
					}
					this.asyncExecutionCollection.Clear();
				}
				return;
			}
			else
			{
				return;
			}
		}

		private void StopBookMarkedWorkflow()
		{
			if (!base.Disposed)
			{
				if (Interlocked.CompareExchange(ref this.terminalStateHandled, 1, 0) != 1)
				{
					lock (this.ReactivateSync)
					{
						if (!base.Disposed)
						{
							this.IsTerminalStateAction = true;
							this.StopAllAsyncExecutions();
							this.Tracer.WriteMessage("Workflow is in Canceled state.");
							this.State = JobState.Stopped;
							this.PerformTaskAtTerminalState();
							this.PerformCleanupAtTerminalState();
							if (base.OnStopped != null)
							{
								base.OnStopped(this);
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
			else
			{
				return;
			}
		}

		private void SubscribeWorkflowApplicationEvents()
		{
			WorkflowApplication workflowApplication = this.workflowApplication;
			workflowApplication.Completed = (Action<WorkflowApplicationCompletedEventArgs>)Delegate.Combine(workflowApplication.Completed, new Action<WorkflowApplicationCompletedEventArgs>(this.HandleWorkflowApplicationCompleted));
			WorkflowApplication workflowApplication1 = this.workflowApplication;
			workflowApplication1.Aborted = (Action<WorkflowApplicationAbortedEventArgs>)Delegate.Combine(workflowApplication1.Aborted, new Action<WorkflowApplicationAbortedEventArgs>(this.HandleWorkflowApplicationAborted));
			WorkflowApplication workflowApplication2 = this.workflowApplication;
			workflowApplication2.OnUnhandledException = (Func<WorkflowApplicationUnhandledExceptionEventArgs, UnhandledExceptionAction>)Delegate.Combine(workflowApplication2.OnUnhandledException, new Func<WorkflowApplicationUnhandledExceptionEventArgs, UnhandledExceptionAction>(this.HandleWorkflowApplicationUnhandledException));
			WorkflowApplication workflowApplication3 = this.workflowApplication;
			workflowApplication3.PersistableIdle = (Func<WorkflowApplicationIdleEventArgs, PersistableIdleAction>)Delegate.Combine(workflowApplication3.PersistableIdle, new Func<WorkflowApplicationIdleEventArgs, PersistableIdleAction>(this.HandleWorkflowApplicationPersistableIdle));
			WorkflowApplication workflowApplication4 = this.workflowApplication;
			workflowApplication4.Idle = (Action<WorkflowApplicationIdleEventArgs>)Delegate.Combine(workflowApplication4.Idle, new Action<WorkflowApplicationIdleEventArgs>(this.HandleWorkflowApplicationIdle));
			WorkflowApplication workflowApplication5 = this.workflowApplication;
			workflowApplication5.Unloaded = (Action<WorkflowApplicationEventArgs>)Delegate.Combine(workflowApplication5.Unloaded, new Action<WorkflowApplicationEventArgs>(this.HandleWorkflowApplicationUnloaded));
		}

		private void UnregisterHandlersForDataAdding(PowerShellStreams<PSObject, PSObject> streams)
		{
			if (streams.OutputStream != null)
			{
				streams.OutputStream.DataAdding -= new EventHandler<DataAddingEventArgs>(this.HandleOutputDataAdding);
			}
			if (streams.ErrorStream != null)
			{
				streams.ErrorStream.DataAdding -= new EventHandler<DataAddingEventArgs>(this.HandleErrorDataAdding);
			}
			if (streams.DebugStream != null)
			{
				streams.DebugStream.DataAdding -= new EventHandler<DataAddingEventArgs>(this.HandleInformationalDataAdding);
			}
			if (streams.VerboseStream != null)
			{
				streams.VerboseStream.DataAdding -= new EventHandler<DataAddingEventArgs>(this.HandleInformationalDataAdding);
			}
			if (streams.WarningStream != null)
			{
				streams.WarningStream.DataAdding -= new EventHandler<DataAddingEventArgs>(this.HandleInformationalDataAdding);
			}
			if (streams.ProgressStream != null)
			{
				streams.ProgressStream.DataAdding -= new EventHandler<DataAddingEventArgs>(this.HandleProgressDataAdding);
			}
		}

		internal override void ValidateIfLabelExists(string label)
		{
			bool flag;
			string str = string.Concat(PSActivity.PSSuspendBookmarkPrefix, label);
			ReadOnlyCollection<BookmarkInfo> bookmarks = this.workflowApplication.GetBookmarks();
			foreach (BookmarkInfo bookmark in bookmarks)
			{
				if (!bookmark.BookmarkName.StartsWith(str, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				flag = true;
			}
			if (flag)
			{
				return;
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = label;
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.InvalidLabel, objArray));
			}
		}

		private bool VerifyRequest(Collection<BookmarkInfo> bookmarks, string prefix)
		{
			if (bookmarks == null || bookmarks.Count <= 0)
			{
				return false;
			}
			else
			{
				Collection<BookmarkInfo> bookmarkInfos = new Collection<BookmarkInfo>();
				bool flag = true;
				foreach (BookmarkInfo bookmark in bookmarks)
				{
					if (bookmark.BookmarkName.Contains(prefix))
					{
						bookmarkInfos.Add(bookmark);
					}
					else
					{
						flag = false;
					}
				}
				foreach (BookmarkInfo bookmarkInfo in bookmarkInfos)
				{
					bookmarks.Remove(bookmarkInfo);
				}
				return flag;
			}
		}
	}
}