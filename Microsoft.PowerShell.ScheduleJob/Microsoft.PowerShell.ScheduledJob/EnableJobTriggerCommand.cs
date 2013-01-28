using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.ScheduledJob
{
	[Cmdlet("Enable", "JobTrigger", SupportsShouldProcess=true, DefaultParameterSetName="JobEnabled", HelpUri="http://go.microsoft.com/fwlink/?LinkID=223917")]
	public sealed class EnableJobTriggerCommand : EnableDisableScheduledJobCmdletBase
	{
		internal override bool Enabled
		{
			get
			{
				return true;
			}
		}

		public EnableJobTriggerCommand()
		{
		}
	}
}