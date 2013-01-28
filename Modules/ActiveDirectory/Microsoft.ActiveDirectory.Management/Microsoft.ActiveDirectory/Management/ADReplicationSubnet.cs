using Microsoft.ActiveDirectory.Management.Commands;
using System;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADReplicationSubnet : ADObject
	{
		static ADReplicationSubnet()
		{
			ADEntity.RegisterMappingTable(typeof(ADReplicationSubnet), ADReplicationSubnetFactory<ADReplicationSubnet>.AttributeTable);
		}

		public ADReplicationSubnet()
		{
		}

		public ADReplicationSubnet(string identity) : base(identity)
		{
		}

		public ADReplicationSubnet(Guid guid) : base(new Guid?(guid))
		{
		}

		public ADReplicationSubnet(ADObject identity)
		{
			base.Identity = identity;
		}
	}
}