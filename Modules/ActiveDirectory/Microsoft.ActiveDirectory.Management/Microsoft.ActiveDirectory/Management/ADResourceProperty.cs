using Microsoft.ActiveDirectory.Management.Commands;
using System;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADResourceProperty : ADClaimTypeBase
	{
		static ADResourceProperty()
		{
			ADEntity.RegisterMappingTable(typeof(ADResourceProperty), ADResourcePropertyFactory<ADResourceProperty>.AttributeTable);
		}

		public ADResourceProperty()
		{
		}

		public ADResourceProperty(string identity) : base(identity)
		{
		}

		public ADResourceProperty(Guid guid) : base(guid)
		{
		}

		public ADResourceProperty(ADClaimTypeBase identity)
		{
			base.Identity = identity;
		}
	}
}