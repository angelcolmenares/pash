using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="RoleInstanceList", ItemName="RoleInstance", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class RoleInstanceList : List<RoleInstance>
	{
		public RoleInstanceList()
		{
		}

		public RoleInstanceList(IEnumerable<RoleInstance> roles) : base(roles)
		{
		}
	}
}