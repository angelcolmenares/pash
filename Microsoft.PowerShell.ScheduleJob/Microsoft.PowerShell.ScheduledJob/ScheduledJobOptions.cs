using System;
using System.Management.Automation;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.PowerShell.ScheduledJob
{
	[Serializable]
	public sealed class ScheduledJobOptions : ISerializable
	{
		private bool _startIfOnBatteries;

		private bool _stopIfGoingOnBatteries;

		private bool _wakeToRun;

		private bool _startIfNotIdle;

		private bool _stopIfGoingOffIdle;

		private bool _restartOnIdleResume;

		private TimeSpan _idleDuration;

		private TimeSpan _idleTimeout;

		private bool _showInTaskScheduler;

		private bool _runElevated;

		private bool _runWithoutNetwork;

		private bool _donotAllowDemandStart;

		private TaskMultipleInstancePolicy _multipleInstancePolicy;

		private ScheduledJobDefinition _jobDefAssociation;

		public bool DoNotAllowDemandStart
		{
			get
			{
				return this._donotAllowDemandStart;
			}
			set
			{
				this._donotAllowDemandStart = value;
			}
		}

		public TimeSpan IdleDuration
		{
			get
			{
				return this._idleDuration;
			}
			set
			{
				this._idleDuration = value;
			}
		}

		public TimeSpan IdleTimeout
		{
			get
			{
				return this._idleTimeout;
			}
			set
			{
				this._idleTimeout = value;
			}
		}

		public ScheduledJobDefinition JobDefinition
		{
			get
			{
				return this._jobDefAssociation;
			}
			internal set
			{
				this._jobDefAssociation = value;
			}
		}

		public TaskMultipleInstancePolicy MultipleInstancePolicy
		{
			get
			{
				return this._multipleInstancePolicy;
			}
			set
			{
				this._multipleInstancePolicy = value;
			}
		}

		public bool RestartOnIdleResume
		{
			get
			{
				return this._restartOnIdleResume;
			}
			set
			{
				this._restartOnIdleResume = value;
			}
		}

		public bool RunElevated
		{
			get
			{
				return this._runElevated;
			}
			set
			{
				this._runElevated = value;
			}
		}

		public bool RunWithoutNetwork
		{
			get
			{
				return this._runWithoutNetwork;
			}
			set
			{
				this._runWithoutNetwork = value;
			}
		}

		public bool ShowInTaskScheduler
		{
			get
			{
				return this._showInTaskScheduler;
			}
			set
			{
				this._showInTaskScheduler = value;
			}
		}

		public bool StartIfNotIdle
		{
			get
			{
				return this._startIfNotIdle;
			}
			set
			{
				this._startIfNotIdle = value;
			}
		}

		public bool StartIfOnBatteries
		{
			get
			{
				return this._startIfOnBatteries;
			}
			set
			{
				this._startIfOnBatteries = value;
			}
		}

		public bool StopIfGoingOffIdle
		{
			get
			{
				return this._stopIfGoingOffIdle;
			}
			set
			{
				this._stopIfGoingOffIdle = value;
			}
		}

		public bool StopIfGoingOnBatteries
		{
			get
			{
				return this._stopIfGoingOnBatteries;
			}
			set
			{
				this._stopIfGoingOnBatteries = value;
			}
		}

		public bool WakeToRun
		{
			get
			{
				return this._wakeToRun;
			}
			set
			{
				this._wakeToRun = value;
			}
		}

		public ScheduledJobOptions()
		{
			this._startIfOnBatteries = false;
			this._stopIfGoingOnBatteries = true;
			this._wakeToRun = false;
			this._startIfNotIdle = true;
			this._stopIfGoingOffIdle = false;
			this._restartOnIdleResume = false;
			this._idleDuration = new TimeSpan(0, 10, 0);
			this._idleTimeout = new TimeSpan(1, 0, 0);
			this._showInTaskScheduler = true;
			this._runElevated = false;
			this._runWithoutNetwork = true;
			this._donotAllowDemandStart = false;
			this._multipleInstancePolicy = TaskMultipleInstancePolicy.IgnoreNew;
		}

		internal ScheduledJobOptions(bool startIfOnBatteries, bool stopIfGoingOnBatters, bool wakeToRun, bool startIfNotIdle, bool stopIfGoingOffIdle, bool restartOnIdleResume, TimeSpan idleDuration, TimeSpan idleTimeout, bool showInTaskScheduler, bool runElevated, bool runWithoutNetwork, bool donotAllowDemandStart, TaskMultipleInstancePolicy multipleInstancePolicy)
		{
			this._startIfOnBatteries = startIfOnBatteries;
			this._stopIfGoingOnBatteries = stopIfGoingOnBatters;
			this._wakeToRun = wakeToRun;
			this._startIfNotIdle = startIfNotIdle;
			this._stopIfGoingOffIdle = stopIfGoingOffIdle;
			this._restartOnIdleResume = restartOnIdleResume;
			this._idleDuration = idleDuration;
			this._idleTimeout = idleTimeout;
			this._showInTaskScheduler = showInTaskScheduler;
			this._runElevated = runElevated;
			this._runWithoutNetwork = runWithoutNetwork;
			this._donotAllowDemandStart = donotAllowDemandStart;
			this._multipleInstancePolicy = multipleInstancePolicy;
		}

		internal ScheduledJobOptions(ScheduledJobOptions copyOptions)
		{
			if (copyOptions != null)
			{
				this._startIfOnBatteries = copyOptions.StartIfOnBatteries;
				this._stopIfGoingOnBatteries = copyOptions.StopIfGoingOnBatteries;
				this._wakeToRun = copyOptions.WakeToRun;
				this._startIfNotIdle = copyOptions.StartIfNotIdle;
				this._stopIfGoingOffIdle = copyOptions.StopIfGoingOffIdle;
				this._restartOnIdleResume = copyOptions.RestartOnIdleResume;
				this._idleDuration = copyOptions.IdleDuration;
				this._idleTimeout = copyOptions.IdleTimeout;
				this._showInTaskScheduler = copyOptions.ShowInTaskScheduler;
				this._runElevated = copyOptions.RunElevated;
				this._runWithoutNetwork = copyOptions.RunWithoutNetwork;
				this._donotAllowDemandStart = copyOptions.DoNotAllowDemandStart;
				this._multipleInstancePolicy = copyOptions.MultipleInstancePolicy;
				this._jobDefAssociation = copyOptions.JobDefinition;
				return;
			}
			else
			{
				throw new PSArgumentNullException("copyOptions");
			}
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		private ScheduledJobOptions(SerializationInfo info, StreamingContext context)
		{
			if (info != null)
			{
				this._startIfOnBatteries = info.GetBoolean("StartIfOnBatteries_Value");
				this._stopIfGoingOnBatteries = info.GetBoolean("StopIfGoingOnBatteries_Value");
				this._wakeToRun = info.GetBoolean("WakeToRun_Value");
				this._startIfNotIdle = info.GetBoolean("StartIfNotIdle_Value");
				this._stopIfGoingOffIdle = info.GetBoolean("StopIfGoingOffIdle_Value");
				this._restartOnIdleResume = info.GetBoolean("RestartOnIdleResume_Value");
				this._idleDuration = (TimeSpan)info.GetValue("IdleDuration_Value", typeof(TimeSpan));
				this._idleTimeout = (TimeSpan)info.GetValue("IdleTimeout_Value", typeof(TimeSpan));
				this._showInTaskScheduler = info.GetBoolean("ShowInTaskScheduler_Value");
				this._runElevated = info.GetBoolean("RunElevated_Value");
				this._runWithoutNetwork = info.GetBoolean("RunWithoutNetwork_Value");
				this._donotAllowDemandStart = info.GetBoolean("DoNotAllowDemandStart_Value");
				this._multipleInstancePolicy = (TaskMultipleInstancePolicy)info.GetValue("TaskMultipleInstancePolicy_Value", typeof(TaskMultipleInstancePolicy));
				this._jobDefAssociation = null;
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
				info.AddValue("StartIfOnBatteries_Value", this._startIfOnBatteries);
				info.AddValue("StopIfGoingOnBatteries_Value", this._stopIfGoingOnBatteries);
				info.AddValue("WakeToRun_Value", this._wakeToRun);
				info.AddValue("StartIfNotIdle_Value", this._startIfNotIdle);
				info.AddValue("StopIfGoingOffIdle_Value", this._stopIfGoingOffIdle);
				info.AddValue("RestartOnIdleResume_Value", this._restartOnIdleResume);
				info.AddValue("IdleDuration_Value", this._idleDuration);
				info.AddValue("IdleTimeout_Value", this._idleTimeout);
				info.AddValue("ShowInTaskScheduler_Value", this._showInTaskScheduler);
				info.AddValue("RunElevated_Value", this._runElevated);
				info.AddValue("RunWithoutNetwork_Value", this._runWithoutNetwork);
				info.AddValue("DoNotAllowDemandStart_Value", this._donotAllowDemandStart);
				info.AddValue("TaskMultipleInstancePolicy_Value", this._multipleInstancePolicy);
				return;
			}
			else
			{
				throw new PSArgumentNullException("info");
			}
		}

		public void UpdateJobDefinition()
		{
			if (this._jobDefAssociation != null)
			{
				this._jobDefAssociation.UpdateOptions(this, true);
				return;
			}
			else
			{
				string str = StringUtil.Format(ScheduledJobErrorStrings.NoAssociatedJobDefinitionForOption, new object[0]);
				throw new RuntimeException(str);
			}
		}
	}
}