using Microsoft.ActiveDirectory.Management.Commands;
using System;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADClaimType : ADClaimTypeBase
	{
		static ADClaimType()
		{
			ADEntity.RegisterMappingTable(typeof(ADClaimType), ADClaimTypeFactory<ADClaimType>.AttributeTable);
		}

		public ADClaimType()
		{
		}

		public ADClaimType(string identity) : base(identity)
		{
		}

		public ADClaimType(Guid guid) : base(guid)
		{
		}

		public ADClaimType(ADClaimTypeBase identity)
		{
			base.Identity = identity;
		}
	}
}