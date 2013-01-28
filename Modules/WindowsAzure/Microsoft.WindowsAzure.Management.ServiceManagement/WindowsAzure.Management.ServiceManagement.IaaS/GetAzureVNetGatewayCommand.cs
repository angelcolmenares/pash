using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Service.Gateway;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Management.Automation;
using System.ServiceModel;
using System.Xml.Linq;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("Get", "AzureVNetGateway")]
	public class GetAzureVNetGatewayCommand : GatewayCmdletBase
	{
		private static XNamespace netconfigNamespace;

		private static XNamespace instanceNamespace;

		[Parameter(Position=0, Mandatory=true, HelpMessage="Virtual network name.")]
		public string VNetName
		{
			get;
			set;
		}

		static GetAzureVNetGatewayCommand()
		{
			GetAzureVNetGatewayCommand.netconfigNamespace = "http://schemas.microsoft.com/ServiceHosting/2011/07/NetworkConfiguration";
			GetAzureVNetGatewayCommand.instanceNamespace = "http://www.w3.org/2001/XMLSchema-instance";
		}

		public GetAzureVNetGatewayCommand()
		{
		}

		public GetAzureVNetGatewayCommand(IGatewayServiceManagement channel)
		{
			base.Channel = channel;
		}

		public VirtualNetworkGatewayContext GetVirtualNetworkGatewayCommandProcess()
		{
			VirtualNetworkGatewayContext virtualNetworkGatewayContext;
			string data;
			string message;
			int id;
			DateTime? nullable;
			Func<string, VnetGateway> func = null;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					GetAzureVNetGatewayCommand getAzureVNetGatewayCommand = this;
					if (func == null)
					{
						func = (string s) => base.Channel.GetVirtualNetworkGateway(s, this.VNetName);
					}
					VnetGateway vnetGateway = ((CmdletBase<IGatewayServiceManagement>)getAzureVNetGatewayCommand).RetryCall<VnetGateway>(func);
					Operation operation = base.WaitForGatewayOperation(base.CommandRuntime.ToString());
					VirtualNetworkGatewayContext state = new VirtualNetworkGatewayContext();
					state.set_OperationId(operation.OperationTrackingId);
					state.set_OperationStatus(operation.Status.ToString());
					state.set_OperationDescription(base.CommandRuntime.ToString());
					VirtualNetworkGatewayContext virtualNetworkGatewayContext1 = state;
					if (vnetGateway.LastEvent != null)
					{
						data = vnetGateway.LastEvent.Data;
					}
					else
					{
						data = null;
					}
					virtualNetworkGatewayContext1.LastEventData = data;
					VirtualNetworkGatewayContext virtualNetworkGatewayContext2 = state;
					if (vnetGateway.LastEvent != null)
					{
						message = vnetGateway.LastEvent.Message;
					}
					else
					{
						message = null;
					}
					virtualNetworkGatewayContext2.LastEventMessage = message;
					VirtualNetworkGatewayContext virtualNetworkGatewayContext3 = state;
					if (vnetGateway.LastEvent != null)
					{
						id = vnetGateway.LastEvent.Id;
					}
					else
					{
						id = -1;
					}
					virtualNetworkGatewayContext3.LastEventID = id;
					VirtualNetworkGatewayContext virtualNetworkGatewayContext4 = state;
					if (vnetGateway.LastEvent != null)
					{
						nullable = new DateTime?(vnetGateway.LastEvent.Timestamp);
					}
					else
					{
						DateTime? nullable1 = null;
						nullable = nullable1;
					}
					virtualNetworkGatewayContext4.LastEventTimeStamp = nullable;
					state.State = vnetGateway.State;
					state.VIPAddress = vnetGateway.VIPAddress;
					virtualNetworkGatewayContext = state;
					return virtualNetworkGatewayContext;
				}
				catch (CommunicationException communicationException1)
				{
					CommunicationException communicationException = communicationException1;
					if (communicationException as EndpointNotFoundException == null || base.IsVerbose())
					{
						this.WriteErrorDetails(communicationException);
					}
					else
					{
						virtualNetworkGatewayContext = null;
						return virtualNetworkGatewayContext;
					}
				}
				return null;
			}
			return virtualNetworkGatewayContext;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				VirtualNetworkGatewayContext virtualNetworkGatewayCommandProcess = this.GetVirtualNetworkGatewayCommandProcess();
				if (virtualNetworkGatewayCommandProcess != null)
				{
					base.WriteObject(virtualNetworkGatewayCommandProcess, true);
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