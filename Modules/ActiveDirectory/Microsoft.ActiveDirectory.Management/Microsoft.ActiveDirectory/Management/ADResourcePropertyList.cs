using Microsoft.ActiveDirectory.Management.Commands;
using System;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADResourcePropertyList : ADObject
	{
		static ADResourcePropertyList()
		{
			ADEntity.RegisterMappingTable(typeof(ADResourcePropertyList), ADResourcePropertyListFactory<ADResourcePropertyList>.AttributeTable);
		}

		public ADResourcePropertyList()
		{
		}

		public ADResourcePropertyList(string identity) : base(identity)
		{
		}

		public ADResourcePropertyList(Guid guid) : base(new Guid?(guid))
		{
		}

		public ADResourcePropertyList(ADObject identity)
		{
			base.Identity = identity;
		}
	}
}