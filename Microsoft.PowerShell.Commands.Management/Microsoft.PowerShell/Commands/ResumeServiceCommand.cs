using System;
using System.Management.Automation;
using System.ServiceProcess;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Resume", "Service", DefaultParameterSetName="InputObject", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113386")]
	[OutputType(new Type[] { typeof(ServiceController) })]
	public sealed class ResumeServiceCommand : ServiceOperationBaseCommand
	{
		public ResumeServiceCommand()
		{
		}

		protected override void ProcessRecord()
		{
			foreach (ServiceController serviceController in base.MatchingServices())
			{
				if (!base.ShouldProcessServiceOperation(serviceController) || !base.DoResumeService(serviceController) || !base.PassThru)
				{
					continue;
				}
				base.WriteObject(serviceController);
			}
		}
	}
}