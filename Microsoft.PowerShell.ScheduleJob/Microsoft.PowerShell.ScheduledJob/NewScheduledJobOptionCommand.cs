using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.ScheduledJob
{
	[Cmdlet("New", "ScheduledJobOption", DefaultParameterSetName="Options", HelpUri="http://go.microsoft.com/fwlink/?LinkID=223919")]
	[OutputType(new Type[] { typeof(ScheduledJobOptions) })]
	public sealed class NewScheduledJobOptionCommand : ScheduledJobOptionCmdletBase
	{
		public NewScheduledJobOptionCommand()
		{
		}

		protected override void ProcessRecord()
		{
			base.WriteObject(new ScheduledJobOptions(base.StartIfOnBattery, !base.ContinueIfGoingOnBattery, base.WakeToRun, !base.StartIfIdle, base.StopIfGoingOffIdle, base.RestartOnIdleResume, base.IdleDuration, base.IdleTimeout, !base.HideInTaskScheduler, base.RunElevated, !base.RequireNetwork, base.DoNotAllowDemandStart, base.MultipleInstancePolicy));
		}
	}
}