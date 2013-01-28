using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Service.Gateway;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("Get", "AzureVNetGatewayKey")]
	public class GetAzureVNetGatewayKeyCommand : GatewayCmdletBase
	{
		[Parameter(Position=1, Mandatory=true, HelpMessage="The local network site name.")]
		[ValidateNotNullOrEmpty]
		public string LocalNetworkSiteName
		{
			get;
			set;
		}

		[Parameter(Position=0, Mandatory=true, HelpMessage="The virtual network name.")]
		[ValidateNotNullOrEmpty]
		public string VNetName
		{
			get;
			set;
		}

		public GetAzureVNetGatewayKeyCommand()
		{
		}

		public GetAzureVNetGatewayKeyCommand(IGatewayServiceManagement channel)
		{
			base.Channel = channel;
		}

		public SharedKeyContext GetVirtualNetworkGatewaySharedKeyProcess()
		{
			SharedKeyContext sharedKeyContext;
			Func<string, SharedKey> func = null;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					GetAzureVNetGatewayKeyCommand getAzureVNetGatewayKeyCommand = this;
					if (func == null)
					{
						func = (string s) => base.Channel.GetVirtualNetworkSharedKey(s, this.VNetName, this.LocalNetworkSiteName);
					}
					SharedKey sharedKey = ((CmdletBase<IGatewayServiceManagement>)getAzureVNetGatewayKeyCommand).RetryCall<SharedKey>(func);
					Operation operation = base.WaitForGatewayOperation(base.CommandRuntime.ToString());
					SharedKeyContext value = new SharedKeyContext();
					value.set_OperationId(operation.OperationTrackingId);
					value.set_OperationDescription(base.CommandRuntime.ToString());
					value.set_OperationStatus(operation.Status);
					value.Value = sharedKey.Value;
					sharedKeyContext = value;
					return sharedKeyContext;
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
						sharedKeyContext = null;
						return sharedKeyContext;
					}
				}
				sharedKeyContext = null;
			}
			return sharedKeyContext;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				SharedKeyContext virtualNetworkGatewaySharedKeyProcess = this.GetVirtualNetworkGatewaySharedKeyProcess();
				if (virtualNetworkGatewaySharedKeyProcess != null)
				{
					base.WriteObject(virtualNetworkGatewaySharedKeyProcess, true);
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