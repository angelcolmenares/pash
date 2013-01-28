using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public abstract class ProvisioningConfigurationSet : ConfigurationSet
	{
		protected ProvisioningConfigurationSet()
		{
		}
	}
}