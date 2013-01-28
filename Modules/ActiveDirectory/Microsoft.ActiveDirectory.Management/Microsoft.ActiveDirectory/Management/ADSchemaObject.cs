using Microsoft.ActiveDirectory.Management.Commands;
using System;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADSchemaObject : ADObject
	{
		static ADSchemaObject()
		{
			ADEntity.RegisterMappingTable(typeof(ADSchemaObject), ADSchemaObjectFactory<ADSchemaObject>.AttributeTable);
		}

		public ADSchemaObject()
		{
		}

		public ADSchemaObject(string identity) : base(identity)
		{
		}

		public ADSchemaObject(Guid guid) : base(new Guid?(guid))
		{
		}

		public ADSchemaObject(ADObject identity)
		{
			base.Identity = identity;
		}
	}
}