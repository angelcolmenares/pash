using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Services.Providers;
using System.Management.Automation;

namespace Microsoft.Management.Odata.PS
{
	internal class ReferenceSetSerializer : SerializerBase
	{
		private ResourceProperty resourceProperty;

		private ResourceType referringResourceType;

		public ReferenceSetSerializer(ResourceType referringResourceType, ResourceProperty resourceProperty) : base(resourceProperty.ResourceType)
		{
			if (resourceProperty.ResourceType.ResourceTypeKind == ResourceTypeKind.EntityType)
			{
				object[] name = new object[1];
				name[0] = resourceProperty.Name;
				referringResourceType.ThrowIfNull("referringResourceType", new ParameterExtensions.MessageLoader(SerializerBase.GetReferringResourceTypeCannotNullMessage), name);
				this.resourceProperty = resourceProperty;
				this.referringResourceType = referringResourceType;
				return;
			}
			else
			{
				throw new ArgumentException("resourceType");
			}
		}

		public override object Serialize(object collection, int depth)
		{
			ReferenceTypeSerializer referenceTypeSerializer = new ReferenceTypeSerializer(this.referringResourceType, this.resourceProperty);
			List<DSResource> dSResources = new List<DSResource>();
			if (collection != null)
			{
				IEnumerable enumerable = LanguagePrimitives.GetEnumerable(collection);
				if (enumerable != null)
				{
					foreach (object obj in enumerable)
					{
						dSResources.Add(referenceTypeSerializer.Serialize(obj, depth) as DSResource);
					}
				}
				else
				{
					throw new ArgumentException("reference set is not a collection type", "collection");
				}
			}
			return dSResources;
		}
	}
}