using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.Core;
using Microsoft.Management.Odata.Schema;
using System;
using System.Collections.Generic;
using System.Data.Services.Providers;

namespace Microsoft.Management.Odata.PS
{
	internal class ReferenceTypeSerializer : SerializerBase
	{
		private PSEntityMetadata.ReferenceSetCmdlets.ReferencePropertyType referencePropertyType;

		public ReferenceTypeSerializer(ResourceType referringResourceType, ResourceProperty resourceProperty) : base(resourceProperty.ResourceType)
		{
			if (resourceProperty.ResourceType.ResourceTypeKind == ResourceTypeKind.EntityType)
			{
				object[] name = new object[1];
				name[0] = resourceProperty.Name;
				referringResourceType.ThrowIfNull("referringResourceType", new ParameterExtensions.MessageLoader(SerializerBase.GetReferringResourceTypeCannotNullMessage), name);
				DataContext currentContext = DataServiceController.Current.GetCurrentContext();
				if (currentContext != null)
				{
					PSEntityMetadata entityMetadata = currentContext.UserSchema.GetEntityMetadata(referringResourceType) as PSEntityMetadata;
					PSEntityMetadata.ReferenceSetCmdlets referenceSetCmdlet = null;
					if (!entityMetadata.CmdletsForReferenceSets.TryGetValue(resourceProperty.Name, out referenceSetCmdlet))
					{
						this.referencePropertyType = PSEntityMetadata.ReferenceSetCmdlets.ReferencePropertyType.Instance;
					}
					else
					{
						this.referencePropertyType = referenceSetCmdlet.PropertyType;
						return;
					}
				}
				return;
			}
			else
			{
				throw new ArgumentException("resourceType");
			}
		}

		public override object Serialize(object clrObject, int depth)
		{
			object obj;
			if (clrObject != null)
			{
				DSResource dSResource = new DSResource(base.ResourceType, true);
				if (this.referencePropertyType != PSEntityMetadata.ReferenceSetCmdlets.ReferencePropertyType.KeyOnly)
				{
					EntityTypeSerializer entityTypeSerializer = new EntityTypeSerializer(base.ResourceType, true);
					dSResource = entityTypeSerializer.Serialize(clrObject, depth + 1) as DSResource;
				}
				else
				{
					IEnumerator<ResourceProperty> enumerator = base.ResourceType.Properties.GetEnumerator();
					using (enumerator)
					{
						while (enumerator.MoveNext())
						{
							ResourceProperty current = enumerator.Current;
							if ((current.Kind & ResourcePropertyKind.Key) != ResourcePropertyKind.Key)
							{
								continue;
							}
							if (clrObject != current.GetCustomState().DefaultValue)
							{
								object obj1 = clrObject;
								dSResource.SetValue(current.Name, SerializerBase.SerializeResourceProperty(obj1, base.ResourceType, current, depth + 1));
							}
							else
							{
								obj = null;
								return obj;
							}
						}
						return dSResource;
					}
					return obj;
				}
				return dSResource;
			}
			else
			{
				return null;
			}
		}
	}
}