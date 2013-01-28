using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.Core;
using Microsoft.Management.Odata.Schema;
using System;
using System.Data.Services.Providers;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.Management.Odata.PS
{
	internal abstract class SerializerBase
	{
		protected ResourceType ResourceType
		{
			get;
			private set;
		}

		public SerializerBase(ResourceType resourceType)
		{
			this.ResourceType = resourceType;
		}

		public static SerializerBase CreateSerializer(ResourceType referringResourceType, ResourceProperty resourceProperty)
		{
			if ((resourceProperty.Kind & ResourcePropertyKind.Primitive) != ResourcePropertyKind.Primitive)
			{
				if ((resourceProperty.Kind & ResourcePropertyKind.ComplexType) != ResourcePropertyKind.ComplexType)
				{
					if ((resourceProperty.Kind & ResourcePropertyKind.Collection) != ResourcePropertyKind.Collection)
					{
						if ((resourceProperty.Kind & ResourcePropertyKind.ResourceReference) != ResourcePropertyKind.ResourceReference)
						{
							if ((resourceProperty.Kind & ResourcePropertyKind.ResourceSetReference) != ResourcePropertyKind.ResourceSetReference)
							{
								object[] str = new object[1];
								str[0] = resourceProperty.Kind.ToString();
								throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.InvalidResourceProperty, str), "resourceProperty");
							}
							else
							{
								return new ReferenceSetSerializer(referringResourceType, resourceProperty);
							}
						}
						else
						{
							return new ReferenceTypeSerializer(referringResourceType, resourceProperty);
						}
					}
					else
					{
						return new CollectionResourceTypeSerializer(resourceProperty.ResourceType);
					}
				}
				else
				{
					return new ComplexTypeSerializer(resourceProperty.ResourceType);
				}
			}
			else
			{
				return new PrimitiveTypeSerializer(resourceProperty.ResourceType, resourceProperty);
			}
		}

		public static SerializerBase CreateSerializer(ResourceType resourceType)
		{
			ResourceTypeKind resourceTypeKind = resourceType.ResourceTypeKind;
			switch (resourceTypeKind)
			{
				case ResourceTypeKind.ComplexType:
				{
					return new ComplexTypeSerializer(resourceType);
				}
				case ResourceTypeKind.Primitive:
				{
					return new PrimitiveTypeSerializer(resourceType);
				}
				case ResourceTypeKind.Collection:
				{
					return new CollectionResourceTypeSerializer(resourceType);
				}
			}
			return new EntityTypeSerializer(resourceType, false);
		}

		internal static string GetInvalidArgMessage()
		{
			return Resources.InvalidArgMessage;
		}

		internal static string GetNullPassedForSerializingEntityMessage()
		{
			return Resources.NullPassedForSerializingEntity;
		}

		internal static string GetReferringResourceTypeCannotNullMessage()
		{
			return Resources.ReferringResourceTypeCannotNull;
		}

		internal static object GetValue(ResourceProperty property, object clrObject)
		{
			string name;
			int num;
			int num1 = -1;
			string[] strArrays = null;
			if (property.GetCustomState() != null && property.GetCustomState().PsProperty != null)
			{
				char[] chrArray = new char[1];
				chrArray[0] = '.';
				strArrays = property.GetCustomState().PsProperty.Split(chrArray);
				num1 = 0;
			}
			do
			{
				if (num1 > -1)
				{
					name = strArrays[num1];
				}
				else
				{
					name = property.Name;
				}
				string str = name;
				if (clrObject as PSObject == null)
				{
					clrObject = TypeSystem.GetPropertyValue(clrObject, str, false);
				}
				else
				{
					PSObject pSObject = clrObject as PSObject;
					PSPropertyInfo item = pSObject.Properties[str];
					if (item != null)
					{
						try
						{
							clrObject = item.Value;
						}
						catch (GetValueException getValueException1)
						{
							GetValueException getValueException = getValueException1;
							TraceHelper.Current.SerializationPropertyNotFound(str, getValueException.Message);
							if (!property.ResourceType.IsPrimitive() || property.ResourceType.IsNullable())
							{
								clrObject = null;
							}
							else
							{
								throw new PSObjectSerializationFailedException(string.Format(Resources.PropertyRetrievalFailed, str, getValueException.Message));
							}
						}
					}
					else
					{
						object[] objArray = new object[1];
						objArray[0] = str;
						TraceHelper.Current.SerializationPropertyNotFound(str, string.Format(CultureInfo.CurrentCulture, Resources.PropertyNotFoundInPSObject, objArray));
						if (!property.ResourceType.IsPrimitive() || property.ResourceType.IsNullable())
						{
							clrObject = null;
							break;
						}
						else
						{
							object[] objArray1 = new object[1];
							objArray1[0] = str;
							throw new PSObjectSerializationFailedException(string.Format(CultureInfo.CurrentCulture, Resources.PropertyNotFoundInPSObject, objArray1));
						}
					}
				}
				if (clrObject == null || strArrays == null)
				{
					break;
				}
				num = num1 + 1;
				num1 = num;
			}
			while (num < (int)strArrays.Length);
			return clrObject;
		}

		public abstract object Serialize(object clrObject, int depth);

		public static DSResource SerializeEntity(object instance, ResourceType resourceType)
		{
			object[] resourceTypeKind = new object[2];
			resourceTypeKind[0] = resourceType.ResourceTypeKind;
			resourceTypeKind[1] = ResourceTypeKind.EntityType;
			ExceptionHelpers.ThrowArgumentExceptionIf("resourceType", resourceType.ResourceTypeKind != ResourceTypeKind.EntityType, new ExceptionHelpers.MessageLoader(SerializerBase.GetInvalidArgMessage), resourceTypeKind);
			DSResource dSResource = SerializerBase.SerializeResourceType(instance, resourceType, 1) as DSResource;
			if (dSResource != null)
			{
				return dSResource;
			}
			else
			{
				throw new InvalidOperationException(Resources.SerializeEntityReturnedNull);
			}
		}

		internal static object SerializeResourceProperty(object clrObject, ResourceType resourceType, ResourceProperty resourceProperty, int depth)
		{
			return SerializerBase.CreateSerializer(resourceType, resourceProperty).Serialize(clrObject, depth);
		}

		internal static object SerializeResourceType(object clrObject, ResourceType resourceType, int depth)
		{
			return SerializerBase.CreateSerializer(resourceType).Serialize(clrObject, depth);
		}
	}
}