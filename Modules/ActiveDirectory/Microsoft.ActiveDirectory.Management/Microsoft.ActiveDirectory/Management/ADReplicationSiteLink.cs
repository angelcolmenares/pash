using Microsoft.ActiveDirectory.Management.Commands;
using System;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADReplicationSiteLink : ADObject
	{
		static ADReplicationSiteLink()
		{
			ADEntity.RegisterMappingTable(typeof(ADReplicationSiteLink), ADReplicationSiteLinkFactory<ADReplicationSiteLink>.AttributeTable);
		}

		public ADReplicationSiteLink()
		{
		}

		public ADReplicationSiteLink(string identity) : base(identity)
		{
		}

		public ADReplicationSiteLink(Guid guid) : base(new Guid?(guid))
		{
		}

		public ADReplicationSiteLink(ADObject identity)
		{
			base.Identity = identity;
		}
	}
}