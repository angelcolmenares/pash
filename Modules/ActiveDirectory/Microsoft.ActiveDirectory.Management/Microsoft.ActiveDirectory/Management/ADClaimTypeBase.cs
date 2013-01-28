using Microsoft.ActiveDirectory.Management.Commands;
using System;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADClaimTypeBase : ADObject
	{
		static ADClaimTypeBase()
		{
			ADEntity.RegisterMappingTable(typeof(ADClaimTypeBase), ADClaimTypeBaseFactory<ADClaimTypeBase>.AttributeTable);
		}

		public ADClaimTypeBase()
		{
		}

		public ADClaimTypeBase(string identity) : base(identity)
		{
		}

		public ADClaimTypeBase(Guid guid) : base(new Guid?(guid))
		{
		}

		public ADClaimTypeBase(ADObject identity)
		{
			base.Identity = identity;
		}
	}
}