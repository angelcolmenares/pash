using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="LocalNetworkSites", ItemName="LocalNetworkSite", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class LocalNetworkSiteList : List<LocalNetworkSite>
	{
		public LocalNetworkSiteList()
		{
		}
	}
}