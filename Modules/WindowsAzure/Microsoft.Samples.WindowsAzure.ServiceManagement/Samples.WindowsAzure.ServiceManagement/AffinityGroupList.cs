using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="AffinityGroups", ItemName="AffinityGroup", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class AffinityGroupList : List<AffinityGroup>
	{
		public AffinityGroupList()
		{
		}

		public AffinityGroupList(IEnumerable<AffinityGroup> affinityGroups) : base(affinityGroups)
		{
		}
	}
}