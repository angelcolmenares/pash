using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.ScheduledJob
{
	[Cmdlet("Enable", "ScheduledJob", SupportsShouldProcess=true, DefaultParameterSetName="Definition", HelpUri="http://go.microsoft.com/fwlink/?LinkID=223926")]
	[OutputType(new Type[] { typeof(ScheduledJobDefinition) })]
	public sealed class EnableScheduledJobCommand : DisableScheduledJobDefinitionBase
	{
		protected override bool Enabled
		{
			get
			{
				return true;
			}
		}

		public EnableScheduledJobCommand()
		{
		}
	}
}