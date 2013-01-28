using System;
using System.Data.Services.Providers;

namespace Microsoft.Management.Odata.Schema
{
	internal class ResourceTypeWithDescription : ResourceType
	{
		public ResourceTypeWithDescription(Type instanceType, ResourceTypeKind resourceTypeKind, ResourceType baseType, string namespaceName, string name, bool isAbstract) : base(instanceType, resourceTypeKind, baseType, namespaceName, name, isAbstract)
		{
		}

		public override string ToString()
		{
			return string.Concat("ResourceType:", base.Name);
		}
	}
}