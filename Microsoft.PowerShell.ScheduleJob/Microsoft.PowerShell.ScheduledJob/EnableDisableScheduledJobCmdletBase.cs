using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.ScheduledJob
{
	public abstract class EnableDisableScheduledJobCmdletBase : ScheduleJobCmdletBase
	{
		protected const string EnabledParameterSet = "JobEnabled";

		private ScheduledJobTrigger[] _triggers;

		internal abstract bool Enabled
		{
			get;
		}

		[Parameter(Position=0, Mandatory=true, ValueFromPipeline=true, ParameterSetName="JobEnabled")]
		[ValidateNotNullOrEmpty]
		public ScheduledJobTrigger[] InputObject
		{
			get
			{
				return this._triggers;
			}
			set
			{
				this._triggers = value;
			}
		}

		protected EnableDisableScheduledJobCmdletBase()
		{
		}

		protected override void ProcessRecord()
		{
			ScheduledJobTrigger[] scheduledJobTriggerArray = this._triggers;
			for (int i = 0; i < (int)scheduledJobTriggerArray.Length; i++)
			{
				ScheduledJobTrigger enabled = scheduledJobTriggerArray[i];
				enabled.Enabled = this.Enabled;
				if (enabled.JobDefinition != null)
				{
					enabled.UpdateJobDefinition();
				}
			}
		}
	}
}