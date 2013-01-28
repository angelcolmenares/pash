using Microsoft.ActiveDirectory.Management.Commands;
using System;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADCentralAccessPolicy : ADObject
	{
		static ADCentralAccessPolicy()
		{
			ADEntity.RegisterMappingTable(typeof(ADCentralAccessPolicy), ADCentralAccessPolicyFactory<ADCentralAccessPolicy>.AttributeTable);
		}

		public ADCentralAccessPolicy()
		{
		}

		public ADCentralAccessPolicy(string identity) : base(identity)
		{
		}

		public ADCentralAccessPolicy(Guid guid) : base(new Guid?(guid))
		{
		}

		public ADCentralAccessPolicy(ADObject identity)
		{
			base.Identity = identity;
		}
	}
}