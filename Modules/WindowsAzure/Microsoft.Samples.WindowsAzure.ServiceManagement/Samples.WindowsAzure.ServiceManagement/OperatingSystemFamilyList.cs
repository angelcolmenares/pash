using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="OperatingSystemFamilies", ItemName="OperatingSystemFamily", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class OperatingSystemFamilyList : List<OperatingSystemFamily>
	{
		public OperatingSystemFamilyList()
		{
		}

		public OperatingSystemFamilyList(IEnumerable<OperatingSystemFamily> operatingSystemFamilies) : base(operatingSystemFamilies)
		{
		}
	}
}