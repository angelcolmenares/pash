using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.ScheduledJob
{
	[Cmdlet("Set", "ScheduledJobOption", DefaultParameterSetName="Options", HelpUri="http://go.microsoft.com/fwlink/?LinkID=223921")]
	[OutputType(new Type[] { typeof(ScheduledJobOptions) })]
	public class SetScheduledJobOptionCommand : ScheduledJobOptionCmdletBase
	{
		private ScheduledJobOptions _jobOptions;

		private SwitchParameter _passThru;

		[Parameter(Position=0, Mandatory=true, ValueFromPipeline=true, ParameterSetName="Options")]
		[ValidateNotNull]
		public ScheduledJobOptions InputObject
		{
			get
			{
				return this._jobOptions;
			}
			set
			{
				this._jobOptions = value;
			}
		}

		[Parameter(ParameterSetName="Options")]
		public SwitchParameter PassThru
		{
			get
			{
				return this._passThru;
			}
			set
			{
				this._passThru = value;
			}
		}

		public SetScheduledJobOptionCommand()
		{
		}

		protected override void ProcessRecord()
		{
			if (base.MyInvocation.BoundParameters.ContainsKey("StartIfOnBattery"))
			{
				this._jobOptions.StartIfOnBatteries = base.StartIfOnBattery;
			}
			if (base.MyInvocation.BoundParameters.ContainsKey("ContinueIfGoingOnBattery"))
			{
				this._jobOptions.StopIfGoingOnBatteries = !base.ContinueIfGoingOnBattery;
			}
			if (base.MyInvocation.BoundParameters.ContainsKey("WakeToRun"))
			{
				this._jobOptions.WakeToRun = base.WakeToRun;
			}
			if (base.MyInvocation.BoundParameters.ContainsKey("StartIfIdle"))
			{
				this._jobOptions.StartIfNotIdle = !base.StartIfIdle;
			}
			if (base.MyInvocation.BoundParameters.ContainsKey("StopIfGoingOffIdle"))
			{
				this._jobOptions.StopIfGoingOffIdle = base.StopIfGoingOffIdle;
			}
			if (base.MyInvocation.BoundParameters.ContainsKey("RestartOnIdleResume"))
			{
				this._jobOptions.RestartOnIdleResume = base.RestartOnIdleResume;
			}
			if (base.MyInvocation.BoundParameters.ContainsKey("HideInTaskScheduler"))
			{
				this._jobOptions.ShowInTaskScheduler = !base.HideInTaskScheduler;
			}
			if (base.MyInvocation.BoundParameters.ContainsKey("RunElevated"))
			{
				this._jobOptions.RunElevated = base.RunElevated;
			}
			if (base.MyInvocation.BoundParameters.ContainsKey("RequireNetwork"))
			{
				this._jobOptions.RunWithoutNetwork = !base.RequireNetwork;
			}
			if (base.MyInvocation.BoundParameters.ContainsKey("DoNotAllowDemandStart"))
			{
				this._jobOptions.DoNotAllowDemandStart = base.DoNotAllowDemandStart;
			}
			if (base.MyInvocation.BoundParameters.ContainsKey("IdleDuration"))
			{
				this._jobOptions.IdleDuration = base.IdleDuration;
			}
			if (base.MyInvocation.BoundParameters.ContainsKey("IdleTimeout"))
			{
				this._jobOptions.IdleTimeout = base.IdleTimeout;
			}
			if (base.MyInvocation.BoundParameters.ContainsKey("MultipleInstancePolicy"))
			{
				this._jobOptions.MultipleInstancePolicy = base.MultipleInstancePolicy;
			}
			if (this._jobOptions.JobDefinition != null)
			{
				this._jobOptions.UpdateJobDefinition();
			}
			if (this._passThru)
			{
				base.WriteObject(this._jobOptions);
			}
		}
	}
}