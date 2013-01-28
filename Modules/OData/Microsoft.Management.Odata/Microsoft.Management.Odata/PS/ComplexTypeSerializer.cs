using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.Core;
using Microsoft.Management.Odata.Schema;
using System;
using System.Data.Services.Providers;
using System.Globalization;

namespace Microsoft.Management.Odata.PS
{
	internal class ComplexTypeSerializer : SerializerBase
	{
		internal const int MaxDepth = 10;

		public ComplexTypeSerializer(ResourceType resourceType) : base(resourceType)
		{
			object[] resourceTypeKind = new object[2];
			resourceTypeKind[0] = resourceType.ResourceTypeKind;
			resourceTypeKind[1] = ResourceTypeKind.ComplexType;
			ExceptionHelpers.ThrowArgumentExceptionIf("resourceType", resourceType.ResourceTypeKind != ResourceTypeKind.ComplexType, new ExceptionHelpers.MessageLoader(SerializerBase.GetInvalidArgMessage), resourceTypeKind);
		}

		public override object Serialize(object clrObject, int depth)
		{
			object value;
			if (depth != 10)
			{
				ResourceType resourceType = base.ResourceType;
				if (clrObject != null)
				{
					resourceType = base.ResourceType.FindResourceType(clrObject.GetType());
				}
				DSResource dSResource = new DSResource(resourceType, false);
				foreach (ResourceProperty property in resourceType.Properties)
				{
					if (clrObject != null)
					{
						value = SerializerBase.GetValue(property, clrObject);
					}
					else
					{
						value = null;
					}
					object obj = value;
					if (obj != null || !property.ResourceType.IsPrimitive() || property.ResourceType.IsNullable())
					{
						if (obj != null || (property.Kind & (ResourcePropertyKind.Primitive | ResourcePropertyKind.ResourceReference)) == 0)
						{
							if (clrObject != null || (property.Kind & ResourcePropertyKind.ComplexType) == 0)
							{
								dSResource.SetValue(property.Name, SerializerBase.SerializeResourceProperty(obj, base.ResourceType, property, depth + 1));
							}
							else
							{
								TraceHelper.Current.DebugMessage(string.Concat(property.Name, " setting null to ComplexType"));
								dSResource.SetValue(property.Name, null);
							}
						}
						else
						{
							TraceHelper.Current.DebugMessage(string.Concat(property.Name, " is null; skipping"));
						}
					}
					else
					{
						object[] name = new object[1];
						name[0] = property.Name;
						throw new PSObjectSerializationFailedException(string.Format(CultureInfo.CurrentCulture, Resources.PropertyNotFoundInPSObject, name));
					}
				}
				return dSResource;
			}
			else
			{
				TraceHelper.Current.SerializationMaximumObjectDepthReached();
				return null;
			}
		}
	}
}