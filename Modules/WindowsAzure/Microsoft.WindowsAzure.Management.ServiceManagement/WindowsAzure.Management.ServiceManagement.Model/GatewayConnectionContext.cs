using Microsoft.WindowsAzure.Management.Model;
using System;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class GatewayConnectionContext : ManagementOperationContext
	{
		public string ConnectivityState
		{
			get;
			set;
		}

		public ulong EgressBytesTransferred
		{
			get;
			set;
		}

		public ulong IngressBytesTransferred
		{
			get;
			set;
		}

		public string LastConnectionEstablished
		{
			get;
			set;
		}

		public string LastEventID
		{
			get;
			set;
		}

		public string LastEventMessage
		{
			get;
			set;
		}

		public string LastEventTimeStamp
		{
			get;
			set;
		}

		public string LocalNetworkSiteName
		{
			get;
			set;
		}

		public GatewayConnectionContext()
		{
		}
	}
}