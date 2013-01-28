using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="Locations", ItemName="Location", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class LocationList : List<Location>
	{
		public LocationList()
		{
		}

		public LocationList(IEnumerable<Location> locations) : base(locations)
		{
		}
	}
}