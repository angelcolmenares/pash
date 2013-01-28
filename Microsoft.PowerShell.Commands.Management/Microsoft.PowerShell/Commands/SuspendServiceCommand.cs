using System;
using System.Management.Automation;
using System.ServiceProcess;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Suspend", "Service", DefaultParameterSetName="InputObject", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113416")]
	[OutputType(new Type[] { typeof(ServiceController) })]
	public sealed class SuspendServiceCommand : ServiceOperationBaseCommand
	{
		public SuspendServiceCommand()
		{
		}

		protected override void ProcessRecord()
		{
			foreach (ServiceController serviceController in base.MatchingServices())
			{
				if (!base.ShouldProcessServiceOperation(serviceController) || !base.DoPauseService(serviceController) || !base.PassThru)
				{
					continue;
				}
				base.WriteObject(serviceController);
			}
		}
	}
}