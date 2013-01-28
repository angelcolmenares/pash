using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.Management.Service.Gateway
{
	[DataContract(Name="UpdateGateway", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class UpdateGateway
	{
		[DataMember]
		public UpdateGatewayOperation Operation
		{
			get;
			set;
		}

		public UpdateGateway()
		{
		}
	}
}