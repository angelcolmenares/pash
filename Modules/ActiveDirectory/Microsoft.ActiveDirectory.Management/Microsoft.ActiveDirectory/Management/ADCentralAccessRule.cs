using Microsoft.ActiveDirectory.Management.Commands;
using System;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADCentralAccessRule : ADObject
	{
		static ADCentralAccessRule()
		{
			ADEntity.RegisterMappingTable(typeof(ADCentralAccessRule), ADCentralAccessRuleFactory<ADCentralAccessRule>.AttributeTable);
		}

		public ADCentralAccessRule()
		{
		}

		public ADCentralAccessRule(string identity) : base(identity)
		{
		}

		public ADCentralAccessRule(Guid guid) : base(new Guid?(guid))
		{
		}

		public ADCentralAccessRule(ADObject identity)
		{
			base.Identity = identity;
		}
	}
}