using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="Disks", ItemName="Disk", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class DiskList : Collection<Disk>
	{
		public DiskList()
		{
		}
	}
}