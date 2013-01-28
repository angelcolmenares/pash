using Microsoft.ActiveDirectory.Management.Commands;
using System;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADReplicationConnection : ADObject
	{
		static ADReplicationConnection()
		{
			ADEntity.RegisterMappingTable(typeof(ADReplicationConnection), ADReplicationConnectionFactory<ADReplicationConnection>.AttributeTable);
			ADEntity.RegisterMappingTable(typeof(ADReplicationConnection), ADNtdsSettingFactory<ADNtdsSetting>.AttributeTable);
		}

		public ADReplicationConnection()
		{
		}

		public ADReplicationConnection(string identity) : base(identity)
		{
		}

		public ADReplicationConnection(Guid guid) : base(new Guid?(guid))
		{
		}

		public ADReplicationConnection(ADObject identity)
		{
			base.Identity = identity;
		}
	}
}