using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.Management.Service.Gateway
{
	[DataContract(Name="Gateway", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class VnetGateway
	{
		[DataMember]
		public GatewayEvent LastEvent
		{
			get;
			set;
		}

		[DataMember]
		public ProvisioningState State
		{
			get;
			set;
		}

		[DataMember(IsRequired=false, EmitDefaultValue=false)]
		public string VIPAddress
		{
			get;
			set;
		}

		public VnetGateway()
		{
		}
	}
}