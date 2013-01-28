using Microsoft.ActiveDirectory.Management.Commands;
using System;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADClaimTransformPolicy : ADObject
	{
		static ADClaimTransformPolicy()
		{
			ADEntity.RegisterMappingTable(typeof(ADClaimTransformPolicy), ADClaimTransformPolicyFactory<ADClaimTransformPolicy>.AttributeTable);
		}

		public ADClaimTransformPolicy()
		{
		}

		public ADClaimTransformPolicy(string identity) : base(identity)
		{
		}

		public ADClaimTransformPolicy(Guid guid) : base(new Guid?(guid))
		{
		}

		public ADClaimTransformPolicy(ADObject identity)
		{
			base.Identity = identity;
		}
	}
}