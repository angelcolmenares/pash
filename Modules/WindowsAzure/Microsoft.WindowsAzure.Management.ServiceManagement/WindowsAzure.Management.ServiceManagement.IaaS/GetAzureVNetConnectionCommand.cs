using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Service;
using Microsoft.WindowsAzure.Management.Service.Gateway;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("Get", "AzureVNetConnection")]
	public class GetAzureVNetConnectionCommand : GatewayCmdletBase
	{
		[Parameter(Position=0, Mandatory=true, HelpMessage="Virtual network name.")]
		public string VNetName
		{
			get;
			set;
		}

		public GetAzureVNetConnectionCommand()
		{
		}

		public GetAzureVNetConnectionCommand(IGatewayServiceManagement channel)
		{
			base.Channel = channel;
		}

		private static GatewayEvent CreateGatewayEvent(GatewayEvent connectionEvent)
		{
			if (connectionEvent != null)
			{
				GatewayEvent gatewayEvent = new GatewayEvent();
				gatewayEvent.Data = connectionEvent.Data;
				gatewayEvent.Id = connectionEvent.Id;
				gatewayEvent.Message = connectionEvent.Message;
				gatewayEvent.Timestamp = connectionEvent.Timestamp;
				return gatewayEvent;
			}
			else
			{
				return null;
			}
		}

		public IEnumerable<GatewayConnectionContext> GetVirtualNetworkConnectionCommandProcess()
		{
			IEnumerable<GatewayConnectionContext> gatewayConnectionContexts;
			Func<string, ConnectionCollection> func = null;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					GetAzureVNetConnectionCommand getAzureVNetConnectionCommand = this;
					if (func == null)
					{
						func = (string s) => base.Channel.ListVirtualNetworkConnections(s, this.VNetName);
					}
					ConnectionCollection connectionCollection = ((CmdletBase<IGatewayServiceManagement>)getAzureVNetConnectionCommand).RetryCall<ConnectionCollection>(func);
					Operation operation = base.WaitForGatewayOperation(base.CommandRuntime.ToString());
					gatewayConnectionContexts = connectionCollection.Select<Connection, GatewayConnectionContext>((Connection c) => {
						string str;
						string str1;
						string str2;
						GatewayConnectionContext gatewayConnectionContext = new GatewayConnectionContext();
						gatewayConnectionContext.OperationId = operation.OperationTrackingId;
						gatewayConnectionContext.OperationDescription = this.CommandRuntime.ToString();
						gatewayConnectionContext.OperationStatus = operation.Status;
						gatewayConnectionContext.ConnectivityState = c.ConnectivityState;
						gatewayConnectionContext.EgressBytesTransferred = c.EgressBytesTransferred;
						gatewayConnectionContext.IngressBytesTransferred = c.IngressBytesTransferred;
						gatewayConnectionContext.LastConnectionEstablished = c.LastConnectionEstablished;
						GatewayConnectionContext gatewayConnectionContext1 = gatewayConnectionContext;
						if (c.LastEvent != null)
						{
							int id = c.LastEvent.Id;
							str = id.ToString();
						}
						else
						{
							str = null;
						}
						gatewayConnectionContext1.LastEventID = str;
						GatewayConnectionContext gatewayConnectionContext2 = gatewayConnectionContext;
						if (c.LastEvent != null)
						{
							str1 = c.LastEvent.Message.ToString();
						}
						else
						{
							str1 = null;
						}
						gatewayConnectionContext2.LastEventMessage = str1;
						GatewayConnectionContext gatewayConnectionContext3 = gatewayConnectionContext;
						if (c.LastEvent != null)
						{
							DateTime timestamp = c.LastEvent.Timestamp;
							str2 = timestamp.ToString();
						}
						else
						{
							str2 = null;
						}
						gatewayConnectionContext3.LastEventTimeStamp = str2;
						gatewayConnectionContext.LocalNetworkSiteName = c.LocalNetworkSiteName;
						return gatewayConnectionContext;
					}
					);
					return gatewayConnectionContexts;
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
						gatewayConnectionContexts = null;
						return gatewayConnectionContexts;
					}
				}
				return null;
			}
			return gatewayConnectionContexts;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				IEnumerable<GatewayConnectionContext> virtualNetworkConnectionCommandProcess = this.GetVirtualNetworkConnectionCommandProcess();
				if (virtualNetworkConnectionCommandProcess != null)
				{
					base.WriteObject(virtualNetworkConnectionCommandProcess, true);
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