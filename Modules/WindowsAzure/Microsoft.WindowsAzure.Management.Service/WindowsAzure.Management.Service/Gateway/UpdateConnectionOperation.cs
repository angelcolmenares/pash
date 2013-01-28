using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.Management.Service.Gateway
{
	[DataContract(Name="UpdateConnectionOperation", Namespace="http://schemas.microsoft.com/windowsazure")]
	public enum UpdateConnectionOperation
	{
		Connect,
		Disconnect,
		Test
	}
}