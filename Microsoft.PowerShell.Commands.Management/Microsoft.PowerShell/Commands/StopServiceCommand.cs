using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.ServiceProcess;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Stop", "Service", DefaultParameterSetName="InputObject", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113414")]
	[OutputType(new Type[] { typeof(ServiceController) })]
	public sealed class StopServiceCommand : ServiceOperationBaseCommand
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

		public StopServiceCommand()
		{
		}

		protected override void ProcessRecord()
		{
			List<ServiceController> serviceControllers = null;
			foreach (ServiceController serviceController in serviceControllers)
			{
				if (!base.ShouldProcessServiceOperation(serviceController))
				{
					continue;
				}
				serviceControllers = base.DoStopService(serviceController, this.Force);
				if (!base.PassThru || serviceControllers.Count <= 0)
				{
					continue;
				}
				List<ServiceController>.Enumerator enumerator = serviceControllers.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						ServiceController serviceController1 = serviceController;
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