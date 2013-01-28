using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.ScheduledJob
{
	public abstract class ScheduledJobOptionCmdletBase : ScheduleJobCmdletBase
	{
		protected const string OptionsParameterSet = "Options";

		private SwitchParameter _runElevated;

		private SwitchParameter _hideInTaskScheduler;

		private SwitchParameter _restartOnIdleResume;

		private TaskMultipleInstancePolicy _multipleInstancePolicy;

		private SwitchParameter _doNotAllowDemandStart;

		private SwitchParameter _requireNetwork;

		private SwitchParameter _stopIfGoingOffIdle;

		private SwitchParameter _wakeToRun;

		private SwitchParameter _continueIfGoingOnBattery;

		private SwitchParameter _startIfOnBattery;

		private TimeSpan _idleTimeout;

		private TimeSpan _idleDuration;

		private SwitchParameter _startIfIdle;

		[Parameter(ParameterSetName="Options")]
		public SwitchParameter ContinueIfGoingOnBattery
		{
			get
			{
				return this._continueIfGoingOnBattery;
			}
			set
			{
				this._continueIfGoingOnBattery = value;
			}
		}

		[Parameter(ParameterSetName="Options")]
		public SwitchParameter DoNotAllowDemandStart
		{
			get
			{
				return this._doNotAllowDemandStart;
			}
			set
			{
				this._doNotAllowDemandStart = value;
			}
		}

		[Parameter(ParameterSetName="Options")]
		public SwitchParameter HideInTaskScheduler
		{
			get
			{
				return this._hideInTaskScheduler;
			}
			set
			{
				this._hideInTaskScheduler = value;
			}
		}

		[Parameter(ParameterSetName="Options")]
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

		[Parameter(ParameterSetName="Options")]
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

		[Parameter(ParameterSetName="Options")]
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

		[Parameter(ParameterSetName="Options")]
		public SwitchParameter RequireNetwork
		{
			get
			{
				return this._requireNetwork;
			}
			set
			{
				this._requireNetwork = value;
			}
		}

		[Parameter(ParameterSetName="Options")]
		public SwitchParameter RestartOnIdleResume
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

		[Parameter(ParameterSetName="Options")]
		public SwitchParameter RunElevated
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

		[Parameter(ParameterSetName="Options")]
		public SwitchParameter StartIfIdle
		{
			get
			{
				return this._startIfIdle;
			}
			set
			{
				this._startIfIdle = value;
			}
		}

		[Parameter(ParameterSetName="Options")]
		public SwitchParameter StartIfOnBattery
		{
			get
			{
				return this._startIfOnBattery;
			}
			set
			{
				this._startIfOnBattery = value;
			}
		}

		[Parameter(ParameterSetName="Options")]
		public SwitchParameter StopIfGoingOffIdle
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

		[Parameter(ParameterSetName="Options")]
		public SwitchParameter WakeToRun
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

		protected ScheduledJobOptionCmdletBase()
		{
			this._runElevated = false;
			this._hideInTaskScheduler = false;
			this._restartOnIdleResume = false;
			this._multipleInstancePolicy = TaskMultipleInstancePolicy.IgnoreNew;
			this._doNotAllowDemandStart = false;
			this._requireNetwork = false;
			this._stopIfGoingOffIdle = false;
			this._wakeToRun = false;
			this._continueIfGoingOnBattery = false;
			this._startIfOnBattery = false;
			this._idleTimeout = new TimeSpan(1, 0, 0);
			this._idleDuration = new TimeSpan(0, 10, 0);
			this._startIfIdle = false;
		}

		protected override void BeginProcessing()
		{
			if (!base.MyInvocation.BoundParameters.ContainsKey("IdleTimeout") || !(this._idleTimeout < TimeSpan.Zero))
			{
				if (!base.MyInvocation.BoundParameters.ContainsKey("IdleDuration") || !(this._idleDuration < TimeSpan.Zero))
				{
					return;
				}
				else
				{
					throw new PSArgumentException(ScheduledJobErrorStrings.InvalidIdleDuration);
				}
			}
			else
			{
				throw new PSArgumentException(ScheduledJobErrorStrings.InvalidIdleTimeout);
			}
		}
	}
}