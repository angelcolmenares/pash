using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class LocalNetworkSite : IExtensibleDataObject
	{
		[DataMember(EmitDefaultValue=false, Order=2)]
		public AddressSpace AddressSpace
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=1)]
		public string Name
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=3)]
		public string VpnGatewayAddress
		{
			get;
			set;
		}

		public LocalNetworkSite()
		{
		}
	}
}