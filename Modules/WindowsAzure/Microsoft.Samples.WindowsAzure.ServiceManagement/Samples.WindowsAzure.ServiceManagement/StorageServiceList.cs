using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="StorageServices", ItemName="StorageService", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class StorageServiceList : List<StorageService>
	{
		public StorageServiceList()
		{
		}

		public StorageServiceList(IEnumerable<StorageService> storageServices) : base(storageServices)
		{
		}
	}
}