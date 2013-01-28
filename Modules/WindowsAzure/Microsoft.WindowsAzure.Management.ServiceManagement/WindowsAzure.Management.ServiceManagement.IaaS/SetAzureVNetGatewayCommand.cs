using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Model;
using Microsoft.WindowsAzure.Management.Service;
using Microsoft.WindowsAzure.Management.Service.Gateway;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("Set", "AzureVNetGateway", DefaultParameterSetName="Connect")]
	public class SetAzureVNetGatewayCommand : GatewayCmdletBase
	{
		[Parameter(Position=0, Mandatory=true, ParameterSetName="Connect", HelpMessage="Connect to Gateway")]
		public SwitchParameter Connect
		{
			get;
			set;
		}

		[Parameter(Position=0, Mandatory=true, ParameterSetName="Disconnect", HelpMessage="Disconnect from Gateway")]
		public SwitchParameter Disconnect
		{
			get;
			set;
		}

		[Parameter(Position=2, Mandatory=true, HelpMessage="Local Site network name.")]
		public string LocalNetworkSiteName
		{
			get;
			set;
		}

		[Parameter(Position=1, Mandatory=true, HelpMessage="Virtual network name.")]
		public string VNetName
		{
			get;
			set;
		}

		public SetAzureVNetGatewayCommand()
		{
		}

		public SetAzureVNetGatewayCommand(IGatewayServiceManagement channel)
		{
			base.Channel = channel;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				GatewayManagementOperationContext gatewayManagementOperationContext = this.SetVirtualNetworkGatewayCommandProcess();
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

		public GatewayManagementOperationContext SetVirtualNetworkGatewayCommandProcess()
		{
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					UpdateConnection updateConnection = new UpdateConnection();
					SwitchParameter connect = this.Connect;
					if (!connect.IsPresent)
					{
						updateConnection.Operation = UpdateConnectionOperation.Disconnect;
					}
					else
					{
						updateConnection.Operation = UpdateConnectionOperation.Connect;
					}
					base.RetryCall<GatewayOperationAsyncResponse>((string s) => base.Channel.UpdateVirtualNetworkGatewayConnection(s, this.VNetName, this.LocalNetworkSiteName, updateConnection));
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
	}
}