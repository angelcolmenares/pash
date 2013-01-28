using System;
using System.Management.Automation;
using System.ServiceProcess;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Start", "Service", DefaultParameterSetName="InputObject", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113406")]
	[OutputType(new Type[] { typeof(ServiceController) })]
	public sealed class StartServiceCommand : ServiceOperationBaseCommand
	{
		public StartServiceCommand()
		{
		}

		protected override void ProcessRecord()
		{
			foreach (ServiceController serviceController in base.MatchingServices())
			{
				if (!base.ShouldProcessServiceOperation(serviceController) || !base.DoStartService(serviceController) || !base.PassThru)
				{
					continue;
				}
				base.WriteObject(serviceController);
			}
		}
	}
}