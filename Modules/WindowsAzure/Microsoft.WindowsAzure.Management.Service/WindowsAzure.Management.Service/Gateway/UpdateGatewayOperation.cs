using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.Management.Service.Gateway
{
	[DataContract(Name="UpdateGatewayOperation", Namespace="http://schemas.microsoft.com/windowsazure")]
	public enum UpdateGatewayOperation
	{
		Failover
	}
}