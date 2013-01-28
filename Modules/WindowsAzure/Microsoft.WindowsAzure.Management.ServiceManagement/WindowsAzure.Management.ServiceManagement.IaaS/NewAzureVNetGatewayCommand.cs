using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Model;
using Microsoft.WindowsAzure.Management.Service;
using Microsoft.WindowsAzure.Management.Service.Gateway;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Management.Automation;
using System.ServiceModel;
using System.Xml.Linq;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("New", "AzureVNetGateway")]
	public class NewAzureVNetGatewayCommand : GatewayCmdletBase
	{
		private static XNamespace netconfigNamespace;

		private static XNamespace instanceNamespace;

		[Parameter(Position=0, Mandatory=true, HelpMessage="Virtual network name.")]
		public string VNetName
		{
			get;
			set;
		}

		static NewAzureVNetGatewayCommand()
		{
			NewAzureVNetGatewayCommand.netconfigNamespace = "http://schemas.microsoft.com/ServiceHosting/2011/07/NetworkConfiguration";
			NewAzureVNetGatewayCommand.instanceNamespace = "http://www.w3.org/2001/XMLSchema-instance";
		}

		public NewAzureVNetGatewayCommand()
		{
		}

		public NewAzureVNetGatewayCommand(IGatewayServiceManagement channel)
		{
			base.Channel = channel;
		}

		public GatewayManagementOperationContext NewVirtualNetworkGatewayCommandProcess()
		{
			Func<string, GatewayOperationAsyncResponse> func = null;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					NewAzureVNetGatewayCommand newAzureVNetGatewayCommand = this;
					if (func == null)
					{
						func = (string s) => base.Channel.NewVirtualNetworkGateway(s, this.VNetName);
					}
					((CmdletBase<IGatewayServiceManagement>)newAzureVNetGatewayCommand).RetryCall<GatewayOperationAsyncResponse>(func);
					Operation operation = base.WaitForGatewayOperation(base.CommandRuntime.ToString());
					ManagementOperationContext managementOperationContext = new ManagementOperationContext();
					managementOperationContext.set_OperationDescription(base.CommandRuntime.ToString());
					managementOperationContext.set_OperationId(operation.OperationTrackingId);
					managementOperationContext.set_OperationStatus(operation.Status);
					ManagementOperationContext managementOperationContext1 = managementOperationContext;
					base.WriteObject(managementOperationContext1, true);
				}
				catch (CommunicationException communicationException1)
				{
					CommunicationException communicationException = communicationException1;
					this.WriteErrorDetails(communicationException);
				}
			}
			return null;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				GatewayManagementOperationContext gatewayManagementOperationContext = this.NewVirtualNetworkGatewayCommandProcess();
				if (gatewayManagementOperationContext != null)
				{
					base.WriteObject(gatewayManagementOperationContext, true);
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}
	}
}