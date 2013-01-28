using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Model;
using Microsoft.WindowsAzure.Management.Service;
using Microsoft.WindowsAzure.Management.Service.Gateway;
using System;
using System.Management.Automation;
using System.ServiceModel;
using System.Xml.Linq;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("Remove", "AzureVNetGateway")]
	public class RemoveAzureVNetGatewayCommand : GatewayCmdletBase
	{
		private static XNamespace netconfigNamespace;

		private static XNamespace instanceNamespace;

		[Parameter(Position=0, Mandatory=true, HelpMessage="Virtual network name.")]
		public string VNetName
		{
			get;
			set;
		}

		static RemoveAzureVNetGatewayCommand()
		{
			RemoveAzureVNetGatewayCommand.netconfigNamespace = "http://schemas.microsoft.com/ServiceHosting/2011/07/NetworkConfiguration";
			RemoveAzureVNetGatewayCommand.instanceNamespace = "http://www.w3.org/2001/XMLSchema-instance";
		}

		public RemoveAzureVNetGatewayCommand()
		{
		}

		public RemoveAzureVNetGatewayCommand(IGatewayServiceManagement channel)
		{
			base.Channel = channel;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				this.RemoveVirtualNetworkGatewayCommandProcess();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		public void RemoveVirtualNetworkGatewayCommandProcess()
		{
			Func<string, GatewayOperationAsyncResponse> func = null;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					RemoveAzureVNetGatewayCommand removeAzureVNetGatewayCommand = this;
					if (func == null)
					{
						func = (string s) => base.Channel.DeleteVirtualNetworkGateway(s, this.VNetName);
					}
					((CmdletBase<IGatewayServiceManagement>)removeAzureVNetGatewayCommand).RetryCall<GatewayOperationAsyncResponse>(func);
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
		}
	}
}