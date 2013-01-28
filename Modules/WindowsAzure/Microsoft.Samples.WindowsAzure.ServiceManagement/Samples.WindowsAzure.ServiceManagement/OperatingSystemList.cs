using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="OperatingSystems", ItemName="OperatingSystem", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class OperatingSystemList : List<OperatingSystem>
	{
		public OperatingSystemList()
		{
		}

		public OperatingSystemList(IEnumerable<OperatingSystem> operatingSystems) : base(operatingSystems)
		{
		}
	}
}