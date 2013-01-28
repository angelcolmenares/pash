using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="Images", ItemName="OSImage", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class OSImageList : Collection<OSImage>
	{
		public OSImageList()
		{
		}
	}
}