using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class InstanceEndpointList : List<InstanceEndpoint>
	{
		public InstanceEndpointList()
		{
		}
	}
}