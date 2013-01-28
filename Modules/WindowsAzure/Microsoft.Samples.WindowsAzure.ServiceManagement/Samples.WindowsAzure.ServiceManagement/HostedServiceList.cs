using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="HostedServices", ItemName="HostedService", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class HostedServiceList : List<HostedService>
	{
		public HostedServiceList()
		{
		}

		public HostedServiceList(IEnumerable<HostedService> hostedServices) : base(hostedServices)
		{
		}
	}
}