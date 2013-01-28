using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.ServiceProcess;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Restart", "Service", DefaultParameterSetName="InputObject", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113385")]
	[OutputType(new Type[] { typeof(ServiceController) })]
	public sealed class RestartServiceCommand : ServiceOperationBaseCommand
	{
		private SwitchParameter force;

		[Parameter]
		public SwitchParameter Force
		{
			get
			{
				return this.force;
			}
			set
			{
				this.force = value;
			}
		}

		public RestartServiceCommand()
		{
		}

		protected override void ProcessRecord()
		{
			List<ServiceController> serviceControllers = base.MatchingServices ();
			foreach (ServiceController serviceController in serviceControllers)
			{
				if (!base.ShouldProcessServiceOperation(serviceController))
				{
					continue;
				}
				serviceControllers = base.DoStopService(serviceController, this.Force);
				if (serviceControllers.Count <= 0)
				{
					continue;
				}
				List<ServiceController>.Enumerator enumerator = serviceControllers.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						ServiceController serviceController1 = serviceController;
						if (!base.DoStartService(serviceController1) || !base.PassThru)
						{
							continue;
						}
						base.WriteObject(serviceController1);
					}
				}
				finally
				{
					enumerator.Dispose();
				}
			}
		}
	}
}