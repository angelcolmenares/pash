using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.ScheduledJob
{
	[Cmdlet("Disable", "JobTrigger", SupportsShouldProcess=true, DefaultParameterSetName="JobEnabled", HelpUri="http://go.microsoft.com/fwlink/?LinkID=223918")]
	public sealed class DisableJobTriggerCommand : EnableDisableScheduledJobCmdletBase
	{
		internal override bool Enabled
		{
			get
			{
				return false;
			}
		}

		public DisableJobTriggerCommand()
		{
		}
	}
}