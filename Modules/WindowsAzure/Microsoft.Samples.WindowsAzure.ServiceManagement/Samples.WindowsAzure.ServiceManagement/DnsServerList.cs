using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="DnsServers", ItemName="DnsServer", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class DnsServerList : List<DnsServer>
	{
		public DnsServerList()
		{
		}
	}
}