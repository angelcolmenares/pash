using System;
using System.Data.Services.Providers;

namespace Microsoft.Management.Odata.Schema
{
	internal class ResourcePropertyWithDescription : ResourceProperty
	{
		public ResourcePropertyWithDescription(string name, ResourcePropertyKind kind, ResourceType myType) : base(name, kind, myType)
		{
		}

		public override string ToString()
		{
			return string.Concat("property:", base.Name);
		}
	}
}