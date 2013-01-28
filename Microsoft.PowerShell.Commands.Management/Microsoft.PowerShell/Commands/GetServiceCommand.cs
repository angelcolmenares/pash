using System;
using System.Management.Automation;
using System.ServiceProcess;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Get", "Service", DefaultParameterSetName="Default", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113332", RemotingCapability=RemotingCapability.SupportedByCommand)]
	[OutputType(new Type[] { typeof(ServiceController) })]
	public sealed class GetServiceCommand : MultipleServiceCommandBase
	{
		private SwitchParameter dependentservices;

		private SwitchParameter requiredservices;

		[Alias(new string[] { "Cn" })]
		[Parameter(Mandatory=false, ValueFromPipelineByPropertyName=true)]
		[ValidateNotNullOrEmpty]
		public string[] ComputerName
		{
			get
			{
				return base.SuppliedComputerName;
			}
			set
			{
				base.SuppliedComputerName = value;
			}
		}

		[Alias(new string[] { "DS" })]
		[Parameter]
		public SwitchParameter DependentServices
		{
			get
			{
				return this.dependentservices;
			}
			set
			{
				this.dependentservices = value;
			}
		}

		[Alias(new string[] { "ServiceName" })]
		[Parameter(Position=0, ParameterSetName="Default", ValueFromPipelineByPropertyName=true, ValueFromPipeline=true)]
		public string[] Name
		{
			get
			{
				return this.serviceNames;
			}
			set
			{
				this.serviceNames = value;
				this.selectionMode = MultipleServiceCommandBase.SelectionMode.ServiceName;
			}
		}

		[Alias(new string[] { "SDO", "ServicesDependedOn" })]
		[Parameter]
		public SwitchParameter RequiredServices
		{
			get
			{
				return this.requiredservices;
			}
			set
			{
				this.requiredservices = value;
			}
		}

		public GetServiceCommand()
		{
		}

		protected override void ProcessRecord()
		{
			foreach (ServiceController serviceController in base.MatchingServices())
			{
				if (this.dependentservices.IsPresent || this.requiredservices.IsPresent)
				{
					if (this.dependentservices.IsPresent)
					{
						ServiceController[] dependentServices = serviceController.DependentServices;
						for (int i = 0; i < (int)dependentServices.Length; i++)
						{
							ServiceController serviceController1 = dependentServices[i];
							base.WriteObject(serviceController1);
						}
					}
					if (!this.requiredservices.IsPresent)
					{
						continue;
					}
					ServiceController[] servicesDependedOn = serviceController.ServicesDependedOn;
					int num = 0;
					while (num < (int)servicesDependedOn.Length)
					{
						ServiceController serviceController2 = servicesDependedOn[num];
						base.WriteObject(serviceController2);
						num++;
					}
				}
				else
				{
					base.WriteObject(serviceController);
				}
			}
		}
	}
}