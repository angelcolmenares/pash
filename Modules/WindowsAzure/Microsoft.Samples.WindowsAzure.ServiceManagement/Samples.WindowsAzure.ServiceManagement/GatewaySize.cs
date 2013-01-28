using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public enum GatewaySize
	{
		Small,
		Medium,
		Large,
		ExtraLarge
	}
}