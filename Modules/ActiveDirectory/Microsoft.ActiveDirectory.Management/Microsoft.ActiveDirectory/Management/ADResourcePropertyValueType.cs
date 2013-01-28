using Microsoft.ActiveDirectory.Management.Commands;
using System;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADResourcePropertyValueType : ADObject
	{
		static ADResourcePropertyValueType()
		{
			ADEntity.RegisterMappingTable(typeof(ADResourcePropertyValueType), ADResourcePropertyValueTypeFactory<ADResourcePropertyValueType>.AttributeTable);
		}

		public ADResourcePropertyValueType()
		{
		}

		public ADResourcePropertyValueType(string identity) : base(identity)
		{
		}

		public ADResourcePropertyValueType(Guid guid) : base(new Guid?(guid))
		{
		}

		public ADResourcePropertyValueType(ADObject identity)
		{
			base.Identity = identity;
		}
	}
}