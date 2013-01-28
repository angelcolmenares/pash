using Microsoft.PowerShell.Workflow;
using System;
using System.Activities;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Tracing;
using System.Threading;

namespace Microsoft.PowerShell.Activities
{
	[Serializable]
	public class PSActivityContext : IDisposable
	{
		[NonSerialized]
		private readonly PowerShellTraceSource _tracer;

		[NonSerialized]
		internal Dictionary<System.Management.Automation.PowerShell, RetryCount> runningCommands;

		[NonSerialized]
		internal ConcurrentQueue<ActivityImplementationContext> commandQueue;

		internal Dictionary<string, object> UserVariables;

		internal List<Exception> exceptions;

		internal PSDataCollection<ErrorRecord> errors;

		internal bool Failed;

		[NonSerialized]
		internal ActivityInstance runningCancelTimer;

		[NonSerialized]
		internal readonly ConcurrentQueue<IAsyncResult> AsyncResults;

		[NonSerialized]
		internal EventHandler<RunspaceStateEventArgs> HandleRunspaceStateChanged;

		[NonSerialized]
		internal PSDataCollection<ProgressRecord> progress;

		[NonSerialized]
		internal object SyncRoot;

		[NonSerialized]
		internal bool AllCommandsStarted;

		[NonSerialized]
		internal int CommandsRunningCount;

		[NonSerialized]
		internal WaitCallback Callback;

		[NonSerialized]
		internal object AsyncState;

		[NonSerialized]
		internal Guid JobInstanceId;

		[NonSerialized]
		internal ActivityParameters ActivityParams;

		[NonSerialized]
		internal PSDataCollection<PSObject> Input;

		[NonSerialized]
		internal PSDataCollection<PSObject> Output;

		[NonSerialized]
		internal PSWorkflowHost WorkflowHost;

		[NonSerialized]
		internal HostParameterDefaults HostExtension;

		[NonSerialized]
		internal bool RunInProc;

		[NonSerialized]
		internal bool MergeErrorToOutput;

		[NonSerialized]
		internal Dictionary<string, object> ParameterDefaults;

		[NonSerialized]
		internal Type ActivityType;

		[NonSerialized]
		internal PrepareSessionDelegate PrepareSession;

		[NonSerialized]
		internal object ActivityObject;

		public bool IsCanceled
		{
			get;
			set;
		}

		internal bool RunWithCustomRemoting
		{
			get;
			set;
		}

		internal Type TypeImplementingCmdlet
		{
			get;
			set;
		}

		public PSActivityContext()
		{
			this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
			this.UserVariables = new Dictionary<string, object>();
			this.exceptions = new List<Exception>();
			this.errors = new PSDataCollection<ErrorRecord>();
			this.AsyncResults = new ConcurrentQueue<IAsyncResult>();
			this.SyncRoot = new object();
		}

		public void Cancel()
		{
			ActivityImplementationContext activityImplementationContext = null;
			IAsyncResult asyncResult = null;
			if (this.WorkflowHost != null)
			{
				if (this.commandQueue != null)
				{
					while (!this.commandQueue.IsEmpty)
					{
						bool flag = this.commandQueue.TryDequeue(out activityImplementationContext);
						if (!flag)
						{
							continue;
						}
						PowerShell powerShellInstance = activityImplementationContext.PowerShellInstance;
						object[] objArray = new object[1];
						objArray[0] = powerShellInstance;
						this._tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PowerShell activity: Cancelling pending command {0}.", objArray));
						powerShellInstance.Dispose();
					}
				}
				PSResumableActivityHostController pSActivityHostController = this.WorkflowHost.PSActivityHostController as PSResumableActivityHostController;
				if (pSActivityHostController == null)
				{
					PSOutOfProcessActivityController pSOutOfProcessActivityController = this.WorkflowHost.PSActivityHostController as PSOutOfProcessActivityController;
					if (pSOutOfProcessActivityController != null)
					{
						while (this.AsyncResults.Count > 0)
						{
							this.AsyncResults.TryDequeue(out asyncResult);
							pSOutOfProcessActivityController.CancelInvokePowerShell(asyncResult);
						}
					}
				}
				else
				{
					pSActivityHostController.StopAllResumablePSCommands(this.JobInstanceId);
				}
				while (this.runningCommands.Count > 0)
				{
					PowerShell powerShell = null;
					lock (this.runningCommands)
					{
						Dictionary<PowerShell, RetryCount>.KeyCollection.Enumerator enumerator = this.runningCommands.Keys.GetEnumerator();
						try
						{
							if (enumerator.MoveNext())
							{
								PowerShell current = enumerator.Current;
								powerShell = current;
							}
						}
						finally
						{
							enumerator.Dispose();
						}
						if (powerShell == null)
						{
							break;
						}
					}
					if (powerShell.InvocationStateInfo.State == PSInvocationState.Running)
					{
						object[] objArray1 = new object[1];
						objArray1[0] = powerShell;
						this._tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PowerShell activity: Stopping command {0}.", objArray1));
						try
						{
							powerShell.Stop();
						}
						catch (NullReferenceException nullReferenceException)
						{
						}
						catch (InvalidOperationException invalidOperationException)
						{
						}
					}
					if (powerShell.InvocationStateInfo.State != PSInvocationState.Completed || powerShell.HadErrors)
					{
						this.Failed = true;
					}
					int num = RunCommandsArguments.DetermineCommandExecutionType(powerShell.Runspace.ConnectionInfo as WSManConnectionInfo, this.RunInProc, this.ActivityType, this);
					if (num != 1)
					{
						PSActivity.CloseRunspaceAndDisposeCommand(powerShell, this.WorkflowHost, this, num);
					}
					this.runningCommands.Remove(powerShell);
				}
				return;
			}
			else
			{
				throw new InvalidOperationException("WorkflowHost");
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				this._tracer.Dispose();
				this.Callback = null;
				this.PrepareSession = null;
				this.HandleRunspaceStateChanged = null;
				this.ActivityObject = null;
				this.ParameterDefaults = null;
				this.Input = null;
				this.Output = null;
				this.errors = null;
				this.progress = null;
				this.WorkflowHost = null;
				this.ActivityParams = null;
				this.exceptions = null;
				this.runningCommands = null;
				this.commandQueue = null;
				return;
			}
			else
			{
				return;
			}
		}

		public bool Execute()
		{
			ActivityImplementationContext activityImplementationContext = null;
			if (this.commandQueue != null)
			{
				bool flag = this.commandQueue.TryDequeue(out activityImplementationContext);
				lock (this.SyncRoot)
				{
					while (flag)
					{
						RunCommandsArguments runCommandsArgument = new RunCommandsArguments(this.ActivityParams, this.Output, this.Input, this, this.WorkflowHost, this.RunInProc, this.ParameterDefaults, this.ActivityType, this.PrepareSession, this.ActivityObject, activityImplementationContext);
						Interlocked.Increment(ref this.CommandsRunningCount);
						PSActivity.BeginRunOneCommand(runCommandsArgument);
						flag = this.commandQueue.TryDequeue(out activityImplementationContext);
					}
					this.AllCommandsStarted = true;
				}
				return true;
			}
			else
			{
				throw new InvalidOperationException("commandQueue");
			}
		}
	}
}