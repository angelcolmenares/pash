using Microsoft.ActiveDirectory.Management.Commands;
using System;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADReplicationSiteLinkBridge : ADObject
	{
		static ADReplicationSiteLinkBridge()
		{
			ADEntity.RegisterMappingTable(typeof(ADReplicationSiteLinkBridge), ADReplicationSiteLinkBridgeFactory<ADReplicationSiteLinkBridge>.AttributeTable);
		}

		public ADReplicationSiteLinkBridge()
		{
		}

		public ADReplicationSiteLinkBridge(string identity) : base(identity)
		{
		}

		public ADReplicationSiteLinkBridge(Guid guid) : base(new Guid?(guid))
		{
		}

		public ADReplicationSiteLinkBridge(ADObject identity)
		{
			base.Identity = identity;
		}
	}
}