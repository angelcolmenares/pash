using Microsoft.ActiveDirectory.Management.Commands;
using System;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADReplicationSite : ADObject
	{
		static ADReplicationSite()
		{
			ADEntity.RegisterMappingTable(typeof(ADReplicationSite), ADReplicationSiteFactory<ADReplicationSite>.AttributeTable);
			ADEntity.RegisterMappingTable(typeof(ADReplicationSite), ADNtdsSiteSettingFactory<ADNtdsSiteSetting>.AttributeTable);
		}

		public ADReplicationSite()
		{
		}

		public ADReplicationSite(string identity) : base(identity)
		{
		}

		public ADReplicationSite(Guid guid) : base(new Guid?(guid))
		{
		}

		public ADReplicationSite(ADObject identity)
		{
			base.Identity = identity;
		}

		public ADReplicationSite(ADDirectoryServer directoryServer)
		{
			if (directoryServer != null)
			{
				base.Identity = directoryServer.Site;
				return;
			}
			else
			{
				throw new ArgumentNullException("directoryServer");
			}
		}
	}
}