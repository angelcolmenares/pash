using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class VirtualNetworkGatewayConfiguration : IExtensibleDataObject
	{
		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=1)]
		public string GatewayIPAddress
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=2)]
		public string GatewayMacAddress
		{
			get;
			set;
		}

		public VirtualNetworkGatewayConfiguration()
		{
		}
	}
}