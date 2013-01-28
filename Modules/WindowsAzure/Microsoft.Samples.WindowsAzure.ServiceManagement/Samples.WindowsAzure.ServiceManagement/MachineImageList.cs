using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="MachineImages", ItemName="MachineImage", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class MachineImageList : List<MachineImage>
	{
		public MachineImageList()
		{
		}
	}
}