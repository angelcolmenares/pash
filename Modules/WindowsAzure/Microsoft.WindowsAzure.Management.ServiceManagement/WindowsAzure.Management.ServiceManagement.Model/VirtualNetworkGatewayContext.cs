using Microsoft.WindowsAzure.Management.Model;
using Microsoft.WindowsAzure.Management.Service.Gateway;
using System;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class VirtualNetworkGatewayContext : ManagementOperationContext
	{
		public string LastEventData
		{
			get;
			set;
		}

		public int LastEventID
		{
			get;
			set;
		}

		public string LastEventMessage
		{
			get;
			set;
		}

		public DateTime? LastEventTimeStamp
		{
			get;
			set;
		}

		public ProvisioningState State
		{
			get;
			set;
		}

		public string VIPAddress
		{
			get;
			set;
		}

		public VirtualNetworkGatewayContext()
		{
		}
	}
}