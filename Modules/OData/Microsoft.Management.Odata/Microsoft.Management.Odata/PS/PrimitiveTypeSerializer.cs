using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.Schema;
using System;
using System.Data.Services;
using System.Data.Services.Providers;

namespace Microsoft.Management.Odata.PS
{
	internal class PrimitiveTypeSerializer : SerializerBase
	{
		private object defaultValue;

		private string name;

		public PrimitiveTypeSerializer(ResourceType resourceType) : this(resourceType, null)
		{
		}

		public PrimitiveTypeSerializer(ResourceType resourceType, ResourceProperty resourceProperty) : base(resourceType)
		{
			object defaultValue;
			object[] resourceTypeKind = new object[2];
			resourceTypeKind[0] = resourceType.ResourceTypeKind;
			resourceTypeKind[1] = ResourceTypeKind.Primitive;
			ExceptionHelpers.ThrowArgumentExceptionIf("resourceType", resourceType.ResourceTypeKind != ResourceTypeKind.Primitive, new ExceptionHelpers.MessageLoader(SerializerBase.GetInvalidArgMessage), resourceTypeKind);
			this.defaultValue = null;
			if (resourceProperty != null)
			{
				if ((resourceProperty.Kind & ResourcePropertyKind.Primitive) != ResourcePropertyKind.Primitive || resourceProperty.ResourceType.InstanceType != resourceType.InstanceType)
				{
					throw new ArgumentException("resourceProperty");
				}
				else
				{
					PropertyCustomState customState = resourceProperty.GetCustomState();
					PrimitiveTypeSerializer primitiveTypeSerializer = this;
					if (customState != null)
					{
						defaultValue = customState.DefaultValue;
					}
					else
					{
						defaultValue = null;
					}
					primitiveTypeSerializer.defaultValue = defaultValue;
					this.name = resourceProperty.Name;
				}
			}
		}

		public override object Serialize(object clrObject, int depth)
		{
			if (clrObject == null)
			{
				clrObject = this.defaultValue;
			}
			else
			{
				if (clrObject.GetType() != base.ResourceType.InstanceType)
				{
					try
					{
						clrObject = TypeConverter.ConvertTo(clrObject, base.ResourceType.InstanceType);
					}
					catch (InvalidCastException invalidCastException1)
					{
						InvalidCastException invalidCastException = invalidCastException1;
						object[] message = new object[2];
						message[0] = this.name;
						message[1] = invalidCastException.Message;
						throw new DataServiceException(ExceptionHelpers.GetExceptionMessage(invalidCastException, Resources.PropertyNotSerialized, message), invalidCastException);
					}
				}
			}
			return clrObject;
		}
	}
}