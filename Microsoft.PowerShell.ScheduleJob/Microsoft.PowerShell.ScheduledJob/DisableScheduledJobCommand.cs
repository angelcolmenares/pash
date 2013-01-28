using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.ScheduledJob
{
	[Cmdlet("Disable", "ScheduledJob", SupportsShouldProcess=true, DefaultParameterSetName="Definition", HelpUri="http://go.microsoft.com/fwlink/?LinkID=223927")]
	[OutputType(new Type[] { typeof(ScheduledJobDefinition) })]
	public sealed class DisableScheduledJobCommand : DisableScheduledJobDefinitionBase
	{
		protected override bool Enabled
		{
			get
			{
				return false;
			}
		}

		public DisableScheduledJobCommand()
		{
		}
	}
}