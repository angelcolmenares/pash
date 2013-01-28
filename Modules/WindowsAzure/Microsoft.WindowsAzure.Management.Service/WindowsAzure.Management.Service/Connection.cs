using Microsoft.WindowsAzure.Management.Service.Gateway;
using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.Management.Service
{
	[DataContract(Name="Connection", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class Connection : IExtensibleDataObject
	{
		[DataMember(Order=1, EmitDefaultValue=false)]
		public string ConnectivityState
		{
			get;
			set;
		}

		[DataMember(Order=2)]
		public ulong EgressBytesTransferred
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=3)]
		public ulong IngressBytesTransferred
		{
			get;
			set;
		}

		[DataMember(Order=4)]
		public string LastConnectionEstablished
		{
			get;
			set;
		}

		[DataMember(Order=5)]
		public GatewayEvent LastEvent
		{
			get;
			set;
		}

		[DataMember(Order=6)]
		public string LocalNetworkSiteName
		{
			get;
			set;
		}

		public Connection()
		{
		}
	}
}