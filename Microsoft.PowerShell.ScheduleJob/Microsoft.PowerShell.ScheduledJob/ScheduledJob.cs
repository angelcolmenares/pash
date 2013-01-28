using Microsoft.PowerShell.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.PowerShell.ScheduledJob
{
	[Serializable]
	public sealed class ScheduledJob : Job2, ISerializable
	{
		private const string AllowHostSetShouldExit = "AllowSetShouldExitFromRemote";

		private ScheduledJobDefinition _jobDefinition;

		private Runspace _runspace;

		private System.Management.Automation.PowerShell _powerShell;

		private Job _job;

		private bool _asyncJobStop;

		private bool _allowSetShouldExit;

		private PSHost _host;

		private StatusInfo _statusInfo;

		internal bool AllowSetShouldExit
		{
			get
			{
				return this._allowSetShouldExit;
			}
			set
			{
				this._allowSetShouldExit = value;
			}
		}

		public string Command
		{
			get
			{
				return this.Status.Command;
			}
		}

		public ScheduledJobDefinition Definition
		{
			get
			{
				return this._jobDefinition;
			}
			internal set
			{
				this._jobDefinition = value;
			}
		}

		public override bool HasMoreData
		{
			get
			{
				if (this._job != null)
				{
					return this._job.HasMoreData;
				}
				else
				{
					if (base.Output.Count > 0 || base.Error.Count > 0 || base.Warning.Count > 0 || base.Verbose.Count > 0 || base.Progress.Count > 0)
					{
						return true;
					}
					else
					{
						return base.Debug.Count > 0;
					}
				}
			}
		}

		public override string Location
		{
			get
			{
				return this.Status.Location;
			}
		}

		private StatusInfo Status
		{
			get
			{
				StatusInfo statusInfo;
				lock (base.SyncRoot)
				{
					if (this._statusInfo == null)
					{
						if (this._job == null)
						{
							this._statusInfo = new StatusInfo(Guid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, JobState.NotStarted, false, base.PSBeginTime, base.PSEndTime, this._jobDefinition);
							statusInfo = this._statusInfo;
						}
						else
						{
							statusInfo = new StatusInfo(this._job.InstanceId, this._job.Name, this._job.Location, this._job.Command, this._job.StatusMessage, this._job.JobStateInfo.State, this._job.HasMoreData, base.PSBeginTime, base.PSEndTime, this._jobDefinition);
						}
					}
					else
					{
						statusInfo = this._statusInfo;
					}
				}
				return statusInfo;
			}
		}

		public override string StatusMessage
		{
			get
			{
				return this.Status.StatusMessage;
			}
		}

		public ScheduledJob(string command, string name, ScheduledJobDefinition jobDefinition) : base(command, name)
		{
			if (command != null)
			{
				if (name != null)
				{
					if (jobDefinition != null)
					{
						this._jobDefinition = jobDefinition;
						base.PSJobTypeName = "PSScheduledJob";
						return;
					}
					else
					{
						throw new PSArgumentNullException("jobDefinition");
					}
				}
				else
				{
					throw new PSArgumentNullException("name");
				}
			}
			else
			{
				throw new PSArgumentNullException("command");
			}
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		private ScheduledJob(SerializationInfo info, StreamingContext context)
		{
			if (info != null)
			{
				this.DeserializeStatusInfo(info);
				this.DeserializeResultsInfo(info);
				base.PSJobTypeName = "PSScheduledJob";
				return;
			}
			else
			{
				throw new PSArgumentNullException("info");
			}
		}

		private void AddSetShouldExitToHost()
		{
			if (!this._allowSetShouldExit || this._host == null)
			{
				return;
			}
			else
			{
				PSObject privateData = this._host.PrivateData;
				if (privateData != null)
				{
					privateData.Properties.Add(new PSNoteProperty("AllowSetShouldExitFromRemote", (object)(true)));
				}
				return;
			}
		}

		private void CopyDebug(ICollection<DebugRecord> fromDebug)
		{
			PSDataCollection<DebugRecord> debugRecords = this.CopyResults<DebugRecord>(fromDebug);
			if (debugRecords != null)
			{
				base.Debug = debugRecords;
			}
		}

		private void CopyError(ICollection<ErrorRecord> fromError)
		{
			PSDataCollection<ErrorRecord> errorRecords = this.CopyResults<ErrorRecord>(fromError);
			if (errorRecords != null)
			{
				base.Error = errorRecords;
			}
		}

		private void CopyOutput(ICollection<PSObject> fromOutput)
		{
			PSDataCollection<PSObject> pSObjects = this.CopyResults<PSObject>(fromOutput);
			if (pSObjects != null)
			{
				base.Output = pSObjects;
			}
		}

		private void CopyProgress(ICollection<ProgressRecord> fromProgress)
		{
			PSDataCollection<ProgressRecord> progressRecords = this.CopyResults<ProgressRecord>(fromProgress);
			if (progressRecords != null)
			{
				base.Progress = progressRecords;
			}
		}

		private PSDataCollection<T> CopyResults<T>(ICollection<T> fromResults)
		{
			if (fromResults == null || fromResults.Count <= 0)
			{
				return null;
			}
			else
			{
				PSDataCollection<T> ts = new PSDataCollection<T>();
				foreach (T fromResult in fromResults)
				{
					ts.Add(fromResult);
				}
				return ts;
			}
		}

		private void CopyVerbose(ICollection<VerboseRecord> fromVerbose)
		{
			PSDataCollection<VerboseRecord> verboseRecords = this.CopyResults<VerboseRecord>(fromVerbose);
			if (verboseRecords != null)
			{
				base.Verbose = verboseRecords;
			}
		}

		private void CopyWarning(ICollection<WarningRecord> fromWarning)
		{
			PSDataCollection<WarningRecord> warningRecords = this.CopyResults<WarningRecord>(fromWarning);
			if (warningRecords != null)
			{
				base.Warning = warningRecords;
			}
		}

		private void DeserializeResultsInfo(SerializationInfo info)
		{
			ScheduledJob.ResultsInfo value = (ScheduledJob.ResultsInfo)info.GetValue("ResultsInfo", typeof(ScheduledJob.ResultsInfo));
			this.CopyOutput(value.Output);
			this.CopyError(value.Error);
			this.CopyWarning(value.Warning);
			this.CopyVerbose(value.Verbose);
			this.CopyProgress(value.Progress);
			this.CopyDebug(value.Debug);
		}

		private void DeserializeStatusInfo(SerializationInfo info)
		{
			StatusInfo value = (StatusInfo)info.GetValue("StatusInfo", typeof(StatusInfo));
			base.Name = value.Name;
			base.PSBeginTime = value.StartTime;
			base.PSEndTime = value.StopTime;
			this._jobDefinition = value.Definition;
			base.SetJobState(value.State, null);
			lock (base.SyncRoot)
			{
				this._statusInfo = value;
			}
		}

		private PSHost GetDefaultHost()
		{
			System.Management.Automation.PowerShell powerShell = System.Management.Automation.PowerShell.Create(RunspaceMode.CurrentRunspace).AddScript("$host");
			Collection<PSHost> pSHosts = powerShell.Invoke<PSHost>();
			if (pSHosts == null || pSHosts.Count == 0)
			{
				return null;
			}
			else
			{
				return pSHosts[0];
			}
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info != null)
			{
				this.SerializeStatusInfo(info);
				this.SerializeResultsInfo(info);
				return;
			}
			else
			{
				throw new PSArgumentException("info");
			}
		}

		private void HandleJobStateChanged(object sender, JobStateEventArgs e)
		{
			base.SetJobState(e.JobStateInfo.State);
			if (this.IsFinishedState(e.JobStateInfo.State))
			{
				base.PSEndTime = new DateTime?(DateTime.Now);
				System.Management.Automation.PowerShell powerShell = null;
				Runspace runspace = null;
				lock (base.SyncRoot)
				{
					if (this._job != null && this.IsFinishedState(this._job.JobStateInfo.State))
					{
						this._powerShell = null;
						this._runspace = null;
					}
				}
				if (powerShell != null)
				{
					powerShell.Dispose();
				}
				if (runspace != null)
				{
					runspace.Dispose();
				}
				if (this._asyncJobStop)
				{
					this._asyncJobStop = false;
					this.OnStopJobCompleted(new AsyncCompletedEventArgs(null, false, null));
				}
				this.RemoveSetShouldExitFromHost();
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

		private void RemoveSetShouldExitFromHost()
		{
			if (!this._allowSetShouldExit || this._host == null)
			{
				return;
			}
			else
			{
				PSObject privateData = this._host.PrivateData;
				if (privateData != null)
				{
					privateData.Properties.Remove("AllowSetShouldExitFromRemote");
				}
				return;
			}
		}

		public override void ResumeJob()
		{
			throw new PSNotSupportedException();
		}

		public override void ResumeJobAsync()
		{
			throw new PSNotSupportedException();
		}

		private void SerializeResultsInfo(SerializationInfo info)
		{
			Collection<PSObject> pSObjects = new Collection<PSObject>();
			Collection<ErrorRecord> errorRecords = new Collection<ErrorRecord>();
			Collection<WarningRecord> warningRecords = new Collection<WarningRecord>();
			Collection<VerboseRecord> verboseRecords = new Collection<VerboseRecord>();
			Collection<ProgressRecord> progressRecords = new Collection<ProgressRecord>();
			Collection<DebugRecord> debugRecords = new Collection<DebugRecord>();
			if (this._job == null)
			{
				foreach (PSObject output in base.Output)
				{
					pSObjects.Add(new PSObject(output.BaseObject));
				}
				foreach (ErrorRecord error in base.Error)
				{
					errorRecords.Add(error);
				}
				foreach (WarningRecord warning in base.Warning)
				{
					warningRecords.Add(warning);
				}
				foreach (VerboseRecord verbose in base.Verbose)
				{
					verboseRecords.Add(verbose);
				}
				foreach (ProgressRecord progress in base.Progress)
				{
					progressRecords.Add(progress);
				}
				foreach (DebugRecord debug in base.Debug)
				{
					debugRecords.Add(debug);
				}
			}
			else
			{
				if (base.JobStateInfo.Reason != null)
				{
					errorRecords.Add(new ErrorRecord(base.JobStateInfo.Reason, "ScheduledJobFailedState", ErrorCategory.InvalidResult, null));
				}
				foreach (ErrorRecord errorRecord in this._job.Error)
				{
					errorRecords.Add(errorRecord);
				}
				foreach (Job job in this._job.ChildJobs)
				{
					if (job.JobStateInfo.Reason != null)
					{
						errorRecords.Add(new ErrorRecord(job.JobStateInfo.Reason, "ScheduledJobFailedState", ErrorCategory.InvalidResult, null));
					}
					foreach (PSObject pSObject in job.Output)
					{
						pSObjects.Add(pSObject);
					}
					foreach (ErrorRecord error1 in job.Error)
					{
						errorRecords.Add(error1);
					}
					foreach (WarningRecord warningRecord in job.Warning)
					{
						warningRecords.Add(warningRecord);
					}
					foreach (VerboseRecord verboseRecord in job.Verbose)
					{
						verboseRecords.Add(verboseRecord);
					}
					foreach (ProgressRecord progressRecord in job.Progress)
					{
						progressRecords.Add(progressRecord);
					}
					IEnumerator<DebugRecord> enumerator = job.Debug.GetEnumerator();
					using (enumerator)
					{
						while (enumerator.MoveNext())
						{
							DebugRecord debugRecord = enumerator.Current;
							debugRecords.Add(debugRecord);
						}
					}
				}
			}
			ScheduledJob.ResultsInfo resultsInfo = new ScheduledJob.ResultsInfo(pSObjects, errorRecords, warningRecords, verboseRecords, progressRecords, debugRecords);
			info.AddValue("ResultsInfo", resultsInfo);
		}

		private void SerializeStatusInfo(SerializationInfo info)
		{
			JobState state;
			Guid instanceId = base.InstanceId;
			string name = base.Name;
			string location = this.Location;
			string command = this.Command;
			string statusMessage = this.StatusMessage;
			if (this._job != null)
			{
				state = this._job.JobStateInfo.State;
			}
			else
			{
				state = base.JobStateInfo.State;
			}
			StatusInfo statusInfo = new StatusInfo(instanceId, name, location, command, statusMessage, state, this.HasMoreData, base.PSBeginTime, base.PSEndTime, this._jobDefinition);
			info.AddValue("StatusInfo", statusInfo);
		}

		public override void StartJob()
		{
			lock (base.SyncRoot)
			{
				if (this._job == null || this.IsFinishedState(this._job.JobStateInfo.State))
				{
					this._statusInfo = null;
					this._asyncJobStop = false;
					base.PSBeginTime = new DateTime?(DateTime.Now);
					if (this._powerShell != null)
					{
						this._powerShell.Commands.Clear();
					}
					else
					{
						InitialSessionState initialSessionState = InitialSessionState.CreateDefault2();
						initialSessionState.Commands.Clear();
						initialSessionState.Formats.Clear();
						initialSessionState.Commands.Add(new SessionStateCmdletEntry("Start-Job", typeof(StartJobCommand), null));
						this._host = this.GetDefaultHost();
						this._runspace = RunspaceFactory.CreateRunspace(this._host, initialSessionState);
						this._runspace.Open();
						this._powerShell = System.Management.Automation.PowerShell.Create();
						this._powerShell.Runspace = this._runspace;
						this.AddSetShouldExitToHost();
					}
					this._job = this.StartJobCommand(this._powerShell);
					this._job.StateChanged += new EventHandler<JobStateEventArgs>(this.HandleJobStateChanged);
					base.SetJobState(this._job.JobStateInfo.State);
					foreach (Job childJob in this._job.ChildJobs)
					{
						base.ChildJobs.Add(childJob);
					}
					ScheduledJobSourceAdapter.AddToRepository(this);
				}
				else
				{
					string str = StringUtil.Format(ScheduledJobErrorStrings.JobAlreadyRunning, this._jobDefinition.Name);
					throw new PSInvalidOperationException(str);
				}
			}
		}

		public override void StartJobAsync()
		{
			throw new PSNotSupportedException();
		}

		private Job StartJobCommand(System.Management.Automation.PowerShell powerShell)
		{
			Job baseObject = null;
			powerShell.AddCommand("Start-Job");
			powerShell.AddParameter("Name", this._jobDefinition.Name);
			CommandParameterCollection item = this._jobDefinition.InvocationInfo.Parameters[0];
			foreach (CommandParameter commandParameter in item)
			{
				string name = commandParameter.Name;
				string str = name;
				if (name == null)
				{
					continue;
				}
				if (str == "ScriptBlock")
				{
					powerShell.AddParameter("ScriptBlock", commandParameter.Value as ScriptBlock);
				}
				else
				{
					if (str == "FilePath")
					{
						powerShell.AddParameter("FilePath", commandParameter.Value as string);
					}
					else
					{
						if (str == "RunAs32")
						{
							powerShell.AddParameter("RunAs32", (bool)commandParameter.Value);
						}
						else
						{
							if (str == "Authentication")
							{
								powerShell.AddParameter("Authentication", (AuthenticationMechanism)commandParameter.Value);
							}
							else
							{
								if (str == "InitializationScript")
								{
									powerShell.AddParameter("InitializationScript", commandParameter.Value as ScriptBlock);
								}
								else
								{
									if (str == "ArgumentList")
									{
										powerShell.AddParameter("ArgumentList", commandParameter.Value as object[]);
									}
								}
							}
						}
					}
				}
			}
			Collection<PSObject> pSObjects = powerShell.Invoke();
			if (pSObjects != null && pSObjects.Count == 1)
			{
				baseObject = pSObjects[0].BaseObject as Job;
			}
			return baseObject;
		}

		public override void StopJob()
		{
			Job job;
			JobState state;
			lock (base.SyncRoot)
			{
				job = this._job;
				state = this.Status.State;
				this._asyncJobStop = false;
			}
			if (!this.IsFinishedState(state))
			{
				if (job != null)
				{
					job.StopJob();
					return;
				}
				else
				{
					base.SetJobState(JobState.Failed);
					return;
				}
			}
			else
			{
				return;
			}
		}

		public override void StopJob(bool force, string reason)
		{
			throw new PSNotSupportedException();
		}

		public override void StopJobAsync()
		{
			Job job;
			JobState state;
			lock (base.SyncRoot)
			{
				job = this._job;
				state = this.Status.State;
				this._asyncJobStop = true;
			}
			if (!this.IsFinishedState(state))
			{
				if (job != null)
				{
					job.StopJob();
					return;
				}
				else
				{
					base.SetJobState(JobState.Failed);
					this.HandleJobStateChanged(this, new JobStateEventArgs(new JobStateInfo(JobState.Failed)));
					return;
				}
			}
			else
			{
				return;
			}
		}

		public override void StopJobAsync(bool force, string reason)
		{
			throw new PSNotSupportedException();
		}

		public override void SuspendJob()
		{
			throw new PSNotSupportedException();
		}

		public override void SuspendJob(bool force, string reason)
		{
			throw new PSNotSupportedException();
		}

		public override void SuspendJobAsync()
		{
			throw new PSNotSupportedException();
		}

		public override void SuspendJobAsync(bool force, string reason)
		{
			throw new PSNotSupportedException();
		}

		public override void UnblockJob()
		{
			throw new PSNotSupportedException();
		}

		public override void UnblockJobAsync()
		{
			throw new PSNotSupportedException();
		}

		internal void Update(ScheduledJob fromJob)
		{
			if (this._job != null || fromJob == null)
			{
				return;
			}
			else
			{
				base.PSEndTime = fromJob.PSEndTime;
				JobState state = fromJob.JobStateInfo.State;
				if (this.Status.State != state)
				{
					base.SetJobState(state, null);
				}
				lock (base.SyncRoot)
				{
					this._statusInfo = new StatusInfo(fromJob.InstanceId, fromJob.Name, fromJob.Location, fromJob.Command, fromJob.StatusMessage, state, fromJob.HasMoreData, fromJob.PSBeginTime, fromJob.PSEndTime, fromJob._jobDefinition);
				}
				this.CopyOutput(fromJob.Output);
				this.CopyError(fromJob.Error);
				this.CopyWarning(fromJob.Warning);
				this.CopyVerbose(fromJob.Verbose);
				this.CopyProgress(fromJob.Progress);
				this.CopyDebug(fromJob.Debug);
				return;
			}
		}

		[Serializable]
		private class ResultsInfo : ISerializable
		{
			private Collection<PSObject> _output;

			private Collection<ErrorRecord> _error;

			private Collection<WarningRecord> _warning;

			private Collection<VerboseRecord> _verbose;

			private Collection<ProgressRecord> _progress;

			private Collection<DebugRecord> _debug;

			internal Collection<DebugRecord> Debug
			{
				get
				{
					return this._debug;
				}
			}

			internal Collection<ErrorRecord> Error
			{
				get
				{
					return this._error;
				}
			}

			internal Collection<PSObject> Output
			{
				get
				{
					return this._output;
				}
			}

			internal Collection<ProgressRecord> Progress
			{
				get
				{
					return this._progress;
				}
			}

			internal Collection<VerboseRecord> Verbose
			{
				get
				{
					return this._verbose;
				}
			}

			internal Collection<WarningRecord> Warning
			{
				get
				{
					return this._warning;
				}
			}

			internal ResultsInfo(Collection<PSObject> output, Collection<ErrorRecord> error, Collection<WarningRecord> warning, Collection<VerboseRecord> verbose, Collection<ProgressRecord> progress, Collection<DebugRecord> debug)
			{
				if (output != null)
				{
					if (error != null)
					{
						if (warning != null)
						{
							if (verbose != null)
							{
								if (progress != null)
								{
									if (debug != null)
									{
										this._output = output;
										this._error = error;
										this._warning = warning;
										this._verbose = verbose;
										this._progress = progress;
										this._debug = debug;
										return;
									}
									else
									{
										throw new PSArgumentNullException("debug");
									}
								}
								else
								{
									throw new PSArgumentNullException("progress");
								}
							}
							else
							{
								throw new PSArgumentNullException("verbose");
							}
						}
						else
						{
							throw new PSArgumentNullException("warning");
						}
					}
					else
					{
						throw new PSArgumentNullException("error");
					}
				}
				else
				{
					throw new PSArgumentNullException("output");
				}
			}

			[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
			private ResultsInfo(SerializationInfo info, StreamingContext context)
			{
				if (info != null)
				{
					this._output = (Collection<PSObject>)info.GetValue("Results_Output", typeof(Collection<PSObject>));
					this._error = (Collection<ErrorRecord>)info.GetValue("Results_Error", typeof(Collection<ErrorRecord>));
					this._warning = (Collection<WarningRecord>)info.GetValue("Results_Warning", typeof(Collection<WarningRecord>));
					this._verbose = (Collection<VerboseRecord>)info.GetValue("Results_Verbose", typeof(Collection<VerboseRecord>));
					this._progress = (Collection<ProgressRecord>)info.GetValue("Results_Progress", typeof(Collection<ProgressRecord>));
					this._debug = (Collection<DebugRecord>)info.GetValue("Results_Debug", typeof(Collection<DebugRecord>));
					return;
				}
				else
				{
					throw new PSArgumentNullException("info");
				}
			}

			[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
			public void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				if (info != null)
				{
					info.AddValue("Results_Output", this._output);
					info.AddValue("Results_Error", this._error);
					info.AddValue("Results_Warning", this._warning);
					info.AddValue("Results_Verbose", this._verbose);
					info.AddValue("Results_Progress", this._progress);
					info.AddValue("Results_Debug", this._debug);
					return;
				}
				else
				{
					throw new PSArgumentException("info");
				}
			}
		}
	}
}