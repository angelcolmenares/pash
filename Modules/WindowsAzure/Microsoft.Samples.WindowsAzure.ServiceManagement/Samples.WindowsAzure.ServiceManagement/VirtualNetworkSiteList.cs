using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="VirtualNetworkSites", ItemName="VirtualNetworkSite", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class VirtualNetworkSiteList : List<VirtualNetworkSite>
	{
		public VirtualNetworkSiteList()
		{
		}

		public VirtualNetworkSiteList(IEnumerable<VirtualNetworkSite> virtualNetworkSites) : base(virtualNetworkSites)
		{
		}
	}
}