using Microsoft.ActiveDirectory.Management.Commands;
using System;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADTrust : ADObject
	{
		static ADTrust()
		{
			ADEntity.RegisterMappingTable(typeof(ADTrust), ADTrustFactory<ADTrust>.AttributeTable);
		}

		public ADTrust()
		{
		}

		public ADTrust(string identity) : base(identity)
		{
		}

		public ADTrust(Guid guid) : base(new Guid?(guid))
		{
		}

		public ADTrust(ADObject identity)
		{
			base.Identity = identity;
		}
	}
}